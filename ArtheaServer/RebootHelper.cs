using ArtheaEngine;
using ArtheaEngine.Model;
using Microsoft.Win32.TaskScheduler;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml.Serialization;
using Thought.Net.Telnet;

namespace ArtheaServer
{
    public interface IRebootService
    {
        string GetServerConfig(int processId);

        void Finish();
    }

    public class RebootService : MarshalByRefObject, IRebootService
    {
        public static readonly int Port = 8080;
        public static readonly string Uri = "Reboot";

        public string GetServerConfig(int processId)
        {

            var config = new ServerConfig(processId, GameServer.Servers);

            XmlSerializer s = new XmlSerializer(config.GetType());

            using (var sw = new StringWriter())
            {

                s.Serialize(sw, config);

                return sw.ToString();
            }
        }

        public void Finish()
        {
            GameServer.Exit();
        }
    }

	public static class RebootHelper
	{
        
        class RemotingClient : IDisposable
        {
            IChannel channel = new TcpChannel();

            IRebootService service;

            public RemotingClient()
            {
                ChannelServices.RegisterChannel(channel, false);

                service = (IRebootService)Activator.GetObject(
                      typeof(IRebootService), "tcp://localhost:" + RebootService.Port + "/" + RebootService.Uri);

            }

            public ServerConfig GetServerConfig()
            {

                XmlSerializer s = new XmlSerializer(typeof(ServerConfig));

                using (var sr = new StringReader(service.GetServerConfig(Process.GetCurrentProcess().Id)))
                {
                    return (ServerConfig)s.Deserialize(sr);
                }
            }

            public void Dispose()
            {
                if (service != null)
                {
                    service.Finish();
                }
                ChannelServices.UnregisterChannel(channel);
            }
        }

        class RemotingServer : IDisposable
        {
            IChannel channel;

            public RemotingServer()
            {
                channel = new TcpChannel(RebootService.Port);

                ChannelServices.RegisterChannel(channel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(
                   typeof(RebootService), RebootService.Uri,
                   WellKnownObjectMode.Singleton);
            }

            public void Dispose()
            {
                ChannelServices.UnregisterChannel(channel);
            }
        }

        private static RemotingServer remotingServer;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Reboot ()
		{
            if(remotingServer == null)
                remotingServer = new RemotingServer();

            foreach (var s in GameServer.Servers)
            {
                Log.Info("Stopping " + s.World.Name + "...");

                s.Stop();
            }

			var assm = Assembly.GetEntryAssembly ();

			switch (Environment.OSVersion.Platform) {
			case PlatformID.MacOSX:
			case PlatformID.Unix:
				StartPosixTask (assm);
				break;
			default:
				StartWin32Task (assm);
				break;
			}

        }

		// starts a secondary process
		private static void StartPosixTask (Assembly assm)
		{
			Process.Start(assm.Location, "/reboot");
		}

		// adds a win32 task to restart the executable 
		private static void StartWin32Task(Assembly assm)
		{
			var taskName = new AssemblyName (assm.FullName).Name;
			var dir = Path.GetDirectoryName (assm.Location);

			Log.Info ("Creating task");
			
			using (var ts = new TaskService())
            {
                Log.Info("current dir " + dir);

                var task = ts.AddTask(taskName,
                    new TimeTrigger(DateTime.Now.AddSeconds(5)),
                    new ExecAction(assm.Location, "/reboot", dir));
                ts.RootFolder.RegisterTaskDefinition(taskName, task.Definition);
            }
		}

        public static List<IServer> Recover()
        {
            var servers = new List<IServer>();
            ServerConfig config = null;

            try
            {
                using (var client = new RemotingClient())
                {
                    config = client.GetServerConfig();

                    if (config.Worlds == null)
                        return servers;

                    Debug.WriteLine("Got server configuration....");

                    foreach (var worldConfig in config.Worlds)
                    {
                        var world = new World();
                        world.Name = worldConfig.Name;
                        world.Port = worldConfig.Port;
                        world.Id = worldConfig.Id;

                        Debug.WriteLine("Recovering world " + world.Name + "...");

                        var server = new TelnetServer(world);

                        servers.Add(server);

                        server.Start();

                        foreach (var conConfig in worldConfig.Connections)
                        {
                            server.StartClient(conConfig);

                        }

                    }
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return servers;
        }
	}
	
	public abstract class ProcessControlException : Exception
	{
		public ProcessControlException()
            : base()
        {
        }

        public ProcessControlException(string msg)
            : base(msg)
        {
        }

        public ProcessControlException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        public ProcessControlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

		public abstract void Execute();
	}
	
    public class RebootException : ProcessControlException
    {
        #region Constructors

        public RebootException()
            : base()
        {
        }

        public RebootException(string msg)
            : base(msg)
        {
        }

        public RebootException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        public RebootException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

		public override void Execute ()
		{
			RebootHelper.Reboot();
		}
    }

    public class ShutdownException : ProcessControlException
    {
        #region Constructors

        public ShutdownException()
            : base()
        {
        }

        public ShutdownException(string msg)
            : base(msg)
        {
        }

        public ShutdownException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        public ShutdownException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

		public override void Execute ()
		{

            GameServer.Exit();
		}
    }
}


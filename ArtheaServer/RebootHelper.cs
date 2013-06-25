using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ArtheaEngine.Model;
using NLog;
using System.Threading;

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

        #region IRebootService Members

        public string GetServerConfig(int processId)
        {
            var config = new ServerConfig(processId, GameServer.Servers);

            var s = new XmlSerializer(config.GetType());

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

        #endregion
    }

    public static class RebootHelper
    {
        private static RemotingServer _remotingServer;

        //private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Reboot()
        {
            if (_remotingServer == null)
                _remotingServer = new RemotingServer();

			GameServer.StopAll();
            
            var assm = Assembly.GetEntryAssembly();

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    StartPosixTask(assm);
                    break;
                default:
                    StartWin32Task(assm);
                    break;
            }

        }

        // starts a secondary process
        private static void StartPosixTask(Assembly assm)
        {
            Process.Start(assm.Location, "/reboot " + GameServer.Arguments);
        }

        // adds a win32 task to restart the executable 
        private static void StartWin32Task(Assembly assm)
        {
            /*var taskName = new AssemblyName(assm.FullName).Name;
            var dir = Path.GetDirectoryName(assm.Location);

            Log.Info("Creating restart task...");

            using (var ts = new TaskService())
            {
                var folder = ts.RootFolder.CreateFolder(taskName);

                taskName = RebootService.Uri;

                var task = ts.AddTask(taskName,
                                       new TimeTrigger(DateTime.Now.AddSeconds(10)),
                                       new ExecAction(assm.Location, "/reboot", dir));

                folder.RegisterTaskDefinition(taskName, task.Definition);
            }*/
            StartPosixTask(assm);
        }

        public static List<IServer> Recover()
        {
            var servers = new List<IServer>();

            try
            {
                using (var client = new RemotingClient())
                {
                    var config = client.GetServerConfig();

                    if (config.Worlds == null)
                        return servers;

                    Debug.WriteLine("Got server configuration....");

                    foreach (WorldConfig worldConfig in config.Worlds)
                    {
                        var world = new World {Name = worldConfig.Name, Port = worldConfig.Port, Id = worldConfig.Id};

                        Debug.WriteLine("Recovering world " + world.Name + "...");

                        var server = new TelnetServer(world);

                        servers.Add(server);

                        server.Start();

                        foreach (ConnectionConfig conConfig in worldConfig.Connections)
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

        #region Nested type: RemotingClient

        private class RemotingClient : IDisposable
        {
            private readonly IChannel _channel = new TcpChannel();

            private readonly IRebootService _service;

            public RemotingClient()
            {
                ChannelServices.RegisterChannel(_channel, false);

                _service = (IRebootService) Activator.GetObject(
                    typeof (IRebootService), "tcp://localhost:" + RebootService.Port + "/" + RebootService.Uri);
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (_service != null)
                {
                    _service.Finish();
                }
                ChannelServices.UnregisterChannel(_channel);
            }

            #endregion

            public ServerConfig GetServerConfig()
            {
                var s = new XmlSerializer(typeof (ServerConfig));

                using (var sr = new StringReader(_service.GetServerConfig(Process.GetCurrentProcess().Id)))
                {
                    return (ServerConfig) s.Deserialize(sr);
                }
            }
        }

        #endregion

        #region Nested type: RemotingServer

        private class RemotingServer : IDisposable
        {
            private readonly IChannel _channel;

            public RemotingServer()
            {
                _channel = new TcpChannel(RebootService.Port);

                ChannelServices.RegisterChannel(_channel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof (RebootService), RebootService.Uri,
                    WellKnownObjectMode.Singleton);
            }

            #region IDisposable Members

            public void Dispose()
            {
                ChannelServices.UnregisterChannel(_channel);
            }

            #endregion
        }

        #endregion
    }

    public abstract class ProcessControlException : Exception
    {
        protected ProcessControlException()
        {
        }

        protected ProcessControlException(string msg)
            : base(msg)
        {
        }

        protected ProcessControlException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        protected ProcessControlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public abstract void Execute();
    }

    public class RebootException : ProcessControlException
    {
        #region Constructors

        public RebootException()
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

        public override void Execute()
        {
            RebootHelper.Reboot();
        }
    }

    public class ShutdownException : ProcessControlException
    {
        #region Constructors

        public ShutdownException()
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

        public override void Execute()
        {
            GameServer.Exit();
        }
    }
}
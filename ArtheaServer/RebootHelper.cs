using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Reflection;
using Microsoft.Win32.TaskScheduler;
using ArtheaEngine;
using System.Runtime.Serialization;
using ArtheaEngine.Model;
using Thought.Net.Telnet;
using System.Net.Sockets;

namespace ArtheaServer
{
	public static class RebootHelper
	{
		
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Reboot (TelnetServer server)
		{
			var config = new WorldConfig(server);

			Log.Info ("Writing {0}.xml", config.Port);

			var sw = new StreamWriter (string.Format("{0}.xml", config.Port));
			var ser = new XmlSerializer (config.GetType ());

			ser.Serialize (sw, config);

			sw.Close ();

			Log.Info ("Stopping server {0}:{1}...", config.Name, config.Name);

			server.Stop();

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
            server.Stop();
        }

		// starts a secondary process
		private static void StartPosixTask (Assembly assm)
		{
			Process.Start(assm.Location);
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
                    new TimeTrigger(DateTime.Now.AddSeconds(10)),
                    new ExecAction(assm.Location, null, dir));
                ts.RootFolder.RegisterTaskDefinition(taskName, task.Definition);
            }
		}

		public static TelnetServer Recover (World world)
		{
			string fname = string.Format ("{0}.xml", world.Port);

			if (!File.Exists (fname))
				return null;

			var sr = new StreamReader (fname);

			var ser = new XmlSerializer (typeof(WorldConfig));

			var config = (WorldConfig)ser.Deserialize (sr);

			sr.Close();

			File.Delete(fname);

			var server = new TelnetServer (world);

			foreach (var c in config.Connections) {
				var con = new TelnetConnection (new TelnetClient (new Socket(c.SockInfo)), server);

				server.Connections.Add(con);
			}

			return server;
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

		public abstract void Execute(TelnetServer server);
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

		public override void Execute (TelnetServer server)
		{
			RebootHelper.Reboot(server);
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

		public override void Execute (TelnetServer server)
		{
			server.Stop();
		}
    }
}


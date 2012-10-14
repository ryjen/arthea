using System;
using ArtheaEngine;
using ArtheaEngine.Model;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Threading;

namespace ArtheaServer
{
	internal class ConsoleGame
	{
		HashSet<IServer> servers = new HashSet<IServer>();
		
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public ConsoleGame ()
		{
		}

		public void Run (Arguments args)
		{
			foreach (var entry in DbManager.Query<World>()) {
				var server = RebootHelper.Recover (entry);

				if (server != null) {
					servers.Add (server);
					continue;
				}

				
				server = new TelnetServer (entry);
				
				servers.Add (server);
			}

			if (args.Has ("world")) {
				var server = servers.FirstOrDefault (w => w.World.Name.Equals (args ["world"]) || w.World.Id.Equals (args ["world"]));

				if (server != null) {
					Log.Info("Starting {0}...", server.World.Name);
					server.Start ();
				} else {
					Log.Error("Could not find world '{0}'.", args["world"]);
				}
			} else {
				foreach (var server in servers) {
					Log.Info("Starting {0}...", server.World.Name);
					server.Start ();
				}
			}

			MainLoop();
		}

		private void MainLoop ()
		{
			bool alive = true;

			while (alive) {

				Thread.Sleep(1000);

				servers.RemoveWhere(s => s.State == ServerState.Stopped);

				alive = servers.Count != 0;
			}
		}
	}
}


using System.Threading;
using ArtheaEngine;
using ArtheaEngine.Model;
using NLog;

namespace ArtheaServer
{
    internal class ConsoleGame
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Run(Arguments args)
        {
            GameServer.Servers = RebootHelper.Recover();

            foreach (World entry in DbManager.Query<World>())
            {
                if (GameServer.IsWorldActive(entry))
                {
                    continue;
                }

                if (args.Has("world"))
                {
                    if (entry.Name.Equals(args["world"]) || entry.Id.Equals(args["world"]))
                    {
                        GameServer.Servers.Add(new TelnetServer(entry));
                        break;
                    }
                    continue;
                }

                GameServer.Servers.Add(new TelnetServer(entry));
            }

            if (args.Has("world") && GameServer.Servers.Count == 0)
            {
                Log.Error("Could not find world '{0}'.", args["world"]);
                return;
            }

            foreach (IServer server in GameServer.Servers)
            {
                Log.Info("Starting {0}...", server.World.Name);
                server.Start();
            }


            MainLoop();
        }

        private void MainLoop()
        {
            bool alive = true;

            while (alive)
            {
                Thread.Sleep(1000);

                int index = 0;

                while (index < GameServer.Servers.Count)
                {
                    IServer server = GameServer.Servers[index];

                    if (server.State == ServerState.Stopped)
                    {
                        GameServer.Servers.RemoveAt(index);
                        continue;
                    }

                    index++;
                }
                alive = GameServer.Servers.Count != 0;
            }
        }
    }
}
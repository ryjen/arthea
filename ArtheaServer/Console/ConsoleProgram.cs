using ArtheaEngine;
using ArtheaEngine.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtheaServer.Console
{
    internal class ConsoleProgram : IProgram
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Run(Arguments args)
        {
            if (args.Has("reboot"))
                GameServer.Servers = RebootHelper.Recover();

            Log.Info("Starting console...");
            if (DbManager.Count<World>() == 0)
            {
                World.GetDefault();
            }

            new ConsoleGame().Run(args);
        }

        public void Exit()
        {
            Environment.Exit(0);
        }
    }

}

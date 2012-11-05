using System;
using ArtheaEngine;
using ArtheaEngine.Model;
using NLog;

namespace ArtheaServer.Console
{
    internal class ConsoleProgram : IProgram
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #region IProgram Members

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

        #endregion
    }
}
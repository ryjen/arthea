using System;
using ArtheaEngine;
using ArtheaEngine.Model;
using NLog;

namespace ArtheaServer.Console
{
    internal class ConsoleProgram : IProgram
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private ConsoleGame _game;

        #region IProgram Members

        public void Run(Arguments args)
        {
			
			Log.Info("Starting console...");

            if (args.Has("reboot"))
                GameServer.Servers = RebootHelper.Recover();

			_game = new ConsoleGame();

			_game.Run(args);
        }

        public void Exit()
        {
			_game.Exit();
        }

        #endregion
    }
}
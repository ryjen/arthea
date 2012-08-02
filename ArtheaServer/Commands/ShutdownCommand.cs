namespace ArtheaServer.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using NLog;

    public class ShutdownCommand : Command
    {
        #region Constructors

        public ShutdownCommand()
            : base("shutdown", "Shuts down the server", Game.MaxLevel)
        {
        }

        #endregion Constructors

        #region Methods

        public override bool CanUse(Character ch)
        {
            return ch.Level >= Game.MaxLevel;
        }

        public override void Execute(Character ch, Argument argument)
        {
            ch.WriteLine("Shutting down...");
            LogManager.GetCurrentClassLogger().Warn("Shutdown by {0}", ch);
            throw new ShutdownException();
        }

        #endregion Methods
    }
}
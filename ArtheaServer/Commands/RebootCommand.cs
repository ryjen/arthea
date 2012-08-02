namespace ArtheaServer.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using NLog;

    public class RebootCommand : Command
    {
        #region Constructors

        public RebootCommand()
            : base("reboot", "Reboots the server.", Game.Immortal)
        {
        }

        #endregion Constructors

        #region Methods

        public override bool CanUse(Character ch)
        {
            return ch.Level >= Game.Immortal;
        }

        public override void Execute(Character ch, Argument argument)
        {
            ch.WriteLine("Rebooting server...");
            LogManager.GetCurrentClassLogger().Warn("Reboot by {0}", ch);

            throw new RebootException();
        }

        #endregion Methods
    }
}
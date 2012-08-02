namespace ArtheaServer.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class SaveCommand : Command
    {
        #region Constructors

        public SaveCommand()
            : base("save", "Saves your character.")
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            if (!(ch is Player)) return;

			Player p = ch as Player;

			p.Save();

			p.Account.Save();

            ch.WriteLine(ch.Name + " saved.");
        }

        #endregion Methods
    }
}
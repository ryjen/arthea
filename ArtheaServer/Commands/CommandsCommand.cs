using System.Collections.Generic;
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Commands
{
    internal class CommandsCommand : Command
    {
        #region Constructors

        public CommandsCommand()
            : base("commands", "Displays available commands.")
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            if (!(ch is Player)) return;

            var p = ch as Player;

            int columns = p.Account.Connection.ScreenWidth/11;
            int i = 0;

            var sorted = new SortedList<string, Command>();

            foreach (Command cmd in Commands)
                sorted.Add(cmd.Name, cmd);

            foreach (var cmd in sorted)
            {
                if (!cmd.Value.CanUse(ch)) continue;

                if ((++i%columns) == 0)
                {
                    ch.WriteLine("{0,-10} ", cmd.Value.Name);
                }
                else
                {
                    ch.Write("{0,-10} ", cmd.Value.Name);
                }
            }
            if ((i%columns) != 0)
            {
                ch.WriteLine();
            }
        }

        #endregion Methods
    }
}
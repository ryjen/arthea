namespace ArtheaServer.Commands
{
    using System.Collections.Generic;

    using ArtheaEngine;
    using ArtheaEngine.Model;

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

            var columns = p.Account.Connection.ScreenWidth/11;
            var i = 0;

            var sorted = new SortedList<string, Command>();

            foreach (var cmd in Commands)
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
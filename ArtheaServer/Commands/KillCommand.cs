namespace ArtheaServer.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class KillCommand : Command
    {
        #region Constructors

        public KillCommand()
            : base("kill", "Attacks another character in the game.", 1)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            var victim = ch.Room.Characters.FirstOrDefault(x => x.Name.HasWord(argument));

            if (victim == null)
            {
                ch.WriteLine("They aren't here.");
                return;
            }

            ch.Fighting = new Combat(ch, victim);

            victim.Fighting = new Combat(victim, ch);

            UpdateManager.Add(ch.Fighting);

            UpdateManager.Add(victim.Fighting);

            ch.Fighting.Tick(); // do something right away
        }

        #endregion Methods
    }
}
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Commands
{
    public class LookCommand : Command
    {
        #region Constructors

        public LookCommand()
            : base("look", "Examines a room, person or object.", 1)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            if (!argument)
            {
                if (ch.Room == null)
                    return;

                ch.WriteLine("~G{0}~x", ch.Room.Name);
                ch.WriteLine(ch.Room.Description);

                ch.WriteLine();
                ch.Write("~g[Exits: ");
                if (ch.Room.Exits.Count == 0)
                {
                    ch.WriteLine("None]~x");
                }
                else
                {
                    foreach (var e in ch.Room.Exits)
                    {
                        ch.Write(e.Key);
                    }
                    ch.WriteLine("]~x");
                }
                if (ch.Room.Characters.Count > 0)
                {
                    ch.WriteLine();

                    foreach (Character rch in ch.Room.Characters)
                    {
                        if (rch == ch) continue;

                        ch.Act("{0} is here.", rch);
                    }
                }

                if (ch.Room.Objects.Count > 0)
                {
                    ch.WriteLine();

                    foreach (Object obj in ch.Room.Objects)
                    {
                        ch.Act("{0:L}", obj);
                    }
                }
            }
        }

        #endregion Methods
    }
}
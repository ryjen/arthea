using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArtheaEngine.Model;
using ArtheaEngine;

namespace ArtheaServer.Commands
{
    public class ForumCommand : Command
    {
        public ForumCommand()
            : base("forum", "Command to read/write notes on the forums.", 1)
        { }

        public override void Execute(Character ch, Argument argument)
        {
            if (!argument)
            {
                int i = 1;
                foreach (var f in Forum.List)
                {
                    if(f.ReadableBy(ch))
                        ch.WriteLine("~R{0:2}> ~G{1}~W: {2}~x", i++, f.Name, f.Description);
                }
            }
        }
    }

    public class NoteCommand : Command
    {
        public NoteCommand()
            : base("note", "Command to read/write notes.", 1)
        { }

        public override void Execute(Character ch, Argument argument)
        {
            
        }
    }
}

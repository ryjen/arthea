namespace ArtheaServer.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using ArtheaServer.Creation;

    using Object = ArtheaEngine.Model.Object;

    public class CreateCommand : Command
    {
        #region Constructors

        public CreateCommand()
            : base("create", "creates an entity", Game.MaxLevel)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            var arg = argument.Next();
            Player p = ch as Player;

            if (p == null) return;

            TelnetConnection conn = p.Account.Connection as TelnetConnection;

            if (arg.IsPrefixOf("character"))
            {
                var npc = new NonPlayer();

                npc.Name = !argument ? "NewMob" : argument.ToString();

                conn.Editing = new CharEditor(conn, npc);
                conn.Editing.Show();
                return;
            }

            if (arg.IsPrefixOf("object"))
            {
                var obj = new ArtheaEngine.Model.Object();

                obj.Name = !argument ? "NewObj" : argument.ToString();

                conn.Editing = new ObjectEditor(conn, obj);
                conn.Editing.Show();
                return;
            }

            if (arg.IsPrefixOf("help"))
            {
                var help = new Help();
                if(!argument.IsEmpty())
                    help.Keywords.Add(argument);

                conn.Editing = new HelpEditor(conn, help);
                conn.Editing.Show();
                return;
            }

            if (arg.IsPrefixOf("room"))
            {
                var room = new Room();

                room.Name = !argument ? "NewRoom" : argument.ToString();

                conn.Editing = new RoomEditor(conn, room);
                conn.Editing.Show();
                return;
            }
        }

        #endregion Methods
    }

    public class EditCommand : Command
    {
        #region Constructors

        public EditCommand()
            : base("edit", "edits entities in the world", Game.MaxLevel)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, ArtheaEngine.Argument argument)
        {
            var arg = argument.Next();
            Player p = ch as Player;

            if(p == null) return;

            TelnetConnection conn = p.Account.Connection as TelnetConnection;

            if (arg.IsPrefixOf("character"))
            {
                var vict = Character.Lookup(argument);
                if (vict == null)
                {
                    ch.WriteLine("No such character.");
                    return;
                }
                conn.Editing = new CharEditor(conn, vict);
                conn.Editing.Show();
                return;
            }

            if (arg.IsPrefixOf("room"))
            {
                var room = ch.Room;

                conn.Editing = new RoomEditor(conn, room);

                conn.Editing.Show();
                return;
            }

            if (arg.IsPrefixOf("object"))
            {
                var obj = Object.Lookup(argument);

                if (obj == null)
                {
                    ch.WriteLine("No such object.");
                    return;
                }

                conn.Editing = new ObjectEditor(conn, obj);
                conn.Editing.Show();
                return;
            }

            conn.WriteLine("Syntax: edit help");
            conn.WriteLine("        edit room");
            conn.WriteLine("        edit object");
            conn.WriteLine("        edit character");
        }

        #endregion Methods
    }
}
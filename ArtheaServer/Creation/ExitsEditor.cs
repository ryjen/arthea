namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class ExitsEditor : Editor
    {
        #region Fields

        private Room _data;
        private Dictionary<Direction, Exit> _exits;
        private PropertyInfo _prop;

        #endregion Fields

        #region Constructors

        public ExitsEditor(TelnetConnection conn, Room data, PropertyInfo prop)
            : base(conn)
        {
            _prop = prop;
            _data = data;
            _exits = (Dictionary<Direction, Exit>) prop.GetValue(data, null);
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("{0} Editor", _prop.Name); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            var arg = argument.Next();
             Room room;
             long id;
             Direction dir;
             string arg2;
             Exit exit;

             if (base.Execute(arg))
                 return true;

             if (arg.Is("L") || arg.Is("link"))
             {
                 arg2 = argument.Next();

                 if (!Enum.TryParse(arg2, true, out dir))
                 {
                     WriteLine("~CSyntax: 'link <direction> <room>'");
                     return true;
                 }

                 if (_exits.ContainsKey(dir))
                 {
                     WriteLine("~CThere is already an exit in that direction.~x");
                     return true;
                 }

                 if (!long.TryParse(argument, out id) || (room = Room.Lookup(id)) == null)
                 {
                     WriteLine("~CInvalid room to link to.");
                     return true;
                 }

                 exit = new Exit();
                 exit.ToRoom = room;
                 exit.Room = _data;

                 _exits.Add(dir, exit);
                 Show();
                 return true;

             }

             if (arg.Is("B") || arg.Is("build"))
             {
                 arg2 = argument.Next();

                 if (!Enum.TryParse(arg2, true, out dir))
                 {
                     WriteLine("~CSyntax: 'build <dir>'");
                     return true;
                 }

                 if (_exits.ContainsKey(dir))
                 {
                     WriteLine("~CCan't build an exit in that direction, one already exists.~x");
                     return true;
                 }

                 var revRoom = new Room();

                 exit = new Exit();
                 exit.ToRoom = revRoom;
                 exit.Room = _data;
                 exit.Direction = dir;

                 var revExit = new Exit();
                 revExit.ToRoom = _data;
                 revExit.Room = revRoom;
                 revExit.Direction = dir.Reverse();

                 revRoom.Exits.Add(dir.Reverse(), revExit);
                 _exits.Add(dir, exit);

                 Show();
                 return true;
             }

             if (arg.Is("A") || arg.Is("add"))
             {
                 arg2 = argument.Next();

                 if (!Enum.TryParse(arg2, true, out dir))
                 {
                     WriteLine("~CSyntax: 'add <direction> <room>'");
                     return true;
                 }

                 if (_exits.ContainsKey(dir))
                 {
                     WriteLine("~CThere is already an exit in that direction.~x");
                     return true;
                 }

                 if (!long.TryParse(argument, out id) || (room = Room.Lookup(id)) == null)
                 {
                     WriteLine("~CInvalid room to add exit to.");
                     return true;
                 }

                 if(room.Exits.ContainsKey(dir.Reverse()))
                 {
                     WriteLine("~CThere is already an exit to the {0} of that room.", dir.Reverse());
                     return true;
                 }

                 exit = new Exit();
                 exit.ToRoom = room;
                 exit.Room = _data;
                 exit.Direction = dir;

                 var revExit = new Exit();
                 revExit.ToRoom = _data;
                 revExit.Room = room;
                 revExit.Direction = dir.Reverse();

                 _exits.Add(dir, exit);
                 room.Exits.Add(dir.Reverse(), revExit);

                 Show();
             }

             if (arg.Is("D") || arg.Is("delete"))
             {
                 if (!Enum.TryParse(argument, true, out dir))
                 {
                     WriteLine("~CSyntax: 'delete <direction>'~x");
                     return true;
                 }

                 if (!_exits.ContainsKey(dir))
                 {
                     WriteLine("~CNo exit in that direction.~x");
                     return true;
                 }

                 _exits.Remove(dir);
                 Show();
                 return true;
             }

             if (arg.Is("E") || arg.Is("edit"))
             {
                 if (!Enum.TryParse(argument, true, out dir))
                 {
                     WriteLine("~CSyntax: 'edit <direction>'~x");
                     return true;
                 }

                 if (!_exits.ContainsKey(dir))
                 {
                     WriteLine("~CNo exit in that direction.~x");
                     return true;
                 }

                 var ed = new ExitEditor(_connection, _exits[dir]);
                 Forward(ed);
                 return true;
             }
             return false;
        }

        public override void Save()
        {
            _prop.SetValue(_data, _exits, null);
        }

        public override void Show()
        {
            Reset();

             WriteLine("~YA) ~CAdd a two way exit");
             WriteLine("~YL) ~CLink a one way exit");
             WriteLine("~YB) ~CBuild an exit and room");
             WriteLine("~YD) ~CDelete exit");
             WriteLine("~YE) ~CEdit exit");

             foreach (var entry in _exits)
             {
                 WriteLine("~W{0}) ~CExit to ~W{1}~C ({2})", entry.Key, entry.Value.ToRoom.Id, entry.Value.ToRoom.Name);
             }
        }

        #endregion Methods
    }
}
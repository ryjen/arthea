namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class RoomEditor : Editor
    {
        #region Fields

        private Room _data;

        #endregion Fields

        #region Constructors

        public RoomEditor(TelnetConnection conn, Room data)
            : base(conn)
        {
            _data = data;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Room Editor - {0}", _data.Name); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            var arg = argument.Next();

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("name"))
            {
                _data.Name = argument;
                Show();
                return true;
            }

            if (arg.Is("B") || arg.Is("description"))
            {
                var tmp = new TextEditor(_connection, _data, PropertyHelper<Room>.GetProperty(x => x.Description));
                Forward(tmp);
                return true;
            }

            if (arg.Is("C") || arg.Is("terrain"))
            {
                Terrain value;
                if (!Enum.TryParse(argument, true, out value))
                {
                    ShowValidEnumValues<Terrain>();
                    return true;
                }

                _data.Terrain = value;
                Show();
                return true;
            }

            if (arg.Is("D") || arg.Is("flags"))
            {
                RoomFlag flag;
                if (!Enum.TryParse(argument, true, out flag))
                {
                    ShowValidEnumValues<RoomFlag>();
                    return true;
                }

                _data.Flags ^= flag;
                Show();
                return true;
            }

            if (arg.Is("E") || arg.Is("exits"))
            {
                var ed = new ExitsEditor(_connection, _data, PropertyHelper<Room>.GetProperty(x => x.Exits));
                Forward(ed);
                return true;
            }

            if (arg.Is("F") || arg.Is("reset"))
            {
                var ed = new ResetEditor(_connection, _data.Reset == null ? new Reset(_data) : _data.Reset);
                Forward(ed);
                return true;
            }

            return false;
        }

        public override void Save()
        {
            /*if (DbManager.Save (_data))
                _connection.WriteLine("Room saved.");
            else
                _connection.WriteLine("Room not saved. See logs for details.");*/
        }

        public override void Show()
        {
            Reset();

            var cols = new ColumnList(2);

            cols.Add("   ~CId: ~W{0}", _data.Id);
            cols.Add("   ~CArea: ~W{0}", _data.Area.Name);
            cols.Add("~YA) ~CName: ~W{0}~x", _data.Name);
            cols.Add(2, TextEditor.Preview("~YB) ~CDescription", _data.Description, _connection.ScreenWidth));
            cols.Add("~YC) ~CTerrain: ~W{0}~x", _data.Terrain.ToString());
            cols.Add("~YD) ~CFlags: ~W{0}~x", _data.Flags);
            cols.Add("~RE) ~CExits: ~W{0} exits~x", _data.Exits.Count);
            cols.Add("~RF) ~CReset: ~W{0}~x", _data.Reset == null ? "none" : _data.Reset.Id);

            cols.Show(_connection.Playing);
        }

        #endregion Methods
    }
}
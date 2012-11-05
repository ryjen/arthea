using System;
using ArtheaEngine;
using ArtheaEngine.Model;
using Object = ArtheaEngine.Model.Object;

namespace ArtheaServer.Creation
{
    public class ExitEditor : Editor
    {
        #region Fields

        private readonly Exit _data;

        #endregion Fields

        #region Constructors

        public ExitEditor(TelnetConnection conn, Exit data)
            : base(conn)
        {
            _data = data;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Exit Editor - {0}", _data.Keyword); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            string arg = argument.Next();

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("name"))
            {
                _data.Keyword = argument;
                Show();
                return true;
            }

            if (arg.Is("B") || arg.Is("description"))
            {
                var ed = new TextEditor(_connection, _data, PropertyHelper<Exit>.GetProperty(x => x.Description));
                Forward(ed);
                return true;
            }

            if (arg.Is("C") || arg.Is("flags"))
            {
                ExitFlag flag;

                if (!Enum.TryParse(argument, true, out flag))
                {
                    ShowValidEnumValues<ExitFlag>();
                    return true;
                }

                _data.Flags ^= flag;
                Show();
                return true;
            }

            if (arg.Is("D") || arg.Is("key"))
            {
                Object key;

                if ((key = Object.Lookup(argument)) == null || key.Type != ObjectType.Key)
                {
                    WriteLine("Invalid key object.");
                    return true;
                }

                _data.Key = key;
                Show();
                return true;
            }
            return false;
        }

        public override void Save()
        {
        }

        public override void Show()
        {
            Reset();

            var cols = new ColumnFormatter(2, _connection.ScreenWidth);

            cols.Add("   ~CId: ~W{0}", _data.Keyword);
            cols.Add("   ~CTo Room: ~W{0}", _data.ToRoom);
            cols.Add("   ~CFrom Room: ~W{0}", _data.Room);
            cols.Add("   ~CDirection: ~W{0}", _data.Direction);
            cols.Add("~YA) ~CName: ~W{0}", _data.Keyword);

            cols.SetSpan(2).Add(TextEditor.Preview("~YB) ~CDescription", _data.Description, cols.Width));

            cols.Add("~YC) ~CFlags: ~W{0}", _data.Flags);
            cols.Add("~YD) ~CKey: ~W{0}",
                     _data.Key == null ? "None" : string.Format("{0} ({1})", _data.Key.Id, _data.Key.Name));

            _connection.Page(cols.GetBuffer());
        }

        #endregion Methods
    }
}
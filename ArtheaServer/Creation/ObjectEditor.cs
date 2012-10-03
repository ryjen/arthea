namespace ArtheaServer.Creation
{
    using ArtheaEngine;
    using ArtheaEngine.Model;
    using System;
    using System.Globalization;
    using Object = ArtheaEngine.Model.Object;

    public class ObjectEditor : Editor
    {
        #region Fields

        Object _data;

        #endregion Fields

        #region Constructors

        public ObjectEditor(TelnetConnection conn, Object data)
            : base(conn)
        {
            _data = data;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Object {0}", _data.Id); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            var arg = argument.Next();

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("type"))
            {
                ObjectType type;
                if (!Enum.TryParse(argument, true, out type))
                {
                    ShowValidEnumValues<ObjectType>();
                    return true;
                }

                _data.Type = type;
                Show();
                return true;
            }

            if (arg.Is("B") || arg.Is("name"))
            {
                _data.Name = argument;
                Show();
                return true;
            }

            if (arg.Is("C") || arg.Is("shortdescr"))
            {
                _data.ShortDescr = argument;
                Show();
                return true;
            }

            if (arg.Is("D") || arg.Is("longdescr"))
            {
                _data.LongDescr = argument;
                Show();
                return true;
            }

            if (arg.Is("E") || arg.Is("description"))
            {
                var ed = new TextEditor(_connection, _data, PropertyHelper<Object>.GetProperty(x => x.Description));
                Forward(ed);
                return true;
            }

            if (arg.Is("F") || arg.Is("level"))
            {
                short lvl;
                if (!short.TryParse(argument, out lvl))
                {
                    WriteLine("Invalid level");
                    return true;
                }

                _data.Level = lvl;
                Show();
                return true;
            }

            if (arg.Is("G") || arg.Is("weight"))
            {
                float weight;
                if (!float.TryParse(argument, out weight))
                {
                    WriteLine("Invalid weight");
                    return true;
                }
                _data.Weight = weight;
                Show();
                return true;
            }

            if (arg.Is("H") || arg.Is("cost"))
            {
                decimal value;
                if (!decimal.TryParse(argument, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat,out value))
                {
                    WriteLine("Invalid cost");
                    return true;
                }

                _data.Cost = value;
                Show();
                return true;
            }

            if (arg.Is("I") || arg.Is("flags"))
            {
                ObjectFlag bit;
                if (!Enum.TryParse(argument, true, out bit))
                {
                    ShowValidEnumValues<ObjectFlag>();
                    return true;
                }

                _data.Flags ^= bit;
                Show();
                return true;
            }

            if (arg.Is("J") || arg.Is("wearflags"))
            {
                WearFlag bit;
                if (!Enum.TryParse(argument, true, out bit))
                {
                    ShowValidEnumValues<WearFlag>();
                    return true;
                }

                _data.WearFlags ^= bit;
                Show();
                return true;
            }

            if (arg.Is("K") || arg.Is("affects"))
            {
                var ed = new AffectsEditor(_connection, _data, PropertyHelper<Object>.GetProperty(x => x.Affects));
                Forward(ed);
                return true;
            }

            return false;
        }

        public override void Save()
        {
            /*if (DbManager.Save(_data))
                _connection.WriteLine("Object saved.");
            else
                _connection.WriteLine("Object not saved. See logs for details.");*/
        }

        public override void Show()
        {
            Reset();

            ColumnFormatter cols = new ColumnFormatter(2, _connection.ScreenWidth);

            cols.Add("   ~CArea: ~W{0}", _data.Area.Name);
            cols.Add("   ~CId: ~W{0}", _data.Id);
            cols.Add("~YA) ~CType: ~W{0}", _data.Type);
            cols.Add("~YB) ~CName: ~W{0}", _data.Name);
            cols.Add("~YC) ~CShort Descr: ~W{0}", _data.ShortDescr);
            cols.Add("~YD) ~CLong Descr: ~W{0}", _data.LongDescr);
            cols.SetSpan(cols.GetSize()).Add(TextEditor.Preview("~YE) ~CDescription", _data.Description, cols.Width));
            cols.Add("~YF) ~CLevel: ~W{0}", _data.Level);
            cols.Add("~YG) ~CWeight: ~W{0}", _data.Weight);
            cols.Add("~YH) ~CCost: ~W{0}", _data.Cost);
            cols.Add("~YI) ~CFlags: ~W{0}", _data.Flags);
            cols.Add("~YJ) ~CWearFlags: ~W{0}", _data.WearFlags);

            cols.Add("~RK) Affects: ~W{0}~C affects", _data.Affects.Count);

			_connection.Page (cols.GetBuffer());
        }

        #endregion Methods
    }
}
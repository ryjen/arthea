using System;
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Creation
{
    public class AffectEditor : Editor
    {
        #region Fields

        private readonly Affect _data;

        #endregion Fields

        #region Constructors

        public AffectEditor(TelnetConnection conn, Affect data)
            : base(conn)
        {
            _data = data;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Affect {0}", _data.Id); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            string arg = argument.Next();

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("location"))
            {
                AffectLocation loc;

                if (!Enum.TryParse(argument, true, out loc))
                {
                    ShowValidEnumValues<AffectLocation>();
                    return true;
                }

                _data.Location = loc;
                Show();
                return true;
            }

            if (arg.Is("B") || arg.Is("modifier"))
            {
                int mod;

                if (!int.TryParse(argument, out mod))
                {
                    WriteLine("~CInvalid modifier");
                    return true;
                }

                _data.Modifier = mod;
                Show();
                return true;
            }

            if (arg.Is("C") || arg.Is("duration"))
            {
                int dur;

                if (!int.TryParse(argument, out dur))
                {
                    WriteLine("~CInvalid duration.");
                    return true;
                }

                _data.Duration = dur;
                Show();
                return true;
            }

            if (arg.Is("D") || arg.Is("flag"))
            {
                AffectFlag flag;

                if (!Enum.TryParse(argument, true, out flag))
                {
                    ShowValidEnumValues<AffectFlag>();
                    return true;
                }

                _data.Flag ^= flag;
                Show();
                return true;
            }

            if (arg.Is("E") || arg.Is("ability"))
            {
                Ability extra = Ability.Lookup(argument);

                if (extra == null)
                {
                    WriteLine("~CInvalid number.");
                    return true;
                }

                _data.Extra = argument;
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

            cols.Add("   ~CId: ~W{0}", _data.Id);
            cols.Add("~YA) ~CLocation: ~W{0}", _data.Location);
            cols.Add("~YB) ~CModifier: ~W{0}", _data.Modifier);
            cols.Add("~YC) ~CDuration: ~W{0}", _data.Duration);
            cols.Add("~YD) ~CFlag: ~W{0}", _data.Flag);
            cols.Add("~YE) ~Ability: ~W{0}", _data.Extra == null ? "none" : Ability.Lookup(_data.Extra).Name);

            _connection.Page(cols.GetBuffer());
        }

        #endregion Methods
    }
}
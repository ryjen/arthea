namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class AffectEditor : Editor
    {
        #region Fields

        Affect _data;

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
            get { return string.Format("Affect Editor - {0}", _data.Id); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            var arg = argument.Next();

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

             if(arg.Is("D") || arg.Is("flag"))
             {
                 AffectFlag flag;

                 if(!Enum.TryParse(argument, true, out flag))
                 {
                     ShowValidEnumValues<AffectFlag>();
                     return true;
                 }

                 _data.Flag ^= flag;
                 Show();
                 return true;
             }

             if(arg.Is("E") || arg.Is("extra"))
             {
                 int extra;

                 if(!int.TryParse(argument, out extra))
                 {
                     WriteLine("~CInvalid number.");
                     return true;
                 }

                 _data.Extra = extra;
                 Show();
                 return true;
             }
             return false;
        }

        public override void Save()
        {
            _data.Save();
        }

        public override void Show()
        {
            Reset();

             var cols = new ColumnList(2);

             cols.Add("   ~CId: ~W{0}", _data.Id);
             cols.Add("~YA) ~CLocation: ~W{0}", _data.Location);
             cols.Add("~YB) ~CModifier: ~W{0}", _data.Modifier);
             cols.Add("~YC) ~CDuration: ~W{0}", _data.Duration);
             cols.Add("~YD) ~CFlag: ~W{0}", _data.Flag);
             cols.Add("~YE) ~CExtra: ~W{0}", _data.Extra);

             cols.Show(_connection);
        }

        #endregion Methods
    }
}
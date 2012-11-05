using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Creation
{
    public class AffectsEditor : Editor
    {
        #region Fields

        private readonly Collection<Affect> _affects;
        private readonly object _data;
        private readonly PropertyInfo _prop;

        #endregion Fields

        #region Constructors

        public AffectsEditor(TelnetConnection conn, object data, PropertyInfo prop)
            : base(conn)
        {
            _prop = prop;
            _data = data;
            _affects = (Collection<Affect>) prop.GetValue(data, null);
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
            string arg = argument.Next();
            int num;
            AffectEditor ed;

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("add"))
            {
                var paf = new Affect();
                ed = new AffectEditor(_connection, paf);
                Forward(ed);
                return true;
            }

            if (arg.Is("D") || arg.Is("delete"))
            {
                if (!int.TryParse(argument, out num))
                {
                    WriteLine("~CSyntax: ~W'delete #'~x");
                    return true;
                }

                if (num < 0 || num >= _affects.Count)
                {
                    WriteLine("~CInvalid affect.");
                    return true;
                }

                _affects.RemoveAt(num);

                Show();
                return true;
            }

            if (arg.IsNumber())
            {
                if (!int.TryParse(arg, out num))
                {
                    WriteLine("~CSyntax: ~W'edit #'~x");
                    return true;
                }

                if (num < 0 || num >= _affects.Count)
                {
                    WriteLine("~CInvalid affect~x");
                    return true;
                }

                ed = new AffectEditor(_connection, _affects.ElementAt(num));
                Forward(ed);
                return true;
            }
            return false;
        }

        public override void Save()
        {
            _prop.SetValue(_data, _affects, null);
        }

        public override void Show()
        {
            Reset();

            WriteLine("~YA) ~CAdd affect");
            WriteLine("~YD) ~CDelete affect");
            WriteLine("~Y#) ~CEdit affect");
            WriteLine();

            for (int i = 0; i < _affects.Count; i++)
            {
                WriteLine("~Y{0,2})~C {1}", i + 1, _affects[i]);
            }
        }

        #endregion Methods
    }
}
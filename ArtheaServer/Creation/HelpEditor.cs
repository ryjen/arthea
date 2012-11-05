using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Creation
{
    public class HelpEditor : Editor
    {
        #region Fields

        private readonly Help _data;

        #endregion Fields

        #region Constructors

        public HelpEditor(TelnetConnection conn, Help help)
            : base(conn)
        {
            _data = help;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Help Editor - {0}", _data.Keywords.ToCSV()); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            string arg = argument.Next();

            if (base.Execute(argument))
                return true;

            if (arg.Is("A") || arg.Is("keywords"))
            {
                if (_data.Keywords.Contains(argument))
                {
                    _data.Keywords.Remove(argument);
                    _connection.WriteLine("~CKeyword removed.~x");
                }
                else
                {
                    _data.Keywords.Add(argument);
                    _connection.WriteLine("~CKeyword added.~x");
                }
                return true;
            }

            if (arg.Is("B") || arg.Is("text"))
            {
                var editor = new TextEditor(_connection, _data, PropertyHelper<Help>.GetProperty(x => x.Text));
                Forward(editor);
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

            var cols = new ColumnFormatter(2, _connection.ScreenWidth);

            cols.Add("~YA) ~CKeywords: ~W{0}~x", _data.Keywords.ToCSV());

            cols.SetSpan(2).Add(TextEditor.Preview("~YB) Text", _data.Text, cols.Width));

            _connection.Page(cols.GetBuffer());
        }

        #endregion Methods
    }
}
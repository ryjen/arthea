using System;
using System.Reflection;
using System.Text;
using ArtheaEngine;

namespace ArtheaServer.Creation
{
    public class TextEditor : Editor
    {
        #region Fields

        private const char KEY = '\\';

        private readonly object _data;
        private readonly PropertyInfo _prop;
        private StringBuilder buf;

        #endregion Fields

        #region Constructors

        public TextEditor(TelnetConnection conn, object data, PropertyInfo prop)
            : base(conn)
        {
            _prop = prop;
            _data = data;
            object temp = prop.GetValue(data, null);
            if (temp == null)
                buf = new StringBuilder();
            else
                buf = new StringBuilder(temp.ToString());

            Show();
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("{0} Editor", _prop.Name); }
        }

        #endregion Properties

        #region Methods

        public static string Preview(string header, string pstr, int width)
        {
            if (string.IsNullOrEmpty(pstr))
            {
                return string.Format("~C{0}: ~WEmpty~x", header);
            }

            int count = 1;
            int indent = header.DisplayLength() + 2;
            int lineLen = Math.Min(pstr.GetLineCountLength(1), width - indent);

            var output = new StringBuilder();

            int pos = 0;
            string temp = pstr.Substring(pos, lineLen);

            output.AppendFormat("~C{0}: {1,2}. ~W{2}", header, count, temp);

            pos += temp.Length;

            while (++count <= 2 && pos < pstr.Length)
            {
                lineLen = Math.Min(pstr.GetLineCountLength(pos, 1), width - indent);
                if (count == 3)
                    lineLen -= Environment.NewLine.Length;
                temp = pstr.Substring(pos, lineLen);
                output.AppendFormat("{0}~C{1,2}. ~W{2}~x", new string(' ', indent), count, temp);
                pos += temp.Length;
            }
            return output.ToString();
        }

        public override bool Execute(Argument argument)
        {
            if (argument.Length > 1 && argument[0] == KEY && argument[1] != KEY)
            {
                string arg = argument.Next();

                switch (arg.Substring(1))
                {
                    default:
                        return false;
                    case "q":
                        Save();
                        Back();
                        return true;
                    case "c":
                        buf = new StringBuilder();
                        WriteLine("~CText cleared.~x");
                        break;
                    case "s":
                        Show();
                        return true;
                    case "S":
                        _connection.Page(buf.ToString());
                        return true;
                    case "r":
                        string arg1 = argument.Next();

                        if (string.IsNullOrEmpty(arg1))
                        {
                            _connection.WriteLine("Usage: {0}r 'old string' 'new string'", KEY);
                        }
                        else
                        {
                            string arg2 = argument.Next();

                            buf = buf.Replace(arg1, arg2);
                            _connection.WriteLine("'{0}' replaced with '{1}'.", arg1, arg2);
                        }
                        break;
                    case "e":
                        int line;

                        if (!int.TryParse(argument.Next(), out line))
                        {
                            _connection.WriteLine("You must specify a line number.");
                            break;
                        }

                        buf = LineReplace(line, argument);
                        _connection.WriteLine("Line replaced.");
                        break;
                    case "i":
                        if (!int.TryParse(argument.Next(), out line))
                        {
                            _connection.WriteLine("You must specify a line number.");
                            break;
                        }
                        buf = LineAdd(line, argument);
                        _connection.WriteLine("Line inserted.");
                        break;
                    case "d":
                        if (int.TryParse(argument.Next(), out line))
                        {
                            buf = LineDelete(line);
                            _connection.WriteLine("Line {0} deleted.", line);
                        }
                        else
                        {
                            buf = DeleteLastLine();
                            _connection.WriteLine("Last line deleted.");
                        }
                        break;
                    case "D":
                        buf.Remove(buf.Length - 1, 1);
                        _connection.WriteLine("Last character deleted.");
                        break;
                    case "h":
                    case "?":
                        Help();
                        return true;
                }
            }
            else
            {
                buf.AppendLine(argument);
            }

            Show();
            return true;
        }

        public void Help()
        {
            _connection.WriteLine();

            _connection.WriteLine("Commands:");
            _connection.WriteLine("/h               - displays this help");
            _connection.WriteLine("/e <line> [text] - replaces line number with text");
            _connection.WriteLine("/i <line> [text] - inserts text at line number");
            _connection.WriteLine("/d [line]        - deletes the last line or the specified line");
            _connection.WriteLine("/r <old> [new]   - replaces old string with new string");
            _connection.WriteLine("/c               - clears the text");
            _connection.WriteLine("/s               - shows the text with line numbers");
            _connection.WriteLine("/S               - shows the text without line numbers");
            _connection.WriteLine("/q               - quits the editor");
        }

        public override void Save()
        {
            _prop.SetValue(_data, buf.ToString(), null);
        }

        public override void Show()
        {
            Reset();

            string[] lines = buf.ToString().GetLines();

            for (int i = 0; i < lines.Length; i++)
            {
                _connection.WriteLine("{0,2}. {1}", i + 1, lines[i]);
            }
        }

        private StringBuilder DeleteLastLine()
        {
            string[] lines = buf.ToString().GetLines();

            var newText = new StringBuilder();

            for (int i = 0; i < lines.Length - 2; i++)
            {
                newText.AppendLine(lines[i]);
            }
            return newText;
        }

        private StringBuilder LineAdd(int line, String addStr)
        {
            string[] lines = buf.ToString().GetLines();
            var newText = new StringBuilder();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                if ((i + 1) == line)
                    newText.AppendLine(addStr);

                newText.AppendLine(lines[i]);
            }

            return newText;
        }

        private StringBuilder LineDelete(int line)
        {
            string[] lines = buf.ToString().GetLines();
            var newText = new StringBuilder();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                if ((i + 1) != line)
                {
                    newText.AppendLine(lines[i]);
                }
            }

            return newText;
        }

        private StringBuilder LineReplace(int line, String addStr)
        {
            string[] lines = buf.ToString().GetLines();
            var newText = new StringBuilder();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                if ((i + 1) == line)
                    newText.AppendLine(addStr);
                else
                    newText.AppendLine(lines[i]);
            }

            return newText;
        }

        #endregion Methods
    }
}
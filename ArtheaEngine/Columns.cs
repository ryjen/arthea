namespace ArtheaEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using ArtheaEngine.Model;

    public class Column
    {
        #region Properties

        public object Content
        {
            get; set;
        }

        public int Span
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Content == null ? string.Empty : Content.ToString();
        }

        #endregion Methods
    }

    public class ColumnList
    {
        #region Fields

        private Row _currRow;
        private LinkedList<Row> _rows = new LinkedList<Row>();
        private int _savedSize;

        #endregion Fields

        #region Constructors

        public ColumnList(int rowLen)
        {
            _currRow = new Row(rowLen);

            _rows.AddLast(_currRow);
        }

        #endregion Constructors

        #region Methods

        public void Add(object content)
        {
            var c = new Column { Span = 1, Content = content };

            AddColumn(c);
        }

        public void Add(int span, object content)
        {
            AddColumn(new Column { Span = span == -1 ? _currRow.Size : span, Content = content });
        }

        public void Add(string format, params object[] values)
        {
            var tmp = string.Format(format, values);

               AddColumn(new Column { Span = 1, Content = tmp });
        }

        public void Add(int span, string format, params object[] values)
        {
            var tmp =  string.Format(format, values);

            AddColumn(new Column { Span = span == -1 ? _currRow.Size : span, Content = tmp });
        }

        public void AddHeader(object content)
        {
            AddColumn(new Header { Span = _currRow.Size, Content = content });
        }

        public void RestoreSize()
        {
            SetSize(_savedSize);
        }

        public void SaveSize()
        {
            _savedSize = _currRow.Size;
        }

        public void SetSize(int size)
        {
            if (_currRow.Span() == 0)
            {
                _currRow.Size = size;
            }
            else
            {
                _currRow = new Row(size);
                _rows.AddLast(_currRow);
            }
        }

        public void Show(IWritable to)
        {
            int width;

            if (to is Player)
            {
                width = (to as Player).Account.Connection.ScreenWidth;
            }
            else if (to is IConnection)
            {
                width = (to as IConnection).ScreenWidth;
            }
            else if (to is Account)
            {
                width = (to as Account).Connection.ScreenWidth;
            }
            else
            {
                width = 80;
            }

            Show(to, width);
        }

        public void Show(IWritable to, int width)
        {
            StringBuilder buf = new StringBuilder();

            foreach (var row in _rows)
            {
                int iCount = 0;

                foreach (var c in row.ToArray())
                {
                    var text = c.ToString();
                    string format;
                    var hiddenLength = (text.Length - text.DisplayLength());

                    iCount += c.Span;

                    if (iCount > row.Size)
                    {
                        buf.AppendLine();
                        iCount = 0;
                    }

                    if (c is Header)
                        format = string.Format("~!B~W{{0,-{0}}}", (width/row.Size) + hiddenLength);
                    else
                        format = string.Format("{{0,-{0}}}", (width/row.Size) + hiddenLength);

                    buf.AppendFormat(format, text);

                }
                buf.AppendLine();
            }
            /*
            for (int i = 0, c = 0; i < _columns.Count; i++)
            {
                var text = _columns[i].ToString();

                if((c % Length) + _columns[i].Span > Length)
                    buf.AppendLine();

                string format;

                if(_columns[i] is Header)
                    format = string.Format("~!B~W{{0,-{0}}}", width + (text.Length - text.DisplayLength()));
                else
                    format = string.Format("{{0,-{0}}}", width + (text.Length - text.DisplayLength()));

                buf.AppendFormat(format, text);

                c += _columns[i].Span;

                if (text.DisplayLength() >= width || (c % Length) == 0 || i == _columns.Count)
                {
                    buf.AppendLine();
                }
            }*/

            to.Page(buf.ToString());
        }

        private void AddColumn(Column c)
        {
            if (!_currRow.Add(c))
            {
                _currRow = new Row(_currRow.Size);
                _rows.AddLast(_currRow);
                _currRow.Add(c);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Handles displaying text in columns
    /// </summary>
    public static class Columns
    {
        #region Fields

        /// <summary>
        /// The default width for a column
        /// </summary>
        public static readonly ushort DefaultWidth = 80;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Shows the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="objects">The objects.</param>
        public static void Show(Player player, ushort columns, IList objects)
        {
            Show(player, columns, GetPlayerDefaultWidth(player, columns), objects);
        }

        /// <summary>
        /// Displays a list of objects in columns
        /// </summary>
        /// <param name="player">the player viewing the columns</param>
        /// <param name="columns">the number of columns</param>
        /// <param name="width">the width of a column</param>
        /// <param name="objects">the list of objects to display in the columns as an array</param>
        public static void Show(IWritable player, ushort columns, ushort width, IList objects)
        {
            StringBuilder buf = new StringBuilder();

            for(var i = 0; i < objects.Count; )
            {
                var text = objects[i].ToString();

                var format = string.Format("{{0,-{0}}}", width + (text.Length - text.DisplayLength()));

                buf.AppendFormat(format, text);

                ++i;

                if (text.DisplayLength() >= width || (i % columns) == 0 || i == objects.Count)
                {
                    buf.AppendLine();
                }
            }

            player.Page(buf.ToString());
        }

        /// <summary>
        /// Gets the width of the player default.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        private static ushort GetPlayerDefaultWidth(Player player, ushort columns)
        {
            return (ushort)(player.Account.Connection.ScreenWidth / columns);
        }

        #endregion Methods
    }

    public class Header : Column
    {
    }

    public class Row
    {
        #region Fields

        private List<Column> _columns = new List<Column>();

        #endregion Fields

        #region Constructors

        public Row(int size)
        {
            Size = size;
        }

        #endregion Constructors

        #region Properties

        public int Size
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public bool Add(Column c)
        {
            if (Span() >= Size)
                return false;

            _columns.Add(c);
            return true;
        }

        public int Span()
        {
            int count = 0;
            foreach (var c in _columns)
            {
                count += c.Span;
            }
            return count;
        }

        public Column[] ToArray()
        {
            return _columns.ToArray();
        }

        #endregion Methods
    }
}
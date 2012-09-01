using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using ArtheaEngine.Model;

namespace ArtheaEngine
{

	[Flags]
	public enum ColumnBorder {
		None, Left, Right 
	}

	public enum ColumnAlign {
		Left, Center, Right
	}
		
    public class Column
    {
		public Column()
		{
			BorderText = '#';
			BorderColor = "~B~!B";
		}

        #region Properties

        public object Content
        {
            get; set;
        }

        public int Span
        {
            get; set;
        }

		public char BorderText {
			get;
			set;
		}

		public string BorderColor {
			get;
			set;
		}
        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Content == null ? string.Empty : Content.ToString();
        }

		public virtual string ToString (int columnWidth)
		{
			var text = ToString ();

			switch (Align) {
			case ColumnAlign.Left:
			default:
				return text;
			case ColumnAlign.Center:
				return text.PadLeft ((columnWidth - text.DisplayLength ()) / 2 + text.DisplayLength ());
			case ColumnAlign.Right:
				return text.PadLeft (columnWidth - text.DisplayLength () + text.DisplayLength ());
			}
		}

		public ColumnBorder Border
		{
			get; set;
		}

		public ColumnAlign Align
		{
			get; set;
		}

		public virtual string Format (int size)
		{
			StringBuilder buf = new StringBuilder ();

			bool right = (Border & ColumnBorder.Right) == ColumnBorder.Right;

			if ((Border & ColumnBorder.Left) == ColumnBorder.Left) {
				buf.Append (BorderColor).Append (BorderText).Append("~x ");
				size -= 2;
			}

			if (right) {
				size -= 2;
			}

			buf.AppendFormat ("{{0,-{0}}}", size);

			if(right)
				buf.Append (' ').Append (BorderColor).Append(BorderText).Append ("~x");

			return buf.ToString();
		}

        #endregion Methods
    }

	public class Header : Column
	{
		public override string Format (int size)
		{
			return string.Format ("{{0,-{0}}}~x", size);
		}

		public override string ToString (int columnWidth)
		{
			var text = ToString ();
			var buf = new StringBuilder();

			switch (Align) {
			case ColumnAlign.Left:
			default:
				int mod = string.IsNullOrEmpty(text) ? 0 : 3;
				
				buf.Append(BorderColor);
				if(mod > 0) {
					buf.Append(BorderText);
					buf.Append("~W ");
					buf.Append(text);
					buf.Append(' ');
					buf.Append(BorderColor);
				}
				buf.Append(BorderText, columnWidth-text.DisplayLength()-mod);
				return buf.ToString();
			case ColumnAlign.Center:
				mod = string.IsNullOrEmpty(text) ? 0 : 1;
				buf.Append(BorderColor);
				buf.Append(BorderText, (columnWidth - text.DisplayLength ()) / 2-mod);
				if(mod > 0)
					buf.Append("~W ").Append(text).Append(' ');
				buf.Append(BorderColor);
				buf.Append(BorderText, (columnWidth-text.DisplayLength())/2-mod);
				return buf.ToString();
			case ColumnAlign.Right:
				mod = string.IsNullOrEmpty(text) ? 0 : 3;
				buf.Append(BorderColor);
				buf.Append(BorderText, columnWidth - text.DisplayLength ()-mod);
				if(mod > 0)
					buf.Append("~W ").Append(text).Append(' ');
				buf.Append (BorderColor).Append(BorderText);
				return buf.ToString();
			}
		}
	}

	/**
	 * Text output formatter
	 * 
	 * +-----------------+------------------+-------------------+
	 * |   column 1      |    column 2      |   column 3        |
	 * +-----------------+------------------+-------------------+
	 */
    public class ColumnFormatter
    {
		private static readonly char DefaultBorderText = '#';
		private static readonly string DefaultBorderColor = "~B~!B";

        #region Fields

        private Row _currRow;
        private List<Row> _rows = new List<Row>();
        private int _savedSize;
		int _nextSpan = 1;
		ColumnBorder _border = ColumnBorder.None;
		ColumnAlign _align = ColumnAlign.Left;
		char _borderText = DefaultBorderText;
		string _borderColor = DefaultBorderColor;

        #endregion Fields

        #region Constructors

        public ColumnFormatter(int columns, int width)
        {
            _currRow = new Row(columns);

			Width = width;

            _rows.Add(_currRow);
        }

		public ColumnFormatter (int columns, object width) : this(columns, GetWidthFromObject(width))
		{
		}

		private static int GetWidthFromObject (object o)
		{
			if (o is IConnection) {
				return (o as IConnection).ScreenWidth;
			}

			if (o is Player) {
				return (o as Player).Account.Connection.ScreenWidth;
			}

			if (o is Account) {
				return (o as Account).Connection.ScreenWidth;
			}

			int w;
			if (int.TryParse (o.ToString (), out w)) {
				return w;
			}

			throw new ArgumentException("cannot determine width from object");
		}

		public int Width
		{
			get; 
			private set;
		}

        #endregion Constructors

        #region Methods

        public ColumnFormatter Add(object content)
        {
            var c = new Column { Border = _border, BorderText = _borderText, BorderColor = _borderColor, Span = _nextSpan, Align = _align, Content = content };

            AddColumn(c);

			return this;

        }

        public ColumnFormatter Add(string format, params object[] values)
        {
            var tmp = string.Format(format, values);

             AddColumn(new Column { Border = _border, BorderText = _borderText, BorderColor = _borderColor, Span = _nextSpan, Align = _align, Content = tmp });

			return this;
        }

        public ColumnFormatter AddHeader(object content)
        {
            AddColumn(new Header { Span = _currRow.Size, BorderText = _borderText, BorderColor = _borderColor, Align = _align, Content = content });

			return this;
        }

		public ColumnFormatter AddHeader()
		{
			AddColumn(new Header { Span = _currRow.Size,BorderText = _borderText, BorderColor = _borderColor });

			return this;
		}

		public ColumnFormatter AddHeader (int span, object content)
		{
			AddColumn (new Header { Span = span, Align = _align, BorderText = _borderText, BorderColor = _borderColor, Content = content });

			return this;
		}

        public ColumnFormatter RestoreSize()
        {
            return SetSize(_savedSize);
        }

        public ColumnFormatter SaveSize()
        {
            _savedSize = _currRow.Size;

			return this;
        }

		public int GetSize ()
		{
			return _currRow.Size;
		}

        public ColumnFormatter SetSize(int size)
        {
            if (_currRow.GetSpan() == 0)
            {
                _currRow.Size = size;
            }
            else
            {
                _currRow = new Row(size);
                _rows.Add(_currRow);
            }

			return this;
        }

		public ColumnBorder GetBorder ()
		{
			return _border;
		}

		public ColumnFormatter SetBorder (ColumnBorder border)
		{
			_border = border;

			return this;
		}

		public ColumnFormatter SetBorderText (char text)
		{
			_borderText = text;

			return this;
		}

		public ColumnFormatter SetBorderColor (string color)
		{
			_borderColor = color;

			return this;
		}

		public char GetBorderText ()
		{
			return _borderText;
		}

		public string GetBorderColor ()
		{
			return _borderColor;
		}

		public ColumnFormatter SetAlign (ColumnAlign align)
		{
			_align = align;

			return this;
		}

		public ColumnAlign GetAlign ()
		{
			return _align;
		}

		public int GetSpan ()
		{
			return _nextSpan == -1 ? _currRow.Size : _nextSpan;
		}

		public ColumnFormatter SetSpan (int span)
		{
			_nextSpan = span == -1 ? _currRow.Size : span;

			return this;
		}

        public StringBuilder GetBuffer()
        {
			StringBuilder buf = new StringBuilder();

            foreach (var row in _rows)
            {
                int iCount = 0;

                foreach (var c in row.ToArray())
                {
					
                    var text = c.ToString();
                    var hiddenLength = (text.Length - text.DisplayLength());
					int columnWidth = (Width/row.Size) + hiddenLength;

					int len = text.DisplayLength();
					int span = c.Span;

					while(len > columnWidth) 
					{
						len -= columnWidth;
						span++;
					}

                    iCount += span;

                    if (iCount > row.Size)
                    {
                        buf.AppendLine();
                        iCount = 0;
                    }

					columnWidth *= span;

					var format = c.Format(columnWidth);

					buf.AppendFormat(format, c.ToString(columnWidth));
				
                }
				
				buf.AppendLine();
            }
			return buf;
        }

		public override string ToString ()
		{
			return GetBuffer ().ToString();
		}

		public static implicit operator string(ColumnFormatter cols)
		{
			return cols.ToString();
		}

        private void AddColumn(Column c)
        {
			_nextSpan = 1;
			_border = ColumnBorder.None;
			_align = ColumnAlign.Left;

            if (!_currRow.Add(c))
            {
                _currRow = new Row(_currRow.Size);
                _rows.Add(_currRow);
                _currRow.Add(c);
            }
        }

        #endregion Methods
    }


    /// <summary>
    /// Handles displaying text in columns
    /// </summary>
	/*
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
    }*/

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
            if (GetSpan() >= Size)
                return false;

            _columns.Add(c);
            return true;
        }

        public int GetSpan()
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
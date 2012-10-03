namespace ArtheaEngine
{
    using ArtheaEngine.Model;
    using System;
    using System.Collections.Generic;
    using System.Text;

    #region Enumerations

    /// <summary>
    /// Column align.
    /// </summary>
    public enum ColumnAlign
    {
        Left, Center, Right
    }

    /// <summary>
    /// Column border.
    /// </summary>
    [Flags]
    public enum ColumnBorder
    {
        None, Left, Right
    }

    #endregion Enumerations

    /// <summary>
    /// Column.
    /// </summary>
    public class Column
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Column"/> class.
        /// </summary>
        public Column()
        {
            BorderText = '#';
            BorderColor = "~B~!B";
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the align.
        /// </summary>
        /// <value>
        /// The align.
        /// </value>
        public ColumnAlign Align
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the border.
        /// </summary>
        /// <value>
        /// The border.
        /// </value>
        public ColumnBorder Border
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        /// <value>
        /// The color of the border.
        /// </value>
        public string BorderColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the border text.
        /// </summary>
        /// <value>
        /// The border text.
        /// </value>
        public char BorderText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public object Content
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the span.
        /// </summary>
        /// <value>
        /// The span.
        /// </value>
        public int Span
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Format the specified size.
        /// </summary>
        /// <param name='size'>
        /// Size.
        /// </param>
        public virtual string Format(int size)
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Column"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Column"/>.
        /// </returns>
        public override string ToString()
        {
            return Content == null ? string.Empty : Content.ToString();
        }

        /// <summary>
        /// Tos the string.
        /// </summary>
        /// <returns>
        /// The string.
        /// </returns>
        /// <param name='columnWidth'>
        /// Column width.
        /// </param>
        public virtual string ToString(int columnWidth)
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

        #endregion Methods
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
        #region Fields

        private static readonly string DefaultBorderColor = "~B~!B";
        private static readonly char DefaultBorderText = '#';

        ColumnAlign _align = ColumnAlign.Left;
        ColumnBorder _border = ColumnBorder.None;
        string _borderColor = DefaultBorderColor;
        char _borderText = DefaultBorderText;
        private Row _currRow;
        int _nextSpan = 1;
        private List<Row> _rows = new List<Row>();
        private int _savedSize;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.ColumnFormatter"/> class.
        /// </summary>
        /// <param name='columns'>
        /// Columns.
        /// </param>
        /// <param name='width'>
        /// Width.
        /// </param>
        public ColumnFormatter(int columns, int width)
        {
            _currRow = new Row(columns);

            Width = width;

            _rows.Add(_currRow);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.ColumnFormatter"/> class.
        /// </summary>
        /// <param name='columns'>
        /// Columns.
        /// </param>
        /// <param name='width'>
        /// Width.
        /// </param>
        public ColumnFormatter(int columns, object width)
            : this(columns, GetWidthFromObject(width))
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <param name='cols'>
        /// Cols.
        /// </param>
        public static implicit operator string(ColumnFormatter cols)
        {
            return cols.ToString();
        }

        /// <summary>
        /// Add the specified content.
        /// </summary>
        /// <param name='content'>
        /// Content.
        /// </param>
        public ColumnFormatter Add(object content)
        {
            var c = new Column { Border = _border, BorderText = _borderText, BorderColor = _borderColor, Span = _nextSpan, Align = _align, Content = content };

            AddColumn(c);

            return this;
        }

        /// <summary>
        /// Add the specified format and values.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='values'>
        /// Values.
        /// </param>
        public ColumnFormatter Add(string format, params object[] values)
        {
            var tmp = string.Format(format, values);

             AddColumn(new Column { Border = _border, BorderText = _borderText, BorderColor = _borderColor, Span = _nextSpan, Align = _align, Content = tmp });

            return this;
        }

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <returns>
        /// The header.
        /// </returns>
        /// <param name='content'>
        /// Content.
        /// </param>
        public ColumnFormatter AddHeader(object content)
        {
            AddColumn(new Header { Span = _currRow.Size, BorderText = _borderText, BorderColor = _borderColor, Align = _align, Content = content });

            return this;
        }

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <returns>
        /// The header.
        /// </returns>
        public ColumnFormatter AddHeader()
        {
            AddColumn(new Header { Span = _currRow.Size,BorderText = _borderText, BorderColor = _borderColor });

            return this;
        }

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <returns>
        /// The header.
        /// </returns>
        /// <param name='span'>
        /// Span.
        /// </param>
        /// <param name='content'>
        /// Content.
        /// </param>
        public ColumnFormatter AddHeader(int span, object content)
        {
            AddColumn (new Header { Span = span, Align = _align, BorderText = _borderText, BorderColor = _borderColor, Content = content });

            return this;
        }

        /// <summary>
        /// Gets the align.
        /// </summary>
        /// <returns>
        /// The align.
        /// </returns>
        public ColumnAlign GetAlign()
        {
            return _align;
        }

        /// <summary>
        /// Gets the border.
        /// </summary>
        /// <returns>
        /// The border.
        /// </returns>
        public ColumnBorder GetBorder()
        {
            return _border;
        }

        /// <summary>
        /// Gets the color of the border.
        /// </summary>
        /// <returns>
        /// The border color.
        /// </returns>
        public string GetBorderColor()
        {
            return _borderColor;
        }

        /// <summary>
        /// Gets the border text.
        /// </summary>
        /// <returns>
        /// The border text.
        /// </returns>
        public char GetBorderText()
        {
            return _borderText;
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <returns>
        /// The buffer.
        /// </returns>
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

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <returns>
        /// The size.
        /// </returns>
        public int GetSize()
        {
            return _currRow.Size;
        }

        /// <summary>
        /// Gets the span.
        /// </summary>
        /// <returns>
        /// The span.
        /// </returns>
        public int GetSpan()
        {
            return _nextSpan == -1 ? _currRow.Size : _nextSpan;
        }

        /// <summary>
        /// Restores the size.
        /// </summary>
        /// <returns>
        /// The size.
        /// </returns>/
        public ColumnFormatter RestoreSize()
        {
            return SetSize(_savedSize);
        }

        /// <summary>
        /// Saves the size.
        /// </summary>
        /// <returns>
        /// The size.
        /// </returns>
        public ColumnFormatter SaveSize()
        {
            _savedSize = _currRow.Size;

            return this;
        }

        /// <summary>
        /// Sets the align.
        /// </summary>
        /// <returns>
        /// The align.
        /// </returns>
        /// <param name='align'>
        /// Align.
        /// </param>
        public ColumnFormatter SetAlign(ColumnAlign align)
        {
            _align = align;

            return this;
        }

        /// <summary>
        /// Sets the border.
        /// </summary>
        /// <returns>
        /// The border.
        /// </returns>
        /// <param name='border'>
        /// Border.
        /// </param>
        public ColumnFormatter SetBorder(ColumnBorder border)
        {
            _border = border;

            return this;
        }

        /// <summary>
        /// Sets the color of the border.
        /// </summary>
        /// <returns>
        /// The border color.
        /// </returns>
        /// <param name='color'>
        /// Color.
        /// </param>
        public ColumnFormatter SetBorderColor(string color)
        {
            _borderColor = color;

            return this;
        }

        /// <summary>
        /// Sets the border text.
        /// </summary>
        /// <returns>
        /// The border text.
        /// </returns>
        /// <param name='text'>
        /// Text.
        /// </param>
        public ColumnFormatter SetBorderText(char text)
        {
            _borderText = text;

            return this;
        }

        /// <summary>
        /// Sets the size.
        /// </summary>
        /// <returns>
        /// The size.
        /// </returns>
        /// <param name='size'>
        /// Size.
        /// </param>
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

        /// <summary>
        /// Sets the span.
        /// </summary>
        /// <returns>
        /// The span.
        /// </returns>
        /// <param name='span'>
        /// Span.
        /// </param>
        public ColumnFormatter SetSpan(int span)
        {
            _nextSpan = span == -1 ? _currRow.Size : span;

            return this;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.ColumnFormatter"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.ColumnFormatter"/>.
        /// </returns>
        public override string ToString()
        {
            return GetBuffer ().ToString();
        }

        private static int GetWidthFromObject(object o)
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
    /// Header column.
    /// </summary>
    public class Header : Column
    {
        #region Methods

        /// <summary>
        /// Format the specified size.
        /// </summary>
        /// <param name='size'>
        /// Size.
        /// </param>
        public override string Format(int size)
        {
            return string.Format ("{{0,-{0}}}~x", size);
        }

        /// <summary>
        /// Tos the string.
        /// </summary>
        /// <returns>
        /// The string.
        /// </returns>
        /// <param name='columnWidth'>
        /// Column width.
        /// </param>
        public override string ToString(int columnWidth)
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

        #endregion Methods
    }

    /// <summary>
    /// A column Row.
    /// </summary>
    public class Row
    {
        #region Fields

        private List<Column> _columns = new List<Column>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Row"/> class.
        /// </summary>
        /// <param name='size'>
        /// Size.
        /// </param>
        public Row(int size)
        {
            Size = size;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add the specified c.
        /// </summary>
        /// <param name='c'>
        /// C.
        /// </param>
        public bool Add(Column c)
        {
            if (GetSpan() >= Size)
                return false;

            _columns.Add(c);
            return true;
        }

        /// <summary>
        /// Gets the span.
        /// </summary>
        /// <returns>
        /// The span.
        /// </returns>
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
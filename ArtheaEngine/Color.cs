namespace ArtheaEngine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    #region Enumerations

    /// <summary>
    /// Color flags.
    /// </summary>
    [Flags]
    public enum ColorFlags
    {
        None = 0,
        Bright = (1 << 0), // 1
        Background = (1 << 1),
        Blink = (1 << 2), // 5
        Reverse = (1 << 3), // 7
        Dim = (1 << 4), // 2
        Underscore = (1 << 5) // 4
    }

    /// <summary>
    /// Color type.
    /// </summary>
    public enum ColorType
    {
        None = 0,

        /// <summary>
        /// A black foreground value
        /// </summary>
        Black = 30,

        /// <summary>
        /// A blue foreground value
        /// </summary>
        Blue = 34,

        /// <summary>
        /// A Cyan foreground value
        /// </summary>
        Cyan = 36,

        /// <summary>
        /// A green foreground value
        /// </summary>
        Green = 32,

        /// <summary>
        /// A magenta foreground value
        /// </summary>
        Magenta = 35,

        /// <summary>
        /// A red foreground value
        /// </summary>
        Red = 31,

        /// <summary>
        /// A white foreground value
        /// </summary>
        White = 37,

        /// <summary>
        /// A yellow foreground value
        /// </summary>
        Yellow = 33,
    }

    #endregion Enumerations

    /// <summary>
    /// Color.
    /// </summary>
    public class Color
    {
        #region Fields

        /// <summary>
        /// The color escape code
        /// </summary>
        public const char Code = '~';

        /// <summary>
        /// Escape code
        /// </summary>
        public const char Escape = '\x1B';

        private static readonly Dictionary<char, ColorFlags> AttrMap = new Dictionary<char, ColorFlags>
                                                                           {
                                                                               {'!', ColorFlags.Background},
                                                                               {'@', ColorFlags.Blink},
                                                                               {'#', ColorFlags.Reverse},
                                                                               {'$', ColorFlags.Dim},
                                                                               {'%', ColorFlags.Underscore}
                                                                           };
        private static readonly Dictionary<char, ColorType> ColorMap = new Dictionary<char, ColorType>
                                                                           {
                                                                               {'g', ColorType.Green},
                                                                               {'r', ColorType.Red},
                                                                               {'b', ColorType.Blue},
                                                                               {'y', ColorType.Yellow},
                                                                               {'m', ColorType.Magenta},
                                                                               {'c', ColorType.Cyan},
                                                                               {'d', ColorType.Black},
                                                                               {'w', ColorType.White}
                                                                           };
        private static readonly Random Rand = new Random ();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Color"/> class.
        /// </summary>
        /// <param name='type'>
        /// Type.
        /// </param>
        public Color(ColorType type)
        {
            Value = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Color"/> class.
        /// </summary>
        /// <param name='type'>
        /// Type.
        /// </param>
        /// <param name='cFlags'>
        /// C flags.
        /// </param>
        public Color(ColorType type, ColorFlags cFlags)
        {
            Value = type;
            Flags = cFlags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        public Color()
        {
            Value = ColorType.None;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public ColorFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public ColorType Value
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the color based on a code.
        /// </summary>
        public static Color Parse(string buf, ref int index)
        {
            var col = new Color ();
            do {
                if (AttrMap.ContainsKey (buf [index])) {
                    col.Flags |= AttrMap [buf [index]];
                    continue;
                }

                if (char.IsLetter (buf [index])) {
                    if (char.IsUpper (buf [index]))
                        col.Flags |= ColorFlags.Bright;
                    else
                        col.Flags &= ~ColorFlags.Bright;

                    var key = char.ToLower (buf [index]);

                    if (ColorMap.ContainsKey (key))
                        col.Value = ColorMap [key];
                } else if (buf [index] == '?') {
                    var types = Enum.GetValues (typeof(ColorType));
                    if (Rand.Next (0, 10) <= 5)
                        col.Flags |= ColorFlags.Bright;
                    col.Value = (ColorType)types.GetValue (Rand.Next (1, types.Length));
                }

                break;
            } while (++index < buf.Length);

            return col;
        }

        /// <summary>
        /// Clear this instance.
        /// </summary>
        public void Clear()
        {
            Value = ColorType.None;
            Flags = 0;
        }

        /// <summary>
        /// Format this instance.
        /// </summary>
        public string Format()
        {
            var buf = new StringBuilder ();

            foreach (var entry in AttrMap) {
                if ((Flags & entry.Value) != 0) {
                    buf.Append (entry.Key);
                }
            }

            foreach (var entry in ColorMap) {
                if (Value != entry.Value)
                    continue;

                if ((Flags & ColorFlags.Bright) != 0) {
                    buf.Append (char.ToUpper (entry.Key));
                } else {
                    buf.Append (entry.Key);
                }
                break;
            }

            return buf.ToString ();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            var buf = new StringBuilder ();

            buf.Append (Escape);
            buf.Append ("[");

            if ((Flags & ColorFlags.Bright) != 0) {
                buf.Append ("1;");
            } else {
                buf.Append ("0;");
            }

            if ((Flags & ColorFlags.Blink) != 0) {
                buf.Append ("5;");
            }

            if ((Flags & ColorFlags.Dim) != 0) {
                buf.Append ("2;");
            }

            if ((Flags & ColorFlags.Reverse) != 0) {
                buf.Append ("7;");
            }

            if ((Flags & ColorFlags.Underscore) != 0) {
                buf.Append ("4;");
            }

            if (Value != ColorType.None) {
                var temp = (byte)Value;

                if ((Flags & ColorFlags.Background) != 0) {
                    temp += 10;
                }
                buf.Append (temp);
            } else {
                buf.Append ("0");
            }

            buf.Append ("m");

            return buf.ToString ();
        }

        #endregion Methods
    }

    /// <summary>
    /// Color writer.
    /// </summary>
    public class ColorWriter : StreamWriter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.ColorWriter"/> class.
        /// </summary>
        /// <param name='stream'>
        /// Stream.
        /// </param>
        /// <param name='conn'>
        /// Conn.
        /// </param>
        public ColorWriter(Stream stream, IConnection conn)
            : base(stream)
        {
            Connection = conn;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public IConnection Connection
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Converts the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public string Convert(string text)
        {
            if (text == null)
                return "";

            var index = text.IndexOf (Color.Code, 0);

            if (index == -1)
                return text;

            var lastIndex = 0;

            var buf = new StringBuilder ();

            while (index != -1) {
                buf.Append (text.Substring (lastIndex, index - lastIndex));

                // increment, and check for trailing codes
                if (++index >= text.Length) {
                    buf.Append (Color.Code);
                    break;
                }

                if (!Connection.Flags.HasFlag (ConnectionFlag.ColorOff)) {
                    switch (text [index]) {
                    case '*':
                        buf.Append ("\007");
                        break;
                    case Color.Code:
                        buf.Append (Color.Code);
                        break;
                    case 'x':
                        buf.Append (new Color ());
                        break;
                    default:
                        buf.Append (Color.Parse (text, ref index));
                        break;
                    }
                }

                lastIndex = ++index;
                index = text.IndexOf (Color.Code, index);
            }

            buf.Append (text.Substring (lastIndex));

            return buf.ToString ();
        }

        /// <summary>
        /// Convert the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public char[] Convert(char[] buffer)
        {
            var str = Convert (new string (buffer));

            return str.ToCharArray ();
        }

        /// <summary>
        /// Write the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public override void Write(char[] buffer)
        {
            base.Write (Convert (buffer));
        }

        /// <summary>
        /// Write the specified buffer, index and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='index'>
        /// Index.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override void Write(char[] buffer, int index, int count)
        {
            base.Write (Convert (buffer), index, count);
        }

        /// <summary>
        /// Write the specified value.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        public override void Write(object value)
        {
            Write (value.ToString ());
        }

        /// <summary>
        /// Write the specified format and arg0.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        public override void Write(string format, object arg0)
        {
            Write (string.Format (format, arg0));
        }

        /// <summary>
        /// Write the specified format, arg0, arg1 and arg2.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        /// <param name='arg1'>
        /// Arg1.
        /// </param>
        /// <param name='arg2'>
        /// Arg2.
        /// </param>
        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            Write (string.Format (format, arg0, arg1, arg2));
        }

        /// <summary>
        /// Write the specified format, arg0 and arg1.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        /// <param name='arg1'>
        /// Arg1.
        /// </param>
        public override void Write(string format, object arg0, object arg1)
        {
            Write (string.Format (format, arg0, arg1));
        }

        /// <summary>
        /// Write the specified format and arg.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg'>
        /// Argument.
        /// </param>
        public override void Write(string format, params object[] arg)
        {
            Write (string.Format (format, arg));
        }

        /// <summary>
        /// Write the specified value.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        public override void Write(string value)
        {
            base.Write (Convert (value));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public override void WriteLine(char[] buffer)
        {
            base.WriteLine (Convert (buffer));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='index'>
        /// Index.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override void WriteLine(char[] buffer, int index, int count)
        {
            base.WriteLine (Convert (buffer), index, count);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        public override void WriteLine(object value)
        {
            WriteLine (value.ToString ());
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        public override void WriteLine(string format, object arg0)
        {
            WriteLine (string.Format (format, arg0));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        /// <param name='arg1'>
        /// Arg1.
        /// </param>
        public override void WriteLine(string format, object arg0, object arg1)
        {
            WriteLine (string.Format (format, arg0, arg1));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg0'>
        /// Arg0.
        /// </param>
        /// <param name='arg1'>
        /// Arg1.
        /// </param>
        /// <param name='arg2'>
        /// Arg2.
        /// </param>
        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            WriteLine (string.Format (format, arg0, arg1, arg2));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='arg'>
        /// Argument.
        /// </param>
        public override void WriteLine(string format, params object[] arg)
        {
            WriteLine (string.Format (format, arg));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        public override void WriteLine(string value)
        {
            base.WriteLine (Convert (value));
        }

        #endregion Methods
    }
}
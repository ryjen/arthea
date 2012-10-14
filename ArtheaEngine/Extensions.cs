using ArtheaEngine.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArtheaEngine
{

    public static class Extensions
    {
        #region Fields

        private static readonly string ColorRegex = Color.Code +
            "[!@#$%]*[GRCBYMDWNgrcbymdwxn?*" + Color.Code + "]";

        #endregion Fields

        #region Methods

        /// <summary>
        /// Adds all.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        /// <param name='arg'>
        /// Argument.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static void AddAll<T>(this ObservableCollection<T> value, IEnumerable<T> arg)
        {
            foreach (var i in arg) {
                value.Add(i);
            }
        }

        /// <summary>
        /// Adds the changed handler.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        /// <param name='action'>
        /// Action.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static void AddChangedHandler<T>(this ObservableCollection<T> value, Action<T,bool> action)
        {
            value.CollectionChanged += new NotifyCollectionChangedEventHandler((o, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (T r in e.NewItems)
                    {
                        action(r, true);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (T r in e.OldItems)
                    {
                        action(r, false);
                    }
                }
            });
        }

        /// <summary>
        /// Makes the readable.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string CamelToHuman(this string str)
        {
            var buf = new StringBuilder();
            var c = str.GetEnumerator();

            if (!c.MoveNext())
                return str;

            buf.Append(char.ToUpper(c.Current));

            while (c.MoveNext())
            {
                if (char.IsUpper(c.Current))
                {
                    buf.Append(" ");
                }
                buf.Append(c.Current);
            }

            return buf.ToString();
        }

        /// <summary>
        /// Puts the first letter in upper case and the rest in lower case
        /// </summary>
        /// <returns>a captialized string</returns>
        public static string Capitalize(this string mString, bool tolower)
        {
            if (string.IsNullOrEmpty(mString))
            {
                return mString;
            }

            var firstLetter = !char.IsUpper(mString[0]) ? char.ToUpper(mString[0]) : mString[0];

            if (tolower)
                return firstLetter + mString.Substring(1).ToLower();

            return firstLetter + mString.Substring(1);
        }

        public static string Capitalize(this string mString)
        {
            return mString.Capitalize(true);
        }

        public static string Center(this string mString, int width)
        {
            var margin = (width/2.0f) - (mString.DisplayLength()/2.0f);
            var buf = new StringBuilder();
            buf.Append(' ', (int) Math.Floor(margin));
            buf.Append(mString);
            buf.Append(' ', (int) Math.Floor(margin));
            return buf.ToString();
        }

        public static string Center(this string mString, Player ch)
        {
            return mString.Center(ch.Account.Connection.ScreenWidth);
        }

        public static int CountLines(this string mString)
        {
            return mString.CountLines(0);
        }

        public static int CountLines(this string mString, int pos)
        {
            if (string.IsNullOrEmpty(mString)) return 0;

            int point = mString.IndexOf(Environment.NewLine);

            int count = 0;

            while (point > 0)
            {
                if (point >= pos)
                    break;

                count++;

                point = mString.IndexOf(Environment.NewLine, point);
            }

            return count;
        }

        public static bool DeepCompare<T>(this T[] orig, T[] passwd)
            where T : IEquatable<T>
        {
            if (orig.Length != passwd.Length)
                return false;

            for (var i = 0; i < orig.Length; i++)
            {
                if (!orig[i].Equals(passwd[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Displaies the length.
        /// </summary>
        /// <returns>
        /// The length.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        public static int DisplayLength(this string mString)
        {
            int len = mString.Length;

            if(len == 0) return 0;

            var matches = Regex.Matches (mString, ColorRegex);

            foreach (Match m in matches) {
                len -= m.Length;
            }

            return len;
        }

        /// <summary>
        /// Get the specified list and pred.
        /// </summary>
        /// <param name='list'>
        /// List.
        /// </param>
        /// <param name='pred'>
        /// Pred.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static T Get<T>(this IEnumerable<T> list, Predicate<T> pred)
        {
            foreach (var t in list)
            {
                if (pred(t))
                    return t;
            }
            return default(T);
        }

        /// <summary>
        /// Gets the length of the line count.
        /// </summary>
        /// <returns>
        /// The line count length.
        /// </returns>
        /// <param name='str'>
        /// String.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public static int GetLineCountLength(this string str, int count)
        {
            return str.GetLineCountLength(0, count);
        }

        /// <summary>
        /// Gets the length of a number of lines.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="count">The number of lines to count.</param>
        /// <returns></returns>
        public static int GetLineCountLength(this string str, int pos, int count)
        {
            var length = str.IndexOf(Environment.NewLine, pos);

            if (length == -1)
                return str.Length - pos;

            var i = 0;
            var point = -1;

            while (point < str.Length)
            {
                point = length + Environment.NewLine.Length;

                if (++i >= count)
                    break;

                length = str.IndexOf(Environment.NewLine, point);

                if (length == -1)
                {
                    point += str.Length - point;
                    break;
                }
            }
            return point - pos;
        }

        /// <summary>
        /// Gets the lines.
        /// </summary>
        /// <returns></returns>
        public static string[] GetLines(this string mString)
        {
            if (mString == null) return new string[0];
            return mString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Finds an argument prefixed by a multiplier (7*argument)
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <param name="str">the string to operate on</param>
        /// <returns></returns>
        public static int GetMultiArg(this string str, out string arg)
        {
            return str.GetNumberedArg(out arg, '*');
        }

        /// <summary>
        /// Finds an argument prefixed by an index (7.argument)
        /// </summary>
        /// <param name="str">the string to operate on</param>
        /// <param name="arg">The arg.</param>
        /// <returns></returns>
        public static int GetNumberedArg(this string str, out string arg)
        {
            return str.GetNumberedArg(out arg, '.');
        }

        /// <summary>
        /// Determines if has flag the specified bits flag.
        /// </summary>
        /// <returns>
        /// <c>true</c> if has flag the specified bits flag; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='bits'>
        /// Bits.
        /// </param>
        /// <param name='flag'>
        /// Flag.
        /// </param>
        public static bool HasFlag(this long bits, long flag)
        {
            return (bits & flag) != 0;
        }

        /// <summary>
        /// Determines if has flag the specified bits flag.
        /// </summary>
        /// <returns>
        /// <c>true</c> if has flag the specified bits flag; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='bits'>
        /// Bits.
        /// </param>
        /// <param name='flag'>
        /// Flag.
        /// </param>
        public static bool HasFlag(this int bits, int flag)
        {
            return (bits & flag) != 0;
        }

        /// <summary>
        /// Determines if has word the specified value word.
        /// </summary>
        /// <returns>
        /// <c>true</c> if has word the specified value word; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='value'>
        /// Value.
        /// </param>
        /// <param name='word'>
        /// Word.
        /// </param>
        public static bool HasWord(this string value, string word)
        {
            var arg = new Argument(value);

            while (!arg.IsEmpty())
            {
                var check = arg.Next();

                if (check.Equals(word, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if is  mString other.
        /// </summary>
        /// <returns>
        /// <c>true</c> if is mString other; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        /// <param name='other'>
        /// Other.
        /// </param>
        public static bool Is(this string mString, string other)
        {
            return mString.Equals(other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified STR has a line.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns>
        /// 	<c>true</c> if the specified STR has a line; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLine(this string str)
        {
            return str.Length > 0 && str.EndsWith(Environment.NewLine);
        }

        /// <summary>
        /// Checks if a string is a number
        /// </summary>
        /// <returns>true if the string is a number</returns>
        public static bool IsNumber(this string mString)
        {
            if (string.IsNullOrEmpty(mString))
                return false;

            bool foundPoint = false;
            bool foundNeg = false;
            bool foundNum = false;

            for (var ch = mString.GetEnumerator(); ch.MoveNext(); )
            {
                if (ch.Current == '.')
                {
                    if (foundPoint) return false;

                    foundPoint = true;
                    continue;
                }

                if (ch.Current == '-')
                {
                    if (foundNeg || foundNum) return false;

                    foundNeg = true;
                    continue;
                }

                if (!char.IsDigit(ch.Current))
                    return false;

                foundNum = true;
            }
            return true;
        }

        /// <summary>
        /// Determines if is prefix of the specified mString value ignoreCase.
        /// </summary>
        /// <returns>
        /// <c>true</c> if is prefix of the specified mString value ignoreCase; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        /// <param name='ignoreCase'>
        /// If set to <c>true</c> ignore case.
        /// </param>
        public static bool IsPrefixOf(this string mString, string value, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(mString) || string.IsNullOrEmpty(value)) return false;

            return value.StartsWith(mString, ignoreCase, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Determines if is prefix of the specified mString value.
        /// </summary>
        /// <returns>
        /// <c>true</c> if is prefix of the specified mString value; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        public static bool IsPrefixOf(this string mString, string value)
        {
            return IsPrefixOf(mString, value, true);
        }

        /// <summary>
        /// Lookup the specified arg and id.
        /// </summary>
        /// <param name='arg'>
        /// Argument.
        /// </param>
        /// <param name='id'>
        /// Identifier.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static T Lookup<T>(this IEnumerable<T> arg, string id)
            where T : IEntity
        {
            return arg.FirstOrDefault (v => v.Id != null && v.Id.Equals (id));
        }

        /// <summary>
        /// Reverse the specified dir.
        /// </summary>
        /// <param name='dir'>
        /// Dir.
        /// </param>
        public static Direction Reverse(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Down: return Direction.Up;
                case Direction.East: return Direction.West;
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.Up: return Direction.Down;
                case Direction.West: return Direction.East;

                default: throw new Exception("Unknown direction for reverse");

            }
        }

        /// <summary>
        /// Strips the color.
        /// </summary>
        /// <returns>
        /// The color.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        public static string StripColor(this string mString)
        {
            return Regex.Replace(mString, ColorRegex, string.Empty);
        }

        /// <summary>
        /// To comma-separated-values.
        /// </summary>
        /// <returns>
        /// The CSV list.
        /// </returns>
        /// <param name='Tags'>
        /// Tags.
        /// </param>
        public static string ToCSV(this ISet<string> Tags)
        {
            var buf = new StringBuilder();

            foreach (var tag in Tags)
            {
                buf.Append(tag).Append(", ");
            }
            if (buf.Length >= 2)
                buf.Remove(buf.Length-2, 2);

            return buf.ToString();
        }

        /// <summary>
        /// Tos the flag.
        /// </summary>
        /// <returns>
        /// The flag.
        /// </returns>
        /// <param name='loc'>
        /// Location.
        /// </param>
        public static WearFlag ToFlag(this WearLocation loc)
        {
            /*switch (loc)
            {
                case WearLocation.Arms: return WearFlag.Arms;
            }*/
            return WearFlag.None;
        }

        /// <summary>
        /// Gets the numbered argument.
        /// </summary>
        /// <returns>
        /// The numbered argument.
        /// </returns>
        /// <param name='mString'>
        /// M string.
        /// </param>
        /// <param name='arg'>
        /// Argument.
        /// </param>
        /// <param name='c'>
        /// C.
        /// </param>
        private static int GetNumberedArg(this string mString, out string arg, char c)
        {
            var index = mString.IndexOf(c);

            if (index == -1)
            {
                arg = mString;
                return 1;
            }

            int number;

            int.TryParse(mString.Substring(0, index), out number);

            arg = mString.Substring(index + 1);

            return number;
        }

        #endregion Methods
    }
}
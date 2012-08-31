using MongoDB.Bson;

namespace ArtheaEngine
{
    using System;
	using System.Collections.Generic;

	public class Argument
	{
        #region Fields

		public static Argument Empty = new Argument ();
		private string _mString;

        #endregion Fields

        #region Constructors

		public Argument (object arg)
		{
			_mString = arg.ToString ();
		}

		private Argument ()
		{
		}

        #endregion Constructors

        #region Properties

		public int Length {
			get { return _mString.Length; }
		}

        #endregion Properties

        #region Indexers

		public char this [int index] {
			get { return _mString [index]; }
		}

        #endregion Indexers

        #region Methods

		/// <summary>
		/// Performs an explicit conversion from <see cref="Arthea.String"/> to <see cref="System.String"/>.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator string (Argument x)
		{
			return x._mString;
		}

		public static implicit operator BsonValue (Argument x)
		{
			return x._mString;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Arthea.String"/>.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Argument (string x)
		{
			return new Argument (x);
		}

		/// <summary>
		/// Checks if a string is empty
		/// </summary>
		/// <param name="str">the string to check</param>
		/// <returns>true if string is empty</returns>
		public static bool operator ! (Argument str)
		{
			return str == null || str.IsEmpty ();
		}

		/// <summary>
		/// Check if two string are not equal (case insensitive)
		/// </summary>
		/// <param name="orig">first string</param>
		/// <param name="str">second string</param>
		/// <returns>true if strings are not equal</returns>
		public static bool operator != (Argument orig, string str)
		{
			return !orig._mString.Equals (str, StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// Checks if two strings are equal (case insensitive)
		/// </summary>
		/// <param name="orig">first string</param>
		/// <param name="str">second string</param>
		/// <returns>true if strings are equal</returns>
		public static bool operator == (Argument orig, string str)
		{
			return orig._mString.Equals (str, StringComparison.CurrentCultureIgnoreCase);
		}

		public string[] All ()
		{
			var args = new List<string> ();
			var temp = new Argument (_mString);

			while (!string.IsNullOrEmpty(temp)) {
				args.Add (temp.Next ());
			}

			return args.ToArray ();
		}

		public bool EndsWith (string part)
		{
			return _mString.EndsWith (part, StringComparison.CurrentCultureIgnoreCase);
		}

		public bool Equals (Argument obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			return Equals (obj._mString, _mString);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(Argument))
				return false;
			return Equals ((Argument)obj);
		}

		public override int GetHashCode ()
		{
			return (_mString != null ? _mString.GetHashCode () : 0);
		}

		public bool IsEmpty ()
		{
			return string.IsNullOrEmpty (_mString);
		}

		public bool IsNumber ()
		{
			return _mString.IsNumber ();
		}

		/// <summary>
		/// Gets the first arg of this string.
		/// Will reassign this string.
		/// </summary>
		/// <returns>a string</returns>
		public string Next ()
		{
			return Next (' ', true);
		}

		public string Next (char delimiter)
		{
			return Next (delimiter, true);
		}

		public string Peek ()
		{
			return Next (' ', false);
		}

		public Argument Replace (string orig, string value)
		{
			return _mString.Replace (orig, value);
		}

		public override string ToString ()
		{
			return _mString;
		}

		/// <summary>
		/// Gets the first argument in a string
		/// Delimited by a space, '', "", or ()
		/// </summary>
		/// <param name="update">true if this string should be updated</param>
		/// <returns>the rest of the string</returns>
		private string Next (char delimiter, bool update)
		{
			string arg;

			if (string.IsNullOrEmpty (_mString)) {
				return _mString;
			}

			var cEnd = delimiter;
			int pos, begin = 0;

			if (_mString [0] == '\'' || _mString [0] == '"' || _mString [0] == '(') {
				cEnd = _mString [0] == '(' ? ')' : _mString [0];
				begin = 1;
			}

			if ((pos = _mString.IndexOf (cEnd, begin)) == -1) {
				if (begin != 0) {
					arg = _mString [0].ToString ();
					if (update) {
						_mString = _mString.Substring (begin).Trim ();
					}
				} else {
					arg = _mString;
					if (update) {
						_mString = string.Empty;
					}
				}
			} else {
				arg = _mString.Substring (begin, pos).Trim ();

				if (update) {
					_mString = _mString.Substring (pos + 1).Trim ();
				}
			}
			return arg;
		}

        #endregion Methods
	}
}
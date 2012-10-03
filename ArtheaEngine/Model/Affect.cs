using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Reflection;
using System.Text;


namespace ArtheaEngine.Model
{

    #region Enumerations

	/// <summary>
	/// Affect flag.
	/// </summary>
    public enum AffectFlag : long
    {
        None
    }

	/// <summary>
	/// Affect location.
	/// </summary>
    public enum AffectLocation
    {
        None,
        AC,
        Hit,
        Mana,
        Move
    }

    #endregion Enumerations

	/// <summary>
	/// Affect.
	/// </summary>
    public class Affect : IEntity, IFormattable
    {
		public Affect()
		{	
			Id = ObjectId.GenerateNewId().ToString();
		}
        #region Properties

		/// <summary>
		/// Gets or sets the duration.
		/// </summary>
		/// <value>
		/// The duration.
		/// </value>
        public int Duration
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the flag.
		/// </summary>
		/// <value>
		/// The flag.
		/// </value>
        public AffectFlag Flag
        {
            get; set;
        }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[BsonId]
        public string Id
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>
		/// The location.
		/// </value>
        public AffectLocation Location
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the modifier.
		/// </summary>
		/// <value>
		/// The modifier.
		/// </value>
        public int Modifier
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the extra.
		/// </summary>
		/// <value>
		/// The extra.
		/// </value>
		public string Extra {
			get;
			set;
		}

        #endregion Properties

        #region Methods

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ArtheaEngine.Model.Affect"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="System.Object"/> to compare with the current <see cref="ArtheaEngine.Model.Affect"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Affect"/>; otherwise, <c>false</c>.
		/// </returns>
        public override bool Equals(object obj)
        {
            var a = obj as Affect;
            if (a == null)
            {
                return false;
            }
            return Flag == a.Flag && Modifier == a.Modifier && Location == a.Location;
        }

		/// <summary>
		/// Serves as a hash function for a <see cref="ArtheaEngine.Model.Affect"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
		/// </returns>
        public override int GetHashCode()
        {
            return ((int)Flag) + Modifier + Duration + (int)Location;
        }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Model.Affect"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Model.Affect"/>.
		/// </returns>
        public override string ToString()
        {
            string temp;

            if (Location != AffectLocation.None)
                temp = string.Format(" modifies {0} by {1}", Location.ToString().ToLower(), Modifier);
            else if (Flag != AffectFlag.None)
                temp = string.Format(" adds {0} affect", Flag.ToString());
            else
                temp = string.Empty;

            return string.Format("Spell {0}{1} for {2} hours.",
                                 (Extra == null ? "unknown" : Ability.Lookup(Extra).Name), temp, Duration);
        }

		/// <summary>
		/// Tos the string.
		/// </summary>
		/// <returns>
		/// The string.
		/// </returns>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='formatProvider'>
		/// Format provider.
		/// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || format == "F")
            {
				Ability ability = Ability.Lookup(Extra);

                var buf = new StringBuilder();
                buf.Append("Affects ");
                if(Flag != AffectFlag.None)
                    buf.Append(Flag);
                else if(ability != null)
                    buf.Append(ability.Name);
                else
                    buf.Append("unknown");
                if(Location != AffectLocation.None)
                {
                    buf.Append(", modifies").Append(Location);
                    buf.Append(" by ").Append(Modifier);
                }
                if(Duration == -1)
                    buf.Append("permanently");
                else
                    buf.AppendFormat("for {0} hours", Duration);

                return buf.ToString();
            }

            var prop = GetType().GetProperty(format, BindingFlags.Public | BindingFlags.Instance
                                                     | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                return prop.GetValue(this, null).ToString();
            }

            throw new FormatException("unknown format");
        }

        #endregion Methods
    }
}
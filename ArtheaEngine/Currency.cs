namespace ArtheaEngine
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Currency.
    /// </summary>
    public class Currency : IEquatable<Currency>
    {
        #region Fields

        uint _silver, _gold; // similar to dollars and cents

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Currency"/> class.
        /// </summary>
        /// <param name='copper'>
        /// Copper.
        /// </param>
        /// <param name='silver'>
        /// Silver.
        /// </param>
        /// <param name='gold'>
        /// Gold.
        /// </param>
        public Currency(uint silver, uint gold)
        {
            Gold = gold;
            Silver = silver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Currency"/> class.
        /// </summary>
        /// <param name='silver'>
        /// Silver.
        /// </param>
        //public Currency(uint silver) {
        //    Silver = silver;
        //}
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Currency"/> class.
        /// </summary>
        /// <param name='money'>
        /// Money.
        /// </param>
        public Currency(Currency money)
        {
            Gold = money.Gold;
            Silver = money.Silver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Currency"/> class.
        /// </summary>
        public Currency()
        {
        }

        public Currency(ulong value)
        {
            Gold = Convert.ToUInt32(value / 100);
            Silver = Convert.ToUInt32(value % 100);
        }

        public Currency(decimal value)
        {
            Gold = (uint) value;

            Silver = (uint)((value - Gold) * 100);
        }

        public Currency(double value)
        {
            Gold = (uint)value;

            _silver = (uint)((value - Gold) * 100);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the copper value (1/10 silver).
        /// </summary>
        /// <value>
        /// The copper.
        /// </value>
        public uint Copper
        {
            get { return Silver % 10; }
        }

        /// <summary>
        /// Gets or sets the gold value.
        /// </summary>
        /// <value>
        /// The gold value
        /// </value>
        public uint Gold
        {
            get { return _gold; }
            set { _gold = value; }
        }

        /// <summary>
        /// Gets or sets the platinum value.
        /// </summary>
        /// <value>
        /// The platinum value.
        /// </value>
        public uint Platinum
        {
            get {
                return Gold / 1000;
            }
        }

        /// <summary>
        /// Gets or sets the silver.
        /// </summary>
        /// <value>
        /// The silver.
        /// </value>
        public uint Silver
        {
            get { return _silver; }

            set {
                while (value >= 100) {
                    Gold++;
                    value -= 100;
                }
                _silver = value;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public ulong Value
        {
            get {
                return Gold * 100UL + Silver;
            }
        }

        #endregion Properties

        #region Methods

        /// <param name='c'>
        /// C.
        /// </param>
        public static implicit operator ulong(Currency c)
        {
            return c.Value;
        }

        public static implicit operator decimal(Currency c)
        {
            return c.Value / 100M;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        public static bool operator !(Currency a)
        {
            return a.Value == 0;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator !=(Currency a, Currency b)
        {
            return a.Value != b.Value;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static Currency operator *(Currency a, uint b)
        {
            return new Currency(a.Value * b);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static Currency operator +(Currency a, Currency b)
        {
            return new Currency(a.Value + b.Value);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        public static Currency operator ++(Currency a)
        {
            return new Currency(a.Value + 1);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static Currency operator -(Currency a, Currency b)
        {
            return new Currency(a.Value - b.Value);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        public static Currency operator --(Currency a)
        {
            return new Currency(a.Value - 1);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static Currency operator /(Currency a, uint b)
        {
            return new Currency(a.Value / b);
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator <(Currency a, Currency b)
        {
            return a.Value < b.Value;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator <=(Currency a, Currency b)
        {
            return a.Value <= b.Value;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator ==(Currency a, Currency b)
        {
            return a.Value == b.Value;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator >(Currency a, Currency b)
        {
            return a.Value > b.Value;
        }

        /// <param name='a'>
        /// The alpha component.
        /// </param>
        /// <param name='b'>
        /// The blue component.
        /// </param>
        public static bool operator >=(Currency a, Currency b)
        {
            return a.Value >= b.Value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ArtheaEngine.Currency"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="ArtheaEngine.Currency"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="ArtheaEngine.Currency"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Currency) {
                return Value.Equals ((obj as Currency).Value);
            }

            try {
                return Value.Equals (Convert.ToUInt64 (obj));
            } catch {
                return base.Equals (obj);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="ArtheaEngine.Currency"/> is equal to the current <see cref="ArtheaEngine.Currency"/>.
        /// </summary>
        /// <param name='other'>
        /// The <see cref="ArtheaEngine.Currency"/> to compare with the current <see cref="ArtheaEngine.Currency"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ArtheaEngine.Currency"/> is equal to the current
        /// <see cref="ArtheaEngine.Currency"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Currency other)
        {
            return Value.Equals(other.Value);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="ArtheaEngine.Currency"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </returns>
        /// <param name='format'>
        /// Format.
        /// </param>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Currency"/>.
        /// </returns>
        /// <param name='format'>
        /// the format of the string
        /// </param>
        /// <param name='formatProvider'>
        /// Format provider.
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = "G";

            switch (format.ToUpper())
            {
                case "P":
                    return (Gold / 1000M).ToString ("C");
                case "G":
                    return (Gold + (Silver / 100M)).ToString("C");
                case "S":
                    return (Silver/100M).ToString("C");
                case "C":
                    return (Copper/100M).ToString("C");
                default:
                    return Value.ToString(format);
            }
        }

        #endregion Methods
    }
}
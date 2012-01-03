namespace ArtheaEngine
{
    using System;
    using System.Globalization;

    #region Enumerations

    public enum Currency
    {
        Copper,
        Silver,
        Gold
    }

    #endregion Enumerations

    public class Money : IEquatable<Money>, IEquatable<decimal>
    {
        #region Fields

        private decimal Value;

        #endregion Fields

        #region Constructors

        public Money(decimal value)
        {
            Value = Math.Round(value, 2);
        }

        public Money()
        {
        }

        #endregion Constructors

        #region Methods

        public static implicit operator decimal(Money a)
        {
            return a.Value;
        }

        public static implicit operator Money(decimal a)
        {
            return new Money(a);
        }

        public static bool operator !(Money a)
        {
            return a.Value == 0;
        }

        public static bool operator !=(Money a, Money b)
        {
            return a.Value != b.Value;
        }

        public static Money operator *(Money a, Money b)
        {
            return new Money(a.Value * b.Value);
        }

        public static Money operator +(Money a, Money b)
        {
            return new Money(a.Value + b.Value);
        }

        public static Money operator ++(Money a)
        {
            return new Money(a.Value + 0.01m);
        }

        public static Money operator -(Money a, Money b)
        {
            return new Money(a.Value - b.Value);
        }

        public static Money operator -(Money a)
        {
            return new Money(-a.Value);
        }

        public static Money operator --(Money a)
        {
            return new Money(a.Value - 0.01m);
        }

        public static Money operator /(Money a, Money b)
        {
            return new Money(a.Value / b.Value);
        }

        public static bool operator <(Money a, Money b)
        {
            return a.Value < b.Value;
        }

        public static bool operator <=(Money a, Money b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator ==(Money a, Money b)
        {
            return a.Value == b.Value;
        }

        public static bool operator >(Money a, Money b)
        {
            return a.Value > b.Value;
        }

        public static bool operator >=(Money a, Money b)
        {
            return a.Value >= b.Value;
        }

        public Money Copper()
        {
            decimal dTmp;

            // subtract the gold
            if (Value >= 0)
            {
                dTmp = Value - Math.Truncate(Value);
            } else {
                dTmp = Value + Math.Truncate(Value);
            }

            // subtract the silver
            dTmp *= 10;

            dTmp -= Math.Truncate(dTmp);

            // get the copper
            dTmp *= 10;

            return dTmp;
        }

        public override bool Equals(object obj)
        {
            if (obj is Money)
            {
                return Value.Equals((obj as Money).Value);
            }
            if (obj is decimal)
            {
                return Value.Equals((decimal)obj);
            }
            return base.Equals(obj);
        }

        public bool Equals(decimal other)
        {
            return Value.Equals(other);
        }

        public bool Equals(Money other)
        {
            return Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public Money Gold()
        {
            return Math.Truncate(Value);
        }

        public Money Silver()
        {
            decimal dTmp;

            // subtract the gold
            if (Value >= 0)
            {
                dTmp = Value - Math.Truncate(Value);
            }
            else
            {
                dTmp = Value + Math.Truncate(Value);
            }

            // get the silver
            dTmp *= 100;

            return dTmp;
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = "G";

            switch (format.ToUpper())
            {
                case "G":
                    return Value.ToString("C");
                case "S":
                    return ValueAs(Currency.Silver).Value.ToString("C1");
                case "C":
                    return ValueAs(Currency.Copper).Value.ToString("C0");
                default:
                    return Value.ToString(format);
            }
        }

        /// <summary>
        /// Values as.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Money ValueAs(Currency type)
        {
            switch (type)
            {
                default:
                case Currency.Gold:
                    return Value;
                case Currency.Silver:
                    return Value * 10;
                case Currency.Copper:
                    return Value * 100;
            }
        }

        #endregion Methods
    }
}
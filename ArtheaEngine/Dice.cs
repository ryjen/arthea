using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ArtheaEngine
{
    /// <summary>
    /// Dice.
    /// </summary>
    [BsonSerializer(typeof(DiceSerializer))]
    public class Dice : IEnumerable<Die>
    {
        #region Fields

        Die[] _dice;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Dice"/> class.
        /// </summary>
        /// <param name='count'>
        /// Count.
        /// </param>
        public Dice(int count)
        {
            _dice = new Die[count];
            for (int i = 0; i < count; i++)
                _dice [i] = new Die ();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Dice"/> class.
        /// </summary>
        /// <param name='count'>
        /// Count.
        /// </param>
        /// <param name='sides'>
        /// Sides.
        /// </param>
        public Dice(int count, int sides)
        {
            _dice = new Die[count];
            for(int i = 0; i < count; i++)
                _dice[i] = new Die(sides);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Dice"/> class.
        /// </summary>
        /// <param name='dice'>
        /// Dice.
        /// </param>
        public Dice(params Die[] dice)
        {
            _dice = new Die[dice.Length];
            for(int i = 0; i < dice.Length; i++)
                _dice[i] = dice[i].Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Dice"/> class.
        /// </summary>
        /// <param name='dice'>
        /// Dice.
        /// </param>
        internal Dice(IEnumerable<Die> dice)
        {
            _dice = new Die[dice.Count ()];
            int i = 0;
            foreach (var d in dice) {
                _dice [i++] = d;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the bonus.
        /// </summary>
        /// <value>
        /// The bonus.
        /// </value>
        public int Bonus
        {
            get; set;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get {
                return _dice.Length;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is synchronized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
        /// </value>
        public bool IsSynchronized
        {
            get {
                return _dice.IsSynchronized;
            }
        }

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>
        /// The sync root.
        /// </value>
        public object SyncRoot
        {
            get {
                return _dice.SyncRoot;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name='to'>
        /// To.
        /// </param>
        /// <param name='index'>
        /// Index.
        /// </param>
        public void CopyTo(Array to, long index)
        {
            _dice.CopyTo(to, index);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        public IEnumerator<Die> GetEnumerator()
        {
            foreach (Die d in _dice) //this does not work because _list is private
                yield return d;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dice.GetEnumerator ();
        }

        /// <summary>
        /// Roll this instance.
        /// </summary>
        public int[] Roll()
        {
            int[] values = new int[Count];

            for (int index = 0; index < values.Length; index++)
            {
                values[index] = _dice[index].Roll();
            }
            return values;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Dice"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Dice"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format ("{0}+{1}", Count, Bonus);
        }

        #endregion Methods
    }

    /// <summary>
    /// Die.
    /// </summary>
    public class Die : ICloneable
    {
        #region Fields

        private static readonly Random rand = new Random();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Die"/> class.
        /// </summary>
        public Die()
            : this(6)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtheaEngine.Die"/> class.
        /// </summary>
        /// <param name='sides'>
        /// Sides.
        /// </param>
        public Die(int sides)
        {
            if (sides <= 0)
            {
                throw new ArgumentException("sides", "Dice must have one or more sides.");
            }

            Sides = sides;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the sides.
        /// </summary>
        /// <value>
        /// The sides.
        /// </value>
        public int Sides
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Clone this instance.
        /// </summary>
        public Die Clone()
        {
            return new Die(Sides);
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Roll this instance.
        /// </summary>
        public int Roll()
        {
            return rand.Next(1, Sides + 1);
        }

        #endregion Methods
    }

    /// <summary>
    /// Dice serializer.
    /// </summary>
    internal class DiceSerializer : IBsonSerializer
    {
        #region Fields

        EnumerableSerializer<Die> ser = new EnumerableSerializer<Die>();

        #endregion Fields

        #region Methods

        public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
            var e = (IEnumerable<Die>)ser.Deserialize(bsonReader, typeof(IEnumerable<Die>), options);
            return new Dice(e);
        }

        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
            var e = (IEnumerable<Die>) ser.Deserialize(bsonReader, nominalType, options);
            return new Dice(e);
        }

        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return ser.GetDefaultSerializationOptions();
        }

        public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
            ser.Serialize(bsonWriter, typeof(IEnumerable<Die>), value, options);
        }

        #endregion Methods
    }
}
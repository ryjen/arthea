using System.Collections;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ArtheaEngine
{

	[BsonSerializer(typeof(DiceSerializer))]
    public class Dice : IEnumerable<Die>
    {
		Die[] _dice;

        #region Constructors

		public Dice (int count)
		{
			_dice = new Die[count];
			for (int i = 0; i < count; i++)
				_dice [i] = new Die ();
		}

        public Dice(int count, int sides)
        {
			_dice = new Die[count];
			for(int i = 0; i < count; i++)
                _dice[i] = new Die(sides);
        }

        public Dice(params Die[] dice)
        {
			_dice = new Die[dice.Length];
			for(int i = 0; i < dice.Length; i++)
				_dice[i] = dice[i].Clone();
        }

		internal Dice (IEnumerable<Die> dice)
		{
			_dice = new Die[dice.Count ()];
			int i = 0;
			foreach (var d in dice) {
				_dice [i++] = d;
			}
		}

		public override string ToString ()
		{
			return string.Format ("{0}+{1}", Count, Bonus);
		}

        #endregion Constructors

        #region Properties

        public int Bonus
        {
            get; set;
        }

		public int Count {
			get {
				return _dice.Length;
			}
		}

		public bool IsSynchronized {
			get {
				return _dice.IsSynchronized;
			}
		}

		public object SyncRoot {
			get {
				return _dice.SyncRoot;
			}
		}

        #endregion Properties

        #region Methods

		public void CopyTo (Array to, long index)
		{
			_dice.CopyTo(to, index);
		}

		public IEnumerator<Die> GetEnumerator ()
		{
			foreach (Die d in _dice) //this does not work because _list is private
				yield return d;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _dice.GetEnumerator ();
		}

        public int[] Roll()
        {
            int[] values = new int[Count];

            for (int index = 0; index < values.Length; index++)
            {
                values[index] = _dice[index].Roll();
            }
            return values;
        }

		#endregion

    }

	public class DiceSerializer : IBsonSerializer
	{
		EnumerableSerializer<Die> ser = new EnumerableSerializer<Die>();

		public object Deserialize (BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
			var e = (IEnumerable<Die>)ser.Deserialize(bsonReader, typeof(IEnumerable<Die>), options);
			return new Dice(e);
		}
		
		public object Deserialize (BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options) {
			if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
			var e = (IEnumerable<Die>) ser.Deserialize(bsonReader, nominalType, options);
			return new Dice(e);
		}
		
		public IBsonSerializationOptions GetDefaultSerializationOptions () {
			return ser.GetDefaultSerializationOptions();
		}

		public void Serialize (BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options) {
			if (nominalType != typeof(Dice)) throw new ArgumentException("Cannot serialize anything but dice");
			ser.Serialize(bsonWriter, typeof(IEnumerable<Die>), value, options);
		}
	}

    public class Die : ICloneable
    {
        #region Fields

        private static readonly Random rand = new Random();

        #endregion Fields

        #region Constructors

		public Die () : this(6)
		{
		}

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

        public int Sides
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public int Roll()
        {
            return rand.Next(1, Sides + 1);
        }

		public Die Clone()
		{
			return new Die(Sides);
		}

		object ICloneable.Clone()   
		{
			return Clone();
		}

        #endregion Methods
    }
}
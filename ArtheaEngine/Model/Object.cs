using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;



namespace ArtheaEngine.Model
{

    #region Enumerations

    [Flags]
    public enum ObjectFlag : long
    {
        None
    }

    public enum ObjectType
    {
        Armor,
        Weapon,
        Furniture,
        Container,
        DrinkContainer,
        Staff,
        Wand,
        Potion,
        Pill,
        Scroll,
        Trash,
        Food,
        Key,
        Portal,
        Money
    }

    public enum WeaponClass
    {
        Exotic,
        Sword,
        Dagger,
        Spear,
        Mace,
        Axe,
        Flail,
        Whip,
        Polearm
    }

    [Flags]
    public enum WearFlag
    {
        None,
        Take
    }

    public enum WearLocation
    {
        None,
        Head,
        Neck,
        Shoulders,
        Arms,
        Hands,
        WristRight,
        WristLeft,
        Torso,
        Waist,
        Legs,
        Feet,
        Wielded,
        Holding,
        Back
    }

    #endregion Enumerations

    public class Object : INamedEntity, IRootLevelDocument, IDisposable, ICloneable
    {
        #region Constructors

        public Object()
        {
			Id = ObjectId.GenerateNewId().ToString();
			Key = new AreaKey();
            Values = new List<object>();
            Contents = new ObservableCollection<Object>();
            Contents.AddChangedHandler<Object>((o, a) => {
                o.Inside = a ? this : null;
            });
            Affects = new ObservableCollection<Affect>();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<Affect> Affects
        {
            get; set;
        }

		[BsonIgnore]
		public Area Area {
			get {
				return Key.Area;
			}
			set {
				Key.Area = value;
			}
        }

		[BsonIgnore]
        public Character CarriedBy
        {
            get; set;
        }

		[BsonDefaultValue(100f)]
		[BsonIgnoreIfDefault]
        public float Condition
        {
            get; set;
        }

		[BsonDefaultValue(WearLocation.None)]
		[BsonIgnoreIfDefaultAttribute]
		public WearLocation WearLocation
        {
            get; set;
        }

        public ObservableCollection<Object> Contents
        {
            get; set;
        }

		[BsonIgnoreIfDefault]
        public decimal Cost
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string Description
        {
            get; set;
        }

		[BsonDefaultValue(ObjectFlag.None)]
		[BsonIgnoreIfDefault]
        public ObjectFlag Flags
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonIgnore]
        public Object Inside
        {
            get; set;
        }

		[BsonIgnoreIfDefault]
        public short Level
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string LongDescr
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string Name
        {
            get; set;
        }

		[BsonIgnoreIfNull]
		[BsonElement("Room")]
		public string RoomRef {
			get { return Room == null ? null : Room.Id; }
			set { Room = Room.Lookup (value); }
		}

		[BsonIgnore]
        public Room Room
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string ShortDescr
        {
            get; set;
        }

		[BsonRequired]
		[BsonDefaultValue(ObjectType.Trash)]
        public ObjectType Type
        {
            get; set;
        }

		[BsonRequired]
		public AreaKey Key {
			get;
			set;
		}

        public IList<object> Values
        {
            get; private set;
        }

		[BsonDefaultValue(WearFlag.None)]
		[BsonIgnoreIfDefault]
        public WearFlag WearFlags
        {
            get; set;
        }

		[BsonIgnoreIfDefault]
        public float Weight
        {
            get; set;
        }

        #endregion Properties

        #region Methods
		
		public bool Save ()
		{
			return DbManager.Save (this);
		}
		
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}
		public static Object Lookup (string id)
		{
			return DbManager.Lookup<Object> (id);
		}

        public Object Clone()
        {
            return MemberwiseClone() as Object;
        }

        public virtual void Dispose()
        {
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "G";

            switch (format.ToUpper())
            {
                case "G":
                case "N":
                    return ShortDescr.ToString(formatProvider);
                case "L":
                    return Description.ToString(formatProvider);
                default:
                    throw new FormatException("Invalid format for object");
            }
        }

        #endregion Methods
    }
}
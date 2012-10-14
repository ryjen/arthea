using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;



namespace ArtheaEngine.Model
{

	/// <summary>
	/// Object flag.
	/// </summary>
    [Flags]
    public enum ObjectFlag : long
    {
        None
    }

	/// <summary>
	/// Object type.
	/// </summary>
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

	/// <summary>
	/// Weapon class.
	/// </summary>
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

	/// <summary>
	/// Wear flag.
	/// </summary>
    [Flags]
    public enum WearFlag
    {
        None,
        Take
    }

	/// <summary>
	/// Wear location.
	/// </summary>
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

	/// <summary>
	/// Object.
	/// </summary>
    public class Object : INamedEntity, IRootLevelDocument, IDisposable, ICloneable
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Object"/> class.
		/// </summary>
        public Object()
        {
			Id = ObjectId.GenerateNewId().ToString();
            Values = new List<object>();
            Contents = new ObservableCollection<Object>();
            Contents.AddChangedHandler<Object>((o, a) => {
                o.Inside = a ? this : null;
            });
            Affects = new ObservableCollection<Affect>();
        }

		/// <summary>
		/// Gets or sets the affects.
		/// </summary>
		/// <value>
		/// The affects.
		/// </value>
        public ObservableCollection<Affect> Affects
        {
            get; set;
        }
		/// <summary>
		/// Gets or sets the area.
		/// </summary>
		/// <value>
		/// The area.
		/// </value>
		[BsonIgnore]
		public Area Area
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the area reference.
		/// </summary>
		/// <value>
		/// The area reference.
		/// </value>
		[BsonRequired]
		[BsonElement("Area")]
		public string AreaRef
		{
			get { return Area == null ? null : Area.Id; }
			
			set { Area = Area.Lookup (value); }
		}
		/// <summary>
		/// Gets or sets the carried by.
		/// </summary>
		/// <value>
		/// The carried by.
		/// </value>
		[BsonIgnore]
        public Character CarriedBy
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the condition.
		/// </summary>
		/// <value>
		/// The condition.
		/// </value>
		[BsonDefaultValue(100f)]
		[BsonIgnoreIfDefault]
        public float Condition
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the wear location.
		/// </summary>
		/// <value>
		/// The wear location.
		/// </value>
		[BsonDefaultValue(WearLocation.None)]
		[BsonIgnoreIfDefaultAttribute]
		public WearLocation WearLocation
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the contents.
		/// </summary>
		/// <value>
		/// The contents.
		/// </value>
        public ObservableCollection<Object> Contents
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the cost.
		/// </summary>
		/// <value>
		/// The cost.
		/// </value>
		[BsonIgnoreIfDefault]
        public decimal Cost
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[BsonIgnoreIfNull]
        public string Description
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
		[BsonDefaultValue(ObjectFlag.None)]
		[BsonIgnoreIfDefault]
        public ObjectFlag Flags
        {
            get; set;
        }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the inside.
		/// </summary>
		/// <value>
		/// The inside.
		/// </value>
		[BsonIgnore]
        public Object Inside
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the level.
		/// </summary>
		/// <value>
		/// The level.
		/// </value>
		[BsonIgnoreIfDefault]
        public short Level
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the long descr.
		/// </summary>
		/// <value>
		/// The long descr.
		/// </value>
		[BsonIgnoreIfNull]
        public string LongDescr
        {
            get; set;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[BsonIgnoreIfNull]
        public string Name
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the room reference.
		/// </summary>
		/// <value>
		/// The room reference.
		/// </value>
		[BsonIgnoreIfNull]
		[BsonElement("Room")]
		public string RoomRef {
			get { return Room == null ? null : Room.Id; }
			set { Room = Room.Lookup (value); }
		}

		/// <summary>
		/// Gets or sets the room.
		/// </summary>
		/// <value>
		/// The room.
		/// </value>
		[BsonIgnore]
        public Room Room
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the short descr.
		/// </summary>
		/// <value>
		/// The short descr.
		/// </value>
		[BsonIgnoreIfNull]
        public string ShortDescr
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		[BsonRequired]
		[BsonDefaultValue(ObjectType.Trash)]
        public ObjectType Type
        {
            get; set;
        }

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
        public IList<object> Values
        {
            get; private set;
        }

		/// <summary>
		/// Gets or sets the wear flags.
		/// </summary>
		/// <value>
		/// The wear flags.
		/// </value>
		[BsonDefaultValue(WearFlag.None)]
		[BsonIgnoreIfDefault]
        public WearFlag WearFlags
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the weight.
		/// </summary>
		/// <value>
		/// The weight.
		/// </value>
		[BsonIgnoreIfDefault]
        public float Weight
        {
            get; set;
        }

		/// <summary>
		/// Save this instance.
		/// </summary>
		public bool Save ()
		{
			return DbManager.Save (this);
		}

		/// <summary>
		/// Delete this instance.
		/// </summary>
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}
		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public static Object Lookup (string id)
		{
			return DbManager.Lookup<Object> (id);
		}

		/// <summary>
		/// Clone this instance.
		/// </summary>
        public Object Clone()
        {
            return MemberwiseClone() as Object;
        }

		/// <summary>
		/// Releases all resource used by the <see cref="ArtheaEngine.Model.Object"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="ArtheaEngine.Model.Object"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="ArtheaEngine.Model.Object"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="ArtheaEngine.Model.Object"/> so
		/// the garbage collector can reclaim the memory that the <see cref="ArtheaEngine.Model.Object"/> was occupying.
		/// </remarks>
        public virtual void Dispose()
        {
        }

		/// <summary>
		/// Clone this instance.
		/// </summary>
        object ICloneable.Clone()
        {
            return this.Clone();
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
    }
}
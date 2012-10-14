using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArtheaEngine.Scripting;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Room flag.
	/// </summary>
    [Flags]
    public enum RoomFlag : long
    {
        None
    }

	/// <summary>
	/// Terrain.
	/// </summary>
    public enum Terrain
    {
        Inside,
        City,
        Field,
        Forest,
        Hills,
        Mountain,
        Water,
        Air,
        Desert,
        Jungle,
        Road,
        Path,
        Cave,
        Swamp
    }

	/// <summary>
	/// Room.
	/// </summary>
    public class Room : INamedEntity, IRootLevelDocument
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Room"/> class.
		/// </summary>
        public Room()
        {
			Id = ObjectId.GenerateNewId().ToString();
            Characters = new ObservableCollection<Character>();
            Characters.AddChangedHandler<Character>((c, a) =>
            {
                c.Room = a ? this : null;
            });
            Objects = new ObservableCollection<Object>();
            Objects.AddChangedHandler<Object>((o, a) =>
            {
                o.Room = a ? this : null;
            });
            Exits = new Dictionary<Direction, Exit>();
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
		/// Gets or sets the characters.
		/// </summary>
		/// <value>
		/// The characters.
		/// </value>
		[BsonIgnore]
        public ObservableCollection<Character> Characters
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
		/// Gets or sets the exits.
		/// </summary>
		/// <value>
		/// The exits.
		/// </value>
		[BsonDictionaryOptions(DictionaryRepresentation.Dynamic)]
        public Dictionary<Direction, Exit> Exits
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
		[BsonDefaultValue(RoomFlag.None)]
		[BsonIgnoreIfDefaultAttribute]
        public RoomFlag Flags
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
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[BsonRequired]
        public string Name
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the objects.
		/// </summary>
		/// <value>
		/// The objects.
		/// </value>
		[BsonIgnore]
        public ObservableCollection<Object> Objects
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the reset.
		/// </summary>
		/// <value>
		/// The reset.
		/// </value>
        public string Reset
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the terrain.
		/// </summary>
		/// <value>
		/// The terrain.
		/// </value>
        public Terrain Terrain
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
		/// Act the specified format and args.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public void Act(string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

		/// <summary>
		/// Act the specified skip, format and args.
		/// </summary>
		/// <param name='skip'>
		/// Skip.
		/// </param>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public void Act(Character skip, string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                if (ch == skip)
                    continue;

                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

		/// <summary>
		/// Act the specified skip, format and args.
		/// </summary>
		/// <param name='skip'>
		/// Skip.
		/// </param>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public void Act(ICollection<Character> skip, string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                if (skip.Contains(ch))
                    continue;

                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

		/// <summary>
		/// Determines whether the specified <see cref="ArtheaEngine.Model.Room"/> is equal to the current <see cref="ArtheaEngine.Model.Room"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="ArtheaEngine.Model.Room"/> to compare with the current <see cref="ArtheaEngine.Model.Room"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="ArtheaEngine.Model.Room"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Room"/>; otherwise, <c>false</c>.
		/// </returns>
        public virtual bool Equals(Room obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id.Equals(Id);
        }

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ArtheaEngine.Model.Room"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="System.Object"/> to compare with the current <see cref="ArtheaEngine.Model.Room"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Room"/>; otherwise, <c>false</c>.
		/// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Room)) return false;
            return Equals((Room) obj);
        }

		/// <summary>
		/// Serves as a hash function for a <see cref="ArtheaEngine.Model.Room"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
		/// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

		/// <summary>
		/// Tick this instance.
		/// </summary>
        public void Tick ()
		{
			if (string.IsNullOrEmpty(Reset)) {
				new ArtheaScript(new ResetScript(this)).Run(Reset);
			}
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
                    return Name.ToString(formatProvider);
                case "D":
                    return Description.ToString(formatProvider);
                default:
                    throw new FormatException("invalid room format");
            }
        }

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public static Room Lookup(string id)
		{
			return DbManager.Lookup<Room>(id);
		}
    }
}
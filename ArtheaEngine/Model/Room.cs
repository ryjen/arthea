using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;

namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;

    using NLog;

    #region Enumerations

    [Flags]
    public enum RoomFlag : long
    {
        None
    }

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

    #endregion Enumerations

    public class Room : INamedEntity, IRootLevelDocument
    {
        #region Constructors

        public Room()
        {
			Id = ObjectId.GenerateNewId().ToString();
			Key = new AreaKey();
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

        #endregion Constructors

        #region Properties

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
        public ObservableCollection<Character> Characters
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string Description
        {
            get; set;
        }

		[BsonDictionaryOptions(DictionaryRepresentation.Dynamic)]
        public Dictionary<Direction, Exit> Exits
        {
            get; set;
        }

		[BsonDefaultValue(RoomFlag.None)]
		[BsonIgnoreIfDefaultAttribute]
        public RoomFlag Flags
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonRequired]
        public string Name
        {
            get; set;
        }

		[BsonIgnore]
        public ObservableCollection<Object> Objects
        {
            get; set;
        }

		[BsonRequired]
		public AreaKey Key
		{
			get; set;
		}

        public Reset Reset
        {
            get; set;
        }

        public Terrain Terrain
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

        public void Act(string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

        public void Act(Character skip, string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                if (ch == skip)
                    continue;

                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

        public void Act(ICollection<Character> skip, string format, params object[] args)
        {
            foreach (var ch in Characters)
            {
                if (skip.Contains(ch))
                    continue;

                ch.WriteLine(string.Format(new ActionFormatter(ch), format, args));
            }
        }

        public virtual bool Equals(Room obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Room)) return false;
            return Equals((Room) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Tick()
        {
            if(Reset != null)
                Reset.Execute();
        }

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

		public static Room Lookup(string id)
		{
			return DbManager.Lookup<Room>(id);
		}

        #endregion Methods
    }
}
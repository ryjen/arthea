using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using NLog;

    #region Enumerations

    public enum Position
    {
        Dead,
        Dying,
        Stunned,
        Sleeping,
        Resting,
        Fighting,
        Standing
    }

    public enum Sex
    {
        Neutral,
        Male,
        Female,
        Hermaphrodite
    }

    public enum Size
    {
        Minuscule,
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Giant
    }

    #endregion Enumerations

    public class ActionFormatter : IFormatProvider, ICustomFormatter
    {
        #region Constructors

        public ActionFormatter(Character viewer)
        {
            Viewer = viewer;
        }

        #endregion Constructors

        #region Properties

        public Character Viewer
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            var ch = arg as Character;
            if (string.IsNullOrEmpty(format) || ch == null) return arg.ToString();
            return ch.ToString(format, formatProvider);
        }

        /// <summary>
        ///                     Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <returns>
        ///                     An instance of the object specified by <paramref name="formatType" />, if the <see cref="T:System.IFormatProvider" /> implementation can supply that type of object; otherwise, null.
        /// </returns>
        /// <param name="formatType">
        ///                     An object that specifies the type of format object to return. 
        ///                 </param><filterpriority>1</filterpriority>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        #endregion Methods
    }

	[BsonKnownTypes(typeof(NonPlayer), typeof(Player))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Character : INamedEntity, IRootLevelDocument, IWritable, IFormattable, IDisposable, ICloneable
    {
        #region Fields

        private static readonly string[] HeShe = { "it", "he", "she", "it" };
        private static readonly string[] HimHer = { "it", "him", "her", "it" };
        private static readonly string[] HisHer = { "its", "his", "her", "it" };
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public Character()
        {
			Id = ObjectId.GenerateNewId().ToString();
            Affects = new ObservableCollection<Affect>();
            Carrying = new ObservableCollection<Object>();
            Carrying.AddChangedHandler((o, n) => {
				o.CarriedBy = n ? this : null;
            });

            Attack = new Dice(2, 4);
            Defense = new Dice(2, 4);

            Vitals = new Vitals();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<Affect> Affects
        {
            get; set;
        }

        public Dice Attack
        {
            get; set;
        }

        public ObservableCollection<Object> Carrying
        {
            get; set;
        }

        public Dice Defense
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public Combat Fighting
        {
            get;
            set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
		{
			get; set;
        }

        public short Level
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public Position Position
        {
            get; set;
        }

        public Race Race
        {
            get; set;
        }

		[BsonElement("Room")]
		public string RoomRef {
			get { 
				return Room == null ? null : Room.Id; 
			}
			set {
				Room = Room.Lookup(value);
			}
		}

		[BsonIgnore]
        public Room Room
        {
            get; set;
        }

        public Sex Sex
        {
            get; set;
        }

        public float Size
        {
            get; set;
        }

        public Vitals Vitals
        {
            get; set;
        }

		[BsonIgnore]
        public abstract World World
        {
            get;
        }

        #endregion Properties

		public static Character Lookup (string id)
		{
			return DbManager.Lookup<Character>(id);
		}

		public static T Lookup<T> (string id) where T:Character
		{
			return DbManager.Lookup<T> (id);
		}

        #region Methods

        public void Act(string format, params object[] args)
        {
            WriteLine(string.Format(new ActionFormatter(this), format, args));
        }

        public virtual bool CanSee(Character who)
        {
            return true;
        }

        public Character Clone()
        {
            return MemberwiseClone() as Character;
        }

        /// <summary>
        ///                     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            if (Room != null)
            {
                Room.Characters.Remove(this);
                Room = null;
            }
        }

        public virtual bool Equals(Character obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Character)) return false;
            return Equals((Character)obj);
        }

        public void Equip (Object obj, WearLocation location)
		{
			if (obj.WearLocation != WearLocation.None) {
				Log.Warn ("Object {0} already equipped", obj);
				return;
			}
            if (!obj.WearFlags.HasFlag(location.ToFlag()))
            {
                Log.Warn("Cannot equip object {0} on {1}", obj, location);
                return;
            }

			obj.WearLocation = location;

            Carrying.Add(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

		/*
        public abstract bool Load(IDbConnection conn);

        public virtual int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Name = reader.GetString(i++);
            Description = reader.GetStringOrNull(i++);
            Level = reader.GetInt16(i++);

            Race = Race.Lookup(reader.GetInt32(i++));
            if (Race == null)
                Race = new UniqueRace();
            Size = reader.GetFloat(i++);
            Position = reader.GetEnum<Position>(i++);
            Sex = reader.GetEnum<Sex>(i++);

            return i;
        }


        public virtual bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_char");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@level", DbType.Int16, Level);
            cmd.AddParameter("@race", DbType.Int32, Race.Id);
            cmd.AddParameter("@size", DbType.Single, Size);
            cmd.AddParameter("@position", DbType.String, Position.ToString());
            cmd.AddParameter("@sex", DbType.String, Sex);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
                List.Add(this);
            }

            return res;
        }

		*/

        public abstract void Page(string buf);

        public override string ToString()
        {
            return Name;
        }

        public virtual string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || format == "N")
            {
                if (formatProvider is ActionFormatter)
                {
                    var viewer = ((ActionFormatter)formatProvider).Viewer;

                    if (!viewer.CanSee(this))
                        return "someone";
                }

                return Name.ToString(formatProvider);
            }

            var prop = GetType().GetProperty(format, BindingFlags.Public | BindingFlags.Instance
                                                     | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                return prop.GetValue(this, null).ToString();
            }

            switch (char.ToUpper(format[0]))
            {
                case 'M':
                    return HimHer[(int)Sex];
                case 'S':
                    return HisHer[(int)Sex];
                case 'E':
                    return HeShe[(int)Sex];
                default:
                    throw new FormatException("unable to locate property");
            }
        }

        public abstract void Write(string format, params object[] args);

        public abstract void Write(byte[] value);

        public abstract void Write(object obj);

        public abstract void WriteLine();

        public abstract void WriteLine(object obj);

        public abstract void WriteLine(string format, params object[] args);

        #endregion Methods
    }

    public class Vitals
    {
        #region Constructors

        public Vitals()
        {
            Life = MaxLife=100;
            Mana = MaxMana = 100;
            Energy = MaxEnergy = 100;
        }

        #endregion Constructors

        #region Properties

        public long Energy
        {
            get; set;
        }

        public long Life
        {
            get; set;
        }

        public long Mana
        {
            get; set;
        }

        public long MaxEnergy
        {
            get; set;
        }

        public long MaxLife
        {
            get; set;
        }

        public long MaxMana
        {
            get; set;
        }

        #endregion Properties
    }
}
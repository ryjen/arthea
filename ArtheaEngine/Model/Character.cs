using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ArtheaEngine.Model
{

	/// <summary>
	/// Position.
	/// </summary>
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

	/// <summary>
	/// Sex.
	/// </summary>
    public enum Sex
    {
        Neutral,
        Male,
        Female,
        Hermaphrodite
    }

	/// <summary>
	/// Size.
	/// </summary>
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

	/// <summary>
	/// Action formatter.
	/// </summary>
    public class ActionFormatter : IFormatProvider, ICustomFormatter
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.ActionFormatter"/> class.
		/// </summary>
		/// <param name='viewer'>
		/// Viewer.
		/// </param>
        public ActionFormatter(Character viewer)
        {
            Viewer = viewer;
        }
	
		/// <summary>
		/// Gets the viewer.
		/// </summary>
		/// <value>
		/// The viewer.
		/// </value>
        public Character Viewer
        {
            get; private set;
        }

		/// <Docs>
		/// To be added: an object of type 'string'
		/// </Docs>
		/// <param name='formatProvider'>
		/// To be added: an object of type 'IFormatProvider'
		/// </param>
		/// <returns>
		/// To be added: an object of type 'string'
		/// </returns>
		/// <summary>
		/// Format the specified format, arg and formatProvider.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='arg'>
		/// Argument.
		/// </param>
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
    }

	/// <summary>
	/// Character (an actor in the world)
	/// </summary>
	[BsonKnownTypes(typeof(NonPlayer), typeof(Player))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Character : INamedEntity, IRootLevelDocument, IWritable, IFormattable, IDisposable, ICloneable
    {
        private static readonly string[] HeShe = { "it", "he", "she", "it" };
        private static readonly string[] HimHer = { "it", "him", "her", "it" };
        private static readonly string[] HisHer = { "its", "his", "her", "it" };
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Character"/> class.
		/// </summary>
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
		/// Gets or sets the attack.
		/// </summary>
		/// <value>
		/// The attack.
		/// </value>
        public Dice Attack
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the carrying.
		/// </summary>
		/// <value>
		/// The carrying.
		/// </value>
        public ObservableCollection<Object> Carrying
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the defense.
		/// </summary>
		/// <value>
		/// The defense.
		/// </value>
        public Dice Defense
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
        public string Description
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the fighting.
		/// </summary>
		/// <value>
		/// The fighting.
		/// </value>
        public Combat Fighting
        {
            get;
            set;
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
		/// Gets or sets the level.
		/// </summary>
		/// <value>
		/// The level.
		/// </value>
        public short Level
        {
            get; set;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        public string Name
        {
            get; set;
        }

        public Position Position
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the race reference.
		/// </summary>
		/// <value>
		/// The race reference.
		/// </value>
		[BsonElement("Race")]
		public string RaceRef {
			get {
				return Race.Id;
			}
			set {
				Race = Race.Lookup (value);
			}
		}

		/// <summary>
		/// Gets or sets the race.
		/// </summary>
		/// <value>
		/// The race.
		/// </value>
		[BsonIgnore]
        public Race Race
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the room reference.
		/// </summary>
		/// <value>
		/// The room reference.
		/// </value>
		[BsonElement("Room")]
		public string RoomRef {
			get { 
				return Room == null ? null : Room.Id; 
			}
			set {
				Room = Room.Lookup(value);
			}
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
		/// Gets or sets the sex.
		/// </summary>
		/// <value>
		/// The sex.
		/// </value>
        public Sex Sex
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>
		/// The size.
		/// </value>
        public float Size
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the vitals.
		/// </summary>
		/// <value>
		/// The vitals.
		/// </value>
        public Vitals Vitals
        {
            get; set;
        }

		/// <summary>
		/// Gets the world.
		/// </summary>
		/// <value>
		/// The world.
		/// </value>
		[BsonIgnore]
        public abstract World World
        {
            get;
        }

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public static Character Lookup (string id)
		{
			return DbManager.Lookup<Character>(id);
		}

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static T Lookup<T> (string id) where T:Character
		{
			return DbManager.LookupAs<Character,T> (id);
		}

		/// <summary>
		/// Save this instance.
		/// </summary>
		public virtual bool Save ()
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
            WriteLine(string.Format(new ActionFormatter(this), format, args));
        }

		/// <summary>
		/// Determines whether this instance can see the specified who.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance can see the specified who; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='who'>
		/// Who.
		/// </param>
        public virtual bool CanSee(Character who)
        {
            return true;
        }

		/// <summary>
		/// Clone this instance.
		/// </summary>
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

		/// <summary>
		/// Determines whether the specified <see cref="ArtheaEngine.Model.Character"/> is equal to the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="ArtheaEngine.Model.Character"/> to compare with the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="ArtheaEngine.Model.Character"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Character"/>; otherwise, <c>false</c>.
		/// </returns>
        public virtual bool Equals(Character obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id.Equals(Id);
        }

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="System.Object"/> to compare with the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Character"/>; otherwise, <c>false</c>.
		/// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Character)) return false;
            return Equals((Character)obj);
        }

		/// <summary>
		/// Equip the specified obj and location.
		/// </summary>
		/// <param name='obj'>
		/// Object.
		/// </param>
		/// <param name='location'>
		/// Location.
		/// </param>
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

		/// <summary>
		/// Serves as a hash function for a <see cref="ArtheaEngine.Model.Character"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
		/// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

		/// <summary>
		/// Clone this instance.
		/// </summary>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
        public abstract void Page(string buf);

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		public abstract void Page(StringBuilder buf);

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.Model.Character"/>.
		/// </returns>
        public override string ToString()
        {
            return Name;
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
        public virtual string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
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

		/// <summary>
		/// Write the specified format and args.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public abstract void Write(string format, params object[] args);

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
        public abstract void Write(byte[] value);

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public abstract void Write(object obj);

		/// <summary>
		/// Writes the line.
		/// </summary>
        public abstract void WriteLine();

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public abstract void WriteLine(object obj);

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public abstract void WriteLine(string format, params object[] args);

    }

	/// <summary>
	/// Vitals.
	/// </summary>
    public class Vitals
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Vitals"/> class.
		/// </summary>
        public Vitals()
        {
            Life = MaxLife=100;
            Mana = MaxMana = 100;
            Energy = MaxEnergy = 100;
        }

		/// <summary>
		/// Gets or sets the energy.
		/// </summary>
		/// <value>
		/// The energy.
		/// </value>
        public long Energy
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the life.
		/// </summary>
		/// <value>
		/// The life.
		/// </value>
        public long Life
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the mana.
		/// </summary>
		/// <value>
		/// The mana.
		/// </value>
        public long Mana
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the max energy.
		/// </summary>
		/// <value>
		/// The max energy.
		/// </value>
        public long MaxEnergy
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the max life.
		/// </summary>
		/// <value>
		/// The max life.
		/// </value>
        public long MaxLife
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the max mana.
		/// </summary>
		/// <value>
		/// The max mana.
		/// </value>
        public long MaxMana
        {
            get; set;
        }
    }
}
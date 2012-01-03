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

    public abstract class Character : IEntity<long>, IWritable, IFormattable, IDisposable, ICloneable, ISaveable, ILoadable
    {
        #region Fields

        public static readonly HashSet<Character> List = new HashSet<Character>();

        private static readonly string[] HeShe = { "it", "he", "she", "it" };
        private static readonly string[] HimHer = { "it", "him", "her", "it" };
        private static readonly string[] HisHer = { "its", "his", "her", "it" };
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public Character()
        {
            Affects = new ObservableCollection<Affect>();
            Carrying = new ObservableCollection<CharObj>();
            Carrying.CollectionChanged += ArtheaHelper.CollectionChangedHandler<CharObj>((o, n) =>
                {
                    if (n)
                    {
                        if (o.Object != null)
                            o.Object.CarriedBy = this;
                        o.Character = this;
                    }
                    else
                    {
                        if (o.Object != null)
                            o.Object.CarriedBy = null;
                        o.Character = null;
                    }
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

        public ObservableCollection<CharObj> Carrying
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

        public long Id
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

        public abstract World World
        {
            get;
        }

        #endregion Properties

        #region Methods

        public static Character Lookup(string name)
        {
            return List.FirstOrDefault( x=> x.Name.HasWord(name) );
        }

        public static T Lookup<T>(string name)
            where T : Character
        {
            return List.OfType<T>().FirstOrDefault(x => x.Name.HasWord(name));
        }

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

        public virtual bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_char");
            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery()== 1;
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
            List.Remove(this);
        }

        public virtual bool Equals(Character obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Character)) return false;
            return Equals((Character)obj);
        }

        public void Equip(Object obj, WearLocation location)
        {
            if (!obj.WearFlags.HasFlag(location.ToFlag()))
            {
                Log.Warn("Cannot equip object {0} on {1}", obj, location);
                return;
            }

            Carrying.Add(new CharObj { Object = obj, Character = this, WearLocation = location });
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

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

        public abstract void Page(string buf);

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

    public class CharObj : IEntity<long>, ISaveable
    {
        #region Properties

        public Character Character
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public Object Object
        {
            get; set;
        }

        public WearLocation WearLocation
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static explicit operator Object(CharObj chObj)
        {
            return chObj.Object;
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_char_obj");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@char_id", DbType.Int64, Character.Id);
            cmd.AddParameter("@obj_id", DbType.Int64, Object.Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            i++;
            Object = new Object();
            Object.Id = reader.GetInt64(i++);
            Object.Load();
            WearLocation = reader.GetEnum<WearLocation>(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_char_obj");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@char_id", DbType.Int64, Character.Id);
            cmd.AddParameter("@obj_id", DbType.Int64, Object.Id);
            cmd.AddParameter("@wearloc", DbType.String, WearLocation.ToString());

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
            }

            return res;
        }

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
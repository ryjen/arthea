namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using ArtheaEngine;

    using NLog;

    /*
    public class TrashObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Trash; }
        }
    }

    public class ScrollObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Scroll; }
        }
    }

    public class PillObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Pill; }
        }
    }

    public class PotionObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Potion; }
        }
    }

    public class WandObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Wand; }
        }
    }

    public class StaffObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Staff; }
        }
    }

    public class DrinkContainerObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.DrinkContainer; }
        }
    }

    public class ContainerObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Container; }
        }
    }

    public class FurnitureObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Furniture; }
        }
    }
    public class ArmorObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Armor; }
        }
    }

    public class WeaponObject : Object
    {
        public override ObjectType Type
        {
            get { return ObjectType.Weapon; }
        }
    }*/

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

    public class Object : IEntity<long>, IDisposable, ICloneable, IPersistent
    {
        #region Fields

        public static readonly HashSet<Object> List = new HashSet<Object>();

        #endregion Fields

        #region Constructors

        public Object()
        {
            Values = new List<object>();
            Contents = new ObservableCollection<Object>();
            Contents.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Object>((o, a) =>
            {
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

        public Area Area
        {
            get; set;
        }

        public Character CarriedBy
        {
            get; set;
        }

        public float Condition
        {
            get; set;
        }

        public ObservableCollection<Object> Contents
        {
            get; set;
        }

        public decimal Cost
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public ObjectFlag Flags
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public Object Inside
        {
            get; set;
        }

        public short Level
        {
            get; set;
        }

        public string LongDescr
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public Room Room
        {
            get; set;
        }

        public string ShortDescr
        {
            get; set;
        }

        public ObjectType Type
        {
            get; set;
        }

        public IList<object> Values
        {
            get; private set;
        }

        public WearFlag WearFlags
        {
            get; set;
        }

        public WearLocation WearLocation
        {
            get;
            set;
        }

        public float Weight
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static Object Load(long id)
        {
            var obj = new Object();
            obj.Id = id;
            obj.Load();

            return obj;
        }

        public static Object Lookup(string argument)
        {
            return List.FirstOrDefault(x => x.Name.HasWord(argument));
        }

        public static Object Lookup(long id)
        {
            return List.FirstOrDefault(x => x.Id == id);
        }

        public Object Clone()
        {
            return MemberwiseClone() as Object;
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_object");
            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public virtual void Dispose()
        {
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_object");
            cmd.AddParameter("@id", DbType.Int64, Id);

            if (!this.Load(cmd))
                return false;

            ReadContents(conn);
            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;
            Id = reader.GetInt64(i++);
            Condition = reader.GetFloat(i++);
            Description = reader.GetString(i++);
            Level = reader.GetInt16(i++);
            LongDescr = reader.GetString(i++);
            ShortDescr = reader.GetString(i++);
            Weight = reader.GetFloat(i++);
            Name = reader.GetString(i++);
            Cost = reader.GetDecimal(i++);
            Flags = reader.GetEnum<ObjectFlag>(i++);
            WearFlags = reader.GetEnum<WearFlag>(i++);
            Area = Area.Lookup(reader.GetInt32(i++));
            Type = reader.GetEnum<ObjectType>(i++);

            byte[] data = reader.GetValue(i++) as byte[];

            if (data == null)
                return i;

            var ms = new MemoryStream(data);
            var bf = new BinaryFormatter();

            Values = bf.Deserialize(ms) as IList<object>;

            return i;
        }

        public void ReadContents(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_object_contents");
            cmd.AddParameter("@id", DbType.Int64, Id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var obj = new Object();
                    obj.MapRow(reader);

                    Contents.Add(obj);
                }
            }
        }

        public virtual bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_object");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@condition", DbType.Single, Condition);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@level", DbType.Int16, Level);
            cmd.AddParameter("@long_descr", DbType.String, LongDescr);
            cmd.AddParameter("@short_descr", DbType.String, ShortDescr);
            cmd.AddParameter("@weight", DbType.Single, Weight);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@cost", DbType.Currency, Cost);
            cmd.AddParameter("@flags", DbType.String, Flags.ToString());
            cmd.AddParameter("@wear_Flags", DbType.String, WearFlags.ToString());
            cmd.AddParameter("@area", DbType.Int64, Area.Id);
            cmd.AddParameter("@type", DbType.String, Type.ToString());

            var ms = new MemoryStream();
            var bf = new BinaryFormatter();

            bf.Serialize(ms, Values);
            cmd.AddParameter("@values", DbType.Binary, ms.ToArray());

            if (Inside != null)
                cmd.AddParameter("@inside", DbType.Int64, Inside.Id);
            else
                cmd.AddParameter("@inside", DbType.Int64, DBNull.Value);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
                List.Add(this);
            }

            return res;
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
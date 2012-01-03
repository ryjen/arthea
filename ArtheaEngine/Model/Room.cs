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

    public class Room : IEntity<long>, IPersistent
    {
        #region Fields

        public static HashSet<Room> List = new HashSet<Room>();

        #endregion Fields

        #region Constructors

        public Room()
        {
            Characters = new ObservableCollection<Character>();
            Characters.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Character>((c, a) =>
                {
                    c.Room = a ? this : null;
                });
            Objects = new ObservableCollection<Object>();
            Objects.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Object>((o, a) =>
                {
                    o.Room = a ? this : null;
                });
            Exits = new Dictionary<Direction, Exit>();
        }

        #endregion Constructors

        #region Properties

        public Area Area
        {
            get; set;
        }

        public ObservableCollection<Character> Characters
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public Dictionary<Direction, Exit> Exits
        {
            get; set;
        }

        public RoomFlag Flags
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public ObservableCollection<Object> Objects
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

        public static Room GetDefault()
        {
            var room = List.FirstOrDefault();

            if (room != null) return room;

            room = new Room { Name = "Limbo", Description = "You are in darkness.", Terrain = Terrain.Air };

            room.Area = Area.GetDefault();

            return room;
        }

        public static Room Lookup(long id)
        {
            return List.FirstOrDefault(x => x.Id == id);
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

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_room");
            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public virtual bool Equals(Room obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id == Id;
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

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_room");
            cmd.AddParameter("@id", DbType.Int64, Id);
            using (var reader = cmd.ExecuteReader())
            {

                if (!reader.Read()) return false;

                MapRow(reader);
            }

            return true;
        }

        public void LoadReset(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_room_reset");
            cmd.AddParameter("@id", DbType.Int64, Id);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {

                    Reset = new Reset(this);
                    Reset.MapRow(reader);
                }
            }
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Area = Area.Lookup(reader.GetInt32(i++));
            Name = reader.GetString(i++);
            Description = reader.GetString(i++);
            Terrain = reader.GetEnum<Terrain>(i++);
            Flags = reader.GetEnum<RoomFlag>(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_room");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@area", DbType.Int32, Area.Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@terrain", DbType.String, Terrain.ToString());
            cmd.AddParameter("@flags", DbType.String, Flags.ToString());

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
                List.Add(this);
            }

            if (Reset != null)
            {
                Reset.Save(conn);
            }
            return res;
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

        #endregion Methods
    }
}
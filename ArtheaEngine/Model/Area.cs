namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.Linq.Mapping;
    using System.Linq;

    using NLog;

    public class Area : IEntity<int>, ISaveable, ILoadable, IFormattable, ITickable
    {
        #region Fields

        public static readonly HashSet<Area> List = new HashSet<Area>();

        #endregion Fields

        #region Constructors

        public Area()
        {
            Rooms = new ObservableCollection<Room>();
            Rooms.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Room>((r, a) =>
            {
                r.Area = a ? this : null;
            });
            Objects = new ObservableCollection<Object>();
            Objects.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Object>((o, a) =>
                {
                    o.Area = a ? this : null;
                });

            NonPlayers = new ObservableCollection<NonPlayer>();
            NonPlayers.CollectionChanged += ArtheaHelper.CollectionChangedHandler<NonPlayer>((n, a) =>
                {
                    n.Area = a ? this : null;
                });

            TickInterval = ArtheaHelper.Randomizer.Next(90000, 120000);
        }

        #endregion Constructors

        #region Properties

        public string Credits
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

       // [Key]
        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public ObservableCollection<NonPlayer> NonPlayers
        {
            get; set;
        }

        public ObservableCollection<Object> Objects
        {
            get; set;
        }

        public ObservableCollection<Room> Rooms
        {
            get; set;
        }

        public int TickInterval
        {
            get;
            set;
        }

        public World World
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static Area GetDefault()
        {
            var area = List.FirstOrDefault(a => a.Name == "Limbo");

            if (area != null) return area;

            area = new Area { Name = "Limbo", Credits = "Arthea", World = World.GetDefault() };

            return area;
        }

        public static Area Lookup(int id)
        {
            return List.FirstOrDefault(x => x.Id == id);
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_area");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_area");
            cmd.AddParameter("@id", DbType.Int32, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            return true;
        }

        public void LoadNpcs(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_area_npcs");

            cmd.AddParameter("@id", DbType.Int32, Id);

            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    var npc = new NonPlayer();
                    npc.MapRow(reader);

                    NonPlayer.List.Add(npc);
                    NonPlayers.Add(npc);
                }
            }
        }

        public void LoadObjects(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_area_objects");

            cmd.AddParameter("@id", DbType.Int32, Id);

            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    var obj = new Object();
                    obj.MapRow(reader);

                    Object.List.Add(obj);
                    Objects.Add(obj);
                }
            }
        }

        public void LoadRooms(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_area_rooms");

            cmd.AddParameter("@id", DbType.Int32, Id);

            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    var room = new Room();
                    room.MapRow(reader);

                    Room.List.Add(room);
                    Rooms.Add(room);
                }
            }

            foreach (var room in Rooms)
            {
                room.LoadReset(conn);
            }
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt32(i++);
            Name = reader.GetString(i++);
            Credits = reader.GetString(i++);
            World = World.List.SingleOrDefault(x => x.Id == reader.GetInt32(i++));

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_area");

            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@credits", DbType.String, Credits);
            cmd.AddParameter("@world", DbType.Int32, World.Id);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt32(conn.LastInsertId());
                List.Add(this);
            }

            return res;
        }

        public void Tick()
        {
            LogManager.GetCurrentClassLogger().Info("{0} tick!", Name);
            foreach (var room in Rooms)
            {
                room.Tick();
            }
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || format == "N")
            {
                return Name.ToString(formatProvider);
            }

            switch (format)
            {
                default:
                    throw new FormatException("unknown format identifier for area");
            }
        }

        #endregion Methods
    }
}
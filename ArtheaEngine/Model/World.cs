namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.Odbc;
    using System.Linq;
    using System.Text;

    using NLog;

    using Object = ArtheaEngine.Model.Object;

    #region Enumerations

    [Flags]
    public enum WorldFlag
    {
        None,
    }

    #endregion Enumerations

    public partial class World : IEntity<int>, ICloneable, IPersistent
    {
        #region Fields

        public static readonly HashSet<World> List = new HashSet<World>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        static World()
        {
            LoadAll();
        }

        public World()
        {
            Connections = new HashSet<IConnection>();
            Areas = new ObservableCollection<Area>();
            Areas.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Area>((a, n) =>
                {
                    a.World = n ? this : null;
                });
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<Area> Areas
        {
            get; private set;
        }

        public ICollection<IConnection> Connections
        {
            get; private set;
        }

        public WorldFlag Flags
        {
            get; set;
        }

        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public short Port
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static World GetDefault()
        {
            var world = List.FirstOrDefault(x => x.Port == 3778);

            if (world != null) return world;

            Log.Info("Created default world");
            world = new World();
            world.Name = "Arthea";
            world.Port = 3778;

            world.Save();

            List.Add(world);

            return world;
        }

        public static void LoadAll()
        {
            using (var conn = ArtheaHelper.NewConnection())
            {
                var cmd = conn.CreateCommand("select_worlds");

                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var world = new World();
                        world.MapRow(reader);

                        List.Add(world);

                        Log.Info("Loaded world {0}", world.Name);
                    }
                }

                foreach (var world in List)
                {

                    world.LoadAreas(conn);
                }
            }
        }

        // Friendly, type-safe clone method
        public virtual World Clone()
        {
            // Start with a flat, memberwise copy
            World x = new World();

            x.Id = Id;
            x.Name = Name;
            x.Port = Port;

            return x;
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_world");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        // Explicit interface method impl -- available for
        // clients of ICloneable, but invisible to casual
        // clients of MyCloneableClass
        object ICloneable.Clone()
        {
            // simply delegate to our type-safe cousin
            return this.Clone();
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_world");
            cmd.AddParameter("@id", DbType.Int32, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            return true;
        }

        public void LoadAreas(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_world_areas");

            cmd.AddParameter("@id", DbType.Int32, Id);

            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    var area = new Area();
                    area.World = this;
                    area.MapRow(reader);

                    Area.List.Add(area);

                    UpdateManager.Add(area);

                    Areas.Add(area);
                }
            }
            foreach (var area in Areas)
            {
                area.LoadNpcs(conn);
                area.LoadObjects(conn);
                area.LoadRooms(conn);

                area.Tick();
            }
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt32(i++);
            Name = reader.GetStringOrNull(i++);
            Port = reader.GetInt16(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_world");
            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@port", DbType.Int16, Port);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt32(conn.LastInsertId());
                List.Add(this);
            }

            return res;
        }

        public override string ToString()
        {
            return Name + ":" + Port;
        }

        #endregion Methods
    }
}
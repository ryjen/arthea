using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;
using System.Text.RegularExpressions;

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

    #region Enumerations

    [Flags]
    public enum WorldFlag
    {
        None,
    }

    #endregion Enumerations

    public partial class World : INamedEntity, ICloneable, IRootLevelDocument
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public World ()
		{
			Id = ObjectId.GenerateNewId().ToString();
			Connections = new HashSet<IConnection> ();
			Players = new ObservableCollection<Player> ();
        }

		static World ()
		{
			Objects = new Dictionary<string, string> ();

			Rooms = new Dictionary<string, string> ();

			NonPlayers = new Dictionary<string, string> ();

			DbManager.Synchronize(new Race[] { new HumanRace(), new DwarfRace() });

			DbManager.Synchronize(new Class[] { new WarriorClass(), new MageClass() });

			foreach (var a in DbManager.Query<Area>()) {

				foreach(var o in DbManager.Query<Object>().Where (x => x.Key.AreaRef == a.Id))
				{
					Objects.Add(a.Key+"-"+o.Key, o.Id);
				}

				foreach(var r in DbManager.Query<Room>().Where (x => x.Key.AreaRef == a.Id))
				{
					Rooms.Add (a.Key+"-"+r.Key, r.Id);
					a.Rooms.Add (r);
				}

				foreach(var npc in DbManager.Query<NonPlayer>().Where (x => x.Key.AreaRef == a.Id))
				{
					NonPlayers.Add (a.Key+"-"+npc.Key, npc.Id);
				}
			}
		}

        #endregion Constructors

        #region Properties


		public static Dictionary<string,string> Objects
		{
			get; private set;
		}

		public static Dictionary<string,string> Rooms
		{
			get; private set;
		}

		public static Dictionary<string,string> NonPlayers
		{
			get; private set;
		}

		/*
		[BsonIgnore]
		public ObservableCollection<Race> Races
		{
			get; private set;
		}

		[BsonIgnore]
		public ObservableCollection<Class> Classes
		{
			get; private set;
		}

		[BsonIgnore]
        public ObservableCollection<Area> Areas
        {
            get; private set;
        }*/

		[BsonIgnore]
		public ObservableCollection<Player> Players
		{
			get; private set;
		}

		[BsonIgnore]
        public ICollection<IConnection> Connections
        {
            get; private set;
        }

		[BsonDefaultValue(WorldFlag.None)]
		[BsonIgnoreIfDefault]
        public WorldFlag Flags
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

		[BsonRequired]
        public short Port
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static World GetDefault()
        {
			if(DbManager.Count<World>() != 0)
				return DbManager.Query<World>().First();

            /*var world = List.FirstOrDefault(x => x.Port == 3778);

            if (world != null) return world;*/

            Log.Info("Created default world");
            var world = new World();
            world.Name = "Arthea";
            world.Port = 3778;

			world.Save();

            return world;
        }
		
		public bool Save ()
		{
			return DbManager.Save (this);
		}
		
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}

		public static Account FindAccount (string login)
		{
			return DbManager.Query<Account> ().Where (x => x.Login.Equals (login)).FirstOrDefault();
		}

		public static Object FindObject (string keyword)
		{
			if(!Objects.ContainsKey(keyword))
				return null;
			return DbManager.Lookup<Object>(Objects[keyword]);
		}

		public static NonPlayer FindNPC (string keyword)
		{
			if(!NonPlayers.ContainsKey(keyword))
				return null;
			return DbManager.Lookup<NonPlayer>(NonPlayers[keyword]);
		}


		public static Room FindRoom (string keyword)
		{
			if(!Rooms.ContainsKey(keyword))
				return null;
			return DbManager.Lookup<Room>(Rooms[keyword]);
		}


		public static Race FindRace (string keyword)
		{
			var regex = new Regex("^" + keyword, RegexOptions.IgnoreCase);

			return DbManager.Query<Race>().Where(x => regex.IsMatch(x.Name)).FirstOrDefault();
		}

		public static Class FindClass(string keyword)
		{
			var regex = new Regex("^" + keyword, RegexOptions.IgnoreCase);

			return DbManager.Query<Class>().Where(x => regex.IsMatch(x.Name)).FirstOrDefault();
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

        // Explicit interface method impl -- available for
        // clients of ICloneable, but invisible to casual
        // clients of MyCloneableClass
        object ICloneable.Clone()
        {
            // simply delegate to our type-safe cousin
            return this.Clone();
        }

		/*
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
        }*/

        public override string ToString()
        {
            return Name + ":" + Port;
        }

        #endregion Methods
    }
}
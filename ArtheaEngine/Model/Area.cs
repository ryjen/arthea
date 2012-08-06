using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.IdGenerators;

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

    public class Area : INamedEntity, IFormattable, ITickable, IRootLevelDocument
    {
        #region Constructors

        public Area()
        {
			
			Id = ObjectId.GenerateNewId().ToString();
            TickInterval = Game.Rand.Next(90000, 120000);
			Rooms = new ObservableCollection<Room>();
			Rooms.AddChangedHandler( (r,a) => {
				r.Area = a ? this : null;
			});
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

		[BsonIgnore]
		public ObservableCollection<Room> Rooms
		{
			get; private set;
		}

		public string Key
		{
			get; set;
		}
		
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
		{
			get; set;
        }

        public string Name
        {
            get; set;
        }

        public int TickInterval
        {
            get;
            set;
        }

		[BsonRequired]
		[BsonElement("World")]
		public string WorldRef {
			get { return World.Id; }
			set 
			{
				World = DbManager.Lookup<World>(value);
			}
		}

		[BsonIgnore]
        public World World
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
        public static Area GetDefault (World world)
		{
			Area area;

			if (DbManager.Count<Area> () != 0) {
				area = DbManager.Query<Area> ().First ();

				area.Rooms.AddAll (DbManager.Query<Room>().Where(r => r.Key.AreaRef.Equals (area.Id)));

				return area;
			}

            area = new Area() { Key = "limbo", World = world, Name = "Limbo", Credits = "Arthea"};

			var room = new Room { Area = area, Name = "Limbo", Key = "limbo-1", Description = "You are in darkness.", Terrain = Terrain.Air };
        
			area.Rooms.Add(room);

			//World.GetDefault().Areas.Add (area);

			area.Save();

			room.Save();

            return area;
        }

        public static Area Lookup(string keyword)
        {
            return DbManager.Lookup<Area>(keyword);
        }

		/*
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
        }*/

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
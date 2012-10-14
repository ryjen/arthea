using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArtheaEngine.Model
{
	/// <summary>
	/// Area.
	/// </summary>
    public class Area : INamedEntity, IFormattable, ITickable, IRootLevelDocument
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Area"/> class.
		/// </summary>
        public Area()
        {
			Id = ObjectId.GenerateNewId().ToString();
            TickInterval = Game.Rand.Next(90000, 120000);
			Rooms = new ObservableCollection<Room>();
			Rooms.AddChangedHandler( (r,a) => {
				r.Area = a ? this : null;
			});
        }

		/// <summary>
		/// Gets or sets the credits.
		/// </summary>
		/// <value>
		/// The credits.
		/// </value>
        public string Credits
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
		/// Gets the rooms.
		/// </summary>
		/// <value>
		/// The rooms.
		/// </value>
		[BsonIgnore]
		public ObservableCollection<Room> Rooms
		{
			get; private set;
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public string Key
		{
			get; set;
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
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        public string Name
        {
            get; set;
        }

		/// <summary>
		/// Gets the tick interval.
		/// </summary>
		/// <value>
		/// The tick interval.
		/// </value>
        public int TickInterval
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the world reference.
		/// </summary>
		/// <value>
		/// The world reference.
		/// </value>
		[BsonRequired]
		[BsonElement("World")]
		public string WorldRef {
			get { return World.Id; }
			set 
			{
				World = DbManager.Lookup<World>(value);
			}
		}

		/// <summary>
		/// Gets or sets the world.
		/// </summary>
		/// <value>
		/// The world.
		/// </value>
		[BsonIgnore]
        public World World
        {
            get; set;
        }

		/// <summary>
		/// Save this instance.
		/// </summary>
		public bool Save ()
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
		/// Gets the default.
		/// </summary>
		/// <returns>
		/// The default.
		/// </returns>
		/// <param name='world'>
		/// World.
		/// </param>
        public static Area GetDefault (World world)
		{
			Area area;

			if (DbManager.Count<Area> () != 0) {
				area = DbManager.Query<Area> ().First ();

				area.Rooms.AddAll (DbManager.Query<Room>().Where(r => r.AreaRef.Equals (area.Id)));

				return area;
			}

            area = new Area() { Key = "limbo", World = world, Name = "Limbo", Credits = "Arthea"};

			var room = new Room { Area = area, Name = "Limbo", Description = "You are in darkness.", Terrain = Terrain.Air };
        
			area.Rooms.Add(room);

			area.Save();

			room.Save();

            return area;
        }

		/// <summary>
		/// Lookup the specified keyword.
		/// </summary>
		/// <param name='keyword'>
		/// Keyword.
		/// </param>
        public static Area Lookup(string keyword)
        {
            return DbManager.Lookup<Area>(keyword);
        }

		/// <summary>
		/// Tick this instance.
		/// </summary>
        public void Tick()
        {
            LogManager.GetCurrentClassLogger().Info("{0} tick!", Name);
            foreach (var room in Rooms)
            {
                room.Tick();
            }
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

    }
}
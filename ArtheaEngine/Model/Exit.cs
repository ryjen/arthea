using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;


namespace ArtheaEngine.Model
{
	/// <summary>
	/// Direction.
	/// </summary>
    public enum Direction
    {
        North,
        South,
        East,
        West,
        Up,
        Down
    }

	/// <summary>
	/// Exit flag.
	/// </summary>
    [Flags]
    public enum ExitFlag
    {
        None, Closed, Door, Locked, NoPass, PickProof
    }

	/// <summary>
	/// Exit.
	/// </summary>
    public class Exit : IEntity
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Exit"/> class.
		/// </summary>
		public Exit ()
		{
			Id = ObjectId.GenerateNewId ().ToString ();
		}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[BsonIgnoreIfNull]
        public string Description
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the direction.
		/// </summary>
		/// <value>
		/// The direction.
		/// </value>
		[BsonRequired]
        public Direction Direction
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
		[BsonDefaultValue(ExitFlag.None)]
		[BsonIgnoreIfDefault]
        public ExitFlag Flags
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
		/// Gets or sets the key reference.
		/// </summary>
		/// <value>
		/// The key reference.
		/// </value>
		[BsonIgnoreIfNull]
        public string KeyRef {
			get { return Key == null ? null : Key.Id; }
			set 
			{
				Key = Object.Lookup(value);
			}
        }

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		[BsonIgnore]
		public Object Key
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the keyword.
		/// </summary>
		/// <value>
		/// The keyword.
		/// </value>
		[BsonIgnoreIfNull]
        public string Keyword
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the room reference.
		/// </summary>
		/// <value>
		/// The room reference.
		/// </value>
		[BsonRequired]
		public string RoomRef {
			get { return Room == null ? null : Room.Id; }
			set 
			{
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
		/// Gets or sets to room reference.
		/// </summary>
		/// <value>
		/// To room reference.
		/// </value>
		[BsonRequired]
		public string ToRoomRef {
			get { return ToRoom == null ? null : ToRoom.Id; }
			set { ToRoom = Room.Lookup (value); }
        }

		/// <summary>
		/// Gets or sets to room.
		/// </summary>
		/// <value>
		/// To room.
		/// </value>
		public Room ToRoom {
			get;
			set; 
		}

    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.Odbc;

using NLog;

namespace ArtheaEngine.Model
{

    #region Enumerations

    public enum Direction
    {
        North,
        South,
        East,
        West,
        Up,
        Down
    }

    [Flags]
    public enum ExitFlag
    {
        None, Closed, Door, Locked, NoPass, PickProof
    }

    #endregion Enumerations

    public class Exit : IEntity
    {
		public Exit ()
		{
			
			Id = ObjectId.GenerateNewId ().ToString ();
		}

        #region Properties

		[BsonIgnoreIfNull]
        public string Description
        {
            get; set;
        }

		[BsonRequired]
        public Direction Direction
        {
            get; set;
        }

		[BsonDefaultValue(ExitFlag.None)]
		[BsonIgnoreIfDefault]
        public ExitFlag Flags
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string KeyRef {
			get { return Key == null ? null : Key.Id; }
			set 
			{
				Key = Object.Lookup(value);
			}
        }

		[BsonIgnore]
		public Object Key
		{
			get; set;
		}

		[BsonIgnoreIfNull]
        public string Keyword
        {
            get; set;
        }

		[BsonRequired]
		public string RoomRef {
			get { return Room == null ? null : Room.Id; }
			set 
			{
				Room = Room.Lookup(value);
			}
		}

		[BsonIgnore]
        public Room Room
        {
            get; set;
        }

		[BsonRequired]
		public string ToRoomRef {
			get { return ToRoom == null ? null : ToRoom.Id; }
			set { ToRoom = Room.Lookup (value); }
        }

		public Room ToRoom {
			get;
			set; 
		}

        #endregion Properties

    }
}
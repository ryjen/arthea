using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ArtheaEngine.Model
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Text;

    using NLog;

    #region Enumerations

    [Flags]
    public enum NpcFlag : long
    {
        None, Aggressive, Guildmaster, Sentinel, StayArea, Wimpy
    }

    #endregion Enumerations

    public class NonPlayer : Character
    {
		public NonPlayer () : base()
		{
		}

        #region Properties

		[BsonRequired]
		public string AreaRef {
			get { return Area == null ? null : Area.Id; }
			set { Area = Area.Lookup (value); }
		}

		[BsonIgnore]
        public Area Area 
		{
			get; set;
        }

		[BsonDefaultValue(NpcFlag.None)]
		[BsonIgnoreIfDefault]
        public NpcFlag Flags
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string LongDescr
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public Shop Shop
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string ShortDescr
        {
            get; set;
        }

		[BsonIgnore]
        public override World World
        {
            get { return Area == null ? null : Area.World; }
        }

        #endregion Properties

        #region Methods

		
		public string Keyword ()
		{
			if (!World.NonPlayers.ContainsValue (Id)) {
				return Id;
			}

			return World.NonPlayers[Id];
		}


        public override void Page(string buf)
        {
        }

        public override void Write(string format, params object[] args)
        {
        }

        public override void Write(byte[] value)
        {
        }

        public override void Write(object obj)
        {
        }

        public override void WriteLine()
        {
        }

        public override void WriteLine(object obj)
        {
        }

        public override void WriteLine(string format, params object[] args)
        {
        }

        #endregion Methods
    }
}
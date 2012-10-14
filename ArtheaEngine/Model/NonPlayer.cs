using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Npc flag.
	/// </summary>
    [Flags]
    public enum NpcFlag : long
    {
        None, Aggressive, Guildmaster, Sentinel, StayArea, Wimpy
    }

	/// <summary>
	/// Non player.
	/// </summary>
    public class NonPlayer : Character
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.NonPlayer"/> class.
		/// </summary>
		public NonPlayer () : base()
		{
		}
		/// <summary>
		/// Gets or sets the area.
		/// </summary>
		/// <value>
		/// The area.
		/// </value>
		[BsonIgnore]
		public Area Area
		{
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the area reference.
		/// </summary>
		/// <value>
		/// The area reference.
		/// </value>
		[BsonRequired]
		[BsonElement("Area")]
		public string AreaRef
		{
			get { return Area == null ? null : Area.Id; }
			
			set { Area = Area.Lookup (value); }
		}
		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
		[BsonDefaultValue(NpcFlag.None)]
		[BsonIgnoreIfDefault]
        public NpcFlag Flags
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the long descr.
		/// </summary>
		/// <value>
		/// The long descr.
		/// </value>
		[BsonIgnoreIfNull]
        public string LongDescr
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the shop.
		/// </summary>
		/// <value>
		/// The shop.
		/// </value>
		[BsonIgnoreIfNull]
        public Shop Shop
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the short descr.
		/// </summary>
		/// <value>
		/// The short descr.
		/// </value>
		[BsonIgnoreIfNull]
        public string ShortDescr
        {
            get; set;
        }

		/// <summary>
		/// Gets the world.
		/// </summary>
		/// <value>
		/// The world.
		/// </value>
		[BsonIgnore]
        public override World World
        {
            get { return Area == null ? null : Area.World; }
        }

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
        public override void Page(string buf)
        {
        }

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		public override void Page (StringBuilder buf)
		{
		}

		/// <summary>
		/// Write the specified format and args.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public override void Write(string format, params object[] args)
        {
        }

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
        public override void Write(byte[] value)
        {
        }

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public override void Write(object obj)
        {
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
        public override void WriteLine()
        {
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public override void WriteLine(object obj)
        {
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public override void WriteLine(string format, params object[] args)
        {
        }
    }
}
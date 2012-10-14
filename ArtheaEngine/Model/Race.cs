using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{

	/// <summary>
	/// Dwarf race.
	/// </summary>
    public class DwarfRace : PcRace
    {
        public DwarfRace()
        {
			Name = "Dwarf";
			Description = "A short, stocky race.";
        }
    }

	/// <summary>
	/// Human race.
	/// </summary>
    public class HumanRace : PcRace
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.HumanRace"/> class.
		/// </summary>
        public HumanRace()
        {
			Name = "Human";
			Description = "The standard race.";
        }
    }

	/// <summary>
	/// Pc race.
	/// </summary>
	[BsonKnownTypes(typeof(HumanRace), typeof(DwarfRace))]
	[BsonDiscriminator(Required = true, RootClass = false)]
    public abstract class PcRace : Race
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.PcRace"/> class.
		/// </summary>
        public PcRace()
        {
        }
    }

	/// <summary>
	/// Race.
	/// </summary>
	[BsonKnownTypes(typeof(PcRace), typeof(UniqueRace))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Race : INamedEntity, IRootLevelDocument
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Race"/> class.
		/// </summary>
		public Race() {
			
			Id = ObjectId.GenerateNewId().ToString();
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
            get; protected set;
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
            get; protected set;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[BsonRequired]
        public string Name
        {
            get; protected set;
        }

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public static Race Lookup (string id)
		{
			return DbManager.Lookup<Race>(id);
		}

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static T Lookup<T> (string id) where T:Race
		{
			return DbManager.LookupAs<Race,T>(id);
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
    }

	/// <summary>
	/// Unique race.
	/// </summary>
    public class UniqueRace : Race
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.UniqueRace"/> class.
		/// </summary>
		public UniqueRace ()
		{
			Name = "unique";
			Description = "A unique, unknown race.";
		}
    }
}
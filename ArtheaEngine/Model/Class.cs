using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{

	/// <summary>
	/// Class.
	/// </summary>
	[BsonKnownTypes(typeof(WarriorClass), typeof(MageClass))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public class Class : INamedEntity, IRootLevelDocument
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Class"/> class.
		/// </summary>
		public Class ()
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
		/// Lookup the specified keyword.
		/// </summary>
		/// <param name='keyword'>
		/// Keyword.
		/// </param>
        public static Class Lookup(string keyword)
        {
			return DbManager.Lookup<Class>(keyword);
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
	/// Mage class.
	/// </summary>
    public class MageClass : Class
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.MageClass"/> class.
		/// </summary>
        public MageClass()
        {
			Name = "Mage";
			Description = "A basic magic using class.";
        }
    }

	/// <summary>
	/// Warrior class.
	/// </summary>
    public class WarriorClass : Class
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.WarriorClass"/> class.
		/// </summary>
        public WarriorClass()
        {
			Name = "Warrior";
			Description = "A basic fighting class.";
        }
    }
}
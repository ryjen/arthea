using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Odbc;
    using System.Linq;
    using System.Reflection;

	[BsonKnownTypes(typeof(WarriorClass), typeof(MageClass))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public class Class : INamedEntity, IRootLevelDocument
    {
		public Class ()
		{
			
			Id = ObjectId.GenerateNewId ().ToString ();
		}

        #region Properties

		[BsonIgnoreIfNull]
        public string Description
        {
			get; protected set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; protected set;
        }

		[BsonRequired]
        public string Name 
		{
			get; protected set;
        }

        #endregion Properties

        #region Methods

        public static Class Lookup(string keyword)
        {
			return DbManager.Lookup<Class>(keyword);
        }

        #endregion Methods
    }

    public class MageClass : Class
    {
        #region Constructors

        public MageClass()
        {
			Name = "Mage";
			Description = "A basic magic using class.";
        }

        #endregion Constructors

    }

    public class WarriorClass : Class
    {
        #region Constructors

        public WarriorClass()
        {
			Name = "Warrior";
			Description = "A basic fighting class.";
        }

        #endregion Constructors

    }
}
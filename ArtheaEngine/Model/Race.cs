using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using NLog;

namespace ArtheaEngine.Model
{

    public class DwarfRace : PcRace
    {
        #region Constructors

        public DwarfRace()
        {
			Name = "Dwarf";
			Description = "A short, stocky race.";
        }

        #endregion Constructors
    }

    public class HumanRace : PcRace
    {
        #region Constructors

        public HumanRace()
        {
			Name = "Human";
			Description = "The standard race.";
        }

        #endregion Constructors

    }
	
	[BsonKnownTypes(typeof(HumanRace), typeof(DwarfRace))]
	[BsonDiscriminator(Required = true, RootClass = false)]
    public abstract class PcRace : Race
    {
        #region Constructors

        public PcRace()
        {
        }

        #endregion Constructors
    }

	[BsonKnownTypes(typeof(PcRace), typeof(UniqueRace))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Race : INamedEntity, IRootLevelDocument
    {
		public Race() {
			
			Id = ObjectId.GenerateNewId().ToString();
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

		public static Race Lookup (string id)
		{
			return DbManager.Lookup<Race>(id);
		}

		public static T Lookup<T> (string id) where T:Race
		{
			return DbManager.LookupAs<Race,T>(id);
		}
		
		public bool Save ()
		{
			return DbManager.Save (this);
		}

		
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}
    }

    public class UniqueRace : Race
    {
        #region Properties

		public UniqueRace ()
		{
			Name = "unique";
			Description = "A unique, unknown race.";
		}
       
        #endregion Properties
    }
}
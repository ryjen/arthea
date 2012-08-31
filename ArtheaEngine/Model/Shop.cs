using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;

namespace ArtheaEngine.Model
{

    public class Shop : IEntity
    {
		public Shop ()
		{
			Id = ObjectId.GenerateNewId ().ToString ();
		}
        #region Properties

        public short CloseHour
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonDefaultValue(6)]
		[BsonIgnoreIfDefault]
        public short OpenHour
        {
            get; set;
        }

		[BsonIgnore]
        public Character Owner
        {
            get; set;
        }

		[BsonDefaultValue(110)]
		[BsonIgnoreIfDefault]
        public short ProfitBuy
        {
            get; set;
        }

		[BsonDefaultValue(110)]
		[BsonIgnoreIfDefault]
        public short ProfitSell
        {
            get; set;
        }

        #endregion Properties

    }
}
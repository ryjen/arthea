using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;

using NLog;

namespace ArtheaEngine.Model
{

    #region Enumerations

    [Flags]
    public enum HelpCategory
    {
        None, Admin, Information
    }

    #endregion Enumerations

    public class Help : IEquatable<Help>, IRootLevelDocument
    {
        #region Constructors

        static Help()
        {
        }

        public Help()
        {
			Id = ObjectId.GenerateNewId().ToString();
        }

        #endregion Constructors

        #region Properties

		[BsonDefaultValue(HelpCategory.None)]
		[BsonIgnoreIfDefault]
        public HelpCategory Category
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public string Id
        {
            get; set;
        }

        public ISet<string> Keywords
        {
            get; set;
        }

        public IList<Help> Related
        {
            get; set;
        }

		[BsonRequired]
        public string Text
        {
            get; set;
        }

        #endregion Properties

        #region Methods

		
		public bool Save ()
		{
			return DbManager.Save (this);
		}
		
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}
        public static Help Lookup(string id)
        {
			return DbManager.Lookup<Help>(id);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Help)) return false;

            return Equals(obj as Help);
        }

        /// <summary>
        ///                     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        ///                     An object to compare with this object.
        ///                 </param>
        public bool Equals(Help other)
        {
			return Id.Equals (other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion Methods
    }
}
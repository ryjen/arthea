using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Help category.
	/// </summary>
    [Flags]
    public enum HelpCategory
    {
        None, Admin, Information
    }

	/// <summary>
	/// Help.
	/// </summary>
    public class Help : IEquatable<Help>, IRootLevelDocument
    {
		/// <summary>
		/// Initializes the <see cref="ArtheaEngine.Model.Help"/> class.
		/// </summary>
        static Help()
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Help"/> class.
		/// </summary>
        public Help()
        {
			Id = ObjectId.GenerateNewId().ToString();
        }

		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>
		/// The category.
		/// </value>
		[BsonDefaultValue(HelpCategory.None)]
		[BsonIgnoreIfDefault]
        public HelpCategory Category
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
		/// Gets or sets the keywords.
		/// </summary>
		/// <value>
		/// The keywords.
		/// </value>
        public ISet<string> Keywords
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the related.
		/// </summary>
		/// <value>
		/// The related.
		/// </value>
        public IList<Help> Related
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		[BsonRequired]
        public string Text
        {
            get; set;
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

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
        public static Help Lookup(string id)
        {
			return DbManager.Lookup<Help>(id);
        }

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ArtheaEngine.Model.Help"/>.
		/// </summary>
		/// <param name='obj'>
		/// The <see cref="System.Object"/> to compare with the current <see cref="ArtheaEngine.Model.Help"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ArtheaEngine.Model.Help"/>; otherwise, <c>false</c>.
		/// </returns>
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

		/// <summary>
		/// Serves as a hash function for a <see cref="ArtheaEngine.Model.Help"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
		/// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
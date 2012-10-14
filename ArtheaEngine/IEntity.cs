using ArtheaEngine.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;

namespace ArtheaEngine
{

    /// <summary>
    /// Entity interface.
    /// </summary>
    public interface IEntity
    {
        #region Properties

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id
        {
            get;
        }

        #endregion Properties
    }

    /// <summary>
    /// Entity with a name.
    /// </summary>
    public interface INamedEntity : IEntity
    {
        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name
        {
            get;
        }

        #endregion Properties
    }

    /// <summary>
    /// root level document interface.
    /// </summary>
    public interface IRootLevelDocument : IEntity
    {
        #region Methods

        bool Delete();

        bool Save();

        #endregion Methods
    }

    /// <summary>
    /// A foreign key to an area
    /// </summary>
    /*public class AreaKey
    {
        #region Fields

        public static readonly char SEPERATOR = '-';

        #endregion Fields

        #region Properties

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
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        /// <param name='key'>
        /// Key.
        /// </param>
        public static implicit operator string(AreaKey key)
        {
            return key.ToString();
        }

        /// <param name='key'>
        /// Key.
        /// </param>
        public static implicit operator Area(AreaKey key)
        {
            return key.Area;
        }

        /// <param name='arg'>
        /// Argument.
        /// </param>
        public static implicit operator AreaKey(string arg)
        {
            var args = arg.Split (new char[]{SEPERATOR});

            if (args.Length < 2) {
                throw new ArgumentException ("invalid key format (area-key)");
            }

            AreaKey key = new AreaKey();

            key.Area = DbManager.Query<Area>().Where(x => x.Key.Equals(args[0])).FirstOrDefault();

            key.Value = int.Parse(args[1]);

            return key;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.AreaKey"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="ArtheaEngine.AreaKey"/>.
        /// </returns>
        public override string ToString()
        {
            return Area.Key + SEPERATOR + Value;
        }

        #endregion Methods
    }*/
}
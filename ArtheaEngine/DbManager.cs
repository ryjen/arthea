using System.Text.RegularExpressions;

namespace ArtheaEngine
{
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using DBQuery = MongoDB.Driver.Builders.Query;

    /// <summary>
    /// Database manager.
    /// </summary>
    public static class DbManager
    {
        #region Constructors

        /// <summary>
        /// Initializes the <see cref="ArtheaEngine.DbManager"/> class.
        /// </summary>
        static DbManager()
        {
            // Legacy entry points receive database settings only from the environment.
            var connectionString = Environment.GetEnvironmentVariable("ARTHEA_MONGODB_URI");
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ARTHEA_MONGODB_URI is required for database access.");
            var url = MongoUrl.Create(connectionString);
            if (String.IsNullOrWhiteSpace(url.DatabaseName))
                throw new InvalidOperationException("ARTHEA_MONGODB_URI must specify a database name.");
            Instance = MongoServer.Create(url.ToServerSettings()).GetDatabase(url.DatabaseName);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        internal static MongoDatabase Instance
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Count records of a type.
        /// </summary>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static long Count<T>()
            where T : IRootLevelDocument
        {
            return Collection<T>().Count();
        }

        /// <summary>
        /// Delete the specified data.
        /// </summary>
        /// <param name='e'>
        /// E.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static bool Delete<T>(T e)
            where T : IEntity, IRootLevelDocument
        {
            return Collection<T>().Remove(DBQuery.EQ("_id", e.Id)).Ok;
        }

        /// <summary>
        /// Lookup the specified document by id.
        /// </summary>
        /// <param name='id'>
        /// Identifier.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static T Lookup<T>(string id)
            where T : IRootLevelDocument
        {
            return Collection<T>().FindOneById(id);
        }

		public static T Find<T>(string key) where T : INamedEntity, IRootLevelDocument
		{
			var regex = new Regex("^" + key, RegexOptions.IgnoreCase);

			return Query<T>().Where(x => regex.IsMatch(x.Name) || key.Equals (x.Id)).FirstOrDefault();
		}

        /// <summary>
        /// Lookups as.
        /// </summary>
        /// <returns>
        /// The as.
        /// </returns>
        /// <param name='id'>
        /// Identifier.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        /// <typeparam name='A'>
        /// The 2nd type parameter.
        /// </typeparam>
        public static A LookupAs<T, A>(string id)
            where T : IRootLevelDocument
        {
            return Collection<T>().FindOneByIdAs<A>(id);
        }

        /// <summary>
        /// Query this instance.
        /// </summary>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static IQueryable<T> Query<T>()
            where T : IRootLevelDocument
        {
            return Instance.GetCollection<T>(typeof(T).Name).AsQueryable();
        }

        /// <summary>
        /// Save the specified data.
        /// </summary>
        /// <param name='data'>
        /// Data.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static void Save<T>(IEnumerable<T> data)
            where T : IRootLevelDocument
        {
            Collection<T>().InsertBatch(data);
        }

        /// <summary>
        /// Save the specified data.
        /// </summary>
        /// <param name='data'>
        /// Data.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static bool Save<T>(T data)
            where T : IRootLevelDocument
        {
            return Collection<T>().Save(data).Ok;
        }

        /// <summary>
        /// Synchronize the specified collection.
        /// </summary>
        /// <param name='collection'>
        /// Collection.
        /// </param>
        /// <typeparam name='T'>
        /// The 1st type parameter.
        /// </typeparam>
        public static IQueryable<T> Synchronize<T>(IEnumerable<T> collection)
            where T : IRootLevelDocument
        {
            if (Count<T>() == 0)
            {
                Collection<T>().InsertBatch(collection);
            }
            return Collection<T>().AsQueryable();
        }

        /**
         * Internal to prevent outside sources from creating new documents
         */
        internal static MongoCollection<T> Collection<T>()
            where T : IRootLevelDocument
        {
            return Instance.GetCollection<T>(GetBaseType(typeof(T)).Name);
        }

        /// <summary>
        /// Gets the type of the base.
        /// </summary>
        /// <returns>
        /// The base type.
        /// </returns>
        /// <param name='type'>
        /// Type.
        /// </param>
        private static Type GetBaseType(Type type)
        {
            Type baseType = type;
            var rootDoc = typeof(IRootLevelDocument);

            for (var temp = type.BaseType; temp != null; temp = temp.BaseType)
            {
                if (!rootDoc.IsAssignableFrom(temp))
                    break;
                baseType = temp;
            }

            return baseType;
        }

        #endregion Methods
    }
}
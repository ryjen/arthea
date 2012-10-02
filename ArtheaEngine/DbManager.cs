using System;
using System.Data;
using System.Configuration;

using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ArtheaEngine
{
	/// <summary>
	/// Database manager.
	/// </summary>
	public static class DbManager
	{
		/// <summary>
		/// Initializes the <see cref="ArtheaEngine.DbManager"/> class.
		/// </summary>
		static DbManager ()
		{
            //var connectionString = "mongodb://localhost/Arthea?safe=true";
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var url = MongoUrl.Create(connectionString);
			Instance = MongoServer.Create(url.ToServerSettings()).GetDatabase(url.DatabaseName);
		}

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

		/// <summary>
		/// Gets the type of the base.
		/// </summary>
		/// <returns>
		/// The base type.
		/// </returns>
		/// <param name='type'>
		/// Type.
		/// </param>
		private static Type GetBaseType (Type type)
		{
			Type baseType = type;
			var rootDoc = typeof(IRootLevelDocument);

			for (var temp = type.BaseType; temp != null; temp = temp.BaseType) {
				if(!rootDoc.IsAssignableFrom(temp))
					break;
				baseType = temp;
			}

			return baseType;
		}

		/**
		 * Internal to prevent outside sources from creating new documents
		 */
		internal static MongoCollection<T> Collection<T>() where T: IRootLevelDocument {
			return Instance.GetCollection<T>(GetBaseType(typeof(T)).Name);
		}

		/// <summary>
		/// Query this instance.
		/// </summary>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static IQueryable<T> Query<T> () where T: IRootLevelDocument
		{
			return Instance.GetCollection<T> (typeof(T).Name).AsQueryable ();
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
		public static void Save<T> (IEnumerable<T> data) where T: IRootLevelDocument
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
		public static bool Save<T> (T data) where T: IRootLevelDocument
		{
			return Collection<T>().Save(data).Ok;
		}

		/// <summary>
		/// Count records of a type.
		/// </summary>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static long Count<T> () where T: IRootLevelDocument
		{
			return Collection<T> ().Count ();
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
		public static T Lookup<T> (string id) where T:IRootLevelDocument
		{
			return Collection<T>().FindOneById(id);
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
		public static A LookupAs<T,A> (string id) where T:IRootLevelDocument
		{
			return Collection<T>().FindOneByIdAs<A>(id);
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
		public static bool Delete<T> (T e) where T : IEntity, IRootLevelDocument
		{
			return Collection<T>().Remove(Query.EQ ("_id", e.Id)).Ok;
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
		public static IQueryable<T> Synchronize<T> (IEnumerable<T> collection) where T:IRootLevelDocument
		{
			if (Count<T>() == 0) {
				Collection<T> ().InsertBatch (collection);
			}
			return Collection<T>().AsQueryable();
		}

	}
}


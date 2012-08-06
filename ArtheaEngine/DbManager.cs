using System;
using System.Data;
using System.Configuration;

using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Linq;

namespace ArtheaEngine
{
	public static class DbManager
	{
		static DbManager ()
		{
            var connectionString = "mongodb://localhost/?safe=true";
			Instance = MongoServer.Create(connectionString).GetDatabase("Arthea");
		}

		internal static MongoDatabase Instance
		{
			get;
			private set;
		}

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

		public static IQueryable<T> Query<T> () where T: IRootLevelDocument
		{
			return Instance.GetCollection<T> (typeof(T).Name).AsQueryable ();
		}

		public static void Save<T> (IEnumerable<T> data) where T: IRootLevelDocument
		{
			Collection<T>().InsertBatch(data);
		}

		public static bool Save<T> (T data) where T: IRootLevelDocument
		{
			return Collection<T>().Save(data).Ok;
		}

		public static long Count<T> () where T: IRootLevelDocument
		{
			return Collection<T> ().Count ();
		}

		public static T Lookup<T> (string id) where T:IRootLevelDocument
		{
			return Collection<T>().FindOneById(id);
		}

		public static A LookupAs<T,A> (string id) where T:IRootLevelDocument
		{
			return Collection<T>().FindOneByIdAs<A>(id);
		}

		public static bool Delete<T> (T e) where T : IEntity, IRootLevelDocument
		{
			return Collection<T>().Remove(Query.EQ ("_id", e.Id)).Ok;
		}

		/*public static bool Save<T>(T e) where T:IRootLevelDocument
		{
			return Collection<T>().Save(e).Ok;
		}*/

		public static IQueryable<T> Synchronize<T> (IEnumerable<T> collection) where T:IRootLevelDocument
		{
			if (Count<T>() == 0) {
				Collection<T> ().InsertBatch (collection);
			}
			return Collection<T>().AsQueryable();
		}

	}
}


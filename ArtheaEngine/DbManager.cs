using System;
using System.Data;
using System.Configuration;

using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

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

		internal static MongoCollection<T> GetCollection<T>() where T: IRootLevelDocument {
			return Instance.GetCollection<T>(typeof(T).Name);
		}

		public static void InsertBatch<T> (IEnumerable<T> data) where T: IRootLevelDocument
		{
			GetCollection<T>().InsertBatch(data);
		}

		public static long Count<T> () where T: IRootLevelDocument
		{
			return GetCollection<T> ().Count ();
		}
		public static bool CollectionExists<T>() where T:IRootLevelDocument
		{
			return Instance.CollectionExists (typeof(T).Name);
		}

		public static T Lookup<T> (string id) where T:IRootLevelDocument
		{
			return GetCollection<T>().FindOneById(id);
		}

		public static MongoCursor<T> GetAll<T> () where T:IRootLevelDocument
		{
			return GetCollection<T> ().FindAll ();
		}

		public static bool Delete<T> (T e) where T : IEntity, IRootLevelDocument
		{
			return GetCollection<T>().Remove(Query.EQ ("Id", e.Id)).Ok;
		}

		internal static IEnumerable<T> Find<T> (IMongoQuery query) where T:IRootLevelDocument
		{
			return GetCollection<T>().Find (query);
		}

		public static bool Save<T>(T e) where T:IRootLevelDocument
		{
			return GetCollection<T>().Save(e).Ok;
		}

		
		public static IEnumerable<T> Synchronize<T> (IEnumerable<T> collection) where T:IRootLevelDocument
		{
			if (!CollectionExists<T> () || Count<T>() == 0) {
				GetCollection<T> ().InsertBatch (collection);
			}
			return GetCollection<T>().FindAll();
		}

	}
}


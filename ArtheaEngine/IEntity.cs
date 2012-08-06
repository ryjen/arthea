using MongoDB.Bson;
using System.Collections.Generic;
using ArtheaEngine.Model;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtheaEngine
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public interface IEntity
    {
		string Id
		{
			get;
		}
	}

	public interface INamedEntity : IEntity
	{
		string Name
		{
			get;
		}
    }

	public class AreaKey
	{
		public static readonly char SEPERATOR = '-';
		
		[BsonRequired]
		[BsonElement("Area")]
		public string AreaRef {
			get { return Area == null ? null : Area.Id; }

			set { Area = Area.Lookup (value); }
		}

		[BsonIgnore]
		public Area Area
		{
			get; set;
		}

		public int Value
		{
			get; set;
		}

		public override string ToString()
		{
			return Area.Key + SEPERATOR + Value;
		}

		public static implicit operator string(AreaKey key)
		{
			return key.ToString();
		}

		public static implicit operator Area (AreaKey key)
		{
			return key.Area;
		}

		public static implicit operator AreaKey (string arg)
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
	}

	public interface IRootLevelDocument : IEntity
	{
		bool Save();
		bool Delete();
	}
}
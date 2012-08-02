using MongoDB.Bson;
using System.Collections.Generic;

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

	public interface IKeyedEntity : IEntity
	{
		int Key
		{
			get;
		}
	}

	public interface IRootLevelDocument : IEntity
	{
	}
}
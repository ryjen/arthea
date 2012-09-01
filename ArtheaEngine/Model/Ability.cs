using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

using NLog;

namespace ArtheaEngine.Model
{

	[BsonKnownTypes(typeof(Skill), typeof(Spell))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Ability : ICloneable, INamedEntity, IRootLevelDocument
    {
		public Ability()
		{	
			Id = ObjectId.GenerateNewId().ToString();
		}
        #region Properties

        public Currency Cost
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public short Level
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; private set;
        }

		public string Name
		{
			get; set;
		}

        #endregion Properties

        #region Methods

		public static Ability Lookup (string keyword)
		{
			return DbManager.Lookup<Ability>(keyword);
		}

		public bool Save ()
		{
			return DbManager.Save (this);
		}

		public bool Delete ()
		{
			return DbManager.Delete (this);
		}
		                                          

        /// <summary>
        ///                     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///                     A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            var newObject = Activator.CreateInstance(GetType());

            Game.CloneObj(newObject, this);

            return newObject;
        }

        public void DisplayOffMsg(object from)
        {
            string msg;

            if (from == null)
                return;

            if (from is Character)
            {
                var ch = from as Character;
                msg = GetAbilityMsg("MsgOffSelf");
                ch.WriteLine(msg);
                msg = GetAbilityMsg("MsgOffChar");
                ch.Room.Act(msg);
            }
            else if (from is Object)
            {
                var item = from as Object;
                if (item.CarriedBy != null || item.Room != null)
                {
                    msg = GetAbilityMsg("MsgOffObj");
                    if (item.CarriedBy != null)
                    {
                        item.CarriedBy.Act(msg);
                    }
                    else if (item.Room != null)
                    {
                        item.Room.Act(msg);
                    }
                }
            }
            else if (from is Room)
            {
                var room = from as Room;
                msg = GetAbilityMsg("MsgOffRoom");

                room.Act(msg);
            }
        }

        protected virtual string GetAbilityMsg(string name)
        {
            var fi = GetType().GetField(name,
                                        BindingFlags.Static | BindingFlags.NonPublic |
                                        BindingFlags.Public);
            return fi.GetValue(null) as string;
        }

        #endregion Methods
    }
}
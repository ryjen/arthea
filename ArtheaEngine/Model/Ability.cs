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
	/// <summary>
	/// Ability.
	/// </summary>
	[BsonKnownTypes(typeof(Skill), typeof(Spell))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Ability : ICloneable, INamedEntity, IRootLevelDocument
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Ability"/> class.
		/// </summary>
		public Ability()
		{	
			Id = ObjectId.GenerateNewId().ToString();
		}
        #region Properties

		/// <summary>
		/// Gets or sets the cost.
		/// </summary>
		/// <value>
		/// The cost.
		/// </value>
        public Currency Cost
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
        public string Description
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the level.
		/// </summary>
		/// <value>
		/// The level.
		/// </value>
        public short Level
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
            get; private set;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name
		{
			get; set;
		}

        #endregion Properties

        #region Methods

		/// <summary>
		/// Lookup the specified keyword.
		/// </summary>
		/// <param name='keyword'>
		/// Keyword.
		/// </param>
		public static Ability Lookup (string keyword)
		{
			return DbManager.Lookup<Ability>(keyword);
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

		/// <summary>
		/// Displaies the off message.
		/// </summary>
		/// <param name='from'>
		/// From.
		/// </param>
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
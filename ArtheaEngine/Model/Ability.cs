using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Reflection;
using ArtheaEngine.Scripting;

namespace ArtheaEngine.Model
{
	/// <summary>
	/// Ability.
	/// </summary>
    public class Ability : ICloneable, INamedEntity, IRootLevelDocument
    {
		private string _script;

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Ability"/> class.
		/// </summary>
		public Ability()
		{	
			Id = ObjectId.GenerateNewId().ToString();
		}

        #region Properties

		[BsonRequired]
		public string Script {
			get { return _script; }
			protected set { 
				_script = value;
				using(var script = new ScriptEngine(new ScriptContext(this)))
				{
					script.Run(value);
				}
			}
		}

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

		public int Mana {
			get;
			set;
		}

		[BsonIgnore]
		public Func<Character, bool> CanUse { get; set; }

		[BsonIgnore]
		public Action<Character, Argument> Execute { get; set; }

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

		public string MsgOffSelf { get; set; }
		public string MsgOffChar { get; set; }
		public string MsgOffObj { get; set; }
		public string MsgOffRoom { get; set; }

		/// <summary>
		/// Displaies the off message.
		/// </summary>
		/// <param name='from'>
		/// From.
		/// </param>
        public void DisplayOffMsg(object from)
        {
            if (from == null)
                return;

            if (from is Character)
            {
                var ch = from as Character;
                ch.WriteLine(MsgOffSelf);
                ch.Room.Act(MsgOffChar);
            }
            else if (from is Object)
            {
                var item = from as Object;
                if (item.CarriedBy != null || item.Room != null)
                {
                    if (item.CarriedBy != null)
                    {
                        item.CarriedBy.Act(MsgOffObj);
                    }
                    else if (item.Room != null)
                    {
                        item.Room.Act(MsgOffObj);
                    }
                }
            }
            else if (from is Room)
            {
                var room = from as Room;
                room.Act(MsgOffRoom);
            }
        }

        #endregion Methods

		public static bool Interpret (Character ch, Argument argument)
		{
			var arg = argument.Peek ();
			
			var ability = DbManager.Find<Ability> (arg);
			
			if (ability != null) {
				
				argument.Next();
				
				ability.Execute(ch, argument);
				return true;
			}
			return false;
		}
    }
}
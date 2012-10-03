using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArtheaEngine.Model
{

    public class Social : INamedEntity, IRootLevelDocument
    {
		public Social() {
			
			Id = ObjectId.GenerateNewId().ToString();
		}

        #region Properties

		[BsonIgnoreIfNull]
        public string CharFound
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string CharNoArg
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string CharNotFound
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string CharObjFound
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string CharSelf
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string Name
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string OthersFound
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string OthersNoArg
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string OthersObjFound
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string OthersSelf
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string VictFound
        {
            get; set;
        }

        #endregion Properties

        #region Methods
		
		public bool Save ()
		{
			return DbManager.Save (this);
		}
		
		public bool Delete ()
		{
			return DbManager.Delete (this);
		}

        public static bool Interpret (Character ch, Argument argument)
		{
			var arg = argument.Peek ();

			var social = Lookup (arg);

			if (social != null) {

				argument.Next();

				social.Parse(ch, argument);
				return true;
			}
            return false;
        }

		public static Social Lookup (String name)
		{
			var regex = new Regex("^" + name, RegexOptions.IgnoreCase);

			return DbManager.Query<Social>().Where(x => regex.IsMatch(x.Name)).FirstOrDefault();
		}

        public virtual void Parse(Character ch, Argument argument)
        {
            if (argument.IsEmpty())
            {
                ch.Room.Act(OthersNoArg, ch);
                ch.Act(CharNoArg);
                return;
            }

            var victim = ch.Room.Characters.Get(c => c.Name.HasWord(argument.ToString()));

            if (victim == null)
            {
                var obj = ch.Room.Objects.Get(o => o.Name.HasWord(argument));

                if (obj == null)
                {
                    ch.WriteLine("That person or object is not here.");
                    return;
                }

                ch.Act(CharObjFound, obj);
                ch.Room.Act(OthersObjFound, ch, obj);
                return;
            }

            if (victim == ch)
            {
                ch.Act(CharSelf);
                ch.Act(OthersSelf, ch);
                return;
            }

            ch.Act(CharFound, victim);
            victim.Act(VictFound, ch);
            victim.Room.Act(new []{ ch, victim}, OthersFound, ch, victim);
            return;
        }

        #endregion Methods
    }
}
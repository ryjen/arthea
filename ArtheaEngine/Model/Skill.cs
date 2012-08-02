using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtheaEngine.Model
{
    public class KickSkill : Skill
    {
		public KickSkill ()
		{
			Name = "Kick";
		}
        #region Methods

        public override bool CanUse(Character ch)
        {
            return true;
        }

        public override void Execute(Character ch, Argument argument)
        {
            ch.WriteLine("You can't kick");
        }

        #endregion Methods
    }

	[BsonKnownTypes(typeof(KickSkill))]
	[BsonDiscriminator(Required = true, RootClass = false)]
    public abstract class Skill : Ability, ICommand
    {
        #region Methods

        public abstract bool CanUse(Character ch);

        public abstract void Execute(Character ch, Argument argument);

        #endregion Methods
    }
}
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;

using System.Linq;

namespace ArtheaEngine.Model
{
    public class HealSpell : Spell
    {
    }

	[BsonKnownTypes(typeof(HealSpell))]
	[BsonDiscriminator(Required = true, RootClass = false)]
    public abstract class Spell : Ability
    {
        #region Properties

		[BsonRequired]
        public int Mana
        {
            get; set;
        }

        #endregion Properties

    }
}
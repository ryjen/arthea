namespace ArtheaEngine.Model
{
    using System.Linq;

    public class HealSpell : Spell
    {
    }

    public abstract class Spell : Ability
    {
        #region Properties

        public override int Id
        {
            get { return 2; }
        }

        public int Mana
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static Spell Lookup(string name)
        {
            return List.FirstOrDefault((a) => a is Spell && a.Name.HasWord(name)) as Spell;
        }

        #endregion Methods
    }
}
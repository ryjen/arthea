namespace ArtheaEngine.Model
{
    public class KickSkill : Skill
    {
        #region Properties

        public override int Id
        {
            get { return 1; }
        }

        #endregion Properties

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

    public abstract class Skill : Ability, ICommand
    {
        #region Methods

        public abstract bool CanUse(Character ch);

        public abstract void Execute(Character ch, Argument argument);

        #endregion Methods
    }
}
namespace ArtheaEngine
{
    using System;
    using System.Runtime.Serialization;

    using ArtheaEngine.Model;

    public interface ICommand
    {
        #region Properties

        string Description
        {
            get;
        }

        string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool CanUse(Character ch);

        void Execute(Character ch, Argument argument);

        #endregion Methods
    }

}
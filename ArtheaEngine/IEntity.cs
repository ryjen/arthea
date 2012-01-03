namespace ArtheaEngine
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public interface IEntity<T>
    {
        #region Properties

        T Id
        {
            get;
        }

        #endregion Properties
    }
}
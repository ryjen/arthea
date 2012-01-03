namespace ArtheaEngine
{
    using System;

    using ArtheaEngine.Model;

    #region Enumerations

    [Flags]
    public enum ConnectionFlag
    {
        None,
        SuppressEcho,
        SuppressGoAhead,
        ColorOff
    }

    #endregion Enumerations

    public interface IConnection : IWritable, IDisposable
    {
        #region Properties

        ConnectionFlag Flags
        {
            get; set;
        }

        Player Playing
        {
            get; set;
        }

        short ScreenHeight
        {
            get;
        }

        short ScreenWidth
        {
            get;
        }

        World World
        {
            get;
        }

        #endregion Properties
    }
}
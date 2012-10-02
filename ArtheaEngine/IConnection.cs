using System;

using ArtheaEngine.Model;

namespace ArtheaEngine
{

    #region Enumerations

	/// <summary>
	/// Connection flag.
	/// </summary>
    [Flags]
    public enum ConnectionFlag
    {
        None,
        SuppressEcho,
        SuppressGoAhead,
        ColorOff
    }

    #endregion Enumerations

	/// <summary>
	/// Connection interface.
	/// </summary>
    public interface IConnection : IWritable, IDisposable
    {
        #region Properties

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
        ConnectionFlag Flags
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the playing character.
		/// </summary>
		/// <value>
		/// The playing character.
		/// </value>
        Player Playing
        {
            get; set;
        }

		/// <summary>
		/// Gets the height of the screen.
		/// </summary>
		/// <value>
		/// The height of the screen.
		/// </value>
        short ScreenHeight
        {
            get;
        }

		/// <summary>
		/// Gets the width of the screen.
		/// </summary>
		/// <value>
		/// The width of the screen.
		/// </value>
        short ScreenWidth
        {
            get;
        }

		/// <summary>
		/// Gets the world.
		/// </summary>
		/// <value>
		/// The world.
		/// </value>
        World World
        {
            get;
        }

        #endregion Properties
    }
}
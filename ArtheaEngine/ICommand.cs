using System;
using System.Runtime.Serialization;

using ArtheaEngine.Model;

namespace ArtheaEngine
{
	/// <summary>
	/// Command interface.
	/// </summary>
    public interface ICommand
    {
        #region Properties

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
        string Description
        {
            get;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

		/// <summary>
		/// Determines whether this instance can use the specified ch.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance can use the specified ch; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='ch'>
		/// Ch.
		/// </param>
        bool CanUse(Character ch);

		/// <summary>
		/// Execute the specified ch and argument.
		/// </summary>
		/// <param name='ch'>
		/// Ch.
		/// </param>
		/// <param name='argument'>
		/// Argument.
		/// </param>
        void Execute(Character ch, Argument argument);

        #endregion Methods
    }

}
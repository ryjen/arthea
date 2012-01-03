namespace Thought.Net.Telnet
{
    using System;

    /// <summary>
    ///     Event arguments for a telnet command event.
    /// </summary>
    public class TelnetCommandEventArgs : EventArgs
    {
        #region Fields

        private readonly TelnetCommand command;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initializes the structure with the specified command.
        /// </summary>
        /// <param name="command">
        ///     The telnet command specific to the event.
        /// </param>
        public TelnetCommandEventArgs(TelnetCommand command)
        {
            this.command = command;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     The command that caused the event.
        /// </summary>
        public TelnetCommand Command
        {
            get { return command; }
        }

        #endregion Properties
    }
}
namespace Thought.Net.Telnet
{
    using System;

    /// <summary>
    ///     Base class for events related to telnet option negotiations.
    /// </summary>
    public class TelnetOptionEventArgs : EventArgs
    {
        #region Fields

        private readonly byte[] data;
        private readonly TelnetOption option;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Creates a new instance of the event arguments structure.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        /// <param name="command">the command</param>
        public TelnetOptionEventArgs(TelnetCommand command, TelnetOption option)
        {
            Command = command;
            this.option = option;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetOptionEventArgs"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        public TelnetOptionEventArgs(TelnetCommand command, TelnetOption option, byte[] data)
            : this(command, option)
        {
            this.data = data;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     Indicates the client agrees to implement a protocol option.
        /// </summary>
        public bool Agreed
        {
            get; set;
        }

        /// <summary>
        ///     The command (e.g. WILL, DONT, etc) for the option.
        /// </summary>
        public TelnetCommand Command
        {
            get; set;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public byte[] Data
        {
            get { return data; }
        }

        /// <summary>
        ///     The <see cref="TelnetOption"/> related to the event..
        /// </summary>
        /// <seealso cref="TelnetOption"/>
        public TelnetOption Option
        {
            get { return option; }
        }

        #endregion Properties
    }
}
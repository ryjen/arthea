namespace Thought.Net.Telnet
{
    using System;

    /// <summary>
    ///     A chunk of information from a Telnet stream.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The Telnet protocol does not have "packets" in the sense that some
    ///         protocols communicate through distict packets or frames.  The term
    ///         packet is for the purposes of the class library only.
    ///     </para>
    /// </remarks>
    public class TelnetPacket
    {
        #region Fields

        private readonly TelnetCommand? command;
        private readonly byte[] data;
        private readonly TelnetOption? option;
        private readonly TelnetPacketType packetType;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initializes a packet of regular data.
        /// </summary>
        /// <param name="data">
        ///     A byte array containing data destined for the user.
        /// </param>
        public TelnetPacket(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.data = (byte[]) data.Clone();
            packetType = TelnetPacketType.Data;
        }

        /// <summary>
        ///     Initializes a new data packet containing a subset
        ///     of the bytes from a byte array.
        /// </summary>
        /// <param name="data">
        ///     A byte array containing the data to copy to the packet.
        /// </param>
        /// <param name="offset">
        ///     The zero-based offset in the source array at which
        ///     to begin copying bytes to the packet.
        /// </param>
        /// <param name="length">
        ///     The number of bytes to copy to the packet.
        /// </param>
        public TelnetPacket(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if ((offset < 0) || (offset >= data.Length))
                throw new ArgumentOutOfRangeException("offset");

            if ((length < 1) || (offset + length > data.Length))
                throw new ArgumentOutOfRangeException("data");

            this.data = new byte[length];
            Array.Copy(
                data,
                offset,
                this.data,
                0,
                length);
        }

        /// <summary>
        ///     Initializes the Telnet packet that represents a command.
        /// </summary>
        /// <param name="command">
        ///     Any telnet command except for those requiring options or data parameters.
        /// </param>
        public TelnetPacket(TelnetCommand command)
        {
            // Some telnet commands require additional information.
            // An exception is raised if one of these commands is specified.
            // The caller should be using a different constructor.

            switch (command)
            {
                case TelnetCommand.Do:
                case TelnetCommand.Dont:
                case TelnetCommand.Will:
                case TelnetCommand.Wont:

                    // The DO, DONT, WILL and WONT commands
                    // require an option code.

                    throw new ArgumentException();
            }

            packetType = TelnetPacketType.Command;
            this.command = command;
        }

        /// <summary>
        ///     Initializes the telnet packet with the specified
        ///     command and option code.
        /// </summary>
        /// <param name="command">
        ///     Any command that uses an option code (e.g. WILL, DO, etc).
        /// </param>
        /// <param name="option">
        ///     The option code.
        /// </param>
        public TelnetPacket(TelnetCommand command, TelnetOption option)
            : this(command, (byte) option)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetPacket"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        public TelnetPacket(TelnetCommand command, TelnetOption option, byte[] data)
            : this(command, (byte) option)
        {
            this.data = data;
        }

        /// <summary>
        ///     Initializes the telnet packet with the specified
        ///     command and option byte.
        /// </summary>
        /// <param name="command">
        ///     Any command that uses an option (e.g. WILL, DO, etc).
        /// </param>
        /// <param name="option">
        ///     The option value.
        /// </param>
        public TelnetPacket(TelnetCommand command, byte option)
        {
            switch (command)
            {
                case TelnetCommand.Do:
                case TelnetCommand.Dont:
                case TelnetCommand.Will:
                case TelnetCommand.Wont:

                    packetType = TelnetPacketType.Command;
                    this.command = command;
                    this.option = (TelnetOption) option;
                    break;
                case TelnetCommand.Subnegotiation:
                    packetType = TelnetPacketType.Subnegotiation;
                    this.command = command;
                    this.option = (TelnetOption) option;
                    break;

                default:

                    // The caller specified a command type that does not
                    // require an option code.  The caller should be using
                    // a different constructor.

                    throw new ArgumentException();
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     The command of the Telnet packet.
        /// </summary>
        public TelnetCommand? Command
        {
            get { return command; }
        }

        /// <summary>
        ///     Any data associated with the packet.
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        /// <summary>
        ///     The option designed in certain commands or subnegotiations.
        /// </summary>
        public TelnetOption? Option
        {
            get { return option; }
        }

        /// <summary>
        ///     The type of packet.
        /// </summary>
        public TelnetPacketType PacketType
        {
            get { return packetType; }
        }

        #endregion Properties
    }
}
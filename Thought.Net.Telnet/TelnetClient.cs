namespace Thought.Net.Telnet
{
    using System;
    using System.Collections;
    using System.Net.Sockets;

    /// <summary>
    ///     Implements a Telnet client based on RFC 854 (Telnet Protocol
    ///     Specification).
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         From the perspective of the protocol, both sides of
    ///         connections are peers -- neither is a true "server"
    ///         in the sense that it controls the flow of the protocol.
    ///         This is very different than command-oriented protocols like
    ///         POP3 or SMTP in which a client sends commands to the server
    ///         and receives responses.  In Telnet, both sides can send
    ///         commands and responses to each other.  Commands and
    ///         responses are embedded in the data being transferred.
    ///     </para>
    /// 
    /// </remarks>
    public class TelnetClient
    {
        #region Fields

        /// <summary>
        ///     The default telnet server port (23 decimal).
        /// </summary>
        public const int DefaultPort = 23;

        private readonly object commandReceivedLock = new object();
        private readonly object dontReceivedLock = new object();
        private readonly object doReceivedLock = new object();
        private readonly BitArray localOptions;
        private readonly TelnetParser parser;
        private readonly BitArray remoteOptions;
        private readonly object subNegRecievedLock = new object();
        private readonly object willReceivedLock = new object();
        private readonly object wontReceivedLock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Creates a telnet client against a connected socket.
        /// </summary>
        /// <param name="socket">
        ///     The socket to be used for sending and receiving data.
        /// </param>
        public TelnetClient(Socket socket)
            : this()
        {
            parser = new TelnetParser(socket);
        }

        /// <summary>
        ///     Initializes the basic Telnet options (private).
        /// </summary>
        /// <remarks>
        ///     The default constructor is marked as private to ensure
        ///     that only the parameterized constructors are called.  This
        ///     may change in a future version of the software.
        /// </remarks>
        private TelnetClient()
        {
            localOptions = new BitArray(256, false);
            remoteOptions = new BitArray(256, false);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        ///     Raised when a command is received by the remote system.
        /// </summary>
        public event TelnetCommandEventHandler CommandReceived
        {
            add
            {
                lock (commandReceivedLock)
                {
                    commandReceived += value;
                }
            }
            remove
            {
                lock (commandReceivedLock)
                {
                    commandReceived -= value;
                }
            }
        }

        /// <summary>
        ///     Raised when a DONT command is received.
        /// </summary>
        /// <seealso cref="DoReceived"/>
        /// <seealso cref="WillReceived"/>
        /// <seealso cref="WontReceived"/>
        public event TelnetOptionEventHandler DontReceived
        {
            add
            {
                lock (dontReceivedLock)
                {
                    dontReceived += value;
                }
            }
            remove
            {
                lock (dontReceivedLock)
                {
                    dontReceived -= value;
                }
            }
        }

        /// <summary>
        ///     Raised when a DO command is received.
        /// </summary>
        /// <seealso cref="DontReceived"/>
        /// <seealso cref="WillReceived"/>
        /// <seealso cref="WontReceived"/>
        public event TelnetOptionEventHandler DoReceived
        {
            add
            {
                lock (doReceivedLock)
                {
                    doReceived += value;
                }
            }
            remove
            {
                lock (doReceivedLock)
                {
                    doReceived -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when [subnegotiation received].
        /// </summary>
        public event TelnetOptionEventHandler SubnegotiationReceived
        {
            add
            {
                lock (subNegRecievedLock)
                {
                    subnegotiationReceived += value;
                }
            }
            remove
            {
                lock (subNegRecievedLock)
                {
                    subnegotiationReceived -= value;
                }
            }
        }

        /// <summary>
        ///     Raised when a WILL command is received.
        /// </summary>
        public event TelnetOptionEventHandler WillReceived
        {
            add
            {
                lock (willReceivedLock)
                {
                    willReceived += value;
                }
            }
            remove
            {
                lock (willReceivedLock)
                {
                    willReceived -= value;
                }
            }
        }

        /// <summary>
        ///     Raised when a WONT command is received.
        /// </summary>
        public event TelnetOptionEventHandler WontReceived
        {
            add
            {
                lock (wontReceivedLock)
                {
                    wontReceived += value;
                }
            }
            remove
            {
                lock (wontReceivedLock)
                {
                    wontReceived -= value;
                }
            }
        }

        // Events:
        private event TelnetCommandEventHandler commandReceived;

        private event TelnetOptionEventHandler dontReceived;

        private event TelnetOptionEventHandler doReceived;

        private event TelnetOptionEventHandler subnegotiationReceived;

        private event TelnetOptionEventHandler willReceived;

        private event TelnetOptionEventHandler wontReceived;

        #endregion Events

        #region Properties

        /// <summary>
        ///     The underlying parser of the telnet protocol.
        /// </summary>
        /// <remarks>
        ///     The parser can be used to send advanced Telnet commands
        ///     that are not supported by the TelnetClient class.  
        /// </remarks>
        public TelnetParser Parser
        {
            get { return parser; }
        }

        #endregion Properties

        #region Methods

        // Event object locks:
        /// <summary>
        ///     Terminates the telnet connection.
        /// </summary>
        /// <remarks>
        ///     This can be called multiple times without raising an exception.
        /// </remarks>
        public void Close()
        {
            parser.Close();
        }

        /// <summary>
        ///     Checks if the client is implementing an option.
        /// </summary>
        /// <param name="option">
        ///     The Telnet option code.
        /// </param>
        /// <returns>
        ///     True if the local client is implementing an option.    
        /// </returns>
        public bool DoingLocalOption(byte option)
        {
            return localOptions[option];
        }

        /// <summary>
        ///     Checks if the remote client is doing an option.
        /// </summary>
        /// <param name="option">
        ///     The Telnet option code.
        /// </param>
        /// <returns>
        ///     True if the remote client is implementing an option.
        /// </returns>
        public bool DoingRemoteOption(byte option)
        {
            return remoteOptions[option];
        }

        /// <summary>
        ///     Reads data from the remote telnet client.
        /// </summary>
        /// <param name="buffer">The buffer in which bytes are to be copied.</param>
        /// <param name="offset">The byte offset in the buffer at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to be read.</param>
        /// <returns>
        ///     The number of data bytes read from the remote system.
        /// </returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            TelnetPacket packet;

            do
            {
                packet = parser.ReadNext(count);

                if (packet.PacketType == TelnetPacketType.Command)
                {
                    switch (packet.Command)
                    {
                        case TelnetCommand.Do:
                            OnDoOption(packet.Option.Value);
                            break;

                        case TelnetCommand.Dont:
                            OnDontOption(packet.Option.Value);
                            break;

                        case TelnetCommand.Will:
                            OnWillOption(packet.Option.Value);
                            break;

                        case TelnetCommand.Wont:
                            OnWontOption(packet.Option.Value);
                            break;

                        case TelnetCommand.AbortOutput:
                        case TelnetCommand.AreYouThere:
                        case TelnetCommand.Break:
                        case TelnetCommand.DataMark:
                        case TelnetCommand.EraseCharacter:
                        case TelnetCommand.EraseLine:
                        case TelnetCommand.GoAhead:
                        case TelnetCommand.InterruptProcess:
                        case TelnetCommand.NoOperation:
                            OnCommandReceived(packet.Command.Value);
                            break;
                    }
                }
                else if (packet.PacketType == TelnetPacketType.Subnegotiation)
                {
                    OnSubnegotiationReceived(packet.Option.Value, packet.Data);
                }
            } while (packet.PacketType != TelnetPacketType.Data);

            Buffer.BlockCopy(
                packet.Data,
                0,
                buffer,
                offset,
                packet.Data.Length);

            return packet.Data.Length;
        }

        /// <summary>
        ///     Sends an IAC DO sequence to the remote system.
        /// </summary>
        /// <param name="option">The <see cref="TelnetOption"/> to send.</param>
        /// <seealso cref="SendDont"/>
        /// <seealso cref="SendWill"/>
        /// <seealso cref="SendWont"/>
        public void SendDo(TelnetOption option)
        {
            parser.SendDo((byte) option);
        }

        /// <summary>
        ///     Sends an IAC DONT sequence to the remote system.
        /// </summary>
        /// <param name="option">The <see cref="TelnetOption"/> to send.</param>
        /// <seealso cref="SendDo"/>
        /// <seealso cref="SendWill"/>
        /// <seealso cref="SendWont"/>
        public void SendDont(TelnetOption option)
        {
            parser.SendDont((byte) option);
        }

        /// <summary>
        ///     Sends an IAC GA (Go Ahead) command to the remote system.
        /// </summary>
        public void SendGoAhead()
        {
            parser.SendGoAhead();
        }

        /// <summary>
        /// Sends the subnegotiation.
        /// </summary>
        /// <param name="option">The option.</param>
        public void SendSubnegotiation(TelnetOption option)
        {
            parser.SendSubnegotiation(option);
        }

        /// <summary>
        ///     Sends an IAC WILL sequence to the remote system.
        /// </summary>
        /// <param name="option">The <see cref="TelnetOption"/> to send.</param>
        /// <seealso cref="SendDo"/>
        /// <seealso cref="SendDont"/>
        /// <seealso cref="SendWont"/>
        public void SendWill(TelnetOption option)
        {
            parser.SendWill((byte) option);
        }

        /// <summary>
        ///     Sends an IAC WONT sequence to the remote system.
        /// </summary>
        /// <param name="option">The <see cref="TelnetOption"/> to send.</param>
        /// <seealso cref="SendDo"/>
        /// <seealso cref="SendDont"/>
        /// <seealso cref="SendWill"/>
        public void SendWont(TelnetOption option)
        {
            parser.SendWont((byte) option);
        }

        /// <summary>
        ///     Writes a byte to the client.
        /// </summary>
        /// <param name="value">
        ///     The byte to send to the client.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This is not an efficient function for it will probably
        ///         trigger a full TCP or underlying network packet to send
        ///         the single byte.  Always try to send data in the largest
        ///         possible chunks.
        ///     </para>
        ///     <para>
        ///         Reserved characters are encoded or escaped prior to sending.
        ///     </para>
        /// </remarks>
        public void Write(byte value)
        {
            parser.SendEscaped(new[] {value});
        }

        /// <summary>
        ///     Escapes and sends a buffer of bytes to the remote system.
        /// </summary>
        /// <param name="buffer">
        ///     A buffer containing raw data to send to the remote system.
        /// </param>
        /// <param name="offset">
        ///     The offset in buffer in which to begin sending.
        /// </param>
        /// <param name="count">
        ///     The number of bytes to send.
        /// </param>
        public void Write(byte[] buffer, int offset, int count)
        {
            parser.SendEscaped(buffer, offset, count);
        }

        /// <summary>
        ///     Raises the <see cref="CommandReceived"/> event.
        /// </summary>
        /// <param name="command">
        ///     Any Telnet command that does not require additional parameters.
        /// </param>
        protected void OnCommandReceived(TelnetCommand command)
        {
            var args = new TelnetCommandEventArgs(command);

            if (commandReceived != null)
                commandReceived(this, args);
        }

        /// <summary>
        ///     Raises the <see cref="DontReceived"/> event.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        /// <seealso cref="DontReceived"/>
        /// <seealso cref="OnDoOption"/>
        /// <seealso cref="OnWillOption"/>
        /// <seealso cref="OnWontOption"/>
        protected void OnDontOption(TelnetOption option)
        {
            // The client (this computer) received a DONT command.
            // Raise an event and notify the programmer.  The programmer
            // can set the Agreed option if he/she agrees to not
            // implement an option.

            var args =
                new TelnetOptionEventArgs(TelnetCommand.Dont, option);

            if (dontReceived != null)
                dontReceived(this, args);

            if (args.Agreed)
            {
                // The programmer agrees to not implement an option.
                // If this was already agreed to earlier, then a response
                // should not be sent to the client.  This is intentional
                // in order to avoid an infinite loop (some clients blindly
                // send a DONT in response to a WONT).

                if (localOptions[(int) option])
                {
                    localOptions[(int) option] = false;
                    SendWont(option);
                }
            }
            else
            {
                // The programmer disagrees to comply with the DONT command.

                // Not implemented.
            }
        }

        /// <summary>
        ///     Raises the <see cref="DoReceived"/> event.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        /// <seealso cref="DoReceived"/>
        /// <seealso cref="OnDontOption"/>
        /// <seealso cref="OnWillOption"/>
        /// <seealso cref="OnWontOption"/>
        protected void OnDoOption(TelnetOption option)
        {
            var args =
                new TelnetOptionEventArgs(TelnetCommand.Do, option);

            if (doReceived != null)
                doReceived(this, args);

            if (args.Agreed)
            {
                // The programmer agreed to implement the option.
                // If the remote system already sent a DO earlier, then
                // a WILL response is not sent.  This avoids an infinite
                // loop because some clients will blindly send a WILL
                // in response to a DO.

                if (!localOptions[(int) option])
                {
                    localOptions[(int) option] = true;
                    parser.SendWill(option);
                }
            }
            else
            {
                // The programmer refuses to implement the option.

                localOptions[(int) option] = false;
                parser.SendWont(option);
            }
        }

        /// <summary>
        /// Called when [subnegotiation received].
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        protected void OnSubnegotiationReceived(TelnetOption option, byte[] data)
        {
            var args = new TelnetOptionEventArgs(TelnetCommand.Subnegotiation, option, data);

            if (subnegotiationReceived != null)
                subnegotiationReceived(this, args);
        }

        /// <summary>
        ///     Raises the <see cref="WillReceived"/> event.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        /// <seealso cref="WillReceived"/>
        /// <seealso cref="OnDoOption"/>
        /// <seealso cref="OnDontOption"/>
        /// <seealso cref="OnWontOption"/>
        protected void OnWillOption(TelnetOption option)
        {
            var args =
                new TelnetOptionEventArgs(TelnetCommand.Will, option);

            if (willReceived != null)
                willReceived(this, args);

            if (args.Agreed != remoteOptions[(int) option])
            {
                // If the remote system is already doing the
                // option, then a DO reply should not be sent.
                // This might cause an infinite loop -- some
                // clients will blindly send another WILL
                // whenever a DO is received.

                remoteOptions[(int) option] = args.Agreed;

                if (args.Agreed)
                {
                    parser.SendDo(option);
                }
                else
                {
                    parser.SendDont(option);
                }
            }
        }

        /// <summary>
        ///     Raises the <see cref="WontReceived"/> event.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        /// <seealso cref="WillReceived"/>
        /// <seealso cref="OnDoOption"/>
        /// <seealso cref="OnDontOption"/>
        /// <seealso cref="OnWillOption"/>
        protected void OnWontOption(TelnetOption option)
        {
            // The client (this computer) received a WONT option
            // from the remote system.  First raise an event to
            // notify the programmer.  The programmer can agree
            // that the remote system should not implement an
            // option by setting the Agreed property of the event args.

            var args =
                new TelnetOptionEventArgs(TelnetCommand.Wont, option);

            if (wontReceived != null)
                wontReceived(this, args);

            // If the remote system has already refused to
            // do the option, then a DONT reply will not be
            // sent.  This will cause an infinite loop if
            // the clint blindly sends a WONT in response
            // to a DONT.

            if (remoteOptions[(int) option])
            {
                parser.SendDont(option);
                remoteOptions[(int) option] = false;
            }
            else
            {
                // The remote system was not doing the
                // option previously.  No reply will be sent.
            }
        }

        #endregion Methods
    }
}
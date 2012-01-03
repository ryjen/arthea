namespace ArtheaServer
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using ArtheaServer.Commands;
    using ArtheaServer.Creation;

    using NLog;

    using Thought.Net.Telnet;

    public class TelnetConnection : IConnection
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly StreamReader _reader;
        private readonly ColorWriter _writer;

        private string _pageBuf;
        private int _pagePos;
        private byte _passwordCount;
        private StateHandler _state;

        #endregion Fields

        #region Constructors

        public TelnetConnection(TelnetClient client, TelnetServer server)
        {
            Server = server;
            Client = client;
            client.WillReceived += TelnetWillRecieved;
            client.DoReceived += TelnetDoRecieved;
            client.DontReceived += TelnetDontRecieved;
            client.WontReceived += TelnetWontRecieved;
            client.SubnegotiationReceived += TelnetSubNegotiationRecieved;
            var stream = new TelnetStream(client);
            _writer = new ColorWriter(stream, this) {AutoFlush = true};
            _reader = new StreamReader(stream);
            ScreenHeight = 24;
            ScreenWidth = 80;
        }

        #endregion Constructors

        #region Delegates

        private delegate void StateHandler(string argument);

        #endregion Delegates

        #region Properties

        public Account Account
        {
            get; set;
        }

        public TelnetClient Client
        {
            get; set;
        }

        public bool Connected
        {
            get { return Client.Parser.Socket.Connected; }
        }

        public Editor Editing
        {
            get; set;
        }

        public ConnectionFlag Flags
        {
            get; set;
        }

        public Player Playing
        {
            get; set;
        }

        public short ScreenHeight
        {
            get; private set;
        }

        public short ScreenWidth
        {
            get; private set;
        }

        public TelnetServer Server
        {
            get; set;
        }

        public string TerminalType
        {
            get; private set;
        }

        public World World
        {
            get { return Server.World; }
        }

        #endregion Properties

        #region Methods

        public void ClearScreen()
        {
            Write("\x1b[2J");
        }

        public void Close()
        {
            Client.Close();
            SetState(null);
        }

        public void DisplayPrompt()
        {
            if (HasPage())
            {
                WriteLine();
                Write("({0}%) (H)elp, (R)efresh page, (P)revious page, or hit ENTER: ",
                    100 * _pageBuf.CountLines(_pagePos) / _pageBuf.CountLines());
            }
            if (Editing != null)
            {

                WriteLine();
                Editing.Prompt();
                return;
            }

            WriteLine();
            Playing.DisplayPrompt();
        }

        /// <summary>
        ///                     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Server.World.Connections.Remove(this);
        }

        public bool HasPage()
        {
            return _pagePos > 0;
        }

        public void Header(string value)
        {
            _writer.WriteLine("~!B~W{0}~x", value.Center(ScreenWidth));
        }

        public bool IsPlaying()
        {
            return Playing != null && (_state == this.PlayingState);
        }

        public void Page(string buf)
        {
            if (HasPage())
            {
                _pageBuf += buf;
            }
            else
            {
                _pageBuf = buf.ToString();
                ShowPage(string.Empty);
            }
        }

        /// <summary>
        /// Requests the client does not echo.
        /// </summary>
        public void RequestEchoOff()
        {
            if (Flags.HasFlag(ConnectionFlag.SuppressGoAhead))
                Client.SendGoAhead();
            Flags |= ConnectionFlag.SuppressEcho;
            Client.SendWill(TelnetOption.Echo);
        }

        /// <summary>
        /// Requests the client echo's.
        /// </summary>
        public void RequestEchoOn()
        {
            if (Flags.HasFlag(ConnectionFlag.SuppressGoAhead))
                Client.SendGoAhead();
            Flags &= ~ConnectionFlag.SuppressEcho;
            Client.SendWont(TelnetOption.Echo);
        }

        public void RestoreCursor()
        {
            Write("\x1b8");
        }

        public void Run()
        {
            WriteLine("Welcome to " + Server.World.Name + "!");

            RequestEchoOn();

            Write("Account Login: ");
            SetState(GetAccountName);

            while (Server.State != ServerState.Stopped && _state != null && Connected)
            {
                try
                {
                    var line = _reader.ReadLine();

                    _state.Invoke(line);

                    if (Connected && IsPlaying())
                        DisplayPrompt();
                }
                catch (IOException)
                {
                    break;
                }
            }

            Dispose();
        }

        public void SaveCursor()
        {
            Write("\x1b7");
        }

        public void SetCursor(int x, int y)
        {
            Write("\x1b[{0};{1}H", x, y);
        }

        public void SetCursor()
        {
            Write("\x1b[H");
        }

        public void ShowPage(string input)
        {
            if (input.Length > 0)
            {
                _pagePos = -1;
                return;
            }

            var length = _pageBuf.GetLineCountLength(_pagePos, ScreenHeight - 1);

            if (length > 0)
            {
                Write(_pageBuf.Substring(_pagePos, length));
            }
            else
            {
                Write(_pageBuf);
                _pagePos = -1;
            }
        }

        public void Write(string value)
        {
            _writer.Write(value);
        }

        public void Write(string format, params object[] args)
        {
            _writer.Write(format, args);
        }

        public void Write(byte[] value)
        {
            Client.Write(value, 0, value.Length);
        }

        public void Write(object obj)
        {
            _writer.Write(obj);
        }

        public void WriteLine(string value)
        {
            _writer.WriteLine(value);
        }

        public void WriteLine(string format, params object[] args)
        {
            _writer.WriteLine(format, args);
        }

        public void WriteLine()
        {
            _writer.WriteLine();
        }

        public void WriteLine(object obj)
        {
            _writer.WriteLine(obj);
        }

        private static void TelnetDontRecieved(object o, TelnetOptionEventArgs args)
        {
            //Log.Trace("Recieved Dont " + args.Option);
        }

        private static void TelnetWontRecieved(object o, TelnetOptionEventArgs args)
        {
            //Log.Trace("Recieved Wont " + args.Option);
        }

        private void ConfirmDeleteAccount(string argument)
        {
            RequestEchoOn();

            if (!Account.Password.ToArray().DeepCompare(Encoding.Default.GetBytes(argument)))
            {

                    Write("~CPasswords do not match. Account deletion cancelled.");
                    SetState(PlayingState);

                return;
            }

            WriteLine("~CSorry to see you go! Safe travels!~x");

            Account.Delete();

            Close();
            Dispose();
        }

        private void ConfirmNewAccount(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                ConfirmNewAccount("Y");
                return;
            }
            switch (char.ToUpper(argument[0]))
            {
                default:
                    Write("~CEnter a password for ~W{0}~C:~x ", Account.Login);
                    SetState(GetNewPassword);
                    RequestEchoOff();
                    break;
                case 'N':
                    Write("~CLogin:~x ");
                    Account = null;
                    SetState(GetAccountName);
                    break;
            }
        }

        private void ConfirmNewCharName(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                ConfirmNewCharName("Y");
                return;
            }

            switch (char.ToUpper(argument[0]))
            {
                default:
                    DisplayRaces();
                    Write("~CWhat is your race?~x ");
                    SetState(GetCharRace);
                    break;

                case 'N':
                    Write("~COk, what is your characters name then?~x ");
                    Playing = null;
                    break;
            }
        }

        private void ConfirmNewPassword(string argument)
        {
            if (!Account.Password.ToArray().DeepCompare(Encoding.Default.GetBytes(argument)))
            {
                if (++_passwordCount > 3)
                {
                    WriteLine("~CToo many password attempts.  Disconnecting.~x");
                    Log.Warn("Too many password attempts for {0}", Account.Login);
                    Client.Close();
                }
                else
                {
                    Write("~CPasswords do not match. Try again ({0}):~x ", _passwordCount);
                }
                return;
            }
            WriteLine();
            Write("~CPlease enter a valid email address:~x ");
            SetState(GetEmail);
            RequestEchoOn();
        }

        private void DisplayAccountMenu()
        {
            ClearScreen();

            SetCursor();

            Header("Account Menu");

            WriteLine();
            WriteLine("~Y(C)~Create New Character");
            WriteLine("~Y(D)~Celete account");
            WriteLine("~Y(E)~Cxit game");
            WriteLine();
            int i = 0;
            for (i = 0; i < Account.Players.Count; i++)
            {
                var p = Account.Players.ElementAt(i);
                WriteLine(" ~Y{0}) ~W{1} ~C(lvl {2})~x", i + 1, p.Name, p.Level);
            }
            if (i != 0)
                WriteLine();
            Write("~CYour selection?~x ");
        }

        private void DisplayClasses()
        {
            ClearScreen();

            SetCursor();

            Header("Available Classes");

            WriteLine();
            foreach (var c in Class.List)
            {
                WriteLine("~W{0,-10}~C: {1}", c.Name, c.Description);
            }
            WriteLine("~x");
        }

        private void DisplayMotd()
        {
            WriteLine("* Don't feed the immortals.");
            WriteLine();
            WriteLine("[Hit return to continue]");

            if (Playing.Room == null)
            {
                Playing.Room = Room.GetDefault();
            }
            Playing.Room.Characters.Add(Playing);
            Character.List.Add(Playing);
            if (Playing.Level == 0)
            {
                using (var conn = ArtheaHelper.NewConnection())
                {
                    if (Convert.ToInt32(conn.CreateCommand("count_players").ExecuteScalar()) == 0)
                    {
                        Playing.Level = ArtheaHelper.MaxLevel;
                        Log.Warn("Setting first player " + Playing.Name + " immortal");
                        Playing.WriteLine("You are now level " + Playing.Level + "!");
                        Playing.Save();
                    }
                    else
                    {
                        Playing.Level = 1;
                    }
                }

            }
            Server.World.Connections.Add(this);

            Log.Info(Playing.Name + " has logged in");
        }

        private void DisplayRaces()
        {
            ClearScreen();

            SetCursor();

            Header("Available Races");

            WriteLine();

            foreach (var r in Race.List)
            {
                WriteLine("~W{0,-10}~C: {1}", r.Name, r.Description);
            }

            WriteLine("~x");
        }

        private void GetAccountName(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Write("~CPlease enter a valid account name:~x ");
                return;
            }

            Account = new Account { Connection = this, Login = argument };

            if(Account.Load(argument))
            {
                Write("~CPassword:~x ");
                RequestEchoOff();
                SetState(GetAccountPassword);
                return;
            }

            Write("~CWould you like to create an account for ~W{0}~C? (Y/n)~x", argument.Capitalize());
            SetState(ConfirmNewAccount);
        }

        private void GetAccountPassword(string argument)
        {
            var passwd = Encoding.Default.GetBytes(argument);

            if (!Account.Password.ToArray().DeepCompare(passwd))
            {
                if (++_passwordCount > 3)
                {
                    WriteLine("~CToo many password attempts.  Disconnecting.~x");
                    Log.Warn("Too many password attempts for {0}", Account.Login);
                    Client.Close();
                }
                else
                {
                    Write("Passwords do not match. Try again ({0}): ", _passwordCount);
                }
                return;
            }
            RequestEchoOn();
            DisplayAccountMenu();
            SetState(GetPlayer);
        }

        private void GetCharClass(string argument)
        {
            var cl = Class.Lookup(argument);

            if (cl == null)
            {
                Write("~CNo such class! Try again:~x ");
                return;
            }
            Playing.Class = cl;

            Account.Players.Add(new AccountPlayer { Account = Account, Id = Playing.Id, Name = Playing.Name, Level = Playing.Level });
            DisplayMotd();
            SetState(PlayingState);
        }

        private void GetCharRace(string argument)
        {
            var r = Race.List.OfType<PcRace>().FirstOrDefault(r2 => r2.Name.StartsWith(argument));

                if (r == null)
                {
                    Write("~CNo such race! Try again:~x ");
                    return;
                }
                Playing.Race = r;

            DisplayClasses();
            Write("~CWhat kind of character shall you be?~x ");
            SetState(GetCharClass);
        }

        private void GetEmail(string argument)
        {
            if (!ArtheaHelper.IsEmail(argument))
            {
                Write("~CPlease enter a valid email address:~x ");
                return;
            }

            Account.Email = argument;
            DisplayAccountMenu();
            SetState(GetPlayer);
        }

        private void GetNewCharName(string argument)
        {
            if (string.IsNullOrEmpty(argument) || argument.Length > 12)
            {
                Write("~CPlease enter a valid character name (12 letters or less):~x ");
                return;
            }

            argument = argument.Capitalize();

                if (Character.Lookup<Player>(argument) != null)
                {
                    WriteLine("~CThere is already a player with that name.~x");
                    Write("~CPlease enter another name:~x ");
                    return;
                }

            Playing = new Player
                          {
                              Account = Account,
                              Name = argument,
                          };
            Character.List.Add(Playing);

            Write("~CCreate a character named ~W{0}~C? (Y/n)~x ", Playing.Name);
            SetState(ConfirmNewCharName);
        }

        private void GetNewPassword(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Write("~CPlease enter a valid password:~x ");
                return;
            }

            Account.Password = Encoding.Default.GetBytes(argument);
            WriteLine();
            Write("~CEnter password again:~x ");
            SetState(ConfirmNewPassword);
        }

        private void GetPlayer(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Write("~CPlease select a valid option:~x ");
                return;
            }

            if (char.ToUpper(argument[0]) == 'C')
            {
                Write("~CWhat is your character's name?~x ");
                SetState(GetNewCharName);
                return;
            }

            if (char.ToUpper(argument[0]) == 'D')
            {
                WriteLine("~CAre you sure you want to delete your account?~x");
                Write("~CEnter your password to confirm:~x ");
                SetState(ConfirmDeleteAccount);
                RequestEchoOff();
                return;
            }

            if (char.ToUpper(argument[0]) == 'E')
            {
                WriteLine("~CGood-Bye!~x");
                Close();
                Dispose();
                return;
            }

            if (!argument.IsNumber())
            {
                GetPlayer(null);
                return;
            }

            var option = int.Parse(argument) - 1;

            if (option < 0 || option > Account.Players.Count)
            {
                GetPlayer(null);
                return;
            }

            Playing = Account.Players.ElementAt(option).Load();
            DisplayMotd();
            SetState(PlayingState);
        }

        private void PlayingState(string argument)
        {
            try
            {
                if (HasPage())
                {
                    ShowPage(argument);
                    return;
                }

                if (Editing != null && Editing.Execute(new Argument(argument)))
                {
                    return;
                }

                if (!Command.Interpret(Playing, new Argument(argument)))
                {
                    WriteLine("Huh?");
                }
            }
            catch (QuitCommandException)
            {
                Playing.Dispose();
                Playing = null;
                if (Editing != null)
                    Editing = null;
                Close();

                Dispose();
            }
            catch (SocketException)
            {
                Log.Warn("{0} disconnected", Client.Parser.Socket.RemoteEndPoint);
                Close();
                Dispose();
            }
            catch (ShutdownException)
            {
                TelnetServer.Shutdown();
            }
            catch (RebootException)
            {
                TelnetServer.Reboot();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void SetState(StateHandler newState)
        {
            _state = newState;
        }

        private void TelnetDoRecieved(object o, TelnetOptionEventArgs args)
        {
            switch (args.Option)
            {
                case TelnetOption.Echo:
                    if (Flags.HasFlag(ConnectionFlag.SuppressEcho))
                        args.Agreed = true;
                    break;
            }
        }

        private void TelnetSubNegotiationRecieved(object o, TelnetOptionEventArgs args)
        {
            switch (args.Option)
            {
                case TelnetOption.WindowSize:
                    ScreenHeight = BitConverter.ToInt16(args.Data.Reverse().ToArray(), 0);
                    ScreenWidth = BitConverter.ToInt16(args.Data.Reverse().ToArray(), 2);
                    //Log.Trace("Recieved " + args.Option + " " + ScreenWidth + " " + ScreenHeight);
                    break;
                case TelnetOption.TerminalType:
                    TerminalType = Encoding.Default.GetString(args.Data.Skip(1).ToArray());
                    break;
            }
        }

        private void TelnetWillRecieved(object o, TelnetOptionEventArgs args)
        {
            switch (args.Option)
            {
                case TelnetOption.WindowSize:
                    args.Agreed = true;
                    break;
                case TelnetOption.TerminalType:
                    args.Agreed = true;
                    Client.SendSubnegotiation(TelnetOption.TerminalType);
                    break;
                case TelnetOption.SuppressGoAhead:
                    args.Agreed = true;
                    break;
                case TelnetOption.EndOfRecord:
                    args.Agreed = true;
                    break;
                case TelnetOption.Echo:
                    args.Agreed = true;
                    break;
            }
        }

        #endregion Methods
    }
}
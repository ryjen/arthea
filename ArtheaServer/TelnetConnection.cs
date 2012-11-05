using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ArtheaEngine;
using ArtheaEngine.Model;
using ArtheaServer.Commands;
using ArtheaServer.Creation;
using NLog;
using Thought.Net.Telnet;

namespace ArtheaServer
{
    public class TelnetConnection : IConnection
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly StreamReader _reader;
        private readonly ColorWriter _writer;
        private StringBuilder _pageBuf;
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

        #region Nested type: StateHandler

        private delegate void StateHandler(string argument);

        #endregion

        #region Properties

        public Account Account { get; set; }

        public TelnetClient Client { get; set; }

        public bool Connected
        {
            get { return Client.Parser.Socket.Connected; }
        }

        public Editor Editing { get; set; }
        public TelnetServer Server { get; set; }

        public string TerminalType { get; private set; }

        public Socket Socket
        {
            get { return Client.Parser.Socket; }
        }

        public ConnectionFlag Flags { get; set; }

        public Player Playing { get; set; }

        public short ScreenHeight { get; private set; }

        public short ScreenWidth { get; private set; }

        public World World
        {
            get { return Server.World; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///                     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Server.World.Connections.Remove(this);
        }

        public void Page(string buf)
        {
            if (HasPage())
            {
                _pageBuf.Append(buf);
            }
            else
            {
                _pageBuf.Append(buf);
                ShowPage(string.Empty);
            }
        }

        public void Page(StringBuilder buf)
        {
            if (HasPage())
            {
                _pageBuf.Append(buf);
            }
            else
            {
                _pageBuf = buf;
                ShowPage(string.Empty);
            }
        }

        public void Write(string format, params object[] args)
        {
            if (!Connected)
                return;

            _writer.Write(format, args);
        }

        public void Write(byte[] value)
        {
            if (!Connected)
                return;

            Client.Write(value, 0, value.Length);
        }

        public void Write(object obj)
        {
            if (!Connected)
                return;

            _writer.Write(obj);
        }

        public void WriteLine(string format, params object[] args)
        {
            if (!Connected)
                return;

            _writer.WriteLine(format, args);
        }

        public void WriteLine()
        {
            if (!Connected)
                return;

            _writer.WriteLine();
        }

        public void WriteLine(object obj)
        {
            if (!Connected)
                return;

            _writer.WriteLine(obj);
        }

        public string State
        {
            get { return _state.Method.Name; }
            set
            {
                try
                {
                    Debugger.Launch();
                    _state = (StateHandler) Delegate.CreateDelegate(typeof (StateHandler), this, value);
                }
                catch (Exception e)
                {
                    Log.Debug(e);
                }
            }
        }

        public void ClearScreen()
        {
            Write("\x1b[2J");
        }

        public void Close()
        {
            Client.Close();
            SetState(null);
        }

        public void Start()
        {
            new Thread(Run) {IsBackground = true}.Start();
        }

        public override string ToString()
        {
            if (Client.Parser.Socket != null)
                return Client.Parser.Socket.RemoteEndPoint.ToString();

            return base.ToString();
        }

        public void DisplayPrompt()
        {
            if (!Connected)
                return;

            if (HasPage())
            {
                WriteLine();
                Write("({0}%) (H)elp, (R)efresh page, (P)revious page, or hit ENTER: ",
                      100*_pageBuf.ToString().CountLines(_pagePos)/_pageBuf.ToString().CountLines());
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
            return Playing != null && (_state == PlayingState);
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
            Debugger.Launch();

            if (_state == null)
            {
                WriteLine("Welcome to " + Server.World.Name + "!");

                RequestEchoOn();

                Write("Account Login: ");
                SetState(GetAccountName);
            }
            while (Server.State != ServerState.Stopped && _state != null && Connected)
            {
                try
                {
                    string line = _reader.ReadLine();

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

            int length = _pageBuf.ToString().GetLineCountLength(_pagePos, ScreenHeight - 1);

            if (length > 0)
            {
                Write(_pageBuf.ToString().Substring(_pagePos, length));
            }
            else
            {
                Write(_pageBuf);
                _pagePos = -1;
            }
        }

        public void Write(string value)
        {
            if (!Connected)
                return;

            _writer.Write(value);
        }

        public void WriteLine(string value)
        {
            if (!Connected)
                return;

            _writer.WriteLine(value);
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
            foreach (string pId in Account.Players)
            {
                Character info = Character.Lookup(pId);
                if (info != null)
                    WriteLine(" ~Y{0}) ~W{1} ~C(lvl {2})~x", i + 1, info.Name, info.Level);
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
            foreach (Class c in DbManager.Query<Class>())
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
                Playing.Room = Area.GetDefault(World).Rooms.First();
            }
            Playing.Room.Characters.Add(Playing);
            World.Players.Add(Playing);
            if (Playing.Level == 0)
            {
                if (DbManager.Count<Account>() == 0)
                {
                    Playing.Level = Game.MaxLevel;
                    Log.Warn("Setting first player " + Playing.Name + " immortal");
                    Playing.WriteLine("You are now level " + Playing.Level + "!");
                    Account.Save();
                }
                else
                {
                    Playing.Level = 1;
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

            foreach (PcRace r in DbManager.Query<Race>().OfType<PcRace>())
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

            Account = World.FindAccount(argument);

            if (Account != null)
            {
                Account.Connection = this;
                Write("~CPassword:~x ");
                RequestEchoOff();
                SetState(GetAccountPassword);
                return;
            }

            Account = new Account(this, argument);

            Write("~CWould you like to create an account for ~W{0}~C? (Y/n)~x", argument);
            SetState(ConfirmNewAccount);
        }

        private void GetAccountPassword(string argument)
        {
            byte[] passwd = Encoding.Default.GetBytes(argument);

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
            Class cl = World.FindClass(argument);

            if (cl == null)
            {
                Write("~CNo such class! Try again:~x ");
                return;
            }
            Playing.Class = cl;

            Account.Players.Add(Playing.Id);
            DisplayMotd();
            SetState(PlayingState);
        }

        private void GetCharRace(string argument)
        {
            Race r = World.FindRace(argument);


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
            if (!Game.IsEmail(argument))
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

            if (DbManager.Lookup<Player>(argument) != null)
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
            World.Players.Add(Playing);

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

            int option = int.Parse(argument) - 1;

            if (option < 0 || option > Account.Players.Count)
            {
                GetPlayer(null);
                return;
            }

            Playing = Character.Lookup<Player>(Account.Players[option]);
            Playing.Account = Account;
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
            catch (ProcessControlException e)
            {
                e.Execute();
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

        private void TelnetSubNegotiationRecieved(object o, TelnetSubNegotiationEventArgs args)
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
                    Client.SendSubNegotiation(TelnetOption.TerminalType, Encoding.ASCII.GetBytes("VT100"));
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
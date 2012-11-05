using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ArtheaEngine;
using ArtheaEngine.Model;
using NLog;
using Thought.Net.Telnet;

namespace ArtheaServer
{

    #region Enumerations

    #endregion Enumerations

    public class TelnetServer : IDisposable, IServer
    {
        #region Fields

        public static readonly HashSet<TelnetServer> List = new HashSet<TelnetServer>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Thread _listenThread;
        private TelnetListener _listener;

        #endregion Fields

        #region Constructors

        public TelnetServer(World world)
        {
            World = world;
            Connections = new HashSet<IConnection>();
            List.Add(this);
        }

        #endregion Constructors

        #region Properties

        public HashSet<IConnection> Connections { get; private set; }

        public ServerState State { get; set; }

        public World World { get; private set; }

        #endregion Properties

        #region Methods

        /*
        public static void Shutdown()
        {
            if (Application.MessageLoop)
            {
                // Use this since we are a WinForms app
                Application.Exit();
            }
            else
            {
                // Use this since we are a console app
                Environment.Exit(1);
            }
        }*/

        #region IDisposable Members

        public void Dispose()
        {
            List.Remove(this);
        }

        #endregion

        #region IServer Members

        public void Listen()
        {
            State = ServerState.Initializing;

            _listener = new TelnetListener(IPAddress.Any, World.Port);

            _listener.Start();

            Log.Info(World.Name + " accepting connections on port " + World.Port);

            State = ServerState.Running;

            while (State == ServerState.Running)
            {
                try
                {
                    TelnetClient client = _listener.Accept();

                    StartClient(client);
                }
                catch (SocketException)
                {
                }
            }

            Stop();
        }

        public void Start()
        {
            _listenThread = new Thread(Listen) {Name = World.Name};
            _listenThread.Start();
        }

        public void Stop()
        {
            State = ServerState.Stopped;

            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }

            if (_listenThread != null)
                _listenThread.Join(500);

            Dispose();
        }

        #endregion

        public TelnetConnection StartClient(TelnetClient client)
        {
            var connection = new TelnetConnection(client, this);

            Connections.Add(connection);

            Log.Info(connection + " connected");

            connection.Start();

            return connection;
        }

        public TelnetConnection StartClient(ConnectionConfig config)
        {
            var connection = new TelnetConnection(new TelnetClient(new Socket(config.SockInfo)), this);

            Connections.Add(connection);

            Log.Info(connection + " reconnected");

            connection.State = config.State;

            connection.Playing = DbManager.Lookup<Player>(config.PlayerId);

            connection.Start();

            return connection;
        }

        #endregion Methods
    }
}
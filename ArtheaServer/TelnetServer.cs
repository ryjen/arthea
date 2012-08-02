namespace ArtheaServer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using Microsoft.Win32.TaskScheduler;

    using NLog;

    using Thought.Net.Telnet;

    #region Enumerations

    public enum ServerState
    {
        Initializing,
        Running,
        Stopped
    }

    #endregion Enumerations

	/*
    public class RebootRecovery : MarshalByRefObject
    {
        #region Fields

        public const short Port = 8773;

        /// <summary>
        /// Signals this recovery is done
        /// </summary>
        public static bool Done = false;

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Methods

        public void Finished()
        {
            Done = true;
        }

        /// <summary>
        /// Transfers the connections.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <returns></returns>
        public Dictionary<string, RebootRecoveryData> TransferConnections(int processId)
        {
            Log.Info("Transferring connections...");

            var connections =
                new Dictionary<string, RebootRecoveryData>();

            foreach (var s in TelnetServer.List)
            {
                foreach (TelnetConnection con in s.Connections)
                {
                    if (!con.IsPlaying())
                    {
                        continue;
                    }
                    connections.Add(con.World.Id, new RebootRecoveryData { PlayerName = con.Playing.Name, SockInfo = con.Client.Parser.Socket.DuplicateAndClose(processId) });
                }
            }

            return connections;
        }

        #endregion Methods
    }

    public class RebootRecoveryData : MarshalByRefObject
    {
        #region Fields

        public string PlayerName;
        public SocketInformation SockInfo;

        #endregion Fields
    }*/

    public class TelnetServer : IDisposable
    {
        #region Fields

        public static readonly HashSet<TelnetServer> List = new HashSet<TelnetServer>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private TelnetListener _listener;
        private Thread _listenThread;

        #endregion Fields

        #region Constructors

        public TelnetServer(World world)
        {
            World = world;
            Connections = new HashSet<TelnetConnection>();
            List.Add(this);
        }

        #endregion Constructors

        #region Properties

        public HashSet<TelnetConnection> Connections
        {
            get;
            private set;
        }

        public ServerState State
        {
            get; set;
        }

        public World World
        {
            get; set;
        }

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

        public void Dispose()
        {
            List.Remove(this);
        }

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
                    var client = _listener.Accept();

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

        public void StartClient(TelnetClient client)
        {
            var connection = new TelnetConnection(client, this);

            Connections.Add(connection);

            Log.Info(client.Parser.Socket.RemoteEndPoint + " connected");

            new Thread(connection.Run) { IsBackground = true }.Start();
        }

        public void Stop()
        {
            State = ServerState.Stopped;

            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }

			if(_listenThread != null)
	            _listenThread.Join(500);

            Dispose();
        }

        #endregion Methods
    }
}
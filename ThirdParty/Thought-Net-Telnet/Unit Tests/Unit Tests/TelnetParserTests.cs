
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;

namespace Thought.Net.Telnet.Tests
{

    [TestFixture]
    public class TelnetParserTests
    {

        private TcpListener listener;

        #region [ Setup ]

        [TestFixtureSetUp]
        public void Setup()
        {

            // Create a new server, listening to any available port
            // on the loopback (127.0.0.1) address.  Port 23 cannot be
            // used because there might already be a test or telnet
            // server running on the computer.

            listener = new TcpListener(IPAddress.Loopback, 0);
            
            // A local firewall (e.g. Windows Firewall) may raise
            // a warning or block the following call.  You will need
            // to allow NUnit to talk to the Internet.

            listener.Start();

        }


        #endregion

        #region [ TearDown ]

        [TestFixtureTearDown]
        public void TearDown()
        {
            listener.Stop();
        }

        #endregion

        #region [ _getClientParser ]

        private TelnetParser _getClientParser()
        {

            Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            socket.Connect(listener.LocalEndpoint);
            return new TelnetParser(socket);

        }

        #endregion

        #region [ _getServerParser ]

        /// <summary>
        ///     Private utility function to get an initialized
        ///     TelnetClient instance that has connected to the
        ///     listener.
        /// </summary>
        private TelnetParser _getServerParser()
        {
            Socket socket = listener.AcceptSocket();
            return new TelnetParser(socket);
        }

        #endregion

        #region [ RandomFlow ]

        [Test]
        public void RandomFlow()
        {

            // This test will send a random buffer over the telnet
            // connection from the server to the client.  The goal
            // is to reconstruct the data packet.

            byte[] original = Helper.GetRandomBytes(1000);

            // Open the client and server connections.

            TelnetParser client = _getClientParser();
            TelnetParser server = _getServerParser();

            // A different thread will send data from the server
            // to the client.  The logic and thread method is
            // implemented in the RandomFlowSender class (defined
            // after this method).

            RandomFlowSender sender =
                new RandomFlowSender(server, original);

            ThreadStart senderStart = new ThreadStart(sender.Start);
            Thread senderThread = new Thread(senderStart);
            senderThread.Start();

            // A separate thread will handle the arriving
            // data at the client.  A wait event will be used
            // to indicate this thread is finished.

            ManualResetEvent doneEvent = new ManualResetEvent(false);

            RandomFlowReceiver receiver =
                new RandomFlowReceiver(client, original.Length, doneEvent);

            ThreadStart receiverStart = new ThreadStart(receiver.Start);
            Thread receiverThread = new Thread(receiverStart);
            receiverThread.Start();

            doneEvent.WaitOne();

            // Get the arrived data as a byte array.

            byte[] arrived = receiver.ToArray();

            Assert.AreEqual(
                original.Length,
                arrived.Length);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], arrived[i], "Different at offset " + i);
            }

        }

        private class RandomFlowReceiver
        {

            private ByteBuffer buffer = new ByteBuffer();
            private TelnetParser client;
            private ManualResetEvent doneEvent;
            private int expectedCount;

            public RandomFlowReceiver(
                TelnetParser client,
                int expectedCount,
                ManualResetEvent doneEvent)
            {
                this.client = client;
                this.expectedCount = expectedCount;
                this.doneEvent = doneEvent;
            }

            public void Start()
            {

                while (buffer.Count < expectedCount)
                {

                    TelnetPacket packet = client.ReadNext();

                    if (packet.PacketType == TelnetPacketType.Data)
                    {
                        buffer.Append(packet.Data);
                    }

                }

                // Notify someone that the data has arrived.

                doneEvent.Set();

            }

            public byte[] ToArray()
            {
                return this.buffer.ToArray();
            }
        }

        /// <summary>
        ///     Sends data from the server to the client.
        /// </summary>
        private class RandomFlowSender
        {

            private byte[] data;
            private int index;
            private Random random;
            private TelnetParser server;

            public RandomFlowSender(TelnetParser server, byte[] data)
            {
                this.random = new Random();
                this.server = server;
                this.data = data;
            }

            public void Start()
            {

                while (this.index < this.data.Length)
                {

                    // Go to sleep for any period of time between
                    // 1 and some number of milliseconds.  This is
                    // intended to encourage the data to arrive at
                    // the client in spurts.

                    Thread.Sleep(this.random.Next(1, 1000));

                    // Calculate the number of bytes that have
                    // not been sent.  The index field maintains
                    // the index of the next byte to send.

                    int remaining = this.data.Length - index;

                    // Select a random packet size.  The packets
                    // will be larger in the early loops because
                    // the possible random numbers are larger.
                    // The sizes will grow smaller and smaller
                    // until they reach 1.

                    int packetSize = this.random.Next(1, remaining + 1);

                    // Send a new packet.

                    server.SendEscaped(this.data, index, packetSize);
                    this.index += packetSize;

                }

            }

        }

        #endregion

        #region [ ReadNext_Maximum_1 ]

        [Test]
        public void ReadNext_Maximum_1()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendEscaped(new byte[] { 1, 2, 3 });

                Helper.AssertDataPacket(
                    client.ReadNext(1),
                    new byte[] { 1 });

                Helper.AssertDataPacket(
                    client.ReadNext(1),
                    new byte[] { 2 });

                Helper.AssertDataPacket(
                    client.ReadNext(1),
                    new byte[] { 3 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ ReadNext_Maximum_Equal ]

        [Test]
        public void ReadNext_Maximum_Equal()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendEscaped(new byte[] { 1, 2, 3 });

                Helper.AssertDataPacket(
                    client.ReadNext(3),
                    new byte[] { 1, 2, 3 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ ReadNext_Maximum_Excessive ]

        [Test]
        public void ReadNext_Maximum_Excessive()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendEscaped(new byte[] { 1, 2, 3 });

                Helper.AssertDataPacket(
                    client.ReadNext(5000),
                    new byte[] { 1, 2, 3 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Command ]

        [Test]
        public void Send_Command()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendDo(1);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Do,
                    TelnetOption.Echo);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data_Command ]

        [Test]
        public void Send_Data_Command()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = Helper.GetRandomBytes(100);

                server.SendEscaped(buffer);
                server.SendDo(1);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Do,
                    TelnetOption.Echo);

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data_Command_Data ]

        [Test]
        public void Send_Data_Command_Data()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer1 = Helper.GetRandomBytes(100);
                byte[] buffer2 = Helper.GetRandomBytes(100);

                server.SendEscaped(buffer1);
                server.SendDo(1);
                server.SendEscaped(buffer2);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer1);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Do,
                    TelnetOption.Echo);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer2);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data ]

        [Test]
        public void Send_Data()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = Helper.GetRandomBytes(1000);
                server.SendEscaped(buffer);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer);

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data_IacData ]

        [Test]
        public void Send_Data_IacData()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = new byte[] { 0, 1, 2, 3, 255 };

                client.SendEscaped(buffer);

                Helper.AssertDataPacket(
                    server.ReadNext(),
                    buffer);

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data_IacData_Data ]

        [Test]
        public void Send_Data_IacData_Data()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = new byte[] { 0, 1, 2, 255, 0, 1, 2 };

                server.SendEscaped(buffer);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer);

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_Data_IacData_Command ]

        [Test]
        public void Send_Data_IacData_Command()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = new byte[] { 0, 1, 2, 255, 0, 1, 2 };

                server.SendEscaped(buffer);
                server.SendDo(1);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Do,
                    TelnetOption.Echo);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_IacData ]

        [Test]
        public void Send_IacData()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendEscaped(new byte[] { 255 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 255 });

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_IacData_IacData ]

        [Test]
        public void Send_IacData_IacData()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                byte[] buffer = new byte[] { 255, 255 };

                server.SendEscaped(buffer);

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    buffer);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ Send_IacData_Data ]

        [Test]
        public void Send_IacData_Data()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendEscaped(new byte[] { 255, 1 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 255, 1 });

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ SendCommand_NoOperation ]

        [Test]
        public void SendCommand_NoOperation()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendCommand(TelnetCommand.NoOperation);

                Helper.AssertNoOperationPacket(client.ReadNext());

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendDo ]

        [Test]
        public void SendDo()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendDo((byte)TelnetOption.Echo);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Do,
                    TelnetOption.Echo);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendDont ]

        [Test]
        public void SendDont()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendDont((byte)TelnetOption.Echo);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Dont,
                    TelnetOption.Echo);

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendGoAhead ]

        [Test]
        public void SendGoAhead()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendGoAhead();

                Helper.AssertGoAheadPacket(client.ReadNext());

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendRaw_CR_LF ]

        [Test]
        public void SendRaw_CR_LF()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendRaw(new byte[] { 13, 10 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 13, 10 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendRaw_CR_NULL ]

        [Test]
        public void SendRaw_CR_NULL()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendRaw(new byte[] { 13, 0 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 13 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendRaw_CR_NUL_TAB ]

        [Test]
        public void SendRaw_CR_NUL_TAB()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendRaw(new byte[] { 13, 0, 8 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 13, 8 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }


        }


        #endregion

        #region [ SendRaw_Iac_Iac ]

        [Test]
        public void SendRaw_Iac_Iac()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                // The IAC character is ASCII 255.  Two of these
                // should be collected by the receiver into a 
                // single IAC.

                server.SendRaw(new byte[] { 255, 255 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 255 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendRaw_Iac_Iac_Iac_Iac ]

        [Test]
        public void SendRaw_Iac_Iac_Iac_Iac()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                // The IAC character is ASCII 255.  Two of these
                // should be collected by the receiver into a 
                // single IAC.

                server.SendRaw(new byte[] { 255, 255, 255, 255 });

                Helper.AssertDataPacket(
                    client.ReadNext(),
                    new byte[] { 255, 255 });

            }
            finally
            {
                if (client != null) client.Close();
                if (server != null) server.Close();
            }

        }

        #endregion

        #region [ SendWill ]

        [Test]
        public void SendWill()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendWill((byte)TelnetOption.Echo);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Will,
                    TelnetOption.Echo);

            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

        #region [ SendWont ]

        [Test]
        public void SendWont()
        {

            TelnetParser client = null;
            TelnetParser server = null;

            try
            {

                client = _getClientParser();
                server = _getServerParser();

                server.SendWont((byte)TelnetOption.Echo);

                Helper.AssertCommandPacket(
                    client.ReadNext(),
                    TelnetCommand.Wont,
                    TelnetOption.Echo);
            }
            finally
            {
                if(client != null) client.Close();
                if(server != null) server.Close();
            }

        }

        #endregion

    }
}

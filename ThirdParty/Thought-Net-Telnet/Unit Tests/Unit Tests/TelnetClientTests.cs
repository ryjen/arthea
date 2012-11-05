
// You may need to configure your firewall to allow
// these tests to operate.  They require TCP access.

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Thought.Net.Telnet;

namespace Thought.Net.Telnet.Tests
{

    [TestFixture]
    public class TelnetClientTests
    {

        private const int TestTelnetPort = 2323;

        private TcpListener listener;


        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            listener = new TcpListener(IPAddress.Loopback, TestTelnetPort);
            listener.Start(1);
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            listener.Stop();
        }


        /// <summary>
        ///     Private utility function to get an initialized
        ///     TelnetClient instance that has connected to the
        ///     listener.
        /// </summary>
        private TelnetClient _getClient()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, TestTelnetPort);
            return new TelnetClient(tcpClient.Client);
        }


        /// <summary>
        ///     Private utility function that gets an intialized
        ///     TelnetClient instance that represents the server.
        /// </summary>
        private TelnetClient _getServer()
        {
            return new TelnetClient(this.listener.AcceptSocket());
        }

        #region [ SendDo ]

        /// <summary>
        ///     Handler for the SendDo test.
        /// </summary>
        private void SendDo_DoReceived(object sender, TelnetOptionEventArgs e)
        {
            e.Agreed = true;
            sendDoReceived = true;
        }

        private bool sendDoReceived;

        [Test]
        public void SendDo()
        {

            // This test sends an DO option to the server.

            TelnetClient client = _getClient();
            TelnetClient server = _getServer();

            server.DoReceived += new TelnetOptionEventHandler(SendDo_DoReceived);

            client.SendDo(TelnetOption.Echo);
            client.Write(12);

            byte[] buffer = new byte[1];
            server.Read(buffer, 0, 1);

            Assert.AreEqual(
                12,
                buffer[0],
                "The 12 should be the next byte.");

            Assert.IsTrue(sendDoReceived);

            Assert.IsTrue(
                server.DoingLocalOption((byte)TelnetOption.Echo));

        }

        #endregion

        #region [ WriteAllBytes ]

        [Test]
        public void WriteAllBytes()
        {

            // This test sends every possible 8-but byte over the
            // Telnet connection and makes sure the same byte is
            // received by the other side.

            TelnetClient client = _getClient();
            TelnetClient server = _getServer();

            byte[] serverBuffer = new byte[1];

            for (int i = 0; i <= 0xFF; i++)
            {

                // Use the client to write a byte, then use the
                // server to read a byte.

                client.Write((byte)i);
                server.Read(serverBuffer, 0, 1);

                Assert.AreEqual(
                    (byte)i,
                    serverBuffer[0],
                    "Value " + i + " failed to send.");
            }

            // Now do it in reverse

            for (int i = 0xFF; i >= 0; i--)
            {
                client.Write((byte)i);
                server.Read(serverBuffer, 0, 1);

                Assert.AreEqual(
                    (byte)i,
                    serverBuffer[0],
                    "Value " + i + " failed to send.");
            }

            server.Close();
            client.Close();

        }

        #endregion

        #region [ WriteIAC ]

        [Test]
        public void WriteIAC()
        {

            // This test sends the IAC character (0xFF), which
            // should be escaped into two IAC characters.

            TelnetClient client = _getClient();
            TelnetClient server = _getServer();

            byte[] serverBuffer = new byte[1];

            client.Write(TelnetParser.IAC);

            // The ReadRaw function loads bytes without
            // any escaping.  Normally calling .Read will
            // cause the doubled/escaped IAC to be collapsed.

            server.Read(serverBuffer, 0, 1);
            Assert.AreEqual(serverBuffer[0], 0xFF);

        }

        #endregion

    }
}

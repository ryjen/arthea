
using System;
using NUnit.Framework;
using Thought.Net.Telnet;

namespace Thought.Net.Telnet.Tests
{

    /// <summary>
    ///     Utility functions for unit tests.
    /// </summary>
    public static class Helper
    {

        #region [ AssertCommandPacket ]

        public static void AssertCommandPacket(
            TelnetPacket packet,
            TelnetCommand command,
            TelnetOption option)
        {

            Assert.IsNotNull(
                packet,
                "A command packet was expected; instead null was encountered.");

            Assert.AreEqual(
                TelnetPacketType.Command,
                packet.PacketType,
                "A command packet was expected.");

            Assert.AreEqual(
                command,
                packet.Command,
                "A command packet of " + command.ToString() + " was expected.");

            Assert.AreEqual(
                option,
                packet.Option,
                "An option of " + option.ToString() + " was expected.");

        }

        #endregion

        #region [ AssertDataPacket ]

        public static void AssertDataPacket(
            TelnetPacket packet,
            byte[] expectedBytes)
        {

            Assert.IsNotNull(
                packet,
                "A data packet was expected; instead null was encountered.");

            Assert.AreEqual(
                TelnetPacketType.Data,
                packet.PacketType,
                "A data packet was expected.");

            Assert.IsNull(
                packet.Option,
                "The Option property should be null for data packets.");

            Assert.AreEqual(
                expectedBytes.Length,
                packet.Data.Length,
                "The data sizes are different.");

            for (int i = 0; i < expectedBytes.Length; i++)
            {

                Assert.AreEqual(
                    expectedBytes[i],
                    packet.Data[i],
                    "Data bytes differ at index " + i);
            }

        }

        #endregion

        #region [ AssertGoAheadPacket ]

        public static void AssertGoAheadPacket(
            TelnetPacket packet)
        {

            Assert.IsNotNull(
                packet,
                "A command packet was expected; instead null was encountered.");

            Assert.AreEqual(
                TelnetPacketType.Command,
                packet.PacketType,
                "A command packet was expected.");

            Assert.AreEqual(
                TelnetCommand.GoAhead,
                packet.Command,
                "A GA packet was expected.");

            Assert.IsNull(
                packet.Option,
                "The Option property should not be set for GA packets.");

        }

        #endregion

        #region [ AssertNoOperationPacket ]

        public static void AssertNoOperationPacket(
            TelnetPacket packet)
        {

            Assert.IsNotNull(
                packet,
                "A command packet was expected; instead null was encountered.");

            Assert.AreEqual(
                TelnetPacketType.Command,
                packet.PacketType,
                "A command packet was expected.");

            Assert.AreEqual(
                TelnetCommand.NoOperation,
                packet.Command,
                "A NOP packet was expected.");

            Assert.IsNull(
                packet.Option,
                "The Option property should not be set for NOP packets.");

        }

        #endregion

        #region [ GetRandomBytes ]

        public static byte[] GetRandomBytes(int size)
        {

            if (size < 1)
                throw new ArgumentOutOfRangeException("size");

            // Instantiate a new random number generator
            // using the system clock as a seed.

            Random random = new Random();

            byte[] buffer = new byte[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = (byte)random.Next(0, 256);
            }

            return buffer;

        }

        #endregion
    
    }
}

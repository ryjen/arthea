
using System;
using NUnit.Framework;
using Thought.Net.Telnet;

namespace Thought.Net.Telnet.Tests
{

    [TestFixture]
    public class TelnetPacketTests
    {

        #region [ Constructor_ByteArray ]

        [Test]
        public void Constructor_ByteArray()
        {

            // Get some random bytes
            byte[] sample = Helper.GetRandomBytes(1000);

            // Create a packet of data using the random bytes
            TelnetPacket packet = new TelnetPacket(sample);

            // Make sure the packet was created property
            Helper.AssertDataPacket(packet, sample);

        }

        #endregion

        #region [ Constructor_ByteArray_Null ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ByteArray_Null()
        {

            // The byte[] constructor assumes the specified byte
            // array contains regular user data.  A null value
            // should result in an argument null exception.

            TelnetPacket packet = new TelnetPacket((byte[])null);
        }

        #endregion

        // The next tests execute the constructor that accepts a TelnetCommand
        // parameter.  Many of the commands do not require any additional
        // properties, while some do.  An exception should be raised if the
        // specified command requires additional information.  In that case
        // the caller should be using a different constructor.
        //
        // DO ..... Requires option
        // DONT ... Requires option
        // WILL ... Requires option
        // WONT ... Requires option

        #region [ Constructor_Command_MissingOption_Do ]

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_Command_MissingOption_Do()
        {
            TelnetPacket packet = new TelnetPacket(TelnetCommand.Do);
        }

        #endregion

        #region [ Constructor_Command_MissingOption_Dont ]

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_Command_MissingOption_Dont()
        {
            TelnetPacket packet = new TelnetPacket(TelnetCommand.Dont);
        }

        #endregion

        #region [ Constructor_Command_MissingOption_Will ]

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_Command_MissingOption_Will()
        {
            TelnetPacket packet = new TelnetPacket(TelnetCommand.Will);
        }

        #endregion

        #region [ Constructor_Command_MissingOption_Wont ]

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Constructor_Command_MissingOption_Wont()
        {
            TelnetPacket packet = new TelnetPacket(TelnetCommand.Wont);
        }

        #endregion

        #region [ Constructor_Subarray_LengthTooLong1 ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_LengthTooLong1()
        {

            // Scenerio #1: The offset is 0 (the start of the array)
            // and the specified length is too long by one byte.

            byte[] sample = Helper.GetRandomBytes(100);

            TelnetPacket packet = new TelnetPacket(
                sample,
                0,
                sample.Length + 1);

        }

        #endregion

        #region [ Constructor_Subarray_LengthTooLong2 ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_LengthTooLong2()
        {
            byte[] sample = Helper.GetRandomBytes(100);

            TelnetPacket packet = new TelnetPacket(
                sample,
                1,
                sample.Length);

        }

        #endregion

        #region [ Constructor_Subarray_NegativeOffset ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_NegativeIndex()
        {

            TelnetPacket packet = new TelnetPacket(
                Helper.GetRandomBytes(10),
                -1,
                10);

        }

        #endregion

        #region [ Constructor_Subarray_NegativeLength ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_NegativeLength()
        {

            TelnetPacket packet = new TelnetPacket(
                Helper.GetRandomBytes(10),
                0,
                -1);

        }

        #endregion

        #region [ Constructor_Subarray_NullData ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Subarray_NullData()
        {

            TelnetPacket packet = new TelnetPacket(
                (byte[])null,
                0,
                1);

        }

        #endregion

        #region [ Constructor_Subarray_OffsetTooLarge ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_OffsetTooLarge()
        {

            byte[] sample = Helper.GetRandomBytes(100);

            TelnetPacket packet = new TelnetPacket(
                sample,
                sample.Length,
                1);

        }

        #endregion

        #region [ Constructor_Subarray_ZeroLength ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Subarray_ZeroLength()
        {

            byte[] sample = Helper.GetRandomBytes(10);

            TelnetPacket packet = new TelnetPacket(
                sample,
                0,
                0);
        }

        #endregion

    }
}

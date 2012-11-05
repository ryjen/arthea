using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Thought.Net.Telnet;

namespace Thought.Net.Telnet.Tests
{

    [TestFixture]
    public class ByteBufferTests
    {

        /// <summary>
        ///     Sample data for use with tests; initialized in FixtureSetup().
        /// </summary>
        private byte[] sampleBuffer;


        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.sampleBuffer = new byte[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 35 };
        }


        [Test]
        public void Clear()
        {

            // This test makes sure the Clear method clears the bytes.

            ByteBuffer buffer = new ByteBuffer(sampleBuffer);

            // Confirm the sample data was loaded.

            Assert.AreEqual(
                sampleBuffer.Length,
                buffer.Count,
                "The buffer should have been loaded with the sample data.");

            // Clear the buffer

            buffer.Clear();

            Assert.AreEqual(
                0,
                buffer.Count,
                "The buffer should have been cleared.");

        }


        [Test]
        public void CollapseDoubles_ValueAtBeginning()
        {

            // This test calls the CollapseDoubles function against
            // a buffer that contains the doubled value at the
            // beginning of the buffer.

            ByteBuffer buffer = new ByteBuffer(
                new byte[] { 0xFF, 0xFF, 3, 4, 5 });

            buffer.CollapseDoubles(0xFF);

            Assert.AreEqual(
                4,
                buffer.Count,
                "The buffer should have been reduced in size by one byte.");

            Assert.AreEqual(0xFF, buffer[0]);
            Assert.AreEqual(0x03, buffer[1]);
            Assert.AreEqual(0x04, buffer[2]);
            Assert.AreEqual(0x05, buffer[3]);

        }


        [Test]
        public void CollapseDoubles_ValueAtEnd()
        {

            // This test calls the CollapseDoubles function against
            // a buffer that contains the doubled value at the end of the buffer.

            ByteBuffer buffer = new ByteBuffer(
                new byte[] { 0, 1, 2, 0xFF, 0xFF });

            buffer.CollapseDoubles(0xFF);

            Assert.AreEqual(
                4,
                buffer.Count,
                "The buffer should have been reduced in size by one byte.");

            Assert.AreEqual(0x00, buffer[0]);
            Assert.AreEqual(0x01, buffer[1]);
            Assert.AreEqual(0x02, buffer[2]);
            Assert.AreEqual(0xFF, buffer[3]);

        }


        [Test]
        public void CollapseDoubles_ValueInMiddle()
        {

            // This test calls the CollapseDoubles function
            // against a buffer that contains the doubled value in
            // the middle of the buffer.

            ByteBuffer buffer = new ByteBuffer(
                new byte[] { 0, 1, 0xFF, 0xFF, 3 });

            buffer.CollapseDoubles(0xFF);

            Assert.AreEqual(
                4,
                buffer.Count,
                "The buffer should have been reduced in size by one byte.");

            Assert.AreEqual(0x00, buffer[0]);
            Assert.AreEqual(0x01, buffer[1]);
            Assert.AreEqual(0xFF, buffer[2]);
            Assert.AreEqual(0x03, buffer[3]);

        }


        [Test]
        public void CollapseDoubles_Simple()
        {

            // This test calls the CollapseDoubles function with a 
            // simple buffer that contains only the value.

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0xFF, 0xFF });

            buffer.CollapseDoubles(0xFF);

            Assert.AreEqual(
                1,
                buffer.Count,
                "The buffer should be left with a single byte after collapsing.");

            Assert.AreEqual(
                0xFF,
                buffer[0],
                "The remaining value should be 0xFF.");

        }


        [Test]
        public void CollapseDoubles_NonExistentValue()
        {

            // This test calls the CollapseDoubles function
            // with a value that is known to not exist in the
            // buffer.  Nothing should happen.

            ByteBuffer buffer = new ByteBuffer(
                new byte[] { 0, 1, 0, 1, 0, 1 });

            buffer.CollapseDoubles(99);

            Assert.AreEqual(
                6,
                buffer.Count,
                "The buffer should not have been modified.");

        }


        [Test]
        public void CollapseDoubles_ShouldNotHappen()
        {

            // This test calls the CollapseDoubles function
            // against a buffer that does not contain any
            // identical values that are adjacent to each other.
            // Therefore the buffer should not be affected.

            ByteBuffer buffer = new ByteBuffer(
                new byte[] { 0, 1, 0, 1, 0, 1 });

            buffer.CollapseDoubles(0);
            buffer.CollapseDoubles(1);

            Assert.AreEqual(
                6,
                buffer.Count,
                "The buffer should not have been affected.");

        }


        [Test]
        public void Constructor_Array()
        {

            // This method makes sure the class properly accepts
            // a byte buffer by storing its contents and also setting
            // the Count property.

            ByteBuffer buffer = new ByteBuffer(sampleBuffer);

            Assert.AreEqual(
                sampleBuffer.Length,
                buffer.Count,
                "The buffer should have been filled with the sample data.");

            for (int i = 0; i < sampleBuffer.Length; i++)
            {

                Assert.AreEqual(
                    sampleBuffer[i],
                    buffer[i],
                    "The byte at offset " + i + " should be " + i + ".");
            }

        }


        [Test]
        public void Constructor_Default()
        {

            // This method tests the default constructor.  The
            // Count should be zero and the Capacity should be
            // the DefaultCapacity of the buffer class.

            ByteBuffer buffer = new ByteBuffer();

            Assert.AreEqual(
                ByteBuffer.DefaultCapacity,
                buffer.Capacity,
                "The default capacity should have been used to initialize the buffer.");

            Assert.AreEqual(
                0,
                buffer.Count,
                "The buffer should not have any contents.");

        }


        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeCapacity()
        {

            // This method makes sure the class does not accept
            // a negative capacity.

            ByteBuffer buffer = new ByteBuffer(-1);
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullArray()
        {
            
            // This method makes sure the class does not
            // accept a null input array.

            ByteBuffer buffer = new ByteBuffer(null);
        }


        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ZeroCapacity()
        {

            // This method makes sure the class does not
            // accept zero as a capacity.

            ByteBuffer buffer = new ByteBuffer(0);
        }


        [Test]
        public void Contains()
        {

            // This test makes sure the Contains method is
            // properly finding (or not finding) values.
            // The code will check every possible 8-bit byte.

            ByteBuffer buffer = new ByteBuffer(sampleBuffer);

            for (int i = 0; i < 255; i++)
            {

                // Look for the byte in the sample data.

                int sampleIndex = Array.IndexOf<byte>(sampleBuffer, (byte)i);

                // The index should be the same in the buffer because
                // the contents were copied.

                int bufferIndex = buffer.IndexOf((byte)i);

                Assert.AreEqual(
                    sampleIndex,
                    bufferIndex,
                    "Results differ for value " + i);

            }

        }


        [Test]
        public void Double1()
        {

            // Makes sure a single byte is doubled.

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0xFF });

            buffer.Double(0xFF);

            Assert.AreEqual(
                2,
                buffer.Count,
                "The 0xFF should have doubled into two 0xFF bytes.");

            Assert.AreEqual(
                0xFF,
                buffer[0],
                "The byte[0] should have been 0xFF.");

            Assert.AreEqual(
                0xFF,
                buffer[1],
                "The byte[1] should have been 0xFF after doubling.");

        }


        [Test]
        public void InsertValues_Beginning()
        {

            // This test inserts two bytes into the middle of a buffer,
            // then verifies that the insert operation was successful.

            ByteBuffer buffer = new ByteBuffer(new byte[] { 2, 3 });

            buffer.Insert(0, new byte[] { 0, 1 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }


        [Test]
        public void InsertValues_End()
        {

            // This test inserts two bytes into the middle of a buffer,
            // then verifies that the insert operation was successful.

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0, 1 });

            buffer.Insert(2, new byte[] { 2, 3 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }


        [Test]
        public void InsertValues_Middle()
        {

            // This test inserts two bytes into the middle of a buffer,
            // then verifies that the insert operation was successful.

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0, 3 });

            buffer.Insert(1, new byte[] { 1, 2 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }


        [Test]
        public void ReplaceValues_Beginning()
        {

            ByteBuffer buffer = new ByteBuffer(new byte[] { 9, 2, 3 });
            buffer.Replace(9, new byte[] { 0, 1 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }


        [Test]
        public void ReplaceValues_End()
        {

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0, 1, 9 });
            buffer.Replace(9, new byte[] { 2, 3 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }


        [Test]
        public void ReplaceValues_Middle()
        {

            ByteBuffer buffer = new ByteBuffer(new byte[] { 0, 9, 3 });
            buffer.Replace(9, new byte[] { 1, 2 });

            Assert.AreEqual(
                4,
                buffer.Count,
                "The expected count is 4.");

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);

        }

    }
}

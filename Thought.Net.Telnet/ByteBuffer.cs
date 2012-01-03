namespace Thought.Net.Telnet
{
    using System;

    /// <summary>
    ///     A resizeable byte array.
    /// </summary>
    public class ByteBuffer
    {
        #region Fields

        /// <summary>
        ///     The default capacity of the buffer (in bytes) if an
        ///     initial capacity is not specified by the developer.
        /// </summary>
        public const int DefaultCapacity = 256;

        /// <summary>
        ///     The internal byte array managed by the class.
        /// </summary>
        private byte[] buffer;

        /// <summary>
        ///     The number of bytes stored in the buffer.
        ///     This is not necessarily the actual length of
        ///     the internal byte array.  It is the number of
        ///     bytes as seen by the user.
        /// </summary>
        private int count;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Creates a buffer with the default capacity.
        /// </summary>
        /// <seealso cref="DefaultCapacity"/>
        public ByteBuffer()
        {
            count = 0;
            buffer = new byte[DefaultCapacity];
        }

        /// <summary>
        ///     Creates a buffer with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the buffer</param>
        public ByteBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity");

            count = 0;
            buffer = new byte[capacity];
        }

        /// <summary>
        ///     Creates a buffer and initializes it with an existing byte array.
        /// </summary>
        /// <param name="data">The data to copy to the buffer.</param>
        public ByteBuffer(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            buffer = new byte[data.Length];
            data.CopyTo(buffer, 0);
            count = data.Length;
        }

        /// <summary>
        ///     Initializes the byte buffer with a subset of bytes
        ///     from a byte array.
        /// </summary>
        /// <param name="data">
        ///     A byte array containing data to copy to the byte buffer.
        /// </param>
        /// <param name="offset">
        ///     The offset of the source array at which to begin copying bytes.
        /// </param>
        /// <param name="count">
        ///     The number of bytes from the source array to copy to the buffer.
        /// </param>
        public ByteBuffer(byte[] data, int offset, int count)
        {
            buffer = new byte[count];
            Append(data, offset, count);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     The capacity of the buffer.
        /// </summary>
        /// <remarks>
        ///     The capacity reflects the number of bytes that can be stored
        ///     in the buffer before a resize operation is necessary.  The
        ///     capacity is always a positive number equal to or greater than
        ///     the <see cref="Count"/> of bytes stored in the buffer.
        /// </remarks>
        /// <seealso cref="Count"/>
        public int Capacity
        {
            get { return buffer.Length; }

            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");

                if (value == buffer.Length)

                    // The programmer specified the same
                    // capacity.  Do nothing.

                    return;

                if (value < count)

                    // The caller specified a size that is smaller than the
                    // current number of bytes in the array.  An exception
                    // is raised instead of risking loss of data.  A future
                    // version may truncate the buffer.

                    throw new NotSupportedException();

                else
                {
                    // The programmer specified a new capacity that is
                    // greater than zero and also greater than the current
                    // number of bytes in the buffer.  A new array will
                    // be created, and if necessary, current bytes will be
                    // copied to the array.

                    var newItems = new byte[value];

                    Array.Copy(
                        buffer, // sourceArray
                        0, // sourceIndex,
                        newItems, // destinationArray
                        0, // destinationIndex
                        count); // length

                    buffer = newItems;
                }
            }
        }

        /// <summary>
        ///     The number of bytes stored in the buffer.
        /// </summary>
        /// <seealso cref="Capacity"/>
        public int Count
        {
            get { return count; }
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        ///     Gets or sets the byte at the specified index.
        /// </summary>
        public byte this[int index]
        {
            get
            {
                if ((index < 0) || (index >= count))

                    // The caller specified an index that is negative
                    // or larger than the past the range of bytes
                    // in the buffer.

                    throw new ArgumentOutOfRangeException("index");

                return buffer[index];
            }
            set
            {
                if ((index < 0) || (index >= count))
                    throw new ArgumentOutOfRangeException("index");

                buffer[index] = value;
            }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        ///     Appends a byte to the end of the buffer.
        /// </summary>
        /// <param name="value">
        ///     The byte to append.
        /// </param>
        public void Append(byte value)
        {
            EnsureCapacity(count + 1);
            buffer[count] = value;
            count++;
        }

        /// <summary>
        ///     Appends an array of bytes to the end of the buffer.
        /// </summary>
        /// <param name="bytes">
        ///     A byte array containing the bytes to append.
        /// </param>
        public void Append(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (bytes.Length == 0)
                return;

            EnsureCapacity(count + bytes.Length);

            Array.Copy(
                bytes, // sourceArray
                0, // sourceIndex
                buffer, // destinationArray
                count, // destinationIndex
                bytes.Length); // length

            count += bytes.Length;
        }

        /// <summary>
        ///     Appends a subset of a byte array to the byte buffer.
        /// </summary>
        /// <param name="sourceArray">
        ///     An array containing bytes to append to the buffer.
        /// </param>
        /// <param name="sourceIndex">
        ///     The index of the source array at which to begin
        ///     copying bytes into the buffer.
        /// </param>
        /// <param name="count">
        ///     The number of bytes from the source array to
        ///     append into the byte buffer.
        /// </param>
        public void Append(byte[] sourceArray, int sourceIndex, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");

            if ((sourceArray.Length == 0) || (count == 0))
                return;

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            EnsureCapacity(this.count + count);

            Array.Copy(
                sourceArray,
                sourceIndex,
                buffer,
                this.count,
                count);

            this.count += count;
        }

        /// <summary>
        ///     Clears the contents of the buffer.
        /// </summary>
        /// <remarks>
        ///     The buffer is not necessarily resized (trimmed).  Clearing
        ///     the buffer will not reduce the memory consumed by the buffer.
        /// </remarks>
        public void Clear()
        {
            if (count > 0)
            {
                count = 0;
            }
        }

        /// <summary>
        ///     Collapses doubled bytes (e.g. two 0xFF bytes are collapsed into a single 0xFF).
        /// </summary>
        /// <param name="value">The value to collapse.</param>
        /// <seealso cref="Double"/>
        public void CollapseDoubles(byte value)
        {
            if (count < 2)

                // The buffer contains less than two bytes, which means
                // there is no possibility of any doubled values.  Return
                // immediately.

                return;

            var index = 0;

            do
            {
                // Grab the next index of the value; exit
                // the loop if no more values are found.

                index = Array.IndexOf(buffer, value, index);

                // The loop is complete if (a) the value was not located,
                // or (b) the value is located at the very end of the
                // data, or (c) the value is located beyond the real
                // data and somewhere in the excess capacity of the buffer.

                if ((index == -1) || (index >= count - 1))
                    break;

                // See if the next byte is the same value.

                index++;

                if (buffer[index] == value)
                {
                    RemoveAt(index);
                }
            } while (index <= count);
        }

        /// <summary>
        ///     Searches for the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the buffer contains the specified byte.</returns>
        public bool Contains(byte value)
        {
            if (count == 0)
            {
                return false;
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    if (buffer[i] == value)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Doubles each instance of a value (e.g. a single 0xFF becomes two 0xFF bytes).
        /// </summary>
        /// <param name="value">The value to double.</param>
        /// <seealso cref="CollapseDoubles"/>
        public void Double(byte value)
        {
            // The index variable contains the current position
            // of the next value.  Start by looking for the
            // first one; the IndexOf function returns -1 if
            // the byte does not exist.

            var index = IndexOf(value);

            while (index != -1)
            {
                // The specified value was found.  Insert
                // a copy of the value at that index.

                Insert(index, value);

                // Look for the next value.  Searching
                // needs to start two characters after
                // the current index.

                index = IndexOf(value, index + 2);
            }
        }

        /// <summary>
        ///     Searches the buffer for a byte with a specified value.
        /// </summary>
        /// <param name="value">The value to locate in the buffer.</param>
        /// <returns>
        ///     Returns the 0-based index of the first byte with the specified value,
        ///     or -1 if no such byte has been stored in the buffer.
        /// </returns>
        public int IndexOf(byte value)
        {
            if (count == 0)

                // No bytes have been copied to the buffer, so the
                // specified index is guaranteed to be -1.  Do not
                // waste any further time.

                return -1;

            else
                return Array.IndexOf(buffer, value, 0, count);
        }

        /// <summary>
        ///     Searches the buffer for a value located at or after a starting index.
        /// </summary>
        /// <param name="value">The value to locate in the buffer.</param>
        /// <param name="startIndex">The starting index to begin the search.</param>
        /// <returns>
        ///     The zero-based index of the first value at or after the starting
        ///     index, or -1 if the value could not be located.
        /// </returns>
        public int IndexOf(byte value, int startIndex)
        {
            return Array.IndexOf(buffer, value, startIndex, count - startIndex);
        }

        /// <summary>
        ///     Inserts a byte into the buffer.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index in the buffer where the byte should be inserted.
        /// </param>
        /// <param name="value">
        ///     The value to insert into the buffer.
        /// </param>
        public void Insert(int index, byte value)
        {
            if ((index < 0) || (index > count))

                // The caller specified a negative index, or an
                // index beyond the end the buffer data.  Note
                // that "inserting" into the first empty slot
                // is allowed.

                throw new ArgumentOutOfRangeException("index");

            if (count == buffer.Length)

                // The buffer cannot hold this byte.  Resize
                // the buffer.  The EnsureCapacity function
                // will make sure that space is available for
                // the desired amount.

                EnsureCapacity(count + 1);

            if (index < count)
            {
                // The caller specified a position within the
                // buffer, which means all bytes after the position
                // need to be pushed outward.

                Array.Copy(
                    buffer, // sourceArray
                    index, // sourceIndex
                    buffer, // destinationArray
                    index + 1, // destinationIndex,
                    count - index); // length
            }

            // Save the value and increment the count.

            buffer[index] = value;
            count++;
        }

        /// <summary>
        ///     Inserts a byte array into the specified
        ///     location of the buffer.
        /// </summary>
        /// <param name="index">
        ///     The location where inserting should begin.
        /// </param>
        /// <param name="values">
        ///     A byte array of values to insert into the buffer.
        /// </param>
        public void Insert(int index, byte[] values)
        {
            if ((index < 0) || (index > count))

                // The caller specified a negative index, or an
                // index beyond the end the buffer data.  Note
                // that "inserting" into the first empty slot
                // is allowed.

                throw new ArgumentOutOfRangeException("index");

            if (values.Length == 0)
                return; // ? Raise exception?

            // Make sure that space is available to
            // hold the inserted values.

            EnsureCapacity(count + values.Length);

            if (index < count)
            {
                // The caller specified a position within the
                // buffer, which means all bytes after the position
                // need to be pushed outward.

                Array.Copy(
                    buffer, // sourceArray
                    index, // sourceIndex
                    buffer, // destinationArray
                    index + values.Length, // destinationIndex,
                    count - index); // length
            }

            // Save the values and increment the count.

            Array.Copy(
                values,
                0,
                buffer,
                index,
                values.Length);

            count += values.Length;
        }

        /// <summary>
        ///     Removes the byte at the specified index of the buffer.
        /// </summary>
        /// <param name="index">The zero-based index of the byte to remove.</param>
        public void RemoveAt(int index)
        {
            if ((index < 0) || (index >= count))

                // The programmer specified an index that is negative
                // or outside the valid range of bytes.

                throw new ArgumentOutOfRangeException("index");

            if (index == count - 1)
            {
                // This is the last item in the data portion
                // of the array.  Merely decrement the count
                // instead of performing an array copy.

                count--;
                return;
            }

            if (index < count)
            {
                // The programmer specified an index with the value
                // range of bytes.  All of the items after the
                // element need to be shifted inward.

                Array.Copy(
                    buffer, // sourceArray
                    index + 1, // sourceIndex,
                    buffer, // destinationArray
                    index, // destinationIndex
                    count - index - 1); // length
            }

            count--;
        }

        /// <summary>
        /// Replaces the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="newValues">The new values.</param>
        public void Replace(byte value, byte[] newValues)
        {
            if (newValues == null)
                throw new ArgumentNullException("newValues");

            var index = IndexOf(value);

            while (index != -1)
            {
                RemoveAt(index);
                Insert(index, newValues);
                index = IndexOf(value, index + newValues.Length);
            }
        }

        /// <summary>
        ///     Copies the contents of the buffer to a new array.
        /// </summary>
        /// <returns>
        ///     A new byte array containing the contents of the buffer.
        /// </returns>
        public byte[] ToArray()
        {
            // Create a new array that will hold the contents
            // of the buffer.  This will be returned to the caller.

            var returnArray = new byte[count];

            Buffer.BlockCopy(
                buffer,
                0,
                returnArray,
                0,
                count);

            // Return the new array to the caller.

            return returnArray;
        }

        /// <summary>
        ///     Ensures the internal buffer can hold a minimum
        ///     number of elements.
        /// </summary>
        /// <param name="minimum">
        ///     The minimum capacity of the buffer after resizing.
        /// </param>
        private void EnsureCapacity(int minimum)
        {
            if (buffer.Length < minimum)
            {
                // The current size of the array is less than the
                // minimum requested amount.  Try doubling the size
                // of the array to see if it exceeds the minimum.
                // The smallest possible value is 16.

                var newCapacity = buffer.Length == 0 ? 16 : buffer.Length*2;

                // If doubling does not exceed the minimum, then just
                // use the specified minimum.  Clearly the caller
                // needs a lot of space.

                if (newCapacity < minimum)
                    newCapacity = minimum;

                // Set the Capacity property to the new value.
                // It is responsible for handling the resize.

                Capacity = newCapacity;
            }
        }

        #endregion Methods
    }
}
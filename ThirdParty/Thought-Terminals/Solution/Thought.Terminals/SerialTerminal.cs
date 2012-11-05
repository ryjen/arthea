
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.IO;
using System.Text;

namespace Thought.Terminals
{

    /// <summary>
    ///     Implements a driver for serial-based remote terminals that
    ///     use ANSI X3.64 or a similar string-oriented protocol for 
    ///     controlling the display.
    /// </summary>
    /// <seealso cref="SystemTerminal"/>
    public class SerialTerminal : Terminal 
    {

        private TextStyle currentStyle;
        private ITerminalProtocol protocol;
        private Stream input;
        private Stream output;

        /// <summary>
        ///     Initializes the driver against the specified stream.
        /// </summary>
        /// <param name="stream">
        ///     A stream that will be used for both input and output.
        /// </param>
        public SerialTerminal(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.input = stream;
            this.output = stream;

        }


        /// <summary>
        ///     Initializes the driver against input and output streams.
        /// </summary>
        /// <param name="input">
        ///     The stream to be used for input.
        /// </param>
        /// <param name="output">
        ///     The stream to be used for output.
        /// </param>
        public SerialTerminal(Stream input, Stream output)
        {

            if (input == null)
                throw new ArgumentNullException("input");

            if (output == null)
                throw new ArgumentNullException("output");

            this.input = input;
            this.output = output;
        }


        #region [ Beep ]

        /// <summary>
        ///     Sends a command to sound a beep.
        /// </summary>
        public override void Beep()
        {
            output.WriteByte((byte)AsciiByte.BEL);
        }

        #endregion


		public override Stream GetStandardInput ()
		{
			throw new System.NotImplementedException ();
		}

		public override Stream GetStandardOutput ()
		{
			throw new System.NotImplementedException ();
		}

        #region [ ClearRow ]

        /// <summary>
        ///     Sends a command to clear the current row.
        /// </summary>
        public override void ClearRow()
        {
            if(protocol != null)
                Write(protocol.ClearRow());
        }

        #endregion

        #region [ ClearRowBeginning ]

        /// <summary>
        ///     Sends a command to clear the text between the start
        ///     of the row and the cursor.
        /// </summary>
        public override void ClearRowBeginning()
        {
            if(protocol != null)
                Write(protocol.ClearRowBeginning());
        }

        #endregion

        #region [ ClearRowEnding ]

        /// <summary>
        ///     Sends a command to clear the text between the
        ///     cursor and the end of the row.
        /// </summary>
        public override void ClearRowEnding()
        {
            if(protocol != null)
                Write(protocol.ClearRowEnding());
        }

        #endregion

        #region [ ClearScreen ]

        /// <summary>
        ///     Sends a command to clear the screen.
        /// </summary>
        public override void ClearScreen()
        {
            if(protocol != null)
                Write(protocol.ClearScreen());
        }

        #endregion

        #region [ ClearScreenBeginning ]

        /// <summary>
        ///     Sends a command to clear the text between the
        ///     start of the screen and the cursor.
        /// </summary>
        public override void ClearScreenBeginning()
        {
            if(protocol != null)
                Write(protocol.ClearScreenBeginning());
        }

        #endregion

        #region [ ClearScreenEnding ]

        /// <summary>
        ///     Sends a command to clear the text between
        ///     the cursor and the end of the screen.
        /// </summary>
        public override void ClearScreenEnding()
        {
            if(protocol != null)
                Write(protocol.ClearScreenEnding());
        }

        #endregion

        #region [ Close ]

        /// <summary>
        ///     Closes the remote terminal streams.
        /// </summary>
        public override void Close()
        {

            if (this.input != null)
            {
                input.Close();
                input = null;
            }

            if (this.output != null)
            {
                output.Flush();
                output.Close();
                output = null;
            }

        }

        #endregion

        #region [ GetStyle ]

        /// <summary>
        ///     Returns the current <see cref="TextStyle"/> that
        ///     is used when writing text.
        /// </summary>
        /// <returns>
        ///     The current <see cref="TextStyle"/>
        /// </returns>
        public override TextStyle GetStyle()
        {
            return this.currentStyle;
        }

        #endregion

        #region [ InputStream ]

        /// <summary>
        ///     The stream used to read from the remote terminal.
        /// </summary>
        public Stream InputStream
        {
            get
            {
                return this.input;
            }
        }

        #endregion

        #region [ MoveCursor ]

        /// <summary>
        ///     Send a command to move the cursor.
        /// </summary>
        /// <param name="column">
        ///     The zero-based column.
        /// </param>
        /// <param name="row">
        ///     The zero-based row.
        /// </param>
        public override void MoveCursor(int column, int row)
        {
            if(protocol != null)
                Write(protocol.MoveCursor(column, row));
        }

        #endregion

        #region [ MoveCursorDown ]

        /// <summary>
        ///     Sends a command to move the cursor downward.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move downward.
        /// </param>
        public override void MoveCursorDown(int rows)
        {
            if (protocol != null)
                Write(protocol.MoveCursorDown(rows));
        }

        #endregion

        #region [ MoveCursorHome ]

        /// <summary>
        ///     Sends a command to move the cursor to the
        ///     home position in the upper-left corner of
        ///     the screen.
        /// </summary>
        public override void MoveCursorHome()
        {
            if (protocol != null)
                Write(protocol.MoveCursorHome());
        }

        #endregion

        #region [ MoveCursorLeft ]

        /// <summary>
        ///     Sends a command to move the cursor to the left.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move.
        /// </param>
        public override void MoveCursorLeft(int columns)
        {
            if (protocol != null)
                Write(protocol.MoveCursorLeft(columns));
        }

        #endregion

        #region [ MoveCursorRight ]

        /// <summary>
        ///     Sends a command to move the cursor to the right.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move.
        /// </param>
        public override void MoveCursorRight(int columns)
        {
            if (protocol != null)
                Write(protocol.MoveCursorRight(columns));
        }

        #endregion

        #region [ MoveCursorUp ]

        /// <summary>
        ///     Sends a command to move the cursor upward.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move upward.
        /// </param>
        public override void MoveCursorUp(int rows)
        {
            if (protocol != null)
                Write(protocol.MoveCursorUp(rows));
        }

        #endregion

        #region [ OutputStream ]

        /// <summary>
        ///     The stream used to write to the remote terminal.
        /// </summary>
        public Stream OutputStream
        {
            get
            {
                return this.output;
            }
        }

        #endregion

        #region [ Protocol ]

        /// <summary>
        ///     The protocol driver that defines the strings
        ///     to be sent to the remote terminal when performing
        ///     screen operations.
        /// </summary>
        public ITerminalProtocol Protocol
        {
            get
            {
                return this.protocol;
            }
            set
            {
                this.protocol = value;
            }
        }

        #endregion

        #region [ ReadChar ]

        /// <summary>
        ///     Read a character from the remote terminal.
        /// </summary>
        /// <returns>
        ///     The next available character.
        /// </returns>
        public override char ReadChar()
        {
            // Read a single character from the input stream
            // using a blocking call (a call that does not
            // return until a character is available).

            int value = input.ReadByte();

            return Convert.ToChar(value);

        }

        #endregion

        #region [ SetStyle ]

        /// <summary>
        ///     Sends a command to change the color.
        /// </summary>
        public override void SetStyle(
            TextStyle newStyle)
        {

            if (protocol != null)
            {
                Write(protocol.SetStyle(this.currentStyle, newStyle));
            }

            this.currentStyle = newStyle.Clone();
        }

        #endregion

        #region [ Write(char) ]

        /// <summary>
        ///     Writes a character to the output stream.
        /// </summary>
        /// <param name="value">
        ///     The character to write to the stream.
        /// </param>
        public override void Write(char value)
        {
            Write(value.ToString());
        }

        #endregion

        #region [ Write(string) ]

        /// <summary>
        ///     Writes a string to the remote terminal.
        /// </summary>
        /// <param name="value">
        ///     The string to write to the terminal.
        /// </param>
        public override void Write(string value)
        {
            if (this.output != null)
            {

                // Get the bytes for the specified encoding.

                byte[] bytes = OutputEncoding.GetBytes(value);

                output.Write(bytes, 0, bytes.Length);

            }
        }

        #endregion

    }
}

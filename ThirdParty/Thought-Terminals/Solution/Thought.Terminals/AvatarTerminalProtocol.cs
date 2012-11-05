
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Text;

namespace Thought.Terminals
{

    /// <summary>
    ///     Encodes common terminal operations into text codes
    ///     that can be interpreted by an AVATAR terminal.
    /// </summary>
    /// <remarks>
    /// 
    ///     <para>
    ///         AVATAR is an old standard that was popular in
    ///         late 1990s as a more efficient protocol than ANSI-BBS.
    ///         However, it never reached popularity outside of the
    ///         BBS community.
    ///     </para>
    /// 
    /// </remarks>
    /// <seealso cref="AnsiTerminalProtocol"/>
    /// <seealso cref="ITerminalProtocol"/>
    public class AvatarTerminalProtocol : ITerminalProtocol
    {

        /// <summary>
        ///     Creates a new instance of the AVATAR encoder.
        /// </summary>
        public AvatarTerminalProtocol() { }


        /// <summary>
        ///     Not supported.
        /// </summary>
        public string ClearRow()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Not supported.
        /// </summary>
        public string ClearRowBeginning()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Builds an AVATAR string that clears the current line from
        ///     the cursor to the end of the row.
        /// </summary>
        /// <returns>
        ///     An AVATAR protocol string that clears the current line from
        ///     the cursor to the end of the row.  The AVATAR terminal will
        ///     clear the cell underneath the cursor in addition to subsequent
        ///     cells.
        /// </returns>
        public string ClearRowEnding()
        {
            // ^V = 0x16 (hex) or 22 (dec)
            // ^G = 0x07 (hex) or 07 (dec)

            return "\x16\x07";
        }



        /// <summary>
        ///     Builds an AVATAR string to clear the screen.
        /// </summary>
        /// <returns>
        ///     An AVATAR protocol string that will clear the screen that
        ///     position the cursor to the home position in the upper-left
        ///     corner of the screen.
        /// </returns>
        /// <remarks>
        /// 
        ///     <para>
        ///         AVATAR uses the control-L character (ASCII) twelve
        ///         to clear the screen.  This is one of the few AVATAR
        ///         codes that do not begin with a control-V prefix.
        ///     </para>
        /// 
        /// </remarks>
        public string ClearScreen()
        {
            return "\x0C";
        }


        /// <summary>
        ///     Not supported.
        /// </summary>
        public string ClearScreenBeginning()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Not supported.
        /// </summary>
        public string ClearScreenEnding()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Builds an AVATAR string to move a cursor to a position.
        /// </summary>
        /// 
        /// <param name="column">
        ///     The zero-based destination column.
        /// </param>
        /// 
        /// <param name="row">
        ///     The zero-based destination row.
        /// </param>
        /// 
        /// <remarks>
        /// 
        ///     <para>
        ///         The AVATAR cursor code is a control-V, control-G,
        ///         followed by the 1-based row and column, each specified
        ///         as a single byte.  The full control code is always
        ///         exactly four bytes in length.
        ///     </para>
        /// 
        ///     <para>
        ///         Note that AVATAR uses 1-based coordinates, whereas .NET
        ///         and terminal engine use 0-based coordinates almost exclusively.
        ///         In AVATAR, the upper-left corner of the screen has the
        ///         coordinates (1,1), not (0,0).
        ///     </para>
        /// 
        /// </remarks>
        /// 
        public string MoveCursor(int column, int row)
        {
            if (column < 0)
                throw new ArgumentOutOfRangeException("column");

            if (row < 0)
                throw new ArgumentOutOfRangeException("row");

            return "\x16\x08" + Convert.ToChar(row + 1) + Convert.ToChar(column + 1);
        }


        /// <summary>
        ///     Builds an AVATAR string that will move the cursor
        ///     downward by a specified number of rows.
        /// </summary>
        /// 
        /// <param name="rows">
        ///     The number of rows to move downward.
        /// </param>
        /// 
        /// <remarks>
        /// 
        ///     <para>
        ///         The AVATAR code for moving the cursor downward
        ///         is a control-V followed by a control-D.  This sequence
        ///         is repeated for each desired row because AVATAR does
        ///         not support a parameterized way of moving the cursor
        ///         downward.  Therefore, the length of returned code will
        ///         be 2 * rows.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="MoveCursor"/>
        public string MoveCursorDown(int rows)
        {

            if (rows < 0)
                throw new ArgumentOutOfRangeException("rows");

            else if (rows == 0)
                return string.Empty;

            else if (rows == 1)
                return "\x16\x04";

            else
            {

                StringBuilder builder = new StringBuilder(rows * 2);

                for (int i = 0; i < rows; i++)
                    builder.Append("\x16\x04");

                return builder.ToString();

            }
        }


        /// <summary>
        ///     Builds an AVATAR string that will move the cursor
        ///     to the home position (the upper-left corner).
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         AVATAR, unlike ANSI, does not support a control
        ///         code to directly move the cursor to the home position.
        ///         Instead this encoder calls the EncodeMoveCursor
        ///         function with the home coordinates (0,0).
        ///     </para>
        /// 
        /// </remarks>
        public string MoveCursorHome()
        {
            return MoveCursor(0, 0);
        }


        /// <summary>
        ///     Returns an AVATAR string that moves the cursor a specified
        ///     number of columns to the left.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move toward the left.
        /// </param>
        public string MoveCursorLeft(int columns)
        {
            if (columns < 0)
                throw new ArgumentOutOfRangeException("columns");

            else if (columns == 0)
                return string.Empty;

            else if (columns == 1)
                return "\x16\x05";

            else
            {

                StringBuilder builder = new StringBuilder(columns * 2);

                for (int i = 0; i < columns; i++)
                    builder.Append("\x16\x05");

                return builder.ToString();

            }

        }


        /// <summary>
        ///     Builds an AVATAR string that moves the cursor to the
        ///     right by the specified number of columns.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move toward the right.
        /// </param>
        public string MoveCursorRight(int columns)
        {
            if (columns < 0)
                throw new ArgumentOutOfRangeException("columns");

            else if (columns == 0)
                return string.Empty;

            else if (columns == 1)
                return "\x16\x06";

            else
            {
                StringBuilder builder = new StringBuilder(columns * 2);

                for (int i = 0; i < columns; i++)
                    builder.Append("\x16\x06");

                return builder.ToString();

            }

        }


        /// <summary>
        ///     Builds an AVATAR string that will move the
        ///     cursor upward by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move upward.
        /// </param>
        /// <returns>
        ///     A ^V followed by a ^C, repeated for each desired row.
        /// </returns>
        public string MoveCursorUp(int rows)
        {

            if (rows < 0)
                throw new ArgumentOutOfRangeException("rows");

            else if (rows == 0)
                return string.Empty;

            else if (rows == 1)
                return "\x16\x03";

            else
            {

                StringBuilder builder = new StringBuilder(rows * 2);

                for (int i = 0; i < rows; i++)
                    builder.Append("\x16\x03");

                return builder.ToString();

            }

        }

        /// <summary>
        ///     Builds an AVATAR string that changes the current style.
        /// </summary>
        /// <param name="oldStyle">
        ///     The original or current style.
        /// </param>
        /// <param name="newStyle">
        ///     The new text style.
        /// </param>
        /// <remarks>
        /// 
        ///     <para>
        ///         An AVATAR color code is a control-V, followed by
        ///         a control-A, followed by the one-byte binary value
        ///         of the color.  The high bit of the byte is stripped
        ///         to avoid problems on communications channels that
        ///         do not support 8-bit bytes.
        ///     </para>
        /// 
        ///     <para>
        ///         The oldStyle attribute is ignored.  Some text
        ///         protocols (such as ANSI-BBS) can generate more
        ///         efficient control sequences when the current style
        ///         is known.  An AVATAR color is always the same size,
        ///         regardless of the current color.
        ///     </para>
        /// 
        /// </remarks>
        /// 
        public string SetStyle(
            TextStyle oldStyle,
            TextStyle newStyle)
        {

            //if (newStyle == null)
            //    throw new ArgumentNullException("newStyle");

            // AVATAR colors are specified in the same encoding as DOS/CGA
            // color attributes.  That is, the lower four bits define the
            // foreground color and upper four bits define the background
            // color and either the intensity or blinking status.

            return "\x16\x01" + Convert.ToChar(newStyle.ToCgaAttribute(CgaHighBit.Blink));

        }

    } // class
} // terminal


/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     <para>
    ///         Builds protocol strings for performing common terminal 
    ///         operations, such as clearing the screen.
    ///     </para>
    /// </summary>
    /// <seealso cref="AnsiTerminalProtocol"/>
    /// <seealso cref="AvatarTerminalProtocol"/>
    public interface ITerminalProtocol
    {

        /// <summary>
        ///     Builds a protocol string that will clear the current row
        ///     without moving the cursor.
        /// </summary>
        /// <returns>
        ///     A protocol string that will cause the terminal to clear
        ///     the current row without moving the cursor.
        /// </returns>
        string ClearRow();


        /// <summary>
        ///     Builds a protocol string that clears the current row from
        ///     the beginning of the line to the cursor.
        /// </summary>
        /// <returns>
        ///     A protocol string that will cause the terminal to
        ///     clear all text between the start of the current row and the
        ///     cursor.
        /// </returns>
        /// <seealso cref="ClearRow"/>
        /// <seealso cref="ClearRowEnding"/>
        string ClearRowBeginning();


        /// <summary>
        ///     Builds a protocol string that clears all text between the
        ///     cursor and the end of the line using the current color.
        /// </summary>
        /// <remarks>
        ///     The character beneath the cursor is cleared; the cursor 
        ///     position itself is not moved.
        /// </remarks>
        /// <seealso cref="ClearRow"/>
        /// <seealso cref="ClearRowBeginning"/>
        string ClearRowEnding();


        /// <summary>
        ///     Builds a protocol string that clears the current screen.
        /// </summary>
        /// <seealso cref="ClearRowBeginning"/>
        /// <seealso cref="ClearRowEnding"/>
        string ClearScreen();


        /// <summary>
        ///     Builds a protocol string that clears the text between
        ///     the start of the screen and the cursor.
        /// </summary>
        /// <seealso cref="ClearScreen"/>
        /// <seealso cref="ClearScreenEnding"/>
        string ClearScreenBeginning();


        /// <summary>
        ///     Builds a protocol string that clears the text between
        ///     the cursor and the end of the screen.
        /// </summary>
        /// <seealso cref="ClearScreen"/>
        /// <seealso cref="ClearScreenBeginning"/>
        string ClearScreenEnding();


        /// <summary>
        ///     Builds a protocol string that will move the cursor to the
        ///     specified 0-based column and row coordinates.  The upper-left
        ///     corner of the screen has the coordinates (0,0).
        /// </summary>
        /// <param name="column">
        ///     The zero-based destination column.
        /// </param>
        /// <param name="row">
        ///     The zero-based destination row.
        /// </param>
        /// <returns></returns>
        string MoveCursor(int column, int row);


        /// <summary>
        ///     Builds a protocol string that moves the cursor downward
        ///     by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move downward.
        /// </param>
        /// <remarks>
        ///     Implementors should not raise an exception if the row count
        ///     is zero; instead return an empty string.  An OutOfRangeException
        ///     should be raised if the row count is negative.
        /// </remarks>
        string MoveCursorDown(int rows);


        /// <summary>
        ///     Builds a protocol string that moves the cursor to the home
        ///     position in the upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        ///     If a control code format does not support a native "home"
        ///     code, the encoder returns the results of EncodeMoveCursor(0, 0).
        ///     The home position has the coordinates (0,0).
        /// </remarks>
        string MoveCursorHome();


        /// <summary>
        ///     Builds a protocol string that moves the cursor toward the
        ///     left by the specified number of columns.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move leftward.
        /// </param>
        /// <remarks>
        ///     Implementors should not raise an exception if the column count
        ///     is zero; instead return an empty string.  An OutOfRangeException
        ///     should be raised if the column count is negative.
        /// </remarks>
        string MoveCursorLeft(int columns);


        /// <summary>
        ///     Builds a protocol string that moves the cursor toward the
        ///     right by the specified number of columns.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move toward the right.
        /// </param>
        /// <remarks>
        ///     Implementors should not raise an exception if the column count
        ///     is zero; instead return an empty string.  An OutOfRangeException
        ///     should be raised if the column count is negative.
        /// </remarks>
        string MoveCursorRight(int columns);


        /// <summary>
        ///     Builds a protocol string that moves the upward
        ///     by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move upward.
        /// </param>
        /// <remarks>
        ///     Implementors should not raise an exception if the row count
        ///     is zero; instead return an empty string.  An OutOfRangeException
        ///     should be raised if the row count is negative.
        /// </remarks>
        string MoveCursorUp(int rows);


        /// <summary>
        ///     Builds a protocol string that changes the current color.
        /// </summary>
        /// <remarks>
        ///     Both the old (current) and new color is provided because
        ///     some encoders may be able to generate more optimal codes if
        ///     the old color information is known.
        /// </remarks>
        string SetStyle(
            TextStyle oldStyle,
            TextStyle newStyle);

    }
}


/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Implements a terminal for the system console.
    /// </summary>
    /// <seealso cref="SerialTerminal"/>
    public class SystemTerminal : Terminal
    {

        #region [ Beep ]

        /// <summary>
        ///     Plays a beep on the terminal.
        /// </summary>
        public override void Beep()
        {
            base.Beep();
        }

        #endregion

        #region [ ClearRow ]

        /// <summary>
        ///     Clears the current row.
        /// </summary>
        public override void ClearRow()
        {

            // Save the current cursor position.  It will be
            // stored after the current row is cleared.

            int currentX = Console.CursorLeft;
            int currentY = Console.CursorTop;

            // Move the cursor to the beginning of the current
            // row and write spaces until the second-to-the
            // last cell.

            Console.SetCursorPosition(0, currentY);
            Console.Write(new string(AsciiChar.Space, Console.BufferWidth - 1));

            // A character is not written to the very last character
            // because it might cause undesired scrolling.  Copy a space
            // to the last character.

            Console.MoveBufferArea(0, currentY, 1, 1, Console.BufferWidth, currentY);
            Console.SetCursorPosition(currentX, currentY);

        }

        #endregion

		public override void ClearScreenBeginning ()
		{
			Console.Clear();
		}

		public override void ClearScreenEnding ()
		{
			Console.Clear();
		}

		public override System.IO.Stream GetStandardInput ()
		{
			return Console.OpenStandardInput();
		}

		public override System.IO.Stream GetStandardOutput ()
		{
			return Console.OpenStandardOutput();
		}

        #region [ ClearRowBeginning ]

        /// <summary>
        ///     Clears the text between the beginning of the current row
        ///     and the cursor.
        /// </summary>
        public override void ClearRowBeginning()
        {

            // Save the current cursor position.  It will be
            // stored after the current row is cleared.

            int currentX = Console.CursorLeft;
            int currentY = Console.CursorTop;

            // Move the cursor to the beginning of the current
            // row and write spaces until the second-to-the
            // last cell.

            Console.SetCursorPosition(0, currentY);
            Console.Write(new string(AsciiChar.Space, currentX));
            Console.SetCursorPosition(currentX, currentY);

        }

        #endregion

        #region [ ClearRowEnding ]

        /// <summary>
        ///     Clears the text between the cursor and the end of the row.
        /// </summary>
        public override void ClearRowEnding()
        {

            // Save the current cursor position.  It will be
            // stored after the text is cleared.

            int currentX = Console.CursorLeft;
            int currentY = Console.CursorTop;

            Console.Write(
                new string(AsciiChar.Space, Console.BufferWidth - currentX));

            Console.SetCursorPosition(currentX, currentY);

        }

        #endregion

        #region [ ClearScreen ]

        /// <summary>
        ///     Clears the screen.
        /// </summary>
        public override void ClearScreen()
        {
            Console.Clear();
        }

        #endregion

        #region [ ConvertBackground ]

        /// <summary>
        ///     Gets the ConsoleColor that corresponds to the background
        ///     color of a CGA/DOS color attribute.
        /// </summary>
        public ConsoleColor ConvertBackground(byte cgaColor)
        {
            return (ConsoleColor)(cgaColor >> 4);
        }

        #endregion

        #region [ ConvertForeground ]

        /// <summary>
        ///     Gets the ConsoleColor that corresponds to the foreground
        ///     color of a CGA/DOS color attribute.
        ///     Converts a CGA
        /// </summary>
        public ConsoleColor ConvertForeground(byte cgaColor)
        {
            return (ConsoleColor)(cgaColor & 0x0F);
        }

        #endregion

        #region [ GetStyle ]

        /// <summary>
        ///     Returns the current <see cref="TextStyle"/>.
        /// </summary>
        /// <returns>
        ///     The current <see cref="TextStyle"/>.
        /// </returns>
        public override TextStyle GetStyle()
        {
            return new TextStyle(
                Console.ForegroundColor,
                Console.BackgroundColor);

        }

        #endregion

        #region [ MoveCursor ]

        /// <summary>
        ///     Moves the cursor to the designated position.
        /// </summary>
        /// <param name="column">
        ///     The zero-based column number.
        /// </param>
        /// <param name="row">
        ///     The zero-based row number.
        /// </param>
        public override void MoveCursor(int column, int row)
        {
            Console.SetCursorPosition(column, row);
        }

        #endregion

        #region [ MoveCursorDown ]

        /// <summary>
        ///     Moves the cursor downward by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move downward.
        /// </param>
        public override void MoveCursorDown(int rows)
        {
            Console.SetCursorPosition(rows, Console.CursorTop + rows);
        }

        #endregion

        #region [ MoveCursorLeft ]

        /// <summary>
        ///     Moves the cursor to the left.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move to the left.
        /// </param>
        public override void MoveCursorLeft(int columns)
        {

            if (columns < 0)
                throw new ArgumentNullException("columns");

            int newColumn = Console.CursorLeft - columns;
            if (newColumn < 0)
                newColumn = 0;

            Console.SetCursorPosition(newColumn, Console.CursorTop);
        }

        #endregion

        #region [ MoveCursorHome ]

        /// <summary>
        ///     Moves the cursor to the home position.
        /// </summary>
        public override void MoveCursorHome()
        {
            Console.SetCursorPosition(0, 0);
        }

        #endregion

        #region [ MoveCursorRight ]

        /// <summary>
        ///     Moves the cursor to the right.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move to the right.
        /// </param>
        public override void MoveCursorRight(int columns)
        {
            Console.SetCursorPosition(Console.CursorLeft + columns, Console.CursorTop);
        }

        #endregion

        #region [ MoveCursorUp ]

        /// <summary>
        ///     Moves the cursor upward.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move upward.
        /// </param>
        public override void MoveCursorUp(int rows)
        {
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - rows);
        }

        #endregion

        #region [ ReadChar ]

        /// <summary>
        ///     Reads a single character from the console.
        /// </summary>
        /// <returns>
        ///     The next available character.
        /// </returns>
        public override char ReadChar()
        {

            // Read the next key.  The poorly-named intercept
            // argument tells the Console to not echo the character.

            ConsoleKeyInfo key = Console.ReadKey(true);
            return key.KeyChar;

        }

        #endregion

        #region [ SetStyle ]

        /// <summary>
        ///     Sets the color.
        /// </summary>
        public override void SetStyle(
            TextStyle style)
        {

            byte cga = style.ToCgaAttribute(CgaHighBit.BrightBackground);

            Console.ForegroundColor = (ConsoleColor)(cga & 0x0F);
            Console.BackgroundColor = (ConsoleColor)(cga >> 4);

        }

        #endregion

        #region [ Write ]

        /// <summary>
        ///     Writes the specified character to the output stream.
        /// </summary>
        /// <param name="value">
        ///     The character to write to the stream.
        /// </param>
        public override void Write(char value)
        {
            Console.Write(value);
        }

        #endregion

    }
}

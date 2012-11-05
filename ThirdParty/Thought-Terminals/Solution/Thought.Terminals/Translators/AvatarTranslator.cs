
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Translates AVATAR color and cursor codes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class interprets basic AVATAR color and cursor
    ///         codes.  AVATAR was a FidoNet proposed standard for controlling
    ///         a terminal using simple and efficient binary codes.  The
    ///         standard was supported by many DOS-based bulletin board
    ///         systems, door games, and terminal emulators.
    ///     </para>
    ///     <para>
    ///         Advanced codes, such as those that manipulate windows, are
    ///         not supported by the translator.  Only the original AVT/0
    ///         standard is supported.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AnsiTranslator"/>

    [TranslatorKey("avatar")]
    public class AvatarTranslator : Translator
    {

        /// <summary>
        ///     Creates a new AVATAR translator.
        /// </summary>
        public AvatarTranslator()
        {
        }


        /// <summary>
        ///     Builds an array of every character that marks the 
        ///     beginning of a potential AVATAR code.
        /// </summary>
        /// <returns>
        ///     An array with three characters: a control-L, a 
        ///     control-v, and a control-Y.  The Control-L character
        ///     is used to clear the screen; a Control-V character
        ///     is the prefix for most AVATAR codes; a Control-Y
        ///     character is the start of an expansion code.
        /// </returns>
        public override char[] GetPrefixChars()
        {
                // ControlL = Clear screen
                // ControlV = Start of most Avatar codes
                // ControlY = Repeat character sequence

                return new char[] { 
                    AsciiControlChar.ControlL,
                    AsciiControlChar.ControlV,
                    AsciiControlChar.ControlY };
        }


        /// <summary>
        ///     Translates an AVATAR code in an output stream.
        /// </summary>
        /// <param name="terminal">
        ///     The terminal on which to execute the control code.
        /// </param>
        /// <param name="text">
        ///     A string containing a potential AVATAR code.
        /// </param>
        /// <param name="index">
        ///     The index of the first AVATAR character.
        /// </param>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            if (text[index] == AsciiControlChar.ControlL)
            {

                // A control-L (ASCII 12) is the Avatar clear screen
                // code.  This is one of two sequences that do not
                // begin with a control-V; the other is the control-Y
                // macro.

                terminal.ClearScreen();
                return index + 1;
            }

            if (text[index] == AsciiControlChar.ControlY)
            {
                // ASCII #25 (Control-Y) marks the beginning of a code
                // that instructs the terminal to repeat a character
                // several times (this is a very simple compression 
                // technique).  The sequence is exactly three bytes
                // in length.

                if (index > text.Length - 3)
                {
                    // There isn't enough room left in the string to
                    // accomodate a full code sequence.
                    // Abort processing.

                    return index;

                }
                else
                {
                    // The AVATAR ^Y code has two parameter bytes.
                    // The first byte is the character to write; the
                    // second byte is the number of times to repeat it.

                    terminal.Write(
                      text[index + 1],
                      text[index + 2]);

                    return index + 3;

                }

            }

            // At this point, only ^V codes are left to be processed.

            if ((text[index] != AsciiControlChar.ControlV) || (index == text.Length - 1))

                // Either this code does not begin with a control-v,
                // or the code begins at the very last character in
                // the string.  Either way, abort.

                return index;

            switch (text[index + 1])
            {

                case AsciiControlChar.ControlA:

                    #region [ Color ]

                    // The ^V^A sequence is used to set the color.
                    // It is immediately followed by the DOS-style
                    // color, making the sequence exactly three bytes
                    // in length.

                    if (index > text.Length - 3)

                        // There isn't enough room to hold a full
                        // sequence of three bytes, so abort.

                        return index;

                    else
                    {

                        int newColor = text[index + 2];

                        // Strip the upper bit of the color.

                        newColor &= 0xEF;

                        terminal.SetStyle((byte)newColor, CgaHighBit.Blink);

                        return index + 3;
                    }

                    #endregion

                case AsciiControlChar.ControlB:

                    // The ^V^B sequence instructs the terminal to set
                    // the blinking bit of the current color.  On DOS-based
                    // colors, this is the highest bit.

                    terminal.SetStyle(terminal.GetStyle().CloneBlinking());
                    return index + 2;

                case AsciiControlChar.ControlC:

                    // Moves the cursor up without changing the column.
                    // The sequence is ignored if the cursor is already
                    // at the highest column (top of the screen).

                    terminal.MoveCursorUp();
                    return index + 2;

                case AsciiControlChar.ControlD:

                    // Moves the cursor down without changing the column.
                    // The sequence is ignored if the cursor is already
                    // at the bottom of the screen.

                    terminal.MoveCursorDown();
                    return index + 2;

                case AsciiControlChar.ControlE:

                    // Moves the cursor left.

                    terminal.MoveCursorLeft();
                    return index + 2;

                case AsciiControlChar.ControlF:

                    // Moves the cursor right.

                    terminal.MoveCursorRight();
                    return index + 2;

                case AsciiControlChar.ControlG:

                    // Instructs the terminal the clear the text
                    // between the cursor and the end of the line using
                    // the current color.

                    terminal.ClearRowEnding();
                    return index + 2;

                case AsciiControlChar.ControlH:

                    // Moves the cursor to the specified 1-based position.
                    // The first parameter (byte) is the row, and the second
                    // parameter is the column.

                    int row = text[index + 2];
                    int col = text[index + 3];

                    terminal.MoveCursor((byte)(col - 1), (byte)(row - 1));
                    return index + 4;

            }

            return index;
        }

    }
}

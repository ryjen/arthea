
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{
    /// <summary>
    ///     Implements an Ezycom BBS color translator.
    /// </summary>
    public class EzycomColorTranslator : Translator
    {

        /// <summary>
        ///     Creates a new instance of the Ezycom BBS color translator.
        /// </summary>
        public EzycomColorTranslator()
        {
        }


        /// <summary>
        ///     Builds a character array containing all characters that
        ///     mark the beginning of a potential Ezycom color code.
        /// </summary>
        /// <returns>
        ///     A character array containing the pipe (|) character
        ///     as a single element.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { '|' };
        }


        /// <summary>
        ///     Translates an Ezycom BBS color code at the specified position in a string.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // The first character of any Ezycom color
            // code is a pipe character.

            if (text[index] != '|')
                return index;

            // See if the pipe character is at the end of the
            // string.  If so, there is no possibility of 
            // this being a valid Ezycom color code.

            if (index == text.Length - 1)
                return index;

            // Check the next character for any of the
            // single-character color letters.

            switch (text[index + 1])
            {
                case 'b':
                    terminal.SetForeground(ConsoleColor.DarkBlue);
                    return index + 2;

                case 'g':
                    terminal.SetForeground(ConsoleColor.DarkGreen);
                    return index + 2;

                case 'c':
                    terminal.SetForeground(ConsoleColor.DarkCyan);
                    return index + 2;

                case 'r':
                    terminal.SetForeground(ConsoleColor.DarkRed);
                    return index + 2;

                case 'm':
                    terminal.SetForeground(ConsoleColor.DarkMagenta);
                    return index + 2;

                case 'p':
                    terminal.SetForeground(ConsoleColor.DarkMagenta);
                    return index + 2;

                case 'y':
                    terminal.SetForeground(ConsoleColor.DarkYellow);
                    return index + 2;

                case 'v':
                    terminal.SetForeground(ConsoleColor.DarkGray);
                    return index + 2;

                case 'B':
                    terminal.SetForeground(ConsoleColor.Blue);
                    return index + 2;

                case 'G':
                    terminal.SetForeground(ConsoleColor.Green);
                    return index + 2;

                case 'C':
                    terminal.SetForeground(ConsoleColor.Cyan);
                    return index + 2;

                case 'R':
                    terminal.SetForeground(ConsoleColor.Red);
                    return index + 2;

                case 'M':
                    terminal.SetForeground(ConsoleColor.Magenta);
                    return index + 2;

                case 'P':
                    terminal.SetForeground(ConsoleColor.Magenta);
                    return index + 2;

                case 'Y':
                    terminal.SetForeground(ConsoleColor.Yellow);
                    return index + 2;

                case 'W':
                    terminal.SetForeground(ConsoleColor.White);
                    return index + 2;

                case '0':
                case '1':

                    // This marks the beginning of a potential numeric color.
                    // Valid codes are:
                    //
                    //   |b       |01      Blue
                    //   |g       |02      Green
                    //   |c       |03      Cyan
                    //   |r       |04      Red
                    //   |m       |05      Magenta
                    //   |p       |06      Magenta (Purple)
                    //   |y       |07      Brown
                    //   |v       |08      Light Gray
                    //   |B       |09      Light Blue
                    //   |G       |10      Light Green
                    //   |C       |11      Light Cyan
                    //   |R       |12      Light Red
                    //   |M       |13      Light Magenta
                    //   |P       |14      Light Magenta (Light Purple)
                    //   |Y       |15      Yellow
                    //   |W       |16      White
                    //
                    // All numeric codes are three characters in length.
                    // The starting position of the code must be at position
                    // length-3 or earlier in the screen (otherwise there
                    // will be no room for the code).

                    if (index > text.Length - 3)
                        return index;

                    int number;
                    if (!int.TryParse(text.Substring(index + 1, 2), out number))
                        return index;

                    if ((number < 0) || (number > 16))
                        return index;

                    terminal.SetStyle((byte)number, CgaHighBit.Blink);
                    return index + 3;

            }

            return index;
        }

    }
}

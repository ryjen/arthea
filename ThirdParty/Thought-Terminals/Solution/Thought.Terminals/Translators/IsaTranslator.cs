
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Translates ISA control codes as described in the A.N.G.E.L.
    ///     BBS and door development kit.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class implements support for so-called ISO color codes.
    ///         Not much is known about the format -- a reference was found
    ///         in the documentation for the A.N.G.E.L. door development kit.
    ///         The behavior of this translator is based on documentation that
    ///         may not be exactly the same as the door kit.
    ///     </para>
    ///     <para>
    ///         An ISA code is always four characters in length.  A code starts
    ///         with a blackslash (\) and a carrot (^), followed by either the
    ///         desired color in hexadecimal format or "LF" to insert a carriage
    ///         return/line feed sequence, or "CL" to clear the screen.  The
    ///         LF and CL codes are not case-sensitive.
    ///     </para>
    ///     <para>
    ///         The A.N.G.E.L. door development kit is copyright 2002 Atlantic
    ///         Software; it was written by Jimmy Rose and Andy Stewert.
    ///     </para>
    /// </remarks>
    
    [TranslatorKey("isa")]
    public class IsaTranslator : Translator
    {

        /// <summary>
        ///     Creates a new ISA control code translator.
        /// </summary>
        public IsaTranslator() :base() { }


        /// <summary>
        ///     Builds a character array containing all characters that mark
        ///     the beginning of a potential ISA code.
        /// </summary>
        /// <returns>
        ///     A character array containing a single backslash character.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { '\\' };
        }


        /// <summary>
        ///     Translates a potential ISA control code.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // First, make sure the ISA code can fit in the string
            // given the specified index.  An ISA code is always four
            // characters in length.

            if ((index < 0) || (index > text.Length - 4))
                return index;

            // Second, make sure the first two characters are
            // the ISA prefix sequence.  This is a backslash (\) followed
            // by a carot (^).

            if ((text[index] != '\\') || (text[index + 1] != '^'))
                return index;

            if (IsHexDigit(text[index + 2]) && IsHexDigit(text[index + 3]))
            {

                // The next two characters are hexadecimal characters,
                // which means this is a color code.  The background color
                // is the first hex digit, and the foreground color is
                // the second hex digit.

                TextStyle newStyle = new TextStyle(
                    (byte)ParseHexChar(text[index + 3]),
                    (byte)ParseHexChar(text[index + 2]),
                    CgaHighBit.Blink);

                terminal.SetStyle(newStyle);

                return index + 4;

            }

            // The code was not a hexadecimal color.  See if the
            // code is "CL" (to clear the screen) or "LF" (to insert
            // a newline).

            switch (text.Substring(index + 2, 2).ToUpperInvariant())
            {
                case "CL":
                    terminal.ClearScreen();
                    return index + 4;

                case "LF":
                    terminal.WriteLine();
                    return index + 4;

            }

            // The code was not recognized.

            return index;

        }

    }
}

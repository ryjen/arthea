
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Interprets RemoteAccess BBS color codes.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         RemoteAccess was a DOS bulletin board system by Andrew
    ///         Milner of Wantree Development in Perth, Australia.  The
    ///         rights to the software were later sold to Bruce Morse
    ///         of the United States, who appears to have abandoned the
    ///         product.
    ///     </para>
    /// 
    /// </remarks>
    [TranslatorKey("ra")]
    public class RemoteAccessColorTranslator : Translator
    {

        /// <summary>
        ///     Creates a new RemoteAccess color translator.
        /// </summary>
        public RemoteAccessColorTranslator()
        {
        }


        /// <summary>
        ///     Builds an array containing all characters that mark the
        ///     start of a supported RemoteAccess control code.
        /// </summary>
        /// <returns>
        ///     A character array containing a Control-K and a Control-L.
        ///     The Control-K marks the start of a color code; the 
        ///     Control-L clears the screen.
        /// </returns>
        public override char[] GetPrefixChars()
        {

            // Control-K: Control Code Prefix
            // Control-L: Clear Screen

            return new char[] { 
                AsciiControlChar.ControlK,
                AsciiControlChar.ControlL };
        }


        /// <summary>
        ///     Translates a potential RemoteAccess control code.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // As a sanity check, make sure the first character is 
            // a control-k.  If not, the translator was improperly 
            // registered with the translation engine.

            if (
                (text[index] != AsciiControlChar.ControlK) &&
                (text[index] != AsciiControlChar.ControlL))

                return index;

            if (text[index] == AsciiControlChar.ControlL)
            {
                // This is a ^L character, which tells RemoteAccess
                // to clear the screen.  

                terminal.ClearScreen();
                return index + 1;
            }

            // This control code has a ^K prefix (the ^L was
            // already checked).  See if room is available for
            // even a minimal ^K code.

            if (index == text.Length - 1)
            {
                // The ^K is located at the very end of the string,
                // which means there is no possibility of this being
                // a RemoteAccess code.  All of the ^K codes have
                // parameters.

                return index;
            }

            if (text[index + 1] == '/')
            {
                // This is a ^K/ code, which tells RemoteAccess
                // to clear the text between the cursor and
                // the end of the line.

                terminal.ClearRowEnding();
                return index + 2;
            }

            if (text[index + 1] == '[')
            {
                // The left bracket marks the beginning of a color
                // sequence.  The next two bytes are expected to
                // be the hexadecimal value of the desired color.

                if (index > text.Length - 4)

                    // No room is available for the full length of
                    // a color code (four bytes -- a ControlK, a
                    // left bracket, and two hex characters).

                    return index;

                // Now make sure the next two bytes are valid
                // hexadecimal characters (0 through 9 and A through
                // F, case-insensitive).

                if (!(
                  (IsHexDigit(text[index + 2])) &
                  (IsHexDigit(text[index + 3]))))
                {
                    return index;
                }

                // The code is formatted properly.

                int backColor = ParseHexChar(text[index + 2]);
                int foreColor = ParseHexChar(text[index + 3]);

                terminal.SetStyle(
                    (byte)((backColor << 4) | foreColor),
                    CgaHighBit.Blink);

                return index + 4;
            }

            // The code reached this point because the code
            // was not recognized or syntactically corrected.

            return index;
        }

    }
}

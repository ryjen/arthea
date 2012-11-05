
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Interprets Nexus color codes.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         A Nexus color code has the format %FFB%, where FF is
    ///         the decimal foreground color (00 to 15) and B is the
    ///         decimal background color (0 to 7).  The code is always
    ///         exactly five characters in length.
    ///     </para>
    /// 
    /// </remarks>
    [TranslatorKey("nexus")]
    public class NexusColorTranslator : Translator
    {

        /// <summary>
        ///     Creates a new Nexus color code translator.
        /// </summary>
        public NexusColorTranslator():base()
        {
        }


        /// <summary>
        ///     Builds an array containing all characters that mark the
        ///     beginning of a potential Nexus color code.
        /// </summary>
        /// <returns>
        ///     A character array containing a percent symbol as the only element.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { '%' };
        }


        /// <summary>
        ///     Translates a Nexus color code in a string.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // A nexus code is exactly five characters in length.
            // Make sure there is enough room for the code.  The
            // HasRoom method is defined in the base Translator
            // class.

            if(!HasRoom(text, index, 5))
                return index;

            // Make sure each character looks valid.  The first
            // and last characters must a percent symbol, and the
            // middle three characters must be digits.

            if (
                !char.IsDigit(text[index + 1]) ||
                !char.IsDigit(text[index + 2]) ||
                !char.IsDigit(text[index + 3]) ||
                (text[index + 0] != '%') ||
                (text[index + 4] != '%'))
            {
                return index;
            }

            // Extract the foreground and background colors.

            byte foreground = Convert.ToByte(text.Substring(index + 1, 2));
            byte background = Convert.ToByte(text.Substring(index + 3, 1));

            // The foreground can range from 0 (black) to 15 (white), while
            // the background can range from 0 (black) to 7 (gray).

            if ((foreground > 15) || (background > 7))
                return index;

            // At this point, the code is valid in both syntax and
            // foreground/background colors.  Set the color of the terminal.

            terminal.SetStyle(foreground, background, CgaHighBit.Blink);

            // Return the new index (where processing should continue).

            return index + 5;

        }

    }
}

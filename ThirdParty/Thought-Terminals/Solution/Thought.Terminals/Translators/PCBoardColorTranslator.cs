
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Translates PCBoard color codes.  These codes have the
    ///     format @XX@, where XX is the padded, two-digit hexadecimal
    ///     value of a color.
    /// </summary>
    [TranslatorKey("pcboard")]
    public class PCBoardColorTranslator : Translator
    {

        /// <summary>
        ///     Creates a new PCBoard color code translator.
        /// </summary>
        public PCBoardColorTranslator()
        {
        }

        #region [ GetPrefixChars ]

        /// <summary>
        ///     Returns an character array containing a single
        ///     "at" (@) symbol.
        /// </summary>
        public override char[] GetPrefixChars()
        {
            return new char[] { '@' };
        }

        #endregion

        #region [ Translate ]

        /// <summary>
        ///     Translates a potential PCBoard BBS color code.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            if (index > text.Length - 4)

                // All PCBoard color codes are exactly four characters
                // in length.  The index being checked if closer than
                // four characters to the end of the string, so therefore
                // this cannot be a valid color code.

                return index;

            else if (
              (text[index] != '@') ||
              (text[index + 3] != '@') ||
              (!IsHexDigit(text[index + 1])) ||
              (!IsHexDigit(text[index + 2])))
            {
                // (a) the first character is not an at-symbol (@), or
                // (b) the fourth character is not an at-symbol (@), or
                // (c) the second character is not a hexchar, or 
                // (d) the third character is not a hexchar

                return index;
            }

            else
            {

                // This is a valid PC Board color code.  Per
                // standard conventions, the first hex character
                // specifies the upper four bits, and the second
                // hex character specifies the lower four bits.

                byte backColor = (byte)ParseHexChar(text[index + 1]);
                byte foreColor = (byte)ParseHexChar(text[index + 2]);

                terminal.SetStyle(
                    new TextStyle(foreColor, backColor, CgaHighBit.Blink));

                return index + 4;
            }

        }

        #endregion

    }
}

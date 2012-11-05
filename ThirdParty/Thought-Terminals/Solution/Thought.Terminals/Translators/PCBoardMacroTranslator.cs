
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Translates PCBoard macro codes like "@CLS@".
    /// </summary>
    public class PCBoardMacroTranslator : Translator 
    {

        /// <summary>
        ///     The at-symbol (@) prefix character.
        /// </summary>
        public const char AtSymbol = '@';


        /// <summary>
        ///     Builds an array containing all characters that mark the
        ///     beginning of a PCBoard color code.
        /// </summary>
        /// <returns>
        ///     A character array containing the at-symbol (@) as the
        ///     only element.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { AtSymbol };
        }


        /// <summary>
        ///     Translates PCBoard color codes in a string.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            if (text[index] != AtSymbol)
                return index;

            // Get the index of the suffix character.  The macro
            // name is between these characters.

            int suffixIndex = text.IndexOf(AtSymbol, index + 1);
            if (suffixIndex == -1)
                return index;

            // Extract the macro name.

            string name =
                text.Substring(index + 1, suffixIndex - index - 1).Trim();

            if (name.Length == 0)
                return index;

            switch (name.ToLowerInvariant())
            {

                case "crleol":
                    terminal.ClearRowEnding();
                    break;

                case "cls":
                    terminal.ClearScreen();
                    break;

                case "hangup":
                    terminal.Close();
                    break;

                case "more":
                    terminal.PromptEnter();
                    break;

                case "pause":
                    terminal.PromptEnter();
                    break;

                case "wait":
                    terminal.PromptEnter();
                    break;

                default:
                    return index;
            }

            return suffixIndex + 1;

        }
    }
}

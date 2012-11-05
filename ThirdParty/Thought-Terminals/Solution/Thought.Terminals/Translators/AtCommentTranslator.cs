
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{
    /// <summary>
    ///     Translates at-comments.
    /// </summary>
    /// <remarks>
    /// 
    ///     <para>
    ///         This translator allows the sysop to place comments
    ///         in a text file.  A comma is prefixed with the two
    ///         characters "@;".  Anything after the at-symbol and
    ///         semicolon are ignored, until the end of the line.
    ///     </para>
    /// 
    /// </remarks>
    [TranslatorKey("comments")]
    public class AtCommentTranslator : Translator
    {

        /// <summary>
        ///     Returns an array containing a single at-symbol (@).
        ///     This is used by the terminal engine to identify this
        ///     translator as a potential handler when the symbol
        ///     is encountered in an output string.
        /// </summary>
        public override char[] GetPrefixChars()
        {
            return new char[] { '@' };
        }


        /// <summary>
        ///     Translates an at-comment in an output string.
        /// </summary>
        /// <param name="terminal">
        ///     The destination terminal of the string.
        /// </param>
        /// <param name="text">
        ///     A string containing a potential control code.
        /// </param>
        /// <param name="index">
        ///     The index of the control code.
        /// </param>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // As a santity check (against misuse of the translator),
            // make sure the string actually contains an at-symbol
            // at the specified index.

            if (text[index] != '@')
                return index;

            // Make sure the at-symbol is not at the very end
            // of the string.  If so, this cannot possibly be a comment
            // because the prefix is two characters.

            if (index == text.Length - 1)
                return index;

            // Make sure the next character is a semicolon.  An at-symbol
            // and a semicolon mark the beginning of a line comment.

            if (text[index + 1] != ';')
                return index;

            // This is a comment.  The next step is to scan through the
            // string, looking for the end of the line or the end of
            // the string.

            int endIndex = text.IndexOfAny(
                new char[] { '\xD', '\xA' }, index);

            // See if any of the newline characters were located.
            // If not, the end of the string was reached (and all
            // of the subsequent text is part of the comment).

            if (endIndex == -1)
                return text.Length;

            // The endIndex variable points to the first character
            // of the end-of-line sequence.  The end-of-line
            // sequence might be a carriage-return/line-feed pair,
            // or just a line-feed.

            if (text[endIndex] == '\xA')
            {
                // The end of the line is marked with a line feed.
                // This is typical of unix-style files.

                return endIndex + 1;
            }
            else
            {

                // The end of the line is marked with a carriage-return.
                // If this is the last character in the line, return
                // immediately.

                if (endIndex == text.Length - 1)
                    return endIndex + 1;

                // If the next character is a line-feed, treat the line
                // feed character as part of the "end of line" character.
                // This is typical of MS-DOS/Windows systems.

                if (text[endIndex + 1] == '\xA')
                    return endIndex + 2;
                else
                    return endIndex + 1;
            }

        }


    }
}

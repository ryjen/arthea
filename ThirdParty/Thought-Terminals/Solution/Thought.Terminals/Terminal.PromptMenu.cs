
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Text;

namespace Thought.Terminals
{
    public partial class Terminal
    {

        /// <summary>
        ///     The settings for a menu prompt.
        /// </summary>
        public class MenuPromptSettings
        {

            /// <summary>
            ///     The style of the prompt brackets.
            /// </summary>
            public TextStyle BracketStyle;

            /// <summary>
            ///     The style of the default hotkey.
            /// </summary>
            public TextStyle DefaultHotKeyStyle;

            /// <summary>
            ///     The style of a hotkey.
            /// </summary>
            public TextStyle HotKeyStyle;

            /// <summary>
            ///     The left bracket character.
            /// </summary>
            public char? LeftBracket;

            /// <summary>
            ///     The right bracket character.
            /// </summary>
            public char? RightBracket;

            /// <summary>
            ///     The style of the prompt text.
            /// </summary>
            public TextStyle PromptTextStyle;

            /// <summary>
            ///     The separator character.
            /// </summary>
            public char? Separator;

            /// <summary>
            ///     The style of the separator character.
            /// </summary>
            public TextStyle SeparatorStyle;

            /// <summary>
            ///     Whether or not to write the menu.
            /// </summary>
            public bool WriteMenu;

            /// <summary>
            ///     Whether or not to write a new line character when finished.
            /// </summary>
            public bool WriteNewLine;

            /// <summary>
            ///     Whether or not to write the word that corresponds to the entry.
            /// </summary>
            public bool WriteWord;

            /// <summary>
            ///     The style of the word that corresponds to a menu item.
            /// </summary>
            public TextStyle WordStyle;

        }

        public char PromptMenu(
            string text,
            char[] menuChars,
            char defaultChar,
            string[] menuWords,
            MenuPromptSettings settings)
        {

            if (menuChars == null)
                throw new ArgumentNullException("menuChars");

            // If words are being provided in conjunction with
            // the characters, then both arrays need to have
            // the same length.

            if (menuWords != null)
            {
                if (menuChars.Length != menuWords.Length)
                    throw new ArgumentException(null, "menuWords");
            }

            // Save the current text style onto the stack (so it
            // can be restored when the prompt exits).  In cases
            // where a style is not defined in the settings, then
            // this style will be used.

            PushStyle();

            // Write the menu text, if specified.  The style
            // of the text can be defined, and also the text
            // may contain embedded color codes.  Any color
            // changes are reverted after writing the text.

            if (!string.IsNullOrEmpty(text))
            {

                PushStyle();
                if (settings.PromptTextStyle != null)
                    SetStyle(settings.PromptTextStyle);

                Translate(text);
                PopStyle();

            }

            if (settings.WriteMenu)
            {
                // Write the left bracket, if specified.  The style
                // of the bracket can be defined in the settings.
                // Any style changes are reverted after writing.

                if (settings.LeftBracket.HasValue)
                {

                    if (settings.BracketStyle == null)
                    {
                        Write(settings.LeftBracket.Value);
                    }
                    else
                    {
                        PushStyle(settings.BracketStyle);
                        Write(settings.LeftBracket.Value);
                        PopStyle();
                    }

                }

                // Loop through each character and write it
                // to the terminal.  A separator can be written
                // between each character.

                for (int i = 0; i < menuChars.Length; i++)
                {

                    char c = menuChars[i];

                    if (char.IsControl(c) || char.IsWhiteSpace(c))
                    {
                        // This is a control or whitespace character
                        // and will not be displayed on the prompt.
                        // However, it will still be accepted as
                        // a user entry.

                    }
                    else
                    {
                        Write(c);
                    }

                    // If this character is not the last character in
                    // the list, and also a separator character has 
                    // been specified, then write the separator.

                    if ((i < menuChars.Length - 1) && (settings.Separator.HasValue))
                    {

                        if (settings.SeparatorStyle == null)
                        {
                            Write(settings.Separator.Value);
                        }
                        else
                        {
                            PushStyle(settings.SeparatorStyle);
                            Write(settings.Separator.Value);
                            PopStyle();
                        }

                    }

                }

                // Output the right bracket, if specified.  The style
                // of the bracket can be defined in the settings.
                // Any style changes are reverted after writing.

                if (settings.RightBracket.HasValue)
                {

                    if (settings.BracketStyle == null)
                    {
                        Write(settings.RightBracket.Value);
                    }
                    else
                    {
                        PushStyle(settings.BracketStyle);
                        Write(settings.RightBracket.Value);
                        PopStyle();
                    }

                }

            }

            // Expand the list of valid characters.  In addition
            // to the printable characters specified by the
            // user, the menu prompt also accepts a carriage return.

            StringBuilder builder = new StringBuilder(menuChars.Length + 2);
            builder.Append(menuChars);
            builder.Append(defaultChar); // Just in case not in array??
            builder.Append(AsciiChar.CR);

            // Wait for the user to press one of the menu characters.

            char entry = ReadChar(builder.ToString(), true);

            if (entry == AsciiChar.CR)
            {
                entry = defaultChar;
            }

            // Write the word that corresponds to this character,
            // if enabled by the menu prompt settings.  Any style
            // changes are reverted after writing the word.

            if (settings.WriteWord)
            {

                int entryIndex = Array.IndexOf<char>(menuChars, entry);
                if (entryIndex != -1)
                {

                    if (settings.WordStyle == null)
                    {
                        PushStyle();
                    }
                    else
                    {
                        PushStyle(settings.WordStyle);
                    }

                    Translate(menuWords[entryIndex]);
                    PopStyle();

                }

            }

            // The last step is to write a newline character
            // if enabled in the menu prompt settings.

            if (settings.WriteNewLine)
                WriteLine();

            return entry;

        }

    }
}

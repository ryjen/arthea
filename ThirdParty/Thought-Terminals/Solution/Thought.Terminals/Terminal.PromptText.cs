
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
        ///     Settings for a text prompt.
        /// </summary>
        public class TextPromptSettings
        {

            /// <summary>
            ///     The character that fills the background of the prompt.
            /// </summary>
            public char BackgroundCharacter = ' ';

            /// <summary>
            ///     The style to fill out the background of the prompt.
            /// </summary>
            public TextStyle BackgroundStyle;

            /// <summary>
            ///     The style of any border characters.
            /// </summary>
            public TextStyle BorderStyle;

            /// <summary>
            ///     The character to display instead of the entered character (e.g. for passwords).
            /// </summary>
            public char? EntryCharacter;

            /// <summary>
            ///     The style of characters entered into the prompt.
            /// </summary>
            public TextStyle EntryStyle;

            /// <summary>
            ///     Whether or not the prompt should fill out its size.
            /// </summary>
            public bool FillOut = true;

            /// <summary>
            ///     The text to write to the left of the entry prompt.
            /// </summary>
            public string LeftBorder;

            /// <summary>
            ///     The length of the prompt.
            /// </summary>
            public int? Length;

            /// <summary>
            ///     The maximum length of the entered text.
            /// </summary>
            public int? MaximumEntryLength;

            /// <summary>
            ///     The minimum length of the entered text.
            /// </summary>
            public int? MinimumEntryLength;

            /// <summary>
            ///     The style of the prompt text.
            /// </summary>
            public TextStyle PromptStyle;

            /// <summary>
            ///     The text to write to the right of the entry prompt.
            /// </summary>
            public string RightBorder;

             /// <summary>
            ///     Whether or not the prompt should write a newline when finished.
            /// </summary>
            public bool WriteNewLine = true;

        }

        public string PromptText(
            string promptText,
            string entryText,
            int length)
        {

            TextPromptSettings settings = new TextPromptSettings();

            settings.BackgroundCharacter = ' ';
            settings.FillOut = true;
            settings.LeftBorder = " [";
            settings.Length = length;
            settings.MaximumEntryLength = length;
            settings.RightBorder = "]";
            settings.WriteNewLine = true;

            return PromptText(promptText, entryText, settings);

        }

        public string PromptText(
            string promptText,
            string entryText,
            TextPromptSettings settings)
        {

            if (settings == null)
                throw new ArgumentNullException("settings");

            // Write the prompt text, which is any optional text
            // shown to the left the entry prompt.  Any style
            // changes are reverted.

            if (!string.IsNullOrEmpty(promptText))
            {

                if (settings.PromptStyle == null)
                {
                    PushStyle();
                }
                else
                {
                    PushStyle(settings.PromptStyle);
                }

                Translate(promptText);
                PopStyle();

            }

            // The next step is to write the initial entry
            // text and/or fill out the prompt (if a full
            // prompt box is being rendered).

            if (settings.FillOut)
            {

                if (!settings.Length.HasValue)
                {
                    throw new InvalidOperationException();
                }

                // Write the left border if specified.  Control codes
                // in this string are not translated.  This is necessary
                // because a control code might cause the final output
                // to be a different length, thus making it difficult
                // for the function to know where to position the
                // cursor during editing.  A future version may be able
                // to make use of save/restore cursor functions that
                // are supported by some terminal types.

                if (!string.IsNullOrEmpty(settings.LeftBorder))
                {
                    if (settings.BorderStyle == null)
                    {
                        Write(settings.LeftBorder);
                    }
                    else
                    {
                        PushStyle(settings.BorderStyle);
                        Write(settings.LeftBorder);
                        PopStyle();
                    }
                }

                // The next step is to write out the initial
                // entry text specified by the caller.  This is
                // the text that is stuffed into the input.

                if (!string.IsNullOrEmpty(entryText))
                {

                    if (settings.EntryStyle != null)
                        PushStyle(settings.EntryStyle);

                    // The prompt can echo a different character
                    // than the character typed by the user.
                    // This is useful for password prompts and
                    // other types of prompts containing sensitive
                    // information.

                    if (settings.EntryCharacter == null)
                    {
                        Write(entryText);
                    }
                    else
                    {
                        Write(settings.EntryCharacter.Value, entryText.Length);
                    }

                    if (settings.EntryStyle != null)
                        PopStyle();

                }

                // The next step is to fill out the rest of the entry prompt.
                // For example, if the prompt is 20 characters in length and
                // the initial entry is 15 characters, then another 5 background
                // characters are left.

                int remainingFiller = string.IsNullOrEmpty(entryText)
                    ? settings.Length.Value
                    : settings.Length.Value - entryText.Length;

                if (remainingFiller > 0)
                {

                    if (settings.BackgroundStyle == null)
                    {
                        Write(settings.BackgroundCharacter, remainingFiller);
                    }
                    else
                    {
                        PushStyle(settings.BackgroundStyle);
                        Write(settings.BackgroundCharacter, remainingFiller);
                        PopStyle();
                    }

                }

                // The right border needs to be written next.
                // Like the left border, control codes are not expanded
                // because this would make it difficult for the
                // function to keep track of the cursor position.

                if (!string.IsNullOrEmpty(settings.RightBorder))
                {
                    if (settings.BorderStyle == null)
                    {
                        Write(settings.RightBorder);
                    }
                    else
                    {
                        PushStyle(settings.BorderStyle);
                        Write(settings.RightBorder);
                        PopStyle();
                    }
                }

                // The cursor is now one column to the immediate right
                // of the border.  Move the cursor leftward to the
                // end of the initial text.  This is where the user
                // will begin editing.
                //
                // The cursor needs to be moved by:
                //
                // (a) the length of the prompt
                // (b) plus the length of the right border
                // (c) minus the length of the initial entry

                MoveCursorLeft(
                    (settings.Length.Value) +
                    (settings.RightBorder == null ? 0 : settings.RightBorder.Length) +
                    (string.IsNullOrEmpty(entryText) ? 0 : -entryText.Length));

            }
            else
            {

                // The prompt is not being filled out.  Therefore
                // only the initial text needs to be written.
                // The behavior of the prompt is similar to the
                // ReadLine method of System.Console.

                Write(entryText);

            }

            // The prompt has now been rendered.  The cursor is located
            // at the next cell where the next character will be echoed.
            // The rest of the function deals with input only.

            StringBuilder buffer = new StringBuilder(entryText);

            PushStyle();

            while (true)
            {

                // Wait for the user to enter the next character.
                // The ReadChar() function is blocking, meaning that
                // it will not return until a character is available.

                char entry = ReadChar();

                switch (entry)
                {

                    case AsciiChar.BS: // Backspace

                        // The user pressed the backspace key.
                        // The character to the left of the cursor
                        // needs to be erased (replaced by a filler),
                        // then the cursor needs to be shifted.

                        if (buffer.Length > 0)
                        {

                            Write(AsciiChar.BS);

                            if (settings.BackgroundStyle != null)
                                PushStyle(settings.BackgroundStyle);

                            Write(settings.BackgroundCharacter);
                            Write(AsciiChar.BS);

                            if (settings.BackgroundStyle != null)
                                PopStyle();

                            buffer.Remove(buffer.Length - 1, 1);

                        }
                        break;

                    case AsciiChar.LF: // Linefeed

                        // The linefeed can be ignored.

                        break;

                    case AsciiChar.CR: // Enter/Carriage Return

                        // The user pressed the Enter key to
                        // submit the current text.  Make sure
                        // the text meets the minimum length.

                        if ((settings.MinimumEntryLength.HasValue) &&
                            (buffer.Length < settings.MinimumEntryLength.Value))
                        {

                            // The minimum length has not been attained.

                            Beep();
                        }
                        else
                        {
                            PopStyle();

                            if (settings.WriteNewLine)
                                WriteLine();

                            return buffer.ToString();
                        }
                        break;

                    default:

                        // The user entered a regular character
                        // that will become part of the entry text.

                        if ((settings.MaximumEntryLength.HasValue) &&
                            (buffer.Length == settings.MaximumEntryLength.Value))
                        {

                            // The maximum number of characters has
                            // already been entered.

                            Beep();

                        }
                        else
                        {

                            if (settings.EntryStyle != null)
                                SetStyle(settings.EntryStyle);

                            Write(entry);
                            buffer.Append(entry);

                        }
                        break;

                }

            }

        }

    }
}

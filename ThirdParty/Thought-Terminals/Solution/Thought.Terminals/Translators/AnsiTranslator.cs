
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections;
using System.Text;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Translates ANS-BBS escape sequences.
    /// </summary>
    /// <seealso cref="AvatarTranslator"/>
    [TranslatorKey("ansi")]
    public class AnsiTranslator : Translator
    {

        private bool debugMode = false;
        private bool stripUnrecognizedCodes = true;


        /// <summary>
        ///     Creates a new ANSI-BBS translator.
        /// </summary>
        public AnsiTranslator() { }

        /// <summary>
        ///     Enables or disables debug mode.
        /// </summary>
        /// <remarks>
        ///     If enabled, unrecognized ANSI-BBS sequences are written
        ///     to the screen with the escape character changed to the
        ///     letters "ESC".  This changes the escape sequence to printable
        ///     text and allows you to see it on the remote terminal.
        /// </remarks>
        public bool DebugMode
        {
            get
            {
                return this.debugMode;
            }
            set
            {
                this.debugMode = value;
            }
        }


        /// <summary>
        ///     Returns a character array containing all prefix
        ///     characters for the ANSI-BBS protocol.
        /// </summary>
        /// <returns>
        ///     A character array containing only the escape (ASCII 27)
        ///     character.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { AsciiChar.ESC };
        }


        /// <summary>
        ///     Parses an ANSI escape sequence.
        /// </summary>
        /// <param name="text">
        ///     A string containing an ANSI escape sequence.
        /// </param>
        /// <param name="index">
        ///     The starting index of the escape sequence.
        /// </param>
        /// <param name="ansiLength">
        ///     The parsed length of the escape sequence.
        /// </param>
        /// <param name="ansiParams">
        ///     The raw parameters extracted from the sequence.
        /// </param>
        /// <param name="ansiCommand">
        ///     The parsed command letter of the sequence.
        /// </param>
        /// <returns>
        ///     True if the escape sequence was correctly formatted.
        /// </returns>
        private bool ParseAnsi(
          string text,
          int index,
          ref int ansiLength,
          ref ArrayList ansiParams,
          ref char ansiCommand)
        {

            // Confirm that the string at the specified position
            // appears to be an ANSI sequence.  The first character
            // must be an escape, followed by a left bracket.
            // There must be room for at least one more character (a
            // command letter is always required -- parameters are
            // optional).

            if ((text[index] != AsciiChar.ESC) ||
              (text[index + 1] != '[') ||
              (index > text.Length - 3))
            {
                return false;
            }

            // The parser will step through each character in
            // the string, starting with the first character
            // after the left bracket.  The position is tracked
            // with the paramIndex variable.

            int paramIndex = index + 2;

            // The parser needs to keep the length of the
            // full ANSI sequence (to ensure the caller knows
            // where to begin parsing in the future).  This
            // is initialized to 2 because the first two
            // characters are the ESC and [ characters.

            int codeLength = 2;

            // The parser also needs to keep a buffer of each
            // parameter character/digit as it is being loaded.

            ArrayList parameters = new ArrayList();
            StringBuilder parameter = new StringBuilder();


            do
            {

                // The paramIndex points to the next character
                // that needs to be examined.  First, see if
                // the end of the string has been reached.

                if (paramIndex == text.Length)

                    // This is the last character in the string,
                    // and since the full code has not been extracted,
                    // this code is invalid (at least for this
                    // string).  The translator (and terminal engine)
                    // currently cannot handle codes that extend across
                    // multiple strings.

                    return false;

                // Grab the character for easier access for the
                // rest of the loop, then increment the length
                // of the sequence so far.

                char c = text[paramIndex];
                codeLength++;

                if (char.IsDigit(c))
                {
                    // This is a digit, which means it is part of
                    // a numeric parameter.  Add it to the buffer
                    // containing all characters for the parameter.

                    parameter.Append(c);
                }
                else if (c == ';')
                {
                    // This is a semicolon, which separates each
                    // parameter in the ANSI sequence.

                    parameters.Add(parameter.ToString());
                    parameter.Remove(0, parameter.Length);

                }
                else if (c == '?')
                {
                    // TODO: determine purpose of this.

                    parameter.Append(c);

                }
                else
                {
                    // This is a command character that marks
                    // the end of the sequence.  First add the
                    // current parameter (if one was being built).

                    if (parameter.Length > 0)
                        parameters.Add(parameter.ToString());

                    ansiLength = codeLength;
                    ansiParams = parameters;
                    ansiCommand = c;

                    return true;
                }

                // Move to the next character.

                paramIndex++;

            } while (true);
        }


        /// <summary>
        ///     Enables or disables filtering of unrecognized X3.64 or
        ///     compatible escape sequences.
        /// </summary>
        /// <remarks>
        ///     Unrecognized codes are filtered by default.  This is a security
        ///     precaution that stops hackers from generating sequences that
        ///     remap keystrokes or perform terminal configurations.  Such codes
        ///     could be sent in an email and blindly executed by the remote
        ///     terminal software of the target victim.
        /// </remarks>
        public bool StripUnrecognizedCodes
        {
            get
            {
                return this.stripUnrecognizedCodes;
            }
            set
            {
                this.stripUnrecognizedCodes = value;
            }
        }


        /// <summary>
        ///     Translates a potential ANSI-BBS escape sequence.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // The following parameters are returned by the
            // ParseAnsi function in this class.  The function
            // steps through an ANSI code and extracts its
            // parameters and command character.

            int ansiLength = 0;
            ArrayList ansiParams = null;
            char ansiCommand = '\x0';

            if (!ParseAnsi(
              text,
              index,
              ref ansiLength,
              ref ansiParams,
              ref ansiCommand))
            {

                // The parser return false, which means it could
                // not parse a valid Ansi sequence from the string.
                // Return the same index to the terminal engine, which
                // will recognize this to mean the translator failed.

                return index;
            }

            switch (ansiCommand)
            {

                case 'A': // ESC[xA (move cursor up)

                    #region [ Move Cursor Up ]

                    if (ansiParams.Count == 0)
                        terminal.MoveCursorUp();
                    else
                        terminal.MoveCursorUp(Convert.ToByte(ansiParams[0]));
                    break;

                    #endregion

                case 'B': // ESC[xB (move cursor down)

                    #region [ Move Cursor Down ]

                    if (ansiParams.Count == 0)
                        terminal.MoveCursorDown();
                    else
                        terminal.MoveCursorDown(Convert.ToByte(ansiParams[0]));
                    break;

                    #endregion

                case 'C': // ESC[xC (move cursor right)

                    #region [ Move Cursor Right ]

                    if (ansiParams.Count == 0)
                        terminal.MoveCursorRight();
                    else
                        terminal.MoveCursorRight(Convert.ToByte(ansiParams[0]));
                    break;

                    #endregion

                case 'D': // ESC[xD (move cursor left)

                    #region [ Move Cursor Left ]

                    if (ansiParams.Count == 0)
                        terminal.MoveCursorLeft();
                    else
                        terminal.MoveCursorLeft(Convert.ToByte(ansiParams[0]));
                    break;

                    #endregion

                case 'h': // ESC[?7h

                    // This code is placed in ANSI files generated by
                    // TheDraw.  It's purpose is unknown.

                    break;

                case 'H': // ESC[#;#H (move cursor)
                case 'f': // ESC[#;#f

                    #region [ Move Cursor ]

                    // Either sequence positions the cursor to the
                    // specified zero-based position.  The first parameter
                    // is the row and the second parameter is the
                    // column.  The origin (0,0) is at the upper-left
                    // corner of the screen.

                    if (ansiParams.Count == 0)

                        // If no parameters are specified, the home
                        // position is assumed.

                        terminal.MoveCursorHome();

                    else if (ansiParams.Count == 2)
                    {

                        int row = Convert.ToInt32(ansiParams[0]) - 1;
                        int col = Convert.ToInt32(ansiParams[1]) - 1;

                        terminal.MoveCursor(col, row);

                    }
                    break;

                    #endregion

                case 'J': //ESC[xJ (clear screen)

                    #region [ Clear Screen ]

                    // The ESC[#J sequence clears multiple lines on the
                    // screen. It has a parameter (defaulting to 0 if
                    // unspecified) that describes how to clear the screen.
                    //
                    // 0 = Clear from the cursor to the end of the screen
                    // 1 = Clear from the beginning of the screen to the cursor
                    // 2 = Clear the entire screen

                    if (ansiParams.Count == 0)

                        terminal.ClearScreenEnding();

                    else if ((string)ansiParams[0] == "2")

                        // The "2" parameter is checked first because it
                        // is the most common parameter used.  The other
                        // parameters are not highly supported.

                        terminal.ClearScreen();

                    else if ((string)ansiParams[0] == "0")

                        terminal.ClearScreenEnding();

                    else if ((string)ansiParams[0] == "1")

                        terminal.ClearScreenBeginning();

                    break;

                    #endregion

                case 'K': // ESC[xK (clear line)

                    #region [ Clear Line ]

                    // The ESC[xK sequence has a single parameter
                    // (defaulting to 0 if not specified).  The
                    // following values are defined in the specification:
                    //
                    // 0 = Clear from cursor to the end of the line
                    // 1 = Clear from the beginning of the line to the cursor
                    // 2 = Clear the line containing the cursor

                    if (ansiParams.Count == 0)

                        terminal.ClearRowEnding();

                    else
                    {
                        int code = Convert.ToInt32(ansiParams[0]);

                        switch (code)
                        {
                            case 0: terminal.ClearRowEnding(); break;
                            case 1: terminal.ClearRowBeginning(); break;
                            case 2: terminal.ClearRow(); break;
                        }
                    }

                    break;

                    #endregion

                case 'm': // ESC[xm (change attributes)

                    #region [ Change Attributes ]

                    try
                    {

                        int[] parameters = new int[ansiParams.Count];
                        for(int i = 0; i < ansiParams.Count; i++)
                        {
                            parameters[i] = Convert.ToInt32(ansiParams[i]);
                        }

                        terminal.SetStyle(new TextStyle(
                            terminal.GetStyle(),
                            parameters));

                    }
                    catch
                    {
                        return index;
                    }

                    break;

                    #endregion

                default:

                    if (this.debugMode)
                    {
                        terminal.Write("ESC");
                        terminal.Write(text.Substring(index + 1, ansiLength - 1));
                    }
                    else
                    {

                        if (!stripUnrecognizedCodes)
                        {
                            return index;
                        }
                    }

                    break;
            }

            return index + ansiLength;
        }

    }
}

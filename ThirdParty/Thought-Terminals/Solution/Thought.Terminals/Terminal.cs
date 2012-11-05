
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Timers;

namespace Thought.Terminals
{

    /// <summary>
    ///     Provides a text terminal system with support for
    ///     colors, cursor positioning, embedded control codes,
    ///     and remote terminal protocols.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The various Read functions (such as ReadChar, ReadDigit,
    ///         etc) read characters without a user interface.  All are
    ///         blocking calls that do not return until a character
    ///         is available.
    ///     </para>
    ///     <para>
    ///         The Prompt functions (such as PromptEnter, PromptBoolean)
    ///         provide visual interfaces, such as colorful prompts or
    ///         hotkey listings.  The functions are sufficient for most
    ///         applications without requiring custom code.
    ///     </para>
    /// </remarks>
    public abstract partial class Terminal
    {

        #region [ Private Fields ]

        /// <summary>
        ///     Stores the default text prompt to show when pausing.
        /// </summary>
        private string defaultPausePrompt;

        /// <summary>
        ///     Stores the default character to show when entering passwords.
        /// </summary>
        private char defaultPasswordCharacter = ' ';

        /// <summary>
        ///     Stores a character that indicates False.
        /// </summary>
        private char falseCharacter = 'N';

        /// <summary>
        ///     Stores a stack of colors.
        /// </summary>
        private Stack<TextStyle> colorStack;

        /// <summary>
        ///     The newline character (or characters).
        /// </summary>
        private string newline;

        /// <summary>
        ///     The character encoding to use when writing text.
        /// </summary>
        private Encoding outputEncoding;

        /// <summary>
        ///     Stores a list of folders containing text files.
        /// </summary>
        private string textPath;

        /// <summary>
        ///     Stores a character that indicates True.
        /// </summary>
        private char trueCharacter = 'Y';

        /// <summary>
        ///     Stores registered translators.
        /// </summary>
        private TranslatorCollection translators;

        #endregion

        /// <summary>
        ///     Creates a new terminal.
        /// </summary>
        public Terminal()
        {

            // Initialize the color stack.  The color stack
            // makes it easier for programmers to temporarily
            // change the color.  It is only necessary to
            // push a color onto the stack, then popup it out
            // later.  The original color need not be saved
            // in a separate variable.

            this.colorStack = new Stack<TextStyle>();

            // Initialize the translator collection.  The
            // collection is instructed to cache lists of
            // translators associated with each prefix character
            // used by any translator.  This improves performance
            // when translating text containing many different
            // control codes and associated translators.

            this.translators = new TranslatorCollection(true);

            this.outputEncoding = Encoding.GetEncoding(437);
            this.newline = Environment.NewLine;

        }

        #region [ Beep ]

        /// <summary>
        ///     Plays a beep on the terminal.
        /// </summary>
        public virtual void Beep()
        {
            // Do nothing
        }

        #endregion

        #region [ ClearRow ]

        /// <summary>
        ///     Clears the current row.
        /// </summary>
		public abstract void ClearRow();

        #endregion

        #region [ ClearRowBeginning ]

        /// <summary>
        ///     Clears the text between the start of the current
        ///     row and the cursor.
        /// </summary>
		public abstract void ClearRowBeginning();

        #endregion

        #region [ ClearRowEnding ]

        /// <summary>
        ///     Clears the text between the cursor and the end 
        ///     of the line using the current color.  The character
        ///     underneath the cursor is cleared.
        /// </summary>
        /// <remarks>
        ///     The cleared text is treated as invisible from the
        ///     perspective of the user; however, the remote client
        ///     may use a spaces or some other 'invisible' character 
        ///     to accomplishing the clearing.  Developers should not
        ///     assume that spaces, or nulls, or any other character
        ///     is guaranteed to fill to cleared space.
        /// </remarks>
		public abstract void ClearRowEnding();

        #endregion

        #region [ ClearScreen ]

        /// <summary>
        ///     Clears the screen and moves the cursor to 
        ///     the home position (0,0).
        /// </summary>
        /// <remarks>
        ///     
        ///     <para>
        ///         The cursor is moved to the home position (0,0)
        ///         upon return from this command.
        ///     </para>
        /// </remarks>
		public abstract void ClearScreen();

        #endregion

        #region [ ClearScreen(TextStyle) ]

        /// <summary>
        ///     Changes to the specified text style and then clears the screen.
        /// </summary>
        /// <param name="textStyle">A text color</param>
        /// <remarks>
        /// 
        ///     <para>
        ///         The specified color is set as the current color
        ///         upon return.  This means the
        ///         function is a convenient way of synchronzing
        ///         the terminal object with the actual color of the
        ///         remote terminal.  Note that upon starting a session,
        ///         the terminal does not have a reliable way of determining
        ///         the current color until it has been set through any
        ///         of the color-related methods or properties.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="ClearScreen()"/>
        public void ClearScreen(TextStyle textStyle)
        {
            SetStyle(textStyle);
            ClearScreen();
        }

        #endregion

        #region [ ClearScreenBeginning ]

        /// <summary>
        ///     Clears the text between the start of the
        ///     screen and the cursor.
        /// </summary>
		public abstract void ClearScreenBeginning();

        #endregion

        #region [ ClearScreenEnding ]

        /// <summary>
        ///     Clears the text between the cursor and the end of the screen.
        /// </summary>
		public abstract void ClearScreenEnding();

        #endregion

        #region [ Close ]

        /// <summary>
        ///     Closes the terminal session.
        /// </summary>
        public virtual void Close()
        {
            // Optionally implemented by the inherited class
        }

        #endregion

        #region [ Close(string) ]

        /// <summary>
        ///     Displays the specified text and closes
        ///     any underlying streams and other resources.
        /// </summary>
        /// <param name="text">Text to display before closing.</param>
        /// <remarks>
        ///     <para>
        ///         Control codes are interpreted.
        ///     </para>
        ///     <para>
        ///         The terminal does not wait for a user response 
        ///         before closing, although it will attempt to flush
        ///         the output to ensure the user has an opportunity
        ///         to see the text.  You may wish to use one of the
        ///         pause-related controls that are supported by
        ///         various control code translators.
        ///     </para>
        /// </remarks>

        public virtual void Close(string text)
        {
            Translate(text);
            Close();
        }

        #endregion

        #region [ DefaultPausePrompt ]

        /// <summary>
        ///     The default text to show when prompting
        ///     the user to hit the Enter key.
        /// </summary>
        public string DefaultPausePrompt
        {
            get
            {
                return this.defaultPausePrompt;
            }
            set
            {
                this.defaultPausePrompt = value;
            }
        }

        #endregion

        #region [ DefaultPasswordCharacter ]

        /// <summary>
        ///     The default character to use when displaying password prompts.
        /// </summary>
        public char DefaultPasswordCharacter
        {
            get
            {
                return this.defaultPasswordCharacter;
            }
            set
            {
                this.defaultPasswordCharacter = value;
            }

        }

        #endregion

        #region [ FalseCharacter ]

        /// <summary>
        ///     The default character to indicate a false value (e.g. 'N').
        /// </summary>
        /// <remarks>
        ///     
        ///     <para>
        ///         This character is used by the prompt functions
        ///         when asking for true/false values.  You should change
        ///         the character to whatever is appropriate for your
        ///         application and the language of the user.  Note: the
        ///         prompt functions do have overloaded versions that
        ///         allow you to specify as a method character; the
        ///         default is used only if unspecified.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="TrueCharacter"/>
        public char FalseCharacter
        {
            get
            {
                return this.falseCharacter;
            }
            set
            {
                this.falseCharacter = value;
            }
        }

        #endregion

        #region [ GetStandardInput ]

        /// <summary>
        ///     Returns the standard input stream.
        /// </summary>
		public abstract Stream GetStandardInput();

        #endregion

        #region [ GetStandardOutput ]

        /// <summary>
        ///     Returns the standard output stream.
        /// </summary>
		public abstract Stream GetStandardOutput();

        #endregion

        #region [ GetStyle ]

        /// <summary>
        ///     Returns the current style.
        /// </summary>
        /// <returns>
        ///     The current <see cref="TextStyle"/> object.
        /// </returns>
        /// <remarks>
        ///     This method must be implemented by an inheriting class.
        /// </remarks>
		public abstract TextStyle GetStyle();

        #endregion

        #region [ MoveCursor ]

        /// <summary>
        ///     Moves the cursor to a new column and row.
        /// </summary>
        /// <param name="column">Zero-based column number</param>
        /// <param name="row">Zero-based row number</param>
        /// <remarks>
        /// 
        ///     <para>
        ///       The home position (0,0) is located at the
        ///       upper-left corner of the screen.  
        ///       Classic BBS and door kits commonly used 1-based
        ///       coordinates.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="MoveCursorDown()"/>
        /// <seealso cref="MoveCursorHome"/>
        /// <seealso cref="MoveCursorLeft()"/>
        /// <seealso cref="MoveCursorRight()"/>
        /// <seealso cref="MoveCursorUp()"/>
		public abstract void MoveCursor(int column, int row);

        #endregion

        #region [ MoveCursorDown ]

        /// <summary>
        ///     Moves the cursor down by one row.
        /// </summary>
        /// <seealso cref="MoveCursor"/>
        /// <seealso cref="MoveCursorDown(int)"/>
        /// <seealso cref="MoveCursorHome"/>
        /// <seealso cref="MoveCursorLeft()"/>
        /// <seealso cref="MoveCursorRight()"/>
        /// <seealso cref="MoveCursorUp()"/>
        public virtual void MoveCursorDown()
        {
            MoveCursorDown(1);
        }

        #endregion

        #region [ MoveCursorDown(int) ]

        /// <summary>
        ///     Moves the cursor downward by a specified number of rows.
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <remarks>
        /// 
        ///     <para>
        ///         The cursor will not move beyond the bottom row
        ///         of the screen, nor will calling this method
        ///         cause the screen to scroll.
        ///     </para>
        /// </remarks>
        /// <seealso cref="MoveCursor"/>
        /// <seealso cref="MoveCursorHome"/>
        /// <seealso cref="MoveCursorDown()"/>
        /// <seealso cref="MoveCursorLeft()"/>
        /// <seealso cref="MoveCursorRight()"/>
        /// <seealso cref="MoveCursorUp()"/>
		public abstract void MoveCursorDown(int rows);

        #endregion

        #region [ MoveCursorHome ]

        /// <summary>
        ///     Moves the cursor to the home position in the
        ///     upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        ///     This call is effectively the same as calling
        ///     MoveCursor with coordinates (0, 0).  However, some
        ///     terminal protocols implement this functionality
        ///     as an efficient control code (rather than a longer
        ///     cursor positioning code).
        /// </remarks>
        public virtual void MoveCursorHome()
        {
            MoveCursor(0, 0);
        }

        #endregion

        #region [ MoveCursorLeft ]

        /// <summary>
        ///     Moves the cursor one position to the left.
        /// </summary>
        /// <remarks>
        ///     The cursor is not moved if it is already located
        ///     on the leftmost side of the screen.
        /// </remarks>
        /// <seealso cref="MoveCursor"/>
        /// <seealso cref="MoveCursorDown()"/>
        /// <seealso cref="MoveCursorHome"/>
        /// <seealso cref="MoveCursorLeft(int)"/>
        /// <seealso cref="MoveCursorRight()"/>
        /// <seealso cref="MoveCursorUp()"/>
        public virtual void MoveCursorLeft()
        {
            MoveCursorLeft(1);
        }

        #endregion

        #region [ MoveCursorLeft(int) ]

        /// <summary>
        ///     Moves the cursor toward the left by the
        ///     specified number of columns.
        /// </summary>
        /// <param name="columns">The number of columns</param>
        /// <remarks>
        /// 
        ///     <para>
        ///         The call is ignored if the number of columns
        ///         is zero (this is not treated as an out-of-range
        ///         exception because some calculations will result
        ///         in a movement of zero characters.
        ///     </para>
        /// 
        ///     <para>
        ///         The cursor is not wrapped past the leftmost side
        ///         of the screen.
        ///     </para>
        /// </remarks>
        /// <seealso cref="MoveCursor"/>
        /// <seealso cref="MoveCursorDown()"/>
        /// <seealso cref="MoveCursorHome"/>
        /// <seealso cref="MoveCursorLeft()"/>
        /// <seealso cref="MoveCursorRight()"/>
        /// <seealso cref="MoveCursorUp()"/>        
		public abstract void MoveCursorLeft(int columns);

        #endregion

        #region [ MoveCursorRight ]

        /// <summary>
        ///     Moves the cursor toward the right by one row.
        /// </summary>
        public virtual void MoveCursorRight()
        {
            MoveCursorRight(1);
        }

        #endregion

        #region [ MoveCursorRight ]

        /// <summary>
        ///     Moves the cursor toward the right by the
        ///     specified number of columns.
        /// </summary>
        /// <param name="columns">Number of columns</param>
        /// <remarks>
        ///     <para>
        ///         A column count of zero is ignored, while
        ///         a negative count will cause an out-of-range
        ///         exception.
        ///     </para>
        /// </remarks>
		public abstract void MoveCursorRight(int columns);

        #endregion

        #region [ MoveCursorUp ]

        /// <summary>
        ///     Moves the cursor upward by one row.
        /// </summary>
        public virtual void MoveCursorUp()
        {
            MoveCursorUp(1);
        }

        #endregion

        #region [ MoveCursorUp ]

        /// <summary>
        ///     Moves the cursor upward by the specified
        ///     number of rows.
        /// </summary>
        /// <param name="rows">Number of rows to move upward</param>
        /// <remarks>
        ///     A movement of zero rows is ignored.
        /// </remarks>
		public abstract void MoveCursorUp(int rows);

        #endregion

        #region [ NewLine ]

        /// <summary>
        ///     The newline character sequence.
        /// </summary>
        public string NewLine
        {
            get
            {
                return this.newline;
            }
            set
            {

                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                else
                {
                    this.newline = value;
                }

            }
        }

        #endregion

        #region [ OutputEncoding ]

        /// <summary>
        ///     The character encoding to use when writing text.
        /// </summary>
        public Encoding OutputEncoding
        {
            get
            {
                return this.outputEncoding;
            }
            set
            {
                this.outputEncoding = value;
            }
        }

        #endregion

        #region [ PopStyle ]

        /// <summary>
        ///     Sets the text style to the most recent style pushed onto
        ///     the stack, then pops that style off of the stack.
        /// </summary>
        /// <returns>
        ///     The new text style.
        /// </returns>
        /// <remarks>
        /// 
        ///     <para>
        ///         The style stack allows you to temporarily change
        ///         colors without remembering the old styles.  In generally,
        ///         you push a new style onto the stack, write any output, then
        ///         pop the style off the stack (which restores the old style).
        ///     </para>
        /// 
        /// </remarks>
        public void PopStyle()
        {
            SetStyle(this.colorStack.Pop());
        }

        #endregion

        #region [ PromptEnter ]

        /// <summary>
        ///    Waits for the user to press the enter key
        ///    using the default wait prompt.
        /// </summary>
        public void PromptEnter()
        {
            PromptEnter((string)null);
        }

        #endregion

        #region [ PromptEnter(bool) ]

        /// <summary>
        ///     Waits for the user to press the Enter key.
        ///     The default prompt can be optionally shown.
        /// </summary>
        /// <param name="showDefaultPrompt">
        ///     Indicates whether or not to show the
        ///     default prompt (defined with the
        ///     <see cref="DefaultPausePrompt"/> property).
        /// </param>
        public void PromptEnter(bool showDefaultPrompt)
        {
            if (showDefaultPrompt)
            {
                PromptEnter(this.defaultPausePrompt);
            }
            else
            {
                PromptEnter(null);
            }
        }

        #endregion

        #region [ PromptEnter(string) ]

        /// <summary>
        ///     Displays a prompt (with translation) and waits
        ///     for the user to press the Enter key.
        /// </summary>
        /// <param name="prompt">
        ///     Text containing optional control codes.
        /// </param>
        /// <remarks>
        ///     
        ///     <para>
        ///         No prompt is displayed if the specified
        ///         text is null or empty.
        ///     </para>
        /// 
        /// </remarks>
        public void PromptEnter(string prompt)
        {

            if(!string.IsNullOrEmpty(prompt))

                // The programmer specified a prompt.
                // Call the Translate() function and display it.
                // This allows the prompt to contain control
                // codes such as colors.

                Translate(prompt);

            // Wait until the Enter/Return key is pressed.

            do
            {
            } while (ReadChar() != AsciiChar.CR);

        }

        #endregion

        #region [ PromptPattern ]

        /// <summary>
        ///     Prompts the user for text based on an input pattern.
        /// </summary>
        public string PromptPattern(string pattern, bool returnFormatted)
        {

            // Two string builders will be maintained.
            // One builder is used to maintain the raw
            // entry from the user; another builder is
            // used to maintain the full formatted entry.

            StringBuilder rawText = new StringBuilder();
            StringBuilder allText = new StringBuilder();

            int index = 0;

            do
            {

                char entry;

                // Look at the next character in the
                // pattern and either write it or
                // wait for a valid character.

                char patternChar = pattern[index];

                switch (patternChar)
                {

                    case '#':
                        entry = ReadDigit();
                        rawText.Append(entry);
                        allText.Append(entry);
                        break;

                    case '_':
                        entry = ReadChar();
                        rawText.Append(entry);
                        allText.Append(entry);
                        break;

                    case '$':
                        entry = ReadLetter();
                        rawText.Append(entry);
                        allText.Append(entry);
                        break;

                    case '\xD':

                        // The user pressed the Enter/Return key.
                        // Either return the raw text or the full
                        // formatted text.

                        if (returnFormatted)
                            return allText.ToString();
                        else
                            return rawText.ToString();

                    default:

                        // This character in the pattern string
                        // is not a control character.  Write it
                        // to the output and append it to the
                        // full (formatted) string.

                        allText.Append(patternChar);
                        Write(patternChar);
                        break;

                }

            } while (true);

        }

       #endregion

        #region [ PushStyle ]

        /// <summary>
        ///     Pushes the current color onto the stack.
        ///     It can be restored later with any of the
        ///     PopColor methods.
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         The color stack provides a convenient
        ///         way of temporarily changing a color
        ///         for an output operation.  For example, a 
        ///         menu drawing method can save the current
        ///         color, display the menu items, then restore
        ///         the original when finished.
        ///     </para>
        /// 
        /// </remarks>
        public void PushStyle()
        {
            this.colorStack.Push(GetStyle());
        }

        #endregion

        #region [ PushStyle(cgaAttribute) ]

        /// <summary>
        ///     Pushes the current foreground and background colors onto
        ///     the stack, then sets the current color to the specified
        ///     CGA-encoded color attribute.
        /// </summary>
        /// <param name="cgaAttribute">
        ///     The encoded CGA attribute.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA attribute.
        /// </param>
        /// <remarks>
        ///     A CGA attribute contains the foreground encoded as the
        ///     lowest four bits and the background encoded as the upper
        ///     four bits.  
        /// </remarks>
        public void PushStyle(byte cgaAttribute, CgaHighBit highBitMeaning)
        {
            PushStyle(new TextStyle(cgaAttribute, highBitMeaning));
        }

        #endregion

        #region [ PushStyle(TextStyle) ]

        /// <summary>
        ///     Pushes a <see cref="TextStyle"/> onto the style stack.
        /// </summary>
        /// <param name="style">
        ///     A <see cref="TextStyle"/> to push onto the stack.
        /// </param>
        public void PushStyle(TextStyle style)
        {

            if (style == null)
                throw new ArgumentNullException("style");

            this.colorStack.Push(GetStyle());
            SetStyle(style);
        }

        #endregion

        #region [ ReadChar ]

        /// <summary>
        ///     Reads a single character from the user.
        /// </summary>
        /// <remarks>
        ///     This is a blocking call; it will not
        ///     return until a character is available
        ///     in the input buffer.
        /// </remarks>
        /// <returns>Character received by the user or remote terminal.</returns>
        public abstract char ReadChar();

        #endregion 

        #region [ ReadChar(char[]) ]

        /// <summary>
        ///     Waits for the user to press any of
        ///     the specified characters.
        /// </summary>
        /// <param name="validChars">
        ///     Array of valid characters.
        /// </param>
        /// <returns>
        ///     The selected character.
        /// </returns>
        /// <remarks>
        /// 
        ///     <para>
        ///         This is a blocking call; it will not return
        ///         until the user has pressed one of the characters
        ///         in the array.  Consider adding the escape
        ///         character (ASCII #27) when appropriate; this is
        ///         the most common key associated with cancelation.
        ///     </para>
        ///     <para>
        ///         The array is case-sensitive.
        ///     </para>
        /// </remarks>
        public char ReadChar(char[] validChars)
        {
            return ReadChar(validChars, false);
        }

        #endregion

        #region [ ReadChar(char[], bool) ]

        /// <summary>
        ///     Waits for the user to press any of the characters
        ///     specified in an array while optionally ignoring
        ///     capitalization.
        /// </summary>
        /// <param name="validChars">
        ///     A character array containing the valid characters
        ///     that can be sent by the user.  Other characters
        ///     are ignored.
        /// </param>
        /// <param name="ignoreCase">
        ///     True if capitalization is ignored.
        /// </param>
        /// <returns>
        ///     The character sent by the user.  The character
        ///     matches the capitalization sent by the user, not
        ///     the capitalization in the validChars array.
        /// </returns>
        public char ReadChar(char[] validChars, bool ignoreCase)
        {

            if (validChars == null)
                throw new ArgumentNullException("validChars");

            char entry;
            int index;

            do
            {
                // Read the next character from the buffer.
                // The base ReadChar() method does not return
                // until a character is available.

                entry = ReadChar();

                // Look for this exact character in the array.

                index = Array.IndexOf<char>(validChars, entry);

                if ((index == -1) && (ignoreCase) && char.IsLetter(entry))
                {

                    // The character was not located AND the
                    // user indicated that case (capitalization)
                    // should be ignored AND the entered character
                    // was a letter. 
                    //
                    // Swap the capitalization and try again.  
                    // That is, if a lower-case letter was pressed,
                    // try searching for an upper-case letter.

                    char tryAgain = char.IsLower(entry) ?
                        char.ToUpper(entry) : char.ToLower(entry);

                    index = Array.IndexOf<char>(validChars, tryAgain);

                }

            } while (index == -1);

            return entry;

        }

        #endregion

        #region [ ReadChar(string) ]

        /// <summary>
        ///    Waits for the user to press one of the
        ///    characters in the specified string.
        /// </summary>
        /// <param name="validChars">
        ///     String containing valid characters.
        /// </param>
        /// <returns>
        ///     The character pressed or sent by the remote terminal.
        /// </returns>
        public char ReadChar(string validChars)
        {
            if (string.IsNullOrEmpty(validChars))
                throw new ArgumentNullException("validChars");

            return ReadChar(validChars.ToCharArray(), false);
        }

        #endregion

        #region [ ReadChar(string, bool) ]

        /// <summary>
        ///     Waits for any character in the specified string
        ///     to be pressed, optionally ignoring capitalization.
        /// </summary>
        /// <param name="validChars">
        ///     A string containing valid characters.  Other characters
        ///     are ignored.
        /// </param>
        /// <param name="ignoreCase">
        ///     Indicates whether to ignore capitalization.
        /// </param>
        /// <returns>
        ///     The exact character sent by the user.  The character
        ///     matches the capitalization sent by the user, not the
        ///     capitalization of the character in the validChars
        ///     string.
        /// </returns>
        public char ReadChar(string validChars, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(validChars))
                throw new ArgumentNullException("validChars");

            return ReadChar(validChars.ToCharArray(), ignoreCase);
        }

        #endregion

        #region [ ReadDigit ]

        /// <summary>
        ///     Waits for the user to press a digit (0 through 9).
        ///     The character is not echoed to the terminal.
        /// </summary>
        /// <returns>
        ///     The digit pressed by the user.
        /// </returns>
        public char ReadDigit()
        {

            char entry;

            do
            {
                entry = ReadChar();

            } while (!char.IsDigit(entry));

            return entry;

        }

        #endregion

        #region [ ReadLetter ]

        /// <summary>
        ///     Waits for the user to press a character
        ///     classified as a letter.
        /// </summary>
        /// <returns>The letter pressed by the user</returns>
        public char ReadLetter()
        {
            char entry;

            do
            {

                entry = ReadChar();

            } while (!char.IsLetter(entry));

            return entry;
        }

        #endregion

        #region [ ResolveFileName ]

        /// <summary>
        ///     Given an unparsed path, attempts to locate a
        ///     text file by searching in all text folders.
        /// </summary>
        /// <param name="filename">File name to resolve</param>
        /// <returns>
        ///     Full path to the file.
        /// </returns>
        /// <seealso cref="TextPath"/>
        public string ResolveFileName(string filename)
        {

            // First, see if the specified file exists.
            // If so, return it without further processing -- the
            // name is resolved.

            if (File.Exists(filename))

                // TODO: return the filename with the current
                // folder (if the folder is not specified).
                // This avoids a subtle bug where a programmer
                // calls this method to validate a filename,
                // but changes the current folder before accessing
                // the file.

                return filename;

            // Split the text files path into its individual
            // folder names.  Each folder is expected to
            // be separated by the OS-specific path separator.
            // This is a semicolon on DOS and Windows.

            string[] folders = textPath.Split(Path.PathSeparator);

            // Loop through each folder and see if the specified
            // file exists in the folder.

            foreach (string folder in folders)
            {

                // The TextFolder set property should have
                // stopped the user from specifying an invalid
                // path -- e.g., a path with an empty folder.
                // However, just in case, check it now.

                if (folder.Length > 0)
                {

                    // Get the full name of the file as it
                    // would appear in this folder.

                    string fullName = Path.Combine(folder, filename);

                    // See if this filename exists.  If so,
                    // the file has been located and the function
                    // can exit.

                    if (File.Exists(fullName))
                        return fullName;

                }

            }

            // The code reached this point but was unable to
            // locate the filename in the current directory
            // or any directory in the text path.

            throw new FileNotFoundException();

        }

        #endregion

        #region [ SetBackground(byte, CgaHighBit) ]


        /// <summary>
        ///     Changes the current background style to the 
        ///     color encoded in a CGA attribute byte.
        /// </summary>
        /// <param name="cgaAttribute">
        ///     An encoded CGA attribute.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA attribute.
        /// </param>
        public void SetBackground(byte cgaAttribute, CgaHighBit highBitMeaning)
        {
            TextStyle style = GetStyle();
            style.SetBackground(cgaAttribute, highBitMeaning);
            SetStyle(style);

        }

        #endregion

        #region [ SetBackground(ConsolColor) ]

        /// <summary>
        ///     Changes the current background to the 
        ///     specified System.ConsoleColor.
        /// </summary>
        /// <param name="color">
        ///     A System.ConsoleColor value.
        /// </param>
        public void SetBackground(ConsoleColor color)
        {
            TextStyle style = GetStyle();
            style.SetBackground(color);
            SetStyle(style);
        }

        #endregion

        #region [ SetForeground(byte) ]

        /// <summary>
        ///     Changes the current foreground color to the 
        ///     foreground encoded in a CGA attribute.
        /// </summary>
        /// <param name="cgaAttribute">
        ///     A CGA attribute.
        /// </param>
        public void SetForeground(byte cgaAttribute)
        {
            TextStyle style = GetStyle();
            style.SetForeground(cgaAttribute);
            SetStyle(style);
        }

        #endregion

        #region [ SetForeground(ConsoleColor) ]

        /// <summary>
        ///     Changes the current foreground color
        ///     to the specified System.ConsoleColor.
        /// </summary>
        /// <param name="color">
        ///     A System.ConsoleColor value.
        /// </param>
        public void SetForeground(ConsoleColor color)
        {
            TextStyle style = GetStyle();
            style.SetForeground(color);
            SetStyle(style);
        }

        #endregion

        #region [ SetStyle(foregroundColor, backgroundColor) ]

        /// <summary>
        ///     Changes the terminal to the specified foreground
        ///     and background colors.
        /// </summary>
        /// <param name="foregroundColor">
        ///     New foreground color.
        /// </param>
        /// <param name="backgroundColor">
        ///     New background color.
        /// </param>
        public virtual void SetStyle(
            TextColor foregroundColor,
            TextColor backgroundColor)
        {
            SetStyle(new TextStyle(foregroundColor, backgroundColor));
        }

        #endregion

        #region [ SetStyle(byte, CgaHighBit) ]

        /// <summary>
        ///     Changes the current color to the style encoded
        ///     in a CGA attribute.
        /// </summary>
        /// <param name="cgaAttribute">
        ///     A CGA attribute that encodes the foreground
        ///     color and background color, and either the blinking
        ///     or background intensity.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA attribute.
        /// </param>
        public void SetStyle(byte cgaAttribute, CgaHighBit highBitMeaning)
        {
            SetStyle(new TextStyle(cgaAttribute, highBitMeaning));
        }

        #endregion

        #region [ SetStyle(byte, byte) ]

        /// <summary>
        ///     Changes the current style based on a CGA encoded color.
        /// </summary>
        /// <param name="cgaForeground">
        ///     The CGA foreground color.
        /// </param>
        /// <param name="cgaBackground">
        ///     The CGA background color and blinking/intensity bit.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA background value.
        /// </param>
        public void SetStyle(
            byte cgaForeground,
            byte cgaBackground,
            CgaHighBit highBitMeaning)
        {
            SetStyle(new TextStyle(cgaForeground, cgaBackground, highBitMeaning));
        }

        #endregion

        #region [ SetStyle(TextStyle) ]

        /// <summary>
        ///     Changes the current style.
        /// </summary>
        /// <param name="textStyle">
        ///     A text style that will be used when writing
        ///     text on the screen.
        /// </param>
        /// <remarks>
        ///     This method must be implemented by inheriting classes.
        /// </remarks>
		public abstract void SetStyle(TextStyle textStyle);

        #endregion

        #region [ TextPath ]

        /// <summary>
        ///     A list of folders that should be search for display files.
        /// </summary>
        /// <remarks>
        ///     Each folder should be separated with the OS-specific
        ///     path separator (e.g. a semicolon on DOS and Windows).
        /// </remarks>
        public string TextPath
        {
            get
            {
                return textPath;
            }
            set
            {
                textPath = value;
            }
        }

        #endregion 

        #region [ Translate(string) ]

        /// <summary>
        ///     Translates a string using the current
        ///     translators registered with the terminal.
        /// </summary>
        /// <param name="text">
        ///     The text to translate.
        /// </param>
        public void Translate(string text)
        {
            Translate(text, this.translators);
        }

        #endregion

        #region [ TranslateLine(string) ]

        /// <summary>
        ///     Translates a string using the current
        ///     translators registered with the terminal, 
        ///     then writes a newline character.
        /// </summary>
        /// <param name="text">
        ///     The text to translate.
        /// </param>
        public void TranslateLine(string text)
        {
            Translate(text, this.translators);
            WriteLine();
        }

        #endregion

        #region [ Translate(string, TranslatorCollection) ]

        /// <summary>
        ///     Displays the specified string while executing
        ///     embedded control codes.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="withTranslators">
        ///     A collection of translators that will be
        ///     valid for this call.
        /// </param>
        public void Translate(
            string text,
            TranslatorCollection withTranslators)
        {

            // Avoid any unnecessary processing and exit immediately
            // if the string is null or empty.  This is not grounds
            // for an exception.

            if(string.IsNullOrEmpty(text))
                return;

            // Avoid the overhead of the translation algorithm
            // if no actual translators are being used!

            if ((withTranslators == null) || (withTranslators.Count == 0))
            {
                Write(text);
                return;
            }

            // For maximum efficiciency, the function does not write
            // each character immediately.  Instead it waits until
            // a control code is encountered, or the end of the string
            // is reached.  The pending characters are stored in 
            // a StringBuilder object.

            StringBuilder pending = new StringBuilder();

            // The translator will step through the string.
            // Start with the first character in the string.

            int index = 0;

            while (index < text.Length)
            {

                // Grab the next character to be processed
                // by the translator.  This character
                // will be used several times in the loop.

                char nextChar = text[index];

                // Get a list of all translators that are
                // capable of handling this character.

                TranslatorCollection list =
                  withTranslators.GetPrefixTranslators(nextChar);

                if (list == null)
                {
                    // No translator can handle this character.
                    // Add the character to the pending buffer.

                    pending.Append(nextChar);
                    index++;
                }
                else
                {

                    // At least one translator is willing to give a shot
                    // at interpreting this segment of the string.  For
                    // example, the ANSI-BBS translator will be used if
                    // an escape (ASCII 27) character is encountered.
                    // Before calling the translator, write any pending
                    // characters.

                    if (pending.Length > 0)
                    {
                        Write(pending.ToString());
                        pending.Remove(0, pending.Length);
                    }

                    // The TranslateSubstring function is a private function
                    // that will call each translator and return when the
                    // first one recognizes the code.

                    int newIndex = TranslateSubstring(text, index, list);

                    if (newIndex <= index)
                    {

                        // No translator recognized the code.  Add the
                        // character to the pending buffer and move to
                        // the next character in the string.

                        pending.Append(nextChar);
                        index++;
                    }
                    else
                    {

                        // One of the translators recognized the code.
                        // It returned the index of the first character
                        // after the its code -- there is where the function
                        // will continue.

                        index = newIndex;
                    }

                }

            }

            // The string has been parsed.  However, there may be
            // pending characters waiting to be written.  Write them
            // now.

            if (pending.Length > 0)
                Write(pending.ToString());

        }

        #endregion

        #region [ TranslateSubstring (private) ]

        /// <summary>
        ///     Translates the code at a position using any of
        ///     the listed translators.  This is used internally by
        ///     the master Translate() function.
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         Before calling this function, any pending characters
        ///         must be flushed because a translator is likely
        ///         to call the terminal display functions.  If pending
        ///         characters are not written, then display calls will be
        ///         executed out of order.
        ///     </para>
        /// 
        ///     <para>
        ///         Like a regular translator, the function returns the
        ///         same index if the code was unrecognized, or a new index
        ///         at which to continue processing.
        ///     </para>
        /// </remarks>
        private int TranslateSubstring(
          string text,
          int index,
          TranslatorCollection translatorList)
        {

            foreach (Translator translator in translatorList)
            {

                // Call the translator.  It should interpret the
                // character sequence and return the new index
                // where parsing can continue.  If the character
                // is not part of a sequence, it is expected
                // to return the same index.

                int newIndex =
                  translator.Translate(this, text, index);

                if (newIndex > index)
                    return newIndex;

            }

            // The execution reached this point because no translator
            // was able to decode the specified portion of
            // the string.

            return index;

        }

    #endregion

        #region [ TranslateFile(path) ]

        /// <summary>
        ///     Displays a file while translating embedded control codes.
        /// </summary>
        /// <param name="path">Path to a text file</param>
        public void TranslateFile(string path)
        {
            TranslateFile(path, false, Encoding.GetEncoding(437));
        }

        #endregion

        #region [ TranslateFile(path, pause) ]

        /// <summary>
        ///     Translates a file using code page 437 encoding.
        /// </summary>
        /// <param name="path">
        ///     The path to the file.
        /// </param>
        /// <param name="pause">
        ///     Determines whether to pause after the file is displayed.
        /// </param>
        public void TranslateFile(string path, bool pause)
        {
            TranslateFile(
              path,
              pause,
              Encoding.GetEncoding(437));
        }

        #endregion

        #region [ TranslateFile ]

        /// <summary>
        ///     Displays a file with the specified encoding.
        /// </summary>
        /// <param name="path">
        ///     The path to the file.
        /// </param>
        /// <param name="encoding">
        ///     The encoding of the file.  DOS(tm) files typically found
        ///     on BBS systems use code page 437.
        /// </param>
        public void TranslateFile(
            string path,
            Encoding encoding)
        {
            TranslateFile(path, false, encoding);
        }

        #endregion

        #region [ TranslateFile(path, pause, encoding) ]

        /// <summary>
        ///     Writes a file to the terminal while translating any
        ///     embedded control codes.
        /// </summary>
        /// <param name="path">
        ///     The path to a text file.
        /// </param>
        /// <param name="pause">
        ///     Enables or disables a pause prompt after the file is displayed.
        /// </param>
        /// <param name="encoding">
        ///     The encoding of the text file.
        /// </param>
        public void TranslateFile(
            string path,
            bool pause,
            Encoding encoding)
        {

            if ((path == null) || (path.Length == 0))
                throw new ArgumentNullException("path");

            // Open the file with a stream reader.  The
            // ResolveFileName function returns the first
            // matching file in the current folder and
            // any folders in the text folder path.
            // An exception will be raised if the file
            // cannot be located or opened.

            StreamReader reader =
              new StreamReader(ResolveFileName(path), encoding);

            string contents;

            try
            {
                // Read the entire contents of the file and store
                // this in a local variable.  This allows the file
                // to be closed before calling the Translate() method,
                // which can lengthy (it is better to open and close
                // resources as fast as possible).

                contents = reader.ReadToEnd();

            }
            catch
            {
                throw;
            }
            finally
            {

                // Close the reader, which also closes the underlying
                // stream used by the reader.

                reader.Close();

            }

            // Call the Translate() method, which will call each
            // decoder for each control sequences in the file (e.g.,
            // ANSI color codes).

            Translate(contents);

            // If specified by the programmer, wait for the
            // enter key to be pressed.

            if (pause)
                PromptEnter();

        }

        #endregion

        #region [ Translators ]

        /// <summary>
        ///     Collection of translators.
        /// </summary>
        /// <seealso cref="Translator"/>
        public TranslatorCollection Translators
        {
            get
            {
                return translators;
            }
        }

        #endregion

        #region [ TrueCharacter ]

        /// <summary>
        ///     Specifies the default character that indicates
        ///     a True value (such as 'T').
        /// </summary>
        public char TrueCharacter
        {
            get
            {
                return this.trueCharacter;
            }
            set
            {
                this.trueCharacter = value;
            }
        }

        #endregion

        #region [ Write(char) ]

        /// <summary>
        ///     Writes a character to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The character to write.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This function must be implemented by the inheriting
        ///         class.  All other functions have default implementations
        ///         that call this function.  The other write functions can
        ///         also be overridden for performance reasons.
        ///     </para>
        /// </remarks>
        public abstract void Write(char value);

        #endregion

        #region [ Write(char, int) ]

        /// <summary>
        ///     Writes a character multiple times.
        /// </summary>
        /// <param name="value">The character to write.</param>
        /// <param name="count">The number of times to write the character</param>
        public void Write(char value, int count)
        {

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0)
            {
                ; // Ignore
            }
            else if (count == 1)
            {

                // If the count is 1, then avoid the overhead
                // of creating a multi-character string and
                // call the abstract Write function directly.

                Write(value);
            }
            else
            {
                Write(new string(value, count));
            }

        }

        #endregion

        #region [ Write(int) ]

        /// <summary>
        ///     Converts an integer to a string and writes it to
        ///     the terminal.
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(int value)
        {
            Write(value.ToString());
        }

        #endregion

        #region [ Write(string) ]

        /// <summary>
        ///     Writes a string to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The string to write.
        /// </param>
        /// <remarks>
        ///     The default implementation calls the Write(char)
        ///     function in a loop.  It is recommended that
        ///     implementors overwrite this function and implement
        ///     a more optimal method of writing a full string.
        /// </remarks>
        public virtual void Write(string value)
        {

            if (value == null)
                return;

            for (int i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }

        }

    #endregion

        #region [ WriteException ]

        /// <summary>
        ///     Writes an exception to the terminal.
        /// </summary>
        /// <param name="exception">
        ///     The exception to write.
        /// </param>
        /// <remarks>
        ///     This method is intended for debugging purposes.  The function
        ///     should not be called on production systems where revealing
        ///     exceptions may allow hackers to gain valuable information 
        ///     about the system.
        /// </remarks>
        public void WriteException(Exception exception)
        {

            if (exception == null)
                throw new ArgumentNullException("exception");

            WriteLine(exception.StackTrace);
            WriteLine(exception.Message);

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                WriteLine(exception.Message);
            }
        }

        #endregion

        #region [ WriteLine ]

        /// <summary>
        ///     Writes a blank line using the newline character.
        ///     This is a carriage return on UNIX and a return + 
        ///     linefeed on Windows and DOS.
        /// </summary>
        public void WriteLine()
        {
            // Write the newline character sequence.
            // This is a carriage return on UNIX, and
            // a return\linefeed on Windows & DOS.

            Write(this.newline);
        }

        #endregion

        #region [ WriteLine(char) ]

        /// <summary>
        ///     Writes a string to the terminal along with the
        ///     current newline character.
        /// </summary>
        public void WriteLine(char value)
        {
            Write(value + this.newline);
        }

        #endregion

        #region [ WriteLine(string) ]

        /// <summary>
        ///     Writes a string to the terminal along with the
        ///     current newline character.
        /// </summary>
        public void WriteLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write(this.newline);
            }
            else
            {
                Write(value + this.newline);
            }
        }

        #endregion

    }
}


/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Text;

namespace Thought.Terminals
{
    /// <summary>
    ///     Translates common terminal operations into control
    ///     codes that can be executed by an ANSI-BBS terminal.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         ANSI-BBS is not an official standard; it is a subset
    ///         of the X3.64 protocol.  The subset contains
    ///         the most popular control codes required by dial-up
    ///         bulletin boards, such as colors and cursor movements.
    ///         Several extensions for sound effects are also treated
    ///         as part of ANSI-BBS, although these extensions are not
    ///         supported here.
    ///     </para>
    /// 
    ///     <para>
    ///         ANSI-BBS largely correspond to the subset of codes
    ///         implemented by the ANSI.SYS driver for MS-DOS(tm).
    ///         ANSI.SYS allowed users to execute color and control
    ///         codes at the command prompt, while also allowing some
    ///         manipulation of the keyboard.  Early BBS and terminal
    ///         programs required ANSI.SYS for its displays.
    ///         Since the codes were so closely associated with ANSI.SYS,
    ///         people began incorrectly calling the subset "ANSI".
    ///         ANSI is the name of an organization, not a standard.
    ///         The terminology "ANSI-BBS" was later introduced to provide
    ///         some clarity.
    ///     </para>
    /// 
    /// </remarks>
    /// <seealso cref="AvatarTerminalProtocol"/>
    /// <seealso cref="ITerminalProtocol"/>
    public class AnsiTerminalProtocol : ITerminalProtocol
    {

        private string controlSequenceIntroducer;


        /// <summary>
        ///     Creates a new instance of the ANSI-BBS encoder.
        /// </summary>
        public AnsiTerminalProtocol()
        {
            this.controlSequenceIntroducer = "\x1B[";
        }


        /// <summary>
        ///     Creates an ANSI-BBS encoder using the specified string
        ///     as the Control Sequence Introducer (CSI).  For ANSI-BBS
        ///     the CSI is an escape character followed by the bracket "[".
        /// </summary>
        /// <param name="controlSequenceIntroducer">
        ///     The string of characters to use as a CSI.
        /// </param>
        /// <seealso cref="ControlSequenceIntroducer"/>
        public AnsiTerminalProtocol(string controlSequenceIntroducer)
        {

            if (string.IsNullOrEmpty(controlSequenceIntroducer))
            {
                throw new ArgumentNullException("controlSequenceIntroducer");
            }
            else
            {
                this.controlSequenceIntroducer = controlSequenceIntroducer;
            }
        }


        /// <summary>
        ///     Builds an ANSI escape code that clears the text
        ///     on the current row without scrolling lower text
        ///     upward and without moving the cursor position.
        /// </summary>
        /// <returns>
        ///     The code CSI 2 K.
        /// </returns>
        public string ClearRow()
        {
            return this.controlSequenceIntroducer + "2K";
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that clears the text
        ///     between the beginning of the current row and the cursor.
        /// </summary>
        /// <returns>
        ///     The code CSI 1 K.
        /// </returns>
        public string ClearRowBeginning()
        {
            return this.controlSequenceIntroducer + "1K";
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code to clear the current
        ///     line from the cursor to the end of the row.
        /// </summary>
        /// <returns>
        ///     An ANSI-BBS escape code.
        /// </returns>
        public string ClearRowEnding()
        {
            return this.controlSequenceIntroducer + 'K';
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code to clear the screen.
        /// </summary>
        /// <returns>
        ///     An ANSI-BBS escape code that clears the screen.
        /// </returns>
        public string ClearScreen()
        {
            return this.controlSequenceIntroducer + "2J";
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that clears the text
        ///     between the start of the screen and the cursor.
        /// </summary>
        /// <returns>
        ///     The code CSI 1 J.
        /// </returns>
        public string ClearScreenBeginning()
        {
            return this.controlSequenceIntroducer + "1J";
        }


        /// <summary>
        ///     Builds a code that clears the text between the
        ///     cursor and the end of the screen.
        /// </summary>
        /// <remarks>
        ///     The X3.64 sequence is CSI 0 J, where CSI is the
        ///     Control Sequence Introducer.  The zero (0) parameter
        ///     is optional; therefore for maximum efficiency the
        ///     function returns only CSI J.
        /// </remarks>
        /// <returns>
        ///     A CSI sequence followed by a capital J.
        /// </returns>
        public string ClearScreenEnding()
        {
            return this.controlSequenceIntroducer + 'J';
        }


        /// <summary>
        ///     The character or characters that mark the beginning
        ///     of a control code.  The most common CSI for DOS and
        ///     BBS systems is an escape character (ASCII 27) followed
        ///     by a left square bracket ("[").
        /// </summary>
        /// <remarks>
        ///     For safety purposes the control sequence introducer
        ///     cannot be modified after the object is created.  The
        ///     CSI can be specified to the constructor.
        /// </remarks>
        public string ControlSequenceIntroducer
        {
            get
            {
                return this.controlSequenceIntroducer;
            }
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that moves the cursor.
        /// </summary>
        /// <param name="column">
        ///     The zero-based column number.
        /// </param>
        /// <param name="row">
        ///     The zero-based row number.
        /// </param>
        /// <returns>
        ///     An X3.64 escape code that moves the cursor to the
        ///     specified column and row.  Note that X3.64 coordinates
        ///     are one-based, not zero-based.
        /// </returns>
        public string MoveCursor(int column, int row)
        {

            if (column < 0)
                throw new ArgumentOutOfRangeException("column");

            if (row < 0)
                throw new ArgumentOutOfRangeException("row");

            if ((column == 0) && (row == 0))
                return this.controlSequenceIntroducer + 'H';

            else
            {

                StringBuilder builder = new StringBuilder();

                builder.Append(this.controlSequenceIntroducer);
                builder.Append(row + 1);
                builder.Append(';');
                builder.Append(column + 1);
                builder.Append('H');

                return builder.ToString();
            }

        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code to move the cursor
        ///     downward by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move downward.
        /// </param>
        /// <returns>
        ///     An ANSI-BBS escape code to move the cursor downward
        ///     by the specified number of rows.  This is the
        ///     sequence CSI n B.
        /// </returns>
        public string MoveCursorDown(int rows)
        {
            if (rows < 0)

                // It is impossible to move a cursor by a negative
                // number of rows.  Raise an exception.

                throw new ArgumentOutOfRangeException("rows");

            else if (rows == 0)

                return string.Empty;

            else if (rows == 1)

                return this.controlSequenceIntroducer + 'B';

            else

                return this.controlSequenceIntroducer + rows.ToString() + 'B';

        }


        /// <summary>
        ///     Build an ANSI-BBS escape code that moves the cursor to
        ///     the home position.
        /// </summary>
        /// <returns>
        ///     An ANSI-BBS escape code that moves the cursor to the
        ///     home position in the upper-left corner of the screen.
        ///     This is the sequence CSI H.
        /// </returns>
        /// <remarks>
        ///     The home position is the upper-left corner of the screen.
        /// </remarks>
        public string MoveCursorHome()
        {
            return this.controlSequenceIntroducer + 'H';
        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that moves a cursor to
        ///     the left by the specified number of columns.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move left.
        /// </param>
        /// <returns>
        ///     An ANSI-BBS escape code that moves the cursor toward the left
        ///     by the specified number of columns.  An empty string is returned
        ///     if the column number is zero.  The escapde code CSI n D.
        /// </returns>
        public string MoveCursorLeft(int columns)
        {

            if (columns < 0)
                throw new ArgumentOutOfRangeException("columns");

            if (columns == 0)
                return string.Empty;

            if (columns == 1)
                return this.controlSequenceIntroducer + 'D';

            else
                return this.controlSequenceIntroducer + columns.ToString() + 'D';

        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that moves the cursor
        ///     toward the right by the specified number of columns.
        /// </summary>
        /// <param name="columns">
        ///     The number of columns to move toward the right.
        /// </param>
        /// <returns>
        ///     An ANSI-BBS escape code that moves the cursor toward the right
        ///     by the specified number of columns.  An empty string is returned
        ///     if the column count is zero.  The escape code is CSI n C.
        /// </returns>
        public string MoveCursorRight(int columns)
        {
            if (columns < 0)
                throw new ArgumentOutOfRangeException("columns");

            if (columns == 0)
                return string.Empty;

            if (columns == 1)
                return this.controlSequenceIntroducer + 'C';

            else
                return this.controlSequenceIntroducer + columns.ToString() + 'C';

        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code that moves the 
        ///     upward by the specified number of rows.
        /// </summary>
        /// <param name="rows">
        ///     The number of rows to move upward.
        /// </param>
        /// <returns>
        ///     An ANSI-BBS escape code that moves the cursor upward by the
        ///     specified number of rows.  An empty string is returned if
        ///     the row count is zero.  The escape code is CSI n A.
        /// </returns>
        public string MoveCursorUp(int rows)
        {
            if (rows < 0)
                throw new ArgumentOutOfRangeException("rows");

            if (rows == 0)
                return string.Empty;

            if (rows == 1)
                return this.controlSequenceIntroducer + "A";

            else
                return this.controlSequenceIntroducer + rows.ToString() + "A";

        }


        /// <summary>
        ///     Builds an ANSI-BBS escape code to change the current color.
        /// </summary>
        /// <param name="oldStyle">
        ///     The old or current color.
        /// </param>
        /// <param name="newStyle">
        ///     The new color.
        /// </param>
        /// <returns>
        ///     An ANSI-BBS escape code that changes the color from the
        ///     old color to the new color.
        /// </returns>
        public string SetStyle(
            TextStyle oldStyle,
            TextStyle newStyle)
        {

            StringBuilder builder = new StringBuilder();

            // All sequences begin with an escape character (ASCII
            // character 27 (dec) or 0x1B (hex), followed by
            // a right square bracket.

            builder.Append(this.controlSequenceIntroducer);

            // Since this is the unoptimal version, start by
            // reseting all attributes.  A future version will
            // consider existing attributes when generating
            // the parameters for the sequence.

            // Foreground Intensity
            //
            // In X3.64, parameter 1 sets the foreground 
            // intensity to bold; parameter 2 sets the foreground
            // intensity to faint.  Parameter 22 can be used
            // reset the intensity to normal.

            switch (newStyle.ForegroundIntensity)
            {
                case TextIntensity.Bold:
				builder.Append("1");
                    break;

                case TextIntensity.Faint:
				builder.Append("2");
                    break;

                default:
				
				builder.Append("0");

                    break;
            }

            // Italic
            //
            // In X3.64, parameter 3 enables italic text (not widely
            // supported); parameter 23 disables italic.

            if (newStyle.Italic)
            {
				builder.Append(";3");
            }

            // Underline
            //
            // In X3.64, parameter 4 sets a single underline, parameter
            // 21 sets a double underline, and parameter 24 removes
            // the underline.

            switch (newStyle.Underline)
            {
                case TextUnderline.Single:
				builder.Append(";4");
                    break;

                case TextUnderline.Double:
				builder.Append(";21");
                    break;

                case TextUnderline.None:
                    break;

            }

            // Blinking 
            //
            // In X3.64, parameter 5 indicates slow blinking
            // and parameter 6 indicates fast blinking.  Blinking
            // can be turned off with parameter 25.

            switch (newStyle.Blinking)
            {

                case TextBlinking.Slow:
				builder.Append(";5");
                    break;

                case TextBlinking.Fast:
				builder.Append(";6");
                    break;

                default:
                    break;
            }

            // Image: Negative (Reversed)
            //
            // In X3.64, parameter 7 reverses the foreground and
            // background colors; parameter 27 reverts back to 
            // the positive state.

            if (newStyle.Reversed)
            {
				builder.Append(";7");
            }

            // Conceal/Reveal
            //
            // In X3.64, parameter 8 conceals text while
            // parameter 28 reveals text.  This is not widely
            // supported.

            if (newStyle.Hidden)
            {
				builder.Append(";8");
            }

            // Strikethrough
            //
            // In X3.64, parameter 9 enables strikethrough while
            // parameter 29 disables strikethrough.

            if (newStyle.StrikeThrough)
            {
				builder.Append(";9");
            }

            // Foreground Color

            switch (newStyle.ForegroundColor)
            {
			case TextColor.Black: builder.Append(";30"); break;
			case TextColor.Blue: builder.Append(";34"); break;
			case TextColor.Green: builder.Append(";32"); break;
			case TextColor.Cyan: builder.Append(";36"); break;
			case TextColor.Red: builder.Append(";31"); break;
			case TextColor.Magenta: builder.Append(";35"); break;
			case TextColor.Yellow: builder.Append(";33"); break;
			case TextColor.White: builder.Append(";37"); break;
            }

            // Background color

            switch (newStyle.BackgroundColor)
            {
			case TextColor.Black:   builder.Append(";40"); break;
			case TextColor.Blue:    builder.Append(";44"); break;
			case TextColor.Green:   builder.Append(";42"); break;
			case TextColor.Cyan:    builder.Append(";46"); break;
			case TextColor.Red:     builder.Append(";41"); break;
			case TextColor.Magenta: builder.Append(";45"); break;
			case TextColor.Yellow:  builder.Append(";43"); break;
			case TextColor.White:   builder.Append(";47"); break;
            }

			builder.Append ("m");

            return builder.ToString();

        }

    }
}

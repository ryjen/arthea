
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections.Generic;
using System.Text;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Implements a subset of the Synchronet v3 BBS color
    ///     and cursor codes.  
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         The Synchronet BBS is one of the few packages
    ///         that survived the collapse of the BBS community after
    ///         the widespread introduction to the Internet.  Synchronet
    ///         is available for download from http://www.synchro.net.
    ///     </para>
    /// 
    ///     <para>
    ///         This translator implements a subset of the color and
    ///         text-related codes, as defined in version 3 of the Synchronet
    ///         documentation.  
    ///     </para>
    /// 
    /// </remarks>
    [TranslatorKey("synchronet")]
    public class SynchronetColorTranslator : Translator
    {

        private bool absorbUnsupportedCodes;


        /// <summary>
        ///     Defines the behavior of recognized but unsupported
        ///     control codes.  If unsupported codes are absorbed, they
        ///     will not be processed and will not be displayed as raw
        ///     text in the terminal.
        /// </summary>
        public bool AbsorbUnsupportedCodes
        {
            get
            {
                return this.absorbUnsupportedCodes;
            }
            set
            {
                this.absorbUnsupportedCodes = value;
            }
        }


        /// <summary>
        ///     Builds a character array containing all characters that
        ///     mark the start of a potential Synchronet control code.
        /// </summary>
        /// <returns>
        ///     A character array containing a single Control-A character.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { AsciiControlChar.ControlA };
        }


        /// <summary>
        ///     Translates a potential Synchronet BBS control code.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            if (text[index] != AsciiControlChar.ControlA)
                return index;

            if (index == text.Length - 1)
                return index;

            switch (text[index + 1])
            {

                // Foreground colors

                case 'K':

                    terminal.SetForeground(ConsoleColor.Black);
                    return index + 2;

                case 'R':
                    terminal.SetForeground(ConsoleColor.DarkRed);
                    return index + 2;

                case 'G':
                    terminal.SetForeground(ConsoleColor.DarkGreen);
                    return index + 2;

                case 'Y':
                    terminal.SetForeground(ConsoleColor.DarkYellow);
                    return index + 2;

                case 'B':
                    terminal.SetForeground(ConsoleColor.DarkBlue);
                    return index + 2;

                case 'M':
                    terminal.SetForeground(ConsoleColor.Magenta);
                    return index + 2;

                case 'C':
                    terminal.SetForeground(ConsoleColor.Cyan);
                    return index + 2;

                case 'W':
                    terminal.SetForeground(ConsoleColor.Gray);
                    return index + 2;

                // Background colors

                case '0':
                    terminal.SetBackground(ConsoleColor.Black);
                    return index + 2;

                case '1':
                    terminal.SetBackground(ConsoleColor.DarkRed);
                    return index + 2;

                case '2':
                    terminal.SetBackground(ConsoleColor.DarkGreen);
                    return index + 2;

                case '3':
                    terminal.SetBackground(ConsoleColor.DarkYellow);
                    return index + 2;

                case '4':
                    terminal.SetBackground(ConsoleColor.DarkBlue);
                    return index + 2;

                case '5':
                    terminal.SetBackground(ConsoleColor.DarkMagenta);
                    return index + 2;

                case '6':
                    terminal.SetBackground(ConsoleColor.DarkCyan);
                    return index + 2;

                case '7':
                    terminal.SetBackground(ConsoleColor.Gray);
                    return index + 2;

                // Miscellaneous codes, defined in the same
                // order as the Synchronet documention.

                case 'H':
                    terminal.SetStyle(terminal.GetStyle().CloneBolded());
                    //terminal.TextStyle.ForegroundIntensity = TextIntensity.Bold;
                    return index + 2;

                case 'I':

                    terminal.SetStyle(terminal.GetStyle().WithoutBlinking());
                    //terminal.TextStyle.Blinking = TextBlinking.None;
                    return index + 2;

                case 'N':

                    TextStyle style = terminal.GetStyle();
                    style.ForegroundIntensity = TextIntensity.Normal;
                    style.Blinking = TextBlinking.None;
                    terminal.SetStyle(style);

                    //terminal.TextStyle.ForegroundIntensity = TextIntensity.Normal;
                    //terminal.TextStyle.Blinking = TextBlinking.None;
                    return index + 2;

                case 'P':
                    terminal.PromptEnter();
                    return index + 2;

                case 'Q':

                    // This code resets the line counter for the
                    // auto-pause message.  It is not currently
                    // supported.

                    return absorbUnsupportedCodes ? index + 2 : index;

                case ',':

                    // Inserts 1/10 second delay.  Not supported yet.

                    return absorbUnsupportedCodes ? index + 2 : index;

                case ';':

                    // Inserts 1/2 second delay.  Not supported yet.

                    return absorbUnsupportedCodes ? index + 2 : index;

                case '.':

                    // Inserts a 2 second delay.  Not supported yet.

                    return absorbUnsupportedCodes ? index + 2 : index;

                case 'D':

                    // Inserts the system date.  The formatting
                    // behavior needs to be defined.

                    terminal.Write(DateTime.Now.ToShortDateString());
                    return index + 2;

                case 'T':

                    // Inserts the system time.  The formatting
                    // behavior needs to be defined.

                    terminal.Write(DateTime.Now.ToShortTimeString());
                    return index + 2;

                case 'L':
                    terminal.ClearScreen();
                    return index + 2;

                case '>':
                    terminal.ClearRowEnding();
                    return index + 2;

                case '<':
                    terminal.Write(AsciiChar.BS);
                    return index + 2;

                case '[':
                    terminal.Write(AsciiChar.CR);
                    return index + 2;

                case ']':
                    terminal.Write(AsciiChar.LF);
                    return index + 2;

                case 'A':
                    terminal.Write(AsciiControlChar.ControlA);
                    return index + 2;

                case 'S':

                    // Synchronizes output with the remote system.
                    // This is unknown.

                    return absorbUnsupportedCodes ? index + 2 : index;

                case 'Z':
                    return text.Length;

                case '-':

                    TextStyle dashStyle = terminal.GetStyle();
                    dashStyle.ForegroundIntensity = TextIntensity.Normal;
                    dashStyle.BackgroundIntensity = TextIntensity.Normal;
                    dashStyle.Blinking = TextBlinking.None;
                    terminal.SetStyle(dashStyle);

                    //terminal.TextStyle.ForegroundIntensity = TextIntensity.Normal;
                    //terminal.TextStyle.BackgroundIntensity = TextIntensity.Normal;
                    //terminal.TextStyle.Blinking = TextBlinking.None;
                    return index + 2;

                case '_':

                    // Needs investigation
                    return index + 2;


                default:
                    return index;
            }

        }

    }
}

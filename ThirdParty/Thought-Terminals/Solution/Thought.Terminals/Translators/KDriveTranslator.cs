
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     The KDriveTranslator class interprets a subset of the control
    ///     codes introduced by the circa-1990s KDrive door development kit.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <list type="table">
    ///         <item>
    ///             <term>^PB#</term>
    ///             <description>
    ///                 Changes the current background color.  The
    ///                 background color is specified as a hexadecimal
    ///                 digit.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PC##</term>
    ///             <description>
    ///                 Changes the current color.  The color is specified
    ///                 with its hexadecimal value padded to two digits.
    ///                 For example ^KC17 changes the current color to
    ///                 gray on a blue background.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PF#</term>
    ///             <description>
    ///                 Changes the current foreground color.  The foreground
    ///                 color is specified as a hexadecimal digit.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PP</term>
    ///             <description>
    ///                 Pauses the screen.  It is not known if KDrive
    ///                 displayed a prompt.  Please inform the author.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PW</term>
    ///             <description>
    ///                 Clears the screen.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PX##</term>
    ///             <description>
    ///                 Changes the horizontal column of the cursor, where
    ///                 the column is specified as a decimal number padded
    ///                 to two digits (e.g. 01 to 80).  Columns are 1-based.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^PY##</term>
    ///             <description>
    ///                 Changes the vertical row of the cursor, where
    ///                 the row is specified as a decimal number padded
    ///                 to two digits (e.g. 01 to 25).  Rows are 1-based.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>^P!</term>
    ///             <description>Not supported (see remarks).</description>
    ///         </item>
    ///         <item>
    ///             <term>^P"</term>
    ///             <description>Not supported (see remarks).</description>
    ///         </item>
    ///         <item>
    ///             <term>^P#</term>
    ///             <description>Not supported (see remarks).</description>
    ///         </item>
    ///     </list>
    /// 
    ///     <para>
    ///         KDrive supported three special codes to
    ///         control the output of text.  The ^P" code directed
    ///         output to the local (sysop) window, the ^P# code directed
    ///         output to the remote system, and the ^P! restored the
    ///         output to both windows.  These codes are not supported
    ///         by this translator.
    ///     </para>
    /// 
    ///     <para>
    ///         This translator is based on documentation provided
    ///         with versions 6.0, 6.01, 6.2 and 7.0 of the KDrive
    ///         Door Game Tool Kit (copyright 1992 SHARKware).  Other
    ///         versions have not been located for reference.  KDrive
    ///         has apparently been abandoned.
    ///     </para>
    /// 
    /// </remarks>
    [TranslatorKey("kdrive")]
    public class KDriveTranslator : Translator
    {

        /// <summary>
        ///     Creates a new KDrive control code translator.
        /// </summary>
        public KDriveTranslator():base() { }


        /// <remarks>
        ///     The KDrive translator always returns an array with
        ///     a single control-p character.
        /// </remarks>
        public override char[] GetPrefixChars()
        {
            return new char[] { AsciiControlChar.ControlP };
        }


        /// <summary>
        ///     Interprets a potential KDrive control code.
        /// </summary>
        /// <param name="terminal">
        ///     The output terminal that will be used to execute color
        ///     and cursor control codes.
        /// </param>
        /// <param name="text">
        ///     The text containing a potential KDrive control code.
        /// </param>
        /// <param name="index">
        ///     The index of the starting character of the code.
        /// </param>
        /// <returns></returns>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // Make sure the character at the specified index
            // is a control-P.  If not, exit immediately.

            if (text[index] != AsciiControlChar.ControlP)
                return index;

            // The control-P is followed by a character
            // that specifies the action to take (e.g.,
            // changing the foreground color).  Make sure
            // the string has room for such a character.

            if (index == text.Length)
                return index;

            // The P (pause), W (clear screen) and Z (clear to
            // end of the line) are all two characters in length.
            // Check for these codes first.

            switch (text[index + 1])
            {

                case 'B':

                    // The ^PB# code changes the background color.
                    // This code is always three characters in length.

                    if (HasRoom(text, index, 3))
                    {

                        byte bg;
                        if (TryParseHexChar(text[index + 2], out bg))
                        {
                            terminal.SetBackground(bg, CgaHighBit.Blink);
                            return index + 3;
                        }

                    }

                    // Either there is insufficient room for a true
                    // ^PB code, or the background parameter was not
                    // a hexadecimal character.

                    return index;

                case 'C':

                    // The ^PC## code changes the current color, where
                    // ## is the color value (foreground and background)
                    // in hexadecimal format.

                    if(HasRoom(text, index, 4))
                    {

                        if (IsHexDigit(text[index + 2]) && IsHexDigit(text[index + 3]))
                        {

                            int backColor = ParseHexChar(text[index + 2]);
                            int foreColor = ParseHexChar(text[index + 3]);

                            terminal.SetStyle(
                                (byte)((backColor << 4) | foreColor),
                                CgaHighBit.Blink);

                            return index + 4;

                        }

                    }

                    return index;

                case 'F':

                    // The ^PF# code changes the foreground color.
                    // This code is always three characters in length.

                    if (HasRoom(text, index, 3))
                    {

                        byte fg;
                        if(TryParseHexChar(text[index+2], out fg))
                        {

                            terminal.SetForeground(fg);
                            return index + 3;
                        }

                    }

                    return index;


                case 'P':

                    // The ^PP code pauses the screen.

                    terminal.PromptEnter();
                    return index + 2;

                case 'W':

                    // The ^PW code clears the screen, presumably
                    // with the current color.

                    terminal.ClearScreen();
                    return index + 2;

                case 'Z':

                    // Th ^PZ code clears the text between the cursor
                    // and the end of the line, inclusively.

                    terminal.ClearRowEnding();
                    return index + 2;

            }


            return 0;
        }

    }
}

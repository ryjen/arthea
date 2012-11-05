
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{
    /// <summary>
    ///     Interprets known color codes from the Ambroshia BBS door game.
    ///     Ambroshia is an adventure/RPG door game for 32-bit BBS systems.
    /// </summary>
    /// <remarks>
    /// 
    ///     <para>
    ///         An Ambroshia color code is simply the name of a color
    ///         surrounded by backquotes (`).  This mechanism makes it
    ///         easy for users to specify colors without understanding
    ///         hexadecimal formats and other advanced notations.
    ///     </para>
    /// 
    ///     <para>
    ///         Unfortunately the Ambroshia documentation does not list
    ///         all supported color names; the codes implemented by
    ///         this class were based on published documentation as
    ///         well as low-level analysis of the game program files.
    ///         This means that full support cannot be guaranteed.
    ///         Codes marked with a (*) are undocumented.
    ///     </para>
    /// 
    ///     <list type="table">
    /// 
    ///         <item>
    ///             <term>`bright blue`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright cyan`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright green`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright magenta`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright red`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright white`</term>
    ///         </item>
    ///         <item>
    ///             <term>`bright yellow`</term>
    ///         </item>
    ///         <item>
    ///             <term>`grey`</term>
    ///         </item>
    ///         <item>
    ///             <term>`dark green` (*)</term>
    ///         </item>
    ///         <item>
    ///             <term>`dark red` (*)</term>
    ///         </item>
    ///         <item>
    ///             <term>`yellow` (*)</term>
    ///         </item>
    /// 
    ///     </list>
    /// 
    ///     <para>
    ///         Be aware that a backquote prefix is used by the Legend of the
    ///         Red Dragon (LORD) door game for its color codes.
    ///     </para>
    /// 
    /// </remarks>
    public class AmbroshiaColorTranslator : Translator
    {

        /// <summary>
        ///     Creates a new instance of the Ambroshia color code translator.
        /// </summary>
        public AmbroshiaColorTranslator():base()
        {
        }


        /// <summary>
        ///     Returns an array containing all characters that mark the
        ///     beginning of an Ambroshia color code.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The array will contain a single backquote (`) character.
        ///         This is currently the only prefix character used for
        ///         Ambroshia color codes.  Note that a backquote is also
        ///         used for colors in the more popular Legend of the Red
        ///         Dragon (LORD) game.
        ///     </para>
        /// </remarks>
        public override char[] GetPrefixChars()
        {
            return new char[] { '`' };
        }


        /// <summary>
        ///     Translates a potential Ambroshia color code.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            if (text[index] != '`')

                // The character at the specified index is not
                // the start of an Ambrosia color code.  

                return index;

            if (index == text.Length - 1)

                // The index is the very last character in the string,
                // which means it cannot possibly be a valid code.

                return index;

            // Get the index of the second backquote.  The Ambrosia
            // color name is expected to be between these quotes.

            int terminator = text.IndexOf('`', index + 1);

            if ((terminator == -1) || (terminator == index + 1))
            {

                // The terminating character (the second backquote)
                // does not exist in the string, or is immediately
                // after the first backquote.  Regardless, this is
                // not a valid Ambrosia code.

                return index;

            }

            // Get the string between the two quotes, and convert
            // to lower case for easier processing.  This is supposed
            // to be the color name.

            string colorname = text.Substring(
              index + 1, terminator - index - 1).ToLowerInvariant();

            switch (colorname)
            {
                case "bright blue":
                    terminal.SetForeground(ConsoleColor.Blue);
                    break;
                
                case "bright cyan":
                    terminal.SetForeground(ConsoleColor.Cyan);
                    break;
                
                case "bright green":
                    terminal.SetForeground(ConsoleColor.Green);
                    break;
                
                case "bright magenta":
                    terminal.SetForeground(ConsoleColor.Magenta);
                    break;
                
                case "bright red":
                    terminal.SetForeground(ConsoleColor.Red);
                    break;
                
                case "bright white":
                    terminal.SetForeground(ConsoleColor.White);
                    break;
                
                case "dark green":
                    terminal.SetForeground(ConsoleColor.DarkGreen);
                    break;
                
                case "dark red":
                    terminal.SetForeground(ConsoleColor.DarkRed);
                    break;
                
                case "grey":
                    terminal.SetForeground(ConsoleColor.Gray);
                    break;
                
                case "yellow":
                    terminal.SetForeground(ConsoleColor.Yellow);
                    break;

                default:
                    return index;
            }

            return terminator + 1;

        }


    }
}

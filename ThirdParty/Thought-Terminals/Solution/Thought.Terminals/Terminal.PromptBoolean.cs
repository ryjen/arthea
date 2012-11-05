
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

/* =========================================================================
 * PromptBoolean
 * -------------------------------------------------------------------------
 * Functions and structures for boolean prompts.  A boolean prompt asks
 * the user to enter a yes/no or true/false answer.
 * ==========================================--============================= */

using System;

namespace Thought.Terminals
{
    public partial class Terminal
    {

        /// <summary>
        ///     The settings for a boolean prompt.
        /// </summary>
        public class BooleanPromptSettings : MenuPromptSettings 
        {

            /// <summary>
            ///     The character that means false.
            /// </summary>
            public char FalseCharacter;

            /// <summary>
            ///     The style of the false character.
            /// </summary>
            public TextStyle FalseStyle;

            /// <summary>
            ///     The word that means false.
            /// </summary>
            public string FalseWord;

            /// <summary>
            ///     The character that means True.
            /// </summary>
            public char TrueCharacter;

            /// <summary>
            ///     The style of the true character.
            /// </summary>
            public TextStyle TrueStyle;

            /// <summary>
            ///     The word that means True.
            /// </summary>
            public string TrueWord;

        }


        /// <summary>
        ///     Prompts the user for a true/false value.
        /// </summary>
        /// <param name="text">
        ///     The optional text prompt to show to the user.
        /// </param>
        /// <param name="defaultValue">
        ///     The default boolean result if the user presses ENTER
        ///     instead of pressing one of the boolean characters.
        /// </param>
        /// <param name="settings">
        ///     A <see cref="BooleanPromptSettings"/> object that
        ///     contains advanced options.
        /// </param>
        /// <returns>
        ///     A boolean value indicating the selection of the user.
        /// </returns>
        public bool PromptBoolean(
            string text,
            bool defaultValue,
            BooleanPromptSettings settings)
        {

            if (settings == null)
                throw new ArgumentNullException("settings");

            char entry = PromptMenu(
                text,
                new char[] { settings.TrueCharacter, settings.FalseCharacter },
                defaultValue ? settings.TrueCharacter : settings.FalseCharacter,
                new string[] { settings.TrueWord, settings.FalseWord },
                settings);

            bool returnValue;

            if ((entry == char.ToLower(settings.FalseCharacter)) || (entry == char.ToUpper(settings.FalseCharacter)))
                returnValue = false;

            else if ((entry == char.ToLower(settings.TrueCharacter)) || (entry == char.ToUpper(settings.TrueCharacter)))
                returnValue = true;

            else
                returnValue = false;

            // Return the value to the caller.

            return returnValue;

        }


        /// <summary>
        ///     Prompts the user to enter a yes/no or
        ///     true/false value.
        /// </summary>
        /// <param name="text">
        ///     Optional text (such as "Do you wish to continue?").
        /// </param>
        /// <param name="defaultValue">
        ///     The default value is the enter key is pressed.
        /// </param>
        /// <returns>
        ///     True or false, depending on the answer selected
        ///     by the user.
        /// </returns>
        /// <remarks>
        ///
        ///     <para>
        ///         All of the terminal prompt functions provide
        ///         basic input methods that are common to many
        ///         applications.  You can use them to quickly
        ///         obtain functionality that would otherwise
        ///         have to write (such as asking for a yes/no
        ///         answer).
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="FalseCharacter"/>
        /// <seealso cref="TrueCharacter"/>
        public bool PromptBoolean(
          string text,
          bool defaultValue)
        {

            BooleanPromptSettings settings = new BooleanPromptSettings();

            settings.LeftBracket = '[';
            settings.FalseCharacter = this.falseCharacter;
            settings.RightBracket = ']';
            settings.Separator = '/';
            settings.TrueCharacter = this.trueCharacter;
            settings.WriteMenu = true;
            settings.WriteNewLine = true;

            return PromptBoolean(text, defaultValue, settings);

        }


    }
}

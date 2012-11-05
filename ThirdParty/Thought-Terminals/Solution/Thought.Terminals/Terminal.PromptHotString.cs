
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections.Generic;
using System.Text;

namespace Thought.Terminals
{
    public partial class Terminal
    {

        /// <summary>
        ///     The settings for a hotstring prompt.
        /// </summary>
        public class HotStringPromptSettings : MenuPromptSettings 
        {
        }

        /// <summary>
        ///     Displays a prompt and waits for the user
        ///     to press one of the hotkeys defined within
        ///     the prompt.
        /// </summary>
        /// <param name="prompt">
        ///     The prompt containing embedded hotkeys using
        ///     ampersand notation.
        /// </param>
        /// <returns>
        ///     The character pressed by the user.
        /// </returns>
        /// <remarks>
        ///     
        ///     <para>
        ///         Hotkeys are specified using the same notation
        ///         as Windows menu hotkeys -- an ampersand followed
        ///         by the desired character.
        ///     </para>
        /// 
        /// </remarks>
        public char PromptHotString(string prompt)
        {

            HotStringPromptSettings settings = new HotStringPromptSettings();
            // The text color is set to the current color.
            // This is obtained by calling the Color property.

            settings.PromptTextStyle = this.GetStyle();

            // The hotchar color is the bolded version of the
            // current color (if possible).

            settings.HotKeyStyle = settings.PromptTextStyle.CloneBolded();

            settings.WriteNewLine = true;

            // Call the master PromptHotString function with
            // these calculated values.  Other parameters are
            // set to default values.

            return PromptHotString(prompt, settings);

        }


        /// <summary>
        ///     Displays a prompt to the user, then waits for
        ///     any defined hotkeys to be pressed.
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         This version of the method implements all of the
        ///         functionality of the prompt.  Other overloaded versions
        ///         are easier to use because they set default values
        ///         when calling this version.
        ///     </para>
        ///
        ///     <para>
        ///         You may use a double-ampersand to force a single
        ///         ampersand in the output prompt.  Due to technical
        ///         limitations, ampersands are not allowed as hotchars.
        ///         This is because there is no reliable way of determing
        ///         the hotchars in some cases.
        ///     </para>
        /// 
        ///     <para>
        ///         For exmple, &amp;&amp;&amp;X could be as a prompt
        ///         containing an ampersand and an X as valid hotchars, or
        ///         a prompt containing a written ampersand and an X as
        ///         a hotchar.  If you really need an ampersand as a menu
        ///         option, you'll need to implement your prompt code
        ///         manually.
        ///     </para>
        /// 
        ///     <para>
        ///         Control codes between ampersands are translated; that
        ///         is, a prompt like "@CLS@Hello, &amp;World" will clear
        ///         the screen if the @CLS@ code is implemented by a 
        ///         translator.  However, control codes crossing over a hotkey
        ///         will not be translated.  A prompt like @CL&amp;S@ will 
        ///         not cause the screen to clear.  This behavior may change
        ///         in a future version of the software.
        ///     </para>
        /// 
        ///     <para>
        ///         Additionally, translators using an ampersand as a prefix
        ///         character may cause undefined behavior with this function.
        ///         If you have such a translator, disable it before calling
        ///         this or any other PromptHotString method.
        ///     </para>
        /// 
        /// </remarks>
        public char PromptHotString(
            string prompt,
            HotStringPromptSettings settings)
        {

            // The prompt is a required argument because the input
            // loop is based on the contents of the prompt!  Raise
            // an exception if a null or blank string was passed
            // to the function.

            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentNullException("prompt");

            // The next step is to step through the raw prompt text
            // to extract the allowed characters while also displaying
            // the prompt.  Control sequences are allowed.

            // When examining each character, there are two 
            // possible states: (a) expecting a hotchar, and (b)
            // not expecting anything.  The scanner is expecting
            // a hotchar after each ampersand (&).  Natually, at
            // startup the scanner is not expecting a hotchar.

            bool expectingHotChar = false;

            // A buffer is needed to hold characters that are part
            // of the prompt (or control codes) and not part of
            // a hotchar sequence.

            StringBuilder pending = new StringBuilder();

            // A buffer is also needed to hold characters that
            // are allowed to be pressed (i.e., the hotchars).

            StringBuilder validChars = new StringBuilder();

            // Save the current text style before processing
            // the string.  Any changes due to control codes
            // or the prompt settings will be reverted
            // after writing the prompt.

            if (settings.PromptTextStyle == null)
            {
                PushStyle();
            }
            else
            {
                PushStyle(settings.PromptTextStyle);
            }

            for (int index = 0; index < prompt.Length; index++)
            {

                char c = prompt[index];

                if (expectingHotChar)
                {
                    // The previous character was an ampersand, which
                    // means that this character will be one of the
                    // menu items that can be pressed by the user.
                    // However, if the character is another ampersand, then
                    // the programmer wants an literal ampersand in the
                    // prompt.

                    if (c == '&')

                        pending.Append('&');

                    else
                    {

                        // The parser is expecting a hotchar, and in fact
                        // this character is a hotchar.  The hotchar will
                        // be written in a moment, so flush out any 
                        // pending characters.

                        Translate(pending.ToString());
                        pending.Length = 0;

                        // Change to the hotchar color (specified by
                        // the caller), write the character, and restore
                        // the original color.

                        if (settings.HotKeyStyle != null)
                            PushStyle(settings.HotKeyStyle);
                        
                        Write(c);

                        if(settings.HotKeyStyle != null)
                            PopStyle();

                        // Save the hotchar in the menu list 
                        // and revert back to the original parser state.

                        validChars.Append(c);
                        expectingHotChar = false;

                    }
                }
                else
                {

                    // The parser is not expecting a hotchar.

                    if (index == prompt.Length - 1)

                        // This is the very last character in the
                        // string.  It doesn't matter if this is an
                        // ampersand because no hotchar can be specified.
                        // A hotchar sequence is always two characters.

                        pending.Append(c);

                    else if (c == '&')

                        // This is an ampersand, which marks the 
                        // beginning of a ampersand/hotchar sequence.
                        // The next character is expected to be the
                        // hotchar, although it could also be another
                        // ampersand.

                        expectingHotChar = true;

                    else
                    {
                        // The parser was not expecting a hotchar,
                        // and this character is not an ampersand, so
                        // it can be placed in the buffer for eventual
                        // output.

                        pending.Append(c);

                    }

                }

            }

            // Every character in the prompt has been processed.
            // Next see if any pending characters need to be flushed
            // to the output.  Once flushed, restore the original
            // text style that was active when the prompt was started.

            if (pending.Length > 0)
                Translate(pending.ToString());

            PopStyle();

            // At this point, the specified prompt has been 
            // parsed and displayed.  First, see if any hotchars
            // were actually found.

            if (validChars.Length == 0)
                throw new ArgumentException(
                  "No hotchars were found.", "prompt");

            return PromptMenu(
                null,
                validChars.ToString().ToCharArray(),
                '\x0',
                null,
                settings);

        }

    }
}

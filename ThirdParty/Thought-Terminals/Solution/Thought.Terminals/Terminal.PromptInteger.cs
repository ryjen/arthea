
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Text;

/* =========================================================================
 * Terminal.PromptInteger
 * -------------------------------------------------------------------------
 * Implements a prompt for getting an integer from the user.
 * ========================================================================= */

namespace Thought.Terminals
{

	public partial class Terminal
	{


        /// <summary>
        ///     Prompts to enter an integer of any range.
        /// </summary>
        /// <returns>
        ///     The integer entered by the user, or null if the user
        ///     pressed the escape key.
        /// </returns>
        public int? PromptInteger()
        {
            return PromptInteger(
                null,   // initialValue
                true,   // allowBlank
                true,   // allowEscape
                null,   // minimum
                null);  // maximum

        }


        /// <summary>
        ///     Prompts the user to enter an integer.
        /// </summary>
        /// <param name="initialValue">
        ///     The initial value shown to the user.
        /// </param>
        /// <returns>
        ///     The value selected by the user, or null if the user
        ///     pressed the escape key.
        /// </returns>
        public int? PromptInteger(int? initialValue)
        {
            return PromptInteger(
                initialValue,
                true,          // allowBlank
                true,          // allowEscape
                null,          // minimum
                null);         // maximum

        }


        /// <summary>
        ///     Prompts the user to enter an integer.
        /// </summary>
        /// <param name="initialValue">
        ///     The initial value shown to the user.
        /// </param>
        /// <param name="allowBlank">
        ///     True if a blank entry is accepted.
        /// </param>
        /// <param name="allowEscape">
        ///     True if the user can press escape to exit the prompt.
        /// </param>
        /// <returns>
        ///     The integer selected by the user, or null if the user
        ///     pressed the escape key (and escaping is allowed).
        /// </returns>
        public int? PromptInteger(int? initialValue, bool allowBlank, bool allowEscape)
        {
            return PromptInteger(
                initialValue,
                allowBlank,
                allowEscape,
                null,         // minimum
                null);        // maximum
        }



        /// <summary>
        ///     Prompts the user to enter an integer.
        /// </summary>
        /// <param name="allowBlank">
        ///     True if a blank entry is allowed.
        /// </param>
        /// <param name="allowEscape">
        ///     True if the user is allowed to exit the prompt by pressing the escape key.
        /// </param>
        /// <returns>
        ///     The integer entered by the user, or null if the user hit the
        ///     escape key (and escaping is allowed).
        /// </returns>
        public int? PromptInteger(bool allowBlank, bool allowEscape)
        {
            return PromptInteger(
                null,
                allowBlank,
                allowEscape,
                null,
                null);
        }


        /// <summary>
        ///    Prompts the user to enter an integer within a specified range.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is the master PromptInteger function.  All other variations
        ///         call this version with appropriate parameters.
        ///     </para>
        /// </remarks>
        public int? PromptInteger(
            int? initialValue,
            bool allowBlank,
            bool allowEscape,
            int? minimum,
            int? maximum)
        {

            // The temp variable is used by the function when
            // attempting to parse the input entered by the user.

            int temp;

            // A StringBuilder is used to hold the valid characters
            // typed by the user.  Only valid integer characters 
            // are placed into the buffer (invalid characters are
            // ignored when recieved).

            StringBuilder builder = new StringBuilder();

            // If an initial value was specified, add it to the
            // buffer and write the text value to the screen.

            if (initialValue != null)
            {
                builder.Append(initialValue.ToString());
                Write(initialValue.ToString());
            }

            do
            {

                char entry = ReadChar();

                switch (entry)
                {

                    case '-':

                        // -----------------------------------------------
                        // Minus Character
                        // -----------------------------------------------
                        //
                        // The minus (-) character is valid only when it
                        // is the first character of a numeric entry.
                        // Otherwise it is ignored.

                        if (builder.Length == 0)
                        {
                            builder.Append(entry);
                            Write(entry);
                        }
                        else
                        {
                            Beep();
                        }

                        break;


                    case AsciiChar.BS:

                        // -----------------------------------------------
                        // Backspace Character
                        // -----------------------------------------------

                        if (builder.Length > 0)
                        {
                            // The user pressed the backspace key.
                            // Erase the last digit, if one has been
                            // written.

                            Write(AsciiChar.BS);
                            Write(AsciiChar.Space);
                            Write(AsciiChar.BS);

                            builder.Remove(builder.Length - 1, 1);
                        }
                        else
                        {
                            Beep();
                        }
                        break;


                    case AsciiChar.ESC:

                        // -----------------------------------------------
                        // Escape Character
                        // -----------------------------------------------

                        if (allowEscape)
                        {
                            return null;
                        }
                        else
                        {
                            Beep();
                        }
                        break;


                    case AsciiChar.CR:

                        // -----------------------------------------------
                        // Carriage Return (CR)
                        // -----------------------------------------------

                        if (int.TryParse(builder.ToString(), out temp))
                        {

                            if ((minimum == null) || (temp >= minimum.Value))
                            {

                                if ((maximum == null) || (temp <= maximum.Value))
                                {
                                    return temp;
                                }

                            }
                        }
                        Beep();
                        break;


                    case ']':
                    case '>':

                        // -----------------------------------------------
                        // Increment Characters
                        // -----------------------------------------------

                        if (builder.Length > 0)
                        {

                            if (int.TryParse(builder.ToString(), out temp))
                            {

                                // Clear the current prompt

                                Write(AsciiChar.BS, builder.Length);
                                Write(AsciiChar.Space, builder.Length);
                                Write(AsciiChar.BS, builder.Length);

                                temp++;
                                builder.Length = 0;
                                builder.Append(temp);
                                Write(builder.ToString());
                            }
                            break;

                        }
                        break;

                    case '[':
                    case '<':

                        // -----------------------------------------------
                        // Decrement Characters
                        // -----------------------------------------------

                        if (builder.Length > 0)
                        {
                            if (int.TryParse(builder.ToString(), out temp))
                            {
                                // Clear the current prompt

                                Write(AsciiChar.BS, builder.Length);
                                Write(AsciiChar.Space, builder.Length);
                                Write(AsciiChar.BS, builder.Length);

                                temp--;
                                builder.Length = 0;
                                builder.Append(temp);
                                Write(builder.ToString());

                            }

                        }
                        break;

                    default:

                        if (char.IsDigit(entry))
                        {

                            // This is a digit character.  Append
                            // the digit to the string if the result is still
                            // considered a valid integer.

                            builder.Append(entry);

                            if (int.TryParse(builder.ToString(), out temp))
                            {
                                Write(entry);
                            }
                            else
                            {
                                builder.Length--;
                            }
                        }
                        break;


                }


            } while (true);

        }

	}

}

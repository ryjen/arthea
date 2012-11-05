
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

    /// <summary>
    ///     Base class for all control code translators.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         A control code is an embedded character sequence that instructs
    ///         the terminal engine to perform certain actions.  For example, the
    ///         WildCat! @CLS@ code causes the screen to be cleared.
    ///     </para>
    /// 
    ///     <para>
    ///         A translator is a class that implements support for
    ///         a particular control code format.  The terminal engine provides
    ///         a flexible framework that allows you to create your own formats
    ///         or implement support for an existing format.  For example,
    ///         the <see cref="Translators.AnsiTranslator"/>
    ///         class provides support for ANSI-BBS escape sequences.
    ///         The <see cref="Translators.AvatarTranslator"/>
    ///         class provides support for basic AVATAR codes.
    ///     </para>
    /// 
    ///     <para>
    ///         All translators derive from this base class.  A derived class
    ///         must override the <see cref="GetPrefixChars"/> and
    ///         <see cref="Translate"/> methods.
    ///     </para>
    /// 
    /// </remarks>

    public abstract class Translator
    {


        /// <summary>
        ///     Combines two translators into a translator collection.
        /// </summary>
        /// <param name="t1">The 1st translator.</param>
        /// <param name="t2">The 2nd translator</param>
        /// <returns>
        ///     A <see cref="TranslatorCollection"/> containing both translators.
        /// </returns>
        public static TranslatorCollection operator +(
            Translator t1,
            Translator t2)
        {

            TranslatorCollection collection = new TranslatorCollection();
            collection.Add(t1);
            collection.Add(t2);
            return collection;

        }

        /// <summary>
        ///     Builds an array of characters that mark the beginning of any
        ///     potential control code supported by the translator.
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         The inherited class must override this method and return
        ///         a character array containing every possible character
        ///         that marks the beginning of a potential control code.
        ///         For example, all ANSI-BBS control codes are prefixed with
        ///         an escape character (ASCII #27).  The <see cref="Translators.AnsiTranslator"/>
        ///         class returns an array containing a single escape character.
        ///     </para>
        /// 
        ///     <para>
        ///         The terminal engine will use this information to identify the
        ///         translator when processing an output character.  Multiple
        ///         translators can be mapped to the same character; in this case,
        ///         the first (earliest) registered translator is given the first 
        ///         chance to process the output string.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="Translate"/>
        public abstract char[] GetPrefixChars();


        /// <summary>
        ///     Determines whether or not there is sufficient room
        ///     for a control code in a string.
        /// </summary>
        /// <param name="text">
        ///     A string containing a control code.
        /// </param>
        /// <param name="index">
        ///     The index of the potential code.
        /// </param>
        /// <param name="minimumLength">
        ///     The minimum length of the string.
        /// </param>
        /// <returns>
        ///     True if a control code of the specified length has room to
        ///     fit within the string at the specified index.  False if
        ///     the string is null, or the index is out of range, or
        ///     a control code of the designated length could not fit
        ///     in the string.
        /// </returns>
        protected bool HasRoom(string text, int index, int minimumLength)
        {

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (minimumLength < 1)
                throw new ArgumentOutOfRangeException("minimumLength");

            if (string.IsNullOrEmpty(text))
                return false;

            if (index >= text.Length)
                return false;

            // Example: suppose the text has a length of 5 and the
            // minimum length is 3 characters.  The control code
            // can be at positions 0, 1 or 2.  The latest position
            // is text.Length-minimumLength.

            if (index > text.Length - minimumLength)
                return false;

            return true;

        }


        /// <summary>
        ///     Indicates whether the specified character is
        ///     a hexidecimal digit.
        /// </summary>
        /// 
        /// <param name="value">
        ///     A unicode character
        /// </param>
        /// 
        /// <remarks>
        /// 
        ///     <para>
        ///         This utility function is provided by the
        ///         base translator class because many control
        ///         code formats make use of hexidecimal numbers
        ///         to specify colors.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="ParseHexChar"/>
        protected static bool IsHexDigit(char value)
        {

            // First, see if the character is
            // a decimal digit.  All decimal digits
            // are also hexadecimal digits.

            if (char.IsDigit(value))

                return true;

            // Next, see if the digit ranges from A through F,
            // case-insensitive.  A=10, B=11, C=12, D=13, E=14, F=15.

            if (
              ((value >= 'A') && (value <= 'F')) ||
              ((value >= 'a') && (value <= 'f')))
            {
                return true;
            }

            // Nope -- this is not a hex digit.

            return false;
        }


        /// <summary>
        ///     Called when the prefix character changes.
        /// </summary>
        /// <remarks>
        ///     An inheriting class must call this method when changing the
        ///     prefix character.  The translator will raise an event to notify
        ///     other classes of the change.
        /// </remarks>
        protected void OnPrefixChanged()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Converts a hexadecimal character to its
        ///     integer value.
        /// </summary>
        /// <param name="value">
        ///     A Unicode character.
        /// </param>
        /// <remarks>
        /// 
        ///     <para>
        ///         This is a utility method for the convenience
        ///         of translator developers.  Many control code
        ///         protocols make use of hexadecimal numbers to
        ///         specify colors and other attributes.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="IsHexDigit"/>
        protected int ParseHexChar(char value)
        {

            // First, see if the character is a digit.  This is
            // tested first because statistically most hexadecimal
            // characters are decimal digits.

            if (char.IsDigit(value))
            {
                return Convert.ToInt32(char.GetNumericValue(value));
            }

            // A = ASCII 65
            // F = ASCII 70
            // a = ASCII 97
            // f = ASCII 102

            else if ((value >= 'A') && (value <= 'F'))
            {

                // The character is one of the characters
                // between 'A' (value 65) and 'F' (value 70).
                // The character "A" (hex) is "10" (decimal).

                return Convert.ToInt32(value) - 55;
            }

            else if ((value >= 'a') && (value <= 'f'))
            {

                // The character is one of the characters
                // between 'a' (value 97) and 'f' (value 102).
                // The character "A" or "a" (hex) is "10" (decimal).

                return Convert.ToInt32(value) - 87;

            }
            else

                // The specified character cannot be interpreted
                // as a written hexadecimal character.  Raise an
                // exception.

                throw new ArgumentOutOfRangeException();

        }


        /// <summary>
        ///     Translates an embedded control code in a string.
        /// </summary>
        /// 
        /// <param name="terminal">
        ///     The output terminal.  The control code should
        ///     manipulate this terminal only.
        /// </param>
        /// 
        /// <param name="text">
        ///     A string containing a control code.
        /// </param>
        /// 
        /// <param name="index">
        ///     The zero-based index of the start of the control code.
        /// </param>
        /// 
        /// <returns>
        ///     The next index to process.  The same index will be returned
        ///     if the code is invalid.
        /// </returns>
        /// 
        /// <remarks>
        /// 
        ///     <para>
        ///         This function will be called by the terminal engine
        ///         when it encounters any of the characters specified
        ///         by the <see cref="GetPrefixChars"/> function.  This
        ///         means the control code may not be syntactically correct;
        ///         it is up to the translator to determine the validity
        ///         of the code.
        ///     </para>
        /// 
        ///     <para>
        ///         The function is expected to return the zero-based index
        ///         where processing should continue upon return from the
        ///         function.  This is normally the index of the first character
        ///         after the control code; however, if the control code is
        ///         unrecognized (or syntactically incorrect), the function
        ///         should either return the same starting index passed to
        ///         the function, or zero.
        ///     </para>
        /// 
        ///     <para>
        ///         Upon detecting such a value, the terminal engine will
        ///         either pass the string to the next translator registered
        ///         against the prefix character, or will send it to the
        ///         terminal unchanged.
        ///     </para>
        /// 
        ///     <para>
        ///         The function should not raise an exception unless
        ///         the parameters are clearly illegal (e.g. a null
        ///         terminal reference).  Unrecognized or syntactically-incorrect
        ///         codes should always be handled by returning the original
        ///         starting index or zero.  This is much faster than
        ///         generating an exception.  Additionally, an exception is
        ///         misleading because the code might be valid from the
        ///         perspective of another translator.  Use the
        ///         <see cref="ValidateParameters"/> method to perform the
        ///         most common parameter checks at the beginning of the
        ///         method.
        ///     </para>
        /// 
        ///     <para>
        ///         Do not attempt to optimize processing by executing
        ///         multiple codes that are adjacent to each other.  Doing
        ///         so violates the priority of each translator as assigned
        ///         by the sysop or developer.  If you must implement such
        ///         an optimization, be sure to make it an optional feature.
        ///     </para>
        /// 
        /// </remarks>
        /// <seealso cref="GetPrefixChars"/>
        /// <seealso cref="ValidateParameters"/>
        public abstract int Translate(
            Terminal terminal,
            string text,
            int index);


        /// <summary>
        ///     Attempts to parse a hexadecimal character.
        /// </summary>
        protected bool TryParseHexChar(char hexchar, out byte result)
        {

            if (IsHexDigit(hexchar))
            {
                result = Convert.ToByte(ParseHexChar(hexchar));
                return true;
            }
            else
            {
                result = 0;
                return false;
            }

        }


        /// <summary>
        ///     Performs common validation of terminal parameters.
        /// </summary>
        /// <param name="terminal">
        ///     The terminal that will executed the control code.
        /// </param>
        /// <param name="text">
        ///     A string containing a potential code.
        /// </param>
        /// <param name="index">
        ///     The index of the potential code in the string.
        /// </param>
        /// <remarks>
        ///     An exception is raised if the terminal reference is null,
        ///     the text reference is null, or the index is outside
        ///     of the valid range for the text.  The method should be
        ///     called at the start of the <see cref="Translate"/> method.
        /// </remarks>
        /// <seealso cref="Translate"/>
        protected void ValidateParameters(Terminal terminal, string text, int index)
        {

            // Check the terminal reference.

            if (terminal == null)
                throw new ArgumentNullException("terminal");

            // Check the string.  Empty or null strings are not
            // valid (and should never have reached this point).

            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            // Make sure the index is within the characters
            // of a string.

            if ((index < 0) || (index >= text.Length))
                throw new ArgumentOutOfRangeException("index");

        }

    }
}

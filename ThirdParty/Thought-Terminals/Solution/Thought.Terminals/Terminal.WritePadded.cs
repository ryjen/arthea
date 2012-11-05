
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

/* =========================================================================
 * WritePadded
 * -------------------------------------------------------------------------
 * The WritePadded functions allow the user to write raw text to a certain
 * length.  The text can be clipped (shortened) or lengthened with a 
 * padding character.  The justification of the text can also be set.
 * ========================================================================= */

namespace Thought.Terminals
{
    public partial class Terminal
    {

        /// <summary>
        ///     Pads or truncates a string and then writes it to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The unformatted string to write.
        /// </param>
        /// <param name="length">
        ///     The desired length of the output.
        /// </param>
        /// <remarks>
        ///     The output is left-justified and padded with a space character
        ///     if the desired length is longer than the text.  The style of
        ///     the output is not changed.
        /// </remarks>
        public void WritePadded(string value, int length)
        {

            WritePadded(
                value,
                length,
                JustifyText.Left,
                AsciiChar.Space,  // The default padding character is a space
                null);            // Do not change the padding color

        }


        /// <summary>
        ///     Pads or truncates a string and then writes it to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The unformatted string to write.
        /// </param>
        /// <param name="length">
        ///     The desired length of the output.
        /// </param>
        /// <param name="paddingChar">
        ///     The character to use as padding when the desired length is
        ///     longer than the supplied text.
        /// </param>
        /// <remarks>
        ///     The output string is left-justified without any style (e.g. color) changes.
        /// </remarks>
        public void WritePadded(string value, int length, char paddingChar)
        {

            WritePadded(
                value,
                length,
                JustifyText.Left,
                paddingChar,
                null);
        }

        /// <summary>
        ///     Pads or truncates a string and then writes it to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The unformatted string to write.
        /// </param>
        /// <param name="length">
        ///     The desired length of the string.
        /// </param>
        /// <param name="justifyText">
        ///     The justification of the text after padding.
        /// </param>
        public void WritePadded(
            string value,
            int length,
            JustifyText justifyText)
        {
            WritePadded(
                value,
                length,
                justifyText,
                AsciiChar.Space,  // The default padding character is a space
                null);            // Do not change the padding color
        }


        /// <summary>
        ///     Pads or truncates a string and then writes it to the terminal.
        /// </summary>
        /// <param name="value">
        ///     The unformatted string to write.
        /// </param>
        /// <param name="length">
        ///     The desired length of the string.
        /// </param>
        /// <param name="justifyText">
        ///     The justification of the text after padding.
        /// </param>
        /// <param name="paddingChar">
        ///     The character to use for padding.
        /// </param>
        public void WritePadded(
            string value,
            int length,
            JustifyText justifyText,
            char paddingChar)
        {

            WritePadded(
                value,
                length,
                justifyText,
                paddingChar,
                null);
        }


        /// <summary>
        ///     Writes a string padded or truncated to the
        ///     specified length.
        /// </summary>
        /// <param name="value">
        ///     The unformatted string to write.
        /// </param>
        /// <param name="length">
        ///     The desired length of the string.
        /// </param>
        /// <param name="justifyText">
        ///     The justification of the text after padding.
        /// </param>
        /// <param name="paddingChar">
        ///     The character to use for padding.
        /// </param>
        /// <param name="paddingStyle">
        ///     The style to use when writing padding characters.
        /// </param>
        public void WritePadded(
            string value,
            int length,
            JustifyText justifyText,
            char paddingChar,
            TextStyle paddingStyle)
        {

            // A negative length is not an error.  Some developers might assume
            // that a negative length switches the justification (which is found
            // in some classic libraries), but this is not supported due to 
            // possible confusion.  Instead the developer should set the JustifyText
            // argument.

            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            // A zero length is legal.  This means that no further processing
            // is necessary (simply exit immediately).  It is possible that
            // a zero length will result in an argument range exception in
            // a future version of the library.  Further real-world experience
            // is needed.
            
            if (length == 0)
                return;

            // If the desired length is the same as the specified text, then
            // no further processing is necessary.  The strange expression
            // below ensures that null values are handled safely by the software.

            if (length == (value == null ? 0 : value.Length))
            {
                if (value != null)
                    Write(value);
                return;
            }

            // At this point in the code, the desired length is 
            // different than the length of the string.  This means
            // that formatting must occur somewhere.  The output
            // has three possible sections:
            //
            //   Any padding characters before the value.
            //   The value clipped as necessary.
            //   Any padding characters after the value.
            //
            // These three components are calculated before output.
            // The algorithm depends on the selected justification.

            string prePadding;
            string formatted;
            string postPadding;

            switch (justifyText)
            {

                case JustifyText.Left:
                    
                    // A string like "ABC" is formatted like "ABC   " when
                    // padding is necessary, or "AB" when clipping is
                    // necessary.

                    prePadding = string.Empty;

                    if ((value == null) || (length > value.Length))
                    {
                        // The desired length is greater than the current
                        // length of the string.  Therefore the string
                        // needs to be written, and then followed with the
                        // padding characters.

                        formatted =
                            (value == null) ? string.Empty : value;
                        
                        postPadding =
                            new string(paddingChar, length - formatted.Length);

                    }
                    else if (length < value.Length)
                    {
                        formatted = value.Substring(0, length);
                        postPadding = string.Empty;
                    }
                    else
                    {
                        formatted = value;
                        postPadding = string.Empty;
                    }

                    break;

                case JustifyText.Right:

                    // RIGHT JUSTIFY: The last character of the formatted output
                    // is the last character of the value.  The start of the 
                    // string contains either padding or the start of the string
                    // or the start of non-truncated characters.

                    postPadding = string.Empty;

                    if ((value == null) || (length > value.Length))
                    {

                        // The desired length is greater than the current
                        // length of the string.  Therefore the padding
                        // characters need to be written, and then followed
                        // with the text.

                        formatted = (value == null) ? string.Empty : value;
                        prePadding = new string(paddingChar, length - formatted.Length);

                    }
                    else if (length < value.Length)
                    {

                        // The desired length is shorter than the current
                        // length of the string.  No padding characters need
                        // to be written -- only the ending of the string.

                        formatted = value.Substring(value.Length - length, length);
                        prePadding = string.Empty;
                    }
                    else
                    {
                        formatted = value;
                        prePadding = string.Empty;
                    }
                    break;

                case JustifyText.Center:

                    if ((value == null) || (length > value.Length))
                    {

                        // The value is shorter than the desired length,
                        // which means the value needs to be centered
                        // within the output.

                        formatted = (value == null) ? string.Empty : value;

                        // Divide the remaining characters by two.

                        prePadding =
                            new string(paddingChar, (length - formatted.Length) / 2);
                        
                        postPadding =
                            new string(paddingChar,
                                ((length - formatted.Length) / 2) + ((length - formatted.Length) % 2));

                    }
                    else
                    {

                        // The desired length is shorter than the provided
                        // text.  This means no padding characters need to 
                        // be written, but characters from one or both sides
                        // must be clipped.

                        prePadding = string.Empty;
                        postPadding = string.Empty;

                        formatted = value.Substring(
                            (value.Length - length) / 2,
                            length);

                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            // The padding strings and truncated values have been calculated.

            if(!string.IsNullOrEmpty(prePadding))
            {

                if (paddingStyle != null)
                    PushStyle(paddingStyle);

                Write(prePadding);

                if (paddingStyle != null)
                    PopStyle();

            }

            if(!string.IsNullOrEmpty(formatted))
                Write(formatted);

            if(!string.IsNullOrEmpty(postPadding))
            {
                if (paddingStyle != null)
                    PushStyle(paddingStyle);

                Write(postPadding);

                if (paddingStyle != null)
                    PopStyle();

            }

        }

    }
}

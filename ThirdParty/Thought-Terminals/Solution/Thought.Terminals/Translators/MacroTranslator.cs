
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Implements functionality common to all basic macro
    ///     translators.  A macro has the format {MacroName} or
    ///     {MacroName:Format}.
    /// </summary>
    public abstract class MacroTranslator : Translator
    {

        /// <summary>
        ///     The default character to use for padding.
        /// </summary>
        public const char DefaultPadding = ' ';


        /// <summary>
        ///     The default prefix character that marks the start of a macro.
        /// </summary>
        public const char DefaultPrefix = '{';


        /// <summary>
        ///     The default suffix character that marks the end of a macro.
        /// </summary>
        private const char DefaultSuffix = '}';


        private char padding;
        private TextStyle paddingStyle;
        private char prefix;
        private char suffix;


        /// <summary>
        ///     Initializes the macro translator with default values.
        /// </summary>
        protected MacroTranslator()
        {
            this.padding = DefaultPadding;
            this.prefix = DefaultPrefix;
            this.suffix = DefaultSuffix;
        }


        /// <summary>
        ///     Initializes the macro translator with the specified
        ///     character that will act as the prefix and suffix
        ///     characters.
        /// </summary>
        /// <param name="prefixAndSuffix">
        ///     The prefix and suffix character for a macro string.
        /// </param>
        protected MacroTranslator(char prefixAndSuffix)
        {
            this.padding = DefaultPadding;
            this.prefix = prefixAndSuffix;
            this.suffix = prefixAndSuffix;
        }


        /// <summary>
        ///     Initializes the macro translator with the specified properties.
        /// </summary>
        /// <param name="prefix">
        ///     The prefix character that marks the beginning of a macro string.
        /// </param>
        /// <param name="suffix">
        ///     The suffix character that marks the end of the macro string.
        /// </param>
        protected MacroTranslator(char prefix, char suffix)
        {
            this.padding = DefaultPadding;
            this.prefix = prefix;
            this.suffix = suffix;
        }


        /// <summary>
        ///     Returns the whole (unformatted) value of the
        ///     specified macro name.  This is implemented by derived
        ///     classes.
        /// </summary>
        /// <param name="name">
        ///     The name of a macro to be examined by the inherited class.
        /// </param>
        /// <returns>
        ///     The whole (unformatted) value of the macro, or null
        ///     if the name is unrecognized.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         The implementing class should not raise an
        ///         exception if the name is unrecognied.
        ///         Multiple macro translators may be active at the
        ///         same time; instead the function should return null
        ///         for unrecognized macros.  Recognized (but blank)
        ///         macros should return an empty string, even if the
        ///         underlying value of the macro is null.  For example,
        ///         if a date macro is null, the function should return
        ///         an empty string.  A null value will cause the macro
        ///         translator to believe the name was unrecognized.
        ///     </para>
        /// </remarks>
        protected abstract object Expand(string name);


        /// <summary>
        ///     Builds a character array containing the prefix (starting character)
        ///     of the macro translator.
        /// </summary>
        /// <returns>
        ///     A single character array containing the the prefix character
        ///     that was used to initialize the class.
        /// </returns>
        public override char[] GetPrefixChars()
        {
            return new char[] { this.prefix };
        }


        /// <summary>
        ///     The padding character to use when justifying expanded macros.
        /// </summary>
        public char Padding
        {
            get
            {
                return this.padding;
            }
            set
            {
                this.padding = value;
            }
        }


        /// <summary>
        ///     The padding color (optional).
        /// </summary>
        public TextStyle PaddingStyle
        {
            get
            {
                return this.paddingStyle;
            }
            set
            {
                this.paddingStyle = value;
            }
        }


        /// <summary>
        ///     The character that marks the start of a macro string.
        /// </summary>
        public char Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {

                if (this.prefix != value)
                {
                    this.prefix = value;
                    base.OnPrefixChanged();
                }
            }
        }


        /// <summary>
        ///     The character that marks the end of a macro string.
        /// </summary>
        public char Suffix
        {
            get
            {
                return this.suffix;
            }
        }


        /// <summary>
        ///     Translates text containing a potential macro.
        /// </summary>
        /// <param name="terminal">
        ///     The terminal that receives output.
        /// </param>
        /// <param name="text">
        ///     The text to parse.
        /// </param>
        /// <param name="index">
        ///     The zero-based starting index where parsing
        ///     should begin.  This is always the prefix character
        ///     for macros.
        /// </param>
        /// <returns>
        ///     The zero-based index where processing should continue
        ///     by the terminal (and other translators).  An invalid index
        ///     is treated by the terminal engine as an unrecognized 
        ///     control code that can be sent as raw text or passed to
        ///     another translator.
        /// </returns>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // The execution reached this point because the parameters
            // passed the initial null and range checks.  From this
            // point forward, any errors are treated as syntactical
            // errors that cause the function to ignore the call but
            // not raise an exception.

            if (text[index] != this.prefix)
            {
                return index;
            }

            // Get the index of the suffix character.  The macro
            // name and formatting strings are located between
            // these characters.

            int suffixIndex = text.IndexOf(this.suffix, index + 1);

            if (suffixIndex == -1)
            {

                // The suffix character is not located in the string.
                // This is not a valid macro code and should
                // be ignored.

                return index;
            }

            // Extract the contents of the macro (the name and
            // any optional formatting codes).

            string contents =
                text.Substring(index + 1, suffixIndex - index - 1).Trim();

            if (contents.Length == 0)
            {

                // There is no content in this macro, so exit processing.
                // This is not a valid macro code.

                return index;

            }

            // A colon (:) is used to separate a macro name
            // and its formatting string.  The formatting string
            // is optional.

            string name;
            string format;

            int colonIndex = contents.IndexOf(':');

            if (colonIndex == -1)
            {
                name = contents;
                format = string.Empty;
            }
            else
            {
                name = contents.Substring(0, colonIndex);
                format = contents.Substring(colonIndex + 1);
            }

            // Now get the basic (unformatted) value of this macro.
            // This is implemented by the inherited class.

            object value = Expand(name);

            if (value == null)
            {

                // The inherited class did not return a value, which
                // means this macro was not recognized. 

                return index;

            }

            if (format.Length == 0)
            {

                // No formatting string was specified.  Write the raw
                // value and exit immediately.

                terminal.Write(value.ToString());
                return suffixIndex + 1;
            }

            int length;
            if (int.TryParse(format, out length))
            {

                // A length format was designated.  A positive
                // value indicates left justification and a negative
                // value indicates right justification.

                if (length != 0)
                {

                    if (length > 0)
                        terminal.WritePadded(
                            value.ToString(),
                            length,
                            JustifyText.Left,
                            padding,
                            paddingStyle);
                    else

                        terminal.WritePadded(
                            value.ToString(),
                            length * -1,
                            JustifyText.Right,
                            padding,
                            paddingStyle);

                }

                return suffixIndex + 1;

            }
            else
            {

                // The formatting string was not recognized.
                // This is not a valid code.

                return index;
            }

        }

    }
}

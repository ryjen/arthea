
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals.Translators
{


    /// <summary>
    ///     Translates GAP BBS XCode strings.  An XCode is a control
    ///     code in the format pXnn, where p is a sysop-defined prefix
    ///     character, X is an upper-case X, and nn is a two-character
    ///     hexadecimal number.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///       GAP collapsed double-instances of the prefix character.
    ///       For example, if the prefix was defined as "@", then "@@"
    ///       would be collapsed to a single "@".
    ///     </para>
    /// </remarks>
    [TranslatorKey("xcode")]
    public class XCodeTranslator : Translator
    {

        #region [ Private Fields ]

        /// <summary>
        ///     Tracks whether or not the sysop wants to collapse
        ///     double instances of the prefix character.
        /// </summary>
        private bool collapseDoubles;

        /// <summary>
        ///     Tracks the prefix character (the default is '@').
        /// </summary>
        private char prefix;

        #endregion

        #region [ Constructors ]

        /// <summary>
        ///     Creates a new x-code translator with default values. 
        /// </summary>
        public XCodeTranslator()
        {
            this.collapseDoubles = true;
            this.prefix = '@';
        }

        /// <summary>
        ///     Creates a new x-code transator with the specified
        ///     character as a prefix character.
        /// </summary>
        /// <param name="prefix">
        ///     A Unicode character.
        /// </param>
        /// <remarks>
        ///     Do not specify a character that is likely to appear
        ///     in text, such as an English letter or number.
        /// </remarks>
        public XCodeTranslator(char prefix)
        {
            this.prefix = prefix;
        }

        #endregion

        #region [ CollapseDoubles ]

        /// <summary>
        ///     Indicates whether or not the translator
        ///     should collapse double prefix characters.
        /// </summary>
        public bool CollapseDoubles
        {
            get
            {
                return this.collapseDoubles;
            }
            set
            {
                this.collapseDoubles = value;
            }
        }

        #endregion

        #region [ GetPrefixChars ]

        /// <summary>
        ///     Returns an array containing the <see cref="Prefix"/>
        ///     configured for the translator.
        /// </summary>
        public override char[] GetPrefixChars()
        {
            return new char[] { this.prefix };
        }

        #endregion

        #region [ Prefix ]

        /// <summary>
        ///     The sysop-defined prefix of the GAP codes.
        /// </summary>
        /// <remarks>
        ///     You must re-register the translator when changing the prefix character.
        ///     This is necessary because the translation engine caches prefix information
        ///     for improved performance.  Changing the prefix invalidates any cached
        ///     information.  It is safe to change the sysop-defined prefix before
        ///     registration.
        /// </remarks>
        public char Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {
                this.prefix = value;
                base.OnPrefixChanged();
            }
        }

        #endregion

        #region [ Translate ]

        /// <summary>
        ///     Translates a GAP-style BBS code at the specified position in a string.
        /// </summary>
        public override int Translate(Terminal terminal, string text, int index)
        {

            ValidateParameters(terminal, text, index);

            // The parameters have been validated.  From this point
            // forward, errors are syntactical errors that should not
            // raise an exception, but rather return the same index
            // passed to the function.  This tells the engine that
            // the translator could not identify a valid code.
            //
            // An XCode color has the format <prefix>Xnn, which is exactly
            // four characters in length.  This is the shortest possible
            // length -- so make sure there is enough room in the
            // string for at least this type of code.

            if (index > text.Length - 4)
            {
                return index;
            }

            // Make sure the current prefix is actually located at
            // the specified position.  It is possible for the engine
            // to call this translator when the index position
            // is not the prefix -- suppose, for example, the
            // prefix property was set by a different thread after
            // the call was started by the engine.  For this reason
            // an exception should not be raised.
            //
            // Also make sure the second character is an upper-case
            // X.  It is not known if GAP was case-sensitive, so 
            // this translator will stick with sensitivity.

            if ((text[index] != prefix) || (text[index + 1] != 'X'))
            {
                return index;
            }

            // At this point, a valid prefix has been found.  The
            // next step is to see if the next two characters (after
            // the upper-case X) are hexadecimal characters.

            if (IsHexDigit(text[index + 2]) && IsHexDigit(text[index + 3]))
            {

                terminal.SetStyle(
                  (byte)ParseHexChar(text[index + 3]),
                  (byte)ParseHexChar(text[index + 2]),
                  CgaHighBit.Blink);

                return index + 4;

            }

            // No recognized code.  In the future, add support
            // for codes like @CLS.

            return index;
        }

        #endregion

    }
}


/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Security;

namespace Thought.Terminals
{
    public partial class Terminal
    {


        /// <summary>
        ///     Prompts the user for a password with the minimum
        ///     and maximum length.
        /// </summary>
        /// <param name="minimumLength">
        ///     The minimum length of the password.
        /// </param>
        /// <param name="maximumLength">
        ///     The maximum length of the password.
        /// </param>
        /// <returns>
        ///     The password entered by the user.
        /// </returns>
        public string PromptPassword(
            int minimumLength,
            int maximumLength)
        {

            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException("minimumLength");

            if (maximumLength < 0)
                throw new ArgumentOutOfRangeException("maximumLength");

            TextPromptSettings settings = new TextPromptSettings();

            settings.EntryCharacter = '*';
            settings.FillOut = true;
            settings.Length = maximumLength;
            settings.WriteNewLine = true;

            return PromptText(null, null, settings);

        }

    }
}

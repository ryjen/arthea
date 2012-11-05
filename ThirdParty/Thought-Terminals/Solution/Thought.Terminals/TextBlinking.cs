
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
    ///     The blinking rate of a text.
    /// </summary>
    public enum TextBlinking
    {

        /// <summary>
        ///     No blinking.
        /// </summary>
        None,

        /// <summary>
        ///     Fast blinking (150 or more per minute).
        /// </summary>
        Fast,

        /// <summary>
        ///     Slow blinking (less than 150 per minute).
        /// </summary>
        Slow
    }
}

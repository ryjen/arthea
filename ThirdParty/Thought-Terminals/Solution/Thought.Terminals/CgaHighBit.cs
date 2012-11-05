
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Possible meanings for the high bit of a CGA/DOS color attribute.
    /// </summary>
    public enum CgaHighBit
    {

        /// <summary>
        ///     The high bit indicates blinking.
        /// </summary>
        Blink,

        /// <summary>
        ///     The high bit indicates a bright background.
        /// </summary>
        BrightBackground,

        /// <summary>
        ///     The high bit is ignored.
        /// </summary>
        Ignore

    }
}

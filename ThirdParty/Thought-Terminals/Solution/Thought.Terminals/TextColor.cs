
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     The color of text or a background.
    /// </summary>
    public enum TextColor
    {
		Undefined = 0xFF,

        /// <summary>
        ///     The color black
        /// </summary>
        Black = 0x00,

        /// <summary>
        ///     The color blue
        /// </summary>
        Blue = 0x01,

        /// <summary>
        ///     The color dark green
        /// </summary>
        Green = 0x02,

        /// <summary>
        ///     The color dark cyan
        /// </summary>
        Cyan = 0x03,

        /// <summary>
        ///     The color dark red
        /// </summary>
        Red = 0x04,

        /// <summary>
        ///     The color dark magenta
        /// </summary>
        Magenta = 0x05,

        /// <summary>
        ///     The color dark yellow (brown)
        /// </summary>
        Yellow = 0x06,

        /// <summary>
        ///     The color gray
        /// </summary>
        White = 0x07,

    }
}

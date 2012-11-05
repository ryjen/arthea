
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     The intensity of text written to the terminal.
    /// </summary>
    public enum TextIntensity
    {

        /// <summary>
        ///     Normal intensity (not faint and not bold)
        /// </summary>
        Normal,

        /// <summary>
        ///     Bold intensity
        /// </summary>
        Bold,

        /// <summary>
        ///     Faint intensity (not widely supported)
        /// </summary>
        Faint

    }
}

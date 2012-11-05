
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Character constants for named ASCII characters.
    /// </summary>
    /// <seealso cref="AsciiByte"/>
    /// <seealso cref="AsciiControlChar"/>
    public static class AsciiChar
    {

        /// <summary>
        ///     Null character (^@)
        /// </summary>
        public const char NUL = '\x0';


        /// <summary>
        ///     Start of Heading (^A)
        /// </summary>
        public const char SOH = '\x1';


        /// <summary>
        ///     Start of Text (^B)
        /// </summary>
        public const char STX = '\x2';


        /// <summary>
        ///     End of Text (^C)
        /// </summary>
        public const char ETX = '\x3';


        /// <summary>
        ///     End of Transmission (^D)
        /// </summary>
        public const char EOT = '\x4';


        /// <summary>
        ///     Enquiry (^E)
        /// </summary>
        public const char ENQ = '\x5';


        /// <summary>
        ///     Acknowledgement (^F)
        /// </summary>
        public const char ACK = '\x6';


        /// <summary>
        ///     Bell (^G)
        /// </summary>
        public const char BEL = '\x7';


        /// <summary>
        ///     Backspace (^H)
        /// </summary>
        public const char BS = '\x8';


        /// <summary>
        ///     Horizontal Tab (^I)
        /// </summary>
        public const char TAB = '\x9';


        /// <summary>
        ///     Linefeed (^J)
        /// </summary>
        public const char LF = '\xA';


        /// <summary>
        ///     Vertical Tab (^K)
        /// </summary>
        public const char VT = '\xB';


        /// <summary>
        ///     Form Feed (^L)
        /// </summary>
        public const char FF = '\xC';


        /// <summary>
        ///     Carriage Return (^M)
        /// </summary>
        public const char CR = '\xD';


        /// <summary>
        ///     Shift Out (^N)
        /// </summary>
        public const char SO  = '\xE';


        /// <summary>
        ///     Shift In (^O)
        /// </summary>
        public const char SI  = '\xF';


        /// <summary>
        ///     Data Link Escape (^P)
        /// </summary>
        public const char DLE = '\x10';


        /// <summary>
        ///     Device Control 1 (^Q)
        /// </summary>
        public const char DC1 = '\x11';


        /// <summary>
        ///     Device Control 2 (^R)
        /// </summary>
        public const char DC2 = '\x12';


        /// <summary>
        ///     Device Control 3 (^S)
        /// </summary>
        public const char DC3 = '\x13';


        /// <summary>
        ///     Device Control 4 (^T)
        /// </summary>
        public const char DC4 = '\x14';


        /// <summary>
        ///     Negative Acknowledge (^U)
        /// </summary>
        public const char NAK = '\x15';


        /// <summary>
        ///     Synchronous Idle (^V)
        /// </summary>
        public const char SYN = '\x16';


        /// <summary>
        ///     End of Transmission Block (^W)
        /// </summary>
        public const char ETB = '\x17';


        /// <summary>
        ///     Cancel (^X)
        /// </summary>
        public const char CAN = '\x18';


        /// <summary>
        ///     End of Medium (^Y)
        /// </summary>
        public const char EM = '\x19';

        /// <summary>
        ///     Substitute (^Z)
        /// </summary>
        public const char SUB = '\x1A';


        /// <summary>
        ///     Escape (^[)
        /// </summary>
        public const char ESC = '\x1B';


        /// <summary>
        ///     File Separator (^\)
        /// </summary>
        public const char FS = '\x1C';


        /// <summary>
        ///     Group Separator (^])
        /// </summary>
        public const char GS = '\x1D';


        /// <summary>
        ///     Record Separator (^^)
        /// </summary>
        public const char RS = '\x1E';


        /// <summary>
        ///     Unit Separator (^_)
        /// </summary>
        public const char US = '\x1F';

        /// <summary>
        ///     Space character
        /// </summary>
        public const char Space = ' ';

        /// <summary>
        ///     Delete (^?)
        /// </summary>
        public const char DEL = '\x7F';

    }
}

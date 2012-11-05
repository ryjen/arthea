
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Enumerates ASCII control characters as bytes.
    /// </summary>
    /// <remarks>
    ///     This enumeration is similar to the <see cref="AsciiChar"/>
    ///     static class.  This class defines each ASCII control character
    ///     as a byte; the AsciiChar class defines each ASCII control
    ///     character as a character constant.
    /// </remarks>
    /// <seealso cref="AsciiChar"/>
    /// <seealso cref="AsciiControlChar"/>
    public enum AsciiByte : byte
    {

        /// <summary>
        ///     Null character (^@)
        /// </summary>
        NUL =  0,

        /// <summary>
        ///     Start of Heading (^A)
        /// </summary>
        SOH =  1,

        /// <summary>
        ///     Start of Text (^B)
        /// </summary>
        STX =  2,

        /// <summary>
        ///     End of Text (^C)
        /// </summary>
        ETX =  3,

        /// <summary>
        ///     End of Transmission (^D)
        /// </summary>
        EOT =  4,

        /// <summary>
        ///     Enquiry (^E)
        /// </summary>
        ENQ =  5,

        /// <summary>
        ///     Acknowledgement (^F)
        /// </summary>
        ACK =  6,

        /// <summary>
        ///     Bell (^G)
        /// </summary>
        BEL =  7,

        /// <summary>
        ///     Backspace (^H)
        /// </summary>
        BS  =  8,

        /// <summary>
        ///     Horizontal Tab (^I)
        /// </summary>
        TAB =  9,

        /// <summary>
        ///     Line Feed (^J)
        /// </summary>
        LF  = 10,

        /// <summary>
        ///     Vertical Tab (^K)
        /// </summary>
        VT  = 11,

        /// <summary>
        ///     Form Feed (^L)
        /// </summary>
        FF  = 12,

        /// <summary>
        ///     Carriage Return (^M)
        /// </summary>
        CR  = 13,

        /// <summary>
        ///     Shift Out (^N)
        /// </summary>
        SO  = 14,

        /// <summary>
        ///     Shift In (^O)
        /// </summary>
        SI  = 15,

        /// <summary>
        ///     Data Link Escape (^P)
        /// </summary>
        DLE = 16,

        /// <summary>
        ///     Device Control 1 (^Q)
        /// </summary>
        DC1 = 17,

        /// <summary>
        ///     Device Control 2 (^R)
        /// </summary>
        DC2 = 18,

        /// <summary>
        ///     Device Control 3 (^S)
        /// </summary>
        DC3 = 19,

        /// <summary>
        ///     Device Control 4 (^T)
        /// </summary>
        DC4 = 20,

        /// <summary>
        ///     Negative Acknowledge (^U)
        /// </summary>
        NAK = 21,

        /// <summary>
        ///     Synchronous Idle (^V)
        /// </summary>
        SYN = 22,

        /// <summary>
        ///     End of Transmission Block (^W)
        /// </summary>
        ETB = 23,

        /// <summary>
        ///     Cancel (^X)
        /// </summary>
        CAN = 24,

        /// <summary>
        ///     End of Medium (^Y)
        /// </summary>
        EM  = 25,

        /// <summary>
        ///     Substitute (^Z)
        /// </summary>
        SUB = 26,

        /// <summary>
        ///     Escape (^[)
        /// </summary>
        ESC = 27,

        /// <summary>
        ///     File Separator (^\)
        /// </summary>
        FS  = 28,

        /// <summary>
        ///     Group Separator (^])
        /// </summary>
        GS  = 29,

        /// <summary>
        ///     Record Separator (^^)
        /// </summary>
        RS  = 30,

        /// <summary>
        ///     Unit Separator (^_)
        /// </summary>
        US  = 31,

        /// <summary>
        ///     Delete (^?)
        /// </summary>
        DEL = 127


    }
}

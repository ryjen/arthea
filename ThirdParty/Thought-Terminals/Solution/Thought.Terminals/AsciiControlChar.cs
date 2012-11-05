
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{
    /// <summary>
    ///     Provides aliases for ASCII control characters (e.g.
    ///     control A, control P, etc).
    /// </summary>
    /// 
    /// <remarks>
    /// 
    ///     <para>
    ///         The majority of the ASCII control characters are
    ///         obsolete; however, they have been used for other
    ///         purposes (such as prefixes for custom color codes) and
    ///         are defined here for easy access.
    ///     </para>
    /// 
    /// </remarks>
    /// 
    /// <seealso cref="AsciiByte"/>
    /// <seealso cref="AsciiChar"/>
    
    public static class AsciiControlChar
    {

        /// <summary>
        ///     Represents a ^@ (ASCII #0), also called NUL.
        /// </summary>
        /// <remarks>
        /// 
        ///     <para>
        ///         An at-sign (@) is used because it the character 
        ///         immediately proceeding a capital A in the
        ///         ASCII character set.  A control-a (^A) is
        ///         ASCII character 1.
        ///     </para>
        /// 
        /// </remarks>
        public const char ControlAt = '\x00';


        /// <summary>
        ///     Control-A (SOH).
        /// </summary>
        public const char ControlA = '\x01';


        /// <summary>
        ///     Control-B (STX).
        /// </summary>
        public const char ControlB = '\x02';


        /// <summary>
        ///     Control-C (ETX).
        /// </summary>
        public const char ControlC = '\x03';


        /// <summary>
        ///     Control-D (EOT).
        /// </summary>
        public const char ControlD = '\x04';


        /// <summary>
        ///     Control-E (ENQ).
        /// </summary>
        public const char ControlE = '\x05';


        /// <summary>
        ///     Control-F (ACK).
        /// </summary>
        public const char ControlF = '\x06';


        /// <summary>
        ///     Control-G (BEL).
        /// </summary>
        public const char ControlG = '\x07';


        /// <summary>
        ///     Control-H (BS).
        /// </summary>
        public const char ControlH = '\x08';


        /// <summary>
        ///     Control-I (TAB).
        /// </summary>
        public const char ControlI = '\x09';


        /// <summary>
        ///     Control-J (LF).
        /// </summary>
        public const char ControlJ = '\x0A';


        /// <summary>
        ///     Control-K (VT).
        /// </summary>
        public const char ControlK = '\x0B';


        /// <summary>
        ///     Control-L (FF).
        /// </summary>
        public const char ControlL = '\x0C';


        /// <summary>
        ///     Control-M (CR).
        /// </summary>
        public const char ControlM = '\x0D';


        /// <summary>
        ///     Control-N (SO).
        /// </summary>
        public const char ControlN = '\x0E';


        /// <summary>
        ///     Control-O (SI).
        /// </summary>
        public const char ControlO = '\x0F';


        /// <summary>
        ///     Control-P (DLE).
        /// </summary>
        public const char ControlP = '\x10';


        /// <summary>
        ///     Control-Q (DC1).
        /// </summary>
        public const char ControlQ = '\x11';


        /// <summary>
        ///     Control-R (DC2).
        /// </summary>
        public const char ControlR = '\x12';


        /// <summary>
        ///     Control-S (DC3).
        /// </summary>
        public const char ControlS = '\x13';


        /// <summary>
        ///     Control-T (DC4).
        /// </summary>
        public const char ControlT = '\x14';


        /// <summary>
        ///     Control-U (NAK).
        /// </summary>
        public const char ControlU = '\x15';


        /// <summary>
        ///     A control-v character (also called the
        ///     SYN or Synchronous Idle character).
        /// </summary>
        /// <remarks>
        ///     A control-v is used as the prefix character
        ///     by the FidoNet AVATAR protocol.  AVATAR is a standard
        ///     for embedding color and cursor commands in
        ///     text sent to a remote terminal; it was popular
        ///     during the late 1980s and early 1990s in the
        ///     dial-up BBS community.
        /// </remarks>
        /// <seealso cref="AvatarTerminalProtocol"/>
        /// <seealso cref="Translators.AvatarTranslator"/>
        public const char ControlV = '\x16';
        

        /// <summary>
        ///     Control-W (ETB).
        /// </summary>
        public const char ControlW = '\x17';


        /// <summary>
        ///     Control-X (CAN).
        /// </summary>
        public const char ControlX = '\x18';


        /// <summary>
        ///     Control-Y (EM).
        /// </summary>
        public const char ControlY = '\x19';


        /// <summary>
        ///     Control-Z (SUB).
        /// </summary>
        public const char ControlZ = '\x1A';
    }
}

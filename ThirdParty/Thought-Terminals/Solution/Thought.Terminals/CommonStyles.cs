
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

namespace Thought.Terminals
{

    /// <summary>
    ///     Factory methods for creating common styles.
    /// </summary>
    public static class CommonStyles
    {

        /// <summary>
        ///     A bright white foreground over a black background.
        /// </summary>
        public static TextStyle BrightWhiteOnBlack()
        {
            return new TextStyle(
                TextColor.White,
                TextIntensity.Bold,
                TextColor.Black);
        }


        /// <summary>
        ///     A cyan foreground over a black background.
        /// </summary>
        public static TextStyle CyanOnBlack()
        {
            return new TextStyle(TextColor.Cyan, TextColor.Black);
        }


        /// <summary>
        ///     A dark gray foreground over a black background.
        /// </summary>
        public static TextStyle DarkGrayOnBlack()
        {
            return new TextStyle(TextColor.Black, TextIntensity.Bold);
        }


        /// <summary>
        ///     A dark gray foreground over a blue background.
        /// </summary>
        public static TextStyle DarkGrayOnBlue()
        {
            return new TextStyle(
                TextColor.Black,
                TextIntensity.Bold,
                TextColor.Blue);
        }


        /// <summary>
        ///     The default AVATAR style, which is a cyan foreground
        ///     over a black background.
        /// </summary>
        public static TextStyle DefaultAvatar()
        {
            return new TextStyle(TextColor.Cyan, TextColor.Black);
        }


        /// <summary>
        ///     The default DOS style, which is a white/gray foreground
        ///     over a black background.
        /// </summary>
        public static TextStyle DefaultDos()
        {
            return new TextStyle(TextColor.White, TextColor.Black);
        }


        /// <summary>
        ///     A green foreground over a black background.
        /// </summary>
        /// <returns></returns>
        public static TextStyle GreenOnBlack()
        {
            return new TextStyle(TextColor.Green, TextColor.Black);
        }


        /// <summary>
        ///     A red foreground over a black background.
        /// </summary>
        public static TextStyle RedOnBlack()
        {
            return new TextStyle(TextColor.Red, TextColor.Black);
        }


        /// <summary>
        ///     A white foreground color and a black background color.
        /// </summary>
        public static TextStyle WhiteOnBlack()
        {
            return new TextStyle(TextColor.White, TextColor.Black);
        }


        /// <summary>
        ///     A white foreground color and a blue background color.
        /// </summary>
        public static TextStyle WhiteOnBlue()
        {
            return new TextStyle(TextColor.White, TextColor.Blue);
        }

    }


}


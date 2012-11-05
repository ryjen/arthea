
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Represents the visual appearance of text.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class describes the visual appearance of text
    ///         such as its foreground color and intensity.  The
    ///         properties are largely based on the ANSI X3.64 and
    ///         similar standards.  Note that not all terminals
    ///         support all of the properties.
    ///     </para>
    /// </remarks>
    
    [Serializable]
    public class TextStyle
    {

        private TextColor backgroundColor;
        private TextIntensity backgroundIntensity;
        private TextBlinking blinking;
        private TextColor foregroundColor;
        private TextIntensity foregroundIntensity;
        private bool hidden;
        private bool italic;
        private bool reversed;
        private bool strikeThrough;
        private TextUnderline underline;

        #region [ TextStyle ]

        /// <summary>
        ///     Creates a blank text style with normal white
        ///     text over a black background.  No special
        ///     attributes are set.
        /// </summary>
        
        public TextStyle()
        {
            this.backgroundColor = TextColor.Undefined;
			this.foregroundColor = TextColor.Undefined;
        }

        #endregion

        #region [ TextStyle(ConsoleColor) ]

        /// <summary>
        ///     Initializes the text style by converting a 
        ///     System.ConsoleColor.
        /// </summary>
        /// <param name="consoleForeground">
        ///     The foreground color encoded as a System.ConsoleColor.
        /// </param>
        /// <remarks>
        ///     The System.ConsoleColor enumeration is not the same as
        ///     the <see cref="TextColor"/> enumeration and cannot be
        ///     used interchangeably without conversion.  The TextColor
        ///     enumeration is based on the ANSI X3.64 standard, while
        ///     the ConsoleColor enumeration is based on Microsoft
        ///     conventions.  Additionally, while some of the field names
        ///     are the same, the underlying values and meanings are
        ///     different.
        /// </remarks>
        public TextStyle(ConsoleColor consoleForeground)
        {
            SetForeground(consoleForeground);
			BackgroundColor = TextColor.Undefined;
        }

        #endregion

        #region [ TextStyle(ConsoleColor, ConsoleColor) ]

        /// <summary>
        ///     Initializes the TextStyle with the specified
        ///     System.ConsoleColor foreground and background colors.
        /// </summary>
        /// <param name="foreground">
        ///     The foreground defined as a System.ConsoleColor value.
        /// </param>
        /// <param name="background">
        ///     The background defined as a System.ConsoleColor value.
        /// </param>
        public TextStyle(ConsoleColor foreground, ConsoleColor background)
        {
            SetForeground(foreground);
            SetBackground(background);
        }

        #endregion

        #region [ TextStyle(TextColor) ]

        /// <summary>
        ///     Initializes the text style to the specified foreground
        ///     color over a black background.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The desired foreground color.
        /// </param>
        /// <remarks>
        ///     The background color is black with all special 
        ///     attributes like intensity and blinking set to
        ///     their default values.
        /// </remarks>
        public TextStyle(TextColor foregroundColor)
        {
            this.backgroundColor = TextColor.Undefined;
            this.foregroundColor = foregroundColor;
        }

        #endregion

        #region [ TextStyle(TextColor, TextColor) ]

        /// <summary>
        ///     Initializes the text style to the specified
        ///     foreground and background colors.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The foreground color.
        /// </param>
        /// <param name="backgroundColor">
        ///     The background color.
        /// </param>
        /// <remarks>
        ///     All other style attributes, such as the intensity 
        ///     and blinking status, are set to their default values.
        /// </remarks>
        public TextStyle(
            TextColor foregroundColor,
            TextColor backgroundColor)
        {
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
        }

        #endregion

        #region [ TextStyle(TextColor, TextIntensity) ]

        /// <summary>
        ///     Initializes the text style to the specified
        ///     foreground color and foreground intensity.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The foreground color.
        /// </param>
        /// <param name="foregroundIntensity">
        ///     The foreground intensity.
        /// </param>
        /// <remarks>
        ///     The background color is black.  Note that a faint
        ///     intensity is not widely supported.  For example, the
        ///     Windows/DOS console window supports normal and 
        ///     bold intensities only.
        /// </remarks>
        public TextStyle(
            TextColor foregroundColor,
            TextIntensity foregroundIntensity)
        {
            this.backgroundColor = TextColor.Undefined;
            this.foregroundColor = foregroundColor;
            this.foregroundIntensity = foregroundIntensity;
        }

        #endregion

        #region [ TextStyle(TextColor, TextIntensity, TextColor) ]

        /// <summary>
        ///     Initializes the text style with the specified foreground
        ///     color, foreground intensity, and background color.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The foreground color.
        /// </param>
        /// <param name="foregroundIntensity">
        ///     The foreground intensity.
        /// </param>
        /// <param name="backgroundColor">
        ///     The background color.
        /// </param>
        public TextStyle(
            TextColor foregroundColor,
            TextIntensity foregroundIntensity,
            TextColor backgroundColor)
        {
            this.foregroundColor = foregroundColor;
            this.foregroundIntensity = foregroundIntensity;
            this.backgroundColor = backgroundColor;
        }

        #endregion

        #region [ TextStyle(TextStyle) ]

        /// <summary>
        ///     Initializes the text style by copying the properties
        ///     of an existing text style.
        /// </summary>
        /// <param name="copyFrom">
        ///     The style to copy from.
        /// </param>
        public TextStyle(TextStyle copyFrom)
        {

            if (copyFrom == null)
                throw new ArgumentNullException("copyFrom");

            this.backgroundColor = copyFrom.backgroundColor;
            this.backgroundIntensity = copyFrom.backgroundIntensity;
            this.blinking = copyFrom.blinking;
            this.foregroundColor = copyFrom.foregroundColor;
            this.foregroundIntensity = copyFrom.foregroundIntensity;
            this.hidden = copyFrom.hidden;
            this.italic = copyFrom.italic;
            this.reversed = copyFrom.reversed;
            this.strikeThrough = copyFrom.strikeThrough;
            this.underline = copyFrom.underline;

        }

        #endregion

        #region [ TextStyle(TextStyle, int[]) ]

        /// <summary>
        ///     Initializes the text style by copying the properties
        ///     of an existing text style.
        /// </summary>
        /// <param name="copyFrom">
        ///     The style to copy from.
        /// </param>
        /// <param name="sgrParameters">
        ///     An array of Set Graphics Rendition values from
        ///     the X3.64 or ECMA-48 standards.
        /// </param>
        public TextStyle(TextStyle copyFrom, int[] sgrParameters)
            : this(copyFrom)
        {

            if (sgrParameters == null)
                throw new ArgumentNullException("sgrParameters");

            ApplyGraphicRenditions(sgrParameters);

        }

        #endregion

        #region [ TextStyle(byte, CgaHighBit) ]

        /// <summary>
        ///     Initializes the text style based on a CGA/DOS attribute.
        /// </summary>
        public TextStyle(
            byte cgaAttribute,
            CgaHighBit highBitMeaning)
        {
            SetColor(cgaAttribute, highBitMeaning);
        }

        #endregion

        #region [ TextStyle(byte, byte, CgaHighBit) ]

        /// <summary>
        ///     Initialize the text style to a CGA-encoded foreground
        ///     and background color.
        /// </summary>
        /// <param name="cgaForeground">
        ///     The foreground encoded as a CGA attribute.
        /// </param>
        /// <param name="cgaBackground">
        ///     The background encoded as a CGA attribute.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA background.
        /// </param>
        public TextStyle(
            byte cgaForeground,
            byte cgaBackground,
            CgaHighBit highBitMeaning)

        {
            SetColor(cgaForeground, cgaBackground, highBitMeaning);
        }

        #endregion

        #region [ ApplyGraphicRenditions ]

        /// <summary>
        ///     Applies a series of Select Graphic Rendition
        ///     parameters from the X3.64 standard (also known
        ///     as ANSI-BBS, ECMA-48 and ISO/IEC 6429).
        /// </summary>
        /// <param name="parameters">
        ///     An integer array containing rendition parameters
        ///     per the specification.  Parameters are applied
        ///     in the order listed in the array.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         The list of parameters is defined in
        ///         section 8.3.117 of ECMA-48.  Not all ECMA-48
        ///         parameters are supported.
        ///     </para>
        ///     <para>
        ///         Some terminals (e.g. SCO ANSI, AIXTERM) support
        ///         parameters to set the foreground and background
        ///         to bright colors.  These parameters are in the
        ///         range of 90 to 97 and 100 to 107.  Unfortunately
        ///         documentation is lacking and both terminals use
        ///         different color values.  These extended 
        ///         parameters are not supported.
        ///     </para>
        /// </remarks>
        public void ApplyGraphicRenditions(int[] parameters)
        {

            foreach (int parameter in parameters)
            {
                switch (parameter)
                {
                    case 0:
                        Reset();
                        break;

                    case 1:
                        this.foregroundIntensity = TextIntensity.Bold;
                        break;

                    case 2:
                        this.foregroundIntensity = TextIntensity.Faint;
                        break;

                    case 3:
                        this.italic = true;
                        break;

                    case 4:
                        this.underline = TextUnderline.Single;
                        break;

                    case 5:
                        this.blinking = TextBlinking.Slow;
                        break;

                    case 6:
                        this.blinking = TextBlinking.Fast;
                        break;

                    case 7:
                        this.reversed = true;
                        break;

                    case 8:
                        this.hidden = true;
                        break;

                    case 9:
                        this.strikeThrough = true;
                        break;

                    case 21:
                        this.underline = TextUnderline.Double;
                        break;

                    case 22:
                        this.foregroundIntensity = TextIntensity.Normal;
                        break;

                    case 23:
                        this.italic = false;
                        break;

                    case 24:
                        this.underline = TextUnderline.None;
                        break;

                    case 25:
                        this.blinking = TextBlinking.None;
                        break;

                    case 27:
                        this.reversed = false;
                        break;

                    case 28:
                        this.hidden = false;
                        break;

                    case 29:
                        this.strikeThrough = false;
                        break;

                    case 30:
                        this.foregroundColor = TextColor.Black;
                        break;

                    case 31:
                        this.foregroundColor = TextColor.Red;
                        break;

                    case 32:
                        this.foregroundColor = TextColor.Green;
                        break;

                    case 33:
                        this.foregroundColor = TextColor.Yellow;
                        break;

                    case 34:
                        this.foregroundColor = TextColor.Blue;
                        break;

                    case 35:
                        this.foregroundColor = TextColor.Magenta;
                        break;

                    case 36:
                        this.foregroundColor = TextColor.Cyan;
                        break;

                    case 37:
                        this.foregroundColor = TextColor.White;
                        break;

                    case 40:
                        this.backgroundColor = TextColor.Black;
                        break;

                    case 41:
                        this.backgroundColor = TextColor.Red;
                        break;

                    case 42:
                        this.backgroundColor = TextColor.Green;
                        break;

                    case 43:
                        this.backgroundColor = TextColor.Yellow;
                        break;

                    case 44:
                        this.backgroundColor = TextColor.Blue;
                        break;

                    case 45:
                        this.backgroundColor = TextColor.Magenta;
                        break;

                    case 46:
                        this.backgroundColor = TextColor.Cyan;
                        break;

                    case 47:
                        this.backgroundColor = TextColor.White;
                        break;

                }

            }

        }

        #endregion

        #region [ BackgroundColor ]

        /// <summary>
        ///     The background color.
        /// </summary>
        /// <seealso cref="BackgroundIntensity"/>
        /// <seealso cref="ForegroundColor"/>
        /// <seealso cref="ForegroundIntensity"/>
        public TextColor BackgroundColor
        {
            get
            {
                return this.backgroundColor;
            }
            set
            {
                this.backgroundColor = value;
            }
        }

        #endregion

        #region [ BackgroundIntensity ]

        /// <summary>
        ///     The intensity of the background.
        /// </summary>
        /// <seealso cref="BackgroundColor"/>
        /// <seealso cref="ForegroundColor"/>
        /// <seealso cref="ForegroundIntensity"/>
        public TextIntensity BackgroundIntensity
        {
            get
            {
                return this.backgroundIntensity;
            }
            set
            {
                this.backgroundIntensity = value;
            }
        }

        #endregion

        #region [ Blinking ]

        /// <summary>
        ///     The blinking state of the text.
        /// </summary>
        /// <remarks>
        ///     The blinking rate is always fast on DOS/IBM systems.
        /// </remarks>
        public TextBlinking Blinking
        {
            get
            {
                return this.blinking;
            }
            set
            {
                this.blinking = value;
            }
        }

        #endregion

        #region [ Clone ]

        /// <summary>
        ///     Builds a copy of the text style.
        /// </summary>
        /// <returns>
        ///     A <see cref="TextStyle"/> with the identical properties.
        /// </returns>
        public TextStyle Clone()
        {
            return new TextStyle(this);
        }

        #endregion

        #region [ CloneBlinking ]

        /// <summary>
        ///     Clones the current style while enabling blinking.
        /// </summary>
        /// <returns>
        ///     A copy of the current style with the
        ///     <see cref="Blinking"/> property to fast.
        /// </returns>
        public TextStyle CloneBlinking()
        {
            TextStyle newStyle = new TextStyle(this);
            newStyle.Blinking = TextBlinking.Fast;
            return newStyle;
        }

        #endregion

        #region [ CloneBolded ]

        /// <summary>
        ///     Builds a copy of the text style but with the foreground bolded.
        /// </summary>
        /// <returns>
        ///     A <see cref="TextStyle"/> with identical properties except
        ///     for the foreground intensity, which is bolded.
        /// </returns>
        public TextStyle CloneBolded()
        {
            TextStyle newStyle = new TextStyle(this);
            newStyle.ForegroundIntensity = TextIntensity.Bold;
            return newStyle;
        }

        #endregion

        #region [ Equals ]

		public override int GetHashCode ()
		{
			return backgroundColor.GetHashCode() + backgroundIntensity.GetHashCode() +
				blinking.GetHashCode() + foregroundColor.GetHashCode() + foregroundIntensity.GetHashCode()
					+ hidden.GetHashCode() + italic.GetHashCode() + reversed.GetHashCode() +
					strikeThrough.GetHashCode() + underline.GetHashCode();
		}
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            TextStyle otherStyle = (TextStyle)obj;

            return
                (this.backgroundColor == otherStyle.backgroundColor) &&
                (this.backgroundIntensity == otherStyle.backgroundIntensity) &&
                (this.blinking == otherStyle.blinking) &&
                (this.foregroundColor == otherStyle.foregroundColor) &&
                (this.foregroundIntensity == otherStyle.foregroundIntensity) &&
                (this.hidden == otherStyle.hidden) &&
                (this.italic == otherStyle.italic) &&
                (this.reversed == otherStyle.reversed) &&
                (this.strikeThrough == otherStyle.strikeThrough) &&
                (this.underline == otherStyle.underline);

        }

        #endregion

        #region [ ForegroundColor ]

        /// <summary>
        ///     The foreground color.
        /// </summary>
        /// <seealso cref="BackgroundColor"/>
        /// <seealso cref="BackgroundIntensity"/>
        /// <seealso cref="ForegroundIntensity"/>
        public TextColor ForegroundColor
        {
            get
            {
                return this.foregroundColor;
            }
            set
            {
                this.foregroundColor = value;
            }
        }

        #endregion

        #region [ ForegroundIntensity ]

        /// <summary>
        ///     The intensity of the foreground.
        /// </summary>
        /// <seealso cref="BackgroundColor"/>
        /// <seealso cref="BackgroundIntensity"/>
        /// <seealso cref="ForegroundColor"/>
        public TextIntensity ForegroundIntensity
        {
            get
            {
                return this.foregroundIntensity;
            }
            set
            {
                this.foregroundIntensity = value;
            }
        }

        #endregion

        #region [ Hidden ]

        /// <summary>
        ///     The visibility of the text.
        /// </summary>
        public bool Hidden
        {
            get
            {
                return this.hidden;
            }
            set
            {
                this.hidden = value;
            }
        }

        #endregion

        #region [ Italic ]

        /// <summary>
        ///     The italic style of the text.
        /// </summary>
        /// <remarks>
        ///     The italic appearance is not widely supported.
        /// </remarks>
        public bool Italic
        {
            get
            {
                return this.italic;
            }
            set
            {
                this.italic = value;
            }
        }

        #endregion

        #region [ Reset ]

        /// <summary>
        ///     Resets the style to white over a black background.
        ///     All other properties are cleared or set to their
        ///     default values.
        /// </summary>
        public void Reset()
        {
            this.backgroundColor = TextColor.Black;
            this.backgroundIntensity = TextIntensity.Normal;
            this.blinking = TextBlinking.None;
            this.foregroundColor = TextColor.White;
            this.foregroundIntensity = TextIntensity.Normal;
            this.hidden = false;
            this.italic = false;
            this.reversed = false;
            this.strikeThrough = false;
            this.underline = TextUnderline.None;
        }

        #endregion

        #region [ Reversed ]

        /// <summary>
        ///     The inversed, reversed or negative status of the text.
        /// </summary>
        public bool Reversed
        {
            get
            {
                return this.reversed;
            }
            set
            {
                this.reversed = value;
            }
        }

        #endregion

        #region [ SetBackground(byte, CgaHighBit) ]

        /// <summary>
        ///     Sets the background based on a CGA color value.
        /// </summary>
        /// <param name="cgaBackground">
        ///     The color as a CGA attribute value ranging from 0 to 16.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high byte in a four-bit CGA color.
        /// </param>
        public void SetBackground(byte cgaBackground, CgaHighBit highBitMeaning)
        {

            switch (cgaBackground & 0x07)
            {
                case 0: this.backgroundColor = TextColor.Black; break;
                case 1: this.backgroundColor = TextColor.Blue; break;
                case 2: this.backgroundColor = TextColor.Green; break;
                case 3: this.backgroundColor = TextColor.Cyan; break;
                case 4: this.backgroundColor = TextColor.Red; break;
                case 5: this.backgroundColor = TextColor.Magenta; break;
                case 6: this.backgroundColor = TextColor.Yellow; break;
                case 7: this.backgroundColor = TextColor.White; break;
            }

            if ((cgaBackground & 0x8) == 0x8)
            {

                switch (highBitMeaning)
                {
                    case CgaHighBit.Blink:
                        this.backgroundIntensity = TextIntensity.Normal;
                        this.blinking = TextBlinking.Fast;
                        break;

                    case CgaHighBit.BrightBackground:
                        this.backgroundIntensity = TextIntensity.Bold;
                        this.blinking = TextBlinking.None;
                        break;

                    default:
                        throw new NotSupportedException();

                }

            }
            else
            {
                this.backgroundIntensity = TextIntensity.Normal;
                this.blinking = TextBlinking.None;
            }
        }

        #endregion

        #region [ SetBackground(ConsoleColor) ]

        /// <summary>
        ///     Sets the background color and intensity status
        ///     based on a ConsoleColor.
        /// </summary>
        /// <param name="consoleColor">
        ///     The background color encoded as a system ConsoleColor.
        /// </param>
        /// <remarks>
        ///     The ConsoleColor enumeration encodes both a color and
        ///     its intensity status.  Therefore calling this function
        ///     will set the background intensity to normal or bold.
        /// </remarks>
        public void SetBackground(ConsoleColor consoleColor)
        {

            switch (consoleColor)
            {
                case ConsoleColor.Black:
                    this.backgroundColor = TextColor.Black;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Blue:
                    this.backgroundColor = TextColor.Blue;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Cyan:
                    this.backgroundColor = TextColor.Cyan;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkBlue:
                    this.backgroundColor = TextColor.Blue;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkCyan:
                    this.backgroundColor = TextColor.Cyan;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkGray:
                    this.backgroundColor = TextColor.Black;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkGreen:
                    this.backgroundColor = TextColor.Green;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkMagenta:
                    this.backgroundColor = TextColor.Magenta;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkRed:
                    this.backgroundColor = TextColor.Red;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkYellow:
                    this.backgroundColor = TextColor.Yellow;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Gray:
                    this.backgroundColor = TextColor.White;
                    this.backgroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Green:
                    this.backgroundColor = TextColor.Green;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Magenta:
                    this.backgroundColor = TextColor.Magenta;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Red:
                    this.backgroundColor = TextColor.Red;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.White:
                    this.backgroundColor = TextColor.White;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Yellow:
                    this.backgroundColor = TextColor.Yellow;
                    this.backgroundIntensity = TextIntensity.Bold;
                    break;

                default:
                    throw new NotSupportedException();
            }

        }

        #endregion

        #region [ SetBackground(TextColor) ]

        /// <summary>
        ///     Changes the background color without impacting any
        ///     other attributes, including the intensity state of
        ///     the background.
        /// </summary>
        /// <param name="backgroundColor">
        ///     The new background color.
        /// </param>
        public void SetBackground(TextColor backgroundColor)
        {
            this.backgroundColor = backgroundColor;
        }

        #endregion

        #region [ SetColor(byte, CgaHighBit) ]

        /// <summary>
        ///     Sets the foreground and background properties based
        ///     on a CGA-encoded color attribute.
        /// </summary>
        /// <param name="cgaAttribute">
        ///     A CGA-encoded color attribute.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA attribute.
        /// </param>
        public void SetColor(byte cgaAttribute, CgaHighBit highBitMeaning)
        {

            // The foreground color is encoded as the lower three
            // bits of the CGA attribute.
            //
            // ----000 = Foreground Black
            // ----001 = Foreground Blue
            // ----010 = Foreground Green
            // ----011 = Foreground Cyan
            // ----100 = Foreground Red
            // ----101 = Foreground Magenta
            // ----110 = Foreground Yellow
            // ----111 = Foreground White

            switch (cgaAttribute & 0x07)
            {
                case 0: this.foregroundColor = TextColor.Black; break;
                case 1: this.foregroundColor = TextColor.Blue; break;
                case 2: this.foregroundColor = TextColor.Green; break;
                case 3: this.foregroundColor = TextColor.Cyan; break;
                case 4: this.foregroundColor = TextColor.Red; break;
                case 5: this.foregroundColor = TextColor.Magenta; break;
                case 6: this.foregroundColor = TextColor.Yellow; break;
                case 7: this.foregroundColor = TextColor.White; break;
            }

            // The foreground intensity is encoded in bit 3.  Faint
            // intensity is not supported.  The text is bold/bright if
            // the bit is set, or normal otherwise.

            this.foregroundIntensity =
                ((cgaAttribute & 0x08) == 0x08) ? TextIntensity.Bold : TextIntensity.Normal;

            // The background color is define in bits 4, 5 and 6.
            //
            // -000--- = Background Black
            // -001--- = Background Blue
            // -010--- = Background Green
            // -011--- = Background Cyan
            // -100--- = Background Red
            // -101--- = Background Magenta
            // -110--- = Background Yellow
            // -111--- = Background White

            switch (cgaAttribute & 0x70)
            {
                case 0x00: this.backgroundColor = TextColor.Black; break;
                case 0x10: this.backgroundColor = TextColor.Blue; break;
                case 0x20: this.backgroundColor = TextColor.Green; break;
                case 0x30: this.backgroundColor = TextColor.Cyan; break;
                case 0x40: this.backgroundColor = TextColor.Red; break;
                case 0x50: this.backgroundColor = TextColor.Magenta; break;
                case 0x60: this.backgroundColor = TextColor.Yellow; break;
                case 0x70: this.backgroundColor = TextColor.White; break;
            }

            // The high bit has two possible meanings.  Under classic DOS
            // and CGA systems, the bit indicating the blinking status.
            // Under newer systems and the Windows console, the bit is
            // used to set high-intensity backgrounds.

            if ((cgaAttribute & 0x80) == 0x80)
            {
                switch (highBitMeaning)
                {
                    case CgaHighBit.Blink:
                        this.blinking = TextBlinking.Fast;
                        this.backgroundIntensity = TextIntensity.Normal;
                        break;

                    case CgaHighBit.BrightBackground:
                        this.blinking = TextBlinking.None;
                        this.backgroundIntensity = TextIntensity.Bold;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                this.backgroundIntensity = TextIntensity.Normal;
                this.blinking = TextBlinking.None;
            }

        }

        #endregion

        #region [ SetColor(byte, byte, CgaHighBit) ]

        /// <summary>
        ///     Sets the foreground and background colors based on
        ///     encoded CGA values.
        /// </summary>
        /// <param name="cgaForeground">
        ///     The foreground color encoded as a CGA attribute.
        /// </param>
        /// <param name="cgaBackground">
        ///     The background color encoded as a CGA attribute.
        /// </param>
        /// <param name="highBitMeaning">
        ///     The meaning of the high bit in the CGA background.
        /// </param>
        public void SetColor(
            byte cgaForeground,
            byte cgaBackground,
            CgaHighBit highBitMeaning)
        {
            SetColor((byte)(cgaForeground | (cgaBackground << 4)), highBitMeaning);
        }

        #endregion

        #region [ SetForeground(byte) ]

        /// <summary>
        ///     Sets the foreground and intensity based on
        ///     a CGA color attribute.
        /// </summary>
        /// <param name="cgaForeground">
        ///     The foreground and intensity based on a CGA
        ///     color attribute.
        /// </param>
        /// <remarks>
        ///     A CGA color attribute contains the fourground
        ///     color encoded as bits 0, 1 and 2 of the byte.
        ///     The foreground intensity is normal if bit 3 is
        ///     reset and bold if bit 4 is set.
        /// </remarks>
        public void SetForeground(byte cgaForeground)
        {

            switch (cgaForeground & 0x07)
            {
                case 0: this.foregroundColor = TextColor.Black; break;
                case 1: this.foregroundColor = TextColor.Blue; break;
                case 2: this.foregroundColor = TextColor.Green; break;
                case 3: this.foregroundColor = TextColor.Cyan; break;
                case 4: this.foregroundColor = TextColor.Red; break;
                case 5: this.foregroundColor = TextColor.Magenta; break;
                case 6: this.foregroundColor = TextColor.Yellow; break;
                case 7: this.foregroundColor = TextColor.White; break;
            }

            // The foreground intensity is encoded in bit 3.  Faint
            // intensity is not supported.  The text is bold/bright if
            // the bit is set, or normal otherwise.

            this.foregroundIntensity =
                ((cgaForeground & 0x08) == 0x08) ? TextIntensity.Bold : TextIntensity.Normal;

        }

        #endregion

        #region [ SetForeground(ConsoleColor) ]

        /// <summary>
        ///     Sets the foreground and intensity based on a
        ///     ConsoleColor value.
        /// </summary>
        /// <param name="consoleForeground">
        ///     The foreground and intensity encoded as a 
        ///     system ConsoleColor value.
        /// </param>
        public void SetForeground(ConsoleColor consoleForeground)
        {
            switch (consoleForeground)
            {
                case ConsoleColor.Black:
                    this.foregroundColor = TextColor.Black;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Blue:
                    this.foregroundColor = TextColor.Blue;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Cyan:
                    this.foregroundColor = TextColor.Cyan;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkBlue:
                    this.foregroundColor = TextColor.Blue;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkCyan:
                    this.foregroundColor = TextColor.Cyan;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkGray:
                    this.foregroundColor = TextColor.Black;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkGreen:
                    this.foregroundColor = TextColor.Green;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkMagenta:
                    this.foregroundColor = TextColor.Magenta;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkRed:
                    this.foregroundColor = TextColor.Red;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkYellow:
                    this.foregroundColor = TextColor.Yellow;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Gray:
                    this.foregroundColor = TextColor.White;
                    this.foregroundIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Green:
                    this.foregroundColor = TextColor.Green;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Magenta:
                    this.foregroundColor = TextColor.Magenta;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Red:
                    this.foregroundColor = TextColor.Red;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.White:
                    this.foregroundColor = TextColor.White;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Yellow:
                    this.foregroundColor = TextColor.Yellow;
                    this.foregroundIntensity = TextIntensity.Bold;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region [ SetForeground(TextColor) ]

        /// <summary>
        ///     Sets the foreground with changing other properties.
        /// </summary>
        /// <param name="foregroundColor">
        ///     The new foreground color.
        /// </param>
        /// <remarks>
        ///     The intensity of the foreground is not affected.
        /// </remarks>
        public void SetForeground(TextColor foregroundColor)
        {
            this.foregroundColor = foregroundColor;
        }

        #endregion

        #region [ StrikeThrough ]

        /// <summary>
        ///     The strikethrough appearance of the text.
        /// </summary>
        public bool StrikeThrough
        {
            get
            {
                return this.strikeThrough;
            }
            set
            {
                this.strikeThrough = value;
            }
        }

        #endregion

        #region [ ToCgaAttribute ]

        /// <summary>
        ///     Converts the style to a CGA/DOS encoded color attribute.
        /// </summary>
        /// <param name="highBitMeaning">
        ///     The intended meaning of the high bit.
        /// </param>
        /// <returns>
        ///     A CGA/DOS color attribute.
        /// </returns>
        public byte ToCgaAttribute(CgaHighBit highBitMeaning)
        {

            byte result;

            switch (this.foregroundColor)
            {
                case TextColor.Black:
                    result = 0;
                    break;

                case TextColor.Blue:
                    result = 1;	
                    break;

                case TextColor.Green:
                    result = 2;
                    break;

                case TextColor.Cyan:
                    result = 3;
                    break;

                case TextColor.Red:
                    result = 4;
                    break;

                case TextColor.Magenta:
                    result = 5;
                    break;

                case TextColor.Yellow:
                    result = 6;
                    break;

                case TextColor.White:
                    result = 7;
                    break;

                default:
                    throw new NotSupportedException();

            }

            // Only a bold (bright) foreground intensity is supported.
            // Faint intensities are not supported in CGA attributes.
            // Consider raising an exception?

            if (this.foregroundIntensity == TextIntensity.Bold)
                result |= 0x08;

            switch (this.backgroundColor)
            {

                case TextColor.Black:
                    break;

                case TextColor.Blue:
                    result |= 0x10;
                    break;

                case TextColor.Green:
                    result |= 0x20;
                    break;

                case TextColor.Cyan:
                    result |= 0x30;
                    break;

                case TextColor.Red:
                    result |= 0x40;
                    break;

                case TextColor.Magenta:
                    result |= 0x50;
                    break;

                case TextColor.Yellow:
                    result |= 0x60;
                    break;

                case TextColor.White:
                    result |= 0x70;
                    break;

                default:
                    throw new NotSupportedException();

            }

            // The high bit in a CGA/DOS color attribute has several
            // possible meanings, depending on the environment.

            switch (highBitMeaning)
            {
                case CgaHighBit.Blink:
                    if (this.blinking != TextBlinking.None)
                        result |= 0x80;
                    break;

                case CgaHighBit.BrightBackground:
                    if (this.backgroundIntensity == TextIntensity.Bold)
                        result |= 0x80;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return result;

        }

        #endregion

        #region [ Underline ]

        /// <summary>
        ///     The underline or underscore appearance of the text.
        /// </summary>
        public TextUnderline Underline
        {
            get
            {
                return this.underline;
            }
            set
            {
                this.underline = value;
            }
        }

        #endregion

        #region [ WithoutBlinking ]

        /// <summary>
        ///     Builds a copy of the current style without any blinking.
        /// </summary>
        /// <returns>
        ///     A <see cref="TextStyle"/> with the same display properties
        ///     as the current style but with blinking disabled.
        /// </returns>
        public TextStyle WithoutBlinking()
        {
            TextStyle style = new TextStyle();
            style.Blinking = TextBlinking.None;
            return style;
        }

        #endregion

    }
}

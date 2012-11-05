using System;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{
    [TestFixture(Description = "Tests for the TextStyle class.")]
    public class TextStyleTests
    {

        /// <summary>
        ///     Number of random samples to test.
        /// </summary>
        /// <remarks>
        ///     There are many thousands of different style combinations.
        ///     This makes it unreasonable to test a particular style
        ///     change against every possible combination.  Instead
        ///     some tests generate a random sample via the _randomStyle
        ///     function in a loop.  This constant defines the number of
        ///     samples to test.
        /// </remarks>
        public const int Sampling = 5000;


        // Functions beginning with an underscore are utility
        // functions used by the rest of the class.

        #region [ _assertDefaults ]

        private void _assertDefaults(TextStyle style)
        {

            Assert.AreEqual(
                TextColor.Black,
                style.BackgroundColor,
                "BackgroundColor is not default.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.BackgroundIntensity,
                "BackgroundIntensity is not default.");

            Assert.AreEqual(
                TextBlinking.None,
                style.Blinking,
                "Blinking is not default.");

            Assert.AreEqual(
                TextColor.White,
                style.ForegroundColor,
                "ForegroundColor is not default.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.ForegroundIntensity,
                "ForegroundIntensity is not default.");

            Assert.IsFalse(
                style.Hidden,
                "Hidden is not default.");

            Assert.IsFalse(
                style.Italic,
                "Italic is not default.");

            Assert.IsFalse(
                style.Reversed,
                "Reversed is not default.");

            Assert.IsFalse(
                style.StrikeThrough,
                "StrikeThrough is not default.");

            Assert.AreEqual(
                TextUnderline.None,
                style.Underline,
                "Underline is not default.");

        }

        #endregion

        #region [ _assertEquals ]

        public static void _assertEquals(TextStyle s1, TextStyle s2)
        {

            Assert.AreEqual(
                s1.BackgroundColor,
                s2.BackgroundColor,
                "BackgroundColor does not match.");

            Assert.AreEqual(
                s1.BackgroundIntensity,
                s2.BackgroundIntensity,
                "BackgroundIntensity does not match.");

            Assert.AreEqual(
                s1.Blinking,
                s2.Blinking,
                "Blinking does not match.");

            Assert.AreEqual(
                s1.ForegroundColor,
                s2.ForegroundColor,
                "ForegroundColor does not match.");

            Assert.AreEqual(
                s1.ForegroundIntensity,
                s2.ForegroundIntensity,
                "ForegroundIntensity does not match.");

            Assert.AreEqual(
                s1.Hidden,
                s2.Hidden,
                "Hidden does not match.");

            Assert.AreEqual(
                s1.Italic,
                s2.Italic,
                "Italic does not match.");

            Assert.AreEqual(
                s1.Reversed,
                s2.Reversed,
                "Reversed does not match.");

            Assert.AreEqual(
                s1.StrikeThrough,
                s2.StrikeThrough,
                "StrikeThrough does not match.");

            Assert.AreEqual(
                s1.Underline,
                s2.Underline,
                "Underline does not match.");

        }

        #endregion

        #region [ _getConsoleColorForeground ]

        private TextColor _getConsoleColorPrimary(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return TextColor.Black;

                case ConsoleColor.Blue:
                    return TextColor.Blue;

                case ConsoleColor.Cyan:
                    return TextColor.Cyan;

                case ConsoleColor.DarkBlue:
                    return TextColor.Blue;

                case ConsoleColor.DarkCyan:
                    return TextColor.Cyan;

                case ConsoleColor.DarkGray:
                    return TextColor.Black;

                case ConsoleColor.DarkGreen:
                    return TextColor.Green;

                case ConsoleColor.DarkMagenta:
                    return TextColor.Magenta;

                case ConsoleColor.DarkRed:
                    return TextColor.Red;

                case ConsoleColor.DarkYellow:
                    return TextColor.Yellow;

                case ConsoleColor.Gray:
                    return TextColor.White;

                case ConsoleColor.Green:
                    return TextColor.Green;

                case ConsoleColor.Magenta:
                    return TextColor.Magenta;

                case ConsoleColor.Red:
                    return TextColor.Red;

                case ConsoleColor.White:
                    return TextColor.White;

                case ConsoleColor.Yellow:
                    return TextColor.Yellow;

                default:
                    throw new NotSupportedException();

            }

        }

        #endregion

        #region [ _getConsoleColorIntensity ]

        private TextIntensity _getConsoleColorIntensity(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return TextIntensity.Normal;

                case ConsoleColor.Blue:
                    return TextIntensity.Bold;

                case ConsoleColor.Cyan:
                    return TextIntensity.Bold;

                case ConsoleColor.DarkBlue:
                    return TextIntensity.Normal;

                case ConsoleColor.DarkCyan:
                    return TextIntensity.Normal;

                case ConsoleColor.DarkGray:

                    // Dark gray is really bright (bold) black.
                    return TextIntensity.Bold;

                case ConsoleColor.DarkGreen:
                    return TextIntensity.Normal;

                case ConsoleColor.DarkMagenta:
                    return TextIntensity.Normal;

                case ConsoleColor.DarkRed:
                    return TextIntensity.Normal;

                case ConsoleColor.DarkYellow:
                    return TextIntensity.Normal;

                case ConsoleColor.Gray:
                    return TextIntensity.Normal;

                case ConsoleColor.Green:
                    return TextIntensity.Bold;

                case ConsoleColor.Magenta:
                    return TextIntensity.Bold;

                case ConsoleColor.Red:
                    return TextIntensity.Bold;

                case ConsoleColor.White:
                    return TextIntensity.Bold;

                case ConsoleColor.Yellow:
                    return TextIntensity.Bold;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        // The following functions test the various constructors.
        // The TextStyle class has many different constructors in
        // order to make the class easier to fit in inline code.

        #region [ Constructor_Defaults ]

        [Test(Description = "Checks for expected default values after creation.")]
        public void Constructor_Defaults()
        {

            TextStyle style = new TextStyle();

            _assertDefaults(style);

        }

        #endregion

        #region [ Constructor_ConsoleColor ]

        [Test]
        public void Constructor_ConsoleColor()
        {
            Constructor_ConsoleColor(ConsoleColor.Black);
            Constructor_ConsoleColor(ConsoleColor.Blue);
            Constructor_ConsoleColor(ConsoleColor.Cyan);
            Constructor_ConsoleColor(ConsoleColor.DarkBlue);
            Constructor_ConsoleColor(ConsoleColor.DarkCyan);
            Constructor_ConsoleColor(ConsoleColor.DarkGray);
            Constructor_ConsoleColor(ConsoleColor.DarkGreen);
            Constructor_ConsoleColor(ConsoleColor.DarkMagenta);
            Constructor_ConsoleColor(ConsoleColor.DarkRed);
            Constructor_ConsoleColor(ConsoleColor.DarkYellow);
            Constructor_ConsoleColor(ConsoleColor.Gray);
            Constructor_ConsoleColor(ConsoleColor.Green);
            Constructor_ConsoleColor(ConsoleColor.Magenta);
            Constructor_ConsoleColor(ConsoleColor.Red);
            Constructor_ConsoleColor(ConsoleColor.White);
            Constructor_ConsoleColor(ConsoleColor.Yellow);
        }

        private void Constructor_ConsoleColor(ConsoleColor color)
        {


            TextColor expectedColor;
            TextIntensity expectedIntensity;

            switch (color)
            {
                case ConsoleColor.Black:
                    expectedColor = TextColor.Black;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Blue:
                    expectedColor = TextColor.Blue;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Cyan:
                    expectedColor = TextColor.Cyan;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkBlue:
                    expectedColor = TextColor.Blue;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkCyan:
                    expectedColor = TextColor.Cyan;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkGray:

                    // This might be an odd one.  The terminal
                    // library treats bright (bold) black as
                    // a dark gray.

                    expectedColor = TextColor.Black;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.DarkGreen:
                    expectedColor = TextColor.Green;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkMagenta:
                    expectedColor = TextColor.Magenta;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkRed:
                    expectedColor = TextColor.Red;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.DarkYellow:
                    expectedColor = TextColor.Yellow;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Gray:
                    expectedColor = TextColor.White;
                    expectedIntensity = TextIntensity.Normal;
                    break;

                case ConsoleColor.Green:
                    expectedColor = TextColor.Green;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Magenta:
                    expectedColor = TextColor.Magenta;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Red:
                    expectedColor = TextColor.Red;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.White:
                    expectedColor = TextColor.White;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                case ConsoleColor.Yellow:
                    expectedColor = TextColor.Yellow;
                    expectedIntensity = TextIntensity.Bold;
                    break;

                default:
                    throw new NotSupportedException();

            }

            TextStyle style = new TextStyle(color);

            Assert.AreEqual(
                expectedColor,
                style.ForegroundColor,
                "The foreground color was not expected.");

            Assert.AreEqual(
                expectedIntensity,
                style.ForegroundIntensity,
                "The foreground intensity was not expected.");

        }

        #endregion

        #region [ Constructor_TextColor ]

        [Test(Description="Makes sure the foreground color is set in the constructor.")]
        public void Constructor_TextColor()
        {

            TextStyle style = new TextStyle(TextColor.Blue);

            Assert.AreEqual(
                TextColor.Blue,
                style.ForegroundColor,
                "The foreground color should have been set.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.ForegroundIntensity,
                "The foreground intensity should have defaulted to normal.");

            Assert.AreEqual(
                TextColor.Black,
                style.BackgroundColor,
                "The background color should have defaulted to black.");

        }

        #endregion

        #region [ Constructor_TextColor_TextColor ]

        [Test(Description="Make sure the fore and back colors are set.")]
        public void Constructor_TextColor_TextColor()
        {

            TextStyle style = new TextStyle(TextColor.Blue, TextColor.Red);

            Assert.AreEqual(
                TextColor.Blue,
                style.ForegroundColor,
                "The foreground color should have been set.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.ForegroundIntensity,
                "The foreground intensity should have defaulted to normal.");

            Assert.AreEqual(
                TextColor.Red,
                style.BackgroundColor,
                "The background color should have been set.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.BackgroundIntensity,
                "The background intensity should have defaulted to normal.");

        }

        #endregion

        #region [ Constructor_TextColor_TextIntensity ]

        [Test(Description="Makes sure the foreground color and intensity is set.")]
        public void Constructor_TextColor_TextIntensity()
        {

            TextStyle style = new TextStyle(TextColor.Red, TextIntensity.Bold);

            Assert.AreEqual(
                TextColor.Red,
                style.ForegroundColor,
                "The foreground color should have been set.");

            Assert.AreEqual(
                TextIntensity.Bold,
                style.ForegroundIntensity,
                "The foreground intensity should have been set.");

            Assert.AreEqual(
                TextColor.Black,
                style.BackgroundColor,
                "The background color should have defaulted to black.");

            Assert.AreEqual(
                TextIntensity.Normal,
                style.BackgroundIntensity,
                "The background should have defaulted to normal intensity.");

        }

        #endregion

        #region [ Constructor_TextStyle ]

        [Test(Description = "Tests the clone of a style via the constructor.")]
        public void Constructor_TextStyle()
        {

            // There are a many different possible styles.
            //
            //   BackgroundColor ............. 8
            //   BackgroundIntensity ......... 3
            //   Blinking .................... 3
            //   ForegroundColor ............. 8
            //   ForegroundIntensity ......... 3
            //   Hidden ...................... 2
            //   Italic ...................... 2
            //   Reversed .................... 2
            //   Strikethrough ............... 2
            //   Underline ................... 3
            //
            // Total combinations: 82,944
            //
            // Therefore a selection of random styles will be
            // used to test the constructor.

            for (int i = 0; i < Sampling; i++)
            {

                // Create a style with random properties.
                TextStyle randomStyle = RandomData.RandomStyle();

                // Create a new style, cloning it.
                TextStyle newStyle = new TextStyle(randomStyle);

                // Make sure they are equal; that is, all
                // properties were copied to the new style.

                _assertEquals(randomStyle, newStyle);

            }


        }

        #endregion

        #region [ Constructor_byte_CgaHighBit ]

        [Test]
        public void Constructor_byte_CgaHighBit()
        {

            // Loop through every possible CGA color attribute

            for (int cgaByte = 0; cgaByte <= 0xFF; cgaByte++)
            {

                TextStyle blinkStyle =
                    new TextStyle((byte)cgaByte, CgaHighBit.Blink);

                // The foreground is encoded in the lower four bits
                // of the CGA byte.  These match the values of the
                // ConsoleColor enumeration and can be casted directly.
                // The background is encoded in bits 6, 5, and 4 (the
                // high bit indicates blinking).  0x70 = 01110000.

                ConsoleColor consoleForeground = (ConsoleColor)(cgaByte & 0xF);
                ConsoleColor consoleBackground = (ConsoleColor)((cgaByte & 0x70) >> 4);

                Assert.AreEqual(
                    _getConsoleColorPrimary(consoleForeground),
                    blinkStyle.ForegroundColor,
                    "For Bit=Blinking: The foreground was not set correctly.");

                Assert.AreEqual(
                    _getConsoleColorIntensity(consoleForeground),
                    blinkStyle.ForegroundIntensity,
                    "For Bit=Blinking: The foreground intensity was not set correctly.");

                Assert.AreEqual(
                    _getConsoleColorPrimary(consoleBackground),
                    blinkStyle.BackgroundColor,
                    "For Bit=Blinking: The background was not set correctly.");

                // Since the high bit indicated blinking, the
                // intensity of the background is always normal.

                Assert.AreEqual(
                    TextIntensity.Normal,
                    blinkStyle.BackgroundIntensity);

                if ((cgaByte & 0x80) == 0x80)
                {
                    Assert.AreEqual(
                        TextBlinking.Fast,
                        blinkStyle.Blinking,
                        "For Bit=Blinking: The blinking should be fast.");
                }
                else
                {
                    Assert.AreEqual(
                        TextBlinking.None,
                        blinkStyle.Blinking,
                        "For Bit=Blinking: The blinking should be disabled.");
                }

                // Now re-create the style again, this time
                // indicating an intense background.

                TextStyle intenseStyle =
                    new TextStyle((byte)cgaByte, CgaHighBit.BrightBackground);

                Assert.AreEqual(
                    _getConsoleColorPrimary(consoleForeground),
                    intenseStyle.ForegroundColor);

                Assert.AreEqual(
                    _getConsoleColorIntensity(consoleForeground),
                    intenseStyle.ForegroundIntensity,
                    "For Bit=Bold: foreground not set correctly.");

                Assert.AreEqual(
                    _getConsoleColorPrimary(consoleBackground),
                    intenseStyle.BackgroundColor);

                // Since the high bit of the CGA attribute indicated an
                // intense background, the blinking is always false.

                Assert.AreEqual(
                    TextBlinking.None,
                    intenseStyle.Blinking,
                    "For Bit=Bold: The blinking should never be enabled.");

                if ((cgaByte & 0x80) == 0x80)
                {
                    Assert.AreEqual(
                        TextIntensity.Bold,
                        intenseStyle.BackgroundIntensity,
                        "For Bit=Bold: The background intensity should be bold.");
                }
                else
                {
                    Assert.AreEqual(
                        TextIntensity.Normal,
                        intenseStyle.BackgroundIntensity,
                        "For Bit=Bold: The background intensity should be normal.");
                }

            }

        }

        #endregion

        #region [ Constructor_byte_byte_CgaHighBit ]

        [Test]
        public void Constructor_byte_byte_CgaHighBit()
        {

            for (int foreByte = 0; foreByte <= 0xF; foreByte++)
                for (int backByte = 0; backByte <= 0xF; backByte++)
                {

                    // The lower four bits of a CGA attribute indicate
                    // the foreground intensity and color.  These
                    // match the enumerated values of System.ConsoleColor.

                    TextColor foreColor = _getConsoleColorPrimary((ConsoleColor)foreByte);
                    TextColor backColor = _getConsoleColorPrimary((ConsoleColor)backByte);

                    TextIntensity foreBold = 
                        _getConsoleColorIntensity((ConsoleColor)foreByte);

                    TextStyle blinkStyle = new TextStyle(
                        (byte)foreByte,
                        (byte)backByte,
                        CgaHighBit.Blink);

                    Assert.AreEqual(
                        foreColor,
                        blinkStyle.ForegroundColor,
                        "Bit=Blink; ForegroundColor not set correctly.");

                    Assert.AreEqual(
                        foreBold,
                        blinkStyle.ForegroundIntensity,
                        "Bit=Blink; ForegroundIntensity not set correctly.");

                    Assert.AreEqual(
                        backColor,
                        blinkStyle.BackgroundColor,
                        "Bit=Blink; BackgroundColor not set correctly.");

                    Assert.AreEqual(
                        TextIntensity.Normal,
                        blinkStyle.BackgroundIntensity,
                        "Bit=Blink; BackgroundIntensity should always be normal.");

                    if ((backByte & 0x8) == 0x8)
                    {

                        Assert.AreEqual(
                            TextBlinking.Fast,
                            blinkStyle.Blinking,
                            "Bit=Blink; Byte=" + backByte + ": Blinking should be fast.");
                    }
                    else
                    {
                        Assert.AreEqual(
                            TextBlinking.None,
                            blinkStyle.Blinking,
                            "Bit=Blink; Byte=" + backByte + ": Blinking should be none.");

                    }

                    // Redo all of the same checks, this time indicating
                    // that the high bit means an intense background.

                    TextStyle intenseStyle = new TextStyle(
                        (byte)foreByte,
                        (byte)backByte,
                        CgaHighBit.BrightBackground);

                    Assert.AreEqual(
                        foreColor,
                        intenseStyle.ForegroundColor,
                        "Bit=Bold; ForegroundColor not set correctly.");

                    Assert.AreEqual(
                        foreBold,
                        intenseStyle.ForegroundIntensity,
                        "Bit=Bold; ForegroundIntensity not set correctly.");

                    Assert.AreEqual(
                        backColor,
                        intenseStyle.BackgroundColor,
                        "Bit=Bold; BackgroundColor not set correctly.");

                    Assert.AreEqual(
                        TextBlinking.None,
                        intenseStyle.Blinking,
                        "Bit=Bold; BB=" + backByte + ": Blinking should always be off.");

                    if ((backByte & 0x8) == 0x8)
                    {
                        Assert.AreEqual(
                            TextIntensity.Bold,
                            intenseStyle.BackgroundIntensity,
                            "Bit=Bold;BB=" + backByte + ": Intensity should be on.");
                    }
                    else
                    {
                        Assert.AreEqual(
                            TextIntensity.Normal,
                            intenseStyle.BackgroundIntensity,
                            "Bit=Bold;BB=" + backByte + ": Intensity should be off.");
                    }

                }

        }

        #endregion

        // The function tests make sure that .Equals returns
        // false if all properties are identical except
        // for a single one.  This will hopefully find
        // cases where the Equals function is not checking
        // a particular property.

        #region [ Equals_Different_BackgroundColor ]

        [Test]
        public void Equals_Different_BackgroundColor()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.BackgroundColor == s2.BackgroundColor)
                s2.BackgroundColor = RandomData.RandomColor();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        #region [ Equals_Different_BackgroundIntensity ]

        [Test]
        public void Equals_Different_BackgroundIntensity()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.BackgroundIntensity == s2.BackgroundIntensity)
                s2.BackgroundIntensity = RandomData.RandomIntensity();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        #region [ Equals_Different_Blinking ]

        [Test]
        public void Equals_Different_Blinking()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.Blinking == s2.Blinking)
                s2.Blinking = RandomData.RandomBlinking();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        #region [ Equals_Different_ForegroundColor ]

        [Test]
        public void Equals_Different_ForegroundColor()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.ForegroundColor == s2.ForegroundColor)
                s2.ForegroundColor = RandomData.RandomColor();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        #region [ Equals_Different_ForegroundIntensity ]

        [Test]
        public void Equals_Different_ForegroundIntensity()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.ForegroundIntensity == s2.ForegroundIntensity)
                s2.ForegroundIntensity = RandomData.RandomIntensity();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        #region [ Equals_Different_Hidden ]

        [Test]
        public void Equals_Different_Hidden()
        {

            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(
                s1,
                s2);

            s2.Hidden = !s1.Hidden;

            Assert.AreNotEqual(
                s1,
                s2);

        }

        #endregion

        #region [ Equals_Different_Italic ]

        [Test]
        public void Equals_Different_Italic()
        {

            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(
                s1,
                s2);

            s2.Italic = !s1.Italic;

            Assert.AreNotEqual(
                s1,
                s2);

        }

        #endregion

        #region [ Equals_Different_Reversed ]

        [Test]
        public void Equals_Different_Reversed()
        {

            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(
                s1,
                s2);

            s2.Reversed = !s1.Reversed;

            Assert.AreNotEqual(
                s1,
                s2);

        }

        #endregion

        #region [ Equals_Different_StrikeThrough ]

        [Test]
        public void Equals_Different_StrikeThrough()
        {

            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(
                s1,
                s2);

            s2.StrikeThrough = !s1.StrikeThrough;

            Assert.AreNotEqual(
                s1,
                s2);

        }

        #endregion

        #region [ Equals_Different_Underline ]

        [Test]
        public void Equals_Different_Underline()
        {
            TextStyle s1 = RandomData.RandomStyle();
            TextStyle s2 = s1.Clone();

            Assert.AreEqual(s1, s2);

            while (s1.Underline == s2.Underline)
                s2.Underline = RandomData.RandomUnderline();

            Assert.AreNotEqual(s1, s2);

        }

        #endregion

        // The function functions make sure a property reads
        // and writes its value property (e.g. a value written
        // to a property also comes out).

        #region [ PropertyReadWrite_BackgroundColor ]

        [Test]
        public void PropertyReadWrite_BackgroundColor()
        {
            
            TextStyle style = new TextStyle();

            TextColor[] colors =
                (TextColor[])Enum.GetValues(typeof(TextColor));

            foreach (TextColor color in colors)
            {
                style.BackgroundColor = color;

                Assert.AreEqual(
                    color,
                    style.BackgroundColor);
            }

        }

        #endregion

        #region [ PropertyReadWrite_BackgroundIntensity ]

        [Test]
        public void PropertyReadWrite_BackgroundIntensity()
        {

            TextIntensity[] intensities =
                (TextIntensity[])Enum.GetValues(typeof(TextIntensity));

            TextStyle style = new TextStyle();

            foreach (TextIntensity intensity in intensities)
            {
                style.BackgroundIntensity = intensity;
                Assert.AreEqual(
                    intensity,
                    style.BackgroundIntensity);
            }

        }

        #endregion

        #region [ PropertyReadWrite_Blinking ]

        [Test]
        public void PropertyReadWrite_Blinking()
        {
            TextBlinking[] blinks =
                (TextBlinking[])Enum.GetValues(typeof(TextBlinking));

            TextStyle style = new TextStyle();

            foreach (TextBlinking blinking in blinks)
            {
                style.Blinking = blinking;
                Assert.AreEqual(
                    blinking,
                    style.Blinking);
            }

        }

        #endregion

        #region [ PropertyReadWrite_ForegroundColor ]

        [Test]
        public void PropertyReadWrite_ForegroundColor()
        {

            TextColor[] colors =
                (TextColor[])Enum.GetValues(typeof(TextColor));

            TextStyle style = new TextStyle();

            foreach (TextColor color in colors)
            {
                style.ForegroundColor = color;

                Assert.AreEqual(
                    color,
                    style.ForegroundColor);
            }

        }

        #endregion

        #region [ PropertyReadWrite_ForegroundIntensity ]

        [Test]
        public void PropertyReadWrite_ForegroundIntensity()
        {

            TextIntensity[] intensities =
                (TextIntensity[])Enum.GetValues(typeof(TextIntensity));

            TextStyle style = new TextStyle();

            foreach (TextIntensity intensity in intensities)
            {
                style.ForegroundIntensity = intensity;
                Assert.AreEqual(
                    intensity,
                    style.ForegroundIntensity);
            }

        }

        #endregion

        #region [ PropertyReadWrite_Hidden ]

        [Test]
        public void PropertyReadWrite_Hidden()
        {
            TextStyle style = new TextStyle();

            style.Hidden = true;
            Assert.IsTrue(style.Hidden);

            style.Hidden = false;
            Assert.IsFalse(style.Hidden);

        }

        #endregion

        #region [ PropertyReadWrite_Italic ]

        [Test]
        public void PropertyReadWrite_Italic()
        {
            TextStyle style = new TextStyle();

            style.Italic = true;
            Assert.IsTrue(style.Italic);

            style.Italic = false;
            Assert.IsFalse(style.Italic);

        }

        #endregion

        #region [ PropertyReadWrite_Reversed ]

        [Test]
        public void PropertyReadWrite_Reversed()
        {
            TextStyle style = new TextStyle();

            style.Reversed = true;
            Assert.IsTrue(style.Reversed);

            style.Reversed = false;
            Assert.IsFalse(style.Reversed);

        }

        #endregion

        #region [ PropertyReadWrite_StrikeThrough ]

        [Test]
        public void PropertyReadWrite_StrikeThrough()
        {
            TextStyle style = new TextStyle();

            style.StrikeThrough = true;
            Assert.IsTrue(style.StrikeThrough);

            style.StrikeThrough = false;
            Assert.IsFalse(style.StrikeThrough);

        }

        #endregion

        #region [ PropertyReadWrite_Underline ]

        [Test]
        public void PropertyReadWrite_Underline()
        {
            TextUnderline[] underlines =
                (TextUnderline[])Enum.GetValues(typeof(TextUnderline));

            TextStyle style = new TextStyle();

            foreach (TextUnderline underline in underlines)
            {
                style.Underline = underline;
                Assert.AreEqual(
                    underline,
                    style.Underline);
            }

        }

        #endregion

        // The following functions test the ApplyGraphicRenditions
        // function.  This function accepts an array of SGR parameters
        // from the X3.64 standard and applies them in order.

        #region [ ApplyGraphicRenditions_Background ]

        [Test]
        public void ApplyGraphicRenditions_Background()
        {

            // SGR 40 = Black
            // SGR 41 = Red
            // SGR 42 = Green
            // SGR 43 = Yellow
            // SGR 44 = Blue
            // SGR 45 = Magenta
            // SGR 46 = Cyan
            // SGR 47 = White

            int[] sgrs = new int[] { 40, 41, 42, 43, 44, 45, 46, 47 };

            // The following array translates each SGR into its
            // corresponding text color.

            TextColor[] colors = new TextColor[] 
            {
                TextColor.Black,
                TextColor.Red,
                TextColor.Green,
                TextColor.Yellow,
                TextColor.Blue,
                TextColor.Magenta,
                TextColor.Cyan,
                TextColor.White 
            };

            for (int i = 0; i < Sampling; i++)
            {

                // Select a random SGR to apply.

                int randomIndex = RandomData.Random.Next(0, 8);

                // Create a random style and apply it

                TextStyle style = RandomData.RandomStyle();

                style.ApplyGraphicRenditions(
                    new int[] { sgrs[randomIndex] });

                Assert.AreEqual(
                    colors[randomIndex],
                    style.BackgroundColor);

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Blinking ]

        [Test]
        public void ApplyGraphicRenditions_Blinking()
        {

            // SGR 05 = Slow
            // SGR 06 = Rapid
            // SGR 25 = Off

            for (int i = 0; i < Sampling; i++)
            {

                TextStyle style;

                // Test SGR 5 (slow blinking)

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 5 });
                Assert.AreEqual(
                    TextBlinking.Slow,
                    style.Blinking);

                // Test SGR 6 (rapid blinking)

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 6 });
                Assert.AreEqual(
                    TextBlinking.Fast,
                    style.Blinking);

                // Test SGR 25

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 25 });
                Assert.AreEqual(
                    TextBlinking.None,
                    style.Blinking);

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Foreground ]

        [Test]
        public void ApplyGraphicRenditions_Foreground()
        {

            // The following SGR parameters set the foreground:
            //
            //   SGR 30 = Black
            //   SGR 31 = Red
            //   SGR 32 = Green
            //   SGR 33 = Yellow
            //   SGR 34 = Blue
            //   SGR 35 = Magenta
            //   SGR 36 = Cyan
            //   SGR 37 = White
            
            int[] sgrs =
                new int[] { 30, 31, 32, 33, 34, 35, 36, 37 };

            // The following is an array that translates
            // each SGR into its corresponding color.

            TextColor[] colors = new TextColor[]
            {
                TextColor.Black,
                TextColor.Red,
                TextColor.Green,
                TextColor.Yellow,
                TextColor.Blue,
                TextColor.Magenta,
                TextColor.Cyan,
                TextColor.White 
            };

            for (int i = 0; i < Sampling; i++)
            {

                // Select one of the random SGR foreground parameters

                int randomIndex = RandomData.Random.Next(0, sgrs.Length);

                // Create a random style and apply it.

                TextStyle style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(
                    new int[] { sgrs[randomIndex] });

                // Make sure the selected foreground was applied.

                Assert.AreEqual(
                    colors[randomIndex],
                    style.ForegroundColor);

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_ForegroundIntensity ]

        [Test]
        public void ApplyGraphicRenditions_ForegroundIntensity()
        {

            // SGR 01 sets the foreground intensity to bold;
            // SGR 02 sets the foreground intensity to faint.
            // SGR 22 sets the foreground intensity to normal.

            for (int i = 0; i < Sampling; i++)
            {

                TextStyle style;

                // For this test, the other intensity parameters
                // are sent as well.  Only the last one should
                // take effect because the function applies the
                // parameters in array order.

                // Test SGR 1 

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 1 });

                Assert.AreEqual(
                    TextIntensity.Bold,
                    style.ForegroundIntensity,
                    "The foreground intensity should have set to bold.");

                // Test SGR 2

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 2 });

                Assert.AreEqual(
                    TextIntensity.Faint,
                    style.ForegroundIntensity,
                    "The foreground intensity should have been set to faint.");

                // Test SGR 22

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 22 });

                Assert.AreEqual(
                    TextIntensity.Normal,
                    style.ForegroundIntensity,
                    "The foreground intensity should have been set to normal.");

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Italic ]

        [Test]
        public void ApplyGraphicRenditions_Italic()
        {

            // SGR 03 = Italic On
            // SGR 23 = Italic Off

            for (int i = 0; i < Sampling; i++)
            {

                TextStyle style;

                // Apply SGR 3 (italic on)

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 3 });
                Assert.IsTrue(
                    style.Italic,
                    "Italic mode should be enabled.");

                // Apply SGR 23 (italic off)

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 23 });
                Assert.IsFalse(
                    style.Italic,
                    "Italic mode should be off.");

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Reset ]

        [Test]
        public void ApplyGraphicRenditions_Reset()
        {

            // An SGR of zero (0) causes all graphics options
            // to revert to their default states.  Apply this
            // value many, many times in hopes of catching
            // a case where it is not reseting a value.  There
            // are many different possible style combinations,
            // making it unreasonable to test the SGR 0 against
            // every one.

            for (int i = 0; i < Sampling; i++)
            {
                TextStyle style = RandomData.RandomStyle();

                style.ApplyGraphicRenditions(new int[] { 0 });

                _assertDefaults(style);
            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Strikethrough ]

        [Test]
        public void ApplyGraphicRenditions_Strikethrough()
        {

            // SGR  9 = Strikethrough On
            // SGR 29 = Strikethrough Off

            for (int i = 0; i < Sampling; i++)
            {

                TextStyle style;

                // Test SGR 9

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 9 });
                Assert.IsTrue(style.StrikeThrough);

                // Test SGR 29

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 29 });
                Assert.IsFalse(style.StrikeThrough);

            }

        }

        #endregion

        #region [ ApplyGraphicRenditions_Underline ]

        [Test]
        public void ApplyGraphicRenditions_Underline()
        {

            // SGR 04 = Underline Single
            // SGR 21 = Underline Double
            // SGR 24 = Underline None

            for (int i = 0; i < Sampling; i++)
            {

                TextStyle style;

                // Test SGR 04 = Single

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 04 });
                Assert.AreEqual(
                    TextUnderline.Single,
                    style.Underline,
                    "The underline should be single.");

                // Test SGR 21 = Double

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 21 });
                Assert.AreEqual(
                    TextUnderline.Double,
                    style.Underline,
                    "The underline should be double.");

                // Test SGR 24 = None

                style = RandomData.RandomStyle();
                style.ApplyGraphicRenditions(new int[] { 24 });
                Assert.AreEqual(
                    TextUnderline.None,
                    style.Underline,
                    "The underline should be none.");

            }

        }

        #endregion

        // The following functions test the various Clone
        // functions supported by the TextStyle class.  These
        // functions are used to quickly create a copy of
        // a style while optionally changing some properties.

        #region [ Clone ]

        [Test]
        public void Clone()
        {

            for (int i = 0; i < Sampling; i++)
            {
                TextStyle style = RandomData.RandomStyle();

                _assertEquals(
                    style,
                    style.Clone());
            }

        }

        #endregion

        #region [ CloneBlinking ]

        [Test]
        public void CloneBlinking()
        {

            for (int i = 0; i < Sampling; i++)
            {

                // Create a random style.

                TextStyle randomStyle = RandomData.RandomStyle();

                // Clone the style with blinking

                TextStyle blinkingStyle = randomStyle.CloneBlinking();

                Assert.AreEqual(
                    TextBlinking.Fast,
                    blinkingStyle.Blinking);

            }

        }

        #endregion

        #region [ CloneBolded ]

        [Test]
        public void CloneBolded()
        {

            for (int i = 0; i < Sampling; i++)
            {

                // Create a random style.

                TextStyle randomStyle = RandomData.RandomStyle();

                // Clone the style with foreground bolding

                TextStyle boldedStyle = randomStyle.CloneBolded();

                Assert.AreEqual(
                    TextIntensity.Bold,
                    boldedStyle.ForegroundIntensity);

            }

        }

        #endregion

    }
}

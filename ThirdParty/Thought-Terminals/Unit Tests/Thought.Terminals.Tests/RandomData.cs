
using System;
using System.Collections.Generic;
using System.Text;
using Thought.Terminals;

namespace Tests
{

    public static class RandomData
    {

        /// <summary>
        ///     Random number generator for use by tests.
        /// </summary>
        public static Random Random = new Random();

        #region [ RandomBool ]

        /// <summary>
        ///     Returns a random boolean (either true or false).
        /// </summary>
        public static bool RandomBool()
        {
            switch (RandomData.Random.Next(0, 2))
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default:
                    throw new InvalidOperationException();
            }

        }

        #endregion

        #region [ RandomBlinking ]

        /// <summary>
        ///     Returns a random blinking state.
        /// </summary>
        public static TextBlinking RandomBlinking()
        {
            TextBlinking[] blinks =
                (TextBlinking[])Enum.GetValues(typeof(TextBlinking));

            return blinks[RandomData.Random.Next(0, blinks.Length)];
        }

        #endregion

        #region [ RandomColor ]

        /// <summary>
        ///     Returns a random text color.
        /// </summary>
        public static TextColor RandomColor()
        {

            TextColor[] colors =
                (TextColor[])Enum.GetValues(typeof(TextColor));

            return colors[RandomData.Random.Next(0, colors.Length)];
        }

        #endregion

        #region [ RandomIntensity ]

        /// <summary>
        ///     Returns a random intensity.
        /// </summary>
        public static TextIntensity RandomIntensity()
        {
            TextIntensity[] intensities =
                (TextIntensity[])Enum.GetValues(typeof(TextIntensity));

            return intensities[RandomData.Random.Next(0, intensities.Length)];
        }

        #endregion

        #region [ RandomStyle ]

        public static TextStyle RandomStyle()
        {

            TextStyle randomStyle = new TextStyle();

            randomStyle.BackgroundColor = RandomColor();
            randomStyle.BackgroundIntensity = RandomIntensity();
            randomStyle.Blinking = RandomBlinking();
            randomStyle.ForegroundColor = RandomColor();
            randomStyle.ForegroundIntensity = RandomIntensity();
            randomStyle.Hidden = RandomBool();
            randomStyle.Italic = RandomBool();
            randomStyle.Reversed = RandomBool();
            randomStyle.StrikeThrough = RandomBool();
            randomStyle.Underline = RandomUnderline();

            return randomStyle;

        }

        #endregion

        #region [ RandomUnderline ]

        /// <summary>
        ///     Returns a random underlining state.
        /// </summary>
        /// <returns></returns>
        public static TextUnderline RandomUnderline()
        {

            TextUnderline[] underlines =
                (TextUnderline[])Enum.GetValues(typeof(TextUnderline));

            return underlines[RandomData.Random.Next(0, underlines.Length)];
        }

        #endregion

    }

}


using System;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{

    [TestFixture]
    public class TranslatorTests
    {

        #region [ HasRoom ]

        [Test]
        public void HasRoom()
        {

            const string CoreText = "Tchaikovsky";

            StubTranslator translator = new StubTranslator();

            // This function will loop through a hypothetical
            // control code that is every length between 1
            // and one greater than the length of the core text.

            for (int length = 1; length <= CoreText.Length + 1; length++)
            {

                // In addition to checking every control code length,
                // the code will also check every starting position.

                for (int index = 0; index < CoreText.Length; index++)
                {

                    // For this length and index (position), call
                    // the HasRoom function and save the result.

                    bool result = translator.ExposeHasRoom(
                        CoreText,
                        index,
                        length);

                    // Consider the following:
                    //
                    //   Text ........: ABCDEFGHIJ
                    //   Text Index ..: 0123456789
                    //   Text length .: 10
                    //   Code ........:       code
                    //   Code Index ..: 6
                    //   Code Length .: 4
                    //
                    // The code can fit in the text if the
                    // desired index + length is less than
                    // or equal to the length of the text.
                    //
                    // In the example above, if the index
                    // is incremented, index (7) + length (4)
                    // is greater than the text length.
                    // Therefore there is no room.
                    //
                    // If the code length is incremented instead,
                    // the index (6) + the length (5) is also
                    // create than the text length (meaning, no room).

                    if (index + length > CoreText.Length)
                    {
                        Assert.IsFalse(result);
                    }
                    else
                    {
                        Assert.IsTrue(result);
                    }

                }

            }

        }

        #endregion

        #region [ HasRoom_EmptyString ]

        [Test]
        public void HasRoom_EmptyString()
        {

            // There is never enough room in an empty string.

            StubTranslator translator = new StubTranslator();

            Assert.IsFalse(
                translator.ExposeHasRoom(string.Empty, 0, 1));

        }

        #endregion

        #region [ HasRoom_IndexLessThanZero ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HasRoom_IndexLessThanZero()
        {
            // A negative index parameter is always invalid
            // and should never occur in real code.  The function
            // raises an exception instead of returning false.

            StubTranslator translator = new StubTranslator();

            translator.ExposeHasRoom(
                "Ignored",
                -1,
                3);

        }

        #endregion

        #region [ HasRoom_MinimumLengthZero ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HasRoom_MinimumLengthZero()
        {

            // A zero minimum length is a programmer error.

            StubTranslator translator = new StubTranslator();

            translator.ExposeHasRoom(
                "Ignore",
                0,
                0);

        }

        #endregion

        #region [ HasRoom_MinimumLengthNegative ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HasRoom_MinimumLengthNegative()
        {

            // A negative minimum length is a programmer error.

            StubTranslator translator = new StubTranslator();

            translator.ExposeHasRoom(
                "Ignore",
                0,
                -1);

        }

        #endregion

        #region [ HasRoom_NullString ]

        [Test]
        public void HasRoom_NullString()
        {

            // The HasRoom function can accept a null
            // string, which means the string never has
            // room at all.  This behavior may change to
            // an ArgumentNullException in the future.

            StubTranslator translator = new StubTranslator();

            Assert.IsFalse(
                translator.ExposeHasRoom(null, 0, 1));
        }

        #endregion

        #region [ IsHexDigit ]

        [Test]
        public void IsHexDigit()
        {

            // The IsHexDigit function is a protected utility
            // class that indicates whether or not a character
            // appears to be a hexadecimal character.  This is
            // useful because many macro systems use hexadecimal
            // characters to specify color values.

            StubTranslator translator = new StubTranslator();

            IsHexDigit_Call(translator, '0', true);
            IsHexDigit_Call(translator, '1', true);
            IsHexDigit_Call(translator, '2', true);
            IsHexDigit_Call(translator, '3', true);
            IsHexDigit_Call(translator, '4', true);
            IsHexDigit_Call(translator, '5', true);
            IsHexDigit_Call(translator, '6', true);
            IsHexDigit_Call(translator, '7', true);
            IsHexDigit_Call(translator, '8', true);
            IsHexDigit_Call(translator, '9', true);
            IsHexDigit_Call(translator, 'A', true);
            IsHexDigit_Call(translator, 'B', true);
            IsHexDigit_Call(translator, 'C', true);
            IsHexDigit_Call(translator, 'D', true);
            IsHexDigit_Call(translator, 'E', true);
            IsHexDigit_Call(translator, 'F', true);
            IsHexDigit_Call(translator, 'a', true);
            IsHexDigit_Call(translator, 'b', true);
            IsHexDigit_Call(translator, 'c', true);
            IsHexDigit_Call(translator, 'd', true);
            IsHexDigit_Call(translator, 'e', true);
            IsHexDigit_Call(translator, 'f', true);

        }


        private void IsHexDigit_Call(
            StubTranslator translator, 
            char value,
            bool expectedHex)
        {

            bool result = translator.ExposeIsHexDigit(value);

            if (expectedHex)
            {

                Assert.IsTrue(
                    result,
                    "The character " + value + " is a hexadecimal character.");

            }
            else
            {

                Assert.IsFalse(
                    result,
                    "The character " + value + " is not a hexadecimal character.");

            }


        }

        #endregion

        #region [ ParseHexChar ]

        [Test]
        public void ParseHexChar()
        {

            StubTranslator translator = new StubTranslator();

            ParseHexChar_Call(translator, '0', 0x0);
            ParseHexChar_Call(translator, '1', 0x1);
            ParseHexChar_Call(translator, '2', 0x2);
            ParseHexChar_Call(translator, '3', 0x3);
            ParseHexChar_Call(translator, '4', 0x4);
            ParseHexChar_Call(translator, '5', 0x5);
            ParseHexChar_Call(translator, '6', 0x6);
            ParseHexChar_Call(translator, '7', 0x7);
            ParseHexChar_Call(translator, '8', 0x8);
            ParseHexChar_Call(translator, 'A', 0xA);
            ParseHexChar_Call(translator, 'B', 0xB);
            ParseHexChar_Call(translator, 'C', 0xC);
            ParseHexChar_Call(translator, 'D', 0xD);
            ParseHexChar_Call(translator, 'E', 0xE);
            ParseHexChar_Call(translator, 'F', 0xF);
            ParseHexChar_Call(translator, 'a', 0xA);
            ParseHexChar_Call(translator, 'b', 0xB);
            ParseHexChar_Call(translator, 'c', 0xC);
            ParseHexChar_Call(translator, 'd', 0xD);
            ParseHexChar_Call(translator, 'e', 0xE);
            ParseHexChar_Call(translator, 'f', 0xF);

        }

        private void ParseHexChar_Call(
            StubTranslator translator,
            char value,
            int expectedValue)
        {

            int result = translator.ExposeParseHexChar(value);

            Assert.AreEqual(
                result,
                expectedValue,
                "The character '" + value + "' should have parsed.");
        }

        #endregion

        #region [ ValidateParameters_NullTerminal ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ValidateParameters_NullTerminal()
        {
            StubTranslator translator = new StubTranslator();

            translator.ExposeValidateParameters(
                null,
                "IgnoredString",
                0);

            
        }

        #endregion

    }
}

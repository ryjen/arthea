
using System;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{
    [TestFixture]
    public class TerminalTests
    {

        #region [ NewLine_Default ]

        [Test]
        public void NewLine_Default()
        {
            StubTerminal terminal = new StubTerminal();

            Assert.AreEqual(
                Environment.NewLine,
                terminal.NewLine,
                "The default NewLine should be Environment.NewLine.");

        }

        #endregion

        #region [ NewLine_EmptyAssignment ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NewLine_EmptyAssignment()
        {
            StubTerminal terminal = new StubTerminal();
            terminal.NewLine = string.Empty;
        }

        #endregion

        #region [ NewLine_NullAssignment ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NewLine_NullAssignment()
        {
            StubTerminal terminal = new StubTerminal();
            terminal.NewLine = null;
        }

        #endregion

        #region [ NewLine_ReadWrite ]

        [Test]
        public void NewLine_ReadWrite()
        {
            StubTerminal terminal = new StubTerminal();

            terminal.NewLine = "ABC";

            Assert.AreEqual(
                "ABC",
                terminal.NewLine);

        }

        #endregion

        #region [ PopStyle_EmptyStack ]

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void PopStyle_EmptyStack()
        {

            // No colors have been defined.  Therefore popping
            // a style will result in an exception.

            StubTerminal terminal = new StubTerminal();
            terminal.PopStyle();

        }

        #endregion

        #region [ PopStyle_PopUntilEmpty ]

        [Test]
        public void PopStyle_PopUntilEmpty()
        {

            StubTerminal terminal = new StubTerminal();

            terminal.PushStyle(CommonStyles.GreenOnBlack());
            terminal.PushStyle(CommonStyles.RedOnBlack());
            terminal.PushStyle(CommonStyles.CyanOnBlack());

            terminal.PopStyle();
            terminal.PopStyle();
            terminal.PopStyle();

            try
            {
                terminal.PopStyle();
                Assert.Fail("The pop should have failed.");
            }
            catch (InvalidOperationException)
            {
                // eat
            }
        }

        #endregion

        #region [ PushStyle ]

        [Test]
        public void PushStyle()
        {

            StubTerminal terminal = new StubTerminal();

            // Save the current style.

            TextStyle originalStyle = terminal.GetStyle();

            // Push the current style and set it to something different.

            terminal.PushStyle(CommonStyles.RedOnBlack());

            Assert.AreNotEqual(originalStyle, terminal.GetStyle());

            // Pop the style.

            terminal.PopStyle();

            Assert.AreEqual(originalStyle, terminal.GetStyle());

        }

        #endregion

        #region [ PromptEnter ]

        [Test]
        public void PromptEnter()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');
            terminal.Input.Enqueue(AsciiChar.CR);

            // Wait for the user to press the enter (carriage-return)
            // key.  All characters up the CR are ignored.

            terminal.PromptEnter();

            // No prompt has been defined.  Therefore no output
            // should have occured.

            Assert.AreEqual(0, terminal.Output.Length);

            // The CR was the last character in the input
            // buffer, so it should now be empty.

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ PromptEnter_DefaultPausePrompt ]

        [Test]
        public void PromptEnter_DefaultPromptPrompt()
        {
            StubTerminal terminal = new StubTerminal("123" + AsciiChar.CR);
            terminal.DefaultPausePrompt = "ABC";

            // Wait for the enter key to be pressed.

            terminal.PromptEnter(true);

            Assert.AreEqual(
                "ABC",
                terminal.Output.ToString());

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ PromptEnter_DefaultPausePrompt_Empty ]

        [Test]
        public void PromptEnter_DefaultPromptPrompt_Empty()
        {
            StubTerminal terminal = new StubTerminal("123" + AsciiChar.CR);
            terminal.DefaultPausePrompt = string.Empty;

            // Wait for the enter key to be pressed.

            terminal.PromptEnter(true);

            Assert.AreEqual(0, terminal.Output.Length);
            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ PromptEnter_DefaultPausePrompt_Null ]

        [Test]
        public void PromptEnter_DefaultPromptPrompt_Null()
        {
            StubTerminal terminal = new StubTerminal("123" + AsciiChar.CR);
            terminal.DefaultPausePrompt = null;

            // Wait for the enter key to be pressed.

            terminal.PromptEnter(true);

            Assert.AreEqual(0, terminal.Output.Length);
            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ PromptEnter_EmptyString ]

        [Test]
        public void PromptEnter_EmptyString()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');
            terminal.Input.Enqueue(AsciiChar.CR);

            // Wait for the user to press the enter (carriage-return)
            // key.  All characters up the CR are ignored.

            terminal.PromptEnter(string.Empty);

            // The specified prompt was empty.  This is valid, but
            // means nothing it outputed.

            Assert.AreEqual(0, terminal.Output.Length);

            // The CR was the last character in the input
            // buffer, so it should now be empty.

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ PromptEnter_NullString ]

        [Test]
        public void PromptEnter_NullString()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');
            terminal.Input.Enqueue(AsciiChar.CR);

            // Wait for the user to press the enter (carriage-return)
            // key.  All characters up the CR are ignored.

            terminal.PromptEnter((string)null);

            // The specified prompt was null.  This is valid, but
            // means nothing it outputed.

            Assert.AreEqual(0, terminal.Output.Length);

            // The CR was the last character in the input
            // buffer, so it should now be empty.

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ ReadChar ]

        [Test]
        public void ReadChar()
        {

            // The base terminal class does not implement
            // the ReadChar method (the child class is expected
            // to do the work).  This test makes sure the
            // StubTerminal.ReadChar method is functional.

            StubTerminal terminal = new StubTerminal();
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');

            char a = terminal.ReadChar();
            char b = terminal.ReadChar();
            char c = terminal.ReadChar();

            Assert.AreEqual('A', a);
            Assert.AreEqual('B', b);
            Assert.AreEqual('C', c);

            // There is no more sample input data.
            // An exception should be raised.
            // Do not put an ExpectedException attribute
            // on this test because one of the earlier
            // calls might incorrectly raise it, which
            // means the test will be "passed".

            try
            {
                char bad = terminal.ReadChar();

                Assert.Fail("No input data is left.");

            }
            catch (InvalidOperationException)
            {
                // Eat
            }

        }

        #endregion

        #region [ ReadChar_validChars_MultipleChars ]

        [Test]
        public void ReadChar_validChars_MultipleChars()
        {

            StubTerminal terminal = new StubTerminal();

            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');

            char c = terminal.ReadChar(new char[] { 'F', 'C', 'N' });

            Assert.AreEqual(c, 'C');
            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ ReadChar_validChars_SingleChar ]

        [Test]
        public void ReadChar_validChars_SingleChar()
        {
            StubTerminal terminal = new StubTerminal();

            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');

            char c = terminal.ReadChar(new char[] { 'C' });

            Assert.AreEqual(
                'C',
                c);

            // Since the 'C' was the last character in
            // the input queue, the other characters should
            // have been dropped.

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ ReadChar_validChars_Null ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ReadChar_validChars_Null()
        {
            Terminal terminal = new StubTerminal();

            char bad = terminal.ReadChar((char[])null);

        }

        #endregion

        #region [ ReadChar_validChars_Bool_MultipleCharsAndIgnoreCase ]

        [Test]
        public void ReadChar_validChars_Bool_MultipleCharsAndIgnoreCase()
        {
            StubTerminal terminal = new StubTerminal();

            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('c');

            char c = terminal.ReadChar(new char[] { 'F', 'C', 'N' }, true);

            Assert.AreEqual(c, 'c');
            Assert.AreEqual(0, terminal.Input.Count);
        }


        #endregion

        #region [ ReadChar_validCharsString ]

        [Test]
        public void ReadChar_validCharsString()
        {

            StubTerminal terminal = new StubTerminal();

            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('C');

            char c = terminal.ReadChar("FCN");

            Assert.AreEqual(c, 'C');
            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ ReadChar_validCharsString_Empty ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ReadChar_validCharsString_Empty()
        {
            StubTerminal terminal = new StubTerminal();

            char bad = terminal.ReadChar(string.Empty);
        }

        #endregion

        #region [ ReadChar_validCharsString_Empty_Bool ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ReadChar_validCharsString_Empty_Bool()
        {
            StubTerminal terminal = new StubTerminal();

            char bad = terminal.ReadChar(string.Empty, true);
        }

        #endregion

        #region [ ReadChar_validCharsString_Null ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ReadChar_validCharsString_Null()
        {
            StubTerminal terminal = new StubTerminal();

            char bad = terminal.ReadChar((string)null);
        }

        #endregion

        #region [ ReadChar_validCharsString_Null_Bool ]

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ReadChar_validCharsString_Null_Bool()
        {
            StubTerminal terminal = new StubTerminal();

            char bad = terminal.ReadChar((string)null, true);
        }

        #endregion

        #region [ ReadDigit ]

        [Test]
        public void ReadDigit()
        {

            StubTerminal terminal = new StubTerminal();

            // Dump some characters into the input queue.
            // The digits are inputted in ascending order.

            terminal.Input.Enqueue('0');
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('1');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('2');
            terminal.Input.Enqueue('C');
            terminal.Input.Enqueue('3');
            terminal.Input.Enqueue('D');
            terminal.Input.Enqueue('4');
            terminal.Input.Enqueue('5');
            terminal.Input.Enqueue('6');
            terminal.Input.Enqueue('E');
            terminal.Input.Enqueue('7');
            terminal.Input.Enqueue('8');
            terminal.Input.Enqueue('F');
            terminal.Input.Enqueue('9');

            // Read the digits.  The non-digit
            // characters should be ignored by the
            // ReadDigit function.

            char d0 = terminal.ReadDigit();
            char d1 = terminal.ReadDigit();
            char d2 = terminal.ReadDigit();
            char d3 = terminal.ReadDigit();
            char d4 = terminal.ReadDigit();
            char d5 = terminal.ReadDigit();
            char d6 = terminal.ReadDigit();
            char d7 = terminal.ReadDigit();
            char d8 = terminal.ReadDigit();
            char d9 = terminal.ReadDigit();

            Assert.AreEqual('0', d0);
            Assert.AreEqual('1', d1);
            Assert.AreEqual('2', d2);
            Assert.AreEqual('3', d3);
            Assert.AreEqual('4', d4);
            Assert.AreEqual('5', d5);
            Assert.AreEqual('6', d6);
            Assert.AreEqual('7', d7);
            Assert.AreEqual('8', d8);
            Assert.AreEqual('9', d9);

            // No characters should be left in the input queue.

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ ReadLetter ]

        [Test]
        public void ReadLetter()
        {

            StubTerminal terminal = new StubTerminal();

            // Dump some characters into the input queue.
            // The letters are inputted in ascending order.

            terminal.Input.Enqueue('0');
            terminal.Input.Enqueue('A');
            terminal.Input.Enqueue('1');
            terminal.Input.Enqueue('B');
            terminal.Input.Enqueue('2');
            terminal.Input.Enqueue('C');
            terminal.Input.Enqueue('3');
            terminal.Input.Enqueue('D');
            terminal.Input.Enqueue('4');
            terminal.Input.Enqueue('5');
            terminal.Input.Enqueue('6');
            terminal.Input.Enqueue('E');
            terminal.Input.Enqueue('7');
            terminal.Input.Enqueue('8');
            terminal.Input.Enqueue('F');

            char a = terminal.ReadLetter();
            char b = terminal.ReadLetter();
            char c = terminal.ReadLetter();
            char d = terminal.ReadLetter();
            char e = terminal.ReadLetter();
            char f = terminal.ReadLetter();

            Assert.AreEqual('A', a);
            Assert.AreEqual('B', b);
            Assert.AreEqual('C', c);
            Assert.AreEqual('D', d);
            Assert.AreEqual('E', e);
            Assert.AreEqual('F', f);

            Assert.AreEqual(0, terminal.Input.Count);

        }

        #endregion

        #region [ Write_Char_Int ]

        [Test]
        public void Write_Char_Int()
        {

            StubTerminal terminal = new StubTerminal();

            // Loop through a variety of different lengths,
            // starting at zero.  The Write(char,count) function
            // will be called for each length.

            for (int length = 0; length < 50; length++)
            {

                terminal.Write('A', length);

                Assert.AreEqual(
                    new string('A', length),
                    terminal.Output.ToString());

                terminal.Output.Length = 0;

            }

        }

        #endregion

        #region [ Write_Char_Int_Negative ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Write_Char_Int_Negative()
        {

            StubTerminal terminal = new StubTerminal();

            // Passing a negative value as a count is a programming
            // error and should result in an ArgumentOutOfRangeException

            terminal.Write('A', -1);

        }

        #endregion

        #region [ Write_Char_Int_Zero ]

        [Test]
        public void Write_Char_Int_Zero()
        {
            StubTerminal terminal = new StubTerminal();

            // Write(char, 0) is allowed and should not
            // result in an exception nor any output.  A
            // zero count is allowed because the function
            // might be called with calculated values.

            terminal.Write('A', 0);

            Assert.AreEqual(
                0,
                terminal.Output.Length);

        }

        #endregion

        #region [ Write_String_Empty ]

        [Test]
        public void Write_String_Empty()
        {

            // Writing an empty string should have no impact.

            StubTerminal terminal = new StubTerminal();

            terminal.Write(string.Empty);

            Assert.AreEqual(0, terminal.Output.Length);

        }

        #endregion

        #region [ Write_String_Null ]

        [Test]
        public void Write_String_Null()
        {
            StubTerminal terminal = new StubTerminal();

            terminal.Write((string)null);

            Assert.AreEqual(0, terminal.Output.Length);

        }

        #endregion

        #region [ WriteLine_AtypicalNewLine ]

        [Test]
        public void WriteLine_AtypicalNewLine()
        {

            // Create a terminal with a newline sequence
            // of "123".  This is guaranteed to never be
            // used for System.Environment.NewLine, and hence
            // will safely test that the WriteLine function is
            // outputing it.

            StubTerminal terminal = new StubTerminal();
            terminal.NewLine = "123";

            // Write a blank line.  Only the newline
            // sequence should be sent to the output.

            terminal.WriteLine();
            Assert.AreEqual("123", terminal.Output.ToString());

            // Clear the output buffer, then call WriteLine
            // with some text.

            terminal.Output.Length = 0;
            terminal.WriteLine("ABC");
            Assert.AreEqual("ABC123", terminal.Output.ToString());

            // Clear the output buffer and call WriteLine with
            // a null string.  This is intended to trap a case
            // where the developer might prematurely exit the function
            // if the parameter is null.

            terminal.Output.Length = 0;
            terminal.WriteLine((string)null);
            Assert.AreEqual("123", terminal.Output.ToString());

        }

        #endregion

        #region [ WriteLine_Empty ]

        [Test]
        public void WriteLine_Empty()
        {
            
            StubTerminal terminal = new StubTerminal();
            terminal.WriteLine(string.Empty);

            Assert.AreEqual(
                terminal.NewLine,
                terminal.Output.ToString());

        }

        #endregion

        #region [ WriteLine_Null ]

        [Test]
        public void WriteLine_Null()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WriteLine((string)null);

            Assert.AreEqual(
                terminal.NewLine,
                terminal.Output.ToString());

        }

        #endregion

        // The following functions test the WritePadded methods.
        //
        // WritePadded1 = WritePadded(value, length)
        // WritePadded2 = WritePadded(value, length, padding)
        // WritePadded3 = WritePadded(value, length, justify-right)
        // WritePadded4 = WritePadded(value, length, justify-center)

        #region [ WritePadded1_LongerLength ]

        [Test]
        public void WritePadded1_LongerLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 9);

            Assert.AreEqual(
                "ABCDEF   ",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded1_NegativeLength ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WritePadded1_NegativeLength()
        {
            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded(string.Empty, -1);
        }

        #endregion

        #region [ WritePadded1_SameLength ]

        [Test]
        public void WritePadded1_SameLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 6);

            Assert.AreEqual(
                "ABCDEF",
                terminal.Output.ToString(),
                "The desired length was the actual length.");

        }

        #endregion

        #region [ WritePadded1_ShorterLength ]

        [Test]
        public void WritePadded1_ShorterLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 3);

            Assert.AreEqual(
                "ABC",
                terminal.Output.ToString());

        }

        #endregion

        #region [ WritePadded1_ZeroLength ]

        [Test]
        public void WritePadded1_ZeroLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEFG", 0);

            Assert.AreEqual(
                0,
                terminal.Output.Length,
                "The desired length was zero so no output should have occured.");

        }

        #endregion

        #region [ WritePadded2_LongerLength ]

        [Test]
        public void WritePadded2_LongerLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 9, 'X');

            Assert.AreEqual(
                "ABCDEFXXX",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded2_NegativeLength ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WritePadded2_NegativeLength()
        {
            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded(string.Empty, -1, 'X');
        }

        #endregion

        #region [ WritePadded2_SameLength ]

        [Test]
        public void WritePadded2_SameLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 6, 'X');

            Assert.AreEqual(
                "ABCDEF",
                terminal.Output.ToString(),
                "The desired length was the actual length.");

        }

        #endregion

        #region [ WritePadded2_ShorterLength ]

        [Test]
        public void WritePadded2_ShorterLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 3, 'X');

            Assert.AreEqual(
                "ABC",
                terminal.Output.ToString());

        }

        #endregion

        #region [ WritePadded2_ZeroLength ]

        [Test]
        public void WritePadded2_ZeroLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEFG", 0, 'X');

            Assert.AreEqual(
                0,
                terminal.Output.Length);

        }

        #endregion

        #region [ WritePadded3_LongerLength ]

        [Test]
        public void WritePadded3_LongerLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 9, JustifyText.Right);

            Assert.AreEqual(
                "   ABCDEF",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded3_NegativeLength ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WritePadded3_NegativeLength()
        {
            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded(string.Empty, -1, JustifyText.Right);
        }

        #endregion

        #region [ WritePadded3_SameLength ]

        [Test]
        public void WritePadded3_SameLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 6, JustifyText.Right);

            Assert.AreEqual(
                "ABCDEF",
                terminal.Output.ToString());

        }

        #endregion

        #region [ WritePadded3_ShorterLength ]

        [Test]
        public void WritePadded3_ShorterLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 3, JustifyText.Right);

            Assert.AreEqual(
                "DEF",
                terminal.Output.ToString());

        }

        #endregion

        #region [ WritePadded3_ZeroLength ]

        [Test]
        public void WritePadded3_ZeroLength()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEFG", 0, JustifyText.Right);

            Assert.AreEqual(
                0,
                terminal.Output.Length);

        }

        #endregion

        #region [ WritePadded4_LongerLength_EvenPadding ]

        [Test]
        public void WritePadded4_LongerLength_EvenPadding()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 8, JustifyText.Center);

            Assert.AreEqual(
                " ABCDEF ",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded4_LongerLength_OddPadding ]

        [Test]
        public void WritePadded4_LongerLength_OddPadding()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 9, JustifyText.Center);

            Assert.AreEqual(
                " ABCDEF  ",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded4_ShorterLength_EvenClipping ]

        [Test]
        public void WritePadded4_ShorterLength_EvenClipping()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 4, JustifyText.Center);

            Assert.AreEqual(
                "BCDE",
                terminal.Output.ToString());
        }

        #endregion

        #region [ WritePadded4_ShorterLength_OddClipping ]

        [Test]
        public void WritePadded4_ShorterLength_OddClipping()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.WritePadded("ABCDEF", 1, JustifyText.Center);

            Assert.AreEqual(
                "C",
                terminal.Output.ToString());
        }

        #endregion

    }
}

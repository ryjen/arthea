
using System;
using System.Text;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{
    /// <summary>
    ///     Tests the AVATAR terminal protocol driver.
    /// </summary>
    /// <remarks>
    ///     Tests should be based on FSC-0025.001.
    /// </remarks>
    [TestFixture]
    public class AvatarTerminalProtocolTests
    {

        private string _repeatString(string text, int count)
        {
            if (count == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder result = new StringBuilder(text.Length * count);
                for (int i = 0; i < count; i++)
                {
                    result.Append(text);
                }
                return result.ToString();
            }
        }

        #region [ ClearRowEnding ]

        [Test]
        public void ClearRowEnding()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();

            Assert.AreEqual(
                new string(new char[] { AsciiControlChar.ControlV, AsciiControlChar.ControlG }),
                protocol.ClearRowEnding());

        }

        #endregion

        #region [ ClearScreen ]

        [Test]
        public void ClearScreen()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            Assert.AreEqual(
                AsciiControlChar.ControlL.ToString(),
                protocol.ClearScreen(),
                "The AVATAR code to clear the screen is a ^L.");

        }

        #endregion

        #region [ MoveCursor_NegativeColumn ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursor_NegativeColumn()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            protocol.MoveCursor(-1, 0);

        }

        #endregion

        #region [ MoveCursor_NegativeRow ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursor_NegativeRow()
        {
            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            protocol.MoveCursor(0, -1);
        }

        #endregion

        #region [ MoveCursorDown ]

        [Test]
        public void MoveCursorDown()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();

            for (int rows = 1; rows < 80; rows++)
            {

                // The AVATAR protocol does not have a parameter to
                // indicate the number of rows to move.

                string expected = _repeatString(
                    new string(new char[] { AsciiControlChar.ControlV, AsciiControlChar.ControlD }),
                    rows);

                string actual = protocol.MoveCursorDown(rows);

                Assert.AreEqual(expected, actual);
            }

        }

        #endregion

        #region [ MoveCursorDown_ZeroRows ]

        [Test]
        public void MoveCursorDown_ZeroRows()
        {
            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorDown(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

        #region [ MoveCursorLeft ]

        [Test]
        public void MoveCursorLeft()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();

            for (int columns = 1; columns < 80; columns++)
            {

                // The AVATAR protocol does not have a parameter to
                // indicate the number of columns to move.

                string expected = _repeatString(
                    new string(new char[] { AsciiControlChar.ControlV, AsciiControlChar.ControlE }),
                    columns);

                string actual = protocol.MoveCursorLeft(columns);

                Assert.AreEqual(expected, actual);
            }

        }

        #endregion

        #region [ MoveCursorLeft_ZeroColumns ]

        [Test]
        public void MoveCursorLeft_ZeroColumns()
        {
            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorLeft(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

        #region [ MoveCursorRight ]

        [Test]
        public void MoveCursorRight()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();

            for (int columns = 1; columns < 80; columns++)
            {

                // The AVATAR protocol does not have a parameter to
                // indicate the number of columns to move.

                string expected = _repeatString(
                    new string(new char[] { AsciiControlChar.ControlV, AsciiControlChar.ControlF }),
                    columns);

                string actual = protocol.MoveCursorRight(columns);

                Assert.AreEqual(expected, actual);
            }

        }

        #endregion

        #region [ MoveCursorRight_ZeroColumns ]

        [Test]
        public void MoveCursorRight_ZeroColumns()
        {
            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorRight(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

        #region [ MoveCursorUp ]

        [Test]
        public void MoveCursorUp()
        {

            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();

            for (int rows = 1; rows < 80; rows++)
            {

                // The AVATAR protocol does not have a parameter to
                // indicate the number of rows to move.

                string expected = _repeatString(
                    new string(new char[] {AsciiControlChar.ControlV, AsciiControlChar.ControlC}),
                    rows);

                string actual = protocol.MoveCursorUp(rows);

                Assert.AreEqual(expected, actual);
            }

        }

        #endregion

        #region [ MoveCursorUp_ZeroRows ]

        [Test]
        public void MoveCursorUp_ZeroRows()
        {
            AvatarTerminalProtocol protocol = new AvatarTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorRight(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

    }
}

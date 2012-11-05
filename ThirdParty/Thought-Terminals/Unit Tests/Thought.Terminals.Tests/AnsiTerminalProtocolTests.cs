
using System;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{

    /// <summary>
    ///     Tests the output of the AnsiTerminalProtocol class.  Note:
    ///     remember to examine external documentation when determining
    ///     whether a function is returning a correct string.
    /// </summary>
    [TestFixture(Description = "Tests of the AnsiTerminalProtocol class.")]
    public class AnsiTerminalProtocolTests
    {

        #region [ _assertParameterlessSequence ]

        public void _assertParameterlessSequence(
            string result,
            string csi,
            char code)
        {
            
            if (string.IsNullOrEmpty(result))
                throw new ArgumentNullException("result");

            if (string.IsNullOrEmpty(csi))
                throw new ArgumentNullException("csi");

            Assert.IsTrue(
                result.StartsWith(csi),
                "The result should have started with the CSI '" + csi + "'.");

            Assert.IsTrue(
                result.EndsWith(code.ToString()),
                "The result should have ended with the character '" + code + "'");

            Assert.AreEqual(
                csi + code,
                result,
                "There should be no parameter in the result.");

        }

        #endregion

        #region [ _assertSequence(..., int) ]

        public void _assertSequence(
            string result,
            string csi,
            char code,
            int parameter)
        {
            _assertSequence(
                result,
                csi,
                code,
                new int[] { parameter });
        }

        #endregion

        #region [ _assertSequence(..., int, int) ]

        public void _assertSequence(
            string result,
            string csi,
            char code,
            int parameter1,
            int parameter2)
        {
            _assertSequence(
                result,
                csi,
                code,
                new int[] { parameter1, parameter2 });

        }

        #endregion

        #region [ _assertSequence(..., int[]) ]

        public void _assertSequence(
            string result,
            string csi,
            char code,
            int[] parameters)
        {

            if (string.IsNullOrEmpty(result))
                throw new ArgumentNullException("result");

            if (string.IsNullOrEmpty(csi))
                throw new ArgumentNullException("csi");

            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (parameters.Length == 0)
                throw new ArgumentException();

            // All X3.64 sequences begin with a CSI (Control
            // Sequence Introducer).  This is a short string of
            // one or two characters that will indicate to the remote
            // terminal that a control sequence is beginning.

            Assert.IsTrue(
                result.StartsWith(csi),
                "The result should have started with the CSI '" + csi + "'.");

            // All X3.64 sequences end with a single character
            // code that identifies the type of command (e.g. an H
            // to move the cursor position).

            Assert.IsTrue(
                result.EndsWith(code.ToString()),
                "The result should have ended with the character '" + code + "'");

            // Get the characters between the CSI and the code.
            // This section is expected to contain parameters,
            // each separated with a semicolon.

            string middle = result.Substring(
                csi.Length,
                result.Length - csi.Length - 1);

            string[] strings =
                middle.Split(new char[] { ';' });

            Assert.AreEqual(
                parameters.Length,
                strings.Length,
                "The parameter list has a different count than expected.");

            for (int index = 0; index < parameters.Length; index++)
            {

                Assert.AreEqual(
                    parameters[index],
                    Convert.ToInt32(strings[index]),
                    "Parameter mismatch index=" + index);

            }

        }

        #endregion

        #region [ ClearRow ]

        [Test]
        public void ClearRow()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.ClearRow();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K',
                2);

        }

        #endregion

        #region [ ClearRow_DifferentCsi ]

        [Test]
        public void ClearRow_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearRow();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K',
                2);

        }

        #endregion

        #region [ ClearRowBeginning ]

        [Test]
        public void ClearRowBeginning()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.ClearRowBeginning();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K',
                1);

        }

        #endregion

        #region [ ClearRowBeginning_DifferentCsi ]

        [Test]
        public void ClearRowBeginning_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearRowBeginning();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K',
                1);

        }

        #endregion

        #region [ ClearRowEnding ]

        [Test]
        public void ClearRowEnding()
        {

            // This is sometimes called ClearEOL in old languages
            // like Pascal.  It clears the text between the cursor
            // and the end of the row.  It is generated with either
            // CSI 0 K or CSI K.  The test assumes CSI K will be
            // returned since that is optimal.

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            string result = protocol.ClearRowEnding();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K');

        }

        #endregion

        #region [ ClearRowEnding_DifferentCsi ]

        [Test]
        public void ClearRowEnding_DifferentCsi()
        {

            // This is the same test as ClearRowEnding()
            // except it initializes the protocol with 
            // a different control sequence introducer.

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearRowEnding();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'K');

        }

        #endregion

        #region [ ClearScreen ]

        [Test]
        public void ClearScreen()
        {

            // A screen is cleared with CSI 2 J.
            // On MS-DOS this also causes the cursor to go to the
            // home position in the upper left corner.

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.ClearScreen();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J',
                2);

        }

        #endregion

        #region [ ClearScreen_DifferentCsi ]

        [Test]
        public void ClearScreen_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearScreen();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J',
                2);

        }

        #endregion

        #region [ ClearScreenBeginning ]

        [Test]
        public void ClearScreenBeginning()
        {
            
            // The sequence CSI 1 J clears the text between
            // the start of the screen at the cursor.

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.ClearScreenBeginning();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J',
                1);

        }

        #endregion

        #region [ ClearScreenBeginning_DifferentCsi ]

        [Test]
        public void ClearScreenBeginning_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearScreenBeginning();

            _assertSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J',
                1);

        }

        #endregion

        #region [ ClearScreenEnding ]

        [Test]
        public void ClearScreenEnding()
        {

            // The sequence CSI 0 J (or CSI J) causes the text
            // to be cleared from the cursor to the end of the screen.
 
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.ClearScreenEnding();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J');

        }

        #endregion

        #region [ ClearScreenEnding_DifferentCsi ]

        [Test]
        public void ClearScreenEnding_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.ClearScreenEnding();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'J');

        }

        #endregion

        #region [ MoveCursor ]

        [Test]
        public void MoveCursor()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int column = 0; column < 80; column++)
            {

                for (int row = 0; row < 120; row++)
                {

                    string result = protocol.MoveCursor(column, row);

                    if ((column == 0) && (row == 0))
                    {

                        _assertParameterlessSequence(
                            result,
                            protocol.ControlSequenceIntroducer,
                            'H');
                    }
                    else
                    {
                        _assertSequence(
                            result,
                            protocol.ControlSequenceIntroducer,
                            'H',
                            row + 1,
                            column + 1);
                    }


                }

            }

        }

        #endregion

        #region [ MoveCursor_DifferentCsi ]

        [Test]
        public void MoveCursor_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            for (int column = 0; column < 80; column++)
            {

                for (int row = 0; row < 120; row++)
                {

                    string result = protocol.MoveCursor(column, row);

                    if ((column == 0) && (row == 0))
                    {

                        _assertParameterlessSequence(
                            result,
                            protocol.ControlSequenceIntroducer,
                            'H');
                    }
                    else
                    {
                        _assertSequence(
                            result,
                            protocol.ControlSequenceIntroducer,
                            'H',
                            row + 1,
                            column + 1);
                    }

                }

            }

        }

        #endregion

        #region [ MoveCursor_Column0_Row0 ]

        [Test]
        public void MoveCursor_Column0_Row0()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.MoveCursor(0, 0);

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'H');

        }

        #endregion

        #region [ MoveCursor_Column0_Row0_DifferentCsi ]

        [Test]
        public void MoveCursor_Column0_Row0_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.MoveCursor(0, 0);

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'H');

        }

        #endregion

        #region [ MoveCursor_NegativeColumn ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursor_NegativeColumn()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            protocol.MoveCursor(-1, 0);

        }

        #endregion

        #region [ MoveCursor_NegativeRow ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursor_NegativeRow()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            protocol.MoveCursor(0, -1);
        }

        #endregion

        #region [ MoveCursorDown ]

        [Test]
        public void MoveCursorDown()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int rows = 1; rows < 256; rows++)
            {

                string result = protocol.MoveCursorDown(rows);

                if (rows == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'B');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'B',
                        rows);

                }

            }

        }

        #endregion

        #region [ MoveCursorDown_DifferentCsi ]

        [Test]
        public void MoveCursorDown_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            for (int rows = 1; rows < 256; rows++)
            {

                string result = protocol.MoveCursorDown(rows);

                if (rows == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'B');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'B',
                        rows);
                }

            }

        }

        #endregion

        #region [ MoveCursorDown_NegativeRows ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursorDown_NegativeRows()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            protocol.MoveCursorDown(-1);
        }

        #endregion

        #region [ MoveCursorDown_ZeroRows ]

        [Test]
        public void MoveCursorDown_ZeroRows()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorDown(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

        #region [ MoveCursorHome ]

        [Test]
        public void MoveCursorHome()
        {

            // The sequence CSI H moves the cursor to the home
            // position.  This is a shortcut of the full CSI row column H
            // sequence.

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            string result = protocol.MoveCursorHome();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'H');

        }

        #endregion

        #region [ MoveCursorHome_DifferentCsi ]

        [Test]
        public void MoveCursorHome_DifferentCsi()
        {

            // The sequence CSI H moves the cursor to the home
            // position.  This is a shortcut of the full CSI row column H
            // sequence.

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            string result = protocol.MoveCursorHome();

            _assertParameterlessSequence(
                result,
                protocol.ControlSequenceIntroducer,
                'H');

        }

        #endregion

        #region [ MoveCursorLeft ]

        [Test]
        public void MoveCursorLeft()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int columns = 1; columns < 256; columns++)
            {

                string result = protocol.MoveCursorLeft(columns);

                if (columns == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'D');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'D',
                        columns);
                }

            }

        }

        #endregion

        #region [ MoveCursorLeft_DifferentCsi ]

        [Test]
        public void MoveCursorLeft_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            for (int columns = 1; columns < 256; columns++)
            {

                string result = protocol.MoveCursorLeft(columns);

                if (columns == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'D');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'D',
                        columns);

                }

            }

        }

        #endregion

        #region [ MoveCursorLeft_NegativeColumns ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursorLeft_NegativeColumns()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            protocol.MoveCursorLeft(-1);
        }

        #endregion

        #region [ MoveCursorLeft_ZeroColumns ]

        [Test]
        public void MoveCursorLeft_ZeroColumns()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
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

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int columns = 1; columns < 256; columns++)
            {

                string result = protocol.MoveCursorRight(columns);

                if (columns == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'C');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'C',
                        columns);
                }

            }

        }

        #endregion

        #region [ MoveCursorRight_DifferentCsi ]

        [Test]
        public void MoveCursorRight_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            for (int columns = 1; columns < 256; columns++)
            {

                string result = protocol.MoveCursorRight(columns);

                if (columns == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'C');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'C',
                        columns);
                }

            }

        }

        #endregion

        #region [ MoveCursorRight_NegativeColumns ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursorRight_NegativeColumns()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            protocol.MoveCursorRight(-1);
        }

        #endregion

        #region [ MoveCursorRight_ZeroColumns ]

        [Test]
        public void MoveCursorRight_ZeroColumns()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
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

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int rows = 1; rows < 256; rows++)
            {

                string result = protocol.MoveCursorUp(rows);

                if (rows == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'A');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'A',
                        rows);

                }

            }

        }

        #endregion

        #region [ MoveCursorUp_DifferentCsi ]

        [Test]
        public void MoveCursorUp_DifferentCsi()
        {

            AnsiTerminalProtocol protocol =
                new AnsiTerminalProtocol("\x9B");

            for (int rows = 1; rows < 256; rows++)
            {

                string result = protocol.MoveCursorUp(rows);

                if (rows == 1)
                {

                    _assertParameterlessSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'A');

                }
                else
                {
                    _assertSequence(
                        result,
                        protocol.ControlSequenceIntroducer,
                        'A',
                        rows);
                }

            }

        }

        #endregion

        #region [ MoveCursorUp_NegativeRows ]

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MoveCursorUp_NegativeRows()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            protocol.MoveCursorUp(-1);

        }

        #endregion

        #region [ MoveCursorUp_ZeroRows ]

        [Test]
        public void MoveCursorUp_ZeroRows()
        {
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();
            Assert.AreEqual(
                string.Empty,
                protocol.MoveCursorUp(0),
                "Since no cursor movement was requested, the result should be empty.");
        }

        #endregion

        #region [ SetStyle ]

        [Test]
        public void SetStyle_Fidelity()
        {

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int i = 0; i < 1000; i++)
            {

                TextStyle style = RandomData.RandomStyle();

                string result = protocol.SetStyle(null, style);

                Assert.IsNotEmpty(result);

            }

        }

        #endregion

    }
}

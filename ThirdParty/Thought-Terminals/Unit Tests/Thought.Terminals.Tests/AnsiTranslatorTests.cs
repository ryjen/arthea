
using System;
using NUnit.Framework;
using Thought.Terminals;
using Thought.Terminals.Translators;

namespace Tests
{

    /// <summary>
    ///     This class tests the capabilities of the ANSI translator.
    ///     This translator intercepts ANSI-BBS (X3.64) codes in the
    ///     early output stream and translates them to terminal
    ///     commands.
    /// </summary>
    [TestFixture]
    public class AnsiTranslatorTests
    {

        #region [ MoveCursor ]

        [Test]
        public void MoveCursor()
        {

            // Create a test terminal and add the ANSI
            // translator to it.

            StubTerminal terminal = new StubTerminal();
            terminal.Translators.Add(new AnsiTranslator());

            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for(int column = 0; column < 80; column++)
                for (int row = 0; row < 100; row++)
                {

                    terminal.Translate(
                        protocol.MoveCursor(column, row));

                    Assert.AreEqual(
                        column,
                        terminal.CurrentColumn,
                        "The column is incorrect; expected row=" + row);

                    Assert.AreEqual(
                        row,
                        terminal.CurrentRow,
                        "The row is incorrect; expected column=" + column);

                }

            Assert.AreEqual(
                80 * 100,
                terminal.CountCursorMovements);

        }

        #endregion

        #region [ MoveCursorDown ]

        [Test]
        public void MoveCursorDown()
        {

            // Create a new stub terminal and register the
            // ANSI translator with it.  Also create an
            // ANSI protocol class for creating escape sequences.

            StubTerminal terminal = new StubTerminal();
            terminal.Translators.Add(new AnsiTranslator());
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            for (int i = 0; i < 100; i++)
            {
                
                // Move the cursor downward by one row.

                terminal.Translate(
                    protocol.MoveCursorDown(1));

                Assert.AreEqual(
                    i + 1,
                    terminal.CurrentRow);

            }

            Assert.AreEqual(
                100,
                terminal.CountCursorMovements,
                "The number of cursor movements is invalid.");

        }

        #endregion

        #region [ MoveCursorHome ]

        [Test]
        public void MoveCursorHome()
        {

            // Create a new stub terminal and add the ANSI/X3.64
            // translator to it (which allows the terminal to
            // handle outbound X3.64 sequences).  Also create
            // an ANSI Protocol class to generate sample sequences.

            StubTerminal terminal = new StubTerminal();
            terminal.Translators.Add(new AnsiTranslator());
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            Random random = new Random();

            for (int tries = 0; tries < 1000; tries++)
            {

                // Move the cursor to a random position.

                terminal.MoveCursor(
                    random.Next(0, 100),
                    random.Next(0, 100));

                terminal.Translate(protocol.MoveCursorHome());

                Assert.AreEqual(
                    0,
                    terminal.CurrentColumn,
                    "The current column should be zero.");

                Assert.AreEqual(
                    0,
                    terminal.CurrentRow,
                    "The current row should be zero.");

            }

            Assert.AreEqual(
                0,
                terminal.Output.Length,
                "Only escape sequences were sent; text output should not happen.");

        }

        #endregion

        #region [ SetStyle ]

        [Test]
        public void SetStyle()
        {

            StubTerminal terminal = new StubTerminal();
            terminal.Translators.Add(new AnsiTranslator());
            AnsiTerminalProtocol protocol = new AnsiTerminalProtocol();

            // Create a thousand random styles, each time sending
            // the corresponding X3.64 code to the terminal and reading
            // back the resulting style.

            for (int samples = 0; samples < 1000; samples++)
            {

                // Generate a random style.  However, an intense
                // or faint background is not supported in X3.64
                // and will be set to normal for this test.

                TextStyle randomStyle = RandomData.RandomStyle();
                randomStyle.BackgroundIntensity = TextIntensity.Normal;

                // Get the full (non-optimized) X3.64 sequence that
                // corresponds to the random style.

                string randomSequence =
                    protocol.SetStyle(null, randomStyle);

                terminal.Translate(randomSequence);

                // The translator should have parsed the sequence
                // and eventually called SetStyle on the terminal.
                // In theory the colors should match.

                TextStyleTests._assertEquals(
                    randomStyle,
                    terminal.GetStyle());

            }

            Assert.AreEqual(
                1000,
                terminal.CountStyleChanges,
                "The style should have been changed each time in the loop.");

        }

        #endregion

    }
}

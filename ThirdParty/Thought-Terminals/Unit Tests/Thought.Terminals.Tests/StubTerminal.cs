
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Thought.Terminals;

namespace Tests
{

    /// <summary>
    ///     Implements a terminal with debugging functionality.
    /// </summary>
    /// <remarks>
    ///     This class inherits from the abstract Terminal class
    ///     in order to track statistics and simulate I/O.
    /// </remarks>
    public class StubTerminal : Terminal 
    {

        private int countCursorMovements;
        private int countStyleChanges;

        private int currentColumn;
        private int currentRow;

        private TextStyle currentStyle;
        private Queue<char> input = new Queue<char>();
        private StringBuilder output = new StringBuilder();


        public StubTerminal()
        {
        }

        #region [ StubTerminal(string) ]

        /// <summary>
        ///     Creates a new instance of the terminal with
        ///     the specified characters poked into the input
        ///     buffer.
        /// </summary>
        public StubTerminal(string injectedInput)
        {
            if (!string.IsNullOrEmpty(injectedInput))
            {
                foreach (char c in injectedInput)
                {
                    input.Enqueue(c);
                }
            }

        }

        #endregion

        #region [ CountCursorMovements ]

        /// <summary>
        ///     Returns the number of times any of the MoveCursor
        ///     functions have been called, including MoveCursorHome.
        /// </summary>
        public int CountCursorMovements
        {
            get
            {
                return this.countCursorMovements;
            }
        }

        #endregion

        #region [ CountStyleChanges ]

        /// <summary>
        ///     The number of times SetStyle has been called.
        /// </summary>
        public int CountStyleChanges
        {
            get
            {
                return this.countStyleChanges;
            }
        }

        #endregion

        #region [ CurrentColumn ]

        /// <summary>
        ///     The current cursor column.
        /// </summary>
        public int CurrentColumn
        {
            get
            {
                return this.currentColumn;
            }
        }

        #endregion

        #region [ CurrentRow ]

        /// <summary>
        ///     The current cursor row.
        /// </summary>
        public int CurrentRow
        {
            get
            {
                return this.currentRow;
            }
        }

        #endregion

        #region [ GetStyle ]

        /// <summary>
        ///     Returns the last style set by the caller via
        ///     a call to SetStyle.
        /// </summary>
        /// <remarks>
        ///     An inheritor of the Terminal class is required
        ///     to implement this method.  The caller is expected
        ///     to return a TextStyle object that represents the
        ///     current writing style.
        /// </remarks>
        public override TextStyle GetStyle()
        {
            if (this.currentStyle == null)
            {
                this.currentStyle = CommonStyles.DefaultDos();
            }

            return this.currentStyle.Clone();
        }

        #endregion

        public override void MoveCursor(int column, int row)
        {
            this.currentColumn = column;
            this.currentRow = row;
            this.countCursorMovements++;
        }

        public override void MoveCursorDown()
        {
            this.currentRow++;
            this.countCursorMovements++;
        }

        public override void MoveCursorDown(int rows)
        {
            this.currentRow++;
            this.countCursorMovements++;
        }

        public override void MoveCursorHome()
        {
            this.currentColumn = 0;
            this.currentRow = 0;
            this.countCursorMovements++;
        }

        public override void MoveCursorLeft()
        {
            this.currentColumn--;
            this.countCursorMovements++;
        }

        public override void MoveCursorLeft(int columns)
        {
            this.currentColumn--;
            this.countCursorMovements++;
        }

        public override void MoveCursorRight()
        {
            this.currentColumn++;
            this.countCursorMovements++;
        }

        public override void MoveCursorRight(int columns)
        {
            this.currentColumn++;
            this.countCursorMovements++;
        }

        public override void MoveCursorUp()
        {
            this.currentRow--;
            this.countCursorMovements++;
        }

        public override void MoveCursorUp(int rows)
        {
            this.currentRow--;
            this.countCursorMovements++;
        }

        public Queue<char> Input
        {
            get
            {
                return this.input;
            }
        }

        public StringBuilder Output
        {
            get
            {
                return output;
            }
        }

        #region [ ReadChar ]

        /// <summary>
        ///     Reads the next character from the input buffer.
        ///     The input buffer can manipulated with the input queue.
        /// </summary>
        /// <remarks>
        ///     An implementor of a terminal is required to override
        ///     the ReadChar method.  Since unit tests are not interactive,
        ///     this testing class simulates input with a character queue
        ///     that can be initialized by a test.
        /// </remarks>
        public override char ReadChar()
        {
            return this.input.Dequeue();
        }

        #endregion

        public override void SetStyle(TextStyle textStyle)
        {
            this.countStyleChanges++;
            this.currentStyle = textStyle.Clone();
        }

        public override void Write(char value)
        {
            output.Append(value);
        }

    }
}

using System.Text;

namespace ArtheaEngine
{

    /// <summary>
    /// Writable interface.
    /// </summary>
    public interface IWritable
    {
        #region Methods

        /// <summary>
        /// Page the specified buf.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        void Page(string buf);

        /// <summary>
        /// Page the specified buf.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        void Page(StringBuilder buf);

        /// <summary>
        /// Write the specified format and args.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='args'>
        /// Arguments.
        /// </param>
        void Write(string format, params object[] args);

        /// <summary>
        /// Write the specified value.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        void Write(byte[] value);

        /// <summary>
        /// Write the specified obj.
        /// </summary>
        /// <param name='obj'>
        /// Object.
        /// </param>
        void Write(object obj);

        /// <summary>
        /// Writes the line.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        void WriteLine(object value);

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name='format'>
        /// Format.
        /// </param>
        /// <param name='args'>
        /// Arguments.
        /// </param>
        void WriteLine(string format, params object[] args);

        #endregion Methods
    }
}
namespace ArtheaEngine
{
    using System.Text;

    public interface IWritable
    {
        #region Methods

        void Page(string buf);

        void Write(string format, params object[] args);

        void Write(byte[] value);

        void Write(object obj);

        void WriteLine();

        void WriteLine(object value);

        void WriteLine(string format, params object[] args);

        #endregion Methods
    }
}
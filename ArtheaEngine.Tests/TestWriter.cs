namespace ArtheaEngine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TestWriter : IWritable
    {
        #region Fields

        private StringBuilder _buf = new StringBuilder();

        #endregion Fields

        #region Methods

        public void Clear()
        {
            _buf.Clear();
        }

        public void Page(string buf)
        {
            _buf.Append(buf);
        }

        public override string ToString()
        {
            return _buf.ToString();
        }

        public void Write(string format, params object[] args)
        {
            _buf.AppendFormat(format, args);
        }

        public void Write(byte[] value)
        {
            _buf.Append(Encoding.UTF8.GetString(value));
        }

        public void Write(object obj)
        {
            _buf.Append(obj.ToString());
        }

        public void WriteLine()
        {
            _buf.AppendLine();
        }

        public void WriteLine(object value)
        {
            _buf.AppendLine(value.ToString());
        }

        public void WriteLine(string format, params object[] args)
        {
            _buf.AppendFormat(format, args).AppendLine();
        }

        #endregion Methods
    }
}
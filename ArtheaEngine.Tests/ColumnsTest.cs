namespace ArtheaEngine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class ColumnsTest
    {
        #region Fields

        private StringBuilder _buf;
        private ColumnFormatter _cols;
        private TestWriter _writer;

        #endregion Fields

        #region Methods

        [SetUp]
        public void Setup()
        {
            _writer = new TestWriter();
            _cols = new ColumnFormatter(2, 18);
            _buf = new StringBuilder();
        }

        [Test]
        public void ShowResizeTest()
        {
            _cols.SaveSize();

            _cols.SetSize(3);

            _cols.Add("aaa");
            _cols.Add("bbb");
            _cols.Add("ccc");
            _cols.Add("ddd");

            _buf.AppendLine("aaa   bbb   ccc   ");
            _buf.AppendLine("ddd   ");

            _writer.Clear();

			_writer.WriteLine(_cols.ToString());

            Assert.AreEqual(_buf.ToString(), _writer.ToString());

            _cols.RestoreSize();

            _writer.Clear();

            _cols.Add("test5");
            _cols.Add("test6");


			_writer.Write(_cols.ToString());

            _buf.AppendLine("test5    test6    ");

            Assert.AreEqual(_buf.ToString(), _writer.ToString());

            _cols.SetSize(3);

            _cols.Add("eee");
            _cols.Add("fff");
            _cols.Add("ggg");

            _writer.Clear();

			_writer.Write(_cols.ToString());

            _buf.AppendLine("eee   fff   ggg   ");

            Assert.AreEqual(_buf.ToString(), _writer.ToString());
        }

        [Test]
        public void ShowTest()
        {
            _cols.Add("test1");
            _cols.Add("test2");
            _cols.Add("test3");
            _cols.Add("test4");

			_writer.Write(_cols.ToString());

            _buf.AppendLine("test1    test2    ");
            _buf.AppendLine("test3    test4    ");

            Assert.AreEqual(_buf.ToString(), _writer.ToString());
        }

        #endregion Methods
    }
}
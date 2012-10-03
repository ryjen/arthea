using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ArtheaEngine.Tests
{

    [TestFixture]
    public class ExtensionsTest
    {
        #region Methods

        [Test]
        public void CamelToHuman()
        {
            var data = "ThisIsATest";

            var str = data.CamelToHuman();

            Assert.AreEqual("This Is A Test", str);
        }

        [Test]
        public void CapitalizeTest()
        {
            string str = "this IS a Test";

            string res = str.Capitalize();

            Assert.AreEqual(res, "This is a test");

            res = str.Capitalize(false);

            Assert.AreEqual(res, "This IS a Test");
        }

        [Test]
        public void CenterTest()
        {
            var str = "test";

            Assert.AreEqual(str.Center(10), "   test   ");
        }

        [Test]
        public void DeepCompareTest()
        {
            var test1 = new byte[] { 5, 4, 3, 2 };
            var test2 = new byte[] { 5, 4, 3, 2 };
            var test3 = new byte[] { 2, 3, 4, 5 };

            Assert.IsTrue(test1.DeepCompare(test2));
            Assert.IsFalse(test1.DeepCompare(test3));

            var test4 = new DeepCompareType[] { new DeepCompareType(5), new DeepCompareType(4), new DeepCompareType(3) };
            var test5 = new DeepCompareType[] { new DeepCompareType(5), new DeepCompareType(4), new DeepCompareType(3) };
            var test6 = new DeepCompareType[] { new DeepCompareType(3), new DeepCompareType(4), new DeepCompareType(3) };

            Assert.IsTrue(test4.DeepCompare(test5));
            Assert.IsFalse(test4.DeepCompare(test6));
        }

        [Test]
        public void DisplayLengthTest()
        {
            string str = "~Wtest~Ring~x";

            Assert.AreEqual(str.DisplayLength(), 7);

            str = "~!B~Wtest~Rtwo~x";

            Assert.AreEqual(str.DisplayLength(), 7);
        }

        [Test]
        public void GetLineCountLengthTest()
        {
            var str = string.Format("this{0}is{1}a{2}test", Environment.NewLine, Environment.NewLine, Environment.NewLine);

            int pos = str.GetLineCountLength(2);

            Assert.AreEqual(6+(Environment.NewLine.Length*2), pos);

            pos = str.GetLineCountLength(6);

            Assert.AreEqual(11+(Environment.NewLine.Length*3), pos);
        }

        [Test]
        public void GetLinesTest()
        {
            var str = string.Format("this{0}is{1}a{2}test", Environment.NewLine, Environment.NewLine, Environment.NewLine);

            var lines = str.GetLines();

            Assert.AreEqual(4, lines.Length);
        }

        [Test]
        public void GetMultiArgTest()
        {
            string temp;
            int count = "2*arg".GetMultiArg(out temp);

            Assert.AreEqual(2, count);
            Assert.AreEqual("arg", temp);
        }

        [Test]
        public void GetNumberedArgTest()
        {
            string temp;
            int count = "2.arg".GetNumberedArg(out temp);

            Assert.AreEqual(2, count);
            Assert.AreEqual("arg", temp);
        }

        [Test]
        public void GetPredicateTest()
        {
            var data = new Int16[] { 3, 5, 6, 7 };

            Assert.AreEqual(data.Get<Int16>(i => i == 3), 3);
            Assert.AreEqual(data.Get<Int16>(i => i == 10), 0);
        }

        [Test]
        public void HasWordTest()
        {
            string temp = "this is a test";

            Assert.IsTrue(temp.HasWord("TEST"));

            Assert.IsFalse(temp.HasWord("siht"));

            Assert.IsFalse(temp.HasWord("thi"));
        }

        [Test]
        public void IsEmailTest()
        {
            Assert.IsFalse(Game.IsEmail("coder78@"));
            Assert.IsTrue(Game.IsEmail("c0der78@gmail.com"));
            Assert.IsFalse(Game.IsEmail("@gmail.com"));
        }

        [Test]
        public void IsLineTest()
        {
            Assert.IsFalse("this is a test".IsLine());

            Assert.IsTrue(string.Format("This is a test{0}", Environment.NewLine).IsLine());
        }

        [Test]
        public void IsNumberTest()
        {
            Assert.IsTrue("20".IsNumber());
            Assert.IsTrue("20.2".IsNumber());
            Assert.IsTrue("-20.3".IsNumber());
            Assert.IsFalse("20arg30".IsNumber());
            Assert.IsFalse("-10-20".IsNumber());
            Assert.IsFalse("20.4.10".IsNumber());
        }

        [Test]
        public void IsPrefixOfTest()
        {
            var test1 = "test1";
            var test2 = "test";
            var test3 = "";
            var test4 = "1test";

            Assert.IsTrue(test2.IsPrefixOf(test1));
            Assert.IsFalse(test4.IsPrefixOf(test1));
            Assert.IsFalse(test3.IsPrefixOf(test1));
        }

        [Test]
        public void IsTest()
        {
            var str = "testA";

            Assert.IsTrue(str.Is("TESTa"));

            Assert.IsFalse(str.Is("Test"));
        }

        [Test]
        public void StripColorTest()
        {
            string data = "~WThisis~Ra~Gtest";

            var res = data.StripColor();

            Assert.AreEqual("Thisisatest", res);
        }

        [Test]
        public void ToCSVTest()
        {
            //var buf = new StringBuilder();

            var tags = new HashSet<string>();

            tags.Add("test1");
            tags.Add("test2");
            tags.Add("test3");

            Assert.AreEqual(tags.ToCSV(), "test1, test2, test3");
        }

        #endregion Methods

        #region Nested Types

        private class DeepCompareType : IEquatable<DeepCompareType>
        {
            #region Constructors

            public DeepCompareType(int id)
            {
                Id = id;
            }

            #endregion Constructors

            #region Properties

            public int Id
            {
                get; set;
            }

            #endregion Properties

            #region Methods

            public bool Equals(DeepCompareType other)
            {
                return Id == other.Id;
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ArtheaEngine.Tests
{

    [TestFixture]
    public class ColorTest
    {
        #region Methods

        [Test]
        public void FormatTest()
        {
            var c = new Color(ColorType.Green, ColorFlags.Bright);

            var data = c.Format();

            Assert.AreEqual("G", data);

            c.Value = ColorType.Blue;
            c.Flags = 0;

            data = c.Format();

            Assert.AreEqual("b", data);
        }

        [Test]
        public void ParseTest()
        {
            var data = "!Wthis~xisa~Rtest";
            int index = 0;

            var col = Color.Parse(data, ref index);

            Assert.AreEqual(1, index);

            Assert.AreEqual(ColorType.White, col.Value);
            Assert.IsTrue((col.Flags & ColorFlags.Bright) != 0);
        }

        #endregion Methods
    }
}
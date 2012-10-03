

using NUnit.Framework;

namespace ArtheaEngine.Tests
{

    [TestFixture]
    public class CurrencyTest
    {
        #region Methods

		[Test]
		public void ConstructorTest ()
		{
			var m = new Currency(53, 6432);

			Assert.AreEqual(643253, m.Value);

			Assert.AreEqual (3, m.Copper);

			Assert.AreEqual(53, m.Silver);

			Assert.AreEqual(6432, m.Gold);

			Assert.AreEqual(6, m.Platinum);

			m = new Currency(35.454);

			Assert.AreEqual(35, m.Gold);

			Assert.AreEqual(45, m.Silver);

			Assert.AreEqual(3545, m.Value);
		}

        [Test]
        public void AddTest()
        {
			var m = new Currency(50, 6543);

            m.Silver += 50;

            Assert.AreEqual(654400UL, m);

            m += new Currency(50, 1);

            Assert.AreEqual(m, 654550UL);

			Assert.AreNotEqual(140UL, m);

			m += new Currency(0, 200);

			Assert.AreEqual(674550UL, m);
        }

        [Test]
        public void CopperTest()
        {
            var m = new Currency(31, 1);

			Assert.AreEqual(1, m.Copper);

            m.Silver -= 1;

			Assert.AreEqual(0, m.Copper);

			m.Silver = 152;

			Assert.AreEqual(2, m.Copper);

			Assert.AreEqual(252, m.Value);
        }

		[Test]
		public void PlatinumTest ()
		{
			var m = new Currency(54, 4503);

			Assert.AreEqual (4, m.Platinum);

			m.Gold -= 600;

			Assert.AreEqual(3, m.Platinum);

		}

        [Test]
        public void DecrementTest()
        {
            var m = new Currency(50, 3);

			Assert.AreEqual(350, m--);

			Assert.AreNotEqual(350, --m);

			Assert.AreEqual(348, m);
        }

        [Test]
        public void DivideTest()
        {
            var m = new Currency(0, 4);

			m /= 2;

            Assert.AreEqual(200, m);
        }

        [Test]
        public void EqualsTest()
        {
            var m = new Currency(50, 3);

            Assert.IsTrue(m.Equals(350));

            Assert.IsFalse(m.Equals(530));
        }

        [Test]
        public void GoldTest()
        {
            var m = new Currency(54, 3);

            Assert.AreEqual(3, m.Gold);

        }

        [Test]
        public void IncrementTest()
        {
            var m = new Currency(50, 3);

			Assert.AreEqual(350, m++);

			Assert.AreEqual(352, ++m);

			Assert.AreNotEqual(249, m);
        }

        [Test]
        public void MultiplyTest()
        {
            var m = new Currency(50, 3);

            m *= 2;

            Assert.AreEqual(700, m.Value);
        }

        [Test]
        public void SilverTest()
        {
            var m = new Currency(54, 3);

            Assert.AreEqual(54, m.Silver);

            m -= new Currency(20, 0);

            Assert.AreEqual(34, m.Silver);

			m.Silver = 35;

			Assert.AreEqual(35, m.Silver);
        }

        [Test]
        public void SubtractTest()
        {
            var m = new Currency(50, 3);

            m -= new Currency(50, 0);

			Assert.AreEqual(300, m);

            m -= new Currency(50, 1);

            Assert.AreEqual(150, m.Value);

            Assert.AreNotEqual(250, m.Value);
        }

        [Test]
        public void ToStringTest()
        {
			var m = new Currency(54, 85453);

			Assert.AreEqual("$85,453.54", m.ToString());

			Assert.AreEqual("$0.04", m.ToString("C"));

			Assert.AreEqual("$0.54", m.ToString("S"));

			Assert.AreEqual("$85,453.54", m.ToString("G"));

			Assert.AreEqual ("$85.45", m.ToString("P"));
        }

        #endregion Methods
    }
}
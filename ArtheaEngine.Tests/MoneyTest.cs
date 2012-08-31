using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ArtheaEngine.Model;

using NUnit.Framework;

namespace ArtheaEngine.Tests
{

    [TestFixture]
    public class MoneyTest
    {
        #region Methods

        [Test]
        public void AddTest()
        {
            var m = new Money(3.5m);

            m += 0.5m;

            Assert.IsTrue(m == 4.0m);

            m += new Money(1.5m);

            Assert.IsTrue(m == new Money(5.5m));

            Assert.IsFalse(m == 1.4m);
        }

        [Test]
        public void CopperTest()
        {
            var m = new Money(-3.54m);

            Assert.AreEqual(m.Copper(), -4);

            m = -m;

            Assert.AreEqual(m.Copper(), 4);

            m -= 0.04m;

            Assert.AreEqual(m.Copper(), 0);
        }

        [Test]
        public void DecrementTest()
        {
            var m = new Money(3.5m);

            Assert.IsTrue(m-- == 3.5m);

            Assert.IsFalse(--m == 3.5m);

            Assert.IsTrue(m == 3.48m);
        }

        [Test]
        public void DivideTest()
        {
            var m = new Money(3.5m);

            m /= 1.5m;

            Assert.IsTrue(m == 2.33m);
        }

        [Test]
        public void EqualsTest()
        {
            var m = new Money(3.5m);

            Assert.IsTrue(m.Equals(3.5m));

            Assert.IsFalse(m.Equals(5.3m));
        }

        [Test]
        public void GoldTest()
        {
            var m = new Money(3.54m);

            Assert.AreEqual(m.Gold(), 3);

            m = -m;

            Assert.AreEqual(m.Gold(), -3);
        }

        [Test]
        public void IncrementTest()
        {
            var m = new Money(3.5m);

            Assert.IsTrue(m++ == 3.5m);

            Assert.IsTrue(++m == 3.52m);

            Assert.IsFalse(m == 2.49m);
        }

        [Test]
        public void MultiplyTest()
        {
            var m = new Money(3.5m);

            m *= 1.5m;

            Assert.IsTrue(m == 5.25m);
        }

        [Test]
        public void SilverTest()
        {
            var m = new Money(3.54m);

            Assert.AreEqual(m.Silver(), 54);

            m -= 0.5m;

            Assert.AreEqual(m.Silver(), 4);
        }

        [Test]
        public void SubtractTest()
        {
            var m = new Money(3.5m);

            m -= 0.5m;

            Assert.IsTrue(m == 3.0m);

            m -= new Money(1.5m);

            Assert.IsTrue(m == new Money(1.5m));

            Assert.IsFalse(m == 2.5m);
        }

        [Test]
        public void ToStringTest()
        {
            var m = new Money(3.54m);

            Assert.AreEqual(m.ToString(), "$3.54");

            Assert.AreEqual(m.ToString("C"), "$354");

            Assert.AreEqual(m.ToString("S"), "$35.4");

            Assert.AreEqual(m.ToString("G"), "$3.54");
        }

        [Test]
        public void ValueAsTest()
        {
            var m = new Money(3.54m);

            Assert.AreEqual(m.ValueAs(Currency.Gold), 3.54m);

            Assert.AreEqual(m.ValueAs(Currency.Silver), 35.4m);

            Assert.AreEqual(m.ValueAs(Currency.Copper), 354m);
        }

        #endregion Methods
    }
}
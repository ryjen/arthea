
using System;
using NUnit.Framework;
using Thought.Terminals;

namespace Tests
{
    [TestFixture]
    public class TranslatorCollectionTests
    {

        #region [ Operator_Collection_Plus_Collection ]

        [Test]
        public void Operator_Collection_Plus_Collection()
        {

            TranslatorCollection c1 = new TranslatorCollection();
            TranslatorCollection c2 = new TranslatorCollection();

            Translator c1_s1 = new StubTranslator();
            Translator c1_s2 = new StubTranslator();
            Translator c1_s3 = new StubTranslator();
            Translator c2_s1 = new StubTranslator();
            Translator c2_s2 = new StubTranslator();

            // Add the two empty collections together.
            // The result should be an empty collection.

            TranslatorCollection result1 = c1 + c2;
            Assert.IsEmpty(result1);

            // Add the stubs to the first collection,
            // then add together.  The result should
            // contain those three stubs.

            c1.Add(c1_s1);
            c1.Add(c1_s2);
            c1.Add(c1_s3);

            TranslatorCollection result2 = c1 + c2;
            Assert.AreEqual(
                3,
                result2.Count);

            // Clear the first collection, then add the
            // two stubs to the second collection.  The
            // result should contain those two stubs.

            c1.Clear();
            c2.Add(c2_s1);
            c2.Add(c2_s2);

            TranslatorCollection result3 = c1 + c2;
            Assert.AreEqual(2, result3.Count);

            // Add the three back to the first collection.
            // Adding all together makes 5 stubs

            c1.Add(c1_s1);
            c1.Add(c1_s2);
            c1.Add(c1_s3);

            TranslatorCollection result4 = c1 + c2;
            Assert.AreEqual(5, result4.Count);

        }

        #endregion

        #region [ Operator_Collection_Plus_Single ]

        [Test]
        public void Operator_Collection_Plus_Single()
        {
            
            StubTranslator t1 = new StubTranslator();
            StubTranslator t2 = new StubTranslator();

            // Create a new translator collection and then
            // add the first translator to it.

            TranslatorCollection c1 = new TranslatorCollection();
            c1.Add(t1);

            // Add this collection with the second translator.
            // The result should be a collection with both
            // stubs in it.

            TranslatorCollection c2 = c1 + t2;

            Assert.AreEqual(
                2,
                c2.Count,
                "The result should have two translators in it.");

            Assert.Contains(
                t1,
                c2);

            Assert.Contains(
                t2,
                c2);

        }

        #endregion

        #region [ Operator_Single_Plus_Single ]

        [Test]
        public void Operator_Single_Plus_Single()
        {

            StubTranslator s1 = new StubTranslator();
            StubTranslator s2 = new StubTranslator();

            TranslatorCollection c = s1 + s2;

            Assert.AreEqual(
                2,
                c.Count,
                "The result should have two translators.");

            Assert.Contains(s1, c, "Translator 1 is missing.");
            Assert.Contains(s2, c, "Translator 2 is missing.");

        }

        #endregion

    }
}

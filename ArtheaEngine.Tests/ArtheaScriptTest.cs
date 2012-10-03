using System;
using NUnit.Framework;

namespace ArtheaEngine.Tests
{
	[TestFixture]
	public class ArtheaScriptTest
	{
		private ScriptContext<ArtheaScriptTest> context;
		private TestWriter writer;

		[SetUp]
		public void Setup()
		{
			writer = new TestWriter();
			context = new ScriptContext<ArtheaScriptTest>(writer);
		}

		[Test]
		public void TestPrint ()
		{
			var script = @"print('Hello, World!');";

			new ArtheaScript<ArtheaScriptTest>(context).Run (script);

			Assert.AreEqual("Hello, World!\n", writer.ToString());
		}
	}
}


using NUnit.Framework;
using System;
using ArtheaEngine.Model;
using Jint.Native;
using System.Text;
using ArtheaEngine.Scripting;

namespace ArtheaEngine.Tests
{
	[TestFixture]
	public class ArtheaScriptTest
	{
		private ScriptContext context;
		private TestWriter writer;

		[SetUp]
		public void Setup()
		{
			writer = new TestWriter();
			context = new WritableScriptContext(writer);
		}

		[Test]
		public void TestPrint ()
		{
			var script = @"print('Hello, World!');";

			new ArtheaScript(context).Run (script);

			Assert.AreEqual("Hello, World!" + Environment.NewLine, writer.ToString());
		}

		[Test]
		public void TestParameters ()
		{
			context.SetParameters( "a", "b", "c", "d" );

			var engine = new ArtheaScript(context);
			
			var script = @"print($1)";

			engine.Run (script);

			Assert.AreEqual("a" + Environment.NewLine, writer.ToString());

			writer.Clear();

			script = @"print($2)";

			engine.Run (script);

			Assert.AreEqual("b" + Environment.NewLine, writer.ToString());
		}

		[Test]
		public void TestComplexParameters()
		{
			NonPlayer npc = new NonPlayer();
			npc.Name = "Actor";
			npc.ShortDescr = "A test actor";
			npc.Level = 22;
			npc.Sex = Sex.Male;
			npc.Race = new DwarfRace();
			npc.Id = "textMob1";

			context.SetParameters(npc);

			var engine = new ArtheaScript(context);

			var script = "print($1.Name)";

			engine.Run(script);

			Assert.AreEqual("Actor" + Environment.NewLine, writer.ToString());

			writer.Clear();

			script = "print($1.Race.Name)";

			engine.Run (script);

			Assert.AreEqual("Dwarf" + Environment.NewLine, writer.ToString());
		}

		class FunctionTest
		{
			public string Name
			{
				get; set;
			}

			public Func<Character,bool> CanUse {
				get;
				set;
			}

			public Action<Character, string> Execute {
				get; set;
			}
		}

		class TestChar : Character
		{
			TestWriter writer;

			public TestChar(TestWriter writer)
			{
				this.writer = writer;
			}

			public override World World {
				get { return new World(); }
			}

			public override void Page (string str)
			{
				writer.Page(str);
			}

			public override void Page (StringBuilder buf)
			{
				writer.Page (buf);
			}

			public override void Write (string format, params object[] args)
			{
				writer.Write(format, args);
			}

			public override void Write (byte[] arg)
			{
				writer.Write(arg);
			}

			public override void Write(object obj)
			{
				writer.Write(obj);
			}

			public override void WriteLine ()
			{
				writer.WriteLine();
			}

			public override void WriteLine(object obj)
			{
				writer.WriteLine(obj);
			}

			public override void WriteLine(string format, params object[] args)
			{
				writer.WriteLine(format, args);
			}
		}

		[Test]
		public void TestReturnValues()
		{
			var obj = new FunctionTest();

			TestChar ch = new TestChar(writer);

			ScriptContext context = new ScriptContext(obj);

			var script = @"
				$1.Name = 'testAbility'; 
				
				$1.CanUse = function(ch) {
					return ch.Name != '';
				};


				$1.Execute = function(ch, arg) {
					ch.Write('hello, ' + arg);
				};
			";

			var engine = new ArtheaScript(context);

			engine.Run (script);

			Assert.AreEqual(obj.Name, "testAbility");

			Assert.AreEqual(obj.CanUse(ch), true);

			obj.Execute(ch, "world");
			          
			Assert.AreEqual(writer.ToString(), "hello, world");
		}
	}
}


using System;
using Jint;
using ArtheaEngine.Model;

namespace ArtheaEngine
{
	public class ArtheaScript<T> : JintEngine
	{
		ScriptContext<T> context;

		public ArtheaScript (ScriptContext<T> context)
		{
			this.context = context;

			base.SetFunction("print", new Action<string>(Print));
		}

		public void Print(string text)
		{
			context.To.WriteLine(text);
		}

		public void Run(string text)
		{
			base.Run (text);
		}
	}
}


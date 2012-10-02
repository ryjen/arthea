using System;
using Jint;
using ArtheaEngine.Model;

namespace ArtheaEngine
{
	public class ArtheaScript : JintEngine
	{
		public ArtheaScript ()
		{
			base.SetFunction("echo", new Action<IWritable,string>(Print));
		}

		public void Print(IWritable to, string text)
		{
			to.WriteLine(text);
		}
	}
}


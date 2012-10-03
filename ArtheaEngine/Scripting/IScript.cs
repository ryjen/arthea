using System;

namespace ArtheaEngine
{
	public interface IScript<T>
	{
		void Run();

		ScriptContext<T> Context {
			get;
		}

		string Text {
			get;
		}
	}
}


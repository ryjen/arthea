using ArtheaEngine.Model;

namespace ArtheaEngine
{
	public class NPCScript : IScript<NonPlayer>
	{
		public NPCScript (string text, ScriptContext<NonPlayer> context)
		{
			Text = text;
			Context = context;
		}

		public string Text 
		{
			get; private set;
		}

		public void Run ()
		{
			new ArtheaScript<NonPlayer>(Context).Run(Text);
		}

		public ScriptContext<NonPlayer> Context
		{
			get; private set;
		}
	}
}


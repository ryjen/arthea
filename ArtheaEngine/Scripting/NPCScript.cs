using System;
using ArtheaEngine.Model;
using Jint;

namespace ArtheaEngine
{
	public class NPCScript : IScript
	{
		public NPCScript (string text, NonPlayer npc)
		{
			Text = text;
			NonPlayer = npc;
		}

		public string Text 
		{
			get; private set;
		}

		public NonPlayer NonPlayer
		{
			get; private set;
		}

		public void Run ()
		{
			new ArtheaScript().Run(Text);
		}
	}
}


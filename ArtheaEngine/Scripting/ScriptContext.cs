using System;
using ArtheaEngine.Model;

namespace ArtheaEngine
{
	public class ScriptContext
	{
		public ScriptContext (Player player)
		{
			Player = player;
		}

		public Player Player
		{
			get; set;
		}
	}
}


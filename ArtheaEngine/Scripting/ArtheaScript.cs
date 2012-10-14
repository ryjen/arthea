using Jint;
using System;

namespace ArtheaEngine.Scripting
{
	/// <summary>
	/// Arthea script.
	/// </summary>
	public class ArtheaScript : JintEngine
	{
		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>
		/// The context.
		/// </value>
		public ScriptContext Context
		{
			get; private set;
		}

		public ArtheaScript ()
		{
			//base.AllowClr = false;
			base.EnableSecurity();
			
#if DEBUG
			base.SetDebugMode(true);
#endif
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.ArtheaScript"/> class.
		/// </summary>
		/// <param name='context'>
		/// Context.
		/// </param>
		public ArtheaScript (ScriptContext context) : this()
		{
			Context = context;

			Context.Apply(this);
		}

	}
}


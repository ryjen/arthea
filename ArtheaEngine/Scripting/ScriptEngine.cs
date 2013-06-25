using Jint;
using System;

namespace ArtheaEngine.Scripting
{
	/// <summary>
	/// Arthea script.
	/// </summary>
	public class ScriptEngine : JintEngine, IDisposable
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

		public ScriptEngine ()
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
		public ScriptEngine (ScriptContext context) : this()
		{
			Context = context;

			Context.Apply(this);
		}

		public void Dispose()
		{

		}
	}
}


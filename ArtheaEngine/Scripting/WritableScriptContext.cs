using System;

namespace ArtheaEngine.Scripting
{
	public class WritableScriptContext : ScriptContext
	{
		public WritableScriptContext (IWritable to)
		{
			To = to;
		}

		public WritableScriptContext(IWritable to, params object[] args)
			: base(args)
		{
			To = to;
		}
		
		/// <summary>
		/// Gets or sets viewer.
		/// </summary>
		/// <value>
		/// The viewer
		/// </value>
		public IWritable To
		{
			get; set;
		}

		
		// some global scripting methods here

		/// <summary>
		/// Print the specified text.
		/// </summary>
		/// <param name='text'>
		/// Text.
		/// </param>
		public void Print(string text)
		{
			if(To != null)
				To.WriteLine(text);
		}

		/// <summary>
		/// Apply the specified script.
		/// </summary>
		/// <param name='script'>
		/// Script.
		/// </param>
		public override void Apply (ArtheaScript script)
		{
			base.Apply (script);

			script.SetFunction ("print", new Action<string> (Print));
		}
	}
}


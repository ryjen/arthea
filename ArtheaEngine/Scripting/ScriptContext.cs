
using System;

namespace ArtheaEngine.Scripting
{
	/// <summary>
	/// Script context.
	/// </summary>
	public class ScriptContext
	{
		object[] arguments;

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.ScriptContext"/> class.
		/// </summary>
		/// <param name='to'>
		/// The writable instance initiating the action
		/// </param>
		/// <param name='args'>
		/// Parameters for the script
		/// </param>
		public ScriptContext (params object[] args)
		{
			this.arguments = args;
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <value>
		/// The parameters.
		/// </value>
		public object[] Parameters {
			get {
				return arguments;
			}
		}

		/// <summary>
		/// Sets the parameters.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		public void SetParameters (params object[] args)
		{
			this.arguments = args;
		}

		/// <summary>
		/// Apply the specified script.
		/// </summary>
		/// <param name='script'>
		/// Script.
		/// </param>
		public virtual void Apply (ScriptEngine script)
		{

			int count = 1;
				
			foreach (var val in Parameters) {
				script.SetParameter ("$" + count, val);
				count++;
			}
		}
	}
}


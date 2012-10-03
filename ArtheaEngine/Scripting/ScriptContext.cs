
namespace ArtheaEngine
{
	public class ScriptContext<T>
	{
		public ScriptContext (IWritable to)
		{
			To = to;
		}

		public IWritable To
		{
			get; set;
		}

		public T Value {
			get;
			set;
		}
	}
}


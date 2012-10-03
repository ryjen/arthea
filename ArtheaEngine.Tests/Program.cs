using System;
using System.Reflection;

namespace ArtheaEngine.Tests
{

    class Program
    {
        #region Methods

        [STAThread]
        static void Main(string[] args)
        {
			NUnit.ConsoleRunner.Runner.Main(new string[] {
				Assembly.GetExecutingAssembly().Location, 
			});
        }

        #endregion Methods
    }
}
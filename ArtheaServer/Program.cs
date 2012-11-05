using System;

namespace ArtheaServer
{
    internal static class Program
    {
        #region Methods

        [STAThread]
        private static void Main(string[] arguments)
        {
            var args = new Arguments(arguments);

            GameServer.Run(args);
        }

        #endregion Methods
    }
}
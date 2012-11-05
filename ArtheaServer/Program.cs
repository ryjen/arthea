using ArtheaEngine;
using NLog;

namespace ArtheaServer
{
    using ArtheaEngine.Model;
    using ArtheaServer.Console;
    using ArtheaServer.GUI;
    using Microsoft.VisualBasic.ApplicationServices;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal static class Program
    {
        #region Methods

        [STAThread]
		static void Main (string[] arguments)
		{
			var args = new Arguments (arguments);

            GameServer.Run(args);
        }

        #endregion Methods
    }
}
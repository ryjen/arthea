using ArtheaEngine;

namespace ArtheaServer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using ArtheaEngine.Model;

    using ArtheaServer.Commands;

    using Microsoft.VisualBasic.ApplicationServices;

    public class ArtheaApp : WindowsFormsApplicationBase
    {
        #region Methods

        protected override void OnCreateMainForm ()
		{
			// Do your initialization here

			if (DbManager.Count<World> () == 0) {
				World.GetDefault ();
			}

            // Then create the main form, the splash screen will automatically close
            this.MainForm = new MainGame();
        }

        protected override void OnCreateSplashScreen()
        {
            this.SplashScreen = new ArtheaSplashScreen();
        }

        #endregion Methods
    }

    internal class Arguments
    {
        #region Fields

        // I think I found this class on CodeProject..
        // Variables
        private readonly StringDictionary Parameters;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Arguments"/> class.
        /// </summary>
        /// <param name="Args">The args.</param>
        public Arguments(IEnumerable<string> Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples:
            // -param1 value1 --param2 /param3:"Test-:-work"
            //   /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);

                switch (Parts.Length)
                {
                    // Found a value (for the last parameter
                    // found (space separator))
                    case 1:
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] =
                                    Remover.Replace(Parts[0], "$1");

                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting.
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting.
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }

                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }

                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (Parameter != null)
            {
                if (!Parameters.ContainsKey(Parameter))
                    Parameters.Add(Parameter, "true");
            }
        }

        #endregion Constructors

        #region Indexers

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified param.
        /// </summary>
        /// <value>the parameter value</value>
        public string this[string Param]
        {
            get { return (Parameters[Param]); }
        }

        #endregion Indexers
    }

    /*internal static class Program
    {
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            var cmdLine = new Arguments(args);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(cmdLine["transfer"] != null));
        }

        #endregion Methods
    }*/
    static class Program
    {
        #region Methods

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new ArtheaApp().Run(args);
        }

        #endregion Methods
    }
}
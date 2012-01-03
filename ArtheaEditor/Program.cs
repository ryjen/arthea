namespace ArtheaEditor
{
    using System;
    using System.Windows.Forms;

    using ArtheaEngine.Model;

    using Microsoft.VisualBasic.ApplicationServices;

    internal static class Program
    {
        #region Fields

        public static CollectionEditor MainEditor;

        #endregion Fields

        #region Methods

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MyApp().Run(args);
        }

        #endregion Methods

        #region Nested Types

        public class MyApp : WindowsFormsApplicationBase
        {
            #region Methods

            protected override void OnCreateMainForm()
            {
                // Do your initialization here

                if (World.List.Count == 0)
                {
                    World.GetDefault();
                }

                // Then create the main form, the splash screen will automatically close
                this.MainForm = new EditorForm();
            }

            protected override void OnCreateSplashScreen()
            {
                this.SplashScreen = new ArtheaSplashScreen();
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}
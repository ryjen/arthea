using System.Windows.Forms;
using ArtheaEngine;
using ArtheaEngine.Model;
using Microsoft.VisualBasic.ApplicationServices;

namespace ArtheaServer.GUI
{
    internal class GUIProgram : WindowsFormsApplicationBase, IProgram
    {
        #region Methods

        private bool reboot;

        public void Run(Arguments args)
        {
            reboot = args.Has("reboot");

            base.Run(args);
        }

        public void Exit()
        {
            Application.Exit();
        }

        protected override void OnCreateMainForm()
        {
            // Do your initialization here

            if (reboot)
                GameServer.Servers = RebootHelper.Recover();

            if (DbManager.Count<World>() == 0)
            {
                World.GetDefault();
            }

            // Then create the main form, the splash screen will automatically close
            MainForm = new MainGame();
        }

        protected override void OnCreateSplashScreen()
        {
            SplashScreen = new ArtheaSplashScreen();
        }

        #endregion Methods
    }
}
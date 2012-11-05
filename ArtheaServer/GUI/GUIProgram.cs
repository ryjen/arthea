using ArtheaEngine;
using ArtheaEngine.Model;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArtheaServer.GUI
{

    internal class GUIProgram : WindowsFormsApplicationBase, IProgram
    {
        #region Methods

        bool reboot;

        protected override void OnCreateMainForm()
        {
            // Do your initialization here

            if(reboot)
                GameServer.Servers = RebootHelper.Recover();

            if (DbManager.Count<World>() == 0)
            {
                World.GetDefault();
            }

            // Then create the main form, the splash screen will automatically close
            this.MainForm = new MainGame();
        }

        protected override void OnCreateSplashScreen()
        {
            this.SplashScreen = new ArtheaSplashScreen();
        }

        public void Run(Arguments args)
        {
            reboot = args.Has("reboot");

            base.Run(args);
        }

        public void Exit()
        {
            Application.Exit();
        }

        #endregion Methods
    }

}

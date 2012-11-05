using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Reflection;
using System.Windows.Forms;
using ArtheaEngine.Model;
using ArtheaServer.Console;
using ArtheaServer.GUI;

namespace ArtheaServer
{
    public static class GameServer
    {
        private static IProgram _program;
        static readonly Dictionary<string, string> _dllMapping = new Dictionary<string, string>();

        static GameServer()
        {
            Servers = new List<IServer>();

            _dllMapping["ArtheaEngine"] = "ArtheaEngine";
            AppDomain.CurrentDomain.AssemblyResolve += GameServerAssemblyResolve;

        }

        private static Assembly GameServerAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly newAssembly = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment deploy = ApplicationDeployment.CurrentDeployment;

                // Get the DLL name from the Name argument.
                string[] nameParts = args.Name.Split(',');
                string dllName = nameParts[0];
                string downloadGroupName = _dllMapping[dllName];

                try
                {
                    deploy.DownloadFileGroup(downloadGroupName);
                }
                catch (DeploymentException)
                {
                    MessageBox.Show("Downloading file group failed. Group name: " + downloadGroupName + "; DLL name: " + args.Name);
                    throw;
                }

                // Load the assembly.
                // Assembly.Load() doesn't work here, as the previous failure to load the assembly
                // is cached by the CLR. LoadFrom() is not recommended. Use LoadFile() instead.
                newAssembly = Assembly.LoadFile(Application.StartupPath + @"\" + dllName + ".dll");
            }
            else
            {
                //Major error - not running under ClickOnce, but missing assembly. Don't know how to recover.
                throw (new Exception("Cannot load assemblies dynamically - application is not deployed using ClickOnce."));
            }


            return (newAssembly);

        }

        public static List<IServer> Servers { get; set; }

        public static bool IsWorldActive(World world)
        {
            return Servers.Exists(x => x.World.Id.Equals(world.Id));
        }

        public static void Exit()
        {
            _program.Exit();
        }

        internal static void Run(Arguments args)
        {
            if (args.Has("console"))
            {
                _program = new ConsoleProgram();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _program = new GUIProgram();
            }

            _program.Run(args);
        }
    }
}
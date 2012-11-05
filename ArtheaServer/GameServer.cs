using ArtheaEngine.Model;
using ArtheaServer.Console;
using ArtheaServer.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtheaServer
{
    public static class GameServer
    {
        static IProgram program;

        static GameServer()
        {
            Servers = new List<IServer>();
        }

        public static List<IServer> Servers
        {
            get;
            set;
        }

        public static bool IsWorldActive(World world)
        {
            return Servers.Exists(x => x.World.Id.Equals(world.Id));
        }

        public static void Exit()
        {
            program.Exit();
        }

        internal static void Run(Arguments args)
        {

            if (args.Has("console"))
            {
               program = new ConsoleProgram();
            }
            else
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

               program = new GUIProgram();
            }

            program.Run(args);
        }
    }
}

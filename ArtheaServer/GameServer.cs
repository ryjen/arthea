using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using ArtheaEngine;

using System.Reflection;
using System.Windows.Forms;
using ArtheaEngine.Model;
using ArtheaServer.Console;
using ArtheaServer.GUI;
using System.Linq;

namespace ArtheaServer
{
    public static class GameServer
    {
		public class GameConfig : IDisposable, IRootLevelDocument
		{
			public GameConfig()
			{
				Version = "1.0";
			}

			[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
			public string Id
			{
				get; private set;
			}

			public string Version {
				get;
				set;
			}

			public void Dispose ()
			{
				DbManager.Save(this);
			}

			public bool Save ()
			{
				return DbManager.Save (this);
			}

			public bool Delete ()
			{
				return DbManager.Delete (this);
			}
		}
        private static IProgram _program;

		public static GameConfig Config {
			get;
			private set;
		}

		public static string Arguments {
			get;
			private set;
		}

        static GameServer ()
		{
			Servers = new List<IServer> ();

			Config = DbManager.Query<GameConfig>().FirstOrDefault();
		}

        public static List<IServer> Servers { get; set; }

		public static void StopAll ()
		{
			while (Servers.Count > 0) {
				Servers [0].Stop ();
				Servers.RemoveAt (0);
			}
		}

        public static bool IsWorldActive(World world)
        {
            return Servers.Exists(x => x.World.Id.Equals(world.Id));
        }

        public static void Exit()
        {
            _program.Exit();
        }

		private static void Init ()
		{
			if (DbManager.Count<World>() == 0)
			{
				World.GetDefault();
			}
		}

        internal static void Run(Arguments args)
        {
			Arguments = args.ToString();

			Init();

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
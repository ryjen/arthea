using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Odbc;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ArtheaEngine;

    using LuaInterface;

    using NLog;

    public class Reset : IEntity
    {
        #region Fields
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        NonPlayer lastNPC;
        Object lastOBJ;

        #endregion Fields

        #region Constructors

        public Reset(Room room)
        {
			Id = ObjectId.GenerateNewId().ToString();
            Room = room;
        }

        #endregion Constructors

        #region Properties

		[BsonRequired]
        public string Code
        {
            get; set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonIgnore]
        public Room Room
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public void EchoReset(string msg)
        {
            foreach (var ch in Room.Characters)
                ch.WriteLine(msg);
        }

        //long id, WearLocation loc)
        public void EquipReset (string[] args)
		{
			if (lastNPC == null || args.Length < 2)
				return;

			WearLocation loc = (WearLocation)Enum.Parse (typeof(WearLocation), args [2], true);

			lastOBJ = World.FindObject (args [1]);

			if (lastOBJ == null) {
				Log.Error ("object {0} not found for reset {1}", lastOBJ, Id);
				return;
			}

            foreach (var co in lastNPC.Carrying)
            {
                if (co.WearLocation == loc) return;
            }

			lastOBJ.WearLocation = loc;

			lastOBJ.CarriedBy = lastNPC;

            lastNPC.Carrying.Add(lastOBJ);
        }

        public void Execute()
        {
            var reader = new StringReader(Code);

            for(var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                var args = line.Split(new []{'(', ',', ')', ' ',}, StringSplitOptions.RemoveEmptyEntries);

                switch (args[0].ToLower())
                {
                    case "npc":
                        NpcReset(args);
                        break;
                    case "equip":
                        EquipReset(args);
                        break;
                    case "give":
                        GiveReset(args);
                        break;
                    case "put":
                        PutReset(args);
                        break;
                    case "echo":
                        EchoReset(line.Substring(line.IndexOf(' ')+1));
                        break;
                }
            }
        }

        public void GiveReset (string[] args)
		{
			
			if (lastNPC == null || args.Length == 0)
				return;
			
			int max = 1;
			int count = 0;

			if (args.Length > 2) {
				max = int.Parse (args [2]);
			}

			lastOBJ = World.FindObject (args [1]);

			if (lastOBJ == null) {
				Log.Error ("object {0} was not found for reset {1}", args[1], Id);
				return;
			}

            foreach (Object o in lastNPC.Carrying)
            {
                if (o.Equals (lastOBJ) ) count++;
            }

            if (count >= max) return;


            lastNPC.Carrying.Add(lastOBJ);
            lastOBJ.CarriedBy = lastNPC;
        }

        public void NpcReset (string[] args)
		{
			if(args.Length == 0)
				return;

			int max = 1;
			int count = 0;

			if (args.Length > 2) {
				max = int.Parse (args [2]);
			}

			lastNPC = World.FindNPC(args[1]);

			if (lastNPC == null) {
				Log.Error ("No such npc {0} for reset {1}", args[1], Id);
				return;
			}

            foreach (Character ch in Room.Characters)
            {
                if (ch.Equals(lastNPC)) count++;
            }

            if (count >= max) return;

            Room.Characters.Add(lastNPC);
        }

        public void ObjReset (string[] args)
		{
			if(args.Length == 0) return;

			int max = 1;
			int count = 0;

			if (args.Length > 2) {
				max = int.Parse (args [2]);
			}
			
			lastOBJ = World.FindObject(args[1]);
			
			if (lastOBJ == null) {
				Log.Error ("Invalid object '{0}' for reset {1}", args[1], Id);
				return;
			}

            foreach (Object o in Room.Objects)
            {
                if (o.Equals(lastOBJ)) count++;
            }

            if (count >= max) return;

            Room.Objects.Add(lastOBJ);
        }

        public void PutReset (string[] args)
		{
			if(lastOBJ == null || args.Length == 0)
				return;

			int max = 1;
			int count = 0;

			if (args.Length > 2) {
				max = int.Parse (args [2]);
			}

			var newObj = World.FindObject(args[1]);

			if (newObj == null) {
				Log.Error ("Invalid object '{0}' for reset {1}", args[1], Id);
				return;
			}

            foreach (Object o in lastOBJ.Contents)
            {
                if (o.Equals(newObj)) count++;
            }

            if (count >= max) return;

            lastOBJ.Contents.Add(newObj);
            newObj.Inside = lastOBJ;
        }

        #endregion Methods
    }
}
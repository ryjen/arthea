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

    public class Reset : IEntity<long>, IPersistent
    {
        #region Fields

        NonPlayer lastNPC;
        Object lastOBJ;

        #endregion Fields

        #region Constructors

        public Reset(Room room)
        {
            Room = room;
        }

        #endregion Constructors

        #region Properties

        public string Code
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public Room Room
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public virtual bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_reset");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public void EchoReset(string msg)
        {
            foreach (var ch in Room.Characters)
                ch.WriteLine(msg);
        }

        //long id, WearLocation loc)
        public void EquipReset(string[] args)
        {
            if (lastNPC == null) return;

            long id = long.Parse(args[1]);

            WearLocation loc = (WearLocation) Enum.Parse(typeof(WearLocation), args[2], true);

            foreach (var co in lastNPC.Carrying)
            {
                if (co.WearLocation == loc) return;
            }

            lastOBJ = Object.Load(id);

            lastNPC.Carrying.Add(new CharObj { Object = lastOBJ, Character = lastNPC, WearLocation = loc });

            lastOBJ.CarriedBy = lastNPC;
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

        public void GiveReset(string[] args)
        {
            long id = long.Parse(args[1]);

            int max = 1;
            int count = 0;

            if (args.Length > 2)
            {
                max = int.Parse(args[2]);
            }

            if (lastNPC == null) return;

            foreach (CharObj o in lastNPC.Carrying)
            {
                if (o.Object.Id == id) count++;
            }

            if (count >= max) return;

            lastOBJ= Object.Load(id);

            lastNPC.Carrying.Add(new CharObj { Object = lastOBJ, Character = lastNPC, WearLocation = WearLocation.None });
            lastOBJ.CarriedBy = lastNPC;
        }

        public virtual bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_reset");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);

            return this.Load(cmd);
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Code = reader.GetString(i++);

            return i;
        }

        public void NpcReset(string[] args)
        {
            int max = 1;
            int count = 0;

            long id = long.Parse(args[1]);

            if (args.Length > 2)
            {
                max = int.Parse(args[2]);
            }

            foreach (Character ch in Room.Characters)
            {
                if (ch.Id == id) count++;
            }

            if (count >= max) return;

            lastNPC = NonPlayer.Load(id);

            Room.Characters.Add(lastNPC);
        }

        public void ObjReset(string[] args)
        {
            long id = long.Parse(args[1]);

            int max = 1;
            int count = 0;

            if (args.Length > 2)
            {
                max = int.Parse(args[2]);
            }

            foreach (Object o in Room.Objects)
            {
                if (o.Id == id) count++;
            }

            if (count >= max) return;

            lastOBJ = Object.Load(id);

            Room.Objects.Add(lastOBJ);
        }

        public void PutReset(string[] args)
        {
            long id = long.Parse(args[1]);
            int max = 1;
            int count = 0;

            if (lastOBJ == null) return;

            if (args.Length > 2)
            {
                max = int.Parse(args[2]);
            }

            foreach (Object o in lastOBJ.Contents)
            {
                if (o.Id == id) count++;
            }

            if (count >= max) return;

            var newObj = Object.Load(id);

            lastOBJ.Contents.Add(newObj);
            newObj.Inside = lastOBJ;
        }

        public virtual bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_reset");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@code", DbType.String, Code);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
                Id = Convert.ToInt64(conn.LastInsertId());

            return res;
        }

        #endregion Methods
    }
}
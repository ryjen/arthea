namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public partial class Social : IEntity<int>, IPersistent
    {
        #region Fields

        public static readonly HashSet<Social> List = new HashSet<Social>();

        #endregion Fields

        #region Constructors

        static Social()
        {
            ReadAll();
        }

        #endregion Constructors

        #region Properties

        public string CharFound
        {
            get; set;
        }

        public string CharNoArg
        {
            get; set;
        }

        public string CharNotFound
        {
            get; set;
        }

        public string CharObjFound
        {
            get; set;
        }

        public string CharSelf
        {
            get; set;
        }

        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string OthersFound
        {
            get; set;
        }

        public string OthersNoArg
        {
            get; set;
        }

        public string OthersObjFound
        {
            get; set;
        }

        public string OthersSelf
        {
            get; set;
        }

        public string VictFound
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static bool Interpret(Character ch, Argument argument)
        {
            var arg = argument.Peek();
            /*var context = new ArtheaModel();

            foreach (var s in context.Socials)
            {
                if (!s.Name.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase)) continue;

                argument.FirstArg();
                s.Parse(ch, argument);
                return true;
            }*/
            return false;
        }

        public static void ReadAll()
        {
            using (var conn = ArtheaHelper.NewConnection())
            {
                var cmd = conn.CreateCommand("select_socials");

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var s = new Social();
                    s.MapRow(reader);

                    List.Add(s);
                }
            }
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_social");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery()== 1;
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_social");
            cmd.AddParameter("@id", DbType.Int32, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);
            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt32(i++);
            CharFound = reader.GetString(i++);
            CharNoArg = reader.GetString(i++);
            CharNotFound = reader.GetString(i++);
            CharObjFound = reader.GetString(i++);
            CharSelf = reader.GetString(i++);
            Name = reader.GetString(i++);
            OthersFound = reader.GetString(i++);
            OthersNoArg = reader.GetString(i++);
            OthersObjFound = reader.GetString(i++);
            OthersSelf = reader.GetString(i++);
            VictFound = reader.GetString(i++);
            return i;
        }

        public virtual void Parse(Character ch, Argument argument)
        {
            if (argument.IsEmpty())
            {
                ch.Room.Act(OthersNoArg, ch);
                ch.Act(CharNoArg);
                return;
            }

            var victim = ch.Room.Characters.Get(c => c.Name.HasWord(argument.ToString()));

            if (victim == null)
            {
                var obj = ch.Room.Objects.Get(o => o.Name.HasWord(argument));

                if (obj == null)
                {
                    ch.WriteLine("That person or object is not here.");
                    return;
                }

                ch.Act(CharObjFound, obj);
                ch.Room.Act(OthersObjFound, ch, obj);
                return;
            }

            if (victim == ch)
            {
                ch.Act(CharSelf);
                ch.Act(OthersSelf, ch);
                return;
            }

            ch.Act(CharFound, victim);
            victim.Act(VictFound, ch);
            victim.Room.Act(new []{ ch, victim}, OthersFound, ch, victim);
            return;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_social");
            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@char_found", DbType.String, CharFound);
            cmd.AddParameter("@char_no_arg", DbType.String, CharNoArg);
            cmd.AddParameter("@char_not_found", DbType.String, CharNotFound);
            cmd.AddParameter("@char_obj_found", DbType.String, CharObjFound);
            cmd.AddParameter("@char_self", DbType.String, CharSelf);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@others_found", DbType.String, OthersFound);
            cmd.AddParameter("@others_no_arg", DbType.String, OthersNoArg);
            cmd.AddParameter("@others_obj_found", DbType.String, OthersObjFound);
            cmd.AddParameter("@others_self", DbType.String, OthersSelf);
            cmd.AddParameter("@victim_found", DbType.String, VictFound);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt32(conn.LastInsertId());
                List.Add(this);
            }

            return res;
        }

        #endregion Methods
    }
}
namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using NLog;

    public abstract class Ability : ICloneable, IEntity<int>, ISaveable, ILoadable
    {
        #region Fields

        public static readonly HashSet<Ability> List = new HashSet<Ability>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        static readonly object[] Parameters = new object[] { 
                "@id", DbType.Int32,
                "@name", DbType.String,
                "@level", DbType.Int16,
                "@descr", DbType.String,
                "@cost", DbType.Currency
            };

        #endregion Fields

        #region Constructors

        static Ability()
        {
            Assembly assm = Assembly.GetExecutingAssembly();

            using (var conn = ArtheaHelper.NewConnection())
            {
                foreach (var t in assm.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(Ability)))
                    {
                        var ability = (Ability)Activator.CreateInstance(t);
                        if (!ability.Load(conn))
                            ability.Save(conn);
                        List.Add(ability);
                    }
                }
            }
        }

        #endregion Constructors

        #region Properties

        public Money Cost
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public abstract int Id
        {
            get;
        }

        public short Level
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static Ability Lookup(int id)
        {
            return List.SingleOrDefault(x => x.Id == id);
        }

        /// <summary>
        ///                     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///                     A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            var newObject = Activator.CreateInstance(GetType());

            ArtheaHelper.CloneObj(newObject, this);

            return newObject;
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_ability");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public void DisplayOffMsg(object from)
        {
            string msg;

            if (from == null)
                return;

            if (from is Character)
            {
                var ch = from as Character;
                msg = GetAbilityMsg("MsgOffSelf");
                ch.WriteLine(msg);
                msg = GetAbilityMsg("MsgOffChar");
                ch.Room.Act(msg);
            }
            else if (from is Object)
            {
                var item = from as Object;
                if (item.CarriedBy != null || item.Room != null)
                {
                    msg = GetAbilityMsg("MsgOffObj");
                    if (item.CarriedBy != null)
                    {
                        item.CarriedBy.Act(msg);
                    }
                    else if (item.Room != null)
                    {
                        item.Room.Act(msg);
                    }
                }
            }
            else if (from is Room)
            {
                var room = from as Room;
                msg = GetAbilityMsg("MsgOffRoom");

                room.Act(msg);
            }
        }

        public bool Load(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand("read_ability"))
            {
                cmd.AddParameter("@id", DbType.Int32, Id);

                return this.Load(cmd);
            }
        }

        public int MapRow(IDataRecord row)
        {
            int i = 0;

            Name = row.GetString(i++);
            Level = row.GetInt16(i++);
            Description = row.GetString(i++);
            Cost = row.GetDecimal(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("update_ability");
            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@level", DbType.Int16, Level);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@cost", DbType.Currency, Cost);

            var res = cmd.ExecuteNonQuery() > 0;

            return res;
        }

        protected virtual string GetAbilityMsg(string name)
        {
            var fi = GetType().GetField(name,
                                        BindingFlags.Static | BindingFlags.NonPublic |
                                        BindingFlags.Public);
            return fi.GetValue(null) as string;
        }

        #endregion Methods
    }
}
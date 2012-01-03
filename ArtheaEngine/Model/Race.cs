namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq.Mapping;
    using System.Data.Odbc;
    using System.Linq;
    using System.Reflection;

    using NLog;

    public class DwarfRace : PcRace
    {
        #region Constructors

        public DwarfRace()
        {
        }

        #endregion Constructors

        #region Properties

        public override string Description
        {
            get { return "A short, stocky race."; }
        }

        public override int Id
        {
            get { return 2; }
        }

        public override string Name
        {
            get { return "dwarf"; }
        }

        #endregion Properties
    }

    public class HumanRace : PcRace
    {
        #region Constructors

        public HumanRace()
        {
        }

        #endregion Constructors

        #region Properties

        public override string Description
        {
            get { return "The standard race."; }
        }

        public override int Id
        {
            get { return 1; }
        }

        public override string Name
        {
            get { return "human"; }
        }

        #endregion Properties
    }

    public abstract class PcRace : Race
    {
        #region Constructors

        public PcRace()
        {
        }

        #endregion Constructors
    }

    public abstract class Race : IEntity<int>, IPersistent
    {
        #region Fields

        public static readonly HashSet<Race> List = new HashSet<Race>();

        #endregion Fields

        #region Constructors

        static Race()
        {
            var assm = Assembly.GetExecutingAssembly();

            using (var conn = ArtheaHelper.NewConnection())
            {
                foreach (var t in assm.GetTypes())
                {
                    if (!t.IsSubclassOf(typeof(Race)) || t.IsAbstract) continue;

                    var r = (Race)Activator.CreateInstance(t);
                    if (!r.Load(conn))
                        r.Save(conn);
                    List.Add(r);
                }
            }

            LogManager.GetCurrentClassLogger().Info("Loaded {0} races", List.Count);
        }

        public Race()
        {
        }

        #endregion Constructors

        #region Properties

        public abstract string Description
        {
            get;
        }

        public abstract int Id
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

        public static Race Lookup(string name)
        {
            return List.FirstOrDefault((r) => r.Name.IsPrefixOf(name));
        }

        public static Race Lookup(int id)
        {
            return List.FirstOrDefault(r => r.Id == id);
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_race");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_race");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return this.Load(cmd);
        }

        public int MapRow(IDataRecord reader)
        {
            return 0;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_race");

            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);

            return cmd.ExecuteNonQuery() > 0;
        }

        #endregion Methods
    }

    public class UniqueRace : Race
    {
        #region Properties

        public override string Description
        {
            get { return "A unique, unknown race."; }
        }

        public override int Id
        {
            get { return 0; }
        }

        public override string Name
        {
            get { return "unique"; }
        }

        #endregion Properties
    }
}
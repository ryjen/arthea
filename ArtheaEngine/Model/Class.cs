namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Odbc;
    using System.Linq;
    using System.Reflection;

    public abstract class Class : IEntity<int>, IPersistent
    {
        #region Fields

        public static readonly HashSet<Class> List = new HashSet<Class>();

        #endregion Fields

        #region Constructors

        static Class()
        {
            var assm = Assembly.GetExecutingAssembly();

            foreach (var t in assm.GetTypes())
            {
                if (!t.IsSubclassOf(typeof(Class)) || t.IsAbstract)
                    continue;

                var c = (Class)Activator.CreateInstance(t);

                if (!c.Load())
                    c.Save();

                List.Add(c);
            }
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

        public static Class Lookup(int id)
        {
            return List.FirstOrDefault(x => x.Id == id);
        }

        public static Class Lookup(string name)
        {
            return List.FirstOrDefault(x => x.Name.HasWord(name));
        }

        public bool Delete(IDbConnection conn)
        {
            //return conn.ExecuteNonQuery("delete_class", DbType.Int32, Id) == 1;
            var cmd = conn.CreateCommand("read_class");

            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            //var reader = conn.ExecuteReader("read_class", DbType.Int32, Id);
            var cmd = conn.CreateCommand("read_class");

            cmd.AddParameter("@id", DbType.Int32, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            return 0;
        }

        public bool Save(IDbConnection conn)
        {
            //return conn.ExecuteNonQuery("save_class", DbType.Int32, Id, DbType.String, Name, DbType.String, Description) == 1;
            var cmd = conn.CreateCommand("read_class");

            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);

            return cmd.ExecuteNonQuery() > 0;
        }

        #endregion Methods
    }

    public class MageClass : Class
    {
        #region Constructors

        public MageClass()
        {
        }

        #endregion Constructors

        #region Properties

        public override string Description
        {
            get { return "Master of magic and the arcane."; }
        }

        public override int Id
        {
            get { return 1; }
        }

        public override string Name
        {
            get { return "mage"; }
        }

        #endregion Properties
    }

    public class WarriorClass : Class
    {
        #region Constructors

        public WarriorClass()
        {
        }

        #endregion Constructors

        #region Properties

        public override string Description
        {
            get { return "Masters of hand to hand combat and weapons."; }
        }

        public override int Id
        {
            get { return 2; }
        }

        public override string Name
        {
            get { return "warrior"; }
        }

        #endregion Properties
    }
}
namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using System.Text;

    using NLog;

    #region Enumerations

    [Flags]
    public enum HelpCategory
    {
        None, Admin, Information
    }

    #endregion Enumerations

    public class Help : IEquatable<Help>, IEntity<long>, IPersistent
    {
        #region Fields

        public static readonly HashSet<Help> List = new HashSet<Help>();

        #endregion Fields

        #region Constructors

        static Help()
        {
            ReadAll();
        }

        public Help()
        {
        }

        #endregion Constructors

        #region Properties

        public HelpCategory Category
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public ISet<string> Keywords
        {
            get; set;
        }

        public IList<Help> Related
        {
            get; set;
        }

        public string Text
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static Help Lookup(long id)
        {
            return List.SingleOrDefault(x => x.Id == id);
        }

        public static void ReadAll()
        {
            using (var conn = ArtheaHelper.NewConnection())
            {
                try
                {
                    var reader = conn.CreateCommand("select_helps").ExecuteReader();

                    while (reader.Read())
                    {
                        var help = new Help();

                        help.MapRow(reader);

                        List.Add(help);
                    }

                    foreach (var help in List)
                    {
                        help.Related = ReadRelated(help.Id, conn);

                        help.Keywords = ReadKeywords(help.Id, conn);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error("could not read all helps: " + ex);
                }
            }
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd =  conn.CreateCommand("delete_help");

            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Help)) return false;

            return Equals(obj as Help);
        }

        /// <summary>
        ///                     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        ///                     An object to compare with this object.
        ///                 </param>
        public bool Equals(Help other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_help");

            cmd.AddParameter("@id", DbType.Int64, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            Related = ReadRelated(Id, conn);

            Keywords = ReadKeywords(Id, conn);
            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Text = reader.GetString(i++);
            Category = reader.GetEnum<HelpCategory>(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_help");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@text", DbType.String, Text);
            cmd.AddParameter("@category", DbType.String, Category.ToString());

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
                List.Add(this);
            }

            foreach (var rel in Related)
            {
                cmd = conn.CreateCommand("save_related_help");
                cmd.AddParameter("@id", DbType.Int64, Id);
                cmd.AddParameter("@related", DbType.Int64, rel.Id);

                res = res && cmd.ExecuteNonQuery() > 0;
            }

            foreach (var keyword in Keywords)
            {
                cmd = conn.CreateCommand("save_help_keyword");
                cmd.AddParameter("@id", DbType.Int64, Id);
                cmd.AddParameter("@keyword", DbType.String, keyword);

                res = res && cmd.ExecuteNonQuery() > 0;
            }

            return res;
        }

        private static ISet<string> ReadKeywords(long id, IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_help_keywords");
            cmd.AddParameter("@id", DbType.Int64, id);

            var reader = cmd.ExecuteReader();

            var list = new HashSet<string>();

            while (reader.Read())
            {
                list.Add(reader.GetString(1));
            }

            return list;
        }

        private static IList<Help> ReadRelated(long id, IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_related_helps");
            cmd.AddParameter("@id", DbType.Int64, id);
            var reader = cmd.ExecuteReader();

            var list = new List<Help>();

            while (reader.Read())
            {
                var relId = reader.GetInt64(1);
                var help = Lookup(relId);
                if (help != null)
                    list.Add(help);
                else
                {
                    var cmd2 = conn.CreateCommand("delete_related_help");
                    cmd2.AddParameter("@id", DbType.Int64, id);
                    cmd2.AddParameter("@related", DbType.Int64, relId);
                    cmd2.ExecuteNonQuery();
                }
            }

            return list;
        }

        #endregion Methods
    }
}
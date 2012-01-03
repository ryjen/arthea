namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Linq.Mapping;
    using System.Diagnostics;
    using System.Text;

    using NLog;

    #region Enumerations

    [Flags]
    public enum AccountFlag
    {
        None,
        Admin
    }

    #endregion Enumerations

    public class Account : ISaveable, IWritable, IEntity<long>, ILoadable
    {
        #region Constructors

        public Account()
        {
            Players = new List<AccountPlayer>();
        }

        #endregion Constructors

        #region Properties

        public virtual IConnection Connection
        {
            get; set;
        }

        public string Email
        {
            get; set;
        }

        public AccountFlag Flags
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public string Login
        {
            get; set;
        }

        public byte[] Password
        {
            get; set;
        }

        public List<AccountPlayer> Players
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_account");
            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(string login)
        {
            using (var conn = ArtheaHelper.NewConnection())
            {
                try
                {
                    using (var cmd = conn.CreateCommand("select_account_by_login"))
                    {
                        cmd.AddParameter("@login", DbType.String, Login);

                        if (!this.Load(cmd))
                            return false;
                    }

                    LoadPlayerInfo(conn);

                    return true;

                }
                catch(Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex.Message);
                }

                return false;
            }
        }

        public bool Load(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand("read_account"))
            {
                cmd.AddParameter("@id", DbType.Int64, Id);

                if (!this.Load(cmd))
                    return false;

            }

            LoadPlayerInfo(conn);

            return true;
        }

        public void LoadPlayerInfo(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand("select_account_players"))
            {
                cmd.AddParameter("@id", DbType.Int64, Id);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    AccountPlayer p = new AccountPlayer();

                    p.MapRow(reader);
                    p.Account = this;

                    Players.Add(p);
                }
            }
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Login = reader.GetString(i++);
            var buf = new byte[255];
            var len = reader.GetBytes(i++, 0, buf, 0, 255);
            Password = new byte[len];
            Array.Copy(buf, Password, len);
            Email = reader.GetString(i++);
            Flags = reader.GetEnum<AccountFlag>(i++);
            return i;
        }

        public virtual void Page(string buf)
        {
            Connection.Page(buf);
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_account");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@login",DbType.String, Login);
            cmd.AddParameter("@passwd", DbType.Binary, Password);
            cmd.AddParameter("@email", DbType.String, Email);
            cmd.AddParameter("@flags", DbType.String, Flags.ToString());

            var res = cmd.ExecuteNonQuery() > 0;
            if (Id == 0 && res)
            {
                Id = Convert.ToInt64(conn.LastInsertId());
            }

            return res;
        }

        public virtual void Write(string format, params object[] args)
        {
            Connection.Write(format, args);
        }

        public virtual void Write(byte[] value)
        {
            Connection.Write(value);
        }

        public virtual void Write(object obj)
        {
            Connection.Write(obj);
        }

        public virtual void WriteLine()
        {
            Connection.WriteLine();
        }

        public virtual void WriteLine(object value)
        {
            Connection.WriteLine(value);
        }

        public virtual void WriteLine(string format, params object[] args)
        {
            Connection.WriteLine(format, args);
        }

        #endregion Methods
    }

    public class AccountPlayer
    {
        #region Properties

        public Account Account
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public DateTime LastLogin
        {
            get; set;
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

        public Player Load()
        {
            var p = new Player();
            p.Id = Id;
            p.Account = Account;

            if (!p.Load())
                return null;

            return p;
        }

        public int MapRow(IDataRecord reader)
        {
            var i = 0;

            Id = reader.GetInt64(i++);
            Name = reader.GetString(i++);
            Level = reader.GetInt16(i++);
            //p.LastLogin = reader.GetDateTime(3);
            return i;
        }

        #endregion Methods
    }
}
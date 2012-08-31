using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

using NLog;

namespace ArtheaEngine.Model
{

    #region Enumerations

    [Flags]
    public enum AccountFlag
    {
        None,
        Admin
    }

    #endregion Enumerations

    public class Account : IWritable, IRootLevelDocument
    {
        #region Constructors

		static Account ()
		{
			var accounts = DbManager.Collection<Account>();
			accounts.EnsureIndex(IndexKeys.Ascending("Email"), IndexOptions.SetUnique(true));
			accounts.EnsureIndex(IndexKeys.Ascending("Login"), IndexOptions.SetUnique(true));
		}

        public Account(IConnection connection, string login)
        {
			Id = ObjectId.GenerateNewId().ToString();
			Connection = connection;
			Login = login;
			Players = new List<string>();
        }

        #endregion Constructors

        #region Properties

		[BsonIgnore]
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

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public string Id {
			get;
			set;
		}

        public string Login
        {
            get; set;
        }

        public byte[] Password
        {
            get; set;
        }

        public List<string> Players
        {
            get; set;
        }

		
		public bool Save ()
		{
			return DbManager.Save (this);
		}

		public bool Delete ()
		{
			return DbManager.Delete (this);
		}


        #endregion Properties

        #region Methods

        public virtual void Page(string buf)
        {
            Connection.Page(buf);
        }

		public virtual void Page (StringBuilder buf)
		{
			Connection.Page (buf);
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


		public static Account Lookup (string id)
		{
			return DbManager.Lookup<Account> (id);
		}

        #endregion Methods
    }
}
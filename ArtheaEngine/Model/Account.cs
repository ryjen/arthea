using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Text;


namespace ArtheaEngine.Model
{

    #region Enumerations
	/// <summary>
	/// Account flag.
	/// </summary>
    [Flags]
    public enum AccountFlag
    {
        None,
        Admin
    }

    #endregion Enumerations

	/// <summary>
	/// Account.
	/// </summary>
    public class Account : IWritable, IRootLevelDocument
    {
        #region Constructors

		/// <summary>
		/// Initializes the <see cref="ArtheaEngine.Model.Account"/> class.
		/// </summary>
		static Account ()
		{
			var accounts = DbManager.Collection<Account>();
			accounts.EnsureIndex(IndexKeys.Ascending("Email"), IndexOptions.SetUnique(true));
			accounts.EnsureIndex(IndexKeys.Ascending("Login"), IndexOptions.SetUnique(true));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Account"/> class.
		/// </summary>
		/// <param name='connection'>
		/// Connection.
		/// </param>
		/// <param name='login'>
		/// Login.
		/// </param>
        public Account(IConnection connection, string login)
        {
			Id = ObjectId.GenerateNewId().ToString();
			Connection = connection;
			Login = login;
			Players = new List<string>();
        }

        #endregion Constructors

        #region Properties

		/// <summary>
		/// Gets or sets the connection.
		/// </summary>
		/// <value>
		/// The connection.
		/// </value>
		[BsonIgnore]
        public virtual IConnection Connection
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value>
		/// The email.
		/// </value>
        public string Email
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		/// <value>
		/// The flags.
		/// </value>
        public AccountFlag Flags
        {
            get; set;
        }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
		public string Id {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the login.
		/// </summary>
		/// <value>
		/// The login.
		/// </value>
        public string Login
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>
		/// The password.
		/// </value>
        public byte[] Password
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the players.
		/// </summary>
		/// <value>
		/// The players.
		/// </value>
        public List<string> Players
        {
            get; set;
        }

		/// <summary>
		/// Save this instance.
		/// </summary>
		public bool Save ()
		{
			return DbManager.Save (this);
		}

		/// <summary>
		/// Delete this instance.
		/// </summary>
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

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public static Account Lookup (string id)
		{
			return DbManager.Lookup<Account> (id);
		}

        #endregion Methods
    }
}
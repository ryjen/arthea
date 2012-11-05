using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Text;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Account flag.
	/// </summary>
    [Flags]
    public enum AccountFlag
    {
        None,
        Admin
    }

	/// <summary>
	/// Account.
	/// </summary>
    public sealed class Account : IWritable, IRootLevelDocument
    {
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

		/// <summary>
		/// Gets or sets the connection.
		/// </summary>
		/// <value>
		/// The connection.
		/// </value>
		[BsonIgnore]
        public IConnection Connection
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

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
        public void Page(string buf)
        {
            Connection.Page(buf);
        }

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		public void Page (StringBuilder buf)
		{
			Connection.Page (buf);
		}

		/// <summary>
		/// Write the specified format and args.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public void Write(string format, params object[] args)
        {
            Connection.Write(format, args);
        }

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
        public void Write(byte[] value)
        {
            Connection.Write(value);
        }

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public void Write(object obj)
        {
            Connection.Write(obj);
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
        public void WriteLine()
        {
            Connection.WriteLine();
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
        public void WriteLine(object value)
        {
            Connection.WriteLine(value);
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='format'>
		/// Format.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
        public void WriteLine(string format, params object[] args)
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
    }
}
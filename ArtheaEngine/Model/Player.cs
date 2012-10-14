using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Text;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Channel flag.
	/// </summary>
    [Flags]
    public enum ChannelFlag
    {
        None
    }

	/// <summary>
	/// Pc flag.
	/// </summary>
    [Flags]
    public enum PcFlag
    {
        None
    }

	/// <summary>
	/// Player.
	/// </summary>
    public class Player : Character
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Player"/> class.
		/// </summary>
		public Player() : base()
        {
            LastNote = new Dictionary<string, DateTime>();
            Prompt = "~R%l/%L ~M%m/%M ~B%e/%E~x";
            CombatPrompt = Prompt;
        }

		/// <summary>
		/// Gets or sets the account reference.
		/// </summary>
		/// <value>
		/// The account reference.
		/// </value>
		[BsonElement("Account")]
		[BsonRequired]
		public string AccountRef {
			get { return Account == null ? null : Account.Id; }
			set { Account = ArtheaEngine.Model.Account.Lookup (value); }
		}

		/// <summary>
		/// Gets or sets the account.
		/// </summary>
		/// <value>
		/// The account.
		/// </value>
		[BsonIgnore]
        public Account Account
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the channel flags.
		/// </summary>
		/// <value>
		/// The channel flags.
		/// </value>
		[BsonDefaultValue(ChannelFlag.None)]
		[BsonIgnoreIfDefault]
        public ChannelFlag ChannelFlags
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the class reference.
		/// </summary>
		/// <value>
		/// The class reference.
		/// </value>
		[BsonElement("Class")]
		[BsonRequired]
		public string ClassRef {
			get { return Class == null ? null : Class.Id; }
			set { Class = Class.Lookup (value); }
		}

		/// <summary>
		/// Gets or sets the class.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		[BsonIgnore]
        public Class Class
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the combat prompt.
		/// </summary>
		/// <value>
		/// The combat prompt.
		/// </value>
		[BsonIgnoreIfNull]
        public string CombatPrompt
        {
            get;
            set;
        }

		/// <summary>
		/// Gets the last note.
		/// </summary>
		/// <value>
		/// The last note.
		/// </value>
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, DateTime> LastNote
        {
            get; private set;
        }

		/// <summary>
		/// Gets or sets the prompt.
		/// </summary>
		/// <value>
		/// The prompt.
		/// </value>
		[BsonIgnoreIfNull]
        public string Prompt
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>
		/// The title.
		/// </value>
		[BsonIgnoreIfNull]
        public string Title
        {
            get; set;
        }

		/// <summary>
		/// Gets the world.
		/// </summary>
		/// <value>
		/// The world.
		/// </value>
		[BsonIgnore]
        public override World World
        {
            get { return Account.Connection.World; }
        }

		/// <summary>
		/// Save this instance.
		/// </summary>
		public override bool Save ()
		{
			if(Account != null)
				Account.Save();

			return base.Save();
		}

		/// <summary>
		/// Displaies the prompt.
		/// </summary>
        public void DisplayPrompt()
        {
            var pos = Prompt.IndexOf('%');

            if (pos == -1)
            {
                Write(Prompt);
                return;
            }

            var buf = new StringBuilder();
            var last = 0;

            do
            {
                buf.Append(Prompt.Substring(last, pos - last));

                switch (Prompt[++pos])
                {
                    case 'l':
                        buf.Append(Vitals.Life);
                        break;
                    case 'L':
                        buf.Append(Vitals.MaxLife);
                        break;
                    case 'm':
                        buf.Append(Vitals.Mana);
                        break;
                    case 'M':
                        buf.Append(Vitals.MaxMana);
                        break;
                    case 'e':
                        buf.Append(Vitals.Energy);
                        break;
                    case 'E':
                        buf.Append(Vitals.MaxEnergy);
                        break;
                }

                last = ++pos;

                pos = Prompt.IndexOf('%', last);
            }
            while (pos != -1);

            buf.Append(Prompt.Substring(last));

            Write(buf);
        }

		/// <summary>
		/// Determines whether this instance has read note the specified note.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance has read note the specified note; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='note'>
		/// Note.
		/// </param>
        public bool HasReadNote(Note note)
        {
            if (!LastNote.ContainsKey(note.Forum.Id))
                return false;

            return note.When < LastNote[note.Forum.Id];
        }

		/*
        public override bool Load(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand("read_player"))
            {
                cmd.AddParameter("@id", DbType.Int64, Id);

                if (!this.Load(cmd))
                    return false;
            }

            LoadCarrying(conn);

            if (Account.Login == null)
            {
                Account.Load();
            }
            return true;
        }

        public override int MapRow(IDataRecord reader)
        {
            var i = base.MapRow(reader);

            Room = Room.Lookup(reader.GetInt64(i++));
            Title = reader.GetStringOrNull(i++);
            ChannelFlags = reader.GetEnum<ChannelFlag>(i++);

            if (Account == null)
            {
                Account = new Account();
                Account.Id = reader.GetInt64(i++);
            }
            else
            {
                if (Account.Id != reader.GetInt64(i++))
                    LogManager.GetCurrentClassLogger().Warn("Loading player for different account!");
            }
            Class = Class.Lookup(reader.GetInt32(i++));
            if (Class == null)
                Class = new MageClass();

            Vitals.Life = reader.GetInt64(i++);
            Vitals.MaxLife = reader.GetInt64(i++);
            Vitals.Mana = reader.GetInt64(i++);
            Vitals.MaxMana = reader.GetInt64(i++);
            Vitals.Energy = reader.GetInt64(i++);
            Vitals.MaxEnergy = reader.GetInt64(i++);

            return i;
        }


        public override bool Save(IDbConnection conn)
        {
            if (!base.Save(conn))
                return false;

            Account.Save(conn);

            var cmd = conn.CreateCommand("save_player");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);
            cmd.AddParameter("@title", DbType.String, Title);
            cmd.AddParameter("@chan_flags", DbType.String, ChannelFlags.ToString());
            cmd.AddParameter("@account", DbType.Int64, Account.Id);
            cmd.AddParameter("@class", DbType.Int32, Class.Id);
            cmd.AddParameter("@life", DbType.Int64, Vitals.Life);
            cmd.AddParameter("@max_life", DbType.Int64, Vitals.MaxLife);
            cmd.AddParameter("@mana", DbType.Int64, Vitals.Mana);
            cmd.AddParameter("@max_mana", DbType.Int64, Vitals.MaxMana);
            cmd.AddParameter("@energy", DbType.Int64, Vitals.Energy);
            cmd.AddParameter("@max_energy", DbType.Int64, Vitals.MaxEnergy);

            var res =  cmd.ExecuteNonQuery() > 0;

            foreach (var obj in Carrying)
            {
                cmd = conn.CreateCommand("save_char_obj");

                cmd.AddParameter("@id", DbType.Int64, obj.Id);
                cmd.AddParameter("@char_id", DbType.Int64, obj.Character.Id);
                cmd.AddParameter("@obj_id", DbType.Int64, obj.Object.Id);
                cmd.AddParameter("@wearloc", DbType.String, obj.WearLocation.ToString());

                res = res && cmd.ExecuteNonQuery() > 0;
            }

            return res;
        }

		*/
        public override void Page(string buf)
        {
            if (Account != null)
                Account.Page(buf);
        }

		/// <summary>
		/// Page the specified buf.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		public override void Page (StringBuilder buf)
		{
			if(Account != null)
				Account.Page(buf);
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
        public override void Write(string format, params object[] args)
        {
            if (Account != null)
            {
                Account.Write(format, args);
            }
        }

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
        public override void Write(byte[] value)
        {
            if (Account == null) return;

            Account.Write(value);
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
        public override void Write(object obj)
        {
            if (Account == null) return;

            Account.Write(obj);
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
        public override void WriteLine()
        {
            if (Account == null) return;

            Account.WriteLine();
        }

		/// <summary>
		/// Writes the line.
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
        public override void WriteLine(object obj)
        {
            if (Account == null) return;

            Account.WriteLine(obj);
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
        public override void WriteLine(string format, params object[] args)
        {
            if (Account == null) return;

            Account.WriteLine(format, args);
        }
    }
}
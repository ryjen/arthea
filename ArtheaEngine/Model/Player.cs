using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using NLog;

namespace ArtheaEngine.Model
{

    #region Enumerations

    [Flags]
    public enum ChannelFlag
    {
        None
    }

    [Flags]
    public enum PcFlag
    {
        None
    }

    #endregion Enumerations

    public class Player : Character
    {
        #region Constructors

		public Player() : base()
        {
            LastNote = new Dictionary<BsonValue, DateTime>();
            Prompt = "~R%l/%L ~M%m/%M ~B%e/%E~x";
            CombatPrompt = Prompt;
        }

        #endregion Constructors

        #region Properties

		[BsonElement("Account")]
		[BsonRequired]
		public string AccountRef {
			get { return Account == null ? null : Account.Id; }
			set { Account = ArtheaEngine.Model.Account.Lookup (value); }
		}

		[BsonIgnore]
        public Account Account
        {
            get; set;
        }

		[BsonDefaultValue(ChannelFlag.None)]
		[BsonIgnoreIfDefault]
        public ChannelFlag ChannelFlags
        {
            get; set;
        }

		[BsonElement("Class")]
		[BsonRequired]
		public string ClassRef {
			get { return Class == null ? null : Class.Id; }
			set { Class = Class.Lookup (value); }
		}

		[BsonIgnore]
        public Class Class
        {
            get; set;
        }

		[BsonIgnoreIfNull]
        public string CombatPrompt
        {
            get;
            set;
        }

		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<BsonValue, DateTime> LastNote
        {
            get; private set;
        }

		[BsonIgnoreIfNull]
        public string Prompt
        {
            get;
            set;
        }

		[BsonIgnoreIfNull]
        public string Title
        {
            get; set;
        }

		[BsonIgnore]
        public override World World
        {
            get { return Account.Connection.World; }
        }

        #endregion Properties

        #region Methods
		
		public override bool Save ()
		{
			if(Account != null)
				Account.Save();

			return base.Save();
		}

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

        public override void Dispose()
        {
            base.Dispose();
        }

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

		public override void Page (StringBuilder buf)
		{
			if(Account != null)
				Account.Page(buf);
		}

        public override void Write(string format, params object[] args)
        {
            if (Account != null)
            {
                Account.Write(format, args);
            }
        }

        public override void Write(byte[] value)
        {
            if (Account == null) return;

            Account.Write(value);
        }

        public override void Write(object obj)
        {
            if (Account == null) return;

            Account.Write(obj);
        }

        public override void WriteLine()
        {
            if (Account == null) return;

            Account.WriteLine();
        }

        public override void WriteLine(object obj)
        {
            if (Account == null) return;

            Account.WriteLine(obj);
        }

        public override void WriteLine(string format, params object[] args)
        {
            if (Account == null) return;

            Account.WriteLine(format, args);
        }

        #endregion Methods
    }
}
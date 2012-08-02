using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtheaEngine.Model
{
    using System;
    using System.Data;
    using System.Data.Odbc;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using NLog;

    #region Enumerations

    public enum AffectFlag : long
    {
        None
    }

    public enum AffectLocation
    {
        None,
        AC,
        Hit,
        Mana,
        Move
    }

    #endregion Enumerations

    public class Affect : IEntity, IFormattable
    {
		public Affect()
		{	
			Id = ObjectId.GenerateNewId().ToString();
		}
        #region Properties

        public int Duration
        {
            get; set;
        }

        public AffectFlag Flag
        {
            get; set;
        }

		[BsonId]
        public string Id
        {
            get; set;
        }

        public AffectLocation Location
        {
            get; set;
        }

        public int Modifier
        {
            get; set;
        }

		public string Extra {
			get;
			set;
		}

        #endregion Properties

        #region Methods

        public override bool Equals(object obj)
        {
            var a = obj as Affect;
            if (a == null)
            {
                return false;
            }
            return Flag == a.Flag && Modifier == a.Modifier && Location == a.Location;
        }

        public override int GetHashCode()
        {
            return ((int)Flag) + Modifier + Duration + (int)Location;
        }

		/*
        public bool Load(IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand("read_affect"))
            {
                cmd.AddParameter("@id", DbType.Int64, Id);

                return this.Load(cmd);
            }
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            Location = reader.GetEnum<AffectLocation>(i++);
            Flag = reader.GetEnum<AffectFlag>(i++);
            Modifier = reader.GetInt16(i++);
            Duration = reader.GetInt16(i++);
            Extra = reader.GetInt32(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_affect");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@loc", DbType.String, Location.ToString());
            cmd.AddParameter("@flag", DbType.String, Flag.ToString());
            cmd.AddParameter("@mod", DbType.Int32, Modifier);
            cmd.AddParameter("@dur", DbType.Int32, Duration);
            cmd.AddParameter("@extra", DbType.Int32, Extra);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
                Id = Convert.ToInt64(conn.LastInsertId());

            return res;
        }*/

        public override string ToString()
        {
            string temp;

            if (Location != AffectLocation.None)
                temp = string.Format(" modifies {0} by {1}", Location.ToString().ToLower(), Modifier);
            else if (Flag != AffectFlag.None)
                temp = string.Format(" adds {0} affect", Flag.ToString());
            else
                temp = string.Empty;

            return string.Format("Spell {0}{1} for {2} hours.",
                                 (Extra == null ? "unknown" : Ability.Lookup(Extra).Name), temp, Duration);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || format == "F")
            {
				Ability ability = Ability.Lookup(Extra);

                var buf = new StringBuilder();
                buf.Append("Affects ");
                if(Flag != AffectFlag.None)
                    buf.Append(Flag);
                else if(ability != null)
                    buf.Append(ability.Name);
                else
                    buf.Append("unknown");
                if(Location != AffectLocation.None)
                {
                    buf.Append(", modifies").Append(Location);
                    buf.Append(" by ").Append(Modifier);
                }
                if(Duration == -1)
                    buf.Append("permanently");
                else
                    buf.AppendFormat("for {0} hours", Duration);

                return buf.ToString();
            }

            var prop = GetType().GetProperty(format, BindingFlags.Public | BindingFlags.Instance
                                                     | BindingFlags.IgnoreCase);

            if (prop != null)
            {
                return prop.GetValue(this, null).ToString();
            }

            throw new FormatException("unknown format");
        }

        #endregion Methods
    }
}
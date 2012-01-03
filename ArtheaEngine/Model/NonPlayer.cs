namespace ArtheaEngine.Model
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Text;

    using NLog;

    #region Enumerations

    [Flags]
    public enum NpcFlag : long
    {
        None, Aggressive, Guildmaster, Sentinel, StayArea, Wimpy
    }

    #endregion Enumerations

    public class NonPlayer : Character
    {
        #region Properties

        public Area Area
        {
            get; set;
        }

        public NpcFlag Flags
        {
            get; set;
        }

        public string LongDescr
        {
            get; set;
        }

        public Shop Shop
        {
            get; set;
        }

        public string ShortDescr
        {
            get; set;
        }

        public override World World
        {
            get { return Area.World; }
        }

        #endregion Properties

        #region Methods

        public static NonPlayer Load(long id)
        {
            var npc = new NonPlayer();
            npc.Id = id;
            npc.Load();

            return npc;
        }

        public override bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_nonplayer");
            cmd.AddParameter("@id", DbType.Int64, Id);

            return cmd.ExecuteNonQuery() == 1 && base.Delete(conn);
        }

        public override bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_nonplayer");

            cmd.AddParameter("@id", DbType.Int64, Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            return true;
        }

        public override int MapRow(IDataRecord reader)
        {
            var i = base.MapRow(reader);

            ShortDescr = reader.GetString(i++);
            LongDescr = reader.GetString(i++);
            Area = Area.Lookup(reader.GetInt32(i++));

            return i;
        }

        public override void Page(string buf)
        {
        }

        public override bool Save(IDbConnection conn)
        {
            if (!base.Save(conn))
                return false;

            var cmd = conn.CreateCommand("save_nonplayer");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@short_descr", DbType.String, ShortDescr);
            cmd.AddParameter("@long_descr", DbType.String, LongDescr);
            cmd.AddParameter("@area", DbType.Int32, Area.Id);

            var res = cmd.ExecuteNonQuery() > 0;

            return res;
        }

        public override void Write(string format, params object[] args)
        {
        }

        public override void Write(byte[] value)
        {
        }

        public override void Write(object obj)
        {
        }

        public override void WriteLine()
        {
        }

        public override void WriteLine(object obj)
        {
        }

        public override void WriteLine(string format, params object[] args)
        {
        }

        #endregion Methods
    }
}
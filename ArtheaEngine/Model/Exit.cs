namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq.Mapping;
    using System.Data.Odbc;

    using NLog;

    #region Enumerations

    public enum Direction
    {
        North,
        South,
        East,
        West,
        Up,
        Down
    }

    [Flags]
    public enum ExitFlag
    {
        None, Closed, Door, Locked, NoPass, PickProof
    }

    #endregion Enumerations

    public class Exit : IEntity<long>, IPersistent
    {
        #region Properties

        public string Description
        {
            get; set;
        }

        public Direction Direction
        {
            get; set;
        }

        public ExitFlag Flags
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public long? Key
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public Room Room
        {
            get; set;
        }

        public Room ToRoom
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_exit");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);
            cmd.AddParameter("@dir", DbType.String, Direction.ToString());

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            //var reader = conn.ExecuteReader("read_exit", DbType.Int64, Id, DbType.Int64, Room.Id, DbType.String, Direction);
            var cmd = conn.CreateCommand("read_exit");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);
            cmd.AddParameter("@dir", DbType.String, Direction.ToString());

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            MapRow(reader);

            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            Id = reader.GetInt64(i++);
            ToRoom = Room.Lookup(reader.GetInt64(i++));
            Key = reader.GetInt64(i++);
            Flags = reader.GetEnum<ExitFlag>(i++);
            Name = reader.GetString(i++);
            Description = reader.GetString(i++);
            i++;
            //Room = Room.List.SingleOrDefault(x => x.Id = reader.GetString(6));
            Direction = reader.GetEnum<Direction>(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_exit");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@to_room", DbType.Int64, ToRoom.Id);
            if (Key.HasValue)
                cmd.AddParameter("@key", DbType.Int64, Key.Value);
            else
                cmd.AddParameter("@key", DbType.Int64, DBNull.Value);

            cmd.AddParameter("@flags", DbType.String, Flags.ToString());
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@room", DbType.Int64, Room.Id);
            cmd.AddParameter("@dir", DbType.String, Direction.ToString());

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
                Id = Convert.ToInt64(conn.LastInsertId());

            return res;
        }

        #endregion Methods
    }
}
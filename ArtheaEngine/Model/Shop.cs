namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Odbc;
    using System.Linq;
    using System.Text;

    public class Shop : IEntity<long>, IPersistent
    {
        #region Properties

        public short CloseHour
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public short OpenHour
        {
            get; set;
        }

        public Character Owner
        {
            get; set;
        }

        public short ProfitBuy
        {
            get; set;
        }

        public short ProfitSell
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_shop");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@owner", DbType.Int64, Owner.Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_shop");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@owner", DbType.Int64, Owner.Id);

            return this.Load(cmd);
        }

        public int MapRow(IDataRecord reader)
        {
            int i = 0;

            ProfitBuy = reader.GetInt16(i++);
            ProfitSell = reader.GetInt16(i++);
            OpenHour = reader.GetInt16(i++);
            CloseHour = reader.GetInt16(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("update_shop");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@pb", DbType.Int16, ProfitBuy);
            cmd.AddParameter("@ps", DbType.Int16, ProfitSell);
            cmd.AddParameter("@oh", DbType.Int16, OpenHour);
            cmd.AddParameter("@ch", DbType.Int16, CloseHour);
            cmd.AddParameter("@owner", DbType.Int64, Owner.Id);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
                Id = Convert.ToInt64(conn.LastInsertId());

            return res;
        }

        #endregion Methods
    }
}
namespace ArtheaEngine
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Odbc;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public interface ILoadable
    {
        #region Methods

        bool Load(IDbConnection conn);

        int MapRow(IDataRecord reader);

        #endregion Methods
    }

    public interface IPersistent : ISaveable, ILoadable
    {
    }

    public interface ISaveable
    {
        #region Methods

        bool Delete(IDbConnection conn);

        bool Save(IDbConnection conn);

        #endregion Methods
    }
}
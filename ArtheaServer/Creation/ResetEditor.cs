namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine.Model;

    public class ResetEditor : Editor
    {
        #region Fields

        //private Reset _data;

        #endregion Fields

        #region Constructors

        public ResetEditor(TelnetConnection conn, Reset reset)
            : base(conn)
        {
            //_data = reset;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { throw new NotImplementedException(); }
        }

        #endregion Properties

        #region Methods

        public override void Save()
        {
            throw new NotImplementedException();
        }

        public override void Show()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
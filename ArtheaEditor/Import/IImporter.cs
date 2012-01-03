namespace ArtheaEditor.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public interface IImporter
    {
        #region Methods

        void Commit();

        void Process(FileInfo fi);

        #endregion Methods
    }
}
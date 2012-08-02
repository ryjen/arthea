namespace ArtheaEditor.Import.Rom
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public abstract class FileSection
    {
        #region Methods

        public abstract void Commit();

        public abstract void Load(FileReader reader);

        #endregion Methods
    }
}
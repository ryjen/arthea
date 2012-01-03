namespace ArtheaEditor.Import
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    public class ImportException : Exception
    {
        #region Constructors

        public ImportException(string msg)
            : base(msg)
        {
        }

        public ImportException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        public ImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ImportException()
        {
        }

        #endregion Constructors
    }
}
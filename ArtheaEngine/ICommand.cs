namespace ArtheaEngine
{
    using System;
    using System.Runtime.Serialization;

    using ArtheaEngine.Model;

    public interface ICommand
    {
        #region Properties

        string Description
        {
            get;
        }

        string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool CanUse(Character ch);

        void Execute(Character ch, Argument argument);

        #endregion Methods
    }

    public class RebootException : Exception
    {
        #region Constructors

        public RebootException()
            : base()
        {
        }

        public RebootException(string msg)
            : base(msg)
        {
        }

        public RebootException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        public RebootException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }

    public class ShutdownException : Exception
    {
        #region Constructors

        public ShutdownException()
            : base()
        {
        }

        public ShutdownException(string msg)
            : base(msg)
        {
        }

        public ShutdownException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        public ShutdownException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}
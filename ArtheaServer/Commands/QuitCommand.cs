using NLog;

namespace ArtheaServer.Commands
{
    using ArtheaEngine;
    using ArtheaEngine.Model;
    using System;
    using System.Runtime.Serialization;

    public class QuitCommand : Command
    {
        #region Fields

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public QuitCommand()
            : base("quit", "Exits the game.")
        {
        }

        #endregion Constructors

        #region Methods

        public override void Execute(Character ch, Argument argument)
        {
            if (!(ch is Player))
                return;

            ch.WriteLine("Good-bye!");
            log.Info(ch.Name + " has logged out");
            throw new QuitCommandException();
        }

        #endregion Methods
    }

    [Serializable]
    public class QuitCommandException : Exception
    {
        #region Constructors

        public QuitCommandException()
        {
        }

        public QuitCommandException(string message)
            : base(message)
        {
        }

        public QuitCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected QuitCommandException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}
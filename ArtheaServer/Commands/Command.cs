using System;
using System.Collections.Generic;
using System.Reflection;
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer.Commands
{
    public abstract class Command : IComparable<Command>, IEqualityComparer<ICommand>
    {
        #region Fields

        protected static readonly ISet<Command> Commands = new HashSet<Command>();

        #endregion Fields

        #region Constructors

        static Command()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsSubclassOf(typeof (Command)) || type.IsAbstract)
                    continue;

                Commands.Add((Command) Activator.CreateInstance(type));
            }
        }

        protected Command(string name)
        {
            Name = name;
            Level = 0;
            Aliases = new HashSet<string>();
        }

        protected Command(string name, string descr)
            : this(name)
        {
            Description = descr;
        }

        protected Command(string name, string descr, short level)
        {
            Name = name;
            Description = descr;
            Level = level;
            Aliases = new HashSet<string>();
        }

        #endregion Constructors

        #region Properties

        public HashSet<string> Aliases { get; protected set; }

        public string Description { get; protected set; }

        public short Level { get; protected set; }

        public string Name { get; protected set; }

        protected byte ListPriority { get; set; }

        #endregion Properties

        #region Methods

        #region IComparable<Command> Members

        public int CompareTo(Command other)
        {
            return Name.CompareTo(other.Name) & ListPriority.CompareTo(other.ListPriority);
        }

        #endregion

        #region IEqualityComparer<ICommand> Members

        public bool Equals(ICommand x, ICommand y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(ICommand obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion

        public static bool Interpret(Character ch, Argument argument)
        {
            if (argument.IsEmpty())
            {
                return true;
            }

            if (Social.Interpret(ch, argument))
            {
                return true;
            }

			if (Ability.Interpret(ch, argument))
			{
				return true;
			}

            string arg = argument.Next();
            foreach (Command cmd in Commands)
            {
                if (!cmd.CanUse(ch))
                    continue;

                if (cmd.Name.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    cmd.Execute(ch, argument);
                    return true;
                }

                foreach (string alias in cmd.Aliases)
                {
                    if (alias.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                    {
                        cmd.Execute(ch, argument);
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool CanUse(Character ch)
        {
            return Level <= ch.Level;
        }

        public abstract void Execute(Character ch, Argument argument);

        #endregion Methods
    }
}
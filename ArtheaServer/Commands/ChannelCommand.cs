namespace ArtheaServer.Commands
{

    using ArtheaEngine;
    using ArtheaEngine.Model;

    #region Enumerations

    public enum ChannelPermission
    {
        Public,
        Admin
    }

    public enum ChannelType
    {
        Normal,
        Who,
        Emote,
        Think
    }

    #endregion Enumerations

    public abstract class ChannelCommand : Command
    {
        #region Constructors

        protected ChannelCommand(string name, string description)
            : base(name, description)
        {
        }

        protected ChannelCommand(string name, string description, short level)
            : base(name, description, level)
        {
        }

        #endregion Constructors

        #region Properties

        public abstract string Color
        {
            get;
        }

        public abstract string DisplayName
        {
            get;
        }

        public abstract ChannelFlag Flag
        {
            get;
        }

        public abstract ChannelPermission Permission
        {
            get;
        }

        #endregion Properties

        #region Methods

        public bool CanUseChannel(Character ch)
        {
            switch (Permission)
            {
                case ChannelPermission.Public:
                    return true;
                case ChannelPermission.Admin:
                    return (ch is Player) && (ch.Level >= Game.Immortal);
                default:
                    return false;
            }
        }

        public override void Execute(Character ch, Argument argument)
        {
            if (!argument)
            {
                var player = ch as Player;

                if (player != null)
                {
                    player.ChannelFlags ^= Flag;

                    player.WriteLine("{0} channel turned {1}.", Name.Capitalize(),
                                     player.ChannelFlags.HasFlag(Flag) ? "off" : "on");
                }

                return;
            }
            else
            {
                var player = ch as Player;
                if (player != null && player.ChannelFlags.HasFlag(Flag))
                {
                    player.ChannelFlags &= ~Flag;
                }
            }

            var type = ChannelType.Normal;
            argument = argument.Replace("~x", Color);

            switch (argument[0])
            {
                case '+':
                    argument.Next();

				    var s = Social.Lookup(argument.Next());

                    if (s == null)
                    {
                        ch.WriteLine("No such social!");
                        return;
                    }

                    if (!argument)
                    {
                        ProcessChannelSocial(ch, null, s.CharNoArg);
                        return;
                    }

                    IEntity arg = Character.Lookup(argument);

                    if (arg != null)
                    {
                        ProcessChannelSocial(ch, (arg == ch) ? s.CharSelf : s.CharFound, arg);
                        return;
                    }

                    arg = Object.Lookup(argument);

                    if (arg != null)
                    {
                        ProcessChannelSocial(ch, s.CharObjFound, arg);
                        return;
                    }

                    ch.WriteLine("That character or item not found.");
                    return;

                case '!':
                    type = ChannelType.Emote;
                    argument.Next();
                    ch.WriteLine("{0}{1} {2} {3}.~x", DisplayName, Color, ch, argument);
                    break;

                case '@':
                    type = ChannelType.Think;
                    argument.Next();
                    ch.WriteLine("{0}{1} {2} . o O ({3})~x", DisplayName, Color, ch, argument);
                    break;

                case '-':
                    switch (argument)
                    {
                        case "--help":
                        case "-h":
                            ch.WriteLine("{0}:");
                            ch.WriteLine("\t{0,-10}: Displays this help", "--help,-h");
                            ch.WriteLine("\t{0,-10}: Displays who can see this channel", "--who,-w");
                            ch.WriteLine("\t{0,-10}: Displays the channel history", "--history,-h");
                            ch.WriteLine("\t{0,-10}: Displays a social over the channel", "+ <social>");
                            ch.WriteLine("\t{0,-10}: Displays an emote over the channel", "! <emote>");
                            ch.WriteLine("\t{0,-10}: Displays a thought over the channel", "@ <thought>");
                            return;
                        case "--who":
                        case "-w":
                            type = ChannelType.Who;
                            ch.WriteLine("Players listening on {0}:~x", DisplayName);
                            break;
                        case "--history":
                        case "-H":
                            ch.WriteLine("Not implemented yet.");
                            return;
                        default:
                            ch.WriteLine("{0}{1} You say '{2}'~x", DisplayName, Color, argument);
                            break;
                    }
                    break;

                default:
                    ch.WriteLine("{0}{1} You say '{2}'~x", DisplayName, Color, argument);
                    break;
            }

            foreach (var player in ch.World.Players)
            {
                if (type == ChannelType.Who || player == ch || !CanUseChannel(player))
                    continue;

                switch (type)
                {
                    case ChannelType.Normal:
                        player.WriteLine("{0}{1} {2} says '{3}'~x", DisplayName, Color, ch, argument);
                        break;
                    case ChannelType.Emote:
                        player.WriteLine("{0}{1} {2} {3}.~x", DisplayName, Color, ch, argument);
                        break;
                    case ChannelType.Think:
                        player.WriteLine("{0}{1} {2} . o O ({3})~x", DisplayName, Color, ch, argument);
                        break;
                    case ChannelType.Who:
                        ch.WriteLine("{0}", player);
                        break;
                }
            }
        }

        private void ProcessChannelSocial(Character ch, string argument, params object[] args)
        {
            foreach (var wch in ch.World.Players)
            {
                if (ch == wch || CanUseChannel(wch))
                {
                    wch.Act(string.Format("{0} {1}{2}~x",
                                                        DisplayName, Color, argument), args);
                }
            }
        }

        #endregion Methods
    }
}
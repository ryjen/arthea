namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public class CharEditor : Editor
    {
        #region Fields

        private Character _data;

        #endregion Fields

        #region Constructors

        public CharEditor(TelnetConnection conn, Character data)
            : base(conn)
        {
            _data = data;
        }

        #endregion Constructors

        #region Properties

        public override string Title
        {
            get { return string.Format("Character {0}", _data.Id); }
        }

        #endregion Properties

        #region Methods

        public override bool Execute(Argument argument)
        {
            var arg = argument.Next();

            if (base.Execute(arg))
                return true;

            if (arg.Is("A") || arg.Is("name"))
            {
                _data.Name = argument;
                Show();
                return true;
            }

            if (arg.Is("E") || arg.Is("sex"))
            {
                Sex sex;
                if (!Enum.TryParse<Sex>(argument, true, out sex))
                {
                    ShowValidEnumValues<Sex>();
                    return true;
                }
                _data.Sex = sex;
                Show();
                return true;
            }

            if (arg.Is("B") || arg.Is("description"))
            {
                var tmp = new TextEditor(_connection, _data, PropertyHelper<Room>.GetProperty(x => x.Description));
                Forward(tmp);
                return true;
            }

            if (arg.Is("C") || arg.Is("size"))
            {
                if (!arg.IsNumber())
                {
                    Size size;
                    if (!Enum.TryParse<Size>(argument, true, out size))
                    {
                        ShowValidEnumValues<Size>();
                        WriteLine("~CYou may also enter a number in the form of #.#~x");
                        return true;
                    }
                    _data.Size = (float) size;
                }
                else
                {
                    float size;
                    if (!float.TryParse(argument, out size))
                    {
                        WriteLine("Invalid size");
                        return true;
                    }
                    if (size > (float)Size.Giant || size < 0)
                    {
                        WriteLine("~CValid sizes are from 0 to {0}", (float)Size.Giant);
                        return true;
                    }
                    _data.Size = size;
                }
                Show();
                return true;
            }

            if (arg.Is("D") || arg.Is("position"))
            {
                Position pos;
                if (!Enum.TryParse<Position>(argument, true, out pos))
                {
                    ShowValidEnumValues<Position>();
                    return true;
                }

                _data.Position = pos;
                Show();
                return true;
            }

            if (arg.Is("F") || arg.Is("race"))
            {
                Race race = Race.Lookup(argument);

                if (race == null)
                {
                    WriteLine("Invalid race");
                    return true;
                }

                _data.Race = race;
                Show();
                return true;
            }

            if (arg.Is("G") || arg.Is("affects"))
            {
                var ed = new AffectsEditor(_connection, _data, PropertyHelper<Character>.GetProperty(x => x.Affects));
                Forward(ed);
                return true;
            }

            if (_data is Player)
            {
                var pc = _data as Player;

                if (arg.Is("H") || arg.Is("title"))
                {
                    pc.Title = argument;
                    Show();
                    return true;
                }

                if (arg.Is("I") || arg.Is("channelflags"))
                {
                    ChannelFlag bit;
                    if (!Enum.TryParse(argument, true, out bit))
                    {
                        ShowValidEnumValues<ChannelFlag>();
                        return true;
                    }
                    pc.ChannelFlags ^= bit;
                    Show();
                    return true;
                }

                if (arg.Is("J") || arg.Is("class"))
                {
                    Class c = Class.Lookup(argument);

                    if (c == null)
                    {
                        WriteLine("Invalid class");
                        return true;
                    }

                    pc.Class = c;
                    Show();
                    return true;
                }
            }

            if (_data is NonPlayer)
            {
                var npc = _data as NonPlayer;

                if (arg.Is("H") || arg.Is("shortdescr"))
                {
                    npc.ShortDescr = argument;
                    Show();
                    return true;
                }

                if (arg.Is("I") || arg.Is("longdescr"))
                {
                    npc.LongDescr = argument;
                    Show();
                    return true;
                }

                if (arg.Is("J") || arg.Is("flags"))
                {
                    NpcFlag bit;
                    if (!Enum.TryParse(argument, true, out bit))
                    {
                        ShowValidEnumValues<NpcFlag>();
                        return true;
                    }

                    npc.Flags ^= bit;
                    Show();
                    return true;
                }

                if (arg.Is("K") || arg.Is("shop"))
                {
                    //var ed = new ShopEditor(_connection, _data, PropertyHelper<NonPlayer>.GetProperty(x => x.Shop));
                    // Forward(ed);
                    return true;
                }
            }

            return false;
        }

        public override void Save()
        {
			_data.Save();
        }

        public override void Show()
        {
            Reset();

            ColumnFormatter cols = new ColumnFormatter(2, _connection.ScreenWidth);

            cols.Add("~YA) ~CName: ~W{0}", _data.Name);
			cols.SetSpan (2);
			cols.Add(TextEditor.Preview("~YB) ~CDescription", _data.Description, cols.Width));
            cols.Add("~YC) ~CSize: ~W{0}", _data.Size);
            cols.Add("~YD) ~CPosition: ~W{0}", _data.Position);
            cols.Add("~YE) ~CSex: ~W{0}", _data.Sex);
            cols.Add("~YF) ~CRace: ~W{0}", _data.Race.Name);

            cols.Add("~RG) ~CAffects: ~W{0}~C affects", _data.Affects.Count);

            if (_data is Player)
            {
                var p = _data as Player;
                cols.Add("~YH) ~CTitle: ~W{0}", p.Title);
                cols.Add("~YI) ~CChannelFlags: ~W{0}", p.ChannelFlags);
                cols.Add("   ~CAccount: ~W{0}", p.Account.Login);
                cols.Add("~YJ) ~CClass: ~W{0}", p.Class.Name);
            }
            if (_data is NonPlayer)
            {
                var npc = _data as NonPlayer;
                cols.Add("~YH) ~CShort Descr: ~W{0}", npc.ShortDescr);
                cols.Add("~YI) ~CLong Descr: ~W{0}", npc.LongDescr);
                cols.Add("   ~CArea: ~W{0}", npc.Area.Name);
                cols.Add("~YJ) ~CFlags: ~W{0}", npc.Flags);
                cols.Add("~RK) ~CShop: ~W{0}", npc.Shop == null ? "None" : npc.Shop.Id);
            }

			_connection.Page(cols.GetBuffer());
        }

        #endregion Methods
    }
}
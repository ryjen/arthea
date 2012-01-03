namespace ArtheaEditor.Import.Rom
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using NLog;

    public class MobileSection : FileSection
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public MobileSection()
        {
            Loaded = new Dictionary<int, NonPlayer>();
        }

        #endregion Constructors

        #region Properties

        public Dictionary<int, NonPlayer> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit(IDbConnection conn)
        {
            foreach (var npc in Loaded)
                {
                    npc.Value.Save(conn);
                }
        }

        public override void Load(FileReader reader)
        {
            Load(reader, true);
        }

        public void Load(FileReader reader, bool newFormat)
        {
            if (RomImporter.CurrentArea == null)
            {
                throw new ImportException("LoadMobiles: area not seen yet.");
            }

            if (newFormat)
                LoadNewFormat(reader);
            else
                LoadOldFormat(reader);

            Log.Info("loaded {0} mobiles", Loaded.Count);
        }

        public void LoadNewFormat(FileReader reader)
        {
            while (!reader.EndOfFile)
            {
                if (reader.ReadLetter() != '#')
                {
                    throw new ImportException("LoadRomMobiles: # not found");
                }

                var num = reader.ReadNumber();

                if (num == 0)
                    break;

                var mob = new NonPlayer();

                mob.Area = RomImporter.CurrentArea;
                mob.Name = reader.ReadString();
                mob.ShortDescr = reader.ReadString();
                mob.LongDescr = reader.ReadString();
                mob.Description = reader.ReadString();
                mob.Race = Race.List.FirstOrDefault(r => r.Name.HasWord(reader.ReadString()));
                if (mob.Race == null)
                    mob.Race = Race.Lookup("unique");

                mob.Flags |= Converter.ConvertActFlags(reader.ReadFlag()); // act flags
                reader.ReadFlag(); // affect flags;

                reader.ReadNumber(); // alignment
                reader.ReadNumber(); // group

                mob.Level = (short)reader.ReadNumber();

                reader.ReadNumber(); //hitroll

                /* hit dice */
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();

                /* mana dice */
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();

                /* damage dice */
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();
                reader.ReadLetter();
                reader.ReadNumber();

                reader.ReadWord(); // dam type

                for (var i = 0; i < 4; i++)
                    reader.ReadNumber(); // AC

                reader.ReadFlag(); // off bits
                reader.ReadFlag(); // imm bits
                reader.ReadFlag(); // res bits
                reader.ReadFlag(); // vuln bits
                mob.Position = Converter.ConvertPosition(reader.ReadWord());

                reader.ReadWord(); // default position
                mob.Sex = Converter.ConvertSex(reader.ReadWord());

                reader.ReadNumber(); // wealth

                reader.ReadFlag(); // form bits
                reader.ReadFlag(); // parts bits

                mob.Size = Converter.ConvertSize(reader.ReadWord());

                reader.ReadWord(); // material

                while (!reader.EndOfFile)
                {
                    var letter = reader.ReadLetter();

                    if (letter == 'F')
                    {
                        var word = reader.ReadWord();
                        reader.ReadFlag();

                        switch (word.ToLower())
                        {
                            case "aff":
                            case "par":
                            case "act":
                            case "off":
                            case "for":
                            case "imm":
                            case "res":
                            case "vul":
                                break;
                            default:
                                throw new ImportException("LoadRomMobiles: flag " + word + " not found");
                        }
                    }
                    else
                    {
                        reader.UnRead();
                        break;
                    }
                }

                Loaded.Add(num, mob);
            }
        }

        private void LoadOldFormat(FileReader reader)
        {
            for (;;) {
            char letter = reader.ReadLetter();
            if (letter != '#') {
                throw new ImportException("# not found");
            }
            int vnum = reader.ReadNumber();
            if (vnum == 0)
                break;

            // TODO: check mob exists

            NonPlayer npc = new NonPlayer();

            npc.Area = RomImporter.CurrentArea;
            npc.Name = reader.ReadString();
            npc.ShortDescr = reader.ReadString();
            npc.LongDescr = reader.ReadString();
            npc.Description = reader.ReadString();

            npc.Flags = Converter.ConvertActFlags(reader.ReadFlag());

            reader.ReadFlag(); // affected_by
            reader.ReadNumber(); // alignment

            letter = reader.ReadLetter();

            npc.Level = (short) reader.ReadNumber();

            reader.ReadNumber();
            reader.ReadNumber();
            reader.ReadNumber();
            reader.ReadLetter();
            reader.ReadNumber();
            reader.ReadLetter();
            reader.ReadNumber();

            reader.ReadNumber();
            reader.ReadLetter();
            reader.ReadNumber();
            reader.ReadLetter();
            reader.ReadNumber();

            reader.ReadNumber(); // wealth
            reader.ReadNumber(); // xp?

            npc.Position = Converter.ConvertPosition(reader.ReadNumber()); // start
                                                                                // position

            Converter.ConvertPosition(reader.ReadNumber()); // default
                                                                                // position

            npc.Sex = Converter.ConvertSex(reader.ReadNumber());

            Argument arg = new Argument(npc.Name);

            npc.Race = Converter.ConvertRace(arg.Next());

            if (npc.Race == null) {
                npc.Race = Race.Lookup("unique");
            }

            if (letter != 'S') {
                throw new ImportException("vnum " + vnum + " bad subformat");
            }

            Loaded.Add(vnum, npc);
            }
        }

        private int[] ReadDice(FileReader reader)
        {
            int[] dice = new int[3];

            dice[0] = reader.ReadNumber();
            reader.ReadLetter();
            dice[1] = reader.ReadNumber();
            reader.ReadLetter();
            dice[2] = reader.ReadNumber();

            return dice;
        }

        #endregion Methods
    }
}
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

    public class ResetSection : FileSection
    {
        #region Fields

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public ResetSection(RomImporter importer)
        {
            Loaded = new List<TempReset>();
            Importer = importer;
        }

        #endregion Constructors

        #region Properties

        public RomImporter Importer
        {
            get; set;
        }

        public IList<TempReset> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit()
        {
            foreach (TempReset temp in Loaded)
                {

                    StringBuilder buf = new StringBuilder();

                    switch (temp.command)
                    {
                        case 'M':
                            if (!Importer.Mobiles.Loaded.ContainsKey(temp.arg1))
                            {
                                Log.Warn("bad reset for mobile {0}", temp.arg1);
                                continue;
                            }
                            buf.Append("npc(");
                            buf.Append(Importer.Mobiles.Loaded[temp.arg1].Keyword());
                            buf.Append(",");
                            buf.Append(temp.arg4); // max room
                            buf.Append(")\n");
                            break;
                        case 'O':
                            if (!Importer.Objects.Loaded.ContainsKey(temp.arg1))
                            {
                                Log.Warn("bad reset for object {0}", temp.arg1);
                                continue;
                            }

                            buf.Append("obj(");
                            buf.Append(Importer.Objects.Loaded[temp.arg1].Keyword());
                            buf.Append(")\n");
                            break;
                        case 'D':
                        case 'R':
                        default:
                            throw new ImportException("invalid top level reset");
                    }

                    if (!Importer.Rooms.Loaded.ContainsKey(temp.arg3))
                    {
                        Log.Warn("bad reset for room {0}", temp.arg3);
                        continue;
                    }

                    Room room = Importer.Rooms.Loaded[temp.arg3];

                    foreach (TempReset sub in temp.subResets)
                    {
                        switch (sub.command)
                        {
                            case 'P':
                                if (!Importer.Objects.Loaded.ContainsKey(sub.arg1))
                                {
                                    throw new ImportException("bad put reset for object "
                                            + sub.arg1);
                                }
                                buf.Append("put(");
                                buf.Append(Importer.Objects.Loaded[sub.arg1].Keyword());
                                buf.Append(")\n");
                                break;
                            case 'G':
                                if (!Importer.Objects.Loaded.ContainsKey(sub.arg1))
                                {
                                    throw new ImportException("bad give reset for object "
                                            + sub.arg1);
                                }
                                buf.Append("give(");
                                buf.Append(Importer.Objects.Loaded[sub.arg1].Keyword());
                                buf.Append(")\n");
                                break;
                            case 'E':
                                if (!Importer.Objects.Loaded.ContainsKey(sub.arg1))
                                {
                                    throw new ImportException("bad equip reset for object "
                                            + sub.arg1);
                                }
                                buf.Append("equip(");
                                buf.Append(Importer.Objects.Loaded[sub.arg1].Keyword());
                                buf.Append(")\n");
                                break;
                        }
                    }

                    Reset reset = new Reset(room);

                    reset.Code = buf.ToString();

                    room.Reset = reset;

					DbManager.Save(room);
                }
        }

        public override void Load(FileReader reader)
        {
            if (RomImporter.CurrentArea == null)
            {
                throw new ImportException("LoadRomResets: area not seen yet");
            }

            TempReset iLastObj = null;
            TempReset iLastRoom = null;

            var count = 0;

            while (!reader.EndOfFile)
            {
                var letter = reader.ReadLetter();

                if (letter == 'S') break;

                if (letter == '*')
                {
                    reader.ReadLine();
                    continue;
                }

                TempReset temp = new TempReset();

                temp.command = letter;

                reader.ReadNumber(); // if_flag

                temp.arg1 = reader.ReadNumber();
                temp.arg2 = reader.ReadNumber();
                temp.arg3 = (letter == 'G' || letter == 'R')
                               ? 0
                               : reader.ReadNumber();
                temp.arg4 = (letter == 'P' || letter == 'M')
                               ? reader.ReadNumber()
                               : 0;

                reader.ReadToEol();

                //Reset reset = _roomIndexes[arg3].Reset;

                switch (letter)
                {
                    case 'M':
                        if (!Importer.Mobiles.Loaded.ContainsKey(temp.arg1))
                            throw new ImportException("Invalid character for reset " + temp.arg1);
                        if (!Importer.Rooms.Loaded.ContainsKey(temp.arg3))
                            throw new ImportException("Invalid room for reset " + temp.arg3);
                        iLastRoom = temp;
                        count++;
                        break;
                    case 'O':
                        if (!Importer.Objects.Loaded.ContainsKey(temp.arg1))
                            throw new ImportException("invalid object for reset "
                            + temp.arg1);
                        if (!Importer.Rooms.Loaded.ContainsKey(temp.arg3))
                            throw new ImportException("invalid room for reset "
                                    + temp.arg3);

                        iLastObj = temp;
                        count++;
                        break;
                    case 'P':
                        if (!Importer.Objects.Loaded.ContainsKey(temp.arg1))
                            throw new ImportException("invalid put for reset "
                            + temp.arg1);

                        if (iLastObj == null)
                            throw new ImportException(
                            "no previous object for put reset");

                        iLastObj.subResets.Add(temp);
                        count++;
                        break;
                    case 'E':
                    case 'G':
                        if (!Importer.Objects.Loaded.ContainsKey(temp.arg1))
                            throw new ImportException("invalid give/equip for reset"
                                    + temp.arg1);

                        if (iLastRoom == null)
                            throw new ImportException(
                                    "no previous reset for give/equip");

                        iLastRoom.subResets.Add(temp);
                        iLastObj = iLastRoom;
                        count++;
                        break;

                    case 'D':
                    case 'R':
                        break;

                    default:
                        throw new ImportException("Unknown reset type " + letter);
                }

                if (temp.command == 'M' || temp.command == 'O')
                    Loaded.Add(temp);
            }

            LogManager.GetCurrentClassLogger().Info("Loaded {0} resets.", count);
        }

        #endregion Methods
    }

    public class TempReset
    {
        #region Fields

        public int arg1, arg2, arg3, arg4;
        public char command;
        public List<TempReset> subResets = new List<TempReset>();

        #endregion Fields
    }
}
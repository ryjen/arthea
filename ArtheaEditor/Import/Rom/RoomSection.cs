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

    public class RoomSection : FileSection
    {
        #region Fields

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors

        public RoomSection()
        {
            Loaded = new Dictionary<int, Room>();
            Exits = new Dictionary<Exit, int>();
        }

        #endregion Constructors

        #region Properties

        public Dictionary<Exit, int> Exits
        {
            get;
            private set;
        }

        public Dictionary<int, Room> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit(IDbConnection conn)
        {
            foreach (var entry in Exits)
            {

                if (!Loaded.ContainsKey(entry.Value))
                {
                    Log.Warn("no exit for vnum {0}", entry.Value);
                    continue;
                }
                Room toRoom = Loaded[entry.Value];

                Exit exit = entry.Key;

                exit.ToRoom = toRoom;
            }

                foreach (var entry in Loaded)
                {
                    entry.Value.Save(conn);
                }
        }

        public override void Load(FileReader reader)
        {
            if (RomImporter.CurrentArea == null)
            {
                throw new ImportException("LoadRomRooms: No area seen yet.");
            }

            while (!reader.EndOfFile)
            {
                if (reader.ReadLetter() != '#')
                {
                    throw new ImportException("LoadRomRooms: '#' not found.");
                }

                var id = reader.ReadNumber();

                if (id == 0)
                    break;

                var room = new Room();

                room.Name = reader.ReadString();
                room.Description = reader.ReadString();
                reader.ReadNumber(); // area number
                room.Flags = Converter.ConvertRoomFlags(reader.ReadFlag());
                room.Terrain = Converter.ConvertSector(reader.ReadNumber());

                while (!reader.EndOfFile)
                {
                    var letter = reader.ReadLetter();

                    if (letter == 'S')
                    {
                        break;
                    }
                    if (letter == 'H')
                    {
                        reader.ReadNumber();
                    }
                    else if (letter == 'M')
                    {
                        reader.ReadNumber();
                    }
                    else if (letter == 'C')
                    {
                        reader.ReadString();
                    }
                    else if (letter == 'D')
                    {
                        var door = Converter.ConvertDirection(reader.ReadNumber());

                        var exit = new Exit();
                        exit.Description = reader.ReadString();
                        exit.Name = reader.ReadString();
                        var locks = reader.ReadNumber();
                        var key = reader.ReadNumber();
                        exit.Key = key;
                        var toRoom = reader.ReadNumber();
                        Exits.Add(exit, toRoom);
                        if (!string.IsNullOrEmpty(exit.Name))
                            exit.Name += " " + door;
                        else
                            exit.Name = door.ToString();

                        switch (locks)
                        {
                            case 0:
                                break;
                            case 1:
                                exit.Flags |= (ExitFlag.Closed | ExitFlag.Door);
                                break;
                            case 2:
                                exit.Flags |= (ExitFlag.Door | ExitFlag.Closed | ExitFlag.Locked);
                                break;
                            case 3:
                                exit.Flags |= (ExitFlag.NoPass | ExitFlag.Door | ExitFlag.Closed |
                                               ExitFlag.Locked);
                                break;
                            case 4:
                                exit.Flags |= (ExitFlag.PickProof | ExitFlag.Door | ExitFlag.Closed |
                                               ExitFlag.Locked);
                                break;
                            case 5:
                                exit.Flags |= (ExitFlag.NoPass | ExitFlag.PickProof | ExitFlag.Door |
                                               ExitFlag.Closed | ExitFlag.Locked);
                                break;
                        }
                        room.Exits.Add(door, exit);
                    }
                    else if (letter == 'E')
                    {
                        //room.ExtraDescriptions.Add(_reader.ReadString(),
                        reader.ReadString();
                        reader.ReadString();
                    }
                    else if (letter == 'O')
                    {
                        reader.ReadString();
                    }
                    else
                    {
                        Log.Error("\troom flag " + letter + "  not found");
                        reader.UnRead();
                    }
                }

                Loaded.Add(id, room);
            }
            Log.Info("Loaded {0} rooms.", Loaded.Count);
        }

        #endregion Methods
    }
}
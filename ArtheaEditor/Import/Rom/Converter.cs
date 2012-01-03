namespace ArtheaEditor.Import.Rom
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using NLog;

    public static class Converter
    {
        #region Methods

        public static NpcFlag ConvertActFlags(long bits)
        {
            var convertTable = new Dictionary<char, NpcFlag>();
            NpcFlag flags = 0;

            convertTable.Add('B', NpcFlag.Sentinel);
            convertTable.Add('F', NpcFlag.Aggressive);
            convertTable.Add('G', NpcFlag.StayArea);
            convertTable.Add('H', NpcFlag.Wimpy);

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    flags |= entry.Value;
                }
            }

            return flags;
        }

        public static string ConvertActTags(string format, int charIndex)
        {
            format = format.Replace("$n", "{" + charIndex + "}");

            format = format.Replace("$m", "{" + charIndex + ":M}");

            format = format.Replace("$s", "{" + charIndex + ":S}");

            format = format.Replace("$e", "{" + charIndex + ":E}");

            return format;
        }

        public static string ConvertActTags(string format, int charIndex, int victimIndex)
        {
            format = ConvertActTags(format, charIndex);

            format = format.Replace("$N", "{" + victimIndex + "}");

            format = format.Replace("$M", "{" + victimIndex + ":M}");

            format = format.Replace("$S", "{" + victimIndex + ":S}");

            format = format.Replace("$E", "{" + victimIndex + ":E}");

            return format;
        }

        public static AffectLocation ConvertAffectLocation(int where)
        {
            switch (where)
            {
                default:
                    return AffectLocation.None;

                case 17:
                    return AffectLocation.AC;
                case 13:
                    return AffectLocation.Hit;
                case 12:
                    return AffectLocation.Mana;
                case 14:
                    return AffectLocation.Move;
            }
        }

        public static long ConvertContainerFlags(long bits)
        {
            var convertTable = new Dictionary<char, long>();
            long flags = 0;

            /*
                convertTable.Add('A', ContainerFlags.Closeable);
                convertTable.Add('B', ContainerFlags.PickProof);
                convertTable.Add('C', ContainerFlags.Closed);
                convertTable.Add('D', ContainerFlags.Locked);
                */

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    flags |= entry.Value;
                }
            }
            return flags;
        }

        public static Direction ConvertDirection(int door)
        {
            switch (door)
            {
                case 0:
                    return Direction.North;
                case 1:
                    return Direction.East;
                case 2:
                    return Direction.South;
                case 3:
                    return Direction.West;
                case 4:
                    return Direction.Up;
                case 5:
                    return Direction.Down;
                default:
                    throw new ImportException("Invalid direction on room " + door);
            }
        }

        public static ExitFlag ConvertExitFlags(long bits)
        {
            var convertTable = new Dictionary<char, ExitFlag>();
            ExitFlag flags = 0;

            convertTable.Add('A', ExitFlag.Door);
            convertTable.Add('B', ExitFlag.Closed);
            convertTable.Add('C', ExitFlag.Locked);
            convertTable.Add('F', ExitFlag.PickProof);
            convertTable.Add('G', ExitFlag.PickProof);

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    flags |= entry.Value;
                }
            }
            return flags;
        }

        public static ObjectFlag ConvertExtraFlags(long bits)
        {
            var convertTable = new Dictionary<char, ObjectFlag>();
            ObjectFlag itemFlags = 0;

            // convertTable.Add('A', ItemFlags.Unknown);

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    itemFlags |= entry.Value;
                }
            }
            return itemFlags;
        }

        public static long ConvertFurnitureFlags(long bits)
        {
            long flags = 0;
            long STAND_AT = (1L << 0);
            long STAND_ON = (1L << 1);
            long STAND_IN = (1L << 2);
            long SIT_AT = (1L << 3);
            long SIT_ON = (1L << 4);
            long SIT_IN = (1L << 5);
            long REST_AT = (1L << 6);
            long REST_ON = (1L << 7);
            long REST_IN = (1L << 8);
            long SLEEP_AT = (1L << 9);
            long SLEEP_ON = (1L << 10);
            long SLEEP_IN = (1L << 11);

            if (bits.HasFlag(STAND_ON | STAND_AT | STAND_IN))
            {
                //flags |= FurnitureObject.Stand;
            }
            if (bits.HasFlag(REST_AT | REST_IN | REST_ON | SIT_AT | SIT_IN | SIT_ON))
            {
                //flags |= FurnitureObject.Sit;
            }
            if (bits.HasFlag(SLEEP_AT | SLEEP_IN | SLEEP_ON))
            {
                //flags |= FurnitureObject.Sleep;
            }

            if (bits.HasFlag(STAND_AT | REST_AT | SIT_AT | SLEEP_AT))
            {
                //furnItem.V2 = FurnitureType.At;
            }
            else if (bits.HasFlag(STAND_IN | REST_IN | SIT_IN | SLEEP_IN))
            {
                //furnItem.Type = FurnitureType.In;
            }
            else if (bits.HasFlag(STAND_ON | REST_ON | SIT_ON | SLEEP_ON))
            {
                //furnItem.Type = FurnitureType.On;
            }

            return flags;
        }

        public static ObjectType ConvertObjectType(string p)
        {
            throw new NotImplementedException();
        }

        public static ObjectType ConvertObjectType(int t)
        {
            return (ObjectType)t;
        }

        public static long ConvertPortalFlags(long bits)
        {
            var convertTable = new Dictionary<char, long>();
            long flags = 0;

            //convertTable.Add('C', PortalFlags.GoWith);
            //convertTable.Add('E', PortalFlags.Random);

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    flags |= entry.Value;
                }
            }
            return flags;
        }

        public static Position ConvertPosition(string posName)
        {
            try
            {
               return (Position)Enum.Parse(typeof(Position), posName, true);
            }
            catch (ArgumentException)
            {
                LogManager.GetCurrentClassLogger().Warn("unknown position {0}", posName);
                return Position.Standing;
            }
        }

        public static Position ConvertPosition(int pos)
        {
            return (Position)pos;
        }

        public static Race ConvertRace(string name)
        {
            Race r = Race.Lookup(name);

            if (r != null)
                return r;

            return Race.Lookup("unique");
        }

        public static RoomFlag ConvertRoomFlags(long bits)
        {
            var convertTable = new Dictionary<char, RoomFlag>();

            RoomFlag roomFlags = 0;

            // convertTable.Add('A', RoomFlags.Example);

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    roomFlags |= entry.Value;
                }
            }

            return roomFlags;
        }

        public static Terrain ConvertSector(int sector)
        {
            switch (sector)
            {
                case 0:
                    return Terrain.Inside;
                case 1:
                    return Terrain.City;
                case 2:
                    return Terrain.Field;
                case 3:
                    return Terrain.Forest;
                case 4:
                    return Terrain.Hills;
                case 5:
                    return Terrain.Mountain;
                case 6:
                case 7:
                    return Terrain.Water;
                case 9:
                    return Terrain.Air;
                case 10:
                    return Terrain.Desert;
                default:
                    LogManager.GetCurrentClassLogger().Error("\tunknown Terrain type " + sector);
                    return Terrain.Inside;
            }
        }

        public static Sex ConvertSex(string sex)
        {
            try
            {
                return (Sex)Enum.Parse(typeof(Sex), sex, true);
            }
            catch (ArgumentException)
            {
                LogManager.GetCurrentClassLogger().Warn("unknown sex {0}", sex);
                return Sex.Neutral;
            }
        }

        public static Sex ConvertSex(int sex)
        {
            return (Sex)sex;
        }

        public static float ConvertSize(string size)
        {
            try
            {
                return (float)Enum.Parse(typeof(Size), size, true);
            }
            catch (ArgumentException)
            {
                LogManager.GetCurrentClassLogger().Warn("unknown size {0}", size);
               return (float)Size.Medium;
            }
        }

        public static WeaponClass ConvertWeaponClass(string weaponType)
        {
            try
            {
                return (WeaponClass)Enum.Parse(typeof(WeaponClass), weaponType, true);
            }
            catch (ArgumentException)
            {
                return WeaponClass.Exotic;
            }
        }

        public static long ConvertWeaponFlags(long bits)
        {
            return bits;
        }

        public static WearFlag ConvertWearFlags(long bits)
        {
            var convertTable = new Dictionary<char, WearFlag>();
            WearFlag flags = 0;

            //convertTable.Add('P', Object.NoSacrifice);

            if (!bits.HasFlag(FileReader.FlagConvert('A')))
            {
                //flags |= Object.Static;
            }

            foreach (var entry in convertTable)
            {
                if (bits.HasFlag(FileReader.FlagConvert(entry.Key)))
                {
                    flags |= entry.Value;
                }
            }
            return flags;
        }

        #endregion Methods
    }
}
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

    using Object = ArtheaEngine.Model.Object;

    public class ObjectSection : FileSection
    {
        #region Constructors

        public ObjectSection()
        {
            Loaded = new Dictionary<int, Object>();
        }

        #endregion Constructors

        #region Properties

        public Dictionary<int, Object> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit()
        {
			DbManager.Save<Object>(Loaded.Values);
            //foreach (var obj in Loaded)
              //      obj.Value.Save(conn);
        }

        public override void Load(FileReader reader)
        {
            Load(reader, true);
        }

        public void Load(FileReader reader, bool newFormat)
        {
            if (RomImporter.CurrentArea == null)
            {
                throw new ImportException("LoadRomObjects: area not seen yet");
            }

            if (newFormat)
                LoadNewFormat(reader);
            else
                LoadOldFormat(reader);

            LogManager.GetCurrentClassLogger().Info("Loaded {0} objects", Loaded.Count);
        }

        public void LoadNewFormat(FileReader reader)
        {
            while (!reader.EndOfFile)
            {
                if (reader.ReadLetter() != '#')
                {
                    throw new ImportException("LoadRomObjects: # not found");
                }

                var id = reader.ReadNumber();

                if (id == 0)
                    break;

                Object item = new Object();

                item.Name = reader.ReadString();
                item.ShortDescr = reader.ReadString();
                item.LongDescr = reader.ReadString();

                reader.ReadString(); // material

                item.Type = Converter.ConvertObjectType(reader.ReadWord());

                item.Flags = Converter.ConvertExtraFlags(reader.ReadFlag()); // extra flags

                item.WearFlags = Converter.ConvertWearFlags(reader.ReadFlag()); // wear flags

                switch (item.Type)
                {
                    case ObjectType.Weapon:
                        item.Values.Add(Converter.ConvertWeaponClass(reader.ReadWord()));

                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadWord()); // attack type
                        item.Values.Add(Converter.ConvertWeaponFlags(reader.ReadFlag()));
                        break;
                    case ObjectType.Container:
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(Converter.ConvertContainerFlags(reader.ReadFlag()));
                        item.Values.Add((long)reader.ReadNumber());
                        reader.ReadFlag();
                        reader.ReadFlag();
                        break;
                    case ObjectType.DrinkContainer:
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadWord()); // liquid type
                        reader.ReadFlag();
                        reader.ReadFlag();
                        break;
                    case ObjectType.Wand:
                    case ObjectType.Staff:
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(reader.ReadNumber());
                        item.Values.Add(Ability.Lookup(reader.ReadWord()).Name);
                        reader.ReadNumber();
                        break;

                    case ObjectType.Potion:
                    case ObjectType.Pill:
                    case ObjectType.Scroll:

                        item.Values.Add(reader.ReadNumber());
                        var spell = Ability.Lookup(reader.ReadWord()); // skill lookup
                        if (spell != null) item.Values.Add(spell.Name);
					spell = Ability.Lookup(reader.ReadWord()); // skill lookup
                        if (spell != null) item.Values.Add(spell.Name);
					spell = Ability.Lookup(reader.ReadWord());
                        if (spell != null) item.Values.Add(spell.Name);
					spell = Ability.Lookup(reader.ReadWord());
                        if (spell != null) item.Values.Add(spell.Name);

                        break;

                    case ObjectType.Food:
                        int hours;
                        try
                        {
                            hours = Convert.ToInt32(reader.ReadFlag());
                        }
                        catch
                        {
                            hours = 0;
                        }
                        item.Values.Add(hours);
                        reader.ReadFlag();
                        reader.ReadFlag();
                        item.Values.Add(reader.ReadFlag());
                        reader.ReadFlag();
                        break;

                    case ObjectType.Armor:
                        item.Values.Add(reader.ReadFlag());
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        break;

                    case ObjectType.Portal:
                        item.Values.Add(Convert.ToInt16(reader.ReadFlag()));
                        item.Values.Add(Converter.ConvertExitFlags(reader.ReadFlag()));
                        item.Values.Add(Converter.ConvertPortalFlags(reader.ReadFlag()));
                        item.Values.Add(Convert.ToInt32(reader.ReadFlag()));
                        reader.ReadFlag();
                        break;

                    case ObjectType.Furniture:
                        reader.ReadFlag();
                        reader.ReadFlag();
                        Converter.ConvertFurnitureFlags(reader.ReadFlag());
                        item.Values.Add(Convert.ToInt32(reader.ReadFlag()));
                        item.Values.Add(reader.ReadFlag());
                        break;

                    case ObjectType.Money:
                        item.Values.Add(Convert.ToDouble(reader.ReadFlag()));
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        break;

                    default:
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        reader.ReadFlag();
                        break;
                }

                item.Level = Convert.ToInt16(reader.ReadNumber());
                item.Weight = Convert.ToInt16(reader.ReadNumber());
                item.Cost = Convert.ToDecimal(reader.ReadNumber());
                item.Area = RomImporter.CurrentArea;

                switch (reader.ReadLetter())
                {
                    case ('P'):
                        item.Condition = 100;
                        break;
                    case ('G'):
                        item.Condition = 90;
                        break;
                    case ('A'):
                        item.Condition = 75;
                        break;
                    case ('W'):
                        item.Condition = 50;
                        break;
                    case ('D'):
                        item.Condition = 25;
                        break;
                    case ('B'):
                        item.Condition = 10;
                        break;
                    case ('R'):
                        item.Condition = 0;
                        break;
                    default:
                        item.Condition = 100;
                        break;
                }

                while (!reader.EndOfFile)
                {
                    var letter = reader.ReadLetter();

                    if (letter == 'A')
                    {
                        var where = Converter.ConvertAffectLocation(reader.ReadNumber());
                        var modifier = (short)reader.ReadNumber();

                        if (where != AffectLocation.None)
                        {
                            var a = new Affect();
                            a.Flag = 0;
							a.Extra = null;
                            a.Location = where;
                            a.Modifier = modifier;
                            item.Affects.Add(a);
                        }
                    }
                    else if (letter == 'F')
                    {
                        switch (reader.ReadLetter())
                        {
                            case 'A':
                            case 'I':
                            case 'R':
                            case 'V':
                                break;
                            default:
                                throw new ImportException("Invalid object flag on item " + item.Id);
                        }

                        reader.ReadNumber(); // location
                        reader.ReadNumber(); // modifier;
                        reader.ReadFlag(); // bitvector
                    }
                    else if (letter == 'E')
                    {
                        //item.ExtraDescriptions.Add(_reader.ReadString(),
                        reader.ReadString();
                        reader.ReadString();
                    }
                    else
                    {
                        reader.UnRead();
                        break;
                    }
                }
                Loaded.Add(id, item);
            }
        }

        public void LoadOldFormat(FileReader reader)
        {
            for (; ; )
            {
                char letter = reader.ReadLetter();
                if (letter != '#')
                {
                    throw new ImportException("# not found");
                }

                int vnum = reader.ReadNumber();

                if (vnum == 0)
                    break;

                // TODO: check obj exists

                Object obj = new Object();

                obj.Name = reader.ReadString();
                obj.ShortDescr = reader.ReadString();
                obj.LongDescr = reader.ReadString();
                reader.ReadString();

                obj.Type = Converter.ConvertObjectType(reader.ReadNumber());

                obj.Area = RomImporter.CurrentArea;

                obj.Flags = Converter.ConvertExtraFlags(reader.ReadFlag());
                obj.WearFlags = Converter.ConvertWearFlags(reader.ReadFlag());

                reader.ReadNumber();
                reader.ReadNumber();
                reader.ReadNumber();
                reader.ReadNumber();

                obj.Weight = reader.ReadNumber();

                obj.Cost = reader.ReadNumber();

                reader.ReadNumber();

                for (; ; )
                {
                    letter = reader.ReadLetter();

                    if (letter == 'A')
                    {
                        Affect paf = new Affect();
                        paf.Location = Converter.ConvertAffectLocation(reader
                                .ReadNumber());
                        paf.Modifier = reader.ReadNumber();

                        // paf.Level = obj.Level;

                        paf.Duration = -1;

                        obj.Affects.Add(paf);

                    }
                    else if (letter == 'E')
                    {
                        reader.ReadString();
                        reader.ReadString();
                    }
                    else
                    {
                        reader.UnRead();
                        break;
                    }
                }

                Loaded.Add(vnum, obj);
            }
        }

        #endregion Methods
    }
}
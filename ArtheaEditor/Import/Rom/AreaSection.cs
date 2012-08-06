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

    public class AreaSection : FileSection
    {
        #region Constructors

        public AreaSection()
        {
            Loaded = new List<Area>();
        }

        #endregion Constructors

        #region Properties

        public List<Area> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit()
        {
			DbManager.Save<Area>(Loaded);

			/*
            foreach (Area a in Loaded)
            {
				DbManager.Save(a);
                //a.Save(conn);
            }*/
        }

        public override void Load(FileReader reader)
        {
            Load(reader, true);
        }

        public void Load(FileReader reader, bool newFormat)
        {
            if (newFormat)
                LoadNewFormat(reader);
            else
                LoadOldFormat(reader);
        }

        private void LoadNewFormat(FileReader reader)
        {
            var area = new Area();

            string word;

            while (!reader.EndOfFile)
            {
                word = reader.ReadWord();

                switch (word.ToLower())
                {
                    case "credits":
                        area.Credits = reader.ReadString();
                        break;
                    case "name":
                        area.Name = reader.ReadString();
                        break;
                    case "security":
                        reader.ReadNumber();
                        break;
                    case "vnums":
                        /*_currentArea.BaseId = (uint)*/
                        reader.ReadNumber();
                        /*_currentArea.MaxId = (uint)*/
                        reader.ReadNumber();
                        break;
                    case "builders":
                        reader.ReadString();
                        break;
                    case "end":
                        /*Area exists = Lists.Areas.FindById(_currentArea.BaseId, _currentArea.MaxId);
                            if (exists != null)
                                _currentArea = exists;*/
                        RomImporter.CurrentArea = area;
                        Loaded.Add(area);
                        return;
                    default:
                        LogManager.GetCurrentClassLogger().Warn("Invalid area key {0}", word);
                        reader.ReadToEol();
                        break;
                }
            }
        }

        private void LoadOldFormat(FileReader reader)
        {
            var area = new Area();

            reader.ReadString(); // filename
            area.Name = reader.ReadString();
            area.Credits = reader.ReadString();
            reader.ReadNumber(); // min vnum
            reader.ReadNumber(); // max vnum

            Loaded.Add(area);

            RomImporter.CurrentArea = area;
        }

        #endregion Methods
    }
}
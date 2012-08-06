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

    public class HelpSection : FileSection
    {
        #region Constructors

        public HelpSection()
        {
            Loaded = new List<Help>();
        }

        #endregion Constructors

        #region Properties

        public List<Help> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit()
        {
			DbManager.Save<Help>(Loaded);

            //foreach (Help help in Loaded)
              //      help.Save(conn);
        }

        public override void Load(FileReader reader)
        {
            var count = 0;

            while (!reader.EndOfFile)
            {
                var help = new Help();

                var level = reader.ReadNumber();

                if (level >= 51) //help.Category = HelpCategory.Admin;
                    help.Category = HelpCategory.Admin;

                var keywords = reader.ReadString();
                foreach (var key in new Argument(keywords).All())
                {
                    help.Keywords.Add(key);
                }

                if (keywords.Contains("$"))
                    break;

                help.Text = reader.ReadString();

                count++;
            }

            LogManager.GetCurrentClassLogger().Info("Loaded {0} helps.", count);
        }

        #endregion Methods
    }
}
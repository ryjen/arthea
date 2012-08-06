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

    public class SocialSection : FileSection
    {
        #region Constructors

        public SocialSection()
        {
            Loaded = new List<Social>();
        }

        #endregion Constructors

        #region Properties

        public IList<Social> Loaded
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit()
        {
			DbManager.Save<Social>(Loaded);

            //foreach (var s in Loaded)
              //  s.Save(conn);
        }

        public override void Load(FileReader reader)
        {
            Social soc = null;

            while (!reader.EndOfFile)
            {
                if (soc != null)
                {
                    //TODO: Save
                    Loaded.Add(soc);
                }

                var temp = reader.ReadWord();

                if (temp == "#0")
                    break;

                soc = new Social();

                soc.Name = temp;

                reader.ReadLine();

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.CharNoArg = temp;

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.OthersNoArg = Converter.ConvertActTags(temp, 0);

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.CharFound = Converter.ConvertActTags(temp, 0, 1);

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.OthersFound = Converter.ConvertActTags(temp, 1, 0);

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.VictFound = Converter.ConvertActTags(temp, 1, 0);

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.CharNotFound = temp;

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.CharSelf = temp;

                temp = reader.ReadLine();

                if (temp == "#")
                    continue;

                if (temp != "$")
                    soc.OthersSelf = Converter.ConvertActTags(temp, 0);
            }

            LogManager.GetCurrentClassLogger().Info("Loaded {0} socials.", Loaded.Count);
        }

        #endregion Methods
    }
}
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

    public class ShopSection : FileSection
    {
        #region Constructors

        public ShopSection(RomImporter importer)
        {
            Loaded = new Dictionary<int, Shop>();
            Importer = importer;
        }

        #endregion Constructors

        #region Properties

        public Dictionary<int, Shop> Loaded
        {
            get;
            private set;
        }

        protected RomImporter Importer
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public override void Commit(IDbConnection conn)
        {
            foreach (var entry in Loaded)
            {
                if (!Importer.Mobiles.Loaded.ContainsKey(entry.Key))
                {
                    LogManager.GetCurrentClassLogger().Warn("no mobile for shop {0}", entry.Key);
                    continue;
                }

                NonPlayer npc = Importer.Mobiles.Loaded[entry.Key];
                Shop shop = entry.Value;

                npc.Shop = shop;

                shop.Owner = npc;

                shop.Save(conn);
                npc.Save(conn);
            }
        }

        public override void Load(FileReader reader)
        {
            for (; ; )
            {
                int vnum = reader.ReadNumber();

                if (vnum == 0)
                    break;

                Shop shop = new Shop();
                for (int i = 0; i < 5; i++)
                    reader.ReadNumber();

                shop.ProfitBuy = (short)reader.ReadNumber();
                shop.ProfitSell = (short)reader.ReadNumber();
                shop.OpenHour = (short)reader.ReadNumber();
                shop.CloseHour = (short)reader.ReadNumber();
                reader.ReadToEol();

                Loaded.Add(vnum, shop);
            }

            LogManager.GetCurrentClassLogger().Info("loaded {0} shops", Loaded.Count);
        }

        #endregion Methods
    }
}
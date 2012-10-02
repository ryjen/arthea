namespace ArtheaEditor.Import.Rom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    using NLog;

    using Conv = Converter;

    using Object = ArtheaEngine.Model.Object;

    public class RomImporter : IImporter
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /*private readonly List<Area> _areas = new List<Area>();
        private readonly Dictionary<int, NonPlayer> _characterIndexes = new Dictionary<int, NonPlayer>();
        private readonly Dictionary<int, Object> _objectIndexes = new Dictionary<int, Object>();
        private readonly Dictionary<int, Room> _roomIndexes = new Dictionary<int, Room>();
		*/
        private FileInfo _file;
        private FileReader _reader;

        #endregion Fields

        #region Constructors

        public RomImporter()
        {
            Mobiles = new MobileSection();
            Objects = new ObjectSection();
            Helps = new HelpSection();
            Resets = new ResetSection(this);
            Socials = new SocialSection();
            Rooms = new RoomSection(this);
            Areas = new AreaSection();
            Shops = new ShopSection(this);
            Specials = new SpecialSection();
        }

        #endregion Constructors

        #region Properties

        public static Area CurrentArea
        {
            get; set;
        }

        public AreaSection Areas
        {
            get;
            private set;
        }

        public HelpSection Helps
        {
            get;
            private set;
        }

        public MobileSection Mobiles
        {
            get; private set;
        }

        public ObjectSection Objects
        {
            get; private set;
        }

        public ResetSection Resets
        {
            get;
            private set;
        }

        public RoomSection Rooms
        {
            get;
            private set;
        }

        public ShopSection Shops
        {
            get;
            private set;
        }

        public SocialSection Socials
        {
            get;
            private set;
        }

        public SpecialSection Specials
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public void Commit()
        {
            try
            {

                Areas.Commit();
                Mobiles.Commit();
                Objects.Commit();
                Rooms.Commit();
                Resets.Commit();
                Shops.Commit();
                Helps.Commit();
                Socials.Commit();
                Specials.Commit();

                Log.Info("Commit Complete.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            
        }

        public void Process(FileInfo fileInfo)
        {
            if (fileInfo.Name.EndsWith(".lst"))
            {
                var listReader = new FileReader(fileInfo.FullName);
                var word = listReader.ReadWord();

                var count = 0;

                while (word != "$")
                {
                    try
                    {
                        Process(new FileInfo(fileInfo.DirectoryName + Path.DirectorySeparatorChar + word));
                        count++;
                    }
                    catch (ImportException ex)
                    {
                        Log.Error("Unable to load {0} ({1})", word, ex.Message);
                        return;
                    }
                    word = listReader.ReadWord();
                }

                Log.Info("Imported {0} area files.", count);
                return;
            }

            _file = fileInfo;

            _reader = new FileReader(_file.FullName);

            while (!_reader.EndOfFile)
            {
                if (_reader.ReadLetter() != '#')
                {
                    Log.Error("Error in rom area _file {0}", _file.Name);
                    throw new ImportException("Invalid _file");
                }

                var word = _reader.ReadWord();

                switch (word.ToLower())
                {
                    case "$":
                        break;
                    case "areadata":
                        Areas.Load(_reader, true);
                        break;
                    case "area":
                        Areas.Load(_reader, false);
                        break;
                    case "mobiles":
                        Mobiles.Load(_reader, true);
                        break;
                    case "objects":
                        Objects.Load(_reader, true);
                        break;
                    case "mobold":
                        Mobiles.Load(_reader, false);
                        break;
                    case "objold":
                        Objects.Load(_reader, false);
                        break;
                    case "rooms":
                        Rooms.Load(_reader);
                        break;
                    case "resets":
                        Resets.Load(_reader);
                        break;
                    case "helps":
                        Helps.Load(_reader);
                        break;
                    case "shops":
                        Shops.Load(_reader);
                        break;
                    case "socials":
                        Socials.Load(_reader);
                        break;
                    case "specials":
                        Specials.Load(_reader);
                        break;
                    default:
                        Log.Error("Bad section name in ROM area {0}", _file.Name);
                        throw new ImportException("Bad section name");
                }
            }
        }

        #endregion Methods
    }
}
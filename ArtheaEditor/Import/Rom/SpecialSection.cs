namespace ArtheaEditor.Import.Rom
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using NLog;

    public class SpecialSection : FileSection
    {
        #region Constructors

        public SpecialSection()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Commit()
        {
        }

        public override void Load(FileReader reader)
        {
            while (!reader.EndOfFile)
            {
                char letter;

                switch ((letter = reader.ReadLetter()))
                {
                    default:
                        LogManager.GetCurrentClassLogger().Error("\timport specials: letter " + letter + " not found.");
                        break;

                    case 'S':
                        return;

                    case '*':
                        break;

                    case 'M':
                        reader.ReadNumber();
                        reader.ReadWord();
                        break;
                }

                reader.ReadToEol();
            }
        }

        #endregion Methods
    }
}
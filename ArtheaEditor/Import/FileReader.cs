namespace ArtheaEditor.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using ArtheaEngine.Model;

    using NLog;

    public class FileReader
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string _file;

        private int _line;
        private int _pos;

        #endregion Fields

        #region Constructors

        public FileReader(string path)
        {
            _file = File.ReadAllText(path);
            _pos = 0;
            _line = 0;
        }

        #endregion Constructors

        #region Properties

        public bool EndOfFile
        {
            get { return _pos >= _file.Length - 1; }
        }

        #endregion Properties

        #region Methods

        public static long FlagConvert(char letter)
        {
            long bitsum = 0;
            char i;

            if ('A' <= letter && letter <= 'Z')
            {
                bitsum = 1;
                for (i = letter; i > 'A'; i--)
                    bitsum *= 2;
            }
            else if ('a' <= letter && letter <= 'z')
            {
                bitsum = 67108864; /* 2^26 */
                for (i = letter; i > 'a'; i--)
                    bitsum *= 2;
            }

            return bitsum;
        }

        public long ReadFlag()
        {
            long number = 0;

            var c = ReadLetter();

            if (char.IsLetter(c)) /* ROM OLC */
            {
                while (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                {
                    number += FlagConvert(c);
                    c = Read();
                }
            }

            while (char.IsDigit(c))
            {
                number = number * 10 + c - '0';
                c = Read();
            }

            if (c == '|')
                number += ReadFlag();

            else if (c != ' ')
                UnRead();

            return number;
        }

        public char ReadLetter()
        {
            char letter;

            do
            {
                letter = Read();
            } while (char.IsWhiteSpace(letter));

            return letter;
        }

        public string ReadLine()
        {
            var letter = Read();

            var line = new StringBuilder();

            while (!EndOfFile && letter != '\n' && letter != '\r')
            {
                line.Append(letter);
                letter = Read();
            }

            while (!EndOfFile && (letter == '\n' || letter == '\r'))
                letter = Read();

            UnRead();

            return line.ToString();
        }

        public int ReadNumber()
        {
            var number = 0;
            var sign = false;
            var c = ReadLetter();

            if (c == '+')
            {
                c = Read();
            }
            else if (c == '-')
            {
                sign = true;
                c = Read();
            }

            if (!char.IsDigit(c))
            {
                Log.Error("ReadNumber: bad format ({0}).", _file.Substring(_pos - 5, 10));
            }

            do
            {
                number = number * 10 + c - '0';
                c = Read();
            } while (char.IsDigit(c));

            if (sign)
                number = 0 - number;

            if (c == '|')
                number += ReadNumber();
            else if (c != ' ')
                UnRead();

            return number;
        }

        public string ReadString()
        {
            var c = ReadLetter();
            var buf = new StringBuilder();

            while (!EndOfFile && c != '~')
            {
                switch (c)
                {
                    default:
                        buf.Append(c);
                        break;
                    case '\n':
                        buf.Append(Environment.NewLine);
                        break;
                    case '\r':
                        break;
                }
                c = Read();
            }

            return buf.ToString();
        }

        public string ReadStringEol()
        {
            char c = ReadLetter();
            StringBuilder buf = new StringBuilder();

            while (!EndOfFile && c != '~' && c != '\n' && c != '\r')
            {

                buf.Append(c);

                c = Read();
            }

            while (!EndOfFile && (c == '\n' || c == '\r'))
            {
                c = Read();
            }

            UnRead();

            return buf.ToString();
        }

        public void ReadToEol()
        {
            char c;

            do
            {
                c = Read();
            } while (c != '\n' && c != '\r');

            do
            {
                c = Read();
            } while (c == '\n' || c == '\r');

            UnRead();
            return;
        }

        public string ReadWord()
        {
            var cEnd = ReadLetter();
            var word = new StringBuilder();

            if (cEnd != '"' && cEnd != '\'')
            {
                word.Append(cEnd);
                cEnd = ' ';
            }

            while (!EndOfFile)
            {
                var next = Read();

                if (cEnd == ' ' ? char.IsWhiteSpace(next) : next == cEnd)
                {
                    if (cEnd == ' ')
                        UnRead();

                    return word.ToString();
                }

                word.Append(next);
            }

            return word.ToString();
        }

        public void UnRead()
        {
            if (_pos > 0)
            {
                _pos--;
                if (_file[_pos] == '\n' && _line > 0)
                    _line--;
            }
        }

        private char Read()
        {
            char c =  _file[_pos++];
            if (c == '\n')
                _line++;
            return c;
        }

        #endregion Methods
    }
}
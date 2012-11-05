
using System;
using Thought.Terminals;
using Thought.Terminals.Translators;

namespace Aurora
{
    public class Program
    {
        static void Main(string[] args)
        {

            Terminal terminal = new SystemTerminal();

            // Add the game text files folder to the search
            // path of the terminal.  The terminal will look for
            // files in the specified path.

            terminal.TextPath = @"..\..\..\Game Files\Text Files";

            // Add an ANSI-BBS translator, which is probably the
            // most commonly used translator of all.  This translator
            // handles a subset of X3.64 escape codes.

            terminal.Translators.Add(new AnsiTranslator());

            // Register the AVATAR translator.  AVATAR codes are
            // popular with sysops because they are small and
            // encoded in a simple manner compatible with DOS and
            // the Windows console.

            terminal.Translators.Add(new AvatarTranslator());

            // Display the title screen.

            terminal.TranslateFile("title.ans", true);

            // Display the introductory titles.

            terminal.TranslateFile("Story1.ans", true);
            terminal.TranslateFile("Story2.ans", true);
            terminal.TranslateFile("Story3.ans", true);
            terminal.TranslateFile("Story4.ans", true);
            

        }
    }
}
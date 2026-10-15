using System;
using Thought.Terminals;
using Thought.Terminals.Translators;

namespace Aurora
{
    public class Program
    {
        private const string RestrictedScreensNotice =
            "Aurora City game screens are omitted because redistribution permission is not established.";

        static void Main(string[] args)
        {
            Terminal terminal = new SystemTerminal();

            // Keep the terminal translator setup for historical illustration.
            terminal.Translators.Add(new AnsiTranslator());
            terminal.Translators.Add(new AvatarTranslator());

            // Do not attempt to load the copyright-restricted example screens.
            Console.WriteLine(RestrictedScreensNotice);
        }
    }
}

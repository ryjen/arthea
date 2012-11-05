
using System;
using Thought.Terminals;

namespace Step3
{
    class Program
    {
        static void Main(string[] args)
        {

            // The previous steps introducted the basics of
            // the Terminal class.  This step shows a few of the more
            // advanced prompt features.

            Terminal terminal = new SystemTerminal();

            // As described in the last step, the functions beginning
            // with the word "Prompt" are high-level input functions.
            // A few of the most common prompting needs are built into
            // the library, and more will be added to the future.
            //
            // One goal is to make the prompt functions sufficiently
            // flexible to accomodate most needs.  However, it is
            // not desired to implement functions with many, many
            // different parameters.  Instead, the prompt functions
            // typically accept a settings class that holds all of 
            // settings that can be specified.

            Terminal.TextPromptSettings settings =
                new Terminal.TextPromptSettings();

            // Try changing some of the settings.
            // Note that you might get an error if you specify
            // settings that are mutually exclusive, thus confusing
            // the library and making it difficult to accomodate
            // your request.  For example, the minimum length of
            // a prompt cannot be larger than the maximum allowed
            // length!

            settings.BackgroundCharacter = '.';
            settings.BackgroundStyle = new TextStyle(TextColor.White, TextColor.Blue);
            settings.BorderStyle = new TextStyle(TextColor.White, TextColor.Yellow);
            settings.EntryStyle = new TextStyle(TextColor.Yellow, TextColor.Blue);
            settings.FillOut = true;
            settings.LeftBorder = "<";
            settings.Length = 40;
            settings.MaximumEntryLength = 40;
            settings.RightBorder = ">";
            settings.WriteNewLine = true;

            terminal.PromptText("Enter your name.", null, settings);

        }
    }
}

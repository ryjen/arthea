
using System;
using Thought.Terminals;

namespace Step2
{
    class Program
    {
        static void Main(string[] args)
        {

            // The previous step showed how to create a new terminal
            // that represented the local console.  This step will
            // show some of the basic operations provided by the terminal.
            
            Terminal terminal = new SystemTerminal();

            // All functions with the name "Write" (or the prefix "Write")
            // are simple output functions.  These functions do not perform
            // anything sophisticated such as conversion of macros, etc.
            // Think of the "Write" functions as being raw output functions.

            terminal.WriteLine("Press a key...");

            // Likewise, the "Read" functions are simple input functions.
            // They are blocking functions, meaning the function does not
            // return until a character is available in the input
            // buffer.  Also, they do not echo to the screen.  Think of
            // the "Read" functions as being low-level input functions.
            //
            // But don't worry: the terminal library provides sophisticated
            // high-level input functions (e.g. a text prompt, a lightbar
            // menu, etc).  Those will be described later.

            char key = terminal.ReadChar();

            terminal.Write("You pressed the following character: ");
            terminal.WriteLine(key);

            // All functions beginning with the word Prompt are high-level
            // I/O functions.  The class implements many of the most types of
            // prompts needed in terminal applications.
            //
            // The PromptText function implements a string-entry prompt.
            // The function supports a wide variety of different options;
            // the call below uses the most basic options.  When running,
            // notice that you cannot enter more than 10 characters.

            string name = terminal.PromptText(
                "Type your name (max 10):", "Bubba", 10);

            terminal.WriteLine("Goodbye, " + name);
            terminal.PromptEnter("Press Enter to exit.");

        }
    }
}

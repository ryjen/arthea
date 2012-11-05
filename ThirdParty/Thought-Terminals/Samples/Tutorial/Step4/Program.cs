
using System;
using Thought.Terminals;
using Thought.Terminals.Translators;

namespace Step4
{
    class Program
    {
        static void Main(string[] args)
        {

            SystemTerminal terminal = new SystemTerminal();
            
            // A Translator is a module that expands control codes and macros
            // that are embedded in output text.  For example, a translator
            // can convert the text "@CLS@" into a command to clear the
            // screen.
            //
            // Many translators are included with the terminal library.
            // These include ANSI-BBS (a subset of X3.64), AVATAR, and
            // utility translators for building macros.  Translators
            // for popular BBS-era games and applications are also
            // included.
            //
            // However, the terminal does not automatically support every
            // possible translator.  This would waste memory and resources.
            // Instead you need to instantiate and register the desired
            // translators when initializing the terminal.
            //
            // This tutorial step will use the Ambroshia color translator.
            // Ambroshia is a game that can be executed by telnet and
            // dial-up bulletin board systems.  Ambrosha has a simple
            // macro system for changing colors.
            //
            // You can add support for a translator by adding the
            // translator object to the Terminal.Translators collection.
            // At that point the terminal engine will automatically
            // use the translator when needed.

            terminal.Translators.Add(new AmbroshiaColorTranslator());

            // An Ambroshia color code can be any of the following:
            //
            //   `bright blue`
            //   `bright cyan`
            //   `bright green`
            //   `bright magenta`
            //   `bright red`
            //   `bright white`
            //   `bright yellow`
            //   `grey`
            //   `dark green`
            //   `dark red`
            //   `yellow`
            //
            // The format of the code is the name of a color surrounded
            // by back quotes (NOTE: the quote character shown above
            // is a back quote, not a forward quote).

            terminal.Translate("Hello `yellow`world`grey`!");
            terminal.WriteLine();

            // The translation engine attempts to be intelligent.
            // It does not delete invalid codes, for example.

            terminal.TranslateLine("This is not an Ambroshia `color code`!");

            terminal.PromptEnter("Press ENTER to continue.");

        }
    }
}

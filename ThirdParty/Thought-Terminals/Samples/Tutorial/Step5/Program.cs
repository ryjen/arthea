
using System;
using System.Collections;
using Thought.Terminals;
using Thought.Terminals.Translators;

namespace Step5
{
    class Program
    {
        static void Main(string[] args)
        {

            // The previous step showed how to register a translator
            // with a terminal.  This step shows the macro translator,
            // which allows you to insert variables into text.

            SystemTerminal terminal = new SystemTerminal();

            // The Macro translator expands embedded variables.
            // The basic format is {Name}, where Name is the name of
            // some value.  The left and right brackets can be
            // changed (e.g. to [Name]).
            //
            // It is also possible to specify formatting strings.
            // Normally a macro is expanded into its standard string
            // value.  A formatting code allows you to control the
            // output.  The library uses the .NET formatting system
            // and hence you can use any formatting string supported
            // by the underlying value (e.g. you can use date formatting
            // strings when expanding date value).
            //
            //   {Name:Format}
            //
            // The base class for macros is MacroTranslator.  This class
            // handles the expansion and formatting of a macro, but does
            // not implement the bit of logic to look up a named value.
            // In other words, the MacroTranslator class will handle
            // everything needed to translate "{UserName:30}" except
            // for the code that provides the underlying value for UserName.
            //
            // That lookup code is implemented by inheriting classes.  This
            // model allows different lookup techniques to be implemented.
            // For example, the DataSourceMacroTranslator can extract
            // values from a data table or IDataRecord.  Other types of
            // lookups can also be implemented.  The advantage is that an
            // implementor of a macro translator does not have to build the
            // entire translator -- only the lookup portion.

            DataSourceMacroTranslator t = new DataSourceMacroTranslator();

            // The data source translator needs a data source (obviously).
            // This example will use a hash table.

            Hashtable hash = new Hashtable();

            hash.Add("UserName", "pinchy");
            hash.Add("Location", "Minneapolis, MN");

            t.DataSource = hash;

            // Register the translator with the terminal.

            terminal.Translators.Add(t);

            // Demonstrate output.  Note that the third line
            // is an unrecognized macro (and will be displayed
            // without modification).

            terminal.TranslateLine("Hello, {UserName}.");
            terminal.TranslateLine("How is the weather in {Location}?");
            terminal.TranslateLine("How is your {Brother} doing?");

            terminal.PromptEnter("Press ENTER to exit.");

        }
    }
}

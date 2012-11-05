
using System;
using Thought.Terminals;

namespace Step1
{
    class Program
    {
        static void Main(string[] args)
        {

            // The Terminal class is an abstract class that defines
            // the basic properties and functions of a terminal (e.g.
            // setting a color or moving the cursor position).
            //
            // The Terminal class itself does not know how to perform
            // most of its operations -- for example, the process of
            // moving a cursor differs from one terminal type to the
            // other.
            //
            // In order to add support for a new type of terminal, 
            // it is necessary to inherit from the Terminal class
            // and override certain methods, such as Beep().
            //
            // Two implementations are already provided by the library.
            // The SystemTerminal class represents the same terminal
            // provided by the .NET static Console class -- that is,
            // the local console window.  The SerialTerminal class
            // represents a terminal that operates over a serial-type
            // connection (e.g. Telnet, serial port, etc).
            //
            // This tutorial demonstrates use of the SystemTerminal.

            Terminal terminal = new SystemTerminal();

            terminal.WriteLine("Hello, World");
            terminal.PromptEnter("Press Enter to exit.");
            terminal.WriteLine("Goodbye, World");

            // One quick note: One advantage of the terminal library
            // is that your I/O code is independent of the particular
            // terminal type.  For example, the above code behaves
            // the same whether the terminal is a system terminal or
            // a VT100 terminal that supports X3.64 escape codes.

        }
    }
}

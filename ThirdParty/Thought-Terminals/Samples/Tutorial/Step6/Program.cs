
using System;
using Thought.Terminals;

namespace Step6
{
    class Program
    {
        static void Main(string[] args)
        {

            SystemTerminal terminal = new SystemTerminal();

            // It is important to understand the concept of a text style.
            // A text style contains all of the visual attributes that can be
            // applied to a character or string, such as underlying, boldness,
            // foreground color, etc.

            TextStyle style = new TextStyle();

            style.BackgroundColor = TextColor.Blue;
            style.ForegroundColor = TextColor.Yellow;
            style.ForegroundIntensity = TextIntensity.Bold;

            terminal.SetStyle(style);
            terminal.WriteLine("This text is bright yellow with a blue background.");

            // Often types you need to temporary change the color
            // and then restore the old color.  The next code
            // does this, but can result in difficult-to-read code
            // when used heavily.

            TextStyle oldStyle = terminal.GetStyle();
            terminal.SetStyle(new TextStyle(TextColor.Cyan));
            terminal.WriteLine("This text was manually set to cyan over a black background.");
            terminal.SetStyle(oldStyle);
            terminal.WriteLine("This color was restored manually.");

            // A better way is to use the color stack.  The color
            // stack allows you to "push" the current color onto the
            // stack and then change it.  Later you can pop out the
            // old color to restore.  The stack can be of any depth.

            terminal.PushStyle();
            terminal.SetStyle(new TextStyle(TextColor.Cyan));
            terminal.WriteLine("The old color was pushed; this text is cyan on a black background.");
            terminal.PopStyle();
            terminal.WriteLine("This is the old color restored.");

            // You can also push the current color and set a new color
            // in a single step.  

            terminal.PushStyle(new TextStyle(TextColor.Red));
            terminal.WriteLine("The old color was pushed and this color was set at the same time.");
            terminal.PopStyle();


            terminal.PromptEnter("Press ENTER to exit.");

        }
    }
}

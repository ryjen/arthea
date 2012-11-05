
using System;
using Thought.Terminals;

namespace Tests
{
    /// <summary>
    ///     This is a test translator that does nothing expect
    ///     implement the abstract methods with stubs.  This
    ///     allows the unit tests to create an instance of a 
    ///     translator for the purpose of testing code implemented
    ///     in the base class (such as protected methods).
    /// </summary>
    public class StubTranslator : Translator
    {
        public override char[] GetPrefixChars()
        {
            throw new NotImplementedException();
        }

        public override int Translate(Terminal terminal, string text, int index)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Provides access to the protected HasRoom function.
        /// </summary>
        public bool ExposeHasRoom(string text, int index, int minimumLength)
        {
            return HasRoom(text, index, minimumLength);
        }

        /// <summary>
        ///     Provides access to the protected IsHexDigit function.
        /// </summary>
        public bool ExposeIsHexDigit(char value)
        {
            return IsHexDigit(value);
        }

        /// <summary>
        ///     Provides access to the protected ParseHexChar function.
        /// </summary>
        public int ExposeParseHexChar(char value)
        {
            return ParseHexChar(value);
        }

        /// <summary>
        ///     Provides access to the protected ValidateParameters function.
        /// </summary>
        public void ExposeValidateParameters(
            Terminal terminal,
            string text,
            int index)
        {
            ValidateParameters(terminal, text, index);
        }

    }

}

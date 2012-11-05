
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;

namespace Thought.Terminals
{

    /// <summary>
    ///     Identifies the lookup name for a translator.
    /// </summary>
    public class TranslatorKeyAttribute : Attribute 
    {

        private string name;

        /// <summary>
        ///     Creates an attribute with the specified name.
        /// </summary>
        /// <param name="name">
        ///     The short lookup name of the translator.
        /// </param>
        public TranslatorKeyAttribute(string name)
        {
            this.name = name;
        }


        /// <summary>
        ///     The keyname of the translator.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

    }
}

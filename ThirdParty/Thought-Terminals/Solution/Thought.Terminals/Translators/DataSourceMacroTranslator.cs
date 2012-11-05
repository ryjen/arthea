
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;

namespace Thought.Terminals.Translators
{

    /// <summary>
    ///     Expands macros using fields from a data source.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A macro is an embedded text code in the format {MacroName};
    ///         this class expands macros that have the same name as a column
    ///         or field in a data source.  Support data sources are 
    ///         IDataRecords, DataRows, and Hashtables.
    ///     </para>
    /// </remarks>
    public class DataSourceMacroTranslator : MacroTranslator
    {

        private object dataSource;

        #region [ Constructors ]

        /// <summary>
        ///     Creates a new data source macro translator.
        /// </summary>
        public DataSourceMacroTranslator() : base() { }

        /// <summary>
        ///     Initializes the macro expander against the specified
        ///     data source.
        /// </summary>
        public DataSourceMacroTranslator(object dataSource):this()
        {
            this.dataSource = dataSource;
        }

        #endregion

        #region [ DataSource ]

        /// <summary>
        ///     Sets the table or record that acts as a source
        ///     of data for macro expansions.
        /// </summary>
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;
            }
        }

        #endregion

        #region [ Expand ]

        /// <summary>
        ///     Returns the value of a named field from the data source.
        /// </summary>
        protected override object Expand(string name)
        {

            // This method is only called by the base Translate
            // method and therefore the parameters should
            // be reliable.

            Debug.Assert(!string.IsNullOrEmpty(name));

            if (this.dataSource == null)
            
                // A null return value indicates an unrecognized
                // name.  Since the data source is not specified,
                // all names are unrecognized.

                return null;

            try
            {

                if (dataSource.GetType() == typeof(IDataRecord))
                    return ExpandDataRecord(name, (IDataRecord)dataSource);

                if (dataSource.GetType() == typeof(DataRow))
                    return ExpandDataRow(name, (DataRow)dataSource);

                if (dataSource.GetType() == typeof(Hashtable))
                    return ExpandHashTable(name, (Hashtable)dataSource);

                return null;
            }
            catch
            {

                // An exception was raised by one of the methods
                // responsible to reading a name from the data source.
                
                return null;
            }

        }

        #endregion

        #region [ ExpandDataRecord ]

        /// <summary>
        ///     Performs expansion from an IDataRecord data source.
        /// </summary>
        /// <param name="name">
        ///     The name of the field.
        /// </param>
        /// <param name="record">
        ///     The IDataRecord datra source.
        /// </param>
        private object ExpandDataRecord(string name, IDataRecord record)
        {

            // An exception is raised by GetOrdinal if the
            // specified name is bad.

            int ordinal = record.GetOrdinal(name);

            if (record.IsDBNull(ordinal))
            {

                // The function should not return null -- this signals the
                // macro expander that a name was not recognized (and should
                // be passed to the next expander).  A blank string, however,
                // means the name was recognized (but was merely empty).

                return string.Empty;

            }
            else
            {

                return record.GetValue(ordinal);
            }
        }

        #endregion

        #region [ ExpandDataRow ]

        /// <summary>
        ///     Expands a macro from a DataRow record.
        /// </summary>
        private object ExpandDataRow(string name, DataRow row)
        {

            // This is a private function and therefore the
            // parameters should (in theory) be reliable.

            Debug.Assert(
                !string.IsNullOrEmpty(name),
                "A blank name was passed to ExpandDataRow");

            if (row.IsNull(name))
            {
                return string.Empty;
            }
            else
            {
                return row[name];
            }

        }

        #endregion

        #region [ ExpandHashTable ]

        /// <summary>
        ///     Expands a named key from a hash table.
        /// </summary>
        private object ExpandHashTable(string name, Hashtable hashtable)
        {

            Debug.Assert(!string.IsNullOrEmpty(name));

            if (hashtable.ContainsKey(name))
            {
                return hashtable[name];
            }
            else
            {
                return null;
            }

        }

        #endregion

    }
}

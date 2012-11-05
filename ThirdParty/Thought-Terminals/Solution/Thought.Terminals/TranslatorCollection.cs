
/* =========================================================================
 * Terminal I/O Library for .NET
 * -------------------------------------------------------------------------
 * Copyright (c) 2007-2009 David Pinch; Licensed under tLGPL; See README.TXT
 * ========================================================================= */

using System;
using System.Collections;

namespace Thought.Terminals
{

    /// <summary>
    ///     A general-purpose collection of translators.
    /// </summary>
    public class TranslatorCollection : CollectionBase
    {

        private bool cachePrefixes;
        private Hashtable prefixHash;

        #region [ Constructors ]

        #region [ TranslatorCollection() ]

        /// <summary>
        ///     Creates an empty translator collection.
        /// </summary>
        public TranslatorCollection()
            : base()
        {
            this.cachePrefixes = false;
        }

        #endregion

        #region [ TranslatorCollection(bool) ]

        /// <summary>
        ///     Creates an empty translator collection with an option
        ///     to cache prefix characters.
        /// </summary>
        /// <param name="cachePrefixes">
        ///     Indicates whether the collection should cache
        ///     a list of translators associated with each
        ///     prefix character.
        /// </param>
        public TranslatorCollection(bool cachePrefixes):base()
        {

            if (cachePrefixes)
            {
                this.cachePrefixes = true;
                this.prefixHash = new Hashtable();
            }

        }

        #endregion

        #endregion

        #region [ Operator + ]

        /// <summary>
        ///     Combines a <see cref="TranslatorCollection"/> and a <see cref="Translator"/>.
        /// </summary>
        /// <param name="col">
        ///     A <see cref="TranslatorCollection"/>.
        /// </param>
        /// <param name="t">
        ///     A <see cref="Translator"/>.
        /// </param>
        /// <returns>
        ///     A new <see cref="TranslatorCollection"/> containing the contents
        ///     of the old translator collection and the new translator.
        /// </returns>
        public static TranslatorCollection operator +(
            TranslatorCollection col,
            Translator t)
        {

            TranslatorCollection newCollection = col.Clone();
            newCollection.Add(t);
            return newCollection;

        }

        /// <summary>
        ///     Combines a <see cref="TranslatorCollection"/> and a <see cref="Translator"/>.
        /// </summary>
        /// <param name="t">
        ///     A <see cref="Translator"/>.
        /// </param>
        /// <param name="c">
        ///     A <see cref="TranslatorCollection"/>.
        /// </param>
        /// <returns>
        ///     A new <see cref="TranslatorCollection"/> containing the contents
        ///     of the old translator collection and the new translator.
        /// </returns>
        public static TranslatorCollection operator +(
            Translator t,
            TranslatorCollection c)
        {
            TranslatorCollection newCollection = c.Clone();
            newCollection.Add(t);
            return newCollection;
        }


        /// <summary>
        ///     Combines two <see cref="TranslatorCollection"/> objects
        ///     into a single collection.
        /// </summary>
        /// <param name="c1">The 1st translator collection.</param>
        /// <param name="c2">The 2nd translator collection.</param>
        /// <returns>
        ///     A new collection containing all of the translators
        ///     from both collections.
        /// </returns>
        public static TranslatorCollection operator +(
            TranslatorCollection c1,
            TranslatorCollection c2)
        {
            TranslatorCollection newCollection = new TranslatorCollection();
            newCollection.AddRange(c1);
            newCollection.AddRange(c2);
            return newCollection;

        }

        #endregion

        #region [ Indexer ]

        /// <summary>
        ///     Sets or gets the translator at the specified 0-based collection index.
        /// </summary>
        public Translator this[int index]
        {
            get
            {
                return (Translator)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        #endregion

        #region [ Add(Translator) ]

        /// <summary>
        ///     Adds a new translator to the collection.
        /// </summary>
        public int Add(Translator translator)
        {

            if (translator == null)
            {
                // The user specified a null translator, which is
                // forbidden for this collection type (it may be
                // allowed in other collections but not this one).

                throw new System.ArgumentNullException("translator");
            }

            return List.Add(translator);

        }

        #endregion

        #region [ AddRange(TranslatorCollection) ]

        /// <summary>
        ///     Adds all of the translators in the specified
        ///     collection to this collection.
        /// </summary>
        /// <param name="translators">
        ///     An initialized translator collection.
        /// </param>
        public void AddRange(TranslatorCollection translators)
        {

            if (translators == null)
            {
                throw new ArgumentNullException("translators");
            }

            foreach (Translator translator in translators)
            {
                List.Add(translator);
            }

        }

        #endregion

        #region [ Clone ]

        /// <summary>
        ///     Creates a shallow copy of the translator collection.
        /// </summary>
        public TranslatorCollection Clone()
        {

            TranslatorCollection collection = new TranslatorCollection(this.cachePrefixes);
            collection.AddRange(this);
            return collection;

        }

        #endregion

        #region [ Contains ]

        /// <summary>
        ///     Returns true if the collection contains the specified <see cref="Translator"/>.
        /// </summary>
        public bool Contains(Translator translator)
        {
            return List.Contains(translator);
        }

        #endregion

        #region [ GetPrefixTranslators ]

        /// <summary>
        ///     Returns all translators that are known to process
        ///     the specified character.  Translators are 
        ///     returned in the order added to the collection.
        /// </summary>
        /// <param name="prefixChar">
        ///     A prefix character to a potential control code.
        /// </param>
        public TranslatorCollection GetPrefixTranslators(char prefixChar)
        {

            if (this.cachePrefixes)
            {

                // The collection was configured to cache
                // lists of translators associated with each
                // prefix character.  Therefore the fetch
                // operation is trivial.

                return (TranslatorCollection)prefixHash[prefixChar];

            }
            else
            {
                return null;
            }

        }

        #endregion

        #region [ IndexOf ]

        /// <summary>
        ///     Returns the collection index of the specified translator,
        ///     of -1 if the translator has not been added to the
        ///     collection.
        /// </summary>
        /// <param name="translator">
        ///     An initialized translator.
        /// </param>
        public int IndexOf(Translator translator)
        {
            return InnerList.IndexOf(translator);
        }

        #endregion

        #region [ Insert ]

        /// <summary>
        ///     Inserts a translator into the collection.
        /// </summary>
        public void Insert(int index, Translator translator)
        {
            List.Insert(index, translator);
        }

        #endregion

        #region [ OnInsertComplete ]

        /// <summary>
        ///     Executed when a translator has been inserted
        ///     into the collection.  The collection will 
        ///     cache information about the translator (such as its
        ///     prefix characters) for better performance when
        ///     translating text.
        /// </summary>
        protected override void OnInsertComplete(int index, object value)
        {

            if (this.cachePrefixes)
            {

                Translator translator = (Translator)value;

                foreach (char c in translator.GetPrefixChars())
                {

                    // Every prefix ever registered in the collection
                    // is associated with another collection containing
                    // all of the translators for that prefix.

                    TranslatorCollection list =
                        (TranslatorCollection)this.prefixHash[c];

                    if (list == null)
                    {
                        // The hashtable did not have a list for
                        // this character, which means this is the first
                        // time the character was added to the collection.
                        // Create a new translator collection -- do
                        // not configure the collection to perform its
                        // own hashtable caching!

                        list = new TranslatorCollection(false);

                        // Add the list to the hashtable.
                        // If any other translators use this key, that
                        // translator will be added to the list.

                        this.prefixHash.Add(c, list);

                    }

                    list.Add(translator);

                }

            }

            base.OnInsertComplete(index, value);
        }

        #endregion

        #region [ OnRemoveComplete ]

        /// <summary>
        ///     Overrides the OnRemoteComplete handler in order to
        ///     give the collection an opportunity to update any
        ///     internal indexes.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the value being removed.
        /// </param>
        /// <param name="value">
        ///     The value being removed.
        /// </param>
        protected override void OnRemoveComplete(int index, object value)
        {

            if (this.cachePrefixes)
            {

                // Recast the added object for easier access to 
                // its properties and methods.

                Translator translator = (Translator)value;

                foreach (char prefixChar in translator.GetPrefixChars())
                {

                    // Each hotchar ever registered in the collection
                    // is associated with an array list.  The list 
                    // contains each translator using that hotchar.
                    // Get the list for this translator.  

                    TranslatorCollection list = 
                        (TranslatorCollection)this.prefixHash[prefixChar];

                    if (list != null)
                    {
                        list.Remove(translator);
                    }

                    if (list.Count == 0)
                    {
                        this.prefixHash.Remove(prefixChar);
                    }
                }
            
            }


            base.OnRemoveComplete(index, value);
        }

        #endregion

        #region [ Remove ]

        /// <summary>
        ///     Removes a translator from the collection.
        /// </summary>
        public void Remove(Translator translator)
        {
            List.Remove(translator);
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using Aucent.MAX.AXE.XBRLParser;

namespace Aucent.MAX.AXE.XBRLParser.Searching
{
    /// <summary>
    /// </summary>
    public class LibrarySearchCriteria : ICloneable
    {
        /// <summary>
        /// </summary>
        public enum SearchableTextField
        {
            /// <summary>
            /// </summary>
            Labels,

            /// <summary>
            /// </summary>
            Names,

            /// <summary>
            /// </summary>
            Definitions,

            /// <summary>
            /// </summary>
            References,

            /// <summary>
            /// </summary>
            ElementId
        }

        #region members

        //IMPORTANT:  Any members added here need to be added to the Clone() and UpdateFrom() methods below
        private List<string> optionalWords;
        private List<string> requiredWords;
        private List<string> prohibitedWords;
        private List<Element.BalanceType> balanceTypes;
        private List<SearchableTextField> textFieldsToSearch;
        private bool isIncludeAbstractElements;
        private bool isIncludeExtendedElements;
        private bool isIncludeOnlyExtendedElements;
        private bool isMatchWholeWordsOnly;
        private string ancestryPath;
        private bool isConvertPluralsToSingular = true;
        private decimal sICPercentage;
        private decimal sECPercentage;
        private bool includeSICPercentage;
        private bool includeSECPercentage;

        #endregion

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria()
        {
            Reset();
        }

        /// <summary>
        /// Same as Reset(LibrarySearchCriteriaResetOptions.Full);
        /// </summary>
        /// <returns></returns>
        public LibrarySearchCriteria Reset()
        {
            return Reset(LibrarySearchCriteriaResetOptions.Full);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria Reset(LibrarySearchCriteriaResetOptions options)
        {
            if (!options.MakeNoChanges)
            {
                optionalWords = new List<string>();
                requiredWords = new List<string>();
                prohibitedWords = new List<string>();
                balanceTypes = new List<Element.BalanceType>();
                isIncludeAbstractElements = true;
                isIncludeExtendedElements = true;
                isIncludeOnlyExtendedElements = false;
                isMatchWholeWordsOnly = false;
                isConvertPluralsToSingular = true;
                textFieldsToSearch = new List<SearchableTextField>();
                textFieldsToSearch.Add(SearchableTextField.Labels);
                textFieldsToSearch.Add(SearchableTextField.Definitions);
                sECPercentage = 0;
                sICPercentage = 0;
                includeSECPercentage = false;
                includeSICPercentage = false;

                if (!options.KeepPath)
                    ancestryPath = null;
            }

            return this;
        }

        #region public getters

        /// <summary>
        /// </summary>
        public bool IncludeSECPercentage { get { return includeSECPercentage; } }

        /// <summary>
        /// </summary>
        public bool IncludeSICPercentage { get { return includeSICPercentage; } }

        /// <summary>
        /// </summary>
        public decimal SICPercentage { get { return sICPercentage; } }

        /// <summary>
        /// </summary>
        public decimal SECPercentage { get { return sECPercentage; } }

        /// <summary>
        /// </summary>
        public string[] OptionalWords { get { return optionalWords.ToArray(); } }

        /// <summary>
        /// </summary>
        public string[] RequiredWords { get { return requiredWords.ToArray(); } }

        /// <summary>
        /// </summary>
        public string[] ProhibitedWords { get { return prohibitedWords.ToArray(); } }

        /// <summary>
        /// </summary>
        public Element.BalanceType[] BalanceTypes { get { return balanceTypes.ToArray(); } }

        /// <summary>
        /// </summary>
        public SearchableTextField[] TextFieldsToSearch { get { return textFieldsToSearch.ToArray(); } }

        /// <summary>
        /// </summary>
        public bool IsIncludeAbstractElements { get { return isIncludeAbstractElements; } }

        /// <summary>
        /// </summary>
        public bool IsIncludeExtendedElements { get { return isIncludeExtendedElements; } }

        /// <summary>
        /// </summary>
        public bool IsIncludeOnlyExtendedElements { get { return isIncludeOnlyExtendedElements; } }

        /// <summary>
        /// </summary>
        public bool IsMatchWholeWordsOnly { get { return isMatchWholeWordsOnly; } }

        /// <summary>
        /// </summary>
        public string AncestryPath { get { return ancestryPath; } }

        /// <summary>
        /// </summary>
        public bool IsConvertPluralsToSingular { get { return isConvertPluralsToSingular; } }

        #endregion

        #region fluent interface (all setter methods that return 'this')

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetIncludeSICPercentage(bool include)
        {
            includeSICPercentage = include;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetIncludeSECPercentage(bool include)
        {
            includeSECPercentage = include;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetSICPercentage(decimal percentage)
        {
            sICPercentage = percentage;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetSECPercentage(decimal percentage)
        {
            sECPercentage = percentage;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria WithOptionalWords(string spaceDelimitedWords)
        {
            optionalWords.Clear();

            if( string.IsNullOrEmpty(spaceDelimitedWords) )
                return this;

            optionalWords.AddRange(spaceDelimitedWords.Split(' '));

            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AddOptionalWord(string word)
        {
            if( !optionalWords.Contains(word) )
                optionalWords.Add(word);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria WithRequiredWords(string spaceDelimitedWords)
        {
            requiredWords.Clear();

            if (string.IsNullOrEmpty(spaceDelimitedWords))
                return this;

            requiredWords.AddRange(spaceDelimitedWords.Split(' '));

            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AddRequiredWord(string word)
        {
            if (!requiredWords.Contains(word))
                requiredWords.Add(word);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria WithProhibitedWords(string spaceDelimitedWords)
        {
            prohibitedWords.Clear();

            if (string.IsNullOrEmpty(spaceDelimitedWords))
                return this;

            prohibitedWords.AddRange(spaceDelimitedWords.Split(' '));

            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AddProhibitedWord(string word)
        {
            if (!prohibitedWords.Contains(word))
                prohibitedWords.Add(word);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AddBalanceType(Element.BalanceType bt)
        {
            if (!balanceTypes.Contains(bt))
                balanceTypes.Add(bt);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria WithBalanceTypes(IEnumerable<Element.BalanceType> bTypes)
        {
            balanceTypes.Clear();
            balanceTypes.AddRange(bTypes);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria IncludingAbstractElements()
        {
            return SetIncludeAbstractElements(true);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria ExcludingAbstractElements()
        {
            return SetIncludeAbstractElements(false);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetIncludeAbstractElements(bool include)
        {
            isIncludeAbstractElements = include;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria IncludingExtendedElements()
        {
            return SetIncludeExtendedElements(true);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria IncludingOnlyExtendedElements()
        {
            return SetIncludeOnlyExtendedElements(true);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria ExcludingExtendedElements()
        {
            return SetIncludeExtendedElements(false);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetIncludeExtendedElements(bool include)
        {
            isIncludeExtendedElements = include;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetIncludeOnlyExtendedElements(bool include)
        {
            isIncludeOnlyExtendedElements = include;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria MatchingWholeWords()
        {
            return SetMatchWholeWordsOnly(true);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria MatchingPartialWords()
        {
            return SetMatchWholeWordsOnly(false);
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria SetMatchWholeWordsOnly(bool yes)
        {
            isMatchWholeWordsOnly = yes;
            isConvertPluralsToSingular = !yes;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AddTextFieldToSearch(SearchableTextField tf)
        {
            if (!textFieldsToSearch.Contains(tf))
                textFieldsToSearch.Add(tf);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria AgainstTextFields(IEnumerable<SearchableTextField> tFields)
        {
            textFieldsToSearch.Clear();
            textFieldsToSearch.AddRange(tFields);
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria DescendingFrom(string path)
        {
            ancestryPath = path;
            return this;
        }
        #endregion

        #region Quick Filters

        /// <summary>
        /// </summary>
        public LibrarySearchCriteria WithQuickFilter_AllExtendedElements()
        {
            Reset(LibrarySearchCriteriaResetOptions.Full)
                .IncludingAbstractElements()
                .IncludingOnlyExtendedElements()
                .DescendingFrom(null);

            return this;
        }
        #endregion

        /// <summary>
        /// </summary>
        public object Clone()
        {
            LibrarySearchCriteria result = new LibrarySearchCriteria();
            result.requiredWords = requiredWords;
            result.optionalWords = optionalWords;
            result.prohibitedWords = prohibitedWords;
            result.balanceTypes = balanceTypes;
            result.ancestryPath = AncestryPath;
            result.isIncludeAbstractElements = IsIncludeAbstractElements;
            result.isIncludeExtendedElements = IsIncludeExtendedElements;
            result.isIncludeOnlyExtendedElements = IsIncludeOnlyExtendedElements;
            result.isMatchWholeWordsOnly = IsMatchWholeWordsOnly;
            result.textFieldsToSearch = textFieldsToSearch;
            result.isConvertPluralsToSingular = isConvertPluralsToSingular;
            result.sICPercentage = sICPercentage;
            result.sECPercentage = sECPercentage;
            result.includeSECPercentage = includeSECPercentage;
            result.includeSICPercentage = includeSICPercentage;

            return result;
        }

        /// <summary>
        /// </summary>
        public void UpdateFrom(LibrarySearchCriteria from)
        {
            requiredWords = from.requiredWords;
            optionalWords = from.optionalWords;
            prohibitedWords = from.prohibitedWords;
            balanceTypes = from.balanceTypes;
            ancestryPath = from.ancestryPath;
            isIncludeAbstractElements = from.isIncludeAbstractElements;
            isIncludeExtendedElements = from.isIncludeExtendedElements;
            isIncludeOnlyExtendedElements = from.isIncludeOnlyExtendedElements;
            isMatchWholeWordsOnly = from.isMatchWholeWordsOnly;
            textFieldsToSearch = from.textFieldsToSearch;
            isConvertPluralsToSingular = from.isConvertPluralsToSingular;
            sICPercentage = from.SICPercentage;
            sECPercentage = from.SECPercentage;
            includeSECPercentage = from.includeSECPercentage;
            includeSICPercentage = from.includeSICPercentage;
        }
    }

    /// <summary>
    /// </summary>
    public class LibrarySearchCriteriaResetOptions
    {
        /// <summary>
        /// </summary>
        public LibrarySearchCriteriaResetOptions()
        {
            KeepPath = false;
            MakeNoChanges = false;
        }

        private bool keepPath;
        /// <summary>
        /// </summary>
        public bool KeepPath
        {
            get
            {
                return keepPath;
            }
            set
            {
                keepPath = value;
            }
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteriaResetOptions WithKeepPath()
        {
            KeepPath = true;
            return this;
        }

        /// <summary>
        /// </summary>
        public LibrarySearchCriteriaResetOptions WithMakeNoChanges()
        {
            MakeNoChanges = true;
            return this;
        }

        private bool makeNoChanges;
        /// <summary>
        /// </summary>
        public bool MakeNoChanges
        {
            get
            {
                return makeNoChanges;
            }
            set
            {
                makeNoChanges = value;
            }
        }

        /// <summary>
        /// </summary>
        public static LibrarySearchCriteriaResetOptions Full { get { return new LibrarySearchCriteriaResetOptions(); } }

        /// <summary>
        /// </summary>
        public static LibrarySearchCriteriaResetOptions ChangeNothing { get { return new LibrarySearchCriteriaResetOptions().WithMakeNoChanges(); } }

    }
}

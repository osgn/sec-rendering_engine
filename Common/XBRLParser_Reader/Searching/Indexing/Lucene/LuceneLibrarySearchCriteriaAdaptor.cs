using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene
{
    ///<summary>
    ///</summary>
    public class LuceneLibrarySearchCriteriaAdaptor
    {
        /// <summary>
        /// CKC - 11/21/08 - I modifed the Lucene PorterStemmer in the following ways because I felt they didn't match our usage:
        ///     Step 1: Words ending with "ies" are trimmed by three chars (normal Porter stemming would replace "ies" with "i"
        ///     Step 2: Step 2 is now ignored (normal Porter stemming would convert words ending with y and have another vowel to end with i instead)
        /// </summary>
        private readonly PorterStemmer pluralStemmer = new PorterStemmer();

        ///<summary>
        ///</summary>
        public static Analyzer ANALYZER { get { return new StandardAnalyzer(); } }

        private static readonly List<string> stopWords = new List<string>(StopAnalyzer.ENGLISH_STOP_WORDS);

        private static bool IsStopWord(string word)
        {
            return stopWords.Contains(word);
        }

        private string _MakeWordsQuery(ICollection<string> words, ICollection<LibrarySearchCriteria.SearchableTextField> inTheseTextFields, bool matchWholeWordsOnly, bool isConvertPluralsToSingular, string matchTypeOperator)
        {
            StringBuilder result = new StringBuilder();

            string matchWholeWordsFragment = matchWholeWordsOnly ? string.Empty : "*";

            int boost = words.Count * inTheseTextFields.Count;

            List<string> cleanedWords = new List<string>();
            foreach(string word in words)
            {
                if( !IsStopWord(word))
                    cleanedWords.Add(CleanWord(word, isConvertPluralsToSingular));
            }

            foreach (LibrarySearchCriteria.SearchableTextField tf in inTheseTextFields)
            {
                result.Append(string.Format("{0}:(", LuceneNodeIndexer.GetFieldName(tf)));

                if (words.Count > 1 && tf == LibrarySearchCriteria.SearchableTextField.Labels)
                {
                    result.Append(string.Format("\"{0}\"~6 ", string.Join(" ", new List<string>(words).ToArray())));
                }

                foreach (string cleanWord in cleanedWords)
                {
                    result.Append(string.Format("{0}{1}{2}^{3} ", matchTypeOperator, cleanWord, matchWholeWordsFragment, boost));
                    --boost;
                }

                result.Remove(result.Length - 1, 1); //Trim the last space from the end
                result.Append(") ");
            }

            if( result.Length > 1 )
                result.Remove(result.Length - 1, 1); //Trim the last space from the end
            
            return result.ToString();            
        }

        private string MakeOneOrMoreWordsQuery(ICollection<string> words, ICollection<LibrarySearchCriteria.SearchableTextField> inTheseTextFields, bool matchWholeWordsOnly, bool isConvertPluralsToSingular)
        {
            if (words == null || words.Count == 0)
                return null;

            string query = _MakeWordsQuery(words, inTheseTextFields, matchWholeWordsOnly, isConvertPluralsToSingular, string.Empty);

            if (string.IsNullOrEmpty(query))
                return null;

            return string.Format("({0})", query);
        }

        private string ConvertFromPluralToSingular(string word)
        {
            string result = pluralStemmer.Stem(word);

            //Example:  Gaming will be stemmed to "game"
            if( word != null && word.EndsWith("ing") )
            {
                int origLength = word.Length;
                int newLength = result.Length;

                if( result.EndsWith("e") && origLength - newLength == 2)
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }

        private string CleanWord(string word, bool isConvertPluralsToSingular)
        {
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            string result = word.Trim().Replace(" ", "?");

            if (isConvertPluralsToSingular)
                result = ConvertFromPluralToSingular(result);

            return result;
        }

        private string MakeAllWordsQuery(ICollection<string> words, ICollection<LibrarySearchCriteria.SearchableTextField> inTheseTextFields, bool matchWholeWordsOnly, bool isConvertPluralsToSingular)
        {
            if (words == null || words.Count == 0)
                return null;

            string query = _MakeWordsQuery(words, inTheseTextFields, matchWholeWordsOnly, isConvertPluralsToSingular, "+");

            if (string.IsNullOrEmpty(query))
                return null;

            return string.Format("+({0})", query);
        }

        private string MakeProhibitedWordsQuery(ICollection<string> words, ICollection<LibrarySearchCriteria.SearchableTextField> inTheseTextFields, bool matchWholeWordsOnly, bool isConvertPluralsToSingular)
        {
            if (words == null || words.Count == 0)
                return null;

            string query = _MakeWordsQuery(words, inTheseTextFields, matchWholeWordsOnly, isConvertPluralsToSingular, string.Empty);

            if( string.IsNullOrEmpty(query) )
                return null;

            return string.Format("-({0})", query);
        }

        private static string MakeBalanceTypeQuery(ICollection<Element.BalanceType> balanceTypes)
        {
            if (balanceTypes == null || balanceTypes.Count == 0)
                return null;

            StringBuilder result = new StringBuilder("(");
            string delimiter = string.Empty;

            foreach(Element.BalanceType bt in balanceTypes)
            {
                result.AppendFormat("{0}{1}:{2}", delimiter, LuceneNodeIndexer.BALANCETYPE_FIELD, bt);
                delimiter = " ";
            }

            return result.Append(')').ToString();
        }

        private static string MakeIncludeNullIfFalseXXXElementsQuery(bool include, string fieldName)
        {
            if (!include)
            {
                return null;
            }

            return string.Format("({0}:{1})", fieldName, true.ToString().ToLower());
        }

        private static string MakeNullIfTrueIncludeXXXElementsQuery(bool include, string fieldName)
        {
            if (include)
            {
                return null;
            }

            return string.Format("({0}:{1})", fieldName, false.ToString().ToLower());
        }

//        private static string MakePathQuery(string path)
//        {
//            if( string.IsNullOrEmpty(path) )
//            {
//                return null;
//            }
//
//            return string.Format("({0}:\"{1}/*\")", LuceneNodeIndexer.PATH_FIELD, path.Replace(LuceneNodeIndexer.PATH_EOL_MARKER,string.Empty));
//        }

        ///<summary>
        ///</summary>
        public string BuildQueryString(LibrarySearchCriteria criteria)
        {
            string oneOrMoreWordsFragment = MakeOneOrMoreWordsQuery(criteria.OptionalWords, criteria.TextFieldsToSearch, criteria.IsMatchWholeWordsOnly, criteria.IsConvertPluralsToSingular);
            string allWordsFragment = MakeAllWordsQuery(criteria.RequiredWords, criteria.TextFieldsToSearch, criteria.IsMatchWholeWordsOnly, criteria.IsConvertPluralsToSingular);
            string prohibitedWordsFragment = MakeProhibitedWordsQuery(criteria.ProhibitedWords, criteria.TextFieldsToSearch, criteria.IsMatchWholeWordsOnly, criteria.IsConvertPluralsToSingular);
            string balanceTypeFragment = MakeBalanceTypeQuery(criteria.BalanceTypes);
            string includeAbstractFragment = MakeNullIfTrueIncludeXXXElementsQuery(criteria.IsIncludeAbstractElements, LuceneNodeIndexer.ISABSTRACT_FIELD);
            string includeExtendedFragment = MakeNullIfTrueIncludeXXXElementsQuery(criteria.IsIncludeExtendedElements, LuceneNodeIndexer.ISEXTENDED_FIELD);
            string includeOnlyExtendedFragment = MakeIncludeNullIfFalseXXXElementsQuery(criteria.IsIncludeOnlyExtendedElements, LuceneNodeIndexer.ISEXTENDED_FIELD);
            //string pathFragment = MakePathQuery(criteria.AncestryPath);

            StringBuilder result = new StringBuilder();

            bool needsAndDelimiter = false;
            const string andDelimiter = " AND ";
            
            if( !string.IsNullOrEmpty(oneOrMoreWordsFragment) )
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, oneOrMoreWordsFragment);
                needsAndDelimiter = true;
            }

            if( !string.IsNullOrEmpty(allWordsFragment) )
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, allWordsFragment);
                needsAndDelimiter = true;
            }

            if (!string.IsNullOrEmpty(prohibitedWordsFragment))
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, prohibitedWordsFragment);
                needsAndDelimiter = true;
            }

            if (!string.IsNullOrEmpty(balanceTypeFragment))
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, balanceTypeFragment);
                needsAndDelimiter = true;
            }

            if (!string.IsNullOrEmpty(includeAbstractFragment))
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, includeAbstractFragment);
            }

            if (!string.IsNullOrEmpty(includeExtendedFragment))
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, includeExtendedFragment);
            }

            if (!string.IsNullOrEmpty(includeOnlyExtendedFragment))
            {
                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, includeOnlyExtendedFragment);
            }

//            if (!string.IsNullOrEmpty(pathFragment))
//            {
//                result.AppendFormat("{0}{1}", needsAndDelimiter ? andDelimiter : string.Empty, pathFragment);
//            }

            if( result.Length == 0 )
                return null;

            return result.ToString();
        }

        ///<summary>
        ///</summary>
        public Query BuildQuery(LibrarySearchCriteria criteria)
        {
            if( criteria == null )
                return null;

            QueryParser queryParser = new QueryParser(string.Empty, ANALYZER);

            string queryString = BuildQueryString(criteria);

            if( string.IsNullOrEmpty(queryString) )
            {
				Debug.WriteLine("Ignoring search request because the resultant query string is null or empty:  Returning null");
                return null;
            }

			Debug.WriteLine("Searching with query: " + queryString);

            return queryParser.Parse(queryString);
        }
    }
}

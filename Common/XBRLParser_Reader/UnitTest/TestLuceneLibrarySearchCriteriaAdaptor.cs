using Aucent.MAX.AXE.XBRLParser.Searching;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class TestLuceneLibrarySearchCriteriaAdaptor
    {
        private static void AssertQueryString(string expected, LibrarySearchCriteria crit)
        {
            LuceneLibrarySearchCriteriaAdaptor adaptor = new LuceneLibrarySearchCriteriaAdaptor();
            string result = adaptor.BuildQueryString(crit);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords()
        {
            string expectedResult = "(Label:(\"Joe Dirt\"~6 Joe^8 Dirt^7) Definition:(Joe^6 Dirt^5) Name:(Joe^4 Dirt^3) References:(Joe^2 Dirt^1))";
            //string expectedResult = "(Label:Joe^8 Definition:Joe^6 Name:Joe^4 References:Joe^2 Label:Dirt^7 Definition:Dirt^5 Name:Dirt^3 References:Dirt^1)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithOptionalWords("Joe Dirt")
                .MatchingWholeWords()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[]
                                       {
                                           LibrarySearchCriteria.SearchableTextField.Labels,
                                           LibrarySearchCriteria.SearchableTextField.Definitions,
                                           LibrarySearchCriteria.SearchableTextField.Names,
                                           LibrarySearchCriteria.SearchableTextField.References
                                       });

            AssertQueryString(expectedResult,crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_AllWords()
        {
            string expectedResult = "(Label:(Joe^1)) AND +(Label:(\"Dirt Kid\"~6 +Dirt^2 +Kid^1))";
//            string expectedResult = "(Label:Joe^1) AND +(Label:Dirt^2) AND +(Label:Kid^1)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .WithRequiredWords("Dirt Kid")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] {LibrarySearchCriteria.SearchableTextField.Labels});

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_AllWords_And_Many_Fields()
        {
            string expectedResult = "+(Label:(\"Dirt Kid\"~6 +Dirt^4 +Kid^3) Definition:(+Dirt^2 +Kid^1))";
            //string expectedResult = "+(Label:Dirt^4 Definition:Dirt^2) AND +(Label:Kid^3 Definition:Kid^1)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithRequiredWords("Dirt Kid")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels, LibrarySearchCriteria.SearchableTextField.Definitions });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_AllWords_Not_Matching_Whole_Words()
        {
            string expectedResult = "(Label:(Joe*^1)) AND +(Label:(+Dirt*^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .WithOptionalWords("Joe")
                .MatchingPartialWords()
                .WithRequiredWords("Dirt")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_ProhibitedWords()
        {
            string expectedResult = "(Label:(Joe^1)) AND -(Label:(-Dirt^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .WithProhibitedWords("Dirt")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ProhibitedWords_Only()
        {
            string expectedResult = "-(Label:(-Dirt^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithProhibitedWords("Dirt")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_BalanceType()
        {
            string expectedResult = "(Label:(Joe^1)) AND (BalanceType:credit BalanceType:debit)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] {LibrarySearchCriteria.SearchableTextField.Labels})
                .WithBalanceTypes(new Element.BalanceType[] {Element.BalanceType.credit, Element.BalanceType.debit});

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_IncludeAbstract()
        {
            string expectedResult = "(Label:(Joe^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .IncludingAbstractElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_Do_Not_IncludeAbstract()
        {
            string expectedResult = "(Label:(Joe^1)) AND (IsAbstract:false)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .ExcludingAbstractElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_IncludeExtended()
        {
            string expectedResult = "(Label:(Joe^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .IncludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_Do_Not_IncludeExtended()
        {
            string expectedResult = "(Label:(Joe^1)) AND (IsExtended:false)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .ExcludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Path()
        {
            //NOTE PATH IS NO LONGER USED BY LUCENE AS A FILTER BECAUSE I COULDN'T MAKE IT WORK RIGHT WITH SOME TOP-LEVEL GROUPS.
            //NOW ALL RESULTS ARE RETURNED FROM LUCENE AND MANUALLY FILTERED BY LuceneLibrarySearchResult
            string expectedResult = "(Label:(Joe^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .DescendingFrom("/A/Path/Is/HereAUEND")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void OneOrMoreWords_With_No_Fields()
        {
            const string expectedResult = null;

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .IncludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[]{});

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void RequiredWords_With_No_Fields()
        {
            const string expectedResult = null;

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .IncludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ProhibitedWords_With_No_Fields()
        {
            const string expectedResult = null;

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithProhibitedWords("Joe")
                .IncludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ConvertPluralsToSingular_With_Partial_Word_Matching()
        {
            const string expectedResult = "(Label:(\"Joes Fishes Exes Candies Dresses Handles\"~6 Joe*^6 Fish*^5 Exe*^4 Cand*^3 Dress*^2 Handl*^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingPartialWords()
                .WithOptionalWords("Joes Fishes Exes Candies Dresses Handles")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ConvertPluralsToSingular_With_Gaming()
        {
            const string expectedResult = "(Label:(Gam*^1))";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingPartialWords()
                .WithOptionalWords("Gaming")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ConvertPluralsToSingular_With_Whole_Word_Matching()
        {
            const string expectedResult = "(Label:(\"Joes Fishes Exes Candies Dresses Handles\"~6 Joes^6 Fishes^5 Exes^4 Candies^3 Dresses^2 Handles^1))";
            //const string expectedResult = "(Label:Joes^6 Label:Fishes^5 Label:Exes^4 Label:Candies^3 Label:Dresses^2 Label:Handles^1)";

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joes Fishes Exes Candies Dresses Handles")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            AssertQueryString(expectedResult, crit);
        }
    }
}
#endif

using Aucent.MAX.AXE.XBRLParser.Searching;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene;
using Lucene.Net.Index;
using Lucene.Net.Search;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class TestLuceneIndexMgr : BaseLuceneIndexMgrTest
    {
        /// <summary>
        /// </summary>
        protected override string GetTaxonomyPath()
        {
            return @"xUSGAAP_CI\us-gaap-ci-2005-02-28.xsd";
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Initialize_Indexes_All_Nodes()
        {
            string elementIdForTestingSearch = _deepNodeFinder.GetNodesForIndexing()[0].Id;
            int expectedNumNodes = _deepNodeFinder.GetNodesForIndexing().Length;

            Assert.AreEqual("usfr-pte_NetCashFlowsProvidedUsedOperatingActivitiesDirectAbstract", elementIdForTestingSearch,
                            "TEST SANITY: element id for test search");
            Assert.AreEqual(1595, expectedNumNodes, "TEST SANITY: Number of nodes in found in the test taxonomy");

            IndexReader indexReader = IndexReader.Open(_indexMgr.LuceneDirectory_ForTesting);

            Assert.AreEqual(expectedNumNodes, indexReader.NumDocs(),
                            "An incorrect number of documents were found in the Lucene directory after initialization");

            IndexSearcher searcher = new IndexSearcher(_indexMgr.LuceneDirectory_ForTesting);
            try
            {
                Hits results =
                    searcher.Search(new TermQuery(new Term(LuceneNodeIndexer.ELEMENTID_FOR_DELETING_FIELD, elementIdForTestingSearch)));

                Assert.AreEqual(1, results.Length(), "Search results should only have 1 hit");
                Assert.AreEqual(elementIdForTestingSearch, results.Doc(0).Get(LuceneNodeIndexer.ELEMENTID_FIELD),
                                "Search results yielded the wrong element!");
            }
            finally
            {
                searcher.Close();
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search()
        {
            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash");

            ILibrarySearchResult result = _indexMgr.Search(criteria);

            Assert.IsNotNull(result, "Search results should not be null");
            Assert.AreEqual(143,result.Count);
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Plural()
        {
            ILibrarySearchResult pluralResult = _indexMgr.Search(
                new LibrarySearchCriteria()
                    .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] {LibrarySearchCriteria.SearchableTextField.Labels})
                    .WithRequiredWords("Costs"));

            ILibrarySearchResult singularResult = _indexMgr.Search(
                new LibrarySearchCriteria()
                    .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels })
                    .WithRequiredWords("Cost"));

            Assert.AreEqual(pluralResult.Count, singularResult.Count,"Plural and Singular result hit numbers should be the same");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_Filters_BannedWords()
        {
            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .MatchingPartialWords()
                .WithProhibitedWords("Dividend Flow")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            ILibrarySearchResult result = _indexMgr.Search(criteria);

            Assert.IsNotNull(result, "Search results should not be null");
            Assert.AreEqual(30, result.Count);

            foreach (ILibrarySearchResultItem hit in result.Items)
            {
                Assert.IsTrue(hit.Label.Contains("Cash"), "Hit with Label '" + hit.Label + "' does not contain 'Cash'");
                Assert.IsFalse(hit.Label.Contains("Dividend"), "Hit with Label '" + hit.Label + "' contains 'Dividend'");
                Assert.IsFalse(hit.Label.Contains("Flow"), "Hit with Label '" + hit.Label + "' contains 'Flow'");
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_Filters_RequiredWords()
        {
            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .MatchingPartialWords()
                .WithRequiredWords("Dividend")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels });

            ILibrarySearchResult result = _indexMgr.Search(criteria);

            Assert.IsNotNull(result, "Search results should not be null");
            Assert.AreEqual(13, result.Count);

            foreach (ILibrarySearchResultItem hit in result.Items)
            {
                Assert.IsTrue(hit.Label.Contains("Cash"), "Hit with Label '" + hit.Label + "' does not contain 'Cash'");
                Assert.IsTrue(hit.Label.Contains("Dividend"), "Hit with Label '" + hit.Label + "' does not contain 'Dividend'");
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_Filters_Abstract()
        {
            //Bad test case...this taxonomy has no abstract elements...

            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .IncludingAbstractElements();
            ILibrarySearchResult resultWith = _indexMgr.Search(criteria);

            criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .ExcludingAbstractElements();
            ILibrarySearchResult resultWithout = _indexMgr.Search(criteria);

            Assert.AreEqual(143,resultWith.Count,"hit count INCLUDING abstract elements in results");
            Assert.AreEqual(112,resultWithout.Count,"hit count EXCLUDING abstract elements in results");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_Filters_Extended()
        {
            //Bad test case...this taxonomy has no extended elements...

            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .IncludingExtendedElements();
            ILibrarySearchResult resultWith = _indexMgr.Search(criteria);

            criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .ExcludingExtendedElements();
            ILibrarySearchResult resultWithout = _indexMgr.Search(criteria);

            Assert.AreEqual(143, resultWith.Count, "hit count INCLUDING extended elements in results");
            Assert.AreEqual(143, resultWithout.Count, "hit count EXCLUDING extended elements in results");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_Filters_BalanceType()
        {
            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels })
                .WithBalanceTypes(new Element.BalanceType[] { Element.BalanceType.credit});
            ILibrarySearchResult resultCredit = _indexMgr.Search(criteria);

            criteria = new LibrarySearchCriteria()
                .WithOptionalWords("Cash")
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.Labels })
                .WithBalanceTypes(new Element.BalanceType[] { Element.BalanceType.debit });
            ILibrarySearchResult resultDebit = _indexMgr.Search(criteria);

            Assert.AreEqual(4, resultCredit.Count, "hit count for credit balance type");
            Assert.AreEqual(16, resultDebit.Count, "hit count for debit balance type");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Path()
        {
            LibrarySearchCriteria criteria = new LibrarySearchCriteria()
                .WithRequiredWords("Hedging")
                .DescendingFrom("/Notes to the Financial Statements/usfr-pte_NotesFinancialStatementsAbstract/usfr-pte_GeneralNotesAbstract/usfr-pte_DerivativesHedgesNote");

            ILibrarySearchResult result = _indexMgr.Search(criteria);

            Assert.AreEqual(30,result.Count,"Num results");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void Search_With_No_Search_Fields_Selected_Returns_A_Zero_Hits_Result()
        {
            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords("Joe")
                .IncludingExtendedElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { });

            ILibrarySearchResult result = _indexMgr.Search(crit);

            Assert.AreEqual(0, result.Count, "Num results");
        }

        /// <summary>
        /// </summary>
        [Test]
        public void ReIndexNode_Replaces_Old_Document()
        {
            Node node = GetNodeProvider().GetNodesForIndexing()[2];

            LibrarySearchCriteria crit = new LibrarySearchCriteria()
                .MatchingWholeWords()
                .WithOptionalWords(node.Id)
                .IncludingExtendedElements()
                .IncludingAbstractElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[] { LibrarySearchCriteria.SearchableTextField.ElementId });

            ILibrarySearchResult result = _indexMgr.Search(crit);

            Assert.AreEqual(1,result.Count, "TEST SANITY: There should only be one result");
            Assert.AreEqual(node.Label,result.Items[0].Label, "TEST SANITY: Node label should match the result label");

            string oldLabel = node.Label;
            string newLabel = oldLabel + " UNIT TEST";

            node.Label = newLabel;

            _indexMgr.ReIndexNode(node);

            ILibrarySearchResult result2 = _indexMgr.Search(crit);

            Assert.AreEqual(1, result2.Count, "There should only be one result");
            Assert.AreEqual(newLabel, result2.Items[0].Label, "The result label should match the new node label");

            //Reset the index for other tests
            node.Label = oldLabel;
            _indexMgr.ReIndexNode(node);
        }
    }
}
#endif

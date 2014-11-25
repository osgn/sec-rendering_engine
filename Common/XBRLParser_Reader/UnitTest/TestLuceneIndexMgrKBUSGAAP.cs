using Aucent.MAX.AXE.XBRLParser.Searching;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class TestLuceneIndexMgrKBUSGAAP : BaseLuceneIndexMgrTest
    {
        /// <summary>
        /// </summary>
        protected override string GetTaxonomyPath()
        {
            return @"KB-USGAAP\KB-USGAAP.xsd";
        }

        /// <summary>
        /// </summary>
        [Test]
        public void AbstractElements()
        {
            LibrarySearchCriteria includingAbstractCriteria = new LibrarySearchCriteria()
                .WithOptionalWords("SalesRevenueGoodsNetAbstract")
                .MatchingPartialWords()
                .IncludingAbstractElements()
                .AgainstTextFields(new LibrarySearchCriteria.SearchableTextField[]{LibrarySearchCriteria.SearchableTextField.Labels})
                .DescendingFrom("/124000 - Statement - Statement of Income (Including Gross Margin)");

            ILibrarySearchResult resultWithAbstract = _indexMgr.Search(includingAbstractCriteria);

            Assert.IsNotNull(resultWithAbstract, "Search results (with abstract elements) should not be null");
            Assert.AreEqual(1, resultWithAbstract.Count);

            LibrarySearchCriteria excludingAbstractCriteria = (includingAbstractCriteria.Clone() as LibrarySearchCriteria).ExcludingAbstractElements();

            ILibrarySearchResult resultWithoutAbstract = _indexMgr.Search(excludingAbstractCriteria);

            Assert.IsNotNull(resultWithoutAbstract, "Search results (without abstract elements) should not be null");
            Assert.AreEqual(0, resultWithoutAbstract.Count);
        }
    }
}
#endif

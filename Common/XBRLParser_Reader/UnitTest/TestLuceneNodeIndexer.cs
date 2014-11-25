using System;
using System.Collections;
using System.Collections.Generic;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;
//using Rhino.Mocks;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class TestLuceneNodeIndexer
    {

        // THIS Test was removed to decouple rhino mock.  the codebase only runs as .net 2.0 and the current version of
        // rhino is 3.5

        /// <summary>
        /// </summary>
        //[Test]
        //public void AddToIndex_Adds_Document_With_The_Correct_Fields()
        //{
        //    List<Document> indexedDocuments = new List<Document>();

        //    string expectedEid = "usfr-pte_NetCashFlowsProvidedUsedOperatingActivitiesIndirectAbstract";
        //    string expectedLbl = "Net Cash Flows Provided By/(Used In) Operating Activities, Indirect";
        //    string expectedDef = "The net amount for cash inflows and outflows arising from operating activities (activities not classified as financing or investing; e.g. production/sale of goods, providing service, buy/sale trading securities, etc.) during an accounting period calculated by converting accrual-basis net income to cash basis net operating cash flows indirectly  Note: This element serves as a category heading only.  No data may be tagged to this element.";

        //    Taxonomy tax = BaseLuceneIndexMgrTest.ParseTaxonomy(@"xUSGAAP_CI\us-gaap-ci-2005-02-28.xsd");

        //    ArrayList nodes = tax.GetNodesByPresentation();
        //    Assert.IsNotNull(nodes,"Nodes should not be null");

        //    Node node = ((Node)nodes[1]).Children[0] as Node;

        //    Assert.IsNotNull(node,"Node should not be null");
        //    Assert.AreEqual(expectedEid, node.Id, "TEST SANITY: Unexpected element id");
        //    Assert.AreEqual(expectedLbl, node.Label, "TEST SANITY: Unexpected label for the test subject node");
        //    Assert.AreEqual(expectedDef, node.GetDefinition("en"), "TEST SANITY: Unexpected definition for the test subject node");

        //    MockRepository repos = new MockRepository();
        //    IndexWriter indexWriter = repos.PartialMock<IndexWriter>(new object[] { new RAMDirectory(), LuceneLibrarySearchCriteriaAdaptor.ANALYZER, true });
        //    indexWriter.AddDocument(new Document());
        //    LastCall.IgnoreArguments().Do(new Action<Document>(indexedDocuments.Add));

        //    repos.ReplayAll();

        //    LuceneNodeIndexer nodeIndexer = new LuceneNodeIndexer(indexWriter, "en");

        //    nodeIndexer.AddToIndex(node,"preferredLabel");

        //    Assert.AreEqual(1, indexedDocuments.Count, "One document should have been added to the indexWriter");
        //    Assert.AreEqual(expectedEid, indexedDocuments[0].Get(LuceneNodeIndexer.ELEMENTID_FIELD), "The element_id that was indexed");
        //    Assert.AreEqual(expectedLbl, indexedDocuments[0].Get(LuceneNodeIndexer.LABEL_FIELD), "The label that was indexed");
        //    Assert.AreEqual(expectedDef, indexedDocuments[0].Get(LuceneNodeIndexer.DEFINITION_FIELD), "The definition that was indexed");
        //    Assert.AreEqual(node.BalanceType, indexedDocuments[0].Get(LuceneNodeIndexer.BALANCETYPE_FIELD), "The balancetype that was indexed");
        //    Assert.AreEqual(node.IsAbstract.ToString().ToLower(), indexedDocuments[0].Get(LuceneNodeIndexer.ISABSTRACT_FIELD), "The isabstract that was indexed");
        //    Assert.AreEqual(node.IsAucentExtendedElement.ToString().ToLower(), indexedDocuments[0].Get(LuceneNodeIndexer.ISEXTENDED_FIELD), "The isextended that was indexed");
        //    Assert.AreEqual(node.Name, indexedDocuments[0].Get(LuceneNodeIndexer.NAME_FIELD), "The name that was indexed");
        //    Assert.AreEqual(LuceneNodeIndexer.MakePathForNode(node), indexedDocuments[0].Get(LuceneNodeIndexer.PATH_FIELD), "The path that was indexed");
        //}

        /// <summary>
        /// </summary>
        [Test]
        public void MakePathForNode()
        {
            Taxonomy taxonomy = BaseLuceneIndexMgrTest.ParseTaxonomy(@"xUSGAAP_CI\us-gaap-ci-2005-02-28.xsd");

            Node node = ((Node)((Node)taxonomy.GetNodesByPresentation()[0]).Children[0]).Children[0] as Node;

            string expected = "/Cash Flow from Operations - Direct Method/usfr-pte_NetCashFlowsProvidedUsedOperatingActivitiesDirectAbstract/usfr-pte_CashSaleGoodsServicesAUEND";

            Assert.AreEqual(expected,LuceneNodeIndexer.MakePathForNode(node));
        }
    }
}
#endif
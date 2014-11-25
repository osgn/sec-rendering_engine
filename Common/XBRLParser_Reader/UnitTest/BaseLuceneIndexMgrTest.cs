using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Aucent.MAX.AXE.XBRLParser.Searching;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.UnitTests
{
    /// <summary>
    /// </summary>
    public abstract class BaseLuceneIndexMgrTest
    {
        /// <summary>
        /// </summary>
        public static readonly string _TestTaxonomiesDirectory = @"S:\TestSchemas";

        /// <summary>
        /// </summary>
        protected readonly List<AbstractIndexMgr> indexMrgsNeedingCleanupAtTearDown = new List<AbstractIndexMgr>();

        /// <summary>
        /// </summary>
        protected LuceneIndexMgr _indexMgr;

        /// <summary>
        /// </summary>
        protected INodesForIndexingProvider _deepNodeFinder;

        /// <summary>
        /// </summary>
        protected abstract string GetTaxonomyPath();

        /// <summary>
        /// </summary>
        [TestFixtureTearDown]
        public void TearDownOnlyOnce()
        {
            if (indexMrgsNeedingCleanupAtTearDown.Count > 0)
            {
                foreach (AbstractIndexMgr im in indexMrgsNeedingCleanupAtTearDown)
                {
                    im.Close();
                }
            }

            indexMrgsNeedingCleanupAtTearDown.Clear();
        }

        /// <summary>
        /// </summary>
        [TestFixtureSetUp]
        public void SetupOnlyOnce()
        {
            _deepNodeFinder = GetNodeProvider();
            _indexMgr = InitializeIndex(_deepNodeFinder);
        }

        /// <summary>
        /// </summary>
        public static Taxonomy ParseTaxonomy(string pathFragment)
        {
            Taxonomy IMTaxonomy = new Taxonomy();

            string path = _TestTaxonomiesDirectory + "\\" + pathFragment;

            if (File.Exists(path))
            {
                int errors;
                IMTaxonomy.Load(path);
                IMTaxonomy.Parse(out errors);
                IMTaxonomy.CurrentLanguage = "en";
                IMTaxonomy.CurrentLabelRole = "label";
                return IMTaxonomy;
            }

            return null;
        }

        /// <summary>
        /// </summary>
        protected INodesForIndexingProvider GetNodeProvider()
        {
            Taxonomy tax = ParseTaxonomy(GetTaxonomyPath());
            tax.CurrentLabelRole = "preferredLabel";
            return new DeepTaxonomyNodeFinder(TaxonomyView.Presentation(tax));
        }

        private LuceneIndexMgr InitializeIndex(INodesForIndexingProvider deepNodeFinder)
        {
            LuceneIndexMgr result = new LuceneIndexMgr();
            indexMrgsNeedingCleanupAtTearDown.Add(result);

            bool isInitializationCompletedHandlerCalled = false;
            result.IndexingCompleted += delegate { isInitializationCompletedHandlerCalled = true; };

            Assert.IsFalse(result.IsInitialized, "TEST SANITY: IsInitialized should be false");

            result.Initialize("en", deepNodeFinder);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (result.IsIndexing)
            {
                if (timer.ElapsedMilliseconds > 120000)
                    throw new Exception("It's taken longer than 120 seconds to initialize");
                Thread.Sleep(500);
            }

            Thread.Sleep(1000);

            Assert.IsTrue(isInitializationCompletedHandlerCalled,
                          "isInitializationCompletedHandlerCalled was false but should be true");
            Assert.IsTrue(result.IsInitialized, "IsInitialized was false but should be true");

            return result;
        }
    }
}
#endif
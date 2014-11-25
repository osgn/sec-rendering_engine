using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene
{
    ///<summary>
    ///</summary>
    public class LuceneIndexMgr : AbstractIndexMgr
    {

        private readonly Dictionary<string, Dictionary<string, Directory>> luceneDirectoryIndexedByLanguageCodeThenView = new Dictionary<string, Dictionary<string, Directory>>();
        private IndexWriter indexWriter;

        private Directory CurrentDirectory
        {
            get
            {
                if (luceneDirectoryIndexedByLanguageCodeThenView.ContainsKey(CurrentLanguage))
                    if (luceneDirectoryIndexedByLanguageCodeThenView[CurrentLanguage].ContainsKey(CurrentView))
                        return luceneDirectoryIndexedByLanguageCodeThenView[CurrentLanguage][CurrentView];

                return null;
            }
        }

        /// <summary>
        /// </summary>
        protected override INodeIndexer GetNodeIndexer()
        {
            indexWriter = new IndexWriter(CurrentDirectory, LuceneLibrarySearchCriteriaAdaptor.ANALYZER , true);
            return new LuceneNodeIndexer(indexWriter, CurrentLanguage);
        }

        /// <summary>
        /// </summary>
        protected override void DoCleanupAfterFullIndexing()
        {
            if (indexWriter == null) return;

			Debug.WriteLine("DoCleanupAfterFullIndexing: Optimizing and Closing index writer");

            indexWriter.Optimize();
            indexWriter.Close();
        }

        /// <summary>
        /// </summary>
        protected override ILibrarySearchResult DoSearch(LibrarySearchCriteria criteria)
        {
            LuceneLibrarySearchCriteriaAdaptor adaptor = new LuceneLibrarySearchCriteriaAdaptor();
            
            Query query = adaptor.BuildQuery(criteria);

            if( query == null )
            {
				Debug.WriteLine("LuceneIndexMgr: Query is null, returning a zero-hits result");
                return new NoHitsLibrarySearchResult();
            }

            IndexSearcher searcher = new IndexSearcher(CurrentDirectory);

            Hits hits = searcher.Search(query);

            string tmpPathFilter = (criteria.AncestryPath == null) ? null : criteria.AncestryPath.Replace(LuceneNodeIndexer.PATH_EOL_MARKER, string.Empty);
            return new LuceneLibrarySearchResult(hits, tmpPathFilter,0.0499);
        }

        private int[] GetDocumentIdsForElementId(string elementId)
        {
            List<int> result = new List<int>();

            IndexSearcher searcher = new IndexSearcher(CurrentDirectory);

            Hits hits = searcher.Search(new TermQuery(new Term(LuceneNodeIndexer.ELEMENTID_FOR_DELETING_FIELD,elementId)));

            for(int x=0; x < hits.Length(); x++)
            {
                result.Add(hits.Id(x));
            }

            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        public int GetMaxDocumentId()
        {
            IndexReader reader = IndexReader.Open(CurrentDirectory);
            return reader.MaxDoc();
        }

        ///<summary>
        ///</summary>
        public Document GetDocument(int id)
        {
            IndexReader reader = IndexReader.Open(CurrentDirectory);
            return reader.Document(id);
        }

        /// <summary>
        /// </summary>
        protected override void DoIndexNode(Node node, bool reindexAfterRemovingOld)
        {
            lock(this)
            {
                foreach (string langKey in luceneDirectoryIndexedByLanguageCodeThenView.Keys)
                {
                    foreach (string viewKey in luceneDirectoryIndexedByLanguageCodeThenView[langKey].Keys)
                    {
                        Directory d = luceneDirectoryIndexedByLanguageCodeThenView[langKey][viewKey];

                        IndexModifier modifier = new IndexModifier(d, LuceneLibrarySearchCriteriaAdaptor.ANALYZER, false);
                        try
                        {
                            foreach (int i in GetDocumentIdsForElementId(node.Id))
                            {
                                Document doc = GetDocument(i);
                                if (MakePath(node) == doc.GetField(LuceneNodeIndexer.PATH_FIELD).StringValue() && node.Order.ToString() == doc.GetField(LuceneNodeIndexer.ORDER_FIELD).StringValue())
                                {
                                    Debug.WriteLine(
                                        string.Format("DoIndexNode: Removing document {0} for element id {1}", i,
                                                      node.Id));
                                    modifier.DeleteDocument(i);
                                }
                            }
                        }
                        finally
                        {
							Debug.WriteLine("DoIndexNode: Closing index reader");
                            modifier.Close();
                        }

                        if (!reindexAfterRemovingOld) return;

                        indexWriter = new IndexWriter(d, LuceneLibrarySearchCriteriaAdaptor.ANALYZER, false);
                        LuceneNodeIndexer indexer = new LuceneNodeIndexer(indexWriter, langKey);
                        indexer.AddToIndex(node, node.PreferredLabel);
                    }
                }
            }
        }

        ///<summary>
        ///</summary>
        public override string MakePath(Node node)
        {
            try
            {
                return LuceneNodeIndexer.MakePathForNode(node);
            }
            catch (Exception e)
            {
				Debug.Write(new Exception("LuceneIndexMgr: An error occurred in MakePath", e));
            }

            return string.Empty;
        }

        ///<summary>
        ///</summary>
        public override string MakePath(Node node, string pathSeparator, bool includeEndToken)
        {
            try
            {
                return LuceneNodeIndexer.MakePathForNode(node, pathSeparator, includeEndToken);
            }
            catch (Exception e)
            {
				Debug.Write(new Exception("LuceneIndexMgr: An error occurred in MakePath", e));
            }

            return string.Empty;
        }

        ///<summary>
        ///</summary>
        public override string[] MakePathAsArray(string path)
        {
            try
            {
                return LuceneNodeIndexer.MakePathAsArray(path);
            }
            catch (Exception e)
            {
				Debug.Write(new Exception("LuceneIndexMgr: An error occurred in MakePathAsArray", e));
            }

            return new string[]{};
        }

        ///<summary>
        ///</summary>
        public override string SingleCharacterWildCardSubstitute
        {
            get { return "?"; }
        }

        /// <summary>
        /// </summary>
        protected override bool IsNeedsReIndexing()
        {
            return CurrentDirectory == null;
        }

#if UNITTEST
        ///<summary>
        ///</summary>
        public Directory LuceneDirectory_ForTesting
        {
            get
            {
                return CurrentDirectory;
            }
        }
#endif

        private static Directory GetInitialDirectory()
        {
            return new RAMDirectory();
        }

        /// <summary>
        /// </summary>
        protected override void DoInitialize(bool isFullRestart)
        {
            lock (this)
            {
                if( isFullRestart )
                {
                    CloseAllDirectories();
                }
                else
                {
                    if (CurrentDirectory != null)
                    {
                        CurrentDirectory.Close();
                        luceneDirectoryIndexedByLanguageCodeThenView[CurrentLanguage].Remove(CurrentView);
                    }
                }

                if( !luceneDirectoryIndexedByLanguageCodeThenView.ContainsKey(CurrentLanguage) )
                {
                    luceneDirectoryIndexedByLanguageCodeThenView.Add(CurrentLanguage,new Dictionary<string, Directory>());
                }

                luceneDirectoryIndexedByLanguageCodeThenView[CurrentLanguage].Add(CurrentView, GetInitialDirectory());
            }
        }

        private void CloseAllDirectories()
        {
            foreach(string langKey in luceneDirectoryIndexedByLanguageCodeThenView.Keys)
            {
                foreach (string viewKey in luceneDirectoryIndexedByLanguageCodeThenView[langKey].Keys)
                {
                    Directory dir = luceneDirectoryIndexedByLanguageCodeThenView[langKey][viewKey];
                    if( dir != null )
                    {
                        dir.Close();
						Debug.WriteLine(string.Format("LuceneIndexMgr: Closing the Lucene directory for language {0} and view {1}", langKey, viewKey));
                    }
                }
                luceneDirectoryIndexedByLanguageCodeThenView[langKey].Clear();
            }
            luceneDirectoryIndexedByLanguageCodeThenView.Clear();
        }

        /// <summary>
        /// </summary>
        protected override void DoClose()
        {
            CloseAllDirectories();
        }
    }
}

using System;
using Aucent.MAX.AXE.XBRLParser.Searching.Indexing;
using ILibraryIndexMgr=Aucent.MAX.AXE.XBRLParser.Searching.Indexing.ILibraryIndexMgr;

namespace Aucent.MAX.AXE.XBRLParser.Searching
{
    /// <summary>
    /// </summary>
    public class LibrarySearchMgr : ILibrarySearchMgr, IDisposable
    {
        private readonly ILibraryIndexMgr _indexMgr;

        private const string PATH_ENUM_VALUE_MARKER = "ENUMVAL:";

        /// <summary>
        /// </summary>
        public LibrarySearchMgr(ILibraryIndexMgr indexMgr)
        {
            _indexMgr = indexMgr;

            _indexMgr.IndexProgressUpdated += _indexMgr_IndexProgressUpdated;
            _indexMgr.IndexingStarted += _indexMgr_IndexingStarted;
            _indexMgr.IndexingCompleted += _indexMgr_IndexingCompleted;
            _indexMgr.ErrorWhileIndexing += _indexMgr_ErrorWhileIndexing;
        }

        void _indexMgr_IndexingStarted(object sender, EventArgs e)
        {
            OnIndexingStarted();
        }

        void _indexMgr_IndexingCompleted(object sender, EventArgs<bool> e)
        {
            OnIndexingCompleted(e.Value);
        }

        void _indexMgr_IndexProgressUpdated(object sender, EventArgs<decimal> e)
        {
            OnIndexProgressUpdated(e.Value);
        }

        void _indexMgr_ErrorWhileIndexing(object sender, EventArgs<Exception> e)
        {
            OnErrorWhileIndexing(e.Value);
        }

        /// <summary>
        /// </summary>
        public ILibrarySearchResult Search(LibrarySearchCriteria criteria)
        {
            return _indexMgr.Search(criteria);
        }

        /// <summary>
        /// </summary>
        public void Initialize(string currentLanguage, INodesForIndexingProvider nodesForIndexingProvider)
        {
            _indexMgr.Initialize(currentLanguage, nodesForIndexingProvider);
        }

        /// <summary>
        /// </summary>
        public void ChangeToLanguage(string langCode)
        {
            _indexMgr.ChangeToLanguage(langCode);
        }

        /// <summary>
        /// </summary>
        public void ChangeToView(INodesForIndexingProvider nodesForIndexingProvider)
        {
            _indexMgr.ChangeToView(nodesForIndexingProvider);
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            _indexMgr.Close();
        }

        /// <summary>
        /// </summary>
        public void ReIndexNode(Node node)
        {
            _indexMgr.ReIndexNode(node);
        }

        /// <summary>
        /// </summary>
        public string MakePath(Node node)
        {
            return _indexMgr.MakePath(node);
        }

        /// <summary>
        /// </summary>
        public string MakePath(Node node, string pathSeparator, bool includeEndToken)
        {
            return _indexMgr.MakePath(node, pathSeparator, includeEndToken);
        }

        /// <summary>
        /// </summary>
        public string[] MakePathAsArray(string path)
        {
            return _indexMgr.MakePathAsArray(path);
        }

        /// <summary>
        /// </summary>
        public string SingleCharacterWildCardSubstitute
        {
            get { return _indexMgr.SingleCharacterWildCardSubstitute; }
        }

        /// <summary>
        /// </summary>
        public bool IsIndexing
        {
            get { return _indexMgr.IsIndexing; }
        }

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs<decimal>> IndexProgressUpdated;

        /// <summary>
        /// </summary>
        public event EventHandler IndexingStarted;

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs<bool>> IndexingCompleted;

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ErrorWhileIndexing;

        public bool IsEnumValueNode(string elementId)
        {
            return elementId != null && elementId.StartsWith(PATH_ENUM_VALUE_MARKER);
        }

        public static Node StaticCreateNodeForEnumValue(Node enumNode, string enumValue)
        {
            Node en = new Node(enumValue);
            en.Id = PATH_ENUM_VALUE_MARKER + enumValue;
            en.Parent = enumNode;
            en.Name = enumValue;
            en.IsAbstract = enumNode.IsAbstract;

            return en;
        }

        public Node CreateNodeForEnumValue(Node enumNode, string enumValue)
        {
            return StaticCreateNodeForEnumValue(enumNode, enumValue);
        }

        public string GetEnumValueElementId(string id)
        {
            return (id == null) ? null : id.Replace(PATH_ENUM_VALUE_MARKER, string.Empty);
        }

        private void OnIndexProgressUpdated(decimal percentCompleted)
        {
            if( IndexProgressUpdated != null )
            {
                IndexProgressUpdated(this, new EventArgs<decimal>(percentCompleted));
            }
        }

        /// <summary>
        /// </summary>
        protected void OnErrorWhileIndexing(Exception e)
        {
            if (ErrorWhileIndexing != null)
            {
                ErrorWhileIndexing(this, new EventArgs<Exception>(e));
            }
        }

        private void OnIndexingCompleted(bool wasErrored)
        {
            if (IndexingCompleted != null)
            {
                IndexingCompleted(this, new EventArgs<bool>(wasErrored));
            }
        }

        private void OnIndexingStarted()
        {
            if (IndexingStarted != null)
            {
                IndexingStarted(this, EventArgs.Empty);
            }
        }

        #region IDisposable
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// </summary>
        protected bool is_disposed;

        /// <summary>
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!is_disposed)
            {
                lock (this)
                {
                    if (!is_disposed)
                    {
                        if (disposing)
                        {
                            Close();
                        }
                        _indexMgr.IndexProgressUpdated -= _indexMgr_IndexProgressUpdated;
                        _indexMgr.IndexingStarted -= _indexMgr_IndexingStarted;
                        _indexMgr.IndexingCompleted -= _indexMgr_IndexingCompleted;
                        _indexMgr.ErrorWhileIndexing -= _indexMgr_ErrorWhileIndexing;
                    }                    
                }
            }
            is_disposed = true;
        }
        #endregion IDisposable

    }
}

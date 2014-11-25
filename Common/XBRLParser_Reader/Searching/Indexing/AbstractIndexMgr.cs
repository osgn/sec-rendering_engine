using System;
using System.Diagnostics;
using System.Threading;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    ///<summary>
    ///</summary>
    public abstract class AbstractIndexMgr : ILibraryIndexMgr
    {
        private bool _isInitialized;
        private bool _isIndexing;
        
        private bool isStopIndexingRequested;

        private bool isErrorOccurredWhileIndexing;

        private QueuedIndexRunner queuedIndexRunner;

        Thread indexingThread;
        Thread monitorThread;

        private INodesForIndexingProvider nodesForIndexingProvider;
        /// <summary>
        /// </summary>
        protected INodesForIndexingProvider NodesForIndexingProvider
        {
            get { return nodesForIndexingProvider; }
            private set { nodesForIndexingProvider = value; }
        }

        private string currentLanguage;
        /// <summary>
        /// </summary>
        protected string CurrentLanguage
        {
            get { return currentLanguage; }
            private set { currentLanguage = value; }
        }

        private string currentView;
        /// <summary>
        /// </summary>
        protected string CurrentView
        {
            get { return currentView; }
            private set { currentView = value; }
        }

        ///<summary>
        ///</summary>
        public ILibrarySearchResult Search(LibrarySearchCriteria criteria)
        {
            if( !IsInitialized )
                throw new InvalidOperationException("Cannot Search: The Search Index Mgr has not been initialized.  Please call Initialize() first.");

            return DoSearch(criteria);
        }

        ///<summary>
        ///</summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        private string GetViewAndLanguageKey()
        {
            return GetViewAndLanguageKey(CurrentView, CurrentLanguage);
        }

        private static string GetViewAndLanguageKey(string view, string language)
        {
            return string.Format("{0}::{1}", view, language);
        }

        private void RunFullIndexingInSeparateThread(ThreadSafeQueue<Node> queuedNodes)
        {
            isErrorOccurredWhileIndexing = false;
            _isIndexing = true;

            StopAllRunningThreads();

            indexingThread = new Thread(RunFullIndexing);
            indexingThread.IsBackground = true;
            indexingThread.Name = "indexingThread";
            indexingThread.Start(queuedNodes);

            string viewAndLanguageKey = GetViewAndLanguageKey(CurrentView, CurrentLanguage);

            monitorThread = new Thread(WatchQueueCompletionProgress);
            monitorThread.IsBackground = true;
            monitorThread.Name = "progressWatchingThread";
            monitorThread.Start(new object[] { queuedNodes, viewAndLanguageKey });
        }

        /// <summary>
        /// </summary>
        protected abstract void DoInitialize(bool isFullRestart);

        /// <summary>
        /// </summary>
        protected abstract void DoClose();

        /// <summary>
        /// </summary>
        protected abstract INodeIndexer GetNodeIndexer();

        /// <summary>
        /// </summary>
        protected abstract void DoCleanupAfterFullIndexing();

        /// <summary>
        /// </summary>
        protected abstract ILibrarySearchResult DoSearch(LibrarySearchCriteria criteria);

        /// <summary>
        /// </summary>
        protected abstract void DoIndexNode(Node node, bool reindexAfterRemovingOld);

        /// <summary>
        /// </summary>
        public abstract string MakePath(Node node);

        /// <summary>
        /// </summary>
        public abstract string MakePath(Node node, string pathSeparator, bool includeEndToken);

        /// <summary>
        /// </summary>
        public abstract string[] MakePathAsArray(string path);

        /// <summary>
        /// </summary>
        public abstract string SingleCharacterWildCardSubstitute { get; }

        /// <summary>
        /// </summary>
        protected abstract bool IsNeedsReIndexing();

        ///<summary>
        ///</summary>
        public void Initialize(string lang, INodesForIndexingProvider nfip)
        {
            Initialize(lang,nfip,true);
        }

        /// <summary>
        /// </summary>
        public void ChangeToView(INodesForIndexingProvider newNodesForIndexingProvider)
        {
            lock (this)
            {
                try
                {
                    NodesForIndexingProvider = newNodesForIndexingProvider;
                    CurrentView = NodesForIndexingProvider.GetHashKeyForIndex();

                    if (IsNeedsReIndexing())
                    {
                        _isInitialized = false;
                        Initialize(CurrentLanguage, NodesForIndexingProvider, false);
                    }
                }
                catch (Exception e)
                {
                    Debug.Write(new Exception("AbstractIndexMgr: An error occurred in ChangeToView", e));
                }
            }
        }

        private void Initialize(string lang, INodesForIndexingProvider nfip, bool isFullRestart)
        {
            if (!_isInitialized)
            {
                lock (this)
                {
                    if (!_isInitialized)
                    {
                        isStopIndexingRequested = false;
                        
                        _isInitialized = false;

                        OnIndexingStarted();

                        NodesForIndexingProvider = nfip;
                        CurrentLanguage = lang;
                        CurrentView = nfip.GetHashKeyForIndex();

                        if (isFullRestart || IsNeedsReIndexing())
                        {
                            DoInitialize(isFullRestart);

                            ThreadSafeQueue<Node> queuedNodes = new ThreadSafeQueue<Node>(NodesForIndexingProvider.GetNodesForIndexing());

                            RunFullIndexingInSeparateThread(queuedNodes);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        protected void RunFullIndexing(object threadStartParam)
        {
            RunFullIndexing((ThreadSafeQueue<Node>)threadStartParam);
        }

        /// <summary>
        /// </summary>
        protected void RunFullIndexing(ThreadSafeQueue<Node> queuedNodes)
        {
            try
            {
				Debug.WriteLine("AbstractIndexMgr: Starting full index on " + queuedNodes.Count + " nodes...");

                _isIndexing = true;
                ManualResetEvent mre = new ManualResetEvent(false);

                queuedIndexRunner = new QueuedIndexRunner(queuedNodes, mre, GetNodeIndexer(), NodesForIndexingProvider.GetLabelRole());
                queuedIndexRunner.QueueDepleted += queuedIndexRunner_QueueDepleted;
                queuedIndexRunner.ErrorWhileIndexing += queuedIndexRunner_ErrorWhileIndexing;

                queuedIndexRunner.Run(true);
            }
            catch (Exception e)
            {
				Trace.Write(new Exception("AbstractIndexMgr: An error occurred in RunFullIndexing", e));
            }
        }

        private void WatchQueueCompletionProgress(object threadStartData)
        {
            try
            {
                object[] objArray = (object[]) threadStartData;
                ThreadSafeQueue<Node> queuedNodes = (ThreadSafeQueue<Node>)objArray[0];
                string viewAndLanguageKey = (string) objArray[1];

                int initialCount = queuedNodes.Count;
                int currentPercentageDone = 0;

                while(!isStopIndexingRequested && initialCount > 0 && currentPercentageDone < 100)
                {
                    int newPercentageDone = Convert.ToInt32( Convert.ToDecimal(initialCount - queuedNodes.Count) / Convert.ToDecimal(initialCount) * 100);

                    if (newPercentageDone != currentPercentageDone)
                    {
                        currentPercentageDone = newPercentageDone;
                        OnIndexProgressUpdate( viewAndLanguageKey, Convert.ToDecimal(currentPercentageDone) / Convert.ToDecimal(100) );
                    }
                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException)
            {
				Debug.WriteLine("AbstractIndexMgr: Caught a ThreadAbortException");
            }
            catch (Exception e)
            {
				Debug.Write(new Exception("AbstractIndexMgr: An error occurred in WatchQueueCompletionProgress", e));
            }
			Debug.WriteLine("AbstractIndexMgr: Finished watching queue completion progress");
        }

        void queuedIndexRunner_QueueDepleted(object sender, EventArgs<bool> e)
        {
            isStopIndexingRequested = true;
            OnIndexingCompleted(isErrorOccurredWhileIndexing || e.Value);
        }

        void queuedIndexRunner_ErrorWhileIndexing(object sender, EventArgs<Exception> e)
        {
            isErrorOccurredWhileIndexing = true;
            OnErrorWhileIndexing(e.Value);
        }

        /// <summary>
        /// </summary>
        public void StopIndexing()
        {
            try
            {
                isStopIndexingRequested = true;

                if (queuedIndexRunner == null)
                {
					Debug.WriteLine("AbstractIndexMgr: StopIndexing:  not indexing");
                    return;
                }

				Debug.WriteLine("AbstractIndexMgr: StopIndexing:  Stopping...");

                queuedIndexRunner.Stop();
                queuedIndexRunner.QueueDepleted -= queuedIndexRunner_QueueDepleted;
                queuedIndexRunner.ErrorWhileIndexing -= queuedIndexRunner_ErrorWhileIndexing;
            }
            catch (Exception e)
            {
                Trace.Write(new Exception("AbstractIndexMgr: An error occurred in StopIndexing", e));
            }
        }

        private void StopAllRunningThreads()
        {
            if (indexingThread != null && indexingThread.IsAlive)
            {
                if (queuedIndexRunner != null)
                {
                    queuedIndexRunner.Stop();
                }
				Debug.WriteLine(string.Format("Aborting live indexing thread named {0}", indexingThread.Name));
                indexingThread.Abort();
                indexingThread = null;
            }

            if (monitorThread != null && monitorThread.IsAlive)
            {
				Debug.WriteLine(string.Format("Aborting live monitoring thread named {0}", monitorThread.Name));
                monitorThread.Abort();
                monitorThread = null;
            }
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            lock (this)
            {
                try
                {
					Debug.WriteLine("AbstractIndexMgr: Closing()...");
                    StopIndexing();
                    DoClose();

                    StopAllRunningThreads();

                    _isInitialized = false;
					Debug.WriteLine("AbstractIndexMgr: Closed.");
                }
                catch (Exception e)
                {
                    Trace.Write(new Exception("AbstractIndexMgr: An error occurred in Close", e));
                }
            }
        }

        /// <summary>
        /// </summary>
        public void ChangeToLanguage(string langCode)
        {
            lock (this)
            {
                try
                {
                    CurrentLanguage = langCode;
                    if (IsNeedsReIndexing())
                    {
                        _isInitialized = false;
                        Initialize(langCode, NodesForIndexingProvider, false);
                    }
                }
                catch (Exception e)
                {
                    Trace.Write(new Exception("AbstractIndexMgr: An error occurred in ChangeToLanguage", e));
                }
            }
        }

        /// <summary>
        /// </summary>
        public void ReIndexNode(Node node)
        {
            try
            {
                OnIndexingStarted();
                DoIndexNode(node, nodesForIndexingProvider != null && nodesForIndexingProvider.IsNodeSuitableForIndexing(node));
                OnIndexingCompleted(false);
            }
            catch (Exception e)
            {
                Trace.Write(new Exception("AbstractIndexMgr: An error occurred in ReIndexNode", e));
            }
        }

        /// <summary>
        /// </summary>
        public bool IsIndexing
        {
            get { return _isIndexing; }
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

        /// <summary>
        /// </summary>
        protected void OnIndexProgressUpdate(string viewAndLanguageKey, decimal percentComplete)
        {
            if (viewAndLanguageKey == GetViewAndLanguageKey())
            {
                if (IndexProgressUpdated != null)
                {
                    IndexProgressUpdated(this, new EventArgs<decimal>(percentComplete));
                }
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

        /// <summary>
        /// </summary>
        protected void OnIndexingStarted()
        {
            _isIndexing = true;

            if (IndexingStarted != null)
            {
                IndexingStarted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// </summary>
        protected void OnIndexingCompleted(bool errorsOccurred)
        {
            try
            {
                DoCleanupAfterFullIndexing();

				Debug.WriteLine("Setting _isInitialized=true and _isIndexing=false");
                _isInitialized = true;
                _isIndexing = false;


                if (IndexingCompleted != null)
                {
                    IndexingCompleted(this, new EventArgs<bool>(errorsOccurred));
                }
            }
            catch (Exception e)
            {
                Trace.Write(new Exception("AbstractIndexMgr: An error occurred in OnIndexingCompleted", e));
            }
        }
    }
}

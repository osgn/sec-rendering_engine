using System;
using System.Diagnostics;
using System.Threading;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    /// <summary>
    /// </summary>
    public class QueuedIndexRunner
    {
        private readonly ThreadSafeQueue<Node> _queueOfNodesToIndex;
        private readonly ManualResetEvent _threadResetMonitor;
        private readonly INodeIndexer _nodeIndexer;
        private readonly string _labelRole;

        private bool isStopRequested;
        private bool isRunning;

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs<bool>> QueueDepleted;
        
        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ErrorWhileIndexing;

        /// <summary>
        /// </summary>
        public QueuedIndexRunner(ThreadSafeQueue<Node> _queueOfNodesToIndex, ManualResetEvent _threadResetMonitor, INodeIndexer _nodeIndexer, string _labelRole)
        {
            this._queueOfNodesToIndex = _queueOfNodesToIndex;
            this._labelRole = _labelRole;
            this._nodeIndexer = _nodeIndexer;
            this._threadResetMonitor = _threadResetMonitor;
        }

        private void OnQueueDepleted(bool wasErrored)
        {
            if (QueueDepleted != null)
            {
                QueueDepleted(this, new EventArgs<bool>(wasErrored));
            }
        }

        private void OnErrorWhileIndexing(Exception e)
        {
            if( ErrorWhileIndexing != null )
            {
                ErrorWhileIndexing(this, new EventArgs<Exception>(e));
            }
        }

        /// <summary>
        /// </summary>
        public void Run(bool stopWhenDepleted)
        {
            try
            {
                isRunning = true;
                isStopRequested = false;

                while( !isStopRequested )
                {
                    Node nodeToIndex;
                    Exception errorThrown = null;
                    while(!isStopRequested && _queueOfNodesToIndex.TryDequeue(out nodeToIndex) )
                    {
                        try
                        {
                            _nodeIndexer.AddToIndex(nodeToIndex, _labelRole);
                        }
                        catch (Exception e)
                        {
                            errorThrown = new Exception("QueuedIndexRunner: An error occurred in _nodeIndexer.AddToIndex on node: " + nodeToIndex, e);
							Debug.WriteLine(errorThrown);
                        }
                    }

                    OnQueueDepleted(errorThrown != null);

                    if (errorThrown != null)
                        OnErrorWhileIndexing(errorThrown);

                    if (stopWhenDepleted)
                        isStopRequested = true;

                    if (!isStopRequested)
                    {
						Debug.WriteLine("QueuedIndexRunner: Waiting for ManualResetEvent");
                        _threadResetMonitor.WaitOne();
                    }
                }
				Debug.WriteLine("QueuedIndexRunner: Finishing Run");
            }
            catch (Exception e)
            {
                Trace.Write(new Exception("QueuedIndexRunner: An error occurred in Run", e));
                OnQueueDepleted(true);
            }
            finally
            {
                isRunning = false;
            }
        }

        /// <summary>
        /// </summary>
        public void EnqueueNode(Node node)
        {
            _queueOfNodesToIndex.Enqueue(node);
            _threadResetMonitor.Set();
        }

        /// <summary>
        /// </summary>
        public void Stop()
        {
			Debug.WriteLine("QueuedIndexRunner: Stop requested...");
            isStopRequested = true;
            _threadResetMonitor.Close();

            Stopwatch timer = new Stopwatch();
            timer.Start();
            while(isRunning && timer.ElapsedMilliseconds < 15000)
            {
                Thread.Sleep(100);
            }
#if DEBUG
            if( isRunning )
				Debug.WriteLine("QueuedIndexRunner: Unable to stop gracefully stop");

			Debug.WriteLine("QueuedIndexRunner: Stopped.");
#endif
        }
    }
}

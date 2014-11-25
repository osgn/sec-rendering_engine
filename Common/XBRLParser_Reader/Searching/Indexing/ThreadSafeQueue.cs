using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    /// <summary>
    /// </summary>
    public class ThreadSafeQueue<T>
    {
        private readonly Queue<T> _innerQueue;

        /// <summary>
        /// </summary>
        public ThreadSafeQueue()
        {
            _innerQueue = new Queue<T>();
        }

        /// <summary>
        /// </summary>
        public ThreadSafeQueue(IEnumerable<T> collectionToPrepopulate)
        {
            _innerQueue = new Queue<T>(collectionToPrepopulate);
        }

        /// <summary>
        /// </summary>
        public int Count
        {
            get
            {
                lock (_innerQueue)
                {
                    return _innerQueue.Count;
                }
            }
        }

        /// <summary>
        /// </summary>
        public void Enqueue(T o)
        {
            lock (_innerQueue)
            {
                _innerQueue.Enqueue(o);
            }
        }

        /// <summary>
        /// </summary>
        public T Dequeue()
        {
            lock (_innerQueue)
            {
                return _innerQueue.Dequeue();
            }
        }

        /// <summary>
        /// </summary>
        public bool TryDequeue(out T o)
        {
            lock (_innerQueue)
            {
                if (_innerQueue.Count > 0)
                {
                    o = _innerQueue.Dequeue();
                    return true;
                }

                o = default(T);
                return false;
            }
        }
    }
}

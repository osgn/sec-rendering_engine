/**************************************************************************
 * WorkerThreadPool (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class is a modified version of the ManagedThreadPool classed created by 
 * Stephen Toub at Microsoft (stoub@microsoft.com).
 * The class was updated to use the Semaphore class defined in .NET 2.0 and later 
 * rather than using its own Semaphore class.  In addition the class can be
 * instantiated and managed as a member or the calling process rather than being static.
 * This allows for more flexability in managing the number of threads created for a 
 * given process.  Sense the class can now be instantiated it also Implements IDisposable
 * so that all of the worker threads spawned for a given Pool will be stopped when the 
 * class itself is disposed of.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aucent.FilingServices.Data
{
	public class WorkerThreadPool: IDisposable
	{
		#region Member Variables

		private bool stopThreads = false;

		private int maxWorkerThreads = -1;
		public int MaxWorkerThreads
		{
			get { return this.maxWorkerThreads; }
		}

		/// <summary>Queue of all the callbacks waiting to be executed.</summary>
		private Queue<WorkerCallback> workerQueue;

		/// <summary>Gets the number of callback delegates currently waiting in the thread pool.</summary>
		public int QueuedCallbacks 
		{ 
			get 
			{
				lock (workerQueue) 
				{
					return workerQueue.Count; 
				}
			}
		}

		/// <summary>
		/// Used to signal that a worker thread is needed for processing.  Note that multiple
		/// threads may be needed simultaneously and as such we use a semaphore instead of
		/// an auto reset event.
		/// </summary>
		private Semaphore workerThreadsSemaphore;

		/// <summary>List of all worker threads at the disposal of the thread pool.</summary>
		private List<Thread> workerThreads;

		/// <summary>Number of threads currently active.</summary>
		private SemaphoreCount waitCount;
		public int WaitCount
		{
			get
			{
				lock (this.waitCount)
				{
					return waitCount.Value;
				}
			}
		}

		#endregion

		#region Construction
		/// <summary>Initialize the thread pool.</summary>
		public WorkerThreadPool(int maxThreadCount)
		{
			// Create our thread stores; we handle synchronization ourself
			// as we may run into situtations where multiple operations need to be atomic.
			// We keep track of the threads we've created just for good measure; not actually
			// needed for any core functionality.
			this.maxWorkerThreads = maxThreadCount;
			workerQueue = new Queue<WorkerCallback>();
			workerThreads = new List<Thread>();
			waitCount = new SemaphoreCount(0);

			// Create our "thread needed" event
			workerThreadsSemaphore = new Semaphore(0, this.maxWorkerThreads);
			
			// Create all of the worker threads
			for (int i = 0; i < this.maxWorkerThreads; i++)
			{
				// Create a new thread and add it to the list of threads.
				Thread newThread = new Thread(new ThreadStart(ProcessQueuedItems));
				workerThreads.Add(newThread);

				// Configure the new thread and start it
				newThread.Name = "WorkerThreadPool #" + i.ToString();
				newThread.IsBackground = true;
				newThread.Start();
			}
		}

		~WorkerThreadPool()
		{
			Dispose();
		}

		public void Dispose()
		{
			lock (workerQueue)
			{
				//Empty the queue so that no more work will be performed,
				EmptyQueue();

				//Set the flag to tell the threads they need to shutdown
				stopThreads = true;

				lock (waitCount)
				{
					//Release will throw an Exception if the value you pass it is less than or equal
					//to zeros.  So only call Release when there are threads in a waiting state.
					if (waitCount.Value > 0)
					{
						//Allow all threads that are currently waiting to stop.  Threads that are still
						//working will stop once they complete the task they are currently working on.
						workerThreadsSemaphore.Release(waitCount.Value);
						waitCount.Value = 0;
					}
				}
			}
		}

		#endregion

		#region Public Methods
		/// <summary>Queues a user work item to the thread pool.</summary>
		/// <param name="callback">
		/// A WaitCallback representing the delegate to invoke when the thread in the 
		/// thread pool picks up the work item.
		/// </param>
		public void QueueUserWorkItem(WaitCallback callback)
		{
			// Queue the delegate with no state
			QueueUserWorkItem(callback, null);
		}

		/// <summary>Queues a user work item to the thread pool.</summary>
		/// <param name="callback">
		/// A WaitCallback representing the delegate to invoke when the thread in the 
		/// thread pool picks up the work item.
		/// </param>
		/// <param name="state">
		/// The object that is passed to the delegate when serviced from the thread pool.
		/// </param>
		public void QueueUserWorkItem(WaitCallback callback, object state)
		{
			// Create a waiting callback that contains the delegate and its state.
			// Add it to the processing queue, and signal that data is waiting.
			WorkerCallback waiting = new WorkerCallback(callback, state);
			lock (workerQueue) 
			{ 
				workerQueue.Enqueue(waiting); 
			}

			//Interlocked does not let us read the value of an int, so instead lets
			//add 0 to the value to get the actual value of waitingThreads
			lock (waitCount)
			{
				if (waitCount.Value > 0)
				{
					//Only call release if we still have waiting threads, otherwise an exception will be
					//thrown by the semaphore.
					waitCount.Value--;
					workerThreadsSemaphore.Release(1);
				}
			}
		}

		/// <summary>Empties the work queue of any queued work items.</summary>
		public void EmptyQueue()
		{
			lock (workerQueue) 
			{ 
				try 
				{
					// Try to dispose of all remaining state
					foreach (object obj in workerQueue)
					{
						WorkerCallback callback = (WorkerCallback)obj;
						if (callback.State is IDisposable) ((IDisposable)callback.State).Dispose();
					}
				} 
				catch
				{
					// Make sure an error isn't thrown.
				}

				// Clear all waiting items and reset the number of worker threads currently needed
				// to be 0 (there is nothing for threads to do)
				workerQueue.Clear();
			}
		}

		#endregion

		#region Thread Processing
		/// <summary>A thread worker function that processes items from the work queue.</summary>
		private void ProcessQueuedItems()
		{
			// Process indefinitely
			while(true)
			{
				// Get the next item in the queue.  If there is nothing there, go to sleep
				// for a while until we're woken up when a callback is waiting.
				WorkerCallback callback = null;
				while (callback == null)
				{
					// Try to get the next callback available.  We need to lock on the 
					// queue in order to make our count check and retrieval atomic.
					lock (workerQueue)
					{
						if (workerQueue.Count > 0)
						{
							try 
							{
								callback = (WorkerCallback)workerQueue.Dequeue(); 
							} 
							catch{} // make sure not to fail here
						}
					}

					// If we can't get one, go to sleep.
					if (callback == null)
					{
						if (stopThreads)
						{
							//Return from the method which will in turn stop the thread
							return;
						}

						lock (waitCount)
						{
							waitCount.Value++;
						}
						//Wait until there is more work to be done;
						workerThreadsSemaphore.WaitOne();
					}
				}

				// We now have a callback.  Execute it.  Make sure to accurately
				// record how many callbacks are currently executing.
				try 
				{
					callback.Callback(callback.State);
				} 
				catch
				{
					// Make sure we don't throw here.  Errors are not our problem.
				}
			}
		}
		#endregion

		/// <summary>Used to hold a callback delegate and the state for that delegate.</summary>
		private class WorkerCallback
		{
			#region Member Variables
			/// <summary>Callback delegate for the callback.</summary>
			private WaitCallback callback;
			public WaitCallback Callback 
			{ 
				get { return callback; } 
			}

			/// <summary>State with which to call the callback delegate.</summary>
			private object state;
			public object State 
			{ 
				get { return state; } 
			}

			#endregion

			#region Construction
			/// <summary>Initialize the callback holding object.</summary>
			/// <param name="callback">Callback delegate for the callback.</param>
			/// <param name="state">State with which to call the callback delegate.</param>
			public WorkerCallback(WaitCallback callback, object state)
			{
				this.callback = callback;
				this.state = state;
			}
			#endregion
		}

		private class SemaphoreCount
		{
			private Int32 myValue = 0;
			public Int32 Value
			{
				get { return myValue; }
				set { myValue = value; }
			}

			public SemaphoreCount(Int32 intValue)
			{
				myValue = intValue;
			}

		}
	}
}

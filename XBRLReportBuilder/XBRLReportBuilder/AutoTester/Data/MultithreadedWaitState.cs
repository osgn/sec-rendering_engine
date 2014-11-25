using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Data
{
	public class MultiThreadedWaitState<T>
	{
		public enum Status
		{
			Ready,
			Processing,
			Canceled,
			Complete
		}

		public ThreadStart CompletedCallback { get; set; }

		public ManualResetEvent ResetEvent { get; set; }

		public T CompletionTarget { get; set; }

		private T completionState;
		public T CompletionState
		{
			get { return this.completionState; }
			set
			{
				if (this.processingStatus != Status.Canceled)
				{
					this.completionState = value;

					if (object.Equals(this.CompletionState, this.CompletionTarget))
					{
						this.FireCompleted();
					}
				}
			}
		}

		private Status processingStatus = Status.Ready;
		public Status ProcessingStatus
		{
			get { return this.processingStatus; }
			private set { this.processingStatus = value; }
		}

		public void Cancel()
		{
			this.ProcessingStatus = Status.Canceled;
			this.FireCompleted();
		}

		private void FireCompleted()
		{
			if (this.CompletedCallback != null)
				this.CompletedCallback();

			if (this.ResetEvent != null)
				this.ResetEvent.Set();
		}
	}
}
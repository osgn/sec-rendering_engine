/**************************************************************************
 * FilingProcessorManager (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class manages the state of a FilingProcessor service and defines the methods
 * for calling a given Processor class that will peform the actual processing on an
 * XBRL data filing.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Data.SqlClient;

using Aucent.FilingServices.Log;
using Aucent.FilingServices.Data;
using Aucent.FilingServices.Data.Interfaces;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;

namespace Aucent.FilingServices.FilingProcessorBase
{
	public class FilingProcessorManager
	{
		#region properties

		//private EventLog myEventLog = null;
		private int completedFilings = -1;
		private WorkerThreadPool myWokerPool = null;

		private IFilingProcessor filingProcessor;

		//Not to be confused with Sytem.Threading.Timer
		private System.Timers.Timer myTimer = null;

		private int myPortNumber = -1;
		private string myEndPoint = string.Empty;

		/// <summary>
		/// The root folder where the SEC (or any other process) will place filings that need to 
		///	 be processed by the service.
		/// </summary>
		private string filingsFolder = string.Empty;
		private DirectoryInfo filingsFolderInfo = null;
		public DirectoryInfo FilingsFolderInfo
		{
			get { return this.filingsFolderInfo; }
		}

		/// <summary>
		/// The root folder where the ReportDispatcher will place filings that need to 
		///	 be processed by the service.
		/// </summary>
		protected string processingFolder = string.Empty;
		protected DirectoryInfo processingFolderInfo = null;
		public DirectoryInfo ProcessingFolderInfo
		{
			get { return this.processingFolderInfo; }
		}

		/// <summary>
		/// The root folder where the service will save the reports that it generates from a filing.
		/// </summary>
		protected string reportsFolder = string.Empty;
		protected DirectoryInfo reportsFolderInfo = null;
		public DirectoryInfo ReportsFolderInfo
		{
			get { return this.reportsFolderInfo; }
		}

		/// <summary>
		/// The maximumn number of threads that can be building reports at a given time
		/// </summary>
		private int maxBuilderThreads = 1;

		/// <summary>
		/// Indicates how long, in seconds, the service will wait between checks of
		/// the FilingsFolder for new filings.
		/// </summary>
		private int processingFrequency = -1;

		private string myDispatcherUri = string.Empty;

		private string myServerKey = string.Empty;
		public string MyServerKey
		{
			get { return myServerKey; }
		}

		//Indicates if the service is in the middle of processing filings
		private bool processing = false;

		//Indicates if the service is attempting to shutdown so that we 
		//will not continue to check for new filings during a shutdown
		private bool shuttingDown = false;

		#endregion

        //private string myDBConnectionString = string.Empty;
        //public string DBConnectionString
		//{
        //    get { return myDBConnectionString; }
        //}

		#region constructor

		protected FilingProcessorManager()
		{
            //myEventLog = new EventLog();
            //if (!System.Diagnostics.EventLog.SourceExists("ReportProcessor"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource(
            //       "ReportProcessor", "ReportBuilder");
            //}
            //myEventLog.Source = "ReportProcessor";
            //myEventLog.Log = "ReportBuilder";

            //if (myEventLog.OverflowAction != OverflowAction.OverwriteAsNeeded)
            //{
            //    myEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
			//}
		}

		#endregion

		#region singleton

		private static FilingProcessorManager theMgr = null;
		public static FilingProcessorManager TheMgr
		{
			get 
			{
				if (theMgr == null)
				{
					theMgr = new FilingProcessorManager();
				}
				return theMgr;
			}
		}

		#endregion

		#region events

		protected void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine( "Checking for new filings..." );

			if( this.shuttingDown )
			{
				System.Diagnostics.Debug.WriteLine( "The server is shutting down; no filings were queued for processing." );
				return;
			}

			if( this.myWokerPool.WaitCount == 0 )
			{
				System.Diagnostics.Debug.WriteLine( "All worker threads are busy; no filings were queued for processing." );
				return;
			}
            
			//Pause the timer while we're working on processing the filings,  this will prevent
			//multiple timer elapses from stepping on each other.
			this.myTimer.Stop();
			this.processing = true;

			try
			{
				string errorMsg;
				if (!CanAccessFolder(filingsFolder, out errorMsg))
				{
					WriteLogEntry(string.Format("Can not process filings in {0}:  The service does not have access to a folder {1}.  Shutting Down the service.", this.myServerKey, filingsFolder), EventLogEntryType.Error);
					ServiceController controller = new ServiceController(this.myServerKey, ".");
					controller.Stop();
					return;
				}
				if (!CanAccessFolder(reportsFolder, out errorMsg))
				{
					WriteLogEntry(string.Format("Can not process filings in {0}:  The service does not have access to a folder {1}.  Shutting Down the service.", this.myServerKey, reportsFolder), EventLogEntryType.Error);
					ServiceController controller = new ServiceController(this.myServerKey, ".");
					controller.Stop();
					return;
				}

				if (!CanAccessFolder(processingFolder, out errorMsg))
				{
					WriteLogEntry(string.Format("Can not process filings in {0}:  The service does not have access to a folder {1}.  Shutting Down the service.", this.myServerKey, processingFolder), EventLogEntryType.Error);
					ServiceController controller = new ServiceController(this.myServerKey, ".");
					controller.Stop();
					return;
				}

				//Sense we pause the timer while processing new filings, we should check for new 
				//filings that got written, while we were processing the last set.  If any exist
				//go ahead and process them immediatly (unless the service is trying to shutdown).
				while( filingProcessor.HasNewFilings() )
				{
					this.ProcessNewFilings();

					if( this.myWokerPool.WaitCount == 0 )
						return;

					if( this.shuttingDown )
						return;
				}
			}
			finally
			{
				this.myTimer.Start();
				this.processing = false;
			}
		}

		#endregion

		#region public methods

		public void Startup(bool bStartTimer)
		{
			WriteLogEntry(string.Format("{0} is starting.", this.myServerKey), EventLogEntryType.Information);

			if (filingProcessor == null)
			{
				WriteLogEntry("Can not start FilingProcessorManager.\r\n\tFilingProcessorManager requires a non-null IFilingProcessor to peform the actualy processing.", EventLogEntryType.Error);
				throw new Exception("Can not start FilingProcessorManager with a null IFilingProcessor");
			}

            //Raj - Reuters processor services may not have filingsFolder,processingFolder & reportsFolder defined in config file.
            //      These values are set by dispatcher through AssignMarketToProcessor method call.
            //      The service should still register as remotable service and register with dispatcher.
            ServiceUtilities.RegisterChannel(this.myPortNumber);
            ServiceUtilities.RegisterService(typeof(FilingProcessorRemotable), this.myEndPoint);

            string errorMsg;
            IFilingDispatcherRemotable dispatcher;
            if (TryGetDispatcherRemotable(out dispatcher, out errorMsg))
            {
                try
                {
                    string myUri = string.Format("tcp://{0}:{1}/{2}", System.Environment.MachineName, myPortNumber, myEndPoint);
                    dispatcher.RegisterProcessor(myUri, this.myServerKey);
                }
                catch (Exception ex)
                {
                    WriteLogEntry("Exception thrown while trying to connect to the Dispatcher: " + ex.Message, EventLogEntryType.Error);
                    throw ex;
                }
            }
            else
            {
                WriteLogEntry("Unable to connect to the Dispatcher: " + errorMsg, EventLogEntryType.Error);
                throw new Exception("Unable to connect to the Dispatcher: " + errorMsg);
            }

            myWokerPool = new WorkerThreadPool(this.maxBuilderThreads);

            if (bStartTimer)
            {
                myTimer = new System.Timers.Timer((double)(processingFrequency * 1000));
                myTimer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer_Elapsed);
                myTimer.Start();
            }
            else
            {
                /*
                 * 
                 * bStartTimer is false for reuters services. We want the timer to start after market has been associated
                 * with this service through the web UI.
                 * On startup we want the service to figure out the market it was associated with.
                 * This is particularly useful when service is restarted after a market had been associated to the service.
                 * 
                 * When the service is started for the first time it will not associated with any market.
                 * 
                 * */
                LoadAssociatedMarketInfo();                     
            }
		}

		public void ShutDown()
		{
			WriteLogEntry(string.Format("{0} is shutting down.", this.myServerKey), EventLogEntryType.Information);

			if (myTimer != null)
			{
				//Stop the time so that we will not try to start processing filings while the service
				//is shutting down.
				myTimer.Stop();
			}
			shuttingDown = true;

			int shutdownAttempt = 0;
			while (processing)
			{
				if (shutdownAttempt == 0)
				{
					WriteLogEntry(string.Format("{0} can't shutdown because it is still processing filings.\r\nThe service will wait until all filings are complete, then shutdown.", this.myServerKey), EventLogEntryType.Information);
					shutdownAttempt++;
				}
				Thread.Sleep(2500);
			}

			string errorMsg;
			IFilingDispatcherRemotable dispatcher;
			if (TryGetDispatcherRemotable(out dispatcher, out errorMsg))
			{
				try
				{
					dispatcher.UnRegisterProcessor(this.myServerKey);
				}
				catch (Exception ex)
				{
					WriteLogEntry("Exception thrown in call to UnRegisterProcessor: " + ex.Message, EventLogEntryType.Warning);
				}
			}
			else
			{
				WriteLogEntry("Failed to UnRegister Processor, " + this.myServerKey + ", from the Dispatcher: " + errorMsg, EventLogEntryType.Warning);
			}
		}

		public bool TryLoadConfigurationSettings(IFilingProcessor processor)
		{
			filingProcessor = processor;

			myServerKey = ConfigurationManager.AppSettings["ServerKey"];
			myEndPoint = ConfigurationManager.AppSettings["EndPoint"];
			myDispatcherUri = ConfigurationManager.AppSettings["DispatcherUri"];

			string errorMsg;
			if (!filingProcessor.TryGetDependentFolderConfiguration(out filingsFolder, out processingFolder, out reportsFolder, out errorMsg))
			{
				WriteLogEntry("Unable to load folder configuration: " + errorMsg, EventLogEntryType.Error);
				return false;
			}

			//processingFolder should never be null, if it is then something went wrong.
			processingFolderInfo = new DirectoryInfo(processingFolder);
			
			if (!string.IsNullOrEmpty(reportsFolder))
			{
				reportsFolderInfo = new DirectoryInfo(reportsFolder);
			}
			if (!string.IsNullOrEmpty(filingsFolder))
			{
				filingsFolderInfo = new DirectoryInfo(filingsFolder);
			}

			string strPortNumber = ConfigurationManager.AppSettings["PortNum"];
			string strMaxThreads = ConfigurationManager.AppSettings["MaxBuilderThreads"] ?? "1";
			string strFrequency = ConfigurationManager.AppSettings["ProcessingFrequency"];

            //try
            //{
            //    myDBConnectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            //}
            //catch { }

			#region validate config settins

			if (string.IsNullOrEmpty(myServerKey))
			{
				WriteLogEntry("Invalid value for configuration setting ServerKey.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

            if (string.IsNullOrEmpty(myDispatcherUri))
            {
                WriteLogEntry("Invalid value for configuration setting DispatcherUri.  This value can not be empty.", EventLogEntryType.Error);
                return false;
            }

            if (string.IsNullOrEmpty(myEndPoint))
            {
                WriteLogEntry("Invalid value for configuration setting EndPoint.  This value can not be empty.", EventLogEntryType.Error);
                return false;
            }

            if (!Int32.TryParse(strPortNumber, out myPortNumber))
            {
                WriteLogEntry("Invalid value for configuration setting PortNum: " + strPortNumber + ".  This value must be a numeric value.", EventLogEntryType.Error);
                return false;
            }

			if( !Int32.TryParse( strMaxThreads, out maxBuilderThreads ) )
			{
				WriteLogEntry( "Invalid value for configuration setting MaxBuilderThreads: " + strMaxThreads + ".  This value must be a numeric value.  Defaulting to " + maxBuilderThreads.ToString() + ".", EventLogEntryType.Error );
				//return false;
			}

            if (!Int32.TryParse(strFrequency, out processingFrequency))
            {
                WriteLogEntry("Invalid value for configuration setting ProcessingFrequency: " + strFrequency + ".  This value must be a numeric value.", EventLogEntryType.Error);
                return false;
            }


			#endregion

			return true;
		}

		public void IncrementCompletedCount()
		{
			//Increment the number of filings that were successfully processde.  This needs to be done
			//using Interlocked because other worker threads could be accessing the property as well
			Interlocked.Increment(ref this.completedFilings);
		}

        public void WriteLogEntry(string message, EventLogEntryType entryType)
        {
            //myEventLog.WriteEntry(message, entryType);
            switch (entryType)
            {
                case EventLogEntryType.Error:
                    LogManager.TheMgr.WriteError(message);
                    break;
                case EventLogEntryType.Information:
                    LogManager.TheMgr.WriteInformation(message);
                    break;
                case EventLogEntryType.Warning:
                    LogManager.TheMgr.WriteWarning(message);
                    break;
                default:
                    LogManager.TheMgr.WriteInformation(message);
                    break;
            }
        }

		public bool CanAccessFolder(string folderPath, out string errorMsg)
		{
			errorMsg = string.Empty;

            bool retValue = true;
			if (!Directory.Exists(folderPath))
			{
				errorMsg = "The given location does not exist.";
				retValue = false;
			}

			string testFile = Path.Combine(folderPath, "test.txt");
			if (retValue)
			{
				try
				{
					File.Create(testFile).Close();
				}
				catch
				{
					errorMsg = "The service does not have permission to Read or Write from the given folder.";
					retValue = false;
				}
			}

			if(retValue)
			{
				bool fileDeleted = false;
				bool retry = true;
				int retryCount = 0;

				//It is possible that eventhough we have closed the file stream that points to the test
				//file we just created, that the OS still has a lock on the file when we try to delete it,
				//So we should retry the delete if it fails just to give the OS a chance to clear any locks
				//that may still exist.
				while (retry && retryCount < 25)
				{
					try
					{
						File.Delete(testFile);
						fileDeleted = true;
						retry = false;
					}
					catch
					{
						retryCount++;

						//Give the OS some time to catch up in case it still has a lock on the test file
						Thread.Sleep(5);
					}
				}

				if (!fileDeleted)
				{
					errorMsg = "The service does not have permission to Read or Write from the given folder.";
				}
				retValue = fileDeleted; ;
			}

			return retValue;
		}

		public void SetFilingsFolder(string folderPath)
		{
			filingsFolder = folderPath;
			if(string.IsNullOrEmpty(folderPath))
			{
				filingsFolderInfo = null;
			}
			else
			{
				filingsFolderInfo = new DirectoryInfo(folderPath);
			}
		}

		public void SetReportsFolder(string folderPath)
		{
			reportsFolder = folderPath;
			if (string.IsNullOrEmpty(folderPath))
			{
				reportsFolderInfo = null;
			}
			else
			{
				reportsFolderInfo = new DirectoryInfo(folderPath);
			}
		}

		#endregion

		#region helper methods

		private void ProcessFilings(List<FilingInfo> filings)
		{
			if (filings == null || filings.Count == 0)
				return;

			//This does not actually lock the folder, but rather the object that refers to the folder
			//we only do this so that ProcessFilings will not run multiple times for a given instance
			//of the service.
			lock (this.processingFolderInfo)
			{
				completedFilings = 0;
				foreach (FilingInfo filing in filings)
				{
					myWokerPool.QueueUserWorkItem(this.filingProcessor.ProcessFilingCallback, filing);

					string msg = string.Format( "Work item {0} queued for processing on server {1}.", filing.AccessionNumber, this.myServerKey );
					WriteLogEntry( msg, EventLogEntryType.Information );
				}
			}
		}

		private void ProcessNewFilings()
		{
			try
			{
				//This does not actually lock the folder, but rather the object that refers to the folder
				//we only do this so that ProcessNewFilings will not run multiple times for a given instnace
				lock (filingsFolderInfo)
				{
					if (filingProcessor.HasNewFilings())
					{
						WriteLogEntry(string.Format("{0} found new filings, prepping the oldest filing for processing.", this.myServerKey), EventLogEntryType.Information);                       
						List<FilingInfo> filings;

                        this.filingProcessor.TryMoveFilingsToProcessingFolder(out filings);
                        this.ProcessFilings(filings);
					}
				}
			}
			catch (Exception ex)
			{
				WriteLogEntry("Exception while processing new filings: " + ex.Message, EventLogEntryType.Error);
				return;
			}
		}

		protected bool IsFilingComplete(FilingInfo filing)
		{
			//TODO:  we need logic here and some of it depends upon the SEC and what they write 
			//with the filing
			return true;
		}
        
		private bool TryGetDispatcherRemotable(out IFilingDispatcherRemotable remotable, out string errorMsg)
		{
			remotable = null;
			errorMsg = null;
			try
			{
				remotable = (IFilingDispatcherRemotable)Activator.GetObject(typeof(IFilingDispatcherRemotable), myDispatcherUri);
			}
			catch
			{
			}

			if (remotable == null)
			{
				errorMsg = "Unable to connect to the Dispatcher";
				return false;
			}

			return true;
		}

        /// <summary>
        /// Method used to set market that needs to be processed.
        /// This method will load relevant data from database.
        /// </summary>
        /// <param name="MarketId"></param>
        public void LoadMarketInfo(int MarketId)
        {

            //Check if filingProcessor object passed in at startup implements IFilingDatabase interface.
            //If IFilingDatabase is implemented then call method to load market information into processor.

            IFilingDatabase filingProcessorDB = this.filingProcessor as IFilingDatabase;
            if (filingProcessorDB != null)
            {
                filingProcessorDB.LoadMarketInfo(MarketId, out filingsFolder);

                //Start the timer
                if (!string.IsNullOrEmpty(filingsFolder))
                {
                    filingsFolderInfo = new DirectoryInfo(filingsFolder);
                    myTimer = new System.Timers.Timer((double)(processingFrequency * 1000));
                    myTimer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer_Elapsed);
                    myTimer.Start();
                }
            }
            else
            {
                WriteLogEntry("LoadMarketInfo failed! Filing Processor does not implement IFilingDatabase", EventLogEntryType.Error);
            }
        }

        public void LoadAssociatedMarketInfo()
        {
            //Check if filingProcessor object passed in at startup implements IFilingDatabase interface.
            //If IFilingDatabase is implemented then call method to load market information into processor.

            IFilingDatabase filingProcessorDB = this.filingProcessor as IFilingDatabase;
            if (filingProcessorDB != null)
            {
                int MarketId;
                if (filingProcessorDB.GetAssociatedMarketId(this.myServerKey,out MarketId))
                {
                    if (MarketId > 0)
                    {
                        string errorMsg;
                        IFilingDispatcherRemotable dispatcher;
                        if (TryGetDispatcherRemotable(out dispatcher, out errorMsg))
                        {
                            try
                            {
                                dispatcher.AssignMarket(this.myServerKey,MarketId);
                            }
                            catch (Exception ex)
                            {
                                WriteLogEntry("Exception thrown in call to AssignMarket: " + ex.Message, EventLogEntryType.Warning);
                            }
                        }
                        else
                        {
                            //WriteLogEntry("Failed to UnRegister Processor, " + this.myServerKey + ", from the Dispatcher: " + errorMsg, EventLogEntryType.Warning);
                            WriteLogEntry("Failed to get remotable dispatcher to assign Associated market", EventLogEntryType.Warning);
                        }

                        //filingProcessorDB.LoadMarketInfo(MarketId, out filingsFolder);

                        ////Start the timer
                        //if (!string.IsNullOrEmpty(filingsFolder))
                        //{
                        //    filingsFolderInfo = new DirectoryInfo(filingsFolder);
                        //    myTimer = new System.Timers.Timer((double)(processingFrequency * 1000));
                        //    myTimer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer_Elapsed);
                        //    myTimer.Start();
                        //}
                    }
                }
            }
        }

        public bool LoadMarketBaseTaxonomy(string path)
        {
            bool retVal = false;
            //Check if filingProcessor object passed in at startup implements IFilingDatabase interface.
            //If IFilingDatabase is implemented then call method to load market information into processor.

            IFilingDatabase filingProcessorDB = this.filingProcessor as IFilingDatabase;
            if (filingProcessorDB != null)
            {
                retVal = filingProcessorDB.LoadBaseTaxonomy(path);
            }
            else
            {
                WriteLogEntry("LoadBaseTaxonomy failed! Filing Processor does not implement IFilingDatabase", EventLogEntryType.Error);
            }
            return retVal;
        }

        public bool CreateBatchDetailExportFile(int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile)
        {
            bool retVal = false;
            IFilingDatabase filingProcessorDB = this.filingProcessor as IFilingDatabase;
            if (filingProcessorDB != null)
            {
                filingProcessorDB.CreateBatchDetailExportFile(batchDetailID, xbrlDocumentID, language, fileFormatType, replaceExistingFile);
				retVal = true;
            }
            else
            {
                WriteLogEntry("Create Batch Detail Export File Failed! Filing Processor does not implement IFilingDatabase", EventLogEntryType.Error);
                retVal = false;
            }
            return retVal;
        }

        public bool CreateBatchExportFiles(int batchMarketID, string language, ExportFileType fileFormatType, bool replaceExistingFiles)
        {
            bool retVal = false;
            IFilingDatabase filingProcessorDB = this.filingProcessor as IFilingDatabase;
            if (filingProcessorDB != null)
            {
                filingProcessorDB.CreateBatchExportFiles(batchMarketID, language, fileFormatType, replaceExistingFiles);
				retVal = true;
            }
            else
            {
                WriteLogEntry("Create Batch Detail Export File Failed! Filing Processor does not implement IFilingDatabase", EventLogEntryType.Error);
                retVal = false;
            }
            return retVal;
        }

		#endregion
	}
}

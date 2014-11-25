using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Aucent.FilingServices.Data;

using XBRLReportBuilder;

namespace Aucent.XBRLReportBuilder.BuilderService
{
	public class ReportBuilderManager
	{
		private EventLog myEventLog = null;
		private int completedFilings = -1;
		private WorkerThreadPool myWokerPool = null;

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

		/// <summary>
		/// The root folder where the ReportDispatcher will place filings that need to 
		///	 be processed by the service.
		/// </summary>
		protected string processingFolder = string.Empty;
		protected DirectoryInfo processingFolderInfo = null;

		/// <summary>
		/// The root folder where the service will save the reports that it generates from a filing.
		/// </summary>
		protected string reportsFolder = string.Empty;
		protected DirectoryInfo reportsFolderInfo = null;

		/// <summary>
		/// The maximumn number of threads that can be building reports at a given time
		/// </summary>
		private int maxBuilderThreads = -1;

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

		#region constructor

		protected ReportBuilderManager()
		{
			myEventLog = new EventLog();
			if (!System.Diagnostics.EventLog.SourceExists("ReportProcessor"))
			{
				System.Diagnostics.EventLog.CreateEventSource(
				   "ReportProcessor", "ReportBuilder");
			}
			myEventLog.Source = "ReportProcessor";
			myEventLog.Log = "ReportBuilder";

			if (myEventLog.OverflowAction != OverflowAction.OverwriteAsNeeded)
			{
				myEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
			}
		}

		#endregion

		#region singleton

		private static ReportBuilderManager theMgr = null;
		public static ReportBuilderManager TheMgr
		{
			get 
			{
				if (theMgr == null)
				{
					theMgr = new ReportBuilderManager();
				}
				return theMgr;
			}
		}

		#endregion

		#region events

		protected void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Checking for new filings");

			//Pause the timer while we working on processing the filings,  this will prevent
			//multiple timer elapses from stepping on each other.
			myTimer.Stop();
			try
			{
				ProcessNewFilings();
			}
			finally
			{
				myTimer.Start();
			}

		}

		#endregion

		#region public methods

		public void Startup()
		{
			myEventLog.WriteEntry(string.Format("{0} is starting.", this.myServerKey));

			filingsFolderInfo = new DirectoryInfo(filingsFolder);
			processingFolderInfo = new DirectoryInfo(processingFolder);
			reportsFolderInfo = new DirectoryInfo(reportsFolder);

			ServiceUtilities.RegisterChannel(this.myPortNumber);
			ServiceUtilities.RegisterService(typeof(ReportBuilderRemotable), this.myEndPoint);

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
					myEventLog.WriteEntry("Exception thrown while trying to connect to the Dispatcher: " + ex.Message, EventLogEntryType.Error);
					throw ex;
				}
			}
			else
			{
				myEventLog.WriteEntry("Unable to connect to the Dispatcher: " + errorMsg, EventLogEntryType.Error);
				throw new Exception("Unable to connect to the Dispatcher: " + errorMsg);
			}

			myWokerPool = new WorkerThreadPool(maxBuilderThreads);

			myTimer = new System.Timers.Timer((double)(processingFrequency * 1000));
			myTimer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer_Elapsed);
			myTimer.Start();
		}

		public void ShutDown()
		{
			myEventLog.WriteEntry(string.Format("{0} is shutting down.", this.myServerKey));

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
					myEventLog.WriteEntry("Exception thrown in call to UnRegisterProcessor: " + ex.Message, EventLogEntryType.Warning);
				}
			}
			else
			{
				myEventLog.WriteEntry("Failed to UnRegister Processor, " + this.myServerKey + ", from the Dispatcher: " + errorMsg, EventLogEntryType.Warning);
			}
		}

		public bool TryLoadConfigurationSettings()
		{
			myServerKey = ConfigurationManager.AppSettings["ServerKey"];
			filingsFolder = ConfigurationManager.AppSettings["FilingsFolder"];
			processingFolder = ConfigurationManager.AppSettings["ProcessingFolder"];
			reportsFolder = ConfigurationManager.AppSettings["ReportsFolder"];
			myEndPoint = ConfigurationManager.AppSettings["EndPoint"];
			myDispatcherUri = ConfigurationManager.AppSettings["DispatcherUri"];

			string strPortNumber = ConfigurationManager.AppSettings["PortNum"];
			string strMaxThreads = ConfigurationManager.AppSettings["MaxBuilderThreads"];
			string strFrequency = ConfigurationManager.AppSettings["ProcessingFrequency"];

			#region validate config settins

			if (string.IsNullOrEmpty(myServerKey))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting ServerKey.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (string.IsNullOrEmpty(filingsFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting FilingsFolder.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}
			if (string.IsNullOrEmpty(processingFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting FilingsFolder.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}
			if (string.IsNullOrEmpty(reportsFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting ReportsFolder.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (!Directory.Exists(filingsFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting FilingsFolder: " + filingsFolder + ".  This directory does not exists or is not accessible.", EventLogEntryType.Error);
				return false;
			}
			if (!Directory.Exists(processingFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting ProcessingFolder: " + processingFolder + ".  This directory does not exists or is not accessible.", EventLogEntryType.Error);
				return false;
			}
			if (!Directory.Exists(reportsFolder))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting FilingsFolder: " + reportsFolder + ".  This directory does not exists or is not accessible.", EventLogEntryType.Error);
				return false;
			}

			if (string.IsNullOrEmpty(myDispatcherUri))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting DispatcherUri.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (string.IsNullOrEmpty(myEndPoint))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting EndPoint.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (!Int32.TryParse(strPortNumber, out myPortNumber))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting PortNum: " + strPortNumber + ".  This value must be a numeric value.", EventLogEntryType.Error);
				return false;
			}

			if (!Int32.TryParse(strMaxThreads, out maxBuilderThreads))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting MaxBuilderThreads: " + strMaxThreads + ".  This value must be a numeric value.", EventLogEntryType.Error);
				return false;
			}

			if (!Int32.TryParse(strFrequency, out processingFrequency))
			{
				myEventLog.WriteEntry("Invalid value for configuration setting ProcessingFrequency: " + strFrequency + ".  This value must be a numeric value.", EventLogEntryType.Error);
				return false;
			}

			#endregion

			return true;
		}

		public void ProcessFilings(List<FilingInfo> filings)
		{
			//This does not actually lock the folder, but rather the object that refers to the folder
			//we only do this so that ProcessFilings will not run multiple times for a given instnace
			//if the service.
			lock (processingFolderInfo)
			{
				string msg = string.Format("Starting processing of {0} filings in {1}.", filings.Count, this.myServerKey);
				myEventLog.WriteEntry(msg, EventLogEntryType.Information);

				completedFilings = 0;
				foreach(FilingInfo filing in filings)
				{
					myWokerPool.QueueUserWorkItem(this.BuildReportsCallBack, filing);
				}

				while (myWokerPool.QueuedCallbacks > 0 ||
					myWokerPool.WaitCount < myWokerPool.MaxWorkerThreads)
				{
					Thread.Sleep(250);
				}

				msg = string.Format("{0} has finished processing. {1} of {2} filings were completed successfully.", this.myServerKey, this.completedFilings, filings.Count);
				myEventLog.WriteEntry(msg, EventLogEntryType.Information);
			}
		}

		public void BuildReportsCallBack(Object objFiling)
		{
			FilingInfo filing = objFiling as FilingInfo;
			if (filing == null)
			{
				return;
			}
			BuildReports(filing);
		}

		public bool BuildReports(FilingInfo filing)
		{
			string instancePath = GetInstanceDocPath(filing);
			if (string.IsNullOrEmpty(instancePath) ||
				!File.Exists(instancePath))
			{
				myEventLog.WriteEntry("Can not find instance document for filing: " + filing.AccessionNumber);
				return false;
			}

			string taxonomyPath = GetTaxonomyPath(filing);
			if (string.IsNullOrEmpty(taxonomyPath) ||
				!File.Exists(taxonomyPath))
			{
				myEventLog.WriteEntry("Can not find taxonomy file for filing: " + filing.AccessionNumber);
				return false;
			}

			ReportBuilder rb = new ReportBuilder();

			string reportPath = string.Format("{0}{1}{2}", reportsFolder, Path.DirectorySeparatorChar, filing.AccessionNumber);
			string filingFile = string.Format("{0}{1}{2}", reportPath, Path.DirectorySeparatorChar, FilingSummary._FilingSummaryXmlName);

			//Make sure there is a clean folder where the reports will be written to.
			if (Directory.Exists(reportPath))
			{
				Directory.Delete(reportPath, true);
			}
			Directory.CreateDirectory(reportPath);

			//TODO:  get this info from the manifest once we have it
			//rb.PeriodEnding = filing.period_ending;
			//rb.FilingDate = filing.filing_date;
			//rb.TickerSymbol = filing.ticker_symbol;
			//rb.CompanyName = filing.company_name;
			//rb.AccessionNumber = filing.accession_number;
			//rb.FiscalYearEnd = filing.fiscal_year_end;

			string error = null;
			FilingSummary summary = null;
			if (!rb.BuildReports(instancePath, taxonomyPath, filingFile, reportPath, filing.FormType, out summary, out error))
			{
				myEventLog.WriteEntry("build reports failed! " + error, EventLogEntryType.Error);
				return false;
			}
			//Increment the number of filings that were successfully process.  This needs to be done
			//using Interlocked because other worker threads could be accessing the property as well
			Interlocked.Increment(ref completedFilings);
			return true;
		}

		#endregion

		#region helper methods

		private void ProcessNewFilings()
		{
			try
			{
				lock (filingsFolderInfo)
				{
					//Get all of the new sub folders in the filings folder
					DirectoryInfo[] filingDirs = filingsFolderInfo.GetDirectories();
					if (filingDirs.Length == 0)
					{
						//There are no new directories, so there is nothing to process, simply return
						return;
					}

					myEventLog.WriteEntry("Found new filings, prepping the folders for processing.");

					string processingSubDir = Guid.NewGuid().ToString();
					DirectoryInfo targetFolderInfo = processingFolderInfo.CreateSubdirectory(processingSubDir);
					List<FilingInfo> filings = new List<FilingInfo>();

					foreach (DirectoryInfo subDir in filingsFolderInfo.GetDirectories())
					{
						if (TryMoveFilingToProcessingFolder(subDir, targetFolderInfo))
						{
							FilingInfo filing = new FilingInfo();
							filing.AccessionNumber = subDir.Name;
							filing.ParentFolder = targetFolderInfo.FullName;
							filings.Add(filing);
						}
					}
					this.ProcessFilings(filings);
				}
			}
			catch (Exception ex)
			{
				myEventLog.WriteEntry("Exception while processing new filings: " + ex.Message);
				return;
			}
		}

		private bool TryMoveFilingToProcessingFolder(DirectoryInfo sourceDir, DirectoryInfo targetDir)
		{
			try
			{
				targetDir.CreateSubdirectory(sourceDir.Name);
				foreach (FileInfo file in sourceDir.GetFiles())
				{
					string destFile = string.Format("{0}{1}{2}{1}{3}", targetDir.FullName, Path.DirectorySeparatorChar, sourceDir.Name, file.Name);
					file.CopyTo(destFile, true);
				}

				FileUtilities.DeleteDirectory(sourceDir, true, true);
			}
			catch
			{
				return false;
			}
			return true;
		}


		protected bool IsFilingComplete(FilingInfo filing)
		{
			//TODO:  we need logic here and some of it depends upon the SEC and what they write 
			//with the filing
			return true;
		}

		protected string GetInstanceDocPath(FilingInfo filing)
		{

			string filingDir = string.Format("{0}{1}{2}", filing.ParentFolder, Path.DirectorySeparatorChar, filing.AccessionNumber);
			string[] files = Directory.GetFiles(filingDir, "*.xml");
			Regex instanceRegex = new Regex(@"\d{8}\.xml$");
			
			string retValue = string.Empty;
			foreach (string file in files)
			{
				Match regexMatch = instanceRegex.Match(file);
				if (regexMatch != null && regexMatch.Success)
				{
					//There should only be one file that matches the regex in each filing, so 
					//once we find it break and return
					retValue = file;
					break;
				}
			}

			return retValue;
		}

		protected string GetTaxonomyPath(FilingInfo filing)
		{
			string taxonomyFile = string.Empty;

			string instanceFile = GetInstanceDocPath(filing);
			if (string.IsNullOrEmpty(instanceFile))
			{
				return string.Empty;
			}

			XmlTextReader xReader = new XmlTextReader(instanceFile);
			try
			{
				xReader.MoveToContent();
				if (xReader.LocalName == "xbrl")
				{
					while (xReader.MoveToNextAttribute())
					{
						if (xReader.Value == "http://ici.org/rr/2006")
						{
							break;
						}
					}

					xReader.Read();
					while (xReader.LocalName != "schemaRef")
					{
						xReader.Read();
					}

					if (xReader.LocalName == "schemaRef")
					{
						while (xReader.MoveToNextAttribute())
						{
							if (xReader.NodeType == XmlNodeType.Attribute &&
								  xReader.Name == "xlink:href")
							{
								taxonomyFile = Path.GetFileName(xReader.Value);
								break;
							}
						}
					}
				}

				taxonomyFile = Path.Combine(Path.GetDirectoryName(instanceFile), taxonomyFile);
			}
			catch (Exception ex)
			{
				myEventLog.WriteEntry("Exception in GetTaxonomy Path: " + ex.Message, EventLogEntryType.Warning);
				taxonomyFile = string.Empty;
			}

			return taxonomyFile;
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

		#endregion
	}
}

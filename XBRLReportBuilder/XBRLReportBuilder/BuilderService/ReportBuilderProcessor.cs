/*****************************************************************************
 * ReportBuilderProcessor (class)
 * Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
 * This class is the Implementation of the IFilingProcessor interface that is used
 * to generate Report files from instance documents.  The class implements the code
 * that handles the moving of new filing files to the processing location, and
 * implements the actuall generation of the Report files themselves.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

using Aucent.FilingServices.Data;
using Aucent.FilingServices.Data.Interfaces;
using Aucent.FilingServices.FilingProcessorBase;

using XBRLReportBuilder;
using XBRLReportBuilder.Utilities;
using System.Threading;
using System.Xml;
using System.Collections.Specialized;
using System.Reflection;
using System.Collections;

using System.Net.Cache;
//using net.xmlcatalog;

namespace Aucent.XBRLReportBuilder.BuilderService
{
	public class ReportBuilderProcessor : IFilingProcessor
	{
		private const string ERROR_FILE_NAME = "errorLog.txt";

		private Dictionary<string, int> filingRetryCount = new Dictionary<string, int>();

		protected int maxUnzipAttempts = 0;

		private static string currencyMappingFile = null;
		public static string CurrencyMappingFile
		{
			get
			{
				lock( configLockObject )
				{
					if( currencyMappingFile == null )
						currencyMappingFile = AppSettings[ "CurrencyMappingFile" ] as string;
				}

				return currencyMappingFile;
			}
		}

		private static bool? deleteProcessedFilings = null;
		public static bool DeleteProcessedFilings
		{
			get
			{
				lock( configLockObject )
				{
					if( deleteProcessedFilings == null )
						deleteProcessedFilings = AppSettings[ "DeleteProcessedFilings" ] == "true";
				}

				return deleteProcessedFilings == true;
			}
		}


		private static string financialRuleFile = string.Empty;
		public static string FinancialRuleFile
		{
			get
			{
				lock( configLockObject )
				{
					if( string.IsNullOrEmpty( financialRuleFile ) )
						financialRuleFile = AppSettings[ "FinancialRuleFile" ];
				}

				return financialRuleFile;
			}
		}

		private static HtmlReportFormat htmlReportFormat = HtmlReportFormat.None;
		public static HtmlReportFormat HtmlReportFormat
		{
			get
			{
				lock( configLockObject )
				{
					if( htmlReportFormat == HtmlReportFormat.None )
					{
						try
						{
							//default to Complete
							htmlReportFormat = HtmlReportFormat.Complete;

							//then override
							htmlReportFormat = (HtmlReportFormat)Enum.Parse( typeof( HtmlReportFormat ), AppSettings[ "HtmlReportFormat" ] as string );
						}
						catch { }
					}
				}

				return htmlReportFormat;
			}
		}

		private static ReportFormat reportFormat = ReportFormat.None;
		public static ReportFormat ReportFormat
		{
			get
			{
				lock( configLockObject )
				{
					if( reportFormat == ReportFormat.None )
					{
						try
						{
							//default to Xml
							reportFormat = ReportFormat.Xml;

							//then override
							reportFormat = (ReportFormat)Enum.Parse( typeof( ReportFormat ), AppSettings[ "ReportFormat" ] as string );
						}
						catch { }
					}
				}

				return reportFormat;
			}
		}

		private static bool remoteFileCachePolicyChecked = false;
		private static RequestCacheLevel remoteFileCachePolicy = RequestCacheLevel.Default;
		public static RequestCacheLevel RemoteFileCachePolicy
		{
			get
			{
				lock( configLockObject )
				{
					if( !remoteFileCachePolicyChecked )
					{
						remoteFileCachePolicyChecked = true;

						try
						{
							string policy = AppSettings[ "RemoteFileCachePolicy" ] as string;
							remoteFileCachePolicy = (RequestCacheLevel)Enum.Parse( typeof( RequestCacheLevel ), policy );
						}
						catch { }
					}
				}

				return remoteFileCachePolicy;
			}
		}

		private static object configLockObject = new object();

		//private static bool xmlCatalogChecked = false;
		//private static XmlCatalogResolver xmlCatalog = null;
		//public static XmlCatalogResolver XmlCatalog
		//{
		//	get
		//	{
		//		if( !xmlCatalogChecked )
		//		{
		//			xmlCatalogChecked = true;					

		//			try
		//			{
		//				string xmlCatalogPath = AppSettings[ "XmlCatalogPath" ] as string;
		//				xmlCatalog = new XmlCatalogResolver( xmlCatalogPath );
		//			}
		//			catch { }
		//		}

		//		return xmlCatalog;
		//	}
		//}

		private static Dictionary<string,string> appSettings = null;
		private static Dictionary<string,string> AppSettings
		{
			get
			{
				lock( configLockObject )
				{
					if( appSettings == null )
					{
						FilingProcessorManager.TheMgr.WriteLogEntry( "Reading default configuration file.", EventLogEntryType.Information );
						appSettings = new Dictionary<string, string>();
						foreach( string key in ConfigurationManager.AppSettings.AllKeys )
						{
							appSettings[ key ] = ConfigurationManager.AppSettings[ key ];
						}
					}

					if( appSettings == null || appSettings.Count == 0 )
					{
						FilingProcessorManager.TheMgr.WriteLogEntry( "Reading assembly configuration file.", EventLogEntryType.Information );

						appSettings = new Dictionary<string, string>();

						Assembly asm = Assembly.GetExecutingAssembly();
						string executable = asm.CodeBase.Replace( "/", "\\" ).Substring( 8 );
						Configuration conf = ConfigurationManager.OpenExeConfiguration( executable );
						foreach( string key in conf.AppSettings.Settings.AllKeys )
						{
							appSettings[ key ] = conf.AppSettings.Settings[ key ].Value;
						}
					}
				}

				return appSettings;
			}
		}

		public ReportBuilderProcessor()
		{
			this.maxUnzipAttempts = 1;
		}

		public ReportBuilderProcessor(int maxUnzipAttempts) : this()
		{
			this.maxUnzipAttempts = maxUnzipAttempts;
		}

		#region IFilingProcessor

		public void ProcessFilingCallback(Object objFiling)
		{
			FilingInfo filing = objFiling as FilingInfo;
			if (filing == null)
			{
				return;
			}
			string error;
			BuildReports(filing, FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, out error);
		}

		public bool HasNewFilings()
		{
			FileInfo[] zipFiles = FilingProcessorManager.TheMgr.FilingsFolderInfo.GetFiles("*.zip", SearchOption.TopDirectoryOnly);
			return zipFiles.Length > 0;
		}

		public bool TryMoveFilingsToProcessingFolder(out List<FilingInfo> filings)
		{
			filings = new List<FilingInfo>();

			FileInfo[] zipFiles = FilingProcessorManager.TheMgr.FilingsFolderInfo.GetFiles("*.zip", SearchOption.TopDirectoryOnly);
			Array.Sort( zipFiles, ( left, right ) => DateTime.Compare( left.CreationTime, right.CreationTime ) );

			Guid batchId = Guid.NewGuid();
			DirectoryInfo processingBatchFolder = FilingProcessorManager.TheMgr.ProcessingFolderInfo.CreateSubdirectory(batchId.ToString());
			
			foreach (FileInfo zipFile in zipFiles)
			{
				string filingName = Path.GetFileNameWithoutExtension(zipFile.Name);
				if( string.IsNullOrEmpty( filingName ) )
				{
					try
					{
						string errorFile = Path.Combine( FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, "UNKNOWN.txt" );
						string errorMessage = "Cannot process a filing zip which has no base name.";
						FilingProcessorManager.TheMgr.WriteLogEntry( errorMessage, EventLogEntryType.Error );
						File.WriteAllText( errorFile, errorMessage );
						zipFile.Delete();
					} catch { }

					continue;
				}

				DirectoryInfo filingProcessingFolder = processingBatchFolder.CreateSubdirectory(filingName);

				string[] unzippedFiles;
				string filingZipFile = string.Format("{0}{1}{2}", filingProcessingFolder.FullName, Path.DirectorySeparatorChar,Path.GetFileName(zipFile.Name));

				if (filingRetryCount.ContainsKey(filingName) &&
					filingRetryCount[filingName] >= maxUnzipAttempts)
				{
					filingRetryCount.Remove(filingName);

					//We have failed to extract the filing multiple times, stop trying to make it work,
					//remove the zip file from the filing folder, log an error in the reports folder,
					//and reomve the entry from the dictionary
					string errorFilename = filingName +"_"+ ERROR_FILE_NAME;
					string errorFile = Path.Combine( FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, filingName );

					string errorMsg = string.Format("Cannot extract files for filing {0}.  The max number of retries has been reached, removing the zip file from the Filings folder. ", Path.GetFileNameWithoutExtension(zipFile.FullName));
					if (File.Exists(errorFile))
					{
						FileUtilities.DeleteFile(new FileInfo(errorFile), true);
					}
					FilingProcessorManager.TheMgr.WriteLogEntry(errorMsg, EventLogEntryType.Error);
					File.WriteAllText(errorFile, errorMsg);

					zipFile.CopyTo(filingZipFile);
					FileUtilities.DeleteFile(zipFile, true);
				}
				else
				{
					string zipError;
					if (ZipUtilities.TryUnzipAndUncompressFiles(zipFile.FullName, filingProcessingFolder.FullName, out unzippedFiles, out zipError))
					{
						FilingInfo filing = new FilingInfo();
						filing.AccessionNumber = filingProcessingFolder.Name;
						filing.ParentFolder = processingBatchFolder.FullName;
						filings.Add(filing);
						
						zipFile.CopyTo(filingZipFile, true);
						FileUtilities.DeleteFile(zipFile, true);

						//SEC0145 - Only queue one filing at a time
						break;
					}
					else
					{
						//Delete the folder from the processing batch folder sense it doesn't contain a valid filing
						FileUtilities.DeleteDirectory(filingProcessingFolder, true, true);
						if (!filingRetryCount.ContainsKey(filingName))
						{
							filingRetryCount.Add(filingName, 0);
						}
						filingRetryCount[filingName]++;
						FilingProcessorManager.TheMgr.WriteLogEntry(string.Format("Can not extract files for filing {0}.  The zip file may not be complete. ", Path.GetFileNameWithoutExtension(zipFile.FullName)), EventLogEntryType.Warning);
					}
				}
			}

			if (processingBatchFolder.GetDirectories().Length == 0)
			{
				//There were not any valid filings in this batch, remove the batch folder
				FileUtilities.DeleteDirectory(processingBatchFolder, true, true);
			}

			return true;
		}

		public bool TryGetDependentFolderConfiguration(out string filingsFolder, out string processingFolder, out string reportsFolder, out string errorMsg)
		{
			errorMsg = string.Empty;

			filingsFolder = ConfigurationManager.AppSettings["FilingsFolder"];
			processingFolder = ConfigurationManager.AppSettings["ProcessingFolder"];
			reportsFolder = ConfigurationManager.AppSettings["ReportsFolder"];

			if (string.IsNullOrEmpty(processingFolder))
			{
				errorMsg = "Invalid value for configuration setting ProcessingFolder.  This value can not be empty.";
				return false;
			}
			if (!FilingProcessorManager.TheMgr.CanAccessFolder(processingFolder, out errorMsg))
			{
				errorMsg = "Invalid value for configuration setting ProcessingFolder: " + processingFolder + ".  This directory does not exists or is not accessible.";
				return false;
			}

			if (string.IsNullOrEmpty(reportsFolder))
			{
				errorMsg = "Invalid value for configuration setting ReportsFolder.  This value can not be empty.";
				return false;
			}
			if (!FilingProcessorManager.TheMgr.CanAccessFolder(reportsFolder, out errorMsg))
			{
				errorMsg = "Invalid value for configuration setting ReportsFolder: " + reportsFolder + ".  This directory does not exists or is not accessible.";
				return false;
			}

			if (string.IsNullOrEmpty(filingsFolder))
			{
				errorMsg = "Invalid value for configuration setting FilingsFolder.  This value can not be empty.";
				return false;
			}
			if (!FilingProcessorManager.TheMgr.CanAccessFolder(filingsFolder, out errorMsg))
			{
				errorMsg = "Invalid value for configuration setting FilingsFolder: " + filingsFolder + ".  This directory does not exists or is not accessible.";
				return false;
			}

			return true;
		}


		#endregion

		#region helper methods

		protected bool BuildReports(FilingInfo filing, string reportsFolder, out string error)
		{
			error = null;

			bool foundFiles = true;
			string errorMsg = string.Empty;

			string filingPath = Path.Combine( filing.ParentFolder, filing.AccessionNumber );
			string filingReportsPath = Path.Combine( filingPath, "Reports" );
			string filingErrorFile = Path.Combine( filingReportsPath, ERROR_FILE_NAME );
			
			string instancePath = filing.GetInstanceDocPath();
			string taxonomyPath = filing.GetTaxonomyPath();

			if (string.IsNullOrEmpty(instancePath) ||
				!File.Exists(instancePath))
			{
				errorMsg = string.Format("Can not find instance document for filing: {0}", filing.AccessionNumber);
				FilingProcessorManager.TheMgr.WriteLogEntry(errorMsg, EventLogEntryType.Error);
				foundFiles = false;
			}
			else if (string.IsNullOrEmpty(taxonomyPath) ||
				!File.Exists(taxonomyPath))
			{
				errorMsg = string.Format("Can not find taxonomy file for filing: {0}", filing.AccessionNumber);
				FilingProcessorManager.TheMgr.WriteLogEntry(errorMsg, EventLogEntryType.Error);
				foundFiles = false;
			}

			bool buildSucceeded = false;
			if (foundFiles)
			{
                string baseResourcePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                FilingProcessorManager.TheMgr.WriteLogEntry(string.Format("Setting base path for Rules Engine. Path: {0}",baseResourcePath), EventLogEntryType.Information);
                
				RulesEngineUtils.SetBaseResourcePath(baseResourcePath);
				ReportBuilder.SetSynchronizedResources( true );
				
				FilingProcessorManager.TheMgr.WriteLogEntry( "Selected rule file: '" + FinancialRuleFile + "' for instance document: '" + Path.GetFileName( instancePath ) + "'", EventLogEntryType.Information );

				ReportBuilder rb = new ReportBuilder( FinancialRuleFile, ReportFormat, HtmlReportFormat );
				rb.CurrencyMappingFile = CurrencyMappingFile;
				rb.RemoteFileCachePolicy = RemoteFileCachePolicy;

				//if( XmlCatalog != null )
				//	rb.XmlCatalog = XmlCatalog;

				//Reports will be saves under the Filing's Processing folder then copied out to the actual reports folder,
				//this will allow us to only copy complete sets of R files to the Reports Folder.
				string filingSummaryFile = string.Format("{0}{1}{2}", filingReportsPath, Path.DirectorySeparatorChar, FilingSummary.FilingSummaryXmlName);

				//Make sure there is a clean folder where the reports will be written to.
				if (Directory.Exists(filingReportsPath))
					Directory.Delete(filingReportsPath, true);

				Directory.CreateDirectory(filingReportsPath);
				FilingSummary summary = null;
				buildSucceeded = rb.BuildReports(instancePath, taxonomyPath, filingSummaryFile, filingReportsPath, out summary, out error);

				if (!buildSucceeded)
				{
					errorMsg = string.Format("Call to BuildReports failed for Filing {0}: {1}", filing.AccessionNumber, error);
					FilingProcessorManager.TheMgr.WriteLogEntry(errorMsg, EventLogEntryType.Error);

					if (!Directory.Exists(filingReportsPath))
						Directory.CreateDirectory(filingReportsPath);

					File.WriteAllText(filingErrorFile, errorMsg);
				}
			}
			else
			{
				if (!Directory.Exists(filingReportsPath))
					Directory.CreateDirectory(filingReportsPath);

				File.WriteAllText(filingErrorFile, errorMsg);
			}
			
			try
			{
				string errorFileName = filing.AccessionNumber + "_" + Path.GetFileName( filingErrorFile );
				string reportErrorFile = Path.Combine( reportsFolder, errorFileName );
				string reportZipFile = Path.Combine( reportsFolder, filing.AccessionNumber + ".zip" );
				if (File.Exists(reportErrorFile))
					FileUtilities.DeleteFile(new FileInfo(reportErrorFile), true);

				if (File.Exists(reportZipFile))
					FileUtilities.DeleteFile(new FileInfo(reportZipFile), true);

				if (buildSucceeded)
				{
					string[] filePathsToZip = Directory.GetFiles(filingReportsPath);
					string[] filesToZip = new string[filePathsToZip.Length];
					for (int i = 0; i < filesToZip.Length; i++)
					{
						filesToZip[i] = Path.GetFileName(filePathsToZip[i]);
					}

					string zipFile = Path.Combine( filingReportsPath, filing.AccessionNumber +".zip");
					if (ZipUtilities.TryZipAndCompressFiles(zipFile, filingReportsPath, filesToZip))
						File.Copy(zipFile, reportZipFile);
				}
				else
				{
					File.Copy(filingErrorFile, reportErrorFile);
				}

				if( DeleteProcessedFilings )
				{
					DirectoryInfo di = new DirectoryInfo( filingPath );
					FileUtilities.DeleteDirectory( di, true, true );

					di = new DirectoryInfo( filing.ParentFolder );
					if( di.GetDirectories().Length == 0 && di.GetFiles().Length == 0 )
						FileUtilities.DeleteDirectory( di, true, true );
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}

			if (buildSucceeded)
				FilingProcessorManager.TheMgr.IncrementCompletedCount();

			return buildSucceeded;
		}

		#endregion
	}
}

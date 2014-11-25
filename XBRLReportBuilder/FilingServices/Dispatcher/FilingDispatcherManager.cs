/**************************************************************************
 * FilingDispatcherManager (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class manages the state of a dispatcher service and defines methods for
 * managing a collection of Processor services that are registered with the dispatcher
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

using Aucent.FilingServices.Log;
using Aucent.FilingServices.Data;
using Aucent.FilingServices.Data.Interfaces;

namespace Aucent.FilingServices.Dispatcher
{
	public class FilingDispatcherManager
	{

		#region properties

		private int myPortNumber = -1;
		private string myEndPoint = string.Empty;
		//private EventLog myEventLog = null;

		private SortedList<string, string> registeredProcessors = null;
		public SortedList<string, string> RegisteredProcessors
		{
			get { return registeredProcessors; }
			set { registeredProcessors = value; }
		}

        private SortedList<string, int> assignedProcessors = null;
        public SortedList<string, int> AssignedProcessors
        {
            get { return assignedProcessors; }
            set { assignedProcessors = value; }
        }

		private string myServerKey = string.Empty;
		public string MyServerKey
		{
			get { return myServerKey; }
		}

		#endregion

		#region singleton

		private static FilingDispatcherManager theMgr = null;
		public static FilingDispatcherManager TheMgr
		{
			get
			{
				if(theMgr == null)
				{
					theMgr = new FilingDispatcherManager();
				}
				return theMgr;
			}
		}

		#endregion

		#region constructors

		private FilingDispatcherManager()
		{
            //myEventLog = new EventLog();
            //if (!System.Diagnostics.EventLog.SourceExists("Dispatcher"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource(
            //       "Dispatcher", "ReportBuilder");
            //}
            //myEventLog.Source = "Dispatcher";
            //myEventLog.Log = "ReportBuilder";

            //if (myEventLog.OverflowAction != OverflowAction.OverwriteAsNeeded)
            //{
            //    myEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
            //}

			registeredProcessors = new SortedList<string, string>();
            assignedProcessors = new SortedList<string, int>();
		}

		#endregion

		#region public methods

		public void Startup()
		{
			WriteLogEntry(string.Format("{0} is starting.", this.myServerKey),EventLogEntryType.Information);
            
			ServiceUtilities.RegisterChannel(this.myPortNumber);
			ServiceUtilities.RegisterService(typeof(FilingDispatcherRemotable), this.myEndPoint);
		}

		public void ShutDown()
		{
			WriteLogEntry(string.Format("{0} is shuttng down.", this.myServerKey), EventLogEntryType.Information);
		}

		public bool TryLoadConfigurationSettings()
		{
			myServerKey = ConfigurationManager.AppSettings["ServerKey"];
			myEndPoint = ConfigurationManager.AppSettings["EndPoint"];

			string strPortNumber = ConfigurationManager.AppSettings["PortNum"];

			#region validate config settins

			if (string.IsNullOrEmpty(myServerKey))
			{
				WriteLogEntry("Invalid value for configuration setting ServerKey.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (string.IsNullOrEmpty(myEndPoint))
			{
				WriteLogEntry("Invalid value for configuration setting DispatcherUri.  This value can not be empty.", EventLogEntryType.Error);
				return false;
			}

			if (!Int32.TryParse(strPortNumber, out myPortNumber))
			{
				WriteLogEntry("Invalid value for configuration setting PortNum: " + strPortNumber + ".  This value must be a numeric value.", EventLogEntryType.Error);
				return false;
			}

			#endregion

			return true;
		}

		public void RegisterProcessor(string processorUri, string serverKey)
		{
			if (registeredProcessors.ContainsKey(serverKey) &&
				registeredProcessors[serverKey] != processorUri)
			{
				string errorMsg = string.Format("Another Builder located at\r\n{0}\r\nis already registerd with the same serverKey, {1}.", registeredProcessors[serverKey], serverKey);
				WriteLogEntry(errorMsg, EventLogEntryType.Warning);
				throw new Exception(errorMsg);
			}
			string msg = string.Format("Registering a new Builder, {0}, located at:\r\n{1}", serverKey, processorUri);
			WriteLogEntry(msg, EventLogEntryType.Information);
			registeredProcessors[serverKey] = processorUri;

		}

		public void UnRegisterProcessor(string serverKey)
		{
			if (registeredProcessors.ContainsKey(serverKey))
			{
				string msg = string.Format("Unregistering Builder, {0}.", serverKey);
				WriteLogEntry(msg, EventLogEntryType.Information);
				registeredProcessors.Remove(serverKey);
                if (assignedProcessors.ContainsKey(serverKey))
                    assignedProcessors.Remove(serverKey);
			}
		}

        /// <summary>
        /// Method to get a list of registered processors.
        /// </summary>
        /// <returns></returns>
        public string[] GetProcessors()
        {
            string[] retVal;
            WriteLogEntry("GetProcessors invoked",EventLogEntryType.Information);
            if (registeredProcessors == null)
                return null;

            retVal = new string[registeredProcessors.Keys.Count];
            int count = 0;
            foreach (string proc in registeredProcessors.Keys)
            {
                retVal[count] = proc;
                count++;
            }
            return retVal;
            //return retVals;
        }

        public string GetProcessor(int marketId)
        {
            string retVal = string.Empty;

            if (assignedProcessors == null || assignedProcessors.Count == 0)
                return retVal;

            foreach (string key in assignedProcessors.Keys)
            {
                if (assignedProcessors[key] == marketId)
                {
                    retVal = key;
                    break;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Method to get a list of registered but unassigned processors
        /// </summary>
        /// <returns></returns>
        public string[] GetUnAssignedProcessors()
        {
            string[] retVal;

            if (registeredProcessors == null)
                return null;

            retVal = new string[registeredProcessors.Keys.Count - assignedProcessors.Keys.Count];
            int count = 0;
            foreach (string proc in registeredProcessors.Keys)
            {
                if (!assignedProcessors.ContainsKey(proc))
                {
                    retVal[count] = proc;
                    count++;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Method to assign market to a processor. 
        /// This is primarily used within Reuters project where a processor service handles 
        /// instance documents from a particular market.
        /// </summary>
        /// <param name="processorServerKey">server key of processor</param>
        /// <param name="marketId">market id in database</param>
        /// <returns></returns>
        public bool AssignMarket(string processorServerKey, int marketId)
        {
            bool retVal = false;
            this.WriteLogEntry(string.Format("Assign Market invoked. Target Service: {0} Market Id: {1}", processorServerKey, marketId.ToString()), EventLogEntryType.Information);
            if (registeredProcessors != null && registeredProcessors.ContainsKey(processorServerKey))
            {
                IFilingProcessorRemotable processor;
                string errors = string.Empty;

                if (TryGetProcessorRemotable(processorServerKey, out processor, out errors))
                {
					try
					{
						//Update the database and the service
						processor.AssignMarketToProcessor(marketId);
						this.WriteLogEntry(string.Format("Assign Market completed. Target Service: {0} Market Id: {1}", processorServerKey, marketId.ToString()), EventLogEntryType.Information);

						//success
						//remove previous market assignment
						string previousProcessor = this.GetProcessor(marketId);
						if (!string.IsNullOrEmpty(previousProcessor))
							assignedProcessors.Remove(previousProcessor);

						//finally, assign the market
						assignedProcessors[processorServerKey] = marketId;
						retVal = true;
					}
                    catch (Exception ex)
                    {
                        WriteLogEntry("Exception thrown in call to AssignMarketToProcessor: " + ex.Message, EventLogEntryType.Warning);
                        retVal = false;
                    }
                }
                else
                {
                    WriteLogEntry("Assign Market to " + this.myServerKey + ", from the Dispatcher Failed: " + errors, EventLogEntryType.Warning);
                    retVal = false;
                }

            }
            return retVal;
        }

        public bool LoadTaxonomy(string processorServerKey,string taxonomyPath)
        {
            bool retVal = false;
            this.WriteLogEntry(string.Format("Load Taxonomy invoked. Target Service: {0} Taxonomy Path: {1}", processorServerKey, taxonomyPath), EventLogEntryType.Information);
            if (registeredProcessors != null && registeredProcessors.ContainsKey(processorServerKey))
            {
                IFilingProcessorRemotable processor;
                string errors = string.Empty;

                if (TryGetProcessorRemotable(processorServerKey, out processor, out errors))
                {
                    try
                    {
                        processor.LoadMarketBaseTaxonomy(taxonomyPath);
                        this.WriteLogEntry(string.Format("Load Taxonomy completed. Target Service: {0} Taxonomy Path: {1}", processorServerKey, taxonomyPath), EventLogEntryType.Information);
                        retVal = true;
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry("Exception thrown in call to LoadTaxonomy: " + ex.Message, EventLogEntryType.Warning);
                    }
                }
                else
                {
                    WriteLogEntry("Load Taxonomy to " + this.myServerKey + ", from the Dispatcher Failed: " + errors, EventLogEntryType.Warning);
                    retVal = false;
                }

            }
            return retVal;
        }

		public void WriteLogEntry(string message, EventLogEntryType entryType)
		{
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

        public bool CreateBatchDetailExportFile(int marketID, int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile, out string error)
        {
            error = null;

            string processorServerKey = GetProcessor(marketID);

            if (string.IsNullOrEmpty(processorServerKey))
            {
                error = string.Format("Loader not found for Market Id: {0}", marketID);
                return false;
            }

            if (registeredProcessors != null && registeredProcessors.ContainsKey(processorServerKey))
            {
                IFilingProcessorRemotable processor;
                string errors = string.Empty;

                if (TryGetProcessorRemotable(processorServerKey, out processor, out errors))
                {
                    try
                    {
                        processor.CreateBatchDetailExportFile(batchDetailID, xbrlDocumentID, language, fileFormatType, replaceExistingFile);
                        this.WriteLogEntry(string.Format("CreateBatchDetailExportFile completed. Target Service: {0} Market Id: {1}", processorServerKey, marketID.ToString()), EventLogEntryType.Information);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry("Exception thrown in call to CreateBatchDetailExportFile: " + ex.Message, EventLogEntryType.Warning);
                        return false;
                    }
                }
                else
                {
                    WriteLogEntry("CreateBatchDetailExportFile() " + this.myServerKey + ", from the Dispatcher Failed: " + errors, EventLogEntryType.Warning);
                    return false;
                }

            }
            else
            {
                error = string.Format("No registered loader");
                return false;
            }
        }

        public bool CreateBatchExportFiles(int marketID, int batchID, string language, ExportFileType fileFormatType, bool replaceExistingFiles, out string error)
        {
            error = null;

            string processorServerKey = GetProcessor(marketID);

            if (string.IsNullOrEmpty(processorServerKey))
            {
                error = string.Format("Loader not found for Market Id: {0}", marketID);
                return false;
            }

            if (registeredProcessors != null && registeredProcessors.ContainsKey(processorServerKey))
            {
                IFilingProcessorRemotable processor;
                string errors = string.Empty;

                if (TryGetProcessorRemotable(processorServerKey, out processor, out errors))
                {
                    try
                    {

                        processor.CreateBatchExportFiles(batchID, language, fileFormatType, replaceExistingFiles);
                        this.WriteLogEntry(string.Format("CreateBatchDetailExportFile completed. Target Service: {0} Market Id: {1}", processorServerKey, marketID.ToString()), EventLogEntryType.Information);
                        return true;
                        
                    }
                    catch (Exception ex)
                    {
                        WriteLogEntry("Exception thrown in call to CreateBatchExportFiles: " + ex.Message, EventLogEntryType.Warning);
                        return false;
                    }
                }
                else
                {
                    WriteLogEntry("CreateBatchExportFiles() " + this.myServerKey + ", from the Dispatcher Failed: " + errors, EventLogEntryType.Warning);
                    return false;
                }

            }
            else
            {
                error = string.Format("No registered loader");
                return false;
            }
        }

		public bool CanProcessorAccessFolder(string processorServerKey, string folderPath, out string errorMsg)
		{
			errorMsg = string.Empty;
			bool retVal = false;
			if (registeredProcessors != null && registeredProcessors.ContainsKey(processorServerKey))
			{
				IFilingProcessorRemotable processor;
				string errors = string.Empty;

				if (TryGetProcessorRemotable(processorServerKey, out processor, out errors))
				{
					try
					{
						retVal = processor.CanAccessFolder(folderPath, out errorMsg);
					}
					catch (Exception ex)
					{
						WriteLogEntry("Exception thrown in call to CanProcessorAccessFolder: " + ex.Message, EventLogEntryType.Warning);
						retVal = false;
					}
				}
			}
			return retVal;
		}

		#endregion

        #region Private methods

        private bool TryGetProcessorRemotable(string serverKey, out IFilingProcessorRemotable processor, out string errorMsg)
        {
            processor = null;
            errorMsg = string.Empty;

            string processorUri = string.Empty;

            if (registeredProcessors == null || !registeredProcessors.ContainsKey(serverKey))
                return false;

            try
            {
                processorUri = registeredProcessors[serverKey];
                processor = (IFilingProcessorRemotable)Activator.GetObject(typeof(IFilingProcessorRemotable), processorUri);
            }
            catch { }

            if (processor == null)
            {
                errorMsg = "Unable to connect to the Processor";
                return false;
            }

            return true;
        }

        #endregion
    }
}

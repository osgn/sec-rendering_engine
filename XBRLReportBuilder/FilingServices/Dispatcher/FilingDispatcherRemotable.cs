/**************************************************************************
 * FilingDispatcherRemotable (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * The implementation of IFilingDispatcherRemotable for the FilingDispatcher service.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aucent.FilingServices.Data;
using Aucent.FilingServices.Data.Interfaces;

namespace Aucent.FilingServices.Dispatcher
{
	public class FilingDispatcherRemotable : MarshalByRefObject, IFilingDispatcherRemotable
	{
		public FilingDispatcherRemotable()
		{}

		#region IFilingDispatcherRemotable Members

		public void RegisterProcessor(string processorUri, string serverKey)
		{
			FilingDispatcherManager.TheMgr.RegisterProcessor(processorUri, serverKey);
		}

		public void UnRegisterProcessor(string serverKey)
		{
			FilingDispatcherManager.TheMgr.UnRegisterProcessor(serverKey);
		}

        public string[] GetProcessors()
        {
            return FilingDispatcherManager.TheMgr.GetProcessors();
        }

        public string[] GetUnassignedProcessors()
        {
            return FilingDispatcherManager.TheMgr.GetUnAssignedProcessors();
        }

        public bool AssignMarket(string processorServerKey, int marketId)
        {
            return FilingDispatcherManager.TheMgr.AssignMarket(processorServerKey, marketId);
        }

        public bool LoadTaxonomy(string processorServerKey, string taxonomyPath)
        {
            return FilingDispatcherManager.TheMgr.LoadTaxonomy(processorServerKey, taxonomyPath);
        }

        public string GetProcessor(int marketId)
        {
            return FilingDispatcherManager.TheMgr.GetProcessor(marketId);
        }

        public bool CreateBatchDetailExportFile(int marketID, int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile, out string error)
        {
            return FilingDispatcherManager.TheMgr.CreateBatchDetailExportFile(marketID, batchDetailID, xbrlDocumentID, language, fileFormatType,replaceExistingFile, out error);
        }

        public bool CreateBatchExportFiles(int marketID, int batchID, string language, ExportFileType fileFormatType, bool replaceExistingFiles, out string error)
        {
            return FilingDispatcherManager.TheMgr.CreateBatchExportFiles(marketID, batchID, language, fileFormatType, replaceExistingFiles, out error);
        }

		public bool CanProcessorAccessFolder(string processorServerKey, string folderPath, out string errorMsg)
		{
			return FilingDispatcherManager.TheMgr.CanProcessorAccessFolder(processorServerKey, folderPath, out errorMsg);
		}
        
		#endregion
}
}

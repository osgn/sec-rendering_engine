/**************************************************************************
 * FilingProcessorRemotable (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * The implementation of IFilingProcessorRemotable for a FilingProcessor service.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aucent.FilingServices.Data;
using Aucent.FilingServices.Data.Interfaces;

namespace Aucent.FilingServices.FilingProcessorBase
{
	/// <summary>
	/// Defines the remoting interface that is exposed by a Filing Processor service
	/// </summary>
	public class FilingProcessorRemotable : MarshalByRefObject, IFilingProcessorRemotable
	{
		public FilingProcessorRemotable()
		{ }

		#region IFilingProcessorRemotable Members

		//Even though there are no methods in this remoting interface, we are keeping it 
		//defined so that we can add new methods that are certain to come along in the future
        public void AssignMarketToProcessor(int MarketId)
        {
            //
            FilingProcessorManager.TheMgr.LoadMarketInfo(MarketId);
        }

        public bool LoadMarketBaseTaxonomy(string taxonomypath)
        {
           return FilingProcessorManager.TheMgr.LoadMarketBaseTaxonomy(taxonomypath);                
        }

        public void CreateBatchDetailExportFile(int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile)
        {
            FilingProcessorManager.TheMgr.CreateBatchDetailExportFile(batchDetailID, xbrlDocumentID, language, fileFormatType,replaceExistingFile);
        }

		public void CreateBatchExportFiles(int batchMarketID, string language, ExportFileType fileFormatType, bool replaceExistingFiles)
        {
            FilingProcessorManager.TheMgr.CreateBatchExportFiles(batchMarketID, language, fileFormatType, replaceExistingFiles);
        }

		public bool CanAccessFolder(string folderPath, out string errorMsg)
		{
			return FilingProcessorManager.TheMgr.CanAccessFolder(folderPath, out errorMsg);
		}

		#endregion
	}
}

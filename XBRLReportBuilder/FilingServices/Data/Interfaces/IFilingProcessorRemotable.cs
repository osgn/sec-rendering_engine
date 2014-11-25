/**************************************************************************
 * IFilingProcessorRemotable (interface)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines the interface for a class that can be used to process XBRL based data
 * filings.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aucent.FilingServices.Data.Interfaces
{
	/// <summary>
	/// Defines the remoting interface that is exposed by a Filing Processor service
	/// </summary>
	public interface IFilingProcessorRemotable
	{
		//Even though there are no methods in this remoting interface, we are keeping it 
		//defined so that we can add new methods that are certain to come along in the future

        //Method to assign a market for a processor service.
        void AssignMarketToProcessor(int MarketId);

        //Method to load base taxonomies for a market
        bool LoadMarketBaseTaxonomy(string taxonomypath);

        //Method to create export file for an instance document
		void CreateBatchDetailExportFile(int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile);

		void CreateBatchExportFiles(int batchMarketID, string language, ExportFileType fileFormatType, bool replaceExistingFiles);

		bool CanAccessFolder(string folderPath, out string errorMessage);
	}
}

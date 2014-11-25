/**************************************************************************
 * IFilingDispatcherRemotable (interface)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines the methods that are available in a Dispatcher service
 * via remoting.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.FilingServices.Data.Interfaces
{
	public interface IFilingDispatcherRemotable
	{
		void RegisterProcessor(string processorUri, string serverKey);
		void UnRegisterProcessor(string serverKey);

        //The following methods are created for use in Reuters project.
        /// <summary>
        /// Get a list of all registered processors.
        /// </summary>
        /// <returns></returns>
        string[] GetProcessors();

        /// <summary>
        /// Get a list of all unassigned processors.
        /// </summary>
        /// <returns></returns>
        string[] GetUnassignedProcessors();

        string GetProcessor(int marketId);

        /// <summary>
        /// Assign a market to a processor.
        /// </summary>
        /// <param name="processorServerKey"></param>
        /// <param name="marketId"></param>
        /// <returns></returns>
        bool AssignMarket(string processorServerKey, int marketId);

        /// <summary>
        /// Load taxonomy for a market.
        /// </summary>
        /// <param name="processorServerKey"></param>
        /// <param name="taxonomyPath"></param>
        /// <returns></returns>
        bool LoadTaxonomy(string processorServerKey, string taxonomyPath);

        /// <summary>
        /// Create Export file for a instance document
        /// </summary>
        /// <param name="marketID"></param>
        /// <param name="batchDetailID"></param>
        /// <param name="xbrlDocumentID"></param>
        /// <param name="language"></param>
        /// <param name="filePath"></param>
        /// <param name="replaceExistingFile"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool CreateBatchDetailExportFile(int marketID, int batchDetailID, int xbrlDocumentID, string language, ExportFileType fileFormatType, bool replaceExistingFile, out string error);

        /// <summary>
        /// Create export files for an entire batch.
        /// </summary>
        /// <param name="marketID"></param>
        /// <param name="batchID"></param>
        /// <param name="language"></param>
        /// <param name="fileFormatType"></param>
        /// <param name="replaceExistingFiles"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool CreateBatchExportFiles(int marketID, int batchID, string language, ExportFileType fileFormatType, bool replaceExistingFiles, out string error);

		/// <summary>
		/// Determines if the processor with the given name can access the location specified
		/// by folderpath
		/// </summary>
		/// <param name="processorName">The processor that needs to access folderPath.</param>
		/// <param name="folderPath">The path that needs to be accessable.</param>
		/// <param name="errorMsg">The reason why the processor can not access the folderPath.</param>
		/// <returns></returns>
		bool CanProcessorAccessFolder(string processorServerKey, string folderPath, out string errorMsg);

	}
}

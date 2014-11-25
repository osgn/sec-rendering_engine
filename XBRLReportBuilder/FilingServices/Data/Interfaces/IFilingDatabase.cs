/**************************************************************************
 * IFilingDatabase (interface)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines the interface for a FilingProcessor that uses a database on the back end
 * as a means of managing information regarding the processor
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.FilingServices.Data.Interfaces
{
    /// <summary>
    /// Exposes methods for updating database with processed filing information.
    /// This interface is implemented by Processor.
    /// </summary>
    public interface IFilingDatabase
    {
        bool GetAssociatedMarketId(string serverKey,out int marketId);
        bool LoadMarketInfo(int marketId,out string documentSource);
        bool LoadBaseTaxonomy(string path);
        int CreateBatchRecord(int totalFiles);
        void CreateBatchDetailExportFile(int batchDetailID, int xbrlDocumentID, string language,ExportFileType fileFormatType, bool replaceExistingFile);
		void CreateBatchExportFiles(int batchMarketID, string language, ExportFileType fileFormatType, bool replaceExistingFiles);
    }
}

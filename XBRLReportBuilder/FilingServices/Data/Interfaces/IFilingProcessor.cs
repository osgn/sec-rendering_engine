/**************************************************************************
 * IFilingProcessor (interface)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines the methods that are available in a FilingProcessor service
 * via remoting.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Aucent.FilingServices.Data;

namespace Aucent.FilingServices.Data.Interfaces
{
	/// <summary>
	/// Exposes methods for processing a Filing.  This Interface is used by the FilingProcessorManager
	/// and allows us to define different ways to process a filing without having to implement a new
	/// manager
	/// 
	/// </summary>
	public interface IFilingProcessor
	{
		/// <summary>
		/// A WaitCallback that can be called asynchronously or from a thread pool that will
		/// process a filing
		/// </summary>
		/// <param name="objFiling"></param>
		void ProcessFilingCallback(Object objFiling);
		bool HasNewFilings();
        bool TryMoveFilingsToProcessingFolder(out List<FilingInfo> filings);
		bool TryGetDependentFolderConfiguration(out string filingsFolder, out string processingFolder, out string reportsFolder, out string errorMsg);
       
	}
}

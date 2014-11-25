using System;
using System.Collections.Generic;
using System.Text;

using Aucent.FilingServices.Data;

namespace Aucent.XBRLReportBuilder.BuilderService
{
	public class ReportBuilderRemotable : MarshalByRefObject, IFilingProcessorRemotable
	{
		public ReportBuilderRemotable()
		{ }

		#region IFilingProcessorRemotable Members

		public void ProcessFilings(List<FilingInfo> filings)
		{
			ReportBuilderManager.TheMgr.ProcessFilings(filings);
		}

		#endregion


	}
}

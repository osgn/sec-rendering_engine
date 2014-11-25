using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Cache;

namespace Aucent.FilingServices.Data
{
	public enum HtmlReportFormat
	{
		None = 0,
		Complete = 1,
		Fragment = 2
	}

	[Flags]
	public enum ReportFormat
	{
		None = 0,
		Xml = 1,
		Html = 2,
		HtmlAndXml = 3
	}

	public interface IReportBuilder
	{
		//bool BuildReports( string instancePath, string taxonomyPath, string filingSummaryPath, string reportDirectory, string formType, out XBRLReportBuilder.FilingSummary myFilingSummary, out string error );
		string CurrencyMappingFile { get; set; }
		HtmlReportFormat HtmlReportFormat { get; set; }
		RequestCacheLevel RemoteFileCachePolicy { get; set; }
		ReportFormat ReportFormat { get; set; }
		void SetPreferredLanguage( string lang );
		void SetRulesFile( string rulesFileNameIn );
	}
}

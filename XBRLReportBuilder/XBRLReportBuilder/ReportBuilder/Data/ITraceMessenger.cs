using System;
namespace XBRLReportBuilder
{
	public interface ITraceMessenger
	{
		void TraceError( string message );
		void TraceInformation( string message );
		void TraceWarning( string message );

		void PrependLogs( FilingSummary tmpFS );
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace XBRLReportBuilder
{
	public class TraceMessengerVoid : ITraceMessenger
	{
		public void TraceError( string message ){}
		public void TraceInformation( string message ){}
		public void TraceWarning( string message ){}

		public void PrependLogs( FilingSummary tmpFS ) { }
	}
}

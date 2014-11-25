using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace XBRLReportBuilder
{
    //TODO remove partial - this is not a partial
	public partial class FilingSummaryTraceWrapper : TraceListener
	{
		FilingSummary fs = null;

        /// <summary>
        /// Creates a new instance of the trace wrapper with <paramref
        /// name="fs"/> as the base.
        /// </summary>
        /// <param name="fs">The <see cref="FilingSummary"/> to use for
        /// creating the trace wrapper.</param>
		public FilingSummaryTraceWrapper( FilingSummary fs )
		{
			this.fs = fs;
			this.Attributes[ "switchValue" ] = "All";
		}

        /// <summary>
        /// Writes the specified message to the trace log.
        /// </summary>
        /// <param name="message">The message to be written.</param>
		public override void Write( string message )
		{
			this.WriteLine( message );
		}

		private TraceLevel type = TraceLevel.Info;

        /// <summary>
        /// Writes the specified line to the trace log, processing the entry to
        /// determine the trace level.
        /// </summary>
        /// <param name="message">The message to be written.</param>
		public override void WriteLine( string message )
		{
			if( string.IsNullOrEmpty( message ) )
				return;

			if( this.fs == null )
				return;

			if( message.EndsWith( "Information: 0 : " ) )
			{
				this.type = TraceLevel.Info;
				return;
			}
			else if( message.EndsWith( "Warning: 0 : " ) )
			{
				this.type = TraceLevel.Warning;
				return;
			}
			else if( message.EndsWith( "Error: 0 : " ) )
			{
				this.type = TraceLevel.Error;
				return;
			}

			lock( this.fs.Logs )
			{
				LogItem log = new LogItem( this.type, message );
				this.fs.Logs.Add( log );
			}
		}
	}
}

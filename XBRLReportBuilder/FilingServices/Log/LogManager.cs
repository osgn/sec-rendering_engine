/*****************************************************************************
 * LogManager (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * A static class that can be used to simplify the interaction with the Windows Event Log
 * The class will write entries to the Event Log based on configuration settings in the
 * application that is using this class.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace Aucent.FilingServices.Log
{
    public sealed class LogManager
    {
        #region Properties

        private EventLog myEventLog = null;

        private ExcludeFlag excludeFlags;

        #endregion

        #region Constructor

        private LogManager() 
        {
            LogSection customSection = ConfigurationManager.GetSection("LogSection") as LogSection;
            if (customSection == null)
                customSection = new LogSection();

			try
			{
				this.excludeFlags = customSection.Exclude;

				string source = customSection.SourceName;
				string logName = customSection.LogName;

				myEventLog = new EventLog();
				myEventLog.Log = logName;
				myEventLog.Source = source;

				if( EventLog.SourceExists( source ) )
					EventLog.DeleteEventSource( source );

				if( !System.Diagnostics.EventLog.SourceExists( source ) )
					System.Diagnostics.EventLog.CreateEventSource( source, logName );

				if( myEventLog.OverflowAction != OverflowAction.OverwriteAsNeeded )
					myEventLog.ModifyOverflowPolicy( OverflowAction.OverwriteAsNeeded, 0 );
			}
			catch( Exception ex )
			{
				Trace.TraceError( ex.Message + " at " + ex.Source ); 
			}
        }

        #endregion

        #region Singleton Instance

        private static LogManager theMgr = null;
        public static LogManager TheMgr
        {
            get
            {
                if (theMgr == null)
                {
                    theMgr = new LogManager();
                }
                return theMgr;
            }
        }


        #endregion

        #region Methods

		public void WriteInformation( string msg )
		{
			this.WriteInformation( msg, EventLogEntryType.Information );
		}

        public void WriteInformation(string msg, EventLogEntryType entryType)
        {
			string entry = msg;

			try
			{
				if( !IsExcluded( ExcludeFlag.Info ) )
					myEventLog.WriteEntry( entry, entryType );
			}
			catch( Exception ex )
			{
				Trace.TraceError( "Log entry failed: '" + entry + "'" + Environment.NewLine + "Reason: " + ex.Message );
			}
        }

        private void WriteException(string msg, Exception ex)
        {
			this.WriteInformation( msg + " Exception:" + ex.Message, EventLogEntryType.Error );
        }

        public void WriteException(string msg, Exception ex,bool bIncludeStackTrace)
        {
            if (!IsExcluded(ExcludeFlag.Error))
            {
                if (bIncludeStackTrace)
					this.WriteInformation( msg + " Exception:" + ex.Message + " Stack trace:" + ex.StackTrace );
                else
                    this.WriteException(msg, ex);
            }
        }

        public void WriteError(string msg)
        {
            if (!IsExcluded(ExcludeFlag.Error))
				this.WriteInformation( msg, EventLogEntryType.Error );
        }

        public void WriteWarning(string msg)
        {
            if (!IsExcluded(ExcludeFlag.Warning))
                this.WriteInformation(msg, EventLogEntryType.Warning);
        }

        private bool IsExcluded(ExcludeFlag flag)
        {
            if (excludeFlags == ExcludeFlag.None)
                return false;

            if (excludeFlags == ExcludeFlag.All || ((excludeFlags & flag) == flag))
                return true;
            else
                return false;

        }

        #endregion
    }
}

// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// AucentCustomEventLogger
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class RivetCustomEventLogSource
	{
		private	string	eventLogName	= string.Empty;
		private	string	eventSourceName = string.Empty;
		
		#region properties
		public string RivetEventLogName
		{
			get {return eventLogName;}
		}
		public string RivetEventSourceName
		{
			get {return eventSourceName;}
		}
		#endregion

		#region constructors
		static List<string> eventLogsUpdated = new List<string>();
			
		/// <summary>
		/// Creates a new AucentCustomEventLogSource.  Pass in your TaxonomyFileName (without extension) to create your log.
		/// </summary>
		public RivetCustomEventLogSource(string EventLogTitle, string EventSourceName, string TargetMachineName)
		{
			try
			{
				if (TargetMachineName == string.Empty || TargetMachineName == null)
					TargetMachineName = Environment.MachineName;
			
				eventLogName = EventLogTitle; 
			
				if(EventLog.SourceExists(EventSourceName, TargetMachineName))
					// Are we dealing with the same log for the source?
					if (EventLogTitle.Trim() != EventLog.LogNameFromSourceName(EventSourceName, TargetMachineName).Trim())
						EventLog.DeleteEventSource(EventSourceName, TargetMachineName);
				
				// Create EventLog if needed
				if(!EventLog.Exists(EventLogTitle, TargetMachineName))
				{
					if (EventLog.SourceExists(EventLogTitle, TargetMachineName))
					{
						// If an event source exists in the name of the EventLog, we should remove the 
						// event source
						try
						{
							EventLog.DeleteEventSource(EventLogTitle, TargetMachineName);
						}
						catch
						{
							// If we had an issue with deleting the EventSource because it matches some EventLog title,
							// we have no choice but to remove the EventLog alltogether!
							EventLog.Delete(EventLogTitle, TargetMachineName);
						}
					}
					EventSourceCreationData dataSource = new EventSourceCreationData(EventSourceName, EventLogTitle);
					dataSource.MachineName = TargetMachineName;
					EventLog.CreateEventSource(dataSource);
				}
				// Create the event source if it does not exist
				if (!EventLog.SourceExists(EventSourceName, TargetMachineName))
				{
					EventSourceCreationData dataSource = new EventSourceCreationData(EventSourceName, EventLogTitle);
					dataSource.MachineName = TargetMachineName;
					EventLog.CreateEventSource(dataSource);
				}
				if (!eventLogsUpdated.Contains(EventLogTitle) )
				{
					EventLog myLog = new EventLog(EventLogTitle, TargetMachineName);
					if (myLog.OverflowAction != OverflowAction.OverwriteAsNeeded)
					{
						myLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
					}

					eventLogsUpdated.Add(EventLogTitle);
				}

			}
			catch(System.Exception ex)
			{
				string msg = ex.Message;
			}
		}

		#endregion

		#region public methods

        ///// <summary>
        ///// Delete the named EventLog
        ///// </summary>
        ///// <param name="EventLogName"></param>
        ///// <param name="TargetMachineName"></param>
        ///// <returns></returns>
        //public bool DeleteRivetEventLog(string EventLogName, string TargetMachineName)
        //{
        //    bool retVal = true;
        //    try
        //    {
        //        if (TargetMachineName == string.Empty || TargetMachineName == null)
        //            TargetMachineName = Environment.MachineName;
			
        //        eventLogName = EventLogName; 
			
        //        if (EventLog.Exists(EventLogName, TargetMachineName))
        //            EventLog.Delete(EventLogName, TargetMachineName);
        //    }
        //    catch (Exception)
        //    {
        //        retVal = false;
        //    }
        //    return retVal;
        //}

        ///// <summary>
        ///// Remove an associate source with this named event log;
        ///// </summary>
        ///// <param name="EventSource"></param>
        ///// <param name="EventLogName"></param>
        ///// <param name="TargetMachineName"></param>
        ///// <returns></returns>
        //public bool DeleteRivetEventSource(string EventSource, string EventLogName, string TargetMachineName)
        //{
        //    bool retVal = true;
        //    try
        //    {
        //        if (TargetMachineName == string.Empty || TargetMachineName == null)
        //            TargetMachineName = Environment.MachineName;
			
        //        eventSourceName = EventSource; 
        //        if (EventLog.SourceExists(EventSource, TargetMachineName))
        //            EventLog.DeleteEventSource(EventSource, TargetMachineName);
        //    }
        //    catch (Exception)
        //    {
        //        retVal = false;
        //    }
        //    return retVal;
        //}

		/// <summary>
		/// Write an entry into the currently associated event log;
		/// </summary>
		/// <param name="Message"></param>
		/// <param name="EntryType"></param>
		/// <returns></returns>
		public bool RivetWriteEntry(string EventSource, string Message, System.Diagnostics.EventLogEntryType EntryType)
		{
			bool retVal = true;
			try
			{
				if (RivetEventLogName != string.Empty)
					EventLog.WriteEntry(EventSource, Message, EntryType);
			}
			catch
			{
				try
				{
					// Clear the log, but keep the last 100 entries.
					retVal = false;
					EventLog myLog = new EventLog();
					myLog.Log = EventLog.LogNameFromSourceName(EventSource, Environment.MachineName);
					ArrayList mySavedEntries = new ArrayList();
					for (int i=0; i < myLog.Entries.Count; i++)
					{
						if ( (myLog.Entries.Count - i) > 100)
							continue;
						mySavedEntries.Add(myLog.Entries[i]);
					}
			
					// Clear the log (removes all entries);
					myLog.Clear();
			
					if (mySavedEntries != null )
					{
						for (int i = 0; i< mySavedEntries.Count; i++)
						{
							EventLogEntry le = (EventLogEntry) mySavedEntries[i];
							EventLog.WriteEntry(le.Source.ToString(), le.Message.ToString(), le.EntryType);
						}
					}

					// Finally, write our original message which could not fit!
					if (RivetEventLogName != string.Empty)
						EventLog.WriteEntry(EventSource, Message,EntryType);

					retVal = true;
				}
				catch
				{
					//Do nothing.  Couldn't write to log, possibly because of restricted permissions.
				}
			}
			
			return retVal;
		}

        ///// <summary>
        ///// Get all the event log entries associated with a particular EventSource
        ///// </summary>
        ///// <param name="EventSource"></param>
        ///// <returns></returns>
        //public ArrayList RivetGetLogEntries(string EventSource)
        //{
        //    ArrayList EventLogMessages = new ArrayList();
        //    try
        //    {
        //        EventLog myLog = new EventLog();
        //        myLog.Log = RivetEventLogName;
        //        if (myLog.Entries.Count > 0)
        //        {
        //            foreach(EventLogEntry entry in myLog.Entries)
        //            {
        //                if (entry.Source == EventSource)
        //                    EventLogMessages.Add(entry);
        //            }
        //        }
        //    }
        //    catch (Exception )
        //    {
        //        //EventLogMessages = null;
        //        //throw (new AucentException("AucentCustomEventLogSource.WriteCustomEntry.Info" ,e.Message));
        //    }
        //    return EventLogMessages;
        //}

		#endregion
		
	}
}
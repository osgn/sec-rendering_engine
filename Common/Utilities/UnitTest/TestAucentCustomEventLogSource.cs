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
#if UNITTEST
namespace Aucent.MAX.AXE.Common.Utilities.Test
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;
    using Aucent.MAX.AXE.Common.Utilities;

	/// <exclude/>
	[TestFixture]
	public class TestRivetCustomEventLogger
    {
        string Custom_EventLog = Utilities.AucentGeneral.RIVET_EVENT_LOG;
		string Custom_Source = Utilities.AucentGeneral.PRODUCT_NAME;
        string Sample_ErrMsg = "Rivet 'test' event log entry." + Environment.TickCount.ToString();

        #region init

		/// <exclude/>
		[TestFixtureSetUp]
        public void RunFirst()
        {
            if (System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName))
                System.Diagnostics.EventLog.Delete(Custom_EventLog, Environment.MachineName);
			Assert.IsFalse(System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName));
		}

		/// <exclude/>
		[TestFixtureTearDown]
        public void RunLast()
        {
			if (System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName))
                System.Diagnostics.EventLog.Delete(Custom_EventLog, Environment.MachineName);
			Assert.IsFalse(System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName));
        }

		/// <exclude/>
		[SetUp]
        public void RunBeforeEachTest()
        { }

		/// <exclude/>
		[TearDown]
        public void RunAfterEachTest()
        { }

        #endregion

		/// <exclude/>
		public TestRivetCustomEventLogger()
        {
        }

		/// <exclude/>
		[Test]
        public void Test_CreateRivetCustomEventLog()
        {
            RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
            Assert.IsTrue(RivetLog.RivetWriteEntry(Custom_Source, Sample_ErrMsg, EventLogEntryType.Error));
            Assert.IsTrue(System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName));
            EventLog myLog = new EventLog();
            myLog.Log = RivetLog.RivetEventLogName;
            Assert.IsTrue(myLog.Entries.Count > 0);
            myLog.Clear();
            Assert.IsTrue(myLog.Entries.Count == 0);
            if (System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName))
                System.Diagnostics.EventLog.Delete(Custom_EventLog, Environment.MachineName);
            Assert.IsFalse(System.Diagnostics.EventLog.Exists(Custom_EventLog, Environment.MachineName));
        }

		/// <exclude/>
		[Test]
        public void Test_RivetWriteEntry()
        {
            RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
            Assert.IsTrue(RivetLog.RivetWriteEntry(Custom_Source, Sample_ErrMsg, System.Diagnostics.EventLogEntryType.Error));
        }

		/// <exclude/>
		[Test]
        public void Test_DeleteRivetEventLog()
        {
            RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
            Assert.IsTrue(RivetLog.DeleteRivetEventLog(Custom_EventLog, string.Empty));
        }

		/// <exclude/>
		[Test]
        public void Test_ReadEventLogName()
        {
            RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
            EventLog myLog = new EventLog();
            myLog.Source = RivetLog.RivetEventLogName;
            Assert.IsTrue(myLog.LogDisplayName == RivetLog.RivetEventLogName);
        }

		/// <exclude/>
		[Test]
        public void Test_ReadEventLogEntrySourceName()
        {
			RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
			EventLog myLog = new EventLog();
			myLog.Source = RivetLog.RivetEventLogName;

            for (int i = 0; i < 10; i++)
                myLog.WriteEntry(Sample_ErrMsg, EventLogEntryType.Error, 1, 1);
            Assert.IsTrue(myLog.Entries.Count > 0);
            foreach (EventLogEntry entry in myLog.Entries)
                Assert.IsTrue(entry.Source == Custom_EventLog);
        }

		/// <exclude/>
		[Test, Ignore]
        public void Test_OverflowingEventLog()
        {
        

            RivetCustomEventLogSource RivetLog = new RivetCustomEventLogSource(Custom_EventLog, Custom_Source, string.Empty);
            RivetLog.RivetWriteEntry(Custom_Source, Sample_ErrMsg, System.Diagnostics.EventLogEntryType.Error);

            // Overflow the event log...
            EventLog myLog = new EventLog();
			myLog.Source = RivetLog.RivetEventLogName;
			myLog.Log = EventLog.LogNameFromSourceName(this.Custom_Source, Environment.MachineName);


            Console.WriteLine("Processing Event Log : " + myLog.LogDisplayName.ToString());

            Console.WriteLine("___________________________________________");

            // Clear the log;
            myLog.Clear();

            Console.WriteLine("Cleared eventlog. Current Log Entry Count : " + myLog.Entries.Count.ToString());

            // Get 102 entries into the log.
            for (int i = 0; i < 102; i++)
            {
                RivetLog.RivetWriteEntry(Custom_Source, Sample_ErrMsg + " " + Environment.TickCount.ToString()
                    , System.Diagnostics.EventLogEntryType.Information);
            }

            Console.WriteLine("Wrote 102 entries. Current Log Entry Count : " + myLog.Entries.Count.ToString());
            // Now write to the event log till we have 101 entries.
            // Which means that we've cleared the log once and have rewritten to it again, up to this message, 
            //  expecting 101 final log entries.


            while (myLog.Entries.Count != 101)
            {
                RivetLog.RivetWriteEntry(Custom_Source, Sample_ErrMsg + " " + Environment.TickCount.ToString()
                    , System.Diagnostics.EventLogEntryType.Information);
                Console.WriteLine("Current Log Entry Count : " + myLog.Entries.Count.ToString());
            }

            // our log should now have only 101.  (100 of the latest entries, plus our current one. 
            Console.WriteLine("Expecting 101 log entries (last saved 100 entries + the current message). Current Count : " + myLog.Entries.Count.ToString());
            Assert.IsTrue(myLog.Entries.Count == 101);

            Console.WriteLine("___________________________________________");

        }
    }
}
#endif
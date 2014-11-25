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
namespace Aucent.MAX.AXE.XBRLParser.Test_Taxonomy
{
	using System;
    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;
	using System.Collections;
	using System.Diagnostics;
	using Aucent.MAX.AXE.XBRLParser.Test;

	/// <exclude/>
	[TestFixture] 
    public class TestReference_2004_08_15 : Reference
    {
		string PT_REFERENCE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-08-15-reference.xml";
		string USFR_SEC_CERT_REFERENCE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"usfr-sec-cert-2004-08-15-references.xml";

#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestReference Comments***" );
			
			//TODO: Add this line back in to see data written
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {
			Console.WriteLine( "***End TestReference Comments***" );
		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
        {}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
        {}

#endregion

		#region Test PT References

		/// <exclude/>
		public void TestGetReferenceData()
		{
			TestReference_2004_08_15 testRef = new TestReference_2004_08_15();

			testRef.Load( PT_REFERENCE );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse PT references took {0}", end-start );

			Assert.AreEqual( 0, errors );
            Assert.AreEqual(582, testRef.ReferencesTable.Count);
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		/// <exclude/>
		public void TestGetReferenceData_SEC_CERT()
		{
			TestReference_2004_08_15 testRef = new TestReference_2004_08_15();

			testRef.Load( USFR_SEC_CERT_REFERENCE );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse SEC CERT references took {0}", end-start );

			Assert.AreEqual( 0, errors );
            Assert.AreEqual(30, testRef.ReferencesTable.Count);
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		#endregion
    }
}
#endif
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
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;
	using System.Collections;
	using System.Diagnostics;

	/// <exclude/>
	[TestFixture] 
    public class TestReference : Reference
    {
		string PT_REFERENCE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-06-15-reference.xml";
		string PT_REFERENCE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-07-06-reference.xml";
		string US_GAAP_INS_REFERENCE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ins-2004-06-15-reference.xml";
		string US_GAAP_INS_REFERENCE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ins-2004-07-06-reference.xml";
		string FAS_REFERENCE = TestCommon.FolderRoot + @"fas\us-fas132-2003-12-01-reference.xml";
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
		[Test]
		public void TestGetReferenceData_PT_06_15_2004()
		{
			TestReference testRef = new TestReference();

			testRef.Load( PT_REFERENCE_06_15 );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse PT references took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 530, testRef.references.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		/// <exclude/>
		[Test]
		public void TestGetReferenceData_PT_07_06_2004()
		{
			TestReference testRef = new TestReference();

			testRef.Load( PT_REFERENCE_07_06 );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse PT references took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 527, testRef.references.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		/// <exclude/>
		[Test]
		public void TestGetReferenceData_INS_07_06_2004()
		{
			TestReference testRef = new TestReference();

			testRef.Load( US_GAAP_INS_REFERENCE_07_06 );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse INS references took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 110, testRef.references.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		/// <exclude/>
		[Test]
		public void TestGetReferenceData_INS_06_15_2004()
		{
			TestReference testRef = new TestReference();

			testRef.Load( US_GAAP_INS_REFERENCE_06_15 );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse INS references took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 110, testRef.references.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		/// <exclude/>
		[Test]
		public void TestGetReferenceData_FAS()
		{
			TestReference testRef = new TestReference();

			testRef.Load( FAS_REFERENCE );

			int errors = 0;
			DateTime start = DateTime.Now;
			testRef.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse FAS references took {0}", end-start );

			Assert.AreEqual( 1, errors , "Expected 1 error to come back... but that was not the case.");
			Assert.AreEqual( 66, testRef.references.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse references takes too long" );
		}

		#endregion
    }
}
#endif
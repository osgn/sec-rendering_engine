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
	using System.Collections;
	using System.Diagnostics;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestLabel : Label
    {
		#region Overrides
//		protected void TestParse( out int errors )
//		{
//			errors = 0;
//			ParseInternal( out errors );
//		}
		#endregion

		string PT_GAAP_FILE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-06-15-label.xml";
		string PT_GAAP_FILE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-07-06-label.xml";
		string US_GAAP_FILE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-06-15-label.xml";
		string US_GAAP_FILE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-07-06-label.xml";
		string US_GAAP2_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-06-15-label-test.xml";
		//string SAG_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\SoftwareAG-Spain-2004-01-30\SoftwareAG-2004-01-30.xml";
	 string PT_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-usfr-pt.xml";

		#region init

	 /// <exclude/>
	 [TestFixtureSetUp]
	 public void RunFirst()
        {
			Console.WriteLine( "***Start TestLabel Comments***" );
			
			//TODO: Add this line back in to see data written
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {
			Console.WriteLine( "***End TestLabel Comments***" );
		}

		///<exclude/>
        [SetUp] public void RunBeforeEachTest()
        {}

        ///<exclude/>
        [TearDown] public void RunAfterEachTest() 
        {}

		#endregion

		#region GAAP
		/// <exclude/>
		[Test]
		public void GAAP_GetLabelData_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_06_15 );

			int errors = 0;
			tl.ParseInternal( out errors );			
			Assert.AreEqual( 0, errors, "wrong number of errors returned" );
            Assert.AreEqual(4, tl.LabelTable.Count);

            LabelLocator ll = tl.LabelTable["usfr-pt_ChangeInBalances"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_ChangeInBalances is null" );
			string info = null;
			
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_ChangeInBalances" );
			Assert.AreEqual( "Statement of Cash Flows", info, "Statement of Cash Flows incorrect" );

			ll = tl.labels["usfr-pt_FinancialAccountingConcepts"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_FinancialAccountingConcepts is null" );
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_FinancialAccountingConcepts" );
			Assert.AreEqual( "Financial Accounting Concepts", info, "Financial Accounting Concepts incorrect" );

			ll = tl.labels["usfr-pt_GeneralConcepts"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_GeneralConcepts is null" );
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_GeneralConcepts" );
			Assert.AreEqual( "US GAAP - Commercial and Industrial", info, "US GAAP - Commercial and Industrial incorrect" );

			ll = tl.labels["usfr-pt_NonOperatingIncomeExpenseAbstract"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_NonOperatingIncomeExpenseAbstract is null" );
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_NonOperatingIncomeExpenseAbstract" );
			Assert.AreEqual( "Nonoperating Income:", info, "Nonoperating Income: incorrect" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetLabelData_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_07_06 );
			int errors = 0;
			tl.ParseInternal( out errors );			
			Assert.AreEqual( 0, errors, "wrong number of errors returned" );
			Assert.AreEqual( 2, tl.labels.Count );

			string info = null;

			LabelLocator ll = tl.labels["usfr-pt_FinancialAccountingConcepts"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_FinancialAccountingConcepts is null" );
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_FinancialAccountingConcepts" );
			Assert.AreEqual( "Financial Accounting Concepts", info, "Financial Accounting Concepts incorrect" );

			ll = tl.labels["usfr-pt_GeneralConcepts"] as LabelLocator;
			Assert.IsNotNull( ll, "usfr-pt_GeneralConcepts is null" );
			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_GeneralConcepts" );
			Assert.AreEqual( "US GAAP - Commercial and Industrial", info, "US GAAP - Commercial and Industrial incorrect" );

//			ll = tl.labels["usfr-pt_NonOperatingIncomeExpenseAbstract"] as LabelLocator;
//			Assert.IsNotNull( ll, "usfr-pt_NonOperatingIncomeExpenseAbstract is null" );
//			Assert.IsTrue( ll.TryGetInfo( "en", "label", out info ), "tryGetInfo failed for usfr-pt_NonOperatingIncomeExpenseAbstract" );
//			Assert.AreEqual( "Nonoperating Income:", info, "Nonoperating Income: incorrect" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetLabelRoles_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_06_15 );
			
			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList roles = tl.LabelRoles;

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, roles.Count );
			Assert.AreEqual( "label", roles[0] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetLabelRoles_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_07_06);
			
			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList roles = tl.labelRoles;

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, roles.Count );
			Assert.AreEqual( "label", roles[0] );
		}

		/// <exclude/>
		[Test]
		public void GAAP2_GetSupportedLanguages()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP2_FILE );

			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList temp = tl.supportedLanguages;
		
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 4, temp.Count, "wrong number of languages returned" );
			Assert.AreEqual( "aa", temp[0] );
			Assert.AreEqual( "cc", temp[1] );
			Assert.AreEqual( "en", temp[2] );
			Assert.AreEqual( "zz", temp[3] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetSupportedLanguages_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_06_15 );

			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList temp = tl.supportedLanguages;
		
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, temp.Count, "wrong number of languages returned" );
			Assert.AreEqual( "en", temp[0] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetSupportedLanguages_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( US_GAAP_FILE_07_06 );

			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList temp = tl.supportedLanguages;
		
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, temp.Count, "wrong number of languages returned" );
			Assert.AreEqual( "en", temp[0] );
		}
		#endregion

		#region PT
		/// <exclude/>
		[Test]
		public void PT_GetLabelData2_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_06_15 );

			int errors = 0;
			DateTime start = DateTime.Now;
			tl.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse PT label2 took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1478, tl.labels.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse labels takes too long" );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLabelData2_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_07_06 );

			int errors = 0;
			DateTime start = DateTime.Now;
			tl.ParseInternal( out errors );
			DateTime end = DateTime.Now;

			Console.WriteLine( "Parse PT label2 took {0}", end-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1556, tl.labels.Count );
			TimeSpan limit = new TimeSpan( 0, 0, 0, 0, 750 );	// allow 750 millisecondstwo seconds to load
			Assert.IsTrue( limit > (end-start), "Parse labels takes too long" );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLabelData_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_07_06 );

			int errors = 0;
			DateTime start = DateTime.Now;
			tl.ParseInternal( out errors );
			Console.WriteLine( "Parse PT label 07-06 file took {0}", DateTime.Now-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1556, tl.labels.Count );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLabelData_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_06_15 );

			int errors = 0;
			DateTime start = DateTime.Now;
			tl.ParseInternal( out errors );
			Console.WriteLine( "Parse PT label 06-15 file took {0}", DateTime.Now-start );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1478, tl.labels.Count );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLabelRoles_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_06_15 );
			
			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList roles = tl.labelRoles;

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 6, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[2] );
			Assert.AreEqual( "periodStartLabel", roles[3] );
			Assert.AreEqual( "terseLabel", roles[4] );
			Assert.AreEqual( "totalLabel", roles[5] );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLabelRoles_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_07_06 );
			
			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList roles = tl.labelRoles;

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 6, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[2] );
			Assert.AreEqual( "periodStartLabel", roles[3] );
			Assert.AreEqual( "terseLabel", roles[4] );
			Assert.AreEqual( "totalLabel", roles[5] );
		}

		/// <exclude/>
		[Test]
		public void PT_GetSupportedLanguages_07_06()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_07_06 );

			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList temp = tl.supportedLanguages;
		
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, temp.Count, "wrong number of languages returned" );
			Assert.AreEqual( "en", temp[0] );
		}

		/// <exclude/>
		[Test]
		public void PT_GetSupportedLanguages_06_15()
		{
			TestLabel tl = new TestLabel();

			tl.Load( PT_GAAP_FILE_06_15 );

			int errors = 0;
			tl.ParseInternal(out errors);

			ArrayList temp = tl.supportedLanguages;
		
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, temp.Count, "wrong number of languages returned" );
			Assert.AreEqual( "en", temp[0] );
		}

		#endregion
	}
}
#endif
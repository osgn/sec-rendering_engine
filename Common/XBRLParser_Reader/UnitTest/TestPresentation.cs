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
	using System.IO;
	using System.Xml;
	using System.Diagnostics;
	using System.Collections;

    using NUnit.Framework;

	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestPresentation : Presentation
    {
		/// <exclude/>
		public XmlNamespaceManager TheManager
		{
			get { return theManager; }
		}


		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {
			Console.WriteLine( "***Start TestPresentation Comments***" );
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {
			Console.WriteLine( "***End TestPresentation Comments***" );
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

		 string BASI_PRESENTATION_FILE_08_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-basi-2004-08-15-presentation.xml";
		 string GAAP_PRESENTATION_FILE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-06-15-presentation.xml";
		 string GAAP_PRESENTATION_FILE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-07-06-presentation.xml";
		 string PT_PRESENTATION_FILE_06_15 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-06-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-06-15-presentation.xml";
		 string PT_PRESENTATION_FILE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-07-06-presentation.xml";
		 string PT_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-usfr-pt-presentation.xml";

		 string GAAP_CI_CALCULATION_FILE_07_06 = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-07-06-calculation.xml";

		#region 06 15 Presentation
		 /// <exclude/>
		 [Test]
		 public void LoadPresentation_06_15()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_06_15, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}
		}

		/// <exclude/>
		[Test]
		public void Test_RoleRefs_06_15()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_06_15, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0]);
			}

			int numErrors = 0;
			tp.LoadRoleRefs( out numErrors );

			Assert.AreEqual( 0, numErrors );
			Assert.IsNotNull( tp.roleRefs );
			Assert.AreEqual( 5, tp.roleRefs.Count );

			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );

			RoleRef rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity"] as RoleRef;
			TestRoleRef rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd#StatementStockholdersEquity", rt.Href );
			Assert.AreEqual( "StatementStockholdersEquity", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd#StatementCashFlowsDirect", rt.Href );
			Assert.AreEqual( "StatementCashFlowsDirect", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd#StatementCashFlowsIndirect", rt.Href );
			Assert.AreEqual( "StatementCashFlowsIndirect", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd#IncomeStatement", rt.Href );
			Assert.AreEqual( "IncomeStatement", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd#StatementFinancialPosition", rt.Href );
			Assert.AreEqual( "StatementFinancialPosition", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-06-15.xsd", rt.GetSchemaName() );

		}

		/// <exclude/>
		[Test]
		public void Test_VerifyRoleRefs_06_15()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_06_15, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}

			int numErrors = 0;
			tp.LoadRoleRefs( out numErrors );
		
			Assert.AreEqual( 0, numErrors );
			Assert.IsNotNull( tp.roleRefs );
			Assert.AreEqual( 5, tp.roleRefs.Count );

			Assert.IsTrue(  tp.VerifyRoleReference( "us-gaap-ci-2004-06-15.xsd", "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );
			Assert.IsFalse( tp.VerifyRoleReference( "aaa", "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );
			Assert.IsFalse( tp.VerifyRoleReference( "us-gaap-ci-2004-06-15.xsd", "bbb" ) );
		}

	
		#endregion

		#region 07 06 Presentation
		/// <exclude/>
		[Test]
		public void LoadPresentation_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}
		}

		/// <exclude/>
		[Test]
		public void Test_Presentation_RoleRefs_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0]);
			}

			int numErrors = 0;
			tp.LoadRoleRefs( out numErrors );

			Assert.AreEqual( 0, numErrors );
			Assert.IsNotNull( tp.roleRefs );
			Assert.AreEqual( 6, tp.roleRefs.Count );

			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/Notes" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );

			RoleRef rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity"] as RoleRef;
			TestRoleRef rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd#StatementStockholdersEquity", rt.Href );
			Assert.AreEqual( "StatementStockholdersEquity", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd#StatementCashFlowsDirect", rt.Href );
			Assert.AreEqual( "StatementCashFlowsDirect", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd#StatementCashFlowsIndirect", rt.Href );
			Assert.AreEqual( "StatementCashFlowsIndirect", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd#IncomeStatement", rt.Href );
			Assert.AreEqual( "IncomeStatement", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", rt.GetSchemaName() );

			//http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition
			rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition"] as RoleRef;
			rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition", rt.Uri );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd#StatementFinancialPosition", rt.Href );
			Assert.AreEqual( "StatementFinancialPosition", rt.GetId() );
			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", rt.GetSchemaName() );

		}

		/// <exclude/>
		[Test]
		public void Test_VerifyRoleRefs_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}

			int numErrors = 0;
			tp.LoadRoleRefs( out numErrors );
		
			Assert.AreEqual( 0, numErrors );
			Assert.IsNotNull( tp.roleRefs );
			Assert.AreEqual( 6, tp.roleRefs.Count );

			Assert.IsTrue(  tp.VerifyRoleReference( "us-gaap-ci-2004-07-06.xsd", "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );
			Assert.IsFalse( tp.VerifyRoleReference( "aaa", "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );
			Assert.IsFalse( tp.VerifyRoleReference( "us-gaap-ci-2004-07-06.xsd", "bbb" ) );
		}

		/// <exclude/>
		[Test]
		public void LoadPresentationLinks_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			DateTime start = DateTime.Now;
			Console.WriteLine( "Reading file: {0}", GAAP_PRESENTATION_FILE_07_06 );

			int errors = 0;
			if ( tp.Load( GAAP_PRESENTATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}

			Assert.AreEqual( "http://www.xbrl.org/2003/linkbase", tp.theManager.DefaultNamespace );

			errors = 0;
			int nodes = tp.LoadLinks( out errors );
			
			Assert.AreEqual( 0, errors, "num errors wrong" );
			Assert.AreEqual( 0, numWarnings, "num warnings wrong" );
			Assert.AreEqual( 9, nodes, "Wrong number of nodes loaded" );

			
			DateTime end = DateTime.Now;

			Console.WriteLine( "Processing complete. Time elapsed: {0}", (end-start).ToString() );
			//Console.WriteLine( tp.ToXmlString() );
		}

		/// <exclude/>
		[Test]
		[Ignore("Use the one in TestTaxonomy")]
		public void Output_USFR_PT_Presentation_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Presentation;

			DateTime start = DateTime.Now;
			int errors = 0;
			Console.WriteLine( "Reading file: {0}", PT_PRESENTATION_FILE_07_06 );

			if ( tp.Load( PT_PRESENTATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}

			Assert.AreEqual( "http://www.xbrl.org/2003/linkbase", tp.theManager.DefaultNamespace );

			errors = 0;
			int nodes = tp.LoadLinks( out errors );
			
			Assert.AreEqual( 4, errors );
			Assert.AreEqual( 3, nodes, "Wrong number of nodes loaded" );

			DateTime end = DateTime.Now;

			Console.WriteLine( "Processing complete. Time elapsed: {0}", (end-start).ToString() );

			using ( StreamWriter sw = new StreamWriter( PT_OUT_FILE ) )
			{
				sw.Write( tp.ToXmlString() );
			}
		}
		#endregion

		#region 07 06 Calculation

		/// <exclude/>
		[Test]
		public void LoadCalculation_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Calculation;

			int errors = 0;
			if ( tp.Load( GAAP_CI_CALCULATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}
		}

		/// <exclude/>
		[Test]
		public void LoadCalculationRoleRefs_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Calculation;

			int errors = 0;
			if ( tp.Load( GAAP_CI_CALCULATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0]);
			}

			int numErrors = 0;
			tp.LoadRoleRefs( out numErrors );

			Assert.AreEqual( 0, numErrors );
			Assert.IsNotNull( tp.roleRefs );
			Assert.AreEqual( 6, tp.roleRefs.Count );

			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/TotalStockholdersEquityCalculation" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement" ) );
			Assert.IsTrue( tp.roleRefs.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );

			RoleRef rr = tp.roleRefs["http://www.xbrl.org/taxonomy/us/fr/gaap/role/TotalStockholdersEquityCalculation"] as RoleRef;
			TestRoleRef rt = new TestRoleRef( rr );

			Assert.IsNotNull( rt );
		}

		/// <exclude/>
		[Test] 
		public void LoadCalculationLinks_07_06()
		{
			TestPresentation tp = new TestPresentation();
			tp.ProcessingPresenationType = PreseantationTypeCode.Calculation;

			DateTime start = DateTime.Now;
			Console.WriteLine( "Reading file: {0}", GAAP_CI_CALCULATION_FILE_07_06 );

			int errors = 0;
			if ( tp.Load( GAAP_CI_CALCULATION_FILE_07_06, out errors ) == false )
			{
				Assert.Fail( (string)tp.ErrorList[0] );
			}

			Assert.AreEqual( "http://www.xbrl.org/2003/linkbase", tp.theManager.DefaultNamespace );

			errors = 0;
			int nodes = tp.LoadLinks( out errors );
			
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 6, nodes, "Wrong number of calculation nodes loaded" );

			

			DateTime end = DateTime.Now;

			Console.WriteLine( "Processing complete. Time elapsed: {0}", (end-start).ToString() );
			//Console.WriteLine( tp.ToXmlString() );
		}
		
		#endregion
	}
}
#endif
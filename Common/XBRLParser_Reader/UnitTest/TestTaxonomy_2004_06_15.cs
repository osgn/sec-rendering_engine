//=============================================================================
// Schema (class)
// Aucent Corporation
//=============================================================================

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test_Taxonomy
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Collections;
	using System.Diagnostics;

    using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.Common.Data;

    [TestFixture] [Ignore( "Obsolete" )]
    public class TestTaxonomy_2004_06_15 : Taxonomy
    {
		protected Hashtable tupleElements = new Hashtable( 1000 );
		
		#region Overrides
		public void TestParse( out int errors )
		{
			errors = 0;
			ParseInternal( out errors );
		}

		#endregion

		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        [TestFixtureSetUp] public void RunFirst()
        {
			Console.WriteLine( "***Start TestTaxonomy_2004_06_15 Comments***" );
			
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {
			Console.WriteLine( "***End TestTaxonomy_2004_06_15 Comments***" );
		}

		/// <summary> Sets up test values before each test is called </summary>
        [SetUp] public void RunBeforeEachTest()
        {}

        /// <summary>Tears down test values after each test is run </summary>
        [TearDown] public void RunAfterEachTest() 
        {}
		#endregion

		// ignore - this is xbrl 2.0
		//string US_GAAP_FILE = @"q:\bin\TestSchemas\US GAAP CI 2003-07-07\us-gaap-ci-2003-07-07.xsd";

#if AUTOMATED
		string PT_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15.xsd";
		string US_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\us-gaap-ci-2004-06-15.xsd";
		string SAG_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\SoftwareAG-Spain-2004-01-30\SoftwareAG-2004-01-30.xml";
		string IFRS_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\ifrs\ifrs-gp-2004-06-15.xsd";
#else
		string PT_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15.xsd";
		string US_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\us-gaap-ci-2004-06-15.xsd";
		string SAG_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\SoftwareAG-Spain-2004-01-30\SoftwareAG-2004-01-30.xml";
		const string PT_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-usfr-pt.xml";
		const string US_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-gaap-ci.xml";
		string IFRS_FILE = @"C:\AUCENT\MAX\AXE\XBRLParser\TestSchemas\ifrs\ifrs-gp-2004-06-15.xsd";

#endif

		#region Common

#if OBSOLETE
		[Test] public void Test_LoadRoleTypes()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( US_GAAP_FILE );
			
			int errors = 0;
			s.LoadRoleTypes( out errors );
			Assert.AreEqual( 0, errors );

			Assert.AreEqual( 6, s.roleTypes.Count );

			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement" ) );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition" ) );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect" ) );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect" ) );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity" ) );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/TotalStockholdersEquityCalculation" ) );

			RoleType t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement"] as RoleType;
			TestRoleType rt = new TestRoleType( t );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "IncomeStatement", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/IncomeStatement", rt.Uri );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// StatementFinancialPosition
			t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "StatementFinancialPosition", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition", rt.Uri );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// StatementCashFlowsIndirect
			t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "StatementCashFlowsIndirect", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsIndirect", rt.Uri );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// StatementCashFlowsDirect
			t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "StatementCashFlowsDirect", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect", rt.Uri );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// StatementStockholdersEquity
			t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "StatementStockholdersEquity", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementStockholdersEquity", rt.Uri );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// TotalStockholdersEquityCalculation
			t = s.roleTypes["http://www.xbrl.org/taxonomy/us/fr/gaap/role/TotalStockholdersEquityCalculation"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "TotalStockholdersEquityCalculation", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/TotalStockholdersEquityCalculation", rt.Uri );
			Assert.AreEqual( 1, rt.WhereUsed.Count );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[0] );
		}
#endif
		#endregion

		#region PT Schema
#if OBSOLETE
		[Test] public void PT_VerifyPresentationTypes()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);

			int errors = 0;

			s.LoadRoleTypes( out errors );
			//s.TestParse( out errors );
			Assert.AreEqual( 0, errors, "load role types failure" );

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual(4, errors );
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif
		[Test] public void PT_Parse()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE );

			int errors = 0;
			s.Parse( out errors );

			Assert.AreEqual( 1898, errors );
		}

		[Test] public void PT_LoadPresentationSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);

			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 4, errors );
		}

		[Test] public void PT_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_2004_06_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_06_15.PRESENTATION_ROLE );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15-presentation.xml", presRef );
#endif
		}
				
		
		#region Elements
		[Test] public void PT_ReadElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 4, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "load elements failed" );
		}

		[Test] public void PT_BindElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			DateTime start = DateTime.Now;
			s.Load( PT_GAAP_FILE);
			
			int errors = 0;

			DateTime startPres = DateTime.Now;
			Presentation p = s.LoadPresentationSchema( out errors );
			DateTime endPres = DateTime.Now;
			Assert.AreEqual( 4, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			DateTime startElems = DateTime.Now;
			s.LoadElements( out errors );
			DateTime endElems = DateTime.Now;

			Assert.AreEqual( 0, errors, "load elements failed" );

			DateTime startBind = DateTime.Now;
			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );
			DateTime endBind = DateTime.Now;

			Console.WriteLine( "Total Time:			{0}", endBind-start );
			Console.WriteLine( "Read Presentation	{0}", endPres-startPres );
			Console.WriteLine( "Read Elements		{0}", endElems-startElems );
			Console.WriteLine( "Bind Elements		{0}", endBind-startBind );

			Assert.AreEqual( 936, errors, "bind elements failed" );

		}

		[Test] [Ignore( "Superceded by GAAP_OutputTaxonomy" )] public void PT_OutputTaxonomy()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( PT_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString() );
			}
#endif
		}

		#endregion

		#region Labels
		[Test] public void PT_LoadLabels()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE );

			int errors = 0;
			s.LoadLabels( out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1478, s.labelTable.Count );


		}

		[Test] public void PT_BindLabelsToElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( PT_GAAP_FILE);

			errors = 0;

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 4, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 936, errors, "bind elements failed" );

			errors = 0;
			s.LoadLabels( out errors );
			Assert.AreEqual( 0, errors, "load labels failed" );


			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			errors = 0;
			s.BindElements( new BindElementDelegate( s.BindLabelToElement ), out errors );
			Assert.AreEqual( 5, errors, "bind labels failure" );	// linkPart_Name, Subparagraph, Number, Paragraph, Chapter, no corresponding label info
			//Trace.Listeners.Clear();
		}

		[Test] public void PT_GetSupportedLanguages()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;
			ArrayList langs = s.GetSupportedLanguages( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		#endregion

		[Test] public void PT_TargetNamespace()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
			
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/common/pt/usfr-pt-2004-06-15", s.targetNamespace );
		}

		[Test] public void PT_LoadSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( PT_GAAP_FILE);
		}
		#endregion

		#region GAAP Schema
		
		#region UI API's
		[Test] [Ignore( "Just used to view nodes")] public void GAAP_Test_GetNodesByPresentation()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByPresentation();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 5, nodeList.Count );

			Console.WriteLine( "Nodes By Presentation: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		[Test] [Ignore( "Just used to view nodes")] public void GAAP_Test_GetNodesByElement()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 1367, nodeList.Count );

			Console.WriteLine( "Nodes By Element: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		#endregion

		#region TestTuples
		
		[Test] public void GAAP_Test_VerifyTuples()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 1367, nodeList.Count );

			Console.WriteLine( "Found Tuple Nodes: " );
			
			foreach (Node n in nodeList )
			{
				RecurseVerifyTuples( n );
			}
		}

		protected void RecurseVerifyTuples( Node n )
		{
			if ( n.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", n.Label );
			}

			if ( n.HasChildren )
			{
				for ( int i=0; i < n.Children.Count; ++i )
				{
					RecurseVerifyTuples( n.Children[i] as Node );
				}
			}
		}

		[Test] public void GAAP_TestTuples()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			s.Parse( out errors );

			IDictionaryEnumerator enumer = s.allElements.GetEnumerator();

			Console.WriteLine( "Found Tuple Elements: " );

			while ( enumer.MoveNext() )
			{
				RecurseElementsForTuples( enumer.Value as Element );
			}
		}

		protected void RecurseElementsForTuples( Element e )
		{
			if ( e.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", e.Name );
			}

			if ( e.HasChildren )
			{
				foreach ( Element c in e.Children )
				{
					RecurseElementsForTuples( c );
				}
			}
		}

		#endregion

		[Test] [Ignore("Takes a while")] public void GAAP_OutputTaxonomy()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( US_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString() );
			}
#endif
		}

		#region Presentation
#if OBSOLETE
		[Test] public void GAAP_VerifyPresentationTypes()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			//s.TestParse( out errors );
			//Assert.AreEqual( 22, errors, "parse failed" );
			s.LoadRoleTypes( out errors );
			Assert.AreEqual( 0, errors, "load role types returned the wrong number of errors" );

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual(22, errors, "presentation schema failed" );
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif
		[Test] public void GAAP_LoadPresentationSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 22, errors );
		}

		[Test] public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_2004_06_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_06_15.PRESENTATION_ROLE );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\us-gaap-ci-2004-06-15-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\us-gaap-ci-2004-06-15-presentation.xml", presRef );
#endif
		}
				
		#endregion

		#region imports
		[Test] public void GAAP_LoadImports()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 22, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load import errors" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AccountingPolicies" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void GAAP_LoadImportsAndBind()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 22, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load import errors" );	// 5 ci elements are duplicated in the usfr-pt schema

			errors = 0;
			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );
			Assert.AreEqual( 690, errors, "wrong number of bind errors" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AccountingPolicies" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void GAAP_GetDependantTaxonomies()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( !s.Load( US_GAAP_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			ArrayList imps = s.GetDependantTaxonomies( false, out errors );
			Assert.AreEqual( 0, errors, "error list not 0" );

			Assert.AreEqual( 1, imps.Count, "wrong number of imports returned" );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15.xsd", imps[0] );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-06-15\usfr-pt-2004-06-15.xsd", imps[0] );
#endif
		}
		#endregion

		[Test] public void GAAP_Parse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );
			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			TimeSpan level = new TimeSpan( 0, 0, 0, 4, 0 );	// 4 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long" );

			PresentationLink pl = s.presentationInfo["Statement of Cash Flows - Direct Method - CI"] as PresentationLink;
			Assert.IsNotNull( pl );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pt_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			Assert.AreEqual( 22+690+5+4+5+953+1474, errors, "parse failure" );

			// 22 = presentation load errors
			// 690 = bind failures - elements w/o presentation
			// 5 = imports - 5 duplicate elements 
			// 4 = duplicate labels
			// 5 = elements without matching labels
			// 953 = elements without ref
			// 1474 = element without calc
			
			//Trace.Listeners.Clear();
		}


		#region Elements
		[Test] public void GAAP_BindElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 22, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 5, errors, "bind elements failed" );
		}

		[Test] public void GAAP_ReadElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			int numElements = s.LoadElements( out errors );
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 5, s.allElements.Count );
		}

		#endregion

		[Test] public void GAAP_GetLabelRoles()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( US_GAAP_FILE );
			
			int errors = 0;
			ArrayList roles = s.GetLabelRoles( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 6, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[2] );
			Assert.AreEqual( "periodStartLabel", roles[3] );
			Assert.AreEqual( "terseLabel", roles[4] );
			Assert.AreEqual( "totalLabel", roles[5] );
		}

		[Test] public void GAAP_GetSupportedLanguages()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;
			ArrayList langs = s.GetSupportedLanguages( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		[Test] public void GAAP_TargetNamespace()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( US_GAAP_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-06-15", s.targetNamespace );
		}

		[Test] public void GAAP_LoadSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
		}
		#endregion

		#region Software AG Schema
		[Test] [Ignore("do this later")] public void SAG_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( SAG_FILE);
			
			string labelRef = s.GetLinkbaseReference( TestTaxonomy_2004_06_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_06_15.PRESENTATION_ROLE );

			Assert.AreEqual( "blue", labelRef );
		}
				

		[Test] [Ignore( "Worry about this later")] public void SAG_TargetNamespace()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( SAG_FILE);

			s.GetTargetNamespace();

			Assert.IsNull( s.targetNamespace );
		}

		[Test] public void SAG_LoadSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( SAG_FILE);

		}
		#endregion

		#region ifrs

		#region TestTuples
		
		[Test] public void ifrs_Test_VerifyTuples()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 2591, nodeList.Count );

			Console.WriteLine( "Found Tuple Nodes: " );
			
			foreach (Node n in nodeList )
			{
				RecurseVerifyTuples( n );
			}
		}

		protected void ifrsRecurseVerifyTuples( Node n )
		{
			if ( n.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", n.Label );
			}

			if ( n.HasChildren )
			{
				for ( int i=0; i < n.Children.Count; ++i )
				{
					RecurseVerifyTuples( n.Children[i] as Node );
				}
			}
		}

		[Test] public void ifrs_TestTuples()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			s.Parse( out errors );

			if (s.boundElements != null)

			{
				IDictionaryEnumerator enumer = s.boundElements.GetEnumerator();

				Console.WriteLine( "Found Tuple Elements: " );

				while ( enumer.MoveNext() )
				{
					RecurseElementsForTuples( enumer.Value as Element );
				}
			}
		}

		protected void ifrsRecurseElementsForTuples( Element e )
		{
			if ( e.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", e.Name );
			}

			if ( e.HasChildren )
			{
				foreach ( Element c in e.Children )
				{
					RecurseElementsForTuples( c );
				}
			}
		}

		#endregion

		[Test] [Ignore("Takes a while")] public void ifrs_OutputTaxonomy()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( US_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString() );
			}
#endif
		}

		#region Presentation
#if OBSOLETE
		[Test] public void ifrs_VerifyPresentationTypes()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			//s.TestParse( out errors );
			//Assert.AreEqual( 22, errors, "parse failed" );
			s.LoadRoleTypes( out errors );
			Assert.AreEqual( 0, errors, "load role types returned the wrong number of errors" );

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual(144, errors, "presentation schema failed" );
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif
		[Test] public void ifrs_LoadPresentationSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 144, errors );
		}

		[Test] public void ifrs_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US IFRS File" );
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_2004_06_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_06_15.PRESENTATION_ROLE );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\ifrs\ifrs-gp-2004-06-15-presentation.xml", presRef );
#else
			Assert.AreEqual( @"C:\AUCENT\MAX\AXE\XBRLParser\TestSchemas\ifrs\ifrs-gp-2004-06-15-presentation.xml", presRef );
#endif
		}
				
		#endregion

		#region imports
		[Test] public void ifrs_LoadImports()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 144, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "OtherLiabilitiesPolicy" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void ifrs_LoadImportsAndBind()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 144, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );	// 5 ci elements are duplicated in the usfr-pt schema

			errors = 0;
			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );
			Assert.AreEqual( 0, errors, "wrong number of bind errors" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "OtherLiabilitiesPolicy" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void ifrs_GetDependantTaxonomies()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			if ( !s.Load( IFRS_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			ArrayList imps = s.GetDependantTaxonomies( false, out errors );
			Assert.AreEqual( 0, errors, "error list not 0" );

			Assert.AreEqual( 0, imps.Count, "wrong number of imports returned" );

		}
		#endregion

		[Test] public void ifrs_Parse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );
			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			TimeSpan level = new TimeSpan( 0, 0, 0, 10, 0 );	// 10 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long" );

			PresentationLink pl = s.presentationInfo["Accounting Policies"] as PresentationLink;
			Assert.IsNotNull( pl );


		}


		#region Elements
		[Test] public void ifrs_BindElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 144, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 5, errors, "load elements failed" ); //MIN missing sometimes, maxOccurs do not exist

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 0, errors, "bind elements failed" );
		}

		[Test] public void ifrs_ReadElements()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			int numElements = s.LoadElements( out errors );
			Assert.AreEqual( 5, errors );
			Assert.AreEqual( 2591, s.allElements.Count );
		}

		#endregion

		[Test] public void ifrs_GetLabelRoles()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( IFRS_FILE );
			
			int errors = 0;
			ArrayList roles = s.GetLabelRoles( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 5, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[2] );
			Assert.AreEqual( "periodStartLabel", roles[3] );
			Assert.AreEqual( "restated", roles[4] );
		}

		[Test] public void ifrs_GetSupportedLanguages()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( IFRS_FILE);
			
			int errors = 0;
			ArrayList langs = s.GetSupportedLanguages( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		[Test] public void ifrs_TargetNamespace()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			s.Load( IFRS_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-06-15", s.targetNamespace );
		}

		[Test] public void ifrs_LoadSchema()
		{
			TestTaxonomy_2004_06_15 s = new TestTaxonomy_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US GAAP File" );
		}

		#endregion


    }
}
#endif
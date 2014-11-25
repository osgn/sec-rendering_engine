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
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.Common.Data;

    [TestFixture] 
    public class TestTaxonomy_2004_07_06 : Taxonomy
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
			Console.WriteLine( "***Start TestTaxonomy_2004_07_06 Comments***" );
			
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {
			Trace.Listeners.Clear();
			Console.WriteLine( "***End TestTaxonomy_2004_07_06 Comments***" );
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
		string PT_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06.xsd";
		string US_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\us-gaap-ci-2004-07-06.xsd";
#else
		string PT_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06.xsd";
		string US_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\us-gaap-ci-2004-07-06.xsd";
		const string PT_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-usfr-pt.xml";
		const string US_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-gaap-ci.xml";
		const string NODE_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-gaap-ci-nodes.xml";
#endif

		#region Common
#if OBSOLETE
		[Test] public void Test_LoadRoleTypes()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE );
			
			int errors = 0;
			s.LoadRoleTypes( out errors );
			Assert.AreEqual( 0, errors );

			Assert.AreEqual( 7, s.roleTypes.Count );

			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/taxonomy/us/fr/gaap/role/Notes" ) );
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

		[Test] public void PT_GetTargetPrefix()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE );

			Assert.AreEqual( "usfr-pt", s.GetTargetPrefix() );
		}

#if OBSOLETE
		[Test] public void PT_VerifyPresentationTypes()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);

			int errors = 0;

			s.LoadRoleTypes( out errors );
			//s.TestParse( out errors );
			Assert.AreEqual( 0, errors, "load role types failure" );

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual(0, errors, "wrong number of errors" );
			Assert.AreEqual(0, s.numWarnings, "wrong number of warnings" );
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif
		[Test] public void PT_Parse()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE );

			int errors = 0;
			s.Parse( out errors );

			Assert.AreEqual( 0, errors, "num errors wrong" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );
		}

		[Test] public void PT_LoadPresentationSchema()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);

			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 0, errors, "wrong number of errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );
		}

		[Test] public void PT_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_2004_07_06.TARGET_LINKBASE_URI + TestTaxonomy_2004_07_06.PRESENTATION_ROLE );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06-presentation.xml", presRef );
#endif
		}
				
		
		#region Elements
		[Test] public void PT_ReadElements()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 0, errors, "load presentation failed" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "load elements failed" );
		}

		[Test] public void PT_BindElements()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			DateTime start = DateTime.Now;
			s.Load( PT_GAAP_FILE);
			
			int errors = 0;

			DateTime startPres = DateTime.Now;
			Presentation p = s.LoadPresentationSchema( out errors );
			DateTime endPres = DateTime.Now;
			Assert.AreEqual( 0, errors, "load presentation failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );

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

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );
		}

		[Test] [Ignore( "Superceded by GAAP_OutputTaxonomy" )] public void PT_OutputTaxonomy()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE );

			int errors = 0;
			s.LoadLabels( out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1556, s.labelTable.Count );


		}

		[Test] public void PT_BindLabelsToElements()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			s.Load( PT_GAAP_FILE);

			errors = 0;

			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 0, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "bind elements failed - wrong number of warnings" );

			errors = 0;
			s.LoadLabels( out errors );
			Assert.AreEqual( 0, errors, "load labels failed" );

			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			errors = 0;
			s.numWarnings = 0;
			s.BindElements( new BindElementDelegate( s.BindLabelToElement ), out errors );
			Assert.AreEqual( 0, errors, "bind labels failure" );	// linkPart_Name, Subparagraph, Number, Paragraph, Chapter, no corresponding label info
			Assert.AreEqual( 0, s.numWarnings, "wrong number of bind labels warnings" );
			//Trace.Listeners.Clear();
		}

		[Test] public void PT_GetSupportedLanguages()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);
			
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/common/pt/usfr-pt-2004-07-06", s.targetNamespace );
		}

		[Test] public void PT_LoadSchema()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( PT_GAAP_FILE);
		}
		#endregion

		#region GAAP Schema
		
		#region UI API's
		[Test] [Ignore( "Just used to view nodes")] public void GAAP_Test_GetNodesByPresentation()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 1445, nodeList.Count );

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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

		[Test] public void GAAP_GetTargetPrefix()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE );

			Assert.AreEqual( "us-gaap-ci", s.GetTargetPrefix() );
		}

		[Test] public void GAAP_OutputTaxonomy()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( US_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString( false ) );
			}
#endif
		}

		[Test] public void GAAP_OutputTaxonomyByNodes()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( NODE_OUT_FILE ) )
			{
				s.currentLanguage = "en";
				s.currentLabelRole = "terseLabel";

				sw.Write( s.ToXmlString( s.GetNodesByPresentation() ) );
			}
#endif
		}


		#region Presentation
		[Test] public void GAAP_VerifyPresentationCorrect()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			string rawXml = s.ToXmlString();

			s.currentLabelRole = "terseLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();


		}
#if OBSOLETE
		[Test] public void GAAP_VerifyPresentationTypes()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			//Assert.AreEqual(22, errors, "presentation schema failed" );	// PROBLEM with us-gaap-ci-2004-07-06-presentation.xml ?
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif
		[Test] public void GAAP_LoadPresentationSchema()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			//Assert.AreEqual( 22, errors );
		}

		[Test] public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_2004_07_06.TARGET_LINKBASE_URI + TestTaxonomy_2004_07_06.PRESENTATION_ROLE );

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\us-gaap-ci-2004-07-06-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\us-gaap-ci-2004-07-06-presentation.xml", presRef );
#endif
		}
				
		#endregion

		#region imports
		[Test] public void GAAP_LoadImports()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );

			errors = 0;
			s.LoadPresentation( out errors );
			//Assert.AreEqual( 22, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load element errors" );
			
			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AccountingPolicies" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void GAAP_LoadImportsAndBind()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );	// 5 ci elements are duplicated in the usfr-pt schema

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load presentation errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of load presentation warnings" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 5, errors, "wrong number of load element errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of load element warnings" );
			
			errors = 0;
			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );
			Assert.AreEqual( 0, errors, "wrong number of bind errors" );

			if ( s.numWarnings > 0 )
			{
				s.ErrorList.Sort();
				foreach ( ParserMessage pm in s.ErrorList )
				{
					if ( pm.Level == TraceLevel.Warning )
					{
						Console.WriteLine( "Warning: " + pm.Message );
					}
				}
			}

			Assert.AreEqual( 0, s.numWarnings, "wrong number of bind warnings" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AccountingPolicies" ) );

			//Trace.Listeners.Clear();

		}

		[Test] public void GAAP_GetDependantTaxonomies()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06.xsd", imps[0] );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-07-06\usfr-pt-2004-07-06.xsd", imps[0] );
#endif
		}
		#endregion


		[Test] public void GAAP_Parse_Label()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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

			Element el = s.allElements["RoyaltyExpense"] as Element;
			string labelString = string.Empty;
			el.TryGetLabel("en", "label", out labelString);
			Assert.IsTrue(labelString != "",  "lable info is not populated");
		}

		[Test] public void GAAP_Parse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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

			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementCashFlowsDirect"] as PresentationLink;
			Assert.IsNotNull( pl, "presentation link is null" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pt_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			// problem with presentation linkbase - don't know the solution yet
			Assert.AreEqual( 5, errors, "parse failure" );

			Assert.AreEqual( 2, s.numWarnings, "wrong number of warnings" );
			// 2 = duplicate labels

			//Trace.Listeners.Clear();

			TimeSpan level = new TimeSpan( 0, 0, 0, 4, 0 );	// 4 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long" );
		}

		[Test] public void GAAP_ElementTaxonomyLinks()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			s.currentLanguage = "en";
			s.currentLabelRole = "terseLabel";

			ArrayList nodes = s.GetNodesByElement();

			Assert.AreEqual( 1, ((Node)nodes[0]).TaxonomyInfoId, "Taxonomy id not correct" );

			TaxonomyItem ti = s.GetTaxonomyInfo( (Node)nodes[0] );

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/common/pt/usfr-pt-2004-07-06", ti.WebLocation, "target namespace wrong" );

			Assert.AreEqual( PT_GAAP_FILE, ti.Location, "targetLocation wrong" );
		}


		#region Elements
		[Test] public void GAAP_BindElements()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			Presentation p = s.LoadPresentationSchema( out errors );
			//Assert.AreEqual( 22, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );
		}

		[Test] public void GAAP_ReadElements()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			int numElements = s.LoadElements( out errors );
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 5, s.allElements.Count );
		}

		#endregion

		[Test] public void GAAP_GetLabelRoles()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

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
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;
			ArrayList langs = s.GetSupportedLanguages( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		[Test] public void GAAP_TargetNamespace()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			s.Load( US_GAAP_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-07-06", s.targetNamespace );
		}

		[Test] public void GAAP_LoadSchema()
		{
			TestTaxonomy_2004_07_06 s = new TestTaxonomy_2004_07_06();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
		}
		#endregion

    }
}
#endif
//=============================================================================
// Schema (class)
// Aucent Corporation
//=============================================================================

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
	using System.Xml;
	using System.Collections;

    using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.Common.Data;

    [TestFixture] 
    public class TestSchema : Schema
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
			Console.WriteLine( "***Start TestSchema Comments***" );
		}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {
			Console.WriteLine( "***End TestSchema Comments***" );
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
		string PT_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\usfr-pt-2004-06-15.xsd";
		string US_GAAP_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15.xsd";
		string SAG_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\SoftwareAG-Spain-2004-01-30\SoftwareAG-2004-01-30.xml";
#else
		string PT_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\usfr-pt-2004-06-15.xsd";
		string US_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15.xsd";
		string SAG_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\SoftwareAG-Spain-2004-01-30\SoftwareAG-2004-01-30.xml";
#endif

		#region Common
		[Test] public void Test_LoadRoleTypes()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( !s.Load( US_GAAP_FILE, out ei ) )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			Assert.IsNull( ei );
			
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

		#endregion

		#region PT Schema
		[Test] public void PT_VerifyPresentationTypes()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( PT_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

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

		[Test] public void PT_Parse()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( PT_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			int errors = 0;
			s.Parse( out ei, out errors );
			if ( ei != null )
			{
				Assert.Fail( ei.ExceptionMsg );
			}

			Assert.AreEqual( 1056, errors );
		}

		[Test] public void PT_LoadPresentationSchema()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( PT_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 4, errors );
		}

		[Test] public void PT_GetLinkbaseRef_Presentation()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( PT_GAAP_FILE, out ei ), "Could not load PT GAAP File" );
			Assert.IsNull( ei );
			
			string presRef = s.GetLinkbaseReference( TestSchema.TARGET_LINKBASE_URI + TestSchema.PRESENTATION_ROLE );

			if ( ei != null )
			{
				throw new Exception( ei.ExceptionMsg );
			}

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\usfr-pt-2004-06-15-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\usfr-pt-2004-06-15-presentation.xml", presRef );
#endif
		}
				
		
		#region Elements
		[Test] public void PT_ReadElements()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( PT_GAAP_FILE, out ei ), "Could not load PT GAAP File" );
			Assert.IsNull( ei );
			
			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 4, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );
			Assert.AreEqual( 1052, errors, "load elements failed" );
		}

		#endregion

		[Test] [Ignore("Do later")] public void PT_GetSupportedLanguages()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( PT_GAAP_FILE, out ei ), "Could not load US GAAP File" );
			Assert.IsNull( ei );
			
			s.GetSupportedLanguages();

			//Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/2003-07-07", s.targetNamespace );

		}

		[Test] public void PT_TargetNamespace()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( PT_GAAP_FILE, out ei ), "Could not load PT GAAP File" );
			Assert.IsNull( ei );
			
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/common/pt/usfr-pt-2004-06-15", s.targetNamespace );
		}

		[Test] public void PT_LoadSchema()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( PT_GAAP_FILE, out ei ), "Could not load PT GAAP File" );
			Assert.IsNull( ei );
		}
		#endregion

		#region GAAP Schema
		#region Presentation
		[Test] public void GAAP_VerifyPresentationTypes()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( US_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			int errors = 0;

			s.TestParse( out errors );
			Assert.AreEqual( 27, errors, "parse failed" );

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual(22, errors, "presentation schema failed" );
			Assert.IsNotNull( p );

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}

		[Test] public void GAAP_Parse()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( US_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			int errors = 0;
			s.Parse( out ei, out errors );
			if ( ei != null )
			{
				Assert.Fail( ei.ExceptionMsg );
			}

			Assert.AreEqual( 27, errors );
		}

		[Test] public void GAAP_LoadPresentationSchema()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( US_GAAP_FILE, out ei ) != true )
			{
				Assert.IsNotNull( ei );
				Assert.Fail( ei.ExceptionMsg );
			}

			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 22, errors );
		}

		[Test] public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out ei ), "Could not load US GAAP File" );
			Assert.IsNull( ei );
			
			string presRef = s.GetLinkbaseReference( TestSchema.TARGET_LINKBASE_URI + TestSchema.PRESENTATION_ROLE );

			if ( ei != null )
			{
				throw new Exception( ei.ExceptionMsg );
			}

#if AUTOMATED
			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15-presentation.xml", presRef );
#else
			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15-presentation.xml", presRef );
#endif
		}
				
		#endregion
		#region Elements
		[Test] [Ignore ("Need to implement label")] public void GAAP_ReadElements()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			if ( s.Load( US_GAAP_FILE, out ei ) == false )
			{
				Assert.Fail( ei.ExceptionMsg );
			}

			if ( ei != null )
			{
				Assert.Fail( ei.ExceptionMsg );
			}
//			int errors = 0;
//			int numElements = s.LoadElements( out errors );
//			Assert.AreEqual( 0, errors );
////			Assert.AreEqual( 5, numElements, "No Elements Found" );
//			Assert.AreEqual( 5, s.elements.Count );
//
//			Assert.IsTrue( s.elements.ContainsKey( "Name" ) );
//			Assert.IsTrue( s.elements.ContainsKey( "Number" ) );
//			Assert.IsTrue( s.elements.ContainsKey( "Chapter" ) );
//			Assert.IsTrue( s.elements.ContainsKey( "Paragraph" ) );
//			Assert.IsTrue( s.elements.ContainsKey( "Subparagraph" ) );
		}

		#endregion

		[Test] [Ignore("Do later")] public void GAAP_GetSupportedLanguages()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out ei ), "Could not load US GAAP File" );
			Assert.IsNull( ei );
			
			s.GetSupportedLanguages();

			//Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/2003-07-07", s.targetNamespace );

		}

		[Test] public void GAAP_TargetNamespace()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out ei ), "Could not load US GAAP File" );
			Assert.IsNull( ei );
			
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-06-15", s.targetNamespace );
		}

		[Test] public void GAAP_LoadSchema()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out ei ), "Could not load US GAAP File" );
			Assert.IsNull( ei );
		}
		#endregion

		#region Software AG Schema
		[Test] [Ignore("do this later")] public void SAG_GetLinkbaseRef_Presentation()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( SAG_FILE, out ei ), "Could not load SAG File" );
			Assert.IsNull( ei );
			
			string labelRef = s.GetLinkbaseReference( TestSchema.TARGET_LINKBASE_URI + TestSchema.PRESENTATION_ROLE );

			Assert.AreEqual( "blue", labelRef );
		}
				

		[Test] [Ignore( "Worry about this later")] public void SAG_TargetNamespace()
		{
			TestSchema s = new TestSchema();

			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( SAG_FILE, out ei ), "Could not load SoftwareAge File" );
			Assert.IsNull( ei );

			s.GetTargetNamespace();

			Assert.IsNull( ei );
			Assert.IsNull( s.targetNamespace );
		}

		[Test] public void SAG_LoadSchema()
		{
			TestSchema s = new TestSchema();
			ErrorInfo ei = null;

			Assert.AreEqual( true, s.Load( SAG_FILE, out ei ), "Could not load SoftwareAG file" );
			Assert.IsNull( ei );

		}
		#endregion

    }
}
#endif
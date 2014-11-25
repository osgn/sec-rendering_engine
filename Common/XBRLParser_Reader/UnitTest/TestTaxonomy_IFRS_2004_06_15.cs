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
	using System.Text;
	using System.Collections;
	using System.Diagnostics;

    using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.Common.Data;

    [TestFixture]
    public class TestTaxonomy_IFRS_2004_06_15 : Taxonomy
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
			Console.WriteLine( "***Start TestTaxonomy_IFRS_2004_06_15 Comments***" );
			
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
			Console.WriteLine( "***End TestTaxonomy_IFRS_2004_06_15 Comments***" );
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

		string IFRS_FILE = @"s:\TestSchemas\ifrs\ifrs-gp-2004-06-15.xsd";
		const string IFRS_OUT_FILE = @"s:\TestSchemas\aucent-ifrs_gp.xml";
		const string NODE_OUT_FILE = @"s:\TestSchemas\aucent-ifrs_gp-nodes.xml";

		#region Helpers
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

		protected void SendErrorsToConsole( ArrayList errorList )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level != TraceLevel.Error )
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		protected void SendWarningsToConsole( ArrayList errorList )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		protected void SendWarningsToConsole( ArrayList errorList, string filter )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Message.IndexOf( filter ) < 0 )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		#endregion

		#region Common
#if OBSOLETE
		[Test] public void Test_LoadRoleTypes()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE );
			
			int errors = 0;
			s.LoadRoleTypes( out errors );
			Assert.AreEqual( 0, errors );

			Assert.AreEqual( 15, s.roleTypes.Count, s.roleTypes.Count.ToString() + " found. expected 15" );

			Assert.IsTrue( s.roleTypes.ContainsKey( "http://www.xbrl.org/2003/frta/role/restated" ), "http://www.xbrl.org/2003/frta/role/restated not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetLiquidity" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetLiquidity not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetNetAssets" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetNetAssets not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByFunction" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByFunction not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByNature" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByNature not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsDirect" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsDirect not found" );

			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsIndirect" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsIndirect not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Equity" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Equity not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/AccountingPolicies" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/AccountingPolicies not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/ExplanatoryDisclosures" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/ExplanatoryDisclosures not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Classes" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Classes not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CurrentNonCurrent" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CurrentNonCurrent not found" );
			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/NetGross" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/NetGross not found" );

			Assert.IsTrue( s.roleTypes.ContainsKey( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/TypeEnumerations" ), "http://xbrl.iasb.org/int/fr/ifrs/gp/role/TypeEnumerations not found" );

			RoleType t = s.roleTypes["http://www.xbrl.org/2003/frta/role/restated"] as RoleType;
			TestRoleType rt = new TestRoleType( t );

			Assert.IsNotNull( rt );

			Assert.AreEqual( "restated", rt.Id );
			Assert.AreEqual( "http://www.xbrl.org/2003/frta/role/restated", rt.Uri );
			Assert.AreEqual( string.Empty, rt.Definition );
			Assert.AreEqual( 1, rt.WhereUsed.Count );
			Assert.AreEqual( "link:label", rt.WhereUsed[0] );

			// BalanceSheetClassified
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "BalanceSheetClassified", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified", rt.Uri );
			Assert.AreEqual( "Balance Sheet, Classified Format", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// BalanceSheetLiquidity
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetLiquidity"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "BalanceSheetLiquidity", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetLiquidity", rt.Uri );
			Assert.AreEqual( "Balance Sheet, Order of Liquidity Format", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// BalanceSheetNetAssets
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetNetAssets"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "BalanceSheetNetAssets", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetNetAssets", rt.Uri );
			Assert.AreEqual( "Balance Sheet, Net Assets Format", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// IncomeStatementByFunction
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByFunction"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "IncomeStatementByFunction", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByFunction", rt.Uri );
			Assert.AreEqual( "Income Statement, By Function Format", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// IncomeStatementByNature
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByNature"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "IncomeStatementByNature", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/IncomeStatementByNature", rt.Uri );
			Assert.AreEqual( "Income Statement, By Nature Format", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// CashFlowsDirect
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsDirect"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "CashFlowsDirect", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsDirect", rt.Uri );
			Assert.AreEqual( "Cash Flows, Direct Method", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// CashFlowsIndirect
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsIndirect"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "CashFlowsIndirect", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CashFlowsIndirect", rt.Uri );
			Assert.AreEqual( "Cash Flows, Indirect Method", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// Equity
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/Equity"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "Equity", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Equity", rt.Uri );
			Assert.AreEqual( "Statement of Changes in Equity", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// AccountingPolicies
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/AccountingPolicies"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "AccountingPolicies", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/AccountingPolicies", rt.Uri );
			Assert.AreEqual( "Accounting Policies", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// ExplanatoryDisclosures
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/ExplanatoryDisclosures"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "ExplanatoryDisclosures", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/ExplanatoryDisclosures", rt.Uri );
			Assert.AreEqual( "Explanatory Disclosures", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// Classes
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/Classes"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "Classes", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/Classes", rt.Uri );
			Assert.AreEqual( "Classes", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// CurrentNonCurrent
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/CurrentNonCurrent"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "CurrentNonCurrent", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/CurrentNonCurrent", rt.Uri );
			Assert.AreEqual( "Current/Non Current Breakdown", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// NetGross
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/NetGross"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "NetGross", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/NetGross", rt.Uri );
			Assert.AreEqual( "Net/Gross Breakdown", rt.Definition );
			Assert.AreEqual( 2, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
			Assert.AreEqual( "link:calculationLink", rt.WhereUsed[1] );

			// NetGross
			t = s.roleTypes["http://xbrl.iasb.org/int/fr/ifrs/gp/role/TypeEnumerations"] as RoleType;
			rt = new TestRoleType( t );
			Assert.IsNotNull( rt );

			Assert.AreEqual( "TypeEnumerations", rt.Id );
			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/role/TypeEnumerations", rt.Uri );
			Assert.AreEqual( "Type Enumerations", rt.Definition );
			Assert.AreEqual( 1, rt.WhereUsed.Count );
			Assert.AreEqual( "link:presentationLink", rt.WhereUsed[0] );
		}
#endif
		#endregion

		#region IFRS Schema
		
		#region UI API's
		[Test] [Ignore( "Just used to view nodes")] public void IFRS_Test_GetNodesByPresentation()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByPresentation();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 24, nodeList.Count );

			Console.WriteLine( "Nodes By Presentation: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		[Test][Ignore("Display only" )] public void IFRS_Test_GetNodesByElement()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 2591, nodeList.Count );

			Console.WriteLine( "Nodes By Element: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		#endregion

		#region TestTuples
		
		[Test] public void VerifyTupleOfTuples()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			// find EquityCompensationPlan - it's a tuple, and should have ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod as a child
			// (it's also a tuple)

			Element e = s.allElements[ "EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find EquityCompensationPlan" );

			bool found = false;
			foreach ( Element ec in e.Children )
			{
				if ( ec.Name == "ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod" );
			Assert.IsFalse( s.boundElements.ContainsKey("ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod"), "ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod found in boundElements" );
			
			// find RelatedParty - it's a tuple, and should have TransactionRelatedParty as a child
			// (it's also a tuple)

			e = s.allElements[ "RelatedParty" ] as Element;
			Assert.IsNotNull( e, "can't find RelatedParty" );

			found = false;
			foreach ( Element ec in e.Children )
			{
				if ( ec.Name == "TransactionRelatedParty" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find TransactionRelatedParty" );
			Assert.IsFalse( s.boundElements.ContainsKey("TransactionRelatedParty"), "TransactionRelatedParty found in boundElements" );

			// find EquityCompensationPlan - it's a tuple, and should have EquityInstrumentIssuedEquityCompensationPlan  as a child
			// (it's also a tuple)

			e = s.allElements[ "EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find EquityCompensationPlan" );

			found = false;
			foreach ( Element ec in e.Children )
			{
				if ( ec.Name == "EquityInstrumentIssuedEquityCompensationPlan" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find EquityInstrumentIssuedEquityCompensationPlan " );
			Assert.IsFalse( s.boundElements.ContainsKey("EquityInstrumentIssuedEquityCompensationPlan"), "EquityInstrumentIssuedEquityCompensationPlan found in boundElements" );
			
			// find EquityCompensationPlan - it's a tuple, and should have MajorClassAssetLiabilityEntityAcquired as a child
			// (it's also a tuple)

			e = s.allElements[ "Acquisition" ] as Element;
			Assert.IsNotNull( e, "can't find Acquisition" );

			found = false;
			foreach ( Element ec in e.Children )
			{
				if ( ec.Name == "MajorClassAssetLiabilityEntityAcquired" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find MajorClassAssetLiabilityEntityAcquired " );
			Assert.IsFalse( s.boundElements.ContainsKey("MajorClassAssetLiabilityEntityAcquired"), "MajorClassAssetLiabilityEntityAcquired found in boundElements" );
			
			// find EquityCompensationPlan - it's a tuple, and should have OwnEquityInstrumentHeldEquityCompensationPlan as a child
			// (it's also a tuple)

			e = s.allElements[ "EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find EquityCompensationPlan" );

			found = false;
			foreach ( Element ec in e.Children )
			{
				if ( ec.Name == "OwnEquityInstrumentHeldEquityCompensationPlan" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find OwnEquityInstrumentHeldEquityCompensationPlan " );
			Assert.IsFalse( s.boundElements.ContainsKey("OwnEquityInstrumentHeldEquityCompensationPlan"), "OwnEquityInstrumentHeldEquityCompensationPlan found in boundElements" );
		}

		[Test] [Ignore("display only" )] public void IFRS_Test_VerifyTuples()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 2586, nodeList.Count );

			Console.WriteLine( "Found Tuple Nodes: " );
			
			foreach (Node n in nodeList )
			{
				RecurseVerifyTuples( n );
			}
		}

		[Test] public void IFRS_TestTuples()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			s.Parse( out errors );

			IDictionaryEnumerator enumer = s.allElements.GetEnumerator();

			Console.WriteLine( "Found Tuple Elements: " );

			while ( enumer.MoveNext() )
			{
				RecurseElementsForTuples( enumer.Value as Element );
			}
		}

		#endregion

		[Test] public void IFRS_GetTargetPrefix()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE );

			Assert.AreEqual( "ifrs-gp", s.GetTargetPrefix() );
		}

		[Test] [Ignore("Display only" )] public void IFRS_OutputTaxonomy()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( IFRS_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString( false ) );
			}
#endif
		}

		[Test] [Ignore("Display only" )] public void IFRS_OutputTaxonomyByNodes()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE);
			
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
		[Test] [Ignore("Display only" )] public void IFRS_VerifyPresentationCorrect()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			string rawXml = s.ToXmlString();

			s.currentLabelRole = "terseLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();
		}

#if OBSOLETE
		[Test] public void IFRS_VerifyPresentationTypes()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

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
			if ( errors > 0 )
			{
				SendErrorsToConsole( p.ErrorList );
			}

			Assert.AreEqual(0, errors, "presentation schema failed" );	// PROBLEM with us-IFRS-ci-2004-08-15-presentation.xml ?
			Assert.IsNotNull( p );

			p.ErrorList.Clear();

			if ( s.VerifyPresentationTypes( p, out errors ) == false )
			{
				if ( errors > 0 )
				{
					SendErrorsToConsole( s.ErrorList );
				}

				Assert.Fail( "There are " + errors + " failures in the VerifyPresentationTypes call" );
			}
		}
#endif

		[Test] public void IFRS_LoadPresentationSchema()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 0, errors );
		}

		[Test] public void IFRS_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US IFRS File" );
			
			string presRef = s.GetLinkbaseReference( TestTaxonomy_IFRS_2004_06_15.TARGET_LINKBASE_URI + TestTaxonomy_IFRS_2004_06_15.PRESENTATION_ROLE );

			Assert.AreEqual( @"s:\TestSchemas\ifrs\ifrs-gp-2004-06-15-presentation.xml", presRef );
		}
				
		#endregion

		#region Calculation
		[Test] public void IFRS_LoadCalculationSchema()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			Presentation p = s.LoadCalculationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 0, errors );
		}

		[Test] public void IFRS_BindCalculation()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.Parse( out errors );

			//			errors = 0;
			//			s.LoadCalculation( out errors );
			//			Assert.AreEqual( 0, errors, "load calc failed" );
			//
			s.numWarnings = errors = 0;
			//s.BindCalculation( out errors );
			s.BindElements( new BindElementDelegate( s.BindElementToCalculationLocator ), out errors );
			if ( errors != 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

//			if ( s.numWarnings != 0 )
//			{
//				SendWarningsToConsole( s.errorList, "Can't find presentation link for element" );
//			}

			Assert.AreEqual( 0, errors, "Bind calc failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );

		}

		#endregion

		#region imports
		[Test] public void IFRS_LoadImports()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );
			//Assert.AreEqual( 22, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AmountDeferredExpenditureNonCurrent" ), "can't find AmountDeferredExpenditureNonCurrent" );

			//Trace.Listeners.Clear();

		}

		[Test] public void IFRS_LoadImportsAndBind()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadPresentation( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			
			errors = 0;
			s.LoadImports( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}
			
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );	// 5 ci elements are duplicated in the usfr-pt schema
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			errors = 0;
			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			//			if ( errors > 0 )
			//			{
			//				SendErrorsToConsole( s.errorList );
			//			}
			//			
			Assert.AreEqual( 0, errors, "wrong number of bind errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "AmountDeferredExpenditureNonCurrent" ), "Can't find AmountDeferredExpenditureNonCurrent" );

			//Trace.Listeners.Clear();

		}

		[Test] public void IFRS_GetDependantTaxonomies()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( !s.Load( IFRS_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			ArrayList imps = s.GetDependantTaxonomies( false, out errors );
			Assert.AreEqual( 0, errors, "error list not 0" );

			Assert.AreEqual( 0, imps.Count, "wrong number of imports returned" );

//#if AUTOMATED
//			Assert.AreEqual( @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\ifrs\ifrs-pt-2004-08-15.xsd", imps[0] );
//#else
//			Assert.AreEqual( @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\ifrs\ifrs-pt-2004-08-15.xsd", imps[0] );
//#endif
		}
		#endregion

		[Test] public void IFRS_Parse_Label()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

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

			Element el = s.allElements["AmountDeferredExpenditureNonCurrent"] as Element;
			Assert.IsNotNull( el, "can't find AmountDeferredExpenditureNonCurrent" );
			string labelString = string.Empty;
			el.TryGetLabel("en", "label", out labelString);
			Assert.IsTrue(labelString != "",  "lable info is not populated");
		}

		[Test] public void IFRS_Parse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				//SendWarningsToConsole( s.errorList );
			}

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			TimeSpan level = new TimeSpan( 0, 0, 0, 4, 0 );	// 4 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long" );

			PresentationLink pl = s.presentationInfo["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified"] as PresentationLink;
			Assert.IsNotNull( pl, "can't find Balance Sheet, Classified Format" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "ifrs-gp_HedgingInstrumentsNonCurrentLiability", out ploc ) );

			// problem with presentation linkbase - don't know the solution yet
			Assert.AreEqual( 0, errors, "parse failure" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			// 1712 = calculation warnings

			//Trace.Listeners.Clear();
		}

		[Test] public void IFRS_ElementTaxonomyLinks()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			s.currentLanguage = "en";
			s.currentLabelRole = "label";

			ArrayList nodes = s.GetNodesByElement();

			Assert.AreEqual( 0, ((Node)nodes[0]).TaxonomyInfoId, "Taxonomy id not correct" );

			TaxonomyItem ti = s.GetTaxonomyInfo( (Node)nodes[0] );

			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-06-15", ti.WebLocation, "target namespace wrong" );

			Assert.AreEqual( IFRS_FILE, ti.Location, "targetLocation wrong" );
		}


		#region Elements
		[Test] public void IFRS_BindElements()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			Presentation p = s.LoadPresentationSchema( out errors );
			//Assert.AreEqual( 22, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindElements( new BindElementDelegate( s.BindElementToLocator ), out errors );

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );
		}

		[Test] public void IFRS_ReadElements()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			s.Load( IFRS_FILE );

			int numElements = s.LoadElements( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of errors" );
			Assert.AreEqual( 3223, s.allElements.Count, "wrong number of elements" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );
		}

		#endregion

		[Test] public void IFRS_GetLabelRoles()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

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

		[Test] public void IFRS_GetSupportedLanguages()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE);
			
			int errors = 0;
			ArrayList langs = s.GetSupportedLanguages( false, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		[Test] public void IFRS_TargetNamespace()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			s.Load( IFRS_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-06-15", s.targetNamespace );
		}

		[Test] public void IFRS_LoadSchema()
		{
			TestTaxonomy_IFRS_2004_06_15 s = new TestTaxonomy_IFRS_2004_06_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US IFRS File" );
		}
		#endregion
	}
}
#endif
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
	using System.IO;
	using System.Xml;
	using System.Text;
	using System.Collections;
	using System.Diagnostics;

    using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture]
    public class TestTaxonomy_IFRS : Taxonomy
    {
		/// <exclude/>
		protected Hashtable tupleElements = new Hashtable(1000);
		
		#region Overrides
//		public void TestParse( out int errors )
//		{
//			errors = 0;
//			ParseInternal( out errors );
//		}

		#endregion

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {
			Console.WriteLine( "***Start TestTaxonomy_IFRS Comments***" );
			
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {
			Trace.Listeners.Clear();
			Console.WriteLine( "***End TestTaxonomy_IFRS Comments***" );
		}

		///<exclude/>
        [SetUp] public void RunBeforeEachTest()
        {}

        ///<exclude/>
        [TearDown] public void RunAfterEachTest() 
        {}
		#endregion

		// ignore - this is xbrl 2.0
		//string US_GAAP_FILE = @"q:\bin\TestSchemas\US GAAP CI 2003-07-07\us-gaap-ci-2003-07-07.xsd";

		string IFRS_06_15_FILE = TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2004-06-15.xsd";
		string IFRS_FILE = TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2004-11-15.xsd";
		string IFRS_FILE_01_15 = TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2005-01-15.xsd";
	 string IFRS_OUT_FILE = TestCommon.FolderRoot + @"aucent-ifrs_gp.xml";
		 string NODE_OUT_FILE = TestCommon.FolderRoot + @"aucent-ifrs_gp-nodes.xml";
		string IFRS_2005_05_15_RC4_FILE = TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"2005_05_15_RC4" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-ALL-2005-05-15.xsd";
		string IFRS_SPANISH_2005_04_11_FILE = TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"2005_04_11_Spanish" +System.IO.Path.DirectorySeparatorChar +"es-be-fs-2005-04-11.xsd";

		#region Helpers
		/// <exclude/>
		protected void RecurseVerifyTuples(Node n)
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

		/// <exclude/>
		protected void RecurseElementsForTuples(Element e)
		{
			if ( e.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", e.Name );
			}

			if ( e.HasChildren )
			{
				foreach ( Element c in e.TupleChildren.GetValueList() )
				{
					RecurseElementsForTuples( c );
				}
			}
		}

		/// <exclude/>
		protected void SendErrorsToConsole(ArrayList errorList)
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

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList, string filter)
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
	    /// <exclude/>
		[Test] 
		public void Test_LoadRoleTypes()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		#region UI API's
		/// <exclude/>
		[Test]
		[Ignore("Just used to view nodes")]
		public void IFRS_Test_GetNodesByPresentation()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		[Ignore("Display only")]
		public void IFRS_Test_GetNodesByElement()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void VerifyTupleOfTuples()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( IFRS_06_15_FILE );
			
			int errors = 0;

			s.Parse( out errors );

			// find EquityCompensationPlan - it's a tuple, and should have ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod as a child
			// (it's also a tuple)

			Element e = s.allElements[ "ifrs-gp_EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find ifrs-gp_EquityCompensationPlan" );

			bool found = false;
			foreach ( Element ec in e.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "ifrs-gp_ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod" );

            Element ele = s.allElements["ifrs-gp_ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod"] as Element;
            Assert.IsNotNull(ele, "element should not be null");

            Assert.IsTrue(ele.HasTupleParents, "ifrs-gp_ShareOptionsExercisedUnderEquityCompensationPlansDuringPeriod should have tuple parents ");

			// find RelatedParty - it's a tuple, and should have TransactionRelatedParty as a child
			// (it's also a tuple)

			e = s.allElements[ "ifrs-gp_RelatedParty" ] as Element;
			Assert.IsNotNull( e, "can't find ifrs-gp_RelatedParty" );

			found = false;
			foreach ( Element ec in e.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "ifrs-gp_TransactionRelatedParty" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ifrs-gp_TransactionRelatedParty" );
            ele = s.allElements["ifrs-gp_TransactionRelatedParty"] as Element;
            Assert.IsNotNull(ele, "element should not be null");

            Assert.IsTrue(ele.HasTupleParents, "ifrs-gp_TransactionRelatedParty should have tuple parents ");

			// find EquityCompensationPlan - it's a tuple, and should have EquityInstrumentIssuedEquityCompensationPlan  as a child
			// (it's also a tuple)

			e = s.allElements[ "ifrs-gp_EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find ifrs-gp_EquityCompensationPlan" );

			found = false;
			foreach ( Element ec in e.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "ifrs-gp_EquityInstrumentIssuedEquityCompensationPlan" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ifrs-gp_EquityInstrumentIssuedEquityCompensationPlan " );
            ele = s.allElements["ifrs-gp_EquityInstrumentIssuedEquityCompensationPlan"] as Element;
            Assert.IsNotNull(ele, "element should not be null");

            Assert.IsTrue(ele.HasTupleParents, "ifrs-gp_EquityInstrumentIssuedEquityCompensationPlan should have tuple parents ");

			// find EquityCompensationPlan - it's a tuple, and should have MajorClassAssetLiabilityEntityAcquired as a child
			// (it's also a tuple)

			e = s.allElements[ "ifrs-gp_Acquisition" ] as Element;
			Assert.IsNotNull( e, "can't find ifrs-gp_Acquisition" );

			found = false;
			foreach ( Element ec in e.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "ifrs-gp_MajorClassAssetLiabilityEntityAcquired" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ifrs-gp_MajorClassAssetLiabilityEntityAcquired " );
            ele = s.allElements["ifrs-gp_MajorClassAssetLiabilityEntityAcquired"] as Element;
            Assert.IsNotNull(ele, "element should not be null");

            Assert.IsTrue(ele.HasTupleParents, "ifrs-gp_MajorClassAssetLiabilityEntityAcquired should have tuple parents ");

	
			// find EquityCompensationPlan - it's a tuple, and should have OwnEquityInstrumentHeldEquityCompensationPlan as a child
			// (it's also a tuple)

			e = s.allElements[ "ifrs-gp_EquityCompensationPlan" ] as Element;
			Assert.IsNotNull( e, "can't find ifrs-gp_EquityCompensationPlan" );

			found = false;
			foreach ( Element ec in e.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "ifrs-gp_OwnEquityInstrumentHeldEquityCompensationPlan" )
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue( found, "could not find ifrs-gp_OwnEquityInstrumentHeldEquityCompensationPlan " );

            ele = s.allElements["ifrs-gp_OwnEquityInstrumentHeldEquityCompensationPlan"] as Element;
            Assert.IsNotNull(ele, "element should not be null");

			Assert.IsTrue( ele.HasTupleParents , "ifrs-gp_OwnEquityInstrumentHeldEquityCompensationPlan should have tuple parents ");
		}

		/// <exclude/>
		[Test]
		[Ignore("display only")]
		public void IFRS_Test_VerifyTuples()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void IFRS_TestTuples()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void IFRS_GetTargetPrefix()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.Load( IFRS_FILE );

			Assert.AreEqual( "ifrs-gp", s.GetNSPrefix() );
		}

		/// <exclude/>
		[Test]
		[Ignore("Display only")]
		public void IFRS_OutputTaxonomy()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		[Ignore("Display only")]
		public void IFRS_OutputTaxonomyByNodes()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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
		/// <exclude/>
		[Test]
		[Ignore("Display only")]
		public void IFRS_VerifyPresentationCorrect()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.Load( IFRS_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			string rawXml = s.ToXmlString();

			s.currentLabelRole = "terseLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();
		}

#if OBSOLETE
		/// <exclude/>
        [Test] public void IFRS_VerifyPresentationTypes()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void IFRS_LoadPresentationSchema()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void IFRS_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US IFRS File" );
			
			string[] presRef = s.GetLinkbaseReference( TestTaxonomy_IFRS.TARGET_LINKBASE_URI + TestTaxonomy_IFRS.PRESENTATION_ROLE );

			Assert.AreEqual( TestCommon.FolderRoot + @"ifrs" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2004-11-15-presentation.xml", presRef[0] );
		}
				
		#endregion

		#region Calculation
		/// <exclude/>
		[Test]
		public void IFRS_LoadCalculationSchema()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

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

		/// <exclude/>
		[Test]
		public void IFRS_BindCalculation()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			int errors = 0;

			if ( s.Load( IFRS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.Parse( out errors );

			s.numWarnings = errors = 0;
			s.BindPresentationCalculationElements( false, out errors );
			if ( errors != 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "Bind calc failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );
		}

		#endregion

		#region imports
		/// <exclude/>
		[Test]
		public void IFRS_LoadImports()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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
			Assert.IsTrue( s.allElements.ContainsKey( "ifrs-gp_WorkInProgress" ), "can't find ifrs-gp_WorkInProgress" );
		}

		/// <exclude/>
		[Test]
		public void IFRS_LoadImportsAndBind()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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
			s.BindPresentationCalculationElements( true, out errors );

			Assert.AreEqual( 0, errors, "wrong number of bind errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "ifrs-gp_WorkInProgress" ), "Can't find ifrs-gp_WorkInProgress" );
		}

		/// <exclude/>
		[Test]
		public void IFRS_GetDependantTaxonomies()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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

		/// <exclude/>
		[Test]
		public void IFRS_Parse_Label()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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

			Element el = s.allElements["ifrs-gp_WorkInProgress"] as Element;
			Assert.IsNotNull( el, "can't find ifrs-gp_WorkInProgress" );
			string labelString = string.Empty;
			el.TryGetLabel("en", "label", out labelString);
			Assert.IsTrue(labelString != "",  "label info is not populated");
		}

		/// <exclude/>
		[Test]
		public void IFRS_Parse()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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
			}

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			TimeSpan level = new TimeSpan( 0, 0, 0, 30, 0 );	// 30 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end-start) + " seconds"  );

			PresentationLink pl = s.presentationInfo["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified"] as PresentationLink;
			Assert.IsNotNull( pl, "can't find Balance Sheet, Classified Format" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "ifrs-gp_HedgingInstrumentsNonCurrentLiability", out ploc ) );

			// problem with presentation linkbase - don't know the solution yet
			Assert.AreEqual( 0, errors, "parse failure" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			// 1712 = calculation warnings
		}

		/// <exclude/>
		[Test]
		public void IFRS_ElementTaxonomyLinks()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
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

			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-11-15", ti.WebLocation, "target namespace wrong" );
			Assert.AreEqual( IFRS_FILE, ti.Location, "targetLocation wrong" );
		}


		#region Elements
		/// <exclude/>
		[Test]
		public void IFRS_BindElements()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			s.Load( IFRS_FILE );

			Presentation p = s.LoadPresentationSchema( out errors );
			//Assert.AreEqual( 22, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindPresentationCalculationElements( true, out errors );

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );
		}

		/// <exclude/>
		[Test]
		public void IFRS_ReadElements()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			int errors = 0;

			s.Load( IFRS_FILE );

			int numElements = s.LoadElements( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of errors" );
			Assert.AreEqual( 3796, s.allElements.Count, "wrong number of elements" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );
		}

		#endregion

		/// <exclude/>
		[Test]
		public void IFRS_GetLabelRoles()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.Load( IFRS_FILE );
			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList roles = s.GetLabelRoles(false, out errors);

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 4, roles.Count );
			Assert.AreEqual( "label", roles[0] );
			Assert.AreEqual( "periodEndLabel", roles[1] );
			Assert.AreEqual( "periodStartLabel", roles[2] );
			Assert.AreEqual( "restatedLabel", roles[3] );
			//Assert.AreEqual( "documentation", roles[3] );
		}

		/// <exclude/>
		[Test]
		public void IFRS_GetSupportedLanguages()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.Load( IFRS_FILE);
			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList langs = s.GetSupportedLanguages(false, out errors);

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		/// <exclude/>
		[Test]
		public void IFRS_TargetNamespace()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();

			s.Load( IFRS_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-11-15", s.targetNamespace );
		}

		/// <exclude/>
		[Test]
		public void IFRS_LoadSchema()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			Assert.AreEqual( true, s.Load( IFRS_FILE, out errors ), "Could not load US IFRS File" );
		}

		/// <exclude/>
		[Test]
		public void IFRS_LoadEnumerations()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			s.Load( IFRS_FILE );
			s.AllocateElementTable();

			s.enumTable = new Hashtable();

			s.LoadEnumerationsAndOtherExtendedDataTypes( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( "ifrs-gp", s.GetNSPrefix(), "prefix wrong" );

			Assert.AreEqual( 0, errors, "wrong number of errors" );
			Assert.AreEqual( 5, s.enumTable.Count, "wrong number of enumerations" );

			Enumeration e1 = s.enumTable["ifrs-gp:CostValuationItemType"] as Enumeration;
			Assert.IsNotNull( e1, "CostValuationItemType not found" );
			Assert.AreEqual( "xbrli:stringItemType", e1.RestrictionType, "CostValuationItemType restriction type is wrong" );
			Assert.AreEqual( 2, e1.Values.Count, "CostValuationItemType wrong number of items" );
			Assert.AreEqual( "Cost", e1.Values[0], "CostValuationItemType[0] is wrong" );
			Assert.AreEqual( "Valuation", e1.Values[1], "CostValuationItemType[1] is wrong" );

			Enumeration e4 = s.enumTable["ifrs-gp:RelatedPartyRelationshipTypeItemType"] as Enumeration;
			Assert.IsNotNull( e4, "RelatedPartyRelationshipTypeItemType not found" );
			Assert.AreEqual( "xbrli:stringItemType", e4.RestrictionType, "RelatedPartyRelationshipTypeItemType restriction type is wrong" );
			Assert.AreEqual( 7, e4.Values.Count, "RelatedPartyRelationshipTypeItemType wrong number of items" );
			Assert.AreEqual( "Parent", e4.Values[0], "RelatedPartyRelationshipTypeItemType[0] is wrong" );
			Assert.AreEqual( "JointControlOrSignificantInfluence", e4.Values[1], "RelatedPartyRelationshipTypeItemType[1] is wrong" );
			Assert.AreEqual( "Subsidiary", e4.Values[2], "RelatedPartyRelationshipTypeItemType[2] is wrong" );
			Assert.AreEqual( "Associate", e4.Values[3], "RelatedPartyRelationshipTypeItemType[3] is wrong" );
			Assert.AreEqual( "JointVenture", e4.Values[4], "RelatedPartyRelationshipTypeItemType[4] is wrong" );
			Assert.AreEqual( "KeyManagementPersonnel", e4.Values[5], "RelatedPartyRelationshipTypeItemType[5] is wrong" );
			Assert.AreEqual( "OtherRelatedParty", e4.Values[6], "RelatedPartyRelationshipTypeItemType[6] is wrong" );
		}

		#region IFRS 2005-01-15
		/// <exclude/>
		[Test]
		public void IFRS_Parse_2005_01_15()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( IFRS_FILE_01_15, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			TimeSpan level = new TimeSpan( 0, 0, 0, 30, 0 );	// 30 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end-start) + " seconds"  );

			PresentationLink pl = s.presentationInfo["http://xbrl.iasb.org/int/fr/ifrs/gp/role/BalanceSheetClassified"] as PresentationLink;
			Assert.IsNotNull( pl, "can't find Balance Sheet, Classified Format" );

			// problem with presentation linkbase - don't know the solution yet
			Assert.AreEqual( 0, errors, "parse failure" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			// 1712 = calculation warnings

			Console.WriteLine( "ifrs_2005_01_15 elements: " + s.AllElements.Count );
		}

		/// <exclude/>
		[Test]
		public void IFRS_Parse_2005_01_15_GetNodes()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			if ( s.Load( IFRS_FILE_01_15, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Console.WriteLine( "ifrs_2005_01_15 elements: " + s.AllElements.Count );

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage	= "en";

			ArrayList nodes = s.GetNodesByPresentation();
		}

		#endregion
	
		#region IFRS 2005-05-15 RC 4
		/// <exclude/>
		[Test]
		public void IFRS_RC4_Parse()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			if ( s.Load( IFRS_2005_05_15_RC4_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//now check the taxonomy properties
			//there should be 3 taxonomy infos
			Assert.AreEqual(4, s.infos.Count, "wrong number of taxonomy infos");

			//there should be 0 warnings and 0 errors
			Assert.AreEqual(0, s.ValidationWarnings.Count, "warnings not 0");
			Assert.AreEqual(0, s.ValidationErrors.Count, "errors not 0");
			
			//there should be 4106 elements
			Assert.AreEqual(4106, s.allElements.Count, "wrong number of elements");

			//there should be 5 enumerations + 2 boolean
			Assert.AreEqual(5+2, s.enumTable.Count, "wrong number of enumerations");

			//there should be 19 calculation files and 26 top level nodes
			Assert.AreEqual(19, s.calculationFile.Length, "wrong number of calculation files");
			Assert.AreEqual(26, s.calculationInfo.Count, "wrong number of top level calculation nodes");

			//there should be 21 presentation files and 23 top level nodes
			Assert.AreEqual(21, s.presentationFile.Length, "wrong number of presentation files");
			Assert.AreEqual(23, s.presentationInfo.Count, "wrong number of top level presentation nodes");

			//there should be 4 label roles and 4106 labels
			Assert.AreEqual(4, s.labelRoles.Count, "wrong number of label roles");
			Assert.AreEqual(4106, s.labelHrefHash.Count, "wrong number of labels");

			//there should be 4106 references
			Assert.AreEqual(4106, s.referenceTable.Count, "wrong number of references");
		}
		#endregion

		#region IFRS Spanish 2005-04-11
		/// <exclude/>
		[Test]
		public void IFRS_Spanish_2005_04_11_Parse()
		{
			TestTaxonomy_IFRS s = new TestTaxonomy_IFRS();
			int errors = 0;

			if ( s.Load( IFRS_SPANISH_2005_04_11_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//now check the taxonomy properties
			//there should be 2 taxonomy infos
			Assert.AreEqual(2, s.infos.Count, "wrong number of taxonomy infos");

			//there should be 0 warnings and 0 errors
			Assert.AreEqual(0, s.ValidationWarnings.Count, "warnings not 0");
			Assert.AreEqual(0, s.ValidationErrors.Count, "errors not 0");
			
			//there should be 4318 elements
			Assert.AreEqual(4318, s.allElements.Count, "wrong number of elements");

			//there should be 5 enumerations + 2 boolean
			Assert.AreEqual(5+2, s.enumTable.Count, "wrong number of enumerations");

			//there should be 1 calculation file and 32 top level nodes
			Assert.AreEqual(1, s.calculationFile.Length, "wrong number of calculation files");
			Assert.AreEqual(32, s.calculationInfo.Count, "wrong number of top level calculation nodes");

			//there should be 1 presentation file and 31 top level nodes
			Assert.AreEqual(1, s.presentationFile.Length, "wrong number of presentation files");
			Assert.AreEqual(31, s.presentationInfo.Count, "wrong number of top level presentation nodes");

			//there should be 8 label roles and 4473 labels
			Assert.AreEqual(8, s.labelRoles.Count, "wrong number of label roles");
			Assert.AreEqual(4318, s.labelHrefHash.Count, "wrong number of labels");

			//there should be 4250 references
			Assert.AreEqual(4250, s.referenceTable.Count, "wrong number of references");
		}
		#endregion
	}
}
#endif
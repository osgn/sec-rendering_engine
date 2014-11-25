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
	using System.Collections;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_QCOM_20060326 : Taxonomy
	{
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
		}

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string DOW_FILE = TestCommon.FolderRoot + @"QCOM-20060326" +System.IO.Path.DirectorySeparatorChar +"qcom-20060326.xsd";

		#region helpers
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
		protected int SendWarningsToConsole(ArrayList errorList)
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Warning )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
					++numwarnings;
				}
			}

			return numwarnings;
		}

		/// <exclude/>
		protected void SendInfoToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Info )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
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

		/// <summary>
		/// Summary description for TestTaxonomy_MorganStanley.
		/// </summary>
		public TestTaxonomy_QCOM_20060326()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		public void QCOM_LoadAndParse()
		{
			TestTaxonomy_QCOM_20060326 s = new TestTaxonomy_QCOM_20060326();

			int errors = 0;

			if ( s.Load( DOW_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			SendWarningsToConsole( s.errorList );
			SendInfoToConsole( s.ErrorList );

			Assert.AreEqual( 0, errors, "errors returned from parse" );

			s.currentLanguage = "en";
			s.currentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByCalculation();

			Node incomeStmt= nodes[3] as Node;
			Assert.IsNotNull( incomeStmt, "incomeStmt is null" );
			Assert.AreEqual( 3, incomeStmt.Children.Count, "wrong number of children" );
			Assert.AreEqual( "Income Statement", incomeStmt.Label, "node 3 wrong name" );
			Assert.IsTrue( incomeStmt.ElementIsNull, "incomeStmt.MyElement is not null");

			Node netIncomeAppl= incomeStmt.Children[2] as Node;
			CheckElement( netIncomeAppl, "Net Income Applicable to Common Stockholders", 2, false ); 

			Node netIncome = netIncomeAppl.Children[0] as Node;
			CheckElement( netIncome, "Net income", 2, false ); 

			Node incomeBeforeCum = netIncome.Children[0] as Node;
			CheckElement( incomeBeforeCum, "Income/(Loss) Before Cumulative Effect of Change in Accounting Principle", 2, false ); 

			Node incomeBeforeEx = incomeBeforeCum.Children[0] as Node;
			CheckElement( incomeBeforeEx, "Income/(Loss) Before Extraordinary Items and Cumulative Effect of Change in Accounting Principle", 4, false );

			Node incomeLossCont = incomeBeforeEx.Children[0] as Node;
			CheckElement( incomeLossCont, "Income/(Loss) from Continuing Operations", 2, false );

			Node incomeLossContTaxes = incomeLossCont.Children[0] as Node;
			CheckElement( incomeLossContTaxes, "Income before income taxes", 3, false );

			Node operatingIncome = incomeLossContTaxes.Children[1] as Node;
			CheckElement( operatingIncome, "Operating income", 4, false );

			Node grossProfit = operatingIncome.Children[2] as Node;
			CheckElement( grossProfit, "Gross Profit", 2, false );

			Node costGoodsServicesSold = grossProfit.Children[1] as Node;
			CheckElement( costGoodsServicesSold, "Cost of equipment and services revenues", 3, true );

		}

		/// <exclude/>
		protected void CheckElement(Node element, string expectedLabel, int numChildren, bool isprohibited)
		{
			Assert.IsNotNull( element, expectedLabel + " is null" );
			Assert.AreEqual( numChildren, element.Children.Count, "wrong number of children" );
			Assert.AreEqual( expectedLabel, element.Label, "label wrong" );
			Assert.AreEqual( isprohibited, element.IsProhibited, expectedLabel + " prohibited wrong" );
		}
	}
}
#endif
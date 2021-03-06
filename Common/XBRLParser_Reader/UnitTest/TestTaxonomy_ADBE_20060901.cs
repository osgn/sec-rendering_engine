// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the �License�); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an �AS IS� basis, WITHOUT WARRANTY OF ANY KIND, 
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
	public class TestTaxonomy_ADBE_20060901 : Taxonomy
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

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{}
		#endregion

		string ADBE_FILE = TestCommon.FolderRoot + @"ADBE-20060901" +System.IO.Path.DirectorySeparatorChar +"ADBE-20060901.xsd";

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

		/// <exclude/>
		public TestTaxonomy_ADBE_20060901()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		public void ADBE_LoadAndParse()
		{
			TestTaxonomy_ADBE_20060901 s = new TestTaxonomy_ADBE_20060901();

			int errors = 0;

			if ( s.Load( ADBE_FILE, out errors ) != true )
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

			ArrayList nodes = s.GetNodesByPresentation();

			Node statements = nodes[5] as Node;
			Assert.IsNotNull( statements, "statements is null" );
			Assert.AreEqual( "Statement of Financial Position", statements.Label, "node 4 wrong name" );
			Assert.IsTrue( statements.ElementIsNull, "statements.MyElement is not null");

			Node statement = statements.Children[0] as Node;
			Assert.IsNotNull( statement, "statement is null" );
			Assert.IsFalse( statement.ElementIsNull, "statement.MyElement is null");

			Node assets = statement.Children[0] as Node;
			Assert.IsNotNull( assets, "assets is null" );
			Assert.AreEqual( 8, assets.Children.Count, "assets has wrong number of children" );

			Node assetsCurrent = assets.Children[0] as Node;
			Assert.IsNotNull( assetsCurrent, "assetsCurrent is null" );
			Assert.AreEqual( 18, assetsCurrent.Children.Count, "assetsCurrent has wrong number of children" );

			Node deferredIncomeTaxes = assetsCurrent.Children[5] as Node;
			Assert.IsNotNull( deferredIncomeTaxes, "deferredIncomeTaxes is null" );
			Assert.IsFalse( deferredIncomeTaxes.IsProhibited, "deferredIncomeTaxes is prohibited" );

			Node prohDeferredIncomeTaxes = assetsCurrent.Children[12] as Node;
			Assert.IsNotNull( prohDeferredIncomeTaxes, "proh deferredIncomeTaxes is null" );
			Assert.IsTrue( prohDeferredIncomeTaxes.IsProhibited, "proh deferredIncomeTaxes is not prohibited" );

			// and check one that's working
			Node propplantequip = assets.Children[4] as Node;
			Assert.IsNotNull( propplantequip, "propplantequip is null" );

			Assert.IsNotNull( propplantequip.IsProhibited, "propplantequip is null" );

			Assert.IsFalse( deferredIncomeTaxes.IsProhibited, "deferredIncomeTaxes is prohibited" );

			// try and get again
			ArrayList nodesAgain = s.GetNodesByPresentation();

			Node statementsAgain = nodesAgain[5] as Node;
			Assert.IsNotNull( statementsAgain, "statementsAgain is null" );
			Assert.AreEqual( "Statement of Financial Position", statementsAgain.Label, "statementsAgain node 4 wrong name" );
			Assert.IsTrue( statementsAgain.ElementIsNull, "statementsAgain.MyElement is not null");

			Node statementAgain = statementsAgain.Children[0] as Node;
			Assert.IsNotNull( statementAgain, "statementAgain is null" );
			Assert.IsFalse( statementAgain.ElementIsNull, "statementAgain.MyElement is null");

			Node assetsAgain = statementAgain.Children[0] as Node;
			Assert.IsNotNull( assetsAgain, "assets is null" );
			Assert.AreEqual( 8, assetsAgain.Children.Count, "assets has wrong number of children" );
			Assert.IsFalse( assetsAgain.ElementIsNull, "assetsAgain.MyElement is null");

//	this may be correct - commented out until we figure out how things should work
//
//			string text = null;
//			cashcashequiv.TryGetLabel( "en", "preferredLabel", out text );
//			Assert.AreEqual( "Cash and Cash Equivalents", text, "cashcasheqiv label incorrect" );
//
//			Assert.AreEqual( "Cash and Cash Equivalents", cashcashequiv.Label, "cashcasheqiv label incorrect" );

//			Node acctReceiveNet = assets.Children[1] as Node;
//
//			acctReceiveNet.MyElement.PreferredLabel = null;
//			acctReceiveNet.SetLabel( "en", "preferredLabel" );
//			acctReceiveNet.TryGetLabel( "en", "preferredLabel", out text );
//			Assert.AreEqual( "Dues receivable, net of allowance of $12,445 and $2,500", text, "acctReceiveNet label incorrect" );
//			
//			Assert.IsNotNull( acctReceiveNet, "acctReceiveNet is null" );
//			Assert.AreEqual( "Dues receivable, net of allowance of $12,445 and $2,500", acctReceiveNet.Label, "acctRecieveNet.Label is wrong" );

		}

	}
}
#endif
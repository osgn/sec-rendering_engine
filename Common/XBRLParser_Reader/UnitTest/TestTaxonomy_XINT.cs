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
	public class TestTaxonomy_XINT : Taxonomy
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

		string XINT_FILE = TestCommon.FolderRoot + @"xint" +System.IO.Path.DirectorySeparatorChar +"XBRLInternationalTaxonomy.xsd";

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
		public TestTaxonomy_XINT()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		public void XINT_LoadAndParse()
		{
			TestTaxonomy_XINT s = new TestTaxonomy_XINT();

			int errors = 0;

			if ( s.Load( XINT_FILE, out errors ) != true )
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

			Node xbrlInt = nodes[7] as Node;

			Assert.IsNotNull( xbrlInt, "XBRL International is null");
			Assert.AreEqual( 5, xbrlInt.Children.Count, "wrong number of children" );

			Node statements = xbrlInt.Children[4] as Node;
			Assert.IsNotNull( statements, "statements is null" );
			Assert.AreEqual( 4, statements.Children.Count, "statements has wrong number of children" );

			Node assets = statements.Children[0] as Node;
			Assert.IsNotNull( assets, "assets is null" );
			Assert.AreEqual( 7, assets.Children.Count, "assets has wrong number of children" );

			Node cashcashequiv = assets.Children[0] as Node;
			Assert.IsNotNull( cashcashequiv, "cash cash eqivalents is null" );

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

		/// <exclude/>
		[Test]
		public void XINT_LoadAndParse_Calculation()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_XINT s = new TestTaxonomy_XINT();

			int errors = 0;

			if ( s.Load( XINT_FILE, out errors ) != true )
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
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.ErrorList );
			}

			Assert.AreEqual( 0, errors, "errors returned from parse" );

			s.currentLabelRole = "preferredLabel";
			s.currentLanguage  = "en";

			errors = 0;
			ArrayList nodes = s.GetNodes(PresentationStyle.Presentation, false, out errors);


			Assert.IsTrue( errors == 0, "errors returned from calculation" );
		}
	}
}
#endif
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
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_EDGAROnline : Taxonomy
	{
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

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string EDGARONLINE_FILE = TestCommon.FolderRoot + @"EDGAROnline" +System.IO.Path.DirectorySeparatorChar +"edgr-20050228.xsd";
		string ICI_FILE = TestCommon.FolderRoot + @"ici" +System.IO.Path.DirectorySeparatorChar +"ici-rr.xsd";

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
		#endregion

		/// <exclude/>
		public TestTaxonomy_EDGAROnline()
		{
		}

		/// <exclude/>
		[Test]
		public void EDGAROnline_LoadAndParsePresentation()
		{
			TestTaxonomy_EDGAROnline s = new TestTaxonomy_EDGAROnline();
			int errors = 0;

			DateTime start = DateTime.Now;
			errors = s.Load( EDGARONLINE_FILE, false );
			if (  errors != 0 )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first
			s.Parse( out errors );

			// there should be 1 error because there's a presentation arc that would create an infinite recursion
			Assert.AreEqual( 1, errors, "wrong number of errors in presentation" );
			
			//s.ErrorList.Sort();

			Console.WriteLine( ((ParserMessage)s.ErrorList[0]).Message );

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );
			TimeSpan level = new TimeSpan( 0, 0, 0, 10, 0 );	// 10 seconds to parse

			Assert.AreEqual(9, s.presentationInfo.Count, "wrong number of presentation links" );
			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/us/fr/lr/role/IncomeStatement" ] as PresentationLink;
			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/us/fr/lr/role/IncomeStatement from presentation" );

			Node parentNode = pl.CreateNode( "en", "label" );
			Assert.AreEqual(2, parentNode.Children.Count, "wrong number of children in presentation" );
			Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end-start) + " seconds"  );
		}

		/// <exclude/>
		[Test]
		public void EDGAROnline_LoadAndParseCalculation()
		{
			for ( int i = 0; i < 10; i++ )
			{
				TestTaxonomy_EDGAROnline s = new TestTaxonomy_EDGAROnline();
				int errors = 0;

				DateTime start = DateTime.Now;
				if ( s.Load( EDGARONLINE_FILE, out errors ) != true )
				{
					Assert.Fail( (string)s.ErrorList[0]);
				}

				errors = 0;

				// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
				s.Parse( out errors );

				// there should be 2 error because there's a calculation arc that would create an infinite recursion
				Assert.AreEqual( 1, errors, "wrong number of errors in calculation" );
			
				DateTime end = DateTime.Now;
				Console.WriteLine( "Parse Time: {0}", end-start );
				TimeSpan level = new TimeSpan( 0, 0, 0, 10, 0 );	// 10 seconds to parse
				Assert.AreEqual(10, s.calculationInfo.Count, "wrong number of calculation links" );
				PresentationLink cl = s.calculationInfo[ "http://www.xbrl.org/us/fr/lr/role/CashFlowOperationsDirect" ] as PresentationLink;
				Assert.IsNotNull( cl, "can't get http://www.xbrl.org/us/fr/lr/role/CashFlowOperationsDirect from calculation" );

				Node parentNode = cl.CreateNode( "en", "label" );
				Assert.AreEqual(1, parentNode.Children.Count, "wrong number of children in calculation" );

				s.currentLabelRole = @"preferredLabel";
				s.currentLanguage = @"en";
				ArrayList temp = s.GetNodesByCalculation();

				Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end-start) + " seconds"  );

			}

		}

		/// <exclude/>
		[Test, Ignore("just verifies load")]
		public void Test_LoadAndParsePresentation_GCDA_Growth_Fund()
		{
			TestTaxonomy_EDGAROnline s = new TestTaxonomy_EDGAROnline();
			int errors = 0;
            string fileName = TestCommon.FolderRoot + @"GCDA_Growth_Fund" +System.IO.Path.DirectorySeparatorChar +"GComFunds.xsd";

			DateTime start = DateTime.Now;
			if ( s.Load( fileName, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole ="preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse( out errors );

			ArrayList nodes = s.GetNodesByPresentation();

			foreach ( Node n in nodes )
			{
				StringBuilder sb = DisplayNode( n, 0 );
				Console.WriteLine( sb.ToString() );
			}
		}

		/// <exclude/>
		public static StringBuilder DisplayNode(Node n, int level)
		{
			StringBuilder sb = new StringBuilder();

			for ( int i=0; i < level; ++i )
			{
				sb.Append( " " );
			}

			sb.Append( n.Label ).Append( Environment.NewLine );

			if ( n.Children != null )
			{
				foreach ( Node c in n.Children )
				{
					sb.Append( DisplayNode( c, level+1 ) );
				}
			}

			return sb;
		}

	}
}
#endif
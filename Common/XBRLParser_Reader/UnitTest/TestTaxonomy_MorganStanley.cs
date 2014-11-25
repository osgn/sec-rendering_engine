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
	public class TestTaxonomy_MS : Taxonomy
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

		string MS_FILE = TestCommon.FolderRoot + @"Morgan Stanley" +System.IO.Path.DirectorySeparatorChar +"MorganStanleyEarningsReleaseTaxonomy.xsd";

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
		public TestTaxonomy_MS()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		public void MS_RoleTest()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_MS s = new TestTaxonomy_MS();

			int errors = 0;

			if ( s.Load( MS_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			s.LoadImports( out errors );

			Assert.AreEqual( 0, errors, "load imports failed" );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			errors = 0;
			s.LoadPresentation( out errors );

//			s.Parse( out errors );
//
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				SendWarningsToConsole( s.errorList );
			}
			Assert.AreEqual( 0, errors, "load presentation failed" );

			errors = 0;
			s.LoadElements( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				//				//SendWarningsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "load elements failed" );

			s.BindPresentationCalculationElements( true, out errors );

			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/2003/role/link" ] as PresentationLink;

			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/2003/role/link" );

			Node parentNode = pl.CreateNode( "en", "label" );

			Assert.AreEqual( 7, parentNode.Children.Count, "wrong number of children" );
		}

		/// <exclude/>
		[Test]
		public void MS_LoadAndParse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_MS s = new TestTaxonomy_MS();

			int errors = 0;

			if ( s.Load( MS_FILE, out errors ) != true )
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
			}

			Assert.AreEqual( 0, errors, "parse failed" );

			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/2003/role/link" ] as PresentationLink;

			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/2003/role/link" );

			Node parentNode = pl.CreateNode( "en", "label" );

			Assert.AreEqual(7, parentNode.Children.Count, "wrong number of children" );

//			parentNode.Children.Sort();
//
//			Node earningsReleaseNode = parentNode.Children[13] as Node;
//
//			string text = null;
//			earningsReleaseNode.MyElement.TryGetLabel( "en", "label", out text );
//			Assert.AreEqual( "Earnings Release", text, "TryGetLabel wrong" );
//
//			Assert.AreEqual( "Earnings Release", earningsReleaseNode.Label, "label wrong" );
//
//			Node earningsReleaseText = earningsReleaseNode.Children[0] as Node;
//
//			earningsReleaseText.TryGetLabel( "en", "label", out text );
//			Assert.AreEqual( "Earnings Release Text", text, "Earnings Release Text label is wrong" );
		}
	}
}
#endif
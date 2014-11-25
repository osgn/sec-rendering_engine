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
	public class TestTaxonomy_OldMutual : Taxonomy
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

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string OLDMUTUAL_FILE = TestCommon.FolderRoot + @"OldMutual" +System.IO.Path.DirectorySeparatorChar +"omaf-20060131.xsd";

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
		public TestTaxonomy_OldMutual()
		{
			// TODO: Add constructor logic here
		}

		/// <exclude/>
		[Test]
		public void OldMutual_LoadAndParse()
		{
			TestTaxonomy_OldMutual s = new TestTaxonomy_OldMutual();
			int errors = 0;

			if ( s.Load( OLDMUTUAL_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			errors = 0;
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.ErrorList );
			}
			Assert.AreEqual( 0, errors, "parse failed due to bad reference loader errors" );
			
			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Node title = nodes[0] as Node;
			Assert.IsNotNull( title, "title is null" );
			Assert.AreEqual( "Financial Highlights", title.Label, "title label is wrong" );
			Assert.AreEqual( 0, title.Order, "title.Order is wrong" );
			Assert.AreEqual( 1, title.Children.Count, "title.Children.Count is wrong" );

			Node operatingPerformance = title.Children[0] as Node;
			Assert.IsNotNull( operatingPerformance, "operatingPerformance is null" );
			Assert.AreEqual( "Operating Performance", operatingPerformance.Label, "operatingPerformance label is wrong" );
			Assert.AreEqual( 0, operatingPerformance.Order, "operatingPerformance.Order is wrong" );

			ArrayList children = new ArrayList();
			foreach (Node n in operatingPerformance.Children)
			{
				if (n.IsProhibited) continue;

				children.Add(n);
			}
			// don't count the prohibited - they've been superceded (by priority) with the optional element 
			Assert.AreEqual(12, children.Count, "operatingPerformance.Children.Count is wrong");

			Node distribPerShare = children[4] as Node;
			Assert.IsNotNull( distribPerShare, "distribPerShare is null" );
			Assert.AreEqual( "Distributions Per Share", distribPerShare.Label, "distribPerShare label is wrong" );
			Assert.AreEqual( 3, distribPerShare.Order, "distribPerShare.Order is wrong" );

			children = new ArrayList();
			foreach (Node n in distribPerShare.Children)
			{
				if (n.IsProhibited) continue;

				children.Add(n);
			}
			Assert.AreEqual(6, children.Count, "distribPerShare.Children.Count is wrong");

			Node distribFromGainsPerShare = children[1] as Node;
			Assert.IsNotNull( distribFromGainsPerShare, "distribFromGainsPerShare is null" );
			Assert.AreEqual( "Distributions From Realized and Unrealized Gains Per Share", distribFromGainsPerShare.Label, "distribFromGainsPerShare label is wrong" );
			Assert.AreEqual( 2, distribFromGainsPerShare.Order, "distribFromGainsPerShare.Order is wrong" );
			children = new ArrayList();
			foreach (Node n in distribFromGainsPerShare.Children)
			{
				if (n.IsProhibited) continue;

				children.Add(n);
			}
			Assert.AreEqual(4, children.Count, "distribFromGainsPerShare.Children.Count is wrong");
		}
	}
}
#endif
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
	public class TestTaxonomy_UNH_20071231 : Taxonomy
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			Common.MyTraceSwitch = new TraceSwitch("Common", "common trace switch");
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast()
		{
			Trace.Listeners.Clear();
		}

		///<exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{ }

		///<exclude/>
		[TearDown]
		public void RunAfterEachTest()
		{ }

		#endregion

		string UNH_FILE = TestCommon.FolderRoot + @"unh-20071231" + System.IO.Path.DirectorySeparatorChar + "unh-20071231.xsd";

		#region helpers
		/// <exclude/>
		protected void SendErrorsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level != TraceLevel.Error)
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
			}
		}

		/// <exclude/>
		protected int SendWarningsToConsole(ArrayList errorList)
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level == TraceLevel.Warning)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
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

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level == TraceLevel.Info)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
				}
			}
		}

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList, string filter)
		{
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Message.IndexOf(filter) < 0)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
				}
			}
		}

		#endregion

		#region tests

		/// <exclude/>
		[Test]
		public void IBM_LoadAndParse()
		{

			TestTaxonomy_UNH_20071231 taxonomy = new TestTaxonomy_UNH_20071231();

			int errors = 0;

			if (taxonomy.Load(UNH_FILE, out errors) != true)
			{
				Assert.Fail((string)taxonomy.ErrorList[0]);
			}

			try
			{
				errors = 0;

				// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
				taxonomy.PromptUser = false;
				taxonomy.Parse(out errors);

				// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
				if (errors > 0)
				{
					SendErrorsToConsole(taxonomy.errorList);
				}

				SendWarningsToConsole(taxonomy.errorList);
				SendInfoToConsole(taxonomy.ErrorList);
			}
			catch (Exception ex)
			{
				Assert.Fail("Exception thrown in IBM_LoadAndParse: " + ex.Message);
			}


			Assert.AreEqual(0, errors, "errors returned from parse");

			taxonomy.currentLanguage = "en";
			taxonomy.currentLabelRole = "preferredLabel";

			ArrayList nodes = taxonomy.GetNodesByPresentation();

			Node n1 = (Node)nodes[0];
			Assert.IsTrue(n1.ElementIsNull, "n1 element is not null");
			Assert.IsNotNull(n1.Children, "n1.children is null");

			Node n2 = (Node)n1.Children[0];
			Assert.IsFalse(n2.ElementIsNull, "n2 element is null");
			Assert.IsNotNull(n2.Children, "n2.children is null");

			Node n3 = (Node)n2.Children[0];
			Assert.IsFalse(n3.ElementIsNull, "n3 element is null");

			ArrayList nodesAgain = taxonomy.GetNodesByPresentation();

			Node n1Again = (Node)nodesAgain[0];
			Assert.IsTrue(n1Again.ElementIsNull, "n1Again element is not null");
			Assert.IsNotNull(n1Again.Children, "n1Again.children is null");

			Node n2Again = (Node)n1Again.Children[0];
			Assert.IsFalse(n2Again.ElementIsNull, "n2Again element is null");
			Assert.IsNotNull(n2Again.Children, "n2Again.children is null");

			Node n3Again = (Node)n2Again.Children[0];
			Assert.IsFalse(n3Again.ElementIsNull, "n3Again element is null");
		}

		#endregion
	}
}

#endif

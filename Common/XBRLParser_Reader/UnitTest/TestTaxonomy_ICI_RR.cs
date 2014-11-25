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
	public class TestTaxonomy_ICI_RR
	{
		private const string ICI_RR_SCHEMAFILE = "http://xbrl.ici.org/rr/2006/ici-rr.xsd";

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine("***Start TestTaxonomy_IFRS Comments***");

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
			Console.WriteLine("***End TestTaxonomy_IFRS Comments***");
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

		/// <exclude/>
		[Test]
		public void TestLoadICI_RR_Taxonomy()
		{
			Taxonomy iciTaxonomy = new Taxonomy();

			int numErrors = iciTaxonomy.Load(ICI_RR_SCHEMAFILE, false);
			Assert.AreEqual(0, numErrors, "Failed to load ICI-RR Taxonomy. " + numErrors + " errors were found");
			
			Assert.IsTrue(iciTaxonomy.Parse(out numErrors), "Failed to parse the ICI-RR Taxonomy: " + numErrors + " errors were found");
		}

		/// <exclude/>
		//This test should only be run manually in case the load fails.  Otherwise DT will prompt the user to select
		//the taxonomy files causing the build to hang
		[Test]
		[Ignore]
		public void TestLoadICI_RR_Taxonomy_WithPrompt()
		{
			Taxonomy iciTaxonomy = new Taxonomy();

			int numErrors;
			Assert.IsTrue(iciTaxonomy.Load(ICI_RR_SCHEMAFILE, out numErrors), "Failed to load ICI-RR Taxonomy. " + numErrors + " errors were found");

			Assert.IsTrue(iciTaxonomy.Parse(out numErrors), "Failed to parse the ICI-RR Taxonomy: " + numErrors + " errors were found");
		}
	}
}
#endif

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
	using System.Collections.Generic;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_DOW_20060331 : Taxonomy, ITaxonomyCache
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

		string DOW_FILE = TestCommon.FolderRoot + @"DOW-20060331" +System.IO.Path.DirectorySeparatorChar +"DOW-20060331.xsd";

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
		public TestTaxonomy_DOW_20060331()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		public void DOW_LoadAndParse()
		{
			TestTaxonomy_DOW_20060331 s = new TestTaxonomy_DOW_20060331();

			int errors = 0;

			if ( s.Load( DOW_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
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

			Node statements = nodes[2] as Node;
			Assert.IsNotNull( statements, "statements is null" );
			Assert.AreEqual( "Income Statement", statements.Label, "node 3 wrong name" );
			Assert.IsTrue( statements.ElementIsNull, "statements.MyElement is not null");

			Node statement = statements.Children[0] as Node;
			Assert.IsNotNull( statement, "statement is null" );
			Assert.IsFalse( statement.ElementIsNull, "statement.MyElement is null");

			Node revenue = statement.Children[0] as Node;
			Assert.IsNotNull( revenue, "revenue is null" );
			Assert.AreEqual( "Revenue", revenue.Label, "revenue wrong name" );
			Assert.AreEqual( 5, revenue.Children.Count, "assets has wrong number of children" );

			Node salesRevenueAbs = revenue.Children[0] as Node;
			Assert.IsNotNull( salesRevenueAbs, "salesRevenueAbs is null" );
			Assert.AreEqual( "Sales Revenue", salesRevenueAbs.Label, "revenue wrong name" );
			Assert.AreEqual( 4, salesRevenueAbs.Children.Count, "salesRevenueAbs has wrong number of children" );

//			Node netSalesProhibited = salesRevenueAbs.Children[2] as Node;
//			Assert.IsNotNull( netSalesProhibited, "netSalesProhibited is null" );
//			Assert.IsTrue( netSalesProhibited.IsProhibited, "netSalesProhibited is not prohibited" );

			Node netSales = salesRevenueAbs.Children[2] as Node;
			Assert.IsNotNull( netSales, "netSales is null" );
			Assert.IsFalse( netSales.IsProhibited, "netSales is prohibited" );


			Node operationExpensesAbstract = statement.Children[3] as Node;
			Assert.IsNotNull( operationExpensesAbstract, "operationExpensesAbstract is null" );
			Assert.AreEqual( "Operating Expenses", operationExpensesAbstract.Label, "operationExpensesAbstract wrong name" );
			Assert.AreEqual(12, operationExpensesAbstract.Children.Count, "operationExpensesAbstract has wrong number of children" );

			Node rdExpenseAbs = operationExpensesAbstract.Children[0] as Node;
			Assert.IsNotNull( rdExpenseAbs, "rdExpenseAbs is null" );
			Assert.AreEqual( "Research and Development Expense", rdExpenseAbs.Label, "rdExpenseAbs wrong name" );

			//old dragon tag bug.. we have the same element displaying twice one for order 3 and one for order 3.005
			Assert.AreEqual(6, rdExpenseAbs.Children.Count, "rdExpenseAbs has wrong number of children" );

			Node rdExpenseEx = rdExpenseAbs .Children[0] as Node;
			Assert.IsNotNull( rdExpenseEx, "rdExpenseEx is null" );
			Assert.AreEqual( "Research and Development Expense (Excluding In - Process)", rdExpenseEx.Label, "rdExpenseEx wrong name" );

			Node rdExpenseDev = rdExpenseAbs.Children[1] as Node;
			Assert.IsNotNull( rdExpenseDev, "rdExpenseDev is null" );
			Assert.AreEqual( "In - Process Research and Development", rdExpenseDev.Label, "rdExpenseDev wrong name" );

			Node rdExpenseInc = rdExpenseAbs.Children[5] as Node;
			Assert.IsNotNull( rdExpenseInc, "rdExpenseInc is null" );
			Assert.AreEqual( "Research and development expenses", rdExpenseInc.Label, "rdExpenseInc wrong name" );
			Assert.IsFalse( rdExpenseInc.IsProhibited, "rdExpenseInc is prohibited" );
			
//			Node rdExpenseInc2 = rdExpenseAbs.Children[3] as Node;
//			Assert.IsNotNull( rdExpenseInc2, "rdExpenseInc2 is null" );
//			Assert.AreEqual( "Research and development expenses", rdExpenseInc2.Label, "rdExpenseInc2 wrong name" );
//			Assert.IsTrue( rdExpenseInc2.IsProhibited, "rdExpenseInc2 is not prohibited" );


			foreach (Element ele in s.allElements.Values)
			{
				TaxonomyItem ti = s.TaxonomyItems[ele.TaxonomyInfoId];
				Assert.IsNotNull(ti, "taxonomy item does not exist");

				if (!ele.Id.StartsWith(ti.Namespace))
				{
					Assert.Fail("Failed to set the correct Taxonomy item id for element ");
				}
			}
		}


		/// <exclude/>
		[Test]
		public void Dow_ParseUsingCloneOfBaseTaxonomy()
		{
			DateTime start = DateTime.Now;

			DOW_LoadAndParse();

			DateTime end = DateTime.Now;

			Console.WriteLine("TIME TAKEN TO LOAD WITHOUT CACHE = {0}", end - start);

			string US_GAAP_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2005-02-28.xsd";

			Taxonomy  bt = new Taxonomy();
			bt.Load(US_GAAP_FILE);


			
			int errors = 0;

			bt.Parse(out errors);

			bt.CurrentLabelRole = "Label";
			bt.CurrentLanguage = "en";
			ArrayList nodeList = bt.GetNodesByPresentation();


			MemoryStream msWrite = new MemoryStream();
			BinaryFormatter formatterWrite = new BinaryFormatter();
			formatterWrite.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			formatterWrite.Serialize(msWrite, bt);

			byte[] bufOrig = msWrite.GetBuffer();
			
			ArrayList flatListOrig = new ArrayList();
			TestTaxonomy_US_GAAP_IM.GetFlatListOfPresentationNodes(nodeList, ref flatListOrig);
			Hashtable taxIdInfos = new Hashtable();
			for (int i = 0; i < flatListOrig.Count; i++)
			{
				Node orig = flatListOrig[i] as Node;

				if( orig.MyElement != null )
				{
					taxIdInfos[orig.Id] = orig.MyElement.TaxonomyInfoId;
				}
			}

			taxonomyCache[Path.GetFileName(US_GAAP_FILE)] = bt;


			Taxonomy.TaxonomyCacheManager = this;


			start = DateTime.Now;

			DOW_LoadAndParse();

			end = DateTime.Now;

			Console.WriteLine("TIME TAKEN TO LOAD WITH CACHE = {0}", end - start);

			ArrayList nodeListAfter = bt.GetNodesByPresentation();

			ArrayList flatListAfter = new ArrayList();
			TestTaxonomy_US_GAAP_IM.GetFlatListOfPresentationNodes(nodeListAfter, ref flatListAfter);



			Assert.AreEqual(flatListOrig.Count, flatListAfter.Count, "should not change");

			for (int i = 0; i < flatListAfter.Count; i++)
			{
				Node orig = flatListOrig[i] as Node;
				Node after = flatListAfter[i] as Node;


				Assert.AreEqual(orig.Label, after.Label, "Label should match");

				Assert.AreEqual(orig.Id, after.Id);
				if (after.MyElement != null)
				{
					Assert.AreEqual(taxIdInfos[after.Id], after.MyElement.TaxonomyInfoId);
				}

			}

			Taxonomy.TaxonomyCacheManager = null;

			msWrite = new MemoryStream();
			formatterWrite = new BinaryFormatter();
			formatterWrite.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			formatterWrite.Serialize(msWrite, bt);

			byte[] bufAfter = msWrite.GetBuffer();

			Assert.AreEqual(bufOrig.Length, bufAfter.Length, "Cache object has changed");



			MemoryStream msRead = new MemoryStream(bufOrig);
			BinaryFormatter formatterRead = new BinaryFormatter();
			formatterRead.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			Taxonomy origbt = formatterRead.Deserialize(msRead) as Taxonomy;


			Assert.AreEqual(origbt.AllElements.Count, bt.AllElements.Count);

	


			//for( int i = 0 ;i < bufAfter.Length ; i++ )
			//{
			//    if (bufAfter[i] != bufOrig[i])
			//    {
			//        Console.WriteLine("Position that is not correct = {0} out of total = {1}" ,i, bufAfter.Length);
			//    }
			//    Assert.AreEqual(bufAfter[i], bufOrig[i], "binary data is different");
			//}
			

		}


		

		private Hashtable taxonomyCache = new Hashtable();
		#region ITaxonomyCache Members
		/// <summary>
		/// implements the taxonomy manager object
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public Taxonomy GetTaxonomyByFileName(string fileName)
		{
			return taxonomyCache[fileName] as Taxonomy;


		}

		#endregion
	}
}
#endif
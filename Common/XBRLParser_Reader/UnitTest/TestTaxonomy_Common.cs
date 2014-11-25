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
	using System.Net;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_Common : Taxonomy
	{
		/// <exclude/>
		protected Hashtable tupleElements = new Hashtable(1000);
		
		#region Overrides
		/// <exclude/>
		public void TestParse(out int errors)
		{
			errors = 0;
			ParseInternal( out errors );
		}

		/// <exclude/>
		public new bool IsAucentExtension
		{
			get { return aucentExtension; }
			set { aucentExtension = value; }
		}

		/// <exclude/>
		public List<TaxonomyItem> Infos 
		{
			get { return infos; }
			set { infos = value; }
		}

		#endregion

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestTaxonomy_Common Comments***" );
			
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
			Console.WriteLine( "***End TestTaxonomy_Common Comments***" );
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

		// ignore - this is xbrl 2.0
		//string US_GAAP_FILE = @"q:\bin\TestSchemas\US GAAP CI 2003-07-07\us-gaap-ci-2003-07-07.xsd";

		string DUP_ID = TestCommon.FolderRoot + @"dup_id.xsd";
		/*string SINGLE_LEVEL_RECURSION = TestCommon.FolderRoot + @"singleLevel.xsd";
		string A_IMP_B = TestCommon.FolderRoot + @"a_imp_b.xsd";*/
		string NO_ID = TestCommon.FolderRoot + @"no_id.xsd";

		 string PT_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-usfr-pt.xml";
		 string US_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-gaap-ci.xml";
		 string NODE_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-gaap-ci-nodes.xml";
		
		// Performance Testing Taxonomies;
		// IFRS:
		 string IFRS_FULL = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"IFRS" +System.IO.Path.DirectorySeparatorChar +"Full" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2005-01-15.xsd";
		 string IFRS_CALC = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"IFRS" +System.IO.Path.DirectorySeparatorChar +"Calc" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2005-01-15.xsd";
		 string IFRS_CORE = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"IFRS" +System.IO.Path.DirectorySeparatorChar +"Core" +System.IO.Path.DirectorySeparatorChar +"ifrs-gp-2005-01-15.xsd";

		// Canadian GAAP-CI:
		 string CAN_FULL = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"Canadian" +System.IO.Path.DirectorySeparatorChar +"Full" +System.IO.Path.DirectorySeparatorChar +"ca-gaap-pfs-2004-11-20.xsd";
		 string CAN_CALC = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"Canadian" +System.IO.Path.DirectorySeparatorChar +"Calc" +System.IO.Path.DirectorySeparatorChar +"ca-gaap-pfs-2004-11-20.xsd";
		 string CAN_CORE = TestCommon.FolderRoot + @"PerformanceTest" +System.IO.Path.DirectorySeparatorChar +"Canadian" +System.IO.Path.DirectorySeparatorChar +"Core" +System.IO.Path.DirectorySeparatorChar +"ca-gaap-pfs-2004-11-20.xsd";



		#region Common
		 /// <exclude/>
		 [Test, Ignore]
		public void Test_NoIdForElementValidation()
		{
			TestTaxonomy_Common s = new TestTaxonomy_Common();

			s.Load( NO_ID );
			Assert.AreEqual( Taxonomy.ValidationStatus.WARNING, s.Validate() );
		}

		/// <exclude/>
		[Test]
		public void Test_SingleCircularRef()
		{
			/*TestTaxonomy_Common s = new TestTaxonomy_Common();

			s.Load( SINGLE_LEVEL_RECURSION );

			ArrayList taxes = new ArrayList();

			try
			{
				s.GetDependantTaxonomies( taxes );
				Assert.Fail( "Exception not thrown" );
			}
			catch ( AucentException ae )
			{
				Console.WriteLine( ae.ResourceKey );
			}
			catch ( Exception ex )
			{
				Assert.Fail( ex.Message );
			}*/
		}

		/// <exclude/>
		[Test]
		public void Test_DeepCircularRef()
		{
			/*TestTaxonomy_Common s = new TestTaxonomy_Common();

			s.Load( A_IMP_B );

			ArrayList taxes = new ArrayList();

			try
			{
				s.GetDependantTaxonomies( taxes );
				Assert.Fail( "Exception not thrown" );
			}
			catch ( AucentException ae )
			{
				Console.WriteLine( ae.ResourceKey );
			}
			catch ( Exception ex )
			{
				Assert.Fail( ex.Message );
			}*/
		}

		/// <exclude/>
		[Test]
		public void Test_LanguageParse()
		{
			TestTaxonomy_Common s = new TestTaxonomy_Common();

			string uiLang = "en";

			s.CurrentLanguage = string.Empty;
			s.CurrentLanguage = uiLang;

			Assert.AreEqual( "en", s.CurrentLanguage, "wrong language info returned" );

		}

		/// <exclude/>
		[Test]
		public void Test_DupIdBug()
		{
			TestTaxonomy_Common s = new TestTaxonomy_Common();

			s.Load( DUP_ID );

			int errors = 0;
			s.Parse( out errors );

			Assert.AreEqual( 0, errors, "wrong number of parse errors returned" );

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Assert.AreEqual( 2, nodes.Count, "wrong number of top level nodes" );

			Node firstEntry = (Node)nodes[0];
			Assert.IsNotNull( firstEntry, "firstEntry is null" );

			Node secondEntry = (Node)nodes[1];
			Assert.IsNotNull( secondEntry, "secondEntry is null" );

			Assert.AreEqual( 1, firstEntry.Children.Count, "firstEntry has the wrong number of nodes" );
			Assert.AreEqual( 1, secondEntry.Children.Count, "secondEntry has the wrong number of nodes" );

			Node firstChild = (Node)firstEntry.Children[0];
			Assert.IsNotNull( firstChild, "firstChild is null" );

			Node secondChild = (Node)secondEntry.Children[0];
			Assert.IsNotNull( secondChild, "secondChild is null" );

			// ok, here we go
			Assert.AreEqual( firstChild.MyElement, secondChild.MyElement, "elements are not equal" );


		}

		/// <exclude/>
		[Test]
		public void TestGetNodeByPath()
		{
			int numErrors;
			Taxonomy tax = new Taxonomy();
			tax.Load(@"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd");
			tax.Parse(out numErrors);
			tax.currentLabelRole = "label";
			tax.currentLanguage = "en";

			ArrayList nodes = tax.GetNodesByPresentation(true);
			Node n = nodes[2] as Node;  //Income Statement root node
			n = n.children[0] as Node;  //Income & Expense abstarct node
			n = n.children[1] as Node;  //Cost of Goods and Services Sold
			n = n.children[1] as Node; // Cost of Services Sold
			n = n.children[5] as Node; // Cost of Services - Total

			string path = n.GetPresentationPath();
			Node testNode = tax.GetNodeFromPresentation(path, true);

			Assert.IsNotNull(testNode, "Failed to get node from presentation using path");
			Assert.AreEqual(n, testNode, "Incorrect node returned by GetNodeFromPresentation");
		}

		#endregion

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


		#region Taxonomy Performance Testing
		private void GetPerfCounters ()
		{
			PerformanceCounterCategory[] arrCategories = PerformanceCounterCategory.GetCategories();
			ArrayList arrItems = new ArrayList();
			PerformanceCounterCategory Category = null;
			for (int i=0; i < arrCategories.Length;i++)
			{
				if ( (!arrCategories[i].CategoryName.ToLower().StartsWith("memory")) &&
					(!arrCategories[i].CategoryName.ToLower().StartsWith(".net clr memory"))   )
					continue;

				arrItems.Add(arrCategories[i].CategoryName);
			}
			foreach (string item in arrItems)
			{
				Category = new PerformanceCounterCategory(item, Environment.MachineName);
				PerformanceCounterCategory pcInfo = new PerformanceCounterCategory(Category.CategoryName, Environment.MachineName);
				string [] Names = pcInfo.GetInstanceNames();
				PerformanceCounter[] Counters = null;
				try
				{
					foreach (string name in Names)
					{
						if ( (name == null) || name == string.Empty )
							continue;

						Counters = pcInfo.GetCounters(name);
						PerformanceCounter CounterInfo = null;
						foreach (System.Diagnostics.PerformanceCounter myCounter in Counters)
						{
							CounterInfo = new PerformanceCounter(Category.CategoryName, myCounter.CounterName);
							if (CounterInfo == null)
								continue;

							Console.WriteLine("-- " +Category.CategoryName + " - " + myCounter.CounterName + ": " + 
								myCounter.RawValue.ToString() );
						}
					}
				}
				catch (System.Exception ex )
				{
					Console.WriteLine("*** ERROR: "+ ex.Message);
				}
			}
		}

		/// <exclude/>
		[Test]
		public void PerformanceTest_Load_IFRS()
		{
			string[]  IFRSTaxonomies = new string[] {IFRS_CORE, IFRS_CALC, IFRS_FULL};
			this.RunPerformanceTest(IFRSTaxonomies);
		}

		/// <exclude/>
		[Test]
		public void PerformanceTest_Load_Canadian_GAAP()
		{
			string[]  CANTaxonomies = new string[] {CAN_CORE, CAN_CALC, CAN_FULL};
			this.RunPerformanceTest(CANTaxonomies);
		}

		/// <exclude/>
		public void RunPerformanceTest(string[] Taxonomies)
		{
			#region Comments - Please Review; 
			//
			//  The performance test was run in N-Unit testing facility and the PerfMon (windows performance monitor)
			//   was used to observe and note the memory usage for the nunit process.
			//   Each cycle of the loop within this method does sleep for 5 seconds (feel free to change) to allow for observation
			//   and notation of the memory size.  After each loop, the memory was cited and @ the end the bytes converted to 
			//   mega-byte amounts to get the results as noted below.
			//   
			//   Here is what each set of tested taxonomies included and what each part means:
			//		o Core : All linkbases except Calculation and Reference
			//		o Calc : Core plus Calculation linkbase
			//		o Full : Calc plus References linkbase
			// 
			// Here are the system specs for which the results were achieved on:
			// 
			// System Information:_____________________________________________________________________________________
			//     Operating System: Windows XP Professional (5.1, Build 2600) Service Pack 1 (2600.xpsp2.040919-1003)
			//             Language: English (Regional Setting: English)
			//  System Manufacturer: IBM
			//         System Model: 23739FU
			//                 BIOS: Phoenix FirstBIOS(tm) Notebook Pro Version 2.0 for IBM ThinkPad
			//            Processor: Intel(R) Pentium(R) M processor 1700MHz, ~1.7GHz
			//               Memory: 766MB RAM
			//            Page File: 357MB used, 1518MB available
			//          Windows Dir: C:\WINDOWS
			//     .Net CLR Version: 1.1.4322.2032
			// Number of Processors: 1
			//	
			//	Results:______________________________
			//	 Canadian GAAP-CI:____________________
			//					Core: 25 MB
			//					Calc: 49 MB (+24 MB)
			//					Full: 81 MB (+32 MB)
			//	 IFRS:________________________________
			//					Core: 205 MB
			//					Calc: 340 MB (+135 MB)
			//					Full: 384 MB (+44  MB)
			//	____________________________________________________________________
			//	Results date: 18 Feb. 2005 (Dragon Tag Version 1.1 - in Development)
			//
			#endregion

			Console.WriteLine("================ INITIALIZING =================");
			System.Threading.Thread.Sleep(5000);
			
			foreach (string taxonomy in Taxonomies)
			{
				Console.WriteLine(System.Environment.NewLine);
				Console.WriteLine("===============================================");

				// Start Timer;
				DateTime TestStart	= new DateTime();
				DateTime TestEnd 	= new DateTime();
				TestStart			= DateTime.Now;

				DateTime TaskStart	= new DateTime();
				DateTime TaskEnd 	= new DateTime();
				TaskStart			= DateTime.Now;

				Console.WriteLine("-- Taxonomy: " + taxonomy );
				Console.WriteLine("-- Taxonomy Test Start Time: " + TestStart.ToString());

				TestTaxonomy_Common tx = null;
				TaskStart = DateTime.Now;
				tx = new TestTaxonomy_Common();
				TaskEnd = DateTime.Now;
				Console.WriteLine("---- Taxonomy Instantiation took: " + (TaskEnd-TaskStart).ToString());
			
				TaskStart = DateTime.Now;
				tx.Load(taxonomy);
				TaskEnd = DateTime.Now;
				Console.WriteLine("---- Taxonomy Loading took: " + (TaskEnd-TaskStart).ToString());

				int errors = 0;
				TaskStart = DateTime.Now;
				tx.Parse( out errors );
				TaskEnd = DateTime.Now;
				
				Console.WriteLine("-- Taxonomy Parsing took: " + (TaskEnd-TaskStart).ToString());

				TestEnd = DateTime.Now;
				Console.WriteLine("-- Test End Time: " + TestEnd.ToString());
				Console.WriteLine("-- Test Total Time: " + (TestEnd-TestStart).ToString());

				Console.WriteLine("===============================================");
				
				System.Threading.Thread.Sleep(5000);
			}
			Console.WriteLine("================ COMPLETED =================");
		}

		#endregion

		#region negated label testing

		/// <exclude/>
		[Test]
		public void TestTryGetDefaultNegatedLabelRole()
		{
			Taxonomy tax = new Taxonomy();
			tax.labelRoles = new ArrayList();
			tax.labelRoles.Add( LABEL );
			tax.labelRoles.Add( LABEL_TOTAL );
			tax.labelRoles.Add( LABEL_PER_START );
			tax.labelRoles.Add( LABEL_PER_END );
			//tax.labelRoles.Add( Taxonomy.NEGATED );
			//tax.labelRoles.Add( Taxonomy.NEGATED_TOTAL );
			//tax.labelRoles.Add( Taxonomy.NEGATED_PER_START );
			//tax.labelRoles.Add( Taxonomy.NEGATED_PER_END );
			tax.labelRoles.Sort();

			string def;
			Assert.IsFalse( tax.TryGetDefaultNegatedLabelRole( out def ), "should not find a negated label role" );

			tax.labelRoles.Add( Taxonomy.NEGATED_PER_END );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.TryGetDefaultNegatedLabelRole( out def ), "did not find per end label" );
			Assert.AreEqual( Taxonomy.NEGATED_PER_END, def, "wrong per end label found" );

			tax.labelRoles.Add( Taxonomy.NEGATED_PER_START );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.TryGetDefaultNegatedLabelRole( out def ), "did not find per start label" );
			Assert.AreEqual( Taxonomy.NEGATED_PER_START, def, "wrong per start label found" );

			tax.labelRoles.Add( Taxonomy.NEGATED_TOTAL );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.TryGetDefaultNegatedLabelRole( out def ), "did not find total label" );
			Assert.AreEqual( Taxonomy.NEGATED_TOTAL, def, "wrong total label found" );

			tax.labelRoles.Add( Taxonomy.NEGATED );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.TryGetDefaultNegatedLabelRole( out def ), "did not find negated label" );
			Assert.AreEqual( Taxonomy.NEGATED, def, "wrong negated label found" );

		}

		/// <exclude/>
		[Test]
		public void TestConvertLabelRoleToNegatedLabelRole()
		{
			Taxonomy tax = new Taxonomy();
			tax.labelRoles = new ArrayList();
			tax.labelRoles.Add( LABEL );
			tax.labelRoles.Add( LABEL_TOTAL );
			tax.labelRoles.Add( LABEL_PER_START );
			tax.labelRoles.Add( LABEL_PER_END );
			tax.labelRoles.Sort();

			string role;
			Assert.IsFalse( tax.ConvertLabelRoleToNegatedLabelRole( LABEL, out role ), "should have failed to convert label with no negated labels defined." );
			

			tax.labelRoles.Add( Taxonomy.NEGATED );
			tax.labelRoles.Add( Taxonomy.NEGATED_TOTAL );
			tax.labelRoles.Add( Taxonomy.NEGATED_PER_START );
			tax.labelRoles.Add( Taxonomy.NEGATED_PER_END );
			tax.labelRoles.Sort();

			Assert.IsTrue( tax.ConvertLabelRoleToNegatedLabelRole( LABEL, out role ), "did not convert label" );
			Assert.AreEqual( Taxonomy.NEGATED, role, "did not find negated label" );

			Assert.IsTrue( tax.ConvertLabelRoleToNegatedLabelRole( LABEL_TOTAL, out role ), "did not convert totalLabel" );
			Assert.AreEqual( Taxonomy.NEGATED_TOTAL, role, "did not find negated label total" );

			Assert.IsTrue( tax.ConvertLabelRoleToNegatedLabelRole( LABEL_PER_START, out role ), "did not convert periodStartLabel" );
			Assert.AreEqual( Taxonomy.NEGATED_PER_START, role, "did not find negated label per start" );

			Assert.IsTrue( tax.ConvertLabelRoleToNegatedLabelRole( LABEL_PER_END, out role ), "did not convert periodEndLabel" );
			Assert.AreEqual( Taxonomy.NEGATED_PER_END, role, "did not find negated label per end" );

			Assert.IsTrue( tax.ConvertLabelRoleToNegatedLabelRole( null, out role ), "did not convert null label" );
			Assert.AreEqual( Taxonomy.NEGATED, role, "did not find negated label" );
		}

		/// <exclude/>
		[Test]
		public void TestConvertNegatedLabelRoleToNonNegatedRole()
		{
			Taxonomy tax = new Taxonomy();
			string role;
			Assert.IsTrue( tax.ConvertNegatedLabelRoleToNonNegatedRole( NEGATED, out role ), "did not convert label" );
			Assert.AreEqual( Taxonomy.LABEL, role, "did not find negated label" );

			Assert.IsTrue( tax.ConvertNegatedLabelRoleToNonNegatedRole( NEGATED_TOTAL, out role ), "did not convert totalLabel" );
			Assert.AreEqual( Taxonomy.LABEL_TOTAL, role, "did not find negated label total" );

			Assert.IsTrue( tax.ConvertNegatedLabelRoleToNonNegatedRole( NEGATED_PER_START, out role ), "did not convert periodStartLabel" );
			Assert.AreEqual( Taxonomy.LABEL_PER_START, role, "did not find negated label per start" );

			Assert.IsTrue( tax.ConvertNegatedLabelRoleToNonNegatedRole( NEGATED_PER_END, out role ), "did not convert periodEndLabel" );
			Assert.AreEqual( Taxonomy.LABEL_PER_END, role, "did not find negated label per end" );

			Assert.IsTrue( tax.ConvertNegatedLabelRoleToNonNegatedRole( null, out role ), "did not convert null label" );
			Assert.AreEqual( Taxonomy.LABEL, role, "did not find negated label" );

			Assert.IsFalse( tax.ConvertNegatedLabelRoleToNonNegatedRole( Taxonomy.LABEL, out role ), "should not find a conversion for label" );


		}

		/// <exclude/>
		[Test]
		public void TestHasNegatedLabelRoleDefined()
		{
			Taxonomy tax = new Taxonomy();
			tax.labelRoles = new ArrayList();
			tax.labelRoles.Add( LABEL );
			tax.labelRoles.Add( LABEL_TOTAL );
			tax.labelRoles.Add( LABEL_PER_START );
			tax.labelRoles.Add( LABEL_PER_END );
			tax.labelRoles.Sort();

			Assert.IsFalse( tax.HasNegatedLabelRoleDefined, "should have failed to find negated label" );


			tax.labelRoles.Add( Taxonomy.NEGATED_PER_END );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.HasNegatedLabelRoleDefined, "should have found a negated label (per end)" );
			tax.labelRoles.Add( Taxonomy.NEGATED_PER_START );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.HasNegatedLabelRoleDefined, "should have found a negated label (per start)" );
			tax.labelRoles.Add( Taxonomy.NEGATED_TOTAL );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.HasNegatedLabelRoleDefined, "should have found a negated label (total)" );
			tax.labelRoles.Add( Taxonomy.NEGATED );
			tax.labelRoles.Sort();
			Assert.IsTrue( tax.HasNegatedLabelRoleDefined, "should have found a negated label" );

		}

		#endregion

        #region Merge Tax Test
        /// <exclude/>
		[Test]
		public void TestLoadTaxonomyFromInstanceDocument()
		{
			string fileName = @"S:\TestSchemas\Instance With TaxonomyInfo.xml";
			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( fileName, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
			tax.LoadTaxonomyFromInstanceDocument( @"S:\TestSchemas", ins, false ),
			"Failed to load taxonomy" );

			ArrayList nodes = tax.GetNodesByPresentation();

			Assert.IsTrue( nodes.Count > 10, "should have presentation nodes" );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_1Tax()
		{
			string fileName = @"http://www.sec.gov/Archives/edgar/data/13610/000095012309006419/bne-20080630.xml";
			string baseHRef = @"http://www.sec.gov/Archives/edgar/data/13610/000095012309006419";

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( fileName, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
				tax.LoadTaxonomyFromInstanceDocument( Path.GetTempPath(), ins, false, baseHRef ),
				"Failed to load taxonomy" );

			ArrayList nodes = tax.GetNodesByPresentation();
			Assert.IsTrue( nodes.Count >= 23, "should have presentation nodes" );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_2Tax()
		{
			string fileName = @"http://www.sec.gov/Archives/edgar/data/89800/000095015208008578/shw-20080930.xml";
			string baseHRef = @"http://www.sec.gov/Archives/edgar/data/89800/000095015208008578";

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( fileName, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
				tax.LoadTaxonomyFromInstanceDocument( Path.GetTempPath(), ins, false, baseHRef ),
				"Failed to load taxonomy" );

			ArrayList nodes = tax.GetNodesByPresentation();
			Assert.IsTrue( nodes.Count >= 5, "should have presentation nodes" );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_4Tax()
		{
			string fileName = @"http://www.sec.gov/Archives/edgar/data/908823/000119312506095211/aabxx-20060228.xml";
			string baseHRef = @"http://www.sec.gov/Archives/edgar/data/908823/000119312506095211/";

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( fileName, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
				tax.LoadTaxonomyFromInstanceDocument( Path.GetTempPath(), ins, false, baseHRef ),
				"Failed to load taxonomy" );

			ArrayList nodes = tax.GetNodesByPresentation();
			Assert.IsTrue( nodes.Count >= 16, "should have presentation nodes" );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_4Tax_Local_HREF()
		{

			string fileName = @"S:\TestSchemas\BaseHRef\Instance\aabxx-20060228.xml";
			string baseHRef = @"S:\TestSchemas\BaseHRef\Instance\";

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( fileName, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
				tax.LoadTaxonomyFromInstanceDocument( Path.GetTempPath(), ins, false, baseHRef ),
				"Failed to load taxonomy" );

			ArrayList nodes = tax.GetNodesByPresentation();
			Assert.IsTrue( nodes.Count >= 16, "should have presentation nodes" );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_4Tax_Local_Partial()
		{
			string copyTo = Path.GetTempFileName();
			string curDir = Path.GetDirectoryName( copyTo );
			string fileName = @"http://www.sec.gov/Archives/edgar/data/908823/000119312506095211/aabxx-20060228.xml";

			WebClient cli = new WebClient();
			cli.DownloadFile( fileName, copyTo );

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( copyTo, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsFalse(
				tax.LoadTaxonomyFromInstanceDocument( curDir, ins, false ),
				"Taxonomy should not have loaded locally." );
		}

		[Test]
		public void TestLoadTaxonomyFromInstanceDocument_SEC_4Tax_Local_Partial2()
		{
			string copyTo = Path.GetTempFileName();
			string curDir = Path.GetDirectoryName( copyTo );
			string fileName = @"http://www.sec.gov/Archives/edgar/data/908823/000119312506095211/aabxx-20060228.xml";
			string baseHRef = @"http://www.sec.gov/Archives/edgar/data/908823/000119312506095211/";

			WebClient cli = new WebClient();
			cli.DownloadFile( fileName, copyTo );

			Instance ins = new Instance();
			ArrayList errors;
			ins.TryLoadInstanceDoc( copyTo, out errors );
			Taxonomy tax = new Taxonomy();

			Assert.IsTrue(
				tax.LoadTaxonomyFromInstanceDocument( curDir, ins, false, baseHRef ),
				"Failed to load taxonomy." );
		}

        #endregion

        [Test]
        public void TestPerformSECValidations1()
        {
            Taxonomy tax = new Taxonomy();

            tax.Load(@"S:\TESTSCHEMAS\FMC Taxonomy\fmc-20090930.xsd");
            int errors;
            Assert.IsTrue(tax.Parse(out errors), "Failed to parse taxonomy");
            Dictionary<string, bool> eleDt = new Dictionary<string,bool>();
            eleDt.Add( "us-gaap_Revenues", true );
            List<ValidationErrorInfo> outParam =  new List<ValidationErrorInfo>();
            tax.PerformSECValidations(null, eleDt, null, false, ref outParam);

        }

        [Test]
        public void TestReportName()
        {
            List<ValidationErrorInfo> errors = new List<ValidationErrorInfo>();
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101 - Sstatement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101- Statement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "Statement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101 - Statement -Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101 - Statement - ", ref errors), "should not be valid");

            Assert.IsFalse(Taxonomy.IsValidateReportName(true, "101 - Sstatement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(true, "101- Statement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(true, "Statement - Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(true, "101 - Statement -Hello Testing", ref errors), "should not be valid");
            Assert.IsFalse(Taxonomy.IsValidateReportName(true, "101 - Statement - ", ref errors), "should not be valid");



            Assert.IsTrue(Taxonomy.IsValidateReportName(false, "101 - Statement - Hello Testing", ref errors), "should be true");
            Assert.IsTrue(Taxonomy.IsValidateReportName(true, "101 - Statement - Hello Testing", ref errors), "should be true");

            Assert.IsTrue(Taxonomy.IsValidateReportName(true, "101 - Statement - Hello {Testing}", ref errors), "should be true");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101 - Statement - Hello {Testing}", ref errors), "curly brace not allowed for sec filings");

            Assert.IsTrue(Taxonomy.IsValidateReportName(true, "101 - Statement - Hello [Testing]", ref errors), "should be true");
            Assert.IsFalse(Taxonomy.IsValidateReportName(false, "101 - Statement - Hello [Testing]", ref errors), "curly brace not allowed for sec filings");

        }


        [Test]
        public void TestPerformSECValidations_TMP()
        {
            Taxonomy tax = new Taxonomy();

            tax.Load(@"C:\Crossfire Stuff\us-gaap-2011-01-31\us-gaap-2011-01-31\ind\ci\testing.xsd");
            int errors;
            Assert.IsTrue(tax.Parse(out errors), "Failed to parse taxonomy");
            Dictionary<string, bool> eleDt = new Dictionary<string, bool>();
            eleDt.Add("us-gaap_AccountsPayableAndAccruedLiabilities", true);
            eleDt.Add("us-gaap_AccountsPayable",true);
            List<ValidationErrorInfo> outParam = new List<ValidationErrorInfo>();
            tax.PerformSECValidations(null, eleDt, null, false, ref outParam);

        }



        [Test]
        public void TestPerformSECValidations2()
        {
            Taxonomy tax = new Taxonomy();

            tax.Load(@"S:\TESTSCHEMAS\BEN Taxonomy\ben-20091231.xsd");
            int errors;
            Assert.IsTrue(tax.Parse(out errors), "Failed to parse taxonomy");



            Instance ins = new Instance();
            ArrayList errorsstr;
            Assert.IsTrue(ins.TryLoadInstanceDoc(@"S:\TESTSCHEMAS\BEN Taxonomy\ben-20091231.xml", out errorsstr));



            Dictionary<string, bool> eleDt = new Dictionary<string,bool>();
            List<MarkupProperty> validMarkups = new List<MarkupProperty>();
            foreach (MarkupProperty mp in ins.markups)
            {
                mp.element = new Node( tax.allElements[mp.elementId] as Element);
               eleDt[mp.elementId] = true;

                validMarkups.Add(mp);
            }

            List<ValidationErrorInfo> outParam =  new List<ValidationErrorInfo>();
            tax.PerformSECValidations(validMarkups, eleDt, null, false, ref outParam);
            outParam.Sort();
            foreach( ValidationErrorInfo vei in outParam )
            {
                Console.WriteLine( vei.MyErrorString);
            }

        }


        [Test]
        public void TestPerformSECValidations3()
        {
            Taxonomy tax = new Taxonomy();

            tax.Load(@"S:\TESTSCHEMAS\EIX Taxonomy\eix.xsd");
            int errors;
            Assert.IsTrue(tax.Parse(out errors), "Failed to parse taxonomy");



           


            Dictionary<string, bool> eleDt = new Dictionary<string, bool>();
            List<MarkupProperty> validMarkups = new List<MarkupProperty>();
           
            List<ValidationErrorInfo> outParam = new List<ValidationErrorInfo>();
            tax.PerformSECValidations(validMarkups, eleDt, null, false, ref outParam);
            outParam.Sort();
            foreach (ValidationErrorInfo vei in outParam)
            {
                Console.WriteLine(vei.MyErrorString);
            }

        }

		 /// <exclude/>
		[Test, Ignore]
		public void TestKoreanTaxonomy()
		{
			Taxonomy tax = new Taxonomy();

			tax.Load(@"S:\TestSchemas\KoreanTest\xbrl03382-2008-04-23-ci.xsd");
			int errors;
			Assert.IsTrue(tax.Parse(out errors), "Failed to parse taxonomy");
		}



		[Test]
		public void TestTryGetDocumentationInformation1()
		{
			foreach (string file in Directory.GetFiles(@"R:\bin\Taxonomy\Documentation"))
			{
				if (file.EndsWith("vssver.scc")) continue;
				Console.WriteLine("Loading file " + file);

				TestTryGetDocumentationInformation(file, false);
			}
		
		}

		[Test]
		public void TestTryGetDocumentationInformation2()
		{
			foreach (string file in Directory.GetFiles(@"R:\bin\Taxonomy\Documentation"))
			{
				if (file.EndsWith("vssver.scc")) continue;
				Console.WriteLine("Loading file " + file);

				TestTryGetDocumentationInformation(file, true);
			}

		}

		[Test]
		public void TestTryGetReferenceInformation1()
		{
			foreach (string file in Directory.GetFiles(@"R:\bin\Taxonomy\Reference"))
			{
				if (file.EndsWith("vssver.scc")) continue;
				Console.WriteLine("Loading file " + file );
				TestTryGetReferenceInformation(file, false);
			}

		}

		[Test]
		public void TestTryGetReferenceInformation2()
		{
			foreach (string file in Directory.GetFiles(@"R:\bin\Taxonomy\Reference"))
			{
				if (file.EndsWith("vssver.scc")) continue;
				Console.WriteLine("Loading file " + file);
				TestTryGetReferenceInformation(file, true);
			}
		}

		[Test]
		public void TestTryGetReferenceInformation_RR2010()
		{
			string file = @"R:\bin\Taxonomy\Reference\rr-ref-2010-01-01.xml";
			Console.WriteLine("Loading file " + file);
			
			TestTryGetReferenceInformation(file, true);
		}

		private void TestTryGetDocumentationInformation(string fileName, bool showElem)
		{
			Dictionary<string, string> documentationByElementId = null;
			string err = null;
			string lang = "en-US";
			if (fileName.Contains("2005") && !fileName.Contains("2008"))
			{
				lang = "en";
			}
			DateTime start = DateTime.Now;
			Assert.IsTrue(
				Taxonomy.TryGetDocumentationInformation(lang, fileName, ref documentationByElementId, out err),
			"Failed to get documentation info " + err);
			DateTime end = DateTime.Now;

			Console.WriteLine("Time taken to load documentation info = {0}", end - start);
			if (showElem)
			{
				foreach (KeyValuePair<string, string> kvp in documentationByElementId)
				{
					Console.WriteLine(kvp.Key + "    " + kvp.Value);
				}

			}
			else
			{
				Console.WriteLine("Total count loaded = {0}", documentationByElementId.Count);
			}
		}

		private void TestTryGetReferenceInformation(string fileName, bool showElems)
		{
			Dictionary<string, string> refByElementId = null;
			string err = null;
			DateTime start = DateTime.Now;
			Assert.IsTrue(
			Taxonomy.TryGetReferenceInformation(fileName, ref refByElementId, out err),
			"Failed to get reference info " + err);
			DateTime end = DateTime.Now;

			Console.WriteLine("Time taken to load reference info = {0}", end - start);
			if (showElems)
			{
				foreach (KeyValuePair<string, string> kvp in refByElementId)
				{
					Console.WriteLine(kvp.Key + "    " + kvp.Value);
				}
			}
			else
			{
				Console.WriteLine("Total count loaded = {0}", refByElementId.Count);
			}
		}
        [Test]
        public void TestMutualFundTaxonomyLoad()
        {
            RemoteFiles.RemoteFileInformation.LoadRemoteFileInformation();

            LoadRRTaxonomy();

            RemoteFiles.RemoteFileInformation.SaveRemoteFileInformation();

            LoadRRTaxonomy();
        }

        [Test]
        public void TestRebuildRemoteFileInformation()
        {
            RemoteFiles.RemoteFileInformation.LoadRemoteFileInformation();
            RemoteFiles.RemoteFileInformation.ClearRemoteFileInformation();

            List<string> taxonomyListToLoad = new List<string>();


            taxonomyListToLoad.Add("http://xbrl.sec.gov/rr/2010/rr-entire-2010-02-28.xsd");
            //2009
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/basi/us-gaap-basi-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/bd/us-gaap-bd-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/ins/us-gaap-ins-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/re/us-gaap-re-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/non-gaap/dei-ent-all-2009-01-31.xsd");

            //2011
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/ci/us-gaap-ci-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/basi/us-gaap-basi-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/bd/us-gaap-bd-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/ins/us-gaap-ins-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/re/us-gaap-re-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/dei/2011/dei-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/invest/2011/invest-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/country/2011/country-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://www.xbrl.org/lrr/role/negated-2009-12-16.xsd");
            taxonomyListToLoad.Add("http://www.xbrl.org/lrr/role/net-2009-12-16.xsd");
           

 

            foreach (string str in taxonomyListToLoad)
            {

                Taxonomy tax = new Taxonomy();
                tax.Load(str);

                int error;
                Assert.IsTrue(tax.Parse(out error));

            }

            RemoteFiles.RemoteFileInformation.SaveRemoteFileInformation();
        }
        [Test]
        public void TestRemoteFileInformationCache()
        {
            RemoteFiles.RemoteFileInformation.LoadRemoteFileInformation();
          

            List<string> taxonomyListToLoad = new List<string>();


            taxonomyListToLoad.Add(@"S:\TESTSCHEMAS\TaxonomyConversionTest1\etp-taxonomy.xsd");
            taxonomyListToLoad.Add(@"S:\TESTSCHEMAS\TaxonomyConversionTest1\tmp\etp-taxonomy.xsd");


            taxonomyListToLoad.Add("http://xbrl.sec.gov/rr/2010/rr-entire-2010-02-28.xsd");
            //2009
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/basi/us-gaap-basi-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/bd/us-gaap-bd-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/ins/us-gaap-ins-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/ind/re/us-gaap-re-stm-dis-all-2009-01-31.xsd");
            taxonomyListToLoad.Add("http://taxonomies.xbrl.us/us-gaap/2009/non-gaap/dei-ent-all-2009-01-31.xsd");

            //2011
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/ci/us-gaap-ci-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/basi/us-gaap-basi-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/bd/us-gaap-bd-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/ins/us-gaap-ins-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.fasb.org/us-gaap/2011/ind/re/us-gaap-re-stm-dis-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/dei/2011/dei-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/invest/2011/invest-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://xbrl.sec.gov/country/2011/country-ent-all-2011-01-31.xsd");
            taxonomyListToLoad.Add("http://www.xbrl.org/lrr/role/negated-2009-12-16.xsd");
            taxonomyListToLoad.Add("http://www.xbrl.org/lrr/role/net-2009-12-16.xsd");




            foreach (string str in taxonomyListToLoad)
            {

                Taxonomy tax = new Taxonomy();
                tax.Load(str);

                int error;
                Assert.IsTrue(tax.Parse(out error));

            }

            
        }

        private void LoadRRTaxonomy()
        {
            string fileName = @"http://xbrl.sec.gov/rr/2010/rr-entire-2010-02-28.xsd";

            DateTime start = DateTime.Now;
            Taxonomy tax = new Taxonomy();
            tax.Load(fileName);

            int error;
            Assert.IsTrue(tax.Parse(out error));
        }
       
        
        private void MutualFundTaxonomyLoad()
        {
            string fileName = @"Q:\Taxonomies\Fidelity.xsd";

            DateTime start = DateTime.Now;
            Taxonomy tax = new Taxonomy();
            tax.Load(fileName);

            int error;
            Assert.IsTrue(tax.Parse(out error));


            tax.GetNodesByPresentation(true);
            List<DimensionNode> dNodes = new List<DimensionNode>();
            Assert.IsTrue(tax.TryGetAllDimensionNodesForDisplay(null, null, true, out dNodes));

            DateTime end = DateTime.Now;

            Console.WriteLine("Time taken = {0}", end - start);

            Element ele = tax.allElements["rr_MaximumAccountFeeOverAssets"] as Element;
            			object notUsed = null;
            string errorStr;

            Assert.IsTrue( ele.TryValidateElement("0.62%", ref notUsed , out errorStr));



        }

        [Test]
        public void Create2011URLMapping()
        {
            Taxonomy tax = new Taxonomy();
            int numErrors;
            string pathToUse = "http://xbrl.fasb.org/us-gaap/2011/entire/us-gaap-entryPoint-all-2011-01-31.xsd";

            if (tax.Load(pathToUse, out numErrors))
            {
                if (tax.Parse(out numErrors))
                {
                    //get all the file URL mappings...
                    Dictionary<string, string> fileUrlMap = tax.BuildTaxonomyUrlMappings();

                    RemoteFiles.UpdateTaxonomyURLMappingInfo(fileUrlMap);
                }
            }
        }


        [Test]
        public void TestCustomDataTypesLoading()
        {
            Taxonomy tax = new Taxonomy();
            int numErrors;
            string pathToUse = @"S:\TESTSCHEMAS\TestTypesIn2011\sdds.xsd";

            if (tax.Load(pathToUse, out numErrors))
            {
                if (tax.Parse(out numErrors))
                {

                    foreach (KeyValuePair<string, string> kvp in tax.GetSimpleCustomElementTypes())
                    {

                        Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }
}
#endif
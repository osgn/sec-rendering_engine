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

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_2005_02_28 : Taxonomy
	{
		/// <exclude/>
		protected Hashtable tupleElements = new Hashtable(1000);
		
		#region Overrides
		/// <exclude/>
		public bool TestParse(out int errors)
		{
			errors = 0;
			ParseInternal( out errors );
			return true;
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
			Console.WriteLine( "***Start TestTaxonomy_2005_02_28 Comments***" );
			
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
			Console.WriteLine( "***End TestTaxonomy_2005_02_28 Comments***" );
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

		string BASI_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"us-gaap-basi-2005-02-28.xsd";
		string INS_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ins-2005-02-28.xsd";
		string US_GAAP_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2005-02-28.xsd";
		string US_GAAP_WEB_FILE = @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd";
		string US_GAAP_PRESNTATION_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2005-02-28-presentation.xml";
		string US_PTR = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2005-02-28" +System.IO.Path.DirectorySeparatorChar +"usfr-ptr-2005-02-28.xsd";

		
		#region Common

		/// <exclude/>
		[Test]
		public void Test_LanguageParse()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			string uiLang = "en";

			s.CurrentLanguage = string.Empty;
			s.CurrentLanguage = uiLang;

			Assert.AreEqual( "en", s.CurrentLanguage, "wrong language info returned" );
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

		#region GAAP Schema

		/// <exclude/>
		[Test]
		public void TestTaxonomyCloneForMerging()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load(US_GAAP_FILE);

			int errors = 0;

			s.Parse(out errors);

			ArrayList nodeList = s.GetNodesByPresentation();

			foreach (Element ele in s.allElements.Values)
			{
				Element tmp = ele.Clone() as Element;
			}

			Taxonomy copy = s.CopyTaxonomyForMerging();
		}

		
		
		#region UI API's
		/// <exclude/>
		[Test]
		[Ignore("Just used to view nodes")]
		public void GAAP_Test_GetNodesByPresentation()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByPresentation();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 5, nodeList.Count );

			Console.WriteLine( "Nodes By Presentation: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		/// <exclude/>
		[Test]
		[Ignore("Just used to view nodes")]
		public void GAAP_Test_GetNodesByElement()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 1367, nodeList.Count );

			Console.WriteLine( "Nodes By Element: " );
			
			foreach (Node n in nodeList )
			{
				Console.WriteLine( TestNode.ToXml( 0, n ) );
			}
		}

		#endregion

		#region TestTuples

		/// <exclude/>
		[Test]
		[Ignore("display only")]
		public void GAAP_Test_VerifyTuples()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			ArrayList nodeList = s.GetNodesByElement();

			Assert.IsNotNull( nodeList );
			Assert.AreEqual( 1331, nodeList.Count );

			Console.WriteLine( "Found Tuple Nodes: " );
			
			foreach (Node n in nodeList )
			{
				RecurseVerifyTuples( n );
			}
		}

		/// <exclude/>
		[Test]
		public void GAAP_TestTuples()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( US_GAAP_FILE );

			s.Parse( out errors );
			s.currentLabelRole = PresentationLocator.preferredLabelRole;
			s.currentLanguage = s.SupportedLanguages[0] as String;

            ArrayList sortedElements = s.PreProcessGetNodesByElement();
            Console.WriteLine("Found Tuple Elements: ");

            foreach (Element ele in sortedElements)
            {
                RecurseElementsForTuples(ele);
            }


		}

		#endregion

		/// <exclude/>
		[Test]
		public void TestSaveToLocalApplicationData()
		{
			Taxonomy tx = new Taxonomy();
			int errors = 0;
			DateTime start = DateTime.Now;

			// we only save to local app data if it's a web file location...
			Assert.AreEqual( true, tx.Load( US_GAAP_WEB_FILE, out errors ), "Could not load US GAAP File" );
			Assert.AreEqual( 0, errors );
			tx.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );
			FileInfo fi = null;

			// Expecting 10 files to copy over...
			// 1. usfr-pte-2005-02-28.xsd
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-pte-2005-02-28.xsd" );
			Assert.IsTrue(fi.Exists, "File not found: " + fi.Name);

			// 2. usfr-pte-2005-02-28-label.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-pte-2005-02-28-label.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 3. usfr-pte-2005-02-28-presentation.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-pte-2005-02-28-presentation.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 4. usfr-pte-2005-02-28-reference.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-pte-2005-02-28-reference.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 5. usfr-ptr-2005-02-28.xsd
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-ptr-2005-02-28.xsd" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 6. usfr-ptr-2005-02-28-calculation.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-ptr-2005-02-28-calculation.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 7. usfr-ptr-2005-02-28-presentation.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-ptr-2005-02-28-presentation.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 8. us-gaap-ci-2005-02-28.xsd
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-2005-02-28.xsd" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 9. us-gaap-ci-2005-02-28-calculation.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-2005-02-28-calculation.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );

			// 10. us-gaap-ci-2005-02-28-presentation.xml
			fi = new FileInfo( AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-2005-02-28-presentation.xml" );
			Assert.IsTrue( fi.Exists, "File not found: " + fi.Name );
		}

		/// <exclude/>
		[Test]
		public void TestValidateTaxonomyRecursively()
		{
			Taxonomy tx = new Taxonomy();
			int errors = 0;
			DateTime start = DateTime.Now;
			Assert.AreEqual( true, tx.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			Assert.AreEqual( 0, errors );
			Console.WriteLine("==========================");
			ValidationStatus VS = tx.Validate();
			Console.WriteLine("Number of Errros:   " + tx.ValidationErrors.Count);
			Console.WriteLine("Number of Warnings: " + tx.ValidationWarnings.Count);
			Console.WriteLine("Validation Status:  " + VS.ToString());
			if (tx.ValidationWarnings.Count > 0)
			{
				System.Collections.IEnumerator vwarnings = tx.ValidationWarnings.GetEnumerator();
				while ( vwarnings.MoveNext() )
					Console.WriteLine("  Warning > " + vwarnings.Current);
			}

			if (tx.ValidationErrors.Count > 0)
			{
				System.Collections.IEnumerator verrors = tx.ValidationErrors.GetEnumerator();
				while ( verrors.MoveNext() )
					Console.WriteLine("  Error  > " + verrors.Current);
			}

			Console.WriteLine("==========================");
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetTargetPrefix()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.Load( US_GAAP_FILE );

			Assert.AreEqual( "us-gaap-ci", s.GetNSPrefix() );
		}

		
		#region Presentation
		/// <exclude/>
		[Test]
		public void GAAP_VerifyNodeOrder()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );

			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Node title = nodes[5] as Node;
			Assert.IsNotNull( title, "title is null" );
			Assert.AreEqual( "Statement of Financial Position", title.Label, "title label is wrong" );
			Assert.AreEqual( 0, title.Order, "title.Order is wrong" );

			Node statement = title.Children[0] as Node;
			Assert.IsNotNull( statement, "statement is null" );
			Assert.AreEqual( "Statement of Financial Position", statement.Label, "statement label is wrong" );
			Assert.AreEqual( 0, statement.Order, "statement.Order is wrong" );
			Assert.AreEqual( 2, statement.Children.Count, "statement.Children.Count is wrong" );

			Node assets = statement.Children[0] as Node;
			Assert.IsNotNull( assets, "statement is null" );
			Assert.AreEqual( "Assets", assets.Label, "assets label is wrong" );
			Assert.AreEqual( 1, assets.Order, "assets.Order is wrong" );
			Assert.AreEqual( 3, assets.Children.Count, "assets.Children.Count is wrong" );

			Node child1 = assets.Children[0] as Node;
			Assert.IsNotNull( child1, "child1 is null" );
			Assert.AreEqual( 1, child1.Order, "child1.Order is wrong" );

			Node child2 = assets.Children[1] as Node;
			Assert.IsNotNull( child2, "child2 is null" );
			Assert.AreEqual( 2, child2.Order, "child2.Order is wrong" );

			Node child3 = assets.Children[2] as Node;
			Assert.IsNotNull( child3, "child3 is null" );
			Assert.AreEqual( 3, child3.Order, "child3.Order is wrong" );

		}

		/// <exclude/>
		[Test]
		public void GAAP_VerifyElementTuplesHaveChildren()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByElement();

			Assert.IsNotNull( nodes, "No nodes returned" );

			foreach ( Node n in nodes )
			{
				if ( n.IsTuple )
				{
					Assert.IsNotNull( n.Children, "children arraylist is null" );
					Assert.IsTrue( n.Children.Count > 0, "Tuple doesn't have children" );
					Assert.AreEqual( 0.0, n.Order, "order not zero" );
				}
			}
		}

		/// <exclude/>
		[Test]
		[Ignore("need to resolve this - it looks like TuplesAbstract is no longer tied to a parent")] 
		public void GAAP_VerifyProhibitedLinks()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );

			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			// ok - here's the hierarchy
			// 0. Statement of Cash Flows - Indirect Method - CI
			// 1. Statement of Financial Position - CI
			//    0. Tuples Abstract
			//	     0. usfr-pt_CommonStock - prohibited
			//		 1. usfr-pt_ConvertiblePreferredStock - prohibited
			//		 2. usfr-pt_NonRedeemableConvertiblePreferredStock - prohibited
			//		 3. usfr-pt_NonRedeemablePreferredStock - prohibited
			//		 4. usfr-pt_PreferredStock - prohibited
			//		 5. usfr-pt_RedeemableConvertiblePreferredStock - prohibited
			//		 6. usfr-pt_RedeemablePreferredStock - prohibited
			//		 7. usfr-pt_TreasuryStock - prohibited

			Node finPos = nodes[1] as Node;
			Assert.IsNotNull( finPos, "Statement of Financial Position - CI not found" );

			Node tupleAbs = finPos.Children[0] as Node;
			Assert.IsNotNull( tupleAbs, "Tuples Abstract not found" );

			// and now the test
			Assert.AreEqual( 8, tupleAbs.Children.Count, "TupleAbstract has children" );

			// make sure are children are prohibited
			Assert.IsTrue( ((Node)tupleAbs.Children[0]).IsProhibited, "child 0 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[1]).IsProhibited, "child 1 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[2]).IsProhibited, "child 2 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[3]).IsProhibited, "child 3 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[4]).IsProhibited, "child 4 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[5]).IsProhibited, "child 5 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[6]).IsProhibited, "child 6 not prohibited" );
			Assert.IsTrue( ((Node)tupleAbs.Children[7]).IsProhibited, "child 7 not prohibited" );
		}

	

		/// <exclude/>
		[Test]
		public void GAAP_VerifyTupleChildMinMaxOccurs()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			if ( s.Parse(out errors ) != true )
			{
				Assert.Fail( "Parse failed." );
			}

			s.CurrentLabelRole = "label";
			s.CurrentLanguage = "en";
			ArrayList nodes = s.GetNodesByPresentation();

			// CI Presentation
			// 5 - Statement of Financial Position
			//     0 - Statement of Financial Position
			//         1 - Liabilities and Stockhoders' Equity
			//		       2 - Stockholder's Equity
			//				   1 - Preferred Stock Value
			//					   0 - Preferred Stock
			//					   3 - Redeemable Convertible Preferred Stock

			Node title = nodes[5] as Node;
			Assert.IsNotNull( title, "title is null" );
			Assert.IsNotNull( title.MyPresentationLink, "presentationLink is null" );

			Node statement = title.Children[0] as Node;
			Assert.IsNotNull( statement , "Statement of Financial Position is null" );

			Node liabSE = statement.Children[1] as Node;
			Assert.IsNotNull( liabSE, "Liabilities and Stockhoders' Equity is null" );

			Node se = liabSE.Children[2] as Node;
			Assert.IsNotNull( se, "Stockholder's Equity is null" );

			Node psv = se.Children[1] as Node;
			Assert.IsNotNull( psv, "Preferred Stock Value is null" );

			Node preferredStock = psv.Children[0] as Node;
			Assert.IsNotNull( preferredStock, "Preferred Stock is null" );

			Node rcPreferredStock = psv.Children[3] as Node;
			Assert.IsNotNull( rcPreferredStock, "RedeemableConvertiblePreferredStock is null" );

			//first check the Preferred Stock children
			//preferredStockDescription is min 1 max 1
			Node psd= preferredStock.Children[0] as Node;
			Assert.IsNotNull( psd, "PreferredStockDescription is null" );
			Assert.IsNotNull( psd.MyElement, "PreferredStockDescription MyElement is null" );
            Assert.AreEqual(1, psd.GetMinOccurance(), "PreferredStockDescription min value should be 1.");
            Assert.AreEqual(1, psd.GetMaxOccurance(), "PreferredStockDescription max value should be 1.");

			//preferredStockValue is min 0 max 1
			Node psvChild= preferredStock.Children[1] as Node;
			Assert.IsNotNull( psvChild, "PreferredStockValue is null" );
			Assert.IsNotNull( psvChild.MyElement, "PreferredStockValue MyElement is null" );
            Assert.AreEqual(0, psvChild.GetMinOccurance(), "PreferredStockValue min value should be 0.");
            Assert.AreEqual(1, psvChild.GetMaxOccurance(), "PreferredStockValue max value should be 1.");

			//now check the Redeemable Convertible Preferred Stock children
			//redeemableConvertiblePreferredStockDescription is min 1 max 1
			Node rcpsd= rcPreferredStock.Children[0] as Node;
			Assert.IsNotNull( rcpsd, "RedeemableConvertiblePreferredStockDescription is null" );
			Assert.IsNotNull( rcpsd.MyElement, "RedeemableConvertiblePreferredStockDescription MyElement is null" );
            Assert.AreEqual(1, rcpsd.GetMinOccurance(), "RedeemableConvertiblePreferredStockDescription min value should be 1.");
            Assert.AreEqual(1, rcpsd.GetMaxOccurance(), "RedeemableConvertiblePreferredStockDescription max value should be 1.");

			//redeemableConvertiblePreferredStockValue is min 0 max 1
			Node rcpsv= rcPreferredStock.Children[1] as Node;
			Assert.IsNotNull( rcpsv, "RedeemableConvertiblePreferredStockValue is null" );
			Assert.IsNotNull( rcpsv.MyElement, "RedeemableConvertiblePreferredStockValue MyElement is null" );
            Assert.AreEqual(0, rcpsv.GetMinOccurance(), "RedeemableConvertiblePreferredStockValue min value should be 0.");
            Assert.AreEqual(1, rcpsv.GetMaxOccurance(), "RedeemableConvertiblePreferredStockValue max value should be 1.");

			//redeemableConvertiblePreferredStockValueSharesSubscribedUnissued is min 0 with no max
			Node rcpsvssu = rcPreferredStock.Children[7] as Node;
			Assert.IsNotNull( rcpsvssu, "RedeemableConvertiblePreferredStockValueSharesSubscribedUnissued is null" );
			Assert.IsNotNull( rcpsvssu.MyElement, "RedeemableConvertiblePreferredStockValueSharesSubscribedUnissued MyElement is null" );
            Assert.AreEqual(0, rcpsvssu.GetMinOccurance(), "RedeemableConvertiblePreferredStockValueSharesSubscribedUnissued min value should be 0.");
            Assert.AreEqual(int.MaxValue, rcpsvssu.GetMaxOccurance(), "RedeemableConvertiblePreferredStockValueSharesSubscribedUnissued max value should be " + int.MaxValue);
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			
			string[] presRef = s.GetLinkbaseReference( TestTaxonomy_2005_02_28.TARGET_LINKBASE_URI + TestTaxonomy_2005_02_28.PRESENTATION_ROLE );

			Assert.AreEqual( US_GAAP_PRESNTATION_FILE, presRef[0] );
		}


		/// <exclude/>
		[Test]
		public void GAAP_VerifyPreferredLabel2()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				//SendWarningsToConsole( s.errorList );
			}

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			errors = 0;
			ArrayList nodes = s.GetNodes(PresentationStyle.Presentation, false, out errors );

			Assert.AreEqual( 0, errors, "errors returned from set nodes" );

			// 5 - Statement of Financial Position
			//     0 - Statement of Financial Position
			//         0 - Assets
			//		       0 - Assets - Current
			//					0 - Cash, Cash Equivalents and Short Term Investments
			//						0 - Cash and Cash Equivalents

			Node stateFinPosTitle = nodes[5] as Node;
			Assert.IsNotNull( stateFinPosTitle, "stateFinPosTitle is null" );
			Assert.IsNotNull( stateFinPosTitle.MyPresentationLink, "presentationLink is null" );

			Node statement = stateFinPosTitle.Children[0] as Node;
			Assert.IsNotNull( statement , "statement is null" );

			Node assets = statement.Children[0] as Node;
			Assert.IsNotNull( assets, "assets is null" );

			Node assetsCur = assets.Children[0] as Node;
			Assert.IsNotNull( assetsCur, "assetsCur is null" );

			Node cashCashEquivParent = assetsCur.Children[0] as Node;
			Assert.IsNotNull( cashCashEquivParent, "cashCashEquivParent is null" );

			Node cashCashEquiv = cashCashEquivParent.Children[0] as Node;
			Assert.IsNotNull( cashCashEquiv, "cashCashEquiv is null" );

			// cash, cash equiv has 3 children
			Assert.IsNotNull( cashCashEquiv.Children, "cashCashEquiv.Children is null" );
			Assert.AreEqual( 3, cashCashEquiv.Children.Count, "wrong number of children for cashCashEquiv" );

			Node child3 = cashCashEquiv.Children[2] as Node;

			Assert.IsNotNull( child3, "child3 is null" );

			//child3.PreferredLabel = null;
			child3.SetLabel( "en", "preferredLabel" );

			string text = null;
			child3.TryGetLabel( "en", "preferredLabel", out text );

			Assert.AreEqual( "totalLabel", child3.PreferredLabel, "child3.PreferredLabel is wrong" );
		}

		#endregion

		#region Calculation
	

		/// <exclude/>
		[Test]
		public void GAAP_BindCalculation()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.Parse( out errors );

			s.numWarnings = errors = 0;
			s.BindPresentationCalculationElements( false, out errors );

			if ( errors != 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "Bind calc failed" );
			Assert.AreEqual( 0, s.numWarnings, "num warnings wrong" );

			// 500 from pt
		}

		/// <exclude/>
		[Test]
		[Ignore("usfr-pt_CashCashEquivalents has moved")]
		public void GAAP_VerifyPreferredLabel()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );

			s.currentLabelRole = "preferredLabel";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			//0. "Statement of Cash Flows - Indirect Method - CI"
			//   0. "usfr-pt_StatementCashFlowsIndirectAbstract"
			//      ?. "usfr-pt_CashCashEquivalents"
	
			Node scf = nodes[0] as Node;
			Assert.IsNotNull( scf, "Statement of Cash Flows - Indirect Method - CI" );

			Node scfiAbs = scf.Children[0] as Node;
			Assert.IsNotNull( scfiAbs, "Failed to find usfr-pt_StatementCashFlowsIndirectAbstract node" );

			Node cce = null;
			foreach(Node node in scfiAbs.Children)
				if(node.Id == "usfr-pt_CashCashEquivalents")
					cce = node;
	
			Assert.IsNotNull( cce, "Failed to find usfr-pt_CashCashEquivalents node" );

			Assert.AreEqual("periodEndLabel", cce.PreferredLabel, "Invalid preferred element label");

			string label = null;
			Assert.AreEqual(true, cce.TryGetLabel(s.CurrentLanguage, s.CurrentLabelRole, out label), "Invalid return value from Node TryGetLabel method");
			Assert.AreEqual("Cash and Cash Equivalents - Ending Balance", label, "Invalid label value returned from Node TryGetLabel method");
		}

		#endregion

		#region imports
		/// <exclude/>
		[Test]
		public void GAAP_LoadImports()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadImports( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of load import errors" );

			errors = 0;
			s.LoadPresentation( out errors );
			Assert.AreEqual( 0, errors, "wrong number of load presentation errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of presentation warnings" );

			errors = 0;
			s.LoadElements( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//Assert.AreEqual( 5, errors, "wrong number of load element errors" ); //no duplicate elements in the latest taxonomy
			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of element warnings" );
			
			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "usfr-pte_AccountingPolicies" ), "usfr-pte_AccountingPolicies not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_LoadImportsAndBind()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) == false )
			{
				Assert.Fail( (string)s.ErrorList[0] );
			}

			errors = 0;
			s.LoadImports( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}
			
			Assert.AreEqual( 0, errors, "wrong number of load import errors" );	
			
			errors = 0;
			s.LoadPresentation( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of load presentation errors" );

			errors = 0;
			s.LoadElements( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//Assert.AreEqual( 5, errors, "wrong number of load element errors" ); //no duplicate elements defined in the latest taxonomy
			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}			

			errors = 0;
			s.BindPresentationCalculationElements( true, out errors );

			Assert.AreEqual( 0, errors, "wrong number of bind errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "usfr-pte_AccountingPolicies" ), "usfr-pte_AccountingPolicies not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_TestDependantPresentations()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( !s.Load( US_GAAP_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			s.LoadImports( out errors );
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			s.LoadPresentation( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			if ( s.numWarnings > 0 )
			{
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.errorList );
			}

			errorList.Clear();
			errors = s.numWarnings = 0;
			s.LoadElements( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			if ( s.numWarnings > 0 )
			{
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.errorList );
			}

			// and now bind elements to presentation
			errorList.Clear();
			errors = s.numWarnings = 0;
			s.BindPresentationCalculationElements( true, out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			if ( s.numWarnings > 0 )
			{
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.errorList );
			}

			// ok, now do some testing
			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/us/fr/lr/role/StatementFinancialPosition"] as PresentationLink;

			Assert.IsNotNull( pl, "Presentation link is null" );

			PresentationLocator presLocator = null;
			pl.TryGetLocator( "usfr-pte_CommonStockShareSubscriptions", out presLocator );

			Assert.IsNotNull( presLocator, "presentation Locator not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetDependantTaxonomies()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( !s.Load( US_GAAP_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			ArrayList imps = s.GetDependantTaxonomies( false, out errors );
			Assert.AreEqual( 0, errors, "error list not 0" );

			Assert.AreEqual( 2, imps.Count, "wrong number of imports returned" );

			Assert.AreEqual(@"S:\TestSchemas\XBRL 2.1 Updated\2005-02-28\usfr-ptr-2005-02-28.xsd", imps[0]);

		}
		#endregion

		/// <exclude/>
		[Test]
		public void GAAP_Parse_Label()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );
			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			Element el = s.allElements["usfr-pte_RoyaltyExpense"] as Element;
			string labelString = string.Empty;
			el.TryGetLabel("en", "label", out labelString);
			Assert.IsTrue(labelString != "",  "label info is not populated");
		}

		/// <exclude/>
		[Test]
		public void GAAP_Parse()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/us/fr/lr/role/StatementCashFlows"] as PresentationLink;
			Assert.IsNotNull( pl, "presentation link not found" );

			Assert.IsNotNull( pl.BaseSchema, "PresentationLink BaseSchema is null" );
			Assert.AreEqual( "http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd", pl.BaseSchema, "BaseSchema wrong" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pte_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			Assert.AreEqual( 0, errors, "parse failure" );	

			if ( s.numWarnings > 0 )
			{
				Assert.AreEqual( s.numWarnings, SendWarningsToConsole( s.errorList ), "numWarnings doesn't match warnings reported in the errorList" );
			}

			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			// 57 = bind failures - elements w/o presentation

			//Trace.Listeners.Clear();
			TimeSpan level = new TimeSpan( 0, 0, 0, 5, 0 );	// 5 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end - start) + " seconds" );
		}

		
		#region Elements
		

		/// <exclude/>
		[Test]
		public void GAAP_ReadElements()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( US_GAAP_FILE );

			int numElements = s.LoadElements( out errors );
			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 0, s.allElements.Count );	
		}

		#endregion

		#region Gets
		/// <exclude/>
		[Test]
		public void GAAP_GetLabelRoles()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.Load( US_GAAP_FILE );
			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList roles = s.GetLabelRoles(false, out errors);

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 8, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[3] );
			Assert.AreEqual( "periodStartLabel", roles[4] );
			Assert.AreEqual( "terseLabel", roles[6] );
			Assert.AreEqual( "totalLabel", roles[7] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetSupportedLanguages()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.Load( US_GAAP_FILE);
			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList langs = s.GetSupportedLanguages(false, out errors);

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_TargetNamespace()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			s.Load( US_GAAP_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/us/fr/gaap/ci/2005-02-28", s.targetNamespace );
		}

		/// <exclude/>
		[Test]
		public void GAAP_LoadSchema()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
		}
		#endregion

		#endregion

		#region BASI Schema
		/// <exclude/>
		[Test]
		public void BASI_Parse()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( BASI_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/us/fr/lr/role/StatementCashFlows"] as PresentationLink;
			Assert.IsNotNull( pl, "presentation link not found" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pte_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			Assert.AreEqual( 0, errors, "parse failure" );	

			// 1 - duplicate element 
			if ( s.numWarnings > 0 )
			{
				Assert.AreEqual( s.numWarnings, SendWarningsToConsole( s.errorList ), "numWarnings doesn't match warnings reported in the errorList" );
			}

			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			TimeSpan level = new TimeSpan( 0, 0, 0, 9, 0 );	// 9 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long - " + (end-start) + " seconds"  );
		}

		/// <exclude/>
		[Test]
		public void BASI_BlankElement()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			if ( s.Load( BASI_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Node incStmtBasi = nodes[4] as Node;
			Assert.IsNotNull( incStmtBasi, "incStmtBasi is null" );
	
			Node incStmt = incStmtBasi.Children[0] as Node;
			Assert.IsNotNull( incStmt, "incStmt is null" );
			
			Node interestExpense = incStmt.Children[1] as Node;
			Assert.IsNotNull( interestExpense, "interestExpense is null" );

			Node emptyElement = interestExpense.Children[5] as Node;
			Assert.IsNotNull( emptyElement, "emptyElement is null" );
		}

		/// <exclude/>
		[Test]
		public void BASI_LabelTest()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( BASI_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			s.BindPresentationCalculationElements( true, out errors );
			s.BindElements( new BindElementDelegate( s.BindLabelToElement ), out errors );

			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/us/fr/lr/role/TupleContentModelsCommonTerms" ] as PresentationLink;

			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/us/fr/lr/role/TupleContentModelsCommonTerms" );

			Node parentNode = pl.CreateNode( "en", "label" );
			System.Text.StringBuilder sb = TestTaxonomy_EDGAROnline.DisplayNode(parentNode, 0);
			Console.WriteLine(sb.ToString());

			// test the fourth child
			Node n = (Node)parentNode.Children[13];

			Assert.AreEqual( n.Label, "Contingencies - Possible Loss", "label wrong" );
		}
		#endregion
	
		#region INS Schema

		/// <exclude/>
		[Test]
		public void INS_VerifyNodes()
		{
			TestTaxonomy_2005_02_28 s = new TestTaxonomy_2005_02_28();
			int errors = 0;

			s.Load( INS_FILE);
			
			s.Parse( out errors );

			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Node title = nodes[3] as Node;
			Assert.IsNotNull( title, "title is null" );
			Assert.AreEqual( "Notes to the Financial Statements", title.Label, "title label is wrong" );
			Assert.AreEqual( 0, title.Order, "title.Order is wrong" );
			Assert.AreEqual( 3, title.Children.Count, "title.Children.Count is wrong" );

			Node notes = title.Children[2] as Node;
			Assert.IsNotNull( notes, "notes is null" );
			Assert.AreEqual( "Notes to the Financial Statements", notes.Label, "notes label is wrong" );
			Assert.AreEqual( 0, notes.Order, "notes.Order is wrong" );
			Assert.AreEqual( 6, notes.Children.Count, "notes.Children.Count is wrong" );

			Node assets = notes.Children[1] as Node;
			Assert.IsNotNull( assets, "statement is null" );
			Assert.AreEqual( "Asset Related Notes", assets.Label, "assets label is wrong" );
			Assert.AreEqual( 2, assets.Order, "assets.Order is wrong" );
			Assert.AreEqual( 19, assets.Children.Count, "assets.Children.Count is wrong" );

			Node investments = assets.Children[5] as Node;
			Assert.IsNotNull( investments, "investments is null" );
			Assert.AreEqual( "Investments Note", investments.Label, "investments label is wrong" );
			Assert.AreEqual( 5, investments.Order, "investments.Order is wrong" );
			Assert.AreEqual( 10, investments.Children.Count, "investments.Children.Count is wrong" );
		}
		#endregion
	}
}
#endif
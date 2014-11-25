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
	public class TestTaxonomy_2004_08_15 : Taxonomy
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
			Console.WriteLine( "***Start TestTaxonomy_2004_08_15 Comments***" );
			
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
			Console.WriteLine( "***End TestTaxonomy_2004_08_15 Comments***" );
		}

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string BASI_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-basi-2004-08-15.xsd";
		string PT_GAAP_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-08-15.xsd";
		string US_GAAP_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-08-15.xsd";
		string US_GAAP_FILE_2_0 = TestCommon.FolderRoot + @"US GAAP CI 2003-07-07" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2003-07-07.xsd";

		string NO_ID = TestCommon.FolderRoot + @"no_id.xsd";

		 string PT_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-usfr-pt.xml";
		 string US_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-gaap-ci.xml";
		 string NODE_OUT_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"aucent-gaap-ci-nodes.xml";

		#region Common
		 /// <exclude/>
		 [Test, Ignore]
		public void Test_NoIdForElementValidation()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( NO_ID );
			Assert.AreEqual( Taxonomy.ValidationStatus.WARNING, s.Validate() );
		}

		/// <exclude/>
		[Test]
		public void Test_LanguageParse()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

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

		#region PT Schema

		/// <exclude/>
		[Test]
		public void PT_GetTargetPrefix()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE );

			Assert.AreEqual( "usfr-pt", s.GetNSPrefix() );
		}

		/// <exclude/>
		[Test]
		public void PT_Parse()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE );

			int errors = 0;
			s.Parse( out errors );

			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			Assert.AreEqual( 0, errors, "errors returned" );	// presentation roleref bug in 8-15 taxonomy
			Assert.AreEqual( 0, s.numWarnings, "number of warnings wrong" );
		}

		/// <exclude/>
		[Test]
		public void PT_LoadPresentationSchema()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);

			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.IsNotNull( p );
			Assert.AreEqual( 0, errors );
		}

		/// <exclude/>
		[Test]
		public void PT_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
			
			string[] presRef = s.GetLinkbaseReference( TestTaxonomy_2004_08_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_08_15.PRESENTATION_ROLE );

			Assert.AreEqual( TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"usfr-pt-2004-08-15-presentation.xml", presRef[0] );
		}
				
		
		#region Elements
		/// <exclude/>
		[Test]
		public void PT_ReadElements()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;
			Presentation p = s.LoadPresentationSchema( out errors );
			Assert.AreEqual( 0, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			s.LoadElements( out errors );
			Assert.AreEqual( 0, errors, "load elements failed" );
		}

		/// <exclude/>
		[Test]
		public void PT_BindElements()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			DateTime start = DateTime.Now;
			s.Load( PT_GAAP_FILE);
			
			int errors = 0;

			DateTime startPres = DateTime.Now;
			Presentation p = s.LoadPresentationSchema( out errors );
			DateTime endPres = DateTime.Now;
			Assert.AreEqual( 0, errors, "load presentation failed" );

			s.presentationInfo = p.PresentationLinks;

			DateTime startElems = DateTime.Now;
			s.LoadElements( out errors );
			DateTime endElems = DateTime.Now;

			Assert.AreEqual( 0, errors, "load elements failed" );

			DateTime startBind = DateTime.Now;
			s.BindPresentationCalculationElements( true, out errors );
			DateTime endBind = DateTime.Now;

			Console.WriteLine( "Total Time:			{0}", endBind-start );
			Console.WriteLine( "Read Presentation	{0}", endPres-startPres );
			Console.WriteLine( "Read Elements		{0}", endElems-startElems );
			Console.WriteLine( "Bind Elements		{0}", endBind-startBind );

			Assert.AreEqual( 0, errors, "bind elements failed" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings" );

		}

		/// <exclude/>
		[Test]
		[Ignore("Superceded by GAAP_OutputTaxonomy")]
		public void PT_OutputTaxonomy()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;

			s.Parse( out errors );

#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( PT_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString() );
			}
#endif
		}

		#endregion

		#region Labels

		/// <exclude/>
		[Test]
		public void PT_GetSupportedLanguages()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList langs = s.GetSupportedLanguages(false, out errors);

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 1, langs.Count );
			Assert.AreEqual( "en", langs[0] );
		}

		#endregion

		/// <exclude/>
		[Test]
		public void PT_TargetNamespace()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
			
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/us/fr/common/pt/2004-08-15", s.targetNamespace );
		}

		/// <exclude/>
		[Test]
		public void PT_LoadSchema()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( PT_GAAP_FILE);
		}
		#endregion

		#region GAAP Schema
		
		#region UI API's
		/// <exclude/>
		[Test]
		[Ignore("Just used to view nodes")]
		public void GAAP_Test_GetNodesByPresentation()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			int errors = 0;

			s.Load( US_GAAP_FILE );

			s.Parse( out errors );
			s.currentLabelRole = PresentationLocator.preferredLabelRole;
			s.currentLanguage = s.SupportedLanguages[0] as String;

            ArrayList sortedElements = s.PreProcessGetNodesByElement();
			Console.WriteLine( "Found Tuple Elements: " );

			foreach( Element ele in sortedElements )
			{
                RecurseElementsForTuples(ele);
			}
		}

		#endregion


		/// <exclude/>
		[Test]
		public void TestSaveToLocalApplicationData()
		{
			string fileName = AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-2005-02-28.xsd";
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			fileName = AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-ptr-2005-02-28.xsd";
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			Taxonomy tx = new Taxonomy();
			int errors = 0;
			DateTime start = DateTime.Now;
			Assert.AreEqual( true, tx.Load( "http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd", out errors ), "Could not load US GAAP File" );
			Assert.AreEqual( 0, errors );
			tx.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine( "Parse Time: {0}", end-start );

			fileName = AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-2005-02-28.xsd";
			Assert.IsTrue(File.Exists(fileName));

			fileName = AucentGeneral.RivetApplicationDataDragonTagPath + System.IO.Path.DirectorySeparatorChar + "usfr-ptr-2005-02-28.xsd";
			Assert.IsTrue(File.Exists(fileName));


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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( US_GAAP_FILE );

			Assert.AreEqual( "us-gaap-ci", s.GetNSPrefix() );
		}

		#region output only

		/// <exclude/>
		[Test]
		[Ignore("output only")]
		public void GAAP_OutputTaxonomy()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( US_OUT_FILE ) )
			{
				sw.Write( s.ToXmlString( false ) );
			}
#endif
		}

		/// <exclude/>
		[Test]
		[Ignore("output only")]
		public void GAAP_OutputTaxonomyByNodes()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );
			
#if !AUTOMATED
			using ( StreamWriter sw = new StreamWriter( NODE_OUT_FILE ) )
			{
				s.currentLanguage = "en";
				s.currentLabelRole = "terseLabel";

				sw.Write( s.ToXmlString( s.GetNodesByPresentation() ) );
			}
#endif
		}

		/// <exclude/>
		[Test]
		[Ignore("output only")]
		public void GAAP_VerifyPresentationCorrect()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			s.Load( US_GAAP_FILE);

			s.Parse( out errors );

			string rawXml = s.ToXmlString();

			s.currentLabelRole = "terseLabel";
			s.CurrentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();
		}
		#endregion

		#region Presentation
		/// <exclude/>
		[Test]
		public void GAAP_VerifyNodeOrder()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			s.Load( US_GAAP_FILE);
			
			s.Parse( out errors );

			s.currentLabelRole = "label";
			s.currentLanguage = "en";

			ArrayList nodes = s.GetNodesByPresentation();

			Node title = nodes[4] as Node;
			Assert.IsNotNull( title, "title is null" );
			Assert.AreEqual( "Statement of Financial Position - CI", title.Label, "title label is wrong" );
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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			s.Load( US_GAAP_FILE);

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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
		public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			
			string[] presRef = s.GetLinkbaseReference( TestTaxonomy_2004_08_15.TARGET_LINKBASE_URI + TestTaxonomy_2004_08_15.PRESENTATION_ROLE );

			Assert.AreEqual( TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-08-15" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-08-15-presentation.xml", presRef[0] );
		}


		/// <exclude/>
		[Test]
		public void GAAP_VerifyPreferredLabel2()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			}

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			errors = 0;
			ArrayList nodes = s.GetNodes(PresentationStyle.Presentation, false, out errors);

			Assert.AreEqual( 0, errors, "errors returned from set nodes" );

			// 4 - Statement of Financial Position - CI
			//     0 - Statement of Financial Position
			//         0 - Assets
			//		       0 - Assets - Current
			//					0 - Cash, Cash Equivalents and Short Term Investments
			//						0 - Cash and Cash Equivalents

			Node stateFinPosTitle = nodes[4] as Node;
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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			Assert.AreEqual( 0, errors, "wrong number of load element errors" );
			Assert.AreEqual( 0, s.numWarnings, "wrong number of element warnings" );
			
			// accounting policies is defined in the usfr-pt schema
			Assert.IsTrue( s.allElements.ContainsKey( "usfr-pt_AccountingPolicies" ), "usfr-pt_AccountingPolicies not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_LoadImportsAndBind()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			Assert.IsTrue( s.allElements.ContainsKey( "usfr-pt_AccountingPolicies" ), "usfr-pt_AccountingPolicies not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_TestDependantPresentations()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			pl.TryGetLocator( "usfr-pt_CommonStockShareSubscriptions", out presLocator );

			Assert.IsNotNull( presLocator, "presentation Locator not found" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetDependantTaxonomies()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			if ( !s.Load( US_GAAP_FILE, out errors ) )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			ArrayList imps = s.GetDependantTaxonomies( false, out errors );
			Assert.AreEqual( 0, errors, "error list not 0" );

			Assert.AreEqual( 2, imps.Count, "wrong number of imports returned" );

			Assert.AreEqual(@"S:\TestSchemas\XBRL 2.1 Updated\2004-08-15\usfr-pt-2004-08-15.xsd", imps[0]);
		}
		#endregion

		/// <exclude/>
		[Test]
		public void GAAP_Parse_Label()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			Element el = s.allElements["usfr-pt_RoyaltyExpense"] as Element;
			string labelString = string.Empty;
			el.TryGetLabel("en", "label", out labelString);
			Assert.IsTrue(labelString != "",  "label info is not populated");
		}

		/// <exclude/>
		[Test]
		public void GAAP_Parse()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/us/fr/lr/role/StatementCashFlowsDirect"] as PresentationLink;
			Assert.IsNotNull( pl, "presentation link not found" );

			Assert.IsNotNull( pl.BaseSchema, "PresentationLink BaseSchema is null" );
			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-08-15/us-gaap-ci-2004-08-15.xsd", pl.BaseSchema, "BaseSchema wrong" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pt_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			// problem with presentation linkbase - don't know the solution yet
			Assert.AreEqual( 0, errors, "parse failure" );	

			if ( s.numWarnings > 0 )
			{
				Assert.AreEqual( s.numWarnings, SendWarningsToConsole( s.errorList ), "numWarnings doesn't match warnings reported in the errorList" );
			}

			Assert.AreEqual( 0, s.numWarnings, "wrong number of warnings " );

			// 57 = bind failures - elements w/o presentation

			TimeSpan level = new TimeSpan( 0, 0, 0, 4, 0 );	// 4 seconds to parse
			Assert.IsTrue( level > (end-start), "Parse takes too long" );
		}

		/// <exclude/>
		[Test]
		public void GAAP_ElementTaxonomyLinks()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			if ( s.Load( US_GAAP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			s.currentLanguage = "en";
			s.currentLabelRole = "terseLabel";

			ArrayList nodes = s.GetNodesByElement();

			Assert.AreEqual( 1, ((Node)nodes[0]).TaxonomyInfoId, "Taxonomy id not correct" );

			TaxonomyItem ti = s.GetTaxonomyInfo( (Node)nodes[0] );

			Assert.AreEqual( "http://www.xbrl.org/us/fr/common/pt/2004-08-15", ti.WebLocation, "target namespace wrong" );

			Assert.AreEqual(@"S:\TestSchemas\XBRL 2.1 Updated\2004-08-15\usfr-pt-2004-08-15.xsd", ti.Location, "targetLocation wrong");
		}


		#region Elements
		
		/// <exclude/>
		[Test]
		public void GAAP_ReadElements()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( US_GAAP_FILE );

			
			int errors = 0;
			s.Parse(out errors);
			Assert.AreEqual(0, errors);
			ArrayList roles = s.GetLabelRoles(false, out errors);

			Assert.AreEqual( 6, roles.Count );
			Assert.AreEqual( "documentation", roles[0] );
			Assert.AreEqual( "label", roles[1] );
			Assert.AreEqual( "periodEndLabel", roles[2] );
			Assert.AreEqual( "periodStartLabel", roles[3] );
			Assert.AreEqual( "terseLabel", roles[4] );
			Assert.AreEqual( "totalLabel", roles[5] );
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetSupportedLanguages()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

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
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			s.Load( US_GAAP_FILE);
	
			s.GetTargetNamespace();

			Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-08-15", s.targetNamespace );
		}

		/// <exclude/>
		[Test]
		public void GAAP_LoadSchema()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();

			int errors = 0;

			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
		}
		#endregion

		/// <exclude/>
		[Test]
		public void TestFailToLoad_2_0_VersionSchema()
		{
			int errors = 0;
			bool ok = this.Load(US_GAAP_FILE_2_0, out errors);

			Assert.IsFalse(ok, "Expected load failure");
			Assert.IsTrue(errors > 0, "Expected some failures to be returned");
			Assert.IsTrue(errorList.Count > 0, "Error list should have data in it");

			errorList.Clear();
		}

		#endregion

		#region BASI Schema
		/// <exclude/>
		[Test]
		public void BASI_Parse()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( BASI_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse( out errors );

			

			DateTime end = DateTime.Now;
			if (errors > 0)
			{
				SendErrorsToConsole(s.errorList);
			}
			Console.WriteLine( "Parse Time: {0}", end-start );

			PresentationLink pl = s.presentationInfo["http://www.xbrl.org/us/fr/lr/role/StatementCashFlowsDirect"] as PresentationLink;
			Assert.IsNotNull( pl, "presentation link not found" );

			PresentationLocator ploc = null;
			Assert.IsTrue( pl.TryGetLocator( "usfr-pt_NetIncreaseDecreaseCashCashEquivalents", out ploc ) );

			Assert.AreEqual( 0, errors, "parse failure" );	

			// 1 - duplicate element 
			if ( s.numWarnings > 7 )
			{
				Assert.AreEqual( s.numWarnings, SendWarningsToConsole( s.errorList ), "numWarnings doesn't match warnings reported in the errorList" );
			}

			Assert.AreEqual( 7, s.numWarnings, "wrong number of warnings " );

			TimeSpan level = new TimeSpan( 0, 0, 0, 8, 0 );	// 8 seconds to parse
			TimeSpan timeTaken = end - start;
			Assert.IsTrue(level > timeTaken, "Parse takes too long - " + timeTaken.ToString() + " seconds");
		}

		/// <exclude/>
		[Test]
		public void BASI_BlankElement()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			Node incStmtBasi = nodes[1] as Node;
			Assert.IsNotNull( incStmtBasi, "incStmtBasi is null" );
			Assert.IsNotNull( incStmtBasi.Children, "incStmtBasi.Children is null" );
	
			Node incStmt = incStmtBasi.Children[0] as Node;
			Assert.IsNotNull( incStmt, "incStmt is null" );
			Assert.IsNotNull( incStmt.Children, "incStmt.Children is null" );
			Assert.AreEqual( "IncomeExpensesAbstract", incStmt.Name );
			
			Node interestExpense = incStmt.Children[1] as Node;
			Assert.IsNotNull( interestExpense, "interestExpense is null" );
			Assert.AreEqual( "InterestExpenseAbstract", interestExpense.Name );
			Assert.IsNotNull( interestExpense.Children, "interestExpense.Children is null" );

			Node emptyElement = interestExpense.Children[5] as Node;
			Assert.IsNotNull( emptyElement, "emptyElement is null" );
		}

		/// <exclude/>
		[Test]
		public void BASI_LabelTest()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/2003/role/link" ] as PresentationLink;

			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/2003/role/link" );

			Node parentNode = pl.CreateNode( "en", "label" );
			System.Text.StringBuilder sb = TestTaxonomy_EDGAROnline.DisplayNode(parentNode, 0);
			Console.WriteLine(sb.ToString());

			// test the fourth child
			Node n = (Node)parentNode.Children[1];

			Assert.AreEqual( n.Label, "Contingencies", "label wrong" );
		}

		/// <exclude/>
		[Test]
		public void Test_BASI_ProhibitedLinks()
		{
			TestTaxonomy_2004_08_15 s = new TestTaxonomy_2004_08_15();
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

			s.currentLanguage = "en";
			s.currentLabelRole = "label";

			ArrayList nodes = s.GetNodesByPresentation();

			Node parentNode = nodes[0] as Node;

			Assert.IsNotNull( parentNode, "node null" );
			Assert.AreEqual( "http://www.xbrl.org/2003/role/link", parentNode.Label, "label name not correct" );
			Assert.AreEqual( 5, parentNode.Children.Count, "wrong number of children" );
			
			Node childOne = parentNode.Children[0] as Node;

			foreach ( Node childOfChildOne in childOne.Children )
			{
				Assert.IsTrue( childOfChildOne.IsProhibited, childOfChildOne.Label + " is not prohibited" );
			}
		}

		#endregion
	}
}
#endif
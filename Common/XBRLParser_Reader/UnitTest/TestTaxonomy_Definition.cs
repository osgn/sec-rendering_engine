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
namespace Aucent.MAX.AXE.Common.XBRLParser_Reader.Test
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Text;

	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.XBRLParser.Interfaces;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_Definition : Taxonomy
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Clear();
			Taxonomy.SkipDefinitionFileLoading = false;

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

		/// <exclude/>
		public static string ICI_FILE = TestCommon.FolderRoot + @"ici" + System.IO.Path.DirectorySeparatorChar + "ici-rr.xsd";

		/// <exclude/>
		public static string COMPANIES_HOUSE_FILE = TestCommon.FolderRoot + @"uk_CompaniesHouse" + System.IO.Path.DirectorySeparatorChar + "uk-gaap-ae-2005-06-01.xsd";

		/// <exclude/>
		public static string COREP_D_TY = TestCommon.FolderRoot + @"Corep Taxonomy 1.2 -- Sep 2006" + System.IO.Path.DirectorySeparatorChar + "d-ty-2006-07-01.xsd";


		/// <exclude/>
		[Test]
		public void Test_Load_ICI()
		{
			TestTaxonomy_Definition s = new TestTaxonomy_Definition();
			int errors = 0;

			DateTime start = DateTime.Now;
			if (s.Load(TestTaxonomy_Definition.ICI_FILE, out errors) != true)
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole ="preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse( out errors );

			Assert.AreEqual( 0, errors, "should not have any errors");

			ArrayList nodes = s.GetNodesByPresentation();

			foreach ( Node n in nodes )
			{
				StringBuilder sb = DisplayNode( n, 0 );
				Console.WriteLine( sb.ToString() );
			}

			//MAKE SURE THAT THE DIMENSION INFORMATION IS LOADED

			Assert.IsNotNull( s.NetDefinisionInfo, "Definition info should not be null");


			//make sure that the subs groups are defined properly at the element level
			Element ele = s.AllElements["ici-rr_RegistrantHypercube"] as Element;
			Assert.AreEqual( "xbrldt:hypercubeItem", ele.SubstitutionGroup, "Substitution group not correct");
			Assert.AreEqual( "xbrli:stringItemType", ele.ElementType, "Type not set correctly");

			ele = s.AllElements["ici-rr_RegistrantDimension"] as Element;
			Assert.AreEqual( "xbrldt:dimensionItem", ele.SubstitutionGroup, "Substitution group not correct");
			Assert.AreEqual( "xbrli:stringItemType", ele.ElementType, "Type not set correctly");

			ele = s.AllElements["ici-rr_AnticipatedEffectiveDate"] as Element;
			Assert.AreEqual( "xbrli:item", ele.SubstitutionGroup, "Substitution group not correct");
			Assert.AreEqual( "xbrli:dateItemType", ele.ElementType, "Type not set correctly");

		}

		/// <exclude/>
		[Test]
		public void Test_Load_UK_COMPANIES_HOUSE()
		{
			TestTaxonomy_Definition s = new TestTaxonomy_Definition();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( COMPANIES_HOUSE_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole ="preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse( out errors );
			Assert.AreEqual( 0, errors, "should not have any errors");

			ArrayList nodes = s.GetNodesByPresentation();

			foreach ( Node n in nodes )
			{
				StringBuilder sb = DisplayNode( n, 0 );
				Console.WriteLine( sb.ToString() );
			}

			//MAKE SURE THAT THE DIMENSION INFORMATION IS LOADED

			Assert.IsNotNull( s.NetDefinisionInfo, "Definition info should not be null");


			Dictionary<string, List<string>> requiresElementRelationships = s.NetDefinisionInfo.GetRequiredElementRelationshipInfo();

			Assert.IsTrue( requiresElementRelationships.Count == 5 , "should have some requires element relationships built");



			List<string> val = requiresElementRelationships["uk-gaap-ae_CompaniesHouseRegisteredNumber"] ;

			Assert.AreEqual( 8, val.Count );

			Assert.IsTrue( val.Contains( "uk-gaap-pt_NetAssetsLiabilitiesIncludingPensionAssetLiability"), "value not found in requires element relationhip");

			foreach( KeyValuePair<string, List<string>> kvp in  requiresElementRelationships)
			{
				foreach (string child in kvp.Value )
				{
					Console.WriteLine("Element {0} depends on Element {1}", kvp.Key, child);
				}
			}
		}


		/// <exclude/>
		[Test]
		public void Test_Load_COREP_D_TY()
		{
			TestTaxonomy_Definition s = new TestTaxonomy_Definition();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( COREP_D_TY, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole ="preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse( out errors );

			Assert.AreEqual( 0, errors, "should not ahve any errors");

			Element el = s.AllElements["d-ty_NationalMarketDimension"] as Element;

			Assert.AreEqual( "d-ty_NationalMarket", el.TypedDimensionId , "Typed dimension Id not set");


			el = s.AllElements["d-ty_SecuritizationInternalCode"] as Element;

			Assert.IsNull( el, "d-ty_SecuritizationInternalCode shouod not be an element as it oes not have a substitution group");


			IXBRLCustomType customType = s.customDataTypesHash["d-ty_SecuritizationInternalCode"] as IXBRLCustomType;

			Assert.IsNotNull( customType, "custom type did not get created");
			Assert.IsTrue( customType is XBRLSimpleType , "custom type is not correct");

			customType = s.customDataTypesHash["d-ty_BasicInformation"] as IXBRLCustomType;

			Assert.IsNotNull( customType, "custom type did not get created");
			Assert.IsTrue( customType is XBRLComplexType , "custom type is not correct");

			customType = s.customDataTypesHash["d-ty_InternalReferenceNumber"] as IXBRLCustomType;

			Assert.IsNotNull( customType, "custom type did not get created");
			Assert.IsTrue( customType is XBRLSimpleType , "custom type is not correct");

			customType = s.customDataTypesHash["d-ty_Day"] as IXBRLCustomType;

			Assert.IsNotNull( customType, "custom type did not get created");
			Assert.IsTrue( customType is XBRLSimpleType , "custom type is not correct");
		}

		/// <exclude/>
		protected StringBuilder DisplayNode(Node n, int level)
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
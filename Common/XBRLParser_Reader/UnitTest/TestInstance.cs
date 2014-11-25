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
namespace Aucent.MAX.AXE.XBRLParser.TestInstance
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.XBRLParser.Test;

	/// <exclude/>
	[TestFixture] 
    public class TestInstance : Instance
    {

		#region init
		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
        {}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
        {
			contexts.Clear();
			units.Clear();
			markups.Clear();
            DocumentTupleList = null;
			attributes.Clear();
			schemaRefs.Clear();

            DocumentTupleList = null;

			xDoc = null;
			theManager = null;

			mergeDocs = false;
			OverrideExistingDocument = false;

			fnProperties.Clear();
			footnoteLink = null;
		}
		#endregion

		#region overrides

		/// <exclude/>
		public bool ValidateAndParse()
		{
			ArrayList errors = new ArrayList();
			return ValidateAndParse(ref errors);
		}

		/// <exclude/>
		[Ignore("not a test")]
		public bool TestSetupNamespaceMgr()
		{
			SetupNamespaceMgr();
			return true;
		}
		#endregion

		#region Constants
#if DEBUG
		/// <exclude/>
		protected string INST_FILE = TestCommon.FolderRoot + @"instance.xml";
		
		/// <exclude/>
		protected string CBSO_04_07_FILE = TestCommon.FolderRoot + @"cbso-rf-2005-04-07" + System.IO.Path.DirectorySeparatorChar + "cbso-rf-2005-04-07.xsd";
#endif
		#endregion

		#region Helpers

		private Node CreateNode( string id, string name )
		{
			Element e = Element.CreateMonetaryElement( name, false, Element.BalanceType.debit, Element.PeriodType.instant );
			e.Id = id;
			TestNode n = new TestNode(id, e );
			return n;
		}

		private Node CreateNode( string id )
		{
			TestNode n = new TestNode(id, Element.CreateMonetaryElement( id, false, Element.BalanceType.debit, Element.PeriodType.instant ) );
			return n;
		}

		private Node CreateNode( string id, string name, string label )
		{
			Node n = CreateNode( id, name );
			n.Label = label;
			return n;
		}

		private Node CreateNode	( Element elem )
		{
			Node n = new Node(elem);
			return n;
		}

		private Precision CreatePrecision( Precision.PrecisionTypeCode typeCode )
		{
			return new Precision( typeCode );
		}

		private Precision CreatePrecision( Precision.PrecisionTypeCode typeCode, int decimals )
		{
			return new Precision( typeCode, decimals );
		}

		private UnitProperty CreateStandardUnit( string id, int scale, string ns, string schema, string val )
		{
			UnitProperty up = new UnitProperty( id, UnitProperty.UnitTypeCode.Standard );
			up.Scale = scale;
			up.StandardMeasure.MeasureNamespace = ns;
			up.StandardMeasure.MeasureSchema = schema;
			up.StandardMeasure.MeasureValue = val;
			return up;
		}

		private ContextProperty CreateInstantContext( string id, string schema, string val, DateTime start )
		{
			ContextProperty cp = new ContextProperty();
			cp.ContextID = id;
			cp.EntitySchema = schema;
			cp.EntityValue = val;
			cp.PeriodType = Element.PeriodType.instant;
			cp.PeriodStartDate = start;
			return cp;
		}

		private ContextProperty CreateDurationContext( string id, string schema, string val, DateTime start, DateTime end )
		{
			ContextProperty cp = new ContextProperty();
			cp.ContextID = id;
			cp.EntitySchema = schema;
			cp.EntityValue = val;
			cp.PeriodType = Element.PeriodType.duration;
			cp.PeriodStartDate = start;
			cp.PeriodEndDate = end;
			return cp;
		}

		private ContextProperty CreateDurationContextWithScenario( string id, string schema, string val, DateTime start, DateTime end )
		{
			ContextProperty cp = new ContextProperty();
			cp.ContextID = id;
			cp.EntitySchema = schema;
			cp.EntityValue = val;
			cp.PeriodType = Element.PeriodType.duration;
			cp.PeriodStartDate = start;
			cp.PeriodEndDate = end;
			cp.AddScenario("og", "Audited","rel","http://www.StandardAdvantage.com/em/og/2004-12-28");
			return cp;
		}
		#endregion


		#region footnotes

		#region footnote skeleton

		/// <exclude/>
		[Test]
		public void Test_FootnoteSkeleton()
		{
			xDoc = new XmlDocument();
			xDoc.AppendChild( xDoc.CreateXmlDeclaration( "1.0", "utf-16", null ) );
			XmlElement root = xDoc.CreateElement( XBRL );
			xDoc.AppendChild( root );

			root.SetAttribute(DocumentBase.XMLNS, DocumentBase.XBRL_INSTANCE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XBRL_LINKBASE_PREFIX), DocumentBase.XBRL_LINKBASE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XLINK_PREFIX), DocumentBase.XLINK_URI);

			CreateInstanceSkeleton( root, true, true );

			System.IO.StringWriter xml = new System.IO.StringWriter();
			xDoc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns=""http://www.xbrl.org/2003/instance"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
  <!--Context Section-->
  <!--Unit Section-->
  <!--Tuple Section-->
  <!--Element Section-->
  <!--Footnote Section-->
  <link:footnoteLink xlink:type=""extended"" xlink:role=""http://www.xbrl.org/2003/role/link"" />
</xbrl>";

			if( System.Environment.OSVersion.Platform.ToString() == "128")
			{
				expectedXml = expectedXml.Replace( "\r\n", "\n");
			}
			Assert.AreEqual( expectedXml, xml.ToString() );
		}
		#endregion

		/// <exclude/>
		[Test]
		public void Test_AddFootnoteLocatorsAndArcs()
		{
			xDoc = new XmlDocument();
			xDoc.AppendChild( xDoc.CreateXmlDeclaration( "1.0", "utf-16", null ) );
			XmlElement root = xDoc.CreateElement( XBRL );
			xDoc.AppendChild( root );

			root.SetAttribute(DocumentBase.XMLNS, DocumentBase.XBRL_INSTANCE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XBRL_LINKBASE_PREFIX), DocumentBase.XBRL_LINKBASE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XLINK_PREFIX), DocumentBase.XLINK_URI);

			CreateInstanceSkeleton( root, true, true );

			ArrayList mps = new ArrayList();

			// markup property 
			MarkupProperty mp = new MarkupProperty();
			mp.Id = "Item-01";
			mp.address = "$c$6";
			mp.Link( new FootnoteProperty( "Footnote-01", "$c$7", "en", "this is the footnote" ) );
			mp.xmlElement = xDoc.CreateElement( "usfr_pt:element1" );
			mps.Add( mp );

			// markup property 2
			MarkupProperty mp2 = new MarkupProperty();
			mp2.Id = "Item-02";
			mp2.address = "$b$6";
			mp2.Link( new FootnoteProperty( "Footnote-02", "$c$8", "en", "this is the footnote2" ) );
			mp2.xmlElement = xDoc.CreateElement( "usfr_pt:element2" );
			mps.Add( mp2 );

			AddFootnoteLocatorsAndArcs( mps );

			System.IO.StringWriter xml = new System.IO.StringWriter();
			xDoc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns=""http://www.xbrl.org/2003/instance"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
  <!--Context Section-->
  <!--Unit Section-->
  <!--Tuple Section-->
  <!--Element Section-->
  <!--Footnote Section-->
  <link:footnoteLink xlink:type=""extended"" xlink:role=""http://www.xbrl.org/2003/role/link"">
    <!--Document address: $c$6 - Element Name: usfr_pt:element1-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-01"" xlink:label=""Item-01_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-01_lbl"" xlink:to=""Footnote-01"" order=""1"" />
    <!--Document address: $b$6 - Element Name: usfr_pt:element2-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-02"" xlink:label=""Item-02_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-02_lbl"" xlink:to=""Footnote-02"" order=""1"" />
  </link:footnoteLink>
</xbrl>";

			if( System.Environment.OSVersion.Platform.ToString() == "128")
			{
				expectedXml = expectedXml.Replace( "\r\n", "\n");
			}
			Assert.AreEqual( expectedXml, xml.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_AddFootnoteResources()
		{
			xDoc = new XmlDocument();
			xDoc.AppendChild( xDoc.CreateXmlDeclaration( "1.0", "utf-16", null ) );
			XmlElement root = xDoc.CreateElement( XBRL );
			xDoc.AppendChild( root );

			root.SetAttribute(DocumentBase.XMLNS, DocumentBase.XBRL_INSTANCE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XBRL_LINKBASE_PREFIX), DocumentBase.XBRL_LINKBASE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XLINK_PREFIX), DocumentBase.XLINK_URI);

			CreateInstanceSkeleton( root, true, true );

			ArrayList mps = new ArrayList();

			// markup property 
			MarkupProperty mp = new MarkupProperty();
			mp.Id = "Item-01";
			mp.address = "$c$6";
			mp.Link( new FootnoteProperty( "Footnote-01", "$c$7", "en", "this is the footnote" ) );
			mp.xmlElement = xDoc.CreateElement( "usfr_pt:element1" );
			mps.Add( mp );

			// markup property 2
			MarkupProperty mp2 = new MarkupProperty();
			mp2.Id = "Item-02";
			mp2.address = "$b$6";
			mp2.Link( new FootnoteProperty( "Footnote-02", "$c$8", "en", "this is the footnote2" ) );
			mp2.xmlElement = xDoc.CreateElement( "usfr_pt:element2" );
			mps.Add( mp2 );

			AddFootnoteLocatorsAndArcs( mps );
			AddFootnoteResources( mps );

			System.IO.StringWriter xml = new System.IO.StringWriter();
			xDoc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns=""http://www.xbrl.org/2003/instance"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
  <!--Context Section-->
  <!--Unit Section-->
  <!--Tuple Section-->
  <!--Element Section-->
  <!--Footnote Section-->
  <link:footnoteLink xlink:type=""extended"" xlink:role=""http://www.xbrl.org/2003/role/link"">
    <!--Document address: $c$6 - Element Name: usfr_pt:element1-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-01"" xlink:label=""Item-01_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-01_lbl"" xlink:to=""Footnote-01"" order=""1"" />
    <!--Document address: $b$6 - Element Name: usfr_pt:element2-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-02"" xlink:label=""Item-02_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-02_lbl"" xlink:to=""Footnote-02"" order=""1"" />
    <!--Document address: $c$7-->
    <link:footnote xlink:type=""resource"" xlink:role=""http://www.xbrl.org/2003/role/footnote"" xlink:label=""Footnote-01"" xml:lang=""en"">this is the footnote</link:footnote>
    <!--Document address: $c$8-->
    <link:footnote xlink:type=""resource"" xlink:role=""http://www.xbrl.org/2003/role/footnote"" xlink:label=""Footnote-02"" xml:lang=""en"">this is the footnote2</link:footnote>
  </link:footnoteLink>
</xbrl>";

			if( System.Environment.OSVersion.Platform.ToString() == "128")
			{
				expectedXml = expectedXml.Replace( "\r\n", "\n");
			}
			Assert.AreEqual( expectedXml, xml.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_AddDuplicateFootnoteResources()
		{
			xDoc = new XmlDocument();
			xDoc.AppendChild( xDoc.CreateXmlDeclaration( "1.0", "utf-16", null ) );
			XmlElement root = xDoc.CreateElement( XBRL );
			xDoc.AppendChild( root );

			root.SetAttribute(DocumentBase.XMLNS, DocumentBase.XBRL_INSTANCE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XBRL_LINKBASE_PREFIX), DocumentBase.XBRL_LINKBASE_URL);
			root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XLINK_PREFIX), DocumentBase.XLINK_URI);

			CreateInstanceSkeleton( root, true, true );

			ArrayList mps = new ArrayList();

			// markup property 
			MarkupProperty mp = new MarkupProperty();
			mp.Id = "Item-01";
			mp.xmlElement = xDoc.CreateElement( "usfr_pt:element1" );
			mp.address = "$c$6";
			FootnoteProperty fp = new FootnoteProperty( "Footnote-01", "$c$7", "en", "this is the footnote" );
			mp.Link( fp );
			mps.Add( mp );

			// markup property 2
			MarkupProperty mp2 = new MarkupProperty();
			mp2.Id = "Item-02";
			mp2.xmlElement = xDoc.CreateElement( "usfr_pt:element2" );
			mp2.address = "$b$6";
			mp2.Link( fp );
			mps.Add( mp2 );

			AddFootnoteLocatorsAndArcs( mps );
			AddFootnoteResources( mps );

			System.IO.StringWriter xml = new System.IO.StringWriter();
			xDoc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns=""http://www.xbrl.org/2003/instance"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
  <!--Context Section-->
  <!--Unit Section-->
  <!--Tuple Section-->
  <!--Element Section-->
  <!--Footnote Section-->
  <link:footnoteLink xlink:type=""extended"" xlink:role=""http://www.xbrl.org/2003/role/link"">
    <!--Document address: $c$6 - Element Name: usfr_pt:element1-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-01"" xlink:label=""Item-01_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-01_lbl"" xlink:to=""Footnote-01"" order=""1"" />
    <!--Document address: $b$6 - Element Name: usfr_pt:element2-->
    <link:loc xlink:type=""locator"" xlink:href=""#Item-02"" xlink:label=""Item-02_lbl"" />
    <link:footnoteArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/fact-footnote"" xlink:from=""Item-02_lbl"" xlink:to=""Footnote-01"" order=""1"" />
    <!--Document address: $c$7-->
    <link:footnote xlink:type=""resource"" xlink:role=""http://www.xbrl.org/2003/role/footnote"" xlink:label=""Footnote-01"" xml:lang=""en"">this is the footnote</link:footnote>
  </link:footnoteLink>
</xbrl>";

			if( System.Environment.OSVersion.Platform.ToString() == "128")
			{
				expectedXml = expectedXml.Replace( "\r\n", "\n");
			}
			Assert.AreEqual( expectedXml, xml.ToString() );
		}

	
	
		
	
		#endregion

		#region Instance Doc Read
		/// <exclude/>
		[Test]
		public void ReadUsGaap_InstanceDocwithSegmentDimensions()
		{

			ArrayList errors = new ArrayList();
			bool ok = TryLoadInstanceDoc(TestCommon.FolderRoot + "Dimension instance segment.xml", out errors);

			if (errors.Count > 0)
			{
				errors.Sort();

				foreach (ParserMessage pm in errors)
				{
					if (pm.Level == TraceLevel.Error)
					{
						Console.WriteLine(pm.Message);
					}
					else
					{
						break;
					}
				}
			}
			int count = 0;
			foreach (ContextProperty cp in this.contexts)
			{
				foreach (Segment seg in cp.Segments)
				{

					if (seg.DimensionInfo != null)
					{
						Console.WriteLine(seg.ToString());
						count++;
					}
				}
			}
			Assert.AreEqual(24, count, "there should be a total of 24 dimension segments in the document");

		}

		/// <exclude/>
		[Test]
		public void ReadUsGaap_InstanceDocwithScenarioDimensions()
		{

			ArrayList errors = new ArrayList();
			bool ok = TryLoadInstanceDoc(TestCommon.FolderRoot + "Dimension instance scenario.xml", out errors);

			if (errors.Count > 0)
			{
				errors.Sort();

				foreach (ParserMessage pm in errors)
				{
					if (pm.Level == TraceLevel.Error)
					{
						Console.WriteLine(pm.Message);
					}
					else
					{
						break;
					}
				}
			}
			int count = 0;
			foreach (ContextProperty cp in this.contexts)
			{
				foreach (Scenario s in cp.Scenarios)
				{

					if (s.DimensionInfo != null)
					{
						Console.WriteLine(s.ToString());
						count++;
					}
				}
			}
			Assert.AreEqual(24, count, "there should be a total of 24 dimension scenarios in the document");

		}

		/// <exclude/>
		[Test]
		public void ReadICI_InstanceDoc()
		{
			string ICI_INSTANCE_FILE = TestCommon.FolderRoot + "ici-instance.xml";

			ArrayList errors = new ArrayList();
			bool ok = TryLoadInstanceDoc( ICI_INSTANCE_FILE, out errors );

			if ( errors.Count > 0 )
			{
				errors.Sort();

				foreach ( ParserMessage pm in errors )
				{
					if ( pm.Level == TraceLevel.Error )
					{
						Console.WriteLine( pm.Message );
					}
					else
					{
						break;
					}
				}
			}

			Assert.IsTrue( ok, "try load returned false" );
			Assert.AreEqual( 0, errors.Count );

            Assert.AreEqual(1, DocumentTupleList.Count, "wrong number of tuples returned");

            TupleSet theSet = (TupleSet)DocumentTupleList[0];
            List<MarkupProperty> allMarkups = new List<MarkupProperty>();

            theSet.GetAllMarkedupElements(ref allMarkups);
            Assert.AreEqual(185, allMarkups.Count, "wrong number of marked up children");

            MarkupProperty mp0 = allMarkups[0];
			Assert.AreEqual( "ici-rr_Heading", mp0.elementId, "element 0 wrong" );
			Assert.AreEqual( "THE SECURITIES AND EXCHANGE COMMISSION HAS NOT APPROVED OR DISAPPROVED THE FUND'S SHARES OR DETERMINED WHETHER THIS PROSPECTUS IS ACCURATE OR COMPLETE.  ANYONE WHO TELLS YOU OTHERWISE IS COMMITTING A CRIME.", mp0.markupData, "element 0 data wrong" );

			Assert.AreEqual( "ici-rr:Prospectus", mp0.TupleParentList[2], "element 0 parent 0 wrong" );
            Assert.AreEqual("ici-rr:RiskReturn", mp0.TupleParentList[1], "element 0 parent 1 wrong");
            Assert.AreEqual("ici-rr:IntroductionHeading", mp0.TupleParentList[0], "element 0 parent 2 wrong");

            MarkupProperty mp1 = allMarkups[1];
			Assert.AreEqual( "ici-rr_Heading", mp1.elementId, "element 1 wrong" );
			Assert.AreEqual( "RISK &RETURN SUMMARY", mp1.markupData, "element 1 data wrong" );
            Assert.AreEqual("ici-rr:Prospectus", mp1.TupleParentList[2], "element 0 parent 0 wrong");
            Assert.AreEqual("ici-rr:RiskReturn", mp1.TupleParentList[1], "element 0 parent 1 wrong");
            Assert.AreEqual("ici-rr:RiskReturnHeading", mp1.TupleParentList[0], "element 0 parent 2 wrong");

            MarkupProperty mp4 = allMarkups[4];
			Assert.AreEqual( "ici-rr_Heading", mp4.elementId, "element 4 wrong" );
			Assert.AreEqual( "PRINCIPAL INVESTMENT POLICIES AND STRATEGIES", mp4.markupData, "element 4 data wrong" );
            Assert.AreEqual("ici-rr:Prospectus", mp4.TupleParentList[3], "element 0 parent 0 wrong");
            Assert.AreEqual("ici-rr:RiskReturn", mp4.TupleParentList[2], "element 0 parent 1 wrong");
            Assert.AreEqual("ici-rr:StrategySection", mp4.TupleParentList[1], "element 4 parent 2 wrong");
            Assert.AreEqual("ici-rr:StrategyHeading", mp4.TupleParentList[0], "element 4 parent 3 wrong");

            MarkupProperty mp7 = allMarkups[7];
			Assert.AreEqual( "ici-rr_Paragraph", mp7.elementId, "element 7 wrong" );
			Assert.AreEqual( "The fund's investments may include securities traded in the over-the-counter markets.  The fund may invest up to 25% of its assets in the securities of a single issuer.", mp7.markupData, "element7 data wrong" );
            Assert.AreEqual("ici-rr:Prospectus", mp7.TupleParentList[3], "element 7 parent 0 wrong");
            Assert.AreEqual("ici-rr:RiskReturn", mp7.TupleParentList[2], "element 7 parent 1 wrong");
            Assert.AreEqual("ici-rr:StrategySection", mp7.TupleParentList[1], "element 7 parent 2 wrong");
            Assert.AreEqual("ici-rr:StrategyNarrativeParagraph", mp7.TupleParentList[0], "element 7 parent 3 wrong");

			foreach (ContextProperty cp in this.contexts)
			{
				foreach (Segment seg in cp.Segments)
				{
					Assert.IsNotNull(seg.DimensionInfo, "Dimension info should not be null");

					Assert.AreEqual("ici-rr_RegistrantDimension", seg.DimensionInfo.dimensionId);

					Console.WriteLine(seg.ToString());
				}
			}

		}
		#endregion


		#region Test Load with Dimensional Segment / Scenario

		/// <exclude/>
		[Test]
		public void TestLoadInstanceWithDimensionSegment()
		{
			string ICI_INSTANCE_FILE = TestCommon.FolderRoot + "ici-instance.xml";

			ArrayList errors = new ArrayList();
			bool ok = TryLoadInstanceDoc(ICI_INSTANCE_FILE, out errors);

			if (errors.Count > 0)
			{
				errors.Sort();

				foreach (ParserMessage pm in errors)
				{
					if (pm.Level == TraceLevel.Error)
					{
						Console.WriteLine(pm.Message);
					}
					else
					{
						break;
					}
				}
			}

			Assert.IsTrue(ok, "try load returned false");
			Assert.AreEqual(0, errors.Count);
		}
		#endregion

		#region read instance document with footnotes

		/// <exclude/>
		[Test]
		public void TestReadBrazilianPetroInstanceWithFootnotes()
		{
			TestInstance ins = new TestInstance();
			ArrayList errors = new ArrayList();
			Assert.IsTrue(ins.TryLoadInstanceDoc(@"S:\TestSchemas\BPC\pbra-20061231.xml", out errors), "Instance document load should succeed");

			Assert.AreEqual(1, ins.fnProperties.Count, "should have 6 footnotes");
			int count = 0;
			int mpWithfootNoteCount = 0;
			foreach (MarkupProperty mp in ins.markups)
			{
				if (mp.Id != null)
				{
					mpWithfootNoteCount++;
				}

				if (mp.Id != null && mp.Id.Equals("id_footnote_elem_6276445"))
				{
					count++;
					Assert.IsTrue(mp.Links.Count > 0, "count should be greater than zero");

					foreach (FootnoteProperty fp in mp.Links)
					{
						Console.WriteLine(fp.markupData);
					}
					
				}
			}

			Assert.IsTrue(mpWithfootNoteCount == 6, "count should be gequal to 6");
			Assert.IsTrue(count > 0, "count should be greater than zero");
		}
		#endregion


		#region inline XBRL loading example
		/// <exclude/>
		[Test]
		public void TestLoadInlineXBRLDocument()
		{
			string fileName = @"S:\TestSchemas\INLINEXBRL\msft-20080331.xml";
			TestInstance ins = new TestInstance();
			ArrayList errors = new ArrayList();
			Assert.IsTrue(ins.TryLoadInstanceDoc(fileName, out errors), "Instance document load should succeed");

			Assert.AreEqual(700, ins.markups.Count, "Should have 700 markups");
		}

		/// <exclude/>
		[Test, Ignore]
		public void TestLoadInlineXBRLDocument1()
		{
			string fileName = @"R:\Liberty Global\20080331\XBRLUSGAAPTaxonomies-2008-03-31\XBRLUSGAAPTaxonomies-2008-03-31\ind\ci\testing.xml";
			TestInstance ins1 = new TestInstance();
			ArrayList errors = new ArrayList();
			Assert.IsTrue(ins1.TryLoadInstanceDoc(fileName, out errors), "Instance document load should succeed");
			fileName = @"R:\Liberty Global\20080331\XBRLUSGAAPTaxonomies-2008-03-31\XBRLUSGAAPTaxonomies-2008-03-31\ind\ci\testing.html";
			TestInstance ins2 = new TestInstance();
			errors = new ArrayList();
			Assert.IsTrue(ins2.TryLoadInstanceDoc(fileName, out errors), "Instance document load should succeed");

			ins1.markups.Sort();

			ArrayList uniqueMarkups = new ArrayList();
			foreach (MarkupProperty mp in ins2.markups)
			{
				int index = ins1.markups.BinarySearch(mp);
				if (index < 0)
				{
					Console.WriteLine("Found markup not in xbrl ");
				}
			}

			Assert.AreEqual(ins1.markups.Count, ins2.markups.Count, "should have the same markups ");
			Assert.AreEqual(ins1.contexts.Count, ins2.contexts.Count, "should have the same contexts ");
			Assert.AreEqual(ins1.units.Count, ins2.units.Count, "should have the same contexts ");

			ins1.markups.Sort();
			ins2.markups.Sort();

			for (int i = 0; i < ins1.markups.Count; i++)
			{
				MarkupProperty mp1 = ins1.markups[i] as MarkupProperty;
				MarkupProperty mp2 = ins2.markups[i] as MarkupProperty;

				if (mp1.CompareTo(mp2) != 0)
				{
					Assert.Fail("Markups are not equal " + i.ToString());
				}
			}



		}
		#endregion
	}
}
#endif
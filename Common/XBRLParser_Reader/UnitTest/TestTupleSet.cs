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
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
	using System.Xml;
	using System.Collections;

    using NUnit.Framework;

	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestTupleSet 
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
        {}

		#endregion

		#region create from xml
		/// <exclude/>
		[Test]
		public void CreateFromXml()
		{
			string startXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<!--Blah Blah Blah-->
<!--Based on XBRL 2.1-->
<!--Blah Blah Blah-->
<xbrl xmlns=""http://www.xbrl.org/2003/instance"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:usfr_pt=""http://www.xbrl.org/2003/usfr"" xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"">
  <link:schemaRef xlink:type=""simple"" xlink:href=""test.xsd"" />
  <!--Context Section-->
  <context id=""current_AsOf"">
    <entity>
      <identifier scheme=""http://www.rivetsoftware.com"">Rivet</identifier>
    </entity>
    <period>
      <instant>2003-12-31</instant>
    </period>
  </context>
  <!--Unit Section-->
  <unit id=""u1"">
    <!--Scale: 0-->
    <measure>iso4217:USD</measure>
  </unit>
  <unit id=""u2"">
    <!--Scale: 3-->
    <measure>iso4217:USD</measure>
  </unit>
  <!--Tuple Section-->
  <usfr_pt:tupleParent1>
    <!--Tuple Set Name: set1-->
    <!--Document address: Excel - Sheet1!$A$2-->
    <usfr_pt:element2 contextRef=""current_AsOf"" unitRef=""u2"" decimals=""10"">98765000</usfr_pt:element2>
    <!--Tuple Set Name: set1-->
    <!--Document address: Excel - Sheet1!$A$1-->
    <usfr_pt:element1 contextRef=""current_AsOf"" unitRef=""u1"" decimals=""10"">12345</usfr_pt:element1>
  </usfr_pt:tupleParent1>
</xbrl>";

			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml( startXml );

			XmlNamespaceManager theManager = new XmlNamespaceManager( xDoc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );
			theManager.AddNamespace( "usfr_pt", "http://www.xbrl.org/2003/usfr" );

			ContextProperty cp = new ContextProperty( "current_AsOf" );
			cp.EntityValue = "Rivet";
			cp.EntitySchema = "http://www.rivetsoftware.com";
			cp.PeriodType = Element.PeriodType.instant;
			cp.PeriodStartDate = new DateTime( 2003, 12, 31 );

			ArrayList contexts = new ArrayList( );
			contexts.Add( cp );

			UnitProperty up = new UnitProperty( "u1", UnitProperty.UnitTypeCode.Standard );
			up.StandardMeasure.MeasureNamespace = "iso4217";
			up.StandardMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			up.StandardMeasure.MeasureValue = "USD";

			ArrayList units = new ArrayList();
			units.Add( up );

			UnitProperty up2 = new UnitProperty( "u2", UnitProperty.UnitTypeCode.Standard );
			up2.StandardMeasure.MeasureNamespace = "iso4217";
			up2.StandardMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			up2.StandardMeasure.MeasureValue = "USD";

			units.Add( up2 );

			XmlNodeList elemList = xDoc.SelectNodes( "//usfr_pt:tupleParent1", theManager );
			Assert.IsNotNull( elemList, "elem not found" );
			Assert.AreEqual( 1, elemList.Count, "elem count is wrong" );

            TupleSet ts = null;
            ArrayList tupleMarkups;
            ArrayList errors = new ArrayList();
            TupleSet.TryCreateFromXml(elemList[0], contexts, units, out ts, out tupleMarkups, ref errors);




            Assert.AreEqual(2, tupleMarkups.Count, "wrong number of elements returned");
            Assert.AreEqual(2, ts.Children.Count , "wrong number of elements returned");
        }

		#endregion
    }
}
#endif
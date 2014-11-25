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
	using System.IO;
	using System.Xml;
	using System.Collections;

	using System.Diagnostics;

	using NUnit.Framework;

	using Aucent.MAX.AXE.Common.Resources;

	/// <exclude/>
	[TestFixture] 
	public class TestUnitProperty : UnitProperty
	{

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestUnitProperty Comments***" );
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Console.WriteLine( "***End TestUnitProperty Comments***" );
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
		[Test]
		public void TestGetMeasurementString()
		{
			this.UnitType = UnitTypeCode.Standard;
			this.StandardMeasure.MeasureSchema = "standard schema";
			this.StandardMeasure.MeasureNamespace = "standard location";
			this.StandardMeasure.MeasureValue = "standard value";

			Assert.AreEqual("standard value",this.GetMeasurementString());

			this.UnitType = UnitTypeCode.Multiply;
			this.MultiplyMeasures[0].MeasureSchema = "multiply schema 1";
			this.MultiplyMeasures[0].MeasureNamespace = "multiply namespace 1";
			this.MultiplyMeasures[0].MeasureValue = "multiply value 1";
			this.MultiplyMeasures[1].MeasureSchema = "multiply schema 2";
			this.MultiplyMeasures[1].MeasureNamespace = "multiply namespace 2";
			this.MultiplyMeasures[1].MeasureValue = "multiply value 2";
		
			Assert.AreEqual("'multiply value 1' * 'multiply value 2'",this.GetMeasurementString());

			this.UnitType = UnitTypeCode.Divide;
			this.NumeratorMeasure.MeasureSchema = "numerator schema";
			this.NumeratorMeasure.MeasureNamespace = "numerator namespace";
			this.NumeratorMeasure.MeasureValue = "numerator value";
			this.DenominatorMeasure.MeasureSchema = "denominator schema";
			this.DenominatorMeasure.MeasureNamespace = "denominator namespace";
			this.DenominatorMeasure.MeasureValue = "denominator value";
		
			Assert.AreEqual("'numerator value' / 'denominator value'",this.GetMeasurementString());
		}

		/// <exclude/>
		[Test]
		public void Equals()
		{
			const string error = "Failure to correctly compare UnitProperty";
			UnitProperty unitProperty0 = new UnitProperty();
			unitProperty0.UnitID= "unitProperty0";
			Assert.AreEqual(true, unitProperty0.Equals(unitProperty0), error);

			UnitProperty unitProperty1 = new UnitProperty();
			unitProperty1.UnitID= "unitProperty1";
			Assert.AreEqual(false, unitProperty0.Equals(unitProperty1), error);
			Assert.AreEqual(true, unitProperty0.ValueEquals(unitProperty1), error);
		}

		/// <exclude/>
		[Test]
		public void Test_Append()
		{
			TestUnitProperty tup = new TestUnitProperty();

			tup.UnitID = "US_Dollars";
			tup.UnitType = UnitTypeCode.Standard;
			tup.StandardMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.StandardMeasure.MeasureNamespace = "iso4217";
			tup.StandardMeasure.MeasureValue="USD";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			tup.CreateElement( doc, root, errors, out unit, true );

			root.AppendChild( unit );

			string expectedXml = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"">
  <unit id=""US_Dollars"">
    <measure>iso4217:USD</measure>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_Append_NO_COMMENT()
		{
			TestUnitProperty tup = new TestUnitProperty();

			tup.UnitID = "US_Dollars";
			tup.UnitType = UnitTypeCode.Standard;
			tup.StandardMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.StandardMeasure.MeasureNamespace = "iso4217";
			tup.StandardMeasure.MeasureValue="USD";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			tup.CreateElement( doc, root, errors, out unit, false );

			root.AppendChild( unit );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"">
  <unit id=""US_Dollars"">
    <measure>iso4217:USD</measure>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		
		#region divide
		/// <exclude/>
		[Test]
		public void Test_BadDivide()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u6";
			tup.UnitType = UnitTypeCode.Divide;
			tup.NumeratorMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.NumeratorMeasure.MeasureNamespace = "iso4217";
			tup.NumeratorMeasure.MeasureValue = "EUR";

			tup.DenominatorMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.DenominatorMeasure.MeasureNamespace = "iso4217";
			tup.DenominatorMeasure.MeasureValue = "EUR";

			XmlDocument doc = new XmlDocument();

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsFalse( tup.CreateElement( doc, null, errors, out unit, true ) );
			Assert.AreEqual( 1, errors.Count );
			Assert.IsNull( unit );
		}

		/// <exclude/>
		[Test]
		public void Test_GoodDivide()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u6";
			tup.UnitType = UnitTypeCode.Divide;
			tup.NumeratorMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.NumeratorMeasure.MeasureNamespace = "iso4217";
			tup.NumeratorMeasure.MeasureValue = "EUR";

			tup.DenominatorMeasure.MeasureSchema = "http://www.xbrl.org/2003/instance";
			tup.DenominatorMeasure.MeasureNamespace = "xbrli";
			tup.DenominatorMeasure.MeasureValue = "shares";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, true ), "element not created" );
			Assert.AreEqual( 0, errors.Count, "errors returned" );
			Assert.IsNotNull( unit, "unit is null" );

			root.AppendChild( unit );

			string expectedXml = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"">
  <unit id=""u6"">
    <divide>
      <unitNumerator>
        <measure>iso4217:EUR</measure>
      </unitNumerator>
      <unitDenominator>
        <measure>xbrli:shares</measure>
      </unitDenominator>
    </divide>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString(), "expected xml is wrong" );
		}

		/// <exclude/>
		[Test]
		public void Test_GoodDivide_NO_COMMENT()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u6";
			tup.UnitType = UnitTypeCode.Divide;
			tup.NumeratorMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.NumeratorMeasure.MeasureNamespace = "iso4217";
			tup.NumeratorMeasure.MeasureValue = "EUR";

			tup.DenominatorMeasure.MeasureSchema = "http://www.xbrl.org/2003/instance";
			tup.DenominatorMeasure.MeasureNamespace = "xbrli";
			tup.DenominatorMeasure.MeasureValue = "shares";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, false ), "did not create element" );
			Assert.AreEqual( 0, errors.Count, "errors returned" );
			Assert.IsNotNull( unit, "unit is null" );

			root.AppendChild( unit );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"">
  <unit id=""u6"">
    <divide>
      <unitNumerator>
        <measure>iso4217:EUR</measure>
      </unitNumerator>
      <unitDenominator>
        <measure>xbrli:shares</measure>
      </unitDenominator>
    </divide>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString(), "expected xml is wrong" );
		}

		#endregion

		#region multiply
		/// <exclude/>
		[Test]
		public void Test_GoodMultiply()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u3";
			tup.UnitType = UnitTypeCode.Multiply;
			tup.MultiplyMeasures[0].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[0].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[0].MeasureValue = "feet";

			tup.MultiplyMeasures[1].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[1].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[1].MeasureValue = "feet";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );

			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, true ) );
			Assert.AreEqual( 0, errors.Count );
			Assert.IsNotNull( unit );

			root.AppendChild( unit );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:myuofm=""http://www.aucent.com"">
  <unit id=""u3"">
    <measure>myuofm:feet</measure>
    <measure>myuofm:feet</measure>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_GoodMultiply_NO_COMMENT()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u3";
			tup.UnitType = UnitTypeCode.Multiply;
			tup.MultiplyMeasures[0].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[0].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[0].MeasureValue = "feet";

			tup.MultiplyMeasures[1].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[1].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[1].MeasureValue = "feet";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );

			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, false ) );
			Assert.AreEqual( 0, errors.Count );
			Assert.IsNotNull( unit );

			root.AppendChild( unit );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:myuofm=""http://www.aucent.com"">
  <unit id=""u3"">
    <measure>myuofm:feet</measure>
    <measure>myuofm:feet</measure>
  </unit>
</xbrl>";

			//doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		#endregion

		#region repopulate from xml
		/// <exclude/>
		[Test]
		public void Test_FromXml()
		{
			TestUnitProperty tup = new TestUnitProperty();

			tup.UnitID = "US_Dollars";
			tup.UnitType = UnitTypeCode.Standard;
			tup.StandardMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.StandardMeasure.MeasureNamespace = "iso4217";
			tup.StandardMeasure.MeasureValue="USD";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, true ) );

			root.AppendChild( unit );

			UnitProperty upBack = null;

			Assert.IsTrue( UnitProperty.TryCreateFromXml( unit, null, out upBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tup.ValueEquals( upBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_GoodDivide()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u6";
			tup.UnitType = UnitTypeCode.Divide;
			tup.NumeratorMeasure.MeasureSchema = "http://www.xbrl.org/2003/iso4217";
			tup.NumeratorMeasure.MeasureNamespace = "iso4217";
			tup.NumeratorMeasure.MeasureValue = "EUR";

			tup.DenominatorMeasure.MeasureSchema = "http://www.xbrl.org/2003/instance";
			tup.DenominatorMeasure.MeasureNamespace = "xbrli";
			tup.DenominatorMeasure.MeasureValue = "shares";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, true ), "element not created" );
			Assert.AreEqual( 0, errors.Count, "errors returned" );
			Assert.IsNotNull( unit, "unit is null" );

			root.AppendChild( unit );

			UnitProperty upBack = null;

			Assert.IsTrue( UnitProperty.TryCreateFromXml( unit, null, out upBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tup.ValueEquals( upBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_GoodMultiply()
		{
			TestUnitProperty tup = new TestUnitProperty();
			tup.UnitID = "u3";
			tup.UnitType = UnitTypeCode.Multiply;
			tup.MultiplyMeasures[0].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[0].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[0].MeasureValue = "feet";

			tup.MultiplyMeasures[1].MeasureSchema = "http://www.aucent.com";
			tup.MultiplyMeasures[1].MeasureNamespace = "myuofm";
			tup.MultiplyMeasures[1].MeasureValue = "feet";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "xbrl" );

			doc.AppendChild( root );

			ArrayList errors = new ArrayList();

			XmlElement unit = null;
			Assert.IsTrue( tup.CreateElement( doc, root, errors, out unit, true ) );
			Assert.AreEqual( 0, errors.Count );
			Assert.IsNotNull( unit );

			root.AppendChild( unit );

			UnitProperty upBack = null;

			Assert.IsTrue( UnitProperty.TryCreateFromXml( unit, null, out upBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tup.ValueEquals( upBack ), "objects don't match" );
		}

		#endregion

	}
}
#endif
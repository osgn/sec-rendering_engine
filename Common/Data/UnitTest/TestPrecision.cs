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
namespace Aucent.MAX.AXE.Common.Data.Test
{
	using System;
	using System.Collections;

	using System.Xml;
	using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture] 
    public class TestPrecision : Precision
    {
#region init
        
		///<exclude/>
        [TestFixtureSetUp] public void RunFirst()
        {}

        ///<exclude/>
        [TestFixtureTearDown] public void RunLast() 
        {}

		///<exclude/>
        [SetUp] public void RunBeforeEachTest()
        {
			Precision p = new Precision();
			this.Name = p.Name;
			this.PrecisionType = p.PrecisionType;
			this.FaceValue = p.FaceValue;
			this.NumberOfDigits = p.NumberOfDigits;
		}

        ///<exclude/>
        [TearDown] public void RunAfterEachTest() 
        {}

#endregion

		/// <exclude/>
		[Test]
		public void Test_Append_DecimalINF()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Decimals );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""INF"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_Append_PrecisionINF()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Precision );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""INF"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_Append_INF_3()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Precision, 3 );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""-3"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_Append_Decimals_M7()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Decimals, -7 );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""-7"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void TestValueEquals()
		{
			const string error = "Invalid return from ValueEquals method";

			TestPrecision p = new TestPrecision();

			Assert.IsTrue(this.ValueEquals(p), error);

			p.PrecisionType = Precision.PrecisionTypeCode.Precision;
			Assert.IsFalse(this.ValueEquals(p), error);
			this.PrecisionType = p.PrecisionType;
			Assert.IsTrue(this.ValueEquals(p), error);

			this.FaceValue = false;
			p.FaceValue = true;
			Assert.IsFalse(this.ValueEquals(p), error);
			this.FaceValue = p.FaceValue;
			Assert.IsTrue(this.ValueEquals(p), error);

			p.NumberOfDigits = 9;
			Assert.IsFalse(this.ValueEquals(p), error);
			this.NumberOfDigits = p.NumberOfDigits;
			Assert.IsTrue(this.ValueEquals(p), error);
		}

		#region from xml
		/// <exclude/>
		[Test]
		public void Test_FromXml_Append_DecimalINF()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Decimals );

			XmlAttribute attr = pp.CreateAttribute( doc );

			Precision p = null;

			ArrayList errors = new ArrayList();

			Assert.IsTrue( Precision.TryCreateFromXml( attr, out p, ref errors ), "precision not created" );

			Assert.IsTrue( p.ValueEquals( pp ), "xml precision different from the real deal" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_Append_Decimals_M7()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			Precision pp = new Precision( Precision.PrecisionTypeCode.Decimals, -7 );

			XmlAttribute attr = pp.CreateAttribute( doc );

			Precision p = null;

			ArrayList errors = new ArrayList();

			Assert.IsTrue( Precision.TryCreateFromXml( attr, out p, ref errors ), "precision not created" );

			Assert.IsTrue( p.ValueEquals( pp ), "xml precision different from the real deal" );
		}

		#endregion
    }
}
#endif
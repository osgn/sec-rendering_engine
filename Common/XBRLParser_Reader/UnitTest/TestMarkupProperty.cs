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
	using NUnit.Framework;

	using System;
	using System.Xml;
	using System.Collections;

	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture] 
	public class TestMarkupProperty : MarkupProperty
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

		#region validation


		private void PopulateNumeric()
		{
			Element.PeriodType periodType = Element.PeriodType.duration;
			element = new Node( Element.CreateMonetaryElement("test", false, Element.BalanceType.credit, periodType) );
			contextRef = new ContextProperty();
			contextRef.PeriodType = periodType;

			unitRef = new UnitProperty();

			markupData = "-123.12";
		}

		private void PopulateNonNumeric()
		{
			Element.PeriodType periodType = Element.PeriodType.duration;
			element = new Node( Element.CreateElement(Element.DataTypeCode.String, "test", false, periodType) );
			contextRef = new ContextProperty();
			contextRef.PeriodType = periodType;

			unitRef = null;

			markupData = "Test Markup Data";
		}

#if OBSOLETE // moved to markup wrapper
		private void TestValidate(int expectedErrorCount)
		{
			const string invalidReturn = "Invalid return from Validate method";
			const string invalidErrorCount = "Invalid validation errors count";
			const string invalidMarkupStatus = "Invalid markup status";

			if(expectedErrorCount == 0)
                Assert.AreEqual(true, Validate(), invalidReturn);
			else
				Assert.AreEqual(false, Validate(), invalidReturn);

			Assert.AreEqual(expectedErrorCount, ValidationErrors.Count, invalidErrorCount);

			if(expectedErrorCount == 0)
				Assert.AreEqual(ValidationStatus.OK, MarkupStatus, invalidMarkupStatus);
			else
				Assert.IsFalse(ValidationStatus.OK == MarkupStatus, invalidMarkupStatus);
		}


		/// <exclude/>
		[Test][Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateValid()
		{
			PopulateNumeric();
			MarkupStatus = ValidationStatus.ERROR;
			ValidationErrors.Add("test");
			//TestValidate(0);

			PopulateNonNumeric();
			MarkupStatus = ValidationStatus.ERROR;
			ValidationErrors.Add("test");
			//TestValidate(0);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateMissingElement()
		{
			PopulateNumeric();
			element = null;
			//TestValidate(1);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateMissingContext()
		{
			PopulateNumeric();
			contextRef = null;
			//TestValidate(1);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateInvalidPeriodType()
		{
			PopulateNumeric();
			element.MyElement.PerType = Element.PeriodType.duration;
			contextRef.PeriodType = Element.PeriodType.forever;
			//TestValidate(1);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateInvalidBalanceType()
		{
			PopulateNumeric();
			element.MyElement.BalType = Element.BalanceType.debit;
			//TestValidate(1);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateValidBalanceType()
		{
			PopulateNumeric();
			element.MyElement.BalType = Element.BalanceType.credit;
			//TestValidate(0);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateInvalidNumeric()
		{
			PopulateNumeric();
			unitRef = null;
			//TestValidate(1);
		}

		/// <exclude/>
		[Test] [Ignore( "Validation moved to MarkupWrapper")]
		public void ValidateInvalidNonNumeric()
		{
			PopulateNonNumeric();
			unitRef = new UnitProperty();
			//TestValidate(1);
		}
#endif
		/// <exclude/>
		[Test]
		public void SetError()
		{
			MarkupStatus = ValidationStatus.OK;
			ArrayList errors = new ArrayList();

			const string incorrectValidationStatus = "Failed to correctly set validation status";
			const string incorrectErrorCount = "Failed to correctly append to error list";

			SetError(ValidationStatus.ERROR, null, "error");
			SetError(ValidationStatus.ERROR, errors, null);
			SetError(ValidationStatus.OK, errors, "error");
			Assert.AreEqual(ValidationStatus.OK, MarkupStatus, incorrectValidationStatus);
			Assert.AreEqual(0, errors.Count, incorrectErrorCount);

			SetError(ValidationStatus.INCOMPLETE, errors, "test");
			Assert.AreEqual(ValidationStatus.INCOMPLETE, MarkupStatus, incorrectValidationStatus);
			Assert.AreEqual(1, errors.Count, incorrectErrorCount);

			SetError(ValidationStatus.ERROR, errors, "test");
			Assert.AreEqual(ValidationStatus.ERROR, MarkupStatus, incorrectValidationStatus);
			Assert.AreEqual(2, errors.Count, incorrectErrorCount);

			// should remain "ERROR" but append to error list
			SetError(ValidationStatus.INCOMPLETE, errors, "test");
			Assert.AreEqual(ValidationStatus.ERROR, MarkupStatus, incorrectValidationStatus);
			Assert.AreEqual(3, errors.Count, incorrectErrorCount);

			SetError(ValidationStatus.OK, errors, "test");
			Assert.AreEqual(ValidationStatus.OK, MarkupStatus, incorrectValidationStatus);
			Assert.AreEqual(3, errors.Count, incorrectErrorCount);
		}

		#endregion

		#region repopulate from xml
		/// <exclude/>
		[Test]
		public void CreateFromXml()
		{
			string startXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<?xml-stylesheet type=""text/xsl"" href=""Test.xsl""?>
<!--XBRLParser message-->
<!--Based on XBRL 2.1-->
<!--Created on SomeDate-->
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
    <!--Scale: 2-->
    <measure>iso4217:USD</measure>
  </unit>
  <!--Element Section-->
  <!--Document address: Excel - Sheet1!$A$1-->
  <usfr_pt:element1 contextRef=""current_AsOf"" unitRef=""u1"" decimals=""10"">1234500</usfr_pt:element1>
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

			MarkupProperty realMP = new MarkupProperty();
			realMP.contextRef = cp;
			realMP.unitRef = up;
			realMP.elementId = "usfr_pt_element1";
			realMP.element = new Node( new Element( "element1" ) );
			realMP.markupData = "1234500";
			realMP.precisionDef = new Precision( Precision.PrecisionTypeCode.Decimals, 10 );

			ArrayList units = new ArrayList();
			units.Add( up );

			XmlNodeList elemList = xDoc.SelectNodes( "//usfr_pt:element1", theManager );
			Assert.AreEqual( 1, elemList.Count, "elem not found" );

			MarkupProperty mp = null;

			ArrayList errors = new ArrayList();

			Assert.IsTrue( MarkupProperty.TryCreateFromXml( 0, elemList, contexts, units, out mp, ref errors ), "markup property not created" );

			Assert.IsTrue( mp.Equals( realMP ), "elem different from the real deal" );
			Assert.AreEqual( "http://www.xbrl.org/2003/usfr", mp.elementNamespace, "namespace wrong" );
		}


        [Test]
        public void TestIsMarkupMorePreciseThanRoundedValue()
        {
            MarkupProperty mp = new MarkupProperty();
            mp.markupData = "123456";
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, -3);
            mp.unitRef = new UnitProperty();
            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());

            mp.unitRef.Scale = 2;
            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());

            mp.unitRef.Scale = 3;
            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());

            mp.markupData = "123456000";
            mp.unitRef.Scale = 0;
            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "126322";
            mp.unitRef.Scale = 4;
            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "126322.3";
            mp.unitRef.Scale = 4;
            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "126322.3";
            mp.unitRef.Scale = 3;
            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());



            mp.markupData = "567456498";
            mp.unitRef.Scale = 0;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 0);

            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());



            //some per share info
            mp.markupData = "1.99";
            mp.unitRef.Scale = 0;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 2);

            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "0.99";
            mp.unitRef.Scale = 0;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 2);

            Assert.IsFalse(mp.IsMarkupMorePreciseThanRoundedValue());



            mp.markupData = "1.991";
            mp.unitRef.Scale = 0;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 2);

            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "9.991";
            mp.unitRef.Scale = 0;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 2);

            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());


            mp.markupData = "476403";
            mp.unitRef.Scale = 3;
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, -6);

            Assert.IsTrue(mp.IsMarkupMorePreciseThanRoundedValue());



        }


		/// <exclude/>
		[Test]
		public void TestGetRoundedMarkedupAmount()
		{
			MarkupProperty mp = new MarkupProperty();
			mp.markupData = "123456.789012";
			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, -3);
			Assert.AreEqual(123000, mp.GetRoundedMarkedupAmount());

			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, -2);
			Assert.AreEqual(123500, mp.GetRoundedMarkedupAmount());

			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 0);
			Assert.AreEqual(123457, mp.GetRoundedMarkedupAmount());
			
			
			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 2);
			Assert.AreEqual(123456.79, mp.GetRoundedMarkedupAmount());
			
			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 4);
			Assert.AreEqual(123456.7890, mp.GetRoundedMarkedupAmount());

			mp.markupData = "(123,456.789012)";
			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, -2);
			Assert.AreEqual(-123500, mp.GetRoundedMarkedupAmount());

			mp.markupData = "22.98";
			mp.precisionDef = new Precision(Precision.PrecisionTypeCode.None);
			mp.unitRef = new UnitProperty();

			decimal sum = mp.GetRoundedMarkedupAmount();
			Console.WriteLine(sum);

			mp.markupData = "3.478";

			sum += mp.GetRoundedMarkedupAmount();

			Assert.AreEqual(26.458, sum); //with double we get floating point error....
			Console.WriteLine(sum);
			mp.markupData = "2.3";

			sum += mp.GetRoundedMarkedupAmount();
			Console.WriteLine(sum);
			mp.markupData = "3.478";

			sum += mp.GetRoundedMarkedupAmount();

			Console.WriteLine(sum);



            mp.markupData = "345000012";
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 0);
            Console.WriteLine(mp.GetRoundedMarkedupAmount());



        
            mp.markupData = "34501.67";
            mp.precisionDef = new Precision(Precision.PrecisionTypeCode.Decimals, 0);
            Console.WriteLine(mp.GetRoundedMarkedupAmount());



		}
#endregion
	}
}
#endif
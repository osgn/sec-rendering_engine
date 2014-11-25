//=============================================================================
// TestPrecisionProperty (class)
// Aucent Corporation
//=============================================================================

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
    using System.Xml;

	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser;

    [TestFixture] 
    public class TestPrecisionProperty : PrecisionProperty
    {
		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        [TestFixtureSetUp] public void RunFirst()
        {}

        /// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {}

		/// <summary> Sets up test values before each test is called </summary>
        [SetUp] public void RunBeforeEachTest()
        {}

        /// <summary>Tears down test values after each test is run </summary>
        [TearDown] public void RunAfterEachTest() 
        {}

		#endregion

		[Test] public void Test_Append_DecimalINF()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			PrecisionProperty pp = new PrecisionProperty( PrecisionProperty.PrecisionTypeCode.Decimals );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""INF"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		[Test] public void Test_Append_PrecisionINF()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			PrecisionProperty pp = new PrecisionProperty( PrecisionProperty.PrecisionTypeCode.Precision );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test precision=""INF"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		[Test] public void Test_Append_INF_3()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			PrecisionProperty pp = new PrecisionProperty( PrecisionProperty.PrecisionTypeCode.Precision, 3 );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test precision=""3"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		[Test] public void Test_Append_Decimals_M7()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement elem = doc.CreateElement( "test" );
			doc.AppendChild( elem );

			PrecisionProperty pp = new PrecisionProperty( PrecisionProperty.PrecisionTypeCode.Decimals, -7 );

			elem.Attributes.Append( pp.CreateAttribute( doc ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<test decimals=""-7"" />";

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}
	}
}
#endif
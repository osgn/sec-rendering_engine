//=============================================================================
// TestInstanceReportColumn (unit test class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// Test methods in the data class InstanceReportColumn.
//=============================================================================

using System;
using System.IO;

using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;
using Aucent.MAX.AXE.XBRLReportBuilder;
using XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{   
    public class TestInstanceReportColumn : InstanceReportColumn
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

		public static InstanceReportColumn LoadFromXml( string xml )
		{
			StringReader sr = new StringReader( xml );
			
			XmlSerializer xmlReport = new XmlSerializer(typeof(InstanceReportColumn));

			InstanceReportColumn ir = (InstanceReportColumn)xmlReport.Deserialize( sr );
			sr.Close();       

			return ir;
		}
    }
}
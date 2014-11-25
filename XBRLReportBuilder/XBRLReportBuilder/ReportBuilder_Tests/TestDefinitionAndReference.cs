//=============================================================================
// TestDefinitionAndReference (unit test class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// Test definition and references.
//=============================================================================

using System;
using System.IO;
using System.Xml;

using NUnit.Framework;
using XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
    [TestFixture] 
    public class TestDefinitionAndReference : DefinitionAndReference
    {
		private string assemblyPath = string.Empty;
		protected string baseDir
		{
			get
			{
				if( string.IsNullOrEmpty( this.assemblyPath ) )
				{
					//holds the assembly file path
					this.assemblyPath = this.GetType().Assembly.CodeBase.Substring( 8 );

					//now it holds the assembly directory (hopefully `bin`)
					this.assemblyPath = Path.GetDirectoryName( this.assemblyPath );

					//now it holds the assembly directory parent (hopefully this is the project root)
					this.assemblyPath = Path.GetDirectoryName( this.assemblyPath );
				}

				return Path.Combine( this.assemblyPath, this.relativeRoot );
			}
		}

		private string relativeRoot = @"TestFiles\Out";
		private string reportDir = null;

		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        [TestFixtureSetUp] public void RunFirst()
        {}

        /// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {}

		/// <summary> Sets up test values before each test is called </summary>
        [SetUp] public void RunBeforeEachTest()
        {
			// both 128 and 4 signify Mono 
			// 128 is returned by .NET 1.1
			// 4 is returned by .NET 2.0
			//baseDir = ( (int)Environment.OSVersion.Platform == 128 || (int)Environment.OSVersion.Platform == 4 ) ? "." : @"t:/ReportBuilder";

			this.reportDir = this.baseDir;
			if( !Directory.Exists( this.reportDir ) )
			{
				Directory.CreateDirectory( this.reportDir );
			}
		}

        /// <summary>Tears down test values after each test is run </summary>
        [TearDown] public void RunAfterEachTest() 
        {
			references.Clear();
		}


		#endregion

		[Test]
		public void SimpleWriteTest()
		{
			AddReference( "key1", "value1" );
			AddReference( "key2", "value2" );
			AddReference( "key3", "value3" );

			BuildXmlDocument( reportDir + @"/authRefSimpleTest.xml" );
		}
	
		[Test]
		public void SimpleReferenceXPathTest()
		{
			AddDefinition( "key1", "value1" );
			AddReference( "key2", "value2" );
			AddReference( "key3", "value3" );

			BuildXmlDocument( reportDir + @"/authRefSimpleTest2.xml" );

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load( reportDir + @"/authRefSimpleTest2.xml" );

			XmlNode n = xDoc.FirstChild.SelectSingleNode( "Element[@Id=\"key2\"]/reference" );

			Assert.IsNotNull( n, "n1 is null" );
			Assert.AreEqual( "value2", n.InnerText );

			XmlNode n2 = xDoc.FirstChild.SelectSingleNode( "Element[@Id=\"key1\"]/reference" );

			Assert.IsNull( n2, "n2 is null" );
		}
	}
}
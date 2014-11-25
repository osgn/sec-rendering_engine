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
	using System.Diagnostics;

	using Aucent.MAX.AXE.XBRLParser;

	using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Resources;

	/// <exclude/>
	[TestFixture] 
	public class TestMeasure : Measure
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestMeasure Comments***" );
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Console.WriteLine( "***End TestMeasure Comments***" );
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

		#region Validation
		/// <exclude/>
		[Test]
		public void Test_4217_Bad()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "bad";
			tm.MeasureSchema = DocumentBase.XBRL_INSTANCE_URL;

			ArrayList errors = new ArrayList();

			Assert.IsFalse( tm.Validate( errors ) );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 1, errors.Count );
		}

		/// <exclude/>
		[Test]
		public void Test_4217_Good()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "iso4217";
			tm.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			ArrayList errors = new ArrayList();

			Assert.IsTrue( tm.Validate(  errors ) );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 0, errors.Count );
		}

		/// <exclude/>
		[Test]
		public void Test_Instance_Bad()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "bad";
			tm.MeasureNamespace = "blah";
			tm.MeasureSchema = "http://www.xbrl.org/2003/instance";

			ArrayList errors = new ArrayList();

			Assert.IsFalse( tm.Validate( errors ) );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 1, errors.Count );
		}

		/// <exclude/>
		[Test]
		public void Test_Instance_Pure()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "pure";
			tm.MeasureNamespace = "blah";
			tm.MeasureSchema = "http://www.xbrl.org/2003/instance";

			ArrayList errors = new ArrayList();

			Assert.IsTrue( tm.Validate( errors ) );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 0, errors.Count );
		}

		/// <exclude/>
		[Test]
		public void Test_Instance_Shares()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "shares";
			tm.MeasureNamespace = "blah";
			tm.MeasureSchema = "http://www.xbrl.org/2003/instance";

			ArrayList errors = new ArrayList();

			bool valid = tm.Validate( errors );
			string msg = null;

			if ( !valid )
			{
				msg = ((ParserMessage)errors[0]).Message;
			}

			Assert.IsTrue( valid, msg );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 0, errors.Count );
		}

		/// <exclude/>
		[Test]
		public void Test_Instance_SharesWithNoNamespace()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "shares";
			tm.MeasureNamespace = "";
			tm.MeasureSchema = "http://www.xbrl.org/2003/instance";

			ArrayList errors = new ArrayList();

			bool valid = tm.Validate( errors );
			string msg = null;

			if ( !valid )
			{
				msg = ((ParserMessage)errors[0]).Message;
			}

			Assert.IsTrue( valid, msg );
			Assert.IsNotNull( errors );
			Assert.AreEqual( 0, errors.Count );
		}

		#endregion

		#region Create Xml
		/// <exclude/>
		[Test]
		public void Test_4217_Element()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "iso4217";
			tm.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			string expectedXml = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"">
  <measure>iso4217:USD</measure>
</xbrl>";

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );
			root.AppendChild( tm.CreateElement( doc, root, false ) );

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
	
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_AppendNamespace()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "iso4217";
			tm.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			string expectedXml = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"" />";

			XmlDocument doc = new XmlDocument();
			XmlElement elem = doc.CreateElement( "xbrl" );
			doc.AppendChild( elem );

			tm.AppendNamespace( elem );

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
	
			Assert.AreEqual( expectedXml, writer.ToString() );
		}

		/// <exclude/>
		[Test]
		public void Test_AppendMultipleNamespaces()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "iso4217";
			tm.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			TestMeasure tm2 = new TestMeasure();
			tm2.MeasureValue = "USD";
			tm2.MeasureNamespace = "iso4217";
			tm2.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<xbrl xmlns:iso4217=""http://www.xbrl.org/2003/iso4217"" />";

			XmlDocument doc = new XmlDocument();
			XmlElement elem = doc.CreateElement( "xbrl" );
			doc.AppendChild( elem );

			tm.AppendNamespace( elem );
			tm2.AppendNamespace( elem );

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
	
			Assert.AreEqual( expectedXml, writer.ToString() );
		}
		#endregion

		#region from xml
		/// <exclude/>
		[Test]
		public void Test_FromXml_4217_Element()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "USD";
			tm.MeasureNamespace = "iso4217";
			tm.MeasureSchema = "http://www.xbrl.org/2003/iso4217";

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "xbrl" );
			doc.AppendChild( root );
			XmlNode elem = tm.CreateElement( doc, root, false );
			
			ArrayList errors = null;
			
			Measure mBack = new Measure();

			Assert.IsTrue( Measure.TryCreateFromXml( elem, ref mBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tm.Equals( mBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_SharesWithNoNamespace()
		{
			TestMeasure tm = new TestMeasure();
			tm.MeasureValue = "shares";
			tm.MeasureNamespace = "";
			tm.MeasureSchema = "http://www.xbrl.org/2003/instance";

			XmlDocument doc = new XmlDocument();
			// set the default namespace for the root, since the element doesn't have a namespace
			XmlElement root = doc.CreateElement( "xbrl", "http://www.xbrl.org/2003/instance");
			doc.AppendChild( root );
			XmlNode elem = tm.CreateElement( doc, root, false );
			
			ArrayList errors = null;
			Measure mBack = new Measure();

			Assert.IsTrue( Measure.TryCreateFromXml( elem, ref mBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tm.Equals( mBack ), "objects don't match" );
		}
		#endregion
	}
}
#endif
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
	using System.Collections;

	using System.Xml;
	using System.Text;
	using System.Diagnostics;

	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.XBRLParser;

	using NUnit.Framework;

	/// <exclude/>
	[TestFixture] 
	public class TestContextProperty : ContextProperty
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestContextProperty Comments***" );
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Console.WriteLine( "***End TestContextProperty Comments***" );
		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{
			Scenarios.Clear();
			Segments.Clear();
		}

		#endregion


		#region Create Current For Period
		/// <exclude/>
		[Test]
		public void CreateCurrentForPeriod()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			string expectedXML = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<root>
  <context id=""current_ForPeriod"">
    <entity>
      <identifier scheme=""http://www.aucent.com/test"">Aucent Corporation</identifier>
    </entity>
    <period>
      <startDate>2004-01-15</startDate>
      <endDate>2004-07-31</endDate>
    </period>
  </context>
</root>";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			root.AppendChild(tcp.Append(doc, root, null));

			doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}
		#endregion

		#region Create Current For Period With Scenario
		/// <exclude/>
		[Test]
		public void CreateCurrentForPeriod_WithScenario()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:fid=""http://www.someInsuranceCo.com/scenarios"">
  <context id=""current_ForPeriod"">
    <entity>
      <identifier scheme=""http://www.aucent.com/test"">Aucent Corporation</identifier>
    </entity>
    <period>
      <startDate>2004-01-15</startDate>
      <endDate>2004-07-31</endDate>
    </period>
    <scenario>
      <fid:residence>MA</fid:residence>
      <fid:nonSmoker>true</fid:nonSmoker>
      <fid:minAge>34</fid:minAge>
      <fid:maxAge>49</fid:maxAge>
    </scenario>
  </context>
</root>";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			root.AppendChild(tcp.Append(doc, root, null));

			doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}
		#endregion

		#region Create Current For Period With Scenario and Segment
		/// <exclude/>
		[Test]
		public void CreateCurrentForPeriod_WithScenarioAndSegment()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			tcp.Segments.Add( new Segment( "name", "MI", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "CO", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "AL", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "AK", "stateProvince", "my", "http://www.someCompany.com/segment" ) );

			string expectedXML = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:my=""http://www.someCompany.com/segment"" xmlns:fid=""http://www.someInsuranceCo.com/scenarios"">
  <context id=""current_ForPeriod"">
    <entity>
      <identifier scheme=""http://www.aucent.com/test"">Aucent Corporation</identifier>
      <segment>
        <my:stateProvince>MI</my:stateProvince>
        <my:stateProvince>CO</my:stateProvince>
        <my:stateProvince>AL</my:stateProvince>
        <my:stateProvince>AK</my:stateProvince>
      </segment>
    </entity>
    <period>
      <startDate>2004-01-15</startDate>
      <endDate>2004-07-31</endDate>
    </period>
    <scenario>
      <fid:residence>MA</fid:residence>
      <fid:nonSmoker>true</fid:nonSmoker>
      <fid:minAge>34</fid:minAge>
      <fid:maxAge>49</fid:maxAge>
    </scenario>
  </context>
</root>";

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			root.AppendChild(tcp.Append(doc, root, null));

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			
			Console.WriteLine( writer.ToString() );

			Assert.AreEqual( expectedXML, writer.ToString() );
		}
		#endregion

		/// <exclude/>
		[Test]
		public void TestGetPeriodString()
		{
			this.PeriodType = Element.PeriodType.duration;
			this.PeriodStartDate = new DateTime(2004,1,14);
			this.PeriodEndDate = new DateTime(2004,2,14);
			Assert.AreEqual("1/14/2004 - 2/14/2004",this.GetPeriodString());

			this.PeriodType = Element.PeriodType.instant;
			Assert.AreEqual("1/14/2004",this.GetPeriodString());

			this.PeriodType = Element.PeriodType.forever;
			Assert.AreEqual("Forever",this.GetPeriodString());

			this.PeriodType = Element.PeriodType.na;
			Assert.AreEqual("None",this.GetPeriodString());
		}

		/// <exclude/>
		[Test]
		public void Test_CurrentAsOf()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_AsOf";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 7, 9 );
			tcp.PeriodType = Element.PeriodType.instant;

			string expectedXML = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<root>
  <context id=""current_AsOf"">
    <entity>
      <identifier scheme=""http://www.aucent.com/test"">Aucent Corporation</identifier>
    </entity>
    <period>
      <instant>2004-07-09</instant>
    </period>
  </context>
</root>";


			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			root.AppendChild(tcp.Append(doc, root, null));

			doc.PreserveWhitespace = false; 

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );
			Assert.AreEqual( expectedXML, writer.ToString() );
		}

		#region scenarios
		/// <exclude/>
		[Test]
		public void Test_ScenarioToXml()
		{
			Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			XmlElement contextElem = doc.CreateElement( "context" );

			doc.AppendChild( root );
			root.AppendChild( contextElem );

			WriteScenarios(contextElem, root, doc, false, null);

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );

			Console.WriteLine( writer.ToString() );

			string expectedXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:fid=""http://www.someInsuranceCo.com/scenarios"">
  <context>
    <scenario>
      <fid:residence>MA</fid:residence>
    </scenario>
  </context>
</root>";

			Assert.AreEqual( expectedXml, writer.ToString() );

		}

		/// <exclude/>
		[Test]
		public void Test_MultipleScenarioToXml()
		{
			Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			XmlElement contextElem = doc.CreateElement( "context" );

			doc.AppendChild( root );
			root.AppendChild( contextElem );

			WriteScenarios(contextElem, root, doc, false, null);

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );

			Console.WriteLine( writer.ToString() );

			string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:fid=""http://www.someInsuranceCo.com/scenarios"">
  <context>
    <scenario>
      <fid:residence>MA</fid:residence>
      <fid:nonSmoker>true</fid:nonSmoker>
      <fid:minAge>34</fid:minAge>
      <fid:maxAge>49</fid:maxAge>
    </scenario>
  </context>
</root>";

			Assert.AreEqual( expectedXml, writer.ToString() );

		}
		#endregion

		#region segments
		/// <exclude/>
		[Test]
		public void Test_SegmentToXml()
		{
			Segments.Add( new Segment( "name", "MI", "stateProvince", "my", "http://www.someCompany.com/segment" ) );

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			XmlElement contextElem = doc.CreateElement( "context" );
			XmlElement entity = doc.CreateElement( "entity" );

			doc.AppendChild( root );
			root.AppendChild( contextElem );
			contextElem.AppendChild( entity );

			WriteSegments(entity, root, doc, false, null);

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );

			Console.WriteLine( writer.ToString() );

			string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:my=""http://www.someCompany.com/segment"">
  <context>
    <entity>
      <segment>
        <my:stateProvince>MI</my:stateProvince>
      </segment>
    </entity>
  </context>
</root>";

			Assert.AreEqual( expectedXml, writer.ToString() );

		}

		/// <exclude/>
		[Test]
		public void Test_MultipleSegmentXml()
		{
			Segments.Add( new Segment( "name", "MI", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			Segments.Add( new Segment( "name", "CO", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			Segments.Add( new Segment( "name", "AL", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			Segments.Add( new Segment( "name", "AK", "stateProvince", "my", "http://www.someCompany.com/segment" ) );


			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			XmlElement contextElem = doc.CreateElement( "context" );
			XmlElement entity = doc.CreateElement( "entity" );

			doc.AppendChild( root );
			root.AppendChild( contextElem );
			contextElem.AppendChild( entity );

			WriteSegments(entity, root, doc, false, null);

			System.IO.StringWriter writer = new System.IO.StringWriter();

			doc.Save( writer );

			Console.WriteLine( writer.ToString() );

			string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:my=""http://www.someCompany.com/segment"">
  <context>
    <entity>
      <segment>
        <my:stateProvince>MI</my:stateProvince>
        <my:stateProvince>CO</my:stateProvince>
        <my:stateProvince>AL</my:stateProvince>
        <my:stateProvince>AK</my:stateProvince>
      </segment>
    </entity>
  </context>
</root>";

			Assert.AreEqual( expectedXml, writer.ToString() );

		}
		#endregion

		#region populate context from xmlNode
		/// <exclude/>
		[Test]
		public void Test_FromXml_CurrentAsOf()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_AsOf";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 7, 9 );
			tcp.PeriodType = Element.PeriodType.instant;

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement cpXml = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsTrue( ContextProperty.TryCreateFromXml( cpXml, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tcp.ValueEquals( cpBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_WithScenarioAndSegment()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			tcp.Segments.Add( new Segment( "name", "MI", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "CO", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "AL", "stateProvince", "my", "http://www.someCompany.com/segment" ) );
			tcp.Segments.Add( new Segment( "name", "AK", "stateProvince", "my", "http://www.someCompany.com/segment" ) );

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsTrue( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tcp.ValueEquals( cpBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_CreateCurrentForPeriod()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsTrue( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tcp.ValueEquals( cpBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_CreateCurrentForPeriod_WithScenario()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsTrue( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );

			Assert.IsTrue( tcp.ValueEquals( cpBack ), "objects don't match" );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_MissingEntityValue()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			//tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsFalse( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_MissingEntitySchema()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			//tcp.EntitySchema = "http://www.aucent.com/test";
			tcp.EntityValue = "Aucent Corporation";
			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append(doc, root, null);
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;

			ContextProperty cpBack = null;

			Assert.IsFalse( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );
		}

		/// <exclude/>
		[Test]
		public void Test_FromXml_MissingPeriod()
		{
			TestContextProperty tcp = new TestContextProperty();

			tcp.ContextID = "current_ForPeriod";
			tcp.EntitySchema = "http://www.aucent.com/test";
			//tcp.EntityValue = "Aucent Corporation";
//			tcp.PeriodStartDate = new DateTime( 2004, 1, 15 );
//			tcp.PeriodEndDate = new DateTime( 2004, 7, 31 );
			tcp.PeriodType = Element.PeriodType.duration;

			tcp.Scenarios.Add( new Scenario( "name", "MA", "residence", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name2", "true", "nonSmoker", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name3", "34",  "minAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );
			tcp.Scenarios.Add( new Scenario( "name4", "49", "maxAge", "fid", "http://www.someInsuranceCo.com/scenarios" ) );

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement elem = tcp.Append( doc, root,null );
			XmlNamespaceManager theManager = new XmlNamespaceManager( doc.NameTable );
			theManager.AddNamespace( "link2", "http://www.xbrl.org/2003/instance" );

			ArrayList errors = null;
			ContextProperty cpBack = null;

			Assert.IsFalse( ContextProperty.TryCreateFromXml( elem, theManager, out cpBack, ref errors  ), errors.Count > 0 ? ((ParserMessage)errors[0]).Message : string.Empty );
		}
		#endregion
	}
}
#endif
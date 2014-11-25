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
	using System.Text;
	using System.Diagnostics;
	using System.Collections;
	
	using NUnit.Framework;

	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	//[TestFixture] 
    public class TestPresentationLink : PresentationLink
    {
		/// <exclude/>
		public Hashtable Locators
		{
			get { return this.locators; }
		}

		/// <exclude/>
		public TestPresentationLink()
		{
		}

		/// <exclude/>
		public TestPresentationLink(string title, string role)
			: base(title, role, null)
		{
		}

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {
			Console.WriteLine( "***Start TestPresentationLink Comments***" );
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {
			Console.WriteLine( "***End TestPresentationLink Comments***" );
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

#if AUTOMATED
			const string GAAP_PRESENTATION_FILE = @"q:\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15-presentation.xml";
#else
			const string GAAP_PRESENTATION_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\us-gaap-ci-2004-06-15-presentation.xml";
#endif

			/// <exclude/>
			[Test]
			public void LoadLocators()
		{
			TestPresentationLink pl = new TestPresentationLink("Statement of Financial Position - CI","http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition");

			int errors = 0;

			TestPresentation p = new TestPresentation();
			
			Console.WriteLine( "Using File {0}", GAAP_PRESENTATION_FILE );

			p.Load( GAAP_PRESENTATION_FILE);

			XmlNodeList pLinksList = p.TheDocument.SelectNodes( Presentation.PLINK_KEY, p.TheManager );

			if ( pLinksList == null || pLinksList.Count == 0 )
			{
				Assert.Fail( "no nodes returned" );
			}

			XmlNode plNode = pLinksList.Item(0);
			pl.LoadChildren( plNode, p.TheManager, out errors );

			Assert.AreEqual( 0, errors );
			Assert.AreEqual( 290, pl.locators.Count );

			//total count of prohibited mappings
			int count = 0;
			foreach ( PresentationLocator ploc in pl.locators.Values )
			{
				TestPresentationLocator tpl = new TestPresentationLocator( ploc );
				count += tpl.NumProhibited();
			}


			Assert.AreEqual( 8, count );

		}
#if false
		/// <exclude/>
		[Test] public void VerifyXmlString()
		{
			TestPresentationLink pl = new TestPresentationLink("Statement of Financial Position - CI","http://www.xbrl.org/taxonomy/us/fr/gaap/role/StatementFinancialPosition");

			int errors = 0;

			TestPresentation p = new TestPresentation();
			
			Console.WriteLine( "Using File {0}", GAAP_PRESENTATION_FILE );

			p.Load( GAAP_PRESENTATION_FILE);

			XmlNodeList pLinksList = p.TheDocument.SelectNodes( Presentation.PLINK_KEY, p.TheManager );

			if ( pLinksList == null || pLinksList.Count == 0 )
			{
				Assert.Fail( "no nodes returned" );
			}

			XmlNode plNode = pLinksList.Item(0);
			pl.LoadChildren( plNode, p.TheManager, out errors );

			Assert.AreEqual( 0, errors );

			string xml = pl.ToXmlString();
			
			//Console.WriteLine( xml );
			
			XmlDocument doc = new XmlDocument();
			//XmlDocumentFragment frag = doc.CreateDocumentFragment();
			doc.LoadXml( xml );
		}
#endif
	}
}
#endif
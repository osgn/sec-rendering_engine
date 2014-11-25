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
	using System.Text;
	using System.Diagnostics;
	using System.Xml;
	using System.IO;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture] 
	public class TestDocumentBase : DocumentBase
	{
		/// <exclude/>
		protected override void ParseInternal(out int numErrors)
		{
			numErrors = 0;
			return;
		}

		/// <exclude/>
		public override string ToXmlString()
		{
			return string.Empty;
		}

		/// <exclude/>
		public override void ToXmlString(int numTabs, bool verbose, string lang, StringBuilder text)
		{
		}

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestDocumentBase Comments***" );
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Console.WriteLine( "***End TestDocumentBase Comments***" );
		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{
			this.errorList.Clear();
		}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{}

		#endregion

		string US_GAAP_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-07-06.xsd";
		string US_GAAP_FILE_2_0 = TestCommon.FolderRoot + @"US GAAP CI 2003-07-07" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2003-07-07.xsd";
		string SAG_FILE = TestCommon.FolderRoot + @"SoftwareAG-Spain-2004-01-30" +System.IO.Path.DirectorySeparatorChar +"SoftwareAG-2004-01-30.xml";
		string NON_SCHEMA_FILE = TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ins-2004-07-06-reference.xml";
		string TESTPRESHREFISURL_FILE = TestCommon.FolderRoot + @"TestPresHrefIsURL.xsd";

		/// <exclude/>
		[Test]
		public void LoadSchema()
		{
			TestDocumentBase s = new TestDocumentBase();
			int errors = 0;
			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			Assert.AreEqual( 0, errors );

			Console.WriteLine( "Verify GAAP namespace:" );

			Console.WriteLine("US_GAAP_FILE.ToLower(): " + US_GAAP_FILE.ToLower());
			Console.WriteLine("s.schemaFile.ToLower(): " + s.schemaFile.ToLower());
			
			Assert.AreEqual( US_GAAP_FILE.ToLower(), s.schemaFile.ToLower() );
			

			Console.WriteLine("TestSchemaPath: " +(TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06").ToLower());
			Console.WriteLine("s.schemaPath:   " + s.schemaPath.ToLower());
			Assert.AreEqual( TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06", s.schemaPath );

			Assert.AreEqual( "us-gaap-ci-2004-07-06.xsd", s.schemaFilename );

			Assert.AreEqual( "http://www.w3.org/2001/XMLSchema", s.theManager.DefaultNamespace );

			// verify namespaces
			foreach ( string pre in s.theManager )
			{
				string x = s.theManager.LookupNamespace( pre );

				switch ( pre )
				{
					case "xhtml":
						Assert.AreEqual( "http://www.w3.org/1999/xhtml", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xhtml " );
						break;

					case "link":
						Assert.AreEqual( "http://www.xbrl.org/2003/linkbase", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: link " );
						break;

					case "xbrli":
						Assert.AreEqual( "http://www.xbrl.org/2003/instance", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xbrli " );
						break;

					case "xlink":
						Assert.AreEqual( "http://www.w3.org/1999/xlink", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xlink " );
						break;
					case "xbrldt":
						Assert.AreEqual("http://xbrl.org/2005/xbrldt", s.theManager.LookupNamespace(pre));
						Console.WriteLine("	Found namespace: xbrldt ");
						break;


					case "us-gaap-ci":
						Assert.AreEqual( "http://www.xbrl.org/taxonomy/us/fr/gaap/ci/us-gaap-ci-2004-07-06", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: us-gaap-ci " );
						break;

					case "xsd":
						Assert.AreEqual( "http://www.w3.org/2001/XMLSchema", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xsd " );
						break;

					case "xml":		// ignore
					case "xmlns":	// ignore
					case "":		// ignore
						//Assert.AreEqual( "http://www.w3.org/2001/XMLSchema", s.theManager.LookupNamespace( pre ) );
						break;

					default:
						Console.WriteLine( "Unknown namespace: prefix: " + pre + " namespace: " + x );
						Assert.Fail(	"Unknown prefix in namespace table: " + pre + " namespace: " + x );
						break;
				}
			}
		}

		/// <exclude/>
		[Test]
		public void SAG_LoadSchema()
		{
			TestDocumentBase s = new TestDocumentBase();
			//ErrorInfo ei = null;

			int errors = 0;
			Assert.AreEqual( true, s.Load( SAG_FILE, out errors ), "Could not load SoftwareAG file" );
			Assert.AreEqual( 0, errors );

			Console.WriteLine( "Verify SAG Namespace:" );

			Assert.AreEqual( "http://www.xbrl.org/2003/instance", s.theManager.DefaultNamespace );

			// verify namespaces
			foreach ( string pre in s.theManager )
			{
				string x = s.theManager.LookupNamespace( pre );

				switch ( pre )
				{
					case "link":
						Assert.AreEqual( "http://www.xbrl.org/2003/linkbase", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: link " );
						break;
					case "xbrldt":
						Assert.AreEqual("http://xbrl.org/2005/xbrldt", s.theManager.LookupNamespace(pre));
						Console.WriteLine("	Found namespace: xbrldt ");
						break;

					case "xlink":
						Assert.AreEqual( "http://www.w3.org/1999/xlink", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xlink" );
						break;

					case "sag-es":
						Assert.AreEqual( "http://www.softwareag.com/es/xbrl/taxonomy/2004-01-30", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: sag-es" );
						break;

					case "ifrs-gp":
						Assert.AreEqual( "http://xbrl.iasb.org/int/fr/ifrs/gp/2004-01-15", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: ifrs-gp " );
						break;

					case "iso4217":
						Assert.AreEqual( "http://www.xbrl.org/2003/iso4217", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: iso4217 " );
						break;

					case "xsd":
						Assert.AreEqual( "http://www.w3.org/2001/XMLSchema", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xsd " );
						break;

					case "xsi":
						Assert.AreEqual( "http://www.w3.org/2001/XMLSchema-instance", s.theManager.LookupNamespace( pre ) );
						Console.WriteLine( "	Found namespace: xsi " );
						break;

					case "xml":		// ignore
					case "xmlns":	// ignore
					case "xbrli":	// ignore
					case "":		// ignore
						//Assert.AreEqual( "http://www.w3.org/2001/XMLSchema", s.theManager.LookupNamespace( pre ) );
						break;

					default:
						Console.WriteLine( "Unknown namespace: prefix: " + pre + " namespace: " + x );
						Assert.Fail( "	Unknown prefix in namespace table: " + pre + " namespace: " + x );
						break;
				}
			}
		}

		/// <exclude/>
		[Test]
		public void GAAP_GetLinkbaseRef_Presentation()
		{
			TestDocumentBase s = new TestDocumentBase();

			//ErrorInfo ei = null;

			int errors = 0;
			Assert.AreEqual( true, s.Load( US_GAAP_FILE, out errors ), "Could not load US GAAP File" );
			Assert.AreEqual( 0, errors );

			const string PRESENTATION_ROLE		= "/presentationLinkbaseRef";

			string[] presRef = s.GetLinkbaseReference( DocumentBase.TARGET_LINKBASE_URI + PRESENTATION_ROLE );

			Assert.AreEqual( TestCommon.FolderRoot + @"XBRL 2.1 Updated" +System.IO.Path.DirectorySeparatorChar +"2004-07-06" +System.IO.Path.DirectorySeparatorChar +"us-gaap-ci-2004-07-06-presentation.xml", presRef[0] );
		}

		/// <exclude/>
		[Test]
		public void TestPresHrefIsURL_GetLinkbaseRef()
		{
			TestDocumentBase s = new TestDocumentBase();

			int errors = 0;
			Assert.AreEqual( true, s.Load( TESTPRESHREFISURL_FILE, out errors ), "Could not load TestPresHrefIsURL File" );
			Assert.AreEqual( 0, errors );

			const string PRESENTATION_ROLE		= "/presentationLinkbaseRef";

			string[] presRef = s.GetLinkbaseReference( DocumentBase.TARGET_LINKBASE_URI + PRESENTATION_ROLE );

			// presRef should be the URL without the schema path
			Assert.AreEqual( @"http://xbrl.iasb.org/int/fr/ifrs/gp/2005-05-15/ifrs-gp-pre-bs-classified-2005-05-15.xml", presRef[0] );
		}

		/// <exclude/>
		[Test]
		public void NullFilename()
		{
			TestDocumentBase s = new TestDocumentBase();

			int errors = 0;

			s.Load( null, out errors );
			Assert.AreEqual( 1, errors );

			Console.WriteLine( "Null Filename msg: {0}", s.ErrorList[0] );
		}

		/// <exclude/>
		[Test]
		public void EmptyFilename()
		{
			TestDocumentBase s = new TestDocumentBase();

			int errors = 0;

			s.Load( "", out errors );
			Assert.AreEqual( 1, errors );

			Console.WriteLine( "Null Filename msg: {0}", s.ErrorList[0] );
		}

		/// <exclude/>
		[Test]
		public void TestValidateXBRLVersion()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(US_GAAP_FILE);

			this.schemaFile = US_GAAP_FILE;

			Console.WriteLine("Begin testing validation of VALID 2.1 XBRL Schema File");
			int errors = 0;
			bool ok = this.ValidateXBRLVersion(doc, out errors);
			Assert.IsTrue(ok, "Should have validated this as version 2.1");
			Assert.IsTrue(errorList.Count == 0);
			Assert.AreEqual(0, errors);
			Console.WriteLine("End testing validation of VALID 2.1 XBRL Schema File");

			doc = new XmlDocument();
			doc.Load(US_GAAP_FILE_2_0);

			this.schemaFile = US_GAAP_FILE_2_0;

			errors = 0;
			Console.WriteLine("Begin testing validation of INVALID 2.1 XBRL Schema File");
			ok = this.ValidateXBRLVersion(doc, out errors);
			Assert.IsFalse(ok, "Should be invalid version 2.0");
			Assert.IsTrue(errorList.Count > 0);
			Assert.IsTrue(errors > 0);
			Console.WriteLine("End testing validation of INVALID 2.1 XBRL Schema File");

			Console.WriteLine("Begin testing validation of a non-schema file");
			doc = new XmlDocument();
			doc.Load(NON_SCHEMA_FILE);
			this.schemaFile = NON_SCHEMA_FILE;

			errors = 0;
			ok = this.ValidateXBRLVersion(doc, out errors);
			Assert.IsFalse(ok, "Should be invalid file type");
			Assert.IsTrue(errorList.Count > 0);
			Assert.IsTrue(errors > 0);
			Console.WriteLine("End testing validation of a non-schema file");
		}


		/// <exclude/>
		[Test]
		public void TestValidateFileExistance()
		{
			// test a local file that should be found
			bool setProperties = true;
			bool FileIsLocal = false;
			string ResolvedLocalFilePath = string.Empty;
			DateTime lastModified = DateTime.MinValue;

			bool URLExists;
			Assert.IsTrue(base.ValidateFileExistance(US_GAAP_FILE, setProperties, 
				out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists ),
				"Did not find local file");
			Assert.IsTrue( FileIsLocal, "is local flag should be true");
			Assert.AreEqual( US_GAAP_FILE, ResolvedLocalFilePath, "wrong resolved path returned for local file");

			// test a http file that should be found
			const string HTTP_GAAP_CI_FILE = @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd";
			Assert.IsTrue(base.ValidateFileExistance(HTTP_GAAP_CI_FILE, setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"1 Did not find HTTP file");
			Assert.IsFalse( FileIsLocal, "is local flag should be FALSE");
			Assert.AreEqual( HTTP_GAAP_CI_FILE, ResolvedLocalFilePath, "wrong resolved path returned for local file");
			Assert.AreEqual( System.IO.Path.GetFileName(HTTP_GAAP_CI_FILE), base.schemaFilename, "schema file name not set correctly");
			Assert.AreEqual( HTTP_GAAP_CI_FILE.Replace(base.schemaFilename, string.Empty), base.schemaPath, "schema path not set correctly");

			// test a local file that's not found
			const string LOCAL_FILE_NOT_FOUND = @"ThisFileShouldNotBeFound";
			Assert.IsFalse(base.ValidateFileExistance(LOCAL_FILE_NOT_FOUND, setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Should not have found the local file");

			// test a http file that's not found
			const string HTTP_FILE_NOT_FOUND = @"http://ThisFileShouldNotBeFound";
			Assert.IsFalse(base.ValidateFileExistance(HTTP_FILE_NOT_FOUND, setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Should not have found the http file");

			// test local file looking in additional folders and is found
			string[] extraLocalFolders = new string[] {TestCommon.FolderRoot, Path.GetDirectoryName(US_GAAP_FILE)};
            this.AdditionalDirectoriesForLoad = extraLocalFolders;
			Assert.IsTrue(base.ValidateFileExistance(Path.GetFileName(US_GAAP_FILE), setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Did not find local file search extra folders");
			Assert.IsTrue( FileIsLocal, "search extra folders is local flag should be true");
			Assert.AreEqual( US_GAAP_FILE, ResolvedLocalFilePath, "search extra folders wrong resolved path returned for local file");

			// test http file looking in additional folders and is found
			extraLocalFolders = new string[] { @"http://www.xbrl.org/us/fr", @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28" };
            this.AdditionalDirectoriesForLoad = extraLocalFolders;

			Assert.IsTrue(base.ValidateFileExistance(Path.GetFileName(HTTP_GAAP_CI_FILE), setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Additional folders Did not find HTTP file");

			Assert.IsFalse( FileIsLocal, "is local flag should be FALSE");
			Assert.AreEqual( HTTP_GAAP_CI_FILE, ResolvedLocalFilePath, "wrong resolved path returned for local file");
			Assert.AreEqual( System.IO.Path.GetFileName(HTTP_GAAP_CI_FILE), base.schemaFilename, "schema file name not set correctly");
			Assert.AreEqual( HTTP_GAAP_CI_FILE.Replace(base.schemaFilename, string.Empty), base.schemaPath, "schema path not set correctly");

			// test local file looking in additional folders and is not found
			extraLocalFolders = new string[] {TestCommon.FolderRoot};
            this.AdditionalDirectoriesForLoad = extraLocalFolders;
			Assert.IsFalse(base.ValidateFileExistance(Path.GetFileName(US_GAAP_FILE), setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Should not have found the local file");


			// test http file looking in additional folders and is not found
			extraLocalFolders = new string[] { @"http://www.xbrl.org/us/fr" };
            this.AdditionalDirectoriesForLoad = extraLocalFolders;
			Assert.IsFalse(base.ValidateFileExistance(Path.GetFileName(HTTP_GAAP_CI_FILE), setProperties, out FileIsLocal, out ResolvedLocalFilePath, out lastModified, out URLExists),
				"Should not have found the http file");


		}
	}
}
#endif
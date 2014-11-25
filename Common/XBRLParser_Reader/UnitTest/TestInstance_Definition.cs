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
namespace Aucent.MAX.AXE.XBRLParser.TestInstance
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Text;

	using System.Collections;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestInstance_Definition : Instance
	{
		#region init
		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Clear();
			Taxonomy.SkipDefinitionFileLoading = false;

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;

		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Trace.Listeners.Clear();

		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{
			contexts.Clear();
			units.Clear();
			markups.Clear();
            DocumentTupleList = null;
			attributes.Clear();
			schemaRefs.Clear();

            DocumentTupleList = null;

			xDoc = null;
			theManager = null;

			mergeDocs = false;
			OverrideExistingDocument = false;

			fnProperties.Clear();
			footnoteLink = null;
		}
		#endregion

		string COMPANIES_HOUSE_FILE = TestCommon.FolderRoot + @"uk_CompaniesHouse" +System.IO.Path.DirectorySeparatorChar +"uk-gaap-ae-2005-06-01.xsd";
		string DAMC_INST_FILE = TestCommon.FolderRoot + @"uk_CompaniesHouse" +System.IO.Path.DirectorySeparatorChar +"dormant-accounts-multi-currency.xml";
		string DAMC_INST_FILE_Missing_Item = TestCommon.FolderRoot + @"uk_CompaniesHouse" +System.IO.Path.DirectorySeparatorChar +"dormant-accounts-multi-currency_missing_Item.xml";
		string DAM_INST_FILE = TestCommon.FolderRoot + @"uk_CompaniesHouse" +System.IO.Path.DirectorySeparatorChar +"dormant-accounts-minimal.xml";
		string DAM_INST_FILE_Missing_Item = TestCommon.FolderRoot + @"uk_CompaniesHouse" +System.IO.Path.DirectorySeparatorChar +"dormant-accounts-minimal_Missing_Item.xml";
		
		#region Requires element tests

		/// <exclude/>
		[Test]
		public void TestUkCompanies_Instances()
		{
			Taxonomy tax = new Taxonomy();
			int errors = 0;

			DateTime start = DateTime.Now;
			if ( tax.Load( COMPANIES_HOUSE_FILE, out errors ) != true )
			{
				Assert.Fail( (string)tax.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			tax.CurrentLabelRole ="preferredLabel";
			tax.CurrentLanguage = "en";

			tax.Parse( out errors );
			Assert.AreEqual( 0, errors, "should not have any errors");

			
			Hashtable prefixXRef = new Hashtable();
			prefixXRef["ae"] = "uk-gaap-ae";
			prefixXRef["pt"] = "uk-gaap-pt";
			prefixXRef["gc"] = "uk-gcd";

			ValidateInstanceDoc( tax, DAMC_INST_FILE, 0,prefixXRef );
			ValidateInstanceDoc( tax, DAMC_INST_FILE_Missing_Item, 2, prefixXRef);
			ValidateInstanceDoc( tax, DAM_INST_FILE, 0,prefixXRef );
			ValidateInstanceDoc( tax, DAM_INST_FILE_Missing_Item, 1,prefixXRef );

			

		}


		private void ValidateInstanceDoc( Taxonomy tax,
			string fileName, int countErrors, Hashtable prefixXRef )
		{
			Instance ins = new Instance();
			ArrayList errs;
			if( !ins.TryLoadInstanceDoc( fileName, out errs ))
			{
				Assert.Fail( "Failed to load instance document" + fileName);
			}
			foreach( MarkupProperty mp in ins.markups )
			{
				if ( prefixXRef[mp.elementPrefix] != null )
				{
					string realPrefix = prefixXRef[mp.elementPrefix] as string;
					mp.elementPrefix = realPrefix;
					mp.elementId = string.Format(DocumentBase.ID_FORMAT, mp.elementPrefix, mp.elementName);

				}

			}
			string[] validationErrors;
			tax.ValidateInstanceInformationForRequiresElementCheck( ins, out validationErrors );

			Assert.IsNotNull( validationErrors , "Validation errors object should not be null");
			foreach( string str in validationErrors )
			{
				Console.WriteLine( str );
			}
			Assert.AreEqual( countErrors, validationErrors.Length,"Failed to ValidateInstanceInformationForRequiresElementCheck");

		}

		#endregion
	
	}
}
#endif
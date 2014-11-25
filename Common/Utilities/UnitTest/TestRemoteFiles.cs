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
namespace Aucent.MAX.AXE.Common.Utilities.Test
{
	using System;
	using System.Collections.Generic;
	using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Utilities;

    [TestFixture] 
    public class TestRemoteFiles : RemoteFiles
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
        {}

        ///<exclude/>
        [TearDown] public void RunAfterEachTest() 
        {}

#endregion
		[Test]
		public void Test_ValidateURLFile()
		{
			RemoteFiles RF = new RemoteFiles();
			DateTime lastMod = DateTime.MinValue;

			// Test existing file
			Assert.IsTrue(RF.CheckURLFileExists("http://www.powersdk.com/sample/test.xml", out lastMod ), "existing file not found");
			// Test non-existing file
			Assert.IsFalse(RF.CheckURLFileExists("http://www.FakeWWWAddress.com/FakeFileNameWhichDoesNotExist.xml", out lastMod), "non-existing file found");
			// Text non-exising, but redirected file
			Assert.IsFalse(RF.CheckURLFileExists("http://www.cnn.com/FakeFileNameWhichDoesNotExist.xml", out lastMod), "non-existing, redirected file found");

			// Test Intranet URL
			// You have to log on to auteam to access our intranet, so this test always failed
//			Assert.IsTrue(RF.CheckURLFileExists("http://auteam/sites/product/Documents/XBRL/us-gaap-ci-2004-06-15.xsd", out lastMod), "intranet url not found");
			
			// Test a non-existing & invalid path local file
			Assert.IsFalse(RF.CheckURLFileExists("c:\\Test.xml", out lastMod),"invalid path double slash found");
			Assert.IsFalse( RF.CheckURLFileExists( "c:Test.xml", out lastMod ), "invalid path no slash found" );

			// ICI-RR comes back as text/plain make sure the utility returns true on its Exists check
			Assert.IsTrue(RF.CheckURLFileExists("http://xbrl.ici.org/rr/2006/ici-rr.xsd", out lastMod), "ICI-RR file was not found");

			// Test content type of text/plain that does not parse as valid xml
			Assert.IsFalse(RF.CheckURLFileExists("http://dev1/DTTestSchemas/InvalidSchema.txt", out lastMod), "Non Xml Text File returned true");
		}


		[Test]
		public void TestIsValidURL()
		{
			string str = @"http://xbrl.us/dis/con/2008-03-31/us-gaap-dis-con-cal-2008-03-31.xml";
			string tmp;
			Assert.IsFalse(RemoteFiles.IsValidURL(str, out tmp));


			str = @"http://xbrl.us/us-gaap/1.0beta2/ind/ci/us-gaap-ci-stm-2008-01-31.xsd";
			Assert.IsTrue(RemoteFiles.IsValidURL(str, out tmp));


		}


		[Test]
		public void TestGetBaseTaxonomyURLMapping()
		{
			Dictionary<string, string> ret = RemoteFiles.GetBaseTaxonomyURLMapping();

			Assert.IsTrue(ret.Count > 100);
		}


		[Test]
		public void TestLoadRemoteFileInformation()
		{
			DateTime start = DateTime.Now;

			RemoteFiles.RemoteFileInformation.LoadRemoteFileInformation();

			DateTime end = DateTime.Now;

			Console.WriteLine("Time taken = {0}", end - start);
		}
	
	}
}
#endif
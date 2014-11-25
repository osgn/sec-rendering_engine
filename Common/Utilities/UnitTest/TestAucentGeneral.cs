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
    using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Utilities;
    using System.Collections.Specialized;

    [TestFixture] 
    public class TestAucentGeneral : AucentGeneral
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
//		[Test]
//		public void Test_AucentApplicationDataPath()
//		{
//			Assert.IsNotNull(AucentGeneral.AucentApplicationDataPath);
//		}

//		public void Test_AucentApplicationDataXBRLAddinPath()
//		{
//			Assert.IsNotNull(AucentGeneral.AucentApplicationDataXBRLAddinPath);
//		}

        [Test]
        public void Test_LanguageCode()
        {
            string desc;
            Assert.IsTrue(
            ISOUtility.TryGetLanguageDescription("en", out desc), "should get desc");
            Console.WriteLine(desc);
            Assert.IsTrue(
  ISOUtility.TryGetLanguageDescription("EN", out desc), "should get desc");
            Console.WriteLine(desc);
            Assert.IsTrue(
            ISOUtility.TryGetLanguageDescription("en-us", out desc), "should get desc");
            Console.WriteLine(desc);
            Assert.IsTrue(
            ISOUtility.TryGetLanguageDescription("en-US", out desc), "should get desc");
            Console.WriteLine(desc);

            Assert.IsFalse(
ISOUtility.TryGetLanguageDescription("dfaa", out desc), "should get desc");
            Console.WriteLine(desc);

            StringCollection ret = ISOUtility.GetLanguages();

            foreach (string str in ret)
            {
                Console.WriteLine(str);

            }
            
        }
		[Test]
		public void Test_AppendFileNameToSchemaPath()
		{
			string ret = AucentGeneral.AppendFileNameToSchemaPath("http://taxonomies.xbrl.us/us-gaap/2009/Ind/CI/",
				"../../elts/us-gaap-dep-pre-2009-01-31.xml");
			
			// URL: HTTP
			Assert.IsTrue (("http://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("http://www.aucent.com/XBRL/", "us-gapp-ci.xsd")));
			
			Assert.IsTrue (("http://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("http://www.aucent.com/XBRL", "us-gapp-ci.xsd")));
			

			Assert.IsTrue (("http://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("http://www.aucent.com/XBRL//////", "us-gapp-ci.xsd")));

			Assert.IsTrue (("http://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("http://www.aucent.com/XBRL//////\\\\", "us-gapp-ci.xsd")));

			Assert.IsTrue (("http://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("http://www.aucent.com/XBRL\\\\", "us-gapp-ci.xsd")));

			// URL: HTTPS
			Assert.IsTrue (("HTTPS://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("HTTPS://www.aucent.com/XBRL/", "us-gapp-ci.xsd")));
			
			Assert.IsTrue (("HTTPS://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("HTTPS://www.aucent.com/XBRL", "us-gapp-ci.xsd")));
			

			Assert.IsTrue (("HTTPS://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("HTTPS://www.aucent.com/XBRL//////", "us-gapp-ci.xsd")));

			Assert.IsTrue (("HTTPS://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("HTTPS://www.aucent.com/XBRL//////\\\\", "us-gapp-ci.xsd")));

			Assert.IsTrue (("HTTPS://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("HTTPS://www.aucent.com/XBRL\\\\", "us-gapp-ci.xsd")));


			// URI: file
			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL","Test.xsd")));

			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL\\\\\\\","Test.xsd")));

			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL\\\\","Test.xsd")));

			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL\\\\//////","Test.xsd")));
			
			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL//////","Test.xsd")));

			Assert.IsTrue((@"file://C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"file://C:\Myfiles\XBRL////\\\\","Test.xsd")));

			// URI: FTP
			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL/", "us-gapp-ci.xsd")));
			
			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL", "us-gapp-ci.xsd")));
			
			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL", "us-gapp-ci.xsd")));

			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL//////", "us-gapp-ci.xsd")));

			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL//////\\\\", "us-gapp-ci.xsd")));

			Assert.IsTrue (("ftp://www.aucent.com/XBRL/us-gapp-ci.xsd") == 
				(AucentGeneral.AppendFileNameToSchemaPath("ftp://www.aucent.com/XBRL\\\\", "us-gapp-ci.xsd")));

			// File System
			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL","Test.xsd")));

			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL\\\\\\\","Test.xsd")));

			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL\\\\","Test.xsd")));

			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL\\\\//////","Test.xsd")));
			
			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL//////","Test.xsd")));

			Assert.IsTrue((@"C:\Myfiles\XBRL\Test.xsd") ==
				(AucentGeneral.AppendFileNameToSchemaPath(@"C:\Myfiles\XBRL////\\\\","Test.xsd")));
		}
    }
}
#endif
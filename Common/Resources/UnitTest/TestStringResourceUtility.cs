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
namespace Aucent.MAX.AXE.Common.Resources.Test
{
	using System;
	using System.Globalization;

    using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Resources;

    [TestFixture] 
    public class TestStringResourceUtility
    {
		static string errorMessage = "Invalid string resource look-up";
		static string unitTestString = "Unit Test String";
		static string unknownErrorString = "Unknown String Resource";
		
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

		/// <summary>
		/// Positive test for GetString() method
		/// </summary>
		[Test]public void TestGetString() 
		{
			Assert.AreEqual(unitTestString, StringResourceUtility.GetString("UnitTest"), errorMessage);
		}

		/// <summary>
		/// Manually set UI culture to German and call GetString()
		/// </summary>
		[Test]
		[Ignore("To test, German resources assembly must be updated.")]
		public void TestGermanGetString() 
		{
			CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

			Assert.AreEqual("German Unit Test String", StringResourceUtility.GetString("UnitTest"), errorMessage);

			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
		}

		/// <summary>
		/// Call GetString() with an invalid key
		/// </summary>
		[Test]public void TestGetInvalidString()
		{
			string garbage = "alfkjdas;lkj";
			Assert.AreEqual(unknownErrorString + ", Key: " + garbage, 
				StringResourceUtility.GetString(garbage), errorMessage);
		}

		/// <summary>
		/// Call GetString() and pass null key parameter
		/// </summary>
		[Test]public void TestGetStringNullKey()
		{
			Assert.AreEqual(unknownErrorString, StringResourceUtility.GetString(null), errorMessage);
		}

		/// <summary>
		/// Call GetString() and pass empty-string key parameter
		/// </summary>
		[Test]public void TestGetStringEmptyKey()
		{
			Assert.AreEqual(unknownErrorString, StringResourceUtility.GetString(""), errorMessage);
		}


		[Test]
		public void TestMessage()
		{
			string str = StringResourceUtility.GetString("XBRLAddin.ETWizard.Error.SECTitleFormatMessage");
			string msg = string.Format(str,"test - tersd -sdsdereff", "{number} - {type} - {text}");


			Console.WriteLine(msg);
		}
    }
}
#endif
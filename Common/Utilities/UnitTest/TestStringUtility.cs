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
	using System.Collections;
	using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Utilities;

	/// <exclude/>
	[TestFixture] 
    public class TestStringUtility
    {
#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
        {}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
        {}

#endregion
		/// <exclude/>
		[Test]
		public void TestInvalidCharsInitInConstructor()
		{
			Assert.IsNotNull( StringUtility.InvalidElementNameCharacters, "invalid chars array was not initialized" );
		}

		/// <exclude/>
		[Test]
		public void TestContainsCharacters()
		{
			foreach ( char str in StringUtility.InvalidString.ToCharArray() )
			{
				string target = new string(str,1);
				Assert.IsTrue( StringUtility.ContainsCharacters( target, StringUtility.InvalidString, true ), "Inalid string tests failed" );
			}

			foreach (char str in StringUtility.InvalidSchemeString.ToCharArray())
			{
				string target = new string(str,1);
				Assert.IsTrue(StringUtility.ContainsCharacters(target, StringUtility.InvalidSchemeString, true), "Invalid scheme string tests failed");
			}

			Assert.IsTrue( StringUtility.ContainsCharacters( new string( '"', 1 ), StringUtility.InvalidString, true ), "Test quote invalid string should have returned false" );
			Assert.IsTrue(StringUtility.ContainsCharacters(new string('"',1), StringUtility.InvalidSchemeString, true), "Test quote invalid scheme string  should have returned false");

			Assert.IsFalse( StringUtility.ContainsCharacters( new string( '"', 1 ), StringUtility.InvalidString, false ), "Test quote invalid string should have returned true" );
			Assert.IsFalse(StringUtility.ContainsCharacters(new string('"',1), StringUtility.InvalidSchemeString, false), "Test quote invalid scheme string should have returned true");
		
			Assert.IsFalse(StringUtility.ContainsCharacters(".", StringUtility.InvalidSchemeString, false), "Test period against scheme string should have returned true");
		}

		/// <exclude/>
		[Test]
		public void TestIsDigitsOnly()
		{
			const string error = "Invalid return value from IsDigitsOnly method";

			Assert.IsFalse(StringUtility.IsDigitsOnly(null), error);
			Assert.IsFalse(StringUtility.IsDigitsOnly(string.Empty), error);
			Assert.IsFalse(StringUtility.IsDigitsOnly("12lkj"), error);
			Assert.IsFalse(StringUtility.IsDigitsOnly("lkj21"), error);
			Assert.IsTrue(StringUtility.IsDigitsOnly("1"), error);
		}

		/// <exclude/>
		[Test]
		public void TestFormatStringForErrorMessage()
		{
			string test = "abcdefg";
			string expected = "a b c d e f g";

			string actual = StringUtility.FormatStringForErrorMessage(test);
			Assert.AreEqual(expected, actual, "Did not format the string correctly");
		}

		/// <exclude/>
		[Test]
		public void TestRemoveXBRLUrl()
		{
			string[] testStrings =
			{
				"http://www.xbrl.org/2003/role/reference",
				"testhttp://www.xbrl.org/2003/role/referencetest",
				"test\r\nhttp://www.xbrl.org/2003/role/reference\r\ntest",
				"Reference:\r\nhttp://www.xbrl.org/2003/role/reference\r\n -Name Regulation S-X\r\n -Number Rule 3\r\n -Chapter 4\r\n -Publisher SEC\r\n -URI http://www.sec.gov/divisions/corpfin/forms/regsx.htm#reg\r\n -URIDate 38299\r\n",
			};

			string[] expectedStrings =
			{
				"",
				"test",
				"test\r\n\r\ntest",
				"Reference:\r\n\r\n -Name Regulation S-X\r\n -Number Rule 3\r\n -Chapter 4\r\n -Publisher SEC\r\n -URI http://www.sec.gov/divisions/corpfin/forms/regsx.htm#reg\r\n -URIDate 38299\r\n",
			};

			string resultString;

			for (int i = 0; i < testStrings.Length; i++)
			{
				resultString = string.Empty;
				resultString = StringUtility.RemoveXBRLUrl(testStrings[i]);

				Console.WriteLine(String.Format("Checking '{0}' against '{1}'", testStrings[i], expectedStrings[i]));
				Assert.AreEqual(expectedStrings[i], resultString, "Did not format the string correctly");
			}
		}

        [Test]
        public void TestSpecialSpaceCharacter()
        {
            string testStr = "D istributionToStockholdersSpinOffDiscoveryHoldingCompanydhcnote 6";

            string newStr = testStr.Replace(" ", string.Empty);
            Console.WriteLine(newStr);

            for (int i = 0; i < testStr.Length; i++)
            {
                
                int tmp = (int)testStr[i];
                Console.WriteLine("{0}:{1}", testStr[i], tmp );

                Console.WriteLine(Char.GetUnicodeCategory(testStr[i]));
            }
        }

		[Test]
		public void TestGetDefaultElementName()
		{
			Console.WriteLine( StringUtility.GetDefaultElementName( "Hello this is a [Member]"));
		}

        [Test]
        public void GetLC3LabelFromElementName()
        {
            string name = "S0002345Member";
            string label = StringUtility.GetLC3LabelFromElementName(name);
            Console.WriteLine( label);

            Assert.AreEqual(name, StringUtility.GetDefaultElementName(label));
        }


		[Test]
		public void Testsyy20081227xsd()
		{
			string prefix;
			string verDate;
			Assert.IsTrue(StringUtility.IsSECFileNameValid("syy-20081227.xsd", out prefix, out verDate),
				"Failed to validate sec filename");

			Assert.IsTrue(StringUtility.IsSECFileNameValid("syy-20081227", out prefix, out verDate),
				"Failed to validate sec filename");

			Assert.IsFalse(StringUtility.IsSECFileNameValid("syy-2008-12-27", out prefix, out verDate),
				"Failed to validate sec filename");

			Assert.IsFalse(StringUtility.IsSECFileNameValid("syy-2008-12-27.xsd", out prefix, out verDate),
	"Failed to validate sec filename");


			Assert.IsFalse(StringUtility.IsSECFileNameValid("test", out prefix, out verDate),
	"Failed to validate sec filename");
		}


		[Test]
		public void TestConvertValidUTF8Chars()
		{
			string source = @"Text Block with Symbols Euro = € and Yen = ¥ and copy right and tm symbol  = ®®™ test this";
			Console.WriteLine(source);
			Console.WriteLine(TextUtilities.CovertValidUTF8ToAsciiInTextBlock(source));

		}

		[Test]
		public void TestCovertValidUTF8ToAsciiInLabel1()
		{
			string source = @"Class A units – 14,231,867 and 14,627,005 units outstan";
			Console.WriteLine(source);
			Console.WriteLine(TextUtilities.CovertValidUTF8ToAsciiInLabel(source));

		}

		[Test]
		public void TestCovertValidUTF8ToAsciiInLabel2()
		{
			string source = @"Total Vornado shareholders’ equity";
			Console.WriteLine(source);
			Console.WriteLine(TextUtilities.CovertValidUTF8ToAsciiInLabel(source));

		}

		[Test]
		public void TestCovertValidUTF8ToAsciiInLabel3()
		{
			string source = @"Income applicable to Alexander’s";
			Console.WriteLine(source);
			Console.WriteLine(TextUtilities.CovertValidUTF8ToAsciiInLabel(source));

		}

		[Test]
		public void TestCovertValidUTF8ToAsciiInLabel4()
		{
			string source = @"(Loss) income applicable to Toys “R” Us";
			Console.WriteLine(source);
			Console.WriteLine(TextUtilities.CovertValidUTF8ToAsciiInLabel(source));

		}


    }
}
#endif
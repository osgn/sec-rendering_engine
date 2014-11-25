#if UNITTEST


using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using System.IO;

namespace Aucent.FilingServices.Data.UnitTests
{
	[TestFixture]
	public class TestFilingInfo
	{
		string testRoot = @"S:\TestFiles";

		#region init

		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp]
		public void RunFirst()
		{ }

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
		[TestFixtureTearDown]
		public void RunLast()
		{ }

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp]
		public void RunBeforeEachTest()
		{ }

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown]
		public void RunAfterEachTest()
		{ }

		#endregion

		[Test]
		public void TestGetInstanceDocPath()
		{
			string[][] testFilings = new string[10][];
			testFilings[0] = new string[] { "0000008670-08-000046", "adp-20080331.xml" };
			testFilings[1] = new string[] { "0000040545-08-000037", "ge-20080630.xml" };
			testFilings[2] = new string[] { "0000796343-08-000005", "adbe-20080616.xml" };
			testFilings[3] = new string[] { "0000893220-08-002188", "dd-20080630.xml" };
			testFilings[4] = new string[] { "0000950133-08-003012", "nihd-20080331.xml" };
			testFilings[5] = new string[] { "0000950135-08-002601", "gis-20080224.xml" };
			testFilings[6] = new string[] { "0000950135-08-003083", "dd-20080331.xml" };
			testFilings[7] = new string[] { "0000950135-08-004978", "trow-20071231.xml" };
			testFilings[8] = new string[] { "0000950137-08-010830", "gis-20080525.xml" };
			testFilings[9] = new string[] { "0000950137-08-012088", "gis-20080824.xml" };

			List<string> errors = new List<string>();
			foreach (string[] filing in testFilings)
			{
				FilingInfo fi = new FilingInfo();
				fi.AccessionNumber = filing[0];
				fi.ParentFolder = testRoot;
				string instanceDocPath = fi.GetInstanceDocPath();

				string expectedPath = string.Format("{0}{1}{2}{1}{3}", testRoot, System.IO.Path.DirectorySeparatorChar, filing[0], filing[1]);

				if (!expectedPath.Equals(instanceDocPath, StringComparison.CurrentCultureIgnoreCase))
				{
					errors.Add("Paths do not match.  Expected: '" + expectedPath + "'. Actual: '" + instanceDocPath + "'");
				}
			}

			if (errors.Count > 0)
			{
				string strErrors = string.Join(Environment.NewLine + "\t", errors.ToArray());
				Assert.Fail("The following errors were found: " + Environment.NewLine + "\t" + strErrors);
			}
		}

		[Test]
		public void TestGetTaxonomyPath()
		{
			string[][] testFilings = new string[10][];
			testFilings[0] = new string[] { "0000008670-08-000046", "adp-20080331.xsd" };
			testFilings[1] = new string[] { "0000040545-08-000037", "ge-20080630.xsd" };
			testFilings[2] = new string[] { "0000796343-08-000005", "adbe-20080530.xsd" };
			testFilings[3] = new string[] { "0000893220-08-002188", "dd-20080630.xsd" };
			testFilings[4] = new string[] { "0000950133-08-003012", "nihd-20080331.xsd" };
			testFilings[5] = new string[] { "0000950135-08-002601", "gis-20080224.xsd" };
			testFilings[6] = new string[] { "0000950135-08-003083", "dd-20080331.xsd" };
			testFilings[7] = new string[] { "0000950135-08-004978", "trow-20071231.xsd" };
			testFilings[8] = new string[] { "0000950137-08-010830", "gis-20080525.xsd" };
			testFilings[9] = new string[] { "0000950137-08-012088", "gis-20080824.xsd" };

			List<string> errors = new List<string>();
			foreach (string[] filing in testFilings)
			{
				FilingInfo fi = new FilingInfo();
				fi.AccessionNumber = filing[0];
				fi.ParentFolder = testRoot;
				string taxonomyPath = fi.GetTaxonomyPath();

				string expectedPath = string.Format("{0}{1}{2}{1}{3}", testRoot, System.IO.Path.DirectorySeparatorChar, filing[0], filing[1]);

				if (!expectedPath.Equals(taxonomyPath, StringComparison.CurrentCultureIgnoreCase))
				{
					errors.Add("Paths do not match.  Expected: '" + expectedPath + "'. Actual: '" + taxonomyPath + "'");
				}
			}

			if (errors.Count > 0)
			{
				string strErrors = string.Join(Environment.NewLine + "\t", errors.ToArray());
				Assert.Fail("The following errors were found: " + Environment.NewLine + strErrors);
			}
		}
	}
}
#endif
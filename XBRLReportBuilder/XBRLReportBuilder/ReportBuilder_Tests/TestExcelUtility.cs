
//=============================================================================
// TestExcelUtility (unit test class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// Test generating Excel workbooks.
//=============================================================================
#if UNITTEST
namespace XBRLReportBuilder.Test
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using NUnit.Framework;

    [TestFixture] 
    public class TestExcelUtility
    {
        string baseDir = @"T:/TestFiles";

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

        #region tests

        [Ignore]
        public void TestBuildExcel_All_Folders()
        {
            DirectoryInfo testDirInfo = new DirectoryInfo(baseDir);

            foreach (DirectoryInfo filingDirectoryInfo in testDirInfo.GetDirectories())
            {

                if (filingDirectoryInfo.Name == "Test Rules") continue;

                string folderPath = string.Format("{0}{1}{2}", 
                    filingDirectoryInfo.FullName, Path.DirectorySeparatorChar, "Reports");

                Console.WriteLine ("Creating: " + folderPath + "/Financial_Report.xls");

                Assert.IsTrue(ExcelUtility.GenerateFinancialExcelWorkbook(folderPath, "Financial_Report"));
            }
        }

        #endregion
    }
}

#endif
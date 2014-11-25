/*****************************************************************************
 * TestReportBuilderProcessor (class)
 * Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
 * This class is a small test class that test the integration between the ReportBuilderService
 * and the ReportBuilderProcessor.
*****************************************************************************/

#if UNITTEST

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Aucent.FilingServices.Data;
using Aucent.XBRLReportBuilder.BuilderService;
using XBRLReportBuilder;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Aucent.FilingServices.FilingProcessorBase;

namespace Aucent.XBRLReportBuilder.BuilderService.UnitTests
{
	[TestFixture]
	public class TestReportBuilderProcessor: ReportBuilderProcessor
	{

		string testRoot = @"T:\TestFiles";
		string reportsFolder = @"T:\TestFiles\ServiceReports";

		#region init

		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Assert.IsTrue(FilingProcessorManager.TheMgr.TryLoadConfigurationSettings(this), "Failed to initialize the Processor Manager");

			if (!Directory.Exists(reportsFolder))
			{
				Directory.CreateDirectory(reportsFolder);
			}
		}

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
		public void TestBuild_mo_20080630()
		{
			string accensionNumber = "0001157523-08-007687";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_gis_20080824()
		{
			string accensionNumber = "0000950137-08-012088";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_nbl_20080630()
		{
			string accensionNumber = "0001140361-08-021862";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_adsk_20080731()
		{
			string accensionNumber = "0001193125-08-200578";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_ibm_20080429()
		{
			string accensionNumber = "0001104659-08-059468";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_mmm_20080630()
		{
			string accensionNumber = "0001104659-08-058923";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_mmm_20080331()
		{
			string accensionNumber = "0001104659-08-058647";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_br_20080630()
		{
			string accensionNumber = "0001193125-08-194588";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_fmcc_20080630()
		{
			string accensionNumber = "0001157523-08-007350";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_fmc_20080630()
		{
			string accensionNumber = "0001157523-08-007340";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}

		[Test]
		public void TestBuild_Alfalfa()
		{
			string accensionNumber = "_RR_ALFALFA_20090501";
			string filingType = "8-K";

			string folderPath = string.Format("{0}{1}{2}", testRoot, Path.DirectorySeparatorChar, accensionNumber);
			Assert.IsTrue(BuildReports(folderPath, filingType), "Failed to generate reports");

			ValidateReports(accensionNumber);
		}


		[Test]
		public void TestBadZip()
		{
			string filingDir =  FilingProcessorManager.TheMgr.FilingsFolderInfo.FullName;
			string badZip = string.Format("{0}{1}Bad.zip", testRoot, Path.DirectorySeparatorChar);
			string filingsZip = string.Format("{0}{1}Bad.zip", filingDir, Path.DirectorySeparatorChar);
			string errorFile = string.Format("{0}{1}Bad_errorLog.txt", FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, Path.DirectorySeparatorChar);

			//Make sure we are testing with a clean filings dir
			string[] files = Directory.GetFiles(filingDir);
			foreach (string file in files)
			{
				FileUtilities.DeleteFile(new FileInfo(file), true);
			}

			//Make sure there is not a previous error file in the reports folder
			if (File.Exists(errorFile))
			{
				FileUtilities.DeleteFile(new FileInfo(errorFile), true);
			}

			//Make sure we have the correct zip file copied to the filings dir
			if (File.Exists(filingsZip))
			{
				FileUtilities.DeleteFile(new FileInfo(filingsZip), true);
			}
			File.Copy(badZip, filingsZip);

			List<FilingInfo> filings = null;
			for (int i = 0; i <= maxUnzipAttempts; i++)
			{
				Assert.IsTrue(TryMoveFilingsToProcessingFolder(out filings), "TryMoveFilingsToProcessingFolder returned false");
			}
			Assert.AreEqual(0, filings.Count, "Invalid number of filings returned by TryMoveFilingsToProcessingFolder");
			Assert.IsTrue(File.Exists(errorFile), "Error file was not generated by TryMoveFilingsToProcessingFolder when called with an invalid zip file");

		}

		[Test]
		public void TestZipNoInstanceDoc()
		{
			string filingDir = FilingProcessorManager.TheMgr.FilingsFolderInfo.FullName;
			string sourceZip = string.Format("{0}{1}NoInstance.zip", testRoot, Path.DirectorySeparatorChar);
			string filingsZip = string.Format("{0}{1}NoInstance.zip", filingDir, Path.DirectorySeparatorChar);
			string errorFile = string.Format("{0}{1}NoInstance_errorLog.txt", FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, Path.DirectorySeparatorChar);

			//Make sure we are testing with a clean filings dir
			string[] files = Directory.GetFiles(filingDir);
			foreach (string file in files)
			{
				FileUtilities.DeleteFile(new FileInfo(file), true);
			}

			//Make sure there is not a previous error file in the reports folder
			if (File.Exists(errorFile))
			{
				FileUtilities.DeleteFile(new FileInfo(errorFile), true);
			}

			//Make sure we have the correct zip file copied to the filings dir
			if (File.Exists(filingsZip))
			{
				FileUtilities.DeleteFile(new FileInfo(filingsZip), true);
			}
			File.Copy(sourceZip, filingsZip);

			string error;
			List<FilingInfo> filings;
			Assert.IsTrue(TryMoveFilingsToProcessingFolder(out filings), "TryMoveFilingsToProcessingFolder returned false");
			Assert.AreEqual(1, filings.Count, "Invalid number of filings returned by TryMoveFilingsToProcessingFolder");
			Assert.IsFalse(BuildReports(filings[0], FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, out error), "BuildReports should have failed");
			Assert.IsTrue(File.Exists(errorFile), "Error file was not generated by BuildReports when called with an invalid zip file");
		}

		[Test]
		public void TestZipNoSchemaFile()
		{
			string filingDir = FilingProcessorManager.TheMgr.FilingsFolderInfo.FullName;
			string sourceZip = string.Format("{0}{1}NoSchema.zip", testRoot, Path.DirectorySeparatorChar);
			string filingsZip = string.Format("{0}{1}NoSchema.zip", filingDir, Path.DirectorySeparatorChar);
			string errorFile = string.Format("{0}{1}NoSchema_errorLog.txt", FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, Path.DirectorySeparatorChar);

			//Make sure we are testing with a clean filings dir
			string[] files = Directory.GetFiles(filingDir);
			foreach (string file in files)
			{
				FileUtilities.DeleteFile(new FileInfo(file), true);
			}

			//Make sure there is not a previous error file in the reports folder
			if (File.Exists(errorFile))
			{
				FileUtilities.DeleteFile(new FileInfo(errorFile), true);
			}

			//Make sure we have the correct zip file copied to the filings dir
			if (File.Exists(filingsZip))
			{
				FileUtilities.DeleteFile(new FileInfo(filingsZip), true);
			}
			File.Copy(sourceZip, filingsZip);

			string error;
			List<FilingInfo> filings;
			Assert.IsTrue(TryMoveFilingsToProcessingFolder(out filings), "TryMoveFilingsToProcessingFolder returned false");
			Assert.AreEqual(1, filings.Count, "Invalid number of filings returned by TryMoveFilingsToProcessingFolder");
			Assert.IsFalse(BuildReports(filings[0], FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, out error), "BuildReports should have failed");
			Assert.IsTrue(File.Exists(errorFile), "Error file was not generated by BuildReports when called with an invalid zip file");
		}

		[Test]
		public void TestZipWithSubFolder()
		{
			string filingDir = FilingProcessorManager.TheMgr.FilingsFolderInfo.FullName;
			string sourceZip = string.Format("{0}{1}CompleteWithSubFolder.zip", testRoot, Path.DirectorySeparatorChar);
			string filingsZip = string.Format("{0}{1}CompleteWithSubFolder.zip", filingDir, Path.DirectorySeparatorChar);
			string outputZip = string.Format("{0}{1}CompleteWithSubFolder.zip", FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, Path.DirectorySeparatorChar);

			//Make sure we are testing with a clean filings dir
			string[] files = Directory.GetFiles(filingDir);
			foreach (string file in files)
			{
				FileUtilities.DeleteFile(new FileInfo(file), true);
			}

			//Make sure there is not a previous zip file in the reports folder
			if (File.Exists(outputZip))
			{
				FileUtilities.DeleteFile(new FileInfo(outputZip), true);
			}

			//Make sure we have the correct zip file copied to the filings dir
			if (File.Exists(filingsZip))
			{
				FileUtilities.DeleteFile(new FileInfo(filingsZip), true);
			}
			File.Copy(sourceZip, filingsZip);

			string error;
			List<FilingInfo> filings;
			Assert.IsTrue(TryMoveFilingsToProcessingFolder(out filings), "TryMoveFilingsToProcessingFolder returned false");
			Assert.AreEqual(1, filings.Count, "Invalid number of filings returned by TryMoveFilingsToProcessingFolder");
			Assert.IsTrue(BuildReports(filings[0], FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, out error), "BuildReports failed");
			Assert.IsTrue(File.Exists(outputZip), "Reports zip file was not generated by BuildReports.");
		}

		[Test]
		public void TestZip_SAIC_CorruptInstanceDoc()
		{
			string filingDir = FilingProcessorManager.TheMgr.FilingsFolderInfo.FullName;
			string sourceZip = string.Format("{0}{1}SAIC_CorruptInstanceDoc.zip", testRoot, Path.DirectorySeparatorChar);
			string filingsZip = string.Format("{0}{1}SAIC_CorruptInstanceDoc.zip", filingDir, Path.DirectorySeparatorChar);
			string errorFile = string.Format("{0}{1}SAIC_CorruptInstanceDoc_errorLog.txt", FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, Path.DirectorySeparatorChar);

			//Make sure we are testing with a clean filings dir
			string[] files = Directory.GetFiles(filingDir);
			foreach (string file in files)
			{
				FileUtilities.DeleteFile(new FileInfo(file), true);
			}

			//Make sure there is not a previous error file in the reports folder
			if (File.Exists(errorFile))
			{
				FileUtilities.DeleteFile(new FileInfo(errorFile), true);
			}

			//Make sure we have the correct zip file copied to the filings dir
			if (File.Exists(filingsZip))
			{
				FileUtilities.DeleteFile(new FileInfo(filingsZip), true);
			}
			File.Copy(sourceZip, filingsZip);

			string error;
			List<FilingInfo> filings;
			Assert.IsTrue(TryMoveFilingsToProcessingFolder(out filings), "TryMoveFilingsToProcessingFolder returned false");
			Assert.AreEqual(1, filings.Count, "Invalid number of filings returned by TryMoveFilingsToProcessingFolder");
			Assert.IsFalse(BuildReports(filings[0], FilingProcessorManager.TheMgr.ReportsFolderInfo.FullName, out error), "BuildReports should have failed");
			Assert.IsTrue(File.Exists(errorFile), "Error file was not generated by BuildReports when called with an invalid zip file");
			Assert.IsFalse(string.IsNullOrEmpty(error), "A valid error message was not generated by BuildReports");
		}

		#region helper methods

		private bool BuildReports(string instanceFolder, string formType)
		{
			string accessionNumber = Path.GetFileName(instanceFolder);

			string reportDir = string.Format(@"{0}{1}Reports", instanceFolder, Path.DirectorySeparatorChar);
			string filingSummaryPath = string.Format(@"{0}{1}FilingSummary.xml", reportDir, Path.DirectorySeparatorChar);

			if (Directory.Exists(reportDir))
			{
				//Make sure the reports dir is empty before starting.  That way we can
				//be sure the test actually created the correct files.
				Directory.Delete(reportDir, true);
			}
			Directory.CreateDirectory(reportDir);

			if (File.Exists(filingSummaryPath))
			{
				File.Delete(filingSummaryPath);
			}

			FilingInfo fi = new FilingInfo();
			fi.AccessionNumber = accessionNumber;
			fi.ParentFolder = testRoot;
			fi.FormType = formType;

			string error;
			return this.BuildReports(fi, reportsFolder, out error);
		}

		/// <summary>
		/// validates that the report files where generated correctly, but does not validate the contents.
		/// That functionality is unit tested in the ReportBuilder project
		/// </summary>
		/// <param name="filingSummary"></param>
		/// <param name="folderPath"></param>
		private void ValidateReports(string accensionNumber)
		{
			string generatedReportsPath = string.Format(@"{0}{1}{2}", this.reportsFolder, Path.DirectorySeparatorChar, accensionNumber);
			string generatedSummaryPath = string.Format(@"{0}{1}Filingsummary.xml", generatedReportsPath, Path.DirectorySeparatorChar);
			Assert.IsTrue(File.Exists(generatedSummaryPath), "Filing Summary file was not generated");

			object objSummary;
			string errorMsg;
			Assert.IsTrue(TryXmlDeserializeObjectFromFile(generatedSummaryPath, typeof(FilingSummary), out objSummary, out errorMsg), "Can not validate FilingSummary, unable to deserialize the \"Expected\" FilingSummary from disk: " + errorMsg);
			FilingSummary filingSummary = objSummary as FilingSummary;
			Assert.IsNotNull(filingSummary, "Can not validate FilingSummary, unable to deserialize the FilingSummary from file.");

			foreach (ReportHeader rh in filingSummary.MyReports)
			{
				string generatedReportFile = string.Format("{0}{1}{2}", generatedReportsPath, Path.DirectorySeparatorChar, rh.XmlFileName);
				if (!string.IsNullOrEmpty(rh.XmlFileName))
				{
					Assert.IsTrue(File.Exists(generatedReportFile), "Report file was not generated: " + rh.XmlFileName);
				}
			}
		}

		private static bool TryXmlDeserializeObjectFromFile(string strFileName, Type type, out object obj, out string errorMsg)
		{
			errorMsg = null;
			bool ret = true;
			obj = null;
			FileStream fsRead = null;
			try
			{

				fsRead = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
				XmlTextReader xmlReader = new XmlTextReader(fsRead);
				xmlReader.Normalization = false;
				ret = TryXmlDeserializeObjectFromFile(xmlReader, type, out obj, out errorMsg);
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
				ret = false;
			}
			finally
			{
				if (fsRead != null) fsRead.Close();
			}
			return ret;
		}

		private static bool TryXmlDeserializeObjectFromFile(XmlTextReader xmlReader, Type type, out object obj, out string errorMsg)
		{
			errorMsg = null;
			obj = null;

			try
			{
				XmlSerializer serializer = new XmlSerializer(type);
				obj = serializer.Deserialize(xmlReader);

				return true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;

				return false;
			}
		}

		#endregion
	}
}

#endif

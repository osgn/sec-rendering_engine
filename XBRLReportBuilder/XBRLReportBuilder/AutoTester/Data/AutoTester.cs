using System;
using System.Configuration;
using System.IO;
using System.Text;
using Aucent.FilingServices.Data;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;
using XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Data
{
    static class AutoTester
    {
        #region members

        private delegate void output(string message);
        private delegate void report(string message);

        public static string baseFilingsPath = ConfigurationManager.AppSettings["baseZipPath"];
        public static string newFilingsPath = ConfigurationManager.AppSettings["newZipPath"];
        public static string logPath = ConfigurationManager.AppSettings["logPath"];

        public const string zipCopyErr = @"Cannot copy zip to the specified folder; file: {0}, error: {1}";
        public const string fileDelErr = @"Cannot delete specified item; file/directory: {0}, error: {1}";

        private const string followingPath = @"Could not reach the following path: {0}";
        private const string errorOccured = @"The following error occured while trying to set up: {0}";
                
        private static string taxPath = string.Empty;
        private static string instancePath = string.Empty;

        public static bool baseHTML = false;

        public static double progressPercentage = 0;

        #endregion
		/// <summary>
		/// Runs a test on the given base (could be either a folder or zip file containing R Files).  The method can
		/// accept either a zip or a folder of R files.  The name of the folder/zip of the new files must match that
		/// of the base path.
		/// </summary>
		/// <param name="testSummary"></param>
		/// <param name="testErrors"></param>
		/// <param name="baseZipOrDir"></param>
		public static void RunTest( FilingResult testSummaryItem, StringBuilder testErrors, string baseZipOrDir )
		{
			// The name of the filing is used to build all paths
			testSummaryItem.Name = Path.GetFileNameWithoutExtension( baseZipOrDir );
			string filingLogFolder = Test_Abstract.PathCombine( AutoTester.logPath, testSummaryItem.Name );

			// Preparing (clean/create) folders for output
			Test_Abstract.CleanAndPrepareFolder(filingLogFolder);

			int errorCount = 0;

			IDirectoryInfo baseDirInfo = null;
			IDirectoryInfo newDirInfo = null;

			try
			{
				// The base can be a directory, and if it is, then it needs to be handled differently.
				if (Directory.Exists(baseZipOrDir))
				{
					baseDirInfo = new ExtendedDirectoryInfo( baseZipOrDir );
				}
				else if (File.Exists(baseZipOrDir))
				{
					baseDirInfo = new ZipDirectoryInfo( baseZipOrDir );
				}
				else
				{
					// This shouldn't occur if the method was called from a file listing, but handle anyway
					string message = "The the zip or folder for the Base filing was missing.  Tried to process the following as a directory and zip file: \r\n\t";
					message += baseZipOrDir;

					string resultPath = Test_Abstract.PathCombine(filingLogFolder, "Base Filing Zip or Folder Missing.txt");
					File.WriteAllText(resultPath, message);

					testSummaryItem.Success = false;
					testSummaryItem.Reason = "The base file name given to the processor could not be found.";
					return;
				}

				// When picking the new files, prefer directory over zip
				if( Directory.Exists( Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name) ) )
				{
					string dirPath = Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name );
					newDirInfo = new ExtendedDirectoryInfo( dirPath );
				}
				else if( File.Exists( Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name + ".zip" ) ) )
				{
					string zipPath = Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name + ".zip" );
					newDirInfo = new ZipDirectoryInfo( zipPath );
				}
				else
				{
					string message = "The the zip or folder for the New filing was missing.  Tried the directory: \r\n\t";
					message += Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name );
					message += "\r\nAnd the file: \r\n\t";
					message += Test_Abstract.PathCombine( AutoTester.newFilingsPath, testSummaryItem.Name + ".zip" );

					string resultPath = Test_Abstract.PathCombine(filingLogFolder, "New Filing Zip or Folder Missing.txt");
					File.WriteAllText(resultPath, message);

					testSummaryItem.Success = false;
					testSummaryItem.Reason = "The new file corresponding to the base file could not be found.";
					return;
				}

				// 
				using (ErrorWriter writer = new ErrorWriter())
				{
					Test_Abstract.VerifyIndividualReports(writer, baseDirInfo, newDirInfo, filingLogFolder);
					Test_Abstract.VerifyCharts( writer, baseDirInfo, newDirInfo );

					if (writer.HasErrors)
					{
						writer.Flush();
						writer.SaveResults(filingLogFolder);

						testSummaryItem.Errors = writer.Errors;
						testSummaryItem.Success = true;
						testSummaryItem.Reason = "See error log.";

						testErrors.AppendFormat( "Error in: {0}\r\n{1}\r\n\r\n\r\n", testSummaryItem.Name, writer.GetStringBuilder().ToString().Trim() );
					}
					else
					{
						testSummaryItem.Success = true;
					}

					errorCount = writer.Errors;
				}
			}
			catch (Exception e)
			{
				testSummaryItem.Success = false;
				testSummaryItem.Reason = "Exception) "+ e.Message;
			}
			finally
			{
				if( errorCount == 0 )
				{
					try
					{
						Directory.Delete( filingLogFolder, true );
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// Prepares the object's paths for output, deleting any content or creating the folders if they don't exist.
		/// </summary>
        public static void PrepareTestPaths()
        {
            // Clutter in output directories could cause conflicts or confuse users
            Test_Abstract.CleanAndPrepareFolder(AutoTester.logPath);
        }

		private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
		{

			foreach (DirectoryInfo dir in source.GetDirectories())
			{
				CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
			}

			foreach (FileInfo file in source.GetFiles())
			{
				FileUtilities.Copy(file.FullName, Test_Abstract.PathCombine(target.FullName, file.Name));
			}
		}
		
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using XBRLReportBuilder;
using XBRLReportBuilder.Utilities;
using XRB = XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using Aucent.FilingServices.Data;
using System.Net.Cache;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Data
{
	public abstract partial  class Test_Abstract
	{

        public static bool BuildAndVerifyWithoutResults = false;
        public static ReportFormat OutputFormat = ReportFormat.Xml;
        public static HtmlReportFormat HtmlFormat = HtmlReportFormat.None;
        public static RequestCacheLevel CachePolicy = RequestCacheLevel.Default;
        public static string ResourcePath = null;
        public static bool UpdateResultsFlag = false;

        //public static bool IsConcurrentTestResult = false;
        public static int ConcurrentTestsCount = 4;
        public static bool ConcurrentTestsRunning = false;
        public static bool RunAllConcurrentFlag = false;
        public static volatile Dictionary<string, Exception> ConcurrentTestsResults = new Dictionary<string, Exception>();

		protected abstract string relativeRoot { get; }

		private const string R_FILE_PATTERN = @"^R\d+\.xml$";
		private static readonly Regex R_FILE_REGEX = new Regex( R_FILE_PATTERN, RegexOptions.Compiled );

		protected static readonly XmlSerializer instanceReportSerializer = new XmlSerializer( typeof( InstanceReport ) );

		private string assemblyPath = string.Empty;
		protected string baseDir
		{
			get
			{
				if( string.IsNullOrEmpty( this.assemblyPath ) )
				{
					//holds the assembly file path
					this.assemblyPath = this.GetType().Assembly.CodeBase.Substring( 8 );

					//now it holds the assembly directory (hopefully `bin`)
					this.assemblyPath = Path.GetDirectoryName( this.assemblyPath );

					//now it holds the assembly directory parent (hopefully this is the project root)
					this.assemblyPath = Path.GetDirectoryName( this.assemblyPath );
				}

				return Path.Combine( this.assemblyPath, this.relativeRoot );
			}
		}

		protected string reportsPath = string.Empty;
		protected string resultsPath = string.Empty;

		protected FilingSummary myFilingSummary = null;
		protected TimeSpan processingTime = TimeSpan.Zero;

		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        public abstract void RunFirst();

		public virtual void RunLast()
		{
		}

		/// <summary> Sets up test values before each test is called </summary>
		public virtual void RunBeforeEachTest()
		{
		}

		/// <summary>Tears down test values after each test is run </summary>
		public abstract void RunAfterEachTest();

		#region PUBLIC

		public bool BuildReports( string instanceFolder, string instanceName, string taxonomyName, out FilingSummary filingSummary, out string error )
		{
			Console.WriteLine( "=================================================" );
			Console.WriteLine( "  BUILD " + instanceFolder + "..." );
			Console.WriteLine( "=================================================" );

			this.processingTime = TimeSpan.Zero;
			DateTime startTime = DateTime.Now;

			string instance = PathCombine( instanceFolder, instanceName + ".xml" );
			string taxonomy = PathCombine( instanceFolder, taxonomyName + ".xsd" );
			string reportDir = PathCombine( instanceFolder, "Reports" );
			string filingSummaryPath = PathCombine( reportDir, "FilingSummary.xml" );

			CleanAndPrepareFolder( reportDir );

			XRB.ReportBuilder myReportBuilder = new XRB.ReportBuilder();
			myReportBuilder.ReportFormat = OutputFormat;
			myReportBuilder.HtmlReportFormat = HtmlFormat;
			myReportBuilder.RemoteFileCachePolicy = CachePolicy;
			myReportBuilder.OnBuildReportsProcessing += this.FireBuildReportsProcessing;
			myReportBuilder.OnBuildReportsProcessed += this.FireBuildReportsProcessed;

			if( string.IsNullOrEmpty( ResourcePath ) )
				RulesEngineUtils.SetBaseResourcePath( null );
			else
				RulesEngineUtils.SetBaseResourcePath( ResourcePath );

			bool status = myReportBuilder.BuildReports( instance, taxonomy, filingSummaryPath, reportDir, out filingSummary, out error );
			this.myFilingSummary = status ? filingSummary : null;

			myReportBuilder.OnBuildReportsProcessing -= this.FireBuildReportsProcessing;
			myReportBuilder.OnBuildReportsProcessed -= this.FireBuildReportsProcessed;

			DateTime endTime = DateTime.Now;
			this.processingTime = ( endTime - startTime );

			return status;
		}

		public static bool CleanAndPrepareFolder( string folder )
		{
			bool deleted = false;
			if (Directory.Exists(folder))
			{
				try
				{
					foreach (string file in Directory.GetFiles(folder))
					{
						File.Delete(file);
					}

					foreach (string dir in Directory.GetDirectories(folder))
					{
						Directory.Delete(dir, true);
					}

					deleted = true;
				}
				catch { }
			}
			else
			{
				Directory.CreateDirectory(folder);
			}

			return deleted;
		}

		public static void CopyResourcesToPath( string copyPath )
		{
			string[] resources = new string[]
			{
				RulesEngineUtils.TransformFile,
				RulesEngineUtils.StylesheetFile,
				RulesEngineUtils.JavascriptFile
			};

			string xsltPath = string.Empty;
			foreach( string resource in resources )
			{
				string from = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, resource );
				string to = Path.Combine( copyPath, resource );
				FileUtilities.Copy( from, to );
			}
		}

		public static string PathCombine( params string[] pathParts )
		{
			string path = string.Join( Path.DirectorySeparatorChar.ToString(), pathParts );
			return path;
		}

		public static string Transform( string xmlPath )
		{
			XsltArgumentList argList = new XsltArgumentList();
			argList.AddParam( "asPage", string.Empty, "true" );
            argList.AddParam( "numberDecimalSeparator", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator );
            argList.AddParam( "numberGroupSeparator", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator );
            argList.AddParam( "numberGroupSize", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSizes[ 0 ].ToString() ); 

			return Transform( xmlPath, argList );
		}

		public static string Transform( string xmlPath, XsltArgumentList argList )
		{
			XmlWriterSettings xSettings = new XmlWriterSettings();
			xSettings.ConformanceLevel = ConformanceLevel.Fragment;
			xSettings.Encoding = InstanceReport.Encoding;
			xSettings.Indent = true;
			xSettings.OmitXmlDeclaration = true;

			//we REALLY need to set this to HTML
			xSettings.GetType().GetProperty( "OutputMethod" ).SetValue( xSettings, XmlOutputMethod.Html, null );

			return Transform( xmlPath, xSettings, argList );
		}

		public static string Transform( string xmlPath, XmlWriterSettings xSettings, XsltArgumentList argList )
		{
			string html = string.Empty;

			string transformFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.TransformFile );

			XslCompiledTransform transform = new XslCompiledTransform();
			transform.Load( transformFile );

			using( XmlReader xmlReader = XmlReader.Create( xmlPath ) )
			{
				using( MemoryStream ms = new MemoryStream() )
				{
					try
					{
						using( XmlWriter xWriter = new RivetXmlWriter( ms, xSettings ) )
						{
							transform.Transform( xmlReader, argList, xWriter );
						}

						html = xSettings.Encoding.GetString( ms.ToArray() );
						return html;
					}
					catch( Exception ex )
					{
						html = "<h2>Error <small>generating preview</small></h2>" +
							"<pre>" + ex.Message + "</pre>";
					}
				}
			}

			return html;
		}

		protected static List<Type> runTypesConcurrent = new List<Type>();
		public static void RunAllConcurrent( Type t )
		{
			if( !runTypesConcurrent.Contains( t ) )
				runTypesConcurrent.Add( t );
		}

		#endregion

		#region PROTECTED

		protected FilingSummary BuildAndVerifyRecursive( string folderInstanceTaxonomy )
		{
			return this.BuildAndVerifyRecursive( folderInstanceTaxonomy, folderInstanceTaxonomy, folderInstanceTaxonomy );
		}

		protected FilingSummary BuildAndVerifyRecursive( string folder, string instanceName, string taxonomyName )
		{
			Console.WriteLine( "=================================================" );
			Console.WriteLine( "  BUILD AND VERIFY " + folder + "..." );
			Console.WriteLine( "=================================================" );

			string baselineResultFolder = PathCombine( this.baseDir, folder, "Results" );
			this.resultsPath = baselineResultFolder;

			if( !BuildAndVerifyWithoutResults && !Directory.Exists( this.resultsPath ) )
				throw new Exception( "There are no results to verify againsts." );

			string generatedResultFolder = PathCombine( this.baseDir, folder, "Reports" );
			this.reportsPath = generatedResultFolder;

			string failedResultsFolder = Path.GetDirectoryName( generatedResultFolder );
			failedResultsFolder = Path.Combine( failedResultsFolder, "Failures" );
			CleanAndPrepareFolder( failedResultsFolder );

			FilingSummary fs = BuildFiling( folder, instanceName, taxonomyName );
			using( ErrorWriter writer = new ErrorWriter() )
			{
				ExtendedDirectoryInfo baselineResultInfo = new ExtendedDirectoryInfo( baselineResultFolder );
				ExtendedDirectoryInfo generatedResultInfo = new ExtendedDirectoryInfo( generatedResultFolder );
				VerifyIndividualReports( writer, baselineResultInfo, generatedResultInfo, failedResultsFolder );
				VerifyCharts( writer, baselineResultInfo, generatedResultInfo );

				if( writer.HasErrors )
				{
					writer.Flush();
					writer.SaveResults( failedResultsFolder );

					Console.Write( writer.GetStringBuilder().ToString().Trim() );
					throw new Exception( writer.GetStringBuilder().ToString().Trim() );
				}
			}

			return fs;
		}

		protected FilingSummary BuildFiling( string accessionNumberIn, string instanceNameIn, string taxonomyNameIn )
		{
			if( !ConcurrentTestsRunning )
			{
				Type myType = MethodInfo.GetCurrentMethod().DeclaringType;

				MethodBase method = null;
				StackTrace trace = new StackTrace();
				StackFrame[] frames = trace.GetFrames();
				for( int f = 0; f < frames.Length; f++ )
				{
					method = frames[ f ].GetMethod();
					if( method.DeclaringType != myType )
						break;
				}

				if( ConcurrentTestsResults.ContainsKey( method.Name ) )
				{
					//RB_Tests.IsConcurrentTestResult = true;
					if( ConcurrentTestsResults[ method.Name ] != null )
						throw ConcurrentTestsResults[ method.Name ];
					else
						return null;
				}
			}

			string error;
			FilingSummary filingSummary = null;
			string folderPath = Path.Combine( baseDir, accessionNumberIn );
			bool buildSuccess = BuildReports( folderPath, instanceNameIn, taxonomyNameIn, out filingSummary, out error );

            if (!buildSuccess)
            {
                throw new Exception("Failed to generate reports: " + error);
            }

			this.myFilingSummary = filingSummary;
			return filingSummary;
		}

		#endregion

		#region PRIVATE

		private void CopyResourcesToReportsFolder()
		{
			if( string.IsNullOrEmpty( this.reportsPath ) )
				return;

			CopyResourcesToPath( this.reportsPath );
		}

		protected void UpdateResults()
		{
			if( !Directory.Exists( this.resultsPath ) )
				return;

			string script1 = @"CALL ""C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"" x86";
			string script2 = @"tf checkout """ + this.resultsPath + @"\R*.xml""";
			string script3 = @"tf checkout """ + this.resultsPath + @"\BarChart*.jpg""";
			string script4 = @"xcopy /RYU """ + this.reportsPath + @"\R*.xml"" """ + this.resultsPath + @"\""";
			string script5 = @"xcopy /RYU """ + this.reportsPath + @"\BarChart*.jpg"" """ + this.resultsPath + @"\""";

			string tmp = Path.GetTempFileName();
			File.Move( tmp, tmp + ".bat" );
			tmp += ".bat";

			try
			{
				File.WriteAllLines( tmp, new string[] { script1, script2, script3, script4, script5 } );

				ProcessStartInfo psi = new ProcessStartInfo( tmp );
				psi.CreateNoWindow = true;
				
				Process p = Process.Start( psi );
				p.WaitForExit();
				p.Close();
			}
			finally
			{
				File.Delete( tmp );
			}
		}

		public static void VerifyCharts( ErrorWriter writer, IDirectoryInfo baselineResultInfo, IDirectoryInfo generatedResultInfo )
		{
			IFileInfo[] baseFiles = new IFileInfo[ 0 ];
			if( baselineResultInfo.Exists )
				baseFiles = baselineResultInfo.GetFiles( "*.jpg" );

			IFileInfo[] genFiles = new IFileInfo[ 0 ];
			if( generatedResultInfo.Exists )
				genFiles = generatedResultInfo.GetFiles( "*.jpg" );

			if( baseFiles.Length < genFiles.Length )
			{
				writer.StartError( "The new rendering generated too MANY charts:" );
				foreach( IFileInfo gFile in genFiles )
				{
					int foundAt = Array.FindIndex( baseFiles, bFile => string.Equals( bFile.Name, gFile.Name ) );
					if( foundAt == -1 )
					{
						writer.WriteError( "\tAdded:  " + gFile );
					}
				}
				writer.EndError();
			}
			else if( baseFiles.Length > genFiles.Length )
			{
				writer.StartError( "The new rendering generated too FEW charts." );
				foreach( IFileInfo bFile in baseFiles )
				{
					int foundAt = Array.FindIndex( genFiles, gFile => string.Equals( gFile.Name, bFile.Name ) );
					if( foundAt == -1 )
					{
						writer.WriteError( "\tRemoved: " + bFile );
					}
				}
				writer.EndError();
			}

			foreach( IFileInfo baseFI in baseFiles )
			{
				IFileInfo genFI = Array.Find( genFiles, gFile => string.Equals( gFile.Name, baseFI.Name ) );

				//due to optimization routines, minor differences occur in the file size of the charts.
				if( baseFI.Length != genFI.Length && Math.Abs( baseFI.Length - genFI.Length ) > 1150 )
				{
					writer.WriteError(
						"<strong>Charts do not match:</strong>",
						"\tExpected length:  " + baseFI.Length,
						"\tGenerated length: " + genFI.Length
					);
				}
			}
		}

		private static readonly Regex WHITE_SPACE = new Regex( @"(&#160;|\s)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture );
		public static void VerifyIndividualReports( ErrorWriter writer, IDirectoryInfo baselineResultInfo, IDirectoryInfo generatedResultInfo, string failedResultsFolder )
		{
			IFileInfo[] baseFiles = new IFileInfo[ 0 ];
			if( baselineResultInfo.Exists )
			{
				baseFiles = baselineResultInfo.GetFiles( "R*.xml" );
				Array.Sort( baseFiles, natcasecmp );
			}

			IFileInfo[] genFiles = new IFileInfo[ 0 ];
			if( generatedResultInfo.Exists )
			{
				genFiles = generatedResultInfo.GetFiles( "R*.xml" );
				Array.Sort( genFiles, natcasecmp );
			}

			if( baseFiles.Length < genFiles.Length )
			{
				writer.StartError( "The new rendering generated too MANY files:" );
				for( int i = 0; i < genFiles.Length; i++ )
				{
					bool found = Array.Exists( baseFiles, bFile => string.Equals( bFile.Name, genFiles[ i ].Name ) );
					if( !found )
						writer.WriteError( "\tAdded:  " + genFiles[ i ].Name );
				}
				writer.EndError();
			}
			else if( baseFiles.Length > genFiles.Length )
			{
				writer.StartError( "The new rendering generated too FEW files." );
				for( int i = 0; i < baseFiles.Length; i++ )
				{
					bool found = Array.Exists( genFiles, gFile => string.Equals( gFile.Name, baseFiles[ i ].Name ) );
					if( !found )
						writer.WriteError( "\tAdded:  " + baseFiles[ i ].Name );
				}
				writer.EndError();
			}

			Dictionary<string, int> allFiles = new Dictionary<string, int>();
			foreach( IFileInfo bFile in baseFiles )
				allFiles[ bFile.Name ] = 1;

			foreach( IFileInfo gFile in genFiles )
				allFiles[ gFile.Name ] = 1;

			Dictionary<string, InstanceReport> tryVerifyEmbedded = new Dictionary<string, InstanceReport>();
			foreach( string file in allFiles.Keys )
			{
				if( !R_FILE_REGEX.IsMatch( file ) )
					continue;

				InstanceReport baseReport = null;
				InstanceReport genReport = null;

				try
				{
					if( baselineResultInfo.FileExists( file ) )
					{
						using( Stream fileStream = baselineResultInfo.OpenStream( file ) )
						{
							baseReport = InstanceReport.LoadXmlStream( fileStream );
						}
					}

					if( generatedResultInfo.FileExists( file ) )
					{
						using( Stream fileStream = generatedResultInfo.OpenStream( file ) )
						{
							genReport = InstanceReport.LoadXmlStream( fileStream );
						}
					}

					if( baseReport == null )
					{
						throw new Exception( "Base report is missing." );
					}
					else
					{
						foreach( InstanceReportRow row in baseReport.Rows )
						{
							foreach( Cell cell in row.Cells )
							{
								cell.FlagID = 0;
							}
						}
					}

					if( genReport == null )
					{
						throw new Exception( "Generated report is missing." );
					}
					else
					{
						foreach( InstanceReportRow row in baseReport.Rows )
						{
							foreach( Cell cell in row.Cells )
							{
								cell.FlagID = 0;
							}
						}
					}

					writer.StartReport( file, baseReport, genReport, failedResultsFolder );
					writer.WriteLine( PathCombine( baselineResultInfo.FullName, file ) );
					writer.WriteLine( PathCombine( generatedResultInfo.FullName, file ) );

					VerifyReports( writer, baseReport, genReport );
				}
				catch( Exception ex )
				{
					writer.StartReport( file, baseReport, genReport, failedResultsFolder );
					writer.WriteError( ex.Message );
				}
				finally
				{
					writer.EndReport( file );
				}
			}
		}

		private static void VerifyReports( ErrorWriter writer, InstanceReport baseReport, InstanceReport genReport )
		{
			if(WHITE_SPACE.Replace( baseReport.ReportLongName, " " ) != WHITE_SPACE.Replace( genReport.ReportLongName, " " ) )
			{
				writer.WriteError( "<strong>Long report name does not match:</strong>",
					"\tExpected:  " + WHITE_SPACE.Replace( baseReport.ReportLongName, " " ),
					"\tGenerated: " + WHITE_SPACE.Replace( genReport.ReportLongName, " " ),
					"\t...continuing...",
					string.Empty );
			}

			if( WHITE_SPACE.Replace( baseReport.ReportName, " " ) != WHITE_SPACE.Replace( genReport.ReportName, " " ) )
			{
				writer.WriteError( "<strong>Short report name does not match:</strong>",
					"\tExpected:  " + WHITE_SPACE.Replace( baseReport.ReportName, " " ),
					"\tGenerated: " + WHITE_SPACE.Replace( genReport.ReportName, " " ),
					"\t...continuing...",
					string.Empty );
			}

			VerifyReportColumns( writer, baseReport, genReport );
			VerifyReportRows( writer, baseReport, genReport );
            VerifyReportFootnotes( writer, baseReport, genReport );
		}

		private static void VerifyReportColumns( ErrorWriter writer, InstanceReport baseReport, InstanceReport genReport )
		{
			if( baseReport.Columns.Count != genReport.Columns.Count )
			{
				int differences = Math.Abs( baseReport.Columns.Count - genReport.Columns.Count );
				List<string> baseColumns = baseReport.Columns.ConvertAll( col => WHITE_SPACE.Replace( col.Label, " " ) );
				List<string> genColumns = genReport.Columns.ConvertAll( col => WHITE_SPACE.Replace( col.Label, " " ) );

				if( baseColumns.Count < genColumns.Count )
				{
					writer.StartError( "The new rendering generated too MANY columns." );

					foreach( string commonLabel in baseColumns )
					{
						if( genColumns.Contains( commonLabel ) )
							genColumns.Remove( commonLabel );
					}

					if( genColumns.Count == differences )
					{
						foreach( string newLabel in genColumns )
						{
							writer.WriteError( "\tAdded:   " + newLabel );
						}
					}
					else
					{
						writer.WriteError( "\tSEVERAL LABELS CHANGED" );
					}

					writer.EndError();
				}
				else
				{
					writer.StartError( "The new rendering generated too FEW columns." );

					foreach( string commonLabel in genColumns )
					{
						if( baseColumns.Contains( commonLabel ) )
							baseColumns.Remove( commonLabel );
					}

					if( baseColumns.Count == differences )
					{
						foreach( string newLabel in baseColumns )
						{
							writer.WriteError( "\tRemoved: " + newLabel );
						}
					}
					else
					{
						writer.WriteError( "\tSEVERAL LABELS CHANGED" );
					}

					writer.EndError();
				}
			}

			for( int bIdx = 0; bIdx < baseReport.Columns.Count; bIdx++ )
			{
				InstanceReportColumn baseColumn = baseReport.Columns[ bIdx ];
				string baseLabel = WHITE_SPACE.Replace( baseColumn.Label, " " );

				InstanceReportColumn genColumn = null;
				if( genReport.Columns.Count > bIdx )
					genColumn = genReport.Columns[ bIdx ];

				if( genColumn == null )
				{
					writer.WriteError( "<strong>Base column not found in generated report.</strong>",
						"\t" + baseLabel,
						"\t***Column Skipped***" );
					continue;
				}

				if( WHITE_SPACE.Replace( genColumn.Label, " " ) == baseLabel )
					continue;

				decimal baseSum = 0.0M;
				Array.ForEach( baseColumn.GetCellArray( baseReport ), cell => baseSum += cell.IsNumeric ? cell.NumericAmount : 0.0M );

				decimal genSum = 0.0M;
				Array.ForEach( genColumn.GetCellArray( genReport ), cell => genSum += cell.IsNumeric ? cell.NumericAmount : 0.0M );

				if( baseSum == genSum )
				{
					writer.WriteError( "<strong>Label doesn't match base column at index " + bIdx + ":</strong>",
						"\tExpected:  " + baseLabel,
						"\tGenerated: " + WHITE_SPACE.Replace( genColumn.Label, " " ) );
					continue;
				}

				genColumn = genReport.Columns.Find( col => WHITE_SPACE.Replace( col.Label, " " ) == baseLabel );
				if( genColumn == null )
				{
					writer.WriteError( "<strong>Base column not found in generated report:</strong>",
						"\t" + baseLabel,
						"\t***Column Skipped***" );
					continue;
				}

				int gIdx = genReport.Columns.IndexOf( genColumn );
				if( bIdx != gIdx )
				{
					writer.WriteError( "<strong>Column moved: " + baseLabel +"</strong>",
						"\tExpected at:  " + bIdx.ToString(),
						"\tGenerated at: " + gIdx.ToString() );
				}
			}
		}

		private static void VerifyReportRows( ErrorWriter writer, InstanceReport baseReport, InstanceReport genReport )
		{
			if( baseReport.Rows.Count != genReport.Rows.Count )
			{
				int differences = Math.Abs( baseReport.Rows.Count - genReport.Rows.Count );
				List<string> baseRows = baseReport.Rows.ConvertAll( row => WHITE_SPACE.Replace( row.Label, " " ) );
				List<string> genRows = genReport.Rows.ConvertAll( row => WHITE_SPACE.Replace( row.Label, " " ) );

				if( baseRows.Count < genRows.Count )
				{
					writer.StartError( "<strong>The new rendering generated too MANY rows.</strong>" );

					foreach( string commonLabel in baseRows )
					{
						if( genRows.Contains( commonLabel ) )
							genRows.Remove( commonLabel );
					}

					if( genRows.Count == differences )
					{
						foreach( string newLabel in genRows )
						{
							writer.WriteError( "\tAdded:   " + newLabel );
						}
					}
					else
					{
						writer.WriteError( "\tSEVERAL LABELS CHANGED" );
					}

					writer.EndError();
				}
				else
				{
					writer.StartError( "<strong>The new rendering generated too FEW rows.</strong>" );

					foreach( string commonLabel in genRows )
					{
						if( baseRows.Contains( commonLabel ) )
							baseRows.Remove( commonLabel );
					}

					if( baseRows.Count == differences )
					{
						foreach( string newLabel in baseRows )
						{
							writer.WriteError( "\tRemoved: " + newLabel );
						}
					}
					else
					{
						writer.WriteError( "\tSEVERAL LABELS CHANGED" );
					}

					writer.EndError();
				}
			}

			for( int bIdx = 0; bIdx < baseReport.Rows.Count; bIdx++ )
			{
				InstanceReportRow baseRow = baseReport.Rows[ bIdx ];
				string baseLabel = WHITE_SPACE.Replace( baseRow.Label, " " );

				int gIdx = -1;
				InstanceReportRow genRow = null;
				if( genReport.Rows.Count > bIdx )
				{
					if( WHITE_SPACE.Replace( genReport.Rows[ bIdx ].Label, " " ) == baseLabel )
					{
						gIdx = bIdx;
						genRow = genReport.Rows[ bIdx ];
					}
				}

				if( genRow == null )
				{
					if( bIdx >= genReport.Rows.Count )
					{
						writer.WriteError(
							"<strong>Generated report has too few rows to match Base row:</strong>",
							"\tRow " + bIdx + ": " + baseLabel,
							"\t***Row Skipped***"
						);
						continue;
					}

					gIdx = bIdx == 0 ?
						genReport.Rows.FindIndex( bIdx, row => WHITE_SPACE.Replace( row.Label, " " ) == baseLabel ) :
						genReport.Rows.FindIndex( bIdx - 1, row => WHITE_SPACE.Replace( row.Label, " " ) == baseLabel );

					if( gIdx == -1 )
					{
						writer.WriteError(
							"<strong>Base row not found in generated report:</strong>",
							"\tRow " + bIdx + ": " + baseLabel,
							"\t***Row Skipped***"
						);
						continue;
					}

					genRow = genReport.Rows[ gIdx ];
					if( bIdx != gIdx )
					{
						writer.WriteError(
							"<strong>Row moved:</strong> " + baseLabel,
							"\tExpected:  " + bIdx.ToString(),
							"\tGenerated: " + gIdx.ToString()
						);
					}
				}

				if( !string.Equals( baseRow.FootnoteIndexer, genRow.FootnoteIndexer ) )
				{
					writer.WriteError(
						"<strong>Row footnotes changed:</strong> " + baseLabel,
						"\tExpected:  " + baseRow.FootnoteIndexer,
						"\tGenerated: " + genRow.FootnoteIndexer
					);
				}

				decimal baseSum = 0M;
				baseRow.Cells.FindAll( c => c.IsNumeric ).ForEach( c => baseSum += c.NumericAmount );

				decimal genSum = 0M;
				genRow.Cells.FindAll( c => c.IsNumeric ).ForEach( c => genSum += c.NumericAmount );

				decimal difference = baseSum - genSum;
				if( difference != 0 )
				{
					if( difference > 0 )
					{
						writer.WriteError(
							"<strong>Row sum is SMALLER than expected:</strong>",
							"\tRow " + bIdx + ": " + baseLabel,
							"\tExpected:  " + baseSum,
							"\tGenerated: " + genSum
						);
					}
					else
					{
						writer.WriteError(
							"<strong>Row sum is LARGER than expected:</strong>",
							"\tRow " + bIdx + ": " + baseLabel,
							"\tExpected:  " + baseSum,
							"\tGenerated: " + genSum
						);
					}
				}

				if( baseRow.IsTotalLabel != genRow.IsTotalLabel )
				{
					writer.WriteError(
						"<strong>Row 'Total Label' flag changed:</strong>",
						"\tRow " + bIdx + ": " + baseLabel,
						"\tExpected:  " + baseRow.IsTotalLabel,
						"\tGenerated: " + genRow.IsTotalLabel
					);
				}

				foreach( Cell baseCell in baseRow.Cells )
				{
					Cell genCell = null;
					int cIdx = baseRow.Cells.IndexOf( baseCell );
					if( genRow.Cells.Count > cIdx )
						genCell = genRow.Cells[ cIdx ];

					if( genCell == null )
					{
						writer.WriteError( "\tBase cell not found in Generated report." );
						continue;
					}

					if( !genCell.IsNumeric )
					{
						genCell.NonNumbericText = WHITE_SPACE.Replace( genCell.NonNumbericText, " " );
					}

					if( !baseCell.IsNumeric )
					{
						if( baseCell.NonNumbericText == "&nbsp;" )
							baseCell.NonNumbericText = Cell.NILL_PLACEHOLDER;
						else
							baseCell.NonNumbericText = WHITE_SPACE.Replace( baseCell.NonNumbericText, " " );
					}


					List<string> diffs = new List<string>();
					if( !baseCell.ValueEquals( genCell, ref diffs ) )
					{
						baseCell.FlagID = 1;
						genCell.FlagID = 1;

						string props = string.Join( ", ", diffs.ToArray() );
						if( !string.IsNullOrEmpty( props ) )
							props = "\t\tProperties: "+ props;

						writer.WriteError( "\t<strong>Cell value does not match:</strong>",
							"\t\tRow " + bIdx + ", Cell " + cIdx,
							"\t\tExpected:  " + baseCell.ToString(),
							"\t\tGenerated: " + genCell.ToString(),
							props );
					}
					else if( !string.Equals( baseCell.FootnoteIndexer, genCell.FootnoteIndexer ) )
					{
						baseCell.FlagID = 1;
						genCell.FlagID = 1;

						writer.WriteError( "<strong>Cell footnotes changed:</strong>",
							"\t\tRow " + bIdx + ", Cell " + cIdx,
							"\t\tExpected:  " + baseCell.FootnoteIndexer,
							"\t\tGenerated: " + genCell.FootnoteIndexer );
					}
				}

				List<Cell> baseEmbedCells = baseRow.Cells.FindAll( c => c.HasEmbeddedReport && c.EmbeddedReport != null && c.EmbeddedReport.InstanceReport != null );
				List<Cell> genEmbedCells = genRow.Cells.FindAll( c => c.HasEmbeddedReport && c.EmbeddedReport != null && c.EmbeddedReport.InstanceReport != null );
				if( baseEmbedCells.Count != genEmbedCells.Count )
				{
					difference = Math.Abs( baseEmbedCells.Count - genEmbedCells.Count );
					if( baseEmbedCells.Count > genEmbedCells.Count )
					{
						writer.WriteError( "<strong>Row has MISSING embedded reports:</strong>",
							"\tExpected:  " + baseEmbedCells.Count,
							"\tGenerated: " + genEmbedCells.Count );
					}
					else
					{
						writer.WriteError( "<strong>Row has EXTRA embedded reports:</strong>",
							"\tExpected:  " + baseEmbedCells.Count,
							"\tGenerated: " + genEmbedCells.Count );
					}
				}

				for( int c = 0; c < baseEmbedCells.Count; c++ )
				{
					Cell embedCell = baseEmbedCells[ c ];
					InstanceReport baseEmbed = embedCell.EmbeddedReport.InstanceReport;
					int genCellIndex = c == 0 ?
						genEmbedCells.FindIndex( cell => baseEmbed.ReportLongName == cell.EmbeddedReport.InstanceReport.ReportLongName ) :
						genEmbedCells.FindIndex( c - 1, cell => baseEmbed.ReportLongName == cell.EmbeddedReport.InstanceReport.ReportLongName );

					if( genCellIndex == -1 )
					{
						writer.WriteError( "<strong>Embedded report not found in generated report:</strong>",
							"\tRow " + bIdx + ", Cell " + c + ": " + baseEmbed.ReportName,
							"\tEMBED Skipped" );
						continue;
					}

					Cell genEmbedCell = genEmbedCells[ genCellIndex ];
					InstanceReport genEmbed = genEmbedCell.EmbeddedReport.InstanceReport;

					try
					{
						writer.StartReport( baseEmbed.ReportLongName + ".xml", baseEmbed, genEmbed, null );
						writer.WriteLine( "*************** EMBED Comparison Started ***************" );

						VerifyReports( writer, baseEmbed, genEmbed );

						writer.WriteLine( "***************  EMBED Comparison Ended  ***************" );
					}
					finally
					{
						writer.EndReport( baseEmbed.ReportLongName + ".xml" );
					}
				}
			}
		}

        /// <summary>
        /// Compares the footnotes of the base and generated reports and writes errors for discrepancies.
        /// </summary>
        /// <param name="writer">Where to output errors</param>
        /// <param name="baseReport">The base report to compare against</param>
        /// <param name="genReport">The new report for testing</param>
        private static void VerifyReportFootnotes( ErrorWriter writer, InstanceReport baseReport, InstanceReport genReport )
        {
            bool hasIssue = false;

            // Initial strict checks for performance
            if( baseReport.Footnotes.Count != genReport.Footnotes.Count )
            {
                hasIssue = true;
            }
            else
            {
                for( int i = 0; i < baseReport.Footnotes.Count; i++ )
                {
                    if( !string.Equals( baseReport.Footnotes[ i ].NoteId, genReport.Footnotes[ i ].NoteId ) || ( baseReport.Footnotes[ i ].Note != baseReport.Footnotes[ i ].Note ) )
                    {
                        // If we find a discrepancy, there's no need to continue
                        hasIssue = true;
                        break;
                    }
                }
            }

            if( !hasIssue )
                return;

            // The first checks if the base footnotes are in the generated report
            bool inGen = false, outOfOrder = false;
            int genFootnoteLocation = -1;
            foreach( Footnote iBase in baseReport.Footnotes )
            {
                // 99 and 100 are used for debug
                if( iBase.NoteId > 98 )
                    continue;

                inGen = false;
                outOfOrder = false;
                foreach( Footnote iGen in genReport.Footnotes )
                {
                    if( string.Equals( WHITE_SPACE.Replace( iBase.Note, " " ), WHITE_SPACE.Replace( iGen.Note, " " ) ) )
                    {
                        inGen = true;
                        if( iBase.NoteId != iGen.NoteId )
                        {
                            outOfOrder = true;
                            genFootnoteLocation = iGen.NoteId;
                        }
                        break;
                    }
                }

                // Not found in the generated report
                if( !inGen )
                {
                    writer.WriteError( "<strong>The generated report is missing a footnote:</strong>",
                        "\t[" + iBase.NoteId.ToString() + "] " + iBase.Note );
                }
                // Found, but out of order
                else if( outOfOrder )
                {
                    writer.WriteError( "<strong>The footnote location has changed:</strong>",
                        "\tOriginal Footnote Location: " + iBase.NoteId.ToString(),
                        "\tNew Footnote Location: " + genFootnoteLocation.ToString(),
                        "\tFootnote Text: " + iBase.Note );
                }
            }

            // This loop ensures all generated footnotes were in the main report
            bool inBase = false;
            foreach( Footnote iGen in genReport.Footnotes )
            {
                // 99 and 100 are used for debug
                if( iGen.NoteId > 98 )
                    continue;

                inBase = false;
                foreach( Footnote iBase in baseReport.Footnotes )
                {
                    if( string.Equals( WHITE_SPACE.Replace( iBase.Note, " " ), WHITE_SPACE.Replace( iGen.Note, " " ) ) )
                    {
                        inBase = true;
                        break;
                    }
                }

                // 99 and 100 are used for debug
                if( !inBase )
                {
                    writer.WriteError( "<strong>The generated report created an additional footnote:</strong>",
                        "\t[" + iGen.NoteId.ToString() + "] " + iGen.Note );
                }
            }

            return;
        }

		#endregion

		#region events

		protected event EventHandler OnBuildReportsProcessing = null;
		private void FireBuildReportsProcessing( object sender, EventArgs e )
		{
			if( this.OnBuildReportsProcessing != null )
				this.OnBuildReportsProcessing( sender, e );
		}

		protected event EventHandler OnBuildReportsProcessed = null;
		private void FireBuildReportsProcessed( object sender, EventArgs e )
		{
			if( this.OnBuildReportsProcessed != null )
				this.OnBuildReportsProcessed( sender, e );
		}

		#endregion
	}
}

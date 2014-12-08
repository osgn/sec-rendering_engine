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
using NUnit.Framework;
using XBRLReportBuilder;
using XBRLReportBuilder.Utilities;
using XRB = XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using Aucent.FilingServices.Data;
using System.Net.Cache;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test.Data
{
	[TestFixture]
	public abstract partial  class Test_Abstract
	{
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
		[TestFixtureSetUp]
		public virtual void RunFirst()
		{
			Type t = this.GetType();
			if( runTypesConcurrent.Contains( t ) )
			{
				runTypesConcurrent.Remove( t );
				RB_Tests.RunAllConcurrent( this );
			}
		}

		[TestFixtureTearDown]
		public virtual void RunLast()
		{
		}

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp]
		public virtual void RunBeforeEachTest()
		{
		}

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown]
		public virtual void RunAfterEachTest()
		{
			if( RB_Tests.UpdateResultsFlag )
			{
				this.UpdateResults();
				//RB_Tests.UpdateResultsFlag = false;
			}
		}

		#region PUBLIC

		public bool BuildReports( string instanceFolder, string instanceName, string taxonomyName, string formType, out FilingSummary filingSummary, out string error )
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

			CleanFolder( reportDir );

			XRB.ReportBuilder myReportBuilder = new XRB.ReportBuilder();
			myReportBuilder.ReportFormat = RB_Tests.OutputFormat;
			myReportBuilder.HtmlReportFormat = RB_Tests.HtmlFormat;
			myReportBuilder.RemoteFileCachePolicy = RB_Tests.CachePolicy;

			if( string.IsNullOrEmpty( RB_Tests.ResourcePath ) )
				RulesEngineUtils.SetBaseResourcePath( null );
			else
				RulesEngineUtils.SetBaseResourcePath( RB_Tests.ResourcePath );

			bool status = myReportBuilder.BuildReports( instance, taxonomy, filingSummaryPath, reportDir, formType, out filingSummary, out error );
			this.myFilingSummary = status ? filingSummary : null;

			DateTime endTime = DateTime.Now;
			this.processingTime = ( endTime - startTime );

			return status;
		}

		public static void CleanFolder( string folder )
		{
			if( Directory.Exists( folder ) )
			{
				bool deleted = false;

				try
				{
					Directory.Delete( folder, true );
					deleted = true;
				}
				catch { }

				if( !deleted )
				{
					try
					{
						foreach( string file in Directory.GetFiles( folder ) )
						{
							File.Delete( file );
						}

						deleted = true;
					}
					catch { }
				}
			}

			if( !Directory.Exists( folder ) )
				Directory.CreateDirectory( folder );
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

			return Transform( xmlPath, argList );
		}

		public static string Transform( string xmlPath, XsltArgumentList argList )
		{
			string html = null;

			try
			{
				string transformFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, "InstanceReport.xslt" );

				XslCompiledTransform transform = new XslCompiledTransform();
				transform.Load( transformFile );

				using( MemoryStream ms = new MemoryStream() )
				{
					XmlWriterSettings xSettings = new XmlWriterSettings();
					xSettings.ConformanceLevel = ConformanceLevel.Fragment;
					xSettings.Encoding = Encoding.ASCII;
					xSettings.Indent = true;
					xSettings.OmitXmlDeclaration = true;

					//we REALLY need to set this to HTML
					//xSettings.GetType().GetProperty( "OutputMethod" ).SetValue( xSettings, XmlOutputMethod.Html, null );
		  
					//set internal outputMethod field to HTML output (or else it winds up being AutoDetect)
					//rivet's original hack above doesn't work on mono
					xSettings.GetType().GetField( "outputMethod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( xSettings, XmlOutputMethod.Html );

					using( XmlWriter xWriter = XmlWriter.Create( ms, xSettings ) )
					{
						transform.Transform( xmlPath, argList, xWriter );
					}

					ms.Flush();
					html = Encoding.ASCII.GetString( ms.ToArray() );
				}
			}
			catch( Exception ex )
			{
				html = "<h2>Error <small>generating preview</small></h2>"+
					"<pre>"+ ex.Message +"</pre>";
			}

			return html;
		}

		private static List<Type> runTypesConcurrent = new List<Type>();
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

			if( !RB_Tests.BuildAndVerifyWithoutResults && !Directory.Exists( this.resultsPath ) )
				Assert.Fail( "There are no results to verify againsts." );

			string generatedResultFolder = PathCombine( this.baseDir, folder, "Reports" );
			this.reportsPath = generatedResultFolder;

			string failedResultsFolder = Path.GetDirectoryName( generatedResultFolder );
			failedResultsFolder = Path.Combine( failedResultsFolder, "Failures" );
			CleanFolder( failedResultsFolder );

			FilingSummary fs = BuildFiling( folder, instanceName, taxonomyName );
			using( ErrorWriter writer = new ErrorWriter() )
			{
				VerifyIndividualReports( writer, baselineResultFolder, generatedResultFolder, failedResultsFolder );
				VerifyCharts( writer, baselineResultFolder, generatedResultFolder );

				if( writer.HasErrors )
				{
					writer.Flush();
					writer.SaveResults( failedResultsFolder );

					Console.Write( writer.GetStringBuilder().ToString().Trim() );
					Assert.Fail( writer.GetStringBuilder().ToString().Trim() );
				}
			}

			return fs;
		}

		protected FilingSummary BuildFiling( string accessionNumberIn, string instanceNameIn, string taxonomyNameIn )
		{
			if( !RB_Tests.ConcurrentTestsRunning )
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

				if( RB_Tests.ConcurrentTestsResults.ContainsKey( method.Name ) )
				{
					//RB_Tests.IsConcurrentTestResult = true;
					if( RB_Tests.ConcurrentTestsResults[ method.Name ] != null )
						throw RB_Tests.ConcurrentTestsResults[ method.Name ];
					else
						return null;
				}
			}

			string error;
			FilingSummary filingSummary = null;
			string folderPath = Path.Combine( baseDir, accessionNumberIn );
			bool buildSuccess = BuildReports( folderPath, instanceNameIn, taxonomyNameIn, "10-Q", out filingSummary, out error );

			Assert.IsTrue( buildSuccess, "Failed to generate reports: " + error );

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

		private void UpdateResults()
		{
			if( !Directory.Exists( this.resultsPath ) )
				return;

			string script1 = @"CALL ""C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"" x86";
			string script2 = @"tf checkout """ + this.resultsPath + @"\R*.xml""";
			string script3 = @"xcopy /RYU """ + this.reportsPath + @"\R*.xml"" """ + this.resultsPath + @"\""";

			string tmp = Path.GetTempFileName();
			File.Move( tmp, tmp + ".bat" );
			tmp += ".bat";

			try
			{
				File.WriteAllLines( tmp, new string[] { script1, script2, script3 } );

				ProcessStartInfo psi = new ProcessStartInfo( tmp );
				Process p = Process.Start( psi );
				p.WaitForExit();
				p.Close();
			}
			finally
			{
				File.Delete( tmp );
			}
		}

		private static void VerifyCharts( ErrorWriter writer, string baselineResultFolder, string generatedResultFolder )
		{
			string[] baseFiles = new string[ 0 ];
			if( Directory.Exists( baselineResultFolder ) )
				baseFiles = Directory.GetFiles( baselineResultFolder, "*.jpg" );

			string[] genFiles = new string[ 0 ];
			if( Directory.Exists( generatedResultFolder ) )
				genFiles = Directory.GetFiles( generatedResultFolder, "*.jpg" );

			if( baseFiles.Length < genFiles.Length )
			{
				writer.StartError( "The new rendering generated too MANY charts:" );
				foreach( string gFile in genFiles )
				{
					int foundAt = Array.IndexOf( baseFiles, gFile );
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
				foreach( string bFile in baseFiles )
				{
					int foundAt = Array.IndexOf( genFiles, bFile );
					if( foundAt == -1 )
					{
						writer.WriteError( "\tRemoved: " + bFile );
					}
				}
				writer.EndError();
			}

			foreach( string baseChart in baseFiles )
			{
				string baseName = Path.GetFileName( baseChart );
				string genChart = Array.Find( baseFiles, file => file.EndsWith( baseName ) );

				FileInfo baseFI = new FileInfo( baseChart );
				FileInfo genFI = new FileInfo( baseChart );

				if( baseFI.Length != genFI.Length )
				{
					writer.WriteError(
						"Charts do not match:",
						"\tExpected length:  " + baseFI.Length,
						"\tGenerated length: " + genFI.Length
					);
				}
			}
		}

		private static readonly Regex WHITE_SPACE = new Regex( @"(&#160;|\s)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture );
		private static void VerifyIndividualReports( ErrorWriter writer, string baselineResultFolder, string generatedResultFolder, string failedResultsFolder )
		{
			string[] baseFiles = new string[0];
			if( Directory.Exists( baselineResultFolder ) )
			{
				baseFiles = Directory.GetFiles( baselineResultFolder, "R*.xml" );
				Array.Sort( baseFiles, natcasecmp );
			}

			string[] genFiles = new string[ 0 ];
			if( Directory.Exists( generatedResultFolder ) )
			{
				genFiles = Directory.GetFiles( generatedResultFolder, "R*.xml" );
				Array.Sort( genFiles, natcasecmp );
			}

			if( baseFiles.Length < genFiles.Length )
			{
				writer.StartError( "The new rendering generated too MANY files:" );
				for( int i = 0; i < genFiles.Length; i++ )
				{
					string gFile = Path.GetFileName( genFiles[ i ] );
					bool found = Array.Exists( baseFiles, bFile => bFile.EndsWith( gFile ) );
					if( !found )
						writer.WriteError( "\tAdded:  " + gFile );
				}
				writer.EndError();
			}
			else if( baseFiles.Length > genFiles.Length )
			{
				writer.StartError( "The new rendering generated too FEW files." );
				for( int i = 0; i < baseFiles.Length; i++ )
				{
					string bFile = Path.GetFileName( baseFiles[ i ] );
					bool found = Array.Exists( genFiles, gFile => gFile.EndsWith( bFile ) );
					if( !found )
						writer.WriteError( "\tAdded:  " + bFile );
				}
				writer.EndError();
			}

			Dictionary<string, int> allFiles = new Dictionary<string, int>();
			foreach( string bFile in baseFiles )
				allFiles[ Path.GetFileName( bFile ) ] = 1;

			foreach( string gFile in genFiles )
				allFiles[ Path.GetFileName( gFile ) ] = 1;

			Dictionary<string, InstanceReport> tryVerifyEmbedded = new Dictionary<string, InstanceReport>();
			foreach( string file in allFiles.Keys )
			{
				if( !R_FILE_REGEX.IsMatch( file ) )
					continue;

				InstanceReport baseReport = null;
				InstanceReport genReport = null;

				try
				{
					string baseReportPath = Path.Combine( baselineResultFolder, file );
					if( File.Exists( baseReportPath ) )
						baseReport = InstanceReport.LoadXml( baseReportPath );

					string genReportPath = Path.Combine( generatedResultFolder, file );
					if( File.Exists( genReportPath ) )
						genReport = InstanceReport.LoadXml( genReportPath );

					if( baseReport == null )
						throw new Exception( "Base report is missing." );

					if( genReport == null )
						throw new Exception( "Generated report is missing." );

					writer.StartReport( file, baseReport, genReport, failedResultsFolder );
					writer.WriteLine( PathCombine( baselineResultFolder, file ) );
					writer.WriteLine( PathCombine( generatedResultFolder, file ) );

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
			if( baseReport.ReportLongName != genReport.ReportLongName )
			{
				writer.WriteError( "Long report name does not match:",
					"\tExpected:  " + WHITE_SPACE.Replace( baseReport.ReportLongName, " " ),
					"\tGenerated: " + WHITE_SPACE.Replace( genReport.ReportLongName, " " ),
					"\t...continuing...",
					string.Empty );
			}

			if( WHITE_SPACE.Replace( baseReport.ReportName, " " ) != WHITE_SPACE.Replace( genReport.ReportName, " " ) )
			{
				writer.WriteError( "Short report name does not match:",
					"\tExpected:  " + WHITE_SPACE.Replace( baseReport.ReportName, " " ),
					"\tGenerated: " + WHITE_SPACE.Replace( genReport.ReportName, " " ),
					"\t...continuing...",
					string.Empty );
			}

			VerifyReportColumns( writer, baseReport, genReport );
			VerifyReportRows( writer, baseReport, genReport );
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
					writer.WriteError( "Base column not found in generated report." );
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
					writer.WriteError( "Label doesn't match base column at index " + bIdx + ":",
						"\tExpected:  " + baseLabel,
						"\tGenerated: " + WHITE_SPACE.Replace( genColumn.Label, " " ) );
					continue;
				}

				genColumn = genReport.Columns.Find( col => WHITE_SPACE.Replace( col.Label, " " ) == baseLabel );
				if( genColumn == null )
				{
					writer.WriteError( "Base column not found in generated report:",
						"\t" + baseLabel,
						"\t***Column Skipped***" );
					continue;
				}

				int gIdx = genReport.Columns.IndexOf( genColumn );
				if( bIdx != gIdx )
				{
					writer.WriteError( "Column moved: " + baseLabel,
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
					writer.StartError( "The new rendering generated too MANY rows." );

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
					writer.StartError( "The new rendering generated too FEW rows." );

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
							"Generated report has too few rows to match Base row:",
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
							"Base row not found in generated report:",
							"\tRow " + bIdx + ": " + baseLabel,
							"\t***Row Skipped***"
						);
						continue;
					}

					genRow = genReport.Rows[ gIdx ];
					if( bIdx != gIdx )
					{
						writer.WriteError(
							"Row moved: " + baseLabel,
							"\tExpected:  " + bIdx.ToString(),
							"\tGenerated: " + gIdx.ToString()
						);
					}
				}

				if( !string.Equals( baseRow.FootnoteIndexer, genRow.FootnoteIndexer ) )
				{
					writer.WriteError(
						"Row footnotes changed: " + baseLabel,
						"\tExpected:  " + baseRow.FootnoteIndexer,
						"\tGenerated: " + genRow.FootnoteIndexer
					);
				}

				decimal baseSum = 0M;
				baseRow.Cells.FindAll( c => c.IsNumeric ).ForEach( c => baseSum += c.NumericAmount );

				decimal genSum = 0M;
				genRow.Cells.FindAll( c => c.IsNumeric ).ForEach( c => genSum += c.NumericAmount );

				bool checkNumericCells = false;
				decimal difference = baseSum - genSum;
				if( difference != 0 )
				{
					checkNumericCells = true;

					if( difference > 0 )
					{
						writer.WriteError(
							"Row sum is SMALLER than expected:",
							"\tRow " + bIdx + ": " + baseLabel,
							"\tExpected:  " + baseSum,
							"\tGenerated: " + genSum
						);
					}
					else
					{
						writer.WriteError(
							"Row sum is LARGER than expected:",
							"\tRow " + bIdx + ": " + baseLabel,
							"\tExpected:  " + baseSum,
							"\tGenerated: " + genSum
						);
					}
				}

				foreach( Cell baseCell in baseRow.Cells )
				{
					if( !checkNumericCells && baseCell.IsNumeric )
						continue;

					Cell genCell = null;
					int cIdx = baseRow.Cells.IndexOf( baseCell );
					if( genRow.Cells.Count > cIdx )
						genCell = genRow.Cells[ cIdx ];

					if( genCell == null )
					{
						writer.WriteError( "Base cell not found in Generated report." );
						continue;
					}

					if( genCell.IsNumeric )
					{
						if( !checkNumericCells )
							continue;
					}
					else
					{
						genCell.NonNumbericText = WHITE_SPACE.Replace( genCell.NonNumbericText, " " );
					}

					if( !baseCell.IsNumeric )
					{
						baseCell.NonNumbericText = WHITE_SPACE.Replace( baseCell.NonNumbericText, " " );
					}


					if( !baseCell.ValueEquals( genCell ) )
					{
						//if( baseCell.IsNumeric )
						//{
						//    genCell = genRow.Cells.Find( c => c.NumericAmount == baseCell.NumericAmount || c.RoundedNumericAmount == baseCell.RoundedNumericAmount );
						//    if( genCell != null )

						//}

						baseCell.FlagID = 1;
						genCell.FlagID = 1;

						writer.WriteError( "Cell value does not match:",
							"\tRow " + bIdx + ", Cell " + cIdx,
							"\tExpected:  " + baseCell.ToString() + "[" + baseCell.FootnoteIndexer + "]",
							"\tGenerated: " + genCell.ToString() + "[" + genCell.FootnoteIndexer + "]" );
					}
					else if( !string.Equals( baseCell.FootnoteIndexer, genCell.FootnoteIndexer ) )
					{
						writer.WriteError( "Cell footnotes changed:",
							"\tRow " + bIdx + ", Cell " + cIdx,
							"\tExpected:  " + baseCell.FootnoteIndexer,
							"\tGenerated: " + genCell.FootnoteIndexer );
					}
				}

				List<Cell> baseEmbedCells = baseRow.Cells.FindAll( c => c.HasEmbeddedReport && c.EmbeddedReport != null && c.EmbeddedReport.InstanceReport != null );
				List<Cell> genEmbedCells = genRow.Cells.FindAll( c => c.HasEmbeddedReport && c.EmbeddedReport != null && c.EmbeddedReport.InstanceReport != null );
				if( baseEmbedCells.Count != genEmbedCells.Count )
				{
					difference = Math.Abs( baseEmbedCells.Count - genEmbedCells.Count );
					if( baseEmbedCells.Count > genEmbedCells.Count )
					{
						writer.WriteError( "Row has MISSING embedded reports:",
							"\tExpected:  " + baseEmbedCells.Count,
							"\tGenerated: " + genEmbedCells.Count );
					}
					else
					{
						writer.WriteError( "Row has EXTRA embedded reports:",
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
						writer.WriteError( "Embedded report not found in generated report:",
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

		#endregion
	}
}
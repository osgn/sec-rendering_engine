using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Aucent.FilingServices.Data;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;
using NUnit.Framework;
using XBRLReportBuilder;
using XRB = XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{
		[TestFixture]
		public class Fall2011Defects : Test_Abstract
		{
			private string _relativeRoot = @"TestFiles\_BaselineFall2011";
			protected override string relativeRoot
			{
				get { return this._relativeRoot; }
			}

			[TestFixtureSetUp]
			public override void RunFirst()
			{
				Type t = this.GetType();
				if( runTypesConcurrent.Contains( t ) )
				{
					runTypesConcurrent.Remove( t );
					RB_Tests.RunAllConcurrent( this );
				}
			}

			/// <summary>Tears down test values after each test is run </summary>
			[TearDown]
			public override void RunAfterEachTest()
			{
				if( Test_Abstract.UpdateResultsFlag )
				{
					this.UpdateResults();
					//RB_Tests.UpdateResultsFlag = false;
				}
			}

			[Test]
			public void Test_24609_ex1_20101231()
			{
				const string accessionNumber = "24609_ex1_20101231";
				const string instanceName = "ex1-20101231";
				const string taxonomyName = "ex1-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			[Ignore( "This defect is deferred by the SEC and will be promoted to feature request later.")]
			public void Test_25490_f_20100331()
			{
				const string accessionNumber = "25490_f_20100331";
				const string instanceName = "f-20100331";
				const string taxonomyName = "f-20100331";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			[Ignore]
			//[Description( "Incorrect abstract is displayed" )]
			public void Test_26759_gm_20110331()
			{
				const string accessionNumber = "26759_gm-20110331";
				const string instanceName = "gm-20110331";
				const string taxonomyName = "gm-20110331";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 007" )]
			public void Test_29018_gd001sec0007()
			{
				const string accessionNumber = "29018_gd001sec0007";
				const string instanceName = "edgar-20101231";
				const string taxonomyName = "edgar-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 127" )]
			public void Test_29023_gd001SEC0127()
			{
				const string accessionNumber = "29023_gd001SEC0127";
				const string instanceName = "xyzCorp-20111231";
				const string taxonomyName = "xyzCorp-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 137" )]
			public void Test_29025_gd001SEC0137()
			{
				const string accessionNumber = "29025_gd001SEC0137";
				const string instanceName = "xyzCorp-20111231";
				const string taxonomyName = "xyzCorp-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 115" )]
			public void Test_29027_gd001SEC0115()
			{
				const string accessionNumber = "29027_gd001SEC0115";
				const string instanceName = "e60403007gd-20111231";
				const string taxonomyName = "e60403007gd-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description("SEC 125")]
			public void Test_29028_gd001SEC0125()
			{
				const string accessionNumber = "29028_gd001SEC0125";
				const string instanceName = "inf-20111231";
				const string taxonomyName = "inf-20111231";

				this.BuildAndVerifyRecursive(accessionNumber, instanceName, taxonomyName);
			}

			[Test]
			[Ignore( "Deferred to feature release" )]
			//[Description( "SEC DE1719" )]
			public void Test_29032_gd001de1719()
			{
				const string accessionNumber = "29032_gd001de1719";
				const string instanceName = "rnd54-20091231";
				const string taxonomyName = "rnd54-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 128" )]
			public void Test_29033_gd001SEC0128()
			{
				const string accessionNumber = "29033_gd001SEC0128";
				const string instanceName = "nws-20101103";
				const string taxonomyName = "nws-20101103";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			//[Description( "SEC 130" )]
			public void Test_29034_ng001decimals3()
			{
				const string accessionNumber = "29034_ng001decimals3";
				const string instanceName = "xyzCorp-20111231";
				const string taxonomyName = "xyzCorp-20111231";

				FilingSummary fs = this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				LogItem item = fs.Logs.Find( log => string.Equals( log.Item, "Element xyzCorp_Afact had a mix of decimals attribute values: -9 -6 -3." ) );
				Assert.AreNotEqual( null, item, "The previous log message should be in the FilingSummary" );
			}

			[Test]
			//[Description( "SEC 140" )]
			public void Test_29039()
			{
				Test_Abstract.OutputFormat = ( Test_Abstract.OutputFormat & ReportFormat.Xml ) | ReportFormat.Html;
				Test_Abstract.HtmlFormat = HtmlReportFormat.Complete;

				const string accessionNumber = "29039";

				string reportsFolder = PathCombine( this.baseDir, accessionNumber, "Reports" );
				CleanAndPrepareFolder( reportsFolder );

				string resultsFolder = PathCombine( this.baseDir, accessionNumber, "Results" );

				FilingSummary fs = new FilingSummary();
				foreach( string from in Directory.GetFiles( resultsFolder, "R1_*.xml" ) )
				{
					string file = Path.GetFileName( from );
					string to = PathCombine( reportsFolder, file );
					FileUtilities.Copy( from, to );

					ReportHeader header = new ReportHeader( file, file );
					fs.MyReports.Add( header );
				}


				XRB.ReportBuilder rb = new XRB.ReportBuilder();
				rb.ReportFormat = ReportFormat.Html;
				rb.HtmlReportFormat = HtmlReportFormat.Complete;
				rb.GetType().GetField( "currentFilingSummary", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( rb, fs );
				rb.GetType().GetField( "currentReportDirectory", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( rb, reportsFolder );
				rb.GetType().GetMethod( "GenerateHtmlFiles", BindingFlags.NonPublic | BindingFlags.Instance ).Invoke( rb, null );

				foreach( string file in Directory.GetFiles( reportsFolder, "R1_*.htm" ) )
				{
					string html = File.ReadAllText( file );
					int start = html.IndexOf( "<body" );
					int end = html.IndexOf( "</body" );
					end = html.IndexOf( '>', end ) + 1;

					html = html.Substring( start, end - start );
					html = html.Replace( "<br>", "<br />" );

					XmlDocument xDoc = new XmlDocument();
					xDoc.LoadXml( html );
					XmlNodeList headerRows = xDoc.SelectNodes( "/body/table/tr[ th ]" );
					Assert.AreEqual( 2, headerRows.Count, "The transform must generate 2 headers rows for the right display." );
				}
			}

			[Test]
			public void Test_29563_caci_20111111()
			{
				const string accessionNumber = "29563_caci_20111111";
				const string instanceName = "caci-20111111";
				const string taxonomyName = "caci-20111111";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_29711_0000844965_11_000110()
			{
				const string accessionNumber = "29711_0000844965_11_000110";
				const string instanceName = "tti-20110630";
				const string taxonomyName = "tti-20110630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

            /// <summary>
            /// Bug: 29026
            /// SEC Bug: 134
            /// Description: Fact values that are QNames are not sensitive to preferred labels
            /// Testing method: Rendering comparison
            /// </summary>
            [Test]
            //[Description("SEC 134")]
			public void Test_29026_gd001SEC0134()
            {
				const string accessionNumber = "29026_gd001SEC0134";
                const string instanceName = "xyzCorp-20111231";
                const string taxonomyName = "xyzCorp-20111231";

                this.BuildAndVerifyRecursive(accessionNumber, instanceName, taxonomyName);
            }


            /// <summary>
            /// Bug: 29029
            /// SEC Bug: 129
            /// Description: Rendering non sensitive to locale/regional settings
            /// Testing Method: Rendering comparison
            /// </summary>
            [Test]
            //[Description( "SEC 129" )]
			public void Test_29029_gd001SEC0129()
            {
				lock( this )
				{
					const string accessionNumber = "29029_gd001SEC0129";
					const string instanceName = "xyzCorp-20111231";
					const string taxonomyName = "xyzCorp-20111231";

					// SEC suggested settings for testing this defect
					CultureInfo initialSetting = CultureInfo.CurrentCulture;
					CultureInfo testSetting = new CultureInfo( "en-US" );

					testSetting.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
					testSetting.DateTimeFormat.DateSeparator = "-";
					testSetting.DateTimeFormat.LongDatePattern = "dddd d MMMM yyyy";
					testSetting.NumberFormat.NumberDecimalSeparator = ",";
					testSetting.NumberFormat.NumberGroupSeparator = " ";

					Thread.CurrentThread.CurrentCulture = testSetting;


					this.OnBuildReportsProcessing += new EventHandler( Test_29029_1_Rule_Change );
					this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );

					// So as not to pollute the thread
					Thread.CurrentThread.CurrentCulture = initialSetting;
				}
            }

            void Test_29029_1_Rule_Change( object sender, EventArgs e )
            {
				#pragma warning disable 0219
                XRB.ReportBuilder rb = sender as XRB.ReportBuilder;
				#pragma warning restore 0219
                // Only turn this on if not running multiple tests
                // rb.BuilderRules.MyRules[ RulesEngineUtils.DISPLAY_US_DATE_FORMAT ].Enabled = false;
            }

            /// <summary>
            /// Bug: 29030
            /// SEC Bug: 131
            /// Description: Multiple embedding command in a single fact should throw a warning
            /// Testing Method: Render, then check filing summary for appropriate entries
            /// </summary>
            [Test]
            //[Description( "SEC 131" )]
            public void Test_29030_SEC0131()
            {
                const string accessionNumber = "29030_SEC0131";
                const string instanceName = "mon-20101231";
                const string taxonomyName = "mon-20101231";
                const string error1 = "Multiple embeddings in Element \"Investment Holdings Details Text Block\" with UnitID \"No UnitID Specified\" and contextID \"no-dim-D\". Only the first embed command will be used.";

                FilingSummary logCheckSummary = this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				LogItem item = logCheckSummary.Logs.Find( log => log.Item.Contains( error1 ) );
                Assert.AreNotEqual( null, item, "An expected warning was not found in the filing summary.  The warning not found is:" + error1 );
            }

            /// <summary>
            /// Bug: 29030
            /// SEC Bug: 131
            /// Description: Multiple embedding command in a single fact should throw a warning
            /// Testing Method: Render, then check filing summary for appropriate entries
            /// </summary>
            [Test]
            //[Description( "SEC 131" )]
            public void Test_29030_multiple()
            {
                const string accessionNumber = "29030_multiple";
                const string instanceName = "eggplant-20090701";
                const string taxonomyName = "eggplant-20090701";
                const string warning1 = "Multiple embeddings in Element \"Risk/Return Detail [Table]\" with UnitID \"No UnitID Specified\" and contextID \"_\". Only the first embed command will be used.";
                const string warning2 = "Multiple embeddings in Element \"Shareholder Fees [Table]\" with UnitID \"No UnitID Specified\" and contextID \"Investors_S123456789\". Only the first embed command will be used.";
                const string warning3 = "Multiple embeddings in Element \"Annual Fund Operating Expenses [Table]\" with UnitID \"No UnitID Specified\" and contextID \"Investors_S123456789\". Only the first embed command will be used.";
                const string warning4 = "Multiple embeddings in Element \"Expense Example with Redemption [Table]\" with UnitID \"No UnitID Specified\" and contextID \"Investors_S123456789\". Only the first embed command will be used.";

                FilingSummary logCheckSummary = this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );


                bool logCheckExists = true;

                logCheckExists &= logCheckSummary.Logs.Exists( iLog => iLog.Item.Contains( warning1 ) );
                logCheckExists &= logCheckSummary.Logs.Exists( iLog => iLog.Item.Contains( warning2 ) );
                logCheckExists &= logCheckSummary.Logs.Exists( iLog => iLog.Item.Contains( warning3 ) );
                logCheckExists &= logCheckSummary.Logs.Exists( iLog => iLog.Item.Contains( warning4 ) );
                if( !logCheckExists )
                {
                    throw new Exception( "This filing has four embed commands that should throw warnings.  The warnings were not found in the filing summary." );
                }

            }

			[Test]
			//[Description( "SEC 134" )]
			public void Test_30007_deere()
			{
				const string accessionNumber = "30007_deere";
				const string instanceName = "de-20110731";
				const string taxonomyName = "de-20110731";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_30530_wbc_20110630()
			{
				const string accessionNumber = "30530_wbc-20110630";
				const string instanceName = "wbc-20110630";
				const string taxonomyName = "wbc-20110630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_30553_snda_20101231()
			{
				const string accessionNumber = "30553_snda-20101231";
				const string instanceName = "snda-20101231";
				const string taxonomyName = "snda-20101231";

				FilingSummary fs = this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.AreEqual( 6, fs.Logs.Count, "This filing should not generate any logs regarding embed commands and/or tildas (~)." );

			}
		}
	}
}

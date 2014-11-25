//=============================================================================
// TestReportBuilder (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the main unit test class that generates reports from 100+ filings      
// and compares the generated results against the prepared expected results.
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using NUnit.Framework;
using XBRLReportBuilder;
using XRB = XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class TestReportBuilderSOI : Test_Abstract
		{
			private string _relativeRoot = @"TestFiles\_Baseline_SOI";
			protected override string relativeRoot
			{
				get { return this._relativeRoot; }
			}

            [TestFixtureSetUp]
            public override void RunFirst()
            {
                Type t = this.GetType();
                if (runTypesConcurrent.Contains(t))
                {
                    runTypesConcurrent.Remove(t);
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
			public void TestParseSimpleInstruction()
			{
				String instruction = @"<div style=""display:none;"">~ http://r/role/RiskReturnDetail column period compact * row dei_LegalEntityAxis compact * row rr_ProspectusShareClassAxis compact * row primary compact * ~</div>";
				EmbedReport thisReport = EmbedReport.LoadAndParse( instruction );
				Assert.AreEqual( 1, thisReport.ColumnIterators.Length );
				CommandIterator thisIterator = thisReport.ColumnIterators[ 0 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsTrue( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );

				Assert.AreEqual( 3, thisReport.RowIterators.Length );
				thisIterator = thisReport.RowIterators[ 0 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );
				Assert.IsTrue( thisIterator.AxisName == "dei_LegalEntityAxis" );
				thisIterator = thisReport.RowIterators[ 1 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );
				Assert.IsTrue( thisIterator.AxisName == "rr_ProspectusShareClassAxis" );
				thisIterator = thisReport.RowIterators[ 2 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );
				Assert.IsTrue( thisIterator.Filter == "*" );

				Assert.AreEqual( @"http://r/role/RiskReturnDetail", thisReport.Role );
				Assert.IsFalse( thisReport.IsTransposed );
			}

			[Test]
			public void TestParseComplexInstruction()
			{
				String instruction = @"~ http://europa.eu/role/R4041 column primary compact * row us-gaap_DerivativeByNatureAxis grouped * row us-gaap_InvestmentSecondaryCategorizationAxis grouped * row invest_InvestmentAxis unitcell * ~";
				EmbedReport thisReport = EmbedReport.LoadAndParse( instruction );
				Assert.AreEqual( 2, thisReport.ColumnIterators.Length );

				CommandIterator thisIterator = thisReport.ColumnIterators[ 0 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsTrue( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Filter == "*" );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );

				thisIterator = thisReport.ColumnIterators[ 1 ] as CommandIterator;
				Assert.IsTrue( thisIterator.IsCompact );
				Assert.IsTrue( thisIterator.IsPrimary );
				Assert.IsTrue( thisIterator.Filter == "*" );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Compact );

				Assert.AreEqual( 3, thisReport.RowIterators.Length );
				thisIterator = thisReport.RowIterators[ 0 ] as CommandIterator;
				Assert.IsFalse( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Grouped );
				Assert.IsTrue( thisIterator.AxisName == "us-gaap_DerivativeByNatureAxis" );
				thisIterator = thisReport.RowIterators[ 1 ] as CommandIterator;
				Assert.IsFalse( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.Grouped );
				Assert.IsTrue( thisIterator.AxisName == "us-gaap_InvestmentSecondaryCategorizationAxis" );
				thisIterator = thisReport.RowIterators[ 2 ] as CommandIterator;
				Assert.IsFalse( thisIterator.IsCompact );
				Assert.IsFalse( thisIterator.IsPeriod );
				Assert.IsTrue( thisIterator.Style == CommandIterator.StyleType.UnitCell );
				Assert.IsTrue( thisIterator.AxisName == "invest_InvestmentAxis" );

				Assert.AreEqual( @"http://europa.eu/role/R4041", thisReport.Role );
				Assert.IsFalse( thisReport.IsTransposed );
			}

			[Test]
			public void TestBuild_akce_20101231()
			{
				this.BuildAndVerifyRecursive( "akce", "akce-20101231", "akce-20101231" );
			}

			[Test]
			public void TestBuild_baht_20101231()
			{
				const string folderInstanceTaxonomy = "baht-20101231";
				this.BuildAndVerifyRecursive( folderInstanceTaxonomy );
			}

			[Test]
			public void TestBuild_bani_20101231()
			{
				this.BuildAndVerifyRecursive( "bani", "bani-20101231", "bani-20101231" );
			}

			[Test]
			public void TestBuild_bu_20101231()
			{
				this.BuildAndVerifyRecursive( "bu", "bu-20101231", "bu-20101231" );
			}

			[Test]
			public void TestBuild_carolin_20101231()
			{
				this.BuildAndVerifyRecursive( "carolin", "carolin-20101231", "carolin-20101231" );
			}

			[Test]
			public void TestBuild_ducat_20101231()
			{
				this.BuildAndVerifyRecursive( "ducat", "ducat-20101231", "ducat-20101231" );
			}

			[Test]
			public void TestBuild_escudo_20101231()
			{
				const string folderInstanceTaxonomy = "escudo-20101231";
				this.BuildAndVerifyRecursive( folderInstanceTaxonomy );
			}

			[Test]
			public void TestBuild_escudo_20101231_grouped()
			{
				const string folder = "escudo-20101231_grouped";
				const string instance = "escudo-20101231";
				const string taxonomy = "escudo-20101231";
				this.BuildAndVerifyRecursive( folder, instance, taxonomy );
			}

			[Test]
			public void TestBuild_escudo_20101231_unitcell()
			{
				const string folder = "escudo-20101231_unitcell";
				const string instance = "escudo-20101231";
				const string taxonomy = "escudo-20101231";
				this.BuildAndVerifyRecursive( folder, instance, taxonomy );
			}

			[Test]
			public void TestBuild_euro_20101231()
			{
				this.BuildAndVerifyRecursive( "euro", "euro-20101231", "euro-20101231" );
			}

			[Test]
			public void TestBuild_Europa_Single_Currency()
			{
				const string accessionNumber = "Europa_Single_Currency";
				const string instanceName = "europa-20091031";
				const string taxonomyName = "europa-20091031";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_Europa_Multi_Currencies()
			{
				const string accessionNumber = "Europa_Multi_Currencies";
				const string instanceName = "europa-20091031";
				const string taxonomyName = "europa-20091031";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_florin_20101231()
			{
				this.BuildAndVerifyRecursive( "florin", "florin-20101231", "florin-20101231" );
			}

			[Test]
			public void TestBuild_groat_20101231()
			{
				this.BuildAndVerifyRecursive( "groat", "groat-20101231", "groat-20101231" );
			}

			[Test]
			[Explicit]
			public void TestBuild_halbag_20101231()
			{
				this.BuildAndVerifyRecursive( "halbag", "halbag-20101231", "halbag-20101231" );
			}

			[Test]
			public void TestBuild_kopek_20101231()
			{
				this.BuildAndVerifyRecursive( "kopek", "kopek-20101231", "kopek-20101231" );
			}

			[Test]
			public void TestBuild_livre_20101231()
			{
				this.BuildAndVerifyRecursive( "livre", "livre-20101231", "livre-20101231" );
			}

			[Test]
			public void TestBuild_mon_20101231()
			{
				this.BuildAndVerifyRecursive( "mon", "mon-20101231", "mon-20101231" );
			}

			[Test]
			public void TestBuild_noble_20101231()
			{
				this.BuildAndVerifyRecursive( "noble", "noble-20101231", "noble-20101231" );
			}

			[Test]
			public void TestBuild_oban_20101231()
			{
				this.BuildAndVerifyRecursive( "oban", "oban-20101231", "oban-20101231" );
			}

			[Test]
			public void TestBuild_pai_20101231()
			{
				this.BuildAndVerifyRecursive( "pai", "pai-20101231", "pai-20101231" );
			}

			[Test]
			public void TestBuild_quattrini_20101231()
			{
				this.BuildAndVerifyRecursive( "quattrini", "quattrini-20101231", "quattrini-20101231" );
			}

			[Test]
			public void TestBuild_salung_20101231()
			{
				this.BuildAndVerifyRecursive( "salung", "salung-20101231", "salung-20101231" );
			}

			[Test]
			public void TestBuild_sik_20101231()
			{
				this.BuildAndVerifyRecursive( "sik", "sik-20101231", "sik-20101231" );
			}

			[Test]
			public void TestBuild_tamlung_20101231()
			{
				this.BuildAndVerifyRecursive( "tamlung", "tamlung-20101231", "tamlung-20101231" );
			}

			[Test]
			public void TestBuild_thaler_20101231()
			{
				this.BuildAndVerifyRecursive( "thaler", "thaler-20101231", "thaler-20101231" );
			}

			[Test]
			public void TestBuild_Walter_1()
			{
				const string accessionNumber = "Walter_SOI_Example_1";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_Walter_2()
			{
				const string accessionNumber = "Walter_SOI_Example_2";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_Walter_3()
			{
				const string accessionNumber = "Walter_SOI_Example_3";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_yen_20101231()
			{
				this.BuildAndVerifyRecursive( "yen", "yen-20101231", "yen-20101231" );
			}

			[Test]
			public void TestBuild_yoda_20101231()
			{
				const string folderInstanceTaxonomy = "yoda-20101231";
				this.BuildAndVerifyRecursive( folderInstanceTaxonomy );
			}

			[Test]
			public void TestBuild_zakim_20101231()
			{
				const string folderInstanceTaxonomy = "zakim-20101231";
				this.BuildAndVerifyRecursive( folderInstanceTaxonomy );
			}

		}
	}
}

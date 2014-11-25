//=============================================================================
// TestReportBuilder (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the main unit test class that generates reports from 100+ filings      
// and compares the generated results against the prepared expected results.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using XRB = XBRLReportBuilder;
using XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class TestReportBuilderFinancial : Test_Abstract
		{
			private string _relativeRoot = @"TestFiles\_BaselineFinancial";
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
                    RB_Tests.RunAllConcurrent(this);
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
			public void Test_Extra_EquityLines_trow_20091231()
			{
				this.BuildAndVerifyRecursive( "trow-20091231" );
			}

			[Test]
			public void Test_L4_DFN_mat_20100630()
			{
				this.BuildAndVerifyRecursive( "mat-20100630" );
			}

			[Test]
			public void Test_L4_DFN_isrg_20100630()
			{
				this.BuildAndVerifyRecursive( "isrg-20100630" );
			}

			[Test]
			public void Test_L4_DFN_yhoo_20100630()
			{
				this.BuildAndVerifyRecursive( "yhoo-20100630" );
			}

			[Test]
			public void Test_MultiCurrencies_1_example_20091031()
			{
				this.BuildAndVerifyRecursive( "example-20091031" );
			}

			[Test]
			public void Test_MultiCurrencies_2_dummy_20091231()
			{
				this.BuildAndVerifyRecursive( "dummy-20091231" );
			}

			[Test]
			public void Test_MultiCurrencies_3_ace_20090930()
			{
				this.BuildAndVerifyRecursive( "ace-20090930" );
			}

			[Test]
			public void Test_MultiCurrencies_USD_CNY()
			{
				this.BuildAndVerifyRecursive( "_MultiCurrencies_20100625", "edgar-20101231", "edgar-20101231" );
			}

			[Test]
			public void TestBuild_ivz_20100630()
			{
				this.BuildAndVerifyRecursive( "ivz-20100630" );
			}

			[Test]
			public void TestBuild_abc_20100101()
			{
				this.BuildAndVerifyRecursive( "abc-20100101" );
			}

			[Test]
			public void TestBuild_large_20091231()
			{
				this.BuildAndVerifyRecursive( "large-20091231" );
			}

			[Test]
			public void TestBuild_aaa_20100310()
			{
				string instDocFolderName = @"aaa-20100310";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_ace_20100331()
			{
				string instDocFolderName = @"ace-20100331";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_acn_20100531()
			{
				string instDocFolderName = @"acn-20100531";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_adbe_20100305()
			{
				string instDocFolderName = @"adbe-20100305";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_aep_20090630()
			{
				string instDocFolderName = @"aep-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_aiz_20090630()
			{
				string instDocFolderName = @"aiz-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_axp_20090630()
			{
				string instDocFolderName = @"axp-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_bcr_20100630()
			{
				string instDocFolderName = @"bcr-20100630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_bidu_20091231()
			{
				this.BuildAndVerifyRecursive( "bidu-20091231" );
			}

			[Test]
			public void TestBuild_bni_20090930()
			{
				string instDocFolderName = @"bni-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_bym_20100630()
			{
				this.BuildAndVerifyRecursive( "bym-20100630" );
			}

			[Test]
			public void TestBuild_cik000093118_20090630()
			{
				string instDocFolderName = @"cik000093118-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_dov_20090930()
			{
				string instDocFolderName = @"dov-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_emc_20090630()
			{
				string instDocFolderName = @"emc-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_esrx_20090630()
			{
				string instDocFolderName = @"esrx-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_euro_20091231_DE1506()
			{
				string instDocFolderName = @"euro-20091231";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_exc_20090930()
			{
				string instDocFolderName = @"exc-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_expd_20090630()
			{
				string instDocFolderName = @"expd-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_f_20090630()
			{
				string instDocFolderName = @"f-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_fcx_20090630()
			{
				string instDocFolderName = @"fcx-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_ge_20090630()
			{
				string instDocFolderName = @"ge-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_ggp_20090630()
			{
				string instDocFolderName = @"ggp-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_gme_20090801()
			{
				string instDocFolderName = @"gme-20090801";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_gnw_20090630()
			{
				string instDocFolderName = @"gnw-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_grmn_20090627()
			{
				string instDocFolderName = @"grmn-20090627";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_gs_20090626()
			{
				string instDocFolderName = @"gs-20090626";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_hig_20090630()
			{
				string instDocFolderName = @"hig-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_hma_20090930_9Mnths_Adj()
			{
				string instDocFolderName = @"hma-20090930_9Mnths_Adj";
				this.BuildAndVerifyRecursive( instDocFolderName, "hma-20090930", "hma-20090930" );
			}

			[Test]
			public void TestBuild_hma_20091111_12Mnths_Adj()
			{
				string instDocFolderName = @"hma-20091111_12Mnths_Adj";
				this.BuildAndVerifyRecursive( instDocFolderName, "hma2-20091111", "hma2-20091111" );
			}

			[Test]
			public void TestBuild_hma_20091111_12Mnths_MultipleAdj()
			{
				string instDocFolderName = @"hma-20091111_12Mnths_MultipleAdj";
				this.BuildAndVerifyRecursive( instDocFolderName, "hma3-20091111", "hma3-20091111" );
			}

			[Test]
			public void TestBuild_hp_20090630()
			{
				string instDocFolderName = @"hp-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_hsy_20090705()
			{
				string instDocFolderName = @"hsy-20090705";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_jci_20090630()
			{
				string instDocFolderName = @"jci-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_lcapa_20090630()
			{
				string instDocFolderName = @"lcapa-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_lpl_20091231()
			{
				string instDocFolderName = @"lpl-20091231";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_luk_20090630()
			{
				string instDocFolderName = @"luk-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_mar_20090911()
			{
				string instDocFolderName = @"mar-20090911";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_mitsy_20100331()
			{
				this.BuildAndVerifyRecursive( "mitsy-20100331" );
			}

			[Test]
			public void TestBuild_mo_20090630()
			{
				string instDocFolderName = @"mo-20090630";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_nflx_20090930()
			{
				string instDocFolderName = @"nflx-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_quick_20100501_DE1361()
			{
				string instDocFolderName = @"quick-20100501";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_rrd_20100331()
			{
				string instDocFolderName = @"rrd-20100331";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_swy_20100619()
			{
				string instDocFolderName = @"swy-20100619";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_test_20100503_DE1370()
			{
				string instDocFolderName = @"test-20100503";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_trow_20090930()
			{
				string instDocFolderName = @"trow-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_trow_20100331()
			{
				string instDocFolderName = @"trow-20100331";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_unp_20090930()
			{
				string instDocFolderName = @"unp-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}

			[Test]
			public void TestBuild_utx_20090930()
			{
				string instDocFolderName = @"utx-20090930";
				this.BuildAndVerifyRecursive( instDocFolderName );
			}
		}

	}
}
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class TestEquity : Test_Abstract
		{
			private string _relativeRoot = @"TestFiles\_BaselineEquity";
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

			[Test, Ignore]
			public void Test_adm_20100630()
			{
				const string accessionNumber = "23134_adm";
				const string instanceName = "adm-20100630";
				const string taxonomyName = "adm-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_ago_20100630()
			{
				const string folderInstanceTaxonomy = "ago-20100630";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_apricot_20111231()
			{
				this.BuildAndVerifyRecursive( "apricot", "apricot-20111231", "apricot-20111231" );
			}

			[Test]
			public void Test_artg_20091231()
			{
				const string folderInstanceTaxonomy = "artg-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_banana_20111231()
			{
				this.BuildAndVerifyRecursive( "banana", "banana-20111231", "banana-20111231" );
			}

			[Test]
			public void Test_bby_20100529()
			{
				const string folderInstanceTaxonomy = "bby-20100529";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_bjs_20091231()
			{
				const string folderInstanceTaxonomy = "bjs-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test, Ignore]
			public void Test_black_20100630()
			{
				const string accessionNumber = "23134_black-20100630";
				const string instanceName = "black-20100630";
				const string taxonomyName = "black-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_brcm_20091231()
			{
				const string folderInstanceTaxonomy = "brcm-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_c_20100630()
			{
				const string folderInstanceTaxonomy = "c-20100630";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_cherry_20111231()
			{
				this.BuildAndVerifyRecursive( "cherry", "cherry-20111231", "cherry-20111231" );
			}

			[Test]
			public void Test_csc_20100702()
			{
				const string folderInstanceTaxonomy = "csc-20100702";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_dow_20100630()
			{
				const string folderInstanceTaxonomy = "dow-20100630";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_esrx_20100331()
			{
				const string folderInstanceTaxonomy = "esrx-20100331";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_farm_20101231()
			{
				const string accessionNumber = "23134_farm-20101231";
				const string instanceName = "farm-20101231";
				const string taxonomyName = "farm-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_farm1_20101231()
			{
				const string accessionNumber = "23134_farm1-20101231";
				const string instanceName = "farm1-20101231";
				const string taxonomyName = "farm1-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test, Ignore]
			public void Test_farm2_20101231()
			{
				const string accessionNumber = "23134_farm2-20101231";
				const string instanceName = "farm2-20101231";
				const string taxonomyName = "farm2-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_fedex1_20100531()
			{
				const string accessionNumber = "23134_fedex1-20100531";
				const string instanceName = "fedex1-20100531";
				const string taxonomyName = "fedex1-20100531";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_fedex2_20100531()
			{
				const string accessionNumber = "23134_fedex2-20100531";
				const string instanceName = "fedex2-20100531";
				const string taxonomyName = "fedex2-20100531";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_fedex3_20100531()
			{
				const string accessionNumber = "23134_fedex3-20100531";
				const string instanceName = "fedex3-20100531";
				const string taxonomyName = "fedex3-20100531";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_fedex4_20100531()
			{
				const string accessionNumber = "23134_fedex4-20100531";
				const string instanceName = "fedex4-20100531";
				const string taxonomyName = "fedex4-20100531";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_fedex5_20100531()
			{
				const string accessionNumber = "23134_fedex5-20100531";
				const string instanceName = "fedex5-20100531";
				const string taxonomyName = "fedex5-20100531";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_Ford_20100331()
			{
				this.BuildAndVerifyRecursive( "Ford", "f-20100331", "f-20100331" );
			}

			[Test]
			public void Test_genz_20091231()
			{
				const string folderInstanceTaxonomy = "genz-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_gr_20091231()
			{
				const string folderInstanceTaxonomy = "gr-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_GreenMountain_20100925()
			{
				this.BuildAndVerifyRecursive( "GreenMountain", "gmcr-20100925", "gmcr-20100925" );
			}

			[Test]
			public void Test_ipcc_20100331()
			{
				const string folderInstanceTaxonomy = "ipcc-20100331";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_jwn_20100130()
			{
				const string folderInstanceTaxonomy = "jwn-20100130";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_KB1_20091231()
			{
				const string accessionNumber = "23134_KB1-20091231";
				const string instanceName = "KB1-20091231";
				const string taxonomyName = "KB1-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_Loews()
			{
				this.BuildAndVerifyRecursive( "Loews", "l-20091231", "l-20091231" );
			}

			[Test]
			public void Test_Loews_2009()
			{
				this.BuildAndVerifyRecursive( "Loews_2009", "l-20091231", "l-20091231" );
			}

			[Test]
			public void Test_luk_20100331()
			{
				const string folderInstanceTaxonomy = "luk-20100331";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_ms_20100331()
			{
				const string folderInstanceTaxonomy = "ms-20100331";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_nty_20100630()
			{
				const string folderInstanceTaxonomy = "nty-20100630";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_park()
			{
				const string accessionNumber = "23134_park";
				const string instanceName = "Park-20101231";
				const string taxonomyName = "Park-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_she1_20091231()
			{
				const string folderInstanceTaxonomy = "she1-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_she2_20091231()
			{
				const string folderInstanceTaxonomy = "she2-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_she3_20091231()
			{
				const string folderInstanceTaxonomy = "she3-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_swn_20100630()
			{
				const string folderInstanceTaxonomy = "swn-20100630";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_tyc_20100625()
			{
				const string folderInstanceTaxonomy = "tyc-20100625";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_urs_20100702()
			{
				const string folderInstanceTaxonomy = "urs-20100702";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_vmi_20100626()
			{
				const string folderInstanceTaxonomy = "vmi-20100626";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_wec_20091231()
			{
				const string folderInstanceTaxonomy = "wec-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

			[Test]
			public void Test_xom_20091231()
			{
				const string folderInstanceTaxonomy = "xom-20091231";
				this.BuildAndVerifyRecursive(folderInstanceTaxonomy );
			}

		}
	}
}
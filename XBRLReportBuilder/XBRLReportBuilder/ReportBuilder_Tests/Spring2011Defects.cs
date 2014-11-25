using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Aucent.MAX.AXE.Common.Utilities;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class Spring2011Defects : Test_Abstract
		{
			private string _relativeRoot = @"TestFiles\_BaselineSpring2011";
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
			public void Test_18585_lm()
			{
				const string accessionNumber = "18585_lm";
				const string instanceName = "lm-20100630";
				const string taxonomyName = "lm-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test, Ignore]
			public void Test_18585_ppl()
			{
				const string accessionNumber = "18585_ppl";
				const string instanceName = "ppl-20100630";
				const string taxonomyName = "ppl-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_21445()
			{
				const string accessionNumber = "21445";
				const string instanceName = "noble-20101231";
				const string taxonomyName = "noble-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_21917()
			{
				const string accessionNumber = "21917";
				const string instanceName = "crao-20100930";
				const string taxonomyName = "crao-20100930";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_21919()
			{
				const string accessionNumber = "21919";
				const string instanceName = "def-20101231";
				const string taxonomyName = "def-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22248()
			{
				const string accessionNumber = "22248";
				const string instanceName = "adm-20100930";
				const string taxonomyName = "adm-20100930";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22249()
			{
				const string accessionNumber = "22249";
				const string instanceName = "inf-20111231";
				const string taxonomyName = "inf-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22250()
			{
				const string accessionNumber = "22250";
				const string instanceName = "noble-20101231";
				const string taxonomyName = "noble-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22250_ATT()
			{
				const string accessionNumber = "22250_ATT";
				const string instanceName = "att-20101231";
				const string taxonomyName = "att-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22409()
			{
				const string accessionNumber = "22409";
				const string instanceName = "rch-20110125";
				const string taxonomyName = "rch-20110125";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_22966()
			{
				const string accessionNumber = "22966";
				const string instanceName = "SEC0107-20091231";
				const string taxonomyName = "SEC0107-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23161()
			{
				const string accessionNumber = "23161";
				const string instanceName = "geBad-20101231";
				const string taxonomyName = "geBad-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23161_eq7()
			{
				const string accessionNumber = "23161_eq7";
				const string instanceName = "eq7-20101231";
				const string taxonomyName = "eq7-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23161_ex1()
			{
				const string accessionNumber = "23161_ex1";
				const string instanceName = "ex1-20101231";
				const string taxonomyName = "ex1-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23264()
			{
				const string accessionNumber = "23264";
				const string instanceName = "qaprshares-20101231";
				const string taxonomyName = "qaprshares-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23323()
			{
				const string accessionNumber = "23323";
				const string instanceName = "valis-20101231";
				const string taxonomyName = "valis-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23332()
			{
				const string accessionNumber = "23332";
				const string instanceName = "duration-20111231";
				const string taxonomyName = "duration-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23332_2()
			{
				const string accessionNumber = "23332_2";
				const string instanceName = "duration-20111231";
				const string taxonomyName = "duration-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23333()
			{
				const string accessionNumber = "23333";
				const string instanceName = "akce-20101231";
				const string taxonomyName = "akce-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23333_bl2()
			{
				const string accessionNumber = "23333_bl2";
				const string instanceName = "bl1-20091231";
				const string taxonomyName = "nereid-20071231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23334()
			{
				const string accessionNumber = "23334";
				const string instanceName = "att-20101231";
				const string taxonomyName = "att-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335()
			{
				const string accessionNumber = "23335";
				const string instanceName = "akce-20101231";
				const string taxonomyName = "akce-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_columns()
			{
				const string accessionNumber = "23335_columns";
				const string instanceName = "akce-20101231";
				const string taxonomyName = "akce-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_ex3()
			{
				const string accessionNumber = "23335_ex3";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_ex4()
			{
				const string accessionNumber = "23335_ex4";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_ex5()
			{
				const string accessionNumber = "23335_ex5";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_ex7()
			{
				const string accessionNumber = "23335_ex7";
				const string instanceName = "ex3-20091231";
				const string taxonomyName = "ex3-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23335_ex10()
			{
				const string accessionNumber = "23335_ex10";
				const string instanceName = "bl1-20091231";
				const string taxonomyName = "nereid-20071231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23336()
			{
				const string accessionNumber = "23336";
				const string instanceName = "baht-20101231";
				const string taxonomyName = "baht-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23337()
			{
				const string accessionNumber = "23337";
				const string instanceName = "nasri-20101231";
				const string taxonomyName = "nasri-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23337_daler()
			{
				const string accessionNumber = "23337_daler";
				const string instanceName = "daler-20101231";
				const string taxonomyName = "daler-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23337_zakim()
			{
				const string accessionNumber = "23337_zakim";
				const string instanceName = "zakim-20101231";
				const string taxonomyName = "zakim-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23995()
			{
				const string accessionNumber = "23995";
				const string instanceName = "tin-20110101";
				const string taxonomyName = "tin-20110101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23995_Mod()
			{
				const string accessionNumber = "23995_Mod";
				const string instanceName = "tin-20110101";
				const string taxonomyName = "tin-20110101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23997()
			{
				const string accessionNumber = "23997";
				const string instanceName = "tin-20110101";
				const string taxonomyName = "tin-20110101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_24634_f_20100331()
			{
				const string accessionNumber = "24634_f_20100331";
				const string instanceName = "f-20100331";
				const string taxonomyName = "f-20100331";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_24634_genz_20091231()
			{
				const string accessionNumber = "24634_genz_20091231";
				const string instanceName = "genz-20091231";
				const string taxonomyName = "genz-20091231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23613()
			{
				const string accessionNumber = "23613";
				const string instanceName = "r23613-20111231";
				const string taxonomyName = "r23613-20111231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23613_20101231()
			{
				const string accessionNumber = "23613-20101231";
				const string instanceName = "23613-20101231";
				const string taxonomyName = "23613-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_23866()
			{
				const string accessionNumber = "23866";
				const string instanceName = "SEC00109-20110211";
				const string taxonomyName = "SEC00109-20110211";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test, Explicit]
			public void Test_24084()
			{
				const string accessionNumber = "24084";
				const string instanceName = "psa-20101231";
				const string taxonomyName = "psa-20101231";

				DateTime start = DateTime.Now;
				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				TimeSpan duration = DateTime.Now - start;
				if( duration.Minutes > 12 )
					Assert.Fail( "Processing time is too long" );
			}

			[Test, Explicit]
			public void Test_24084_fragments()
			{
				const string accessionNumber = "24084_fragments";
				const string instanceName = "psa-20101231";
				const string taxonomyName = "psa-20101231";

				DateTime start = DateTime.Now;
				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				TimeSpan duration = DateTime.Now - start;
				if( duration.Minutes > 12 )
					Assert.Fail( "Processing time is too long" );
			}

			[Test]
			public void Test_24901()
			{
				const string accessionNumber = "24901";
				const string instanceName = "pru-20101231";
				const string taxonomyName = "pru-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_25047()
			{
				const string accessionNumber = "25047";
				const string instanceName = "yen-20101231";
				const string taxonomyName = "yen-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_25047_unit3_20101231()
			{
				const string accessionNumber = "25047-unit3-20101231";
				const string instanceName = "unit3-20101231";
				const string taxonomyName = "unit3-20101231";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_25261_tgt()
			{
				const string accessionNumber = "25261_tgt";
				const string instanceName = "tgt-20100731";
				const string taxonomyName = "tgt-20100731";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test, Explicit]
			public void Test_25516()
			{
				this.BuildAndVerifyRecursive( "25516", "e60403007gd-20111231", "e60403007gd-20111231" );
			}

			[Test]
			public void Test_CopyInstantValues()
			{
				const string accessionNumber = "CopyInstantValues";
				const string instanceName = "adp-20100630";
				const string taxonomyName = "adp-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void Test_CustomUnitRounding()
			{
				const string accessionNumber = "CustomUnitRounding";
				const string instanceName = "adm-20100630";
				const string taxonomyName = "adm-20100630";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}
		}

	}
}

//=============================================================================
// TestReportBuilder (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the main unit test class that generates reports from 100+ filings      
// and compares the generated results against the prepared expected results.
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using XBRLReportBuilder;
using XRB = XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class TestReportBuilderRiskReturn : Test_Abstract
		{
			private int simple = 30;
			private int complex = 60;

			private string _relativeRoot = @"TestFiles\_Baseline_RR";
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
				}
			}

			[Test]
			public void TestBuild_ALFALFA_20090501()
			{
				const string accessionNumber = "_RR_ALFALFA_20090501";
				const string instanceName = "alfalfa-20090501";
				const string taxonomyName = "alfalfa-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_BARLEY_20090930()
			{
				const string accessionNumber = "_RR_BARLEY_20090930";
				const string instanceName = "barley-20090430";
				const string taxonomyName = "barley-20090430";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_CABBAGE_20090501()
			{
				const string accessionNumber = "_RR_CABBAGE_20090501";
				const string instanceName = "cabbage-20090501";
				const string taxonomyName = "cabbage-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_DANDELION_20090711()
			{
				const string accessionNumber = "_RR_DANDELION_20090711";
				const string instanceName = "dandelion-20090711";
				const string taxonomyName = "dandelion-20090711";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < simple, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_EGGPLANT_20090701()
			{
				const string accessionNumber = "_RR_EGGPLANT_20090701";
				const string instanceName = "eggplant-20090701";
				const string taxonomyName = "eggplant-20090701";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_FENNEL_20090711()
			{
				const string accessionNumber = "_RR_FENNEL_20090711";
				const string instanceName = "fennel-20090711";
				const string taxonomyName = "fennel-20090711";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < simple, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_GARLIC_20090501()
			{
				const string accessionNumber = "_RR_GARLIC_20090501";
				const string instanceName = "garlic-20090501";
				const string taxonomyName = "garlic-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_HORSERADISH_20090626()
			{
				const string accessionNumber = "_RR_HORSERADISH_20090626";
				const string instanceName = "horseradish-20090626";
				const string taxonomyName = "horseradish-20090626";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
				Assert.IsTrue( this.processingTime.Seconds < complex, "Exceed pre-determined processing time." );
			}

			[Test]
			public void TestBuild_IMLA_20090101()
			{
				const string accessionNumber = "_RR_IMLA_20090101";
				const string instanceName = "imla-20090101";
				const string taxonomyName = "imla-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_JICAMA_20090101()
			{
				const string accessionNumber = "_RR_JICAMA_20090101";
				const string instanceName = "jicama-20090101";
				const string taxonomyName = "jicama-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_KALE_20090101()
			{
				const string accessionNumber = "_RR_KALE_20090101";
				const string instanceName = "kale-20090101";
				const string taxonomyName = "kale-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_LENTIL_20090101()
			{
				const string accessionNumber = "_RR_LENTIL_20090101";
				const string instanceName = "lentil-20090101";
				const string taxonomyName = "lentil-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_MUSTARD_20090501()
			{
				const string accessionNumber = "_RR_MUSTARD_20090501";
				const string instanceName = "mustard-20090501";
				const string taxonomyName = "mustard-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_NORI_20090501()
			{
				const string accessionNumber = "_RR_NORI_20090501";
				const string instanceName = "nori-20090501";
				const string taxonomyName = "nori-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_OKRA_20090501()
			{
				const string accessionNumber = "_RR_OKRA_20090501";
				const string instanceName = "okra-20090501";
				const string taxonomyName = "okra-20090501";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_POTATO()
			{
				const string accessionNumber = "_RR_POTATO";
				const string instanceName = "potato-20090101";
				const string taxonomyName = "potato-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}

			[Test]
			public void TestBuild_QUANDONG_20090501()
			{
				const string accessionNumber = "_RR_QUANDONG";
				const string instanceName = "quandong-20090101";
				const string taxonomyName = "quandong-20090101";

				this.BuildAndVerifyRecursive( accessionNumber, instanceName, taxonomyName );
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Xml;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{

		[TestFixture]
		public class Test_DS21_Defects : Test_Abstract
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

			/// <summary>
			/// The new Equity processing logic has fixed this scenario.  The old Equity had some issues when processing flow-throughs, and that might have been where this beginning balance was getting lost.
			/// </summary>
			[Test]
			public void Test_12588_gs_20091231()
			{
				this.BuildAndVerifyRecursive( "gs-20091231" );
			}

			/// <summary>
			/// We need to ensure that the transform continues to produce the expected number of "N Months Ended" columns
			/// </summary>
			[Test]
			public void Test_12593_N_MonthsEnded_XSLT()
			{
				//the xml file to transform
				string ddPath = PathCombine( this.baseDir, "Transform", "dd-R42.xml" );
				string ddXml = Transform( ddPath );

				ddXml = Regex.Replace( ddXml, @"<br[^>]*>", string.Empty );
				ddXml = Regex.Replace( ddXml, @"<link[^>]+>", string.Empty );
				ddXml = Regex.Replace( ddXml, @"<META[^>]+>", string.Empty );
				ddXml = ddXml.Replace( "&#xA0;", "&#160;" );
				ddXml = ddXml.Replace( "&#xA0", "&#160;" );

				//now load it into an XML doc so we can count it
				XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml( ddXml );
				XmlNodeList nodes = xDoc.SelectNodes( "//th[ contains( ., ' Ended' ) ]" );// and contains( @Label, ' Ended' ) ]" );
				Assert.AreEqual( 20, nodes.Count, "The transform did not produce the expected number of 'N Months Ended' labels." );

				string agnPath = PathCombine( this.baseDir, "Transform", "agn-R1.xml" );
				string agnXml = Transform( agnPath );

				agnXml = Regex.Replace( agnXml, @"<br[^>]*>", string.Empty );
				agnXml = Regex.Replace( agnXml, @"<link[^>]+>", string.Empty );
				agnXml = Regex.Replace( agnXml, @"<META[^>]+>", string.Empty );


				//now load it into an XML doc so we can count it
				xDoc.LoadXml( agnXml );
				nodes = xDoc.SelectNodes( "//th[ contains( ., ' Ended' ) ]" );
				Assert.AreEqual( 1, nodes.Count, "The transform did not produce the expected number of 'N Months Ended' labels." );
				foreach( XmlNode node in nodes )
				{
					Assert.AreEqual( node.Attributes[ "colspan" ].Value, "4", "The transform did not produce the expected number of 'N Months Ended' labels." );
					break;
				}
			}

			/// <summary>
			/// I added special logic to check that the beginning balances don't provide MORE detail than the preceding ending balance.
			/// </summary>
			[Test]
			public void Test_12602_ctolen_20100630()
			{
				this.BuildAndVerifyRecursive( "ctolen-20100630" );
			}

			[Test]
			public void Test_DE1719_rnd54_20091231()
			{
				this.BuildAndVerifyRecursive( "rnd54-20091231" );
			}

			/// <summary>
			/// Emily's upgrades to the "Process Segments" logic fixed this - this report no longer goes vertical.
			/// </summary>
			[Test]
			public void Test_17544_vfc_20100703()
			{
				this.BuildAndVerifyRecursive( "vfc-20100703" );
			}

			/// <summary>
			/// Verify that custom units do not cause extraneous columns to be created
			/// </summary>
			[Test]
			public void Test_17579_exc_20100630_SEC0058()
			{
				this.BuildAndVerifyRecursive( "exc-20100630" );
			}

			[Test]
			public void Test_18032_aig_20091231()
			{
				this.BuildAndVerifyRecursive( "aig-20091231" );
			}

			[Test]
			public void Test_18032_atwd_20100630()
			{
				this.BuildAndVerifyRecursive( "atwd-20100630" );
			}

			[Test]
			public void Test_18032_dva_20090630()
			{
				this.BuildAndVerifyRecursive( "dva-20090630" );
			}

			[Test]
			public void Test_DE1875_bmy_20100630()
			{
				this.BuildAndVerifyRecursive( "bmy-20100630" );
			}

			[Test]
			public void Test_DE1875_per_20091231()
			{
				this.BuildAndVerifyRecursive( "per-20091231" );
			}

			[Test]
			public void Test_DE1958_fwlt_20100630()
			{
				this.BuildAndVerifyRecursive( "fwlt-20100630" );
			}

			[Test]
			public void Test_DE2013_wm_20100806()
			{
				this.BuildAndVerifyRecursive( "wm-20100806" );
			}

			[Test]
			public void Test_DimensionsAreSorted_CCE()
			{
				this.BuildAndVerifyRecursive( "cce-20100702" );
			}

			[Test]
			public void Test_DimensionsAreSorted_EUROPA()
			{
				this.BuildAndVerifyRecursive( "europa-20091031" );
			}
		}

	}
}
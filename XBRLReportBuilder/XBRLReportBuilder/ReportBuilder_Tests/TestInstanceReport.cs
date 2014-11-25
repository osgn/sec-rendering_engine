//=============================================================================
// TestInstanceReport (unit test class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// Test methods in the data class InstanceReport.
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using NUnit.Framework;
using XRB = XBRLReportBuilder;
using XBRLReportBuilder;
using XBRLReportBuilder.Utilities;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
    [TestFixture] 
    public class TestInstanceReport : InstanceReport
    {
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

		protected string relativeRoot = @"TestFiles\_BaselineFinancial";

		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        [TestFixtureSetUp] public void RunFirst()
        {}

        /// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {}

		/// <summary> Sets up test values before each test is called </summary>
        [SetUp] public void RunBeforeEachTest()
        {
		}

        /// <summary>Tears down test values after each test is run </summary>
        [TearDown] public void RunAfterEachTest() 
        {}

		#endregion

		#region Tests

		[Test]
		public void TestMergeInstanceAndDuration()
		{
			InitializeTestInstanceReport();

			this.MergeInstantAndDuration();
			Assert.AreEqual(5, this.Columns.Count, "Duration and Instance columns were not correctly merged.");
		}

		[Test]
		public void TestProcessSegmentsConsolidated()
		{
			InitializeTestInstanceReport();

            this.MergeInstantAndDuration();
			bool processed = this.ProcessSegments();
			Assert.IsTrue(processed, "ProcessSegments did not set processed to true");

			Assert.AreEqual(3, this.Columns.Count, "The Segmented columns were not converted to rows");
		}

		[Test]
		public void TestProcessSegmentsCommonSegmented()
		{
			InitializeTestInstanceReportCommonSegment();

            this.MergeInstantAndDuration();
			bool processed = this.ProcessSegments();
			Assert.IsTrue(processed, "ProcessSegments did not set processed to true");

			Assert.AreEqual(5, this.Columns.Count, "The Segmented columns were not converted to rows");
		}

		[Test]
		public void TestProcessColumnHeaders()
		{
			InitializeTestInstanceReport();

			this.ProcessColumnHeaders();

			DateTime expectedDate = new DateTime(2006, 12, 31);
			LabelLine expectedLabel = new LabelLine(1, "3 Months Ended");
			Assert.IsTrue(this.Columns[0].Labels.Contains(expectedLabel), "Column 0 does not contain the Months Ended label");
			expectedLabel = new LabelLine(2, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[0].Labels.Contains(expectedLabel), "Column 0 does not contain the Date label");

			expectedDate = new DateTime(2007, 12, 31);
			expectedLabel = new LabelLine(1, "3 Months Ended");
			Assert.IsTrue(this.Columns[1].Labels.Contains(expectedLabel), "Column 1 does not contain the Months Ended label");
			expectedLabel = new LabelLine(2, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[1].Labels.Contains(expectedLabel), "Column 1 does not contain the Date label");

			expectedDate = new DateTime(2007, 12, 31);
			expectedLabel = new LabelLine(1, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[2].Labels.Contains(expectedLabel), "Column 2 does not contain the date label");

			expectedDate = new DateTime(2006, 12, 31);
			expectedLabel = new LabelLine(1, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[3].Labels.Contains(expectedLabel), "Column 2 does not contain the date label");

			expectedDate = new DateTime(2008, 12, 31);
			expectedLabel = new LabelLine(1, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[4].Labels.Contains(expectedLabel), "Column 2 does not contain the date label");

			expectedDate = new DateTime(2008, 12, 31);
			expectedLabel = new LabelLine(1, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[5].Labels.Contains(expectedLabel), "Column 2 does not contain the date label");

			expectedDate = new DateTime(2008, 12, 31);
			expectedLabel = new LabelLine(1, expectedDate.ToString("MMM. dd, yyyy"));
			Assert.IsTrue(this.Columns[6].Labels.Contains(expectedLabel), "Column 2 does not contain the date label");
		}

		[Test]
		public void TestPromoteSharedColumnLabels()
		{
			InitializeTestInstanceReportWithSharedLabels();

			this.PromoteSharedColumnLabels(false);

			Assert.IsTrue(this.ReportName.Contains("Shared Segment"), "The Shared Segment was not promoted to the Report name");
		}

		[Ignore,Obsolete]
		public void TestEquityHelperMethods()
		{
			InitializeTestEquityReport();

			#pragma warning disable 0219
			Dictionary<CalendarPeriod, List<InstanceReportColumn>> colsByPer = new Dictionary<CalendarPeriod, List<InstanceReportColumn>>();
			Dictionary<string, int> masterColsBySeg = new Dictionary<string, int>();
            Dictionary<string, List<int>> prevReportColsBySeg = new Dictionary<string, List<int>>();
            Dictionary<string, List<int>> adjustmentColsBySeg = new Dictionary<string, List<int>>();
            Dictionary<int, ColumnMap> colMaps = new Dictionary<int, ColumnMap>();
			#pragma warning restore 0219

            //this.BuildColumnGroups(colsByPer, masterColsBySeg, prevReportColsBySeg, adjustmentColsBySeg, colMaps);

			Assert.AreEqual(2, colsByPer.Count, "Invalid number of periods found by BuildColumnGroups");

			CalendarPeriod cp1 = new CalendarPeriod(new DateTime(2006, 10, 1), new DateTime(2006, 12, 31));
			CalendarPeriod cp2 = new CalendarPeriod(new DateTime(2007, 10, 1), new DateTime(2007, 12, 31));
			Assert.IsTrue(colsByPer.ContainsKey(cp1), "Context period 10/1/2006 - 12/31/2006 was not returned as a group");
			Assert.AreEqual(3, colsByPer[cp1].Count, "Invalid number of Coluns grouped under CP1");
			
			Assert.IsTrue(colsByPer.ContainsKey(cp2), "Context period 10/1/2007 - 12/31/2007 was not returned as a group");
			Assert.AreEqual(3, colsByPer[cp2].Count, "Invalid number of Coluns grouped under CP2");

			Assert.AreEqual(3, masterColsBySeg.Count, "Invalid number of colums define as master columns");
			List<int> expectedMasterColIds = new List<int>(new int[] {0, 2, 4});
			foreach(string segKey in masterColsBySeg.Keys)
			{
				int colId = masterColsBySeg[segKey];
				Assert.IsTrue(expectedMasterColIds.Contains(colId), string.Format("colId, {0}, not found in masterColsBySeg", colId));

				string expectedSegmentString = colMaps[colId].IRC.GetSegmentsString(true, false, false, false, false, this.ReportName);
				Assert.AreEqual(expectedSegmentString, segKey, string.Format("Column {0} was declared a master column for the wrong segment set", colId));
			}

			for(int i = 0; i < masterColsBySeg.Count; i++)
			{
				int colId = i;
				int mappedColId = (i % 2 == 0) ? i : (i - 1);

				Assert.AreEqual(colId, colMaps[colId].IRC.Id, String.Format("Column {0} was not keyed correctly in the Dictionary", colMaps[colId].IRC.Id));
				Assert.AreEqual(mappedColId, colMaps[colId].MappedColumnId, string.Format("Column {0} was not mapped to the correct column", colId));
			}

			//this.RemoveDuplicateSegmentColumns(masterColsBySeg, colMaps);
			Assert.AreEqual(3, this.Columns.Count, "The columns were not correctly condensed by RemoveDuplicateSegmentColumns");

//            this.BuildEquityRowCollection(colsByPer, prevReportColsBySeg, adjustmentColsBySeg, colMaps, "MMM. dd, yyyy");

			Assert.AreEqual(8, this.Rows.Count, "Incorrect number of rows after BuildEquityRowCollection");

			for (int i = 0; i < 2; i++)
			{
				int beginningBalRowId = i * 4;
				Assert.IsTrue(this.Rows[beginningBalRowId].IsBeginningBalance, string.Format("Row {0} is not marked as a Beginning Balance", beginningBalRowId));
				Assert.IsTrue(this.Rows[beginningBalRowId].IsCalendarTitle, string.Format("Row {0} is not marked as a Calendar Title", beginningBalRowId));
			}

			for (int i = 0; i < 8; i++)
			{
				int rowIdx = i;
				int origRowIdx = rowIdx % 4;

				InstanceReportRow row = this.Rows[rowIdx];
				InstanceReportRow origRow = this.Rows[origRowIdx];

				Assert.AreEqual(this.Columns.Count, row.Cells.Count, string.Format("Incorrect number of cells in row {0}", rowIdx));
				for(int j = 0; j < this.Columns.Count; j++)
				{
					Cell c = row.Cells[j] as Cell;
					Assert.AreEqual(this.Columns[j].Id, c.Id, string.Format("Cell {0} on Row {1} does not have a valid cell id", j, i));
				}

				Assert.AreEqual(origRow.SimpleDataType, row.SimpleDataType, "Data types do not match");
				Assert.AreEqual(origRow.ElementName, row.ElementName, "Data types do not match");
				Assert.AreEqual(origRow.ElementPrefix, row.ElementPrefix, "Data types do not match");
				Assert.AreEqual(origRow.BalanceType, row.BalanceType, "Data types do not match");
			}

		}

        [Test]
        public void TestMonthsBetweenSameYear()
        {
            InstanceReport report = new InstanceReport();                                 
            InstanceReportColumn irc = new InstanceReportColumn();
			ContextProperty cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2011, 1, 1);
			cp.PeriodEndDate = new DateTime(2014, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()), "Calendar");
			irc.Labels.Add(ll);
            report.Columns.Add(irc);
            report.SetCalendarLabels("MMM. dd, yyyy", "{n} Months Ended");
            Assert.AreEqual("48 Months Ended", (irc.Labels[0]).Label, "wrong label");            
        }

        [Test]
        public void TestMonthsBetweenDifferentYear()
        {
            InstanceReport report = new InstanceReport();
            InstanceReportColumn irc = new InstanceReportColumn();
            ContextProperty cp = new ContextProperty();
            cp.PeriodStartDate = new DateTime(2011, 7, 1);
            cp.PeriodEndDate = new DateTime(2014, 6, 30);
            cp.PeriodType = Element.PeriodType.duration;
            irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
            LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()), "Calendar");
            irc.Labels.Add(ll);
            report.Columns.Add(irc);
            report.SetCalendarLabels("MMM. dd, yyyy", "{n} Months Ended");
            Assert.AreEqual("36 Months Ended", (irc.Labels[0]).Label, "wrong label");
        }

		delegate string tmp();

		[Test]
		public void TestRoundingOption()
		{
			InstanceReport instance = new InstanceReport();

			tmp cb = delegate()
			{
				instance.SetRoundingOption
				(
					"In {level}",

					"Shares",
					"Shares in {level}",

					"Share data",
					"Share data in {level}",

					"Per Share data",
					"Per Share data",

					"unless otherwise specified"
				);

				return instance.RoundingOption;
			};

			Type rlType = typeof( RoundingLevel );

			string matrixPath = Path.Combine( baseDir, "RoundingOptions.csv" );
			string[] lines = File.ReadAllLines( matrixPath );
			for( int i = 1; i < lines.Length; i++ )
			{
				string line = lines[ i ];
				string tmp = line.Replace( ",", string.Empty ).Trim();
				if( tmp.Length == 0 )
					continue;

				string[] values = line.Split( ',' );
				instance.MonetaryRoundingLevel = (RoundingLevel)Enum.Parse( rlType, values[ 0 ] );
				instance.SharesRoundingLevel = (RoundingLevel)Enum.Parse( rlType, values[ 1 ] );
				instance.PerShareRoundingLevel = (RoundingLevel)Enum.Parse( rlType, values[ 2 ] );
				instance.HasCustomUnits = values[ 3 ] == "Y";

				string actual = cb();

				int use = values.Length - 4;
				string expected = string.Join( ",", values, 4, use );
				expected = expected.Trim( '"' );

				Assert.AreEqual( actual, expected );
			}
		}

		#endregion

        #region Test Promote Footnotes

        [Test]
        public void TestRowLevelPromotions_1()
        {
            InstanceReport thisReport = new InstanceReport();

            //[1], [1], [1], [1], NULL --> [1] promoted
            InstanceReportRow thisRow = new InstanceReportRow();
            thisRow.Label = "Row 1";
            thisRow.Id = 1;
            for (int index = 0; index <= 3; index++)
            {
                Cell c = new Cell(index);
                c.IsNumeric = true;
                c.NumericAmount = 100;
                c.FootnoteIndexer = "[1]";
                thisRow.Cells.Add(c);
            }
            Cell c4 = new Cell(4);
            c4.IsNumeric = false;            
            thisRow.Cells.Add(c4);

            thisReport.Rows.Add(thisRow);
            thisReport.PromoteFootnotes();

            Console.WriteLine ("Row level index = " + thisRow.FootnoteIndexer);
            foreach (Cell c in thisRow.Cells)
            {
                Console.WriteLine(c.Id + " Cell level index = " + c.FootnoteIndexer);
            }

            Assert.IsTrue(thisRow.FootnoteIndexer == "[1]");
            for (int index = 0; index <= 3; index++)
            {
                Cell c1 = thisRow.Cells[index] as Cell;
                Assert.IsTrue(String.IsNullOrEmpty(c1.FootnoteIndexer));

            }


        }

        [Test]
        public void TestRowLevelPromotions_2()
        {
            InstanceReport thisReport = new InstanceReport();

            //[1],[2]    [1],[2],[3]      [1],[2],[3]     [1],[2] NULL --> [1],[2] promoted
            InstanceReportRow thisRow = new InstanceReportRow();
            thisRow.Label = "Row 1";           
            thisRow.Id = 1;

            Cell c0 = new Cell(0);
            c0.IsNumeric = true;
            c0.FootnoteIndexer = "[1],[2]";
            thisRow.Cells.Add(c0);

            Cell c1 = new Cell(1);
            c1.IsNumeric = true;
            c1.FootnoteIndexer = "[1],[2],[3]";
            thisRow.Cells.Add(c1);


            Cell c2 = new Cell(2);
            c2.IsNumeric = true;
            c2.FootnoteIndexer = "[1],[2],[3]";
            thisRow.Cells.Add(c2);

            Cell c3 = new Cell(3);
            c3.IsNumeric = true;
            c3.FootnoteIndexer = "[1],[2]";
            thisRow.Cells.Add(c3);

           
            Cell c4 = new Cell(4);
            c4.IsNumeric = false;
            thisRow.Cells.Add(c4);

            thisReport.Rows.Add(thisRow);
            thisReport.PromoteFootnotes();

            Console.WriteLine("Row level index = " + thisRow.FootnoteIndexer);
            foreach (Cell c in thisRow.Cells)
            {
                Console.WriteLine(c.Id + " Cell level index = " + c.FootnoteIndexer);
            }

            Assert.IsTrue(thisRow.FootnoteIndexer == "[1],[2]");
            Assert.IsTrue((thisRow.Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[1] as Cell).FootnoteIndexer == "[3]");
            Assert.IsTrue((thisRow.Cells[2] as Cell).FootnoteIndexer == "[3]");
            Assert.IsTrue((thisRow.Cells[3] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[4] as Cell).FootnoteIndexer == "");

        }

        [Test]
        public void TestRowLevelPromotions_3()
        {
            InstanceReport thisReport = new InstanceReport();

            //NULL    [1]     [1],[2],[3]     NULL   [1],[3] --> [1] promoted
            InstanceReportRow thisRow = new InstanceReportRow();
            thisRow.Label = "Row 1";
            thisRow.Id = 1;

            Cell c0 = new Cell(0);
            c0.IsNumeric = false;
            thisRow.Cells.Add(c0);


            Cell c1 = new Cell(1);
            c1.IsNumeric = true;
            c1.FootnoteIndexer = "[1]";
            thisRow.Cells.Add(c1);


            Cell c2 = new Cell(2);
            c2.IsNumeric = true;
            c2.FootnoteIndexer = "[1],[2],[3]";
            thisRow.Cells.Add(c2);


            Cell c3 = new Cell(3);
            c3.IsNumeric = false;
            thisRow.Cells.Add(c3);

            Cell c4 = new Cell(4);
            c4.IsNumeric = true;
            c4.FootnoteIndexer = "[1],[3]";
            thisRow.Cells.Add(c4);

            thisReport.Rows.Add(thisRow);
            thisReport.PromoteFootnotes();

            Console.WriteLine("Row level index = " + thisRow.FootnoteIndexer);
            foreach (Cell c in thisRow.Cells)
            {
                Console.WriteLine(c.Id + " Cell level index = " + c.FootnoteIndexer);
            }

            Assert.IsTrue(thisRow.FootnoteIndexer == "[1]");
            Assert.IsTrue((thisRow.Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[1] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[2] as Cell).FootnoteIndexer == "[2],[3]");
            Assert.IsTrue((thisRow.Cells[3] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[4] as Cell).FootnoteIndexer == "[3]");

        }

        [Test]
        public void TestRowLevelPromotions_4()
        {
            InstanceReport thisReport = new InstanceReport();

            //[1]    [2]     [1],[2]     NULL   [3],[4]    --> NOTHING promoted
            InstanceReportRow thisRow = new InstanceReportRow();
            thisRow.Label = "Row 1";
            thisRow.Id = 1;

            Cell c0 = new Cell(0);
            c0.IsNumeric = true;
            c0.FootnoteIndexer = "[1]";
            thisRow.Cells.Add(c0);


            Cell c1 = new Cell(1);
            c1.IsNumeric = true;
            c1.FootnoteIndexer = "[2]";
            thisRow.Cells.Add(c1);


            Cell c2 = new Cell(2);
            c2.IsNumeric = true;
            c2.FootnoteIndexer = "[1],[2]";
            thisRow.Cells.Add(c2);


            Cell c3 = new Cell(3);
            c3.IsNumeric = false;
            thisRow.Cells.Add(c3);

            Cell c4 = new Cell(4);
            c4.IsNumeric = true;
            c4.FootnoteIndexer = "[3],[4]";
            thisRow.Cells.Add(c4);

            thisReport.Rows.Add(thisRow);
            thisReport.PromoteFootnotes();

            Console.WriteLine("Row level index = " + thisRow.FootnoteIndexer);
            foreach (Cell c in thisRow.Cells)
            {
                Console.WriteLine(c.Id + " Cell level index = " + c.FootnoteIndexer);
            }

            Assert.IsTrue(thisRow.FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[0] as Cell).FootnoteIndexer == "[1]");
            Assert.IsTrue((thisRow.Cells[1] as Cell).FootnoteIndexer == "[2]");
            Assert.IsTrue((thisRow.Cells[2] as Cell).FootnoteIndexer == "[1],[2]");
            Assert.IsTrue((thisRow.Cells[3] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisRow.Cells[4] as Cell).FootnoteIndexer == "[3],[4]");

        }

        [Test]
        public void TestColumnLevelPromotions_1()
        {
            InstanceReport thisReport = new InstanceReport();
            InstanceReportColumn irc =  new InstanceReportColumn();

            //[1], [1], [1], [1], [1] --> nothing is promoted to column, should already be promoted to ROW
            irc.Id = 0;
            thisReport.Columns.Add(irc);

            for (int index = 0; index <= 4; index++)
            {
                InstanceReportRow irr = new InstanceReportRow();
                irr.Id = index;
                Cell c = new Cell(0);
                irr.Cells.Add(c);
                thisReport.Rows.Add(irr);
            }
            (thisReport.Rows[0].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[1].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[2].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[3].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[4].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer = "[1]";

            thisReport.PromoteFootnotes();

            Console.WriteLine("Column level index = " + irc.FootnoteIndexer);
            foreach (InstanceReportRow irr in thisReport.Rows)            
            {
                Cell c = irr.Cells[0] as Cell;
                Console.WriteLine(c.Id + " Cell level index = " + c.FootnoteIndexer);
            }

            Assert.IsTrue(irc.FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer == "");

            
        }

        [Test]
        public void TestColumnLevelPromotions_2()
        {
            InstanceReport thisReport = new InstanceReport();
            InstanceReportColumn irc1 = new InstanceReportColumn();
            InstanceReportColumn irc2 = new InstanceReportColumn();

            //c0: [1], [1], [1], [1], [1] --> [1]
            //c1: [2], [3], [4], [5], [6] --> NULL
            irc1.Id = 0;
            thisReport.Columns.Add(irc1);
            irc2.Id = 1;
            thisReport.Columns.Add(irc2);

            for (int index = 0; index <= 4; index++)
            {
                InstanceReportRow irr = new InstanceReportRow();
                irr.Id = index;
                Cell c1 = new Cell(0);
                irr.Cells.Add(c1);
                Cell c2 = new Cell(1);
                irr.Cells.Add(c2);
                thisReport.Rows.Add(irr);
            }
            (thisReport.Rows[0].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[1].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[2].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[3].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer = "[1]";
            (thisReport.Rows[4].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer = "[1]";

            (thisReport.Rows[0].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[0].Cells[1] as Cell).FootnoteIndexer = "[2]";
            (thisReport.Rows[1].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[1].Cells[1] as Cell).FootnoteIndexer = "[3]";
            (thisReport.Rows[2].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[2].Cells[1] as Cell).FootnoteIndexer = "[4]";
            (thisReport.Rows[3].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[3].Cells[1] as Cell).FootnoteIndexer = "[5]";
            (thisReport.Rows[4].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[4].Cells[1] as Cell).FootnoteIndexer = "[6]";

            thisReport.PromoteFootnotes();

            Console.WriteLine("Column 1 level index = " + irc1.FootnoteIndexer);
            Console.WriteLine("Column 2 level index = " + irc2.FootnoteIndexer);
            foreach (InstanceReportRow irr in thisReport.Rows)
            {
                Cell c1 = irr.Cells[0] as Cell;
                Console.WriteLine(c1.Id + " Cell level index = " + c1.FootnoteIndexer);
                Cell c2 = irr.Cells[1] as Cell;
                Console.WriteLine(c2.Id + " Cell level index = " + c2.FootnoteIndexer);
            }

            Assert.IsTrue(irc1.FootnoteIndexer == "[1]");
            Assert.IsTrue(irc2.FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer == "");

            Assert.IsTrue((thisReport.Rows[0].Cells[1] as Cell).FootnoteIndexer == "[2]");
            Assert.IsTrue((thisReport.Rows[1].Cells[1] as Cell).FootnoteIndexer == "[3]");
            Assert.IsTrue((thisReport.Rows[2].Cells[1] as Cell).FootnoteIndexer == "[4]");
            Assert.IsTrue((thisReport.Rows[3].Cells[1] as Cell).FootnoteIndexer == "[5]");
            Assert.IsTrue((thisReport.Rows[4].Cells[1] as Cell).FootnoteIndexer == "[6]");


        }

        [Test]
        public void TestColumnLevelPromotions_3()
        {
            InstanceReport thisReport = new InstanceReport();
            InstanceReportColumn irc1 = new InstanceReportColumn();
            InstanceReportColumn irc2 = new InstanceReportColumn();

            //c0: [1],[2]    [1],[2]    [1],[2]    [1],[2],[3]     [1],[2] --> [1],[2]
            //c1: [4], [5], [6], [7], [8] --> NULL
            irc1.Id = 1;
            thisReport.Columns.Add(irc1);
            irc2.Id = 2;
            thisReport.Columns.Add(irc2);

            for (int index = 0; index <= 4; index++)
            {
                InstanceReportRow irr = new InstanceReportRow();
                irr.Id = index + 1;
                Cell c1 = new Cell(1);
                irr.Cells.Add(c1);
                Cell c2 = new Cell(2);
                irr.Cells.Add(c2);
                thisReport.Rows.Add(irr);
            }
            (thisReport.Rows[0].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer = "[1],[2]";
            (thisReport.Rows[1].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer = "[1],[2]";
            (thisReport.Rows[2].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer = "[1],[2]";
            (thisReport.Rows[3].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer = "[1],[2],[3]";
            (thisReport.Rows[4].Cells[0] as Cell).IsNumeric = true;
            (thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer = "[1],[2]";

            (thisReport.Rows[0].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[0].Cells[1] as Cell).FootnoteIndexer = "[4]";
            (thisReport.Rows[1].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[1].Cells[1] as Cell).FootnoteIndexer = "[5]";
            (thisReport.Rows[2].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[2].Cells[1] as Cell).FootnoteIndexer = "[6]";
            (thisReport.Rows[3].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[3].Cells[1] as Cell).FootnoteIndexer = "[7]";
            (thisReport.Rows[4].Cells[1] as Cell).IsNumeric = true;
            (thisReport.Rows[4].Cells[1] as Cell).FootnoteIndexer = "[8]";

            thisReport.PromoteFootnotes();

            Console.WriteLine("Column 1 level index = " + irc1.FootnoteIndexer);
            Console.WriteLine("Column 2 level index = " + irc2.FootnoteIndexer);
            foreach (InstanceReportRow irr in thisReport.Rows)
            {
                Cell c1 = irr.Cells[0] as Cell;
                Console.WriteLine(c1.Id + " Cell level index = " + c1.FootnoteIndexer);
                Cell c2 = irr.Cells[1] as Cell;
                Console.WriteLine(c2.Id + " Cell level index = " + c2.FootnoteIndexer);
            }

            Assert.IsTrue(irc1.FootnoteIndexer == "[1],[2]");
            Assert.IsTrue(irc2.FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[0].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[1].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[2].Cells[0] as Cell).FootnoteIndexer == "");
            Assert.IsTrue((thisReport.Rows[3].Cells[0] as Cell).FootnoteIndexer == "[3]");
            Assert.IsTrue((thisReport.Rows[4].Cells[0] as Cell).FootnoteIndexer == "");

            Assert.IsTrue((thisReport.Rows[0].Cells[1] as Cell).FootnoteIndexer == "[4]");
            Assert.IsTrue((thisReport.Rows[1].Cells[1] as Cell).FootnoteIndexer == "[5]");
            Assert.IsTrue((thisReport.Rows[2].Cells[1] as Cell).FootnoteIndexer == "[6]");
            Assert.IsTrue((thisReport.Rows[3].Cells[1] as Cell).FootnoteIndexer == "[7]");
            Assert.IsTrue((thisReport.Rows[4].Cells[1] as Cell).FootnoteIndexer == "[8]");


        }
       

        #endregion

        #region private helpers

        private void InitializeTestInstanceReport()
		{
			this.Columns.Clear();
			this.Rows.Clear();

			//Add reporting periods to columns for testing MergeInstanceAndDuration
			InstanceReportColumn irc = new InstanceReportColumn();
			ContextProperty cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()), "Calendar");
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			ll = new LabelLine( 0, string.Format( "{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString() ), "Calendar" );
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu2", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString(), "Calendar");
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu3", cp);
			ll = new LabelLine( 0, cp.PeriodStartDate.ToShortDateString(), "Calendar" );
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu4", cp);
			ll = new LabelLine( 0, cp.PeriodStartDate.ToShortDateString(), "Calendar" );
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(new Segment("Segment1", "Segment One", "Value1"));
			irc.MCU = new MergedContextUnitsWrapper("mcu5", cp);
			ll = new LabelLine( 0, cp.PeriodStartDate.ToShortDateString(), "Calendar" );
			irc.Labels.Add(ll);
			irc.Labels.Add(new LabelLine(1, "Segment One"));
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(new Segment("Segment2", "Segment Two", "Value2"));
			irc.MCU = new MergedContextUnitsWrapper("mcu6", cp);
			ll = new LabelLine( 0, cp.PeriodStartDate.ToShortDateString(), "Calendar" );
			irc.Labels.Add(ll);
			irc.Labels.Add(new LabelLine(2, "Segment Two"));
			this.Columns.Add(irc);

			InstanceReportRow irr = new InstanceReportRow("Test Elem", 0);
			irr.ElementName = "Test Elem";
			for (int i = 0; i < 7; i++)
			{
				Cell cell = new Cell();

				if( i < 2 || 4 < i )
				{
					cell.IsNumeric = true;
					cell.NumericAmount = (decimal)( ( i + 1 ) * 10.0 );
				}

				irr.Cells.Add(cell);
			}
			this.Rows.Add( irr );

			irr = new InstanceReportRow( "Test Elem2", 0 );
			irr.ElementName = "Test Elem2";
			for( int i = 0; i < 7; i++ )
			{
				Cell cell = new Cell();

				if( 1 < i && i < 5 )
				{
					cell.IsNumeric = true;
					cell.NumericAmount = (decimal)( ( i + 1 ) * 10.0 );
				}

				irr.Cells.Add( cell );
			}


			this.Rows.Add(irr);
		}

		private void InitializeTestInstanceReportCommonSegment()
		{
            this.Columns.Clear();
            this.Rows.Clear();

			Segment sharedSegment = new Segment("SharedSeg", "Shared Segment", "Shared Segment Value");
			sharedSegment.DimensionInfo = new ContextDimensionInfo();
			sharedSegment.DimensionInfo.Id = "SharedSegmentAxis";
			sharedSegment.DimensionInfo.dimensionId = "SharedSegmentMember";

			//Add reporting periods to columns for testing MergeInstanceAndDuration
			InstanceReportColumn irc = new InstanceReportColumn();
			ContextProperty cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			cp.Segments.Add(sharedSegment);
			LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			cp.Segments.Add(sharedSegment);
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu2", cp);
			cp.Segments.Add(sharedSegment);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu3", cp);
			cp.Segments.Add(sharedSegment);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(sharedSegment);
			irc.MCU = new MergedContextUnitsWrapper("mcu4", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(sharedSegment);
			irc.MCU = new MergedContextUnitsWrapper("mcu5", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			cp.Segments.Add(sharedSegment);
			irc.Labels.Add(new LabelLine(1, "Segment One"));
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(sharedSegment);
			irc.MCU = new MergedContextUnitsWrapper("mcu6", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			cp.Segments.Add(sharedSegment);
			irc.Labels.Add(new LabelLine(2, "Segment Two"));
			this.Columns.Add(irc);

			InstanceReportRow irr = new InstanceReportRow("Test Elem", 7);
			irr.ElementName = "Test Elem";
			for (int i = 0; i < 7; i++)
			{
				Cell cell = new Cell();
				cell.NumericAmount = (decimal)((i + 1) * 10.0);
				irr.Cells.Add(cell);
			}
			this.Rows.Add(irr);
		}

		private void InitializeTestInstanceReportWithSharedLabels()
		{
			this.Columns.Clear();
			this.Rows.Clear();

			//Add reporting periods to columns for testing MergeInstanceAndDuration
			InstanceReportColumn irc = new InstanceReportColumn();
			ContextProperty cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu2", cp);
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			irc.MCU = new MergedContextUnitsWrapper("mcu3", cp);
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			irc.MCU = new MergedContextUnitsWrapper("mcu4", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			irc.MCU = new MergedContextUnitsWrapper("mcu5", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			irc.Labels.Add(new LabelLine(1, "Segment One"));
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2008, 12, 31);
			cp.PeriodType = Element.PeriodType.instant;
			cp.Segments.Add(new Segment("SharedSeg", "Shared Segment", "Shared Segment Value"));
			irc.MCU = new MergedContextUnitsWrapper("mcu6", cp);
			ll = new LabelLine(0, cp.PeriodStartDate.ToShortDateString());
			irc.Labels.Add(ll);
			irc.Labels.Add(new LabelLine(2, "Segment Two"));
			this.Columns.Add(irc);

			InstanceReportRow irr = new InstanceReportRow("Test Elem", 7);
			irr.ElementName = "Test Elem";
			for (int i = 0; i < 7; i++)
			{
				Cell cell = new Cell();
				cell.NumericAmount = (decimal)((i + 1) * 10.0);
				irr.Cells.Add(cell);
			}
			this.Rows.Add(irr);
		}

		private void InitializeTestEquityReport()
		{
			this.Columns.Clear();
			this.Rows.Clear();

			//Add reporting periods to columns for testing MergeInstanceAndDuration
			InstanceReportColumn irc = new InstanceReportColumn();
			irc.Id = 0;
			ContextProperty cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			cp.Segments.Add(new Segment("Segment 1", "Shared One", "Segment One Value"));
			LabelLine ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			irc.Id = 1;
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			cp.Segments.Add(new Segment("Segment 1", "Shared One", "Segment One Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			irc.Id = 2;
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			cp.Segments.Add(new Segment("Segment 2", "Shared Two", "Segment Two Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			irc.Id = 3;
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			cp.Segments.Add(new Segment("Segment 2", "Shared Two", "Segment Two Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			irc.Id = 4;
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2006, 10, 1);
			cp.PeriodEndDate = new DateTime(2006, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu0", cp);
			cp.Segments.Add(new Segment("Segment 3", "Shared Three", "Segment Three Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			irc = new InstanceReportColumn();
			irc.Id = 5;
			cp = new ContextProperty();
			cp.PeriodStartDate = new DateTime(2007, 10, 1);
			cp.PeriodEndDate = new DateTime(2007, 12, 31);
			cp.PeriodType = Element.PeriodType.duration;
			irc.MCU = new MergedContextUnitsWrapper("mcu1", cp);
			cp.Segments.Add(new Segment("Segment 3", "Shared Three", "Segment Three Value"));
			ll = new LabelLine(0, string.Format("{0} - {1}", cp.PeriodStartDate.ToShortDateString(), cp.PeriodEndDate.ToShortDateString()));
			irc.Labels.Add(ll);
			this.Columns.Add(irc);

			for (int i = 0; i < 4; i++)
			{
				InstanceReportRow irr = new InstanceReportRow("Test Elem", 6);
				irr.ElementName = "Test Elem";
				irr.IsBeginningBalance = (i == 0);

				for (int j = 0; j < 6; j++)
				{
					Cell cell = new Cell();
					cell.Id = j;
					cell.NumericAmount = (decimal)((j + 1) * 10.0);
					irr.Cells.Add(cell);
				}
				this.Rows.Add(irr);
			}
		}

		#endregion
	}
}
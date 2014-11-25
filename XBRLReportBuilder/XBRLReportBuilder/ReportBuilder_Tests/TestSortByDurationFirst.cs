//=============================================================================
// SortByDurationFirst (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// This unit test contains tests for sorting "contexts" based on various test cases.
//=============================================================================

using System;
using System.Collections;

using NUnit.Framework;

using Aucent.MAX.AXE.XBRLParser;
using XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
    [TestFixture] 
    public class TestSortByDurationFirst : SortByDurationFirst
    {

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

		#region PerformSegmentScenarioSort tests
		[Test]
		public void SimpleSegmentTest()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp2.AddSegment( "key2", "key2", "key2", "key2" );

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			// and compare them
			Assert.AreEqual( 1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( -1, Compare( cuw1, cuw2 ), "2 sort wrong" );
			Assert.AreEqual( 0, Compare( cuw1, cuw1 ), "3 sort wrong" );
		}
	
		[Test]
		public void TwoSegsBeforeOneSeg()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp1.AddSegment( "key3", "key3", "key3", "key3" );

			cp2.AddSegment( "key2", "key2", "key2", "key2" );

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			// and compare them
			Assert.AreEqual( 1, CompareSegments( cp2.Segments, cp1.Segments ), "segment sort 1 wrong" );
			Assert.AreEqual( -1, CompareSegments( cp1.Segments, cp2.Segments ), "segment sort 2 wrong" );

			Assert.AreEqual( 1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( -1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void OneSegBeforeTwoScenarios()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddScenario( "key11", "key11", "key11", "key11" );
			cp1.AddScenario( "key3", "key3", "key3", "key3" );

			cp2.AddSegment( "key1", "key1", "key1", "key1" );

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			// and compare them
			Assert.AreEqual( -1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( 1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void DurationScenarioBeforeNoSegNoScenarioInstant()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddScenario( "key1", "key1", "key1", "key1" );
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.PeriodStartDate = new DateTime( 2000,1,1);
			cp1.PeriodEndDate = new DateTime( 2000, 1, 2);

			cp2.PeriodType = Element.PeriodType.instant;
			cp2.PeriodStartDate = new DateTime( 2000,1,1);

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			Assert.AreEqual( 1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( -1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void BothHaveSegsOneWithScenario()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.PeriodStartDate = new DateTime( 2000,1,1);
			cp1.PeriodEndDate = new DateTime( 2000, 1, 2);

			cp1.AddScenario( "key1", "key1", "key1", "key1" );

			cp2.AddSegment( "key1", "key1", "key1", "key1" );
			cp2.PeriodType = Element.PeriodType.duration;
			cp2.PeriodStartDate = new DateTime( 2000,1,1);
			cp2.PeriodEndDate = new DateTime( 2000,1,2);

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			Assert.AreEqual( -1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( 1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void BothHaveSegsAndScenariosCheckDates()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp1.AddScenario( "key9", "key9", "key9", "key9" );
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.PeriodStartDate = new DateTime( 2000,1,1);
			cp1.PeriodEndDate = new DateTime( 2000, 1, 2);

			cp2.AddSegment( "key1", "key1", "key1", "key1" );
			cp2.AddScenario( "key2", "key2", "key2", "key2" );
			cp2.PeriodType = Element.PeriodType.duration;
			cp2.PeriodStartDate = new DateTime( 2000,1,1);
			cp2.PeriodEndDate = new DateTime( 2000, 1, 3);

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			// and compare them - no scen should be first
			Assert.AreEqual( -1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( 1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void BothHaveSegsAndScenariosCheckScenarios()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp1.AddScenario( "key1", "key1", "key1", "key1" );
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.PeriodStartDate = new DateTime( 2000,1,1);
			cp1.PeriodEndDate = new DateTime( 2000, 1, 2);

			cp2.AddSegment( "key1", "key1", "key1", "key1" );
			cp2.AddScenario( "key2", "key2", "key2", "key2" );
			cp2.PeriodType = Element.PeriodType.duration;
			cp2.PeriodStartDate = new DateTime( 2000,1,1);
			cp2.PeriodEndDate = new DateTime( 2000, 1, 2);

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			Assert.AreEqual( 0, CompareSegments( cuw1.contextRef.Segments, cuw2.contextRef.Segments ), "segments are not equal" );
			Assert.AreEqual( 0, CompareDates( cuw1, cuw2 ), "dates are not equal" );
			Assert.AreEqual( 1, CompareScenarios( cuw2.contextRef.Scenarios, cuw1.contextRef.Scenarios ), "scenario 1 sort wrong" );

			Assert.AreEqual( 1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( -1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		
        [Ignore("This is not a valid test?")]
		public void SegVsScenarioScenarioWins()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.AddSegment( "key1", "key1", "key1", "key1" );
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.PeriodStartDate = new DateTime( 2000,1,1);
			cp1.PeriodEndDate = new DateTime( 2000, 1, 2);

			cp2.AddScenario( "key2", "key2", "key2", "key2" );
			cp2.PeriodType = Element.PeriodType.duration;
			cp2.PeriodStartDate = new DateTime( 2000,1,1);
			cp2.PeriodEndDate = new DateTime( 2000, 1, 2);

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			// no segments before segments
			Assert.AreEqual( -1, CompareSegments( cuw1.contextRef.Segments, cuw2.contextRef.Segments ), "segments are not equal" );
			Assert.AreEqual( 0, CompareDates( cuw1, cuw2 ), "dates are not equal" );
			
			// no scenario before scenario
			Assert.AreEqual( -1, CompareScenarios( cuw2.contextRef.Scenarios, cuw1.contextRef.Scenarios ), "scenario 1 sort wrong" );

			Assert.AreEqual( -1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( 1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}
		#endregion

		[Test]
		public void SimpleCompareTest()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.PeriodType = Element.PeriodType.duration;
			cp2.PeriodType = Element.PeriodType.instant;

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			Assert.AreEqual( 1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( -1, Compare( cuw1, cuw2 ), "2 sort wrong" );
		}

		[Test]
		public void SimpleCompareTestWithSegment()
		{
			ContextProperty cp1 = new ContextProperty();
			ContextProperty cp2 = new ContextProperty();
			
			cp1.PeriodType = Element.PeriodType.duration;
			cp1.AddSegment( "key1", "key1", "key1", "key1" );

			cp2.PeriodType = Element.PeriodType.instant;

			MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
			MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

			Assert.AreEqual( -1, Compare( cuw2, cuw1 ), "1 sort wrong" );
			Assert.AreEqual( 1, Compare( cuw1, cuw2 ), "2 sort wrong" );

			ArrayList test = new ArrayList();
			test.Add( cuw1 );
			test.Add( cuw2 );

			test.Sort( new SortByDurationFirst() );

			Assert.AreEqual( cuw2.KeyName, ((MergedContextUnitsWrapper)test[0]).KeyName );
		}

        [Test]
        public void DateTest()
        {
            ContextProperty cp1 = new ContextProperty();
            ContextProperty cp2 = new ContextProperty();

            cp1.PeriodType = Element.PeriodType.duration;
            cp1.PeriodStartDate = new DateTime( 2006, 1, 1 );
            cp1.PeriodEndDate = new DateTime( 2006, 3, 31 );

            cp2.PeriodType = Element.PeriodType.duration;
            cp2.PeriodStartDate = new DateTime( 2006, 4, 1 );
            cp2.PeriodEndDate = new DateTime( 2006, 6, 30 );

            MergedContextUnitsWrapper cuw1 = new MergedContextUnitsWrapper( "key1", cp1 );
            MergedContextUnitsWrapper cuw2 = new MergedContextUnitsWrapper( "key2", cp2 );

            /// x == y -> 0
            /// x > y -> 1
            /// x < y -> -1

            ArrayList arrg = new ArrayList();
            arrg.Add( cuw2 );
            arrg.Add( cuw1 );
            arrg.Sort( new SortByDurationFirst() );

            Assert.AreEqual( cp2.PeriodEndDate, ((MergedContextUnitsWrapper)arrg[0]).contextRef.PeriodEndDate, "index 0 wrong" );
            Assert.AreEqual( cp1.PeriodEndDate, ((MergedContextUnitsWrapper)arrg[1]).contextRef.PeriodEndDate, "index 1 wrong" );

        }

    }
}
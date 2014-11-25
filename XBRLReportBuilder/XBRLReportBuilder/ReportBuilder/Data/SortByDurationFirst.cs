//=============================================================================
// SortByDurationFirst (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
//  This data class implements sorting of reporting periods.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	/// <summary>
	/// SortByDurationFirst
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public  class SortByDurationFirst : IComparer
	{
        public SortByDurationFirst()
        {
        }

        public SortByDurationFirst( bool yearlyReportGeneration )
        {
            YearlyReport = yearlyReportGeneration;
        }

		public SortByDurationFirst(Dictionary<string, NodeSortingWrapper> nodeWrappers)
		{
			this.nodeSortingWrappers = nodeWrappers;
			this.segmentOrderSorter = new SegmentOrderSorter(nodeSortingWrappers);
			this.scenarioOrderSorter = new ScenarioOrderSorter(nodeSortingWrappers);
		}

		#region properties

        public bool YearlyReport = false;
		private Dictionary<string, NodeSortingWrapper> nodeSortingWrappers = null;
		
		private SegmentOrderSorter segmentOrderSorter = null;
		private SegmentValueSorter segmentValueSorter = new SegmentValueSorter();

		private ScenarioOrderSorter scenarioOrderSorter = null;
		private ScenarioValueSorter scenarioValueSorter = new ScenarioValueSorter();

		#endregion

		#region IComparer Members

		/// <summary>
		/// Compare two MergedContextUnitsWrappers
		/// 
		/// Sort order is:
		///	1. Segments
		///		A. If an object has no segments, it goes first
		///		B. If both objects have segments, do type then name comparisons
		///		C. If neither object has segments, do date comparison
		///		D. If both objects have segments, and the segments are equal, then do date comparison.
		///	2. Date comparison
		///		A. Period type of duration before instant
		///		B. If both duration, shorter duration is first
		///		C. If both durations are the same, latest year first, if years are the same, earliest start date
		///		D. If both same, scenario sort
		///	3. Scenario sort
		///		A. If an object has no scenarios, it goes first
		///		B. If both objects have scenarios, do type then name comparisons
		///		C. If neither object has scenarios, they're equal
		///		D. If both objects have scenarios, and the scenarios are equal, they're equal
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns> 
        /// x == y -> 0
        /// x > y -> 1
        /// x < y -> -1
        /// </returns>
		public int Compare(object x, object y)
		{
			IHasContextProperty cuwX = (IHasContextProperty)x;
			IHasContextProperty cuwY = (IHasContextProperty)y;

			if ( cuwX.contextRef.Segments.Count > 0 && cuwY.contextRef.Segments.Count == 0 )
			{
				// if y doesn't have segments or scenario's, it goes first
				// if y has scenarios but x has segments, x goes first
				return cuwY.contextRef.Scenarios.Count == 0 ? 1 : -1;
			}
			else if ( cuwX.contextRef.Segments.Count == 0 && cuwY.contextRef.Segments.Count > 0 )
			{
				// if x doesn't have segments or scenario's, it goes first
				// if x has scenarios but y has segments, y goes first
				return cuwX.contextRef.Scenarios.Count == 0 ? -1 : 1;
			}

			// segment sorting first
			int segRet = CompareSegments( cuwX.contextRef.Segments, cuwY.contextRef.Segments );
			if ( segRet != 0 )
			{
				return segRet;
			}

			// either no segments or same segments, check date

			int dateRet = CompareDates( cuwX, cuwY );
			if ( dateRet != 0 )
			{
				return dateRet;
			}

			// now check scenario's
			return CompareScenarios( cuwX.contextRef.Scenarios, cuwY.contextRef.Scenarios);
		}

		#endregion

		#region Segment compare

		protected int CompareSegments( ArrayList x, ArrayList y )
		{
			if ( x.Count == 0 && y.Count == 0 )
			{
				return 0;
			}

			if (this.segmentOrderSorter != null)
			{
				x.Sort(this.segmentOrderSorter);
				y.Sort(this.segmentOrderSorter);
			}
			else
			{
				x.Sort(this.segmentValueSorter);
				y.Sort(this.segmentValueSorter);
			}

			for ( int i=0; i < x.Count; ++i )
			{
				if ( i > y.Count-1 )
				{
					// x is bigger, return y first
					return 1;
				}

				int segRet = 0;
				if (segmentOrderSorter != null)
				{
					segRet = segmentOrderSorter.Compare(x[i], y[i]);
				}
				else
				{
					segRet = segmentValueSorter.Compare(x[i], y[i]);
				}

				if ( segRet != 0 )
				{
					return segRet;
				}
			}

			// if y is bigger, return x first
			return y.Count > x.Count ? -1 : 0;
		}

		#endregion

		#region scenario comparer

		protected int CompareScenarios( ArrayList x, ArrayList y )
		{
			if ( x.Count == 0 && y.Count == 0 )
			{
				return 0;
			}

			if (this.scenarioOrderSorter != null)
			{
				x.Sort(this.scenarioOrderSorter);
				y.Sort(this.scenarioOrderSorter);
			}
			else
			{
				x.Sort(scenarioValueSorter);
				y.Sort(scenarioValueSorter);
			}

			for ( int i=0; i < x.Count; ++i )
			{
				if ( i > y.Count-1 )
				{
					// x is bigger, return y first
					return 1;
				}


				int scenReturn = 0;
				if (scenarioOrderSorter != null)
				{
					scenReturn = scenarioOrderSorter.Compare(x[i], y[i]);
				}
				else
				{
					scenReturn = scenarioValueSorter.Compare(x[i], y[i]);
				}

				if (scenReturn != 0)
				{
					return scenReturn;
				}
			}

			// if y is bigger, return x first
			return y.Count > x.Count ? -1 : 0;
		}
		#endregion

		#region date compare
		protected int CompareDates( IHasContextProperty cuwX, IHasContextProperty cuwY )
		{
			//duration before instant
			if ( cuwX.contextRef.PeriodType != cuwY.contextRef.PeriodType )
			{
				return cuwX.contextRef.PeriodType == Element.PeriodType.duration ? -1 : 1;
			}
			else if ( cuwX.contextRef.PeriodType == Element.PeriodType.duration )
			{
				// if both cuwX and cuwY are duration - put the shorter one first
				// if equal periods, just fall through

                int spanRetVal = 0;
                if ( !YearlyReport )
                {
                    TimeSpan xSpan = cuwX.contextRef.PeriodEndDate - cuwX.contextRef.PeriodStartDate;
                    TimeSpan ySpan = cuwY.contextRef.PeriodEndDate - cuwY.contextRef.PeriodStartDate;

                    // if xspan and yspan are within 5 days of each other, consider them equal 

                    TimeSpan diff = ySpan - xSpan;

                    //diff = diff.Add( new TimeSpan( 10, 0, 0, 0, 0 ) );

                    if ( diff.Days < -15 || diff.Days > 15 )
                    {
                        // we're outside the range, return the actual value
                        // smaller duration first
                        return xSpan.CompareTo( ySpan );
                    }

                    // otherwise, consider them equal and fall through

                    // first compare years, take the latest first
                    if ( cuwX.contextRef.PeriodEndDate.Year != cuwY.contextRef.PeriodEndDate.Year )
                    {
                        if ( cuwX.contextRef.PeriodEndDate.Year > cuwY.contextRef.PeriodEndDate.Year )
                        {
                            return -1;
                        }

                        return 1;
                    }
                }
				// if span's are equal, go against the latest date
                spanRetVal = DateTimeCompareTo( cuwX.contextRef.PeriodEndDate, cuwY.contextRef.PeriodEndDate );
				if ( spanRetVal != 0 )
				{
                    // most recent first
                    return spanRetVal * -1;
                    //return YearlyReport ? spanRetVal*-1 : spanRetVal;
				}
			}

			// if we get here they're the same period type and if duration, with the same timespan
			IHasContextProperty cuwLeftHand = null;
			IHasContextProperty cuwRightHand = null;
 
			cuwLeftHand = cuwY;
			cuwRightHand = cuwX;

			// if they're duration, check start and end dates
			if ( cuwLeftHand.contextRef.PeriodType == Element.PeriodType.duration )
			{
				// if start or end date is the same,we need to sort by duration length.
				// the shorter durations are on the left (come first) 
				// and the longer durations are on the right(come last).
				if ( cuwLeftHand.contextRef.PeriodStartDate.Ticks == cuwRightHand.contextRef.PeriodStartDate.Ticks )
				{
					return DateTimeCompareTo( cuwRightHand.contextRef.PeriodEndDate, cuwLeftHand.contextRef.PeriodEndDate );
				}
				else if ( cuwLeftHand.contextRef.PeriodEndDate.Ticks == cuwRightHand.contextRef.PeriodEndDate.Ticks )
				{
					return DateTimeCompareTo( cuwLeftHand.contextRef.PeriodStartDate, cuwRightHand.contextRef.PeriodStartDate );
				}
				else
				{
					// just check end dates
					return DateTimeCompareTo( cuwLeftHand.contextRef.PeriodEndDate, cuwRightHand.contextRef.PeriodEndDate );
				}
			}

			// if they're instants, only check start dates
			if ( cuwLeftHand.contextRef.PeriodType == Element.PeriodType.instant )
			{
				return DateTimeCompareTo( cuwLeftHand.contextRef.PeriodStartDate, cuwRightHand.contextRef.PeriodStartDate );
			}

			return 0;
		}

        protected int DateTimeCompareTo( DateTime x, DateTime y )
        {
            return x.CompareTo( y );
        }

		#endregion

	}

	public class NodeSortingWrapper
	{
		private Node theNode;
		public Node TheNode
		{
			get { return theNode; }
			set { theNode = value; }
		}

		private int level;
		public int Level
		{
			get { return level; }
			set { level = value; }
		}

		public NodeSortingWrapper(Node node, int nodeLevel)
		{
			this.theNode = node;
			this.level = nodeLevel;
		}
	}
}
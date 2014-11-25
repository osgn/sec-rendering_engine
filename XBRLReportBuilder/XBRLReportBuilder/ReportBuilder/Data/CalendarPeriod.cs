using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLParser;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	[Serializable]
	public class CalendarPeriod : IComparable
	{
		#region enums/properties

		//public enum PeriodTypeCode {Duration, Instant, Forever, Unknown};
		public Element.PeriodType PeriodType = Element.PeriodType.na;

		public DateTime StartDate = DateTime.MinValue;
		public DateTime EndDate = DateTime.MinValue;

		[XmlIgnore]
		public TimeSpan Span
		{
			get { return EndDate - StartDate; }
		}
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new <see cref="CalendarPeriod"/>.
		/// </summary>
		public CalendarPeriod()
		{
		}

        /// <summary>
        /// Creates a new <see cref="CalendarPeriod"/> of instant duration with
        /// <paramref name="asOfDate"/> indicating the date.
        /// </summary>
        /// <param name="asOfDate">The <see cref="DateTime"/> with which to
        /// iniate the object.</param>
		public CalendarPeriod( DateTime asOfDate )
		{
			this.PeriodType = Element.PeriodType.instant;
			this.StartDate = asOfDate;
		}

        /// <summary>
        /// Creates a new <see cref="CalendarPeriod"/> of a duration from
        /// <paramref name="startDate"/> to <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">The <see cref="DateTime"/> on which to
        /// begin the duration.</param>
        /// <param name="endDate"></param>
		public CalendarPeriod( DateTime startDate, DateTime endDate )
		{
			this.PeriodType = Element.PeriodType.duration;
			this.StartDate = startDate;
			this.EndDate = endDate;
		}

		#endregion

		public override bool Equals( object obj )
		{
			CalendarPeriod cp = obj as CalendarPeriod;
			if( cp == null )
				return false;

			if( this.PeriodType != cp.PeriodType )
				return false;

			if( this.StartDate != cp.StartDate )
				return false;

			if( this.EndDate != cp.EndDate )
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public const string DATE_FORMAT = "M/d/yyyy";
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if( this.PeriodType == Element.PeriodType.instant ) //instant
				return this.StartDate.ToString( DATE_FORMAT );
			else if( DateTime.Equals( this.StartDate, this.EndDate ) )
				return this.StartDate.ToString( DATE_FORMAT );
			else
				return string.Format( "{0} - {1}", this.StartDate.ToString( DATE_FORMAT ), this.EndDate.ToString( DATE_FORMAT ) );
		}

		#region IComparable Members

        /// <summary>
        /// Implementation of IComparable method CompareTo.
        /// </summary>
        /// <param name="obj">Object to compare against.</param>
        /// <returns>An <see cref="int"/> indicating the results of the
        /// comparison. A value of 0 indicates the objects are equal.</returns>
		public int CompareTo( object obj )
		{
			return Compare( this, obj as CalendarPeriod );
		}

		#endregion

        /// <summary>
        /// Sorts the two given <see cref="CalendarPeriod"/> inputs for use on
        /// columns.
        /// </summary>
        /// <param name="cpLeft">The left side comparison operand</param>
        /// <param name="cpRight">The right side comparison operand</param>
        /// <returns>An <see cref="int"/> indicating if <paramref
        /// name="cpLeft"/> occurs after (1), before (-1), or same as (0)
        /// <paramref name="cpRight"/>.</returns>
		public static int AscendingSorter( CalendarPeriod cpLeft, CalendarPeriod cpRight )
		{
			if( cpLeft == null )
			{
				if( cpRight == null )
					return 0;
				else
					return 1;
			}
			else if( cpRight == null )
			{
				return -1;
			}

			int cmp = 0;
			if( cpLeft.PeriodType == cpRight.PeriodType )
			{
				cmp = DateTime.Compare( cpLeft.StartDate, cpRight.StartDate );

				if( cmp == 0 )
					cmp = DateTime.Compare( cpLeft.EndDate, cpRight.EndDate );

				return cmp;
			}

			CalendarPeriod cpDuration = null;
			CalendarPeriod cpInstant = null;
			if( cpLeft.PeriodType == Element.PeriodType.instant )
			{
				cpDuration = cpRight;
				cpInstant = cpLeft;
			}
			else
			{
				cpDuration = cpLeft;
				cpInstant = cpRight;
			}

			cmp = DateTime.Compare( cpInstant.StartDate, cpDuration.EndDate );
			if( cmp == 0 )
			{
				if( cpLeft == cpInstant )
					return -1;
				else
					return 1;
			}

			return cmp;
		}

        /// <summary>
        /// Sorts the two given <see cref="CalendarPeriod"/> inputs for use on
        /// rows.
        /// </summary>
        /// <param name="cpLeft">The left side comparison operand</param>
        /// <param name="cpRight">The right side comparison operand</param>
        /// <returns>An <see cref="int"/> indicating if <paramref
        /// name="cpLeft"/> occurs after (-1), before (1), or same as (0)
        /// <paramref name="cpRight"/>.</returns>
		public static int ReportSorter( CalendarPeriod cpLeft, CalendarPeriod cpRight )
		{
			if( cpLeft == null )
			{
				if( cpRight == null )
					return 0;
				else
					return 1;
			}
			else if( cpRight == null )
			{
				return -1;
			}



			int cmp = 0;
			if( cpLeft.PeriodType != cpRight.PeriodType )
			{
				if( cpLeft.PeriodType == Element.PeriodType.instant )
					return 1;  //instants last
				else
					return -1; //durations first
			}

			if( cpLeft.PeriodType == Element.PeriodType.instant )
				return DateTime.Compare( cpLeft.StartDate, cpRight.StartDate );

			cmp = TimeSpan.Compare( cpLeft.Span, cpLeft.Span );
			if( cmp != 0 )
				return cmp;

			//we actually want the opposite: newer == -1
			cmp = DateTime.Compare( cpLeft.EndDate, cpRight.EndDate );
			return cmp * -1;
		}

        /// <summary>
        /// Compares two <see cref="CalendarPeriod"/> objects.
        /// </summary>
        /// <param name="cpLeft">The first object to compare</param>
        /// <param name="cpRight">The second object to compare</param>
        /// <returns>An integer indicating the results of the comparison.  A
        /// value of 0 indicates the objects are identical.</returns>
		public static int Compare( CalendarPeriod cpLeft, CalendarPeriod cpRight )
		{
			if( cpRight == null ) return -1;

			int result = cpLeft.PeriodType.CompareTo( cpRight.PeriodType );
			if( result != 0 ) return result;

			result = cpLeft.StartDate.CompareTo( cpRight.StartDate );
			if( result != 0 ) return result;

			return cpLeft.EndDate.CompareTo( cpRight.EndDate );
		}
	}
}

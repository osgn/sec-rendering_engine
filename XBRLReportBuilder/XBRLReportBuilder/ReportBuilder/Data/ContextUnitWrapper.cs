//=============================================================================
// ContextUnitWrapper (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// This data class stores merged contect unit information.
//=============================================================================

using System;
using System.Text;
using System.Collections;

using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;

namespace XBRLReportBuilder
{
	/// <summary>
	/// ContextUnitWrapper
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class ContextUnitWrapper : IComparable
	{
		#region properties

		public string			KeyName;
		public ContextProperty	CP;
		public UnitProperty		UP;
        public string CurrencySymbol = InstanceUtils.USDCurrencySymbol;// "$";//default to USD

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new ContextUnitWrapper.
		/// </summary>
		public ContextUnitWrapper(string keyName, ContextProperty cp, UnitProperty up)
		{
			this.KeyName = keyName;
			this.CP = cp;
			this.UP = up;
		}       

		#endregion

		#region overrides

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(TraceUtility.FormatStringResource("DragonView.Data.EntityIDColon",this.CP.EntityValue));
			sb.Append(Environment.NewLine);

			foreach (Segment s in CP.Segments)
			{
				sb.Append(s.ValueType).Append(": ").Append(s.ValueName).Append(Environment.NewLine);
			}

			if (CP.PeriodEndDate.Year == 1)//instant
			{
				sb.Append (TraceUtility.FormatStringResource("DragonView.Data.AsOfDate", CP.PeriodStartDate.ToShortDateString()));
			}
			else
			{
				sb.Append(TraceUtility.FormatStringResource("DragonView.Data.FromToDates", CP.PeriodStartDate.ToShortDateString(), CP.PeriodEndDate.ToShortDateString()));
			}
			sb.Append (Environment.NewLine);

			if (UP != null)
			{
				sb.Append(TraceUtility.FormatStringResource("DragonView.Data.UnitColon", UP.UnitID)).Append (Environment.NewLine);
			}

			foreach (Scenario s in CP.Scenarios)
			{
				sb.Append(s.ValueType).Append(": ").Append(s.ValueName).Append(Environment.NewLine);
			}

			return sb.ToString();
		}

		#endregion

		#region public methods

		public string ToStringForFootnote(string tupleSetName)
		{
			StringBuilder sb = new StringBuilder();

            if (tupleSetName != string.Empty)
            {
                sb.Append(KeyName).Append("_").Append(tupleSetName).Append("|");
            }
            else
            {
                sb.Append(KeyName).Append("|");
            }

			foreach (Segment s in CP.Segments)
			{
				sb.Append(s.ValueType).Append(": ").Append(s.ValueName).Append(Environment.NewLine);
			}

			if (CP.PeriodEndDate.Year == 1)//instant
			{
				sb.Append (TraceUtility.FormatStringResource("DragonView.Data.AsOfDate", CP.PeriodStartDate.ToShortDateString()));
			}
			else
			{
				sb.Append(TraceUtility.FormatStringResource("DragonView.Data.FromToDates", CP.PeriodStartDate.ToShortDateString(), CP.PeriodEndDate.ToShortDateString()));
			}
			sb.Append (Environment.NewLine);

            //if (UP != null)
            //{
            //    sb.Append(TraceUtility.FormatStringResource("DragonView.Data.UnitColon", UP.UnitID)).Append (Environment.NewLine);
            //}

			foreach (Scenario s in CP.Scenarios)
			{
				sb.Append(s.ValueType).Append(": ").Append(s.ValueName).Append(Environment.NewLine);
			}

			return sb.ToString();
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object inObj)
		{
			return this.KeyName.CompareTo ((inObj as ContextUnitWrapper).KeyName);
		}

		#endregion

		#region sorter classes

		public sealed class SortByDate : IComparer
		{
			#region properties

//			private UserOptions.CalendarSort calendarSortOption = UserOptions.CalendarSort.ByYearAscending;
//			public UserOptions.CalendarSort CalendarSortOption
//			{
//				get {return calendarSortOption;}
//				set {calendarSortOption = value;}
//			}

			#endregion

			#region IComparer Members

			public int Compare(object x, object y)
			{
				ContextUnitWrapper cuwX = (ContextUnitWrapper)x;
				ContextUnitWrapper cuwY = (ContextUnitWrapper)y;

				if ( cuwX.CP.PeriodType != cuwY.CP.PeriodType )
				{
					if ( cuwX.CP.PeriodType == Element.PeriodType.duration )
						return -1;
					else
						return 1;
				}

				// if we get here they're the same period type

				ContextUnitWrapper cuwLeftHand = null;
				ContextUnitWrapper cuwRightHand = null;
				// set left hand side and right hand side of the variables depending on
				// whether we're sorting ascending or descending.
//				if ( this.calendarSortOption == UserOptions.CalendarSort.ByYearAscending )
//				{
//					cuwLeftHand = cuwX;
					cuwRightHand = cuwY;
//				}
//				else
//				{
//					cuwLeftHand = cuwY;
//					cuwRightHand = cuwX;
//				}

				// if they're duration, check start and end dates
				if ( cuwLeftHand.CP.PeriodType == Element.PeriodType.duration )
				{
					// if start or end date is the same,we need to sort by duration length.
					// the shorter durations are on the left (come first) 
					// and the longer durations are on the right(come last).
					if ( cuwLeftHand.CP.PeriodStartDate.Equals( cuwRightHand.CP.PeriodStartDate ) )
					{
//						if ( this.calendarSortOption == UserOptions.CalendarSort.ByYearAscending )
//						{
							return cuwLeftHand.CP.PeriodEndDate.CompareTo( cuwRightHand.CP.PeriodEndDate );
//						}
//						else
//						{
//							 reverse the compare to if end dates are the same so that we can sort
//							 shorter duratiosn to up to the front
//							return cuwRightHand.CP.PeriodEndDate.CompareTo( cuwLeftHand.CP.PeriodEndDate );
//						}
					}
					else if ( cuwLeftHand.CP.PeriodEndDate.Equals( cuwRightHand.CP.PeriodEndDate ) )
					{
//						if ( this.calendarSortOption == UserOptions.CalendarSort.ByYearAscending )
//						{
							// reverse the compare to if end dates are the same so that we can sort
							// shorter duratiosn to up to the front
							return cuwRightHand.CP.PeriodStartDate.CompareTo( cuwLeftHand.CP.PeriodStartDate );
//						}
//						else
//						{
//							return cuwLeftHand.CP.PeriodStartDate.CompareTo( cuwRightHand.CP.PeriodStartDate );
//						}
					}
					else
					{
						// just check end dates
						return cuwLeftHand.CP.PeriodEndDate.CompareTo( cuwRightHand.CP.PeriodEndDate );
					}
				}

				// if they're instants, only check start dates
				if ( cuwLeftHand.CP.PeriodType == Element.PeriodType.instant )
				{
					return cuwLeftHand.CP.PeriodStartDate.CompareTo( cuwRightHand.CP.PeriodStartDate );
				}

				return 0;
			}

			#endregion
		}

		#endregion

	}
}
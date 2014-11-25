using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XBRLReportBuilder;
using Aucent.MAX.AXE.Common.Data;
using XBRLReportBuilder.Utilities;
using Aucent.MAX.AXE.XBRLParser;
using System.Diagnostics;
using System.Collections;

namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data
{
	/// <summary>
	/// Helper class which hides the complexities of transforming a "Standard" report into the "Equity" layout.
	/// </summary>
	public class EquityDataSet
	{
		public InstanceReport EquityCandidate = new InstanceReport();


		public List<InstanceReportColumn> SegmentColumns
		{
			get { return this.EquityCandidate.Columns; }
		}

		private List<InstanceReportRow> BBRows;
		private List<InstanceReportRow> RegRows;
		private List<InstanceReportRow> EBRows;

		private List<Segment> AdjustmentMembers;
		private List<Segment> PreviousMembers;

		private List<InstanceReportColumn> DurationCalendars;
		private List<InstanceReportColumn> InstantCalendars;

		private XmlDocument AdjustedAndPRMemberLookup = null;

        /// <summary>
        /// Sets the <see cref="AdjustedAndPRMemberLookup"/> to the given
        /// <see cref="membersXDoc"/>.
        /// </summary>
        /// <param name="membersXDoc">The XML document for the members to set
        /// for the equity report.</param>
		public EquityDataSet( XmlDocument membersXDoc )
		{
			this.AdjustedAndPRMemberLookup = membersXDoc;
		}

        /// <summary>
        /// Perform some manipulation on the equity report to clean it up, sort
        /// it, and shift around segments, and shift items between rows and
        /// columns.
        /// </summary>
        /// <param name="baseReport">The base report upon which the equity
        /// report is being processed.</param>
		public void AdjustEquityPeriods( InstanceReport baseReport )
		{
			Dictionary<string, EquityPeriod> periodGroupings = new Dictionary<string, EquityPeriod>();
			foreach( InstanceReportRow row in this.EquityCandidate.Rows )
			{
				if( !periodGroupings.ContainsKey( row.EmbedRequirements.Period.ToString() ) )
					periodGroupings[ row.EmbedRequirements.Period.ToString() ] = new EquityPeriod();

				periodGroupings[ row.EmbedRequirements.Period.ToString() ].Rows.Add( row );
			}

			List<string> keysToRemove = new List<string>();
			foreach( KeyValuePair<string, EquityPeriod> kvp in periodGroupings )
			{
				kvp.Value.DistributeRows();
				kvp.Value.RemoveEmptyRows();
				kvp.Value.RemoveActivityStyles();

				if( kvp.Value.AreBalancesEmpty() )
					keysToRemove.Add( kvp.Key );
				else if( kvp.Value.IsActivityEmpty() )
					keysToRemove.Add( kvp.Key );
				else if( !kvp.Value.HasSegmentedData( this.EquityCandidate.Columns ) )
				    keysToRemove.Add( kvp.Key );
			}

			foreach( string key in keysToRemove )
			{
				periodGroupings.Remove( key );
			}

			EquityPeriod nextPeriod = null;
			this.EquityCandidate.Rows.Clear();
			List<string> periodStrings = new List<string>( periodGroupings.Keys );
			if( periodStrings.Count == 0 )
				return;

			if( periodStrings.Count == 1 )
			{
				nextPeriod = periodGroupings[ periodStrings[ 0 ] ];
				nextPeriod.DistributeRows();
				nextPeriod.Sort( baseReport );
			}
			else
			{
				for( int k = 0; k < periodStrings.Count - 1; k++ )
				{
					string thisPK = periodStrings[ k ];
					EquityPeriod thisPeriod = periodGroupings[ thisPK ];
					thisPeriod.DistributeRows();

					string nextPK = periodStrings[ k + 1 ];
					nextPeriod = periodGroupings[ nextPK ];
					nextPeriod.DistributeRows();

					if( thisPeriod.EndDateMatchesBeginDate( nextPeriod ) )
						thisPeriod.MergeAndReduceBeginningBalances( nextPeriod );

					thisPeriod.Sort( baseReport );
					this.EquityCandidate.Rows.AddRange( thisPeriod.Rows );
				}

				nextPeriod.Sort( baseReport );
			}

			this.EquityCandidate.Rows.AddRange( nextPeriod.Rows );
		}

        /// <summary>
        /// Clean up equity columns, fixing some symbols and items in the
        /// header, arranging the total columns correctly, and processing date
        /// style labels.
        /// </summary>
		public void CleanupEquityColumns()
		{
			foreach( InstanceReportColumn col in this.EquityCandidate.Columns )
			{
				col.CurrencyCode = col.EmbedRequirements.UnitCode;
				col.CurrencySymbol = col.EmbedRequirements.UnitSymbol;
				col.ApplyCurrencyLabel();

				col.Segments = new ArrayList();
				if( col.EmbedRequirements.Segments == null ||
					col.EmbedRequirements.Segments.Count == 0 )
					continue;

				foreach( Segment seg in col.EmbedRequirements.Segments )
				{
					if( !seg.IsDefaultForEntity )
						col.Segments.Add( seg );
				}
			}

			//now shift them to the end
			foreach( InstanceReportColumn col in this.EquityCandidate.Columns )
			{
				if( !col.hasSegments )
				{
					if( col.Labels.Count == 0 )
					{
						col.Labels.Add( new LabelLine( 0, InstanceReport.EQUITY_TOTAL_HEADER ) );
					}
					else
					{
						col.Labels[ 0 ].Id++;
						col.Labels.Insert( 0, new LabelLine( 0, InstanceReport.EQUITY_TOTAL_HEADER ) );
					}
				}

				//Do not move the total column - follow the presentation
				//
				//int at = this.EquityCandidate.Columns.IndexOf( col );
				//this.EquityCandidate.Columns.Remove( col );
				//this.EquityCandidate.Columns.Add( col );

				//foreach( InstanceReportRow row in this.EquityCandidate.Rows )
				//{
				//    row.Cells.Add( row.Cells[ at ] );
				//    row.Cells.RemoveAt( at );
				//}
			}
		}

        /// <summary>
        /// Cleans up the rows in an equity report, processing labels, and
        /// removing unused periods.
        /// </summary>
        /// <param name="dateFormat">The date format with which to process
        /// labels on the equity rows.</param>
        /// <param name="membersXDoc">An XML document containing the members
        /// within the report.</param>
		public void CleanupEquityRows( string dateFormat, XmlDocument membersXDoc )
		{
			//SOI creates new rows for every instant calendar too - but we only want the durations
			this.EquityCandidate.Rows.RemoveAll( row => row.EmbedRequirements.Period.PeriodType == Element.PeriodType.instant );

			//transfer the balance flags
			foreach( InstanceReportRow row in this.EquityCandidate.Rows )
			{
				row.Labels.Clear();
				foreach( LabelLine ll in row.EmbedRequirements.ElementRow.Labels )
				{
					LabelLine copy = (LabelLine)ll.Clone();
					row.Labels.Add( copy );
				}

				if( row.EmbedRequirements.IsAdjusted( membersXDoc ) )
					row.IsEquityAdjustmentRow = true;

				if( row.EmbedRequirements.IsAsPreviouslyReported( membersXDoc ) )
					row.IsEquityPrevioslyReportedAsRow = true;

				//=======================Do not update abstracts with dates or labels===================
				if( row.IsAbstractGroupTitle )
					continue;

				if( row.IsBeginningBalance ||
					row.IsEndingBalance ||
					row.IsEquityAdjustmentRow ||
					row.IsEquityPrevioslyReportedAsRow )
				{
					row.IsCalendarTitle = true;

					//apply the balance date...
					row.ApplyBalanceDate();

					//but only show it for the non-adjustments
					if( !row.IsEquityAdjustmentRow )
						row.ApplyBalanceDateLabel( dateFormat );

					if( row.EmbedRequirements.Segments == null )
						continue;

					foreach( Segment seg in row.EmbedRequirements.Segments )
					{
						if( seg.IsDefaultForEntity )
							continue;

						row.ApplySpecialLabel( seg );
					}
				}
			}
		}

        /// <summary>
        /// Load the calendar columns from the base report.
        /// </summary>
        /// <param name="baseReport">The base report to load calendar items
        /// from.</param>
		public void LoadCalendarColumns( InstanceReport baseReport )
		{
			List<InstanceReportColumn> uniqueCalendars = new List<InstanceReportColumn>();
			foreach( InstanceReportColumn baseCol in baseReport.Columns )
			{
				if( !ReportUtils.Exists( uniqueCalendars, col => col.ReportingPeriodEquals( baseCol ) ) )
				{
					//create a clone so that we can slowly digest this data without affecting other references
					InstanceReportColumn newCol = (InstanceReportColumn)baseCol.Clone();
					uniqueCalendars.Add( newCol );
				}
			}

			uniqueCalendars.Sort( InstanceReport.CompareCalendars );
			this.DurationCalendars = new List<InstanceReportColumn>( uniqueCalendars );
			this.InstantCalendars = SplitCalendars( uniqueCalendars );
		}

        /// <summary>
        /// Pull the rows from the <paramref name="baseReport"/> into the
        /// respective lists for each type of balance.
        /// </summary>
        /// <param name="baseReport">The base report to load rows from.</param>
		public void LoadRowTypes( InstanceReport baseReport )
		{
			this.BBRows = baseReport.Rows.FindAll( row => row.IsBeginningBalance );
			this.RegRows = baseReport.Rows.FindAll( row => !( row.IsBeginningBalance || row.IsEndingBalance ) );
			this.EBRows = baseReport.Rows.FindAll( row => row.IsEndingBalance );
		}

        /// <summary>
        /// Looks through each segment of each axis in the given report to
        /// determine which is column/row based, then passes the results on for
        /// embed processing.  The result is stored in <see
        /// cref="EquityCandidate"/>.
        /// </summary>
        /// <param name="baseReport">The <see cref="InstanceReport"/> upon
        /// which to generate an equity style report.</param>
        /// <returns>A <see cref="bool"/> indicating success.</returns>
		public bool PopulateEquityReport( InstanceReport baseReport )
		{
			Dictionary<string, CommandIterator> columnCommands = new Dictionary<string, CommandIterator>();
			foreach( string axis in baseReport.AxisByPresentation )
			{
				bool axisFound = false;
				foreach( InstanceReportColumn segmentColumn in this.SegmentColumns )
				{
					if( segmentColumn.Segments == null || segmentColumn.Segments.Count == 0 )
						continue;

					foreach( Segment seg in segmentColumn.Segments )
					{
						if( string.Equals( seg.DimensionInfo.dimensionId, axis ) )
						{
							CommandIterator ci = new CommandIterator( CommandIterator.IteratorType.Column, axis, CommandIterator.StyleType.Compact, "*" );
							columnCommands[ ci.AxisName ] = ci;
							axisFound = true;
						}
					}

					if( axisFound )
						break;
				}
			}

			CommandIterator unitCmd = new CommandIterator( CommandIterator.IteratorType.Column, "unit", CommandIterator.StyleType.Grouped, "*" );
			columnCommands[ unitCmd.SelectionString ] = unitCmd;





			Dictionary<string, CommandIterator> rowCommands = new Dictionary<string, CommandIterator>();
			CommandIterator calCmd = new CommandIterator( CommandIterator.IteratorType.Row, "period", CommandIterator.StyleType.Grouped, "*" );
			rowCommands[ calCmd.SelectionString ] = calCmd;

			CommandIterator elCmd = new CommandIterator( CommandIterator.IteratorType.Row, "primary", CommandIterator.StyleType.Compact, "*" );
			rowCommands[ elCmd.SelectionString ] = elCmd;

			foreach( string axis in baseReport.AxisByPresentation )
			{
				if( columnCommands.ContainsKey( axis ) )
					continue;

				if( rowCommands.ContainsKey( axis ) )
					continue;

				CommandIterator specCmd = new CommandIterator( CommandIterator.IteratorType.Row, axis, CommandIterator.StyleType.Compact, "*" );
				rowCommands[ specCmd.AxisName ] = specCmd;
			}


			EmbedReport equityTransform = new EmbedReport();
			List<CommandIterator> colCmds = new List<CommandIterator>( columnCommands.Values );
			equityTransform.ColumnIterators = colCmds.ToArray();

			List<CommandIterator> rowCmds = new List<CommandIterator>( rowCommands.Values );
			equityTransform.RowIterators = rowCmds.ToArray();

			equityTransform.AxisByPresentation = baseReport.AxisByPresentation;
			if( equityTransform.ProcessEmbedCommands( baseReport, null ) )
			{
				this.EquityCandidate = equityTransform.InstanceReport;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates and saves a list of unique columns based on their segments and scenarios.
		/// </summary>
		/// <param name="baseReport">The base report to load segment scenarios
		/// from.</param>
		public void LoadSegmentScenarioColumns( InstanceReport baseReport )
		{
			Dictionary<string, Segment> uniqueAdjustments = new Dictionary<string, Segment>();
			Dictionary<string, Segment> uniquePrevious = new Dictionary<string, Segment>();


			List<InstanceReportColumn> consolidatedColumns = new List<InstanceReportColumn>();
			List<InstanceReportColumn> uniqueSegmentColumns = new List<InstanceReportColumn>();
			foreach( InstanceReportColumn baseColumn in baseReport.Columns )
			{
				//create a clone so that we can digest this data without affecting other references
				InstanceReportColumn clone = (InstanceReportColumn)baseColumn.Clone();

				int index;
				if( clone.IsAdjusted( this.AdjustedAndPRMemberLookup, out index ) )
				{
					Segment seg = (Segment)clone.Segments[ index ];
					uniqueAdjustments[ seg.DimensionInfo.dimensionId ] = seg;

					clone.RemoveAdjustedPreviouslyReported( this.AdjustedAndPRMemberLookup );
					clone.RemoveMissingSegmentLabels();
				}
				else if( clone.IsAsPreviouslyReported( this.AdjustedAndPRMemberLookup, out index ) )
				{
					Segment seg = (Segment)clone.Segments[ index ];
					uniquePrevious[ seg.DimensionInfo.dimensionId ] = seg;

					clone.RemoveAdjustedPreviouslyReported( this.AdjustedAndPRMemberLookup );
					clone.RemoveMissingSegmentLabels();
				}


				bool exists = ReportUtils.Exists( uniqueSegmentColumns,
				tmp =>
				{
					if( !tmp.SegmentAndScenarioEquals( clone ) )
						return false;

					if( !tmp.CurrencyEquals( clone ) )
						return false;

					return true;
				} );

				if( !exists )
				{
					if( clone.Segments == null || clone.Segments.Count == 0 )
						consolidatedColumns.Add( clone );

					uniqueSegmentColumns.Add( clone );
				}
			}

			if( consolidatedColumns != null && consolidatedColumns.Count > 0 )
			{
				foreach( InstanceReportColumn cCol in consolidatedColumns )
				{
					uniqueSegmentColumns.Remove( cCol );
					uniqueSegmentColumns.Add( cCol );
					cCol.Labels[ 0 ].Label = InstanceReport.EQUITY_TOTAL_HEADER;
				}
			}


			this.EquityCandidate.Columns.AddRange( uniqueSegmentColumns );

			//now clean off the calendars
			this.SegmentColumns.ForEach( col => col.ClearReportingPeriod() );

			//and sort according to presentation
			this.SegmentColumns.Sort( baseReport.CompareSegments );

			this.AdjustmentMembers = new List<Segment>( uniqueAdjustments.Values );
			this.PreviousMembers = new List<Segment>( uniquePrevious.Values );
		}

		/// <summary>
		/// Splits a list of calendar columns into instant and duration.
		/// </summary>
		/// <param name="allCalendars"></param>
		/// <returns></returns>
		private static List<InstanceReportColumn> SplitCalendars( List<InstanceReportColumn> allCalendars )
		{
			List<InstanceReportColumn> instantCalendars = allCalendars.FindAll( cal => cal.MyContextProperty.PeriodType == Element.PeriodType.instant );
			allCalendars.RemoveAll( cal => instantCalendars.Contains( cal ) );
			return instantCalendars;
		}
	}
}

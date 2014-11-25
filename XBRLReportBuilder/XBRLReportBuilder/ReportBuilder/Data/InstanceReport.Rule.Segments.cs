using System.Collections.Generic;
using Aucent.MAX.AXE.Common.Data;
using XBRLReportBuilder.Utilities;
using System.Text.RegularExpressions;
using System;
using Aucent.MAX.AXE.XBRLParser;
using System.Collections;
using System.Diagnostics;
namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{

		public bool ProcessSegments()
		{
			bool processedSegments = false;
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.SEGMENTS_RULE );
			if( isRuleEnabled )
			{
				BooleanWrapper processed = new BooleanWrapper( false );

				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				contextObjects.Add( "Processed", processed );

				builderRules.ProcessRule( RulesEngineUtils.SEGMENTS_RULE, contextObjects );

				processedSegments = processed.Value;

				foreach( InstanceReportRow irr in this.Rows )
				{
					if( irr.IsSegmentTitle )
					{
						irr.Label = Regex.Replace( irr.Label, @"\{.*?\} : ", String.Empty ).Trim();
					}
				}

				this.FireRuleProcessed( RulesEngineUtils.SEGMENTS_RULE );
			}
			return processedSegments;
		}

		public bool ProcessSegments_Rule( BooleanWrapper processed )
		{
			//collect all of the segments to be permuted vertically
			List<InstanceReportColumn> uniqueSegmentColumns = this.GetSegmentScenarioColumnsForSegmentProcessing();
			//if( uniqueSegmentColumns.Count < 2 )
			//	return false;

			//collect all of the calendars to be retained horizontally
			Dictionary<int, List<InstanceReportColumn>> calendarsBySegment = this.GetSegmentScenarioCalendars( uniqueSegmentColumns );
			//if( uniqueSegmentColumns.Count < 2 )
			//	return false;

			//find the set of `allCalendars` which will hold all data without any adjustment
			List<InstanceReportColumn> allCalendars;
			bool doCalendarsOverlap = DoCalendarsOverlap( calendarsBySegment, out allCalendars );
			if( !doCalendarsOverlap )
				return false;


			List<Segment> commonSegmentsToPromote = GetCommonSegments( uniqueSegmentColumns );
			PromoteConsolidatedSegment( uniqueSegmentColumns, commonSegmentsToPromote );

			//set up a temporary holder for the rows - this helps with debugging
			InstanceReport segmentCandidate = new InstanceReport();
			segmentCandidate.Columns.AddRange( allCalendars );

			//The first row in every report is usually `IsReportTitle` - copy it to the temporary report
			InstanceReportRow reportTitleRow = this.FindCloneAndTruncate( row => row.IsReportTitle, allCalendars.Count );
			if( reportTitleRow != null )
			{
				reportTitleRow.Id = 0;
				segmentCandidate.Rows.Add( reportTitleRow );
			}

			//Now, for every `segmentSet`, rebuild the data vertically
			foreach( InstanceReportColumn segmentSet in uniqueSegmentColumns )
			{
				//If this segment set has segments, create an `IsSegmentTitle` row
				if( segmentSet.Segments != null && segmentSet.Segments.Count > 0 )
				{
					string label = string.Empty;
					foreach( Segment seg in segmentSet.Segments )
					{
						if( commonSegmentsToPromote != null && commonSegmentsToPromote.Count > 0 )
						{
							bool isCommon = ReportUtils.Exists( commonSegmentsToPromote, cSeg => cSeg.Equals( seg ) );
							if( isCommon )
								continue;
						}

						if( !string.IsNullOrEmpty( label ) )
							label += " | ";

						label += seg.ValueName;
					}

					if( !string.IsNullOrEmpty( label ) )
					{
						InstanceReportRow segmentRow = new InstanceReportRow( label, allCalendars.Count );
						segmentRow.IsSegmentTitle = true;
						segmentRow.Id = segmentCandidate.Rows.Count;
						segmentRow.OriginalInstanceReportColumn = (InstanceReportColumn)segmentSet.Clone();

						segmentCandidate.Rows.Add( segmentRow );
					}
				}

				//`segmentSets` combined with `rows` provide our vertical (y) axis
				//for each row in the "base" report, we need to pull in the data
				InstanceReportRow lastAbstract = null;
				foreach( InstanceReportRow row in this.Rows )
				{
					if( row.IsReportTitle )
						continue;

					//retain abstracts...
					if( row.IsAbstractGroupTitle )
					{
						//...unless they're consecutive - retain the most recent one
						if( lastAbstract != null )
						{
							int at = segmentCandidate.Rows.IndexOf( lastAbstract );
							if( at == segmentCandidate.Rows.Count - 1 )
								segmentCandidate.Rows.RemoveAt( at );
						}

						InstanceReportRow abstractRow = CloneAndTruncate( row, allCalendars.Count );
						abstractRow.Id = segmentCandidate.Rows.Count;
						segmentCandidate.Rows.Add( abstractRow );

						lastAbstract = abstractRow;
						continue;
					}

					//`calendars` provide our horizontal (x) axis
					//`calendars` (x) combined with `segmentSets` & `rows` (y) allow us to look up the correct cell
					bool hasData = false;
					InstanceReportRow currentRow = new InstanceReportRow();
					foreach( InstanceReportColumn calendar in allCalendars )
					{
						List<InstanceReportColumn> matches = this.GetMatchingColumns( calendar, segmentSet, row );

						//apply exact match
						InstanceReportColumn exactColumn = matches.Find( m => m.ReportingPeriodEquals( calendar ) );
						if( exactColumn != null )
						{
							Cell exactCell = row.Cells.Find( c => c.Id == exactColumn.Id );
							if( exactCell != null && exactCell.HasData )
							{
								hasData = true;
								Cell newCell = (Cell)exactCell.Clone();
								newCell.EmbeddedReport = exactCell.EmbeddedReport;

								if( !string.IsNullOrEmpty( segmentSet.CurrencyCode ) )
								{
									if( (int)row.Unit == (int)UnitType.Monetary ||
										(int)row.Unit == (int)UnitType.EPS )
									{
										newCell.CurrencyCode = segmentSet.CurrencyCode;
										newCell.CurrencySymbol = segmentSet.CurrencySymbol;
									}
								}

								currentRow.Cells.Add( newCell );
								continue;
							}
						}

						//apply similar matches
						{
							List<Cell> cells = matches.ConvertAll( col => row.Cells.Find( c => c.Id == col.Id ) );

							//Now reduce our cells to those with values...
							cells.RemoveAll( c => c == null || !c.HasData );

							//...and non-duplicates
							for( int c = 0; c < cells.Count; c++ )
							{
								Cell curCell = cells[ c ];
								cells.RemoveAll( cell => cell.Id != curCell.Id && cell.NumericAmount == curCell.NumericAmount );
							}

							switch( cells.Count )
							{
								case 0:
									Cell emptyCell = new Cell();
									currentRow.Cells.Add( emptyCell );
									break;
								case 1:
									hasData = true;
									Cell newCell = (Cell)cells[ 0 ].Clone();
									newCell.EmbeddedReport = cells[ 0 ].EmbeddedReport;

									if( !string.IsNullOrEmpty( segmentSet.CurrencyCode ) )
									{
										if( (int)row.Unit == (int)UnitType.Monetary ||
											(int)row.Unit == (int)UnitType.EPS )
										{
											newCell.CurrencyCode = segmentSet.CurrencyCode;
											newCell.CurrencySymbol = segmentSet.CurrencySymbol;
										}
									}

									currentRow.Cells.Add( newCell );
									break;
								default:
									Debug.Assert( false, "Too many cells" );
									break;
							}
						}
					}

					//if we actually found data for this row, let's clone the original, and swap out the cells
					if( hasData )
					{
						InstanceReportRow clonedRow = (InstanceReportRow)row.Clone( false, false );
						clonedRow.Cells.AddRange( currentRow.Cells );
						clonedRow.Id = segmentCandidate.Rows.Count;

						segmentCandidate.Rows.Add( clonedRow );
					}
				}

				//Same as above, don't preserve consecutive abstract rows
				if( lastAbstract != null )
				{
					int at = segmentCandidate.Rows.IndexOf( lastAbstract );
					if( at == segmentCandidate.Rows.Count - 1 )
						segmentCandidate.Rows.RemoveAt( at );
				}
			}

			//now that the permutation is complete, apply the new rows and columns to the "base" report
			this.Columns.Clear();
			this.Columns.AddRange( segmentCandidate.Columns );

			this.Rows.Clear();
			this.Rows.AddRange( segmentCandidate.Rows );

			this.SynchronizeGrid();

			//this.InstantValues();

			processed.Value = true;
			return true;
		}

		private void InstantValues()
		{
			List<InstanceReportRow> instantRows = this.Rows.FindAll( row => row.PeriodType == "instant" );
			if( instantRows.Count == 0 )
				return;

			for( int c = 0; c < this.Columns.Count; c++ )
			{
				InstanceReportColumn curColumn = this.Columns[ c ];

				DateTime? curDate = null;
				if( curColumn.MyPeriodType == Element.PeriodType.instant )
					curDate = curColumn.MyContextProperty.PeriodStartDate;

				for( int c2 = c + 1; c2 < this.Columns.Count; c2++ )
				{
					InstanceReportColumn nextColumn = this.Columns[ c2 ];
					if( !curColumn.SegmentAndScenarioEquals( nextColumn ) )
						break;

					bool curHasCurrency = !string.IsNullOrEmpty( curColumn.CurrencyCode );
					if( curHasCurrency )
					{
						bool nextHasCurrency = !string.IsNullOrEmpty( nextColumn.CurrencyCode );
						if( nextHasCurrency )
						{
							if( !string.Equals( curColumn.CurrencyCode, nextColumn.CurrencyCode ) )
								continue;
						}
					}

					DateTime? nextDate = null;
					if( nextColumn.MyPeriodType == Element.PeriodType.instant )
						nextDate = nextColumn.MyContextProperty.PeriodStartDate;

					foreach( InstanceReportRow row in instantRows )
					{
						DateTime? leftDate, rightDate;
						if( curDate.HasValue )
						{
							leftDate = curDate;
						}
						else
						{
							if( row.IsBeginningBalance )
								leftDate = curColumn.MyContextProperty.PeriodStartDate;
							else
								leftDate = curColumn.MyContextProperty.PeriodEndDate;
						}

						if( nextDate.HasValue )
						{
							rightDate = nextDate;
						}
						else
						{
							if( row.IsBeginningBalance )
								rightDate = nextColumn.MyContextProperty.PeriodStartDate;
							else
								rightDate = nextColumn.MyContextProperty.PeriodEndDate;
						}

						if( DateTime.Equals( leftDate, rightDate ) )
						{
							bool canShareData = row.Cells[ c ].HasData ^ row.Cells[ c2 ].HasData;
							if( canShareData )
							{
								if( !row.Cells[ c ].HasData )
									row.Cells[ c ].AddData( row.Cells[ c2 ] );
								else
									row.Cells[ c2 ].AddData( row.Cells[ c ] );
							}
						}
					}
				}
			}
		}

		private void PromoteConsolidatedSegment( List<InstanceReportColumn> uniqueSegmentColumns, List<Segment> commonSegmentsToPromote )
		{
			//move the consolidated segment to the top, if it exists
			InstanceReportColumn consolidatedColumn = uniqueSegmentColumns.Find( uCol => uCol.Segments.Count == commonSegmentsToPromote.Count );
			if( consolidatedColumn != null )
			{
				uniqueSegmentColumns.Remove( consolidatedColumn );
				uniqueSegmentColumns.Insert( 0, consolidatedColumn );
			}
		}

		private List<Segment> GetCommonSegments( List<InstanceReportColumn> uniqueSegmentColumns )
		{
			List<Segment> commonSegments = new List<Segment>();
			if( uniqueSegmentColumns.Count < 2 )
				return commonSegments;

			InstanceReportColumn testColumn = null;
			foreach( InstanceReportColumn cmpColumn in uniqueSegmentColumns )
			{
				//it only takes one bad apple...
				if( cmpColumn.Segments == null || cmpColumn.Segments.Count == 0 )
					return commonSegments;

				if( testColumn == null )
					testColumn = cmpColumn;
				else if( testColumn.Segments.Count > cmpColumn.Segments.Count )
					testColumn = cmpColumn;
			}

			//if we didn't find any segments to promote, return early
			if( testColumn == null )
				return commonSegments;

			foreach( Segment seg in testColumn.Segments )
			{
				bool allColumnsMatch = true;
				foreach( InstanceReportColumn col in uniqueSegmentColumns )
				{
					bool segmentFound = false;
					foreach( Segment cSeg in col.Segments )
					{
						if( InstanceReportColumn.SegmentEquals( seg, cSeg ) )
						{
							segmentFound = true;
							break;
						}
					}

					if( !segmentFound )
					{
						allColumnsMatch = false;
						break;
					}
				}

				if( allColumnsMatch )
					commonSegments.Add( seg );
			}

			return commonSegments;
		}

		private InstanceReportRow FindCloneAndTruncate( Predicate<InstanceReportRow> rowTest, int cellCount )
		{
			InstanceReportRow rowFound = this.Rows.Find( rowTest );
			if( rowFound == null )
				return null;

			InstanceReportRow newRow = CloneAndTruncate( rowFound, cellCount );
			return newRow;
		}

		private static InstanceReportRow CloneAndTruncate( InstanceReportRow rowFound, int cellCount )
		{
			int cellsToRemove = rowFound.Cells.Count - cellCount;
			InstanceReportRow newRow = (InstanceReportRow)rowFound.Clone();
			newRow.Cells.RemoveRange( cellCount, cellsToRemove );
			return newRow;
		}

		private List<InstanceReportColumn> GetMatchingColumns( InstanceReportColumn calendar, InstanceReportColumn segmentSet, InstanceReportRow row )
		{
			bool isInstantElement = row.PeriodType == "instant";

			List<InstanceReportColumn> matches = this.Columns.FindAll(
			col =>
			{
				if( !col.SegmentAndScenarioEquals( segmentSet ) )
					return false;

				//calendar must be the first paramter
				if( !ContainsCalendar( calendar, col, row.IsBeginningBalance, isInstantElement ) )
					return false;

				return true;
			} );

			if( matches.Count > 1 )
			{
				List<InstanceReportColumn> tmpMatches = matches.FindAll( m => m.MyPeriodType == calendar.MyPeriodType );
				if( tmpMatches.Count == 1 )
					matches = tmpMatches;
			}

			return matches;
		}

		private static bool DoCalendarsOverlap( Dictionary<int, List<InstanceReportColumn>> calendarsBySegment,
			out List<InstanceReportColumn> allCalendars )
		{
			allCalendars = new List<InstanceReportColumn>();
			bool contains = true;

			foreach( KeyValuePair<int, List<InstanceReportColumn>> kvpCalendars in calendarsBySegment )
			{
				contains = true;
				foreach( KeyValuePair<int, List<InstanceReportColumn>> kvpMatches in calendarsBySegment )
				{
					if( int.Equals( kvpCalendars.Key, kvpMatches.Key ) )
						continue;

					foreach( InstanceReportColumn match in kvpMatches.Value )
					{
						contains = ReportUtils.Exists( kvpCalendars.Value, cal => ContainsCalendar( cal, match, false, false ) );
						if( !contains )
							break;
					}

					if( !contains )
						break;
				}

				if( contains )
				{
					allCalendars = kvpCalendars.Value;
					break;
				}
			}

			if( !contains )
				return false;

			allCalendars.ForEach(
			cal =>
			{
				foreach( Segment seg in cal.Segments )
				{
					cal.Labels.RemoveAll( ll => ll.Label == seg.ValueName );
				}

				cal.Segments = new ArrayList();
			} );
			return true;
		}

		private static bool ContainsCalendar( InstanceReportColumn leftCol, InstanceReportColumn rightCol, bool isBeginningBalance, bool isInstantElement )
		{
			if( leftCol.ReportingPeriodEquals( rightCol ) )
				return true;

			if( leftCol.MyPeriodType == Element.PeriodType.duration )
			{
				if( isBeginningBalance )
				{
					if( leftCol.MyPeriodType != rightCol.MyPeriodType )
					{
						//duration start vs instant start
						if( DateTime.Equals( leftCol.MyContextProperty.PeriodStartDate, rightCol.MyContextProperty.PeriodStartDate ) )
							return true;
					}
					else if( isInstantElement )
					{
						//duration start vs duration start
						if( DateTime.Equals( leftCol.MyContextProperty.PeriodStartDate, rightCol.MyContextProperty.PeriodStartDate ) )
							return true;
					}
				}
				else
				{
					if( leftCol.MyPeriodType != rightCol.MyPeriodType )
					{
						//duration end vs instant start
						if( DateTime.Equals( leftCol.MyContextProperty.PeriodEndDate, rightCol.MyContextProperty.PeriodStartDate ) )
							return true;
					}
					else if( isInstantElement )
					{
						//duration end vs duration end
						if( DateTime.Equals( leftCol.MyContextProperty.PeriodEndDate, rightCol.MyContextProperty.PeriodEndDate ) )
							return true;
					}
				}
			}

			return false;
		}

		private Dictionary<int, List<InstanceReportColumn>> GetSegmentScenarioCalendars( List<InstanceReportColumn> uniqueSegmentColumns )
		{
			Dictionary<int, List<InstanceReportColumn>> calendarsBySegment = new Dictionary<int, List<InstanceReportColumn>>();
			foreach( InstanceReportColumn uSegCol in uniqueSegmentColumns )
			{
				List<InstanceReportColumn> uniqueCalendars = new List<InstanceReportColumn>();
				foreach( InstanceReportColumn anyCol in this.Columns )
				{
					if( !uSegCol.SegmentAndScenarioEquals( anyCol ) )
						continue;

					if( !ReportUtils.Exists( uniqueCalendars, uCal => uCal.ReportingPeriodEquals( anyCol ) ) )
					{
						//create a clone so that we can slowly digest this data without affecting other references
						InstanceReportColumn clone = (InstanceReportColumn)anyCol.Clone();
						uniqueCalendars.Add( clone );
					}
				}

				if( uniqueCalendars.Count > 0 )
					calendarsBySegment[ uSegCol.Id ] = uniqueCalendars;
			}

			return calendarsBySegment;
		}

		/// <summary>
		/// Creates a list of unique columns based on their segments and scenarios.
		/// </summary>
		/// <returns></returns>
		private List<InstanceReportColumn> GetSegmentScenarioColumnsForSegmentProcessing()
		{
			//#1 - this will be our X axis - a set of unique segments and scenarios
			//   It will be our "master" list of columns for the equity report
			List<InstanceReportColumn> uniqueSegmentScenarioColumns = new List<InstanceReportColumn>();
			for( int c = 0; c < this.Columns.Count; c++ )
			{
				InstanceReportColumn column = this.Columns[ c ];
				bool exists = ReportUtils.Exists( uniqueSegmentScenarioColumns,
				tmp =>
				{
					if( !tmp.SegmentAndScenarioEquals( column ) )
						return false;

					//if( !tmp.CurrencyEquals( column ) )
					//	return false;

					return true;
				} );

				if( !exists )
				{
					//create a clone so that we can digest this data without affecting other references
					InstanceReportColumn newColumn = (InstanceReportColumn)column.Clone();
					uniqueSegmentScenarioColumns.Add( newColumn );
				}
			}

			return uniqueSegmentScenarioColumns;
		}
	}
}
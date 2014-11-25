using System.Collections;
using Aucent.MAX.AXE.XBRLParser;
using XBRLReportBuilder.Utilities;
using System.Text;
using Aucent.MAX.AXE.Common.Data;
using System;
using System.Collections.Generic;
namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		public void ProcessBeginningAndEndingBalances()
		{
			//First, separate the duration columns and instant columns
			ArrayList durationColumnIndex = new ArrayList();
			ArrayList instantColumnIndex = new ArrayList();
			for( int colIndex = 0; colIndex < this.Columns.Count; colIndex++ )
			{
				InstanceReportColumn irc = this.Columns[ colIndex ] as InstanceReportColumn;
				if( irc.MyContextProperty != null &&
					irc.MyContextProperty.PeriodType == Element.PeriodType.duration )
				{
					durationColumnIndex.Add( colIndex );
				}
				else
				{
					instantColumnIndex.Add( colIndex );
				}
			}

			//Process Cash & Cash Equivalents Beginning/Ending Balances
			int beginingBalanceRowIndex = -1;
			int endingBalanceRowIndex = -1;

			for( int rowIndex = 0; rowIndex < this.Rows.Count; rowIndex++ )
			{
				InstanceReportRow irr = this.Rows[ rowIndex ] as InstanceReportRow;

				if( !irr.IsReportTitle && !irr.IsSegmentTitle && ( irr.IsBeginningBalance || irr.IsEndingBalance ) )
				{
					if( irr.IsBeginningBalance )
					{
						beginingBalanceRowIndex = rowIndex;
						HandleBalances( true, irr, durationColumnIndex, instantColumnIndex );
					}
					if( irr.IsEndingBalance )
					{
						endingBalanceRowIndex = rowIndex;
						HandleBalances( false, irr, durationColumnIndex, instantColumnIndex );
					}
				}
			}

			//Check if one more step is necesary to realign the beginning/ending balances
			if( ReportUtils.IsCashFlowFromOps( this.ReportName ) &&
				beginingBalanceRowIndex != -1 && endingBalanceRowIndex != -1 )
			{
				RealignCashRows( durationColumnIndex, beginingBalanceRowIndex, endingBalanceRowIndex );
				//Check if we should delete the "empty" instance column and possible extra duration columns                                
				DeleteEmptyColumns( beginingBalanceRowIndex, endingBalanceRowIndex, instantColumnIndex, durationColumnIndex );
			}

		}

		private void DeleteEmptyColumns( int beginingBalanceRowIndex, int endingBalanceRowIndex, ArrayList instantColumnIndex, ArrayList durationColumnIndex )
		{
			//Handle instance columns
			ArrayList emptyColumns = new ArrayList();
			ArrayList cashBalances = new ArrayList();
			if( beginingBalanceRowIndex != -1 )
			{
				foreach( Cell c in ( this.Rows[ beginingBalanceRowIndex ] as InstanceReportRow ).Cells )
				{
					if( c.NumericAmount != 0 )
						cashBalances.Add( c.NumericAmount );
				}
			}
			if( endingBalanceRowIndex != -1 )
			{
				foreach( Cell c in ( this.Rows[ endingBalanceRowIndex ] as InstanceReportRow ).Cells )
				{
					if( c.NumericAmount != 0 )
						cashBalances.Add( c.NumericAmount );
				}
			}

			cashBalances.Sort();

			foreach( int colIndex in instantColumnIndex )
			{
				bool empty = true;
				foreach( InstanceReportRow irr in this.Rows )
				{
					Cell c = irr.Cells[ colIndex ] as Cell;
					//					if( c.IsNumeric )
					//					{
					decimal amount = c.NumericAmount;
					if( amount != 0 && cashBalances.BinarySearch( amount ) < 0 )
					{
						empty = false;
						break;
					}
					//					}
					//					else
					//					{
					//						if( !string.IsNullOrEmpty( c.NonNumericTextHeader ) )
					//						{
					//							empty = false;
					//							break;
					//						}
					//					}
				}

				if( empty )
				{
					emptyColumns.Add( colIndex );
				}
			}

			//Handle duration columns

			foreach( int colIndex in durationColumnIndex )
			{
				bool hasCashData = false;
				foreach( InstanceReportRow irr in this.Rows )
				{
					Cell c = irr.Cells[ colIndex ] as Cell;
					//					if( c.IsNumeric )
					//					{
					decimal amount = c.NumericAmount;
					if( amount != 0 && irr.Label.ToLower().IndexOf( "cash" ) >= 0 )
					{
						hasCashData = true;
						break;
					}
					//					}
					//					else
					//					{
					//						if( !string.IsNullOrEmpty( c.NonNumericTextHeader ) )
					//						{
					//							hasCashData = true;
					//							break;
					//						}
					//					}
				}

				if( !hasCashData )
				{
					emptyColumns.Add( colIndex );
				}
			}


			//Remove empty columns
			emptyColumns.Sort();

			for( int index = emptyColumns.Count - 1; index >= 0; index-- )
			{
				int colIndex = (int)emptyColumns[ index ];

				//delete columns
				this.Columns.RemoveAt( colIndex );

				//delete cells
				foreach( InstanceReportRow irr in this.Rows )
				{
					irr.Cells.RemoveAt( colIndex );
				}
			}
		}

		private void RealignCashRows( ArrayList durationColumnIndex, int beginingBalanceRowIndex, int endingBalanceRowIndex )
		{
			InstanceReportRow irBegin = this.Rows[ beginingBalanceRowIndex ] as InstanceReportRow;
			InstanceReportRow irEnd = this.Rows[ endingBalanceRowIndex ] as InstanceReportRow;

			//Fill in missing beginning balances from ending balance columns
			foreach( int colIndex in durationColumnIndex )
			{
				DateTime periodStartDate = this.Columns[ colIndex ].MyContextProperty.PeriodStartDate; ;
				//Try look for an ending balance cell that has the end date = periodStartDate, or periodStartDate -1
				foreach( int colIndex2 in durationColumnIndex )
				{
					DateTime periodEndDate = this.Columns[ colIndex2 ].MyContextProperty.PeriodEndDate;
					if( periodEndDate == periodStartDate || periodEndDate == periodStartDate.AddDays( -1 ) )
					{
						( irBegin.Cells[ colIndex ] as Cell ).NumericAmount = ( irEnd.Cells[ colIndex2 ] as Cell ).NumericAmount;
						( irBegin.Cells[ colIndex ] as Cell ).RoundedNumericAmount = ( irEnd.Cells[ colIndex2 ] as Cell ).RoundedNumericAmount;
						break;
					}
				}
			}

			//Fill in missing ending balances from same period end columns
			foreach( int colIndex in durationColumnIndex )
			{
				if( ( irEnd.Cells[ colIndex ] as Cell ).NumericAmount == 0 )//find the ending balance from another duration column
				{
					DateTime periodEndDate = this.Columns[ colIndex ].MyContextProperty.PeriodEndDate;
					foreach( int colIndex2 in durationColumnIndex )
					{
						if( colIndex2 != colIndex &&
							periodEndDate == this.Columns[ colIndex2 ].MyContextProperty.PeriodEndDate &&
							( irEnd.Cells[ colIndex2 ] as Cell ).NumericAmount != 0 )
						{
							( irEnd.Cells[ colIndex ] as Cell ).IsNumeric = true;
							( irEnd.Cells[ colIndex ] as Cell ).NumericAmount = ( irEnd.Cells[ colIndex2 ] as Cell ).NumericAmount;
							( irEnd.Cells[ colIndex ] as Cell ).RoundedNumericAmount = ( irEnd.Cells[ colIndex2 ] as Cell ).RoundedNumericAmount;
							break;
						}
					}
				}
			}

		}

		private bool SegmentsScenarioMatch( ArrayList firstSets, ArrayList secondSets, bool isSegment )
		{
			if( firstSets.Count == 0 && secondSets.Count == 0 )
			{
				return true;
			}
			else if( firstSets.Count == secondSets.Count ) //possible the same, check the actual values
			{
				StringBuilder first = new StringBuilder();
				StringBuilder second = new StringBuilder();

				if( isSegment )
				{
					foreach( Segment s in firstSets )
					{
						first.Append( s.ValueType ).Append( s.ValueName );
					}
					foreach( Segment s in secondSets )
					{
						second.Append( s.ValueType ).Append( s.ValueName );
					}
				}
				else
				{
					foreach( Scenario s in firstSets )
					{
						first.Append( s.ValueType ).Append( s.ValueName );
					}
					foreach( Scenario s in secondSets )
					{
						second.Append( s.ValueType ).Append( s.ValueName );
					}
				}
				return first.ToString() == second.ToString();
			}

			return false;
		}

		private void HandleBalances( bool isBeginningBalance, InstanceReportRow irr, ArrayList durationColumnIndex, ArrayList instantColumnIndex )
		{
			try
			{
				Dictionary<int, bool> balanceCellsToClear = new Dictionary<int, bool>();
				Dictionary<int, bool> durationCellsChanged = new Dictionary<int, bool>();
				for( int colIndex = 0; colIndex < this.Columns.Count; colIndex++ )
				{
					InstanceReportColumn currentCol = this.Columns[ colIndex ] as InstanceReportColumn;
					DateTime periodStartDate = currentCol.MyContextProperty.PeriodStartDate;
					DateTime periodEndDate = currentCol.MyContextProperty.PeriodEndDate;

					bool foundMatch = false;
					//find the matching begining balance    
					//bool foundAnyMatch = false;
					for( int colIndexCheck = colIndex + 1; colIndexCheck < this.Columns.Count; colIndexCheck++ )
					{
						if( colIndexCheck == colIndex ) continue;

						InstanceReportColumn currentBalanceColumn = this.Columns[ colIndexCheck ] as InstanceReportColumn;
						DateTime checkPeriodEndDate = currentBalanceColumn.MyContextProperty.PeriodEndDate;
						if( currentBalanceColumn.MyContextProperty.PeriodType == Element.PeriodType.instant )
						{
							checkPeriodEndDate = currentBalanceColumn.MyContextProperty.PeriodStartDate;
						}
						if( isBeginningBalance )
						{
							foundMatch = ( checkPeriodEndDate == periodStartDate ) || ( checkPeriodEndDate == periodStartDate.AddDays( -1 ) );
						}
						else
						{
							foundMatch = ( checkPeriodEndDate == periodEndDate );
						}
						foundMatch = foundMatch && currentCol.CurrencyCode.ToLower() == currentBalanceColumn.CurrencyCode.ToLower();

						if( foundMatch &&
							SegmentsScenarioMatch( currentBalanceColumn.Segments, this.Columns[ colIndex ].Segments, true ) &&
							SegmentsScenarioMatch( currentBalanceColumn.Scenarios, this.Columns[ colIndex ].Scenarios, false ) )
						{
							//foundAnyMatch = true;

							Cell durationCell = irr.Cells[ colIndex ] as Cell;
							Cell balanceCell = irr.Cells[ colIndexCheck ] as Cell;

							/**
							 *because 'MergeInstantAndDuration' should have already placed
							 *   the beginning balances in the correct location
							 *   ensure that we do not overwrite the correct date
							 *   which already exists.
							 **/
							if( !durationCell.HasData ) //  || ((durationCell.NumericAmount != balanceCell.NumericAmount) && balanceCell.NumericAmount != 0))
							{
								durationCell.AddData( balanceCell );

								/*
								durationCell.ShowCurrencySymbol = balanceCell.ShowCurrencySymbol;
								durationCell.IsNumeric = balanceCell.IsNumeric;
								durationCell.NumericAmount = balanceCell.NumericAmount;
								durationCell.RoundedNumericAmount = balanceCell.RoundedNumericAmount;
								durationCell.FootnoteIndexer = balanceCell.FootnoteIndexer;

								//DE1403
								//Old note from PH: Need to copy Non-Numeric data along with everything else.
								durationCell.NonNumbericText = balanceCell.NonNumbericText;
								*/
								 
								durationCellsChanged[ colIndex ] = true;
								balanceCellsToClear[ colIndexCheck ] = true;
							}


						}
					}

					//if (isBeginningBalance && !foundAnyMatch)
					//{
					//    (irr.Cells[colIndex] as Cell).Clear();
					//}
				}


				if( isBeginningBalance )
				{
					//cycle through CANDIDATES of cells to clear
					foreach( int cellIndex in balanceCellsToClear.Keys )
					{
						//if this candidate WAS NOT changeded by some other part of the process
						//   clear it
						//if( durationCellsChanged.ContainsKey( cellIndex ) )
						//{
						//    (irr.Cells[cellIndex] as Cell).Clear();
						//}
						( irr.Cells[ cellIndex ] as Cell ).Clear();
					}
				}
			}
			catch
			{
			}
		}

	}
}
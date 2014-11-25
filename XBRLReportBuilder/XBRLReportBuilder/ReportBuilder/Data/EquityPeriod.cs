using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder;
using System.Xml;
using Aucent.MAX.AXE.Common.Data;
using System.Diagnostics;

namespace XBRLReportBuilder
{
	public class EquityPeriod
	{
		private List<InstanceReportRow> BBRows = new List<InstanceReportRow>();
		private List<InstanceReportRow> RegRows = new List<InstanceReportRow>();
		private List<InstanceReportRow> EBRows = new List<InstanceReportRow>();

		public List<InstanceReportRow> Rows = new List<InstanceReportRow>();

		private InstanceReport baseReport = null;

		public EquityPeriod()
		{ }

		public EquityPeriod( List<InstanceReportRow> periodRows )
		{
			this.Rows = periodRows;
		}

		public bool AreBalancesEmpty()
		{
			this.DistributeRows();

			if( !this.BBRows.TrueForAll( row => row.IsEmpty() ) )
				return false;

			if( !this.EBRows.TrueForAll( row => row.IsEmpty() ) )
				return false;

			return true;
		}

		public void MergeAndReduceBeginningBalances( EquityPeriod that )
		{
			this.DistributeRows();
			that.DistributeRows();

			foreach( InstanceReportRow ebRow in this.EBRows )
			{
				InstanceReportRow bbRow = that.BBRows.Find( bb => IsMatchingRow( ebRow, bb ) );
				if( bbRow == null )
					continue;

				for( int c = 0; c < ebRow.Cells.Count; c++ )
				{
					Cell ebCell = ebRow.Cells[ c ];
					Cell bbCell = bbRow.Cells[ c ];
					if( !ebCell.HasData && bbCell.HasData )
					{
						Cell newCell = (Cell)bbCell.Clone();
						ebRow.ReplaceCell( null, c, newCell );
					}
				}

				that.BBRows.Remove( bbRow );
			}
		}

		public void DistributeRows()
		{
			if( this.Rows == null || this.Rows.Count == 0 )
				return;

			this.BBRows = new List<InstanceReportRow>();
			this.RegRows = new List<InstanceReportRow>();
			this.EBRows = new List<InstanceReportRow>();

			foreach( InstanceReportRow row in this.Rows )
			{
				if( row.IsBeginningBalance )
					this.BBRows.Add( row );
				else if( row.IsEndingBalance )
					this.EBRows.Add( row );
				else
					this.RegRows.Add( row );
			}

			this.Rows.Clear();
			this.BBRows.RemoveAll( row => row.IsAbstractGroupTitle );
			this.EBRows.RemoveAll( row => row.IsAbstractGroupTitle );

			//if we have consecutive abstract rows, remove the first
			for( int r = 0; r < this.RegRows.Count - 1; r++ )
			{
				if( !this.RegRows[ r ].IsAbstractGroupTitle )
					continue;

				if( !this.RegRows[ r + 1 ].IsAbstractGroupTitle )
				{
					r++;
					continue;
				}

				//remove the "top" row
				this.RegRows.RemoveAt( r );
				r--;
			}

			int lastRow = this.RegRows.Count - 1;
			if( lastRow > -1 && this.RegRows[ lastRow ].IsAbstractGroupTitle )
				this.RegRows.RemoveAt( lastRow );
		}

		public bool EndDateMatchesBeginDate( EquityPeriod that )
		{
			this.DistributeRows();
			that.DistributeRows();

			if( this.EBRows == null || this.EBRows.Count == 0 )
				return false;

			if( that.BBRows == null || that.BBRows.Count == 0 )
				return false;

			DateTime thisLastDate = this.EBRows[ 0 ].BalanceDate.StartDate;
			DateTime thatFirstDate = that.BBRows[ 0 ].BalanceDate.StartDate;
			if( DateTime.Equals( thisLastDate, thatFirstDate ) )
				return true;

			return false;
		}

		public bool HasSegmentedData( List<InstanceReportColumn> allColumns )
		{
			this.DistributeRows();

			List<InstanceReportColumn> segColumns = allColumns.FindAll( col => col.hasSegments );
			if( !HasSegmentedData( this.RegRows, segColumns ) )
				return false;

			if( HasSegmentedData( this.BBRows, segColumns ) )
				return true;

			if( HasSegmentedData( this.EBRows, segColumns ) )
				return true;

			return false;
		}

		public bool IsActivityEmpty()
		{
			this.DistributeRows();

			bool isEmpty = this.RegRows.TrueForAll( row => row.IsEmpty() );
			return isEmpty;
		}

		public void RemoveActivityStyles()
		{
			this.DistributeRows();

			foreach( InstanceReportRow row in this.RegRows )
			{
				row.IsCalendarTitle = false;
			}
		}

		public void RemoveEmptyRows()
		{
			RemoveEmptyRows( this.BBRows );


			RemoveEmptyRows( this.EBRows );
		}

		private static void RemoveEmptyRows( List<InstanceReportRow> rows  )
		{
			bool hasData = false;
			foreach( InstanceReportRow row in rows )
				hasData |= !row.IsEmpty();

			if( hasData )
			{
				rows.RemoveAll( row => row.IsEmpty() );
				return;
			}

			Dictionary<string, List<InstanceReportRow>> elementGroups = new Dictionary<string, List<InstanceReportRow>>();
			foreach( InstanceReportRow row in rows )
			{
				if( !elementGroups.ContainsKey( row.ElementName ) )
					elementGroups[ row.ElementName ] = new List<InstanceReportRow>();

				elementGroups[ row.ElementName ].Add( row );
			}

			InstanceReportRow monetaryRow = null;
			Dictionary<string, InstanceReportRow> protectedRowsByElement = new Dictionary<string, InstanceReportRow>();
			foreach( List<InstanceReportRow> elRows in elementGroups.Values )
			{
				int lowScore = int.MaxValue;
				foreach( InstanceReportRow row in elRows )
				{
					int curScore = 0;
					foreach( Segment seg in row.EmbedRequirements.Segments )
					{
						if( !seg.IsDefaultForEntity )
							curScore++;
					}

					if( curScore < lowScore )
					{
						//Defect: SEC0126
						lowScore = curScore;
						protectedRowsByElement[ row.ElementName ] = row;

						if( row.IsMonetary )
							monetaryRow = row;
					}
				}
			}

			if( monetaryRow != null )
			{
				rows.RemoveAll( row => row != monetaryRow );
			}
			else
			{
				List<InstanceReportRow> protectedRows = new List<InstanceReportRow>( protectedRowsByElement.Values );
				rows.RemoveAll( row => !protectedRows.Contains( row ) );
			}
		}

		public void Sort( InstanceReport baseReport )
		{
			try
			{
				this.baseReport = baseReport;
				this.BBRows.Sort( SortSpecialRows );
				this.EBRows.Sort( SortSpecialRows );
			}
			finally
			{
				this.baseReport = null;
			}

			this.Rows.Clear();
			this.Rows.AddRange( this.BBRows );
			this.Rows.AddRange( this.RegRows );
			this.Rows.AddRange( this.EBRows );
		}

		private static bool HasSegmentedData( List<InstanceReportRow> rows, List<InstanceReportColumn> segColumns )
		{
			foreach( InstanceReportRow row in rows )
			{
				foreach( InstanceReportColumn col in segColumns )
				{
					Cell cell = row.Cells.Find( c => c.Id == col.Id );
					if( cell == null )
						continue;

					if( cell.HasData )
						return true;
				}
			}

			return false;
		}

		private bool IsMatchingRow( InstanceReportRow leftRow, InstanceReportRow rightRow )
		{
			if( leftRow.ElementName != rightRow.ElementName )
				return false;

			foreach( Segment keepSeg in leftRow.EmbedRequirements.Segments )
			{
				bool found = false;
				foreach( Segment remSeg in rightRow.EmbedRequirements.Segments )
				{
					if( string.Equals( keepSeg.DimensionInfo.dimensionId, remSeg.DimensionInfo.dimensionId ) )
					{
						if( string.Equals( keepSeg.DimensionInfo.Id, remSeg.DimensionInfo.Id ) )
						{
							found = true;
							break;
						}
						else
						{
							return false;
						}
					}
				}

				if( !found )
					return false;
			}

			return true;
		}

		private int SortSpecialRows( InstanceReportRow top, InstanceReportRow bot )
		{
			//sort base or extended?
			if( top.IsBaseElement != bot.IsBaseElement)
			{
				if( top.IsBaseElement )
					return -1;
				else
					return 1;
			}


			//sort unit
			if( top.Unit != bot.Unit )
			{
				if( top.IsMonetary != bot.IsMonetary )
				{
					if( top.IsMonetary )
						return -1;
					else
						return 1;
				}

				if( top.IsShares != bot.IsShares )
				{
					if( top.IsShares )
						return -1;
					else
						return 1;
				}

				if( top.Unit < bot.Unit )
					return -1;
				else
					return 1;
			}



			if( top.IsEquityPrevioslyReportedAsRow != bot.IsEquityPrevioslyReportedAsRow )
			{
				if( top.IsEquityPrevioslyReportedAsRow )
					return -1;
				else
					return 1;
			}




			if( top.IsEquityAdjustmentRow != bot.IsEquityAdjustmentRow )
			{
				if( top.IsEquityAdjustmentRow )
					return -1;
				else
					return 1;
			}


			//we've already established that these are the same
			if( top.IsEquityPrevioslyReportedAsRow || top.IsEquityAdjustmentRow )
			{
				int cmp = baseReport.CompareSegments( top.EmbedRequirements.Segments, bot.EmbedRequirements.Segments );
				if( cmp != 0 )
					return cmp;
			}


			return 0;
		}
	}
}

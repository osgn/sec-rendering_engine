using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder.Utilities;

namespace XBRLReportBuilder
{
	public class EquityRowResult
	{
		public InstanceReportRow Row
		{
			get
			{
				if( this.Rows.Count > 0 )
					return this.Rows[ 0 ];
				else
					return null;
			}
			set
			{
				if( this.Rows == null )
					this.Rows = new List<InstanceReportRow>();
				else
					this.Rows.Clear();

				if( value != null )
					this.Rows.Add( value );
			}
		}

		public List<InstanceReportRow> Rows { get; set; }
		public int PopulatedCells { get; set; }
		public int PopulatedSegmentedCells { get; set; }

		public EquityRowResult()
		{
			this.Row = null;
			this.Rows = new List<InstanceReportRow>();
			this.PopulatedCells = 0;
			this.PopulatedSegmentedCells = 0;
		}

		public void RemoveAbstractsWithoutElements()
		{
			List<int> rowsToRemove = new List<int>();

			int rowMax = this.Rows.Count - 1;
			for( int r = 0; r < this.Rows.Count; r++ )
			{
				InstanceReportRow thisRow = this.Rows[ r ];
				if( !thisRow.IsAbstractGroupTitle )
					continue;

				if( r == rowMax )
				{
					rowsToRemove.Add( r );
					continue;
				}

				InstanceReportRow nextRow = this.Rows[ r + 1 ];
				if( !nextRow.IsAbstractGroupTitle )
					continue;

				rowsToRemove.Add( r );
			}

			//put them in order from low to high
			rowsToRemove.Sort();

			//now make it high to low...
			rowsToRemove.Reverse();

			//now remove them
			rowsToRemove.ForEach( r => this.Rows.RemoveAt( r ) );
		}

		public bool HasNonBalanceRows()
		{
			bool hasNonBalanceRows = ReportUtils.Exists( this.Rows, row => !( row.IsCalendarTitle || row.IsAbstractGroupTitle ) );
			return hasNonBalanceRows;
		}

		/// <summary>
		/// When used with a collection Balance Rows, this method will clear all of the empty rows.
		/// If all of the rows are empty, the first non-empty row will be retained.
		/// </summary>
		public void RemoveEmptyRows()
		{
			InstanceReportRow firstRow = this.Row;

			this.Rows.RemoveAll( row => row.IsEmpty() );

			if( this.Rows.Count == 0 )
				this.Row = firstRow;
		}
	}
}

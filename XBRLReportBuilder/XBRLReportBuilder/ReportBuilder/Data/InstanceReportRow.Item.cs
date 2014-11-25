using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	public partial class InstanceReportRow : InstanceReportItem
	{
		/// <summary>
		/// Returns the label for this row.  This value is never null.
		/// </summary>
		//[XmlElement( Order = 30 )]
		public override string Label //TODO remove - redundant
		{
			get { return base.Label; }
			set { base.Label = value; }
		}

		[XmlIgnore]
		public override List<LabelLine> Labels
		{
			get { return base.Labels; }
			set { base.Labels = value; }
		}

        /// <summary>
        /// Implementation of <see cref="InstanceReportItem.GetCellArray"/>
        /// abstract method.  Retrieves an array of <see cref="Cell"/> from the
        /// given report.
        /// </summary>
        /// <param name="report">The report to retrieve cells from.</param>
        /// <returns>An array of <see cref="Cell"/>.</returns>
		public override Cell[] GetCellArray( InstanceReport report )
		{
			return this.Cells.ToArray();
		}

        /// <summary>
        /// Implementation of <see cref="InstanceReportItem.RemoveSelf"/>
        /// abstract method.  Removes this instances rows from the given
        /// report.
        /// </summary>
        /// <param name="report">The report to remove a row from.</param>
		public override void RemoveSelf( InstanceReport report )
		{
			report.Rows.Remove( this );
		}

        /// <summary>
        /// Replace a cell at a specific location within the <see
        /// cref="InstanceReportRow"/>.
        /// </summary>
        /// <param name="report">Unused.</param>
        /// <param name="cellIndex">The index at which to perform the cell
        /// replacement.</param>
        /// <param name="cell">The new cell.</param>
		public override void ReplaceCell( InstanceReport report, int cellIndex, Cell cell )
		{
			this.Cells.RemoveAt( cellIndex );
			this.Cells.Insert( cellIndex, cell );
		}
	}
}

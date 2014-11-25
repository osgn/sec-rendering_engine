using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	public partial class InstanceReportColumn : InstanceReportItem
	{
		[XmlIgnore]
		public override string Label
		{
			get { return base.Label; }
			set { base.Label = value; }
		}

		[XmlArray( "Labels" )]// Order = 11 )]
		[XmlArrayItem( "Label", typeof( LabelLine ) )]
		public override List<LabelLine> Labels
		{
			get { return base.Labels; }
			set { base.Labels = value; }
		}

		public override Cell[] GetCellArray( InstanceReport report )
		{
			Cell[] values = new Cell[ report.Rows.Count ];

			for( int r = 0; r < values.Length; r++ )
			{
				values[ r ] = report.Rows[ r ].Cells.Find( ( c ) => c.Id == this.Id );
			}

			return values;
		}

		public override void RemoveSelf( InstanceReport report )
		{
			int cellIndex = report.Columns.IndexOf( this );
			report.Columns.Remove( this );
			foreach( InstanceReportRow row in report.Rows )
			{
				row.Cells.RemoveAt( cellIndex );
			}
		}

		public override void ReplaceCell( InstanceReport report, int rowIndex, Cell cell )
		{
			int cellIndex = report.Columns.IndexOf( this );
			report.Rows[ rowIndex ].Cells.RemoveAt( cellIndex );
			report.Rows[ rowIndex ].Cells.Insert( cellIndex, cell );
		}
	}
}

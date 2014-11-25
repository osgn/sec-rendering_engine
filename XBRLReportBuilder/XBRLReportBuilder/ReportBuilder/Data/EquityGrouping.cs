using System;
using System.Collections.Generic;
using System.Text;

namespace XBRLReportBuilder
{
	public class EquityGrouping
	{
		private Dictionary<EquityRoleSegmentKey, RowGroup> subGroups =
			new Dictionary<EquityRoleSegmentKey, RowGroup>( new EquityRoleSegmentKey() );

		public RowGroup this[ EquityRoleSegmentKey key ]
		{
			get
			{
				if( !this.subGroups.ContainsKey( key ) )
					this.subGroups[ key ] = new RowGroup();

				return subGroups[ key ];
			}
			set
			{
				this.subGroups[ key ] = value;
			}
		}

		public RowGroup this[ InstanceReport.EquityRowRole role, InstanceReport.EquityRowSegment segment ]
		{
			get
			{
				EquityRoleSegmentKey key = new EquityRoleSegmentKey( role, segment );
				return this[ key ];
			}
			set
			{
				EquityRoleSegmentKey key = new EquityRoleSegmentKey( role, segment );
				this[ key ] = value;
			}
		}

		public InstanceReportColumn DurationCalendar { get; private set; }

		public InstanceReportColumn InstantCalendar { get; private set; }

		public InstanceReportColumn BeginningBalanceCalendar { get; private set; }

		public EquityGrouping( InstanceReportColumn durationCalendar,
			InstanceReportColumn instantCalendar,
			InstanceReportColumn beginningBalanceCalendar )
		{
			this.DurationCalendar = durationCalendar;
			this.InstantCalendar = instantCalendar;
			this.BeginningBalanceCalendar = beginningBalanceCalendar;
		}

		public void Load( List<InstanceReportRow> list )
		{
			foreach( InstanceReport.EquityRowRole role in Enum.GetValues( typeof( InstanceReport.EquityRowRole ) ) )
			{
				foreach( InstanceReport.EquityRowSegment segment in Enum.GetValues( typeof( InstanceReport.EquityRowSegment ) ) )
				{
					this.Load( role, segment, list );
				}
			}
		}

		public List<InstanceReportRow> Load( InstanceReport.EquityRowRole role, InstanceReport.EquityRowSegment segment, List<InstanceReportRow> list )
		{
			return this.Load( new EquityRoleSegmentKey( role, segment ), list );
		}

		public List<InstanceReportRow> Load( EquityRoleSegmentKey key, List<InstanceReportRow> list )
		{
			List<InstanceReportRow> rows = new List<InstanceReportRow>();

			switch( key.Role )
			{
				case InstanceReport.EquityRowRole.None:
					rows = list.FindAll( row => !( row.IsBeginningBalance || row.IsEndingBalance ) );
					break;
				case InstanceReport.EquityRowRole.BeginningBalance:
					rows = list.FindAll( row => row.IsBeginningBalance );
					break;
				case InstanceReport.EquityRowRole.EndingBalance:
					rows = list.FindAll( row => row.IsEndingBalance );
					break;
			}

			switch( key.Segment )
			{
				case InstanceReport.EquityRowSegment.None:
					break;
				case InstanceReport.EquityRowSegment.Adjusted:
					break;
				case InstanceReport.EquityRowSegment.PreviouslyReported:
					break;
			}




			return rows;
		}
	}

	public class RowGroup : List<InstanceReportRow>
	{
	}

	public class EquityRoleSegmentKey : IEqualityComparer<EquityRoleSegmentKey>
	{
		private InstanceReport.EquityRowRole role = XBRLReportBuilder.InstanceReport.EquityRowRole.None;
		public InstanceReport.EquityRowRole Role
		{
			get { return this.role; }
			set { this.role = value; }
		}

		private InstanceReport.EquityRowSegment segment = InstanceReport.EquityRowSegment.None;
		public InstanceReport.EquityRowSegment Segment
		{
			get { return this.segment; }
			set { this.segment = value; }
		}

		public EquityRoleSegmentKey() { }

		public EquityRoleSegmentKey( InstanceReport.EquityRowRole role, InstanceReport.EquityRowSegment segment )
		{
			this.Role = role;
			this.Segment = segment;
		}

		#region IEqualityComparer<EquityRoleSegmentKey> Members

		public bool Equals( EquityRoleSegmentKey x, EquityRoleSegmentKey y )
		{
			if( x.Role != y.Role )
				return false;

			if( x.Segment != y.Segment )
				return false;

			return true;
		}

		public int GetHashCode( EquityRoleSegmentKey obj )
		{
			return base.GetHashCode();
		}

		#endregion
	}
}

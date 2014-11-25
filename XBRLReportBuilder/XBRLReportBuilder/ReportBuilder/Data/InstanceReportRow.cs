//=============================================================================
// InstanceReport (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class contains information for a report row.
//=============================================================================


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using Aucent.MAX.AXE.XBRLParser;


namespace XBRLReportBuilder
{
	/// <summary>
	/// InstanceReportRow
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// 
	[Serializable]
	[XmlInclude( typeof( Cell ) )]
	[XmlInclude( typeof( LabelLine ) )]
	public partial class InstanceReportRow : IComparable, ICloneable
	{
		//[XmlElement( Order = 5 )]
		public int Level = 0;

		//[XmlElement( Order = 6 )]
		public string ElementName = string.Empty;

		//[XmlElement( Order = 7 )]
		public string ElementPrefix = string.Empty;

		//[XmlElement( Order = 8 )]
		public bool IsBaseElement = true;

		//[XmlElement( Order = 9 )]
		public string BalanceType = string.Empty;

		//[XmlElement( Order = 10 )]
		public string PeriodType = string.Empty;

		//[XmlElement( Order = 11 )]
		public bool IsReportTitle = false;

		//[XmlElement( Order = 12 )]
		public bool IsSegmentTitle = false;

		/// <summary>
		/// Indicates where this row has a special role within the "Equity" format.
		/// </summary>
		//[XmlElement( Order = 13 )]
		public bool IsCalendarTitle = false;

		//[XmlElement( Order = 14 )]
		public bool IsEquityPrevioslyReportedAsRow = false;

		//[XmlElement( Order = 15 )]
		public bool IsEquityAdjustmentRow = false;

		//[XmlElement( Order = 16 )]
		public bool IsBeginningBalance = false;

		//[XmlElement( Order = 17 )]
		public bool IsEndingBalance = false;

		//[XmlElement( Order = 18 )]
		public bool IsReverseSign = false;

		//[XmlElement( Order = 19 )]
		public string PreferredLabelRole = string.Empty;

		//[XmlElement( Order = 20 )]
		public string FootnoteIndexer = string.Empty;

		//[XmlElement( Order = 21, ElementName = "Cell", Type = typeof( Cell ) )]
		[XmlArray( "Cells" )]//, Order = 21 )]
		[XmlArrayItem( "Cell", typeof( Cell ) )]
		public List<Cell> Cells = new List<Cell>();

		//[XmlElement( Order = 22 )]
		public InstanceReportColumn OriginalInstanceReportColumn = null;

		//[XmlElement( Order = 23 )]
		public int UnitID
		{
			get { return (int)this.Unit; }
			set { this.Unit = (UnitType)value; }
		}

		//[XmlElement( Order = 24 )]
		public string ElementDataType = "na";

		//[XmlElement( Order = 25 )]
		public string SimpleDataType = "na";

		//[XmlElement( Order = 26 )]
		public string ElementDefenition;

		//[XmlElement( Order = 27 )]
		public string ElementReferences;

		//[XmlElement( Order = 28 )]
		public bool IsTotalLabel = false;

		//[XmlElement( Order = 29 )]
		public CalendarPeriod BalanceDate = null;

		#region properties

		public string ElementKey
		{
			get
			{
				string key = this.ElementName + ( this.PreferredLabelRole ?? string.Empty );
				return key;
			}
		}

		[XmlIgnore]
		public Precision MyPrecision;

		public bool IsNumericDataType
		{
			get
			{
				bool isNumeric = InstanceUtils.IsNumericDataType( this.ElementDataType, this.SimpleDataType );
				return isNumeric;
			}
		}

		#region UNIT LOGIC

		[XmlIgnore]
		public UnitType Unit = UnitType.Other;

		public bool IsEPS
		{
			get
			{
				if( this.Unit == UnitType.EPS )
					return true;
				else
					return false;
			}
		}

		public bool IsExchangeRate
		{
			get
			{
				if( this.Unit == UnitType.ExchangeRate )
					return true;
				else
					return false;
			}
		}

		public bool IsMonetary
		{
			get
			{
				if( this.Unit > UnitType.Monetary )
					return false;

				if( ( this.Unit & UnitType.Monetary ) == UnitType.Monetary )
					return true;
				else
					return false;
			}
		}

		public bool IsShares
		{
			get
			{
				if( ( this.Unit & UnitType.Shares ) == UnitType.Shares )
					return true;
				else
					return false;
			}
		}

		public bool HasCurrency
		{
			get
			{
				if( ( this.Unit & UnitType.Monetary ) == UnitType.Monetary )
					return true;
				else
					return false;
			}
		}

		#endregion

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new InstanceReportRow.
		/// </summary>
		public InstanceReportRow() : base() { }

		public InstanceReportRow( InstanceReportRow arg )
			: this()
		{
			CopyHeader( arg );

			CopyPrecision( arg );

			foreach( Cell c in arg.Cells )
			{
				this.Cells.Add( (Cell)c.Clone() );
			}
		}

		public InstanceReportRow( string label, int numBlankCells )
			: this()
		{
			this.Label = label;
			for( int i = 0; i < numBlankCells; ++i )
			{
				Cells.Add( new Cell() );
			}
		}

		public bool CloneCellToCell( int fromIndex, int toIndex )
		{
			if( this.Cells.Count < toIndex )
				return false;

			Cell newCell = (Cell)this.Cells[ fromIndex ].Clone();

			//if we actually have this cell, remove it
			//otherwise, let the new cell append
			int toID = toIndex;
			if( this.Cells.Count > toIndex )
			{
				toID = this.Cells[ toIndex ].Id;
				this.Cells.RemoveAt( toIndex );
			}
			else
			{
				toID = this.Cells[ toIndex - 1 ].Id + 1;
			}

			newCell.Id = toID;
			this.Cells.Insert( toIndex, newCell );
			return true;
		}

		public object Clone()
		{
			//InstanceReportRow row = new InstanceReportRow( this );
			InstanceReportRow row = (InstanceReportRow)this.Clone( true, true );
			return row;
		}

		public object Clone( bool keepCells, bool cloneCells )
		{
			InstanceReportRow row = (InstanceReportRow)this.MemberwiseClone();

			if( this.BalanceDate != null )
				row.BalanceDate = new CalendarPeriod( this.BalanceDate.StartDate, this.BalanceDate.EndDate );

			//row.EmbedRequirements = null;
			//row.GroupTotalSegment = null;

			if( row.MyPrecision != null )
			{
				row.MyPrecision = new Precision(
					this.MyPrecision.PrecisionType, this.MyPrecision.NumberOfDigits );
			}

			if( keepCells )
			{
				if( cloneCells )
				{
					//do not use clear
					row.Cells = new List<Cell>();
					foreach( Cell c in this.Cells )
					{
						Cell cp = (Cell)c.Clone();
						row.Cells.Add( cp );
					}
				}
			}
			else
			{
				//do not use clear
				row.Cells = new List<Cell>();
			}

			return row;
		}

		#endregion

		#region overrides

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder( this.Label );
			sb.AppendFormat( " ({0}): ", this.Unit.ToString() );

			foreach( Cell rc in this.Cells )
			{
				sb.AppendFormat( "|{0})", rc.Id );

				if( rc.IsNumeric )
					sb.AppendFormat( " {0}", rc.NumericAmount.ToString() );
				else
					sb.AppendFormat( " {0}", rc.NonNumbericText );
			}

			return sb.ToString();
		}

		public override bool Equals( object obj )
		{
			InstanceReportRow irr = obj as InstanceReportRow;
			if( irr == null )
				return false;

			if( this.Cells.Count != irr.Cells.Count ) return false;

			if( this.ElementName != irr.ElementName ) return false;
			if( this.ElementPrefix != irr.ElementPrefix ) return false;

			if( this.Id != irr.Id ) return false;
			if( this.IsAbstractGroupTitle != irr.IsAbstractGroupTitle ) return false;
			if( this.IsBaseElement != irr.IsBaseElement ) return false;
			if( this.IsEPS != irr.IsEPS ) return false;
			if( this.PreferredLabelRole != irr.PreferredLabelRole ) return false;
			if( this.IsReverseSign != irr.IsReverseSign ) return false;
			if( this.IsTotalLabel != irr.IsTotalLabel ) return false;

			if( this.Label != irr.Label ) return false;
			if( this.Level != irr.Level ) return false;

			for( int i = 0; i < Cells.Count; ++i )
			{
				if( !( (Cell)Cells[ i ] ).Equals( irr.Cells[ i ] ) ) return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		public void ApplyBalanceDate()
		{
			if( this.EmbedRequirements == null )
				return;

			this.ApplyBalanceDate( this.EmbedRequirements.Period );
		}

		public void ApplyBalanceDate( InstanceReportColumn calendarColumn )
		{
			CalendarPeriod cp = calendarColumn.GetCalendarPeriod();
			this.ApplyBalanceDate( cp );
		}

		public void ApplyBalanceDate( CalendarPeriod calendarPeriod )
		{
			if( calendarPeriod.PeriodType == Element.PeriodType.duration )
			{
				if( this.IsBeginningBalance )
				{
					this.BalanceDate = new CalendarPeriod( calendarPeriod.StartDate.AddDays( -1 ) );
					this.BalanceDate.PeriodType = Element.PeriodType.instant;
				}
				else if( this.IsEndingBalance )
				{
					this.BalanceDate = new CalendarPeriod( calendarPeriod.EndDate );
					this.BalanceDate.PeriodType = Element.PeriodType.instant;
				}
				else
				{
					this.BalanceDate = new CalendarPeriod( calendarPeriod.StartDate, calendarPeriod.EndDate );
					this.BalanceDate.PeriodType = Element.PeriodType.duration;
				}
			}
			else
			{
				this.BalanceDate = new CalendarPeriod( calendarPeriod.StartDate, calendarPeriod.EndDate );
				this.BalanceDate.PeriodType = Element.PeriodType.instant;
			}
		}

		public void ApplyBalanceDateLabel()
		{
			if( this.EmbedRequirements == null )
				return;

			this.ApplyBalanceDateLabel( string.Empty );
		}

		public void ApplyBalanceDateLabel( string dateFormat )
		{
			if( this.EmbedRequirements == null )
				return;

			if( this.BalanceDate != null )
				this.ApplyBalanceDateLabel( this.BalanceDate, dateFormat );
			else if( this.EmbedRequirements.Period != null )
				this.ApplyBalanceDateLabel( this.EmbedRequirements.Period, dateFormat );
		}

		public void ApplyBalanceDateLabel( InstanceReportColumn calendarColumn )
		{
			CalendarPeriod cp = calendarColumn.GetCalendarPeriod();
			this.ApplyBalanceDateLabel( cp );
		}

		public void ApplyBalanceDateLabel( InstanceReportColumn calendarColumn, string dateFormat )
		{
			CalendarPeriod cp = calendarColumn.GetCalendarPeriod();
			this.ApplyBalanceDateLabel( cp, dateFormat );
		}

		public void ApplyBalanceDateLabel( CalendarPeriod calendarPeriod )
		{
			this.ApplyBalanceDateLabel( calendarPeriod, string.Empty );
		}

		public void ApplyBalanceDateLabel( CalendarPeriod calendarPeriod, string dateFormat )
		{
			if( this.BalanceDate == null )
				return;

			string dtString = this.BalanceDate.StartDate.ToString( dateFormat );
			LabelLine newLabel = new LabelLine( this.Labels.Count + 1, string.Format( " at {1}", this.Label, dtString ) );
			this.Labels.Add( newLabel );
		}


		public void ApplyDataTo( InstanceReportRow that )
		{
			that.BalanceType = this.BalanceType;
			that.ElementDataType = this.ElementDataType;
			that.ElementDefenition = this.ElementDefenition;
			that.ElementName = this.ElementName;
			that.ElementPrefix = this.ElementPrefix;
			that.ElementReferences = this.ElementReferences;

			that.IsAbstractGroupTitle = this.IsAbstractGroupTitle;
			that.IsBaseElement = this.IsBaseElement;
			that.IsBeginningBalance = this.IsBeginningBalance;
			that.IsEndingBalance = this.IsEndingBalance;
			that.IsEquityAdjustmentRow = this.IsEquityAdjustmentRow;
			that.IsEquityPrevioslyReportedAsRow = this.IsEquityPrevioslyReportedAsRow;
			that.IsReverseSign = this.IsReverseSign;
			that.IsTotalLabel = this.IsTotalLabel;

			that.MyPrecision = this.MyPrecision;
			that.PeriodType = this.PeriodType;
			that.PreferredLabelRole = this.PreferredLabelRole;
			that.SimpleDataType = this.SimpleDataType;

			that.Unit = this.Unit;
		}

		public void ApplyRounding( decimal divisor )
		{
			this.Cells.ForEach(
				cell => cell.ApplyRounding( this.MyPrecision, divisor ) );
		}

		public bool ApplySpecialLabel( Segment specialSegment )
		{
			if( string.IsNullOrEmpty( specialSegment.ValueName ) )
				return false;

			if( specialSegment.IsDefaultForEntity )
				return false;

			LabelLine newLabel = new LabelLine( this.Labels.Count + 1, string.Format( " ({1})", this.Label, specialSegment.ValueName ) );
			this.Labels.Add( newLabel );
			return true;
		}

		public void CopyHeader( InstanceReportRow arg )
		{
			if( arg == null )
				return;

			Type stringType = typeof( string );
			Type myType = this.GetType();  //get the type from `this`
			foreach( PropertyInfo pi in myType.GetProperties() )
			{
				if( pi.CanRead && pi.CanWrite )
				{
					if( pi.PropertyType.IsEnum || pi.PropertyType.IsPrimitive || pi.PropertyType == stringType )
					{
						object value = pi.GetValue( arg, null );
						pi.SetValue( this, value, null );
					}
				}
			}

			foreach( FieldInfo fi in myType.GetFields() )
			{
				if( fi.FieldType.IsEnum || fi.FieldType.IsPrimitive || fi.FieldType == stringType )
				{
					object value = fi.GetValue( arg );
					fi.SetValue( this, value );
				}
			}

			//one extra
			BalanceDate = arg.BalanceDate;
		}

		public void CopyPrecision( InstanceReportRow arg )
		{
			if( arg.MyPrecision != null )
			{
				this.MyPrecision = new Precision( arg.MyPrecision.PrecisionType, arg.MyPrecision.NumberOfDigits );
			}

		}

		#region IComparable Members

		public int CompareTo( object obj )
		{
			if( this.Label != null )
			{
				return this.Label.CompareTo( ( obj as InstanceReportRow ).Label );
			}
			else
			{
				return -1;
			}
		}

		#endregion

		public bool IsEmpty()
		{
			foreach( Cell c in this.Cells )
			{
				if( c.HasData )
					return false;
			}

			return true;
		}

		public bool IsNumericDataNil()
		{
			//Can't be nill if the data type is not numeric.
			if( !this.IsNumericDataType )
				return false;

			bool isNill = this.Cells.TrueForAll( cell => cell.IsNil );

			return isNill;
		}
	}
}
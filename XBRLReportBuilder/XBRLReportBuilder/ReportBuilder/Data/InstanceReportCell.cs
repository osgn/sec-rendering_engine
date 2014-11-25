using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	[Serializable]
	[XmlInclude( typeof( EmbedReport ) )]
	public class Cell : IComparable, ICloneable
	{
		public const string NILL_PLACEHOLDER = "&nbsp;&nbsp;";

		#region properties

		[XmlIgnore]
		public bool HasData
		{
			get
			{
				if( this.IsNumeric )
					return true;

				if( string.IsNullOrEmpty( this.NonNumbericText ) )
					return false;

				return true;
			}
		}

		[XmlAttribute]
		public int FlagID = 0;

		public int Id = 0;

		public bool IsNil
		{
			get
			{
				return string.Equals( this.NonNumbericText, Cell.NILL_PLACEHOLDER );
			}
		}

		public bool IsNumeric = false;
		public bool IsRatio = false;
		public bool DisplayZeroAsNone = false;

		public decimal NumericAmount = 0;
		public string RoundedNumericAmount = "0";

		public string NonNumbericText = string.Empty;
		public string FootnoteIndexer = string.Empty;

		[XmlIgnore]
		public bool IsMonetary = false;

		public string CurrencyCode = string.Empty;
		public string CurrencySymbol = string.Empty;
		public bool IsIndependantCurrency = false;
		public bool ShowCurrencySymbol = false;

		[XmlIgnore]
		public bool HasEmbeddedReport
		{
			get { return this.EmbeddedReport != null; }
		}

		public EmbedReport EmbeddedReport = null;
		public bool DisplayDateInUSFormat = false;

		[XmlIgnore]
		public MarkupProperty Markup;

		[XmlAttribute]
		public string ContextID = string.Empty;

		[XmlAttribute]
		public string UnitID = string.Empty;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Cell.
		/// </summary>
		public Cell()
		{
		}

		public Cell( int id )
		{
			this.Id = id;
		}

		public Cell( MarkupProperty mp )
		{
			if( string.IsNullOrEmpty( mp.markupData ) )
				this.AddData( NILL_PLACEHOLDER );
			else
				this.AddData( mp.markupData );

			if( mp.Links.Count > 0 )
			{
				foreach( FootnoteProperty fnp in mp.Links )
				{
					this.FootnoteIndexer += string.Format( "[{0}],", fnp.Id );
				}

				this.FootnoteIndexer = this.FootnoteIndexer.TrimEnd( ',' );
			}

			this.Markup = mp;
			this.ContextID = mp.contextRef.ContextID;

			if( mp.unitRef == null )
				this.UnitID = string.Empty;
			else
				this.UnitID = mp.unitRef.UnitID;
		}

		#endregion

		public void Clear()
		{
			this.DisplayDateInUSFormat = false;
			this.DisplayZeroAsNone = false;

			this.EmbeddedReport = null;

			this.IsNumeric = false;
			this.IsRatio = false;
			
			this.NumericAmount = 0;
			this.NonNumbericText = string.Empty;
			this.RoundedNumericAmount = "0";
		}

		public void AddData( Cell rc )
		{
			if( rc == null )
				return;

			// if we don't already have data, it's ok to add it
			//if( !this.HasData )
				this.SetValues( rc );
		}

		public void AddData( decimal val, string data )
		{
			this.DisplayDateInUSFormat = false;
			this.DisplayZeroAsNone = false;

			this.IsNumeric = true;
			this.IsRatio = false;

			this.NumericAmount = val;
			this.RoundedNumericAmount = data;
		}

		public void AddData( string data )
		{
			this.DisplayDateInUSFormat = false;
			this.DisplayZeroAsNone = false;

			this.IsNumeric = false;
			this.IsRatio = false;

			this.NonNumbericText = data;

			//CEE: don't do this
			//if( EmbedReport.HasMatch( data, null ) )
			//    this.EmbeddedReport = EmbedReport.LoadAndParse( data, null );
		}

		public void ApplyRounding( Precision precision, decimal divisor )
		{
			if( Math.Abs( this.NumericAmount ) >= 100 )
			{
				this.RoundedNumericAmount =
					InstanceReport.CalculateRoundedScaledNumber( this.NumericAmount, divisor, precision );
			}
			else
			{
				this.RoundedNumericAmount = this.NumericAmount.ToString();
			}
		}

		public static bool IsEmbedded( Cell cell )
		{
			return cell.HasEmbeddedReport;
		}

		private void SetValues( Cell rc )
		{
			this.ContextID = rc.ContextID;
			this.UnitID = rc.UnitID;

			this.DisplayDateInUSFormat = rc.DisplayDateInUSFormat;
			this.DisplayZeroAsNone = rc.DisplayZeroAsNone;

			this.FootnoteIndexer = rc.FootnoteIndexer;

			this.IsNumeric = rc.IsNumeric;
			this.IsRatio = rc.IsRatio;

			this.NonNumbericText = rc.NonNumbericText;
			this.NumericAmount = rc.NumericAmount;
			
			this.RoundedNumericAmount = rc.RoundedNumericAmount;
			this.ShowCurrencySymbol = rc.ShowCurrencySymbol;

			if( EmbedReport.HasMatch( rc.NonNumbericText ) )
			    this.EmbeddedReport = EmbedReport.LoadAndParse( rc.NonNumbericText );
		}

		public override bool Equals( object obj )
		{
			Cell rc = (Cell)obj;

			if( Id != rc.Id ) return false;

			return ValueEquals( rc );
		}

		public bool ValueEquals( Cell rc )
		{
			List<string> diffs = null;
			return ValueEquals( rc, ref diffs );
		}

		public bool ValueEquals( Cell rc, ref List<string> diffs )
		{
			bool ret = true;

			if( this.DisplayZeroAsNone != rc.DisplayZeroAsNone )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "DisplayZeroAsNone" );
				else
					return ret;
			}

			if( this.FootnoteIndexer != rc.FootnoteIndexer )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "FootnoteIndexer" );
				else
					return ret;
			}

			if( this.IsNumeric != rc.IsNumeric )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "IsNumeric" );
				else
					return ret;
			}

			if( this.IsRatio != rc.IsRatio )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "IsRatio" );
				else
					return ret;
			}

			if( this.NonNumbericText != rc.NonNumbericText )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "NonNumbericText" );
				else
					return ret;
			}

			if( this.NumericAmount != rc.NumericAmount )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "NumericAmount" );
				else
					return ret;
			}

			if( this.RoundedNumericAmount != rc.RoundedNumericAmount )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "RoundedNumericAmount" );
				else
					return ret;
			}

			if( this.ShowCurrencySymbol != rc.ShowCurrencySymbol )
			{
				ret = false;

				if( diffs != null )
					diffs.Add( "ShowCurrencySymbol" );
				else
					return ret;
			}

			if( this.IsIndependantCurrency != rc.IsIndependantCurrency )
			{
				if( this.ShowCurrencySymbol != rc.ShowCurrencySymbol )
				{
					ret = false;

					if( diffs != null )
						diffs.Add( "IsIndependantCurrency" );
					else
						return ret;
				}
			}

            if( this.DisplayDateInUSFormat != rc.DisplayDateInUSFormat && !this.IsNumeric )
			{
				if( !( this.IsNil || string.IsNullOrEmpty( this.NonNumbericText ) ) )
				{
					ret = false;

					if( diffs != null )
						diffs.Add( "DisplayDateInUSFormat" );
					else
						return ret;
				}
			}

			if( rc.ShowCurrencySymbol )
			{
				if( this.CurrencyCode != rc.CurrencyCode )
				{
					ret = false;

					if( diffs != null )
						diffs.Add( "CurrencyCode" );
					else
						return ret;
				}

				if( this.CurrencySymbol != rc.CurrencySymbol )
				{
					ret = false;

					if( diffs != null )
						diffs.Add( "CurrencySymbol" );
					else
						return ret;
				}
			}

			return ret;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IComparable Members

		public int CompareTo( object inObj )
		{
			return this.Id.CompareTo( ( inObj as Cell ).Id );
		}

		#endregion

		public override string ToString()
		{
			bool isRounded = !( string.IsNullOrEmpty( this.RoundedNumericAmount ) || this.RoundedNumericAmount == "0" );
			return this.ToString( isRounded );
		}

		public string ToString( InstanceReport report )
		{
			bool isRounded = !string.IsNullOrEmpty( report.RoundingOption );
			return this.ToString( isRounded );
		}

		public string ToString( bool isRounded )
		{
			StringBuilder sb = new StringBuilder();
			if( this.ShowCurrencySymbol && !string.IsNullOrEmpty( this.CurrencySymbol ) )
				sb.Append( this.CurrencySymbol + " " );

			if( !this.IsNumeric )
				sb.Append( this.NonNumbericText );
			else if( isRounded )
				sb.Append( this.RoundedNumericAmount );
			else
				sb.Append( this.NumericAmount );

			if( !string.IsNullOrEmpty( this.FootnoteIndexer ) )
				sb.Append( " " + this.FootnoteIndexer );

			return sb.ToString();
		}

		#region ICloneable Members

		public object Clone()
		{
			Cell newCell = (Cell)this.MemberwiseClone();

			if( this.HasEmbeddedReport )
				newCell.EmbeddedReport = EmbedReport.LoadAndParse( this.NonNumbericText );

			return newCell;
		}

		#endregion
	}
}

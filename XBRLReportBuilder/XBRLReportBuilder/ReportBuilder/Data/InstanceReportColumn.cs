//=============================================================================
// InstanceReportColumn (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class contains information for a report columns.
//=============================================================================

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;

using Aucent.MAX.AXE.XBRLParser;
using XBRLReportBuilder.Utilities;

namespace XBRLReportBuilder
{
	/// <summary>
	/// InstanceReportColumn
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// 
	[Serializable]
	[XmlInclude( typeof( LabelLine ) )]
	public partial class InstanceReportColumn : ICloneable
	{
		//[XmlElement( Order = 5 )]
		public string CurrencyCode = InstanceUtils.USDCurrencyCode;

		//[XmlElement( Order = 6 )]
		public string FootnoteIndexer = string.Empty;//store promoted footnotes 

		//[XmlElement( Order = 7 )]
		public bool hasSegments
		{
			get
			{
				if( this.Segments == null )
					return false;

				if( this.Segments.Count == 0 )
					return false;

				return true;
			}
			set
			{
				//need a setter so that the property will xml serialize, 
				//but we never will actually set a property in this case
			}
		}

		//[XmlElement( Order = 8 )]
		public bool hasScenarios
		{
			get
			{
				bool bReturn = false;
				if (this.Scenarios != null)
				{
					bReturn = this.Scenarios.Count > 0;
				}
				return bReturn;
			}
			set
			{
				//need a setter so that the property will xml serialize,
				//but we never will actually set a property in this case
			}
		}

		//[XmlElement( Order = 9 )]
		public MergedContextUnitsWrapper MCU { get; set; }

		private string currencySymbol = InstanceUtils.USDCurrencySymbol;

		//[XmlElement( Order = 10 )]
		public string CurrencySymbol
		{
			get { return currencySymbol; }
			set { currencySymbol = value; }
		}


		#region properties
		[XmlIgnore]
        public TimeSpan ReportingSpan
        {
            get 
			{
				TimeSpan retValue = TimeSpan.MinValue;
				if (this.MCU != null && this.MCU.contextRef != null)
				{
					retValue = this.MCU.contextRef.PeriodEndDate - this.MCU.contextRef.PeriodStartDate;
				}
				return retValue; 
			}
        }

		[XmlIgnore]
		public ContextProperty MyContextProperty
		{
			get
			{
				if (this.MCU == null)
					return null;
				else
					return this.MCU.contextRef;
			}
		}

		[XmlIgnore]
		public ArrayList Segments
		{
			get
			{
				ArrayList retValue = null;
				if (this.MCU != null && this.MCU.contextRef != null)
				{
					retValue = this.MCU.contextRef.Segments;
				}
				return retValue;
			}
			set
			{
				if (this.MCU == null)
				{
					ContextProperty cp = new ContextProperty();
					this.MCU = new MergedContextUnitsWrapper(string.Empty, cp);
				}
				this.MCU.contextRef.Segments = value;
			}
		}

		[XmlIgnore]
		public ArrayList Scenarios
		{
			get
			{
				ArrayList retValue = null;
				if (this.MCU != null && this.MCU.contextRef != null)
				{
					retValue = this.MCU.contextRef.Scenarios;
				}
				return retValue;
			}
			set
			{
				if (this.MCU == null)
				{
					ContextProperty cp = new ContextProperty();
					this.MCU = new MergedContextUnitsWrapper(string.Empty, cp);
				}
				this.MCU.contextRef.Scenarios = value;

			}
		}

		[XmlIgnore]
		public List<UnitProperty> Units
		{
			get
			{
				if( this.MCU == null )
					return null;
				else
					return this.MCU.UPS;
			}
			set
			{
				if (this.MCU == null)
				{
					this.MCU = new MergedContextUnitsWrapper(string.Empty, new ContextProperty());
				}
				this.MCU.UPS = value;
			}
		}

		[XmlIgnore]
		public Element.PeriodType MyPeriodType
		{
			get
			{
				Element.PeriodType retValue = Element.PeriodType.na;
				if (this.MCU != null && this.MCU.contextRef != null)
				{
					retValue = this.MCU.contextRef.PeriodType;
				}
				return retValue;
			}
		}

		/// <summary>
		/// Returns the integer value of the <see cref="Element.PeriodType"/> 
		/// associated with this <see cref="InstanceReportColumn"/>.
		/// </summary>
		[XmlIgnore]
		public int IntPeriodType
		{
			get
			{
				return ((int)(this.MyPeriodType));
			}
		}

		[XmlIgnore]
		public bool IsPseudoColumn = false;

		[XmlIgnore]
		public bool IsPseudoColumnChanged = false;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new InstanceReportColumn.
		/// </summary>
		public InstanceReportColumn() : base(){}

		public InstanceReportColumn( InstanceReportColumn arg ) : this()
		{
			this.Id = arg.Id;

			this.Labels = new List<LabelLine>();
			foreach (LabelLine ll in arg.Labels)
			{
				this.Labels.Add(new LabelLine(ll));
			}

			if (arg.MCU != null)
			{
				this.MCU = arg.MCU.Clone() as MergedContextUnitsWrapper;
			}

            this.CurrencySymbol = arg.CurrencySymbol;
		}
		#endregion

		public object Clone()
		{
			InstanceReportColumn irc = (InstanceReportColumn)this.MemberwiseClone();

			if( this.MCU != null )
				irc.MCU = (MergedContextUnitsWrapper)this.MCU.Clone();

			if (this.Labels != null)
			{
				irc.Labels = new List<LabelLine>();
				foreach (LabelLine ll in this.Labels)
				{
					irc.Labels.Add( (LabelLine)ll.Clone() );
				}
			}

			return irc;
		}

        public void FinalProcessing()
        {
			if (Units != null)
			{
				foreach (UnitProperty up in Units)
				{
					if (up.UnitType == UnitProperty.UnitTypeCode.Standard)
					{
						up.MultiplyMeasures = null;
						up.NumeratorMeasure = null;
						up.DenominatorMeasure = null;
					}
					else if (up.UnitType == UnitProperty.UnitTypeCode.Divide)
					{
						up.StandardMeasure = null;
						up.MultiplyMeasures = null;
					}
					else if (up.UnitType == UnitProperty.UnitTypeCode.Multiply)
					{
						up.NumeratorMeasure = null;
						up.DenominatorMeasure = null;
					}
				}
			}
        }

        public void ClearLabels()
        {
            Labels.Clear();
        }

		public void ClearReportingPeriod()
		{
			if (this.MyContextProperty != null)
			{
				this.MyContextProperty.PeriodDisplayName = null;
				this.MyContextProperty.PeriodStartDate = DateTime.MinValue;
				this.MyContextProperty.PeriodEndDate = DateTime.MinValue;
				this.MyContextProperty.PeriodType = Element.PeriodType.na;
			}

			//Remove the period labels
			for(int i = this.Labels.Count - 1; i >= 0; i--)
			{
				LabelLine ll = this.Labels[i];
				string[] strArray = ll.Label.Split('-');

				DateTime dt;
				if(strArray.Length == 2 &&
					DateTime.TryParse(strArray[0], out dt) &&
					DateTime.TryParse(strArray[1], out dt))
				{
					this.Labels.RemoveAt(i);
				}
				else if(DateTime.TryParse(ll.Label, out dt))
				{
					this.Labels.RemoveAt(i);
				}
			}
		}

		public bool HasCustomUnits()
		{
			return ReportUtils.Exists( this.Units, InstanceReportColumn.IsCustomUnit );
		}

		public bool HasExchangeRate()
		{
			if( this.MCU == null || this.MCU.UPS == null || this.MCU.UPS.Count == 0 )
				return false;

			if( ReportUtils.Exists( this.MCU.UPS, IsExchangeRateUnit ) )
				return true;

			return false;
		}

		public bool HasMonetary()
		{
			if( this.MCU == null || this.MCU.UPS == null || this.MCU.UPS.Count == 0 )
				return false;

			if( ReportUtils.Exists( this.MCU.UPS, IsMonetaryUnit ) )
				return true;

			return false;
		}

		public bool HasPerShare()
		{
			if( this.MCU == null || this.MCU.UPS == null || this.MCU.UPS.Count == 0 )
				return false;

			if( ReportUtils.Exists( this.MCU.UPS, IsEPSUnit ) )
				return true;

			return false;
		}

		public bool IsAdjusted( XmlDocument membersXDoc )
		{
			int index = -1;
			return this.IsAdjusted( membersXDoc, out index );
		}

		public bool IsAdjusted( XmlDocument membersXDoc, out int index )
		{
			index = -1;

			if( membersXDoc == null )
				return false;

			if( this.Segments == null || this.Segments.Count == 0 )
				return false;

			int i = 0;
			foreach( Segment seg in this.Segments )
			{
				if( ColumnRowRequirement.IsSegmentAdjusted( seg, membersXDoc ) )
				{
					index = i;
					return true;
				}

				i++;
			}

			return false;
		}

		public bool IsAsPreviouslyReported( XmlDocument membersXDoc )
		{
			int index = -1;
			return this.IsAsPreviouslyReported( membersXDoc, out index );
		}

		public bool IsAsPreviouslyReported( XmlDocument membersXDoc, out int index )
		{
			index = -1;

			if( membersXDoc == null )
				return false;

			if( this.Segments == null || this.Segments.Count == 0 )
				return false;

			int i = 0;
			foreach( Segment seg in this.Segments )
			{
				if( ColumnRowRequirement.IsSegmentPreviouslyReported( seg, membersXDoc ) )
				{
					index = i;
					return true;
				}

				i++;
			}

			return false;
		}

		public static bool IsCustomUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			if( up.StandardMeasure == null ||
				string.IsNullOrEmpty( up.StandardMeasure.MeasureValue ) )
				return false;

			if( up.UnitType != UnitProperty.UnitTypeCode.Standard )
				return false;

			if( InstanceReportColumn.IsPureUnit( up ) ||
				InstanceReportColumn.IsSharesUnit( up ) ||
				InstanceReportColumn.IsMonetaryUnit( up ) )
				return false;

			return true;
		}

		public static bool IsEPSUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			//currency / shares = "shares"
			if( up.UnitType == UnitProperty.UnitTypeCode.Divide &&
				string.Equals( up.NumeratorMeasure.MeasureNamespace, InstanceUtils.CURRENCY_NAMESPACE, StringComparison.CurrentCultureIgnoreCase ) &&
				string.Equals( up.DenominatorMeasure.MeasureValue, "shares", StringComparison.CurrentCultureIgnoreCase ) )
			{
				return true;
			}

			return false;
		}

		public static bool IsExchangeRateUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			//currency / currency = "exchangeRate"
			if( up.UnitType == UnitProperty.UnitTypeCode.Divide &&
				string.Equals( up.NumeratorMeasure.MeasureNamespace, InstanceUtils.CURRENCY_NAMESPACE, StringComparison.CurrentCultureIgnoreCase ) &&
				string.Equals( up.DenominatorMeasure.MeasureNamespace, InstanceUtils.CURRENCY_NAMESPACE, StringComparison.CurrentCultureIgnoreCase ) )
			{
				return true;
			}

			return false;
		}

		public static bool IsMonetaryUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			if( up.UnitType == UnitProperty.UnitTypeCode.Standard &&
				string.Equals( up.StandardMeasure.MeasureNamespace, InstanceUtils.CURRENCY_NAMESPACE, StringComparison.CurrentCultureIgnoreCase ) )
			{
				return true;
			}

			return false;
		}

		public static bool IsPureUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			if( up.UnitType == UnitProperty.UnitTypeCode.Standard &&
				string.Equals( up.StandardMeasure.MeasureValue, "pure", StringComparison.CurrentCultureIgnoreCase ) )
			{
				return true;
			}

			return false;
		}

		public static bool IsSharesUnit( UnitProperty up )
		{
			if( up == null )
				return false;

			if( up.UnitType == UnitProperty.UnitTypeCode.Standard &&
				string.Equals( up.StandardMeasure.MeasureValue, "shares", StringComparison.CurrentCultureIgnoreCase ) )
			{
				return true;
			}

			return false;
		}


        /// <summary>
        /// Adds a label to the top of the stack on the column.
        /// </summary>
        /// <param name="label">The name of the label.</param>
        /// <param name="key">The key to use to identify the label.</param>
        public void PrependLabel( string label, string key )
        {
            LabelLine newLine = new LabelLine( this.Labels.Count, label, key );

            this.Labels.Insert( 0, newLine );
        }

        public bool ContextEquals( InstanceReportColumn irc )
        {
            if ( !SegmentAndScenarioEquals( irc ) ) return false;

            return ReportingPeriodEquals( irc );
        }

		public override bool Equals(object obj)
		{
			InstanceReportColumn irc = (InstanceReportColumn)obj;
			if ( Id != irc.Id )
				return false;

            return ContextEquals( irc );
		}

		public bool SegmentAndScenarioEquals( InstanceReportColumn irc )
		{
			if( hasScenarios != irc.hasScenarios )
				return false;

			if( hasSegments != irc.hasSegments )
				return false;

			if( hasSegments )
			{
				if( Segments.Count != irc.Segments.Count )
					return false;

				for( int i = 0; i < Segments.Count; ++i )
				{
					if( !SegmentEquals( (Segment)Segments[ i ], (Segment)irc.Segments[ i ] ) )
						return false;
				}
			}

			if( hasScenarios )
			{
				if( Scenarios.Count != irc.Scenarios.Count ) return false;
				for( int i = 0; i < Scenarios.Count; ++i )
				{
					if( !ScenarioEquals( (Scenario)Scenarios[ i ], (Scenario)irc.Scenarios[ i ] ) )
						return false;
				}
			}

			return true;
		}

		public bool SegmentsContain( ArrayList segments )
		{
			List<Segment> segmentList = new List<Segment>();
			segmentList.AddRange( (Segment[])segments.ToArray( typeof( Segment ) ) );
			return SegmentsContain( segmentList );
		}

		public bool SegmentsContain( List<Segment> segments )
		{
			foreach( Segment seg in segments )
			{
				bool found = false;
				foreach( Segment cSeg in this.Segments )
				{
					if( seg.Equals( cSeg ) )
					{
						found = true;
						break;
					}
				}

				if( !found )
					return false;
			}

			return true;
		}

        public bool ReportingPeriodEquals( InstanceReportColumn irc )
        {
			return this.ReportingPeriodEquals( irc.MyContextProperty );
        }

		public bool ReportingPeriodEquals( ContextProperty cp )
		{
			if( this.MyContextProperty == null )
			{
				if( cp != null )
					return false;

				if( cp == null )
					return true;
			}

			if( this.MyContextProperty == cp ) return true;
			if( this.MyContextProperty.PeriodType != cp.PeriodType ) return false;
			if( this.MyContextProperty.PeriodStartDate != cp.PeriodStartDate ) return false;
			if( this.MyContextProperty.PeriodEndDate != cp.PeriodEndDate ) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public string GetReportingPeriodString()
		{
            StringBuilder sb = new StringBuilder();
			if (this.MyContextProperty != null)
			{
				if (this.MyContextProperty.PeriodType == Element.PeriodType.instant) //instant
				{
					sb.Append(this.MyContextProperty.PeriodStartDate.ToString("d"));
				}
				else //duration
				{
					sb.Append(this.MyContextProperty.PeriodStartDate.ToString("d")).Append(" - ").Append(this.MyContextProperty.PeriodEndDate.ToShortDateString());
				}
			}
            return sb.ToString();
		}

		public string GetSegmentsString(bool includeScenarios, bool includePromotedLabels, bool includePrevReportScenario, bool includeAdjustmentLabel, bool includeCurrency, string reportName)
		{
			StringBuilder sbSegment = new StringBuilder();
			
			int idxLastParen = reportName.LastIndexOf('(');
			string promotedLabels = idxLastParen > 0 ? reportName.Substring(idxLastParen) : string.Empty;
			for (int index = 0; index < this.Segments.Count; index++)
			{
				Segment s = this.Segments[index] as Segment;

				string strSegment = InstanceUtils.BuildSegmentScenarioName(s);
				if (includePromotedLabels ||
						!promotedLabels.Contains(strSegment))
				{
                    if (!includePrevReportScenario &&
                            s.DimensionInfo != null &&
                            s.DimensionInfo.dimensionId == InstanceUtils._ScenarioAxis &&
                            s.DimensionInfo.Id == InstanceUtils._ScenarioAsPreviouslyReport)
                    {
                        //If the caller does not want to include the "As Previously Reported" scenario in the label
                        //and we are on that scenario, we need to skip it
                        continue;
                    }

                    if (!includeAdjustmentLabel &&
                        s.DimensionInfo != null &&
                        s.DimensionInfo.dimensionId == InstanceUtils._ScenarioAxis &&
                        s.DimensionInfo.Id == InstanceUtils._ScenarioAdjustment)
                    {
                        //If the caller does not want to include the "Adjustment" scenario in the label
                        //and we are on that scenario, we need to skip it
                        continue;
                    }

					//Not the first value appended to the string, append the delimeter
					if (sbSegment.Length > 0)
						sbSegment.Append(" | ");

					sbSegment.Append(strSegment);
				}
			}

			if (includeScenarios &&
				this.hasScenarios)
			{
				for (int index = 0; index < this.Scenarios.Count; index++)
				{
					Scenario s = this.Scenarios[index] as Scenario;

					string strScenario = InstanceUtils.BuildSegmentScenarioName(s);
					if (includePromotedLabels ||
						!reportName.Contains(strScenario))
					{
                        if (!includePrevReportScenario &&
                            s.DimensionInfo != null &&
                            s.DimensionInfo.dimensionId == InstanceUtils._ScenarioAxis &&
                            s.DimensionInfo.Id == InstanceUtils._ScenarioAsPreviouslyReport)
                        {
                            //If the caller does not want to include the "As Previously Reported" scenario in the label
                            //and we are on that scenario, we need to skip it
                            continue;
                        }

                        if (!includeAdjustmentLabel &&
                            s.DimensionInfo != null &&
                            s.DimensionInfo.dimensionId == InstanceUtils._ScenarioAxis &&
                            s.DimensionInfo.Id == InstanceUtils._ScenarioAdjustment)
                        {
                            //If the caller does not want to include the "Adjustment" scenario in the label
                            //and we are on that scenario, we need to skip it
                            continue;
                        }

                        //Not the first value appended to the string, append the delimeter
                        if (sbSegment.Length > 0)
                            sbSegment.Append(" | ");

						sbSegment.Append(strScenario);
					}
				}
			}
			if( includeCurrency )
			{
				string curCode = InstanceUtils.GetCurrencyCodeFromMCU( this.MCU );
				sbSegment.Append( curCode );
			}

			return sbSegment.ToString();
		}

        public static bool SegmentEquals( Segment s1, Segment s2 )
        {
			if( s1 == null )
			{
				if( s2 == null )
					return true;
				else
					return false;
			}
			else if( s2 == null )
			{
				return false;
			}


			if( s1.DimensionInfo == null )
			{
				if( s2.DimensionInfo == null )
					return true;
				else
					return false;
			}
			else if( s2.DimensionInfo == null )
			{
				return false;
			}

			if( !string.Equals( s1.DimensionInfo.Id, s2.DimensionInfo.Id ) )
				return false;

			if( !string.Equals( s1.DimensionInfo.dimensionId, s2.DimensionInfo.dimensionId ) )
				return false;

            return true;
        }

        protected bool ScenarioEquals( Scenario s1, Scenario s2 )
        {
            if ( s1.ValueName.CompareTo( s2.ValueName ) != 0 ) return false;
            if ( s1.ValueType.CompareTo( s2.ValueType ) != 0 ) return false;

            return true;
        }

		public override string ToString()
		{
			return this.Label;
		}

		public void RemoveAdjustedPreviouslyReported( XmlDocument membersXDoc )
		{
			int adjustedIndex = -1;
			while( this.IsAdjusted( membersXDoc, out adjustedIndex ) )
			{
				this.Segments.RemoveAt( adjustedIndex );
			}

			int asPrevReportedIndex = -1;
			while( this.IsAsPreviouslyReported( membersXDoc, out asPrevReportedIndex ) )
			{
				this.Segments.RemoveAt( asPrevReportedIndex );
			}
		}

		public CalendarPeriod GetCalendarPeriod()
		{
			CalendarPeriod cp = new CalendarPeriod(
				this.MyContextProperty.PeriodStartDate,
				this.MyContextProperty.PeriodEndDate
			);
			cp.PeriodType = this.MyContextProperty.PeriodType;
			return cp;
		}

		public bool ContainsSegmentMembers( IEnumerable<CommandIterator> iterators )
		{
			foreach( CommandIterator iterator in iterators )
			{
				if( !iterator.IsAxis )
					continue;

				Segment specificMember = (Segment)iterator.TempCurrentMemberValue;
				if( !this.IsSegmentInUse( specificMember ) )
					return false;
			}

			return true;
		}

		public Segment GetAxisMember( string axis, Segment defaultMember )
		{
			if( this.Segments == null )
				return defaultMember;

			foreach( Segment segment in this.Segments )
			{
				if( segment.DimensionInfo.dimensionId == axis )
					return segment;
			}

			return defaultMember;
		}

		public bool IsSegmentInUse( Segment specificMember )
		{
			//get this columns copy of the segment
			Segment colSeg = this.GetAxisMember( specificMember.DimensionInfo.dimensionId, null );

			//null represents the default member (this segment cannot be found in the this.Segments)
			if( colSeg == null )
			{
				//Were we actually looking for the default member?
				//If so, this will return true.  Otherwise, false.
				return specificMember.IsDefaultForEntity;
			}

			//this is not the default member, so check if the "specific" member is a match
			if( colSeg.DimensionInfo.Id == specificMember.DimensionInfo.Id )
			{
				return true;
			}

			return false;
		}

		public Dictionary<string, int> GetUniqueInUseSegments()
		{
			Dictionary<string, int> inUseSegments = new Dictionary<string, int>();

			if( this.Segments == null || this.Segments.Count == 0 )
				return inUseSegments;

			foreach( Segment seg in this.Segments )
			{
				string segKey = seg.DimensionInfo.dimensionId + ":" + seg.DimensionInfo.Id;
				if( !inUseSegments.ContainsKey( segKey ) )
				{
					inUseSegments[ segKey ] = 1;
				}
			}

			return inUseSegments;
		}

		private ArrayList originalSegments = null;
		public void HideSpecialSegments( XmlDocument membersXDoc )
		{
			this.originalSegments = new ArrayList( this.Segments );
			this.RemoveAdjustedPreviouslyReported( membersXDoc );
		}

		public void ShowSpecialSegments( XmlDocument membersXDoc )
		{
			this.Segments = this.originalSegments;
			this.originalSegments = null;
		}

		public bool CurrencyEquals( InstanceReportColumn that )
		{
			if(string.Equals( this.CurrencyCode, that.CurrencyCode ) )
				return true;

			if( string.IsNullOrEmpty( this.CurrencyCode ) && string.Equals( that.CurrencyCode, InstanceUtils.USDCurrencyCode ) )
				return true;

			if( string.IsNullOrEmpty( that.CurrencyCode ) && string.Equals( this.CurrencyCode, InstanceUtils.USDCurrencyCode ) )
				return true;

			return false;
		}

		public void RemoveMissingSegmentLabels()
		{
			this.Labels.RemoveAll( 
				ll =>
				{
					bool removeLabel = true;
					foreach( Segment seg in this.Segments )
					{
						if( string.Equals( ll.Label, seg.ValueName ) )
						{
							removeLabel = false;
							break;
						}
					}

					return removeLabel;
				} );
		}

		public void ApplyCurrencyLabel()
		{
			if( string.IsNullOrEmpty( this.CurrencyCode ) )
				return;

			if( this.HasCurrencyLabel() )
				return;

			string currencyLabel = this.CurrencyCode;
			if( !string.IsNullOrEmpty( this.CurrencySymbol ) )
				currencyLabel += " (" + this.CurrencySymbol + ")";

			LabelLine ll = new LabelLine( this.Labels.Count + 1, currencyLabel );
			this.Labels.Add( ll );
		}

		public bool HasCurrencyLabel()
		{
			if( string.IsNullOrEmpty( this.CurrencyCode ) )
				return false;

			foreach( LabelLine ll in this.Labels )
			{
				if( ll.Label.StartsWith( this.CurrencyCode ) )
					return true;
			}

			return false;
		}

		public void RemoveCurrencyLabel()
		{
			if( string.IsNullOrEmpty( this.CurrencyCode ) )
				return;

			for( int i = 0; i < this.Labels.Count; )
			{
				LabelLine lbl = this.Labels[ i ];
				if( !lbl.Label.StartsWith( this.CurrencyCode ) )
				{
					i++;
					continue;
				}

				if( !string.IsNullOrEmpty( this.CurrencySymbol ) )
				{
					if( lbl.Label.Contains( this.CurrencySymbol ) )
						this.Labels.RemoveAt( i );
					else
						i++;
				}
				else
				{
					this.Labels.RemoveAt( i );
				}
			}
		}
	}
}

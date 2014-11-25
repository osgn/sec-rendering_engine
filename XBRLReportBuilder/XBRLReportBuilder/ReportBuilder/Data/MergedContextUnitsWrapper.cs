//=============================================================================
// MergedContextUnitsWarpper (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class impelements the comparer for merged context unit(s) wrappers.
//=============================================================================

using System;
using System.Text;
using System.Collections;

using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.Common.Data;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	/// <summary>
	/// MergedContextUnitsWarpper
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
    [Serializable]
	[XmlInclude( typeof( Segment ) )]
	[XmlInclude( typeof( Scenario ) )]
	[XmlInclude( typeof( UnitProperty ) )]
	[XmlInclude( typeof( ContextProperty ) )]
	public class MergedContextUnitsWrapper : IHasContextProperty, IComparable, ICloneable
	{
		#region properties

		public string KeyName;

		public ContextProperty contextRef { get; set; }

		public List<UnitProperty> UPS { get; set; }

		//default to USD
		public string CurrencySymbol = InstanceUtils.USDCurrencySymbol;
		
		//default to USD
		private string currencyCode = InstanceUtils.USDCurrencyCode;
		public string CurrencyCode
		{
			get { return this.currencyCode; }
			set
			{
				this.currencyCode = value;

				if( !value.Contains( "/" ) )
					this.OriginalCurrencyCode = value;
			}
		}

		private string originalCurrencyCode = InstanceUtils.USDCurrencyCode;
		public string OriginalCurrencyCode
		{
			get { return this.originalCurrencyCode; }
			set { this.originalCurrencyCode = value; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new MergedContextUnitsWarpper.
		/// </summary>
		/// 
		public MergedContextUnitsWrapper()
		{
			this.UPS = new List<UnitProperty>();
		}

		public MergedContextUnitsWrapper(string keyName, ContextProperty cp) : this()
		{
			this.KeyName = keyName;
			this.contextRef = cp;
		}

		public MergedContextUnitsWrapper(string keyName, ContextProperty cp, List<UnitProperty> ups) : this( keyName, cp )
		{
			this.UPS = ups;
		}

        public MergedContextUnitsWrapper( string keyName, ContextProperty cp, UnitProperty up )
            : this( keyName, cp )
        {
            if ( up != null )
            {
                this.UPS.Add( up );
            }
        }

		/// <summary>
		/// Unique method for generating the "generic" key of a column.  Test this method extensively before using.
		/// </summary>
		/// <returns></returns>
		public string BuildDateAndSegmentKey()
		{
			StringBuilder keyNameString = new StringBuilder();
			InstanceUtils.BuildSharedKeyParts( keyNameString, this.contextRef, false );
			InstanceUtils.AppendScenarios( keyNameString, this.contextRef );

			string key = keyNameString.ToString();
			return key;
		}

#endregion

		#region IComparable Members

		public int CompareTo(object inObj)
		{
			return this.KeyName.CompareTo ((inObj as MergedContextUnitsWrapper).KeyName);
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			//Clones all base type properties and shallow clones any complex types
			//Complex types will be recloned further down in the code.
			MergedContextUnitsWrapper mcu = this.MemberwiseClone() as MergedContextUnitsWrapper;

			mcu.contextRef = new ContextProperty( this.contextRef.ContextID );
			mcu.contextRef.EntitySchema = this.contextRef.EntitySchema;
			mcu.contextRef.EntityValue = this.contextRef.EntityValue;
			mcu.contextRef.PeriodDisplayName = this.contextRef.PeriodDisplayName;
			mcu.contextRef.PeriodEndDate = this.contextRef.PeriodEndDate;
			mcu.contextRef.PeriodStartDate = this.contextRef.PeriodStartDate;
			mcu.contextRef.PeriodType = this.contextRef.PeriodType;

			mcu.contextRef.Segments = new ArrayList();

			#region clone segments

			foreach( Segment seg in this.contextRef.Segments )
			{
				Segment segClone = new Segment();

				//Setting DimensionInfo sets some of the other properties on the Segment, so to ensure that
				//the object is truely cloned set DimensionInfo first then set the other properties
				if (seg.DimensionInfo != null)
				{
					segClone.DimensionInfo = seg.DimensionInfo.Clone() as ContextDimensionInfo;
				}

				segClone.Name = seg.Name;
				segClone.ValueName = seg.ValueName;
				segClone.ValueType = seg.ValueType;
				segClone.Namespace = seg.Namespace;
				segClone.Schema = seg.Schema;

				mcu.contextRef.AddSegment( segClone );
			}

			#endregion

			#region clone scenarios

			foreach( Scenario scen in this.contextRef.Scenarios )
			{
				Segment scenClone = new Segment();

				//Setting DimensionInfo sets some of the other properties on the scenario, so to ensure that
				//the object is truely cloned set DimensionInfo first then set the other properties
				if (scen.DimensionInfo != null)
				{
					scenClone.DimensionInfo = scen.DimensionInfo.Clone() as ContextDimensionInfo;
				}

				scenClone.Name = scen.Name;
				scenClone.ValueName = scen.ValueName;
				scenClone.ValueType = scen.ValueType;
				scenClone.Namespace = scen.Namespace;
				scenClone.Schema = scen.Schema;

				mcu.contextRef.AddScenario( scen );
			}

			#endregion

			#region clone unit properties

			mcu.UPS = new List<UnitProperty>();
			foreach (UnitProperty up in this.UPS)
			{
				UnitProperty upClone = new UnitProperty(up.UnitID, up.UnitType);
				upClone.Scale = up.Scale;

				if (up.StandardMeasure != null)
				{
					upClone.StandardMeasure = new Aucent.MAX.AXE.XBRLParser.Measure();
					upClone.StandardMeasure.MeasureNamespace = up.StandardMeasure.MeasureNamespace;
					upClone.StandardMeasure.MeasureSchema = up.StandardMeasure.MeasureSchema;
					upClone.StandardMeasure.MeasureValue = up.StandardMeasure.MeasureValue;
				}

				if (up.MultiplyMeasures != null)
				{
					upClone.MultiplyMeasures = new Aucent.MAX.AXE.XBRLParser.Measure[up.MultiplyMeasures.Length];
					for (int i = 0; i < up.MultiplyMeasures.Length; i++)
					{
						upClone.MultiplyMeasures[i] = new Aucent.MAX.AXE.XBRLParser.Measure();
						upClone.MultiplyMeasures[i].MeasureNamespace = up.MultiplyMeasures[i].MeasureNamespace;
						upClone.MultiplyMeasures[i].MeasureSchema = up.MultiplyMeasures[i].MeasureSchema;
						upClone.MultiplyMeasures[i].MeasureValue = up.MultiplyMeasures[i].MeasureValue;
					}
				}

				if (up.NumeratorMeasure != null)
				{
					upClone.NumeratorMeasure = new Aucent.MAX.AXE.XBRLParser.Measure();
					upClone.NumeratorMeasure.MeasureNamespace = up.NumeratorMeasure.MeasureNamespace;
					upClone.NumeratorMeasure.MeasureSchema = up.NumeratorMeasure.MeasureSchema;
					upClone.NumeratorMeasure.MeasureValue = up.NumeratorMeasure.MeasureValue;
				}

				if (up.DenominatorMeasure != null)
				{
					upClone.DenominatorMeasure = new Aucent.MAX.AXE.XBRLParser.Measure();
					upClone.DenominatorMeasure.MeasureNamespace = up.DenominatorMeasure.MeasureNamespace;
					upClone.DenominatorMeasure.MeasureSchema = up.DenominatorMeasure.MeasureSchema;
					upClone.DenominatorMeasure.MeasureValue = up.DenominatorMeasure.MeasureValue;
				}

				mcu.UPS.Add(upClone);
			}

			#endregion

            mcu.CurrencySymbol = this.CurrencySymbol;
            mcu.CurrencyCode = this.CurrencyCode;

			return mcu;
		}

		#endregion
	}
}
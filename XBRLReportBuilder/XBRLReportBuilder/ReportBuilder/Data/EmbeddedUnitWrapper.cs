using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLParser;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using XBRLReportBuilder.Utilities;

namespace XBRLReportBuilder
{
	[Serializable]
	[XmlInclude( typeof( UnitProperty ) )]
	public class EmbeddedUnitWrapper
	{
		public List<UnitProperty> OtherUnits { get; set; }

		public Measure DenominatorMeasure
		{
			get { return this.UnderlyingUnitProperty.DenominatorMeasure; }
		}

		public Measure[] MultiplyMeasures
		{
			get { return this.UnderlyingUnitProperty.MultiplyMeasures; }
		}

		public Measure NumeratorMeasure
		{
			get { return this.UnderlyingUnitProperty.NumeratorMeasure; }
		}

		public int Scale
		{
			get { return this.UnderlyingUnitProperty.Scale; }
		}

		public Measure StandardMeasure
		{
			get { return this.UnderlyingUnitProperty.StandardMeasure; }
		}

		public string UnitID
		{
			get { return this.UnderlyingUnitProperty.UnitID; }
		}

		public UnitProperty.UnitTypeCode UnitType
		{
			get { return this.UnderlyingUnitProperty.UnitType; }
		}

		public UnitProperty UnderlyingUnitProperty { get; set; }

        //TODO make private
		public EmbeddedUnitWrapper() { }

        //TODO remove - unused
		public EmbeddedUnitWrapper( UnitProperty up )
		{
			this.UnderlyingUnitProperty = up;
		}

        /// <summary>
        /// Create a new <see cref="EmbeddedUnitWrapper"/> with the given
        /// values.
        /// </summary>
        /// <param name="up">The underlying unit property.</param>
        /// <param name="otherUPs">Other unit properties.</param>
		public EmbeddedUnitWrapper( UnitProperty up, List<UnitProperty> otherUPs )
		{
			this.UnderlyingUnitProperty = up;
			this.OtherUnits = new List<UnitProperty>( otherUPs );
			if( OtherUnits.Contains( this.UnderlyingUnitProperty ) )
				OtherUnits.Remove( this.UnderlyingUnitProperty );
		}

        /// <summary>
        /// Retrieve the currency code of this wrapper's underlying unit
        /// property.
        /// </summary>
        /// <returns>A string of the currency code.</returns>
		public string GetCurrencyCode()
		{
			if( this.UnderlyingUnitProperty == null )
				return string.Empty;

			return InstanceUtils.GetCurrencyCodeFromUnit( this.UnderlyingUnitProperty );
		}

        /// <summary>
        /// Determine if the other units within this instance are custom.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if any of the other units
        /// are custom.</returns>
		public bool HasCustomUnits()
		{
			if( this.OtherUnits == null )
				return false;

			return ReportUtils.Exists( this.OtherUnits, InstanceReportColumn.IsCustomUnit );
		}

        /// <summary>
        /// Determine if the underlying unit within this instance is monetary.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if the underlying unit is
        /// monetary.</returns>
		public bool IsMonetary()
		{
			if( this.UnderlyingUnitProperty == null )
				return false;

			if( !InstanceReportColumn.IsMonetaryUnit( this.UnderlyingUnitProperty ) )
				return false;

			return true;
		}

        /// <summary>
        /// Add the provided property to the other units list.
        /// </summary>
        /// <param name="up">The <see cref="UnitProperty"/> to add to the
        /// other units list.</param>
		public void AddOtherUnits( UnitProperty up )
		{
			AddOtherUnits( new UnitProperty[]{ up } );
		}

        /// <summary>
        /// Add the provided properties to the other units list.
        /// </summary>
        /// <param name="upList">A <see cref="List{UnitProperty}"/> containing
        /// the units to add to the other units list.</param>
		public void AddOtherUnits( List<UnitProperty> upList )
		{
			AddOtherUnits( upList.ToArray() );
		}

        //TODO make private
		public void AddOtherUnits( params UnitProperty[] upList )
		{
			foreach( UnitProperty up in upList )
			{
				if( this.UnderlyingUnitProperty != null && string.Equals( this.UnitID, up.UnitID ) )
					continue;

				if( this.OtherUnits == null )
				{
					this.OtherUnits = new List<UnitProperty>();
				}
				else if( this.OtherUnits.Count > 0 )
				{
					if( ReportUtils.Exists( this.OtherUnits, ou => string.Equals(ou.UnitID, up.UnitID ) ) )
						continue;
				}

				this.OtherUnits.Add( up );
			}
		}
	}
}

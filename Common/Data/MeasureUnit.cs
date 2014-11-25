// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.Text;
using System.Collections;

using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Utilities;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// MeasureUnit, a type of markup item to support Unit Properties.
    /// </summary>
    /// <remarks>
    /// if MeasureUnitType = MeasureUnitTypeCode.Complex AND
    ///    CalculationType = CalculationTypeCode.Divide
    /// the calculation = Measure1 / Measure2
    /// if MeasureUnitType = MeasureUnitTypeCode.Complex AND 
    ///    CalculationType = CalculationTypeCode.Multiply
    /// the calculation = Measure1 x Measure2
    /// if MeasureUnitType = MeasureUnitTypeCode.Simple, should only specify Measure1
    /// if MeasureUnitType = MeasureUnitTypeCode.Currency or Shares, should only specify Measure1
	/// </remarks>
	[Serializable]
	public class MeasureUnit : MarkupItem
	{
		


		#region enum

			/// <summary>
			/// XBRL supports 5 types of measures:
			/// (1) Currency Code
			/// (2) Shares
			/// (3) Pure
			/// (4) Other simple measures
			/// (5) Other complex measures
			/// </summary>
			public enum MeasureUnitTypeCode 
            {   
                /// <summary>
                /// Not specified
                /// </summary>
                Unknown, 
                /// <summary>
                /// Monetary
                /// </summary>
                Currency, 
                /// <summary>
                /// Shares
                /// </summary>
                Shares, 
                /// <summary>
                /// Decimal
                /// </summary>
                Pure, 
                /// <summary>
                /// Other simple units
                /// </summary>
                Simple, 
                /// <summary>
                /// Other calculated units (for example, USD/Share).
                /// </summary>
                Complex
            };
			
            /// <summary>
            /// For calculated unit properties, there are two types of calculations that are supported:
            /// (1) Divide
            /// (3) Multiply
            /// </summary>
            public enum CalculationTypeCode 
            {
                /// <summary>
                /// Not specified
                /// </summary>
                NA, 
                /// <summary>
                /// Divide two numbers
                /// </summary>
                Divide, 
                /// <summary>
                /// Multiply two numbers
                /// </summary>
                Multiply
            };

		#endregion

		#region properties

        /// <summary>
        /// Shares
        /// </summary>
		public const string _shares = "shares";
        /// <summary>
        /// Pure (decimal)
        /// </summary>
        public const string _pure = "pure";

		/// <summary>
		/// Namespace for currency codes 
		/// </summary>
		public const string _currencyNamespace = "iso4217";
        /// <summary>
        /// Schema for currency codes 
        /// </summary>
		public const string _currencySchema = @"http://www.xbrl.org/2003/iso4217";
        /// <summary>
        /// Namespace for pure unit type
        /// </summary>
		public const string _sharesPureNamespace = "xbrli";
        /// <summary>
        /// Schema for pure unit type
        /// </summary>
		public const string _sharesPureSchema = @"http://www.xbrl.org/2003/instance";
               

        /// <summary>
        /// Measure type code for this unit.
        /// </summary>
		public MeasureUnitTypeCode MeasureUnitType = MeasureUnitTypeCode.Unknown;
        /// <summary>
        /// Calculation type code for this unit -- only applicable when the unit is a calculated unit.
        /// </summary>
		public CalculationTypeCode CalculationType = CalculationTypeCode.NA;
		
        /// <summary>
        /// The first measure unit.
        /// </summary>
		public Measure Measure1;
        /// <summary>
        /// The 2nd measure unit (this is only applicabel if the measure is calclated).
        /// </summary>
		public Measure Measure2;

		#endregion

		#region constructors

        /// <summary>
        /// Creates a new MeasureUnit.
        /// </summary>
		public MeasureUnit()
		{
		}

        /// <summary>
        /// Creates a new Currency-Type MeasureUnit.
        /// </summary>
        /// <param name="name">Name of the unit</param>
        /// <param name="currencyCode">Currency Code</param>
		public void NewCurrencyMeasure(string name, string currencyCode)
		{
			this.Name = name;
			this.MeasureUnitType = MeasureUnitTypeCode.Currency; //currency
			this.CalculationType = CalculationTypeCode.NA;//not applicable
			Measure newMeasure = new Measure(currencyCode, _currencyNamespace, _currencySchema, Measure.MeasureTypeCode.Currency);
			this.Measure1 = newMeasure;
			this.Measure2 = null;
		}

        /// <summary>
        /// Creates a new Shares-Type MeasureUnit.
        /// </summary>
        /// <param name="name">Name of the unit</param>
		public void NewSharesMeasure(string name)
		{
			this.Name = name;
			this.MeasureUnitType = MeasureUnitTypeCode.Shares; //shares
			this.CalculationType = CalculationTypeCode.NA;//not applicable
			Measure newMeasure = new Measure(_shares, _sharesPureNamespace, _sharesPureSchema, Measure.MeasureTypeCode.Shares);
			this.Measure1 = newMeasure;
			this.Measure2 = null;
		}

        /// <summary>
        /// Creates a new Decimal-Type MeasureUnit.
        /// </summary>
        /// <param name="name">Name of the unit</param>
		public void NewPureMeasure(string name)
		{
			this.Name = name;
			this.MeasureUnitType = MeasureUnitTypeCode.Pure; //pure
			this.CalculationType = CalculationTypeCode.NA;//not applicable
			Measure newMeasure = new Measure(_pure, _sharesPureNamespace, _sharesPureSchema, Measure.MeasureTypeCode.Pure);
			this.Measure1 = newMeasure;
			this.Measure2 = null;
		}

        /// <summary>
        /// Creates a new Simple (Non-Calculated) MeasureUnit.
        /// </summary>
        /// <param name="name">Name of the unit</param>
        /// <param name="simpleMeasure">The simple measure</param>
		public void NewSimpleMeasure(string name, Measure simpleMeasure)
		{
			this.Name = name;
			this.MeasureUnitType = MeasureUnitTypeCode.Simple; //simple
			this.CalculationType = CalculationTypeCode.NA;//not applicable
			this.Measure1 = simpleMeasure;
			this.Measure2 = null;
		}

        /// <summary>
        /// Creates a new Calculated MeasureUnit.
        /// </summary>
        /// <param name="name">Name of the unit</param>
        /// <param name="calcType">The calculation type (divide or multiply)</param>
        /// <param name="measure1">The 1st unit used in the calculation</param>
        /// <param name="measure2">The 2nd unit used in the calculation</param>
		public void NewComplexMeasure(string name, CalculationTypeCode calcType, Measure measure1, Measure measure2)
		{
			this.Name = name;
			this.MeasureUnitType = MeasureUnitTypeCode.Complex; //complex
			this.CalculationType = calcType;
			this.Measure1 = measure1;
			this.Measure2 = measure2;
		}

		#endregion

		#region methods 

        /// <summary>
        /// This method override the base CopyTo() method for an Markup Item.
        /// </summary>
        /// <param name="receiver">Measure unit to be modify.</param>
		public override void CopyTo(MarkupItem receiver)
		{
			base.CopyTo (receiver);
			
			((MeasureUnit)receiver).MeasureUnitType = MeasureUnitType;
			((MeasureUnit)receiver).CalculationType = CalculationType;

			CopyMeasure( Measure1, ((MeasureUnit)receiver).Measure1 );
			CopyMeasure( Measure2, ((MeasureUnit)receiver).Measure2 );
		}
        
		internal void CopyMeasure( Measure measureBase, Measure newMeasure )
		{
			if ( measureBase == null )
			{
				newMeasure = null;
			}
			else if ( newMeasure == null )
			{
				newMeasure = new Measure( measureBase );
			}
			else	// neither is null
			{
				measureBase.CopyTo( newMeasure );
			}
		}

        /// <summary>
        /// This method override the ToString() and created a description for the measure unit object.   
        /// The format of the description depends on the type of the measure unit.
        /// </summary>
        /// <returns>Returns the display string for the measure unit object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			switch (this.MeasureUnitType)
			{
				case MeasureUnitTypeCode.Currency:
					sb.Append (this.Name).Append(":Currency Code[").Append(Measure1.ValueName).Append ("]");
					return sb.ToString();
				case MeasureUnitTypeCode.Shares:
					sb.Append (this.Name).Append(":").Append(Measure1.ValueName);
					return sb.ToString();
				case MeasureUnitTypeCode.Pure:
					sb.Append (this.Name).Append(":").Append(Measure1.ValueName);
					return sb.ToString();
				case MeasureUnitTypeCode.Simple:
					sb.Append (this.Name).Append(" --").Append(Environment.NewLine);
					sb.Append (this.Measure1.ToString());
					return sb.ToString();
				case MeasureUnitTypeCode.Complex:
					sb.Append (this.Name).Append(" --").Append(Environment.NewLine);
					sb.Append (this.Measure1.ToString()).Append(Environment.NewLine);
					sb.Append (this.CalculationType.ToString()).Append(Environment.NewLine).Append(Environment.NewLine);
					sb.Append (this.Measure2.ToString()).Append(Environment.NewLine);
					return sb.ToString();
				default:
					return base.ToString();
			}
		}

        /// <summary>
        /// This method override the Isvalid() method defined for the base -- Markup Item to validate the name.
        /// </summary>
        /// <param name="markupType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <param name="CheckFRTA">Flag indicates if the system should check to see if the name is FRTA compliant.</param>
        /// <returns>Returns true, if the measure unit is valid.</returns>
		public override bool IsValid(MarkupItem.MarkupTypeCode markupType, out ArrayList errorMessages, bool CheckFRTA)
		{
			bool isValid = base.IsValid( markupType, out errorMessages, CheckFRTA );

			if (!isValid)
				return false;

			// always validate measure 1
			isValid = Measure1.IsValid( markupType, errorMessages );

			if ( IsComplexType() )
			{
				if (CalculationType == CalculationTypeCode.NA )
				{
					errorMessages.Add( StringResourceUtility.GetString("Common.Error.ChooseCalculationType") );
					isValid = false;
				}

				// validate measure 2
				if ( !Measure2.IsValid( markupType, errorMessages ) )
                    isValid = false;
			}


			return isValid;
		}


        /// <summary>
        /// This method checks to see if all properties are not specified.
        /// </summary>
        /// <returns>Returns true, if the all properties are not specified.</returns>
		public override bool IsEmpty()
		{
			if (Measure1 != null && Measure2 == null)
			{
				//simple type - just check the Measure1 valueName
				if (Measure1.ValueName == string.Empty)
					return true;
			}
			else
			{
				//complex type - check both Measure1 and Measure2
				if (Measure1.ValueName == string.Empty && Measure2.ValueName == string.Empty)
					return true;
			}

			return false;
		}
		
        /// <summary>
        /// Indicates if this measure is a "Share" type unit.
        /// </summary>
        /// <returns></returns>
		public bool IsShareType()
		{
			return (this.MeasureUnitType == MeasureUnitTypeCode.Shares);
		}
        /// <summary>
        /// Indicates if this measure is a "Pure" (decimal) type unit.
        /// </summary>
        /// <returns></returns>
		public bool IsPureType()
		{
			return (this.MeasureUnitType == MeasureUnitTypeCode.Pure);
		}
        /// <summary>
        /// Indicates if this measure is a "Simple" type unit.
        /// </summary>
        /// <returns></returns>
		public bool IsSimpleType()
		{
			return (this.MeasureUnitType == MeasureUnitTypeCode.Simple);
		}

        /// <summary>
        /// Indicates if this measure is a "Complex" (calculated) type unit.
        /// </summary>
        /// <returns></returns>
		public bool IsComplexType()
		{
			return (this.MeasureUnitType == MeasureUnitTypeCode.Complex);
		}

        /// <summary>
        /// Indicates if this measure is a "Currency type unit.
        /// </summary>
        /// <returns></returns>
		public bool IsCurrencyType()
		{
			return (this.MeasureUnitType == MeasureUnitTypeCode.Currency);
		}

		/// <summary>
		/// Determines whether the value of a supplied <see cref="object"/> is equivalent to the value 
		/// of this instance of <see cref="MeasureUnit"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="MeasureUnit"/>
		/// that is to be compared to this instance of <see cref="MeasureUnit"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the object values are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="MeasureUnit"/> items values are considered equal if their measure unit types, 
		/// calculation types, measure 1, and measure 2 properties are equal and <see cref="ValueEquals"/> returns 
		/// true for the two <see cref="MeasureUnit"/> objects.</remarks>
		public override bool ValueEquals(object obj)
		{
			if (!(obj is MeasureUnit)) return false;

			MeasureUnit mu = obj as MeasureUnit;

			if(this.MeasureUnitType != mu.MeasureUnitType) return false;
			if(this.CalculationType != mu.CalculationType) return false;

			if(this.Measure1 != null)
			{
				if( !this.Measure1.ValueEquals(mu.Measure1) ) return false;
			}
			else
			{
				if(this.Measure1 != mu.Measure1) return false;
			}

			if(this.Measure2 != null)
			{
				if( !this.Measure2.ValueEquals(mu.Measure2) ) return false;
			}
			else
			{
				if(this.Measure2 != mu.Measure2) return false;
			}

			return base.ValueEquals(obj);
		}

		#endregion
	}

	#region Class Measure 

	/// <summary>
	/// Namespace and Schema are disabled as part of the Segment/Scenario/Unit Schema support
	/// </summary>
	[Serializable]
	public class Measure
	{
		#region enum

		/// <summary>
		/// Defines the types of measures.
		/// </summary>
		public enum MeasureTypeCode
		{
			/// <summary>
			/// Unknown measure type.
			/// </summary>
			Unknown,

			/// <summary>
			/// Currency measure type.
			/// </summary>
			Currency,

			/// <summary>
			/// Share measure type.
			/// </summary>
			Shares,

			/// <summary>
			/// Pure measure type.
			/// </summary>
			Pure,

			/// <summary>
			/// Other measure type.
			/// </summary>
			Other
		}
		#endregion

        /// <summary>
        /// Type of this measure.
        /// </summary>
        private string measureTypeDescription = string.Empty;

        /// <summary>
        ///  Description of this measure.
        /// </summary>
        private string measureDescription = string.Empty;

        /// <summary>
        /// The name of the measure unit value.
        /// </summary>
		public string ValueName = string.Empty;

        /// <summary>
        /// The namespace this measure unit de defined.
        /// </summary>
		public string Namespace = string.Empty;
        /// <summary>
        /// The Schema this measure unit de defined.
        /// </summary>
		public string Schema = string.Empty;

		/// <summary>
		/// The type of measure represented by this <see cref="Measure"/>.
		/// </summary>
		public MeasureTypeCode MeasureType = MeasureTypeCode.Unknown;

        /// <summary>
        /// Description of this <see cref="Measure"/>.
        /// </summary>
        /// <remarks>
        /// This could be the "description" - for example "Liters".
        /// </remarks>
        public string MeasureDescription
        {
            get
            {
                return this.measureDescription;
            }
            set
            {
                this.measureDescription = value;
            }
        }

        /// <summary>
        /// Type of this <see cref="Measure"/>.
        /// </summary>
        /// <remarks>
        /// This could be the type in the type - for example "volumeItemType".
        /// </remarks> 
        public string MeasureTypeDescription
        {
            get
            {
                return this.measureTypeDescription;
            }
            set
            {
                this.measureTypeDescription = value;
            }
        }

		/// <summary>
		/// Constructs a new instance of <see cref="Measure"/>
		/// </summary>
		public Measure()
		{
		}

		/// <summary>
		/// Overloaded. Constructs a new instance of <see cref="Measure"/>
		/// </summary>
		public Measure(Measure src)
		{
			this.ValueName		= src.ValueName;
			this.Namespace		= src.Namespace;
			this.Schema			= src.Schema;
			this.MeasureType	= src.MeasureType;
		}

		/// <summary>
		/// Overloaded. Constructs a new instance of <see cref="Measure"/>
		/// </summary>
		public Measure(string valueName)
		{
			this.ValueName = valueName;
		}

		/// <summary>
		/// Overloaded. Constructs a new instance of <see cref="Measure"/>
		/// </summary>
		public Measure(string valueName, string namespaceString, string schema, MeasureTypeCode measureType)
		{
			this.ValueName = valueName;
			this.Namespace = namespaceString;
			this.Schema = schema;
			this.MeasureType = measureType;
		}

		/// <summary>
		/// Copies values from this instance of <see cref="Measure"/> to a parameter-supplied
		/// <see cref="Measure"/>.
		/// </summary>
		/// <param name="receiver">The <see cref="Measure"/> to which values are to be copied.</param>
		public void CopyTo( Measure receiver )
		{
			receiver.ValueName = ValueName;
			receiver.Namespace = Namespace;
			receiver.Schema = Schema;
			receiver.MeasureType = MeasureType;
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents this instance of <see cref="Measure"/>.
		/// </summary>
		/// <returns>The string representation of this instance of <see cref="Measure"/>.  If 
		/// <see cref="ValueName"/> is defined, it is returned.  If not, <see cref="MeasureType"/>
		/// is returned.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.ValueName != null && this.ValueName.Length > 0)
				sb.Append("   Value:").Append (this.ValueName).Append(Environment.NewLine);
			sb.Append("   Type:").Append (this.MeasureType).Append(Environment.NewLine);

			return sb.ToString ();
		}

        /// <summary>
        /// This method override the Isvalid() method defined for the base -- Markup Item to validate the name.
        /// Specially, this method doesn't check for FRTA compliance as other markup items.
        /// </summary>
        /// <param name="markupType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <returns>Returns true, if the scenario is valid.</returns>

		public bool IsValid(MarkupItem.MarkupTypeCode markupType, ArrayList errorMessages)
		{
			string invalidString = @" ~`!@#$%^&*()+={}[]|\/;’'<>?,.";
			char[] invalidCharacters = invalidString.ToCharArray(0, invalidString.Length -1);

			bool isValid = true;
			if ( MeasureType == MeasureTypeCode.Unknown )
			{
				errorMessages.Add( StringResourceUtility.GetString("Common.Error.MeasureTypeMustBeSelected") );
				isValid = false;
			}

			if( (ValueName == null) || (ValueName.Length <= 0) )
			{
				errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueName",invalidString) );
				isValid = false;
			}
			else
			{
				if (ValueName.IndexOfAny(invalidCharacters, 0, ValueName.Length) >= 0)
				{
					errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueName",invalidString) );
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Indicates if this <see cref="Measure"/> is a currency measure.
		/// </summary>
		/// <returns>If true, the <see cref="MeasureType"/> for this <see cref="Measure"/> is 
		/// "currency".</returns>
		public bool IsCurrencyType()
		{
			return (this.MeasureType == MeasureTypeCode.Currency);
		}

		/// <summary>
		/// Indicates if this <see cref="Measure"/> is a shares measure.
		/// </summary>
		/// <returns>If true, the <see cref="MeasureType"/> for this <see cref="Measure"/> is 
		/// "shares".</returns>
		public bool IsShareType()
		{
			return (this.MeasureType == MeasureTypeCode.Shares);
		}

		/// <summary>
		/// Indicates if this <see cref="Measure"/> is a pure measure.
		/// </summary>
		/// <returns>If true, the <see cref="MeasureType"/> for this <see cref="Measure"/> is 
		/// "pure".</returns>
		public bool IsPureType()
		{
			return (this.MeasureType == MeasureTypeCode.Pure);
		}

		/// <summary>
		/// Indicates if this <see cref="Measure"/> is an other measure.
		/// </summary>
		/// <returns>If true, the <see cref="MeasureType"/> for this <see cref="Measure"/> is 
		/// "other".</returns>
		public bool IsOtherType()
		{
			return (this.MeasureType == MeasureTypeCode.Other);
		}

		/// <summary>
		/// Determines if a parameter-supplied <see cref="Object"/>, assumed to be a <see cref="Measure"/>,
		///  has a value equal to that of this instance of <see cref="Measure"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this instance of <see cref="Measure"/>.
		/// </param>
		/// <returns>True if <paramref name="obj"/> is equivalent to this instance of <see cref="Measure"/>.
		/// </returns>
		/// <remarks><paramref name="obj"/> has the same value as this instance of <see cref="Measure"/> if:
		/// <bl>
		/// <li><see cref="ValueName"/> is equal.</li>
		/// <li><see cref="MeasureType"/> is equal.</li>
		/// <li><see cref="Namespace"/> is equal.</li>
		/// <li><see cref="Schema"/> is equal.</li>
		/// </bl></remarks>
		public bool ValueEquals(object obj)
		{
			if (!(obj is Measure)) return false;

			Measure m = obj as Measure;

			if(this.ValueName != m.ValueName) return false;
			if(this.MeasureType != m.MeasureType) return false;
			if(this.Namespace != m.Namespace) return false;
			if(this.Schema != m.Schema) return false;

			return true;
		}
	}

	#endregion
}
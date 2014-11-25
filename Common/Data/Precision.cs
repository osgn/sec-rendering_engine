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
using System.Xml;
using System.Text;
using System.Collections;

using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// Precision is a type of Markup Item. 
    /// It's used for specifying Precision in XBRL documents.
	/// </summary>
	[Serializable]
	public class Precision : MarkupItem
	{
		#region Constants
        /// <summary>
        /// Precision not applicable"
        /// </summary>
		protected const string	_na			= "N/A";
        /// <summary>
        /// Digits as shown.
        /// </summary>
        protected const string INF          = "INF";

        /// <summary>
        /// Decimals.
        /// </summary>
		public const string		DECIMALS	= "decimals";        
        /// <summary>
        /// Precision
        /// </summary>
		public const string		PRECISION	= "precision";		
		#endregion
		
		#region enums

        /// <summary>
        /// Precision type code is used to indicates if the precision is expressed in the form of precision or decimals.
        /// </summary>
		public enum PrecisionTypeCode
		{
            /// <summary>
            /// Not specified.
            /// </summary>
			None,
            /// <summary>
            /// Indicates the precision is expressed in the form of precision.
            /// </summary>
			Precision,
            /// <summary>
            /// Indicates the precision is expressed in the form of decimals.
            /// </summary>
			Decimals
		}
		
		/// <summary>
		/// Represenst the range of NumberOfDigits values for Precision type precision objects
		/// The names also match up with the string resources keys for each precision allowing
		/// you to retrive the localized string from StringResourceUtility using the following 
		/// key structure "Client.Precision."
		/// </summary>
		public enum PrecisionNumDigits
		{
			/// <summary>
			/// Indicates the number is accurate to Tens position
			/// </summary>
			Ten = 1,
			/// <summary>
			/// Indicates the number is accurate to Hundreds position
			/// </summary>
			Hundred = 2,
			/// <summary>
			/// Indicates the number is accurate to Thousands position
			/// </summary>
			Thousand = 3,
			/// <summary>
			/// Indicates the number is accurate to TenThousands position
			/// </summary>
			TenThousand = 4,
			/// <summary>
			/// Indicates the number is accurate to HundredThousands position
			/// </summary>
			HundredThousand = 5,
			/// <summary>
			/// Indicates the number is accurate to Millions position
			/// </summary>
			Million = 6,
			/// <summary>
			/// Indicates the number is accurate to TenMillsion position
			/// </summary>
			TenMillion = 7,
			/// <summary>
			/// Indicates the number is accurate to HundredMillions position
			/// </summary>
			HundredMillion = 8,
			/// <summary>
			/// Indicates the number is accurate to Billions position
			/// </summary>
			Billion = 9,
			/// <summary>
			/// Indicates the number is accurate to TenBillions position
			/// </summary>
			TenBillion = 10,
			/// <summary>
			/// Indicates the number is accurate to HundredBillions position
			/// </summary>
			HundredBillion = 11,
			/// <summary>
			/// Indicates the number is accurate to Trillions position
			/// </summary>
			Trillion = 12
		}

		/// <summary>
		/// Represenst the range of NumberOfDigits values for Decimal type precision objects
		/// The names also match up with the string resources keys for each precision allowing
		/// you to retrive the localized string from StringResourceUtility using the following 
		/// key structure "Client.Precision."
		/// </summary>
		public enum DecimalNumDigits
		{
            /// <summary>
            /// Just the Whole number is accurate
            /// </summary>
            Oneth = 0,
			/// <summary>
			/// Indicates the number is accurate to 1 decimal place
			/// </summary>
			Tenth = 1,
			/// <summary>
			/// Indicates the number is accurate to 2 decimal places
			/// </summary>
			Hundredth = 2,
			/// <summary>
			/// Indicates the number is accurate to 3 decimal places
			/// </summary>
			Thousandth = 3,
			/// <summary>
			/// Indicates the number is accurate to 4 decimal places
			/// </summary>
			TenThousand = 4,
			/// <summary>
			/// Indicates the number is accurate to 5 decimal places
			/// </summary>
			HundredThousandth = 5,
			/// <summary>
			/// Indicates the number is accurate to 6 decimal places
			/// </summary>
			Millionth = 6,
			/// <summary>
			/// Indicates the number is accurate to 7 decimal places
			/// </summary>
			TenMillionth = 7,
			/// <summary>
			/// Indicates the number is accurate to 8 decimal places
			/// </summary>
			HundredMillionth = 8,
			/// <summary>
			/// Indicates the number is accurate to 9 decimal places
			/// </summary>
			Billionth = 9,
			/// <summary>
			/// Indicates the number is accurate to 10 decimal places
			/// </summary>
			TenBillionth = 10,
			/// <summary>
			/// Indicates the number is accurate to 11 decimal places
			/// </summary>
			HundredBillionth = 11,
			/// <summary>
			/// Indicates the number is accurate to 12 decimal places
			/// </summary>
			Trillionth = 12
		}

		#endregion

		#region properties

		/// <summary>		
		/// If precisionType = "Precision" and precision="9" then it measns we have 
		/// precision of nine digits. The first 9 digits, counting from the left,
		/// starting at the first non-zero digit in the lexical representation of the
		/// value of the numeric fact are known to be trustworthy for the
		/// purposes of computations to be performed using that numeric fact.														
		/// </summary>
		public PrecisionTypeCode PrecisionType = PrecisionTypeCode.None;

		/// <summary>
		/// FaceValue means that the number is 'as is' with no precision or decimal changes
		/// </summary>
		public bool FaceValue = false;

		/// <summary>
		/// NumberOfDigits tells the user for how many decimal places (if PrecisionType is Decimal)) or how precise (if the
		/// PrecisionType is Precision)
		/// </summary>
		public int NumberOfDigits = 0;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Precision.
		/// </summary>
		public Precision()
		{
		}

        /// <summary>
        /// Creates a new Precision by accepting a type code.
        /// </summary>
        /// <param name="precisionType">Precision type code.</param>
		public Precision(PrecisionTypeCode precisionType)
		{
			this.PrecisionType = precisionType;
			FaceValue = true;
		}

        /// <summary>
        /// Creates a new Precision by accepting a type code and number of digites.
        /// </summary>
        /// <param name="precisionType">Precision type code.</param>
        /// <param name="numDigits">Number of digits.</param>
		public Precision( PrecisionTypeCode precisionType, int numDigits )
		{
			PrecisionType = precisionType;
			NumberOfDigits = numDigits;
			FaceValue = false;
		}

        /// <summary>
        /// This method tries to create a Precision object from xml attribute.
        /// </summary>
        /// <param name="attr">XML attribute.</param>
        /// <param name="p">The precision object to be created.</param>
        /// <param name="errors">If the precison can't be created, returns the exceptions.</param>
        /// <returns>Returns true, if the precision object can be created.</returns>
		public static bool TryCreateFromXml( XmlAttribute attr, out Precision p, ref ArrayList errors )
		{
			p = new Precision();
			if ( errors == null )
			{
				errors = new ArrayList();
			}

			if ( attr.LocalName == DECIMALS )
			{
				p.PrecisionType = PrecisionTypeCode.Decimals;
			}
			else 
			{
				p.PrecisionType = PrecisionTypeCode.Precision;
			}

			if ( attr.Value.CompareTo( INF ) == 0 )
			{
				p.FaceValue = true;
			}
			else
			{
				try
				{
					p.NumberOfDigits = int.Parse( attr.Value );
				}
				catch ( FormatException )
				{
					return false;
				}
			}

			return true;
		}
		#endregion

        /// <summary>
        /// This method creates a XML attribute from the precision object.
        /// </summary>
        /// <param name="doc">XML document the attribute would be added to.</param>
        /// <returns></returns>
		public XmlAttribute CreateAttribute( XmlDocument doc )
		{
			// always print out the decimal attribute
			XmlAttribute attr = doc.CreateAttribute( /*PrecisionType == PrecisionTypeCode.Decimals ?*/ DECIMALS /*: PRECISION*/ );

			if ( FaceValue )
			{
				attr.Value = INF;
			}
			else if ( PrecisionType == PrecisionTypeCode.Decimals )
			{
				attr.Value =  NumberOfDigits.ToString();
			}
			else	// precision type == -Decimals so always use decimals and make number of digits negative 
			{
				attr.Value = ( NumberOfDigits * -1 ).ToString();
			}

			return attr;
		}
        
        /// <summary>
        /// This method overrides the base CopyTo() method for an Markup Item.
        /// </summary>
        /// <param name="receiver">Precision to be modify.</param>
		public override void CopyTo(MarkupItem receiver)
		{
			base.CopyTo (receiver);
			((Precision)receiver).PrecisionType = PrecisionType;
			((Precision)receiver).FaceValue		= FaceValue;
			((Precision)receiver).NumberOfDigits= NumberOfDigits;
		}

        /// <summary>
        /// This method overrides the ToString() and creates a description for the precision object.        
        /// </summary>
        /// <returns>Returns the display string for the precison object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append((Name != null && Name.Length > 0) ? Name: _na).Append(Environment.NewLine);
			//sb.Append ("Type:").Append(PrecisionType).Append(Environment.NewLine);
			sb.Append ("#Digits:").Append(NumberOfDigits).Append(Environment.NewLine);

			return sb.ToString();
		}

        /// <summary>
		/// This method overrides the IsValid() method defined for the base -- Markup Item to validate the name.
        /// </summary>
        /// <param name="segmentType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <param name="CheckFRTA">Flag indicates if the system should check to see if the name is FRTA compliant.</param>
        /// <returns>True if the name of this <see cref="Precision"/> is valid.</returns>
		public override bool IsValid(MarkupItem.MarkupTypeCode segmentType, out ArrayList errorMessages, bool CheckFRTA)
		{
			bool isValid = base.IsValid( segmentType, out errorMessages, CheckFRTA );

			return isValid;
		}

		/// <summary>
		/// Determines whether the value of a supplied <see cref="object"/> is equivalent to the value 
		/// of this instance of <see cref="Precision"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="Precision"/>
		/// that is to be compared to this instance of <see cref="Precision"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the object values are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="Precision"/> objects have the same value if their precision type, 
		/// face value, and number of digits properties are equal and <see cref="MarkupItem.ValueEquals"/> for the 
		/// two objects is true.</remarks>
		public override bool ValueEquals(object obj)
		{
			if ( obj == null ) return false;

			if (!(obj is Precision)) return false;

			Precision p = obj as Precision;

			if(this.PrecisionType != p.PrecisionType) return false;
			if(this.FaceValue != p.FaceValue) return false;
			if(this.NumberOfDigits != p.NumberOfDigits) return false;

			return base.ValueEquals(obj);
		}

		/// <summary>
		/// Builds the string resource key for the display name of the precision object.
		/// Posslbe return values:
		/// Client.Precision.Ten, Client.Precision.Hundred, 
		/// Client.Precision.Thousand, Client.Precision.TenThousand Client.Precision.HundredThousand
		/// Client.Precision.Million, Client.Precision.TenMillion, Client.Precision.HundredMillion
		/// Client.Precision.Billion, Client.Precision.TenBillion, Client.Precision.HundredBillion
		/// Client.Precision.Trillion
		/// Client.Precision.Tenth, Client.Precision.Hundredth
		/// Client.Precision.Thousandth, Client.Precision.TenThousand, Client.Precision.HundredThousandth
		/// Client.Precision.Millionth, Client.Precision.TenMillionth, Client.Precision.HundredMillionth
		/// Client.Precision.Billionth, Client.Precision.TenBillionth, Client.Precision.HundredBillionth
		/// Client.Precision.Trillionth
		/// Client.FaceValue
		/// </summary>
		/// <returns></returns>
		public string GetDisplayName()
		{
			string precisionKey;
			if (this.PrecisionType == Precision.PrecisionTypeCode.Decimals)
			{
				precisionKey = string.Format("Client.Precision.{0}", ((Precision.DecimalNumDigits)this.NumberOfDigits).ToString());
			}
			else if (this.PrecisionType == Precision.PrecisionTypeCode.Precision)
			{
                //bug 507 the mix entities were created incorrectly and have a presion type set to precesion when it should have 
                //ben set to type of none.  
                if (NumberOfDigits != 0)
                {
                    precisionKey = string.Format("Client.Precision.{0}", ((Precision.PrecisionNumDigits)this.NumberOfDigits).ToString());
                }
                else
                {
                    precisionKey = "Client.FaceValue";
                }
			}
			else
			{
				precisionKey = "Client.FaceValue";
			}
			return StringResourceUtility.GetString(precisionKey);
		}
	}
}
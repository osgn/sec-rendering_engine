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
using System.Collections;
using System.Text;
using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Utilities;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// Segment is a type of markup item. It's part of the context property.
	/// </summary>
	[Serializable]
	public class Segment: MarkupItem, IComparable		
	{
		#region properties

		private const string _na = "N/A";

        /// <summary>
        /// The name of the segment value.
        /// </summary>
		public string ValueName = string.Empty;
		/// <summary>
		/// The name of the segment type.
		/// </summary>
		public string ValueType = string.Empty;
        /// <summary>
        /// The namespace where the segment is defineds.
        /// </summary>
		public string Namespace = string.Empty;
        /// <summary>
        /// The schema where the segment is defineds.
        /// </summary>
		public string Schema = string.Empty;

		private ContextDimensionInfo dimensionInfo;
        /// <summary>
        /// Part of the dimension linkbase support.
        /// This property specified the dimension-related information for the segment.
        /// </summary>
		public ContextDimensionInfo DimensionInfo
		{
			get { return dimensionInfo; }
			set
			{ 
				dimensionInfo = value;
				if (dimensionInfo != null)
				{
					ValueName = dimensionInfo.Id;
					ValueType = dimensionInfo.dimensionId;
				}
			}

		}
		#endregion

		#region constructors

        /// <summary>
        /// Creates a new Segment.
        /// </summary>
		public Segment()
		{
		}
        /// <summary>
        /// Creates a new Segment.
        /// </summary>
        /// <param name="name">The friendly name of the segment.</param>
        /// <param name="valueName">The name of the segment value.</param>
        /// <param name="valueType">The name of the segment type.</param>
		public Segment(string name, string valueName, string valueType)
		{
			this.Name = name;
			this.ValueName = valueName;
			this.ValueType = valueType;
		}

        /// <summary>
        /// Creates a new Segment.
        /// </summary>
        /// <param name="name">The friendly name of the segment.</param>
        /// <param name="valueName">The name of the segment value.</param>
        /// <param name="valueType">The name of the segment type.</param>
        /// <param name="namespaceString">The namespace the segment is defined.</param>
        /// <param name="schema">The nameschema the segment is defined</param>
		public Segment(string name, string valueName, string valueType, 
			           string namespaceString, string schema)
		{
			this.Name = name;
			this.ValueName = valueName;
			this.ValueType = valueType;
			this.Namespace = namespaceString;
			this.Schema = schema;
		}

		#endregion

		#region methods

        /// <summary>
        /// This method override the ToString() and created a description for the segment object.        
        /// </summary>
        /// <returns>Returns the display string for the segment object.</returns>

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.DimensionInfo == null)
			{
				sb.Append(StringResourceUtility.GetString("Common.Scenario.Name")).Append((Name != null && Name.Length > 0) ? Name : _na).Append(Environment.NewLine);
				sb.Append(StringResourceUtility.GetString("Common.Scenario.ValueType")).Append((ValueType != null && ValueType.Length > 0) ? ValueType : _na).Append(Environment.NewLine);
				sb.Append(StringResourceUtility.GetString("Common.Scenario.ValueName")).Append((ValueName != null && ValueName.Length > 0) ? ValueName : _na).Append(Environment.NewLine);
			}
			else
			{
				sb.Append( string.Format( "Dimension {0}  Value {1}", DimensionInfo.dimensionId, DimensionInfo.Id ));
			}
			return sb.ToString();


		
		}

        /// <summary>
        /// This method override the base CopyTo() method for an Markup Item.
        /// </summary>
        /// <param name="receiver">Segment to be modify.</param>

		public override void CopyTo(MarkupItem receiver)
		{
			base.CopyTo(receiver);

			Segment copy = receiver as Segment;

			copy.ValueName = ValueName;
			copy.ValueType = ValueType;
			copy.Namespace = Namespace;
			copy.Schema = Schema;

			if (this.DimensionInfo != null)
			{
				copy.DimensionInfo = this.DimensionInfo.Clone() as ContextDimensionInfo;
			}
			else
			{
				copy.DimensionInfo = null;
			}



		}

        /// <summary>
        /// This method override the Isvalid() method defined for the base -- Markup Item to validate the name.
        /// We need to be more restricting that the xbrl spec with respect to
        /// segments and scenarios as we create ssu.xsd for all the segments and
        /// scenarios and create elements for both type and value
        /// hence both element and value needs to adhere to the xml element name limitations
        /// </summary>
        /// <param name="segmentType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <param name="CheckFRTA">Flag indicates if the system should check to see if the name is FRTA compliant.</param>
        /// <returns>Returns true, if the segment is valid.</returns>
        public override bool IsValid(MarkupItem.MarkupTypeCode segmentType, out ArrayList errorMessages, bool CheckFRTA)
		{
			string invalidStringType = @" ~`!@#$%^&*()+={}[]|\/;’'<>?,.";
			char[] invalidCharactersType = invalidStringType.ToCharArray(0, invalidStringType.Length -1);
			string invalidStringName = @" ~`!@#$%^&*()+={}[]|\/;’'<>?,.";
			char[] invalidCharactersName = invalidStringName.ToCharArray(0, invalidStringName.Length -1);

			bool isValid = base.IsValid( segmentType, out errorMessages, CheckFRTA);

			if( (ValueName == null) || (ValueName.Length <= 0) )
			{
				errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueName",invalidStringName) );
				isValid = false;
			}
			else
			{
				if (ValueName.IndexOfAny(invalidCharactersName, 0, ValueName.Length) >= 0)
				{
					errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueName",invalidStringName) );
					isValid = false;
				}
				
				//Bug 923 - check that it does not start with a number
				if( char.IsDigit(Convert.ToChar(ValueName.Substring(0,1))) )
				{
					errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.ValueNameStartsWithDigit") );
					isValid = false;
				}
			}

			if( (ValueType == null) || (ValueType.Length <= 0) )
			{
				errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueType",invalidStringType) );
				isValid = false;
			}
			else
			{
				if (ValueType.IndexOfAny(invalidCharactersType, 0, ValueType.Length) >= 0)
				{
					errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.InvalidValueType",invalidStringType) );
					isValid = false;
				}
				
				//Bug 923 - check that it does not start with a number
				if( char.IsDigit(Convert.ToChar(ValueType.Substring(0,1))) )
				{
					errorMessages.Add( TraceUtility.FormatStringResource("Common.Error.ValueTypeStartsWithDigit") );
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// This method checks to see if all properties are not specified.
		/// </summary>
		/// <returns>Returns true, if the all properties are not specified.</returns>
		public override bool IsEmpty()
		{
			return (this.ValueName == string.Empty && this.ValueType == string.Empty && this.dimensionInfo == null );
		}

        /// <summary>
        /// This method returns the dimension info in the segment.
        /// </summary>
        /// <returns>Returns dimension info if available. Otherwise, null would be returned.</returns>
		public override ContextDimensionInfo GetContextDimensionInfo()
		{
			return this.dimensionInfo;
		}

        /// <summary>
        /// This method sets the dimension info for the segment.
        /// </summary>
        /// <param name="cdi">The ContextDimensionInfo.</param>
		public override void SetContextDimensionInfo(ContextDimensionInfo cdi)
		{
			DimensionInfo = cdi;
		}
		#endregion

		/// <summary>
		/// Determines if supplied <see cref="Segment"/> value is equal to this <see cref="Segment"/>
		///  by calling the CompareValue overload that compares on all significant properties.
		/// </summary>
		public override bool ValueEquals(object obj)
		{
			if (!(obj is Segment)) return false;

			Segment s = obj as Segment;

			if ( CompareValue( s, true ) != 0 ) return false;

			return base.ValueEquals(obj);
		}

		/// <summary>
		/// Determines if supplied <see cref="Segment"/> value is equal to this <see cref="Segment"/>
		///  by calling the CompareValue overload that allows comparison on all properties but namespace and schema.
		/// </summary>
		public bool ValueEquals(object obj, Boolean includeNamespaceAndSchema)
		{
			if (!(obj is Segment)) return false;

			Segment s = obj as Segment;

			if (CompareValue(s, includeNamespaceAndSchema) != 0) return false;

			return base.ValueEquals(obj);
		}

		/// <summary>
		/// Determines if supplied <see cref="Segment"/> value equals this <see cref="Segment"/> by comparing on all significant properties.
		/// </summary>
		public int CompareValue(Segment s)
		{
			return this.CompareValue(s, true);
		}

		/// <summary>
		/// Determines if supplied <see cref="Segment"/> value equals this <see cref="Segment"/> by comparing on properties defined
		///  by supplied parameter.
		/// </summary>
		/// <param name="s"><see cref="Segment"/> to be compared with this <see cref="Segment"/>.</param>
		/// <param name="includeNamespaceAndSchema">A <see cref="Boolean"/> that, if true, indicates that
		/// method is to include namespace and schema in comparison.</param>
		/// <returns>An <see cref="int"/>: Zero if supplied <see cref="Segment"/> is equal in value
		/// to this <see cref="Segment"/>.</returns>
		public int CompareValue(Segment s, Boolean includeNamespaceAndSchema)
		{
			int ret = 0;
			if (this.DimensionInfo != null)
			{
				if (s.DimensionInfo == null) return 1;

				return this.DimensionInfo.CompareValue(s.DimensionInfo);
			}
			else if (s.DimensionInfo != null) return -1;
			
			ret = this.ValueName.CompareTo( s.ValueName);
			if( ret != 0 )
				return ret;
			ret = this.ValueType.CompareTo( s.ValueType);
			if( ret != 0 )
				return ret;

			if (includeNamespaceAndSchema == true)
			{
				ret = this.Namespace.CompareTo(s.Namespace);
				if (ret != 0)
					return ret;

				ret = this.Schema.CompareTo(s.Schema);
			}

			return ret;
		}

		#region IComparable Members

		//used only to sort not to make sure things are the same
		int IComparable.CompareTo(object obj)
		{
			Segment other = obj as Segment;

			if (this.dimensionInfo != null && other.dimensionInfo != null)
			{
				int ret = this.dimensionInfo.dimensionId.CompareTo(other.dimensionInfo.dimensionId);
				if (ret != 0) return ret;

				return this.dimensionInfo.Id.CompareTo(other.dimensionInfo.Id);
			}
			else
			{
				int ret = this.ValueType.CompareTo(other.ValueType);
				if (ret != 0) return ret;

				return this.ValueName.CompareTo(other.ValueName);
			}
		}

		#endregion
	}
}
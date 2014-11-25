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
using System.Diagnostics;
using Aucent.MAX.AXE.Common.Utilities;

using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
    /// Scenario is a type of markup item. It's part of the context property.
	/// </summary>
	[Serializable]
	public class Scenario: MarkupItem, IComparable
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
        /// The namespace where the scenario is defineds.
        /// </summary>
        public string Namespace = string.Empty;
        /// <summary>
        /// The schema where the scenario is defineds.
        /// </summary>
        public string Schema = string.Empty;

		private ContextDimensionInfo dimensionInfo;
        /// <summary>
        /// Part of the dimension linkbase support.
        /// This property specified the dimension-related information for the scenario.
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
        /// Creates a new Scenario.
        /// </summary>
		public Scenario()
		{
		}

        /// <summary>
        /// Creates a new Scenario.
        /// </summary>
        /// <param name="name">The friendly name of the scenario.</param>
        /// <param name="valueName">The name of the scenario value.</param>
        /// <param name="valueType">The name of the scenario type.</param>

		public Scenario(string name, string valueName, string valueType)
		{
			this.Name = name;
			this.ValueName = valueName;
			this.ValueType = valueType;
		}

        /// <summary>
        /// Creates a new Scenario.
        /// </summary>
        /// <param name="name">The friendly name of the scenario.</param>
        /// <param name="valueName">The name of the scenario value.</param>
        /// <param name="valueType">The name of the scenario type.</param>
        /// <param name="namespaceString">The namespace the scenario is defined.</param>
        /// <param name="schema">The nameschema the scenario is defined</param>

		public Scenario(string name, string valueName, string valueType, 
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
        /// This method override the ToString() and created a description for the scenario object.        
        /// </summary>
        /// <returns>Returns the display string for the scenario object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if( this.DimensionInfo == null )
			{
				sb.Append (StringResourceUtility.GetString("Common.Scenario.Name")).Append((Name != null && Name.Length > 0) ? Name: _na).Append(Environment.NewLine);
				sb.Append (StringResourceUtility.GetString("Common.Scenario.ValueType")).Append((ValueType != null && ValueType.Length > 0) ? ValueType: _na).Append(Environment.NewLine);
				sb.Append (StringResourceUtility.GetString("Common.Scenario.ValueName")).Append((ValueName != null && ValueName.Length > 0) ? ValueName: _na).Append(Environment.NewLine);
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
        /// <param name="receiver">Scenario to be modify.</param>

		public override void CopyTo(MarkupItem receiver)
		{
			base.CopyTo (receiver);
			Scenario copy = receiver as Scenario;

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
        /// <param name="scenarioType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <param name="CheckFRTA">Flag indicates if the system should check to see if the name is FRTA compliant.</param>
        /// <returns>Returns true, if the scenario is valid.</returns>
		public override bool IsValid(MarkupItem.MarkupTypeCode scenarioType, out ArrayList errorMessages, bool CheckFRTA)
		{
			//we need to be more restricting that the xbrl spec with respect to
			//segments and scenarios as we create ssu.xsd for all the segments and
			//scenarios and create elements for both type and value
			//hence both element and value needs to adhere to the xml element name limitations

			//type does not allow forward slash also
			//3731
			string invalidStringType = @" ~`!@#$%^&*()+={}[]|\/;’'<>?,.";
			char[] invalidCharactersType = invalidStringType.ToCharArray(0, invalidStringType.Length -1);
			string invalidStringName = @" ~`!@#$%^&*()+={}[]|\/;’'<>?,.";
			char[] invalidCharactersName = invalidStringName.ToCharArray(0, invalidStringName.Length -1);

			bool isValid = base.IsValid( scenarioType, out errorMessages, CheckFRTA );

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
			return (this.ValueName == string.Empty && this.ValueType == string.Empty && this.DimensionInfo == null );
		}

        /// <summary>
        /// This method returns the dimension info in the scenario.
        /// </summary>
        /// <returns>Returns dimension info if available. Otherwise, null would be returned.</returns>

		public override ContextDimensionInfo GetContextDimensionInfo()
		{
			return this.dimensionInfo;
		}

        /// <summary>
        /// This method sets the dimension info for the scenario.
        /// </summary>
        /// <param name="cdi">The ContextDimensionInfo.</param>

		public override void SetContextDimensionInfo(ContextDimensionInfo cdi)
		{
			DimensionInfo = cdi;
		}

		#endregion

		/// <summary>
		/// Determines if supplied <see cref="Scenario"/> value is equal to this <see cref="Scenario"/>
		///  by calling the CompareValue overload that compares on all significant properties.
		/// </summary>
		public override bool ValueEquals(object obj)
		{
			if (!(obj is Scenario)) return false;

			Scenario s = obj as Scenario;

			if (CompareValue(s, true) != 0) return false;

			return base.ValueEquals(obj);
		}

		/// <summary>
		/// Determines if supplied <see cref="Scenario"/> value is equal to this <see cref="Scenario"/>
		///  by calling the CompareValue overload that allows comparison on all properties but namespace and schema.
		/// </summary>
		public bool ValueEquals(object obj, Boolean includeNamespaceAndSchema)
		{
			if (!(obj is Scenario)) return false;

			Scenario s = obj as Scenario;

			if (CompareValue(s, includeNamespaceAndSchema) != 0) return false;

			return base.ValueEquals(obj);
		}

		/// <summary>
		/// Determines if supplied <see cref="Scenario"/> value equals this <see cref="Scenario"/> by comparing on all significant properties.
		/// </summary>
		public int CompareValue(Scenario s)
		{
			return CompareValue(s, true);
		}

		/// <summary>
		/// Determines if supplied <see cref="Scenario"/> value equals this <see cref="Scenario"/> by comparing on properties defined
		///  by supplied parameter.
		/// </summary>
		/// <param name="s"><see cref="Scenario"/> to be compared with this <see cref="Scenario"/>.</param>
		/// <param name="includeNamespaceAndSchema">A <see cref="Boolean"/> that, if true, indicates that
		/// method is to include namespace and schema in comparison.</param>
		/// <returns>An <see cref="int"/>: Zero if supplied <see cref="Scenario"/> is equal in value
		/// to this <see cref="Scenario"/>.</returns>
		public int CompareValue(Scenario s, Boolean includeNamespaceAndSchema)
		{
			//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Comparing s=" + s.ValueName + " to this=" + this.ValueName);
			int ret = 0;
			if (this.DimensionInfo != null)
			{
				//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": this.DimensionInfo not null.  Comparing dimensions.");
				if (s.DimensionInfo == null)
				{
					//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal because s has no dimension.");
					return 1;
				}

				return this.DimensionInfo.CompareValue(s.DimensionInfo);
				
			}
			else
			{
				//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": this.DimensionInfo is null. Looking for dimensions in s.");
				if (s.DimensionInfo != null)
				{
					//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal because s has dimension.");
					return -1;
				}
			}

			ret = this.ValueName.CompareTo( s.ValueName);
			if (ret != 0)
			{
				//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal due to value names");
				return ret;
			}

			ret = this.ValueType.CompareTo( s.ValueType);
			if (ret != 0)
			{
				//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal due to value types. "
				//    + "this.ValueType=" + this.ValueType + ", s.ValueType=" + s.ValueType );
				return ret;
			}

			if (includeNamespaceAndSchema == true)
			{
				ret = this.Namespace.CompareTo(s.Namespace);
				if (ret != 0)
				{
					//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal due to namespaces. "
					//    + "this.Namespace=" + this.Namespace + ", s.Namespace=" + s.Namespace);
					return ret;
				}

				ret = this.Schema.CompareTo(s.Schema);
				if (ret != 0)
				{
					//Debug.WriteLine(System.Reflection.MethodInfo.GetCurrentMethod().Name + ": Scenarios NOT equal due to schemas. "
					//    + "this.Schema=" + this.Schema + ", s.Schema=" + s.Schema);
					return ret;
				}
			}

			return ret;
		}

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			Scenario other = obj as Scenario;

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
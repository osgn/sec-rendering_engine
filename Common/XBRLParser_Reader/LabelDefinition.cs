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
using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// LabelReference
	/// </summary>
	[Serializable]
	public class LabelDefinition : IComparable	, IComparable<LabelDefinition>
	{
        public static readonly string DEPRECATED_LABEL_ROLE = "deprecatedLabel";
        public static readonly string DEPRECATEDDATE_LABEL_ROLE = "deprecatedDateLabel";

		/// <summary>
		/// The role of this <see cref="LabelDefinition"/>.
		/// </summary>
		public string LabelRole = string.Empty;

		/// <summary>
		/// The language code of this <see cref="LabelDefinition"/>.
		/// </summary>
		public string Language = string.Empty;

		/// <summary>
		/// The text of this <see cref="LabelDefinition"/>.
		/// </summary>
		public string Label = string.Empty;

		/// <summary>
		/// Constructs a new instance of <see cref="LabelDefinition"/>.
		/// </summary>
		public LabelDefinition()
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="LabelDefinition"/>.
		/// </summary>
		/// <param name="label">The text of the label.</param>
		/// <param name="labelRole">The role to be assigned to the newly created 
		/// <see cref="LabelDefinition"/>.</param>
		/// <param name="language">The language code to be assigned to the newly 
		/// created <see cref="LabelDefinition"/>.</param>
		public LabelDefinition(string labelRole, string language, string label)
		{
			this.LabelRole = labelRole;
			this.Language = language;
			this.Label = label;
		}

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="LabelDefinition"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="objIn">An <see cref="object"/> to which this instance of <see cref="LabelDefinition"/>
		/// is to be compared.  Assumed to be a <see cref="LabelDefinition"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="LabelDefinition"/>.</returns>
		/// <remarks>Method first compares the label roles of the two <see cref="LabelDefinition"/> objects. If
		/// they are equal, method returns the results of <see cref="String.Compare(String, String)"/> for the
		/// languages.  If the label rows are not equal, method returns the results of 
		/// <see cref="String.Compare(String, String)"/> for the label roles.</remarks>
		public int CompareTo(object objIn)
		{
			LabelDefinition labelDefIn = objIn as LabelDefinition;

			if (string.Compare(labelDefIn.LabelRole, this.LabelRole) != 0)
			{
				return string.Compare(labelDefIn.LabelRole, this.LabelRole);
			}
			else
			{
				return string.Compare(labelDefIn.Language, this.Language);
			}
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="LabelDefinition"/>.
		/// </summary>
		/// <param name="objIn">The <see cref="Object"/> to be compared to this <see cref="LabelDefinition"/>.  
		/// Assumed to be a <see cref="LabelDefinition"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="LabelDefinition"/>.</returns>
		/// <remarks>To be equal, role and language in the two <see cref="LabelDefinition"/> must be equal.</remarks>
		public override bool Equals(object objIn)
		{
			//consider two objects equal if the role and language match			
			LabelDefinition labelDefIn = objIn as LabelDefinition;

			if (labelDefIn != null)
			{
				if (string.Compare(labelDefIn.LabelRole, this.LabelRole) != 0 ||
					string.Compare(labelDefIn.Language,this.Language) != 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="LabelDefinition"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="LabelDefinition"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		#endregion


		#region IComparable<LabelDefinition> Members

		/// <summary>
		/// Compares this instance of <see cref="LabelDefinition"/> to a supplied <see cref="LabelDefinition"/>.
		/// </summary>
		/// <param name="other">A <see cref="LabelDefinition"/> to which this instance of <see cref="LabelDefinition"/>
		/// is to be compared.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="other"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="LabelDefinition"/>.</returns>
		/// <remarks>The results returned are equivalent to <see cref="String.CompareTo(String)"/> for the language 
		/// and label role properties of the two objects.</remarks>
		public int CompareTo(LabelDefinition other)
		{
            int ret = string.Compare(this.Language, other.Language, true);
			if (ret != 0) return ret;

			return this.LabelRole.CompareTo(other.LabelRole);
		}

		#endregion
	}
}
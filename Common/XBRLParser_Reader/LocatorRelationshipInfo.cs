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

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// LocatorRelationshipInfo
	/// </summary>
	[Serializable]
	public class LocatorRelationshipInfo : IComparable
	{
		#region properties
		
		string label;
        /// <summary>
        /// Label
        /// </summary>
        public string Label
		{
			get { return label; }
			set { label = value; }
		}
		
		int priority;
        /// <summary>
        /// Priority
        /// </summary>
        public int Priority
		{
			get { return priority; }
			set { priority = value; }
		}

		float order;
		/// <summary>
		/// unique value given to an href when order is not provided
		/// </summary>
        public float Order
		{
			get { return order; }
			set { order = value; }
		}

		float origOrder;
		/// <summary>
		/// order as provided by the linkbase document
		/// </summary>
		public float OrigOrder
		{
			get { return origOrder; }
			set { origOrder = value; }
		}

		bool isProhibited = false;
        /// <summary>
        /// Is prohibited
        /// </summary>
        public bool IsProhibited
		{
			get { return isProhibited; }
			set { isProhibited = value; }
		}

		string weight ="1";
        /// <summary>
        /// weight
        /// </summary>
        public string CalculationWeight
		{
			get { return weight; }
			set { weight = value; }
		}
		
		string prefLabel;
        /// <summary>
        /// Perf label
        /// </summary>
        public string PrefLabel
		{
			get { return prefLabel; }
			set { prefLabel = value; }
		}
		
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		public LocatorRelationshipInfo()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="LocatorRelationshipInfo"/> 
		/// and copies properties from a parameter-supplied <see cref="LocatorRelationshipInfo"/> 
		/// to the newly created <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		/// <param name="lri">The <see cref="LocatorRelationshipInfo"/> from which properties are 
		/// to be copied to the newly created <see cref="LocatorRelationshipInfo"/>.</param>
		public LocatorRelationshipInfo(LocatorRelationshipInfo lri)
		{
			this.isProhibited = lri.isProhibited;
			this.label = lri.label;
			this.order = lri.order;
			this.priority = lri.priority;
			this.weight = lri.weight;
			this.origOrder = lri.origOrder;
			this.prefLabel = lri.prefLabel;
		}

		#endregion

		#region public 

		
		public static LocatorRelationshipInfo CreateObj(  string label,  string priority,
			float origOrder, float orderArg, string prefLabel, string weight, bool isProhibited)
		{
			LocatorRelationshipInfo obj = new LocatorRelationshipInfo();
			obj.label = label;
			obj.isProhibited = isProhibited;
			obj.prefLabel = prefLabel;
			obj.order = orderArg;
			obj.origOrder = origOrder;
			obj.priority = Convert.ToInt32( priority );
			obj.weight = weight;

			if ( prefLabel != null )
			{
				int lastSlash = prefLabel.LastIndexOf( "/" )+1;
				obj.prefLabel = prefLabel.Substring( lastSlash, prefLabel.Length - lastSlash );
			}


			return obj;
		}

		/// <summary>
		/// Determines whether a supplied <see cref="LocatorRelationshipInfo"/> is equal to this <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		/// <param name="lri">The <see cref="LocatorRelationshipInfo"/> to be compared to this <see cref="LocatorRelationshipInfo"/>.</param>
		/// <returns>True if <paramref name="lri"/> is equal to this <see cref="LocatorRelationshipInfo"/>.</returns>
		/// <remarks>To be equal, the label, preferred label, order, original order, and 
		/// calculation weight properties in the two <see cref="LocatorRelationshipInfo"/> must be equal.</remarks>
		public bool BaseEquals(LocatorRelationshipInfo lri)
		{
			if ( this.label != null )
			{
				if ( this.label.CompareTo( lri.Label ) != 0 ) return false;
			}
			else if ( lri.Label != null ) 
			{
				return false;
			}

			if ( prefLabel != null )
			{
				if ( prefLabel.CompareTo( lri.PrefLabel ) != 0 ) return false;
			}
			else if ( lri.prefLabel != null )
			{
				return false;
			}

			if ( lri.Order != Order ) return false;
			if ( lri.origOrder != origOrder ) return false;
		
			if ( lri.CalculationWeight.CompareTo( CalculationWeight ) != 0 ) return false;

			return true;
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="LocatorRelationshipInfo"/>.  
		/// Assumed to be a <see cref="LocatorRelationshipInfo"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="LocatorRelationshipInfo"/>.</returns>
		/// <remarks>To be equal, <see cref="BaseEquals"/> must be true and the "IsProhibited" and 
		/// "Priority" properties in the two <see cref="LocatorRelationshipInfo"/> must be equal.</remarks>
		public override bool Equals(object obj)
		{
			LocatorRelationshipInfo lri = obj as LocatorRelationshipInfo;
			if ( lri == null ) return false;

			if ( !BaseEquals( lri ) ) return false;

			if ( lri.IsProhibited != IsProhibited ) return false;
			if ( lri.Priority != Priority ) return false;

			return true;
		}

        virtual internal  bool IsNonXlinkEquivalentRelationship(LocatorRelationshipInfo other)
        {
            if (!this.origOrder.Equals(other.origOrder)) return false;
            if (!this.weight.Equals(other.weight)) return false;

            string thisLabel = string.IsNullOrEmpty(this.prefLabel) ? string.Empty : this.prefLabel;
            string otherLabel = string.IsNullOrEmpty(other.prefLabel) ? string.Empty : other.prefLabel;

            if (!thisLabel.Equals(otherLabel)) return false;

            return true;
        }

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="LocatorRelationshipInfo"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="LocatorRelationshipInfo"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="LocatorRelationshipInfo"/>
		/// is to be compared.  Assumed to be a <see cref="LocatorRelationshipInfo"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="LocatorRelationshipInfo"/>.</returns>
		/// <remarks>The following <see cref="LocatorRelationshipInfo"/> properties are compared in order:
		/// <bl>
		/// <li>Original order.</li>
		/// <li>Priority.</li>
		/// <li>Prohibited.</li>
		/// </bl>
		/// </remarks>
		public int CompareTo(object obj)
		{
			LocatorRelationshipInfo other = (LocatorRelationshipInfo)obj;
          
            string thisLabel = string.IsNullOrEmpty(this.prefLabel) ? string.Empty : this.prefLabel;
            string otherLabel = string.IsNullOrEmpty(other.prefLabel) ? string.Empty : other.prefLabel;
            int ret = thisLabel.CompareTo(otherLabel);
            if (ret != 0)
            {
                return ret;
            }
           

            ret = this.weight.CompareTo(other.weight);
            if (ret != 0)
            {
                return ret;
            }
            
			ret = this.origOrder.CompareTo( other.origOrder );
			if ( ret != 0 )
			{
				return ret;
			}

			ret = this.Priority.CompareTo( other.priority );
			if( ret != 0 )
				return ret;

		
            //false before true...
			return this.isProhibited.CompareTo(other.isProhibited);
			
		}

		#endregion

	}
}
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

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// Scale Factor
	/// </summary>
	/// <remarks>
	/// This is not part of XBRL spec. 
    /// It's created to enable the user to mark up numbers that are rounded (match their published financials).
    /// If the scale factor is 3, the number would be multiplied by 1,000 (the number is rounded to thousands for display).
    /// If the scale factor is 6, the number would be multiplied by 1,000,000 (the number is rounded to millions for display).
	/// </remarks>
	[Serializable]
	public class Scale : MarkupItem
	{
        
		#region properties

		private const string _na = "N/A";

        /// <summary>
        /// The scale fator to be applied.
        /// </summary>
		protected int factor = 0;
        /// <summary>
        /// The scale fator to be applied.
        /// If the scale factor is 3, the number would be multiplied by 1,000 (the number is rounded to thousands for display).
        /// If the scale factor is 6, the number would be multiplied by 1,000,000 (the number is rounded to millions for display).
        /// </summary>
		public int Factor
		{
			get {return factor;}
			set {factor = value;}
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Scale.
		/// </summary>
		public Scale()
		{
		}
        /// <summary>
        /// Creates a new Scale by accepting the scal factor.
        /// </summary>
		public Scale( int factor )
		{
			this.factor = factor;
		}

		#endregion


        /// <summary>
        /// This method override the base CopyTo() method for an Markup Item.
        /// </summary>
        /// <param name="receiver">Scale to be modify.</param>
		public override void CopyTo(MarkupItem receiver)
		{
			base.CopyTo (receiver);
			
			((Scale)receiver).factor = factor;
		}


        /// <summary>
        /// This method override the ToString() and created a description for the scale object.        
        /// </summary>
        /// <returns>Returns the display string for the scale object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append((Name != null && Name.Length > 0) ? Name: _na).Append(Environment.NewLine);
			//sb.Append ("Scale Factor:").Append(factor).Append(Environment.NewLine);

			return sb.ToString();
		}
        /// <summary>
        /// This method override the Isvalid() method defined for the base -- Markup Item to validate the name.
        /// </summary>
        /// <param name="segmentType">Markup type code.</param>
        /// <param name="errorMessages">Exception messages.</param>
        /// <param name="CheckFRTA">Flag indicates if the system should check to see if the name is FRTA compliant.</param>
        /// <returns>Returns true, if the scale is valid.</returns>

		public override bool IsValid(MarkupItem.MarkupTypeCode segmentType, out ArrayList errorMessages, bool CheckFRTA)
		{
			bool isValid = base.IsValid( segmentType, out errorMessages, CheckFRTA );

			return isValid;
		}

		/// <summary>
		/// Determines whether the value of a supplied <see cref="object"/> is equivalent to the value 
		/// of this instance of <see cref="Scale"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="Scale"/>
		/// that is to be compared to this instance of <see cref="Scale"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the object values are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="Scale"/> objects have the same value if their factor property values are equal 
		/// and <see cref="MarkupItem.ValueEquals"/> for the two objects is true.</remarks>
		public override bool ValueEquals(object obj)
		{
			if (!(obj is Scale)) return false;

			Scale s = obj as Scale;

			if(this.Factor != s.Factor) return false;

			return base.ValueEquals(obj);
		}
	}
}
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
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Utilities;

namespace Aucent.MAX.AXE.Common.Data
{
    /// <summary>
    /// ContextDimensionInfo
    /// </summary>
    [Serializable]
    public class ContextDimensionInfo : ICloneable
    {
        /// <summary>
        /// The type of dimension.
        /// </summary>
        public enum DimensionType
        {
            /// <summary>
            /// Indicates the dimension is explicit (non-typed).
            /// </summary>
            explicitMember,
            /// <summary>
            /// Indicates the dimension is typed.
            /// </summary>
            typedMember
        }
        /// <summary>
        /// ID
        /// </summary>
        public string Id;
        /// <summary>
        /// Dimension ID
        /// </summary>
        public string dimensionId;
        /// <summary>
        /// Dimension type -- default to Explicit (non-typed).
        /// </summary>
        public DimensionType type = DimensionType.explicitMember;

		private object dimensionNode;
        /// <summary>
        /// DimensionNode is from the segment tag in the tree node we need access to the dimensionNode data.
        /// </summary>
		[XmlIgnore]
		public object DimensionNode 
		{
			get { return dimensionNode;  }
			set { dimensionNode = value; }
		}

		/// <summary>
        /// Creates a new ContextDimensionInfo.
		/// </summary>
		public ContextDimensionInfo()
		{

		}

        /// <summary>
        /// Creates a new ContextDimensionInfo.
        /// </summary>
        /// <param name="copy">The ContextDimensionInfo object to be copied.</param>
		public ContextDimensionInfo(ContextDimensionInfo copy)
		{
			this.Id = copy.Id;
			this.dimensionId = copy.dimensionId;
			this.type = copy.type;
			this.dimensionNode = copy.dimensionNode;//this is a shallow copy 
		}

        /// <summary>
        /// This method compares the values of two ContextDimensionInfo objects.
        /// Comparison order:
        /// (1) ID
        /// (2) DimensionID
        /// (3) Type
        /// </summary>
        /// <param name="s">The ContextDimensionInfo object.</param>
        /// <returns>Returns the comparison results.</returns>
		public int CompareValue(ContextDimensionInfo s)
		{
			int ret = Id.CompareTo( s.Id);
			if (ret != 0)
				return ret;

			ret = dimensionId.CompareTo(s.dimensionId);
			if (ret != 0)
				return ret;

			return type.CompareTo(s.type);


		}

       

		#region ICloneable Members

        /// <summary>
        /// Creates a copy of the ContextDimensionInfo objects.
        /// </summary>
        /// <returns></returns>
		public object Clone()
		{
			return new ContextDimensionInfo(this);
		}

		#endregion

        /// <summary>
        /// Desc of the context dimension info.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", this.dimensionId, this.Id);
        }
	}
}

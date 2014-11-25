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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

using Aucent.MAX.AXE.Common.Data;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Encapsulates the properties and methods associated with an XBRL dimension node.
	/// </summary>
    public class DimensionNode : Node
    {
		/// <summary>
		/// Defines the types of <see cref="DimensionNode"/>s.
		/// </summary>
        public enum NodeType
        {
			/// <summary>
			/// A hypercube dimension node.
			/// </summary>
            Hypercube,

			/// <summary>
			/// A dimension dimension node.
			/// </summary>
			Dimension,

			/// <summary>
			/// A item--member or measure--dimension node.
			/// </summary>
			Item 
        }


        #region Properties

		private DefinitionLink definitionLink = null;

		/// <summary>
		/// The XBRL definition linkbase link underlying this <see cref="DimensionNode"/>.
		/// </summary>
		public DefinitionLink MyDefinitionLink
		{
			get { return definitionLink; }
			set { definitionLink = value; }

		}

		private DefinitionLocatorRelationshipInfo nodeInfo = null;

		/// <summary>
		/// The definition linkbase locator information for this <see cref="DimensionNode"/>.
		/// </summary>
		public DefinitionLocatorRelationshipInfo NodeDimensionInfo
		{
			get { return nodeInfo; }
			set { nodeInfo = value; }
		}


		//determine if the dimension node that is being built is a segment markup..
		//this is useful in determining if a dimension member needs to be treated as a segment of scenario..
		//default is segment as that seems to be the more common option...
		private bool isSegmentMarkup = true;
		public bool IsSegmentMarkup
		{
			get { return isSegmentMarkup; }
			set { isSegmentMarkup = value; }
		}

        #endregion

        #region Ctors
		/// <summary>
		/// Constructs a new instance of <see cref="DimensionNode"/>.
		/// </summary>
        public DimensionNode():base()
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="DimensionNode"/>.
		/// </summary>
		public DimensionNode(string titleArg)
			: base(titleArg)
		{
		
		}

		/// <summary>
		/// Constructs a new instance of <see cref="DimensionNode"/>.
		/// </summary>
		public DimensionNode(Element elem)
			: base(elem)
		{
			
		}

	
        #endregion

        #region Public methods

		/// <summary>
		/// Constructs a <see cref="ContextDimensionInfo"/> object from the 
		/// parent <see cref="DimensionNode"/> of this <see cref="DimensionNode"/>.
		/// </summary>
		/// <returns>The newly constructed <see cref="ContextDimensionInfo"/>.</returns>
		public ContextDimensionInfo BuildContextInfo()
		{
			DimensionNode parentNode = GetParentDimensionTypeDimensionNode();
			if (parentNode == null  || parentNode.Id == this.Id ) return null;

			ContextDimensionInfo cdi = new ContextDimensionInfo();
			cdi.Id = this.Id;

			cdi.dimensionId = parentNode.Id;
			cdi.type = ContextDimensionInfo.DimensionType.explicitMember;
			return cdi;
		}

		/// <summary>
		/// Get the Parent node that is of type XBRLI:Dimension.
		/// </summary>
		/// <returns></returns>
		public DimensionNode GetParentDimensionTypeDimensionNode()
		{
			if (this.nodeInfo == null) return null;
			if (this.nodeInfo.NodeType == NodeType.Dimension)
			{
				return this;
			}
			if (this.parent == null || !( parent is DimensionNode ) ) return null;

			return (this.parent as DimensionNode).GetParentDimensionTypeDimensionNode();

			
		}

		

		/// <summary>
		/// Determins if the node or any of the node's parents have the TargetRoleProperty set
		/// </summary>
		/// <returns></returns>
		public bool HasTargetRole()
		{
			if (this.NodeDimensionInfo == null ) return false;
            if( this.NodeDimensionInfo.TargetRole != null) return true;
			if (this.Parent == null) return false;

			DimensionNode parentDn = this.Parent as DimensionNode;
			if (parentDn == null ) return false;

			return parentDn.HasTargetRole();
		}

        public DimensionNode GetDefaultChildOfAxis()
        {
            if (this.nodeInfo.NodeType != NodeType.Dimension)
            {
                return null;
            }

            return GetDefaultChild();
        }

        private DimensionNode GetDefaultChild()
        {

            if (this.children != null)
            {
                foreach (DimensionNode dn in this.children)
                {
                    if (dn.IsProhibited) continue;

                    if (dn.nodeInfo.NodeType == NodeType.Item &&
                        dn.nodeInfo.IsDefault)
                    {
                        return dn;
                    }
                    DimensionNode defDN = dn.GetDefaultChild();
                    if (defDN != null)
                    {
                        return defDN;
                    }
                }
            }


            return null;
        }

		internal bool HasDefaultChild()
		{

			if (this.children != null)
			{
				foreach (DimensionNode dn in this.children)
				{
                    if (dn.IsProhibited) continue;
					if (dn.nodeInfo.NodeType == NodeType.Item &&
						dn.nodeInfo.IsDefault)
					{
						return true;
					}

					if (dn.HasDefaultChild())
					{
						return true;
					}
				}
			}
			

			return false;
		}

		public override void RecursivelyGetAllTargetRoleInfos(string currentRole,
			ref Dictionary<string, string> allTargetRoles)
		{

			if (this.IsProhibited) return;

			if (this.children == null) return;

			if ( this.nodeInfo != null && !string.IsNullOrEmpty(this.nodeInfo.TargetRole))
			{
				if (!currentRole.Equals(this.nodeInfo.TargetRole) )
				{
					allTargetRoles[currentRole] = nodeInfo.TargetRole;
				}
			}

			foreach (Node n in this.children)
			{
				n.RecursivelyGetAllTargetRoleInfos(currentRole, ref allTargetRoles);
			}

			return;
		}


		internal void GetChildrenCount(   ref int count)
		{
			if (this.children != null)
			{
				foreach (DimensionNode dn in this.children)
				{
					count++;
					dn.GetChildrenCount( ref count);


				}
			}
			
		}
//
//        public void AddHyperCubeChildInfo(DefinitionLocator hdl,
//            DefinitionLocator.LocatorIndex indexInfo, string role )
//        {
//            MeasureHyperCubeInfo mhci = new MeasureHyperCubeInfo();
//            mhci.HRef = hdl.HRef;
//            mhci.RoleRef = role;
//            mhci.IsScenario = indexInfo.isScenario;
//            mhci.IsClosed = indexInfo.isClosed;
//            mhci.IsAllRelationShip = indexInfo.isAllRelationShip;
//            if (measureHyperCubeInfos == null)
//            {
//                measureHyperCubeInfos = new ArrayList();
//            }
//            measureHyperCubeInfos.Add(mhci);
//
//        }
//
//
//        public void GetNetMeasureHyperCubeInfos(ArrayList netInfo)
//        {
//            if (this.measureHyperCubeInfos != null)
//            {
//                netInfo.AddRange(measureHyperCubeInfos);
//            }
//
//            if (this.parent != null)
//            {
//                (parent as DimensionNode).GetNetMeasureHyperCubeInfos(netInfo);
//            }
//        }
        #endregion
    
    }
}

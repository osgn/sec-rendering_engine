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
	/// DefinitionLocatorRelationshipInfo
	/// </summary>
	[Serializable]
	public class DefinitionLocatorRelationshipInfo : LocatorRelationshipInfo
	{
        
		#region properties
		/// <summary>
		/// TargetRole to continue the relationship
		/// </summary>
		public string TargetRole
		{
			get { return NodeInfo.TargetRole; }
		}

		/// <summary>
		/// Type of the child item: Dimension, hypercube or item.
		/// </summary>
		public DimensionNode.NodeType NodeType
		{
			get { return NodeInfo.NodeType; }

		}

		/// <summary>
		/// True if the item is usable as a dimension item.
		/// </summary>
		public bool Usable
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Item)
				{
					return (NodeInfo as DefinitionNodeInfoMember).Usable;
				}

                return false;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsDefault
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Item)
				{
					return (NodeInfo as DefinitionNodeInfoMember).IsDefault;
				}

                return false;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsRequiresElement
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Item)
				{
					return (NodeInfo as DefinitionNodeInfoMember).IsRequiresElement;
				}

                return false;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsClosed
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Hypercube)
				{
					return (NodeInfo as DefinitionNodeInfoHypercube).IsClosed;
				}

				throw new ApplicationException("Unsupported request");

			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsScenario
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Hypercube)
				{
					return (NodeInfo as DefinitionNodeInfoHypercube).IsScenario;
				}

				throw new ApplicationException("Unsupported request");

			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsAllRelationShip
		{
			get
			{
				if (NodeInfo.NodeType == DimensionNode.NodeType.Hypercube)
				{
					return (NodeInfo as DefinitionNodeInfoHypercube).IsAllRelationShip;
				}

				throw new ApplicationException("Unsupported request");

			}
		}

		
		/// <summary>
		/// 
		/// </summary>
		public DefinitionNodeInfoBase NodeInfo;


	

		#endregion

		#region constructors
		private DefinitionLocatorRelationshipInfo()
		{

		}
		/// <summary>
		/// Creates a new instance of <see cref="DefinitionLocatorRelationshipInfo"/>.
		/// </summary>
		public DefinitionLocatorRelationshipInfo(DimensionNode.NodeType nodeType)
		{
			switch (nodeType)
			{
				case DimensionNode.NodeType.Dimension:
					this.NodeInfo = new DefinitionNodeInfoDimension(null);
					break;
				case DimensionNode.NodeType.Hypercube:
					this.NodeInfo = new DefinitionNodeInfoHypercube();
					break;

				case DimensionNode.NodeType.Item:
					this.NodeInfo = new DefinitionNodeInfoMember();
					break;


			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="DefinitionLocatorRelationshipInfo"/> 
		/// and initializes its properties from a parameter-supplied <see cref="DefinitionLocatorRelationshipInfo"/>
		/// </summary>
		/// <param name="orig"></param>
		public DefinitionLocatorRelationshipInfo(DefinitionLocatorRelationshipInfo orig)
			: base(orig)
		{
			this.NodeInfo = orig.NodeInfo.CreateCopy();
		}

		#endregion

		#region methods

        internal bool IsNonXlinkEquivalentRelationship(DefinitionLocatorRelationshipInfo other)
        {
            if (!base.IsNonXlinkEquivalentRelationship(other)) return false;
           

            return true;
        }


		internal static DefinitionLocatorRelationshipInfo CreateObj(string label, string priority,
			 float origOrder, float orderArg, string prefLabel, bool isProhibited,
			string targetRole, bool? isClosed,
			bool? isUsable, bool? isall, bool? isScenario,
			bool? isRequiresElement,
			bool? isDefault,
			DimensionNode.NodeType nodeType)
		{
			DefinitionLocatorRelationshipInfo obj = new DefinitionLocatorRelationshipInfo();
			obj.Label = label;
			obj.IsProhibited = isProhibited;
			obj.PrefLabel = prefLabel;
			obj.Order = orderArg;
			obj.OrigOrder = origOrder;
			obj.Priority = Convert.ToInt32(priority);
			obj.NodeInfo = 	DefinitionNodeInfoBase.CreateDefinitionNodeInfo(isClosed,
			isUsable, isall, isScenario, isDefault, isRequiresElement,
			targetRole, nodeType);

			if (prefLabel != null)
			{
				int lastSlash = prefLabel.LastIndexOf("/") + 1;
				obj.PrefLabel = prefLabel.Substring(lastSlash, prefLabel.Length - lastSlash);
			}

			return obj;
		}


		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="DefinitionLocatorRelationshipInfo"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="DefinitionLocatorRelationshipInfo"/>.  
		/// Assumed to be a <see cref="DefinitionLocatorRelationshipInfo"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="DefinitionLocatorRelationshipInfo"/>.</returns>
		/// <remarks>
		/// To be equal, the following properties in the two <see cref="DefinitionLocatorRelationshipInfo"/> must be equal:
		/// <bl>
		/// <li>Target role.</li>
		/// <li>Is closed.</li>
		/// <li>Usable.</li>
		/// <li>IsAllRelationship.</li>
		/// <li>IsScenario.</li>
		/// <li>IsDefault.</li>
		/// <li>IsRequiresElement.</li>
		/// <li>NodeType.</li>
		/// </bl>
		/// </remarks>
		public override bool Equals(object obj)
		{
			DefinitionLocatorRelationshipInfo lri = obj as DefinitionLocatorRelationshipInfo;
			if( !this.NodeInfo.Equals( lri.NodeInfo )) return false;
			
			return base.Equals( obj );
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="DefinitionLocatorRelationshipInfo"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="DefinitionLocatorRelationshipInfo"/>.
		/// </returns>
		/// <remarks>
		/// Override GetHashCode() is required for overriding Equals().
		/// </remarks>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		#endregion

	}
}
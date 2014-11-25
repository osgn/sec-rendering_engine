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
	/// 
	/// </summary>
	[Serializable]
	public abstract class DefinitionNodeInfoBase 
	{
		#region properties

		internal DimensionNode.NodeType NodeType = DimensionNode.NodeType.Item; //type of the child item..dimension, hypercube or item..
		public  string TargetRole = null; //target role to continue the relationship

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new DefinitionNodeInfo.
		/// </summary>
		public DefinitionNodeInfoBase()
		{
		}

		#endregion

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			DefinitionNodeInfoBase other = obj as DefinitionNodeInfoBase;
			if (this.NodeType != other.NodeType) return false;
			if (!string.Equals(this.TargetRole, other.TargetRole)) return false;

			return base.Equals(obj);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract DefinitionNodeInfoBase CreateCopy();

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isClosed"></param>
		/// <param name="isUsable"></param>
		/// <param name="isall"></param>
		/// <param name="isScenario"></param>
		/// <param name="isDefault"></param>
		/// <param name="isRequired"></param>
		/// <param name="targetRole"></param>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		public static DefinitionNodeInfoBase CreateDefinitionNodeInfo(bool? isClosed,
			bool? isUsable, bool? isall, bool? isScenario,
			bool? isDefault, bool? isRequired, string targetRole,  DimensionNode.NodeType nodeType)
		{
			DefinitionNodeInfoBase ret = null;
			switch( nodeType )
			{
				case DimensionNode.NodeType.Dimension:
					ret = new DefinitionNodeInfoDimension(targetRole);
					break;
				case DimensionNode.NodeType.Hypercube:
					ret = new DefinitionNodeInfoHypercube(isClosed, isall, isScenario, targetRole);
					break;

				case DimensionNode.NodeType.Item:
					ret = new DefinitionNodeInfoMember(isUsable, isDefault, isRequired, targetRole);
					break;


			}

			return ret;
		}

	}
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class DefinitionNodeInfoDimension : DefinitionNodeInfoBase
	{
		#region properties


		#endregion

		#region constructors

		/// <summary>
		/// Creates a new DefinitionNodeInfo.
		/// </summary>
		public DefinitionNodeInfoDimension( string targetRole )
		{
			this.NodeType = DimensionNode.NodeType.Dimension;
			this.TargetRole = targetRole;
		}

	
		#endregion

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{

			
			return base.Equals(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DefinitionNodeInfoBase CreateCopy()
		{
			DefinitionNodeInfoDimension ret = new DefinitionNodeInfoDimension(TargetRole);


			return ret;
		}


		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class DefinitionNodeInfoHypercube : DefinitionNodeInfoBase
	{
		#region properties

		private bool? isClosed = new Nullable<bool>();// = true; //if the cube is closed and cannot be expanded..
		private bool? isAllRelationShip = new Nullable<bool>();// = true; //is a cube is allowed for an item
		private bool? isScenario = new Nullable<bool>();// = false; //if a cube is sued as a scenario or segment 

		/// <summary>
		/// 
		/// </summary>
		public bool IsClosed
		{
			get
			{
				if (isClosed.HasValue)
				{
					return isClosed.Value;
				}

				return false; //default value...
			}
			set
			{
				isClosed = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsAllRelationShip
		{
			get
			{
				if (isAllRelationShip.HasValue)
				{
					return isAllRelationShip.Value;
				}

				return true; //default value...
			}
			set
			{
				isAllRelationShip = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsScenario
		{
			get
			{
				if (isScenario.HasValue)
				{
					return isScenario.Value;
				}

				return false; //default value...
			}
			set
			{
				isScenario = value;
			}
		}




		#endregion

		#region constructors

		/// <summary>
		/// Creates a new DefinitionNodeInfo.
		/// </summary>
		public DefinitionNodeInfoHypercube()
		{
			this.NodeType = DimensionNode.NodeType.Hypercube;
		}
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="isCls"></param>
		/// <param name="isall"></param>
		/// <param name="isSce"></param>
		/// <param name="targetRole"></param>
		public DefinitionNodeInfoHypercube(bool? isCls,
			bool? isall, bool? isSce, string targetRole)
		{
			this.NodeType = DimensionNode.NodeType.Hypercube;
			this.TargetRole = targetRole;
			this.isClosed = isCls;
			this.isAllRelationShip = isall;
			this.isScenario = isSce;
			
		}
		#endregion


		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			DefinitionNodeInfoHypercube other = obj as DefinitionNodeInfoHypercube;
			//check the properties as we want to assume default values if it is not specified...
			//for equality purposes....
			if (this.IsClosed != other.IsClosed) return false;
			if (this.IsAllRelationShip != other.IsAllRelationShip) return false;
			if (this.IsScenario != other.IsScenario) return false;

			return base.Equals(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DefinitionNodeInfoBase CreateCopy()
		{
			DefinitionNodeInfoHypercube ret = new DefinitionNodeInfoHypercube();

			ret.TargetRole = this.TargetRole;
			ret.isScenario = this.isScenario;
			ret.isClosed = this.isClosed;
			ret.isAllRelationShip = this.isAllRelationShip;


			return ret;
		}
		#endregion
	}
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class DefinitionNodeInfoMember : DefinitionNodeInfoBase
	{
		#region properties

		private bool? usable = new Nullable<bool>();// = true; //if the item is usable as a dimension item
		/// <summary>
		/// 
		/// </summary>
		public bool Usable
		{
			get
			{
				if (usable.HasValue)
				{
					return usable.Value;
				}
				if (IsDefault) return false;
				return true; //default value...
			}
			set
			{
				usable = value;
			}
		}


		private bool? isDefault = new Nullable<bool>();// = false; //default item for a dimension
		/// <summary>
		/// 
		/// </summary>
		public bool IsDefault
		{
			get
			{
				if (isDefault.HasValue)
				{
					return isDefault.Value;
				}

				return false; //default value...
			}
			set
			{
				isDefault = value;
			}
		}

		private bool? isRequiresElement = new Nullable<bool>();// = false; //reqyuires element relationship
		/// <summary>
		/// 
		/// </summary>
		public bool IsRequiresElement
		{
			get
			{
				if (isRequiresElement.HasValue)
				{
					return isRequiresElement.Value;
				}

				return false; //default value...
			}
			set
			{
				isRequiresElement = value;
			}
		}


		#endregion

		#region constructors

		/// <summary>
		/// Creates a new DefinitionNodeInfo.
		/// </summary>
		public DefinitionNodeInfoMember()
		{
			this.NodeType = DimensionNode.NodeType.Item;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="isUsable"></param>
		/// <param name="isDef"></param>
		/// <param name="isReq"></param>
		/// <param name="targetRole"></param>
		public DefinitionNodeInfoMember(bool? isUsable, bool? isDef, bool? isReq, string targetRole)
		{
			this.NodeType = DimensionNode.NodeType.Item;
			this.TargetRole = targetRole;


			this.usable = isUsable;
			this.isDefault = isDef;
			this.isRequiresElement = isReq;

		}
		#endregion


		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			DefinitionNodeInfoMember other = obj as DefinitionNodeInfoMember;
			//check the properties as we want to assume default values if it is not specified...
			//for equality purposes....
			if (this.Usable != other.Usable) return false;
			if (this.IsDefault != other.IsDefault) return false;
			if (this.IsRequiresElement != other.IsRequiresElement) return false;

			return base.Equals(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override DefinitionNodeInfoBase CreateCopy()
		{
			DefinitionNodeInfoMember ret = new DefinitionNodeInfoMember();

			ret.TargetRole = this.TargetRole;
			ret.usable = this.usable;
			ret.isDefault = this.isDefault;
			ret.isRequiresElement = this.isRequiresElement;


			return ret;
		}
		#endregion

	}
}
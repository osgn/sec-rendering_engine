//=============================================================================
// ScenarioSorter (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
//  This data class implements sorting of scenarions -- if dimension based, sort by the 
//  presenatation linkbase.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	/// <summary>
	/// ScenarioSorter
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class ScenarioValueSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			Scenario scenX = (Scenario)x;
			Scenario scenY = (Scenario)y;

			return Compare(scenX, scenY);
		}

		#endregion

		public static int Compare( Scenario x, Scenario y )
		{
			int typeCompare = x.ValueType.CompareTo( y.ValueType );
			if ( typeCompare == 0 )
			{
				return x.ValueName.CompareTo( y.ValueName );
			}

			return typeCompare;
		}
	}

	public class ScenarioOrderSorter : IComparer
	{
		private Dictionary<string, NodeSortingWrapper> nodeSortingWrappers = null;

		public ScenarioOrderSorter(Dictionary<string, NodeSortingWrapper> sortingWrappers)
		{
			this.nodeSortingWrappers = sortingWrappers;
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			Scenario scenX = x as Scenario;
			Scenario scenY = y as Scenario;

			int retValue = 0;
			if (scenX.DimensionInfo != null && scenY.DimensionInfo != null)
			{
				//both segments are dimensions, sort by the order defined in the taxonomy
				NodeSortingWrapper xWrapper = nodeSortingWrappers.ContainsKey(scenX.DimensionInfo.ToString()) ? nodeSortingWrappers[scenX.DimensionInfo.ToString()] : null;
				NodeSortingWrapper yWrapper = nodeSortingWrappers.ContainsKey(scenY.DimensionInfo.ToString()) ? nodeSortingWrappers[scenY.DimensionInfo.ToString()] : null;

				if (xWrapper == null && yWrapper == null)
				{
					return ScenarioValueSorter.Compare(scenX, scenY);
				}
				else if (xWrapper == null && yWrapper != null)
				{
					return -1;
				}
				else if (yWrapper == null && xWrapper != null)
				{
					return 1;
				}

				DimensionNode xNode = null;
				DimensionNode yNode = null;

				//Determine if the nodes are at the same level.  If they are not then find the parent/grandparent
				//of the lower level node that is at the same level as the other node.
				if (xWrapper.Level > yWrapper.Level)
				{
					Node node = xWrapper.TheNode;
					for (int i = xWrapper.Level; i > yWrapper.Level; i--)
					{
						node = node.Parent;
					}
					xNode = node as DimensionNode;
					yNode = yWrapper.TheNode as DimensionNode;
				}
				else if (xWrapper.Level < yWrapper.Level)
				{
					Node node = yWrapper.TheNode;
					for (int i = yWrapper.Level; i > xWrapper.Level; i--)
					{
						node = node.Parent;
					}
					xNode = xWrapper.TheNode as DimensionNode;
					yNode = node as DimensionNode;
				}
				else
				{
					xNode = xWrapper.TheNode as DimensionNode;
					yNode = yWrapper.TheNode as DimensionNode;
				}

				//Do the nodes share the same parent, if so then sort based on their own order
				//otherwise sort on the order of the parents.
				if (xNode.Parent.Id == yNode.Parent.Id)
				{
					retValue = xNode.Order.CompareTo(yNode.Order);
					if (retValue != 0) return retValue;

					//The xNode and yNode have the same order, and the same parent, so they must be the same node
					//check to see if the original nodes are different and if so sort the lower level node after the
					//higher level one
					retValue = xWrapper.Level.CompareTo(yWrapper.Level);
					if (retValue != 0) return retValue;
				}
				else
				{
					//Navigate up the tree until we find a common parent, then compare the order of the nodes
					//one level down from that parent
					DimensionNode xParent = xNode.Parent as DimensionNode;
					DimensionNode yParent = yNode.Parent as DimensionNode;
					while (xParent != null &&
						yParent != null &&
						xParent.Id != yParent.Id)
					{
						xNode = xParent;
						yNode = yParent;
						xParent = xNode.Parent as DimensionNode;
						yParent = yNode.Parent as DimensionNode;
					}

					if (string.IsNullOrEmpty(xParent.Id) && string.IsNullOrEmpty(xParent.Id))
					{
						//The dimensions belong to different reports, sort by the report name
						retValue = xParent.Label.CompareTo(yParent.Label);
						if (retValue != 0) return retValue;
					}
					else
					{
						retValue = xNode.Order.CompareTo(yNode.Order);
						if (retValue != 0) return retValue;
					}
				}

				//The nodes are equal from a dimension standpoint, just make sure the values are the same
				//as well 
				return ScenarioValueSorter.Compare(scenX, scenY);
			}
			else
			{
				//One or both of the scenarios are not dimensions, sort by value
				return ScenarioValueSorter.Compare(scenX, scenY);
			}
		}

		#endregion
	}

}
 //=============================================================================
// SegmentSorter (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
//  This data class implements sorting of segments -- if dimension based, sort by the 
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
	/// SegmentSorter
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class SegmentValueSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			Segment xSeg = (Segment)x;
			Segment ySeg = (Segment)y;

			return Compare( xSeg, ySeg );
		}

		#endregion

		public static int Compare( Segment x, Segment y )
		{
			int typeCompare = x.ValueType.CompareTo( y.ValueType );
			if ( typeCompare == 0 )
			{
				return x.ValueName.CompareTo( y.ValueName );
			}

			return typeCompare;
		}
	}

	public class SegmentOrderSorter: IComparer
	{
		private Dictionary<string, NodeSortingWrapper> nodeSortingWrappers = null;

		public SegmentOrderSorter(Dictionary<string, NodeSortingWrapper> sortingWrappers)
		{
			this.nodeSortingWrappers = sortingWrappers;
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			Segment segX = x as Segment;
			Segment segY = y as Segment;

			int retValue = 0;
			if (segX.DimensionInfo != null && segY.DimensionInfo != null)
			{
				//both segments are dimensions, sort by the order defined in the taxonomy
				NodeSortingWrapper xWrapper = nodeSortingWrappers.ContainsKey(segX.DimensionInfo.ToString()) ? nodeSortingWrappers[segX.DimensionInfo.ToString()] : null;
				NodeSortingWrapper yWrapper = nodeSortingWrappers.ContainsKey(segY.DimensionInfo.ToString()) ? nodeSortingWrappers[segY.DimensionInfo.ToString()] : null;
				if (xWrapper == null && yWrapper == null)
				{
					return SegmentValueSorter.Compare(segX, segY);
				}
				else if (xWrapper == null && yWrapper != null)
				{
					return 1;
				}
				else if (yWrapper == null && xWrapper != null)
				{
					return -1;
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
					while ( xParent != null &&
						yParent != null &&
						xParent.Id != yParent.Id)
					{
						xNode = xParent;
						yNode = yParent;
						xParent = xNode.Parent as DimensionNode;
						yParent = yNode.Parent as DimensionNode;
					}

					if (xParent == null || yParent == null)
					{
						//One node got to the root before the other node, continue up the tree for 
						//the other node until it reaches the root and sort by reportname
						if (xParent != null)
						{
							while (xParent != null)
							{
								xNode = xParent;
								xParent = xNode.Parent as DimensionNode;
							}
						}
						else if (yParent != null)
						{
							while (yParent != null)
							{
								yNode = yParent;
								yParent = yNode.Parent as DimensionNode;
							}
						}
						//The dimensions belong to different reports, sort by the report name
						retValue = xNode.Label.CompareTo(yNode.Label);
						if (retValue != 0) return retValue;
					}
					else
					{
						if (string.IsNullOrEmpty(xParent.Id) && string.IsNullOrEmpty(yParent.Id))
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
				}

				//The nodes are equal from a dimension standpoint, just make sure the values are the same
				//as well 
				return SegmentValueSorter.Compare(segX, segY);
			}
			else
			{
				//One or both of the segments are not dimensions, sort by value
				return SegmentValueSorter.Compare(segX, segY);
			}
		}

		#endregion
	}
}
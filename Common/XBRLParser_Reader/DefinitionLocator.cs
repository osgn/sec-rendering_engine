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
using System.Diagnostics;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Aucent.MAX.AXE.Common.Exceptions;
using Aucent.MAX.AXE.Common.Utilities;


namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents a locator link within the definition linkbase.
	/// </summary>
	[Serializable]
	public class DefinitionLocator : LocatorBase, IComparable
	{

		#region properties
		/// <summary>
		/// Key  = href
		/// Value = ChildDefinitionLocator
		/// </summary>
		internal HybridDictionary childLocatorsByHRef;

		/// <summary>
		/// Key = order
		/// Value = href
		/// </summary>
		private HybridDictionary childDisplayOrder;

		ArrayList parents = null;
		private ArrayList Parents
		{
			get { return parents; }
		}

		private bool HasParent
		{
			get { return parents != null; } 
		}

		private Element e = null;
		/// <summary>
		/// The <see cref="Element"/> underlying this <see cref="DefinitionLocator"/>.
		/// </summary>
		public Element MyElement
		{
			set { e = value; }
			get { return e; }
		}

		#endregion

		#region constructors
		/// <summary>
		/// Creates a new instance of <see cref="DefinitionLocator"/>.
		/// </summary>
		public DefinitionLocator()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="DefinitionLocator"/>.
		/// </summary>
		/// <param name="hrefArg">The "href" property value to be assigned to the 
		/// new <see cref="DefinitionLocator"/>.</param>
		public DefinitionLocator(string hrefArg)
			: base(hrefArg)
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="DefinitionLocator"/> and 
		/// initializes its properties from a parameter-supplied <see cref="DefinitionLocator"/>.
		/// </summary>
		public DefinitionLocator(DefinitionLocator copy)
		{

			this.childLocatorsByHRef = copy.childLocatorsByHRef;
			this.childDisplayOrder = copy.childDisplayOrder;
			
			this.e = copy.e;
			this.HRef = copy.HRef;
			this.LabelArray = copy.LabelArray;
			this.e = copy.MyElement;
			this.parents = copy.parents;
			this.unpartitionedHref = copy.unpartitionedHref;
			this.xsd = copy.xsd;

		
		}

		#endregion

		#region methods
		private int GetNextPriority(string childId)
		{
			if (childLocatorsByHRef != null)
			{
				ChildDefinitionLocator cpl = childLocatorsByHRef[childId] as ChildDefinitionLocator;

				if (cpl != null)
				{
					int maxPriority = 0;
					foreach (LocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos)
					{
						maxPriority = Math.Max(lri.Priority, maxPriority);
					}

					return maxPriority + 1;
				}
			}

			return 1;
		}


		internal void AddChild( DefinitionLocator childPl,
			float orderArg, string prefLabel, 
			string targetRole, bool? isClosed, 
			bool? isUsable, bool? isall, bool? isScenario,
			bool? isDefault, bool? isRequiresElement, DimensionNode.NodeType nodeType,
			string priority, bool isProhibited )
		{

			childPl.AddParent( this );

			if( this.childLocatorsByHRef == null )
			{
				this.childLocatorsByHRef = new HybridDictionary();
				this.childDisplayOrder = new HybridDictionary();
			}

            float dispOrder = orderArg;

			DefinitionLocatorRelationshipInfo newRelation = DefinitionLocatorRelationshipInfo.CreateObj( prefLabel,
				priority, orderArg,dispOrder, prefLabel,  isProhibited, 
				targetRole ,isClosed, isUsable, isall,isScenario , isRequiresElement, isDefault, nodeType );

			ChildDefinitionLocator cpl = childLocatorsByHRef[childPl.HRef] as ChildDefinitionLocator;

			if ( cpl == null )
			{
				// doesn't exist in either the href table or the order table
				cpl = new ChildDefinitionLocator( childPl.HRef, newRelation );
				
				childLocatorsByHRef.Add( childPl.HRef, cpl );

				// keep these separate so they don't impact each other
				#pragma warning disable 0219
				ChildDefinitionLocator cplOrder = new ChildDefinitionLocator( cpl );
				#pragma warning restore 0219

			}
			else 
			{
				// the cpl already exists, append the existing relation info
				cpl.AddRelationship( newRelation );

			}
		}

		
		internal void Append( DefinitionLocator arg)
		{			
			if ( arg.childLocatorsByHRef != null )
			{
				if ( this.childLocatorsByHRef == null )
				{
					this.childLocatorsByHRef = new HybridDictionary();
					this.childDisplayOrder = new HybridDictionary();

				}
				foreach( ChildDefinitionLocator cpl in arg.childLocatorsByHRef.Values )
				{
					
					ChildDefinitionLocator orig = this.childLocatorsByHRef[cpl.HRef] as ChildDefinitionLocator;

					if( orig == null )
					{
						
						childLocatorsByHRef[cpl.HRef] = cpl;
					}
					else
					{
						foreach( DefinitionLocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos )
						{
							if ( orig.CanAddRelationship( lri ))
							{
								orig.AddRelationship(lri);
							}
						}
					}

				}

			}
				
					
			if ( arg.parents != null && arg.parents.Count > 0 )
			{
				if ( parents == null )
				{
					parents = arg.parents;
				}
				else
				{
					// unique parents only please
					foreach ( DefinitionLocator parent in arg.Parents )
					{
						if ( !parents.Contains( parent ) )
						{
							parents.Add( parent );
						}
					}
				}
			}

			foreach ( string label in arg.labelArray )
			{
				AddLabel( label );
			}
		}




	
		#endregion

		#region Create Node

		internal DimensionNode CreateDimensionNode(string lang, string role,
			Node parentNode, string parentId , DefinitionLink dLink, bool recursive, Hashtable definisionLinks,
			DefinitionLocatorRelationshipInfo lri, DimensionNode newNode, bool buildHypercubeChildren, bool isScenarioMarkup)
		{
			if (e == null) return null;

			if (newNode == null)
			{
				newNode = new DimensionNode(e);
				newNode.IsSegmentMarkup = !isScenarioMarkup;
				newNode.SetLabel(lang, role);
				newNode.NodeDimensionInfo = lri;
				newNode.IsDraggable = false;
				if (lri != null && lri.NodeType == DimensionNode.NodeType.Item && lri.Usable )
				{
					
                    if (!e.IsAbstract)
                    {
                        newNode.IsDraggable = true;
                    }
				}
                
				newNode.MyDefinitionLink = dLink;
			}

			if (lri != null)
			{
				newNode.SetOrder(lri.Order);


				if (PresentationLocator.preferredLabelRole.Equals(role))
				{
					if (lri.PrefLabel != null)
					{
						newNode.UpdatePreferredLabel(lang, lri.PrefLabel);
					}
				}
			}


			if (parentNode != null)
			{
				// and add it
				parentNode.AddChild(newNode);
				parentId = parentNode.Id;
			}

			
			
		
			if (recursive)
			{

				if (lri != null && lri.TargetRole != null && dLink.Role != lri.TargetRole)
				{
					if (definisionLinks != null)
					{
						DefinitionLink targetDLink = definisionLinks[lri.TargetRole] as DefinitionLink;

						if (targetDLink != null)
						{
							DefinitionLocator targetLocator;
							if (targetDLink.TryGetLocator(newNode.Id, out targetLocator))
							{

								CreateChildNodes(lang, role, newNode,
									targetDLink, definisionLinks,
									targetLocator.childLocatorsByHRef, buildHypercubeChildren, isScenarioMarkup);
							}

						}

					}
					if (newNode.Children != null)
					{
						newNode.Children.Sort(new NodeOrderSorter());
					}
					return newNode;

				}


				CreateChildNodes(lang, role, newNode, dLink, definisionLinks, childLocatorsByHRef, buildHypercubeChildren, isScenarioMarkup);

				
				//need to sort the nodes 
				if (newNode.Children != null)
				{
					newNode.Children.Sort(new NodeOrderSorter());
				}
			}


			return newNode;
		}


		private void CreateChildNodes(string lang, string role,
			DimensionNode parentNode ,
			DefinitionLink dLink,  Hashtable definisionLinks,
			HybridDictionary ChildDefinitionLocatorsHD, bool buildHypercubeChildren, bool isScenarioMarkup)
		{
			if (ChildDefinitionLocatorsHD == null) return;

            if (!buildHypercubeChildren)
            {
                if (parentNode.NodeDimensionInfo != null &&
                    parentNode.NodeDimensionInfo.NodeType == DimensionNode.NodeType.Hypercube)
                {
                    //no need to build the children...
                    return;
                }
            }
			foreach (ChildDefinitionLocator cdl in ChildDefinitionLocatorsHD.Values)
			{
				
				DefinitionLocator childLocator;
				if (!dLink.TryGetLocator(cdl.HRef, out childLocator))
				{
					continue;
				}
				
				bool recursive = true;
				if (parentNode != null && parentNode.GetParent(childLocator.HRef) != null)
				{
					recursive = false; //we have a recursion..
					//this might be ok if one of the links is a prohibitted link...

				}

				//organize locators by non xlink attributes 
				//we should have only one valid LocatorRelationshipInfo for each order...
                DefinitionLocatorRelationshipInfo currentRelationShip=null;

				for (int i = cdl.LocatorRelationshipInfos.Count - 1; i >= 0; i--)
				{
					DefinitionLocatorRelationshipInfo lri = cdl.LocatorRelationshipInfos[i] as DefinitionLocatorRelationshipInfo;
                    if (currentRelationShip != null && lri.IsNonXlinkEquivalentRelationship(currentRelationShip)) continue;
                    currentRelationShip = lri;

					//if (lri.IsDefault) continue; //this is just the default definition...no need to include it when we build the tree..
					if ( lri.IsProhibited)
						continue;

					childLocator.CreateDimensionNode(lang, role,
						 parentNode, parentNode.Id, dLink, recursive, definisionLinks, lri, null, buildHypercubeChildren, isScenarioMarkup );


					



				}



			}

		}



		#endregion

		internal void AddParent( DefinitionLocator parent )
		{
			if ( parents == null )
			{
				parents = new ArrayList();
			}

			// BUG 1610 - don't add duplicate parents
			if ( !parents.Contains( parent ) )
			{
				parents.Add( parent );
			}
		}


		/// <summary>
		/// Determines whether the supplied <see cref="Object"/> is equal to this <see cref="DefinitionLocator"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="DefinitionLocator"/>.  
		/// Assumed to be a <see cref="DefinitionLocator"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="DefinitionLocator"/>.</returns>
		/// <remarks>To be equal, the label and href of the two <see cref="DefinitionLocator"/> objects must be equal.</remarks>
		/// <exception cref="AucentException">If <paramref name="obj"/> is not a <see cref="DefinitionLocator"/>.</exception>
		public override bool Equals(object obj)
		{
			if ( !(obj is DefinitionLocator ) ) throw new AucentException( "Given an unknown object type to DefinitionLocator.Equals(). object type is: " + obj.ToString() );
			if ( !((DefinitionLocator)obj).Label.Equals( Label ) ) return false;
			return ((DefinitionLocator)obj).href.Equals( href );
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="DefinitionLocator"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="DefinitionLocator"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		internal void SetRequiredElementRelationshipInfo(ref List<string> vals)
		{
			foreach( ChildDefinitionLocator cdl in this.childLocatorsByHRef.Values )
			{
				float checkedOrder = -1;
				for( int i = cdl.LocatorRelationshipInfos.Count-1; i >= 0 ; i-- )
				{
					DefinitionLocatorRelationshipInfo dlri = cdl.LocatorRelationshipInfos[i] as DefinitionLocatorRelationshipInfo;

					if( dlri.IsRequiresElement != true || dlri.OrigOrder == checkedOrder ) continue;

					checkedOrder = dlri.OrigOrder;

					if( !dlri.IsProhibited )
					{
						vals.Add( cdl.HRef );
					}

				}
				
			}
		}
	
        #region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="DefinitionLocator"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="DefinitionLocator"/>
		/// is to be compared.  Assumed to be a <see cref="DefinitionLocator"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="DefinitionLocator"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> 
		/// for the hrefs of the two <see cref="DefinitionLocator"/>.</remarks>
		public int CompareTo(object obj)
        {
            DefinitionLocator other = obj as DefinitionLocator;

            return this.href.CompareTo(other.href);
        }

        #endregion

		internal DefinitionLocator CreateCopyForMerging()
		{
			DefinitionLocator copy = new DefinitionLocator();
			copy.CopyLocatorBaseInformation(this);
			if (this.childDisplayOrder != null)
			{
				copy.childDisplayOrder = new HybridDictionary();
				foreach (DictionaryEntry de1 in this.childDisplayOrder)
				{
					copy.childDisplayOrder[de1.Key] = de1.Value;
				}

			}

			if (this.childLocatorsByHRef != null)
			{
				copy.childLocatorsByHRef = new HybridDictionary();
				foreach (DictionaryEntry de1 in this.childLocatorsByHRef)
				{
					ChildDefinitionLocator cdl = de1.Value as ChildDefinitionLocator;
					copy.childLocatorsByHRef[de1.Key] = cdl.CreateCopyForMerging();
				}
			}


			//parent info udjused later
			copy.parents = this.parents;


			return copy;
		}

		/// <summary>
		/// Map the parent values back to the new locator list
		/// </summary>
		/// <param name="newLocatorList"></param>
		internal void ResetParentInformation(Hashtable newLocatorList)
		{
			if (this.parents != null && this.parents.Count > 0)
			{

				ArrayList newList = new ArrayList(this.parents.Count);

				for (int i = 0; i < this.parents.Count; i++)
				{
					DefinitionLocator dl = parents[i] as DefinitionLocator;
					DefinitionLocator newdl = newLocatorList[dl.href] as DefinitionLocator;
					if (newdl != null)
					{
						newList.Add(newdl);
					}
					else
					{
						throw new ApplicationException("Failed to find corresponding parent values");
					}
				}

				this.parents = newList;
			}
		}
    }
}
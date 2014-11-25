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
using System.Collections.Specialized;

using Aucent.MAX.AXE.Common.Exceptions;
using Aucent.MAX.AXE.Common.Utilities;


namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents a "locator" type link within an XBRL presentation linkbase.
	/// </summary>
	[Serializable]
	public class PresentationLocator : LocatorBase
	{
        public static string preferredLabelRole = TraceUtility.FormatStringResource("XBRLAddin.PreferredLabelRole");

		#region properties
		
		/// <summary>
		/// Key  = href
		/// Value = ChildLocator
		/// </summary>
		internal HybridDictionary childLocatorsByHRef;

		/// <summary>
		/// Key = order
		/// Value = href
		/// </summary>
		internal HybridDictionary childDisplayOrder;

		ArrayList parents = null;
		private ArrayList Parents
		{
			get { return parents; }
		}

		internal bool HasParent
		{
			get { return parents != null; } 
		}

		private Element e = null;

		/// <summary>
		/// The <see cref="Element"/> underlying this <see cref="PresentationLocator"/>.
		/// </summary>
		public Element MyElement
		{
			set { e = value; }
			get { return e; }
		}

        public HybridDictionary ChildDisplayOrder
        {
            get { return childDisplayOrder; }
        }

        public HybridDictionary ChildLocatorsByHRef
        {
            get { return childLocatorsByHRef; }
        }

		#endregion

		#region constructors
		/// <summary>
		/// Constructs a new instance of <see cref="PresentationLocator"/>.
		/// </summary>
		public PresentationLocator()
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="PresentationLocator"/>.
		/// </summary>
		/// <param name="hrefArg">The "raw" value of the locator link's "href" attribute.</param>
		public PresentationLocator(string hrefArg)
			: base(hrefArg)
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="PresentationLocator"/>, initializing 
		/// properties from a parameter-supplied <see cref="PresentationLocator"/>.
		/// </summary>
		/// <param name="copy">The <see cref="PresentationLocator"/> from which the properties
		/// of the new <see cref="PresentationLocator"/> are to be initialized.</param>
		public PresentationLocator(PresentationLocator copy)
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

		public void AddChild(  PresentationLocator childPl, float orderArg,
			string prefLabel, string calculationWeight  )
		{
			AddChild( childPl,childPl.Label, "0", orderArg, prefLabel, calculationWeight ,false );
		}

		public void AddChild( PresentationLocator childPl, string label,  string priority,
			float orderArg,  string prefLabel, string weight, bool isProhibited )
		{
			childPl.AddParent( this );

			if( this.childLocatorsByHRef == null )
			{
				this.childLocatorsByHRef = new HybridDictionary();
				this.childDisplayOrder = new HybridDictionary();
			}


			LocatorRelationshipInfo newRelation = LocatorRelationshipInfo.CreateObj( label,
                priority, orderArg, orderArg, prefLabel, weight, isProhibited);
			ChildPresentationLocator cpl = childLocatorsByHRef[childPl.HRef] as ChildPresentationLocator;

			if ( cpl == null )
			{
				// doesn't exist in either the href table or the order table
				cpl = new ChildPresentationLocator( childPl.HRef, newRelation );
				
				childLocatorsByHRef.Add( childPl.HRef, cpl );

				// keep these separate so they don't impact each other
				ChildPresentationLocator cplOrder = new ChildPresentationLocator( cpl );

			}
			else 
			{
				// the cpl already exists, append the existing relation info
				cpl.AddRelationship( newRelation );

			}
		}

		
		public void Append( PresentationLocator arg)
		{
            
			if ( arg.childLocatorsByHRef != null )
			{
				if ( this.childLocatorsByHRef == null )
				{
					this.childLocatorsByHRef = new HybridDictionary();
					this.childDisplayOrder = new HybridDictionary();

				}

				foreach( ChildPresentationLocator cpl in arg.childLocatorsByHRef.Values )
				{
					ChildPresentationLocator orig = this.childLocatorsByHRef[cpl.HRef] as ChildPresentationLocator;

					if( orig == null )
					{
						
						childLocatorsByHRef[cpl.HRef] = cpl;
					}
					else
					{
						foreach( LocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos )
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
					foreach ( PresentationLocator parent in arg.Parents )
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



		


		#region Create Node

		/// <summary>
		/// Creates a Node and also any children - e.g. it's recursive
		/// </summary>
		/// <param name="lang">The default language to use.</param>
		/// <param name="role">The default label role to use.</param>
		/// <param name="parentId"></param>
		/// <param name="pLink"></param>
		/// <returns></returns>
		/// <remarks>
		/// Note: Prohibited relationships are still created.  It is the job of the presenter to either ignore the 
		/// prohibited flag, or follow the rules and not display the node.
		/// </remarks>
		public Node CreateNode( string lang, string role,  string parentId , PresentationLink pLink)
		{
			return CreateNode( lang, role,  0,  parentId, null,null, pLink , true, null  );
		}


		public Node CreateNode( string lang, string role, int level,
			string currentId, Node parentNode, LocatorRelationshipInfo relation, 
			PresentationLink pLink, bool recursive,
			Dimension dimensionInfo )
		{
			Node current = null;

			if (e == null && pLink != null)
			{
				/* Something went wrong during mergePresentation because we should have an element at 
				 * this point.  Go to the presentationLink and try to get the element. */
				PresentationLocator loc = null;
				pLink.TryGetLocator(this.HRef, out loc);
				if (loc != null && loc.MyElement != null)
				{
					//update the element and the child locators
					e = loc.MyElement;
					this.childLocatorsByHRef = loc.childLocatorsByHRef;
				}
			}
            if (e == null)
            {
                //it is a locator to an invalid element

                //just return null at this point...
                return null;
            }
			//make sure we have the right element if there's a parent (there could be clones)
            Element elementToUse = e;
            
			if (dimensionInfo != null && elementToUse != null)
			{
				if (elementToUse.IsDimensionItem())
				{
					//get the parent node 
					if (parentNode != null)
					{
						
							//DimensionNode hierNode;
							//if (dimensionInfo.TryGetHypercubeNode(lang,
							//    role, pLink.Role, parentNode.Id, out hierNode))
							//{
							//    foreach (DimensionNode dn in hierNode.Children)
							//    {
							//        if (dn.Id == elementToUse.Id)
							//        {
							//            if (parentNode != null)
							//            {

							//                 and add it
							//                parentNode.AddChild(dn);

							//            }

							//            return dn;
							//        }
							//    }


							//}







						DimensionNode hierNode;
						if (dimensionInfo.TryGetHypercubeNode(lang,
							role, pLink.Role, parentNode.Id,true, out hierNode))
						{
                            if (hierNode.Children != null)
                            {
                                foreach (DimensionNode dn in hierNode.Children)
                                {
                                    if (dn.Id == elementToUse.Id)
                                    {
                                        if (parentNode != null)
                                        {

                                            // and add it
                                            parentNode.AddChild(dn);

                                        }

                                        return dn;
                                    }
                                }

                            }


							return null; //strange ..could not find the correct hierarchy..
						}
						else
						{
							return null;
						}
					}
				}
			}

		


			current = elementToUse==null ? new Node( href ) : elementToUse.CreateNode( lang, role, false );

			if (relation != null)
			{
				current.SetOrder(relation.Order);
				current.CalculationWeight = relation.CalculationWeight;

				if (preferredLabelRole.Equals(role))
				{
					if (relation.PrefLabel != null)
					{
						current.UpdatePreferredLabel(lang, relation.PrefLabel);
					}
				}
				// now check to see if it's prohibited
				current.IsProhibited = relation.IsProhibited;

			}
			if (parentNode != null)
			{



				// and add it
				parentNode.AddChild(current);

			}

		

			if ( childLocatorsByHRef != null && recursive )
			{
               

				foreach( ChildPresentationLocator cpl in childLocatorsByHRef.Values )
				{
					PresentationLocator childLocator;
					if ( !pLink.TryGetLocator( cpl.HRef, out childLocator ))
					{
						continue;
					}
					
					bool innerRecursive = true;
					if (parentNode != null && parentNode.GetParent(cpl.HRef) != null)
					{
						//this might be ok if one of the links is a prohibitted link...
						innerRecursive = false; //looks like we have a recursion...
					}

					//organize locators by base order
					//we should have only one valid LocatorRelationshipInfo for each non xlink information...
                    LocatorRelationshipInfo currentRelationShip=null;
                    for (int i = cpl.LocatorRelationshipInfos.Count - 1; i >= 0; i--)
                    {
                        LocatorRelationshipInfo lri = cpl.LocatorRelationshipInfos[i] as LocatorRelationshipInfo;
                        if (currentRelationShip != null && lri.IsNonXlinkEquivalentRelationship(currentRelationShip)) continue;
                        currentRelationShip = lri;

                        // always create the child
                        
                        childLocator.CreateNode(lang, role,
							level + 1, cpl.HRef, current, lri, pLink, innerRecursive, dimensionInfo);
                      

                    }

					
					
				}
				//need to sort the nodes 
				if (current.Children != null)
				{
					current.Children.Sort(new NodeOrderSorter());
				}
			}

			return current;
		}

		#endregion

	

		/// <summary>
		/// Determines the next available priority within the locator relationship 
		/// of a given child locator.
		/// </summary>
		/// <param name="childId">The id of the child for whom the next available locator 
		/// relationship priority is to be returned.</param>
		/// <returns>The calculated next available priority.</returns>
		public int GetNextPriority( string childId )
		{
			if ( childLocatorsByHRef != null )
			{
				ChildPresentationLocator cpl = childLocatorsByHRef[childId] as ChildPresentationLocator;

				if ( cpl != null )
				{
					int maxPriority = 0;
					foreach( LocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos )
					{
						maxPriority = Math.Max( lri.Priority, maxPriority );
					}
					
					return maxPriority+1;
				}
			}

			return 1;
		}


		internal string GetLocatorLabel( Node childNode , ref int priority )
		{
			if ( childLocatorsByHRef != null )
			{
                ChildPresentationLocator cpl = childLocatorsByHRef[childNode.Id] as ChildPresentationLocator;

				if ( cpl != null )
				{
					for (int i = cpl.LocatorRelationshipInfos.Count - 1; i >= 0; i--)
					{
						LocatorRelationshipInfo lri = cpl.LocatorRelationshipInfos[i] as LocatorRelationshipInfo;

                        if (lri.Order == (float)childNode.order &&
                            lri.PrefLabel == childNode.PreferredLabel &&
                            lri.CalculationWeight == childNode.Weight )
						{
							priority = lri.Priority;
							return lri.Label;
						}
					}
					LocatorRelationshipInfo netRel = cpl.NetRelationShipInfo;
					priority = netRel.Priority;
					return netRel.Label;
				}
			}

			return null;
		}

		internal bool IsChildProhibited( string id, double order )
		{
			ChildPresentationLocator cpl = childLocatorsByHRef[id] as ChildPresentationLocator;
			if( cpl != null )
			{
				for (int i = cpl.LocatorRelationshipInfos.Count - 1; i >= 0; i--)
				{
					LocatorRelationshipInfo lri = cpl.LocatorRelationshipInfos[i] as LocatorRelationshipInfo;

					if (lri.Order == order)
					{
						return lri.IsProhibited;
					}
				}

				return cpl.NetRelationShipInfo.IsProhibited;
			}

			return false;
		}

		private Element GetElementToUse( string parentId, Node parentNode )
		{
			//TODO: need to determine if we need to keep some sort of tuple hier in the node...

			return e;
		}

		internal void AddParent( PresentationLocator parent )
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
		/// Determines whether a supplied <see cref="object"/> is equivalent to this instance
		/// of <see cref="PresentationLocator"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="PresentationLocator"/>
		/// that is to be compared to this instance of <see cref="PresentationLocator"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the objects are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="PresentationLocator"/> objects are considered equal if their labels and
		/// HRef properties are equal.</remarks>
		public override bool Equals(object obj)
		{
			if ( !(obj is PresentationLocator ) ) throw new AucentException( "Given an unknown object type to PresentationLocator.Equals(). object type is: " + obj.ToString() );
			if ( !((PresentationLocator)obj).Label.Equals( Label ) ) return false;
			return ((PresentationLocator)obj).href.Equals( href );
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="PresentationLocator"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="PresentationLocator"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}


		internal PresentationLocator CreateCopyForMerging()
		{
			PresentationLocator copy = new PresentationLocator();
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
					ChildPresentationLocator cpl = de1.Value as ChildPresentationLocator;
					copy.childLocatorsByHRef[de1.Key] = cpl.CreateCopyForMerging();
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
		internal void ResetParentInformation(Hashtable newLocatorList )
		{
			if (this.parents != null && this.parents.Count > 0)
			{

				ArrayList newList = new ArrayList(this.parents.Count);

				for (int i = 0; i < this.parents.Count; i++)
				{
					PresentationLocator pl = parents[i] as PresentationLocator;
					PresentationLocator newPl = newLocatorList[pl.href] as PresentationLocator;
					if (newPl != null)
					{
						newList.Add(newPl);
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


	/// <summary>
	/// Provides method for comparing <see cref="PresentationLocator"/> objects.
	/// </summary>
	public class PresentationLocatorByHRefComparer : IComparer
    {
        #region IComparer Members

		/// <summary>
		/// Compare two <see cref="Object"/>s, assumed to be <see cref="PresentationLocator"/>s.
		/// </summary>
		/// <param name="x">The first <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="PresentationLocator"/>.</param>
		/// <param name="y">The second <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="PresentationLocator"/>.</param>
		/// <returns><bl>
		/// <li>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>.</li>
		/// <li>Zero if <paramref name="x"/> is equal to <paramref name="y"/>.</li>
		/// <li>Greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.</li>
		/// </bl></returns>
		/// <remarks>The results of the comparison is that of <see cref="String.CompareTo(String)"/> 
		/// on 
		/// the HRef of the two <see cref="PresentationLocator"/>s.</remarks>
		public int Compare(object x, object y)
        {
            PresentationLocator a = x as PresentationLocator;
            PresentationLocator b = y as PresentationLocator;

            return a.HRef.CompareTo(b.HRef);
        }

        #endregion
    }
}
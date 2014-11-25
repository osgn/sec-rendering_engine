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

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// ChildPresentationLocator
	/// </summary>
	[Serializable]
	public class ChildPresentationLocator 
	{
		#region properties

		string hRef;
		internal string HRef
		{
			get { return hRef; }
			set { hRef = value; }
		}
		
		internal LocatorRelationshipInfo NetRelationShipInfo
		{
			get { return (LocatorRelationshipInfo)locatorRelationshipInfos[locatorRelationshipInfos.Count-1]; }
		}

		private int NumInfos
		{
			get { return locatorRelationshipInfos.Count; }
		}

		/// <summary>
		/// List sorted by priority and is Prohibited flag
		/// </summary>
		private ArrayList locatorRelationshipInfos = new ArrayList();
		public ArrayList LocatorRelationshipInfos
		{
			get { return locatorRelationshipInfos; }
			set { locatorRelationshipInfos = value; }

		}

		#endregion

		#region constructors

		public ChildPresentationLocator()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="ChildPresentationLocator"/>.
		/// </summary>
		public ChildPresentationLocator( string hRef,
			LocatorRelationshipInfo info )
		{
		
			this.hRef = hRef;
			locatorRelationshipInfos.Add( info );
		}

		/// <summary>
		/// Creates a new instance of <see cref="ChildPresentationLocator"/>, copying properties
		/// from a parameter-supplied <see cref="ChildPresentationLocator"/>.
		/// </summary>
		/// <param name="cpl">A <see cref="ChildPresentationLocator"/> from which the properties
		/// of the new <see cref="ChildPresentationLocator"/> are to be copied.</param>
		public ChildPresentationLocator(ChildPresentationLocator cpl) 
		{
			hRef = cpl.hRef;

			foreach ( LocatorRelationshipInfo lri in cpl.locatorRelationshipInfos )
			{
				locatorRelationshipInfos.Add( lri );
			}
		}

//		public ChildPresentationLocator( string hRef, string label, 
//			string priority,
//			float orderArg,  string prefLabel, string weight, bool isProhibited  ) : base( href, LocatorRelationshipInfo.CreateObj(label, priority,
//			orderArg, prefLabel, weight, isProhibited ) )
//		{
//		}


		#endregion

		#region Public methods

		/// <summary>
		/// Determines if a parameter-supplied <see cref="LocatorRelationshipInfo"/> can 
		/// be added to this <see cref="ChildPresentationLocator"/>'s collection of 
		/// <see cref="LocatorRelationshipInfo"/>.  A <see cref="LocatorRelationshipInfo"/>
		///  can be added it is not already present in the collection.
		/// </summary>
		/// <param name="newRelation">The <see cref="LocatorRelationshipInfo"/> for which 
		/// method is to search.</param>
		/// <returns>True if <paramref name="newRelation"/> can be added (if it is not found 
		/// in the collection.  False otherwise.</returns>
		public bool CanAddRelationship( LocatorRelationshipInfo newRelation )
		{
			foreach( LocatorRelationshipInfo lri in this.locatorRelationshipInfos )
			{
				if ( lri.Equals( newRelation ) ) 
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Adds a parameter-supplied <see cref="LocatorRelationshipInfo"/> to 
		/// this <see cref="ChildPresentationLocator"/>'s collection of 
		/// <see cref="LocatorRelationshipInfo"/>.
		/// </summary>
		/// <param name="newRelation">The <see cref="LocatorRelationshipInfo"/> that method 
		/// is to add.</param>
		public void AddRelationship(LocatorRelationshipInfo newRelation)
		{
			if ( CanAddRelationship( newRelation ) )
			{
				this.locatorRelationshipInfos.Add( newRelation );
				locatorRelationshipInfos.Sort();
			}
		}

	
		#endregion

//		#region IComparable Members
//
//		public int CompareTo(object obj)
//		{
//			ChildPresentationLocator other  = (ChildPresentationLocator)obj;
//
//			return this.NetRelationShipInfo.Order.CompareTo( other.NetRelationShipInfo.Order );
//		}
//
//		#endregion


		internal  ChildPresentationLocator CreateCopyForMerging()
		{
			ChildPresentationLocator ret = new ChildPresentationLocator();
			ret.hRef = this.hRef;
			ret.locatorRelationshipInfos = new ArrayList(this.locatorRelationshipInfos.Count);
			for( int i = 0 ; i < locatorRelationshipInfos.Count ; i++ )
			{
				ret.locatorRelationshipInfos.Add( new LocatorRelationshipInfo( locatorRelationshipInfos[i] as LocatorRelationshipInfo ));
			}


			return ret;
		}
	}
}
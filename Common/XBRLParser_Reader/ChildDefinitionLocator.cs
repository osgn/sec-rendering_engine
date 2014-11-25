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
using System.Collections ;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// ChildDefinitionLocator
	/// </summary>
	[Serializable]
	public class ChildDefinitionLocator 
	{
		#region properties

		string hRef;
		internal string HRef
		{
			get { return hRef; }
			set { hRef = value; }
		}
		
		
		private int NumInfos
		{
			get { return locatorRelationshipInfos.Count; }
		}

		/// <summary>
		/// List sorted by priority and is Prohibited flag
		/// </summary>
		private ArrayList locatorRelationshipInfos = new ArrayList();
		internal ArrayList LocatorRelationshipInfos
		{
			get { return locatorRelationshipInfos; }
			set { locatorRelationshipInfos = value; }

		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="ChildDefinitionLocator"/>.
		/// </summary>
		protected ChildDefinitionLocator()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="ChildDefinitionLocator"/>.
		/// </summary>
		public ChildDefinitionLocator( string hRef,
			LocatorRelationshipInfo info )
		{
			this.hRef = hRef;
			locatorRelationshipInfos.Add( info );
		}

		/// <summary>
		/// Creates a new instance of <see cref="ChildDefinitionLocator"/>, copying
		/// properties from a parameter-supplied <see cref="ChildDefinitionLocator"/>.
		/// </summary>
		/// <param name="cpl">The <see cref="ChildDefinitionLocator"/> from which properties
		/// are to be copied to the new instance of <see cref="ChildDefinitionLocator"/>.</param>
		public ChildDefinitionLocator(ChildDefinitionLocator cpl) 
		{
			hRef = cpl.hRef;

			foreach ( DefinitionLocatorRelationshipInfo lri in cpl.locatorRelationshipInfos )
			{
				locatorRelationshipInfos.Add( lri );
			}
		}

		#endregion


		#region Public methods

		internal bool CanAddRelationship( DefinitionLocatorRelationshipInfo newRelation )
		{
			foreach( DefinitionLocatorRelationshipInfo lri in this.locatorRelationshipInfos )
			{
				if ( lri.Equals( newRelation ) ) 
				{
					return false;
				}
			}

			return true;
		}


		internal void AddRelationship( DefinitionLocatorRelationshipInfo newRelation )
		{
			if ( CanAddRelationship( newRelation ) )
			{
				bool add = true;
				if (locatorRelationshipInfos.Count > 0)
				{
					if (newRelation.NodeType == DimensionNode.NodeType.Item)
					{
						if (newRelation.IsDefault)
						{
							//we maybe just adding the default flag  to an existing relationship.
							add = false;
							foreach (DefinitionLocatorRelationshipInfo lri in this.locatorRelationshipInfos)
							{
								//since default canot be used in the instance document....
								(lri.NodeInfo as DefinitionNodeInfoMember).IsDefault = true;
								(lri.NodeInfo as DefinitionNodeInfoMember).Usable = false;
							}

						}
						else
						{
							//if the list already has a default value then we need to update it
							//with the current one..
							//the default should have all the members other than the order matching.
							for (int i = 0; i < locatorRelationshipInfos.Count; i++)
							{
								DefinitionLocatorRelationshipInfo lri = locatorRelationshipInfos[i] as DefinitionLocatorRelationshipInfo;
								if (lri.IsDefault)
								{
									//update lri
									if (newRelation.IsProhibited == lri.IsProhibited &&
										newRelation.Priority == lri.Priority)
									{
										(newRelation.NodeInfo as DefinitionNodeInfoMember).IsDefault = true;
										(newRelation.NodeInfo as DefinitionNodeInfoMember).Usable = false;
										locatorRelationshipInfos[i] = newRelation;
										add = false;
									}
									else
									{
										//since there is a defaul we need to make this new relationship also a default.
										(newRelation.NodeInfo as DefinitionNodeInfoMember).IsDefault = true;
										(newRelation.NodeInfo as DefinitionNodeInfoMember).Usable = false;

									}

								}
							}
						}
					}

				}

				if (add)
				{
					this.locatorRelationshipInfos.Add(newRelation);
					locatorRelationshipInfos.Sort();

				}
			}
		}

	
		#endregion


		internal ChildDefinitionLocator CreateCopyForMerging()
		{
			ChildDefinitionLocator ret = new ChildDefinitionLocator();
			ret.hRef = this.hRef;
			ret.locatorRelationshipInfos = new ArrayList(this.locatorRelationshipInfos.Count);
			for (int i = 0; i < locatorRelationshipInfos.Count; i++)
			{
				ret.locatorRelationshipInfos.Add(new DefinitionLocatorRelationshipInfo(locatorRelationshipInfos[i] as DefinitionLocatorRelationshipInfo));
			}


			return ret;

		}
	}
}
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
using System.Xml;
using System.Xml.Xsl;

using System.Text;
using System.Reflection;
using System.Globalization;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL presentation link.
	/// </summary>
	[Serializable]
	public class PresentationLink : LinkBase
	{
		#region constants
		private const string ARCROLE_TAG		= "xlink:arcrole";
		private const string ORDER_TAG		= "order";
		private const string USE_TAG			= "use";
		private const string PREFLABEL_TAG	= "preferredLabel";
		private const string WEIGHT_TAG		= "weight";
		private const string PRIORITY_TAG	= "priority";

		private const string PARENT_CHILD_RELATIONSHIP = "parent-child";
		private const string CHILD_PARENT_RELATIONSHIP = "child-parent";
		private const string SUMMATION_ITEM_RELATIONSHIP = "summation-item";

		private const string PROHIBITED_TAG	= "prohibited";

		private const string HEADER_STR		= "PresentationLink";
		#endregion

		#region properties

		private string role = null;

		/// <summary>
		/// The role of this <see cref="PresentationLink"/>.
		/// </summary>
		public string Role
		{
			get { return role; }
		}

		private string title = null;

		/// <summary>
		/// The title of this <see cref="PresentationLink"/>.
		/// </summary>
		public string Title
		{
			get { return title; }
			set { title = value ; }

		}

		/// <summary>
		/// The number of locators associated with this <see cref="PresentationLink"/>.
		/// </summary>
		public int LocatorCount
		{
			get	{ return locators == null ? 0 : locators.Count; }
		}

		/// <summary>
		/// A collection of <see cref="ParserMessage"/> that is the errors that have occurred
		/// while loading and parsing this document.
		/// </summary>
		public ArrayList ErrorList
		{
			get { return errorList; } 
		}

		/// <summary>
		/// key = label
		/// value = arraylist of hrefs
		/// </summary>
		/// <remarks>
		/// This member is available only when we are loading 
		/// a linkbase file and trying to build arcs
		/// after which this member is set to null.
		/// </remarks>
		private Hashtable LabelHrefMapping;

		/// <summary>
		/// Base schema.
		/// </summary>
		public string BaseSchema = null;

		/// <summary>
		/// Href underlying this <see cref="PresentationLink"/>.
		/// </summary>
		public string MyHref = null;

		#endregion

		#region constructors
		/// <summary>
		/// Constructs a new instance of <see cref="PresentationLink"/>.
		/// </summary>
		public PresentationLink() 
		{
			Initialize();
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="PresentationLink"/>.
		/// </summary>
		/// <param name="errorListArg">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="titleArg">A <see cref="String"/> from which the <see cref="Title"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="roleArg">A <see cref="String"/> from which the <see cref="Role"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		public PresentationLink(string titleArg, string roleArg, ArrayList errorListArg)
			: base(errorListArg)
		{
			Initialize();

			title = titleArg;
			role  = roleArg;
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="PresentationLink"/>.
		/// </summary>
		/// <param name="errorListArg">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="titleArg">A <see cref="String"/> from which the <see cref="Title"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="roleArg">A <see cref="String"/> from which the <see cref="Role"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="baseSchema">The base schema to be assigned to the newly created 
		/// <see cref="PresentationLink"/>.</param>
		public PresentationLink(string titleArg, string roleArg, string baseSchema, ArrayList errorListArg)
			: base(errorListArg)
		{
			Initialize();

			BaseSchema = baseSchema;
			title = titleArg;
			role  = roleArg;
		}

		private void Initialize()
		{
			AllocateSpace();
		}

		#endregion

		#region append
		public void Append( PresentationLink arg, ArrayList errors )
		{
			Hashtable tempLocators = new Hashtable();
			if ( arg.locators != null && arg.locators.Count > 0 )
			{
				foreach( DictionaryEntry de in arg.locators )
				{
                    
					PresentationLocator presLocator 
						= locators[de.Key] as PresentationLocator;					
					if ( presLocator != null )
					{
						tempLocators[de.Key] = de.Value;
					}
					else
					{
						
						locators[de.Key] = de.Value;
					}
				}
			}

			// fix up the new locators
			foreach( DictionaryEntry de in tempLocators )
			{
				PresentationLocator presLocator = locators[de.Key] as PresentationLocator;
				PresentationLocator oldLocator = de.Value as PresentationLocator;

				presLocator.Append( oldLocator );

			}

			//preserve the old BaseSchema, so roleRef href will point to the correct schema
			BaseSchema = arg.BaseSchema;
		}
		#endregion

		/// <summary>
		/// Creates and returns a new <see cref="LocatorBase"/>
		/// </summary>
		/// <param name="href">The href to be assigned to the new <see cref="LocatorBase"/>.</param>
		/// <returns>The new <see cref="LocatorBase"/>.  
		/// The underlying type of the returned <see cref="LocatorBase"/> is <see cref="PresentationLocator"/>.
		/// </returns>
		public override LocatorBase CreateLocator(string href)
		{
			return new PresentationLocator( href );
		}

		bool FRTAMessageWritten = false;


		public void LoadChildren( XmlNode parentNode,
			XmlNamespaceManager theManager,
            Dictionary<string, string> discoveredSchemas, string schemaPath,
			out int errorsEncountered )
		{
			errorsEncountered = 0;
			LabelHrefMapping = new Hashtable();

			// first load the locators
			XmlNodeList locatorNodes = parentNode.SelectNodes( "./*[@"+TYPE_TAG+"='"+LOCATOR_TYPE+"']", theManager );
			if ( locatorNodes == null  )
			{
				return;	// no locators, can't do anything
			}

			foreach ( XmlNode locator in locatorNodes )
			{
				if (!LoadLocator(locator, discoveredSchemas, schemaPath ))
				{
					++errorsEncountered;
				}
			}

			// finally load the arcs
			XmlNodeList arcNodes = parentNode.SelectNodes( "./*[@"+TYPE_TAG+"='"+ARC_TYPE+"']", theManager );
			if ( arcNodes != null  )
			{
				foreach ( XmlNode arc in arcNodes )
				{
					if ( !LoadArc( arc ) )
					{
						++errorsEncountered;
					}
				}
			}

			LabelHrefMapping = null;
		}

		private bool LoadLocator(XmlNode locNode,
            Dictionary<string, string> discoveredSchemas, string schemaPath)
		{
			string href = null;
			string label = null;

			if (!Common.GetAttribute(locNode, HREF_TAG, ref href, errorList) ||
				!Common.GetAttribute(locNode, LABEL_TAG, ref label, errorList))
			{
				return false;
			}

			if( string.IsNullOrEmpty( href ) )
				return false;

			LocatorBase locator = CreateLocator(href);

			locator.ParseHRef(errorList);
            LinkBase.AddDiscoveredSchema(schemaPath, locator.Xsd, discoveredSchemas);


			locator.AddLabel(label);


			if (locators.ContainsKey(locator.HRef))
			{
				PresentationLocator oll = (PresentationLocator)locators[locator.HRef];


				// add a new one, pointing to the same locator
				if (!oll.LabelArray.Contains(label))
				{
					oll.AddLabel(label);

				}
			}
			else
			{
				locators[locator.HRef] = locator;
			}

			HybridDictionary hrefList = this.LabelHrefMapping[label] as HybridDictionary;
			if (hrefList == null)
			{
				hrefList = new HybridDictionary();
				this.LabelHrefMapping[label] = hrefList;
			}

			hrefList[locator.HRef] = 1;

			return true;
		}

		public override bool LoadArc( XmlNode node )
		{
			string arcrole = string.Empty;
			string from = string.Empty;
			string to = string.Empty;

			if ( !Common.GetAttribute( node, ARCROLE_TAG, ref arcrole, errorList )	||
				!Common.GetAttribute( node, FROM_TAG, ref from, errorList )		||
				!Common.GetAttribute( node, TO_TAG, ref to, errorList ) )
			{
				return false;
			}

			//default order should be 1 as defined by xbrl spec 2.1 ( section 3.5.3.9.5)
			string order = "1";
			string use = string.Empty;
			string prefLabel = null;
			string weight = string.Empty;
			string priority = "0";

			if ( !Common.GetAttribute( node, ORDER_TAG, ref order, null ) && !FRTAMessageWritten )
			{
				Common.WriteWarning( "XBRLParser.Warning.LinkbaseNotFRTACompliant", errorList, ORDER_TAG );
				FRTAMessageWritten = true;
			}

			Common.GetAttribute( node, USE_TAG, ref use, null );
			Common.GetAttribute( node, PREFLABEL_TAG, ref prefLabel, null );
			Common.GetAttribute( node, PRIORITY_TAG, ref priority, null );
			Common.GetAttribute( node, WEIGHT_TAG, ref weight, null );

			if ( arcrole.IndexOf( CHILD_PARENT_RELATIONSHIP ) > -1  )
			{
			//need to switch from and to
				string tmp = from;
				from = to;
				to = tmp;
			}
          
			HybridDictionary fromhd = LabelHrefMapping[from] as HybridDictionary;
			HybridDictionary tohd = LabelHrefMapping[to] as HybridDictionary;


			if( fromhd == null || tohd == null )
			{
				Common.WriteError( "XBRLParser.Warning.PLocatorParentChildNotFound", errorList, from, to, title );
				return false;

			}
			ArrayList parentLocators = new ArrayList(fromhd.Keys);
			ArrayList childLocators = new ArrayList(tohd.Keys);

           
			

			if ( parentLocators == null || childLocators == null )
			{
				Common.WriteError( "XBRLParser.Warning.PLocatorParentChildNotFound", errorList, from, to, title );
				return false;
			}

			foreach( string parentHref in parentLocators )
			{
				foreach( string childHref in childLocators )
				{
					//need to build the relationshipbetween 
					PresentationLocator parentPl = this.locators[parentHref] as PresentationLocator;
					PresentationLocator childPl = this.locators[childHref] as PresentationLocator;


					if ( parentPl == null || childPl == null )
					{
						Common.WriteError( "XBRLParser.Warning.PLocatorParentChildNotFound", errorList, from, to, title );
						return false;
					}

					float ord = float.Parse( order, NumberFormatInfo.InvariantInfo );
					bool isProhibited = false;
					if ( use == PROHIBITED_TAG )
					{
						isProhibited = true;
					}

					parentPl.AddChild( childPl, to, priority, ord,
						prefLabel, weight, isProhibited);
				}
			}

			return true;
		}

		public Node CreateNode(string lang, string role)
		{
			return CreateNode(lang, role, null  );
		}

		/// <returns>The parent node for this link</returns>
		/// <exception cref="AucentException">throws if this link does not contain any locator objects</exception>
		public Node CreateNode( string lang, string role , Dimension dimension )
		{
			#pragma warning disable 0219
			ArrayList nodeList = new ArrayList( locators.Count +1 );
			#pragma warning restore 0219
			
			Node parent = new Node(title);
			parent.MyPresentationLink = this;

			IDictionaryEnumerator enumer = locators.GetEnumerator();

			while ( enumer.MoveNext() )
			{
				PresentationLocator pl = (PresentationLocator)enumer.Value;
				if ( !pl.HasParent )
				{
					
					// create all the nodes for this presentationLink
					try
					{
						Node childNode = pl.CreateNode(lang, role, 0, 
							pl.href, parent, null, this, true, dimension);

						if ( childNode.Children == null &&
							childNode.MyElement != null && childNode.MyElement.IsHyperCubeItem())
						{
							//we might have decided to not show an hypercube.. as it does not have 
							//any children nodes that are valid segments...
							return null;
							
						}
						//parent.AddChild(childNode);
					}
					catch ( Exception ex )
					{
						string msg = ex.Message;
					}
				}
			}

			return parent;
		}

	

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <exception cref="NotImplementedException">Always thrown.</exception>
		public override bool LoadResource(XmlNode child)
		{
			throw new NotImplementedException( "LoadResource should not be called when parsing Presentation locator elements" );
		}

		/// <summary>
		/// Retrieve a <see cref="PresentationLocator"/> for this <see cref="PresentationLink"/>'s locators 
		/// collection by URI (href).
		/// </summary>
		/// <param name="href">The URI (href) of the locator to be retrieved.</param>
		/// <param name="locator">If return value is true, the retrieved locator.</param>
		/// <returns>True if the requested <see cref="PresentationLocator"/> occurs in this <see cref="PresentationLink"/>'s locators 
		/// collection.</returns>
		public bool TryGetLocator(string href, out PresentationLocator locator)
		{
			locator = null;

			if ( locators == null )
			{
				return false;
			}

			locator = locators[href] as PresentationLocator;
			return locator != null;
		}

		/// <summary>
		/// Retrieves those locators from the locators collection for this <see cref="PresentationLink"/> 
		/// that have a given URI.
		/// </summary>
		/// <param name="href">The URI for which method is to search.</param>
		/// <param name="locatorList">An output parameter.  A collection 
		/// of <see cref="PresentationLocator"/> objects containing those locators matching <paramref name="href"/>.</param>
		/// <returns>True if one or more locators was found.  False otherwise.</returns>
		public bool TryGetLocatorsByHref(string href, out ArrayList locatorList)
		{
			// BUG 1610 - There can be multiple locators with the same href but different labels, 
			// and the element needs to be bound to all of them.
			locatorList = new ArrayList();

			if ( locators == null )
			{
				return false;
			}

			PresentationLocator pl = this.locators[href] as PresentationLocator;
			if( pl != null )
			{
				locatorList.Add( pl );
			}

	
			return locatorList.Count > 0;
		}

		/// <summary>
		/// Retrieves an optional label for a given <see cref=" Node"/>.
		/// </summary>
		/// <param name="elementToMove">The "base" node for which an optional label is 
		/// to be returned.</param>
		/// <param name="elementToMoveParent">The "parent" node for which an optional label 
		/// is to be returned.</param>
		/// <param name="elementToMovePriority">An output parameter.  The priority of any retrieved 
		/// locator label for <paramref name="elementToMoveParent"/>.</param>
		/// <returns><see cref="String.Empty"/> if <paramref name="elementToMove"/> is null.  If 
		/// <paramref name="elementToMoveParent"/> is not null, it locator level.  If a locator can 
		/// be retrieved for <paramref name="elementToMove"/>, its label.  Otherwise, the id of 
		/// <paramref name="elementToMove"/>.</returns>
		public string TryGetOptionalLabelForNode(Node elementToMove, Node elementToMoveParent, 
			ref int elementToMovePriority)
		{
			//find the right label to use for the node
			if (elementToMove == null)
				return string.Empty;

			string elementToMoveLabel = elementToMove.Id;
			
			if (elementToMoveParent != null )
			{
				PresentationLocator parent ;
				if ( TryGetLocator(elementToMoveParent.Id, out parent ) )
				{
					elementToMoveLabel = parent.GetLocatorLabel( elementToMove, ref elementToMovePriority );

					if ( elementToMoveLabel != null )
					{
						return elementToMoveLabel;
					}
				}
			}

			PresentationLocator elementLocator;
			if ( TryGetLocator(elementToMove.Id, out elementLocator ) )
			{
				elementToMoveLabel = elementLocator.Label;
			}

            return  string.IsNullOrEmpty( elementToMoveLabel ) ?  elementToMove.Id : elementToMoveLabel ;
		}

		public bool IsChildProhibited(string parentHref, string childHref, double order )
		{
			PresentationLocator parentPl = this.locators[parentHref] as PresentationLocator;
			if( parentPl != null )
			{
				return parentPl.IsChildProhibited(childHref, order);
			}

			return false;
		}

		/// <summary>
		/// Determines if a <see cref="Node"/> has an optional label.
		/// </summary>
		/// <param name="elementToMove">The "base" node for which an optional label is 
		/// to be reviewed.</param>
		/// <param name="elementToMoveParent">The "parent" node for which an optional label 
		/// is to be reviewed.</param>
		/// <returns>True if an optional label can be located.  False otherwise.</returns>
		/// <remarks>
		/// If <paramref name="elementToMoveParent"/> is not null, its locators will be searched.  
		/// Otherwise, <paramref name="elementToMove"/>'s locators will be searched..  Neither parameter 
		/// may be prohibited.
		/// </remarks>
		public bool NodeHasOptionalLabel(Node elementToMove, Node elementToMoveParent)
		{
			ArrayList elementLabels = new ArrayList();
			
			//check if parentId is null
			if (elementToMoveParent == null || elementToMoveParent.Id == null || 
				elementToMoveParent.Id == string.Empty)
			{
				//elementToMove must be a first level node, since it has no parent
				TryGetLocatorsByHref(elementToMove.Id, out elementLabels);
				if (elementLabels.Count == 0)
				{
					//elementToMove is not in this presentationLink
					return false;
				}
				else
				{
					return !elementToMove.IsProhibited;
				}
			}

			//check each of the node's labels

			if (!elementToMoveParent.IsProhibited && TryGetLocatorsByHref(elementToMove.Id, out elementLabels))
			{
				if (elementLabels.Count > 0 )
				{
					return IsChildProhibited(elementToMoveParent.Id, elementToMove.Id, elementToMove.Order );
				}

				//no prohibited arcs for this parent - just return true as long as there's a label
				return elementLabels.Count > 0;
			}

			//parent is prohibited or there's a problem with the parent or locators
			return false;
		}


		private void ResetHashtables()
		{
			locators = null;
			arcs = null;
			resources = null;

			GC.Collect();

			Initialize();
		}

		/// <summary>
		/// Determines whether the supplied <see cref="Object"/> is equal to this <see cref="PresentationLink"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="PresentationLink"/>.  
		/// Assumed to be a <see cref="PresentationLink"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="PresentationLink"/>.</returns>
		/// <remarks>Two <see cref="PresentationLink"/> objects are considered equal if their roles, compared 
		/// as <see cref="String"/> objects, are equal.</remarks>
		public override bool Equals(object obj)
		{
			return role.Equals( ((PresentationLink)obj).role );
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="PresentationLink"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="PresentationLink"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		

        public void BuildLocatorHashbyHref(Hashtable result)
        {
            if (this.locators == null) return;

            foreach (PresentationLocator pl in this.locators.Values)
            {
                ArrayList locsList = result[pl.HRef] as ArrayList;
                if (locsList == null)
                {
                    locsList = new ArrayList();
                    result[pl.HRef] = locsList;

                }
                locsList.Add(pl);

            }
        }

		#region XML strings
		internal static void WriteHeader( int numTabs, string title, StringBuilder xml )
		{
			for ( int i=0; i < numTabs; ++i )
			{
				xml.Append( "\t" );
			}

			xml.Append( "<" ).Append( HEADER_STR ).Append(" title=\"" ).Append( title ).Append( "\">" ).Append( Environment.NewLine );
		}

		internal static void WriteFooter( int numTabs, StringBuilder xml )
		{
			for ( int i=0; i < numTabs; ++i )
			{
				xml.Append( "\t" );
			}

			xml.Append( "</" ).Append( HEADER_STR ).Append( ">" ).Append( Environment.NewLine );
		}

		#endregion


		internal PresentationLink CreateCopyForMerging()
		{
			PresentationLink ret = new PresentationLink();

			ret.role = this.role;
			ret.title = this.title;

			ret.BaseSchema = this.BaseSchema;
			ret.MyHref = this.MyHref;

			//need to copy the locators

			ret.locators = new Hashtable();
			foreach (DictionaryEntry de in this.locators)
			{
				PresentationLocator orig = de.Value as PresentationLocator;

				PresentationLocator copy = orig.CreateCopyForMerging();



				ret.locators[de.Key] = copy;
			}

			//using the parentnewvalue list we need to reset the parent infotrmationin
			//presentation locators
			foreach (PresentationLocator pl in ret.locators.Values)
			{
				pl.ResetParentInformation(ret.locators);
			}
			return ret;
		}


        /// <summary>
		/// UPDATE PRESENTATION LINK TO CHANGE AN ARC TO PROHIBITED
        /// </summary>
        /// <param name="elementToProhibit"></param>
        /// <param name="ParentNode"></param>
        /// <param name="isPresentation"></param>
        /// <returns></returns>
        public bool ProhibitArc( Node elementToProhibit,
            Node ParentNode, bool isPresentation )
        {
            PresentationLocator parentLocator = locators[ParentNode.Id] as PresentationLocator;

            if (parentLocator == null) return false;

            ChildPresentationLocator cpl = parentLocator.childLocatorsByHRef[elementToProhibit.Id] as ChildPresentationLocator;

            foreach (LocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos)
            {
                if (((double)(lri.Order)).ToString( "####0.####", new CultureInfo( "en-US" ) ).Equals(
					elementToProhibit.order.ToString( "####0.####", new CultureInfo( "en-US" ) )) && 
                    (!isPresentation || lri.PrefLabel == elementToProhibit.PreferredLabel  )&&
					( isPresentation || lri.CalculationWeight == elementToProhibit.CalculationWeight) &&
                    lri.IsProhibited == false)
                {
                    //simply prohibit the link...
                    //when we reload the prohibited link would anyway superseed the optional one
                    lri.IsProhibited = true;
                    break;
                }
            }

            return true;
        }

     
        /// <summary>
        /// Add a new relationship between two locators...
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="parentElementId"></param>
        /// <param name="newLocatorRelationshipInfo"></param>
        /// <param name="taxonomy"></param>
        /// <returns></returns>
        public bool UpdateArc(string elementId, string parentElementId ,
            LocatorRelationshipInfo newLocatorRelationshipInfo, Taxonomy taxonomy  )
        {
			PresentationLocator parentLocator = locators[parentElementId] as PresentationLocator;


			if (parentLocator == null && !string.IsNullOrEmpty(parentElementId))
			{


				parentLocator = new PresentationLocator();
				parentLocator.href = parentElementId;
				parentLocator.MyElement = taxonomy.allElements[parentElementId] as Element;

				locators[parentElementId] = parentLocator;



			}

            PresentationLocator childLocator = locators[elementId] as PresentationLocator;

			if (childLocator == null )
            {
                childLocator = new PresentationLocator();
                childLocator.href = elementId;
                childLocator.MyElement = taxonomy.allElements[elementId] as Element;

				if (parentLocator != null)
				{
					childLocator.AddParent(parentLocator);

				}
                locators[elementId] = childLocator;
            }

			if (parentLocator != null)
			{
				if (parentLocator.childLocatorsByHRef == null)
				{
					parentLocator.childLocatorsByHRef = new HybridDictionary();
				}
				ChildPresentationLocator cpl = parentLocator.childLocatorsByHRef[elementId] as ChildPresentationLocator;
				if (cpl != null)
				{
					cpl.AddRelationship(newLocatorRelationshipInfo);
				}
				else
				{
					cpl = new ChildPresentationLocator(elementId, newLocatorRelationshipInfo);
					parentLocator.childLocatorsByHRef[elementId] = cpl;
				}

			}
			


            return true;


        }



	}
}
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
	/// Represents an XBRL definition link.
	/// </summary>
	[Serializable]
	public class DefinitionLink : LinkBase
	{
		#region constants
		private const string ARCROLE_TAG		= "xlink:arcrole";
		private const string ORDER_TAG		= "order";
		private const string USE_TAG			= "use";
		private const string PREFLABEL_TAG	= "preferredLabel";
		private const string PRIORITY_TAG	= "priority";

		/// <summary>
		/// The XBRL International arc role relationship from a hypercube to a dimension.
		/// </summary>
        public const string HYPERCUBE_DIMENSION_RELATIONSHIP = "hypercube-dimension";

		/// <summary>
		/// The XBRL International arc role relationship from a dimension to a domain.
		/// </summary>
		public const string DIMENSION_DOMAIN_RELATIONSHIP = "dimension-domain";

		/// <summary>
		/// The XBRL International arc role relationship from a domain to a domain member.
		/// </summary>
		public const string DOMAIN_MEMBER_RELATIONSHIP = "domain-member";
		/// <summary>
		///  The XBRL International arc role relationship
		/// </summary>
		public const string ALL_RELATIONSHIP = "all";
		/// <summary>
		///  The XBRL International arc role relationship
		/// </summary>
		public const string NOT_ALL_RELATIONSHIP = "notAll";
		/// <summary>
		///  The XBRL International arc role relationship
		/// </summary>
		public const string DIMENSION_DEFAULT_RELATIONSHIP = "dimension-default";
		private const string REQUIRES_ELEMENT_RELATIONSHIP = "requires-element";

        private const string USABLE_TAG = "xbrldt:usable";
        private const string TARGETROLE_TAG = "xbrldt:targetRole";
        private const string CLOSED_TAG = "xbrldt:closed";
        private const string CONTEXT_TAG = "xbrldt:contextElement";

		private const string PROHIBITED_TAG	= "prohibited";

		private const string HEADER_STR		= "DefinitionLink";
		#endregion

        #region enum

    
        #endregion

        #region properties

        private string role = null;

		/// <summary>
		/// The arc role for this <see cref="DefinitionLink"/>.
		/// </summary>
		public string Role
		{
			get { return role; }
		}

		private string title = null;

		/// <summary>
		/// The title for this <see cref="DefinitionLink"/>/
		/// </summary>
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		/// <summary>
		/// The number of locators associated with this <see cref="DefinitionLink"/>.
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
		/// this member is available only when we are loading 
		/// a linkbase file and trying to build arcs
		/// after which this member is set to null
		/// </summary>
		private Hashtable LabelHrefMapping;

		/// <summary>
		/// BaseSchema
		/// </summary>
		public string BaseSchema = null;

		/// <summary>
		/// HRef for this <see cref="DefinitionLink"/>.
		/// </summary>
		public string MyHref = null;

        /// <summary>
        /// list of all locator hrefs that are hypercube items.
        /// </summary>
		internal List<string> HypercubeLocatorsHrefs = new List<string>();


		/// <summary>
		/// member available only during parse....set to null after we parse....
		/// </summary>
		internal List<string> NotAllRelationshipHypercubes = new List<string>();

		/// <summary>
		/// member available only during parse....set to null after we parse....
		/// list of all dimension href that have defaults....
		/// </summary>
		internal List<string> DimensionHrefsWithDefault = new List<string>();
	
		/// <summary>
		///  member available only during parse....
		/// </summary>
		internal List<string> DimensionLocatorsHrefs = new List<string>();

		
		/// <summary>
		/// List of all locator hrefs that are measure items
		/// i.e. it has hypercube as a child....
		/// </summary>
		internal List<string> MeasureLocatorsHrefs = new List<string>();

		/// <summary>
		/// list of all items that has a requires element relationship..
		/// </summary>
		internal List<string> RequiresElementRelationshipHrefs = new List<string>();


		internal List<string> segmentHypercubesHRef = new List<string>();

		internal List<string> scenarioHypercubesHref = new List<string>();

        /// <summary>
        /// simple list as we do not expect to have too many hypercubes per definitionlink
        /// </summary>
        public List<DimensionNode> HyperCubeNodesList = new List<DimensionNode>();

        internal List<ElementHypercubeRelationhipInfo> ElementHypercubeRelationships;
		#endregion

		#region constructors
		/// <summary>
		/// Constructs a new instance of <see cref="DefinitionLink"/>,
		/// </summary>
		private DefinitionLink() 
		{
			Initialize();
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="DefinitionLink"/>.
		/// </summary>
		/// <param name="errorListArg">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="DefinitionLink"/> is to be initialized.</param>
		/// <param name="titleArg">A <see cref="String"/> from which the <see cref="Title"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="roleArg">A <see cref="String"/> from which the <see cref="Role"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		public DefinitionLink(string titleArg, string roleArg, ArrayList errorListArg)
			: base(errorListArg)
		{
			Initialize();

			title = titleArg;
			role  = roleArg;
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="DefinitionLink"/>.
		/// </summary>
		/// <param name="errorListArg">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="DefinitionLink"/> is to be initialized.</param>
		/// <param name="titleArg">A <see cref="String"/> from which the <see cref="Title"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="roleArg">A <see cref="String"/> from which the <see cref="Role"/> of 
		/// this <see cref="PresentationLink"/> is to be initialized.</param>
		/// <param name="baseSchema">A <see cref="String"/> from which the <see cref="BaseSchema"/> 
		/// of this <see cref="PresentationLink"/> is to be initialized.</param>
		public DefinitionLink(string titleArg, string roleArg, string baseSchema, ArrayList errorListArg)
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
		public void Append( DefinitionLink arg, ArrayList errors )
		{
			Hashtable tempLocators = new Hashtable();
			if ( arg.locators != null && arg.locators.Count > 0 )
			{
				foreach( DictionaryEntry de in arg.locators )
				{
					DefinitionLocator loc 
						= locators[de.Key] as DefinitionLocator;					
					if ( loc != null )
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
				DefinitionLocator loc = locators[de.Key] as DefinitionLocator;
				DefinitionLocator oldLocator = de.Value as DefinitionLocator;

				loc.Append( oldLocator );

			}

			MergeStringList(HypercubeLocatorsHrefs, arg.HypercubeLocatorsHrefs);
			MergeStringList(NotAllRelationshipHypercubes, arg.NotAllRelationshipHypercubes);
			MergeStringList(DimensionHrefsWithDefault, arg.DimensionHrefsWithDefault);
			MergeStringList(DimensionLocatorsHrefs, arg.DimensionLocatorsHrefs);
			MergeStringList(MeasureLocatorsHrefs, arg.MeasureLocatorsHrefs);
			MergeStringList(RequiresElementRelationshipHrefs, arg.RequiresElementRelationshipHrefs);
			MergeStringList(scenarioHypercubesHref, arg.scenarioHypercubesHref);
			MergeStringList(segmentHypercubesHRef, arg.segmentHypercubesHRef);

			//preserve the old BaseSchema, so roleRef href will point to the correct schema
			BaseSchema = arg.BaseSchema;
		}

		/// <summary>
		/// merging two into one
		/// </summary>
		/// <param name="one"></param>
		/// <param name="two"></param>
		private void MergeStringList(List<string> one, List<string> two)
		{
			if (two.Count > 0)
			{

				if (one.Count == 0)
				{
					one.AddRange(two);
				}
				else
				{
					for (int i = 0; i < two.Count; i++)
					{
						if (!one.Contains(two[i]))
						{
							one.Add(two[i]);
						}
					}
				}
			}
		}
		#endregion

		internal bool HasDimensionInfo(bool forSegment, bool commonOnly)
		{
			if (forSegment)
			{

				bool ret = HypercubeLocatorsHrefs.Count - scenarioHypercubesHref.Count - segmentHypercubesHRef.Count > 0 ? true : false;
				if (!commonOnly && !ret)
				{
					ret = HypercubeLocatorsHrefs.Count - scenarioHypercubesHref.Count > 0 ? true : false;
				}

				return ret;
			}
			else
			{
				return scenarioHypercubesHref.Count > 0 ? true : false;
			}
		}

		/// <summary>
		/// Creates a returns a new locator.
		/// </summary>
		/// <param name="href">The URL for the newly created locator.</param>
		/// <returns>A <see cref="LocatorBase"/> that is the newly created locator.</returns>
		public override LocatorBase CreateLocator(string href)
		{
			return new DefinitionLocator( href );
		}

		bool FRTAMessageWritten = false;

		internal void LoadChildren( XmlNode parentNode,
			XmlNamespaceManager theManager,
            Dictionary<string, string> discoveredSchemas, string schemaPath, out int errorsEncountered)
		{
			errorsEncountered = 0;
			LabelHrefMapping = new Hashtable();

			// first load the locators
			XmlNodeList locatorNodes = parentNode.SelectNodes( "./*[@"+TYPE_TAG+"='"+LOCATOR_TYPE+"']", theManager );
			if ( locatorNodes == null  )
			{
				return;	// no locators, can't do anything
			}
			bool foundLoc = false;
			foreach ( XmlNode locator in locatorNodes )
			{
				foundLoc = true;
				if (!LoadLocator(locator,  discoveredSchemas, schemaPath))
				{
					++errorsEncountered;
				}
			}
			if (!foundLoc) return; // no locators, can't do anything
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


		/// <summary>
		/// override the locator creation process as we want to use href as key and not the 
		/// label
		/// </summary>
		private bool LoadLocator(XmlNode locNode,
            Dictionary<string, string> discoveredSchemas, string schemaPath )
		{
			string href = null;
			string label = null;

			if ( !Common.GetAttribute( locNode, HREF_TAG, ref href, errorList ) ||
				!Common.GetAttribute( locNode, LABEL_TAG, ref label, errorList ) )
			{
				return false;
			}

			LocatorBase locator = CreateLocator( href );

			locator.ParseHRef( errorList );
            LinkBase.AddDiscoveredSchema(schemaPath, locator.Xsd, discoveredSchemas);

          
			locator.AddLabel( label );



			if ( locators.ContainsKey( locator.HRef ) )
			{
				LocatorBase oll = (LocatorBase)locators[locator.HRef];


				// add a new one, pointing to the same locator
				if ( !oll.LabelArray.Contains(label) )
				{
					oll.AddLabel( label );

				}
			}
			else
			{
				locators[locator.HRef] = locator;
			}

			HybridDictionary hrefList = this.LabelHrefMapping[ label ] as HybridDictionary;
			if( hrefList == null )
			{
				hrefList = new HybridDictionary();
				this.LabelHrefMapping[ label ] = hrefList;
			}

			hrefList[locator.HRef ] = 1;

			return true;
		}

		/// <summary>
		/// Loads an XLink Definition Link arc into this <see cref="DefinitionLink"/> from 
		/// a parameter-supplied <see cref="XmlNode"/>.
		/// </summary>
		/// <param name="node">The node from which arc information is to be taken.</param>
		/// <returns>True if the arc node could be successfully loaded.  False otherwise.</returns>
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

			string order = "1";
			string use = string.Empty;
			string prefLabel = null;
			string priority = "0";
            string targetRole = null;

			if ( !Common.GetAttribute( node, ORDER_TAG, ref order, null ) && !FRTAMessageWritten )
			{
				Common.WriteWarning( "XBRLParser.Warning.LinkbaseNotFRTACompliant", errorList, ORDER_TAG );
				FRTAMessageWritten = true;
			}

			Common.GetAttribute( node, USE_TAG, ref use, null );
			Common.GetAttribute( node, PREFLABEL_TAG, ref prefLabel, null );
            Common.GetAttribute(node, PRIORITY_TAG, ref priority, null);
            Common.GetAttribute(node, TARGETROLE_TAG, ref targetRole, null);
            if (targetRole != null)
            {
                Console.WriteLine(targetRole);
            }
			

			HybridDictionary fromhd = LabelHrefMapping[from] as HybridDictionary;
			HybridDictionary tohd = LabelHrefMapping[to] as HybridDictionary;


			if( fromhd == null || tohd == null )
			{
				Common.WriteError( "XBRLParser.Warning.DLocatorParentChildNotFound", errorList, from, to, title );
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
					DefinitionLocator parent = this.locators[parentHref] as DefinitionLocator;
					DefinitionLocator child = this.locators[childHref] as DefinitionLocator;

					if ( parent == null || child == null )
					{
						Common.WriteError( "XBRLParser.Warning.DLocatorParentChildNotFound", errorList, from, to, title );
						return false;
					}
					bool? isUsable = new Nullable<bool>();
					bool? isScenario = new Nullable<bool>();
					bool? isAll = new Nullable<bool>();
					bool? isClosed = new Nullable<bool>();

					bool? isDefault = new Nullable<bool>();
					bool? isRequiresElement = new Nullable<bool>();
					DimensionNode.NodeType nodeType = DimensionNode.NodeType.Item;
			

					if (arcrole.IndexOf(HYPERCUBE_DIMENSION_RELATIONSHIP) >= 0 )
					{
						nodeType = DimensionNode.NodeType.Dimension;

						//need to add the parent locator to the hypercube list of the
						//link 
						if (!HypercubeLocatorsHrefs.Contains(parent.HRef))
						{
							HypercubeLocatorsHrefs.Add(parent.HRef);
						}
						DimensionLocatorsHrefs.Add(child.HRef);


					}
					else if (arcrole.IndexOf(DIMENSION_DOMAIN_RELATIONSHIP)>= 0)
					{
						nodeType = DimensionNode.NodeType.Item;
						if (!DimensionLocatorsHrefs.Contains(parent.HRef))
						{
							DimensionLocatorsHrefs.Add(parent.HRef);
						}

					}
					else if (arcrole.IndexOf(DOMAIN_MEMBER_RELATIONSHIP)>= 0)
					{
						nodeType = DimensionNode.NodeType.Item;

					}
					else if (arcrole.IndexOf(ALL_RELATIONSHIP)>= 0)
					{
						isAll = true;
						this.MeasureLocatorsHrefs.Add( parent.HRef );
						if (!HypercubeLocatorsHrefs.Contains(child.HRef))
						{
							HypercubeLocatorsHrefs.Add(child.HRef);
						}
						nodeType = DimensionNode.NodeType.Hypercube;

					}
					else if (arcrole.IndexOf(NOT_ALL_RELATIONSHIP)>= 0)
					{
						isAll = false;
						this.MeasureLocatorsHrefs.Add( parent.HRef );
						if (!HypercubeLocatorsHrefs.Contains(child.HRef))
						{
							HypercubeLocatorsHrefs.Add(child.HRef);
						}
						if (!NotAllRelationshipHypercubes.Contains(child.HRef))
						{
							NotAllRelationshipHypercubes.Add(child.HRef);
						}
						
						nodeType = DimensionNode.NodeType.Hypercube;

					}
					else if (arcrole.IndexOf(DIMENSION_DEFAULT_RELATIONSHIP)>= 0)
					{
						nodeType = DimensionNode.NodeType.Item;
						isDefault = true;

						DimensionHrefsWithDefault.Add(parent.HRef);
						if (!DimensionLocatorsHrefs.Contains(parent.HRef))
						{
							DimensionLocatorsHrefs.Add(parent.HRef); 
						}

					}
					else if (arcrole.IndexOf(REQUIRES_ELEMENT_RELATIONSHIP)>= 0)
					{
						nodeType = DimensionNode.NodeType.Item;
						isRequiresElement = true;
						RequiresElementRelationshipHrefs.Add( parent.HRef );
					}
					else 
					{
						Common.WriteError("XBRLParser.Warning.UnDefinedArcRole", errorList, from, to, arcrole);
						return false;
					}
			

					if (nodeType == DimensionNode.NodeType.Hypercube)
					{
						string tmpStr = "false";
						Common.GetAttribute(node, CLOSED_TAG, ref tmpStr, null);
						if (tmpStr.ToLower() == "true")
						{
							isClosed = true;
						}
						tmpStr = "segment";
						Common.GetAttribute(node, CONTEXT_TAG, ref tmpStr, null);
						if (tmpStr.ToLower() == "scenario")
						{
							this.scenarioHypercubesHref.Add(child.HRef);

							isScenario = true;
						}
						else
						{
							isScenario = false;
							this.segmentHypercubesHRef.Add(child.HRef);

						}

						
					}

					else if (nodeType == DimensionNode.NodeType.Item)
					{
						string tmpStr = "true";
						isUsable = true;
						Common.GetAttribute(node, USABLE_TAG, ref tmpStr, null);
						if (tmpStr.ToLower() == "false")
						{
							isUsable = false;
						}
					}
				
			
					
					float ord = float.Parse( order, NumberFormatInfo.InvariantInfo );
					bool isProhibited = false;
					if ( use == PROHIBITED_TAG )
					{
						isProhibited = true;
					}


					// add the child to the parent
					parent.AddChild( child, ord , prefLabel,
						targetRole, isClosed, isUsable, isAll, isScenario,
						isDefault, isRequiresElement, nodeType, 
						priority, isProhibited );
				}

			}

			return true;
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
		/// Retrieve a <see cref="DefinitionLocator"/> for this <see cref="DefinitionLink"/>'s locators 
		/// collection by URI (href).
		/// </summary>
		/// <param name="href">The URI (href) of the locator to be retrieved.</param>
		/// <param name="locator">If return value is true, the retrieved locator.</param>
		/// <returns>True if the requested <see cref="DefinitionLocator"/> occurs in this <see cref="DefinitionLink"/>'s locators 
		/// collection.</returns>
		public bool TryGetLocator( string href, out DefinitionLocator locator )
		{
			locator = null;

			if ( locators == null )
			{
				return false;
			}

			locator = locators[href] as DefinitionLocator;
			return locator != null;
		}

		/// <summary>
		/// Determines if the locators collection for this <see cref="DefinitionLink"/> 
		/// contains a parameter-supplied URI.
		/// </summary>
		/// <param name="href">The URI for which method is to search.</param>
		/// <returns>True if URI is present in the locators collection for 
		/// this <see cref="DefinitionLink"/>.  False otherwise.</returns>
		public bool HasLocator(string href)
		{
			if (this.locators == null)
			{
				return false;
			}

			return this.locators[href] != null;
		}

		/// <summary>
		/// Retrieves those locators from the locators collection for this <see cref="DefinitionLink"/> 
		/// that have a given URI.
		/// </summary>
		/// <param name="href">The URI for which method is to search.</param>
		/// <param name="locatorList">An output parameter.  A collection 
		/// of <see cref="DefinitionLocator"/> objects containing those locators matching <paramref name="href"/>.</param>
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

			DefinitionLocator pl = this.locators[href] as DefinitionLocator;
			if( pl != null )
			{
				locatorList.Add( pl );
			}

			return locatorList.Count > 0;
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
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="DefinitionLink"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="DefinitionLink"/>.  
		/// Assumed to be a <see cref="DefinitionLink"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="DefinitionLink"/>.</returns>
		/// <remarks>To be equal, the role property of the two <see cref="DefinitionLink"/> objects must be equal.</remarks>
		public override bool Equals(object obj)
		{
			return role.Equals( ((DefinitionLink)obj).role );
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="DefinitionLink"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="DefinitionLink"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		

        internal void BuildLocatorHashbyHref(Hashtable result)
        {
            if (this.locators == null) return;

            foreach (DefinitionLocator pl in this.locators.Values)
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


		internal void GetRequiredElementRelationshipInfo(Dictionary<string, List<string>> relationInfo)
		{
			Hashtable hrefsEvaluated = new Hashtable();
			foreach( string href in RequiresElementRelationshipHrefs )
			{
				if( hrefsEvaluated[href] != null ) continue;

				hrefsEvaluated[href] = 1;


			    DefinitionLocator loc = this.locators[href] as DefinitionLocator;

				List<string> vals;
				if (!relationInfo.TryGetValue(href, out vals))
				{
					vals = new List<string>();
					relationInfo[href] = vals;


				}
				loc.SetRequiredElementRelationshipInfo(ref vals);

			}
		}


		
		

		/// <summary>
		/// based on the parent and child element types in the arc...
		/// we need to decide on the arc type...
		/// </summary>
		/// <param name="parentType"></param>
		/// <param name="childType"></param>
		/// <param name="relInfo"></param>
		/// <returns></returns>
		private static string GetArcRoleType(DimensionNode.NodeType parentType, 
			DimensionNode.NodeType childType ,
			DefinitionLocatorRelationshipInfo relInfo)
		{

			if ( childType == DimensionNode.NodeType.Hypercube )
			{
				//child is a hypercube..
				//so it has to be a all or not all relationship
				if( relInfo.IsAllRelationShip )
				{
					return ALL_RELATIONSHIP;
				}
				else
				{
					return NOT_ALL_RELATIONSHIP;
				}
			}

			if( childType == DimensionNode.NodeType.Dimension )
			{

				return HYPERCUBE_DIMENSION_RELATIONSHIP;

			}

			if( parentType == DimensionNode.NodeType.Dimension && 
				childType == DimensionNode.NodeType.Item )
			{
				if (relInfo.IsDefault)
				{
					return DIMENSION_DEFAULT_RELATIONSHIP;
				}

				return DIMENSION_DOMAIN_RELATIONSHIP;
			}

			

			return DOMAIN_MEMBER_RELATIONSHIP;
		}


		internal DefinitionLink CreateCopyForMerging()
		{
			DefinitionLink ret = new DefinitionLink();
			ret.role = this.role;
			ret.title = this.title;

			ret.BaseSchema = this.BaseSchema;
			ret.MyHref = this.MyHref;


			if (HypercubeLocatorsHrefs != null)
			{
				ret.HypercubeLocatorsHrefs = new List<string>(HypercubeLocatorsHrefs);
			}
			
			if (NotAllRelationshipHypercubes != null)
			{
				ret.NotAllRelationshipHypercubes = new List<string>(NotAllRelationshipHypercubes);
			}
			
			
			if (DimensionHrefsWithDefault != null)
			{
				ret.DimensionHrefsWithDefault = new List<string>(DimensionHrefsWithDefault);
			}
			
			if (DimensionLocatorsHrefs != null)
			{
				ret.DimensionLocatorsHrefs = new List<string>(DimensionLocatorsHrefs);
			}
			
			if (MeasureLocatorsHrefs != null)
			{
				ret.MeasureLocatorsHrefs = new List<string>(MeasureLocatorsHrefs);
			}
			
			if (RequiresElementRelationshipHrefs != null)
			{
				ret.RequiresElementRelationshipHrefs = new List<string>(RequiresElementRelationshipHrefs);
			}
			
			if (segmentHypercubesHRef != null)
			{
				ret.segmentHypercubesHRef = new List<string>(segmentHypercubesHRef);
			}
			
			if (scenarioHypercubesHref != null)
			{
				ret.scenarioHypercubesHref = new List<string>(scenarioHypercubesHref);
			}
			
			

			if (this.locators != null)
			{
				ret.locators = new Hashtable();

				foreach (DictionaryEntry de in this.locators)
				{
					DefinitionLocator dl = de.Value as DefinitionLocator;

					#pragma warning disable 0219
					DefinitionLocator copy = dl.CreateCopyForMerging();
					#pragma warning restore 0219

				}


				//using the parentnewvalue list we need to reset the parent infotrmationin
				//definition locators
				foreach (DefinitionLocator dl in ret.locators.Values)
				{
					dl.ResetParentInformation(ret.locators);
				}
			}

			return ret;
		}

        public bool ProhibitArc(string parentElementId,
            string childElementId, string order )
        {
            //the cache of dimension node is not clean so clear it..
            this.HyperCubeNodesList.Clear();



            DefinitionLocator parentLocator = locators[parentElementId] as DefinitionLocator;

            if (parentLocator == null) return false;

            ChildDefinitionLocator cpl = parentLocator.childLocatorsByHRef[childElementId] as ChildDefinitionLocator;
            if (cpl != null)
            {
                foreach (DefinitionLocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos)
                {

                    if (((double)(lri.Order)).ToString("####0.####", new CultureInfo("en-US")).Equals(order)
                        && !lri.IsProhibited)
                    {
                        //simply prohibit the link...
                        //when we reload the prohibited link would anyway superseed the optional one
                        lri.IsProhibited = true;
                    }
                }


                //if it is a axis then we need to remove it from the axis collection
                this.DimensionHrefsWithDefault.Remove(childElementId);
                this.DimensionLocatorsHrefs.Remove(childElementId);


                //if it is a hypercube
                this.HypercubeLocatorsHrefs.Remove(childElementId);
            }


            return true;
        }

        /// <summary>
        /// UPDATE DEFINITION LINK TO CHANGE AN ARC TO PROHIBITED
        /// </summary>
        /// <param name="elementToProhibit"></param>
        /// <param name="ParentNode"></param>
        /// <returns></returns>
        public bool ProhibitArc(DimensionNode elementToProhibit,
            Node ParentNode)
        {
            //the cache of dimension node is not clean so clear it..
            this.HyperCubeNodesList.Clear();



            DefinitionLocator parentLocator = locators[ParentNode.Id] as DefinitionLocator;
            
            if (parentLocator == null) return false;

            ChildDefinitionLocator cpl = parentLocator.childLocatorsByHRef[elementToProhibit.Id] as ChildDefinitionLocator;
            if (cpl != null)
            {
                foreach (DefinitionLocatorRelationshipInfo lri in cpl.LocatorRelationshipInfos)
                {

                    if (((double)(lri.Order)).ToString("####0.####", new CultureInfo("en-US")).Equals(
                            elementToProhibit.order.ToString("####0.####", new CultureInfo("en-US"))) &&
                            lri.IsProhibited == false)
                    {
                        //simply prohibit the link...
                        //when we reload the prohibited link would anyway superseed the optional one
                        lri.IsProhibited = true;
                    }
                }

                //if it is a axis then we need to remove it from the axis collection
                this.DimensionHrefsWithDefault.Remove(elementToProhibit.Id);
                this.DimensionLocatorsHrefs.Remove(elementToProhibit.Id);


                //if it is a hypercube
                this.HypercubeLocatorsHrefs.Remove(elementToProhibit.Id);

            }


            

            

            return true;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hypercubeId"></param>
		/// <returns></returns>
		public DimensionNode GetHypercubeNode(string hypercubeId)
		{
			lock (this)
			{
				foreach (DimensionNode dn in HyperCubeNodesList)
				{
					if (dn.Id.Equals(hypercubeId))
					{
						return dn;
					}
				}

			}

			return null;

		}


        #region Build get Hypercube Dimension Member hierarchy
        public  bool TryGetHypercubeNode(string currentLanguage, string currentLabelRole,
            Hashtable definitionLinks, 
            string hypercubeId,
            bool buildnew, out DimensionNode hypercubeNode)
        {
            lock (this)
            {
                if (!buildnew)
                {
                    foreach (DimensionNode dn in HyperCubeNodesList)
                    {
                        if (dn.Id.Equals(hypercubeId))
                        {
                            hypercubeNode = dn;
                            return true;
                        }
                    }
                }

                hypercubeNode = null;
                DefinitionLocator dloc;
                if (!this.TryGetLocator(hypercubeId, out dloc)) return false;


                hypercubeNode = dloc.CreateDimensionNode(currentLanguage, currentLabelRole,
				   null, hypercubeId, this, true, definitionLinks, null, null, true, IsScenarioHypercube(hypercubeId, definitionLinks ) );

                if (hypercubeNode != null)
                {


                    hypercubeNode.NodeDimensionInfo = new DefinitionLocatorRelationshipInfo( DimensionNode.NodeType.Hypercube);

                    if (!buildnew)
                    {
                        HyperCubeNodesList.Add(hypercubeNode);

                    }



                }

            }

            return hypercubeNode != null;

        }


		private bool IsScenarioHypercube(string hypercubeId, Hashtable definitionLinks)
		{
			if( this.segmentHypercubesHRef.Contains( hypercubeId )) return false ;
			if( this.scenarioHypercubesHref.Contains( hypercubeId )) return true ;

			bool hasScenario = false;
			bool hassegment = false;
			if (definitionLinks != null)
			{
				foreach (DefinitionLink dl in definitionLinks.Values)
				{
					if (dl.segmentHypercubesHRef.Contains(hypercubeId)) return false;
					if (dl.scenarioHypercubesHref.Contains(hypercubeId)) return true;


					hasScenario = hasScenario || dl.scenarioHypercubesHref.Count > 0;
					hassegment = hassegment || dl.segmentHypercubesHRef.Count > 0;

				}
			}

			//okit is defined neither as a segment or scenario..
			if (hasScenario && !hassegment)
			{
				//the tax has a few scenarios and no segments...
				// so it is better off treating this as a scenario....
				return true;
			}



			return false;


		}

        #endregion


        #region Helper methods
        internal bool HasDefaultDefined(string dimHRef )
        {
            return this.DimensionHrefsWithDefault.Contains(dimHRef);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tax"></param>
		/// <param name="hypercubeId"></param>
		/// <param name="buildHyperubeChildren"></param>
		/// <param name="dimNodes"></param>
		public void BuildMeasureElementTreeForHypercubeId(Taxonomy tax, string hypercubeId,
		bool buildHyperubeChildren, out List<DimensionNode> dimNodes)
		{
			dimNodes = new List<DimensionNode>();
			string curLang = tax.currentLanguage;
			string curLabelRole = tax.currentLabelRole;

			if (curLang == null)
			{
				curLang = "en";

			}

			if (curLabelRole == null)
			{
				curLabelRole = PresentationLocator.preferredLabelRole;
			}

			//determine all the measure element parents of the hypercube element id

			foreach (string measureId in this.MeasureLocatorsHrefs)
			{
				DefinitionLocator dloc;
				if (!this.TryGetLocator(measureId, out dloc)) continue;

				if (dloc.childLocatorsByHRef == null) continue;

				bool add = false;
				foreach (string key in dloc.childLocatorsByHRef.Keys)
				{
					if (key.Equals(hypercubeId))
					{
						ChildDefinitionLocator lri = dloc.childLocatorsByHRef[hypercubeId] as ChildDefinitionLocator;
						foreach (DefinitionLocatorRelationshipInfo dri in lri.LocatorRelationshipInfos)
						{
							if (!dri.IsProhibited)
							{

								add = true;

							

							}

						}
						break;
		
					}
				}


				if (add )
				{
					DefinitionLocatorRelationshipInfo parentDRI = new DefinitionLocatorRelationshipInfo(DimensionNode.NodeType.Item);


					DimensionNode dimNode = dloc.CreateDimensionNode(curLang, curLabelRole,
					null,	measureId, this, true, tax.NetDefinisionInfo.DefinitionLinks, 
					parentDRI, null, buildHyperubeChildren,
					IsScenarioHypercube(hypercubeId, tax.NetDefinisionInfo.DefinitionLinks));

					if (dimNode != null)
					{
						dimNodes.Add(dimNode);
					}

				}
			}







			return;
		}

        /// <summary>
        /// Build the element hypercube relationship info to be able to validate
        /// markups
        /// </summary>
        /// <param name="tax"></param>
        /// <param name="definisionLinks"></param>
        internal void BuildDimensionValidationInformation( 
            Taxonomy tax, Hashtable definisionLinks  )
        {
            if (ElementHypercubeRelationships != null) return;

            ElementHypercubeRelationships = new List<ElementHypercubeRelationhipInfo>();

            if (MeasureLocatorsHrefs == null || MeasureLocatorsHrefs.Count == 0 ) return;

            foreach (string measureId in MeasureLocatorsHrefs)
            {
                DefinitionLocator dloc;
                if (!this.TryGetLocator(measureId, out dloc)) continue;

                DimensionNode measureNode = 
                dloc.CreateDimensionNode(tax.currentLanguage, tax.currentLabelRole,
				   null, measureId, this, true, definisionLinks, null, null, false, false);

                if (measureNode.children != null)
                {
                    List<string> memberList = new List<string>();
                    //this list would contain the hypercubes as well ,but we will remove it...
                    //later
                    RecursivelyBuildMemberList(measureNode, ref memberList);
                    //build list of children measures...
                    foreach (DimensionNode dn in measureNode.children)
                    {
                        DefinitionLink targetLink = this;
                        if (dn.NodeDimensionInfo.NodeType == DimensionNode.NodeType.Hypercube)
                        {
                            memberList.Remove(dn.Id);

                            if (!string.IsNullOrEmpty(dn.NodeDimensionInfo.TargetRole))
                            {
                                targetLink = definisionLinks[dn.NodeDimensionInfo.TargetRole] as DefinitionLink;
                                if (targetLink == null)
                                {
                                    targetLink = this;
                                }
                            }

                            //build the hypercube with its dimension children separately..
                            //as this might have got built earlier when the tax object was used
                            //or if multiple element uses the same hypercube... this does not get 
                            //rebuilt every time the hypercube is used...
                            DimensionNode hypercubeNode;
                            if (targetLink.TryGetHypercubeNode(tax.currentLanguage,
                                tax.currentLabelRole, definisionLinks, dn.Id,false, out hypercubeNode))
                            {
                                ElementHypercubeRelationhipInfo info = new ElementHypercubeRelationhipInfo();
                                info.IsAll = dn.NodeDimensionInfo.IsAllRelationShip;
                                info.IsClosed = dn.NodeDimensionInfo.IsClosed;
                                info.IsSegment = !dn.NodeDimensionInfo.IsScenario;
                                info.ParentBaseSet = this;
								info.ParentTaxonomyObj = tax;
                                info.HypercubeId = dn.Id;
                                info.ElementIdList = memberList;

                                if (hypercubeNode.children != null)
                                {
                                    foreach (DimensionNode dimNode in hypercubeNode.children)
                                    {
                                        info.DimensionsById[dimNode.Id] = dimNode;
                                    }
                                }

                                ElementHypercubeRelationships.Add(info);

                            }
                        }
                    }

                }

            }
        }

        private void RecursivelyBuildMemberList(DimensionNode dn, ref List<string> members )
        {
            members.Add(dn.Id);
            if (dn.children != null)
            {
                foreach (DimensionNode child in dn.children)
                {
                    RecursivelyBuildMemberList(child, ref members);
                }

            }
        }

        /// <summary>
        /// Is a given markup element valid in this definition linkbase
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="segments"></param>
        /// <param name="scenarios"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal ElementHypercubeRelationhipInfo.ValidationStatus GetMarkupValidationStatus(
            string elementId , ArrayList segments, ArrayList scenarios, out string error )
        {
            error = null;
            if (ElementHypercubeRelationships == null)
            {
                throw new ApplicationException("Build ElementHypercubeRelationships before calling validate");
            }
            ElementHypercubeRelationhipInfo.ValidationStatus ret = ElementHypercubeRelationhipInfo.ValidationStatus.None;
            string errorFound = null;
            foreach (ElementHypercubeRelationhipInfo info in this.ElementHypercubeRelationships)
            {
                ElementHypercubeRelationhipInfo.ValidationStatus tmp = info.ValidateMarkup(
                    elementId,segments,scenarios,  out error);

				if (tmp == ElementHypercubeRelationhipInfo.ValidationStatus.Valid)
				{
					return tmp; //even if one of them is valid then it is good enough....
			
				}
				else if (tmp == ElementHypercubeRelationhipInfo.ValidationStatus.NotValid)
                {
                    errorFound = error;
					ret = tmp;
                }

            }
            if (ret == ElementHypercubeRelationhipInfo.ValidationStatus.NotValid)
            {
                error = errorFound;
            }
            return ret;
        }


        internal bool HasDimensionWithoutDefault()
        {
            return this.DimensionLocatorsHrefs.Count != this.DimensionHrefsWithDefault.Count;
        }

		internal bool TrySetDefaultMember(string dim, string defMember)
		{

			 DefinitionLocator dl = this.locators[dim] as DefinitionLocator;

			 if (dl != null)
			 {
				 List<string> locsChecked = new List<string>();
				 DefinitionLocator parentDL =
					 RecursivelyGetParentLocatorForMember(dl, ref locsChecked, defMember);

				 if (parentDL != null)
				 {

					 foreach (ChildDefinitionLocator cpl in parentDL.childLocatorsByHRef.Values)
					 {
						 if (cpl.HRef.Equals(defMember))
						 {


							 foreach (DefinitionLocatorRelationshipInfo dri in cpl.LocatorRelationshipInfos)
							 {
								 if (!dri.IsProhibited)
								 {
									 (dri.NodeInfo as DefinitionNodeInfoMember).IsDefault = true;
									 (dri.NodeInfo as DefinitionNodeInfoMember).Usable = false;
								 }

							 }

							 return true;
						 }

					 }

				 }

				 //member is not a immediate child of the dim..
				 //get the parent of the member
			 }

			 


			return false;
		}

		DefinitionLocator RecursivelyGetParentLocatorForMember(DefinitionLocator currentDL,
			ref List<string> locsChecked, 
			string childId )
		{
			if (locsChecked.Contains(currentDL.href)) return null;
			//to avoid recursion
			locsChecked.Add(currentDL.href);
			if (currentDL.childLocatorsByHRef != null)
			{
				foreach (ChildDefinitionLocator cpl in currentDL.childLocatorsByHRef.Values)
				{
					bool useChild = false;
					foreach (DefinitionLocatorRelationshipInfo dri in cpl.LocatorRelationshipInfos)
					{
						if (!dri.IsProhibited)
						{
							useChild = true;
						}

					}
					if (!useChild) continue;

					if (cpl.HRef.Equals(childId))
					{

						return currentDL;

					}
					DefinitionLocator childDL = this.locators[cpl.HRef] as DefinitionLocator;
					if (childDL != null)
					{

						DefinitionLocator ret = RecursivelyGetParentLocatorForMember(childDL, ref locsChecked,
							childId);
						if (ret != null) return ret;
					}





				}
			}

				 
			 


			 return null;
			
		}

		internal void SetDimensionDefaultMember(ref Dictionary<string, List<string>> DefaultMemberByDimension)
        {
            foreach (string dimHRef in this.DimensionHrefsWithDefault)
            {
                DefinitionLocator dl = this.locators[dimHRef] as DefinitionLocator;

                if (dl != null)
                {
                    foreach (ChildDefinitionLocator cpl in dl.childLocatorsByHRef.Values)
                    {
                        bool found = false;
                        foreach (DefinitionLocatorRelationshipInfo dri in cpl.LocatorRelationshipInfos)
                        {
                            if (dri.IsDefault && !dri.IsProhibited)
                            {
								List<string> defs;
								if (!DefaultMemberByDimension.TryGetValue(dimHRef, out defs))
								{
									defs = new List<string>();
									DefaultMemberByDimension[dimHRef] = defs;
								}

								if (!defs.Contains(cpl.HRef))
								{
									defs.Add(cpl.HRef);
								}
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }
                }
            }
        }




        public bool UpdateOptionalArc(string elementId, string parentElementId,
    DefinitionLocatorRelationshipInfo newLocatorRelationshipInfo, Taxonomy taxonomy)
        {
            DefinitionLocator parentLocator = locators[parentElementId] as DefinitionLocator;
            newLocatorRelationshipInfo.IsProhibited = false;
            newLocatorRelationshipInfo.OrigOrder = newLocatorRelationshipInfo.Order;

            if (parentLocator == null && !string.IsNullOrEmpty(parentElementId))
            {


                parentLocator = new DefinitionLocator();
                parentLocator.href = parentElementId;
                parentLocator.MyElement = taxonomy.allElements[parentElementId] as Element;

                locators[parentElementId] = parentLocator;



            }

            DefinitionLocator childLocator = locators[elementId] as DefinitionLocator;

            if (childLocator == null)
            {
                childLocator = new DefinitionLocator();
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
                ChildDefinitionLocator cpl 
                    = parentLocator.childLocatorsByHRef[elementId] as ChildDefinitionLocator;
                if (cpl != null)
                {
                    cpl.AddRelationship(newLocatorRelationshipInfo);
                }
                else
                {
                    cpl = new ChildDefinitionLocator(elementId, newLocatorRelationshipInfo);
                    parentLocator.childLocatorsByHRef[elementId] = cpl;
                }

            }



            return true;


        }

       
        #endregion



		
    }
}
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL linkbase.
	/// </summary>
	[Serializable]
	public abstract class LinkBase
	{
		internal const string TYPE_TAG = "xlink:type";

		internal const string LOCATOR_TYPE = "locator";
		internal const string ARC_TYPE = "arc";
		internal const string RES_TYPE = "resource";

		internal const string TITLE_TAG = "xlink:title";
		internal const string LABEL_TAG = "xlink:label";
		internal const string REFERENCE_TAG = "xlink:reference";
		internal const string HREF_TAG = "xlink:href";

		internal const string FROM_TAG = "xlink:from";
		internal const string TO_TAG = "xlink:to";

		#region properties
		/// <key>label</key>
		/// <value>labelLocator objects</value>
		protected Hashtable locators;

		/// <key>to label</key>
		/// <value>labelLocator object</value>
		protected Hashtable arcs;

		/// <key>to label</key>
		/// <value>TempLabelData object</value>
		protected Hashtable resources;

		internal ArrayList errorList;

		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="LinkBase"/>.
		/// </summary>
		protected LinkBase()
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="LinkBase"/>.
		/// </summary>
		/// <param name="errors">A collection to be used to initialize the errorList 
		/// property of the new <see cref="LinkBase"/>.</param>
		public LinkBase(ArrayList errors)
		{
			errorList = errors;
		}
		#endregion

		internal virtual void AllocateSpace()
		{
			if (locators == null)
			{
				locators = new Hashtable();
				arcs = new Hashtable();
				resources = new Hashtable();
			}
		}

		public virtual void LoadChildren(XmlNode parentNode, XmlNamespaceManager theManager, out int errorsEncountered)
		{
			errorsEncountered = 0;

			// first load the locators
			XmlNodeList locatorNodes = parentNode.SelectNodes("./*[@" + TYPE_TAG + "='" + LOCATOR_TYPE + "']", theManager);
            //don't use XmlNodeList.Count as it's very slow just do the count in the next loop and if 0 then return from the
            //foreach loop below
            //http://hashlist.blogspot.com/2009/03/c-xmlnodelist-iteration.html
            // || locatorNodes.Count == 0)
			if (locatorNodes == null)
			{
				return;	// no locators, can't do anything
			}

		    int locatorNodesCount = 0;

			foreach (XmlNode locator in locatorNodes)
			{
				if (!LoadLocator(locator))
				{
					++errorsEncountered;
				}
			    ++locatorNodesCount;
			}

            if(locatorNodesCount == 0)
            {
                // no locators, can't do anything
                return; 
            }

			// finally load the arcs
			XmlNodeList arcNodes = parentNode.SelectNodes("./*[@" + TYPE_TAG + "='" + ARC_TYPE + "']", theManager);
			if (arcNodes != null )
			{
				foreach (XmlNode arc in arcNodes)
				{
					if (!LoadArc(arc))
					{
						++errorsEncountered;
					}
				}
			}
			// then the resources - if they exist
			XmlNodeList resourceNodes = parentNode.SelectNodes("./*[@" + TYPE_TAG + "='" + RES_TYPE + "']", theManager);
			if (resourceNodes != null )
			{

				foreach (XmlNode resource in resourceNodes)
				{
					if (!LoadResource(resource))
					{
						++errorsEncountered;
					}
				}
			}

		}

		internal static void AddDiscoveredSchema(string linkbaseFilePath, 
            string discoveredXSD, Dictionary<string, string> discoveredSchemas )
		{
            if (!discoveredSchemas.ContainsKey(discoveredXSD ))
			{
                string convertedFileName = discoveredXSD;
                //using the linkbasefilePath.. we might need to get the discovered xsd path.
                if (Directory.Exists(linkbaseFilePath))
                {
					if (discoveredXSD.StartsWith("http", StringComparison.OrdinalIgnoreCase))
					{
						convertedFileName = discoveredXSD;

					}
					else
					{
						convertedFileName = linkbaseFilePath + Path.DirectorySeparatorChar + discoveredXSD;
					}

                }
                else if(  linkbaseFilePath.ToLower().StartsWith( "http"))
                {
					if (discoveredXSD.StartsWith("http", StringComparison.OrdinalIgnoreCase))
					{
						convertedFileName =  discoveredXSD;

					}
					else
					{
						if (linkbaseFilePath.EndsWith("/"))
						{
							convertedFileName = linkbaseFilePath  + discoveredXSD;

						}
						else
						{
							convertedFileName = linkbaseFilePath + @"/" + discoveredXSD;
						}
					}

                }
                discoveredSchemas.Add(discoveredXSD, convertedFileName);
			}
		}


		public virtual bool LoadLocator(XmlNode locNode)
		{
			string href = null;
			string label = null;

			if (!Common.GetAttribute(locNode, HREF_TAG, ref href, errorList) ||
				 !Common.GetAttribute(locNode, LABEL_TAG, ref label, errorList))
			{
				return false;
			}

			LocatorBase locator = CreateLocator(href);

			locator.ParseHRef(errorList);
			locator.AddLabel(label);

			LocatorBase oll = null;

			if (locators.ContainsKey(label))
			{
				oll = (LocatorBase)locators[label];

				// add a new one, pointing to the same locator
				if (!oll.LabelArray.Contains(label))
				{
					// BUG 1610 - create a new presentation locator, since two arcs could have different
					//		priorities.
					PresentationLocator newPL = new PresentationLocator();
					newPL.HRef = oll.HRef;
					newPL.UnpartitionedHref = oll.UnpartitionedHref;
					newPL.Xsd = oll.Xsd;

					// Add the label to the new presentation locator.  This will differentiate the two locators.
					newPL.AddLabel(label);

					locators[label] = newPL;
				}
			}
			else
			{
				locators[label] = locator;
			}

			return true;
		}

		public virtual bool LoadArc(XmlNode child)
		{
			string from = string.Empty;
			string to = string.Empty;

			if (!Common.GetAttribute(child, FROM_TAG, ref from, errorList) ||
				 !Common.GetAttribute(child, TO_TAG, ref to, errorList))
			{
				return false;
			}

			LocatorBase ll = locators[from] as LocatorBase;
			if (ll != null)
			{

				ArrayList locs = arcs[to] as ArrayList;
				if (locs == null)
				{
					locs = new ArrayList();
					arcs[to] = locs;
				}
				locs.Add(ll);


			}


			return true;
		}

		internal void OnParseComplete()
		{
			//since this is used only during the parse process...
			this.arcs = null;
			this.resources = null;
		}

		internal virtual void UpdateResources(LocatorBase locator, string to)
		{
		}

		public abstract LocatorBase CreateLocator(string href);
		public abstract bool LoadResource(XmlNode child);
	}
}
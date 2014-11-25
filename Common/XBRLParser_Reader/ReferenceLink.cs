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

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL reference link.
	/// </summary>
	[Serializable]
	internal class ReferenceLink  : LinkBase
	{
		#region properties

		internal Hashtable References
		{
			get { return locators; }
		}

		#endregion

		#region constructors
		/// <summary>
		/// Constructs a new instance of <see cref="ReferenceLink"/>.
		/// </summary>
		public ReferenceLink()
		{
		}

		/// <summary>
		/// Overloaded. Constructs a new instance of <see cref="ReferenceLink"/>.
		/// </summary>
		/// <param name="errorList">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="ReferenceLink"/> is to be initialized.</param>
		public ReferenceLink(ArrayList errorList)
			: base(errorList)
		{
		}

		#endregion

		internal void ParseLinks( XmlNode parent, XmlNamespaceManager theManager, out int numErrors )
		{
			numErrors = 0;
			AllocateSpace();
			LoadChildren( parent, theManager, out numErrors );
		}

		public override LocatorBase CreateLocator( string href )
		{
			return new ReferenceLocator( href, false );
		}

		public override bool LoadResource( XmlNode child )
		{
			//Find the ReferenceLocator that contains the element label
			string label = "";
			if ( !Common.GetAttribute( child, LABEL_TAG, ref label, errorList ))
			{
				return false;
			}
			//if ((child.Attributes["xlink:role"]) == null)
			//{
			//    Common.WriteError("XBRLParser.Warning.NoTagForLocator2", this.errorList, "xlink:role", child.OuterXml);
			//    return false;
			//}
			ArrayList rls = arcs[label] as ArrayList;

			if (rls != null)
			{
				foreach (ReferenceLocator rl in rls)
				{
					rl.AddInformation(child);

				}
			}
			return true;
		}

		internal override void UpdateResources( LocatorBase locator, string to)
		{
			ReferenceLocator data = resources[ to ] as ReferenceLocator;

			((ReferenceLocator)locator).AddInformation( data );

			//resources.Remove( to );
		}
	}
}
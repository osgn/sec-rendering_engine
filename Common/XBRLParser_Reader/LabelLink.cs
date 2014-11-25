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
	/// Represents an XBRL label link.
	/// </summary>
	[Serializable]
	public class LabelLink : LinkBase
	{
		#region TempLabelData class
		private class TempLabelData
		{
			internal string label;
			internal string language;
			internal string role;
			internal string labelData;

			/// <summary>
			/// Constructs a new instance of <see cref="LabelLink"/>.
			/// </summary>
			/// <param name="label">The label property of the new <see cref="LabelLink"/>.</param>
			/// <param name="language">The language property of the new <see cref="LabelLink"/>.</param>
			/// <param name="role">The role property of the new <see cref="LabelLink"/>.</param>
			/// <param name="labelData">The labeldata property of the new <see cref="LabelLink"/>.</param>
			public TempLabelData( string label, string language, string role, string labelData )
			{
				this.label		= label;
				this.language	= language;
				this.role		= role;
				this.labelData	= labelData;
			}
		}
		#endregion

		#region properties

		internal Hashtable Labels
		{
			get { return locators; }
		}
		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="LabelLink"/>.
		/// </summary>
		protected LabelLink()
		{
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="LabelLink"/>.
		/// </summary>
		/// <param name="errorList">An <see cref="ArrayList"/> from which the 
		/// error list for this <see cref="LabelLink"/> is to be initialized.</param>
		public LabelLink(ArrayList errorList)
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
			return new LabelLocator( href );
		}

		public override bool LoadResource( XmlNode child )
		{
			string role = "http://www.xbrl.org/2003/role/label";
			string label = string.Empty;
			string lang = string.Empty;

			if ( !Common.GetAttribute( child, LABEL_TAG, ref label, errorList )	||
				!Common.GetAttribute( child, Label.LANG_ATTR, ref lang, errorList ) )
			{
				return false;
			}

			Common.GetAttribute(child, Label.ROLE_ATTR, ref role, null);

			string labelRole = role.Substring(role.LastIndexOf('/') + 1);

			ArrayList lls = arcs[label] as ArrayList;

			if (lls != null)
			{
				foreach (LabelLocator ll in lls)
				{
					try
					{
						ll.AddLabel(lang, labelRole, child.InnerText);
					}
					catch (ArgumentException)
					{
						// label aready exists
						Common.WriteWarning("XBRLParser.Warning.LabelRoleAlreadyExists", errorList, ll.Label, labelRole, lang, child.InnerText);
					}

				}
			}
			else
			{
				// store it away
				TempLabelData tld = new TempLabelData( label, lang, labelRole, child.InnerText );

				resources[label] = tld;
				return true;
			}
	
		
			return true;
		}

		internal override void UpdateResources( LocatorBase locator, string to)
		{
			TempLabelData tld = resources[ to ] as TempLabelData;

			((LabelLocator)locator).AddLabel( tld.language, tld.role, tld.labelData );

			resources.Remove( to );
		}
	}
}
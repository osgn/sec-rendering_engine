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

using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XML schema definition enumeration.
	/// </summary>
	[Serializable]
	public class Enumeration
	{
		/// <summary>
		/// The name of the enumeration.
		/// </summary>
        public string Name;

		/// <summary>
		/// The type underlying the enumeration (e.g., "xbrli:stringItemType").
		/// </summary>
		public string RestrictionType;

		/// <summary>
		/// The values underlying this <see cref="Enumeration"/>.
		/// </summary>
		public ArrayList Values = new ArrayList();

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Enumeration"/>.
		/// </summary>
		public Enumeration()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Enumeration"/>.
		/// </summary>
		public Enumeration(string name)
		{
			Name = name;
		}
		#endregion

		/// <summary>
		/// Creates and returns a new <see cref="Enumeration"/> of a Boolean type.
		/// </summary>
		/// <param name="name">The name to be assigned to the created <see cref="Enumeration"/>.</param>
		/// <returns>The newly created <see cref="Enumeration"/>.</returns>
		public static Enumeration CreateBooleanEnum( string name )
		{
			Enumeration enumer = new Enumeration( name );

            //US 22336: "Yes" and "No" are not actually valid values for Boolean within the XBRL spec:
            //http://www.schemacentral.com/sc/xbrl21/t-xbrli_booleanItemType.html.  However, given that users tend to think of
            //booleans as "Yes" and "No" validation has been updated to handle "Yes" and "No" values.
			//enumer.Values.Add( StringResourceUtility.GetString( "XBRLParser.BooleanEnumeration.Yes" ) );
			//enumer.Values.Add( StringResourceUtility.GetString( "XBRLParser.BooleanEnumeration.No" ) );
            enumer.Values.Add( StringResourceUtility.GetString( "XBRLParser.BooleanEnumeration.True" ) );
            enumer.Values.Add( StringResourceUtility.GetString( "XBRLParser.BooleanEnumeration.False" ) );

			return enumer;
		}
	}
}
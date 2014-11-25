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

namespace Aucent.MAX.AXE.XBRLParser.Interfaces
{
	/// <summary>
	/// Defines the methods and properties that must be implemented by a class
	/// to which other objects can be linked.
	/// </summary>
	public interface ILinkable
	{
		/// <summary>
		/// The key of the object.  How it is identified.
		/// </summary>
		string MyKey
		{
			get;
		}

		/// <summary>
		/// The object that supports linking to/from this <see cref="ILinkable"/> object.
		/// </summary>
		ILink LinkHelper
		{
			get;
		}

		/// <summary>
		/// Determines if this <see cref="ILinkable"/> is linked to the parameter-supplied 
		/// <see cref="ILinkable"/> object.
		/// </summary>
		/// <param name="link">The object for which method is to determine if a link from this
		/// <see cref="ILinkable"/> to <paramref name="link"/> exists.</param>
		/// <returns>True if this <see cref="ILinkable"/> is linked to the parameter-supplied 
		/// <see cref="ILinkable"/> object.  False otherwise.</returns>
		bool IsLinkedTo( ILinkable link );
	}
}
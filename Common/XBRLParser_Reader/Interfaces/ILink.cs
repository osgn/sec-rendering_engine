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
	/// Defines the methods that a class implements to support XBRL linking.
	/// </summary>
	public interface ILink
	{
		/// <summary>
		/// Links two objects that implement <see cref="ILinkable"/>.
		/// </summary>
		/// <param name="parent">The object to be linked to <paramref name="child"/>.</param>
		/// <param name="child">The object to be linked to <paramref name="parent"/>.</param>
		void Link( ILinkable parent, ILinkable child  );

		/// <summary>
		/// Add a parameter-supplied <see cref="ILinkable"/> object to this <see cref="ILink"/>'s links
		/// collection.
		/// </summary>
		/// <param name="lw">The object to be added to this <see cref="Linker"/>'s collection of links.</param>
		void LinkInternal(ILinkable lw);

		/// <summary>
		/// Removes a link of a given parameter supplied key from this <see cref="ILink"/>'s links
		/// collection.
		/// </summary>
		/// <param name="key">The key of the link to be removed.</param>
		/// <returns>True.</returns>
		/// <remarks>Should be internal to the class</remarks>
		bool UnlinkInternal(string key);

		/// <summary>
		/// Removes the link between two <see cref="ILinkable"/> objects.
		/// </summary>
		/// <param name="parent">The object to be unlinked from <paramref name="child"/>.</param>
		/// <param name="child">The object to be unlinked from <paramref name="parent"/>.</param>
		bool Unlink(ILinkable parent, ILinkable child);
		//void RemoveAllLinks( ILinkable parent );
	}
}
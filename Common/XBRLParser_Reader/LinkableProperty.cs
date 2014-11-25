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

using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// LinkableProperty
	/// </summary>
	public class LinkableProperty : ILinkable
	{
		#region properties

		/// <summary>
		/// The key of this <see cref="LinkableProperty"/>.
		/// </summary>
		public string Id;

		/// <summary>
		/// The address of this <see cref="LinkableProperty"/>.
		/// </summary>
		public string address;

		/// <summary>
		/// The markup data of this <see cref="LinkableProperty"/>.
		/// </summary>
		public string markupData;


		/// <summary>
		/// Locaton of the markup in Excel   Word, Crostag
		/// It is either a range name or MarkupKey
		/// </summary>
		public object MarkupLocation = null;

		/// <summary>
		/// The <see cref="Linker"/> (that implements <see cref="ILink"/>) that 
		/// supports linking to/from this <see cref="LinkableProperty"/> object.
		/// </summary>
		public Linker _linker = new Linker();

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new LinkableProperty.
		/// </summary>
		public LinkableProperty()
		{
		}

		#endregion

		#region ILinkable Members

		/// <summary>
		/// The key of this <see cref="LinkableProperty"/>.  Its <see cref="Id"/>.
		/// </summary>
		public string MyKey
		{
			get	{ return Id; }
		}

		/// <summary>
		/// The <see cref="ILink"/> that supports linking to/from this <see cref="LinkableProperty"/> object.
		/// </summary>
		public ILink LinkHelper
		{
			get	{ return _linker; }
		}

		/// <summary>
		/// Determines if this <see cref="LinkableProperty"/> is linked to the parameter-supplied 
		/// <see cref="ILinkable"/> object.
		/// </summary>
		/// <param name="link">The object for which method is to determine if a link from this
		/// <see cref="LinkableProperty"/> to <paramref name="link"/> exists.</param>
		/// <returns>True if this <see cref="LinkableProperty"/> is linked to the parameter-supplied 
		/// <see cref="ILinkable"/> object.  False otherwise.</returns>
		public bool IsLinkedTo(ILinkable link)
		{
			return _linker.IsLinkedTo( link );
		}

		#endregion

		/// <summary>
		/// Creates a link between this <see cref="LinkableProperty"/> and another 
		/// <see cref="LinkableProperty"/>.
		/// </summary>
		/// <param name="lp">The <see cref="LinkableProperty"/> to which this <see cref="LinkableProperty"/> 
		/// is to be linked.</param>
		public void Link( LinkableProperty lp )
		{
			_linker.Link( this, lp );
		}

		/// <summary>
		/// Removes a link between this <see cref="LinkableProperty"/> and another 
		/// <see cref="LinkableProperty"/>.
		/// </summary>
		/// <param name="lp">The <see cref="LinkableProperty"/> from which this <see cref="LinkableProperty"/> 
		/// is to be linked.</param>
		public bool Unlink(LinkableProperty lp)
		{
			return _linker.Unlink( this, lp );
		}

		/// <summary>
		/// This method removes all associated links before clearing the array
		/// </summary>
		public void RemoveAllLinks()
		{
			_linker.RemoveAllLinks( this );
		}

		/// <summary>
		/// The number of links associated with this <see cref="LinkableProperty"/>.
		/// </summary>
		public int LinkCount
		{
			get { return _linker.Count; }
		}

		/// <summary>
		/// The links associated with this <see cref="LinkableProperty"/>.
		/// </summary>
		public ICollection Links
		{
			get { return _linker.links.Values; }
		}

	}
}
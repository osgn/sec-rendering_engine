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
	/// Linker
	/// </summary>
	public class Linker : ILink
	{
		#region properties
		/// <summary>
		/// The collection of links maintained by this <see cref="Linker"/>
		/// </summary>
		/// <remarks><see cref="SortedList"/> key is <see cref="ILinkable.MyKey"/>.
		///   <see cref="SortedList"/> value is the <see cref="ILinkable"/> object.</remarks>
		public SortedList links = new SortedList();
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Linker"/>.
		/// </summary>
		public Linker()
		{
		}

		#endregion

		/// <summary>
		/// Links two objects that implement <see cref="ILinkable"/>.
		/// </summary>
		/// <param name="parent">The object to be linked to <paramref name="child"/>.</param>
		/// <param name="child">The object to be linked to <paramref name="parent"/>.</param>
		/// <remarks>The link is implemented by forwarding the link request to the <see cref="ILinkable.LinkHelper"/> of 
		/// the <paramref name="child"/>.</remarks>
		public void Link(ILinkable parent, ILinkable child)
		{
			LinkInternal( child );
			child.LinkHelper.LinkInternal( parent );
		}

		/// <summary>
		/// Add a parameter-supplied <see cref="ILinkable"/> object to this <see cref="Linker"/>'s links
		/// collection.
		/// </summary>
		/// <param name="lw">The object to be added to this <see cref="Linker"/>'s collection of links.</param>
		public void LinkInternal(ILinkable lw)
		{
			if ( !links.ContainsKey( lw.MyKey ) )
			{
				links.Add( lw.MyKey, lw );
			}
		}

		/// <summary>
		/// Removes the link between two <see cref="ILinkable"/> objects.
		/// </summary>
		/// <param name="parent">The object to be unlinked from <paramref name="child"/>.</param>
		/// <param name="child">The object to be unlinked from <paramref name="parent"/>.</param>
		/// <remarks>The unlink is implemented by forwarding the request to the <see cref="ILinkable.LinkHelper"/> of 
		/// the <paramref name="child"/>.</remarks>
		public bool Unlink(ILinkable parent, ILinkable child)
		{
			UnlinkInternal( child.MyKey );

			// and unlink the other
			return child.LinkHelper.UnlinkInternal( parent.MyKey );
		}

		/// <summary>
		/// Removes a link of a given parameter supplied key from this <see cref="Linker"/>'s links
		/// collection.
		/// </summary>
		/// <param name="key">The key of the link to be removed.</param>
		/// <returns>True.</returns>
		/// <remarks>Should be internal to the class</remarks>
		public bool UnlinkInternal( string key )
		{
			links.Remove( key );
			return true;
		}

		/// <summary>
		/// This method removes all associated links before clearing the array
		/// </summary>
		public void RemoveAllLinks( ILinkable parent )
		{
			while (links.Count > 0)
			{
				Unlink( parent, (ILinkable)links.GetByIndex(0) );
			}
		}

		/// <summary>
		/// Determines if this <see cref="Linker"/>'s links collection contains a link
		/// for a parameter-supplied <see cref="ILinkable"/>/
		/// </summary>
		/// <param name="lw">The <see cref="ILinkable"/> for which this <see cref="Linker"/>'s links
		/// collection is to be searched.</param>
		/// <returns>True if this <see cref="Linker"/>'s links collections contains <paramref name="lw"/>.
		/// </returns>
		public bool IsLinkedTo(ILinkable lw)
		{
			return links.ContainsKey( lw.MyKey );
		}

		/// <summary>
		/// The number of links maintained by this <see cref="Linker"/>.
		/// </summary>
		public int Count
		{
			get { return links.Count; }
		}

//		#region compare to override
//		public override int CompareTo(object obj)
//		{
//			if ( obj is string ) return BookmarkKey.CompareTo( (string)obj );
//			if ( obj is LinkableWrapper ) return BookmarkKey.CompareTo( ((LinkableWrapper)obj).BookmarkKey );
//
//			return base.CompareTo (obj);
//		}
//		#endregion
	}
}
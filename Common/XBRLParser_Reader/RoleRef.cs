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
using System.Collections.Generic;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// RoleRef
	/// </summary>
	[Serializable]
	public class RoleRef
	{
		#region properties

		internal string href;
		internal string uri;

		internal List<string> fileReferences;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="RoleRef"/>.
		/// </summary>
		public RoleRef()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="RoleRef"/>.
		/// </summary>
		/// <param name="hrefArg">The href property value to be assigned 
		/// to the newly created <see cref="RoleRef"/>.</param>
		/// <param name="uriArg">The uri property value to be assigned 
		/// to the newly created <see cref="RoleRef"/>.</param>
		public RoleRef(string hrefArg, string uriArg)
		{
			href = hrefArg;
			uri = uriArg;
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="RoleRef"/>, copying 
		/// properties from a parameter-supplied <see cref="RoleRef"/>.
		/// </summary>
		/// <param name="rr">The <see cref="RoleRef"/> from which the properties 
		/// of the newly created <see cref="RoleRef"/> are to be copied.</param>
		public RoleRef(RoleRef rr)
		{
			href = rr.href;
			uri = rr.uri;
		}
		#endregion

		/// <summary>
		/// Retrieves the schema portion of this <see cref="RoleRef"/>'s href property.
		/// </summary>
		/// <example>
		/// If href = "balancesheet.xsd#assets", GetSchemaName = "balancesheet.xsd".
		/// </example>
		/// <returns>The parsed schema.</returns>
		/// <exception cref="ApplicationException">If the href is not well formed.</exception>
		/// <exception cref="ArgumentNullException">If the href is null.</exception>
		public string GetSchemaName()
		{
			string[] strs = href.Split( '#' );

			if ( strs.Length < 2 )
			{
				throw new AucentException( "Can't split href: " + href );
			}

			return strs[0];
		}

		/// <summary>
		/// Retrieves the id portion of this <see cref="RoleRef"/>'s href property.
		/// </summary>
		/// <example>
		/// If href = "balancesheet.xsd#assets", GetSchemaName = "assets".
		/// </example>
		/// <exception cref="ApplicationException">If the href is not well formed.</exception>
		/// <exception cref="ArgumentNullException">If the href is null.</exception>
		public string GetId()
		{
			string[] strs = href.Split( '#' );

			if ( strs.Length < 2 )
			{
				throw new AucentException( "Can't split href: " + href );
			}

			return strs[1];
		}

		public string GetHref()
		{
			return href;
		}

		internal void SetHref(string s)
		{
			href = s;
		}

		#region file references

		/// <summary>
		/// Returns the file references property of this <see cref="RoleRef"/>.
		/// </summary>
		/// <returns>The file references property of this <see cref="RoleRef"/>.</returns>
		public List<string> GetFileReferences()
		{
			return this.fileReferences;
		}

		public void AddFileReference( string fileRef )
		{
			if ( string.IsNullOrEmpty( fileRef ) )
			{
				return;
			}

			if ( this.fileReferences == null )
			{
				this.fileReferences = new List<string>();
			}

			if ( !this.fileReferences.Contains( fileRef ) )
			{
				this.fileReferences.Add( fileRef );
			}
		}

		public void MergeFileReferences( List<string> fileRefs )
		{
			if ( fileRefs == null || fileRefs.Count == 0 )
			{
				return;
			}

			foreach ( string file in fileRefs )
			{
				this.AddFileReference( file );
			}
		}

		#endregion
	}
}
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

using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// RoleType
	/// </summary>
	[Serializable]
	public class RoleType : IComparable<RoleType>
	{
		#region properties

		private string uri;
		/// <summary>
		/// Gets the URI for this role type as it was defined in the taxonomy files.
		/// </summary>
		public string Uri
		{
			get { return uri; }
            set { uri = value; }
		}

		internal string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

		private string schemaFullFileName;
		/// <summary>
		/// Public accessor for the taxonomy schema file name for this role type.
		/// </summary>
		public string SchemaFullFileName
		{
			get { return schemaFullFileName; }
		}

		private string href;
        public string Href
        {
            get { return href; }
            set { href = value; }
        }

		internal ArrayList whereUsed = new ArrayList();

		private string definition = string.Empty;

		/// <summary>
		/// The definition of this <see cref="RoleType"/>.
		/// </summary>
		public string Definition
		{
			get { return definition; }
			set { definition = value; }
		}
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="RoleType"/>.
		/// </summary>
		public RoleType()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="RoleType"/>.
		/// </summary>
		/// <param name="idArg">The id to be assigned to the newly created 
		/// <see cref="RoleType"/>.</param>
		/// <param name="uriArg">The uri to be assigned to the newly created 
		/// <see cref="RoleType"/>.</param>
		/// <param name="schemaFileName">The full path of the taxonomy schema file where the role type was defined.
		/// <see cref="RoleType"/>.</param>
		public RoleType(string uriArg, string idArg, string schemaFileName )
		{
			uri = uriArg;
			id  = idArg;
			schemaFullFileName = schemaFileName;
		}

		/// <summary>
		/// Creates a new instance of <see cref="RoleType"/> and initialize 
		/// its properties from a supplied <see cref="RoleType"/>.
		/// </summary>
		/// <param name="arg">The <see cref="RoleType"/> from which 
		/// property values are to be copied.</param>
		public RoleType(RoleType arg)
		{
			uri = arg.uri;
			id  = arg.id;
			this.schemaFullFileName = arg.schemaFullFileName;
			whereUsed = arg.whereUsed;
			definition = arg.definition;
		}

		#endregion

		/// <summary>
		/// Add a supplied link to the "where used" collection of this <see cref="RoleType"/>.
		/// </summary>
		/// <param name="link">The link to be added.</param>
		/// <exception cref="ArgumentNullException">Thrown when the link element is null</exception>
		/// <exception cref="AucentException">Thrown when there is no colon (":") present 
		/// in <paramref name="link"/></exception>
		public void AddLink( string link )
		{
			if ( link == null )
			{
				throw new ArgumentNullException( "link", "link:usedOn element has no data" );
			}
			else if ( link.IndexOf( ':' ) == -1 )
			{
				throw new AucentException( "link must be in the format 'link:uri'" );
			}

			whereUsed.Add( link );
		}

		/// <summary>
		/// Set the definition property of this <see cref="RoleType"/>/
		/// </summary>
		/// <param name="def">The definition value to be assigned.</param>
		public void SetDefinition (string def)
		{
			this.definition = def;
		}

		public bool UsedIn( string reference )
		{
			// this is expected to be a small array, if not we should change this algorithm
			if ( whereUsed.Count == 0 )
			{
				return false;
			}
			else if ( whereUsed.Count > 100 )
			{
				Common.WriteWarning( "XBRLParser.Warning.ChangeAlgorithm", null, whereUsed.Count.ToString() );
			}

			bool found = false;
			for ( int i=0; i < whereUsed.Count && !found; ++i )
			{
				string[] vals = (whereUsed[i] as string).Split( ':' );

				found = string.Compare( vals[1], reference, true ) == 0;
			}

			return found;
		}

		#region href

		public string GetHref()
		{
			return href;
		}

		internal void SetHref( string s )
		{
			href = s;
		}

        public string GetId()
        {
            

            return id;
        }

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Definition))
			{
				return this.uri;

			}
			else
			{

				return this.definition;
			}

		}

        #region IComparable<RoleType> Members

        /// <summary>
        /// sorts by the URI
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(RoleType other)
        {
            return this.uri.CompareTo(other.uri);
        }

        #endregion
    }
}
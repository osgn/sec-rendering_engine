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
using System.IO;
using System.Text;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Encapsulates and methods and properties associated with an individual component
	/// of a taxonomy, loaded from a unique location.
	/// </summary>
	[Serializable]
	public class TaxonomyItem : IComparable
	{
		#region properties

		/// <summary>
		/// Indicates if this <see cref="TaxonomyItem"/> represents an Rivet extended
		/// taxonomy.
		/// </summary>
		public bool			AucentExtendedTaxonomy = false;

		/// <summary>
		/// The namespace prefix for this <see cref="TaxonomyItem"/>.
		/// </summary>
		public string		Namespace = string.Empty;

		/// <summary>
		/// The namespace location for this <see cref="TaxonomyItem"/>.
		/// </summary>
		public string WebLocation = string.Empty;

		/// <summary>
		/// The fully-qualified name of the file underlying this <see cref="TaxonomyItem"/>.
		/// </summary>
		public string		Location = string.Empty;

		/// <summary>
		/// (Appears to be referenced in code, but is deprecated.)
		/// </summary>
		public const int InvalidIndex = -1;

		/// <summary>
		/// member to determine if the taxonomy has defined custom complex types
		/// this information is useful in determining if a tax item needs to be retained
		/// when building entry points...
		/// </summary>
		public bool HasCustomTypes = false;

		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="TaxonomyItem"/>.
		/// </summary>
		public TaxonomyItem()
		{
		}

		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="TaxonomyItem"/>.
		/// </summary>
		/// <param name="isExtended">If true, this new <see cref="Taxonomy"/> item will
		/// be a Rivet extended taxonomy.</param>
		/// <param name="location">The fully-qualified name of the file underlying this
		/// new <see cref="TaxonomyItem"/>.</param>
		/// <param name="Namespace">The namespace prefix for this new <see cref="TaxonomyItem"/>.</param>
		/// <param name="WebLocation">The namespace location for this new <see cref="TaxonomyItem"/>.</param>
		/// <param name="hasCustomerTypes"> <see cref="TaxonomyItem"/>.</param>
		public TaxonomyItem(string WebLocation, string location, string Namespace, bool isExtended, bool hasCustomerTypes)
		{
			Initialize( WebLocation, location, Namespace,  isExtended , hasCustomerTypes);
		}


		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="TaxonomyItem"/>.
		/// </summary>
		/// <param name="isExtended">If true, this new <see cref="Taxonomy"/> item will
		/// be a Rivet extended taxonomy.</param>
		/// <param name="location">The fully-qualified name of the file underlying this
		/// new <see cref="TaxonomyItem"/>.</param>
		/// <param name="Namespace">The namespace prefix for this new <see cref="TaxonomyItem"/>.</param>
		/// <param name="WebLocation">The namespace location for this new <see cref="TaxonomyItem"/>.</param>
		public TaxonomyItem(string WebLocation, string location, string Namespace, bool isExtended)
		{
			Initialize(WebLocation, location, Namespace, isExtended, false);
		}

	
		private void Initialize( string WebLocation, string location, string Namespace, bool isExtended, bool hasCustomTypes)
		{
			this.Namespace = Namespace;
			this.Location = location;
			this.WebLocation = WebLocation;
			this.AucentExtendedTaxonomy = isExtended;
			this.HasCustomTypes = hasCustomTypes;
		}

//		public static string GetHashKey(string nameSpace, string location)
//		{
//			StringBuilder key = new StringBuilder(nameSpace);
//			key.Append("?").Append(location);
//			return key.ToString();
//		}

		#endregion

		/// <summary>
		/// The non fully-qualified (no directory) name of the file underlying this <see cref="TaxonomyItem"/>.
		/// </summary>
		public string Filename
		{
			get 
			{ 
				int lastSlash = Location.LastIndexOf( "\\" );

				if ( lastSlash == -1 )
				{
					lastSlash = Location.LastIndexOf( "//" );
				}
				if (lastSlash == -1)
				{
					lastSlash = Location.LastIndexOf("/");
				}
				return Location.Substring( lastSlash+1, Location.Length-lastSlash-1 );
			}
		}

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="TaxonomyItem"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="TaxonomyItem "/>
		/// is to be compared.  Assumed to be a <see cref="TaxonomyItem "/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="TaxonomyItem"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> for the
		/// <see cref="WebLocation"/> properties of this <see cref="TaxonomyItem"/> and <paramref name="obj"/>.</remarks>
		public int CompareTo(object obj)
		{
			if ( obj is TaxonomyItem )
			{
				return string.Compare( WebLocation, ((TaxonomyItem)obj).WebLocation );
			}
			else
			{
				throw new ApplicationException( "hashtable error: TaxonomyItem hashtable contains non-TaxonomyItems. Offending object: " + obj.ToString() );
			}
		}

		#endregion

		

		internal TaxonomyItem CloneTaxonomyItem()
		{
			TaxonomyItem clone = new TaxonomyItem();
			clone.AucentExtendedTaxonomy = this.AucentExtendedTaxonomy;
			clone.Namespace = this.Namespace;
			clone.WebLocation = this.WebLocation;
			clone.Location = this.Location;
			clone.HasCustomTypes = this.HasCustomTypes;


			return clone;
		}

		
	}
}
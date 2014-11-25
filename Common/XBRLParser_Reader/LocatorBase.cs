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
using System.Xml;
using System.Xml.Serialization;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents a "locator" type link within an XBRL linkbase.
	/// </summary>
	[Serializable]
	public class LocatorBase
	{
		#region properties

		internal string xsd = null;
		/// <summary>
		/// The schema portion of <see cref="UnpartitionedHref"/>.
		/// </summary>
		/// <remarks><see cref="ParseHRef"/> must first be called to populate
		/// this property.</remarks>
		public string Xsd
		{
			get { return xsd; }
			set { xsd = value; }
		}

		internal string unpartitionedHref = null;
		/// <summary>
		/// The "raw" value of the link's "href" attribute.
		/// </summary>
		public string UnpartitionedHref
		{
			get { return unpartitionedHref; }
			set { unpartitionedHref = value; }
		}

		internal string href = null;
		public string HRef
		{
			get { return href; }
			set { href = value; }
		}

		internal ArrayList labelArray = new ArrayList();
		public ArrayList LabelArray
		{
			get { return labelArray; }
			set { labelArray = value; }
		}

		public string Label
		{
			get	{ return labelArray.Count > 0 ? (string)labelArray[0] : string.Empty; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="LocatorBase"/>.
		/// </summary>
		public LocatorBase()
		{
		}

		/// <summary>
		/// Constructs a new instance of <see cref="LocatorBase"/>.
		/// </summary>
		/// <param name="hrefArg">The "raw" value of the locator link's "href" attribute.</param>
		public LocatorBase(string hrefArg)
		{
			unpartitionedHref = hrefArg;
		}

		#endregion

		#region Label
		internal void AddLabel( string newLabel )
		{
			int index = labelArray.BinarySearch( newLabel );
			if ( index < 0 )
			{
				labelArray.Insert( ~index, newLabel );
			}
		}

		private bool HasLabel( string newLabel )
		{
			return labelArray.BinarySearch( newLabel ) > -1;
		}

		#endregion

		/// <summary>
		/// Parses the <see cref="unpartitionedHref"/> property of this object,
		/// populating <see cref="xsd"/> and <see cref="href"/>.
		/// </summary>
		/// <param name="errorList">An <see cref="ArrayList"/> to which any errors 
		/// encountered will be added.</param>
		public void ParseHRef( ArrayList errorList )
		{
			if ( unpartitionedHref != null && unpartitionedHref.IndexOf( '#' ) != -1 )
			{
				string[] vals = unpartitionedHref.Split( '#' );

				if ( vals.Length == 2 )
				{
					this.xsd = vals[0];
					this.href = vals[1];
				}
				else
				{
					Common.WriteWarning( "XBRLParser.Warning.IncorrectHRefPartitions", errorList, vals.Length.ToString(), unpartitionedHref );
				}
			}
		}

		/// <summary>
		/// Initializes this instance of <see cref="LocatorBase"/> from a parameter-supplied
		/// <see cref="LocatorBase"/>.
		/// </summary>
		/// <param name="orig">The <see cref="LocatorBase"/> from which this instance of <see cref="LocatorBase"/>
		///  is to be initialized.</param>
		internal void CopyLocatorBaseInformation(LocatorBase orig)
		{
			this.xsd = orig.xsd;
			this.unpartitionedHref = orig.unpartitionedHref;
			this.href = orig.href;

			foreach (string str in orig.labelArray)
			{
				this.labelArray.Add(str);
			}

		}
	}
}
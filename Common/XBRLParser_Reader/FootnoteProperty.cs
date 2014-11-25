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
using Aucent.MAX.AXE.Common.Utilities;

using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// FootnoteProperty
	/// </summary>
	public class FootnoteProperty : LinkableProperty, ILinkable, IComparable
	{
		#region properties
		//language should be the 2 character code for the language
		internal string language;

		/// <summary>
		/// True if this <see cref="FootnoteProperty"/> has been written.
		/// </summary>
		public bool HasBeenWritten = false;

		internal string HtmlLink = string.Empty;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new <see cref="FootnoteProperty"/>.
		/// </summary>
		public FootnoteProperty()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new <see cref="FootnoteProperty"/>.
		/// </summary>
		/// <param name="addr">The address to be assigned to the newly created <see cref="FootnoteProperty"/>.</param>
		/// <param name="id">The id to be assigned to the newly created <see cref="FootnoteProperty"/>.</param>
		/// <param name="data">Markup data to be assigned to the newly created <see cref="FootnoteProperty"/>.</param>
		/// <param name="lang">The XBRL-standard language code to be assigned to the newly created <see cref="FootnoteProperty"/>.  
		/// Must be a valid language code or "en" will be assigned.</param>
		public FootnoteProperty(string id, string addr, string lang, string data)
		{
			Id = id;
			address = addr;
			markupData = data;
            language = lang;
            			
		}
		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="FootnoteProperty"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="FootnoteProperty"/>
		/// is to be compared.  Assumed to be a <see cref="FootnoteProperty"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="FootnoteProperty"/>.</returns>
		/// <remarks>The results returned are equivalent to <see cref="String.CompareTo(String)"/> for the <see cref="HtmlLink"/> 
		/// properties of the two objects.</remarks>
		public int CompareTo(object obj)
		{
			return HtmlLink.CompareTo( ((FootnoteProperty)obj).HtmlLink ) ;
		}

		#endregion
	}

    public class FootnoteIdComparer : IComparer
    {

        #region IComparer Members

        public int Compare(object x, object y)
        {
            FootnoteProperty one = x as FootnoteProperty;
            FootnoteProperty two = y as FootnoteProperty;

            string subone = one.Id.Replace("Footnote-", string.Empty);
            string subtwo = two.Id.Replace("Footnote-", string.Empty);

            int id1 = 0, id2 = 0;

            if (int.TryParse(subone, out id1) && int.TryParse(subtwo, out id2))
            {
                return id1.CompareTo(id2);
            }

            return one.Id.CompareTo(two.Id);
        }

        #endregion
    }
}
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
using System.Text;
using System.Xml;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL reference linkbase.
	/// </summary>
	public class Reference: DocumentBase	
	{
		#region constants

		private  const string LOCATOR_KEY			= "//link:loc[@xlink:type='locator']";
		private  const string REFERENCE_LINK_KEY	= "//link:referenceLink";
		private  const string BIND_ELEMENT_KEY		= "//xlink:from";


		private  const string LOCATOR_TYPE			= "locator";
		private  const string ARC_TYPE				= "arc";
		private  const string RES_TYPE				= "resource";

		#endregion
        
		#region properties

		internal Hashtable references;

		public Hashtable ReferencesTable
		{
			get { return references; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Reference.
		/// </summary>
		public Reference()
		{
		}

		#endregion

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="Presentation"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		protected override void ParseInternal(out int numErrors)
		{
			numErrors = 0;

				XmlNodeList locNodes = theDocument.SelectNodes( LOCATOR_KEY, theManager );
			if ( locNodes == null || locNodes.Count == 0 )
			{
				Common.WriteWarning( "XBRLParser.Error.ReferenceDoesNotContainReferences", errorList, schemaFilename );
				return;
			}

			
			XmlNodeList refs = theDocument.SelectNodes( REFERENCE_LINK_KEY, theManager );
			foreach ( XmlNode node in refs )
			{
				ReferenceLink refLink = new ReferenceLink(errorList);

				refLink.ParseLinks( node, theManager, out numErrors );

				
					if (refLink.References != null && refLink.References.Count > 0)
					{
						if (references == null)
						{
							references = new Hashtable();
						}
						foreach (ReferenceLocator rl in refLink.References.Values )
						{
							if (references.ContainsKey(rl.HRef))
							{
								ReferenceLocator orig = references[rl.HRef] as ReferenceLocator;
								orig.Merge(rl);
							}
							else
							{
								references[rl.HRef] = rl;
							}
						}

					}
				

			}

		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <exception cref="NotImplementedException">Always thrown.</exception>
		public override string ToXmlString()
		{
			throw new NotImplementedException( "Reference.ToXmlString() not implemented - should not be called" );
		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <exception cref="NotImplementedException">Always thrown.</exception>
		public override void ToXmlString(int numTabs, bool verbose, string language, StringBuilder text)
		{
			throw new NotImplementedException( "Reference.ToXmlString( int, StringBuilder ) not implemented - should not be called" );
		}

	}
}
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
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL presentation linkbase or calculation linkbase.
	/// </summary>
	/// <remarks>This class is also used to represent a calculation linkbase.</remarks>
	public class Presentation : DocumentBase
	{
		internal const string PLINK_KEY = "//link:presentationLink";
		private const string CLINK_KEY = "//link:calculationLink";
		private const string RREF_KEY = "//link:roleRef";

		private const string ROLE_TAG = "xlink:role";
		private const string TITLE_TAG = "xlink:title";
		private const string HREF_TAG = "xlink:href";
		private const string RURI_TAG = "roleURI";

		private const string HEADER_STR = "Presentation";
		private const string CALATIONHEADER_STR = "Calculation";

		#region enums

		/// <summary>
		/// The linkbase types that can be represented by the <see cref="Presentation"/> class.
		/// </summary>
		public enum PreseantationTypeCode
		{
			/// <summary>
			/// The linkbase type is not defined.
			/// </summary>
			None, 

			/// <summary>
			/// Presentation linkbase.
			/// </summary>
			Presentation, 

			/// <summary>
			/// Calculation linkbase.
			/// </summary>
			Calculation
		}
		#endregion

		#region properties

		private PreseantationTypeCode processingPresenationType = PreseantationTypeCode.None;

		/// <summary>
		/// The linkbase type represented by this instance of <see cref="Presentation"/>.
		/// </summary>
		public PreseantationTypeCode ProcessingPresenationType
		{
			get { return processingPresenationType; }
			set { processingPresenationType = value; }
		}

		internal Hashtable roleRefs = null;

		/// <key>title</key>
		/// <value>PresentationLink object</value>
		private Hashtable presentationLinks = null;

		public Hashtable PresentationLinks
		{
			get { return presentationLinks; }
			set { presentationLinks = value; }
		}

		private Hashtable calculationLinks = null;

		/// <key>title</key>
		/// <value>PresentationLink object</value>
		public Hashtable CalculationLinks
		{
			get { return calculationLinks; }
			set { calculationLinks = value; }
		}

        public Hashtable RoleRefs
        {
            get { return roleRefs; }
        }

		internal string BaseSchema;
		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="Presentation"/>.
		/// </summary>
		public Presentation()
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
			LoadRoleRefs(out numErrors);

			int errors = 0;

			LoadLinks(out errors);

			numErrors += errors;
		}

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="Presentation"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="discoveredSchemas">The collection to which additional XML schemas associated with 
		/// locator links will be added.
		/// </param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		protected override void ParseInternal(	Dictionary<string, string> discoveredSchemas, out int numErrors)
		{
			numErrors = 0;
            LoadRoleRefs( discoveredSchemas, out numErrors);

			int errors = 0;
			LoadLinks( discoveredSchemas, out errors);

			numErrors += errors;
		}


		private bool HasRoleReferences()
		{
			return roleRefs != null;
		}

		public bool VerifyRoleReference(string xsdFile, string uri)
		{
			RoleRef rr = roleRefs[uri] as RoleRef;

			if (rr == null)
				return false;

			try
			{
				return string.Compare(xsdFile, rr.GetSchemaName(), true) == 0;
			}
			catch (AucentException)
			{
				return false;
			}
			catch (ArgumentNullException)
			{
				return false;
			}
		}

        public void LoadRoleRefs(out int numErrors)
        {

            LoadRoleRefs( new Dictionary<string, string>(), out numErrors);
        }

        public void LoadRoleRefs( Dictionary<string, string> discoveredSchemas, out int numErrors)
		{
			numErrors = 0;

			XmlNodeList rolesList = theDocument.SelectNodes(RREF_KEY, theManager);

			if (rolesList == null )
			{
				return;
			}
			

			foreach (XmlNode role in rolesList)
			{
				if (roleRefs == null)
				{
					roleRefs = new Hashtable();
				}
				string uri = string.Empty;
				string href = string.Empty;

				if (!Common.GetAttribute(role, HREF_TAG, ref href, errorList) ||
					!Common.GetAttribute(role, RURI_TAG, ref uri, errorList))
				{
					++numErrors;
					continue;
				}

				RoleRef rr = new RoleRef(href, uri);
				roleRefs[uri] = rr;


                string xsdName = rr.GetSchemaName();
                LinkBase.AddDiscoveredSchema(this.schemaPath, xsdName, discoveredSchemas);

               
			}
		}

		protected int LoadLinks(out int errorsEncountered)
		{
			return LoadLinks( new Dictionary<string, string>(), out errorsEncountered);
		}

		private  int LoadLinks(	Dictionary<string, string> discoveredSchemas, out int errorsEncountered)
		{
			errorsEncountered = 0;
			string linkKey = "";

			if (processingPresenationType == PreseantationTypeCode.Presentation)
			{
				linkKey = PLINK_KEY;
				presentationLinks = new Hashtable();
			}
			else if (processingPresenationType == PreseantationTypeCode.Calculation)
			{
				linkKey = CLINK_KEY;
				calculationLinks = new Hashtable();
			}

			XmlNodeList pLinksList = theDocument.SelectNodes(linkKey, theManager);
			if (pLinksList == null )
			{
				//it is ok to have empty pres /calc linkbase files
				//Common.WriteWarning("XBRLParser.Error.LinkbaseDoesNotContainLinks", errorList, schemaFilename);
				return 0;
			}
			int counter = 0;
			foreach (XmlNode plNode in pLinksList)
			{
				counter++;
				// first get the label
				string role = string.Empty;
				string title = string.Empty;

				// only role attribute is required
				if (!Common.GetAttribute(plNode, ROLE_TAG, ref role, errorList))
				{
					++errorsEncountered;
					continue;
				}

				Common.GetAttribute(plNode, TITLE_TAG, ref title, null);

				if (title == string.Empty)
				{
					title = role;
				}

				if (processingPresenationType == PreseantationTypeCode.Presentation)
				{
					Common.WriteInfo("XBRLParser.Info.CurrentPresentationLink", title);
					ProcessPresentationLinks(plNode, role, title, ref errorsEncountered,  discoveredSchemas);
				}
				else if (processingPresenationType == PreseantationTypeCode.Calculation)
				{
					Common.WriteInfo("XBRLParser.Info.CurrentCalculationLink", title == null ? role : title);
					ProcessCalculationLinks(plNode, role, title == null ? role : title, ref errorsEncountered,  discoveredSchemas);
				}
			}

			return counter;
		}

		private void ProcessPresentationLinks(XmlNode plNode,
			string role, string title, ref int errorsEncountered, 
			Dictionary<string, string> discoveredSchemas)
		{
			PresentationLink pl = presentationLinks[role] as PresentationLink;

			if (pl == null)
			{
				// create the object
				pl = new PresentationLink(title, role, BaseSchema, errorList);

				// put it in the hashtable
				presentationLinks[role] = pl;
			}

			// and load up the links
			int linkErrors = 0;
			pl.LoadChildren(plNode, theManager,  discoveredSchemas, this.schemaPath, out linkErrors);

			errorsEncountered += linkErrors;
		}

		private void ProcessCalculationLinks(XmlNode plNode,
			string role, string title, ref int errorsEncountered, 
			Dictionary<string, string> discoveredSchemas)
		{
			PresentationLink pl = calculationLinks[role] as PresentationLink;

			if (pl == null)
			{
				// create the object
				pl = new PresentationLink(title, role, BaseSchema, errorList);

				// put it in the hashtable
				calculationLinks[role] = pl;
			}

			// and load up the links
			int linkErrors = 0;
			pl.LoadChildren(plNode, theManager,  discoveredSchemas, this.schemaPath, out linkErrors);

			errorsEncountered += linkErrors;
		}

		/// <summary>
		/// Deprecated.  Do not use.
		/// </summary>
		/// <returns>Null.</returns>
		public override string ToXmlString()
		{
#if false
			FileInfo fi = new FileInfo( schemaFile );

			int len=0;
			try
			{
				len = (int)fi.Length*2;
			}
			catch ( OverflowException )
			{
				len = (int)fi.Length;
			}

			StringBuilder text = new StringBuilder(len);
			ToXmlString(0, true, "en", text);

			return text.ToString();
#endif
			return null;
		}

		/// <summary>
		/// Deprecated.  Do not use.
		/// </summary>
		/// <param name="numTabs">Not used.</param>
		/// <param name="verbose">Not used.</param>
		/// <param name="language">Not used</param>
		/// <param name="xml">Not used.</param>
		public override void ToXmlString(int numTabs, bool verbose, string language, StringBuilder xml)
		{
#if false
			if ( numTabs == 0 )
			{
				xml.Append( "<?xml version=\"1.0\" encoding=\"utf-8\"?>").Append(Environment.NewLine );
				xml.Append( "<!-- Linkbase based on XBRL standard v.2.1  Created by Aucent XBRLParser ").Append( ParserMessage.ExecutingAssemblyVersion ).Append( " Contact www.aucent.com -->" ).Append( Environment.NewLine );
			}
			else
			{
				for ( int i=0; i < numTabs; ++i )
				{
					xml.Append( "\t" );
				}
			}

			IDictionaryEnumerator enumer = null;
			if (processingPresenationType == PreseantationTypeCode.Presentation)
			{
				xml.Append( "<" ).Append( HEADER_STR ).Append(" file=\"" ).Append( schemaFile ).Append( "\">" ).Append( Environment.NewLine );
				enumer = presentationLinks.GetEnumerator();
			}
			else
			{
				if (processingPresenationType == PreseantationTypeCode.Calculation)
				{
					xml.Append( "<" ).Append( CALATIONHEADER_STR ).Append(" file=\"" ).Append( schemaFile ).Append( "\">" ).Append( Environment.NewLine );
					enumer = calculationLinks.GetEnumerator();
				}
			}

			while ( enumer.MoveNext() )
			{
				PresentationLink pl = enumer.Value as PresentationLink;
				pl.ToXmlString( numTabs+1, verbose, language, xml );
			}

			for ( int i=0; i < numTabs; ++i )
			{
				xml.Append( "\t" );
			}

			xml.Append( "</" ).Append( HEADER_STR ).Append( ">" ).Append( Environment.NewLine );
#endif
		}
	}
}
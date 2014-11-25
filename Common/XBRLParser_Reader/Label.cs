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
using System.Xml.Xsl;
using System.Collections;

using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL label linkbase.
	/// </summary>
	public class Label : DocumentBase 
	{
		private const string LABEL_LANG_KEY	= "//link:label[@xml:lang]";
		private const string LOCATOR_KEY		= "//link:loc[@xlink:type='locator']";
		private const string LABEL_LINK_KEY	= "//link:labelLink";

		internal const string LANG_ATTR		= "xml:lang";
		internal const string ROLE_ATTR		= "xlink:role";
        
		private const string LOCATOR_TYPE		= "locator";
		private const string ARC_TYPE			= "arc";
		private const string RES_TYPE			= "resource";

		#region properties

		internal ArrayList supportedLanguages = new ArrayList();
		public ArrayList SupportedLanguages
		{
			get { return supportedLanguages; }
			set { supportedLanguages = value; }

		}

		internal ArrayList labelRoles = new ArrayList();
		public ArrayList LabelRoles
		{
			get { return labelRoles; }
			set { labelRoles = value; }

		}
		/// <key>label</key>
		/// <value>labelLocator objects</value>
		internal Hashtable labels;

		public Hashtable LabelTable
		{
			get { return labels; }
		}
		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="Label"/>.
		/// </summary>
		public Label()
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

            try
            {
                XmlNodeList locNodes = theDocument.SelectNodes(LOCATOR_KEY, theManager);
                if (locNodes == null || locNodes.Count == 0)
                {
					//Common.WriteError("XBRLParser.Error.LabelDoesNotContainLabels", errorList, schemaFilename);
					//++numErrors;
                    return;
                }
            }
            catch
            {
                //No need to write the exception again, more than likely, the exception is already caught outside (missing label file)           
                return;
            }
			

			
			XmlNodeList labelList = theDocument.SelectNodes( LABEL_LINK_KEY, theManager );
			labels = new Hashtable();
			foreach ( XmlNode node in labelList )
			{
				LabelLink ll = new LabelLink(errorList);
				ll.ParseLinks(node, theManager, out numErrors);

				if (ll.Labels != null)
				{
					foreach (LabelLocator lloc in ll.Labels.Values)
					{
						LabelLocator orig = labels[lloc.HRef] as LabelLocator;
						if (orig == null)
						{
							labels[lloc.HRef] = lloc;
						}
						else
						{
							orig.AddLabels(lloc, errorList);
						}
					}
				}
			}

			

			ParseRolesAndLanguages(ref numErrors);
		}

		private void ParseRolesAndLanguages( ref int numErrors )
		{
			numErrors = 0;

			labelRoles.Clear();
			supportedLanguages.Clear();
			
			XmlNodeList langs = theDocument.SelectNodes( LABEL_LANG_KEY, theManager );
			if ( langs == null )
			{
				//we could be checking for labels inside presentation file
				//as the role was not defined in the xsd and we had to assume that the file can be
				//anything pres, calc, definition, label etc...
				Common.WriteError( "XBRLParser.Error.LabelDoesNotContainLangAttr", errorList, schemaFilename );
				return;
			}
            //else
            //{
            //    //we're going through and counting the the xml nodes because xmlNodesList count takes a long time
            //    int langCounter = 0;
            //    foreach (XmlNode xmlNode in langs)
            //    {
            //        langCounter++;
            //        break;
            //    }

            //    if(langCounter == 0)
            //    {
            //        //we could be checking for labels inside presentation file
            //        //as the role was not defined in the xsd and we had to assume that the file can be
            //        //anything pres, calc, definition, label etc...
            //        Common.WriteError("XBRLParser.Error.LabelDoesNotContainLangAttr", errorList, schemaFilename);
            //        return;
            //    }
            //}

			foreach ( XmlNode lang in langs )
			{
				string language = null;
				if ( !Common.GetAttribute( lang, LANG_ATTR, ref language, errorList ) )
				{
					++numErrors;
					continue;
				}
				int index = supportedLanguages.BinarySearch( language );

				if ( index < 0 )
				{
					// and add it in
					supportedLanguages.Insert( ~index, language );
				}

				string role = null;
				if ( Common.GetAttribute( lang, ROLE_ATTR, ref role, null ) )
				{
					string roleName = role.Substring(role.LastIndexOf('/') + 1);

					index = labelRoles.BinarySearch( roleName );

					if ( index < 0 )
					{
						// and add it in
						labelRoles.Insert( ~index, roleName );
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
			throw new NotImplementedException( "Label.ToXmlString() not implemented - should not be called" );
		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <exception cref="NotImplementedException">Always thrown.</exception>
		public override void ToXmlString(int numTabs, bool verbose, string language, StringBuilder text)
		{
			throw new NotImplementedException( "Label.ToXmlString( int, StringBuilder ) not implemented - should not be called" );
		}
	}
}
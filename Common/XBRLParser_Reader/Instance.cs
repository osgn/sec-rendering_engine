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
using System.Globalization;

using System.Xml.Schema;

using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Instance
	/// </summary>
	public class Instance
	{
		#region Constants
		private const string	XML							= "xml";

		/// <summary>
		/// The element name for the root element of an XBRL instance document 
		/// ("xbrl").
		/// </summary>
		protected const string	XBRL						= "xbrl";
		/// <summary>
		/// The element name for the root element of an XBRL instance document 
		/// ("xbrl").
		/// </summary>
		protected const string HTML = "html";

		private const string  HREF_FORMAT					= "{0}#{1}";
		private const string  LABEL_FORMAT				= "{0}_lbl";

		/// <summary>
		/// The name of the XBRL "contextRef" attribute.
		/// </summary>
		public const string		CONTEXT_REF					= "contextRef";

		/// <summary>
		/// The name of the XBRL "unitRef" attribute.
		/// </summary>
		public const string		UNIT_REF					= "unitRef";

		/// <summary>
		/// The name of the XBRL "id" attribute.
		/// </summary>
		public const string		ID							= "id";

		/// <summary>
		/// The element name ("schemaRef") for an XBRL taxonomy schema reference element.
		/// </summary>
		protected const string	SCHEMA_REF					= "schemaRef";

		/// <summary>
		/// The name of the XBRL type attribute ("type").
		/// </summary>
		protected const string	TYPE						= "type";

		/// <summary>
		/// The XBRL "type" attribute value to indicate a simple type.
		/// </summary>
		protected const string	SIMPLE						= "simple";

		/// <summary>
		/// The name of the XBRL "href" attribute ("href").
		/// </summary>
		protected const string	HREF						= "href";

		/// <summary>
		/// 
		/// </summary>
		protected const string EXTENDED = "extended";

		/// <summary>
		/// 
		/// </summary>
		protected const string FOOTNOTE_LINK = "http://www.xbrl.org/2003/role/link";
		private const string  FOOTNOTE					= "footnoteLink";
		/// <summary>
		/// 
		/// </summary>
		protected const string ROLE = "role";
		private const string  LOC							= "loc";
		private const string  LOC_KEY						= "//link:loc[@xlink:href='{0}']";
		private const string  LOCATOR						= "locator";
		private const string  LABEL						= "label";

		private const string  FOOTNOTE_ARC				= "footnoteArc";
		private const string  FOOTNOTE_ARCROLE			= "http://www.xbrl.org/2003/arcrole/fact-footnote";

		private const string	ARC							= "arc";
		private const string  FROM						= "from";
		private const string	TO							= "to";
		private const string	ORDER						= "order";
		private const string  ARCROLE						= "arcrole";

		private const string	FOOTNOTE_RESOURCE			= "footnote";
		private const string  RESOURCE					= "resource";
		private const string  FOOTNOTE_RESOURCE_ARCROLE	= "http://www.xbrl.org/2003/role/footnote";
		private const string  LANG						= "lang";


		#endregion

		#region properties

		/// <summary>
		/// The <see cref="XmlDocument"/> underlying this <see cref="Instance"/>.
		/// </summary>
		public XmlDocument xDoc;

		/// <summary>
		/// The <see cref="XmlNamespaceManager"/> for this <see cref="Instance"/>.
		/// </summary>
		public XmlNamespaceManager theManager;
		
		private XmlComment contextComment;
		private XmlComment unitComment;

		/// <summary>
		/// Any "Tuple Section" comment located within the comments of this <see cref="Instance"/>.
		/// </summary>
		protected XmlComment tupleComment;

		private XmlComment elementComment;

		/// <summary>
		/// Any "Footnote Section" comment located within this <see cref="Instance"/>.
		/// </summary>
		protected XmlComment footnoteComment;

		/// <summary>
		/// The <see cref="XmlElement"/> for any footnote link.
		/// </summary>
		protected XmlElement footnoteLink;

        /// <summary>
        /// List of all tuples in the document
        /// just the top level tuples 
        /// </summary>
        public List<TupleSet> DocumentTupleList;

		/// <summary>
		/// If true, indicates that <see cref="XmlComment"/> elements are to be created/written when 
		/// this instance document is rendered as XML.
		/// </summary>
		protected bool writeComments = true;

		/// <summary>
		/// If true, indicates that <see cref="XmlComment"/> elements are to be created/written when 
		/// this instance document is rendered as XML.
		/// </summary>
		public bool WriteComments
		{
			get {return writeComments;}
			set {writeComments = value;}
		}

		// merge props
		/// <summary>
		/// If true, indicates that properties of this document are to overwrite those of any existing 
		/// document when a merge is performed.
		/// </summary>
		public bool OverrideExistingDocument = false;

		/// <summary>
		/// If true, information from another instance document will be merged (as opposed to appended) 
		/// to this document.
		/// </summary>
		protected bool mergeDocs = false;

		/// <summary>
		/// The name of any file merged into this <see cref="Instance"/>.
		/// </summary>
		public string mergeFilename = string.Empty;
		
		/// <summary>
		/// The collection of schema refs (<see cref="String"/> objects) referenced by this <see cref="Instance"/>.
		/// </summary>
		public ArrayList schemaRefs = new ArrayList();

		/// <summary>
		/// The collection of contexts (<see cref="ContextProperty"/> objects) referenced by this <see cref="Instance"/>.
		/// </summary>
		public ArrayList contexts = new ArrayList();

		/// <summary>
		/// The collection of units (<see cref="UnitProperty"/> objects) referenced by this <see cref="Instance"/>.
		/// </summary>
		public ArrayList units = new ArrayList();

		/// <summary>
		/// The collection of markups (<see cref="MarkupProperty"/> objects) referenced by this <see cref="Instance"/>.
		/// </summary>
		public ArrayList markups = new ArrayList();

		
      
		/// <summary>
		/// Collection of attributes (<see cref="String"/> objects) defined in the instance document header.
		/// URI, Prefix etc...
		/// </summary>
		public ArrayList attributes = new ArrayList();

		/// <summary>
		/// Collection of <see cref="FootnoteProperty"/> objects.  Used to 
		/// check for duplicate footnote properties.
		/// </summary>
		/// <remarks>
		/// Key is footnoteId (a <see cref="String"/>).  Value is a <see cref="FootnoteProperty"/>.
		/// </remarks>
		public Hashtable fnProperties = new Hashtable();
		
		public ArrayList mpIDs = new ArrayList();

		/// <summary>
		/// Namespace for custom segment, scenario, unit used by <see cref="Instance"/>.
		/// </summary>
		public string SSUNamespace = string.Empty;

		/// <summary>
		/// Web location for custom segment, scenario, unit used by <see cref="Instance"/>.
		/// </summary>
		public string SSUWebLocation = string.Empty;

		/// <summary>
		/// Filename for custom segment, scenario, unit used by <see cref="Instance"/>.
		/// </summary>
		public string SSUFilename = string.Empty;



	

		/// <summary>
		/// determines whethere the document loaded is an inline xbrl document....
		/// </summary>
		public bool IsInLineXbrlDocument = false;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new <see cref="Instance"/>
		/// </summary>
		public Instance()
		{
		}

		#endregion

		#region mono require xml validations
		int numAttributeErrors;

		/// <summary>
		/// The number of attribute errors that has occurred in validation 
		/// of this instance document.
		/// </summary>
		public int NumAttributeErrors
		{
			get { return numAttributeErrors; }
		}

		int numSchemaErrors;

		/// <summary>
		/// The number of errors that occurred while reading the XML underlying 
		/// this <see cref="Instance"/>.
		/// </summary>
		public int NumSchemaErrors
		{
			get { return numSchemaErrors; }
		}

		private void ValidationCallBack (object sender, ValidationEventArgs args)
		{
			++numSchemaErrors;
		}

		private void schemas_ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			++numAttributeErrors;
		}
		#endregion

		#region load docs
		/// <summary>
		/// Reads and parses an XML file.  Populating the <see cref="XmlDocument"/> underlying 
		/// this <see cref="Instance"/>.
		/// </summary>
		/// <param name="filename">The file that is the be read and parsed.</param>
		/// <param name="errors">Any errors that occur during the load process will be 
		/// added to this collection as <see cref="String"/> objects.</param>
		public bool TryLoadInstanceDoc(string filename, out ArrayList errors)
		{
			errors = new ArrayList();
			// this stuff is necessary for mono - much harsher schema validation
			// ValidationType.None doesn't work due to Mono coding mistake

			numSchemaErrors = numAttributeErrors = 0;

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
			XmlReader xReader = XmlTextReader.Create(filename, settings);

			xDoc = new XmlDocument();

			try
			{
				//xDoc.Load( filename );
				xDoc.Load(xReader);

				SetupNamespaceMgr();
				mergeFilename = filename;
				return ValidateAndParse(ref errors);

			}

			catch (Exception ex)
			{
				errors.Add(new ParserMessage(System.Diagnostics.TraceLevel.Error, ex.Message + " " + ex.StackTrace));
				return false;
			}
			finally
			{
				xReader.Close();
			}
		}

		/// <summary>
		/// Searches the nodes of a parameter-supplied <see cref="XmlElement"/> for 
		/// <see cref="XmlComment"/> nodes.  Populates key comment node properties 
		/// of this <see cref="Instance"/>
		/// </summary>
		/// <param name="root">The XML element to be searched.</param>
		/// <returns>True if </returns>
		protected bool TryFindComments( XmlElement root )
		{
			bool foundOne = false;

			// try and find the comments, otherwise add them?
			foreach ( XmlNode node in root.ChildNodes )
			{
				if( node is XmlComment )
				{
					string comment = ((XmlComment)node).Value;

					if ( comment.CompareTo( StringResourceUtility.GetString( "XBRLParser.Info.InstanceDocContexts" ) ) == 0 )
					{
						foundOne = true;
						this.contextComment = (XmlComment)node;
					}
					else if ( comment.CompareTo( StringResourceUtility.GetString( "XBRLParser.Info.InstanceDocUnits" ) ) == 0 )
					{
						foundOne = true;
						this.unitComment = (XmlComment)node;
					}
					else if ( comment.CompareTo( StringResourceUtility.GetString( "XBRLParser.Info.InstanceDocTuples" ) ) == 0 )
					{
						foundOne = true;
						this.tupleComment = (XmlComment)node;
					}
					else if ( comment.CompareTo( StringResourceUtility.GetString( "XBRLParser.Info.InstanceDocElements" ) ) == 0 )
					{
						foundOne = true;
						this.elementComment = (XmlComment)node;
					}
					else if ( comment.CompareTo( StringResourceUtility.GetString( "XBRLParser.Info.InstanceDocFootnotes" ) ) == 0 )
					{
						foundOne = true;
						this.footnoteComment = (XmlComment)node;
					}
				}
			}

			return foundOne;
		}

		/// <summary>
		/// Initializes the namespace manager property for this <see cref="Instance"/>.  Initializes 
		/// key properties for that namespace manage.
		/// </summary>
		protected void SetupNamespaceMgr()
		{
			theManager = new XmlNamespaceManager( xDoc.NameTable );
			theManager.AddNamespace("link2", DocumentBase.XBRL_INSTANCE_URL);
			theManager.AddNamespace(DocumentBase.XBRL_LINKBASE_PREFIX, DocumentBase.XBRL_LINKBASE_URL);
			theManager.AddNamespace(DocumentBase.XLINK_PREFIX, DocumentBase.XLINK_URI);
			theManager.AddNamespace(DocumentBase.XBRLDI_PREFIX, DocumentBase.XBRLDI_URI);
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hasDimensionInfo">An output parameter.  True if </param>
		/// <param name="markupElements">The collection of <see cref="MarkupProperty"/> to be 
		/// reviewed.</param>
		/// <remarks>
		/// Duplicate markups are allowed as long as they have the same element, context, and
		/// markupdata. If the data in the cells is different, both are flagged as errors and don't
		/// get here. In any event, we need to remote the duplicates but append the cell address.
		/// </remarks>
		protected ArrayList FilterMarkups( Hashtable markupElements, out bool hasDimensionInfo  )
		{
			hasDimensionInfo = false;
			ArrayList markups = new ArrayList();
			string ignoreError = null;

			// ignore the return and string error - all we want is the attribute type
			object parsedObj = null;

			foreach ( MarkupProperty mp in markupElements.Values )
			{
				if (!hasDimensionInfo)
				{
					hasDimensionInfo = mp.contextRef.HasDimensionInfo();
				}

				if ( mp.element.MyElement.TryValidateElement( mp.markupData, ref parsedObj, out ignoreError ) )
				{
                    if (parsedObj == null)
                    {
                        mp.markupData = null;
                    }
				}

				int index = markups.BinarySearch( mp );
				if ( index < 0 )
				{
					markups.Insert( ~index, mp );
				}
				else
				{
                    if (string.IsNullOrEmpty(mp.TupleSetName))
                    {
                        MarkupProperty main = (MarkupProperty)markups[index];

                        // append the address
                        main.address += ", " + mp.address;


                        //TODO: need to merge the footnotes...
                        //as one markup might have the footnote and other one might need it
                        foreach (FootnoteProperty fp in new ArrayList( mp.Links ))
                        {
                            mp.Unlink(fp);
                            bool add = true;
                            foreach (FootnoteProperty fpm in main.Links)
                            {
                                if (fpm.Id == fp.Id ||  string.Equals( fpm.markupData, fp.markupData) )
                                {
                                    add = false;
                                    break;
                                }
                            }
                            if (add)
                            {
                                if (main.Id == null)
                                {
                                    main.Id = mp.MyKey;
                                }
                                main.Link(fp);

                            }
                        }
                    }
                    else
                    {
                        //multiples are allowed for tuples....
                        markups.Insert(index, mp);

                    }
					
				}

				//make sure id is unique
				if ( mp.Id != null && mp.Id != string.Empty)
				{
					int idCounter = 1;
					string newId = "Item-" + idCounter.ToString("0000");

					if (mpIDs.Contains(newId))
					{
						while (mpIDs.Contains(newId))
						{
							idCounter++;
							//id already exists, so loop to find a new one that is unique
							newId = "Item-" + idCounter.ToString("0000");
						}

						mpIDs.Add( newId );
                       
						//loop through all of the footnotes associated with this current markupProperty
						ArrayList footnotes = new ArrayList( mp._linker.links.Values );
						foreach ( FootnoteProperty linkedFN in footnotes )
						{
							//remove the link to the current footnote
							mp.Unlink(linkedFN);
						}

						// update markup id and re-link the footnotes.
						mp.Id = newId;
						foreach ( FootnoteProperty linkedFN in footnotes )
						{
							//link to the loaded footnote
							mp.Link( linkedFN );
						}
					}
				}
			}

			return markups;
		}

		
		/// <summary>
		/// Checks the following conditions: 
		///		contextID == newContextID with the same internal data
		///		contextID == newContextID with different internal data
		///		contextID != newContextID with the same internal data
		///		contextID != newContextID with different internal data
		///		
		///		It changes the MarkupProperty accordingly
		/// </summary>
		/// <param name="mp">The markup property to be verified.</param>
		protected void CheckDuplicateContexts( MarkupProperty mp )
		{
			ContextProperty cp = mp.contextRef;
			foreach ( ContextProperty existingCP in contexts )
			{
				// context ids are the same
				if ( string.Compare( existingCP.ContextID, cp.ContextID ) == 0 )
				{
					// duplicate id found
					if ( existingCP.ValueEquals( cp ) == false )
					{
						// but contains unique data, just give it a new id
						cp.ContextID = CreateUniqueName( cp.ContextID, contexts );

						// now rerun the check to see if data is duplicated
						CheckDuplicateContexts( mp );
						break;
					}
					else
					{
						// they are complete duplicates - they won't be written out twice
					}
				}
				else	// names aren't the same, is the data the same?
				{
					if ( existingCP.ValueEquals( cp ) == true )
					{
						// point to the context that already exists
						mp.contextRef = existingCP;

						break;
					}
				}
			}
		}

		private string CreateUniqueName( string baseName, ArrayList existingObjects )
		{
			string newName = null;
			int i = 2;

			do
			{
				newName = baseName + i.ToString();
				++i;
			}
			while ( existingObjects.BinarySearch( newName ) > -1 );

			return newName;
		}

		/// <summary>
		/// Ensures that the <see cref="UnitProperty"/> for a given <see cref="MarkupProperty"/> 
		/// has the same value as the equivalent <see cref="UnitProperty"/> is the units collection 
		/// of this <see cref="Instance"/>.  If the <see cref="UnitProperty"/> for the given 
		/// <see cref="MarkupProperty"/> does not exists in the units collection, it will be added.  
		/// If the unit ids of the two are not the same, the one in the units collection will be 
		/// updated.
		/// </summary>
		/// <param name="mp">The <see cref="MarkupProperty"/> whose <see cref="UnitProperty"/> is 
		/// to be checked.</param>
		protected void CheckDuplicateUnits( MarkupProperty mp )
		{
			UnitProperty up = mp.unitRef;

			// check to see if it's used
			int upIndex = units.BinarySearch( up.UnitID );
			if ( upIndex > -1 )
			{
				// yep, the unit is used, now check to ensure the units have the same internal data
				UnitProperty existingUP = (UnitProperty)units[upIndex];

				if ( !existingUP.ValueEquals( up ) )
				{
					//see if any of the other units can be used
					bool createNew = true;
					foreach (UnitProperty u in units)
					{
						if (up.ValueEquals(u))
						{
							up.UnitID = u.UnitID;

							createNew = false;
							break;
						}
					}

					if (createNew)
					{
						up.UnitID = CreateUniqueName(up.UnitID, units);
					}
				}
			}
			else
			{
				units.Insert( ~upIndex, up );
			}
		}

		/// <summary>
		/// Removes a parameter supplied <see cref="MarkupProperty"/>, including related 
		/// comments, from its parent <see cref="XmlNode"/>.
		/// </summary>
		/// <param name="mp">The <see cref="MarkupProperty"/> to be removed.</param>
		/// <remarks>If, by the deletion, the parent is empty, it will be removed from 
		/// it parent.</remarks>
		protected void RemoveMarkupInfo( MarkupProperty mp )
		{
			if (mp == null)
				return;

			XmlNode parent = mp.xmlElement.ParentNode;
			if (parent != null)
			{
				parent.RemoveChild( mp.xmlElement );
			}
			mp.xmlElement = null;

			foreach ( XmlComment comment in mp.xmlComments )
			{
				if (comment.ParentNode != null)
				{
					comment.ParentNode.RemoveChild( comment );
				}
			}

			mp.xmlComments.Clear();

			if ( mp.oldXmlComments.Count > 0 )
			{
				foreach ( XmlComment comment in mp.oldXmlComments )
				{
					if (comment.ParentNode != null)
					{
						comment.ParentNode.RemoveChild( comment );
					}
				}

				mp.oldXmlComments.Clear();
			}

			// I don't think this is necessary. By definition, a markup property is equal if it's element name 
			// and context property are equal - once a context is used, it will never be removed
			// mp.contextRef.RemoveXmlNode();

			// but units are a different story
			if (mp.unitRef != null)
				mp.unitRef.RemoveXmlNode();

			// BUG 1143 - if there are no child nodes then delete the parent too (clean up the empty tuple parent)
			if (parent != null && !parent.HasChildNodes)
			{
				XmlNode grandParent = parent.ParentNode;
				if (grandParent != null)
				{
					grandParent.RemoveChild( parent );
				}
			}
		}

		#region write context and unit info
		/// <summary>
		/// Appends a context node to a parameter supplied root element if it does not exist in <see cref="xDoc"/>.  
		/// Sets the "contextRef" attribute to reflect the context id of a parameter-supplied <see cref="ContextProperty"/>.  
		/// </summary>
		/// <param name="root">The document-level root element to which elements will be appended.</param>
		/// <param name="elem">The element whose "contextRef" attribute is to be set.</param>
		/// <param name="context">The context for which an element is to be added and for which the contexts collections is 
		/// to be updated.</param>
		/// <param name="taxonomies">A collection of <see cref="Taxonomy"/> used in writing segment and scenario information.</param>
		protected void AppendContextInfo(XmlElement root, XmlElement elem, ContextProperty context, Taxonomy[] taxonomies)
		{
			// context first
			string mergeKey = mergeDocs ? @"//link2:" : @"//";

			if ( this.xDoc.SelectSingleNode( string.Format( mergeKey + "context[@id=\"{0}\"]", context.ContextID ), theManager ) == null )
			{
				this.contexts.Add( context );
				XmlElement contextElem = mergeDocs ? context.AppendWithNamespaces(xDoc, root, taxonomies) : context.Append(xDoc, root, taxonomies);
				root.InsertAfter( contextElem, contextComment );
			}

			elem.SetAttribute( CONTEXT_REF, context.ContextID );
		}

		/// <summary>
		/// Appends unit nodes to a parameter supplied root element if they do not exist in <see cref="xDoc"/>.  
		/// Sets the "unitRef" attribute to reflect the unit id 
		/// of a parameter-supplied <see cref="UnitProperty"/>.  
		/// </summary>
		/// <param name="root">The document-level root element to which elements will be appended.</param>
		/// <param name="elem">The element whose attributes are to be set.</param>
		/// <param name="errors">A collection of <see cref="String"/> objects to which method 
		/// will append any errors.</param>
		/// <param name="presProp">A <see cref="Precision"/> from which precision attributes will be created.</param>
		/// <param name="unit">A <see cref="UnitProperty"/> from which unit elements will be created.</param>
		protected void AppendUnitInfo(XmlElement root, XmlElement elem, UnitProperty unit, Precision presProp, ArrayList errors)
		{
			elem.SetAttribute( UNIT_REF, unit.UnitID );

			if ( presProp != null )
			{
				elem.Attributes.Append( presProp.CreateAttribute( xDoc ) );
			}

			string mergeKey = mergeDocs ? @"//link2:" : @"//";
			if ( this.xDoc.SelectSingleNode( string.Format( mergeKey + "unit[@id=\"{0}\"]", unit.UnitID ), theManager ) == null )
			{
				XmlElement unitNode = null;
				if ( mergeDocs )
				{
					units.Add( unit );
					if ( unit.CreateElementWithNamespaces( xDoc, root, errors, out unitNode, this.writeComments ) )
					{
						root.InsertAfter( unitNode, unitComment );
					}	
				}
				else
				{
					if ( unit.CreateElement( xDoc, root, errors, out unitNode, this.writeComments ) )
					{
						root.InsertAfter( unitNode, unitComment );
					}	
				}
			}
		}
		#endregion

		#region schema ref and skeleton
		/// <summary>
		/// Appends a schema node to a parameter supplied root element if it does not exist in <see cref="xDoc"/>.  
		/// </summary>
		/// <param name="root">The document-level root element.</param>
		/// <param name="schemaName">The schema name to which the node is to refer.</param>
		/// <param name="previousElement">Use this as reference element to nsert the scheam ref element.</param>
		protected void AppendSchemaRef(XmlElement root, string schemaName, XmlElement previousElement)
		{
			// check to see if it already exists
			if ( this.xDoc.SelectSingleNode( string.Format( "//link:schemaRef[@xlink:href=\"{0}\"]", schemaName ), theManager ) == null )
			{
				// add it
				XmlElement schemaRef = xDoc.CreateElement(DocumentBase.XBRL_LINKBASE_PREFIX, SCHEMA_REF,
					DocumentBase.XBRL_LINKBASE_URL);
				XmlAttribute typeAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, TYPE, DocumentBase.XLINK_URI );
				typeAttr.Value = SIMPLE;
				schemaRef.SetAttributeNode( typeAttr );

				XmlAttribute hrefAttr = xDoc.CreateAttribute(DocumentBase.XLINK_PREFIX, HREF, DocumentBase.XLINK_URI);
				hrefAttr.Value = schemaName;
				schemaRef.SetAttributeNode( hrefAttr );

				previousElement.InsertAfter(schemaRef, null);	// insert it first
			}
		}

		/// <summary>
		/// Creates comment, tuple comment, and footnote comment elements and appends them to a parameter-supplied 
		/// <see cref="XmlElement"/>.
		/// </summary>
		/// <param name="root">The document-level root element to which elements will be appended.</param>
		/// <param name="hasTuples">If true, tuple comment elements will be appended.</param>
		/// <param name="hasFootnotes">If true, footnote comment elements will be appended.</param>
		protected void CreateInstanceSkeleton( XmlElement root, bool hasTuples, bool hasFootnotes )
		{
			contextComment = this.xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocContexts" ) );
			root.AppendChild( contextComment );

			unitComment = this.xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocUnits" ) );
			root.AppendChild( unitComment );

			if ( hasTuples )
			{
				tupleComment = this.xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocTuples" ) );
				root.AppendChild( tupleComment );
			}

			elementComment = this.xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocElements" ) );
			root.AppendChild( elementComment );

			footnoteComment = this.xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocFootnotes" ) );
			root.AppendChild( footnoteComment );
			
			if ( hasFootnotes )
			{
				CreateFootnoteSkeleton( root );
			}
		}

		/// <summary>
		/// Creates a footnote comment element and appends it to a parameter-supplied 
		/// <see cref="XmlElement"/>
		/// </summary>
		/// <param name="root">The document-level root element to which the footnote element 
		/// is to be appended.</param>
		protected void CreateFootnoteSkeleton(XmlElement root)
		{
			// and add the footnote link
			footnoteLink = xDoc.CreateElement(DocumentBase.XBRL_LINKBASE_PREFIX, FOOTNOTE, DocumentBase.XBRL_LINKBASE_URL);
			XmlAttribute footnoteType = xDoc.CreateAttribute(DocumentBase.XLINK_PREFIX, TYPE, DocumentBase.XLINK_URI);
			footnoteType.Value = EXTENDED;
			XmlAttribute footnoteRole = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, ROLE, DocumentBase.XLINK_URI );
			footnoteRole.Value = FOOTNOTE_LINK;

			footnoteLink.Attributes.Append( footnoteType );
			footnoteLink.Attributes.Append( footnoteRole );

			root.AppendChild( footnoteLink );
		}
		#endregion

		#region validation
		/// <summary>
		/// Validates key properties of a given <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="mp">The <see cref="MarkupProperty"/> to be validated.</param>
		/// <param name="errors">A collection of <see cref="String"/> objects to which method 
		/// will append any validation issues.</param>
		/// <returns>True if the properties of <paramref name="mp"/> are value.  False otherwise.</returns>
		protected bool TryValidateMarkup( MarkupProperty mp, ArrayList errors )
		{
			if ( mp.element == null || mp.contextRef == null )
			{
				Common.WriteError( string.Format( "Internal error: markup hashtable (address: {0}) contains a null {1}", mp.address, mp.element==null ? "element" : "context" ), errors );
				return false;
			}

			if ( mp.taxonomyIndex == -1 )
			{
				Common.WriteError( string.Format( "Internal error: markup object located at address {0} containing element name {1} contains an incorrect taxonomy index", mp.address, mp.element.Label ), errors );
				return false;
			}

			return true;
		}

		/// <summary>
		/// Validates key properties of <see cref="xDoc"/>.  Parses it and updates key properties of this <see cref="Instance"/>.
		/// </summary>
		/// <param name="errors">A collection of <see cref="String"/> objects to which this method will add any 
		/// errors encountered.</param>
		/// <returns>True if method was able to successfully validate and parse the <see cref="XmlDocument"/></returns>
		protected bool ValidateAndParse( ref ArrayList errors )
		{
			if ( errors == null )
				errors = new ArrayList();

			XmlElement root = xDoc.DocumentElement;
			if ( root.LocalName.CompareTo( XBRL ) != 0 )
			{
				bool isValidXBRLDocument = false;
				//it is possible that the doucment loaded is an inline xbrl document
				if (root.LocalName.ToLower().Equals(HTML))
				{
					//check if the inline xbrl namespace is defined ...
					foreach (XmlAttribute attr in root.Attributes)
					{
						if (attr.Value.ToLower().Equals(DocumentBase.INLINE_XBRL_URI.ToLower()))
						{
							isValidXBRLDocument = true;
							IsInLineXbrlDocument = true;

							break;
						}
					}
				}
				if( !isValidXBRLDocument )
				{
					Common.WriteError("XBRLParser.Error.InstanceDocNot2_1", errors);
					return false;

				}
			}

			

			if (IsInLineXbrlDocument)
			{
				//need to parse the document like it is an inline xbrl document.....

				return TryParseInlineXBRLDocument(ref errors);
			}


			// grab the attributes
			foreach (XmlAttribute attr in root.Attributes)
			{
				this.attributes.Add(attr.LocalName + "=" + attr.Value);
			}
			// parse out the schemaRefs and keep them
			XmlNodeList schemaNodes = xDoc.SelectNodes("//" + DocumentBase.XBRL_LINKBASE_PREFIX + ":" + SCHEMA_REF, theManager); 
			if ( schemaNodes == null )
			{
				Common.WriteError( "XBRLParser.Error.NoSchemaRefs", errors );
				return false;
			}
			else
			{
				foreach ( XmlNode schemaNode in schemaNodes )
				{
					this.schemaRefs.Add(schemaNode.Attributes[string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XLINK_PREFIX, HREF)].Value);
				}
			}

			// parse out the contexts and make sure they're valid
			XmlNodeList contextNodes = xDoc.SelectNodes( "//link2:" + ContextProperty.CONTEXT, theManager );
			if ( contextNodes != null )
			{
				foreach ( XmlNode context in contextNodes )
				{
					ContextProperty cp = null;

					if ( !ContextProperty.TryCreateFromXml( context, theManager, out cp, ref errors ) )
						return false;

					int index = contexts.BinarySearch( cp );

					if ( index < 0 )
					{
						this.contexts.Insert( ~index, cp );
					}
					else
					{
						Common.WriteError( "XBRLParser.Error.DuplicateContextIdFound", errors );
						return false;
					}
				}
			}

			// parse out the units
			XmlNodeList unitNodes = xDoc.SelectNodes( "//link2:" + UnitProperty.UNIT, theManager );
			if ( unitNodes != null )
			{
				foreach ( XmlNode unit in unitNodes )
				{
					UnitProperty up = null;
					if ( !UnitProperty.TryCreateFromXml( unit, theManager, out up, ref errors ) )
						return false;

					int index = units.BinarySearch( up );
					if ( index < 0 )
					{
						this.units.Insert( ~index, up );
					}
					else
					{
						Common.WriteError( "XBRLParser.Error.DuplicateUnitIdFound", errors );
						return false;
					}
				}
			}

			//read the footnotes
			Dictionary<string, List<FootnoteProperty>> footnoteInfos = ReadFootnoteDataFromDoc(root, 
				"//link:footnoteLink", true );

			// then the elements and tuples
			
			//reset the mpIDs array (an array of unique markupProperty ID values)
			this.mpIDs.Clear();

			for ( int i=0; i < root.ChildNodes.Count; ++i )
			{
				XmlNode child = root.ChildNodes[i];

				//footnoteLink nodes are processed as part of the element
				if ( child.LocalName == ContextProperty.CONTEXT || 
					 child.LocalName == UnitProperty.UNIT || 
					 child.LocalName == SCHEMA_REF ||
					 child.LocalName == FOOTNOTE ||
					 !(child is XmlElement) )
				{
					continue;
				}

				// It's an element or a tuple.  If it has a contextRef, it can't be a tuple.
				if ( child.Attributes[CONTEXT_REF] != null )
				{
					MarkupProperty mp = null;

					if ( !MarkupProperty.TryCreateFromXml( i, root.ChildNodes, contexts, units, out mp, ref errors ) )
					{
						continue;
					}

					mp.xmlElement = child;
					
					//check for footnotes
					XmlAttribute elementID = child.Attributes["id"];
					if (elementID != null)
					{
						ConnectFootnotes(mp,  elementID, footnoteInfos);
					}

					int mpIndex = markups.BinarySearch( mp );
					if ( mpIndex < 0 )
					{
						markups.Insert( ~mpIndex, mp );
					}
				}
				else
				{
                    TupleSet ts = null;
                    ArrayList tupleMarkups;
                    if (!TupleSet.TryCreateFromXml(child, contexts, units, out ts, out tupleMarkups, ref errors))
						continue;
                    this.markups.AddRange(tupleMarkups);
					//update the markups in the tupleset to  connect footnotes
                    foreach (MarkupProperty usedMP in tupleMarkups)
					{
                        //set the top level tupleset into each of the markup 
                        //so that it is easy to group them....
                        usedMP.TopLevelTupleset = ts;
						
						//check for footnotes
						XmlAttribute elementID = usedMP.xmlElement.Attributes["id"];
						if (elementID != null)
						{
							ConnectFootnotes(usedMP, elementID, footnoteInfos);
						}
					}
                    if (DocumentTupleList == null) DocumentTupleList = new List<TupleSet>();
                    DocumentTupleList.Add(ts);

					
				}
			}

			return true;
		}


		

		/// <summary>
		/// Reads all the footnotes defined in a parameter-supplied <see cref="XmlElement"/>.  Places those 
		/// footnotes into a returned dictionary.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="footnoteLinkString"></param>
		/// <param name="AddSuffix"></param>
		/// <returns></returns>
		private Dictionary<string, List<FootnoteProperty>> ReadFootnoteDataFromDoc(XmlElement root,
			string footnoteLinkString, bool AddSuffix  )
		{
			Dictionary<string, List<FootnoteProperty>> ret = new Dictionary<string, List<FootnoteProperty>>();

			XmlNodeList linkList = root.SelectNodes(footnoteLinkString, theManager);

            //if we have multiple footnotelinks it is possible for the footnoteid
            //to repeat. as each footnotelink can have the same footnote id
            Dictionary<XmlNode, string> suffixByFootnoteLink = new Dictionary<XmlNode, string>();
            if (linkList.Count > 1 && AddSuffix )
            {
                int counter = 1;
                foreach (XmlNode fl in linkList)
                {
                    suffixByFootnoteLink[fl] =      counter.ToString().PadLeft(4,'0');
                    counter++;
                }

            }


            XmlNodeList locList = root.SelectNodes("//link:footnote", theManager);
            foreach (XmlNode footnoteNode in locList)
			{
				string footnoteId = ((XmlElement)footnoteNode).GetAttribute("xlink:label");
                if (suffixByFootnoteLink.Count > 0)
                {
                    string suffix = string.Empty;
                    if (suffixByFootnoteLink.TryGetValue(footnoteNode.ParentNode, out suffix))
                    {
                        footnoteId += suffix;
                    }
                }
				string footnoteAddress = string.Empty;
				string footnoteLang = ((XmlElement)footnoteNode).GetAttribute("xml:lang");


				string footnoteData = ( (XmlElement)footnoteNode ).InnerXml;
				FootnoteProperty fp = new FootnoteProperty(footnoteId, footnoteAddress, footnoteLang, footnoteData);
				fnProperties.Add(footnoteId, fp);
			}

			locList = root.SelectNodes( footnoteLinkString  + "/link:loc", theManager);

			Dictionary<string, List<string>> labeltoElementIdMapDt = new Dictionary<string, List<string>>();
			foreach (XmlNode loc in locList)
			{
				string id = ((XmlElement)loc).GetAttribute("xlink:href");
				string[] idParts = id.Split('#');
				if (idParts != null && idParts.Length > 1)
				{
					id = idParts[1];
				}
				string label = ((XmlElement)loc).GetAttribute("xlink:label");
                if (suffixByFootnoteLink.Count > 0)
                {
                    string suffix = string.Empty;
                    if (suffixByFootnoteLink.TryGetValue(loc.ParentNode, out suffix))
                    {
                        label += suffix;
                    }
                }
				List<string> data;
				if (!labeltoElementIdMapDt.TryGetValue(label, out data))
				{
					data = new List<string>();
					labeltoElementIdMapDt[label] = data;
				}
				data.Add(id);

			}

			locList = root.SelectNodes("//link:footnoteArc", theManager);
			foreach (XmlNode arcs in locList)
			{
				string from = ((XmlElement)arcs).GetAttribute("xlink:from");
                if (suffixByFootnoteLink.Count > 0)
                {
                    string suffix = string.Empty;
                    if (suffixByFootnoteLink.TryGetValue(arcs.ParentNode, out suffix))
                    {
                        from += suffix;
                    }
                }
				string to = ((XmlElement)arcs).GetAttribute("xlink:to");
                if (suffixByFootnoteLink.Count > 0)
                {
                    string suffix = string.Empty;
                    if (suffixByFootnoteLink.TryGetValue(arcs.ParentNode, out suffix))
                    {
                        to += suffix;
                    }
                }
				FootnoteProperty fp = fnProperties[to] as FootnoteProperty;
				if (fp == null) continue;
				List<string> data;
				if (labeltoElementIdMapDt.TryGetValue(from, out data))
				{

					foreach (string id in data)
					{

						List<FootnoteProperty> fpsById;
						if (!ret.TryGetValue(id, out fpsById))
						{
							fpsById = new List<FootnoteProperty>();
							ret[id] = fpsById;
						}
						fpsById.Add(fp);

					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Connects footnotes to a markup, using the markup's element id.
		/// </summary>
		private void ConnectFootnotes(MarkupProperty mp, XmlAttribute elementID,
			Dictionary<string, List<FootnoteProperty>> footnoteInfos )
		{
			string id = elementID.Value;
			List<FootnoteProperty> relatedFootnotes;
			if (footnoteInfos.TryGetValue(id, out relatedFootnotes))
			{
				if (mp.Id == null)
					mp.Id = elementID.Value;

				//add the id to the arraylist (to check for dup IDs in FilterMarkups)
				int idx = mpIDs.BinarySearch(mp.Id);
				if (idx < 0)
				{
					mpIDs.Insert(~idx, mp.Id);
				}


				foreach (FootnoteProperty fp in relatedFootnotes)
				{
					

					//link the footnoteProperty to the markupProperty
					mp.Link(fp);

				}

			}

		}

	
		#endregion

		#region footnotes
		/// <summary>
		/// Adds footnote arcs to <see cref="XmlDocument"/> underlying this <see cref="Instance"/> 
		/// using the <see cref="LinkableProperty"/> objects within a parameter-supplied 
		/// collection of <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="mpElements">The collection of <see cref="MarkupProperty"/> objects 
		/// whose links will be used as the source of footnote arcs.</param>
		protected void AddFootnoteLocatorsAndArcs( ArrayList mpElements )
		{
			foreach ( MarkupProperty mp in mpElements )
			{
				AddFootnoteLocatorAndArcs( mp, this.footnoteLink );
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mp"></param>
		/// <param name="parentElement"></param>
		protected void AddFootnoteLocatorAndArcs(MarkupProperty mp, XmlElement parentElement)
		{
			if (mp.LinkCount > 0)
			{
				// add the locator
				AddFootnoteLocator(mp, parentElement);
			}

			// and add the arcs
			int order = 1;
			foreach (LinkableProperty lp in mp.Links)
			{
				AddFootnoteArc(mp.Id, lp.Id, order++, parentElement);
			}
		}

		private void AddFootnoteLocator(MarkupProperty mp, XmlElement parentElement)
		{
			XmlElement locator = null;
			locator = xDoc.CreateElement(DocumentBase.XBRL_LINKBASE_PREFIX, LOC, DocumentBase.XBRL_LINKBASE_URL);

			XmlAttribute typeAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, TYPE, DocumentBase.XLINK_URI );
			typeAttr.Value = LOCATOR;
			locator.SetAttributeNode( typeAttr );

			XmlAttribute hrefAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, HREF, DocumentBase.XLINK_URI );
			hrefAttr.Value = string.Format( HREF_FORMAT, string.Empty, mp.Id );
			locator.SetAttributeNode( hrefAttr );

			XmlAttribute labelAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, LABEL, DocumentBase.XLINK_URI );
			labelAttr.Value = string.Format( LABEL_FORMAT, mp.Id ); 
			locator.SetAttributeNode( labelAttr );

			parentElement.AppendChild(locator);

			if (writeComments && mp.xmlElement != null)
			{
				XmlComment locComment = xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocFootnoteMarkupAddress", mp.address, mp.xmlElement.Name ) );
				parentElement.InsertBefore(locComment, locator);
			}
		}


		private void AddFootnoteArc(string fromId, string toId, int order, XmlElement parentElement)
		{
			XmlElement arc = null;
			arc = xDoc.CreateElement(DocumentBase.XBRL_LINKBASE_PREFIX, FOOTNOTE_ARC, DocumentBase.XBRL_LINKBASE_URL);

			XmlAttribute typeAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, TYPE, DocumentBase.XLINK_URI );
			typeAttr.Value = ARC;
			arc.SetAttributeNode( typeAttr );

			XmlAttribute arcRoleAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, ARCROLE, DocumentBase.XLINK_URI );
			arcRoleAttr.Value = FOOTNOTE_ARCROLE;
			arc.SetAttributeNode( arcRoleAttr );

			XmlAttribute fromAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, FROM, DocumentBase.XLINK_URI );
			fromAttr.Value = string.Format( LABEL_FORMAT, fromId );
			arc.SetAttributeNode( fromAttr );

			XmlAttribute toAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, TO, DocumentBase.XLINK_URI );
			toAttr.Value = toId;
			arc.SetAttributeNode( toAttr );

			XmlAttribute orderAttr = xDoc.CreateAttribute( ORDER );
			orderAttr.Value = order.ToString();
			arc.SetAttributeNode( orderAttr );

			parentElement.AppendChild(arc);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mpElements"></param>
		protected void AddFootnoteResources( ArrayList mpElements )
		{
			foreach ( MarkupProperty mp in mpElements )
			{
				if ( mp.LinkCount > 0 )
				{
					foreach ( FootnoteProperty fp in mp.Links )
					{
						if ( !fp.HasBeenWritten )
						{
							AddFootnoteResource( fp, this.footnoteLink );
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fp"></param>
		/// <param name="parentEle"></param>
		protected void AddFootnoteResource( FootnoteProperty fp, XmlElement parentEle )
		{
			XmlElement resource = null;
			resource = this.xDoc.CreateElement(DocumentBase.XBRL_LINKBASE_PREFIX, FOOTNOTE_RESOURCE, DocumentBase.XBRL_LINKBASE_URL);

			XmlAttribute resAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, TYPE, DocumentBase.XLINK_URI );
			resAttr.Value = RESOURCE;
			resource.SetAttributeNode( resAttr );

			XmlAttribute resRoleAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, ROLE, DocumentBase.XLINK_URI );
			resRoleAttr.Value = FOOTNOTE_RESOURCE_ARCROLE;
			resource.SetAttributeNode( resRoleAttr );

			XmlAttribute labelAttr = xDoc.CreateAttribute( DocumentBase.XLINK_PREFIX, LABEL, DocumentBase.XLINK_URI );
			labelAttr.Value = fp.Id;
			resource.SetAttributeNode( labelAttr );

			XmlAttribute langAttr = xDoc.CreateAttribute(XML, LANG, DocumentBase.XBRL_INSTANCE_URL);
			langAttr.Value = fp.language;
			resource.SetAttributeNode( langAttr );


            try
            {
                resource.InnerXml = fp.markupData;
            }
            catch (Exception)
            {
                resource.InnerText = fp.markupData;
            }
            

			parentEle.AppendChild(resource);

			if ( writeComments )
			{
				XmlComment locComment = xDoc.CreateComment( TraceUtility.FormatStringResource( "XBRLParser.Info.InstanceDocMarkupAddress", fp.address ) );
				parentEle.InsertBefore(locComment, resource);
			}

			fp.HasBeenWritten = true;
		}
		#endregion


        #region embedded linkbases in instance doc

        internal string GetEmbeddedLinkbaseInfo()
        {
            StringBuilder ret  = new StringBuilder();
            XmlNodeList linkbaseNodes = xDoc.SelectNodes("//link:linkbaseRef", theManager);
            if (linkbaseNodes != null && linkbaseNodes.Count > 0)
            {
                ret.AppendLine("<annotation>");
                ret.AppendLine("<appinfo>");
                foreach (XmlNode node in linkbaseNodes)
                {
                    ret.AppendLine(node.OuterXml);
                }

                ret.AppendLine("</appinfo>");
                ret.AppendLine("</annotation>");


            }


            return ret.ToString();
        }
        #endregion


		#region Prefix Fix
		/// <summary>
		/// Fix the prefix in the instance document to match the taxonomy prefix defined in the taxonomy..
		/// </summary>
		/// <param name="taxonomies"></param>
		public void FixPrefixInInstanceDocument(ArrayList taxonomies)
		{
			Dictionary<string, string> prefixMap = BuildPrefixXref(taxonomies);

			if (prefixMap.Count > 0)
			{
				foreach (MarkupProperty mp in this.markups)
				{
					foreach (KeyValuePair<string, string> kvp in prefixMap)
					{
						if (mp.elementPrefix.Equals(kvp.Key))
						{
							mp.elementPrefix = kvp.Value;
							mp.elementId = string.Format(DocumentBase.ID_FORMAT, mp.elementPrefix, mp.elementName);

							if (mp.element != null)
							{
								mp.element.Id = mp.elementId;
								mp.element.MyElement.ProcessId();
							}
						}
						break;
					}

				}

				if (this.DocumentTupleList != null)
				{
					foreach (TupleSet ts in this.DocumentTupleList)
					{
						ts.FixPrefix(prefixMap);
					}
				}

			}
		}

		private  Dictionary<string, string> BuildPrefixXref(ArrayList taxonomies )
		{
			

			// build a hash of prefixes and namespaces in the taxonomy
			// key = path, value = prefix
			Dictionary<string, string> ret = new Dictionary<string, string>();
			Dictionary<string, string> taxPrefixDT = new Dictionary<string, string>();
			foreach (Taxonomy tax in taxonomies)
			{
				for (int i = 0; i < tax.TaxonomyItems.Length; i++)
				{
					taxPrefixDT[tax.TaxonomyItems[i].WebLocation] = tax.TaxonomyItems[i].Namespace;
				}
			}
			



			// loop through the instance namespaces and try to match the prefixes
			foreach (string uri in this.attributes)
			{
				// get the prefix from the namespace attribute
				string[] uriParts = uri.Split('=');
				string prefix = uriParts[0];
				string instanceNamespace = uriParts[1];

				if (prefix.CompareTo("xmlns") == 0)
				{
					// only process namespaces, excluding xmlns
					continue;
				}

				string taxPrefix;
				if (taxPrefixDT.TryGetValue(instanceNamespace, out taxPrefix))
				{
					if (prefix.CompareTo(taxPrefix) != 0)
					{
						ret[prefix] = taxPrefix;
					}
				}
				

			}

			return ret;
		}

		#endregion



		#region Inline XBRL Parsing

		private string inLineXBRLPrefix = "ix";
		private string xbrliPrefix = "xbrli";

		#region inline xbrl Header example region.....
		/*
		 
<ix:header>
- <ix:references>
  <link:schemaRef xlink:type="simple" xlink:href="http://www.sec.gov/Archives/edgar/data/789019/000119312508089475//msft-20080331.xsd" /> 
  </ix:references>
- <ix:resources>
- <xbrli:unit xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:html="http://www.w3.org/1999/xhtml" xmlns:this="this" id="shares">
  <xbrli:measure>xbrli:shares</xbrli:measure> 
  </xbrli:unit>
- <xbrli:unit xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:html="http://www.w3.org/1999/xhtml" xmlns:this="this" id="usd">
  <xbrli:measure>iso4217:USD</xbrli:measure> 
  </xbrli:unit>
- <xbrli:unit xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:html="http://www.w3.org/1999/xhtml" xmlns:this="this" id="pure">
  <xbrli:measure>xbrli:pure</xbrli:measure> 
  </xbrli:unit>
- <xbrli:unit xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:html="http://www.w3.org/1999/xhtml" xmlns:this="this" id="usdPerShare">
- <xbrli:divide>
- <xbrli:unitNumerator>
  <xbrli:measure>iso4217:USD</xbrli:measure> 
  </xbrli:unitNumerator>
- <xbrli:unitDenominator>
  <xbrli:measure>xbrli:shares</xbrli:measure> 
  </xbrli:unitDenominator>
  </xbrli:divide>
  </xbrli:unit>
- <xbrli:context id="c20080331_3_cik_00009869________">
- <xbrli:entity>
  <xbrli:identifier scheme="http://sec.gov/cik">00009869</xbrli:identifier> 
  </xbrli:entity>
- <xbrli:period>
  <xbrli:startDate>2008-01-01</xbrli:startDate> 
  <xbrli:endDate>2008-03-31</xbrli:endDate> 
  </xbrli:period>
  </xbrli:context>

- <xbrli:context id="c20060630_0_cik_00009869________">
- <xbrli:entity>
  <xbrli:identifier scheme="http://sec.gov/cik">00009869</xbrli:identifier> 
  </xbrli:entity>
- <xbrli:period>
  <xbrli:instant>2006-06-30</xbrli:instant> 
  </xbrli:period>
  </xbrli:context>
- <xbrli:context id="c20080331_0_cik_00009869_us_DeferredRevenueArrangementTypeAxis_us_SoftwareLicenseArrangementMember____">
- <xbrli:entity>
  <xbrli:identifier scheme="http://sec.gov/cik">00009869</xbrli:identifier> 
- <xbrli:segment>
  <xbrldi:explicitMember xmlns:xbrldi="http://xbrl.org/2006/xbrldi" dimension="us-gaap:DeferredRevenueArrangementTypeAxis">us-gaap:SoftwareLicenseArrangementMember</xbrldi:explicitMember> 
  </xbrli:segment>
  </xbrli:entity>
- <xbrli:period>
  <xbrli:instant>2008-03-31</xbrli:instant> 
  </xbrli:period>
  </xbrli:context>

  </ix:resources>
  </ix:header>

		 
		*/
		#endregion
		#region inline xbrl footnote example region....

		/*
		 <ix:footnoteLink xlink:type="extended" xlink:role="http://www.xbrl.org/2003/role/link">
  <link:loc xlink:type="locator" xlink:href="#d7e4732" xlink:label="d7e4732" /> 
  <link:footnoteArc xlink:type="arc" xlink:to="CorporateLevelActivity" xlink:from="d7e4732" order="1" xlink:arcrole="http://www.xbrl.org/2003/arcrole/fact-footnote" /> 
  </ix:footnoteLink>
		 
		 <ix:footnoteLink xlink:type="extended" xlink:role="http://www.xbrl.org/2003/role/link">
  <link:footnote xlink:type="resource" xlink:label="CorporateLevelActivity" xlink:role="http://www.xbrl.org/2003/role/footnote" xml:lang="en">Corporate-level activity excludes stock-based compensation expense and revenue reconciling amounts presented separately in those line items.</link:footnote> 
  </ix:footnoteLink>

		 
		 
		 */
		#endregion
		#region non fraction example
		/*
		<ix:nonFraction ix:name="us-gaap:SellingAndMarketingExpense" 
		 ix:format="commadot" ix:scale="6" decimals="-6" unitRef="usd" 
		 contextRef="c20080331_9_cik_00009869________">9,161
		</ix:nonFraction> */
		#endregion
		#region nonNumeric example
		/*
		<ix:nonNumeric ix:name="us-gaap:SignificantAccountingPoliciesTextBlock" contextRef="c20080331_9_cik_00009869________">
- <p class="c64">
  <span class="c16">Note 1 - Basis of Presentation and Consolidation and Recent Accounting Pronouncements</span> 
  </p>
- <p class="c124">
  <span class="c16">Basis of Presentation</span> 
  </p>
- <p class="c126">
- <span class="MsoNormal">
  <span class="c125">In the opinion of management, the accompanying balance sheets and related interim statements of income, cash flows, and stockholders' equity include all adjustments, consisting only of normal recurring items, necessary for their fair presentation in conformity with accounting principles generally accepted in the United States of America ("U.S. GAAP"). Preparing financial statements requires management to make estimates and assumptions that affect the reported amounts of assets, liabilities, revenue, and expenses. Examples include: estimates of loss contingencies, product warranties, product life cycles, product returns, and stock-based compensation forfeiture rates; assumptions such as the elements comprising a software arrangement, including the distinction between upgrades/enhancements and new products; when technological feasibility is achieved for our products; the potential outcome of future tax consequences of events that have been recognized in our financial statements or tax returns; estimating the fair value and/or goodwill impairment for our reporting units; and determining when investment impairments are other-than-temporary. Actual results and outcomes may differ from management's estimates and assumptions.</span> 
  </span>
  </p>
- <p class="c126">
- <span class="MsoNormal">
  <span class="c125">Interim results are not necessarily indicative of results for a full year. The information included in this Form 10-Q should be read in conjunction with information included in the Microsoft Corporation 2007 Form 10-K.</span> 
  </span>
  <span class="c16" /> 
  </p>
  </ix:nonNumeric>

		*/
		#endregion


		private bool TryParseInlineXBRLDocument(ref ArrayList errors)
		{
			//add the inline prefix to the namespace manager...
			XmlElement root = xDoc.DocumentElement;
			Dictionary<string, string> namespaceused = new Dictionary<string, string>();
			foreach (XmlAttribute attr in root.Attributes)
			{
				this.attributes.Add(attr.LocalName + "=" + attr.Value);

				if (attr.Value.ToLower().Equals(DocumentBase.INLINE_XBRL_URI.ToLower()))
				{
					inLineXBRLPrefix = attr.LocalName;
				}
				else if (attr.Value.ToLower().Equals(DocumentBase.XBRL_INSTANCE_URL.ToLower()))
				{
					xbrliPrefix = attr.LocalName;
				}


				namespaceused[attr.LocalName] = attr.Value;
			}

			theManager.AddNamespace(inLineXBRLPrefix, DocumentBase.INLINE_XBRL_URI);
			theManager.AddNamespace(xbrliPrefix, DocumentBase.XBRL_INSTANCE_URL);


			//first get the header region
			XmlNode xHeader = xDoc.SelectSingleNode("//"+inLineXBRLPrefix+":header", theManager);
			if (xHeader == null)
			{
				//failed to find the inline xbrl header.....
				Common.WriteError("XBRLParser.Error.NoInlineXBRLHeader", errors);
				return false;
			}
			//first get the schema nodes...

			// parse out the schemaRefs and keep them
			XmlNodeList schemaNodes = xHeader.SelectNodes("./" + inLineXBRLPrefix + ":references/link:schemaRef", theManager);
			if (schemaNodes == null)
			{
				Common.WriteError("XBRLParser.Error.NoSchemaRefs", errors);
				return false;
			}
			else
			{
				foreach (XmlNode schemaNode in schemaNodes)
				{
					this.schemaRefs.Add(schemaNode.Attributes[string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XLINK_PREFIX, HREF)].Value);
				}
			}

			// parse out the contexts and make sure they're valid
			XmlNodeList contextNodes = xHeader.SelectNodes("./" + inLineXBRLPrefix + ":resources/link2:" + ContextProperty.CONTEXT, theManager);
			if (contextNodes != null)
			{
				foreach (XmlNode context in contextNodes)
				{
					ContextProperty cp = null;

					if (!ContextProperty.TryCreateFromXml(context, theManager, out cp, ref errors))
						return false;

					int index = contexts.BinarySearch(cp);

					if (index < 0)
					{
						this.contexts.Insert(~index, cp);
					}
					else
					{
						Common.WriteError("XBRLParser.Error.DuplicateContextIdFound", errors);
						return false;
					}
				}
			}

			// parse out the units
			XmlNodeList unitNodes = xHeader.SelectNodes("./" + inLineXBRLPrefix + ":resources/link2:" + UnitProperty.UNIT, theManager);
			if (unitNodes != null)
			{
				foreach (XmlNode unit in unitNodes)
				{
					UnitProperty up = null;
					if (!UnitProperty.TryCreateFromXml(unit, theManager, out up, ref errors))
						return false;

					int index = units.BinarySearch(up);
					if (index < 0)
					{
						this.units.Insert(~index, up);
					}
					else
					{
						Common.WriteError("XBRLParser.Error.DuplicateUnitIdFound", errors);
						return false;
					}
				}
			}

			//read the footnotes
			Dictionary<string, List<FootnoteProperty>> footnoteInfos = ReadFootnoteDataFromDoc(root,
				"//" + inLineXBRLPrefix + ":footnoteLink", false);


			this.mpIDs.Clear();
			Dictionary<string, Dictionary<decimal, ITupleSetChild>> tupleMarkups = new Dictionary<string, Dictionary<decimal, ITupleSetChild>>();
			ParseInLineXBRLElementInformation(root, "//" + inLineXBRLPrefix + ":nonFraction", true,namespaceused, tupleMarkups, footnoteInfos, ref errors);
			ParseInLineXBRLElementInformation(root, "//" + inLineXBRLPrefix + ":nonNumeric", false,namespaceused, tupleMarkups, footnoteInfos, ref errors);



			#region Tuple handling
			//handling only tuple with IDs for now.. as there is no example for how 
			//tuples withput ids and nesting works correctly....

			Dictionary<string, TupleSet> tupleSetById = new Dictionary<string, TupleSet>();

			//List<XmlNode> tuplesWithoutIds = new List<XmlNode>();
			//tuples can be linked either with tupleRef or as children of the tuple element...
			XmlNodeList tupleNodes = root.SelectNodes("//" + inLineXBRLPrefix + ":tuple", theManager);
			if (tupleNodes != null && tupleNodes.Count > 0)
			{

				#region get all tuples with ID
				foreach (XmlNode tn in tupleNodes)
				{
					XmlAttribute nameAttr = tn.Attributes[inLineXBRLPrefix+":name"];
					if (nameAttr == null)
					{
						nameAttr = tn.Attributes["name"];
					}

					if (nameAttr == null)
					{
						errors.Add("Failed to parse " + tn.OuterXml);
						continue;

					}

					if (tn.Attributes[inLineXBRLPrefix + ":tupleID"] == null)
					{
						//the elements are nested inside the tuples....
						//continue;
					}
					else
					{


						TupleSet ts = new TupleSet();
						ts.Name = tn.Attributes[inLineXBRLPrefix + ":tupleID"].Value;
						ts.TupleParentElementId = nameAttr.Value.Replace(':', '_');

						tupleSetById[ts.Name] = ts;


						if (tn.Attributes[inLineXBRLPrefix + ":order"] != null && tn.Attributes[inLineXBRLPrefix + ":tupleRef"] != null)
						{
							decimal order = 0;
							if (decimal.TryParse(tn.Attributes[inLineXBRLPrefix + ":order"].Value, System.Globalization.NumberStyles.Any,
				null, out order))
							{
								//we have a nested tuple....as the tuple has a reference to another tuple...
								string tupleId = tn.Attributes[inLineXBRLPrefix + ":tupleRef"].Value;

								Dictionary<decimal, ITupleSetChild> inner;
								if (!tupleMarkups.TryGetValue(tupleId, out inner))
								{
									inner = new Dictionary<decimal, ITupleSetChild>();
									tupleMarkups[tupleId] = inner;
								}

								inner[order] = ts;
							}
						}
					}



				}




				#endregion

				#region add children to the tuple sets....
				foreach (KeyValuePair<string, TupleSet> kvp in tupleSetById)
				{
					TupleSet ts = kvp.Value;

					Dictionary<decimal, ITupleSetChild> children;
					if (tupleMarkups.TryGetValue(kvp.Key, out children))
					{
						foreach (KeyValuePair<decimal, ITupleSetChild> kvp2 in children)
						{
							ts.Children.Add((float)kvp2.Key, kvp.Value);

							if (kvp.Value is TupleSet)
							{
								((TupleSet)kvp.Value).ParentSet = ts;
							}
						}
					}
				}
				#endregion

				DocumentTupleList = new List<TupleSet>();
				#region get the top level tuple sets...
				foreach (KeyValuePair<string, TupleSet> kvp in tupleSetById)
				{
					if (kvp.Value.ParentSet == null)
					{
						kvp.Value.UpdateMarkupsWithTupleSetInformation(kvp.Value, new List<string>());
						DocumentTupleList.Add(kvp.Value);
					}
				}

				#endregion


			
			}
			#endregion

			return true;
		}



		private void ParseInLineXBRLElementInformation(XmlNode startingNode, string xpathInfo,
			bool isNumeric, Dictionary<string, string> namespaceused,
			Dictionary<string, Dictionary<decimal, ITupleSetChild>> tupleMarkups,
			Dictionary<string, List<FootnoteProperty>> footnoteInfos, ref ArrayList errors )
		{
			XmlNodeList selectedNodes = startingNode.SelectNodes(xpathInfo, theManager);
			if (selectedNodes != null && selectedNodes.Count > 0)
			{

				foreach (XmlNode aNode in selectedNodes)
				{
					MarkupProperty mp = null;

					if (!MarkupProperty.TryCreateFromInlineXbrl(aNode,inLineXBRLPrefix,xbrliPrefix,
						namespaceused, contexts, units, isNumeric, 
						tupleMarkups, out mp, ref errors))
					{
						continue;
					}


					//check for footnotes
					XmlAttribute elementID = aNode.Attributes["id"];
					if (elementID != null)
					{
						ConnectFootnotes(mp, elementID, footnoteInfos);
					}

					int mpIndex = markups.BinarySearch(mp);
					if (mpIndex < 0)
					{
						markups.Insert(~mpIndex, mp);
					}
				}

			}
		}
		#endregion



      
	}
}
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Exceptions;
using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Provides properties and methods to encapsulate an XBRL taxonomy.
	/// </summary>
	[Serializable]
	public partial class Taxonomy : DocumentBase
	{
		/// <summary>
		/// Enum defining the style that is used to present the Taxonomy.
		/// </summary>
		public enum PresentationStyle 
		{ 
			/// <summary> No style </summary>
			None,
			/// <summary> Style from Presentation linkbase </summary>
			Presentation,
			/// <summary> Style from Calculation linkbase </summary>
			Calculation,
			/// <summary> Style from Element linkbase </summary>
			Element 
		};

		/// <summary>
		/// delegate to let outside processes know this method's progress
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void ProcessingFileChangedEventHandler( object sender, ProcessingFileChangedEventArgs e );
		/// <summary>
		/// delegate and event to let outside processes know this method's progress
		/// </summary>
		public static event ProcessingFileChangedEventHandler ProcessingFileChanged;

		/// <summary>
		/// This class is used to add a message to the eventArgs class, so outside processes know ParseInternal's progress
		/// </summary>
		[Serializable]
		public class ProcessingFileChangedEventArgs : EventArgs
		{
			/// <summary>
			/// The message the consumer can use to display the progress of the parsing.
			/// </summary>
			public string textMessage;

			/// <summary>
			/// Public constructor of the changed event args.
			/// </summary>
			/// <param name="text">The text that can be displayed by the consumer of the process file changed event.</param>
			public ProcessingFileChangedEventArgs( string text )
			{
				textMessage = text;
			}
		}

		#region Constants
		/// <summary>
		/// The XBRL label role name for documentation.
		/// </summary>
		public const string DOCUMENTATION = "documentation";
		/// <summary>
		/// The XBRL label role name for Guidance.
		/// </summary>
		public const string GUIDANCE = "Guidance";

		/// <summary>
		/// The attribute name for the label preferred label attribute on a presentation arc.
		/// </summary>
		public const string PREFERRED_LABEL = "preferredLabel";

		

		/// <summary>
		/// The name of the element used to create a custom role in an XBRL extended link.
		/// </summary>
		private const string RTYPE_KEY = "//link:roleType";

		/// <summary>
		/// XPath query string to find "xsd:element" elements ) defined with a substitution group of "xbrli:tuple".
		/// </summary>
		private const string TUPLE_KEY = "//xsd:element[@substitutionGroup = \"xbrli:tuple\"]";

		/// <summary>
		/// XPath query string to find any "xsd:element" element with a "substitutionGroup"  attribute.
		/// </summary>
		private const string ELEM_NO_REFS_KEY = "//xsd:element[@substitutionGroup]";

		/// <summary>
		/// XPath query string to find "xsd:import" elements.
		/// </summary>
		private const string IMPORT_KEY = "//xsd:import";

		/// <summary>
		/// substitutionGroup attribute value that defines a tuple.
		/// </summary>
		private const string TUPLE_TAG = "xbrli:tuple";

		/// <summary>
		/// substitutionGroup attribute value for the "element" element that defines reference components
		/// for the reference linkbase.  
		/// </summary>
		private const string LINK_PART_TAG = "link:part";

		/// <summary>
		/// The name of the "id" attribute on the XBRL "element" element.
		/// </summary>
		private const string ID_TAG = "id";

		/// <summary>
		/// The name of the "name" attribute on the XBRL "element" element.
		/// </summary>
		private const string NAME_TAG = "name";

		/// <summary>
		/// Name of attribute on "element" element that defines substitution group.
		/// Does not include "xbrli" namespace qualifier.
		/// </summary>
		private const string SUBST_GROUP_TAG = "substitutionGroup";


		/// <summary>
		/// Name of the attribute on the "element" element that defines the domain for 
		/// a typed dimension.
		/// </summary>
		private const string XBRLDT_TYPEDDOMAINREF = "xbrldt:typedDomainRef";

		/// <summary>
		/// XPath query string to find "xsd:element" elements with a given "id" attribute value.
		/// </summary>
		/// <remarks>Used in finding domain elements for typed dimensions.</remarks>
		private const string XBRLDT_TYPEDDOMAINREF_XPATH = "//xsd:element[@id='{0}']";

		/// <summary>
		/// Name of XML schema element that defines a simple type.
		/// </summary>
		private const string SIMPLE_TYPE = "xsd:simpleType";

		/// <summary>
		/// Name of XML schema element that defines a complex type.
		/// </summary>
		private const string COMPLEX_TYPE = "xsd:complexType";

		/// <summary>
		/// XPath query string to find "xsd:simpleType" elements with a given "name" attribute
		/// value.
		/// </summary>
		private const string SIMPLE_TYPE_NAME = "//xsd:simpleType[@name='{0}']";

		/// <summary>
		/// XPath query string to find "xsd:complexType" elements with a given "name" attribute
		/// value.
		/// </summary>
		private const string COMPLEX_TYPE_NAME = "//xsd:complexType[@name='{0}']";

		/// <summary>
		/// The name of the "type" attribute
		/// </summary>
		/// <remarks>Used in the XBRL "element" element.</remarks>
		private const string TYPE_TAG = "type";

		/// <summary>
		/// The name of the "nillable" attribute
		/// </summary>
		/// <remarks>Used in the XBRL "element" element.</remarks>
		private const string NILLABLE_TAG = "nillable";

		/// <summary>
		/// The name of the "periodType" attribute on the XBRL "element" element.  Include "xbrli"
		/// namespace qualifier.
		/// </summary>
		private const string PERIOD_TYPE_TAG = "xbrli:periodType";

		/// <summary>
		/// The name of the "balance" attribute on the XBRL "element" element.  Include "xbrli"
		/// namespace qualifier.
		/// </summary>
		private const string BALANCE_TAG = "xbrli:balance";

		/// <summary>
		/// The name of the "abstract" attribute within the XML schema definition language (XSD) 
		/// that defines abstract types.
		/// </summary>
		private const string ABSTRACT_TAG = "abstract";

		/// <summary>
		/// The name of the "ref" attribute. 
		/// </summary>
		private const string REF_TAG = "ref";

		/// <summary>
		/// The name of the "choice" element.
		/// </summary>
		/// <remarks>The choice element is used in schema definition to define mutually exclusive 
		/// element types within a group.  Used in XBRL to define the elements within a tuple.</remarks>
		private const string CHOICE_TAG = "choice";

		/// <summary>
		/// The name of the "minOccurs" attribute within the XML schema definition language (XSD) 
		/// that defines the minimum number of times an element can occur.  Also used with XBRL
		/// to define the minimum number of times an element can occur with a tuple.
		/// </summary>
		private const string MIN_TAG = "minOccurs";

		/// <summary>
		/// The name of the "maxOccurs" attribute within the XML schema definition language (XSD) 
		/// that defines the maximum number of times an element can occur.  Also used with XBRL
		/// to define the maximum number of times an element can occur with a tuple.
		/// </summary>
		private const string MAX_TAG = "maxOccurs";

		/// <summary>
		/// The name of the "sequence" element.
		/// </summary>
		/// <remarks>The sequence element is used in schema definition to define the order of
		/// element types within a group.  Used in XBRL to define the element sequence within a tuple.</remarks>
		private const string SEQ_TAG = "sequence";

		/// <summary>
		/// The name of the attribute within the "roleType" element whose value contains the URI 
		/// of the role.
		/// </summary>
		private const string RURI_TAG = "roleURI";

		/// <summary>
		/// The name of the namespace attribute used within the "import" element 
		/// of an XML schema definition.
		/// </summary>
		private const string NS_TAG = "namespace";

		/// <summary>
		/// The name of the schema location attribute used within the "import" element 
		/// of an XML schema definition.
		/// </summary>
		private const string SL_TAG = "schemaLocation";

		/// <summary>
		/// The suffix with which the namespaces of instance documents end.
		/// </summary>
		/// <example>"http://www.xbrl.org/2003/instance" ends with the suffix "instance".</example>
		private const string INSTANCE_TAG = "instance";

		/// <summary>
		/// The suffix with which the namespaces of linkbase documents end.
		/// </summary>
		/// <example>"http://www.xbrl.org/2003/linkbase" ends with the suffix "linkbase".</example>
		private const string LINKBASE_TAG = "linkbase";


		/// <summary>
		/// The suffix with which the namespaces of dimension item (hypercubeItem and dimensionItem) definition documents
		/// end.
		/// </summary>
		/// <example>"http://xbrl.org/2005/xbrldt" ends with the suffix "xbrldt".</example>
		private const string DimensionTAG = "xbrldt";

		/// <summary>
		/// The suffix with which the namespaces of dimension item (not hypercubeItem or dimensionItem) definition documents
		/// end.
		/// </summary>
		/// <example>"http://xbrl.org/2006/xbrldi" ends with the suffix "xbrldi".</example>
		private const string DefinitionTAG = "xbrldi";

		/// <summary>
		/// The name of the "definition" element.
		/// </summary>
		/// <remarks>The definition element is used within XBRL to provide definitional information
		/// within an arcrole element.</remarks>
		private const string DEF_TAG = "definition";

		/// <summary>
		/// XPath query string to find "xsd:complexType" with a "@name" attribute.
		/// </summary>
		private const string COMPLEX_KEY_FOR_ENUM = "//xsd:complexType[@name]";

		/// <summary>
		/// XPath query string to find "xsd:restriction" elements.
		/// </summary>
		private const string RESTRICTION_KEY = ".//xsd:restriction";

		/// <summary>
		/// The name of the "base" attribute within the "element" attribute
		/// </summary>
		/// <remarks>Generally, the base attribute is used to define a base type
		/// for a new type in XML schema language.</remarks>
		private const string BASE_TAG = "base";

		/// <summary>
		/// XPath query string to find "xsd:enumeration" elements.
		/// </summary>
		private const string ENUM_KEY = ".//xsd:enumeration";

		/// <summary>
		/// The name of the value attribute used within the "xsd:enumeration"
		/// element.
		/// </summary>
		/// <remarks>Generally, the value attribute defines a value that an 
		/// individual component of an enumeration represents.</remarks>
		private const string VALUE_TAG = "value";

		private const string HEADER_STR = "AucentInternalTaxonomy";

		private const string LABEL_USED_ON = "link:label";

		internal const string NEGATED = "negated";
		internal const string NEGATED_TOTAL = "negatedTotal";
		internal const string NEGATED_PER_START = "negatedPeriodStart";
		internal const string NEGATED_PER_END = "negatedPeriodEnd";

        internal const string NEGATED2 = "negatedLabel";
        internal const string NEGATED_TOTAL2 = "negatedTotalLabel";
        internal const string NEGATED_PER_START2 = "negatedPeriodStartLabel";
        internal const string NEGATED_PER_END2 = "negatedPeriodEndLabel";



		internal const string LABEL = "label";
		internal const string LABEL_TOTAL = "totalLabel";
		internal const string LABEL_PER_START = "periodStartLabel";
		internal const string LABEL_PER_END = "periodEndLabel";

		#endregion

		#region delegates
		internal delegate bool BindElementDelegate( Element e );
		#endregion

		#region public properties
		/// <summary>
		/// A flag to tell if this taxonomy has a presentation linkbase or not.
		/// Set in <see cref="ParseExtensionTaxonomyFilenames"/>.
		/// </summary>
		public bool HasPresentation = false;

		/// <summary>
		/// A flag to tell if this taxonomy has a calculation linkbase or not.
		/// Set in <see cref="ParseExtensionTaxonomyFilenames"/>.
		/// </summary>
		public bool HasCalculation = false;
		#endregion

		#region Custom DataType Conversion

	
		internal Hashtable customDataTypesHash = new Hashtable();

		#endregion

		#region properties

		private string nsPrefix = null;

		/// <summary>
		/// The target namespace (a URL) for this taxonomy.
		/// </summary>
		internal string targetNamespace = null;

		/// <summary>
		/// The target namespace (a URL) for this taxonomy.
		/// </summary>
		public string TargetNamespace
		{
			get { return targetNamespace; }
			set { targetNamespace = value; }
		}

	

		/// <summary>
		/// The collection of calculation linkbases (URLs) for this taxonomy.
		/// </summary>
		internal string[] calculationFile;

		/// <summary>
		/// The collection of calculation linkbases (URLs) for this taxonomy.
		/// </summary>
		public string[] CalculationFile
		{
			get { return calculationFile; }
		}

		/// <summary>
		/// The collection of presentation linkbases (URLs) for this taxonomy.
		/// </summary>
		internal string[] presentationFile;

		/// <summary>
		/// The collection of presentation linkbases (URLs) for this taxonomy.
		/// </summary>
		public string[] PresentationFile
		{
			get { return presentationFile; }
		}

		/// <summary>
		/// The collection of reference linkbases (URLs) for this taxonomy.
		/// </summary>
		private string[] referenceFile;

		/// <summary>
		/// The collection of reference linkbases (URLs) for this taxonomy.
		/// </summary>
		public string[] ReferenceFile
		{
			get { return referenceFile; }
		}

		/// <summary>
		/// The collection of label linkbases (URLs) for this taxonomy.
		/// </summary>
		private string[] labelFile;

		/// <summary>
		/// The collection of label linkbases (URLs) for this taxonomy.
		/// </summary>
		public string[] LabelFile
		{
			get { return labelFile; }
		}

		/// <summary>
		/// The collection of definition linkbases (URLs) for this taxonomy.
		/// </summary>
		private string[] definitionFile;

		/// <summary>
		/// The collection of definition linkbases (URLs) for this taxonomy.
		/// </summary>
		public string[] DefinitionFile
		{
			get { return definitionFile; }
		}
		private bool innerTaxonomy = false;

		/// <key>title</key>
		/// <value>PresentationLink object</value>
		internal Hashtable presentationInfo;
		/// <summary>
		/// Public accessor for the internal presentationInfo.
		/// </summary>
		public Hashtable PresentationInfo
		{
			get { return presentationInfo; }
            set { presentationInfo = value; } 
		}

		internal Hashtable calculationInfo;
		/// <summary>
		/// Public accessor for the internal calculationInfo.
		/// </summary>
		public Hashtable CalculationInfo
		{
			get { return calculationInfo; }
		}

		/// <summary>
		/// The number of errors encountered while loading the presentation linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		private int numPresErrors;

		/// <summary>
		/// The number of errors encountered while loading the presentation linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		public int NumPresErrors
		{
			get { return numPresErrors; }
		}

		/// <summary>
		/// The number of errors encountered while loading the definition linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		private int numDefErrors;

		/// <summary>
		/// The number of errors encountered while loading the definition linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		public int NumDefErrors
		{
			get { return numDefErrors; }
		}

		/// <summary>
		/// The number of errors encountered while loading the calculation linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		private int numCalcErrors;

		/// <summary>
		/// The number of errors encountered while loading the calculation linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		public int NumCalcErrors
		{
			get { return numCalcErrors; }
		}

		/// <summary>
		/// The number of errors encountered while loading the label linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		private int numLabelErrors;

		/// <summary>
		/// The number of errors encountered while loading the label linkbases 
		/// in this and dependent taxonomies.
		/// </summary>
		public int NumLabelErrors
		{
			get { return numLabelErrors; }
		}

		/// <summary>
		/// The number of errors encountered while loading elements of the taxonomy
		/// and any dependent taxonomies.
		/// </summary>
		private int numElementErrors;

		/// <summary>
		/// The number of errors encountered while loading elements of the taxonomy
		/// and any dependent taxonomies.
		/// </summary>
		public int NumElementErrors
		{
			get { return numElementErrors; }
		}

		/// <summary>
		/// The number of errors encountered while loading items to which this 
		/// taxonomy refers.
		/// </summary>
		private int numRefErrors;

		/// <summary>
		/// The number of errors encountered while loading items to which this 
		/// taxonomy refers.
		/// </summary>
		public int NumReferenceErrors
		{
			get { return numRefErrors; }
		}


        /// <summary>
        /// we need to skip someomf the cleanup we do when a taxonomy is used as a cached base taxonomy 
        /// as it gets embedded into another tax and we want all the data structures to be intact.
        /// </summary>
        private bool usedAsCachedBaseTaxonomy = false;
        public bool UsedAsCachedBaseTaxonomy 
        {
            get { return usedAsCachedBaseTaxonomy; }
            set { usedAsCachedBaseTaxonomy = value;  }
        }

		/// <summary>
		/// Collection of <see cref="TaxonomyItem"/> of which this <see cref="Taxonomy"/> is comprised.
		/// </summary>
		internal List<TaxonomyItem> infos = new List<TaxonomyItem>();

		/// <summary>
		/// Equivalent to <see cref="infos"/> but the collection is returned as an 
		/// array of <see cref="TaxonomyItem"/> rather than an <see cref="ArrayList"/>.
		/// </summary>
		public TaxonomyItem[] TaxonomyItems
		{
			get { return infos.Count == 0 ? null : infos.ToArray(); }
		}


        public Hashtable CustomDataTypesHash
        {
            get { return customDataTypesHash; }
        }
		/// <summary>
		/// Collection of filenames of the taxonomies on which this <see cref="Taxonomy"/> is dependent.
		/// </summary>
		private List<string> dependantTaxonomyFilenames = new List<string>();

		/// <summary>
		/// Collection of <see cref="Taxonomy"/> objects on which this <see cref="Taxonomy"/> 
		/// depends.
		/// </summary>
		private ArrayList dependantTaxonomies = new ArrayList();


		private ArrayList validationErrors = new ArrayList();
		private ArrayList validationWarnings = new ArrayList();

		/// <summary>
		/// Collection of <see cref="String"/> containing the language
		/// code supported by the loaded taxonomy.
		/// </summary>
		private ArrayList supportedLanguages = new ArrayList();

		/// <summary>
		/// Collection of <see cref="String"/> containing the language
		/// code supported by the loaded taxonomy.
		/// </summary>
		public ArrayList SupportedLanguages
		{
			get { return supportedLanguages; }
			set { supportedLanguages = value; }
		}

		private bool DefinesCustomTypes
		{
			get
			{
				if (infos.Count > 0)
				{
					return infos[0].HasCustomTypes;
				}


				return false;
			}
		}

		/// <summary>
		/// Collection of <see cref="String"/> containing the loaded label roles 
		/// (e.g., "periodStartLabel", "documentation".
		/// </summary>
		internal ArrayList labelRoles = new ArrayList();

		/// <summary>
		/// Collection of <see cref="String"/> containing the loaded label roles 
		/// (e.g., "periodStartLabel", "documentation".
		/// </summary>
		public ArrayList LabelRoles
		{
			get { return labelRoles; }
			set { labelRoles = value; }
		}
		private static ArrayList skipLabelList = new ArrayList();
		private ArrayList allFiles;

		/// <summary>
		/// a dep tax that is not loaded as it is already loaded by another taxonomy...
		/// this is used by Dragon view to show the taxonomy hierarchy
		/// this is populated only if it initialized.
		/// </summary>
		public List<string> DirectDependantTaxonomies = null;

		/// Contains all elements in a flat list 
		/// <key>Element id (string)</key>
		/// <value>Element</value>
		internal Hashtable allElements;

		

		/// <key>label</key>
		/// <value>LabelLocator object</value>
		Hashtable tmplabelTable;
		//BUG 3558 - create a hash of labels with href (not label) as the key
		internal Hashtable labelHrefHash = new Hashtable();
		internal Hashtable referenceTable;

		/// <key>enum name</key>
		/// <value>ElementEnumerator object</value>
		internal Hashtable enumTable;

		/// <key>extended type name</key>
		/// <value>base type name</value>
		private Hashtable extendedDataMappings;

		internal string currentLanguage = null;
		internal string currentLabelRole = null;
		

		/// <summary>
		/// the load of this taxonomy resulted in these dependent taxonomy  to be loaded.
		/// </summary>
		private Taxonomy TopLevelTaxonomy = null;


        internal List<LinkbaseFileInfo> linkbaseFileInfos = new List<LinkbaseFileInfo>();
        /// <summary>
        /// get the list of linkbasefile infos in taxonomy...
        /// </summary>
        public List<LinkbaseFileInfo> LinkbaseFileInfos
        {
            get { return this.linkbaseFileInfos; }
        }

		/// <summary>
		/// Defines the possible <see cref="Taxonomy"/> validation status values.
		/// </summary>
		public enum ValidationStatus
		{
			/// <summary>
			/// Undefined.  
			/// </summary>
			UNDEFINED, // used to display folder icon in views if needed.

			/// <summary>
			/// Validated with no errors or warnings.
			/// </summary>
			OK,

			/// <summary>
			/// Validated with warnings.
			/// </summary>
			WARNING,

			/// <summary>
			/// Validated with errors.
			/// </summary>
			ERROR
		}

		internal bool aucentExtension = false;

		private ValidationStatus validationStatus = ValidationStatus.OK;

		/// <summary>
		/// Indicates the validation status of this <see cref="Taxonomy"/>.
		/// </summary>
		public ValidationStatus MyValidationStatus
		{
			// Set this based on the validation of the taxonomy.
			// Also this lines up to the icon image index used to display 
			// Taxonomies in the list and tree views.
			get { return validationStatus; }
			set { validationStatus = value; }
		}

		/// <summary>
		/// has all the role refs from the presentation , calculation and definition files...
		/// </summary>
		internal Dictionary<string, RoleRef> roleRefs = new Dictionary<string, RoleRef>();
		internal Dictionary<string, RoleType> roleTypes = new Dictionary<string, RoleType>();


        public Dictionary<string, RoleType> MyRoleTypes
        {
            get { return this.roleTypes; }
        }

        public Dictionary<string, RoleRef> MyRoleRefs
        {
            get { return roleRefs; }
        }

		Dimension netDefinisionInfo = null;
		/// <summary>
		/// Public accessor for the internal netDefinisionInfo member.
		/// </summary>
		public Dimension NetDefinisionInfo
		{
			get { return netDefinisionInfo; }
			set { netDefinisionInfo = value; }
		}

      

		private bool isCopiedForMerging = false;


		private bool skipAucentExtensions = false;



 		
		#endregion

		#region Properties that control behavior

		/// <summary>
		/// Static flag to tell if a reference to the Xml Document that was loaded
		/// is to be kept in memory when the taxonomy is closed.
		/// If this is false, we will null the xml document property in DocumentBase when closing.
		/// If this is true, we will keep a reference to the xml document. 
		/// Keep this static as we need this for the taxonomy as well as all the linkbase files..
		/// no point copying this member all over the place
		/// </summary>
		public static bool KeepXMLDocument = false;

		/// <summary>
		/// Static flag to tell if the dependent taxonomies list is cleared after loading and parsing the taxonomy.
		/// If this is false, we will clear the list of dependent taxonomies.
		/// If this is true, we will keep the list populated.
		/// </summary>
		public bool KeepInnerTaxonomies = false;

		/// <summary>
		/// Static flag to tell if the direct dependent taxonomies will be populated with
		/// dependent taxonomy file names when adding dependent taxonomy. 
		/// </summary>
		public  bool BuildTaxonomyRelationship = false;


		/// <summary>
		/// Interface that allows dependent taxonomy to used directly rather than loading it from a file.
		/// </summary>
		public static ITaxonomyCache TaxonomyCacheManager;
		#endregion

		#region Accessors

		/// <summary>
		/// Public accessor for the internal allElements member.
		/// </summary>
		public Hashtable AllElements
		{
			get { return allElements; }

			private set { allElements = value; }

		}

		/// <summary>
		/// Getter to tell if this taxonomy is an extension taxonomy generated by Dragon View product.
		/// </summary>
		public bool IsAucentExtension
		{
			get
			{
				CheckForAucentExtension();
				return aucentExtension;
			}
		}

		/// <summary>
		/// Gets or sets the current language property.
		/// The setter only takes the first 2 characters of the string passed in and ignore the rest.
		/// </summary>
		public string CurrentLanguage
		{
			get 
			{

				if (string.IsNullOrEmpty(currentLanguage))
				{
					if (supportedLanguages != null && supportedLanguages.Count > 0)
					{
						currentLanguage = supportedLanguages[0] as string;
					}
					else
					{

						currentLanguage = "en-US";
					}
				}
				return currentLanguage; 
			}
			set
			{
				if (!string.IsNullOrEmpty(value) )
				{
					currentLanguage = value;	// first two letters are the iso code, ignore anything else
				}
				else
				{
					currentLanguage = string.Empty;
				}
			}
		}

		/// <summary>
		/// Accessor for the internal currentLabelRole member.
		/// </summary>
		public string CurrentLabelRole
		{
			get { return currentLabelRole; }
			set { currentLabelRole = value; }
		}

		
		/// <summary>
		/// Taxonomy imported by the aucent extension taxonomy.  
		/// Used to seperate the UI of the base from the aucent extensions.
		/// </summary>
		public string TaxonomyImportedByAucentExtension = null;

		/// <summary>
		/// Getter for the internal dependantTaxonomies member.
		/// </summary>
		public ArrayList DependantTaxonomies
		{
			get { return dependantTaxonomies; }
		}

		/// <summary>
		/// Getter for the internal validationErrors member.
		/// </summary>
		public ArrayList ValidationErrors
		{
			get { return validationErrors; }
		}

		/// <summary>
		/// Getter for the internal validationWarnings member.
		/// </summary>
		public ArrayList ValidationWarnings
		{
			get { return validationWarnings; }
		}

		/// <summary>
		/// Flag to tell if the calculation is defined for this taxonomy.
		/// If the internal calculationInfo is null, this return false.
		/// If the internal calculationInfo is not null, this returns true.
		/// </summary>
		public bool CalculationIsDefined
		{
			get { return calculationInfo != null; }
		}

		/// <summary>
		/// Set the presentation, definition, calculation, label and reference file name lists.
		/// Also sets the HasPresentation and HasCalculation flags.
		/// </summary>
		public void ParseExtensionTaxonomyFilenames()
		{
			if ( targetNamespace == null )
			{
				GetTargetNamespace();

				// get the presentation file
				presentationFile = GetLinkbaseReference( TARGET_LINKBASE_URI + PRESENTATION_ROLE );
				if ( presentationFile != null && presentationFile.Length > 0 )
					HasPresentation = true;

				definitionFile = GetLinkbaseReference( TARGET_LINKBASE_URI + DEFINISION_ROLE );


				// get the calculation file
				calculationFile = GetLinkbaseReference( TARGET_LINKBASE_URI + CALCULATION_ROLE );
				if ( calculationFile != null && calculationFile.Length > 0 )
					HasCalculation = true;

				// get the label file
				labelFile = GetLinkbaseReference( TARGET_LINKBASE_URI + LABEL_ROLE );

				// get the reference file
				referenceFile = GetLinkbaseReference( TARGET_LINKBASE_URI + REFERENCE_ROLE );

                if (Taxonomy.TaxonomyCacheManager == null)
                {
                    if (!HasPresentation || !HasCalculation)
                    {
                        /* BUG 3406 - Per spec 5.1.2, if a dependent taxonomy has calculation then it must be
                         * included in the DTS, even if the current taxonomy does not have a calculation file. */

                        /* GetDependentTaxonomies populates the dependantTaxonomies array with the
                         * dependant taxonomy objects, which we can check for presentation/calculation. */
                        int errors = 0;
                        GetDependantTaxonomies(out errors);

                        foreach (Taxonomy dt in this.dependantTaxonomies)
                        {
                            int errs = 0;
                            dt.Load(dt.SchemaFile, out errs);
                            if (!HasPresentation &&
                                dt.presentationFile != null && dt.presentationFile.Length > 0)
                                HasPresentation = true;

                            if (!HasCalculation &&
                                dt.calculationFile != null && dt.calculationFile.Length > 0)
                                HasCalculation = true;

                            if (HasPresentation && HasCalculation)
                                break;
                        }
                    }
                }
			}
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Schema.
		/// </summary>
		public Taxonomy()
			: this( false )
		{
		}

		private Taxonomy( bool dependantArg )
		{
			innerTaxonomy = dependantArg;

			if ( !dependantArg )
			{
				allFiles = new ArrayList();
			}
		}

		static Taxonomy()
		{
			BuildSkipList();

		}
		#endregion

		/// <summary>
		/// A method to wrap the call to the event delegate for telling the outside world if
		/// which file is being processed on the load. It gets called to provide some progress status
		/// report of what is happening with the load. This wrapper checks if the ProcessingFileChanged
		/// event handler is not null and then calls the delegate. If it is null, nothing happens.
		/// </summary>
		/// <param name="tax"></param>
		/// <param name="e"></param>
		public static void OnProcessingFileChanged( Taxonomy tax, ProcessingFileChangedEventArgs e )
		{
			if ( ProcessingFileChanged != null )
			{
				ProcessingFileChanged( tax, e );
			}
		}

		/// <summary>
		/// This method loads and parses the rest of the supporting files 
		/// (e.g. Presentation, Calculation, Labels, Definition, etc.)
		/// and completes the parsing of the taxonomy. 
		/// It is called after the Taxonomy internal Xml Document is loaded.
		/// </summary>
		/// <param name="numErrors">The number of errors in parsing.</param>
		protected override void ParseInternal( out int numErrors )
		{
			ProcessingFileChangedEventArgs e = new ProcessingFileChangedEventArgs( "Processing " + this.SchemaFile + "..." );
			OnProcessingFileChanged( this, e );
			numErrors = 0;
			// if we've already been through, don't go through again
			if (this.allElements != null)
			{
				return;
			}

			int errors = 0;
			errorList.Clear();

			numWarnings = numErrors = 0;
			numPresErrors = numCalcErrors = numLabelErrors = numElementErrors = numDefErrors = 0;

			

			CheckForAucentExtension();

			GetTargetNamespace();

#if UNITTEST
			DateTime start = DateTime.Now;
#endif


			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
#if UNITTEST
			start = DateTime.Now;
#endif

			LoadImports( out errors );
			numErrors += errors;
			

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadImports", DateTime.Now - start );
			start = DateTime.Now;
#endif

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			e = new ProcessingFileChangedEventArgs( "Processing Presentation files for " + this.SchemaFile + "..." );
			OnProcessingFileChanged( this, e );

			errors = 0;
			LoadPresentation( out errors );

			/* BUG 3406 - Per spec 5.1.2, if a dependent taxonomy has presentation then it must be 
			 * included in the DTS, even if the current taxonomy does not have a presentation file. */
			if ( presentationInfo != null && presentationInfo.Count > 0 )
				HasPresentation = true;

			numErrors += errors;

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadPresentation", DateTime.Now - start );
			start = DateTime.Now;
#endif

			// loads the calculation linkbase for this taxonomy and merges the dependant taxonomy calculation linkbases
			e = new ProcessingFileChangedEventArgs( "Processing Calculation files for " + this.SchemaFile + "..." );
			OnProcessingFileChanged( this, e );

			errors = 0;
			LoadCalculation( out errors ); //TODO

			/* BUG 3406 - Per spec 5.1.2, if a dependent taxonomy has calculation then it must be 
			 * included in the DTS, even if the current taxonomy does not have a calculation file. */
			if ( calculationInfo != null && calculationInfo.Count > 0 )
				HasCalculation = true;

			numErrors += errors;

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadCalculation", DateTime.Now - start );
			start = DateTime.Now;
#endif


			if (!SkipDefinitionFileLoading)
			{

				LoadDefinition(out errors);
				numErrors += errors;

#if UNITTEST
				Console.WriteLine("File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadDefinitions", DateTime.Now - start);
				start = DateTime.Now;

#endif
            }





			#region might have to add the discovered taxonomies found in the dep taxonomy
			foreach (Taxonomy depTax in new ArrayList(this.dependantTaxonomies))
			{
				if (depTax.dependantTaxonomyFilenames != null &&
					depTax.dependantTaxonomies != null &&
					depTax.dependantTaxonomyFilenames.Count ==
					depTax.dependantTaxonomies.Count)
				{
					for (int i = 0; i < depTax.dependantTaxonomyFilenames.Count; i++)
					{

						this.MergeDiscoveredTaxonomy(depTax.dependantTaxonomyFilenames[i],
							depTax.dependantTaxonomies[i] as Taxonomy, out  errors);
						numErrors += errors;

					}
				}


			}
			#endregion



			// loads the labels for this taxonomy and merges the dependant taxonomy labels
			e = new ProcessingFileChangedEventArgs( "Processing Label files for " + this.SchemaFile + "..." );
			OnProcessingFileChanged( this, e );

			errors = 0;
			LoadLabels( out errors );
			numErrors += errors;

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadLabels", DateTime.Now - start );
			start = DateTime.Now;
#endif

			// loads the references for this taxonomy and merges the dependant taxonomy references
			e = new ProcessingFileChangedEventArgs( "Processing Reference files for " + this.SchemaFile + "..." );
			OnProcessingFileChanged( this, e );

			errors = 0;
			LoadReferences( out errors );
			numErrors += errors;

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadReferences", DateTime.Now - start );
			start = DateTime.Now;
#endif

			LoadRoleTypes(out errors);
			numErrors += errors;




			// loads the elements for this taxonomy and merges the dependant taxonomy elements
			errors = 0;
			LoadElements( out errors );
			numErrors += errors;

#if UNITTEST
			Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "LoadElements", DateTime.Now - start );
			start = DateTime.Now;
#endif




			if ( !innerTaxonomy )
			{
				foreach (Taxonomy depTax in this.dependantTaxonomies)
				{
					this.MergeRoleRefsFromTaxonomy(depTax);
					this.MergeRoleTypes(depTax);
                    this.MergeLinkbaseFileInfos(depTax);
				}



				if ( presentationInfo != null && presentationInfo.Count > 0 )
				{
					errors = 0;
					BindPresentationCalculationElements( true, out errors );
					numErrors += errors;

#if UNITTEST
					Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "BindElementToLocator", DateTime.Now - start );
					start = DateTime.Now;
#endif
				}

				if ( labelHrefHash != null && labelHrefHash.Count > 0 )
				{


					errors = 0;
					BindElements( new BindElementDelegate( BindLabelToElement ), out errors );

                    
					numErrors += errors;
                    //do not clear for a cached taxonomy...
                    if (!usedAsCachedBaseTaxonomy)
                        labelHrefHash.Clear();

#if UNITTEST
					Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "BindLabelToElement", DateTime.Now - start );
					start = DateTime.Now;
#endif

				}

				if ( referenceTable != null && referenceTable.Count > 0 )
				{
					errors = 0;
					BindElements( new BindElementDelegate( BindReferenceToElement ), out errors );
					numErrors += errors;

                    //do not clear for a cached taxonomy...
                    if (!usedAsCachedBaseTaxonomy)
                        referenceTable.Clear();

#if UNITTEST
					Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "BindReferenceToElement", DateTime.Now - start );
					start = DateTime.Now;
#endif
				}

				if ( calculationInfo != null && calculationInfo.Count > 0 )
				{
					errors = 0;
					BindPresentationCalculationElements( false, out errors );
					numErrors += errors;

#if UNITTEST
					Console.WriteLine( "File: {0} Method: {1} ElapsedTime: {2}", schemaFilename, "BindElementToCalculationLocator", DateTime.Now - start );
					start = DateTime.Now;
#endif
				}

				if ( netDefinisionInfo != null && netDefinisionInfo.DefinitionLinks.Count > 0 )
				{

					errors = 0;
					netDefinisionInfo.BindElementsToLocator( this.allElements );
					//update information on parse complete.....
					netDefinisionInfo.OnParseComplete();
					numErrors += errors;

				}


				if ( extendedDataMappings.Count > 0 ) //reassign data type for the elements that uses the extended types
				{

#if UNITTEST
					Console.WriteLine( "Begin Modify Element Types -- Extended Types:" + DateTime.Now.ToShortTimeString() );
#endif

					foreach ( Element e1 in this.allElements.Values )
					{
                        List<string> checkedTypes = new List<string>();
                        SetBaseType(e1, checkedTypes );

						
					}
#if UNITTEST
					Console.WriteLine( "End Modify Element Types -- Extended Types:" + DateTime.Now.ToShortTimeString() );

#endif

				}

				foreach ( Taxonomy depTax in this.dependantTaxonomies )
				{
					this.validationErrors.AddRange( depTax.validationErrors );
					this.ValidationWarnings.AddRange(depTax.ValidationWarnings);
					foreach ( string key in depTax.customDataTypesHash.Keys )
					{
						this.customDataTypesHash[key] = 1;
					}
				}




				if ( customDataTypesHash.Count > 0 )
				{

					//need to xpath into the xml doucment and read the information related to the
					//custom data type so that typed domain refs works...
					foreach ( string name in new ArrayList( customDataTypesHash.Keys ) )
					{
						this.customDataTypesHash[name] = LoadCustomDataType( name );
					}
				}

				//we want to call close on dep tax only after we get a chance to call
				//load custom data types as .. as the extended tax might depend on
				//a custom type created in the base taxonomy...
				foreach ( Taxonomy depTax in this.dependantTaxonomies )
				{
					depTax.Close();
				}

				//this is required for extensions to get created properly.
				UpdateAllRelativeRoleRefs();

				//fix substitution group 
				#region good place to set the correct substitution group and type of the element

				//substitution group needs to point back to 
				//item , tuple , dimenension, hypercube....

				//type can be nested...and we need to get the base type ....
				//for each element....
				foreach ( Element el in new ArrayList( AllElements.Values ) )
				{
					if ( !el.IsBaseSubstGroup() )
					{


						//recursvely walk up the chain till we find a base substitution group.
						Hashtable preventRecursion = new Hashtable();
						el.SetSubstitutionGroup( AllElements, preventRecursion );

						//the following code is commented out as
						//we have not been able to establish if this is a valid way to 
						//configure tuples
						//the german taxonomy has elements that support this theory
						//and also elements that do not support this theory
						//so the code is being commented out until we can figure out the expected behaviour
						//look at unit test  TestGermanCI_Presentation()
						#region move elements into tuples based on subst group

						//string otherElementId = el.OrigsubstGroup.Replace(":", "_");

						//Element origEle = allElements[otherElementId] as Element;

						//if (origEle == null) continue;


						//ArrayList allClonesOfOrig = new ArrayList();
						//allClonesOfOrig.Add(origEle);
						//allClonesOfOrig.AddRange(origEle.clonedElements);

						//foreach (Element master in allClonesOfOrig)
						//{

						//    if ( master.Parent != null)
						//    {

						//        Element child = el;
						//        Element parent = master.Parent;

						//        if (el.Parent != null)
						//        {
						//            if (el.Parent.Id == parent.Id)
						//            {
						//                //nothing to do here
						//                continue;
						//            }
						//            else
						//            {
						//                child = (Element)el.Clone();
						//            }
						//        }
						//        parent.AddChild(child);
						//        child.SetOccurances(master.MinOccurances, master.MaxOccurances);

						//        if (master.IsChoice)
						//        {
						//            //orig is a choice item , need to add the current item to the parent choice list

						//            parent.ChoiceContainer.TheChoices.Add(
						//                new Choice(parent.ChoiceContainer, child, child.MinOccurances, child.MaxOccurances));
						//        }



						//        // and remove this child from the hashtable
						//        if (boundElements.ContainsKey(child.Id))
						//        {
						//            boundElements.Remove(child.Id);
						//        }


						//    }
						//}

						#endregion

					}

				}


				#endregion

				//enums might exist in one dep tax 
				//and it might get used  in multiple other dep tax...
				//element enum information might not get populated ...
				//this might be a good place to correct it.
				RelinkEnumerationInformationWithElements();



                //this is a good place to switch out all the titles in the linkbases to the
                //definition of the role refs..
                ReplaceTitleWithRoleDefinition();


				this.Close();

				// and get rid of the dependant taxonomies
				if (!this.KeepInnerTaxonomies)
				{
					dependantTaxonomies.Clear();
				}


                //load rivet extensions if any is available
                LoadRivetExtensions();
			}

			
		}

		private void SetBaseType( Element e1 , List<string> checkedTypes )
		{
            //there is nothing to look up - exit recursion
			if( string.IsNullOrEmpty( e1.ElementType )  || checkedTypes.Contains( e1.ElementType) )
				return;

           
						
			//there was nothing found - exit recursion
			if( extendedDataMappings[ e1.ElementType ] == null )
				return;

			//remember this so that as we exit all of our loops
			//   we will ALWAYS have the original element type
			//SEE: finally{}
			string topMostType = e1.ElementType;

			try
			{
				//shift down from the current element...
				e1.OrigElementType = e1.ElementType;
				//... to the base element
				e1.ElementType = extendedDataMappings[ e1.ElementType ] as string;

                checkedTypes.Add(e1.ElementType);
				//when we do this recursively, we can get to the bottom-most element
				//  and its type
                SetBaseType(e1, checkedTypes);
			}
			finally
			{
				//remember this so that as we exit all of our loops
				//   we will ALWAYS have the original element type
				//SEE: topMostType = e1.ElementType;
				e1.OrigElementType = topMostType;
			}
		}

		/// <summary>
		/// Reads and parses an XML file.  Populating the <see cref="XmlDocument"/> underlying 
		/// this <see cref="DocumentBase"/>.
		/// </summary>
		/// <param name="filename">The file that is the be read and parsed.</param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered
		/// during the load.</param>
        public override bool Load(string filename, out int numErrors)
        {
            try
            {
                if (base.Load(filename, out numErrors))
                {
                    //populate the filenames for this taxonomy
                    ParseExtensionTaxonomyFilenames();

                    // make sure it is the version of XBRL we support
                    return ValidateXBRLVersion(theDocument, out numErrors);
                }
            }
            catch (XmlException xe)
            {
                numErrors = 1;
                Common.WriteError("XBRLParser.Error.Exception", errorList, xe.Message);
                return false;
            }
            catch (Exception xse)
            {
                numErrors = 1;
                Common.WriteError("XBRLParser.Error.Exception", errorList, xse.Message);
                return false;
            }

            return false;
        }
		private void CheckForAucentExtension()
		{
			if ( theDocument != null )
			{
				// the first comment should say Aucent if it's an Aucent extended taxonomy
				XmlNode comment1 = theDocument.ChildNodes[1];

				aucentExtension = false;

				if ( comment1 != null )
					aucentExtension = comment1.OuterXml.IndexOf(Aucent.MAX.AXE.Common.Utilities.AucentGeneral.RIVET_NAME) != -1;

			}
		}

		/// <summary>
		/// Checks the element id to see if it is already in the all elements hash table.
		/// </summary>
		/// <param name="id">The element id to be checked against the all elements hash.</param>
		/// <returns>True if the id is not already in the all elements hash.
		/// False if the element id already exists in the all elements hash.</returns>
		public bool IsUniqueElementId( string id )
		{
			return allElements.ContainsKey( id ) == false;
		}

		#region Load Definition



		/// <summary>
		/// DO NOT USE THIS PROPERTY: it is only used for Unit Testing purposes
		/// and that is the only reason it is made public.
		/// </summary>
		private static bool SkipDefinitionFileLoading = false;

		private void LoadDefinition( out int numErrors )
		{
			numErrors = 0;

			Dimension d = LoadDefinitionSchema( out numErrors );

			if ( d == null && innerTaxonomy )
				return;	// nothing to do

			if ( d == null )
			{
				d = new Dimension();
				d.DefinitionLinks = new Hashtable();
			}

			netDefinisionInfo = d;

			if ( !innerTaxonomy )
			{
				if ( dependantTaxonomies.Count > 0 )
				{
					foreach ( Taxonomy depTax in dependantTaxonomies )
					{
						ArrayList tmp;
						netDefinisionInfo.MergeDimensionLinks( depTax.netDefinisionInfo, out tmp );
						if ( tmp.Count > 0 )
						{
							numErrors += tmp.Count;
							errorList.AddRange( tmp );
						}

					}

				}
			}
			//if ( netDefinisionInfo != null )
			//{
			//    netDefinisionInfo.RemoveRecursiveLocator( ref numErrors );
			//}
			if ( numErrors > 0 )
			{
				numDefErrors += numErrors;
				errorList.AddRange( d.ErrorList );
			}

		}


		private Dimension LoadDefinitionSchema( out int numErrors )
		{
			numErrors = 0;
			definitionFile = GetLinkbaseReference( TARGET_LINKBASE_URI + DEFINISION_ROLE );

			if ( definitionFile == null )
				return null;

			Hashtable tempDefHash = new Hashtable();
			Dimension tempD = new Dimension();
            tempD.loadingTaxonomy = this.GetLoadingTaxonomy();
            tempD.PromptUser = this.PromptUser;
			foreach ( string file in definitionFile )
			{

				LoadDefinitionLinkbase( tempD, file, out numErrors );

				//build the presentation links
				if ( tempD.DefinitionLinks != null )
				{
					IDictionaryEnumerator linkEnumer = tempD.DefinitionLinks.GetEnumerator();
                    while (linkEnumer.MoveNext())
                    {
                        if (!tempDefHash.ContainsKey(linkEnumer.Key))
                        {
                            tempDefHash[linkEnumer.Key] = linkEnumer.Value;
                        }

                        else
                        {
                            //need to merge the info..
                            ArrayList tmp = new ArrayList();
                            DefinitionLink orig = tempDefHash[linkEnumer.Key] as DefinitionLink;
                            orig.Append(linkEnumer.Value as DefinitionLink, tmp);
                        }

                    }

				}

				//build the roleRefs
                this.MergeRoleRefsFromLinkbase(tempD.roleRefs, tempD.BaseSchema,
                     tempD.SchemaFile, LinkbaseFileInfo.LinkbaseType.Definition);
				tempD.roleRefs = null;

			}


			Dimension d = new Dimension();
			d.DefinitionLinks = tempDefHash;

			return d;

		}

      
		private void LoadDefinitionLinkbase( Dimension d, string linkbase, out int numErrors )
		{

			numErrors = 0;

			string baseUri = GetTargetNamespace();


			if ( baseUri[baseUri.Length - 1] != '/' && baseUri[baseUri.Length - 1] != '\\' )
			{
				baseUri += "/";
			}
			string bs = baseUri + schemaFilename;


			Dictionary<string, string> discoveredSchemas = new Dictionary<string, string>();

			if ( linkbase.StartsWith( "http" ) )
			{
				baseUri = linkbase.Replace( Path.GetFileName( linkbase ), string.Empty );
			}


			d.BaseSchema = bs;
			d.OwnerHandle = OwnerHandle;
			d.Load( linkbase );
			if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy &&
				d.IsDocumentCreatedByRivet())
			{
				//if this is a top level aucent extension taxonomy and ...
				//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
				//as we will merge the rivet created files later....
				return;
			}
			if ( d.ErrorList.Count > 0 )
			{
				for ( int i = 0; i < d.ErrorList.Count; i++ )
					Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
						SchemaFile + ": DEFINITION_ROLE Error: " + d.ErrorList[i] );
			}

			d.Parse( discoveredSchemas,
				out numErrors );


			foreach ( string val in discoveredSchemas.Values )
			{
				string filename = Path.GetFileName( val );
				string hintPath = val.Replace(filename, string.Empty);

				LoadAdditionalDependantTaxonomy(filename, hintPath, ref  numErrors);
				
			}
		}


		/// <summary>
		/// check if the taxonomy has segment / scenario definition information
		/// </summary>
		/// <param name="forSegment"></param>
		/// <returns></returns>
		public bool HasDimensionInfo( bool forSegment , bool  commonOnly)
		{
			if ( this.netDefinisionInfo == null )
				return false;

			return netDefinisionInfo.HasDimensionInfo(forSegment, commonOnly);
		}

		/// <summary>
		/// Uses the Dimension property of the taxonomy to get the common dimension nodes for display.
		/// This would be the method that would show the dimensions in the segments and scenarios tab.
		/// </summary>
        /// <param name="curLang">Which language to load.</param>
        /// <param name="curLabelRole">Which label role to display</param>
		/// <param name="isSegment"></param>
		/// <param name="dimensionNodes">The result list of dimension nodes.</param>
		/// <returns></returns>
		public bool TryGetCommonDimensionNodesForDisplay( string curLang, string curLabelRole,
			bool isSegment, out List<DimensionNode> dimensionNodes)
		{
            if (curLang == null)
            {
                if (this.currentLanguage != null)
                {
                    curLang = this.currentLanguage;

                }
                else
                {
                    if (this.supportedLanguages.Count > 0)
                    {
                        curLang = supportedLanguages[0] as string;
                    }
                    else
                    {
                        curLang = "en";
                    }

                }
            }

            if (curLabelRole == null)
            {
                curLabelRole = PresentationLocator.preferredLabelRole;
            }
			if (this.netDefinisionInfo != null)
			{
                Dictionary<string, DimensionNode> tmp;
                if (this.netDefinisionInfo.TryGetDimensionNodesForDisplay(curLang, curLabelRole,
					this.presentationInfo,
                    isSegment, false, this.roleTypes, out tmp))
                {
                    dimensionNodes = new List<DimensionNode>(tmp.Values);
                    //sort my name....
                    dimensionNodes.Sort();

                    return true;
                }
			}
			dimensionNodes = null;

			return false;
		}

		/// <summary>
		/// Uses the Dimension property of the taxonomy to get all the dimension nodes for display.
		/// </summary>
		/// <param name="currentLanguage">Which language to load.</param>
		/// <param name="currentLabelRole">Which label role to display</param>
		/// <param name="isSegment"></param>
		/// <param name="dimensionNodes">The result list of dimension nodes.</param>
		/// <returns></returns>
		public bool TryGetAllDimensionNodesForDisplay(string curLang, string curLabelRole,
			bool isSegment,   out List<DimensionNode> dimensionNodes)
		{
			if (this.netDefinisionInfo != null)
			{

				if (curLang == null)
				{
					if (this.currentLanguage != null)
					{
						curLang = this.currentLanguage;

					}
					else
					{
						if (this.supportedLanguages.Count > 0)
						{
							curLang = supportedLanguages[0] as string;
						}
						else
						{
							curLang = "en";
						}

					}
				}

				if (curLabelRole == null)
				{
					curLabelRole = PresentationLocator.preferredLabelRole;
				}


                Dictionary<string, DimensionNode> tmp;
				if (this.netDefinisionInfo.TryGetDimensionNodesForDisplay(curLang, curLabelRole,this.presentationInfo,
                    isSegment, true, this.roleTypes, out tmp))
                {
                    dimensionNodes = new List<DimensionNode>(tmp.Values);
                    //sort my name....
                    dimensionNodes.Sort();
                    return true;
                }

			}
			dimensionNodes = null;

			return false;
		}


		/// <summary>
		/// so that it gets rebuilt one more time...
		/// </summary>
		public void ClearDimensionNodeList()
		{
			if (netDefinisionInfo != null)
			{
				foreach (DefinitionLink dl in netDefinisionInfo.DefinitionLinks.Values)
				{
					dl.HyperCubeNodesList.Clear();
				}
			}
		}



		#endregion


		#region Load References

		private void LoadReferences( out int numErrors )
		{
			numErrors = 0;

			if ( dependantTaxonomies.Count == 0 )
			{
				int errs = LoadDependantTaxonomies(this.schemaPath);
				numErrors += errs;
				numRefErrors += errs;
			}

			if ( referenceFile == null )
			{
				referenceFile = GetLinkbaseReference( TARGET_LINKBASE_URI + REFERENCE_ROLE );
			}
			int errors = 0;

			if ( referenceTable == null )
			{
				referenceTable = new Hashtable();
			}

			if ( referenceFile != null )
			{
				foreach ( string refFile in referenceFile )
				{
					Reference reference = new Reference();
					reference.OwnerHandle = OwnerHandle;
                    reference.loadingTaxonomy = this.GetLoadingTaxonomy();
                    reference.PromptUser = this.PromptUser;

					if ( refFile != null )
					{
						reference.Load( refFile, out numErrors );
						if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy &&
							reference.IsDocumentCreatedByRivet())
						{
							//if this is a top level aucent extension taxonomy and ...
							//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
							//as we will merge the rivet created files later....
							continue ;
						}
						if ( reference.ErrorList.Count > 0 )
						{
							for ( int i = 0; i < reference.ErrorList.Count; i++ )
								Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
									SchemaFile + ": REFERENCE_ROLE Error: " + reference.ErrorList[i] );
						}


						reference.Parse( out errors );
						if ( errors > 0 )
						{
							numErrors += errors;
							errorList.AddRange( reference.ErrorList );
							numRefErrors += errors;
						}

                        LinkbaseFileInfo refLinkbaseInfo = new LinkbaseFileInfo();
                        refLinkbaseInfo.LinkType = LinkbaseFileInfo.LinkbaseType.Reference;
                        refLinkbaseInfo.Filename = refFile;
						refLinkbaseInfo.XSDFileName = schemaFile;

                        this.linkbaseFileInfos.Add(refLinkbaseInfo);

					}

					if ( reference.ReferencesTable != null )
					{
						#pragma warning disable 0219
						IDictionaryEnumerator enumer = reference.ReferencesTable.GetEnumerator();
						#pragma warning restore 0219

						foreach( ReferenceLocator otherRL in reference.ReferencesTable.Values )
						{
							if (referenceTable.Contains(otherRL.HRef))
							{
								//TODO: add merge code similar to label locators
								int i = 0; ++i;

								ReferenceLocator rl = (ReferenceLocator)referenceTable[otherRL.HRef];
								rl.Merge( otherRL );
							}
							else
							{
								referenceTable[otherRL.HRef] = otherRL;
							}
						}
					}
				}
			}

			if ( !innerTaxonomy )
			{
				//append the references to the root level referenceTable
				foreach ( Taxonomy t in dependantTaxonomies )
				{
					if ( t.referenceTable != null && t.referenceTable.Count > 0 )
					{
						errors = 0;
						// now append the references
						IDictionaryEnumerator enumer = t.referenceTable.GetEnumerator();
						while ( enumer.MoveNext() )
						{
							if ( referenceTable.ContainsKey( enumer.Key ) )
							{
								++numWarnings;
								Common.WriteWarning( "XBRLParser.Warning.DuplicateReference", errorList, enumer.Key.ToString() );
								continue;
							}

							// otherwise, add it
							referenceTable[enumer.Key] = enumer.Value;
						}
					}
				}
			}
		}

		#endregion

		#region Load Labels
		private void LoadLabels( out int numErrors )
		{
			numErrors = 0;
			int errors = 0;

			if ( dependantTaxonomies.Count == 0 )
			{
				int errs = LoadDependantTaxonomies(this.schemaPath);
				numErrors += errs;
				numLabelErrors += errs;
			}

			if ( labelFile == null )
			{
				labelFile = GetLinkbaseReference( TARGET_LINKBASE_URI + LABEL_ROLE );
			}

			if (tmplabelTable == null)
			{
				tmplabelTable = new Hashtable();

			}

			if ( labelFile != null )
			{
				//build the list of label files first..
				//as if there are multiple files then we want to load 
				//the extended label file first....
				List<Label> labelsToLoad = new List<Label>();

				foreach ( string labFile in labelFile )
				{
					if ( labFile == null || labFile.Length == 0 )
						continue;

					Label l = new Label();
					l.OwnerHandle = OwnerHandle;
                    l.loadingTaxonomy = this.GetLoadingTaxonomy();
                    l.PromptUser = this.PromptUser;
                    l.Load(labFile, out numErrors);
					if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy &&
						l.IsDocumentCreatedByRivet())
					{
						//if this is a top level aucent extension taxonomy and ...
						//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
						//as we will merge the rivet created files later....
						continue;
					}
					if ( l.ErrorList.Count > 0 )
					{
						for ( int i = 0; i < l.ErrorList.Count; i++ )
							Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
								SchemaFile + ": LABEL_ROLE Error: " + l.ErrorList[i] );

					}


					l.Parse( out errors );
					this.MergeLanguagesAndLabelRoles( l.SupportedLanguages, l.LabelRoles );
					if ( errors > 0 )
					{
						numErrors += errors;
						errorList.AddRange( l.ErrorList );
						numLabelErrors += errors;
					}

                    //add the lable file info to the linkbasefileinfo
                    LinkbaseFileInfo labLinkbaseInfo = new LinkbaseFileInfo();
                    labLinkbaseInfo.LinkType = LinkbaseFileInfo.LinkbaseType.Label;
                    labLinkbaseInfo.Filename = labFile;
					labLinkbaseInfo.XSDFileName = schemaFile;

                    this.linkbaseFileInfos.Add(labLinkbaseInfo);


					

					//TODO: Make sure we're not loading the same label file more than once

					if ( l.LabelTable != null )
					{
						bool added = false;
						if (!innerTaxonomy && this.IsAucentExtension )
						{
							//if this is the extended label file....
							if (l.IsDocumentCreatedByRivet())
							{
								labelsToLoad.Insert(0, l);
								added = true;
							}
							
						}
						if (!added)
						{
							labelsToLoad.Add(l);

						}
						
					}

				
				}

				foreach (Label l in labelsToLoad)
				{
					foreach (LabelLocator labelFileLL in l.LabelTable.Values)
					{

						if (tmplabelTable.Contains(labelFileLL.href))
						{
							//element already exists in labelTable, so add new labelLocator to the existing value
							ArrayList labelErrors = new ArrayList();
							LabelLocator ll = tmplabelTable[labelFileLL.href] as LabelLocator;
							ll.AddLabels(labelFileLL, labelErrors);
						}
						else
						{
							tmplabelTable[labelFileLL.href] = labelFileLL;
						}
					}
				}
			}

			if ( !innerTaxonomy )
			{
				labelHrefHash = new Hashtable();
				PopulateHrefHash(this.tmplabelTable);
				// and add the dependant taxonomy label files
				foreach ( Taxonomy t in dependantTaxonomies )
				{
					PopulateHrefHash(t.tmplabelTable);
					//not sure what the merged label table is goin to be used for
					//but for now leaving it alone as don't want to create any new impact
					errors = 0;


					//merge the supported languages and roles....
					this.MergeLanguagesAndLabelRoles( t.supportedLanguages, t.labelRoles );
				}
			}
		}

		private void MergeLanguagesAndLabelRoles(ArrayList newLanguages, ArrayList newRoles)
		{

			if ( newLanguages != null )
			{
				foreach ( string language in newLanguages )
				{
					int index = supportedLanguages.BinarySearch( language );

					if ( index < 0 )
					{
						bool foundCaseInsensitive = false;
						foreach ( string lang in supportedLanguages )
						{
							if ( string.Equals( lang, language, StringComparison.OrdinalIgnoreCase ) )
							{
								foundCaseInsensitive = true;
								break;
							}
						}

						if ( !foundCaseInsensitive )
						{

							this.supportedLanguages.Insert( ~index, language );
						}
					}
				}
			}

			if ( newRoles != null )
			{
				foreach ( string roleName in newRoles )
				{
					int index = labelRoles.BinarySearch( roleName );

					if ( index < 0 )
					{
						this.labelRoles.Insert( ~index, roleName );
					}
				}
			}
		}


		#endregion

		#region Load Elements
		public int LoadElements( out int numErrors )
		{
			numErrors = 0;

			if ( targetNamespace == null )
			{
				GetTargetNamespace();
			}

			AllocateElementTable();

			int errors = 0;
			LoadEnumerationsAndOtherExtendedDataTypes(out errors);
			numErrors += errors;
			numElementErrors += errors;

			//errors = 0;
			////LoadExtendedDataTypes( out errors );
			//numErrors += errors;
			//numElementErrors += errors;

			//get enumerations defined in dependent taxonomies so they can be connected to this taxonomy's elements
			for ( int i = 0; i < dependantTaxonomies.Count; ++i )
			{
				Taxonomy t = (Taxonomy)dependantTaxonomies[i];

				//as we loop through multiple dependent taxonomies there will be duplicates, so expect errors
				//set writeErrors = false to prevent reporting bogus errors
				int errs2 = MergeEnumerations( enumTable, t.enumTable, t.schemaFile, false );
				errs2 += MergeExtendedDataTypes( extendedDataMappings, t.extendedDataMappings );

				numErrors += errs2;
				numElementErrors += errs2;
			}

			LoadAllElements( out errors );
			numErrors += errors;
			numElementErrors += errors;

			int errs = BindTuples();
			numErrors += errs;
			numElementErrors += errs;


			// it is very important to read the dependant taxonomies back in the same order we originally read them in LoadImports
			// LoadImports adds TaxonomyItem objects and places them in the infos array
			// we read them back the same way so that the infos array stays in sync

			if ( !innerTaxonomy )
			{
				Dictionary<string, int> taxInofsDt = new Dictionary<string, int>();
				for( int i = 0 ; i < infos.Count ; i++ )
				{
					taxInofsDt[infos[i].WebLocation.ToLower()] = i;
				}

				for ( int i = 0; i < dependantTaxonomies.Count ; ++i )
				{
					Taxonomy t = (Taxonomy)dependantTaxonomies[i];

					if (t.schemaFile.Equals(this.schemaFile))
					{
						continue;

					}

					MergeElements(t, taxInofsDt);

					

					
				}
			}

			return allElements.Count;
		}

		internal void AllocateElementTable()
		{
			if ( allElements != null  )
			{
				//TODO: XBRLParser.Error.ReAllocationAttempt
				throw new AucentException( "element table already allocated" );
			}

			enumTable = new Hashtable();
			extendedDataMappings = new Hashtable();

			allElements = new Hashtable();	// memory isn't allocated - capacity is set

			// add the yes/no elements
			enumTable.Add( "xbrli:booleanItemType", Enumeration.CreateBooleanEnum( "xbrli:booleanItemType" ) );
			enumTable.Add( "booleanItemType", Enumeration.CreateBooleanEnum( "booleanItemType" ) );
		}

		private void LoadAllElements(out int numErrors)
		{
			numErrors = 0;
			if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy)
			{
				//if this is a top level aucent extension taxonomy and ...
				//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
				//as we will merge the rivet created files later....
				return;
			}


			XmlNodeList elemList;
			//			XmlNodeList elemList = theDocument.SelectNodes( TUPLE_KEY, theManager );

			// if the document is not xbrl SelectNodes will throw
			try
			{
				elemList = theDocument.SelectNodes(ELEM_NO_REFS_KEY, theManager);
			}
			catch
			{
				return;
			}

			Element parent = null;

			foreach (XmlNode elem in elemList)
			{

				try
				{
					parent = LoadBaseElement(elem);

					if (string.Compare(parent.SubstitutionGroup, LINK_PART_TAG, true) == 0)
					{
						Common.WriteInfo("XBRLParser.Info.IgnoreElementWithSubstGroupLinkPart", errorList, new string[] { elem.Name, LINK_PART_TAG });
						continue;
					}
					else if (string.Compare(parent.SubstitutionGroup, TUPLE_TAG, true) != 0)		// don't load additional info for tuples
					{
						numErrors += AddAdditionalElementInfo(parent, elem, errorList);
					}
				}
				catch (ApplicationException)
				{
					++numErrors;
				}

				parent.IsAucentExtendedElement = aucentExtension;

				// add the element to the hashtable
				allElements[parent.Id] = parent;

				if (!parent.NameMatchesId())
				{
					allElements[parent.GetNameWithNamespacePrefix()] = parent;
				}
			}

		}

		private int AddAdditionalElementInfo( Element e, XmlNode elem, ArrayList errorList )
		{
			string type = null;
			string nillable = "false";
			string ptype = "na";
			string bal = "na";
			string abst = "false";

			// get optional arguments
			if ( !Common.GetAttribute( elem, TYPE_TAG, ref type, null ) )
			{
				if( elem.ChildNodes != null )
				{
					foreach (XmlNode cn in elem.ChildNodes)
					{
						if (cn is XmlComment) continue;

						if (cn.Name.IndexOf(@"complexType") > 0)
						{
							XmlNode restrictionNode = cn.SelectSingleNode(RESTRICTION_KEY, theManager);

							if (restrictionNode != null)
							{
								type = restrictionNode.Attributes[BASE_TAG].Value;

							}
							
						}
					}


					if (type == null)
					{
						//Write it ourselves
						Common.WriteError("XBRLParser.Warning.NoTagForLocator2", errorList, TYPE_TAG, elem.OuterXml);
						return 1;
					}
				}
			
			}

			////fix type perfix to make sure it says xbrli if it is a xbrli type
			//if (!type.StartsWith("xbrli"))
			//{
			//    bool convertToxbrli= false;
			//    string[] parts = type.Split(':');
			//    if( parts.Length > 1 )
			//    {
			//    //it might be a different prefix but still uses the same namespace
			//    theManager.LookupNamespace( 
			//}
			Common.GetAttribute(elem, NILLABLE_TAG, ref nillable, null);
			Common.GetAttribute(elem, ABSTRACT_TAG, ref abst, null);
			if (this.HasStandardXBRLIPrefix)
			{
				Common.GetAttribute(elem, PERIOD_TYPE_TAG, ref ptype, null);
				Common.GetAttribute(elem, BALANCE_TAG, ref bal, null);
			}
			else
			{
				//convert the value retured by type...

				type = this.ConvertToXBRLIPrefixIfValidPrefix(type);
				Common.GetAttribute(elem, ConvertXBRLIToValidPrefix(PERIOD_TYPE_TAG), ref ptype, null);
				Common.GetAttribute(elem, ConvertXBRLIToValidPrefix(BALANCE_TAG), ref bal, null);

			}

			e.AddOptionals( type,
				Boolean.Parse( nillable ),
				(Element.PeriodType)Enum.Parse( typeof( Element.PeriodType ), ptype, true ),
				(Element.BalanceType)Enum.Parse( typeof( Element.BalanceType ), bal, true ),
				Boolean.Parse( abst ) );

			if ( type != null && enumTable.Count > 0 && enumTable.ContainsKey( type ) )
			{
				// set the enumeration
				e.EnumData = enumTable[type] as Enumeration;
			}

			return 0;
		}


        private void ReplaceTitleWithRoleDefinition()
        {
            if (this.presentationInfo != null)
            {
                foreach (PresentationLink pl in presentationInfo.Values)
                {
                    RoleType rt;
                    if (this.roleTypes.TryGetValue(pl.Role, out rt))
                    {
                        if (!string.IsNullOrEmpty(rt.Definition))
                        {
                            pl.Title = rt.Definition;

                        }

                    }
                }


            }

            if (this.calculationInfo != null)
            {
                foreach (PresentationLink pl in calculationInfo.Values)
                {
                    RoleType rt;
                    if (this.roleTypes.TryGetValue(pl.Role, out rt))
                    {
                        if (!string.IsNullOrEmpty(rt.Definition))
                        {
                            pl.Title = rt.Definition;

                        }

                    }
                }


            }

            if (netDefinisionInfo != null && netDefinisionInfo.DefinitionLinks != null)
            {
                foreach (DefinitionLink dl in netDefinisionInfo.DefinitionLinks.Values)
                {
                    RoleType rt;
                    if (this.roleTypes.TryGetValue(dl.Role, out rt))
                    {
                        if (!string.IsNullOrEmpty(rt.Definition))
                        {
                            dl.Title = rt.Definition;

                        }

                    }
                }



            }

        }


		private void RelinkEnumerationInformationWithElements()
		{

			foreach (Element ele in this.allElements.Values)
			{


				if (ele.EnumData == null && ele.OrigElementType != null )
				{
					ele.EnumData = enumTable[ele.OrigElementType] as Enumeration;

				}
			}

		}
		/// <summary>
		/// Gets the tuples out of the xsd, matches them up to those in the elements hash (we've already processed them)
		/// and reads in the children and binds them up (makes them children of the tuple parent
		/// </summary>
		/// <returns></returns>
		private int BindTuples()
		{
			if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy)
			{
				//if this is a top level aucent extension taxonomy and ...
				//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
				//as we will merge the rivet created files later....
				return 0;
			}
			int numErrors = 0;

			XmlNodeList elemList = null;
			if (this.HasStandardXBRLIPrefix)
			{
				elemList = theDocument.SelectNodes(TUPLE_KEY, theManager);
			}
			else
			{
				elemList = theDocument.SelectNodes(ConvertXBRLIToValidPrefix(TUPLE_KEY), theManager);

			}

			foreach ( XmlNode elem in elemList )
			{

				Element temp = LoadBaseElement( elem );
				// better find this guy

				Element tupleParent = allElements[temp.Id] as Element;
				if ( tupleParent == null )
				{
					Common.WriteError( "XBRLParser.Error.TupleParentNotFoundInAllElements", errorList, temp.Name );
					++numErrors;
					continue;
				}

				if ( elem.ChildNodes.Count > 0 )
				{
					numErrors += LoadChildren( elem.ChildNodes, tupleParent );
				}
			}

			return numErrors;
		}

		private XmlNode GetFirstNonCommentNode( XmlNodeList nodeList )
		{
			foreach ( XmlNode node in nodeList )
			{
				if ( node.NodeType != XmlNodeType.Comment ) return node;
			}

			return null;
		}

		private int LoadChildren( XmlNodeList childList, Element parent )
		{
			int numErrors = 0;
			XmlNode outerChild = GetFirstNonCommentNode( childList );


			while ( (outerChild != null) && (outerChild.Name.IndexOf( SEQ_TAG ) == -1)
				)
			{
				outerChild = outerChild.FirstChild;
			}
			if ( outerChild == null )
			{

				outerChild = GetFirstNonCommentNode( childList );
				while ( (outerChild != null) && (outerChild.Name.IndexOf( CHOICE_TAG ) == -1)
					)
				{
					outerChild = outerChild.FirstChild;
				}

				if ( outerChild == null )
				{
					Common.WriteError( "XBRLParser.Error.AdvancedTuplesNotSupported", errorList );
					++numErrors;
					return numErrors;
				}
				outerChild = outerChild.ParentNode;

			}

			// process the children of the tuple
			foreach ( XmlElement child in outerChild )
			{
				//ignore the comments
				if ( child.NodeType == XmlNodeType.Comment ) continue;


				if ( child.LocalName.CompareTo( CHOICE_TAG ) == 0 )
				{
					if ( !LoadChoices( child, parent, ref numErrors ) )
					{
						return numErrors;
					}
				}
				else
				{

					Element e = LoadChild( child, parent, ref numErrors );

					if ( e != null )
					{
						int minOccurs = 0;
						int maxOccurs = 0;

						GetOccurances( child, out minOccurs, out maxOccurs );

                        parent.AddChildInfo( e.Id, minOccurs, maxOccurs);
					}


				}
			}

			return numErrors;
		}

		private bool LoadChoices( XmlNode choice, Element parent, ref int numErrors )
		{
			// create the new choice
			int minOccurs = 0;
			int maxOccurs = 1;

			GetOccurances( (XmlElement)choice, out minOccurs, out maxOccurs );

			if ( parent.ChoiceContainer != null )
			{
				Common.WriteError( "XBRLParser.Error.ChoiceContainerFull", errorList, parent.Id );

				++numErrors;
				return false;
			}

			parent.ChoiceContainer = new Choices( minOccurs, maxOccurs );

			foreach ( XmlElement child in choice )
			{
				int elemMinOccurs = 0;
				int elemMaxOccurs = 0;

				Element e = LoadChild( child, parent, ref numErrors );
				GetOccurances( child, out elemMinOccurs, out elemMaxOccurs );

				if ( e == null )
				{
					return false;
				}

				parent.ChoiceContainer.TheChoices.Add( new Choice( parent.ChoiceContainer, e, elemMinOccurs, elemMaxOccurs ) );
			}

			return true;
		}

		private Element LoadChild( XmlElement child, Element parent, ref int numErrors )
		{
			string elementRef = null;

			if ( !Common.GetAttribute( child, REF_TAG, ref elementRef, null ) )
			{
				if ( child.Name == "attribute" )
				{
					Common.WriteWarning( "Tuple has an attribute defined and Parser does not read attributes " + child.OuterXml, errorList, child.OuterXml );
					return null;
				}

				// write our own error
				Common.WriteError( "XBRLParser.Warning.NoTagForLocator2", errorList, REF_TAG, child.OuterXml );
				++numErrors;
				return null;
			}

			// lookup the element
			string[] refStrings = elementRef.Split( ':' );
			//
			//				if ( refStrings.Length != 2 )
			//				{
			//					Common.WriteError( "XBRLParser.Warning.IncorrectRefFormat", errorList, elementRef, refStrings.Length.ToString(), child.OuterXml );
			//					++numErrors;
			//					continue;
			//				}

			string ns = theManager.LookupNamespace( theManager.NameTable.Get( refStrings[0] ) );

			if ( ns == null )
			{
				Common.WriteError( "XBRLParser.Warning.IncorrectChildNamespace", errorList, new string[] { refStrings[0], elementRef, schemaFile, child.OuterXml } );
				++numErrors;
				return null;
			}

			string elemId = elementRef.Replace( ':', '_' );

			Element e = allElements[elemId] as Element;

			// TODO: stupid check?
			// No, not stupid - we need to bind the element that is defined in another schema to this parent,
			// but we have no way to get the other element
			if ( ns != targetNamespace )
			{
				// see if it's in one of the dependant taxonomies
				foreach ( Taxonomy t in dependantTaxonomies )
				{
					// see if it's in all elements, if so, return it
					if (t.allElements == null) continue;
					if ( (e = t.allElements[elemId] as Element) != null )
					{
						break;
					}
				}

				if ( e == null )
				{

					if (this.TopLevelTaxonomy != null)
					{
						foreach (Taxonomy t in TopLevelTaxonomy.dependantTaxonomies)
						{
							// see if it's in all elements, if so, return it
							if (t.allElements == null) continue;
							if ((e = t.allElements[elemId] as Element) != null)
							{
								break;
							}
						}
					}

					if (e == null)
					{
						// if we made it here there is no element that matches
						Common.WriteError("XBRLParser.Warning.NotSupportedRemoteLinks", errorList, new string[] { refStrings[0], elementRef, schemaFile, child.OuterXml });
						++numErrors;
						return null;
					}
				}
			}

			// lookup the child node in the element table
			// this ensures we always get the element, even though it may have been removed from the 

			if ( e == null )
			{
				if ( refStrings.Length == 1 )
				{
					Common.WriteError( "XBRLParser.Warning.IncorrectRefFormat", errorList, elementRef, refStrings.Length.ToString(), child.OuterXml );
					++numErrors;
					return null;
				}
				else
				{
					e = allElements[refStrings[1]] as Element;

					if ( e == null )
					{
						Common.WriteError( "XBRLParser.Error.ComplexElementsNotSupported", errorList, elemId, elementRef );
						++numErrors;
						return null;
					}
				}
			}
			
			//e.Parent = parent;

			parent.AddChild( e );

			

			return e;
		}

		private void GetOccurances( XmlElement child, out int minOccurs, out int maxOccurs )
		{
			string min = "0";
			string max = int.MaxValue.ToString();

			Common.GetAttribute( child, MIN_TAG, ref min, null );
			Common.GetAttribute( child, MAX_TAG, ref max, null );

			minOccurs = int.Parse( min );

			if ( string.Compare( max, Element.UNBOUNDED, true ) != 0 )
			{
				maxOccurs = int.Parse( max );
			}
			else
			{
				maxOccurs = int.MaxValue;
			}
		}

		private Element LoadBaseElement( XmlNode elem )
		{
			string id = null;
			string name = null;
			string substGroup = null;

			// id is optional
			Common.GetAttribute( elem, ID_TAG, ref id, null );

			if ( !Common.GetAttribute( elem, NAME_TAG, ref name, errorList ) ||
				!Common.GetAttribute( elem, SUBST_GROUP_TAG, ref substGroup, errorList )  )
			{
				throw new AucentException( "XBRLParser.Error.NoNameOrSubstitutionGroup" );
			}
			if (!this.HasStandardXBRLIPrefix)
			{

				substGroup = ConvertToXBRLIPrefixIfValidPrefix(substGroup);
			}

			if ( id == null )
			{
				Common.WriteWarning( "XBRLParser.Warning.NoIDForElement", errorList, name );
				++numWarnings;
			}


			Element el = new Element( id, name, substGroup );

			el.CreateNameWithNamespacePrefix(this.nsPrefix);
			
			if ( el.SubstitutionGroup == Element.DIMENSION_ITEM_TYPE )
			{
				//check if it is a typed dimension and if it is then need to set the name 
				string typedDimensionId = null;
				if ( Common.GetAttribute( elem, XBRLDT_TYPEDDOMAINREF, ref typedDimensionId, null ) )
				{
					if ( typedDimensionId != null )
					{
						if ( typedDimensionId.IndexOf( '#' ) >= 0 )
						{
							string[] vals = typedDimensionId.Split( '#' );
							if ( vals.Length == 2 )
							{
								typedDimensionId = vals[1];
							}
						}
						//add name to the list of complex type to load...
						el.TypedDimensionId = typedDimensionId;

						customDataTypesHash[typedDimensionId] = 1;
					}
				}
			}

			return el;
		}



		internal void LoadEnumerationsAndOtherExtendedDataTypes( out int errors )
		{
			errors = 0;
			if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy)
			{
				//if this is a top level aucent extension taxonomy and ...
				//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
				//as we will merge the rivet created files later....
				return ;
			}
			
			XmlNodeList enumNodes = theDocument.SelectNodes( COMPLEX_KEY_FOR_ENUM, theManager );
			if ( enumNodes != null && enumNodes.Count > 0 )
			{
				this.infos[0].HasCustomTypes = true;
				foreach ( XmlNode complexNode in enumNodes )
				{
					// create an enumeration object
					Enumeration e = new Enumeration();
					XmlAttribute ComplexNodeName = complexNode.Attributes[NAME_TAG];

					e.Name = string.Format(DocumentBase.NAME_FORMAT, GetNSPrefix(), ComplexNodeName.Value);

					// look for a restriction
					XmlNode restrictionNode = complexNode.SelectSingleNode( RESTRICTION_KEY, theManager );

					if ( restrictionNode != null )
					{
						e.RestrictionType = restrictionNode.Attributes[BASE_TAG].Value;
                        extendedDataMappings[e.Name] = e.RestrictionType;
					}

					// now go through the enumerations
					XmlNodeList enums = complexNode.SelectNodes( ENUM_KEY, theManager );
					if ( enums == null || enums.Count == 0 )
					{
						//need to load other information regarding this complex type..
						
						continue;
					}

					foreach ( XmlNode enumVal in enums )
					{
						e.Values.Add( enumVal.Attributes[VALUE_TAG].Value );
					}

					// and add it to the hashtable
					enumTable[e.Name] = e;
				}
			}

		}


		private XmlNode GetNodeFromTaxonomyByIdRef( string name )
		{

			string path = string.Format( XBRLDT_TYPEDDOMAINREF_XPATH, name );
			return GetNodeFromTaxonomy( path );
		}

		private XmlNode GetSimpleTypeNodeFromTaxonomyByName( string name )
		{

			string path = string.Format( SIMPLE_TYPE_NAME, name );
			return GetNodeFromTaxonomy( path );
		}

		private XmlNode GetComplexTypeNodeFromTaxonomyByName( string name )
		{

			string path = string.Format( COMPLEX_TYPE_NAME, name );
			return GetNodeFromTaxonomy( path );
		}

		private XmlNode GetNodeFromTaxonomy( string path )
		{

			XmlNode node = this.theDocument.SelectSingleNode( path, theManager );

			if ( node == null )
			{
				foreach ( Taxonomy depTax in this.dependantTaxonomies )
				{
					if ( depTax.theDocument != null )
					{
						node = depTax.GetNodeFromTaxonomy( path );
						if ( node != null ) return node;
					}

				}
			}

			return node;
		}


		private IXBRLCustomType LoadCustomDataType( string name )
		{

			XmlNode node = GetNodeFromTaxonomyByIdRef( name );



			if ( node != null )
			{
				//four possible routes...
				//custom type is a simple type and the type attribute defines the base type
				//custom type has a child element called simple type
				//custom type  has a child element called complex type
				//custom type has a child that is another element in the taxonomy..

				//case 1: Test to see if the node has a child element called simple type
				XmlNode simpleChildNode = node.SelectSingleNode( SIMPLE_TYPE, theManager );
				if ( simpleChildNode != null )
				{
					return new XBRLSimpleType();
				}

				XmlNode complexChildNode = node.SelectSingleNode( COMPLEX_TYPE, theManager );
				if ( complexChildNode != null )
				{
					return new XBRLComplexType();
				}

				XmlNode namedAttribute = node.SelectSingleNode( "@type", theManager );

				if ( namedAttribute != null )
				{
					string val = namedAttribute.Value;

					//check if name is base xbrl type....
					XBRLSimpleType simpType = XBRLCustomTypeCreator.CreateSimpleType( val );
					if ( simpType != null )
						return simpType;

					//need to strip out the prefix...
					if ( val.IndexOf( ':' ) >= 0 )
					{
						string[] tmp = val.Split( ':' );
						if ( tmp.Length > 1 )
						{
							val = tmp[1];
						}
					}
					// it is a custom type.. need to find if it is a simple type or complex type
					simpleChildNode = GetSimpleTypeNodeFromTaxonomyByName( val );
					if ( simpleChildNode != null )
					{
						return new XBRLSimpleType();
					}
					complexChildNode = GetComplexTypeNodeFromTaxonomyByName( val );
					if ( complexChildNode != null )
					{
						return new XBRLComplexType();
					}
				}
			}

			return new XBRLUndefinedType();
		}

		#endregion

		#region Bind Elements
		internal void BindElements( BindElementDelegate BinderDelegate, out int numErrors )
		{
			numErrors = 0;
			IDictionaryEnumerator enumer = allElements.GetEnumerator();

			// go through each element 
			while ( enumer.MoveNext() )
			{
				Element e = enumer.Value as Element;
				if ( !BinderDelegate( e ) )
				{
					++numErrors;
					++numElementErrors;
				}
				
			}
		}
		
		public void BindPresentationCalculationElements( bool isPresentation, out int numErrors )
		{
			numErrors = 0;
			Hashtable locsByHRef = BuildPresentatonCalculationByHRef( isPresentation );
			BindPresentatonCalculationElements( locsByHRef );
		}

		private Hashtable BuildPresentatonCalculationByHRef( bool isPresentation )
		{
			Hashtable ret = new Hashtable();

			Hashtable source = this.presentationInfo;
			if ( !isPresentation )
			{
				source = this.calculationInfo;
			}

			if ( source == null ) return ret;

			foreach ( PresentationLink pl in source.Values )
			{
				if (roleTypes != null)
				{
					RoleType rt;
					if (roleTypes.TryGetValue(pl.Title, out rt))
					{

						if (!string.IsNullOrEmpty(rt.Definition))
						{
							pl.Title = rt.Definition;
						}
					}
				}


				pl.BuildLocatorHashbyHref( ret );
				pl.OnParseComplete(); //clears up unused information.

			}

			return ret;

		}

		private void BindPresentatonCalculationElements( Hashtable locatorsByHref )
		{
			IDictionaryEnumerator enumer = allElements.GetEnumerator();

			// go through each element 
			while ( enumer.MoveNext() )
			{
				Element e = enumer.Value as Element;
				ArrayList locators = locatorsByHref[e.Id] as ArrayList;
				if ( locators == null )
				{
					continue;
					//Common.WriteInfo( "XBRLParser.Warning.NoLocatorForElement", errorList, e.ToString() );
				}
				else
				{
					foreach ( PresentationLocator pl in locators )
					{
						pl.MyElement = e;
					}

				}
			}


		}


		private void PopulateHrefHash( Hashtable newlabelTable )
		{
			/* BUG 3558 - The labelTable has label as the key, which may or may not match the e.Id.  
			 * In the gg-2006-12-31.xsd taxonomy some labels have a label that matches the e.Id  but
			 * others don't, so loop through every labelLocator in the labelTable and check each
			 * HRef to see if that locator should be used.  If the HRefs match then add the labelData for 
			 * each language that needs to be added. 
			 * 
			 * The labelDatas hash is built in LinkBase.LoadLocator().  The hash key cannot be changed
			 * for LabelLink without breaking PresentationLink and ReferenceLink, which also inherit
			 * LinkBase.  Ultimately it would be better to have the correct hash key instead of looping
			 * through every labelLocator in labelTable. */

			//build a hash with href as the key so we can use elementId to look up the labels
			foreach ( LabelLocator currentLL in newlabelTable.Values )
			{
				if ( currentLL == null || currentLL.HRef == null )
					continue;

				if ( labelHrefHash.ContainsKey( currentLL.HRef ) )
				{
					//combine this labelLocator with the existing ones
					LabelLocator existingLL = labelHrefHash[currentLL.HRef] as LabelLocator;
					numWarnings += existingLL.AddLabels( currentLL, errorList );



				}
				else
				{
					//this is the first labelLocator for this href
					labelHrefHash[currentLL.HRef] = currentLL;
				}

			}
		}

		internal bool BindLabelToElement( Element e )
		{
			/* BUG 3558 - The labelTable has label as the key, which may or may not match the e.Id.  
			 * In the gg-2006-12-31.xsd taxonomy some labels have a label that matches the e.Id  but
			 * others don't, so use the labelHrefHash and get the labels for each HRef. */

			if ( labelHrefHash.ContainsKey( e.Id ) )
			{
				e.LabelInfo = labelHrefHash[e.Id] as LabelLocator;
			}

			if ( e.LabelInfo == null )
			{
				//Common.WriteInfo( "XBRLParser.Warning.NoLabelForElement", errorList, e.Id );
				return true;
			}

			return true;
		}

		private bool BindReferenceToElement( Element e )
		{
			ReferenceLocator rl = null;

			if ( referenceTable != null )
			{

				rl = referenceTable[e.Id] as ReferenceLocator;

				if ( rl == null )
				{

					//Common.WriteInfo( "XBRLParser.Warning.NoReferenceForElement", errorList, e.Id );
					return true;
				}

				e.ReferenceInfo = rl;
			}
			return true;
		}


		#endregion

		#region Import other Taxonomies
		/// <summary>
		/// Loads and parses taxonomies on which this <see cref="Taxonomy"/> is dependent.
		/// </summary>
		/// <param name="numErrors">An output parameter containing the number of errors
		/// encountered in the load and parse process.</param>
		public void LoadImports( out int numErrors )
		{
			numErrors = 0;

			if ( dependantTaxonomies.Count == 0 )
			{
				numErrors += LoadDependantTaxonomies(this.schemaPath);
			}

			infos.Clear();

			// put ourselves first (0th index)
			this.infos.Add( new TaxonomyItem( GetTargetNamespace(), schemaFile, GetNSPrefix(), IsAucentExtension, false ) );

			if (dependantTaxonomyFilenames.Count > 0 && IsAucentExtension)
			{
				TaxonomyImportedByAucentExtension = dependantTaxonomyFilenames[0] as string;
			}

			ParseDependentTaxonomies( new ArrayList(dependantTaxonomies),
				ref numErrors );
		}

		private void ParseDependentTaxonomies( ArrayList taxonomies, ref int numErrors )
		{
			for ( int i = 0; i < taxonomies.Count; ++i )
			{
				Taxonomy t = (Taxonomy)taxonomies[i];
				

				// load up the elements associated with this schema
				int errors = 0;
				t.ParseInternal( out errors );
				if (t.isCopiedForMerging && t.infos.Count > 0)
				{
					this.infos.AddRange(t.infos);
				}
				else
				{
					this.infos.Add(new TaxonomyItem(t.GetTargetNamespace(), t.schemaFile, t.GetNSPrefix(), t.IsAucentExtension, t.DefinesCustomTypes));
				}
				numErrors += errors;

				numCalcErrors += t.NumCalcErrors;
				numLabelErrors += t.NumLabelErrors;
				numPresErrors += t.NumPresErrors;
				numRefErrors += t.NumReferenceErrors;
				numDefErrors += t.NumDefErrors;

				// get the errors out of the taxonomy and add them to the parent
				if (errors > 0 || (t.errorList != null &&  t.errorList.Count > 0 ) )
				{
					errorList.AddRange( t.errorList );
				}
			}
		}

		



		private int MergeElements( Taxonomy childTaxonomy , Dictionary<string, int> taxItemIdDt )
		{
			//sometimes the same element might have more than one key...
			//as the id  does not match prefix_name 
			//id is plural and name is singular in the example below...
			//<element name="OtherReceivablesBank" id="krfr-pte_OtherReceivablesBanks" type="xbrli:monetaryItemType" substitutionGroup="xbrli:item" nillable="true" xbrli:balance="debit" xbrli:periodType="instant" /> 

			Dictionary<string, Element> idsChecked = new Dictionary<string, Element>();

			foreach( DictionaryEntry de in childTaxonomy.allElements)
			{

				if (!this.allElements.ContainsKey(de.Key))
				{

					Element ele = de.Value as Element;

					if (idsChecked.ContainsKey(ele.Id))
					{
						Element otherEle;
						if (idsChecked.TryGetValue(ele.Id, out otherEle))
						{
							ele.TaxonomyInfoId = otherEle.TaxonomyInfoId;
						}
					}
					else
					{

						ele.TaxonomyInfoId = taxItemIdDt[childTaxonomy.infos[ele.TaxonomyInfoId].WebLocation.ToLower()];
						idsChecked[ele.Id] = ele;
					}

					this.allElements[de.Key] = ele;


				}

			}

			return 0;

			
		}


	
		private int MergeExtendedDataTypes( Hashtable parentTable, Hashtable childTable )
		{
			int errors = 0;
			foreach ( string key in childTable.Keys )
			{
				parentTable[key] = childTable[key] as string;
			}

			return errors;
		}

		private int MergeEnumerations( Hashtable parentTable, Hashtable childTable, string childSchema, bool writeErrors )
		{
			int errors = 0;
			IDictionaryEnumerator enumer = childTable.GetEnumerator();
			while ( enumer.MoveNext() )
			{
				if ( parentTable.ContainsKey( enumer.Key ) )
				{
					//TODO:  downgrade this to a warning?
					if ( writeErrors )
					{
						Common.WriteError( "XBRLParser.Warning.ElementAlreadyExists", errorList, enumer.Key.ToString(), childSchema, this.schemaFile );
						++errors;
					}
					continue;
				}

				parentTable[enumer.Key] = enumer.Value;
			}

			return errors;
		}


		/// <summary>
		/// Returns a list of all the file names and locations of the dependent taxonomies 
		/// that this taxonomy imports.
		/// </summary>
		/// <param name="numErrors"></param>
		/// <returns></returns>
		public ArrayList FindAllImports( out int numErrors )
		{
			ArrayList allImports = new ArrayList();
			numErrors = 0;

			if ( dependantTaxonomies.Count == 0 )
			{
				numErrors += LoadDependantTaxonomies(this.schemaPath);
			}

			// put ourselves first (0th index)
			TaxonomyItem tItem = new TaxonomyItem( GetTargetNamespace(),
				schemaFile, GetNSPrefix(), this.IsAucentExtension, false );
			allImports.Add( tItem.Location );
			

			for ( int i = 0; i < dependantTaxonomies.Count; ++i )
			{
				Taxonomy t = (Taxonomy)dependantTaxonomies[i];

				#pragma warning disable 0219
				TaxonomyItem depTItem = new TaxonomyItem( t.GetTargetNamespace(),
					t.schemaFile, t.GetNSPrefix(), t.IsAucentExtension, t.DefinesCustomTypes);
				#pragma warning restore 0219

				allImports.Add(t.schemaFile);
			}

			return allImports;
		}
		#endregion

		#region Load Calculation

		private void LoadCalculation( out int numErrors )
		{
			numErrors = 0;

            Presentation p = LoadCalculationSchema(out numErrors);
			if ( p == null && innerTaxonomy )
				return;

			if ( p == null )
			{
				p = new Presentation();
				p.CalculationLinks = new Hashtable();
			}

			calculationInfo = p.CalculationLinks;

			if ( !innerTaxonomy )
			{
				if ( dependantTaxonomies.Count == 1 )
				{
					// it's just a simple merge
					calculationInfo = MergePresentations( ((Taxonomy)dependantTaxonomies[0]).calculationInfo, calculationInfo, out errorList );
				}
				else if ( dependantTaxonomies.Count > 1 )
				{
					for ( int i = dependantTaxonomies.Count - 1; i != 0; --i )
					{
						((Taxonomy)dependantTaxonomies[i - 1]).calculationInfo = MergePresentations( ((Taxonomy)dependantTaxonomies[i]).calculationInfo,
							((Taxonomy)dependantTaxonomies[i - 1]).calculationInfo, out errorList );
						numErrors += errorList.Count;
						numCalcErrors += errorList.Count;
					}

					// now merge against the current presentation
					calculationInfo = MergePresentations( calculationInfo,
						((Taxonomy)dependantTaxonomies[0]).calculationInfo, out errorList );
					numErrors += errorList.Count;
					numCalcErrors += errorList.Count;
				}

				// nothing to do if there are no inner taxonomies
			}
			//if ( calculationInfo != null )
			//{
			//    foreach ( PresentationLink pl in calculationInfo.Values )
			//    {
			//        int err = 0;
			//        pl.RemoveRecursiveLocator( ref err );
			//        numErrors += err;
			//        numCalcErrors += err;

			//        if ( pl.ErrorList != null )
			//        {
			//            errorList.AddRange( pl.ErrorList );
			//        }
			//    }
			//}

			if ( numErrors > 0 )
			{
				errorList.AddRange( p.ErrorList );
			}



		}

		internal Presentation LoadCalculationSchema( out int numErrors )
		{
			numErrors = 0;
			calculationFile = GetLinkbaseReference( TARGET_LINKBASE_URI + CALCULATION_ROLE );

			if ( calculationFile == null )
				return null;

			Hashtable tempCalcHash = new Hashtable();
			Presentation tempCalc = new Presentation();
            tempCalc.loadingTaxonomy = this.GetLoadingTaxonomy();
            tempCalc.PromptUser = this.PromptUser;

			tempCalc.ProcessingPresenationType = Presentation.PreseantationTypeCode.Calculation;

			foreach ( string calcFile in calculationFile )
			{
				LoadPresentationLinkbase( tempCalc, calcFile, out numErrors );

				//build the calculation links
				if ( tempCalc.CalculationLinks != null )
				{
					IDictionaryEnumerator calcLinkEnumer = tempCalc.CalculationLinks.GetEnumerator();
					while ( calcLinkEnumer.MoveNext() )
					{
						if ( !tempCalcHash.ContainsKey( calcLinkEnumer.Key ) )
						{
							tempCalcHash[calcLinkEnumer.Key] = calcLinkEnumer.Value;
						}
                        else
                        {
                            //need to merge the info..
                            ArrayList tmp = new ArrayList();
                            PresentationLink orig = tempCalcHash[calcLinkEnumer.Key] as PresentationLink;
                            orig.Append(calcLinkEnumer.Value as PresentationLink, tmp);
                        }

					}
				}

				//build the roleRefs
                this.MergeRoleRefsFromLinkbase(tempCalc.roleRefs, tempCalc.BaseSchema,
                    tempCalc.SchemaFile,  LinkbaseFileInfo.LinkbaseType.Calculation);
				tempCalc.roleRefs = null;
			}

			Presentation p = new Presentation();
			p.ProcessingPresenationType = Presentation.PreseantationTypeCode.Calculation;

			p.CalculationLinks = tempCalcHash;

			return p;
		}


		#endregion

		#region Load Presentation
		public void LoadPresentation( out int numErrors )
		{
			numErrors = 0;

			Presentation p = LoadPresentationSchema( out numErrors );

			if ( p == null && innerTaxonomy )
				return;	// nothing to do

			if ( p == null )
			{
				p = new Presentation();
				p.PresentationLinks = new Hashtable();
			}


			p.OwnerHandle = OwnerHandle;
			p.ProcessingPresenationType = Presentation.PreseantationTypeCode.Presentation;

			presentationInfo = p.PresentationLinks;


#if OBSOLETE
			if ( roleTypes != null )
			{
				int errors = 0;
				if ( !VerifyPresentationTypes( p, out errors ) )
				{
					numErrors += errors;
				}
			}
#endif
			// as presentations get merged, null the child presentations out so they don't get counted twice
			//			if ( !innerTaxonomy )
			//			{
			if ( dependantTaxonomies.Count == 1 )
			{
				// it's just a simple merge
				presentationInfo = MergePresentations(presentationInfo, ((Taxonomy)dependantTaxonomies[0]).presentationInfo, out errorList);
				((Taxonomy)dependantTaxonomies[0]).presentationInfo = null;
			}
			else if ( dependantTaxonomies.Count > 1 )
			{
				for ( int i = dependantTaxonomies.Count - 1; i != 0; --i )
				{
					((Taxonomy)dependantTaxonomies[i - 1]).presentationInfo = MergePresentations( ((Taxonomy)dependantTaxonomies[i - 1]).presentationInfo,
						((Taxonomy)dependantTaxonomies[i]).presentationInfo, out errorList );
					((Taxonomy)dependantTaxonomies[i]).presentationInfo = null;
				}

				// now merge against the current presentation
				presentationInfo = MergePresentations( presentationInfo,
					((Taxonomy)dependantTaxonomies[0]).presentationInfo, out errorList );
				((Taxonomy)dependantTaxonomies[0]).presentationInfo = null;
			}

			// nothing to do if there are no inner taxonomies
			//			}
			//if ( presentationInfo != null )
			//{
			//    foreach ( PresentationLink pl in presentationInfo.Values )
			//    {
			//        pl.RemoveRecursiveLocator( ref numErrors );
			//    }
			//}

			if ( numErrors > 0 )
			{
				numPresErrors += numErrors;
				errorList.AddRange( p.ErrorList );
			}
		}

		private Hashtable MergePresentations( Hashtable parentPresentation, Hashtable childPresentation,
			out ArrayList errors )
		{
			errors = new ArrayList();

			if ( parentPresentation == null )
			{
				return childPresentation;
			}
			else if ( childPresentation == null )
			{
				return parentPresentation;
			}

			IDictionaryEnumerator enumer = childPresentation.GetEnumerator();
			while ( enumer.MoveNext() )
			{
				if ( parentPresentation.ContainsKey( enumer.Key ) )
				{
					((PresentationLink)parentPresentation[enumer.Key]).Append( enumer.Value as PresentationLink, errors );
				}
				else
				{
					// otherwise, add it
					parentPresentation[enumer.Key] = enumer.Value;
				}
			}

			return parentPresentation;
		}

		public Presentation LoadPresentationSchema( out int numErrors )
		{
			numErrors = 0;
			presentationFile = GetLinkbaseReference( TARGET_LINKBASE_URI + PRESENTATION_ROLE );

			if ( presentationFile == null )
				return null;

			Hashtable tempPresHash = new Hashtable();
			Presentation tempPres = new Presentation();
            tempPres.loadingTaxonomy = this.GetLoadingTaxonomy();
            tempPres.PromptUser = this.PromptUser;

			tempPres.ProcessingPresenationType = Presentation.PreseantationTypeCode.Presentation;

			foreach ( string presFile in presentationFile )
			{
				LoadPresentationLinkbase( tempPres, presFile, out numErrors );

				//build the presentation links
				if ( tempPres.PresentationLinks != null )
				{
					IDictionaryEnumerator presLinkEnumer = tempPres.PresentationLinks.GetEnumerator();
					while ( presLinkEnumer.MoveNext() )
					{
                        if (!tempPresHash.ContainsKey(presLinkEnumer.Key))
                        {
                            tempPresHash[presLinkEnumer.Key] = presLinkEnumer.Value;
                        }
                        else
                        {
                            //need to merge the info..
                            ArrayList tmp = new ArrayList();
                            PresentationLink orig = tempPresHash[presLinkEnumer.Key] as PresentationLink;
                            orig.Append(presLinkEnumer.Value as PresentationLink, tmp);
                        }
					}
				}

				//build the roleRefs
				this.MergeRoleRefsFromLinkbase( tempPres.roleRefs, tempPres.BaseSchema,
                    tempPres.SchemaFile,  LinkbaseFileInfo.LinkbaseType.Presentation);
				tempPres.roleRefs = null;
			}


			Presentation p = new Presentation();
			p.ProcessingPresenationType = Presentation.PreseantationTypeCode.Presentation;
			p.PresentationLinks = tempPresHash;

			return p;

		}

		private string UpdateRelativeHref( string origHref, string id, Dictionary<string, string> tmpCache )
		{
            if (!origHref.StartsWith("http"))
            {
                string filename = Path.GetFileName(origHref);
                int pound = filename.IndexOf("#");
                string element = string.Empty;
                if (pound >= 0)
                {
                    element = filename.Substring(pound, filename.Length - pound);

                    filename = filename.Substring(0, pound);
                }

                string val;
                if (!tmpCache.TryGetValue(filename, out val))
                {

                    for (int i = 0; i < this.TaxonomyItems.Length; i++)
                    {
						if (TaxonomyItems[i].Filename.Contains(filename))
						{
							val = AdjustURLFileNameInExtensions(TaxonomyItems[i].Location);
							tmpCache[filename] = val;

						}
                    }

                }

                if (!string.IsNullOrEmpty(element))
                {
                    return val + element;
                }
                else
                {
                    return val + "#" + id;
                }

            }
            else
            {
				if (id != null)
				{
					return origHref + "#" + id;
				}
				else
				{
					return origHref;
				}
            }

		}

		private void UpdateAllRelativeRoleRefs()
		{
			//since the rolerefs are added as and when they appear,
			//most fo them might not have the correct URL set in the href parameter...
			//we need the correct href so that we can create extensions properly.
			Dictionary<string, string> tmpCache = new Dictionary<string, string>();
			foreach ( RoleRef rr in this.roleRefs.Values )
			{
				string origHref = rr.GetHref();
				string updatedHref = UpdateRelativeHref( origHref, null, tmpCache );

				rr.SetHref( updatedHref );

			}

			foreach ( RoleType rt in this.roleTypes.Values )
			{
				string updatedHref = UpdateRelativeHref( rt.SchemaFullFileName, rt.id, tmpCache );

				rt.SetHref( updatedHref );
			}

			if (presentationInfo != null)
			{
				foreach (PresentationLink pl in this.presentationInfo.Values)
				{
					string href = GetHrefFromTaxonomyRoleRefs(pl.Role);
					if (href != null)
					{
						pl.MyHref = href;
					}
				}
			}

			if (calculationInfo != null)
			{
				foreach (PresentationLink pl in this.calculationInfo.Values)
				{
					string href = GetHrefFromTaxonomyRoleRefs(pl.Role);
					if (href != null)
					{
						pl.MyHref = href;
					}
				}
			}

			if (this.netDefinisionInfo != null && this.netDefinisionInfo.DefinitionLinks != null)
			{
				foreach (DefinitionLink dl in this.netDefinisionInfo.DefinitionLinks.Values)
				{
					string href = GetHrefFromTaxonomyRoleRefs(dl.Role);
					if (href != null)
					{
						dl.MyHref = href;
					}
				}
			}


		}

		/// <summary>
		/// Looks up the RoleRef by URI (the key to the RoleRefs dictionary)
		/// and returns the Href string.
		/// </summary>
		/// <param name="roleRefUri"></param>
		/// <returns></returns>
		public string GetHrefFromTaxonomyRoleRefs( string roleRefUri )
		{
			RoleRef rr ;
			if (this.roleRefs.TryGetValue(roleRefUri, out rr))
			{
				return rr.GetHref();

			}
			

			return null;
		}

		/// <summary>
		/// Looks up the RoleType by URI (the key to the RoleTypes dictionary)
		/// and returns the Href string.
		/// </summary>
		/// <param name="roleTypeUri"></param>
		/// <returns></returns>
		public string GetHrefFromTaxonomyRoleTypes( string roleTypeUri )
		{
			RoleType rt;
			if ( this.roleTypes.TryGetValue( roleTypeUri, out rt ) )
			{
				return rt.GetHref();

			}

			return null;
		}

		private void LoadPresentationLinkbase( Presentation p, string linkbase, out int numErrors )
		{
			numErrors = 0;

			string baseUri = GetTargetNamespace();


			if ( baseUri[baseUri.Length - 1] != '/' && baseUri[baseUri.Length - 1] != '\\' )
			{
				baseUri += "/";
			}
			string bs = baseUri + schemaFilename;


			Dictionary<string, string> discoveredSchemas = new Dictionary<string, string>();

			if ( linkbase.StartsWith( "http" ) )
			{
				baseUri = linkbase.Replace( Path.GetFileName( linkbase ), string.Empty );
			}


			p.BaseSchema = bs;
			p.OwnerHandle = OwnerHandle;
			p.Load( linkbase );
			if (IsAucentExtension && skipAucentExtensions && !this.innerTaxonomy &&
				p.IsDocumentCreatedByRivet())
			{
				//if this is a top level aucent extension taxonomy and ...
				//if the skipAucentExtensions is set to true ... we want to load just the no rivet created files...
				//as we will merge the rivet created files later....
				return;
			}
			if ( p.ErrorList.Count > 0 )
			{
				if ( p.ProcessingPresenationType == Presentation.PreseantationTypeCode.Presentation )
				{
					for ( int i = 0; i < p.ErrorList.Count; i++ )
						Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
							this.SchemaFile + ": PRESENTATION_ROLE Error: " + p.ErrorList[i] );
				}
				else
				{

					for ( int i = 0; i < p.ErrorList.Count; i++ )
						Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
							this.SchemaFile + ": CALCULATION_ROLE Error: " + p.ErrorList[i] );

				}
			}


			p.Parse(  discoveredSchemas,
				out numErrors );


			foreach ( string val in discoveredSchemas.Values )
			{
				string filename = Path.GetFileName( val );
				string hintPath = val.Replace( filename, string.Empty );

				LoadAdditionalDependantTaxonomy(filename, hintPath, ref  numErrors);
			}
		}

		#endregion

		#region Role Refs

		private void MergeRoleRefsFromLinkbase( Hashtable linkbaseRoleRefs, 
			string baseXsdFullFileName,
            string linkbaseFileFullName,
            LinkbaseFileInfo.LinkbaseType linkbaseType )
		{
			if ( linkbaseRoleRefs != null )
			{
                LinkbaseFileInfo fileInfo = new LinkbaseFileInfo();
                fileInfo.Filename = linkbaseFileFullName;
				fileInfo.XSDFileName = baseXsdFullFileName;
                fileInfo.LinkType = linkbaseType;
                this.linkbaseFileInfos.Add(fileInfo);
				foreach ( DictionaryEntry de in linkbaseRoleRefs )
				{
					string key = de.Key as string;
                    fileInfo.RoleRefURIs.Add(key);

					RoleRef linkRR = de.Value as RoleRef;

					// add the file name reference
					linkRR.AddFileReference(Path.GetFileName(linkbaseFileFullName));

					RoleRef rr;
					if ( this.roleRefs.TryGetValue( key, out rr ) )
					{
						rr.MergeFileReferences( linkRR.GetFileReferences() );
					}
					else
					{
						//update the href to be a full path (hash roleRef only has schema filename)
						//string newHref = GetHrefForRoleRef( linkRR, linkbaseSchema );
						//linkRR.SetHref( newHref );

						this.roleRefs.Add( key, linkRR );
					}

				}

                

			}

		}

		private void MergeRoleRefsFromTaxonomy( Taxonomy depTax )
		{
			foreach ( KeyValuePair<string, RoleRef> kvp in depTax.roleRefs )
			{
				RoleRef rr;
				if ( this.roleRefs.TryGetValue( kvp.Key, out rr ) )
				{
					rr.MergeFileReferences( kvp.Value.GetFileReferences() );
				}
				else
				{
					roleRefs[kvp.Key] = kvp.Value;
				}

			}
		}

		/// <summary>
		/// Looks up and returns a list of RoleRef objects given the list of RoleType objects to be searched for.
		/// </summary>
		/// <param name="searchRoleTypes">The list of RoleTypes to look for.</param>
		/// <param name="foundRoleRefs">The list of RoleRefs found. Will be null if method fails.</param>
		/// <returns>True of if RoleRefs are found.</returns>
		public bool TryGetRoleRefsFromRoleTypes( List<RoleType> searchRoleTypes, out List<RoleRef> foundRoleRefs )
		{
			foundRoleRefs = null;
			if ( searchRoleTypes != null && searchRoleTypes.Count > 0 
				&& this.roleRefs != null && this.roleRefs.Count > 0 )
			{
				foundRoleRefs = new List<RoleRef>();
				foreach ( RoleType rt in searchRoleTypes )
				{
					RoleRef rr;
					if ( this.roleRefs.TryGetValue( rt.Uri, out rr ) )
					{
						foundRoleRefs.Add( rr );
					}
				}
			}

			return (foundRoleRefs != null);
		}

		#endregion

        #region LinkbaseFileInfos

        private void MergeLinkbaseFileInfos(Taxonomy tax)
        {
            this.linkbaseFileInfos.AddRange(tax.linkbaseFileInfos);
        }
        #endregion

        #region Role Types
        private void LoadRoleTypes( out int numErrors )
		{
			numErrors = 0;

			XmlNodeList pTypesList = theDocument.SelectNodes(RTYPE_KEY, theManager);

			if (pTypesList == null || pTypesList.Count == 0)
			{
				return; // nothing to do
			}
			else if (roleTypes == null)
			{
				roleTypes = new Dictionary<string,RoleType>(pTypesList.Count);
			}

			ArrayList labelRoleTypes = new ArrayList();

			foreach (XmlNode node in pTypesList)
			{
				if (node.ChildNodes.Count == 0)
				{
					++numWarnings;

					Common.WriteWarning("XBRLParser.Warning.NoChildrenForNode", errorList, node.OuterXml);
					continue;
				}

				string id = null;
				string uri = null;

				if (!Common.GetAttribute(node, ID_TAG, ref id, errorList) ||
					!Common.GetAttribute(node, RURI_TAG, ref uri, errorList))
				{
					++numErrors;
					continue;	// Couldn't find an attribute, just ignore it
				}

				RoleType rt = new RoleType( uri, id, this.schemaFile );



				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.NodeType == XmlNodeType.Comment) continue;

					if (child.LocalName == DEF_TAG)
					{
						rt.SetDefinition(child.InnerText);
					}
					else
					{
						try
						{
							rt.AddLink(child.InnerText);
						}
						catch (ArgumentNullException)
						{
							Common.WriteWarning("XBRLParser.Warning.EmptyNode", errorList, child.OuterXml);
							++numWarnings;
						}
					}
				}

				roleTypes[uri] = rt;

				// if it's a label role type, then we want to union it
				// with the other label roles that were set when we loaded
				// the taxonomy label file. 
				if ( RoleTypeDefinesLabelUsage( rt ) )
				{
					labelRoleTypes.Add( rt.id );
				}

			}

			MergeLanguagesAndLabelRoles( null, labelRoleTypes );
		}

		private void MergeRoleTypes( Taxonomy tax )
		{
			if (tax.roleTypes != null)
			{
				if (this.roleTypes == null)
				{
					roleTypes = tax.roleTypes;
				}
				else
				{
					foreach (KeyValuePair<string, RoleType> kvp in tax.roleTypes)
					{

						if (!this.roleTypes.ContainsKey(kvp.Key))
						{
							roleTypes[kvp.Key] = kvp.Value;
						}
					}
				}

			}
		}

		/// <summary>
		/// Goes through all the role types and finds the ones that have been defined as Label role types.
		/// Those role types that are defined as used on "link:label" are returned.
		/// </summary>
		/// <param name="labelRoleTypesById">Key: Role Type Id; Value: The Role Type</param>
		/// <returns></returns>
		public bool TryGetRoleTypesForLabelDefinitions( out Dictionary<string, RoleType> labelRoleTypesById )
		{
			labelRoleTypesById = new Dictionary<string, RoleType>();

			foreach ( RoleType rt in roleTypes.Values )
			{
				if ( RoleTypeDefinesLabelUsage( rt ) )
				{
					labelRoleTypesById[rt.id] = rt;
				}
			}

			return (labelRoleTypesById != null && labelRoleTypesById.Count > 0);

		}

		/// <summary>
		/// Looking for those role types that are defined as used on "link:label".
		/// </summary>
		/// <param name="rt"></param>
		/// <returns></returns>
		private bool RoleTypeDefinesLabelUsage( RoleType rt )
		{
			if ( rt != null )
			{
				if ( rt.whereUsed != null && rt.whereUsed.Count > 0 )
				{
					foreach ( string where in rt.whereUsed )
					{
						// if it's a label role type, then we want to union it
						// with the other label roles that were set when we loaded
						// the taxonomy label file. 
						if ( where == LABEL_USED_ON )
						{
							return true;
						}
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Goes through the role refs and finds the corresponding
		/// role type for each one and returns a list of the used role types.
		/// </summary>
		/// <param name="usedRoleTypes"></param>
		/// <returns></returns>
		public bool TryGetUsedRoleTypes( out List<RoleType> usedRoleTypes )
		{
			usedRoleTypes = null;

			if ( this.roleRefs != null && this.roleRefs.Count > 0 )
			{
				if ( this.roleTypes != null && this.roleTypes.Count > 0 )
				{
					usedRoleTypes = new List<RoleType>();
					foreach ( string roleRefUri in this.roleRefs.Keys )
					{
						RoleType rt;
						if ( this.roleTypes.TryGetValue( roleRefUri, out rt ) )
						{
							usedRoleTypes.Add( rt );
						}
					}
				}
			}

			return (usedRoleTypes != null && usedRoleTypes.Count > 0);
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public List<RoleType>  GetListOfExtendedRoles()
		{
			List<RoleType> ret = new List<RoleType>();
			if (this.IsAucentExtension)
			{
				List<RoleType> rts;
				if (this.TryGetUsedRoleTypes(out rts))
				{
					string extFileName = Path.GetFileName(this.infos[0].Location);

					foreach (RoleType rt in rts)
					{
						if (string.Compare(extFileName,Path.GetFileName( rt.SchemaFullFileName), true) == 0)
						{
							ret.Add(rt);
						}
					}

				}
				

			}


			return ret;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="roleURI"></param>
		/// <returns></returns>
		public bool IsExtendedRole(string roleURI)
		{

			RoleType rt;

			if (this.roleTypes.TryGetValue(roleURI, out rt))
			{
				if (string.Compare(Path.GetFileName(this.infos[0].Location), Path.GetFileName(rt.SchemaFullFileName), true) == 0)
				{
					return true;
				}

			}


			return false;
		}
		/// <summary>
		/// This method is a hard-coded helper representing the Regular Expressions that are
		/// found in us-types-2008-03-31.xsd.  At this time there are only 3...
		/// </summary>
		/// <param name="elementID"></param>
		/// <param name="regex"></param>
		/// <returns></returns>
		public bool TryGetElementRegex( string elementID, out string regex )
		{
			regex = string.Empty;

			if( string.IsNullOrEmpty( elementID ) )
				return false;

			Element el = this.AllElements[ elementID ] as Element;
			if( el == null )
				return false;

			string type = el.OrigElementType;
			if( string.IsNullOrEmpty( type ) )
				return false;

			//This should save some character matching
            type = type.Replace("us-types:", string.Empty);
            type = type.Replace("nonnum:", string.Empty);
            type = type.Replace("num:", string.Empty);

			switch( type ){
				case "centralIndexKeyItemType":
					regex = @"[A-Z0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]";
					return true;
				case "countryItemType":
					regex = @"[A-Z][A-Z0-9]";
					return true;
				case "currencyItemType":
					regex = @"[A-Z][A-Z][A-Z]";
					return true;
				case "nineDigitItemType":
					regex = @"[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]";
					return true;
			}

			return false;
		}

        /// <summary>
        /// determines if a specific role is used in the given taxonomy.... 
        /// </summary>
        /// <param name="roleURI"></param>
        /// <returns></returns>
        public bool IsRoleUsed(string roleURI)
        {
            return this.roleRefs != null ? roleRefs.ContainsKey(roleURI) : false;

        }
		#endregion

		#region Selectable labels
        /// <summary>
        /// make sure all the values are lower case....
        /// </summary>
		private static void BuildSkipList()
		{
			//BUG 961 - show Documentation, to allow for multiple languages
			//skipLabelList.Add( DOCUMENTATION.ToLower() );
			skipLabelList.Add( GUIDANCE.ToLower() );
			skipLabelList.Add( PREFERRED_LABEL.ToLower() );
			skipLabelList.Add("axisdefault");
			skipLabelList.Add("deprecateddate");
            skipLabelList.Add(LabelDefinition.DEPRECATED_LABEL_ROLE.ToLower());
            skipLabelList.Add(LabelDefinition.DEPRECATEDDATE_LABEL_ROLE.ToLower());
            skipLabelList.Add("futuredeprecate");
            skipLabelList.Add("releasenotes2009");
            skipLabelList.Add("newconcept");

			skipLabelList.Sort();
		}

        public static bool SkipLabel(string label)
		{
            if (!string.IsNullOrEmpty(label))
            {
                return skipLabelList.BinarySearch(label.ToLower()) > -1;
            }

            return false;
		}

         

		/// <summary>
		/// Gets a list of label roles that can be selected for this taxonomy display.  
		/// The method removes some labels that should be skipped and returns the supported label roles.
		/// </summary>
		/// <returns>List of strings representing the selectable label roles.</returns>
		public ArrayList GetSelectableLabelRoles()
		{
			int errors = 0;
			ArrayList allLabels = GetLabelRoles( false, out errors );

			for ( int i = 0; i < allLabels.Count; ++i )
			{
				if ( SkipLabel( ((string)allLabels[i]) ) )
				{
					allLabels.RemoveAt( i );
					--i;
				}
			}

			int index = allLabels.BinarySearch("terseLabel");
			if (index < 0)
			{
				allLabels.Insert(~index, "terseLabel");
			}

			index = allLabels.BinarySearch("verboseLabel");
			if (index < 0)
			{
				allLabels.Insert(~index, "verboseLabel");
			}

			return allLabels;
		}

		#endregion

		#region Gets

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public TaxonomyItem GetTopLevelTaxonomyInfo()
		{
			return (TaxonomyItem)this.infos[0];
		}

		/// <summary>
		/// Uses the taxonomy info id in the Node parameter and returns the
		/// corresponding TaxonomyItem from the internal list of infos.
		/// </summary>
		/// <param name="n">The Node with the taxonomy info id for lookup in the internal list.</param>
		/// <returns></returns>
		public TaxonomyItem GetTaxonomyInfo( Node n )
		{
			return infos[n.TaxonomyInfoId];
		}

        /// <summary>
        /// Uses the taxonomy info id in the Node parameter and returns the
        /// corresponding TaxonomyItem from the internal list of infos.
        /// </summary>
        /// <param name="e">The element with the taxonomy info id for lookup in the internal list.</param>
        /// <returns></returns>
        public TaxonomyItem GetTaxonomyInfo(Element e)
        {
            return infos[e.TaxonomyInfoId];
        }

		/// <summary>
		/// Gets the complete URL for the web location of the taxonomy.
		/// </summary>
		/// <returns></returns>
		public string GetCompleteWebLocation()
		{
			// make sure it's loaded up
			GetTargetNamespace();

			if ( targetNamespace[targetNamespace.Length - 1] == '\\' || targetNamespace[targetNamespace.Length - 1] == '/' )
			{
				return GetTargetNamespace() + SchemaFilename;
			}

			return GetTargetNamespace() + "/" + SchemaFilename;
		}

		/// <summary>
		/// Uses boundElements to provide presentation. boundElements hashtable provides tuple hierarchy,
		/// allElements does not.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetNodesByElement()
		{
			ArrayList sortedElements =   PreProcessGetNodesByElement();

			ArrayList nodeList = new ArrayList( sortedElements.Count );

			foreach ( Element e in sortedElements )
			{
				nodeList.Add( e.CreateNode(currentLanguage, currentLabelRole, true ) );
			}

			nodeList.Sort();

			return nodeList;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, ArrayList> GetNodesDictionaryByElement()
		{
            ArrayList sortedElements = PreProcessGetNodesByElement();

			Dictionary<string, ArrayList> alphabeticGrouping = new Dictionary<string, ArrayList>();

			for ( int i = 0; i < sortedElements.Count; i++ )
			{
				Element e = sortedElements[i] as Element;

                Node n = e.CreateNode(currentLanguage, currentLabelRole, true);
				string firstChar = n.GetLabelWithoutLeadingCharacters();
				if ( !string.IsNullOrEmpty( firstChar ) )
				{
					firstChar = firstChar.Substring( 0, 1 ).ToUpper();
				}
				else if ( firstChar == null )
				{
					firstChar = string.Empty;
				}

				ArrayList nodes;
				if ( !alphabeticGrouping.TryGetValue( firstChar, out nodes ) )
				{
					nodes = new ArrayList();
					alphabeticGrouping[firstChar] = nodes;
				}

				nodes.Add( n );
			}

			foreach ( ArrayList nodes in alphabeticGrouping.Values )
			{
				nodes.Sort();
			}

			return alphabeticGrouping;
		}

        internal ArrayList PreProcessGetNodesByElement()
        {
            errorList.Clear();

            if (currentLanguage == null || currentLabelRole == null)
            {
                throw new AucentException("XBRLError.MustSetCurrent");
            }

           

            ArrayList sortedElements = new ArrayList(allElements.Count);
            foreach (Element e in this.allElements.Values)
            {
                if (e.HasTupleParents) continue;

                sortedElements.Add(e);
            }

            sortedElements.Sort();

            return sortedElements;

        }

	
		/// <summary>
		/// See overloaded <see cref="GetNodesByPresentation(Boolean)"/>.
		/// This will retrieve nodes by presentation with the flag to include segment dimensions
		/// set to false.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetNodesByPresentation()
		{
			return GetNodesByPresentation(false, null );
		}
        /// <summary>
        /// Returns flat linked-list of nodes by presentation view.
        /// Parameter determines if we need to include the dimension hierarchy 
        /// inside the presentation hierarchy. 
        /// This is useful in dragon tag where we want to display both 
        /// elements and segments in the reporting element pane.
        /// </summary>
        /// <param name="includeSegmentDimensions"></param>
        /// <returns></returns>
        public ArrayList GetNodesByPresentation(bool includeSegmentDimensions)
        {
            return GetNodesByPresentation(includeSegmentDimensions, null);
        }

		/// <summary>
		/// Returns flat linked-list of nodes by presentation view.
		/// Parameter determines if we need to include the dimension hierarchy 
		/// inside the presentation hierarchy. 
		/// This is useful in dragon tag where we want to display both 
		/// elements and segments in the reporting element pane.
		/// </summary>
		/// <param name="includeSegmentDimensions"></param>
		/// <returns></returns>
        public ArrayList GetNodesByPresentation(bool includeSegmentDimensions,
            List<string> excludedReports )
        {
            errorList.Clear();
            if (presentationInfo == null || presentationInfo.Count == 0)
            {
                throw new AucentException("XBRLParser.Error.MustLoadAndParse");
            }

            if (currentLanguage == null)
            {

                if (this.supportedLanguages.Count > 0)
                {
                    currentLanguage = supportedLanguages[0] as string;
                }
                else
                {
                    currentLanguage = "en";
                }


            }

            if (currentLabelRole == null)
            {
                currentLabelRole = PresentationLocator.preferredLabelRole;
            }




            ArrayList nodeList = new ArrayList();

            IDictionaryEnumerator enumer = presentationInfo.GetEnumerator();

            while (enumer.MoveNext())
            {
                PresentationLink pLink = enumer.Value as PresentationLink;
                if (excludedReports != null && excludedReports.Contains(pLink.Role))
                {
                    continue;
                }


                Node n = pLink.CreateNode(currentLanguage,
                    currentLabelRole, includeSegmentDimensions == true ? this.netDefinisionInfo : null);

                if (n != null)
                {
                    nodeList.Add(n);
                }
            }

            //sort the top level (the presentationLink titles)
            nodeList.Sort();

            //sort the first level under each presentationLink (there is no order)
            foreach (Node childNode in nodeList)
            {
                if (childNode.Children != null)
                {
                    childNode.Children.Sort();
                }
            }




            return nodeList;
        }

		/// <summary>
		/// get just one top level report....saves time if we are interested in just one report.
		/// </summary>
		/// <param name="includeSegmentDimensions"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public Node GetPresentationForRole(bool includeSegmentDimensions, string roleId)
		{
			errorList.Clear();

			if (presentationInfo == null || presentationInfo.Count == 0)
			{
                return null;
			}

			if (currentLanguage == null)
			{

				if (this.supportedLanguages.Count > 0)
				{
					currentLanguage = supportedLanguages[0] as string;
				}
				else
				{
					currentLanguage = "en";
				}


			}

			if (currentLabelRole == null)
			{
				currentLabelRole = PresentationLocator.preferredLabelRole;
			}

			PresentationLink pl = presentationInfo[roleId] as PresentationLink;
			if (pl == null) return null;

			return  pl.CreateNode(currentLanguage,
				   currentLabelRole, includeSegmentDimensions == true ? this.netDefinisionInfo : null);

		}

		/// <summary>
		/// get just one top level report....saves time if we are interested in just one report.
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public Node GetCalculationForRole(string roleId)
		{
			errorList.Clear();

			if (calculationInfo == null || calculationInfo.Count == 0)
			{
                return null;
			}

			if (currentLanguage == null)
			{

				if (this.supportedLanguages.Count > 0)
				{
					currentLanguage = supportedLanguages[0] as string;
				}
				else
				{
					currentLanguage = "en";
				}


			}
			if (currentLabelRole == null)
			{
				currentLabelRole = PresentationLocator.preferredLabelRole;
			}


			PresentationLink pl = calculationInfo[roleId] as PresentationLink;
			if (pl == null) return null;

			return pl.CreateNode(currentLanguage,   currentLabelRole,null);

		}


        public DimensionNode GetDimensionForRole(string roleId)
        {

            if (this.netDefinisionInfo != null && this.netDefinisionInfo.DefinitionLinks[roleId] != null )
            {
                if (currentLanguage == null)
                {

                    if (this.supportedLanguages.Count > 0)
                    {
                        currentLanguage = supportedLanguages[0] as string;
                    }
                    else
                    {
                        currentLanguage = "en";
                    }


                }
                if (currentLabelRole == null)
                {
                    currentLabelRole = PresentationLocator.preferredLabelRole;
                }
                DimensionNode ret ;
                if (netDefinisionInfo.TryGetDimensionNodeForRole(currentLanguage, currentLabelRole, roleId, out ret))
                {

                    return ret;
                }

            }




            return null;
        }


        public DimensionNode GetDimensionMeasureForRole(string roleId)
        {

            if (this.netDefinisionInfo != null && this.netDefinisionInfo.DefinitionLinks[roleId] != null)
            {
                if (currentLanguage == null)
                {

                    if (this.supportedLanguages.Count > 0)
                    {
                        currentLanguage = supportedLanguages[0] as string;
                    }
                    else
                    {
                        currentLanguage = "en";
                    }


                }
                if (currentLabelRole == null)
                {
                    currentLabelRole = PresentationLocator.preferredLabelRole;
                }
                DimensionNode ret;
                if (netDefinisionInfo.TryGetMeasureDimensionNodesForRole(currentLanguage, currentLabelRole, roleId, this,
                    out ret))
                {

                    return ret;
                }

            }




            return null;
        }

        


		public List<DimensionNode> GetHyperCubeNodesForRole(string roleId)
		{
			errorList.Clear();
			if (netDefinisionInfo  == null )
			{
				throw new AucentException("XBRLParser.Error.MustLoadAndParse");
			}

			DefinitionLink dlink = netDefinisionInfo.DefinitionLinks[roleId] as DefinitionLink;



			if (dlink  != null)
			{
				if( dlink.HyperCubeNodesList.Count == 0 )
				{
					//good idea to build all the hypercube nodes...for all the reports...
					if (currentLanguage == null)
					{

						if (this.supportedLanguages.Count > 0)
						{
							currentLanguage = supportedLanguages[0] as string;
						}
						else
						{
							currentLanguage = "en";
						}


					}
					if (currentLabelRole == null)
					{
						currentLabelRole = PresentationLocator.preferredLabelRole;
					}
					List<DimensionNode> tmp;
					this.TryGetAllDimensionNodesForDisplay(currentLanguage, currentLabelRole,
						true, out tmp);

					this.TryGetAllDimensionNodesForDisplay(currentLanguage, currentLabelRole,
						false, out tmp);
	
				}
				return dlink.HyperCubeNodesList;
			}


			return null;
		}


		/// <summary>
		/// Returns flat linked-list of nodes by calculation view.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetNodesByCalculation()
		{
			errorList.Clear();

            if (currentLanguage == null)
            {

                if (this.supportedLanguages.Count > 0)
                {
                    currentLanguage = supportedLanguages[0] as string;
                }
                else
                {
                    currentLanguage = "en";
                }


            }

            if (currentLabelRole == null)
            {
                currentLabelRole = PresentationLocator.preferredLabelRole;
            }

			ArrayList nodeList = new ArrayList();
			if ( calculationInfo == null || calculationInfo.Count == 0 )
			{
				return nodeList;
			}

			IDictionaryEnumerator enumer = calculationInfo.GetEnumerator();

			while ( enumer.MoveNext() )
			{
                Node n = (enumer.Value as PresentationLink).CreateNode(currentLanguage, currentLabelRole, null);
                if (n != null)
                {
                    nodeList.Add(n);

                }
			}

			//sort the top level (the presentationLink titles)
			nodeList.Sort();

			//sort the first level under each presentationLink (there is no order)
			foreach ( Node childNode in nodeList )
			{
				if ( childNode.Children != null )
				{
					childNode.Children.Sort();
				}
			}

			return nodeList;
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="style"></param>
		/// <param name="includeSegmentDimensions"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public ArrayList GetNodes(PresentationStyle style,    bool includeSegmentDimensions, out int errors)
		{
			errors = 0;
			switch (style)
			{
				case PresentationStyle.Calculation:
					return GetNodesByCalculation();


				case PresentationStyle.Element:
					return GetNodesByElement();
				//break;

				case PresentationStyle.Presentation:
					return GetNodesByPresentation(includeSegmentDimensions);
				//break;
			}

			return null;
		}

		

		/// <summary>
		/// Returns the list of label roles for this Taxonomy.
		/// </summary>
		/// <param name="forceReparse">This parameter is not used. It needs to be removed but requires recompile of other products.</param>
		/// <param name="numErrors"></param>
		/// <returns>List of strings representing the supported label roles for this Taxonomy.</returns>
		public ArrayList GetLabelRoles( bool forceReparse, out int numErrors )
		{
			numErrors = 0;

			if (labelRoles.Count > 0)
			{
				return labelRoles;
			}
			else
			{
				ArrayList tempList = new ArrayList();
				tempList.Add( LABEL );

				return tempList;
			}
		}

		/// <summary>
		/// Returns the list of supported languages for this Taxonomy. 
		/// </summary>
		/// <param name="forceReparse">This parameter is not used. It needs to be removed but requires recompile of other products.</param>
		/// <param name="numErrors"></param>
		/// <returns>List of strings representing the supported languages for this Taxonomy.</returns>
		public ArrayList GetSupportedLanguages( bool forceReparse, out int numErrors )
		{
			numErrors = 0;

			if (supportedLanguages.Count > 0)
			{
				return supportedLanguages;
			}
			else
			{
				ArrayList tempList = new ArrayList();
				tempList.Add("en");

				return tempList;
			}
		}


		/// <summary>
		/// determine if a particular dependent taxonomy is loaded.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private bool IsDependentTaxonomyLoaded( string fileName )
		{
			if ( string.Compare( fileName, schemaFilename, true ) == 0 )
			{
				return true;
			}

			//sometimes the href in the dependant taxomomy references just the scheam file name without the 
			//extensions so it is beter to do this check before we assume that we need to load the dependant 
			//taxonomy.
			if ( schemaFilename.ToLower().Contains( fileName.ToLower() ) ) return true;


			if ( allFiles != null )
			{
				foreach ( string fn in allFiles )
				{
					if ( string.Equals(fn, fileName, StringComparison.OrdinalIgnoreCase )) return true;
				}

			}
			return false;
		}


		private void LoadAdditionalDependantTaxonomy( string fileName, string hintPath, ref int numErrors )
		{
			numErrors = 0;
			if ( IsDependentTaxonomyLoaded( fileName ) ) return;

			int origDepTaxCount = this.dependantTaxonomies.Count;

			AddDependantTaxonomy(fileName, hintPath, string.Empty,   ref numErrors);

			if ( dependantTaxonomies.Count == origDepTaxCount ) return;


			ArrayList newTaxList = new ArrayList();
			newTaxList.AddRange( dependantTaxonomies.GetRange( origDepTaxCount, dependantTaxonomies.Count - origDepTaxCount ) );

			

			ParseDependentTaxonomies( newTaxList, ref numErrors );

			
			

		}

		private int LoadDependantTaxonomies( string parentLocation )
		{
			int myErrors = 0;
			if ( theDocument == null )
				return myErrors; //there are no dependant taxonomies.
			XmlNodeList impList = theDocument.SelectNodes( IMPORT_KEY, theManager );

			if ( impList == null || impList.Count == 0 )
			{
				Common.WriteInfo( "XBRLParser.Info.NoImports", schemaFilename );
				return myErrors; // nothing to do
			}

			foreach ( XmlNode imp in impList )
			{
				string ns = null;
				string sl = null;

				if ( !IsValidImport( imp, ref ns, ref sl, ref errorList ) )
				{
					continue;
				}

				// found an honest to goodness import

				//TODO: Allow http paths as well
				//string fn = schemaPath + "\\" + sl;

				string hintpath = null;
				string filename = null;
				int lastslash = -1;

				if ( sl.IndexOf( '\\' ) != -1 )
				{
					lastslash = sl.LastIndexOf( '\\' );
				}
				else if ( sl.IndexOf( '/' ) != -1 )
				{
					lastslash = sl.LastIndexOf( '/' );
				}
				else
				{
					filename = sl;
				}

				if ( lastslash != -1 )
				{
					hintpath = sl.Substring( 0, lastslash );
					filename = sl.Substring( lastslash + 1, sl.Length - lastslash - 1 );
				}

				AddDependantTaxonomy( filename, hintpath, ns,  ref myErrors );

			}







			return myErrors;
		}


		private void AddDependantTaxonomy(string filename, string hintpath, string ns, ref int myErrors)
		{
			// compare against ourself
			if (string.Compare(filename, schemaFilename, true) == 0)
			{
				throw new AucentFatalException("XBRLParser.Error.RecursiveTaxonomies");
			}

			string fileNameWithoutPath = Path.GetFileName(filename);

			int index = allFiles.BinarySearch(fileNameWithoutPath);
			if (index >= 0 || IsDependentTaxonomyLoaded(fileNameWithoutPath))
			{
				Console.WriteLine("Taxonomy.GetDependantTaxonomies (" + schemaFile + "): Skip: " + fileNameWithoutPath);
				return; //nothing to add as the file is already loaded.
			}


			if (TaxonomyCacheManager != null)
			{
				Taxonomy depTax = TaxonomyCacheManager.GetTaxonomyByFileName(fileNameWithoutPath);

				if (depTax != null)
				{
					allFiles.Insert(~index, fileNameWithoutPath);

					//we found the dependant taxonomy in the cache .. we need to copy it to 
					// make sure that we are not messing with the original
					Taxonomy copyToAdd = depTax.CopyTaxonomyForMerging();
					if (copyToAdd.allFiles != null)
					{
						this.allFiles.AddRange(copyToAdd.allFiles);
						this.allFiles.Sort();
					}
					dependantTaxonomies.Add(copyToAdd);
					dependantTaxonomyFilenames.Add(copyToAdd.schemaFilename);



					return;
				}

			}
			string fn = filename;
			string schemaFolder = schemaPath.Replace("\\", "/");
			if (!schemaFolder.EndsWith("/"))
			{
				schemaFolder = schemaFolder + "/";
			}

			
			if (hintpath != null)
			{
				hintpath = hintpath.Replace("\\", "/");
			}
			//hint could be relative.. in which case we need to combine it with the 
			//schema location of the taxonomy...
			if (hintpath == null || hintpath.Length == 0 || hintpath.StartsWith("..") || hintpath.StartsWith("/"))
			{
				if (hintpath != null && hintpath.Length > 0)
				{
					fn = schemaFolder + hintpath + "/" + filename;
				}
				else
				{
					fn = schemaFolder +  filename;

				}
			}
			else if (hintpath != null )
			{
				if (hintpath.EndsWith("/"))
				{
					fn = hintpath  + filename;

				}
				else
				{

					fn = hintpath + "/" + filename;
				}
			}
			

			

			bool local = false;

			DateTime lastModified = DateTime.MinValue;
			bool URLExists;
			Dictionary<string, bool> filesAlreadyChecked = new Dictionary<string, bool>();
			bool isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
			filesAlreadyChecked[fn] = true;
			if (!isValid && Directory.Exists(schemaFolder))
			{
				fn = schemaFolder + "/" + filename;
				isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
				filesAlreadyChecked[fn] = true;

				if (!isValid)
				{
					fn = schemaFolder + hintpath + "/" + filename;
					isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
					filesAlreadyChecked[fn] = true;

				}
			}
	

			


			while (true )
			{
				
				#region Check other locations if we cannot find the file in the location it is supposed to be in

				//check the following locations
				//ns + filename 
				//hintpath + file name
				//parent location + hint path + filename
				//parent location + filename 
				//current schema path + hint path + file name
				//current schema path + file name
				//ns + hint path + filename 
				//just file name
				//isolated storage
				bool localPromptUser = false;

				if( !isValid)
				{
					//hintpath + file name
					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(hintpath, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}

				if (!isValid)
				{
					//parent location + hint path + filename
					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(schemaPath + hintpath, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}
				if (!isValid)
				{
					//parent location + filename 
					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(schemaPath, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}
				if (!isValid)
				{
					//ns + filename 
					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(ns, filename);
					if (!isValid && !filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}

				if (!isValid)
				{
					//current schema path + hint path + file name
					string path2 = string.Empty;

					if (local)
						path2 = schemaPath + Path.DirectorySeparatorChar + hintpath;
					else
						path2 = schemaPath + "/" + hintpath;

					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(path2, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}
				if (!isValid)
				{
					//current path + filename try - 
					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(schemaPath, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}
				if (!isValid)
				{
					//ns + hint path + filename 
					string path2 = string.Empty;
					if (local)
						path2 = ns + Path.DirectorySeparatorChar + hintpath;
					else
						path2 = ns + "/" + hintpath;

					fn = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(path2, filename);
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}
				}
				if (!isValid)
				{
					fn = filename;
					if (!filesAlreadyChecked.ContainsKey(fn))
					{
						isValid = ValidateFileExistance(fn, false, out local, out fn, out lastModified, out  URLExists);
						filesAlreadyChecked[fn] = true;
					}

				}

				if (!isValid)
				{
                    localPromptUser = true;
                    // don't bother to test this one - we're done trying
                    fn = filename;
					

				}

				Console.WriteLine("Taxonomy.GetDependantTaxonomies (" + schemaFile + "): Load: " + fn);
				#endregion

				Taxonomy t = new Taxonomy(true);
				t.PromptUser = this.PromptUser;
				t.OwnerHandle = OwnerHandle;
                //set the loading tax to the current tax or its loading tax object...
                t.loadingTaxonomy = GetLoadingTaxonomy();

				// continue adding all files
				t.allFiles = allFiles;
				if (this.TopLevelTaxonomy != null)
				{
					t.TopLevelTaxonomy = this.TopLevelTaxonomy;
				}
				else
				{
					t.TopLevelTaxonomy = this;
				}

				try
				{
                    if (t.Load(fn, localPromptUser && t.PromptUser ) != 0)
					{

						if (!localPromptUser)
						{
							//we found a valid file that does not exist....
							//lets see if we can find a different file this time
							continue;
						}
						else
						{
							Common.WriteError("XBRLParser.Error.CantLoadFilename", errorList, fn, " to find the file");
							++myErrors;
							string errorMsg = null;
							if (errorList != null && errorList.Count > 0)
							{
								errorMsg = errorList[errorList.Count - 1] as string;

                                if (errorMsg == null && errorList[errorList.Count - 1] is ParserMessage)
                                {
                                    ParserMessage msg = errorList[errorList.Count - 1] as ParserMessage;
                                    if (msg != null)
                                    {
                                        errorMsg = msg.Message;
                                    }
                                }
                               
							}
							else
							{
								errorMsg = string.Format("Failed to load file {0}.", fn);
							}
							throw new AucentFatalException(errorMsg);

						}
					}
				}
				catch (XmlException xe)
				{
					Common.WriteError("XBRLParser.Error.CantLoadFilename", errorList, fn, xe.Message);
					++myErrors;
					return;
				}

				if (BuildTaxonomyRelationship)
				{
					if (DirectDependantTaxonomies == null) DirectDependantTaxonomies = new List<string>();
					if (!DirectDependantTaxonomies.Contains(fn))
					{
						DirectDependantTaxonomies.Add(fn);
					}
				}



				allFiles.Insert(~index, fileNameWithoutPath);

				dependantTaxonomyFilenames.Add(fn);
				dependantTaxonomies.Add(t);

				int errors = t.LoadDependantTaxonomies(t.schemaPath);

				dependantTaxonomies.AddRange(t.dependantTaxonomies);
				dependantTaxonomyFilenames.AddRange(t.dependantTaxonomyFilenames);


				if (errors > 0)
				{
					errorList.Add(t.errorList);
					myErrors += errors;
				}

				break;
			}

		}

		private static bool IsValidImport( XmlNode imp, ref string ns, ref string sl, ref ArrayList errorList )
		{
			if ( !Common.GetAttribute( imp, NS_TAG, ref ns, errorList ) ||
				!Common.GetAttribute( imp, SL_TAG, ref sl, errorList ) )
			{
				//++myErrors; not an error, just a warning
				return false;	// Couldn't find an attribute, just ignore it
			}


			// ignore the linkbase , dimension xsd, definision xsd and instance imports for now - maybe always?
			if ( ns.EndsWith( INSTANCE_TAG ) ||
				ns.EndsWith( LINKBASE_TAG ) ||
				ns.EndsWith( DimensionTAG ) ||
				ns.EndsWith( DefinitionTAG ) ||
				ns.EndsWith( REF_TAG ) )
			{
				//Common.WriteInfo( "XBRLParser.Info.FoundInstanceOrLinkbase" );
				return false;
			}
			return true;
		}

		private ArrayList GetDependantTaxonomies( out int numErrors )
		{
			errorList.Clear();
			numErrors = 0;
			if (dependantTaxonomyFilenames.Count > 0)
				return new ArrayList(dependantTaxonomyFilenames);

			dependantTaxonomies.Clear();
			try
			{
				numErrors += LoadDependantTaxonomies(this.schemaPath);
			}
			catch ( AucentFatalException afe )
			{
				throw afe;
			}
			catch ( AucentException ae )
			{
				Common.WriteError( ae.ResourceKey, errorList );
				++numErrors;
			}
			return new ArrayList(dependantTaxonomyFilenames);
		}

		/// <summary>
		/// Gets the list of dependent taxonomy files names.
		/// </summary>
		/// <param name="forceReparse">If True, then the list of dependent taxonomies is rebuilt before returning the list.
		/// If False, then whatever is in the list of dependant taxonomy filenames is returned if there is 1 or more in the list.</param>
		/// <param name="numErrors"></param>
		/// <returns>Collection of strings representing the dependant taxonomy filenames.</returns>
		public ArrayList GetDependantTaxonomies( bool forceReparse, out int numErrors )
		{
			errorList.Clear();

			//ArrayList dts = new ArrayList();
			numErrors = 0;

			if ( dependantTaxonomyFilenames.Count > 0 )
			{
				if ( !forceReparse )
				{
					return new ArrayList(dependantTaxonomyFilenames);
				}

				// otherwise, fallthrough
				dependantTaxonomies.Clear();
			}

			try
			{
				numErrors += LoadDependantTaxonomies(this.schemaPath);
			}
			catch ( AucentFatalException afe )
			{
				throw afe;
			}
			catch ( AucentException ae )
			{
				Common.WriteError( ae.ResourceKey, errorList );
				++numErrors;
			}

			return new ArrayList(dependantTaxonomyFilenames);
		}

		/// <summary>
		/// Finds the target namespace in the Xml document.
		/// </summary>
		/// <returns>String that is the target namespace of the taxonomy.</returns>
		/// <exception cref="AucentException">thrown if the TargetNamespace is not found</exception>
		public string GetTargetNamespace()
		{
			if ( targetNamespace == null )
			{
				if ( theDocument != null && theDocument.DocumentElement != null && theDocument.DocumentElement.Attributes != null )
				{
					XmlNode tn = theDocument.DocumentElement.Attributes.GetNamedItem( TARGET_NAMESPACE );
					if ( tn != null )
					{
						targetNamespace = tn.Value;
					}
					else
					{
						throw new AucentException( "Target Namespace not found" );
					}
				}
				else
				{
					throw new AucentException( "Target Namespace not found" );
				}
			}

			return targetNamespace;
		}

		/// <summary>
		/// Gets the Taxonomy namespace prefix.
		/// If the namespace prefix is null, then this method looks it up in the xml document.
		/// </summary>
		/// <returns></returns>
		public string GetNSPrefix()
		{
			if ( nsPrefix != null ) return nsPrefix;

			// make sure we're initialized
			string tn = GetTargetNamespace();

			nsPrefix = theManager.LookupPrefix( theManager.NameTable.Get( tn ) );

			return nsPrefix;
		}

		/// <summary>
		/// Navigates the Presentation hierarchy to the given path, and returnes the node at that
		/// Location.  Returns null if the path is not found in the presentation
		/// </summary>
		/// <param name="path">The location in the Presentation hierarchy to return the node from</param>
		/// <param name="includeDimensions">Should the Dimension hierarchy be included in the presentation hierarchy</param>
		/// <returns>The node at 'path' or null if 'path' is not valid</returns>
		public Node GetNodeFromPresentation(string path, bool includeDimensions)
		{
			string[] pathPieces = path.Split('\\');
			ArrayList presentationNodes = this.GetNodesByPresentation(includeDimensions);

			//Get the first node in the path from the array list
			Node currNode = null;
			string currId = pathPieces[0];
			foreach (Node node in presentationNodes)
			{
				//The top level nodes in the arraylist should all be PresentationLinks,
				//so compare the Title to the Id in the path
				if(node.MyPresentationLink.Title == currId)
				{
					currNode = node;
					break;
				}
			}
			//If the node was not found, return null
			if (currNode == null)
			{
				return null;
			}

			//Search the node's children collection to find each subsequent level in the path
			for (int i = 1; i < pathPieces.Length; i++)
			{
				
				currId = pathPieces[i];
				Node childNode = null;
				foreach (Node node in currNode.children)
				{
					if (node.Id == currId)
					{
						childNode = node;
						break;
					}
				}
				//The node at this level was not found, return null
				if (childNode == null)
				{
					return null;
				}
				currNode = childNode;
			}
			return currNode;
		}

		#endregion

		#region Merge discovered taxonomies

		private void MergeDiscoveredTaxonomy(string fn, Taxonomy depTax, out int numErrors )
		{
			numErrors = 0;
			if (this.dependantTaxonomyFilenames.Contains(fn)) return;

			dependantTaxonomyFilenames.Add(fn);
			dependantTaxonomies.Add(depTax);

			this.infos.Add(new TaxonomyItem(depTax.GetTargetNamespace(), depTax.schemaFile, depTax.GetNSPrefix(), depTax.IsAucentExtension, depTax.DefinesCustomTypes));
			ArrayList tmp;


			//merge the presentation, calculation, definition linkbases...

			if (!this.innerTaxonomy)
			{
				presentationInfo = MergePresentations(presentationInfo, depTax.presentationInfo,
					out tmp);
				depTax.presentationInfo = null;
				if (tmp.Count > 0)
				{
					numErrors += tmp.Count;
					errorList.AddRange(tmp);
				}



				calculationInfo = MergePresentations(calculationInfo,
					depTax.calculationInfo, out tmp);
				if (tmp.Count > 0)
				{
					numErrors += tmp.Count;
					errorList.AddRange(tmp);
				}
				if (this.netDefinisionInfo == null)
				{
					this.netDefinisionInfo = depTax.netDefinisionInfo;
				}
				else
				{
					netDefinisionInfo.MergeDimensionLinks(depTax.netDefinisionInfo, out tmp);
					if (tmp.Count > 0)
					{
						numErrors += tmp.Count;
						errorList.AddRange(tmp);
					}

				}
			}
		}

		#endregion

		private int NumElements()
		{
			int len = 0;
			IDictionaryEnumerator enumer = presentationInfo.GetEnumerator();

			while ( enumer.MoveNext() )
			{
				PresentationLink pl = enumer.Value as PresentationLink;
				len += pl.LocatorCount;
			}

			len = Math.Max( len, allElements.Count );

			return len;
		}


		#region Validation

		/// <summary>
		/// Method used for validation. Returns a dictionary of element ids (key)
		/// and a list of element ids (value) that the element depends on. 
		/// That means that if you report the element in an instance document,
		/// you have to report the other required elements also for the instance document
		/// to be valid.
		/// An example of this would be the UKs Company House filings. 
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, List<string>> GetRequiredElementRelationshipInfo()
		{
			if ( this.netDefinisionInfo == null ) return null;

			return netDefinisionInfo.GetRequiredElementRelationshipInfo();
		}

		public void ValidateInstanceInformationForRequiresElementCheck( Instance ins,
			out string[] validationsFailures )
		{
			validationsFailures = null;
			if ( this.NetDefinisionInfo == null ) return;

			Hashtable uniqueElementIds = new Hashtable();
			foreach ( MarkupProperty mp in ins.markups )
			{
				uniqueElementIds[mp.elementId] = 1;
			}

			ValidateInstanceInformationForRequiresElementCheck( uniqueElementIds, out validationsFailures );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uniqueElementIds"></param>
		/// <param name="validationsFailures"></param>
		public void ValidateInstanceInformationForRequiresElementCheck(
			Hashtable uniqueElementIds, out string[] validationsFailures )
		{

			validationsFailures = null;
			if ( netDefinisionInfo == null ) return;

			Dictionary<string, List<string>> requiresElementRelationShips
				= this.NetDefinisionInfo.GetRequiredElementRelationshipInfo();
			if ( requiresElementRelationShips == null ) return;

			ArrayList errorStrings = new ArrayList();
			//get the actual element id from the id in the requiresElementRelationShips
			//as id as defined in the xsd does not always equal to the prefix_name of the element
			Hashtable ElementIdHrefMap = new Hashtable();
			foreach ( string href in requiresElementRelationShips.Keys )
			{
				Element ele = this.allElements[href] as Element;

                ElementIdHrefMap[ele.GetNameWithNamespacePrefix()] = href;
			}

			foreach ( string elementId in uniqueElementIds.Keys )
			{
				if ( ElementIdHrefMap[elementId] != null )
				{
					//found a requires element relationship..
					//make sure thal all the dependend elements exists in the instance doc
					string href = ElementIdHrefMap[elementId] as string;
					List<string> depHrefs = requiresElementRelationShips[href] as List<string>;

					foreach ( string dhref in depHrefs )
					{
						Element ele = this.allElements[dhref] as Element;
                        if (uniqueElementIds[ele.GetNameWithNamespacePrefix()] == null)
						{
							errorStrings.Add( string.Format( "Required element {0}, is missing in the instance document", ele.Name ) );

						}

					}
				}
			}

			validationsFailures = errorStrings.ToArray( typeof( string ) ) as string[];

		}



		/// <summary>
		/// Validates the Taxonomy and all of its dependent taxonomies (if any), plus all link base references.  
		/// </summary>
		/// <returns>A <see cref="ValidationStatus"/> containing the validation results.  <see cref="ValidationStatus.ERROR"/>
		/// will be returned if any exception is thrown.  <see cref="ValidationStatus.ERROR"/> will be returned if 
		/// there is one or more validation errors.  <see cref="ValidationStatus.WARNING"/> will be returned if there
		/// are no errors and there is one or more validation warnings.</returns>
		public ValidationStatus Validate()
		{
			ValidationStatus VS = ValidationStatus.OK;
			try
			{
				int tmp = 0;
				this.Parse( out tmp );

			}
			catch ( AucentException ae )
			{
				Common.WriteError( ae.ResourceKey, validationErrors );
				VS = ValidationStatus.ERROR;

				// don't swallow this - we need to abort processing
				throw ae;
			}
			catch ( Exception Err )
			{
				Common.WriteError( "XBRLParser.Error.Exception", validationErrors,
					"Validate Error: " + Err.Message );
				VS = ValidationStatus.ERROR;
			}
			finally
			{
				if ( validationWarnings.Count > 0 )
					VS = ValidationStatus.WARNING;
				if ( this.validationErrors.Count > 0 )
					VS = ValidationStatus.ERROR;
			}
			return VS;
		}

        /// <summary>
        /// build the data required to validate dimension information...
        /// </summary>
        /// <returns></returns>
        public bool TryBuildDimensionValidationInformation()
        {
            if (this.netDefinisionInfo != null)
            {
                netDefinisionInfo.BuildDimensionValidationInformation(this);

                return true;
            }

            return false;
        }

        /// <summary>
        /// for a given element id, validate if the segment list and scenarios used is valid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="segments"></param>
        /// <param name="scenarios"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool IsDimensionInformationValid(string id, ArrayList segments,
            ArrayList scenarios, out string error )
        {
            error = null;
            if (this.netDefinisionInfo != null)
            {
                return netDefinisionInfo.IsDimensionInformationValid(id, segments, scenarios, out error  );

            }

            return true;
        }

		#endregion

		#region Xml string creation

		/// <summary>
		/// Returns an Xml representation of this object as a string.
		/// </summary>
		/// <returns></returns>
		public override string ToXmlString()
		{
			return ToXmlString( true );
		}

		internal string ToXmlString( bool verbose )
		{
			int len = 0;
			try
			{
				len = NumElements() * 200;	// assume 200 characters per element
			}
			catch ( OverflowException )
			{
				len = NumElements();
			}

			StringBuilder text = new StringBuilder( len );
			ToXmlString( 0, verbose, "en", text );

			return text.ToString();
		}

		internal string ToXmlString( ArrayList nodes )
		{
			int len = 0;
			try
			{
				len = NumElements() * 200;	// assume 200 characters per element
			}
			catch ( OverflowException )
			{
				len = NumElements();
			}

			StringBuilder text = new StringBuilder( len );
			ToXmlString( 0, false, "en", text );

			return text.ToString();
		}

		/// <summary>
		/// Writes the nodes in a parameter-supplied <see cref="ArrayList"/> of <see cref="Node"/>
		/// objects to a parameter-supplied XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="numTabs">The number of tab characters to be appended <paramref name="xml"/>
		/// before each XML node is written.</param>
		/// <param name="verbose"></param>
		/// <param name="lang">The language code for which <see cref="LabelLocator"/> objects within
		/// this <see cref="Node"/> are to be written.  See <see cref="Element.WriteXmlFragment"/>.</param>
		/// <param name="xml">The output XML to which this <paramref name="Node"/> is to be appended.</param>
		/// <param name="nodes">The <see cref="ArrayList"/> of <see cref="Node"/> objects that are be 
		/// appended to <paramref name="xml"/>.</param>
		private void ToXmlString( int numTabs, bool verbose, string lang, StringBuilder xml, ArrayList nodes )
		{
			WriteHeader( numTabs, xml );

			foreach ( Node n in nodes )
			{
				n.ToXmlString( numTabs + 1, verbose, lang, xml );
			}

			WriteFooter( numTabs, xml );
		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <param name="numTabs">Not used.</param>
		/// <param name="xml">Not used.</param>
		private void WriteHeader( int numTabs, StringBuilder xml )
		{
			//			if ( numTabs == 0 )
			//			{
			//				xml.Append( "<?xml version=\"1.0\" encoding=\"utf-8\"?>").Append(Environment.NewLine );
			//				xml.Append( "<!-- Linkbase based on XBRL standard v.2.1  Created by Aucent XBRLParser ").Append( ParserMessage.ExecutingAssemblyVersion ).Append( " Contact www.aucent.com -->" ).Append( Environment.NewLine );
			//			}
			//			else
			//			{
			//				for ( int i=0; i < numTabs; ++i )
			//				{
			//					xml.Append( "\t" );
			//				}
			//			}
			//
			//			xml.Append( "<" ).Append( HEADER_STR ).Append(" file=\"" ).Append( schemaFile ).Append( "\">" ).Append( Environment.NewLine );

		}

		/// <summary>
		/// Appends the current <see cref="HEADER_STR"/> to a parameter supplied XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="numTabs">The number of tab characters to be appended to <paramref name="xml"/> before 
		/// <see cref="HEADER_STR"/> is appended.</param>
		/// <param name="xml">The XML <see cref="StringBuilder"/> to which <see cref="HEADER_STR"/> is to be 
		/// appended.</param>
		private void WriteFooter( int numTabs, StringBuilder xml )
		{
			for ( int i = 0; i < numTabs; ++i )
			{
				xml.Append( "\t" );
			}

			xml.Append( "</" ).Append( HEADER_STR ).Append( ">" ).Append( Environment.NewLine );
		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		/// <param name="numTabs">Not used.</param>
		/// <param name="verbose">Not used.</param>
		/// <param name="language">Not used.</param>
		/// <param name="xml">Not used.</param>
		public override void ToXmlString( int numTabs, bool verbose, string language, StringBuilder xml )
		{
			System.Diagnostics.Debug.Assert( false, "Don't use this deprecated ToXmlString method." );

			//			WriteHeader( numTabs, xml );
			//
			//			IDictionaryEnumerator enumer = presentationInfo.GetEnumerator();
			//
			//			while ( enumer.MoveNext() )
			//			{
			//				PresentationLink pl = enumer.Value as PresentationLink;
			//				pl.ToXmlString( numTabs+1, verbose, language, xml );
			//			}
			//
			//			WriteFooter( numTabs, xml );
		}
		#endregion



		#region namespace fixer

		/// <summary>
		/// Takes a web URL and fixes the string so that it is a format that we support.
		/// If it starts with "http", then check for a valid URL.
		/// If we find it's valid, we return the validated URL. 
		/// If it is not valid, we strip off everything before the last "/" to return the file name only.
		/// </summary>
		/// <param name="origName"></param>
		/// <returns></returns>
		public static string FixURLFileName( string origName )
		{
			if (origName.StartsWith("http"))
			{
				//need to make sure that the web location is valid.
				string actualURL;
				if (!RemoteFiles.IsValidURL(origName, out actualURL))
				{
					//need to strip out the folder info
					int index = origName.LastIndexOf("/") + 1;
					if (origName.Length > index)
					{
						return origName.Substring(index, origName.Length - index);
					}
				}
				else
				{
					return actualURL;
				}
			}

			return Path.GetFileName(origName);
		}

		/// <summary>
		/// Adjusts the URL file name to convert the absolute path web URLs to
		/// relative paths. Path is made relative to the root taxonomy. 
		/// </summary>
		/// <param name="origName"></param>
		/// <returns></returns>
		public string AdjustURLFileNameInExtensions( string origName )
		{
			if (origName.StartsWith("http"))
			{
				//need to make sure that the web location is valid.
				string actualURL;
				if (!RemoteFiles.IsValidURL(origName, out actualURL))
				{
					return GetCorrectRelativePath(Path.GetFileName(origName), origName);

				}
				else
				{
					return actualURL;
				}

			}

			return GetCorrectRelativePath(Path.GetFileName(origName), origName);

		}

        /// <summary>
        /// we might be loading linkbase filename locally but we know the local location of the tax file
        /// and we also know the web location of the tax...
        /// we might have to get the relative position of the linkbase file from the local tax and then use it 
        /// relative to the tax web location.....
        /// </summary>
        /// <param name="lbFileName"></param>
        /// <returns></returns>
        public string AdjustLinkbaseFileNameInExtensions(string lbFileName)
        {
           
			if (lbFileName.StartsWith("http"))
			{
				//need to make sure that the web location is valid.
				string actualURL;
				if (RemoteFiles.IsValidURL(lbFileName, out actualURL))
				{
					return actualURL;

				}

			}
			string ret;
			if (TryConvertAbolutePathtoRelative(TaxonomyItems[0].Location, lbFileName, out ret))
			{

				return ret;
			}


			return lbFileName;

		}


       

     


		private string GetCorrectRelativePath(string fileName, string fullFileName)
		{
			//need to make it relative to the root taxonomy
			//i.e. relative to TaxonomyItem[0];
			for (int i = 0; i < this.infos.Count; i++)
			{
				if (this.infos[i].Location.Contains(fileName))
				{
					string ret;
					if (TryConvertAbolutePathtoRelative(infos[0].Location, infos[i].Location, out ret ))
					{
						return ret;
					}

					return infos[i].Location;
				}
			}
			if (this.infos.Count > 0)
			{
				string ret;
				if (TryConvertAbolutePathtoRelative(infos[0].Location, fullFileName, out ret))
				{
					return ret;
				}

				return fullFileName;
			}

			return fileName;

		}

		/// <summary>
		/// Create a relative path for the file.....
		/// </summary>
		/// <param name="CurrentFile"></param>
		/// <param name="FileToMakeRelative"></param>
		/// <param name="newRelativePath"></param>
		/// <returns></returns>
		public static bool TryConvertAbolutePathtoRelative(string CurrentFile, string FileToMakeRelative, out string newRelativePath )
		{
			newRelativePath = FileToMakeRelative;
			string path1 = Path.GetDirectoryName(CurrentFile);
			string path2 = Path.GetDirectoryName(FileToMakeRelative);

			if (path1.Length > 0 && path2.Length == 0) return true;

			path1 = path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			path2 = path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);


			string[] path1Parts = path1.Split(Path.DirectorySeparatorChar);
			string[] path2Parts = path2.Split(Path.DirectorySeparatorChar);

			string relativeFile = Path.GetFileName(FileToMakeRelative);

			if (path1Parts[0].ToLower() == path2Parts[0].ToLower())
			{
				int countToIgnore = 1;

				for (int i = 1; i < path1Parts.Length && i < path2Parts.Length; i++)
				{
					if (path1Parts[i].ToLower() == path2Parts[i].ToLower())
					{
						countToIgnore++;
						continue;
					}

					break;
				}

				int moveUp = path1Parts.Length - countToIgnore;

				StringBuilder relativePath = new StringBuilder();
				if (moveUp > 0)
				{
					for (int i = 0; i < moveUp; i++)
					{
						relativePath.Append("../");
					}
				}
				else
				{

					if (countToIgnore == path2Parts.Length)
					{
						newRelativePath = relativeFile;
						return true;
					}
					relativePath.Append("./");
				}


				for (int i = countToIgnore; i < path2Parts.Length; i++)
				{
					relativePath.Append(path2Parts[i]);
					relativePath.Append("/");

				}

				newRelativePath = relativePath.ToString() + relativeFile;



				return true;

			}
			else
			{
				return false;
			}
		}

		#endregion

		#region binary serialization

		internal static void Serialize( Taxonomy currentTaxonomy, string fileName, out long FileSize )
		{
			FileStream fs = new FileStream(fileName, FileMode.Create);

			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				formatter.Serialize(fs, currentTaxonomy);
				FileSize = fs.Length;
			}
			catch (SerializationException e)
			{
				Console.WriteLine("Failed to serialize: " + e.Message);
				System.Diagnostics.Debug.WriteLine("Failed to serialize: " + e.Message);
				throw;
			}
			finally
			{
				fs.Close();
			}
		}

		private static void Deserialize( string fileName, out Taxonomy currentTaxonomy )
		{
			FileStream fs = new FileStream(fileName, FileMode.Open);
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();

				currentTaxonomy = (Taxonomy) formatter.Deserialize(fs);
			}
			catch (SerializationException e)
			{
				Console.WriteLine("Failed to serialize: " + e.Message);
				System.Diagnostics.Debug.WriteLine("Failed to deserialize: " + e.Message);
				throw;
			}
			finally
			{
				fs.Close();
			}
		}

		#endregion

		/// <summary>
		/// Clones all the information that the taxonomy needs so that this taxonomy can 
		/// be used as a dependent taxonomy and can get imported into the other taxonomy
		/// when a taxonomy gets imported, its pointers gets mixed with the pointers in the main taxonomy
		/// and hence its information gets modified. That is why we need to clone the taxonomy before
		/// merging it with another taxonomy.
		/// 
		/// Some of the information is not deep copied as the information is not affected by 
		/// merging. 
		/// </summary>
		/// <returns>The cloned <see cref="Taxonomy"/>.</returns>
		public Taxonomy CopyTaxonomyForMerging()
		{
			lock (this)
			{
				Taxonomy clone = new Taxonomy();
				clone.infos = new List<TaxonomyItem>(this.infos.Count);
				for (int i = 0; i < this.infos.Count; i++)
				{
					clone.infos.Add(this.infos[i].CloneTaxonomyItem());
				}
				clone.HasPresentation = this.HasPresentation;
				clone.HasCalculation = this.HasPresentation;

				clone.nsPrefix = this.nsPrefix;
				clone.targetNamespace = this.targetNamespace;

				clone.supportedLanguages.AddRange(this.supportedLanguages);
				if (this.allFiles != null)
				{
					clone.allFiles.AddRange(this.allFiles);
				}


				clone.labelRoles.AddRange(this.labelRoles);

				clone.currentLanguage = this.currentLanguage;
				clone.currentLabelRole = this.currentLabelRole;
				clone.aucentExtension = this.aucentExtension;
				clone.PromptUser = this.PromptUser;

				clone.schemaFilename = this.schemaFilename;
				clone.schemaFile = this.schemaFile;
				clone.schemaPath = this.schemaPath;
				clone.schemaPathURI = this.schemaPathURI;

				#region Copy Label
				if (this.labelHrefHash != null)
				{
					clone.tmplabelTable = new Hashtable();
					foreach (LabelLocator ll in this.labelHrefHash.Values)
					{
						clone.tmplabelTable[ll.Label] = ll.CreateCopyForMerging();
					}
				}

				#endregion

				#region Copy Calculation
				if (this.calculationInfo != null)
				{
					clone.calculationInfo = new Hashtable();
					foreach (DictionaryEntry de in this.calculationInfo)
					{
						PresentationLink pl = de.Value as PresentationLink;

						clone.calculationInfo[de.Key] = pl.CreateCopyForMerging();
					}
				}
				#endregion

				#region Copy Presentation
				if (this.presentationInfo != null)
				{
					clone.presentationInfo = new Hashtable();
					foreach (DictionaryEntry de in this.presentationInfo)
					{
						PresentationLink pl = de.Value as PresentationLink;

						clone.presentationInfo[de.Key] = pl.CreateCopyForMerging();
					}
				}
				#endregion

				#region Copy Definition
				if (this.netDefinisionInfo != null)
				{
					clone.netDefinisionInfo = netDefinisionInfo.CreateCopyForMerging();
				}
				#endregion

				#region Copy Elements
				//since we are going to modify the taxonomy item index....
				//we need to clone the elements
				clone.allElements = new Hashtable();

				foreach (DictionaryEntry de in this.allElements)
				{
					clone.allElements[de.Key] = (de.Value as Element).CreateCopyForMerging();
				}

				//fix thr parent element 
                foreach (Element ele in clone.allElements.Values)
                {



                    ele.ResetTupleRelationship(clone.allElements);

                }

				foreach (DictionaryEntry de in customDataTypesHash)
				{
					clone.customDataTypesHash[de.Key] = de.Value;
				}

				foreach (KeyValuePair<string, RoleRef> kvp in this.roleRefs)
				{
					clone.roleRefs[kvp.Key] = kvp.Value;
				}

				foreach (KeyValuePair<string, RoleType> kvp in this.roleTypes)
				{
					clone.roleTypes[kvp.Key] = kvp.Value;
				}

				if (this.enumTable != null)
				{
					clone.enumTable = new Hashtable();
					foreach (DictionaryEntry de in enumTable)
					{
						clone.enumTable[de.Key] = de.Value;
					}
				}

				if (this.extendedDataMappings != null)
				{
					clone.extendedDataMappings = new Hashtable();
					foreach (DictionaryEntry de in extendedDataMappings)
					{
						clone.extendedDataMappings[de.Key] = de.Value;
					}

				}

				if (this.referenceTable != null)
				{
					clone.referenceTable = new Hashtable();
					foreach (DictionaryEntry de in referenceTable)
					{
						clone.referenceTable[de.Key] = de.Value;
					}

				}


				if (this.DirectDependantTaxonomies != null)
				{
					clone.DirectDependantTaxonomies = new List<string>();
					clone.DirectDependantTaxonomies.AddRange(this.DirectDependantTaxonomies);
				}

				if (this.presentationFile != null)
				{
					List<string> tmp = new List<string>(this.presentationFile);
					clone.presentationFile = tmp.ToArray();
				}
				if (this.calculationFile != null)
				{
					List<string> tmp = new List<string>(this.calculationFile);
					clone.calculationFile = tmp.ToArray();
				}

				if (this.labelFile != null)
				{
					List<string> tmp = new List<string>(this.labelFile);
					clone.labelFile = tmp.ToArray();
				}

				if (this.referenceFile != null)
				{
					List<string> tmp = new List<string>(this.referenceFile);
					clone.referenceFile = tmp.ToArray();
				}

				if (this.definitionFile != null)
				{
					List<string> tmp = new List<string>(this.definitionFile);
					clone.definitionFile = tmp.ToArray();
				}

				clone.dependantTaxonomyFilenames = new List<string>(this.dependantTaxonomyFilenames);

				#endregion

				//shallow copy as the information is not modified....
				#region shallow copy

			

				clone.theDocument = this.theDocument;
				clone.theManager = this.theManager;



				#endregion


				#region things not copied
				//this.numCalcErrors;
				//this.numPresErrors;
				//this.numLabelErrors;
				//this.numDefErrors;
				//this.numRefErrors;
				//this.numWarnings;
				//this.OwnerHandle;
				//this.errorList;
				//numElementErrors;
				//dependantTaxonomies;
				//validationErrors
				//validationWarnings

				#endregion

				//since this is going to be merged into another taxonomy
				//inner taxonomy needs to be set to true
				clone.innerTaxonomy = true;
				clone.isCopiedForMerging = true;

				return clone;

			}
        }


        #region Entry point 

        /// <summary>
        /// set the list of roles for which we need to add the targetrole information
        /// this is to ensure that the entry point created with the new us gaap taxonomy 
        /// is usable with respect to the common dimensions...
        /// </summary>
		/// <param name="baseTaxonomyList"></param>
		/// <param name="selectedURIs"></param>
		/// <param name="targetExts"></param>
        /// <returns></returns>
        public static bool DoesAnyOfTheSelectedRolesNeedTargetRole(Taxonomy[] baseTaxonomyList,
			List<string> selectedURIs,
            out List<Dimension.TargetDimensionInfo> targetExts )
        {

			targetExts = new List<Dimension.TargetDimensionInfo>();
			Dictionary<string, DefinitionLink> rolesByCommonDimension = new Dictionary<string, DefinitionLink>();
			List<string> commonRoles = new List<string>();
			foreach (Taxonomy tax in baseTaxonomyList)
			{

				if (tax.allElements == null || tax.allElements.Count == 0) continue;
				if (tax.netDefinisionInfo == null) continue;
				if (tax.CurrentLanguage == null)
				{
					if (tax.supportedLanguages.Count == 0) continue;

					tax.CurrentLanguage = tax.supportedLanguages[0] as string;
				}

				if (tax.currentLabelRole == null)
				{
					tax.currentLabelRole = PresentationLocator.preferredLabelRole;
				}

				Dictionary<string, DimensionNode> commonDimensionNodes;
				if (!tax.netDefinisionInfo.TryGetDimensionNodesForDisplay(tax.currentLanguage, tax.currentLabelRole,
					tax.presentationInfo,
					true, false, tax.roleTypes, out commonDimensionNodes))
				{

					continue;
				}

				

				foreach (KeyValuePair<string, DimensionNode> kvp in commonDimensionNodes)
				{
					if (!selectedURIs.Contains(kvp.Key)) continue; //this common dimension is not used...
					commonRoles.Add(kvp.Key);

				}

				if (commonRoles.Count == 0) continue; //no common dimension....



				tax.netDefinisionInfo.BuildSelectedCommonDimensionDictionary(commonRoles,
					commonDimensionNodes, ref rolesByCommonDimension);

			}
			foreach (Taxonomy tax in baseTaxonomyList)
			{

				Dictionary<string, DimensionNode> allDimensionNodes;
				if (!tax.netDefinisionInfo.TryGetDimensionNodesForDisplay(tax.currentLanguage, tax.currentLabelRole,
					tax.presentationInfo,
					true, true, tax.roleTypes, out allDimensionNodes))
				{

					continue;
				}


				tax.netDefinisionInfo.DoesAnyOfTheSelectedRolesNeedTargetRole(selectedURIs,
					commonRoles, rolesByCommonDimension, allDimensionNodes, ref targetExts, tax);


			}


			return targetExts.Count > 0;
        }


		private Dictionary<string, List<string> > PresentationToDefinitionRoleMap = null;
		private void BuildPresentationToDefinitionRoleMap()
		{
			if (PresentationToDefinitionRoleMap != null) return;

			PresentationToDefinitionRoleMap = new Dictionary<string, List<string>>();
			if (this.presentationInfo == null || this.netDefinisionInfo == null ||
				netDefinisionInfo.DefinitionLinks == null) return;
			foreach (string presRole in this.PresentationInfo.Keys)
			{
				//found def for the current pres
				if (netDefinisionInfo.DefinitionLinks[presRole] != null) continue;
				foreach (DefinitionLink dl in netDefinisionInfo.DefinitionLinks.Values)
				{
					//ignore def that has presentation
					if (PresentationInfo[dl.Role] != null) continue;

					if (dl.Role.StartsWith(presRole))
					{


						string dDef = this.roleTypes[dl.Role].Definition;
						string pDef = this.roleTypes[presRole].Definition;

						if (dDef != null && pDef != null && 
							dDef.Length > 10 && pDef.Length > 10)
						{
							dDef = dDef.Substring(0, 10);
							pDef = pDef.Substring(0, 10);
						}

						//we need the number to match for both...
						if (dDef != pDef) continue;
						
						//string orig;
						//if (PresentationToDefinitionRoleMap.TryGetValue(presRole, out orig))
						//{
						//    double curPer = presRole.Length * 100 / dl.Role.Length;
						//    double origPer = presRole.Length * 100 / orig.Length;

						//    if (curPer > origPer)
						//    {
						//        PresentationToDefinitionRoleMap[presRole] = dl.Role;
						//    }

						//}
						//else
						//{
						//    PresentationToDefinitionRoleMap[presRole] = dl.Role;

						//}

						List<string> val = null;
						if (!PresentationToDefinitionRoleMap.TryGetValue(presRole, out val))
						{
							val = new List<string>();
							PresentationToDefinitionRoleMap[presRole] = val;

						}

						val.Add(dl.Role);

						
					}
				}

			}
		}


		public void GetDependentDefinitionOnlyRoles(List<string> selectedRoles,
			ref Dictionary<string, List<string>> presToDefinitionRoles)
		{
			BuildPresentationToDefinitionRoleMap();
			if (presToDefinitionRoles == null) presToDefinitionRoles = new Dictionary<string, List<string>>();


			foreach (string key in selectedRoles)
			{
				List<string> val;
				if (PresentationToDefinitionRoleMap.TryGetValue(key, out val))
				{
					List<string> origList = null;
					if (presToDefinitionRoles.TryGetValue(key, out origList))
					{
						foreach (string str in val)
						{
							if (!origList.Contains(str))
							{
								origList.Add(str);
							}
						}
					}
					else
					{
						presToDefinitionRoles[key] = val;
					}
				}

			}

			return;

		}


		public void GetAllDepDefintionRoles(ref Dictionary<string, string> depDefRoles)
		{
			if (depDefRoles == null) depDefRoles = new Dictionary<string, string>();
			BuildPresentationToDefinitionRoleMap();

			foreach (KeyValuePair<string, List<string>> kvp  in PresentationToDefinitionRoleMap)
			{
				foreach (string def in kvp.Value)
				{
					depDefRoles[def] = kvp.Key;
				}
			}

			
		}



        #endregion


        #region Merge Taxonomy
        	private const string WrapperTaxonomyTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<schema elementFormDefault=""qualified"" xmlns:xbrll=""http://www.xbrl.org/2003/linkbase"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns=""http://www.w3.org/2001/XMLSchema"" targetNamespace=""{0}"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" xmlns:{1}=""{0}"">
  {3}
  <import namespace=""http://www.xbrl.org/2003/instance"" schemaLocation=""http://www.xbrl.org/2003/xbrl-instance-2003-12-31.xsd""/>
  <import namespace=""http://www.xbrl.org/2003/linkbase"" schemaLocation=""http://www.xbrl.org/2003/xbrl-linkbase-2003-12-31.xsd"" /> 
  {2}
</schema>";

        private static object synObjectForLoadTaxonomyFromInstanceDocument = new object();

		public bool LoadTaxonomyFromInstanceDocument(string currentFolder, Instance instanceDoc, bool promptUser)
		{
			return LoadTaxonomyFromInstanceDocument(currentFolder, instanceDoc, promptUser, string.Empty);
		}


		/// <summary>
		/// Load and Parser a taxonomy based on the instance document
		/// </summary>
		/// <param name="currentFolder"></param>
		/// <param name="instanceDoc"></param>
		/// <param name="promptUser"></param>
		/// <returns></returns>
		public bool LoadTaxonomyFromInstanceDocument(string currentFolder, Instance instanceDoc,
			bool promptUser, string baseHref)
		{
			if (!string.IsNullOrEmpty(baseHref))
			{

				baseHref = baseHref.TrimEnd('/', '\\');

			}

			lock (synObjectForLoadTaxonomyFromInstanceDocument)
			{
				string oldworkingfolder = System.Environment.CurrentDirectory;
				System.Environment.CurrentDirectory = currentFolder;

				this.PromptUser = promptUser;

				bool deleteFile = false;
				string fileName = string.Empty;

				try
				{
					if (instanceDoc.schemaRefs.Count == 1)
					{
						fileName = ApplyBaseHRef(baseHref, instanceDoc.schemaRefs[0] as string);
					}
					else
					{
						deleteFile = true;
						fileName = this.CreateMergedTaxonomyFileNameFromInstanceDocumentMultiple(instanceDoc, promptUser, baseHref);

						if (string.IsNullOrEmpty(fileName))
						{
							string msg = string.Format("Failed to build Taxonomy from Instance document {0}", instanceDoc.mergeFilename);
							errorList = new ArrayList();
							errorList.Add(msg);
							Common.WriteError(msg, new ArrayList() );
							return false;

						}
					}

					if (this.Load(fileName, promptUser) > 0)
						return false;

					int error = 0;
					return this.Parse(out error);
				}
				catch (Exception exp)
				{
					string msg = string.Format("Failed to build Taxonomy from Instance document {0}", instanceDoc.mergeFilename);
					errorList = new ArrayList();
					errorList.Add(msg);

					Common.WriteError("XBRLParser.Error.Exception", errorList, exp.Message);
					return false;
				}
				finally
				{
					if (deleteFile)
					{
						if (File.Exists(fileName))
							File.Delete(fileName);
					}

					System.Environment.CurrentDirectory = oldworkingfolder;
				}
			}
		}

		private string CreateMergedTaxonomyFileNameFromInstanceDocumentMultiple(Instance instanceDoc, bool promptUser, string baseHref)
		{
			StringBuilder taxonomyNameSpace = new StringBuilder();
			StringBuilder prefix = new StringBuilder();
			StringBuilder importStatement = new StringBuilder();
			string linkbaseStatement = instanceDoc.GetEmbeddedLinkbaseInfo();

			foreach (string loc in instanceDoc.schemaRefs)
			{
				string location = ApplyBaseHRef(baseHref, loc);

				Taxonomy t = new Taxonomy();
				if (t.Load(location, promptUser) > 0)
					return string.Empty;


				t.GetNSPrefix();
				prefix.Append(t.nsPrefix);
				taxonomyNameSpace.Append(t.TargetNamespace);

				string href = ApplyBaseHRef(baseHref, t.schemaFile);

				//CEE 2009-05-28:  Clean up the & within "W R Grace & Company"
				//Remove 1 level of encoding, and then reapply it.
				href = href.Replace( "&amp;", "&" ).Replace( "&", "&amp;" );

				importStatement.Append(string.Format(@"<import namespace=""{0}"" schemaLocation=""{1}"" />",
					t.TargetNamespace, href ) );

				importStatement.Append(Environment.NewLine);
			}


			string fileName = "Temp_" + DateTime.Now.Ticks.ToString() + "_Merged.xsd";
			fileName = Path.Combine(System.Environment.CurrentDirectory, fileName);


			string taxonomyFileInfo = string.Format(Taxonomy.WrapperTaxonomyTemplate,
				taxonomyNameSpace.ToString(), prefix.ToString(), importStatement.ToString(), linkbaseStatement);

			StreamWriter sw = new StreamWriter(fileName, false);
			sw.Write(taxonomyFileInfo);
			sw.Close();

			return fileName;

		}





		private string ApplyBaseHRef(string baseHRef, string path)
		{
			if (string.IsNullOrEmpty(baseHRef))
				return path;

			if (Path.IsPathRooted(path))
				return path;

			if (path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
				return path;

			if (baseHRef.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
			{
				if (path.StartsWith("/"))
					return baseHRef + path;
				else
					return baseHRef + "/" + path;




			}
			else
			{
				return Path.Combine(baseHRef, path);






			}

		}
        #endregion

		#region taxonomy url mappings
		/// <summary>
		/// return the actual url for all the files in the taxonomy....
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,string> BuildTaxonomyUrlMappings()
		{
			Dictionary<string, string> ret = new Dictionary<string, string>();

			foreach (TaxonomyItem item in this.infos)
			{
				if (item.Location.StartsWith("http"))
				{
					string val;
					if (RemoteFiles.IsValidURL(item.Location, out val))
					{
						ret[Path.GetFileName(item.Location).ToLower()] = val;

					}
				}
			}

			foreach (LinkbaseFileInfo lbi in this.linkbaseFileInfos)
			{
				string val = AdjustLinkbaseFileNameInExtensions(lbi.Filename);

				if (val.StartsWith("http"))
				{
					ret[Path.GetFileName(lbi.Filename).ToLower()] = val;
				}
			}


			return ret;
		}
		#endregion


	    


		#region Calculation  logic

		/// <summary>
		/// Determine if a given element is part of the calc hier..
		/// this determines the choices that the user has when the element is edited in  DT/CT
		/// </summary>
		/// <param name="elementId"></param>
		/// <returns></returns>
		public bool DoesElementExistInCalculation(string elementId)
		{
			if (this.calculationInfo != null)
			{
				PresentationLocator loc;
				foreach (PresentationLink clink in this.calculationInfo.Values)
				{
					if (clink.TryGetLocator(elementId, out loc))
					{
						return true;
					}
				}
			}

			return false;

		}
		/// <summary>
		/// Determine if there are any incorrect weights in the taxonomy
		/// </summary>
		/// <param name="summationErrors"></param>
		public void GetSummationItemErrors(out List<string> summationErrors)
		{
			summationErrors = new List<string>();
			if (this.HasCalculation)
			{
				ArrayList calcNodes = this.GetNodesByCalculation();
				foreach (Node n in calcNodes)
				{
					if (n.children != null && n.children.Count > 0)
					{
						foreach (Node topLevelNode in n.children)
						{
							RecursivelyGetSummationItemErrors(topLevelNode, ref summationErrors);
						}

					}
				}

			}
		}


		private void RecursivelyGetSummationItemErrors(Node n, ref List<string> summationErrors)
		{
			if (n.IsProhibited  || n.children == null || n.children.Count == 0 ) return;

			foreach (Node child in n.children)
			{
				if (child.IsProhibited) continue;

				if (n.MyElement.BalType != Element.BalanceType.na && child.MyElement.BalType != Element.BalanceType.na)
				{


					bool hasError = false;
					double weight = 1;

					if (double.TryParse(child.Weight, out weight))
					{
						if (n.MyElement.BalType == child.MyElement.BalType)
						{
							if (weight < 0) hasError = true;
						}
						else
						{
							if (weight > 0) hasError = true;

						}
					}
					if (hasError)
					{

						summationErrors.Add(string.Format("SummationItemBalanceWeightMismatchError ,  the parent '{0}' is a {1} while the child '{2}' is a {3} , the weight of the calculation relationship should be {4}.",
							n.Label, n.BalanceType, child.Label, child.BalanceType, weight >0 ? "negative" : "positive"));

					}
				}
				
				RecursivelyGetSummationItemErrors(child, ref summationErrors);
			}
		}

		/// <summary>
		/// for a given tax .. return nodes that have calc hier
		/// I.e. for each element return a list of nodes where each node is a single calc for that node.
		/// this does not return any element that does not have children or is prohibitted...
		/// </summary>
		/// <returns></returns>
		public void BuildTaxCalculationsByElementId( List<string> excludedReports,
            ref Dictionary<string, List<Node>> taxCalculationsByElementId )
		{
			if (this.HasCalculation)
			{
				ArrayList calcNodes = this.GetNodesByCalculation();

				foreach (Node n in calcNodes)
				{
                    if (excludedReports != null && excludedReports.Contains(n.GetPresentationLink().Role))
                        continue;
					if (n.children != null && n.children.Count > 0)
					{
						foreach (Node topLevelNode in n.children)
						{
							RecursivelyBuildTaxCalculationsByElementId(topLevelNode, ref taxCalculationsByElementId);
						}

					}
				}

			}

		}

		private void RecursivelyBuildTaxCalculationsByElementId(Node n, ref Dictionary<string, List<Node>> taxCalculationsByElementId)
		{
			if (n.IsProhibited) return;


			//we are not interested in the node if it does not have any children....
			if (n.children != null && n.children.Count > 0)
			{
				if (!n.MyElement.IsAbstract)
				{
					//need to add it to the dictionary...
					List<Node> nodeList;
					if (!taxCalculationsByElementId.TryGetValue(n.MyElement.Id, out nodeList))
					{
						nodeList = new List<Node>();
						taxCalculationsByElementId[n.MyElement.Id] = nodeList;
					}
					nodeList.Add(n);
				}

				foreach (Node childNode in n.children)
				{
					if (childNode.IsProhibited) continue;
					RecursivelyBuildTaxCalculationsByElementId(childNode, ref taxCalculationsByElementId);
				}

			}

		}


		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="role"></param>
		/// <param name="rt"></param>
		/// <returns></returns>
		public bool TryGetRoleType(string role, out RoleType rt)
		{
			return this.roleTypes.TryGetValue(role, out rt);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool SupportsTextBlockItemType()
		{
            return this.extendedDataMappings != null && extendedDataMappings[Element.TEXT_BLOCK_ITEM_TYPE] != null;
		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool SupportsDomainItemType()
		{
            return this.extendedDataMappings != null && (extendedDataMappings["us-types:domainItemType"] != null ||
                extendedDataMappings["nonnum:domainItemType"] != null);

		}

        public bool SupportsDomainItemType2009()
        {
            return this.extendedDataMappings != null && extendedDataMappings["us-types:domainItemType"] != null;

        }

        public bool SupportsDomainItemType2011()
        {
            return this.extendedDataMappings != null && extendedDataMappings["nonnum:domainItemType"] != null;

        }


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool SupportsCustomType(string cusType )
		{
			return this.extendedDataMappings != null && extendedDataMappings[cusType] != null;

		}

		
		//used to perform a smart load of the taxonomy
	    /// <summary>
	    /// Do a smart reload of a taxonomy by cloning the base taxonomy instead of 
		/// reloading everything about the base taxonomy....
	    /// </summary>
		//private Taxonomy baseTaxonomy = null; 
		
		///// <summary>
		///// 
		///// </summary>
		///// <param name="filename"></param>
		///// <param name="isEntryPointChange"></param>
		///// <param name="origTaxonomy"></param>
		///// <param name="retTaxonomy"></param>
		///// <returns></returns>
		//public static bool TryLoadParseExtensionTaxonomy(string filename, bool isEntryPointChange, 
		//    Taxonomy origTaxonomy, out Taxonomy retTaxonomy )
		//{
		//    retTaxonomy = new Taxonomy();
		//    retTaxonomy.baseTaxonomy = null;
		//    if (!isEntryPointChange && origTaxonomy != null )
		//    {
		//        if (string.Compare(Path.GetFileName(origTaxonomy.infos[0].Filename), Path.GetFileName(filename), true) == 0)
		//        {
		//            retTaxonomy.baseTaxonomy = origTaxonomy.baseTaxonomy;
		//        }
		//    }

		//    if (retTaxonomy.baseTaxonomy == null)
		//    {
		//        //first load the base taxonomy only...

		//        Taxonomy tmp = new Taxonomy();
		//        tmp.skipAucentExtensions = true;
		//        if( tmp.Load(filename) != 0  )
		//        {
		//            //copy errors
		//            retTaxonomy.errorList = tmp.errorList;
		//            return false;
		//        }
		//        int numErrors;
		//        if (!tmp.Parse(out numErrors))
		//        {
		//            retTaxonomy.errorList = tmp.errorList;
		//            return false;

		//        }

		//        if (!tmp.IsAucentExtension)
		//        {
		//            retTaxonomy = tmp;
		//            return true;
		//        }

		//        retTaxonomy.baseTaxonomy = tmp.CopyTaxonomyForMerging();
		//    }

		//    retTaxonomy.dependantTaxonomies = new ArrayList();
		//    retTaxonomy.dependantTaxonomies.Add(retTaxonomy.baseTaxonomy);
		//    //do a special load by loading just the extensions...

		//    throw new ApplicationException("TODO: Still working on this");

		//}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="linkbaseToXSDMap"></param>
		/// <param name="xsdToLinkbaseMap"></param>
		/// <param name="xsdTaxonomyItemMap"></param>
		public void GetLinkbaseXSDMapInfo(out Dictionary<string, List<string>> linkbaseToXSDMap,
			out Dictionary<string, List<string>> xsdToLinkbaseMap, out Dictionary<string, TaxonomyItem> xsdTaxonomyItemMap )
		{
			linkbaseToXSDMap = new Dictionary<string, List<string>>();
			xsdToLinkbaseMap = new Dictionary<string, List<string>>();
			xsdTaxonomyItemMap = new Dictionary<string, TaxonomyItem>();

			foreach (LinkbaseFileInfo lfi in this.linkbaseFileInfos)
			{

				string linkFileName = Path.GetFileName(lfi.Filename).ToLower();
				string xsdName = Path.GetFileName(lfi.XSDFileName).ToLower();

				List<string> list;
				if (!linkbaseToXSDMap.TryGetValue(linkFileName, out list))
				{
					list = new List<string>();
					linkbaseToXSDMap[linkFileName] = list;
				}
				list.Add(xsdName);
				list = null;
				if (!xsdToLinkbaseMap.TryGetValue(xsdName, out list))
				{
					list = new List<string>();
					xsdToLinkbaseMap[xsdName] = list;
				}
				list.Add(linkFileName);

			}

			foreach (TaxonomyItem ti in this.infos)
			{
				xsdTaxonomyItemMap[Path.GetFileName(ti.Location).ToLower()] = ti;
			}
		}


		public bool IsDuplicateTitle(string role, string newTitle)
		{

			if (this.roleRefs != null && this.roleRefs.Count > 0)
			{
				if (this.roleTypes != null && this.roleTypes.Count > 0)
				{
					foreach (string roleRefUri in this.roleRefs.Keys)
					{
						if (roleRefUri.Equals(role)) continue;
						RoleType rt;
						if (this.roleTypes.TryGetValue(roleRefUri, out rt))
						{
							if (rt.Definition.ToUpper().Equals(newTitle.ToUpper())) return true;
						}
					}
				}
			}

			return false;
		}

		public void UpdateExtendedLinkTitle(string role, string newTitle)
		{

			if (this.presentationInfo != null)
			{
				PresentationLink pl = this.presentationInfo[role] as PresentationLink;
				if (pl != null)
				{
					pl.Title = newTitle;

				}
			}

			if (this.calculationInfo != null)
			{
				PresentationLink pl = this.calculationInfo[role] as PresentationLink;
				if (pl != null)
				{
					pl.Title = newTitle;

				}
			}

			if (this.netDefinisionInfo != null && this.netDefinisionInfo.DefinitionLinks != null)
			{
				DefinitionLink dl = this.netDefinisionInfo.DefinitionLinks[role] as DefinitionLink;
				if (dl != null)
				{
					dl.Title = newTitle;

				}
			}
			RoleType rt;
			if ( this.roleTypes.TryGetValue( role, out rt ))
			{
				rt.Definition = newTitle;
			}
			
		}


        /// <summary>
        /// get all the custom types defined in the taxonomy...
        /// that are not enumerations.....
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSimpleCustomElementTypes()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            if (this.extendedDataMappings != null)
            {

                foreach (DictionaryEntry de in extendedDataMappings)
                {
                    if (enumTable != null && enumTable.ContainsKey(de.Key)) continue;


                    ret[de.Key as string] = de.Value as string;

                }

            }


            return ret;
        }
	}

	/// <summary>
	/// Interface from which we can get cached taxonomies by file name
	/// the file name needs to be name without the path
	/// </summary>
	public interface ITaxonomyCache
	{
		/// <summary>
		/// Get taxonomy from cache using filename without the path.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Taxonomy GetTaxonomyByFileName(string fileName);

	}
}
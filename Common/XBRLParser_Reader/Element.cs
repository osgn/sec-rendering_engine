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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Element
	/// </summary>
	[Serializable]
	public class Element : IComparable, ICloneable
	{
		#region constants

		/// <summary>
		/// The XML element name (i.e., "element") for an XBRL element.
		/// </summary>
		public const string ELEMENT = "element";
		private const string NAME = "name";
		private const string TYPE = "type";
		private const string SUBST_GROUP = "substitutionGroup";
		private const string NILLABLE = "nillable";
		private const string PER_TYPE = "periodType";
		private const string BALANCE = "balance";
		private const string ABSTRACT = "abstract";

		private const string COMPLEX_TYPE = "complexType";
		private const string COMPLEX_CONTENT = "complexContent";
		private const string RESTRICTION = "restriction";
		private const string BASE = "base";
		private const string ANY_TYPE = "anyType";

		private const string SEQUENCE = "sequence";
		private const string ATTR = "attribute";
		private const string REF = "ref";
		private const string MIN_OCCURS = "minOccurs";
		private const string MAX_OCCURS = "maxOccurs";
		private const string USE = "use";
		private const string OPTIONAL = "optional";

		/// <summary>
		/// String resource prefix for element type error lookups
		/// </summary>
		private const string elementTypeErrorPrefix = "XBRLParser.Error.ElementTypeError_";

		internal const string UNBOUNDED = "unbounded";
		private const string TUPLE = "tuple";

		#region base substitution group for elements
		private const string TUPLE_ITEM_TYPE = "xbrli:tuple";
		private const string SUBST_ITEM_TYPE = "xbrli:item";

		/// <summary>
		/// The substitutionGroup attribute value for an XBRL element representing a dimension item.
		/// </summary>
		public const string DIMENSION_ITEM_TYPE = "xbrldt:dimensionItem";

		/// <summary>
		/// The substitutionGroup attribute value for an XBRL element representing a hypercube item.
		/// </summary>
		public const string HYPERCUBE_ITEM_TYPE = "xbrldt:hypercubeItem";
		#endregion

		internal const string MONETARY = "xbrli:monetary";
		internal const string SHARES = "xbrli:shares";
		internal const string PURE = "xbrli:pure";
		internal const string NONZERODECIMAL = "xbrli:nonZeroDecimal";
		internal const string NONZERONONINFINITEFLOAT = "xbrli:nonZeroNonInfiniteFloat";
		internal const string PRECISION_TYPE = "xbrli:precisionType";
		internal const string DECIMALS_TYPE = "xbrli:decimalsType";

		//supported item types
		/// <summary>
		/// The value of the XBRL type attribute for monetary items.
		/// </summary>
		public const string MONETARY_ITEM_TYPE = "xbrli:monetaryItemType";
		internal const string STRING_ITEM_TYPE = "xbrli:stringItemType";
		internal const string TEXT_BLOCK_ITEM_TYPE = "us-types:textBlockItemType";
		internal const string NORMALIZEDSTRING_ITEM_TYPE = "xbrli:normalizedStringItemType";
		internal const string DATETIME_ITEM_TYPE = "xbrli:dateTimeItemType";
		internal const string DATE_ITEM_TYPE = "xbrli:dateItemType";
		internal const string DECIMAL_ITEM_TYPE = "xbrli:decimalItemType";
		internal const string PURE_ITEM_TYPE = "xbrli:pureItemType";

		/// <summary>
		/// The value of the XBRL type attribute for share items.
		/// </summary>
		public const string SHARES_ITEM_TYPE = "xbrli:sharesItemType";
		internal const string POSITIVEINTEGER_ITEM_TYPE = "xbrli:positiveIntegerItemType";

		/// <summary>
		/// The value of the XBRL type attribute for boolean items.
		/// </summary>
		public const string BOOLEAN_ITEM_TYPE = "xbrli:booleanItemType";
		internal const string ANYURI_ITEM_TYPE = "xbrli:anyURIItemType";

		//used for partial comparisons
		private const string NONNEGATIVEMONETARY = "nonnegativemonetary";
		private const string NONNEGATIVEDECIMAL = "nonnegativedecimal";


		//unsupported item types
		public const string FLOAT_ITEM_TYPE = "xbrli:floatItemType";
        public const string DOUBLE_ITEM_TYPE = "xbrli:doubleItemType";
        public const string FRACTION_ITEM_TYPE = "xbrli:fractionItemType";
        public const string INTEGER_ITEM_TYPE = "xbrli:integerItemType";
        public const string NONPOSITIVEINTEGER_ITEM_TYPE = "xbrli:nonPositiveIntegerItemType";
        public const string NEGATIVEINTEGER_ITEM_TYPE = "xbrli:negativeIntegerItemType";
        public const string LONG_ITEM_TYPE = "xbrli:longItemType";
        public const string INT_ITEM_TYPE = "xbrli:intItemType";
        public const string SHORT_ITEM_TYPE = "xbrli:shortItemType";
        public const string BYTE_ITEM_TYPE = "xbrli:byteItemType";
        public const string NONNEGATIVEINTEGER_ITEM_TYPE = "xbrli:nonNegativeIntegerItemType";
        public const string UNSIGNEDLONG_ITEM_TYPE = "xbrli:unsignedLongItemType";
        public const string UNSIGNEDINT_ITEM_TYPE = "xbrli:unsignedIntItemType";
        public const string UNSIGNEDSHORT_ITEM_TYPE = "xbrli:unsignedShortItemType";
        public const string UNSIGNEDBYTE_ITEM_TYPE = "xbrli:unsignedByteItemType";
        public const string HEXBINARY_ITEM_TYPE = "xbrli:hexBinaryItemType";
        public const string BASE64BINARY_ITEM_TYPE = "xbrli:base64BinaryItemType";
        public const string QNAME_ITEM_TYPE = "xbrli:QNameItemType";
        public const string DURATION_ITEM_TYPE = "xbrli:durationItemType";
        public const string TIME_ITEM_TYPE = "xbrli:timeItemType";
        public const string GYEARMONTH_ITEM_TYPE = "xbrli:gYearMonthItemType";
		public const string GYEAR_ITEM_TYPE = "xbrli:gYearItemType";
        public const string GMONTHDAY_ITEM_TYPE = "xbrli:gMonthDayItemType";
        public const string GDAY_ITEM_TYPE = "xbrli:gDayItemType";
        public const string GMONTH_ITEM_TYPE = "xbrli:gMonthItemType";
        public const string TOKEN_ITEM_TYPE = "xbrli:tokenItemType";
        public const string LANGUAGE_ITEM_TYPE = "xbrli:languageItemType";
        public const string NAME_ITEM_TYPE = "xbrli:NameItemType";
        public const string NCNAME_ITEM_TYPE = "xbrli:NCNameItemType";
        public const string DATEUNION = "xbrli:dateUnion";

		private static string[] MISTYPED_TEXT_BLOCKS = new string[0];

		#endregion

		#region Enums
		/// <summary>
		/// Represents the XBRL standard period types.
		/// </summary>
		public enum PeriodType
		{
			/// <summary>
			/// Not applicable.  Represents a period type that is not recognized as standard XBRL.
			/// </summary>
			na,

			/// <summary>
			/// Instant.  Equivalent to "xbrli:instant".
			/// </summary>
			instant,

			/// <summary>
			/// Forever.  Equivalent to "xbrli:forever".
			/// </summary>
			forever,

			/// <summary>
			/// Duration.  Equivalent to "xbrli:duration".
			/// </summary>
			duration
		}

		/// <summary>
		/// Represents the XBRL standard balance types.
		/// </summary>
		public enum BalanceType
		{
			/// <summary>
			/// Not applicable.  Represents a balance type that is not recognized as standard XBRL.
			/// </summary>
			na,

			/// <summary>
			/// Credit.
			/// </summary>
			credit,

			/// <summary>
			/// Debit.
			/// </summary>
			debit
		}

		/// <summary>
		/// Defines the value type of an XBRL fact.
		/// </summary>
		public enum AttributeType {
			/// <summary>
			/// Not applicable.  Represents a value type that is not recognized as standard XBRL.
			/// </summary>
			na,

			/// <summary>
			/// Numeric.
			/// </summary>
			numeric,

			/// <summary>
			/// Not numeric.
			/// </summary>
			nonNumeric 
		}


		/// <summary>
		/// Represents the XBRL standard data types.
		/// </summary>
		public enum DataTypeCode
		{  //the order of this enumeration must be kept in sync with ETWizardElementDetails.cmbDataTypes
			/// <summary>
			/// Monetary.  Equivalent to "xbrli:monetaryItemType".
			/// </summary>
			Monetary,

			/// <summary>
			/// Decimal.  Equivalent to "xbrli:decimalItemType".
			/// </summary>
			Decimal,

			/// <summary>
			/// Pure. Equivalent to "xbrli:pureItemType".
			/// </summary>
			Pure,

			/// <summary>
			/// Shares.  Equivalent to "xbrli:sharesItemType"
			/// </summary>
			Shares,

			/// <summary>
			/// Positive Integer.  Equivalent to "xbrli:positiveIntegerItemType"
			/// </summary>
			PositiveInteger,

			/// <summary>
			/// Integer.  Equivalent to "xbrli:integerItemType".
			/// </summary>
			Integer,

			/// <summary>
			/// String.  Equivalent to "xbrli:stringItemType".
			/// </summary>
			String,

			/// <summary>
			/// Normalized string.  Equivalent to xbrli:normalizedStringItemType".
			/// </summary>
			NormalizedString,

			/// <summary>
			/// DateTime.  Equivalent to "xbrli:dateTimeItemType".
			/// </summary>
			DateTime,

			/// <summary>
			/// Date.  Equivalent to "xbrli:dateItemType".
			/// </summary>
			Date,

			/// <summary>
			/// AnyUri.  Equivalent to "xbrli:anyURIItemType".
			/// </summary>
			anyURI,

			/// <summary>
			/// Not applicable.  Represents a data type that is not recognized as standard XBRL.
			/// </summary>
			na,

			/// <summary>
			/// Not used.
			/// </summary>
			TupleParent,

			/// <summary>
			/// MonthDay.  Equivalent to "xbrli:gMonthDayItemType".
			/// </summary>
			MonthDay,

			/// <summary>
			/// YearMonth.  Equivalent to "xbrli:gYearMonthItemType".
			/// </summary>
			YearMonth,

			/// <summary>
			/// TextBlock.  Equivalent to "us-types:textBlockItemType".
			/// </summary>
			TextBlock
		}

		/// <summary>
		/// Represents the XBRL standard numeric data types.
		/// </summary>
		public enum NumericDataTypes
		{
			/// <summary>
			/// Monetary.  Equivalent to "xbrli:monetaryItemType".
			/// </summary>
			Monetary = DataTypeCode.Monetary,

			/// <summary>
			/// Decimal.  Equivalent to "xbrli:decimalItemType".
			/// </summary>
			Decimal = DataTypeCode.Decimal,

			/// <summary>
			/// Pure. Equivalent to "xbrli:pureItemType".
			/// </summary>
			Pure = DataTypeCode.Pure,

			/// <summary>
			/// Shares.  Equivalent to "xbrli:sharesItemType"
			/// </summary>
			Shares = DataTypeCode.Shares,

			/// <summary>
			/// Positive Integer.  Equivalent to "xbrli:positiveIntegerItemType"
			/// </summary>
			PositiveInteger = DataTypeCode.PositiveInteger,

			/// <summary>
			/// Integer.  Equivalent to "xbrli:integerItemType".
			/// </summary>
			Integer = DataTypeCode.Integer
		};

		#endregion

		#region properties

		// changed from the string "null" to better support extended taxonomies - efj 10/7/04
		private string id = string.Empty;
		private string name = string.Empty;
		internal string type = string.Empty;

		private string substGroup = string.Empty;

		private string prefix = string.Empty;


		/// <summary>
		/// Sometimes the prefix does not match the namespace prefix.
		/// I.e. with msft latest taxonomy ( 12-2007 ) the name space prefix is
		/// msft and the prefix on all the elements is msft07.
		/// This causes some confusion as we need to use the msft07 to link the linkbase files
		/// and use the msft prefix to read instance documents properly.
		/// </summary>
		private string nameWithNSPrefix;


		//the orig substitution group can be another element and we need to walk the tree
		//to determine the actual substitution group....
		private string origsubstGroup = null;
		internal string OrigsubstGroup
		{
			get { return origsubstGroup == null ? substGroup : origsubstGroup; }
			set { origsubstGroup = value; }
		}


		private AttributeType attributeType;

		// added to ensure we can't drag/drop presentationLinks
		private bool isNull = true;


		private bool tuple;
		private bool nillable;
		private PeriodType perType;
		private BalanceType balType;
		private bool abst;


		/// <summary>
		/// Collection of <see cref="Element"/> representing the tuple children of this <see cref="Element"/>.
		/// </summary>
		private SortedList tupleChildren;

        /// <summary>
        /// List of tuple parents that has the current element as its child element
        /// </summary>
        private List<Element> tupleParentList;


        /// <summary>
        /// determines if a element has tuple parents...
        /// </summary>
        public bool HasTupleParents
        {
            get { return tupleParentList == null ? false : tupleParentList.Count > 0; }
        }

		private LabelLocator labelInfo;
		private ReferenceLocator referenceInfo;

		private int taxonomyInfoId = 0;

		// moved to Node
		//private string preferredLabel = null;

		private Hashtable markupValues = null;

		/// <summary>
		/// Contains the enumeration data if it exists. May be null, which denotes no enumeration support
		/// </summary>
		public Enumeration EnumData = null;

		

		/// <summary>
		/// True if this <see cref="Element"/> is an Aucent (Rivet) extended element.
		/// </summary>
		public bool IsAucentExtendedElement = false;

		/// <summary>
		/// Always true.
		/// </summary>
		public bool HasCompleteTupleFamily = true;

		/// <summary>
		/// The collection of <see cref="Choice"/> objects associated with this <see cref="Element"/>.
		/// </summary>
		public Choices ChoiceContainer = null;

		/// <summary>
		/// Indicates that element is to be displayed in a user interface using an icon 
		/// representing choices.  This <see cref="Element"/> represents mutually-exclusive 
		/// choices.
		/// </summary>
		public bool UseChoiceIcon = false;

		private bool isChoice = false;
		internal bool IsChoice
		{
			get
			{
				if (isChoice) return true;
                //TODO: Check This
				//if (parent != null) return parent.IsChoice;

				return false;
			}

			set { isChoice = value; }
		}

        private List<ChildInfo> childrenInfo;

		private string typedDimensionId = null;

		/// <summary>
		/// The id of this element typed dimension.
		/// </summary>
		public string TypedDimensionId
		{
			get { return typedDimensionId; }
			set { typedDimensionId = value; }
		}

        public bool IsDeprecated
        {
            get
            {
                bool deprecated = false;
                if(this.LabelInfo != null && this.LabelInfo.LabelDatas != null)
                {
                    foreach(LabelDefinition labelDef in this.LabelInfo.LabelDatas)
                    {
                        if (labelDef.LabelRole.Equals(LabelDefinition.DEPRECATED_LABEL_ROLE, StringComparison.OrdinalIgnoreCase))
                        {
                            deprecated = true;
                            break;
                        }

                        if (labelDef.LabelRole.Equals(LabelDefinition.DEPRECATEDDATE_LABEL_ROLE, StringComparison.OrdinalIgnoreCase))
                        {
                            deprecated = true;
                            break;
                        }
                    }
                }
                return deprecated;
            }
        }
		#endregion

		#region Accessors

		/// <summary>
		/// The attribute type (e.g. numeric, non-numeric) of this <see cref="Element"/>.
		/// </summary>
		public AttributeType MyAttributeType
		{
			get 
			{
				if (attributeType == AttributeType.na)
				{
					SetAttributeType();
				}
				return attributeType; 
			}
		}

		/// <summary>
		/// The data type (e.g., monetary, shares) of this <see cref="Element"/>.
		/// </summary>
		public DataTypeCode MyDataType
		{
			get
			{
				switch (type)
				{
					case MONETARY_ITEM_TYPE:
						return DataTypeCode.Monetary;

					case STRING_ITEM_TYPE:
						return DataTypeCode.String;

					case NORMALIZEDSTRING_ITEM_TYPE:
						return DataTypeCode.NormalizedString;

					case TEXT_BLOCK_ITEM_TYPE:
						return DataTypeCode.TextBlock;

					case DATETIME_ITEM_TYPE:
						return DataTypeCode.DateTime;

					case DATE_ITEM_TYPE:
						return DataTypeCode.Date;

					case DECIMAL_ITEM_TYPE:
						return DataTypeCode.Decimal;

					case PURE_ITEM_TYPE:
						return DataTypeCode.Pure;

					case SHARES_ITEM_TYPE:
						return DataTypeCode.Shares;

					case POSITIVEINTEGER_ITEM_TYPE:
					case GYEAR_ITEM_TYPE:
					case GDAY_ITEM_TYPE:
					case GMONTH_ITEM_TYPE:
						return DataTypeCode.PositiveInteger;

					case NONNEGATIVEINTEGER_ITEM_TYPE:
						return DataTypeCode.PositiveInteger;

					case INTEGER_ITEM_TYPE:
						return DataTypeCode.Integer;

					//					case BOOLEAN_ITEM_TYPE:
					//						return DataTypeCode.Boolean;

					case ANYURI_ITEM_TYPE:
						return DataTypeCode.anyURI;

					case GYEARMONTH_ITEM_TYPE:
						return DataTypeCode.YearMonth;
					case GMONTHDAY_ITEM_TYPE:

						return DataTypeCode.MonthDay;


					default:
						if (type != null)
						{
							string lower = type.ToLower();
							if (lower.IndexOf(NONNEGATIVEMONETARY) >= 0)
							{
								return DataTypeCode.Monetary;
							}
							else if (lower.IndexOf(NONNEGATIVEDECIMAL) >= 0)
							{
								return DataTypeCode.Decimal;

							}
						}
						return DataTypeCode.na;
				}
			}
		}

		/// <summary>
		/// Returns true if the data type of this element is a numeric type.
		/// </summary>
		public bool IsNumeric
		{
			get
			{
				return CheckIsNumeric(this.MyDataType);
			}
		}

		/// <summary>
		/// Given a data type, this utility method tells if it is a numeric data type.
		/// </summary>
		/// <param name="dataType">The data type for which method is to determine numeric/non-numeric.</param>
		/// <returns>True if item is a recognized <see cref="NumericDataTypes"/> item.</returns>
		public static bool CheckIsNumeric(DataTypeCode dataType)
		{
			ArrayList numericTypes = new ArrayList(Enum.GetValues(typeof(NumericDataTypes)));
			foreach (int i in numericTypes)
			{
				if (i == (int)dataType)
				{
					return true;
				}
			}

			return false;
		}

		private Hashtable MarkupValues
		{
			get { return markupValues; }
			set { markupValues = value; }
		}

		

		/// <summary>
		/// The label locator information related to this <see cref="Element"/>.
		/// </summary>
		public LabelLocator LabelInfo
		{
			get { return labelInfo; }
			set { labelInfo = value; }
		}

		/// <summary>
		/// The reference locator information related to this <see cref="Element"/>.
		/// </summary>
		public ReferenceLocator ReferenceInfo
		{
			get { return referenceInfo; }
			set { referenceInfo = value; }
		}

		/// <summary>
		/// The XBRL ID <see cref="Element"/>.
		/// </summary>
		public string Id
		{
			get { return id == string.Empty ? Name : id; }
			set
			{
				id = value;
				isNull = false;
				ProcessId();
			}
		}

		/// <summary>
		/// The substitution group (e.g., "link:part", "xbrli:tuple") for 
		/// this <see cref="Element"/>.
		/// </summary>
		public string SubstitutionGroup
		{
			get { return substGroup; }
			set { substGroup = value; }
		}

		/// <summary>
		/// The type of this <see cref="Element"/> as represented by XBRL (e.g., "xbrli:stringItemType", 
		/// "xbrli:dateItemType").
		/// </summary>
		public string ElementType
		{
			get { return type; }
			set { type = value; }
		}

		private string origElementType;

		/// <summary>
		/// The original type of this <see cref="Element"/> as represented by XBRL (e.g., "xbrli:stringItemType", 
		/// "xbrli:dateItemType").  The type before it was changed by an extension.
		/// </summary>
		public string OrigElementType
		{
			get { return origElementType != null ? origElementType : type; }
			set { origElementType = value; }
		}

		/// <summary>
		/// The name of this XBRL <see cref="Element"/>.
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				isNull = false;
			}
		}

		/// <summary>
		/// The XBRL period type (e.g. "instant") for this <see cref="Element"/>.
		/// </summary>
		public PeriodType PerType
		{
			get { return perType; }
			set { perType = value; }
		}

		/// <summary>
		/// The XBRL balance type (e.g. "credit") for this <see cref="Element"/>.
		/// </summary>
		public BalanceType BalType
		{
			get { return balType; }
			set { balType = value; }
		}

		/// <summary>
		/// True if this <see cref="Element"/> is nillable.  False if not.
		/// </summary>
		public bool Nillable
		{
			get { return nillable; }
			set { nillable = value; }
		}

		

		
		/// <summary>
		/// True if this <see cref="Element"/> has tuple children.  False otherwise.
		/// </summary>
		public bool HasChildren
		{
			get { return tupleChildren != null && tupleChildren.Count > 0; }
		}

		/// <summary>
		/// The collection of <see cref="Element"/> objects that are the tuple children 
		/// of this <see cref="Element"/>/
		/// </summary>
		public SortedList TupleChildren
		{
			get { return tupleChildren; }
		}

        /// <summary>
        /// The collection of <see cref="Element"/> objects that are the tuple children 
        /// of this <see cref="Element"/>/
        /// </summary>
        public List<Element> TupleParentList
        {
            get { return tupleParentList; }
        }

		/// <summary>
		/// Indicates that this element is usable for marking-up XBRL documents.  True 
		/// if this <see cref="Element"/> is not null, is not a tuple, is not 
		/// abstract and contains no enumeration data.
		/// </summary>
		public bool CanMarkup
		{
			get { return !IsNull && !IsTuple && !IsAbstract && EnumData == null; }
		}

		/// <summary>
		/// True if this element is a tuple.  False otherwise.
		/// </summary>
		public bool IsTuple
		{
			get { return tuple; }
			set { tuple = value; }
		}

		/// <summary>
		/// True if this <see cref="Element"/> is abstract.  False if not.
		/// </summary>
		public bool IsAbstract
		{
			get { return abst; }
			set { abst = value; }
		}

		/// <summary>
		/// True if this <see cref="Element"/> is null.  False if not.
		/// </summary>
		public bool IsNull
		{
			get { return isNull; }
		}

		/// <summary>
		/// The TaxonomyItems index associated with this <see cref="Element"/>.
		/// </summary>
		public int TaxonomyInfoId
		{
			get { return taxonomyInfoId; }
			set	{ taxonomyInfoId = value;

				
			}
		}

		
		/// <summary>
		/// Retrieves and returns the "documentation" label text for this <see cref="Element"/>.
		/// </summary>
		/// <param name="lang">The language code for which the label is to be retrieved.</param>
		/// <returns>The retrieved label.</returns>
		public string GetDefinition(string lang)
		{
			string definition = string.Empty;
			TryGetLabel(lang, "documentation", out definition);

			return definition;
		}

		
		#endregion

		#region constructors

		static Element()
		{
			MISTYPED_TEXT_BLOCKS = new string[]{
				"us-gaap_AvailableForSaleSecuritiesTextBlock",
				"us-gaap_BankingAndThriftDisclosureTextBlock",
				"us-gaap_CompensationRelatedCostsGeneralTextBlock",
				"us-gaap_ComprehensiveIncomeNoteTextBlock",
				"us-gaap_CostMethodInvestmentsDescriptionTextBlock",
				"us-gaap_DisclosureOfCompensationRelatedCostsShareBasedPaymentsTextBlock",
				"us-gaap_EnvironmentalExitCostsByCostTextBlock",
				"us-gaap_FairValueAssetsMeasuredOnNonrecurringBasisUnobservableInputsDescriptionAndDevelopmentTextBlock",
				"us-gaap_FairValueByBalanceSheetGroupingTextBlock",
				"us-gaap_FairValueOptionDisclosuresRelatedToElectionItemsExistingAtEffectiveDateTextBlock",
				"us-gaap_FairValueOptionQualitativeDisclosuresRelatedToElectionTextBlock",
				"us-gaap_FairValueOptionQuantitativeDisclosuresTextBlock",
				"us-gaap_GainLossOnInvestmentsTextBlock",
				"us-gaap_HeldToMaturitySecuritiesTextBlock",
				"us-gaap_IntangibleAssetsDisclosureTextBlock",
				"us-gaap_InvestmentIncomeTextBlock",
				"us-gaap_LongTermPurchaseCommitmentTextBlock",
				"us-gaap_MinimumFinancialRequirementsForFuturesCommissionMerchantsUnderCommodityExchangeActTextBlock",
				"us-gaap_MortgageLoansOnRealEstateByLoanDisclosureTextBlock",
				"us-gaap_MortgageLoansOnRealEstateWriteDownOrReserveDisclosureTextBlock",
				"us-gaap_ProductLiabilityContingenciesTextBlock",
				"us-gaap_PropertyPlantAndEquipmentTextBlock",
				"us-gaap_ScheduleOfAgingOfCapitalizedExploratoryWellCostsTextBlock",
				"us-gaap_ScheduleOfCapitalUnitsTextBlock",
				"us-gaap_ScheduleOfCausesOfIncreaseDecreaseInLiabilityForUnpaidClaimsAndClaimsAdjustmentExpenseTextBlock",
				"us-gaap_ScheduleOfCommonStockByClassTextBlock",
				"us-gaap_ScheduleOfConvertiblePreferredStockByClassTextBlock",
				"us-gaap_ScheduleOfDueToFromBrokerDealersAndClearingOrganizationsTextBlock",
				"us-gaap_ScheduleOfFairValueOffBalanceSheetRisksTextBlock",
				"us-gaap_ScheduleOfHealthCareTrustFundTextBlock",
				"us-gaap_ScheduleOfLifeSettlementContractsInvestmentMethodTextBlock",
				"us-gaap_ScheduleOfMalpracticeInsuranceTextBlock",
				"us-gaap_ScheduleOfOtherAssetsNoncurrentTextBlock",
				"us-gaap_ScheduleOfOtherInvestmentsNotReadilyMarketableTextBlock",
				"us-gaap_ScheduleOfPreferredStockByClassTextBlock",
				"us-gaap_ScheduleOfProjectsWithExploratoryWellCostsCapitalizedForMoreThanOneYearTextBlock",
				"us-gaap_ScheduleOfPropertySubjectToOrAvailableForOperatingLeaseTextBlock",
				"us-gaap_ScheduleOfSecuritiesOwnedNotReadilyMarketableTextBlock",
				"us-gaap_ScheduleOfTreasuryStockByClassTextBlock",
				"us-gaap_TradingSecuritiesAndCertainTradingAssetsTextBlock"
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="Element"/>.
		/// </summary>
		public Element()
		{
			isNull = true;
			abst = true;
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Element"/>.
		/// </summary>
		/// <param name="nameArg">The name to be assigned to the newly created <see cref="Element"/>.</param>
		public Element(string nameArg)
		{
			isNull = false;
			name = nameArg;
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Element"/>.
		/// </summary>
		/// <param name="nameArg">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="isTup">Indicates if this element is a tuple.</param>
		public Element(string nameArg, bool isTup)
		{
			isNull = false;

			name = nameArg;
			tuple = isTup;
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Element"/>.
		/// </summary>
		/// <param name="nameArg">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="isTup">Indicates if this element is a tuple.</param>
		/// <param name="idArg">The id to be assigned to the newly created <see cref="Element"/>.</param>
		public Element(string nameArg, string idArg, bool isTup)
		{
			isNull = false;

			name = nameArg;
			id = idArg;
			tuple = isTup;

			// id is an optional attribute - but just about everything we use 
			// is keyed on id, so set it equal to name
			if (id == null || id.Length == 0)
			{
				id = name;
			}
			else
			{
				ProcessId();
			}
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Element"/>.
		/// </summary>
		/// <param name="nameArg">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="idArg">The id to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="substArg">The substitution group to be assigned to the newly created <see cref="Element"/>.</param>
		public Element(string idArg, string nameArg, string substArg)
			: this(nameArg, idArg, false)
		{
			substGroup = substArg;

			if (substGroup != null && substGroup.Length > 0)
			{
				tuple = substGroup.IndexOf(TUPLE) != -1;
			}
		}

		private Element(Element e, bool UseClone)
		{
			this.id = e.id;
			this.prefix = e.prefix;
			this.nameWithNSPrefix = e.nameWithNSPrefix;
			this.name = e.name;
			this.type = e.type;
			this.origElementType = e.origElementType;
			this.substGroup = e.substGroup;
			this.attributeType = e.attributeType;
			this.isNull = e.isNull;
			this.tuple = e.tuple;
			this.nillable = e.nillable;
			this.perType = e.perType;
			this.balType = e.balType;
			this.abst = e.abst;
			this.typedDimensionId = e.typedDimensionId;

			//this.tupleChildren = e.tupleChildren;

			// need to do a deep copy - the IM taxonomy on .NET 2.0 children are pointing to the wrong parent
			// this didn't seem to make a difference, but I think it's a good thing, so I'm going to leave it in
			if (UseClone)
			{
				if (e.tupleChildren != null)
				{
					tupleChildren = new SortedList();
					IDictionaryEnumerator enumer = e.tupleChildren.GetEnumerator();

					while (enumer.MoveNext())
					{
						this.AddChild((double)enumer.Key, (Element)((Element)enumer.Value).Clone());
					}
				}

                if (e.tupleParentList != null)
                {
                    tupleParentList = new List<Element>();
                    foreach( Element tp in e.tupleParentList )
                    {
						if (tp.id == this.id)
						{
							tupleParentList.Add(this);
						}
						else
						{
							tupleParentList.Add(tp.Clone() as Element);
						}
                    }
                }


			}
			else
			{
				this.tupleChildren = e.tupleChildren;
                this.tupleParentList = e.tupleParentList;
			}
			this.labelInfo = e.labelInfo;
			this.referenceInfo = e.referenceInfo;
			this.taxonomyInfoId = e.taxonomyInfoId;
			this.markupValues = e.markupValues;
			this.EnumData = e.EnumData;
			this.IsAucentExtendedElement = e.IsAucentExtendedElement;
			this.HasCompleteTupleFamily = e.HasCompleteTupleFamily;

			this.isChoice = e.isChoice;
			this.UseChoiceIcon = e.UseChoiceIcon;
			this.ChoiceContainer = e.ChoiceContainer;
            this.childrenInfo = e.childrenInfo;
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Element"/> and initializes it properties 
		/// from a parameter-supplied <see cref="Element"/>.
		/// </summary>
		/// <param name="e">The <see cref="Element"/> from which properties are to be copies to the newly-created 
		/// <see cref="Element"/>.</param>
		public Element(Element e)
			: this(e, true)
		{


		}

		internal void ProcessId()
		{
			// strip off the prefix and keep it
			int prefixUnder = id.IndexOf("_");
			if (prefixUnder != -1)
			{
				prefix = id.Substring(0, prefixUnder + 1);
			}
			else
			{
				prefixUnder = id.IndexOf(":");
				if (prefixUnder != -1)
				{
					prefix = id.Substring(0, prefixUnder + 1);
				}
			}
		}

		#endregion

		#region Adders
		/// <summary>
		/// Retrieves and returns a requested <see cref="Element"/> from this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="order">The key of the tuple child to be returned.</param>
		/// <returns>The requested tuple child item.</returns>
		public Element GetTupleChild(double order)
		{
			if (tupleChildren == null || tupleChildren.Count == 0 || !tupleChildren.ContainsKey(order))
			{
				return null;
			}

			return (Element)tupleChildren[order];
		}

		/// <summary>
		/// Returns the index of a parameter-supplied <see cref="Element"/> within this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="e">The <see cref="Element"/> to be located.</param>
		/// <returns>The requested tuple child item index or -1 if the item was not found.</returns>
		public double GetTupleChildOrder(Element e)
		{
			return GetTupleChildOrder(e.Id);
		}

		/// <summary>
		/// Returns the index of a parameter-supplied element id within this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="id">The id of the <see cref="Element"/> to be located.</param>
		/// <returns>The requested tuple child item index or -1 if the item was not found.</returns>
		public double GetTupleChildOrder(string id)
		{
			if (tupleChildren == null || tupleChildren.Count == 0)
			{
				return -1.0;
			}

			int index = FindChildElement(id);

			if (index > -1)
			{
				return (double)tupleChildren.GetKey(index);
			}
			else
			{
				return -1.0;
			}
		}

		/// <summary>
		/// Populates optional properties for this <see cref="Element"/>.
		/// </summary>
		/// <param name="typeArg">The value to be assigned to this <see cref="Element"/>'s type property.</param>
		/// <param name="nillit">The value to be assigned to this <see cref="Element"/>'s nillable property.</param>
		/// <param name="pt">The value to be assigned to this <see cref="Element"/>'s period type property.</param>
		/// <param name="bt">The value to be assigned to this <see cref="Element"/>'s balance type property.</param>
		/// <param name="abs">The value to be assigned to this <see cref="Element"/>'s abstract property.</param>
		public void AddOptionals(string typeArg, bool nillit, PeriodType pt, BalanceType bt, bool abs)
		{
			this.type = typeArg;
			this.nillable = nillit;
			this.perType = pt;
			this.balType = bt;
			this.abst = abs;
		}


        private void AddParentElement(Element parent)
        {
            if (this.tupleParentList == null) this.tupleParentList = new List<Element>();

            if (!tupleParentList.Contains(parent))
            {
                tupleParentList.Add(parent);
            }
        }

		/// <summary>
		/// Adds a parameter-supplied <see cref="Element"/> to this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="child">The <see cref="Element"/> to be added.</param>
		/// <param name="order">The index ("order") at which <paramref name="child"/> is to 
		/// be added.</param>
		public void AddChild(double order, Element child)
		{
            AddParentElement(this);

			if (tupleChildren == null)
			{
				tupleChildren = new SortedList();
			}
			else
			{
				int elemIndex = FindChildElement(child.Id);
				if (elemIndex != -1)
				{
					tupleChildren.RemoveAt(elemIndex);
				}
			}

			// now add it with the new order
			tupleChildren.Add(order, child);
		}

		/// <summary>
		/// Adds a parameter-supplied <see cref="Element"/> to this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="child">The <see cref="Element"/> to be added.</param>
		public void AddChild(Element child)
		{
            AddParentElement(this);

			if (tupleChildren == null)
			{
				tupleChildren = new SortedList();
			}

			double order = 1.0;

			if (tupleChildren.Count > 1)
			{
				int findIndex = FindChildElement(child.Id);
				if (findIndex != -1)
				{
					order = (double)tupleChildren.GetKey(findIndex);
					tupleChildren.RemoveAt(findIndex);
					tupleChildren.Add(order, child);
					return;
				}
			}

			// not found, add it to the end
			order *= (tupleChildren.Count + 1);

			tupleChildren.Add(order, child);
		}


		/// <summary>
		/// Returns the index of an <see cref="Element"/> within this <see cref="Element"/>'s 
		/// tuple children collection.
		/// </summary>
		/// <param name="id">The id of the <see cref="Element"/> to be located.</param>
		/// <returns>The requested tuple child item index or -1 if the item was not found.</returns>
		public int FindChildElement(string id)
		{
			if (tupleChildren != null)
			{
				for (int i = 0; i < tupleChildren.Count; ++i)
				{
					Element e = (Element)tupleChildren.GetByIndex(i);
					if (e.Id.CompareTo(id) == 0)
					{
						return i;
					}
				}
			}

			return -1;
		}

		#endregion

		
		#region Creators

		/// <summary>
		/// Creates and returns a new <see cref="Element"/>.
		/// </summary>
		/// <param name="dt">The data type to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="name">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="nillable">Indicates if the newly-created <see cref="Element"/> is to be nillable.</param>
		/// <param name="perType">The period type to be assigned to the newly created <see cref="Element"/>.</param>
		/// <returns>The newly-created <see cref="Element"/>.</returns>
		public static Element CreateElement(Element.DataTypeCode dt, string name, bool nillable, PeriodType perType)
		{
			Element e = new Element(name);

			e.abst = false;
			e.substGroup = SUBST_ITEM_TYPE;
			e.nillable = nillable;
			e.PerType = perType;

			switch (dt)
			{
				case DataTypeCode.String:
					e.type = STRING_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
				case DataTypeCode.NormalizedString:
					e.type = NORMALIZEDSTRING_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
				case DataTypeCode.TextBlock:
					e.type = TEXT_BLOCK_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
				case DataTypeCode.DateTime:
					e.type = DATETIME_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
				case DataTypeCode.Date:
					e.type = DATE_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
				case DataTypeCode.Decimal:
					e.type = DECIMAL_ITEM_TYPE;
					e.attributeType = AttributeType.numeric;
					break;
				case DataTypeCode.Pure:
					e.type = PURE_ITEM_TYPE;
					e.attributeType = AttributeType.numeric;
					break;
				case DataTypeCode.Shares:
					e.type = SHARES_ITEM_TYPE;
					e.attributeType = AttributeType.numeric;
					break;
				case DataTypeCode.PositiveInteger:
					e.type = POSITIVEINTEGER_ITEM_TYPE;
					e.attributeType = AttributeType.numeric;
					break;
				case DataTypeCode.Integer:
					e.type = INTEGER_ITEM_TYPE;
					e.attributeType = AttributeType.numeric;
					break;
				/* case DataTypeCode.Boolean:
				e.type = BOOLEAN_ITEM_TYPE;
				e.attributeType = AttributeType.nonNumeric;
				break;*/
				case DataTypeCode.anyURI:
					e.type = ANYURI_ITEM_TYPE;
					e.attributeType = AttributeType.nonNumeric;
					break;
			}

			return e;
		}

		/// <summary>
		/// Creates and returns a new abstract <see cref="Element"/>.
		/// </summary>
		/// <param name="name">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <returns>The newly-created <see cref="Element"/>.</returns>
		public static Element CreateAbstractElement(string name)
		{
			Element e = new Element(name);

			e.abst = true;
			e.substGroup = SUBST_ITEM_TYPE;
			e.type = STRING_ITEM_TYPE;
			e.PerType = PeriodType.duration;
			e.attributeType = AttributeType.nonNumeric;

			return e;
		}

		/// <summary>
		/// Creates and returns a new monetary <see cref="Element"/>.
		/// </summary>
		/// <param name="name">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="nillable">Indicates if the newly-created <see cref="Element"/> is to be nillable.</param>
		/// <param name="perType">The period type to be assigned to the newly created <see cref="Element"/>.</param>
		/// <param name="balType">The balance type to be assigned to the newly created <see cref="Element"/>.</param>
		/// <returns>The newly-created <see cref="Element"/>.</returns>
		public static Element CreateMonetaryElement(string name, bool nillable, BalanceType balType, PeriodType perType)
		{
			Element e = new Element(name);

			e.substGroup = SUBST_ITEM_TYPE;
			e.type = MONETARY_ITEM_TYPE;
			e.nillable = nillable;
			e.PerType = perType;
			e.BalType = balType;
			e.attributeType = AttributeType.numeric;

			return e;
		}

		/// <summary>
		/// Creates and returns a new tuple parent <see cref="Element"/>.
		/// </summary>
		/// <param name="name">The name to be assigned to the newly created <see cref="Element"/>.</param>
		/// <returns>The newly-created <see cref="Element"/>.</returns>
		public static Element CreateTupleParent(string name)
		{
			Element e = new Element(name);

			e.substGroup = TUPLE_ITEM_TYPE;
			e.nillable = true;
			//e.tupleParent = 1;
			e.tuple = true;

			return e;
		}

		/// <summary>
		/// create node for the element 
        /// and if addChildren is set to true then it recursively creates the child node....
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="role"></param>
		/// <param name="addChildren"></param>
		/// <returns></returns>
		public Node CreateNode(string lang, string role, bool addChildren )
		{
			Node n = new Node(this);

		
			n.SetLabel(lang, role);

			if (addChildren)
			{
				if (tupleChildren != null)
				{
					foreach (Element c in tupleChildren.GetValueList())
					{
						n.AddChild(c.CreateNode(lang, role, addChildren));
					}
				}
			}
			return n;
		}

		
		#endregion

	

		#region tuple order
		/// <summary>
		/// Identifies and returns the order at which an <see cref="Element"/> 
		/// may be inserted in the tuple children collection of this <see cref="Element"/> 
		/// in order to be after a given <see cref="Element"/>.
		/// </summary>
		/// <param name="existingElement">The <see cref="Element"/> for which method 
		/// is to determine the "after" index.</param>
		/// <returns>The "after" index.</returns>
		public double GetNewTupleOrderForAfter(Element existingElement)
		{
			if (tupleChildren != null && tupleChildren.Count > 0)
			{
				int elemIndex = FindChildElement(existingElement.Id);

				if (elemIndex == -1)
				{
					throw new ArgumentOutOfRangeException("existingElement", "object not found");
				}
				else if (elemIndex == tupleChildren.Count - 1)
				{
					// get the previous tuple index
					return (double)tupleChildren.GetKey(elemIndex) + 1.0;
				}
				else
				{
					double existing = (double)tupleChildren.GetKey(elemIndex);
					double after = (double)tupleChildren.GetKey(elemIndex + 1);

					return ((after - existing) / 2.0) + existing;
				}
			}

			return 1.0;
		}

		/// <summary>
		/// Identifies and returns the order at which an <see cref="Element"/> 
		/// may be inserted in the tuple children collection of this <see cref="Element"/> 
		/// in order to be before a given <see cref="Element"/>.
		/// </summary>
		/// <param name="existingElement">The <see cref="Element"/> for which method 
		/// is to determine the "before" index.</param>
		/// <returns>The "before" index.</returns>
		public double GetNewTupleOrderForBefore(Element existingElement)
		{
			// find it in the list
			if (tupleChildren != null && tupleChildren.Count > 0)
			{
				int elemIndex = FindChildElement(existingElement.Id);

				if (elemIndex == -1)
				{
					throw new ArgumentOutOfRangeException("existingElement", "object not found");
				}
				else if (elemIndex == 0)
				{
					return GetNewFirstTupleOrder();
				}
				else
				{
					// get the previous tuple index
					double prevChildOrder = (double)tupleChildren.GetKey(elemIndex - 1);
					double existingChildOrder = (double)tupleChildren.GetKey(elemIndex);

					return ((existingChildOrder - prevChildOrder) / 2.0) + prevChildOrder;
				}
			}

			return 1.0;
		}

		/// <summary>
		/// Returns the index (order) at which an <see cref="Element"/> may be inserted 
		/// in the tuple children collection of this <see cref="Element"/> 
		/// in order to be the first <see cref="Element"/>.
		/// </summary>
		/// <returns>The "first" index.</returns>
		public double GetNewFirstTupleOrder()
		{
			if (tupleChildren != null)
			{
				return (double)tupleChildren.GetKey(0) / 2.0;
			}

			return 1.0;
		}

		/// <summary>
		/// Moves a given child <see cref="Element"/> within the tuple children 
		/// collection of this <see cref="Element"/>.
		/// </summary>
		/// <param name="child">The child to be moved.</param>
		/// <param name="newOrder">The new location (order) at which <paramref name="child"/> is 
		/// to be located.</param>
		/// <remarks><paramref name="child"/> is located within the tuple children collection 
		/// by id.</remarks>
		public void MoveChild(Element child, double newOrder)
		{
			int index = FindChildElement(child.Id);

			if (index < 0)
			{
				throw new AucentException("XBRLParser.Error.ChildNotFound", new ArrayList(new string[] { child.Id }));
			}

			tupleChildren.RemoveAt(index);
			tupleChildren.Add(newOrder, child);

		}
		#endregion

		#region Overrides
		/// <summary>
		/// Converts the value of this <see cref="Element"/> to its equivalent string representation.
		/// </summary>
		/// <returns>A <see cref="String"/> containing the representation of this <see cref="Element"/>.</returns>
		/// <remarks>A <see cref="Element"/> is represented by its name and id.</remarks>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Environment.NewLine);
			sb.Append("Element: ").Append(Environment.NewLine);
			sb.Append("\tName: ").Append(name).Append(Environment.NewLine);
			sb.Append("\tId: ").Append(id).Append(Environment.NewLine);

			return sb.ToString();
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="Element"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="Element"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="Element"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="Element"/>.  
		/// Assumed to be a <see cref="Element"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="Element"/>.</returns>
		/// <remarks>
		/// To be equal, the following properties in the two <see cref="Element"/> must be equal:
		/// <bl>
		/// <li>Id.</li>
		/// <li>Name.</li>
		/// <li>Type.</li>
		/// <li>Substitution group.</li>
		/// <li>Tuple.</li>
		/// <li>Nillable.</li>
		/// <li>Period type.</li>
		/// <li>Balance type.</li>
		/// <li>Abstract.</li>
		/// <li>Minimum occurrences.</li>
		/// <li>Maximum occurrences.</li>
		/// <li>Parent is null.</li>
		/// <li>Tuple children.</li>
		/// </bl>
		/// </remarks>
		public override bool Equals(object obj)
		{
			if (!(obj is Element)) return false;
			Element arg = (Element)obj;
			if (!id.Equals(arg.id)) return false;
			if (!name.Equals(arg.name)) return false;
			if (!type.Equals(arg.type)) return false;
			if (!substGroup.Equals(arg.substGroup)) return false;
			if (tuple != arg.tuple) return false;
			if (nillable != arg.nillable) return false;
			if (perType != arg.perType) return false;
			if (balType != arg.balType) return false;
			if (abst != arg.abst) return false;

			if (tupleChildren == null && arg.tupleChildren != null) return false;
			if (tupleChildren != null)
			{
				if (arg.tupleChildren == null) return false;

				if (tupleChildren.Count != arg.tupleChildren.Count) return false;

				for (int i = 0; i < tupleChildren.Count; ++i)
				{
					if (!tupleChildren.GetByIndex(i).Equals(arg.tupleChildren.GetByIndex(i))) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// True if the name and id of this <see cref="Element"/> are equal when compared 
		/// as <see cref="String"/> objects.
		/// </summary>
		/// <returns></returns>
		public bool NameMatchesId()
		{
            return id.CompareTo(GetNameWithNamespacePrefix()) == 0;
		}

		/// <summary>
		/// The name of this element, preceded with this prefix.
		/// </summary>
		/// <returns>The name of this element, preceded with this prefix.</returns>
		private string GetNameWithPrefixPrepended()
		{
			return prefix + name;
		}

		/// <summary>
		/// Sets the "name with namespace prefix" property of this <see cref="Element"/>.
		/// </summary>
		/// <param name="nsPrefix">The namespace prefix to be used to set the property.</param>
		/// <remarks>The "name with namespace prefix" property will only be set if it would 
		/// be different than <see cref="Element"/>.prefix + <see cref="Element"/>.name.</remarks>
		public void CreateNameWithNamespacePrefix(string nsPrefix)
		{
			string altName = nsPrefix + "_" + this.name;

			if (!altName.Equals(this.GetNameWithPrefixPrepended()))
			{
				this.nameWithNSPrefix = altName;
			}
		}

		/// <summary>
		/// Gets the "name with namespace prefix" property of this <see cref="Element"/>.
		/// </summary>
		/// <remarks>
		/// Public to support other Rivet uses.
		/// </remarks>
		public string GetNameWithNamespacePrefix()
		{
			return nameWithNSPrefix == null ? this.GetNameWithPrefixPrepended() : nameWithNSPrefix;
		}

		#endregion

		#region Xml Strings
		/// <summary>
		/// Writes the nodes in a parameter-supplied <see cref="Element"/> to a parameter-supplied
		/// XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="e">The <see cref="Element"/> to be written to <paramref name="xml"/>.</param>
		/// <param name="verbose">If true, attributes such as "nillable", "abstract", and "periodType" 
		/// are written to the output XML <see cref="StringBuilder"/>.</param>
		/// <param name="language">The language code for which <see cref="LabelLocator"/> objects within
		/// this <see cref="Element"/> are to be written.  Labels are filtered by <paramref name="language"/>
		/// only if <paramref name="verbose"/> is true.  Otherwise English is assumed.</param>
		/// <param name="xml">The output XML to which <paramref name="e"/> is to be appended.</param>
		public static void WriteXmlFragment(Element e, bool verbose, string language, StringBuilder xml)
		{
			Element temp = (e == null) ? new Element() : e;
			xml.Append("name=\"").Append(temp.name);

			if (verbose)
			{
				xml.Append("\" nillable=\"").Append(temp.nillable)
					.Append("\" abstract=\"").Append(temp.abst)
					.Append("\" tuple=\"").Append(temp.tuple)
					.Append("\" type=\"").Append(temp.type)
					.Append("\" substitutionGroup=\"").Append(temp.substGroup)
					.Append("\" periodType=\"").Append(temp.perType)
					.Append("\" balanceType=\"").Append(temp.balType)
					.Append("\"");

				if (temp.labelInfo != null)
				{
					temp.labelInfo.WriteXmlFragment(language, xml);
				}
			}
			else
			{
				xml.Append("\"");	// close after the name;

				string text = string.Empty;

				if (temp.TryGetLabel("en", "terseLabel", out text))
				{
					xml.Append(" terseLabel=\"").Append(text).Append("\"");
				}
				else if (temp.TryGetLabel("en", "label", out text))
				{
					xml.Append(" label=\"").Append(text).Append("\"");
				}
			}
		}

		/// <summary>
		/// Writes the nodes in this <see cref="Element"/> and its tuple children to a parameter-supplied
		/// XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="numTabs">The number of tab characters to be appended <paramref name="xml"/>
		/// before each XML node is written.</param>
		/// <param name="language">The language code for which <see cref="LabelLocator"/> objects within
		/// this <see cref="Element"/> are to be written.  Labels are filtered by <paramref name="language"/>
		/// only if <paramref name="verbose"/> is true.  Otherwise English is assumed.</param>
		/// <param name="xml">The output XML to which method is to be appended.</param>
		public void ToXmlString(int numTabs, string language, StringBuilder xml)
		{
			for (int i = 0; i < numTabs; ++i)
			{
				xml.Append("\t");
			}

			if (tupleChildren != null)
			{
				xml.Append("<parentElement ");
				WriteXmlFragment(this, true, language, xml);
				xml.Append(">").Append(Environment.NewLine);

				foreach (Element c in tupleChildren.GetValueList())
				{
					c.ToXmlString(numTabs + 1, language, xml);
				}

				xml.Append("</parentElement>").Append(Environment.NewLine);
			}
			else	// child
			{
				xml.Append("<element ");
				WriteXmlFragment(this, true, language, xml);
				xml.Append("/>").Append(Environment.NewLine);
			}
		}

		internal void WriteXml(string prefix, XmlElement elem, XmlDocument doc)
		{
			//just for unit testing purposes...
			WriteXml(prefix, elem, doc, null); 
		}

		/// <summary>
		/// Updates a parameter-supplied <see cref="XmlElement"/> with key information 
		/// from this <see cref="Element"/>.
		/// </summary>
		/// <param name="prefix">A prefix to be appended to this <see cref="Element"/>'s name property 
		/// to created the id attribute for <paramref name="elem"/>.</param>
		/// <param name="elem">The <see cref="XmlElement"/> to be updated.</param>
		/// <param name="doc">The <see cref="XmlDocument"/> from which new XML elements and attributes
		///  are to be created.</param>
		/// <param name="tax">Taxonomy object for which extension is being created, can be null</param>
		public void WriteXml(string prefix, XmlElement elem, XmlDocument doc, Taxonomy tax )
		{
			// and add the attributes

			Id = string.Format(Taxonomy.ID_FORMAT, prefix, this.Name);
			elem.SetAttribute(Taxonomy.ID, Id);
			elem.SetAttribute(NAME, Name);

			elem.SetAttribute(NILLABLE, Nillable.ToString().ToLower());

			if (!tuple)
			{
				elem.SetAttribute(TYPE, OrigElementType);
			}

			elem.SetAttribute(SUBST_GROUP, SubstitutionGroup);

			if (IsAbstract)
			{
				elem.SetAttribute(ABSTRACT, "true");
			}

			



			if (tuple)
			{
				return;		// we're done here
			}

			bool useDefaultPath = true;
			string xbrliPrefix = Taxonomy.XBRLI_PREFIX;
			if (tax != null)
			{
				if (!tax.HasStandardXBRLIPrefix)
				{
					useDefaultPath = false;
					xbrliPrefix = tax.documentXBRLIPrefix;
				}

			}

            if (useDefaultPath)
            {


                XmlAttribute periodAttr = doc.CreateAttribute(Taxonomy.XBRLI_PREFIX, PER_TYPE, Taxonomy.XBRL_INSTANCE_URL);
                periodAttr.Value = PerType.ToString();
                elem.SetAttributeNode(periodAttr);


                if (ElementType == Element.MONETARY_ITEM_TYPE && BalType != Element.BalanceType.na)
                {
                    XmlAttribute balAttr = doc.CreateAttribute(Taxonomy.XBRLI_PREFIX, BALANCE, Taxonomy.XBRL_INSTANCE_URL);
                    balAttr.Value = BalType.ToString();
                    elem.SetAttributeNode(balAttr);
                }

            }
            else
            {


                XmlAttribute periodAttr = doc.CreateAttribute(xbrliPrefix, PER_TYPE, Taxonomy.XBRL_INSTANCE_URL);
                periodAttr.Value = PerType.ToString();
                elem.SetAttributeNode(periodAttr);


                if (ElementType == tax.ConvertXBRLIToValidPrefix(Element.MONETARY_ITEM_TYPE) && BalType != Element.BalanceType.na)
                {
                    XmlAttribute balAttr = doc.CreateAttribute(xbrliPrefix, BALANCE, Taxonomy.XBRL_INSTANCE_URL);
                    balAttr.Value = BalType.ToString();
                    elem.SetAttributeNode(balAttr);
                }
            }

		}


        /// <summary>
        /// Write the tuple child information for the given parent element
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="prefix"></param>
        /// <param name="tupleParent"></param>
        /// <param name="doc"></param>
        /// <param name="theManager"></param>
        /// <param name="modifyDocument"></param>
        public void WriteTupleChildXml( Element Parent, 
            string prefix, XmlElement tupleParent, XmlDocument doc, XmlNamespaceManager theManager, bool modifyDocument)
		{
			XmlNode sequence = modifyDocument ? tupleParent.SelectSingleNode(".//link2:" + SEQUENCE, theManager) : tupleParent.SelectSingleNode(".//" + SEQUENCE);
			//XmlNode sequence =  tupleParent.SelectSingleNode( SEQUENCE );

			if (sequence == null)
			{
				sequence = CreateTupleParentStructure(doc, tupleParent, modifyDocument);
				// tupleParent is modified--child appended--by previous call.
			}

			// append the child
			XmlElement child = modifyDocument ? doc.CreateElement(ELEMENT, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(ELEMENT);
			sequence.AppendChild(child);

			XmlAttribute refAttr = doc.CreateAttribute(REF);
			child.Attributes.Append(refAttr);
			refAttr.Value = string.Format(DocumentBase.NAME_FORMAT, prefix, Name);

			XmlAttribute minOccurs = doc.CreateAttribute(MIN_OCCURS);
			child.Attributes.Append(minOccurs);
            minOccurs.Value = Parent.GetChildMinOccurance(this.id).ToString();

			XmlAttribute maxOccurs = doc.CreateAttribute(MAX_OCCURS);
			child.Attributes.Append(maxOccurs);
            int maxOccurances = Parent.GetChildMaxOccurance(id);
			// if maxOccurs is the max output unbounded, not the number
			if (maxOccurances == int.MaxValue)
			{
				maxOccurs.Value = StringResourceUtility.GetString("XBRLData.Wizard.Unbounded");
			}
			else
			{
				maxOccurs.Value = maxOccurances.ToString();
			}
		}

		private XmlElement CreateTupleParentStructure(XmlDocument doc, XmlElement parent, bool modifyDocument)
		{
			// create the structure
			XmlElement complexType = modifyDocument ? doc.CreateElement(COMPLEX_TYPE, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(COMPLEX_TYPE);
			parent.AppendChild(complexType);

			XmlElement complexContent = modifyDocument ? doc.CreateElement(COMPLEX_CONTENT, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(COMPLEX_CONTENT);
			complexType.AppendChild(complexContent);

			XmlElement restriction = modifyDocument ? doc.CreateElement(RESTRICTION, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(RESTRICTION);
			complexContent.AppendChild(restriction);

			XmlAttribute baseAttr = doc.CreateAttribute(BASE);
			restriction.Attributes.Append(baseAttr);
			baseAttr.Value = ANY_TYPE;

			XmlElement sequence = modifyDocument ? doc.CreateElement(SEQUENCE, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(SEQUENCE);
			restriction.AppendChild(sequence);

			XmlElement idElem = modifyDocument ? doc.CreateElement(ATTR, Taxonomy.XML_SCHEMA_URL) : doc.CreateElement(ATTR);
			restriction.AppendChild(idElem);

			XmlAttribute idName = doc.CreateAttribute(NAME);
			idElem.Attributes.Append(idName);
			idName.Value = Taxonomy.ID;

			XmlAttribute idType = doc.CreateAttribute(TYPE);
			idElem.Attributes.Append(idType);
			idType.Value = Taxonomy.ID.ToUpper();

			XmlAttribute idUse = doc.CreateAttribute(USE);
			idElem.Attributes.Append(idUse);
			idUse.Value = OPTIONAL;

			return sequence;
		}
		#endregion

		#region Label retrieval
		/// <summary>
		/// Retrieves and returns the default label for this <see cref="Element"/>.
		/// </summary>
		/// <param name="lang">The language code for the requested default label.</param>
		/// <returns>The retrieved label.</returns>
		public string GetDefaultLabel(string lang)
		{
			if (labelInfo == null)
			{
				return "[" + name + "]";
			}

			//BUG 1038 - if default label is empty return the element name in brackets
			string label = labelInfo.GetDefaultLabel(lang);

			if (label.Length == 0)
			{
				label = "[" + this.Name + "]";
			}

			return label;
		}

		/// <summary>
		/// get the label for the label role "label" as defined in any language....
		/// if not then just use the first label defined....
		/// </summary>
		/// <returns></returns>
		public string GetAnyLabel()
		{
			if (labelInfo == null ||  labelInfo.LabelDatas == null || labelInfo.LabelDatas.Count == 0 )
			{
				return "[" + name + "]";
			}

			string ret = null;
			foreach (LabelDefinition ld in labelInfo.LabelDatas)
			{
				if (ld.LabelRole == "label")
				{
					ret = ld.Label;
					break;
				}
			}


			return ret != null ? ret : labelInfo.LabelDatas[0].Label;
		}




		/// <summary>
		/// Returns the text of an XBRL label associated with this element.
		/// </summary>
		/// <param name="lang">The language code for the desired label.</param>
		/// <param name="role">The role for the desired label.</param>
		/// <param name="text">An output parameter.  The retrieved label.</param>
		/// <returns>True if the label could be retrieved.</returns>
		public bool TryGetLabel(string lang, string role, out string text)
		{
			text = string.Empty;

			if ((role == null) || (role.Length == 0))
			{
				return false;
			}

			if (labelInfo == null)
			{
				
				return false;
			}

			//			if(role.CompareTo( TraceUtility.FormatStringResource("XBRLAddin.PreferredLabelRole") ) == 0)
			//			{
			//				if( (PreferredLabel != null) && (PreferredLabel.Length > 0) )
			//				{
			//					return labelInfo.TryGetInfo( lang, PreferredLabel, out text );
			//				}
			//
			//				return false;
			//			}

			return labelInfo.TryGetInfo(lang, role, out text);
		}
		#endregion

		#region Reference retrieval
		/// <summary>
		/// Returns a <see cref="String"/> representation of the references for this <see cref="Element"/>.
		/// </summary>
		/// <param name="references">An output parameter.  The <see cref="String"/> representation.</param>
		/// <returns>Always true.</returns>
		public bool TryGetReferences(out string references)
		{
			references = "";
			StringBuilder sbReference = new StringBuilder();

			ReferenceLocator refLocator;
			TryGetReferenceLocators(out refLocator);

			if (refLocator != null && refLocator.References.Count > 0)
			{
				int refCount = 1;

				foreach (DictionaryEntry de in refLocator.References)
				{
					sbReference.Append(string.Format(TraceUtility.FormatStringResource("XBRLUserControls.ElementBuilder.ReferenceNum"), refCount));

					string key = de.Key as String;
					int index = key.LastIndexOf(" ");
					if (index >= 0)
					{
						key = key.Substring(0, index);
					}
					sbReference.Append(key);
					sbReference.Append(Environment.NewLine);

					ArrayList arRef = de.Value as ArrayList;
					if (arRef[0] is string)
					{

						foreach (string refItem in arRef)
						{
							sbReference.Append(" -").Append(refItem);
						}
					}
					else
					{
						if (arRef[0] is System.Collections.DictionaryEntry)
						{
							foreach (System.Collections.DictionaryEntry refItem in arRef)
							{
								sbReference.Append(" -").Append(refItem.Key).Append(": ").
									Append(refItem.Value).Append(Environment.NewLine);
							}
						}
					}

					sbReference.Append(Environment.NewLine);

					refCount++;
				}
				references = sbReference.ToString();
			}
			return true;
		}

		/// <summary>
		/// Returns this <see cref="Element"/>'s reference information.
		/// </summary>
		/// <param name="references">An output parameter.  The returned reference information.</param>
		/// <returns>True if a non-null <see cref="ReferenceLocator"/> is associated with this <see cref="Element"/> 
		/// (and is returned).</returns>
		/// <remarks>Public for use by Dragon View.</remarks>
		public bool TryGetReferenceLocators(out ReferenceLocator references)
		{
			if (referenceInfo != null)
			{
				references = referenceInfo;
				return true;
			}
			else
			{
				references = null;
				return false;
			}
		}
		#endregion

		#region validation

		/// <summary>
		/// Returns element-specific error for type validation
		/// </summary>
		/// <param name="elementType"></param>
		/// <returns></returns>
		private static string GetTypeErrorForElement(string elementType)
		{
			if ((elementType == null) || (elementType.Length == 0))
			{
				return string.Empty;
			}

			StringBuilder errorKey = new StringBuilder(elementTypeErrorPrefix);
			errorKey.Append(elementType);
			return TraceUtility.FormatStringResource(errorKey.ToString(), elementType);
		}


		private void SetAttributeType()
		{

			switch (type)
			{
				// no attribute types

				case MONETARY:
				case SHARES:
				case PURE:
				case NONZERODECIMAL:
				case NONZERONONINFINITEFLOAT:
				case PRECISION_TYPE:
				case DECIMALS_TYPE:
					attributeType = AttributeType.na;
					break;

				// numeric attribute types

				case FRACTION_ITEM_TYPE:
				case DECIMAL_ITEM_TYPE:
				case MONETARY_ITEM_TYPE:
				case SHARES_ITEM_TYPE:
				case PURE_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case FLOAT_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case DOUBLE_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case LONG_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case INTEGER_ITEM_TYPE:
				case NONPOSITIVEINTEGER_ITEM_TYPE:
				case NEGATIVEINTEGER_ITEM_TYPE:
				case NONNEGATIVEINTEGER_ITEM_TYPE:
				case POSITIVEINTEGER_ITEM_TYPE:
				case INT_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case GDAY_ITEM_TYPE:
				case GYEAR_ITEM_TYPE:
				case GMONTH_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					break;

				case SHORT_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case BYTE_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case UNSIGNEDLONG_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case UNSIGNEDINT_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case UNSIGNEDSHORT_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				case UNSIGNEDBYTE_ITEM_TYPE:
					attributeType = AttributeType.numeric;
					break;

				// nonNumeric attribute type

				case STRING_ITEM_TYPE:
				case HEXBINARY_ITEM_TYPE:		// converts to System.Byte[], but assume string for now
				case BASE64BINARY_ITEM_TYPE:	// converts to System.Byte[], but assume string for now
				case ANYURI_ITEM_TYPE:			// converts to System.Uri, but assume string for now
				//		case QNAME_ITEM_TYPE:			// converts to complex type - System.XmlXmlQualifiedName, but don't address for
				case NORMALIZEDSTRING_ITEM_TYPE:
				case TOKEN_ITEM_TYPE:
				case LANGUAGE_ITEM_TYPE:
				case NAME_ITEM_TYPE:
				case NCNAME_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					break;

				case BOOLEAN_ITEM_TYPE:
					

					attributeType = AttributeType.nonNumeric;
					break;

				case DURATION_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					break;

				case DATETIME_ITEM_TYPE:
				case DATE_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;

					break;

				case TIME_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					break;

				case DATEUNION:
					attributeType = AttributeType.nonNumeric;
					break;


				case GYEARMONTH_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					


					
					break;

				case GMONTHDAY_ITEM_TYPE:
					attributeType = AttributeType.nonNumeric;
					
					
					

					break;

				default:
					attributeType = AttributeType.na;
					break;
			}

		}


		/// <summary>
		/// Parses markup content to corresponding type and gets attribute family for element
		/// (conversion mapping is per MSDN documentation for XmlConvert class)
		/// </summary>
		/// <param name="markupData">The markup content.</param>
		/// <param name="parsedObj">The parsed value of <paramref name="markupData"/>.</param>
		/// <param name="error">An output parameter.  The text of any error that occurred in the 
		/// conversion process.</param>
		/// <returns>False if <paramref name="markupData"/> could not be parsed and converted.</returns>
		private bool TryValidateElementType(string markupData, ref object parsedObj, out string error)
		{
			//attributeType = AttributeType.na;
			parsedObj = null;
			error = null;

			if ((this.type == null) || (type.Length == 0))
				return false;

			string theMarkup = markupData;
			if ( string.IsNullOrEmpty( theMarkup ) )
			{
				if (!nillable)
				{
					error = GetTypeErrorForElement(type);
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				theMarkup = theMarkup.Trim();
				//				if(theMarkup.Length == 0)
				//				{
				//					error = GetTypeErrorForElement(ElementType);
				//					return false;
				//				}
			}

			//Bug 981 - include any user-defined formats set up in Regional Settings
			CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.Name, true);

			try
			{
				switch (type)
				{
					// no attribute types

					case MONETARY:
					case SHARES:
					case PURE:
					case NONZERODECIMAL:
					case NONZERONONINFINITEFLOAT:
					case PRECISION_TYPE:
					case DECIMALS_TYPE:
						attributeType = AttributeType.na;
						parsedObj = decimal.Parse(theMarkup, NumberStyles.Currency, ci);
						break;

					// numeric attribute types

					case FRACTION_ITEM_TYPE:
					case DECIMAL_ITEM_TYPE:
					case MONETARY_ITEM_TYPE:
					case SHARES_ITEM_TYPE:
					case PURE_ITEM_TYPE:
						attributeType = AttributeType.numeric;
                        parsedObj = decimal.Parse(theMarkup, NumberStyles.Any, ci);
						break;

					case FLOAT_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = float.Parse(theMarkup, NumberStyles.Float | NumberStyles.AllowThousands, ci);
						break;

					case DOUBLE_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = double.Parse(theMarkup, NumberStyles.Any, ci);
						break;

					case LONG_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = Int64.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

					case INTEGER_ITEM_TYPE:
					case NONPOSITIVEINTEGER_ITEM_TYPE:
					case NEGATIVEINTEGER_ITEM_TYPE:
					case NONNEGATIVEINTEGER_ITEM_TYPE:
					case POSITIVEINTEGER_ITEM_TYPE:
					case INT_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = Int32.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

                    case GDAY_ITEM_TYPE:
                    case GYEAR_ITEM_TYPE:
                    case GMONTH_ITEM_TYPE:
                        attributeType = AttributeType.nonNumeric;
                        parsedObj = Int32.Parse(theMarkup, NumberStyles.Integer, ci);
                        break;

					case SHORT_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = Int16.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

					case BYTE_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = SByte.Parse(theMarkup, NumberStyles.Integer, ci);
						break;

					case UNSIGNEDLONG_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = UInt64.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

					case UNSIGNEDINT_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = UInt32.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

					case UNSIGNEDSHORT_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = UInt16.Parse(theMarkup, NumberStyles.Integer | NumberStyles.AllowThousands, ci);
						break;

					case UNSIGNEDBYTE_ITEM_TYPE:
						attributeType = AttributeType.numeric;
						parsedObj = Byte.Parse(theMarkup, NumberStyles.Integer, ci);
						break;

					// nonNumeric attribute type

					case STRING_ITEM_TYPE:
					case HEXBINARY_ITEM_TYPE:		// converts to System.Byte[], but assume string for now
					case BASE64BINARY_ITEM_TYPE:	// converts to System.Byte[], but assume string for now
					case ANYURI_ITEM_TYPE:			// converts to System.Uri, but assume string for now
					//		case QNAME_ITEM_TYPE:			// converts to complex type - System.XmlXmlQualifiedName, but don't address for
					case NORMALIZEDSTRING_ITEM_TYPE:
					case TOKEN_ITEM_TYPE:
					case LANGUAGE_ITEM_TYPE:
					case NAME_ITEM_TYPE:
					case NCNAME_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;
						parsedObj = theMarkup;
						break;

					case BOOLEAN_ITEM_TYPE:
						// this should come in as yes or no - need to convert it to true/false
						string yes = StringResourceUtility.GetString("XBRLParser.BooleanEnumeration.Yes");

						string temp = "false";
						if (theMarkup.CompareTo(yes) == 0 || theMarkup.CompareTo("True") == 0)
						{
							temp = "true";
						}

						attributeType = AttributeType.nonNumeric;
						parsedObj = XmlConvert.ToBoolean(temp);
						break;

					case DURATION_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;
						parsedObj = XmlConvert.ToTimeSpan(theMarkup);
						break;

					case DATETIME_ITEM_TYPE:
					case DATE_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;

						//Bug 674 - use DateTime.Parse instead of XmlConvert.ToDateTime
						parsedObj = DateTime.Parse(theMarkup, ci);
						break;

					case TIME_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;
						DateTime tmp = XmlConvert.ToDateTime(theMarkup, XmlDateTimeSerializationMode.Unspecified);
						parsedObj = tmp.TimeOfDay;
						break;

					case DATEUNION:
						attributeType = AttributeType.nonNumeric;
						parsedObj = XmlConvert.ToDateTime(theMarkup, XmlDateTimeSerializationMode.Unspecified);
						break;


					case GYEARMONTH_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;
						try
						{
							parsedObj = XmlConvert.ToDateTime(theMarkup, XmlDateTimeSerializationMode.Unspecified);
							parsedObj = ((DateTime)parsedObj).ToString("yyyy-MM");
						}
						catch (Exception)
						{
						}

						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "yyyy-MM");
								parsedObj = ((DateTime)parsedObj).ToString("yyyy-MM");
							}
							catch (Exception)
							{
							}
						}


						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "yy-MM");
								parsedObj = ((DateTime)parsedObj).ToString("yyyy-MM");
							}
							catch (Exception)
							{
							}
						}

						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "yy/MM");
								parsedObj = ((DateTime)parsedObj).ToString("yyyy-MM");
							}
							catch (Exception)
							{
							}
						}

						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "yyyy/MM");
								parsedObj = ((DateTime)parsedObj).ToString("yyyy-MM");
							}
							catch (Exception)
							{
							}
						}

						if (parsedObj == null)
						{
							error = GetTypeErrorForElement(type);
							return false;
						}
						break;

					case GMONTHDAY_ITEM_TYPE:
						attributeType = AttributeType.nonNumeric;
						try
						{
							parsedObj = XmlConvert.ToDateTime(theMarkup, XmlDateTimeSerializationMode.Unspecified);
							parsedObj = ((DateTime)parsedObj).ToString("--MM-dd");
						}
						catch (Exception)
						{
						}

                        if (parsedObj == null)
                        {
                            try
                            {
                                parsedObj = XmlConvert.ToDateTime(theMarkup, "--MM-dd");
                                parsedObj = ((DateTime)parsedObj).ToString("--MM-dd");
                            }
                            catch (Exception)
                            {
                            }
                        }


						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "MM-dd");
								parsedObj = ((DateTime)parsedObj).ToString("--MM-dd");
							}
							catch (Exception)
							{
							}
						}


						if (parsedObj == null)
						{
							try
							{
								parsedObj = XmlConvert.ToDateTime(theMarkup, "MM/dd");
								parsedObj = ((DateTime)parsedObj).ToString("--MM-dd");
							}
							catch (Exception)
							{
							}
						}

						if (parsedObj == null)
						{
							error = GetTypeErrorForElement(type);
							return false;

						}
						break;

					default:
						attributeType = AttributeType.na;
						parsedObj = theMarkup;
						break;
				}
			}
			catch (Exception ex)
			{
				//removes the warning
				Exception e = ex;

				StringBuilder errorMessage = new StringBuilder(GetTypeErrorForElement(type));
#if DEBUG
				errorMessage.Append(" Exception: ").Append(ex.Message);
#endif
				error = errorMessage.ToString();
				return false;
			}
			finally
			{
			}

			return true;
		}

        /// <summary>
        /// Determines whether an element can have unit ref associated with it 
        /// </summary>
        /// <returns></returns>
        public bool CanUseUnitRef()
        {
            switch (this.type)
            {
                
                case MONETARY_ITEM_TYPE:
                case DECIMAL_ITEM_TYPE:
                case FLOAT_ITEM_TYPE:
                case DOUBLE_ITEM_TYPE:
                case INT_ITEM_TYPE:
                case NONPOSITIVEINTEGER_ITEM_TYPE:
                case NEGATIVEINTEGER_ITEM_TYPE:
                case LONG_ITEM_TYPE:
                case INTEGER_ITEM_TYPE:
                case SHORT_ITEM_TYPE:
                case BYTE_ITEM_TYPE:
                case NONNEGATIVEINTEGER_ITEM_TYPE:
                case UNSIGNEDBYTE_ITEM_TYPE:
                case UNSIGNEDINT_ITEM_TYPE:
                case UNSIGNEDLONG_ITEM_TYPE:
                case UNSIGNEDSHORT_ITEM_TYPE:
                case POSITIVEINTEGER_ITEM_TYPE:
                case SHARES_ITEM_TYPE:
                case PURE_ITEM_TYPE:
                case FRACTION_ITEM_TYPE:

                case MONETARY:
                case SHARES:
                case PURE:
                case NONZERODECIMAL:
                case NONZERONONINFINITEFLOAT:
                case PRECISION_TYPE:
                case DECIMALS_TYPE:



                    return true;
            }


            return false;
        }

		/// <summary>
		/// Determines whether or not an element is monetary type.
		/// </summary>
		/// <returns>True it element is monetary type.</returns>
		public bool IsMonetary()
		{
			switch ( this.type )
			{
				case MONETARY_ITEM_TYPE:
				case MONETARY:
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determins whether the element (this) is marked at a textblock, or if it was supposed to be.
		/// </summary>
		/// <returns>True if the element is a valid TextBlock</returns>
		public bool IsTextBlock()
		{
			if( this.MyDataType == DataTypeCode.TextBlock )
			{
				return true;
			}

			if( this.MyDataType == DataTypeCode.String )
			{
				if( this.OrigElementType == TEXT_BLOCK_ITEM_TYPE )
				{
					return true;
				}
				else if( this.Id.StartsWith( "us-gaap_" ) )
				{
					if( Array.BinarySearch( MISTYPED_TEXT_BLOCKS, this.Id ) >= 0 )
					{
						return true;
					}
				}
			}

			return false;
		}

		//private bool TryParseDatePartMarkup( string markup, out string parsedString )
		//{

		//}

		/// <summary>
		/// Validates element vs. markup content
		/// </summary>
		/// <param name="markupData">The markup content to be validated.</param>
		/// <param name="error">An output parameter.  The text of any error that occurred in the 
		/// conversion process.</param>
		/// <param name="parsedObj">The parsed value of <paramref name="markupData"/>.</param>
		/// <returns>False if <paramref name="markupData"/> could not be parsed and converted.</returns>
		public bool TryValidateElement(string markupData, ref object parsedObj, out string error)
		{
			if (markupData != null)
			{
				markupData = markupData.Trim();
			}
			error = null;
			if (nillable && string.IsNullOrEmpty(markupData))
			{
				parsedObj = markupData;

				
				return true;

			}


			if (!TryValidateElementType(markupData, ref parsedObj, out error))
			{
				return false;
			}
			if (ElementType == Element.GDAY_ITEM_TYPE)
			{
				int data = (int)parsedObj;

				if (data < 1 || data > 31)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.GMONTH_ITEM_TYPE)
			{
				int data = (int)parsedObj;

				if (data < 1 || data > 12)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.GYEAR_ITEM_TYPE)
			{
				int data = (int)parsedObj;

				if (data < 0)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if ((ElementType == Element.NONZERODECIMAL) || (ElementType == Element.NONZERONONINFINITEFLOAT))
			{
				if ((decimal)parsedObj == 0M)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if ((ElementType == Element.PRECISION_TYPE))
			{
				if ((decimal)parsedObj < 0M)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if ((ElementType == Element.NONPOSITIVEINTEGER_ITEM_TYPE))
			{
				if ((int)parsedObj > 0M)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.NEGATIVEINTEGER_ITEM_TYPE)
			{
				if ((int)parsedObj >= 0M)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if ((ElementType == Element.POSITIVEINTEGER_ITEM_TYPE))
			{
				if ((int)parsedObj <= 0M)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.NONNEGATIVEINTEGER_ITEM_TYPE)
			{
				if ((int)parsedObj < 0)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.NORMALIZEDSTRING_ITEM_TYPE)
			{
				//carriage return not allowed
				if (markupData.IndexOf('\r') > 0)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}

				//line feed not allowed
				if (markupData.IndexOf('\n') > 0)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}

				//tab not allowed
				if (markupData.IndexOf('\t') > 0)
				{
					error = GetTypeErrorForElement(ElementType);
					return false;
				}
			}

			if (ElementType == Element.ANYURI_ITEM_TYPE)
			{
				//uri must begin with http or www
				if (markupData.Length < 4 ||
					((markupData.Substring(0, 4) != "http") &&
					(markupData.Substring(0, 3) != "www")))
				{
					error = TraceUtility.FormatStringResource("XBRLParser.Error.InvalidURI");
					return false;
				}
			}

			return true;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="Element"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="Element"/>
		/// is to be compared.  Assumed to be a <see cref="Element"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="Element"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> 
		/// for the names of the two <see cref="Element"/>.</remarks>
		public int CompareTo(object obj)
		{
			return string.Compare(name, (obj as Element).name);
		}

		#endregion

		#region update
		/// <summary>
		/// Replaces key properties of this <see cref="Element"/> with equivalent properties 
		/// in a parameter-supplied <see cref="Element"/>.
		/// </summary>
		/// <param name="newElemInfo">The <see cref="Element"/> from which property values are
		///  to be copied.</param>
		///  <remarks>
		/// The following properties are copied:
		/// <bl>
		/// <li>Nillable.</li>
		/// <li>Period type.</li>
		/// <li>Balance type.</li>
		/// <li>Abstract.</li>
		/// <li>Type.</li>
		/// <li>Minimum occurrences.</li>
		/// <li>Maximum occurrences.</li>
		/// </bl>
		///  </remarks>
		public void Update(Element newElemInfo)
		{
			this.nillable = newElemInfo.nillable;
			this.perType = newElemInfo.perType;
			this.balType = newElemInfo.balType;
			this.abst = newElemInfo.abst;
			this.type = newElemInfo.type;
			this.origElementType = newElemInfo.origElementType;
		}


		/// <summary>
		/// Adds the <see cref="LabelDefinition"/> objects within a supplied collection to this 
		///  <see cref="Element"/>'s collection of labels.
		/// </summary>
		/// <param name="labels">An <see cref="Array"/> of <see cref="LabelDefinition"/>.</param>
		public void UpdateLabels(LabelDefinition[] labels)
		{
			if (labelInfo == null)
			{
				labelInfo = new LabelLocator();
			}

			labelInfo.Update(labels);
		}

        /// <summary>
        /// Adding new references to the element
        /// </summary>
        /// <param name="references"></param>
        public void UpdateReferences(ReferenceExtenderDefinition[] references)
        {
            if (this.referenceInfo == null)
            {
                referenceInfo = new ReferenceLocator();

            }
            referenceInfo.UpdateNodeReferences(references);
        }
		#endregion

		#region ICloneable Members

		/// <summary>
		/// Creates and returns a new <see cref="Element"/> copied from this <see cref="Element"/>.
		/// </summary>
		/// <returns>The new <see cref="Element"/>.</returns>
		/// <remarks>The newly created <see cref="Element"/> is added to the cloned element collection 
		/// for this <see cref="Element"/>.</remarks>
		public object Clone()
		{
			return new Element(this);
		}

		#endregion

	

	

		#region type and subst group validations
		/// <summary>
		/// Indicates if the substitution group for this <see cref="Element"/> 
		/// is a "base" XBRL substitution group--item, tuple, dimension, or hypercube.
		/// </summary>
		/// <returns>True if the substitution group for this <see cref="Element"/> 
		/// is a "base" XBRL substitution group--item, tuple, dimension, or hypercube.</returns>
		public bool IsBaseSubstGroup()
		{
			if (this.substGroup == SUBST_ITEM_TYPE ||
				this.substGroup == TUPLE_ITEM_TYPE ||
				this.substGroup == DIMENSION_ITEM_TYPE ||
				this.substGroup == HYPERCUBE_ITEM_TYPE)
			{
				return true;
			}

			return false;
		}


		/// <summary>
		/// Indicates this <see cref="Element"/> is a dimension or hypercube item.
		/// </summary>
		/// <returns>True if this <see cref="Element"/> is a dimension or hypercube item.</returns>
		/// <remarks>A dimension or hypercube item must be an abstract item and have a substitution
		/// group indicating dimension or hypercube.</remarks>
		public bool IsDimensionOrHyperCubeItem()
		{
			if (abst)
			{
				if (this.substGroup == DIMENSION_ITEM_TYPE ||
					this.substGroup == HYPERCUBE_ITEM_TYPE)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Indicates this <see cref="Element"/> is a hypercube item.
		/// </summary>
		/// <returns>True if this <see cref="Element"/> is a hypercube item.</returns>
		/// <remarks>A hypercube item must be an abstract item and have a substitution
		/// group indicating hypercube.</remarks>
		public bool IsHyperCubeItem()
		{
			if (abst && this.substGroup == HYPERCUBE_ITEM_TYPE)
			{
				return true;

			}

			return false;
		}

		/// <summary>
		/// Indicates this <see cref="Element"/> is a dimension item.
		/// </summary>
		/// <returns>True if this <see cref="Element"/> is a dimension item.</returns>
		/// <remarks>A dimension item must be an abstract item and have a substitution
		/// group indicating dimension.</remarks>
		public bool IsDimensionItem()
		{
			if (abst && this.substGroup == DIMENSION_ITEM_TYPE)
			{
				return true;

			}

			return false;
		}

		/// <summary>
		/// Determines if the data type of this <see cref="Element"/> is a standard 
		/// XBRL data type.
		/// </summary>
		/// <returns>True if the data type is a standard XBRL data type.  False otherwise.</returns>
		public bool IsBaseDataType()
		{

			return this.MyDataType == DataTypeCode.na ? false : true;
		}


		/// <summary>
		/// Updates the substitution group for this <see cref="Element"/>.
		/// </summary>
		/// <param name="allElements">A <see cref="Hashtable"/> of <see cref="Element"/> 
		/// objects in which method must find the substitution group before performing updates.</param>
		/// <param name="preventRecursion">A <see cref="Hashtable"/> of previously visited 
		/// element Ids.</param>
		/// <returns>True if the substitution group could be updated.  False otherwise.</returns>
		public bool SetSubstitutionGroup(Hashtable allElements, Hashtable preventRecursion)
		{
			string otherElementId = this.substGroup.Replace(":", "_");

			if (preventRecursion[otherElementId] != null) return false;

			Element el = allElements[otherElementId] as Element;
			preventRecursion[otherElementId] = 0;

			if (el != null)
			{
				if (el.IsBaseSubstGroup())
				{
					this.origsubstGroup = substGroup;
					this.substGroup = el.substGroup;

					return true;
				}
				else
				{
					if (el.SetSubstitutionGroup(allElements, preventRecursion))
					{
						this.origsubstGroup = substGroup;
						this.substGroup = el.substGroup;

						return true;
					}
				}
			}

			return false;
		}




		#endregion

		internal Element CreateCopyForMerging()
		{
			Element ret = new Element(this, false);

			ret.labelInfo = null;
			ret.referenceInfo = null;

			
		


			return ret;
		}

        internal void ResetTupleRelationship(Hashtable allElements )
		{
            if (this.tupleParentList != null)
            {
                List<Element> newList = new List<Element>();
                foreach (Element oldEle in this.tupleParentList)
                {
                    Element newEle = allElements[oldEle.id] as Element;
                    if (newEle != null)
                    {
                        newList.Add(newEle);
                    }
                    else
                    {
                        throw new ApplicationException("Failed to find the  element");
                    }
                }

                this.tupleParentList = newList;
            }

			if (this.tupleChildren != null)
			{
				IDictionaryEnumerator enumer = tupleChildren.GetEnumerator();
				tupleChildren = new SortedList();
				while (enumer.MoveNext())
				{
					Element origChild = (Element)enumer.Value;

					Element childFromList = allElements[origChild.id] as Element;
					

                    if (childFromList != null)
					{
                        this.AddChild((double)enumer.Key, childFromList);
					}
					else
					{
						throw new ApplicationException("Failed to find the  element");
					}
				}
			}


        }


        #region tuple children info
        /// <summary>
        /// add a new child element
        /// </summary>
        /// <param name="id"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void AddChildInfo(string id, int min, int max)
        {
            if (this.childrenInfo == null) childrenInfo = new List<ChildInfo>();

            childrenInfo.Add(new ChildInfo(id, min, max));
        }

        /// <summary>
        /// get min occurance of child element
        /// </summary>
        /// <param name="childid"></param>
        /// <returns></returns>
        public int GetChildMinOccurance(string childid)
        {
            if (this.childrenInfo == null) return 0;
            for (int i = 0; i < childrenInfo.Count; i++)
            {
                if (childrenInfo[i].Id.Equals(childid))
                {
                    return childrenInfo[i].MinOccurances;
                }
            }

            return 0;
        }

        /// <summary>
        /// get max occurance of child element
        /// </summary>
        /// <param name="childid"></param>
        /// <returns></returns>
        public int GetChildMaxOccurance(string childid)
        {
            if (this.childrenInfo == null) return int.MaxValue;

            for (int i = 0; i < childrenInfo.Count; i++)
            {
                if (childrenInfo[i].Id.Equals(childid))
                {
                    return childrenInfo[i].MaxOccurances;
                }
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// stores the min max occurance info of every child element of this tuple
        /// </summary>
		[Serializable]
        public class ChildInfo
        {
            /// <summary>
            /// id of child element
            /// </summary>
            public string Id;
            /// <summary>
            /// min occurance of child element
            /// </summary>
            public int MinOccurances;
            /// <summary>
            /// max occurance of child element
            /// </summary>
            public int MaxOccurances;

            /// <summary>
            /// add child info
            /// </summary>
            /// <param name="childId"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            public ChildInfo(string childId, int min, int max)
            {
                this.Id = childId;
                this.MinOccurances = min;
                this.MaxOccurances = max;

            }
        }
	}
}
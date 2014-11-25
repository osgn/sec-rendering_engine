using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
	public class ValidationErrorInfo : IComparable<ValidationErrorInfo>
	{
        public enum ValidationCategoryType
        {
            DataTypeError,
            InvalidDataError,
            DimensionError,
            ConsistencyWarning,
            SECValidationError,
            CalculationError, 
            Warning_MonetaryInstantShouldBePositive,
            Warning_NegativeShouldBePositive,
            Warning_PositiveShouldBeNegative,
            SECValidationWarning,
            SECValidationWarning_LC3,
            SECValidationWarning_LC3_Members,
            SharesAmountWarning,
            CalculationWeightWarning,
            PureOnPerShareError,
            UnitTypeError,
            ExtendedElememntMonetaryNA,
            Other
        }

        public enum SequenceEnum
        {
            #region Data Type Errors

            REPORTING_ELEMENT_DOES_NOT_EXIST_ERROR = -1000,
            INVALID_ENUMERATION_ERROR = -999,
            DUPLICATE_MARKUP_ERROR = -998,
            VALUE_PRECISION_SCALE_MISMATCH_WARNING = -997,
            EXCEPTION_ERROR = -996,
            INTEGER_WITHOUT_UNIT_ERROR = -995,
            MISSING_PRECISION_ERROR = -994,
            MISSING_UNIT_ERROR = -993,
            MISSING_REPORTING_PERIOD_ERROR = -992,
            INVALID_DATA_ERROR = -991,
            PUREON_PERSHARE_ERROR = -992,
            EXTENDED_ELEMENT_MONETARY_NA = -993,

            #endregion

            #region SEC Data Errors

            SCENARIO_NOT_ALLOWED_ERROR = -800,
            SEGMENT_NOT_ALLOWED_ERROR = -799,
            EntityCentralIndexKeyERROR = -798,
            SEC_AMENDMENT_DESC_ERROR = -797,
            INSTANT_DURATION_CONFLICT = -796,
            EntityCommonStockSharesOutstanding_ERROR = -795,
            MARKUP_WITH_WHITESPACE_ERROR = -794,
            NEGATIVE_VALUE_ERROR = -793,
            POSITIVE_VALUE_ERROR = -792,

            #endregion

            #region NON SEC ERRORS

            DIMENSION_ERROR = -500,

            #endregion

            #region SEC Taxonomy Errors

            EXTENDED_ELEMENT_MISSING_IN_PRESENTATION_ERROR = 5,
            REPORT_NAME_URI_ERROR = 10,
            REPORT_NAME_ERROR = 15,
            CALCULATION_PERIOD_ERROR = 15,
            MISSING_IN_PRESENTATION_ERROR = 20,
            TEXT_BLOCK_PRESENTATION_RELATION_ERROR = 21,
            
            
            
            DUPLICATION_IN_PRESENTATION_ERROR = 22,

            USGAAP_STRUCTURE_ERROR_MULTIPLEAXISWITHSAMEDEFAULT = 25,
            USGAAP_STRUCTURE_ERROR_UNNECESSARYAXIS = 26,
            USGAAP_STRUCTURE_ERROR_TABLE = 27,
            USGAAP_STRUCTURE_ERROR_AXIS = 28,
            USGAAP_STRUCTURE_ERROR_AXIS_DOMAIN_NOT_DEFAULT = 29,
            USGAAP_STRUCTURE_ERROR_AXIS_MULTIPLE_DEFAULTS = 31,
            USGAAP_STRUCTURE_ERROR_DIMENSION = 32,
            USGAAP_STRUCTURE_ERROR_LINE_ITEM = 33,
            USGAAP_STRUCTURE_ERROR_LINE_ITEM2 = 34,
            USGAAP_STRUCTURE_ERROR_ROOT_NODE = 35,
            USGAAP_STRUCTURE_ERROR_ROOT_NODE_ABSTRACT = 36,
            USGAAP_STRUCTURE_LINEITEM_MISMATCH = 37,
            USGAAP_STRUCTURE_MULTIPLE_LINEITEM_MISMATCH = 38,
            USGAAP_STRUCTURE_ERROR_ELEMENT_ERROR = 39,

            CALCULATION_RELATIONSHIP_MISSING_IN_PRESENTATION_ERROR = 46,
            DUPLICATE_CALCULATION_ARC_ERROR = 47,
            DUPLICATE_IN_CALCULATION = 48,
            CALCULATION_PARENT_CHILD_PERIOD_TYPE_MISMATCH = 49,
            RECURSION_IN_CALCULATION = 50,

            DIMENSION_VALIDATION = 55,
            DOMAIN_DUPLICATION_ERROR = 56,
            MEMBER_DUPLICATION_ERROR = 57,
        

            EXTENDED_ELEMENT_NAME_ERROR = 60,
            EXTENDED_DIMENSION_AXIS_NAME_ERROR1 = 61,
            EXTENDED_DIMENSION_AXIS_NAME_ERROR2 = 62,
            EXTENDED_TABLE_NAME_ERROR1 = 63,
            EXTENDED_TABLE_NAME_ERROR2 = 64,
            EXTENDED_LINE_ITEM_NAME_ERROR1 = 65,
            EXTENDED_LINE_ITEM_NAME_ERROR2 = 66,
            EXTENDED_DIMENSION_MEMBER_NAME_ERROR1 = 67,
            EXTENDED_DIMENSION_MEMBER_NAME_ERROR2 = 68,
            EXTENDED_DIMENSION_MEMBER_NAME_ERROR3 = 69,
            EXTENDED_DIMENSION_MEMBER_TYPE_ERROR = 70,

            MULTIPLE_ELEMENT_SAME_STD_LABEL_ERROR = 200,
            EMPTY_LABEL_ERROR = 201,
            INVALID_CHARACTER_IN_LABEL_ERROR = 202,
            INVALID_ASCII_SEQUENCE_IN_LABEL_ERROR = 203,
            MAX_LABEL_SIZE_ERROR = 204,
            DIMENSION_AS_ELEMENT_ERROR = 205,
            DECPRECATED_ELEMENT_USED_ERROR = 206,

            CALCULATION_TOTAL_NOT_PRESENTATION_TOTAL = 300,
            PRESENTATION_TOTAL_NOT_CALCULATION_TOTAL = 301,

            #endregion

            #region Calculation 

            CALCULATION_ERROR = 400,
            CALCULATION_ERROR_EXTENDED = 401,

            #endregion

            #region warnings

            POSITIVE_VALUE_WARNING = 479,
            NEGATIVE_VALUE_WARNING = 480,
            CONSISTENCY_WARNING_ALMOST_NEVER_RECIEVE_DIGIT_AS_SHOWN = 487,
            SHARE_VALUE_MEETEXCEED_ONE_BILLION = 488,
            EntityCommonStockSharesOutstanding_MEMBER_WARNING = 489,
            CONSISTENCY_WARNING_PRECISION = 490,
            CONSISTENCY_WARNING_SCALE = 491,
            CONSISTENCY_WARNING_DIGIT_AS_SHOWN = 492,
            CONSISTENCY_WARNING_ALWAYS_RECIEVE_DIGITS = 493,
            CALCULATION_WARNING_UNUSED_TOTAL_ELEMENT = 495,
            ELEMENT_IN_CALCULATION_NOT_IN_PRESENTATION_WARNING = 496,
            CALCULATION_WARNING_POSSIBLE_APPALACHIAN_TREE = 498,
            PRESENTATION_REPORT_MISSING_WARNING = 501,
            LC3_WARNING = 510,
            LC3_WARNING_MEMBERS = 511,
            CALCULATION_WEIGHT_WARNING_NetCashProvidedByUsedInOperatingActivities = 600,
            CALCULATION_WEIGHT_WARNING_ALMOST_NEVER_RECIEVE_CALCULATIONWEIGHT_NEGATIVE = 601,
            CALCULATION_WEIGHT_WARNING_ALMOST_NEVER_RECIEVE_CALCULATIONWEIGHT_POSITIVE = 602,
            OTHER_LOW_PRIORITY_WARNING = 1000

            #endregion
        }


		public enum ErrorType
		{
			Error,
			Warning,
		}

		public ErrorType MyErrorType = ErrorType.Error;

		
        /// <summary>
        /// grouping of the error
        /// </summary>
        public ValidationCategoryType MyCategoryType = ValidationCategoryType.Other;

        /// <summary>
        /// sub grouping of the error
        /// </summary>
        public string MyCategorySubType;

        /// <summary>
        /// Error string
        /// </summary>
        public string MyErrorString;

		/// <summary>
		/// word/ excel/crosstag location if it is available..
		/// </summary>
		public object  MyLocationKey;

        /// <summary>
        /// Additional info about the error 
        /// </summary>
        public object additionalErrorInfo = null;

        /// <summary>
        /// sequence number of the error
        /// </summary>
        public SequenceEnum Sequence = SequenceEnum.OTHER_LOW_PRIORITY_WARNING;

		/// <summary>
		/// for some errors it does not make sense to perform calc validations	if that error occurs
		/// so AllowCalculationValidation will get set to true..
		/// most  sec validaiton errors should  not prevent calc val so this is left as true..
		/// </summary>
		public bool AllowCalculationValidation = true;

        public ValidationErrorInfo(
            string error,
            ValidationCategoryType catType,
            string subType,
            SequenceEnum seq)
        {
            MyErrorString = error;
            MyCategoryType = catType;
            MyCategorySubType = subType;
            Sequence = seq;
        }
		

		public ValidationErrorInfo(
            string error,
            ValidationCategoryType catType, 
            string subType, 
            SequenceEnum seq,object key )
		{
			MyErrorString = error;
            MyCategoryType = catType;
            MyCategorySubType = subType;
            MyLocationKey = key;
            Sequence = seq;
		}

		#region IComparable<ValidationErrorInfo> Members

		public int CompareTo(ValidationErrorInfo other)
		{
            int ret = this.MyCategoryType.CompareTo(other.MyCategoryType);
			if (ret != 0) return ret;


            return this.Sequence.CompareTo(other.Sequence);
		}

		#endregion
	}


    
}

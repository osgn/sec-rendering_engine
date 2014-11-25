//=============================================================================
// InstanceUtils (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This utility class contains methods in processing the instance documents.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Aucent.FilingServices.RulesRepository;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.XBRLParser;
using XBRLReportBuilder.Utilities;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;

namespace XBRLReportBuilder
{
	/// <summary>
	/// InstanceUtils
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public static class InstanceUtils
	{
		public const string USDCurrencyCode = "USD";
		public static string USDCurrencySymbol = "$";

		#region constants
        
		public const string CURRENCY_NAMESPACE = @"iso4217";

		public const string _noDefAvailable = "No definition available.";
		public const string _noAuthRefAvailable = "No authoritative reference available.";

		public const string _Precision = "Precision";
		public const string _PrecisionSegmented = "PrecisionSegmented";

		public const string _ScenarioAxis = "us-gaap_StatementScenarioAxis";
		public const string _ScenarioAsPreviouslyReport = "us-gaap_ScenarioPreviouslyReportedMember";
		public const string _ScenarioAdjustment = "us-gaap_ScenarioAdjustmentMember";

		public const string TOP_LEVEL_REPORT_INDICATOR = "R";
	


		#endregion

		public static bool TryLoadInstanceDocument (string instanceDocPath, 
			out Instance currentInstance, out ArrayList errors)
		{
			errors = new ArrayList();
			currentInstance = new Instance();

			if (currentInstance.TryLoadInstanceDoc( instanceDocPath, out errors ))
			{
				if ( currentInstance.NumAttributeErrors > 0 || currentInstance.NumSchemaErrors > 0 )
				{
					Console.WriteLine( "Instance doc parsing attribute errors: " + currentInstance.NumAttributeErrors );
					Console.WriteLine( "Instance doc parsing schema errors: " + currentInstance.NumSchemaErrors );
				}

				return true;
			}
			else
			{
				Console.WriteLine( "TryLoadInstanceDoc failed" );
				errors.Sort();
				foreach ( ParserMessage pm in errors )
				{
					if ( pm.Level != TraceLevel.Error )
					{
						break; // thru the errors
					}

					Console.WriteLine( "InstanceDoc error: " + pm.Message );
				}
			}
			
			return false;
		}

		public static Precision WritePrecision( MarkupProperty markup, ref int previousPrecision, ref int previousPrecisionSegmented, out bool hasSegments )
		{
			hasSegments = false;

			if( markup.precisionDef == null )
				return null;

			if( markup.contextRef.Segments != null && markup.contextRef.Segments.Count > 0 )
				hasSegments = true;

			int oldPrecision = 0;
			int newPrecision = markup.precisionDef.NumberOfDigits;
			if( hasSegments )
			{
				oldPrecision = previousPrecisionSegmented;
				newPrecision = Math.Max( oldPrecision, newPrecision );
				if( newPrecision > oldPrecision )
				{
					previousPrecisionSegmented = newPrecision;
					return markup.precisionDef;
				}
			}
			else
			{
				oldPrecision = previousPrecision;
				newPrecision = Math.Max( oldPrecision, newPrecision );
				if( newPrecision > oldPrecision )
				{
					previousPrecision = newPrecision;
					return markup.precisionDef;
				}
			}

			return null;
		}

		public static readonly List<string> ExtendedNegatedRoles = new List<string>(
			new string[]
			{
				"negated"
				,"negatedLabel"
				,"negatedNetLabel"

				,"negatedPeriodEnd"
				,"negatedPeriodEndLabel"

				,"negatedPeriodStart"
				,"negatedPeriodStartLabel"

				,"negatedTerseLabel"

				,"negatedTotal"
				,"negatedTotalLabel"
			}
		);


		public static bool IsDisplayReversed( Node n )
		{
			//check base taxonomy negated label roles
			if( n.IsDisplayReversed() )
				return true;

			//check others
			if( ExtendedNegatedRoles.Contains( n.PreferredLabel ) )
				return true;

			return false;
		}

		public static void BuildSharedKeyParts(StringBuilder keyNameString, ContextProperty cp, bool includeEntityID)
		{
			if (includeEntityID)
			{
				keyNameString.Append(TraceUtility.FormatStringResource("DragonView.Data.EntityIDColon", cp.EntityValue));
				keyNameString.Append(Environment.NewLine);
			}
			
			foreach (Segment s in cp.Segments)
			{
				keyNameString.Append (InstanceUtils.BuildSegmentScenarioName( s ) ).Append(Environment.NewLine);
			}           

			if ( cp.PeriodType == Element.PeriodType.duration )
			{
				keyNameString.Append( string.Format("{0} - {1}",
                    cp.PeriodStartDate.ToString("d"),
                    cp.PeriodEndDate.ToString("d")));
			}
			else if (cp.PeriodType == Element.PeriodType.instant)
			{
				// instant
				keyNameString.Append( cp.PeriodStartDate.ToString("d"));
			}
			else //forever
			{
				keyNameString.Append (TraceUtility.FormatStringResource("DragonView.Data.Forever"));
			}

			keyNameString.Append(Environment.NewLine);
		}

		public static string GetCurrencyCodeFromMCU( MergedContextUnitsWrapper mcu )
		{
			if( mcu == null )
				return string.Empty;

			foreach( UnitProperty up in mcu.UPS )
			{
				string code = GetCurrencyCodeFromUnit( up );
				if( !string.IsNullOrEmpty( code ) )
					return code;
			}

			return string.Empty;
		}

		public static string GetCurrencyCodeFromUnit( UnitProperty up )
		{
			if( up == null )
				return string.Empty;

			string code = string.Empty;
			if( up.UnitType == UnitProperty.UnitTypeCode.Standard && up.StandardMeasure.MeasureSchema.Contains( "4217" ) )
			{
				code = up.StandardMeasure.MeasureValue;
			}
			else if( up.UnitType == UnitProperty.UnitTypeCode.Divide && up.NumeratorMeasure != null && up.NumeratorMeasure.MeasureSchema.Contains( "4217" ) )
			{
				code = up.NumeratorMeasure.MeasureValue;
			}

			if( code == null )
				return string.Empty;

			code = code.Trim().ToUpper();
			return code;
		}

		public static string GetCurrencySymbolFromCode( string currencyCode )
		{
			ISOUtility.CurrencyCode cc = ISOUtility.GetCurrencyCode( currencyCode );
			string currencySymbol = GetCurrencySymbol( cc );
			return currencySymbol;
		}

        public static string GetCurrencySymbol(ISOUtility.CurrencyCode cc)
        {
			if( cc == null || string.IsNullOrEmpty( cc.Symbol ) )
				return string.Empty;

            if ( string.Equals( cc.Code, InstanceUtils.USDCurrencyCode))
                return InstanceUtils.USDCurrencySymbol;

			return cc.Symbol;

			//DEFECT 23613
			//CEE: I have no idea at all why we used to do the following
			//
            //get the decimal value from cc.Symbol, which has the format of &#36;
			//
			//MatchCollection matches = Regex.Matches( cc.Symbol, @"&#(\d+);" );
			//if( matches.Count == 0 )
			//    return string.Empty;

			//StringBuilder symbol = new StringBuilder();
			//foreach( Match m in matches )
			//{
			//    int decimalValue = int.Parse( m.Groups[ 1 ].Value );
			//    symbol.AppendFormat( "&#{0};", decimalValue );
			//}

			//return symbol.ToString();
        }

		public static void AppendScenarios (StringBuilder keyNameString, ContextProperty cp)
		{
			foreach (Scenario s in cp.Scenarios)
			{
				keyNameString.Append(InstanceUtils.BuildSegmentScenarioName(s)).Append(Environment.NewLine);
			}
		}

		public static InstanceStatistics GetStatisticsFromInstance (Instance currentInstance)
		{
			InstanceStatistics instanceStat = new InstanceStatistics(currentInstance);
			return instanceStat;
		}      

		public static string BuildSegmentScenarioName(Segment s)
		{
            s.ValueName = s.ValueName.Replace(Environment.NewLine, " ");
            return s.ValueName == string.Empty ? s.ValueType : "{" + s.ValueType + "} : " + s.ValueName;
            //return s.ValueName == string.Empty ? s.ValueType : s.ValueName;
		}

		public static string BuildSegmentScenarioName(Scenario s)
		{
            s.ValueName = s.ValueName.Replace(Environment.NewLine, " ");
			return s.ValueName == string.Empty ? s.ValueType : "{" + s.ValueType + "} : " + s.ValueName;
            //return s.ValueName == string.Empty ? s.ValueType : s.ValueName;
		}

		public static RoundingLevel GetRoundingLevelFromPrecision(Precision prec)
		{
			if( prec == null )
				return RoundingLevel.UnKnown;

			if( prec.PrecisionType != Precision.PrecisionTypeCode.Decimals )
				return RoundingLevel.UnKnown;

			if( prec.NumberOfDigits > 0 )
				return RoundingLevel.NoRounding;

			int posNumDigits = prec.NumberOfDigits * -1;
			if( !Enum.IsDefined( typeof( RoundingLevel ), posNumDigits ) )
				return RoundingLevel.UnKnown;

			RoundingLevel rounding = (RoundingLevel)posNumDigits;

			//Ensure that `tens` and `hundreds` are not rounded
			if( rounding == RoundingLevel.Hundreds || rounding == RoundingLevel.Tens )
				return RoundingLevel.NoRounding;
			else
				return rounding;
		}

		public static bool IsNumericDataType( string elementDataType, string simpleDataType )
		{
			bool isNumeric = false;

			// floats and doubles are numeric types, but are not recognized as so by the parser
			if( string.Equals( elementDataType, "xbrli:floatItemType", StringComparison.CurrentCultureIgnoreCase ) ||
				string.Equals( elementDataType, "xbrli:doubleItemType", StringComparison.CurrentCultureIgnoreCase ) )
			{
				isNumeric = true;
			}
			else
			{
				bool isGYear = elementDataType.Equals( Element.GYEAR_ITEM_TYPE, StringComparison.CurrentCultureIgnoreCase );
				bool isNA = simpleDataType.Equals( "na", StringComparison.CurrentCultureIgnoreCase );
				bool isString = simpleDataType.ToLower().Contains( "string" );
				isNumeric = !( isGYear || isNA || isString );
			}

			return isNumeric;
		}

		public static List<string> BEGINNING_BALANCE_ROLES = new List<string>(
		new string[]
		{
			"periodStart",
			"negatedPeriodStart",
			"negatedPeriodStartLabel"
		});

		public static bool IsBeginningBalance( InstanceReportRow row )
		{
			if( string.IsNullOrEmpty( row.PreferredLabelRole ) )
				return false;

			foreach( string role in BEGINNING_BALANCE_ROLES )
			{
				if( row.PreferredLabelRole.IndexOf( role, StringComparison.InvariantCulture ) > -1 )
					return true;
			}

			return false;
		}

		public static List<string> ENDING_BALANCE_ROLES = new List<string>(
		new string[]
		{
			"periodEnd",
			"negatedPeriodEnd",
			"negatedPeriodEndLabel"
		} );

		public static bool IsEndingBalance( InstanceReportRow row )
		{
			if( string.IsNullOrEmpty( row.PreferredLabelRole ) )
				return false;

			foreach( string role in ENDING_BALANCE_ROLES )
			{
				if( row.PreferredLabelRole.IndexOf( role, StringComparison.InvariantCulture ) > -1 )
					return true;
			}

			return false;
		}
	}
}
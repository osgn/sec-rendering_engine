//=============================================================================
// ReportUtils (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the utility class that handles processing report-level properties.
//=============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace XBRLReportBuilder.Utilities
{
    public class ReportUtils
    {
		public static readonly string REP_NUM_STMT_STOCKHOLDERS_EQ = "148600";
		public static readonly string REP_NUM_STMT_STOCKHOLDERS_EQ_ALT = "148610";
        public static readonly string PARENTICAL = "parenthetical";
        public static readonly string STATEMENT = "statement";

		public static readonly Regex TYPE_CHECK = new Regex(@"^\s*[0-9]+\s*-\s*(?<type>[a-z]+)\s*-", RegexOptions.IgnoreCase);

		public static bool Exists<T>( List<T> list, Predicate<T> test )
		{
			bool exists = list.FindIndex( test ) > -1;
			return exists;
		}

		/// <summary>
		/// Determines the type of report based on the long name.  Returns null if unable to identify a report type.
		/// </summary>
		/// <param name="reportName">Long name of report.</param>
		/// <returns>A <see cref="T:String"/> indicating the report type, or null if the report name does not indicate a type.</returns>
		/// <exception cref="T:ArgumentNullException"><paramref name="reportName"/> is a null reference.</exception>
		public static string GetType(string reportName)
		{
			if (reportName == null)
			{
				throw new ArgumentNullException("Cannot perform detection of report type because the report name is null.");
			}

			Match reportType = ReportUtils.TYPE_CHECK.Match(reportName);

			if (!reportType.Success)
			{
				return null;
			}
			else
			{
				return reportType.Groups["type"].Value;
			}
		}


		/// <summary>
		/// Determines if the report is a statement.  If the report type cannot be determine, returns null.
		/// </summary>
		/// <param name="reportName"></param>
		/// <returns>A nullable <see cref="T:Boolean"/> indicating if the report is a Statement or null if it cannot be determined.</returns>
		/// <exception cref="T:ArgumentNullException"><paramref name="reportName"/> is a null reference.</exception>
		public static bool? IsStatement(string reportName)
		{
			if (reportName == null)
			{
				throw new ArgumentNullException("Cannot determine if report is Statement because the report name is null.");
			}

			string reportType = ReportUtils.GetType(reportName);

			// Report type cannot be determined
			if (reportType == null)
			{
				return null;
			}

			// Report type is statement
			if (string.Equals(reportType, ReportUtils.STATEMENT, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
			// Report type is something other than statement
			else
			{
				return false;
			}
		}
    
        public static bool IsTransposeReport( string reportName )
        {
            return (reportName.IndexOf("{transposed}", StringComparison.CurrentCultureIgnoreCase) >= 0);                           
        }

        public static bool IsDataReport(string reportName)
        {
            return (reportName.IndexOf("(data)", StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        public static bool IsUnlabeledReport(string reportName)
        {
            return (reportName.IndexOf("{unlabeled}", StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        public static bool IsStatementOfCashFlows( string reportName )
        {
            if ( reportName.IndexOf( "statement", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
                reportName.IndexOf( "cash", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
                reportName.IndexOf( "flow", StringComparison.CurrentCultureIgnoreCase ) >= 0 )
            {
                return true;
            }

            return false;
        }

        public static bool IsStatementOfDisclosureCashFlows(string reportName)
        {
            if (reportName.IndexOf("statement", StringComparison.CurrentCultureIgnoreCase) >=0)
            {
                if (reportName.IndexOf("cash", StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    reportName.IndexOf("flow", StringComparison.CurrentCultureIgnoreCase) >= 0
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDisclosureReport(string reportName)
        {           
            return reportName.IndexOf("disclosure", StringComparison.CurrentCultureIgnoreCase) >= 0;
                
        }

        public static bool IsIncomeStatement( string reportName )
        {
            // make sure we don't return Comprehensive Income as income statement
            return reportName.IndexOf( "income", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
                reportName.IndexOf( "comprehensive", StringComparison.CurrentCultureIgnoreCase ) == -1;
        }

        public static bool IsHighlights(string reportName)
        {
            return reportName.IndexOf("highlights", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool IsChangesInNetAssets(string reportName)
        {
            return reportName.IndexOf("Change", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                   reportName.IndexOf("Net Assets", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool IsFinancialHighlights(string reportName)
        {
            return reportName.IndexOf("financial", StringComparison.CurrentCultureIgnoreCase) >= 0 && reportName.IndexOf("highlights", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool IsEarningRelease(string reportName)
        {
            return reportName.IndexOf("earning", StringComparison.CurrentCultureIgnoreCase) >= 0 && reportName.IndexOf("release", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool IsBalanceSheet( string reportName )
        {
            if ( reportName.IndexOf( "balance sheet", StringComparison.CurrentCultureIgnoreCase ) >= 0 ||
                reportName.IndexOf( "balancesheet", StringComparison.CurrentCultureIgnoreCase ) >= 0 ||
                (reportName.IndexOf( "statement", StringComparison.CurrentCultureIgnoreCase ) >= 0 && reportName.IndexOf( "financial position", StringComparison.CurrentCultureIgnoreCase ) >= 0) )
            {
                return true;
            }

            return false;
        }

        public static bool IsLevel4DetailReport(string reportName)
        {
            return reportName.EndsWith("(Details)", StringComparison.CurrentCultureIgnoreCase) ||
                   reportName.EndsWith("(Detail)", StringComparison.CurrentCultureIgnoreCase) ||
                   reportName.EndsWith("(Details) (Parenthetical)", StringComparison.CurrentCultureIgnoreCase) ||
                   reportName.EndsWith("(Detail) (Parenthetical)", StringComparison.CurrentCultureIgnoreCase);                            
        }

        public static bool IsStatementOfStockholdersEquity( string reportName )
        {
			if (reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ) ||
				reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ_ALT))
			{
				return true;
			}

            if (reportName.IndexOf(STATEMENT, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                if (reportName.IndexOf("stockholder", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf("equity", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    return true;
                }
				if( reportName.IndexOf( "stockholder", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
					reportName.IndexOf( "deficit", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
					reportName.IndexOf( PARENTICAL, StringComparison.CurrentCultureIgnoreCase ) < 0 )
				{
					return true;
				}
                else if (reportName.IndexOf("partners", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf("capital", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    return true;
                }
                else if(reportName.IndexOf("changes", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                   reportName.IndexOf("equity", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                   reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    return true;
                }
                else if (reportName.IndexOf("capital", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf("accounts", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                    reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    return true;

                }
            }
            return false;
        }

        public static bool IsStatementOfShareholdersEquity(string reportName)
        {

			if (reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ) ||
				reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ_ALT))
			{
				return true;
			}
            else if (reportName.IndexOf("shareholder", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf("equity", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                return true;
            }
			else if( reportName.IndexOf( "shareholder", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
				reportName.IndexOf( "deficit", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
				reportName.IndexOf( PARENTICAL, StringComparison.CurrentCultureIgnoreCase ) < 0 )
			{
				return true;
			}
            
            return false;
        }

        public static bool IsStatementOfChangesEquity(string reportName)
        {

            if (reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ) ||
                reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ_ALT))
            {
                return true;
            }
			else if( reportName.IndexOf( "changes", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
				reportName.IndexOf( "deficit", StringComparison.CurrentCultureIgnoreCase ) >= 0 &&
				reportName.IndexOf( PARENTICAL, StringComparison.CurrentCultureIgnoreCase ) < 0 )
			{
				return true;
			}
            else if (reportName.IndexOf("changes", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf("equity", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsStatementOfEquity(string reportName)
        {

            if (reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ) ||
                reportName.Contains(REP_NUM_STMT_STOCKHOLDERS_EQ_ALT))
            {
                return true;
            }
            else if (reportName.IndexOf("statement", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf("equity", StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                reportName.IndexOf(PARENTICAL, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsCashFlowFromOps( string reportName )
        {
            if ( reportName.IndexOf( "cash", StringComparison.CurrentCultureIgnoreCase ) >= 0 || 
                reportName.IndexOf( "flow", StringComparison.CurrentCultureIgnoreCase ) >= 0 )
            {
                return true;
            }

            return false;
        }

        public static bool IsShowElements(string reportName)
        {
            return (reportName.IndexOf("{elements}", StringComparison.CurrentCultureIgnoreCase) >= 0);
        }


		public static bool IsStatementOfEquityCombined( string reportLongName )
		{
			// Disclosure reports should not be rendered as equity statements
			//if( ReportUtils.IsDisclosureReport( reportLongName ) )
				//return false;

			// No type other than Statement should be rendered as equity (although
			// we allow indeterminate reports to fall through here)
			if ( ReportUtils.IsStatement( reportLongName ) == false )
				return false;

			if( ReportUtils.IsStatementOfStockholdersEquity( reportLongName ) )
				return true;

			if( ReportUtils.IsStatementOfShareholdersEquity( reportLongName ) )
				return true;
			
			if( ReportUtils.IsStatementOfChangesEquity( reportLongName ) )
				return true;

			if( ReportUtils.IsStatementOfEquity( reportLongName ) )
				return true;

			return false;
		}
	}
}

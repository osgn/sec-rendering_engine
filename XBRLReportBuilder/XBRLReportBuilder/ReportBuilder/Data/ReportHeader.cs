//=============================================================================
// Report (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class impelements properties for the report header. 
// Includes report type, long name, short name and the actula XML file name for the 
// R file.
//=============================================================================

using System;
using System.Text;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	/// <summary>
	/// Report
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class ReportHeader
	{
		#region properties

        private const string _tupleContentFriendlyName = "Other Items";

		/// <summary>
		/// If isDefault, this will be the first report to be displayed when a filing is selected.
		/// </summary>
		public bool IsDefault;

		public bool HasEmbeddedReports;
		public string HtmlFileName;
		public string LongName;
		public ReportHeaderType ReportType;
		public string Role;
		public string ShortName = string.Empty;

		[XmlIgnore]
		public Node TopLevelNode;

		public string XmlFileName;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Report.
		/// </summary>
		public ReportHeader()
		{
		}

        public ReportHeader( string longName, string xmlFileName ) : this()
        {
            this.LongName = longName;
            this.ShortName = GetShortName( longName );
            this.XmlFileName = xmlFileName;
        }

        public ReportHeader( string longName, string xmlFileName, ReportHeaderType type ) :
			this( longName, xmlFileName )
        {
            this.ReportType = type;
        }
		#endregion

		#region helper functions

        public void ReplaceWith( ReportHeader arg )
        {
            this.IsDefault = arg.IsDefault;
            this.LongName = arg.LongName;
            this.ReportType = arg.ReportType;
            this.ShortName = arg.ShortName;
            this.XmlFileName = arg.XmlFileName;
        }

        public bool IsIncomeStatement()
        {
            return ReportUtils.IsIncomeStatement( ShortName );
        }

        public bool IsBalanceSheet()
        {
            return ReportUtils.IsBalanceSheet( ShortName );
        }

        public bool IsStatementOfStockholdersEquity()
        {
            return ReportUtils.IsStatementOfEquityCombined( this.LongName );
        }

        public bool IsCashFlowFromOps()
        {
            return ReportUtils.IsCashFlowFromOps( ShortName );
        }

        public bool IsStatementOfCashFlows()
        {
            return ReportUtils.IsStatementOfCashFlows( ShortName );
        }

        public bool IsFinancialHighlights()
        {
            return ReportUtils.IsFinancialHighlights(ShortName);
        }

        public static string GetShortName( string longName )
		{
			string strippedURL = longName;
			string shortName = string.Empty;

            strippedURL = strippedURL.Replace("{Unlabeled}", "").Replace("(Data)", "");
            strippedURL = strippedURL.Replace("{Transposed}", "").Replace("(Shareholders' Equity)", "");

            if (longName.IndexOf("http://", StringComparison.CurrentCultureIgnoreCase) >= 0)//URL
            {
                int index = longName.LastIndexOf(@"/");
                if (index >= 0 && index < longName.Length - 1)
                {
                    strippedURL = longName.Substring(index + 1);
                }
            }

			//remove underscores and replace with spaces
			strippedURL = strippedURL.Replace( "_", " " );

            if (strippedURL.IndexOf(" ") < 0 && !FoundLowerCaseLetters(strippedURL)) //there is no spaces in the name, simply return the name
                return strippedURL;

			if (strippedURL.IndexOf (" ") > 0)
			{
                if (strippedURL.ToLower().IndexOf("tuple") >= 0 &&
                    strippedURL.ToLower().IndexOf("content") >= 0 &&
                    strippedURL.ToLower().IndexOf("model") >= 0)
                {
                    return _tupleContentFriendlyName;
                }
                else
                {
                    return strippedURL;
                }
			}
			else
			{
				shortName = strippedURL;
				StringBuilder sbNewName = new StringBuilder();
				//Look for upper case letters to create breaking point (add space between words)
				int len = strippedURL.Length;
				int wordBeginIndex = 0;

				for (int lIndex = 1; lIndex < len; lIndex++)
				{
					char myChar = strippedURL[lIndex];
					int ASCCode = myChar;

					if (ASCCode >= 65 && ASCCode <= 90) //upper case
					{
						string word = strippedURL.Substring (wordBeginIndex, lIndex - wordBeginIndex );
						sbNewName.Append (word).Append (" ");
						wordBeginIndex = lIndex;
					}
				}

				sbNewName.Append (strippedURL.Substring (wordBeginIndex));

				if (sbNewName.ToString().Length > 0)
					shortName = sbNewName.ToString();

			}

            bool foundLowerCaseLetter = FoundLowerCaseLetters(shortName);

			if (!foundLowerCaseLetter)
			{
				shortName = shortName.Substring(0, 1) + shortName.Substring (1).ToLower();
			}

            if (shortName.ToLower().IndexOf("tuple") >= 0 &&
                shortName.ToLower().IndexOf("content") >= 0 &&
                shortName.ToLower().IndexOf("model") >= 0)
            {
                return _tupleContentFriendlyName;
            }

            
			return shortName;
		}

		/// <summary>
		/// Processing tokens are identified by {}
		/// </summary>
		/// <param name="originalName"></param>
		/// <returns></returns>
		public static string RemoveProcessingTokens( string originalName )
		{
			String removedTokenstring = Regex.Replace( originalName, @"\{.*?\}", String.Empty ).Trim();
			return removedTokenstring;
		}

		/// <summary>
		/// This method will remove the leading report number and the report type
		/// For example, a report might be 142000 - Statement - Income Statement, the short name would be chnaged to just "Income Statement"
		/// Anotehr example, a rport might be 199999 - Notes to the Financial Statement, the short name would be chnaged to just "Notes to the Financial Statement"
		/// </summary>
		/// <param name="shortName"></param>
		/// <returns></returns>
		public static string RemoveReportNumber( string shortName )
		{
            Match reportNamePieces = Regex.Match( shortName, @"^\s*(?<sortcode>\d+)\s*-\s*((?<type>\w+)\s+-\s+)?(?<reportname>.+)$", RegexOptions.ExplicitCapture | RegexOptions.Singleline );

            // No match, return full
            if( !reportNamePieces.Success )
            {
                return shortName;
            }

            // TODO enumerate type if it becomes standardized
            return reportNamePieces.Groups[ "reportname" ].ToString();
		}

        private static bool FoundLowerCaseLetters(string name)
        {
            foreach (char c in name)
            {
                int ASCCode = c;
                if ((ASCCode < 65 || ASCCode > 90) && ASCCode != 32)
                {
                    return true;
                    
                }
            }

            return false;
        }

		#endregion
	}
}
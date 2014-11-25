using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder.Utilities;
using System.IO;
using Aucent.MAX.AXE.XBRLParser;
using System.Collections;

namespace XBRLReportBuilder
{
	public partial class ReportBuilder
	{
		private Dictionary<string, List<string>> uniqueReportElements = new Dictionary<string, List<string>>();
		private Dictionary<string, List<string>> uniqueReportSegments = new Dictionary<string, List<string>>();
		private Dictionary<string, InstanceReport.ElementSegmentCombinations> uniqueReportElementSegmentCombos = new Dictionary<string, InstanceReport.ElementSegmentCombinations>();

		/// <summary>
		/// <para>Applies 3 formatting rules to <paramref name="instanceReport"/>:</para>
		/// <para> - Underlines total label rows</para>
		/// <para> - Displays a value as a ratio (percent) instead of decimal</para>
		/// <para> - Displays the word 'none' instead of a 0</para>
		/// </summary>
		/// <param name="instanceReport">The report whose rows/cells should be modified</param>
		private void ApplyRulesToReport( InstanceReport instanceReport )
		{
			instanceReport.Rows.ForEach( ir =>
			{
				this.currentRowPreferredLabelRole = ir.PreferredLabelRole;
				this.ProcessTotalLabelRow( ir );
			} );

			instanceReport.Rows.ForEach( ir =>
			{
				this.IsRatioElement = false;
				this.ProcessRatioCell( ir );
				if( this.IsRatioElement )
					ir.Cells.ForEach( rc => rc.IsRatio = true );
			} );

			instanceReport.Rows.ForEach( ir =>
			{
				this.IsZeroAsNoneElement = false;
				this.ProcessZeroAsNoneCell( ir );
				if( this.IsZeroAsNoneElement )
					ir.Cells.ForEach( rc => rc.DisplayZeroAsNone = true );
			} );
		}

		/// <summary>
		/// Builds a nested dictionary of all elements and which Segments they are combined with for all reports.
		/// </summary>
		/// <param name="rhCurrent">The current <see cref="ReportHeader"/> - its report will be skipped from cataloging.</param>
		/// <returns></returns>
		private InstanceReport.ElementSegmentCombinations BuildInUseElementSegmentCombinationsForAllReports( ReportHeader rhCurrent )
		{
			InstanceReport.ElementSegmentCombinations allReportCombinations = new InstanceReport.ElementSegmentCombinations();

			foreach( ReportHeader rh in this.currentFilingSummary.MyReports )
			{
				if( rh.XmlFileName == rhCurrent.XmlFileName )
					continue;

				InstanceReport.ElementSegmentCombinations reportCombinations = this.BuildInUseElementSegmentCombinationsForCurrentReport( rh );
				if( reportCombinations != null )
					allReportCombinations.Merge( reportCombinations );
			}

			return allReportCombinations;
		}

		/// <summary>
		/// Builds a nested dictionary of all elements and which Segments they are combined with for 1 report, specified by <paramref name="rh"/>.
		/// </summary>
		/// <param name="rh">The <see cref="ReportHeader"/> whose element/segment combinations should be loaded.</param>
		/// <returns></returns>
		private InstanceReport.ElementSegmentCombinations BuildInUseElementSegmentCombinationsForCurrentReport( ReportHeader rh )
		{
			if( rh.ReportType == ReportHeaderType.Sheet || rh.ReportType == ReportHeaderType.Notes )
			{
				if( !this.uniqueReportElementSegmentCombos.ContainsKey( rh.XmlFileName ) ||
					this.uniqueReportElementSegmentCombos == null ||
					this.uniqueReportElementSegmentCombos.Count == 0 )
				{
					InstanceReport report = InstanceReport.LoadXml( Path.Combine( this.currentReportDirectory, rh.XmlFileName ) );
					InstanceReport.ElementSegmentCombinations elementSegmentCombinations = report.GetUniqueInUseElementSegmentCombinations();
					this.uniqueReportElementSegmentCombos[ rh.XmlFileName ] = elementSegmentCombinations;
				}
			}

			if( this.uniqueReportElementSegmentCombos.ContainsKey( rh.XmlFileName ) )
				return uniqueReportElementSegmentCombos[ rh.XmlFileName ];

			return null;
		}

		/// <summary>
		/// Builds a nested dictionary of all elements for all reports.
		/// </summary>
		/// <param name="rhCurrent">The current <see cref="ReportHeader"/> - its report will be skipped from cataloging.</param>
		/// <returns></returns>
		private Dictionary<string, int> BuildInUseElementsForAllReports( ReportHeader rhCurrent )
		{
			Dictionary<string, int> allUniqueElements = new Dictionary<string, int>();
			foreach( ReportHeader rh in this.currentFilingSummary.MyReports )
			{
				if( rh.XmlFileName == rhCurrent.XmlFileName )
					continue;

				Dictionary<string, int> reportUniqueElements = this.BuildInUseElementsForCurrentReport( rh );
				if( reportUniqueElements.Count > 0 )
				{
					foreach( string key in reportUniqueElements.Keys )
					{
						allUniqueElements[ key ] = 1;
					}
				}
			}

			return allUniqueElements;
		}

		/// <summary>
		/// Builds a dictionary of all elements for 1 report, specified by <paramref name="rh"/>.
		/// </summary>
		/// <param name="rh">The <see cref="ReportHeader"/> whose elements should be loaded.</param>
		/// <returns></returns>
		private Dictionary<string, int> BuildInUseElementsForCurrentReport( ReportHeader rh )
		{
			if( rh.ReportType == ReportHeaderType.Sheet || rh.ReportType == ReportHeaderType.Notes )
			{
				if( !this.uniqueReportElements.ContainsKey( rh.XmlFileName ) ||
					this.uniqueReportElements == null ||
					this.uniqueReportElements.Count == 0 )
				{
					InstanceReport report = InstanceReport.LoadXml( Path.Combine( this.currentReportDirectory, rh.XmlFileName ) );
					string[] uniqueElements = report.GetUniqueInUseElements();
					this.uniqueReportElements[ rh.XmlFileName ] = new List<string>( uniqueElements );
				}
			}

			Dictionary<string, int> reportUniqueElements = new Dictionary<string, int>();
			if( this.uniqueReportElements.ContainsKey( rh.XmlFileName ) && this.uniqueReportElements.Count > 0 )
			{
				foreach( string key in this.uniqueReportElements[ rh.XmlFileName ] )
				{
					reportUniqueElements[ key ] = 1;
				}
			}

			return reportUniqueElements;
		}


		/// <summary>
		/// For each report, check to see if at least ONE element do not exist in all other reports. If not, delete the report.
		/// For each column, check to see if at least ONE element do not exist in all other reports. If not, delete the column.        
		/// </summary>
		public void CleanupFlowThroughReports()
		{
			ArrayList reportsToRemove = new ArrayList();
			foreach( ReportHeader rh in this.currentFilingSummary.MyReports )
			{
				//EH: Based on Christy's request, if the report is Stockholder's Equity, do not remove columns
				//DuPont and Oceanic filings will not look correct if we remove the columns. All elements for certain periods are in both 
				//Balance Sheet and Stockholder's Equity
				if( ( rh.ReportType == ReportHeaderType.Sheet || rh.ReportType == ReportHeaderType.Notes ) )
				{
					if( !rh.IsStatementOfStockholdersEquity() )
					{
						Dictionary<string, int> htElementsCurrentReport = this.BuildInUseElementsForCurrentReport( rh );
						Dictionary<string, int> htElementsOtherReports = this.BuildInUseElementsForAllReports( rh );
						//Dictionary<string, int> uniqueSegments = this.BuildInUseSegmentsForAllReports( rh );

						int elementFoundInOtherReports = 0;
						foreach( string currentElementName in htElementsCurrentReport.Keys )
						{
							if( htElementsOtherReports.ContainsKey( currentElementName ) )
							{
								elementFoundInOtherReports += 1;
							}
						}

						if( elementFoundInOtherReports == htElementsCurrentReport.Count ) //all elements in other report
						{
							//Remove the report
							if( !ReportUtils.IsHighlights( rh.ShortName ) && !ReportUtils.IsBalanceSheet( rh.ShortName ) && !ReportUtils.IsIncomeStatement( rh.ShortName ) )
							{
								reportsToRemove.Add( rh );
							}
						}
						else
						{
							InstanceReport.ElementSegmentCombinations allInUseElementsSegments = this.BuildInUseElementSegmentCombinationsForAllReports( rh );
							this.CleanupColumns( rh, allInUseElementsSegments );
							this.uniqueReportElementSegmentCombos.Remove( rh.XmlFileName );

							//CleanupColumns( rh, htElementsOtherReports, uniqueSegments );
						}
					}
				}
			}

			if( reportsToRemove.Count > 0 )
			{
				foreach( ReportHeader rhToRemove in reportsToRemove )
				{
					//delete from MyFilingSummary
					for( int rIndex = currentFilingSummary.MyReports.Count - 1; rIndex >= 0; rIndex-- )
					{
						ReportHeader rh1 = currentFilingSummary.MyReports[ rIndex ] as ReportHeader;
						if( rh1.XmlFileName == rhToRemove.XmlFileName )
						{
							currentFilingSummary.MyReports.RemoveAt( rIndex );
							break;
						}
					}
					string fileName = currentReportDirectory + Path.DirectorySeparatorChar + rhToRemove.XmlFileName;
					this.currentFilingSummary.TraceWarning( "Process Flow-Through Report: removing '" + rhToRemove.LongName + "'" );
					File.Delete( fileName );
				}
			}
		}

		/// <summary>
		/// Go thorugh each column to determine if the columns should be removed -- 
		/// if all elements (that have data) in the columns can be found in other reports then consider this a flow thru column and delete it.
		/// For "disclosures", if one element or one dimension member s unique, do remove ANY columns from the disclosure
		/// </summary>
		/// <param name="rh"></param>
		/// <param name="allInUseElementsSegments"></param>
		private void CleanupColumns( ReportHeader rh, InstanceReport.ElementSegmentCombinations allInUseElementsSegments )
		{
			string filePath = Path.Combine( this.currentReportDirectory, rh.XmlFileName );
			InstanceReport report = InstanceReport.LoadXml( filePath );

			if( report == null )
				return;

			InstanceReportColumn lastSegmentColumn = null;
			bool hasSegmentsOnRows = report.HasSegmentsOnRows();
			List<InstanceReportColumn> columnsToRetain = new List<InstanceReportColumn>();
			foreach( InstanceReportRow row in report.Rows )
			{
				if( hasSegmentsOnRows )
				{
					if( row.OriginalInstanceReportColumn != null )
						lastSegmentColumn = row.OriginalInstanceReportColumn;
				}

				//we can't test this row
				if( string.IsNullOrEmpty( row.ElementName ) )
					continue;

				//we can't test this row
				if( row.IsEmpty() )
					continue;

				for( int c = 0; c < row.Cells.Count; c++ )
				{
					//skip this cell if we already have this column
					InstanceReportColumn currentColumn = report.Columns[ c ];
					if( columnsToRetain.Contains( currentColumn ) )
						continue;

					//we only want columns for cells which have data
					Cell cell = row.Cells[ c ];
					if( !cell.HasData )
						continue;

					//if the element doesn't even exist, we're set - add the column
					InstanceReportColumn segmentColumn = hasSegmentsOnRows ? lastSegmentColumn : currentColumn;
					if( !allInUseElementsSegments.ContainsElement( row.ElementName ) )
					{
						columnsToRetain.Add( currentColumn );
					}
					else if( segmentColumn != null )
					{
						if( segmentColumn.Segments != null && segmentColumn.Segments.Count > 0 )
						{
							//if the element exists, but maybe its combination with the segments does not
							StringBuilder sb = new StringBuilder();
							foreach( string segmentKey in segmentColumn.GetUniqueInUseSegments().Keys )
							{
								sb.AppendLine( segmentKey );
							}

							if( !allInUseElementsSegments.ContainsElementAndSegments( row.ElementName, sb.ToString() ) )
								columnsToRetain.Add( currentColumn );
						}
					}
				}

				if( columnsToRetain.Count == report.Columns.Count )
					return;
			}

			if( columnsToRetain.Count == 0 || columnsToRetain.Count == report.Columns.Count )
				return;

			List<InstanceReportColumn> columnsToRemove = report.Columns.FindAll( col => !columnsToRetain.Contains( col ) );
			foreach( InstanceReportColumn col in columnsToRemove )
			{
				this.currentFilingSummary.TraceInformation( "\tProcess Flow-Through: Removing column '" + col.Label + "'" );
				col.RemoveSelf( report );
			}

			if( report.Columns.Count > 1 )
				report.PromoteSharedColumnLabelsAfterRemoveFlowThroughColumns();

			report.BuildXMLDocument( filePath, false, false, this.currentFilingSummary );
		}

		/// <summary>
		/// Go thorugh each column to determine if the columns should be removed -- 
		/// if all elements (that have data) in the columns can be found in other reports then consider this a flow thru column and delete it.
		/// For "disclosures", if one element or one dimension member s unique, do remove ANY columns from the disclosure
		/// </summary>
		/// <param name="rh"></param>
		/// <param name="inUseElements"></param>
		/// <param name="inUseSegments"></param>
		private void CleanupColumns( ReportHeader rh, Dictionary<string, int> inUseElements, Dictionary<string, int> inUseSegments )
		{
			string filePath = Path.Combine( this.currentReportDirectory, rh.XmlFileName );
			InstanceReport report = InstanceReport.LoadXml( filePath );

			if( report == null )
				return;

			List<InstanceReportColumn> columnsToRemove = new List<InstanceReportColumn>();
			List<InstanceReportRow> uniqueRows = report.Rows.FindAll(
			row =>
			{
				if( string.IsNullOrEmpty( row.ElementName ) )
					return false;

				if( row.IsEmpty() )
					return false;

				if( inUseElements.ContainsKey( row.ElementName ) )
					return false;

				return true;
			} );

			bool hasAnyUniqeRows = uniqueRows.Count > 0;
			for( int c = 0; c < report.Columns.Count; c++ )
			{
				InstanceReportColumn col = report.Columns[ c ];
				bool columnHasUniqueSegments = false;
				bool columnHasUniqueCells = ReportUtils.Exists( uniqueRows, row => row.Cells[ c ].HasData );
				if( !columnHasUniqueCells ) //this column might need to be removed
				{
					Dictionary<string, int> colSegments = report.GetUniqueInUseSegments( col );
					foreach( string key in colSegments.Keys )
					{
						if( !inUseSegments.ContainsKey( key ) )
						{
							columnHasUniqueSegments = true;
							break;
						}
					}
				}

				if( !( columnHasUniqueCells || columnHasUniqueSegments ) )
				{
					columnsToRemove.Add( col );
				}
			}

			if( columnsToRemove.Count > 0 && columnsToRemove.Count < report.Columns.Count )
			{
				foreach( InstanceReportColumn col in columnsToRemove )
				{
					col.RemoveSelf( report );
				}

				if( report.Columns.Count > 1 )
					report.PromoteSharedColumnLabelsAfterRemoveFlowThroughColumns();

				report.BuildXMLDocument( filePath, false, false, this.currentFilingSummary );
			}
		}

		public void CleanupFlowThroughColumns()
		{
			for( int rhIndex = 0; rhIndex < currentFilingSummary.MyReports.Count; rhIndex++ )
			{
				ReportHeader rh = currentFilingSummary.MyReports[ rhIndex ] as ReportHeader;

				//Equity reports that do not have segments still need to have flow through columns
				//cleaned up because they were not processed with the regular equity processing logic
				bool isEquityStatement = false;

				if( ReportUtils.IsStatementOfEquityCombined( rh.LongName ) )
				{
					isEquityStatement = true;
				}

				if( isEquityStatement )
				{
					string instanceFile = Path.Combine( this.currentReportDirectory, rh.XmlFileName );
					InstanceReport ir = InstanceReport.LoadXml( instanceFile );
					if( ir.IsEquityReport )
						continue;
				}

				if( rh.ReportType == ReportHeaderType.Sheet || rh.ReportType == ReportHeaderType.Notes )
				{
					if( cleanupStatementsOnly )
					{
						if( ReportUtils.IsStatement( rh.LongName ) == true )
						{
							this.currentFilingSummary.TraceInformation( "Process Flow-Through: " + rh.LongName );

							InstanceReport.ElementSegmentCombinations allInUseElementsSegments = this.BuildInUseElementSegmentCombinationsForAllReports( rh );
							this.CleanupColumns( rh, allInUseElementsSegments );
							this.uniqueReportElementSegmentCombos.Remove( rh.XmlFileName );
						}
					}
					else
					{
						this.currentFilingSummary.TraceInformation( "Process Flow-Through: " + rh.LongName );

						InstanceReport.ElementSegmentCombinations allInUseElementsSegments = this.BuildInUseElementSegmentCombinationsForAllReports( rh );
						this.CleanupColumns( rh, allInUseElementsSegments );
						this.uniqueReportElementSegmentCombos.Remove( rh.XmlFileName );
					}
				}
			}

			this.uniqueReportElements = new Dictionary<string, List<string>>();
			this.uniqueReportSegments = new Dictionary<string, List<string>>();
			this.uniqueReportElementSegmentCombos = new Dictionary<string, InstanceReport.ElementSegmentCombinations>();
		}

		/// <summary>
		/// Uses the rules engine (<see cref="NxBRE"/> to selects a preferred language from the language supported by this.<see cref="currentTaxonomy"/>.
		/// </summary>
		/// <returns>The preferred language code.</returns>
		private string GetPreferredLanguage()
		{
			string preferredLang = string.Empty;

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.PREFERRED_LANGUAGE_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				PrefferedLanguageWrapper langWrapper = new PrefferedLanguageWrapper();
				langWrapper.SupportedLanguages = this.currentTaxonomy.SupportedLanguages;
				langWrapper.PrefferedLanguage = String.Empty;
				contextObjects.Add( "Languages", langWrapper );

				BuilderRules.ProcessRule( RulesEngineUtils.PREFERRED_LANGUAGE_RULE, contextObjects );
				preferredLang = langWrapper.PrefferedLanguage;

				this.FireRuleProcessed( RulesEngineUtils.PREFERRED_LANGUAGE_RULE );
			}

			return preferredLang;
		}

		/// <summary>
		/// A helper method for the 'DisplayAsRatio' rule.  See <see cref="ApplyRulesToReport"/>
		/// </summary>
		/// <param name="irr">The row to check if it is based on a 'percent' element.</param>
		/// <returns>True is this <paramref name="irr"/> is based on a 'percent' element.</returns>
		private bool IsAcceptedpercentItemType( InstanceReportRow irr )
		{
			bool isAccepted = false;

			string elemDataType = irr.ElementDataType;
			string[] parts = elemDataType.Split( ':' ) as string[];

			if( parts.Length == 2 )
			{
				string namespaceStr = parts[ 0 ];
				string dataType = parts[ 1 ].ToLower();

				bool acceptableNamespace = false;
				foreach( TaxonomyItem taxItem in currentTaxonomy.TaxonomyItems )
				{
					//www.xbrl.org/
					//xbrl.org/
					//xbrl.us/
					//xbrl.sec.gov/
					//fasb.org/
					//xbrl.iasb.org

					if( taxItem.Namespace == namespaceStr )
					{
						string webLocation = taxItem.WebLocation.ToLower();
						acceptableNamespace =
							( webLocation.IndexOf( "xbrl.org/" ) >= 0 ||
							webLocation.IndexOf( "xbrl.us/" ) >= 0 ||
							webLocation.IndexOf( "xbrl.sec.gov/" ) >= 0 ||
							webLocation.IndexOf( "fasb.org/" ) >= 0 ||
							webLocation.IndexOf( "xbrl.iasb.org/" ) >= 0 );
						break;
					}
				}
				isAccepted = acceptableNamespace && ( dataType == "percentitemtype" );
			}

			return isAccepted;


		}

		/// <summary>
		/// <para>Cleans (removes) columns and/or reports which only contain "flow-through" element values.</para>
		/// </summary>
		/// <seealso cref="CleanupFlowThroughColumns"/>
		/// <seealso cref="BuildInUseElementSegmentCombinationsForAllReports"/>
		/// <seealso cref="CleanupFlowThroughReports"/>
		/// <seealso cref="BuildInUseElementsForAllReports"/>
		/// <seealso cref="BuildInUseElementsForCurrentReport"/>
		/// <seealso cref="BuildInUseElementSegmentCombinationsForAllReports"/>
		private void ProcessFlowThroughColumnsReports()
		{
			Dictionary<string, object> contextObjects = new Dictionary<string, object>();
			contextObjects.Add( "ReportBuilder", this );

			//Cleanup columns
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.CLEAN_FLOWTHROUGH_COLUMNS_RULE );
			if( isRuleEnabled )
			{
				BuilderRules.ProcessRule( RulesEngineUtils.CLEAN_FLOWTHROUGH_COLUMNS_RULE, contextObjects );
				this.FireRuleProcessed( RulesEngineUtils.CLEAN_FLOWTHROUGH_COLUMNS_RULE );
			}

			//Cleanup statements only
			if( BuilderRules.IsRuleEnabled( RulesEngineUtils.CLEAN_FLOWTHROUGH_ONLY_FOR_STATEMENT_RULE ) )
			{
				this.cleanupStatementsOnly = true;
			}

			//Cleanup reports
			isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.CLEAN_FLOWTHROUGH_REPORTS_RULE );
			if( isRuleEnabled )
			{
				BuilderRules.ProcessRule( RulesEngineUtils.CLEAN_FLOWTHROUGH_REPORTS_RULE, contextObjects );
				this.FireRuleProcessed( RulesEngineUtils.CLEAN_FLOWTHROUGH_REPORTS_RULE );
			}
		}

		/// <summary>
		/// A helper method for the 'DisplayAsRatio' rule.  See <see cref="ApplyRulesToReport"/>
		/// </summary>
		/// <param name="thisRow">A row that was passed to <see cref="IsAcceptedpercentItemType"/> resulting in 'true'.</param>
		private void ProcessRatioCell( InstanceReportRow thisRow )
		{
			string elementName = thisRow.ElementName;
			int sepPos = elementName.IndexOf( "_" );
			if( sepPos > 0 )
			{
				elementName = elementName.Substring( sepPos + 1 );
			}
			this.CurrentElementName = elementName;

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.DISPLAY_AS_RATIO );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "ReportBuilder", this );
				BuilderRules.ProcessRule( RulesEngineUtils.DISPLAY_AS_RATIO, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.DISPLAY_AS_RATIO );

				//If the element is a annual return or the data type is PercentItemType, then this is a ratio element
				if( thisRow.ElementName.IndexOf( "AnnualReturn" ) >= 0 || IsAcceptedpercentItemType( thisRow ) )
				{
					IsRatioElement = true;
				}
			}
			else
			{
				IsRatioElement = false;
			}

		}

		/// <summary>
		/// A helper method for the 'TotalLabel' rule.  See <see cref="ApplyRulesToReport"/>
		/// </summary>
		/// <param name="thisRow">The row to check if it is based on a 'percent' element.</param>
		private void ProcessTotalLabelRow( InstanceReportRow thisRow )
		{
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.TOTAL_LABEL_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "Row", thisRow );
				contextObjects.Add( "ReportBuilder", this );

				BuilderRules.ProcessRule( RulesEngineUtils.TOTAL_LABEL_RULE, contextObjects );
				this.FireRuleProcessed( RulesEngineUtils.TOTAL_LABEL_RULE );
			}
		}

		/// <summary>
		/// A helper method for the 'DisplayZeroAsNone' rule.  See <see cref="ApplyRulesToReport"/>
		/// </summary>
		/// <param name="thisRow">The row to check if it is based on an element which should display as 'none'.</param>
		private void ProcessZeroAsNoneCell( InstanceReportRow thisRow )
		{
			string elementName = thisRow.ElementName;
			int sepPos = elementName.IndexOf( "_" );
			if( sepPos > 0 )
			{
				elementName = elementName.Substring( sepPos + 1 );
			}
			this.CurrentElementName = elementName;

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.DISPLAY_ZERO_AS_NONE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "ReportBuilder", this );
				BuilderRules.ProcessRule( RulesEngineUtils.DISPLAY_ZERO_AS_NONE, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.DISPLAY_ZERO_AS_NONE );
			}
			else
			{
				IsZeroAsNoneElement = false;
			}
		}

		public event EventHandler OnBuildReportsProcessing = null;
		private void FireBuildReportsProcessing()
		{
			if( this.OnBuildReportsProcessing != null )
				this.OnBuildReportsProcessing( this, new EventArgs() );
		}

		public event EventHandler OnBuildReportsProcessed = null;
		private void FireBuildReportsProcessed()
		{
			if( this.OnBuildReportsProcessed != null )
				this.OnBuildReportsProcessed( this, new EventArgs() );
		}

		public event RuleCancelHandler OnRuleProcessing = null;
		private bool FireRuleProcessing( string rule )
		{
			bool isRuleEnabled = this.builderRules.IsRuleEnabled( rule );

			if( this.OnRuleProcessing != null )
			{
				RuleCancelEventArgs args = new RuleCancelEventArgs( rule, !isRuleEnabled );
				this.OnRuleProcessing( this, args );
				isRuleEnabled = !args.Cancel;
			}

			return isRuleEnabled;
		}

		public event RuleProcessedHandler OnRuleProcessed = null;
		private void FireRuleProcessed( string rule )
		{
			if( this.OnRuleProcessing != null )
			{
				RuleEventArgs args = new RuleEventArgs( rule );
				this.OnRuleProcessed( this, args );
			}
		}

		private class PrefferedLanguageWrapper
		{
			private ArrayList supportedLanguages = null;
			public ArrayList SupportedLanguages
			{
				get { return supportedLanguages; }
				set { supportedLanguages = value; }
			}

			private string prefferedLanguage = null;
			public string PrefferedLanguage
			{
				get { return prefferedLanguage; }
				set { prefferedLanguage = value; }
			}

			public PrefferedLanguageWrapper()
			{ }
		}
	}
}

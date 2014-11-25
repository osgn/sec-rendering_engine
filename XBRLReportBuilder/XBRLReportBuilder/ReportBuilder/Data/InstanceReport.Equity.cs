using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using System.Collections;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.IO;
using System.Xml;
using NxBRE.FlowEngine;

namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		private bool ProcessEquitySegments( ITraceMessenger messenger )
		{
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.EQUITY_STATEMENT_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				contextObjects.Add( "messenger", messenger );

				IBRERuleResult ruleResult;
				builderRules.ProcessRule( RulesEngineUtils.EQUITY_STATEMENT_RULE, contextObjects, out ruleResult );
				this.FireRuleProcessed( RulesEngineUtils.EQUITY_STATEMENT_RULE );

				Exception outerException = ruleResult.Result as Exception;
				if( outerException != null )
				{
					IncompleteEquityException equityEx = outerException.InnerException as IncompleteEquityException;
					if( equityEx != null && messenger != null )
					{
						switch( equityEx.Type )
						{
							case IncompleteEquityException.ErrorType.Incomplete:
								messenger.TraceError( "Error: The equity rendering routine did not create any data for " + this.ReportLongName + "." +
									Environment.NewLine + "The equity statement will use the basic rendering." );
								break;
							case IncompleteEquityException.ErrorType.MissingBeginningBalance:
								messenger.TraceWarning( "Warning: The equity rendering routine was not applied to " + this.ReportLongName + "." +
									Environment.NewLine + "None of the elements feature the 'periodStartLabel' preferred label role." );
								break;
							case IncompleteEquityException.ErrorType.MissingEndingBalance:
								messenger.TraceWarning( "Warning: The equity rendering routine was not applied to " + this.ReportLongName + "." +
									Environment.NewLine + "None of the elements feature the 'periodEndLabel' preferred label role." );
								break;
						}
					}

					return false;
				}
			}

			return true;
		}

		public void ProcessEquity( string dateFormat, string equityMembersXml, ITraceMessenger messenger )
		{
			//we can't create the special equity layout without both beginning and ending balances
			//if there are none, exit early.
			XmlDocument membersXDoc;
			ValidateEquityStructure( equityMembersXml, out membersXDoc );

			//ensure that all rows/columns/cells are in sync
			this.SynchronizeGrid();

			/**
			 * Consider the possible SQL statement as a model for this process:
			 * 
			 * SELECT [Element values by Segment]
			 * FROM [Equity Statement]
			 * GROUP BY [Reporting Period]
			 * ORDER BY [Element Presentation Id]
			 * 
			 **/
			EquityDataSet dataSet = new EquityDataSet( membersXDoc );

			//#1 - this will be our X axis - a set of unique segments and scenarios, minus adjustments and previously reported
			dataSet.LoadSegmentScenarioColumns( this );

			//#2 - this will be the GROUPING for our Y axis - a set of unique calendars
			dataSet.LoadCalendarColumns( this );

			//#3 - Get each type of row role so that we can control the position
			dataSet.LoadRowTypes( this );

			if( !dataSet.PopulateEquityReport( this ) )
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.Incomplete );

			//ensure that all rows/columns/cells are in sync
			dataSet.EquityCandidate.SynchronizeGrid();
			dataSet.CleanupEquityColumns();
			dataSet.CleanupEquityRows( dateFormat, membersXDoc );

			//repair any elements or segments which might not fit the preferred rendering
			dataSet.EquityCandidate.SynchronizeGrid();
			dataSet.AdjustEquityPeriods( this );
			if( dataSet.EquityCandidate.Columns.Count == 0 || dataSet.EquityCandidate.Rows.Count == 0 )
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.Incomplete );

			this.Columns.Clear();
			this.Columns.AddRange( dataSet.EquityCandidate.Columns );

			this.Rows.Clear();
			this.Rows.AddRange( dataSet.EquityCandidate.Rows );

			this.Footnotes.Clear();
			this.Footnotes.AddRange( dataSet.EquityCandidate.Footnotes );

			//ensure that all rows/columns/cells are in sync
			this.SynchronizeGrid();

			this.PromoteCurrencyCode();	
		}

		private bool PromoteCurrencyCode()
		{
			if( this.Columns == null || this.Columns.Count == 0 )
				return false;

			string currencyCode = this.Columns[ 0 ].CurrencyCode;
			if( string.IsNullOrEmpty( currencyCode ) )
				return false;

			if( !this.ReportName.Contains( "(" + currencyCode ) )
				return false;

			//are all of the currencies the same?
			foreach( InstanceReportColumn col in this.Columns )
			{
				if( !string.Equals( currencyCode, col.CurrencyCode ) )
					return false;  //no
			}

			//yes
			foreach( InstanceReportColumn col in this.Columns )
			{
				col.RemoveCurrencyLabel();
			}

			return true;
		}

		private void ValidateEquityStructure( string equityMembersXml, out XmlDocument membersXDoc )
		{
			membersXDoc = new XmlDocument();

			bool hasBeginningBalance = ReportUtils.Exists( this.Rows, row => row.IsBeginningBalance );
			if( !hasBeginningBalance )
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.MissingBeginningBalance );

			bool hasEndingBalance = ReportUtils.Exists( this.Rows, row => row.IsEndingBalance );
			if( !hasEndingBalance )
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.MissingEndingBalance );

			string equityMembersXmlPath = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Rules, equityMembersXml );
			if( !File.Exists( equityMembersXmlPath ) )
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.MissingMembersFile );

			try
			{
				membersXDoc.Load( equityMembersXmlPath );
			}
			catch
			{
				throw new IncompleteEquityException( IncompleteEquityException.ErrorType.MembersFileFormat );
			}
		}
	}
}

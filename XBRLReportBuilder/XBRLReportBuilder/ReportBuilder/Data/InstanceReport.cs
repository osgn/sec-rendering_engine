//=============================================================================
// InstanceReport (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class contains information for a specific report in the filing.
// This is called the "R" file -- intermediate file that contains the 
// processed data from the instance and the taxonomy for a specific report.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Aucent.FilingServices.RulesRepository;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using NxBRE.FlowEngine;
using XBRLReportBuilder.Utilities;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace XBRLReportBuilder
{
	[Serializable]
	[XmlInclude( typeof( Footnote ) )]
	[XmlInclude( typeof( InstanceReportRow ) )]
	[XmlInclude( typeof( InstanceReportColumn ) )]
	[XmlInclude( typeof( EmbeddedUnitWrapper ) )]
	public partial class InstanceReport : IDisposable
	{
		//[XmlElement( Order = 1 )]
		public string Version = string.Empty;

		//[XmlElement( Order = 2 )]
		public string ReportLongName = string.Empty;

		//[XmlElement( Order = 3 )]
		public bool DisplayLabelColumn = true;

		//[XmlElement( Order = 4 )]
		public bool ShowElementNames = false;

		//Example: (in Millions, except per share data)
		//[XmlElement( Order = 5 )]
		public string RoundingOption = string.Empty;

		//[XmlElement( Order = 6 )]
		public bool HasEmbeddedReports = false;

		
		[XmlArray( "Columns" )]//, Order = 7 )]
		[XmlArrayItem( "Column", typeof( InstanceReportColumn ) )]
		public List<InstanceReportColumn> Columns = new List<InstanceReportColumn>();


		[XmlArray( "Rows" )]//, Order = 8 )]
		[XmlArrayItem( "Row", typeof( InstanceReportRow ) )]
		public List<InstanceReportRow> Rows = new List<InstanceReportRow>();


		[XmlArray( "Footnotes" )]//, Order = 9 )]
		[XmlArrayItem( "Footnote", typeof( Footnote ) )]
		public List<Footnote> Footnotes = new List<Footnote>();

		//[XmlElement( Order = 10 )]
		public bool IsEquityReport = false;

		//[XmlElement( Order = 11 )]
		public int NumberOfCols
		{
			get { return this.Columns.Count; }
			set
			{
				//Need a setter so that thie property will XML Serialize when we write out to file, but 
				//sense the value is derived from another property the setter shouldn't actually do anything
			}
		}

		//[XmlElement( Order = 12 )]
		public int NumberOfRows
		{
			get { return this.Rows.Count; }
			set
			{
				//Need a setter so that thie property will XML Serialize when we write out to file, but 
				//sense the value is derived from another property the setter shouldn't actually do anything
			}
		}

		//[XmlElement( Order = 13 )]
		public string ReportName = string.Empty;

		//[XmlElement( Order = 14 )]
		public RoundingLevel MonetaryRoundingLevel = RoundingLevel.UnKnown;

		//[XmlElement( Order = 15 )]
		public RoundingLevel SharesRoundingLevel = RoundingLevel.UnKnown;

		//[XmlElement( Order = 16 )]
		public RoundingLevel PerShareRoundingLevel = RoundingLevel.UnKnown;

		//[XmlElement( Order = 17 )]
		public RoundingLevel ExchangeRateRoundingLevel = RoundingLevel.UnKnown;

		//[XmlElement( Order = 18 )]
		public bool HasCustomUnits = false;

		//[XmlElement( Order = 19 )]
		public bool IsEmbedReport = false;

		//[XmlElement( Order = 20 )]
		public bool IsMultiCurrency = false;

		//[XmlElement( Order = 21 )]
		public ReportHeaderType ReportType;

		//[XmlElement( Order = 22 )]
		public string RoleURI = string.Empty;

		#region properties

		public static Encoding Encoding = Encoding.ASCII;

		public static readonly string EQUITY_TOTAL_HEADER = "Total";
		private const string ROUNDING_LEVEL_PLACEHOLDER = "{level}";

		[NonSerialized]
		private bool isUncatReport = false;

		[XmlIgnore]
		public bool IsUncatReport
		{
			get { return this.isUncatReport; }
			set { this.isUncatReport = value; }
		}

		[NonSerialized]
		private RulesRepository builderRules = null;

		[XmlIgnore]
		[NonSerialized]
		public Node TopLevelNode = null;

		//private bool containNonUSCurrency = false;
		[NonSerialized]
		private Dictionary<string, Segment> promotedToReportHeaderSegments = new Dictionary<string, Segment>( 0 );
		
		[XmlIgnore]
		public List<string> AxisByPresentation = new List<string>();

		[XmlIgnore]
		public Dictionary<string, List<Segment>> AxisMembersByPresentation = new Dictionary<string, List<Segment>>();

		[XmlIgnore]
		public Dictionary<string, Segment> AxisMemberDefaults = new Dictionary<string, Segment>();

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new InstanceReport.
		/// </summary>
		public InstanceReport()
		{
			this.LoadDefaultRules();
		}

		public InstanceReport( RulesRepository builderRules )
		{
			this.builderRules = builderRules;
		}

		~InstanceReport()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.builderRules = null;
			this.OnRuleProcessing = null;
			this.OnRuleProcessed = null;
		}

		private void LoadDefaultRules()
		{
			const string rulesFileName = "ReportBuilder";

			string rulesFolder = RulesEngineUtils.GetRulesFolder();
			DirectoryInfo di = new DirectoryInfo( rulesFolder );

			this.builderRules = new RulesRepository( rulesFileName, di );
			this.builderRules.TryLoadExistingRulesList();
		}



		#endregion

		#region override methods

#if DEBUG
		private static string ExtractTable( string reportHTML )
		{
			int tableStart = reportHTML.IndexOf( "<table", StringComparison.CurrentCultureIgnoreCase );
			int tableEnd = reportHTML.LastIndexOf( "</table", StringComparison.CurrentCultureIgnoreCase );
			tableEnd = reportHTML.IndexOf( '>', tableEnd ) + 1;

			string table = reportHTML.Substring( tableStart, tableEnd - tableStart );
			return table;
		}

		public static string ToHTML( params InstanceReport[] reports )
		{
			StringBuilder output = new StringBuilder();

			bool isTemplateReady = false;
			string footer = string.Empty;
			foreach( InstanceReport report in reports )
			{
				string reportHTML = report.ToHTML();

				if( !isTemplateReady )
				{
					isTemplateReady = true;
					int tableStart = reportHTML.IndexOf( "<table", StringComparison.CurrentCultureIgnoreCase );
					string header = reportHTML.Substring( 0, tableStart );

					int tableEnd = reportHTML.LastIndexOf( "</table", StringComparison.CurrentCultureIgnoreCase );
					tableEnd = reportHTML.IndexOf( '>', tableEnd ) + 1;
					footer = reportHTML.Substring( tableEnd );

					output.Append( header );
					output.AppendLine( "<table>" );
					output.AppendLine( "<tr>" );
				}

				string table = ExtractTable( reportHTML );

				output.AppendLine( "<td valign=\"top\">" );
				output.Append( table );
				output.AppendLine( "</td>" );
			}

			output.AppendLine( "</tr>" );
			output.AppendLine( "</table>" );
			output.Append( footer );
			return output.ToString();
		}

		public string ToHTML()
		{
			string reportCSSresource = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, "report.css" );
			string reportCSStemp = Path.Combine( Path.GetTempPath(), Path.GetFileName( reportCSSresource ) );
			if( !File.Exists( reportCSStemp ) )
			{
				try
				{
					//non-fatal:  helpful but not necessary
					File.Copy( reportCSSresource, reportCSStemp );
				}
				catch { }
			}



			string transformFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.TransformFile );

			XslCompiledTransform transform = new XslCompiledTransform();
			transform.Load( transformFile );

			XsltArgumentList argList = new XsltArgumentList();
			argList.AddParam( "asPage", string.Empty, "true" );

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = InstanceReport.Encoding;
			writerSettings.Indent = true;
			writerSettings.OmitXmlDeclaration = true;

			StringBuilder html = new StringBuilder();
			using( MemoryStream xmlStream = new MemoryStream() )
			{
				xmlSerializer.Serialize( xmlStream, this );
				xmlStream.Seek( 0, SeekOrigin.Begin );

				using( XmlReader xmlReader = XmlReader.Create( xmlStream ) )
				{
					using( TextWriter writer = new StringWriter( html ) )
					{
						transform.Transform( xmlReader, argList, writer );
					}
				}
			}

			return html.ToString();
		}
#endif

		#endregion

		#region public -- I/O

		public void FinalProcessing( bool isIMBased )
		{
			this.FinalProcessing( isIMBased, null );
		}

		public void FinalProcessing( bool isIMBased, ITraceMessenger messenger )
		{
			bool isShowElements = ReportUtils.IsShowElements( this.ReportLongName );
			if( !isShowElements )
			{
				if( this.IsEmbedReport )
				{
					this.MergeInstantAndDuration();
					this.ProcessRounding( messenger );
					return;
				}
			}

			this.PromoteSharedColumnLabels( false );
			this.MergeInstantAndDuration();
			this.ApplyCustomUnits();
		
			if( this.HasSegments() )
			{
				bool processedSegments = false;
				if( this.IsEquityReport )
				{
					if( this.ProcessEquitySegments( messenger ) )
						processedSegments = true;
					else
						this.IsEquityReport = false;
				}
				else if( this.IsEmbedReport)
				{
					if( !isShowElements )
						processedSegments = true;
				}
				else if( this.ContainMultiCurrencies() )
				{
					processedSegments = true;
				}
				
				if( !processedSegments )
					this.ProcessSegments();
			}
			else if( this.IsEquityReport )
			{
				this.IsEquityReport = false;
			}

			if( ReportUtils.IsStatementOfDisclosureCashFlows( this.ReportName ) )
				this.RemoveExtraColumnsFromCashFlows();

			this.CheckRowsAndScrubCurrencyCode();
			this.ProcessRounding( messenger );

			if( !this.HasSegments() || ( !this.IsEquityReport && !isIMBased ) )
			{
				this.ProcessColumnHeaders();

				if( isShowElements )
				{
					this.Columns.ForEach( Remove0MonthsEnded );
					this.SynchronizeGrid();
				}
			}

			this.CleanupDuplicateCurrencyLabel();
			this.RemoveCellsWithoutColumnHeader();
			this.RemoveExtraHeaderRow();

			if( !this.IsEquityReport )
			{
				this.PopulateSimilarColumns();
			}
		}

		public void ApplyAllColumnLabels()
		{
			foreach( InstanceReportColumn column in this.Columns )
			{
				InstanceReport.ApplyColumnLabels( this, column );
			}
		}

		public static bool ApplyColumnLabels( InstanceReport report, InstanceReportColumn localColumn )
		{
			localColumn.Labels.Clear();

			//add segment labels
			if( localColumn.Segments != null )
			{
				foreach( Segment columnSegment in localColumn.Segments )
				{
					List<Segment> members;
					if( !report.AxisMembersByPresentation.TryGetValue( columnSegment.DimensionInfo.dimensionId, out members ) )
						return false;

					Segment member = members.Find( reportSegment => string.Equals( columnSegment.DimensionInfo.Id, reportSegment.DimensionInfo.Id ) );
					if( member == null )
					{
						//this member is not in the presentation, therefore this column should not be on this report.
						return false;
					}

					columnSegment.ValueName = member.ValueName;
					string memberKey = string.Format( "{0}:{1}", member.DimensionInfo.dimensionId, member.DimensionInfo.Id );
					localColumn.Labels.Add( new LabelLine( localColumn.Labels.Count, member.ValueName, memberKey ) );
				}
			}


			//add calendar label
			localColumn.Labels.Add( new LabelLine( localColumn.Labels.Count, localColumn.GetReportingPeriodString(), "Calendar" ) );


			//get the currency code
			localColumn.CurrencyCode = InstanceUtils.GetCurrencyCodeFromMCU( localColumn.MCU );
			if( !string.IsNullOrEmpty( localColumn.CurrencyCode ) )
			{
				//get the currency symbol
				localColumn.CurrencySymbol = InstanceUtils.GetCurrencySymbolFromCode( localColumn.CurrencyCode );

				string currencyLabel;
				if( string.IsNullOrEmpty( localColumn.CurrencySymbol ) )
					currencyLabel = localColumn.CurrencyCode;
				else
					currencyLabel = string.Format( "{0} ({1})", localColumn.CurrencyCode, localColumn.CurrencySymbol );

				//add unit label
				localColumn.Labels.Add( new LabelLine( localColumn.Labels.Count, currencyLabel, localColumn.Units[ 0 ].UnitID ) );
			}

			return true;
		}

		private void ApplyCustomUnits()
		{
			this.SynchronizeGrid();

			List<InstanceReportRow> otherRows = this.Rows.FindAll( row => row.Unit > UnitType.StandardUnits );
			if( otherRows.Count == 0 )
				return;

			List<InstanceReportColumn> customColumns = this.Columns.FindAll( col => col.HasCustomUnits() );
			if( customColumns.Count == 0 )
				return;

			foreach( InstanceReportColumn col in customColumns )
			{
				int maxLabelId = 0;
				col.Labels.ForEach( ll => maxLabelId = Math.Max( ll.Id, maxLabelId ) );

				foreach( InstanceReportRow row in otherRows )
				{
					bool showCustomUnits = ReportUtils.Exists( row.Cells, c => c.Id == col.Id && c.HasData );
					if( !showCustomUnits )
						continue;

					//we've already affirmed that these exist
					List<UnitProperty> ups = col.Units.FindAll( InstanceReportColumn.IsCustomUnit );
					foreach( UnitProperty up in ups )
					{
						string unitLabel = up.StandardMeasure.MeasureValue;
						bool labelExists = ReportUtils.Exists( col.Labels, ll => string.Equals( ll.Label, unitLabel ) );
						if( labelExists )
							continue;

						maxLabelId++;
						col.Labels.Add( new LabelLine( maxLabelId, unitLabel ) );
					}
					break;
				}
			}
		}

		private void Remove0MonthsEnded( InstanceReportColumn col )
		{
			col.Labels.RemoveAll( lbl => lbl.Label == "0 Months Ended" );
		}

		/// <summary>
		/// Finds similar columns (peers) and populates the blanks of non-monetary rows if a populated cell exists
		/// </summary>
		private Dictionary<string, List<int>> PopulateSimilarColumns()
		{
			//Now that this report is "clean" check for duplicate contiguous columns
			//If we find them, copy non-monetary values over
			List<InstanceReportRow> nonMonetaryRows = this.Rows.FindAll( row => ( row.Unit & UnitType.Monetary ) != UnitType.Monetary );

			//<string context key, List<int> column.Id>
			Dictionary<string, List<int>> peerColumns = new Dictionary<string, List<int>>();
			foreach( InstanceReportColumn priCol in this.Columns )
			{
				//get all of our "similar" columns using the date, segments, and scenarios
				//DO NOT COMPARE UNITS
				string priKey = priCol.MCU.BuildDateAndSegmentKey();
				List<InstanceReportColumn> peers = this.Columns.FindAll(
					( column ) => string.Equals( priKey, column.MCU.BuildDateAndSegmentKey() ) );

				//now that we have the columns, do we have more than 1 that match?
				if( peers.Count > 1 )
					peerColumns[ priKey ] = peers.ConvertAll<int>( ( peer ) => ( peer.Id ) );
			}

			//if no groupings were created, there's no work to do
			if( peerColumns.Count == 0 )
				return peerColumns;

			//generate a key column of who has the values for each peer group
			//use this key to populate other cells
			foreach( InstanceReportRow row in nonMonetaryRows )
			{
				foreach( string key in peerColumns.Keys )
				{
					int fromIndex = -1;
					foreach( int colID in peerColumns[ key ] )
					{
						fromIndex = row.Cells.FindIndex( c => c.Id == colID && c.HasData );
						if( fromIndex > -1 )
							break;
					}

					//if we didn't find a valid index, let's just wait, and not apply any cells
					if( fromIndex == -1 )
						continue;

					foreach( int toID in peerColumns[ key ] )
					{
						int toIndex = row.Cells.FindIndex( c => c.Id == toID );
						if( toIndex < 0 )
							continue;

						if( toIndex == fromIndex )
							continue;

						if( row.Cells[ toIndex ].HasData )
							continue;
							
						row.Cells[ toIndex ].AddData( row.Cells[ fromIndex ] );
					}
				}
			}

			return peerColumns;
		}


		public string[] GetUniqueInUseElements()
		{
			Dictionary<string, int> inUseElements = new Dictionary<string, int>();

			foreach( InstanceReportRow irr in this.Rows )
			{
				if( irr.ElementName == null )
					continue;

				if( irr.ElementName.Trim().Length == 0 )
					continue;

				if( inUseElements.ContainsKey( irr.ElementName ) )
					continue;

				//check to make sure we have some data for this row before adding the element.
				foreach( Cell c in irr.Cells )
				{
					if( c.HasData )
					{
						inUseElements[ irr.ElementName ] = 1;
						break;
					}
				}
			}

			string[] elements = new string[ inUseElements.Count ];
			inUseElements.Keys.CopyTo( elements, 0 );
			return elements;
		}

		public string[] GetUniqueInUseSegments()
		{
			Dictionary<string, int> inUseSegments = new Dictionary<string, int>();

			foreach( InstanceReportColumn col in this.Columns )
			{
				Dictionary<string, int> tmpSegments = this.GetUniqueInUseSegments( col );
				foreach( string key in tmpSegments.Keys )
				{
					if( !inUseSegments.ContainsKey( key ) )
						inUseSegments[ key ] = 1;
				}
			}

			string[] segments = new string[ inUseSegments.Count ];
			inUseSegments.Keys.CopyTo( segments, 0 );
			return segments;
		}

		public Dictionary<string, int> GetUniqueInUseSegments( InstanceReportColumn col )
		{
			if( !this.HasSegmentsOnRows() )
				return col.GetUniqueInUseSegments();

			int colIndex = this.Columns.IndexOf( col );
			InstanceReportColumn lastSegmentColumn = null;
			Dictionary<string, int> inUseSegments = new Dictionary<string, int>();

			foreach( InstanceReportRow row in this.Rows )
			{
				if( row.OriginalInstanceReportColumn != null )
					lastSegmentColumn = row.OriginalInstanceReportColumn;

				if( row.Cells[ colIndex ].HasData )
				{
					Dictionary<string, int> tmpSegments = lastSegmentColumn.GetUniqueInUseSegments();
					foreach( string key in tmpSegments.Keys )
					{
						if( !inUseSegments.ContainsKey( key ) )
							inUseSegments[ key ] = 1;
					}
				}
			}

			return inUseSegments;
		}

		private void CheckRowsAndScrubCurrencyCode()
		{
			this.SynchronizeGrid();
			List<InstanceReportRow> monetaryRows = this.Rows.FindAll( row => ( row.Unit & UnitType.Monetary ) == UnitType.Monetary );

			for( int col = 0; col < this.Columns.Count; col++ )
			{
				InstanceReportColumn column = this.Columns[ col ];
				if( column == null )
					continue;

				if( string.IsNullOrEmpty( column.CurrencyCode ) )
					continue;

				bool colHasMonetary = false;
				foreach( InstanceReportRow row in monetaryRows )
				{
					if( row.Cells[ col ].HasData )
					{
						colHasMonetary = true;
						break;
					}
				}

				if( !colHasMonetary )
				{
					column.RemoveCurrencyLabel();
					column.CurrencyCode = string.Empty;
					column.CurrencySymbol = string.Empty;
				}
			}
		}

		private bool ContainMultiCurrencies()
		{
			List<string> Currencies = new List<string>( 0 );
			foreach( InstanceReportColumn irc in this.Columns )
			{
				if( irc.Units != null && irc.Units.Count > 0 )
				{
					foreach( UnitProperty up in irc.Units )
					{
						if( up.StandardMeasure != null && up.StandardMeasure.MeasureSchema.IndexOf( "iso4217", StringComparison.CurrentCultureIgnoreCase ) >= 0 )
						{
							if( Currencies.BinarySearch( up.StandardMeasure.MeasureValue ) < 0 )
							{
								Currencies.Add( up.StandardMeasure.MeasureValue );
								Currencies.Sort();
							}
						}
						//per share
						else if( up.NumeratorMeasure != null && up.NumeratorMeasure.MeasureSchema.IndexOf( "iso4217", StringComparison.CurrentCultureIgnoreCase ) >= 0 )
						{
							if( Currencies.BinarySearch( up.NumeratorMeasure.MeasureValue ) < 0 )
							{
								Currencies.Add( up.NumeratorMeasure.MeasureValue );
								Currencies.Sort();
							}
						}
					}


				}
			}

			return ( Currencies.Count > 1 );
		}

		/// <summary>
		/// Clean-up method for the Cash Flows statement
		/// </summary>
		private void RemoveExtraColumnsFromCashFlows()
		{
			//get the largest duration - instant columns skew this value
			int daysInPeriod = 0;
			foreach( InstanceReportColumn c in this.Columns )
			{
				if( c.ReportingSpan.Days > daysInPeriod )
					daysInPeriod = c.ReportingSpan.Days;
			}


			int minRowsRequired = this.Rows.FindAll( r => !r.IsAbstractGroupTitle ).Count / 4;
			List<int> colToDelete = new List<int>( 0 );
			for( int cIndex = this.Columns.Count - 1; cIndex >= 0; cIndex-- )
			{
				//if this duration is a "month" smaller than the largest duration
				InstanceReportColumn c = this.Columns[ cIndex ] as InstanceReportColumn;
				if( daysInPeriod - 20 >= c.ReportingSpan.Days )
				{
					int cellsWithValue = 0;
					foreach( InstanceReportRow r in this.Rows )
					{
						//this will probably miss nils
						if( ( r.Cells[ cIndex ] as Cell ).IsNumeric )
							cellsWithValue += 1;
					}

					if( cellsWithValue < minRowsRequired )
						colToDelete.Add( cIndex );
				}
			}

			if( colToDelete.Count > 0 )
			{
				foreach( int index in colToDelete )
				{
					this.Columns.RemoveAt( index );
				}

				foreach( InstanceReportRow ir in this.Rows )
				{
					foreach( int index in colToDelete )
					{
						try
						{
							ir.Cells.RemoveAt( index );
						}
						catch { }
					}
				}

				this.SynchronizeGrid();
			}
		}

		private void RemoveExtraHeaderRow()
		{
			if( this.Rows.Count == 0 )
				return;

			InstanceReportRow firstRow = this.Rows[ 0 ];
			if( firstRow.IsReportTitle || ( firstRow.IsSegmentTitle && this.ReportName.IndexOf( firstRow.Label ) >= 0 ) )
			{
				this.Rows.RemoveAt( 0 );
				this.SynchronizeGrid();
			}
		}

		private void ProcessCurrencySymbolRule()
		{
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.CURRENCY_SYMBOL_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				builderRules.ProcessRule( RulesEngineUtils.CURRENCY_SYMBOL_RULE, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.CURRENCY_SYMBOL_RULE );
			}
		}

		public void AssignCurrencySymbol()
		{
			//DE1523 indicates we should not show currency symbol for ExchangeRate
			//DE1516 and 1545 are fixed with this as well. All 3 defects have been tested here. 

			InstanceReportRow firstRow = null;
			InstanceReportRow lastRow = null;

			//we now have 1 loop instead of 3
			//we're going to cycle through all of the rows:
			//update all EPS, record first & last monetary rows
			foreach( InstanceReportRow thisRow in this.Rows )
			{
				if( thisRow.IsEmpty() )
					continue;

				//this is cheaper - do it first
				if( thisRow.IsEPS )
				{
					this.ToggleShowCurrency( thisRow );
					continue;
				}

				if( (int)thisRow.Unit == (int)UnitType.Monetary )
				{
					//record the first monetary row
					if( firstRow == null )
						firstRow = thisRow;

					//every row thereafter is "the last row"
					else
						lastRow = thisRow;
				}
			}

			this.ToggleShowCurrency( firstRow );
			if( firstRow != lastRow )
				this.ToggleShowCurrency( lastRow );
		}

		private void ToggleShowCurrency( InstanceReportRow thisRow )
		{
			if( thisRow == null )
				return;

			for( int c = 0; c < thisRow.Cells.Count; c++ )
			{
				//this probably does not work for nils
				//but that may not matter either, because the field will display as blank
				Cell cell = thisRow.Cells[ c ] as Cell;
				if( !cell.IsNumeric )
				{
					cell.ShowCurrencySymbol = false;
					continue;
				}

				if( !string.IsNullOrEmpty( this.Columns[ c ].CurrencyCode ) )
				{
					cell.CurrencyCode = this.Columns[ c ].CurrencyCode;
					cell.CurrencySymbol = this.Columns[ c ].CurrencySymbol;
				}

				cell.ShowCurrencySymbol = true;
			}
		}

		protected void RemoveCellsWithoutColumnHeader()
		{
			int lastColumn = this.Columns.Count;

			foreach( InstanceReportRow r in this.Rows )
			{
				for( int cellIndex = r.Cells.Count - 1; cellIndex >= 0; cellIndex-- )
				{
					if( cellIndex >= lastColumn )
					{
						r.Cells.RemoveAt( cellIndex );
					}
				}
			}
		}

		private void RemoveUnreferencedFootnotes()
		{
			if( this.Footnotes == null || this.Footnotes.Count == 0 )
				return;

			int nextID = 1;
			bool hasFootnotes = false;
			Dictionary<string, int> sortedFootnoteIndexes = new Dictionary<string, int>( 0 );
			foreach( InstanceReportRow irr in this.Rows )
			{
				if( !string.IsNullOrEmpty( irr.FootnoteIndexer ) )
				{
					hasFootnotes = true;
					string[] indexParts = GetFootnoteIndexers( irr.FootnoteIndexer );
					foreach( string indexPart in indexParts )
					{
						if( !sortedFootnoteIndexes.ContainsKey( indexPart ) )
							sortedFootnoteIndexes[ indexPart ] = nextID++;
					}
				}

				foreach( Cell c in irr.Cells )
				{
					if( !string.IsNullOrEmpty( c.FootnoteIndexer ) )
					{
						hasFootnotes = true;
						string[] indexParts = GetFootnoteIndexers( c.FootnoteIndexer );
						foreach( string indexPart in indexParts )
						{
							if( !sortedFootnoteIndexes.ContainsKey( indexPart ) )
								sortedFootnoteIndexes[ indexPart ] = nextID++;
						}
					}
				}
			}

			if( !hasFootnotes )
			{
				this.Footnotes = new List<Footnote>();
				return;
			}

			bool needToReSequenceFootnoteIndex = false;
			for( int i = 0; i < this.Footnotes.Count; )
			{
				int footnoteID = this.Footnotes[ i ].NoteId;
				if( sortedFootnoteIndexes.ContainsKey( "[" + footnoteID.ToString() + "]" ) )
				{
					i++;
				}
				else
				{
					this.Footnotes.RemoveAt( i );
					needToReSequenceFootnoteIndex = true;
				}
			}


			if( !needToReSequenceFootnoteIndex )
			{
				foreach( string key in sortedFootnoteIndexes.Keys )
				{
					string keyNum = key.Replace( "[", "" ).Replace( "]", "" );
					int keyValue = int.Parse( keyNum );
					if( keyValue != sortedFootnoteIndexes[ key ] )
					{
						needToReSequenceFootnoteIndex = true;
						break;
					}
				}
			}

			if( needToReSequenceFootnoteIndex )
			{
				for( int index = 0; index < this.Footnotes.Count; index++ )
				{
					Footnote fn = this.Footnotes[ index ];
					fn.NoteId = sortedFootnoteIndexes[ string.Format( @"[{0}]", fn.NoteId ) ];
				}

				this.Footnotes.Sort();
			}

			foreach( InstanceReportRow row in this.Rows )
			{
				if( !string.IsNullOrEmpty( row.FootnoteIndexer ) )
				{
					string[] indexParts = GetFootnoteIndexers( row.FootnoteIndexer );
					row.FootnoteIndexer = RebuildFootnoteIndexers( indexParts, sortedFootnoteIndexes );
				}

				foreach( Cell c in row.Cells )
				{
					if( !string.IsNullOrEmpty( c.FootnoteIndexer ) )
					{
						string[] indexParts = GetFootnoteIndexers( c.FootnoteIndexer );
						c.FootnoteIndexer = RebuildFootnoteIndexers( indexParts, sortedFootnoteIndexes );
					}
				}
			}
		}

		private static string[] GetFootnoteIndexers( string indexer )
		{
			string[] indexParts = new string[0];

			//Once upon a time, we got "[1],,[3]"
			//I am unsure why, but let's protect the scenario
			while( indexer.Contains( ",," ) )
			{
				indexer = indexer.Replace( ",,", "," );
			}

			if( indexer.Contains( "][" ) )
				indexer = indexer.Replace( "][", "],[" );

			//single index
			if( indexer.Contains( "," ) ) 
				indexParts = indexer.Split( ',' );
			else
				indexParts = new string[ 1 ] { indexer };

			return indexParts;
		}

		private static string RebuildFootnoteIndexers( string[] indexParts, Dictionary<string, int> sortedFootnoteIndexes )
		{
			if( indexParts == null || indexParts.Length == 0 )
				return string.Empty;

			for( int i = 0; i < indexParts.Length; i++ )
			{
				//convert the current footnote ID to its sorted index
				if( sortedFootnoteIndexes.ContainsKey( indexParts[ i ] ) )
					indexParts[ i ] = "[" + sortedFootnoteIndexes[ indexParts[ i ] ].ToString() + "]";
			}

			//resort the indexes
			Array.Sort( indexParts );
			string indexer = string.Join( ",", indexParts );
			return indexer;
		}

		protected void ResequenceRows()
		{
			int rowId = 1;
			foreach( InstanceReportRow irr in Rows )
			{
				irr.Id = rowId++;
			}
		}

		public void RemoveColumn( int columnIndex )
		{
			foreach( InstanceReportRow row in this.Rows )
			{
				row.Cells.RemoveAt( columnIndex );
			}

			this.Columns.RemoveAt( columnIndex );
		}

		public void RemoveColumnByID( int columnID )
		{
			this.Columns.RemoveAll( col => col.Id == columnID );
			for( int r = 0; r < this.Rows.Count; r++ )
			{
				InstanceReportRow row = this.Rows[ r ];
				row.Cells.RemoveAll( cell => cell.Id == columnID );
			}
		}

		public void RemoveEmptyColumns()
		{
			List<int> columnsToRemove = new List<int>( this.Columns.Count );
			for( int colIndex = 0; colIndex < this.Columns.Count; colIndex++ )
			{
				if( this.IsColumnEmpty( colIndex ) )
					columnsToRemove.Add( colIndex );
			}

			//right to left is better for this
			columnsToRemove.Reverse();
			columnsToRemove.ForEach( idx => this.RemoveColumn( idx ) );
			this.SynchronizeGrid();
		}

		public bool IsColumnEmpty( int colIndex )
		{
			foreach( InstanceReportRow irr in this.Rows )
			{
				if( ( irr.Cells[ colIndex ] as Cell ).HasData )
					return false;
			}

			return true;
		}

		private int CountColumnPopulatedElements( int colIndex )
		{
			Dictionary<string, bool> columnPopulatedElements = new Dictionary<string, bool>();

			foreach( InstanceReportRow irr in this.Rows )
			{
				if( ( irr.Cells[ colIndex ] as Cell ).HasData )
					columnPopulatedElements[ irr.ElementName ] = true;
			}

			return columnPopulatedElements.Count;
		}

		//Remove empty rows and possible segment header row
		public void RemoveEmptyRows()
		{
			for( int rowIndex = this.Rows.Count - 1; rowIndex >= 0; rowIndex-- )
			{
				InstanceReportRow thisRow = this.Rows[ rowIndex ] as InstanceReportRow;
				bool rowHasData = false;
				if( thisRow.IsSegmentTitle || thisRow.IsReportTitle || thisRow.IsAbstractGroupTitle )
				{
					if( rowIndex == this.Rows.Count - 1 ) //last row, empty segment header
					{
						this.Rows.RemoveAt( rowIndex );
					}
					else //check if this segment has no data, if so, remove the segment title
					{
						InstanceReportRow nextRow = this.Rows[ rowIndex + 1 ] as InstanceReportRow;
						if( ( thisRow.IsSegmentTitle && nextRow.IsSegmentTitle ) ||
							( thisRow.IsAbstractGroupTitle && ( nextRow.IsAbstractGroupTitle || nextRow.IsSegmentTitle ) ) )
						{
							this.Rows.RemoveAt( rowIndex );
						}
					}
				}
				else if( this.IsEquityReport && ( thisRow.IsBeginningBalance || thisRow.IsEndingBalance ) )
				{
					continue;
				}
				else
				{
					foreach( Cell c in thisRow.Cells )
					{
						if( c.HasData )
						{
							rowHasData = true;
							break;
						}
					}
					if( !rowHasData )
					{
						this.Rows.RemoveAt( rowIndex );
					}
				}
			}

			this.SynchronizeGrid();
		}

		private void RebuildColumnLabels()
		{
			if( !ContainsNumericRows() )
			{
				foreach( InstanceReportColumn col in this.Columns )
				{
					for( int index = col.Labels.Count - 1; index >= 0; index-- )
					{
						LabelLine ll = col.Labels[ index ] as LabelLine;
						if( ll.Label.ToLower().IndexOf( "usd" ) >= 0 || ( ll.Label.ToLower().IndexOf( "/" ) >= 0 && ll.Label.ToLower().IndexOf( "shares" ) >= 0 ) )
						{
							col.Labels.RemoveAt( index );
						}
					}
				}
			}
		}

		public void BuildXMLDocument( string fileName, bool finalProcessingRequired, bool isIMBased, ITraceMessenger messenger )
		{
			this.SynchronizeGrid();

			if( finalProcessingRequired )
				this.FinalProcessing( isIMBased, messenger );

			if( !this.IsEmbedReport )
			{
				this.RemoveUnreferencedFootnotes();
				this.RemoveEmptyRows();

				if( finalProcessingRequired )
					this.RebuildColumnLabels();
				
				this.ProcessCurrencySymbolRule();
				this.RemoveLabelColumn();
			}

			if( this.Columns.Count > 0 && this.Rows.Count > 0 )
				this.SaveAsXml( fileName );
		}

		public bool CheckForEmbeddedReports()
		{
			if( this.Columns == null || this.Columns.Count == 0 )
				return false;

			if( this.Rows == null || this.Rows.Count == 0 )
				return false;

			return ReportUtils.Exists( this.Rows, row => ReportUtils.Exists( row.Cells, cell => cell.HasEmbeddedReport ) );
		}

		private static void RepairLabelEntities( string fileName )
		{
			FixFileBetween( fileName, "<CurrencySymbol>", "</CurrencySymbol>" );
			FixFileBetween( fileName, "<Label>", "</Label>" );
			FixFileBetween( fileName, "<ReportName>", "</ReportName>" );
			FixFileBetween( fileName, " Label=\"", "\" " );
		}

		private static void FixFileBetween( string fileName, string elementStart, string elementEnd )
		{
			bool changesMade = false;
			string tmpPath = fileName + ".tmp";
			using( StreamWriter writer = new StreamWriter( tmpPath, false, InstanceReport.Encoding ) )
			{
				using( StreamReader reader = new StreamReader( fileName ) )
				{
					int needStart = 1;
					string buffer = string.Empty;
					while( !reader.EndOfStream )
					{
						buffer += FixFileRead( reader ); ;

						switch( needStart )
						{
							case 0:
								int endAt = buffer.IndexOf( elementEnd );
								if( endAt == -1 )
									break;

								needStart = 1;
								string content = buffer.Substring( 0, endAt );
								buffer = buffer.Substring( endAt );
								if( content.Contains( "&amp;#" ) )
								{
									changesMade = true;
									string newContent = RepairLabel( content );
									writer.Write( newContent );
								}
								else
								{
									writer.Write( content );
								}

								goto case 1;
							case 1:
								int startAt = buffer.IndexOf( elementStart );
								if( startAt == -1 )
								{
									if( buffer.Length > elementStart.Length )
										startAt = buffer.Length - elementStart.Length;
									else
										startAt = 0;
								}
								else
								{
									startAt += elementStart.Length;
									needStart = 0;
								}

								string write = buffer.Substring( 0, startAt );
								buffer = buffer.Substring( startAt );
								writer.Write( write );
								startAt = 0;

								if( needStart == 0 )
									goto case 0;
								else
									break;
						}
					}

					writer.Write( buffer );
				}
			}

			if( changesMade )
			{
				File.Delete( fileName );
				File.Move( tmpPath, fileName );
			}
			else
			{
				File.Delete( tmpPath );
			}
		}

		private static string FixFileRead( StreamReader reader )
		{
			char[] chars = new char[ 1024 ];
			int charsRead = reader.Read( chars, 0, chars.Length );

			string text = new string( chars );
			if( charsRead < chars.Length )
				text = text.Substring( 0, charsRead );

			return text;
		}

		private static string RepairLabel( string label )
		{
			return label.Replace( "&amp;#", "&#" );
		}

		private static BinaryFormatter binSerializer = new BinaryFormatter();
		public static InstanceReport LoadBinary( string fileName )
		{
			using( FileStream reader = File.Open( fileName, FileMode.Open, FileAccess.Read ) )
			{
				InstanceReport report = binSerializer.Deserialize( reader ) as InstanceReport;
				return report;
			}
		}

		public void SaveAsBinary( string fileName )
		{
			using( FileStream fstream = new FileStream( fileName, FileMode.Create, FileAccess.Write ) )
			{
				binSerializer.Serialize( fstream, this );
			}
		}

		private static XmlSerializer xmlSerializer = new XmlSerializer( typeof( InstanceReport ) );
		public static InstanceReport LoadXml( string fileName )
		{
			using( FileStream fs = new FileStream( fileName, FileMode.Open, FileAccess.Read ) )
			{
				return LoadXmlStream( fs );
			}
		}

		public static InstanceReport LoadXmlStream( Stream input )
		{
			using( XmlTextReader reader = new XmlTextReader( input ) )
			{
				InstanceReport report = xmlSerializer.Deserialize( reader ) as InstanceReport;
				return report;
			}
		}

		public void SaveAsXml( string fileName )
		{
			this.SynchronizeGrid();
			using( FileStream fstream = File.Open( fileName, FileMode.Create, FileAccess.Write ) )
			{

				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Encoding = InstanceReport.Encoding;

#if DEBUG
				settings.Indent = true;
#endif

				using( XmlWriter writer = new RivetXmlWriter( fstream, settings ) )
				{
					xmlSerializer.Serialize( writer, this );
				}
			}

			if( this.IsMultiCurrency )
				InstanceReport.RepairLabelEntities( fileName );
		}

		#endregion


		#region Stockholders Equity

		public void CleanupFlowThroughTotalEquityRows( Dictionary<string, int> htNonUniqueElements )
		{
			this.CleanupFlowThroughTotalEquityRows( ref this.Rows, htNonUniqueElements );
		}

		private List<int> GetTotalColumnIndexes()
		{
			//Move the "Total" Column and the associated cells to the end of the report
			List<int> totalColumnIndexes = new List<int>();
			for( int i = 0; i < this.Columns.Count; i++ )
			{
				InstanceReportColumn irc = this.Columns[ i ];
				string segmentString = irc.GetSegmentsString( false, false, false, false, false, this.ReportName );
				if( segmentString == string.Empty )
				{
					totalColumnIndexes.Add( i );
				}
			}

			if( totalColumnIndexes.Count > 0 )
			{
				totalColumnIndexes.Sort();
			}

			return totalColumnIndexes;
		}

		private void CleanupFlowThroughTotalEquityRows( ref List<InstanceReportRow> rowCollection, Dictionary<string, int> htNonUniqueElements )
		{
			List<int> totalColumns = GetTotalColumnIndexes();

			for( int rowIndex = rowCollection.Count - 1; rowIndex >= 0; rowIndex-- )
			{
				InstanceReportRow thisRow = rowCollection[ rowIndex ];
				if( thisRow.IsBeginningBalance ||
					thisRow.IsEndingBalance ||
					thisRow.IsAbstractGroupTitle )
				{
					//Abstract rows, and ending balance rows should
					//not be remvoed for now
					continue;
				}
				else
				{
					bool nonTotalCellHasDara = false;
					bool totalCellHasData = false;
					for( int colIndex = 0; colIndex <= thisRow.Cells.Count - 1; colIndex++ )
					{
						Cell thisCell = thisRow.Cells[ colIndex ] as Cell;

						if( thisCell.HasData )
						{
							if( totalColumns.Contains( colIndex ) )
							{
								totalCellHasData = true;
							}
							else
							{
								nonTotalCellHasDara = true;
							}
						}
					}

					if( totalCellHasData && !nonTotalCellHasDara ) //Only total has data
					{
						if( htNonUniqueElements.ContainsKey( thisRow.ElementName ) )
						{
							rowCollection.RemoveAt( rowIndex );
						}
					}
				}
			}
		}

		#endregion

		private void CleanupDuplicateCurrencyLabel()
		{
			foreach( InstanceReportColumn irc in this.Columns )
			{
				List<int> labelLineToRemove = new List<int>( 0 );

				for( int index = irc.Labels.Count - 1; index > 0; index-- )
				{
					LabelLine ll = irc.Labels[ index ];
					string myLabel = ll.Label;
					for( int indexCheck = 0; indexCheck < irc.Labels.Count; indexCheck++ )
					{
						if( indexCheck == index ) continue;
						bool skipAlreadyRemoved = false;
						foreach( int removeIndex in labelLineToRemove )
						{
							if( indexCheck == removeIndex )
							{
								skipAlreadyRemoved = true;
								break;
							}
						}

						if( skipAlreadyRemoved ) continue;

						LabelLine llCheck = irc.Labels[ indexCheck ];
						string myLabelCheck = llCheck.Label;
						if( myLabelCheck.ToLower() == myLabel.ToLower() )
						{
							labelLineToRemove.Add( index );
							break;
						}
					}

				}

				if( labelLineToRemove.Count > 0 )
				{
					foreach( int index in labelLineToRemove )
					{
						irc.Labels.RemoveAt( index );
					}
				}

			}
		}

		#region promote shared labels/Footnotes

		public void PromoteFootnotes()
		{
			foreach( InstanceReportRow irr in this.Rows )
			{
				string sharedFootnoteIndex = string.Empty;
				List<string> lstSharedFootnotes = new List<string>( 0 );

				#region Find shared footnote(s)

				Dictionary<int, ArrayList> cellFootnotes = new Dictionary<int, ArrayList>( 0 );

				foreach( Cell c in irr.Cells )
				{
					if( !String.IsNullOrEmpty( c.FootnoteIndexer ) && c.HasData )
					{
						cellFootnotes[ c.Id ] = new ArrayList( 0 );
						string[] parts = c.FootnoteIndexer.Split( ',' ) as string[];
						foreach( string part in parts )
						{
							( cellFootnotes[ c.Id ] as ArrayList ).Add( part );
						}
					}
				}

				if( cellFootnotes.Count == 0 ) continue;

				foreach( int id in cellFootnotes.Keys )
				{
					ArrayList lst = cellFootnotes[ id ] as ArrayList;
					foreach( string footnote in lst )
					{
						bool inOtherCells = true;
						foreach( Cell otherCell in irr.Cells )
						{
							if( otherCell.HasData )
							{
								if( cellFootnotes.ContainsKey( otherCell.Id ) )
								{
									ArrayList lstCompare = cellFootnotes[ otherCell.Id ] as ArrayList;
									if( lstCompare.BinarySearch( footnote ) < 0 )
									{
										inOtherCells = false;
										break;
									}
								}
								else
								{
									inOtherCells = false;
									break;
								}
							}
						}
						if( inOtherCells )
						{
							lstSharedFootnotes.Add( footnote );
						}
					}
					break;//only need to check the first group against other groups
				}

				if( lstSharedFootnotes.Count > 0 )
				{
					StringBuilder sb = new StringBuilder();
					for( int index = 0; index < lstSharedFootnotes.Count; index++ )
					{
						if( index == 0 )
						{
							sb.Append( lstSharedFootnotes[ index ] );
						}
						else
						{
							sb.Append( "," ).Append( lstSharedFootnotes[ index ] );
						}
					}
					sharedFootnoteIndex = sb.ToString();
				}

				#endregion

				#region Apply shared footnote and clean up cell level footnotes

				if( !String.IsNullOrEmpty( sharedFootnoteIndex ) )
				{
					irr.FootnoteIndexer = sharedFootnoteIndex;
					foreach( Cell c in irr.Cells )
					{
						if( !String.IsNullOrEmpty( c.FootnoteIndexer ) )
						{
							foreach( string oneNote in lstSharedFootnotes )
							{
								c.FootnoteIndexer = c.FootnoteIndexer.Replace( oneNote, "" );
							}
							int openBracketPos = c.FootnoteIndexer.IndexOf( "[" );
							int closBracketPos = c.FootnoteIndexer.LastIndexOf( "]" );
							if( openBracketPos >= 0 && closBracketPos >= 0 )
							{
								c.FootnoteIndexer = c.FootnoteIndexer.Substring( openBracketPos, closBracketPos - openBracketPos + 1 );
							}
							else
							{
								c.FootnoteIndexer = "";
							}
						}
					}
				}

				#endregion
			}

			foreach( InstanceReportColumn irc in this.Columns )
			{
				string sharedFootnoteIndex = string.Empty;
				List<string> lstSharedFootnotes = new List<string>( 0 );

				#region Find shared footnote(s)

				Dictionary<int, ArrayList> cellFootnotes = new Dictionary<int, ArrayList>( 0 );

				foreach( InstanceReportRow irr in this.Rows )
				{
					Cell c = irr.Cells.Find( tc => tc.Id == irc.Id );
					if( !String.IsNullOrEmpty( c.FootnoteIndexer ) && c.HasData )
					{
						cellFootnotes[ irr.Id ] = new ArrayList( 0 );
						string[] parts = c.FootnoteIndexer.Split( ',' ) as string[];
						foreach( string part in parts )
						{
							( cellFootnotes[ irr.Id ] as ArrayList ).Add( part );
						}
					}
				}

				if( cellFootnotes.Count == 0 ) continue;

				foreach( int id in cellFootnotes.Keys )
				{
					//InstanceReportRow irr = this.Rows[id - 1] as InstanceReportRow;
					ArrayList lst = cellFootnotes[ id ] as ArrayList;
					foreach( string footnote in lst )
					{
						bool inOtherCells = true;
						foreach( InstanceReportRow compareRow in this.Rows )
						{
							if( compareRow.Id != id )
							{
								Cell otherCell = compareRow.Cells.Find( cell => cell.Id == irc.Id );
								if( otherCell.HasData )
								{
									if( cellFootnotes.ContainsKey( compareRow.Id ) )
									{
										ArrayList lstCompare = cellFootnotes[ compareRow.Id ] as ArrayList;
										if( lstCompare.BinarySearch( footnote ) < 0 )
										{
											inOtherCells = false;
											break;
										}
									}
									else
									{
										inOtherCells = false;
										break;
									}
								}
							}
						}
						if( inOtherCells )
						{
							lstSharedFootnotes.Add( footnote );
						}
					}
					break;//only need to check the first group against other groups
				}

				if( lstSharedFootnotes.Count > 0 )
				{
					StringBuilder sb = new StringBuilder();
					for( int index = 0; index < lstSharedFootnotes.Count; index++ )
					{
						if( index == 0 )
						{
							sb.Append( lstSharedFootnotes[ index ] );
						}
						else
						{
							sb.Append( "," ).Append( lstSharedFootnotes[ index ] );
						}
					}
					sharedFootnoteIndex = sb.ToString();
				}

				#endregion

				#region Apply shared footnote and clean up cell level footnotes

				if( !String.IsNullOrEmpty( sharedFootnoteIndex ) )
				{
					irc.FootnoteIndexer = sharedFootnoteIndex;
					foreach( InstanceReportRow irr in this.Rows )
					{
						Cell c = irr.Cells.Find( cell => cell.Id == irc.Id );
						if( !String.IsNullOrEmpty( c.FootnoteIndexer ) )
						{
							foreach( string oneNote in lstSharedFootnotes )
							{
								c.FootnoteIndexer = c.FootnoteIndexer.Replace( oneNote, "" );
							}
							int openBracketPos = c.FootnoteIndexer.IndexOf( "[" );
							int closBracketPos = c.FootnoteIndexer.LastIndexOf( "]" );
							if( openBracketPos >= 0 && closBracketPos >= 0 )
							{
								c.FootnoteIndexer = c.FootnoteIndexer.Substring( openBracketPos, closBracketPos - openBracketPos + 1 );
							}
							else
							{
								c.FootnoteIndexer = "";
							}
						}
					}
				}

				#endregion
			}
		}

		private bool ContainsNumericRows()
		{
			foreach( InstanceReportRow row in this.Rows )
			{
				if( IsNumericRow( row ) )
					return true;
			}
			return false;
		}

		private bool IsNumericRow( InstanceReportRow row )
		{
			if( row.IsMonetary )
				return true;

			if( row.IsEPS )
				return true;

			if( row.IsShares )
				return true;

			return false;
		}

		public void PromoteSharedColumnLabelsAfterRemoveFlowThroughColumns()
		{
			if( this.Columns.Count == 0 )
			{
				return;
			}

			DateTime resultDate = DateTime.MinValue;
			Dictionary<string, ArrayList> htLables = new Dictionary<string, ArrayList>( 0 );
			foreach( InstanceReportColumn irc in this.Columns )
			{
				foreach( LabelLine ll in irc.Labels )
				{
					if( !DateTime.TryParse( ll.Label, out resultDate ) && ll.Label.ToLower().IndexOf( "month" ) < 0 ) //not a date related label
					{
						if( !htLables.ContainsKey( ll.Label ) )
						{
							htLables.Add( ll.Label, new ArrayList( 0 ) );
						}
						( htLables[ ll.Label ] as ArrayList ).Add( irc.Id );
					}
				}
			}
			string combinedSharedLabels = "";
			foreach( string sharedlabel in htLables.Keys )
			{
				if( ( htLables[ sharedlabel ] as ArrayList ).Count == this.Columns.Count )
				{
					foreach( InstanceReportColumn irc in this.Columns )
					{
						for( int labelIndex = irc.Labels.Count - 1; labelIndex >= 0; labelIndex-- )
						{
							if( irc.Labels[ labelIndex ].Label == sharedlabel )
							{
								irc.Labels.RemoveAt( labelIndex );
							}
						}
					}
					if( combinedSharedLabels.Length > 0 )
					{
						combinedSharedLabels += " ";
					}
					combinedSharedLabels += sharedlabel;
				}
			}


			if( combinedSharedLabels.Length > 0 ) //modify the report header
			{
				int sharedBeginIndex = this.ReportName.IndexOf( "("+ InstanceUtils.USDCurrencyCode );

				if( sharedBeginIndex >= 0 )
				{
					this.ReportName = this.ReportName.Insert( sharedBeginIndex + 1, combinedSharedLabels + ", " );
				}
				else
				{
					this.ReportName += "(" + combinedSharedLabels + ")";
				}
			}
		}

		public void PromoteSharedColumnLabels( bool preserveSharedlabels )
		{

			if( this.Columns.Count == 0 )
			{
				return;
			}


			string sharedInformation = ProcessPromoteSharedLables();
			if( !string.IsNullOrEmpty( sharedInformation ) && sharedInformation.Length > 0 )
			{
				if( preserveSharedlabels )
				{
					this.ReportName += " [" + sharedInformation + "]";
				}
				else
				{
					this.ReportName += " (" + sharedInformation + ")";
				}
			}
		}

		private string ProcessPromoteSharedLables()
		{
			StringWrapper sharedLabel = new StringWrapper();

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.PROMOTE_SHARED_LABELS_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				Dictionary<string, int> segmentsAndScenarios = new Dictionary<string, int>();
				List<string> currencyCodes = new List<string>();


				contextObjects.Add( "InstanceReport", this );
				contextObjects.Add( "Columns", this.Columns );
				contextObjects.Add( "SegmentsAndScenarios", segmentsAndScenarios );
				contextObjects.Add( "CurrencyCodes", currencyCodes );
				contextObjects.Add( "SharedLabel", sharedLabel );

				builderRules.ProcessRule( RulesEngineUtils.PROMOTE_SHARED_LABELS_RULE, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.PROMOTE_SHARED_LABELS_RULE );
			}

			return sharedLabel.Value;
		}

		public Dictionary<string, int> GetSegmentScenarioLabels()
		{
			string currentLabel;
			Dictionary<string, int> segmentsAndScenarios = new Dictionary<string, int>();

			//Determine what segments, scenarios and currencies are in the report and how often they
			//are used.
			foreach( InstanceReportColumn ic in this.Columns )
			{
				// just skip this one, maybe it's a placeholder column
				if( ic.MCU != null )
				{
					foreach( Segment s in ic.MCU.contextRef.Segments )
					{
						currentLabel = InstanceUtils.BuildSegmentScenarioName( s );

						if( !segmentsAndScenarios.ContainsKey( currentLabel ) )
						{
							segmentsAndScenarios[ currentLabel ] = 0;
						}
						segmentsAndScenarios[ currentLabel ] = segmentsAndScenarios[ currentLabel ] + 1;
					}

					foreach( Scenario s in ic.MCU.contextRef.Scenarios )
					{
						currentLabel = InstanceUtils.BuildSegmentScenarioName( s );

						if( !segmentsAndScenarios.ContainsKey( currentLabel ) )
						{
							segmentsAndScenarios[ currentLabel ] = 0;
						}
						segmentsAndScenarios[ currentLabel ] = segmentsAndScenarios[ currentLabel ] + 1;
					}
				}
			}

			return segmentsAndScenarios;
		}

		public List<string> GetCurrencyLabels()
		{
			List<string> currencyCodes = new List<string>();

			if( !this.HasMonetaryRows() )
				return currencyCodes;

			//Determine what segments, scenarios and currencies are in the report and how often they
			//are used.
			foreach( InstanceReportColumn ic in this.Columns )
			{
				if( ic.HasMonetary() || ic.HasExchangeRate() || !String.IsNullOrEmpty( ic.MCU.CurrencyCode ) )
				{
					int binIndex = currencyCodes.BinarySearch( ic.MCU.CurrencyCode );
					if( binIndex < 0 )
					{
						currencyCodes.Insert( ~binIndex, ic.MCU.CurrencyCode );
					}
				}
			}

			return currencyCodes;
		}

		public string GetSharedSegmentsAndScenariosLabel( Dictionary<string, int> segmentsAndScenarios )
		{
			List<string> sharedLabels = new List<string>();
			foreach( string label in segmentsAndScenarios.Keys )
			{
				//If the usage count for a given label is equal to the number of columns then
				//the label is used by all columns, promote it to the shared info and remove it
				//from the column
				if( segmentsAndScenarios[ label ] == this.Columns.Count )
				{
					sharedLabels.Add( label );
					foreach( InstanceReportColumn ic in this.Columns )
					{
						for( int index = ic.Labels.Count - 1; index >= 0; index-- )
						{
							string columnLabel = ( ic.Labels[ index ] as LabelLine ).Label.Trim();
							string cleanedUpLabel = Regex.Replace( label, @"\{.*?\} : ", String.Empty ).Trim();
							if( columnLabel.CompareTo( cleanedUpLabel ) == 0 )
								ic.Labels.RemoveAt( index );
						}
						for( int sIndex = ic.MyContextProperty.Segments.Count - 1; sIndex >= 0; sIndex-- )
						{
							Segment s = ic.MyContextProperty.Segments[ sIndex ] as Segment;
							if( s.ValueName == Regex.Replace( label, @"\{.*?\} : ", String.Empty ).Trim() )
							{
								string key = s.ValueType + ":" + s.ValueName;
								if( !promotedToReportHeaderSegments.ContainsKey( key ) )
								{
									promotedToReportHeaderSegments.Add( key, s );
								}
							}
						}
					}
				}
			}

			//Build the shared information label
			StringBuilder sharedInformationSB = new StringBuilder();
			for( int index = 0; index < sharedLabels.Count; index++ )
			{
				sharedLabels[ index ] = Regex.Replace( sharedLabels[ index ], @"\{.*?\} : ", String.Empty ).Trim();
				sharedInformationSB.Append( sharedLabels[ index ] );
				if( index < sharedLabels.Count - 1 )
					sharedInformationSB.Append( ", " );
			}

			return sharedInformationSB.ToString();
		}

		public string GetSharedCurrencyLabel( List<string> currencyCodes )
		{
			if( currencyCodes.Count != 1 )
				return string.Empty;

			//If there is only once currency on the report then promote the currency label to the
			//shared information label.
			string currencyLabel = currencyCodes[ 0 ].Trim();
			if( !currencyLabel.Contains( "/" ) )
			{
				string symbol = ReportBuilder.GetISOCurrencySymbol( currencyLabel ); // InstanceUtils.GetCurrencySymbolFromCode(currencyLabel); // ReportBuilder.GetISOCurrencySymbol(currencyLabel);
				if( !string.IsNullOrEmpty( symbol ) )
				{
					currencyLabel += " " + symbol;
				}
			}

			RemoveUnitRelatedLabels( currencyCodes );
			return currencyLabel;
		}

		public string ConcatenateSharedLabels( string sharedSegmentsScenariosLabel, string sharedCurrencyLabel )
		{
			string sharedLabel = null;
			if( !string.IsNullOrEmpty( sharedSegmentsScenariosLabel ) &&
				!string.IsNullOrEmpty( sharedCurrencyLabel ) )
			{
				sharedLabel = string.Format( "{0}, {1}", sharedSegmentsScenariosLabel, sharedCurrencyLabel );
			}
			else if( !string.IsNullOrEmpty( sharedSegmentsScenariosLabel ) )
			{
				sharedLabel = sharedSegmentsScenariosLabel;
			}
			else if( !string.IsNullOrEmpty( sharedCurrencyLabel ) )
			{
				sharedLabel = sharedCurrencyLabel;
			}
			return sharedLabel;
		}

		private void RemoveUnitRelatedLabels( List<string> currencyCodes )
		{
			if( currencyCodes == null || currencyCodes.Count == 0 )
				return;

			//only use 1 currency code
			string cc = currencyCodes[ 0 ].ToLower();

			this.Columns.ForEach(
				( col ) =>
				{
					col.Labels.RemoveAll(
						( ll ) =>
						{
							//don't delete EPS
							if( ll.Label.ToLower().Contains( "/ shares" ) )
								return false;

							bool isPure = string.Equals( ll.Label, "pure", StringComparison.CurrentCultureIgnoreCase );
							if( isPure )
								return true;

							bool isShares = string.Equals( ll.Label, "shares", StringComparison.CurrentCultureIgnoreCase );
							if( isShares )
								return true;

							if( ll.Label.ToLower().Contains( cc ) )
								return true;

							return false;
						}
					);
				}
			);
		}

		#endregion


		/// <summary>
		/// This method is designed to copy the segment title from the label column to the first data column
		/// Support label-less reports.
		/// </summary>
		public void RemoveLabelColumn()
		{
			if( ReportUtils.IsUnlabeledReport( this.ReportLongName ) )
			{
				this.DisplayLabelColumn = false;

				foreach( InstanceReportColumn irc in this.Columns )
				{
					if( irc.Labels != null )
					{
						irc.Labels.Clear();
					}
				}
				foreach( InstanceReportRow ir in this.Rows )
				{
					if( ir.IsSegmentTitle )
					{
						if( ( ir.Cells[ 0 ] as Cell ).NonNumbericText.Length == 0 )
						{
							( ir.Cells[ 0 ] as Cell ).NonNumbericText = Regex.Replace( ir.Label, @"\{.*?\} : ", String.Empty ).Trim();
						}
					}
				}

				for( int rowIndex = this.Rows.Count - 1; rowIndex >= 0; rowIndex-- )
				{
					InstanceReportRow irr = this.Rows[ rowIndex ] as InstanceReportRow;
					if( irr.IsAbstractGroupTitle )
					{
						this.Rows.RemoveAt( rowIndex );
					}
				}

				this.SynchronizeGrid();
			}
		}

		private bool HasMonetaryRows()
		{
			return ReportUtils.Exists( this.Rows,
				( row ) =>
				{
					if( row.IsMonetary )
						return true;

					if( row.IsEPS )
						return true;

					return false;
				});
		}

		public bool HasSegments()
		{
			if( this.HasSegmentsOnRows() )
				return true;

			bool exists = ReportUtils.Exists( this.Columns, col => col.Segments != null && col.Segments.Count > 0 );
			return exists;
		}

		/// <summary>
		/// Iterate through all rows to see if there are segment based columns copied from the segment processing
		/// </summary>
		/// <returns></returns>
		public bool HasSegmentsOnRows()
		{
			foreach( InstanceReportRow row in this.Rows )
			{
				if( row.OriginalInstanceReportColumn == null )
					continue;

				if( row.OriginalInstanceReportColumn.Segments == null || row.OriginalInstanceReportColumn.Segments.Count == 0 )
					continue;

				return true;
			}

			return false;
		}

		public void RepairColumnSegmentLabels()
		{
			foreach( InstanceReportColumn col in this.Columns )
			{
				this.RepairColumnSegmentLabels( col );
			}
		}

		public bool RepairColumnSegmentLabels( InstanceReportColumn col )
		{
			bool allSegmentsExist = false;

			foreach( LabelLine ll in col.Labels )
			{
				string newLabel;
				if( RepairColumnSegmentLabel( ll.Label, col, out newLabel ) )
					ll.Label = newLabel;
			}

			foreach( Segment colSeg in col.Segments )
			{
				if( colSeg == null || colSeg.DimensionInfo == null )
					continue;

				if( !this.AxisMembersByPresentation.ContainsKey( colSeg.DimensionInfo.dimensionId ) )
					continue;

				Segment preSeg = this.AxisMembersByPresentation[ colSeg.DimensionInfo.dimensionId ]
					.Find( pSeg => pSeg.DimensionInfo.Id == colSeg.DimensionInfo.Id );

				if( preSeg == null )
					allSegmentsExist = false;
				else
					colSeg.ValueName = preSeg.ValueName;
			}

			return allSegmentsExist;
		}

		private bool RepairColumnSegmentLabel( string segmentLabel, InstanceReportColumn col, out string newLabel )
		{
			newLabel = string.Empty;
			segmentLabel = segmentLabel.Trim();

			bool isLabelUpdated = false;

			//we need discrete labels to split by '|' in case this was on a row
			string[] tmpSegments = segmentLabel.Split( new char[] { '|' } );

			//remember how this label looked
			bool hadPipes = tmpSegments.Length > 1;

			//build a clean, maleable collection
			List<string> segmentLabels = new List<string>();
			foreach( string tmp in tmpSegments )
				segmentLabels.Add( tmp.Trim() );

			foreach( Segment columnSegment in col.Segments )
			{
				if( columnSegment == null || columnSegment.DimensionInfo == null )
					continue;

				if( !this.AxisMembersByPresentation.ContainsKey( columnSegment.DimensionInfo.dimensionId ) )
					continue;

				Segment presentationSegment = this.AxisMembersByPresentation[ columnSegment.DimensionInfo.dimensionId ]
					.Find( pSeg => pSeg.DimensionInfo.Id == columnSegment.DimensionInfo.Id );

				if( presentationSegment == null )
					continue;

				//generate the bad label for matching
				string generatedLabel = InstanceUtils.BuildSegmentScenarioName( columnSegment );
				for( int s = 0; s < 2; s++ )
				{
					generatedLabel = generatedLabel.Trim();

					//find if out set of labels have any of the "bad" labels and fix them
					for( int i = 0; i < segmentLabels.Count; i++ )
					{
						if( string.Equals( segmentLabels[ i ], generatedLabel ) )
						{
							isLabelUpdated = true;
							segmentLabels[ i ] = presentationSegment.ValueName.Trim();
						}
					}

					int splitAt = generatedLabel.IndexOf( ':' );
					if( splitAt == -1 )
						break;
					else
						generatedLabel = generatedLabel.Substring( splitAt + 1 );
				}
			}


			if( !isLabelUpdated )
				return false;

			//clean out any "default members" which may have removed the visible label
			segmentLabels.RemoveAll( lbl => string.Equals( lbl, string.Empty ) );

			newLabel = hadPipes ?
				string.Join( " | ", segmentLabels.ToArray() ) :
				string.Join( string.Empty, segmentLabels.ToArray() );

			return true;
		}

		public void SortColumns()
		{
			//Fix the segment position
			foreach( string axis in this.AxisByPresentation )
			{
				foreach( InstanceReportColumn col in this.Columns )
				{
					for( int s = 0; s < col.Segments.Count; s++ )
					{
						Segment colSeg = col.Segments[ s ] as Segment;
						if( colSeg.DimensionInfo.dimensionId == axis )
						{
							col.Segments.RemoveAt( s );
							col.Segments.Add( colSeg );
							break;
						}
					}
				}
			}

			//now relabel the columns
			this.ApplyAllColumnLabels();

			this.SortColumns(
				InstanceReport.CompareObjects,
				this.CompareSegments,
				InstanceReport.CompareCalendars,
				InstanceReport.CompareCurrencies
			);
		}

		public void SortColumns( params Comparison<InstanceReportColumn>[] comparisons )
		{
			Comparison<InstanceReportColumn> sorter = ( left, right ) =>
			{
				foreach( Comparison<InstanceReportColumn> comparison in comparisons )
				{
					int cmp = comparison( left, right );
					if( cmp != 0 )
						return cmp;
				}

				return 0;
			};

			this.SynchronizeGrid();

			InstanceReportColumn[] columns = this.Columns.ToArray();
			Array.Sort( columns, sorter );

			//we must work accoring to the expected positions so that columns are reordered left to right
			for( int expectedIdx = 0; expectedIdx < columns.Length; expectedIdx++ )
			{
				this.MoveColumn( columns[ expectedIdx ], expectedIdx );
			}

			this.SynchronizeGrid();
		}

		public static int CompareCalendars( InstanceReportColumn leftCol, InstanceReportColumn rightCol )
		{
			int issetCmp = InstanceReport.CompareObjects( leftCol, rightCol );
			if( issetCmp != 0 )
				return issetCmp;

			int calendarCmp = InstanceReport.CompareCalendars( leftCol.MyContextProperty, rightCol.MyContextProperty );
			return calendarCmp;
		}

		public static int CompareCalendars( ContextProperty leftCtx, ContextProperty rightCtx )
		{
			int issetCmp = CompareObjects( leftCtx, rightCtx );
			if( issetCmp != 0 )
				return issetCmp;

			if( leftCtx.PeriodType != rightCtx.PeriodType )
			{
				if( leftCtx.PeriodType == Element.PeriodType.duration )
					return -1;
				else
					return 1;
			}

			if( leftCtx.PeriodType == Element.PeriodType.duration )
			{
				TimeSpan leftSpan = leftCtx.PeriodEndDate - leftCtx.PeriodStartDate;
				TimeSpan rightSpan = rightCtx.PeriodEndDate - rightCtx.PeriodStartDate;
				TimeSpan diffSpan = leftSpan > rightSpan ?
					leftSpan - rightSpan :
					rightSpan - leftSpan;

				if( diffSpan.Days > 15 )
				{
					if( leftSpan < rightSpan )
						return -1;
					else
						return 1;
				}

				int dateCmp = DateTime.Compare( leftCtx.PeriodStartDate, rightCtx.PeriodStartDate );
				if( dateCmp != 0 )
					return dateCmp * -1;

				dateCmp = DateTime.Compare( leftCtx.PeriodEndDate, rightCtx.PeriodEndDate );
				if( dateCmp != 0 )
					return dateCmp;
			}
			else
			{
				int dateCmp = DateTime.Compare( leftCtx.PeriodStartDate, rightCtx.PeriodStartDate );
				if( dateCmp != 0 )
					return dateCmp * -1;
			}

			return 0;
		}

		public static int CompareCurrencies( InstanceReportColumn leftCol, InstanceReportColumn rightCol )
		{
			int issetCmp = InstanceReport.CompareObjects( leftCol, rightCol );
			if( issetCmp != 0 )
				return issetCmp;

			bool leftEmpty = string.IsNullOrEmpty( leftCol.CurrencyCode );
			bool rightEmpty = string.IsNullOrEmpty( rightCol.CurrencyCode );
			if( leftEmpty && rightEmpty )
				return 0;

			//push "empty" currencies to the left
			if( leftEmpty )
				return -1;

			if( rightEmpty )
				return 1;

			if( string.Equals( leftCol.CurrencyCode, rightCol.CurrencyCode ) )
			{
				if( leftCol.Units.Count == 1 && rightCol.Units.Count == 1 )
				{
					//push EPS to the left
					if( leftCol.Units[ 0 ].UnitType == UnitProperty.UnitTypeCode.Divide )
					{
						if( rightCol.Units[ 0 ].UnitType == UnitProperty.UnitTypeCode.Divide )
							return 0;
						else
							return -1;
					}
					else if( rightCol.Units[ 0 ].UnitType == UnitProperty.UnitTypeCode.Divide )
					{
						return 1;
					}
				}

				return 0;
			}


			//the currencies are not the same - push USD to the left
			if( string.Equals( leftCol.CurrencyCode, InstanceUtils.USDCurrencyCode ) )
				return -1;
			else if( string.Equals( rightCol.CurrencyCode, InstanceUtils.USDCurrencyCode ) )
				return 1;

			//sort other currencies alphabetically
			int cmp = string.Compare( leftCol.CurrencyCode, rightCol.CurrencyCode );
			return cmp;
		}

		public int CompareSegments( Segment leftSeg, Segment rightSeg )
		{
			int leftIdx = this.GetSegmentMemberIndex( leftSeg );
			int rightIdx = this.GetSegmentMemberIndex( rightSeg );

			if( leftIdx < rightIdx )
				return -1;
			else if( leftIdx > rightIdx )
				return 1;
			else
				return 0;
		}

		public int CompareSegments( InstanceReportColumn leftCol, InstanceReportColumn rightCol )
		{
			int cmp = InstanceReport.CompareObjects( leftCol, rightCol );
			if( cmp != 0 )
				return cmp;

			cmp = this.CompareSegments( leftCol.Segments, rightCol.Segments );
			if( cmp != 0 )
				return cmp;

			return 0;
		}

		public int CompareSegments( IList leftSegments, IList rightSegments )
		{
			int cmp = InstanceReport.CompareCountables( leftSegments, rightSegments );
			if( cmp != 0 )
			{
				//push non-segments to the left
				return cmp * -1;
			}

			if( leftSegments == null )
				return 0;

			foreach( string axis in this.AxisByPresentation )
			{
				int leftIdx = GetSegmentMemberIndex( axis, leftSegments );
				int rightIdx = GetSegmentMemberIndex( axis, rightSegments );
				if( leftIdx < rightIdx )
					return -1;
				else if( leftIdx > rightIdx )
					return 1;
			}

			return 0;
		}

		public int GetSegmentMemberIndex( string axis, IList columnSegments )
		{
			Segment colSeg = null;
			foreach( Segment seg in columnSegments )
			{
				if( seg.DimensionInfo.dimensionId == axis )
				{
					colSeg = seg;
					break;
				}
			}

			int segIdx = -1;
			if( this.AxisMembersByPresentation != null && this.AxisMembersByPresentation.ContainsKey( axis ) )
			{
				if( colSeg == null )
					segIdx = this.AxisMembersByPresentation[ axis ].FindIndex( seg => seg.IsDefaultForEntity );
				else
					segIdx = this.AxisMembersByPresentation[ axis ].FindIndex( seg => seg.DimensionInfo.Id == colSeg.DimensionInfo.Id );
			}

			return segIdx;
		}

		private int GetSegmentMemberIndex( Segment member )
		{
			int foundAt = this.AxisMembersByPresentation[ member.DimensionInfo.dimensionId ]
				.FindIndex( seg => seg.DimensionInfo.Id == member.DimensionInfo.Id );
			return foundAt;
		}

		private static int CompareCountables( ICollection leftObj, ICollection rightObj )
		{
			int cmp = InstanceReport.CompareObjects( leftObj, rightObj );
			if( cmp == 0 && leftObj != null )
			{
				if( leftObj.Count == 0 )
				{
					if( rightObj.Count == 0 )
						return 0;
					else
						return 1;
				}
				else if( rightObj.Count == 0 )
				{
					return -1;
				}
			}

			return cmp;
		}

		private static int CompareObjects( object leftObj, object rightObj )
		{
			if( leftObj == null )
			{
				if( rightObj == null )
					return 0;
				else
					return 1;
			}
			else if( rightObj == null )
			{
				return -1;
			}

			return 0;
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

		public InstanceReport.ElementSegmentCombinations GetUniqueInUseElementSegmentCombinations()
		{
			InstanceReportColumn lastSegmentColumn = null;
			bool hasSegmentsOnRows = this.HasSegmentsOnRows();
			InstanceReport.ElementSegmentCombinations combinations = new ElementSegmentCombinations();

			foreach( InstanceReportRow row in this.Rows )
			{
				if( hasSegmentsOnRows )
				{
					if( row.OriginalInstanceReportColumn != null )
						lastSegmentColumn = row.OriginalInstanceReportColumn;
				}

				if( row.ElementName == null )
					continue;

				if( row.ElementName.Trim().Length == 0 )
					continue;

				//check to make sure we have some data for this row before adding the element.
				for( int c = 0; c < row.Cells.Count; c++ )
				{
					Cell cell = row.Cells[ c ];
					if( !cell.HasData )
						continue;

					combinations.AddElement( row.ElementName );

					InstanceReportColumn segmentColumn = hasSegmentsOnRows ?
						lastSegmentColumn : this.Columns[ c ];

					if( segmentColumn != null )
					{
						StringBuilder sb = new StringBuilder();
						foreach( string segmentKey in segmentColumn.GetUniqueInUseSegments().Keys )
						{
							sb.AppendLine( segmentKey );
						}
						combinations.AddElementSegment( row.ElementName, sb.ToString() );
					}
				}
			}

			return combinations;
		}

		public class ElementSegmentCombinations
		{
			private Dictionary<string, Dictionary<string, int>> elementSegmentLookup = new Dictionary<string, Dictionary<string, int>>();

			public ElementSegmentCombinations() { }

			public void AddElement( string element )
			{
				if( this.ContainsElement( element ) )
					return;

				this.elementSegmentLookup[ element ] = new Dictionary<string, int>();
			}

			public void AddElementSegment( string element, string segment )
			{
				this.AddElement( element );
				this.elementSegmentLookup[ element ][ segment ] = 1;
			}

			public bool ContainsElement( string element )
			{
				if( element == null )
					return false;

				return this.elementSegmentLookup.ContainsKey( element );
			}

			public bool ContainsElementAndSegments( string element, params Segment[] segments )
			{
				if( !this.ContainsElement( element ) )
					return false;

				string[] segmentStrings = new string[ segments.Length ];
				for( int s = 0; s < segments.Length; s++ )
				{
					Segment seg = segments[ s ];
					segmentStrings[ s ] = seg.DimensionInfo.dimensionId + ":" + seg.DimensionInfo.Id;
				}

				return this.ContainsElementAndSegments( element, segmentStrings );
			}

			public bool ContainsElementAndSegments( string element, params string[] segments )
			{
				if( !this.ContainsElement( element ) )
					return false;

				foreach( string segmentKey in segments )
				{
					if( !this.elementSegmentLookup[ element ].ContainsKey( segmentKey ) )
						return false;
				}

				return true;
			}

			public void Merge( ElementSegmentCombinations reportCombinations )
			{
				foreach( string element in reportCombinations.elementSegmentLookup.Keys )
				{
					this.AddElement( element );

					foreach( string segmentKey in reportCombinations.elementSegmentLookup[ element ].Keys )
					{
						this.AddElementSegment( element, segmentKey );
					}
				}
			}
		}

		public void MoveColumn( InstanceReportColumn col, int expectedIdx )
		{
			int actualIdx = this.Columns.IndexOf( col );
			if( actualIdx == expectedIdx )
				return;

			this.Columns.Remove( col );
			this.Columns.Insert( expectedIdx, col );

			if( this.Rows == null )
				return;

			foreach( InstanceReportRow row in this.Rows )
			{
				Cell cell = row.Cells[ actualIdx ]; // row.Cells.Find( c => c.Id == col.Id );
				row.Cells.RemoveAt( actualIdx );
				row.Cells.Insert( expectedIdx, cell );
			}
		}
	}

    #region Helper Classes

    public class ColumnInstantDurationMap : IComparable
    {
        public int InstantColumnIndex = -1;
        public int DurationColumnIndex = -1;
        TimeSpan TimeBucketSize;

        public ColumnInstantDurationMap(int instantColumnIndex, int durationColumnIndex, TimeSpan timeBucketSize)
        {
            this.InstantColumnIndex = instantColumnIndex;
            this.DurationColumnIndex = durationColumnIndex;
            this.TimeBucketSize = timeBucketSize;
        }

        /// <summary>
        /// Not sure how to call a constructor from the NxBRE rules engine so instead we will call this helper method to build
        /// a new ColumnMap
        /// </summary>
        /// <param name="instanceColIndex"></param>
        /// <param name="durationColIndex"></param>
        /// <param name="durationContext"></param>
        /// <returns></returns>
        public static ColumnInstantDurationMap NewColumnMap(int instanceColIndex, int durationColIndex, ContextProperty durationContext)
        {
            return new ColumnInstantDurationMap(instanceColIndex, durationColIndex, durationContext.PeriodEndDate - durationContext.PeriodStartDate);
        }

        #region IComparable Members

        public int CompareTo(object inObj)
        {
            return ((inObj as ColumnInstantDurationMap).TimeBucketSize).CompareTo(this.TimeBucketSize);
        }

        #endregion
    }

    public class ColumnMap : IComparable
    {
        /// <summary>
        /// The Original InstanceReportColumn with all of it's metadata and column Id
        /// </summary>
        public InstanceReportColumn IRC;

        /// <summary>
        /// The Id of the column that IRC is being mapped to
        /// </summary>
        public int MappedColumnId = 0;

        public bool IsEquityPrevioslyReportedAsCol = false;
        public bool IsEquityAdjustmentColumn = false;

        public ColumnMap(InstanceReportColumn irc, int id)
        {
            this.IRC = irc;
            this.MappedColumnId = id;
        }

        public override string ToString()
        {
            return this.MappedColumnId.ToString() + " " + this.IRC.ToString();
        }
        #region IComparable Members

        public int CompareTo(object inObj)
        {
            return this.MappedColumnId.CompareTo((inObj as ColumnMap).MappedColumnId);
        }

        #endregion
    }

    [Serializable]
    public class Footnote : IComparable
    {
        public int NoteId = 0;
        public string Note = string.Empty;

        public Footnote()
        {
        }

        public Footnote(int Id, string note)
        {
            this.NoteId = Id;
            this.Note = note;
        }

        public int CompareTo(object inObj)
        {
            return this.NoteId.CompareTo((inObj as Footnote).NoteId);
        }
    }

    #endregion

}
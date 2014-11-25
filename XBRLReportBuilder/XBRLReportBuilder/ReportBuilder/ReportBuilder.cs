//=============================================================================
// ReportBuilder (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the main functionality entry point for the rendering engine.
// The method "BuildReports" is called to process the filing.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

using Aucent.FilingServices.Data;
using Aucent.FilingServices.RulesRepository;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Threading;
//using net.xmlcatalog;

namespace XBRLReportBuilder
{
	/// <summary>
	/// <para>The primary class for "building reports" using an XBRL instance document and taxonomy.</para>
	/// <para>Building reports is initiated with <see cref="BuildReports"/>.</para>
	/// </summary>
	/// <seealso cref="RulesEngineUtils"/>.<see>SetBaseResourcePath</see>
	/// <seealso cref="SetSynchronizedResources" />
	/// <seealso cref="CurrencyMappingFile" />
	/// <seealso cref="RemoteFileCachePolicy" />
	public partial class ReportBuilder : IReportBuilder
	{
		#region constants

		private const string _instanceNotExist = "Cannot find the instance document";
		private const string _taxonomyNotExist = "Cannot find the company taxonomy";
		private const string _allReports = "All Reports";
		private const string _uncategorizedLineItems = "Uncategorized Items";

		private const int _missingReportIndex = 9999;
		private const string _authRefName = "defnref.xml";
		private const string _reportTitle = "rptitle";

		private static readonly Regex qnameRegex = new Regex( @"^\S+:\S+$", RegexOptions.Compiled );
		private static readonly Regex w3cDurationRegex = new Regex( @"^\s*
(?<minus>-)?
P
((?<years>\d+)Y)?
((?<months>\d+)M)?
((?<days>\d+)D)?
(T(?=((\d+(H|M))|(\d+(\.\d+)?S)))
	((?<hours>\d+)H)?
	((?<minutes>\d+)M)?
	((?<seconds>\d+(\.\d+)?)S)?
)?
\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace );


		private static readonly Regex FOOTNOTE_MATCH = new Regex( @"\[([^\]]+)\]", RegexOptions.Compiled );

		#endregion

		#region fields

		private bool cleanupStatementsOnly = true;


		/// <summary>
		/// The current FilingSummary object.  Only populated within BuildReports()
		/// </summary>
		private FilingSummary currentFilingSummary = null;

		/// <summary>
		/// The current version of this assembly
		/// </summary>
		private readonly string currentAssemblyVersion = string.Empty;

		private Instance currentInstance = null;
		private Taxonomy currentTaxonomy = null;
		private string currentInstancePath = null;
		private string currentTaxonomyPath = null;

		private string currentReportDirectory = string.Empty;
		private string preferredLanguage = string.Empty;
		private string rulesFileName = "ReportBuilder";

		//NOTE - Used for rule processing - DNE!
		public string CurrentElementName = string.Empty;
		public bool IsRatioElement = false;
		public bool IsZeroAsNoneElement = false;

		//Deprecated 2.4.0.2
		//public string AccessionNumber;
		//public string CompanyName;
		//public string FilingDate;
		//public string FiscalYearEnd;
		//public string PeriodEnding;
		//public string TickerSymbol;

		/// <summary>
		/// This dictionary can help discern if an element's type is different from the unit type that is applied
		/// </summary>
		Dictionary<string, UnitType> elementUnitTypes = new Dictionary<string, UnitType>();

		List<ReportHeader> reportsWithEmbeds = new List<ReportHeader>();

		List<string> commonAxes = new List<string>();

		Dictionary<string, List<Segment>> commonDimensions = new Dictionary<string, List<Segment>>();

		Dictionary<string, int> internalReports = new Dictionary<string, int>( StringComparer.CurrentCultureIgnoreCase );

		//roleAxes[ role, List<axis> ]
		Dictionary<string, List<string>> roleAxes = new Dictionary<string, List<string>>( StringComparer.CurrentCultureIgnoreCase );

		Dictionary<string, int> presentationElements = new Dictionary<string, int>();

		//roleAxisDefaults[ role, axis, Segment ]
		Dictionary<string, Dictionary<string, Segment>> roleAxisDefaults = new Dictionary<string, Dictionary<string, Segment>>( StringComparer.CurrentCultureIgnoreCase );

		//roleAxisMembers[ role, axis, List<member> ]
		Dictionary<string, Dictionary<string, List<Segment>>> roleAxisMembers = new Dictionary<string, Dictionary<string, List<Segment>>>( StringComparer.CurrentCultureIgnoreCase );

		//columnCache[ contextID ][ unitID ] = obj as InstanceReportColumn;
		private Dictionary<string, Dictionary<string, InstanceReportColumn>> columnCache = new Dictionary<string, Dictionary<string, InstanceReportColumn>>();

		//customUnits[ unitID ] = (int)(UnitType)
		private Dictionary<string, int> customUnits = new Dictionary<string, int>();
		private Dictionary<int, string> unitDictionary = new Dictionary<int, string>();

		//elementCache[ elementID ] = obj as Element;
		private Dictionary<string, Element> elementCache = new Dictionary<string, Element>();

		//elementRounding[ elementID ][ InstanceUtils._Precision* ] = obj as Precision
		private Dictionary<string, Dictionary<string, Precision>> elementRounding = new Dictionary<string, Dictionary<string, Precision>>();

		//markupCache[ elementID ] = obj as List<Cell>;
		private Dictionary<string, List<Cell>> markupCache = new Dictionary<string, List<Cell>>();

		#endregion


		#region properties

		private string _currencyMappingFile = RulesEngineUtils.DefaultCurrencyMappingFile;
		public string CurrencyMappingFile
		{
			get { return this._currencyMappingFile; }
			set { this._currencyMappingFile = value; }
		}

		public HtmlReportFormat HtmlReportFormat { get; set; }
		public ReportFormat ReportFormat { get; set; }
		public RequestCacheLevel RemoteFileCachePolicy { get; set; }
		//public XmlCatalogResolver XmlCatalog;

		/// <summary>
		/// role names of all the excluded reports....
		/// </summary>
		public List<string> ExcludedReports { get; set; }

		private RulesRepository builderRules = null;
		public RulesRepository BuilderRules
		{
			get
			{
				return builderRules;
			}
			private set
			{
				builderRules = value;
			}
		}

		///Indicates the current row's preferred label role during processing
		///this is used from the the RuleProcessing code base
		private string currentRowPreferredLabelRole = "label";
		public string CurrentRowPreferredLabelRole
		{
			get
			{
				if( string.IsNullOrEmpty( this.currentRowPreferredLabelRole ) )
					return null;

				return this.currentRowPreferredLabelRole.ToLower();
			}
		}

        private string xsltStylesheetPath = string.Empty;
		public string XsltStylesheetPath
		{
            get
            {
                return xsltStylesheetPath;
            }
            set
            {
                xsltStylesheetPath = value;
            }
        }

		//DO NOT REMOVE - These variables are used for rule processing
		private bool isGAAP2005 = false;
		public bool IsGAAP2005
		{
			get { return isGAAP2005; }
		}

		//DO NOT REMOVE - These variables are used for rule processing
		private bool isNewGAAP = true;
		public bool IsNewGAAP
		{
			get { return isNewGAAP; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// The default constructor.  Assumes that the default rendering rules (<see cref="RulesEngineUtils.DefaultRulesFile"/>) should be used.
		/// Also, sets the output format to <see cref="ReportFormat"/>.Xml.
		/// </summary>
		public ReportBuilder()
			: this( RulesEngineUtils.DefaultRulesFile, ReportFormat.Xml, HtmlReportFormat.None )
		{
		}

		/// <summary>
		/// <para>This constructor expects you to pass the name of the rules file WITHOUT the extension.</para>
		/// <para>See <seealso cref="RulesEngineUtils.DefaultRulesFile"/> (public)</para>
		/// <para>See reportBuilderInstance.<seealso cref="SetRulesFile"/> (public)</para>
		/// </summary>
		/// <param name="rulesFileNameIn">
		///		<para>The name of the rules file WITHOUT the extension.  Default: <see cref="RulesEngineUtils.DefaultRulesFile"/></para>
		/// </param>
		/// <param name="type">
		///		<para>The format(s) to output. Possible values:</para>
		///		<para> - <see cref="ReportFormat"/>.Xml</para>
		///		<para> - <see cref="ReportFormat"/>.Html</para>
		///		<para> - <see cref="ReportFormat"/>.HtmlAndXml</para>
		/// </param>
		/// <param name="htmlFormat">
		///		<para>The HTML format to output, if <see cref="ReportFormat"/> contains 'Html'.</para>
		///		<para>Possible values:</para>
		///		<para> - <see cref="HtmlReportFormat"/>.Complete</para>
		///		<para> - <see cref="HtmlReportFormat"/>.Fragment</para>
		/// </param>
		public ReportBuilder( string rulesFileNameIn, ReportFormat type, HtmlReportFormat htmlFormat )
		{
			this.currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			this.HtmlReportFormat = htmlFormat;
			this.ReportFormat = type;
			this.SetRulesFile( rulesFileNameIn );
		}


		~ReportBuilder()
		{
		}

		/// <summary>
		/// <para>Sets the rules file for reportBuilderInstance.<see cref="BuildReports"/> and loads its settings.</para>
		/// <para>See <seealso cref="RulesEngineUtils"/>.<seealso>DefaultRulesFile</seealso> (public)</para>
		/// <para>See reportBuilderInstance.<seealso cref="InitializeReportBuilder"/> (private)</para>
		/// </summary>
		/// <param name="rulesFileNameIn">The name of the rules file WITHOUT the extension.  Default: <see cref="RulesEngineUtils.DefaultRulesFile"/></param>
		public void SetRulesFile( string rulesFileNameIn )
		{
			this.rulesFileName = rulesFileNameIn;
			this.InitializeReportBuilder();
		}

		/// <summary>
		/// Performs several actions integral to ReportBuilder:
		/// <para> - Synchronize (extract) the embedded resources (if necessary). (See <see cref="RulesEngineUtils.SynchronizeResources()" />)</para>
		/// <para> - Loads "TaxonomyAddongManager.xml" in order to look up well-known Documentation and References</para>
		/// <para> - Loads the RulesFile and rule-file references.</para>
		/// <remarks>A rules file can be named in the constructor, or using reportBuilderInstance.<see cref="SetRulesFile"/></remarks>
		/// </summary>
		private void InitializeReportBuilder()
		{
			lock( synchronizedResourcesLock )
			{
				if( !SynchronizedResources )
				{
					bool isSynchronized = RulesEngineUtils.SynchronizeResources();
					SetSynchronizedResources( isSynchronized );
				}

				if( SynchronizedResources && _defRefHelper == null )
				{
					string taConfig = RulesEngineUtils.GetResourcePath( string.Empty, "TaxonomyAddonManager.xml" );

					TaxonomyAddonManager tam;
					if( TaxonomyAddonManager.TryLoadFile( taConfig, out tam ) )
					{
						TaxonomyAddonManager.BasePath = RulesEngineUtils.GetBaseResourcePath();
						_defRefHelper = tam;
					}
				}
			}

			string rulesFolder = RulesEngineUtils.GetRulesFolder();
			DirectoryInfo di = new DirectoryInfo( rulesFolder );

			BuilderRules = new RulesRepository( rulesFileName, di );
			BuilderRules.TryLoadExistingRulesList();
		}

		#endregion

		#region public methods

		/// <summary>
		/// <para>Performs the "building of reports" based on the <paramref name="instancePath"/> and <paramref name="taxonomyPath"/> provided.</para>
		/// <para>As a result, <paramref name="filingSummary"/>.<see>MyReports</see> (<see cref="FilingSummary"/>) will be populated with instances of <see cref="ReportHeader"/>,</para>
		/// <para>and <paramref name="filingSummary"/>.<see>MyReports</see> will be XML serialized to <paramref name="filingSummaryPath"/></para>
		/// <para>If an error occurs, <paramref name="error" /> will be populated.</para>
		/// </summary>
		/// <param name="instancePath">The path to the instance document.</param>
		/// <param name="taxonomyPath">The path to the taxonomy document.</param>
		/// <param name="filingSummaryPath">The path where the generated <see cref="FilingSummary"/> object should be saved.</param>
		/// <param name="reportDirectory">The path where the generated content is saved.</param>
		/// <param name="filingSummary">The <see cref="FilingSummary"/> object to populate.</param>
		/// <param name="error">The error message for any critical errors which might occur.</param>
		/// <returns>True on success or false for fail.</returns>
		public bool BuildReports( string instancePath,
			string taxonomyPath,
			string filingSummaryPath,
			string reportDirectory,
			out FilingSummary filingSummary, out string error )
		{
			error = string.Empty;
			filingSummary = null;

			if( !instancePath.StartsWith( "http", StringComparison.InvariantCultureIgnoreCase ) && !File.Exists( instancePath ) )
			{
				error = string.Format( "{0} : {1}", _instanceNotExist, instancePath );
				return false;
			}

			if( !taxonomyPath.StartsWith( "http", StringComparison.InvariantCultureIgnoreCase ) && !File.Exists( taxonomyPath ) )
			{
				error = string.Format( "{0} : {1}", _taxonomyNotExist, taxonomyPath );
				return false;
			}

			try
			{
				string baseHRef = string.Empty;
				string curDir = Environment.CurrentDirectory;
				if( instancePath.StartsWith( "http", StringComparison.InvariantCultureIgnoreCase ) )
				{
					int len = instancePath.Length - Path.GetFileName( instancePath ).Length;
					baseHRef = instancePath.Substring( 0, len );
				}
				else
				{
					curDir = Path.GetDirectoryName( instancePath );
				}


				//this.XmlCatalog

				int numErrors = 0;
				Taxonomy taxonomy = null;
				string errorMsg = string.Empty;
				if( !TaxonomyUtils.TryLoadTaxonomy( taxonomyPath, this.RemoteFileCachePolicy, out taxonomy, out numErrors, out errorMsg ) )
				{
					error = "Error parsing the taxonomy: " + errorMsg;
					return false;
				}

				this.currentTaxonomyPath = taxonomyPath;
                bool isBuilt = this.BuildReports( instancePath, taxonomy, filingSummaryPath, reportDirectory, out filingSummary, out error );
				return isBuilt;
			}
			catch( Exception ex )
			{
				error = string.Format( "Exception thrown in BuildReports: {0}", ex.Message );
			}
			finally
			{
				this.currentTaxonomyPath = null;
			}

			return false;
		}

		/// <summary>
		/// <para>Performs the "building of reports" based on the <paramref name="instancePath"/> and <paramref name="taxonomy"/> provided.</para>
		/// <para>As a result, <paramref name="filingSummary"/>.<see>MyReports</see> (<see cref="FilingSummary"/>) will be populated with instances of <see cref="ReportHeader"/>,</para>
		/// <para>and <paramref name="filingSummary"/>.<see>MyReports</see> will be XML serialized to <paramref name="filingSummaryPath"/></para>
		/// <para>If an error occurs, <paramref name="error" /> will be populated.</para>
		/// </summary>
		/// <param name="instancePath">The path to the instance document.</param>
		/// <param name="taxonomy">The loaded and parsed taxonomy object.</param>
		/// <param name="filingSummaryPath">The path where the generated <see cref="FilingSummary"/> object should be saved.</param>
		/// <param name="reportDirectory">The path where the generated content is saved.</param>
		/// <param name="filingSummary">The <see cref="FilingSummary"/> object to populate.</param>
		/// <param name="error">The error message for any critical errors which might occur.</param>
		/// <returns>True on success or false for fail.</returns>
		public bool BuildReports( string instancePath, Taxonomy taxonomy,
			string filingSummaryPath, string reportDirectory,
			out FilingSummary filingSummary, out string error )
		{
			error = string.Empty;
			filingSummary = null;
			DateTime dtStart = DateTime.Now;

			try
			{
				this.currentFilingSummary = new FilingSummary();
				this.currentInstancePath = instancePath;
				this.currentTaxonomy = taxonomy;
				this.currentReportDirectory = reportDirectory;

				if( !this.ValidateSettings( out error ) )
					return false;

				//create the reports directory so that defnref can be generated
				if( !Directory.Exists( this.currentReportDirectory ) )
					Directory.CreateDirectory( this.currentReportDirectory );

				if( !string.IsNullOrEmpty( this.CurrencyMappingFile ) )
					this.LoadCurrencies();

				//set up this symbol for reuse throughout
				InstanceUtils.USDCurrencySymbol = InstanceUtils.GetCurrencySymbolFromCode( InstanceUtils.USDCurrencyCode );

				if( string.IsNullOrEmpty( this.preferredLanguage ) )
					this.preferredLanguage = this.GetPreferredLanguage();

				bool isIMBased = this.CheckIsIMBased();

				//DateTime startInstance = DateTime.Now;

				ArrayList errors = null;
				if( !InstanceUtils.
                    TryLoadInstanceDocument( this.currentInstancePath, out this.currentInstance, out errors ) )
				{
					string[] arrErrors = new string[ errors.Count ];
					for( int i = 0; i < errors.Count; i++ )
					{
						arrErrors[ i ] = ( (ParserMessage)errors[ i ] ).Message;
					}

					string instanceDocErrors = string.Join( "\r\n  ", arrErrors );
					Regex splitPoint = new Regex( @"\S\s+at" );
					arrErrors = splitPoint.Split( instanceDocErrors );
					instanceDocErrors = arrErrors[ 0 ];
					error = "Unable to load the instance document:\r\n  " + instanceDocErrors;
					return false;
				}

				ArrayList taxonomies = new ArrayList();
				taxonomies.Add( this.currentTaxonomy );
				this.currentInstance.FixPrefixInInstanceDocument( taxonomies );

				//this.currentFilingSummary.InstanceLoadTime = DateTime.Now - startInstance;
				//this.currentFilingSummary.FactCount = this.currentInstance.markups.Count;

				this.PopulateMarkupDictionaries();
				this.BuildFilingSummary();

				this.FireBuildReportsProcessing();

				this.currentFilingSummary.MyReports.Clear();
				ArrayList topNodes = currentTaxonomy.GetNodesByPresentation( false, this.ExcludedReports );
				foreach( Node topNode in topNodes )
				{
					InstanceReport report = null;

					try
					{
						if( this.BuildReport( topNode, out report ) )
						{
							if( report.IsEmbedReport || report.HasEmbeddedReports )
							{
								this.roleAxes[ report.RoleURI ] = report.AxisByPresentation;
								this.roleAxisDefaults[ report.RoleURI ] = report.AxisMemberDefaults;
								this.roleAxisMembers[ report.RoleURI ] = report.AxisMembersByPresentation;
							}
							
							ReportHeader header = this.currentFilingSummary.AddReport( report );
							this.ApplyRulesToReport( report );

							report.OnRuleProcessing -= this.OnRuleProcessing;
							report.OnRuleProcessing += this.OnRuleProcessing;

							report.OnRuleProcessed -= this.OnRuleProcessed;
							report.OnRuleProcessed += this.OnRuleProcessed;

							report.UnitDictionary = this.unitDictionary;
							string fullPath = Path.Combine( this.currentReportDirectory, header.XmlFileName );
							report.BuildXMLDocument( fullPath, true, isIMBased, this.currentFilingSummary );
							report.UnitDictionary = null;

							report.OnRuleProcessing -= this.OnRuleProcessing;
							report.OnRuleProcessed -= this.OnRuleProcessed;

							if( header.HasEmbeddedReports )
								this.reportsWithEmbeds.Add( header );
						}
					}
					finally
					{
						if( report != null )
							report.Dispose();
					}
				}

				//Build Missing Elements (Uncategorized) Report before we flush the markups
				InstanceReport missingReport;
				if( this.BuildMissingDataReport( out missingReport ) )
				{
				    ReportHeader uncatHeader = this.currentFilingSummary.AddReport( missingReport );
				    uncatHeader.XmlFileName = InstanceUtils.TOP_LEVEL_REPORT_INDICATOR+ _missingReportIndex +".xml";
				}

				//Free up some resources
				this.currentInstance = null;

				if( this.internalReports.Count == 0 )
				{
					this.currentTaxonomy.Close();
					this.currentTaxonomy = null;
				}

				//clear the dictionaries after checking internalReports
				this.ClearMarkupDictionaries();

				 
				#region Clean up Columns

				//if the company has filed earning release, do not touch the reports.
				//Based on request from SEC, do not remove any reports if the base taxonomy is the new GAAP taxonomy (2208)

				if( !this.HasEarningRelease() )
				{
					//DO NOT REMOVE - These are used in rule processing
					isGAAP2005 = this.TaxonomyIsGAAP2005();
					isNewGAAP = ( !isGAAP2005 );

					ProcessFlowThroughColumnsReports();
				}

				#endregion

				#region Build All Reports

				//Build book 
				ReportHeader r1 = new ReportHeader();
				r1.LongName = _allReports;
				r1.ShortName = _allReports;
				r1.ReportType = ReportHeaderType.Book;
				r1.IsDefault = this.currentFilingSummary.MyReports.Count == 0;
				this.currentFilingSummary.AddReports( r1 );

				#endregion

				#region Process Embedded Reports

				bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.EMBED_REPORTS_RULE );
				if( isRuleEnabled && this.reportsWithEmbeds.Count > 0 )
				{
					List<ReportHeader> embedReports = this.ProcessEmbeddedReports();
					if( embedReports.Count > 0 )
					{
						foreach( ReportHeader embedReport in embedReports )
						{
							if( this.currentFilingSummary.MyReports.Contains( embedReport ) )
							{
								this.currentFilingSummary.MyReports.Remove( embedReport );

								string reportNameToDelete = Path.Combine( this.currentReportDirectory, embedReport.XmlFileName );
								if( File.Exists( reportNameToDelete ) )
									File.Delete( reportNameToDelete );
							}
						}
					}

					this.FireRuleProcessed( RulesEngineUtils.EMBED_REPORTS_RULE );
				}
				#endregion

				#region Generate Excel Workbook

				isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.GENERATE_EXCEL_RULE );
				if( isRuleEnabled )
				{
					string excelReportName = "Financial_Report";
					bool processed = ExcelUtility.GenerateExcelWorkbook( this.currentFilingSummary, this.currentReportDirectory, excelReportName );
					this.FireRuleProcessed( RulesEngineUtils.GENERATE_EXCEL_RULE );

					if( !processed )
					{
						error = "Failed to generate Excel Workbook for report: " + this.currentReportDirectory + " " + excelReportName;
						return false;
					}
				}

				#endregion

				#region Generate Output Formats
				if( ( this.ReportFormat & ReportFormat.Html ) == ReportFormat.Html )
				{
					if( !this.GenerateHtmlFiles() )
					{
						//The error was logged to the tracer
					}
				}

				if( ( this.ReportFormat & ReportFormat.Xml ) == ReportFormat.Xml )
				{
					//this format already exists
				}
				else
				{
					// this.currentFilingSummary.TraceInformation( "Information: Report Formart does not include 'Xml'.  Removing references and files." );

					foreach( ReportHeader rh in this.currentFilingSummary.MyReports )
					{
						if( !string.IsNullOrEmpty( rh.HtmlFileName ) )
						{
							string deleteFile = Path.Combine( this.currentReportDirectory, rh.XmlFileName );
							if( File.Exists( deleteFile ) )
								File.Delete( deleteFile );

							rh.XmlFileName = null;
						}
					}
				}

				#endregion

				#region Check and fix default report 

				//Recheck the default report just in case it was deleted
				bool foundDefault = false;
				foreach( ReportHeader header in this.currentFilingSummary.MyReports )
				{
					if( header.IsDefault )
					{
						foundDefault = true;
						break;
					}
				}

				if( !foundDefault )
				{
					foreach( ReportHeader header in this.currentFilingSummary.MyReports )
					{
						if( header.IsBalanceSheet() || header.ReportType == ReportHeaderType.Book )
						{
							header.IsDefault = true;
							break;
						}
					}
				}

				
				#endregion

				return true;
			}
			catch( Exception ex )
			{
			    error = string.Format( "Exception thrown in BuildReports: {0}", ex.Message );
			    return false;
			}
			finally
			{
				this.FireBuildReportsProcessed();

				this.currentFilingSummary.ProcessingTime = DateTime.Now - dtStart;
				this.currentFilingSummary.SaveAsXml( filingSummaryPath );
				filingSummary = this.currentFilingSummary;
				this.currentFilingSummary = null;

				if( this.currentTaxonomy != null )
					this.currentTaxonomy.Close();

				this.currentTaxonomy = null;
			}
		}

		/// <summary>
		/// Checks for any unused elements in the presentation and populates the <paramref name="missingReport" /> variable.
		/// </summary>
		/// <param name="missingReport">The InstanceReport object to populate.</param>
		/// <returns>True on success or false for fail.</returns>
		private bool BuildMissingDataReport( out InstanceReport missingReport )
		{
			missingReport = null;

			if( this.markupCache.Count == this.presentationElements.Count )
				return false;

			missingReport = new InstanceReport();
			missingReport.ReportName = ReportBuilder._uncategorizedLineItems;
			missingReport.ReportLongName = ReportBuilder._uncategorizedLineItems;
			missingReport.Version = this.currentAssemblyVersion;

			List<string> markupElements = new List<string>( this.markupCache.Keys );
			markupElements.Sort();

			foreach( string markupEl in markupElements )
			{
				if( this.presentationElements.ContainsKey( markupEl ) )
					continue;

				InstanceReportRow row = new InstanceReportRow();
				row.BalanceType = "na";
				row.Cells.AddRange( this.markupCache[ markupEl ] );
				row.ElementDataType = "string";
				row.ElementDefenition = null;
				row.ElementReferences = null;
				row.Label = string.Format( "[{0}]", markupEl );
				row.PeriodType = "na";

				if( markupEl.Contains( "_" ) )
					row.ElementPrefix = markupEl.Split( '_' )[ 0 ];

				missingReport.Rows.Add( row );
			}

			if( missingReport.Rows.Count == 0 )
				return false;

			this.BuildReportColumns( missingReport, true );
			ReportBuilder.FillMissingCells( missingReport );

			//InstanceUtils.TOP_LEVEL_REPORT_INDICATOR
			string saveAs = Path.Combine( this.currentReportDirectory, InstanceUtils.TOP_LEVEL_REPORT_INDICATOR + _missingReportIndex + ".xml" );
			missingReport.SaveAsXml( saveAs );
			return true;
		}

		/// <summary>
		/// Checks if this.<see cref="currentTaxonomy"/> is based on any of the published IM taxonomies
		/// </summary>
		/// <returns>True on success or false for fail.</returns>
		private bool CheckIsIMBased()
		{
			int errors;
			ArrayList dependentTaxonomies = this.currentTaxonomy.GetDependantTaxonomies( false, out errors );

			foreach( string taxonomyName in dependentTaxonomies )
			{
				string name = Path.GetFileNameWithoutExtension( taxonomyName );
				//check for old and new GAAP taxonomies
				//TODO: Exract method "IsIMBased"
				if( name.IndexOf( "gaap-ci" ) >= 0 ||
					name.IndexOf( "gaap-im" ) >= 0 ||
					name.IndexOf( "usfr-ime" ) >= 0 ||
					name.IndexOf( "usfr-fste" ) >= 0 ||
					name.IndexOf( "gaap-basi" ) >= 0 ||
					name.IndexOf( "gaap-ins" ) >= 0 ||
					name.IndexOf( "gaap-bd" ) >= 0 ||
					name.IndexOf( "gaap-re" ) >= 0 ||
					name.IndexOf( "usfr-ar" ) >= 0 ||
					name.IndexOf( "usfr-mr" ) >= 0 ||
					name.IndexOf( "usfr-seccert" ) >= 0 ||
					name.IndexOf( "usfr-mda" ) >= 0 ||
					name.IndexOf( "cistm" ) >= 0 ||
					name.IndexOf( "mdastm" ) >= 0 ||
					name.IndexOf( "mda-" ) >= 0 ||
					name.IndexOf( "usgaap-" ) >= 0 )
				{
					if( name.IndexOf( "gaap-im" ) >= 0 ||
						name.IndexOf( "usfr-ime" ) >= 0 ||
						name.IndexOf( "usfr-fste" ) >= 0 )
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Sets the value for multiple collections to null to clear RAM. See this.<see cref="PopulateMarkupDictionaries"/>.  
		/// </summary>
		private void ClearMarkupDictionaries()
		{
			this.columnCache = null;
			this.commonAxes = null;
			this.commonDimensions = null;
			this.elementCache = null;
			this.elementRounding = null;
			this.elementUnitTypes = null;
			this.internalReports = null;
			this.markupCache = null;
		}

		/// <summary>
		/// Sets and counts the different unit types used for an element based on the markup (<paramref name="mp"/>).
		/// </summary>
		/// <param name="predominantElementTypes">Holds the count of element uses for a given unit type.</param>
		/// <param name="mp">An instance of <see cref="MarkupProperty"/> which represents a single fact in the instance document.</param>
		private void CountUnitType( Dictionary<string, Dictionary<UnitType, int>> predominantElementTypes, MarkupProperty mp )
		{
			UnitType unitType = GetUnitType( mp );

			if( !predominantElementTypes.ContainsKey( mp.elementId ) )
				predominantElementTypes[ mp.elementId ] = new Dictionary<UnitType, int>();

			if( !predominantElementTypes[ mp.elementId ].ContainsKey( unitType ) )
				predominantElementTypes[ mp.elementId ][ unitType ] = 0;

			predominantElementTypes[ mp.elementId ][ unitType ]++;
		}

		/// <summary>
		/// Looks up the predefined <see cref="UnitType"/> or custom UnitType (bit value > 128) which is being used on the <paramref name="mp"/> (<see cref="MarkupProperty"/>).
		/// </summary>
		/// <param name="mp"></param>
		/// <returns></returns>
		private UnitType GetUnitType( MarkupProperty mp )
		{
			if( InstanceReportColumn.IsEPSUnit( mp.unitRef ) )
				return UnitType.EPS;

			if( InstanceReportColumn.IsExchangeRateUnit( mp.unitRef ) )
				return UnitType.ExchangeRate;

			if( InstanceReportColumn.IsMonetaryUnit( mp.unitRef ) )
				return UnitType.Monetary;

			if( InstanceReportColumn.IsSharesUnit( mp.unitRef ) )
				return UnitType.Shares;

			if( InstanceReportColumn.IsCustomUnit( mp.unitRef ) )
			{
				int pos = -1;
				string unit = mp.unitRef.StandardMeasure.MeasureValue;
				if( this.customUnits.ContainsKey( unit ) )
				{
					pos = this.customUnits[ unit ];
				}
				else
				{
					pos = 1 << 8;
					pos = pos << this.customUnits.Count;
					this.customUnits[ unit ] = pos;
					this.unitDictionary[ pos ] = unit;
				}

				return (UnitType)pos;
			}

			return UnitType.Other;
		}

		/// <summary>
		/// Crawls a <see cref="DimensionNode"/> (<paramref name="dNode"/>) looking for Segments and storing them in this.<see cref="commonDimensions"/>
		/// </summary>
		/// <param name="dNode">The <see cref="DimensionNode"/> to crawl looking for Segments.</param>
		//[Obsolete]
		private void PopulateCommonDimensions( DimensionNode dNode )
		{
			if( dNode.NodeDimensionInfo != null && dNode.NodeDimensionInfo.NodeType == DimensionNode.NodeType.Item )
			{
				DimensionNode pNode = dNode.GetParentDimensionTypeDimensionNode();
				if( !this.commonDimensions.ContainsKey( pNode.Id ) )
				{
					this.commonAxes.Add( pNode.Id );
					this.commonDimensions[ pNode.Id ] = new List<Segment>();
				}

				Segment segment = new Segment
				{
					DimensionInfo = new ContextDimensionInfo
					{
						dimensionId = pNode.Id,
						Id = dNode.Id
					},
					ValueName = dNode.Label,
					ValueType = pNode.Id
				};

				this.commonDimensions[ pNode.Id ].Add( segment );
			}

			if( dNode.HasChildren && dNode.Children.Count > 0 )
			{
				foreach( DimensionNode child in dNode.Children )
				{
					this.PopulateCommonDimensions( child );
				}
			}
		}

		/// <summary>
		/// Loads data from this.<see cref="currentInstance"/> into dictionaries which will be used across all reports. See this.<see cref="ClearMarkupDictionaries"/>.  
		/// </summary>
		private void PopulateMarkupDictionaries()
		{
			//globals
			this.dar = new DefinitionAndReference();
			this.markupCache = new Dictionary<string, List<Cell>>();
			this.internalReports = new Dictionary<string, int>( StringComparer.CurrentCultureIgnoreCase );

			Dictionary<string,InstanceReportRow> localRowCache = new Dictionary<string, InstanceReportRow>();
			Dictionary<string, Dictionary<UnitType, int>> predominantElementTypes = new Dictionary<string, Dictionary<UnitType, int>>();
			foreach( MarkupProperty mp in this.currentInstance.markups )
			{
				if( mp.unitRef != null )
				{
					switch( mp.unitRef.UnitType )
					{
						case UnitProperty.UnitTypeCode.Standard:
							mp.unitRef.DenominatorMeasure = null;
							mp.unitRef.MultiplyMeasures = null;
							mp.unitRef.NumeratorMeasure = null;
							break;
						case UnitProperty.UnitTypeCode.Multiply:
							mp.unitRef.DenominatorMeasure = null;
							mp.unitRef.NumeratorMeasure = null;
							mp.unitRef.StandardMeasure = null;
							break;
						case UnitProperty.UnitTypeCode.Divide:
							mp.unitRef.MultiplyMeasures = null;
							mp.unitRef.StandardMeasure = null;
							break;
					}
				}

				this.customUnits = new Dictionary<string, int>();
				this.unitDictionary = new Dictionary<int, string>();
				this.unitDictionary[ (int)UnitType.Other ] = "Other";
				this.unitDictionary[ (int)UnitType.Shares ] = "EPS";
				this.unitDictionary[ (int)UnitType.Monetary ] = "Monetary";
				this.unitDictionary[ (int)UnitType.EPS ] = "EPS";
				this.unitDictionary[ (int)UnitType.ExchangeRate ] = "ExchangeRate";

				this.CountUnitType( predominantElementTypes, mp );


				InstanceReportRow row = null;
				Element element = this.currentTaxonomy.AllElements[ mp.elementId ] as Element;
				if( !localRowCache.TryGetValue( mp.elementId, out row ) )
				{
					this.markupCache[ mp.elementId ] = new List<Cell>();
					
					row = new InstanceReportRow();
					if( element != null )
					{
						row.ElementDataType = element.OrigElementType;
						row.SimpleDataType = element.MyDataType.ToString().ToLower();
						localRowCache[ mp.elementId ] = row;

						string def = element.GetDefinition( this.preferredLanguage );
						if( string.IsNullOrEmpty( def ) )
						{
							def = this.GetDocumentationInformation( mp.elementId, this.preferredLanguage );

							if( string.IsNullOrEmpty( def ) )
								def = InstanceUtils._noAuthRefAvailable;
						}


						string @ref = null;
						if( !element.TryGetReferences( out @ref ) || string.IsNullOrEmpty( @ref ) )
						{
							@ref = this.GetReferenceInformation( mp.elementId );

							if( string.IsNullOrEmpty( @ref ) )
								@ref = InstanceUtils._noDefAvailable;
						}

						this.dar.Add( element.Id, def, @ref );
					}
					else
					{
						row.SimpleDataType = "na";
					}
				}

				//populate this.markupCache
				Cell cell = new Cell( mp );
				if( row != null )
				{
					if( row.IsNumericDataType )
					{
						if( decimal.TryParse( cell.NonNumbericText, out cell.NumericAmount ) )
						{
							cell.IsNumeric = true;
							cell.NonNumbericText = string.Empty;
						}
					}
					else if( element != null )
					{
						string newValue;

						if( this.TryGetW3CDuration( element, cell.NonNumbericText, out newValue ) )
						{
							cell.NonNumbericText = newValue;
						}
						else if( cell.NonNumbericText.Contains( "~" ) && EmbedReport.ROLE_COMMAND_SPLITTER.IsMatch( cell.NonNumbericText ) )
						{
							if( EmbedReport.HasMatch( cell.NonNumbericText ) )
							{
								string embedWarning = string.Empty;
								cell.EmbeddedReport = EmbedReport.LoadAndParse( cell.NonNumbericText, out embedWarning );

								if( !string.IsNullOrEmpty( embedWarning ) )
									this.WriteEmbedWarning( row, element, cell, embedWarning );

								if( !this.internalReports.ContainsKey( cell.EmbeddedReport.Role ) )
									this.internalReports[ cell.EmbeddedReport.Role ] = 0;

								this.internalReports[ cell.EmbeddedReport.Role ]++;
							}
							else
							{
								const string INCORRECT_FORMAT_WARNING = @"Warning: Incorrectly formatted embedding in Element ""[Unknown Element]"" with UnitID ""[Unknown UnitID]"" and contextID ""[Unknown ContextID]"".";
								this.WriteEmbedWarning( row, element, cell, INCORRECT_FORMAT_WARNING );
							}
						}
					}
				}

				this.markupCache[ mp.elementId ].Add( cell );
			}


			//now that we have collected all of the types an element is used like,
			//take the most used type and remember it
			this.elementUnitTypes = new Dictionary<string, UnitType>();
			foreach( string elementID in predominantElementTypes.Keys )
			{
				int lastCount = 0;
				foreach( KeyValuePair<UnitType, int> unitType in predominantElementTypes[ elementID ] )
				{
					if( lastCount < unitType.Value )
					{
						lastCount = unitType.Value;
						this.elementUnitTypes[ elementID ] = unitType.Key;
					}
				}
			}


			//populate this.elementRounding
			this.elementRounding = new Dictionary<string, Dictionary<string, Precision>>();
			foreach( KeyValuePair<string, List<Cell>> markups in this.markupCache )
			{
				bool hasSegments;
				int previousPrecision = int.MinValue;
				int previousPrecisionSegmented = int.MinValue;
				this.elementRounding[ markups.Key ] = new Dictionary<string, Precision>();

				Dictionary<int, int> uniqueDecimalValues = new Dictionary<int, int>();
				foreach( Cell cell in markups.Value )
				{
					Precision p = InstanceUtils.WritePrecision( cell.Markup, ref previousPrecision, ref previousPrecisionSegmented, out hasSegments );
					if( p != null )
					{
						uniqueDecimalValues[ p.NumberOfDigits ] = 1;
						if( hasSegments )
							this.elementRounding[ cell.Markup.elementId ][ InstanceUtils._PrecisionSegmented ] = p;
						else
							this.elementRounding[ cell.Markup.elementId ][ InstanceUtils._Precision ] = p;
					}
				}


				if( uniqueDecimalValues.Count > 1 )
				{
					List<int> tmpDecimalValues = new List<int>( uniqueDecimalValues.Keys );
					tmpDecimalValues.Sort();
					tmpDecimalValues.Reverse();

					string[] tmpStrings = new string[ tmpDecimalValues.Count ];
					for( int s = 0; s < tmpStrings.Length; s++ )
					{
						tmpStrings[ s ] = tmpDecimalValues[ s ].ToString();
					}

					Array.Reverse( tmpStrings );
					string decimalAttributeValues = string.Join( " ", tmpStrings );
					string info = string.Format( "Element {0} had a mix of decimals attribute values: {1}.", markups.Key, decimalAttributeValues );
					this.currentFilingSummary.TraceInformation( info );
				}
			}

			//Common dimensions
			this.commonAxes = new List<string>();
			this.commonDimensions = new Dictionary<string,List<Segment>>();

			List<DimensionNode> dimensionNodes = null;
			string labelRole = string.IsNullOrEmpty( this.currentTaxonomy.CurrentLabelRole ) ? "preferredLabel" : currentTaxonomy.CurrentLabelRole;
			if( currentTaxonomy.TryGetCommonDimensionNodesForDisplay( currentTaxonomy.CurrentLanguage, labelRole, true, out dimensionNodes ) )
			{
				ArrayList topNodes = this.currentTaxonomy.GetNodesByPresentation( false, this.ExcludedReports );

				foreach( DimensionNode dNode in dimensionNodes )
				{
					Node pNode = null;
					foreach( Node topNode in topNodes )
					{
						if( string.Equals( topNode.MyPresentationLink.Role, dNode.MyDefinitionLink.Role ) )
						{
							pNode = topNode;
							break;
						}
					}

					if( pNode != null )
						ReportBuilder.GetDimensions( this.currentTaxonomy, pNode, null, ref this.commonAxes, ref this.commonDimensions );
					else
						this.PopulateCommonDimensions( dNode );
				}
			}
		}

		/// <summary>
		/// <para>Writes the <paramref name="embedWarning"/> to the this.<see cref="currentFilingSummary"/>, exchanging templated tokens for their actual values.</para>
		/// <para><paramref name="embedWarning"/> is expected to be in a templated format.</para>
		/// </summary>
		/// <param name="row">Provides the element's PreferredLabelRole.</param>
		/// <param name="element">A taxonomy <see cref="Element"/> allowing us to look up its labels.</param>
		/// <param name="cell">A <see cref="Cell" /> object which provides the context and unit IDs.</param>
		/// <param name="embedWarning">The warning to rewrite.</param>
		/// <returns>The warning with substitution tokens replaced.</returns>
		private string WriteEmbedWarning( InstanceReportRow row, Element element, Cell cell, string embedWarning )
		{
			string labelForWarning = null;
			// Try preferred
			if( !string.IsNullOrEmpty( row.PreferredLabelRole ) )
			{
				element.TryGetLabel( this.preferredLanguage, row.PreferredLabelRole, out labelForWarning );
			}

			// If preferred not available, try default
			if( string.IsNullOrEmpty( labelForWarning ) )
			{
				element.TryGetLabel( this.preferredLanguage, "label", out labelForWarning );
			}

			// If default not available, fall back to element name
			if( string.IsNullOrEmpty( labelForWarning ) )
			{
				labelForWarning = element.Name;
			}

			// Specify warning scope and trace
			embedWarning = embedWarning.Replace( "[Unknown Element]", labelForWarning );
			// Unit refs are not required
			if( cell.Markup.unitRef != null )
			{
				embedWarning = embedWarning.Replace( "[Unknown UnitID]", cell.Markup.unitRef.UnitID );
			}
			else
			{
				embedWarning = embedWarning.Replace( "[Unknown UnitID]", "No UnitID Specified" );
			}
			embedWarning = embedWarning.Replace( "[Unknown ContextID]", cell.Markup.contextRef.ContextID );
			this.currentFilingSummary.TraceWarning( embedWarning );
			return embedWarning;
		}

		/// <summary>
		/// Performs all of the steps necessary to build a raw <paramref name="report"/> for the rendering engine.
		/// </summary>
		/// <param name="topNode">The top-level node from the presentation taxonomy which represents this report.</param>
		/// <param name="report">The report object to populate.</param>
		/// <returns>True on success, false on fail.</returns>
		private bool BuildReport( Node topNode, out InstanceReport report )
		{
			report = new InstanceReport();
			report.ReportLongName = topNode.Label;
			Console.WriteLine( "Building Report: " + report.ReportLongName );

			report.ReportName = ReportHeader.GetShortName( topNode.Label );
			report.ReportName = ReportHeader.RemoveReportNumber( report.ReportName );
			report.ReportName = ReportHeader.RemoveProcessingTokens( report.ReportName );

			if( ReportUtils.IsStatementOfEquityCombined( report.ReportLongName ) )
				report.IsEquityReport = true;

			report.RoleURI = topNode.MyPresentationLink.Role;
			report.TopLevelNode = topNode;
			report.Version = this.currentAssemblyVersion;

			report.IsEmbedReport = this.internalReports.ContainsKey( report.RoleURI );
			report.ShowElementNames = ReportUtils.IsShowElements( report.ReportLongName );

			if( report.ReportName.ToLower().IndexOf( "notes" ) >= 0 )
				report.ReportType = ReportHeaderType.Notes;

			ReportBuilder.GetDimensions( this.currentTaxonomy, topNode, null, ref report.AxisByPresentation, ref report.AxisMembersByPresentation );
			this.BuildReportRows( report, topNode, 0 );

			if( report.Rows.Count == 0 )
				return false;

			this.GetCommonDimensions( ref report.AxisByPresentation, ref report.AxisMembersByPresentation );
			this.GetDimensionDefaults( report, ref report.AxisMemberDefaults );
			this.BuildReportColumns( report );

			if( report.Columns.Count == 0 )
				return false;

			ReportBuilder.FillMissingCells( report );

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.ABSTRACT_HEADER_RULE );
			if( isRuleEnabled )
			{
				this.RemoveChildlessAbstracts( report );
				this.FireRuleProcessed( RulesEngineUtils.ABSTRACT_HEADER_RULE );
			}

			this.ApplyRowPrecision( report );
			this.ApplyFootnotes( report );
			report.HasEmbeddedReports = report.CheckForEmbeddedReports();

			report.SortColumns();
			ReportBuilder.MergeColumns( report );

			return true;
		}

		/// <summary>
		/// Looks at report.Rows checking for sequences of row.IsAbstractGroupTitle, removing "grandparents" without populated child rows.
		/// </summary>
		/// <param name="report">The report to clean.</param>
		private void RemoveChildlessAbstracts( InstanceReport report )
		{
			bool foundAbstract = false;
			for( int r = 0; r < report.Rows.Count; r++ )
			{
				InstanceReportRow cur = report.Rows[ r ];
				if( cur.IsAbstractGroupTitle )
				{
					if( !foundAbstract )
						foundAbstract = true;

					continue;
				}

				if( !foundAbstract )
					continue;

				foundAbstract = false;
				int rowsRemoved = 0;
				for( int a = r - 1; a >= 0; a-- )
				{
					InstanceReportRow prev = report.Rows[ a ];
					if( prev.IsReportTitle ||
						!prev.IsAbstractGroupTitle ||
						!prev.IsEmpty() )
					{
						break;
					}

					if( foundAbstract )
					{
						rowsRemoved++;
						report.Rows.RemoveAt( a );
						continue;
					}
					else if( prev.Level < cur.Level )
					{
						foundAbstract = true;
					}
					else if( prev.Level >= cur.Level )
					{
						rowsRemoved++;
						report.Rows.RemoveAt( a );
						continue;
					}
				}

				r -= rowsRemoved;
				foundAbstract = false;
			}

			if( report.Rows.Count > 0 )
			{
				int testRow = 0;
				if( report.Rows[ 0 ].IsReportTitle )
					testRow++;

				if( report.Rows[ testRow ].IsAbstractGroupTitle )
				{
					if( report.Rows[ testRow ].ElementName.IndexOf( "statementlineitems", StringComparison.CurrentCultureIgnoreCase ) > -1 )
						report.Rows.RemoveAt( testRow );
				}
			}
		}

		/// <summary>
		/// <para>Converts the instance document's footnote IDs to local references, 1, 2, 3, etc...</para>
		/// <para>Also, adds the footnote's text to the <paramref name="report"/>.</para>
		/// </summary>
		/// <param name="report">The report to clean.</param>
		private void ApplyFootnotes( InstanceReport report )
		{
			int fnCounter = 1;
			Dictionary<string, Footnote> uniqueFootnotes = new Dictionary<string, Footnote>();
			foreach( InstanceReportRow row in report.Rows )
			{
				foreach( Cell cell in row.Cells )
				{
					if( string.IsNullOrEmpty( cell.FootnoteIndexer ) )
						continue;

					Dictionary<int, int> cellFootnotes = new Dictionary<int, int>();
					MatchCollection mc = ReportBuilder.FOOTNOTE_MATCH.Matches( cell.FootnoteIndexer );
					foreach( Match m in mc )
					{
						string footnoteID = m.Groups[ 1 ].Value;
						FootnoteProperty fnp = this.currentInstance.fnProperties[ footnoteID ] as FootnoteProperty;
						if( fnp == null )
							continue;
						
						Footnote fn;
						if( !uniqueFootnotes.TryGetValue( fnp.markupData, out fn ) )
						{
							fn = new Footnote( fnCounter, fnp.markupData );
							uniqueFootnotes[ fn.Note ] = fn;
							fnCounter++;
						}

						cellFootnotes[ fn.NoteId ] = 1;					
					}

					//sort and apply footnotes to the cell
					List<int> fnIDs = new List<int>( cellFootnotes.Keys );
					fnIDs.Sort();

					string[] tmp = new string[ fnIDs.Count ];
					for( int i = 0; i < fnIDs.Count; i++ )
					{
						tmp[ i ] = fnIDs[ i ].ToString();
					}

					cell.FootnoteIndexer = string.Format( "[{0}]", string.Join( "],[", tmp ) );
				}
			}

			if( uniqueFootnotes.Count > 0 )
				report.Footnotes.AddRange( uniqueFootnotes.Values );
		}

		/// <summary>
		/// Checks whether <paramref name="report"/> contains segmented data.  If so, it applies the "lower" (most precise) of the segmented and non-segmented precisions, if available.
		/// </summary>
		/// <param name="report"></param>
		private void ApplyRowPrecision( InstanceReport report )
		{
			bool hasSegmentedData = false;
			foreach( InstanceReportColumn col in report.Columns )
			{
				if( col.Segments != null && col.Segments.Count > 0 )
				{
					hasSegmentedData = true;
					break;
				}
			}

			foreach( InstanceReportRow row in report.Rows )
			{
				Dictionary<string, Precision> precisionLookup;
				if( this.elementRounding.TryGetValue( row.ElementName, out precisionLookup ) )
				{
					Precision precision = null;
					precisionLookup.TryGetValue( InstanceUtils._Precision, out precision );

					Precision precisionSegmented = null;
					precisionLookup.TryGetValue( InstanceUtils._PrecisionSegmented, out precisionSegmented );

					if( precision != null )
					{
						row.MyPrecision = precision;

						if( hasSegmentedData && precisionSegmented != null )
						{
							if( precisionSegmented.NumberOfDigits > precision.NumberOfDigits )
								row.MyPrecision = precisionSegmented;
						}
					}
					else if( hasSegmentedData && precisionSegmented != null )
					{
						row.MyPrecision = precisionSegmented;
					}
				}
			}
		}

		/// <summary>
		/// Fills in missing cells so that they align with their columns.  This method assumes all cells and columns have sequential IDs.
		/// </summary>
		/// <param name="report"></param>
		private static void FillMissingCells( InstanceReport report )
		{
			//now sort the cells and fill in the blanks
			foreach( InstanceReportRow row in report.Rows )
			{
				row.Cells.Sort( ( l, r ) => Comparer<int>.Default.Compare( l.Id, r.Id ) );

				//define this outside so we can continue the sequence
				int c;
				for( c = 0; c < row.Cells.Count; c++ )
				{
					int actualID = row.Cells[ c ].Id;
					int expectedID = c + 1;
					for( int i = expectedID; i < actualID; i++ )
					{
						Cell blank = new Cell( i );
						row.Cells.Insert( blank.Id - 1, blank );
					}
				}

				//advance the pointer past our last cell
				c++;
				for( ; c <= report.Columns.Count; c++ )
				{
					Cell blank = new Cell( c );
					row.Cells.Insert( blank.Id - 1, blank );
				}

				Debug.Assert( row.Cells.Count == report.Columns.Count, "Why don't the cell and column counts match?" );
			}
		}

		/// <summary>
		/// Builds the <see cref="InstanceReportColumn"/>s to match all of the cells in this <paramref name="report"/>.
		/// </summary>
		/// <param name="report"></param>
		private void BuildReportColumns( InstanceReport report )
		{
			this.BuildReportColumns( report, false );
		}

		/// <summary>
		/// Builds the <see cref="InstanceReportColumn"/>s to match all of the cells in this <paramref name="report"/>.
		/// </summary>
		/// <param name="report"></param>
		/// <param name="ignorePresentation">When true, checks if a cell's context is valid within the <paramref name="report"/>.  When false, these constraints are not checked.</param>
		private void BuildReportColumns( InstanceReport report, bool ignorePresentation )
		{
			Dictionary<string, int> currencies = new Dictionary<string, int>();

			InstanceReportColumn localColumn;
			Dictionary<string, InstanceReportColumn> unitCache;
			Dictionary<string, Dictionary<string, InstanceReportColumn>> localColumnCache = new Dictionary<string, Dictionary<string, InstanceReportColumn>>();

			for( int r = 0; r < report.Rows.Count; r++ )
			{
				InstanceReportRow row = report.Rows[r];
				if( row.IsAbstractGroupTitle )
					continue;

				for( int c = 0; c < row.Cells.Count; c++ )
				{
					Cell cell = row.Cells[ c ];
					if( ignorePresentation || this.CellExistsInPresentation( report, cell ) )
					{
						cell = (Cell)cell.Clone();
						row.Cells[ c ] = cell;

						if( row.IsNumericDataType )
						{
							if( row.IsReverseSign )
								cell.NumericAmount *= -1;
						}
						else
						{
							string newValue;
							if( this.TryGetQNameValue( row, cell.NonNumbericText, out newValue ) )
								cell.NonNumbericText = newValue;
						}
					}
					else
					{
						row.Cells.RemoveAt( c );
						c--;
						continue;
					}

					//reset these so that the local cache gets populated
					localColumn = null;
					unitCache = null;
					if( !localColumnCache.TryGetValue( cell.ContextID, out unitCache ) || !unitCache.TryGetValue( cell.UnitID, out localColumn ) )
					{
						localColumn = new InstanceReportColumn();
						localColumn.CurrencyCode = InstanceUtils.GetCurrencyCodeFromUnit( cell.Markup.unitRef );
						localColumn.CurrencySymbol = InstanceUtils.GetCurrencySymbolFromCode( localColumn.CurrencyCode );
						localColumn.Id = report.Columns.Count + 1;
						localColumn.MCU = new MergedContextUnitsWrapper( string.Empty, cell.Markup.contextRef, cell.Markup.unitRef );
						localColumn.MCU.OriginalCurrencyCode = localColumn.CurrencyCode;
						localColumn.MCU.CurrencyCode = localColumn.CurrencyCode;
						localColumn.MCU.CurrencySymbol = localColumn.CurrencySymbol;

						InstanceReport.ApplyColumnLabels( report, localColumn );

						//add the column to the report
						report.Columns.Add( localColumn );
						currencies[ localColumn.CurrencyCode ] = 1;

						//add it to the local cache (which represents the report's columns)
						if( unitCache == null )
							localColumnCache[ cell.ContextID ] = new Dictionary<string, InstanceReportColumn>();

						localColumnCache[ cell.ContextID ][ cell.UnitID ] = localColumn;
					}

					cell.Id = localColumn.Id;
				}

				if( row.Cells.Count == 0 )
				{
					if( report.Rows.Contains( row ) )
						report.Rows.Remove( row );

					int r2 = r - 1;
					//we just removed a child row - remove any parent abstracts
					while( 0 < r2 && r2 < report.Rows.Count - 1 )
					{
						InstanceReportRow prevRow = report.Rows[ r2 ];
						if( !prevRow.IsAbstractGroupTitle )
							break;

						if( row.Level <= prevRow.Level )
							break;

						//If the next row starts a new branch, remove this abstract
						InstanceReportRow nextRow = report.Rows[ r2 + 1 ];
						if( nextRow.Level <= prevRow.Level )
						{
							//then remove the previous row
							report.Rows.Remove( prevRow );
							r2--;
						}
						else
						{
							break;
						}
					}

					r = r2;
				}
			}

			if( currencies.ContainsKey( string.Empty ) )
				currencies.Remove( string.Empty );

			if( currencies.ContainsKey( InstanceUtils.USDCurrencyCode ) )
				currencies.Remove( InstanceUtils.USDCurrencyCode );

			report.IsMultiCurrency = currencies.Count > 0;
		}

		/// <summary>
		/// <para>Recursively crawls the child nodes of <paramref name="parent"/>, creating <see cref="InstanceReportRow"/>s for abstract elements and populated elements.</para>
		/// <para>These rows are applied to <paramref name="report"/>.</para>
		/// </summary>
		/// <param name="report">The report which will contain candidate rows.</param>
		/// <param name="parent">The presentation node to be checked: is it abstract? or does it have markups?</param>
		/// <param name="level">The depth of <paramref name="parent"/>.</param>
		/// <returns>The number of "child" rows added for the current <paramref name="parent"/>.</returns>
		private int BuildReportRows( InstanceReport report, Node parent, int level )
		{
			int rowsAdded = 0;

			//apply markups
			InstanceReportRow row;
			if( this.BuildReportRowCells( report, parent, out row ) )
			{
				row.Level = level;
				this.BuildReportRowProperties( report, parent, row );

				if( level == 0 )
				{
					row.IsReportTitle = true;
					row.Label = report.ReportName;
				}

				report.Rows.Add( row );
				rowsAdded++;
			}
			else
			{
				Node dNode;
				if( ReportBuilder.IsDimension( parent, out dNode ) )
					return 0;
			}

			int childRows = 0;
			if( parent.HasChildren )
			{
				foreach( Node child in parent.Children )
				{
					if( !child.IsProhibited )
						childRows += this.BuildReportRows( report, child, level + 1 );
				}

				if( childRows == 0 && row != null && row.IsAbstractGroupTitle )
				{
					//there is no data under this branch, remove it
					report.Rows.Remove( row );

					//remove "row" from the count
					rowsAdded--;
				}
			}

			return rowsAdded + childRows;
		}

		/// <summary>
		/// Updates a <paramref name="row"/> with relevant properties from the taxonomy.
		/// </summary>
		/// <param name="report">[Obsolete]</param>
		/// <param name="parent">The <see cref="Node"/> which created this <paramref name="row"/>.</param>
		/// <param name="row">The row to be updated.</param>
		/// <seealso cref="BuildReportRows"/>
		private void BuildReportRowProperties( InstanceReport report, Node parent, InstanceReportRow row )
		{
			row.BalanceType = parent.BalanceType;
			row.Id = report.Rows.Count + 1;
			row.IsReverseSign = InstanceUtils.IsDisplayReversed( parent );		
			row.PreferredLabelRole = parent.PreferredLabel;
			row.IsBeginningBalance = InstanceUtils.IsBeginningBalance( row );
			row.IsEndingBalance = InstanceUtils.IsEndingBalance( row );
			row.PeriodType = parent.PeriodType;

			//apply the element label
			string label = null;
			if( !string.IsNullOrEmpty( row.PreferredLabelRole ) && parent.TryGetLabel( this.preferredLanguage, row.PreferredLabelRole, out label ) )
				row.Label = label;
			else if( parent.TryGetLabel( this.preferredLanguage, "label", out label ) )
				row.Label = label;
			else
				row.Label = string.Format( "[{0}]", parent.Name );


			//apply element properties
			Element element = parent.MyElement;
			if( element == null && !elementCache.TryGetValue( parent.Id, out element ) )
			{
				element =
				this.elementCache[ parent.Id ] =
					this.currentTaxonomy.AllElements[ parent.Id ] as Element;
			}

			if( element == null )
			{
				Debug.Assert( false, "Why isn't this element in the taxonomy?" );
			}
			else
			{
				if( row.Cells != null && row.Cells.Count > 0 )
				{
					if( !this.presentationElements.ContainsKey( parent.Id ) )
						this.presentationElements[ parent.Id ] = 0;

					this.presentationElements[ parent.Id ]++;
				}

				row.ElementDataType = element.OrigElementType;
				row.ElementName = parent.Id;
				row.ElementPrefix = parent.Id.Substring( 0, parent.Id.IndexOf( '_' ) + 1 );
				row.IsBaseElement = row.ElementPrefix.StartsWith( "usfr_" ) || row.ElementPrefix.StartsWith( "us-gaap_" );
				row.SimpleDataType = element.MyDataType.ToString().ToLower();

				DefAndRef dr;
				if( this.dar.references.TryGetValue( parent.Id, out dr ) )
				{
					row.ElementDefenition = dr.Def;
					row.ElementReferences = dr.Ref;
				}

				UnitType unit;
				if( this.elementUnitTypes.TryGetValue( row.ElementName, out unit ) )
					row.Unit = unit;

				//perhaps the user used the wrong unit - attempt to find monetary, perShare
				if( row.Unit == UnitType.Other || row.Unit == UnitType.Monetary )
				{
					if( element.OrigElementType.IndexOf( "perShare", StringComparison.CurrentCultureIgnoreCase ) > -1 )
						row.Unit = UnitType.EPS;
				}
			}


			if( BuilderRules.IsRuleEnabled( RulesEngineUtils.DISPLAY_US_DATE_FORMAT ) )
			{
				if( string.Equals( row.SimpleDataType, "date", StringComparison.CurrentCultureIgnoreCase ) )
				{
					foreach( Cell cell in row.Cells )
					{
						cell.DisplayDateInUSFormat = true;
					}
				}
			}
		}

		/// <summary>
		/// Applies any markups from the instance document to <paramref name="row"/>
		/// </summary>
		/// <param name="report"></param>
		/// <param name="parent">The <see cref="Node"/> from the presentation taxonomy which the <paramref name="row"/> will represent</param>
		/// <param name="row">The resulting <see cref="InstanceReportRow"/> with cells, unless it <see>IsAbstractGroupTitle</see></param>
		/// <returns>True on success, false on fail.</returns>
		/// <seealso cref="CellExistsInPresentation"/>
		private bool BuildReportRowCells( InstanceReport report, Node parent, out InstanceReportRow row )
		{
			row = null;

			Node dNode;
			List<Cell> cells;
			if( ReportBuilder.IsDimension( parent, out dNode ) )
			{
				//Can a axis/dimension tree have actual elements?
				return false;
			}
			else if( parent.IsAbstract )
			{
				row = new InstanceReportRow();
				row.IsAbstractGroupTitle = true;
				return true;
			}
			else if( this.markupCache.TryGetValue( parent.Id, out cells ) )
			{
				row = new InstanceReportRow();
				row.Cells.AddRange( cells );
				return true;
			}
			else 
			{
				//Debug.Assert( false, "Why is this element in the presentation without values?" );
				return false;
			}
		}

		/// <summary>
		/// Checks <paramref name="report"/>.AxisMembersByPresentation for the segments used by <paramref name="c"/>.Markup
		/// </summary>
		/// <param name="report">The <see cref="InstanceReport"/> providing the axis members.</param>
		/// <param name="c">The cell to check (and thus c.Markup)</param>
		/// <returns>True on success, false on fail.</returns>
		private bool CellExistsInPresentation( InstanceReport report, Cell c )
		{
			foreach( Segment cellSegment in c.Markup.contextRef.Segments )
			{
				List<Segment> members;
				if( !report.AxisMembersByPresentation.TryGetValue( cellSegment.DimensionInfo.dimensionId, out members ) )
				{
					//this member is not in the presentation, therefore this column should not be on this report.
					return false;
				}

				bool found = false;
				foreach( Segment reportSegment in members )
				{
					if( string.Equals( cellSegment.DimensionInfo.Id, reportSegment.DimensionInfo.Id ) )
					{
						cellSegment.ValueName = reportSegment.ValueName;
						found = true;
						break;
					}
				}

				if( !found )
				{
					Element el = this.currentTaxonomy.AllElements[ cellSegment.DimensionInfo.Id ] as Element;
					if( el != null )
					{
						bool wasIdDifferent = !string.Equals( cellSegment.DimensionInfo.Id, el.Id );
						bool isTaxonomyIdSame = string.Equals( cellSegment.DimensionInfo.Id, el.GetNameWithNamespacePrefix() );
						if( isTaxonomyIdSame && wasIdDifferent )
						{
							cellSegment.DimensionInfo.Id = el.Id;
							found = true;
						}
					}
				}

				if( !found )
					return false;
			}

			return true;
		}

		/// <summary>
		/// Applies the global "common dimensions" to the axis and member lists (<paramref name="axisByPresentation"/> and <paramref name="axisMembersByPresentation"/> respectively).
		/// </summary>
		/// <param name="axisByPresentation"></param>
		/// <param name="axisMembersByPresentation"></param>
		private void GetCommonDimensions( ref List<string> axisByPresentation, ref Dictionary<string, List<Segment>> axisMembersByPresentation )
		{
			//apply common dimensions
			string defaultMember = string.Empty;
			foreach( string axis in this.commonAxes )
			{
				defaultMember = string.Empty;
				if( axisByPresentation.Contains( axis ) )
					continue;

				axisByPresentation.Add( axis );

				if( !axisMembersByPresentation.ContainsKey( axis ) )
					axisMembersByPresentation[ axis ] = new List<Segment>();

				if( this.currentTaxonomy != null && this.currentTaxonomy.NetDefinisionInfo != null )
					this.currentTaxonomy.NetDefinisionInfo.TryGetDefaultMember( axis, out defaultMember );

				foreach( Segment commonSegment in this.commonDimensions[ axis ] )
				{
					bool hasMatch = false;
					foreach( Segment inUseSegment in axisMembersByPresentation[ axis ] )
					{
						if( InstanceReportColumn.SegmentEquals( inUseSegment, commonSegment ) )
						{
							hasMatch = true;
							break;
						}
					}

					if( !hasMatch )
					{
						commonSegment.IsDefaultForEntity = string.Equals( commonSegment.DimensionInfo.Id, defaultMember );
						axisMembersByPresentation[ axis ].Add( commonSegment );
					}
				}
			}
		}

		/// <summary>
		/// Finds all default members for each axis-member group in <paramref name="report"/>.<see>AxisMembersByPresentation</see>
		/// </summary>
		/// <param name="report"></param>
		/// <param name="dictionary">The store for default members, keyed by axis.</param>
		private void GetDimensionDefaults( InstanceReport report, ref Dictionary<string, Segment> dictionary )
		{
			foreach( KeyValuePair<string, List<Segment>> kvp in report.AxisMembersByPresentation )
			{
				Segment @default = kvp.Value.Find( seg => seg.IsDefaultForEntity );
				if( @default != null )
					report.AxisMemberDefaults[ kvp.Key ] = @default;
			}
		}

		/// <summary>
		/// Generates HTML report files if allowed by this.<see cref="ReportFormat"/> and as specified by this.<see cref="HtmlReportFormat"/>.
		/// </summary>
		/// <returns>True on success, false on fail.</returns>
		private bool GenerateHtmlFiles()
		{	
            // Use custom XSLT if provided
            string transformFile = String.Empty;
            if (!String.IsNullOrEmpty(this.xsltStylesheetPath))
            {
                transformFile = this.xsltStylesheetPath;
            }
            else
            {
                transformFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.TransformFile );
            }

			if( !File.Exists( transformFile ) )
			{
				Trace.TraceError( "Error: Transform File not found at:\n\t" + transformFile + "\nHtml Conversion aborted." );
				return false;
			}


			bool anyFiles = false;

			//load the transform once for all R-files
			XslCompiledTransform transform = new XslCompiledTransform();
            try
            {
                transform.Load(transformFile);
            }
            catch ( XmlException )
            {
                Trace.TraceError("Error: Transform File contains invalid XML:\n\t" + transformFile + "\nHtml Conversion aborted.");
                return false;
            }
            catch ( XsltException )
            {
                Trace.TraceError("Error: Transform File contains invalid XSLT:\n\t" + transformFile + "\nHtml Conversion aborted.");
                return false;
            }

			XsltArgumentList argList = new XsltArgumentList();
            argList.Clear();
            
            if( this.HtmlReportFormat == HtmlReportFormat.Complete )
            {
                argList.AddParam( "asPage", string.Empty, "true" );

            }

            argList.AddParam( "numberDecimalSeparator", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator );
            argList.AddParam( "numberGroupSeparator", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator );
            argList.AddParam( "numberGroupSize", string.Empty, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSizes[ 0 ].ToString() ); 

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = InstanceReport.Encoding;
			writerSettings.Indent = true;
			writerSettings.OmitXmlDeclaration = true;

			//set intelligent HTML output
			writerSettings.GetType().GetProperty( "OutputMethod" ).SetValue( writerSettings, XmlOutputMethod.Html, null );

			foreach( ReportHeader header in this.currentFilingSummary.MyReports )
			{
				switch( header.ReportType )
				{
					case ReportHeaderType.Notes:
					case ReportHeaderType.Sheet:
						break;
					default:
						continue;
				}


				string inFile = Path.Combine( this.currentReportDirectory, header.XmlFileName );
				if( !File.Exists( inFile ) )
				{
					Trace.TraceWarning( "Warning: The FilingSummary provided '"+ header.XmlFileName +"' but the file was not found." +
						Environment.NewLine +"\tReport skipped: "+ header.LongName );
					continue;
				}

				int extLen = Path.GetExtension( header.XmlFileName ).Length;
				string outFile = inFile.Substring( 0, inFile.Length - extLen ) +".htm";

				using( XmlReader xReader = XmlReader.Create( inFile ) )
				{
					using( FileStream msOut = new FileStream( outFile, FileMode.Create, FileAccess.Write ) )
					{
						try
						{
							using( RivetXmlWriter xWriter = new RivetXmlWriter( msOut, writerSettings ) )
							{
								transform.Transform( xReader, argList, xWriter );
							}

							anyFiles = true;
							header.HtmlFileName = Path.GetFileName( outFile );
						}
						catch( Exception ex )
						{
							Trace.TraceWarning( "Warning: An error occurred while converting '" + header.XmlFileName + "' to HTML." +
								Environment.NewLine + "\tError: " + ex.Message +
								Environment.NewLine + "\tReport skipped: " + header.LongName );
						}
					}
				}
			}

			if( !anyFiles )
				return true;

			string styleSheetTo = Path.Combine( this.currentReportDirectory, RulesEngineUtils.StylesheetFile );
			string stylesheetFrom = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.StylesheetFile );
			FileUtilities.Copy( stylesheetFrom, styleSheetTo );

			string javascriptTo = Path.Combine( this.currentReportDirectory, RulesEngineUtils.JavascriptFile );
			string javascriptFrom = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.JavascriptFile );
			FileUtilities.Copy( javascriptFrom, javascriptTo );
			return true;
		}

		/// <summary>
		/// <para>Loads the configured currencies provided by Currencies.xml</para>
		/// </summary>
		private void LoadCurrencies()
		{
			string currencyCodeFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.None, this.CurrencyMappingFile );
			if( File.Exists( currencyCodeFile ) )
			{
				try
				{
					ISOUtility.GetCurrencyCodes( currencyCodeFile );
				}
				catch( Exception ex )
				{
					Trace.TraceError( "Error: Loading of the Currency Mapping File caused the filing error:" + Environment.NewLine + ex.Message );
				}
			}
			else
			{
				Trace.TraceWarning( "Warning: The Currency Mapping File specified could not be found." + Environment.NewLine +
					"File not found at: " + currencyCodeFile );
			}
		}

		public void SetPreferredLanguage( string lang )
		{
			this.preferredLanguage = lang;
		}

		/// <summary>
		/// Ensures that the settings this.<see cref="ReportFormat"/> and this.<see cref="HtmlReportFormat"/> indicate at least one possible output format.
		/// </summary>
		/// <param name="error">The error message is the output formats conflict</param>
		/// <returns>True on success (no conflict), false on error.</returns>
		private bool ValidateSettings( out string error )
		{
			error = string.Empty;

			if( this.ReportFormat == ReportFormat.None )
			{
				error = "The processing format may not be 'None'.  Set the format to 'Xml', 'Html', or both and try again.";
				return false;
			}

			if( ( ( this.ReportFormat & ReportFormat.Html ) == ReportFormat.Html ) && this.HtmlReportFormat == HtmlReportFormat.None )
			{
				error = "The html report format may not be 'None'.  Set the format to 'Complete' or 'Fragment' and try again.";
				return false;
			}

			if( string.IsNullOrEmpty( this.currentInstancePath ) || !File.Exists( this.currentInstancePath ) )
			{
				error = string.Format( "{0} : {1}", _instanceNotExist, this.currentInstancePath );
				return false;
			}

			if( this.currentTaxonomy == null )
			{
				error = "The taxonomy may not be null.";
				return false;
			}

			return true;
		}


		#endregion

		#region helper functions

		/// <summary>
		/// Checks if this taxonomy is based on us-gaap 2005 standards.
		/// </summary>
		/// <returns>True on success, false on fail.</returns>
		private bool TaxonomyIsGAAP2005()
		{
			return ( !string.IsNullOrEmpty( this.currentFilingSummary.BaseTaxonomyFullPath ) && this.currentFilingSummary.BaseTaxonomyFullPath.IndexOf( "2005" ) >= 0 );
		}

		/// <summary>
		/// Checks if any of the reports in the taxonomy represent an Earnings Release.
		/// </summary>
		/// <returns>True on success, false on fail.</returns>
		private bool HasEarningRelease()
		{
			foreach( ReportHeader rh in this.currentFilingSummary.MyReports )
			{
				if( ReportUtils.IsEarningRelease( rh.ShortName ) )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Loads relevant information from this.<see cref="currentInstance"/> and retains auxiliary files from this.<see cref="currentReportDirectory"/>.
		/// </summary>
		private void BuildFilingSummary()
		{
			InstanceStatistics currentInstanceStat = InstanceUtils.GetStatisticsFromInstance( this.currentInstance );

			this.currentFilingSummary.ReportFormat = this.ReportFormat;
			this.currentFilingSummary.Version = this.currentAssemblyVersion;

			//Deprecated - 2.4.0.2
			//myFilingSummary.FilingDate = FilingDate;
			//myFilingSummary.PeriodEndDate = PeriodEnding;
			//myFilingSummary.TickerSymbol = TickerSymbol;
			//myFilingSummary.AccessionNumber = AccessionNumber;
			//myFilingSummary.FiscalYearEnd = FiscalYearEnd;

			//add the filing package files to the filing summary
			//OR copy the resource into the reports directory
			string instPath = Path.GetDirectoryName( this.currentInstancePath );
			bool reportDirectoryExists = Directory.Exists( this.currentReportDirectory );
			if( Directory.Exists( instPath ) )
			{
				List<string> localTaxonomyFiles = new List<string>();
				if( this.currentTaxonomy != null && this.currentTaxonomy.LinkbaseFileInfos != null )
				{
					foreach( LinkbaseFileInfo linkInfo in currentTaxonomy.LinkbaseFileInfos )
					{
						if( Path.IsPathRooted( linkInfo.Filename ) && File.Exists( linkInfo.Filename ) )
						{
							string tmpName = Path.GetFileName( linkInfo.Filename );
							localTaxonomyFiles.Add( tmpName.ToLower() );
						}
					}
				}

				string prefix = Path.GetFileNameWithoutExtension( this.currentInstancePath );
				string[] packageFiles = Directory.GetFiles( instPath );
				foreach( string path in packageFiles )
				{
					string ext = Path.GetExtension( path ).ToLower();
					string file = Path.GetFileName( path );

					switch( ext )
					{
						case ".xsd":
						case ".xml":
							bool isInstance = false;
							if( !string.IsNullOrEmpty( this.currentInstancePath ) )
								isInstance = this.currentInstancePath.EndsWith( file, StringComparison.CurrentCultureIgnoreCase );

							bool isTaxonomy = false;
							if( !string.IsNullOrEmpty( this.currentTaxonomyPath ) )
								isTaxonomy = this.currentTaxonomyPath.EndsWith( file, StringComparison.CurrentCultureIgnoreCase );

							bool isTaxonomyReference = localTaxonomyFiles.Contains( file.ToLower() );
							if( isInstance || isTaxonomy || isTaxonomyReference )
							{
								//add the filing package files to the filing summary
								this.currentFilingSummary.InputFiles.Add( file );
							}
							else
							{
								goto default;
							}
							break;
						default:
							if( reportDirectoryExists && FilingSummary.IsEdgarAttachmentFile( file ) )
							{
								//copy the resource into the reports directory
								string copyTo = Path.Combine( this.currentReportDirectory, file );
								FileUtilities.Copy( path, copyTo );

								this.currentFilingSummary.SupplementalFiles.Add( file );
							}
							break;
					}
				}
			}

			this.currentFilingSummary.SetStatistics( currentInstanceStat.NumberOfEntities, currentInstanceStat.NumberOfContexts, currentInstanceStat.NumberOfSegments,
				currentInstanceStat.NumberOfScenarios, currentInstanceStat.NumberOfUnitRefs, currentInstanceStat.NumberOfElements,
				currentInstanceStat.HasFootnotes, currentInstanceStat.HasTuples );

			int errors = 0;
			ArrayList dependentTaxonomies = currentTaxonomy.GetDependantTaxonomies( false, out errors );
			foreach( string taxonomyName in dependentTaxonomies )
			{
				string name = Path.GetFileNameWithoutExtension( taxonomyName );
				//check for old and new GAAP taxonomies
				//TODO: Exract method "IsIMBased"
				if( name.IndexOf( "gaap-ci" ) >= 0 ||
					name.IndexOf( "gaap-im" ) >= 0 ||
					name.IndexOf( "usfr-ime" ) >= 0 ||
					name.IndexOf( "usfr-fste" ) >= 0 ||
					name.IndexOf( "gaap-basi" ) >= 0 ||
					name.IndexOf( "gaap-ins" ) >= 0 ||
					name.IndexOf( "gaap-bd" ) >= 0 ||
					name.IndexOf( "gaap-re" ) >= 0 ||
					name.IndexOf( "usfr-ar" ) >= 0 ||
					name.IndexOf( "usfr-mr" ) >= 0 ||
					name.IndexOf( "usfr-seccert" ) >= 0 ||
					name.IndexOf( "usfr-mda" ) >= 0 ||
					name.IndexOf( "cistm" ) >= 0 ||
					name.IndexOf( "mdastm" ) >= 0 ||
					name.IndexOf( "mda-" ) >= 0 ||
					name.IndexOf( "usgaap-" ) >= 0 )
				{
					this.currentFilingSummary.BaseTaxonomies.Add( name );

					if( this.currentFilingSummary.BaseTaxonomyFullPath == null )
						this.currentFilingSummary.BaseTaxonomyFullPath = taxonomyName;
				}
			}

			this.currentFilingSummary.BaseTaxonomies.Sort();
			if( this.currentFilingSummary.BaseTaxonomies.Count == 0 ) //need to go one more level down
			{
			    foreach( string taxonomyFilePath in dependentTaxonomies )
			    {
			        Taxonomy t1 = new Taxonomy();
			        t1.Load( taxonomyFilePath, false );
			    }
			}

			this.currentFilingSummary.HasCalculationLinkbase = currentTaxonomy.HasCalculation;
			this.currentFilingSummary.HasPresentationLinkbase = currentTaxonomy.HasPresentation;
		}

		/// <summary>
		/// Checks if an row's element is a QName type, and attempts to parse the markup value.
		/// </summary>
		/// <param name="row">The row whose element is checked.</param>
		/// <param name="cellText">The markup value to parse as a QName.</param>
		/// <param name="newValue">The result of QName parsing.</param>
		/// <returns>True on success, false on fail.</returns>
		private bool TryGetQNameValue( InstanceReportRow row, string cellText, out string newValue )
		{
			newValue = string.Empty;

			if( row == null )
				return false;

			if( !string.Equals( row.ElementDataType, "xbrli:QNameItemType" ) )
				return false;

			if( !qnameRegex.IsMatch( cellText ) )
				return false;

			string[] prefixElement = cellText.Split( ':' );

			//we'll overwrite 'newValue' soon
			newValue = prefixElement[ 1 ];

            

			string elName = cellText.Replace( ':', '_' );
			foreach( TaxonomyItem ti in this.currentTaxonomy.TaxonomyItems )
			{
				if( string.Equals( ti.Namespace, prefixElement[ 0 ] ) )
				{
					Element nsEl = this.currentTaxonomy.AllElements[ elName ] as Element;
					if( nsEl != null )
					{
						string label;
						if( nsEl.TryGetLabel( this.preferredLanguage, row.PreferredLabelRole, out label ) )
						{
							newValue = label;
						}
						else if( nsEl.TryGetLabel( this.preferredLanguage, "label", out label ) )
						{
							newValue = label;
						}
						else
						{
							newValue = string.Format( "[{0}]", newValue );
						}
					}

					break;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if an markup's element is a duration type, and attempts to parse the markup value from a w3c duration.
		/// </summary>
		/// <param name="el">The <see cref="Element"/> whose type should be checked for duration.</param>
		/// <param name="w3cDuration">The markup value which may or may not be a w3c duration.</param>
		/// <param name="newValue">The verbose representation of the w3c duration.</param>
		/// <returns>True on success or false on fail.</returns>
		/// <see>http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/#duration</see>
		private bool TryGetW3CDuration( Element el, string w3cDuration, out string newValue )
		{
			newValue = string.Empty;

			if( el == null )
				return false;

			bool isDurationElement = string.Equals( el.OrigElementType, "xbrli:durationItemType" ) || string.Equals( el.OrigElementType, "us-types:durationStringItemType" );
			if( !isDurationElement )
				return false;

			if( !w3cDurationRegex.IsMatch( w3cDuration ) )
				return false;

			Match m = w3cDurationRegex.Match( w3cDuration );
			StringBuilder durationString = new StringBuilder();
			if( m.Groups[ "minus" ].Success )
			{
				durationString.Append( "minus " );
			}

			if( m.Groups[ "years" ].Success )
			{
				int years = int.Parse( m.Groups[ "years" ].Value );
				if( years == 1 )
					durationString.AppendFormat( "{0} year ", years );
				else
					durationString.AppendFormat( "{0} years ", years );
			}

			if( m.Groups[ "months" ].Success )
			{
				int months = int.Parse( m.Groups[ "months" ].Value );
				if( months == 1 )
					durationString.AppendFormat( "{0} month ", months );
				else
					durationString.AppendFormat( "{0} months ", months );
			}

			if( m.Groups[ "days" ].Success )
			{
				int days = int.Parse( m.Groups[ "days" ].Value );
				if( days == 1 )
					durationString.AppendFormat( "{0} day ", days );
				else
					durationString.AppendFormat( "{0} days ", days );
			}

			if( m.Groups[ "hours" ].Success )
			{
				int hours = int.Parse( m.Groups[ "hours" ].Value );
				if( hours == 1 )
					durationString.AppendFormat( "{0} hour ", hours );
				else
					durationString.AppendFormat( "{0} hours ", hours );
			}

			if( m.Groups[ "minutes" ].Success )
			{
				int minutes = int.Parse( m.Groups[ "minutes" ].Value );
				if( minutes == 1 )
					durationString.AppendFormat( "{0} minute ", minutes );
				else
					durationString.AppendFormat( "{0} minutes ", minutes );
			}

			if( m.Groups[ "seconds" ].Success )
			{
				decimal seconds = decimal.Parse( m.Groups[ "seconds" ].Value );
				if( seconds == 1 )
					durationString.AppendFormat( "{0} second ", seconds );
				else
					durationString.AppendFormat( "{0} seconds ", seconds );
			}

			newValue = durationString.ToString().TrimEnd();
			if( string.IsNullOrEmpty( newValue ) )
				return false;
			else
				return true;
		}

		public string GetDocumentationInformation( string elementID, string language )
		{
			try
			{
				if( this.currentTaxonomy == null || this.currentTaxonomy.AllElements == null )
					return string.Empty;

				Aucent.MAX.AXE.XBRLParser.Element foundElement = this.currentTaxonomy.AllElements[ elementID ] as Aucent.MAX.AXE.XBRLParser.Element;
				if( foundElement == null || foundElement.LabelInfo == null || foundElement.LabelInfo.Xsd == null )
					return string.Empty;

				string taxonomy = Path.GetFileName( foundElement.LabelInfo.Xsd );
				if( ReportBuilder.defRefHelper == null )
					return string.Empty;

				string definition = ReportBuilder.defRefHelper.GetDefinition( taxonomy, elementID, language );
				return definition;
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.TargetSite.ToString() + ": " + ex.ToString() );
				Debug.WriteLine( ex.TargetSite.ToString() + ": " + ex.ToString() );
			}

			return string.Empty;
		}

		public string GetReferenceInformation( string elementID )
		{
			if( this.currentTaxonomy == null || this.currentTaxonomy.AllElements == null )
				return string.Empty;

			Aucent.MAX.AXE.XBRLParser.Element foundElement = this.currentTaxonomy.AllElements[ elementID ] as Aucent.MAX.AXE.XBRLParser.Element;
			if( foundElement == null || foundElement.LabelInfo == null || foundElement.LabelInfo.Xsd == null )
				return string.Empty;

			string taxonomy = Path.GetFileName( foundElement.LabelInfo.Xsd );
			if( ReportBuilder.defRefHelper == null )
				return string.Empty;

			string references = ReportBuilder.defRefHelper.GetReferences( taxonomy, elementID );
			return references;
		}

		#endregion

		#region helper functions -- embed reports

		/// <summary>
		/// Iterate through report headers to find reports that contain embedded reports for 2nd level processing
		/// </summary>
		private List<ReportHeader> ProcessEmbeddedReports()
		{
			int chartReferenceNumber = 0;

			string reportDirectory = string.Empty;
			List<ReportHeader> reportsToRemove = new List<ReportHeader>();
			foreach( ReportHeader completeReportHeader in this.reportsWithEmbeds )
			{
				string reportPath = Path.Combine( this.currentReportDirectory, completeReportHeader.XmlFileName );
				InstanceReport completeReport = InstanceReport.LoadXml( reportPath );
				completeReport.AxisByPresentation = this.roleAxes[ completeReport.RoleURI ];
				completeReport.AxisMembersByPresentation = this.roleAxisMembers[ completeReport.RoleURI ];
				completeReport.AxisMemberDefaults = this.roleAxisDefaults[ completeReport.RoleURI ];

				foreach( InstanceReportRow row in completeReport.Rows )
				{
					List<Cell> embededCells = row.Cells.FindAll( Cell.IsEmbedded );
					if( embededCells.Count == 0 )
						continue;

					foreach( Cell cell in embededCells )
					{
						ReportHeader embededReportHeader = GetEmbeddedReportHeader( cell.EmbeddedReport.Role );
						if( embededReportHeader == null )
							continue;

						string baseReportPath = Path.Combine( this.currentReportDirectory, embededReportHeader.XmlFileName );
						InstanceReport baseReport = InstanceReport.LoadXml( baseReportPath );
						baseReport.AxisByPresentation = this.roleAxes[ cell.EmbeddedReport.Role ];
						baseReport.AxisMembersByPresentation = this.roleAxisMembers[ cell.EmbeddedReport.Role ];
						baseReport.AxisMemberDefaults = this.roleAxisDefaults[ cell.EmbeddedReport.Role ];
						baseReport.RepairColumnSegmentLabels();

						cell.EmbeddedReport.AxisByPresentation = this.roleAxes[ embededReportHeader.Role ];
						if( cell.EmbeddedReport.ProcessEmbedCommands( baseReport, this.currentFilingSummary ) )
						{
							if( cell.EmbeddedReport.InstanceReport.IsMultiCurrency )
								completeReport.IsMultiCurrency = true;

							cell.EmbeddedReport.InstanceReport.ReportName =
								embededReportHeader.ShortName + Environment.NewLine + cell.EmbeddedReport.InstanceReport.ReportName;
							cell.EmbeddedReport.InstanceReport.RoundingOption = baseReport.RoundingOption;
							this.RelabelEmbeddedReport( cell.EmbeddedReport.InstanceReport );
						}

						SortedDictionary<int, double> barChartData = cell.EmbeddedReport.GenerateBarChartData();
						if( barChartData.Count > 0 )
						{

							string completeFileName;

							do
							{
								chartReferenceNumber++;
								string chartFileName = "BarChart" + chartReferenceNumber;
								completeFileName = Path.Combine( this.currentReportDirectory, chartFileName + ".jpg" );
							}
							while( File.Exists( completeFileName ) );

							if( this.CreateBarChartImageFile( completeFileName, barChartData ) )
							{
								//Do not retain full path which may be a security risk
								string baseName = Path.GetFileName( completeFileName );
								cell.EmbeddedReport.BarChartImageFileName = baseName;
								this.currentFilingSummary.SupplementalFiles.Add( baseName );

#if DEBUG
								cell.EmbeddedReport.InstanceReport.Footnotes.Add(
									new Footnote( 100, cell.EmbeddedReport.InstanceReport.ToHTML() )
								);
#endif
							}
						}

						if( !reportsToRemove.Contains( embededReportHeader ) )
							reportsToRemove.Add( embededReportHeader );
					}
				}

				completeReport.SaveAsXml( reportPath );
			}

			return reportsToRemove;
		}

		/// <summary>
		/// Rebuilds the labels for the columns and rows in <paramref name="instanceReport" />.
		/// </summary>
		/// <param name="instanceReport">The report whose columns and rows should be relabeled.</param>
		private void RelabelEmbeddedReport( InstanceReport instanceReport )
		{
			instanceReport.Columns.ForEach( RelabelItem );
			instanceReport.Rows.ForEach( RelabelGroupingTotalRows );
		}

		/// <summary>
		/// Rebuilds the labels for an <paramref name="item"/> (<see cref="InstanceReportColumn"/> or <see cref="InstanceReportRow"/>) based on its <see>EmbedRequirements</see> (<see cref="ColumnRowRequirement"/>).
		/// </summary>
		/// <param name="item">A column (<see cref="InstanceReportColumn"/>) or row (<see cref="InstanceReportRow"/>).</param>
		private void RelabelItem( InstanceReportItem item )
		{
			string label = ( item.Label ?? string.Empty ).Trim();
			if( !string.IsNullOrEmpty( label ) )
				return;

			if( item.EmbedRequirements == null || item.EmbedRequirements.EmbedCommands == null )
				return;

			foreach( CommandIterator ci in item.EmbedRequirements.EmbedCommands )
			{
				if( ci.Selection != CommandIterator.SelectionType.Axis )
					continue;

				if( ci.Style == CommandIterator.StyleType.NoDisplay )
					continue;

				Segment seg = item.EmbedRequirements.GetMemberKeyValue( ci ).Value as Segment;
				if( seg == null )
					continue;

				string totalLabel;
				if( GetSegmentTotalLabel( seg, out totalLabel ) )
				{
					seg.IsDefaultForEntity = false;
					seg.ValueName = totalLabel;
				}

				//Not sure how to support this yet.
				//
				//if( ci.Style == CommandIterator.StyleType.Grouped )
				//	break;
			}

			List<CommandIterator> hiddenCIs = new List<CommandIterator>();

			try
			{
				//hide the non-segments
				foreach( CommandIterator ci in item.EmbedRequirements.EmbedCommands )
				{
					if( ci.Selection != CommandIterator.SelectionType.Axis )
					{
						hiddenCIs.Add( ci );
						item.EmbedRequirements.HideLabel( ci, null );
					}
				}

				item.GenerateEmbedLabel( item.EmbedRequirements, true );
			}
			finally
			{
				//show them again
				foreach( CommandIterator ci in hiddenCIs )
				{
					item.EmbedRequirements.ShowLabel( ci, null );
				}
			}
		}

		/// <summary>
		/// Looks up the total label (corresponding to totalLabel role) for a segment.
		/// </summary>
		/// <param name="seg">The segment whose total label should be retrieved.</param>
		/// <param name="label">The total label found.</param>
		/// <returns>True on success, false on fail.</returns>
		private bool GetSegmentTotalLabel( Segment seg, out string label )
		{
			label = null;

			string dimensionId = seg.DimensionInfo.Id;
			Element el = this.currentTaxonomy.AllElements[ dimensionId ] as Element;
			if( el == null )
				return false;

			if( !el.TryGetLabel( this.preferredLanguage, "totalLabel", out label ) )
				return false;

			return true;
		}

		/// <summary>
		/// Gets an applies the total label for a row's element and applies it to the row.
		/// </summary>
		/// <param name="row">The row whose element will be checked and whose label will be updated.</param>
		private void RelabelGroupingTotalRows( InstanceReportRow row )
		{
			if( !row.IsGroupTotal )
			    return;

			string label;
			if( GetSegmentTotalLabel( row.GroupTotalSegment, out label ) )
				row.Label = label;
		}

		/// <summary>
		/// Finds the <see cref="ReportHeader"/> from this.<see cref="currentFilingSummary"/>.<see>MyReports</see> based on its role URI.
		/// </summary>
		/// <param name="role">The 'key' used to find a <see cref="ReportHeader"/></param>
		/// <returns>The <see cref="ReportHeader"/> found, or null on fail.</returns>
		private ReportHeader GetEmbeddedReportHeader( string role )
		{
			ReportHeader embededReportHeader = this.currentFilingSummary.MyReports.Find( h => string.Equals( h.Role, role, StringComparison.CurrentCultureIgnoreCase ) );
			return embededReportHeader;
		}

		#endregion
	}
}



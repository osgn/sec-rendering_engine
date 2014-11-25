//=============================================================================
// FilingSummary (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved. 
// This data class stores the summary information for a filing.
// Incldues:
//  Collection of reports
//  Statistical information
//=============================================================================

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.Common.Data;
using System.Collections.Generic;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using System.Diagnostics;
using XBRLReportBuilder.Utilities;
using Aucent.FilingServices.Data;
using System.Text.RegularExpressions;

namespace XBRLReportBuilder
{
	/// <summary>
	/// FilingSummary
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// 
	[Serializable]
	[XmlInclude(typeof(ReportHeader))]

	public class FilingSummary : ITraceMessenger
	{
        public const string FilingSummaryXmlName = "FilingSummary.xml";
        
		#region properties

		public string Version { get; set; }



		//[XmlIgnore]
		public TimeSpan ProcessingTime { get; set; }

		//allow serialization of the ProcessingTime
		//[XmlElement("ProcessingTime")]
		//public string ProcessingTimeString
		//{
		//	get { return ProcessingTime.ToString(); }
		//	set { ProcessingTime = TimeSpan.Parse(value); }
		//}

		//[XmlIgnore]
		//public TimeSpan InstanceLoadTime { get; set; }

		//allow serialization of the InstanceLoadTime
		//[XmlElement( "InstanceLoadTime" )]
		//public string InstanceLoadTimeString
		//{
		//	get { return InstanceLoadTime.ToString(); }
		//	set { InstanceLoadTime = TimeSpan.Parse( value ); }
		//}

		//public int FactCount = 0;

		public ReportFormat ReportFormat { get; set; }


		//Deprecated 2.4.0.2
		//public string ReportType { get; set; }
		//public string FilingDate { get; set; }
		//public string PeriodEndDate { get; set; }
		//public string TickerSymbol { get; set; }
        //public string AccessionNumber { get; set; }
        //public string FiscalYearEnd { get; set; }

		//Deprecated 2.4.0.2
		//private RoundingLevel myRoundingLevel = RoundingLevel.UnKnown;
		//public RoundingLevel MyRoundingLevel
		//{
		//    get { return this.myRoundingLevel; }
		//}

		//Deprecated 2.4.0.2
		//private Boolean sharesShouldBeRounded = true;
		//public Boolean SharesShouldBeRounded
		//{
		//    get { return this.sharesShouldBeRounded; }
		//}

        #region statistics

        /// <summary>
		/// Stores the raw statistics of the instance document
		/// </summary>
		/// 

		public int ContextCount { get; set; }
		public int ElementCount { get; set; }
		public int EntityCount { get; set; }
		public bool FootnotesReported { get; set; }
		public int SegmentCount { get; set; }
		public int ScenarioCount { get; set; }
		public bool TuplesReported { get; set; }
		public int UnitCount { get; set; }

		#endregion



		/// <summary>
		/// Stores the collection of reports in the filing
		/// </summary>
		private List<ReportHeader> reports = new List<ReportHeader>();
		[XmlArrayItem( "Report" )]
		public List<ReportHeader> MyReports
		{
			get { return reports; }
		}



		private List<LogItem> logs = new List<LogItem>();
		[XmlArrayItem( "Log" )]
		public List<LogItem> Logs
		{
			get { return this.logs; }
			set { this.logs = value; }
		}


		private List<string> inputFiles = new List<string>();
		[XmlArrayItem( "File" )]
		public List<string> InputFiles
		{
			get { return this.inputFiles; }
			set { this.inputFiles = value; }
		}

		private List<string> supplementalFiles = new List<string>();
		[XmlArrayItem( "File" )]
		public List<string> SupplementalFiles
		{
			get { return this.supplementalFiles; }
			set { this.supplementalFiles = value; }
		}

		#region taxonomyInfo

		[XmlIgnore]
		public string BaseTaxonomyFullPath { get; set; }

		private ArrayList baseTaxonomies = new ArrayList();
		[XmlArrayItem ("BaseTaxonomy", typeof (string))]
		public ArrayList BaseTaxonomies
		{
			get {return baseTaxonomies;}
			set {baseTaxonomies = value;}
		}

		private bool hasPresentationLinkbase = true;
		public bool HasPresentationLinkbase
		{
			get {return hasPresentationLinkbase;}
			set {hasPresentationLinkbase = value;}
		}

		private bool hasCalculationLinkbase = true;
		public bool HasCalculationLinkbase
		{
			get {return hasCalculationLinkbase;}
			set {hasCalculationLinkbase = value;}
		}

		#endregion

		private FilingSummaryTraceWrapper fstw = null;
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new FilingSummary.
		/// </summary>
		public FilingSummary()
		{
			//this.AttachTraceListener();
		}

		~FilingSummary()
		{
			//this.DetachTraceListener();
		}

		#endregion

		public void AddReports(ReportHeader r)
		{
			this.reports.Add (r);
		}
		
		public void SetStatistics (int entityCount, int contextCount, int segmentCount, int scenarioCount, int unitCount, 
			int elementCount, bool hasFootnotes, bool hasTuples)
		{
			this.ContextCount = contextCount;
			this.ElementCount = elementCount;
			this.EntityCount = entityCount;
			this.FootnotesReported = hasFootnotes;
			this.ScenarioCount = scenarioCount;
			this.SegmentCount = segmentCount;
			this.TuplesReported = hasTuples;
			this.UnitCount = unitCount;
        }

        public void SaveAsXml(string fileName)
		{
			using( StreamWriter sw = new StreamWriter( fileName ) )
			{
				XmlSerializer xmlFilingSummary = new XmlSerializer( this.GetType() );
				xmlFilingSummary.Serialize( sw, this );
			}
		}

		public static FilingSummary Load( string directory )
		{
            string fn = directory + Path.DirectorySeparatorChar + FilingSummaryXmlName;

            if ( !File.Exists( fn ) )
            {
                return null;
            }

			XmlTextReader xmlReader = new XmlTextReader(fn);
			
			XmlSerializer xmlReport = new XmlSerializer(typeof(FilingSummary));

			FilingSummary fs = (FilingSummary)xmlReport.Deserialize(xmlReader);
			xmlReader.Close();       

			return fs;
        }

		private void AttachTraceListener()
		{
			try
			{
				this.fstw = new FilingSummaryTraceWrapper( this );
				Trace.Listeners.Add( this.fstw );
			}
			catch( Exception ex )
			{
				this.fstw = null;
				Trace.TraceWarning( "Warning: Filing Summary could not attach as a Trace Listener: '" + ex.Message + "'" );
			}
		}

		public void DetachTraceListener()
		{
			if( this.fstw != null )
			{
				try
				{
					Trace.Listeners.Remove( this.fstw );
				}
				catch( Exception ex )
				{
					Trace.TraceWarning( "Warning: Filing Summary could not detach its Trace Listener: '" + ex.Message + "'" );
				}
			}
		}

		private void TraceAny( TraceLevel level, string message )
		{
			if( string.IsNullOrEmpty( message ) )
				return;

			lock( this.Logs )
			{
				LogItem log = new LogItem( level, message );
				this.Logs.Add( log );
			}
		}

		public void TraceInformation( string message )
		{
			this.TraceAny( TraceLevel.Info, message );
		}

		public void TraceWarning( string message )
		{
			this.TraceAny( TraceLevel.Warning, message );
		}

		public void TraceError( string message )
		{
			this.TraceAny( TraceLevel.Error, message );
		}

		public void PrependLogs( FilingSummary tmpFS )
		{
			for( int i = 0; i < tmpFS.Logs.Count; i++ )
			{
				LogItem log = tmpFS.Logs[ i ];
				this.Logs.Insert( i, log );
			}
		}

		private static Regex anyLetter = new Regex( @"^[a-z]", RegexOptions.Compiled | RegexOptions.ExplicitCapture ); 
		private static Regex whiteSpace = new Regex( @"\s+", RegexOptions.Compiled | RegexOptions.ExplicitCapture ); 
		public static bool IsEdgarAttachmentFile( string filePath )
		{
			string fileName = Path.GetFileName( filePath );
			if( fileName == null )
				return false;

			//Attachment filenames are up to 32 characters...
			if( fileName.Length > 32 )
				return false;

			//...lowercase...
			string tmpFileName = fileName.ToLower();
			if( !string.Equals( tmpFileName, fileName ) )
				return false;

			//...must start with a letter...
			if( !anyLetter.IsMatch( fileName ) )
				return false;

			//...may have one dash...
			if( fileName.Split( '-' ).Length > 2 )
				return false;

			//...and one underscore...
			if( fileName.Split( '_' ).Length > 2 )
				return false;

			//...but no spaces
			if( whiteSpace.IsMatch( fileName ) )
				return false;

			//Now check the extension
			string ext = Path.GetExtension( fileName );
			switch( ext )
			{
				//case ".dat":
				case ".gif":
				//case ".htm":
				case ".jpg":
				//case ".pdf":
				//case ".txt":
				case ".xml":
				case ".xsd":
					break;
				default:
					return false;
			}

			return true;
		}

		public ReportHeader AddReport( InstanceReport report )
		{
			ReportHeader header = new ReportHeader();
			header.HasEmbeddedReports = report.HasEmbeddedReports;
			header.LongName = report.ReportLongName;
			header.ReportType = report.ReportType;
			header.Role = report.RoleURI;
			header.ShortName = report.ReportName;
			header.TopLevelNode = report.TopLevelNode;

			int reportHeaderIndex = this.MyReports.Count;
			do
			{
				reportHeaderIndex++;
				header.XmlFileName = string.Format( "{0}{1}.xml", InstanceUtils.TOP_LEVEL_REPORT_INDICATOR, reportHeaderIndex );
			}
			while( File.Exists( header.XmlFileName ) );

			this.MyReports.Add( header );
			return header;
		}
		
/*
		public ReportHeader AddReport( InstanceReport report )
		{
			ReportHeader header = AddReport( report.TopLevelNode );
			header.HasEmbeddedReports = report.HasEmbeddedReports;

			report.ReportLongName = header.LongName;
			report.ReportType = header.ReportType;
			report.RoleURI = header.Role;
			report.ReportName = header.ShortName;

			return header;
		}

		public ReportHeader AddReport( Node topLevelNode )
		{
			ReportHeader header = new ReportHeader();

			header.LongName = topLevelNode.Label;

			header.ShortName = ReportHeader.GetShortName( header.LongName );
			header.ShortName = ReportHeader.RemoveReportNumber( header.ShortName );
			header.ShortName = ReportHeader.RemoveProcessingTokens( header.ShortName );

			if( header.ShortName.IndexOf( "notes", StringComparison.CurrentCultureIgnoreCase ) >= 0 )
				header.ReportType = ReportHeaderType.Notes;
			else
				header.ReportType = ReportHeaderType.Sheet;

			header.Role = topLevelNode.MyPresentationLink.Role;
			header.TopLevelNode = topLevelNode;


			int reportHeaderIndex = this.MyReports.Count;
			do
			{
				reportHeaderIndex++;
				header.XmlFileName = string.Format( "{0}{1}.xml", InstanceUtils.TOP_LEVEL_REPORT_INDICATOR, reportHeaderIndex );
			}
			while( File.Exists( header.XmlFileName ) );

			lock( this )
			{
				this.MyReports.Add( header );
			}

			return header;
		}

		public ReportHeader UpdateReport( InstanceReport report )
		{
			ReportHeader header = this.MyReports.Find( h => string.Equals( h.Role, report.RoleURI ) );
			header.HasEmbeddedReports = report.HasEmbeddedReports;

			header.LongName = report.ReportLongName;
			header.ReportType = report.ReportType;
			header.Role = report.RoleURI;
			header.ShortName = report.ReportName;
			return header;
		}

		public void RemoveReport( InstanceReport report )
		{
			ReportHeader header = this.MyReports.Find( h => string.Equals( h.Role, report.RoleURI ) );

			lock( this )
			{
				this.MyReports.Remove( header );
			}
		}
*/
	}
}
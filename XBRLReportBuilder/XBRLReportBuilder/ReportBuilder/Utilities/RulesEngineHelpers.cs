//=============================================================================
// RulesEngineUtils (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This is the utility class that provides methods in interacting with 
// the rule-based engine.
//=============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NxBRE.FlowEngine;
using NxBRE.FlowEngine.Factories;
using NxBRE.FlowEngine.IO;
using Aucent.MAX.AXE.Common.Data.AssemblyIO;
using System.Reflection;
using System.ComponentModel;


namespace XBRLReportBuilder.Utilities
{
	public static class RulesEngineUtils
	{
		[Obfuscation( Exclude = true )]
		public enum ReportBuilderFolder
		{
			None,
			Resources,
			Rules,
			Taxonomy_DocAndRed
		}

		public const string DefaultCurrencyMappingFile = "CurrencyCodes.xml";

		public const string DefaultRulesFile = "ReportBuilder";

		public const string TransformWorkbookFile = "InstanceReport_XmlWorkbook.xslt";

		public const string TransformFile = "InstanceReport.xslt";
		public const string XSLT = "InstanceReport.xslt";

		public const string StylesheetFile = "report.css";
		public const string CSS = "report.css";

		public const string JavascriptFile = "Show.js";
		public const string JS = "Show.js";

		public const string RESOURCES_FOLDER = "Resources";
		public const string RULES_FOLDER = "Rule_Files";
		public const string TAXONOMY_FOLDER = "Taxonomy";

		public const string INSTANCE_AND_DURATION_RULE = "InstantAndDuration";
		public const string ROUNDING_RULE = "Rounding";
		public const string BEGINNING_ENDING_BAL_RULE = "ProcessBeginningEndingBalance";
		public const string PREFERRED_LANGUAGE_RULE = "PreferredLang";
		public const string CLEAN_FLOWTHROUGH_COLUMNS_RULE = "CleanupFlowthroughColumns";
        public const string CLEAN_FLOWTHROUGH_ONLY_FOR_STATEMENT_RULE = "CleanupFlowthroughColumnsOnlyForStatements";
		public const string CLEAN_FLOWTHROUGH_REPORTS_RULE = "CleanupFlowThroughReports";
		public const string PROMOTE_SHARED_LABELS_RULE = "PromoteSharedLabels";
		public const string SEGMENTS_RULE = "Segments";
		public const string COLUMN_HEADERS_RULE = "ColumnHeaders";
		public const string EQUITY_STATEMENT_RULE = "EquityStatement";
        public const string TOTAL_LABEL_RULE = "TotalLabel";
        public const string CURRENCY_SYMBOL_RULE = "CurrencySymbol";
        public const string ABSTRACT_HEADER_RULE = "AbstractHeader";
        public const string GENERATE_EXCEL_RULE = "GenerateExcelFile";
        public const string GENERATE_COMPARISON_RULE = "GenerateRedlineComparisonFile";
        public const string EMBED_REPORTS_RULE = "ProcessEmbeddedReports";
        public const string COMPLETE_REPORT_RULE = "ProcessCompleteReports";
        public const string DISPLAY_US_DATE_FORMAT = "DisplayDateInUSFormat";
        public const string DISPLAY_AS_RATIO = "DisplayAsRatio";
        public const string DISPLAY_ZERO_AS_NONE = "DisplayZeroAsNone";

		public static bool SynchronizeResources()
		{		
			try
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				AssemblyFS asmFS = new AssemblyFS( asm );

				string resourcePath = GetBaseResourcePath();
				if( !Directory.Exists( resourcePath ) )
					Directory.CreateDirectory( resourcePath );

				string error = string.Empty;
				if( asmFS.Synchronize( resourcePath, out error ) )
					return true;
			}
			catch { }

			return false;
		}

		#region getter methods

		private static object resPathLockObject = new object();
		private static string baseResourcePath = null;

		/// <summary>
		/// Sets the base path for any resources required by <see cref="ReportBuilder"/>.
		/// Often used in conjunction with <see cref="ReportBuilder.SetSynchronizedResources"/>
		/// </summary>
		/// <param name="path"></param>
        public static void SetBaseResourcePath(string path)
        {
			lock( resPathLockObject )
			{
				baseResourcePath = path;
			}
        }

		public static string GetBaseResourcePath()
		{
			if( baseResourcePath == null )
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				AssemblyName asmName = asm.GetName();

				string appDataPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				string adRivetPath = Path.Combine( appDataPath, "Rivet" );
				baseResourcePath = Path.Combine( adRivetPath, asmName.Name );
			}

			return baseResourcePath;
		}

		public static string GetResourcePath( ReportBuilderFolder rbFolder, string resourceName )
		{
			switch( rbFolder )
			{
				case ReportBuilderFolder.Resources:
					return GetResourcePath( RESOURCES_FOLDER, resourceName );
				case ReportBuilderFolder.Rules:
					return GetResourcePath( RULES_FOLDER, resourceName );
				case ReportBuilderFolder.Taxonomy_DocAndRed:
					return GetResourcePath( TAXONOMY_FOLDER, resourceName );
				default:
					return GetResourcePath( string.Empty, resourceName );
			}
		}

		public static string GetResourcePath( string resourceFolder, string resourceName )
		{
			string resPath = GetBaseResourcePath();

			if( !string.IsNullOrEmpty( resourceFolder ) )
				resPath = Path.Combine( resPath, resourceFolder );

			if( !string.IsNullOrEmpty( resourceName ) )
				resPath = Path.Combine( resPath, resourceName );

			return resPath;
		}

		public static string GetResourcesFolder()
		{
			return GetResourcePath( RESOURCES_FOLDER, string.Empty );
		}

		public static string GetRulesFolder()
		{
			return GetResourcePath( RULES_FOLDER, string.Empty );
		}

		public static string GetTaxonomyFolder()
		{
			return GetResourcePath( TAXONOMY_FOLDER, string.Empty );
		}

		#endregion

		#region helper methods
		//There are some things that the Rules Engine does not support, like accessing an item in an array.  So
		//these methods were created to wrap that funcationality when needed.

		public static object GetItem(IList collection, int index)
		{
			return collection[index];
		}

		public static bool DictionaryContainsKey(IDictionary dictionary, object key)
		{
			return dictionary.Contains(key);
		}

		public static object GetDictionaryItem(IDictionary dictionary, object key)
		{
			object retValue = null;
			if (dictionary.Contains(key))
			{
				retValue = dictionary[key];
			}
			return retValue;
		}

		#endregion
	}

	/// <summary>
	/// The rules engine does not let you change the value of a base type that is on the context and have that value
	/// persisted after the rule is finished processing, so instead we will wrap the base type in an object and change 
	/// the value of the baset type property in the object.
	/// </summary>
	public class BooleanWrapper
	{
		private bool myValue;
		public bool Value
		{
			get { return myValue; }
			set { myValue = value; }
		}

		public BooleanWrapper()
		{
			this.myValue = false;
		}

		public BooleanWrapper(bool bValue)
		{
			this.myValue = bValue;
		}
	}

	/// <summary>
	/// The rules engine does not let you change the value of a base type that is on the context and have that value
	/// persisted after the rule is finished processing, so instead we will wrap the base type in an object and change 
	/// the value of the baset type property in the object.
	/// </summary>
	public class StringWrapper
	{
		private string myValue;
		public string Value
		{
			get { return myValue; }
			set { myValue = value; }
		}

		public StringWrapper()
		{
			this.myValue = string.Empty;
		}

		public StringWrapper(string strValue)
		{
			this.myValue = strValue;
		}
	}

	public delegate void RuleProcessedHandler( object sender, RuleEventArgs args );
	public class RuleEventArgs : EventArgs
	{
		public string Rule { get; set; }

		public RuleEventArgs( string rule )
			: base()
		{
			this.Rule = rule;
		}
	}

	public delegate void RuleCancelHandler( object sender, RuleCancelEventArgs args );
	public class RuleCancelEventArgs : CancelEventArgs
	{
		public string Rule { get; set; }

		public RuleCancelEventArgs( bool cancel )
			: base( cancel )
		{ }

		public RuleCancelEventArgs( string rule ) : base()
		{
			this.Rule = rule;
		}

		public RuleCancelEventArgs( string rule, bool cancel )
			: base( cancel )
		{
			this.Rule = rule;
		}
	}
}

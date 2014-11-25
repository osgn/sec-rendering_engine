//=============================================================================
// EmbedReport (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class contains information for a report row.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;
using XBRLReportBuilder.Utilities;
using System.Diagnostics;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	/// <summary>
	/// <para>Helper class which contains a valid, parsed "embedding command"</para>
	/// <para>See <see cref="EmbedInstruction" /> (public)</para>
	/// <para>See <see cref="Role"/> (public)</para>
	/// <para>See <see cref="selections"/> (private)</para>
	/// </summary>
    [Serializable]
    [XmlInclude(typeof(CommandIterator))]        
    public class EmbedReport
    {
		public const string COMMAND_DELIMITER = "~";
		private const string ROLE_URI_TOKEN = "roleURI";
		private const string COMMANDS_TOKEN = "rowsAndCols";
		private const string COMMAND_PATTERN = @"\s*(?<" + GROUP_TYPE + @">row|column)\s+(?<" + GROUP_SELECTION + @">\S+)\s+(?<" + GROUP_STYLE + @">\S+)\s+(?<" + GROUP_FILTER + @">(""[^""]*""|'[^']*'|\S+))\s*";

		// The following null regexes are prepared in the static constructor
		public static readonly Regex ROLE_COMMAND_SPLITTER = null;
		private static readonly Regex COMMAND_PARSER = null;
		private static readonly Regex COMMAND_VALIDATOR = null;

		private static readonly Regex WHITE_SPACE = new Regex( @"\s+", RegexOptions.Compiled );

		private const string GROUP_TYPE = "type";
		private const string GROUP_SELECTION = "selection";
		private const string GROUP_STYLE = "style";
		private const string GROUP_FILTER = "filter";

        #region properties

		private InstanceReport baseReport = null;
		private bool hasMultiCurrency = false;

		public string BarChartImageFileName = string.Empty;
		public string EmbedInstruction = string.Empty;
		public bool IsTransposed = false;
		public string Role = string.Empty;

		public InstanceReport InstanceReport = null;

		//[XmlIgnore]
		//public List<string> CommonDimensionRoles = new List<string>();
        public int duprows, dupcols, colcnt, rowcnt, recurs1, recurs2 = 0;

		[XmlIgnore]
		public List<string> AxisByPresentation = new List<string>();

		private Dictionary<string, CommandIterator> selections = new Dictionary<string, CommandIterator>();

		public CommandIterator[] ColumnIterators
		{
			get
			{
				List<CommandIterator> iterators = new List<CommandIterator>();
				foreach( CommandIterator itr in this.selections.Values )
				{
					if( itr.Type == CommandIterator.IteratorType.Column )
						iterators.Add( itr );
				}

				iterators.Sort();
				return iterators.ToArray();
			}
			set
			{
				this.LoadIterators( value );
			}
		}

		public CommandIterator[] RowIterators
		{
			get
			{
				List<CommandIterator> iterators = new List<CommandIterator>();
				foreach( CommandIterator itr in this.selections.Values )
				{
					if( itr.Type == CommandIterator.IteratorType.Row )
						iterators.Add( itr );
				}

				iterators.Sort();
				return iterators.ToArray();
			}
			set
			{
				this.LoadIterators( value );
			}
		}

		private int selectionCount = 0;

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new instance of <see cref="EmbedReport"/>.
        /// </summary>
		public EmbedReport()
		{
			if( !this.selections.ContainsKey( "period" ) )
			{
				/**
				 * If the command has no iterator for “period”, it is assumed as an iterator starting the entire command, “column period compact *”.
				 **/
				this.selections[ "period" ] = CommandIterator.CreateDefaultPeriod();
			}

			//this.selections[ "unit" ] = new CommandIterator( CommandIterator.IteratorType.Column, "unit", CommandIterator.StyleType.Compact, "*" ) );
		}

        /// <summary>
        /// Creates a new instance of <see cref="EmbedReport"/> with
        /// <paramref name="embedInstruction"/> as the embed command.
        /// </summary>
        /// <param name="embedInstruction">The embed instruction.</param>
        public EmbedReport( string embedInstruction ) : this()
        {
			this.EmbedInstruction = embedInstruction;
        }

		static EmbedReport()
		{
			ROLE_COMMAND_SPLITTER = new Regex( @"(" + COMMAND_DELIMITER + @")\s*(?<" + ROLE_URI_TOKEN + @">http\S+)\s+(?<" + COMMANDS_TOKEN + @">.*?)\1",
				RegexOptions.Compiled | RegexOptions.Singleline );

			COMMAND_PARSER = new Regex( COMMAND_PATTERN, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline );
			COMMAND_VALIDATOR = new Regex( @"^(" + COMMAND_PATTERN + ")+$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline );
		}

        #endregion

        #region Methods

		private static void Distribute<T>( ref T[] sourceArray, ref T[] destArray )
		{
			Distribute<T>( false, ref sourceArray, ref destArray );
		}

		private static void Distribute<T>( bool trimValues, ref T[] sourceArray, ref T[] destArray )
		{
			int min = Math.Min( sourceArray.Length, 4 );
			for( int i = 0; i < min; i++ )
			{
				destArray[ i ] = sourceArray[ i ];
			}

			if( trimValues )
			{
				int cpyCount = sourceArray.Length - destArray.Length;
				T[] tmpValues = new T[ cpyCount ];
				Array.Copy( sourceArray, destArray.Length, tmpValues, 0, cpyCount );
				sourceArray = tmpValues;
			}
		}

		private static void DistributeTokens( bool trimValues, ref string[] sourceArray,
			ref string type, ref string cmdSelection, ref string style, ref string cmdFilter )
		{
			string[] destArray = new string[ 4 ];
			Distribute<string>( true, ref sourceArray, ref destArray );

			type = destArray[ 0 ];
			cmdSelection = destArray[ 1 ];
			style = destArray[ 2 ];
			cmdFilter = destArray[ 3 ];
		}


        /// <summary>
        /// Generate bar chart data based on data within the embed report.
        /// </summary>
        /// <returns>A <see cref="SortedDictionary{TKey,TValue}"/> with the
        /// keys being years and values containing data.</returns>
		public SortedDictionary<int, double> GenerateBarChartData()
		{
			SortedDictionary<int, double> barChartData = new SortedDictionary<int, double>();
			if( !IsBarChartReport() )
				return barChartData;

			CommandIterator.IteratorType elPos = this.InstanceReport.GetElementLocation();

			List<InstanceReportItem> items = null;
			if( elPos == CommandIterator.IteratorType.Column )
				items = this.InstanceReport.Columns.ConvertAll( col => (InstanceReportItem)col );
			else
				items = this.InstanceReport.Rows.ConvertAll( row => (InstanceReportItem)row );

			foreach( InstanceReportItem item in items )
			{
				if( item.EmbedRequirements == null )
					continue;

				InstanceReportRow irr = item.EmbedRequirements.ElementRow;
				if( irr.ElementName.IndexOf( "AnnualReturn", StringComparison.CurrentCultureIgnoreCase ) == -1 )
					continue;

				string yearString = irr.ElementName.Substring( irr.ElementName.Length - 4, 4 );
				int yearValue = 0;
				if( !int.TryParse( yearString, out yearValue ) )
					continue;

				Cell[] cells = item.GetCellArray( this.InstanceReport );
				if( cells == null || cells.Length == 0 )
					continue;

				foreach( Cell c in cells )
				{
					if( c.HasData )
					{
						barChartData[ yearValue ] = (double)c.NumericAmount;
						break;
					}
				}
			}

			return barChartData;
		}

		private Dictionary<string, object> GetSelectionMembers(
			IEnumerable<CommandIterator> iteratorHierarchy, CommandIterator currentIterator,
			out bool hasDefaultMember )
		{
			hasDefaultMember = false;
			Dictionary<string, object> memberValues = null;

			switch( currentIterator.Selection )
			{
				//these axis dictionaries are already in order by presentation
				//the biggest challenge comes with the default member when moving data
				case CommandIterator.SelectionType.Axis:
					if( !this.AxisByPresentation.Contains( currentIterator.AxisName ) )
					{
						memberValues = new Dictionary<string, object>();
					}
					else
					{
						Segment defaultMember;
						memberValues = this.baseReport.GetInUseSegmentDictionary( iteratorHierarchy, currentIterator, out defaultMember );
						hasDefaultMember = ( defaultMember != null );
					}
					break;
				case CommandIterator.SelectionType.Element:
					//memberValues = this.baseReport.GetElementDictionary( currentIterator.Type );
					memberValues = this.baseReport.GetInUseElementDictionary( iteratorHierarchy, currentIterator );
					break;
				case CommandIterator.SelectionType.Period:
					memberValues = this.baseReport.GetPeriodDictionary( currentIterator.Type );
					break;
				case CommandIterator.SelectionType.Unit:
					//memberValues = this.baseReport.GetUnitDictionary( currentIterator.Type );
					memberValues = this.baseReport.GetInUseUnitDictionary( iteratorHierarchy, currentIterator );
					break;
			}

			return memberValues;
		}

        /// <summary>
        /// Determine if this report contains bar chart data.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if this embed report
        /// contains bar chart commands.</returns>
		public bool IsBarChartReport()
		{
			bool found = this.Role.IndexOf( "BarChart", StringComparison.CurrentCultureIgnoreCase ) > -1;
			return found;
		}

        /// <summary>
        /// Determine if a specified string contains an embedding command.  This overload does not throw a warning.
        /// </summary>
        /// <param name="embeddingInstruction">The string in which to search for an embedding command.</param>
        /// <returns>A boolean indicating if an embedding command was found.</returns>
        public static bool HasMatch( string embeddingInstruction )
        {
            string embedWarning, roleURI, rowsAndCols;
            return HasMatch( embeddingInstruction, out embedWarning, out roleURI, out rowsAndCols );
        }

		/// <summary>
		/// Determine if a specified string contains an embedding command.  If multiple commands are found, a warning is thrown.
		/// </summary>
		/// <param name="embedWarning">If the function is called successfully, but there were warnings, will contain the warning message(s).</param>
		/// <param name="embeddingInstruction">The string in which to search for an embedding command.</param>
		/// <returns>A boolean indicating if an embedding command was found.</returns>
		public static bool HasMatch(string embeddingInstruction, out string embedWarning )
		{
            embedWarning = string.Empty;

			if( embeddingInstruction == null )
				return false;

			string roleURI, rowsAndCols;
			return HasMatch( embeddingInstruction, out embedWarning, out roleURI, out rowsAndCols );
		}

		/// <summary>
		/// Determine if a specified string contains an embedding command.  If multiple commands are found, a warning is thrown.
		/// </summary>
		/// <param name="embeddingInstruction">The string in which to search for an embedding command.</param>
        /// <param name="embedWarning">If the function is called successfully, but there were warnings, will contain the warning message(s).</param>
        /// <param name="roleURI">If the function is called successfully, contains the role URI of the first located embedding command within the string.</param>
        /// <param name="rowsAndCols">If the function is called successfully, contains the row and column counts of the first located embedding command within the string.</param>
		/// <returns>A boolean indicating if an embedding command was found.</returns>
		private static bool HasMatch( string embeddingInstruction, out string embedWarning, out string roleURI, out string rowsAndCols )
		{

            embedWarning = string.Empty;
			roleURI = string.Empty;
			rowsAndCols = string.Empty;

			if( string.IsNullOrEmpty( embeddingInstruction ) )
				return false;

			if( !embeddingInstruction.Contains( COMMAND_DELIMITER ) )
				return false;

			// Normalize the white space to single regular spaces
			string tmpInstruction = WHITE_SPACE.Replace( embeddingInstruction, " " );

			MatchCollection	m = ROLE_COMMAND_SPLITTER.Matches( embeddingInstruction );
            if( m.Count == 0 )
            {
                return false;
            }
            else if( m.Count > 1 )
            {
                embedWarning = @"Warning: Multiple embeddings in Element ""[Unknown Element]"" with UnitID ""[Unknown UnitID]"" and contextID ""[Unknown ContextID]"". Only the first embed command will be used.";
            }

			string tokenString = m[0].Groups[ COMMANDS_TOKEN ].Value.Trim();
			if( tokenString.Length == 0 )
			{
				roleURI = m[0].Groups[ ROLE_URI_TOKEN ].Value;
				rowsAndCols = string.Empty;
				return true;
			}

			if( COMMAND_VALIDATOR.IsMatch( tokenString ) )
			{
				roleURI = m[0].Groups[ ROLE_URI_TOKEN ].Value;
				rowsAndCols = m[0].Groups[ COMMANDS_TOKEN ].Value;
				return true;
			}

			return false;
		}

        /// <summary>
        /// Load, and then parse, the given <paramref
        /// name="embeddingInstruction"/>.
        /// </summary>
        /// <param name="embeddingInstruction">The embed command to parse.
        /// </param>
        /// <returns>An <see cref="EmbedReport"/> object for the given embed
        /// command.</returns>
        public static EmbedReport LoadAndParse( string embeddingInstruction )
        {
            string embedWarning = string.Empty;
            return EmbedReport.LoadAndParse( embeddingInstruction, out embedWarning );
        }

        /// <summary>
        /// Load, and then parse, the given <paramref
        /// name="embeddingInstruction"/>.
        /// </summary>
        /// <param name="embeddingInstruction">The embed command to parse.
        /// </param>
        /// <param name="embedWarning">A <see cref="string"/> containing any
        /// warnings enountered during parsing.</param>
        /// <returns>An <see cref="EmbedReport"/> object for the given embed
        /// command.</returns>
		public static EmbedReport LoadAndParse( string embeddingInstruction, out string embedWarning )
		{
			EmbedReport report = new EmbedReport( embeddingInstruction );
			if( report.Parse(out embedWarning) )
				return report;
			else
				return null;
		}

        //TODO make private
		public void LoadIterator( CommandIterator iterator )
		{
			if( iterator.Selection == CommandIterator.SelectionType.Axis )
				this.selections[ iterator.SelectionString ] = iterator;
			else if( iterator.Selection == CommandIterator.SelectionType.Element )
				this.selections[ "primary" ] = iterator;
			else
				this.selections[ iterator.Selection.ToString().ToLower() ] = iterator;
		}
        //TODO make private
		public void LoadIterators( IEnumerable<CommandIterator> iterators )
		{
			foreach( CommandIterator iterator in iterators )
			{
				this.LoadIterator( iterator );
			}
		}
        //TODO make private
		public bool Parse( out string embedWarning )
		{
			//As we read in all of our command tokens,
			//there are a couple of things to keep in mind:
			//Walter suggests that commands types have a specific order:
			//    columns then rows.
			//If for any reason we can't discern the IteratorType,
			//the default will be decided based on whether or not we've
			//parsed any 'row' types yet.
			bool foundRows = false;

			string rowsAndCols;
			if( !HasMatch( this.EmbedInstruction, out embedWarning, out this.Role, out rowsAndCols ) )
				return false;

			MatchCollection matches = COMMAND_PARSER.Matches( rowsAndCols );
			foreach( Match m in matches )
			{
				string type = m.Groups[ GROUP_TYPE ].Value;
				string cmdSelection = m.Groups[ GROUP_SELECTION ].Value;
				string style = m.Groups[ GROUP_STYLE ].Value;
				string cmdFilter = m.Groups[ GROUP_FILTER ].Value;

				CommandIterator.IteratorType cmdType = CommandIterator.ParseIteratorType( type, ref foundRows );
				CommandIterator.StyleType cmdStyle = CommandIterator.ParseStyleType( style );

				CommandIterator iterator = new CommandIterator( cmdType, cmdSelection, cmdStyle, cmdFilter );
				this.LoadIterator( iterator );
			}
			return true;
		}

        /// <summary>
        /// Process the embed commands within the given <paramref
        /// name="baseReport"/>.
        /// </summary>
        /// <param name="baseReport">The <see cref="InstanceReport"/> in which
        /// to process embed commands.</param>
        /// <param name="messenger">An <see cref="ITraceMessenger"/> on which
        /// to log messages encountered during processing commands.</param>
        /// <returns></returns>
		public bool ProcessEmbedCommands( InstanceReport baseReport, ITraceMessenger messenger )
		{
			//Fill this in in case bar charts need to generate values
			if( this.IsBarChartReport() )
			{
				//TODO: audit this...
				foreach( InstanceReportRow row in baseReport.Rows )
				{
					row.EmbedRequirements = new ColumnRowRequirement
					{
						ElementRow = (InstanceReportRow)row.Clone()
					};
				}
			}

			if( ReportUtils.IsShowElements( baseReport.ReportLongName ) )
			{
				this.InstanceReport = baseReport;
				return false;
			}

			try
			{
				this.AddDefaultIterators( baseReport );
				if( this.RowIterators.Length == 0 )
				{
					messenger.TraceWarning( "Warning: The embed commands for '" + baseReport.ReportLongName + "' are incomplete." +
						Environment.NewLine + "No 'row' commands were found." );
					this.InstanceReport = baseReport;
					return false;
				}

				if( this.ColumnIterators.Length == 0 )
				{
					messenger.TraceWarning( "Warning: The embed commands for '" + baseReport.ReportLongName + "' are incomplete." +
						Environment.NewLine + "No 'column' commands were found." );
					this.InstanceReport = baseReport;
					return false;
				}


				Console.WriteLine( "Building Embedded Report: " + baseReport.ReportLongName );
				foreach( CommandIterator itr in this.selections.Values )
				{
					Console.WriteLine( "\t"+ itr.ToString() );
				}

				this.IsTransposed = ReportUtils.IsTransposeReport( baseReport.ReportLongName );
				if( this.IsTransposed )
					this.TransposeIterators();

				this.baseReport = baseReport;

				this.InstanceReport = new InstanceReport();
				this.InstanceReport.AxisByPresentation = this.baseReport.AxisByPresentation;
				this.InstanceReport.AxisMembersByPresentation = this.baseReport.AxisMembersByPresentation;
				this.InstanceReport.AxisMemberDefaults = this.baseReport.AxisMemberDefaults;
				this.InstanceReport.IsEquityReport = this.baseReport.IsEquityReport;

				LinkedList<CommandIterator> iterators = new LinkedList<CommandIterator>( this.ColumnIterators );
				foreach( CommandIterator itr in this.RowIterators )
				{
					iterators.AddLast( itr );
				}

				DateTime start = DateTime.Now;
				{
					this.selectionCount = 0;
					this.ProcessIteratorsHierarchically( iterators, messenger );
					TimeSpan ts = DateTime.Now - start;
//					Console.WriteLine( "Sep Recurs: " + recurs1 + " Iter Recurs: " + recurs2 + "..." + this.selectionCount + " selections performed in " + ts.ToString() );
//                    Console.WriteLine("Rows: " + rowcnt + " Cols: " + colcnt + " DupRows: " + duprows + " DupCols: " + dupcols);

					//because the rows might come in out of order, we simply need to correct the order at the last moment.
					this.SortByCommands( this.RowIterators );
                    Console.WriteLine("After SortBYCommands");
                }

				if( this.InstanceReport.Rows.Count == 0 || this.InstanceReport.Columns.Count == 0 )
				{
					this.InstanceReport = baseReport;
					return false;
				}

                this.InstanceReport.PopulateEmbeddedReport( this.baseReport, this.ColumnIterators, this.RowIterators, messenger );
                TimeSpan ts1 = DateTime.Now - start;
                Console.WriteLine("After PopulateEmbeddedReport...Done at " + ts1.ToString());
                if (this.InstanceReport.Rows.Count == 0 || this.InstanceReport.Columns.Count == 0)
				{
					this.InstanceReport = baseReport;
					this.baseReport = null;   //a little cleanup
					return false;
				}
				else
				{
					this.baseReport = null;   //a little cleanup
					return true;
				}
			}
			finally
			{
				this.InstanceReport.ReportLongName = baseReport.ReportLongName;
				this.InstanceReport.ShowElementNames = ReportUtils.IsShowElements( baseReport.ReportLongName );
				this.InstanceReport.RemoveLabelColumn();
#if DEBUG
				this.InstanceReport.Footnotes.Add(
					new Footnote( 99, this.EmbedInstruction.Replace( "<", "&lt;" ).Replace( ">", "&gt;" ) )
				);
#endif
			}
		}

		private void AddDefaultIterators( InstanceReport embedReport )
		{
			//period and primary are added in the constructor and Parse() respectively.
			//Now we look for any additional segments that are not on the ColumnIterators
			//And perhaps also add the units
			Dictionary<string, int> uniqueAxisList = new Dictionary<string, int>();
			foreach( InstanceReportColumn col in embedReport.Columns )
			{
				if( col.Segments == null )
					continue;

				if( col.Segments.Count == 0 )
					continue;

				foreach( Segment s in col.Segments )
				{
					uniqueAxisList[ s.ValueType ] = 1;
				}
			}

			foreach( string axis in this.selections.Keys )
			{
				if( uniqueAxisList.ContainsKey( axis ) )
					uniqueAxisList.Remove( axis );
			}

			List<string> axisList = new List<string>( uniqueAxisList.Keys );
			axisList.Sort( this.AxisSorter );

			foreach( string axis in axisList )
			{
				CommandIterator itr = new CommandIterator
				{
					Type = CommandIterator.IteratorType.Column,
					SelectionString = axis,
					Style = CommandIterator.StyleType.Compact,
					Filter = "*"
				};

				this.selections[ itr.SelectionString ] = itr;
			}

			if( !this.selections.ContainsKey( "primary" ) )
			{
				/**
				 * If the command has no iterator for “primary”, it is assumed that the entire command ends with iterator “row primary compact *”.
				 **/
				this.selections[ "primary" ] = CommandIterator.CreateDefaultPrimary();
			}

			if( !this.selections.ContainsKey( "unit" ) )
			{
				/**
				 * If the command has no iterator for “primary”, it is assumed that the entire command ends with iterator “row primary compact *”.
				 **/
				this.selections[ "unit" ] = CommandIterator.CreateDefaultUnit();
			}
		}

		private int AxisSorter( string leftAxis, string rightAxis )
		{
			bool leftEmpty = leftAxis == null || leftAxis.Trim().Length == 0;
			bool rightEmpty = rightAxis == null || rightAxis.Trim().Length == 0;

			if( leftEmpty )
			{
				if( rightEmpty )
					return 0;
				else
					return 1;
			}

			if( rightEmpty )
				return -1;

			int leftIndex = this.AxisByPresentation.IndexOf( leftAxis );
			int rightIndex = this.AxisByPresentation.IndexOf( rightAxis );
			if( leftIndex < rightIndex )
				return -1;

			if( leftIndex > rightIndex )
				return 1;

			return 0;
		}
        //TODO make private
		public void ProcessIteratorsHierarchically( LinkedList<CommandIterator> iterators, ITraceMessenger messenger )
		{
			Stack<CommandIterator> iteratorHierarchy = new Stack<CommandIterator>();
			ProcessIteratorsHierarchically( iterators.First, iteratorHierarchy, messenger );
		}

		private void ProcessIteratorsHierarchically( LinkedListNode<CommandIterator> iterator, Stack<CommandIterator> iteratorHierarchy, ITraceMessenger messenger )
		{
			if( iterator == null )
			{
				this.selectionCount++;

				Dictionary<CommandIterator.IteratorType, List<CommandIterator>> itrs = new Dictionary<CommandIterator.IteratorType, List<CommandIterator>>();
				itrs[ CommandIterator.IteratorType.Column ] = new List<CommandIterator>();
				itrs[ CommandIterator.IteratorType.Row ] = new List<CommandIterator>();

				Dictionary<CommandIterator.IteratorType, ColumnRowRequirement> reqs = new Dictionary<CommandIterator.IteratorType, ColumnRowRequirement>();
				reqs[ CommandIterator.IteratorType.Column ] = new ColumnRowRequirement();
				reqs[ CommandIterator.IteratorType.Row ] = new ColumnRowRequirement();

				List<CommandIterator> iterators = new List<CommandIterator>( iteratorHierarchy );
				iterators.Reverse();

//                DateTime start = DateTime.Now;
//                Console.WriteLine("Selection: " + this.selectionCount + "  Time: " + start.ToString());

                foreach( CommandIterator iMember in iterators )
                {
                    itrs[iMember.Type].Add(iMember);
   
                    switch (iMember.Selection)
                    {
                        case CommandIterator.SelectionType.Axis:
                            reqs[iMember.Type].Segments.Add((Segment)iMember.TempCurrentMemberValue);
                            break;
                        case CommandIterator.SelectionType.Element:
                            reqs[iMember.Type].ElementRow = (InstanceReportRow)iMember.TempCurrentMemberValue;
                            break;
                        case CommandIterator.SelectionType.Period:
                            reqs[iMember.Type].Period = (CalendarPeriod)iMember.TempCurrentMemberValue;
                            break;
                        case CommandIterator.SelectionType.Unit:
                            reqs[iMember.Type].Unit = (EmbeddedUnitWrapper)iMember.TempCurrentMemberValue;
                            break;
                    }
                }


				//if either the rows or columns are missing, this intersection cannot be created - skip it.
				if( itrs[ CommandIterator.IteratorType.Column ].Count == 0 || itrs[ CommandIterator.IteratorType.Row ].Count == 0 )
					return;

				//write the column
				{
					ColumnRowRequirement colReqs = reqs[ CommandIterator.IteratorType.Column ];
					colReqs.EmbedCommands = itrs[ CommandIterator.IteratorType.Column ].ToArray();

					bool areSame = false;
					//Check if the previous column matches this one - this only works because column iterators are first
					if( this.InstanceReport.Columns.Count > 0 )
					{
						InstanceReportColumn prevCol = this.InstanceReport.Columns[ this.InstanceReport.Columns.Count - 1 ];
						areSame = ColumnRowRequirement.AreSameExceptCurrency( prevCol.EmbedRequirements, colReqs );

						if( areSame )
						{
							//the previous check doesn't check currency - check it here
							areSame = string.Equals( prevCol.EmbedRequirements.UnitCode, colReqs.UnitCode );
                        }
					}

					//If they are the same, skip the new one - we don't want or need duplicates
                    if (!areSame)
                    {
                        InstanceReportColumn col = new InstanceReportColumn();
                        col.EmbedRequirements = colReqs;
                        col.HasMultiCurrency = this.hasMultiCurrency;

                        if (col.EmbedRequirements.HasSelectionType(CommandIterator.SelectionType.Period))
                        {
                            col.MCU = new MergedContextUnitsWrapper();
                            col.MCU.CurrencyCode = string.Empty;
                            col.MCU.CurrencySymbol = string.Empty;
                            col.MCU.contextRef = new ContextProperty();

                            if (col.EmbedRequirements.Period.PeriodType == Element.PeriodType.instant)
                            {
                                col.MCU.contextRef.PeriodType = Element.PeriodType.instant;
                                col.MCU.contextRef.PeriodStartDate = col.EmbedRequirements.Period.StartDate;
                            }
                            else if (col.EmbedRequirements.Period.PeriodType == Element.PeriodType.duration)
                            {
                                col.MCU.contextRef.PeriodType = Element.PeriodType.duration;
                                col.MCU.contextRef.PeriodStartDate = col.EmbedRequirements.Period.StartDate;
                                col.MCU.contextRef.PeriodEndDate = col.EmbedRequirements.Period.EndDate;
                            }
                 
                        }
                        colcnt++;
//                        Console.WriteLine("New Col #: " + colcnt);
                        this.InstanceReport.Columns.Add(col);
                    }
                    else
                    {
                        dupcols++;
//                        Console.WriteLine("Duplicate Column #: " + dupcols);
                    }
				}


				//write the row
				{
					ColumnRowRequirement rowReqs = reqs[ CommandIterator.IteratorType.Row ];
					rowReqs.EmbedCommands = itrs[ CommandIterator.IteratorType.Row ].ToArray();

					bool areSame = false;
					//Check if ANY of the previous rows matches this one...
					if( this.InstanceReport.Rows.Count > 0 )
					{
                        foreach (InstanceReportRow prevRow in this.InstanceReport.Rows)
                        {
                            areSame = ColumnRowRequirement.AreSameExceptCurrency(prevRow.EmbedRequirements, rowReqs);

                            if (areSame)
                            {
                                //the previous check doesn't check currency - check it here
                                areSame = string.Equals(prevRow.EmbedRequirements.UnitCode, rowReqs.UnitCode);
                            }

                            //let `areSame` fall through, and this row will not get picked up
                            if (areSame)
                            {
                               break;
                            }
                        }
//						InstanceReportRow prevRow = this.InstanceReport.Rows[this.InstanceReport.Rows.Count - 1 ]; 
//						areSame = ColumnRowRequirement.AreSameExceptCurrency( prevRow.EmbedRequirements, rowReqs );
//                        if ( areSame )
//    					{
//							//the previous check doesn't check currency - check it here
//							areSame = string.Equals( prevRow.EmbedRequirements.UnitCode, rowReqs.UnitCode );
//						}

						//let `areSame` fall through, and this row will not get picked up
	//					if( areSame )
	//						break;
	
	    			}

					//If they are the same, skip the new one - we don't want or need duplicates
                    if (!areSame)
                    {
                        InstanceReportRow row = new InstanceReportRow();
                        row.EmbedRequirements = rowReqs;
                        row.HasMultiCurrency = this.hasMultiCurrency;

                        if (row.EmbedRequirements.ElementRow != null)
                            row.EmbedRequirements.ElementRow.ApplyDataTo(row);
                        rowcnt++;
//                        Console.WriteLine("New Row #: " + rowcnt);
                        this.InstanceReport.Rows.Add(row);
                    }
                    else
                    {
                        duprows++;
//                        Console.WriteLine("Duplicate Row #: " + duprows);
                    }                       
				}
			}
			else if( iterator.Value.Selection == CommandIterator.SelectionType.Separator )
			{
				iteratorHierarchy.Push( iterator.Value );
//                Console.WriteLine("Separator Recurse: ");
                recurs1++;
                ProcessIteratorsHierarchically(iterator.Next, iteratorHierarchy, messenger);
				iteratorHierarchy.Pop();
			}
			else
			{
				bool hasDefaultMember;
				Dictionary<string, object> memberValues = this.GetSelectionMembers( iteratorHierarchy, iterator.Value, out hasDefaultMember );
				if( memberValues.Count == 0 )
				{
					if( messenger != null )
					{
						messenger.TraceWarning( "Warning: The selection of the following command, '" + iterator.Value.SelectionString + "', did not match any values available on this report:" +
							Environment.NewLine + iterator.Value.ToString() );
					}
					return;
				}

				switch( iterator.Value.Selection )
				{
					case CommandIterator.SelectionType.Axis:
						if( !hasDefaultMember )
						{
							iterator.Value.Style = CommandIterator.StyleType.Compact;
						}
						break;
					case CommandIterator.SelectionType.Unit:
						if( memberValues.Count > 1 )
						{
							if( memberValues.ContainsKey( string.Empty ) && memberValues.Count == 2 )
								this.hasMultiCurrency = false;
							else
								this.hasMultiCurrency = true;
						}
						break;
				}

				int valuesFound = 0;
				foreach( KeyValuePair<string, object> member in memberValues )
				{
					if( string.Equals( iterator.Value.Filter, "*" ) ||
						string.Equals( iterator.Value.Filter, member.Key, StringComparison.CurrentCultureIgnoreCase ) )
					{
						valuesFound++;

						iteratorHierarchy.Push( iterator.Value );
						iterator.Value.TempCurrentMemberKey = member.Key;
						iterator.Value.TempCurrentMemberValue = member.Value;
//                        Console.WriteLine("Iterator Recurse " + iterator.Value + "  " + member.Key + "  " + member.Value);
                        recurs2++;
						ProcessIteratorsHierarchically( iterator.Next, iteratorHierarchy, messenger );

						iterator.Value.TempCurrentMemberKey = null;
						iterator.Value.TempCurrentMemberValue = null;
						iteratorHierarchy.Pop();
					}
				}

				if( valuesFound == 0 )
				{
					if( messenger != null )
					{
						messenger.TraceWarning( "Warning: The filter '" + iterator.Value.Filter + "' of following embed command did not match any values available on this report:" +
							Environment.NewLine + iterator.Value.ToString() );
					}
				}
			}
		}

		private delegate int ItemSorter( InstanceReportItem left, InstanceReportItem right );
		private bool SortByCommands( CommandIterator[] commands )
		{
			if( commands == null || commands.Length == 0 )
				return false;

			CommandIterator.IteratorType type = commands[ 0 ].Type;

			//sorting may only be performed on rows or columsn.  'Unknown' is not allowed.
			if( type == CommandIterator.IteratorType.Unknown )
				return false;

			//sorting may only be performed on items of a consistent type (rows or columns)
			if( !Array.TrueForAll( commands, c => c.Type == type ) )
				return false;

			//collect all of the possible values in command order
			List<string>[] commandValueKeys = new List<string>[ commands.Length ];

			CommandIterator[] emptySet = new CommandIterator[ 0 ];
			for( int i = 0; i < commands.Length; i++ )
			{
				//we can't compare a separator
				if( commands[ i ].Selection == CommandIterator.SelectionType.Separator )
				{
					commandValueKeys[ i ] = null;
				}
				else
				{
					bool hasDefaultMember;
					Dictionary<string, object> tmp = this.GetSelectionMembers( emptySet, commands[ i ], out hasDefaultMember );
					List<string> sortedKeys = new List<string>( tmp.Keys );
					commandValueKeys[ i ] = sortedKeys;
				}
			}

			ItemSorter sorter = ( left, right ) =>
			{
				for( int i = 0; i < commands.Length; i++ )
				{
					//we can't compare a separator
					if( commands[ i ].Selection == CommandIterator.SelectionType.Separator )
						continue;

					List<string> valueKeys = commandValueKeys[ i ];

					int leftIdx = 0;
					int rightIdx = 0;

					switch( commands[ i ].Selection )
					{
						case CommandIterator.SelectionType.Axis:
							string leftSegment = ( (Segment)left.EmbedRequirements.GetMemberKeyValue( commands[ i ] ).Value ).DimensionInfo.Id;
							string rightSegment = ( (Segment)right.EmbedRequirements.GetMemberKeyValue( commands[ i ] ).Value ).DimensionInfo.Id;

							leftIdx = valueKeys.IndexOf( leftSegment );
							rightIdx = valueKeys.IndexOf( rightSegment );
							break;
						case CommandIterator.SelectionType.Element:
							leftIdx = valueKeys.IndexOf( left.EmbedRequirements.ElementKey );
							rightIdx = valueKeys.IndexOf( right.EmbedRequirements.ElementKey );
							break;
						case CommandIterator.SelectionType.Period:
							leftIdx = valueKeys.IndexOf( left.EmbedRequirements.PeriodLabel );
							rightIdx = valueKeys.IndexOf( right.EmbedRequirements.PeriodLabel );
							break;
						case CommandIterator.SelectionType.Unit:
							leftIdx = valueKeys.IndexOf( left.EmbedRequirements.UnitCode );
							rightIdx = valueKeys.IndexOf( right.EmbedRequirements.UnitCode );
							break;
					}

					if( leftIdx < rightIdx )
						return -1;

					if( leftIdx > rightIdx )
						return 1;
				}

				return 0;
			};

			if( type == CommandIterator.IteratorType.Column )
				this.InstanceReport.Columns.Sort( (left, right ) => sorter( left, right ) );
			else
				this.InstanceReport.Rows.Sort( (left, right ) => sorter( left, right ) );

			return true;
		}

		private void TransposeIterators()
		{
			foreach( CommandIterator itr in this.selections.Values )
			{
				itr.Type = itr.Type == CommandIterator.IteratorType.Column ?
					CommandIterator.IteratorType.Row :
					CommandIterator.IteratorType.Column;
			}
		}

		#endregion
	}
}

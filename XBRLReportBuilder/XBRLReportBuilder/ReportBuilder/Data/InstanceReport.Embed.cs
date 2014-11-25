using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using System.Diagnostics;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Collections.Specialized;

namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		public CommandIterator.IteratorType GetElementLocation()
		{
			foreach( InstanceReportItem item in this.Columns )
			{
				if( item.EmbedRequirements == null )
					continue;

				if( !string.IsNullOrEmpty( item.EmbedRequirements.ElementId ) )
					return CommandIterator.IteratorType.Column;

				//because every "EmbedRequirement" should have this value populated
				//only check the first one
				break;
			}

			return CommandIterator.IteratorType.Row;
		}

		public CommandIterator.IteratorType GetPeriodLocation()
		{
			foreach( InstanceReportItem item in this.Columns )
			{
				//this instance report is the default
				if( item.EmbedRequirements == null )
					return CommandIterator.IteratorType.Column;

				//this instance report is extended
				if( item.EmbedRequirements.Period != null )
					return CommandIterator.IteratorType.Column;

				//because every "EmbedRequirement" should have this value populated
				//only check the first one
				break;
			}

			return CommandIterator.IteratorType.Row;
		}

		public CommandIterator.IteratorType GetUnitLocation()
		{
			foreach( InstanceReportItem item in this.Rows )
			{
				if( item.EmbedRequirements == null )
					continue;

				if( item.EmbedRequirements.Unit == null )
					continue;

				return CommandIterator.IteratorType.Row;
			}

			return CommandIterator.IteratorType.Column;
		}

		public Dictionary<string, object> GetElementDictionary( CommandIterator.IteratorType iteratorType )
		{
			//based on the iteratorType, this may affect the sorting
			Dictionary<string, object> elementDictionary = new Dictionary<string, object>();

			foreach( InstanceReportRow row in this.Rows )
			{
				if( string.IsNullOrEmpty( row.ElementName ) )
					continue;
				
				if( !this.IsEquityReport && row.IsAbstractGroupTitle )
				    continue;

				string elementKey = row.ElementKey;
				if( !elementDictionary.ContainsKey( elementKey ) )
				{
					InstanceReportRow tmpRow = (InstanceReportRow)row.Clone( false, false );
					elementDictionary[ elementKey ] = tmpRow;
				}
			}

			return elementDictionary;
		}

		public Dictionary<string, object> GetInUseElementDictionary( IEnumerable<CommandIterator> iteratorHierarchy, CommandIterator currentIterator )
		{
			//based on the iteratorType, this may affect the sorting
			Dictionary<string, object> elementDictionary = new Dictionary<string, object>();

			foreach( InstanceReportRow row in this.Rows )
			{
				if( string.IsNullOrEmpty( row.ElementName ) )
					continue;

				if( !this.IsEquityReport && row.IsAbstractGroupTitle )
					continue;

				if( elementDictionary.ContainsKey( row.ElementKey ) )
					continue;

				for( int c = 0; c < this.Columns.Count; c++ )
				{
					if( this.Columns[ c ].ContainsSegmentMembers( iteratorHierarchy ) )
					{
						InstanceReportRow tmpRow = (InstanceReportRow)row.Clone( false, false );
						elementDictionary[ row.ElementKey ] = tmpRow;
						break;
					}
				}
			}

			return elementDictionary;
		}

		public Dictionary<string, object> GetPeriodDictionary( CommandIterator.IteratorType iteratorType )
		{
			List<CalendarPeriod> uniqueCalendars = new List<CalendarPeriod>();
			this.Columns.ForEach(
			col =>
			{
				CalendarPeriod cp = new CalendarPeriod( col.MyContextProperty.PeriodStartDate, col.MyContextProperty.PeriodEndDate );
				cp.PeriodType = col.MyContextProperty.PeriodType;

				int foundAt = uniqueCalendars.BinarySearch( cp );
				if( foundAt < 0 )
					uniqueCalendars.Insert( ~foundAt, cp );
			} );

			if( uniqueCalendars.Count > 1 )
			{
				uniqueCalendars.RemoveAll(
				iCal =>
				{
					if( iCal.PeriodType == Element.PeriodType.duration )
						return false;

					return ReportUtils.Exists( uniqueCalendars, dCal => DateTime.Equals( iCal.StartDate, dCal.EndDate ) );
				} );
			}

			if( uniqueCalendars.Count > 1 )
			{
				if( iteratorType == CommandIterator.IteratorType.Column )
					uniqueCalendars.Sort( CalendarPeriod.ReportSorter );
				else
					uniqueCalendars.Sort( CalendarPeriod.AscendingSorter );
			}

			Dictionary<string, object> periodDictionary = new Dictionary<string, object>();
			foreach( CalendarPeriod cp in uniqueCalendars )
			{
				periodDictionary[ cp.ToString() ] = cp;
			}
			return periodDictionary;
		}

		public Dictionary<string, object> GetInUseSegmentDictionary(
			IEnumerable<CommandIterator> iteratorHierarchy, CommandIterator currentIterator,
			out Segment defaultMember )
		{
			string axis = currentIterator.AxisName;
			defaultMember = this.AxisMemberDefaults.ContainsKey( axis ) ?
				this.AxisMemberDefaults[ axis ] : null;

			Segment colSeg = null;
			Dictionary<string, object> segmentDictionary = new Dictionary<string, object>();
			if( string.Equals( currentIterator.Filter, "*" ) )
			{
				foreach( InstanceReportColumn col in this.Columns )
				{
					colSeg = col.GetAxisMember( axis, defaultMember );
					if( colSeg == null )
						continue;

					if( segmentDictionary.ContainsKey( colSeg.DimensionInfo.Id ) )
						continue;

					if( col.ContainsSegmentMembers( iteratorHierarchy ) )
						segmentDictionary[ colSeg.DimensionInfo.Id ] = colSeg;
				}

				Segment[] allSegments = new Segment[ segmentDictionary.Count ];
				segmentDictionary.Values.CopyTo( allSegments, 0 );
				Array.Sort( allSegments, this.CompareSegments );

				segmentDictionary.Clear();
				foreach( Segment seg in allSegments )
				{
					segmentDictionary[ seg.DimensionInfo.Id ] = seg;
				}
			}
			else
			{
				foreach( InstanceReportColumn col in this.Columns )
				{
					colSeg = col.GetAxisMember( axis, defaultMember );
					if( colSeg != null && string.Equals( currentIterator.Filter, colSeg.DimensionInfo.Id ) )
					{
						segmentDictionary[ colSeg.DimensionInfo.Id ] = colSeg;
						break;
					}
				}
			}

			return segmentDictionary;
		}

		public Dictionary<string, object> GetUnitDictionary( CommandIterator.IteratorType iteratorType )
		{
			//based on the iteratorType, this may affect the sorting
			SortedDictionary<string, EmbeddedUnitWrapper> currencyCodes = new SortedDictionary<string, EmbeddedUnitWrapper>();
			foreach( InstanceReportColumn col in this.Columns )
			{
				UnitProperty up;
				string cc = GetColumnCurrencyCode( col, out up );
				if( currencyCodes.ContainsKey( cc ) )
					currencyCodes[ cc ].AddOtherUnits( col.Units );
				else
					currencyCodes[ cc ] = new EmbeddedUnitWrapper( up, col.Units );
			}

			//put USD first
			Dictionary<string, object> unitDictionary = new Dictionary<string, object>();
			if( currencyCodes.ContainsKey( InstanceUtils.USDCurrencyCode ) )
			{
				unitDictionary[ InstanceUtils.USDCurrencyCode ] = currencyCodes[ InstanceUtils.USDCurrencyCode ];
				currencyCodes.Remove( InstanceUtils.USDCurrencyCode );
			}

			//the rest in alphabetical order
			foreach( string key in currencyCodes.Keys )
			{
				unitDictionary[ key ] = currencyCodes[ key ];
			}

			return unitDictionary;
		}

		public Dictionary<string, object> GetInUseUnitDictionary( IEnumerable<CommandIterator> iteratorHierarchy, CommandIterator currentIterator )
		{
			//based on the iteratorType, this may affect the sorting
			SortedDictionary<string, EmbeddedUnitWrapper> currencyCodes = new SortedDictionary<string, EmbeddedUnitWrapper>();
			foreach( InstanceReportColumn col in this.Columns )
			{
				if( !col.ContainsSegmentMembers( iteratorHierarchy ) )
					continue;

				UnitProperty up;
				string cc = GetColumnCurrencyCode( col, out up );
				if( currencyCodes.ContainsKey( cc ) )
					currencyCodes[ cc ].AddOtherUnits( col.Units );
				else
					currencyCodes[ cc ] = new EmbeddedUnitWrapper( up, col.Units );
			}

			//put USD first
			Dictionary<string, object> unitDictionary = new Dictionary<string, object>();
			if( currencyCodes.ContainsKey( InstanceUtils.USDCurrencyCode ) )
			{
				unitDictionary[ InstanceUtils.USDCurrencyCode ] = currencyCodes[ InstanceUtils.USDCurrencyCode ];
				currencyCodes.Remove( InstanceUtils.USDCurrencyCode );
			}

			//the rest in alphabetical order
			foreach( string key in currencyCodes.Keys )
			{
				unitDictionary[ key ] = currencyCodes[ key ];
			}

			return unitDictionary;
		}

		public void PopulateEmbeddedReport( InstanceReport baseReport,
			CommandIterator[] columnIterators, CommandIterator[] rowIterators )
		{
			this.PopulateEmbeddedReport( baseReport, columnIterators, rowIterators, null );
		}

		public void PopulateEmbeddedReport( InstanceReport baseReport,
			CommandIterator[] columnIterators, CommandIterator[] rowIterators, ITraceMessenger messenger )
		{
			this.IsMultiCurrency = baseReport.IsMultiCurrency;
			foreach( InstanceReportColumn col in this.Columns )
			{
				col.GenerateEmbedLabel();
			}

			foreach( InstanceReportRow row in this.Rows )
			{
				row.Cells.Clear();
				row.GenerateEmbedLabel();

				for( int c = 0; c < this.Columns.Count; c++ )
					row.Cells.Add( new Cell() );
			}

			ColumnRowRequirement[,] netCells = new ColumnRowRequirement[ this.Rows.Count, this.Columns.Count ];
			for( int r = 0; r < this.Rows.Count; r++ )
			{
				for( int c = 0; c < this.Columns.Count; c++ )
				{
					netCells[ r, c ] = BuildNetCell( this.Rows[ r ].EmbedRequirements, this.Columns[ c ].EmbedRequirements );
				}
			}

			for( int r = 0; r < this.Rows.Count; r++ )
			{
				for( int c = 0; c < this.Columns.Count; c++ )
				{
					List<InstanceReportColumn> baseColumns = FindBaseColumns( netCells[ r, c ], baseReport.Columns );
					if( baseColumns.Count == 0 )
						continue;

					List<InstanceReportRow> baseRows = FindBaseRows( netCells[ r, c ], baseReport.Rows );
					if( baseRows.Count == 0 )
						continue;

					bool found = false;
					foreach( InstanceReportColumn col in baseColumns )
					{
						foreach( InstanceReportRow row in baseRows )
						{
							//CEE:  By moving this up, we are using the "strict" logic
							//Strict:  Instead of seeking to the first populated cell
							//   the netCell is expected to point to 1 and only 1 cell
							//found = true;

							Cell bCell = row.Cells.Find( cell => cell.Id == col.Id );
							if( bCell != null && bCell.HasData )
							{
								Cell clone = (Cell)bCell.Clone();
								clone.ShowCurrencySymbol = false;

								if( !this.IsEquityReport && this.IsMultiCurrency && row.IsMonetary )
								{
									clone.CurrencyCode = col.CurrencyCode;
									clone.CurrencySymbol = col.CurrencySymbol;
									clone.IsMonetary = true;
								}

								this.Rows[ r ].Cells.RemoveAt( c );
								this.Rows[ r ].Cells.Insert( c, clone );

								//CEE:  By moving this down, we are using the "loose" logic
								//Loose:  Instead of expecting the netCell to point to 
								//   1 and only 1 cell, we seek to the first populated cell
								found = true;
								break;
							}
						}

						if( found )
							break;
					}
				}
			}

			//clean up columns
			this.RemoveEmptyColumns();

			if( this.Columns.Count == 0 )
				return;

			//clean up rows
			this.RemoveEmptyRows();

			this.MergeCurrencyDifferences();
			this.CheckAndScrubCurrencies();
			this.ApplyCustomUnitsEmbedded();

			//apply grouped and unitcell
			this.ApplyRowStyles( rowIterators, messenger );

			if( !this.IsEquityReport )
			{
				this.PromoteDefaultMemberGroups();
				this.RemoveDefaultMemberHeaders();
			}

			this.RelabelDefaultTotalRow();
			this.ApplyUnitCell();

			//how many unique periods are on these columns?
			//if 1, remove the label
			this.RemoveSingularPeriod();

			//if a unit occurs on all columns...
			//   remove it from the columns so it can't get promoted as "shared"
			//   save it so it can be added last
			string currencyLabel = this.RemoveSharedCurrency();

			//if a label occurs on all columns, shift it to the header
			this.PromoteSharedLabels();

			//if there is a shared currency label, add it last
			if( !string.IsNullOrEmpty( currencyLabel ) )
				this.ReportName += Environment.NewLine + currencyLabel;

			//copy over the footnotes
			this.PromoteFootnotes( baseReport );

			this.SynchronizeGrid();

			this.ProcessColumnHeaders();
		}

		/// <summary>
		/// Repairs the label for the last row which is made up of all default members.
		/// </summary>
		private void RelabelDefaultTotalRow()
		{
			InstanceReportRow foundRow = this.Rows.Find( row => string.Empty == row.Label.Trim() );
			if( foundRow != null && foundRow.EmbedRequirements.Segments.Count > 0 )
			{
				Segment topSegment = foundRow.EmbedRequirements.Segments[ 0 ];
				foundRow.Label = topSegment.ValueName;
				foundRow.Level = 0;
				foundRow.GroupTotalSegment = topSegment;
			}
		}

		private void PromoteDefaultMemberGroups()
		{
			if( this.IsEquityReport )
				return;

			for( int r = 0; r < this.Rows.Count; r++ )
			{
				if( !IsDefaultMemberGroupRow( this.Rows[ r ] ) )
					continue;

				InstanceReportRow row = this.Rows[ r ];
				Segment rowSegment = row.EmbedRequirements.Segments[ 0 ];


				//This is the one we need to promote
				//Let's move backwards to the "break" in this grouping
				//We can do this by finding when the "Level" of the previous row
				//    drops below the "Level" of this row
				int insertAfter = r - 1;
				for( ; insertAfter >= 0; insertAfter-- )
				{
					if( this.Rows[ insertAfter ].Level < row.Level )
						break;
				}

				int insertAt = insertAfter + 1;
				if( insertAt < r )
				{
					//we're going to remove this row, regardless of whether we replace it
					this.Rows.RemoveAt( r );

					InstanceReportRow currentRow = this.Rows[ insertAt ];
					if( !IsDefaultMemberGroupRow( currentRow ) )
					{
						this.Rows.Insert( insertAt, row );
					}
					else
					{
						Segment currentRowSegment = currentRow.EmbedRequirements.Segments[ 0 ];
						if( 0 == CompareSegments( rowSegment, currentRowSegment ) )
						{
							//this means we have two headers for the same label / member, therefore...
							// - do not replace the row
							// - step back one row
							r--;
						}
						else
						{
							this.Rows.Insert( insertAt, row );
						}
					}
				}
			}

			this.ResequenceRows();
		}

		private void RemoveDefaultMemberHeaders()
		{
			if( this.IsEquityReport )
				return;

			this.Rows.RemoveAll(
			row =>
			{
				if( !IsDefaultMemberGroupRow( row ) )
					return false;

				return row.IsEmpty();
			} );
		}

		private bool IsMemberGroupRow( InstanceReportRow row )
		{
			if( row == null )
				return false;

			if( !row.IsAbstractGroupTitle ||
				row.EmbedRequirements == null ||
				row.EmbedRequirements.Segments == null ||
				row.EmbedRequirements.Segments.Count != 1 )
				return false;

			return true;
		}

		private bool IsDefaultMemberGroupRow( InstanceReportRow row )
		{
			if( !IsMemberGroupRow( row ) )
				return false;

			return row.EmbedRequirements.Segments[ 0 ].IsDefaultForEntity;
		}

		private void ApplyCustomUnitsEmbedded()
		{
			CommandIterator.IteratorType elementLoc = this.GetElementLocation();
			List<InstanceReportItem> elementItems = this.GetItems( elementLoc );
			List<InstanceReportItem> otherRows = elementItems.FindAll( row => row.EmbedRequirements.ElementRow.Unit > UnitType.StandardUnits );
			if( otherRows.Count == 0 )
				return;


			CommandIterator.IteratorType unitLoc = this.GetUnitLocation();
			List<InstanceReportItem> unitItems = this.GetItems( unitLoc );
			List<InstanceReportItem> customUnits = unitItems.FindAll( col => col.EmbedRequirements.Unit.HasCustomUnits() );
			if( customUnits.Count == 0 )
				return;

			foreach( InstanceReportItem col in customUnits )
			{
				int maxLabelId = 0;
				col.Labels.ForEach( ll => maxLabelId = Math.Max( ll.Id, maxLabelId ) );

				foreach( InstanceReportItem row in otherRows )
				{
					Cell[] cells = row.GetCellArray( this );
					bool showCustomUnits = Array.Exists( cells, c => c.Id == col.Id && c.HasData );
					if( !showCustomUnits )
						continue;

					//we've already affirmed that these exist
					List<UnitProperty> ups = col.EmbedRequirements.Unit.OtherUnits.FindAll( InstanceReportColumn.IsCustomUnit );
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

		private void CheckAndScrubCurrencies()
		{
			CommandIterator.IteratorType elementLoc = this.GetElementLocation();
			CommandIterator.IteratorType unitLoc = this.GetUnitLocation();
			if( elementLoc == unitLoc )
			{
				List<InstanceReportItem> items = this.GetItems( elementLoc );
				items.ForEach( i =>
				{
					i.HasMultiCurrency = i.EmbedRequirements.ElementRow.IsMonetary;
					i.GenerateEmbedLabel();
				} );
			}
			else
			{
				//it only takes a single element to turn on the currency symbol
				List<InstanceReportItem> eItems = this.GetItems( elementLoc );
				bool hasMonetaryElement = ReportUtils.Exists( eItems, eI => eI.EmbedRequirements.ElementRow.IsMonetary );

				List<InstanceReportItem> uItems = this.GetItems( unitLoc );
				uItems.ForEach( uI =>
				{
					uI.HasMultiCurrency = hasMonetaryElement;
					uI.GenerateEmbedLabel();
				} );
			}
		}

		private List<InstanceReportItem> GetItems( CommandIterator.IteratorType location )
		{
			return this.GetItems( location, false );
		}

		private List<InstanceReportItem> GetItems( CommandIterator.IteratorType location, bool removeAbstractGroupTitle )
		{
			List<InstanceReportItem> items = location == CommandIterator.IteratorType.Column ?
				this.Columns.ConvertAll( col => (InstanceReportItem)col ):
				this.Rows.ConvertAll( row => (InstanceReportItem)row );

			if( removeAbstractGroupTitle )
				items.RemoveAll( i => i.IsAbstractGroupTitle );

			return items;
		}


		private void SynchronizeGrid()
		{
			int c = 1;
			this.Columns.ForEach( col => col.Id = c++ );

			int r = 1;
			this.Rows.ForEach(
			row => 
			{
				c = 1;
				row.Id = r++;
				row.Cells.ForEach( cell => cell.Id = c++ );
			});
		}

		private void PromoteFootnotes( InstanceReport baseReport )
		{
			//if there are no footnotes to copy, return
			if( baseReport.Footnotes == null || baseReport.Footnotes.Count == 0 )
				return;

			//collect the footnotes from the baseReport
			SortedDictionary<int, Footnote> footnotes = new SortedDictionary<int, Footnote>();
			foreach( Footnote fn in baseReport.Footnotes )
			{
				footnotes[ fn.NoteId ] = fn;
			}

			//which of these footnotes are in use?
			List<int> footnoteIDs = new List<int>();
			foreach( InstanceReportRow row in this.Rows )
			{
				foreach( Cell cell in row.Cells )
				{
					if( string.IsNullOrEmpty( cell.FootnoteIndexer ) )
						continue;

					MatchCollection mc = Regex.Matches( cell.FootnoteIndexer, @"\[(\d+)\]" );
					if( mc.Count == 0 )
						continue;

					foreach( Match m in mc )
					{
						int fnID = int.Parse( m.Groups[ 1 ].Value );
						if( !footnoteIDs.Contains( fnID ) )
						{
							footnoteIDs.Add( fnID );
						}
					}
				}
			}

			//no footnotes found to retain
			if( footnoteIDs.Count == 0 )
				return;

			//we might be working on a reference to baseReport, so clear out the footnotes
			this.Footnotes.Clear();

			//we need to remember what footnotes get re-indexed as
			Dictionary<int, int> translationTable = new Dictionary<int, int>();
			for( int f = 0; f < footnoteIDs.Count; f++ )
			{
				int id = footnoteIDs[ f ];
				translationTable[ id ] = f + 1;

				Footnote fn = footnotes[ id ];
				fn.NoteId = f + 1;

				if( !ReportUtils.Exists( this.Footnotes, note => note.NoteId == fn.NoteId ) )
					this.Footnotes.Add( fn );
			}

			//update the footnote indexers on the cells
			foreach( InstanceReportRow row in this.Rows )
			{
				foreach( Cell cell in row.Cells )
				{
					if( string.IsNullOrEmpty( cell.FootnoteIndexer ) )
						continue;

					cell.FootnoteIndexer = Regex.Replace( cell.FootnoteIndexer, @"\[(\d+)\]",
					( match ) =>
					{
						int fnID = int.Parse( match.Groups[ 1 ].Value );
						string newValue = match.Value.Replace( fnID.ToString(), translationTable[ fnID ].ToString() );
						return newValue;
					} );
				}
			}

			//now promote indexers to the row...
			foreach( Footnote fn in this.Footnotes )
			{
				string indexer = string.Format( "[{0}]", fn.NoteId );
				foreach( InstanceReportRow row in this.Rows )
				{
					List<Cell> dataCells = row.Cells.FindAll( c => c.HasData );
					if( dataCells.Count == 0 )
						continue;

					bool allCellsHaveIndexer = dataCells.TrueForAll(
						cell => cell.FootnoteIndexer.Contains( indexer ) );

					//all cells have this indexer - promote it to the row
					if( allCellsHaveIndexer )
					{
						row.FootnoteIndexer += indexer;
						row.Cells.ForEach( cell =>
						{
							cell.FootnoteIndexer = cell.FootnoteIndexer.Replace( indexer, string.Empty ).Replace( ",,", "," ).Trim( ',' );
						} );
					}
				}
			}
		}

		private void ApplyRowStyles( CommandIterator[] rowIterators, ITraceMessenger messenger )
		{
			int r = 0;
			int depth = 0;
			Stack<CommandIterator> iteratorHierarchy = new Stack<CommandIterator>();
			LinkedList<CommandIterator> iterators = new LinkedList<CommandIterator>( rowIterators );
			bool hasUnitCell = Array.Exists( rowIterators, itr => itr.Style == CommandIterator.StyleType.UnitCell );
			this.ApplyRowStylesHierarchically( depth, ref r, iterators.First, iteratorHierarchy, hasUnitCell, messenger );
		}

		private bool ApplyRowStylesHierarchically( int depth, ref int r,
			LinkedListNode<CommandIterator> iteratorNode, Stack<CommandIterator> iteratorHierarchy,
			bool hasUnitCell, ITraceMessenger messenger )
		{
			if( iteratorNode == null )
			{
				InstanceReportRow row = this.Rows[ r ];
				row.Level = depth;

				foreach( CommandIterator ci in iteratorHierarchy )
				{
					if( ci.Style == CommandIterator.StyleType.Grouped )
					{
						row.EmbedRequirements.HideLabel( ci, ci.SelectionString );
					}
				}
				row.GenerateEmbedLabel();

				if( row.Label.Trim() == string.Empty )
					ShowLastNonDefaultLabel( depth, iteratorHierarchy, row );

				//if( row.Label.Trim() == string.Empty )
				//{
				//    InstanceReportRow prevRow = null;
				//    for( int pr = r - 1; pr > 0; pr-- )
				//    {
				//        prevRow = this.Rows[ pr ];
				//        if( IsMemberGroupRow( prevRow ) )
				//        {
				//            ShowLastNonDefaultLabel( depth, iteratorHierarchy, row );
				//            break;
				//        }
				//    }
				//}

				return true;
			}

			CommandIterator iterator = iteratorNode.Value;
			if( iterator.Style != CommandIterator.StyleType.Grouped ||
				iterator.Selection != CommandIterator.SelectionType.Axis )
			{
				return this.ApplyRowStylesHierarchically( depth, ref r, iteratorNode.Next, iteratorHierarchy, hasUnitCell, messenger );
			}

			Segment defaultMember = this.AxisMemberDefaults.ContainsKey( iterator.AxisName ) ?
				this.AxisMemberDefaults[ iterator.AxisName ] : null;
			if( defaultMember == null )
			{
				if( messenger != null )
				{
					//Keyword ‘grouped’ has [...] no impact on a ‘primary’ axis
					//Keyword ‘grouped’ reverts to the same treatment as ‘compact’ when its axis has no default member.
					messenger.TraceWarning( "Warning: Default member is missing for '" + iterator.AxisName + "'." + Environment.NewLine + "The keyword ‘grouped’ reverts to the same treatment as ‘compact’ when its axis has no default member." );
				}

				return this.ApplyRowStylesHierarchically( depth, ref r, iteratorNode.Next, iteratorHierarchy, hasUnitCell, messenger );
			}

			KeyValuePair<string, object> memberGroupValue = new KeyValuePair<string, object>();
			Dictionary<string, object> matchValues = CreateHierarchicalDictionary( this.Rows[ r ], iteratorHierarchy );
			for( ; r < this.Rows.Count; )
			{
				InstanceReportRow row = this.Rows[ r ];
				row.Level = depth;

				KeyValuePair<string, object> member = row.EmbedRequirements.GetMemberKeyValue( iterator );
				if( !RowMatchesHierarchy( row, iteratorHierarchy, matchValues ) )
					return false;

				int cmp = Comparer<object>.Default.Compare( memberGroupValue.Value, member.Value );
				if( cmp != 0 )
				{
					memberGroupValue = member;
					InstanceReportRow memberRow = this.CreateStyleRow( depth, iterator, member.Value );
					this.Rows.Insert( r++, memberRow );
				}

				iteratorHierarchy.Push( iterator );

				if( this.ApplyRowStylesHierarchically( depth + 1, ref r, iteratorNode.Next, iteratorHierarchy, hasUnitCell, messenger ) )
					r++;

				iteratorHierarchy.Pop();
			}

			return true;
		}

		private static void ShowLastNonDefaultLabel( int depth, Stack<CommandIterator> iteratorHierarchy, InstanceReportRow row )
		{
			Segment ciSegment = null;
			CommandIterator ciSave = null;
			CommandIterator[] ciList = iteratorHierarchy.ToArray();
			foreach( CommandIterator ci in ciList )
			{
				if( depth > 0 )
					depth--;

				Segment seg = row.EmbedRequirements.GetMemberKeyValue( ci ).Value as Segment;
				if( seg != null && !seg.IsDefaultForEntity )
				{
					ciSegment = seg;
					ciSave = ci;
					break;
				}
			}

			if( ciSave != null )
			{
				row.Level = depth;
				row.GroupTotalSegment = ciSegment;
				row.EmbedRequirements.ShowLabel( ciSave, ciSave.SelectionString );
				row.GenerateEmbedLabel();
			}
		}

		private void ApplyUnitCell()
		{
			//start at 1 because unitcell only works with groupings
			//if row 0 has unitcell, it cannot be "rolled up" anyway
			for( int r = 1; r < this.Rows.Count; r++ )
			{
				//read until we have unitcell rows
				// r will be the "start row"
				InstanceReportRow firstRow = this.Rows[ r ];
				if( !firstRow.HasUnitCell() )
					continue;

				InstanceReportRow groupRow = this.Rows[ r - 1 ];
				if( !IsMemberGroupRow( groupRow) )
					continue;

				//Segment groupRowSegment = groupRow.EmbedRequirements.Segments[0];
				ColumnRowRequirement groupRequirement = groupRow.EmbedRequirements;
				if( !groupRequirement.IsDimensionMatch( firstRow.EmbedRequirements ) )
					continue;


				//read until we DO NOT have unitcell rows
				// i will be the "end row"
				int i = r + 1;
				for( ; i < this.Rows.Count; i++ )
				{
					InstanceReportRow lastRow = this.Rows[ i ];
					if( !lastRow.HasUnitCell() )
						break;

					if( !groupRequirement.IsDimensionMatch( lastRow.EmbedRequirements ) )
						break;
				}

				List<InstanceReportRow> ucRows = this.Rows.GetRange( r, i - r );
				if( ucRows.Count > 1 )
				{
					//prepare a dictionary to check for the movability of cells
					Dictionary<int, int> cellUseCount = new Dictionary<int, int>();
					for( int c = 0; c < this.Columns.Count; c++ )
						cellUseCount[ c ] = 0;

					bool isCollision = false;
					foreach( InstanceReportRow ucRow in ucRows )
					{
						for( int c = 0; c < ucRow.Cells.Count; c++ )
						{
							if( !ucRow.Cells[ c ].HasData )
								continue;

							//if this cell has multiple values,
							//unit-cell logic cannot be applied
							cellUseCount[ c ]++;
							if( cellUseCount[ c ] > 1 )
							{
								isCollision = true;
								break;
							}
						}

						if( isCollision )
							break;
					}

					if( isCollision )
					{
						r = i;
						continue;
					}
				}

				foreach( InstanceReportRow ucRow in ucRows )
				{
					this.Rows.Remove( ucRow );
					for( int c = 0; c < ucRow.Cells.Count; c++ )
					{
						Cell cell = ucRow.Cells[ c ];
						if( ucRow.Cells[ c ].HasData )
						{
							groupRow.Cells.RemoveAt( c );
							groupRow.Cells.Insert( c, cell );
						}
					}
				}
			}

			this.ResequenceRows();
		}

		private Dictionary<string, int> CountEmbeddedPeriods( CommandIterator.IteratorType periodLocation )
		{
			Dictionary<string, int> periods = new Dictionary<string, int>();

			if( periodLocation == CommandIterator.IteratorType.Column )
			{
				this.Columns.ForEach(
				col =>
				{
					if( col.EmbedRequirements != null )
						periods[ col.EmbedRequirements.PeriodLabel ] = 1;
				} );
			}
			else
			{
				this.Rows.ForEach(
				row =>
				{
					if( row.EmbedRequirements == null )
						return;

					if( string.IsNullOrEmpty( row.EmbedRequirements.PeriodLabel ) )
						return;

					periods[ row.EmbedRequirements.PeriodLabel ] = 1;
				} );
			}

			return periods;
		}

		private static ColumnRowRequirement BuildNetCell( params ColumnRowRequirement[] requirements )
		{
			ColumnRowRequirement netCell = new ColumnRowRequirement();
			Dictionary<string, int> segments = new Dictionary<string, int>();

			foreach( ColumnRowRequirement req in requirements )
			{
				if( string.IsNullOrEmpty( netCell.ElementKey ) )
					netCell.ElementRow = req.ElementRow;

				if( netCell.Period == null )
					netCell.Period = req.Period;

				foreach( Segment s in req.Segments )
				{
					if( !ReportUtils.Exists( netCell.Segments, seg => Segment.Equals( seg, s ) ) )
						netCell.Segments.Add( s );
				}

				if( netCell.Unit == null )
					netCell.Unit = req.Unit;
			}

			return netCell;
		}

		private static string GetColumnCurrencyCode( InstanceReportColumn col )
		{
			UnitProperty up;
			return GetColumnCurrencyCode( col, out up );
		}

		private static string GetColumnCurrencyCode( InstanceReportColumn col, out UnitProperty unitProp )
		{
			unitProp = new UnitProperty();
			string cc = string.Empty;

			if( col == null )
				return cc;

			foreach( UnitProperty up in col.Units )
			{
				if( InstanceReportColumn.IsMonetaryUnit( up ) )
				{
					cc = InstanceUtils.GetCurrencyCodeFromUnit( up );
					if( cc == null )
						continue;

					cc = cc.Trim().ToUpper();
					if( cc == string.Empty )
						continue;

					unitProp = up;
					return cc;
				}
			}

			return string.Empty;
		}

		private List<InstanceReportColumn> FindBaseColumns( ColumnRowRequirement netCell, List<InstanceReportColumn> columns )
		{
			List<InstanceReportColumn> foundColumns = columns.FindAll( col => FindColumn( col, netCell ) );
			return foundColumns;
		}

		private List<InstanceReportRow> FindBaseRows( ColumnRowRequirement netCell, List<InstanceReportRow> rows )
		{
			//here we make the assumption that the only items on our rows are elements...
			List<InstanceReportRow> foundRows = rows.FindAll(
			row =>
			{
				if( string.IsNullOrEmpty( netCell.ElementKey ) )
					return true;

				if( !string.Equals( netCell.ElementKey, row.ElementKey ) )
					return false;

				if( netCell.ElementRow.IsBeginningBalance && !row.IsBeginningBalance )
					return false;

				if( netCell.ElementRow.IsEndingBalance && !row.IsEndingBalance )
					return false;

				return true;
			} );

			return foundRows;
		}

		private bool FindColumn( InstanceReportColumn col, ColumnRowRequirement netCell )
		{
			if( netCell.Period != null )
			{
				CalendarPeriod colCalendar = col.GetCalendarPeriod();
				if( netCell.Period.PeriodType == colCalendar.PeriodType )
				{
					if( !CalendarPeriod.Equals( colCalendar, netCell.Period ) )
						return false;
				}
				else
				{
					CalendarPeriod cpDuration = null;
					CalendarPeriod cpInstant = null;
					if( colCalendar.PeriodType == Element.PeriodType.instant )
					{
						cpDuration = netCell.Period;
						cpInstant = colCalendar;
					}
					else
					{
						cpDuration = colCalendar;
						cpInstant = netCell.Period;
					}

					if( netCell.ElementRow != null && netCell.ElementRow.IsBeginningBalance )
					{
						if( !DateTime.Equals( cpInstant.StartDate.AddDays(1), cpDuration.StartDate ) )
							return false;
					}
					else
					{
						if( !DateTime.Equals( cpInstant.StartDate, cpDuration.EndDate ) )
							return false;
					}
				}
			}

			if( netCell.Unit != null )
			{
				string colCC = GetColumnCurrencyCode( col );
				string unitCC = netCell.UnitCode;
				bool unitsMatch = colCC == unitCC;
				if( !unitsMatch )
					return false;
			}

			if( netCell.Segments != null )
			{
				foreach( Segment seg in netCell.Segments )
				{
					bool found = false;
					foreach( Segment cSeg in col.Segments )
					{
						if( cSeg.DimensionInfo.dimensionId == seg.DimensionInfo.dimensionId )
						{
							if( cSeg.DimensionInfo.Id == seg.DimensionInfo.Id )
							{
								found = true;
								break;
							}
							else
							{
								return false;
							}
						}
					}

					if( !found && !seg.IsDefaultForEntity )
						return false;
				}
			}

			return true;
		}

		private Dictionary<string, object> CreateHierarchicalDictionary( InstanceReportRow row, Stack<CommandIterator> iteratorHierarchy )
		{
			Dictionary<string, object> hierarchicalDictionary = new Dictionary<string, object>();
			foreach( CommandIterator cmdItr in iteratorHierarchy )
			{
				KeyValuePair<string, object> member = row.EmbedRequirements.GetMemberKeyValue( cmdItr );
				hierarchicalDictionary[ member.Key ] = member.Value;
			}
			return hierarchicalDictionary;
		}

		private bool RowMatchesHierarchy( InstanceReportRow row, Stack<CommandIterator> iteratorHierarchy, Dictionary<string, object> matchValues )
		{
			foreach( CommandIterator cmdItr in iteratorHierarchy )
			{
				KeyValuePair<string, object> member = row.EmbedRequirements.GetMemberKeyValue( cmdItr );
				int cmp = Comparer<object>.Default.Compare( matchValues[ cmdItr.SelectionString ], member.Value );
				if( cmp != 0 )
				{
					Segment seg = member.Value as Segment;
					if( seg == null )
						return false;

					if( !seg.IsDefaultForEntity )
						return false;

					return false;
				}
			}

			return true;
		}

		private InstanceReportRow CreateStyleRow( int depth, CommandIterator iterator, object memberValue )
		{
			InstanceReportRow groupedRow = new InstanceReportRow();
			groupedRow.EmbedRequirements = new ColumnRowRequirement( iterator );
			groupedRow.EmbedRequirements.Add( iterator, memberValue );
			groupedRow.IsAbstractGroupTitle = true;
			groupedRow.GenerateEmbedLabel( groupedRow.EmbedRequirements, true );
			groupedRow.Level = depth;

			for( int c = 0; c < this.Columns.Count; c++ )
				groupedRow.Cells.Add( new Cell( this.Columns[ c ].Id ) );

			return groupedRow;
		}

		private static string[] GetSharedLabels( List<InstanceReportItem> items )
		{
			Dictionary<string, int> labelCounts = new Dictionary<string, int>();
			foreach( InstanceReportItem col in items )
			{
				foreach( LabelLine ll in col.Labels )
				{
					if( !labelCounts.ContainsKey( ll.Label ) )
						labelCounts[ ll.Label ] = 0;

					labelCounts[ ll.Label ]++;
				}
			}

			List<string> sharedLabels = new List<string>();
			foreach( KeyValuePair<string, int> kvpLabelCount in labelCounts )
			{
				if( kvpLabelCount.Value == items.Count )
					sharedLabels.Add( kvpLabelCount.Key );
			}

			return sharedLabels.ToArray();
		}

		private void MergeCurrencyDifferences()
		{
			if( this.Columns[ 0 ].EmbedRequirements.Unit == null )
			{
				List<InstanceReportItem> rows = this.Rows.ConvertAll( row => (InstanceReportItem)row );
				this.MergeCurrencyDifferences( rows );
			}
			else
			{
				List<InstanceReportItem> columns = this.Columns.ConvertAll( col => (InstanceReportItem)col );
				this.MergeCurrencyDifferences( columns );
			}

			this.SynchronizeGrid();
		}

		private void MergeCurrencyDifferences( List<InstanceReportItem> items )
		{
			CommandIterator unitIterator = CommandIterator.CreateDefaultUnit();

			//it's easier to do this for rows than for columns, so perform the row merge first...
			Dictionary<int, string> cellCounts = new Dictionary<int, string>();
			for( int i = 0; i < items.Count - 1; i++ )
			{
				InstanceReportItem thisItem = items[ i ];
				if( thisItem.IsAbstractGroupTitle )
					continue;

				InstanceReportItem nextItem = items[ i + 1 ];
				if( nextItem.IsAbstractGroupTitle )
					continue;

				if( !ColumnRowRequirement.AreSameExceptCurrency( thisItem.EmbedRequirements, nextItem.EmbedRequirements ) )
					continue;

				Cell[] theseCells = thisItem.GetCellArray( this );
				for( int c = 0; c < theseCells.Length; c++ )
				{
					cellCounts[ c ] = null; //reset
					if( theseCells[ c ].HasData )
						cellCounts[ c ] = theseCells[ c ].ToString( this );
				}

				bool collides = false;
				Cell[] nextCells = nextItem.GetCellArray( this );
				for( int c = 0; c < nextCells.Length; c++ )
				{
					Cell nextCell = nextCells[ c ];
					if( nextCell.HasData )
					{
						if( string.IsNullOrEmpty( cellCounts[ c ] ) )
						{
							//this is ok
						}
						else if( cellCounts[ c ] == nextCell.ToString( this ) )
						{
							//this is ok
						}
						else
						{
							collides = true;
							break;
						}
					}
				}

				if( !collides )
				{
					if( thisItem.EmbedRequirements.Unit.IsMonetary() != nextItem.EmbedRequirements.Unit.IsMonetary() )
					{
						if( thisItem.EmbedRequirements.Unit.IsMonetary() )
						{
							//thisItem is the monetary item - no need to change Underlying `UnderlyingUnitProperty`
							//simply append the other, unique units
							thisItem.EmbedRequirements.Unit.AddOtherUnits( nextItem.EmbedRequirements.Unit.UnderlyingUnitProperty );
							thisItem.EmbedRequirements.Unit.AddOtherUnits( nextItem.EmbedRequirements.Unit.OtherUnits );
						}
						else
						{
							//thisItem IS NOT the monetary item, but nextItem is, therefore...
							//append the unique units from thisItem, and then write the whole unit
							//from nextItem back to thisItem
							nextItem.EmbedRequirements.Unit.AddOtherUnits( thisItem.EmbedRequirements.Unit.UnderlyingUnitProperty );
							nextItem.EmbedRequirements.Unit.AddOtherUnits( thisItem.EmbedRequirements.Unit.OtherUnits );

							thisItem.EmbedRequirements.Unit = nextItem.EmbedRequirements.Unit;
						}
					}

					thisItem.EmbedRequirements.HideLabel( unitIterator, null );
					thisItem.GenerateEmbedLabel();
					for( int c = 0; c < nextCells.Length; c++ )
					{
						Cell cell = theseCells[ c ];
						int id = cell.Id;
						if( nextCells[ c ].HasData )
						{
							cell = nextCells[ c ];
							cell.Id = id;
							thisItem.ReplaceCell( this, c, cell );
						}

						if( cell.IsMonetary )
						{
							cell.IsIndependantCurrency = true;
							cell.ShowCurrencySymbol = true;
						}
					}

					nextItem.RemoveSelf( this );
					items.Remove( nextItem );
					i--;
				}
			}
		}

		private string RemoveSharedCurrency()
		{
			CommandIterator.IteratorType unitLoc = this.GetUnitLocation();
			List<InstanceReportItem> items = this.GetItems( unitLoc );

			Dictionary<string, int> currencyCounts = new Dictionary<string, int>();
			foreach( InstanceReportItem item in items )
			{
				if( item == null )
					return string.Empty;

				if( item.EmbedRequirements == null )
					return string.Empty;

				if( item.EmbedRequirements.Unit == null )
					return string.Empty;

				string currencyLabel = item.EmbedRequirements.GetLabel( CommandIterator.DefaultUnit, false );
				if( !ReportUtils.Exists( item.Labels, lbl => lbl.Label == currencyLabel ) )
					return string.Empty;

				if( !currencyCounts.ContainsKey( item.EmbedRequirements.UnitCode ) )
					currencyCounts[ item.EmbedRequirements.UnitCode ] = 0;

				currencyCounts[ item.EmbedRequirements.UnitCode ]++;
			}

			//Is the currency code turned on for all items and is it the same code?
			bool isCurrencyShared = false;
			if( currencyCounts.ContainsKey( string.Empty ) && currencyCounts.Count == 2 )
			{
				//clean up the dictionary to check for actual units
				currencyCounts.Remove( string.Empty );
			}

			if( currencyCounts.Count == 1 )
			{
				List<int> count = new List<int>( currencyCounts.Values );
				if( items.Count == count[ 0 ] )
				{
					//Is the currency code turned on for all items and is it the same code?
					isCurrencyShared = true;
				}
			}
			
			if( isCurrencyShared )
			{
				InstanceReportItem item = items[ 0 ];
				string promotedMonetaryLabel = string.Format( "({0}{1})", item.EmbedRequirements.UnitCode, "{0}" ); ;
				if( !string.IsNullOrEmpty( item.EmbedRequirements.UnitSymbol ) )
					promotedMonetaryLabel = string.Format( promotedMonetaryLabel, " " + item.EmbedRequirements.UnitSymbol );
				else
					promotedMonetaryLabel = string.Format( promotedMonetaryLabel, string.Empty );

				//this.ReportName += Environment.NewLine + promotedMonetaryLabel;
				RemoveSelectionLabels( items, CommandIterator.SelectionType.Unit );
				return promotedMonetaryLabel;
			}
			else
			{
				return string.Empty;
			}
		}

		private void PromoteSharedLabels()
		{
			if( this.Columns.Count > 1 )
			{
				List<InstanceReportItem> items = this.GetItems( CommandIterator.IteratorType.Column );
				this.PromoteSharedLabels( items );
			}

			if( this.Rows.Count > 1 )
			{
				List<InstanceReportItem> items = this.GetItems( CommandIterator.IteratorType.Row, true );
				this.PromoteSharedLabels( items );
			}
		}

		private void PromoteSharedLabels( List<InstanceReportItem> items )
		{
			string[] sharedLabels = GetSharedLabels( items );
			string[] labelsRemoved = RemoveSharedLabels( items, sharedLabels );
			if( labelsRemoved.Length > 0 )
			{
				foreach( string label in labelsRemoved )
				{
					//string promotedMonetaryLabel;
					//if( this.IsLabelMonetary( label, items, out promotedMonetaryLabel ) )
					//    this.ReportName += promotedMonetaryLabel + Environment.NewLine;
					//else
					this.ReportName += Environment.NewLine + label;
				}
			}
		}

		private static string[] RemoveSharedLabels( List<InstanceReportItem> items, string[] sharedLabels )
		{
			return RemoveSharedLabels( items, sharedLabels, false );
		}

		private static string[] RemoveSharedLabels( List<InstanceReportItem> items, string[] sharedLabels, bool force )
		{
			List<string> labelsRemoved = new List<string>();

			if( sharedLabels.Length > 0 )
			{
				foreach( string label in sharedLabels )
				{
					//ensure that we don't remove the last label
					if( !force )
					{
						if( !items.TrueForAll( item => item.Labels.Count > 1 ) )
							break;
					}

					foreach( InstanceReportItem item in items )
					{
						if( !labelsRemoved.Contains( label ) )
							labelsRemoved.Add( label );

						item.Labels.RemoveAll( lbl => string.Equals( lbl.Label, label ) );
					}
				}
			}

			return labelsRemoved.ToArray();
		}

		private void RemoveSingularPeriod()
		{
			CommandIterator.IteratorType periodLocation = this.GetPeriodLocation();
			Dictionary<string, int> periods = this.CountEmbeddedPeriods( periodLocation );
			if( periods.Count == 1 )
			{
				List<InstanceReportItem> items = this.GetItems( periodLocation, true );
				RemoveSelectionLabels( items, CommandIterator.SelectionType.Period );
			}
		}

		private static void RemoveSelectionLabels( List<InstanceReportItem> items, CommandIterator.SelectionType selectionType )
		{
			CommandIterator iterator = CommandIterator.CreateDefault( selectionType );
			foreach( InstanceReportItem item in items )
			{
				if( item.EmbedRequirements == null )
					continue;

				if( !item.EmbedRequirements.HasSelectionType( iterator ) )
					continue;

				string label = item.EmbedRequirements.GetLabel( iterator, true );
				item.Labels.RemoveAll( lbl => lbl.Label == label );
			}
		}
	}
}

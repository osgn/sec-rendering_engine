using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder.Utilities;
using System.Collections;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		private void ProcessMergeInstanceDuration( out List<ColumnInstantDurationMap> columnMaps )
		{
			columnMaps = new List<ColumnInstantDurationMap>();

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.INSTANCE_AND_DURATION_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				contextObjects.Add( "ColumnMaps", columnMaps );

				this.builderRules.ProcessRule( RulesEngineUtils.INSTANCE_AND_DURATION_RULE, contextObjects );
				this.FireRuleProcessed( RulesEngineUtils.INSTANCE_AND_DURATION_RULE );
			}
		}

		public bool ColumnsShouldBeMerged( InstanceReportColumn ircDuration, InstanceReportColumn ircInstant )
		{
			if( !ircDuration.SegmentAndScenarioEquals( ircInstant ) )
				return false;

			if( ircDuration.MCU.contextRef.PeriodType != Element.PeriodType.duration )
				return false;

			if( ircInstant.MCU.contextRef.PeriodType != Element.PeriodType.instant )
				return false;


			//check for end of current duration
			bool isMergeable = false;
			if( ircInstant.MCU.contextRef.PeriodStartDate == ircDuration.MCU.contextRef.PeriodEndDate )
				isMergeable = true;

			//check for beginning of current duration
			if( ircInstant.MCU.contextRef.PeriodStartDate == ircDuration.MCU.contextRef.PeriodStartDate.AddDays( -1 ) ||
				ircInstant.MCU.contextRef.PeriodStartDate == ircDuration.MCU.contextRef.PeriodStartDate )
				isMergeable = true;


			if( !isMergeable )
				return false;

			//continue TopLevelNode check for sgement/scenario
			//Get all of the "labels" that this column uses
			//removed empty entries and the calendar entry - we already matched that
			char[] lineBreak = { '\r', '\n' };
			string[] durationLabelParts = ircDuration.Label.Split( lineBreak );
			string periodString = ircDuration.GetReportingPeriodString();
			List<string> durationLables = new List<string>( durationLabelParts );
			durationLables.RemoveAll(
				( lbl ) => { return MergedColumnLabelFilter( lbl, periodString ); } );


			string[] instantLabelParts = ircInstant.Label.Split( lineBreak );
			periodString = ircInstant.GetReportingPeriodString();
			List<string> instantLables = new List<string>( instantLabelParts );
			instantLables.RemoveAll(
				( lbl ) => { return MergedColumnLabelFilter( lbl, periodString ); } );


			int labelDifferences = Math.Abs( instantLables.Count - durationLables.Count );
			if( labelDifferences > 1 )
			{
				//there are too many label differences for us to repair
				return false;
			}

			//Check if one of the columns has a unit label, and the other does not.
			if( labelDifferences == 1 )
			{
				if( durationLables.Count > instantLables.Count )
				{
					string dLastLabel = durationLables[ durationLables.Count - 1 ];
					if( dLastLabel.StartsWith( ircDuration.CurrencyCode ) &&
						dLastLabel.Contains( ircDuration.CurrencySymbol ) )
					{
						durationLables.Remove( dLastLabel );
					}
				}
				else
				{
					string iLastLabel = instantLables[ instantLables.Count - 1 ];
					if( iLastLabel.StartsWith( ircInstant.CurrencyCode ) &&
						iLastLabel.Contains( ircInstant.CurrencySymbol ) )
					{
						instantLables.Remove( iLastLabel );
					}
				}
			}

			//alright - we've done the best we can - let's see if they match
			for( int index = 0; index < instantLables.Count; index++ )
			{
				string instantStr = instantLables[ index ].ToString();
				string durationStr = durationLables[ index ].ToString();

				//converted from durationStr.CompareTo( instantStr ) != 0
				if( durationStr == instantStr )
					continue;

				return false;
			}

			return true;
		}

		public void MergeInstantAndDuration()
		{
			bool reprocessMerge = false;
			List<ColumnInstantDurationMap> columnMaps;

			do
			{
				this.ProcessMergeInstanceDuration( out columnMaps );

				if( this.ContainMultiCurrencies() )
					reprocessMerge = this.ProcessMultiCurrencyMaps( columnMaps );
			}
			while( reprocessMerge );

			this.ProcessColumnMaps( columnMaps );

			this.RemoveUnchangedPseudoColumns();
		}

		public void ProcessMergeInstanceDuration_Rule( List<ColumnInstantDurationMap> ColumnMaps )
		{
			//columnMaps = new List<ColumnInstantDurationMap>();

			if( this.IsUncatReport )
			{
				this.MergeColumnsWithSameContext();
			}
			else
			{
				List<InstanceReportColumn> instanceColumns = this.Columns.FindAll( c => c.MyPeriodType == Element.PeriodType.instant );
				List<InstanceReportColumn> durationColumns = this.Columns.FindAll( c => c.MyPeriodType == Element.PeriodType.duration );

				foreach( InstanceReportColumn instCol in instanceColumns )
				{
					int ic = this.Columns.IndexOf( instCol );

					List<InstanceReportColumn> durCols = durationColumns.FindAll( dc => ColumnsShouldBeMerged( dc, instCol ) );
					foreach( InstanceReportColumn durCol in durCols )
					{
						int dc = this.Columns.IndexOf( durCol );

						TimeSpan timeBucketSize = durCol.MyContextProperty.PeriodEndDate - durCol.MyContextProperty.PeriodStartDate;
						ColumnInstantDurationMap instanceColMap = new ColumnInstantDurationMap( ic, dc, timeBucketSize );
						ColumnMaps.Add( instanceColMap );
					}
				}
			}

			this.SynchronizeGrid();
		}



		private bool MergedColumnLabelFilter( string lbl, string periodString )
		{
			if( lbl.Length == 0 )
				return true;

			if( lbl == periodString )
				return true;

			bool isEPS = lbl.IndexOf( "/ shares", StringComparison.CurrentCultureIgnoreCase ) >= 0;
			if( isEPS )
				return true;

			return false;
		}

		private void MergeColumnsWithSameContext()
		{
			Hashtable htColumnMap = new Hashtable( 0 );//key -- target column, value -- array list of columns to merge
			ArrayList arProcessedColumns = new ArrayList( 0 );//stores the columns that's processed

			for( int colIndex = 0; colIndex < this.Columns.Count; colIndex++ )
			{
				if( arProcessedColumns.BinarySearch( colIndex ) < 0 )
				{
					for( int colIndexToCheck = colIndex + 1; colIndexToCheck < this.Columns.Count; colIndexToCheck++ ) //columns are sorted, only need to check forward
					{
						if( arProcessedColumns.BinarySearch( colIndexToCheck ) < 0 )
						{
							InstanceReportColumn ircTarget = this.Columns[ colIndex ] as InstanceReportColumn;
							InstanceReportColumn ircCheck = this.Columns[ colIndexToCheck ] as InstanceReportColumn;


							bool mergable = ( ircTarget.MCU.contextRef.PeriodType == ircCheck.MCU.contextRef.PeriodType &&
											 ircTarget.MCU.contextRef.PeriodStartDate == ircCheck.MCU.contextRef.PeriodStartDate && ircTarget.MCU.contextRef.PeriodEndDate == ircCheck.MCU.contextRef.PeriodEndDate );

							#region check segments/scenarios

							if( mergable ) //continue TopLevelNode check for sgement/scenario
							{
								char[] lineBreak = { '\r', '\n' };
								string[] durationLabelParts = ircTarget.Label.Split( lineBreak );
								string[] instantLabelParts = ircCheck.Label.Split( lineBreak );

								ArrayList durationLables = new ArrayList();
								durationLables.AddRange( durationLabelParts );

								ArrayList instantLables = new ArrayList();
								instantLables.AddRange( instantLabelParts );

								//Segments/scenarios should match
								//First remove all blank ones

								for( int index = durationLables.Count - 1; index >= 0; index-- )
								{
									if( durationLables[ index ].ToString() == "" )
									{
										durationLables.RemoveAt( index );
									}
								}

								for( int index = instantLables.Count - 1; index >= 0; index-- )
								{
									if( instantLables[ index ].ToString() == "" )
									{
										instantLables.RemoveAt( index );
									}
								}

								if( instantLables.Count == durationLables.Count )
								{
									for( int index = 0; index < instantLables.Count; index++ )
									{
										string instantStr = instantLables[ index ].ToString();
										string durationStr = durationLables[ index ].ToString();
										if( durationStr.CompareTo( instantStr ) != 0 && durationStr.IndexOf( instantStr ) < 0 )
										{
											mergable = false;
											break;
										}
									}

								}
								else
								{
									mergable = false;
								}
							}
							else
							{
								mergable = false;
							}

							#endregion

							if( mergable )
							{
								arProcessedColumns.Add( colIndexToCheck );
								arProcessedColumns.Sort();

								if( htColumnMap[ colIndex ] == null ) htColumnMap[ colIndex ] = new ArrayList();
								( htColumnMap[ colIndex ] as ArrayList ).Add( colIndexToCheck );
							}
							else
							{
								break;
							}
						}
					}
				}
			}

			//process the merging
			foreach( int colIndex in htColumnMap.Keys )
			{
				for( int rowIndex = 0; rowIndex < this.Rows.Count; rowIndex++ )
				{
					InstanceReportRow currentRow = this.Rows[ rowIndex ] as InstanceReportRow;
					foreach( int colToMerge in htColumnMap[ colIndex ] as ArrayList )
					{
						Cell checkCell = currentRow.Cells[ colToMerge ] as Cell;
						if( checkCell.HasData )
						{
							Cell targetCell = currentRow.Cells[ colIndex ] as Cell;
							targetCell.AddData( checkCell );
						}
					}
				}
			}

			for( int colIndexToRemove = this.Columns.Count - 1; colIndexToRemove >= 0; colIndexToRemove-- )
			{
				if( arProcessedColumns.BinarySearch( colIndexToRemove ) >= 0 )
				{
					this.Columns.RemoveAt( colIndexToRemove );

					//Remove columns from rows
					foreach( InstanceReportRow ir in this.Rows )
					{
						ir.Cells.RemoveAt( colIndexToRemove );
					}
				}
			}
		}

		private void ProcessColumnMaps( List<ColumnInstantDurationMap> columnMaps )
		{
			//Merge columns in the column map
			if( columnMaps == null || columnMaps.Count == 0 )
				return;

			ProcessMissingMapsMultiCurrency( columnMaps );

			//ANY columns that appear in our column map have been approved for merging
			//THEREFORE we will merge the units from the instant into the duration
			columnMaps.ForEach(
				( colMap ) =>
				{
					InstanceReportColumn ircDuration = this.Columns[ colMap.DurationColumnIndex ] as InstanceReportColumn;
					InstanceReportColumn ircInstant = this.Columns[ colMap.InstantColumnIndex ] as InstanceReportColumn;

					//If the merged column does not have a currency, apply the currency and the currency label
					if( string.IsNullOrEmpty( ircDuration.CurrencyCode ) &&
						!string.IsNullOrEmpty( ircInstant.CurrencyCode ) )
					{
						ircDuration.CurrencyCode = ircInstant.CurrencyCode;
						ircDuration.CurrencySymbol = ircInstant.CurrencySymbol;

						//we MIGHT need to apply the unit label to the duration column
						LabelLine llCurrency = ircInstant.Labels[ ircInstant.Labels.Count - 1 ];
						if( llCurrency.Label.StartsWith( ircInstant.CurrencyCode ) &&
							llCurrency.Label.Contains( ircInstant.CurrencySymbol ) )
						{
							ircDuration.Labels.Add( llCurrency );
						}
					}

					//Merge all units
					ircInstant.MCU.UPS.ForEach(
						( iUP ) =>
						{
							bool durationAlreadyHasUnitID = ReportUtils.Exists( ircDuration.MCU.UPS,
								( dUP ) =>
								{
									bool idMatches = dUP.UnitID == iUP.UnitID;
									return idMatches;
								} );

							if( !durationAlreadyHasUnitID )
								ircDuration.MCU.UPS.Add( iUP );
						}
					);
				}
			);

			//Do not allow destination columns to be deleted
			Dictionary<int, bool> destinationColumns = new Dictionary<int, bool>();

			//Only try to delete "source" columns
			Dictionary<int, Dictionary<string, bool>> sourceColumns = new Dictionary<int, Dictionary<string, bool>>();

			//we don't want to remove these - retain them so that 'ProcessBeginningEndingBalance' has some work to do
			List<ColumnInstantDurationMap> bbMaps = new List<ColumnInstantDurationMap>();
			List<ColumnInstantDurationMap> otherMaps = new List<ColumnInstantDurationMap>();


			foreach( ColumnInstantDurationMap colMap in columnMaps )
			{
				InstanceReportColumn ircInstant = this.Columns[ colMap.InstantColumnIndex ] as InstanceReportColumn;
				InstanceReportColumn ircDuration = this.Columns[ colMap.DurationColumnIndex ] as InstanceReportColumn;

				//is this a beginning balance column mapping?
				if( ircInstant.MCU.contextRef.PeriodStartDate == ircDuration.MCU.contextRef.PeriodStartDate.AddDays( -1 ) || ircInstant.MCU.contextRef.PeriodStartDate == ircDuration.MCU.contextRef.PeriodStartDate )
					bbMaps.Add( colMap );
				else
					otherMaps.Add( colMap );
			}

			Dictionary<int, bool> columnsMapped = new Dictionary<int, bool>();

			//For each map, go through the rows and switch the colID
			foreach( InstanceReportRow irr in this.Rows )
			{
				if( irr.IsBeginningBalance )
					ProcessMapsOnRow( irr, bbMaps, destinationColumns, sourceColumns );
				else
					ProcessMapsOnRow( irr, otherMaps, destinationColumns, sourceColumns );
			}

			//Get a SORTED list of all columns to remove
			List<int> columnsToRemove = new List<int>();
			foreach( int col in sourceColumns.Keys )
			{
				if( destinationColumns.ContainsKey( col ) )
					continue;

				if( columnsToRemove.Contains( col ) )
					continue;

				int populatedCells = this.CountColumnPopulatedElements( col );
				if( populatedCells <= sourceColumns[ col ].Count )
				{
					columnsToRemove.Add( col );
				}
				else
				{
					Dictionary<string, bool> elementLookup = sourceColumns[ col ];

					foreach( InstanceReportRow row in this.Rows )
					{
						if( !elementLookup.ContainsKey( row.ElementName ) )
							continue;

						( (Cell)row.Cells[ col ] ).Clear();
					}
				}
			}

			columnsToRemove.Sort();

			//Remove columns
			for( int index = columnsToRemove.Count - 1; index >= 0; index-- )
			{
				this.RemoveColumn( columnsToRemove[ index ] );
			}

			this.SynchronizeGrid();
		}

		private void ProcessMapsOnRow( InstanceReportRow irr, List<ColumnInstantDurationMap> maps,
			Dictionary<int, bool> destinationColumns, Dictionary<int, Dictionary<string, bool>> sourceColumns )
		{
			foreach( ColumnInstantDurationMap colMap in maps )
			{
				Cell instantCell = irr.Cells[ colMap.InstantColumnIndex ];
				if( !instantCell.HasData )
					continue;

				Cell durationCell = irr.Cells[ colMap.DurationColumnIndex ];

				/*
				if( durationCell.HasData )
					continue;

				bool hasUnit = false;
				foreach( UnitProperty up in this.Columns[ colMap.DurationColumnIndex ].Units )
				{
					if( string.Equals( up.UnitID, instantCell.UnitID ) )
					{
						hasUnit = true;
						break;
					}
				}
				*/


				if( !sourceColumns.ContainsKey( colMap.InstantColumnIndex ) )
					sourceColumns[ colMap.InstantColumnIndex ] = new Dictionary<string, bool>();

				sourceColumns[ colMap.InstantColumnIndex ][ irr.ElementName ] = true;
				destinationColumns[ colMap.DurationColumnIndex ] = true; ;
				durationCell.AddData( instantCell );

				if( this.Columns[ colMap.DurationColumnIndex ].IsPseudoColumn )
					this.Columns[ colMap.DurationColumnIndex ].IsPseudoColumnChanged = true;
			}
		}

		private bool ProcessMultiCurrencyMaps( List<ColumnInstantDurationMap> columnMaps )
		{
			this.SynchronizeGrid();

			//let's put this way to the right
			int maxColumnId = this.Columns.Count + 5;

			//first, convert the duration index to the duration ID
			//also, lookup and store the instant columns
			Dictionary<int, List<InstanceReportColumn>> durIdToInstantCols = new Dictionary<int, List<InstanceReportColumn>>();
			foreach( ColumnInstantDurationMap colMap in columnMaps )
			{
				int durID = this.Columns[ colMap.DurationColumnIndex ].Id;
				if( !durIdToInstantCols.ContainsKey( durID ) )
					durIdToInstantCols[ durID ] = new List<InstanceReportColumn>();

				durIdToInstantCols[ durID ].Add( this.Columns[ colMap.InstantColumnIndex ] );
			}

			//now we get a set of duration IDs in reverse so that we can work "from right to left"
			List<int> durationIDs = new List<int>( durIdToInstantCols.Keys );
			durationIDs.Sort();
			durationIDs.Reverse();

			bool reprocessMerge = false;
			foreach( int durID in durationIDs )
			{
				InstanceReportColumn durationColumn = this.Columns.Find( col => col.Id == durID );
				if( !string.IsNullOrEmpty( durationColumn.CurrencyCode ) )
					continue;

				List<InstanceReportColumn> instantColumns = durIdToInstantCols[ durID ];
				if( instantColumns.Count < 2 )
					continue;

				List<string> currencies = new List<string>();
				foreach( InstanceReportColumn iCol in instantColumns )
				{
					if( string.IsNullOrEmpty( iCol.CurrencyCode ) )
						continue;

					if( !currencies.Contains( iCol.CurrencyCode ) )
						currencies.Add( iCol.CurrencyCode );
				}

				if( currencies.Count < 2 )
					continue;

				//set this flag so that we know that the mapping needs to be run again.
				reprocessMerge = true;

				//alphabetical order...
				currencies.Sort();

				//...USD first
				if( currencies.Contains( InstanceUtils.USDCurrencyCode ) )
				{
					currencies.Remove( InstanceUtils.USDCurrencyCode );
					currencies.Insert( 0, InstanceUtils.USDCurrencyCode );
				}

				int newLabelId = 0;
				durationColumn.Labels.ForEach( ll => newLabelId = Math.Max( newLabelId, ll.Id ) );

				int durationIndex = this.Columns.IndexOf( durationColumn );
				int insertIndex = durationIndex;

				//create the clones first...
				InstanceReportColumn original = null;
				foreach( string currency in currencies )
				{
					insertIndex++;

					//keep moving this up - we do not want duplicated IDs
					maxColumnId++;

					InstanceReportColumn clone = (InstanceReportColumn)durationColumn.Clone();
					clone.Id = maxColumnId;
					clone.CurrencyCode = currency;
					clone.CurrencySymbol = ReportBuilder.GetISOCurrencySymbol( currency );

					//we will have to clean up at the end - make sure this column sticks somehow
					if( string.Equals( clone.CurrencyCode, durationColumn.CurrencyCode ) )
					{
						//if the currency code matches, this is preferred - always store it
						original = clone;
					}
					else
					{
						//we will have to clean up at the end - if no column has been selected yet, pick the first
						//later, if the currency code matches, we'll replace `original` with that
						if( original == null )
							original = clone;

						clone.IsPseudoColumn = true;
					}

					if( string.IsNullOrEmpty( clone.CurrencySymbol ) )
						clone.Labels.Add( new LabelLine( newLabelId, clone.CurrencyCode ) );
					else
						clone.Labels.Add( new LabelLine( newLabelId, clone.CurrencyCode + " (" + clone.CurrencySymbol + ")" ) );

					this.Columns.Insert( insertIndex, clone );
					foreach( InstanceReportRow row in this.Rows )
					{
						Cell c = (Cell)row.Cells[ durationIndex ].Clone();
						c.Id = maxColumnId;
						row.Cells.Insert( insertIndex, c );
					}
				}

				original.IsPseudoColumn = false;

				//...then remove the original
				this.RemoveColumn( durationIndex );
			}

			if( reprocessMerge )
				this.SynchronizeGrid();

			return reprocessMerge;
		}

		private void ProcessMissingMapsMultiCurrency( List<ColumnInstantDurationMap> columnMaps )
		{
			if( !this.ContainMultiCurrencies() )
				return;

			for( int durIndex = 0; durIndex < this.Columns.Count; durIndex++ )
			{
				InstanceReportColumn ircDuration = this.Columns[ durIndex ];
				if( ircDuration.MyPeriodType != Element.PeriodType.duration )
					continue;

				if( !string.Equals( ircDuration.CurrencyCode, InstanceUtils.USDCurrencyCode, StringComparison.CurrentCultureIgnoreCase ) )
					continue;

				for( int instantIndex = 0; instantIndex < this.Columns.Count; instantIndex++ )
				{
					InstanceReportColumn ircInstant = this.Columns[ instantIndex ];
					if( ( ircInstant.CurrencyCode.ToLower() == "usd" || String.IsNullOrEmpty( ircInstant.CurrencyCode ) ) &&
						ircInstant.MyPeriodType == Element.PeriodType.instant &&
						ircInstant.MyContextProperty.PeriodStartDate == ircDuration.MyContextProperty.PeriodEndDate )
					{
						if( ircDuration.SegmentAndScenarioEquals( ircInstant ) )
						{
							bool found = false;
							ColumnInstantDurationMap newMap = new ColumnInstantDurationMap( instantIndex, durIndex, ircDuration.ReportingSpan );
							foreach( ColumnInstantDurationMap map in columnMaps )
							{
								if( map.DurationColumnIndex == durIndex && map.InstantColumnIndex == instantIndex )
								{
									found = true;
									break;
								}
							}

							if( !found )
								columnMaps.Add( newMap );
						}
					}
				}
			}
		}

		private void RemoveUnchangedPseudoColumns()
		{
			for( int c = 0; c < this.Columns.Count; )
			{
				InstanceReportColumn col = this.Columns[ c ];
				if( col.IsPseudoColumn && !col.IsPseudoColumnChanged )
				{
					this.RemoveColumn( c );
				}
				else
				{
					c++;
				}
			}
		}
	}
}

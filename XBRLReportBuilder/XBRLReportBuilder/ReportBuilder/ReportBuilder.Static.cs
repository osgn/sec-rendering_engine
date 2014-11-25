using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLParser;
using System.Reflection;
using System.Xml;
using System.IO;
using Aucent.MAX.AXE.Common.Data;
using System.Collections;
using System.Data;
using Aucent.MAX.AXE.Common.Utilities;
using XBRLReportBuilder.Utilities;
using System.Diagnostics;

namespace XBRLReportBuilder
{
	public partial class ReportBuilder
	{
		private static object synchronizedResourcesLock = new object();

		private DefinitionAndReference dar = null;

		private static TaxonomyAddonManager _defRefHelper = null;
		private static TaxonomyAddonManager defRefHelper
		{
			get
			{
				if( _defRefHelper == null )
					TaxonomyAddonManager.TryAutoLoad( out _defRefHelper );

				return _defRefHelper;
			}
		}

		private static bool synchronizedResources = false;
		public static bool SynchronizedResources
		{
			get { return synchronizedResources; }
			private set { synchronizedResources = value; }
		}

		/// <summary>
		/// Crawls <paramref name="currentNode"/> recursively checking for <see cref="Node"/>s where <paramref name="currentNode"/>.<see>MyElement</see>.<see>IsDimensionItem</see>"/> is true.
		/// </summary>
		/// <param name="currentTaxonomy">The <see cref="Taxonomy"/> containing <paramref name="currentNode"/>, used to look up the default member of an axis.</param>
		/// <param name="currentNode">The current <see cref="Node"/>.  When called, this should be a top-level node from the presentation taxonomy.</param>
		/// <param name="axisName">The axis for any dimensions found.  Null or empty until a dimension is found.</param>
		/// <param name="axisByPresentation">A list of axes to be populated.</param>
		/// <param name="axisMembersByPresentation">A list of dimensions to be populated, grouped and key by axis.</param>
		private static void GetDimensions( Taxonomy currentTaxonomy, Node currentNode, string axisName, ref List<string> axisByPresentation, ref Dictionary<string, List<Segment>> axisMembersByPresentation )
		{
			if( string.IsNullOrEmpty( axisName ) )
			{
				if( currentNode.MyElement.IsDimensionItem() )
				{
					GetDimensions( currentTaxonomy, currentNode, currentNode.Id, ref axisByPresentation, ref axisMembersByPresentation );
				}
				else if( currentNode.HasChildren )
				{
					foreach( Node childNode in currentNode.Children )
					{
						GetDimensions( currentTaxonomy, childNode, null, ref axisByPresentation, ref axisMembersByPresentation );
					}
				}
			}
			else
			{
				//this is a dimension
				// - add it to the list
				if( !axisByPresentation.Contains( axisName ) )
					axisByPresentation.Add( axisName );

				// - collect the members in order
				if( !axisMembersByPresentation.ContainsKey( axisName ) )
					axisMembersByPresentation[ axisName ] = new List<Segment>();

				string defaultMember = null;
				if( currentTaxonomy != null && currentTaxonomy.NetDefinisionInfo != null )
					currentTaxonomy.NetDefinisionInfo.TryGetDefaultMember( axisName, out defaultMember );

				if( currentNode.HasChildren )
				{
					foreach( Node childNode in currentNode.Children )
					{
						Segment newSeg = new Segment
						{
							DimensionInfo = new ContextDimensionInfo
							{
								dimensionId = axisName,
								Id = childNode.Id
							},
							ValueName = childNode.Label,
							ValueType = axisName
						};

						//always store the default member
						if( string.Equals( defaultMember, childNode.Id ) )
							newSeg.IsDefaultForEntity = true;

						axisMembersByPresentation[ axisName ].Add( newSeg );

						if( childNode.HasChildren )
							GetDimensions( currentTaxonomy, childNode, axisName, ref axisByPresentation, ref axisMembersByPresentation );
					}
				}
			}
		}

		public static string GetISOCurrencySymbol( string currencyCode )
		{
			if( currencyCode == null )
				return InstanceUtils.USDCurrencySymbol;

			string decodedString = "";
			ISOUtility.CurrencyCode cc = ISOUtility.GetCurrencyCode( currencyCode );
			if( string.Equals( currencyCode, InstanceUtils.USDCurrencyCode, StringComparison.InvariantCultureIgnoreCase ) )
				return InstanceUtils.USDCurrencySymbol;

			if( cc != null && cc.Symbol.Length > 0 )
				decodedString = cc.Symbol;

			return decodedString;
		}

		public static string GetRoundingLevelString( RoundingLevel rl )
		{
			string retValue = string.Empty;
			switch( rl )
			{
				case RoundingLevel.Tens:
					retValue = "Tens";
					break;
				case RoundingLevel.Hundreds:
					retValue = "Hundreds";
					break;
				case RoundingLevel.Thousands:
					retValue = "Thousands";
					break;
				case RoundingLevel.TenThousands:
				case RoundingLevel.HundredThousands:
				case RoundingLevel.Millions:
					retValue = "Millions";
					break;
				case RoundingLevel.TenMillions:
				case RoundingLevel.HundredMillions:
				case RoundingLevel.Billions:
					retValue = "Billions";
					break;
				case RoundingLevel.TenBillions:
				case RoundingLevel.HundredBillions:
				case RoundingLevel.Trillions:
					retValue = "Trillions";
					break;
                case RoundingLevel.TenTrillions:
                case RoundingLevel.HundredTrillions:
                case RoundingLevel.Quadrillions:
                    retValue = "Quadrillions";
                    break;
				//case RoundingLevel.NoRounding:
				//    retValue = "Digits as shown";
				//    break;
				default:
					break;
			}
			return retValue;
		}

		/// <summary>
		/// Safely checks if the node provided is a dimension.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="dNode"></param>
		/// <returns></returns>
		private static bool IsDimension( Node node, out Node dNode )
		{
			dNode = null;
			if( node == null )
				return false;

			if( node.MyElement != null && node.MyElement.IsDimensionItem() )
			{
				dNode = node;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Consider calling <see>report</see>.<see cref="InstanceReport.SortColumns"/> first.  Merges 1) non-monetary units and 2) monetary units of the same currency and 3) merges them under the same calendar/segments if possible.
		/// </summary>
		/// <param name="report">The report whose columns will be merged.</param>
		/// <returns>True on success, false on fail.</returns>
		private static bool MergeColumns( InstanceReport report )
		{
			if( report == null )
				return false;

			bool hasMergedColumns = false;
			for( int c = 0; c < report.Columns.Count - 1; c++ )
			{
				InstanceReportColumn curColumn = report.Columns[ c ];
				InstanceReportColumn nextColumn = report.Columns[ c + 1 ];
				if( !curColumn.ReportingPeriodEquals( nextColumn )
					|| !curColumn.SegmentAndScenarioEquals( nextColumn ) )
					continue;

				bool curHasCurrency = !string.IsNullOrEmpty( curColumn.CurrencyCode );
				if( curHasCurrency )
				{
					bool nextHasCurrency = !string.IsNullOrEmpty( nextColumn.CurrencyCode );
					if( nextHasCurrency )
					{
						if( !string.Equals( curColumn.CurrencyCode, nextColumn.CurrencyCode ) )
							continue;
					}
				}

				bool columnValuesAreUnique = true;
				foreach( InstanceReportRow row in report.Rows )
				{
					if( row.Cells[ c ].HasData && row.Cells[ c + 1 ].HasData )
					{
						columnValuesAreUnique = false;
						break;
					}
				}

				if( columnValuesAreUnique )
				{
					//since we have unique columns, merge them and remove the column + cells
					hasMergedColumns = true;
					foreach( InstanceReportRow row in report.Rows )
					{
						if( !row.Cells[ c ].HasData && row.Cells[ c + 1 ].HasData )
							row.Cells[ c ].AddData( row.Cells[ c + 1 ] );
					}

					//please don't move this up ;-)
					report.RemoveColumn( c + 1 );
					c--;

					curColumn.Units.AddRange( nextColumn.Units );
					if( string.IsNullOrEmpty( curColumn.CurrencyCode ) && !string.IsNullOrEmpty( nextColumn.CurrencyCode ) )
					{
						curColumn.CurrencyCode = nextColumn.CurrencyCode;
						curColumn.CurrencySymbol = nextColumn.CurrencySymbol;

						curColumn.MCU.CurrencyCode = nextColumn.MCU.CurrencyCode;
						curColumn.MCU.CurrencySymbol = nextColumn.MCU.CurrencySymbol;

						//this is probably the currency label - since we didn't have a currency, we didn't have the label
						//copy the reference to this label over
						LabelLine last = nextColumn.Labels[ nextColumn.Labels.Count - 1 ];
						last.Id = curColumn.Labels.Count;
						curColumn.Labels.Add( last );
					}
				}
			}

			return hasMergedColumns;
		}

		/// <summary>
		/// <para>Sets whether or not <see cref="ReportBuilder" /> has already synchronized (extracted) its required resources.</para>
		/// <para>Often used in conjuction with <see cref="RulesEngineUtils.SetBaseResourcePath"/>.</para>
		/// </summary>
		/// <param name="isSynchronized">
		/// <para>Tells <see cref="ReportBuilder" /> &amp; <see cref="RulesEngineUtils"/> whether required resources have already been synchronized to the file system.</para>
		/// <remarks>
		/// <para>When true, no synchronization occurs, overwriting existing files at <see cref="RulesEngineUtils"/>.<see>.baseResourcePath</see> (private).</para>
		/// <para>When false, the first use within the application will synchronize files to <see cref="RulesEngineUtils"/>.<see>.baseResourcePath</see> (private).</para>
		/// </remarks>
		/// </param>
		/// <seealso cref="RulesEngineUtils.SetBaseResourcePath"/>
		public static void SetSynchronizedResources( bool isSynchronized )
		{
			lock( synchronizedResourcesLock )
			{
				SynchronizedResources = isSynchronized;
			}
		}
	}
}

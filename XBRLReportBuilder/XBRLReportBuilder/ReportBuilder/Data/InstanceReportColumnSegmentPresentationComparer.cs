using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Aucent.MAX.AXE.Common.Data;

namespace XBRLReportBuilder
{
	public class InstanceReportColumnSegmentPresentationComparer : IComparer<InstanceReportColumn>
	{
		private Dictionary<string, Hashtable> lstDimensionPresentation = null;
		private Dictionary<string, double> effectiveDimensions = null;
		private Dictionary<string, double> effectiveDimensionDefaults = null;

		private bool hasMultiplePeriods = false;

		public InstanceReportColumnSegmentPresentationComparer( Dictionary<string, Hashtable> lstDimensionPresentationIn, Dictionary<string, double> effectiveDimensionsIn, bool hasMultiplePeriodsIn )
		{
			hasMultiplePeriods = hasMultiplePeriodsIn;

			if( lstDimensionPresentationIn != null )
				lstDimensionPresentation = lstDimensionPresentationIn;

			if( effectiveDimensionsIn != null )
			{
				effectiveDimensions = effectiveDimensionsIn;
				foreach( string dimName in effectiveDimensions.Keys )
				{
					double defaultMemberOrder = double.MinValue;
					if( lstDimensionPresentation.ContainsKey( dimName ) )
					{
						Hashtable htDim = lstDimensionPresentation[ dimName ] as Hashtable;
						foreach( string name in htDim.Keys )
						{
							if( name.StartsWith( "***" ) )
							{
								defaultMemberOrder = double.Parse( htDim[ name ].ToString() );
								break;
							}
						}
					}
					if( effectiveDimensionDefaults == null )
					{
						effectiveDimensionDefaults = new Dictionary<string, double>( 0 );
					}
					if( defaultMemberOrder != double.MinValue )
					{
						effectiveDimensionDefaults[ dimName ] = defaultMemberOrder;
					}
					else
					{
						effectiveDimensionDefaults[ dimName ] = 1.0;
					}
				}
			}
		}

		#region private methods

		private int CompareSegmentMembers( Segment segX, Segment segY )
		{
			double xOrder = 0.0;
			double yOrder = 0.0;

			Hashtable htCurrentDimension = null;
			string memberName = segX.DimensionInfo.Id;
			int pos1 = memberName.IndexOf( "_" );
			if( pos1 >= 0 )
			{
				memberName = memberName.Substring( pos1 + 1 );
			}
			if( lstDimensionPresentation != null )
			{
				foreach( string dimName in lstDimensionPresentation.Keys )
				{
					htCurrentDimension = lstDimensionPresentation[ dimName ] as Hashtable;
					if( htCurrentDimension[ memberName ] != null )
					{
						xOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
						break;
					}
				}
			}

			memberName = segY.DimensionInfo.Id;
			pos1 = memberName.IndexOf( "_" );
			if( pos1 >= 0 )
			{
				memberName = memberName.Substring( pos1 + 1 );
			}
			if( lstDimensionPresentation != null )
			{
				for( int index = 0; index < lstDimensionPresentation.Count; index++ )
				{
					if( htCurrentDimension[ memberName ] != null )
					{
						yOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
						break;
					}
				}
			}

			return xOrder.CompareTo( yOrder );

		}

		private int CompareSegmentMembers( Segment segX, double orderY )
		{
			double xOrder = 0.0;
			double yOrder = orderY;

			Hashtable htCurrentDimension = null;
			string memberName = segX.DimensionInfo.Id;
			int pos1 = memberName.IndexOf( "_" );
			if( pos1 >= 0 )
			{
				memberName = memberName.Substring( pos1 + 1 );
			}
			if( lstDimensionPresentation != null )
			{
				foreach( string dimName in lstDimensionPresentation.Keys )
				{
					htCurrentDimension = lstDimensionPresentation[ dimName ] as Hashtable;
					if( htCurrentDimension[ memberName ] != null )
					{
						xOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
						break;
					}
				}
			}

			return xOrder.CompareTo( yOrder );

		}

		private int CompareSegmentMembers( double orderX, Segment segY )
		{
			double xOrder = orderX;
			double yOrder = 0.0;

			Hashtable htCurrentDimension = null;
			string memberName = segY.DimensionInfo.Id;
			int pos1 = memberName.IndexOf( "_" );
			if( pos1 >= 0 )
			{
				memberName = memberName.Substring( pos1 + 1 );
			}
			if( lstDimensionPresentation != null )
			{
				foreach( string dimName in lstDimensionPresentation.Keys )
				{
					htCurrentDimension = lstDimensionPresentation[ dimName ] as Hashtable;
					if( htCurrentDimension[ memberName ] != null )
					{
						yOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
						break;
					}
				}
			}

			return xOrder.CompareTo( yOrder );

		}


		private int CompareEffectiveDimensions( InstanceReportColumn x, InstanceReportColumn y )
		{
			int result = 0;

			foreach( string eDim in effectiveDimensions.Keys )
			{
				if( hasMultiplePeriods && eDim.StartsWith( "period" ) )
				{
					bool sortDescending = eDim.EndsWith( "column" );

					DateTime xDate = DateTime.MinValue;
					foreach( LabelLine ll in x.Labels )
					{
						if( DateTime.TryParse( ll.Label, out xDate ) )
						{
							break;
						}
					}
					DateTime yDate = DateTime.MinValue;
					foreach( LabelLine ll in y.Labels )
					{
						if( DateTime.TryParse( ll.Label, out yDate ) )
						{
							break;
						}
					}

					if( sortDescending )
					{
						result = yDate.CompareTo( xDate );
					}
					else
					{
						result = xDate.CompareTo( yDate );
					}

					if( result != 0 ) return result;
				}
				else
				{
					bool foundX = false;
					bool foundY = false;
					Segment thisSegX = null;
					Segment thisSegY = null;

					foreach( Segment segX in x.Segments )
					{
						if( segX.ValueType == eDim )
						{
							foundX = true;
							thisSegX = segX;
							break;
						}
					}
					foreach( Segment segY in y.Segments )
					{
						if( segY.ValueType == eDim )
						{
							foundY = true;
							thisSegY = segY;
							break;
						}
					}

					if( !foundX && !foundY )
					{
						continue;
					}
					else
					{
						if( foundX && foundY )
						{
							result = CompareSegmentMembers( thisSegX, thisSegY );
							if( result == 0 )
							{
								continue;
							}
							else
							{
								break;
							}
						}
						else if( foundX && !foundY )
						{
							double defaultOrderY = (double)effectiveDimensionDefaults[ eDim ];
							result = CompareSegmentMembers( thisSegX, defaultOrderY );
							break;
						}
						else if( foundY && !foundX )
						{
							double defaultOrderX = (double)effectiveDimensionDefaults[ eDim ];
							result = CompareSegmentMembers( defaultOrderX, thisSegY );
							break;
						}
					}
				}
			}

			return result;
		}

		private int CompareListDimensionPresentation( InstanceReportColumn x, InstanceReportColumn y )
		{
			double xOrder = 0.0;
			double yOrder = 0.0;

			foreach( Segment s in x.Segments )
			{
				string memberName = s.DimensionInfo.Id;

				if( effectiveDimensions != null && effectiveDimensions.ContainsKey( s.ValueType ) )
				{
					xOrder += effectiveDimensions[ s.DimensionInfo.dimensionId ];
				}
				int pos1 = memberName.IndexOf( "_" );
				if( pos1 >= 0 )
				{
					memberName = memberName.Substring( pos1 + 1 );
				}
				if( lstDimensionPresentation != null )
				{
					foreach( string dimName in lstDimensionPresentation.Keys )
					{
						Hashtable htCurrentDimension = lstDimensionPresentation[ dimName ] as Hashtable;
						if( htCurrentDimension[ memberName ] != null )
						{
							double currentOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
							xOrder += currentOrder;
						}
					}
				}
			}

			foreach( Segment s in y.Segments )
			{
				string memberName = s.DimensionInfo.Id;
				if( effectiveDimensions != null && effectiveDimensions.ContainsKey( s.ValueType ) )
				{
					yOrder += effectiveDimensions[ s.DimensionInfo.dimensionId ];
				}
				int pos1 = memberName.IndexOf( "_" );
				if( pos1 >= 0 )
				{
					memberName = memberName.Substring( pos1 + 1 );
				}
				if( lstDimensionPresentation != null )
				{
					foreach( string dimName in lstDimensionPresentation.Keys )
					{
						Hashtable htCurrentDimension = lstDimensionPresentation[ dimName ] as Hashtable;
						if( htCurrentDimension[ memberName ] != null )
						{
							double currentOrder = double.Parse( htCurrentDimension[ memberName ].ToString() );
							yOrder += currentOrder;
						}
					}
				}
			}

			return xOrder.CompareTo( yOrder );
		}

		#endregion

		#region IComparer<InstanceReportColumn> Members

		public int Compare( InstanceReportColumn x, InstanceReportColumn y )
		{
			int result = 0;

			bool compareEffective = false;
			foreach( string dimName in lstDimensionPresentation.Keys )
			{
				foreach( string effectiveDimName in effectiveDimensions.Keys )
				{
					if( dimName == effectiveDimName )
					{
						compareEffective = true;
						break;
					}
				}
			}
			if( compareEffective )
			{
				result = CompareEffectiveDimensions( x, y );
			}
			else
			{
				result = CompareListDimensionPresentation( x, y );
			}

			return result;
		}

		#endregion
	}
}

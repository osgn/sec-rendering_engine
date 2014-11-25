using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Diagnostics;
using Aucent.MAX.AXE.Common.Data;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		[XmlIgnore]
		public Dictionary<int,string> UnitDictionary = null;

		private void ProcessRounding( ITraceMessenger messenger )
		{
			ProcessRoundingLevel( messenger );

			bool hasMonetary = this.MonetaryRoundingLevel > RoundingLevel.NoRounding;
			bool hasShares = this.SharesRoundingLevel > RoundingLevel.NoRounding;
			bool hasPerShare = this.PerShareRoundingLevel > RoundingLevel.NoRounding;

			if( hasMonetary || hasShares || hasPerShare )
			{
				decimal monetaryDivisor = GetRoundingDivisor( this.MonetaryRoundingLevel );
				decimal sharesDivisor = GetRoundingDivisor( this.SharesRoundingLevel );
				decimal perSharesDivisor = GetRoundingDivisor( this.PerShareRoundingLevel );
				decimal exchangeRateDivisor = GetRoundingDivisor( this.ExchangeRateRoundingLevel );

				//Process rounded values - monetary
				this.Rows.FindAll( row => row.IsMonetary ).ForEach(
					row => row.ApplyRounding( monetaryDivisor ) );

				//Process rounded values - shares
				this.Rows.FindAll( row => row.IsShares ).ForEach(
					row => row.ApplyRounding( sharesDivisor ) );

				//Process rounded values - EPS
				this.Rows.FindAll( row => row.IsEPS ).ForEach(
					row => row.ApplyRounding( perSharesDivisor ) );

				//Process rounded values - Exchange rate
				this.Rows.FindAll( row => row.IsExchangeRate ).ForEach(
					row => row.ApplyRounding( exchangeRateDivisor ) );

				this.Rows.FindAll( row => row.Unit == 0 || row.Unit > UnitType.StandardUnits ).ForEach(
					row => row.ApplyRounding( 1 ) );
			}
			else
			{
				this.Rows.ForEach(
					row => row.Cells.ForEach(
						cell => cell.RoundedNumericAmount = cell.NumericAmount.ToString()
					)
				);
			}
		}

		public static string CalculateRoundedScaledNumber( decimal number, decimal rounding, Precision precisionWrapper )
		{
			decimal tmpRounding = rounding;
			int scaleFactor = 0;
			while( tmpRounding >= 10 )
			{
				scaleFactor++;
				tmpRounding /= 10;
			}

			int precision = 0;
			if( precisionWrapper != null )
				precision = precisionWrapper.NumberOfDigits * -1;

			if( scaleFactor >= precision )
			{
				decimal scaledNumber = number / rounding;

				int decimalPlaces = scaleFactor - precision;
				decimal roundedNumber = Round( scaledNumber, decimalPlaces );

				string decimalFormat = new String( '0', decimalPlaces );
				if( decimalFormat.Length > 0 )
					decimalFormat = "." + decimalFormat;

				//in this scenario, the scaling is GREATER than the rounding
				//therefore we don't have any decimals
				string numberFormat = string.Format( "##################0{0};-##################0{0};0{0}", decimalFormat );
				string formattedNumber = roundedNumber.ToString( numberFormat );
				return formattedNumber;
			}
			else
			{
				decimal scaledNumber = number / rounding;

				//this must be negative or 0
				//ex:
				//Math.Round( 123456, -3 ) == 123000
				int roundoffPlaces = scaleFactor - precision;
				Debug.Assert( roundoffPlaces <= 0 );

				decimal roundedNumber = Round( scaledNumber, roundoffPlaces );

				string decimalFormat = new String( '0', 0 );
				if( decimalFormat.Length > 0 )
					decimalFormat = "." + decimalFormat;

				//in this scenario, the scaling is LESS than the rounding
				//therefore we don't have any decimals
				string formattedNumber = roundedNumber.ToString( "##################0;-##################0;0" );
				return formattedNumber;
			}
		}

		private static decimal Round( decimal number, int decimals )
		{
			if( decimals < 0 )
			{
				//10, 100, 1000, etc
				decimal scaleFactor = (decimal) Math.Pow( 10, decimals * -1 );
				decimal roundedNumber = Math.Round( number / scaleFactor, 0 ) * scaleFactor;
				return roundedNumber;
			}
			else
			{
				decimal roundedNumber = Math.Round( number, decimals );
				return roundedNumber;
			}
		}

		private void ProcessRoundingLevel( ITraceMessenger messenger )
		{
			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.ROUNDING_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				contextObjects.Add( "messenger", messenger );
				builderRules.ProcessRule( RulesEngineUtils.ROUNDING_RULE, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.ROUNDING_RULE );
			}
		}

		public void EvaluateRoundingLevels( ITraceMessenger messenger )
		{
			this.HasCustomUnits = false;
			Dictionary<UnitType, RoundingLevel> selectedRounding = new Dictionary<UnitType, RoundingLevel>();
			foreach( int unit in this.UnitDictionary.Keys )
			{
				UnitType ut = (UnitType)unit;
				selectedRounding[ ut ] = RoundingLevel.UnKnown;

				List<InstanceReportRow> unitRows = this.Rows.FindAll( row => row.Unit == ut );
				if( unitRows.Count == 0 )
					continue;

				if( !this.HasCustomUnits )
				{
					switch( ut )
					{
						case UnitType.EPS:
						case UnitType.ExchangeRate:
						case UnitType.Monetary:
						case UnitType.Shares:
							break;
						default:
							this.HasCustomUnits = true;
							break;
					}
				}

				foreach( InstanceReportRow row in unitRows )
				{
					if( row.MyPrecision == null )
						continue;

					//Numeric data that is Nill for all values will not have a precision
					//defined, so we don't want to include these rows in our processing logic
					if( row.IsNumericDataNil() )
						continue;

					RoundingLevel currentRounding = InstanceUtils.GetRoundingLevelFromPrecision( row.MyPrecision );
					selectedRounding[ ut ] = this.SelectRoundingLevel( row.Unit, selectedRounding[ row.Unit ], currentRounding, row.ElementName, messenger );

					if( selectedRounding[ ut ] > RoundingLevel.NoRounding )
					{
						double factor = Math.Pow( 10, row.MyPrecision.NumberOfDigits * -1 );
						foreach( Cell cell in row.Cells )
						{
							if( cell.IsNil )
								continue;

							if( !cell.HasData || cell.NumericAmount == 0 )
								continue;

							if( Math.Abs( (double)cell.NumericAmount ) < factor )
							{
								selectedRounding[ row.Unit ] = RoundingLevel.NoRounding;
								break;
							}
						}
					}

					if( selectedRounding[ row.Unit ] == RoundingLevel.NoRounding )
						break;
				}
			}

			this.MonetaryRoundingLevel = RoundingLevel.UnKnown;
			if( selectedRounding.ContainsKey( UnitType.Monetary ) )
				this.MonetaryRoundingLevel = selectedRounding[ UnitType.Monetary ];

			this.SharesRoundingLevel = RoundingLevel.UnKnown;
			if( selectedRounding.ContainsKey( UnitType.Shares ) )
				this.SharesRoundingLevel = selectedRounding[ UnitType.Shares ];

			this.PerShareRoundingLevel = RoundingLevel.UnKnown;
			if( selectedRounding.ContainsKey( UnitType.EPS ) )
				this.PerShareRoundingLevel = selectedRounding[ UnitType.EPS ];

			this.ExchangeRateRoundingLevel = RoundingLevel.UnKnown;
			if( selectedRounding.ContainsKey( UnitType.ExchangeRate ) )
				this.ExchangeRateRoundingLevel = selectedRounding[ UnitType.ExchangeRate ];
		}

		private RoundingLevel SelectRoundingLevel( UnitType unitType, RoundingLevel unitTypeRounding, RoundingLevel elementRounding, string elementName, ITraceMessenger messenger )
		{
			string typeName = string.Empty;
			switch( unitType )
			{
				case UnitType.Monetary:
					typeName = "Monetary";
					break;
				case UnitType.Shares:
					typeName = "Shares";
					break;
				case UnitType.EPS:
					typeName = "Per Share";
					break;
				case UnitType.ExchangeRate:
					typeName = "Exchange Rate";
					break;
			}

			if( unitTypeRounding == RoundingLevel.UnKnown )
			{
				//If sharesRounding has not been set yet then set the value equal to the
				//value of the current row
				return elementRounding;
			}
			else if( !AreRoundingLevelsEquivalent( unitTypeRounding, elementRounding ) )
			{
				if( messenger != null )
				{
					string info = "'" + typeName + "' elements on report '" + this.ReportLongName + "' had a mix of different decimal attribute values.";
					messenger.TraceInformation( info );
				}

				//Make sure the rounding level on the current row matches
				//the rounding of the previous rows
				return RoundingLevel.NoRounding;
			}
			else if( elementRounding < unitTypeRounding )
			{
				if( elementRounding == RoundingLevel.NoRounding || elementRounding == RoundingLevel.UnKnown )
				{
					if( messenger != null )
					{
                        string info = "'" + typeName + "' elements on report '" + this.ReportLongName + "' had a mix of different decimal attribute values.";
						messenger.TraceInformation( info );
					}
				}

				//The rounding levels are equivalent, now make sure we use the
				//"Lowest" equivalent RoundingLevel
				return elementRounding;
			}

			return unitTypeRounding;
		}


		public void SetRoundingOption(
			string uniformRoundingString,

			string sharesNoRoundingString,
			string sharesRoundingString,

			string bothSharesNoRoundingString,
			string bothSharesRoundingString,

			string perShareNoRoundingString,
			string perShareRoundingString,

			string otherRoundingString )
		{
			if( uniformRoundingString.Split( new string[] { ROUNDING_LEVEL_PLACEHOLDER }, StringSplitOptions.None ).Length > 2 )
				throw new Exception( "Invalid format for uniformRoundingString" );
			else
				uniformRoundingString = uniformRoundingString.Replace( ROUNDING_LEVEL_PLACEHOLDER, "{0}" );

			if( sharesRoundingString.Split( new string[] { ROUNDING_LEVEL_PLACEHOLDER }, StringSplitOptions.None ).Length > 2 )
				throw new Exception( "Invalid format for shareRoundingString" );
			else
				sharesRoundingString = sharesRoundingString.Replace( ROUNDING_LEVEL_PLACEHOLDER, "{0}" );

			if( bothSharesRoundingString.Split( new string[] { ROUNDING_LEVEL_PLACEHOLDER }, StringSplitOptions.None ).Length > 2 )
				throw new Exception( "Invalid format for shareRoundingString" );
			else
				bothSharesRoundingString = bothSharesRoundingString.Replace( ROUNDING_LEVEL_PLACEHOLDER, "{0}" );


			//Let's get the simple scenarios out of the way...
			//#1 - If all of the "known" items are of the same scale, then return that scale
			Dictionary<RoundingLevel, int> levels = new Dictionary<RoundingLevel, int>();

			//Store the normalized value

			RoundingLevel roundingMonetary = GetNormalizedRoundingLevel( this.MonetaryRoundingLevel );
			levels[ roundingMonetary ] = 1;  //monetary

			RoundingLevel roundingShares = GetNormalizedRoundingLevel( this.SharesRoundingLevel );
			levels[ roundingShares ] = 1;  //shares

			RoundingLevel roundingPerShare = GetNormalizedRoundingLevel( this.PerShareRoundingLevel );
			levels[ roundingPerShare ] = 1;  //per share

			levels.Remove( RoundingLevel.UnKnown );
			if( levels.Count == 0 )
			{
				//All values were unknown
				this.RoundingOption = string.Empty;
				return;
			}

			//if we only have 1 distinct level, return that value in order of precedence
			if( levels.Count == 1 )
			{
				RoundingLevel[] tmpLevel = new RoundingLevel[ 1 ];
				levels.Keys.CopyTo( tmpLevel, 0 );

				RoundingLevel level = tmpLevel[ 0 ];
				if( level == RoundingLevel.NoRounding )
				{
					//All values were whole numbers
					this.RoundingOption = string.Empty;
					return;
				}

				if( level == roundingMonetary )
				{
					string stLevel = ReportBuilder.GetRoundingLevelString( roundingMonetary );
					this.RoundingOption = string.Format( uniformRoundingString, stLevel );

					if( this.HasCustomUnits )
						this.RoundingOption += ", " + otherRoundingString;

					return;
				}

				if( level == roundingShares )
				{
					string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
					this.RoundingOption = string.Format( uniformRoundingString, stLevel );

					if( this.HasCustomUnits )
						this.RoundingOption += ", " + otherRoundingString;

					return;
				}
			}

			int roundingLevels = 0;
			StringBuilder sbRoundingOption = new StringBuilder();
			WriteMonetary( ref roundingLevels, sbRoundingOption, roundingMonetary, uniformRoundingString );

			WriteShares( ref roundingLevels, sbRoundingOption,
					roundingMonetary, roundingShares, roundingPerShare,
					uniformRoundingString, sharesNoRoundingString, sharesRoundingString,
					bothSharesNoRoundingString, bothSharesRoundingString );


			if( this.HasCustomUnits )
			{
				//The`otherRoundingString` should be the final string
				//Increasing the `roundingLevel` negates the possibility of entering the "per share" block
				//   it also negates the possibility of duplicates below

				if( roundingLevels == 2 )
				{
					roundingLevels = 3;
					sbRoundingOption.Append( ", " + otherRoundingString );
				}
				else if( roundingLevels == 1 )
				{
					if( roundingPerShare == RoundingLevel.UnKnown ||
						roundingPerShare == roundingShares )
					{
						roundingLevels = 3;
						sbRoundingOption.Append( ", " + otherRoundingString );
					}
				}
			}

			if( roundingLevels < 3 )
			{
				WritePerShare( ref roundingLevels, sbRoundingOption, roundingMonetary, roundingShares, roundingPerShare,
					perShareNoRoundingString, perShareRoundingString, otherRoundingString );
			}

			if( this.HasCustomUnits )
			{
				if( roundingLevels == 2 )
					sbRoundingOption.Append( ", " + otherRoundingString );
			}

			this.RoundingOption = sbRoundingOption.ToString();
		}

		private void WriteMonetary( ref int roundingLevels, StringBuilder sbRoundingOption, RoundingLevel roundingMonetary, string uniformRoundingString )
		{
			//nothing to see here
			if( roundingMonetary <= RoundingLevel.NoRounding )
				return;

			roundingLevels++;
			string stLevel = ReportBuilder.GetRoundingLevelString( roundingMonetary );
			sbRoundingOption.Append( string.Format( uniformRoundingString, stLevel ) );
		}

		private void WriteShares( ref int roundingLevels, StringBuilder sbRoundingOption,
			RoundingLevel roundingMonetary, RoundingLevel roundingShares, RoundingLevel roundingPerShare,
			string uniformRoundingString,
			string sharesNoRoundingString, string sharesRoundingString,
			string bothSharesNoRoundingString, string bothSharesRoundingString )
		{
			//nothing to see here
			if( roundingShares == RoundingLevel.UnKnown )
				return;

			//if we are equal, this value has already been set
			if( roundingShares == roundingMonetary )
				return;

			//if not monetary, and not shares, do not set the rounding
			if( roundingMonetary <= RoundingLevel.NoRounding &&
				roundingShares <= RoundingLevel.NoRounding )
				return;

			//write out the "uniform" rounding string
			if( roundingMonetary == RoundingLevel.UnKnown &&
				roundingShares > RoundingLevel.NoRounding )
			{
				roundingLevels++;
				string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
				sbRoundingOption.Append( string.Format( uniformRoundingString, stLevel ) );
				return;
			}

			if( roundingMonetary == RoundingLevel.NoRounding &&
				roundingShares > RoundingLevel.NoRounding )
			{
				if( roundingShares != roundingPerShare )
				{
					string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
					sbRoundingOption.Append( string.Format( sharesRoundingString, stLevel ) );
				}
				else
				{
					string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
					sbRoundingOption.Append( string.Format( bothSharesRoundingString, stLevel ) );
				}

				roundingLevels++;
				return;
			}

			if( roundingMonetary > RoundingLevel.NoRounding )
			{
				//the scenario where these match is covered above
				if( roundingShares > RoundingLevel.UnKnown )
					sbRoundingOption.Append( ", except " );

				if( roundingShares == RoundingLevel.NoRounding )
				{
					if( roundingPerShare == RoundingLevel.NoRounding )
						sbRoundingOption.Append( bothSharesNoRoundingString );
					else
						sbRoundingOption.Append( sharesNoRoundingString );
				}
				else
				{
					if( roundingShares == roundingPerShare )
					{
						string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
						sbRoundingOption.Append( string.Format( bothSharesRoundingString, stLevel ) );
					}
					else
					{
						string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
						sbRoundingOption.Append( string.Format( sharesRoundingString, stLevel ) );
					}
				}

				roundingLevels++;
				return;
			}
		}

		private void WritePerShare( ref int roundingLevels, StringBuilder sbRoundingOption,
			RoundingLevel roundingMonetary, RoundingLevel roundingShares, RoundingLevel roundingPerShare,
			string perShareNoRoundingString, string perShareRoundingString, string otherRoundingString )
		{
			//nothing to see here
			if( roundingPerShare == RoundingLevel.UnKnown )
				return;

			//still nothing to see here
			if( roundingLevels == 0 )
				return;

			//if we are equal, this value has already been set
			if( roundingPerShare == roundingShares )
				return;

			if( roundingLevels == 1 )
			{
				sbRoundingOption.Append( ", except " );

				if( roundingPerShare == RoundingLevel.NoRounding )
				{
					sbRoundingOption.Append( perShareNoRoundingString );
				}
				else
				{
					string stLevel = ReportBuilder.GetRoundingLevelString( roundingShares );
					sbRoundingOption.Append( string.Format( perShareRoundingString, stLevel ) );
				}
			}
			else //2
			{
				//if we are equal, this value has already been set
				if( roundingPerShare == roundingMonetary )
					return;

				sbRoundingOption.Append( ", " + otherRoundingString );
			}

			roundingLevels++;
		}


		private RoundingLevel GetNormalizedRoundingLevel( RoundingLevel roundingLevel )
		{
			switch( roundingLevel )
			{
				case RoundingLevel.UnKnown:
				case RoundingLevel.NoRounding:
					return roundingLevel;
			}

			try
			{
				string levelStr = ReportBuilder.GetRoundingLevelString( roundingLevel );
				return (RoundingLevel)Enum.Parse( typeof( RoundingLevel ), levelStr );
			}
			catch { }

			return RoundingLevel.UnKnown;
		}

		private static decimal GetRoundingDivisor( RoundingLevel roundingLevel )
		{
			decimal divisor = 1;
			switch( roundingLevel )
			{
                case RoundingLevel.Quadrillions:
                case RoundingLevel.TenTrillions:
                case RoundingLevel.HundredTrillions:
                    divisor = (decimal)(Math.Pow(10d, (double)RoundingLevel.Quadrillions));
                    break;
                case RoundingLevel.Trillions:
				case RoundingLevel.TenBillions:
				case RoundingLevel.HundredBillions:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Trillions ) );
					break;
				case RoundingLevel.Billions:
				case RoundingLevel.TenMillions:
				case RoundingLevel.HundredMillions:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Billions ) );
					break;
				case RoundingLevel.Millions:
				case RoundingLevel.TenThousands:
				case RoundingLevel.HundredThousands:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Millions ) );
					break;
				case RoundingLevel.Thousands:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Thousands ) );
					break;
				case RoundingLevel.Hundreds:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Hundreds ) );
					break;
				case RoundingLevel.Tens:
					divisor = (decimal)( Math.Pow( 10d, (double)RoundingLevel.Tens ) );
					break;
				default:
					divisor = 1;
					break;
			}
			return divisor;
		}

		private static bool AreRoundingLevelsEquivalent( RoundingLevel levelX, RoundingLevel levelY )
		{
			bool equal = false;
			switch( levelX )
			{
                case RoundingLevel.Quadrillions:
                case RoundingLevel.TenTrillions:
                case RoundingLevel.HundredTrillions:
                    equal = levelY == RoundingLevel.Quadrillions ||
                            levelY == RoundingLevel.TenTrillions ||
                            levelY == RoundingLevel.HundredTrillions;
                    break;
                case RoundingLevel.Trillions:
				case RoundingLevel.TenBillions:
				case RoundingLevel.HundredBillions:
					equal = levelY == RoundingLevel.Trillions ||
							levelY == RoundingLevel.TenBillions ||
							levelY == RoundingLevel.HundredBillions;
					break;
				case RoundingLevel.Billions:
				case RoundingLevel.TenMillions:
				case RoundingLevel.HundredMillions:
					equal = levelY == RoundingLevel.Billions ||
							levelY == RoundingLevel.TenMillions ||
							levelY == RoundingLevel.HundredMillions;
					break;
				case RoundingLevel.Millions:
				case RoundingLevel.TenThousands:
				case RoundingLevel.HundredThousands:
					equal = levelY == RoundingLevel.Millions ||
							levelY == RoundingLevel.TenThousands ||
							levelY == RoundingLevel.HundredThousands;
					break;
				default:
					equal = levelX == levelY;
					break;
			}
			return equal;
		}

		private int GetRoundingDecimalPlaces( RoundingLevel roundingLevel )
		{
			int decimalPlaces = 0;
			switch( roundingLevel )
			{
                case RoundingLevel.HundredTrillions:
                case RoundingLevel.HundredBillions:
				case RoundingLevel.HundredMillions:
				case RoundingLevel.HundredThousands:
				case RoundingLevel.Hundreds:
					decimalPlaces = 1;
					break;
                case RoundingLevel.TenTrillions:
                case RoundingLevel.TenBillions:
				case RoundingLevel.TenMillions:
				case RoundingLevel.TenThousands:
				case RoundingLevel.Tens:
				case RoundingLevel.NoRounding:
					decimalPlaces = 2;
					break;
				default:
					decimalPlaces = 0;
					break;
			}
			return decimalPlaces;
		}

		private string GetRoundedNumberFormatString( RoundingLevel roundingLevel )
		{
			string numberFormat = string.Empty;
			switch( roundingLevel )
			{
                case RoundingLevel.Quadrillions:
                case RoundingLevel.Trillions:
				case RoundingLevel.Billions:
				case RoundingLevel.Millions:
				case RoundingLevel.Thousands:
					numberFormat = "##################0;-##################0;0";
					break;
                case RoundingLevel.HundredTrillions:
                case RoundingLevel.HundredBillions:
				case RoundingLevel.HundredMillions:
				case RoundingLevel.HundredThousands:
				case RoundingLevel.Hundreds:
					numberFormat = "##################0.0;-##################0.0;0.0";
					break;
                case RoundingLevel.TenTrillions:
                case RoundingLevel.TenBillions:
				case RoundingLevel.TenMillions:
				case RoundingLevel.TenThousands:
				case RoundingLevel.Tens:
				case RoundingLevel.NoRounding:
					numberFormat = "##################0.00;-##################0.00;0.00";
					break;
				default:
					numberFormat = "##################0;-##################0;0";
					break;
			}
			return numberFormat;
		}
	}
}

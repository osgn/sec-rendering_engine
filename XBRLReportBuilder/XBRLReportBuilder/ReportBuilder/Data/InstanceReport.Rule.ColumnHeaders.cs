using System;
using System.Collections.Generic;
using XBRLReportBuilder.Utilities;
using System.Globalization;


namespace XBRLReportBuilder
{
	public partial class InstanceReport
	{
		protected void ProcessColumnHeaders()
		{
			//Apply new column header here if contains two currencies:
			if( this.Columns.Count == 0 )
				return;

			bool isRuleEnabled = this.FireRuleProcessing( RulesEngineUtils.COLUMN_HEADERS_RULE );
			if( isRuleEnabled )
			{
				Dictionary<string, object> contextObjects = new Dictionary<string, object>();
				contextObjects.Add( "InstanceReport", this );
				builderRules.ProcessRule( RulesEngineUtils.COLUMN_HEADERS_RULE, contextObjects );

				this.FireRuleProcessed( RulesEngineUtils.COLUMN_HEADERS_RULE );
			}
		} 

        /// <summary>
        /// Processes all columns across the report, removing the default calendar items, and replacing them with
        /// more intelligent representations based on the period type and dates specified.
        /// </summary>
        /// <param name="dateFormat">The formatting string to use for formatting the end date results.  Note that 
        /// the pattern "MMM." (with period) will have its period removed for May in all English locales.</param>
        /// <param name="durationText">The text to use for a superheading when a duration is present, for example:
        /// "{n} Months Ended" where "{n}" will be replaced by the number of months elapsed.</param>
        /// <param name="cultureSetting">A string override to specify which culture to use for formatting.</param>
        public void SetCalendarLabels( string dateFormat, string durationText )
        {

            // The {n} is more logical than {0} for user input in this narrow scope
            durationText = durationText.Replace( "{n}", "{0}" );

            // For English cultures, we don't want "May." -- remove the decimal
            // Added by Defect DE1512
            string mayDateFormat = dateFormat;
            if( CultureInfo.CurrentCulture.ToString().IndexOf( "en-" ) == 0 )
            {
                mayDateFormat = mayDateFormat.Replace( "MMM.", "MMM" );
            }

            foreach( InstanceReportColumn iRC in this.Columns )
            {
                // We only alter instants and durations, but do so differently
                string newDateLabel = string.Empty;
                string newSpanLabel = string.Empty;

                switch( iRC.MyPeriodType )
                {

                    case Aucent.MAX.AXE.XBRLParser.Element.PeriodType.duration:

                        // Treat 0 or fewer days as instant
                        //if( iRC.ReportingSpan.Days <= 0 )
                        //    goto case Aucent.MAX.AXE.XBRLParser.Element.PeriodType.instant;

                        // May in English is treated differently, see mayDateFormat declaration above
                        if( iRC.MyContextProperty.PeriodEndDate.Month == 5 )
                        {
                            // The .ToString will respect locale
                            newDateLabel = iRC.MyContextProperty.PeriodEndDate.ToString( mayDateFormat );
                        }
                        else
                        {
                            // The .ToString will respect locale
                            newDateLabel = iRC.MyContextProperty.PeriodEndDate.ToString( dateFormat );
                        }

                        

                        // A month is not 30 days!  This call will also be more tolerant on standard numbers
                        int monthsEnded = InstanceReport.CalculateMonthsEnded( iRC.ReportingSpan.Days );

                        newSpanLabel = string.Format( durationText, monthsEnded );

                        break;


                    case Aucent.MAX.AXE.XBRLParser.Element.PeriodType.instant:
                        // May in English is treated differently, see mayDateFormat declaration above
                        if( iRC.MyContextProperty.PeriodStartDate.Month == 5 )
                        {
                            // The instant type uses the first day, not last day for the duration types
                            newDateLabel = iRC.MyContextProperty.PeriodStartDate.ToString( mayDateFormat );
                        }
                        else
                        {
                            // The instant type uses the first day, not last day for the duration types
                            newDateLabel = iRC.MyContextProperty.PeriodStartDate.ToString( dateFormat );
                        }
                        

                        break;

                    // These types cannot be processed as a date
                    case Aucent.MAX.AXE.XBRLParser.Element.PeriodType.forever:
                    case Aucent.MAX.AXE.XBRLParser.Element.PeriodType.na:
                    default:
                        break;
				}

				//only apply calendar labels if the previous calendar was found.
				bool removed = false;

                // There's no point in doing any work if we're not altering a label
                if( !string.IsNullOrEmpty( newDateLabel ) )
                {
                    // Remove initial "Calendar" label, should be only one
                    for( int i = 0; i < iRC.Labels.Count; i++ )
                    {
                        if( iRC.Labels[ i ].Key == "Calendar" )
                        {
							removed = true;
                            iRC.Labels.RemoveAt( i );
                            break;
                        }
                    }

					//only apply calendar labels if the previous calendar was found.
					if( removed )
					{
						iRC.PrependLabel( newDateLabel, "Calendar" );

						if( !string.IsNullOrEmpty( newSpanLabel ) )
						{
							iRC.PrependLabel( newSpanLabel, "CalendarSupplement" );
						}
					}
                }

					//only apply calendar labels if the previous calendar was found.
				if( removed )
				{
					// Since we've shuffled labels, reindex them
					for( int i = 0; i < iRC.Labels.Count; i++ )
					{
						iRC.Labels[ i ].Id = i;
					}
				}
            }
        
        }

        /// <summary>
        /// Calculate the number of months elapsed within a specified number of days.  Returns -1 if the dates are the same or the end date occurs before the start date.
        /// </summary>
        /// <param name="daysCount">Number of days elapsed.</param>
        /// <returns>A <see cref="T:int"/> specifying how many months have elapsed between the two dates, or -1 if less than 1 day has elapsed.</returns>
        private static int CalculateMonthsEnded( int daysCount ) {
            // This is an average number of days per month over a 4 year period (with a leap year)
            // (365*4 + 1) / 48
            const double DAYS_PER_MONTH = 30.4375;

            int monthsCount = 0;

            // Use tolerant bucket logic to create a wider range for standard timeframes
            if (daysCount >= 25 && daysCount <= 35)
                monthsCount = 1;
            else if (daysCount >= 70 && daysCount <= 110)
                monthsCount = 3;
            else if (daysCount >= 160 && daysCount <= 200)
                monthsCount = 6;
            else if (daysCount >= 250 && daysCount <= 290)
                monthsCount = 9;
            else if (daysCount >= 345 && daysCount <= 385)
                monthsCount = 12;
            else if (daysCount >= 430 && daysCount <= 470)
                monthsCount = 15;
            else if (daysCount >= 520 && daysCount <= 560)
                monthsCount = 18;

            // If buckets don't fit, use an approximation based on number of days elapsed
            // This is leap year and long time period safe
            if( daysCount > 0 && monthsCount == 0 )
            {
                monthsCount = (int) Math.Round( daysCount / DAYS_PER_MONTH );
            }

            return monthsCount;
        }
	}
}
using System;
using System.Collections.Generic;
using System.Text;

namespace XBRLReportBuilder
{
    /// <summary>
    /// The sole purpose of this class is to make unexecuted calls to the Rules methods.  These methods
    /// are not called by any code directly, instead they are invoked by ReportBuilder.BuilderRules.ProcessRule.
    /// Without these calls, the methods would be flagged as unreferenced and may be inadvertently removed.
    /// DO NOT instantiate this class or invoke any of its methods!
    /// </summary>
    class RulesHolder
    {
        public RulesHolder()
        {
            ReportBuilder neverUsed = new ReportBuilder();

			//CleanupFlowthroughColumns.xml
			neverUsed.CleanupFlowThroughColumns();
			bool isNewGAAP = neverUsed.IsNewGAAP;
			bool isGAAP2005 = neverUsed.IsGAAP2005;

			//CleanupFlowthroughReports.xml
            neverUsed.CleanupFlowThroughReports();
			isGAAP2005 = neverUsed.IsGAAP2005;

			//DisplayAsRatio.xml
			neverUsed.CurrentElementName += neverUsed.CurrentElementName;
            neverUsed.IsRatioElement |= neverUsed.IsRatioElement;

			//DisplayZeroAsNone.xml
			neverUsed.CurrentElementName += neverUsed.CurrentElementName;
            neverUsed.IsZeroAsNoneElement |= neverUsed.IsZeroAsNoneElement;

			//TotalLabel.xml
			string currentRowPreferredLabelRole = neverUsed.CurrentRowPreferredLabelRole;

            InstanceReport neverUsedReport = new InstanceReport();

			//ColumnHeaders.xml
			neverUsedReport.SetCalendarLabels( null, null );

			//CurrencySymbol.xml
            neverUsedReport.AssignCurrencySymbol();

			//EquityStatement.xml
            neverUsedReport.ProcessEquity(null, null, null);

			//InstantAndDuration.xml
            neverUsedReport.ProcessMergeInstanceDuration_Rule(null);

			//ProcessBeginningEndingBalances.xml
            neverUsedReport.ProcessBeginningAndEndingBalances();

			//PromoteSharedLabels.xml
            neverUsedReport.GetSegmentScenarioLabels();
            neverUsedReport.GetCurrencyLabels();
            neverUsedReport.GetSharedSegmentsAndScenariosLabel(null);
            neverUsedReport.GetSharedCurrencyLabel(null);
            neverUsedReport.ConcatenateSharedLabels(null, null);

			//Rounding.xml
            neverUsedReport.EvaluateRoundingLevels(null);
            neverUsedReport.SetRoundingOption(null, null, null, null, null, null, null, null);

			//Segments.xml
            neverUsedReport.ProcessSegments_Rule(null);

        }
    }
}

// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Exceptions;
using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// Provides properties and methods to encapsulate an XBRL taxonomy.
    /// </summary>
    public partial class Taxonomy : DocumentBase
    {
        #region Negated Labels

        /// <summary>
        /// Returns true if the Taxonomy has a negated label role defined.
        /// </summary>
        /// <returns></returns>
        public bool HasNegatedLabelRoleDefined
        {
            get
            {
                if (this.labelRoles != null && this.labelRoles.Count > 0)
                {
                    foreach (string role in this.labelRoles)
                    {
                        if (IsNegatedLabelRole(role))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }


        static List<string> negatedLabelList = null;
        /// <summary>
        /// Tests if the give label role is a recognized negated label role.
        /// </summary>
        /// <param name="labelRoleToTest"></param>
        /// <returns></returns>
        public static bool IsNegatedLabelRole(string labelRoleToTest)
        {
            if (negatedLabelList == null)
            {
                negatedLabelList = new List<string>();
                negatedLabelList.Add("negated");
                negatedLabelList.Add("negatedTotal");
                negatedLabelList.Add("negatedPeriodStart");
                negatedLabelList.Add("negatedPeriodEnd");

                negatedLabelList.Add("negatedLabel");
                negatedLabelList.Add("negatedTotalLabel");
                negatedLabelList.Add("negatedPeriodStartLabel");
                negatedLabelList.Add("negatedPeriodEndLabel");
                negatedLabelList.Add("negatedNetLabel");
                negatedLabelList.Add("negatedTerseLabel");


            }


            if (!string.IsNullOrEmpty(labelRoleToTest))
            {
                if (negatedLabelList.Contains(labelRoleToTest))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests if the give label role is a recognized total label role.
        /// </summary>
        /// <param name="labelRoleToTest"></param>
        /// <returns></returns>
        public static bool IsTotalLabelRole(string labelRoleToTest)
        {
            if (!string.IsNullOrEmpty(labelRoleToTest))
            {
                if (labelRoleToTest == NEGATED_TOTAL || labelRoleToTest == NEGATED_TOTAL2 || labelRoleToTest == LABEL_TOTAL)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds a corresponding total label role for the given label role.
        /// For Example:
        ///		"label"				==> "totalLabel"
        ///		"negated"		==> "negatedTotal"
        /// </summary>
        /// <param name="otherLabelRole">The original label role we want to be converted to a total label role.</param>
        /// <param name="totalLabelRole">The total label role resultant of the conversion.</param>
        /// <returns></returns>
        public bool ConvertLabelRoleToTotalLabelRole(string otherLabelRole, out string totalLabelRole)
        {
            totalLabelRole = null;

            if (labelRoles != null && labelRoles.Count > 0)
            {
                if (string.IsNullOrEmpty(otherLabelRole))
                {
                    return TryGetDefaultTotalLabelRole(out totalLabelRole);
                }

                if (otherLabelRole.ToLower().Contains( "periodstart")
                    || otherLabelRole.ToLower().Contains("periodend") )
                {
                    return false;
                }

                string totalRoleToFind = null;
                if (otherLabelRole == LABEL)
                {
                    totalRoleToFind = LABEL_TOTAL;
                }
                else if (otherLabelRole == NEGATED)
                {
                    totalRoleToFind = NEGATED_TOTAL;
                }
                else if (otherLabelRole == NEGATED2)
                {
                    totalRoleToFind = NEGATED_TOTAL2;
                }

                if (!string.IsNullOrEmpty(totalRoleToFind) && this.labelRoles.BinarySearch(totalRoleToFind) >= 0)
                {
                    totalLabelRole = totalRoleToFind;
                }
                else
                {
                    TryGetDefaultTotalLabelRole(out totalLabelRole);
                }
            }
            return (!string.IsNullOrEmpty(totalLabelRole));
        }

        /// <summary>
        /// Finds a corresponding negated label role for the given label role.
        /// For Example:
        ///		"label"				==> "negated"
        ///		"totalLabel"		==> "negatedTotal"
        ///		"periodStartLabel"	==> "negatedPeriodStart"
        ///		"periodEndLabel"	==> "negatedPeriodEnd"
        /// </summary>
        /// <param name="otherLabelRole">The original label role we want to be converted to a negated label role.</param>
        /// <param name="negatedLabelRole">The negated label role resultant of the conversion.</param>
        /// <returns></returns>
        public bool ConvertLabelRoleToNegatedLabelRole(string otherLabelRole, out string negatedLabelRole)
        {
            negatedLabelRole = null;

            if (this.labelRoles != null && this.labelRoles.Count > 0)
            {
                if (string.IsNullOrEmpty(otherLabelRole))
                {
                    return TryGetDefaultNegatedLabelRole(out negatedLabelRole);
                }

                string negatedRoleToFind = null;
                if (otherLabelRole == LABEL)
                {
                    negatedRoleToFind = this.labelRoles.BinarySearch(NEGATED) >= 0 ? NEGATED : NEGATED2; 
                }
                else if (otherLabelRole == LABEL_TOTAL)
                {
                    negatedRoleToFind = this.labelRoles.BinarySearch(NEGATED_TOTAL) >= 0 ? NEGATED_TOTAL : NEGATED_TOTAL2;  
                }
                else if (otherLabelRole == LABEL_PER_START)
                {
                    negatedRoleToFind = this.labelRoles.BinarySearch(NEGATED_PER_START) >= 0 ? NEGATED_PER_START : NEGATED_PER_START2;
                }
                else if (otherLabelRole == LABEL_PER_END)
                {
                    negatedRoleToFind = this.labelRoles.BinarySearch(NEGATED_PER_END) >= 0 ? NEGATED_PER_END : NEGATED_PER_END2;
                }

                if (!string.IsNullOrEmpty(negatedRoleToFind) && this.labelRoles.BinarySearch(negatedRoleToFind) >= 0)
                {
                    negatedLabelRole = negatedRoleToFind;
                }
                else
                {
                    TryGetDefaultNegatedLabelRole(out negatedLabelRole);
                }

            }

            return (!string.IsNullOrEmpty(negatedLabelRole));
        }

        /// <summary>
        /// Finds a corresponding label role for the given negated label role.
        /// For Example:
        ///		"negated"				==> "label"
        ///		"negatedTotal"			==> "totalLabel"
        ///		"negatedPeriodStart"	==> "periodStartLabel"
        ///		"negatedPeriodEnd"		==> "periodEndLabel"
        /// </summary>
        /// <param name="negatedLabelRole">The original negated label role we want to convert to a non-negated label role.</param>
        /// <param name="otherLabelRole">The non-negated label role resultant of the conversion.</param>
        /// <returns></returns>
        public bool ConvertNegatedLabelRoleToNonNegatedRole(string negatedLabelRole, out string otherLabelRole)
        {
            otherLabelRole = null;
            if (string.IsNullOrEmpty(negatedLabelRole))
            {
                otherLabelRole = LABEL;
            }
            else
            {
                switch(negatedLabelRole)
                {
                    case NEGATED:
                    case NEGATED2:
                        otherLabelRole = LABEL;
                        break;

                    case NEGATED_TOTAL:
                    case NEGATED_TOTAL2:
                        otherLabelRole = LABEL_TOTAL;
                        break;

                    case NEGATED_PER_START:
                    case NEGATED_PER_START2:
                        otherLabelRole = LABEL_PER_START;
                        break;

                    case NEGATED_PER_END:
                    case NEGATED_PER_END2:
                        otherLabelRole = LABEL_PER_END;
                        break;


                }


            
            }

            return (!string.IsNullOrEmpty(otherLabelRole));
        }

        /// <summary>
        /// Finds a corresponding label role for the given total label role.
        /// For Example:
        ///		"totalLabel"			==> "label"
        ///		"negatedTotal"			==> "negated"
        /// </summary>
        /// <param name="totalLabelRole">The original total label role we want to convert to a non-total label role.</param>
        /// <param name="otherLabelRole">The non-total label role resultant of the conversion.</param>
        /// <returns></returns>
        public bool ConvertTotalLabelRoleToNonTotalRole(string totalLabelRole, out string otherLabelRole)
        {
            otherLabelRole = null;
            if (string.IsNullOrEmpty(totalLabelRole))
            {
                otherLabelRole = LABEL;
            }
            else
            {
                if (totalLabelRole == NEGATED_TOTAL)
                {
                    otherLabelRole = NEGATED;
                }
                else if (totalLabelRole == NEGATED_TOTAL2)
                {
                    otherLabelRole = NEGATED2;
                }
                else if (totalLabelRole == LABEL_TOTAL)
                {
                    otherLabelRole = LABEL;
                }
            }

            return (!string.IsNullOrEmpty(otherLabelRole));
        }

        internal bool TryGetDefaultTotalLabelRole(out string defaultTotalLabel)
        {
            defaultTotalLabel = LABEL_TOTAL;
            return true;

           


        }

        internal bool TryGetDefaultNegatedLabelRole(out string defaultNegatedLabel)
        {
            defaultNegatedLabel = null;
            if (this.labelRoles != null && this.labelRoles.Count > 0)
            {
                if (this.labelRoles.BinarySearch(NEGATED) >= 0)
                {
                    defaultNegatedLabel = NEGATED;
                }
                else if (this.labelRoles.BinarySearch(NEGATED2) >= 0)
                {
                    defaultNegatedLabel = NEGATED2;
                }
                

            }

            return (!string.IsNullOrEmpty(defaultNegatedLabel));
        }

        #endregion

        private static string SECReportDefinitionRegex = @"[0-9]+ - ((Statement)|(Disclosure)|(Schedule)|(Document)) - .*[\S]";
        public static string SECDisclosureReportDefinitionRegex = @"[0-9]+ - ((Disclosure)|(Schedule)) - .*[\S]";
        public static string SECReportDefinitionRegexSpecialNotAllowed = @"[\{\}\[\]]+";
        internal static string disNumberFormat = @"[0-9][0-9][0-9][0-9]";
        public string GetExtendedRoleNamePrefix()
        {
            if (this.IsAucentExtension)
            {
                List<RoleType> rts;
                if (this.TryGetUsedRoleTypes(out rts))
                {
                    string extFileName = Path.GetFileName(this.infos[0].Location);

                    foreach (RoleType rt in rts)
                    {
                        if (string.Compare(extFileName, Path.GetFileName(rt.SchemaFullFileName), true) == 0)
                        {
                            return rt.Uri.Substring(0, rt.Uri.LastIndexOf("/") + 1);

                        }
                    }

                }


            }


            return null;
        }
        #region Reviewer's Guide Report Sequence

        /// <summary>
        /// 00100 – Facing financial statements – increasing by 100s
        //10101 – Level 1 Block tag (1 indicates block, 01 is the footnote number, and 1 is the XBRL detail level).  Example: Note 16 Fair Value Measurements = 11601.
        //20102 – Level 2 Policy tag (2 indicates policies, 01 is the footnote number, and 2 is the XBRL detail level). Example: Note 16 Fair Value Measurements (Policy) = 21602.
        //30103 – Level 3 Tables tag (3 indicates tables, 01 is the footnote number, and 3 is the XBRL detail level). Example: Note 16 Fair Value Measurements (Tables) = 31603.
        //40101 – Level 4 Detail tag (4 indicates details, 01 is the footnote number, and 01 is the XBRL detail level). Example: Note 16 Fair Value Measurements (Details) = 41601.
        //40111 – Level 4 Detail parenthetical tag (4 indicates details, 01 is the footnote number, 1 is the parenthetical toggle, and 4 is the XBRL detail level). Example: Note 16 Fair Value Measurements Parenthetical (Details) = 41614.
        //40102 – Level 4 Detail another ne (4 indicates details, 01 is the footnote number, 1 is the parenthetical toggle, and 4 is the XBRL detail level). Example: Note 16 Fair Value Measurements Parenthetical (Details) = 41614.

        /// </summary>
        /// <param name="excludedReports"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetPresentationSequenceForReviewersGuide(List<string> excludedReports,
            out Dictionary<string, string> excelNameToReportNameMap,
            out List<string> ret)
        {
            ret = null;
            excelNameToReportNameMap = new Dictionary<string, string>();
            ret = new List<string>();
            ArrayList nodes = GetNodesByPresentation();
            Dictionary<double, string> linkSequence = new Dictionary<double, string>();
            int nodeCount = 0;
            bool isMFtax = this.IsMutualFundTaxonomy();
            foreach (Node n in nodes)
            {
                nodeCount++;
                if (excludedReports != null && excludedReports.Contains(n.GetPresentationLink().Role)) continue;
                if (n.children == null || n.children.Count == 0) continue;


                double reportNumber = 0;
				string longReportName = n.GetPresentationLink().Title;
				string title = longReportName.Trim();

                RoleType rt;
                if (this.TryGetRoleType(n.GetPresentationLink().Role, out rt))
                {
                    title = rt.Definition;
                }
                string excelTitle = null;
                if (Regex.IsMatch(title, SECReportDefinitionRegex))
                {
                    int firstindex = title.IndexOf('-', 0);
                    if (firstindex > 0)
                    {


                        int secondindex = title.IndexOf('-', firstindex + 1);

                        if (secondindex > 0)
                        {
                            int startPos = 0;
                            if (!isMFtax &&
                                Regex.IsMatch(title, SECDisclosureReportDefinitionRegex))
                            {

                                startPos = 1;
                            }
                            //if it is a disclosure or scehdule need to ignore the first number
                            //as it represents
                            string numberStr = title.Substring(startPos, firstindex - startPos);
                            if (!double.TryParse(numberStr, out reportNumber))
                            {
                                reportNumber = 0;
                            }
                            else
                            {
                                if (startPos == 1)
                                {
                                    int level = 0;
                                    int.TryParse(title.Substring(0, 1), out level);
                                    if (level != 0)
                                    {

                                        if (Regex.IsMatch(numberStr, disNumberFormat))
                                        {
                                            //first two characters determine note number
                                            //third and fourth determines the seq within a level.
                                            int noteNumber = (int)(reportNumber / 100);
                                            string levelSeq = numberStr.Substring(2, 2);

                                            int levelseqNo = 0;
                                            int.TryParse(levelSeq, out levelseqNo);

                                            if (level == 4)
                                            {
                                                excelTitle = string.Format("Note{0} - L{1}{2}",
                                                            noteNumber,
                                                            level,
                                                            levelseqNo);


                                            }
                                            else
                                            {
                                                excelTitle = string.Format("Note{0} - L{1}",
                                                            noteNumber,
                                                            level);
                                            }



                                            double.TryParse(string.Format("{0}{1}{2}",
                                                noteNumber,
                                                level,
                                                levelSeq), out reportNumber);
                                            //just to make sure disclosures are after statements...
                                            if (!isMFtax)
                                            reportNumber *= 10000;

                                        }


                                    }
                                    else
                                    {
                                        reportNumber = nodeCount;
                                    }
                                }
                            }




                            title = title.Substring(secondindex + 1).Trim();
                        }

                    }



                }

                while (linkSequence.ContainsKey(reportNumber))
                {
                    reportNumber += 0.05;
                }

                int counter = 1;
				string key = longReportName;
				while( excelNameToReportNameMap.ContainsKey( key ) )
				{
					key = string.Format( "{0}_{1}", longReportName, counter++ );
				}

				if( string.IsNullOrEmpty( excelTitle ) )
					excelTitle = title;

				excelNameToReportNameMap[ key ] = excelTitle;
				linkSequence[ reportNumber ] = key;
            }



            List<double> dictKeys = new List<double>(linkSequence.Keys);
            dictKeys.Sort();

            foreach (double k in dictKeys)
            {
                ret.Add(linkSequence[k]);
            }

            return true;
        }

        #endregion
        #region SEC Validations

        private static char[] TRIMSET = new char[] { ' ', '\t', '\n', '\r' };
        private static Regex multispaceRegEx = new Regex(@"\s{2,}");


        /// <summary>
        /// Performs the SEC validations.
        /// </summary>
        /// <param name="validMarkups">The valid markups.</param>
        /// <param name="markedupElements">The marked up elements.</param>
        /// <param name="excludedReports">The excluded reports.</param>
        /// <param name="isMutualFundsTaxonomy">if set to <c>true</c> [is mutual funds taxonomy].</param>
        /// <param name="secValidationErrors">The sec validation errors.</param>
        public void PerformSECValidations(
            List<MarkupProperty> validMarkups,
            Dictionary<string, bool> markedupElements, 
            List<string> excludedReports,
            bool isMutualFundsTaxonomy,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            PerformSECValidations(validMarkups, markedupElements, excludedReports, isMutualFundsTaxonomy, null, ref secValidationErrors);


        }

        /// <summary>
        /// Performs the SEC validations.
        /// </summary>
        /// <param name="validMarkups">The valid markups.</param>
        /// <param name="markedupElements">The marked up elements.</param>
        /// <param name="excludedReports">The excluded reports.</param>
        /// <param name="isMutualFundsTaxonomy">if set to <c>true</c> [is mutual funds taxonomy].</param>
        /// <param name="calculationItems">The calculation items.</param>
        /// <param name="secValidationErrors">The sec validation errors.</param>
        public void PerformSECValidations(
            List<MarkupProperty> validMarkups,
            Dictionary<string, bool> markedupElements, //should refactor to remove this as we want to use valid markups
            List<string> excludedReports, bool isMutualFundsTaxonomy, 
            List<ValidationCalculationItem> calculationItems,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (validMarkups != null && validMarkups.Count > 0 )
            {
                //rebuild the markedupElements based on the valid markups...
                markedupElements = new Dictionary<string, bool>();

                foreach (MarkupProperty mp in validMarkups)
                {
                    if (mp.element == null || mp.element.MyElement == null ||
                        string.IsNullOrEmpty(mp.element.MyElement.Id)) continue;

                    markedupElements[mp.element.MyElement.Id] = true;
                }

            }

            List<RoleType> rts;
            this.TryGetUsedRoleTypes(out rts);
            foreach (RoleType rt in rts)
            {
                if (!this.IsExtendedRole(rt.Uri))
                    continue;

                IsValidateReportName(isMutualFundsTaxonomy, rt.Definition, ref  secValidationErrors);

               
            }
            ArrayList presNodes = this.GetNodesByPresentation(true);
            ArrayList calcNodes = this.GetNodesByCalculation();

            List<DimensionNode> dimNodes;
            TryGetAllDimensionNodesForDisplay(null, null, true, out dimNodes);

            if (dimNodes == null) dimNodes = new List<DimensionNode>();

            if (excludedReports != null && excludedReports.Count > 0)
            {
                ArrayList tmp = new ArrayList();
                if (presNodes != null)
                {
                    foreach (Node n in presNodes)
                    {
                        if (!excludedReports.Contains(n.GetPresentationLink().Role))
                        {
                            tmp.Add(n);
                        }
                    }

                    presNodes = tmp;
                }

                tmp = new ArrayList();
                if (calcNodes != null)
                {
                    foreach (Node n in calcNodes)
                    {
                        if (!excludedReports.Contains(n.GetPresentationLink().Role))
                        {
                            tmp.Add(n);
                        }
                    }

                    calcNodes = tmp;
                }

                if (dimNodes != null)
                {
                    List<DimensionNode> tmpDN = new List<DimensionNode>();

                    foreach (DimensionNode dn in dimNodes)
                    {
                        if (!excludedReports.Contains(dn.MyDefinitionLink.Role))
                        {
                            tmpDN.Add(dn);
                        }
                    }
                    dimNodes = tmpDN;
                }
            }

            //perform taxonomy level validations on the markedup elements.
            #region Presentation validations
            //Missing in Presentation
            List<Node> nodesNotInPresentation = GetInUseNodesMissingInPresentation(markedupElements,
                true, presNodes, excludedReports);
            foreach (Node n in nodesNotInPresentation)
            {
                ValidationErrorInfo info = new ValidationErrorInfo(
                    string.Format("In use element '{0}' does not have a presentation relationship defined.", n.Label),
                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.MISSING_IN_PRESENTATION_ERROR);
                secValidationErrors.Add(info);
            }

            //allow the same inuse element in multiple places only if it has different label roles..
            Dictionary<string, List<Node>> inUsePresNodes = new Dictionary<string, List<Node>>();
            Dictionary<string, List<string>> inUsePresElementIdsByReport = new Dictionary<string, List<string>>();
            foreach (Node n in presNodes)
            {
                List<string> inUseElementsInReport = new List<string>();
                RecursivelyGetInUseNodes(markedupElements, n, ref inUsePresNodes, ref inUseElementsInReport);

                inUsePresElementIdsByReport[n.GetPresentationLink().Role] = inUseElementsInReport;
            }

            //use this to avoid duplicate error display....
            List<string> checkedErrors = new List<string>();

            foreach (KeyValuePair<string, List<Node>> kvp in inUsePresNodes)
            {
                if (kvp.Value.Count <= 1)
                {

                    continue;

                }

                Dictionary<string, Node> infos = new Dictionary<string, Node>();
                foreach (Node n in kvp.Value)
                {
                    string nInfo = string.Format("{0}-{1}-{2}", n.GetPresentationLink().Role, n.parent.Id, n.PreferredLabel == null ? string.Empty : n.PreferredLabel);
                    if (checkedErrors.Contains(nInfo))
                    {
                        continue;
                    }
                    Node other;
                    if (infos.TryGetValue(nInfo, out other))
                    {
                        if (other.Order != n.Order)
                        {

                            ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Found duplicate occurrence of the element '{0}' in the report '{1}'.", n.Label, n.GetPresentationLink().Title),
                                        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.DUPLICATION_IN_PRESENTATION_ERROR);
                            secValidationErrors.Add(info);

                            checkedErrors.Add(nInfo);
                        }
                    }

                    infos[nInfo] = n;
                }

            }

            Dictionary<string, List<Node>> parentChildArcToRoleInfo = new Dictionary<string, List<Node>>();
            //Withina report missing in Presentation but exists in calculation.
            if (calculationInfo != null)
            {
                foreach (Node calcRootNode in calcNodes)
                {

                    PresentationLink pl = calcRootNode.GetPresentationLink();


                    bool missingPresentation = false;
                    List<Node> missingNodes = GetInUseCalculationNodesMissingInCurrentReport(markedupElements,
                        pl.Role, true, ref missingPresentation, calcRootNode);
                    foreach (Node n in missingNodes)
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("Report '{0}' has the element '{1}' in the calculation view but is missing in the presentation view.", pl.Title, n.Label),
                ValidationErrorInfo.ValidationCategoryType.SECValidationWarning, "Presentation Warning", ValidationErrorInfo.SequenceEnum.ELEMENT_IN_CALCULATION_NOT_IN_PRESENTATION_WARNING);
                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                        secValidationErrors.Add(info);
                    }

                    if (missingPresentation)
                    {

                        ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Report '{0}' does not have a presentation view.", pl.Role),
            ValidationErrorInfo.ValidationCategoryType.SECValidationWarning, "Presentation Warning", ValidationErrorInfo.SequenceEnum.PRESENTATION_REPORT_MISSING_WARNING);
                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                        secValidationErrors.Add(info);
                    }

                    if (calcRootNode.children != null)
                    {
                        foreach (Node parent in calcRootNode.children)
                        {
                            RecursivelyCheckUniquenessOfParentChildArc(parent, markedupElements,
                                ref parentChildArcToRoleInfo,
                                ref secValidationErrors);

                            CheckRecursionInCalculation(calcRootNode.GetPresentationLink(), parent, ref secValidationErrors);

                            List<string> allChildren = new List<string>();
                            allChildren.Add(parent.MyElement.GetNameWithNamespacePrefix());
                            RecursivelyBuildChildrenForParent(parent, ref allChildren, ref secValidationErrors);


                            RecursivelyCheckPeriodTypeOnParentChild(parent, ref secValidationErrors);
                        }
                    }
                }
            }

            PerformTextBlockElementValidation(presNodes, markedupElements, ref secValidationErrors);
            
            #endregion
            Dictionary<string, Node> reportLineItems = new Dictionary<string, Node>();
            Dictionary<string, List<string>> reportDimensionInfos = new Dictionary<string, List<string>>();
            ValidateStructure(excludedReports, presNodes, isMutualFundsTaxonomy, ref reportLineItems, ref reportDimensionInfos,
                ref secValidationErrors);

            //CheckIfAxisIsRequired(excludedReports, presNodes, validMarkups,
            //    ref secValidationErrors);


            Dictionary<string, List<Node>> CalcParents = new Dictionary<string, List<Node>>();
            ValidatePresentationandCalculationForUsGaapArchitecture(excludedReports,
                markedupElements, ref CalcParents,
                ref secValidationErrors);


            CheckPeriodTypeMismatchInCalculation(calcNodes, CalcParents, ref secValidationErrors);

            // For extended elements with a Monetary data type and Balance Type of N/A, show warning
            foreach (string eleId in markedupElements.Keys)
            {
                Element ele = this.allElements[eleId] as Element;
                if (ele != null && ele.IsAucentExtendedElement && ele.IsMonetary() && ele.BalType == Element.BalanceType.na)
                {
                    ValidationErrorInfo info = new ValidationErrorInfo(
                        string.Format("The extended element {0} has a data type of Monetary and a Balance Type of N/A.  Please confirm the appropriateness with guidance at EFM 6.8.11 and EFM 6.11.5 ", ele.Name),
                        ValidationErrorInfo.ValidationCategoryType.ExtendedElememntMonetaryNA,
                        "Element Error",
                        ValidationErrorInfo.SequenceEnum.EXTENDED_ELEMENT_MONETARY_NA);
                    
                    info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                    
                    secValidationErrors.Add(info);
                }
            }



            GetCalculationRelationshipsMissingInPresentation(markedupElements,
                CalcParents,
                inUsePresElementIdsByReport,
              ref secValidationErrors);





            if (validMarkups != null)
            {
                PerformSECDimensionValidation(validMarkups, reportLineItems, reportDimensionInfos,
                    ref secValidationErrors);

            }

            Dictionary<string, Element> elementsDt = new Dictionary<string, Element>();

            foreach (Node n in presNodes)
            {
                if (n.children != null)
                {
                    foreach (Node childNode in n.children)
                    {
                        RecursivelyGetAllAvailableElements(ref elementsDt, childNode);
                    }
                }
            }

            foreach (Node n in calcNodes)
            {
                if (n.children != null && calculationItems != null )
                {
                    foreach (ValidationCalculationItem calcItem in calculationItems)
                    {
                        // only do the check if the item is in the marked up elements list.
                        if (markedupElements.ContainsKey(calcItem.ElementId))
                        {
                            Node result = n.GetChild(calcItem.ElementId);
                            if (result != null)
                                CheckCalcRules(result, calcItem, ref secValidationErrors);
                        }
                    }

                    foreach (Node childNode in n.children)
                    {
                        RecursivelyGetAllAvailableElements(ref elementsDt, childNode);
                    }
                }
            }

            foreach (Node n in dimNodes)
            {
                if (n.children != null)
                {
                    foreach (Node childNode in n.children)
                    {
                        RecursivelyGetAllAvailableElements(ref elementsDt, childNode);
                    }
                }
            }




            #region Extended elements validations
            //Extended element Id naming convension checks
            PerformExtendedElementValidation(ref secValidationErrors,
                markedupElements, presNodes,
                dimNodes, elementsDt, excludedReports, isMutualFundsTaxonomy);




            #endregion

            #region Extended Roles Validation
            PerfromExtendedRolesValidation(ref secValidationErrors);

            #endregion





            PerformElementLabelValidations(ref secValidationErrors,
                elementsDt,
                markedupElements, 
                presNodes,
                isMutualFundsTaxonomy);


            secValidationErrors.Sort();

        }


        public static bool IsValidateReportName (bool isMutualFundTaxonomy, 
            string reportTitle, ref List<ValidationErrorInfo> secValidationErrors)
        {

            
            if (!System.Text.RegularExpressions.Regex.IsMatch(reportTitle, SECReportDefinitionRegex))
            {
                string str = TraceUtility.FormatStringResource("XBRLAddin.ETWizard.Error.SECTitleFormatMessage");
                string msg = string.Format(str, reportTitle, "{number} - {type} - {text}");
                ValidationErrorInfo info = new ValidationErrorInfo(msg,
                    ValidationErrorInfo.ValidationCategoryType.SECValidationError,
                    "Report Name Error", ValidationErrorInfo.SequenceEnum.REPORT_NAME_ERROR, null);
                secValidationErrors.Add(info);


                return false;


            }
            //for sec we do not want to allow square brackets or curly brackets...
            if (!isMutualFundTaxonomy)
            {

                if (System.Text.RegularExpressions.Regex.IsMatch(reportTitle, SECReportDefinitionRegexSpecialNotAllowed))
                {
                    string str = TraceUtility.FormatStringResource("XBRLAddin.ETWizard.Error.SECTitleFormatMessage");
                    string msg = string.Format(str, reportTitle, "{number} - {type} - {text}");
                    msg += " Square brackets and curly braces are not allowed in the title as they  would cause rendering issues.";
                    ValidationErrorInfo info = new ValidationErrorInfo(msg,
                        ValidationErrorInfo.ValidationCategoryType.SECValidationError,
                        "Report Name Error", ValidationErrorInfo.SequenceEnum.REPORT_NAME_ERROR, null);
                    secValidationErrors.Add(info);


                    return false;


                }

            }


            return true;
        }

        private void CheckCalcRules(Node node, ValidationCalculationItem calcItem, ref List<ValidationErrorInfo> secValidationErrors)
        {
            decimal nodeCalcWeight = 0;
            decimal itemCalcWeight = 0;

            // convert the weights into decimals.  some of the values are marked 1 and 1.0
            if (decimal.TryParse(node.CalculationWeight, out nodeCalcWeight))
            {
                if (decimal.TryParse(calcItem.Value, out itemCalcWeight))
                {
                    if (itemCalcWeight != nodeCalcWeight && calcItem.Category == "NetCashProvidedbyUsedInOperatingActivitiesShouldHavePositiveOneWeight")
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("The element {0} should almost always have a weight of 1.  Please confirm calculation weight", node.Id),
                            ValidationErrorInfo.ValidationCategoryType.CalculationWeightWarning,
                            "Calculation Weight Warning",
                            ValidationErrorInfo.SequenceEnum.CALCULATION_WEIGHT_WARNING_NetCashProvidedByUsedInOperatingActivities);

                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;

                        secValidationErrors.Add(info);
                    }

                    if (itemCalcWeight != nodeCalcWeight && calcItem.Category == "ElementsShouldHaveWeightPositiveOne")
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("The element {0} almost always should receive a calculation weight of 1 per Rivet Rules.  Please confirm calculation weight.", node.Id),
                            ValidationErrorInfo.ValidationCategoryType.CalculationWeightWarning,
                            "Calculation Weight Warning",
                            ValidationErrorInfo.SequenceEnum.CALCULATION_WEIGHT_WARNING_ALMOST_NEVER_RECIEVE_CALCULATIONWEIGHT_NEGATIVE);

                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;

                        secValidationErrors.Add(info);
                    }


                    if (itemCalcWeight != nodeCalcWeight && calcItem.Category == "ElementsShouldHaveWeightNegativeOne")
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("The element {0} almost always should receive a calculation weight of -1 per Rivet Rules.  Please confirm calculation weight.", node.Id),
                            ValidationErrorInfo.ValidationCategoryType.CalculationWeightWarning,
                            "Calculation Weight Warning",
                            ValidationErrorInfo.SequenceEnum.CALCULATION_WEIGHT_WARNING_ALMOST_NEVER_RECIEVE_CALCULATIONWEIGHT_POSITIVE);

                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;

                        secValidationErrors.Add(info);
                    }
                }
            }
        }

        public void PerformSECDimensionValidation(List<MarkupProperty> validMarkups,
            Dictionary<string, Node> reportLineItems,
            Dictionary<string, List<string>> reportDimensionInfos,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            foreach (MarkupProperty mp in validMarkups)
            {
                if (mp.contextRef == null) continue;

                if (mp.contextRef.Segments != null && mp.contextRef.Segments.Count > 0)
                {
                    List<string> eleDimMembers = new List<string>();
                    StringBuilder dimVals = new StringBuilder();
                    foreach (Segment seg in mp.contextRef.Segments)
                    {
                        if (seg.DimensionInfo == null) continue;
                        string key = seg.DimensionInfo.ToString();
                        dimVals.Append(key + " ");
                        eleDimMembers.Add(key);
                    }

                    if (eleDimMembers.Count == 0) continue;

                    bool isValid = false;
                    foreach (KeyValuePair<string, Node> kvp in reportLineItems)
                    {
                        if (isValid) break;
                        List<string> dimInfos;

                        if (!reportDimensionInfos.TryGetValue(kvp.Key, out dimInfos)) continue;

                        if (kvp.Value.GetChild(mp.element.Id) != null)
                        {
                            //element exists in this report...
                            bool isGood = true;
                            foreach (string key in eleDimMembers)
                            {
                                if (!dimInfos.Contains(key))
                                {
                                    isGood = false;
                                    break;
                                }

                            }

                            if (isGood)
                            {
                                //found a report that is valid....
                                isValid = true;
                                break;
                            }
                        }
                    }

                    if (!isValid)
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
               string.Format("Element {0} is using the following dimension(s){1}. At least one valid report should have the element as a decendant of the Line item and all the dimension members as the descendant of the Axis corresponding to the Line item.",
               mp.element.Name, dimVals.ToString()),
               ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Dimension Error", ValidationErrorInfo.SequenceEnum.DIMENSION_VALIDATION);
                        secValidationErrors.Add(info);
                    }

                }

            }
        }

        private void RecursivelyCheckPeriodTypeOnParentChild(Node parent, ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (parent.IsProhibited) return;

            if (parent.children != null)
            {
                List<string> childIds = new List<string>();
                foreach (Node child in parent.children)
                {
                    if (child.IsProhibited) continue;
                    if (childIds.Contains(child.Id))
                    {
                        //we cannot have the same child twice for a single parent
                        ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("In the Calculation view of the Report '{0}' Element '{1}' appears more than once as the child of the element '{2}'. Please remove all duplicate occurances of the same child element.", parent.GetPresentationLink().Title, child.Label, parent.Label),
                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Error", ValidationErrorInfo.SequenceEnum.DUPLICATE_IN_CALCULATION);
                        secValidationErrors.Add(info);
                    }
                    else
                    {
                        childIds.Add(child.Id);
                    }
                    if (child.MyElement.PerType != parent.MyElement.PerType)
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("In the Calculation view of the Report '{0}' period type of parent element '{1}' does not match the period type of child element '{2}'. Please correct this as  the source and target of all calculation relationships must have the same period type.",
                parent.GetPresentationLink().Title, parent.Label, child.Label),
                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Error", ValidationErrorInfo.SequenceEnum.CALCULATION_PARENT_CHILD_PERIOD_TYPE_MISMATCH);
                        secValidationErrors.Add(info);

                    }
                    else
                    {
                        RecursivelyCheckPeriodTypeOnParentChild(child, ref secValidationErrors);
                    }


                }
            }


        }


        private void RecursivelyBuildChildrenForParent(Node parent, ref List<string> cns
            , ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (parent.children == null) return;
            if (parent.IsProhibited) return;

            foreach (Node c in parent.children)
            {
                if (c.IsProhibited) continue;

                if (cns.Contains(c.MyElement.GetNameWithNamespacePrefix()))
                {

                    ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("In Report '{0}' element '{1}' with Label '{2} is used multiple times in calculating a parent value. This might result in incorrect calculated values for the parent element.",
                parent.GetPresentationLink().Title, c.MyElement.GetNameWithNamespacePrefix(), c.Label),
                ValidationErrorInfo.ValidationCategoryType.SECValidationWarning, "Calculation Warning", ValidationErrorInfo.SequenceEnum.CALCULATION_WARNING_POSSIBLE_APPALACHIAN_TREE);
                    info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                    secValidationErrors.Add(info);
                }
                else
                {
                    cns.Add(c.MyElement.GetNameWithNamespacePrefix());


                    RecursivelyBuildChildrenForParent(c, ref cns, ref secValidationErrors);
                }
            }
        }

        #region Taxonomy structure validations

        private void ValidateStructure(List<string> excludedReports,
            ArrayList pNodes,
            bool isMutualFundsTaxonomy,
            ref Dictionary<string, Node> reportLineItems,
            ref Dictionary<string, List<string>> reportDimensionInfos,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            List<string> mfSkippedReports = new List<string>();
            if (isMutualFundsTaxonomy)
            {
                //the following base reports can be skipped for mutual fund taxonomies...

                mfSkippedReports.Add("http://xbrl.sec.gov/rr/role/Prospectus");
                mfSkippedReports.Add("http://xbrl.sec.gov/rr/role/Class");
                mfSkippedReports.Add("http://xbrl.sec.gov/rr/role/PerformanceMeasure");
                mfSkippedReports.Add("http://xbrl.sec.gov/rr/role/Series");
            }
            Dictionary<string, DimensionNode> axisDefaultInfo = new Dictionary<string, DimensionNode>();
            foreach (Node root in pNodes)
            {
                if (excludedReports != null && excludedReports.Contains(root.GetPresentationLink().Role))
                {
                    //need to skip this report...
                    continue;
                }


                if (isMutualFundsTaxonomy)
                {
                    //the following base reports can be skipped...



                    if (mfSkippedReports.Contains(root.GetPresentationLink().Role))
                    {
                        //need to skip this report...
                        continue;
                    }
                }
                //1. Every report need to have the structure 
                //Presentation
                //abstractParent 
                //Table
                //Axis                               
                //Line Items

                //2. cannot have the same dimension member  more than once in a report...


                Node absRoot = null;
                int countValid = 0;
                if (root.children != null)
                {
                    foreach (Node abs in root.children)
                    {
                        if (abs.IsProhibited) continue;
                        countValid++;
                        if (abs.IsAbstract && !abs.MyElement.IsDimensionOrHyperCubeItem())
                        {
                            absRoot = abs;
                        }
                    }
                }

                if (absRoot != null && countValid == 1)
                {
                    Node tableNode = null;
                    countValid = 0;
                    if (absRoot.children != null)
                    {
                        foreach (Node tn in absRoot.children)
                        {
                            if (tn.IsProhibited) continue;
                            countValid++;
                            if (tn.MyElement.IsHyperCubeItem())
                            {
                                tableNode = tn;
                            }
                        }
                    }
                    if (tableNode != null && countValid == 1 && tableNode.children != null)
                    {
                        ArrayList axises = new ArrayList();
                        Node lineItem = null;
                        countValid = 0;
                        foreach (Node tn in tableNode.children)
                        {
                            if (tn.IsProhibited) continue;
                            if (tn.MyElement.IsDimensionItem())
                            {
                                axises.Add(tn);
                            }
                            else
                            {
                                if (tn.IsHypercubeNodeInHierarchy())
                                {
                                    //we have another hypercube..nested in the presentation
                                    ValidationErrorInfo info = new ValidationErrorInfo(
              string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. Found mulitple tables in the report.",
              root.GetPresentationLink().Title, tableNode.Name),
              ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_TABLE);
                                    secValidationErrors.Add(info);

                                }
                                countValid++;
                                if (tn.MyElement.Name.EndsWith("LineItems"))
                                {
                                    lineItem = tn;

                                }
                                else if (isMutualFundsTaxonomy && tn.MyElement.IsAbstract)
                                {
                                    lineItem = tn;
                                }
                            }

                        }

                        if (countValid == 1 && lineItem != null && axises.Count > 0)
                        {
                           
                            List<string> allDimMembers = new List<string>();
                            foreach (DimensionNode dnAxis in axises)
                            {
                                if (dnAxis.IsProhibited || dnAxis.children == null) continue;
                                //check to make sure we do not have the same member more than once in the dimension array
                                //as it would result in non directed cycles......
                                List<string> allMembers = new List<string>();
                                RecursivlyCheckForDuplicateMembers(dnAxis.Id,
                                    dnAxis.children,
                                    ref allDimMembers,
                                    ref allMembers, ref secValidationErrors);


                                //raise error if it has non default children 
                                foreach (DimensionNode cDN in dnAxis.children)
                                {
                                    if (cDN.IsProhibited) continue;

                                    if (!cDN.NodeDimensionInfo.IsDefault)
                                    {
                                        ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The only child that an axis element should have is the default domain. Please move the non default children like {2} of axis {1} to be under the domain element.",
            root.GetPresentationLink().Title, dnAxis.Name, cDN.Name),
            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_AXIS);
                                        secValidationErrors.Add(info);
                                    }
                                    else
                                    {

                                        if (!cDN.MyElement.Name.EndsWith("domain", StringComparison.OrdinalIgnoreCase))
                                        {
                                            ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The default {1} for the axis {2} does not end with the word Domain. Please use an Element that ends with Domain as the default member.",
            root.GetPresentationLink().Title, dnAxis.Id, cDN.Id),
            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_AXIS_DOMAIN_NOT_DEFAULT);
                                            secValidationErrors.Add(info);



                                        }


                                        DimensionNode defaultMem = null;
                                        if (!axisDefaultInfo.TryGetValue(dnAxis.Id, out defaultMem))
                                        {
                                            axisDefaultInfo[dnAxis.Id] = cDN;
                                        }
                                        else
                                        {
                                            if (!defaultMem.Id.Equals(cDN.Id))
                                            {
                                                ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Axis {0} does not comply with Rivet recommended structure. Report {1} has Member {2} as the default and Report {3} has Member {4} as the default. Please ensure that the same member is used as the default for a given axis accross all reports.  ",
            dnAxis.Label, root.GetPresentationLink().Title, cDN.Id, defaultMem.MyDefinitionLink.Title, defaultMem.Id),
            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_AXIS_MULTIPLE_DEFAULTS);
                                                secValidationErrors.Add(info);
                                            }
                                        }
                                    }
                                }

                            }



                            reportLineItems[root.GetPresentationLink().Role] = lineItem;
                            reportDimensionInfos[root.GetPresentationLink().Role] = allDimMembers;

                            //need to make sure that the line item in pres is the same as the line item in definition file.
                            DefinitionLink dl = this.netDefinisionInfo.DefinitionLinks[root.GetPresentationLink().Role] as DefinitionLink;
                            if (dl.MeasureLocatorsHrefs == null || !dl.MeasureLocatorsHrefs.Contains(lineItem.Id))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
           string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The hypercube line item does not match the presentation line item.",
           root.GetPresentationLink().Title),
           ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_LINEITEM_MISMATCH);
                                secValidationErrors.Add(info);
                            }

                            if (dl.MeasureLocatorsHrefs.Count > 1)
                            {
                                List<string> uniqueMeasureRoots = new List<string>();
                                foreach (string mr in dl.MeasureLocatorsHrefs)
                                {
                                    if (!uniqueMeasureRoots.Contains(mr))
                                    {
                                        uniqueMeasureRoots.Add(mr);
                                    }
                                }

                                if (uniqueMeasureRoots.Count > 1)
                                {
                                    ValidationErrorInfo info = new ValidationErrorInfo(
           string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. Multiple hypercube line items found in the dimension linkbase.",
           root.GetPresentationLink().Title),
           ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_MULTIPLE_LINEITEM_MISMATCH);
                                    secValidationErrors.Add(info);
                                }
                            }
                        }
                        else
                        {
                            if (axises.Count == 0)
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
               string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The Table element {1} should have atleast one Dimension child element.",
               root.GetPresentationLink().Title, tableNode.Name),
               ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_DIMENSION);
                                secValidationErrors.Add(info);
                            }

                            if (lineItem == null || countValid != 1)
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
               string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The Table should have only dimension elements and a single line item element as its children.",
               root.GetPresentationLink().Title, tableNode.Name),
               ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_LINE_ITEM);
                                secValidationErrors.Add(info);
                            }

                        }

                    }
                    else
                    {
                        //error
                        ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The root node should have a single Table element as its child element.", root.GetPresentationLink().Title),
                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_ROOT_NODE);
                        secValidationErrors.Add(info);
                    }


                }
                else
                {
                    ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("Report {0} does not comply with the Rivet recommended Presentation structure. The report should have a single abstract (non table) element as the root node.", root.GetPresentationLink().Title),
                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_ROOT_NODE_ABSTRACT);
                    secValidationErrors.Add(info);
                    //validation error....
                }

            }



        }


        private void ValidateDimensionDefault(List<string> excludedReports,
            Dictionary<string, List<string>> reportDimensionInfos,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            //make sure that all in use axises have just a single default member and the 
            //member is the same one in all reports...

        }

        private void RecursivlyCheckForDuplicateMembers(string AxisId,
            ArrayList nodes,
            ref List<string> allDimMembers,
            ref List<string> allMembers,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            foreach (DimensionNode dn in nodes)
            {
                if (dn.IsProhibited) continue;
                if (allMembers.Contains(dn.Id))
                {
                    if (dn.Id.EndsWith("Domain"))
                    {


                        ValidationErrorInfo info = new ValidationErrorInfo(
                    string.Format("Dimension Domain {0} exists in more than one location in Report {1}. This will result in undirected dimension cycles error.",
                    dn.MyElement.Name,
                    dn.MyDefinitionLink.Title),
                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.DOMAIN_DUPLICATION_ERROR);

                        secValidationErrors.Add(info);

                    }
                    else
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                   string.Format("Dimension Member {0} exists in more than one location in Report {1}. This will result in undirected dimension cycles error.",
                   dn.MyElement.Name,
                   dn.MyDefinitionLink.Title),
                   ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.MEMBER_DUPLICATION_ERROR);

                        secValidationErrors.Add(info);
                    }
                }
                else
                {
                    allMembers.Add(dn.Id);
                }
                if (!dn.NodeDimensionInfo.IsDefault)
                {
                    allDimMembers.Add(string.Format("{0}:{1}", AxisId, dn.Id));

                }
                if (dn.children != null)
                {
                    RecursivlyCheckForDuplicateMembers(AxisId,
                        dn.children, ref allDimMembers,
                        ref allMembers, ref secValidationErrors);
                }
            }
        }
        #endregion



        //#region Check if axis is required
        //private void CheckIfAxisIsRequired(List<string> excludedReports,
        //    ArrayList presNodesWithDimensions,
        //    List<MarkupProperty> validMarkups,
        //    ref List<ValidationErrorInfo> secValidationErrors
        //    )
        //{

            



        //    Dictionary<string, List<string>> ElementsUsedByAxis = new Dictionary<string, List<string>>();

        //    foreach (MarkupProperty mp in validMarkups)
        //    {

        //        if (mp.contextRef != null && mp.contextRef.Segments != null &&
        //            mp.contextRef.Segments.Count > 0)
        //        {
        //            foreach (Segment seg in mp.contextRef.Segments)
        //            {
        //                if (seg.DimensionInfo != null)
        //                {
        //                    List<string> vals;
        //                    if (!ElementsUsedByAxis.TryGetValue(seg.DimensionInfo.dimensionId, out vals))
        //                    {

        //                        vals = new List<string>();
        //                        ElementsUsedByAxis[seg.DimensionInfo.dimensionId] = vals;

        //                    }
        //                    if (!vals.Contains(mp.element.MyElement.Id))
        //                    vals.Add(mp.element.MyElement.Id);
        //                }

        //            }

        //        }

        //    }
        //    //List<string> skipReports = new List<string>();
        //    Dictionary<string, List<string>> axisByDomain = new Dictionary<string, List<string>>();
        //    foreach (KeyValuePair<string, List<string>> kvp in ElementsUsedByAxis)
        //    {
        //        string domain;
        //        if (this.netDefinisionInfo.TryGetDefaultMember(kvp.Key, out domain))
        //        {
        //            List<string> axises;
        //            if (!axisByDomain.TryGetValue(domain, out axises))
        //            {
        //                axises = new List<string>();
        //                axisByDomain[domain] = axises;
        //            }
        //            axises.Add(kvp.Key);
        //        }

        //        foreach (Node topNode in presNodesWithDimensions)
        //        {
        //            if (excludedReports != null &&
        //                excludedReports.Contains(topNode.GetPresentationLink().Role))
        //                continue;

        //            //if (skipReports.Contains(topNode.GetPresentationLink().Role))
        //            //    continue;

        //            if (topNode.GetChild(kvp.Key) != null)
        //            {
        //                //at least one of the elements that use the axis must be in this report
        //                bool haserror = true;

        //                foreach (string val in kvp.Value)
        //                {
        //                    if (topNode.GetChild(val) != null)
        //                    {
        //                        haserror = false;
        //                        break;
        //                    }

        //                }

        //                if (haserror)
        //                {

        //                    ValidationErrorInfo info = new ValidationErrorInfo(
        //       string.Format("Dimension Axis {0} is  not required in report  {1}. This will result in architecural error.",
        //       kvp.Key,
        //       topNode.GetPresentationLink().Title),
        //       ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_UNNECESSARYAXIS);

        //                    secValidationErrors.Add(info);

        //                    //skipReports.Add(topNode.GetPresentationLink().Role);

        //                }
        //            }

        //        }

        //    }

        //    foreach (KeyValuePair<string, List<string>> kvp in axisByDomain)
        //    {
        //        if (kvp.Value.Count > 1)
        //        {

        //            ValidationErrorInfo info = new ValidationErrorInfo(
        //      string.Format("Dimension Domain {0} is  is the default for {1} axises. This will result in architecural error.",
        //      kvp.Key,
        //      kvp.Value.Count),
        //      ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_MULTIPLEAXISWITHSAMEDEFAULT);

        //            secValidationErrors.Add(info);
        //        }

        //    }
        //}

        //#endregion

        #region Check Total Label and Totals In Calculation
        private void ValidatePresentationandCalculationForUsGaapArchitecture(
            List<string> excludedReports,
            Dictionary<string, bool> markedupElements,
            ref Dictionary<string, List<Node>> CalcParents,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            ArrayList pNodes = this.GetNodesByPresentation();
            ArrayList cNodes = this.GetNodesByCalculation();
            if (pNodes == null || cNodes == null) return;
            ArrayList tmp = new ArrayList();

            foreach (Node n in pNodes)
            {
                if (excludedReports != null && excludedReports.Contains(n.GetPresentationLink().Role))
                {
                    //need to skip this report...
                    continue;
                }
                tmp.Add(n);
            }
            pNodes = tmp;

            tmp = new ArrayList();
            foreach (Node n in cNodes)
            {
                if (excludedReports != null && excludedReports.Contains(n.GetPresentationLink().Role))
                {
                    //need to skip this report...
                    continue;
                }
                tmp.Add(n);
            }
            cNodes = tmp;




            RecursivelyProhibitUnusedNodes(markedupElements, pNodes);
            RecursivelyProhibitUnusedNodes(markedupElements, cNodes);

            Dictionary<string, Node> totalLabelElements = new Dictionary<string, Node>();
            RecursivelyGetListOfCalcParents(cNodes, ref CalcParents);
            RecursivelyGetTotalLabelElements(pNodes, ref totalLabelElements);



            foreach (string ele in CalcParents.Keys)
            {
                if (!markedupElements.ContainsKey(ele))
                {

                    //we have a calc parent that is not used..
                    //we want to issue a warning for this case..
                    //so that PS can remove this element if required.
                    ValidationErrorInfo info = new ValidationErrorInfo(
           string.Format("Element {0} is a unused total element in the calculation report {1}. Please verify if this element is required as a parent of other markedup elements.",
           ele, CalcParents[ele][0].GetPresentationLink().Title),
       ValidationErrorInfo.ValidationCategoryType.SECValidationWarning, "Calculation Warning", ValidationErrorInfo.SequenceEnum.CALCULATION_WARNING_UNUSED_TOTAL_ELEMENT);
                    info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                    secValidationErrors.Add(info);
                    continue;

                }

                if (!totalLabelElements.ContainsKey(ele))
                {


                    ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Element {0} is a total element in the calculation report {1}. At least one presentation location of this element should use a total label role.",
            ele, CalcParents[ele][0].GetPresentationLink().Title),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Error", ValidationErrorInfo.SequenceEnum.CALCULATION_TOTAL_NOT_PRESENTATION_TOTAL);
                    secValidationErrors.Add(info);
                }
            }



            foreach (string ele in totalLabelElements.Keys)
            {
                if (!CalcParents.ContainsKey(ele))
                {
                    ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Element {0} is defined with a total label role in the presentation report {1}. It is not a valid total element in any calculation report.",
            ele, totalLabelElements[ele].GetPresentationLink().Title),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.PRESENTATION_TOTAL_NOT_CALCULATION_TOTAL);
                    secValidationErrors.Add(info);
                }
            }


            RecursivelyCheckLineItemsInPresentation(pNodes, ref secValidationErrors);
        }


        private void RecursivelyCheckLineItemsInPresentation(ArrayList nNodes,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (nNodes == null) return;

            foreach (Node n in nNodes)
            {
                if (n.IsProhibited) continue;
                if (n.Parent != null)
                {
                    if (n.IsAbstract && n.Id.EndsWith("LineItems"))
                    {
                        bool isError = true;
                        if (n.Parent.MyElement != null && n.Parent.MyElement.IsHyperCubeItem())
                        {
                            isError = false;
                        }
                        if (isError)
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("Line Item {0} in report {1} does not confirm with the US GAAP architecture requirement of line items should only be a child of a table element.",
            n.MyElement.Name, n.GetPresentationLink().Title),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.USGAAP_STRUCTURE_ERROR_LINE_ITEM2);
                            secValidationErrors.Add(info);
                        }
                    }
                }

                RecursivelyCheckLineItemsInPresentation(n.children, ref secValidationErrors);
            }
        }

        private void RecursivelyProhibitUnusedNodes(
            Dictionary<string, bool> markedupElements, ArrayList nodes)
        {

            foreach (Node n in nodes)
            {
                if (n.IsProhibited) continue;
                n.IsProhibited = !IsNodeOrChildrenInUse(n, markedupElements);

                if (!n.IsProhibited &&
                    n.children != null && n.children.Count > 0)
                {


                    RecursivelyProhibitUnusedNodes(markedupElements, n.children);

                }

            }

        }

        private bool IsNodeOrChildrenInUse(
            Node n, Dictionary<string, bool> markedupElements)
        {
            bool inUse = markedupElements.ContainsKey(n.Id);

            if (!inUse)
            {
                if (n.children != null && n.children.Count > 0)
                {
                    foreach (Node cn in n.children)
                    {
                        if (cn.IsProhibited) continue;

                        inUse = IsNodeOrChildrenInUse(cn, markedupElements);

                        if (inUse) break;
                    }

                }
                else
                {
                    n.IsProhibited = true;
                }
            }


            return inUse;
        }


        private void RecursivelyGetListOfCalcParents(
            ArrayList cNodes,
           ref Dictionary<string, List<Node>> CalcParents)
        {

            if (cNodes == null) return;

            foreach (Node cn in cNodes)
            {
                if (cn.IsProhibited) continue;

                if (cn.children != null && cn.children.Count > 0)
                {
                    bool hasValidChild = false;
                    foreach (Node c in cn.children)
                    {
                        if (c.IsProhibited) continue;

                        hasValidChild = true;
                        break;
                    }
                    if (hasValidChild)
                    {
                        if (!string.IsNullOrEmpty(cn.Id))
                        {
                            List<Node> vals;
                            if (!CalcParents.TryGetValue(cn.Id, out vals))
                            {
                                vals = new List<Node>();
                                CalcParents[cn.Id] = vals;
                            }
                            vals.Add(cn);

                        }
                    }
                    RecursivelyGetListOfCalcParents(cn.children, ref CalcParents);

                }

            }

        }



        private void RecursivelyGetTotalLabelElements(ArrayList pNodes,
            ref Dictionary<string, Node> totalLabelElements)
        {

            if (pNodes == null) return;

            foreach (Node cn in pNodes)
            {
                if (cn.IsProhibited) continue;
                if (!string.IsNullOrEmpty(cn.Id) &&
                    cn.PreferredLabel != null
                    && cn.PreferredLabel.ToLower().Contains("total"))
                {
                    totalLabelElements[cn.Id] = cn;

                }
                if (cn.children != null && cn.children.Count > 0)
                {

                    RecursivelyGetTotalLabelElements(cn.children, ref totalLabelElements);

                }

            }

        }


        #endregion

        private void CheckRecursionInCalculation(PresentationLink link,
            Node current, ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (current.IsProhibited) return;

            if (current.parent != null)
            {
                if (current.parent.GetParent(current.Id) != null)
                {
                    ValidationErrorInfo info = new ValidationErrorInfo(
                string.Format("Report '{0}' has a recursive relationship for the element '{1}' in the calculation view.", link.Title, current.Label),
                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Error", ValidationErrorInfo.SequenceEnum.RECURSION_IN_CALCULATION);
                    secValidationErrors.Add(info);
                }
            }


            if (current.children != null)
            {
                foreach (Node c in current.children)
                {
                    CheckRecursionInCalculation(link, c, ref secValidationErrors);
                }
            }

        }

        private void PerformExtendedElementValidation(ref List<ValidationErrorInfo> secValidationErrors,
            Dictionary<string, bool> markedupElements, ArrayList presNodes, List<DimensionNode> dimNodes,
            Dictionary<string, Element> validelementsDt,
            List<string> excludedReports, bool isMutualFundTaxonomy)
        {
            if (this.IsAucentExtension)
            {
                Dictionary<string, bool> baseElementNames = new Dictionary<string, bool>();
                Dictionary<string, bool> extendedElements = new Dictionary<string, bool>();
                foreach (Element ele in this.allElements.Values)
                {
                    if (ele.TaxonomyInfoId != 0)
                    {

                        baseElementNames[ele.Name] = true;

                    }
                }

                foreach (Element ele in this.allElements.Values)
                {
                    if (ele.TaxonomyInfoId == 0 && validelementsDt.ContainsKey(ele.Id))
                    {

                        extendedElements[ele.Id] = true;

                        if (baseElementNames.ContainsKey(ele.Name))
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The name of an extended element cannot equal the name of a standard element. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_ELEMENT_NAME_ERROR);
                            secValidationErrors.Add(info);
                        }

                        if (ele.OrigsubstGroup.Equals(Element.DIMENSION_ITEM_TYPE))
                        {
                            if (!ele.Name.EndsWith("Axis"))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The name of an extended dimension element must end with 'Axis'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_AXIS_NAME_ERROR1);
                                secValidationErrors.Add(info);
                            }
                        }
                        else
                        {
                            if (ele.Name.EndsWith("Axis", StringComparison.CurrentCultureIgnoreCase))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Only Dimension type elements can have names ending with 'Axis'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_AXIS_NAME_ERROR2);
                                secValidationErrors.Add(info);
                            }
                        }

                        if (ele.OrigsubstGroup.Equals(Element.HYPERCUBE_ITEM_TYPE))
                        {
                            if (!ele.Name.EndsWith("Table"))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The name of an extended hypercube element must end with 'Table'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_TABLE_NAME_ERROR1);
                                secValidationErrors.Add(info);
                            }
                        }
                        else
                        {
                            if (ele.Name.EndsWith("Table", StringComparison.CurrentCultureIgnoreCase))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Only hypercube elements can have names ending with 'Table'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_TABLE_NAME_ERROR2);
                                secValidationErrors.Add(info);
                            }
                        }

                        if (ele.Name.EndsWith("LineItems", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!ele.IsAbstract)
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Only Abstract elements can have names ending with 'LineItems'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_LINE_ITEM_NAME_ERROR1);
                                secValidationErrors.Add(info);
                            }
                        }

                        if (ele.OrigElementType.EndsWith(":domainItemType"))
                        {
                            if (!ele.Name.EndsWith("Domain") && !ele.Name.EndsWith("Member"))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Dimension member must have name that ends with either 'Domain' or 'Member'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_MEMBER_NAME_ERROR1);
                                secValidationErrors.Add(info);

                            }
                        }
                        else
                        {
                            if (ele.Name.EndsWith("Domain", StringComparison.CurrentCultureIgnoreCase))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Only dimension members can have names ending with 'Domain'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_MEMBER_NAME_ERROR2);
                                secValidationErrors.Add(info);
                            }
                            else if (ele.Name.EndsWith("Member", StringComparison.CurrentCultureIgnoreCase))
                            {
                                ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Only dimension members can have names ending with 'Member'. Element to change = '{0}'.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_MEMBER_NAME_ERROR3);
                                secValidationErrors.Add(info);
                            }
                        }

                    }
                }

                if (extendedElements.Count > 0)
                {

                    List<Node> missing = GetInUseNodesMissingInPresentation(extendedElements,
                        false, presNodes, excludedReports);
                    foreach (Node n in missing)
                    {
                        if (isMutualFundTaxonomy)
                        {
                            //if it is a dimension member then it should be ok to miss it in presenation
                            if (n.MyElement.OrigElementType.EndsWith(":domainItemType"))
                            {
                                continue;
                            }
                        }

                        ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("Extended element '{0}' is missing in the Presentation view.", n.Label),
                            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.EXTENDED_ELEMENT_MISSING_IN_PRESENTATION_ERROR);
                        secValidationErrors.Add(info);
                    }

                }
                Dictionary<string, DimensionNode> extNodes = GetExtendedDimensionMembers(dimNodes);

                foreach (DimensionNode dn in extNodes.Values)
                {
                    if (!validelementsDt.ContainsKey(dn.Id)) continue;
                    if (dn.NodeDimensionInfo.NodeType == DimensionNode.NodeType.Item)
                    {
                        //member...
                        if (!dn.MyElement.Name.EndsWith("Domain") && !dn.MyElement.Name.EndsWith("Member"))
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("Dimension member must have name that ends with either 'Domain' or 'Member'. Element to change = '{0}'.", dn.MyElement.Name),
                            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_MEMBER_NAME_ERROR1);
                            secValidationErrors.Add(info);

                        }


                        if (!dn.MyElement.OrigElementType.EndsWith(":domainItemType"))
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
                            string.Format("Dimension member was created with an incorrect type. Please recreate the member '{0}' to fix this issue.", dn.MyElement.Name),
                            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Extension Error", ValidationErrorInfo.SequenceEnum.EXTENDED_DIMENSION_MEMBER_TYPE_ERROR);
                            secValidationErrors.Add(info);

                        }
                    }


                }
            }
        }
        private void PerformTextBlockElementValidation(ArrayList presNodes,
            Dictionary<string, bool> markedupElements,
            ref List<ValidationErrorInfo> secValidationErrors)
        {

            foreach (Node n in presNodes)
            {
                if (n.children != null && n.children.Count > 0)
                {

                    foreach (Node topLevelNode in n.children)
                    {
                        //we are interested only in non abstract elements that are in use...
                        if (topLevelNode.IsAbstract ||
                            !markedupElements.ContainsKey(topLevelNode.MyElement.Id) ||
                            !topLevelNode.MyElement.IsTextBlock()) continue;


                        //we have an extended element as a top level node in presentation...
                        if (topLevelNode.MyElement.IsTextBlock())
                        {
                            //we have a text block as the top level node..
                            //if this node is not a valid child node else where ... it is a bug...
                            //based on the edgar manual bug...

                            /* Edgar manual info....
             A Text Block for each Footnote must appear in at least one presentation relationship in a base set.
             Each base set for a “Footnote as a Text Block” presentation link must contain one
                presentation relationship whose target is a Text Block.
                            */
                            bool isValidChild = false;
                            foreach (Node pn in presNodes)
                            {
                                if (isValidChild) break;
                                if (pn.children != null && pn.children.Count > 0)
                                {

                                    foreach (Node tpn in n.children)
                                    {
                                        if (RecursivelyCheckElementIfItIsaValidChild(tpn, topLevelNode.MyElement.Id))
                                        {
                                            isValidChild = true;
                                            break;
                                        }
                                    }

                                }
                            }


                            if (!isValidChild)
                            {
                                //error///
                                ValidationErrorInfo info = new ValidationErrorInfo(
                        string.Format("In use text block element '{0}' needs to be a target of at least one presentation relationship. It is defined only as a top level element in the report {1}", topLevelNode.MyElement.Id, n.MyPresentationLink.Title),
                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.TEXT_BLOCK_PRESENTATION_RELATION_ERROR);
                                secValidationErrors.Add(info);
                            }

                        }
                    }



                }


            }
        }


        private bool RecursivelyCheckElementIfItIsaValidChild(Node parentNode, string elementId)
        {
            if (parentNode.IsProhibited) return false;

            if (parentNode.children != null)
            {
                foreach (Node childNode in parentNode.children)
                {
                    if (childNode.IsProhibited) continue;

                    if (childNode.MyElement.Id.Equals(elementId)) return true;

                    if (RecursivelyCheckElementIfItIsaValidChild(childNode, elementId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void PerfromExtendedRolesValidation(ref List<ValidationErrorInfo> secValidationErrors)
        {
            List<RoleType> extendedRoles = this.GetListOfExtendedRoles();
            Uri targetNameSpaceURI = new Uri(this.targetNamespace);
            foreach (RoleType rt in extendedRoles)
            {
                bool isValid = rt.Uri.Contains("/role/");


                if (isValid)
                {

                    Uri roleURI = new Uri(rt.Uri);

                    isValid = targetNameSpaceURI.Authority.Equals(roleURI.Authority);

                }

                if (!isValid)
                {

                    ValidationErrorInfo info = new ValidationErrorInfo(
            string.Format("URI '{0}' of Report '{1}' does not comply with the required format {2}. Please recreate the report.", rt.Uri, rt.Definition, "http://{Authority}/.../role/{mnemonic name in LC3 format}"),
            ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Presentation Error", ValidationErrorInfo.SequenceEnum.REPORT_NAME_URI_ERROR);
                    secValidationErrors.Add(info);
                }

            }
        }

        public static string[] invalidAsciiSequences = new string[] { "&lt;", "&#60;", "&#x3C" };

        //public static string InvalidSECLabelCharaters = "?|<>*+=#\"";
        public static string InvalidSECLabelCharaters = "<";
        private void PerformElementLabelValidations(ref List<ValidationErrorInfo> secValidationErrors,
            Dictionary<string, Element> elementsDt, Dictionary<string, bool> markedupElements,
            ArrayList presNodes, bool isMutualFundsTaxonomy )
        {

            Dictionary<string, bool> elementsInPres = new Dictionary<string, bool>();
            Dictionary<string, Element> stdLabelDt = new Dictionary<string, Element>();

           

            List<string> allTopLevelPresentationElements = new List<string>();
            //since the top level elements cannot have a pref label...
            //if two of the top level elements have the same label we need to show an error...
            foreach (Node tn in presNodes)
            {
                if (tn.children != null)
                {

                    foreach (Node pn in tn.children)
                    {
                        if (!allTopLevelPresentationElements.Contains(pn.Id))
                        {
                            allTopLevelPresentationElements.Add(pn.Id);

                        }
                    }
                }
            }

            //TODO: need to do just the elements in the presentation , calculation and definition files.
            foreach (Element ele in this.allElements.Values)
            {
                if (!elementsDt.ContainsKey(ele.Id)) continue;

                if (ele.LabelInfo == null) continue;


                bool checkStandardLabel = true;


                if (markedupElements.ContainsKey(ele.Id))
                {
                    if (ele.OrigElementType.EndsWith(":domainItemType"))
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The dimension element {0} is being used as a regualar fact, please use it only as a segment.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Segment Element Error", ValidationErrorInfo.SequenceEnum.DIMENSION_AS_ELEMENT_ERROR);


                        secValidationErrors.Add(info);
                    }
                }


                if ( ele.TaxonomyInfoId == 0 && ele.OrigElementType.EndsWith(":domainItemType") 
                    )
                {
                    //for mutual funds we are going to change the label to terse label so 
                    //no need to check for stanard label problems...
                    checkStandardLabel = false;
                }
                


                foreach (LabelDefinition ld in ele.LabelInfo.LabelDatas)
                {
                    if (ld.LabelRole.Equals("documentation", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue; //as we do not want to include the documentation
                    }

                    if (ld.LabelRole.Equals(LabelDefinition.DEPRECATED_LABEL_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        
                            ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The element {0} has been deprecated, please use a different element in the markup.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Deprecated Element Error", ValidationErrorInfo.SequenceEnum.DECPRECATED_ELEMENT_USED_ERROR);
                            secValidationErrors.Add(info);

                        
                        continue; //as we do not want to include the documentation
                    }
                    if (ld.LabelRole.Equals(LabelDefinition.DEPRECATEDDATE_LABEL_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        
                            ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The element {0} has been deprecated, please use a different element in the markup.", ele.Name),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Deprecated Element Error", ValidationErrorInfo.SequenceEnum.DECPRECATED_ELEMENT_USED_ERROR);
                            secValidationErrors.Add(info);

                        
                        continue; //as we do not want to include the documentation
                    }
                    if (ld.Label.Length >= 511)
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
        string.Format("The text of the label should be fewer than 511 characters. Please correct the label '{0}' for element '{1}'.", ld.Label, ele.Name),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Label Error", ValidationErrorInfo.SequenceEnum.MAX_LABEL_SIZE_ERROR);
                        secValidationErrors.Add(info);
                    }

                    if (ld.Label.IndexOfAny(InvalidSECLabelCharaters.ToCharArray()) >= 0)
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
        string.Format("The following special characters : '{2}' are not allowed in the text of a label. Please correct the label '{0}' for element '{1}'.", ld.Label, ele.Name, InvalidSECLabelCharaters),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Label Error", ValidationErrorInfo.SequenceEnum.INVALID_CHARACTER_IN_LABEL_ERROR);
                        secValidationErrors.Add(info);
                    }

                    foreach (string str in invalidAsciiSequences)
                    {
                        if (ld.Label.Contains(str))
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
        string.Format("The following ASCII sequence is prohibited in the content of labels other than documentation labels: '{2}'. Please correct the label '{0}' for element '{1}'.", ld.Label, ele.Name, str),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Label Error", ValidationErrorInfo.SequenceEnum.INVALID_ASCII_SEQUENCE_IN_LABEL_ERROR);
                            secValidationErrors.Add(info);
                        }
                    }

                    if (ld.LabelRole == Node.LABELROLE)
                    {




                        if (checkStandardLabel)
                        {

                            if (ld.Language.Equals("en-us", StringComparison.CurrentCultureIgnoreCase) ||
                                ld.Language.Equals("en", StringComparison.CurrentCultureIgnoreCase))
                            {
                                ld.Label = ld.Label.Trim(TRIMSET);

                                ld.Label = multispaceRegEx.Replace(ld.Label, " ");//replace double space with a single space...

                                string asciiLabel = TextUtilities.CovertValidUTF8ToAsciiInLabel(ld.Label.ToLower());
                               

                                Element other;
                                if (stdLabelDt.TryGetValue(asciiLabel, out other))
                                {







                                    ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("Multiple elements cannot have the same standard english label. Element '{0}' and element '{1}' have the same label '{2}'.", ele.Name, other.Name, ld.Label),
                                ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Label Error", ValidationErrorInfo.SequenceEnum.MULTIPLE_ELEMENT_SAME_STD_LABEL_ERROR);
                                    secValidationErrors.Add(info);



                                }
                                else
                                {
                                    stdLabelDt[asciiLabel] = ele;
                                }



                                if (ele.TaxonomyInfoId == 0)
                                {

                                    if (string.Compare(ele.Name,
                                                    StringUtility.GetDefaultElementName(ld.Label)) != 0)
                                    {

                                        ValidationErrorInfo.ValidationCategoryType cType = ValidationErrorInfo.ValidationCategoryType.SECValidationWarning_LC3;

                                        if (ele.OrigElementType.EndsWith(":domainItemType"))
                                        {
                                            cType = ValidationErrorInfo.ValidationCategoryType.SECValidationWarning_LC3_Members;
                                        }

                                        ValidationErrorInfo info = new ValidationErrorInfo(
                                string.Format("The name of an extended element '{0}' should consist of capitalized words corresponding to the standard english label, a convention called LC3. The standard label is '{1}' and its corresponding LC3 name is '{2}'.",
                                ele.Name, ld.Label, StringUtility.GetDefaultElementName(ld.Label)),
                                cType, "Element Name Warning", ValidationErrorInfo.SequenceEnum.LC3_WARNING);

                                        info.MyErrorType = ValidationErrorInfo.ErrorType.Warning;
                                        secValidationErrors.Add(info);

                                    }
                                }

                            }
                        }

                    }


                    if (string.IsNullOrEmpty(ld.Label.Trim()))
                    {
                        ValidationErrorInfo info = new ValidationErrorInfo(
        string.Format("The text of the label is an emply string. Please correct the label with label role '{0}' for the for element '{1}'.", ld.LabelRole, ele.Name),
        ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Label Error", ValidationErrorInfo.SequenceEnum.EMPTY_LABEL_ERROR);
                        secValidationErrors.Add(info);
                    }


                }



            }


        }


        ////just a flat list of all axis and members...
        //private Dictionary<string, bool>  BuildDimensionElementsUsed( 
        //    List<MarkupProperty> validMarkups  )
        //{
        //    Dictionary<string, bool> ret = new Dictionary<string, bool>();

        //    foreach (MarkupProperty mp in validMarkups)
        //    {
        //        if (mp.contextRef == null) continue;

        //        if (mp.contextRef.Segments != null && mp.contextRef.Segments.Count > 0)
        //        {
        //            List<string> eleDimMembers = new List<string>();
        //            StringBuilder dimVals = new StringBuilder();
        //            foreach (Segment seg in mp.contextRef.Segments)
        //            {
        //                if (seg.DimensionInfo == null) continue;
        //                ret[seg.DimensionInfo.Id] = true;
        //                ret[seg.DimensionInfo.dimensionId] = true;
        //            }

        //        }

        //    }

        //    return ret;
        //}



        private void RecursivelyCheckUniquenessOfParentChildArc(Node parent,
            Dictionary<string, bool> markedupElements,
            ref Dictionary<string, List<Node>> parentChildArcToRoleInfo,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (parent.IsProhibited) return;


            if (parent.children != null && parent.children.Count > 0)
            {

                foreach (Node child in parent.children)
                {
                    if (child.IsProhibited) continue;
                    if (markedupElements.ContainsKey(parent.Id) && markedupElements.ContainsKey(child.Id))
                    {
                        string key = parent.Id + child.Id;

                        List<Node> otherParentNodes;
                        bool addParentNode = true;

                        if (parentChildArcToRoleInfo.TryGetValue(key, out otherParentNodes))
                        {

                            foreach (Node opn in otherParentNodes)
                            {
                                if (opn.GetPresentationLink().Role == parent.GetPresentationLink().Role)
                                {
                                    //current parent already checked....
                                    addParentNode = false;
                                    break;
                                }
                                Node GoodParent, badParent;
                                if (IsInCompleteCalculation(opn, parent, markedupElements,
                                    out GoodParent, out badParent))
                                {

                                    ValidationErrorInfo info = new ValidationErrorInfo(
                                    string.Format("Calculation relationship between '{0}' and '{1}' is defined in report '{2}' and report '{3}'. Please remove this relationship from one of the reports to correct this problem.",
                                    parent.Label, child.Label, parent.GetPresentationLink().Title,
                                    opn.GetPresentationLink().Title),
                                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Arc Error", ValidationErrorInfo.SequenceEnum.DUPLICATE_CALCULATION_ARC_ERROR);
                                    secValidationErrors.Add(info);
                                }
                            }


                        }
                        else
                        {
                            otherParentNodes = new List<Node>();
                            parentChildArcToRoleInfo[key] = otherParentNodes;

                        }


                        if (addParentNode)
                        {
                            otherParentNodes.Add(parent);
                        }

                    }


                    RecursivelyCheckUniquenessOfParentChildArc(child, markedupElements,
                        ref parentChildArcToRoleInfo, ref secValidationErrors);
                }
            }
        }

        /// <summary>
        /// SEC does not want duplicate calc info or partial calc infos
        /// so either one of the parent is a duplicate of the other or is a subset of the other...
        /// either cse we will report an error...
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <param name="markedupElements"></param>
        /// <param name="GoodParent"></param>
        /// <param name="badParent"></param>
        /// <returns></returns>
        private bool IsInCompleteCalculation(Node parent1, Node parent2, Dictionary<string, bool> markedupElements,
            out Node GoodParent, out Node badParent)
        {
            GoodParent = badParent = null;

            List<string> children1 = new List<string>();
            if (parent1.children != null)
            {
                foreach (Node child in parent1.children)
                {
                    if (child.IsProhibited) continue;
                    if (!markedupElements.ContainsKey(child.Id)) continue;

                    children1.Add(child.Id);
                }
            }

            List<string> children2 = new List<string>();
            if (parent2.children != null)
            {
                foreach (Node child in parent2.children)
                {
                    if (child.IsProhibited) continue;
                    if (!markedupElements.ContainsKey(child.Id)) continue;

                    children2.Add(child.Id);
                }
            }


            if (children1.Count == 0 || children2.Count == 0) return false;

            //looks like the list of children  is not the same so.. it is ok to have these calc arcs..
            if (children1.Count != children2.Count) return false;

            bool isOneBad = true;
            foreach (string childId1 in children1)
            {
                if (!children2.Contains(childId1))
                {
                    isOneBad = false;
                    break;
                }
            }

            if (isOneBad)
            {
                GoodParent = parent2;
                badParent = parent1;
                return true;
            }

            bool isTwoBad = true;
            foreach (string childId2 in children2)
            {
                if (!children1.Contains(childId2))
                {
                    isTwoBad = false;
                    break;
                }
            }

            if (isTwoBad)
            {
                GoodParent = parent1;
                badParent = parent2;
                return true;
            }


            return false;

        }

        private void RecursivelyGetInUseNodes(Dictionary<string, bool> markedupElements, Node n,
            ref Dictionary<string, List<Node>> inUsePresNodes,
            ref List<string> inUseElementsInReport)
        {
            if (n.IsProhibited) return;

            if (markedupElements.ContainsKey(n.Id))
            {
                inUseElementsInReport.Add(n.Id);
                List<Node> ns;
                if (!inUsePresNodes.TryGetValue(n.Id, out ns))
                {
                    ns = new List<Node>();
                    inUsePresNodes[n.Id] = ns;
                }
                ns.Add(n);
            }
            if (n.children != null && n.children.Count > 0)
            {
                foreach (Node cn in n.children)
                {
                    RecursivelyGetInUseNodes(markedupElements, cn, ref inUsePresNodes, ref inUseElementsInReport);
                }
            }
        }

        private void CheckPeriodTypeMismatchInCalculation(
            ArrayList calcNodes, 
            Dictionary<string, List<Node>> CalcParents,
            ref List<ValidationErrorInfo> secValidationErrors)
        {

            //go through each top level report...and if the element exists in CalcParents then 
            //recursively check for same period type...
            foreach (Node top in calcNodes)
            {
                if (top.children != null)
                {
                    foreach (Node rootNode in top.children)
                    {
                        if (!rootNode.IsProhibited && CalcParents.ContainsKey(rootNode.Id))
                        {

                            //recursively check for period type..
                            RecursivelyCheckPeriodTypeMismatch(rootNode, ref secValidationErrors);
                        }


                    }
                }

            }
        }

        private void RecursivelyCheckPeriodTypeMismatch( Node currentNode,
            ref List<ValidationErrorInfo> secValidationErrors)
        {
            if (currentNode.IsProhibited) return;

            if (currentNode.children != null)
            {
                foreach (Node child in currentNode.children)
                {
                    if (child.IsProhibited) continue;
                    if (currentNode.PeriodType != child.PeriodType)
                    {

                        ValidationErrorInfo info = new ValidationErrorInfo(
                                    string.Format("Calculation relationship between '{0}' and '{1}' in report {2} is not allowed as their period types are not the same.",
                                    currentNode.Label, child.Label, child.GetPresentationLink().Title,
                                    currentNode.GetPresentationLink().Title),
                                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Arc Error", ValidationErrorInfo.SequenceEnum.CALCULATION_PERIOD_ERROR);
                        secValidationErrors.Add(info);
                    }
                    else
                    {
                        RecursivelyCheckPeriodTypeMismatch(child, ref secValidationErrors);

                    }
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inUseElements"></param>
        /// <param name="role"></param>
        /// <param name="includeSegments"></param>
        /// <param name="missingPresentation"></param>
        /// <returns></returns>
        public void GetCalculationRelationshipsMissingInPresentation(
            Dictionary<string, bool> inUseElements,
            Dictionary<string, List<Node>> CalcParents,
            Dictionary<string, List<string>> inUsePresElementIdsByReport,
            ref List<ValidationErrorInfo> secValidationErrors)
        {


            if (inUseElements.Count == 0) return;


            foreach (KeyValuePair<string, List<Node>> kvp in CalcParents)
            {

                if (!inUseElements.ContainsKey(kvp.Value[0].Id)) continue;


                Dictionary<string, List<string>> reportsWithParent = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> kvp1 in inUsePresElementIdsByReport)
                {
                    if (kvp1.Value.Contains(kvp.Value[0].Id))
                    {
                        reportsWithParent[kvp1.Key] = kvp1.Value;
                    }
                }

                foreach (Node pn in kvp.Value)
                {
                    if (pn.children == null) continue;



                    foreach (Node cn in pn.children)
                    {
                        if (!inUseElements.ContainsKey(cn.Id)) continue;


                        bool found = false;
                        foreach (KeyValuePair<string, List<string>> kvp2 in reportsWithParent)
                        {
                            if (kvp2.Value.Contains(cn.Id))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            ValidationErrorInfo info = new ValidationErrorInfo(
                    string.Format("Calculation Report '{0}' has element {1} as a parent and element {2} as a child. There should be at least one effective presentation relationship between these elements in any presentation report.",
                    pn.GetPresentationLink().Title,
                    pn.Name, cn.Name),
                    ValidationErrorInfo.ValidationCategoryType.SECValidationError, "Calculation Error", ValidationErrorInfo.SequenceEnum.CALCULATION_RELATIONSHIP_MISSING_IN_PRESENTATION_ERROR);
                            secValidationErrors.Add(info);
                        }

                    }

                }

            }




        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inUseElements"></param>
        /// <param name="role"></param>
        /// <param name="includeSegments"></param>
        /// <param name="missingPresentation"></param>
        /// <returns></returns>
        public List<Node> GetInUseCalculationNodesMissingInCurrentReport(Dictionary<string, bool> inUseElements,
            string role, bool includeSegments, ref bool missingPresentation, Node calcRootNode)
        {
            List<Node> nodesNotInPresentation = new List<Node>();

            if (inUseElements.Count > 0)
            {
                if (calcRootNode == null)
                {
                    calcRootNode = this.GetCalculationForRole(role);
                }
                if (calcRootNode == null) return nodesNotInPresentation;

                Node presRootNode = this.GetPresentationForRole(includeSegments, role);

                if (presRootNode == null)
                {
                    missingPresentation = true;
                    return nodesNotInPresentation;
                }




                Dictionary<string, bool> elementsAvailableInPresentation = new Dictionary<string, bool>();
                RecursivelyBuildAvailableElementInfo(presRootNode, ref elementsAvailableInPresentation);
                Dictionary<string, bool> elementsAvailableInCalc = new Dictionary<string, bool>();
                RecursivelyBuildAvailableElementInfo(calcRootNode, ref elementsAvailableInCalc);



                foreach (string eleId in inUseElements.Keys)
                {

                    if (elementsAvailableInCalc.ContainsKey(eleId) &&
                        !elementsAvailableInPresentation.ContainsKey(eleId))
                    {
                        Element ele = this.allElements[eleId] as Element;

                        if (ele != null)
                        {
                            Node node = new Node(ele);
                            node.UpdateLabel(this.CurrentLanguage, PresentationLocator.preferredLabelRole);
                            nodesNotInPresentation.Add(node);
                        }
                        else
                        {
                            //found element not in the current tax... just ignore for now....
                        }

                    }
                }



            }

            return nodesNotInPresentation;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="inUseElements"></param>
        /// <param name="includeSegments"></param>
        /// <returns></returns>
        public List<Node> GetInUseNodesMissingInPresentation(Dictionary<string, bool> inUseElements,
            bool includeSegments, ArrayList nodes, List<string> excludedReports)
        {
            List<Node> nodesNotInPresentation = new List<Node>();

            if (inUseElements.Count > 0)
            {
                Dictionary<string, bool> elementsAvailableInPresentation = GetElementsWithPresenationRelationship(includeSegments, nodes, excludedReports);

                foreach (string eleId in inUseElements.Keys)
                {

                    if (!elementsAvailableInPresentation.ContainsKey(eleId))
                    {

                        Element ele = this.allElements[eleId] as Element;

                        if (ele != null)
                        {
                            Node node = new Node(ele);
                            node.UpdateLabel(this.currentLanguage, PresentationLocator.preferredLabelRole);
                            nodesNotInPresentation.Add(node);
                        }
                        else
                        {
                            //found element not in the current tax... just ignore for now....
                        }

                    }
                }

            }

            return nodesNotInPresentation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSegments"></param>
        /// <returns></returns>
        private Dictionary<string, bool> GetElementsWithPresenationRelationship(
            bool includeSegments, ArrayList nodes, List<string> excludedReports)
        {
            if (nodes == null)
            {
                nodes = this.GetNodesByPresentation(includeSegments);

            }
            Dictionary<string, bool> elementsAvailableInPresentation = new Dictionary<string, bool>();
            foreach (Node n in nodes)
            {

                if (excludedReports != null && excludedReports.Contains(n.GetPresentationLink().Role))
                {
                    //need to skip this report...
                    continue;
                }
                if (n.children == null) continue;
                foreach (Node tn in n.children)
                {
                    //we do not include elements that do not have a pres arc..
                    //top level nodes that are stand alone nodes... are not to be considered...
                    if (tn.children == null || tn.children.Count == 0) continue;

                    RecursivelyBuildAvailableElementInfo(tn, ref elementsAvailableInPresentation);
                }
            }


            return elementsAvailableInPresentation;
        }



        private void RecursivelyBuildAvailableElementInfo(Node n, ref Dictionary<string, bool> elementsAvailableInPresentation)
        {
            if (n.IsProhibited) return;
            elementsAvailableInPresentation[n.Id] = true;

            if (n.children != null)
            {
                foreach (Node cn in n.children)
                {
                    RecursivelyBuildAvailableElementInfo(cn, ref elementsAvailableInPresentation);
                }
            }

        }




        private Dictionary<string, DimensionNode> GetExtendedDimensionMembers(List<DimensionNode> dimNodes)
        {
            Dictionary<string, DimensionNode> ret = new Dictionary<string, DimensionNode>();
            foreach (DimensionNode dn in dimNodes)
            {


                RecursivelyGetExtendedDimensionNodes(dn, ref ret);
            }



            return ret;
        }

        private void RecursivelyGetExtendedDimensionNodes(DimensionNode dn, ref Dictionary<string, DimensionNode> extNodes)
        {

            if (dn.TaxonomyInfoId == 0 && dn.NodeDimensionInfo != null)
            {
                extNodes[dn.Id] = dn;
            }
            if (dn.children != null)
            {
                foreach (DimensionNode child in dn.children)
                {
                    RecursivelyGetExtendedDimensionNodes(child, ref extNodes);
                }
            }

        }


        public void RecursivelyGetAllAvailableElements(ref Dictionary<string, Element> elements, Node n)
        {
            if (n == null ||  n.IsProhibited) return;
            elements[n.Id] = n.MyElement;
            if (n.children != null)
            {
                foreach (Node child in n.children)
                {
                    RecursivelyGetAllAvailableElements(ref elements, child);
                }
            }
        }


        #endregion



        


        #region load additional reference / documentation data


        public static bool TryGetDocumentationInformation(string language, string fileName,
            ref Dictionary<string, string> documentationByElementId, out string err)
        {
            if (documentationByElementId == null)
            {
                documentationByElementId = new Dictionary<string, string>();
            }
            err = null;

            //assuming local file
            if (File.Exists(fileName))
            {
                Label l = new Label();
                l.loadingTaxonomy = null;
                l.PromptUser = false;
                int numErrors;
                l.Load(fileName, out numErrors);

                if (numErrors == 0)
                {
                    l.Parse(out numErrors);

                }

                if (numErrors > 0)
                {
                    err = "Failed to load label file " + fileName;
                    return false;
                }
                if (l.LabelTable != null)
                {
                    foreach (LabelLocator ll in l.LabelTable.Values)
                    {
                        foreach (LabelDefinition ld in ll.LabelDatas)
                        {
                            //TODO: update the parse to filter out based on languange and label role...
                            if (ld.Language.Equals(language) && ld.LabelRole == DOCUMENTATION)
                            {
                                documentationByElementId[ll.href] = ld.Label;
                            }
                        }
                    }

                }

            }
            else
            {
                err = "Label file does not exist " + fileName;
            }



            return err == null;
        }


        public static bool TryGetReferenceInformation(string fileName,
            ref Dictionary<string, string> referenceByElementId, out string err)
        {
            if (referenceByElementId == null)
            {
                referenceByElementId = new Dictionary<string, string>();
            }
            err = null;

            //assuming local file
            if (File.Exists(fileName))
            {
                Reference reference = new Reference();
                reference.loadingTaxonomy = null;
                reference.PromptUser = false;
                int numErrors;


                reference.Load(fileName, out numErrors);
                if (numErrors == 0)
                {
                    reference.Parse(out numErrors);

                }

                if (numErrors > 0)
                {
                    err = "Failed to load reference file " + fileName;
                    return false;
                }

                if (reference.ReferencesTable != null)
                {
                    string formatStr = TraceUtility.FormatStringResource("XBRLUserControls.ElementBuilder.ReferenceNum");

                    foreach (ReferenceLocator refLocator in reference.ReferencesTable.Values)
                    {
                        if (refLocator.References != null && refLocator.References.Count > 0)
                        {
                            StringBuilder sbReference = new StringBuilder();

                            int refCount = 1;

                            foreach (DictionaryEntry de in refLocator.References)
                            {
                                sbReference.Append(string.Format(formatStr, refCount));

                                string key = de.Key as String;
                                int index = key.LastIndexOf(" ");
                                if (index >= 0)
                                {
                                    key = key.Substring(0, index);
                                }
                                sbReference.Append(key);
                                sbReference.Append(Environment.NewLine);

                                ArrayList arRef = de.Value as ArrayList;
                                if (arRef[0] is string)
                                {

                                    foreach (string refItem in arRef)
                                    {
                                        sbReference.Append(" -").Append(refItem);
                                    }
                                }
                                else
                                {
                                    if (arRef[0] is System.Collections.DictionaryEntry)
                                    {
                                        foreach (System.Collections.DictionaryEntry refItem in arRef)
                                        {
                                            sbReference.Append(" -").Append(refItem.Key).Append(": ").
                                                Append(refItem.Value).Append(Environment.NewLine);
                                        }
                                    }
                                }

                                sbReference.Append(Environment.NewLine);

                                refCount++;
                            }


                            referenceByElementId[refLocator.href] = sbReference.ToString();
                        }



                    }

                }


            }
            else
            {
                err = "Reference file does not exist " + fileName;
            }



            return err == null;
        }

        #endregion


        public bool IsMutualFundTaxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.us/rr/2010-01-01") || ti.WebLocation.Contains("http://xbrl.sec.gov/rr/2010-02-28"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns true if the current taxonomy is using 2008 taxonomy and it's extended one
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireExtendedUSGAAP2008Taxonomy()
        {
            if (this.IsAucentExtension)
                return IsCrossfireUSGAAP2008Taxonomy();

            return false;
        }

        /// <summary>
        /// returns true if the current taxonomy is using 2009 taxonomy and it's extended one
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireExtendedUSGAAP2009Taxonomy()
        {
            if (this.IsAucentExtension)
                return IsCrossfireUSGAAP2009Taxonomy();

            return false;
        }

        /// <summary>
        /// returns true if the current taxonomy is using 2011 taxonomy and it's extended one
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireExtendedUSGAAP2011Taxonomy()
        {
            if (this.IsAucentExtension)
                return IsCrossfireUSGAAP2011Taxonomy();
   
            return false;
        }


        /// <summary>
        /// returns true if the current taxonomy is using 2008 taxonomy 
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireUSGAAP2008Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.us/us-gaap/2008-03-31"))
                {
                    return true;
                }
            }
 
            return false;
        }

        /// <summary>
        /// returns true if the current taxonomy is using 2009 taxonomy
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireUSGAAP2009Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.us/us-gaap/2009-01-31"))
                {
                    return true;
                }
            }
 
            return false;
        }

        /// <summary>
        /// returns true if the current taxonomy is using 2011 taxonomy
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireUSGAAP2011Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://fasb.org/us-gaap/2011-01-31"))
                    return true;
            }
  
            return false;
        }


        /// <summary>
        /// returns true if the current DEI taxonomy is using 2008 taxonomy 
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireDEI2008Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.us/dei/2008-03-31"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns true if the current DEI taxonomy is using 2009 taxonomy
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireDEI2009Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.us/dei/2009-01-31"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns true if the current DEI taxonomy is using 2011 taxonomy
        /// </summary>
        /// <returns></returns>
        public bool IsCrossfireDEI2011Taxonomy()
        {
            foreach (TaxonomyItem ti in this.infos)
            {
                if (ti.WebLocation.Contains("http://xbrl.sec.gov/dei/2011-01-31"))
                    return true;
            }

            return false;
        }
    }
}

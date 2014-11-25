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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// information that relates an element to an hypercube....
    /// This information is maintained inside the defintionLink for every
    /// "ALL" , "NOTALL" arc defined in the defintionLink ( also called as BaseSet in the spec )
    /// </summary>
    public class ElementHypercubeRelationhipInfo
    {
        /// <summary>
        /// validation status of each markup....
        /// </summary>
        public enum ValidationStatus
        {
            /// <summary>
            /// Status does not apply
            /// </summary>
            None = 0,
            /// <summary>
            /// Valid for the relationship specified.
            /// </summary>
            Valid = 1,
            /// <summary>
            /// not valid for the relationship specified.
            /// </summary>
            NotValid = -1

        }

        #region Properties

        /// <summary>
        /// List of all dependent element ..determined by walking down the element tree.
        /// these elements inherit the same dimension rules as the ElementId
        /// </summary>
        public List<string> ElementIdList = new List<string>();

        /// <summary>
        /// Determines if this relationship is for segment or scenario
        /// </summary>
        public bool IsSegment;
        /// <summary>
        /// if the relationship is closed then the elements cannot have anything 
        /// that is outside of the dimensions supported by this hypercube
        /// If it is a not all relation ship then the negative of that is true...
        /// </summary>
        public bool IsClosed;

        /// <summary>
        /// Determines whether the dimension is usable on not usable for this element..
        /// </summary>
        public bool IsAll;
        /// <summary>
        /// Id of the hypercube...
        /// </summary>
        public string HypercubeId;


        private string segmentScenarioText
        {
            get
            {
                return IsSegment ? "Segment" : "Scenario";

            }
        }

        /// <summary>
        /// Dimensions impacted and its members....
        /// </summary>
        public Dictionary<string, DimensionNode> DimensionsById = new Dictionary<string, DimensionNode>();

        /// <summary>
        /// The base set for which this object is a member...
        /// </summary>
        public DefinitionLink ParentBaseSet;


		internal Taxonomy ParentTaxonomyObj ;
        #endregion

        #region public methods

        internal ValidationStatus ValidateMarkup(string elementId, ArrayList segments, ArrayList scenarios, out string errorString)
        {
            ValidationStatus ret = ValidationStatus.None;
            errorString = null;
            if (!this.ElementIdList.Contains(elementId))
            {
                return ret; //this element is not impacted by this ElementHypercubeRelationhipInfo
            }

            List<ContextDimensionInfo> cdiList; 
            bool hasUnSupportedData = false;
	
            GetInformationToValidate(segments, scenarios,
                out cdiList,ref hasUnSupportedData );

			if (this.IsClosed)
			{
				#region Positive validations
				if (IsAll)
				{
					//closed relationship cannot have any thing else in the context
					if (hasUnSupportedData)
					{
						errorString = TraceUtility.FormatStringResource("XBRLParser.Error.DimensionUnsupportedContext", segmentScenarioText, elementId);
						//errorString = string.Format("Error|xbrl.dimensions.PrimaryItemDimensionallyInvalidError:Element {1} has a context that uses unsupported {0} information", segmentScenarioText, elementId);
						return ValidationStatus.NotValid;

					}

					if (this.DimensionsById.Count == 0 && cdiList.Count > 0)
					{
						//closed relationship with an empty hypercube...
						//so should not have any dimension information
						errorString = TraceUtility.FormatStringResource("XBRLParser.Error.DimensionInvalidContextInfo", segmentScenarioText, elementId);
						//errorString = string.Format( "Error|xbrl.dimensions.PrimaryItemDimensionallyInvalidError:Element {1} cannot have any {0} information in its context", segmentScenarioText, elementId );

						return ValidationStatus.NotValid;
					}


				}
				#endregion
			}
			
			//need to make sure that if a dimension is not used then we atleast have a default for it
			if (cdiList.Count < this.DimensionsById.Count)
			{
				//we need to error out if the dimensions not in the cdilist 
				//does not have a default value
				//only dimension that is not required are the ones that 
				//has a default value
				foreach (DimensionNode dimNode in DimensionsById.Values)
				{

					if (dimNode.HasDefaultChild() ||
						this.ParentBaseSet.HasDefaultDefined(dimNode.Id))
					{

						continue;
					}



					bool found = false;

					//no default and if we cannot find it in the cdiList then it is an error
					foreach (ContextDimensionInfo cdi in cdiList)
					{
						if (cdi.dimensionId.Equals(dimNode.Id))
						{
							found = true;
							break;
						}

					}

					if (!found)
					{
						errorString = TraceUtility.FormatStringResource("XBRLParser.Error.ElementMissingDimensionInContext", segmentScenarioText, elementId, dimNode.Id);
						//errorString = string.Format( "Error|xbrl.dimensions.PrimaryItemDimensionallyInvalidError:Element {1} is missing dimension {2} in its context {0} information", segmentScenarioText, elementId, dimId );

						return ValidationStatus.NotValid;

					}

				}

			}

			

            foreach (ContextDimensionInfo cdi in cdiList)
            {

                DimensionNode dn;
                if (this.DimensionsById.TryGetValue(cdi.dimensionId, out dn))
                {

                    if (dn.GetChild(cdi.Id) == null)
                    {
                        //this is an error condition if for isaLL
                        if (IsAll)
                        {
							errorString = TraceUtility.FormatStringResource( "XBRLParser.Error.InvalidDimensionMemberWithElement", cdi.Id, cdi.dimensionId, elementId );
							//errorString = string.Format( "Error|xbrl.dimensions.PrimaryItemDimensionallyInvalidError:Member {0} is not a valid member of the dimension {1} when used with element {2}",
							//    cdi.Id, cdi.dimensionId, elementId );

                            return ValidationStatus.NotValid;

                        }
                    }
                    else
                    {
                        //this is an error condition for the not all relationship
                        //as we do not want to find it...
                        if (!IsAll)
                        {
							errorString = TraceUtility.FormatStringResource( "XBRLParser.Error.ElementDoesNotAllowDimension", elementId, cdi.ToString(), segmentScenarioText );
							//errorString = string.Format( "Error|xbrl.dimensions.PrimaryItemDimensionallyInvalidError:Element {0} does not allow the use of Dimension {1} in its context", elementId, cdi.ToString(),
							//           segmentScenarioText);

                            return ValidationStatus.NotValid;

                        }
                    }

                }
               

            }


            return ValidationStatus.Valid;
        }

        private void GetInformationToValidate( ArrayList segments, ArrayList scenarios, 
            out List<ContextDimensionInfo> cdiList, ref bool hasUnSupportedData )
        {
            cdiList = new List<ContextDimensionInfo>();
            if( this.IsSegment )
            {
                foreach( Segment seg in segments )
                {
                   ContextDimensionInfo cdi =  seg.GetContextDimensionInfo();
                    if( cdi != null )
                    {
                        cdiList.Add( cdi );
                        if(! this.DimensionsById.ContainsKey( cdi.dimensionId ))
                        {
                            hasUnSupportedData = true;
                        }
                    }
                    else
                    {
                        hasUnSupportedData = true;
                    }
                }

            }
            else
            {
                 foreach( Scenario sce in scenarios )
                {
                   ContextDimensionInfo cdi =  sce.GetContextDimensionInfo();
                    if( cdi != null )
                    {
                        cdiList.Add( cdi );
                         if(! this.DimensionsById.ContainsKey( cdi.dimensionId ))
                        {
                            hasUnSupportedData = true;
                        }
                    }
                    else
                    {
                        hasUnSupportedData = true;
                    }
                }
            }
        }

       

       

        #endregion
    }
}

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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL dimension.
	/// </summary>
	[Serializable]
	public class Dimension : DocumentBase
	{
		private const string DLINK_KEY = "//link:definitionLink";
		private const string RREF_KEY = "//link:roleRef";

		private const string ROLE_TAG = "xlink:role";
		private const string TITLE_TAG = "xlink:title";
		private const string HREF_TAG = "xlink:href";
		private const string RURI_TAG = "roleURI";

		private const string HEADER_STR = "Definition";

		#region properties

		/// <summary>
		/// A collection of <see cref="RoleRef"/> objects for this <see cref="Dimension"/>, 
		/// indexed by URI.
		/// </summary>
		public Hashtable roleRefs = null;

		private Hashtable definitionLinks = new Hashtable();
		//key = dim id value is list of defaults as it could have a different def in diff base set
		//TODO: should have only one default oer dimension as default is accross base sets..
		public Dictionary<string, List<string>> DefaultMemberByDimension = new Dictionary<string, List<string>>();

		/// <summary>
		/// Collection of <see cref="DefinitionLink"/> objects related to this <see cref="Dimension"/>.
		/// </summary>
		/// <remarks>
		/// <key>title</key>
		/// <value><see cref="DefinitionLink"/> object</value>
		/// </remarks>
		public Hashtable DefinitionLinks
		{
			get { return definitionLinks; }
			set { definitionLinks = value; }
		}

		/// <summary>
		/// The base schema (URI + filename) underlying this <see cref="Dimension"/>.
		/// </summary>
		public string BaseSchema;
		private string MyHref;



        internal bool HasDimensionsWithoutDefault = false;

       

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Dimension"/>.
		/// </summary>
		public Dimension()
		{
		}

		#endregion

		public Dictionary<string, List<string>> GetRequiredElementRelationshipInfo()
		{
			//key = href , value = arraylist of hrefs...
			Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();

			foreach (DefinitionLink dl in this.definitionLinks.Values)
			{
				dl.GetRequiredElementRelationshipInfo(ret);
			}

			return ret.Count > 0 ? ret : null;

		}

		/// <summary>
		/// Use <see cref="ParseInternal( Dictionary{String, String}, out int)"/> rather 
		/// than this method.
		/// </summary>
		/// <param name="numErrors">Not used.</param>
		/// <exception cref="ApplicationException">Always thrown.</exception>
		protected override void ParseInternal(out int numErrors)
		{
			throw new ApplicationException("Please use the override which does schema discovery");
		}

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="Dimension"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="discoveredSchemas">The collection to which additional XML schemas associated with 
		/// locator links will be added.
		/// </param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		protected override void ParseInternal(Dictionary<string, string> discoveredSchemas, out int numErrors)
		{
			numErrors = 0;
			LoadRoleRefs(out numErrors);

			int errors = 0;
			LoadLinks( discoveredSchemas, out errors);

			numErrors += errors;
		}

		private bool HasRoleReferences()
		{
			return roleRefs != null;
		}

		private bool VerifyRoleReference(string xsdFile, string uri)
		{
			RoleRef rr = roleRefs[uri] as RoleRef;

			if (rr == null)
				return false;

			try
			{
				return string.Compare(xsdFile, rr.GetSchemaName(), true) == 0;
			}
			catch (AucentException)
			{
				return false;
			}
			catch (ArgumentNullException)
			{
				return false;
			}
		}

		private void LoadRoleRefs(out int numErrors)
		{
			numErrors = 0;

			XmlNodeList rolesList = theDocument.SelectNodes(RREF_KEY, theManager);

			if (rolesList == null )
			{
				return;
			}


		   
			foreach (XmlNode role in rolesList)
			{
				if (roleRefs == null)
				{
					roleRefs = new Hashtable();
				}

				string uri = string.Empty;
				string href = string.Empty;

				if (!Common.GetAttribute(role, HREF_TAG, ref href, errorList) ||
					!Common.GetAttribute(role, RURI_TAG, ref uri, errorList))
				{
					++numErrors;
					continue;
				}

				RoleRef rr = new RoleRef(href, uri);
				roleRefs[uri] = rr;
			}
		}


		private int LoadLinks(	Dictionary<string, string> discoveredSchemas, out int errorsEncountered)
		{
			errorsEncountered = 0;


			XmlNodeList dLinksList = theDocument.SelectNodes(DLINK_KEY, theManager);
			if (dLinksList == null )
			{
				//it is ok to have empty definition linkbase files..
				//Common.WriteWarning("XBRLParser.Error.LinkbaseDoesNotContainLinks", errorList, schemaFilename);
				return 0;
			}

			int counter = 0;

			foreach (XmlNode dlNode in dLinksList)
			{
				counter++;
				// first get the label
				string role = string.Empty;
				string title = string.Empty;

				// only role attribute is required
				if (!Common.GetAttribute(dlNode, ROLE_TAG, ref role, errorList))
				{
					++errorsEncountered;
					continue;
				}

				Common.GetAttribute(dlNode, TITLE_TAG, ref title, null);

				if (title == string.Empty)
				{
					title = role;
				}


				ProcessDimensionLinks(dlNode, role, title, ref errorsEncountered,  discoveredSchemas);


			}

			return counter;
		}

		private void ProcessDimensionLinks(XmlNode dlNode,
			string role, string title, ref int errorsEncountered, 
			Dictionary<string, string> discoveredSchemas)
		{
			DefinitionLink dl = definitionLinks[role] as DefinitionLink;

			if (dl == null)
			{
				// create the object
				dl = new DefinitionLink(title, role, BaseSchema, errorList);

				// put it in the hashtable
				definitionLinks[role] = dl;
			}

			// and load up the links
			int linkErrors = 0;
			dl.LoadChildren(dlNode, theManager,  discoveredSchemas, this.schemaPath, out linkErrors);

			errorsEncountered += linkErrors;
		}


		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		public override string ToXmlString()
		{
			FileInfo fi = new FileInfo(schemaFile);

			int len = 0;
			try
			{
				len = (int)fi.Length * 2;
			}
			catch (OverflowException)
			{
				len = (int)fi.Length;
			}

			StringBuilder text = new StringBuilder(len);
			ToXmlString(0, true, "en", text);

			return text.ToString();
		}

		/// <summary>
		/// Deprecated and non-functional.  Do not use.
		/// </summary>
		public override void ToXmlString(int numTabs, bool verbose, string language, StringBuilder xml)
		{
		}

		/// <summary>
		/// Merges the <see cref="DefinitionLinks"/> for a parameter-supplied <see cref="Dimension"/> 
		/// into the <see cref="DefinitionLinks"/> for this <see cref="Dimension"/>.
		/// </summary>
		/// <param name="child">The <see cref="Dimension"/> whose <see cref="DefinitionLinks"/> are to 
		/// be merged.</param>
		/// <param name="errors">Initialized by method to new <see cref="ArrayList"/>; however, not 
		/// otherwise updated.</param>
		public void MergeDimensionLinks(Dimension child, out ArrayList errors)
		{
			errors = new ArrayList();
			if (child == null || child.DefinitionLinks == null || child.DefinitionLinks.Count == 0)
				return;

			if (this.DefinitionLinks == null || this.DefinitionLinks.Count == 0)
			{
				this.DefinitionLinks = child.DefinitionLinks;
				return;
			}

			IDictionaryEnumerator enumer = child.DefinitionLinks.GetEnumerator();
			while (enumer.MoveNext())
			{
				if (this.DefinitionLinks.ContainsKey(enumer.Key))
				{
					((DefinitionLink)this.DefinitionLinks[enumer.Key]).Append(enumer.Value as DefinitionLink, errors);
				}
				else
				{
					// otherwise, add it
					this.DefinitionLinks[enumer.Key] = enumer.Value;
				}
			}
		}


		

		

		/// <summary>
		/// Binds all elements in a parameter-supplied collection to all 
		/// <see cref="DefinitionLocator"/> objects in the locators collection 
		/// for this <see cref="Dimension"/> which have the same element ID.
		/// </summary>
		/// <param name="allElements">The collection of elements to be bound.</param>
		public void BindElementsToLocator(Hashtable allElements)
		{
			Hashtable locatorsByHref = GetLocatorsByHref();

			foreach (Element e in allElements.Values)
			{
				ArrayList locators = locatorsByHref[e.Id] as ArrayList;
				if (locators != null)
				{
					foreach (DefinitionLocator dl in locators)
					{
						dl.MyElement = e;
					}
				}
			}
		}


	
		//        public void BuildPrimaryMeasureNodes(string currentLanguage, string currentLabelRole )
		//        {
		//            ArrayList nodeList = new ArrayList();
		//
		//
		//            foreach (DefinitionLink dl in this.dimensionLinks.Values)
		//            {
		//
		//
		//                dl.CreateMeasureNodes(currentLanguage, currentLabelRole, 
		//                    this, nodeList );
		//
		//               
		//
		//
		//            }
		//
		//            BuildPrimaryElementNodes(nodeList);
		//        }
		//
		//        private void BuildPrimaryElementNodes(ArrayList nodes)
		//        {
		//
		//              foreach (DimensionNode dn in nodes)
		//            {
		//
		//                this.primaryElementNodes[dn.Id] = dn;
		//                if (dn.Children != null)
		//                {
		//                    BuildPrimaryElementNodes(dn.Children);
		//                }
		//            }
		//
		//        }
		//
		private Hashtable GetLocatorsByHref()
		{
			Hashtable ret = new Hashtable();
			foreach (DefinitionLink dl in this.DefinitionLinks.Values)
			{
				dl.BuildLocatorHashbyHref(ret);
			}

			return ret;
		}

		/// <summary>
		/// Determines if the definition links collection for this <see cref="Dimension"/> 
		/// contains any dimension information.
		/// </summary>
		/// <param name="forSegment">If false, scenario-related information in each definition 
		/// link is check.</param>
		/// <returns>True if any one of the links as dimension information.</returns>
		public bool HasDimensionInfo(bool forSegment, bool commonOnly)
		{
			foreach (DefinitionLink dl in this.definitionLinks.Values)
			{
				if (dl.HasDimensionInfo(forSegment, commonOnly)) return true;
			}


			return false;
		}

		#region post parse data population

		/// <summary>
		/// capture information regarding dimensions without defaults etc...
		/// hypercubes that are setup with not all relationship
		/// hypercubes that are closed....
		/// </summary>
		internal void OnParseComplete()
		{
			foreach (DefinitionLink dl in this.definitionLinks.Values)
			{

				dl.OnParseComplete();
			}

			SetDefaultInformation();

		}

		#endregion

        #region clear Dimension node 
        /// <summary>
        /// once we prohibit some dimension info etc...
        /// we want to clear the dimensionnode cache so that it gets rebuild properly.
        /// 
        /// </summary>
        public void ClearDimensionNodeInfo()
        {
            foreach (DefinitionLink dl in this.definitionLinks.Values)
            {
                dl.HyperCubeNodesList.Clear();
                dl.ElementHypercubeRelationships = null;
            }
        }

        #endregion
        #region Get Dimension Nodes for Display


        /// <summary>
		/// Retrieves the segment and scenario dimension nodes for display.
		/// </summary>
		internal bool TryGetDimensionNodesForDisplay(string currentLanguage, string currentLabelRole,
			Hashtable presentationLinks, 
			bool isSegment, bool allDimensions,  Dictionary<string, RoleType> roleTypesDt,
			out Dictionary<string, DimensionNode> dimensionNodes)
		{
            dimensionNodes = new Dictionary<string, DimensionNode>();

			foreach (DefinitionLink dl in this.definitionLinks.Values)
			{
				string label = dl.Title;
				if (roleTypesDt != null)
				{
					RoleType rt;
					if (roleTypesDt.TryGetValue(label, out rt))
					{

						if (!string.IsNullOrEmpty(rt.Definition))
						{
							label = rt.Definition;
						}
					}
				}

				DimensionNode topLevelNode = new DimensionNode(label);
				topLevelNode.MyDefinitionLink = dl;
				if (isSegment)
				{
					foreach (string hypercubeHref in dl.HypercubeLocatorsHrefs)
					{
						if (!allDimensions)
						{
							if ( presentationLinks!= null &&  presentationLinks.Contains(dl.Role))
							{
								//if the hyper cube is part of the segment list or
								//if it part of the scenario list do not show it here 
								//as we are going to show it in the element pane....
								if (dl.segmentHypercubesHRef.Contains(hypercubeHref) ||
									dl.scenarioHypercubesHref.Contains(hypercubeHref)) continue;

							}
						}
                        DimensionNode hyperCubeNode;
                        if (dl.TryGetHypercubeNode(currentLanguage, currentLabelRole, this.definitionLinks,
                            hypercubeHref,false, out hyperCubeNode))
                        {
                            topLevelNode.AddChild(hyperCubeNode);

                        }
						

					}
				}
				else
				{
                    foreach (string hypercubeHref in dl.HypercubeLocatorsHrefs)
                    {
                        //we want to show all the scenario dimensions and 
                        //and all the  dimensions that are not associated with the segment

                        if (dl.scenarioHypercubesHref.Contains(hypercubeHref)
                            || !dl.segmentHypercubesHRef.Contains(hypercubeHref))
                        {

                            DimensionNode hyperCubeNode;
                            if (dl.TryGetHypercubeNode(currentLanguage, currentLabelRole, this.definitionLinks,
                                hypercubeHref,false, out hyperCubeNode))
                            {
                                topLevelNode.AddChild(hyperCubeNode);

                            }

                           
                        }

                    }
					
				}

                if (topLevelNode.Children != null && topLevelNode.Children.Count > 0)
                {
                    dimensionNodes.Add(dl.Role, topLevelNode);
                }


				
			}

			return true;
		}


		internal bool TryGetDimensionNodeForRole(string currentLanguage, string currentLabelRole,
			string roleId,	out DimensionNode  topLevelNode )
		{
			topLevelNode = null;
			DefinitionLink dl = definitionLinks[roleId] as DefinitionLink;

			if( dl != null )
			{

				string label = dl.Title;


				topLevelNode = new DimensionNode(label);
				topLevelNode.MyDefinitionLink = dl;
				if (dl.HypercubeLocatorsHrefs.Count > 0)
				{
					foreach (string hypercubeHref in dl.HypercubeLocatorsHrefs)
					{

						DimensionNode hyperCubeNode;
						if (dl.TryGetHypercubeNode(currentLanguage, currentLabelRole, this.definitionLinks,
							hypercubeHref,false, out hyperCubeNode))
						{
							topLevelNode.AddChild(hyperCubeNode);

						}


					}
				}
				else
				{
					//we might have to build the dimension nodes by itself...
					if (dl.DimensionLocatorsHrefs.Count > 0)
					{
						foreach (string dimHref in dl.DimensionLocatorsHrefs)
						{
							DefinitionLocator dloc;
							if (!dl.TryGetLocator(dimHref, out dloc)) return false;

						    DimensionNode dn  = dloc.CreateDimensionNode(currentLanguage, currentLabelRole,
							   null, dimHref, dl, true, definitionLinks, null, null, true, false);

							if (dn != null)
							{


								topLevelNode.AddChild(dn);



							}
						}
					}
				}
				
				

				



			}

			return topLevelNode != null;
		}

        internal bool TryGetMeasureDimensionNodesForRole(string currentLanguage, string currentLabelRole,
            string roleId, Taxonomy tax,   out DimensionNode topLevelNode)
        {
            topLevelNode = null;
            DefinitionLink dl = definitionLinks[roleId] as DefinitionLink;

            if (dl != null)
            {

                string label = dl.Title;


                topLevelNode = new DimensionNode(label);
                topLevelNode.MyDefinitionLink = dl;
                if (dl.HypercubeLocatorsHrefs.Count > 0)
                {
                    foreach (string hypercubeHref in dl.HypercubeLocatorsHrefs)
                    {
                        List<DimensionNode> dns ;
                       dl.BuildMeasureElementTreeForHypercubeId( tax, hypercubeHref, false, out dns);

                        if( dns != null)
                        {
                            foreach (DimensionNode dn in dns)
                            {
                                topLevelNode.AddChild(dn);
                            }
                        }


                    }
                }
                






            }

            return topLevelNode != null;
        }


        internal bool TryGetHypercubeNode(string currentLanguage, string currentLabelRole,
           string role,
            string hypercubeId,
            out DimensionNode hypercubeNode)
        {
            return TryGetHypercubeNode(currentLanguage, currentLabelRole,
                role, hypercubeId, false, out hypercubeNode);
        }
		



        internal bool TryGetHypercubeNode(string currentLanguage, string currentLabelRole,
           string role,    
            string hypercubeId,
            bool buildnew,
            out DimensionNode hypercubeNode)
        {
            hypercubeNode = null;
			DefinitionLink dl = definitionLinks[role] as DefinitionLink;

            if (dl != null)
            {
                if (dl.TryGetHypercubeNode(currentLanguage, currentLabelRole, this.definitionLinks,
                    hypercubeId,buildnew, out hypercubeNode))
                {
                    return true;
                }

                return false;

            }

            return false;

        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tax"></param>
		/// <param name="curNode"></param>
		/// <returns></returns>
		public DimensionNode GetMemberUsingTargetRole(Taxonomy tax,    DimensionNode curNode)
		{
			if (curNode.NodeDimensionInfo.TargetRole == null) return curNode;
			DimensionNode topLevelNode;
			if (TryGetDimensionNodeForRole(tax.CurrentLanguage,
				tax.CurrentLabelRole,
				curNode.NodeDimensionInfo.TargetRole,
				out topLevelNode))
			{

				Node ret =  topLevelNode.GetChildNode(curNode.parent.Id, curNode.Id) ;
				if (ret != null) return ret as DimensionNode;


			}
			


			return curNode;
		}

		public bool TryGetDefaultMember(string dimensionId, out string defaultMemberId)
		{
			defaultMemberId = null;
			List<string> tmp;
			if (DefaultMemberByDimension.TryGetValue(dimensionId, out tmp))
			{
				if (tmp.Count > 0)
				{
					defaultMemberId = tmp[0];
				}
			}

			return defaultMemberId != null;
		}

        internal void SetDefaultInformation()
        {



			
			foreach (DefinitionLink dl in this.definitionLinks.Values)
			{

				dl.SetDimensionDefaultMember(ref DefaultMemberByDimension);
			}
                
            

            if ( DefaultMemberByDimension.Count > 0)
            {

				foreach (DefinitionLink dl in this.definitionLinks.Values)
				{
					foreach (string dim in dl.DimensionLocatorsHrefs)
					{
						//if (dl.DimensionHrefsWithDefault.Contains(dim)) continue;

						List<string> defs;

						if (DefaultMemberByDimension.TryGetValue(dim, out defs))
						{
							foreach (string def in defs)
							{

								if (dl.TrySetDefaultMember(dim, def))
								{
									//this dim got the dfault from another base set.
									dl.DimensionHrefsWithDefault.Add(dim);
									break;
								}
							}
						}
					}

				}


            }
        }

		#endregion

        


        #region validations
       

        /// <summary>
        /// build the validation information so that we can apply it to markups...
        /// </summary>
        /// <param name="tax"></param>
        internal void BuildDimensionValidationInformation( Taxonomy tax)
        {
            if (tax.CurrentLanguage == null)
            {
                if (tax.SupportedLanguages.Count == 0)
                {
                    tax.CurrentLanguage = "en";
                }
                else
                {

                    tax.CurrentLanguage = tax.SupportedLanguages[0] as string;
                }
            }

            if (tax.currentLabelRole == null)
            {
                tax.currentLabelRole = PresentationLocator.preferredLabelRole;
            }


            foreach (DefinitionLink dl in this.definitionLinks.Values)
            {
                if (dl.ElementHypercubeRelationships != null) continue;

                dl.BuildDimensionValidationInformation(tax, definitionLinks);

                //determine if there are any dimensions without defaults..
                //if there are no dimensions without default then we 
                //need not validate any of the markups that does not have any
                //segment/ scenario information....
                if (!HasDimensionsWithoutDefault)
                {
                    HasDimensionsWithoutDefault = dl.HasDimensionWithoutDefault();
                }
            }
        }

        internal bool IsDimensionInformationValid(string id, ArrayList segments,
            ArrayList scenarios, out string error)
        {
            error = null;
            if (!HasDimensionsWithoutDefault)
            {
                if (segments.Count == 0 && scenarios.Count == 0)
                {
                    //there are no dimensions without default so not
                    //having any dimensions information in the markup cannot be invalid
                    return true;
                }
            }
            bool isValid = true;
            foreach (DefinitionLink dl in this.definitionLinks.Values)
            {
                string tmpError;
                //as long as it is valid in one base set , the markup ca be considered valid]
                ElementHypercubeRelationhipInfo.ValidationStatus retStatus
                = dl.GetMarkupValidationStatus(id, segments, scenarios, out tmpError);

                if (retStatus == ElementHypercubeRelationhipInfo.ValidationStatus.Valid)
                {
                    isValid = true;
                    return true;
                }
                else if (retStatus == ElementHypercubeRelationhipInfo.ValidationStatus.NotValid)
                {
                    error = tmpError;
                    isValid = false;
                }
            }

            return isValid;

        }


  
		#endregion

		internal Dimension CreateCopyForMerging()
		{
			Dimension copy = new Dimension();

			copy.BaseSchema = this.BaseSchema;
			copy.MyHref = this.MyHref;

			foreach (DictionaryEntry de in this.definitionLinks)
			{
				DefinitionLink dl = de.Value as DefinitionLink;

				copy.definitionLinks[de.Key] = dl.CreateCopyForMerging();
			}
			return copy;
        }

        #region Entry point 

        

		internal void  BuildSelectedCommonDimensionDictionary(List<string> commonRoles,
						Dictionary<string, DimensionNode> commonDimensionNodes,
						ref Dictionary<string, DefinitionLink> rolesByCommonDimension )
		{

			foreach (KeyValuePair<string, DimensionNode> kvp in commonDimensionNodes)
			{
				if (!commonRoles.Contains(kvp.Key)) continue;

				if (kvp.Value.children == null) continue;

				foreach (DimensionNode hypercubeNode in kvp.Value.children)
				{
					if (hypercubeNode.children == null) continue;

					foreach (DimensionNode dn in hypercubeNode.children)
					{
						if (dn.children == null) continue;

						foreach (DimensionNode dimDom in dn.children)
						{
							if (dimDom.children != null && dimDom.children.Count > 0)
							{
								//dimension domain has children members....
								DefinitionLink orig;
								if (rolesByCommonDimension.TryGetValue(dn.Id, out orig))
								{
									//we have the dimension in multiple linkbase
									//hardcoding to get the right us role..,.,
									if (dn.MyDefinitionLink.Role.Contains("http://xbrl.us/us-gaap/role/statement/CommonDomainMembers"))
									{
										rolesByCommonDimension[dn.Id] = dn.MyDefinitionLink;
									}
									else if (orig.Role.Contains("http://xbrl.us/us-gaap/role/statement/CommonDomainMembers"))
									{
										//essentially do nothing as we are setting back to the orig
										rolesByCommonDimension[dn.Id] = orig;
									}
									else
									{
										//found more than one common role... 
										//better update with the one that has more children 
										//so that we can get a more complete view...
										int origCount = 0;
										int curCount = 0;
										dimDom.GetChildrenCount(ref curCount);
										foreach (DimensionNode hyNode in orig.HyperCubeNodesList)
										{
											DimensionNode otherdimDom = hyNode.GetChildNode(dn.Id, dimDom.Id) as DimensionNode;
											if (otherdimDom != null)
											{
												otherdimDom.GetChildrenCount(ref origCount);
												break;
											}
										}
										
										// need to update only 
										//with the one that has more children...
										if (curCount > origCount)
										{
											rolesByCommonDimension[dn.Id] = dn.MyDefinitionLink;
										}

									}
								}
								else
								{
									rolesByCommonDimension[dn.Id] = dn.MyDefinitionLink;
								}
							}
						}

					}

				}
			}

	

		}



		internal bool DoesAnyOfTheSelectedRolesNeedTargetRole(List<string> selectedURIs,
			List<string> commonRoles,
			Dictionary<string, DefinitionLink> rolesByCommonDimension,
			Dictionary<string, DimensionNode> allDimensionNodes,
			ref List<Dimension.TargetDimensionInfo> targetExts,
			Taxonomy taxonomy)
		{
			

			foreach (string str in selectedURIs)
			{
				//if (commonRoles.Contains(str)) continue; //it is a common role.. no need to check for target role logic..

				DimensionNode titleNode;
				if (!allDimensionNodes.TryGetValue(str, out titleNode)) continue; //role doesnot have any dimension or is not part of this taxonomy

				if (titleNode.children == null) continue;

				foreach (DimensionNode hypercubeNode in titleNode.children)
				{
					if (hypercubeNode.children == null) continue;

					foreach (DimensionNode dn in hypercubeNode.children)
					{
						if (dn.children == null) continue;
						//check if this is a common dimension node...
						DefinitionLink targetRole;
						if (!rolesByCommonDimension.TryGetValue(dn.Id, out targetRole)) continue;

						if (!targetRole.Role.Equals(str))
						{
							foreach (DimensionNode dimDom in dn.children)
							{
								if (dimDom.children == null || dimDom.children.Count == 0)
								{
									//found a common dimension that does not have any children defined...
									TargetDimensionInfo tdi = new TargetDimensionInfo();
									tdi.DimensionNode = dn;
									tdi.MemberNode = dimDom;
									tdi.TargetRole = targetRole;
									tdi.Taxonomy = taxonomy;
									targetExts.Add(tdi);
								}
							}

						}
					}
				}


			}

			return targetExts.Count > 0;

		}

        /// <summary>
        /// informaiton required to create the  target role arc from dimension to its initial domain member..
        /// </summary>
        public class TargetDimensionInfo
        {
            /// <summary>
            /// elemet id of the dimension
            /// </summary>
            public DimensionNode DimensionNode;
            /// <summary>
            /// element id of the member
            /// </summary>
            public DimensionNode MemberNode;
            /// <summary>
            /// target role to use for continuing the dimension..
            /// </summary>
            public DefinitionLink  TargetRole;

            /// <summary>
            /// taxonomy used...
            /// </summary>
            public Taxonomy Taxonomy;


            #region public methods
            /// <summary>
            /// string display of information
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0}->{1} for {2}:{3}", DimensionNode.MyDefinitionLink.Role, TargetRole.Role, DimensionNode.Id, MemberNode.Id);
            }
            #endregion

        }
 
        #endregion
    }
}
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
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// interface that determines whether a item can be member of the tupleset
    /// </summary>
    public interface ITupleSetChild
    {
        /// <summary>
        /// Element Id of the tuple child 
        /// </summary>
        /// <returns></returns>
        string GetId();
    }
    /// <summary>
    /// Tuple set information that is read / written into instance document
    /// </summary>
    public class TupleSet : IComparable<TupleSet>, ITupleSetChild
    {
        #region Ctors
        /// <summary>
        /// Constructs a new instance of <see cref="TupleSet"/>.
        /// </summary>
        public TupleSet()
        {
        }

        /// <summary>
        /// Overloaded.  Constructs a new instance of <see cref="TupleSet"/>.
        /// </summary>
        public TupleSet(string name)
        {
            Name = name;
        }


        /// <summary>
        /// Overloaded.  Constructs a new instance of <see cref="TupleSet"/>.
        /// </summary>
        public TupleSet(string name, string taxonomyNamespace )
        {
            Name = name;
            this.TaxonomyNamespace = taxonomyNamespace;
        }
        #endregion

        #region Properties


        /// <summary>
        /// TupleSet name.
        /// </summary>
        public string Name;


        /// <summary>
        /// This property is used to ensure that a tuple set that has elements from one taxonomy
        /// does not also have elements from a different taxonomy (within the same workbook)
        /// </summary>
        public string TaxonomyNamespace = null;

        /// <summary>
        /// Element Id of the immediate parent tuple.
        /// </summary>
        public string TupleParentElementId;

        /// <summary>
        /// parent Set
        /// </summary>
        public TupleSet ParentSet;



        /// <summary>
        /// sorted list of the children members...
        /// members could be MarkupProperty , TupleSet or Nodes...
        /// </summary>
        public SortedList<float, ITupleSetChild> Children = new SortedList<float, ITupleSetChild>();




        #endregion

        #region public methods

		/// <summary>
		/// All markup elemenets need the top level tuple set specified.
		/// All tupleset names should be unique. I.e. even an inner set should not have the same 
		/// name as the top level set.
		/// </summary>
		/// <param name="rootSet"></param>
		/// <param name="usedNames"></param>
		public void MakeTupleSetInlineXBRLSerializable(TupleSet rootSet, ref List<string> usedNames)
		{
			#pragma warning disable 0219
			string newName = this.Name;
			#pragma warning restore 0219

			int counter = 0;
			while (usedNames.Contains(this.Name))
			{
				counter++;
				this.Name = Name + "_inline" + counter.ToString();
			}

			usedNames.Add(this.Name);
			foreach (ITupleSetChild child in this.Children.Values)
			{
				if (child is MarkupProperty)
				{
					(child as MarkupProperty).TupleSetName = this.Name;
					(child as MarkupProperty).TopLevelTupleset = rootSet;

				}
				else if (child is TupleSet)
				{
					(child as TupleSet).MakeTupleSetInlineXBRLSerializable(rootSet, ref usedNames);
				}
			}

		}

        /// <summary>
        /// Id of the element
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            return TupleParentElementId; 
        }
        /// <summary>
        /// Create a new instance document tuple set
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pSet"></param>
        /// <param name="parentElementId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static TupleSet CreateInstanceDocumentTupleSet(string name,
            TupleSet pSet,
            string parentElementId, float order)
        {
            TupleSet ret = new TupleSet();
            ret.Name = name;
            ret.ParentSet = pSet;
            ret.TupleParentElementId = parentElementId;
            if (ret.ParentSet != null)
            {

                ret.ParentSet.Children.Add(order, ret);
            }

            return ret;
        }

        /// <summary>
        /// Get the element id of the top level tuple parent....
        /// </summary>
        /// <returns></returns>
        public string RecursivelyGetTopLevelTupleParentId()
        {
            if (ParentSet == null) return this.TupleParentElementId;

            return ParentSet.RecursivelyGetTopLevelTupleParentId();
        }
        /// <summary>
        /// Get the top most tuple set..
        /// </summary>
        /// <returns></returns>
        public TupleSet GetTopLevelTupleSet()
        {
            if (ParentSet == null) return this;

            return ParentSet.GetTopLevelTupleSet();

        }
        /// <summary>
        /// List of tuple parents for this markup item
        /// item zero is the immediate parent .....
        /// </summary>
        /// <param name="list"></param>
        public void GetTupleParentList(ref List<string> list)
        {
            list.Add(this.TupleParentElementId);
            if (this.ParentSet != null)
            {
                ParentSet.GetTupleParentList(ref list);
            }
        }



        /// <summary>
        /// Adds a child to the list in sequence..
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(ITupleSetChild child)
        {

            float order = 1;

            // not found, add it to the end
            order *= (this.Children.Count + 1);

            
            Children.Add(order, child);

            if (child is TupleSet)
            {
                (child as TupleSet).ParentSet = this;
            }
        }

        /// <summary>
        /// check if it contains the node...
        /// </summary>
        /// <param name="node"></param>
        public bool ContainsNode(ITupleSetChild node)
        {
            return Children.IndexOfValue(node) >= 0 ? true : false;
        }

      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allMarkups"></param>
        public void GetAllMarkedupElements(ref List<MarkupProperty> allMarkups)
        {
            foreach (ITupleSetChild child in this.Children.Values)
            {
                if (child is MarkupProperty)
                {
                    allMarkups.Add(child as MarkupProperty);
                }
                else if (child is TupleSet)
                {
                    (child as TupleSet).GetAllMarkedupElements(ref allMarkups);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="allTSs"></param>
        private void GetAllTupleSets(ref List<TupleSet> allTSs)
        {
            foreach (ITupleSetChild child in this.Children.Values)
            {
                if (child is TupleSet )
                {
                    allTSs.Add(child as TupleSet);
                    (child as TupleSet).GetAllTupleSets(ref allTSs);
                }
               
            }
        }

        /// <summary>
        /// Recursively build all the lcoators for this tuple set
        /// </summary>
        /// <param name="locators"></param>
        public void RecursivelyBuildLocators(ref List<TupleSetLocator> locators)
        {
            TupleSetLocator loc = new TupleSetLocator();
            loc.Name = this.Name;
            this.GetTupleParentList(ref loc.TupleParentList);
            loc.TupleSet = this;
            locators.Add(loc);

            foreach (ITupleSetChild child in this.Children.Values)
            {
                if (child is TupleSet)
                {
                    (child as TupleSet).RecursivelyBuildLocators(ref locators);
                }
            }
        }

        /// <summary>
        /// Add achild element to the current tupleset
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parentId"></param>
        /// <param name="taxonomies"></param>
        public void Add(ITupleSetChild child, string parentId,  Taxonomy[] taxonomies)
        {
            Element ele = null;
            foreach( Taxonomy tax in taxonomies)
            {
                ele = tax.AllElements[parentId] as Element;
                if (ele != null) break;
            }

            if (ele != null)
            {
                double order = ele.GetTupleChildOrder(child.GetId());
                if (order >= 0)
                {
					while (Children.ContainsKey((float)order))
					{
						order = order + 0.005;
					}
                    this.Children.Add((float)order, child);
                    return;
                }
            }

            this.AddChild(child);
        }

        /// <summary>
        /// Get the top level tuple set that has the same name as this set
        /// it might still be an inner set but the tuplesetname got used starting at this level
        /// </summary>
        /// <returns></returns>
        public TupleSet GetTopLevelTupleSetWithSameName()
        {
            if (this.ParentSet == null) return this;

            if (this.ParentSet.Name.Equals(this.Name)) return this.ParentSet.GetTopLevelTupleSetWithSameName();

            return this;
        }

        /// <summary>
        /// we want to remove all empty children sets before we 
        /// create instance document wih tuple sets...
        /// </summary>
        public void RemoveEmptyChildrenSets()
        {
            List<float> keys = new List<float>( this.Children.Keys);
            foreach (float key in keys )
            {

                ITupleSetChild child;
                if (Children.TryGetValue(key, out child))
                {
                    if (child is TupleSet)
                    {
                        TupleSet childSet = child as TupleSet;
                        childSet.RemoveEmptyChildrenSets();

                        if (childSet.Children.Count == 0)
                        {
                            //no data members in this set so it can be removed.
                            this.Children.Remove(key);
                        }


                    }
                }
            }
        }

        /// <summary>
        /// Get the set / child set that has the name setName
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        public TupleSet GetTupleSet(string setName )
        {
            if (this.Name.Equals(setName)) return this;

            foreach (ITupleSetChild child in this.Children.Values)
            {
                TupleSet childSet = child as TupleSet;
                if (childSet != null)
                {
                    TupleSet ret = childSet.GetTupleSet(setName);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// when we read tuple from instance document
        /// we get a colon that separates prefix from the name
        /// we need it to be an underscore to make it work with the element id
        /// </summary>
        public void ConvertColonToUnderScore()
        {
            this.TupleParentElementId = this.TupleParentElementId.Replace(":", "_");
            foreach (ITupleSetChild child in this.Children.Values)
            {
                TupleSet childSet = child as TupleSet;
                if (childSet != null)
                {
                    childSet.ConvertColonToUnderScore();

                }
                else
                {

                    MarkupProperty mp = child as MarkupProperty;
                    if (mp != null && mp.TupleParentList != null )
                    {
                        for (int i = 0; i < mp.TupleParentList.Count; i++)
                        {
                            mp.TupleParentList[i] = mp.TupleParentList[i].Replace(":", "_");
                        }
                    }
                }
            }
        }



		internal void UpdateMarkupsWithTupleSetInformation(TupleSet root, List<string> parentList)
		{
			List<string> totalParentList = new List<string>();
			totalParentList.Add(this.TupleParentElementId);
			totalParentList.AddRange(parentList);
			foreach (ITupleSetChild child in this.Children.Values)
			{
				if (child is MarkupProperty)
				{
					MarkupProperty mp = child as MarkupProperty;
					mp.TopLevelTupleset = root;
					mp.TupleParentList = totalParentList;
					mp.TupleSetName = this.Name;
				}
				else if (child is TupleSet)
				{
					(child as TupleSet).UpdateMarkupsWithTupleSetInformation(root, totalParentList);
				}
			}
		}
     
        #endregion



        #region IComparable<TupleSet> Members
        /// <summary>
        /// compare based on name and Id
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TupleSet other)
        {
            int ret = this.Name.CompareTo(other.Name);
            if (ret != 0) return ret;

            return this.TupleParentElementId.CompareTo(other.TupleParentElementId);
        }

        #endregion


        #region Create Set from XML

        public static bool TryCreateFromXml(XmlNode tupleNode, ArrayList contexts, ArrayList units,
        out TupleSet ts, out ArrayList markups, ref ArrayList errors)
        {
            markups = new ArrayList();
            //create a new tupleset
            ts = CreateInstanceDocumentTupleSet(tupleNode);


            //check for a tuple parent with no children
            if (ts == null)
            {
                return false;
            }



            bool result = RecursivelyBuildTupleFromXML(tupleNode, contexts, units, ts, ref markups, ref errors
                 );
            return result;
        }


        internal static bool RecursivelyBuildTupleFromXML(XmlNode node, ArrayList contexts, ArrayList units,
            TupleSet parentTupleSet,
            ref ArrayList markups, ref ArrayList errors)
        {
            for (int i = 0; i < node.ChildNodes.Count; ++i)
            {
                XmlNode child = node.ChildNodes[i];
                if (child is XmlElement)
                {
                    if (child.FirstChild == null ||
                        (!(child.FirstChild is XmlComment) && child.FirstChild.Value != null))
                    {
                        //child is a markup
                        MarkupProperty mp = null;
                        if (!MarkupProperty.TryCreateFromXml(i, node.ChildNodes, contexts, units,
                            out mp, ref errors))
                        {
							continue;
                        }



                        mp.TupleSetName = parentTupleSet.Name;
                        mp.xmlElement = child;
                        mp.TupleParentList = new List<string>();
                        parentTupleSet.GetTupleParentList(ref mp.TupleParentList);


                        markups.Add(mp);
                        parentTupleSet.AddChild(mp);
                    }
                    else
                    {

                        TupleSet childTupleSet = CreateInstanceDocumentTupleSet(child);
                        if (childTupleSet != null)
                        {
                            childTupleSet.ParentSet = parentTupleSet;
                            parentTupleSet.AddChild(childTupleSet);

                            if (!RecursivelyBuildTupleFromXML(child, contexts, units, childTupleSet, ref markups, ref errors))
                            {
                                return false;
                            }
                        }


                    }
                }
            }

            return true;
        }


        internal static TupleSet CreateInstanceDocumentTupleSet(XmlNode tupleNode)
        {
            TupleSet ts = new TupleSet();
            ts.TupleParentElementId = tupleNode.Name;


            //check for a tuple parent with no children
            if (tupleNode.FirstChild == null)
            {
                return null;
            }

            // set a tupleset name
            if (tupleNode.FirstChild.Value != null && tupleNode.OuterXml.IndexOf("Tuple Set Name") != -1)
            {
                // there's a comment containing the tupleset name (xml was created by Rivet)
                string tsNameComment = tupleNode.FirstChild.Value;
                string[] commentSplit = tsNameComment.Split(':');
                ts.Name = commentSplit[1].Trim();
            }
            else
            {
                //no tupleset name in the xml, so create a unique string
                ts.Name = Guid.NewGuid().ToString();
            }

            return ts;
        }

        #endregion

        #region build TupleSetLocators
        /// <summary>
        /// Build locators for all the tuplesets....
        /// </summary>
        /// <param name="tupleSets"></param>
        /// <returns></returns>
        public static List<TupleSetLocator> BuildTupleSetLocators(TupleSet[] tupleSets)
        {
            if (tupleSets == null) return null;
            List<TupleSetLocator> ret = new List<TupleSetLocator>();

            foreach (TupleSet set in tupleSets)
            {
                set.RecursivelyBuildLocators(ref ret);
            }

            ret.Sort();
            return ret;
        }
        #endregion

		#region Fix Prefix

		internal void FixPrefix(Dictionary<string, string> prefixMap)
		{
			foreach (KeyValuePair<string, string> kvp in prefixMap)
			{
				if (this.TupleParentElementId.StartsWith(kvp.Key))
				{
					TupleParentElementId = kvp.Value + TupleParentElementId.Substring(kvp.Key.Length);
					break;
				}
			}

			foreach (ITupleSetChild child in this.Children.Values)
			{
				TupleSet childSet = child as TupleSet;
				if (childSet != null)
				{
					childSet.FixPrefix(prefixMap);

				}
				else
				{

					MarkupProperty mp = child as MarkupProperty;
					if (mp != null && mp.TupleParentList != null)
					{
						for (int i = 0; i < mp.TupleParentList.Count; i++)
						{
							foreach (KeyValuePair<string, string> kvp in prefixMap)
							{
								if (mp.TupleParentList[i].StartsWith(kvp.Key))
								{
									mp.TupleParentList[i] = kvp.Value + mp.TupleParentList[i].Substring(kvp.Key.Length);
									break;
								}
							}
						}
					}
				}
			}
		}
		#endregion
	}


    /// <summary>
    /// Used to locate a tuple set given the name and the TupleParentList information
    /// Used to associate Markup Property to TupleSet
    /// </summary>
    public class TupleSetLocator : IComparable<TupleSetLocator>
    {
        /// <summary>
        /// TupleSet name.
        /// </summary>
        public string Name = string.Empty;



        /// <summary>
        /// List of tuple parents for this markup item
        /// item zero is the immediate parent .....
        /// </summary>
        public List<string> TupleParentList = new List<string>();



        /// <summary>
        /// Tupe set that has the name ans the set of TupleParentList 
        /// </summary>
        public TupleSet TupleSet;


        #region IComparable<TupleSetLocator> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TupleSetLocator other)
        {
            int ret = this.Name.CompareTo(other.Name);
            if (ret != 0) return ret;

            ret = TupleParentList.Count.CompareTo(other.TupleParentList.Count);
            if (ret != 0) return ret;

            for (int i = 0; i < this.TupleParentList.Count; i++)
            {
                ret = TupleParentList[i].CompareTo(other.TupleParentList[i]);
                if (ret != 0) return ret;

            }

            return 0;
        }

        #endregion
    }
}
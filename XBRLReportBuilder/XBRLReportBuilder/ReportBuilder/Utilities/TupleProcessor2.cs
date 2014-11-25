//=============================================================================
// TupleProcessor2 (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// This is the utility class that process tuple-based line items.
//=============================================================================
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder.Utilities
{
    public class TupleProcessor2
    {
        
		protected List<TupleSet> tupleSets;
        protected Hashtable keyMapName;

        protected ArrayList viewTables;
        protected ArrayList colsWithData;

        protected int level;

        protected List<TupleMarkup> regularTupleMarkups;
        protected List<TupleMarkup> nestedTupleMarkups;
        public List<string> UsedTupleSetNames;

        public Dictionary<string, List<string>> AssociatedTupleNames;

        public TupleProcessor2()
        {
            regularTupleMarkups = new List<TupleMarkup>();
            nestedTupleMarkups = new List<TupleMarkup>();

            AssociatedTupleNames = new Dictionary<string, List<string>>();

            UsedTupleSetNames = new List<string>();
        }

		public void SetItems(ArrayList viewTables, ArrayList colsWithData, List<TupleSet> instanceTupleSets,
			List<string> topLevelTupleSetNames, Hashtable keyMapName)
		{
			this.viewTables = viewTables;
			this.colsWithData = colsWithData;
			tupleSets = new List<TupleSet>(instanceTupleSets.Count);
			foreach (TupleSet ts in instanceTupleSets)
			{
				if (topLevelTupleSetNames.Contains(ts.Name))
				{
					tupleSets.Add(ts);
				}
			}
			this.keyMapName = keyMapName;

			regularTupleMarkups.Clear();
			nestedTupleMarkups.Clear();
			AssociatedTupleNames.Clear();
			UsedTupleSetNames.Clear();
		}


        public void Process()
        {
            try
            {
                ProcessTuples();
            }
            catch (Exception e)
            {
#if DEBUG 
                throw e;
#else
                Console.WriteLine("Tuple Processing FAILED!");
                Console.WriteLine(e.Message + " " + e.StackTrace);
#endif
            }
        }

        public TupleMarkup GetMatchingTupleMarkup(DataRow instRow, string tupleName)
        {
            TupleMarkup temp = new TupleMarkup((string)instRow[InstanceUtils._ElementIDCol],
            (string)instRow[InstanceUtils._parentIdCol], tupleName);

            if ((bool)instRow[InstanceUtils._IsNestedTuple])
            {
                int index2 = nestedTupleMarkups.BinarySearch(temp);
                if (index2 >= 0)
                {
                    return nestedTupleMarkups[index2];
                }

                return null;
            }

            // this gives us the first match in the list, which may or may not have already been consumed
            int index = regularTupleMarkups.BinarySearch(temp, new ElementParentComparer());

            if (index >= 0)
            {
                return regularTupleMarkups[index];
            }

            return null;
        }

        public void AddUsedTupleNames(List<string> usedNames)
        {
            foreach (string name in usedNames)
            {
                int index = UsedTupleSetNames.BinarySearch(name);
                if (index < 0)
                {
                    UsedTupleSetNames.Insert(~index, name);
                }
            }
        }

        public bool AlreadyProcessedTuple(string tupleName)
        {
            int index = UsedTupleSetNames.BinarySearch(tupleName);

            return index >= 0;
        }

		protected void ProcessTuples()
		{
			for (int i = 0; i < tupleSets.Count; ++i)
			{
				ProcessTupleset(tupleSets[i]);
			}
		}

		private void RecursivelySetAssociatedTupleSetnames(TupleSet ts)
		{
			if (ts.ParentSet != null)
			{


				string parentName = ts.ParentSet.Name;

				if (!parentName.Equals(ts.Name))
				{

					List<string> association;
					if (!AssociatedTupleNames.TryGetValue(parentName, out association))
					{
						association = new List<string>();
						AssociatedTupleNames[parentName] = association;
					}

					if (!association.Contains(ts.Name))
					{
						association.Add(ts.Name);
					}
				}

			}

			foreach (ITupleSetChild child in ts.Children.Values)
			{
				TupleSet childSet = child as TupleSet;
				if (childSet != null)
				{
					RecursivelySetAssociatedTupleSetnames(childSet);
				}
			}


		}

        protected void ProcessTupleset(TupleSet set)
        {
			RecursivelySetAssociatedTupleSetnames(set);
			List<MarkupProperty> markedupChildren = new List<MarkupProperty>();
			set.GetAllMarkedupElements(ref markedupChildren);


			foreach (MarkupProperty mp in markedupChildren)
			{


				string keyNameString = keyMapName[InstanceUtils.BuildMergedContextKeyNameStringFromMarkupProperty(mp.contextRef, false)] as string;

				DataRow row = GetInstanceRow(mp);

                if (row != null)
                {

                    string markupData = InstanceUtils.GetMassagedMarkup(mp.markupData);


                    if ((bool)row[InstanceUtils._IsNestedTuple])
                    {
                        InsertNestedTuple(row, mp.TupleSetName, keyNameString, markupData, level);
                    }
                    else
                    {
                        InsertRegularTuple(row, mp.TupleSetName, keyNameString, markupData, level);
                    }

                    if (!colsWithData.Contains(keyNameString))
                    {
                        colsWithData.Add(keyNameString);
                    }

                    CascadeHasData(row, mp.TupleSetName);
                }
                
            }
        }

        protected void InsertNestedTuple(DataRow instRow, string tupleName, string colName, string markupData, int level)
        {
            TupleMarkup tm = new TupleMarkup((string)instRow[InstanceUtils._ElementIDCol],
                (string)instRow[InstanceUtils._parentIdCol], tupleName, level);

            int index = nestedTupleMarkups.BinarySearch(tm);
            if (index < 0)
            {
                tm.AddMarkup(colName, markupData, tupleName);
                tm.InstanceRow = instRow;
                nestedTupleMarkups.Insert(~index, tm);
            }
            else
            {
                // this probably won't be hit
                nestedTupleMarkups[index].AddMarkup(colName, markupData, tupleName);
                nestedTupleMarkups[index].TupleSetNames.Add(tupleName);
            }
        }

        /// <summary>
        /// We're making an assumption that the tuples given to us are correct - that a tupleset can't contain
        /// multiple elements with the same context.
        /// </summary>
        protected void InsertRegularTuple(DataRow instRow, string tupleName, string colName, string markupData, int level)
        {
            TupleMarkup tm = new TupleMarkup((string)instRow[InstanceUtils._ElementIDCol],
                (string)instRow[InstanceUtils._parentIdCol], tupleName, level);

            int index = regularTupleMarkups.BinarySearch(tm, new ElementParentComparer());
            if (index < 0)
            {
                tm.AddMarkup(colName, markupData, tupleName);
                tm.InstanceRow = instRow;
                regularTupleMarkups.Insert(~index, tm);
            }
            else
            {
                regularTupleMarkups[index].AddMarkup(colName, markupData, tupleName);
                regularTupleMarkups[index].TupleSetNames.Add(tupleName);
            }
        }

        protected DataRow GetInstanceRow(MarkupProperty mp)
        {
			string pId = mp.TupleParentList[0].Replace(":", "_");

            return GetInstanceRow(mp.elementId, pId, viewTables.Count - 1);
        }

        protected DataRow GetInstanceRow(string elementId, string parentId, int levelToStart)
        {
            string where = PrepareStringForWhereClause(InstanceUtils._RealElementIdCol, elementId, false);
//#if DEBUG
//            Console.WriteLine("GetRow: where=" + where);
//#endif

            DataRow[] dRows = null;

            for (level = levelToStart; level > 0; --level)
            {
                try
                {
                    dRows = (viewTables[level] as DataTable).Select(where);
                    foreach (DataRow row in dRows)
                    {
                        if (row[InstanceUtils._parentIdCol].Equals(parentId))
                        {
                            return row;
                        }
                    }

                    if (dRows.Length > 0)
                    {
                        // found an element match, but parent doesn't match
                        // so work up the list looking for a parent that does match
                        foreach (DataRow rowAgain in dRows)
                        {
                            int curLevel = level;
                            DataRow r = GetInstanceRow( (string)rowAgain[InstanceUtils._parentIdCol], parentId, level-1 );
                            if ( r != null )
                            {
                                return r;
                            }
                            level = curLevel;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Line 1: " + where + " " + e.Message + " " + e.StackTrace);
                    return null;
                }
            }

            return null;
        }

        protected string PrepareStringForWhereClause(string columnName, string originalString, bool useWildcard)
        {
            //replace apostrophe with a double apostrophe
            string firstTempString = originalString.Replace(@"'", @"''").Replace(":", "_");

            //replace % with ? (% breaks LIKE functionality)
            string tempString = firstTempString.Replace(@"%", @"?");

            string whereTemp = string.Empty;
            string where = columnName;
            if (useWildcard)
                where = where + " LIKE '" + tempString + "*'";
            else
                where = where + " = '" + tempString + "'";

            int openBracket = where.IndexOf("[");
            if (openBracket > -1)
            {
                whereTemp = where.Insert(openBracket + 1, "[]");
            }
            int closeBracket = whereTemp.LastIndexOf("]");
            if (closeBracket > -1)
            {
                where = whereTemp.Insert(closeBracket, "[]");
            }

            return where;
        }

        protected void CascadeHasData(DataRow row, string tupleSetName )
        {
            row[InstanceUtils._hasDataCol] = InstanceUtils._HasData_True;

            AddTupleSetNameToRow(row, tupleSetName);

            string parentName2 = (string)row[InstanceUtils._parentNameCol];

            for (int i = level - 1; i >= 0; i--)
            {
                //replace any apostrophes with double apostrophes so the select runs
                string hasDataWhere = PrepareStringForWhereClause(InstanceUtils._myNameCol, parentName2, false);
                DataRow[] hasDataRows = null;
                try
                {
                    hasDataRows = (viewTables[i] as DataTable).Select(hasDataWhere);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Line 4: " + hasDataWhere + " " + e.Message + " " + e.StackTrace);
                    continue;
                }

                if (hasDataRows != null && hasDataRows.Length > 0)
                {
                    DataRow rowToUpdate = hasDataRows[0];
                    rowToUpdate[InstanceUtils._hasDataCol] = InstanceUtils._HasData_True;
                    parentName2 = (string)rowToUpdate[InstanceUtils._parentNameCol];

                    // make sure the parent has the tuple name also - but only the parent
                    if (i == level - 1)
                    {
                        AddTupleSetNameToRow(rowToUpdate, tupleSetName);
                    }
                }
            }
        }

        protected void AddTupleSetNameToRow(DataRow row, string tupleSetName)
        {
            if (row[InstanceUtils._TupleSetName] == DBNull.Value)
            {
                row[InstanceUtils._TupleSetName] = tupleSetName;
            }
            else
            {
                string currentSets = (string)row[InstanceUtils._TupleSetName];
                if (!currentSets.Contains(tupleSetName))
                {
                    StringBuilder val = new StringBuilder(currentSets).Append(';').Append(tupleSetName);
                    row[InstanceUtils._TupleSetName] = val.ToString();
                }
            }
        }
    }

    public class ElementParentComparer : IComparer<TupleMarkup>
    {
        public ElementParentComparer()
        {
        }

        #region IComparer<TupleMarkup> Members

        int IComparer<TupleMarkup>.Compare(TupleMarkup x, TupleMarkup y)
        {
            return x.ElementParentCompareTo(y);
        }

        #endregion
    }

    public class TupleMarkup : IComparable<TupleMarkup>
    {
        struct ColKey
        {
            public int dupId;
            public string colName;

            public ColKey(string colNameArg)
            {
                dupId = 0;
                colName = colNameArg;
            }
        }

        struct MarkupValue
        {
            public string tuplesetName;
            public string data;

            public MarkupValue(string name, string dataArg)
            {
                tuplesetName = name;
                data = dataArg;
            }
        }

        public string ElementId;
        public string ParentId;
        public int Depth;
        public int NumDupes;
        public int DupesWritten;

        public bool HasBeenWritten
        {
            get { return DupesWritten > NumDupes; }
        }

        Dictionary<ColKey,MarkupValue> markups;

        public List<string> TupleSetNames;

        public DataRow InstanceRow;

        public TupleMarkup()
        {
 
            Depth = 0;
            NumDupes = 0;
            DupesWritten = 0;

            markups = new Dictionary<ColKey, MarkupValue>();
            TupleSetNames = new List<string>();
       }

        public TupleMarkup(string elementId, string parentId, string tupleName) : this()
        {
            ElementId = elementId;
            ParentId = parentId;
            TupleSetNames.Add(tupleName);
        }

        public TupleMarkup( string elementId, string parentId, string tupleName, int depth) : 
            this( elementId, parentId, tupleName )
        {
            Depth = depth;
        }

        public bool TryGetMarkups( out Dictionary<string, string> someMarkups, out List<string> someTupleNames)
        {
            someMarkups = null;
            someTupleNames = null;

            if (DupesWritten > NumDupes)
            {
                return false;
            }

            someMarkups = new Dictionary<string, string>();
            someTupleNames = new List<string>();

            foreach ( KeyValuePair<ColKey, MarkupValue> kvp in markups )
            {
				
                if (kvp.Key.dupId == DupesWritten)
                {
					if (kvp.Key.colName != null)
					{
						someMarkups.Add(kvp.Key.colName, kvp.Value.data);
					}
                    someTupleNames.Add(kvp.Value.tuplesetName);
                }
            }

            ++DupesWritten;

			return true;
        }

        public void AddMarkup(string colName, string markup, string tupleName)
        {
            ColKey ck = new ColKey( colName );

            while (markups.ContainsKey(ck))
            {
                ++ck.dupId;
            }

            if (ck.dupId > NumDupes)
            {
                NumDupes = ck.dupId;
            }

            markups.Add(ck, new MarkupValue( tupleName, markup) );
        }

        public int ElementParentCompareTo(TupleMarkup other)
        {
            int elementCompare = ElementId.CompareTo(other.ElementId);
            if (elementCompare != 0)
            {
                return elementCompare;
            }

            int parentCompare = ParentId.CompareTo(other.ParentId);
            if (parentCompare != 0)
            {
                return parentCompare;
            }

            return 0;
        }

        public int CompareTo(TupleMarkup other)
        {
            int elem_parentCompare = ElementParentCompareTo(other);
            if (elem_parentCompare != 0)
            {
                return elem_parentCompare;
            }

            return TupleSetNames[0].CompareTo( other.TupleSetNames[0]);
        }

        #region IComparable<TupleMarkup> Members

        int IComparable<TupleMarkup>.CompareTo(TupleMarkup other)
        {
            return CompareTo(other);
        }

        #endregion
    }
}

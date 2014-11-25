using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// TupleSetNew
    /// </summary>
    public class TupleSetNew
    {
        #region Properties

        /// <summary>
        /// TupleSet name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Element Id of the immediate parent tuple.
        /// </summary>
        public string TupleParentElementId;

        /// <summary>
        /// parent Set
        /// </summary>
        public TupleSetNew ParentSet;


        /// <summary>
        /// List of all the children sets....
        /// children set might also have the same name as the parent set
        /// the only difference is the children set would have a link to the parent set..
        /// </summary>
        public List<TupleSetNew> ChildrenSets;


        /// <summary>
        /// List of elements that have been marked up for this tupleset
        /// </summary>
        public List<MarkedUpElement> markedupElements;

        /// <summary>
        /// Elements used inside this tuple set
        /// </summary>
        public class MarkedUpElement : IComparable<MarkedUpElement>
        {
            /// <summary>
            /// Id of the element that is being marked up
            /// </summary>
            public string ElementId;

            /// <summary>
            /// sequence in which this element needs to be written to the instance document
            /// </summary>
            public float ElementSequence;
            
            

            #region IComparable<MarkedUpElement> Members

            /// <summary>
            /// comparison of two elements in the tupleset
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(MarkedUpElement other)
            {
                //int ret = this.TupleParentIdList.Count.CompareTo(other.TupleParentIdList.Count);
                //if (ret != 0) return ret;
                return this.ElementSequence.CompareTo(other.ElementSequence);
            }

            #endregion
        }

        #endregion
    }
}

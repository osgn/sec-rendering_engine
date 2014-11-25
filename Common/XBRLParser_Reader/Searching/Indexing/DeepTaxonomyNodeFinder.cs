using System.Collections;
using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    #region TaxonomyView class

    ///<summary>
    ///</summary>
    public abstract class TaxonomyView
    {
        private readonly Taxonomy _taxonomy;

        /// <summary>
        /// </summary>
        protected TaxonomyView(Taxonomy _taxonomy)
        {
            this._taxonomy = _taxonomy;
        }

        ///<summary>
        ///</summary>
        public string CurrentLabelRole { get { return _taxonomy.CurrentLabelRole; } }

        ///<summary>
        ///</summary>
        public string CurrentLanguage { get { return _taxonomy.CurrentLanguage; } }

        ///<summary>
        ///</summary>
        public abstract ArrayList GetNodesInView();

        ///<summary>
        ///</summary>
        public abstract string GetHashKeyForIndex();

        ///<summary>
        ///</summary>
        public static TaxonomyView Presentation(Taxonomy t) { return new PresentationTaxonomyView(t); }

        ///<summary>
        ///</summary>
        public static TaxonomyView Calculation(Taxonomy t) { return new CalculationTaxonomyView(t); }

        ///<summary>
        ///</summary>
        public static TaxonomyView Element(Taxonomy t) { return new ElementTaxonomyView(t); }

        /// <summary>
        /// </summary>
        protected class PresentationTaxonomyView : TaxonomyView
        {
            /// <summary>
            /// </summary>
            public PresentationTaxonomyView(Taxonomy _taxonomy)
                : base(_taxonomy)
            {
            }

            /// <summary>
            /// </summary>
            public override ArrayList GetNodesInView()
            {
                return _taxonomy.GetNodesByPresentation(true);
            }

            /// <summary>
            /// </summary>
            public override string GetHashKeyForIndex()
            {
                return "PRESENTATION";
            }
        }

        /// <summary>
        /// </summary>
        protected class CalculationTaxonomyView : TaxonomyView
        {
            /// <summary>
            /// </summary>
            public CalculationTaxonomyView(Taxonomy _taxonomy)
                : base(_taxonomy)
            {
            }

            /// <summary>
            /// </summary>
            public override ArrayList GetNodesInView()
            {
                return _taxonomy.GetNodesByCalculation();
            }

            /// <summary>
            /// </summary>
            public override string GetHashKeyForIndex()
            {
                return "CALCULATION";
            }
        }

        /// <summary>
        /// </summary>
        protected class ElementTaxonomyView : TaxonomyView
        {
            /// <summary>
            /// </summary>
            public ElementTaxonomyView(Taxonomy _taxonomy)
                : base(_taxonomy)
            {
            }

            /// <summary>
            /// </summary>
            public override ArrayList GetNodesInView()
            {
                return _taxonomy.GetNodesByElement();
            }

            /// <summary>
            /// </summary>
            public override string GetHashKeyForIndex()
            {
                return "ELEMENT";
            }
        }
    }
    #endregion

    ///<summary>
    ///</summary>
    public class DeepTaxonomyNodeFinder : INodesForIndexingProvider
    {
        private readonly TaxonomyView _taxonomyView;

        ///<summary>
        ///</summary>
        public DeepTaxonomyNodeFinder(TaxonomyView _taxonomyView)
        {
            this._taxonomyView = _taxonomyView;
        }

        private void RecursiveFindAndAddNodes(ICollection<Node> foundNodes, ArrayList nodesToSearch)
        {
            foreach (Node node in nodesToSearch)
            {
                if (!IsNodeSuitableForIndexing(node))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(node.Id))
                {
                    foundNodes.Add(node);
                }

                if( node.EnumData != null && node.EnumData.Count > 0 )
                {
                    foreach(string enumVal in node.EnumData)
                    {
                        Node en = LibrarySearchMgr.StaticCreateNodeForEnumValue(node, enumVal);
                        foundNodes.Add(en);
                    }
                }

                if (node.HasChildren)
                {
                    RecursiveFindAndAddNodes(foundNodes, node.Children);
                }
            }
        }

        ///<summary>
        ///</summary>
        public Node[] GetNodesForIndexing()
        {
            List<Node> result = new List<Node>();
            RecursiveFindAndAddNodes(result, _taxonomyView.GetNodesInView());
            return result.ToArray();
        }

        public bool IsNodeSuitableForIndexing(Node n)
        {
            return n != null && !n.IsProhibited;
        }

        ///<summary>
        ///</summary>
        public string GetLabelRole()
        {
            return _taxonomyView.CurrentLabelRole;
        }

        ///<summary>
        ///</summary>
        public string GetCurrentLanguage()
        {
            return _taxonomyView.CurrentLanguage;
        }

        ///<summary>
        ///</summary>
        public string GetHashKeyForIndex()
        {
            return _taxonomyView.GetHashKeyForIndex();
        }
    }
}

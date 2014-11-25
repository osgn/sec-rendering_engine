using System;
using System.Diagnostics;
using Aucent.MAX.AXE.XBRLParser;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene
{
    /// <summary>
    /// </summary>
    public class LuceneNodeIndexer : INodeIndexer
    {
        /// <summary>
        /// </summary>
        public static readonly string LABEL_FIELD = "Label";
        
        /// <summary>
        /// </summary>
        public static readonly string ELEMENTID_FIELD = "ElementId";
        
        /// <summary>
        /// </summary>
        public static readonly string ELEMENTID_FOR_DELETING_FIELD = "ElementIdAsKey";
        
        /// <summary>
        /// </summary>
        public static readonly string DEFINITION_FIELD = "Definition";
        
        /// <summary>
        /// </summary>
        public static readonly string ISABSTRACT_FIELD = "IsAbstract";
        
        /// <summary>
        /// </summary>
        public static readonly string ISEXTENDED_FIELD = "IsExtended";
        
        /// <summary>
        /// </summary>
        public static readonly string BALANCETYPE_FIELD = "BalanceType";
        
        /// <summary>
        /// </summary>
        public static readonly string PATH_FIELD = "Path";
        
        /// <summary>
        /// </summary>
        public static readonly string NAME_FIELD = "Name";
        
        /// <summary>
        /// </summary>
        public static readonly string REFERENCES_FIELD = "References";

        public static readonly string ORDER_FIELD = "Order";

        /// <summary>
        /// </summary>
        public static readonly string PATH_EOL_MARKER = "AUEND";

        /// <summary>
        /// </summary>
        public static readonly char PATH_SEPARATOR = '/';

        private readonly IndexWriter _indexWriter;
        private readonly string _currentLanguage;

        ///<summary>
        ///</summary>
        public LuceneNodeIndexer(IndexWriter _indexWriter, string _currentLanguage)
        {
            this._indexWriter = _indexWriter;
            this._currentLanguage = _currentLanguage;
        }

        /// <summary>
        /// </summary>
        public static string GetFieldName(LibrarySearchCriteria.SearchableTextField fromSearchableTextField)
        {
            switch(fromSearchableTextField)
            {
                case LibrarySearchCriteria.SearchableTextField.Definitions: return DEFINITION_FIELD;
                case LibrarySearchCriteria.SearchableTextField.Labels: return LABEL_FIELD;
                case LibrarySearchCriteria.SearchableTextField.Names: return NAME_FIELD;
                case LibrarySearchCriteria.SearchableTextField.References: return REFERENCES_FIELD;
                case LibrarySearchCriteria.SearchableTextField.ElementId: return ELEMENTID_FIELD;
            }

            throw new NotImplementedException("The universe is crashing:  Unable to get index field name for SearchableTextField " + fromSearchableTextField);
        }

        private string GetDefinition(Node node)
        {
            return node.GetDefinition(_currentLanguage) ?? string.Empty;
        }

        private string GetLabel(Node node, string labelRole)
        {
            string label;
            try
            {
                node.TryGetLabel(_currentLanguage, labelRole, out label);
            }
            catch (Exception e)
            {
                label = null;
				Debug.WriteLine(new Exception(string.Format("LuceneNodeIndexer: An error occurred while attempting to get the label for node {0}", node.Id), e));
            }

            return string.IsNullOrEmpty(label) ? node.Label : label;
        }

        private static string GetReferences(Node node)
        {
            //CKC:  Sri suggests that we write a simpler "GetReferencesUnformatted()" that will not be so slow.
            string references;
            if (!node.TryGetReferences(out references))
                references = string.Empty;
            return references;
        }

        /// <summary>
        /// </summary>
        public static string MakePathForNode(Node node)
        {
            return MakePathForNode(node, Convert.ToString(PATH_SEPARATOR), true);
        }

        /// <summary>
        /// </summary>
        public static string MakePathForNode(Node node, string pathSeparator, bool includeEndToken)
        {
            string result = string.Empty;

            Node parent = node;

            while(parent != null)
            {
                string thisPathPiece =  !string.IsNullOrEmpty(parent.Id) ? parent.Id : parent.Label;
                result = pathSeparator + thisPathPiece + result;
                parent = parent.Parent;
            }

            if( includeEndToken )
                result += PATH_EOL_MARKER;

            return result;
        }

        /// <summary>
        /// </summary>
        public static string[] MakePathAsArray(string path)
        {
            if( string.IsNullOrEmpty(path) )
                return new string[]{};

            string tmp = path;
            if (tmp.EndsWith(PATH_EOL_MARKER))
                tmp = tmp.Substring(0, tmp.Length - PATH_EOL_MARKER.Length);

            return tmp.Split(new char[] { PATH_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// </summary>
        public void AddToIndex(Node node, string labelRole)
        {
            Document doc = new Document();

            doc.Add(new Field(LABEL_FIELD, GetLabel(node, labelRole), Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(ELEMENTID_FOR_DELETING_FIELD, node.Id, Field.Store.YES, Field.Index.UN_TOKENIZED));
            doc.Add(new Field(ELEMENTID_FIELD, node.Id, Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(DEFINITION_FIELD, GetDefinition(node), Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(BALANCETYPE_FIELD, node.BalanceType, Field.Store.YES, Field.Index.UN_TOKENIZED));
            doc.Add(new Field(ORDER_FIELD, node.Order.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED));
            doc.Add(new Field(ISABSTRACT_FIELD, node.IsAbstract.ToString().ToLower(), Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(ISEXTENDED_FIELD, node.IsAucentExtendedElement.ToString().ToLower(), Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(PATH_FIELD, MakePathForNode(node), Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(NAME_FIELD, node.Name, Field.Store.YES, Field.Index.TOKENIZED));
            doc.Add(new Field(REFERENCES_FIELD, GetReferences(node), Field.Store.YES, Field.Index.TOKENIZED));

            _indexWriter.AddDocument(doc);
        }
    }
}

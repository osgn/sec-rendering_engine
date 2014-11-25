using Aucent.MAX.AXE.XBRLParser;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    /// <summary>
    /// </summary>
    public interface INodeIndexer
    {
        /// <summary>
        /// </summary>
        void AddToIndex(Node node, string labelRole);
    }
}

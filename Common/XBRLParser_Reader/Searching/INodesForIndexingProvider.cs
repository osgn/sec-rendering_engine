using Aucent.MAX.AXE.XBRLParser;

namespace Aucent.MAX.AXE.XBRLParser.Searching
{
    /// <summary>
    /// </summary>
    public interface INodesForIndexingProvider
    {
        /// <summary>
        /// </summary>
        Node[] GetNodesForIndexing();

        bool IsNodeSuitableForIndexing(Node n);

        /// <summary>
        /// </summary>
        string GetLabelRole();

        /// <summary>
        /// </summary>
        string GetCurrentLanguage();

        /// <summary>
        /// </summary>
        string GetHashKeyForIndex();
    }
}

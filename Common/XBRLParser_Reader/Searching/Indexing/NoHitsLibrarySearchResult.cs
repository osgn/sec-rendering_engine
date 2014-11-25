using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    /// <summary>
    /// </summary>
    public class NoHitsLibrarySearchResult : ILibrarySearchResult
    {
        /// <summary>
        /// </summary>
        public int Count
        {
            get { return 0; }
        }

        /// <summary>
        /// </summary>
        public ILibrarySearchResultItem[] Items
        {
            get { return new ILibrarySearchResultItem[]{}; }
        }

        /// <summary>
        /// </summary>
        public IEnumerator<ILibrarySearchResultItem> Iterator()
        {
            return new List<ILibrarySearchResultItem>().GetEnumerator();
        }
    }
}

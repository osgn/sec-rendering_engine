using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    ///<summary>
    ///</summary>
    public interface ILibrarySearchResult
    {
        ///<summary>
        ///</summary>
        int Count { get; }

        ///<summary>
        ///</summary>
        ILibrarySearchResultItem[] Items { get; }

        ///<summary>
        ///</summary>
        IEnumerator<ILibrarySearchResultItem> Iterator();
    }

    ///<summary>
    ///</summary>
    public interface ILibrarySearchResultItem
    {
        ///<summary>
        ///</summary>
        string ElementID { get; }

        ///<summary>
        ///</summary>
        string TaxonomyPath { get; }

        ///<summary>
        ///</summary>
        float SearchScore { get; }

        ///<summary>
        ///</summary>
        string Label { get; }

        ///<summary>
        ///</summary>
        double ElementOrder { get; }


    }
}

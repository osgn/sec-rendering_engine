using System;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing
{
    ///<summary>
    ///</summary>
    public interface ILibraryIndexMgr
    {
        ///<summary>
        ///</summary>
        ILibrarySearchResult Search(LibrarySearchCriteria criteria);

        ///<summary>
        ///</summary>
        void Initialize(string currentLanguage, INodesForIndexingProvider nodesForIndexingProvider);

        ///<summary>
        ///</summary>
        void ChangeToView(INodesForIndexingProvider nodesForIndexingProvider);

        ///<summary>
        ///</summary>
        void Close();

        ///<summary>
        ///</summary>
        void ChangeToLanguage(string langCode);

        ///<summary>
        ///</summary>
        void ReIndexNode(Node node);

        /// <summary>
        /// Is the manager currently indexing the library's taxonomy?
        /// </summary>
        bool IsIndexing { get; }

        /// <summary>
        /// Called whenever the indexing progress increments.  The event argument will be
        /// the percentage complete, from 0 to 1 (0 meaing 0% and 1 meaning 100%).
        /// </summary>
        event EventHandler<EventArgs<decimal>> IndexProgressUpdated;

        /// <summary>
        /// Called when the library starts the indexing process.
        /// </summary>
        event EventHandler IndexingStarted;

        /// <summary>
        /// Called when the library completes the indexing process.  The bool argument is true if one or more error occurred during indexing
        /// </summary>
        event EventHandler<EventArgs<bool>> IndexingCompleted;

        /// <summary>
        /// Called when the library encounters an error during the indexing process.
        /// </summary>
        event EventHandler<EventArgs<Exception>> ErrorWhileIndexing;

        ///<summary>
        ///</summary>
        string MakePath(Node node);

        ///<summary>
        ///</summary>
        string MakePath(Node node, string pathSeparator, bool includeEndToken);

        ///<summary>
        ///</summary>
        string[] MakePathAsArray(string path);

        ///<summary>
        ///</summary>
        string SingleCharacterWildCardSubstitute { get; }
    }
}

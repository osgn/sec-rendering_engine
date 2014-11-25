using System;

namespace Aucent.MAX.AXE.XBRLParser.Searching
{
    /// <summary>
    /// </summary>
    public interface ISearchInitializationObserver
    {
        /// <summary>
        /// </summary>
        void ObserveChangeInProgress(decimal percentCompleteFrom0To1);

        /// <summary>
        /// </summary>
        void ObserveInitializationComplete(bool wasErrored);

        /// <summary>
        /// </summary>
        void ObserveError(Exception e);
    }
}

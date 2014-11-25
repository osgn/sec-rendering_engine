using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
    [Serializable]
    public class TaxonomyChangeSetInfo
    {
        #region properties
        /// <summary>
        /// List of element changes
        /// </summary>
        public List<ElementChangeSetInfo> ElementChanges = new List<ElementChangeSetInfo>();


        public DateTime ChangeSetTimeStamp = DateTime.UtcNow;

        #endregion
        #region inner class
        [Serializable]
        public class ElementChangeSetInfo
        {
            #region properties
            public string FromElementId { get; set; }
            public string ToElementId { get; set; }
            #endregion
        }
        #endregion

    }
}

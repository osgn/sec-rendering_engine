using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
    /// <summary>
    /// We are going to add a rivet extension file to the rivet extended taxonomy in order to 
    /// help us track additional information about the taxonomy...
    /// </summary>
    [Serializable]
    public class RivetTaxonomyExtension
    {
        

        #region properties
       
       
        /// <summary>
        /// during migration any base report that points to the old tax will be converted to an extended report
        /// we need to keep a map of this info as folio might have prohibitted the old base report..
        /// so we want to prohibit the new extended report..
        /// 
        /// </summary>
        public List<MapInfo> OldBaseURLToNewExtendedURLMaps = new List<MapInfo>();


        public List<TaxonomyChangeSetInfo> TaxonomyChangeSetInfos = new List<TaxonomyChangeSetInfo>();



        public List<string> ElementsImpactedByMigration = new List<string>();

        

        #endregion
        #region inner class
        public class MapInfo
        {
            public string FromValue { get; set; }
            public string ToValue { get; set; }
        }
        #endregion


        public void Clear()
        {
        
            this.ElementsImpactedByMigration.Clear();
            this.OldBaseURLToNewExtendedURLMaps.Clear();
            this.TaxonomyChangeSetInfos.Clear();
        }

    }
}

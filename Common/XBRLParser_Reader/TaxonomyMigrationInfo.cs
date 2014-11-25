using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;

namespace Aucent.MAX.AXE.XBRLParser
{
    
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class TaxonomyMigrationInfo
    {
        #region properties

        public List<TaxonomyItemMap> TaxonomyItemMaps = new List<TaxonomyItemMap>();
        public List<LinkbaseFileInfoMap> LinkbaseFileInfoMaps = new List<LinkbaseFileInfoMap>();


        public const string NumericTypesXSD = "http://www.xbrl.org/dtr/type/numeric-2009-12-16.xsd";
        public const string NumericTypesNamespace = "http://www.xbrl.org/dtr/type/numeric";
        public const string NonNumericTypesXSD = "http://www.xbrl.org/dtr/type/nonNumeric-2009-12-16.xsd";
        public const string NonNumericTypesNamespace = "http://www.xbrl.org/dtr/type/non-numeric";

        public const string negated2011XSD = "http://www.xbrl.org/lrr/role/negated-2009-12-16.xsd";
        public const string negated2011Namespace = "http://www.xbrl.org/2009/role/negated";


        public const string net2011XSD = "http://www.xbrl.org/lrr/role/net-2009-12-16.xsd";
        public const string net2011Namespace = "http://www.xbrl.org/2009/role/net";

       
        [XmlIgnore]
        public Dictionary<string, string> NegatedLabelURIMap = new Dictionary<string, string>();


        public static string UsTypesXSD = "http://taxonomies.xbrl.us/us-gaap/2009/elts/us-types-2009-01-31.xsd";

        public const string negated2009XSD = "http://www.xbrl.org/lrr/role/negated-2008-03-31.xsd";

        [XmlIgnore]
        public Taxonomy Negated2011Tax = null;
        [XmlIgnore]
        public Taxonomy Negated2009Tax = null;


        [XmlIgnore]
        public TaxonomyItem NumericTypesTI = null;
        [XmlIgnore]
        public TaxonomyItem NonNumericTypesTI = null;

        [XmlIgnore]
        public List<string> NumericCustomTypes = null;

        [XmlIgnore]
        public List<string> NonNumericCustomTypes = null;

        [XmlIgnore]
        public Dictionary<string, string> UsTypesToNewTypesMap = new Dictionary<string, string>();

        #endregion

        #region inner classes
     
        #region Taxonomy Items Map
        
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class TaxonomyItemMap
        {
            public TaxonomyItem FromItem;
            public TaxonomyItem ToItem;

            public TaxonomyItemMap()
            {

            }

            public TaxonomyItemMap(TaxonomyItem f , TaxonomyItem t)
            {

                FromItem = f;
                ToItem = t;
            }

        }
        #endregion

        #region Linkbase reference Maps
        
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class LinkbaseFileInfoMap
        {
            public string FromItem;
            public string ToItem;

            public LinkbaseFileInfoMap()
            {

            }

            public LinkbaseFileInfoMap(string f, string t)
            {

                FromItem = f;
                ToItem = t;
            }

        }
        #endregion

        #endregion


    }
}

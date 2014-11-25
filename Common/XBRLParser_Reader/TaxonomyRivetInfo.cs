using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Exceptions;
using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// Provides properties and methods to encapsulate an XBRL taxonomy.
    /// </summary>
    public partial class Taxonomy 
    {

        public RivetTaxonomyExtension RivetExtensionInfo = new RivetTaxonomyExtension();

        private string GetRivetExtensionFileName()
        {

            return GetRivetExtensionFileName(schemaFile);
        }

        public string GetRivetExtensionFileName(string taxFileName )
        {

            return taxFileName.Replace(".xsd", "_riv.xml");
        }

        /// <summary>
        /// if we have rivet specific customizations that are available 
        /// </summary>
        private void LoadRivetExtensions()
        {

         

            if (this.IsAucentExtension)
            {
                string rivetExtensionName = GetRivetExtensionFileName();

                if (File.Exists(rivetExtensionName))
                {

                    FileStream fsRead = null;
                    
                    try
                    {
                        fsRead = new FileStream(rivetExtensionName, FileMode.Open, FileAccess.Read);


                        XmlSerializer serializer = new XmlSerializer(typeof(RivetTaxonomyExtension));
                        RivetExtensionInfo = serializer.Deserialize(fsRead) as RivetTaxonomyExtension;
                       
                    }
                    finally
                    {
                        
                        if (fsRead != null)
                            fsRead.Close();
                    }
                    

                    

                }
                

            }

        }

    }
}

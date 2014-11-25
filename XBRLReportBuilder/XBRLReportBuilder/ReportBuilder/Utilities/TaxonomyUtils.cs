//=============================================================================
// TaxonomyUtils (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This utility class contains the methods to process the XBRL taxonomies.
//=============================================================================

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;

using System.Xml.XPath;

using Aucent.MAX.AXE.Common.Utilities;
using System.Net.Cache;
//using net.xmlcatalog;

namespace XBRLReportBuilder
{
	/// <summary>
	/// TaxonomyUtils
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// 
	public static class TaxonomyUtils
	{
		public static bool TryLoadTaxonomy( string taxonomyPath, RequestCacheLevel remoteFileCachePolicy, 
			//XmlCatalogResolver xmlCatalog,
			out Taxonomy currentTaxonomy, out int numberErrors, out string errorMsg)
		{
			errorMsg = string.Empty;

			currentTaxonomy = new Taxonomy();
            currentTaxonomy.PromptUser = false;
			currentTaxonomy.CachePolicy = remoteFileCachePolicy;
			//currentTaxonomy.XmlCatalog = xmlCatalog;

			try
			{
				numberErrors = currentTaxonomy.Load(taxonomyPath, false);
				if (numberErrors == 0)
				{
					currentTaxonomy.Parse(out numberErrors);
				}
			}
			catch (XPathException)
			{
				numberErrors = 1;
				errorMsg = "Error parsing the taxonomy: Unable to find one or more of the dependent taxonomy files for taxonomy " + taxonomyPath;
				return false;
			}

			// ignore calc linkbase errors - don'care
            if (numberErrors != 0 && currentTaxonomy.ErrorList.Count > 0)
			{
				Console.WriteLine( "	Pres Errors: " + currentTaxonomy.NumPresErrors );
				Console.WriteLine( "	Calc Errors: " + currentTaxonomy.NumCalcErrors );
				Console.WriteLine( "	Label Errors: " + currentTaxonomy.NumLabelErrors );
				Console.WriteLine( "	Reference Errors: " + currentTaxonomy.NumReferenceErrors );

				currentTaxonomy.ErrorList.Sort();

				try
				{
					foreach ( ParserMessage pm in currentTaxonomy.ErrorList )
					{
						if ( pm.Level != TraceLevel.Error )
						{
							break;	// all the errors should be first after sort
						}

						errorMsg += pm.Message + Environment.NewLine;
						Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
					}

					errorMsg = "Error parsing the taxonomy: "+ errorMsg.Trim();
				}
				//Do nothing.  Error wasn't written to the event log.
				catch { }	

				// don't care about calc errors - if it's anything else, bomb out
				if ( numberErrors != currentTaxonomy.NumCalcErrors )
				{
					return false;
				}
			}

			return true;
		}
	}
}
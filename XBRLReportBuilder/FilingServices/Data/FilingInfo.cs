/**************************************************************************
 * FilingInfo (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class is used to store information about an individual XBRL data filing.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Aucent.FilingServices.Data
{
	[Serializable()]
	public class FilingInfo
	{
		/// <summary>
		/// This is really the name of the folder that contains the filing.  Right now in the
		/// Case of the SEC Report builder the folder name is an accessionNumber, but if that
		/// changes we may want to change the name of this variable.
		/// </summary>
		private string accessionNumber = string.Empty;
		public string AccessionNumber
		{
			get { return accessionNumber; }
			set { accessionNumber = value; }
		}

		/// <summary>
		/// This is the parent folder of the folder that contains the filing.
		/// </summary>
		private string parentFolder = string.Empty;
		public string ParentFolder
		{
			get { return parentFolder; }
			set { parentFolder = value; }
		}

		private string formType = string.Empty;
		public string FormType
		{
			get { return formType; }
			set { formType = value; }
		}

		private int batchRecordId = -1;
		public int BatchRecordId
		{
			get { return batchRecordId; }
			set { batchRecordId = value; }
		}

		public FilingInfo()
		{ }

		public string GetInstanceDocPath()
		{

			string filingDir = string.Format( "{0}{1}{2}", this.ParentFolder, Path.DirectorySeparatorChar, this.AccessionNumber );
			string[] files = Directory.GetFiles( filingDir, "*.xml" );
			Regex instanceRegex = new Regex( @"\d{8}\.xml$" );

			string retValue = string.Empty;
			foreach( string file in files )
			{
				Match regexMatch = instanceRegex.Match( file );
				if( regexMatch != null && regexMatch.Success )
				{
					//There should only be one file that matches the regex in each filing, so 
					//once we find it break and return
					retValue = file;
					break;
				}
			}

			return retValue;
		}

		public string GetTaxonomyPath()
		{
			string instanceFile = this.GetInstanceDocPath();
			if( string.IsNullOrEmpty( instanceFile ) )
				return string.Empty;

			try
			{
				List<string> foundTaxonomies = new List<string>();
				using( XmlTextReader doc = new XmlTextReader( instanceFile ) )
				{
					doc.MoveToContent();
					if( doc.LocalName != "xbrl" )
						return string.Empty;

					while( doc.Read() )
					{
						if( doc.NodeType != XmlNodeType.Element )
							continue;

						if( doc.LocalName != "schemaRef" )
							continue;

						while( doc.MoveToNextAttribute() )
						{
							if( doc.NodeType != XmlNodeType.Attribute || doc.Name != "xlink:href" )
								continue;

							if( doc.Value == Path.GetFileName( doc.Value ) )
							{
								//this one is prefered because it exists in the same folder
								string taxonomyPath = Path.Combine( Path.GetDirectoryName( instanceFile ), doc.Value );
								return taxonomyPath;
							}
							else
							{
								foundTaxonomies.Add( doc.Value );
							}
						}
					}
				}

				if( foundTaxonomies.Count > 0 )
				{
					string taxonomyPath = Path.Combine( Path.GetDirectoryName( instanceFile ), foundTaxonomies[ 0 ] );
					return taxonomyPath;
				}
			}
			catch { }

			return string.Empty;
		}
	}
}

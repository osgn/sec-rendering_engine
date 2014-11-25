using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilderRenderer
{
	public class Filing
	{
		internal enum SaveAs
		{
			Auto = 0,
			Xml = 1,
			Zip = 2
		}

		public string InstancePath { get; set; }
		public string TaxonomyPath { get; set; }

		public Filing( string instancePath )
		{
			if( !Path.IsPathRooted( instancePath ) )
			{
				string tmp = Path.Combine( FilingProcessor.cd, instancePath );
				Console.WriteLine( "Information: Instance document has a relative path '" + instancePath + "'." );
				Console.WriteLine( "\tAdjusting path to current location '" + tmp + "'" );
				instancePath = tmp;
			}

			this.InstancePath = instancePath;
		}
	}
}

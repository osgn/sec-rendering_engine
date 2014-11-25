using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilderRenderer;
using System.Diagnostics;

namespace ReportBuilderRenderer
{
	class Program
	{
		static void Main( string[] args )
		{
			if( ( args.Length == 0 ) ||
				( Array.IndexOf( args, "/?" ) > -1 ) ||
				( Array.IndexOf( args, "help" ) > -1 ) )
			{
				Syntax();
				return;
			}

			if( Array.IndexOf( args, "DEBUG" ) > -1 )
				Debugger.Break();

			try
			{
				FilingProcessor proc = FilingProcessor.Load( args );
				proc.ProcessFilings();
			}
			catch( Exception ex )
			{
				Console.WriteLine();
				Console.WriteLine( "Unexpected error: "+ ex.Message );
			}
		}

		private static void Syntax()
		{
			Console.WriteLine();
			Console.WriteLine( "The syntax of this command is:" );
			Console.WriteLine();

			Console.Write( "\t"+@"ReportBuilderRenderer.exe /"+ FilingProcessor.INSTANCE_COMMAND +@"=""[drive:][path]instance.xml"" ");
			Console.WriteLine( @"[/"+ FilingProcessor.INSTANCE_COMMAND +@"=""[drive:][path]instance.xml""] [...]" );
			Console.WriteLine( "\t\t- or -" );
			Console.Write( "\t" + @"ReportBuilderRenderer.exe /" + FilingProcessor.INSTANCE_COMMAND + @"=""[drive:][path]package.zip"" " );
			Console.WriteLine( @"[/" + FilingProcessor.INSTANCE_COMMAND + @"=""[drive:][path]package.zip""] [...]" );
			Console.WriteLine();

			Console.WriteLine( @"Optional parameters:" );
			Console.WriteLine();
			Console.WriteLine( "Set the base output path:" );
			Console.WriteLine( "\t/"+ FilingProcessor.REPORTS_FOLDER_COMMAND +"=[drive:][path\\to\\]folder" );
			Console.WriteLine();
			Console.WriteLine( "Set the report output format(s):" );
			Console.WriteLine( "\t/"+ FilingProcessor.REPORT_FORMAT_COMMAND +"=(Xml|Html|HtmlAndXml)" );
			Console.WriteLine();
			Console.WriteLine( "If format contains Html, set the html format:" );
			Console.WriteLine( "\t/"+ FilingProcessor.HTML_REPORT_FORMAT_COMMAND +"=(Complete|Fragment)" );
			Console.WriteLine();
			Console.WriteLine( "Set the caching policy of remote files:" );
			Console.WriteLine( "Value descriptions can be found at 'http://msdn.microsoft.com/en-us/library/system.net.cache.requestcachelevel.aspx'" );
			Console.WriteLine( "\t/"+ FilingProcessor.REMOTE_CACHE_POLICY_COMMAND +"=(Default|BypassCache|CacheOnly|CacheIfAvailable|Revalidate|Reload|NoCacheNoStore)" );
			Console.WriteLine();
			Console.WriteLine( "Set quiet mode, the application will not interact with the user:" );
			Console.WriteLine( "\t/"+ FilingProcessor.QUIET_COMMAND );
			Console.WriteLine();

			Console.WriteLine( "Set the output format for all filings in this session:" );
			Console.WriteLine( "\t/" + FilingProcessor.SAVEAS_COMMAND + "=(Xml|Zip)" );
			Console.WriteLine();
            Console.WriteLine("Set a custom XSLT stylesheet for the Html output format:");
            Console.WriteLine("\t/" + FilingProcessor.XSLT_STYLESHEET_COMMAND + @"=""[drive:][path]stylesheet.xslt"" ");
            Console.WriteLine();
		}
	}
}

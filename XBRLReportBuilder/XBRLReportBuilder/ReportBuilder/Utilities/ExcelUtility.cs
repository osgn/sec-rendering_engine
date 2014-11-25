//=============================================================================
// ExcelUtility (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This class handles creating Excel worksbooks based on the processed filings.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using XBRLReportBuilder;

using System.Xml.Xsl;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;
using Aucent.FilingServices.Data;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Diagnostics;

namespace XBRLReportBuilder
{
	public static class ExcelUtility
	{
		/// <summary>
		/// The method name refers to RR, but it is for all Instance Documents
		/// </summary>
		/// <param name="reportDirectory">The folder containing all reports (R1.xml, etc).
		/// This is also the destination folder.</param>
		/// <param name="excelReportName">The destination file name, without path or extention</param>
		/// <param name="xsltPath">The path to InstanceReport_XmlWorkbook.xslt</param>
		/// <param name="cssPath">The path to report.css</param>
		/// <returns></returns>
		public static bool GenerateExcelWorkbook( FilingSummary fs, string reportDirectory, string excelReportName )
		{
			if( !Directory.Exists( reportDirectory ) )
			    return false;


			try
			{
				SingleFileWorkbook sfwb = GenerateExcelWorkbook( fs, reportDirectory );

				string saveAt = Path.Combine( reportDirectory, excelReportName +".xls" );
				sfwb.SaveAs( saveAt );
			    return true;
			}
			catch { }

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reportDirectory">The folder containing all reports (R1.xml, etc)</param>
		/// <param name="xsltPath">The path to InstanceReport_XmlWorkbook.xslt</param>
		/// <param name="cssPath">The path to report.css</param>
		/// <returns></returns>
		public static SingleFileWorkbook GenerateExcelWorkbook( FilingSummary myFilingSummary, string reportDirectory )
		{
			string cssPath = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.StylesheetFile );
			string xsltPath = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.TransformWorkbookFile );

			XslCompiledTransform xslt = new XslCompiledTransform();
			xslt.Load( xsltPath );

			SingleFileWorkbook sfwb = new SingleFileWorkbook();
			Dictionary<string, int> sheetNames = new Dictionary<string, int>();

			//Process the R files. Each R File will be transformed into a worksheet in the workbook
			int idx = 0;
			foreach( ReportHeader reportHeader in myFilingSummary.MyReports )
			{
				if( reportHeader.ReportType == ReportHeaderType.Book )
					continue;

				string reportPath = Path.Combine( reportDirectory, reportHeader.XmlFileName );
				if( !File.Exists( reportPath ) )
				{
					Trace.TraceError(
						"Error: An error occurred while creating the Excel Workbook.\n" +
						"Sheet '" + reportHeader.ShortName + "' could not be loaded.\n" +
						"Reason:\n\tFile not found."
					);
					continue;
				}

				try
				{
					idx++;
					string tmpPath = Path.GetTempFileName();

					if( File.Exists( tmpPath + ".html" ) )
					{
						FileInfo fi = new FileInfo( tmpPath + ".html" );
						FileUtilities.DeleteFile( fi, true );
					}

					File.Move( tmpPath, tmpPath + ".html" );
					tmpPath += ".html";

					xslt.Transform( reportPath, tmpPath );

					SingleFileAttachment sfa = sfwb.AddAttachment( tmpPath );
					sfa.UseName = "Sheet" + idx.ToString( "00" ) + ".html";
					sfa.Name = NameSheet( reportHeader.ShortName, sheetNames );
					sheetNames[ sfa.Name ] = 1;
				}
				catch( Exception ex )
				{
					Trace.TraceWarning(
						"Warning: An error occurred while creating the Excel Workbook.\n" +
						"Sheet '" + reportHeader.ShortName + "' may be missing.\n"+
						"Reason:\n\t" + ex.Message
					);
				}
			}

			string[] charts = Directory.GetFiles( reportDirectory, "*.jpg" );
			foreach( string chart in charts )
			{
				try
				{
					SingleFileAttachment sfa = sfwb.AddAttachment( chart );
				}
				catch (Exception ex )
				{
					Trace.TraceWarning(
						"Warning: An error occurred while creating the Excel Workbook.\n" +
						"This attachment could not be embedded:\n\t"+ Path.GetFileName( chart )+
						"\nReason:\n\t"+ ex.Message
					);
				}
			}

			return sfwb;
		}

		public static string NameSheet( string title, Dictionary<string, int> sheetNames )
		{
			return NameSheet( title, sheetNames, 30 );
		}

		public static string NameSheet( string title, Dictionary<string, int> sheetNames, int truncateLength )
		{
			if( title == null )
				title = string.Empty;

			//#1 - replace spaces with underscores
			title = Regex.Replace( title, @"\s+", "_" );

			//#2 - remove any non-text characters
			title = Regex.Replace( title, @"\W+", string.Empty );

			//#3 - Step #2 might create sequential underscores - clean them up
			title = Regex.Replace( title, @"[_]+", "_" );
			title = title.Trim( '_' );

			if( truncateLength > 0 )
			{
				//Either use the current size, or N characters, whichever is smaller
				title = title.Substring( 0, Math.Min( title.Length, truncateLength ) );
			}

			int cnt = 0;
			string test = title;
			while( test.Length == 0 || sheetNames.ContainsKey( test ) )
			{
				//we support up to 9 similarly named reports / sheets
				cnt++;
				test = string.Format( "{0}{1}", title, cnt.ToString( "0" ) );
			}

			return test;
		}
	}
}
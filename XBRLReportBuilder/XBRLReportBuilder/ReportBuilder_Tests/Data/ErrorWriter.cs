using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder;
using System.IO;
using System.Xml.Xsl;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test.Data
{
	public class ErrorWriter : StringWriter
	{
		private InstanceReport _baseReport = null;
		private InstanceReport _generatedReport = null;
		private string _failedPath = null;
		private string _fileName = null;

		public int Errors { get; set; }

		public bool HasErrors
		{
			get { return this.Errors > 0; }
		}

		public bool InError { get; private set; }

		private Stack<ErrorWriter> _reports = new Stack<ErrorWriter>();
		private ErrorWriter currentReport
		{
			get
			{
				if( this._reports.Count == 0 )
					return null;

				return this._reports.Peek();
			}
		}

		public ErrorWriter()
		{
			this.Initialize();
		}

		public ErrorWriter( string file, InstanceReport baseReport, InstanceReport genReport, string failedPath )
		{
			this._baseReport = baseReport;
			this._generatedReport = genReport;
			this._failedPath = failedPath;
			this._fileName = Path.GetFileName( file );
		}

		private void Initialize()
		{
			this.Errors = 0;
			this.InError = false;
		}

		public void StartReport( string file, InstanceReport baseReport, InstanceReport genReport, string failedPath )
		{
			ErrorWriter report = new ErrorWriter( file, baseReport, genReport, failedPath );
			this._reports.Push( report );

			this.WriteLine();
			this.WriteLine( "*************************Comparison STARTED " + file + "*************************" );
		}

		public void EndReport( string file )
		{
			this.WriteLine( "*************************Comparison ENDED " + file + "*************************" );
			this.WriteLine();

			ErrorWriter report = this._reports.Pop();
			if( report.HasErrors )
			{
				report.Flush();

				if( this.currentReport == null )
				{
					this.Errors += report.Errors;
					report.SaveFailedFiles();
				}
				else
				{
					this.currentReport.Errors += report.Errors;
				}

				this.Write( report.GetStringBuilder().ToString().Trim() );
			}
		}

		private void SaveFailedFiles()
		{
			if( string.IsNullOrEmpty( this._failedPath ) )
				return;

			if( !Directory.Exists( this._failedPath ) )
				Directory.CreateDirectory( this._failedPath );

			Test_Abstract.CopyResourcesToPath( this._failedPath );

			string basePath = null;
			if( this._baseReport != null )
			{
				basePath = Path.Combine( this._failedPath, "base_" + this._fileName );
				this._baseReport.SaveAsXml( basePath, true );
			}

			string genPath = null;
			if( this._generatedReport != null )
			{
				genPath = Path.Combine( this._failedPath, this._fileName );
				this._generatedReport.SaveAsXml( genPath, true );
			}

			XsltArgumentList args = new XsltArgumentList();
			args.AddParam( "asPage", string.Empty, "false" );
			args.AddParam( "showFlags", string.Empty, "true" );

			string baseHtml = basePath == null ? "<h1>Missing</h1>" : Test_Abstract.Transform( basePath, args );
			string genHtml = genPath == null ? "<h1>Missing</h1>" : Test_Abstract.Transform( genPath, args );
			string html = string.Format( COMPARISON_FORMAT, baseHtml, genHtml, this.GetStringBuilder().ToString().Trim() );

			string comparePath = Path.Combine( this._failedPath, "compare_" + this._fileName );
			File.WriteAllText( comparePath + ".html", html );
		}

		protected const string COMPARISON_FORMAT = @"<html>
<head>
	<link href=""report.css"" media=""all"" rel=""stylesheet"" type=""text/css"" />
	<script language=""JavaScript"" type=""text/javascript"" src=""Show.js""></script>
	<style type=""text/css"">
		.Flag1 {{
			background-color: yellow;
		}}
	</style>
</head>
<body>
<table>
<tr>
	<td><strong>Base</strong></td>
	<td><strong>Generated</strong></td>
</tr>
<tr>
	<td valign=""top"">{0}</td>
	<td valign=""top"">{1}</td>
</tr>
</table>
<pre>{2}</pre>
</body>
</html>";

		public void SaveResults( string failedPath )
		{
			if( !Directory.Exists( failedPath ) )
				Directory.CreateDirectory( failedPath );

			string failedResults = Path.Combine( failedPath, "Results.txt" );
			File.WriteAllText( failedResults, this.GetStringBuilder().ToString().Trim() );
		}

		public void StartError( params string[] lines )
		{
			if( this.currentReport == null )
			{
				this.WriteError( lines );
				this.InError = true;
			}
			else
			{
				this.currentReport.StartError( lines );
			}
		}

		public void EndError( params string[] lines )
		{
			if( this.currentReport == null )
			{
				this.WriteError( lines );
				this.InError = false;
			}
			else
			{
				this.currentReport.EndError( lines );
			}
		}

		public void WriteError( params string[] lines )
		{
			if( this.currentReport == null )
			{
				if( !this.InError )
					this.Errors++;

				Array.ForEach( lines, line => this.WriteLine( line ) );
			}
			else
			{
				this.currentReport.WriteError( lines );
			}
		}

		public override void Write( string value )
		{
			if( this.currentReport == null )
				base.Write( value );
			else
				this.currentReport.Write( value );
		}

		public override void WriteLine( string value )
		{
			if( this.currentReport == null )
				base.WriteLine( value );
			else
				this.currentReport.WriteLine( value );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Data
{
	public class SingleFileWorkbook : IDisposable
	{
		private List<SingleFileAttachment> attachments = new List<SingleFileAttachment>();
		public List<SingleFileAttachment> Attachments
		{
			get { return attachments; }
			set { attachments = value; }
		}

		private string contentLocationBase = "http://www.example.com/";
		public string ContentLocationBase
		{
			get { return contentLocationBase; }
			set { contentLocationBase = value; }
		}

		private string mimeBoundary = string.Empty;
		public string MIMEBoundary
		{
			get
			{
				return this.mimeBoundary;
			}
			private set
			{
				this.mimeBoundary = value;
			}
		}

		public SingleFileWorkbook()
		{
			string guid = Guid.NewGuid().ToString();
			guid = guid.Replace( "-", "_" );

			this.ContentLocationBase = "file:///C:/" + guid;
			this.MIMEBoundary = "----=_NextPart_" + guid;
		}

		~SingleFileWorkbook()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			foreach( SingleFileAttachment att in this.Attachments )
			{
				att.Dispose();
			}
		}

		public SingleFileAttachment AddAttachment( string path )
		{
			SingleFileAttachment sfa = new SingleFileAttachment( path );

			string ext = Path.GetExtension( path );
			if( !string.IsNullOrEmpty( ext ) )
				ext = ext.ToLower();

			sfa.RelPath = "Worksheets";
			this.Attachments.Add( sfa );
			return sfa;
		}

		public string Build()
		{
			StringBuilder source = new StringBuilder();
			source.Append( this.BuildPreamble() );
			source.AppendLine();
			source.Append( this.BuildIndex() );
			source.AppendLine();


			foreach( SingleFileAttachment sfa in this.Attachments )
			{
				source.Append( this.BuildAttachmentHeader( sfa ) );
				source.AppendLine();

				source.AppendLine( sfa.GetSource() );
				source.AppendLine();
			}

			//source.AppendLine();
			source.Append( this.BuildFileList() );
			source.AppendLine();
			source.AppendLine( "--"+ this.MIMEBoundary +"--" );

			return source.ToString();
		}

		public string BuildAttachmentHeader( SingleFileAttachment att )
		{
			string header = att.GetAttachmentHeader( this.ContentLocationBase, this.MIMEBoundary );
			return header;
		}

		public string BuildFileList()
		{
			string tmp = Path.GetTempPath();
			SingleFileAttachment sfa = new SingleFileAttachment( tmp );
			sfa.Name = "filelist.xml";
			sfa.RelPath = "Worksheets";
			sfa.UseName = "filelist.xml";

			string fileList = this.BuildAttachmentHeader( sfa );
			fileList += Environment.NewLine;


			string files = string.Empty;
			foreach( SingleFileAttachment att in this.Attachments )
			{
				files += @" <o:File HRef=3D"""+ Path.GetFileName( att.RelPath ) +@"""/>
";
			}

			string fileListTmpl = @"<xml xmlns:o=3D""urn:schemas-microsoft-com:office:office"">
 <o:MainFile HRef=3D""../Workbook.html""/>
{0}</xml>";

			fileList += string.Format( fileListTmpl, files );
			return fileList;
		}

		public string BuildIndex()
		{
			string tmp = Path.GetTempPath();
			SingleFileAttachment sfa = new SingleFileAttachment( tmp );
			sfa.Name = "Workbook.html";
			sfa.UseName = "Workbook.html";

			string index = this.BuildAttachmentHeader( sfa );
			index += Environment.NewLine;

			//string links = string.Empty;
			string sheets = string.Empty;
			foreach( SingleFileAttachment att in this.Attachments )
			{
				if( att.UseName.StartsWith( "Sheet" ) )
				{
					//links += att.GetWorksheetLink();
					sheets += att.GetWorksheetXML();
				}
			}

			index += string.Format( INDEX_TEMPLATE, sheets );
			index += Environment.NewLine;
			return index;
		}

		public string BuildPreamble()
		{
			string preamble = "" +
@"MIME-Version: 1.0
X-Document-Type: Workbook
Content-Type: multipart/related; boundary=""" + this.MIMEBoundary + @"""

This document is a Single File Web Page, also known as a Web Archive file.  If you are seeing this message, your browser or editor doesn't support Web Archive files.  Please download a browser that supports Web Archive, such as Microsoft Internet Explorer.
";

			return preamble;
		}

		public bool SaveAs( string path )
		{
			try
			{
				string source = this.Build();
                
				File.WriteAllText( path, source, Encoding.UTF8);                               
				return true;
			}
			catch(Exception)
			{

			}

			return false;
		}

       
        
		public const string INDEX_TEMPLATE = @"<html xmlns:v=3D""urn:schemas-microsoft-com:vml"" xmlns:o=3D""urn:schemas-microsoft-com:office:office"" xmlns:x=3D""urn:schemas-microsoft-com:office:excel"" xmlns=3D""http://www.w3.org/TR/REC-html40"">
<head>
<meta name=3D""Excel Workbook Frameset"">

<meta name=3DProgId content=3DExcel.Sheet>
<link rel=3DFile-List href=3D""Worksheets/filelist.xml"">

<!--[if gte mso 9]><xml>
 <x:ExcelWorkbook>
  <x:ExcelWorksheets>
{0}  </x:ExcelWorksheets>
  <x:Stylesheet HRef=3D""Worksheets/report.css""/>
  <x:ActiveSheet>0</x:ActiveSheet>
  <x:ProtectStructure>False</x:ProtectStructure>
  <x:ProtectWindows>False</x:ProtectWindows>
 </x:ExcelWorkbook>
</xml><![endif]-->
</head>
  <body>
   <p>This page should be opened with Microsoft Excel XP or newer.</p>
  </body>
</html>";
	}
}

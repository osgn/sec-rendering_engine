using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Data
{
	public partial class SingleFileAttachment : IDisposable
	{
		private string name = string.Empty;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string path = string.Empty;
		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		private string relPath = string.Empty;
		public string RelPath
		{
			get
			{
				if( string.IsNullOrEmpty( this.relPath ) )
					return this.UseName;

				string path = this.relPath + "/" + this.UseName;
				return path;
			}
			set { relPath = value; }
		}

		private string useName = string.Empty;
		public string UseName
		{
			get { return useName; }
			set { useName = value; }
		}

        public static readonly Dictionary<string, string> currencyHtmlCodes =
            new Dictionary<string, string>()
            {
                {"$", "$"},			//Dollar
		        {"£", "&#xa3;"},	//Pound
                {"€", "&#x20ac;"},  //Euro
                {"¥", "&#xa5;"},    //Yen; Yuan
                {"¤", "&#xa4;"},  	//Currency 
                //{"P", "&#x20a7;"},	//Peseta
				{"₩", "&#x20a9;"},	//Won
				//{"₱", "&#x20b1;"},	//Peso (Philippines)
				{"₨", "&#x20a8;"},	//India Rupee
				//{"руб", "&#x440;"},	//Rubles
                //{"£", "&#x20a4;"},	//Lira
			};
       
		public SingleFileAttachment( string attachmentPath )
		{
			this.Name = System.IO.Path.GetFileNameWithoutExtension( attachmentPath );
			this.Path = attachmentPath;
			this.UseName = System.IO.Path.GetFileName( attachmentPath );
		}

		~SingleFileAttachment()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if( File.Exists( this.Path )  && !this.useName.ToLower().EndsWith (".jpg"))
			{
				try
				{
					File.Delete( this.Path );
				}
				catch { }
			}
		}

		public string GetSource()
		{
			string source = string.Empty;

			string ext = System.IO.Path.GetExtension( this.UseName ).ToLower();
			switch( ext ){
				case ".htm":
				case ".html":
					source = this.GetSourceHTML();
					break;
				case ".gif":
				case ".jpg":
				case ".jpeg":
				case ".png":
					source = this.GetSourceChunked();
					break;
				default:
					source = this.GetSourceRaw();
					break;
			}

			return source;
		}

		public string GetSourceChunked()
		{
			byte[] source = File.ReadAllBytes( this.Path );
			string strSource = Convert.ToBase64String( source );
			string chunked = WrapStringAt( strSource, 76 );
			return chunked;
		}

		public string GetSourceHTML()
		{
			string source = File.ReadAllText( this.Path );
			string cleanSource = source;

			cleanSource = Regex.Replace( cleanSource, @"<br\s*\/?>", "\n", RegexOptions.IgnoreCase );
			cleanSource = ScrubHTML( source );
            cleanSource = CleanCurrency(cleanSource);

			//source = Regex.Replace( source, @"\salign=""(\w+)""", " align=3D$1", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\sclass=""(\w+)""", " class=3D$1", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\scolspan=""(\d+)""", " colspan=3D$1", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\sheight=""(\d+)""", " height=3D$1", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\srowspan=""(\d+)""", " rowspan=3D$1", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\ssrc=""([^\""]+)""", " src=3D'$1'", RegexOptions.IgnoreCase );
			//source = Regex.Replace( source, @"\sstyle=""([^\""]+)""", " style=3D'$1'", RegexOptions.IgnoreCase );

			return cleanSource;
		}

        private string CleanCurrency(string cleaned)
        {
            string str = cleaned;
            foreach (string currency in currencyHtmlCodes.Keys)
            {
                str = str.Replace(currency, currencyHtmlCodes[currency]);
            }
            return str;
        }

		public string GetSourceRaw()
		{
			string source = File.ReadAllText( this.Path );
			return source;
		}

		public string GetWorksheetLink()
		{
			string link = @"<link id=3D""shLink"" href=3D"""+ this.RelPath +@""">
";

			return link;
		}

		public string GetWorksheetXML()
		{
			string xml = "" +
@"   <x:ExcelWorksheet>
    <x:Name>" + this.Name + @"</x:Name>
    <x:WorksheetSource HRef=3D""" + this.RelPath + @"""/>
   </x:ExcelWorksheet>
";

			return xml;
		}

		public string GetAttachmentHeader( string contentLocationBase, string mimeBoundary )
		{
			string header = string.Empty;

			string ext = System.IO.Path.GetExtension( this.UseName ).ToLower();
			switch( ext ){
				case ".htm":
				case ".html":
					header = 
@"--" + mimeBoundary + @"
Content-Location: " + contentLocationBase + "/" + this.RelPath + @"
Content-Transfer-Encoding: quoted-printable
Content-Type: text/html; charset=""us-ascii""
";
					break;
				case ".gif":
					header = 
@"--" + mimeBoundary + @"
Content-Location: " + contentLocationBase + "/" + this.RelPath + @"
Content-Transfer-Encoding: base64
Content-Type: image/gif
";
					break;
				case ".jpg":
				case ".jpeg":
					header = 
@"--" + mimeBoundary + @"
Content-Location: " + contentLocationBase + "/" + this.RelPath + @"
Content-Transfer-Encoding: base64
Content-Type: image/jpeg
";
					break;
				case ".png":
					header = 
@"--" + mimeBoundary + @"
Content-Location: " + contentLocationBase + "/" + this.RelPath + @"
Content-Transfer-Encoding: base64
Content-Type: image/png
";
					break;
				default:
					header = 
@"--" + mimeBoundary + @"
Content-Location: " + contentLocationBase + "/" + this.RelPath + @"
Content-Transfer-Encoding: quoted-printable
Content-Type: text/html; charset=""us-ascii""
";
					break;
			}



			return header;
		}
	}
}

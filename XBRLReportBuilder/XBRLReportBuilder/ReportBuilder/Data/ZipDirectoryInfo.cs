using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.Common.ZipCompressDecompress.Zip;
using System.Text.RegularExpressions;

namespace XBRLReportBuilder
{
	public class ZipDirectoryInfo : ZipFile, IDirectoryInfo
	{
		public ZipDirectoryInfo( string path ) : base( path )
		{
			this._fullName = path;
		}

		#region IDirectoryInfo Members

		public bool Exists
		{
			get
			{
				//if this class even loaded, it exists
				return true;
			}
		}

		private string _fullName = null;
		public string FullName
		{
			get
			{
				return this._fullName;
			}
		}

		public bool FileExists( string file )
		{
			int at = this.FindEntry( file, false );
			bool exists = at > -1;
			return exists;
		}

		public IFileInfo[] GetFiles( string searchPattern )
		{
			searchPattern = searchPattern.Replace( ".", "\\." );
			searchPattern = searchPattern.Replace( "*", ".*" );
			Regex validator = new Regex( searchPattern );

			List<IFileInfo> files = new List<IFileInfo>();
			foreach( ZipEntry zip in this.entries )
			{
				if( validator.IsMatch( zip.Name ) )
					files.Add( new ZipFileInfo( zip ) );
			}
			return files.ToArray();
		}

		public System.IO.Stream OpenStream( string file )
		{
			ZipEntry zip = this.GetEntry( file );
			return this.GetInputStream( zip );
		}

		#endregion
	}
}

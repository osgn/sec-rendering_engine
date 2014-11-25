using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace XBRLReportBuilder
{
	public class ExtendedDirectoryInfo : IDirectoryInfo
	{
		private DirectoryInfo _baseDirectoryInfo = null;

        /// <summary>
        /// Creates a new <see cref="ExtendedDirectoryInfo"/> from the given
        /// path.
        /// </summary>
        /// <param name="path">The path to the directory to create an extended
        /// directory info for.</param>
		public ExtendedDirectoryInfo( string path )
		{
			this._baseDirectoryInfo = new DirectoryInfo( path );
		}

		#region IDirectoryInfo Members

		public bool Exists
		{
			get
			{
				return this._baseDirectoryInfo.Exists;
			}
		}

		public string FullName
		{
			get
			{
				return this._baseDirectoryInfo.FullName;
			}
		}

        /// <summary>
        /// Determine if the given filename pattern has at least one match
        /// within the directory.
        /// </summary>
        /// <param name="file">The search pattern.  For specifics, see
        /// documentation for <see cref="DirectoryInfo.GetFiles(string)"/>.
        /// </param>
        /// <returns>A <see cref="bool"/> containing true if at least one match
        /// was found.</returns>
		public bool FileExists( string file )
		{
			FileInfo[] fis = this._baseDirectoryInfo.GetFiles( file );
			bool exists = fis.Length == 1;
			return exists;
		}

        /// <summary>
        /// Returns an array of <see cref="IFileInfo"/> representing all files
        /// within the directory matching the given search pattern.
        /// </summary>
        /// <param name="searchPattern">The search pattern for locating files.
        /// </param>
        /// <returns>An array of <see cref="IFileInfo"/> containing matches
        /// from the search pattern within the directory.</returns>
		public IFileInfo[] GetFiles( string searchPattern )
		{
			FileInfo[] fis = this._baseDirectoryInfo.GetFiles( searchPattern );
			IFileInfo[] ifis = Array.ConvertAll( fis, fi => new ExtendedFileInfo( fi ) );
			return ifis;
		}

		public Stream OpenStream( string file )
		{
			FileInfo[] fis = this._baseDirectoryInfo.GetFiles( file );
			if( fis.Length != 1 )
				return null;

			return fis[ 0 ].OpenRead();
		}

		#endregion
	}
}

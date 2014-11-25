using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace XBRLReportBuilder
{
	public class ExtendedFileInfo : IFileInfo
	{
		private FileInfo _baseFileInfo = null;

        //TODO remove - unused
		public ExtendedFileInfo( string path )
		{
			this._baseFileInfo = new FileInfo( path );
		}

        /// <summary>
        /// Creates a new <see cref="ExtendedFileInfo"/> from the given
        /// <paramref name="fi"/>.
        /// </summary>
        /// <param name="fi">The <see cref="FileInfo"/> instance to use as a
        /// base for creating the instance of <see cref="ExtendedFileInfo"/>.
        /// </param>
		public ExtendedFileInfo( FileInfo fi )
		{
			this._baseFileInfo = fi;
		}

		public long Length
		{
			get { return this._baseFileInfo.Length; }
		}

		public string Name
		{
			get { return this._baseFileInfo.Name; }
		}
	}
}

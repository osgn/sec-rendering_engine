using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.Common.ZipCompressDecompress.Zip;

namespace XBRLReportBuilder
{
	public class ZipFileInfo : IFileInfo
	{

        /// <summary>
        /// Creates a new <see cref="ZipFileInfo"/> instance with the given zip
        /// entry as the base.
        /// </summary>
        /// <param name="zip">The base <see cref="ZipEntry"/> upon which to
        /// create an instance.</param>
		public ZipFileInfo( ZipEntry zip )
		{
			this._length = zip.Size;
			this.Name = zip.Name;
		}

		private long _length;
		public long Length
		{
			get { return this._length; }
			private set { this._length = value; }
		}

		private string _name;
		public string Name
		{
			get { return this._name; }
			private set { this._name = value; }
		}
	}
}

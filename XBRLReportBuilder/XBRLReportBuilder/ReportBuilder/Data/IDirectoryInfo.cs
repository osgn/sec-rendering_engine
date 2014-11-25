using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace XBRLReportBuilder
{
	public interface IDirectoryInfo
	{
		bool Exists { get; }
		string FullName { get; }

		bool FileExists( string file );
		IFileInfo[] GetFiles( string searchPattern );
		Stream OpenStream( string file );
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace XBRLReportBuilder
{
	public interface IFileInfo
	{
		long Length { get; }
		string Name { get; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.XBRLParser;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// This interface was created because the Dimension & Calendar sorter only requires the ContextProperty.
	/// Any classes which implement this interface can now reuse this sorter.
	/// 
	/// This sorter is found at XBRLReportBuilder\ReportBuilder\Data\SortByDurationFirst.cs
	/// </summary>
	public interface IHasContextProperty
	{
		ContextProperty contextRef { get; }
	}
}

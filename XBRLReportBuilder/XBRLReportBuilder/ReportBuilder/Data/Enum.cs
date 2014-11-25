using System;
using System.Collections.Generic;
using System.Text;

namespace XBRLReportBuilder
{
	/// <summary>
	/// Sheet: individual report
	/// Book: entire report
	/// Uncategorized: stores data can't be placed in any of the report
	/// </summary>
	public enum ReportHeaderType
	{
		Sheet = 0,
		Yearly,
		Book,
		Notes,
		Uncategorized
	}

	public enum RoundingLevel
	{
		UnKnown = -1,
		NoRounding = 0,
		Tens = 1,
		Hundreds = 2,
		Thousands = 3,
		TenThousands = 4,
		HundredThousands = 5,
		Millions = 6,
		TenMillions = 7,
		HundredMillions = 8,
		Billions = 9,
		TenBillions = 10,
		HundredBillions = 11,
		Trillions = 12,
        TenTrillions = 13,
        HundredTrillions = 14,
        Quadrillions = 15,
	}

	[Flags]
	public enum UnitType
	{
		Other = 0,
		Shares = 1,
		Monetary = 2,
		EPS = 3,
		ExchangeRate = 4 + 2,
		StandardUnits = 128
	}
}

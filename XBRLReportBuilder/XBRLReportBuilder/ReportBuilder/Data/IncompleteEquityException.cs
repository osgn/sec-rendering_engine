using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data
{
	public class IncompleteEquityException : Exception
	{
		public enum ErrorType
		{
			None,
			Incomplete,
			MissingBeginningBalance,
			MissingEndingBalance,
			MissingMembersFile,
			MembersFileFormat
		}

		public ErrorType Type = ErrorType.None;

        /// <summary>
        /// Creates a new <see cref="IncompleteEquityException"/> with the
        /// given type.
        /// </summary>
        /// <param name="type">The type of Incomplete Equity Exception to
        /// create.</param>
		public IncompleteEquityException( ErrorType type )
		{
			this.Type = type;
		}
	}
}

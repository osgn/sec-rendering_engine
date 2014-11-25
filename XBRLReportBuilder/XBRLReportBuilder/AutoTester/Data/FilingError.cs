using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Data
{
	public class FilingResult
	{
		public string Name = string.Empty;
		public int Errors = 0;
		public bool Success = true;
		public string Reason = string.Empty;

		public FilingResult() { }

		public override string ToString()
		{
 			string text = string.Format( "{0}: {1} - {2}",
				 this.Name, this.Success ? "Successful Test" : "Unsuccessful Test", this.Reason );

			return text;
		}
	}
}

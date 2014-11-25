using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.CLI
{
	public class CommandLineMethodAttribute : Attribute, ICommandLineArgument
	{
		/// <summary>
		/// Indicates the verbose command for the parameter.
		/// It can be called using the format {--command} where command is this Name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Indicates the shortcut command for the parameter.
		/// It can be called using the format {-c} where c is the this Shortcut.
		/// </summary>
		public char Shortcut { get; set; }
	}
}

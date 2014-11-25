using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;

namespace XBRLReportBuilder
{
	[XmlRoot( "Log" )]
	public class LogItem
	{
		[XmlAttribute( "type" )]
		public TraceLevel Type { get; set; }

		[XmlText]
		public string Item { get; set; }

        /// <summary>
        /// Creates a new <see cref="LogItem"/>.
        /// </summary>
		public LogItem() { }

        /// <summary>
        /// Creates a new <see cref="LogItem"/> with a specific trace level and
        /// message.
        /// </summary>
        /// <param name="type">The trace level of the log item.</param>
        /// <param name="message">The message for the log item.</param>
		public LogItem( TraceLevel type, string message )
		{
			this.Item = message;
			this.Type = type;
		}
	}
}

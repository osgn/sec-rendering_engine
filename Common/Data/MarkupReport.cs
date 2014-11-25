using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// Represents the properties and functionality of a markup report.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(MarkupReportItem))]
	public class MarkupReport
	{
		private string taxonomyFriendlyName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string TaxonomyFriendlyName
		{
			get { return taxonomyFriendlyName; }
			set { taxonomyFriendlyName = value; }
		}

		private string entityCode = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string EntityCode
		{
			get { return entityCode; }
			set { entityCode = value; }
		}

		private string entityName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string EntityName
		{
			get { return entityName; }
			set { entityName = value; }
		}

		private MarkupReportItem[] makupItems = null;

		/// <summary>
		/// 
		/// </summary>
		public MarkupReportItem[] MarkupItems
		{
			get { return makupItems; }
			set { makupItems = value; }
		}

		/// <summary>
		/// Constructs a new instance of <see cref="MarkupReport"/>
		/// </summary>
		public MarkupReport()
		{
		}

		/// <summary>
		/// Returns an XML serialized <see cref="String"/> representation of this <see cref="MarkupReport"/>.
		/// </summary>
		/// <param name="error">An output parameter.  Any error that occurred in the serialization process.</param>
		/// <returns></returns>
		public string GetXMLString(out string error)
		{
			error = string.Empty;

			XmlDocument currentXMLDocument = new XmlDocument();
			string xmlString = string.Empty;

			if (Aucent.MAX.AXE.Common.Utilities.SerializationUtilities.TryXmlSerializeObjectToString(this, out xmlString, out error))
			{
				return xmlString;
			}
			else
			{
				return string.Empty;
			}
		}

	}

	/// <summary>
	/// Encapsulate the properties of an item on the markup report.
	/// </summary>
	[Serializable]
	public class MarkupReportItem
	{
		/// <summary>
		/// 
		/// </summary>
		public string ReportName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public object Data = null;

		/// <summary>
		/// 
		/// </summary>
		public Nullable<decimal> DataAppliedScale = null; //if numeric, might need to modify by Scale Factor        

		/// <summary>
		/// 
		/// </summary>
		public bool IsExtended = false;

		/// <summary>
		/// 
		/// </summary>
		public Nullable<DateTime> ReportPeriodBeginDate = null;

		/// <summary>
		/// 
		/// </summary>
		public Nullable<DateTime> ReportPeriodEndDate = null; //End date would store AS OF DATE for instant and the EndDate for duration      

		/// <summary>
		/// 
		/// </summary>
		public string ReportingPeriodDisplayName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementID = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementLabel = string.Empty;


		//public string ElementLabelRole = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementType = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementDRCR = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementPeriodType = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string ElementDefinition = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string UnitName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string UnitPrecision = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public int UnitScaleFactor = 1;

		/// <summary>
		/// 
		/// </summary>
		public string Segments = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string Scenarios = string.Empty;

		/// <summary>
		/// Constructs a new instance of <see cref="MarkupReportItem"/>
		/// </summary>
		public MarkupReportItem()
		{
		}
	}
}

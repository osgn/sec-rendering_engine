using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Net;

namespace Aucent.MAX.AXE.Common.Data
{
	public class RivetFilingFeed
	{
		private string description = string.Empty;
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		private string errorMessage = string.Empty;
		public string ErrorMessage
		{
			get { return errorMessage; }
			set { errorMessage = value; }
		}

		private List<RivetFiling> filings = new List<RivetFiling>();
		public List<RivetFiling> Filings
		{
			get { return filings; }
			set { filings = value; }
		}

		private string link;
		public string Link
		{
			get { return link; }
			set { link = value; }
		}

		[XmlIgnore]
		public bool Loaded
		{
			get { return string.IsNullOrEmpty( this.ErrorMessage ); }
		}

		private DateTime published = DateTime.Now;
		public DateTime Published
		{
			get { return published; }
			set { published = value; }
		}

		private string title = string.Empty;
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public RivetFilingFeed()
		{
		}

		public static RivetFilingFeed Load( string path )
		{
			RivetFilingFeed feed = new RivetFilingFeed();

			if( path.StartsWith( "http", StringComparison.InvariantCultureIgnoreCase ) )
			{
				string tmp = Path.GetTempFileName();
				using( WebClient cli = new WebClient() )
				{
					cli.DownloadFile( path, tmp );
				}
				path = tmp;
			}

			using( FileStream fs = new FileStream( path, FileMode.Open, FileAccess.Read ) )
			{
				XmlSerializer s = new XmlSerializer( typeof( RivetFilingFeed ) );
				feed = (RivetFilingFeed)s.Deserialize( fs );
			}

			return feed;
		}

		private bool TryLoadXML( string uri )
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load( uri );
				
				//The root node is just a wrapper
				foreach( XmlNode filing in xDoc.DocumentElement.ChildNodes )
				{
					RivetFiling f = new RivetFiling( filing );
					this.Filings.Add( f );
				}

				return true;
			}
			catch( Exception ex )
			{
				this.ErrorMessage = ex.Message;
			}

			return false;
		}
	}

	public class RivetFiling
	{
		private string cik = string.Empty;
		public string CIK
		{
			get { return cik; }
			set { cik = value; }
		}

		private string company = string.Empty;
		public string Company
		{
			get { return company; }
			set { company = value; }
		}

		private string accession = string.Empty;
		public string Accession
		{
			get { return accession; }
			set { accession = value; }
		}

		private DateTime filed = DateTime.MinValue;
		public DateTime Filed
		{
			get { return filed; }
			set { filed = value; }
		}

		private string instancePath = string.Empty;
		public string InstancePath
		{
			get { return instancePath; }
			set { instancePath = value; }
		}

		private string taxonomyPath = string.Empty;
		public string TaxonomyPath
		{
			get { return taxonomyPath; }
			set { taxonomyPath = value; }
		}

		public RivetFiling()
		{

		}

		public RivetFiling( XmlNode filing )
		{
			this.Accession = GetNodeValue( filing, "Accession" );
			this.CIK = GetNodeValue( filing, "CIK" );
			this.Company = GetNodeValue( filing, "Company" );
			this.InstancePath = GetNodeValue( filing, "InstancePath" );
			this.TaxonomyPath = GetNodeValue( filing, "TaxonomyPath" );

			string filed = GetNodeValue( filing, "Filed" );
			this.Filed = DateTime.Parse( filed );
		}

		public static string GetNodeValue( XmlNode parent, string nodeName )
		{
			XmlNode child = parent.SelectSingleNode( "*[ name() = '"+ nodeName +"' ]" );
			if( child == null ||
				child.FirstChild == null )
				return string.Empty;

			return child.FirstChild.Value;
		}
	}
}

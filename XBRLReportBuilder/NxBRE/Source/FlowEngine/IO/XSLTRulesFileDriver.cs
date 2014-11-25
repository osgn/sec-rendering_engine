namespace NxBRE.FlowEngine.IO {
	using System;
	using System.IO;
	using System.Diagnostics;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Xsl;
	using System.Xml.XPath;
	
	using NxBRE.FlowEngine;
	using NxBRE.Util;
	
	/// <summary>
	/// Driver for loading rules, which first executes an XSLT.
	/// The resulting XML document must be valid against businessRules.xsd
	/// </summary>
	/// <author>David Dossot</author>
	/// <remarks>
	///  businessRules.xsd must be included in the assembly.
	/// </remarks>
	public class XSLTRulesFileDriver:AbstractRulesDriver {

		private string xslFileURI = null;
		protected XslCompiledTransform xslt = null;
		protected string inputXMLSchema = null;
		
		protected XSLTRulesFileDriver(string xmlFileURI):base(xmlFileURI) {}
		
		public XSLTRulesFileDriver(string xmlFileURI, XslCompiledTransform xslt):base(xmlFileURI) {
			if (xslt == null)
				throw new BRERuleFatalException("Null is not a valid XslTransform");

			this.xslt = xslt;
		}	
		
		public XSLTRulesFileDriver(string xmlFileURI, string xslFileURI):base(xmlFileURI) {
			if (xslFileURI == null)
				throw new BRERuleFatalException("Null is not a valid XSL File URI");

			this.xslFileURI = xslFileURI;
		}
		
		private XslCompiledTransform GetXSLT() {
			if (xslt == null) {
				if (Logger.IsFlowEngineInformation) Logger.FlowEngineSource.TraceEvent(TraceEventType.Information, 0, "XSLTRulesFileDriver loading " + xslFileURI);
				
				xslt = new XslCompiledTransform();
				xslt.Load(xslFileURI);
			}
			
			return xslt;
		}
		
		protected override XmlReader GetReader() {
			if( Logger.IsFlowEngineInformation )
			{
				Logger.FlowEngineSource.TraceEvent( TraceEventType.Information, 0, "XSLTRulesFileDriver loading " + xmlSource );
			}

			//Console.WriteLine( "Start" );

			MemoryStream stream = new MemoryStream();
			using( XmlReader fileReader = this.MyGetXmlInputReader( xmlSource, inputXMLSchema ) )
			{			
				XPathDocument xpDoc = new XPathDocument(fileReader);
				XslCompiledTransform tx = GetXSLT();
				tx.Transform( xpDoc, null, stream);
			}

		  	stream.Seek(0, SeekOrigin.Begin);

			return GetXmlInputReader(stream, "businessRules.xsd");
		}

		protected XmlReader MyGetXmlInputReader( string inputFile, string xsdResourceName )
		{
			XmlTextReader xmlReader = new XmlTextReader( inputFile );
			XmlSchemaSet sc = new XmlSchemaSet();

			using (Stream s = GetDeployedResourceStream(xsdResourceName))
			{
				XmlSchema xs = XmlSchema.Read( s, null );
				sc.Add( xs );
			}

			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.Schemas.Add( sc );
			return XmlReader.Create( xmlReader, xmlReaderSettings );
		}
	}
}


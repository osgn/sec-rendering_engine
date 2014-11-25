namespace NxBRE.FlowEngine.IO {
	using System;
	using System.IO;
	using System.Net;
	using System.Xml;
	using System.Xml.Schema;
	
	using NxBRE.FlowEngine;
	using NxBRE.Util;
	using Aucent.MAX.AXE.Common.Data.AssemblyIO;
	using System.Reflection;
	
	/// <summary>
	/// Driver for loading NxBRE rules from different sources.
	/// <see cref="NxBRE.FlowEngine.IO.IRulesDriver"/>
	/// </summary>
	/// <author>David Dossot</author>
	public abstract class AbstractRulesDriver:IRulesDriver {
		protected string xmlSource = null;

		///<summary>Builder pattern where the actual implementation is delegated to a descendant concrete class</summary>
		protected abstract XmlReader GetReader();
		
		
		public XmlReader GetXmlReader() {
			return GetReader();			
		}
		
		///<summary>The XML rule source (either an URI or a string containing an XML fragment)</summary>
		/// <param name="xmlSource">The URI of the rule file</param>
		protected AbstractRulesDriver(string xmlSource) {
			if (xmlSource == null)
				throw new BRERuleFatalException("Null is not a valid XML source");

			this.xmlSource = xmlSource;
		}
		
		protected AbstractRulesDriver() {}
		
		protected XmlReader GetXmlInputReader(XmlTextReader xmlReader, string xsdResourceName) {
			XmlReader sourceReader;
			
			if (xsdResourceName != null) {
				// we validate against a well defined schema
				sourceReader = Xml.NewValidatingReader(xmlReader, ValidationType.Schema, xsdResourceName);
			}
			else {
				// it is easier to by default be lax if no internal XSD resource has been given
				sourceReader = xmlReader;
			}
			
			return sourceReader;
		}
		
		protected XmlReader GetXmlInputReader(string sourceURI, string xsdResourceName) {
			return GetXmlInputReader(new XmlTextReader(sourceURI), xsdResourceName);
		}
		
		protected XmlReader GetXmlInputReader(Stream sourceStream, string xsdResourceName) {
			return GetXmlInputReader(new XmlTextReader(sourceStream), xsdResourceName);
		}

		protected Stream GetDeployedResourceStream(string resourceName)
		{
			//CEE - let's see if we can use a stream for the embedded resource instead...

			//Assembly.CodeBase returns the path in URI format, but we need the real path, so run the uri path through
            //Path.GetDirectory name to convert the path seperators to the system path separator, and pull off the assmembly
            //name.  Then get SubString(8) to pull off the f:/// portion of the string

			//string executionPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
			//executionPath = System.IO.Path.GetDirectoryName(executionPath);
			
			//string resFolder = Path.Combine( executionPath, "Resources" );

			//string fullResourceName = Path.Combine( resFolder, resourceName);
			//if( !File.Exists( fullResourceName ) )
			//    fullResourceName = Path.Combine( executionPath, resourceName);;

			//FileStream fs = new FileStream(fullResourceName, FileMode.Open, FileAccess.Read);

			//CEE - let's see if we can use a stream for the embedded resource instead...
			Assembly asm = Assembly.GetExecutingAssembly();
			AssemblyFS asmFS = new AssemblyFS( asm );

			string relResName = Path.Combine( "Resources", resourceName );
			Stream s = asmFS.File.Open( relResName );
			return s;
		}
	}
}


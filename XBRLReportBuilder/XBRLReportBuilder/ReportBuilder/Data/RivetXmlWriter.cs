using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using Aucent.FilingServices.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data
{
	public class RivetXmlWriter : XmlWriter
	{
		XmlWriter _baseWriter = null;

		public RivetXmlWriter( string outputFileName )
		{
			this._baseWriter = XmlWriter.Create( outputFileName );	
		}

		public RivetXmlWriter( Stream output )
		{
			this._baseWriter = XmlWriter.Create( output );
		}

		public RivetXmlWriter( Stream output, XmlWriterSettings settings )
		{
			this._baseWriter = XmlWriter.Create( output, settings );
		}

		public override void Close()
		{
			this._baseWriter.Close();
		}

		public override void Flush()
		{
			this._baseWriter.Flush();
		}

		public override string LookupPrefix( string ns )
		{
			return this._baseWriter.LookupPrefix( ns );
		}

		public override void WriteBase64( byte[] buffer, int index, int count )
		{
			this._baseWriter.WriteBase64( buffer, index, count );
		}

		public override void WriteCData( string text )
		{
			this._baseWriter.WriteCData( text );
		}

		public override void WriteCharEntity( char ch )
		{
			this._baseWriter.WriteCharEntity( ch );
		}

		public override void WriteChars( char[] buffer, int index, int count )
		{
			this._baseWriter.WriteChars( buffer, index, count );
		}

		public override void WriteComment( string text )
		{
			this._baseWriter.WriteComment( text );
		}

		public override void WriteDocType( string name, string pubid, string sysid, string subset )
		{
			this._baseWriter.WriteDocType( name, pubid, sysid, subset );
		}

		public override void WriteEndAttribute()
		{
			this._baseWriter.WriteEndAttribute();
		}

		public override void WriteEndDocument()
		{
			this._baseWriter.WriteEndDocument();
		}

		public override void WriteEndElement()
		{
			this._baseWriter.WriteEndElement();
		}

		public override void WriteEntityRef( string name )
		{
			this._baseWriter.WriteEntityRef( name );
		}

		public override void WriteFullEndElement()
		{
			this._baseWriter.WriteFullEndElement();
		}

		public override void WriteProcessingInstruction( string name, string text )
		{
			this._baseWriter.WriteProcessingInstruction( name, text );
		}

		public override void WriteRaw( string data )
		{
			if( this._baseWriter.Settings != null && string.Equals( this._baseWriter.Settings.Encoding.BodyName, Encoding.ASCII.BodyName ) )
				this.WriteRawCarefully( data );
			else
				this._baseWriter.WriteRaw( data );
		}

		public void WriteRawCarefully( string data )
		{
			foreach( char c in data )
			{
				int ord = (int)c;
				if( ord > 127 )
				{
					string hex = string.Format( "&#x{0};", ord.ToString( "X" ) );
					this._baseWriter.WriteRaw( hex );
				}
				else
				{
					this._baseWriter.WriteRaw( c.ToString() );
				}
			}
		}

		public override void WriteRaw( char[] buffer, int index, int count )
		{
			this._baseWriter.WriteRaw( buffer, index, count );
		}

		public override void WriteStartAttribute( string prefix, string localName, string ns )
		{
			this._baseWriter.WriteStartAttribute( prefix, localName, ns );
		}

		public override void WriteStartDocument( bool standalone )
		{
			this._baseWriter.WriteStartDocument( standalone );
		}

		public override void WriteStartDocument()
		{
			this._baseWriter.WriteStartDocument();
		}

		public override void WriteStartElement( string prefix, string localName, string ns )
		{
			this._baseWriter.WriteStartElement( prefix, localName, ns );
		}

		public override WriteState WriteState
		{
			get { return this._baseWriter.WriteState; }
		}

		public override void WriteString( string text )
		{
			if( this._baseWriter.Settings != null && string.Equals( this._baseWriter.Settings.Encoding.BodyName, Encoding.ASCII.BodyName ) )
				this.WriteStringCarefully( text );
			else
				this._baseWriter.WriteString( text );
		}

		public void WriteStringCarefully( string text )
		{
			foreach( char c in text )
			{
				int ord = (int)c;
				if( ord > 127 )
				{
					string hex = string.Format( "&#x{0};", ord.ToString( "X" ) );
					this._baseWriter.WriteRaw( hex );
				}
				else
				{
					this._baseWriter.WriteString( c.ToString() );
				}
			}
		}

		public override void WriteSurrogateCharEntity( char lowChar, char highChar )
		{
			this._baseWriter.WriteSurrogateCharEntity( lowChar, highChar );
		}

		public override void WriteWhitespace( string ws )
		{
			this._baseWriter.WriteWhitespace( ws );
		}
	}
}

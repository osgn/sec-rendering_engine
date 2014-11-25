/*****************************************************************************
 * XmlUtilities (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * A set of static methods that wrap and simplify the XML serialization classes in .NET
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Aucent.FilingServices.RulesRepository
{
	public abstract class XmlUtilities
	{
		public static bool TryXmlSerializeObjectToFile(string strFileName, object obj, out string strError)
		{
			bool ret;
			strError = string.Empty;
			FileStream fsWrite = null;
			try
			{
				fsWrite = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
				ret = TryXmlSerializeObjectToFile(fsWrite, obj, out strError);
			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return false;
			}
			finally
			{
				if (fsWrite != null) fsWrite.Close();
			}
			return ret;
		}

		public static bool TryXmlSerializeObjectToFile(FileStream fsWrite, object obj, out string strError)
		{
			strError = string.Empty;
			try
			{
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
				serializer.Serialize(fsWrite, obj);
			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return false;
			}
			return true;
		}

		public static bool TryXmlSerializeObjectToString(object obj, out string xmlString, out string strError)
		{
			xmlString = null;
			strError = null;
			MemoryStream msWrite = null;
			try
			{
				msWrite = new MemoryStream();
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
				serializer.Serialize(msWrite, obj);
				xmlString = ASCIIEncoding.ASCII.GetString(msWrite.ToArray());
				return true;
			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return false;
			}
			finally
			{
				if (msWrite != null) msWrite.Close();
			}
		}


		public static bool TryXmlDeserializeObjectFromFile(string strFileName, Type type, out object obj, out string errorMsg)
		{
			obj = null;
			errorMsg = null;

			try
			{
				using( FileStream fsRead = new FileStream( strFileName, FileMode.Open, FileAccess.Read ) )
				{
					return TryXmlDeserializeObjectFromFile( fsRead, type, out obj, out errorMsg );
				}
			}
			catch( Exception ex )
			{
				errorMsg = ex.Message;
			}
			return false;
		}

		public static bool TryXmlDeserializeObjectFromFile( Stream xmlStream, Type type, out object obj, out string errorMsg )
		{
			errorMsg = null;
			obj = null;

			try
			{
				XmlSerializer serializer = new XmlSerializer( type );
				obj = serializer.Deserialize( xmlStream );
				return true;
			}
			catch( Exception ex )
			{
				errorMsg = ex.Message;
			}

			return false;
		}

		public static bool TryXmlDeserializeObjectFromString(string xmlString, Type type, out object obj, out string errorMsg)
		{
			obj = null;
			errorMsg = null;

			if (string.IsNullOrEmpty(xmlString))
			{
				return true;
			}

			MemoryStream msReader = null;
			try
			{
				msReader = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xmlString));
				XmlSerializer serializer = new XmlSerializer(type);
				obj = serializer.Deserialize(msReader);
				return true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
				return false;
			}
			finally
			{
				if (msReader != null) msReader.Close();
			}
		}
	}
}

// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.Xml;
using System.Collections;

using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// XBRLCustomTypeCreator
	/// </summary>
	public class XBRLCustomTypeCreator
	{
		#region Public methods
		/// <summary>
		/// Returns a new <see cref="XBRLSimpleType"/>
		/// </summary>
		/// <param name="node">Not used.</param>
		/// <returns>A new <see cref="XBRLSimpleType"/>.</returns>
		public static IXBRLCustomType CreateCustomType(XmlNode node)
		{
			return new XBRLSimpleType();
		}

		/// <summary>
		/// Returns a new <see cref="XBRLSimpleType"/>.
		/// </summary>
		/// <param name="name">A recognized XBRL simple type name (e.g., xsd:decimal").</param>
		/// <returns>A new <see cref="XBRLSimpleType"/> if <paramref name="name"/> is a valid
		/// XBRL data type.  Null otherwise.</returns>
		public static XBRLSimpleType CreateSimpleType(string name)
		{
			XBRLSimpleType ret = new XBRLSimpleType();

			switch (name)
			{
				case "xsd:string":
					break;

				case "xsd:decimal":
					break;

				case "xsd:boolean":
					break;

				case "xsd:float":
					break;

				case "xsd:double":
					break;

				case "xsd:dateTime":
					break;

				case "xsd:time":
					break;

				case "xsd:date":
					break;

				case "xsd:anyURI":
					break;

				case "xsd:interger":
					break;

				case "xsd:nonPositiveInteger":
					break;

				case "xsd:negativeInteger":
					break;

				case "xsd:nonNegativeInteger":
					break;

				case "xsd:positiveInteger":
					break;

				case "xsd:long":
					break;

				case "xsd:integer":
					break;

				case "xsd:int":
					break;

				case "xsd:short":
					break;

				case "xsd:byte":
					break;

				case "xsd:unsignedLong":
					break;

				case "xsd:unsignedInt":
					break;

				case "xsd:unsignedShort":
					break;

				case "xsd:unsignedByte":
					break;

				default:
					ret = null;
					break;
			}

			return ret;
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="XBRLCustomTypeCreator"/>.
		/// </summary>
		public XBRLCustomTypeCreator()
		{
		}

		#endregion

	}

	/// <summary>
	/// Encapsulates methods and properties associated with an XBRL simple type.
	/// </summary>
	public class XBRLSimpleType : XBRLCustomTypeBase, IXBRLCustomType
	{
		#region IXBRLCustomType Members

		/// <summary>
		/// Deprecated.  Do not use.
		/// </summary>
		/// <param name="data">Not used.</param>
		/// <param name="error">Not used.</param>
		/// <returns>False.</returns>
		public bool IsValid(string data, out string error)
		{
			// TODO:  Add XBRLSimpleType.IsValid implementation
			error = null;
			return false;
		}

		#endregion
	}

	/// <summary>
	/// Encapsulates methods and properties associated with an XBRL complex type.
	/// </summary>
	public class XBRLComplexType : XBRLCustomTypeBase, IXBRLCustomType
	{
		#region IXBRLCustomType Members

		/// <summary>
		/// Deprecated.  Do not use.
		/// </summary>
		/// <param name="data">Not used.</param>
		/// <param name="error">Not used.</param>
		/// <returns>False.</returns>
		public bool IsValid(string data, out string error)
		{
			// TODO:  Add XBRLComplexType.IsValid implementation
			error = null;
			return false;
		}

		#endregion
	}


	/// <summary>
	/// Encapsulates methods and properties associated with an XBRL undefined type.
	/// </summary>
	public class XBRLUndefinedType : XBRLCustomTypeBase, IXBRLCustomType
	{
		#region IXBRLCustomType Members

		/// <summary>
		/// Deprecated.  Do not use.
		/// </summary>
		/// <param name="data">Not used.</param>
		/// <param name="error">Not used.</param>
		/// <returns>False.</returns>
		public bool IsValid(string data, out string error)
		{
			// TODO:  Add XBRLUndefinedType.IsValid implementation
			error = null;
			return false;
		}

		#endregion
	}

	/// <summary>
	/// Base class from which other XBRL data type classes inherit.
	/// </summary>
	abstract public class XBRLCustomTypeBase
	{
	}

}
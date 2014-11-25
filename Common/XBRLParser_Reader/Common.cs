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
using System.Diagnostics;
using System.Collections;

using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Common
	/// </summary>
	public class Common
	{
        
		#region properties
		static TraceSwitch traceSwitch = null;

		/// <summary>
		/// The "tracing level"--the severity level at which message are to be traced.
		/// </summary>
		public static TraceSwitch MyTraceSwitch
		{
			get { return traceSwitch; }
			set { traceSwitch = value; }
		}

		static bool includeProcessInfo = false;
		internal static bool IncludeProcessInfo
		{
			get { return includeProcessInfo; }
			set { includeProcessInfo = value; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Common.
		/// </summary>
		public Common()
		{
		}

		#endregion

		/// <summary>
		/// Retrieves a named attribute from a supplied <see cref="XmlNode"/>.
		/// </summary>
		/// <param name="node">The <see cref="XmlNode"/> in whose attributes method is to search for 
		/// the attribute with the name specified in <paramref name="attrTag"/>.</param>
		/// <param name="attrTag">The attribute name for which method is to search in <paramref name="node"/>.</param>
		/// <param name="val">A reference parameter into which method will place the attribute value.  <paramref name="val"/>
		///  will be updated if the attribute is found and <paramref name="errorList"/> is not null.</param>
		/// <param name="errorList">A collection of <see cref="ParserMessage"/> into which method will place
		///  a warning if the attribute cannot be found.</param>
		/// <returns>False if the attribute cannot be found.  True otherwise.</returns>
		public static bool GetAttribute( XmlNode node, string attrTag, ref string val, ArrayList errorList )
		{
			XmlNode attr = node.Attributes.GetNamedItem( attrTag );
			if ( attr == null )
			{
				if ( errorList != null )
				{
					val = null;

					WriteWarning( "XBRLParser.Warning.NoTagForLocator2", errorList, attrTag, node.OuterXml );
				}

				return false;
			}

			val = attr.Value;

			return true;
		}

		#region Info
		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as an information-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="objs">An <see cref="Array"/> of objects that is to be substituted via <see cref="String.Format(String, Object[])"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteInfo(string key, ArrayList errorList, params string[] objs)
		{
			WriteMsg( key, TraceLevel.Info, errorList, objs );
		}

		/// <summary>
		/// Method is deprecated and should not be used.
		/// </summary>
		/// <param name="key">Not used.</param>
		internal static void WriteInfo(string key)
		{
			WriteMsg(key, TraceLevel.Info, null);
		}

		/// <summary>
		/// Method is deprecated and should not be used.
		/// </summary>
		/// <param name="key">Not used.</param>
		/// <param name="obj">Not used.</param>
		internal static void WriteInfo(string key, string obj)
		{
			WriteMsg(key, TraceLevel.Info, null, obj);
		}
		#endregion

		#region Errors
		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as an error-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="arg0">An object that is to be substituted via <see cref="String.Format(String, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteError(string key, ArrayList errorList, string arg0)
		{
			WriteMsg( key, TraceLevel.Error, errorList, arg0 );
		}

		/// <summary>
		/// Retrieves a localized string resource 
		/// and appends that resource, as an error-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteError(string key, ArrayList errorList)
		{
			WriteMsg( key, TraceLevel.Error, errorList );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as an error-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="arg0">An object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="arg1">A second object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteError(string key, ArrayList errorList, string arg0, string arg1)
		{
			WriteMsg( key, TraceLevel.Error, errorList, arg0, arg1 );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as an error-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="arg0">An object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="arg1">A second object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="arg2">A third object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteError(string key, ArrayList errorList, string arg0, string arg1, string arg2)
		{
			WriteMsg( key, TraceLevel.Error, errorList, arg0, arg1, arg2 );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as an error-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="objs">An <see cref="Array"/> of objects that is to be substituted via <see cref="String.Format(String, Object[])"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteError(string key, ArrayList errorList, params string[] objs)
		{
			WriteMsg( key, TraceLevel.Error, errorList, objs );
		}
		#endregion

		#region Warnings
		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a warning-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="obj">An object that is to be substituted via <see cref="String.Format(String, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteWarning(string key, ArrayList errorList, string obj)
		{
			WriteMsg( key, TraceLevel.Warning, errorList, obj );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a warning-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="obj1">An object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj2">A second object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteWarning(string key, ArrayList errorList, string obj1, string obj2)
		{
			WriteMsg( key, TraceLevel.Warning, errorList, obj1, obj2 );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a warning-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="obj1">An object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj2">A second object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj3">A third object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteWarning(string key, ArrayList errorList, string obj1, string obj2, string obj3)
		{
			WriteMsg( key, TraceLevel.Warning, errorList, obj1, obj2, obj3 );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a warning-level <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="objs">An <see cref="Array"/> of objects that is to be substituted via <see cref="String.Format(String, Object[])"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteWarning(string key, ArrayList errorList, params string[] objs)
		{
			WriteMsg( key, TraceLevel.Warning, errorList, objs );
		}
		#endregion

		#region helpers
		/// <summary>
		/// Retrieves a localized string resource 
		/// and appends that resource, as a <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="level">The <see cref="TraceLevel"/> to be associated with the retrieved 
		/// string resource within the <see cref="ParserMessage"/>.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteMsg(string key, TraceLevel level, ArrayList errorList)
		{
			string msg = TraceUtility.FormatStringResource( key );

			if ( errorList != null )
			{
				errorList.Add( new ParserMessage( level, msg ) );
			}

//			WriteMsg( level, msg );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="level">The <see cref="TraceLevel"/> to be associated with the retrieved 
		/// string resource within the <see cref="ParserMessage"/>.</param>
		/// <param name="obj">An object that is to be substituted via <see cref="String.Format(String, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteMsg(string key, TraceLevel level, ArrayList errorList, string obj)
		{
			string msg = TraceUtility.FormatStringResource( key, obj );

			if ( errorList != null )
			{
				errorList.Add( new ParserMessage( level, msg ) );
			}

//			WriteMsg( level, msg );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="level">The <see cref="TraceLevel"/> to be associated with the retrieved 
		/// string resource within the <see cref="ParserMessage"/>.</param>
		/// <param name="obj">An object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj2">A second object that is to be substituted via <see cref="String.Format(String, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteMsg(string key, TraceLevel level, ArrayList errorList, string obj, string obj2)
		{
			string msg = TraceUtility.FormatStringResource( key, obj, obj2 );

			if ( errorList != null )
			{
				errorList.Add( new ParserMessage( level, msg ) );
			}

//			WriteMsg( level, msg );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="level">The <see cref="TraceLevel"/> to be associated with the retrieved 
		/// string resource within the <see cref="ParserMessage"/>.</param>
		/// <param name="obj">An object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj2">A second object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <param name="obj3">A third object that is to be substituted via <see cref="String.Format(String, Object, Object, Object)"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteMsg(string key, TraceLevel level, ArrayList errorList, string obj, string obj2, string obj3)
		{
			string msg = TraceUtility.FormatStringResource( key, obj, obj2, obj3 );

			if ( errorList != null )
			{
				errorList.Add( new ParserMessage( level, msg ) );
			}

//			WriteMsg( level, msg );
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// and appends that resource, as a <see cref="ParserMessage"/>, into a parameter-supplied <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="errorList">The <see cref="ArrayList"/> to which the retrieved string
		/// resources, encapsulated within a <see cref="ParserMessage"/>, is to be appended.</param>
		/// <param name="level">The <see cref="TraceLevel"/> to be associated with the retrieved 
		/// string resource within the <see cref="ParserMessage"/>.</param>
		/// <param name="objs">An <see cref="Array"/> of objects that is to be substituted via <see cref="String.Format(String, Object[])"/>,
		/// into the retrieved string.</param>
		/// <remarks>Message is not <u>not</u> written anywhere.  It is only appended to the 
		/// supplied <see cref="ArrayList"/>.</remarks>
		public static void WriteMsg(string key, TraceLevel level, ArrayList errorList, params string[] objs)
		{
			string msg = TraceUtility.FormatStringResource( key, objs );

			if ( errorList != null )
			{
				errorList.Add( new ParserMessage( level, msg ) );
			}

//			WriteMsg( level, msg );
		}

//		public static void WriteMsg( TraceLevel level, string msg )
//		{
//			TraceUtility.IncludeProcessInfo = includeProcessInfo;
//			TraceUtility.WriteLineIf( traceSwitch == null ? TraceLevel.Verbose : traceSwitch.Level, level, msg );
//
//		}
		#endregion
	}
}
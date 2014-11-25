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
using System.Diagnostics;
using System.Reflection;


namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// ParserMessage
	/// </summary>
	[Serializable]
	public class ParserMessage : IComparable
	{
        
		#region properties
		TraceLevel level;

		/// <summary>
		/// The level (e.g., information, error) of this <see cref="ParserMessage"/>.
		/// </summary>
		public TraceLevel Level
		{
			get { return level; }
		}

		string msg;
		/// <summary>
		/// The text of this <see cref="ParserMessage"/>.
		/// </summary>
		public string Message
		{
			get { return msg; }
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="ParserMessage"/> object.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="ParserMessage"/> object.</returns>
		public override string ToString()
		{
			return msg;
		}

		private  static  string assemblyVersion = string.Empty;

		/// <summary>
		/// The .Net assembly version of the assembly that contains the code that is currently executing.
		/// </summary>
		public static string ExecutingAssemblyVersion
		{
			get
			{
				if (ParserMessage.assemblyVersion.Length == 0)
				{
					ParserMessage.assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				}

				return ParserMessage.assemblyVersion;
			}

			set { ParserMessage.assemblyVersion = value; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="ParserMessage"/>.
		/// </summary>
		public ParserMessage()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="ParserMessage"/>.
		/// </summary>
		/// <param name="levelArg">The level of be assigned to the newly created 
		/// <see cref="ParserMessage"/>.</param>
		/// <param name="msgArg">The text of the message.</param>
		public ParserMessage(TraceLevel levelArg, string msgArg)
		{
			level = levelArg;
			msg = msgArg;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="ParserMessage"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="ParserMessage"/>
		/// is to be compared.  Assumed to be a <see cref="ParserMessage"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="ParserMessage"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="Enum.CompareTo(Object)"/> 
		/// for the levels of the two <see cref="ParserMessage"/> objects.</remarks>
		public int CompareTo(object obj)
		{
			if ( !( obj is ParserMessage ) )
			{
				throw new ApplicationException( "ParserMessage.CompareTo received an unknown object. Object type: " + obj.ToString() );
			}

			//return Convert.ToInt32( Level ) - Convert.ToInt32( ((ParserMessage)obj).Level );
			return Level.CompareTo( ((ParserMessage)obj).Level );
		}

		#endregion
	}
}
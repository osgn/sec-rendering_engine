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
using System.Text;
using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// Utility for outputting formatted trace messages
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public sealed class TraceUtility
	{
		#region properties

        private static bool includeProcessInfo = true;

        #endregion

		#region public methods

        public static void WriteLineError(TraceLevel srvrLevel, string key, params object[] formatParams)
        {
            WriteLineIf(srvrLevel, TraceLevel.Error, key, formatParams);
        }

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="formatParam">An <see cref="Array"/> of objects that is to be substituted 
		/// via <see cref="String.Format"/>, into the retrieved string.</param>
		/// <returns>The retrieved and formatted string resource.</returns>
		public static string FormatStringResource(string key, params object[] formatParams)
		{
			string resource = StringResourceUtility.GetString(key);

			return formatParams != null ? string.Format(resource, formatParams) : resource;
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="formatParam">An object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <param name="fp2">A second object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <param name="fp3">A third object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <returns>The retrieved and formatted string resource.</returns>
		public static string FormatStringResource(string key, object formatParam, object fp2, object fp3)
		{
			string resource = StringResourceUtility.GetString(key);

			return string.Format(resource, formatParam, fp2, fp3);
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="formatParam">An object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <param name="fp2">A second object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <returns>The retrieved and formatted string resource.</returns>
		public static string FormatStringResource(string key, object formatParam, object fp2)
		{
			string resource = StringResourceUtility.GetString(key);

			return string.Format(resource, formatParam, fp2);
		}

		/// <summary>
		/// Retrieves and performs parameter substitution on a localized string resource 
		/// based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <param name="formatParam">An object that is to be substituted via <see cref="String.Format"/>,
		/// into the retrieved string.</param>
		/// <returns>The retrieved and formatted string resource.</returns>
		public static string FormatStringResource(string key, object formatParam)
		{
			string resource = StringResourceUtility.GetString(key);

			return string.Format(resource, formatParam);
		}

		/// <summary>
		/// Retrieves a localized string resource based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <returns>The retrieved string resource.</returns>
		public static string FormatStringResource(string key)
		{
			return StringResourceUtility.GetString(key);
		}


		#endregion

		#region private methods

        /// <summary>
        /// Common trace output method
        /// </summary>
        /// <param name="level">
        /// Param to identify what trace level is to be output
        /// </param>
        /// <param name="bUseCommonResource">
        /// Flag to determine which string resource to load the string from
        /// </param>
        /// <param name="key">
        /// The string resource to find
        /// </param>
        /// <param name="formatParams">
        /// List of optional parameters for string formatting
        /// </param>
        private static void WriteLineIf(TraceLevel serverLevel, TraceLevel msgLevel, string key, params object[] formatParams)
        {
            if (serverLevel >= msgLevel)
            {
                WriteIt(FormatStringResource(key, formatParams));
            }
        }

        private static void WriteIt(string msg)
        {
            StringBuilder output = new StringBuilder();

            if (includeProcessInfo)
            {
                output.Append(AppDomain.CurrentDomain.FriendlyName);
                output.Append(", Process ID: ");
                output.Append(Process.GetCurrentProcess().Id);
                output.Append(", Thread ID: ");
                output.Append(System.Threading.Thread.CurrentThread.ManagedThreadId);
                output.Append(", ");
            }

            if ((msg != null) && (msg.Length > 0))
            {
                output.Append(msg).Append(Environment.NewLine);
            }

            //Trace.WriteLine( output.ToString() );

            RivetCustomEventLogSource Logger = new RivetCustomEventLogSource(AucentGeneral.RIVET_EVENT_LOG,
                AucentGeneral.PRODUCT_NAME, Environment.MachineName);
            if (Logger != null)
            {
                Logger.RivetWriteEntry(AucentGeneral.PRODUCT_NAME, output.ToString(), EventLogEntryType.Information);
            }
            Trace.Flush();
        }


		#endregion		

	}
}
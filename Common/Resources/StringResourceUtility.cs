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
using System.Resources;
using System.Reflection;
using System.Text;
using System.Collections;

namespace Aucent.MAX.AXE.Common.Resources
{
	/// <summary>
	/// Utility for fetching localized strings
	/// </summary>
	/// <remarks>
	/// Call StringResourceUtility.GetString(key)
	/// to fetch a localized string as listed in .resx file 
	/// </remarks>
	public sealed class StringResourceUtility
	{
		public const string unknownStringResource = "Unknown String Resource";
        
		#region properties

		/// <summary>
		/// The one and only string resource manager
		/// </summary>
		static ResourceManager resourceManager = null;

		/// <summary>
		/// Name of default, embedded resource for strings
		/// </summary>
		static string resourceName = "Aucent.MAX.AXE.Common.Resources.StringResources";

		#endregion

		#region constructors

		#endregion

		#region public methods

		/// <summary>
		/// Retrieves a localized string resource based on caller's current culture settings.
		/// </summary>
		/// <param name="key">
		/// Name of resource string to retrieve.
		/// </param>
		/// <returns>The retrieved string resource.</returns>
		public static string GetString(string key)
		{
			if( (key != null) && (key != string.Empty) )
			{
				if(resourceManager == null)
				{
					// create ResourceManager
					resourceManager = new ResourceManager(
						resourceName,
						Assembly.GetExecutingAssembly() );
				}

				// look up resource
				string resource = resourceManager.GetString(key);
				if(resource != null)
				{
					return resource;
				}
			}
	
			StringBuilder errorMessage = new StringBuilder();
			errorMessage.Append(unknownStringResource);
			if( (key != null) && (key != string.Empty) )
			{
				errorMessage.Append(", Key: ").Append(key);
			}

			return errorMessage.ToString();
		}

		#endregion

		#region private methods

		#endregion		

		#region Code Implementing Multiple Resource Files - NOT IMPLEMENTED CURRENTLY
		
//		/// <summary>
//		/// Collection of the various difference resource managers
//		/// </summary>
//		private static Hashtable resourceManagers = new Hashtable();
//		private static Hashtable ResourceManagers
//		{
//			get {return Hashtable.Synchronized(resourceManagers);}
//		}
//
//		private static string resourceNamespace = "Aucent.MAX.AXE.Common.Resources.";
//
//		/// <summary>
//		/// Name of default, embedded resource for strings
//		/// </summary>
//		private static string resourceName = "StringResources";
//
//		/// <summary>
//		/// Name of the resource specific to this executing assembly.
//		/// </summary>
//		public static string assemblyResourceName = Assembly.GetExecutingAssembly().GetName().Name;
//
//		/// <summary>
//		/// Creates a new StringResourceUtility.
//		/// </summary>
//		static StringResourceUtility()
//		{
//			resourceName = resourceNamespace + resourceName;
//			assemblyResourceName = resourceNamespace + assemblyResourceName;
//		}
//
//		/// <summary>
//		/// Retrieves localized string resource
//		/// based on caller's current culture settings
//		/// </summary>
//		/// <param name="bUseCommon">
//		/// Flag to determine which resource to look in for key
//		/// </param>
//		/// <param name="key">
//		/// Name of resource string to retrieve
//		/// </param>
//		/// <returns></returns>
//		public static string GetString(bool bUseCommon, string key)
//		{
//			if( (key != null) && (key != string.Empty) )
//			{
//				ResourceManager resMgr = null;
//				if (bUseCommon)
//				{
//					resMgr = ResourceManagers[resourceName] as ResourceManager;
//					if(resMgr == null)
//					{
//						// create ResourceManager & add it to hash
//						resMgr = new ResourceManager(
//							resourceName,
//							Assembly.GetExecutingAssembly() );
//
//						ResourceManagers[resourceName] = resMgr;
//					}
//				}
//				else
//				{
//					resMgr = ResourceManagers[assemblyResourceName] as ResourceManager;
//					if(resMgr == null)
//					{
//						// create ResourceManager & add it to hash
//						resMgr = new ResourceManager(
//							assemblyResourceName,
//							Assembly.GetExecutingAssembly() );
//
//						ResourceManagers[assemblyResourceName] = resMgr;
//					}
//				}
//
//				// look up resource
//				string resource = resMgr.GetString(key);
//				if(resource != null)
//				{
//					return resource;
//				}
//			}
//	
//			StringBuilder errorMessage = new StringBuilder();
//			errorMessage.Append("Unknown String Resource");
//			if( (key != null) && (key != string.Empty) )
//			{
//				errorMessage.Append(", Key: ").Append(key);
//			}
//
//			return errorMessage.ToString();
//		}
//
//		/// <summary>
//		/// Retrieves localized string resource
//		/// based on caller's current culture settings
//		/// Always gets string from Common resource file.
//		/// </summary>
//		/// <param name="key">
//		/// Name of resource string to retrieve
//		/// </param>
//		/// <returns></returns>
//		public static string GetString(string key)
//		{
//			// default to get string from Common resource
//			return GetString(true, key);
//		}

		#endregion

	}
}
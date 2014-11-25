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
using System.IO;
using System.Collections.Generic;


namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// Aucent
	/// </summary>
	public class AucentGeneral
	{

		public static string RIVET_NAME = "Rivet";
        public static string PRODUCT_NAME = "Crossfire";
		public static string RIVET_EVENT_LOG = "Rivet Logs";
		public static string RIVET_SOFTWARE = "Rivet Software";

		public static string DRAGON_TAG_FOLDER = "Dragon Tag";


		public const string RIVET_WEBSITE			= "http://www.rivetsoftware.com";
		public const string DRAGON_TAG_HELP = @"\DragonTag Help.chm";
		public const string DRAGON_TAG_DOWNLOADS = @"http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=46&navID=119";


		#region properties
		
		
	
		/// <summary>
		/// Returns the correctly concattinated SchemaPath+FileName. If the Path is URL or System, it will put the correct
		/// directory separator character and return a correct FileNamePath.
		/// </summary>
		/// <param name="SchemaPath">The SchemaPath</param>
		/// <param name="FileName">The FileName to append to it.</param>
		/// <returns>The correctly formatted FileNamePath</returns>
		public static string AppendFileNameToSchemaPath(string SchemaPath, string FileName)
		{
			if (SchemaPath == null || SchemaPath.Length == 0)
			{
				return FileName;
			}

			string FileNamePath = string.Empty;

			try
			{
				

				// Make sure the file name is clean and does not contain any of the 
				// directoy separator or URI spearator characters.
				//FileName = FileName.Replace(Path.DirectorySeparatorChar.ToString(), "");
				//FileName = FileName.Replace("/", "");

				// Make sure the SchemaPath is clean of any redundant / or \ characters @ the end of it.

				if (SchemaPath != null && SchemaPath.Length > 0)
				{
					while(	((	(SchemaPath.LastIndexOf(Path.DirectorySeparatorChar.ToString())	==	SchemaPath.Length-1))  ||
						(	(SchemaPath.LastIndexOf("/")									==	SchemaPath.Length-1))	))
						SchemaPath = SchemaPath.Substring(0, SchemaPath.Length-1);

					int BackSlashIndex = SchemaPath.LastIndexOf(Path.DirectorySeparatorChar);
					int ForwardSlashIndex = SchemaPath.LastIndexOf("/");

					// Append the FileName to the SchemaPath.
					//BUG 1417 - added = condition: if SchemaPath is a root drive BackSlash and ForwardSlash will both be -1
					if (BackSlashIndex >= ForwardSlashIndex)
						FileNamePath = SchemaPath + Path.DirectorySeparatorChar + FileName;
					else
						FileNamePath = SchemaPath + "/" + FileName;
				}
			}
			catch(Exception)
			{
				FileNamePath = string.Empty;
			}
			return FileNamePath;
		}
		/// <summary>
		/// Returns the full path for the Aucent' folder within the ApplicationData SpecialFolder. 
		/// </summary>
//		public static string AucentApplicationDataPath
//		{
//			get
//			{
//				string myPath = 
//					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + AUCENT;
//				if (!Directory.Exists(myPath))
//					Directory.CreateDirectory(myPath);
//				return myPath;
//			}
//		}

        /// <summary>
        /// Returns the full path for the Aucent 'XBRLAddin' folder within the ApplicationData SpecialFolder.
        /// </summary>
//		public static string AucentApplicationDataXBRLAddinPath
//		{
//			get
//			{
//				string myPath = 
//				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + AUCENT
//					+ Path.DirectorySeparatorChar + XBRL_ADDIN ;
//				if (!Directory.Exists(myPath))
//					Directory.CreateDirectory(myPath);
//				return myPath;
//			}
//		}
		

		/// <summary>
        /// Returns the full path for the Rivet 'Dragon Tag' folder within the ApplicationData SpecialFolder.
        /// </summary>
		public static string RivetApplicationDataDragonTagPath
		{
			get
			{
				string myPath =
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Rivet" 
					+ Path.DirectorySeparatorChar + DRAGON_TAG_FOLDER ;
				if (!Directory.Exists(myPath))
					Directory.CreateDirectory(myPath);
				return myPath;
			}
		}

		
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new Aucent.
		/// </summary>
		public AucentGeneral()
		{
		}


		#endregion


		#region File Mapping information
		private static Dictionary<string, string> FileMappingInformation = null;
		private static string MAPPING_FILE_NAME = @"DragonTag.FileMapping.xml";

		private static void LoadFileMappingInformation()
		{
			if (FileMappingInformation != null) return;
			string fileName = RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + MAPPING_FILE_NAME;
			FileMappingInformation = new Dictionary<string, string>();

			if (File.Exists(fileName))
			{
				string[] vals = RemoteFiles.XmlDeserializeObjectFromFile(fileName, typeof(string[])) as string[];
				if (vals != null)
				{
					for (int i = 1; i < vals.Length; i = i + 2)
					{
						FileMappingInformation[vals[i - 1]] = vals[i];
					}
				}
			}


			
		}

		public static void UpdateFileMappingInformation(Dictionary<string, string> newMappings)
		{
			LoadFileMappingInformation();

			foreach (KeyValuePair<string, string> kvp in newMappings)
			{
				FileMappingInformation[kvp.Key] = kvp.Value;
			}
			string fileName = RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + MAPPING_FILE_NAME;
			

			List<string> serializableObject = new List<string>();
			foreach (KeyValuePair<string, string> kvp in FileMappingInformation)
			{
				serializableObject.Add(Path.GetFileName(kvp.Key).ToLower());
				serializableObject.Add(kvp.Value);
			}
			RemoteFiles.XmlSerializeObjectToFile(fileName, serializableObject.ToArray());
		}

		public static bool TryGetFileMappingInfo( string origName, out string newName )
		{
			LoadFileMappingInformation();
			string fileName = Path.GetFileName(origName).ToLower();
			return FileMappingInformation.TryGetValue(fileName, out newName);

		}

		#endregion


		

	}
}
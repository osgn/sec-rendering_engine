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
using System.Xml;
using System.Text;
using System.Xml.Xsl;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using System.Net;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Utilities;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// The base class for key XBRL document objects (e.g., presentation linkbase, 
	/// reference linkbase, label linkbase).
	/// </summary>
	[Serializable]
	public abstract class DocumentBase
	{
		/// <summary>
		/// The URL for the World Wide Web Consortium XML schema definition.
		/// </summary>
		public const string XML_SCHEMA_URL = "http://www.w3.org/2001/XMLSchema";

		/// <summary>
		/// The XML namespace qualifier for the XML schema namespace.  
		/// See <see cref="XML_SCHEMA_URL"/>.
		/// </summary>
		private const string XML_SCHEMA_PREFIX = "xsd";

		/// <summary>
		/// The URL for XBRL International's XBRL linkbase definition namespace.
		/// </summary>
		public const string XBRL_LINKBASE_URL = "http://www.xbrl.org/2003/linkbase";

		/// <summary>
		/// The XML namespace qualifier for the XBRL linkbase definition namespace.  
		/// See <see cref="XBRL_LINKBASE_URL"/>.
		/// </summary>
		public const string XBRL_LINKBASE_PREFIX = "link";

		/// <summary>
		/// The URL for XBRL International's XBRL instance document definition namespace.
		/// </summary>
		public const string XBRL_INSTANCE_URL = "http://www.xbrl.org/2003/instance";

		/// <summary>
		/// The XML namespace qualifier for the XBRL instance document definition namespace.
		/// See <see cref="XBRL_INSTANCE_URL "/>.
		/// </summary>
		public const string XBRLI_PREFIX = "xbrli";

		/// <summary>
		/// The XML namespace qualifier for the World Wide Web Consortium XML XLink definition.  
		/// See <see cref="XLINK_URI"/>.
		/// </summary>
		public const string XLINK_PREFIX = "xlink";


		/// <summary>
		/// prefix used for inline xbr....
		/// </summary>
		public const string INLINE_XBRL_PREFIX = "ix";


		/// <summary>
		/// inline xbrl namespace......
		/// </summary>
		public const string INLINE_XBRL_URI = "http://www.xbrl.org/2008/inlineXBRL";


		/// <summary>
		/// prefix used for inline xbrl transformation....
		/// </summary>
		public const string INLINE_TRANS_PREFIX = "ixt";


		/// <summary>
		/// inline xbrl namespace......
		/// </summary>
		public const string INLINE_TRANS_URI = "http://www.xbrl.org/2008/inlineXBRL/transformation";



		/// <summary>
		/// The URL for the World Wide Web Consortium XML XLink definition.
		/// </summary>
		public const string XLINK_URI = "http://www.w3.org/1999/xlink";

		/// <summary>
		/// The XML namespace qualifier for XBRL International's XBRL dimension namespace.
		/// See <see cref="XBRLDT_URI"/>.
		/// </summary>
		public const string XBRLDT_PREFIX = "xbrldt";

		/// <summary>
		/// The URL for XBRL International's XBRL dimension namespace.
		/// </summary>
		public const string XBRLDT_URI = "http://xbrl.org/2005/xbrldt";

		/// <summary>
		/// The XML namespace qualifier for XBRL International's XBRL dimension namespace.
		/// See <see cref="XBRLDI_URI"/>.
		/// </summary>
		public const string XBRLDI_PREFIX = "xbrldi";

		/// <summary>
		/// The URL for XBRL International's XBRL dimension namespace.
		/// </summary>
		public const string XBRLDI_URI = "http://xbrl.org/2006/xbrldi";

		/// <summary>
		/// The name of the "targetNamespace" attribute that defines the namespace
		/// of elements within a schema.
		/// </summary>
		public const string TARGET_NAMESPACE = "targetNamespace";

        protected const string TARGET_LINKBASE_URI = "http://www.xbrl.org/2003/role";

		/// <summary>
		/// The suffix, applied to the xlink:role within a linkbaseRef element, to indicate the 
		/// presentation linkbase reference.
		/// </summary>
		/// <example>
		/// The base xlink:role of "http://www.xbrl.org/2003/role" is qualified with
		/// <see cref="PRESENTATION_ROLE"/> to create "http://www.xbrl.org/2003/role/presentationLinkbaseRef".
		/// </example>
        protected const string PRESENTATION_ROLE = "/presentationLinkbaseRef";

		/// <summary>
		/// The suffix, applied to the xlink:role within a linkbaseRef element, to indicate the 
		/// definition linkbase reference.
		/// </summary>
		/// <example>
		/// The base xlink:role of "http://www.xbrl.org/2003/role" is qualified with
		/// <see cref="DEFINISION_ROLE"/> to create "http://www.xbrl.org/2003/role/definitionLinkbaseRef".
		/// </example>
		protected const string DEFINISION_ROLE = "/definitionLinkbaseRef";

		/// <summary>
		/// The suffix, applied to the xlink:role within a linkbaseRef element, to indicate the 
		/// calculation linkbase reference.
		/// </summary>
		/// <example>
		/// The base xlink:role of "http://www.xbrl.org/2003/role" is qualified with
		/// <see cref="CALCULATION_ROLE"/> to create "http://www.xbrl.org/2003/role/calculationLinkbaseRef".
		/// </example>
		protected const string CALCULATION_ROLE = "/calculationLinkbaseRef";

		/// <summary>
		/// The suffix, applied to the xlink:role within a linkbaseRef element, to indicate the 
		/// label linkbase reference.
		/// </summary>
		/// <example>
		/// The base xlink:role of "http://www.xbrl.org/2003/role" is qualified with
		/// <see cref="LABEL_ROLE"/> to create "http://www.xbrl.org/2003/role/labelLinkbaseRef".
		/// </example>
		protected const string LABEL_ROLE = "/labelLinkbaseRef";

		/// <summary>
		/// The suffix, applied to the xlink:role within a linkbaseRef element, to indicate the 
		/// reference linkbase reference.
		/// </summary>
		/// <example>
		/// The base xlink:role of "http://www.xbrl.org/2003/role" is qualified with
		/// <see cref="REFERENCE_ROLE"/> to create "http://www.xbrl.org/2003/role/referenceLinkbaseRef".
		/// </example>
		protected const string REFERENCE_ROLE = "/referenceLinkbaseRef";
		/// <summary>
		/// The XML namespace qualifier for the XML schema instance namespace.  
		/// See <see cref="XSI_URI"/>.
		/// </summary>
		public const string XSI = "xsi";

		/// <summary>
		/// The URL for the World Wide Web Consortium XML schema instance namespace.
		/// </summary>
		public const string XSI_URI = "http://www.w3.org/2001/XMLSchema-instance";

		/// <summary>
		/// The XML namespace qualifier declaring namespaces.
		/// </summary>
		public const string XMLNS = "xmlns";

		/// <summary>
		/// A <see cref="String.Format(String, Object, Object)"/> format string, used for 
		/// creating an element ID consisting of a prefix and an element name.
		/// </summary>
		public const string ID_FORMAT = "{0}_{1}";

		/// <summary>
		/// A <see cref="String.Format(String, Object, Object)"/> format string, generally used for 
		/// creating a XML reference consisting of a namespace prefix and a URL.
		/// </summary>
		public const string NAME_FORMAT = "{0}:{1}";

		/// <summary>
		/// The name of the element attribute that defines the id of the element.
		/// </summary>
		public const string ID = "id";

		public System.Net.Cache.RequestCacheLevel CachePolicy = System.Net.Cache.RequestCacheLevel.Default;

		internal ArrayList errorList = new ArrayList();
		internal int numWarnings;

		internal string schemaFilename = null;
		internal string schemaFile		= null;
		internal string schemaPath		= null;
		internal string schemaPathURI	= null;

		/// <summary>
		/// Determines whether to prompt the user if load for some file is not setup properly
		/// </summary>
		public bool PromptUser= true;

		[NonSerialized()]
		internal XmlDocument theDocument = null;

		[NonSerialized()]
        protected XmlNamespaceManager theManager = null;

        /// <summary>
        /// the original tax that is used for loading this documentbase..
        /// use the additionalDirectoriesForLoad from the loadingTaxonomy...
        /// </summary>
        internal DocumentBase loadingTaxonomy = null;

		private string[] additionalDirectoriesForLoad = null;

		/// <summary>
		/// A collection of additional folders in which to search for instance documents and taxonomies.
		/// </summary>
		public string[] AdditionalDirectoriesForLoad
		{
            get { return this.additionalDirectoriesForLoad; }
            set { this.additionalDirectoriesForLoad = value; }
		}

		#region properties

		/// <summary>
		/// The handle to the Windows window that is to own any dialogs displayed
		/// by this <see cref="DocumentBase"/>.
		/// </summary>
		public WindowWrapper OwnerHandle = new WindowWrapper();

		/// <summary>
		/// The fully-qualified file path and name for the XML schema underlying this
		/// <see cref="DocumentBase"/>.
		/// </summary>
		public string SchemaFile
		{
			get { return schemaFile; }
            set { schemaFile = value; }
		}

		/// <summary>
		/// The file name (no path) for the XML schema underlying this
		/// <see cref="DocumentBase"/>.
		/// </summary>
		public string SchemaFilename
		{
			get { return schemaFilename; }
		}

        public string SchemaPath
        {
            get { return schemaPath; }
        }

		/// <summary>
		/// A collection of <see cref="ParserMessage"/> that is the errors that have occurred
		/// while loading and parsing this document.
		/// </summary>
		public ArrayList ErrorList
		{
			get { return errorList; }
			set { errorList = value; }
		}

		/// <summary>
		/// The number of warnings encountered while loading and parsing this document.
		/// </summary>
		public int NumWarnings
		{
			get { return numWarnings; }
		}

		/// <summary>
		/// The <see cref="XmlDocument"/> object underlying this document.
		/// </summary>
		public XmlDocument TheDocument
		{
			get { return theDocument; }
			set { theDocument = value; }
		}
		#endregion

		#region Interfaces
		
		

		#endregion
		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="DocumentBase"/>.
		/// </summary>
		public DocumentBase()
		{
		}

		#endregion

		

		/// <summary>
		/// Reads and parses an XML file.  Populating the <see cref="XmlDocument"/> underlying 
		/// this <see cref="DocumentBase"/>.
		/// </summary>
		/// <param name="filename">The file that is the be read and parsed.</param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered
		/// during the load.</param>
		public virtual bool Load(string filename, out int numErrors)
		{
			numErrors = 0;
			try
			{
				numErrors = Load(filename);
				return (numErrors == 0);
			}
			catch ( Exception e )
			{
				Common.WriteError("XBRLParser.Error.Exception",  errorList, e.Message);
				++numErrors;
				
				return false;
			}
		}
		/// <summary>
		/// Validates a file's existence (Either a Windows file or a Web file)
		/// </summary>
		/// <param name="filename">The full filename path: C:\Folder\FileName.xsd or http://www.server.com/FileName.xsd </param>
		/// <param name="FileIsLocal">An output parameter.  True if the file exists on the local file system.</param>
		/// <param name="lastModified">An output parameter.  The <see cref="DateTime"/> on which the file was last modified (if
		/// it exists).</param>
		/// <param name="ResolvedLocalFilePath">The fully-qualified name and location of the file, if it exists.</param>
		/// <param name="setProperties">If true, the <see cref="schemaFilename"/> and <see cref="schemaPath"/> for this
		/// instance of <see cref="DocumentBase"/> will be initialized from <paramref name="filename"/>, if <paramref name="filename"/>
		/// is a URL.</param>
		/// <param name="URLExists">An output parameter.  True if the file exists as a URL.</param>
		/// <returns>True if the file exists.  False otherwise.</returns>
		public bool ValidateFileExistance(string filename,
			bool setProperties, out bool FileIsLocal,
			out string ResolvedLocalFilePath,
			out DateTime lastModified,
			out bool URLExists  )
		{

			if ( LookForFile( filename, setProperties, out FileIsLocal,
				out ResolvedLocalFilePath, out lastModified, out URLExists ))
			{
				return true;
			}
            string[] addnDirectories = this.GetAdditionalDirectoriesForLoad();
            if (addnDirectories != null && addnDirectories.Length > 0)
			{
                foreach (string dir in addnDirectories)
				{
					if ( dir == null || dir.Length == 0 )
						continue;

					string tempFileName = Path.Combine( dir, Path.GetFileName( filename ) ).Replace(@"\", @"/");
					if ( LookForFile( tempFileName, setProperties, out FileIsLocal,
						out ResolvedLocalFilePath, out lastModified, out URLExists ))
					{
						return true;
					}
				}
			}

			return false;
		}


        private string[] GetAdditionalDirectoriesForLoad()
        {
            if (this.loadingTaxonomy != null)
            {
                return loadingTaxonomy.additionalDirectoriesForLoad;
            }


            return this.additionalDirectoriesForLoad;
        }

        private void AddToAdditionalDirectoriesForLoad(string directory)
        {
            if (this.loadingTaxonomy != null)
            {
                if (loadingTaxonomy.additionalDirectoriesForLoad == null)
                {
                    loadingTaxonomy.additionalDirectoriesForLoad = new string[] { directory };
                }
                else
                {
                    List<string> tmp = new List<string>(loadingTaxonomy.additionalDirectoriesForLoad);
                    tmp.Add(directory);

                    loadingTaxonomy.additionalDirectoriesForLoad = tmp.ToArray();
                }
            }
            else
            {
                if (this.additionalDirectoriesForLoad == null)
                {
                    this.additionalDirectoriesForLoad = new string[] { directory };
                }
                else
                {
                    List<string> tmp = new List<string>(this.additionalDirectoriesForLoad);
                    tmp.Add(directory);

                    this.additionalDirectoriesForLoad = tmp.ToArray();
                }
            }


        }

		private bool LookForFile(string filename,
			bool setProperties, out bool FileIsLocal, 
			out string ResolvedLocalFilePath, out DateTime lastModified ,
			out bool URLExists)
		{
			URLExists		= false;
			bool LocalExists	= false;
			FileIsLocal			= LocalExists;
			ResolvedLocalFilePath = filename;

			// Determine if file is @ URL or local file
			RemoteFiles RF = new RemoteFiles();
			RF.CachePolicy = this.CachePolicy;
			if (URLExists = RF.CheckURLFileExists(filename, out lastModified ))
			{
				if ( setProperties )
				{
					this.schemaFilename	= System.IO.Path.GetFileName(filename);
					this.schemaPath		= filename.Replace(schemaFilename, string.Empty);
				}
				ResolvedLocalFilePath = filename;
			}
			// bug 705 - if we can't find the file, we just pass the filename (no directory or URL)
			// expecting it to fail. Sometimes, however, the FileInfo object will pick up the last
			// used directory from the OpenFileDialog dialog. To ensure that our file shouldn't be found
			// make sure the filename being passed in contains the :\ as in c:\ or \\ as in \\app1
			else //if ( filename.IndexOf( @":\" ) != -1 || filename.IndexOf( @"\\" ) != -1 )
			{
				// we changed this to use System.IO.File.Exists() so that we don't have to worry
				// about checking all the stuff above with the file name contents. 
				// Also, File.Exists() should not pick up the last used directory from OpenFileDialog...
				try
				{
//					FileInfo fi = new FileInfo(filename);
//					if (fi.Exists)
					if ( File.Exists( filename ) )
					{
						LocalExists = true;
						FileIsLocal = LocalExists;
						//schemaFilename = fi.Name;
						//schemaPath = fi.DirectoryName;

//						ResolvedLocalFilePath = fi.FullName;
						ResolvedLocalFilePath = Path.GetFullPath( filename );
					}
				}
				catch (System.ArgumentException)
				{
				}
				catch (System.NotSupportedException)
				{
					return false;
				}
			}

			// if the file is not found to exist either @ a URL location or a local location, 
			// figure out if the file was intended (by path) to be local.
			if (!(URLExists|LocalExists))
				FileIsLocal = (filename.LastIndexOf(Path.DirectorySeparatorChar) > 0); 

			return (URLExists|LocalExists);
		}

		/// <summary>
		/// Reads and parses an XML file.  Populating the <see cref="XmlDocument"/> underlying 
		/// this <see cref="DocumentBase"/>.
		/// </summary>
		/// <param name="filename">The file that is the be read and parsed.</param>
		/// <returns>The number of error encountered during the load process.</returns>
		public int Load(string filename)
		{
			return Load(filename, PromptUser);
		}


		private bool TryLoadWebFile(string filename)
		{
			string actualFileName = filename;
			theDocument = null;
			RemoteFiles rf = new RemoteFiles();
			rf.CachePolicy = this.CachePolicy;
			if (rf.LoadWebFile(filename, ref actualFileName,  out theDocument))
			{

				
				schemaFile = actualFileName;
				this.schemaFilename = System.IO.Path.GetFileName(actualFileName);
				this.schemaPath = actualFileName.Replace(schemaFilename, string.Empty);

				theManager = new XmlNamespaceManager(theDocument.NameTable);
				PopulateNamespaceManager(theManager, theDocument);
				SetDocumentXBRLIPrefix();

				return true;
			}

			return false;
		}

		/// <summary>
		/// Reads and parses an XML file.  Populating the <see cref="XmlDocument"/> underlying 
		/// this <see cref="DocumentBase"/>.
		/// </summary>
		/// <param name="filename">The file that is the be read and parsed.</param>
		/// <param name="promptUser">If true, method will interact with the user if error conditions are encountered.</param>
		/// <returns>The number of error encountered during the load process.</returns>
		public int Load(string filename, bool promptUser)
		{
			//a simpler method to load a web file ...
			//for valid web files, it should go through this function and be done...
			//not go through the maze of code that is below...
			if (TryLoadWebFile(filename))
			{
				return 0;
			}

			bool		usingRivetFile			= false;
			bool		FileExists				= false;
			bool		FileIsLocal				= false;
			FileInfo	IsolatedStorageFile		= null;
			int			numErrors				= 0;
			schemaFile							= filename;
			string		UserMessage				= string.Empty;

			if (filename == string.Empty || filename == null)
			{
				throw (new System.Exception("Aucent.MAX.AXE.XBRLParser.Load - Error: " + TraceUtility.FormatStringResource("XBRLParser.DocumentBase.FileNotFound")));
			}
			


			theDocument	= new XmlDocument();
			errorList.Clear();
			bool addToIsolatedStorage = false;
			DateTime lastModified = DateTime.MinValue;
			FileExists = ValidateFileExistance(filename, true,
				out FileIsLocal, out filename, out lastModified, out addToIsolatedStorage);

			if (addToIsolatedStorage)
			{
				IsolatedStorageFile = new FileInfo(AucentGeneral.RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + Path.GetFileName(filename));

				if (IsolatedStorageFile.Exists && !FileIsLocal)
				{
					if (IsolatedStorageFile.LastWriteTime.CompareTo(lastModified) < 0)
					{
						// the local one is old, no change to anything
						usingRivetFile = false;
					}
					else
					{
						usingRivetFile = true;
					}
				}
			}

			

			if ( FileExists )
			{
                schemaFile = filename;

				if (!usingRivetFile &&
					filename.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
				{
					WebClient client = new WebClient();
					try
					{
						IWebProxy iwp20 = WebRequest.DefaultWebProxy;
						Uri uri = new Uri(filename);
						Uri proxyUri = iwp20.GetProxy(uri);
						WebProxy theProxy = new WebProxy(proxyUri);
						if (!string.Equals(theProxy.Address.AbsoluteUri, uri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
						{
							theProxy.UseDefaultCredentials = true;
							client.Proxy = theProxy;
						}
						string content = client.DownloadString(filename);
						theDocument.LoadXml(content);
					}
					catch (Exception)
					{
						//the file is not a valid xml file
						FileExists = false;
					}
					finally
					{
						client.Dispose();
					}
				}
				else
				{

					// Using XmlUrlResolver will allow us to access 'secured' files based on user security credentials.
					XmlUrlResolver xurlr = new XmlUrlResolver();
					xurlr.Credentials = System.Net.CredentialCache.DefaultCredentials;
					XmlTextReader xrdr = null;
					
					if (!usingRivetFile)
					{
						xrdr = new XmlTextReader(filename);

					}
					else
					{
						xrdr = new XmlTextReader(IsolatedStorageFile.FullName);
					}

					try
					{
						xrdr.XmlResolver = xurlr;
				
						this.theDocument.Load(xrdr);
					}
					catch (XmlException)
					{
						//the file is not a valid xml file
						FileExists = false;
					}
					finally
					{
						xrdr.Close();
					}

				}
			}

            string additionalDirectory = null;
			//the above function can change the FileExists flag 
			if ( !FileExists )
			{
                if (IsolatedStorageFile == null)
                {
                    IsolatedStorageFile = new FileInfo(AucentGeneral.RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + Path.GetFileName(filename));
                }

				#region using isolated storage file
				if (IsolatedStorageFile != null && IsolatedStorageFile.Exists)
				{
					if ( promptUser)
					{
#if AUTOMATED
					throw (new System.Exception("Error Source: DocumentBase.cs | Please review your DocumentBase.Load method call. " +
						" Make sure you use the PromptUser parameter correctly." +
						" Suppressing message & dialog boxes." ));
#else
						if (DialogResult.Yes == this.ConfirmLocalFileUsage(filename,IsolatedStorageFile))
						{
                            additionalDirectory = Path.GetDirectoryName(IsolatedStorageFile.FullName);
                            schemaFile = filename;
							FileExists = FileIsLocal = true;
							usingRivetFile = true;
						}
						else
						{
							//not using local file, so browse for it
							FileInfo fi = null;
							if ((fi = PromptUserForFile(filename)) != null)
							{
								FileExists = true;
								schemaFile = filename = fi.FullName;
                                additionalDirectory = Path.GetDirectoryName(filename);

							}
							else
							{
								//user cancelled browse, so display message and exit
								UserMessage = TraceUtility.FormatStringResource("XBRLParser.DocumentBase.FileNotFound",
									Path.GetFileName(filename));
								MessageBox.Show(this.OwnerHandle, UserMessage, Path.GetFileName(filename), 
									MessageBoxButtons.OK, MessageBoxIcon.Information);
								Common.WriteError("Engine.Error.InvalidFilePath", this.errorList);
								numErrors++;
								return numErrors;
							}
						}
#endif
					}
					else
					{
                        schemaFile = filename;
						FileExists = FileIsLocal = true;
						usingRivetFile = true;
					}
				}

                #endregion
                #region Prompt for User action

                if (!FileExists && promptUser)
				{
#if AUTOMATED
				throw (new System.Exception("Error Source: DocumentBase.cs | Please review your DocumentBase.Load method call. " +
					" Make sure you use the PromptUser parameter correctly." +
					" Suppressing message & dialog boxes." ));
#else
					try
					{
						UserMessage = TraceUtility.FormatStringResource("XBRLParser.DocumentBase.FileNotFound",
							filename)+ Environment.NewLine + Environment.NewLine;
						UserMessage += TraceUtility.FormatStringResource("XBRLParser.DocumentBase.PleaseBrowseForFile",
							Path.GetFileName(filename));
						System.Windows.Forms.MessageBox.Show(this.OwnerHandle, UserMessage, Path.GetFileName(filename), 
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					
						FileInfo fi = null;
						if ((fi = PromptUserForFile(filename)) != null)
						{
							FileExists = true;
							schemaFile = filename = fi.FullName;
							//addToIsolatedStorage = true;
						}

						//reset the cursor to Wait (playing with message boxes changes it to an arrow)
						Cursor.Current = Cursors.WaitCursor;
					}
					catch (System.ArgumentException)
					{
					}
#endif
				}

				#endregion

				#region load file if it exists 
				if ( FileExists )
				{
					
			
					// Using XmlUrlResolver will allow us to access 'secured' files based on user security credentials.
					XmlUrlResolver xurlr = new XmlUrlResolver();
					xurlr.Credentials = System.Net.CredentialCache.DefaultCredentials;
					XmlTextReader xrdr = null;
				

					if ( !usingRivetFile )
					{
						xrdr = new XmlTextReader(schemaFile);
					}
					else
					{
						xrdr = new XmlTextReader(IsolatedStorageFile.FullName);
					}					

					try
					{
						xrdr.XmlResolver = xurlr;
						theDocument.Load(xrdr);
					}
					finally
					{
						xrdr.Close();
					}
				}

				#endregion
			}

			if (FileExists)
			{
                //just because the web location says that the file exists
                //it does not mean that the file that is out there is a valid xml file.
                //only set the filename and path if schemaFile is a directory, not a url
                if (!schemaFile.StartsWith("http"))
                {
                    schemaFilename = Path.GetFileName(schemaFile);
                    schemaPath = Path.GetDirectoryName(schemaFile);

                  
                }
                else
                {
                    this.schemaFilename = System.IO.Path.GetFileName(schemaFile);
                    this.schemaPath = filename.Replace(schemaFilename, string.Empty);


                }

                if (additionalDirectory != null)
                {
                    this.AddToAdditionalDirectoriesForLoad(additionalDirectory);

                }

                

				// Save the document to the local ApplicationData location
				//if it is a web based file.
				if (!usingRivetFile && addToIsolatedStorage)
				{
					string localCopyPath = AucentGeneral.RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + schemaFilename;

					try
					{
						//save the newer version
						theDocument.Save(localCopyPath);

					}
					catch (Exception)
					{
						FileExists = false;

					}
				}

				theManager = new XmlNamespaceManager( theDocument.NameTable );
				PopulateNamespaceManager(theManager, theDocument);
				SetDocumentXBRLIPrefix();
			}
			else
			{
				Common.WriteError("Engine.Error.InvalidFilePath", this.errorList);
				numErrors++;
			}
			return numErrors;
		}

		/// <summary>
		/// Look at the schema node of the taxonomy document and check to make sure that
		/// the Instance namespace and/or 
		/// </summary>
		public bool ValidateXBRLVersion(XmlDocument theDocument, out int numErrors)
		{
			numErrors = 0;

			XmlNamespaceManager aManager = new XmlNamespaceManager( theDocument.NameTable );
			aManager.AddNamespace("xsd", XML_SCHEMA_URL);

			XmlNode namespaceNode = theDocument.SelectSingleNode("//xsd:schema", aManager);

			if (namespaceNode == null)
			{
				Common.WriteError("XBRLParser.Error.FileIsNotValidXBRLTaxonomyFile", 
					this.errorList, schemaFile);

				numErrors++;
				return false;
			}

			XmlNode instanceNode = namespaceNode.Attributes.GetNamedItem("xmlns:xbrli");
			XmlNode linkNode = namespaceNode.Attributes.GetNamedItem("xmlns:link");

			if (instanceNode != null)
			{
				ValidateNamespaceURL(instanceNode, XBRL_INSTANCE_URL, ref numErrors);
			}

			if (linkNode != null)
			{
				ValidateNamespaceURL(linkNode, XBRL_LINKBASE_URL, ref numErrors);
			}

			return (numErrors == 0);
		}

		private FileInfo PromptUserForFile(string MissingFile)
		{
			FileInfo fi = null;
			
			try
			{
				RivetOpenFileDialog ROFD	= new RivetOpenFileDialog();
				ROFD.CheckFileExists		= true;
				ROFD.CheckPathExists		= true;
				ROFD.HideReadOnly			= true;
				ROFD.Filter					= "XML files (*.xml)\0*.xml\0XSD files (*.xsd)\0*.xsd\0All files (*.*)\0*.*\0";

				//set the filterIndex for the appropriate file type
				if (MissingFile.EndsWith("xml"))
					ROFD.FilterIndex = 1;
				else
					ROFD.FilterIndex = 2;
				
				ROFD.RestoreDirectory		= false;
				ROFD.Multiselect			= false;
				ROFD.DefaultExtension		= "xsd";
				ROFD.FileName				= MissingFile;

				if (ROFD.ShowDialog(this.OwnerHandle) == DialogResult.OK)
				{
					if ( ROFD.OriginalPath.IndexOf( "http" ) == 0 )
					{
						/* The dialog downloads the file to Temporary Internet Files.  If there are
						 * multiple copies, a [number] is appended to the file name, so we need to clean
						 * it up before using it. */
						string newFileName = ROFD.FileName;
						if (newFileName.IndexOf(@"[") > -1)
						{
							int startBracket = newFileName.IndexOf(@"[");
							int endBracket = newFileName.IndexOf(@"]");
							if (startBracket > 0 && endBracket > 0)
							{
								string tempString = newFileName.Remove(startBracket, endBracket - startBracket + 1);
								newFileName = tempString;
							}
						}

						fi = new FileInfo(newFileName);
					}
					else
					{
						fi = new FileInfo(ROFD.OriginalPath);
					}

					string MissingFileName = MissingFile;
					int lastSlash = MissingFile.LastIndexOf(@"/");
					if (lastSlash < 0)
					{
						lastSlash = MissingFile.LastIndexOf(@"\");
					}
					if (lastSlash > -1)
					{
						MissingFileName = MissingFile.Substring(lastSlash + 1, MissingFile.Length - lastSlash - 1);
					}
					StringBuilder msg = new StringBuilder(TraceUtility.FormatStringResource("XBRLAddin.ExportInstance.WrongExtensionFile", MissingFileName));
					
					while (fi.Name.CompareTo(MissingFileName) != 0)
					{
						//new file name does not match original file
						MessageBox.Show(msg.ToString(), AucentGeneral.PRODUCT_NAME,MessageBoxButtons.OK);
						
						//reset the dialog box
						ROFD.Filter	= "XML files (*.xml)\0*.xml\0XSD files (*.xsd)\0*.xsd\0All files (*.*)\0*.*\0";
						if (MissingFile.EndsWith("xml"))
							ROFD.FilterIndex = 1;
						else
							ROFD.FilterIndex = 2;
						ROFD.FileName = MissingFile;

						if (ROFD.ShowDialog(this.OwnerHandle) == DialogResult.OK)
						{
							FileInfo newFileInfo = new FileInfo(ROFD.FileName);
							
							/* The dialog downloads the file to Temporary Internet Files.  If there are
							 * multiple copies, a [number] is appended to the file name, so we need to clean
							 * it up before using it. */
							string newFileName = ROFD.FileName;
							if (newFileName.IndexOf(@"[") > -1)
							{
								int startBracket = newFileName.IndexOf(@"[");
								int endBracket = newFileName.IndexOf(@"]");
								if (startBracket > 0 && endBracket > 0)
								{
									string tempString = newFileName.Remove(startBracket, endBracket - startBracket + 1);
									newFileName = tempString;
								}
							}

							//check that new file exists and name matches original file
							if (newFileInfo.Exists && 
								Path.GetFileName(newFileName).CompareTo(MissingFileName) == 0)
							{
								fi = newFileInfo;
							}
						}
						else
						{
							//user cancelled out of open file dialog so exit while loop
							break;
						}
					}
				}
				else
				{
					fi = (FileInfo) null;
				}
			}
			catch
			{
				fi = (FileInfo) null;
			}
			return fi;
		}

		private DialogResult ConfirmLocalFileUsage(string OriginalFileRequested, FileInfo IsolatedStorageFile)
		{
			string UserMessage = TraceUtility.FormatStringResource("XBRLParser.DocumentBase.FileNotFoundAtLocation",
				Path.GetFileName(OriginalFileRequested),OriginalFileRequested)+ Environment.NewLine + Environment.NewLine;
			
			UserMessage += TraceUtility.FormatStringResource("XBRLParser.DocumentBase.IsolatedStorageFileFound",
				IsolatedStorageFile.Name, IsolatedStorageFile.FullName)+ Environment.NewLine + Environment.NewLine;

			UserMessage += TraceUtility.FormatStringResource("XBRLParser.DocumentBase.UseLocalFileInstead", 
				IsolatedStorageFile.Name);
			
			return System.Windows.Forms.MessageBox.Show(this.OwnerHandle,UserMessage, Path.GetFileName(OriginalFileRequested), 
				System.Windows.Forms.MessageBoxButtons.YesNo,
				System.Windows.Forms.MessageBoxIcon.Question);
		}

		private void ValidateNamespaceURL(XmlNode node, string url, ref int numErrors)
		{
			if (node.InnerText != url)
			{
				Common.WriteError("XBRLParser.Error.TaxonomyFileIsWrongXBRLVersion",
					this.errorList, schemaFile);

				Common.WriteError("XBRLParser.Error.UnsupportedXBRLNamespaceReference",
					this.errorList, schemaFile, node.InnerText);

				numErrors++;
			}
		}

		/// <summary>
		/// Closes this <see cref="DocumentBase"/>, setting the underlying <see cref="XmlDocument"/>
		/// object to null.
		/// </summary>
		/// <remarks><see cref="XmlDocument"/> is disposed only if <see cref="Taxonomy.KeepXMLDocument"/> is false.</remarks>
		public void Close()
		{
			if (!Taxonomy.KeepXMLDocument)
			{
				if (theDocument != null)
				{
					theDocument = null;
				}
			}
		}

		/// <summary>
		/// Populates a parameter-supplied <see cref="XmlNamespaceManager"/> with standard XML
		/// and XBRL namespaces as well as those namespaces referenced by a parameter-supplied
		/// <see cref="XmlDocument"/>.
		/// </summary>
		/// <param name="theManager">The <see cref="XmlNamespaceManager"/> to be populated.</param>
		/// <param name="theDocument">The <see cref="XmlDocument"/> from which <paramref name="theManager"/>
		/// is to be populated.</param>
		public static void PopulateNamespaceManager(XmlNamespaceManager theManager, XmlDocument theDocument)
		{
			// always add the xsd namespace
			theManager.AddNamespace( XML_SCHEMA_PREFIX,		XML_SCHEMA_URL );
			theManager.AddNamespace( XBRL_LINKBASE_PREFIX,	XBRL_LINKBASE_URL );
			theManager.AddNamespace( XLINK_PREFIX,			XLINK_URI );
			theManager.AddNamespace( XBRLDT_PREFIX,			XBRLDT_URI );
			theManager.AddNamespace(XBRLI_PREFIX		, XBRL_INSTANCE_URL);

				IEnumerator attributes = theDocument.DocumentElement.Attributes.GetEnumerator();
			while( attributes.MoveNext() )
			{
				XmlAttribute attribute = (XmlAttribute)attributes.Current;
				string attributeName = attribute.Name;
				if (attributeName.ToLower(CultureInfo.CurrentCulture).StartsWith(XMLNS))
				{
					string [] strings = attributeName.Split( ':' );
					if( strings.Length > 1 )
					{
						switch ( strings[1] )
						{
							// these have already been added - don't add them again
							case XML_SCHEMA_PREFIX:
							case XBRL_LINKBASE_PREFIX:
							case XLINK_PREFIX:
								break;

								// everything else gets added
							default:
								theManager.AddNamespace( strings[1], attribute.InnerText );
								break;
						}							
					}
					else	// default namespace
					{
						theManager.AddNamespace( "", attribute.InnerText );
					}
				}
			}
		}

        protected string[] GetLinkbaseReference(string linkRole)
		{
			ArrayList filenames = new ArrayList();
			string currentHref = string.Empty;

			XmlNodeList nodes = theDocument.SelectNodes("//link:linkbaseRef[@xlink:role='" + linkRole + "']", theManager);
			//XmlNode node = theDocument.SelectSingleNode( "/xsd:schema/xsd:annotation/xsd:appinfo/link:linkbaseRef[@xlink:role='" + linkRole + "']", theManager );

			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					if (node.Attributes.GetNamedItem("href", XLINK_URI) != null)
					{
						// BUG 1362 - don't append the schemaPath if the href is already a URL
						currentHref = node.Attributes.GetNamedItem("href", XLINK_URI).Value;
						if (currentHref.StartsWith("http", StringComparison.OrdinalIgnoreCase))
						{
							//currentHref is URL so just use it
							filenames.Add(currentHref);
						}
						else
						{
							string fileName = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(schemaPath, currentHref);
							if (!fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
								!File.Exists(fileName))
                            {
                                if (File.Exists(currentHref))
                                {
                                    fileName = currentHref;
                                }
                                else
                                {
                                    string tmpName = Path.Combine(schemaPath, Path.GetFileName(currentHref));
                                    if (File.Exists(tmpName))
                                    {
                                        fileName = tmpName;
                                    } 
                                }
                            }
							//currentHref is not URL so build schema-filename string
							filenames.Add(fileName);
						}
					}
				}

				if (filenames.Count > 0)
				{
					return (string[])filenames.ToArray(typeof(string));
				}
			}

			//it is possible that the @xlink:role is not defined and we have just simple link:linkbaseRef
			//need to find all such references
			nodes = theDocument.SelectNodes("//link:linkbaseRef", theManager);

			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					if (node.Attributes.GetNamedItem("xlink:role") != null)
					{
						//need to process it if it is a role that we do not understand
						string roleVal = node.Attributes.GetNamedItem("xlink:role").Value;
						if( roleVal == TARGET_LINKBASE_URI + PRESENTATION_ROLE ||
							roleVal == TARGET_LINKBASE_URI + CALCULATION_ROLE ||
							roleVal == TARGET_LINKBASE_URI + DEFINISION_ROLE ||
							roleVal == TARGET_LINKBASE_URI + LABEL_ROLE ||
							roleVal == TARGET_LINKBASE_URI + REFERENCE_ROLE )
						continue;

					}

					if (node.Attributes.GetNamedItem("href", XLINK_URI) != null)
					{
						// BUG 1362 - don't append the schemaPath if the href is already a URL
						currentHref = node.Attributes.GetNamedItem("href", XLINK_URI).Value;
						if (currentHref.StartsWith("http", StringComparison.OrdinalIgnoreCase))
						{
							//currentHref is URL so just use it
							filenames.Add(currentHref);
						}
						else
						{
							//currentHref is not URL so build schema-filename string
							string fileName = Aucent.MAX.AXE.Common.Utilities.AucentGeneral.AppendFileNameToSchemaPath(schemaPath, currentHref);
							if (!fileName.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) &&
								!File.Exists(fileName))
							{
								
								if (File.Exists(currentHref))
								{
									fileName = currentHref;
								}
							}
							//currentHref is not URL so build schema-filename string
							filenames.Add(fileName);
						}
					}
				}

				return (string[])filenames.ToArray(typeof(string));
			}
			else
			{


				return null;
			}
		}


		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="Presentation"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="discoveredSchemas">The collection to which additional XML schemas associated with 
		/// locator links will be added.
		/// </param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		/// <remarks>Method is equivalent to <see cref="ParseInternal( Dictionary{String, String}, out int)"/>; 
		/// however, an <see cref="XmlException"/>s or <see cref="XsltException"/>s encountered are caught
		/// and logged.
		/// </remarks>
		public virtual bool Parse(	Dictionary<string, string> discoveredSchemas, out int numErrors )
		{
			errorList.Clear();
			numErrors = 0;

			try
			{
				ParseInternal( discoveredSchemas, out numErrors );
			}
			catch ( XmlException xe )
			{
				++numErrors;
				Common.WriteError("XBRLParser.Error.Exception",  errorList, xe.Message);
				return false;
			}
			catch ( XsltException xse )
			{
				++numErrors;
				Common.WriteError("XBRLParser.Error.Exception",  errorList, xse.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="DocumentBase"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		/// <remarks>Method is equivalent to <see cref="ParseInternal(out int)"/>; 
		/// however, an <see cref="XmlException"/>s or <see cref="XsltException"/>s encountered are caught
		/// and logged.
		/// </remarks>
		public virtual bool Parse(out int numErrors)
		{
			errorList.Clear();
			numErrors = 0;

			try
			{
				System.Diagnostics.Debug.WriteLine( "Parse file: " + this.schemaFile );
				ParseInternal(out numErrors);
			}
			catch (XmlException xe)
			{
				++numErrors;
				Common.WriteError("XBRLParser.Error.Exception", errorList, xe.Message);
				return false;
			}
			catch (Exception xse)
			{
				++numErrors;
				Common.WriteError("XBRLParser.Error.Exception", errorList, xse.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="DocumentBase"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		protected abstract void ParseInternal(out int numErrors);

		/// <summary>
		/// Parse the <see cref="XmlDocument"/> underlying this <see cref="Presentation"/> object, 
		/// populating role and link information.
		/// </summary>
		/// <param name="discoveredSchemas">The collection to which additional XML schemas associated with 
		/// locator links will be added.
		/// </param>
		/// <param name="numErrors">An output parameter.  The number of errors encountered during 
		/// the parse process.</param>
		/// <exception cref="ApplicationException">Always thrown.  This method must be implemented in 
		/// derived classes.</exception>
		protected virtual void ParseInternal(Dictionary<string, string> discoveredSchemas, out int numErrors)
		{
			throw new ApplicationException("Please implement in the derived class");
		}

		/// <summary>
		/// Produces an XML representation of this <see cref="DocumentBase"/>.
		/// </summary>
		/// <returns>The XML representation of this <see cref="DocumentBase"/>.</returns>
		public abstract string ToXmlString();

		/// <summary>
		/// Writes an XML representation of this <see cref="DocumentBase"/> to a 
		/// parameter-supplied XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="numTabs">The number of tab characters to be appended <paramref name="text"/>
		/// before each XML node is written.</param>
		/// <param name="verbose">If true, attributes such as "nillable", "abstract", and "periodType" 
		/// are written to the output XML <see cref="StringBuilder"/>.</param>
		/// <param name="language">The language code for which <see cref="LabelLocator"/> objects within
		/// this <see cref="DocumentBase"/> are to be written.</param>
		/// <param name="text">The output XML to which this <paramref name="Node"/> is to be appended.</param>
		public abstract void ToXmlString(int numTabs, bool verbose, string language, StringBuilder text);

		#region Standard namespace prefix mapping

		/// <summary>
		/// if the tax has xbrli as the prefix for http://www.xbrl.org/2003/instance
		/// we do not need to do anything.
		/// if the prefix is empty or is something else then we need to map it back to the 
		/// xbrli as we want to use xbrli:monetaryItemType etc... to do comparisions etc...
		/// </summary>
		public bool HasStandardXBRLIPrefix = true;
		internal string documentXBRLIPrefixToReplace = null;
		internal string documentXBRLIPrefix = null;

		private void SetDocumentXBRLIPrefix()
		{
			if (this.theManager != null)
			{
				string prefix = this.theManager.LookupPrefix("http://www.xbrl.org/2003/instance");
				if (prefix != null && !prefix.Equals("xbrli"))
				{
					HasStandardXBRLIPrefix = false;
					documentXBRLIPrefix = prefix;
					if (prefix.Length > 0)
					{
						documentXBRLIPrefixToReplace = prefix + ":";
					}
					else
					{
						//http://www.xbrl.org/2003/instance is the default namespace and hence is
						//not prefixed with anything.
						documentXBRLIPrefixToReplace = prefix;
					}
				}
			}
		}

		/// <summary>
		/// Parser uses xbrli: as the prefix to search fr some information
		/// we need to convert it to the valid prefix for the document
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public string ConvertXBRLIToValidPrefix(string data)
		{
			if (this.documentXBRLIPrefixToReplace != null)
			{
				return data.Replace( "xbrli:", documentXBRLIPrefixToReplace);
			}

			return data;
		}

		/// <summary>
		/// check prefix to see if it represents the http://www.xbrl.org/2003/instance namespace
		/// and if it does then convert it to xbrli: so that all other validations works
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		internal string ConvertToXBRLIPrefixIfValidPrefix(string data)
		{
			if (this.documentXBRLIPrefixToReplace != null)
			{
				if (documentXBRLIPrefixToReplace.Length > 0)
				{
					if (data.StartsWith(documentXBRLIPrefixToReplace))
					{
						return data.Replace(documentXBRLIPrefixToReplace, "xbrli:");
					}
				}
				else
				{
					//default prefix is xbrli... so data should not have any prefix...
					if (!data.Contains(":"))
					{
						return data.Replace(documentXBRLIPrefixToReplace, "xbrli:");

					}
				}
			}

			return data;
		}
		#endregion

		internal bool IsDocumentCreatedByRivet()
		{
			bool ret = false;
			if (theDocument != null)
			{
				// the first comment should say Aucent if it's an Aucent extended taxonomy
				XmlNode comment1 = theDocument.ChildNodes[1];


				if (comment1 != null)
					ret = comment1.OuterXml.IndexOf(Aucent.MAX.AXE.Common.Utilities.AucentGeneral.RIVET_NAME) != -1;

			}

			return ret;
		}

        internal DocumentBase GetLoadingTaxonomy()
        {
            return this.loadingTaxonomy == null ? this : this.loadingTaxonomy;
        }

	}

	/// <summary>
	/// Encapsulates properties for a Microsoft Windows window.
	/// </summary>
	[Serializable]
	public class WindowWrapper : System.Windows.Forms.IWin32Window
	{
		private IntPtr _hwnd;

		/// <summary>
		/// Constructs a new instance of <see cref="WindowWrapper"/>.
		/// </summary>
		public WindowWrapper()
		{
		}
		/// <summary>
		/// Overloaded.  Constructs a new instance of <see cref="WindowWrapper"/>.
		/// </summary>
		/// <param name="handle">The handle for the window.</param>
		public WindowWrapper(IntPtr handle)
		{
			_hwnd = handle;
		}

		/// <summary>
		/// The Windows handle for the window.
		/// </summary>
		public IntPtr Handle
		{
			get { return _hwnd; }
			set { _hwnd = value; }
		}
	}
}
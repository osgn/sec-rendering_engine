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
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// RemoteFiles
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
    [global::System.Reflection.Obfuscation(Exclude = true)]
    public class RemoteFiles
	{
        
		#region properties

		public System.Net.Cache.RequestCacheLevel CachePolicy = System.Net.Cache.RequestCacheLevel.Default;

		private static string MAPPING_FILE_NAME = @"DragonTag.TaxonomyURLInfo.xml";
		/// <summary>
		/// dictionary of all files that have been checked for valid url.
		/// </summary>
		private static Dictionary<string, string> remoteFilesDt = new Dictionary<string, string>();

		private static Dictionary<string, string> taxonomyURLMappingInfo;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new RemoteFiles.
		/// </summary>
		public RemoteFiles()
		{
		}

		#endregion

		#region methods

		#region URL Mappings
		public static Dictionary<string, string> GetBaseTaxonomyURLMapping()
		{
			if (taxonomyURLMappingInfo == null)
			{
				taxonomyURLMappingInfo = LoadTaxonomyURLMappingInfo();
			}

			return taxonomyURLMappingInfo;
		}

		private static bool TryGetUrlMapping(string fullfileName, out string actualFileUrl)
		{
			if (taxonomyURLMappingInfo == null)
			{
				taxonomyURLMappingInfo = LoadTaxonomyURLMappingInfo();
			}

			string fileName = Path.GetFileName(fullfileName).ToLower();

			return taxonomyURLMappingInfo.TryGetValue(fileName, out actualFileUrl);
		}

		private static Dictionary<string,string> LoadTaxonomyURLMappingInfo()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			string fileName = System.IO.Path.GetDirectoryName(assembly.Location) + @"\" + MAPPING_FILE_NAME;
			Dictionary<string, string> ret = new Dictionary<string, string>();

			if (File.Exists(fileName))
			{
				string[] vals = XmlDeserializeObjectFromFile(fileName, typeof(string[])) as string[];
				if (vals != null)
				{
					for (int i = 1; i < vals.Length; i = i + 2)
					{
						ret[vals[i - 1]] = vals[i];
					}
				}
			}
			else
			{
				Stream str = assembly.GetManifestResourceStream("Aucent.MAX.AXE.Common.Utilities."+ MAPPING_FILE_NAME);
				if (str != null)
				{

					XmlSerializer serializer = new XmlSerializer(typeof(string[]));
					string[] vals = serializer.Deserialize(str) as string[];
					if (vals != null)
					{
						for (int i = 1; i < vals.Length; i = i + 2)
						{
							ret[vals[i - 1]] = vals[i];
						}
					}

					str.Close();
				}
			}


			return ret;
		}

		public static void UpdateTaxonomyURLMappingInfo(Dictionary<string, string> newMappings)
		{
			if (taxonomyURLMappingInfo == null)
			{
				taxonomyURLMappingInfo = LoadTaxonomyURLMappingInfo();
			}

			foreach (KeyValuePair<string, string> kvp in newMappings)
			{
				taxonomyURLMappingInfo[kvp.Key] = kvp.Value;
			}
			Assembly assembly = Assembly.GetExecutingAssembly();

			string fileName = System.IO.Path.GetDirectoryName(assembly.Location) + @"\" + MAPPING_FILE_NAME;
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			List<string> serializableObject = new List<string>();
			foreach (KeyValuePair<string, string> kvp in taxonomyURLMappingInfo)
			{
				serializableObject.Add(kvp.Key);
				serializableObject.Add(kvp.Value);
			}
			XmlSerializeObjectToFile(fileName, serializableObject.ToArray() );
		}

		public static long XmlSerializeObjectToFile(string strFileName, object obj)
		{
			long size = -1;
			FileStream fsWrite = null;
			try
			{
				fsWrite = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
				size = XmlSerializeObjectToFile(fsWrite, obj);
			}
			finally
			{
				if (fsWrite != null)
					fsWrite.Close();
			}
			return size;
		}

		internal static long XmlSerializeObjectToFile(FileStream fsWrite, object obj)
		{
			//Rather than using XMLSerializer class we need to call XmlSerializeObjectToString and
			//write that string to file.  This ensures that what we write to the database and what we
			//write to file will always be in the same format.
			string xmlData = XmlSerializeObjectToString(obj);
			//TextWriter tw = new StreamWriter(fsWrite);
			//tw.WriteLine(xmlData);
			//tw.Flush();
			fsWrite.Write(UTF8Encoding.UTF8.GetBytes(xmlData), 0, UTF8Encoding.UTF8.GetByteCount(xmlData));
			fsWrite.Flush();
			return fsWrite.Length;
		}
		internal static string XmlSerializeObjectToString(object obj)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

			MemoryStream writeStream = new MemoryStream();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(writeStream, System.Text.Encoding.UTF8);
			xmlSerializer.Serialize(xmlTextWriter, obj);
			xmlTextWriter.Close();

			System.Text.UTF8Encoding utf8Encoding = new UTF8Encoding();
			string xml = utf8Encoding.GetString(writeStream.ToArray());
			return xml;
		}


		internal static object XmlDeserializeObjectFromFile(string strFileName, Type type)
		{
			object obj = null;
			FileStream fsRead = null;
			try
			{
				fsRead = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
				obj = XmlDeserializeObjectFromFile(fsRead, type);
			}
			finally
			{
				if (fsRead != null)
					fsRead.Close();
			}
			return obj;
		}

		internal static object XmlDeserializeObjectFromFile(FileStream fsRead, Type type)
		{
			XmlSerializer serializer = new XmlSerializer(type);

			return serializer.Deserialize(fsRead);
		}

		#endregion

	
		/// <summary>
		/// Determines if a given file URL exists.
		/// </summary>
		/// <param name="FileURL">The file URL to be located.</param>
		/// <param name="actualFileUrl">An output parameter containing the URL received from 
		/// the server for the file.</param>
		/// <returns>True if the existence of the file has successfully been verified with the 
		/// server.</returns>
		/// <remarks>Once the server has been contacted, the existence of the file and the URL 
		/// from the server is cached.</remarks>
		public static bool IsValidURL(string FileURL, out string actualFileUrl )
		{
			actualFileUrl = FileURL;

			lock (remoteFilesDt)
			{
				if (remoteFilesDt.TryGetValue(FileURL, out actualFileUrl)) return true;
				if (TryGetUrlMapping(FileURL, out actualFileUrl)) return true;

				if (remoteFilesDt.Count > 5000) remoteFilesDt.Clear();

				DateTime lastModified;

				RemoteFiles rf = new RemoteFiles();

				if (rf.CheckURLFileExists(FileURL,  ref actualFileUrl, out lastModified))
				{
					RemoteFiles.remoteFilesDt[FileURL] = actualFileUrl;

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines if a parameter supplied file URL exists.
		/// </summary>
		/// <param name="FileURL">The file URL to be located.</param>
		/// <param name="lastModified">If the file is located, the date and time on which 
		/// the file was last modified.</param>
		/// <returns>True if method was able to contact the URL server and verify the file's 
		/// existence.</returns>
		public bool CheckURLFileExists(string FileURL, out DateTime lastModified)
		{
			string actualURL = string.Empty;
			return CheckURLFileExists(FileURL,  ref  actualURL, out lastModified);
		}

		public bool LoadWebFile(string FileURL,
			ref string actualURL,
			 out XmlDocument dataFile)
		{


			dataFile = null;
			if (!FileURL.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string isolatedStorageFileName = AucentGeneral.RivetApplicationDataDragonTagPath + Path.DirectorySeparatorChar + Path.GetFileName(FileURL);
			actualURL = FileURL;
			DateTime lastModified = DateTime.Now;
			bool useIsolatedStorageFile = false;
			FileInfo fi = new FileInfo(isolatedStorageFileName);
			long fileLength = 0;
			if (RemoteFileInformation.TryGetFileInformation(FileURL, ref actualURL, ref lastModified, ref fileLength ))
			{



				if (fi.Exists && fi.LastWriteTime >= lastModified && fi.Length == fileLength )
				{
					dataFile = new XmlDocument();
					dataFile.Load(isolatedStorageFileName);

					return true;
				}
				else if (fi.Exists)
				{
					//wrong date or length
					try
					{
						fi.Delete();
					}
					catch (Exception)
					{

					}
					finally
					{
						
						fi = new FileInfo(isolatedStorageFileName);

					}
				}


			}



			if (fi.Exists)
			{
				HttpWebResponse webResponse = null;
				try
				{
					HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(FileURL);
					myRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy( this.CachePolicy );
					myRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
					myRequest.KeepAlive = false;
					myRequest.Timeout = 1000;
					//used fiddler to determine what IE was setting the UserAgent to and used the same to 
					//make this call work....
					myRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; GTB5; .NET CLR 2.0.50727; .NET CLR 1.1.4322; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30428)";
					myRequest.Method = "Head";

					IWebProxy iwp20 = WebRequest.DefaultWebProxy;
					Uri proxyUri = iwp20.GetProxy(myRequest.RequestUri );
					WebProxy theProxy = new WebProxy(proxyUri);
					if ( !string.Equals(  theProxy.Address.AbsoluteUri, myRequest.RequestUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase ))
					{
						theProxy.UseDefaultCredentials = true;
						myRequest.Proxy = theProxy;
					}

					webResponse = (HttpWebResponse)myRequest.GetResponse();
					switch (webResponse.StatusCode)
					{
						case (HttpStatusCode.Accepted):
						case (HttpStatusCode.Found):
						case (HttpStatusCode.OK):
							if (fi.LastWriteTime >= webResponse.LastModified)
							{
								lastModified = webResponse.LastModified;
								actualURL = webResponse.ResponseUri.AbsoluteUri;
								dataFile = new XmlDocument();
								dataFile.Load(isolatedStorageFileName);
								RemoteFiles.remoteFilesDt[FileURL] = actualURL;
								RemoteFileInformation.AddFileInformation(FileURL, actualURL, lastModified, fi.Length );

								return true;
							}
							break;

					}

				}
				catch (Exception)
				{

				}
				finally
				{
					if (webResponse != null)
					{
						webResponse.Close();

						(webResponse as IDisposable).Dispose();
					}
				}

			}

			





			bool retVal = false;


			// to make sure that this call is single threaded and tom update the remoteFilesDt list.
			lock (RemoteFiles.remoteFilesDt)
			{

				int count = 0;
				int timeout = 5000;
				while (true)
				{
					HttpWebResponse webResponse = null;
					try
					{
						HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(FileURL);
						myRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy( this.CachePolicy );
						myRequest.Timeout = timeout;	// waiting 5 seconds
						myRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
						myRequest.KeepAlive = false;
						//used fiddler to determine what IE was setting the UserAgent to and used the same to 
						//make this call work....
						myRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; GTB5; .NET CLR 2.0.50727; .NET CLR 1.1.4322; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30428)";
						IWebProxy iwp20 = WebRequest.DefaultWebProxy;
						Uri proxyUri = iwp20.GetProxy(myRequest.RequestUri);
						WebProxy theProxy = new WebProxy(proxyUri);
						if (!string.Equals(theProxy.Address.AbsoluteUri, myRequest.RequestUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
						{
							theProxy.UseDefaultCredentials = true;
							myRequest.Proxy = theProxy;
						}
						
						//myRequest.IfModifiedSince //304
						//				myRequest.Proxy = new WebProxy(FileURL, true);
						//				myRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
						webResponse = (HttpWebResponse)myRequest.GetResponse();

						switch (webResponse.StatusCode)
						{
							case (HttpStatusCode.Accepted):
							case (HttpStatusCode.Found):
							case (HttpStatusCode.OK):
								//make sure that the file returned is the file requested
								retVal = true;
								lastModified = webResponse.LastModified;
								actualURL = webResponse.ResponseUri.AbsoluteUri;

								
									useIsolatedStorageFile = false;
									

										if (fi.Exists && fi.LastWriteTime >= lastModified)
										{
											useIsolatedStorageFile = true;
										}
									

									if (useIsolatedStorageFile)
									{

                                        FileInfo tmp = new FileInfo(isolatedStorageFileName);
                                        RemoteFileInformation.AddFileInformation(FileURL, actualURL, lastModified, tmp.Length);


										dataFile = new XmlDocument();
										dataFile.Load(isolatedStorageFileName);
										return true;
									}
									else
									{
										try
										{
											dataFile = new XmlDocument();
											dataFile.Load(webResponse.GetResponseStream());
										}
										catch (Exception)
										{
											return false;
										}

										RemoteFiles.remoteFilesDt[FileURL] = actualURL;
										if (retVal && isolatedStorageFileName != null)
										{
											dataFile.Save(isolatedStorageFileName);

											FileInfo tmp = new FileInfo(isolatedStorageFileName);
											RemoteFileInformation.AddFileInformation(FileURL, actualURL, lastModified, tmp.Length);
										}


										return true;
									}

								






								
							case (HttpStatusCode.NotFound):
								retVal = false;
								break;
							default:
								retVal = false;
								break;
						}
					}
					catch (WebException exp)
					{

						if (exp.Status == WebExceptionStatus.ConnectionClosed || exp.Status == WebExceptionStatus.Timeout)
						{
							if (exp.Status == WebExceptionStatus.Timeout)
							{
								//give the application couple of more sec to get the data.

								timeout += 2000;

							}
							//it is a good idea to retry this before we give up..
							count++;
							if (count <= 5) continue;
						}
						retVal = false;
					}
					catch (System.InvalidCastException)
					{
						return false;
					}
					catch (System.UriFormatException)
					{
						return false;
					}
					catch (Exception )
					{

						return false;
					}
					finally
					{
						if (webResponse != null)
						{
							webResponse.Close();

							(webResponse as IDisposable).Dispose();
						}
					}
					

					return retVal;


				}
			}


		}



		public class RemoteFileInformation : IComparable<RemoteFileInformation>
		{
			public string FileURL;
			public string ActualURL;
			public DateTime LastModified;
			public long FileLength;

			internal static List<RemoteFileInformation> remoteList = new List<RemoteFileInformation>(1000);

			internal static bool TryGetFileInformation(string FileURL,
				ref string actualURL, ref DateTime lastModified, ref long fileLength)
			{
				RemoteFileInformation search = new RemoteFileInformation();
				search.FileURL = FileURL;

				lock (remoteList)
				{
					int index = remoteList.BinarySearch(search);
					if (index >= 0)
					{
						actualURL = remoteList[index].ActualURL;
						lastModified = remoteList[index].LastModified;
						fileLength = remoteList[index].FileLength;
						return true;
					}
				}

				return false;
			}

			internal static void AddFileInformation(string fileURL, string actualURL, DateTime lastModified, 
				long FileLength )
			{
				lock (remoteList)
				{
					RemoteFileInformation search = new RemoteFileInformation();
					search.FileURL = fileURL;
					search.ActualURL = actualURL;
					search.LastModified = lastModified;
					search.FileLength = FileLength;
					int index = remoteList.BinarySearch(search);
					if (index < 0)
					{
						remoteList.Insert(~index, search);
					}
					else
					{
						remoteList[index] = search;
					}


				}
			}

			#region IComparable<RemoteFileInformation> Members

			public int CompareTo(RemoteFileInformation other)
			{
				return this.FileURL.CompareTo(other.FileURL);
			}

			#endregion


			#region Serialization

			public static void SaveRemoteFileInformation()
			{
				//save it to a file in "S:\Common\Utilities" as it is an embedded resource...
				string fileName = @"S:\Common\Utilities\RemoteFileInfo.xml";


				XmlSerializeObjectToFile(fileName, remoteList.ToArray());

			}

            public static void ClearRemoteFileInformation()
            {
                //save it to a file in "S:\Common\Utilities" as it is an embedded resource...
                remoteList.Clear();

            }



			public static void LoadRemoteFileInformation()
			{
				//load from embedded resource...
				Assembly assembly = Assembly.GetExecutingAssembly();

				Stream str = assembly.GetManifestResourceStream("Aucent.MAX.AXE.Common.Utilities.RemoteFileInfo.xml");
				if (str != null)
				{

					XmlSerializer serializer = new XmlSerializer(typeof(RemoteFileInformation[]));
					RemoteFileInformation[] val =  serializer.Deserialize(str) as RemoteFileInformation[];
					if (val != null)
					{
						remoteList = new List<RemoteFileInformation>(val);
					}

					str.Close();
				}
			}
			#endregion
		}


		protected bool CheckURLFileExists(string FileURL,
			ref string actualURL, out DateTime lastModified)
		{
            
			lastModified = DateTime.MinValue;
            if (System.IO.File.Exists(FileURL))
            {
                return false;
            }
			long tmp=0;
			if (RemoteFileInformation.TryGetFileInformation(FileURL, ref actualURL, ref lastModified, ref tmp))
			{
				return true;
			}
			bool retVal = false;


			

			// to make sure that this call is single threaded and tom update the remoteFilesDt list.
			lock (RemoteFiles.remoteFilesDt)
			{

				int count = 0;
				int timeout = 5000;
				while (true)
				{
					HttpWebResponse webResponse = null;
					try
					{
						HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(FileURL);
						myRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy( this.CachePolicy );
						myRequest.Timeout = timeout;	// waiting 5 seconds
						myRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
						myRequest.KeepAlive = false;
						//used fiddler to determine what IE was setting the UserAgent to and used the same to 
						//make this call work....
						myRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; GTB5; .NET CLR 2.0.50727; .NET CLR 1.1.4322; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30428)";
						IWebProxy iwp20 = WebRequest.DefaultWebProxy;
						Uri proxyUri = iwp20.GetProxy(myRequest.RequestUri);
						WebProxy theProxy = new WebProxy(proxyUri);
						if (!string.Equals(theProxy.Address.AbsoluteUri, myRequest.RequestUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
						{
							theProxy.UseDefaultCredentials = true;
							myRequest.Proxy = theProxy;
						}
						//myRequest.IfModifiedSince //304
						//				myRequest.Proxy = new WebProxy(FileURL, true);
						//				myRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
						webResponse = (HttpWebResponse)myRequest.GetResponse();
						switch (webResponse.StatusCode)
						{
							case (HttpStatusCode.Accepted):
							case (HttpStatusCode.Found):
							case (HttpStatusCode.OK):
								//make sure that the file returned is the file requested
								retVal = true;
								if (string.Compare(FileURL, webResponse.ResponseUri.AbsoluteUri, true) != 0)
								{
									//uri did not match
									//just check file name
									if (string.Compare(System.IO.Path.GetFileName(FileURL), System.IO.Path.GetFileName(webResponse.ResponseUri.AbsoluteUri), true) != 0)
									{
										//we got some website redirect that is not the file we asked for and so it is not a valid uri....
										retVal = false;

									}

                                  
								}

								// many invalid urls are listed as for sale on other sites like goDaddy.com
								// This means even though the url is not valid, we will get a response of OK
								// and content type other than text/xml
								// However some taxonomy schemas are posted as text/plain. So if our response is 
								// of type plain/text we need to parse the content to see if it is a valid xml file
								if( !webResponse.ContentType.ToLower().Contains( "/xml"))
								{
									//Get the contenst of the file at the given url so that we can
									//determin if it represents xml data
									WebClient client = new WebClient();
									if (!string.Equals(theProxy.Address, myRequest.RequestUri.AbsoluteUri))
									{
										theProxy.UseDefaultCredentials = true;
										client.Proxy = theProxy;
									}
									string content = client.DownloadString(FileURL);

									//Test for urls that return empty text data
									if(content.Length == 0)
									{
										retVal = false;
									}
									else
									{
										try
										{
											XmlDocument doc = new XmlDocument();
											doc.LoadXml(content);
										}
										catch(Exception)
										{
											retVal = false;
										}
									}
								}

								lastModified = webResponse.LastModified;
								actualURL = webResponse.ResponseUri.AbsoluteUri;
								break;
							case (HttpStatusCode.NotFound):
								retVal = false;
								break;
							default:
								retVal = false;
								break;
						}
					}
					//			catch(WebException we) 
					//			{
					//				if ( we.InnerException != null )
					//				{
					//					System.Windows.Forms.MessageBox.Show( "Error caught in CheckURLFileExists: " + we.Message + " Inner: " + we.InnerException.Message );
					//				}
					//				else
					//				{
					//					System.Windows.Forms.MessageBox.Show( "Error caught in CheckURLFileExists: " + we.Message );
					//				}
					//
					//				retVal = false;
					//			} 
					catch (WebException  exp)
					{

						if (exp.Status == WebExceptionStatus.ConnectionClosed || exp.Status == WebExceptionStatus.Timeout)
                        {
							if( exp.Status == WebExceptionStatus.Timeout)
							{
								//give the application couple of more sec to get the data.
#if DEBUG
								Console.WriteLine("Time out occured for URL " + FileURL );
#endif
								timeout += 2000;

							}
                            //it is a good idea to retry this before we give up..
                            count++;
                            if (count <= 5) continue;
                        }
						retVal = false;
					}
					catch (System.InvalidCastException)
					{
						retVal = false;
					}
					catch (System.UriFormatException)
					{
						retVal = false;
					}
					catch (Exception Ex)
					{

						throw (new AucentException("Engine.Error.OpenFileFailure", Ex.Message, Ex));
					}
					finally
					{
						if (webResponse != null)
						{
							webResponse.Close();

							//(webResponse as IDisposable).Dispose();
						}
					}
					if (retVal)
					{
						RemoteFiles.remoteFilesDt[FileURL] = actualURL;
						//RemoteFileInformation.AddFileInformation(FileURL, actualURL, lastModified);
					}

					return retVal;

					
				}
			}

	
		}



	


		#endregion


	}

}
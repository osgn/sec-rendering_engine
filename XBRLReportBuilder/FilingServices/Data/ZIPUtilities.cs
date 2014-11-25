/**************************************************************************
 * ZipUtilities (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * A collection of static methods use for creating, and managing zip files on the
 * host system.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Aucent.MAX.AXE.Common.ZipCompressDecompress.Zip;

namespace Aucent.FilingServices.Data
{
	public static class ZipUtilities
	{

		#region public static methods

		public static byte[] TryCompressByteStream(byte[] bytes)
		{
			byte[] compressedBytes = null;
			using (MemoryStream ms = new MemoryStream())
			{
				using (ZipOutputStream zos = new ZipOutputStream(ms))
				{
					zos.SetLevel(9);	// max compression
					// add an entry
					zos.PutNextEntry(new ZipEntry("test"));

					zos.Write(bytes, 0, bytes.Length);
				}

				compressedBytes = ms.ToArray();
			}

			return compressedBytes;

		}

		public static byte[] TryUnCompressByteStream(byte[] compressedBytes)
		{
			byte[] uncompressedBytes = null;
			using (MemoryStream ms = new MemoryStream(compressedBytes))
			{
				// decompress the file
				using (ZipInputStream zis = new ZipInputStream(ms))
				{
					// should only be on entry per file
					ZipEntry entry = zis.GetNextEntry();
					uncompressedBytes = new byte[entry.Size];
					zis.Read(uncompressedBytes, 0, (int)entry.Size);
				}
			}

			return uncompressedBytes;


		}


		/// <summary>
		/// Zips and compresses files.
		/// You should give the full path for the zipFileName.
		/// You should give the full path for the rootDirectoryName.
		/// You should give relative path for the names of the files to zip. Relative to the root
		///		Example: ZipCompressUtility.TryZipAndCompressFiles( "C:\Aucent\Folio1.zip",
		///						"C:\DirectoryWithHtmlFiles\Folio1",
		///						new string[]{"index.html", "DependentHoppers\hopper.html"} );
		/// </summary>
		/// <param name="zipFileName">Set to the name that you want the zip file to be. (e.g. C:\Aucent\SalesFolio.zip)</param>
		/// <param name="rootDirectoryName">The root directory to start looking for files and directories in.</param>
		/// <param name="filesToZip">List of all the files you want to zip.</param>
		/// <returns></returns>
		public static bool TryZipAndCompressFiles(string zipFileName, string rootDirectoryName, string[] filesToZip)
		{
			bool ok = false;
			using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFileName)))
			{
				try
				{
					if (rootDirectoryName[rootDirectoryName.Length - 1] != Path.DirectorySeparatorChar)
					{
						// append directory separator char
						rootDirectoryName += Path.DirectorySeparatorChar;
					}

					s.SetLevel(9); // 0 - store only to 9 - means best compression

					if (filesToZip != null)
					{
						foreach (string file in filesToZip)
						{
							byte[] buffer = FileUtilities.ReadFileToByteArray(rootDirectoryName + file);
							if (buffer != null)
							{
								ZipEntry entry = new ZipEntry(file);
								s.PutNextEntry(entry);
								s.Write(buffer, 0, buffer.Length);
							}
						}
					}

					ok = true;
				}
				catch
				{
					ok = false;
				}
			}
			return ok;
		}

		/// <summary>
		/// Unzips and uncompresses a zip file.
		/// You should give the full path for the zipFileName.
		/// You should give the full path for the rootDirectoryName.
		/// Will unzip and uncompress zipFileName and put the files in the location of rootDirectoryName
		/// </summary>
		/// <param name="zipFileName"></param>
		/// <param name="rootDirectoryName"></param>
		/// <param name="listFilesUnzipped"></param>
		/// <returns></returns>
		public static bool TryUnzipAndUncompressFiles(string zipFileName, string rootDirectoryName, out string[] listFilesUnzipped, out string errorMsg)
		{
			errorMsg = string.Empty;
			listFilesUnzipped = null;
			List<string> listFiles = new List<string>();

			if (!File.Exists(zipFileName))
			{
				errorMsg = string.Format("The given file name, {0}, does not exists.", zipFileName);
				return false;

			}

			if (rootDirectoryName[rootDirectoryName.Length - 1] != Path.DirectorySeparatorChar)
			{
				// append directory separator char
				rootDirectoryName += Path.DirectorySeparatorChar;
			}

			if (!Directory.Exists(rootDirectoryName))
				Directory.CreateDirectory(rootDirectoryName);

			bool ok = false;
			using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileName)))
			{
				try
				{
					ZipEntry theEntry;
					while ((theEntry = s.GetNextEntry()) != null)
					{
						string fullPath = rootDirectoryName + theEntry.Name;
						string fileName = fullPath;
						if (fullPath.Length > 170)
						{
							fileName = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + Guid.NewGuid();
						}
						//if the directory doesn't exist, we need to create it
						string dirName = Path.GetDirectoryName(fileName);
						if (!Directory.Exists(dirName))
							Directory.CreateDirectory(dirName);

						if (File.Exists(fileName))
							File.Delete(fileName);

						// create the file
						using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
						{
							long dataSize = theEntry.Size > 0 ? theEntry.Size : 1024;
							byte[] data = new byte[dataSize];
							while (true)
							{
								int size = s.Read(data, 0, data.Length);
								if (size > 0)
									fs.Write(data, 0, size);
								else
									break;
							}
						}

						if (fullPath != fileName)
						{
							//defect 1969 fix
							//if the fullPath has been manipulated above, the path may not yet exist
							dirName = Path.GetDirectoryName(fullPath);
							if (!Directory.Exists(dirName))
								Directory.CreateDirectory(dirName);

							//If a file with the same name exists in the target path then we need to delete it before
							//we move otherwise .NET will throw an exception
							if (File.Exists(fullPath))
								File.Delete(fullPath);

							File.Move(fileName, fullPath);
						}

						listFiles.Add(fullPath);
					}

					listFilesUnzipped = listFiles.ToArray();

					ok = true;
				}
				catch (PathTooLongException)
				{

					errorMsg = string.Format("Can not unzip the given zip file {0}.\r\n" +
						"A path in the zip file combined with the extraction folder would result in a path that exceeds" +
						"the Operating System limitation of 248 charaters for a folder path, or 260 characters for a file path.\r\n" +
						"Extraction Folder: {1}.", zipFileName, rootDirectoryName);

					try
					{
						foreach (string file in listFiles)
						{
							File.Delete(file);
						}
					}
					catch { }

					ok = false;
				}
				catch(Exception)
				{
					ok = false;
				}
			}
			return ok;
		}

		#endregion public static methods
	}
}

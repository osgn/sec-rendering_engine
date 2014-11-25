/**************************************************************************
 * FileUtilities (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines static methods that are used managing files on the host system.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aucent.FilingServices.Data
{
	public class FileUtilities
	{
		private static readonly long READ_SIZE = 50000;

		#region binary serialization

		public static byte[] ReadFileToByteArray(string filename)
		{
			FileInfo fi = new FileInfo(filename);
			if (!fi.Exists)
			{
				return null;
			}

			long sizeRead = 0;
			int maxReadSize = (int)Math.Min(fi.Length, READ_SIZE);

			// read it in
			using (BinaryReader br = new BinaryReader(fi.Open(FileMode.Open, FileAccess.Read)))
			{
				byte[] bts = new byte[maxReadSize];

				List<byte> temp = new List<byte>((int)fi.Length);
				while (sizeRead < fi.Length)
				{
					int sizeToRead = (int)Math.Min(fi.Length - sizeRead, maxReadSize);

					bts = br.ReadBytes(sizeToRead);
					temp.AddRange(bts);

					sizeRead += sizeToRead;
				}

				return temp.ToArray();
			}
		}

		public static void WriteFileFromByteArray(string filename, byte[] byteStream)
		{

			FileInfo fileInfo = new FileInfo(filename);

			using (BinaryWriter bw = new BinaryWriter(fileInfo.OpenWrite()))
			{
				bw.Write(byteStream);
			}

		}

		#endregion

		public static void Copy( string from, string to )
		{
			try
			{
				File.WriteAllBytes( to, File.ReadAllBytes( from ) );
			} catch { }
		}

		public static void DeleteDirectory(DirectoryInfo dir, bool recursive, bool deleteReadonly)
		{
			FileInfo[] files = null;
			if (recursive)
			{
				files = dir.GetFiles("*.*", SearchOption.AllDirectories);
			}
			else
			{
				files = dir.GetFiles();
			}

			foreach (FileInfo file in files)
			{
				DeleteFile(file, deleteReadonly);
			}

			//At this point the directory should be empty if recursive is true, but pass it to
			//the delete method anyways as a failsafe.
			dir.Delete(recursive);
		}

		/// <summary>
		/// Deletes the given file from the file system.
		/// </summary>
		/// <param name="file">The file to be deleted</param>
		/// <param name="deleteReadonly">If this value is true then the file will be set to writable before
		/// trying to delete it.</param>
		public static void DeleteFile(FileInfo file, bool deleteReadonly)
		{
			if (file.IsReadOnly && deleteReadonly)
			{
				FileAttributes atts = File.GetAttributes(file.FullName);
				atts &= ~System.IO.FileAttributes.ReadOnly;
				File.SetAttributes(file.FullName, atts);
			}
			file.Delete();
		}

		public static bool TryCopyDirectory(DirectoryInfo sourceDir, DirectoryInfo targetDir, bool deleteSource)
		{
			try
			{
				foreach (FileInfo file in sourceDir.GetFiles())
				{
					string destFile = string.Format("{0}{1}{2}", targetDir.FullName, Path.DirectorySeparatorChar, file.Name);
					file.CopyTo(destFile, true);
				}

				if (deleteSource)
				{
					FileUtilities.DeleteDirectory(sourceDir, true, true);
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static string GetExecutionPath()
		{
			//Using CodeBase instead of Location, because when the process runs as a service or in a unit test the assemblies
			//are shadow copied and Location returns the path to the shadow copy.  CodeBase returns the original location 
			//which is the location we really want.
			//Assembly.CodeBase returns the path in URI format, but we need the real path, so run the uri path through
			//Path.GetDirectory name to convert the path seperators to the system path separator, and pull off the assmembly
			//name.  Then get SubString(8) to pull off the file:/// portion of the string
			string executionPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8);
			executionPath = System.IO.Path.GetDirectoryName(executionPath);
			return executionPath;
		}
	}

    public enum ExportFileType
    {
        CSV,
        XML
    };
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

using Aucent.MAX.AXE.Common.ZipCompressDecompress.Zip;


namespace Aucent.MAX.AXE.Common.Data.AssemblyIO
{
	public class AssemblyFS
	{
		protected Assembly asm = null;
		public Assembly ASM
		{
			get { return this.asm; }
			protected set { this.asm = value; }
		}

		protected AssemblyFile file = null;
		public AssemblyFile File
		{
			get { return this.file; }
			protected set { this.file = value; }
		}

		public AssemblyFS( Assembly asm )
		{
			this.ASM = asm;
			this.File = new AssemblyFile( asm );
		}

		public bool Synchronize( string pathToPopulate, out string error )
		{
			error = string.Empty;
		
			try
			{
				if( !Directory.Exists( pathToPopulate ) )
				{
					error = "The destination path could not be found or created." + Environment.NewLine + pathToPopulate;;
					Directory.CreateDirectory( pathToPopulate );
				}
				
				AssemblyName asmNameTmp = this.ASM.GetName();
				string asmName = asmNameTmp.Name;

				string[] resources = this.ASM.GetManifestResourceNames();
				foreach( string res in resources )
				{
					#pragma warning disable 0219
					ManifestResourceInfo mri = this.ASM.GetManifestResourceInfo( res );
					#pragma warning restore 0219

					string resPath = res.Replace( asmName, string.Empty );
					resPath = resPath.TrimStart( '.' );
					resPath = resPath.Replace( '.', Path.DirectorySeparatorChar );
					
					//If it looks like we had a file extension, repair that...
					int lastSlash = resPath.LastIndexOf( Path.DirectorySeparatorChar );
					if( lastSlash + 5 >= resPath.Length )
					{
						resPath = resPath.Insert( lastSlash, "." );
						resPath = resPath.Remove( lastSlash + 1, 1 );
						
						//Get the new "last slash" so that we can find what the destination should be
						lastSlash = resPath.LastIndexOf( Path.DirectorySeparatorChar );
					}

					string filename = resPath.Substring( lastSlash + 1 );
					string subDirPath = pathToPopulate;
					if( lastSlash >= 0 )
					{
						string directory = resPath.Substring( 0, lastSlash );
						subDirPath = Path.Combine( pathToPopulate, directory );
						if( !Directory.Exists( subDirPath ) )
						{
							error = "A subdirectory of the destination could not be found or created." + Environment.NewLine + subDirPath;
							Directory.CreateDirectory( subDirPath );
						}
					}

					string filePath = Path.Combine( subDirPath, filename );
					this.File.Copy( resPath, filePath, true );

                    if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        //we will have to unzip it... in place...
                        string[] files;
                        string errorstr;
                        ZipUtils.TryUnzipAndUncompressFiles(filePath,
                            Path.GetDirectoryName(filePath), out files, out errorstr);
                       
                    }
				}
				
				return true;
			}
			catch{}

			return false;
		}

		public class AssemblyFile
		{
			protected Assembly asm = null;
			public Assembly ASM
			{
				get { return this.asm; }
				protected set { this.asm = value; }
			}

			protected string asmName = null;
			public string ASMName
			{
				get { return this.asmName; }
				protected set { this.asmName = value; }
			}

			public AssemblyFile( Assembly asm )
			{
				this.ASM = asm;
				this.ASMName = asm.GetName().Name;
			}

			public void Copy( string sourceFileName, string destFileName )
			{
				this.Copy( sourceFileName, destFileName );
			}

			public void Copy( string sourceFileName, string destFileName, bool overwrite )
			{
				if( !overwrite && System.IO.File.Exists( destFileName ) )
					return;

				using( Stream s = this.Open( sourceFileName ) )
				{
					using( FileStream fs = new FileStream( destFileName, FileMode.Create, FileAccess.Write ) )
					{
						byte[] buffer = new byte[ s.Length ];
						s.Read( buffer, 0, (int)s.Length );
						fs.Write( buffer, 0, (int)s.Length );
					}
				}
			}

			public bool Exists( string path )
			{		
				string newPath = this.BuildPath( path );
				ManifestResourceInfo mri = this.ASM.GetManifestResourceInfo( newPath );
				bool exists = !( mri == null );
				return exists;
			}

			public Stream Open( string path )
			{
				string newPath = this.BuildPath( path );
				return this.ASM.GetManifestResourceStream( newPath );
			}

			public StreamReader OpenText( string path )
			{
				Stream s = this.Open( path );
				StreamReader sr = new StreamReader( s );
				return sr;
			}

			public byte[] ReadAllBytes( string path )
			{
				byte[] buffer = new byte[0];
				using( Stream s = this.Open( path ) )
				{
					s.Read( buffer, 0, (int)s.Length );
				}
				return buffer;
			}

			public string[] ReadAllLines( string path )
			{
				return this.ReadAllLines( path, Encoding.Default );
			}

			public string[] ReadAllLines( string path, Encoding encoding )
			{
				List<string> lines = new List<string>();
				using( Stream s = this.Open( path ) )
				{
					using( StreamReader sr = new StreamReader( s, encoding ) )
					{
						while( !sr.EndOfStream )
						{
							lines.Add( sr.ReadLine() );
						}
					}
				}
				return lines.ToArray();
			}
			
			public string ReadAllText( string path )
			{
				return this.ReadAllText( path, Encoding.Default );
			}

			public string ReadAllText( string path, Encoding encoding )
			{
				string text = string.Empty;
				using( Stream s = this.Open( path ) )
				{
					using( StreamReader sr = new StreamReader( s, encoding ) )
					{
						while( !sr.EndOfStream )
						{
							text = sr.ReadToEnd();
						}
					}
				}
				return text;
			}
			

			public void Replace( string sourceFileName, string destinationFileName, string destinationBackupFileName )
			{
				System.IO.File.Move( destinationFileName, destinationFileName );
				this.Copy( sourceFileName, destinationFileName );
			}

			protected string BuildPath( string path )
			{
				string newPath = path.Replace( Path.DirectorySeparatorChar, '.' );
				newPath = AssemblyPath.Combine( this.ASMName, newPath );
				return newPath;
			}
		}
		
		public static class AssemblyPath
		{
			public static string Combine( params string[] pieces )
			{
				string path = string.Join( ".", pieces );
				return path;
			}
		}


       
	}

    internal static class ZipUtils
    {

        #region public static methods

        
       
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
                        {
                            Directory.CreateDirectory(dirName);
                        }

                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }


                        // create the file
                        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            long dataSize = theEntry.Size > 0 ? theEntry.Size : 1024;
                            byte[] data = new byte[dataSize];
                            int size = 0;
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    fs.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (fullPath != fileName)
                        {
                            //defect 1969 fix
                            //if the fullPath has been manipulated above, the path may not yet exist
                            dirName = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(dirName))
                            {
                                Directory.CreateDirectory(dirName);
                            }

                            //If a file with the same name exists in the target path then we need to delete it before
                            //we move otherwise .NET will throw an exception
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
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
                catch (Exception)
                {
                    ok = false;
                }
            }
            return ok;
        }

        #endregion public static methods
    }
}
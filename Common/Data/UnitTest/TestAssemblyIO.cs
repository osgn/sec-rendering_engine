using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.IO;
using Aucent.MAX.AXE.Common.Data.AssemblyIO;

namespace Aucent.MAX.AXE.Common.Data.Nunit
{
	[TestFixture]
	public class TestAssemblyIO
	{
		[Test]
		public void Test_Synchronize()
		{
			string pathToPopulate = Path.GetTempPath();
		
			Assembly asm = Assembly.GetExecutingAssembly();
			AssemblyFS asmIO = new AssemblyFS( asm );

			string error = string.Empty;
			if( !asmIO.Synchronize( pathToPopulate, out error ) )
				Assert.Fail( error );

			AssemblyName asmNameTmp = asmIO.ASM.GetName();
			string asmName = asmNameTmp.Name;
			
			string[] resources = asmIO.ASM.GetManifestResourceNames();
			foreach( string res in resources )
			{
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
				string directory = resPath.Substring( 0, lastSlash );

				string subDirPath = Path.Combine( pathToPopulate, directory );
				string filePath = Path.Combine( subDirPath, filename );
				Assert.IsTrue( File.Exists( filePath ) );
			}
		}
	}
}

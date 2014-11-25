//=============================================================================
// TestCompress (class)
// Aucent Corporation
//=============================================================================

#if UNITTEST
namespace ZipCompressDecompress.UnitTests.Test
{
	using System;
	using System.IO;
	using System.Reflection;

    using NUnit.Framework;
	using Aucent.MAX.AXE.Common.ZipCompressDecompress.Zip;

    [TestFixture] 
    public class TestCompress
    {
		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
        [TestFixtureSetUp] public void RunFirst()
        {}

        /// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
        [TestFixtureTearDown] public void RunLast() 
        {}

		/// <summary> Sets up test values before each test is called </summary>
        [SetUp] public void RunBeforeEachTest()
        {}

        /// <summary>Tears down test values after each test is run </summary>
        [TearDown] public void RunAfterEachTest() 
        {}
		#endregion

		protected byte[] ExtractTestFolio123()
		{
			Assembly assem = this.GetType().Assembly;   
			//string[] resNames = assem.GetManifestResourceNames();
			byte[] bytes = null;

			using( Stream stream = assem.GetManifestResourceStream("ZipCompressDecompress.UnitTests.Folio 123.xls") )   
			{
				bytes = new byte[stream.Length];

				stream.Read( bytes, 0, (int)stream.Length );
			}

			return bytes;
		}

		[Test] public void TestCompressDecompress()
		{
			// get the bytes
			byte[] bytes = ExtractTestFolio123();

			Assert.IsFalse( bytes.Length == 0, "no bytes read or returned" );

			// and compress them
			byte[] compressedBytes = null;
			using ( MemoryStream ms = new MemoryStream() )
			{
				using ( ZipOutputStream zos = new ZipOutputStream( ms ) )
				{
					zos.SetLevel(9);	// max compression
					// add an entry
					zos.PutNextEntry( new ZipEntry( "test" ) );

					zos.Write( bytes, 0, bytes.Length );
				}

				compressedBytes = ms.ToArray();
			}

			// and decompress

			// and compare the returned bytes against the original bytes
			byte[] uncompressedBytes = null;
			using ( MemoryStream ms = new MemoryStream( compressedBytes ) )
			{
				// decompress the file
				using ( ZipInputStream zis = new ZipInputStream( ms  ) )
				{
					// should only be on entry per file
					ZipEntry entry = zis.GetNextEntry();
					uncompressedBytes = new byte[ entry.Size ];
					zis.Read( uncompressedBytes, 0, (int)entry.Size );
				}
			}

			// and compare them
			Assert.AreEqual( bytes.Length, uncompressedBytes.Length, "byte lengths are different" );

			for ( int i=0; i < bytes.Length; ++i )
			{
				Assert.AreEqual( bytes[i], uncompressedBytes[i], "bytes differ at position " + i );

			}
		}
    }

}
#endif
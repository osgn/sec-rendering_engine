using System;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Aucent.MAX.AXE.Common.Utilities
{
	public class SerializationUtilities
	{
		public static bool TryXmlSerializeObjectToString(object obj, out string objString, out string errorMsg)
		{
			objString = string.Empty;
			errorMsg = string.Empty;

			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

				MemoryStream writeStream = new MemoryStream();
				XmlTextWriter xmlTextWriter = new XmlTextWriter(writeStream, Encoding.UTF8);
				xmlSerializer.Serialize(xmlTextWriter, obj);
				xmlTextWriter.Close();

				objString = Encoding.UTF8.GetString(writeStream.ToArray());

				return true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
				return false;
			}
		}

		public static bool TryXmlDeserializeStringToObject(string xml, Type type, out object objOut, out string errorMsg)
		{
			objOut = null;
			errorMsg = string.Empty;

			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(type);

				MemoryStream streamRead = new MemoryStream(Encoding.UTF8.GetBytes(xml));
				XmlTextReader xmlTextReader = new XmlTextReader(streamRead);
				objOut = xmlSerializer.Deserialize(xmlTextReader);
				xmlTextReader.Close();

				return true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
				return false;
			}
		}

        private static readonly byte[] alg = Encoding.ASCII.GetBytes("RIVETSOFT2202008");
        private static readonly byte[] iv = Encoding.ASCII.GetBytes("RIVET SOFTWARE IV");

        public static string DecryptFromFile(string filePath)
        {
			try
			{
				using( FileStream fStream = File.Open( filePath, FileMode.Open, FileAccess.Read ) )
				{
					Rijndael algorithm = Rijndael.Create();
					using( CryptoStream cStream = new CryptoStream( fStream, algorithm.CreateDecryptor( alg, iv ), CryptoStreamMode.Read ) )
					{
						using( StreamReader sReader = new StreamReader( cStream ) )
						{
							return sReader.ReadLine();
						}
					}
				}
			}
			catch{}

			return string.Empty;
        }

        public static void EncryptToFile(string filePath, string contents)
        {
			try
			{
				using( FileStream fStream = File.Open( filePath, FileMode.Create, FileAccess.Write ) )
				{
					Rijndael algorithm = Rijndael.Create();
					using( CryptoStream cStream = new CryptoStream( fStream, algorithm.CreateEncryptor( alg, iv ), CryptoStreamMode.Write ) )
					{
						using( StreamWriter sWriter = new StreamWriter( cStream ) )
						{
							sWriter.WriteLine( contents );
						}
					}
				}
			}
			catch { }
        }

        public static string Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

	}
}

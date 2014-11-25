using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Aucent.MAX.AXE.Common.Utilities
{
    [global::System.Reflection.Obfuscation(Exclude = true)]
    public static class TextUtilities
	{
		public static string CleanHTML(string html, bool removeHtml)
		{
			string htmlCopy = html;

			//work on just the body
			htmlCopy = TextUtilities.GetHtmlBody( htmlCopy );

			//Remove <!--comments-->
			htmlCopy = Regex.Replace( htmlCopy, @"<!--[^-]*-->", string.Empty, RegexOptions.None ).Trim();

			//Remove XML Declarations
			htmlCopy = Regex.Replace( htmlCopy, @"<\?xml:[^>]*>", string.Empty, RegexOptions.None ).Trim();

			if( removeHtml )
			{
				//Remove any html node
				htmlCopy = Regex.Replace( htmlCopy, @"<\/?[a-zA-Z]{1}\w*[^>]*>", string.Empty, RegexOptions.None ).Trim();

				//The previous comment removal only removes single line comments
				htmlCopy = Regex.Replace( htmlCopy, @"<!--.*?-->", string.Empty, RegexOptions.Singleline );
			}
			else
			{
				//remove o tags
				htmlCopy = Regex.Replace( htmlCopy, @"</?o:[^>]*>", string.Empty );

				//remove st1 tags
				htmlCopy = Regex.Replace( htmlCopy, @"</?st1:[^>]*>", string.Empty );

				//Fix Quotes and try to remove 
				htmlCopy = Regex.Replace( htmlCopy, @"<[\w]+[^>]*>", new MatchEvaluator( FixQuotes ) );
			}

			return htmlCopy;
		}


		public static string ConvertUtf8ToAscii(string utf8)
		{
			//Always perform these kinds of string replacements from
			//  larger search string, to smaller.

			StringBuilder sb = new StringBuilder(utf8);
			sb.Replace("‚Äú", "\"")
				.Replace("‚Äù", "\"")
				.Replace("‚Äô", "'")
				.Replace("‚Äî", "-")
				.Replace("‚Äì", "-")
				.Replace("‚Äë", "-")
				.Replace("‚Ñ¢", "(TM)")
				.Replace("‚Ç¨", "Ä")
				.Replace("‚Öï", "1/5")
				.Replace("‚Öñ", "2/5")
				.Replace("‚Öì", "1/3")
				.Replace("‚Öî", "2/3")
				.Replace("√§", "(TM)")
				.Replace("√∂", "o")
				.Replace("©", "(C)")
				.Replace("Æ", "(R)")
				.Replace("ô", "(TM)")
				.Replace("ó", "-");

			//After all of the above replacements,
			//  this is usually a 'garbage' character
			sb.Replace("¬", string.Empty);

			return sb.ToString();
		}

		/// <summary>
		/// http://www.w3.org/TR/html4/sgml/entities.html + edgar document
		/// list of allowed utf8 characters in the text block values...that needs to be converted to ascii
		/// list from http://www.w3.org/TR/html4/sgml/entities.html
		/// 160 -255 
		/// 338-339
		/// 352-353
		/// 376
		/// 402
		/// 710
		/// 732
		/// 913-929
		/// 931-937
		/// 945-969
		/// 977-978
		/// 982
		/// 8194-8195
		/// 8201
		/// 8204-8207
		/// 8211-8212
		/// 8216-8218
		/// 8220-8222
		/// 8224-8226
		/// 8230
		/// 8240
		/// 8242-8243
		/// 8249-8250
		/// 8254
		/// 8260
		/// 8364
		/// 8472
		/// 8465
		/// 8476
		/// 8482
		/// 8501
		/// 8592-8596
		/// 8629
		/// 8656-8660
		/// 8704
		/// 8706
		/// 8707
		/// 8709
		/// 8711-8713
		/// 8715
		/// 8719
		/// 8721
		/// 8722
		/// 8727
		/// 8730
		/// 8733-8734
		/// 8736
		/// 8743-8747
		/// 8756
		/// 8764
		/// 8773
		/// 8776
		/// 8800-8801
		/// 8804-8805
		/// 8834-8836
		/// 8838-8839
		/// 8853
		/// 8855
		/// 8869
		/// 8901
		/// 8968-8971
		/// 9001-9002
		/// 9674-
		/// 9824
		/// 9827
		/// 9829-9830
		/// List from edgar not in the html reference
		/// 131
		/// 133-140
		/// 149
		/// 153-156
		/// 159
		/// </summary>
		/// <param name="utf8"></param>
		/// <returns></returns>
		public static string CovertValidUTF8ToAsciiInTextBlock( string utf8 )
		{
#if DEBUG
			bool showErrors = true;
#endif

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < utf8.Length; i++)
			{
				if (utf8[i] <= 127  )
				{
					sb.Append(utf8[i]);
				}
				else
				{
					bool allow = false;
					char c = utf8[i];
					if (c >= 160 && c <= 255)
					{
						allow = true;
					}
					else if (c == 8364 || c == 8482 || c == 339 ) //euro sign and other commn symbols,,
					{
						allow = true;
					}
					else if (c <= 159)
					{

						/// 131
						/// 133-140
						/// 149
						/// 153-156
						/// 
                        if (c == 131 || (c >= 133 && c <= 140) || (c >= 145 && c <= 149) || (c >= 153 && c <= 156) || c == 159)
                        {
                            allow = true;
                        }

					}
					else if (c < 1000 || c >= 8194 && c <= 8971)
					{
						allow = GetISO8859Chars().BinarySearch(c) >= 0;
					}

                    if (allow)
                    {
                        int x = c;
                        sb.Append(string.Format("&#{0};", x));
                    }
                    else
                    {
#if DEBUG
						int x = c;
						string logMessage = "Discarding a UTF8 character: " + string.Format( "&#{0};", x );
						Debug.WriteLine( logMessage );
						Console.WriteLine( logMessage );

						if( showErrors )
						{
							DialogResult res = MessageBox.Show(
								logMessage + Environment.NewLine+
								"Click 'Cancel' to stop these messages.",
								"Debug message",
								MessageBoxButtons.OKCancel );

							showErrors = res == DialogResult.OK;
						}
#endif

                    }
					
				


					
				}
			}

			
			//sb.Replace("£", "&#163;")
			//    .Replace("Ä", "&#8364;")
			//    .Replace("•", "&#165;");

			return sb.ToString();
		}


		/// <summary>
	    /// As of we convert few of the known non ascii to a comparable ascii
		/// everything else will get dropped from the label
		/// </summary>
		/// <param name="utf8"></param>
		/// <returns></returns>
		public static string CovertValidUTF8ToAsciiInLabel(string utf8)
		{

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < utf8.Length; i++)
			{
				if (utf8[i] <= 127)
				{
					sb.Append(utf8[i]);
				}
				else
				{

					if (utf8[i] == 160) //non breaking space getsa converted to simple space.
					{
						sb.Append(' ');

					}
					else if (utf8[i] == 8211 || utf8[i] == 8212 || utf8[i] == 150 || utf8[i] == 151) //ó ( word created long dash . it is char 8212 )
					{
						sb.Append("-"); //simple dash

					}
					else if (utf8[i] == 145 || utf8[i] == 146 || utf8[i] == 8216 || utf8[i] == 8217)
					{
						sb.Append("'"); // simple single quote

					}
					else if (utf8[i] == 147 || utf8[i] == 148 || utf8[i] == 8220 || utf8[i] == 8221)
					{
						sb.Append('"'); //simple double quote

					}
					else if (utf8[i] == 130 )
					{
						sb.Append(','); //simple coma

					}
					else if (utf8[i] == 133)
					{
						sb.Append("..."); //simple three dots

					}



				}
			}


			//sb.Replace("£", "&#163;")
			//    .Replace("Ä", "&#8364;")
			//    .Replace("•", "&#165;");

			return sb.ToString();
		}


		private static List<int> additionalISO8859Chars = null;

		private static List<int> GetISO8859Chars()
		{
			if (additionalISO8859Chars == null)
			{
				additionalISO8859Chars = new List<int>();



				for (int c = 913; c <= 929; c++)
				{
					additionalISO8859Chars.Add(c);
				}
				for (int c = 931; c <= 937; c++)
				{
					additionalISO8859Chars.Add(c);
				}
				for (int c = 945; c <= 969; c++)
				{
					additionalISO8859Chars.Add(c);
				}
				additionalISO8859Chars.AddRange(new int[] { 338, 339, 352, 353, 376, 402, 710, 732, 977, 978, 982 });

			

				additionalISO8859Chars.AddRange(new int[] { 8194, 8195,8201,
					8204, 8205, 8206,
					8211,8212,8216, 8217,8218,
					8220,8221,8222,8224, 8225,8226,
					8230,8240,8242,8243,8249, 8250,8254,
					8260,8364,8465,8476,8472, 8482,8501,
					8531, 8532, 8533, 8534, 8535, 3536,     // 1/3, 2/3, 1/5, 2/5, 3/5, 4/5
                    8537, 8538, 8539, 8540, 8541, 8342,    // 1/6, 5/6, 1/8, 3/8, 5/8, 7/8
					8592,8593,8594,8595,8596,8629,
					8656,8657,8658,8659,8660,
					8704,8706,8707,8709,
				    8711, 8712, 8713,
					8715,8719,8721,8722,8727,8730,8730, 8733, 8734, 8736, 
		            8743,8744,8745,8746,8747,
		            8756,8764,8773,8776,
					8800,8801,
					8804,8805,
					8834, 8835, 8836,
					8968, 8969, 8970, 8971, 
					8838, 8839, 8853, 8855, 8869, 8901 });
				additionalISO8859Chars.AddRange(new int[] { 9001, 9002, 9674, 9824, 9827, 9829, 9830 });

				additionalISO8859Chars.Sort();
				
			}

			return additionalISO8859Chars;
		}

		private static string FixQuotes(Match match)
		{
			StringBuilder element = new StringBuilder(match.Value);
			MatchCollection matches = Regex.Matches(element.ToString(), @"[\w-]+=(\w+|'[^']*')");

			foreach (Match m in matches)
			{
				//MSO puts double quotes around font names - remove them.
				string quotesFixed = m.Groups[ 1 ].Value.Replace( "\"", string.Empty );
				quotesFixed = quotesFixed.Trim( '\'' );

				//Since the regex grabs the entire attribute, updated the value, and replace the entire attribute
				quotesFixed = m.Value.Replace( m.Groups[1].Value, "\"" + quotesFixed + "\"" );

				string msoBookmarkFixed = Regex.Replace(quotesFixed, @"mso-bookmark:\s*_\w+;?", string.Empty);
				if (msoBookmarkFixed == "style=\"\"")
				{
					msoBookmarkFixed = string.Empty;
				}

				//Update the "reference
				element.Replace(m.Value, msoBookmarkFixed);
			}

			return element.ToString();
		}

		public static string[] GetAllBetween( string htmlToSearch, string tag )
		{
			List<string> found = new List<string>();
			
			#pragma warning disable 0219
			string ret = htmlToSearch;
			#pragma warning restore 0219
			
			string regex = string.Format( @"<{0}[^>]*>(.*?)</{0}>", tag );

			MatchCollection matches = Regex.Matches( htmlToSearch, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if( matches.Count > 0 )
			{
				foreach( Match styleMatch in matches )
				{
					if( styleMatch.Success )
					{
						found.Add( styleMatch.Groups[ 1 ].Value );
					}
				}
				return found.ToArray();
			}

			return new string[0];
		}

		public static string GetBetween( string htmlToSearch, string tag)
		{
  		#pragma warning disable 0219
			string ret = htmlToSearch;
			#pragma warning restore 0219
			
			string regex = string.Format( @"<{0}[^>]*>(.*)</{0}>", tag );

			Match m = Regex.Match( htmlToSearch, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if( m.Success )
			{
				return m.Groups[ 1 ].Value;
			}
			else
			{
				return string.Empty;
			}
		}

		public static string GetHeadStyle(string html)
		{
			string head = GetHtmlHead( html );
			if( !string.IsNullOrEmpty( head ) )
			{
				string[] styles = GetStyles( head );
				if( styles.Length > 0 )
				{
					return string.Format( "<style>{0}</style>", string.Join( Environment.NewLine, styles ) );
				}
			}

			return string.Empty;
		}

		public static string GetHtmlBody(string html)
		{
			string body = GetBetween( html, "body" );
			return string.IsNullOrEmpty( body ) ?
				html : body;
		}

		public static string GetHtmlHead(string html)
		{
			return GetBetween( html, "head" );
		}

		public static string[] GetStyles( string html )
		{
			return GetAllBetween( html, "style" );
		}

		public static bool HasTags( string html ){
			return Regex.IsMatch( html, @"<\/?[a-zA-Z]{1}\w*[^>]*>" );
		}

		public static string NormalizeWhitespace( string html )
		{
			string htmlCopy = Regex.Replace( html, @"\s+", " " );
			return htmlCopy;
		}

	}
}
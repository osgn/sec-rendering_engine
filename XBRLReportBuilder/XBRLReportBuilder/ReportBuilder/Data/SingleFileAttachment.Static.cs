using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Data
{
	public partial class SingleFileAttachment
	{
		//http://www.regular-expressions.info/xmlcharclass.html
		public const string REGEX_C = @"[_a-zA-Z0-9:\.\-]";
		public const string REGEX_I = @"[_a-zA-Z:]";

		public static readonly string ELEMENT_PATTERN = null;
		public static readonly Regex ELEMENT_REGEX = null;

		public static readonly string ATTRIBUTE_PATTERN = null;
		public static readonly Regex ATTRIBUTE_REGEX = null;

		static SingleFileAttachment()
		{
			//element

			string tmp = @"<(?<closer>\/)?(?<tag>\i\c*)(?<attributes>[^>]*)?>";
			ELEMENT_PATTERN = tmp.Replace( @"\c", REGEX_C ).Replace( @"\i", REGEX_I );
			ELEMENT_REGEX = new Regex( ELEMENT_PATTERN, RegexOptions.Compiled );

			//attribute
			tmp = @"\s+(?<name>\i\c*)\s*=((?<value>[\w\.]+)|\s*'(?<value>[^']*)'|\s*""(?<value>[^""]*)"")";
			ATTRIBUTE_PATTERN = tmp.Replace( @"\c", REGEX_C ).Replace( @"\i", REGEX_I );
			ATTRIBUTE_REGEX = new Regex( ATTRIBUTE_PATTERN, RegexOptions.Compiled );
		}

		public static string WrapStringAt( string src, int size )
		{
			int idx = 0;
			int rem = src.Length;
			StringBuilder sb = new StringBuilder();

			while( rem > 0 )
			{
				int sel = Math.Min( rem, size );
				sb.AppendLine( src.Substring( idx, sel ) );
				idx += sel;
				rem -= sel;
			}

			return sb.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="html"></param>
		public string ScrubHTML( string html )
		{
			string cleanHTML = ELEMENT_REGEX.Replace( html, new MatchEvaluator( ScrubHTMLElement ) );
			return cleanHTML;
		}

		private string currentElement = string.Empty;
		private string ScrubHTMLElement( Match m )
		{
			if( m.Groups[ "attributes" ].Value.Length == 0 )
				return m.Value;

			string newElement = "<";

			this.currentElement = m.Groups[ "tag" ].Value;
			string newAttStr = ATTRIBUTE_REGEX.Replace( m.Groups[ "attributes" ].Value, new MatchEvaluator( ScrubHTMLAttribute ) );

			newElement += m.Groups[ "closer" ].Value;
			newElement += m.Groups[ "tag" ].Value;
			newElement += newAttStr;
			newElement += ">";

			return newElement;
		}

		private string ScrubHTMLAttribute( Match m )
		{
			string newAttribute = string.Empty;

			Match space = Regex.Match( m.Value, @"^\s*" );
			if( space.Success )
				newAttribute += space.Value;

			string name = m.Groups[ "name" ].Value;
			string value = m.Groups[ "value" ].Value;

			//is it a TD?
			if( string.Compare( this.currentElement, "td", true ) == 0 )
			{
				//is it the height attribute?
				if( string.Compare( name, "height", true ) == 0 )
				{
					//remove it!
					return string.Empty;
				}
			}

			//is it a TR?
			if( string.Compare( this.currentElement, "td", true ) == 0 )
			{
				//is it the height attribute?
				if( string.Compare( name, "height", true ) == 0 )
				{
					//remove it!
					return string.Empty;
				}
			}

			if( string.Compare( name, "style", true ) == 0 )
			{
				value = CleanStyles( value );
				if( string.IsNullOrEmpty( value ) )
					return string.Empty;
			}

			if( Regex.IsMatch( value, @"\s" ) )
				newAttribute += name + "=3D'" + value + "'";
			else
				newAttribute += name + "=3D" + value;

			return newAttribute;
		}

		private string CleanStyles( string value )
		{
			try
			{
				bool isTDorTR = string.Compare( this.currentElement, "td", true ) == 0;
				if( !isTDorTR )
					isTDorTR = string.Compare( this.currentElement, "tr", true ) == 0;

				Dictionary<string, string> styles = GetStyles( value );

				string[] keys = new string[ styles.Count ];
				styles.Keys.CopyTo( keys, 0 );

				bool changed = false;
				foreach( string key in keys )
				{
					if( isTDorTR )
					{
						if( string.Compare( key, "height", true ) == 0 )
						{
							changed = true;
							styles.Remove( key );
                            continue;
						}
					}

                    if (string.Compare(key, "font-family", true) == 0)
                    {
                        if (string.Compare(styles[key], "Symbol", true) == 0)
                        {
                            changed = true;
                            styles.Remove(key);
                            continue;
                        }
                    }

					if( string.Compare( key, "mso-bidi-font-family", true ) == 0 )
					{
                        if( string.Compare( styles[ key ], "Symbol", true ) == 0 )
                        {
						    changed = true;
						    styles.Remove( key );
                            continue;
                        }
					}

                    if (string.Compare(key, "mso-fareast-font-family", true) == 0)
                    {
                        if (string.Compare(styles[key], "Symbol", true) == 0)
                        {
                            changed = true;
                            styles.Remove(key);
                            continue;
                        }
                    }
				}

				if( !changed )
					return value;

				StringBuilder sb = new StringBuilder();
				foreach( string key in styles.Keys )
				{
					sb.Append( key + ": " + styles[ key ] + "; " );
				}

				string newValue = sb.ToString();
				return newValue.Trim();
			}
			catch
			{ }

			return value;
		}

        public static Dictionary<string, string> GetStyles(string styleBlock)
        {
            Dictionary<string, string> styleValues = new Dictionary<string, string>();
            string[] styleStrings = styleBlock.Split(';');
            foreach (string styleString in styleStrings)
            {
                try
                {
                    string[] styleValue = styleString.Split(':');
                    if (styleValue.Length != 2)
                        continue;

                    string style = styleValue[0].Trim().ToLower();
                    string value = styleValue[1].Replace("\"", "'").Trim();
                    styleValues[style] = value;
                }
                catch { }
            }

            return styleValues;
        }
	}
}

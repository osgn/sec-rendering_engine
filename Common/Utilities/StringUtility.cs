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
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// StringUtility
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
    [global::System.Reflection.Obfuscation(Exclude = true)]
    public static class StringUtility
	{
      
		#region constants

		public const string InvalidString				= @"~`!@#$%^&*()+={}[]|\:;’'<>?,.£€";
		public const string InvalidElementNameString	= @"~`!@#$%^&*()+={}[]|\/<>:;’'<>?,.-–";
		public const string InvalidNamespaceString	= @"~`!@#$%^&*()+={}[]|\/:;’'<>?,";
		public const string InvalidSchemeString		= @"~`!@#$%^&*()+={}[]|;’'<>?,";
		public const string NumberString			= @"0123456789";

		public static char[] InvalidElementNameCharacters;

		private static List<string> LC3ConnectiveWords;

		#endregion

		static StringUtility()
		{
			InvalidElementNameCharacters = InvalidElementNameString.ToCharArray();

			//create a string array of prohibited connective words, per FRTA
			LC3ConnectiveWords = new List<string>();
			LC3ConnectiveWords.Add( "the" );
			LC3ConnectiveWords.Add("a");
			LC3ConnectiveWords.Add("an");
			LC3ConnectiveWords.Sort();

		}

		public static bool ContainsCharacters(string target, string testString, bool checkDoubleQuote)
		{
			if (target == null || testString == null)
				return false;

			char [] testStringArray = testString.ToCharArray();
			bool found = (target.IndexOfAny(testStringArray) >= 0);
			if (!found && checkDoubleQuote)
				found = (target.IndexOf('"') >= 0);

			return found;
		}

		/// <summary>
		/// Determines if the given name is FRTA compliant.  
		/// </summary>
		/// <param name="NameString"></param>
		/// <returns></returns>
		public static bool IsNameFRTAValid(string TrimmedName)
		{
			// Determines if the given name is FRTA compliant;
			// A name must start with an alphabetic character
			// A name must not contain spaces
			// A name must not contain special characters
			// A name must not exceed 256 chars

			if ( (TrimmedName == null ) ||TrimmedName == string.Empty ) 
				return false;

			// Check length
			if (TrimmedName.Length >= 256)
				return false;

			// Check for spaces
			if (TrimmedName.IndexOf(" ") > -1 )
				return false;

			// Check for special characters
			if ( StringUtility.ContainsCharacters( TrimmedName, StringUtility.InvalidString, true ) )
				return false;

			if (TrimmedName.IndexOfAny(new char[]{'-','"', ' '}) > -1 )
				return false;

			// Check to see if it starts with a digit;
			if (char.IsDigit((TrimmedName.ToCharArray())[0]))
				return false;

			return true;
		}

		/// <summary>
		/// Checks string to see whether all characters are digits
		/// </summary>
		/// <param name="theString"></param>
		/// <returns></returns>
		public static bool IsDigitsOnly(string theString)
		{
			if( (theString == null) || (theString.Length == 0) )
				return false;
		
			foreach(char c in theString)
			{
				if( !char.IsDigit(c) )
					return false;
			}

			return true;
		}

		public static string FormatStringForErrorMessage(string strToFormat)
		{
			if (strToFormat == null)
				return string.Empty;

			StringBuilder formattedStr = new StringBuilder(strToFormat.Length * 2);
			foreach(char character in strToFormat)
			{
				formattedStr.Append(character);
				formattedStr.Append(" ");
			}

			return formattedStr.ToString().Trim();
		}

        public static string GetLC3LabelFromElementName(string elementName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(elementName[0]);
            for (int i = 1; i < elementName.Length; i++)
            {
                char c = elementName[i];
                if (c >= 'A' && c <= 'Z')
                {
                    sb.Append(' ');
                }
                sb.Append(elementName[i]);

            }

            return sb.ToString();
        }

		public static string GetDefaultElementName( string labelName )
		{
			StringBuilder sb = new StringBuilder();

			//trim and change to lower
			labelName = labelName.ToLower().Trim();

			// Fix 5937 - There is a char 160 (non breaking space) in the string
			// so the element name would have invalid chars in it and creating instance doc fails.
			labelName = StringUtility.ReplaceUnprintableCharacters( labelName, string.Empty );


			//replace InvalidElementNameCharacters with " " so that we can do a camel case around it.
			int index = labelName.IndexOfAny(InvalidElementNameCharacters);

			while (index != -1)
			{
				labelName = labelName.Replace(labelName[index], ' ');
				index = labelName.IndexOfAny(InvalidElementNameCharacters);
			}


			//change to lowerCamelCase
			string[] words = labelName.Split( ' ' );
			foreach ( string word in words )
			{
				if ( word.Length > 0 )
				{
					//omit LC3 connective words
					if ( LC3ConnectiveWords.BinarySearch( word ) < 0 )
						sb.Append( char.ToUpper( word[0] ) ).Append( word.Substring( 1, word.Length - 1 ) );
				}
			}


			//CEE: Changed this to ensure ALL whitespace gets removed
			string defaultName = Regex.Replace( sb.ToString(), @"\s+", string.Empty );
			return defaultName;
		}



		public static string ReplaceUnprintableCharacters( string stringIn, string replacement )
		{
			if ( string.IsNullOrEmpty( stringIn ) )
			{
				return stringIn;
			}

			//The range of printable characters are the characters with ASCII code 32-126
			//build a regex string the contains all of the characters concatinated together
			StringBuilder sbValidCharacters = new StringBuilder();
			for ( int idx = 32; idx <= 126; idx++ )
			{
				sbValidCharacters.Append( (char)idx );
			}

			//Include newlines, carraige returns and tabs in the characters that we want to keep
			sbValidCharacters.Append( '\n' ).Append( '\r' ).Append( '\t' );

			//Build the regex as the set of characters NOT in the set that we want to keep
			Regex regExp = new Regex( "[^" + sbValidCharacters.ToString() + "]{1}" );


			//Replace the characters that are not valid with string.Empty
			return regExp.Replace( stringIn, string.Empty );

		}


       
       

		public static string RemoveXBRLUrl(string inString)
		{
			string outString = string.Empty;

			outString = Regex.Replace(inString, @"(http://\S*xbrl.org\S*)", "");

			return outString;
		}

		public static bool IsSECFileNameValid(string fileName,
			out string prefix, out string secVersionDate)
		{
			prefix = secVersionDate = string.Empty;
			string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);

			if (string.IsNullOrEmpty(fileNameWithoutExt))
				return false;

			Match m = Regex.Match(fileNameWithoutExt, @"^(.*-(\d{8}))$", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				prefix = m.Groups[1].Value;
				secVersionDate = string.Format("{0}-{1}-{2}", m.Groups[2].Value.Substring(0, 4),
					m.Groups[2].Value.Substring(4, 2), m.Groups[2].Value.Substring(6, 2));

				DateTime isValidDate;
				if (!DateTime.TryParse(secVersionDate, out isValidDate))
				{

					return false;
				}
			}

			return m.Success;
		}


        public static string GetRoleURIFromTitle(string definition, string weblocation, out string roleID)
        {

            weblocation = weblocation.Trim();
            if (!weblocation.EndsWith("/")) weblocation += "/";

            roleID = definition;

			//curName += e.KeyCode.ToString();
			//need to strip out all leading numerics...
			int startIndex = 0;
            for (; startIndex < roleID.Length; startIndex++)
			{
                if ((roleID[startIndex] >= 'a' && roleID[startIndex] <= 'z') ||
                      (roleID[startIndex] >= 'A' && roleID[startIndex] <= 'Z'))
				{
                    break ;
				}

			}
            if (startIndex >= roleID.Length)
			{
                roleID = string.Empty;
			}
			else
			{
                roleID = roleID.Substring(startIndex);
                roleID = GetDefaultElementName(roleID);

				//for simplicity just get the alphanumeric part of the curName information
				//this would make sure none of utf 8 weird characters get 
                roleID = System.Text.RegularExpressions.Regex.Replace(roleID, "[^a-zA-Z0-9]*", string.Empty);

			}

            return string.Format("{0}role/{1}", weblocation, roleID);
		
        }

	}
}
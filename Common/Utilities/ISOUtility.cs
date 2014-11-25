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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// ISO
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
    [global::System.Reflection.Obfuscation(Exclude = true)]
	public class ISOUtility
	{

		#region properties
		protected static ArrayList languageReferences;
		protected static ArrayList currencyCodes;
		private static List<SICReference> sortedSicReferences;
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new ISO.
		/// </summary>
		static ISOUtility()
		{
			CultureInfo[] cls = CultureInfo.GetCultures( CultureTypes.AllCultures );
			foreach( CultureInfo ci in cls )
			{
				languageDescriptions[ ci.Name ] = ci.DisplayName;
			}

			FillCurrencyCodes();
		}

		#endregion

		#region Language Codes

		private static Dictionary<string, string> languageDescriptions = new Dictionary<string, string>();
		/// <summary>
		/// given a language code returns the culture specific description
		/// </summary>
		/// <param name="code"></param>
		/// <param name="desc"></param>
		/// <returns></returns>
		public static bool TryGetLanguageDescription( string code, out string desc )
		{
			lock( languageDescriptions )
			{

				if( languageDescriptions.TryGetValue( code, out desc ) ) return true;

				try
				{
					CultureInfo ci = new CultureInfo( code );
					desc = ci.DisplayName;
				}
				catch( Exception )
				{
					desc = null;
				}
				languageDescriptions[ code ] = desc;

			}

			return desc != null;
		}


		public static StringCollection GetLanguages()
		{
			StringCollection ret = new StringCollection();
			lock( languageDescriptions )
			{

				foreach( KeyValuePair<string, string> kvp in languageDescriptions )
				{
					ret.Add( kvp.Key + " " + kvp.Value );
				}
			}

			return ret;
		}
		#endregion

		#region Currency Codes
		public static CurrencyCode GetCurrencyCode( string code )
		{
			lock( currencyCodes )
			{
				int index = currencyCodes.BinarySearch( code );
				return index > -1 ? (CurrencyCode)currencyCodes[ index ] : null;
			}
		}

		public static ArrayList GetCurrencyCodes()
		{
			lock( currencyCodes )
			{
				return currencyCodes;
			}
		}

		private static string currencyCodePath = null;
		public static ArrayList GetCurrencyCodes( string xmlPath )
		{
			lock( currencyCodes )
			{
				if( string.Equals( currencyCodePath, xmlPath ) )
					return currencyCodes;

				currencyCodes.Clear();
				currencyCodePath = xmlPath;

				if( !File.Exists( xmlPath ) )
					return currencyCodes;

				using( FileStream fs = new FileStream( xmlPath, FileMode.Open, FileAccess.Read ) )
				{
					ISOUtility.CurrencyCode[] tmpCodes = ParseStream( fs );
					currencyCodes.AddRange( tmpCodes );
					currencyCodes.Sort();
				}

				return currencyCodes;
			}
		}

		protected static ArrayList FillCurrencyCodes()
		{
			/* Last Updated: 2007-08-07 from http://www.iso.org/iso/en/prods-services/popstds/currencycodeslist.html */
			if( currencyCodes == null )
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				string resName = asm.GetName().Name + ".CurrencyCodes.xml";
				using( Stream s = asm.GetManifestResourceStream( resName ) )
				{
					CurrencyCode[] tmpCurrencies = ParseStream( s );

					currencyCodes = new ArrayList();
					lock( currencyCodes )
					{
						currencyCodes.AddRange( tmpCurrencies );
						currencyCodes.Sort();
					}
				}

				//currencyCodes = new ArrayList( 185 );
				//lock( currencyCodes )
				//{
				//    currencyCodes.Add( new CurrencyCode( "AED", "UAE Dirham" ) );
				//    currencyCodes.Add( new CurrencyCode( "AFN", "Afghani" ) );
				//    currencyCodes.Add( new CurrencyCode( "ALL", "Albanian Lek" ) );
				//    currencyCodes.Add( new CurrencyCode( "ANG", "Netherlands Antillian Guilder", "&#402;" ) );
				//    currencyCodes.Add( new CurrencyCode( "AOA", "Angolan Kwanza" ) );
				//    currencyCodes.Add( new CurrencyCode( "AMD", "Armenian Dram" ) );
				//    currencyCodes.Add( new CurrencyCode( "ARS", "Argentine Peso", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "AUD", "Australian Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "AWG", "Aruban Guilder", "&#402;" ) );
				//    currencyCodes.Add( new CurrencyCode( "AZN", "Azerbaijanian Manat" ) );
				//    currencyCodes.Add( new CurrencyCode( "BAM", "Bosnia-Herzegovina Convertible Marks" ) );
				//    currencyCodes.Add( new CurrencyCode( "BBD", "Barbados Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "BDT", "Bangladesh Taka" ) );
				//    currencyCodes.Add( new CurrencyCode( "BGN", "Bulgarian Lev" ) );
				//    currencyCodes.Add( new CurrencyCode( "BHD", "Bahraini Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "BIF", "Burundi Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "BMD", "Bermuda Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "BND", "Brunei Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "BOB", "Bolivian Boliviano" ) );
				//    currencyCodes.Add( new CurrencyCode( "BOV", "Bolivian Mvdol" ) );
				//    currencyCodes.Add( new CurrencyCode( "BRL", "Brazilian Real" ) );
				//    currencyCodes.Add( new CurrencyCode( "BSD", "Bahamian Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "BTN", "Bhutan Ngultrum" ) );
				//    currencyCodes.Add( new CurrencyCode( "BWP", "Botswana Pula" ) );
				//    currencyCodes.Add( new CurrencyCode( "BYR", "Belarussian Ruble" ) );
				//    currencyCodes.Add( new CurrencyCode( "BZD", "Belize Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "CAD", "Canadian Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "CDF", "Franc Congolais" ) );
				//    currencyCodes.Add( new CurrencyCode( "CHE", "WIR Euro" ) );
				//    currencyCodes.Add( new CurrencyCode( "CHF", "Swiss Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "CHW", "WIR Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "CLF", "Chilean Unidades de fomento" ) );
				//    currencyCodes.Add( new CurrencyCode( "CLP", "Chilean Peso" ) );
				//    currencyCodes.Add( new CurrencyCode( "CNY", "Yuan Renminbi" ) );
				//    currencyCodes.Add( new CurrencyCode( "COP", "Colombian Peso", "&#8369;" ) );
				//    currencyCodes.Add( new CurrencyCode( "COU", "Columbian Unidad de Valor Real" ) );
				//    currencyCodes.Add( new CurrencyCode( "CRC", "Costa Rican Colon", "&#8353;" ) );
				//    currencyCodes.Add( new CurrencyCode( "CUP", "Cuban Peso", "&#8369;" ) );
				//    currencyCodes.Add( new CurrencyCode( "CVE", "Cape Verde Escudo" ) );
				//    currencyCodes.Add( new CurrencyCode( "CYP", "Cyprus Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "CZK", "Czech Koruna" ) );
				//    currencyCodes.Add( new CurrencyCode( "DJF", "Djibouti Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "DKK", "Danish Krone" ) );
				//    currencyCodes.Add( new CurrencyCode( "DOP", "Dominican Peso" ) );
				//    currencyCodes.Add( new CurrencyCode( "DZD", "Algerian Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "EEK", "Estonian Kroon" ) );
				//    currencyCodes.Add( new CurrencyCode( "EGP", "Egyptian Pound" ) );
				//    currencyCodes.Add( new CurrencyCode( "ERN", "Eritrean Nakfa" ) );
				//    currencyCodes.Add( new CurrencyCode( "ETB", "Ethiopian Birr" ) );
				//    currencyCodes.Add( new CurrencyCode( "EUR", "Euro", "&#8364;" ) );
				//    currencyCodes.Add( new CurrencyCode( "FJD", "Fiji Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "FKP", "Falkland Islands Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "GBP", "Pound Sterling", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "GEL", "Georgian Lari" ) );
				//    currencyCodes.Add( new CurrencyCode( "GHS", "Ghana Cedi" ) );
				//    currencyCodes.Add( new CurrencyCode( "GIP", "Gibraltar Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "GMD", "Gambian Dalasi" ) );
				//    currencyCodes.Add( new CurrencyCode( "GNF", "Guinea Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "GTQ", "Guatamalan Quetzal" ) );
				//    currencyCodes.Add( new CurrencyCode( "GWP", "Guinea-Bissau Peso" ) );
				//    currencyCodes.Add( new CurrencyCode( "GYD", "Guyana Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "HKD", "Hong Kong Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "HNL", "Honduran Lempira" ) );
				//    currencyCodes.Add( new CurrencyCode( "HRK", "Croatian Kuna" ) );
				//    currencyCodes.Add( new CurrencyCode( "HTG", "Haitian Gourde" ) );
				//    currencyCodes.Add( new CurrencyCode( "HUF", "Hungarian Forint" ) );
				//    currencyCodes.Add( new CurrencyCode( "IDR", "Indonesian Rupiah" ) );
				//    currencyCodes.Add( new CurrencyCode( "ILS", "New Israeli Sheqel" ) );
				//    currencyCodes.Add( new CurrencyCode( "INR", "Indian Rupee", "&#8360;" ) );
				//    currencyCodes.Add( new CurrencyCode( "IQD", "Iraqi Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "IRR", "Iranian Rial", "&#65020;" ) );
				//    currencyCodes.Add( new CurrencyCode( "ISK", "Iceland Krona" ) );
				//    currencyCodes.Add( new CurrencyCode( "JMD", "Jamaican Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "JOD", "Jordanian Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "JPY", "Japanese Yen", "&#165;" ) );
				//    currencyCodes.Add( new CurrencyCode( "KES", "Kenyan Shilling" ) );
				//    currencyCodes.Add( new CurrencyCode( "KGS", "Kyrgyzstan Som" ) );
				//    currencyCodes.Add( new CurrencyCode( "KHR", "Cambodian Riel", "&#6107;" ) );
				//    currencyCodes.Add( new CurrencyCode( "KMF", "Comoro Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "KPW", "North Korean Won", "&#8361;" ) );
				//    currencyCodes.Add( new CurrencyCode( "KRW", "South Korean Won", "&#8361;" ) );
				//    currencyCodes.Add( new CurrencyCode( "KWD", "Kuwaiti Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "KYD", "Cayman Islands Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "KZT", "Kazakhstan Tenge" ) );
				//    currencyCodes.Add( new CurrencyCode( "LAK", "Lao Kip", "&#8365;" ) );
				//    currencyCodes.Add( new CurrencyCode( "LBP", "Lebanese Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "LKR", "Sri Lanka Rupee", "&#3065;" ) );
				//    currencyCodes.Add( new CurrencyCode( "LRD", "Liberian Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "LSL", "Lesotho Loti" ) );
				//    currencyCodes.Add( new CurrencyCode( "LTL", "Lithuanian Litus" ) );
				//    currencyCodes.Add( new CurrencyCode( "LVL", "Latvian Lats" ) );
				//    currencyCodes.Add( new CurrencyCode( "LYD", "Libyan Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "MAD", "Moroccan Dirham" ) );
				//    currencyCodes.Add( new CurrencyCode( "MDL", "Moldovan Leu" ) );
				//    currencyCodes.Add( new CurrencyCode( "MGA", "Malagascy Ariary" ) );
				//    currencyCodes.Add( new CurrencyCode( "MKD", "Macedonia" ) );
				//    currencyCodes.Add( new CurrencyCode( "MMK", "Myanmar Kyat" ) );
				//    currencyCodes.Add( new CurrencyCode( "MNT", "Mongolian Tugrik", "&#8366;" ) );
				//    currencyCodes.Add( new CurrencyCode( "MOP", "Macau Pataca" ) );
				//    currencyCodes.Add( new CurrencyCode( "MRO", "Mauritania Ouguiya" ) );
				//    currencyCodes.Add( new CurrencyCode( "MTL", "Maltese Lira" ) );
				//    currencyCodes.Add( new CurrencyCode( "MUR", "Mauritius Rupee" ) );
				//    currencyCodes.Add( new CurrencyCode( "MVR", "Maldives Rufiyaa" ) );
				//    currencyCodes.Add( new CurrencyCode( "MWK", "Malawi Kwacha" ) );
				//    currencyCodes.Add( new CurrencyCode( "MXN", "Mexican Peso", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "MXV", "Mexican Unidad de Inversion" ) );
				//    currencyCodes.Add( new CurrencyCode( "MYR", "Malaysian Ringgit" ) );
				//    currencyCodes.Add( new CurrencyCode( "MZN", "Moazambique Metical" ) );
				//    currencyCodes.Add( new CurrencyCode( "NAD", "Namibian Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "NGN", "Nigerian Naira", "&#8358;" ) );
				//    currencyCodes.Add( new CurrencyCode( "NIO", "Nicaraguan Cordoba Oro" ) );
				//    currencyCodes.Add( new CurrencyCode( "NOK", "Norwegian Krone" ) );
				//    currencyCodes.Add( new CurrencyCode( "NPR", "Nepalese Rupee" ) );
				//    currencyCodes.Add( new CurrencyCode( "NZD", "New Zealand Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "OMR", "Rial Omani", "&#65020;" ) );
				//    currencyCodes.Add( new CurrencyCode( "PAB", "Panama Balboa" ) );
				//    currencyCodes.Add( new CurrencyCode( "PEN", "Peruvian Nuevo Sol", "&#83;&#47;&#46;" ) );
				//    currencyCodes.Add( new CurrencyCode( "PGK", "Papua New Guinea Kina" ) );
				//    currencyCodes.Add( new CurrencyCode( "PHP", "Philippine Peso", "&#8360;" ) );
				//    currencyCodes.Add( new CurrencyCode( "PKR", "Pakistan Rupee", "&#8360;" ) );
				//    currencyCodes.Add( new CurrencyCode( "PLN", "Polish Zloty" ) );
				//    currencyCodes.Add( new CurrencyCode( "PYG", "Paraguay Guarani" ) );
				//    currencyCodes.Add( new CurrencyCode( "QAR", "Qatari Rial", "&#65020;" ) );
				//    currencyCodes.Add( new CurrencyCode( "ROL", "Romanian Old Leu" ) );
				//    currencyCodes.Add( new CurrencyCode( "RON", "Romanian New Leu" ) );
				//    currencyCodes.Add( new CurrencyCode( "RSD", "Serbian Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "RUB", "Russian Ruble" ) );
				//    currencyCodes.Add( new CurrencyCode( "RWF", "Rwanda Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "SAR", "Saudi Riyal", "&#65020;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SBD", "Solomon Islands Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SCR", "Seychelles Rupee", "&#8360;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SDG", "Sudanese Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "SEK", "Swedish Krona", "&#107;&#114;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SGD", "Singapore Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SHP", "Saint Helena Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SKK", "Slovak Koruna" ) );
				//    currencyCodes.Add( new CurrencyCode( "SLL", "Sierra Leonean Leone" ) );
				//    currencyCodes.Add( new CurrencyCode( "SOS", "Somali Shilling" ) );
				//    currencyCodes.Add( new CurrencyCode( "SRD", "Suriname Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "STD", "So Tom and Principe Dobra" ) );
				//    currencyCodes.Add( new CurrencyCode( "SVC", "El Salvador Colon", "&#8353;" ) );
				//    currencyCodes.Add( new CurrencyCode( "SZL", "Swaziland Lilangeni" ) );
				//    currencyCodes.Add( new CurrencyCode( "SYP", "Syrian Pound", "&#163;" ) );
				//    currencyCodes.Add( new CurrencyCode( "THB", "Thai Baht", "&#3647;" ) );
				//    currencyCodes.Add( new CurrencyCode( "TOP", "Tongan Pa'anga" ) );
				//    currencyCodes.Add( new CurrencyCode( "TJS", "Tajikistan Somoni" ) );
				//    currencyCodes.Add( new CurrencyCode( "TMM", "Turkmenistan Manat" ) );
				//    currencyCodes.Add( new CurrencyCode( "TND", "Tunisian Dinar" ) );
				//    currencyCodes.Add( new CurrencyCode( "TRY", "New Turkish Lira", "&#8356;" ) );
				//    currencyCodes.Add( new CurrencyCode( "TTD", "Trinidad and Tobago Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "TWD", "New Taiwan Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "TZS", "Tanzanian Shilling" ) );
				//    currencyCodes.Add( new CurrencyCode( "UAH", "Ukrainian Hryvnia" ) );
				//    currencyCodes.Add( new CurrencyCode( "UGX", "Uganda Shilling" ) );
				//    currencyCodes.Add( new CurrencyCode( "USD", "United States Dollar", "&#36;" ) );
				//    currencyCodes.Add( new CurrencyCode( "USN", "United States Dollar (Next Day)" ) );
				//    currencyCodes.Add( new CurrencyCode( "USS", "United States Dollar (Same Day)" ) );
				//    currencyCodes.Add( new CurrencyCode( "UYI", "Uruguay Peso en Unidades Indexadeas" ) );
				//    currencyCodes.Add( new CurrencyCode( "UYU", "Peso Uruguayo", "&#8369;" ) );
				//    currencyCodes.Add( new CurrencyCode( "UZS", "Uzbekistan Sum" ) );
				//    currencyCodes.Add( new CurrencyCode( "VEF", "Venezuelan Bolivar Fuerte" ) );
				//    currencyCodes.Add( new CurrencyCode( "VND", "Viet Nam Dong", "&#8363;" ) );
				//    currencyCodes.Add( new CurrencyCode( "VUV", "Vanuatu Vatu" ) );
				//    currencyCodes.Add( new CurrencyCode( "WST", "Samoa Tala" ) );
				//    currencyCodes.Add( new CurrencyCode( "XBA", "European Composite Unit (EURCO)" ) );
				//    currencyCodes.Add( new CurrencyCode( "XBB", "European Monetary Unit (E.M.U.-6)" ) );
				//    currencyCodes.Add( new CurrencyCode( "XBC", "European Unit of Account 9 (E.U.A.-9)" ) );
				//    currencyCodes.Add( new CurrencyCode( "XBD", "European Unit of Account 17 (E.U.A.-17)" ) );
				//    currencyCodes.Add( new CurrencyCode( "XCD", "East Caribbean Dollar" ) );
				//    currencyCodes.Add( new CurrencyCode( "XAF", "CFA Franc BEAC" ) );
				//    currencyCodes.Add( new CurrencyCode( "XAG", "Silver Ounce" ) );
				//    currencyCodes.Add( new CurrencyCode( "XAU", "Gold Ounce" ) );
				//    currencyCodes.Add( new CurrencyCode( "XDR", "Special Drawing Rights (SDR)" ) );
				//    currencyCodes.Add( new CurrencyCode( "XFO", "Gold-Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "XFU", "UIC-Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "XOF", "CFA Franc BCEAO" ) );
				//    currencyCodes.Add( new CurrencyCode( "XPD", "Palladium Ounce" ) );
				//    currencyCodes.Add( new CurrencyCode( "XPF", "CFP Franc" ) );
				//    currencyCodes.Add( new CurrencyCode( "XPT", "Platinum Ounce" ) );
				//    currencyCodes.Add( new CurrencyCode( "YER", "Yemeni Rial", "&#65020;" ) );
				//    currencyCodes.Add( new CurrencyCode( "ZAR", "South African Rand", "&82;" ) );
				//    currencyCodes.Add( new CurrencyCode( "ZMK", "Zambian Kwacha" ) );
				//    currencyCodes.Add( new CurrencyCode( "ZWD", "Zimbabwe Dollar", "&#36;" ) );

				//    currencyCodes.Sort();
				//}
			}

			return currencyCodes;
		}

		public static ISOUtility.CurrencyCode[] ParseStream( Stream s )
		{
			XmlSerializer xSer = new XmlSerializer( typeof( Aucent.MAX.AXE.Common.Utilities.ISOUtility.CurrencyCode[] ) );
			ISOUtility.CurrencyCode[] tmpCurrencyCodes = (ISOUtility.CurrencyCode[])xSer.Deserialize( s );
			return tmpCurrencyCodes;
		}

		#endregion


		#region CurrencyCode class
        [global::System.Reflection.Obfuscation(Exclude = true)]
		public class CurrencyCode : IComparable
		{
			public string Code = string.Empty;
			public string Description = string.Empty;
			public string Symbol = string.Empty;

			/// <summary>
			/// Creates a new instance of <see cref="CurrencyCode"/>.
			/// </summary>
			public CurrencyCode()
			{
			}

			/// <summary>
			/// Creates a new instance of <see cref="CurrencyCode"/>.
			/// </summary>
			/// <param name="code">The ISO standard code to be assigned to 
			/// this <see cref="CurrencyCode"/>.</param>
			/// <param name="description">Description to be assigned to 
			/// this <see cref="CurrencyCode"/>.</param>
			public CurrencyCode( string code, string description )
			{
				this.Code = code;
				this.Description = description;
			}

			/// <summary>
			/// Creates a new instance of <see cref="CurrencyCode"/>.
			/// </summary>
			/// <param name="code">The ISO standard code to be assigned to 
			/// this <see cref="CurrencyCode"/>.</param>
			/// <param name="description">Description to be assigned to 
			/// this <see cref="CurrencyCode"/>.</param>
			/// <param name="symbol">The display symbol for this <see cref="CurrencyCode"/>.</param>
			public CurrencyCode( string code, string description, string symbol )
				: this( code, description )
			{
				this.Symbol = symbol;
			}

			#region IComparable Members

			/// <summary>
			/// Compares this instance of <see cref="CurrencyCode"/> to a supplied <see cref="Object"/>.
			/// </summary>
			/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="CurrencyCode"/>
			/// is to be compared.  Assumed to be a <see cref="CurrencyCode"/> or <see cref="String"/>.</param>
			/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
			/// greater than (>0), or equal to (0) this instance of <see cref="CurrencyCode"/>.</returns>
			/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> 
			/// for the codes of the two <see cref="CurrencyCode"/> objects.  If <paramref name="obj"/> is 
			/// a <see cref="String"/>, it is compared to the <see cref="Code"/> of this <see cref="CurrencyCode"/>.</remarks>
			public int CompareTo( object obj )
			{
				if( obj is string ) return Code.CompareTo( (string)obj );

				return this.Code.CompareTo( ( (CurrencyCode)obj ).Code );
			}

			#endregion

			/// <summary>
			/// Returns a <see cref="String"/> that represents the current <see cref="CurrencyCode"/> object.
			/// </summary>
			/// <returns>A <see cref="String"/> that represents the current <see cref="CurrencyCode"/> object.</returns>
			public override string ToString()
			{
				return String.Format( "{0}    {1}", Code, Description );
			}

		}

		#endregion

		#region SIC Codes
		/// <summary>
		/// SICReference
		/// </summary>
        [global::System.Reflection.Obfuscation(Exclude = true)]
		public class SICReference : IComparable
		{
			/// <summary>
			/// A <see cref="String"/> containing the code by which the SIC
			/// is identified.
			/// </summary>
			public string SICCode = string.Empty;

			/// <summary>
			/// A <see cref="String"/> containing the name of the SIC
			/// </summary>
			public string SICName = string.Empty;

			/// <summary>
			/// Constructs a new instance of the <see cref="SICReference"/> class.
			/// </summary>
			public SICReference()
			{
			}
			/// <summary>
			/// Oveeride the base ToString, return SIC Code - SIC Name
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return string.Format( @"{0} - {1}", SICCode, SICName );
			}
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="code"></param>
			/// <param name="name"></param>
			public SICReference( string code, string name )
			{
				this.SICCode = code;
				this.SICName = name;
			}

			#region IComparable Members

			/// <summary>
			/// Compare based on code
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public int CompareTo( object obj )
			{
				SICReference source = obj as SICReference;
				return string.Compare( this.SICCode, source.SICCode );
			}

			#endregion
		}


		private static int SortSICReferenceByCode( SICReference a, SICReference b )
		{
			return a.SICCode.CompareTo( b.SICCode );
		}
		/// <summary>
		/// Get a list of know SIC Code/Names
		/// </summary>
		/// <returns></returns>
		public static List<SICReference> GetSICList()
		{
			if( sortedSicReferences == null )
			{
				lock( typeof( ISOUtility ) )
				{
					sortedSicReferences = new List<SICReference>();

					//CEE: 2009-06-04 - New SIC list from the following URL:
					//http://www.sec.gov/info/edgar/siccodes.htm

					sortedSicReferences.Add( new SICReference( "0100", "AGRICULTURAL PRODUCTION-CROPS" ) );

					sortedSicReferences.Add( new SICReference( "0110", "CASH GRAINS" ) );
					sortedSicReferences.Add( new SICReference( "0111", "WHEAT" ) );
					sortedSicReferences.Add( new SICReference( "0112", "RICE" ) );
					sortedSicReferences.Add( new SICReference( "0115", "CORN" ) );
					sortedSicReferences.Add( new SICReference( "0116", "SOYBEANS" ) );
					sortedSicReferences.Add( new SICReference( "0119", "CASH GRAINS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0130", "FIELD CROPS, EXCEPT CASH GRAINS" ) );
					sortedSicReferences.Add( new SICReference( "0131", "COTTON" ) );
					sortedSicReferences.Add( new SICReference( "0132", "TOBACCO" ) );
					sortedSicReferences.Add( new SICReference( "0133", "SUGARCANE AND SUGAR BEETS" ) );
					sortedSicReferences.Add( new SICReference( "0134", "IRISH POTATOES" ) );
					sortedSicReferences.Add( new SICReference( "0139", "FIELD CROPS, EXCEPT CASH GRAINS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0160", "VEGETABLES AND MELONS" ) );
					sortedSicReferences.Add( new SICReference( "0161", "VEGETABLES AND MELONS" ) );

					sortedSicReferences.Add( new SICReference( "0170", "FRUITS AND TREE NUTS" ) );
					sortedSicReferences.Add( new SICReference( "0171", "BERRY CROPS" ) );
					sortedSicReferences.Add( new SICReference( "0172", "GRAPES" ) );
					sortedSicReferences.Add( new SICReference( "0173", "TREE NUTS" ) );
					sortedSicReferences.Add( new SICReference( "0174", "CITRUS FRUITS" ) );
					sortedSicReferences.Add( new SICReference( "0175", "DECIDUOUS TREE FRUITS" ) );
					sortedSicReferences.Add( new SICReference( "0179", "FRUITS AND TREE NUTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0180", "HORTICULTURAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0181", "ORNAMENTAL FLORICULTURE AND NURSERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "0182", "FOOD CROPS GROWN UNDER COVER" ) );

					sortedSicReferences.Add( new SICReference( "0190", "GENERAL FARMS, PRIMARILY CROP" ) );
					sortedSicReferences.Add( new SICReference( "0191", "GENERAL FARMS, PRIMARILY CROP" ) );

					sortedSicReferences.Add( new SICReference( "0200", "AGRICULTURAL PROD-LIVESTOCK AND ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0210", "LIVESTOCK, EXCEPT DAIRY AND POULTRY" ) );
					sortedSicReferences.Add( new SICReference( "0211", "BEEF CATTLE FEEDLOTS" ) );
					sortedSicReferences.Add( new SICReference( "0212", "BEEF CATTLE, EXCEPT FEEDLOTS" ) );
					sortedSicReferences.Add( new SICReference( "0213", "HOGS" ) );
					sortedSicReferences.Add( new SICReference( "0214", "SHEEP AND GOATS" ) );
					sortedSicReferences.Add( new SICReference( "0219", "GENERAL LIVESTOCK, EXCEPT DAIRY AND POULTRY" ) );

					sortedSicReferences.Add( new SICReference( "0240", "DAIRY FARMS" ) );
					sortedSicReferences.Add( new SICReference( "0241", "DAIRY FARMS" ) );

					sortedSicReferences.Add( new SICReference( "0250", "POULTRY AND EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0251", "BROILER, FRYER, AND ROASTER CHICKENS" ) );
					sortedSicReferences.Add( new SICReference( "0252", "CHICKEN EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0253", "TURKEYS AND TURKEY EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0254", "POULTRY HATCHERIES" ) );
					sortedSicReferences.Add( new SICReference( "0259", "POULTRY AND EGGS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0270", "ANIMAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0271", "FUR-BEARING ANIMALS AND RABBITS" ) );
					sortedSicReferences.Add( new SICReference( "0272", "HORSES AND OTHER EQUINES" ) );
					sortedSicReferences.Add( new SICReference( "0273", "ANIMAL AQUACULTURE" ) );
					sortedSicReferences.Add( new SICReference( "0279", "ANIMAL SPECIALTIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0290", "GENERAL FARMS, PRIMARILY LIVESTOCK AND ANIMAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0291", "GENERAL FARMS, PRIMARILY LIVESTOCK AND ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0700", "AGRICULTURAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0710", "SOIL PREPARATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0711", "SOIL PREPARATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0720", "CROP SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0721", "CROP PLANTING, CULTIVATING, AND PROTECTING" ) );
					sortedSicReferences.Add( new SICReference( "0722", "CROP HARVESTING, PRIMARILY BY MACHINE" ) );
					sortedSicReferences.Add( new SICReference( "0723", "CROP PREPARATION SERVICES FOR MARKET, EXCEPT COTTON GINNING" ) );
					sortedSicReferences.Add( new SICReference( "0724", "COTTON GINNING" ) );

					sortedSicReferences.Add( new SICReference( "0740", "VETERINARY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0741", "VETERINARY SERVICES FOR LIVESTOCK" ) );
					sortedSicReferences.Add( new SICReference( "0742", "VETERINARY SERVICES FOR ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0750", "ANIMAL SERVICES, EXCEPT VETERINARY" ) );
					sortedSicReferences.Add( new SICReference( "0751", "LIVESTOCK SERVICES, EXCEPT VETERINARY" ) );
					sortedSicReferences.Add( new SICReference( "0752", "ANIMAL SPECIALTY SERVICES, EXCEPT VETERINARY" ) );

					sortedSicReferences.Add( new SICReference( "0760", "FARM LABOR AND MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0761", "FARM LABOR CONTRACTORS AND CREW LEADERS" ) );
					sortedSicReferences.Add( new SICReference( "0762", "FARM MANAGEMENT SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0780", "LANDSCAPE AND HORTICULTURAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0781", "LANDSCAPE COUNSELING AND PLANNING" ) );
					sortedSicReferences.Add( new SICReference( "0782", "LAWN AND GARDEN SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0783", "ORNAMENTAL SHRUB AND TREE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0800", "FORESTRY" ) );

					sortedSicReferences.Add( new SICReference( "0810", "TIMBER TRACTS" ) );
					sortedSicReferences.Add( new SICReference( "0811", "TIMBER TRACTS" ) );

					sortedSicReferences.Add( new SICReference( "0830", "FOREST NURSERIES AND GATHERING OF FOREST PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "0831", "FOREST NURSERIES AND GATHERING OF FOREST PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "0850", "FORESTRY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0851", "FORESTRY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0900", "FISHING, HUNTING AND TRAPPING" ) );

					sortedSicReferences.Add( new SICReference( "0910", "COMMERCIAL FISHING" ) );
					sortedSicReferences.Add( new SICReference( "0912", "FINFISH" ) );
					sortedSicReferences.Add( new SICReference( "0913", "SHELLFISH" ) );
					sortedSicReferences.Add( new SICReference( "0919", "MISCELLANEOUS MARINE PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "0920", "FISH HATCHERIES AND PRESERVES" ) );
					sortedSicReferences.Add( new SICReference( "0921", "FISH HATCHERIES AND PRESERVES" ) );

					sortedSicReferences.Add( new SICReference( "0970", "HUNTING AND TRAPPING, AND GAME PROPAGATION" ) );
					sortedSicReferences.Add( new SICReference( "0971", "HUNTING AND TRAPPING, AND GAME PROPAGATION" ) );

					sortedSicReferences.Add( new SICReference( "1000", "METAL MINING" ) );

					sortedSicReferences.Add( new SICReference( "1010", "IRON ORES" ) );
					sortedSicReferences.Add( new SICReference( "1011", "IRON ORES" ) );

					sortedSicReferences.Add( new SICReference( "1020", "COPPER ORES" ) );
					sortedSicReferences.Add( new SICReference( "1021", "COPPER ORES" ) );

					sortedSicReferences.Add( new SICReference( "1030", "LEAD AND ZINC ORES" ) );
					sortedSicReferences.Add( new SICReference( "1031", "LEAD AND ZINC ORES" ) );

					sortedSicReferences.Add( new SICReference( "1040", "GOLD AND SILVER ORES" ) );
					sortedSicReferences.Add( new SICReference( "1041", "GOLD ORES" ) );
					sortedSicReferences.Add( new SICReference( "1044", "SILVER ORES" ) );

					sortedSicReferences.Add( new SICReference( "1060", "FERROALLOY ORES, EXCEPT VANADIUM" ) );
					sortedSicReferences.Add( new SICReference( "1061", "FERROALLOY ORES, EXCEPT VANADIUM" ) );

					sortedSicReferences.Add( new SICReference( "1080", "METAL MINING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1081", "METAL MINING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "1090", "MISCELLANEOUS METAL ORES" ) );
					sortedSicReferences.Add( new SICReference( "1094", "URANIUM-RADIUM-VANADIUM ORES" ) );
					sortedSicReferences.Add( new SICReference( "1099", "MISCELLANEOUS METAL ORES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1220", "BITUMINOUS COAL AND LIGNITE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1221", "BITUMINOUS COAL AND LIGNITE SURFACE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1222", "BITUMINOUS COAL UNDERGROUND MINING" ) );

					sortedSicReferences.Add( new SICReference( "1230", "ANTHRACITE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1231", "ANTHRACITE MINING" ) );

					sortedSicReferences.Add( new SICReference( "1240", "COAL MINING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1241", "COAL MINING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "1310", "CRUDE PETROLEUM AND NATURAL GAS" ) );
					sortedSicReferences.Add( new SICReference( "1311", "CRUDE PETROLEUM AND NATURAL GAS" ) );

					sortedSicReferences.Add( new SICReference( "1320", "NATURAL GAS LIQUIDS" ) );
					sortedSicReferences.Add( new SICReference( "1321", "NATURAL GAS LIQUIDS" ) );

					sortedSicReferences.Add( new SICReference( "1380", "OIL AND GAS FIELD SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1381", "DRILLING OIL AND GAS WELLS" ) );
					sortedSicReferences.Add( new SICReference( "1382", "OIL AND GAS FIELD EXPLORATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1389", "OIL AND GAS FIELD SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1400", "MINING AND QUARRYING OF NONMETALLIC MINERALS (NO FUELS)" ) );

					sortedSicReferences.Add( new SICReference( "1410", "DIMENSION STONE" ) );
					sortedSicReferences.Add( new SICReference( "1411", "DIMENSION STONE" ) );

					sortedSicReferences.Add( new SICReference( "1420", "CRUSHED AND BROKEN STONE, INCLUDING RIPRAP" ) );
					sortedSicReferences.Add( new SICReference( "1422", "CRUSHED AND BROKEN LIMESTONE" ) );
					sortedSicReferences.Add( new SICReference( "1423", "CRUSHED AND BROKEN GRANITE" ) );
					sortedSicReferences.Add( new SICReference( "1429", "CRUSHED AND BROKEN STONE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1440", "SAND AND GRAVEL" ) );
					sortedSicReferences.Add( new SICReference( "1442", "CONSTRUCTION SAND AND GRAVEL" ) );
					sortedSicReferences.Add( new SICReference( "1446", "INDUSTRIAL SAND" ) );

					sortedSicReferences.Add( new SICReference( "1450", "CLAY, CERAMIC, AND REFRACTORY MINERALS" ) );
					sortedSicReferences.Add( new SICReference( "1455", "KAOLIN AND BALL CLAY" ) );
					sortedSicReferences.Add( new SICReference( "1459", "CLAY, CERAMIC, AND REFRACTORY MINERALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1470", "CHEMICAL AND FERTILIZER MINERAL MINING" ) );
					sortedSicReferences.Add( new SICReference( "1474", "POTASH, SODA, AND BORATE MINERALS" ) );
					sortedSicReferences.Add( new SICReference( "1475", "PHOSPHATE ROCK" ) );
					sortedSicReferences.Add( new SICReference( "1479", "CHEMICAL AND FERTILIZER MINERAL MINING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1480", "NONMETALLIC MINERALS SERVICES, EXCEPT FUELS" ) );
					sortedSicReferences.Add( new SICReference( "1481", "NONMETALLIC MINERALS SERVICES, EXCEPT FUELS" ) );

					sortedSicReferences.Add( new SICReference( "1490", "MISCELLANEOUS NONMETALLIC MINERALS, EXCEPT FUELS" ) );
					sortedSicReferences.Add( new SICReference( "1499", "MISCELLANEOUS NONMETALLIC MINERALS, EXCEPT FUELS" ) );

					sortedSicReferences.Add( new SICReference( "1520", "GENERAL BUILDING CONTRACTORS-RESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "1521", "GENERAL CONTRACTORS-SINGLE-FAMILY HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "1522", "GENERAL CONTRACTORS-RESIDENTIAL BUILDINGS, OTHER THAN SINGLE-FAMI" ) );

					sortedSicReferences.Add( new SICReference( "1530", "OPERATIVE BUILDERS" ) );
					sortedSicReferences.Add( new SICReference( "1531", "OPERATIVE BUILDERS" ) );

					sortedSicReferences.Add( new SICReference( "1540", "GENERAL BUILDING CONTRACTORS-NONRESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "1541", "GENERAL CONTRACTORS-INDUSTRIAL BUILDINGS AND WAREHOUSES" ) );
					sortedSicReferences.Add( new SICReference( "1542", "GENERAL CONTRACTORS-NONRESIDENTIAL BUILDINGS, OTHER THAN INDUSTRI" ) );

					sortedSicReferences.Add( new SICReference( "1600", "HEAVY CONSTRUCTION OTHER THAN BLDG CONST - CONTRACTORS" ) );

					sortedSicReferences.Add( new SICReference( "1610", "HIGHWAY AND STREET CONSTRUCTION, EXCEPT ELEVATED HIGHWAYS" ) );
					sortedSicReferences.Add( new SICReference( "1611", "HIGHWAY AND STREET CONSTRUCTION, EXCEPT ELEVATED HIGHWAYS" ) );

					sortedSicReferences.Add( new SICReference( "1620", "HEAVY CONSTRUCTION, EXCEPT HIGHWAY AND STREET CONSTRUCTION" ) );
					sortedSicReferences.Add( new SICReference( "1622", "BRIDGE, TUNNEL, AND ELEVATED HIGHWAY CONSTRUCTION" ) );
					sortedSicReferences.Add( new SICReference( "1623", "WATER, SEWER, PIPELINE, AND COMMUNICATIONS AND POWER LINE CONSTRU" ) );
					sortedSicReferences.Add( new SICReference( "1629", "HEAVY CONSTRUCTION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1700", "CONSTRUCTION - SPECIAL TRADE CONTRACTORS" ) );

					sortedSicReferences.Add( new SICReference( "1710", "PLUMBING, HEATING AND AIR-CONDITIONING" ) );
					sortedSicReferences.Add( new SICReference( "1711", "PLUMBING, HEATING AND AIR-CONDITIONING" ) );

					sortedSicReferences.Add( new SICReference( "1720", "PAINTING AND PAPER HANGING" ) );
					sortedSicReferences.Add( new SICReference( "1721", "PAINTING AND PAPER HANGING" ) );

					sortedSicReferences.Add( new SICReference( "1730", "ELECTRICAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "1731", "ELECTRICAL WORK" ) );

					sortedSicReferences.Add( new SICReference( "1740", "MASONRY, STONEWORK, TILE SETTING, AND PLASTERING" ) );
					sortedSicReferences.Add( new SICReference( "1741", "MASONRY, STONE SETTING, AND OTHER STONE WORK" ) );
					sortedSicReferences.Add( new SICReference( "1742", "PLASTERING, DRYWALL, ACOUSTICAL, AND INSULATION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1743", "TERRAZZO, TILE, MARBLE, AND MOSAIC WORK" ) );

					sortedSicReferences.Add( new SICReference( "1750", "CARPENTRY AND FLOOR WORK" ) );
					sortedSicReferences.Add( new SICReference( "1751", "CARPENTRY WORK" ) );
					sortedSicReferences.Add( new SICReference( "1752", "FLOOR LAYING AND OTHER FLOOR WORK, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1760", "ROOFING, SIDING, AND SHEET METAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "1761", "ROOFING, SIDING, AND SHEET METAL WORK" ) );

					sortedSicReferences.Add( new SICReference( "1770", "CONCRETE WORK" ) );
					sortedSicReferences.Add( new SICReference( "1771", "CONCRETE WORK" ) );

					sortedSicReferences.Add( new SICReference( "1780", "WATER WELL DRILLING" ) );
					sortedSicReferences.Add( new SICReference( "1781", "WATER WELL DRILLING" ) );

					sortedSicReferences.Add( new SICReference( "1790", "MISCELLANEOUS SPECIAL TRADE CONTRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "1791", "STRUCTURAL STEEL ERECTION" ) );
					sortedSicReferences.Add( new SICReference( "1793", "GLASS INSTALLATION, EXCEPT AUTOMOTIVE-CONTRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "1794", "EXCAVATION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1795", "WRECKING AND DEMOLITION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1796", "INSTALLATION OR ERECTION OF BUILDING EQUIPMENT, NOT ELSEWHERE CLA" ) );
					sortedSicReferences.Add( new SICReference( "1799", "SPECIAL TRADE CONTRACTORS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2000", "FOOD AND KINDRED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2010", "MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2011", "MEAT PACKING PLANTS" ) );
					sortedSicReferences.Add( new SICReference( "2013", "SAUSAGES AND OTHER PREPARED MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2015", "POULTRY SLAUGHTERING AND PROCESSING" ) );

					sortedSicReferences.Add( new SICReference( "2020", "DAIRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2021", "CREAMERY BUTTER" ) );
					sortedSicReferences.Add( new SICReference( "2022", "NATURAL, PROCESSED, AND IMITATION CHEESE" ) );
					sortedSicReferences.Add( new SICReference( "2023", "DRY, CONDENSED, AND EVAPORATED DAIRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2024", "ICE CREAM AND FROZEN DESSERTS" ) );
					sortedSicReferences.Add( new SICReference( "2026", "FLUID MILK" ) );

					sortedSicReferences.Add( new SICReference( "2030", "CANNED, FROZEN, AND PRESERVED FRUITS, VEGETABLES, AND FOOD SPECIAL" ) );
					sortedSicReferences.Add( new SICReference( "2032", "CANNED SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "2033", "CANNED FRUITS, VEGETABLES, PRESERVES, JAMS, AND JELLIES" ) );
					sortedSicReferences.Add( new SICReference( "2034", "DRIED AND DEHYDRATED FRUITS, VEGETABLES, AND SOUP MIXES" ) );
					sortedSicReferences.Add( new SICReference( "2035", "PICKLED FRUITS AND VEGETABLES, VEGETABLE SAUCES AND SEASONINGS, A" ) );
					sortedSicReferences.Add( new SICReference( "2037", "FROZEN FRUITS, FRUIT JUICES, AND VEGETABLES" ) );
					sortedSicReferences.Add( new SICReference( "2038", "FROZEN SPECIALTIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2040", "GRAIN MILL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2041", "FLOUR AND OTHER GRAIN MILL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2043", "CEREAL BREAKFAST FOODS" ) );
					sortedSicReferences.Add( new SICReference( "2044", "RICE MILLING" ) );
					sortedSicReferences.Add( new SICReference( "2045", "PREPARED FLOUR MIXES AND DOUGHS" ) );
					sortedSicReferences.Add( new SICReference( "2046", "WET CORN MILLING" ) );
					sortedSicReferences.Add( new SICReference( "2047", "DOG AND CAT FOOD" ) );
					sortedSicReferences.Add( new SICReference( "2048", "PREPARED FEEDS AND FEED INGREDIENTS FOR ANIMALS AND FOWLS, EXCEPT" ) );

					sortedSicReferences.Add( new SICReference( "2050", "BAKERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2051", "BREAD AND OTHER BAKERY PRODUCTS, EXCEPT COOKIES AND CRACKERS" ) );
					sortedSicReferences.Add( new SICReference( "2052", "COOKIES AND CRACKERS" ) );
					sortedSicReferences.Add( new SICReference( "2053", "FROZEN BAKERY PRODUCTS, EXCEPT BREAD" ) );

					sortedSicReferences.Add( new SICReference( "2060", "SUGAR AND CONFECTIONERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2061", "CANE SUGAR, EXCEPT REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2062", "CANE SUGAR REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2063", "BEET SUGAR" ) );
					sortedSicReferences.Add( new SICReference( "2064", "CANDY AND OTHER CONFECTIONERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2066", "CHOCOLATE AND COCOA PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2067", "CHEWING GUM" ) );
					sortedSicReferences.Add( new SICReference( "2068", "SALTED AND ROASTED NUTS AND SEEDS" ) );

					sortedSicReferences.Add( new SICReference( "2070", "FATS AND OILS" ) );
					sortedSicReferences.Add( new SICReference( "2074", "COTTONSEED OIL MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2075", "SOYBEAN OIL MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2076", "VEGETABLE OIL MILLS, EXCEPT CORN, COTTONSEED, AND SOYBEAN" ) );
					sortedSicReferences.Add( new SICReference( "2077", "ANIMAL AND MARINE FATS AND OILS" ) );
					sortedSicReferences.Add( new SICReference( "2079", "SHORTENING, TABLE OILS, MARGARINE, AND OTHER EDIBLE FATS AND OILS" ) );

					sortedSicReferences.Add( new SICReference( "2080", "BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "2082", "MALT BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "2083", "MALT" ) );
					sortedSicReferences.Add( new SICReference( "2084", "WINES, BRANDY, AND BRANDY SPIRITS" ) );
					sortedSicReferences.Add( new SICReference( "2085", "DISTILLED AND BLENDED LIQUORS" ) );
					sortedSicReferences.Add( new SICReference( "2086", "BOTTLED AND CANNED SOFT DRINKS AND CARBONATED WATERS" ) );
					sortedSicReferences.Add( new SICReference( "2087", "FLAVORING EXTRACTS AND FLAVORING SYRUPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2090", "MISCELLANEOUS FOOD PREPARATIONS AND KINDRED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2091", "CANNED AND CURED FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "2092", "PREPARED FRESH OR FROZEN FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "2095", "ROASTED COFFEE" ) );
					sortedSicReferences.Add( new SICReference( "2096", "POTATO CHIPS, CORN CHIPS, AND SIMILAR SNACKS" ) );
					sortedSicReferences.Add( new SICReference( "2097", "MANUFACTURED ICE" ) );
					sortedSicReferences.Add( new SICReference( "2098", "MACARONI, SPAGHETTI, VERMICELLI, AND NOODLES" ) );
					sortedSicReferences.Add( new SICReference( "2099", "FOOD PREPARATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2100", "TOBACCO PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2110", "CIGARETTES" ) );
					sortedSicReferences.Add( new SICReference( "2111", "CIGARETTES" ) );

					sortedSicReferences.Add( new SICReference( "2120", "CIGARS" ) );
					sortedSicReferences.Add( new SICReference( "2121", "CIGARS" ) );

					sortedSicReferences.Add( new SICReference( "2130", "CHEWING AND SMOKING TOBACCO AND SNUFF" ) );
					sortedSicReferences.Add( new SICReference( "2131", "CHEWING AND SMOKING TOBACCO AND SNUFF" ) );

					sortedSicReferences.Add( new SICReference( "2140", "TOBACCO STEMMING AND REDRYING" ) );
					sortedSicReferences.Add( new SICReference( "2141", "TOBACCO STEMMING AND REDRYING" ) );

					sortedSicReferences.Add( new SICReference( "2200", "TEXTILE MILL PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2210", "BROADWOVEN FABRIC MILLS, COTTON" ) );
					sortedSicReferences.Add( new SICReference( "2211", "BROADWOVEN FABRIC MILLS, COTTON" ) );

					sortedSicReferences.Add( new SICReference( "2220", "BROADWOVEN FABRIC MILLS, MANMADE FIBER AND SILK" ) );
					sortedSicReferences.Add( new SICReference( "2221", "BROADWOVEN FABRIC MILLS, MAN MADE FIBER AND SILK" ) );

					sortedSicReferences.Add( new SICReference( "2230", "BROADWOVEN FABRIC MILLS, WOOL (INCLUDING DYEING AND FINISHING)" ) );
					sortedSicReferences.Add( new SICReference( "2231", "BROADWOVEN FABRIC MILLS, WOOL (INCLUDING DYEING AND FINISHING)" ) );

					sortedSicReferences.Add( new SICReference( "2240", "NARROW FABRIC AND OTHER SMALLWARES MILLS: COTTON, WOOL, SILK, AND" ) );
					sortedSicReferences.Add( new SICReference( "2241", "NARROW FABRIC AND OTHER SMALLWARES MILLS: COTTON, WOOL, SILK, AND" ) );

					sortedSicReferences.Add( new SICReference( "2250", "KNITTING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2251", "WOMEN'S FULL-LENGTH AND KNEE-LENGTH HOSIERY, EXCEPT SOCKS" ) );
					sortedSicReferences.Add( new SICReference( "2252", "HOSIERY, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "2253", "KNIT OUTERWEAR MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2254", "KNIT UNDERWEAR AND NIGHTWEAR MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2257", "WEFT KNIT FABRIC MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2258", "LACE AND WARP KNIT FABRIC MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2259", "KNITTING MILLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2260", "DYEING AND FINISHING TEXTILES, EXCEPT WOOL FABRICS AND KNIT GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2261", "FINISHERS OF BROADWOVEN FABRICS OF COTTON" ) );
					sortedSicReferences.Add( new SICReference( "2262", "FINISHERS OF BROADWOVEN FABRICS OF MANMADE FIBER AND SILK" ) );
					sortedSicReferences.Add( new SICReference( "2269", "FINISHERS OF TEXTILES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2270", "CARPETS AND RUGS" ) );
					sortedSicReferences.Add( new SICReference( "2273", "CARPETS AND RUGS" ) );

					sortedSicReferences.Add( new SICReference( "2280", "YARN AND THREAD MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2281", "YARN SPINNING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2282", "ACETATE FILAMENT YARN: THROWING, TWISTING, WINDING, OR SPOOLING" ) );
					sortedSicReferences.Add( new SICReference( "2284", "THREAD MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2290", "MISCELLANEOUS TEXTILE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2295", "COATED FABRICS, NOT RUBBERIZED" ) );
					sortedSicReferences.Add( new SICReference( "2296", "TIRE CORD AND FABRICS" ) );
					sortedSicReferences.Add( new SICReference( "2297", "NONWOVEN FABRICS" ) );
					sortedSicReferences.Add( new SICReference( "2298", "CORDAGE AND TWINE" ) );
					sortedSicReferences.Add( new SICReference( "2299", "TEXTILE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2300", "APPAREL AND OTHER FINISHD PRODS OF FABRICS AND SIMILAR MATL" ) );

					sortedSicReferences.Add( new SICReference( "2310", "MEN'S AND BOYS' SUITS, COATS, AND OVERCOATS" ) );
					sortedSicReferences.Add( new SICReference( "2311", "MEN'S AND BOYS' SUITS, COATS, AND OVERCOATS" ) );

					sortedSicReferences.Add( new SICReference( "2320", "MEN'S AND BOYS' FURNISHINGS, WORK CLOTHING, AND ALLIED GARMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2321", "MEN'S AND BOYS' SHIRTS, EXCEPT WORK SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2322", "MEN'S AND BOYS' UNDERWEAR AND NIGHTWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2323", "MEN'S AND BOYS' NECKWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2325", "MEN'S AND BOYS' SEPARATE TROUSERS AND SLACKS" ) );
					sortedSicReferences.Add( new SICReference( "2326", "MEN'S AND BOYS' WORK CLOTHING" ) );
					sortedSicReferences.Add( new SICReference( "2329", "MEN'S AND BOYS' CLOTHING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2330", "WOMEN'S, MISSES', AND JUNIORS' OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2331", "WOMEN'S, MISSES', AND JUNIORS' BLOUSES AND SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2335", "WOMEN'S, MISSES', AND JUNIORS' DRESSES" ) );
					sortedSicReferences.Add( new SICReference( "2337", "WOMEN'S, MISSES', AND JUNIORS' SUITS, SKIRTS, AND COATS" ) );
					sortedSicReferences.Add( new SICReference( "2339", "WOMEN'S, MISSES', AND JUNIORS' OUTERWEAR, NOT ELSEWHERE CLASSIFIE" ) );

					sortedSicReferences.Add( new SICReference( "2340", "WOMEN'S, MISSES', CHILDREN'S, AND INFANTS' UNDERGARMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2341", "WOMEN'S, MISSES', CHILDREN'S, AND INFANTS' UNDERWEAR AND NIGHTWEA" ) );
					sortedSicReferences.Add( new SICReference( "2342", "BRASSIERES, GIRDLES, AND ALLIED GARMENTS" ) );

					sortedSicReferences.Add( new SICReference( "2350", "HATS, CAPS, AND MILLINERY" ) );
					sortedSicReferences.Add( new SICReference( "2353", "HATS, CAPS, AND MILLINERY" ) );

					sortedSicReferences.Add( new SICReference( "2360", "GIRLS', CHILDREN'S, AND INFANTS' OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2361", "GIRLS', CHILDREN'S, AND INFANTS' DRESSES, BLOUSES, AND SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2369", "GIRLS', CHILDREN'S, AND INFANTS' OUTERWEAR, NOT ELSEWHERE CLASSIF" ) );

					sortedSicReferences.Add( new SICReference( "2370", "FUR GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2371", "FUR GOODS" ) );

					sortedSicReferences.Add( new SICReference( "2380", "MISCELLANEOUS APPAREL AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "2381", "DRESS AND WORK GLOVES, EXCEPT KNIT AND ALL-LEATHER" ) );
					sortedSicReferences.Add( new SICReference( "2384", "ROBES AND DRESSING GOWNS" ) );
					sortedSicReferences.Add( new SICReference( "2385", "WATERPROOF OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2386", "LEATHER AND SHEEP-LINED CLOTHING" ) );
					sortedSicReferences.Add( new SICReference( "2387", "APPAREL BELTS" ) );
					sortedSicReferences.Add( new SICReference( "2389", "APPAREL AND ACCESSORIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2390", "MISCELLANEOUS FABRICATED TEXTILE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2391", "CURTAINS AND DRAPERIES" ) );
					sortedSicReferences.Add( new SICReference( "2392", "HOUSEFURNISHINGS, EXCEPT CURTAINS AND DRAPERIES" ) );
					sortedSicReferences.Add( new SICReference( "2393", "TEXTILE BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2394", "CANVAS AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2395", "PLEATING, DECORATIVE AND NOVELTY STITCHING, AND TUCKING FOR THE T" ) );
					sortedSicReferences.Add( new SICReference( "2396", "AUTOMOTIVE TRIMMINGS, APPAREL FINDINGS, AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2397", "SCHIFFLI MACHINE EMBROIDERIES" ) );
					sortedSicReferences.Add( new SICReference( "2399", "FABRICATED TEXTILE PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2400", "LUMBER AND WOOD PRODUCTS (NO FURNITURE)" ) );

					sortedSicReferences.Add( new SICReference( "2410", "LOGGING" ) );
					sortedSicReferences.Add( new SICReference( "2411", "LOGGING" ) );

					sortedSicReferences.Add( new SICReference( "2420", "SAWMILLS AND PLANING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2421", "SAWMILLS AND PLANTING MILLS, GENERAL" ) );
					sortedSicReferences.Add( new SICReference( "2426", "HARDWOOD DIMENSION AND FLOORING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2429", "SPECIAL PRODUCT SAWMILLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2430", "MILLWOOD, VENEER, PLYWOOD, AND STRUCTURAL WOOD MEMBERS" ) );
					sortedSicReferences.Add( new SICReference( "2431", "MILLWORK" ) );
					sortedSicReferences.Add( new SICReference( "2434", "WOOD KITCHEN CABINETS" ) );
					sortedSicReferences.Add( new SICReference( "2435", "HARDWOOD VENEER AND PLYWOOD" ) );
					sortedSicReferences.Add( new SICReference( "2436", "SOFTWOOD VENEER AND PLYWOOD" ) );
					sortedSicReferences.Add( new SICReference( "2439", "STRUCTURAL WOOD MEMBERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2440", "WOOD CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "2441", "NAILED AND LOCK CORNER WOOD BOXES AND SHOOK" ) );
					sortedSicReferences.Add( new SICReference( "2448", "WOOD PALLETS AND SKIDS" ) );
					sortedSicReferences.Add( new SICReference( "2449", "WOOD CONTAINERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2450", "WOOD BUILDINGS AND MOBILE HOMES" ) );
					sortedSicReferences.Add( new SICReference( "2451", "MOBILE HOMES" ) );
					sortedSicReferences.Add( new SICReference( "2452", "PREFABRICATED WOOD BUILDINGS AND COMPONENTS" ) );

					sortedSicReferences.Add( new SICReference( "2490", "MISCELLANEOUS WOOD PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2491", "WOOD PRESERVING" ) );
					sortedSicReferences.Add( new SICReference( "2493", "RECONSTITUTED WOOD PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2499", "WOOD PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2510", "HOUSEHOLD FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2511", "WOOD HOUSEHOLD FURNITURE, EXCEPT UPHOLSTERED" ) );
					sortedSicReferences.Add( new SICReference( "2512", "WOOD HOUSEHOLD FURNITURE, UPHOLSTERED" ) );
					sortedSicReferences.Add( new SICReference( "2514", "METAL HOUSEHOLD FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2515", "MATTRESSES, FOUNDATIONS, AND CONVERTIBLE BEDS" ) );
					sortedSicReferences.Add( new SICReference( "2517", "WOOD TELEVISION, RADIO, PHONOGRAPH, AND SEWING MACHINE CABINETS" ) );
					sortedSicReferences.Add( new SICReference( "2519", "HOUSEHOLD FURNITURE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2520", "OFFICE FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2521", "WOOD OFFICE FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2522", "OFFICE FURNITURE, EXCEPT WOOD" ) );

					sortedSicReferences.Add( new SICReference( "2530", "PUBLIC BUILDING AND RELATED FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2531", "PUBLIC BUILDING AND RELATED FURNITURE" ) );

					sortedSicReferences.Add( new SICReference( "2540", "PARTITIONS, SHELVING, LOCKERS, AND OFFICE AND STORE FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "2541", "WOOD OFFICE AND STORE FIXTURES, PARTITIONS, SHELVING, AND LOCKERS" ) );
					sortedSicReferences.Add( new SICReference( "2542", "OFFICE AND STORE FIXTURES, PARTITIONS, SHELVING, AND LOCKERS, EXC" ) );

					sortedSicReferences.Add( new SICReference( "2590", "MISCELLANEOUS FURNITURE AND FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "2591", "DRAPERY HARDWARE AND WINDOW BLINDS AND SHADES" ) );
					sortedSicReferences.Add( new SICReference( "2599", "FURNITURE AND FIXTURES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2600", "PAPERS AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2610", "PULP MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2611", "PULP MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2620", "PAPER MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2621", "PAPER MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2630", "PAPERBOARD MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2631", "PAPERBOARD MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2650", "PAPERBOARD CONTAINERS AND BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2652", "SETUP PAPERBOARD BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2653", "CORRUGATED AND SOLID FIBER BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2655", "FIBER CANS, TUBES, DRUMS, AND SIMILAR PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2656", "SANITARY FOOD CONTAINERS, EXCEPT FOLDING" ) );
					sortedSicReferences.Add( new SICReference( "2657", "FOLDING PAPERBOARD BOXES, INCLUDING SANITARY" ) );

					sortedSicReferences.Add( new SICReference( "2670", "CONVERTED PAPER AND PAPERBOARD PRODUCTS, EXCEPT CONTAINERS AND BOX" ) );
					sortedSicReferences.Add( new SICReference( "2671", "PACKAGING PAPER AND PLASTICS FILM, COATED AND LAMINATED" ) );
					sortedSicReferences.Add( new SICReference( "2672", "COATED AND LAMINATED PAPER, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "2673", "PLASTICS, FOIL, AND COATED PAPER BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2674", "UNCOATED PAPER AND MULTIWALL BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2675", "DIE-CUT PAPER AND PAPERBOARD AND CARDBOARD" ) );
					sortedSicReferences.Add( new SICReference( "2676", "SANITARY PAPER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2677", "ENVELOPES" ) );
					sortedSicReferences.Add( new SICReference( "2678", "STATIONERY, TABLETS, AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2679", "CONVERTED PAPER AND PAPERBOARD PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2710", "NEWSPAPERS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2711", "NEWSPAPERS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2720", "PERIODICALS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2721", "PERIODICALS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2730", "BOOKS" ) );
					sortedSicReferences.Add( new SICReference( "2731", "BOOKS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2732", "BOOK PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2740", "MISCELLANEOUS PUBLISHING" ) );
					sortedSicReferences.Add( new SICReference( "2741", "MISCELLANEOUS PUBLISHING" ) );

					sortedSicReferences.Add( new SICReference( "2750", "COMMERCIAL PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2752", "COMMERCIAL PRINTING, LITHOGRAPHIC" ) );
					sortedSicReferences.Add( new SICReference( "2754", "COMMERCIAL PRINTING, GRAVURE" ) );
					sortedSicReferences.Add( new SICReference( "2759", "COMMERCIAL PRINTING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2760", "MANIFOLD BUSINESS FORMS" ) );
					sortedSicReferences.Add( new SICReference( "2761", "MANIFOLD BUSINESS FORMS" ) );


					sortedSicReferences.Add( new SICReference( "2770", "GREETING CARDS" ) );
					sortedSicReferences.Add( new SICReference( "2771", "GREETING CARDS" ) );

					sortedSicReferences.Add( new SICReference( "2780", "BLANKBOOKS, LOOSELEAF BINDERS, AND BOOKBINDING AND RELATED WORK" ) );
					sortedSicReferences.Add( new SICReference( "2782", "BLANKBOOKS, LOOSELEAF BINDERS AND DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "2789", "BOOKBINDING AND RELATED WORK" ) );

					sortedSicReferences.Add( new SICReference( "2790", "SERVICE INDUSTRIES FOR THE PRINTING TRADE" ) );
					sortedSicReferences.Add( new SICReference( "2791", "TYPESETTING" ) );
					sortedSicReferences.Add( new SICReference( "2796", "PLATEMAKING AND RELATED SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "2800", "CHEMICALS AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2810", "INDUSTRIAL INORGANIC CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2812", "ALKALIES AND CHLORINE" ) );
					sortedSicReferences.Add( new SICReference( "2813", "INDUSTRIAL GASES" ) );
					sortedSicReferences.Add( new SICReference( "2816", "INORGANIC PIGMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2819", "INDUSTRIAL INORGANIC CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2820", "PLASTICS MATERIALS AND SYNTHETIC RESINS, SYNTHETIC RUBBER, CELLULO" ) );
					sortedSicReferences.Add( new SICReference( "2821", "PLASTICS MATERIALS, SYNTHETIC RESINS, AND NONVULCANIZABLE ELASTOM" ) );
					sortedSicReferences.Add( new SICReference( "2822", "SYNTHETIC RUBBER (VULCANIZABLE ELASTOMERS)" ) );
					sortedSicReferences.Add( new SICReference( "2823", "CELLULOSIC MANMADE FIBERS" ) );
					sortedSicReferences.Add( new SICReference( "2824", "MANMADE ORGANIC FIBERS, EXCEPT CELLULOSIC" ) );

					sortedSicReferences.Add( new SICReference( "2830", "DRUGS" ) );
					sortedSicReferences.Add( new SICReference( "2833", "MEDICINAL CHEMICALS AND BOTANICAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2834", "PHARMACEUTICAL PREPARATIONS" ) );
					sortedSicReferences.Add( new SICReference( "2835", "IN VITRO AND IN VIVO DIAGNOSTIC SUBSTANCES" ) );
					sortedSicReferences.Add( new SICReference( "2836", "BIOLOGICAL PRODUCTS, EXCEPT DIAGNOSTIC SUBSTANCES" ) );

					sortedSicReferences.Add( new SICReference( "2840", "SOAP, DETERGENTS, AND CLEANING PREPARATIONS; PERFUMES, COSMETICS," ) );
					sortedSicReferences.Add( new SICReference( "2841", "SOAP AND OTHER DETERGENTS, EXCEPT SPECIALTY CLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "2842", "SPECIALTY CLEANING, POLISHING, AND SANITATION PREPARATIONS" ) );
					sortedSicReferences.Add( new SICReference( "2843", "SURFACE ACTIVE AGENTS, FINISHING AGENTS, SULFONATED OILS, AND ASS" ) );
					sortedSicReferences.Add( new SICReference( "2844", "PERFUMES, COSMETICS, AND OTHER TOILET PREPARATIONS" ) );

					sortedSicReferences.Add( new SICReference( "2850", "PAINTS, VARNISHES, LACQUERS, ENAMELS, AND ALLIED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2851", "PAINTS, VARNISHES, LACQUERS, ENAMELS, AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2860", "INDUSTRIAL ORGANIC CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2861", "GUM AND WOOD CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2865", "CYCLIC ORGANIC CRUDES AND INTERMEDIATES, AND ORGANIC DYES AND PIG" ) );
					sortedSicReferences.Add( new SICReference( "2869", "INDUSTRIAL ORGANIC CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2870", "AGRICULTURAL CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2873", "NITROGENOUS FERTILIZERS" ) );
					sortedSicReferences.Add( new SICReference( "2874", "PHOSPHATIC FERTILIZERS" ) );
					sortedSicReferences.Add( new SICReference( "2875", "FERTILIZERS, MIXING ONLY" ) );
					sortedSicReferences.Add( new SICReference( "2879", "PESTICIDES AND AGRICULTURAL CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2890", "MISCELLANEOUS CHEMICAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2891", "ADHESIVES AND SEALANTS" ) );
					sortedSicReferences.Add( new SICReference( "2892", "EXPLOSIVES" ) );
					sortedSicReferences.Add( new SICReference( "2893", "PRINTING INK" ) );
					sortedSicReferences.Add( new SICReference( "2895", "CARBON BLACK" ) );
					sortedSicReferences.Add( new SICReference( "2899", "CHEMICALS AND CHEMICAL PREPARATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2910", "PETROLEUM REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2911", "PETROLEUM REFINING" ) );

					sortedSicReferences.Add( new SICReference( "2950", "ASPHALT PAVING AND ROOFING MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "2951", "ASPHALT PAVING MIXTURES AND BLOCKS" ) );
					sortedSicReferences.Add( new SICReference( "2952", "ASPHALT FELTS AND COATINGS" ) );

					sortedSicReferences.Add( new SICReference( "2990", "MISCELLANEOUS PRODUCTS OF PETROLEUM AND COAL" ) );
					sortedSicReferences.Add( new SICReference( "2992", "LUBRICATING OILS AND GREASES" ) );
					sortedSicReferences.Add( new SICReference( "2999", "PRODUCTS OF PETROLEUM AND COAL, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3010", "TIRES AND INNER TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3011", "TIRES AND INNER TUBES" ) );

					sortedSicReferences.Add( new SICReference( "3020", "RUBBER AND PLASTICS FOOTWEAR" ) );
					sortedSicReferences.Add( new SICReference( "3021", "RUBBER AND PLASTICS FOOTWEAR" ) );

					sortedSicReferences.Add( new SICReference( "3050", "GASKETS, PACKING, AND SEALING DEVICES AND RUBBER AND PLASTICS HOSE" ) );
					sortedSicReferences.Add( new SICReference( "3052", "RUBBER AND PLASTICS HOSE AND BELTING" ) );
					sortedSicReferences.Add( new SICReference( "3053", "GASKETS, PACKING, AND SEALING DEVICES" ) );

					sortedSicReferences.Add( new SICReference( "3060", "FABRICATED RUBBER PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3061", "MOLDED, EXTRUDED, AND LATHE-CUT MECHANICAL RUBBER GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3069", "FABRICATED RUBBER PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3080", "MISCELLANEOUS PLASTICS PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3081", "UNSUPPORTED PLASTICS FILM AND SHEET" ) );
					sortedSicReferences.Add( new SICReference( "3082", "UNSUPPORTED PLASTICS PROFILE SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "3083", "LAMINATED PLASTICS PLATE, SHEET, AND PROFILE SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "3084", "PLASTICS PIPE" ) );
					sortedSicReferences.Add( new SICReference( "3085", "PLASTICS BOTTLES" ) );
					sortedSicReferences.Add( new SICReference( "3086", "PLASTICS FOAM PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3087", "CUSTOM COMPOUNDING OF PURCHASED PLASTICS RESINS" ) );
					sortedSicReferences.Add( new SICReference( "3088", "PLASTICS PLUMBING FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "3089", "PLASTICS PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3100", "LEATHER AND LEATHER PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3110", "LEATHER TANNING AND FINISHING" ) );
					sortedSicReferences.Add( new SICReference( "3111", "LEATHER TANNING AND FINISHING" ) );

					sortedSicReferences.Add( new SICReference( "3130", "BOOT AND SHOE CUT STOCK AND FINDINGS" ) );
					sortedSicReferences.Add( new SICReference( "3131", "BOOT AND SHOE CUT STOCK AND FINDINGS" ) );

					sortedSicReferences.Add( new SICReference( "3140", "FOOTWEAR, EXCEPT RUBBER" ) );
					sortedSicReferences.Add( new SICReference( "3142", "HOUSE SLIPPERS" ) );
					sortedSicReferences.Add( new SICReference( "3143", "MEN'S FOOTWEAR, EXCEPT ATHLETIC" ) );
					sortedSicReferences.Add( new SICReference( "3144", "WOMEN'S FOOTWEAR, EXCEPT ATHLETIC" ) );
					sortedSicReferences.Add( new SICReference( "3149", "FOOTWEAR, EXCEPT RUBBER, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3150", "LEATHER GLOVES AND MITTENS" ) );
					sortedSicReferences.Add( new SICReference( "3151", "LEATHER GLOVES AND MITTENS" ) );

					sortedSicReferences.Add( new SICReference( "3160", "LUGGAGE" ) );
					sortedSicReferences.Add( new SICReference( "3161", "LUGGAGE" ) );

					sortedSicReferences.Add( new SICReference( "3170", "HANDBAGS AND OTHER PERSONAL LEATHER GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3171", "WOMEN'S HANDBAGS AND PURSES" ) );
					sortedSicReferences.Add( new SICReference( "3172", "PERSONAL LEATHER GOODS, EXCEPT WOMEN'S HANDBAGS AND PURSES" ) );

					sortedSicReferences.Add( new SICReference( "3190", "LEATHER GOODS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3199", "LEATHER GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3210", "FLAT GLASS" ) );
					sortedSicReferences.Add( new SICReference( "3211", "FLAT GLASS" ) );

					sortedSicReferences.Add( new SICReference( "3220", "GLASS AND GLASSWARE, PRESSED OR BLOWN" ) );
					sortedSicReferences.Add( new SICReference( "3221", "GLASS CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "3229", "PRESSED AND BLOWN GLASS AND GLASSWARE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3230", "GLASS PRODUCTS, MADE OF PURCHASED GLASS" ) );
					sortedSicReferences.Add( new SICReference( "3231", "GLASS PRODUCTS, MADE OF PURCHASED GLASS" ) );

					sortedSicReferences.Add( new SICReference( "3240", "CEMENT, HYDRAULIC" ) );
					sortedSicReferences.Add( new SICReference( "3241", "CEMENT, HYDRAULIC" ) );

					sortedSicReferences.Add( new SICReference( "3250", "STRUCTURAL CLAY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3251", "BRICK AND STRUCTURAL CLAY TILE" ) );
					sortedSicReferences.Add( new SICReference( "3253", "CERAMIC WALL AND FLOOR TILE" ) );
					sortedSicReferences.Add( new SICReference( "3255", "CLAY REFRACTORIES" ) );
					sortedSicReferences.Add( new SICReference( "3259", "STRUCTURAL CLAY PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3260", "POTTERY AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3261", "VITREOUS CHINA PLUMBING FIXTURES AND CHINA AND EARTHENWARE FITTIN" ) );
					sortedSicReferences.Add( new SICReference( "3262", "VITREOUS CHINA TABLE AND KITCHEN ARTICLES" ) );
					sortedSicReferences.Add( new SICReference( "3263", "FINE EARTHENWARE (WHITEWARE) TABLE AND KITCHEN ARTICLES" ) );
					sortedSicReferences.Add( new SICReference( "3264", "PORCELAIN ELECTRICAL SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3269", "POTTERY PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3270", "CONCRETE, GYPSUM, AND PLASTER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3271", "CONCRETE BLOCK AND BRICK" ) );
					sortedSicReferences.Add( new SICReference( "3272", "CONCRETE PRODUCTS, EXCEPT BLOCK AND BRICK" ) );
					sortedSicReferences.Add( new SICReference( "3273", "READY-MIXED CONCRETE" ) );
					sortedSicReferences.Add( new SICReference( "3274", "LIME" ) );
					sortedSicReferences.Add( new SICReference( "3275", "GYPSUM PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3280", "CUT STONE AND STONE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3281", "CUT STONE AND STONE PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3290", "ABRASIVE, ASBESTOS, AND MISCELLANEOUS NONMETALLIC MINERAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3291", "ABRASIVE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3292", "ASBESTOS PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3295", "MINERALS AND EARTHS, GROUND OR OTHERWISE TREATED" ) );
					sortedSicReferences.Add( new SICReference( "3296", "MINERAL WOOL" ) );
					sortedSicReferences.Add( new SICReference( "3297", "NONCLAY REFRACTORIES" ) );
					sortedSicReferences.Add( new SICReference( "3299", "NONMETALLIC MINERAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3310", "STEEL WORKS, BLAST FURNACES, AND ROLLING AND FINISHING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "3312", "STEEL WORKS, BLAST FURNACES (INCLUDING COKE OVENS), AND ROLLING M" ) );
					sortedSicReferences.Add( new SICReference( "3313", "ELECTROMETALLURGICAL PRODUCTS, EXCEPT STEEL" ) );
					sortedSicReferences.Add( new SICReference( "3315", "STEEL WIREDRAWING AND STEEL NAILS AND SPIKES" ) );
					sortedSicReferences.Add( new SICReference( "3316", "COLD-ROLLED STEEL SHEET, STRIP, AND BARS" ) );
					sortedSicReferences.Add( new SICReference( "3317", "STEEL PIPE AND TUBES" ) );

					sortedSicReferences.Add( new SICReference( "3320", "IRON AND STEEL FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3321", "GRAY AND DUCTILE IRON FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3322", "MALLEABLE IRON FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3324", "STEEL INVESTMENT FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3325", "STEEL FOUNDRIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3330", "PRIMARY SMELTING AND REFINING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3331", "PRIMARY SMELTING AND REFINING OF COPPER" ) );
					sortedSicReferences.Add( new SICReference( "3334", "PRIMARY PRODUCTION OF ALUMINUM" ) );
					sortedSicReferences.Add( new SICReference( "3339", "PRIMARY SMELTING AND REFINING OF NONFERROUS METALS, EXCEPT COPPER" ) );

					sortedSicReferences.Add( new SICReference( "3340", "SECONDARY SMELTING AND REFINING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3341", "SECONDARY SMELTING AND REFINING OF NONFERROUS METALS" ) );

					sortedSicReferences.Add( new SICReference( "3350", "ROLLING, DRAWING, AND EXTRUDING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3351", "ROLLING, DRAWING, AND EXTRUDING OF COPPER" ) );
					sortedSicReferences.Add( new SICReference( "3353", "ALUMINUM SHEET, PLATE, AND FOIL" ) );
					sortedSicReferences.Add( new SICReference( "3354", "ALUMINUM EXTRUDED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3355", "ALUMINUM ROLLING AND DRAWING, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3356", "ROLLING, DRAWING, AND EXTRUDING OF NONFERROUS METALS, EXCEPT COPP" ) );
					sortedSicReferences.Add( new SICReference( "3357", "DRAWING AND INSULATING OF NONFERROUS WIRE" ) );

					sortedSicReferences.Add( new SICReference( "3360", "NONFERROUS FOUNDRIES (CASTINGS)" ) );
					sortedSicReferences.Add( new SICReference( "3363", "ALUMINUM DIE-CASTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3364", "NONFERROUS DIE-CASTINGS, EXCEPT ALUMINUM" ) );
					sortedSicReferences.Add( new SICReference( "3365", "ALUMINUM FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3366", "COPPER FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3369", "NONFERROUS FOUNDRIES, EXCEPT ALUMINUM AND COPPER" ) );

					sortedSicReferences.Add( new SICReference( "3390", "MISCELLANEOUS PRIMARY METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3398", "METAL HEAT TREATING" ) );
					sortedSicReferences.Add( new SICReference( "3399", "PRIMARY METAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3410", "METAL CANS AND SHIPPING CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "3411", "METAL CANS" ) );
					sortedSicReferences.Add( new SICReference( "3412", "METAL SHIPPING BARRELS, DRUMS, KEGS, AND PAILS" ) );

					sortedSicReferences.Add( new SICReference( "3420", "CUTLERY, HANDTOOLS, AND GENERAL HARDWARE" ) );
					sortedSicReferences.Add( new SICReference( "3421", "CUTLERY" ) );
					sortedSicReferences.Add( new SICReference( "3423", "HAND AND EDGE TOOLS, EXCEPT MACHINE TOOLS AND HANDSAWS" ) );
					sortedSicReferences.Add( new SICReference( "3425", "SAW BLADES AND HANDSAWS" ) );
					sortedSicReferences.Add( new SICReference( "3429", "HARDWARE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3430", "HEATING EQUIPMENT, EXCEPT ELECTRIC AND WARM AIR; AND PLUMBING FIXT" ) );
					sortedSicReferences.Add( new SICReference( "3431", "ENAMELED IRON AND METAL SANITARY WARE" ) );
					sortedSicReferences.Add( new SICReference( "3432", "PLUMBING FIXTURE FITTINGS AND TRIM" ) );
					sortedSicReferences.Add( new SICReference( "3433", "HEATING EQUIPMENT, EXCEPT ELECTRIC AND WARM AIR FURNACES" ) );

					sortedSicReferences.Add( new SICReference( "3440", "FABRICATED STRUCTURAL METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3441", "FABRICATED STRUCTURAL METAL" ) );
					sortedSicReferences.Add( new SICReference( "3442", "METAL DOORS, SASH, FRAMES, MOLDINGS AND TRIM" ) );
					sortedSicReferences.Add( new SICReference( "3443", "FABRICATED PLATE WORK (BOILER SHOPS)" ) );
					sortedSicReferences.Add( new SICReference( "3444", "SHEET METAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "3446", "ARCHITECTURAL AND ORNAMENTAL METALWORK" ) );
					sortedSicReferences.Add( new SICReference( "3448", "PREFABRICATED METAL BUILDINGS AND COMPONENTS" ) );
					sortedSicReferences.Add( new SICReference( "3449", "MISCELLANEOUS STRUCTURAL METALWORK" ) );

					sortedSicReferences.Add( new SICReference( "3450", "SCREW MACHINE PRODUCTS, AND BOLTS, NUTS, SCREWS, RIVETS, AND WASHE" ) );
					sortedSicReferences.Add( new SICReference( "3451", "SCREW MACHINE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3452", "BOLTS, NUTS, SCREWS, RIVETS, AND WASHERS" ) );

					sortedSicReferences.Add( new SICReference( "3460", "METAL FORGINGS AND STAMPINGS" ) );
					sortedSicReferences.Add( new SICReference( "3462", "IRON AND STEEL FORGINGS" ) );
					sortedSicReferences.Add( new SICReference( "3463", "NONFERROUS FORGINGS" ) );
					sortedSicReferences.Add( new SICReference( "3465", "AUTOMOTIVE STAMPINGS" ) );
					sortedSicReferences.Add( new SICReference( "3466", "CROWNS AND CLOSURES" ) );
					sortedSicReferences.Add( new SICReference( "3469", "METAL STAMPINGS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3470", "COATING, ENGRAVING, AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "3471", "ELECTROPLATING, PLATING, POLISHING, ANODIZING, AND COLORING" ) );
					sortedSicReferences.Add( new SICReference( "3479", "COATING, ENGRAVING, AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3480", "ORDNANCE AND ACCESSORIES, EXCEPT VEHICLES AND GUIDED MISSILES" ) );
					sortedSicReferences.Add( new SICReference( "3482", "SMALL ARMS AMMUNITION" ) );
					sortedSicReferences.Add( new SICReference( "3483", "AMMUNITION, EXCEPT FOR SMALL ARMS" ) );
					sortedSicReferences.Add( new SICReference( "3484", "SMALL ARMS" ) );
					sortedSicReferences.Add( new SICReference( "3489", "ORDNANCE AND ACCESSORIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3490", "MISCELLANEOUS FABRICATED METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3491", "INDUSTRIAL VALVES" ) );
					sortedSicReferences.Add( new SICReference( "3492", "FLUID POWER VALVES AND HOSE FITTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3493", "STEEL SPRINGS, EXCEPT WIRE" ) );
					sortedSicReferences.Add( new SICReference( "3494", "VALVES AND PIPE FITTINGS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3495", "WIRE SPRINGS" ) );
					sortedSicReferences.Add( new SICReference( "3496", "MISCELLANEOUS FABRICATED WIRE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3497", "METAL FOIL AND LEAF" ) );
					sortedSicReferences.Add( new SICReference( "3498", "FABRICATED PIPE AND PIPE FITTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3499", "FABRICATED METAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3510", "ENGINES AND TURBINES" ) );
					sortedSicReferences.Add( new SICReference( "3511", "STEAM, GAS, AND HYDRAULIC TURBINES, AND TURBINE GENERATOR SET UNI" ) );
					sortedSicReferences.Add( new SICReference( "3519", "INTERNAL COMBUSTION ENGINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3520", "FARM AND GARDEN MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3523", "FARM MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3524", "LAWN AND GARDEN TRACTORS AND HOME LAWN AND GARDEN EQUIPMENT" ) );

					sortedSicReferences.Add( new SICReference( "3530", "CONSTRUCTION, MINING, AND MATERIALS HANDLING MACHINERY AND EQUIPME" ) );
					sortedSicReferences.Add( new SICReference( "3531", "CONSTRUCTION MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3532", "MINING MACHINERY AND EQUIPMENT, EXCEPT OIL AND GAS FIELD MACHINER" ) );
					sortedSicReferences.Add( new SICReference( "3533", "OIL AND GAS FIELD MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3534", "ELEVATORS AND MOVING STAIRWAYS" ) );
					sortedSicReferences.Add( new SICReference( "3535", "CONVEYORS AND CONVEYING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3536", "OVERHEAD TRAVELING CRANES, HOISTS, AND MONORAIL SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "3537", "INDUSTRIAL TRUCKS, TRACTORS, TRAILERS, AND STACKERS" ) );

					sortedSicReferences.Add( new SICReference( "3540", "METALWORKING MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3541", "MACHINE TOOLS, METAL CUTTING TYPES" ) );
					sortedSicReferences.Add( new SICReference( "3542", "MACHINE TOOLS, METAL FORMING TYPES" ) );
					sortedSicReferences.Add( new SICReference( "3543", "INDUSTRIAL PATTERNS" ) );
					sortedSicReferences.Add( new SICReference( "3544", "SPECIAL DIES AND TOOLS, DIE SETS, JIGS AND FIXTURES, AND INDUSTRI" ) );
					sortedSicReferences.Add( new SICReference( "3545", "CUTTING TOOLS, MACHINE TOOL ACCESSORIES, AND MACHINISTS' PRECISIO" ) );
					sortedSicReferences.Add( new SICReference( "3546", "POWER-DRIVEN HANDTOOLS" ) );
					sortedSicReferences.Add( new SICReference( "3547", "ROLLING MILL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3548", "ELECTRIC AND GAS WELDING AND SOLDERING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3549", "METALWORKING MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3550", "SPECIAL INDUSTRY MACHINERY, EXCEPT METALWORKING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3552", "TEXTILE MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3553", "WOODWORKING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3554", "PAPER INDUSTRIES MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3555", "PRINTING TRADES MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3556", "FOOD PRODUCTS MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3559", "SPECIAL INDUSTRY MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3560", "GENERAL INDUSTRIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3561", "PUMPS AND PUMPING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3562", "BALL AND ROLLER BEARINGS" ) );
					sortedSicReferences.Add( new SICReference( "3563", "AIR AND GAS COMPRESSORS" ) );
					sortedSicReferences.Add( new SICReference( "3564", "INDUSTRIAL AND COMMERCIAL FANS AND BLOWERS AND AIR PURIFING EQUIP" ) );
					sortedSicReferences.Add( new SICReference( "3565", "PACKAGING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3566", "SPEED CHANGERS, INDUSTRIAL HIGH-SPEED DRIVES, AND GEARS" ) );
					sortedSicReferences.Add( new SICReference( "3567", "INDUSTRIAL PROCESS FURNACES AND OVENS" ) );
					sortedSicReferences.Add( new SICReference( "3568", "MECHANICAL POWER TRANSMISSION EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3569", "GENERAL INDUSTRIAL MACHINERY AND EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3570", "COMPUTER AND OFFICE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3571", "ELECTRONIC COMPUTERS" ) );
					sortedSicReferences.Add( new SICReference( "3572", "COMPUTER STORAGE DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3575", "COMPUTER TERMINALS" ) );
					sortedSicReferences.Add( new SICReference( "3576", "COMPUTER COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3577", "COMPUTER PERIPHERAL EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3578", "CALCULATING AND ACCOUNTING MACHINES, EXCEPT ELECTRONIC COMPUTERS" ) );
					sortedSicReferences.Add( new SICReference( "3579", "OFFICE MACHINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3580", "REFRIGERATION AND SERVICE INDUSTRY MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3581", "AUTOMATIC VENDING MACHINES" ) );
					sortedSicReferences.Add( new SICReference( "3582", "COMMERCIAL LAUNDRY, DRYCLEANING, AND PRESSING MACHINES" ) );
					sortedSicReferences.Add( new SICReference( "3585", "AIR-CONDITIONING AND WARM AIR HEATING EQUIPMENT AND COMMERCIAL AN" ) );
					sortedSicReferences.Add( new SICReference( "3586", "MEASURING AND DISPENSING PUMPS" ) );
					sortedSicReferences.Add( new SICReference( "3589", "SERVICE INDUSTRY MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3590", "MISCELLANEOUS INDUSTRIAL AND COMMERCIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3592", "CARBURETORS, PISTONS, PISTON RINGS, AND VALVES" ) );
					sortedSicReferences.Add( new SICReference( "3593", "FLUID POWER CYLINDERS AND ACTUATORS" ) );
					sortedSicReferences.Add( new SICReference( "3594", "FLUID POWER PUMPS AND MOTORS" ) );
					sortedSicReferences.Add( new SICReference( "3596", "SCALES AND BALANCES, EXCEPT LABORATORY" ) );
					sortedSicReferences.Add( new SICReference( "3599", "INDUSTRIAL AND COMMERCIAL MACHINERY AND EQUIPMENT, NOT ELSEWHERE" ) );

					sortedSicReferences.Add( new SICReference( "3600", "ELECTRONIC AND OTHER ELECTRICAL EQUIPMENT (NO COMPUTER EQUIP)" ) );

					sortedSicReferences.Add( new SICReference( "3610", "ELECTRIC TRANSMISSION AND DISTRIBUTION EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3612", "POWER, DISTRIBUTION, AND SPECIALTY TRANSFORMERS" ) );
					sortedSicReferences.Add( new SICReference( "3613", "SWITCHGEAR AND SWITCHBOARD APPARATUS" ) );

					sortedSicReferences.Add( new SICReference( "3620", "ELECTRICAL INDUSTRIAL APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3621", "MOTORS AND GENERATORS" ) );
					sortedSicReferences.Add( new SICReference( "3624", "CARBON AND GRAPHITE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3625", "RELAYS AND INDUSTRIAL CONTROLS" ) );
					sortedSicReferences.Add( new SICReference( "3629", "ELECTRICAL INDUSTRIAL APPARATUS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3630", "HOUSEHOLD APPLIANCES" ) );
					sortedSicReferences.Add( new SICReference( "3631", "HOUSEHOLD COOKING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3632", "HOUSEHOLD REFRIGERATORS AND HOME AND FARM FREEZERS" ) );
					sortedSicReferences.Add( new SICReference( "3633", "HOUSEHOLD LAUNDRY EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3634", "ELECTRIC HOUSEWARES AND FANS" ) );
					sortedSicReferences.Add( new SICReference( "3635", "HOUSEHOLD VACUUM CLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "3639", "HOUSEHOLD APPLIANCES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3640", "ELECTRIC LIGHTING AND WIRING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3641", "ELECTRIC LAMP BULBS AND TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3643", "CURRENT-CARRYING WIRING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3644", "NONCURRENT-CARRYING WIRING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3645", "RESIDENTIAL ELECTRIC LIGHTING FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "3646", "COMMERCIAL, INDUSTRIAL, AND INSTITUTIONAL ELECTRIC LIGHTING FIXTU" ) );
					sortedSicReferences.Add( new SICReference( "3647", "VEHICULAR LIGHTING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3648", "LIGHTING EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3650", "HOUSEHOLD AUDIO AND VIDEO EQUIPMENT, AND AUDIO RECORDINGS" ) );
					sortedSicReferences.Add( new SICReference( "3651", "HOUSEHOLD AUDIO AND VIDEO EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3652", "PHONOGRAPH RECORDS AND PRERECORDED AUDIO TAPES AND DISKS" ) );

					sortedSicReferences.Add( new SICReference( "3660", "COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3661", "TELEPHONE AND TELEGRAPH APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3663", "RADIO AND TELEVISION BROADCASTING AND COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3669", "COMMUNICATIONS EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3670", "ELECTRONIC COMPONENTS AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "3671", "ELECTRON TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3672", "PRINTED CIRCUIT BOARDS" ) );
					sortedSicReferences.Add( new SICReference( "3674", "SEMICONDUCTORS AND RELATED DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3675", "ELECTRONIC CAPACITORS" ) );
					sortedSicReferences.Add( new SICReference( "3676", "ELECTRONIC RESISTORS" ) );
					sortedSicReferences.Add( new SICReference( "3677", "ELECTRONIC COILS, TRANSFORMERS, AND OTHER INDUCTORS" ) );
					sortedSicReferences.Add( new SICReference( "3678", "ELECTRONIC CONNECTORS" ) );
					sortedSicReferences.Add( new SICReference( "3679", "ELECTRONIC COMPONENTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3690", "MISCELLANEOUS ELECTRICAL MACHINERY, EQUIPMENT, AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3691", "STORAGE BATTERIES" ) );
					sortedSicReferences.Add( new SICReference( "3692", "PRIMARY BATTERIES, DRY AND WET" ) );
					sortedSicReferences.Add( new SICReference( "3694", "ELECTRICAL EQUIPMENT FOR INTERNAL COMBUSTION ENGINES" ) );
					sortedSicReferences.Add( new SICReference( "3695", "MAGNETIC AND OPTICAL RECORDING MEDIA" ) );
					sortedSicReferences.Add( new SICReference( "3699", "ELECTRICAL MACHINERY, EQUIPMENT, AND SUPPLIES, NOT ELSEWHERE CLAS" ) );

					sortedSicReferences.Add( new SICReference( "3710", "MOTOR VEHICLES AND MOTOR VEHICLE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3711", "MOTOR VEHICLES AND PASSENGER CAR BODIES" ) );
					sortedSicReferences.Add( new SICReference( "3713", "TRUCK AND BUS BODIES" ) );
					sortedSicReferences.Add( new SICReference( "3714", "MOTOR VEHICLE PARTS AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "3715", "TRUCK TRAILERS" ) );
					sortedSicReferences.Add( new SICReference( "3716", "MOTOR HOMES" ) );

					sortedSicReferences.Add( new SICReference( "3720", "AIRCRAFT AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3721", "AIRCRAFT" ) );
					sortedSicReferences.Add( new SICReference( "3724", "AIRCRAFT ENGINES AND ENGINE PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3728", "AIRCRAFT PARTS AND AUXILIARY EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3730", "SHIP AND BOAT BUILDING AND REPAIRING" ) );
					sortedSicReferences.Add( new SICReference( "3731", "SHIP BUILDING AND REPAIRING" ) );
					sortedSicReferences.Add( new SICReference( "3732", "BOAT BUILDING AND REPAIRING" ) );

					sortedSicReferences.Add( new SICReference( "3740", "RAILROAD EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3743", "RAILROAD EQUIPMENT" ) );

					sortedSicReferences.Add( new SICReference( "3750", "MOTORCYCLES, BICYCLES, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3751", "MOTORCYCLES, BICYCLES, AND PARTS" ) );

					sortedSicReferences.Add( new SICReference( "3760", "GUIDED MISSILES AND SPACE VEHICLES AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3761", "GUIDED MISSILES AND SPACE VEHICLES" ) );
					sortedSicReferences.Add( new SICReference( "3764", "GUIDED MISSILE AND SPACE VEHICLE PROPULSION UNITS AND PROPULSION" ) );
					sortedSicReferences.Add( new SICReference( "3769", "GUIDED MISSILE AND SPACE VEHICLE PARTS AND AUXILIARY EQUIPMENT, N" ) );

					sortedSicReferences.Add( new SICReference( "3790", "MISCELLANEOUS TRANSPORTATION EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3792", "TRAVEL TRAILERS AND CAMPERS" ) );
					sortedSicReferences.Add( new SICReference( "3795", "TANKS AND TANK COMPONENTS" ) );
					sortedSicReferences.Add( new SICReference( "3799", "TRANSPORTATION EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3810", "SEARCH, DETECTION, NAVIGATION, GUIDANCE, AERONAUTICAL, AND NAUTICA" ) );
					sortedSicReferences.Add( new SICReference( "3812", "SEARCH, DETECTION, NAVIGATION, GUIDANCE, AERONAUTICAL, AND NAUTIC" ) );

					sortedSicReferences.Add( new SICReference( "3820", "LABORATORY APPARATUS AND ANALYTICAL, OPTICAL, MEASURING, AND CONTR" ) );
					sortedSicReferences.Add( new SICReference( "3821", "LABORATORY APPARATUS AND FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "3822", "AUTOMATIC CONTROLS FOR REGULATING RESIDENTIAL AND COMMERCIAL ENVI" ) );
					sortedSicReferences.Add( new SICReference( "3823", "INDUSTRIAL INSTRUMENTS FOR MEASUREMENT, DISPLAY, AND CONTROL OF P" ) );
					sortedSicReferences.Add( new SICReference( "3824", "TOTALIZING FLUID METERS AND COUNTING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3825", "INSTRUMENTS FOR MEASURING AND TESTING OF ELECTRICITY AND ELECTRIC" ) );
					sortedSicReferences.Add( new SICReference( "3826", "LABORATORY ANALYTICAL INSTRUMENTS" ) );
					sortedSicReferences.Add( new SICReference( "3827", "OPTICAL INSTRUMENTS AND LENSES" ) );
					sortedSicReferences.Add( new SICReference( "3829", "MEASURING AND CONTROLLING DEVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3840", "SURGICAL, MEDICAL, AND DENTAL INSTRUMENTS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3841", "SURGICAL AND MEDICAL INSTRUMENTS AND APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3842", "ORTHOPEDIC, PROSTHETIC, AND SURGICAL APPLIANCES AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3843", "DENTAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3844", "X-RAY APPARATUS AND TUBES AND RELATED IRRADIATION APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3845", "ELECTROMEDICAL AND ELECTROTHERAPEUTIC APPARATUS" ) );

					sortedSicReferences.Add( new SICReference( "3850", "OPHTHALMIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3851", "OPHTHALMIC GOODS" ) );

					sortedSicReferences.Add( new SICReference( "3860", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3861", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );

					sortedSicReferences.Add( new SICReference( "3870", "WATCHES, CLOCKS, CLOCKWORK OPERATED DEVICES, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3873", "WATCHES, CLOCKS, CLOCKWORK OPERATED DEVICES, AND PARTS" ) );

					sortedSicReferences.Add( new SICReference( "3910", "JEWELRY, SILVERWARE, AND PLATED WARE" ) );
					sortedSicReferences.Add( new SICReference( "3911", "JEWELRY, PRECIOUS METAL" ) );
					sortedSicReferences.Add( new SICReference( "3914", "SILVERWARE, PLATED WARE, AND STAINLESS STEEL WARE" ) );
					sortedSicReferences.Add( new SICReference( "3915", "JEWELERS' FINDINGS AND MATERIALS, AND LAPIDARY WORK" ) );

					sortedSicReferences.Add( new SICReference( "3930", "MUSICAL INSTRUMENTS" ) );
					sortedSicReferences.Add( new SICReference( "3931", "MUSICAL INSTRUMENTS" ) );

					sortedSicReferences.Add( new SICReference( "3940", "DOLLS, TOYS, GAMES AND SPORTING AND ATHLETIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3942", "DOLLS AND STUFFED TOYS" ) );
					sortedSicReferences.Add( new SICReference( "3944", "GAMES, TOYS, AND CHILDREN'S VEHICLES, EXCEPT DOLLS AND BICYCLES" ) );
					sortedSicReferences.Add( new SICReference( "3949", "SPORTING AND ATHLETIC GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3950", "PENS, PENCILS, AND OTHER ARTISTS' MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "3951", "PENS, MECHANICAL PENCILS, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3952", "LEAD PENCILS, CRAYONS, AND ARTISTS' MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "3953", "MARKING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3955", "CARBON PAPER AND INKED RIBBONS" ) );

					sortedSicReferences.Add( new SICReference( "3960", "COSTUME JEWELRY, COSTUME NOVELTIES, BUTTONS, AND MISCELLANEOUS NOT" ) );
					sortedSicReferences.Add( new SICReference( "3961", "COSTUME JEWELRY AND COSTUME NOVELTIES, EXCEPT PRECIOUS METAL" ) );
					sortedSicReferences.Add( new SICReference( "3965", "FASTENERS, BUTTONS, NEEDLES, AND PINS" ) );

					sortedSicReferences.Add( new SICReference( "3990", "MISCELLANEOUS MANUFACTURING INDUSTRIES" ) );
					sortedSicReferences.Add( new SICReference( "3991", "BROOMS AND BRUSHES" ) );
					sortedSicReferences.Add( new SICReference( "3993", "SIGNS AND ADVERTISING SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "3995", "BURIAL CASKETS" ) );
					sortedSicReferences.Add( new SICReference( "3996", "LINOLEUM, ASPHALTED-FELT-BASE, AND OTHER HARD SURFACE FLOOR COVER" ) );
					sortedSicReferences.Add( new SICReference( "3999", "MANUFACTURING INDUSTRIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4010", "RAILROADS" ) );
					sortedSicReferences.Add( new SICReference( "4011", "RAILROADS, LINE-HAUL OPERATING" ) );
					sortedSicReferences.Add( new SICReference( "4013", "RAILROAD SWITCHING AND TERMINAL ESTABLISHMENTS" ) );

					sortedSicReferences.Add( new SICReference( "4100", "LOCAL AND SUBURBAN TRANSIT AND INTERURBAN HWY PASSENGER TRANS" ) );

					sortedSicReferences.Add( new SICReference( "4110", "LOCAL AND SUBURBAN PASSENGER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4111", "LOCAL AND SUBURBAN TRANSIT" ) );
					sortedSicReferences.Add( new SICReference( "4119", "LOCAL PASSENGER TRANSPORTATION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4120", "TAXICABS" ) );
					sortedSicReferences.Add( new SICReference( "4121", "TAXICABS" ) );

					sortedSicReferences.Add( new SICReference( "4130", "INTERCITY AND RURAL BUS TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4131", "INTERCITY AND RURAL BUS TRANSPORTATION" ) );

					sortedSicReferences.Add( new SICReference( "4140", "BUS CHARTER SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4141", "LOCAL BUS CHARTER SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4142", "BUS CHARTER SERVICE, EXCEPT LOCAL" ) );

					sortedSicReferences.Add( new SICReference( "4150", "SCHOOL BUSES" ) );
					sortedSicReferences.Add( new SICReference( "4151", "SCHOOL BUSES" ) );

					sortedSicReferences.Add( new SICReference( "4170", "TERMINAL AND SERVICE FACILITIES FOR MOTOR VEHICLE PASSENGER TRANSP" ) );
					sortedSicReferences.Add( new SICReference( "4173", "TERMINAL AND SERVICE FACILITIES FOR MOTOR VEHICLE PASSENGER TRANS" ) );

					sortedSicReferences.Add( new SICReference( "4210", "TRUCKING AND COURIER SERVICES, EXCEPT AIR" ) );
					sortedSicReferences.Add( new SICReference( "4212", "LOCAL TRUCKING WITHOUT STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4213", "TRUCKING, EXCEPT LOCAL" ) );
					sortedSicReferences.Add( new SICReference( "4214", "LOCAL TRUCKING WITH STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4215", "COURIER SERVICES, EXCEPT BY AIR" ) );

					sortedSicReferences.Add( new SICReference( "4220", "PUBLIC WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4221", "FARM PRODUCT WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4222", "REFRIGERATED WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4225", "GENERAL WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4226", "SPECIAL WAREHOUSING AND STORAGE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4230", "TERMINAL AND JOINT TERMINAL MAINTENANCE FACILITIES FOR MOTOR FREIG" ) );
					sortedSicReferences.Add( new SICReference( "4231", "TERMINAL AND JOINT TERMINAL MAINTENANCE FACILITIES FOR MOTOR FREI" ) );

					sortedSicReferences.Add( new SICReference( "4310", "UNITED STATES POSTAL SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4311", "UNITED STATES POSTAL SERVICE" ) );

					sortedSicReferences.Add( new SICReference( "4400", "WATER TRANSPORTATION" ) );

					sortedSicReferences.Add( new SICReference( "4410", "DEEP SEA FOREIGN TRANSPORTATION OF FREIGHT" ) );
					sortedSicReferences.Add( new SICReference( "4412", "DEEP SEA FOREIGN TRANSPORTATION OF FREIGHT" ) );

					sortedSicReferences.Add( new SICReference( "4420", "DEEP SEA DOMESTIC TRANSPORTATION OF FREIGHT" ) );
					sortedSicReferences.Add( new SICReference( "4424", "DEEP SEA DOMESTIC TRANSPORTATION OF FREIGHT" ) );

					sortedSicReferences.Add( new SICReference( "4430", "FREIGHT TRANSPORTATION ON THE GREAT LAKES&die;ST. LAWRENCE SEAWAY" ) );
					sortedSicReferences.Add( new SICReference( "4432", "FREIGHT TRANSPORTATION ON THE GREAT LAKES&die;ST. LAWRENCE SEAWAY" ) );

					sortedSicReferences.Add( new SICReference( "4440", "WATER TRANSPORTATION OF FREIGHT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "4449", "WATER TRANSPORTATION OF FREIGHT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4480", "WATER TRANSPORTATION OF PASSENGERS" ) );
					sortedSicReferences.Add( new SICReference( "4481", "DEEP SEA TRANSPORTATION OF PASSENGERS, EXCEPT BY FERRY" ) );
					sortedSicReferences.Add( new SICReference( "4482", "FERRIES" ) );
					sortedSicReferences.Add( new SICReference( "4489", "WATER TRANSPORTATION OF PASSENGERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4490", "SERVICES INCIDENTAL TO WATER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4491", "MARINE CARGO HANDLING" ) );
					sortedSicReferences.Add( new SICReference( "4492", "TOWING AND TUGBOAT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4493", "MARINAS" ) );
					sortedSicReferences.Add( new SICReference( "4499", "WATER TRANSPORTATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4510", "AIR TRANSPORTATION, SCHEDULED, AND AIR COURIER SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4512", "AIR TRANSPORTATION, SCHEDULED" ) );
					sortedSicReferences.Add( new SICReference( "4513", "AIR COURIER SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4520", "AIR TRANSPORTATION, NONSCHEDULED" ) );
					sortedSicReferences.Add( new SICReference( "4522", "AIR TRANSPORTATION, NONSCHEDULED" ) );

					sortedSicReferences.Add( new SICReference( "4580", "AIRPORTS, FLYING FIELDS, AND AIRPORT TERMINAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4581", "AIRPORTS, FLYING FIELDS, AND AIRPORT TERMINAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4610", "PIPELINES, EXCEPT NATURAL GAS" ) );
					sortedSicReferences.Add( new SICReference( "4612", "CRUDE PETROLEUM PIPELINES" ) );
					sortedSicReferences.Add( new SICReference( "4613", "REFINED PETROLEUM PIPELINES" ) );
					sortedSicReferences.Add( new SICReference( "4619", "PIPELINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4700", "TRANSPORTATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4720", "ARRANGEMENT OF PASSENGER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4724", "TRAVEL AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "4725", "TOUR OPERATORS" ) );
					sortedSicReferences.Add( new SICReference( "4729", "ARRANGEMENT OF PASSENGER TRANSPORTATION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4730", "ARRANGEMENT OF TRANSPORTATION OF FREIGHT AND CARGO" ) );
					sortedSicReferences.Add( new SICReference( "4731", "ARRANGEMENT OF TRANSPORTATION OF FREIGHT AND CARGO" ) );

					sortedSicReferences.Add( new SICReference( "4740", "RENTAL OF RAILROAD CARS" ) );
					sortedSicReferences.Add( new SICReference( "4741", "RENTAL OF RAILROAD CARS" ) );

					sortedSicReferences.Add( new SICReference( "4780", "MISCELLANEOUS SERVICES INCIDENTAL TO TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4783", "PACKING AND CRATING" ) );
					sortedSicReferences.Add( new SICReference( "4785", "FIXED FACILITIES AND INSPECTION AND WEIGHING SERVICES FOR MOTOR V" ) );
					sortedSicReferences.Add( new SICReference( "4789", "TRANSPORTATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4810", "TELEPHONE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4812", "RADIOTELEPHONE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4813", "TELEPHONE COMMUNICATIONS, EXCEPT RADIOTELEPHONE" ) );

					sortedSicReferences.Add( new SICReference( "4820", "TELEGRAPH AND OTHER MESSAGE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4822", "TELEGRAPH AND OTHER MESSAGE COMMUNICATIONS" ) );

					sortedSicReferences.Add( new SICReference( "4830", "RADIO AND TELEVISION BROADCASTING STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4832", "RADIO BROADCASTING STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4833", "TELEVISION BROADCASTING STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "4840", "CABLE AND OTHER PAY TELEVISION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4841", "CABLE AND OTHER PAY TELEVISION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4890", "COMMUNICATIONS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "4899", "COMMUNICATIONS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4900", "ELECTRIC, GAS AND SANITARY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4910", "ELECTRIC SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4911", "ELECTRIC SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4920", "GAS PRODUCTION AND DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4922", "NATURAL GAS TRANSMISSION" ) );
					sortedSicReferences.Add( new SICReference( "4923", "NATURAL GAS TRANSMISISON AND DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4924", "NATURAL GAS DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4925", "MIXED, MANUFACTURED, OR LIQUEFIED PETROLEUM GAS PRODUCTION AND/OR" ) );

					sortedSicReferences.Add( new SICReference( "4930", "COMBINATION ELECTRIC AND GAS, AND OTHER UTILITY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4931", "ELECTRIC AND OTHER SERVICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "4932", "GAS AND OTHER SERVICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "4939", "COMBINATION UTILITIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4940", "WATER SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "4941", "WATER SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "4950", "SANITARY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4952", "SEWERAGE SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4953", "REFUSE SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4955", "HAZARDOUS WASTE MANAGEMENT" ) );
					sortedSicReferences.Add( new SICReference( "4959", "SANITARY SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4960", "STEAM AND AIR-CONDITIONING SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "4961", "STEAM AND AIR-CONDITIONING SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "4970", "IRRIGATION SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4971", "IRRIGATION SYSTEMS" ) );

					sortedSicReferences.Add( new SICReference( "4991", "COGENERATION SERVICES AND SMALL POWER PRODUCERS" ) );

					sortedSicReferences.Add( new SICReference( "5000", "WHOLESALE-DURABLE GOODS" ) );

					sortedSicReferences.Add( new SICReference( "5010", "WHOLESALE-MOTOR VEHICLES AND MOTOR VEHICLE PARTS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5012", "AUTOMOBILES AND OTHER MOTOR VEHICLES" ) );
					sortedSicReferences.Add( new SICReference( "5013", "WHOLESALE-MOTOR VEHICLE SUPPLIES AND NEW PARTS" ) );
					sortedSicReferences.Add( new SICReference( "5014", "TIRES AND TUBES" ) );
					sortedSicReferences.Add( new SICReference( "5015", "MOTOR VEHICLE PARTS, USED" ) );

					sortedSicReferences.Add( new SICReference( "5020", "WHOLESALE-FURNITURE AND HOME FURNISHINGS" ) );
					sortedSicReferences.Add( new SICReference( "5021", "FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "5023", "HOMEFURNISHINGS" ) );

					sortedSicReferences.Add( new SICReference( "5030", "WHOLESALE-LUMBER AND OTHER CONSTRUCTION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5031", "WHOLESALE-LUMBER, PLYWOOD, MILLWORK AND WOOD PANELS" ) );
					sortedSicReferences.Add( new SICReference( "5032", "BRICK, STONE, AND RELATED CONSTRUCTION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5033", "ROOFING, SIDING, AND INSULATION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5039", "CONSTRUCTION MATERIALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5040", "WHOLESALE-PROFESSIONAL AND COMMERCIAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5043", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5044", "OFFICE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5045", "WHOLESALE-COMPUTERS AND PERIPHERAL EQUIPMENT AND SOFTWARE" ) );
					sortedSicReferences.Add( new SICReference( "5046", "COMMERCIAL EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5047", "WHOLESALE-MEDICAL, DENTAL AND HOSPITAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5048", "OPHTHALMIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5049", "PROFESSIONAL EQUIPMENT AND SUPPLIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5050", "WHOLESALE-METALS AND MINERALS (NO PETROLEUM)" ) );
					sortedSicReferences.Add( new SICReference( "5051", "WHOLESALE-METALS SERVICE CENTERS AND OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "5052", "COAL AND OTHER MINERALS AND ORES" ) );

					sortedSicReferences.Add( new SICReference( "5060", "ELECTRICAL GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5063", "ELECTRICAL APPARATUS AND EQUIPMENT, WIRING SUPPLIES, AND CONSTRUC" ) );
					sortedSicReferences.Add( new SICReference( "5064", "WHOLESALE-ELECTRICAL APPLIANCES, TV AND RADIO SETS" ) );
					sortedSicReferences.Add( new SICReference( "5065", "WHOLESALE-ELECTRONIC PARTS AND EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5070", "WHOLESALE-HARDWARE AND PLUMBING AND HEATING EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5072", "WHOLESALE-HARDWARE" ) );
					sortedSicReferences.Add( new SICReference( "5074", "PLUMBING AND HEATING EQUIPMENT AND SUPPLIES (HYDRONICS)" ) );
					sortedSicReferences.Add( new SICReference( "5075", "WARM AIR HEATING AND AIR-CONDITIONING EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5078", "REFRIGERATION EQUIPMENT AND SUPPLIES" ) );

					sortedSicReferences.Add( new SICReference( "5080", "WHOLESALE-MACHINERY, EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5082", "CONSTRUCTION AND MINING (EXCEPT PETROLEUM) MACHINERY AND EQUIPMEN" ) );
					sortedSicReferences.Add( new SICReference( "5083", "FARM AND GARDEN MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5084", "WHOLESALE-INDUSTRIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5085", "INDUSTRIAL SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5087", "SERVICE ESTABLISHMENT EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5088", "TRANSPORTATION EQUIPMENT AND SUPPLIES, EXCEPT MOTOR VEHICLES" ) );

					sortedSicReferences.Add( new SICReference( "5090", "WHOLESALE-MISC DURABLE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5091", "SPORTING AND RECREATIONAL GOODS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5092", "TOYS AND HOBBY GOODS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5093", "SCRAP AND WASTE MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5094", "WHOLESALE-JEWELRY, WATCHES, PRECIOUS STONES AND METALS" ) );
					sortedSicReferences.Add( new SICReference( "5099", "WHOLESALE-DURABLE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5110", "WHOLESALE-PAPER AND PAPER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5111", "PRINTING AND WRITING PAPER" ) );
					sortedSicReferences.Add( new SICReference( "5112", "STATIONERY AND OFFICE SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5113", "INDUSTRIAL AND PERSONAL SERVICE PAPER" ) );

					sortedSicReferences.Add( new SICReference( "5120", "DRUGS, DRUG PROPRIETARIES, AND DRUGGISTS' SUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "5122", "WHOLESALE-DRUGS, PROPRIETARIES AND DRUGGISTS' SUNDRIES" ) );

					sortedSicReferences.Add( new SICReference( "5130", "WHOLESALE-APPAREL, PIECE GOODS AND NOTIONS" ) );
					sortedSicReferences.Add( new SICReference( "5131", "PIECE GOODS, NOTIONS, AND OTHER DRY GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5136", "MEN'S AND BOYS' CLOTHING AND FURNISHINGS" ) );
					sortedSicReferences.Add( new SICReference( "5137", "WOMEN'S, CHILDREN'S, AND INFANTS' CLOTHING AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "5139", "FOOTWEAR" ) );

					sortedSicReferences.Add( new SICReference( "5140", "WHOLESALE-GROCERIES AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5141", "WHOLESALE-GROCERIES, GENERAL LINE" ) );
					sortedSicReferences.Add( new SICReference( "5142", "PACKAGED FROZEN FOODS" ) );
					sortedSicReferences.Add( new SICReference( "5143", "DAIRY PRODUCTS, EXCEPT DRIED OR CANNED" ) );
					sortedSicReferences.Add( new SICReference( "5144", "POULTRY AND POULTRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5145", "CONFECTIONERY" ) );
					sortedSicReferences.Add( new SICReference( "5146", "FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "5147", "MEATS AND MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5148", "FRESH FRUITS AND VEGETABLES" ) );
					sortedSicReferences.Add( new SICReference( "5149", "GROCERIES AND RELATED PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5150", "WHOLESALE-FARM PRODUCT RAW MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5153", "GRAIN AND FIELD BEANS" ) );
					sortedSicReferences.Add( new SICReference( "5154", "LIVESTOCK" ) );
					sortedSicReferences.Add( new SICReference( "5159", "FARM-PRODUCT RAW MATERIALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5160", "WHOLESALE-CHEMICALS AND ALLIED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5162", "PLASTICS MATERIALS AND BASIC FORMS AND SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "5169", "CHEMICALS AND ALLIED PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5170", "PETROLEUM AND PETROLEUM PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5171", "WHOLESALE-PETROLEUM BULK STATIONS AND TERMINALS" ) );
					sortedSicReferences.Add( new SICReference( "5172", "PETROLEUM AND PETROLEUM PRODUCTS WHOLESALERS, EXCEPT BULK STATION" ) );

					sortedSicReferences.Add( new SICReference( "5180", "WHOLESALE-BEER, WINE AND DISTILLED ALCOHOLIC BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "5181", "BEER AND ALE" ) );
					sortedSicReferences.Add( new SICReference( "5182", "WINE AND DISTILLED ALCOHOLIC BEVERAGES" ) );

					sortedSicReferences.Add( new SICReference( "5190", "WHOLESALE-MISCELLANEOUS NONDURABLE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5191", "FARM SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5192", "BOOKS, PERIODICALS, AND NEWSPAPERS" ) );
					sortedSicReferences.Add( new SICReference( "5193", "FLOWERS, NURSERY STOCK, AND FLORISTS' SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5194", "TOBACCO AND TOBACCO PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5198", "PAINTS, VARNISHES, AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5199", "NONDURABLE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5200", "RETAIL-BUILDING MATERIALS, HARDWARE, GARDEN SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "5210", "LUMBER AND OTHER BUILDING MATERIALS DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5211", "RETAIL-LUMBER AND OTHER BUILDING MATERIALS DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5230", "PAINT, GLASS, AND WALLPAPER STORES" ) );
					sortedSicReferences.Add( new SICReference( "5231", "PAINT, GLASS, AND WALLPAPER STORES" ) );

					sortedSicReferences.Add( new SICReference( "5250", "HARDWARE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5251", "HARDWARE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5260", "RETAIL NURSERIES, LAWN AND GARDEN SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5261", "RETAIL NURSERIES, LAWN AND GARDEN SUPPLY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5270", "MOBILE HOME DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5271", "RETAIL-MOBILE HOME DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5310", "DEPARTMENT STORES" ) );
					sortedSicReferences.Add( new SICReference( "5311", "RETAIL-DEPARTMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5330", "VARIETY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5331", "RETAIL-VARIETY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5390", "MISCELLANEOUS GENERAL MERCHANDISE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5399", "MISCELLANEOUS GENERAL MERCHANDISE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5400", "RETAIL-FOOD STORES" ) );

					sortedSicReferences.Add( new SICReference( "5410", "GROCERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5411", "RETAIL-GROCERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5412", "RETAIL-CONVENIENCE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5420", "MEAT AND FISH (SEAFOOD) MARKETS, INCLUDING FREEZER PROVISIONERS" ) );
					sortedSicReferences.Add( new SICReference( "5421", "MEAT AND FISH (SEAFOOD) MARKETS, INCLUDING FREEZER PROVISIONERS" ) );

					sortedSicReferences.Add( new SICReference( "5430", "FRUIT AND VEGETABLE MARKETS" ) );
					sortedSicReferences.Add( new SICReference( "5431", "FRUIT AND VEGETABLE MARKETS" ) );

					sortedSicReferences.Add( new SICReference( "5440", "CANDY, NUT, AND CONFECTIONERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5441", "CANDY, NUT, AND CONFECTIONERY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5450", "DAIRY PRODUCTS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5451", "DAIRY PRODUCTS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5460", "RETAIL BAKERIES" ) );
					sortedSicReferences.Add( new SICReference( "5461", "RETAIL BAKERIES" ) );

					sortedSicReferences.Add( new SICReference( "5490", "MISCELLANEOUS FOOD STORES" ) );
					sortedSicReferences.Add( new SICReference( "5499", "MISCELLANEOUS FOOD STORES" ) );

					sortedSicReferences.Add( new SICReference( "5500", "RETAIL-AUTO DEALERS AND GASOLINE STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "5510", "MOTOR VEHICLE DEALERS (NEW AND USED)" ) );
					sortedSicReferences.Add( new SICReference( "5511", "MOTOR VEHICLE DEALERS (NEW AND USED)" ) );

					sortedSicReferences.Add( new SICReference( "5520", "MOTOR VEHICLE DEALERS (USED ONLY)" ) );
					sortedSicReferences.Add( new SICReference( "5521", "MOTOR VEHICLE DEALERS (USED ONLY)" ) );

					sortedSicReferences.Add( new SICReference( "5530", "AUTO AND HOME SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5531", "RETAIL-AUTO AND HOME SUPPLY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5540", "GASOLINE SERVICE STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "5541", "GASOLINE SERVICE STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "5550", "BOAT DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5551", "BOAT DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5560", "RECREATIONAL VEHICLE DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5561", "RECREATIONAL VEHICLE DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5570", "MOTORCYCLE DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5571", "MOTORCYCLE DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5590", "AUTOMOTIVE DEALERS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5599", "AUTOMOTIVE DEALERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5600", "RETAIL-APPAREL AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5610", "MEN'S AND BOYS' CLOTHING AND ACCESSORY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5611", "MEN'S AND BOYS' CLOTHING AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5620", "WOMEN'S CLOTHING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5621", "RETAIL-WOMEN'S CLOTHING STORES" ) );

					sortedSicReferences.Add( new SICReference( "5630", "WOMEN'S ACCESSORY AND SPECIALTY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5632", "WOMEN'S ACCESSORY AND SPECIALTY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5640", "CHILDREN'S AND INFANTS' WEAR STORES" ) );
					sortedSicReferences.Add( new SICReference( "5641", "CHILDREN'S AND INFANTS' WEAR STORES" ) );

					sortedSicReferences.Add( new SICReference( "5650", "FAMILY CLOTHING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5651", "RETAIL-FAMILY CLOTHING STORES" ) );

					sortedSicReferences.Add( new SICReference( "5660", "SHOE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5661", "RETAIL-SHOE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5690", "MISCELLANEOUS APPAREL AND ACCESSORY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5699", "MISCELLANEOUS APPAREL AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5700", "RETAIL-HOME FURNITURE, FURNISHINGS AND EQUIPMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5710", "HOME FURNITURE AND FURNISHINGS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5712", "RETAIL-FURNITURE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5713", "FLOOR COVERING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5714", "DRAPERY, CURTAIN, AND UPHOLSTERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5719", "MISCELLANEOUS HOMEFURNISHINGS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5720", "HOUSEHOLD APPLIANCE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5722", "HOUSEHOLD APPLIANCE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5730", "RADIO, TELEVISION, CONSUMER ELECTRONICS, AND MUSIC STORES" ) );
					sortedSicReferences.Add( new SICReference( "5731", "RADIO, TELEVISION, AND CONSUMER ELECTRONICS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5734", "RETAIL-COMPUTER AND COMPUTER SOFTWARE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5735", "RETAIL-RECORD AND PRERECORDED TAPE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5736", "MUSICAL INSTRUMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5810", "RETAIL-EATING AND DRINKING PLACES" ) );
					sortedSicReferences.Add( new SICReference( "5812", "RETAIL-EATING  PLACES" ) );
					sortedSicReferences.Add( new SICReference( "5813", "DRINKING PLACES (ALCOHOLIC BEVERAGES)" ) );

					sortedSicReferences.Add( new SICReference( "5900", "RETAIL-MISCELLANEOUS RETAIL" ) );

					sortedSicReferences.Add( new SICReference( "5910", "DRUG STORES AND PROPRIETARY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5912", "RETAIL-DRUG STORES AND PROPRIETARY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5920", "LIQUOR STORES" ) );
					sortedSicReferences.Add( new SICReference( "5921", "LIQUOR STORES" ) );

					sortedSicReferences.Add( new SICReference( "5930", "USED MERCHANDISE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5932", "USED MERCHANDISE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5940", "RETAIL-MISCELLANEOUS SHOPPING GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5941", "SPORTING GOODS STORES AND BICYCLE SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5942", "BOOK STORES" ) );
					sortedSicReferences.Add( new SICReference( "5943", "STATIONERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5944", "RETAIL-JEWELRY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5945", "RETAIL-HOBBY, TOY AND GAME SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5946", "CAMERA AND PHOTOGRAPHIC SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5947", "GIFT, NOVELTY, AND SOUVENIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5948", "LUGGAGE AND LEATHER GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5949", "SEWING, NEEDLEWORK, AND PIECE GOODS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5960", "RETAIL-NONSTORE RETAILERS" ) );
					sortedSicReferences.Add( new SICReference( "5961", "RETAIL-CATALOG AND MAIL-ORDER HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "5962", "AUTOMATIC MERCHANDISING MACHINE OPERATORS" ) );
					sortedSicReferences.Add( new SICReference( "5963", "DIRECT SELLING ESTABLISHMENTS" ) );

					sortedSicReferences.Add( new SICReference( "5980", "FUEL DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5983", "FUEL OIL DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5984", "LIQUEFIED PETROLEUM GAS (BOTTLED GAS) DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5989", "FUEL DEALERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5990", "RETAIL-RETAIL STORES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5992", "FLORISTS" ) );
					sortedSicReferences.Add( new SICReference( "5993", "TOBACCO STORES AND STANDS" ) );
					sortedSicReferences.Add( new SICReference( "5994", "NEWS DEALERS AND NEWSSTANDS" ) );
					sortedSicReferences.Add( new SICReference( "5995", "OPTICAL GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5999", "MISCELLANEOUS RETAIL STORES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6010", "CENTRAL RESERVE DEPOSITORY INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6011", "FEDERAL RESERVE BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6019", "CENTRAL RESERVE DEPOSITORY INSTITUTIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6020", "COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6021", "NATIONAL COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6022", "STATE COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6029", "COMMERCIAL BANKS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6030", "SAVINGS INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6035", "SAVINGS INSTITUTIONS, FEDERALLY CHARTERED" ) );
					sortedSicReferences.Add( new SICReference( "6036", "SAVINGS INSTITUTIONS, NOT FEDERALLY CHARTERED" ) );

					sortedSicReferences.Add( new SICReference( "6060", "CREDIT UNIONS" ) );
					sortedSicReferences.Add( new SICReference( "6061", "CREDIT UNIONS, FEDERALLY CHARTERED" ) );
					sortedSicReferences.Add( new SICReference( "6062", "CREDIT UNIONS, NOT FEDERALLY CHARTERED" ) );

					sortedSicReferences.Add( new SICReference( "6080", "FOREIGN BANKING AND BRANCHES AND AGENCIES OF FOREIGN BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6081", "BRANCHES AND AGENCIES OF FOREIGN BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6082", "FOREIGN TRADE AND INTERNATIONAL BANKING INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6090", "FUNCTIONS RELATED TO DEPOSITORY BANKING" ) );
					sortedSicReferences.Add( new SICReference( "6091", "NONDEPOSIT TRUST FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "6099", "FUNCTIONS RELATED TO DEPOSITORY BANKING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6110", "FEDERAL AND FEDERALLY-SPONSORED CREDIT AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "6111", "FEDERAL AND FEDERALLY-SPONSORED CREDIT AGENCIES" ) );

					sortedSicReferences.Add( new SICReference( "6140", "PERSONAL CREDIT INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6141", "PERSONAL CREDIT INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6150", "BUSINESS CREDIT INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6153", "SHORT-TERM BUSINESS CREDIT INSTITUTIONS, EXCEPT AGRICULTURAL" ) );
					sortedSicReferences.Add( new SICReference( "6159", "MISCELLANEOUS BUSINESS CREDIT INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6160", "MORTGAGE BANKERS AND BROKERS" ) );
					sortedSicReferences.Add( new SICReference( "6162", "MORTGAGE BANKERS AND LOAN CORRESPONDENTS" ) );
					sortedSicReferences.Add( new SICReference( "6163", "LOAN BROKERS" ) );

					sortedSicReferences.Add( new SICReference( "6172", "FINANCE LESSORS" ) );

					sortedSicReferences.Add( new SICReference( "6189", "ASSET-BACKED SECURITIES" ) );

					sortedSicReferences.Add( new SICReference( "6199", "FINANCE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "6200", "SECURITY AND COMMODITY BROKERS, DEALERS, EXCHANGES AND SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "6210", "SECURITY BROKERS, DEALERS, AND FLOTATION COMPANIES" ) );
					sortedSicReferences.Add( new SICReference( "6211", "SECURITY BROKERS, DEALERS, AND FLOTATION COMPANIES" ) );

					sortedSicReferences.Add( new SICReference( "6220", "COMMODITY CONTRACTS BROKERS AND DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "6221", "COMMODITY CONTRACTS BROKERS AND DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "6230", "SECURITY AND COMMODITY EXCHANGES" ) );
					sortedSicReferences.Add( new SICReference( "6231", "SECURITY AND COMMODITY EXCHANGES" ) );

					sortedSicReferences.Add( new SICReference( "6280", "SERVICES ALLIED WITH THE EXCHANGE OF SECURITIES OR COMMODITIES" ) );
					sortedSicReferences.Add( new SICReference( "6282", "INVESTMENT ADVICE" ) );
					sortedSicReferences.Add( new SICReference( "6289", "SERVICES ALLIED WITH THE EXCHANGE OF SECURITIES OR COMMODITIES, N" ) );

					sortedSicReferences.Add( new SICReference( "6310", "LIFE INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6311", "LIFE INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6320", "ACCIDENT AND HEALTH INSURANCE AND MEDICAL SERVICE PLANS" ) );
					sortedSicReferences.Add( new SICReference( "6321", "ACCIDENT AND HEALTH INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6324", "HOSPITAL AND MEDICAL SERVICE PLANS" ) );

					sortedSicReferences.Add( new SICReference( "6330", "FIRE, MARINE, AND CASUALTY INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6331", "FIRE, MARINE, AND CASUALTY INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6350", "SURETY INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6351", "SURETY INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6360", "TITLE INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6361", "TITLE INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6370", "PENSION, HEALTH, AND WELFARE FUNDS" ) );
					sortedSicReferences.Add( new SICReference( "6371", "PENSION, HEALTH, AND WELFARE FUNDS" ) );

					sortedSicReferences.Add( new SICReference( "6390", "INSURANCE CARRIERS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "6399", "INSURANCE CARRIERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6410", "INSURANCE AGENTS, BROKERS, AND SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "6411", "INSURANCE AGENTS, BROKERS, AND SERVICE" ) );

					sortedSicReferences.Add( new SICReference( "6500", "REAL ESTATE" ) );

					sortedSicReferences.Add( new SICReference( "6510", "REAL ESTATE OPERATORS (EXCEPT DEVELOPERS) AND LESSORS" ) );
					sortedSicReferences.Add( new SICReference( "6512", "OPERATORS OF NONRESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6513", "OPERATORS OF APARTMENT BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6514", "OPERATORS OF DWELLINGS OTHER THAN APARTMENT BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6515", "OPERATORS OF RESIDENTIAL MOBILE HOME SITES" ) );
					sortedSicReferences.Add( new SICReference( "6517", "LESSORS OF RAILROAD PROPERTY" ) );
					sortedSicReferences.Add( new SICReference( "6519", "LESSORS OF REAL PROPERTY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6530", "REAL ESTATE AGENTS AND MANAGERS" ) );
					sortedSicReferences.Add( new SICReference( "6531", "REAL ESTATE AGENTS AND MANAGERS (FOR OTHERS)" ) );
					sortedSicReferences.Add( new SICReference( "6532", "REAL ESTATE DEALERS (FOR THEIR OWN ACCOUNT)" ) );

					sortedSicReferences.Add( new SICReference( "6540", "TITLE ABSTRACT OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6541", "TITLE ABSTRACT OFFICES" ) );

					sortedSicReferences.Add( new SICReference( "6550", "LAND SUBDIVIDERS AND DEVELOPERS" ) );
					sortedSicReferences.Add( new SICReference( "6552", "LAND SUBDIVIDERS AND DEVELOPERS, EXCEPT CEMETERIES" ) );
					sortedSicReferences.Add( new SICReference( "6553", "CEMETERY SUBDIVIDERS AND DEVELOPERS" ) );

					sortedSicReferences.Add( new SICReference( "6710", "HOLDING OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6712", "OFFICES OF BANK HOLDING COMPANIES" ) );
					sortedSicReferences.Add( new SICReference( "6719", "OFFICES OF HOLDING COMPANIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6720", "INVESTMENT OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6722", "MANAGEMENT INVESTMENT OFFICES, OPEN-END" ) );
					sortedSicReferences.Add( new SICReference( "6726", "UNIT INVESTMENT TRUSTS, FACE-AMOUNT CERTIFICATE OFFICES, AND CLOS" ) );

					sortedSicReferences.Add( new SICReference( "6730", "TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6732", "EDUCATIONAL, RELIGIOUS, AND CHARITABLE TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6733", "TRUSTS, EXCEPT EDUCATIONAL, RELIGIOUS, AND CHARITABLE" ) );

					sortedSicReferences.Add( new SICReference( "6770", "BLANK CHECKS" ) );

					sortedSicReferences.Add( new SICReference( "6790", "MISCELLANEOUS INVESTING" ) );
					sortedSicReferences.Add( new SICReference( "6792", "OIL ROYALTY TRADERS" ) );
					sortedSicReferences.Add( new SICReference( "6794", "PATENT OWNERS AND LESSORS" ) );
					sortedSicReferences.Add( new SICReference( "6795", "MINERAL ROYALTY TRADERS" ) );
					sortedSicReferences.Add( new SICReference( "6798", "REAL ESTATE INVESTMENT TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6799", "INVESTORS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7000", "HOTELS, ROOMING HOUSES, CAMPS AND OTHER LODGING PLACES" ) );

					sortedSicReferences.Add( new SICReference( "7010", "HOTELS AND MOTELS" ) );
					sortedSicReferences.Add( new SICReference( "7011", "HOTELS AND MOTELS" ) );

					sortedSicReferences.Add( new SICReference( "7020", "ROOMING AND BOARDING HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "7021", "ROOMING AND BOARDING HOUSES" ) );

					sortedSicReferences.Add( new SICReference( "7030", "CAMPS AND RECREATIONAL VEHICLE PARKS" ) );
					sortedSicReferences.Add( new SICReference( "7032", "SPORTING AND RECREATIONAL CAMPS" ) );
					sortedSicReferences.Add( new SICReference( "7033", "RECREATIONAL VEHICLE PARKS AND CAMPSITES" ) );

					sortedSicReferences.Add( new SICReference( "7040", "ORGANIZATION HOTELS AND LODGING HOUSES, ON MEMBERSHIP BASIS" ) );
					sortedSicReferences.Add( new SICReference( "7041", "ORGANIZATION HOTELS AND LODGING HOUSES, ON MEMBERSHIP BASIS" ) );

					sortedSicReferences.Add( new SICReference( "7200", "PERSONAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7210", "LAUNDRY, CLEANING, AND GARMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7211", "POWER LAUNDRIES, FAMILY AND COMMERCIAL" ) );
					sortedSicReferences.Add( new SICReference( "7212", "GARMENT PRESSING, AND AGENTS FOR LAUNDRIES AND DRYCLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "7213", "LINEN SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "7215", "COIN-OPERATED LAUNDRIES AND DRYCLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7216", "DRYCLEANING PLANTS, EXCEPT RUG CLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7217", "CARPET AND UPHOLSTERY CLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7218", "INDUSTRIAL LAUNDERERS" ) );
					sortedSicReferences.Add( new SICReference( "7219", "LAUNDRY AND GARMENT SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7220", "PHOTOGRAPHIC STUDIOS, PORTRAIT" ) );
					sortedSicReferences.Add( new SICReference( "7221", "PHOTOGRAPHIC STUDIOS, PORTRAIT" ) );

					sortedSicReferences.Add( new SICReference( "7230", "BEAUTY SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7231", "BEAUTY SHOPS" ) );

					sortedSicReferences.Add( new SICReference( "7240", "BARBER SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7241", "BARBER SHOPS" ) );

					sortedSicReferences.Add( new SICReference( "7250", "SHOE REPAIR SHOPS AND SHOESHINE PARLORS" ) );
					sortedSicReferences.Add( new SICReference( "7251", "SHOE REPAIR SHOPS AND SHOESHINE PARLORS" ) );

					sortedSicReferences.Add( new SICReference( "7260", "FUNERAL SERVICE AND CREMATORIES" ) );
					sortedSicReferences.Add( new SICReference( "7261", "FUNERAL SERVICE AND CREMATORIES" ) );

					sortedSicReferences.Add( new SICReference( "7290", "MISCELLANEOUS PERSONAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7291", "TAX RETURN PREPARATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7299", "MISCELLANEOUS PERSONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7310", "ADVERTISING" ) );
					sortedSicReferences.Add( new SICReference( "7311", "ADVERTISING AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "7312", "OUTDOOR ADVERTISING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7313", "RADIO, TELEVISION, AND PUBLISHERS' ADVERTISING REPRESENTATIVES" ) );
					sortedSicReferences.Add( new SICReference( "7319", "ADVERTISING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7320", "CONSUMER CREDIT REPORTING AGENCIES, MERCANTILE REPORTING AGENCIES," ) );
					sortedSicReferences.Add( new SICReference( "7322", "ADJUSTMENT AND COLLECTION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7323", "CREDIT REPORTING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7330", "MAILING, REPRODUCTION, COMMERCIAL ART AND PHOTOGRAPHY, AND STENOGR" ) );
					sortedSicReferences.Add( new SICReference( "7331", "DIRECT MAIL ADVERTISING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7334", "PHOTOCOPYING AND DUPLICATING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7335", "COMMERCIAL PHOTOGRAPHY" ) );
					sortedSicReferences.Add( new SICReference( "7336", "COMMERCIAL ART AND GRAPHIC DESIGN" ) );
					sortedSicReferences.Add( new SICReference( "7338", "SECRETARIAL AND COURT REPORTING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7340", "SERVICES TO DWELLINGS AND OTHER BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "7342", "DISINFECTING AND PEST CONTROL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7349", "BUILDING CLEANING AND MAINTENANCE SERVICES, NOT ELSEWHERE CLASSIF" ) );

					sortedSicReferences.Add( new SICReference( "7350", "MISCELLANEOUS EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7352", "MEDICAL EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7353", "HEAVY CONSTRUCTION EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7359", "EQUIPMENT RENTAL AND LEASING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7360", "PERSONNEL SUPPLY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7361", "EMPLOYMENT AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "7363", "HELP SUPPLY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7370", "COMPUTER PROGRAMMING, DATA PROCESSING, AND OTHER COMPUTER RELATED" ) );
					sortedSicReferences.Add( new SICReference( "7371", "COMPUTER PROGRAMMING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7372", "PREPACKAGED SOFTWARE" ) );
					sortedSicReferences.Add( new SICReference( "7373", "COMPUTER INTEGRATED SYSTEMS DESIGN" ) );
					sortedSicReferences.Add( new SICReference( "7374", "COMPUTER PROCESSING AND DATA PREPARATION AND PROCESSING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7375", "INFORMATION RETRIEVAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7376", "COMPUTER FACILITIES MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7377", "COMPUTER RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7378", "COMPUTER MAINTENANCE AND REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7379", "COMPUTER RELATED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7380", "MISCELLANEOUS BUSINESS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7381", "DETECTIVE, GUARD, AND ARMORED CAR SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7382", "SECURITY SYSTEMS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7383", "NEWS SYNDICATES" ) );
					sortedSicReferences.Add( new SICReference( "7384", "PHOTOFINISHING LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "7385", "TELEPHONE INTERCONNECT SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "7389", "BUSINESS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7500", "AUTOMOTIVE REPAIR, SERVICES AND PARKING" ) );

					sortedSicReferences.Add( new SICReference( "7510", "AUTOMOTIVE RENTAL AND LEASING, WITHOUT DRIVERS" ) );
					sortedSicReferences.Add( new SICReference( "7513", "TRUCK RENTAL AND LEASING, WITHOUT DRIVERS" ) );
					sortedSicReferences.Add( new SICReference( "7514", "PASSENGER CAR RENTAL" ) );
					sortedSicReferences.Add( new SICReference( "7515", "PASSENGER CAR LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7519", "UTILITY TRAILER AND RECREATIONAL VEHICLE RENTAL" ) );

					sortedSicReferences.Add( new SICReference( "7520", "AUTOMOBILE PARKING" ) );
					sortedSicReferences.Add( new SICReference( "7521", "AUTOMOBILE PARKING" ) );

					sortedSicReferences.Add( new SICReference( "7530", "AUTOMOTIVE REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7532", "TOP, BODY, AND UPHOLSTERY REPAIR SHOPS AND PAINT SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7533", "AUTOMOTIVE EXHAUST SYSTEM REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7534", "TIRE RETREADING AND REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7536", "AUTOMOTIVE GLASS REPLACEMENT SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7537", "AUTOMOTIVE TRANSMISSION REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7538", "GENERAL AUTOMOTIVE REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7539", "AUTOMOTIVE REPAIR SHOPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7540", "AUTOMOTIVE SERVICES, EXCEPT REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7542", "CARWASHES" ) );
					sortedSicReferences.Add( new SICReference( "7549", "AUTOMOTIVE SERVICES, EXCEPT REPAIR AND CARWASHES" ) );

					sortedSicReferences.Add( new SICReference( "7600", "MISCELLANEOUS REPAIR SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7620", "ELECTRICAL REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7622", "RADIO AND TELEVISION REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7623", "REFRIGERATION AND AIR-CONDITIONING SERVICE AND REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7629", "ELECTRICAL AND ELECTRONIC REPAIR SHOPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7630", "WATCH, CLOCK, AND JEWELRY REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7631", "WATCH, CLOCK, AND JEWELRY REPAIR" ) );

					sortedSicReferences.Add( new SICReference( "7640", "REUPHOLSTERY AND FURNITURE REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7641", "REUPHOLSTERY AND FURNITURE REPAIR" ) );

					sortedSicReferences.Add( new SICReference( "7690", "MISCELLANEOUS REPAIR SHOPS AND RELATED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7692", "WELDING REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7694", "ARMATURE REWINDING SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7699", "REPAIR SHOPS AND RELATED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7810", "MOTION PICTURE PRODUCTION AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7812", "MOTION PICTURE AND VIDEO TAPE PRODUCTION" ) );
					sortedSicReferences.Add( new SICReference( "7819", "SERVICES ALLIED TO MOTION PICTURE PRODUCTION" ) );

					sortedSicReferences.Add( new SICReference( "7820", "MOTION PICTURE DISTRIBUTION AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7822", "MOTION PICTURE AND VIDEO TAPE DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "7829", "SERVICES ALLIED TO MOTION PICTURE DISTRIBUTION" ) );

					sortedSicReferences.Add( new SICReference( "7830", "MOTION PICTURE THEATERS" ) );
					sortedSicReferences.Add( new SICReference( "7832", "MOTION PICTURE THEATERS, EXCEPT DRIVE-IN" ) );
					sortedSicReferences.Add( new SICReference( "7833", "DRIVE-IN MOTION PICTURE THEATERS" ) );

					sortedSicReferences.Add( new SICReference( "7840", "VIDEO TAPE RENTAL" ) );
					sortedSicReferences.Add( new SICReference( "7841", "VIDEO TAPE RENTAL" ) );

					sortedSicReferences.Add( new SICReference( "7900", "AMUSEMENT AND RECREATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7910", "DANCE STUDIOS, SCHOOLS, AND HALLS" ) );
					sortedSicReferences.Add( new SICReference( "7911", "DANCE STUDIOS, SCHOOLS, AND HALLS" ) );

					sortedSicReferences.Add( new SICReference( "7920", "THEATRICAL PRODUCERS (EXCEPT MOTION PICTURE), BANDS, ORCHESTRAS, A" ) );
					sortedSicReferences.Add( new SICReference( "7922", "THEATRICAL PRODUCERS (EXCEPT MOTION PICTURE) AND MISCELLANEOUS TH" ) );
					sortedSicReferences.Add( new SICReference( "7929", "BANDS, ORCHESTRAS, ACTORS, AND OTHER ENTERTAINERS AND ENTERTAINME" ) );

					sortedSicReferences.Add( new SICReference( "7930", "BOWLING CENTERS" ) );
					sortedSicReferences.Add( new SICReference( "7933", "BOWLING CENTERS" ) );

					sortedSicReferences.Add( new SICReference( "7940", "COMMERCIAL SPORTS" ) );
					sortedSicReferences.Add( new SICReference( "7941", "PROFESSIONAL SPORTS CLUBS AND PROMOTERS" ) );
					sortedSicReferences.Add( new SICReference( "7948", "RACING, INCLUDING TRACK OPERATION" ) );

					sortedSicReferences.Add( new SICReference( "7990", "MISCELLANEOUS AMUSEMENT AND RECREATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7991", "PHYSICAL FITNESS FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "7992", "PUBLIC GOLF COURSES" ) );
					sortedSicReferences.Add( new SICReference( "7993", "COIN-OPERATED AMUSEMENT DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "7996", "AMUSEMENT PARKS" ) );
					sortedSicReferences.Add( new SICReference( "7997", "MEMBERSHIP SPORTS AND RECREATION CLUBS" ) );
					sortedSicReferences.Add( new SICReference( "7999", "AMUSEMENT AND RECREATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8000", "HEALTH SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8010", "OFFICES AND CLINICS OF DOCTORS OF MEDICINE" ) );
					sortedSicReferences.Add( new SICReference( "8011", "OFFICES AND CLINICS OF DOCTORS OF MEDICINE" ) );

					sortedSicReferences.Add( new SICReference( "8020", "OFFICES AND CLINICS OF DENTISTS" ) );
					sortedSicReferences.Add( new SICReference( "8021", "OFFICES AND CLINICS OF DENTISTS" ) );

					sortedSicReferences.Add( new SICReference( "8030", "OFFICES AND CLINICS OF DOCTORS OF OSTEOPATHY" ) );
					sortedSicReferences.Add( new SICReference( "8031", "OFFICES AND CLINICS OF DOCTORS OF OSTEOPATHY" ) );

					sortedSicReferences.Add( new SICReference( "8040", "OFFICES AND CLINICS OF OTHER HEALTH PRACTITIONERS" ) );
					sortedSicReferences.Add( new SICReference( "8041", "OFFICES AND CLINICS OF CHIROPRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "8042", "OFFICES AND CLINICS OF OPTOMETRISTS" ) );
					sortedSicReferences.Add( new SICReference( "8043", "OFFICES AND CLINICS OF PODIATRISTS" ) );
					sortedSicReferences.Add( new SICReference( "8049", "OFFICES AND CLINICS OF HEALTH PRACTITIONERS, NOT ELSEWHERE CLASSI" ) );

					sortedSicReferences.Add( new SICReference( "8050", "NURSING AND PERSONAL CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8051", "SKILLED NURSING CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8052", "INTERMEDIATE CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8059", "NURSING AND PERSONAL CARE FACILITIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8060", "HOSPITALS" ) );
					sortedSicReferences.Add( new SICReference( "8062", "GENERAL MEDICAL AND SURGICAL HOSPITALS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8063", "PSYCHIATRIC HOSPITALS" ) );
					sortedSicReferences.Add( new SICReference( "8069", "SPECIALTY HOSPITALS, EXCEPT PSYCHIATRIC" ) );

					sortedSicReferences.Add( new SICReference( "8070", "MEDICAL AND DENTAL LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "8071", "MEDICAL LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "8072", "DENTAL LABORATORIES" ) );

					sortedSicReferences.Add( new SICReference( "8080", "HOME HEALTH CARE SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8082", "HOME HEALTH CARE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8090", "MISCELLANEOUS HEALTH AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8092", "KIDNEY DIALYSIS CENTERS" ) );
					sortedSicReferences.Add( new SICReference( "8093", "SPECIALTY OUTPATIENT FACILITIES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8099", "HEALTH AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8110", "LEGAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8111", "LEGAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8200", "EDUCATIONAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8210", "ELEMENTARY AND SECONDARY SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8211", "ELEMENTARY AND SECONDARY SCHOOLS" ) );

					sortedSicReferences.Add( new SICReference( "8220", "COLLEGES, UNIVERSITIES, PROFESSIONAL SCHOOLS, AND JUNIOR COLLEGES" ) );
					sortedSicReferences.Add( new SICReference( "8221", "COLLEGES, UNIVERSITIES, AND PROFESSIONAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8222", "JUNIOR COLLEGES AND TECHNICAL INSTITUTES" ) );

					sortedSicReferences.Add( new SICReference( "8230", "LIBRARIES" ) );
					sortedSicReferences.Add( new SICReference( "8231", "LIBRARIES" ) );

					sortedSicReferences.Add( new SICReference( "8240", "VOCATIONAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8243", "DATA PROCESSING SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8244", "BUSINESS AND SECRETARIAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8249", "VOCATIONAL SCHOOLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8290", "SCHOOLS AND EDUCATIONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8299", "SCHOOLS AND EDUCATIONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8300", "SOCIAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8320", "INDIVIDUAL AND FAMILY SOCIAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8322", "INDIVIDUAL AND FAMILY SOCIAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8330", "JOB TRAINING AND VOCATIONAL REHABILITATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8331", "JOB TRAINING AND VOCATIONAL REHABILITATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8350", "CHILD DAY CARE SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8351", "CHILD DAY CARE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8360", "RESIDENTIAL CARE" ) );
					sortedSicReferences.Add( new SICReference( "8361", "RESIDENTIAL CARE" ) );

					sortedSicReferences.Add( new SICReference( "8390", "SOCIAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8399", "SOCIAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8410", "MUSEUMS AND ART GALLERIES" ) );
					sortedSicReferences.Add( new SICReference( "8412", "MUSEUMS AND ART GALLERIES" ) );

					sortedSicReferences.Add( new SICReference( "8420", "ARBORETA AND BOTANICAL OR ZOOLOGICAL GARDENS" ) );
					sortedSicReferences.Add( new SICReference( "8422", "ARBORETA AND BOTANICAL OR ZOOLOGICAL GARDENS" ) );

					sortedSicReferences.Add( new SICReference( "8600", "MEMBERSHIP ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8610", "BUSINESS ASSOCIATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8611", "BUSINESS ASSOCIATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8620", "PROFESSIONAL MEMBERSHIP ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8621", "PROFESSIONAL MEMBERSHIP ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8630", "LABOR UNIONS AND SIMILAR LABOR ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8631", "LABOR UNIONS AND SIMILAR LABOR ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8640", "CIVIC, SOCIAL, AND FRATERNAL ASSOCIATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8641", "CIVIC, SOCIAL, AND FRATERNAL ASSOCIATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8650", "POLITICAL ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8651", "POLITICAL ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8660", "RELIGIOUS ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8661", "RELIGIOUS ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8690", "MEMBERSHIP ORGANIZATIONS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8699", "MEMBERSHIP ORGANIZATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8700", "ENGINEERING, ACCOUNTING, RESEARCH, MANAGEMENT" ) );

					sortedSicReferences.Add( new SICReference( "8710", "ENGINEERING, ARCHITECTURAL, AND SURVEYING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8711", "ENGINEERING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8712", "ARCHITECTURAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8713", "SURVEYING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8720", "ACCOUNTING, AUDITING, AND BOOKKEEPING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8721", "ACCOUNTING, AUDITING, AND BOOKKEEPING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8730", "RESEARCH, DEVELOPMENT, AND TESTING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8731", "COMMERCIAL PHYSICAL AND BIOLOGICAL RESEARCH" ) );
					sortedSicReferences.Add( new SICReference( "8732", "COMMERCIAL ECONOMIC, SOCIOLOGICAL, AND EDUCATIONAL RESEARCH" ) );
					sortedSicReferences.Add( new SICReference( "8733", "NONCOMMERCIAL RESEARCH ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8734", "TESTING LABORATORIES" ) );

					sortedSicReferences.Add( new SICReference( "8740", "MANAGEMENT AND PUBLIC RELATIONS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8741", "MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8742", "MANAGEMENT CONSULTING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8743", "PUBLIC RELATIONS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8744", "FACILITIES SUPPORT MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8748", "BUSINESS CONSULTING SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8810", "PRIVATE HOUSEHOLDS" ) );
					sortedSicReferences.Add( new SICReference( "8811", "PRIVATE HOUSEHOLDS" ) );

					sortedSicReferences.Add( new SICReference( "8880", "AMERICAN DEPOSITARY RECEIPTS" ) );
					sortedSicReferences.Add( new SICReference( "8888", "FOREIGN GOVERNMENTS" ) );

					sortedSicReferences.Add( new SICReference( "8900", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8990", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8999", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9110", "EXECUTIVE OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "9111", "EXECUTIVE OFFICES" ) );

					sortedSicReferences.Add( new SICReference( "9120", "LEGISLATIVE BODIES" ) );
					sortedSicReferences.Add( new SICReference( "9121", "LEGISLATIVE BODIES" ) );

					sortedSicReferences.Add( new SICReference( "9130", "EXECUTIVE AND LEGISLATIVE OFFICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "9131", "EXECUTIVE AND LEGISLATIVE OFFICES COMBINED" ) );

					sortedSicReferences.Add( new SICReference( "9190", "GENERAL GOVERNMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "9199", "GENERAL GOVERNMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9210", "COURTS" ) );
					sortedSicReferences.Add( new SICReference( "9211", "COURTS" ) );

					sortedSicReferences.Add( new SICReference( "9220", "PUBLIC ORDER AND SAFETY" ) );
					sortedSicReferences.Add( new SICReference( "9221", "POLICE PROTECTION" ) );
					sortedSicReferences.Add( new SICReference( "9222", "LEGAL COUNSEL AND PROSECUTION" ) );
					sortedSicReferences.Add( new SICReference( "9223", "CORRECTIONAL INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "9224", "FIRE PROTECTION" ) );
					sortedSicReferences.Add( new SICReference( "9229", "PUBLIC ORDER AND SAFETY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9310", "PUBLIC FINANCE, TAXATION, AND MONETARY POLICY" ) );
					sortedSicReferences.Add( new SICReference( "9311", "PUBLIC FINANCE, TAXATION, AND MONETARY POLICY" ) );

					sortedSicReferences.Add( new SICReference( "9410", "ADMINISTRATION OF EDUCATIONAL PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9411", "ADMINISTRATION OF EDUCATIONAL PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9430", "ADMINISTRATION OF PUBLIC HEALTH PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9431", "ADMINISTRATION OF PUBLIC HEALTH PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9440", "ADMINISTRATION OF SOCIAL, HUMAN RESOURCE AND INCOME MAINTENANCE PR" ) );
					sortedSicReferences.Add( new SICReference( "9441", "ADMINISTRATION OF SOCIAL, HUMAN RESOURCE AND INCOME MAINTENANCE PR" ) );

					sortedSicReferences.Add( new SICReference( "9450", "ADMINISTRATION OF VETERANS' AFFAIRS, EXCEPT HEALTH AND INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "9451", "ADMINISTRATION OF VETERANS' AFFAIRS, EXCEPT HEALTH AND INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "9510", "ADMINISTRATION OF ENVIRONMENTAL QUALITY PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9511", "AIR AND WATER RESOURCE AND SOLID WASTE MANAGEMENT" ) );
					sortedSicReferences.Add( new SICReference( "9512", "LAND, MINERAL, WILDLIFE, AND FOREST CONSERVATION" ) );

					sortedSicReferences.Add( new SICReference( "9530", "ADMINISTRATION OF HOUSING AND URBAN DEVELOPMENT PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9531", "ADMINISTRATION OF HOUSING PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9532", "ADMINISTRATION OF URBAN PLANNING AND COMMUNITY AND RURAL DEVELOPM" ) );

					sortedSicReferences.Add( new SICReference( "9610", "ADMINISTRATION OF GENERAL ECONOMIC PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9611", "ADMINISTRATION OF GENERAL ECONOMIC PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9620", "REGULATION AND ADMINISTRATION OF TRANSPORTATION PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9621", "REGULATION AND ADMINISTRATION OF TRANSPORTATION PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9630", "REGULATION AND ADMINISTRATION OF COMMUNICATIONS, ELECTRIC, GAS, AN" ) );
					sortedSicReferences.Add( new SICReference( "9631", "REGULATION AND ADMINISTRATION OF COMMUNICATIONS, ELECTRIC, GAS, A" ) );

					sortedSicReferences.Add( new SICReference( "9640", "REGULATION OF AGRICULTURAL MARKETING AND COMMODITIES" ) );
					sortedSicReferences.Add( new SICReference( "9641", "REGULATION OF AGRICULTURAL MARKETING AND COMMODITIES" ) );

					sortedSicReferences.Add( new SICReference( "9650", "REGULATION, LICENSING, AND INSPECTION OF MISCELLANEOUS COMMERCIAL" ) );
					sortedSicReferences.Add( new SICReference( "9651", "REGULATION, LICENSING, AND INSPECTION OF MISCELLANEOUS COMMERCIAL" ) );

					sortedSicReferences.Add( new SICReference( "9660", "SPACE RESEARCH AND TECHNOLOGY" ) );
					sortedSicReferences.Add( new SICReference( "9661", "SPACE RESEARCH AND TECHNOLOGY" ) );

					sortedSicReferences.Add( new SICReference( "9710", "NATIONAL SECURITY" ) );
					sortedSicReferences.Add( new SICReference( "9711", "NATIONAL SECURITY" ) );

					sortedSicReferences.Add( new SICReference( "9720", "INTERNATIONAL AFFAIRS" ) );
					sortedSicReferences.Add( new SICReference( "9721", "INTERNATIONAL AFFAIRS" ) );

					sortedSicReferences.Add( new SICReference( "9990", "NONCLASSIFIABLE ESTABLISHMENTS" ) );
					sortedSicReferences.Add( new SICReference( "9995", "NON-OPERATING ESTABLISHMENTS" ) );

					//CEE: 2009-06-04 - Old SIC list committed by Chris Chew
					/*
					sortedSicReferences.Add( new SICReference( "0100", "AGRICULTURAL PRODUCTION-CROPS" ) );

					sortedSicReferences.Add( new SICReference( "0110", "CASH GRAINS" ) );
					sortedSicReferences.Add( new SICReference( "0111", "WHEAT" ) );
					sortedSicReferences.Add( new SICReference( "0112", "RICE" ) );
					sortedSicReferences.Add( new SICReference( "0115", "CORN" ) );
					sortedSicReferences.Add( new SICReference( "0116", "SOYBEANS" ) );
					sortedSicReferences.Add( new SICReference( "0119", "CASH GRAINS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0130", "FIELD CROPS, EXCEPT CASH GRAINS" ) );
					sortedSicReferences.Add( new SICReference( "0131", "COTTON" ) );
					sortedSicReferences.Add( new SICReference( "0132", "TOBACCO" ) );
					sortedSicReferences.Add( new SICReference( "0133", "SUGARCANE AND SUGAR BEETS" ) );
					sortedSicReferences.Add( new SICReference( "0134", "IRISH POTATOES" ) );
					sortedSicReferences.Add( new SICReference( "0139", "FIELD CROPS, EXCEPT CASH GRAINS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0160", "VEGETABLES AND MELONS" ) );
					sortedSicReferences.Add( new SICReference( "0161", "VEGETABLES AND MELONS" ) );

					sortedSicReferences.Add( new SICReference( "0170", "FRUITS AND TREE NUTS" ) );
					sortedSicReferences.Add( new SICReference( "0171", "BERRY CROPS" ) );
					sortedSicReferences.Add( new SICReference( "0172", "GRAPES" ) );
					sortedSicReferences.Add( new SICReference( "0173", "TREE NUTS" ) );
					sortedSicReferences.Add( new SICReference( "0174", "CITRUS FRUITS" ) );
					sortedSicReferences.Add( new SICReference( "0175", "DECIDUOUS TREE FRUITS" ) );
					sortedSicReferences.Add( new SICReference( "0179", "FRUITS AND TREE NUTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0180", "HORTICULTURAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0181", "ORNAMENTAL FLORICULTURE AND NURSERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "0182", "FOOD CROPS GROWN UNDER COVER" ) );

					sortedSicReferences.Add( new SICReference( "0190", "GENERAL FARMS, PRIMARILY CROP" ) );
					sortedSicReferences.Add( new SICReference( "0191", "GENERAL FARMS, PRIMARILY CROP" ) );

					sortedSicReferences.Add( new SICReference( "0200", "AGRICULTURAL PROD-LIVESTOCK AND ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0210", "LIVESTOCK, EXCEPT DAIRY AND POULTRY" ) );
					sortedSicReferences.Add( new SICReference( "0211", "BEEF CATTLE FEEDLOTS" ) );
					sortedSicReferences.Add( new SICReference( "0212", "BEEF CATTLE, EXCEPT FEEDLOTS" ) );
					sortedSicReferences.Add( new SICReference( "0213", "HOGS" ) );
					sortedSicReferences.Add( new SICReference( "0214", "SHEEP AND GOATS" ) );
					sortedSicReferences.Add( new SICReference( "0219", "GENERAL LIVESTOCK, EXCEPT DAIRY AND POULTRY" ) );

					sortedSicReferences.Add( new SICReference( "0240", "DAIRY FARMS" ) );
					sortedSicReferences.Add( new SICReference( "0241", "DAIRY FARMS" ) );

					sortedSicReferences.Add( new SICReference( "0250", "POULTRY AND EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0251", "BROILER, FRYER, AND ROASTER CHICKENS" ) );
					sortedSicReferences.Add( new SICReference( "0252", "CHICKEN EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0253", "TURKEYS AND TURKEY EGGS" ) );
					sortedSicReferences.Add( new SICReference( "0254", "POULTRY HATCHERIES" ) );
					sortedSicReferences.Add( new SICReference( "0259", "POULTRY AND EGGS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0270", "ANIMAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0271", "FUR-BEARING ANIMALS AND RABBITS" ) );
					sortedSicReferences.Add( new SICReference( "0272", "HORSES AND OTHER EQUINES" ) );
					sortedSicReferences.Add( new SICReference( "0273", "ANIMAL AQUACULTURE" ) );
					sortedSicReferences.Add( new SICReference( "0279", "ANIMAL SPECIALTIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "0290", "GENERAL FARMS, PRIMARILY LIVESTOCK AND ANIMAL SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "0291", "GENERAL FARMS, PRIMARILY LIVESTOCK AND ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0700", "AGRICULTURAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0710", "SOIL PREPARATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0711", "SOIL PREPARATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0720", "CROP SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0721", "CROP PLANTING, CULTIVATING, AND PROTECTING" ) );
					sortedSicReferences.Add( new SICReference( "0722", "CROP HARVESTING, PRIMARILY BY MACHINE" ) );
					sortedSicReferences.Add( new SICReference( "0723", "CROP PREPARATION SERVICES FOR MARKET, EXCEPT COTTON GINNING" ) );
					sortedSicReferences.Add( new SICReference( "0724", "COTTON GINNING" ) );

					sortedSicReferences.Add( new SICReference( "0740", "VETERINARY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0741", "VETERINARY SERVICES FOR LIVESTOCK" ) );
					sortedSicReferences.Add( new SICReference( "0742", "VETERINARY SERVICES FOR ANIMAL SPECIALTIES" ) );

					sortedSicReferences.Add( new SICReference( "0750", "ANIMAL SERVICES, EXCEPT VETERINARY" ) );
					sortedSicReferences.Add( new SICReference( "0751", "LIVESTOCK SERVICES, EXCEPT VETERINARY" ) );
					sortedSicReferences.Add( new SICReference( "0752", "ANIMAL SPECIALTY SERVICES, EXCEPT VETERINARY" ) );

					sortedSicReferences.Add( new SICReference( "0760", "FARM LABOR AND MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0761", "FARM LABOR CONTRACTORS AND CREW LEADERS" ) );
					sortedSicReferences.Add( new SICReference( "0762", "FARM MANAGEMENT SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0780", "LANDSCAPE AND HORTICULTURAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0781", "LANDSCAPE COUNSELING AND PLANNING" ) );
					sortedSicReferences.Add( new SICReference( "0782", "LAWN AND GARDEN SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0783", "ORNAMENTAL SHRUB AND TREE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0800", "FORESTRY" ) );

					sortedSicReferences.Add( new SICReference( "0810", "TIMBER TRACTS" ) );
					sortedSicReferences.Add( new SICReference( "0811", "TIMBER TRACTS" ) );

					sortedSicReferences.Add( new SICReference( "0830", "FOREST NURSERIES AND GATHERING OF FOREST PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "0831", "FOREST NURSERIES AND GATHERING OF FOREST PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "0850", "FORESTRY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "0851", "FORESTRY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "0900", "FISHING, HUNTING AND TRAPPING" ) );

					sortedSicReferences.Add( new SICReference( "0910", "COMMERCIAL FISHING" ) );
					sortedSicReferences.Add( new SICReference( "0912", "FINFISH" ) );
					sortedSicReferences.Add( new SICReference( "0913", "SHELLFISH" ) );
					sortedSicReferences.Add( new SICReference( "0919", "MISCELLANEOUS MARINE PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "0920", "FISH HATCHERIES AND PRESERVES" ) );
					sortedSicReferences.Add( new SICReference( "0921", "FISH HATCHERIES AND PRESERVES" ) );

					sortedSicReferences.Add( new SICReference( "0970", "HUNTING AND TRAPPING, AND GAME PROPAGATION" ) );
					sortedSicReferences.Add( new SICReference( "0971", "HUNTING AND TRAPPING, AND GAME PROPAGATION" ) );

					sortedSicReferences.Add( new SICReference( "1000", "METAL MINING" ) );

					sortedSicReferences.Add( new SICReference( "1010", "IRON ORES" ) );
					sortedSicReferences.Add( new SICReference( "1011", "IRON ORES" ) );

					sortedSicReferences.Add( new SICReference( "1020", "COPPER ORES" ) );
					sortedSicReferences.Add( new SICReference( "1021", "COPPER ORES" ) );

					sortedSicReferences.Add( new SICReference( "1030", "LEAD AND ZINC ORES" ) );
					sortedSicReferences.Add( new SICReference( "1031", "LEAD AND ZINC ORES" ) );

					sortedSicReferences.Add( new SICReference( "1040", "GOLD AND SILVER ORES" ) );
					sortedSicReferences.Add( new SICReference( "1041", "GOLD ORES" ) );
					sortedSicReferences.Add( new SICReference( "1044", "SILVER ORES" ) );

					sortedSicReferences.Add( new SICReference( "1060", "FERROALLOY ORES, EXCEPT VANADIUM" ) );
					sortedSicReferences.Add( new SICReference( "1061", "FERROALLOY ORES, EXCEPT VANADIUM" ) );

					sortedSicReferences.Add( new SICReference( "1080", "METAL MINING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1081", "METAL MINING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "1090", "MISCELLANEOUS METAL ORES" ) );
					sortedSicReferences.Add( new SICReference( "1094", "URANIUM-RADIUM-VANADIUM ORES" ) );
					sortedSicReferences.Add( new SICReference( "1099", "MISCELLANEOUS METAL ORES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1220", "BITUMINOUS COAL AND LIGNITE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1221", "BITUMINOUS COAL AND LIGNITE SURFACE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1222", "BITUMINOUS COAL UNDERGROUND MINING" ) );

					sortedSicReferences.Add( new SICReference( "1230", "ANTHRACITE MINING" ) );
					sortedSicReferences.Add( new SICReference( "1231", "ANTHRACITE MINING" ) );

					sortedSicReferences.Add( new SICReference( "1240", "COAL MINING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1241", "COAL MINING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "1310", "CRUDE PETROLEUM AND NATURAL GAS" ) );
					sortedSicReferences.Add( new SICReference( "1311", "CRUDE PETROLEUM AND NATURAL GAS" ) );

					sortedSicReferences.Add( new SICReference( "1320", "NATURAL GAS LIQUIDS" ) );
					sortedSicReferences.Add( new SICReference( "1321", "NATURAL GAS LIQUIDS" ) );

					sortedSicReferences.Add( new SICReference( "1380", "OIL AND GAS FIELD SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1381", "DRILLING OIL AND GAS WELLS" ) );
					sortedSicReferences.Add( new SICReference( "1382", "OIL AND GAS FIELD EXPLORATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "1389", "OIL AND GAS FIELD SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1400", "MINING AND QUARRYING OF NONMETALLIC MINERALS (NO FUELS)" ) );

					sortedSicReferences.Add( new SICReference( "1410", "DIMENSION STONE" ) );
					sortedSicReferences.Add( new SICReference( "1411", "DIMENSION STONE" ) );

					sortedSicReferences.Add( new SICReference( "1420", "CRUSHED AND BROKEN STONE, INCLUDING RIPRAP" ) );
					sortedSicReferences.Add( new SICReference( "1422", "CRUSHED AND BROKEN LIMESTONE" ) );
					sortedSicReferences.Add( new SICReference( "1423", "CRUSHED AND BROKEN GRANITE" ) );
					sortedSicReferences.Add( new SICReference( "1429", "CRUSHED AND BROKEN STONE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1440", "SAND AND GRAVEL" ) );
					sortedSicReferences.Add( new SICReference( "1442", "CONSTRUCTION SAND AND GRAVEL" ) );
					sortedSicReferences.Add( new SICReference( "1446", "INDUSTRIAL SAND" ) );

					sortedSicReferences.Add( new SICReference( "1450", "CLAY, CERAMIC, AND REFRACTORY MINERALS" ) );
					sortedSicReferences.Add( new SICReference( "1455", "KAOLIN AND BALL CLAY" ) );
					sortedSicReferences.Add( new SICReference( "1459", "CLAY, CERAMIC, AND REFRACTORY MINERALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1470", "CHEMICAL AND FERTILIZER MINERAL MINING" ) );
					sortedSicReferences.Add( new SICReference( "1474", "POTASH, SODA, AND BORATE MINERALS" ) );
					sortedSicReferences.Add( new SICReference( "1475", "PHOSPHATE ROCK" ) );
					sortedSicReferences.Add( new SICReference( "1479", "CHEMICAL AND FERTILIZER MINERAL MINING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1480", "NONMETALLIC MINERALS SERVICES, EXCEPT FUELS" ) );
					sortedSicReferences.Add( new SICReference( "1481", "NONMETALLIC MINERALS SERVICES, EXCEPT FUELS" ) );

					sortedSicReferences.Add( new SICReference( "1490", "MISCELLANEOUS NONMETALLIC MINERALS, EXCEPT FUELS" ) );
					sortedSicReferences.Add( new SICReference( "1499", "MISCELLANEOUS NONMETALLIC MINERALS, EXCEPT FUELS" ) );

					sortedSicReferences.Add( new SICReference( "1520", "GENERAL BUILDING CONTRACTORS-RESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "1521", "GENERAL CONTRACTORS-SINGLE-FAMILY HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "1522", "GENERAL CONTRACTORS-RESIDENTIAL BUILDINGS, OTHER THAN SINGLE-FAMI" ) );

					sortedSicReferences.Add( new SICReference( "1530", "OPERATIVE BUILDERS" ) );
					sortedSicReferences.Add( new SICReference( "1531", "OPERATIVE BUILDERS" ) );

					sortedSicReferences.Add( new SICReference( "1540", "GENERAL BUILDING CONTRACTORS-NONRESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "1541", "GENERAL CONTRACTORS-INDUSTRIAL BUILDINGS AND WAREHOUSES" ) );
					sortedSicReferences.Add( new SICReference( "1542", "GENERAL CONTRACTORS-NONRESIDENTIAL BUILDINGS, OTHER THAN INDUSTRI" ) );

					sortedSicReferences.Add( new SICReference( "1600", "HEAVY CONSTRUCTION OTHER THAN BLDG CONST - CONTRACTORS" ) );

					sortedSicReferences.Add( new SICReference( "1610", "HIGHWAY AND STREET CONSTRUCTION, EXCEPT ELEVATED HIGHWAYS" ) );
					sortedSicReferences.Add( new SICReference( "1611", "HIGHWAY AND STREET CONSTRUCTION, EXCEPT ELEVATED HIGHWAYS" ) );

					sortedSicReferences.Add( new SICReference( "1620", "HEAVY CONSTRUCTION, EXCEPT HIGHWAY AND STREET CONSTRUCTION" ) );
					sortedSicReferences.Add( new SICReference( "1622", "BRIDGE, TUNNEL, AND ELEVATED HIGHWAY CONSTRUCTION" ) );
					sortedSicReferences.Add( new SICReference( "1623", "WATER, SEWER, PIPELINE, AND COMMUNICATIONS AND POWER LINE CONSTRU" ) );
					sortedSicReferences.Add( new SICReference( "1629", "HEAVY CONSTRUCTION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1700", "CONSTRUCTION - SPECIAL TRADE CONTRACTORS" ) );

					sortedSicReferences.Add( new SICReference( "1710", "PLUMBING, HEATING AND AIR-CONDITIONING" ) );
					sortedSicReferences.Add( new SICReference( "1711", "PLUMBING, HEATING AND AIR-CONDITIONING" ) );

					sortedSicReferences.Add( new SICReference( "1720", "PAINTING AND PAPER HANGING" ) );
					sortedSicReferences.Add( new SICReference( "1721", "PAINTING AND PAPER HANGING" ) );

					sortedSicReferences.Add( new SICReference( "1730", "ELECTRICAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "1731", "ELECTRICAL WORK" ) );

					sortedSicReferences.Add( new SICReference( "1740", "MASONRY, STONEWORK, TILE SETTING, AND PLASTERING" ) );
					sortedSicReferences.Add( new SICReference( "1741", "MASONRY, STONE SETTING, AND OTHER STONE WORK" ) );
					sortedSicReferences.Add( new SICReference( "1742", "PLASTERING, DRYWALL, ACOUSTICAL, AND INSULATION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1743", "TERRAZZO, TILE, MARBLE, AND MOSAIC WORK" ) );

					sortedSicReferences.Add( new SICReference( "1750", "CARPENTRY AND FLOOR WORK" ) );
					sortedSicReferences.Add( new SICReference( "1751", "CARPENTRY WORK" ) );
					sortedSicReferences.Add( new SICReference( "1752", "FLOOR LAYING AND OTHER FLOOR WORK, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "1760", "ROOFING, SIDING, AND SHEET METAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "1761", "ROOFING, SIDING, AND SHEET METAL WORK" ) );

					sortedSicReferences.Add( new SICReference( "1770", "CONCRETE WORK" ) );
					sortedSicReferences.Add( new SICReference( "1771", "CONCRETE WORK" ) );

					sortedSicReferences.Add( new SICReference( "1780", "WATER WELL DRILLING" ) );
					sortedSicReferences.Add( new SICReference( "1781", "WATER WELL DRILLING" ) );

					sortedSicReferences.Add( new SICReference( "1790", "MISCELLANEOUS SPECIAL TRADE CONTRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "1791", "STRUCTURAL STEEL ERECTION" ) );
					sortedSicReferences.Add( new SICReference( "1793", "GLASS INSTALLATION, EXCEPT AUTOMOTIVE-CONTRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "1794", "EXCAVATION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1795", "WRECKING AND DEMOLITION WORK" ) );
					sortedSicReferences.Add( new SICReference( "1796", "INSTALLATION OR ERECTION OF BUILDING EQUIPMENT, NOT ELSEWHERE CLA" ) );
					sortedSicReferences.Add( new SICReference( "1799", "SPECIAL TRADE CONTRACTORS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2000", "FOOD AND KINDRED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2010", "MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2011", "MEAT PACKING PLANTS" ) );
					sortedSicReferences.Add( new SICReference( "2013", "SAUSAGES AND OTHER PREPARED MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2015", "POULTRY SLAUGHTERING AND PROCESSING" ) );

					sortedSicReferences.Add( new SICReference( "2020", "DAIRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2021", "CREAMERY BUTTER" ) );
					sortedSicReferences.Add( new SICReference( "2022", "NATURAL, PROCESSED, AND IMITATION CHEESE" ) );
					sortedSicReferences.Add( new SICReference( "2023", "DRY, CONDENSED, AND EVAPORATED DAIRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2024", "ICE CREAM AND FROZEN DESSERTS" ) );
					sortedSicReferences.Add( new SICReference( "2026", "FLUID MILK" ) );

					sortedSicReferences.Add( new SICReference( "2030", "CANNED, FROZEN, AND PRESERVED FRUITS, VEGETABLES, AND FOOD SPECIAL" ) );
					sortedSicReferences.Add( new SICReference( "2032", "CANNED SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "2033", "CANNED FRUITS, VEGETABLES, PRESERVES, JAMS, AND JELLIES" ) );
					sortedSicReferences.Add( new SICReference( "2034", "DRIED AND DEHYDRATED FRUITS, VEGETABLES, AND SOUP MIXES" ) );
					sortedSicReferences.Add( new SICReference( "2035", "PICKLED FRUITS AND VEGETABLES, VEGETABLE SAUCES AND SEASONINGS, A" ) );
					sortedSicReferences.Add( new SICReference( "2037", "FROZEN FRUITS, FRUIT JUICES, AND VEGETABLES" ) );
					sortedSicReferences.Add( new SICReference( "2038", "FROZEN SPECIALTIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2040", "GRAIN MILL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2041", "FLOUR AND OTHER GRAIN MILL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2043", "CEREAL BREAKFAST FOODS" ) );
					sortedSicReferences.Add( new SICReference( "2044", "RICE MILLING" ) );
					sortedSicReferences.Add( new SICReference( "2045", "PREPARED FLOUR MIXES AND DOUGHS" ) );
					sortedSicReferences.Add( new SICReference( "2046", "WET CORN MILLING" ) );
					sortedSicReferences.Add( new SICReference( "2047", "DOG AND CAT FOOD" ) );
					sortedSicReferences.Add( new SICReference( "2048", "PREPARED FEEDS AND FEED INGREDIENTS FOR ANIMALS AND FOWLS, EXCEPT" ) );

					sortedSicReferences.Add( new SICReference( "2050", "BAKERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2051", "BREAD AND OTHER BAKERY PRODUCTS, EXCEPT COOKIES AND CRACKERS" ) );
					sortedSicReferences.Add( new SICReference( "2052", "COOKIES AND CRACKERS" ) );
					sortedSicReferences.Add( new SICReference( "2053", "FROZEN BAKERY PRODUCTS, EXCEPT BREAD" ) );

					sortedSicReferences.Add( new SICReference( "2060", "SUGAR AND CONFECTIONERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2061", "CANE SUGAR, EXCEPT REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2062", "CANE SUGAR REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2063", "BEET SUGAR" ) );
					sortedSicReferences.Add( new SICReference( "2064", "CANDY AND OTHER CONFECTIONERY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2066", "CHOCOLATE AND COCOA PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2067", "CHEWING GUM" ) );
					sortedSicReferences.Add( new SICReference( "2068", "SALTED AND ROASTED NUTS AND SEEDS" ) );

					sortedSicReferences.Add( new SICReference( "2070", "FATS AND OILS" ) );
					sortedSicReferences.Add( new SICReference( "2074", "COTTONSEED OIL MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2075", "SOYBEAN OIL MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2076", "VEGETABLE OIL MILLS, EXCEPT CORN, COTTONSEED, AND SOYBEAN" ) );
					sortedSicReferences.Add( new SICReference( "2077", "ANIMAL AND MARINE FATS AND OILS" ) );
					sortedSicReferences.Add( new SICReference( "2079", "SHORTENING, TABLE OILS, MARGARINE, AND OTHER EDIBLE FATS AND OILS" ) );

					sortedSicReferences.Add( new SICReference( "2080", "BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "2082", "MALT BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "2083", "MALT" ) );
					sortedSicReferences.Add( new SICReference( "2084", "WINES, BRANDY, AND BRANDY SPIRITS" ) );
					sortedSicReferences.Add( new SICReference( "2085", "DISTILLED AND BLENDED LIQUORS" ) );
					sortedSicReferences.Add( new SICReference( "2086", "BOTTLED AND CANNED SOFT DRINKS AND CARBONATED WATERS" ) );
					sortedSicReferences.Add( new SICReference( "2087", "FLAVORING EXTRACTS AND FLAVORING SYRUPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2090", "MISCELLANEOUS FOOD PREPARATIONS AND KINDRED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2091", "CANNED AND CURED FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "2092", "PREPARED FRESH OR FROZEN FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "2095", "ROASTED COFFEE" ) );
					sortedSicReferences.Add( new SICReference( "2096", "POTATO CHIPS, CORN CHIPS, AND SIMILAR SNACKS" ) );
					sortedSicReferences.Add( new SICReference( "2097", "MANUFACTURED ICE" ) );
					sortedSicReferences.Add( new SICReference( "2098", "MACARONI, SPAGHETTI, VERMICELLI, AND NOODLES" ) );
					sortedSicReferences.Add( new SICReference( "2099", "FOOD PREPARATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2100", "TOBACCO PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2110", "CIGARETTES" ) );
					sortedSicReferences.Add( new SICReference( "2111", "CIGARETTES" ) );

					sortedSicReferences.Add( new SICReference( "2120", "CIGARS" ) );
					sortedSicReferences.Add( new SICReference( "2121", "CIGARS" ) );

					sortedSicReferences.Add( new SICReference( "2130", "CHEWING AND SMOKING TOBACCO AND SNUFF" ) );
					sortedSicReferences.Add( new SICReference( "2131", "CHEWING AND SMOKING TOBACCO AND SNUFF" ) );

					sortedSicReferences.Add( new SICReference( "2140", "TOBACCO STEMMING AND REDRYING" ) );
					sortedSicReferences.Add( new SICReference( "2141", "TOBACCO STEMMING AND REDRYING" ) );

					sortedSicReferences.Add( new SICReference( "2200", "TEXTILE MILL PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2210", "BROADWOVEN FABRIC MILLS, COTTON" ) );
					sortedSicReferences.Add( new SICReference( "2211", "BROADWOVEN FABRIC MILLS, COTTON" ) );

					sortedSicReferences.Add( new SICReference( "2220", "BROADWOVEN FABRIC MILLS, MANMADE FIBER AND SILK" ) );
					sortedSicReferences.Add( new SICReference( "2221", "BROADWOVEN FABRIC MILLS, MAN MADE FIBER AND SILK" ) );

					sortedSicReferences.Add( new SICReference( "2230", "BROADWOVEN FABRIC MILLS, WOOL (INCLUDING DYEING AND FINISHING)" ) );
					sortedSicReferences.Add( new SICReference( "2231", "BROADWOVEN FABRIC MILLS, WOOL (INCLUDING DYEING AND FINISHING)" ) );

					sortedSicReferences.Add( new SICReference( "2240", "NARROW FABRIC AND OTHER SMALLWARES MILLS: COTTON, WOOL, SILK, AND" ) );
					sortedSicReferences.Add( new SICReference( "2241", "NARROW FABRIC AND OTHER SMALLWARES MILLS: COTTON, WOOL, SILK, AND" ) );

					sortedSicReferences.Add( new SICReference( "2250", "KNITTING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2251", "WOMEN'S FULL-LENGTH AND KNEE-LENGTH HOSIERY, EXCEPT SOCKS" ) );
					sortedSicReferences.Add( new SICReference( "2252", "HOSIERY, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "2253", "KNIT OUTERWEAR MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2254", "KNIT UNDERWEAR AND NIGHTWEAR MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2257", "WEFT KNIT FABRIC MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2258", "LACE AND WARP KNIT FABRIC MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2259", "KNITTING MILLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2260", "DYEING AND FINISHING TEXTILES, EXCEPT WOOL FABRICS AND KNIT GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2261", "FINISHERS OF BROADWOVEN FABRICS OF COTTON" ) );
					sortedSicReferences.Add( new SICReference( "2262", "FINISHERS OF BROADWOVEN FABRICS OF MANMADE FIBER AND SILK" ) );
					sortedSicReferences.Add( new SICReference( "2269", "FINISHERS OF TEXTILES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2270", "CARPETS AND RUGS" ) );
					sortedSicReferences.Add( new SICReference( "2273", "CARPETS AND RUGS" ) );

					sortedSicReferences.Add( new SICReference( "2280", "YARN AND THREAD MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2281", "YARN SPINNING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2282", "ACETATE FILAMENT YARN: THROWING, TWISTING, WINDING, OR SPOOLING" ) );
					sortedSicReferences.Add( new SICReference( "2284", "THREAD MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2290", "MISCELLANEOUS TEXTILE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2295", "COATED FABRICS, NOT RUBBERIZED" ) );
					sortedSicReferences.Add( new SICReference( "2296", "TIRE CORD AND FABRICS" ) );
					sortedSicReferences.Add( new SICReference( "2297", "NONWOVEN FABRICS" ) );
					sortedSicReferences.Add( new SICReference( "2298", "CORDAGE AND TWINE" ) );
					sortedSicReferences.Add( new SICReference( "2299", "TEXTILE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2300", "APPAREL AND OTHER FINISHD PRODS OF FABRICS AND SIMILAR MATL" ) );

					sortedSicReferences.Add( new SICReference( "2310", "MEN'S AND BOYS' SUITS, COATS, AND OVERCOATS" ) );
					sortedSicReferences.Add( new SICReference( "2311", "MEN'S AND BOYS' SUITS, COATS, AND OVERCOATS" ) );

					sortedSicReferences.Add( new SICReference( "2320", "MEN'S AND BOYS' FURNISHINGS, WORK CLOTHING, AND ALLIED GARMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2321", "MEN'S AND BOYS' SHIRTS, EXCEPT WORK SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2322", "MEN'S AND BOYS' UNDERWEAR AND NIGHTWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2323", "MEN'S AND BOYS' NECKWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2325", "MEN'S AND BOYS' SEPARATE TROUSERS AND SLACKS" ) );
					sortedSicReferences.Add( new SICReference( "2326", "MEN'S AND BOYS' WORK CLOTHING" ) );
					sortedSicReferences.Add( new SICReference( "2329", "MEN'S AND BOYS' CLOTHING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2330", "WOMEN'S, MISSES', AND JUNIORS' OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2331", "WOMEN'S, MISSES', AND JUNIORS' BLOUSES AND SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2335", "WOMEN'S, MISSES', AND JUNIORS' DRESSES" ) );
					sortedSicReferences.Add( new SICReference( "2337", "WOMEN'S, MISSES', AND JUNIORS' SUITS, SKIRTS, AND COATS" ) );
					sortedSicReferences.Add( new SICReference( "2339", "WOMEN'S, MISSES', AND JUNIORS' OUTERWEAR, NOT ELSEWHERE CLASSIFIE" ) );

					sortedSicReferences.Add( new SICReference( "2340", "WOMEN'S, MISSES', CHILDREN'S, AND INFANTS' UNDERGARMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2341", "WOMEN'S, MISSES', CHILDREN'S, AND INFANTS' UNDERWEAR AND NIGHTWEA" ) );
					sortedSicReferences.Add( new SICReference( "2342", "BRASSIERES, GIRDLES, AND ALLIED GARMENTS" ) );

					sortedSicReferences.Add( new SICReference( "2350", "HATS, CAPS, AND MILLINERY" ) );
					sortedSicReferences.Add( new SICReference( "2353", "HATS, CAPS, AND MILLINERY" ) );

					sortedSicReferences.Add( new SICReference( "2360", "GIRLS', CHILDREN'S, AND INFANTS' OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2361", "GIRLS', CHILDREN'S, AND INFANTS' DRESSES, BLOUSES, AND SHIRTS" ) );
					sortedSicReferences.Add( new SICReference( "2369", "GIRLS', CHILDREN'S, AND INFANTS' OUTERWEAR, NOT ELSEWHERE CLASSIF" ) );

					sortedSicReferences.Add( new SICReference( "2370", "FUR GOODS" ) );
					sortedSicReferences.Add( new SICReference( "2371", "FUR GOODS" ) );

					sortedSicReferences.Add( new SICReference( "2380", "MISCELLANEOUS APPAREL AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "2381", "DRESS AND WORK GLOVES, EXCEPT KNIT AND ALL-LEATHER" ) );
					sortedSicReferences.Add( new SICReference( "2384", "ROBES AND DRESSING GOWNS" ) );
					sortedSicReferences.Add( new SICReference( "2385", "WATERPROOF OUTERWEAR" ) );
					sortedSicReferences.Add( new SICReference( "2386", "LEATHER AND SHEEP-LINED CLOTHING" ) );
					sortedSicReferences.Add( new SICReference( "2387", "APPAREL BELTS" ) );
					sortedSicReferences.Add( new SICReference( "2389", "APPAREL AND ACCESSORIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2390", "MISCELLANEOUS FABRICATED TEXTILE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2391", "CURTAINS AND DRAPERIES" ) );
					sortedSicReferences.Add( new SICReference( "2392", "HOUSEFURNISHINGS, EXCEPT CURTAINS AND DRAPERIES" ) );
					sortedSicReferences.Add( new SICReference( "2393", "TEXTILE BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2394", "CANVAS AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2395", "PLEATING, DECORATIVE AND NOVELTY STITCHING, AND TUCKING FOR THE T" ) );
					sortedSicReferences.Add( new SICReference( "2396", "AUTOMOTIVE TRIMMINGS, APPAREL FINDINGS, AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2397", "SCHIFFLI MACHINE EMBROIDERIES" ) );
					sortedSicReferences.Add( new SICReference( "2399", "FABRICATED TEXTILE PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2400", "LUMBER AND WOOD PRODUCTS (NO FURNITURE)" ) );

					sortedSicReferences.Add( new SICReference( "2410", "LOGGING" ) );
					sortedSicReferences.Add( new SICReference( "2411", "LOGGING" ) );

					sortedSicReferences.Add( new SICReference( "2420", "SAWMILLS AND PLANING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2421", "SAWMILLS AND PLANTING MILLS, GENERAL" ) );
					sortedSicReferences.Add( new SICReference( "2426", "HARDWOOD DIMENSION AND FLOORING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2429", "SPECIAL PRODUCT SAWMILLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2430", "MILLWOOD, VENEER, PLYWOOD, AND STRUCTURAL WOOD MEMBERS" ) );
					sortedSicReferences.Add( new SICReference( "2431", "MILLWORK" ) );
					sortedSicReferences.Add( new SICReference( "2434", "WOOD KITCHEN CABINETS" ) );
					sortedSicReferences.Add( new SICReference( "2435", "HARDWOOD VENEER AND PLYWOOD" ) );
					sortedSicReferences.Add( new SICReference( "2436", "SOFTWOOD VENEER AND PLYWOOD" ) );
					sortedSicReferences.Add( new SICReference( "2439", "STRUCTURAL WOOD MEMBERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2440", "WOOD CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "2441", "NAILED AND LOCK CORNER WOOD BOXES AND SHOOK" ) );
					sortedSicReferences.Add( new SICReference( "2448", "WOOD PALLETS AND SKIDS" ) );
					sortedSicReferences.Add( new SICReference( "2449", "WOOD CONTAINERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2450", "WOOD BUILDINGS AND MOBILE HOMES" ) );
					sortedSicReferences.Add( new SICReference( "2451", "MOBILE HOMES" ) );
					sortedSicReferences.Add( new SICReference( "2452", "PREFABRICATED WOOD BUILDINGS AND COMPONENTS" ) );

					sortedSicReferences.Add( new SICReference( "2490", "MISCELLANEOUS WOOD PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2491", "WOOD PRESERVING" ) );
					sortedSicReferences.Add( new SICReference( "2493", "RECONSTITUTED WOOD PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2499", "WOOD PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2510", "HOUSEHOLD FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2511", "WOOD HOUSEHOLD FURNITURE, EXCEPT UPHOLSTERED" ) );
					sortedSicReferences.Add( new SICReference( "2512", "WOOD HOUSEHOLD FURNITURE, UPHOLSTERED" ) );
					sortedSicReferences.Add( new SICReference( "2514", "METAL HOUSEHOLD FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2515", "MATTRESSES, FOUNDATIONS, AND CONVERTIBLE BEDS" ) );
					sortedSicReferences.Add( new SICReference( "2517", "WOOD TELEVISION, RADIO, PHONOGRAPH, AND SEWING MACHINE CABINETS" ) );
					sortedSicReferences.Add( new SICReference( "2519", "HOUSEHOLD FURNITURE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2520", "OFFICE FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2521", "WOOD OFFICE FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2522", "OFFICE FURNITURE, EXCEPT WOOD" ) );

					sortedSicReferences.Add( new SICReference( "2530", "PUBLIC BUILDING AND RELATED FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "2531", "PUBLIC BUILDING AND RELATED FURNITURE" ) );

					sortedSicReferences.Add( new SICReference( "2540", "PARTITIONS, SHELVING, LOCKERS, AND OFFICE AND STORE FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "2541", "WOOD OFFICE AND STORE FIXTURES, PARTITIONS, SHELVING, AND LOCKERS" ) );
					sortedSicReferences.Add( new SICReference( "2542", "OFFICE AND STORE FIXTURES, PARTITIONS, SHELVING, AND LOCKERS, EXC" ) );

					sortedSicReferences.Add( new SICReference( "2590", "MISCELLANEOUS FURNITURE AND FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "2591", "DRAPERY HARDWARE AND WINDOW BLINDS AND SHADES" ) );
					sortedSicReferences.Add( new SICReference( "2599", "FURNITURE AND FIXTURES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2600", "PAPERS AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2610", "PULP MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2611", "PULP MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2620", "PAPER MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2621", "PAPER MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2630", "PAPERBOARD MILLS" ) );
					sortedSicReferences.Add( new SICReference( "2631", "PAPERBOARD MILLS" ) );

					sortedSicReferences.Add( new SICReference( "2650", "PAPERBOARD CONTAINERS AND BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2652", "SETUP PAPERBOARD BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2653", "CORRUGATED AND SOLID FIBER BOXES" ) );
					sortedSicReferences.Add( new SICReference( "2655", "FIBER CANS, TUBES, DRUMS, AND SIMILAR PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2656", "SANITARY FOOD CONTAINERS, EXCEPT FOLDING" ) );
					sortedSicReferences.Add( new SICReference( "2657", "FOLDING PAPERBOARD BOXES, INCLUDING SANITARY" ) );

					sortedSicReferences.Add( new SICReference( "2670", "CONVERTED PAPER AND PAPERBOARD PRODUCTS, EXCEPT CONTAINERS AND BOX" ) );
					sortedSicReferences.Add( new SICReference( "2671", "PACKAGING PAPER AND PLASTICS FILM, COATED AND LAMINATED" ) );
					sortedSicReferences.Add( new SICReference( "2672", "COATED AND LAMINATED PAPER, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "2673", "PLASTICS, FOIL, AND COATED PAPER BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2674", "UNCOATED PAPER AND MULTIWALL BAGS" ) );
					sortedSicReferences.Add( new SICReference( "2675", "DIE-CUT PAPER AND PAPERBOARD AND CARDBOARD" ) );
					sortedSicReferences.Add( new SICReference( "2676", "SANITARY PAPER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2677", "ENVELOPES" ) );
					sortedSicReferences.Add( new SICReference( "2678", "STATIONERY, TABLETS, AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2679", "CONVERTED PAPER AND PAPERBOARD PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2710", "NEWSPAPERS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2711", "NEWSPAPERS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2720", "PERIODICALS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2721", "PERIODICALS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2730", "BOOKS" ) );
					sortedSicReferences.Add( new SICReference( "2731", "BOOKS: PUBLISHING, OR PUBLISHING AND PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2732", "BOOK PRINTING" ) );

					sortedSicReferences.Add( new SICReference( "2740", "MISCELLANEOUS PUBLISHING" ) );
					sortedSicReferences.Add( new SICReference( "2741", "MISCELLANEOUS PUBLISHING" ) );

					sortedSicReferences.Add( new SICReference( "2750", "COMMERCIAL PRINTING" ) );
					sortedSicReferences.Add( new SICReference( "2752", "COMMERCIAL PRINTING, LITHOGRAPHIC" ) );
					sortedSicReferences.Add( new SICReference( "2754", "COMMERCIAL PRINTING, GRAVURE" ) );
					sortedSicReferences.Add( new SICReference( "2759", "COMMERCIAL PRINTING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2760", "MANIFOLD BUSINESS FORMS" ) );
					sortedSicReferences.Add( new SICReference( "2761", "MANIFOLD BUSINESS FORMS" ) );


					sortedSicReferences.Add( new SICReference( "2770", "GREETING CARDS" ) );
					sortedSicReferences.Add( new SICReference( "2771", "GREETING CARDS" ) );

					sortedSicReferences.Add( new SICReference( "2780", "BLANKBOOKS, LOOSELEAF BINDERS, AND BOOKBINDING AND RELATED WORK" ) );
					sortedSicReferences.Add( new SICReference( "2782", "BLANKBOOKS, LOOSELEAF BINDERS AND DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "2789", "BOOKBINDING AND RELATED WORK" ) );

					sortedSicReferences.Add( new SICReference( "2790", "SERVICE INDUSTRIES FOR THE PRINTING TRADE" ) );
					sortedSicReferences.Add( new SICReference( "2791", "TYPESETTING" ) );
					sortedSicReferences.Add( new SICReference( "2796", "PLATEMAKING AND RELATED SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "2800", "CHEMICALS AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2810", "INDUSTRIAL INORGANIC CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2812", "ALKALIES AND CHLORINE" ) );
					sortedSicReferences.Add( new SICReference( "2813", "INDUSTRIAL GASES" ) );
					sortedSicReferences.Add( new SICReference( "2816", "INORGANIC PIGMENTS" ) );
					sortedSicReferences.Add( new SICReference( "2819", "INDUSTRIAL INORGANIC CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2820", "PLASTICS MATERIALS AND SYNTHETIC RESINS, SYNTHETIC RUBBER, CELLULO" ) );
					sortedSicReferences.Add( new SICReference( "2821", "PLASTICS MATERIALS, SYNTHETIC RESINS, AND NONVULCANIZABLE ELASTOM" ) );
					sortedSicReferences.Add( new SICReference( "2822", "SYNTHETIC RUBBER (VULCANIZABLE ELASTOMERS)" ) );
					sortedSicReferences.Add( new SICReference( "2823", "CELLULOSIC MANMADE FIBERS" ) );
					sortedSicReferences.Add( new SICReference( "2824", "MANMADE ORGANIC FIBERS, EXCEPT CELLULOSIC" ) );

					sortedSicReferences.Add( new SICReference( "2830", "DRUGS" ) );
					sortedSicReferences.Add( new SICReference( "2833", "MEDICINAL CHEMICALS AND BOTANICAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2834", "PHARMACEUTICAL PREPARATIONS" ) );
					sortedSicReferences.Add( new SICReference( "2835", "IN VITRO AND IN VIVO DIAGNOSTIC SUBSTANCES" ) );
					sortedSicReferences.Add( new SICReference( "2836", "BIOLOGICAL PRODUCTS, EXCEPT DIAGNOSTIC SUBSTANCES" ) );

					sortedSicReferences.Add( new SICReference( "2840", "SOAP, DETERGENTS, AND CLEANING PREPARATIONS; PERFUMES, COSMETICS," ) );
					sortedSicReferences.Add( new SICReference( "2841", "SOAP AND OTHER DETERGENTS, EXCEPT SPECIALTY CLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "2842", "SPECIALTY CLEANING, POLISHING, AND SANITATION PREPARATIONS" ) );
					sortedSicReferences.Add( new SICReference( "2843", "SURFACE ACTIVE AGENTS, FINISHING AGENTS, SULFONATED OILS, AND ASS" ) );
					sortedSicReferences.Add( new SICReference( "2844", "PERFUMES, COSMETICS, AND OTHER TOILET PREPARATIONS" ) );

					sortedSicReferences.Add( new SICReference( "2850", "PAINTS, VARNISHES, LACQUERS, ENAMELS, AND ALLIED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2851", "PAINTS, VARNISHES, LACQUERS, ENAMELS, AND ALLIED PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "2860", "INDUSTRIAL ORGANIC CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2861", "GUM AND WOOD CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2865", "CYCLIC ORGANIC CRUDES AND INTERMEDIATES, AND ORGANIC DYES AND PIG" ) );
					sortedSicReferences.Add( new SICReference( "2869", "INDUSTRIAL ORGANIC CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2870", "AGRICULTURAL CHEMICALS" ) );
					sortedSicReferences.Add( new SICReference( "2873", "NITROGENOUS FERTILIZERS" ) );
					sortedSicReferences.Add( new SICReference( "2874", "PHOSPHATIC FERTILIZERS" ) );
					sortedSicReferences.Add( new SICReference( "2875", "FERTILIZERS, MIXING ONLY" ) );
					sortedSicReferences.Add( new SICReference( "2879", "PESTICIDES AND AGRICULTURAL CHEMICALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2890", "MISCELLANEOUS CHEMICAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "2891", "ADHESIVES AND SEALANTS" ) );
					sortedSicReferences.Add( new SICReference( "2892", "EXPLOSIVES" ) );
					sortedSicReferences.Add( new SICReference( "2893", "PRINTING INK" ) );
					sortedSicReferences.Add( new SICReference( "2895", "CARBON BLACK" ) );
					sortedSicReferences.Add( new SICReference( "2899", "CHEMICALS AND CHEMICAL PREPARATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "2910", "PETROLEUM REFINING" ) );
					sortedSicReferences.Add( new SICReference( "2911", "PETROLEUM REFINING" ) );

					sortedSicReferences.Add( new SICReference( "2950", "ASPHALT PAVING AND ROOFING MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "2951", "ASPHALT PAVING MIXTURES AND BLOCKS" ) );
					sortedSicReferences.Add( new SICReference( "2952", "ASPHALT FELTS AND COATINGS" ) );

					sortedSicReferences.Add( new SICReference( "2990", "MISCELLANEOUS PRODUCTS OF PETROLEUM AND COAL" ) );
					sortedSicReferences.Add( new SICReference( "2992", "LUBRICATING OILS AND GREASES" ) );
					sortedSicReferences.Add( new SICReference( "2999", "PRODUCTS OF PETROLEUM AND COAL, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3010", "TIRES AND INNER TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3011", "TIRES AND INNER TUBES" ) );

					sortedSicReferences.Add( new SICReference( "3020", "RUBBER AND PLASTICS FOOTWEAR" ) );
					sortedSicReferences.Add( new SICReference( "3021", "RUBBER AND PLASTICS FOOTWEAR" ) );

					sortedSicReferences.Add( new SICReference( "3050", "GASKETS, PACKING, AND SEALING DEVICES AND RUBBER AND PLASTICS HOSE" ) );
					sortedSicReferences.Add( new SICReference( "3052", "RUBBER AND PLASTICS HOSE AND BELTING" ) );
					sortedSicReferences.Add( new SICReference( "3053", "GASKETS, PACKING, AND SEALING DEVICES" ) );

					sortedSicReferences.Add( new SICReference( "3060", "FABRICATED RUBBER PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3061", "MOLDED, EXTRUDED, AND LATHE-CUT MECHANICAL RUBBER GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3069", "FABRICATED RUBBER PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3080", "MISCELLANEOUS PLASTICS PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3081", "UNSUPPORTED PLASTICS FILM AND SHEET" ) );
					sortedSicReferences.Add( new SICReference( "3082", "UNSUPPORTED PLASTICS PROFILE SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "3083", "LAMINATED PLASTICS PLATE, SHEET, AND PROFILE SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "3084", "PLASTICS PIPE" ) );
					sortedSicReferences.Add( new SICReference( "3085", "PLASTICS BOTTLES" ) );
					sortedSicReferences.Add( new SICReference( "3086", "PLASTICS FOAM PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3087", "CUSTOM COMPOUNDING OF PURCHASED PLASTICS RESINS" ) );
					sortedSicReferences.Add( new SICReference( "3088", "PLASTICS PLUMBING FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "3089", "PLASTICS PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3100", "LEATHER AND LEATHER PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3110", "LEATHER TANNING AND FINISHING" ) );
					sortedSicReferences.Add( new SICReference( "3111", "LEATHER TANNING AND FINISHING" ) );

					sortedSicReferences.Add( new SICReference( "3130", "BOOT AND SHOE CUT STOCK AND FINDINGS" ) );
					sortedSicReferences.Add( new SICReference( "3131", "BOOT AND SHOE CUT STOCK AND FINDINGS" ) );

					sortedSicReferences.Add( new SICReference( "3140", "FOOTWEAR, EXCEPT RUBBER" ) );
					sortedSicReferences.Add( new SICReference( "3142", "HOUSE SLIPPERS" ) );
					sortedSicReferences.Add( new SICReference( "3143", "MEN'S FOOTWEAR, EXCEPT ATHLETIC" ) );
					sortedSicReferences.Add( new SICReference( "3144", "WOMEN'S FOOTWEAR, EXCEPT ATHLETIC" ) );
					sortedSicReferences.Add( new SICReference( "3149", "FOOTWEAR, EXCEPT RUBBER, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3150", "LEATHER GLOVES AND MITTENS" ) );
					sortedSicReferences.Add( new SICReference( "3151", "LEATHER GLOVES AND MITTENS" ) );

					sortedSicReferences.Add( new SICReference( "3160", "LUGGAGE" ) );
					sortedSicReferences.Add( new SICReference( "3161", "LUGGAGE" ) );

					sortedSicReferences.Add( new SICReference( "3170", "HANDBAGS AND OTHER PERSONAL LEATHER GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3171", "WOMEN'S HANDBAGS AND PURSES" ) );
					sortedSicReferences.Add( new SICReference( "3172", "PERSONAL LEATHER GOODS, EXCEPT WOMEN'S HANDBAGS AND PURSES" ) );

					sortedSicReferences.Add( new SICReference( "3190", "LEATHER GOODS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3199", "LEATHER GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3210", "FLAT GLASS" ) );
					sortedSicReferences.Add( new SICReference( "3211", "FLAT GLASS" ) );

					sortedSicReferences.Add( new SICReference( "3220", "GLASS AND GLASSWARE, PRESSED OR BLOWN" ) );
					sortedSicReferences.Add( new SICReference( "3221", "GLASS CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "3229", "PRESSED AND BLOWN GLASS AND GLASSWARE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3230", "GLASS PRODUCTS, MADE OF PURCHASED GLASS" ) );
					sortedSicReferences.Add( new SICReference( "3231", "GLASS PRODUCTS, MADE OF PURCHASED GLASS" ) );

					sortedSicReferences.Add( new SICReference( "3240", "CEMENT, HYDRAULIC" ) );
					sortedSicReferences.Add( new SICReference( "3241", "CEMENT, HYDRAULIC" ) );

					sortedSicReferences.Add( new SICReference( "3250", "STRUCTURAL CLAY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3251", "BRICK AND STRUCTURAL CLAY TILE" ) );
					sortedSicReferences.Add( new SICReference( "3253", "CERAMIC WALL AND FLOOR TILE" ) );
					sortedSicReferences.Add( new SICReference( "3255", "CLAY REFRACTORIES" ) );
					sortedSicReferences.Add( new SICReference( "3259", "STRUCTURAL CLAY PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3260", "POTTERY AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3261", "VITREOUS CHINA PLUMBING FIXTURES AND CHINA AND EARTHENWARE FITTIN" ) );
					sortedSicReferences.Add( new SICReference( "3262", "VITREOUS CHINA TABLE AND KITCHEN ARTICLES" ) );
					sortedSicReferences.Add( new SICReference( "3263", "FINE EARTHENWARE (WHITEWARE) TABLE AND KITCHEN ARTICLES" ) );
					sortedSicReferences.Add( new SICReference( "3264", "PORCELAIN ELECTRICAL SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3269", "POTTERY PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3270", "CONCRETE, GYPSUM, AND PLASTER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3271", "CONCRETE BLOCK AND BRICK" ) );
					sortedSicReferences.Add( new SICReference( "3272", "CONCRETE PRODUCTS, EXCEPT BLOCK AND BRICK" ) );
					sortedSicReferences.Add( new SICReference( "3273", "READY-MIXED CONCRETE" ) );
					sortedSicReferences.Add( new SICReference( "3274", "LIME" ) );
					sortedSicReferences.Add( new SICReference( "3275", "GYPSUM PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3280", "CUT STONE AND STONE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3281", "CUT STONE AND STONE PRODUCTS" ) );

					sortedSicReferences.Add( new SICReference( "3290", "ABRASIVE, ASBESTOS, AND MISCELLANEOUS NONMETALLIC MINERAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3291", "ABRASIVE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3292", "ASBESTOS PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3295", "MINERALS AND EARTHS, GROUND OR OTHERWISE TREATED" ) );
					sortedSicReferences.Add( new SICReference( "3296", "MINERAL WOOL" ) );
					sortedSicReferences.Add( new SICReference( "3297", "NONCLAY REFRACTORIES" ) );
					sortedSicReferences.Add( new SICReference( "3299", "NONMETALLIC MINERAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3310", "STEEL WORKS, BLAST FURNACES, AND ROLLING AND FINISHING MILLS" ) );
					sortedSicReferences.Add( new SICReference( "3312", "STEEL WORKS, BLAST FURNACES (INCLUDING COKE OVENS), AND ROLLING M" ) );
					sortedSicReferences.Add( new SICReference( "3313", "ELECTROMETALLURGICAL PRODUCTS, EXCEPT STEEL" ) );
					sortedSicReferences.Add( new SICReference( "3315", "STEEL WIREDRAWING AND STEEL NAILS AND SPIKES" ) );
					sortedSicReferences.Add( new SICReference( "3316", "COLD-ROLLED STEEL SHEET, STRIP, AND BARS" ) );
					sortedSicReferences.Add( new SICReference( "3317", "STEEL PIPE AND TUBES" ) );

					sortedSicReferences.Add( new SICReference( "3320", "IRON AND STEEL FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3321", "GRAY AND DUCTILE IRON FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3322", "MALLEABLE IRON FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3324", "STEEL INVESTMENT FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3325", "STEEL FOUNDRIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3330", "PRIMARY SMELTING AND REFINING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3331", "PRIMARY SMELTING AND REFINING OF COPPER" ) );
					sortedSicReferences.Add( new SICReference( "3334", "PRIMARY PRODUCTION OF ALUMINUM" ) );
					sortedSicReferences.Add( new SICReference( "3339", "PRIMARY SMELTING AND REFINING OF NONFERROUS METALS, EXCEPT COPPER" ) );

					sortedSicReferences.Add( new SICReference( "3340", "SECONDARY SMELTING AND REFINING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3341", "SECONDARY SMELTING AND REFINING OF NONFERROUS METALS" ) );

					sortedSicReferences.Add( new SICReference( "3350", "ROLLING, DRAWING, AND EXTRUDING OF NONFERROUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "3351", "ROLLING, DRAWING, AND EXTRUDING OF COPPER" ) );
					sortedSicReferences.Add( new SICReference( "3353", "ALUMINUM SHEET, PLATE, AND FOIL" ) );
					sortedSicReferences.Add( new SICReference( "3354", "ALUMINUM EXTRUDED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3355", "ALUMINUM ROLLING AND DRAWING, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3356", "ROLLING, DRAWING, AND EXTRUDING OF NONFERROUS METALS, EXCEPT COPP" ) );
					sortedSicReferences.Add( new SICReference( "3357", "DRAWING AND INSULATING OF NONFERROUS WIRE" ) );

					sortedSicReferences.Add( new SICReference( "3360", "NONFERROUS FOUNDRIES (CASTINGS)" ) );
					sortedSicReferences.Add( new SICReference( "3363", "ALUMINUM DIE-CASTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3364", "NONFERROUS DIE-CASTINGS, EXCEPT ALUMINUM" ) );
					sortedSicReferences.Add( new SICReference( "3365", "ALUMINUM FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3366", "COPPER FOUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "3369", "NONFERROUS FOUNDRIES, EXCEPT ALUMINUM AND COPPER" ) );

					sortedSicReferences.Add( new SICReference( "3390", "MISCELLANEOUS PRIMARY METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3398", "METAL HEAT TREATING" ) );
					sortedSicReferences.Add( new SICReference( "3399", "PRIMARY METAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3410", "METAL CANS AND SHIPPING CONTAINERS" ) );
					sortedSicReferences.Add( new SICReference( "3411", "METAL CANS" ) );
					sortedSicReferences.Add( new SICReference( "3412", "METAL SHIPPING BARRELS, DRUMS, KEGS, AND PAILS" ) );

					sortedSicReferences.Add( new SICReference( "3420", "CUTLERY, HANDTOOLS, AND GENERAL HARDWARE" ) );
					sortedSicReferences.Add( new SICReference( "3421", "CUTLERY" ) );
					sortedSicReferences.Add( new SICReference( "3423", "HAND AND EDGE TOOLS, EXCEPT MACHINE TOOLS AND HANDSAWS" ) );
					sortedSicReferences.Add( new SICReference( "3425", "SAW BLADES AND HANDSAWS" ) );
					sortedSicReferences.Add( new SICReference( "3429", "HARDWARE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3430", "HEATING EQUIPMENT, EXCEPT ELECTRIC AND WARM AIR; AND PLUMBING FIXT" ) );
					sortedSicReferences.Add( new SICReference( "3431", "ENAMELED IRON AND METAL SANITARY WARE" ) );
					sortedSicReferences.Add( new SICReference( "3432", "PLUMBING FIXTURE FITTINGS AND TRIM" ) );
					sortedSicReferences.Add( new SICReference( "3433", "HEATING EQUIPMENT, EXCEPT ELECTRIC AND WARM AIR FURNACES" ) );

					sortedSicReferences.Add( new SICReference( "3440", "FABRICATED STRUCTURAL METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3441", "FABRICATED STRUCTURAL METAL" ) );
					sortedSicReferences.Add( new SICReference( "3442", "METAL DOORS, SASH, FRAMES, MOLDINGS AND TRIM" ) );
					sortedSicReferences.Add( new SICReference( "3443", "FABRICATED PLATE WORK (BOILER SHOPS)" ) );
					sortedSicReferences.Add( new SICReference( "3444", "SHEET METAL WORK" ) );
					sortedSicReferences.Add( new SICReference( "3446", "ARCHITECTURAL AND ORNAMENTAL METALWORK" ) );
					sortedSicReferences.Add( new SICReference( "3448", "PREFABRICATED METAL BUILDINGS AND COMPONENTS" ) );
					sortedSicReferences.Add( new SICReference( "3449", "MISCELLANEOUS STRUCTURAL METALWORK" ) );

					sortedSicReferences.Add( new SICReference( "3450", "SCREW MACHINE PRODUCTS, AND BOLTS, NUTS, SCREWS, RIVETS, AND WASHE" ) );
					sortedSicReferences.Add( new SICReference( "3451", "SCREW MACHINE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3452", "BOLTS, NUTS, SCREWS, RIVETS, AND WASHERS" ) );

					sortedSicReferences.Add( new SICReference( "3460", "METAL FORGINGS AND STAMPINGS" ) );
					sortedSicReferences.Add( new SICReference( "3462", "IRON AND STEEL FORGINGS" ) );
					sortedSicReferences.Add( new SICReference( "3463", "NONFERROUS FORGINGS" ) );
					sortedSicReferences.Add( new SICReference( "3465", "AUTOMOTIVE STAMPINGS" ) );
					sortedSicReferences.Add( new SICReference( "3466", "CROWNS AND CLOSURES" ) );
					sortedSicReferences.Add( new SICReference( "3469", "METAL STAMPINGS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3470", "COATING, ENGRAVING, AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "3471", "ELECTROPLATING, PLATING, POLISHING, ANODIZING, AND COLORING" ) );
					sortedSicReferences.Add( new SICReference( "3479", "COATING, ENGRAVING, AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3480", "ORDNANCE AND ACCESSORIES, EXCEPT VEHICLES AND GUIDED MISSILES" ) );
					sortedSicReferences.Add( new SICReference( "3482", "SMALL ARMS AMMUNITION" ) );
					sortedSicReferences.Add( new SICReference( "3483", "AMMUNITION, EXCEPT FOR SMALL ARMS" ) );
					sortedSicReferences.Add( new SICReference( "3484", "SMALL ARMS" ) );
					sortedSicReferences.Add( new SICReference( "3489", "ORDNANCE AND ACCESSORIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3490", "MISCELLANEOUS FABRICATED METAL PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3491", "INDUSTRIAL VALVES" ) );
					sortedSicReferences.Add( new SICReference( "3492", "FLUID POWER VALVES AND HOSE FITTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3493", "STEEL SPRINGS, EXCEPT WIRE" ) );
					sortedSicReferences.Add( new SICReference( "3494", "VALVES AND PIPE FITTINGS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3495", "WIRE SPRINGS" ) );
					sortedSicReferences.Add( new SICReference( "3496", "MISCELLANEOUS FABRICATED WIRE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3497", "METAL FOIL AND LEAF" ) );
					sortedSicReferences.Add( new SICReference( "3498", "FABRICATED PIPE AND PIPE FITTINGS" ) );
					sortedSicReferences.Add( new SICReference( "3499", "FABRICATED METAL PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3510", "ENGINES AND TURBINES" ) );
					sortedSicReferences.Add( new SICReference( "3511", "STEAM, GAS, AND HYDRAULIC TURBINES, AND TURBINE GENERATOR SET UNI" ) );
					sortedSicReferences.Add( new SICReference( "3519", "INTERNAL COMBUSTION ENGINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3520", "FARM AND GARDEN MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3523", "FARM MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3524", "LAWN AND GARDEN TRACTORS AND HOME LAWN AND GARDEN EQUIPMENT" ) );

					sortedSicReferences.Add( new SICReference( "3530", "CONSTRUCTION, MINING, AND MATERIALS HANDLING MACHINERY AND EQUIPME" ) );
					sortedSicReferences.Add( new SICReference( "3531", "CONSTRUCTION MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3532", "MINING MACHINERY AND EQUIPMENT, EXCEPT OIL AND GAS FIELD MACHINER" ) );
					sortedSicReferences.Add( new SICReference( "3533", "OIL AND GAS FIELD MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3534", "ELEVATORS AND MOVING STAIRWAYS" ) );
					sortedSicReferences.Add( new SICReference( "3535", "CONVEYORS AND CONVEYING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3536", "OVERHEAD TRAVELING CRANES, HOISTS, AND MONORAIL SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "3537", "INDUSTRIAL TRUCKS, TRACTORS, TRAILERS, AND STACKERS" ) );

					sortedSicReferences.Add( new SICReference( "3540", "METALWORKING MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3541", "MACHINE TOOLS, METAL CUTTING TYPES" ) );
					sortedSicReferences.Add( new SICReference( "3542", "MACHINE TOOLS, METAL FORMING TYPES" ) );
					sortedSicReferences.Add( new SICReference( "3543", "INDUSTRIAL PATTERNS" ) );
					sortedSicReferences.Add( new SICReference( "3544", "SPECIAL DIES AND TOOLS, DIE SETS, JIGS AND FIXTURES, AND INDUSTRI" ) );
					sortedSicReferences.Add( new SICReference( "3545", "CUTTING TOOLS, MACHINE TOOL ACCESSORIES, AND MACHINISTS' PRECISIO" ) );
					sortedSicReferences.Add( new SICReference( "3546", "POWER-DRIVEN HANDTOOLS" ) );
					sortedSicReferences.Add( new SICReference( "3547", "ROLLING MILL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3548", "ELECTRIC AND GAS WELDING AND SOLDERING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3549", "METALWORKING MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3550", "SPECIAL INDUSTRY MACHINERY, EXCEPT METALWORKING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3552", "TEXTILE MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3553", "WOODWORKING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3554", "PAPER INDUSTRIES MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3555", "PRINTING TRADES MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3556", "FOOD PRODUCTS MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3559", "SPECIAL INDUSTRY MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3560", "GENERAL INDUSTRIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3561", "PUMPS AND PUMPING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3562", "BALL AND ROLLER BEARINGS" ) );
					sortedSicReferences.Add( new SICReference( "3563", "AIR AND GAS COMPRESSORS" ) );
					sortedSicReferences.Add( new SICReference( "3564", "INDUSTRIAL AND COMMERCIAL FANS AND BLOWERS AND AIR PURIFING EQUIP" ) );
					sortedSicReferences.Add( new SICReference( "3565", "PACKAGING MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3566", "SPEED CHANGERS, INDUSTRIAL HIGH-SPEED DRIVES, AND GEARS" ) );
					sortedSicReferences.Add( new SICReference( "3567", "INDUSTRIAL PROCESS FURNACES AND OVENS" ) );
					sortedSicReferences.Add( new SICReference( "3568", "MECHANICAL POWER TRANSMISSION EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3569", "GENERAL INDUSTRIAL MACHINERY AND EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3570", "COMPUTER AND OFFICE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3571", "ELECTRONIC COMPUTERS" ) );
					sortedSicReferences.Add( new SICReference( "3572", "COMPUTER STORAGE DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3575", "COMPUTER TERMINALS" ) );
					sortedSicReferences.Add( new SICReference( "3576", "COMPUTER COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3577", "COMPUTER PERIPHERAL EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "3578", "CALCULATING AND ACCOUNTING MACHINES, EXCEPT ELECTRONIC COMPUTERS" ) );
					sortedSicReferences.Add( new SICReference( "3579", "OFFICE MACHINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3580", "REFRIGERATION AND SERVICE INDUSTRY MACHINERY" ) );
					sortedSicReferences.Add( new SICReference( "3581", "AUTOMATIC VENDING MACHINES" ) );
					sortedSicReferences.Add( new SICReference( "3582", "COMMERCIAL LAUNDRY, DRYCLEANING, AND PRESSING MACHINES" ) );
					sortedSicReferences.Add( new SICReference( "3585", "AIR-CONDITIONING AND WARM AIR HEATING EQUIPMENT AND COMMERCIAL AN" ) );
					sortedSicReferences.Add( new SICReference( "3586", "MEASURING AND DISPENSING PUMPS" ) );
					sortedSicReferences.Add( new SICReference( "3589", "SERVICE INDUSTRY MACHINERY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3590", "MISCELLANEOUS INDUSTRIAL AND COMMERCIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3592", "CARBURETORS, PISTONS, PISTON RINGS, AND VALVES" ) );
					sortedSicReferences.Add( new SICReference( "3593", "FLUID POWER CYLINDERS AND ACTUATORS" ) );
					sortedSicReferences.Add( new SICReference( "3594", "FLUID POWER PUMPS AND MOTORS" ) );
					sortedSicReferences.Add( new SICReference( "3596", "SCALES AND BALANCES, EXCEPT LABORATORY" ) );
					sortedSicReferences.Add( new SICReference( "3599", "INDUSTRIAL AND COMMERCIAL MACHINERY AND EQUIPMENT, NOT ELSEWHERE" ) );

					sortedSicReferences.Add( new SICReference( "3600", "ELECTRONIC AND OTHER ELECTRICAL EQUIPMENT (NO COMPUTER EQUIP)" ) );

					sortedSicReferences.Add( new SICReference( "3610", "ELECTRIC TRANSMISSION AND DISTRIBUTION EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3612", "POWER, DISTRIBUTION, AND SPECIALTY TRANSFORMERS" ) );
					sortedSicReferences.Add( new SICReference( "3613", "SWITCHGEAR AND SWITCHBOARD APPARATUS" ) );

					sortedSicReferences.Add( new SICReference( "3620", "ELECTRICAL INDUSTRIAL APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3621", "MOTORS AND GENERATORS" ) );
					sortedSicReferences.Add( new SICReference( "3624", "CARBON AND GRAPHITE PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "3625", "RELAYS AND INDUSTRIAL CONTROLS" ) );
					sortedSicReferences.Add( new SICReference( "3629", "ELECTRICAL INDUSTRIAL APPARATUS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3630", "HOUSEHOLD APPLIANCES" ) );
					sortedSicReferences.Add( new SICReference( "3631", "HOUSEHOLD COOKING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3632", "HOUSEHOLD REFRIGERATORS AND HOME AND FARM FREEZERS" ) );
					sortedSicReferences.Add( new SICReference( "3633", "HOUSEHOLD LAUNDRY EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3634", "ELECTRIC HOUSEWARES AND FANS" ) );
					sortedSicReferences.Add( new SICReference( "3635", "HOUSEHOLD VACUUM CLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "3639", "HOUSEHOLD APPLIANCES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3640", "ELECTRIC LIGHTING AND WIRING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3641", "ELECTRIC LAMP BULBS AND TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3643", "CURRENT-CARRYING WIRING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3644", "NONCURRENT-CARRYING WIRING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3645", "RESIDENTIAL ELECTRIC LIGHTING FIXTURES" ) );
					sortedSicReferences.Add( new SICReference( "3646", "COMMERCIAL, INDUSTRIAL, AND INSTITUTIONAL ELECTRIC LIGHTING FIXTU" ) );
					sortedSicReferences.Add( new SICReference( "3647", "VEHICULAR LIGHTING EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3648", "LIGHTING EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3650", "HOUSEHOLD AUDIO AND VIDEO EQUIPMENT, AND AUDIO RECORDINGS" ) );
					sortedSicReferences.Add( new SICReference( "3651", "HOUSEHOLD AUDIO AND VIDEO EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3652", "PHONOGRAPH RECORDS AND PRERECORDED AUDIO TAPES AND DISKS" ) );

					sortedSicReferences.Add( new SICReference( "3660", "COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3661", "TELEPHONE AND TELEGRAPH APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3663", "RADIO AND TELEVISION BROADCASTING AND COMMUNICATIONS EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3669", "COMMUNICATIONS EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3670", "ELECTRONIC COMPONENTS AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "3671", "ELECTRON TUBES" ) );
					sortedSicReferences.Add( new SICReference( "3672", "PRINTED CIRCUIT BOARDS" ) );
					sortedSicReferences.Add( new SICReference( "3674", "SEMICONDUCTORS AND RELATED DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3675", "ELECTRONIC CAPACITORS" ) );
					sortedSicReferences.Add( new SICReference( "3676", "ELECTRONIC RESISTORS" ) );
					sortedSicReferences.Add( new SICReference( "3677", "ELECTRONIC COILS, TRANSFORMERS, AND OTHER INDUCTORS" ) );
					sortedSicReferences.Add( new SICReference( "3678", "ELECTRONIC CONNECTORS" ) );
					sortedSicReferences.Add( new SICReference( "3679", "ELECTRONIC COMPONENTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3690", "MISCELLANEOUS ELECTRICAL MACHINERY, EQUIPMENT, AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3691", "STORAGE BATTERIES" ) );
					sortedSicReferences.Add( new SICReference( "3692", "PRIMARY BATTERIES, DRY AND WET" ) );
					sortedSicReferences.Add( new SICReference( "3694", "ELECTRICAL EQUIPMENT FOR INTERNAL COMBUSTION ENGINES" ) );
					sortedSicReferences.Add( new SICReference( "3695", "MAGNETIC AND OPTICAL RECORDING MEDIA" ) );
					sortedSicReferences.Add( new SICReference( "3699", "ELECTRICAL MACHINERY, EQUIPMENT, AND SUPPLIES, NOT ELSEWHERE CLAS" ) );

					sortedSicReferences.Add( new SICReference( "3710", "MOTOR VEHICLES AND MOTOR VEHICLE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3711", "MOTOR VEHICLES AND PASSENGER CAR BODIES" ) );
					sortedSicReferences.Add( new SICReference( "3713", "TRUCK AND BUS BODIES" ) );
					sortedSicReferences.Add( new SICReference( "3714", "MOTOR VEHICLE PARTS AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "3715", "TRUCK TRAILERS" ) );
					sortedSicReferences.Add( new SICReference( "3716", "MOTOR HOMES" ) );

					sortedSicReferences.Add( new SICReference( "3720", "AIRCRAFT AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3721", "AIRCRAFT" ) );
					sortedSicReferences.Add( new SICReference( "3724", "AIRCRAFT ENGINES AND ENGINE PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3728", "AIRCRAFT PARTS AND AUXILIARY EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3730", "SHIP AND BOAT BUILDING AND REPAIRING" ) );
					sortedSicReferences.Add( new SICReference( "3731", "SHIP BUILDING AND REPAIRING" ) );
					sortedSicReferences.Add( new SICReference( "3732", "BOAT BUILDING AND REPAIRING" ) );

					sortedSicReferences.Add( new SICReference( "3740", "RAILROAD EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3743", "RAILROAD EQUIPMENT" ) );

					sortedSicReferences.Add( new SICReference( "3750", "MOTORCYCLES, BICYCLES, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3751", "MOTORCYCLES, BICYCLES, AND PARTS" ) );

					sortedSicReferences.Add( new SICReference( "3760", "GUIDED MISSILES AND SPACE VEHICLES AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3761", "GUIDED MISSILES AND SPACE VEHICLES" ) );
					sortedSicReferences.Add( new SICReference( "3764", "GUIDED MISSILE AND SPACE VEHICLE PROPULSION UNITS AND PROPULSION" ) );
					sortedSicReferences.Add( new SICReference( "3769", "GUIDED MISSILE AND SPACE VEHICLE PARTS AND AUXILIARY EQUIPMENT, N" ) );

					sortedSicReferences.Add( new SICReference( "3790", "MISCELLANEOUS TRANSPORTATION EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "3792", "TRAVEL TRAILERS AND CAMPERS" ) );
					sortedSicReferences.Add( new SICReference( "3795", "TANKS AND TANK COMPONENTS" ) );
					sortedSicReferences.Add( new SICReference( "3799", "TRANSPORTATION EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3810", "SEARCH, DETECTION, NAVIGATION, GUIDANCE, AERONAUTICAL, AND NAUTICA" ) );
					sortedSicReferences.Add( new SICReference( "3812", "SEARCH, DETECTION, NAVIGATION, GUIDANCE, AERONAUTICAL, AND NAUTIC" ) );

					sortedSicReferences.Add( new SICReference( "3820", "LABORATORY APPARATUS AND ANALYTICAL, OPTICAL, MEASURING, AND CONTR" ) );
					sortedSicReferences.Add( new SICReference( "3821", "LABORATORY APPARATUS AND FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "3822", "AUTOMATIC CONTROLS FOR REGULATING RESIDENTIAL AND COMMERCIAL ENVI" ) );
					sortedSicReferences.Add( new SICReference( "3823", "INDUSTRIAL INSTRUMENTS FOR MEASUREMENT, DISPLAY, AND CONTROL OF P" ) );
					sortedSicReferences.Add( new SICReference( "3824", "TOTALIZING FLUID METERS AND COUNTING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3825", "INSTRUMENTS FOR MEASURING AND TESTING OF ELECTRICITY AND ELECTRIC" ) );
					sortedSicReferences.Add( new SICReference( "3826", "LABORATORY ANALYTICAL INSTRUMENTS" ) );
					sortedSicReferences.Add( new SICReference( "3827", "OPTICAL INSTRUMENTS AND LENSES" ) );
					sortedSicReferences.Add( new SICReference( "3829", "MEASURING AND CONTROLLING DEVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3840", "SURGICAL, MEDICAL, AND DENTAL INSTRUMENTS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3841", "SURGICAL AND MEDICAL INSTRUMENTS AND APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3842", "ORTHOPEDIC, PROSTHETIC, AND SURGICAL APPLIANCES AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3843", "DENTAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3844", "X-RAY APPARATUS AND TUBES AND RELATED IRRADIATION APPARATUS" ) );
					sortedSicReferences.Add( new SICReference( "3845", "ELECTROMEDICAL AND ELECTROTHERAPEUTIC APPARATUS" ) );

					sortedSicReferences.Add( new SICReference( "3850", "OPHTHALMIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3851", "OPHTHALMIC GOODS" ) );

					sortedSicReferences.Add( new SICReference( "3860", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "3861", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );

					sortedSicReferences.Add( new SICReference( "3870", "WATCHES, CLOCKS, CLOCKWORK OPERATED DEVICES, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3873", "WATCHES, CLOCKS, CLOCKWORK OPERATED DEVICES, AND PARTS" ) );

					sortedSicReferences.Add( new SICReference( "3910", "JEWELRY, SILVERWARE, AND PLATED WARE" ) );
					sortedSicReferences.Add( new SICReference( "3911", "JEWELRY, PRECIOUS METAL" ) );
					sortedSicReferences.Add( new SICReference( "3914", "SILVERWARE, PLATED WARE, AND STAINLESS STEEL WARE" ) );
					sortedSicReferences.Add( new SICReference( "3915", "JEWELERS' FINDINGS AND MATERIALS, AND LAPIDARY WORK" ) );

					sortedSicReferences.Add( new SICReference( "3930", "MUSICAL INSTRUMENTS" ) );
					sortedSicReferences.Add( new SICReference( "3931", "MUSICAL INSTRUMENTS" ) );

					sortedSicReferences.Add( new SICReference( "3940", "DOLLS, TOYS, GAMES AND SPORTING AND ATHLETIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "3942", "DOLLS AND STUFFED TOYS" ) );
					sortedSicReferences.Add( new SICReference( "3944", "GAMES, TOYS, AND CHILDREN'S VEHICLES, EXCEPT DOLLS AND BICYCLES" ) );
					sortedSicReferences.Add( new SICReference( "3949", "SPORTING AND ATHLETIC GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "3950", "PENS, PENCILS, AND OTHER ARTISTS' MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "3951", "PENS, MECHANICAL PENCILS, AND PARTS" ) );
					sortedSicReferences.Add( new SICReference( "3952", "LEAD PENCILS, CRAYONS, AND ARTISTS' MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "3953", "MARKING DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "3955", "CARBON PAPER AND INKED RIBBONS" ) );

					sortedSicReferences.Add( new SICReference( "3960", "COSTUME JEWELRY, COSTUME NOVELTIES, BUTTONS, AND MISCELLANEOUS NOT" ) );
					sortedSicReferences.Add( new SICReference( "3961", "COSTUME JEWELRY AND COSTUME NOVELTIES, EXCEPT PRECIOUS METAL" ) );
					sortedSicReferences.Add( new SICReference( "3965", "FASTENERS, BUTTONS, NEEDLES, AND PINS" ) );

					sortedSicReferences.Add( new SICReference( "3990", "MISCELLANEOUS MANUFACTURING INDUSTRIES" ) );
					sortedSicReferences.Add( new SICReference( "3991", "BROOMS AND BRUSHES" ) );
					sortedSicReferences.Add( new SICReference( "3993", "SIGNS AND ADVERTISING SPECIALTIES" ) );
					sortedSicReferences.Add( new SICReference( "3995", "BURIAL CASKETS" ) );
					sortedSicReferences.Add( new SICReference( "3996", "LINOLEUM, ASPHALTED-FELT-BASE, AND OTHER HARD SURFACE FLOOR COVER" ) );
					sortedSicReferences.Add( new SICReference( "3999", "MANUFACTURING INDUSTRIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4010", "RAILROADS" ) );
					sortedSicReferences.Add( new SICReference( "4011", "RAILROADS, LINE-HAUL OPERATING" ) );
					sortedSicReferences.Add( new SICReference( "4013", "RAILROAD SWITCHING AND TERMINAL ESTABLISHMENTS" ) );

					sortedSicReferences.Add( new SICReference( "4100", "LOCAL AND SUBURBAN TRANSIT AND INTERURBAN HWY PASSENGER TRANS" ) );

					sortedSicReferences.Add( new SICReference( "4110", "LOCAL AND SUBURBAN PASSENGER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4111", "LOCAL AND SUBURBAN TRANSIT" ) );
					sortedSicReferences.Add( new SICReference( "4119", "LOCAL PASSENGER TRANSPORTATION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4120", "TAXICABS" ) );
					sortedSicReferences.Add( new SICReference( "4121", "TAXICABS" ) );

					sortedSicReferences.Add( new SICReference( "4130", "INTERCITY AND RURAL BUS TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4131", "INTERCITY AND RURAL BUS TRANSPORTATION" ) );

					sortedSicReferences.Add( new SICReference( "4140", "BUS CHARTER SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4141", "LOCAL BUS CHARTER SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4142", "BUS CHARTER SERVICE, EXCEPT LOCAL" ) );

					sortedSicReferences.Add( new SICReference( "4150", "SCHOOL BUSES" ) );
					sortedSicReferences.Add( new SICReference( "4151", "SCHOOL BUSES" ) );

					sortedSicReferences.Add( new SICReference( "4170", "TERMINAL AND SERVICE FACILITIES FOR MOTOR VEHICLE PASSENGER TRANSP" ) );
					sortedSicReferences.Add( new SICReference( "4173", "TERMINAL AND SERVICE FACILITIES FOR MOTOR VEHICLE PASSENGER TRANS" ) );

					sortedSicReferences.Add( new SICReference( "4210", "TRUCKING AND COURIER SERVICES, EXCEPT AIR" ) );
					sortedSicReferences.Add( new SICReference( "4212", "LOCAL TRUCKING WITHOUT STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4213", "TRUCKING, EXCEPT LOCAL" ) );
					sortedSicReferences.Add( new SICReference( "4214", "LOCAL TRUCKING WITH STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4215", "COURIER SERVICES, EXCEPT BY AIR" ) );

					sortedSicReferences.Add( new SICReference( "4220", "PUBLIC WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4221", "FARM PRODUCT WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4222", "REFRIGERATED WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4225", "GENERAL WAREHOUSING AND STORAGE" ) );
					sortedSicReferences.Add( new SICReference( "4226", "SPECIAL WAREHOUSING AND STORAGE, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4230", "TERMINAL AND JOINT TERMINAL MAINTENANCE FACILITIES FOR MOTOR FREIG" ) );
					sortedSicReferences.Add( new SICReference( "4231", "TERMINAL AND JOINT TERMINAL MAINTENANCE FACILITIES FOR MOTOR FREI" ) );

					sortedSicReferences.Add( new SICReference( "4310", "UNITED STATES POSTAL SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "4311", "UNITED STATES POSTAL SERVICE" ) );

					sortedSicReferences.Add( new SICReference( "4400", "WATER TRANSPORTATION" ) );

					sortedSicReferences.Add( new SICReference( "4410", "DEEP SEA FOREIGN TRANSPORTATION OF FREIGHT" ) );
					sortedSicReferences.Add( new SICReference( "4412", "DEEP SEA FOREIGN TRANSPORTATION OF FREIGHT" ) );

					sortedSicReferences.Add( new SICReference( "4420", "DEEP SEA DOMESTIC TRANSPORTATION OF FREIGHT" ) );
					sortedSicReferences.Add( new SICReference( "4424", "DEEP SEA DOMESTIC TRANSPORTATION OF FREIGHT" ) );

					sortedSicReferences.Add( new SICReference( "4430", "FREIGHT TRANSPORTATION ON THE GREAT LAKES&die;ST. LAWRENCE SEAWAY" ) );
					sortedSicReferences.Add( new SICReference( "4432", "FREIGHT TRANSPORTATION ON THE GREAT LAKES&die;ST. LAWRENCE SEAWAY" ) );

					sortedSicReferences.Add( new SICReference( "4440", "WATER TRANSPORTATION OF FREIGHT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "4449", "WATER TRANSPORTATION OF FREIGHT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4480", "WATER TRANSPORTATION OF PASSENGERS" ) );
					sortedSicReferences.Add( new SICReference( "4481", "DEEP SEA TRANSPORTATION OF PASSENGERS, EXCEPT BY FERRY" ) );
					sortedSicReferences.Add( new SICReference( "4482", "FERRIES" ) );
					sortedSicReferences.Add( new SICReference( "4489", "WATER TRANSPORTATION OF PASSENGERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4490", "SERVICES INCIDENTAL TO WATER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4491", "MARINE CARGO HANDLING" ) );
					sortedSicReferences.Add( new SICReference( "4492", "TOWING AND TUGBOAT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4493", "MARINAS" ) );
					sortedSicReferences.Add( new SICReference( "4499", "WATER TRANSPORTATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4510", "AIR TRANSPORTATION, SCHEDULED, AND AIR COURIER SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4512", "AIR TRANSPORTATION, SCHEDULED" ) );
					sortedSicReferences.Add( new SICReference( "4513", "AIR COURIER SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4520", "AIR TRANSPORTATION, NONSCHEDULED" ) );
					sortedSicReferences.Add( new SICReference( "4522", "AIR TRANSPORTATION, NONSCHEDULED" ) );

					sortedSicReferences.Add( new SICReference( "4580", "AIRPORTS, FLYING FIELDS, AND AIRPORT TERMINAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4581", "AIRPORTS, FLYING FIELDS, AND AIRPORT TERMINAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4610", "PIPELINES, EXCEPT NATURAL GAS" ) );
					sortedSicReferences.Add( new SICReference( "4612", "CRUDE PETROLEUM PIPELINES" ) );
					sortedSicReferences.Add( new SICReference( "4613", "REFINED PETROLEUM PIPELINES" ) );
					sortedSicReferences.Add( new SICReference( "4619", "PIPELINES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4700", "TRANSPORTATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4720", "ARRANGEMENT OF PASSENGER TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4724", "TRAVEL AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "4725", "TOUR OPERATORS" ) );
					sortedSicReferences.Add( new SICReference( "4729", "ARRANGEMENT OF PASSENGER TRANSPORTATION, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4730", "ARRANGEMENT OF TRANSPORTATION OF FREIGHT AND CARGO" ) );
					sortedSicReferences.Add( new SICReference( "4731", "ARRANGEMENT OF TRANSPORTATION OF FREIGHT AND CARGO" ) );

					sortedSicReferences.Add( new SICReference( "4740", "RENTAL OF RAILROAD CARS" ) );
					sortedSicReferences.Add( new SICReference( "4741", "RENTAL OF RAILROAD CARS" ) );

					sortedSicReferences.Add( new SICReference( "4780", "MISCELLANEOUS SERVICES INCIDENTAL TO TRANSPORTATION" ) );
					sortedSicReferences.Add( new SICReference( "4783", "PACKING AND CRATING" ) );
					sortedSicReferences.Add( new SICReference( "4785", "FIXED FACILITIES AND INSPECTION AND WEIGHING SERVICES FOR MOTOR V" ) );
					sortedSicReferences.Add( new SICReference( "4789", "TRANSPORTATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4810", "TELEPHONE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4812", "RADIOTELEPHONE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4813", "TELEPHONE COMMUNICATIONS, EXCEPT RADIOTELEPHONE" ) );

					sortedSicReferences.Add( new SICReference( "4820", "TELEGRAPH AND OTHER MESSAGE COMMUNICATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4822", "TELEGRAPH AND OTHER MESSAGE COMMUNICATIONS" ) );

					sortedSicReferences.Add( new SICReference( "4830", "RADIO AND TELEVISION BROADCASTING STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4832", "RADIO BROADCASTING STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "4833", "TELEVISION BROADCASTING STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "4840", "CABLE AND OTHER PAY TELEVISION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4841", "CABLE AND OTHER PAY TELEVISION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4890", "COMMUNICATIONS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "4899", "COMMUNICATIONS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4900", "ELECTRIC, GAS AND SANITARY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4910", "ELECTRIC SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4911", "ELECTRIC SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "4920", "GAS PRODUCTION AND DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4922", "NATURAL GAS TRANSMISSION" ) );
					sortedSicReferences.Add( new SICReference( "4923", "NATURAL GAS TRANSMISISON AND DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4924", "NATURAL GAS DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "4925", "MIXED, MANUFACTURED, OR LIQUEFIED PETROLEUM GAS PRODUCTION AND/OR" ) );

					sortedSicReferences.Add( new SICReference( "4930", "COMBINATION ELECTRIC AND GAS, AND OTHER UTILITY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4931", "ELECTRIC AND OTHER SERVICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "4932", "GAS AND OTHER SERVICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "4939", "COMBINATION UTILITIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4940", "WATER SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "4941", "WATER SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "4950", "SANITARY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "4952", "SEWERAGE SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4953", "REFUSE SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4955", "HAZARDOUS WASTE MANAGEMENT" ) );
					sortedSicReferences.Add( new SICReference( "4959", "SANITARY SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "4960", "STEAM AND AIR-CONDITIONING SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "4961", "STEAM AND AIR-CONDITIONING SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "4970", "IRRIGATION SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "4971", "IRRIGATION SYSTEMS" ) );

					sortedSicReferences.Add( new SICReference( "4991", "COGENERATION SERVICES AND SMALL POWER PRODUCERS" ) );

					sortedSicReferences.Add( new SICReference( "5000", "WHOLESALE-DURABLE GOODS" ) );

					sortedSicReferences.Add( new SICReference( "5010", "WHOLESALE-MOTOR VEHICLES AND MOTOR VEHICLE PARTS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5012", "AUTOMOBILES AND OTHER MOTOR VEHICLES" ) );
					sortedSicReferences.Add( new SICReference( "5013", "WHOLESALE-MOTOR VEHICLE SUPPLIES AND NEW PARTS" ) );
					sortedSicReferences.Add( new SICReference( "5014", "TIRES AND TUBES" ) );
					sortedSicReferences.Add( new SICReference( "5015", "MOTOR VEHICLE PARTS, USED" ) );

					sortedSicReferences.Add( new SICReference( "5020", "WHOLESALE-FURNITURE AND HOME FURNISHINGS" ) );
					sortedSicReferences.Add( new SICReference( "5021", "FURNITURE" ) );
					sortedSicReferences.Add( new SICReference( "5023", "HOMEFURNISHINGS" ) );

					sortedSicReferences.Add( new SICReference( "5030", "WHOLESALE-LUMBER AND OTHER CONSTRUCTION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5031", "WHOLESALE-LUMBER, PLYWOOD, MILLWORK AND WOOD PANELS" ) );
					sortedSicReferences.Add( new SICReference( "5032", "BRICK, STONE, AND RELATED CONSTRUCTION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5033", "ROOFING, SIDING, AND INSULATION MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5039", "CONSTRUCTION MATERIALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5040", "WHOLESALE-PROFESSIONAL AND COMMERCIAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5043", "PHOTOGRAPHIC EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5044", "OFFICE EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5045", "WHOLESALE-COMPUTERS AND PERIPHERAL EQUIPMENT AND SOFTWARE" ) );
					sortedSicReferences.Add( new SICReference( "5046", "COMMERCIAL EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5047", "WHOLESALE-MEDICAL, DENTAL AND HOSPITAL EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5048", "OPHTHALMIC GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5049", "PROFESSIONAL EQUIPMENT AND SUPPLIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5050", "WHOLESALE-METALS AND MINERALS (NO PETROLEUM)" ) );
					sortedSicReferences.Add( new SICReference( "5051", "WHOLESALE-METALS SERVICE CENTERS AND OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "5052", "COAL AND OTHER MINERALS AND ORES" ) );

					sortedSicReferences.Add( new SICReference( "5060", "ELECTRICAL GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5063", "ELECTRICAL APPARATUS AND EQUIPMENT, WIRING SUPPLIES, AND CONSTRUC" ) );
					sortedSicReferences.Add( new SICReference( "5064", "WHOLESALE-ELECTRICAL APPLIANCES, TV AND RADIO SETS" ) );
					sortedSicReferences.Add( new SICReference( "5065", "WHOLESALE-ELECTRONIC PARTS AND EQUIPMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5070", "WHOLESALE-HARDWARE AND PLUMBING AND HEATING EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5072", "WHOLESALE-HARDWARE" ) );
					sortedSicReferences.Add( new SICReference( "5074", "PLUMBING AND HEATING EQUIPMENT AND SUPPLIES (HYDRONICS)" ) );
					sortedSicReferences.Add( new SICReference( "5075", "WARM AIR HEATING AND AIR-CONDITIONING EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5078", "REFRIGERATION EQUIPMENT AND SUPPLIES" ) );

					sortedSicReferences.Add( new SICReference( "5080", "WHOLESALE-MACHINERY, EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5082", "CONSTRUCTION AND MINING (EXCEPT PETROLEUM) MACHINERY AND EQUIPMEN" ) );
					sortedSicReferences.Add( new SICReference( "5083", "FARM AND GARDEN MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5084", "WHOLESALE-INDUSTRIAL MACHINERY AND EQUIPMENT" ) );
					sortedSicReferences.Add( new SICReference( "5085", "INDUSTRIAL SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5087", "SERVICE ESTABLISHMENT EQUIPMENT AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5088", "TRANSPORTATION EQUIPMENT AND SUPPLIES, EXCEPT MOTOR VEHICLES" ) );

					sortedSicReferences.Add( new SICReference( "5090", "WHOLESALE-MISC DURABLE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5091", "SPORTING AND RECREATIONAL GOODS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5092", "TOYS AND HOBBY GOODS AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5093", "SCRAP AND WASTE MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5094", "JEWELRY, WATCHES, PRECIOUS STONES, AND PRECIOUS METALS" ) );
					sortedSicReferences.Add( new SICReference( "5099", "WHOLESALE-DURABLE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5110", "WHOLESALE-PAPER AND PAPER PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5111", "PRINTING AND WRITING PAPER" ) );
					sortedSicReferences.Add( new SICReference( "5112", "STATIONERY AND OFFICE SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5113", "INDUSTRIAL AND PERSONAL SERVICE PAPER" ) );

					sortedSicReferences.Add( new SICReference( "5120", "DRUGS, DRUG PROPRIETARIES, AND DRUGGISTS' SUNDRIES" ) );
					sortedSicReferences.Add( new SICReference( "5122", "WHOLESALE-DRUGS, PROPRIETARIES AND DRUGGISTS' SUNDRIES" ) );

					sortedSicReferences.Add( new SICReference( "5130", "WHOLESALE-APPAREL, PIECE GOODS AND NOTIONS" ) );
					sortedSicReferences.Add( new SICReference( "5131", "PIECE GOODS, NOTIONS, AND OTHER DRY GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5136", "MEN'S AND BOYS' CLOTHING AND FURNISHINGS" ) );
					sortedSicReferences.Add( new SICReference( "5137", "WOMEN'S, CHILDREN'S, AND INFANTS' CLOTHING AND ACCESSORIES" ) );
					sortedSicReferences.Add( new SICReference( "5139", "FOOTWEAR" ) );

					sortedSicReferences.Add( new SICReference( "5140", "WHOLESALE-GROCERIES AND RELATED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5141", "WHOLESALE-GROCERIES, GENERAL LINE" ) );
					sortedSicReferences.Add( new SICReference( "5142", "PACKAGED FROZEN FOODS" ) );
					sortedSicReferences.Add( new SICReference( "5143", "DAIRY PRODUCTS, EXCEPT DRIED OR CANNED" ) );
					sortedSicReferences.Add( new SICReference( "5144", "POULTRY AND POULTRY PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5145", "CONFECTIONERY" ) );
					sortedSicReferences.Add( new SICReference( "5146", "FISH AND SEAFOODS" ) );
					sortedSicReferences.Add( new SICReference( "5147", "MEATS AND MEAT PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5148", "FRESH FRUITS AND VEGETABLES" ) );
					sortedSicReferences.Add( new SICReference( "5149", "GROCERIES AND RELATED PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5150", "WHOLESALE-FARM PRODUCT RAW MATERIALS" ) );
					sortedSicReferences.Add( new SICReference( "5153", "GRAIN AND FIELD BEANS" ) );
					sortedSicReferences.Add( new SICReference( "5154", "LIVESTOCK" ) );
					sortedSicReferences.Add( new SICReference( "5159", "FARM-PRODUCT RAW MATERIALS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5160", "WHOLESALE-CHEMICALS AND ALLIED PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5162", "PLASTICS MATERIALS AND BASIC FORMS AND SHAPES" ) );
					sortedSicReferences.Add( new SICReference( "5169", "CHEMICALS AND ALLIED PRODUCTS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5170", "PETROLEUM AND PETROLEUM PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5171", "WHOLESALE-PETROLEUM BULK STATIONS AND TERMINALS" ) );
					sortedSicReferences.Add( new SICReference( "5172", "PETROLEUM AND PETROLEUM PRODUCTS WHOLESALERS, EXCEPT BULK STATION" ) );

					sortedSicReferences.Add( new SICReference( "5180", "WHOLESALE-BEER, WINE AND DISTILLED ALCOHOLIC BEVERAGES" ) );
					sortedSicReferences.Add( new SICReference( "5181", "BEER AND ALE" ) );
					sortedSicReferences.Add( new SICReference( "5182", "WINE AND DISTILLED ALCOHOLIC BEVERAGES" ) );

					sortedSicReferences.Add( new SICReference( "5190", "WHOLESALE-MISCELLANEOUS NONDURABLE GOODS" ) );
					sortedSicReferences.Add( new SICReference( "5191", "FARM SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5192", "BOOKS, PERIODICALS, AND NEWSPAPERS" ) );
					sortedSicReferences.Add( new SICReference( "5193", "FLOWERS, NURSERY STOCK, AND FLORISTS' SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5194", "TOBACCO AND TOBACCO PRODUCTS" ) );
					sortedSicReferences.Add( new SICReference( "5198", "PAINTS, VARNISHES, AND SUPPLIES" ) );
					sortedSicReferences.Add( new SICReference( "5199", "NONDURABLE GOODS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5200", "RETAIL-BUILDING MATERIALS, HARDWARE, GARDEN SUPPLY" ) );

					sortedSicReferences.Add( new SICReference( "5210", "LUMBER AND OTHER BUILDING MATERIALS DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5211", "RETAIL-LUMBER AND OTHER BUILDING MATERIALS DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5230", "PAINT, GLASS, AND WALLPAPER STORES" ) );
					sortedSicReferences.Add( new SICReference( "5231", "PAINT, GLASS, AND WALLPAPER STORES" ) );

					sortedSicReferences.Add( new SICReference( "5250", "HARDWARE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5251", "HARDWARE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5260", "RETAIL NURSERIES, LAWN AND GARDEN SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5261", "RETAIL NURSERIES, LAWN AND GARDEN SUPPLY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5270", "MOBILE HOME DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5271", "RETAIL-MOBILE HOME DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5310", "DEPARTMENT STORES" ) );
					sortedSicReferences.Add( new SICReference( "5311", "RETAIL-DEPARTMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5330", "VARIETY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5331", "RETAIL-VARIETY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5390", "MISCELLANEOUS GENERAL MERCHANDISE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5399", "MISCELLANEOUS GENERAL MERCHANDISE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5400", "RETAIL-FOOD STORES" ) );

					sortedSicReferences.Add( new SICReference( "5410", "GROCERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5411", "RETAIL-GROCERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5412", "RETAIL-CONVENIENCE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5420", "MEAT AND FISH (SEAFOOD) MARKETS, INCLUDING FREEZER PROVISIONERS" ) );
					sortedSicReferences.Add( new SICReference( "5421", "MEAT AND FISH (SEAFOOD) MARKETS, INCLUDING FREEZER PROVISIONERS" ) );

					sortedSicReferences.Add( new SICReference( "5430", "FRUIT AND VEGETABLE MARKETS" ) );
					sortedSicReferences.Add( new SICReference( "5431", "FRUIT AND VEGETABLE MARKETS" ) );

					sortedSicReferences.Add( new SICReference( "5440", "CANDY, NUT, AND CONFECTIONERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5441", "CANDY, NUT, AND CONFECTIONERY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5450", "DAIRY PRODUCTS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5451", "DAIRY PRODUCTS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5460", "RETAIL BAKERIES" ) );
					sortedSicReferences.Add( new SICReference( "5461", "RETAIL BAKERIES" ) );

					sortedSicReferences.Add( new SICReference( "5490", "MISCELLANEOUS FOOD STORES" ) );
					sortedSicReferences.Add( new SICReference( "5499", "MISCELLANEOUS FOOD STORES" ) );

					sortedSicReferences.Add( new SICReference( "5500", "RETAIL-AUTO DEALERS AND GASOLINE STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "5510", "MOTOR VEHICLE DEALERS (NEW AND USED)" ) );
					sortedSicReferences.Add( new SICReference( "5511", "MOTOR VEHICLE DEALERS (NEW AND USED)" ) );

					sortedSicReferences.Add( new SICReference( "5520", "MOTOR VEHICLE DEALERS (USED ONLY)" ) );
					sortedSicReferences.Add( new SICReference( "5521", "MOTOR VEHICLE DEALERS (USED ONLY)" ) );

					sortedSicReferences.Add( new SICReference( "5530", "AUTO AND HOME SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5531", "RETAIL-AUTO AND HOME SUPPLY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5540", "GASOLINE SERVICE STATIONS" ) );
					sortedSicReferences.Add( new SICReference( "5541", "GASOLINE SERVICE STATIONS" ) );

					sortedSicReferences.Add( new SICReference( "5550", "BOAT DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5551", "BOAT DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5560", "RECREATIONAL VEHICLE DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5561", "RECREATIONAL VEHICLE DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5570", "MOTORCYCLE DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5571", "MOTORCYCLE DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "5590", "AUTOMOTIVE DEALERS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5599", "AUTOMOTIVE DEALERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5600", "RETAIL-APPAREL AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5610", "MEN'S AND BOYS' CLOTHING AND ACCESSORY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5611", "MEN'S AND BOYS' CLOTHING AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5620", "WOMEN'S CLOTHING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5621", "RETAIL-WOMEN'S CLOTHING STORES" ) );

					sortedSicReferences.Add( new SICReference( "5630", "WOMEN'S ACCESSORY AND SPECIALTY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5632", "WOMEN'S ACCESSORY AND SPECIALTY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5640", "CHILDREN'S AND INFANTS' WEAR STORES" ) );
					sortedSicReferences.Add( new SICReference( "5641", "CHILDREN'S AND INFANTS' WEAR STORES" ) );

					sortedSicReferences.Add( new SICReference( "5650", "FAMILY CLOTHING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5651", "RETAIL-FAMILY CLOTHING STORES" ) );

					sortedSicReferences.Add( new SICReference( "5660", "SHOE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5661", "RETAIL-SHOE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5690", "MISCELLANEOUS APPAREL AND ACCESSORY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5699", "MISCELLANEOUS APPAREL AND ACCESSORY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5700", "RETAIL-HOME FURNITURE, FURNISHINGS AND EQUIPMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5710", "HOME FURNITURE AND FURNISHINGS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5712", "RETAIL-FURNITURE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5713", "FLOOR COVERING STORES" ) );
					sortedSicReferences.Add( new SICReference( "5714", "DRAPERY, CURTAIN, AND UPHOLSTERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5719", "MISCELLANEOUS HOMEFURNISHINGS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5720", "HOUSEHOLD APPLIANCE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5722", "HOUSEHOLD APPLIANCE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5730", "RADIO, TELEVISION, CONSUMER ELECTRONICS, AND MUSIC STORES" ) );
					sortedSicReferences.Add( new SICReference( "5731", "RADIO, TELEVISION, AND CONSUMER ELECTRONICS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5734", "RETAIL-COMPUTER AND COMPUTER SOFTWARE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5735", "RETAIL-RECORD AND PRERECORDED TAPE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5736", "MUSICAL INSTRUMENT STORES" ) );

					sortedSicReferences.Add( new SICReference( "5810", "RETAIL-EATING AND DRINKING PLACES" ) );
					sortedSicReferences.Add( new SICReference( "5812", "RETAIL-EATING PLACES" ) );
					sortedSicReferences.Add( new SICReference( "5813", "DRINKING PLACES (ALCOHOLIC BEVERAGES)" ) );

					sortedSicReferences.Add( new SICReference( "5900", "RETAIL-MISCELLANEOUS RETAIL" ) );

					sortedSicReferences.Add( new SICReference( "5910", "DRUG STORES AND PROPRIETARY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5912", "RETAIL-DRUG STORES AND PROPRIETARY STORES" ) );

					sortedSicReferences.Add( new SICReference( "5920", "LIQUOR STORES" ) );
					sortedSicReferences.Add( new SICReference( "5921", "LIQUOR STORES" ) );

					sortedSicReferences.Add( new SICReference( "5930", "USED MERCHANDISE STORES" ) );
					sortedSicReferences.Add( new SICReference( "5932", "USED MERCHANDISE STORES" ) );

					sortedSicReferences.Add( new SICReference( "5940", "RETAIL-MISCELLANEOUS SHOPPING GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5941", "SPORTING GOODS STORES AND BICYCLE SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5942", "BOOK STORES" ) );
					sortedSicReferences.Add( new SICReference( "5943", "STATIONERY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5944", "RETAIL-JEWELRY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5945", "RETAIL-HOBBY, TOY AND GAME SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5946", "CAMERA AND PHOTOGRAPHIC SUPPLY STORES" ) );
					sortedSicReferences.Add( new SICReference( "5947", "GIFT, NOVELTY, AND SOUVENIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "5948", "LUGGAGE AND LEATHER GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5949", "SEWING, NEEDLEWORK, AND PIECE GOODS STORES" ) );

					sortedSicReferences.Add( new SICReference( "5960", "RETAIL-NONSTORE RETAILERS" ) );
					sortedSicReferences.Add( new SICReference( "5961", "RETAIL-CATALOG AND MAIL-ORDER HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "5962", "AUTOMATIC MERCHANDISING MACHINE OPERATORS" ) );
					sortedSicReferences.Add( new SICReference( "5963", "DIRECT SELLING ESTABLISHMENTS" ) );

					sortedSicReferences.Add( new SICReference( "5980", "FUEL DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5983", "FUEL OIL DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5984", "LIQUEFIED PETROLEUM GAS (BOTTLED GAS) DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "5989", "FUEL DEALERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "5990", "RETAIL-RETAIL STORES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "5992", "FLORISTS" ) );
					sortedSicReferences.Add( new SICReference( "5993", "TOBACCO STORES AND STANDS" ) );
					sortedSicReferences.Add( new SICReference( "5994", "NEWS DEALERS AND NEWSSTANDS" ) );
					sortedSicReferences.Add( new SICReference( "5995", "OPTICAL GOODS STORES" ) );
					sortedSicReferences.Add( new SICReference( "5999", "MISCELLANEOUS RETAIL STORES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6010", "CENTRAL RESERVE DEPOSITORY INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6011", "FEDERAL RESERVE BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6019", "CENTRAL RESERVE DEPOSITORY INSTITUTIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6020", "COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6021", "NATIONAL COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6022", "STATE COMMERCIAL BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6029", "COMMERCIAL BANKS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6030", "SAVINGS INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6035", "SAVINGS INSTITUTIONS, FEDERALLY CHARTERED" ) );
					sortedSicReferences.Add( new SICReference( "6036", "SAVINGS INSTITUTIONS, NOT FEDERALLY CHARTERED" ) );

					sortedSicReferences.Add( new SICReference( "6060", "CREDIT UNIONS" ) );
					sortedSicReferences.Add( new SICReference( "6061", "CREDIT UNIONS, FEDERALLY CHARTERED" ) );
					sortedSicReferences.Add( new SICReference( "6062", "CREDIT UNIONS, NOT FEDERALLY CHARTERED" ) );

					sortedSicReferences.Add( new SICReference( "6080", "FOREIGN BANKING AND BRANCHES AND AGENCIES OF FOREIGN BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6081", "BRANCHES AND AGENCIES OF FOREIGN BANKS" ) );
					sortedSicReferences.Add( new SICReference( "6082", "FOREIGN TRADE AND INTERNATIONAL BANKING INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6090", "FUNCTIONS RELATED TO DEPOSITORY BANKING" ) );
					sortedSicReferences.Add( new SICReference( "6091", "NONDEPOSIT TRUST FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "6099", "FUNCTIONS RELATED TO DEPOSITORY BANKING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6110", "FEDERAL AND FEDERALLY-SPONSORED CREDIT AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "6111", "FEDERAL AND FEDERALLY-SPONSORED CREDIT AGENCIES" ) );

					sortedSicReferences.Add( new SICReference( "6140", "PERSONAL CREDIT INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6141", "PERSONAL CREDIT INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6150", "BUSINESS CREDIT INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "6153", "SHORT-TERM BUSINESS CREDIT INSTITUTIONS, EXCEPT AGRICULTURAL" ) );
					sortedSicReferences.Add( new SICReference( "6159", "MISCELLANEOUS BUSINESS CREDIT INSTITUTIONS" ) );

					sortedSicReferences.Add( new SICReference( "6160", "MORTGAGE BANKERS AND BROKERS" ) );
					sortedSicReferences.Add( new SICReference( "6162", "MORTGAGE BANKERS AND LOAN CORRESPONDENTS" ) );
					sortedSicReferences.Add( new SICReference( "6163", "LOAN BROKERS" ) );

					sortedSicReferences.Add( new SICReference( "6172", "FINANCE LESSORS" ) );

					sortedSicReferences.Add( new SICReference( "6189", "ASSET-BACKED SECURITIES" ) );

					sortedSicReferences.Add( new SICReference( "6199", "FINANCE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "6200", "SECURITY AND COMMODITY BROKERS, DEALERS, EXCHANGES AND SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "6210", "SECURITY BROKERS, DEALERS, AND FLOTATION COMPANIES" ) );
					sortedSicReferences.Add( new SICReference( "6211", "SECURITY BROKERS, DEALERS, AND FLOTATION COMPANIES" ) );

					sortedSicReferences.Add( new SICReference( "6220", "COMMODITY CONTRACTS BROKERS AND DEALERS" ) );
					sortedSicReferences.Add( new SICReference( "6221", "COMMODITY CONTRACTS BROKERS AND DEALERS" ) );

					sortedSicReferences.Add( new SICReference( "6230", "SECURITY AND COMMODITY EXCHANGES" ) );
					sortedSicReferences.Add( new SICReference( "6231", "SECURITY AND COMMODITY EXCHANGES" ) );

					sortedSicReferences.Add( new SICReference( "6280", "SERVICES ALLIED WITH THE EXCHANGE OF SECURITIES OR COMMODITIES" ) );
					sortedSicReferences.Add( new SICReference( "6282", "INVESTMENT ADVICE" ) );
					sortedSicReferences.Add( new SICReference( "6289", "SERVICES ALLIED WITH THE EXCHANGE OF SECURITIES OR COMMODITIES, N" ) );

					sortedSicReferences.Add( new SICReference( "6310", "LIFE INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6311", "LIFE INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6320", "ACCIDENT AND HEALTH INSURANCE AND MEDICAL SERVICE PLANS" ) );
					sortedSicReferences.Add( new SICReference( "6321", "ACCIDENT AND HEALTH INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6324", "HOSPITAL AND MEDICAL SERVICE PLANS" ) );

					sortedSicReferences.Add( new SICReference( "6330", "FIRE, MARINE, AND CASUALTY INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6331", "FIRE, MARINE, AND CASUALTY INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6350", "SURETY INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6351", "SURETY INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6360", "TITLE INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "6361", "TITLE INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "6370", "PENSION, HEALTH, AND WELFARE FUNDS" ) );
					sortedSicReferences.Add( new SICReference( "6371", "PENSION, HEALTH, AND WELFARE FUNDS" ) );

					sortedSicReferences.Add( new SICReference( "6390", "INSURANCE CARRIERS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "6399", "INSURANCE CARRIERS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6410", "INSURANCE AGENTS, BROKERS, AND SERVICE" ) );
					sortedSicReferences.Add( new SICReference( "6411", "INSURANCE AGENTS, BROKERS, AND SERVICE" ) );

					sortedSicReferences.Add( new SICReference( "6500", "REAL ESTATE" ) );

					sortedSicReferences.Add( new SICReference( "6510", "REAL ESTATE OPERATORS (EXCEPT DEVELOPERS) AND LESSORS" ) );
					sortedSicReferences.Add( new SICReference( "6512", "OPERATORS OF NONRESIDENTIAL BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6513", "OPERATORS OF APARTMENT BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6514", "OPERATORS OF DWELLINGS OTHER THAN APARTMENT BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "6515", "OPERATORS OF RESIDENTIAL MOBILE HOME SITES" ) );
					sortedSicReferences.Add( new SICReference( "6517", "LESSORS OF RAILROAD PROPERTY" ) );
					sortedSicReferences.Add( new SICReference( "6519", "LESSORS OF REAL PROPERTY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6530", "REAL ESTATE AGENTS AND MANAGERS" ) );
					sortedSicReferences.Add( new SICReference( "6531", "REAL ESTATE AGENTS AND MANAGERS (FOR OTHERS)" ) );
					sortedSicReferences.Add( new SICReference( "6532", "REAL ESTATE DEALERS (FOR THEIR OWN ACCOUNT)" ) );

					sortedSicReferences.Add( new SICReference( "6540", "TITLE ABSTRACT OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6541", "TITLE ABSTRACT OFFICES" ) );

					sortedSicReferences.Add( new SICReference( "6550", "LAND SUBDIVIDERS AND DEVELOPERS" ) );
					sortedSicReferences.Add( new SICReference( "6552", "LAND SUBDIVIDERS AND DEVELOPERS, EXCEPT CEMETERIES" ) );
					sortedSicReferences.Add( new SICReference( "6553", "CEMETERY SUBDIVIDERS AND DEVELOPERS" ) );

					sortedSicReferences.Add( new SICReference( "6710", "HOLDING OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6712", "OFFICES OF BANK HOLDING COMPANIES" ) );
					sortedSicReferences.Add( new SICReference( "6719", "OFFICES OF HOLDING COMPANIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "6720", "INVESTMENT OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "6722", "MANAGEMENT INVESTMENT OFFICES, OPEN-END" ) );
					sortedSicReferences.Add( new SICReference( "6726", "UNIT INVESTMENT TRUSTS, FACE-AMOUNT CERTIFICATE OFFICES, AND CLOS" ) );

					sortedSicReferences.Add( new SICReference( "6730", "TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6732", "EDUCATIONAL, RELIGIOUS, AND CHARITABLE TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6733", "TRUSTS, EXCEPT EDUCATIONAL, RELIGIOUS, AND CHARITABLE" ) );

					sortedSicReferences.Add( new SICReference( "6770", "BLANK CHECKS" ) );

					sortedSicReferences.Add( new SICReference( "6790", "MISCELLANEOUS INVESTING" ) );
					sortedSicReferences.Add( new SICReference( "6792", "OIL ROYALTY TRADERS" ) );
					sortedSicReferences.Add( new SICReference( "6794", "PATENT OWNERS AND LESSORS" ) );
					sortedSicReferences.Add( new SICReference( "6795", "MINERAL ROYALTY TRADERS" ) );
					sortedSicReferences.Add( new SICReference( "6798", "REAL ESTATE INVESTMENT TRUSTS" ) );
					sortedSicReferences.Add( new SICReference( "6799", "INVESTORS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7000", "HOTELS, ROOMING HOUSES, CAMPS AND OTHER LODGING PLACES" ) );

					sortedSicReferences.Add( new SICReference( "7010", "HOTELS AND MOTELS" ) );
					sortedSicReferences.Add( new SICReference( "7011", "HOTELS AND MOTELS" ) );

					sortedSicReferences.Add( new SICReference( "7020", "ROOMING AND BOARDING HOUSES" ) );
					sortedSicReferences.Add( new SICReference( "7021", "ROOMING AND BOARDING HOUSES" ) );

					sortedSicReferences.Add( new SICReference( "7030", "CAMPS AND RECREATIONAL VEHICLE PARKS" ) );
					sortedSicReferences.Add( new SICReference( "7032", "SPORTING AND RECREATIONAL CAMPS" ) );
					sortedSicReferences.Add( new SICReference( "7033", "RECREATIONAL VEHICLE PARKS AND CAMPSITES" ) );

					sortedSicReferences.Add( new SICReference( "7040", "ORGANIZATION HOTELS AND LODGING HOUSES, ON MEMBERSHIP BASIS" ) );
					sortedSicReferences.Add( new SICReference( "7041", "ORGANIZATION HOTELS AND LODGING HOUSES, ON MEMBERSHIP BASIS" ) );

					sortedSicReferences.Add( new SICReference( "7200", "PERSONAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7210", "LAUNDRY, CLEANING, AND GARMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7211", "POWER LAUNDRIES, FAMILY AND COMMERCIAL" ) );
					sortedSicReferences.Add( new SICReference( "7212", "GARMENT PRESSING, AND AGENTS FOR LAUNDRIES AND DRYCLEANERS" ) );
					sortedSicReferences.Add( new SICReference( "7213", "LINEN SUPPLY" ) );
					sortedSicReferences.Add( new SICReference( "7215", "COIN-OPERATED LAUNDRIES AND DRYCLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7216", "DRYCLEANING PLANTS, EXCEPT RUG CLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7217", "CARPET AND UPHOLSTERY CLEANING" ) );
					sortedSicReferences.Add( new SICReference( "7218", "INDUSTRIAL LAUNDERERS" ) );
					sortedSicReferences.Add( new SICReference( "7219", "LAUNDRY AND GARMENT SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7220", "PHOTOGRAPHIC STUDIOS, PORTRAIT" ) );
					sortedSicReferences.Add( new SICReference( "7221", "PHOTOGRAPHIC STUDIOS, PORTRAIT" ) );

					sortedSicReferences.Add( new SICReference( "7230", "BEAUTY SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7231", "BEAUTY SHOPS" ) );

					sortedSicReferences.Add( new SICReference( "7240", "BARBER SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7241", "BARBER SHOPS" ) );

					sortedSicReferences.Add( new SICReference( "7250", "SHOE REPAIR SHOPS AND SHOESHINE PARLORS" ) );
					sortedSicReferences.Add( new SICReference( "7251", "SHOE REPAIR SHOPS AND SHOESHINE PARLORS" ) );

					sortedSicReferences.Add( new SICReference( "7260", "FUNERAL SERVICE AND CREMATORIES" ) );
					sortedSicReferences.Add( new SICReference( "7261", "FUNERAL SERVICE AND CREMATORIES" ) );

					sortedSicReferences.Add( new SICReference( "7290", "MISCELLANEOUS PERSONAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7291", "TAX RETURN PREPARATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7299", "MISCELLANEOUS PERSONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7310", "ADVERTISING" ) );
					sortedSicReferences.Add( new SICReference( "7311", "ADVERTISING AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "7312", "OUTDOOR ADVERTISING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7313", "RADIO, TELEVISION, AND PUBLISHERS' ADVERTISING REPRESENTATIVES" ) );
					sortedSicReferences.Add( new SICReference( "7319", "ADVERTISING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7320", "CONSUMER CREDIT REPORTING AGENCIES, MERCANTILE REPORTING AGENCIES," ) );
					sortedSicReferences.Add( new SICReference( "7322", "ADJUSTMENT AND COLLECTION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7323", "CREDIT REPORTING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7330", "MAILING, REPRODUCTION, COMMERCIAL ART AND PHOTOGRAPHY, AND STENOGR" ) );
					sortedSicReferences.Add( new SICReference( "7331", "DIRECT MAIL ADVERTISING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7334", "PHOTOCOPYING AND DUPLICATING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7335", "COMMERCIAL PHOTOGRAPHY" ) );
					sortedSicReferences.Add( new SICReference( "7336", "COMMERCIAL ART AND GRAPHIC DESIGN" ) );
					sortedSicReferences.Add( new SICReference( "7338", "SECRETARIAL AND COURT REPORTING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7340", "SERVICES TO DWELLINGS AND OTHER BUILDINGS" ) );
					sortedSicReferences.Add( new SICReference( "7342", "DISINFECTING AND PEST CONTROL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7349", "BUILDING CLEANING AND MAINTENANCE SERVICES, NOT ELSEWHERE CLASSIF" ) );

					sortedSicReferences.Add( new SICReference( "7350", "MISCELLANEOUS EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7352", "MEDICAL EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7353", "HEAVY CONSTRUCTION EQUIPMENT RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7359", "EQUIPMENT RENTAL AND LEASING, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7360", "PERSONNEL SUPPLY SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7361", "EMPLOYMENT AGENCIES" ) );
					sortedSicReferences.Add( new SICReference( "7363", "HELP SUPPLY SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7370", "COMPUTER PROGRAMMING, DATA PROCESSING, AND OTHER COMPUTER RELATED" ) );
					sortedSicReferences.Add( new SICReference( "7371", "COMPUTER PROGRAMMING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7372", "PREPACKAGED SOFTWARE" ) );
					sortedSicReferences.Add( new SICReference( "7373", "COMPUTER INTEGRATED SYSTEMS DESIGN" ) );
					sortedSicReferences.Add( new SICReference( "7374", "COMPUTER PROCESSING AND DATA PREPARATION AND PROCESSING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7375", "INFORMATION RETRIEVAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7376", "COMPUTER FACILITIES MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7377", "COMPUTER RENTAL AND LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7378", "COMPUTER MAINTENANCE AND REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7379", "COMPUTER RELATED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7380", "MISCELLANEOUS BUSINESS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7381", "DETECTIVE, GUARD, AND ARMORED CAR SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7382", "SECURITY SYSTEMS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7383", "NEWS SYNDICATES" ) );
					sortedSicReferences.Add( new SICReference( "7384", "PHOTOFINISHING LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "7385", "TELEPHONE INTERCONNECT SYSTEMS" ) );
					sortedSicReferences.Add( new SICReference( "7389", "BUSINESS SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7500", "AUTOMOTIVE REPAIR, SERVICES AND PARKING" ) );

					sortedSicReferences.Add( new SICReference( "7510", "AUTOMOTIVE RENTAL AND LEASING, WITHOUT DRIVERS" ) );
					sortedSicReferences.Add( new SICReference( "7513", "TRUCK RENTAL AND LEASING, WITHOUT DRIVERS" ) );
					sortedSicReferences.Add( new SICReference( "7514", "PASSENGER CAR RENTAL" ) );
					sortedSicReferences.Add( new SICReference( "7515", "PASSENGER CAR LEASING" ) );
					sortedSicReferences.Add( new SICReference( "7519", "UTILITY TRAILER AND RECREATIONAL VEHICLE RENTAL" ) );

					sortedSicReferences.Add( new SICReference( "7520", "AUTOMOBILE PARKING" ) );
					sortedSicReferences.Add( new SICReference( "7521", "AUTOMOBILE PARKING" ) );

					sortedSicReferences.Add( new SICReference( "7530", "AUTOMOTIVE REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7532", "TOP, BODY, AND UPHOLSTERY REPAIR SHOPS AND PAINT SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7533", "AUTOMOTIVE EXHAUST SYSTEM REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7534", "TIRE RETREADING AND REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7536", "AUTOMOTIVE GLASS REPLACEMENT SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7537", "AUTOMOTIVE TRANSMISSION REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7538", "GENERAL AUTOMOTIVE REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7539", "AUTOMOTIVE REPAIR SHOPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7540", "AUTOMOTIVE SERVICES, EXCEPT REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7542", "CARWASHES" ) );
					sortedSicReferences.Add( new SICReference( "7549", "AUTOMOTIVE SERVICES, EXCEPT REPAIR AND CARWASHES" ) );

					sortedSicReferences.Add( new SICReference( "7600", "MISCELLANEOUS REPAIR SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7620", "ELECTRICAL REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7622", "RADIO AND TELEVISION REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7623", "REFRIGERATION AND AIR-CONDITIONING SERVICE AND REPAIR SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7629", "ELECTRICAL AND ELECTRONIC REPAIR SHOPS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7630", "WATCH, CLOCK, AND JEWELRY REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7631", "WATCH, CLOCK, AND JEWELRY REPAIR" ) );

					sortedSicReferences.Add( new SICReference( "7640", "REUPHOLSTERY AND FURNITURE REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7641", "REUPHOLSTERY AND FURNITURE REPAIR" ) );

					sortedSicReferences.Add( new SICReference( "7690", "MISCELLANEOUS REPAIR SHOPS AND RELATED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7692", "WELDING REPAIR" ) );
					sortedSicReferences.Add( new SICReference( "7694", "ARMATURE REWINDING SHOPS" ) );
					sortedSicReferences.Add( new SICReference( "7699", "REPAIR SHOPS AND RELATED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "7810", "MOTION PICTURE PRODUCTION AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7812", "MOTION PICTURE AND VIDEO TAPE PRODUCTION" ) );
					sortedSicReferences.Add( new SICReference( "7819", "SERVICES ALLIED TO MOTION PICTURE PRODUCTION" ) );

					sortedSicReferences.Add( new SICReference( "7820", "MOTION PICTURE DISTRIBUTION AND ALLIED SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7822", "MOTION PICTURE AND VIDEO TAPE DISTRIBUTION" ) );
					sortedSicReferences.Add( new SICReference( "7829", "SERVICES ALLIED TO MOTION PICTURE DISTRIBUTION" ) );

					sortedSicReferences.Add( new SICReference( "7830", "MOTION PICTURE THEATERS" ) );
					sortedSicReferences.Add( new SICReference( "7832", "MOTION PICTURE THEATERS, EXCEPT DRIVE-IN" ) );
					sortedSicReferences.Add( new SICReference( "7833", "DRIVE-IN MOTION PICTURE THEATERS" ) );

					sortedSicReferences.Add( new SICReference( "7840", "VIDEO TAPE RENTAL" ) );
					sortedSicReferences.Add( new SICReference( "7841", "VIDEO TAPE RENTAL" ) );

					sortedSicReferences.Add( new SICReference( "7900", "AMUSEMENT AND RECREATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "7910", "DANCE STUDIOS, SCHOOLS, AND HALLS" ) );
					sortedSicReferences.Add( new SICReference( "7911", "DANCE STUDIOS, SCHOOLS, AND HALLS" ) );

					sortedSicReferences.Add( new SICReference( "7920", "THEATRICAL PRODUCERS (EXCEPT MOTION PICTURE), BANDS, ORCHESTRAS, A" ) );
					sortedSicReferences.Add( new SICReference( "7922", "THEATRICAL PRODUCERS (EXCEPT MOTION PICTURE) AND MISCELLANEOUS TH" ) );
					sortedSicReferences.Add( new SICReference( "7929", "BANDS, ORCHESTRAS, ACTORS, AND OTHER ENTERTAINERS AND ENTERTAINME" ) );

					sortedSicReferences.Add( new SICReference( "7930", "BOWLING CENTERS" ) );
					sortedSicReferences.Add( new SICReference( "7933", "BOWLING CENTERS" ) );

					sortedSicReferences.Add( new SICReference( "7940", "COMMERCIAL SPORTS" ) );
					sortedSicReferences.Add( new SICReference( "7941", "PROFESSIONAL SPORTS CLUBS AND PROMOTERS" ) );
					sortedSicReferences.Add( new SICReference( "7948", "RACING, INCLUDING TRACK OPERATION" ) );

					sortedSicReferences.Add( new SICReference( "7990", "MISCELLANEOUS AMUSEMENT AND RECREATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "7991", "PHYSICAL FITNESS FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "7992", "PUBLIC GOLF COURSES" ) );
					sortedSicReferences.Add( new SICReference( "7993", "COIN-OPERATED AMUSEMENT DEVICES" ) );
					sortedSicReferences.Add( new SICReference( "7996", "AMUSEMENT PARKS" ) );
					sortedSicReferences.Add( new SICReference( "7997", "MEMBERSHIP SPORTS AND RECREATION CLUBS" ) );
					sortedSicReferences.Add( new SICReference( "7999", "AMUSEMENT AND RECREATION SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8000", "HEALTH SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8010", "OFFICES AND CLINICS OF DOCTORS OF MEDICINE" ) );
					sortedSicReferences.Add( new SICReference( "8011", "OFFICES AND CLINICS OF DOCTORS OF MEDICINE" ) );

					sortedSicReferences.Add( new SICReference( "8020", "OFFICES AND CLINICS OF DENTISTS" ) );
					sortedSicReferences.Add( new SICReference( "8021", "OFFICES AND CLINICS OF DENTISTS" ) );

					sortedSicReferences.Add( new SICReference( "8030", "OFFICES AND CLINICS OF DOCTORS OF OSTEOPATHY" ) );
					sortedSicReferences.Add( new SICReference( "8031", "OFFICES AND CLINICS OF DOCTORS OF OSTEOPATHY" ) );

					sortedSicReferences.Add( new SICReference( "8040", "OFFICES AND CLINICS OF OTHER HEALTH PRACTITIONERS" ) );
					sortedSicReferences.Add( new SICReference( "8041", "OFFICES AND CLINICS OF CHIROPRACTORS" ) );
					sortedSicReferences.Add( new SICReference( "8042", "OFFICES AND CLINICS OF OPTOMETRISTS" ) );
					sortedSicReferences.Add( new SICReference( "8043", "OFFICES AND CLINICS OF PODIATRISTS" ) );
					sortedSicReferences.Add( new SICReference( "8049", "OFFICES AND CLINICS OF HEALTH PRACTITIONERS, NOT ELSEWHERE CLASSI" ) );

					sortedSicReferences.Add( new SICReference( "8050", "NURSING AND PERSONAL CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8051", "SKILLED NURSING CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8052", "INTERMEDIATE CARE FACILITIES" ) );
					sortedSicReferences.Add( new SICReference( "8059", "NURSING AND PERSONAL CARE FACILITIES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8060", "HOSPITALS" ) );
					sortedSicReferences.Add( new SICReference( "8062", "GENERAL MEDICAL AND SURGICAL HOSPITALS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8063", "PSYCHIATRIC HOSPITALS" ) );
					sortedSicReferences.Add( new SICReference( "8069", "SPECIALTY HOSPITALS, EXCEPT PSYCHIATRIC" ) );

					sortedSicReferences.Add( new SICReference( "8070", "MEDICAL AND DENTAL LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "8071", "MEDICAL LABORATORIES" ) );
					sortedSicReferences.Add( new SICReference( "8072", "DENTAL LABORATORIES" ) );

					sortedSicReferences.Add( new SICReference( "8080", "HOME HEALTH CARE SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8082", "HOME HEALTH CARE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8090", "MISCELLANEOUS HEALTH AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8092", "KIDNEY DIALYSIS CENTERS" ) );
					sortedSicReferences.Add( new SICReference( "8093", "SPECIALTY OUTPATIENT FACILITIES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8099", "HEALTH AND ALLIED SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8110", "LEGAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8111", "LEGAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8200", "EDUCATIONAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8210", "ELEMENTARY AND SECONDARY SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8211", "ELEMENTARY AND SECONDARY SCHOOLS" ) );

					sortedSicReferences.Add( new SICReference( "8220", "COLLEGES, UNIVERSITIES, PROFESSIONAL SCHOOLS, AND JUNIOR COLLEGES" ) );
					sortedSicReferences.Add( new SICReference( "8221", "COLLEGES, UNIVERSITIES, AND PROFESSIONAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8222", "JUNIOR COLLEGES AND TECHNICAL INSTITUTES" ) );

					sortedSicReferences.Add( new SICReference( "8230", "LIBRARIES" ) );
					sortedSicReferences.Add( new SICReference( "8231", "LIBRARIES" ) );

					sortedSicReferences.Add( new SICReference( "8240", "VOCATIONAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8243", "DATA PROCESSING SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8244", "BUSINESS AND SECRETARIAL SCHOOLS" ) );
					sortedSicReferences.Add( new SICReference( "8249", "VOCATIONAL SCHOOLS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8290", "SCHOOLS AND EDUCATIONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8299", "SCHOOLS AND EDUCATIONAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8300", "SOCIAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8320", "INDIVIDUAL AND FAMILY SOCIAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8322", "INDIVIDUAL AND FAMILY SOCIAL SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8330", "JOB TRAINING AND VOCATIONAL REHABILITATION SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8331", "JOB TRAINING AND VOCATIONAL REHABILITATION SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8350", "CHILD DAY CARE SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8351", "CHILD DAY CARE SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8360", "RESIDENTIAL CARE" ) );
					sortedSicReferences.Add( new SICReference( "8361", "RESIDENTIAL CARE" ) );

					sortedSicReferences.Add( new SICReference( "8390", "SOCIAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8399", "SOCIAL SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8410", "MUSEUMS AND ART GALLERIES" ) );
					sortedSicReferences.Add( new SICReference( "8412", "MUSEUMS AND ART GALLERIES" ) );

					sortedSicReferences.Add( new SICReference( "8420", "ARBORETA AND BOTANICAL OR ZOOLOGICAL GARDENS" ) );
					sortedSicReferences.Add( new SICReference( "8422", "ARBORETA AND BOTANICAL OR ZOOLOGICAL GARDENS" ) );

					sortedSicReferences.Add( new SICReference( "8600", "MEMBERSHIP ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8610", "BUSINESS ASSOCIATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8611", "BUSINESS ASSOCIATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8620", "PROFESSIONAL MEMBERSHIP ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8621", "PROFESSIONAL MEMBERSHIP ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8630", "LABOR UNIONS AND SIMILAR LABOR ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8631", "LABOR UNIONS AND SIMILAR LABOR ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8640", "CIVIC, SOCIAL, AND FRATERNAL ASSOCIATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8641", "CIVIC, SOCIAL, AND FRATERNAL ASSOCIATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8650", "POLITICAL ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8651", "POLITICAL ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8660", "RELIGIOUS ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8661", "RELIGIOUS ORGANIZATIONS" ) );

					sortedSicReferences.Add( new SICReference( "8690", "MEMBERSHIP ORGANIZATIONS, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8699", "MEMBERSHIP ORGANIZATIONS, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8700", "ENGINEERING, ACCOUNTING, RESEARCH, MANAGEMENT" ) );

					sortedSicReferences.Add( new SICReference( "8710", "ENGINEERING, ARCHITECTURAL, AND SURVEYING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8711", "ENGINEERING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8712", "ARCHITECTURAL SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8713", "SURVEYING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8720", "ACCOUNTING, AUDITING, AND BOOKKEEPING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8721", "ACCOUNTING, AUDITING, AND BOOKKEEPING SERVICES" ) );

					sortedSicReferences.Add( new SICReference( "8730", "RESEARCH, DEVELOPMENT, AND TESTING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8731", "COMMERCIAL PHYSICAL AND BIOLOGICAL RESEARCH" ) );

					sortedSicReferences.Add( new SICReference( "8732", "COMMERCIAL ECONOMIC, SOCIOLOGICAL, AND EDUCATIONAL RESEARCH" ) );
					sortedSicReferences.Add( new SICReference( "8733", "NONCOMMERCIAL RESEARCH ORGANIZATIONS" ) );
					sortedSicReferences.Add( new SICReference( "8734", "TESTING LABORATORIES" ) );

					sortedSicReferences.Add( new SICReference( "8740", "MANAGEMENT AND PUBLIC RELATIONS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8741", "MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8742", "MANAGEMENT CONSULTING SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8743", "PUBLIC RELATIONS SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8744", "FACILITIES SUPPORT MANAGEMENT SERVICES" ) );
					sortedSicReferences.Add( new SICReference( "8748", "BUSINESS CONSULTING SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8810", "PRIVATE HOUSEHOLDS" ) );
					sortedSicReferences.Add( new SICReference( "8811", "PRIVATE HOUSEHOLDS" ) );

					sortedSicReferences.Add( new SICReference( "8880", "AMERICAN DEPOSITARY RECEIPTS" ) );
					sortedSicReferences.Add( new SICReference( "8888", "FOREIGN GOVERNMENTS" ) );

					sortedSicReferences.Add( new SICReference( "8900", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "8990", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "8999", "SERVICES, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9110", "EXECUTIVE OFFICES" ) );
					sortedSicReferences.Add( new SICReference( "9111", "EXECUTIVE OFFICES" ) );

					sortedSicReferences.Add( new SICReference( "9120", "LEGISLATIVE BODIES" ) );
					sortedSicReferences.Add( new SICReference( "9121", "LEGISLATIVE BODIES" ) );

					sortedSicReferences.Add( new SICReference( "9130", "EXECUTIVE AND LEGISLATIVE OFFICES COMBINED" ) );
					sortedSicReferences.Add( new SICReference( "9131", "EXECUTIVE AND LEGISLATIVE OFFICES COMBINED" ) );

					sortedSicReferences.Add( new SICReference( "9190", "GENERAL GOVERNMENT, NOT ELSEWHERE CLASSIFIED" ) );
					sortedSicReferences.Add( new SICReference( "9199", "GENERAL GOVERNMENT, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9210", "COURTS" ) );
					sortedSicReferences.Add( new SICReference( "9211", "COURTS" ) );

					sortedSicReferences.Add( new SICReference( "9220", "PUBLIC ORDER AND SAFETY" ) );
					sortedSicReferences.Add( new SICReference( "9221", "POLICE PROTECTION" ) );
					sortedSicReferences.Add( new SICReference( "9222", "LEGAL COUNSEL AND PROSECUTION" ) );
					sortedSicReferences.Add( new SICReference( "9223", "CORRECTIONAL INSTITUTIONS" ) );
					sortedSicReferences.Add( new SICReference( "9224", "FIRE PROTECTION" ) );
					sortedSicReferences.Add( new SICReference( "9229", "PUBLIC ORDER AND SAFETY, NOT ELSEWHERE CLASSIFIED" ) );

					sortedSicReferences.Add( new SICReference( "9310", "PUBLIC FINANCE, TAXATION, AND MONETARY POLICY" ) );
					sortedSicReferences.Add( new SICReference( "9311", "PUBLIC FINANCE, TAXATION, AND MONETARY POLICY" ) );

					sortedSicReferences.Add( new SICReference( "9410", "ADMINISTRATION OF EDUCATIONAL PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9411", "ADMINISTRATION OF EDUCATIONAL PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9430", "ADMINISTRATION OF PUBLIC HEALTH PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9431", "ADMINISTRATION OF PUBLIC HEALTH PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9440", "ADMINISTRATION OF SOCIAL, HUMAN RESOURCE AND INCOME MAINTENANCE PR" ) );
					sortedSicReferences.Add( new SICReference( "9441", "ADMINISTRATION OF SOCIAL, HUMAN RESOURCE AND INCOME MAINTENANCE PR" ) );

					sortedSicReferences.Add( new SICReference( "9450", "ADMINISTRATION OF VETERANS' AFFAIRS, EXCEPT HEALTH AND INSURANCE" ) );
					sortedSicReferences.Add( new SICReference( "9451", "ADMINISTRATION OF VETERANS' AFFAIRS, EXCEPT HEALTH AND INSURANCE" ) );

					sortedSicReferences.Add( new SICReference( "9510", "ADMINISTRATION OF ENVIRONMENTAL QUALITY PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9511", "AIR AND WATER RESOURCE AND SOLID WASTE MANAGEMENT" ) );
					sortedSicReferences.Add( new SICReference( "9512", "LAND, MINERAL, WILDLIFE, AND FOREST CONSERVATION" ) );

					sortedSicReferences.Add( new SICReference( "9530", "ADMINISTRATION OF HOUSING AND URBAN DEVELOPMENT PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9531", "ADMINISTRATION OF HOUSING PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9532", "ADMINISTRATION OF URBAN PLANNING AND COMMUNITY AND RURAL DEVELOPM" ) );

					sortedSicReferences.Add( new SICReference( "9610", "ADMINISTRATION OF GENERAL ECONOMIC PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9611", "ADMINISTRATION OF GENERAL ECONOMIC PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9620", "REGULATION AND ADMINISTRATION OF TRANSPORTATION PROGRAMS" ) );
					sortedSicReferences.Add( new SICReference( "9621", "REGULATION AND ADMINISTRATION OF TRANSPORTATION PROGRAMS" ) );

					sortedSicReferences.Add( new SICReference( "9630", "REGULATION AND ADMINISTRATION OF COMMUNICATIONS, ELECTRIC, GAS, AN" ) );
					sortedSicReferences.Add( new SICReference( "9631", "REGULATION AND ADMINISTRATION OF COMMUNICATIONS, ELECTRIC, GAS, A" ) );

					sortedSicReferences.Add( new SICReference( "9640", "REGULATION OF AGRICULTURAL MARKETING AND COMMODITIES" ) );
					sortedSicReferences.Add( new SICReference( "9641", "REGULATION OF AGRICULTURAL MARKETING AND COMMODITIES" ) );

					sortedSicReferences.Add( new SICReference( "9650", "REGULATION, LICENSING, AND INSPECTION OF MISCELLANEOUS COMMERCIAL" ) );
					sortedSicReferences.Add( new SICReference( "9651", "REGULATION, LICENSING, AND INSPECTION OF MISCELLANEOUS COMMERCIAL" ) );

					sortedSicReferences.Add( new SICReference( "9660", "SPACE RESEARCH AND TECHNOLOGY" ) );
					sortedSicReferences.Add( new SICReference( "9661", "SPACE RESEARCH AND TECHNOLOGY" ) );

					sortedSicReferences.Add( new SICReference( "9710", "NATIONAL SECURITY" ) );
					sortedSicReferences.Add( new SICReference( "9711", "NATIONAL SECURITY" ) );

					sortedSicReferences.Add( new SICReference( "9720", "INTERNATIONAL AFFAIRS" ) );
					sortedSicReferences.Add( new SICReference( "9721", "INTERNATIONAL AFFAIRS" ) );

					sortedSicReferences.Add( new SICReference( "9990", "NONCLASSIFIABLE ESTABLISHMENTS" ) );
					sortedSicReferences.Add( new SICReference( "9995", "NON-OPERATING ESTABLISHMENTS" ) );
					*/

					sortedSicReferences.Sort( SortSICReferenceByCode );
				}
			}
			return sortedSicReferences;
		}


		#endregion

	}
}
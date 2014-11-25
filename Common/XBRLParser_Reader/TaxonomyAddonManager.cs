using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Aucent.MAX.AXE.XBRLParser
{
    [Serializable]
    [Obfuscation(Exclude = true)]
	public class TaxonomyAddonManager
	{
		private static XmlSerializer serializer;

		public static string BasePath = string.Empty;

		/// <summary>
		/// The relative path to the Definition Files
		/// </summary>
		private string definitionPath = @"Taxonomy\Documentation";
		public string DefinitionPath
		{
			get { return definitionPath; }
			set { definitionPath = value; }
		}

		private string language = "en-US";
		public string Language
		{
			get { return language; }
		}

		/// <summary>
		/// The relative path to the References Files
		/// </summary>
		private string referencesPath = @"Taxonomy\Reference";
		public string ReferencesPath
		{
			get { return referencesPath; }
			set { referencesPath = value; }
		}
	

		private Dictionary<string, TaxonomyAddon> taxonomies = new Dictionary<string, TaxonomyAddon>();
		[XmlIgnore]
		public Dictionary<string, TaxonomyAddon> Taxonomies
		{
			get { return this.taxonomies; }
			set { this.taxonomies = value; }
		}

		public TaxonomyAddon[] TaxonomyList
		{
			get
			{
				List<TaxonomyAddon> list = new List<TaxonomyAddon>();
				foreach( TaxonomyAddon sr in this.taxonomies.Values )
				{
					list.Add( sr );
				}
				return list.ToArray();
			}
			set
			{
				foreach( TaxonomyAddon t in value )
				{
					this.taxonomies[ t.Taxonomy ] = t;
				}
			}
		}

		public TaxonomyAddonManager()
		{
		}

		public TaxonomyAddonManager( string language )
		{
			this.language = language;
		}

		public void SetDefinitionPath( string path )
		{
			this.definitionPath = path;
		}

		public void SetReferencesPath( string path ){
			this.referencesPath = path;
		}

		private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> definitionCache = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
		//For the moment, we are ignoring the language
		public string GetDefinition( string taxonomy, string elementID, string language )
		{
			return this.GetDefinition( taxonomy, elementID );
		}

		public string GetDefinition( string taxonomy, string elementID )
		{
			taxonomy = Path.GetFileName( taxonomy );

			if( !this.Taxonomies.ContainsKey( taxonomy ) )
				return string.Empty;

			if( !definitionCache.ContainsKey( this.Language ) )
				definitionCache[ this.Language ] = new Dictionary<string, Dictionary<string, string>>();

			if( !definitionCache[ this.Language ].ContainsKey( taxonomy ) )
			{
				string basePath = Path.Combine( GetBasePath(), DefinitionPath );
				Dictionary<string, string> tmp = definitionCache[ this.Language ][ taxonomy ] = new Dictionary<string, string>();
				foreach( string definitionFile in this.Taxonomies[ taxonomy ].DefinitionFiles )
				{
					string err;
					string file = Path.Combine( basePath, definitionFile );
					if( !Taxonomy.TryGetDocumentationInformation( this.Language, file, ref tmp, out err ) )
					{

					}
				}
			}

			if( definitionCache[ this.Language ][ taxonomy ].ContainsKey( elementID ) )
				return definitionCache[ this.Language ][ taxonomy ][ elementID ];

			return string.Empty;
		}

		/**
		 * language => taxonomy => element => value
		 **/
		private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> referenceCache = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
		public string GetReferences( string taxonomy, string elementID )
		{
			taxonomy = Path.GetFileName( taxonomy );

			if( !this.Taxonomies.ContainsKey( taxonomy ) )
				return string.Empty;

			if( !referenceCache.ContainsKey( this.Language ) )
				referenceCache[ this.Language ] = new Dictionary<string, Dictionary<string, string>>();

			if( !referenceCache[ this.Language ].ContainsKey( taxonomy ) )
			{
				string basePath = Path.Combine( GetBasePath(), ReferencesPath );
				Dictionary<string, string> tmp = referenceCache[ this.Language ][ taxonomy ] = new Dictionary<string, string>();
				foreach( string referenceFile in this.Taxonomies[ taxonomy ].ReferenceFiles )
				{
					string err;
					string file = Path.Combine( basePath, referenceFile );
					if( !Taxonomy.TryGetReferenceInformation( file, ref tmp, out err ) )
					{

					}
				}
			}

			if( referenceCache[ this.Language ][ taxonomy ].ContainsKey( elementID ) )
				return referenceCache[ this.Language ][ taxonomy ][ elementID ];

			return string.Empty;
		}



		public static bool TryAutoLoad( out TaxonomyAddonManager tam )
		{
			tam = null;
			string filePath = GetDefaultFilePath();
			TaxonomyAddonManager.TryLoadFile( filePath, out tam );

			return !( tam == null );
		}

		public static bool TryLoad( Stream stream, out TaxonomyAddonManager tam )
		{
			tam = null;
			if( serializer == null )
				serializer = new XmlSerializer( typeof( TaxonomyAddonManager ) );

			if( serializer == null )
				return false;

			try
			{
				tam = (TaxonomyAddonManager)TaxonomyAddonManager.serializer.Deserialize( stream );
				return true;
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.TargetSite.ToString() + ": " + ex.ToString() );
				Debug.WriteLine( ex.TargetSite.ToString() + ": " + ex.ToString() );
			}

			return false;
		}

		public static bool TryLoadFile( string filePath, out TaxonomyAddonManager tam )
		{
			tam = null;
			if( !File.Exists( filePath ) )
				return false;

			try
			{
				using( FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
				{
					return TaxonomyAddonManager.TryLoad( fs, out tam );
				}
			}
			catch{}

			return false;
		}

		public bool TryAutoSave()
		{
			string filePath = GetDefaultFilePath();
			return this.TrySaveFile( filePath );
		}

		public bool TrySave( Stream stream )
		{
			if( TaxonomyAddonManager.serializer == null )
				TaxonomyAddonManager.serializer = new XmlSerializer( typeof( TaxonomyAddonManager ) );

			if( TaxonomyAddonManager.serializer == null )
				return false;

			try
			{
				TaxonomyAddonManager.serializer.Serialize( stream, this );
				return true;
			}
			catch{}

			return false;
		}

		public bool TrySaveFile( string filePath )
		{
			try
			{
				using( FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write ) )
				{
					return this.TrySave( fs );
				}
			}
			catch{}

			return false;
		}

		private static string GetBasePath()
		{
			if( !string.IsNullOrEmpty( BasePath ) )
			{
				return BasePath;
			}
			else
			{
				string currentDLLName = System.Reflection.Assembly.GetExecutingAssembly().Location;
				string defaultDirName = System.IO.Path.GetDirectoryName( currentDLLName );
				return defaultDirName;				
			}
		}

		private static string GetDefaultFilePath()
		{
			string filePath = Path.Combine( GetBasePath(), "TaxonomyAddonManager.xml" );
			return filePath;
		}
	}

	[Serializable]
	[Obfuscation( Exclude = true )]
	public class TaxonomyAddon
	{
		private string taxonomy = string.Empty;
		public string Taxonomy
		{
			get { return this.taxonomy; }
			set { this.taxonomy = value; }
		}

		private List<string> definitionFiles = new List<string>();
		public List<string> DefinitionFiles
		{
			get { return definitionFiles; }
			set { definitionFiles = value; }
		}

		private List<string> referenceFiles = new List<string>();
		public List<string> ReferenceFiles
		{
			get { return referenceFiles; }
			set { referenceFiles = value; }
		}

		public TaxonomyAddon()
		{

		}

		public TaxonomyAddon( string taxonomy )
		{
			this.taxonomy = taxonomy;
		}
	}
}

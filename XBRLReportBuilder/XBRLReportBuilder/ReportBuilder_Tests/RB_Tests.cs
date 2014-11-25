using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Aucent.FilingServices.Data;
using System.Net.Cache;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Net;
using XBRLReportBuilder;
using Aucent.MAX.AXE.XBRLReportBuilder.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	[TestFixture]
	public partial class RB_Tests
	{
		#region init

		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp]
		public virtual void RunFirst()
		{
		}

		[TestFixtureTearDown]
		public virtual void RunLast()
		{
		}

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp]
		public virtual void RunBeforeEachTest()
		{
		}

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown]
		public virtual void RunAfterEachTest()
		{
		}

		#endregion

		[Test]
		public void DefaultSettings()
		{
			Test_Abstract.BuildAndVerifyWithoutResults = false;
			Test_Abstract.OutputFormat = ReportFormat.Xml;
			Test_Abstract.HtmlFormat = HtmlReportFormat.None;
			Test_Abstract.CachePolicy = RequestCacheLevel.Default;
			Test_Abstract.ResourcePath = null;
			Test_Abstract.UpdateResultsFlag = false;

			//IsConcurrentTestResult = false;
			Test_Abstract.ConcurrentTestsCount = 4;
			Test_Abstract.ConcurrentTestsRunning = false;
			Test_Abstract.ConcurrentTestsResults = new Dictionary<string, Exception>();
			Test_Abstract.RunAllConcurrentFlag = false;
		}

		[Test, Explicit]
		public void RemoveXmlOutput()
		{
            Test_Abstract.OutputFormat = Test_Abstract.OutputFormat ^ ReportFormat.Xml;
		}

		[Test, Explicit]
		public void RunAllConcurrent()
		{
			Type absType = typeof( Test_Abstract );
			MethodBase mi = MethodInfo.GetCurrentMethod();
			Type[] types = this.GetType().GetNestedTypes();
			foreach( Type t in types )
			{
				if( t.IsSubclassOf( absType ) )
				{
					MethodInfo mi2 = t.GetMethod( mi.Name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static );
					mi2.Invoke( null, new object[]{ t } );
				}
			}

            Test_Abstract.RunAllConcurrentFlag = true;
		}

		[Test, Explicit]
		public void SetBuildAndVerifyWithoutResults()
		{
            Test_Abstract.BuildAndVerifyWithoutResults = true;
		}

		[Test, Explicit]
		public void SetHtmlOutput_Complete()
		{
            Test_Abstract.OutputFormat = (Test_Abstract.OutputFormat & ReportFormat.Xml) | ReportFormat.Html;
            Test_Abstract.HtmlFormat = HtmlReportFormat.Complete;
		}

		[Test, Explicit]
		public void SetHtmlOutput_Fragment()
		{
            Test_Abstract.OutputFormat = (Test_Abstract.OutputFormat & ReportFormat.Xml) | ReportFormat.Html;
            Test_Abstract.HtmlFormat = HtmlReportFormat.Fragment;
		}

		[Test, Explicit]
		public void SetCachePolicy_CacheIfAvailable()
		{
            Test_Abstract.CachePolicy = RequestCacheLevel.CacheIfAvailable;
		}

		[Test, Explicit]
		public void SetCachePolicy_CacheOnly()
		{
            Test_Abstract.CachePolicy = RequestCacheLevel.CacheOnly;
		}

		[Test, Explicit]
		public void SetCachePolicy_Revalidate()
		{
            Test_Abstract.CachePolicy = RequestCacheLevel.Revalidate;
		}

		[STAThread]
		[Test, Explicit]
		public void SetResourcePath()
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			if( DialogResult.OK != fbd.ShowDialog() )
				return;

            Test_Abstract.ResourcePath = fbd.SelectedPath;
		}

		[STAThread]
		[Test, Explicit]
		public void Test_Any_File()
		{
			OpenFileDialog ofdBrowseInstance = new OpenFileDialog();
			ofdBrowseInstance.DefaultExt = "xml";
			ofdBrowseInstance.Filter = "XML Files (*.xml)|*.xml|XBRL Files (*.xbrl)|*.xbrl";
			ofdBrowseInstance.RestoreDirectory = true;
			ofdBrowseInstance.ShowReadOnly = true;
			ofdBrowseInstance.Title = "Select Instance Document";

			if( DialogResult.OK != ofdBrowseInstance.ShowDialog() )
				return;
			
			string txtInstancePath = ofdBrowseInstance.FileName;

			if( !File.Exists( txtInstancePath ) )
			{
				MessageBox.Show( "The path to the instance document could not be found.  Please edit the path, or browse to the file and try again." );
				return;
			}
			
			string txtTaxonomyPath;
			if( !RB_Tests.IsInstance( txtInstancePath, out txtTaxonomyPath ) )
			{
				MessageBox.Show( "The selected file does not appear to be an instance document.  Please check the file and try again." );
				return;
			}
			
			string error;
			FilingSummary fs;
			string folderPath = Path.GetDirectoryName( txtInstancePath );
			string instancePath = Path.GetFileNameWithoutExtension( txtInstancePath );
			string taxonomyPath = Path.GetFileNameWithoutExtension( txtTaxonomyPath );

			TestReportBuilderFinancial trb = new TestReportBuilderFinancial();
			bool isBuilt = trb.BuildReports( folderPath, instancePath, taxonomyPath, out fs, out error );
			Assert.IsTrue( isBuilt, error );
		}

		[Test, Explicit]
		public void UpdateResults()
		{
            Test_Abstract.UpdateResultsFlag = true;
		}

		public static void RunAllConcurrent( object instance )
		{
            if (Test_Abstract.ConcurrentTestsRunning)
				return;

            if (!Test_Abstract.RunAllConcurrentFlag)
				return;

            Test_Abstract.ConcurrentTestsRunning = true;
            Test_Abstract.ConcurrentTestsResults = new Dictionary<string, Exception>();

			testsFailed = 0;
			threadsRunning = 0;
            Semaphore sem = new Semaphore(Test_Abstract.ConcurrentTestsCount, Test_Abstract.ConcurrentTestsCount);

			try
			{
				Type t = instance.GetType();
				MethodInfo[] mis = t.GetMethods( BindingFlags.Instance | BindingFlags.Public );
				Array.Sort( mis, ( mi1, mi2 ) => string.Compare( mi1.Name, mi2.Name ) );

				for( int mIdx = 0; mIdx < mis.Length; mIdx++ )
				{
					MethodInfo mi = mis[ mIdx ];
					object[] atts = mi.GetCustomAttributes( typeof( TestAttribute ), false );
					if( atts.Length == 0 )
						continue;

					atts = mi.GetCustomAttributes( typeof( IgnoreAttribute ), false );
					if( atts.Length > 0 )
						continue;

					atts = mi.GetCustomAttributes( typeof( ExplicitAttribute ), false );
					if( atts.Length > 0 )
						continue;

					threadsRunning++;
                    Test_Abstract.ConcurrentTestsResults[mi.Name] = null;
					WaitCallback callback = GetCallback( sem, instance, mi );
					ThreadPool.QueueUserWorkItem( callback );
				}

				while( threadsRunning > 0 )
				{
                    if (threadsRunning >= Test_Abstract.ConcurrentTestsCount)
					{
						sem.WaitOne();
						sem.Release();
					}

					Thread.Sleep( 2000 );
				}
			}
			catch( Exception tEx )
			{
                Test_Abstract.ConcurrentTestsResults = null;
				Trace.TraceError( "Error: " + tEx.Message );
			}
			finally
			{
                Test_Abstract.ConcurrentTestsRunning = false;
			}

			while( threadsRunning > 0 )
			{
                if (threadsRunning >= Test_Abstract.ConcurrentTestsCount)
				{
					sem.WaitOne();
					sem.Release();
				}

				Thread.Sleep( 2000 );
			}
		}

		#region PRIVATE

		private static int testsFailed = 0;
		private static int threadsRunning = 0;
		private static WaitCallback GetCallback( Semaphore sem, object instance, MethodInfo mi )
		{
			WaitCallback waitCB = new WaitCallback( obj =>
			{
				sem.WaitOne();

				try
				{
                    if (Test_Abstract.ConcurrentTestsResults == null)
						return;

					mi.Invoke( instance, null );
				}
				catch( Exception ex )
				{
					testsFailed++;
                    Test_Abstract.ConcurrentTestsResults[mi.Name] = ex;
				}
				finally
				{
					sem.Release();
					threadsRunning--;
				}
			} );

			return waitCB;
		}

		public static bool IsInstance( string file, out string taxonomy )
		{
			taxonomy = string.Empty;

			if( !file.StartsWith( "http" ) && !File.Exists( file ) )
				return false;

			if( Path.GetExtension( file ) != ".xml" )
				return false;

			string readFile = file;
			if( file.StartsWith( "http" ) )
			{
				readFile = Path.GetTempFileName();

				WebClient cli = new WebClient();
				cli.DownloadFile( file, readFile );
			}

			try
			{
				XmlTextReader doc = new XmlTextReader( readFile );

				doc.MoveToContent();
				if( doc.LocalName != "xbrl" )
					return false;

				List<string> foundTaxonomies = new List<string>();
				int count = 0;
				while( doc.Read() && count++ < 10 )
				{
					if( doc.NodeType != XmlNodeType.Element )
						continue;

					if( doc.LocalName != "schemaRef" )
						continue;

					while( doc.MoveToNextAttribute() )
					{
						if( doc.NodeType != XmlNodeType.Attribute || doc.Name != "xlink:href" )
							continue;

						//this one is prefered because it exists in the same folder
						if( doc.Value == Path.GetFileName( doc.Value ) )
						{
							taxonomy = doc.Value;
							return true;
						}
						else
						{
							foundTaxonomies.Add( doc.Value );
						}
					}
				}

				if( foundTaxonomies.Count > 0 )
				{
					taxonomy = foundTaxonomies[ 0 ];
					return true;
				}

			}
			catch { }

			return false;
		}

		#endregion
	}
}

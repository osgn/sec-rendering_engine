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
#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test_Taxonomy
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Collections;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_NTP : Taxonomy
	{
		#region Overrides
//		public void TestParse( out int errors )
//		{
//			errors = 0;
//			ParseInternal( out errors );
//		}

		#endregion

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;


			Taxonomy.SkipDefinitionFileLoading = true;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Trace.Listeners.Clear();
			Taxonomy.SkipDefinitionFileLoading = false;
		}

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string NTP_FILE = TestCommon.FolderRoot + @"ntp" +System.IO.Path.DirectorySeparatorChar +"ntpCommon-2005-04-01.xsd";
		string NTP2005_FILE = TestCommon.FolderRoot + @"NTP2005-0809\Vs10_20060908_2005\bd\report\bd-rp-vennootschapsbelasting-2005.xsd";
		string NTP2006_FILE = TestCommon.FolderRoot + @"NTP2005-0809\Vs10_20060908_2005\bd\report\bd-rp-intra-communautaire-leveringen-2006.xsd";	

		#region helpers
		/// <exclude/>
		protected void SendErrorsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level != TraceLevel.Error )
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		/// <exclude/>
		protected int SendWarningsToConsole(ArrayList errorList)
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Warning )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
					++numwarnings;
				}
			}

			return numwarnings;
		}

		/// <exclude/>
		protected void SendInfoToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Info )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList, string filter)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Message.IndexOf( filter ) < 0 )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		#endregion

		/// <summary>
		/// Summary description for TestTaxonomy_MorganStanley.
		/// </summary>
		public TestTaxonomy_NTP()
		{
			//
			// TODO: Add constructor logic here
			//
		}


		/// <exclude/>
		[Test]
		public void NTP2006_LoadAndParse()
		{
			TestTaxonomy_NTP s = new TestTaxonomy_NTP();

			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( NTP2006_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			s.Parse( out errors );

			DateTime end = DateTime.Now;

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//			SendWarningsToConsole( s.errorList );
			//			SendInfoToConsole( s.ErrorList );

			Assert.AreEqual( 0, errors, "errors returned from parse" );

			DateTime getNodesStart = DateTime.Now;

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";
			ArrayList vals = s.GetNodesByPresentation();

			DateTime getNodesEnd = DateTime.Now;

			Console.WriteLine( "Load and Parse: " + (end-start).ToString() );
			Console.WriteLine( "Show: " + (getNodesEnd-getNodesStart).ToString() );

			//Verify that all elements have MyDataType set (extendeted data types converted to base types)
			int naCount = 0;
			int blankElementTypeCount = 0;
			Hashtable htMissingTypes = new Hashtable(0);

			
			//TEST all elements
			foreach (Element e1 in s.AllElements.Values)
			{

				if (e1.MyDataType.ToString().ToLower() == "na")
				{
					if (e1.ElementType.Length == 0)
					{
						blankElementTypeCount += 1;
					}
					else
					{
						if (htMissingTypes[e1.ElementType] == null)
						{
							htMissingTypes[e1.ElementType] = 1;
						}
						else
						{
							int cnt = int.Parse(htMissingTypes[e1.ElementType].ToString());
							htMissingTypes[e1.ElementType] = cnt + 1;
						}
					}
					naCount += 1;
				}
				
			}

			Console.WriteLine ("Total element do not have MyDataType = " + naCount.ToString());
			Console.WriteLine ("no element type:"+ blankElementTypeCount.ToString());
			foreach (string typeName in htMissingTypes.Keys)
			{
				Console.WriteLine (typeName + ":" + htMissingTypes[typeName].ToString());
			}

			Assert.AreEqual (731, naCount, "Incorrent number of [N/A] elements.");
			Assert.AreEqual (578, blankElementTypeCount, "Incorrent number of elements without Element Type specified.");

			//Test colond elements
			Console.WriteLine ("=================== Cloned elements");
			naCount = 0;
			blankElementTypeCount = 0;
			htMissingTypes = new Hashtable(0);

			foreach (Element e1 in s.AllElements.Values)
			{

                if (e1.MyDataType.ToString().ToLower() == "na")
                {



                    if (e1.ElementType.Length == 0)
                    {
                        blankElementTypeCount += 1;
                    }
                    else
                    {
                        if (htMissingTypes[e1.ElementType] == null)
                        {
                            htMissingTypes[e1.ElementType] = 1;
                        }
                        else
                        {
                            int cnt = int.Parse(htMissingTypes[e1.ElementType].ToString());
                            htMissingTypes[e1.ElementType] = cnt + 1;
                        }
                    }


                }
				
			}
			Assert.IsTrue (htMissingTypes.Count == 0, "Incorrect clone elements test results.");

		}

		/// <exclude/>
		[Test]
		public void NTP2005_LoadAndParse()
		{
			TestTaxonomy_NTP s = new TestTaxonomy_NTP();

			int errors = 0;

			DateTime start = DateTime.Now;
			if ( s.Load( NTP2005_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			s.Parse( out errors );

			DateTime end = DateTime.Now;

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			//			SendWarningsToConsole( s.errorList );
			//			SendInfoToConsole( s.ErrorList );

			Assert.AreEqual( 0, errors, "errors returned from parse" );

			DateTime getNodesStart = DateTime.Now;

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";
			ArrayList vals = s.GetNodesByPresentation();

			DateTime getNodesEnd = DateTime.Now;

			Console.WriteLine( "Load and Parse: " + (end-start).ToString() );
			Console.WriteLine( "Show: " + (getNodesEnd-getNodesStart).ToString() );

			//Verify that all elements have MyDataType set (extendeted data types converted to base types)
			int naCount = 0;
			int blankElementTypeCount = 0;
			Hashtable htMissingTypes = new Hashtable(0);

			foreach (Element e1 in s.AllElements.Values)
			{
				if (e1.MyDataType.ToString().ToLower() == "na")
				{

					if (e1.ElementType.Length == 0)
					{
						blankElementTypeCount += 1;
					}
					else
					{
						if (htMissingTypes[e1.ElementType] == null)
						{
							htMissingTypes[e1.ElementType] = 1;
						}
						else
						{
							int cnt = int.Parse(htMissingTypes[e1.ElementType].ToString());
							htMissingTypes[e1.ElementType] = cnt + 1;
						}
					}
					naCount += 1;
				}
				
			}

			Console.WriteLine ("Total element do not have MyDataType = " + naCount.ToString());
			Console.WriteLine ("no element type:"+ blankElementTypeCount.ToString());
			foreach (string typeName in htMissingTypes.Keys)
			{
				Console.WriteLine (typeName + ":" + htMissingTypes[typeName].ToString());
			}

			Assert.AreEqual (731, naCount, "Incorrent number of [N/A] elements.");
			Assert.AreEqual (578, blankElementTypeCount, "Incorrent number of elements without Element Type specified.");

		}


//		/// <exclude/>
//      [Test] public void NTP_LoadAndParse()
//		{
//			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
//
//			TestTaxonomy_NTP s = new TestTaxonomy_NTP();
//
//			int errors = 0;
//
//			DateTime start = DateTime.Now;
//			if ( s.Load( NTP_FILE, out errors ) != true )
//			{
//				Assert.Fail( (string)s.ErrorList[0]);
//			}
//
//			errors = 0;
//
//			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
//			s.Parse( out errors );
//
//			DateTime end = DateTime.Now;
//
//			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
//			if ( errors > 0 )
//			{
//				SendErrorsToConsole( s.errorList );
//			}
//
////			SendWarningsToConsole( s.errorList );
////			SendInfoToConsole( s.ErrorList );
//
//			Assert.AreEqual( 0, errors, "errors returned from parse" );
//
//			DateTime getNodesStart = DateTime.Now;
//
//			s.CurrentLabelRole = "preferredLabel";
//			s.CurrentLanguage = "en";
//			ArrayList vals = s.GetNodesByPresentation();
//
//			DateTime getNodesEnd = DateTime.Now;
//
//			Console.WriteLine( "Load and Parse: " + (end-start).ToString() );
//			Console.WriteLine( "Show: " + (getNodesEnd-getNodesStart).ToString() );
//
//			
//		}

//		[Test] public void XINT_LoadAndParse_Calculation()
//		{
//			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
//
//			TestTaxonomy_XINT s = new TestTaxonomy_XINT();
//
//			int errors = 0;
//
//			if ( s.Load( XINT_FILE, out errors ) != true )
//			{
//				Assert.Fail( (string)s.ErrorList[0]);
//			}
//
//			errors = 0;
//
//			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
//			s.Parse( out errors );
//
//			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
//			if ( errors > 0 )
//			{
//				SendErrorsToConsole( s.errorList );
//				SendWarningsToConsole( s.errorList );
//				SendInfoToConsole( s.ErrorList );
//			}
//
//			Assert.AreEqual( 0, errors, "errors returned from parse" );
//
//			s.currentLabelRole = "preferredLabel";
//			s.currentLanguage  = "en";
//
//			errors = 0;
//			ArrayList nodes = s.GetNodes( out errors );
//
//
//			Assert.IsTrue( errors == 0, "errors returned from calculation" );
//		}
	}
}
#endif
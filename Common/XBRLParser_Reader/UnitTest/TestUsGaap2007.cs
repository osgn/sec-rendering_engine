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
	using System.Text;
	using System.Xml;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using NUnit.Framework;

	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture]
	[Serializable]
	public class TestUsGaap2008 : Taxonomy
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

			Common.MyTraceSwitch = new TraceSwitch("Common", "common trace switch");
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast()
		{
			Trace.Listeners.Clear();
		}

		///<exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{ }

		///<exclude/>
		[TearDown]
		public void RunAfterEachTest()
		{ }
		#endregion


		#region helpers
		/// <exclude/>
		protected void SendErrorsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level != TraceLevel.Error)
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
			}
		}

		/// <exclude/>
		protected int SendWarningsToConsole(ArrayList errorList)
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level == TraceLevel.Warning)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
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

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Level == TraceLevel.Info)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
				}
			}
		}

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList, string filter)
		{
			// now display the errors 
			errorList.Sort();

			foreach (ParserMessage pm in errorList)
			{
				if (pm.Message.IndexOf(filter) < 0)
				{
					Console.WriteLine(pm.Level.ToString() + ": " + pm.Message);
				}
			}
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public TestUsGaap2008()
		{
			
		}



		/// <exclude/>
		[Test, Ignore]
		public void TestUsGaap2009_TMP()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = @"S:\rr-2010-01-01\rr-ref-2010-01-01.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should have no errors");

			//s.CurrentLanguage = s.SupportedLanguages[0] as string;
			//s.currentLabelRole = PresentationLocator.preferredLabelRole;
			//start = DateTime.Now;
			//ArrayList nodes = s.GetNodesByPresentation(true);

			//Dictionary<string, string> targetRoleInfos = new Dictionary<string, string>();

			//foreach (Node n in nodes)
			//{
			//    n.RecursivelyGetAllTargetRoleInfos(n.MyPresentationLink.Role, ref targetRoleInfos);
			//}

			//foreach (KeyValuePair<string, string> kvp in targetRoleInfos)
			//{
			//    Console.WriteLine("Statement Role = {0} Target Role used = {1}", kvp.Key, kvp.Value);
			//}
			//s.GetNodesByPresentation();
			//s.GetNodesByCalculation();
			//List<DimensionNode> tmp;
			//s.TryGetAllDimensionNodesForDisplay(s.currentLanguage, s.currentLabelRole,
			//    true, out tmp);
		}

		/// <exclude/>
		[Test, Ignore]
		public void TestDEI2007_Ext()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-10-30-prerelease" + System.IO.Path.DirectorySeparatorChar + "Test6.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");


			Assert.AreEqual(11, s.roleRefs.Count);

			foreach (RoleRef rr in s.roleRefs.Values)
			{
				Assert.IsTrue(rr.GetHref().Contains("us-roles-2007-12-31.xsd"), "href in the role ref is not valid");
			}


			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";
			ArrayList nodes = s.GetNodesByPresentation(true);

			DateTime end = DateTime.Now;

			Console.WriteLine("time taken = {0}", end - start);
			int count = 0;
			int dimensionNodeCount = 0;
			foreach (Node n in nodes)
			{
				StringBuilder sb = DisplayNode(n, 0, ref count, ref dimensionNodeCount);
				Console.WriteLine(sb.ToString());
			}
			Console.WriteLine("Count of elements in presentation = {0}", count);
			Console.WriteLine("Count of dimension elements in presentation = {0}", dimensionNodeCount);
			Console.WriteLine("time taken = {0}", end - start);




		}

		/// <exclude/>
		[Test, Ignore]
		public void TestDEI2007()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-11-09-prerelease\non-gaap" + System.IO.Path.DirectorySeparatorChar + "dei-all-2007-12-31.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");


			Element e = s.allElements["dei_Z4932"] as Element;

			Assert.AreEqual("us-types:domainItemType", e.OrigElementType, "Element type is not set correctly");
			Assert.AreEqual("xbrli:stringItemType", e.ElementType, "Element type is not set correctly");


			e = s.allElements["dei_FormerFiscalYearEndDate"] as Element;

			Assert.AreEqual("xbrli:gMonthDayItemType", e.OrigElementType, "Element type is not set correctly");
			Assert.AreEqual("xbrli:gMonthDayItemType", e.ElementType, "Element type is not set correctly");



		}

		/// <exclude/>
		[Test]
		public void TestUsGaap2008_ci_all()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"1.0.9564\ind\ci" + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-stm-dis-all-2008-03-31.xsd";
			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine("time taken = {0}", end - start);

			Assert.AreEqual(0, errors, "should not have any errors");
			int countCustomTypeTax = 0;
			foreach (TaxonomyItem ti in s.infos)
			{
				if (ti.HasCustomTypes)
				{
					countCustomTypeTax++;
					Console.WriteLine("FOUND CUSTOM TYPES INFO");
					Console.WriteLine(ti.WebLocation + ti.Location);

				}
			}
			Assert.AreEqual(1, countCustomTypeTax, "Should have only one custom type taxonomy");
			//none of the href references should be web based.
			foreach (RoleRef rr in s.roleRefs.Values)
			{
				Assert.IsFalse(rr.href.StartsWith("http"), "Failed to build the correct href");
				Assert.IsTrue( rr.href.StartsWith( ".."),"Failed to build the correct href");
			}

			foreach (RoleType rt in s.roleTypes.Values)
			{
				Assert.IsFalse(rt.GetHref().StartsWith("http"), "Failed to build the correct href");
				Assert.IsTrue(rt.GetHref().StartsWith(".."), "Failed to build the correct href");
			}

			foreach (TaxonomyItem ti in s.TaxonomyItems)
			{
				Assert.IsFalse(ti.Location.StartsWith("http"), "Failed to build the correct href");

			}

			int countRef = 0;
			int countNoRef = 0;

			int noInCal = 0;
			int noNotInCal = 0;
			Hashtable elementTypes = new Hashtable();
			foreach (Element ele in s.allElements.Values)
			{

				Assert.IsTrue(ele.LabelInfo.labelDatas.Count >= 1, "Failed to find label");
				
				ReferenceLocator rl;
				if (ele.TryGetReferenceLocators(out rl))
				{
					countRef++;
				}
				else
				{
					if (!ele.Id.EndsWith("Abstract"))
					{
						countNoRef++;
					}
				}

				if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedPartiesCurrent"))
				{
					Assert.AreEqual(3, rl.References.Count, "Should have three references");
				}


				if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedPartiesCurrent"))
				{
					Assert.AreEqual(3, rl.References.Count, "Should have three references");
				}
				
				if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedParties"))
				{
					Assert.AreEqual(4, rl.References.Count, "Should have four references");
				}

				if (s.DoesElementExistInCalculation(ele.Id))
				{
					noInCal++;
					elementTypes[ele.OrigElementType] = 1;
				}
				else
				{
					noNotInCal++;
				}

			}

			Console.WriteLine("Count ref = {0} Count no Ref = {1}", countRef, countNoRef);
			Console.WriteLine("Count in calculation = {0} Count not in calculation = {1}", noInCal, noNotInCal);


			foreach (string type in elementTypes.Keys)
			{
				Console.WriteLine("Cal element type = " + type);
			}
		}

        /// <exclude/>
        [Test]
        public void TestUsGaap2009_ci_all()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"XBRLUS-USGAAP-Taxonomies-2009-01-31\ind\ci" + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-stm-all-2009-01-31.xsd";
            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);
            DateTime end = DateTime.Now;
            Console.WriteLine("time taken = {0}", end - start);

            Assert.AreEqual(0, errors, "should not have any errors");
            int countCustomTypeTax = 0;
            foreach (TaxonomyItem ti in s.infos)
            {
                if (ti.HasCustomTypes)
                {
                    countCustomTypeTax++;
                    Console.WriteLine("FOUND CUSTOM TYPES INFO");
                    Console.WriteLine(ti.WebLocation + ti.Location);

                }
            }
            Assert.AreEqual(1, countCustomTypeTax, "Should have only one custom type taxonomy");
            //none of the href references should be web based.
            foreach (RoleRef rr in s.roleRefs.Values)
            {
                Assert.IsFalse(rr.href.StartsWith("http"), "Failed to build the correct href");
                Assert.IsTrue(rr.href.StartsWith(".."), "Failed to build the correct href");
            }

            foreach (RoleType rt in s.roleTypes.Values)
            {
                Assert.IsFalse(rt.GetHref().StartsWith("http"), "Failed to build the correct href");
                Assert.IsTrue(rt.GetHref().StartsWith(".."), "Failed to build the correct href");
            }

            foreach (TaxonomyItem ti in s.TaxonomyItems)
            {
                Assert.IsFalse(ti.Location.StartsWith("http"), "Failed to build the correct href");

            }

            int countRef = 0;
            int countNoRef = 0;

            int noInCal = 0;
            int noNotInCal = 0;
            Hashtable elementTypes = new Hashtable();
            foreach (Element ele in s.allElements.Values)
            {

                Assert.IsTrue(ele.LabelInfo.labelDatas.Count >= 1, "Failed to find label");

                ReferenceLocator rl;
                if (ele.TryGetReferenceLocators(out rl))
                {
                    countRef++;
                }
                else
                {
                    if (!ele.Id.EndsWith("Abstract"))
                    {
                        countNoRef++;
                    }
                }

                if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedPartiesCurrent"))
                {
                    Assert.AreEqual(3, rl.References.Count, "Should have three references");
                }


                if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedPartiesCurrent"))
                {
                    Assert.AreEqual(3, rl.References.Count, "Should have three references");
                }

                if (ele.Id.Equals("us-gaap_AccountsReceivableRelatedParties"))
                {
                    Assert.AreEqual(4, rl.References.Count, "Should have four references");
                }

                if (s.DoesElementExistInCalculation(ele.Id))
                {
                    noInCal++;
                    elementTypes[ele.OrigElementType] = 1;
                }
                else
                {
                    noNotInCal++;
                }

            }

            Console.WriteLine("Count ref = {0} Count no Ref = {1}", countRef, countNoRef);
            Console.WriteLine("Count in calculation = {0} Count not in calculation = {1}", noInCal, noNotInCal);


            foreach (string type in elementTypes.Keys)
            {
                Console.WriteLine("Cal element type = " + type);
            }



            Dictionary<string, string> dt = s.GetSimpleCustomElementTypes();

            foreach (KeyValuePair<string, string> kvp in dt)
            {
                Console.WriteLine("Ext type {0} for base type {1}", kvp.Key, kvp.Value);
            }
        }


		/// <exclude/>
		[Test]
		public void TestUsGaap2009_ci_dis_all()
		{

			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"XBRLUS-USGAAP-Taxonomies-2009-01-31\ind\ci" + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine("time taken = {0}", end - start);
		
		}



        /// <exclude/>
        [Test,Ignore]
        public void TestUSGaap2009WebThreeTimes()
        {
            int maxCount = 3;
            for(int i=0;i<maxCount;i++)
            {
                TestUsGaap2009_ci_web();
            }
        }

        /// <exclude/>

        private void TestUsGaap2009_ci_web()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
            string fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd";
            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);
            DateTime end = DateTime.Now;
            Console.WriteLine("time taken = {0}", end - start);

        }


		/// <exclude/>
		[Test, Ignore]
		public void TestGetSummationItemErrors()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = @"S:\XBRLUSGAAPTaxonomies-2008-03-31\ind\ci\simulation.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine("time taken = {0}", end - start);

			Assert.AreEqual(0, errors, "should not have any errors");
			List<string> summationErrors;
			s.GetSummationItemErrors( out summationErrors );

			foreach( string str in summationErrors )
			{
				Console.WriteLine( str );
			}
		}

		[Test]
		public void TestUsGaap2009_ci_all_Web_slow()
		{

			DateTime start = DateTime.Now;
			string fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);
			DateTime end = DateTime.Now;

			Console.WriteLine("Time taken = {0}", end - start);
		}

		[Test]
		public void TestUsGaap2009_ci_all_Web_Fast()
		{
			RemoteFiles.RemoteFileInformation.LoadRemoteFileInformation();
			DateTime start = DateTime.Now;
			string fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);
			DateTime end = DateTime.Now;

			Console.WriteLine("Time taken = {0}", end - start);
		}

		[Test]
		public void TestLoadLastModifiedInfoFor2009Taxonomies()
		{
			string fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/ci/us-gaap-ci-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);

			fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/ins/us-gaap-ins-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);

			fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/bd/us-gaap-bd-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);

			fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/basi/us-gaap-basi-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);

			fileName = "http://taxonomies.xbrl.us/us-gaap/2009/ind/re/us-gaap-re-stm-dis-all-2009-01-31.xsd";
			TestUsGaap2009Web(fileName);

			RemoteFiles.RemoteFileInformation.SaveRemoteFileInformation();

		}

        [Test, Ignore]
        public void TestLoadLastModifiedInfoFor2011Taxonomies()
        {
            //TestUsGaap2009Web method is loading file, it's not 2009 spesific

            string fileName = "http://xbrl.fasb.org/us-gaap/2011/ind/ci/us-gaap-ci-stm-dis-all-2011-01-31.xsd";
            TestUsGaap2009Web(fileName);

            fileName = "http://xbrl.fasb.org/us-gaap/2011/ind/ins/us-gaap-ins-stm-dis-all-2011-01-31.xsd";
            TestUsGaap2009Web(fileName);

            fileName = "http://xbrl.fasb.org/us-gaap/2011/ind/bd/us-gaap-bd-stm-dis-all-2011-01-31.xsd";
            TestUsGaap2009Web(fileName);

            fileName = "http://xbrl.fasb.org/us-gaap/2011/ind/basi/us-gaap-basi-stm-dis-all-2011-01-31.xsd";
            TestUsGaap2009Web(fileName);

            fileName = "http://xbrl.fasb.org/us-gaap/2011/ind/re/us-gaap-re-stm-dis-all-2011-01-31.xsd";
            TestUsGaap2009Web(fileName);

            RemoteFiles.RemoteFileInformation.SaveRemoteFileInformation();

        }


		[Test]
		public void TestAlfalafaTaxonomy()
		{
			TestLoadParseTaxonomy(@"S:\alfalfa-20090501\alfalfa-20090501.xsd");
		}

		private void TestLoadParseTaxonomy(string fileName)
		{
			TestUsGaap2008 s = new TestUsGaap2008();
			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine("time taken = {0}", end - start);

		}

		/// <exclude/>
		private void TestUsGaap2009Web(string fileName)
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestUsGaap2008 s = new TestUsGaap2008();
			s.KeepInnerTaxonomies = true;
			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);
			DateTime end = DateTime.Now;
			Console.WriteLine("time taken = {0}", end - start);

			Assert.AreEqual(0, errors, "should not have any errors");
			s.CurrentLanguage = s.SupportedLanguages[0] as string;
			s.currentLabelRole =  PresentationLocator.preferredLabelRole;
			start = DateTime.Now;
			s.GetNodesByPresentation(true);
			s.GetNodesByPresentation();
			s.GetNodesByCalculation();
			List<DimensionNode> tmp;
			s.TryGetAllDimensionNodesForDisplay(s.currentLanguage, s.currentLabelRole,
				true, out tmp);

			end = DateTime.Now;

			Console.WriteLine("GET NODES BY PRESENTATION TIME = {0}", end - start);
			//all the href references should be web based....
			foreach (RoleRef rr in s.roleRefs.Values)
			{
				Assert.IsTrue(rr.href.StartsWith("http"), "Failed ot build the correct href");
			}

			foreach (RoleType rt in s.roleTypes.Values)
			{
				Assert.IsTrue(rt.GetHref().StartsWith("http"), "Failed to build the correct href");
			}

			foreach( TaxonomyItem ti in s.TaxonomyItems )
			{
				Assert.IsTrue(ti.Location.StartsWith("http"), "Failed ot build the correct href");

			}

			int countCustomTypeTax = 0;
			foreach (TaxonomyItem ti in s.infos)
			{
				if (ti.HasCustomTypes)
				{
					countCustomTypeTax++;
					Console.WriteLine("FOUND CUSTOM TYPES INFO");
					Console.WriteLine(ti.WebLocation + ti.Location);

				}
			}
			Assert.AreEqual(1, countCustomTypeTax, "Should have only one custom type taxonomy");
			List<string> xsdNames = new List<string>();
			foreach (LinkbaseFileInfo lfi in s.linkbaseFileInfos)
			{
				
				foreach (string role in lfi.RoleRefURIs)
				{
					Console.WriteLine(" ROLE = {0} XSD = {1}", role , lfi.XSDFileName);

				}
			}

			Dictionary<string, string> linkbaseXSDMap = new Dictionary<string, string>();

			foreach (Taxonomy depT in s.DependantTaxonomies)
			{
				string xsdName = Path.GetFileName(depT.infos[0].Location);

				if (depT.presentationFile != null)
				{
					foreach (string file in depT.presentationFile)
					{
						string val;
						if (linkbaseXSDMap.TryGetValue(Path.GetFileName(file), out val))
						{
							if (!val.Equals(xsdName))
							{
								Console.WriteLine("FOUND TWO XSD INCLUDING THE SAME PRESENTATION LINKBASE ");
								Console.WriteLine("{0} {1}", xsdName, val);
							}
						}
						else
						{
							linkbaseXSDMap.Add(Path.GetFileName(file), xsdName);
						}
						Console.WriteLine("{0} - {1}", Path.GetFileName(file), xsdName);
					}
				}
				if (depT.DefinitionFile != null)
				{
					foreach (string file in depT.DefinitionFile)
					{
						string val;
						if (linkbaseXSDMap.TryGetValue(Path.GetFileName(file), out val))
						{
							if (!val.Equals(xsdName))
							{
								Console.WriteLine("FOUND TWO XSD INCLUDING THE SAME DEFINITION LINKBASE ");
								Console.WriteLine("{0} {1}", xsdName, val);
							}
						}
						else
						{
							linkbaseXSDMap.Add(Path.GetFileName(file), xsdName);
						}
						Console.WriteLine("{0} - {1}", Path.GetFileName(file), xsdName);
					}
				}
				if (depT.calculationFile != null)
				{
					foreach (string file in depT.calculationFile)
					{
						string val;
						if (linkbaseXSDMap.TryGetValue(Path.GetFileName(file), out val))
						{
							if (!val.Equals(xsdName))
							{
								Console.WriteLine("FOUND TWO XSD INCLUDING THE SAME  CALCULATION LINKBASE ");
								Console.WriteLine("{0} {1}", xsdName, val);
							}
						}
						else
						{
							linkbaseXSDMap.Add(Path.GetFileName(file), xsdName);
						}
						Console.WriteLine("{0} - {1}", Path.GetFileName(file), xsdName);
					}
				}

				if (depT.LabelFile != null)
				{
					foreach (string file in depT.LabelFile)
					{
						string val;
						if (linkbaseXSDMap.TryGetValue(Path.GetFileName(file), out val))
						{
							if (!val.Equals(xsdName))
							{
								Console.WriteLine("FOUND TWO XSD INCLUDING THE SAME LABEL LINKBASE ");
								Console.WriteLine("{0} {1}", xsdName, val);
							}
						}
						else
						{
							linkbaseXSDMap.Add(Path.GetFileName(file), xsdName);
						}
						Console.WriteLine("{0} - {1}", Path.GetFileName(file), xsdName);
					}
				}


				if (depT.ReferenceFile != null)
				{
					foreach (string file in depT.ReferenceFile)
					{
						string val;
						if (linkbaseXSDMap.TryGetValue(Path.GetFileName(file), out val))
						{
							if (!val.Equals(xsdName))
							{
								Console.WriteLine("FOUND TWO XSD INCLUDING THE SAME REFERENCE LINKBASE ");
								Console.WriteLine("{0} {1}", xsdName, val);
							}
						}
						else
						{
							linkbaseXSDMap.Add(Path.GetFileName(file), xsdName);
						}
						Console.WriteLine("{0} - {1}", Path.GetFileName(file), xsdName);
					}
				}


			}



		}


		/// <exclude/>
		[Test, Ignore]
		public void TestUsGaap2007_basi()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-10-30-prerelease" + System.IO.Path.DirectorySeparatorChar + "us-gaap-entryPoint-basi-2007-12-31.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");




			Assert.AreEqual(13320, s.allElements.Count, "wrong number of elements returned");

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();

			DateTime end = DateTime.Now;

			Console.WriteLine("time taken = {0}", end - start);
			int count = 0;
			int dimensionNodeCount = 0;
			foreach (Node n in nodes)
			{
				StringBuilder sb = DisplayNode(n, 0, ref count, ref dimensionNodeCount);
				Console.WriteLine(sb.ToString());
			}
			Console.WriteLine("Count of elements in presentation = {0}", count);
			Console.WriteLine("time taken = {0}", end - start);


			ArrayList langs =    s.GetSupportedLanguages(false, out errors);

			Assert.AreEqual(0, errors);
			Assert.AreEqual(1, langs.Count , "should have one supported language defined");





		}

		/// <exclude/>
		[Test, Ignore]
		public void TestUsGaap2007_all()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-11-09-prerelease\test" + System.IO.Path.DirectorySeparatorChar + "us-gaap-entryPoint-all-2007-12-31.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");




			Assert.AreEqual(14062, s.allElements.Count, "wrong number of elements returned");

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();

			DateTime end = DateTime.Now;

			int count = 0;
			int dimensionNodeCount = 0;
			foreach (Node n in nodes)
			{
				StringBuilder sb = DisplayNode(n, 0, ref count, ref dimensionNodeCount);
				Console.WriteLine(sb.ToString());
			}
			Console.WriteLine("Count of elements in presentation = {0}", count);
			Console.WriteLine("time taken = {0}", end - start);


		}

		/// <exclude/>
		[Test, Ignore]
		public void TestUsGaap2007_ci_spc()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-10-30-prerelease" + System.IO.Path.DirectorySeparatorChar + "us-gaap-stm-ci-spc-2007-12-31.xsd";

			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			foreach (ParserMessage  err in s.errorList)
			{
				Console.WriteLine(err.ToString() );
			}

			Assert.AreEqual(0, errors, "should not have any errors");




			Assert.AreEqual(13301, s.allElements.Count, "wrong number of elements returned");

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();

			DateTime end = DateTime.Now;

			int count = 0;
			int dimensionNodeCount = 0;
			foreach (Node n in nodes)
			{
				StringBuilder sb = DisplayNode(n, 0, ref count, ref dimensionNodeCount);
				Console.WriteLine(sb.ToString());
			}
			Console.WriteLine("Count of elements in presentation = {0}", count);
			Console.WriteLine("time taken = {0}", end - start);


		}

		/// <exclude/>
		[Test, Ignore]
		public void Test_us_gaap_ci_stm_dis_all_2007_12_31()
		{
			string fileName = TestCommon.FolderRoot + @"USFRTF-2007-11-21-prerelease\ind\ci" + System.IO.Path.DirectorySeparatorChar + "us-gaap-ci-stm-dis-all-2007-12-31.xsd";
			Taxonomy s = new Taxonomy();

			int errors = 0;
			DateTime startLoad = DateTime.Now;
			DateTime startLoadParse = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}
			DateTime endLoad = DateTime.Now;

			DateTime startParse = DateTime.Now;
			s.Parse(out errors);
			DateTime endParse = DateTime.Now;
			DateTime endLoadParse = DateTime.Now;

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			long FileSize = new long();
			DateTime startSerialize = DateTime.Now;
			Taxonomy.Serialize(s, @"C:\Aucent\TaxonomySerialized.bin", out FileSize);
			DateTime endSerialize = DateTime.Now;

			//Taxonomy sNew = new Taxonomy();
			//DateTime startDeserialize = DateTime.Now;
			//Taxonomy.Deserialize(@"C:\Aucent\TaxonomySerialized.bin", out sNew);
			//DateTime endDeserialize = DateTime.Now;

			DateTime startGetNodes = DateTime.Now;
			ArrayList nodes = s.GetNodesByPresentation(true);
			DateTime endGetNodes = DateTime.Now;

			//ArrayList nodesNew = sNew.GetNodesByPresentation(true);

			//int count = 0;
			//int dimensionNodeCount = 0;
			//foreach (Node n in nodes)
			//{
			//    StringBuilder sb = DisplayNode(n, 0, ref count, ref dimensionNodeCount);
			//    Console.WriteLine(sb.ToString());
			//}

			//int countNew = 0;
			//int dimensionNodeCountNew = 0;
			//foreach (Node n in nodesNew)
			//{
			//    StringBuilder sb = DisplayNode(n, 0, ref countNew, ref dimensionNodeCountNew);
			//    Console.WriteLine(sb.ToString());
			//}

			Console.WriteLine("Time to Load: {0}", endLoad - startLoad);
			Console.WriteLine("Time to Parse: {0}", endParse - startParse);
			Console.WriteLine("Time to Load/Parse: {0}", endLoadParse - startLoadParse);
			Console.WriteLine("Time to Serialize: {0}", endSerialize - startSerialize);
			//Console.WriteLine("Time to Deserialize: {0}", endDeserialize - startDeserialize);
			Console.WriteLine("Time to GetNodes: {0}", endGetNodes - startGetNodes);
			Console.WriteLine("Serialized Filesize: {0}", FileSize);
			//Console.WriteLine("Count of elements in original presentation = {0}", count);
			//Console.WriteLine("Count of elements in deserialized presentation = {0}", countNew);
		}


        /// <exclude/>
        [Test, Ignore]
        public void TestUsGaap2008_DimensionValidation()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
            string fileName = TestCommon.FolderRoot + @"XBRLUSGAAPTaxonomies-2008-02-13\ind\ci\us-gaap-ci-stm-2008-01-31.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            Assert.AreEqual(0, errors, "should not have any errors");




            s.CurrentLanguage = s.SupportedLanguages[0] as string;
            s.CurrentLabelRole = "preferredLabel";

           
            ArrayList nodes = s.GetNodesByPresentation(true);


            s.TryBuildDimensionValidationInformation();

            ArrayList segments = new ArrayList();
            ArrayList scenarios = new ArrayList();


            string elementId = "us-gaap_NetIncomeLossAvailableToCommonStockholdersBasic";

            Segment seg = new Segment();
            seg.DimensionInfo = new ContextDimensionInfo();
            seg.DimensionInfo.dimensionId = "us-gaap_StatementScenarioAxis";
            seg.DimensionInfo.Id = "us-gaap_ScenarioActualMember";

            segments.Add(seg);

            string error;
            bool ret = s.IsDimensionInformationValid(elementId, segments, scenarios, out error);

            Assert.IsFalse(ret, "should return failure");
            Console.WriteLine(error);
            s.linkbaseFileInfos.Sort();

            foreach (LinkbaseFileInfo lbfi in s.linkbaseFileInfos)
            {
                Console.WriteLine(lbfi.ToString());
            }
        }

        /// <exclude/>
        [Test]
        public void TestUsGaap2008_DimensionValidation2()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
            string fileName = @"C:\Users\srikanth.srinivasan\AppData\Roaming\Rivet\CrossTag\Taxonomies\cov\cov.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            Assert.AreEqual(0, errors, "should not have any errors");




            s.CurrentLanguage = s.SupportedLanguages[0] as string;
            s.CurrentLabelRole = "preferredLabel";


            ArrayList nodes = s.GetNodesByPresentation(true);


            s.TryBuildDimensionValidationInformation();

            ArrayList segments = new ArrayList();
            ArrayList scenarios = new ArrayList();


            string elementId = "us-gaap_GainLossRelatedToLitigationSettlement";

            Segment seg = new Segment();
            seg.DimensionInfo = new ContextDimensionInfo();
            seg.DimensionInfo.dimensionId = "us-gaap_ProductOrServiceAxis";
            seg.DimensionInfo.Id = "cov_LitigationLossMember";

            segments.Add(seg);

            string error;
            bool ret = s.IsDimensionInformationValid(elementId, segments, scenarios, out error);

            Assert.IsFalse(ret, "should return failure");
            Console.WriteLine(error);
            s.linkbaseFileInfos.Sort();

            foreach (LinkbaseFileInfo lbfi in s.linkbaseFileInfos)
            {
                Console.WriteLine(lbfi.ToString());
            }
        }
        /// <exclude/>
        [Test, Ignore]
        public void TestUsGaap2008_TargetRoleTests1()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName1 = @"S:\1.0.9564\ind\ci\us-gaap-ci-stm-dis-2008-03-31.xsd";
			string fileName2 = @"S:\1.0.9564\non-gaap\dei-ent-2008-03-31.xsd";


            TestUsGaap2008 s1 = new TestUsGaap2008();

            int errors = 0;
			if (s1.Load(fileName1, out errors) != true)
            {
                Assert.Fail((string)s1.ErrorList[0]);
            }

            s1.Parse(out errors);

            Assert.AreEqual(0, errors, "should not have any errors");


			TestUsGaap2008 s2 = new TestUsGaap2008();

			errors = 0;
			if (s2.Load(fileName2, out errors) != true)
			{
				Assert.Fail((string)s2.ErrorList[0]);
			}

			s2.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");



            List<string> selectedURIs = new List<string>(s1.roleRefs.Keys);

			selectedURIs.AddRange(s2.roleRefs.Keys);

            List<Dimension.TargetDimensionInfo> targetExts;
            Assert.IsTrue(Taxonomy.DoesAnyOfTheSelectedRolesNeedTargetRole(new Taxonomy[] {s1, s2},
				selectedURIs, out targetExts),
                "should have roles that need target role ");

            foreach (Dimension.TargetDimensionInfo tdi in targetExts)
            {
                Console.WriteLine(tdi.ToString());
            }
            
        }

        /// <exclude/>
        [Test]
        public void TestUsGaap2008_TargetRoleTests2()
        {
            //Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
            string fileName = TestCommon.FolderRoot + @"Testing10.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            Assert.AreEqual(0, errors, "should not have any errors");




            s.CurrentLanguage = s.SupportedLanguages[0] as string;
            s.CurrentLabelRole = "preferredLabel";

            List<string> selectedURIs = new List<string>(s.roleRefs.Keys);

            List<Dimension.TargetDimensionInfo> targetExts;
			Assert.IsFalse(Taxonomy.DoesAnyOfTheSelectedRolesNeedTargetRole(new Taxonomy[] { s }, selectedURIs, out targetExts),
                "should not have roles that need target role ");

            List<DimensionNode> titleNodes;
            s.TryGetAllDimensionNodesForDisplay(s.CurrentLanguage, s.currentLabelRole, true,
                out titleNodes);
            int count = 0;
            foreach( DimensionNode titlenode in titleNodes )
            {

                foreach (DimensionNode hyNode in titlenode.children)
                {

                    foreach( DimensionNode dimN in hyNode.children )
                    {

                        foreach (DimensionNode domNode in dimN.children)
                        {

                            if (domNode.Id.Contains("ScenarioUnspecifiedDomain"))
                            {
                                Assert.AreEqual(4, domNode.children.Count, "should have 4 children");
                                count++;
                            }
                        }

                    }
                }

            }

            Assert.AreEqual(5, count, "should have the dimension in 5 places and should all have 4 children");
           

        }

		private StringBuilder DisplayNode(Node n, int level, ref int count, ref int dimensionNodeCount)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < level; ++i)
			{
				sb.Append(" ");
			}
			if (n.Label == "[]" && n.Id == string.Empty)
			{
				Console.WriteLine("Element Id = {0}", n.Id);
				Assert.Fail("Found an empty element");
			}

			sb.Append(n.Label).Append(Environment.NewLine);
			count++;
			if (n is DimensionNode)
			{
				dimensionNodeCount++;
			}
			if (n.Children != null)
			{
				foreach (Node c in n.Children)
				{
					sb.Append(DisplayNode(c, level + 1, ref count, ref dimensionNodeCount ));
				}
			}

			return sb;
		}

		/// <exclude/>
		[Test]
		public void TestPepsi()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = @"\\app1\shared\XBRL Stuff\ID_SECFilers\Pepsi\quarter ended September 6, 2008\pep-20080906.xsd";


			TestUsGaap2008 s = new TestUsGaap2008();

			int errors = 0;
			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");




			s.CurrentLanguage = s.SupportedLanguages[0] as string;
			s.CurrentLabelRole = "preferredLabel";

			ArrayList ret =  s.GetNodesByPresentation();

			
		}
         
        
        [Test]
        public void TestTryGetPresentationSequenceForReviewersGuide1()
        {
            string fileName = @"S:\TESTSCHEMAS\rrd-XBRL Files\rrd-20100331.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            ArrayList nodes = s.GetNodesByPresentation();

            foreach (Node n in nodes)
            {

                Console.WriteLine(n.GetPresentationLink().Title);
            }


            List<string> reportSeq;
            Dictionary<string, string> excelNameToReportNameMap;
            s.TryGetPresentationSequenceForReviewersGuide(null, out excelNameToReportNameMap, out reportSeq);

            foreach (string r in reportSeq)
            {
                Console.WriteLine("Excel SheetName {0} Report name {1}", r, excelNameToReportNameMap[r]);
            }



        }
        [Test]
        public void TestTryGetPresentationSequenceForReviewersGuide2()
        {
            string fileName = @"S:\TESTSCHEMAS\RRD\rrd-20100331.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            ArrayList nodes = s.GetNodesByPresentation();

            foreach (Node n in nodes)
            {

                Console.WriteLine(n.GetPresentationLink().Title);
            }



            List<string> reportSeq;
            Dictionary<string, string> excelNameToReportNameMap;
            s.TryGetPresentationSequenceForReviewersGuide(null, out excelNameToReportNameMap, out reportSeq);

            foreach (string r in reportSeq)
            {
                Console.WriteLine("Excel SheetName {0} Report name {1}", r, excelNameToReportNameMap[r]);
            }



        }

        [Test]
        public void TestTryGetPresentationSequenceForReviewersGuide3()
        {
            string fileName = @"S:\TESTSCHEMAS\RRD-20100630\rrd-20100630.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            ArrayList nodes = s.GetNodesByPresentation();

            foreach (Node n in nodes)
            {

                Console.WriteLine(n.GetPresentationLink().Title);
            }



            List<string> reportSeq;
            Dictionary<string, string> excelNameToReportNameMap;
            s.TryGetPresentationSequenceForReviewersGuide(null, out excelNameToReportNameMap, out reportSeq);

            foreach (string r in reportSeq)
            {
                Console.WriteLine("Excel SheetName {0} Report name {1}", r, excelNameToReportNameMap[r]);
            }



        }
        [Test]
        public void TestUSGaap2009()
        {
            string fileName = @"\\app1\shared\XBRL Stuff\US 2009\Taxonomy\us-gaap-ci-stm-dis-std-2009-01-31.xsd";

            TestUsGaap2008 s = new TestUsGaap2008();

            int errors = 0;
            DateTime start = DateTime.Now;
            if (s.Load(fileName, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            s.Parse(out errors);

            Assert.AreEqual(0, errors, "should not have any errors");



        }

	}
}
#endif
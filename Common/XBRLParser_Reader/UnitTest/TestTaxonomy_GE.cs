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
    public class TestTaxonomy_GE : Taxonomy
    {
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

		/// <exclude/>
		[SetUp]
        public void RunBeforeEachTest()
        { }

		/// <exclude/>
		[TearDown]
        public void RunAfterEachTest()
        { }
        #endregion

        string GE_FILE = TestCommon.FolderRoot + @"GE" + System.IO.Path.DirectorySeparatorChar + "ge-20060331.xsd";

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
        #endregion

		/// <exclude/>
		public TestTaxonomy_GE()
        {
        }

		/// <exclude/>
		[Test]
        public void GL_LoadAndParsePresentation()
        {
            TestTaxonomy_GE s = new TestTaxonomy_GE();
            int errors = 0;

            DateTime start = DateTime.Now;
            if (s.Load(GE_FILE, out errors) != true)
            {
                Assert.Fail((string)s.ErrorList[0]);
            }

            errors = 0;

            // this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
            // parse presentation first
            s.Parse(out errors);

            s.currentLabelRole = @"preferredLabel";
            s.currentLanguage = @"en";
            ArrayList temp = s.GetNodesByPresentation();

            //make sure that role= http://www.xbrl.org/us/fr/lr/role/CashFlowOperationsIndirect
            //has only one child. NetCashFlowsProvidedUsedOperatingActivitiesIndirectAbstract
            int found = 0;
            foreach (Node n in temp)
            {
				if (n.MyPresentationLink.Role == "http://www.xbrl.org/us/fr/lr/role/CashFlowOperationsIndirect")
				{
					found++;
					Assert.AreEqual(1, n.Children.Count, "Should have only one child");
					Assert.AreEqual("usfr-pte_NetCashFlowsProvidedUsedOperatingActivitiesIndirectAbstract", ((Node)(n.Children[0])).Id,
						"Node id is not  correct");
				}
				else if (n.MyPresentationLink.Role == "http://www.xbrl.org/us/fr/lr/role/StatementCashFlows")
				{
					found++;

					Node StatementOfCashFlow = n.Children[1] as Node;

					foreach( Node child in StatementOfCashFlow.Children )
					{
						if( child.Id == "usfr-pte_NetCashFlowsProvidedUsedInvestingActivitiesAbstract" )
						{
							found++;

						}
					}


				}
            }
            Assert.AreEqual(3, found, "Failed to find the link http://www.xbrl.org/us/fr/lr/role/CashFlowOperationsIndirect");



        }

      
    }
}
#endif
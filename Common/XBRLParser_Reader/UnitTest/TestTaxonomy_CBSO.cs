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
	using System.Text;
	using System.Collections;
	using System.Diagnostics;

	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture]
	public class TestTaxonomy_CBSO : Taxonomy
	{
		/// <exclude/>
		protected Hashtable tupleElements = new Hashtable(1000);
		
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestTaxonomy_CBSO Comments***" );
			
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Trace.Listeners.Clear();
			Console.WriteLine( "***End TestTaxonomy_CBSO Comments***" );
		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{}
		#endregion

		string CBSO_04_07_FILE = TestCommon.FolderRoot + @"cbso-rf-2005-04-07" +System.IO.Path.DirectorySeparatorChar +"cbso-rf-2005-04-07.xsd";

		#region Helpers
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
		protected void SendWarningsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
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
		
		#region TestTuples

		/// <exclude/>
		[Test]
		public void CBSO_TestOneTupleChildWithMultipleParents()
		{
			//load and parse
			TestTaxonomy_CBSO s = new TestTaxonomy_CBSO();
			int errors = 0;

			s.Load( CBSO_04_07_FILE );
			s.Parse( out errors );

			Assert.IsNotNull(s,"failed to load and parse taxonomy");

			//CBSO has three tuples that contain the same child (cbso-rf_IdentificationCode)

			// find MostImportantAssociateConsolidated - it's a tuple that has 5 children
			Node MIAC = new Node (s.allElements[ "cbso-rf_MostImportantAssociateConsolidated" ] as Element);
			Assert.IsNotNull( MIAC, "can't find cbso-rf_MostImportantAssociateConsolidated" );
			Assert.AreEqual(5,MIAC.MyElement.TupleChildren.Count,"wrong number of children in cbso-rf_MostImportantAssociateConsolidated");
			
			//confirm this tuple contains cbso-rf_IdentificationCode
			bool found = false;
			foreach ( Element ec in MIAC.MyElement.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "cbso-rf_IdentificationCode" )
				{
					found = true;
					break;
				}
			}
			Assert.IsTrue( found, "could not find IdentificationCode in cbso-rf_MostImportantAssociateConsolidated" );

			//verify each child element has the correct parent
			foreach ( Element ec in MIAC.MyElement.TupleChildren.GetValueList() )
			{
				//create a node from the element and set it's parent node
				Node n = new Node(ec);
				MIAC.AddChild(n);

				if ( n.Parent.Id.CompareTo("cbso-rf_MostImportantAssociateConsolidated") != 0 )
				{
					Assert.Fail(ec.Id + " does not have cbso-rf_MostImportantAssociateConsolidated as its parent");
				}
			}

			// find MostImportantJointVentureConsolidated - it's a tuple that has 4 children
			Node MIJVC = new Node(s.allElements[ "cbso-rf_MostImportantJointVentureConsolidated" ] as Element);
			Assert.IsNotNull( MIJVC, "cbso-rf_MostImportantJointVentureConsolidated" );
			Assert.AreEqual(4,MIJVC.MyElement.TupleChildren.Count,"wrong number of children in cbso-rf_MostImportantJointVentureConsolidated");

			//confirm this tuple contains cbso-rf_IdentificationCode
			found = false;
			foreach ( Element ec in MIJVC.MyElement.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "cbso-rf_IdentificationCode" )
				{
					found = true;
					break;
				}
			}
			Assert.IsTrue( found, "could not find IdentificationCode in cbso-rf_MostImportantJointVentureConsolidated" );

			//verify each child element has the correct parent
			foreach ( Element ec in MIJVC.MyElement.TupleChildren.GetValueList() )
			{
				//create a node from the element and set it's parent node
				Node n = new Node(ec);
				MIJVC.AddChild(n);
				
				if ( n.Parent.Id.CompareTo("cbso-rf_MostImportantJointVentureConsolidated") != 0 )
				{
					Assert.Fail(ec.Id + " does not have cbso-rf_MostImportantJointVentureConsolidated as its parent");
				}
			}

			// find MostImportantSubsidiaryConsolidated - it's a tuple that has 5 children
			Node MISC = new Node(s.allElements[ "cbso-rf_MostImportantSubsidiaryConsolidated" ] as Element);
			Assert.IsNotNull( MISC, "can't find cbso-rf_MostImportantSubsidiaryConsolidated" );
			Assert.AreEqual(5,MISC.MyElement.TupleChildren.Count,"wrong number of children in cbso-rf_MostImportantSubsidiaryConsolidated");

			//confirm this tuple contains cbso-rf_IdentificationCode
			found = false;
			foreach ( Element ec in MISC.MyElement.TupleChildren.GetValueList() )
			{
				if ( ec.Id == "cbso-rf_IdentificationCode" )
				{
					found = true;
					break;
				}
			}
			Assert.IsTrue( found, "could not find IdentificationCode in cbso-rf_MostImportantAssociateConsolidated" );
			
			//verify each child element has the correct parent
			foreach ( Element ec in MISC.MyElement.TupleChildren.GetValueList() )
			{
				//create a node from the element and set it's parent node
				Node n = new Node(ec);
				MISC.AddChild(n);
				
				if ( n.Parent.Id.CompareTo("cbso-rf_MostImportantSubsidiaryConsolidated") != 0 )
				{
					Assert.Fail(ec.Id + " does not have cbso-rf_MostImportantSubsidiaryConsolidated as its parent");
				}
			}
		}

		#endregion
	}
}
#endif
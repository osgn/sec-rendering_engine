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
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
	using System.Collections.Generic;
    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;
	using System.Diagnostics;

	/// <exclude/>
	[TestFixture] 
	public class TestRoleRef : RoleRef
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{}

		#endregion

		/// <exclude/>
		public string Uri
		{
			get { return uri; }
		}

		/// <exclude/>
		public string Href
		{
			get { return this.GetHref(); }
		}

		/// <exclude/>
		public TestRoleRef()
		{
		}

		/// <exclude/>
		public TestRoleRef(RoleRef rr)
			: base(rr)
		{
		}

		/// <exclude/>
		[Test]
		public void TestAddAndMergeFileReference()
		{
			RoleRef rr = new RoleRef();
			rr.AddFileReference( null );
			Assert.IsNull( rr.GetFileReferences(), "should not have any file refs" );
			rr.AddFileReference( string.Empty );
			Assert.IsNull( rr.GetFileReferences(), "should not have any file refs" );

			rr.AddFileReference( "a.xsd" );
			Assert.AreEqual( 1, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "a.xsd", rr.GetFileReferences()[0], "wrong file ref added" );
			rr.AddFileReference( "b.xsd" );
			Assert.AreEqual( 2, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "b.xsd", rr.GetFileReferences()[1], "wrong file ref added" );
			rr.AddFileReference( "b.xsd" );
			Assert.AreEqual( 2, rr.GetFileReferences().Count, "wrong num file refs" );

			rr.MergeFileReferences( null );
			Assert.AreEqual( 2, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "a.xsd", rr.GetFileReferences()[0], "wrong file ref added" );
			Assert.AreEqual( "b.xsd", rr.GetFileReferences()[1], "wrong file ref added" );
			rr.MergeFileReferences( new List<string>() );
			Assert.AreEqual( 2, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "a.xsd", rr.GetFileReferences()[0], "wrong file ref added" );
			Assert.AreEqual( "b.xsd", rr.GetFileReferences()[1], "wrong file ref added" );


			List<string> merge = new List<string>( rr.GetFileReferences() );
			rr.MergeFileReferences( merge );
			Assert.AreEqual( 2, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "a.xsd", rr.GetFileReferences()[0], "wrong file ref added" );
			Assert.AreEqual( "b.xsd", rr.GetFileReferences()[1], "wrong file ref added" );

			merge.Add( "c.xsd" );
			merge.Add( "d.xsd" );
			rr.MergeFileReferences( merge );
			Assert.AreEqual( 4, rr.GetFileReferences().Count, "wrong num file refs" );
			Assert.AreEqual( "a.xsd", rr.GetFileReferences()[0], "wrong file ref added" );
			Assert.AreEqual( "b.xsd", rr.GetFileReferences()[1], "wrong file ref added" );
			Assert.AreEqual( "c.xsd", rr.GetFileReferences()[2], "wrong file ref added" );
			Assert.AreEqual( "d.xsd", rr.GetFileReferences()[3], "wrong file ref added" );

		}

		/// <exclude/>
		[Test]
		public void TestGetSchemaName()
		{
			this.href = @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd#IncomeStatement#";

			Assert.AreEqual( @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd", this.GetSchemaName(), "did not get schema name" );
		}

		/// <exclude/>
		[Test]
		public void TestGetId()
		{
			this.href = @"http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28.xsd#IncomeStatement#";

			Assert.AreEqual( @"IncomeStatement", this.GetId(), "did not get id" );
		}

	}
}
#endif
// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the �License�); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an �AS IS� basis, WITHOUT WARRANTY OF ANY KIND, 
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
	using System.Collections;
	using System.Diagnostics;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestRoleType : RoleType
    {
		/// <exclude/>
		public TestRoleType()
		{
		}

		/// <exclude/>
		public TestRoleType(RoleType rt)
			: base(rt)
		{
		}

		/// <exclude/>
		

		/// <exclude/>
		public ArrayList WhereUsed
		{
			get { return whereUsed; }
		}

		
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
		[Test]
		public void Test_WhereUsed()
		{
			RoleType rt = new RoleType( "uri", "id", "schemaFile" );

			rt.AddLink( "link:one" );
			rt.AddLink( "link:two" );
			rt.AddLink( "link:Three" );

			Assert.IsTrue( rt.UsedIn( "one" ) );
			Assert.IsTrue( rt.UsedIn( "two" ) );
			Assert.IsFalse( rt.UsedIn( "seven" ) );
		}
    }
}
#endif
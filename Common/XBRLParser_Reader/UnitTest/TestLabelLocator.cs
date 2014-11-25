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
	using System.Text;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestLabelLocator : LabelLocator
    {
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {}

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
		public void Test_XmlFragment()
		{
			TestLabelLocator tll = new TestLabelLocator();

			tll.AddLabel( "en", "role1", "aaa" );
			tll.AddLabel( "en", "role2", "bbb" );
			tll.AddLabel( "en", "role3", "ccc" );

			tll.AddLabel( "foo", "role1", "zzz" );
			tll.AddLabel( "foo", "role2", "yyy" );
			tll.AddLabel( "foo", "role3", "xxx" );

			
			StringBuilder xml = new StringBuilder();

			tll.WriteXmlFragment( "en", xml );

			Assert.AreEqual( " lang=\"en\" role1=\"aaa\" role2=\"bbb\" role3=\"ccc\"", xml.ToString() );

			xml.Remove( 0, xml.Length );

			tll.WriteXmlFragment( "foo", xml );
			Assert.AreEqual( " lang=\"foo\" role1=\"zzz\" role2=\"yyy\" role3=\"xxx\"", xml.ToString() );
		}
    }
}
#endif
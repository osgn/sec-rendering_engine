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
	using System.Collections;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestChildPresentationLocator : ChildPresentationLocator
    {
		#region constructors
		/// <exclude/>
		public TestChildPresentationLocator(string href, LocatorRelationshipInfo lri)
			: base(href, lri)
		{
		}

		#endregion

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
		public void TestCanAddRelationship()
		{
			LocatorRelationshipInfo lri = LocatorRelationshipInfo.CreateObj( "label", "1", 1F,1F, "pref label", "1", false );
			LocatorRelationshipInfo lri2 = LocatorRelationshipInfo.CreateObj( "label", "2", 1F,1F, "pref label", "1", false );
			ChildPresentationLocator cpl = new ChildPresentationLocator( "href1", lri );

			Assert.IsFalse( cpl.CanAddRelationship( lri ), "can add dup" );
			Assert.IsTrue( cpl.CanAddRelationship( lri2 ), "can't add new val" );
		}

		/// <exclude/>
		[Test]
		public void TestAddReplaceRelationship()
		{
			LocatorRelationshipInfo lri = LocatorRelationshipInfo.CreateObj( "label", "1", 1F,1F, "pref label", "1", false );
			LocatorRelationshipInfo lri2 = LocatorRelationshipInfo.CreateObj( "label", "2", 1F,1F, "pref label", "1", true );

			TestChildPresentationLocator cpl = new TestChildPresentationLocator( "href1", lri );

			cpl.AddRelationship( lri2 );

			Assert.AreEqual( 1, cpl.LocatorRelationshipInfos.Count, "wrong number of LRI's" );
			Assert.AreEqual( lri2, cpl.LocatorRelationshipInfos[0], "LRI is wrong" );
		}

		/// <exclude/>
		[Test]
		public void TestAddReplaceRelationship2()
		{
			LocatorRelationshipInfo lri = LocatorRelationshipInfo.CreateObj( "label", "1", 1F,1F, "pref label", "1", false );
			LocatorRelationshipInfo lri2 = LocatorRelationshipInfo.CreateObj( "label", "2", 1F,1F, "pref label", "1", true );
			LocatorRelationshipInfo lri3 = LocatorRelationshipInfo.CreateObj( "label", "3", 1F,1F, "pref label", "1", false );

			TestChildPresentationLocator cpl = new TestChildPresentationLocator( "href1", lri );

			cpl.AddRelationship( lri2 );

			Assert.AreEqual( 1, cpl.LocatorRelationshipInfos.Count, "wrong number of LRI's" );
			Assert.AreEqual( lri3, cpl.LocatorRelationshipInfos[0], "LRI is wrong" );
		}

		/// <exclude/>
		[Test]
		public void TestAddReplaceRelationship3()
		{
			LocatorRelationshipInfo lri = LocatorRelationshipInfo.CreateObj( "label", "1", 1F,1F, "pref label", "1", false );
			LocatorRelationshipInfo lri2 = LocatorRelationshipInfo.CreateObj( "label", "2", 1F,1F, "pref label", "1", true );
			LocatorRelationshipInfo lri3 = LocatorRelationshipInfo.CreateObj( "label2", "1", 1F,1F, "pref label", "1", false );

			TestChildPresentationLocator cpl = new TestChildPresentationLocator( "href1", lri );

			cpl.AddRelationship( lri2 );

			Assert.AreEqual( 2, cpl.LocatorRelationshipInfos.Count, "wrong number of LRI's" );
			Assert.AreEqual( lri2, cpl.LocatorRelationshipInfos[0], "LRI 0 is wrong" );
			Assert.AreEqual( lri3, cpl.LocatorRelationshipInfos[1], "LRI 1 is wrong" );
		}
	}
}
#endif
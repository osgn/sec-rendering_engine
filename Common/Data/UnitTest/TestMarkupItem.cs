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
namespace Aucent.MAX.AXE.Common.Data.Test
{
	using System;

	using NUnit.Framework;

	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture]
	public class TestMarkupItem : MarkupItem
	{
		#region init
        
		///<exclude/>
		[TestFixtureSetUp] public void RunFirst()
		{}

		///<exclude/>
		[TestFixtureTearDown] public void RunLast() 
		{}

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{
			TestMarkupItem tmi = new TestMarkupItem();
			this.Name = tmi.Name;
			this.IsDefaultForEntity = tmi.IsDefaultForEntity;
			this.InUseCount = 0;
		}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{
			if (this.TreeNodes != null)
                this.TreeNodes.Clear();
		}

		#endregion

		/// <exclude/>
		public TestMarkupItem()
		{
		}

		/// <exclude/>
		[Test]
		public void TestEquals()
		{
			const string error = "Invalid return from Equals method";

			Assert.IsFalse(this.Equals(null), error);
			Assert.IsFalse(this.Equals(error), error);

			TestMarkupItem tmi = new TestMarkupItem();

			Assert.IsTrue(this.Equals(this), error);
			Assert.IsTrue(this.Equals(tmi), error);

			tmi.Name = "test";
			Assert.IsFalse(this.Equals(tmi), error);
			this.Name = tmi.Name;
			Assert.IsTrue(this.Equals(tmi), error);

			this.IsDefaultForEntity = false;
			tmi.IsDefaultForEntity = true;
			Assert.IsFalse(this.Equals(tmi), error);
			this.IsDefaultForEntity = tmi.IsDefaultForEntity;
			Assert.IsTrue(this.Equals(tmi), error);
		}

		/// <exclude/>
		[Test]
		public void TestValueEquals()
		{
			const string error = "Invalid return from ValueEquals method";
			Assert.IsFalse(this.ValueEquals(null), error);
			Assert.IsFalse(this.ValueEquals(error), error);

			TestMarkupItem tmi = new TestMarkupItem();

			Assert.IsTrue(this.ValueEquals(this), error);
			Assert.IsTrue(this.ValueEquals(tmi), error);
		}

		/// <exclude/>
		[Test]
		public void TestIncrement()
		{
			IncrementInUseCount(1);
			Assert.AreEqual( 1, InUseCount, "Did not increment by 1");
			InUseCount = 10;
			IncrementInUseCount( 5 );
			Assert.AreEqual( 15, InUseCount, "Did not increment by 5");

		}

		/// <exclude/>
		[Test]
		public void TestDecrement()
		{
			DecrementInUseCount(10);
			Assert.AreEqual(0, InUseCount, "Count should be 0");
			InUseCount = 10;
			DecrementInUseCount(6);
			Assert.AreEqual( 4, InUseCount, "Decrement failed");
		}

		/// <exclude/>
		[Test]
		public void TestAddTreeNode()
		{
            object obj = new object();
            object obj2 = new object();

			AddTreeNode( obj );
			Assert.AreEqual(1, TreeNodes.Count, "Incorrect number of nodes");
			AddTreeNode( obj2 );
			Assert.AreEqual(2, TreeNodes.Count, "Incorrect number of nodes");
			
			// try adding same one again, should not add it
			AddTreeNode( obj );
			Assert.AreEqual(2, TreeNodes.Count, "Incorrect number of nodes");
            AddTreeNode(obj2);
			Assert.AreEqual(2, TreeNodes.Count, "Incorrect number of nodes");
		}

		/// <exclude/>
		[Test]
		public void TestGetTreeNodeAt()
		{
            object obj = new object();
            object obj2 = new object();

			AddTreeNode( obj );
			Assert.AreEqual(1, TreeNodes.Count, "Incorrect number of nodes");
			AddTreeNode( obj2 );
			Assert.AreEqual(2, TreeNodes.Count, "Incorrect number of nodes");

			object badObj = GetTreeNodeAt(5);
			Assert.IsNull(badObj, "Oject should not have been found");

			object tmp1 = GetTreeNodeAt(0);
            Assert.AreSame(obj, tmp1, "Tree node at index is not correct");
			object tmp2 = GetTreeNodeAt(1);
            Assert.AreSame(obj2, tmp2, "Tree node at index is not correct");
		}

		/// <exclude/>
		[Test]
		public void TestGetTreeNodeContainerAt()
		{
            Precision obj = new Precision();
            Precision obj2 = new Precision();

			AddTreeNode( obj );
			Assert.AreEqual(1, TreeNodes.Count, "Incorrect number of nodes");
			AddTreeNode( obj2 );
			Assert.AreEqual(2, TreeNodes.Count, "Incorrect number of nodes");

            Precision badObj = GetTreeNodeContainerAt(5) as Precision;
			Assert.IsNull(badObj, "Oject should not have been found");

            Precision tmp1 = GetTreeNodeContainerAt(0) as Precision;
			Assert.AreSame(obj, tmp1, "Tree node at index is not correct");
            Precision tmp2 = GetTreeNodeContainerAt(1) as Precision;
			Assert.AreSame(obj2, tmp2, "Tree node at index is not correct");
		}
	}
}
#endif
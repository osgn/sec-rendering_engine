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
    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.XBRLParser.Interfaces;

	/// <exclude/>
	public class TestLinkableImpl : ILinkable
	{
		/// <exclude/>
		public Linker _linker = new Linker();
		/// <exclude/>
		public string id;

		/// <exclude/>
		public TestLinkableImpl()
		{
		}

		/// <exclude/>
		public TestLinkableImpl(string key)
		{
			id = key;
		}

		#region ILinkable Members

		/// <exclude/>
		public string MyKey
		{
			get	{ return id; }
		}

		/// <exclude/>
		public ILink LinkHelper
		{
			get	{ return _linker; }
		}

		/// <exclude/>
		public bool IsLinkedTo(ILinkable child)
		{
			return _linker.IsLinkedTo( child );
		}

		#endregion

		/// <exclude/>
		public void Link(ILinkable child)
		{
			_linker.Link( this, child );
		}

		/// <exclude/>
		public void Unlink(ILinkable child)
		{
			_linker.Unlink( this, child );
		}
	}

	/// <exclude/>
	[TestFixture] 
    public class TestLinker : Linker
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

		#region comparisons

		/// <exclude/>
		[Test]
		public void TestAddWrappers()
		{
			TestLinkableImpl mw1 = new TestLinkableImpl("key 1");
			TestLinkableImpl fw1 = new TestLinkableImpl( "foot b");
			mw1.Link( fw1 );

			TestLinkableImpl fw2 = new TestLinkableImpl( "foot c" );
			mw1.Link( fw2 );
			
			TestLinkableImpl fw3 = new TestLinkableImpl( "foot a" );
			mw1.Link( fw3 );

			Assert.IsTrue( ((ILinkable)mw1).IsLinkedTo( fw1 ), "footnotes a wrong" );
			Assert.IsTrue( ((ILinkable)mw1).IsLinkedTo( fw2 ), "footnotes b wrong" );
			Assert.IsTrue( ((ILinkable)mw1).IsLinkedTo( fw3 ), "footnotes c wrong" );
		}

		/// <exclude/>
		[Test]
		public void TestRemoveWrappers()
		{
			TestLinkableImpl mw1 = new TestLinkableImpl( "key 1");
			TestLinkableImpl fw1 = new TestLinkableImpl( "foot b" );

			mw1.Link( fw1 );

			TestLinkableImpl fw2 = new TestLinkableImpl( "foot c" );

			mw1.Link( fw2 );
			
			TestLinkableImpl fw3 = new TestLinkableImpl( "foot a" );

			mw1.Link( fw3 );

			mw1.Unlink( fw1 );

			Assert.AreEqual( 2, mw1._linker.links.Count, "wrong number of associated wrappers" );

			Assert.IsTrue( mw1._linker.links.ContainsKey( "foot c" ), "mw1 missing foot c"  );
			Assert.IsTrue( mw1._linker.links.ContainsKey( "foot a" ), "mw1 missing foot a"  );

			Assert.IsTrue( fw2._linker.links.Count == 1, "wrong count for fw2" );
			Assert.IsTrue( fw3._linker.links.Count == 1, "wrong count for fw3" );
		}
		#endregion

    }
}
#endif
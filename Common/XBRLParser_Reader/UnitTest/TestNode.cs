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
    public class TestNode : Node
    {
		/// <exclude/>
		public new Node Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <exclude/>
		public TestNode()
		{
		}

		/// <exclude/>
		public TestNode(string name)
			: base(name)
		{
		}

		/// <exclude/>
		public TestNode(string id, Element e)
			: base(e)
		{
		}

		/// <exclude/>
		public new double Order
		{
			get { return this.order; }
			set { this.order = value; }
		}

		/// <exclude/>
		public static string ToXml(int numTabs, Node n)
		{
			StringBuilder sb = new StringBuilder();

			for ( int i=0; i < numTabs; ++i )
			{
				sb.Append( "\t" );
			}

			sb.Append( "<" ).Append( n.Label ).Append( " abstract=\"" ).Append( n.IsAbstract.ToString() ).Append( "\"" );

			if ( n.Children != null )
			{
				sb.Append( ">" ).Append( Environment.NewLine );

				foreach ( Node c in n.Children )
				{
					sb.Append( TestNode.ToXml( numTabs+1, c ) );
				}

				for ( int i=0; i < numTabs; ++i )
				{
					sb.Append( "\t" );
				}

				sb.Append( "</" ).Append( n.Label ).Append( ">" ).Append( Environment.NewLine );
			}
			else
			{
				sb.Append( "/>" ).Append( Environment.NewLine );
			}

			return sb.ToString();
		}

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
		public void Test_NodesEqual()
		{
			Node n1 = new Node( "n1" );
			Node n2 = new Node( "n1" );

			Assert.IsTrue( n1.Equals( n2 ), "n1 != n2" );

			Node n3 = new Node( new Element( "e3" ) );

			Node n4 = new Node( new Element( "e3" ) );

			Assert.IsTrue( n3.Equals( n4 ), "n3 != n4" );

			Node n5 = new Node( "n5" );
			n5.Tag = "same";
			Node n6 = new Node( "n5" );
			n6.Tag = "same";

			Assert.IsTrue( n5.Equals( n6 ), "n5 != n6" );

			Node n7 = new Node( "n7" );
			n7.AddChild( n3 );

			Node n8 = new Node( "n7" );
			n8.AddChild( n4 );

			Assert.IsTrue( n7.Equals( n8 ), "n7 != n8" );
		}

		/// <exclude/>
		[Test]
		public void Test_NodesNotEqual()
		{
			Node n1 = new Node( "n1" );
			Node n2 = new Node( "n2" );

			Assert.IsFalse( n1.Equals( n2 ), "n1 == n2" );

			Node n3 = new Node( new Element( "e3" ) );

			Node n4 = new Node( new Element( "e4" ) );

			Assert.IsFalse( n3.Equals( n4 ), "n3 == n4" );

			Node n5 = new Node( "n5" );
			n5.Tag = "same";
			Node n6 = new Node( "n6" );
			n6.Tag = "different";

			Assert.IsFalse( n5.Equals( n6 ), "n5 == n6" );

			Node n7 = new Node( "n7" );
			n7.AddChild( n3 );

			Node n8 = new Node( "n8" );
			n8.AddChild( n4 );

			Assert.IsFalse( n7.Equals( n8 ), "n7 == n8" );

			Node c1 = new Node( "c1" );
			Node n9 = new Node( "n9" );
			n9.AddChild( c1 );

			Node c2 = new Node( "c1" );
			Node n10 = new Node( "n9" );
			n10.AddChild( c2 );

			// ignore this test - nodes considered equal even though they have different parents
			Assert.IsTrue( c1.Equals( c2 ), "c1 == c2" );

		}

		
		/// <exclude/>
		[Test]
		public void TestGetLabelWithoutLeadingCharacters()
		{
			// other tests need to be written, just doing quick
			// test to see if there is only 1 char & it gets stripped what happens.

			this.Label = "?";

			string str = this.GetLabelWithoutLeadingCharacters();

			Assert.AreEqual( Label, str, "should not remove if only 1 char" );

			this.Label = "?ABCD";

			str = this.GetLabelWithoutLeadingCharacters();

			Assert.AreEqual( "ABCD", str, "should not remove if only 1 char" );
		}
	}
}
#endif
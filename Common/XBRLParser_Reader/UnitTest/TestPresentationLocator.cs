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
	using System.Xml;
	using System.Collections;
	using System.Collections.Specialized;

	using System.Text;
	using System.Diagnostics;

    using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
	public class TestPresentationLocator : PresentationLocator
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Console.WriteLine( "***Start TestPresentationLocator Comments***" );
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Console.WriteLine( "***End TestPresentationLocator Comments***" );
		}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{
			this.childDisplayOrder = null;
		}

		#endregion

		#region constructor
		/// <exclude/>
		public TestPresentationLocator()
		{
		}

		/// <exclude/>
		public TestPresentationLocator(PresentationLocator pl)
			: base(pl)
		{
		}

		/// <exclude/>
		public TestPresentationLocator(string label, string href)
			: base(href)
		{
			labelArray.Add( label );
		}

		#endregion

		#region extended methods 
		/// <exclude/>
		public int NumProhibited()
		{
			int count = 0;

			if( childLocatorsByHRef != null )
			{
				foreach( ChildPresentationLocator cpl in childLocatorsByHRef.Values )
				{
					if( ((LocatorRelationshipInfo)(cpl.LocatorRelationshipInfos[cpl.LocatorRelationshipInfos.Count-1])).IsProhibited )
					{
						count++;
					}

				}

			}

			return count;
		}

	
		#endregion

		/// <exclude/>
		[Test]
		public void CreatePresentationLocator()
		{
			TestPresentationLocator pl = new TestPresentationLocator();
		}



	

		/// <exclude/>
		[Test]
		public void CheckOrderDiffPriority()
		{
			PresentationLocator parent	= new TestPresentationLocator( "parent", "xsd#parent" );
			ArrayList errors = new ArrayList();

			parent.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator child	= new TestPresentationLocator( "child", "xsd#child" );
			PresentationLocator child2	= new TestPresentationLocator( "child2", "xsd#child2" );
//			PresentationLocator child3	= new TestPresentationLocator( "child_7.0_pro", "xsd#child" );

			child.ParseHRef( errors );
			//child.MyElement = new Element( "child_1.7", false );
			Assert.AreEqual( 0, errors.Count );

			child2.ParseHRef( errors );
			//child2.MyElement = new Element( "child_7.0_opt", false );
			Assert.AreEqual( 0, errors.Count );
			
			//child3.ParseHRef( errors );
			//child3.MyElement = new Element( "child_7.0_pro", false );
			//Assert.AreEqual( 0, errors.Count );

			PresentationLink pl = new PresentationLink("Test", "Test", new ArrayList() );
			pl.AddLocator( parent );
			pl.AddLocator( child );
			pl.AddLocator( child2 );

			parent.AddChild( child, "Label", "0", 1.7F, "label", "0", false );
			parent.AddChild( child, "Label", "0", 7.0F, "label", "0", false );
			parent.AddChild( child, "Label", "2", 7.0F, "label", "0", true );

			parent.AddChild( child2, "Label", "0", 2.0F, "label", "0", false );

//			pl.AddLocator( child3 );

			Node n = parent.CreateNode( "en", "", "", pl );

			//should be:
			//	child (1.7)
			//	child2 (2.0)
			//  child - prohibited (7.0)

			Assert.AreEqual( "parent", n.Label, "parent expected" );
			Assert.AreEqual( "child", ((Node)n.Children[0]).Label, "child 0 wrong" );
			Assert.AreEqual( "child2", ((Node)n.Children[1]).Label, "child 1 wrong" );

			Assert.AreEqual( 3, n.Children.Count, "wrong number of children" );
			Assert.AreEqual( "child", ((Node)n.Children[2]).Label, "child 2 wrong" );

			Assert.IsFalse( ((Node)n.Children[0]).IsProhibited, "child 0 is not prohibited" );
			Assert.IsFalse( ((Node)n.Children[1]).IsProhibited, "child 1 is not prohibited" );
			Assert.IsTrue( ((Node)n.Children[2]).IsProhibited, "child 2 is  prohibited" );
		}

		/// <exclude/>
		[Test]
		public void CheckAppend()
		{
			ArrayList errors = new ArrayList();

			TestPresentationLocator parent = new TestPresentationLocator( "label", "parent#parent" );
			TestPresentationLocator parentToMerge = new TestPresentationLocator( "label", "parent#parent" );

			PresentationLocator pl1 = new PresentationLocator( "child#child" );
			pl1.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl2 = new PresentationLocator( "child#child2" );
			pl2.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			parentToMerge.AddChild( pl1, 1.0F, "pref", "1" );
			parentToMerge.AddChild( pl2, 2.0F, "pref", "1" );

			parent.Append( parentToMerge );

			Assert.AreEqual( parent.childDisplayOrder.Count, 2, "wrong number of orders" );
			Assert.AreEqual( parent.childLocatorsByHRef.Count, 2, "wrong number of href's" );
		}

		/// <exclude/>
		[Test]
		public void CheckAppend2()
		{
			ArrayList errors = new ArrayList();

			TestPresentationLocator parent = new TestPresentationLocator( "label", "parent#parent" );
			TestPresentationLocator parentToMerge = new TestPresentationLocator( "label", "parent#parent" );

			PresentationLocator pl1 = new PresentationLocator( "child#child" );
			pl1.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl2 = new PresentationLocator( "child#child2" );
			pl2.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl3 = new PresentationLocator( "child#child" );
			pl3.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl4 = new PresentationLocator( "child#child2" );
			pl4.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			parent.AddChild( pl1, 1.0F, "pref", "1" );
			parent.AddChild( pl2, 2.0F, "pref", "1" );
			
			parentToMerge.AddChild( pl3, 1.0F, "pref", "1" );
			parentToMerge.AddChild( pl4, 2.0F, "pref", "1" );

			parent.Append( parentToMerge );

			Assert.AreEqual( parent.childDisplayOrder.Count, 2, "wrong number of orders" );
			Assert.AreEqual( parent.childLocatorsByHRef.Count, 2, "wrong number of href's" );
		}

		/// <exclude/>
		[Test]
		public void CheckAppend3()
		{
			ArrayList errors = new ArrayList();
			
			TestPresentationLocator parent = new TestPresentationLocator( "label", "parent#parent" );
			TestPresentationLocator parentToMerge = new TestPresentationLocator( "label", "parent#parent" );

			PresentationLocator pl1 = new PresentationLocator( "child#child" );
			pl1.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl2 = new PresentationLocator( "child#child2" );
			pl2.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );
			
			PresentationLocator pl3 = new PresentationLocator( "child#child" );
			pl3.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			PresentationLocator pl4 = new PresentationLocator( "child#child2" );
			pl4.ParseHRef( errors );
			Assert.AreEqual( 0, errors.Count );

			parent.AddChild( pl1, 1.0F, "pref", "1" );
			parent.AddChild( pl2, 2.0F, "pref", "1" );

			parentToMerge.AddChild( pl3, 1.0F, "pref", "1" );
			parentToMerge.AddChild( pl4,"label", "2", 2.0F, "pref", "1", true );

			parent.Append( parentToMerge );

			Assert.AreEqual( parent.childDisplayOrder.Count, 2, "wrong number of orders" );
			Assert.AreEqual( parent.childLocatorsByHRef.Count, 2, "wrong number of href's" );
			int count = ((ChildPresentationLocator)parent.childLocatorsByHRef[parent.childDisplayOrder[2.0F]]).LocatorRelationshipInfos.Count;
			LocatorRelationshipInfo lri = ((ChildPresentationLocator)parent.childLocatorsByHRef[parent.childDisplayOrder[2.0F]]).LocatorRelationshipInfos[count - 1] as LocatorRelationshipInfo;
			Assert.IsTrue(lri.IsProhibited);

		}
	}
}
#endif
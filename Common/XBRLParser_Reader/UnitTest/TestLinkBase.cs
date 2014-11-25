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
    public class TestLinkBase : LinkBase
    {
		/// <exclude/>
		public TestLinkBase()
		{
		}

		/// <exclude/>
		public TestLinkBase(ArrayList errors)
			: base(errors)
		{
		}

		#region Overrides
		/// <exclude/>
		internal override bool LoadArc(System.Xml.XmlNode child)
		{
			return false;
		}

		/// <exclude/>
		internal override bool LoadLocator(System.Xml.XmlNode child)
		{
			return false;
		}

		/// <exclude/>
		internal override bool LoadResource(System.Xml.XmlNode child)
		{
			return false;
		}

		/// <exclude/>
		internal override LocatorBase CreateLocator(string href)
		{
			return null;
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
    }
}
#endif
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
	using System.Collections;

	using NUnit.Framework;

	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture]
	public class TestScenario : Scenario
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
			Scenario s = new Scenario();
			this.Name = s.Name;
			this.ValueName = s.ValueName;
			this.ValueType = s.ValueType;
			this.Schema = s.Schema;
			this.Namespace = s.Namespace;
		}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}

		#endregion

		/// <exclude/>
		public TestScenario()
		{
		}

		/// <exclude/>
		[Test]
		public void TestIsValid()
		{
			Name = "Test";
			ValueName = "test";
			ValueType = "test";
			Schema = "test";
			Namespace = "test";

			const string errorReturn = "Invalid return value from IsValid method";
			const string errorCount = "Invalid error count";
			ArrayList errors = new ArrayList();

			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);

			ValueName = string.Empty;
			Assert.AreEqual(false, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(1, errors.Count, errorCount);
			ValueName = null;
			Assert.AreEqual(false, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(1, errors.Count, errorCount);
			ValueName = "test";

			ValueType = string.Empty;
			Assert.AreEqual(false, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(1, errors.Count, errorCount);
			ValueType = null;
			Assert.AreEqual(false, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(1, errors.Count, errorCount);
			ValueType = "test";
		
			Schema = string.Empty;
			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);
			Schema = null;
			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);
			Schema = "test";

			Namespace = string.Empty;
			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);
			Namespace = null;
			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);
			Namespace = "test";

			ValueName = null;
			ValueType = null;
			Schema = null;
			Namespace = null;
			Assert.AreEqual(false, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(2, errors.Count, errorCount);
			Assert.AreEqual(false, IsValid(MarkupTypeCode.MAX, out errors, false), errorReturn);
			Assert.AreEqual(2, errors.Count, errorCount);
		}

		/// <exclude/>
		[Test]
		public void TestValueEquals()
		{
			const string error = "Invalid return from ValueEquals method";

			TestScenario s = new TestScenario();

			Assert.IsTrue(this.ValueEquals(s), error);

			string testStr = "test";

			s.ValueName = testStr;
			Assert.IsFalse(this.ValueEquals(s), error);
			this.ValueName = s.ValueName;
			Assert.IsTrue(this.ValueEquals(s), error);

			s.ValueType = testStr;
			Assert.IsFalse(this.ValueEquals(s), error);
			this.ValueType = s.ValueType;
			Assert.IsTrue(this.ValueEquals(s), error);

			s.Namespace = testStr;
			Assert.IsFalse(this.ValueEquals(s), error);
			this.Namespace = s.Namespace;
			Assert.IsTrue(this.ValueEquals(s), error);

			s.Schema = testStr;
			Assert.IsFalse(this.ValueEquals(s), error);
			this.Schema = s.Schema;
			Assert.IsTrue(this.ValueEquals(s), error);
		}

		/// <exclude/>
		[Test]
		public void Bug_669_ColonInvalidInNamespace()
		{
			Name = "Test";
			ValueName = "test";
			ValueType = "test";
			Schema = "test";
			Namespace = "http://test";

			const string errorReturn = "Invalid return value from IsValid method";
			const string errorCount = "Invalid error count";
			ArrayList errors = new ArrayList();

			Assert.AreEqual(true, IsValid(MarkupTypeCode.XBRL, out errors, false), errorReturn);
			Assert.AreEqual(0, errors.Count, errorCount);
		}
	}
}
#endif
/*****************************************************************************
 * TestRulesRepository (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class defines a series of Unit Tests that validate the interaction between
 * the RulesRepository class and the NxBRE rules engine.
*****************************************************************************/

#if UNITTEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;
using NxBRE.FlowEngine;

namespace Aucent.FilingServices.RulesRepository.UnitTest
{
	[TestFixture]
	public class TestRulesRepository
	{
		string rulesFolder = @"S:\TestFiles\Test Rules";
#region init

		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp]
		public void RunFirst()
		{}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
		[TestFixtureTearDown]
		public void RunLast()
		{}

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown]
		public void RunAfterEachTest()
		{}

		#endregion

		[Test]
		public void TestTryLoadNewRulesList()
		{
			DirectoryInfo di = new DirectoryInfo(rulesFolder);
			RulesRepository rr = new RulesRepository("testRules", di);

			rr.TryLoadNewRulesList();
			Assert.AreEqual(2, rr.MyRules.Count, "Invalid number of Rules loaded by TryLoadRulesList");
			foreach (Rule currentRule in rr.MyRules.Values)
			{
				switch (currentRule.FriendlyName)
				{
					case "SetValue":
					case "IncrementValue":
						break;
					default:
						Assert.Fail("Unexpected rule file loaded from the Repository.");
						break;
				}
			}
		}

		[Test]
		public void TestTryLoadExistingRulesList()
		{
			DirectoryInfo di = new DirectoryInfo(rulesFolder);
			RulesRepository rr = new RulesRepository("testRules", di);

			string ruleListFile = string.Format("{0}{1}{2}.rul", rulesFolder, Path.DirectorySeparatorChar, rr.Name);
			if (!File.Exists(ruleListFile))
			{
				rr.TryLoadNewRulesList();
				rr.TrySaveRulesList();
				if (!File.Exists(ruleListFile))
				{
					Assert.Fail("Can not run test TestTryLoadExistingRulesList.  No existing rule file was found, and a new one could not be created");
				}
			}

			rr.TryLoadExistingRulesList();
			Assert.AreEqual(2, rr.MyRules.Count, "Invalid number of Rules loaded by TryLoadRulesList");
			foreach (Rule currentRule in rr.MyRules.Values)
			{
				switch (currentRule.FriendlyName)
				{
					case "SetValue":
					case "IncrementValue":
						break;
					default:
						Assert.Fail("Unexpected rule file loaded from the Repository.");
						break;
				}
			}
		}

		[Test]
		public void TestTrySaveRulesList()
		{
			DirectoryInfo di = new DirectoryInfo(rulesFolder);
			RulesRepository rr = new RulesRepository("testRules", di);

			string ruleListFile = string.Format("{0}{1}{2}.rul", rulesFolder, Path.DirectorySeparatorChar, rr.Name);
			if (File.Exists(ruleListFile))
			{
				File.SetAttributes(ruleListFile, FileAttributes.Normal);
				File.Delete(ruleListFile);
			}

			//Load a new rule list so that we have something to save.
			rr.TryLoadNewRulesList();
			rr.TrySaveRulesList();
			Assert.IsTrue(File.Exists(ruleListFile), "TrySaveRulesList failed to create the rule file");
		}

		[Test]
		public void TestProcessRule()
		{
			DirectoryInfo di = new DirectoryInfo(rulesFolder);
			RulesRepository rr = new RulesRepository("testRules", di);

			//Load a new rule list so that we have rules to execute.
			rr.TryLoadNewRulesList();

			TestClass tc = new TestClass();
			Dictionary<string, object> contextObjects = new Dictionary<string, object>();
			contextObjects.Add("TestObject", tc);

			IBRERuleResult ruleResult;
			rr.ProcessRule("SetValue", contextObjects, out ruleResult);

			Assert.AreEqual(5, tc.MyValue, "The rule processed, but did not correctly set the value on the test object");
		}
	}

	public class TestClass
	{
		private int myValue = -1;
		public int MyValue
		{
			get { return myValue; }
			set { myValue = value; }
		}

		public TestClass()
		{ }

		public void IncrementValue(int step)
		{
			this.myValue += step;
		}
	}
}
#endif

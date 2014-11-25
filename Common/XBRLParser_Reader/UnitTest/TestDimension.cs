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
namespace Aucent.MAX.AXE.Common.XBRLParser_Reader.Test
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Text;

	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using NUnit.Framework;

	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.XBRLParser.Interfaces;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture]
	public class TestDimension : Taxonomy
	{
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{ }

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast()
		{ }

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{ }

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest()
		{ }

		#endregion


		/// <exclude/>
		[Test]
		public void TestLoadICIDimensionInfos()
		{

			TestDimension s = new TestDimension();
			int errors = 0;

			DateTime start = DateTime.Now;
			if (s.Load(TestTaxonomy_Definition.ICI_FILE, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse(out errors);


			Assert.IsNotNull(s.NetDefinisionInfo, "Dimension info did not load");

			List<DimensionNode> dimensionNodes;

			if (s.HasDimensionInfo(false, true))
			{
				Assert.IsTrue(
				s.TryGetCommonDimensionNodesForDisplay(s.CurrentLanguage, s.currentLabelRole,
					false, out dimensionNodes));

				Console.WriteLine("*********SCENARIO dimension information*************");

				foreach (DimensionNode n in dimensionNodes)
				{
					StringBuilder sb = DisplayNode(n, 0);
					Console.WriteLine(sb.ToString());
				}

			}

			if (s.HasDimensionInfo(true, true))
			{
				Console.WriteLine("*********SEGMENT dimension information*************");

				Assert.IsTrue(
				s.TryGetCommonDimensionNodesForDisplay(s.CurrentLanguage, s.currentLabelRole,
					true, out dimensionNodes));


				foreach (DimensionNode n in dimensionNodes)
				{
					StringBuilder sb = DisplayNode(n, 0);
					Console.WriteLine(sb.ToString());
				}

			}

		}

		/// <exclude/>
		[Test, Ignore]
		public void TestLoadNewUSGaapDimensionInfos()
		{

			TestDimension s = new TestDimension();
			int errors = 0;

			string fileName = @"S:\TestSchemas\UGT-Prototype3b-2007-07-11\us-gaap-master.xsd";

			DateTime start = DateTime.Now;
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			// parse presentation first

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			s.Parse(out errors);


			Assert.IsNotNull(s.NetDefinisionInfo, "Dimension info did not load");

			List<DimensionNode> dimensionNodes;

			Assert.IsTrue(
			s.TryGetCommonDimensionNodesForDisplay(s.CurrentLanguage, s.currentLabelRole,
				false, out dimensionNodes));

			Console.WriteLine("*********SCENARIO dimension information*************");

			foreach (DimensionNode n in dimensionNodes)
			{
				StringBuilder sb = DisplayNode(n, 0);
				Console.WriteLine(sb.ToString());
			}

			Console.WriteLine("*********SEGMENT dimension information*************");

			Assert.IsTrue(
			s.TryGetCommonDimensionNodesForDisplay(s.CurrentLanguage, s.currentLabelRole,
				true, out dimensionNodes));


			foreach (DimensionNode n in dimensionNodes)
			{
				StringBuilder sb = DisplayNode(n, 0);
				Console.WriteLine(sb.ToString());
			}

		}

		/// <exclude/>
		protected StringBuilder DisplayNode(DimensionNode n, int level)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < level; ++i)
			{
				sb.Append(" ");
			}

			sb.Append(n.Label).Append(Environment.NewLine);

			if (n.Children != null)
			{
				foreach (DimensionNode c in n.Children)
				{
					sb.Append(DisplayNode(c, level + 1));
				}
			}

			return sb;
		}

	}
}
#endif
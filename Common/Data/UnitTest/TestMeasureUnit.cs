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
	using System.IO;
	using System.Text;

	using System.Xml;
	using System.Xml.Serialization;

	using System.Collections;

    using NUnit.Framework;
	using Aucent.MAX.AXE.Common.Data;

	/// <exclude/>
	[TestFixture] 
    public class TestMeasureUnit : MeasureUnit
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
			MeasureUnit mu = new MeasureUnit();
			this.Name = mu.Name;
			this.MeasureUnitType = mu.MeasureUnitType;
			this.CalculationType = mu.CalculationType;
			this.Measure1 = mu.Measure1;
			this.Measure2 = mu.Measure2;
		}

        ///<exclude/>
        [TearDown] public void RunAfterEachTest() 
        {}

#endregion

		/// <exclude/>
		[Test]
		public void TestCurrencyMeasure()
		{
			this.NewCurrencyMeasure("test", "USD");
			Console.WriteLine (this.ToString());
		}

		/// <exclude/>
		[Test]
		public void TestSharesMeasure()
		{
			this.NewSharesMeasure("test");
			Console.WriteLine (this.ToString());
		}

		/// <exclude/>
		[Test]
		public void TestPureMeasure()
		{
			this.NewPureMeasure("testPure");
			Console.WriteLine (this.ToString());
		}

		/// <exclude/>
		[Test]
		public void TestOtherSimpleMeasure()
		{
			Measure measure = new Measure ("footage", "Some Namespace", "Some Schema", Measure.MeasureTypeCode.Other);
			this.NewSimpleMeasure("SimpleMeasure", measure);
			Console.WriteLine (this.ToString());
		}

		/// <exclude/>
		[Test]
		public void TestComplexMeasure()
		{
			Measure measure1 = new Measure ("measure 1", "Some Namespace", "Some Schema", Measure.MeasureTypeCode.Shares);
			Measure measure2 = new Measure ("measure 2", "Some Namespace", "Some Schema", Measure.MeasureTypeCode.Currency);
			this.NewComplexMeasure("ComplexMeasure", CalculationTypeCode.Divide, measure1, measure2);
			Console.WriteLine (this.ToString());
		}

		/// <exclude/>
		[Test]
		public void TestSerializeMeasure()
		{
			ArrayList al = new ArrayList();

			MeasureUnit mu = new MeasureUnit();
			mu.NewCurrencyMeasure( "TestCurrencyMeasure", "USD" );
			al.Add( mu );

			MeasureUnit mu2 = new MeasureUnit();
			mu2.NewComplexMeasure( "Test2", CalculationTypeCode.Divide, new Measure( "M1" ), new Measure( "m2" ) );
			al.Add( mu2 );

			MeasureUnit mu3 = new MeasureUnit();
			mu3.NewSharesMeasure( "Shares" );
			al.Add( mu3 );

			MeasureUnit mu4 = new MeasureUnit();
			mu4.NewSimpleMeasure( "simple measure", new Measure( "simple" ) );
			al.Add( mu4 );

			MeasureUnit mu5 = new MeasureUnit();
			mu5.NewPureMeasure("Pure");
			al.Add( mu5 );

			MemoryStream ms = new MemoryStream();

			XmlTextWriter tw = new XmlTextWriter( ms, Encoding.UTF8 );

			XmlSerializer xml = new XmlSerializer( typeof( ArrayList ), new Type[] { typeof( MeasureUnit ) } );
			xml.Serialize(  tw, al );
		}

		/// <exclude/>
		[Test]
		public void TestMeasureValueEquals()
		{
			const string error = "Invalid return from ValueEquals method";

			Measure a = new Measure();
			Measure b = new Measure();

			Assert.IsFalse(a.ValueEquals(null), error);
			Assert.IsFalse(a.ValueEquals(error), error);
			Assert.IsTrue(a.ValueEquals(b), error);

			string testStr = "test";

			b.ValueName = testStr;
			Assert.IsFalse(a.ValueEquals(b), error);
			a.ValueName = b.ValueName;
			Assert.IsTrue(a.ValueEquals(b), error);

			b.Namespace = testStr;
			Assert.IsFalse(a.ValueEquals(b), error);
			a.Namespace = b.Namespace;
			Assert.IsTrue(a.ValueEquals(b), error);

			b.Schema = testStr;
			Assert.IsFalse(a.ValueEquals(b), error);
			a.Schema = b.Schema;
			Assert.IsTrue(a.ValueEquals(b), error);

			b.MeasureType = Measure.MeasureTypeCode.Currency;
			Assert.IsFalse(a.ValueEquals(b), error);
			a.MeasureType = b.MeasureType;
			Assert.IsTrue(a.ValueEquals(b), error);
		}

		/// <exclude/>
		[Test]
		public void TestMeasureUnitValueEquals()
		{
			const string error = "Invalid return from ValueEquals method";

			TestMeasureUnit mu = new TestMeasureUnit();

			Assert.IsTrue(this.ValueEquals(mu), error);

			mu.MeasureUnitType = MeasureUnit.MeasureUnitTypeCode.Complex;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.MeasureUnitType = mu.MeasureUnitType;
			Assert.IsTrue(this.ValueEquals(mu), error);

			mu.CalculationType = MeasureUnit.CalculationTypeCode.Multiply;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.CalculationType = mu.CalculationType;
			Assert.IsTrue(this.ValueEquals(mu), error);

			Measure m0 = new Measure();
			m0.Schema = "m0";
			Measure m1 = new Measure();
			m1.Schema = "m1";

			mu.Measure1 = m0;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.Measure1 = m0;
			Assert.IsTrue(this.ValueEquals(mu), error);

			mu.Measure2 = m1;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.Measure2 = m1;
			Assert.IsTrue(this.ValueEquals(mu), error);

			mu.Measure2 = null;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.Measure2 = null;
			Assert.IsTrue(this.ValueEquals(mu), error);

			mu.Measure1 = null;
			Assert.IsFalse(this.ValueEquals(mu), error);
			this.Measure1 = null;
			Assert.IsTrue(this.ValueEquals(mu), error);
		}		
    }
}
#endif
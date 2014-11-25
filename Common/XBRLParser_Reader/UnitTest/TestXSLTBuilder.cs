//=============================================================================
// XSLTBuilder (class)
// Aucent Corporation
//=============================================================================

#if false
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Collections;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;

	[TestFixture] 
	public class TestXSLTBuilder : XSLTBuilder_Obsolete
	{

		const string XSL_NS			= "http://www.w3.org/1999/XSL/Transform";
		const string PT_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-08-15\usfr-pt-2004-08-15.xsd";
		const string US_GAAP_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\2004-08-15\us-gaap-ci-2004-08-15.xsd";
		const string PT_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-usfr-pt.xml";
		const string US_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-gaap-ci.xml";
		const string NODE_OUT_FILE = @"c:\Aucent\MAX\axe\XBRLParser\TestSchemas\XBRL 2.1 Updated\aucent-gaap-ci-nodes.xml";
		const string US_GAAP_FILE_2_0 = @"C:\Aucent\MAX\AXE\XBRLParser\TestSchemas\US GAAP CI 2003-07-07\us-gaap-ci-2003-07-07.xsd";


		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp] public void RunFirst()
		{}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
		[TestFixtureTearDown] public void RunLast() 
		{}

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp] public void RunBeforeEachTest()
		{}

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown] public void RunAfterEachTest() 
		{}

		#endregion

		#region Helpers
		protected void RecurseVerifyTuples( Node n )
		{
			if ( n.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", n.Label );
			}

			if ( n.HasChildren )
			{
				for ( int i=0; i < n.Children.Count; ++i )
				{
					RecurseVerifyTuples( n.Children[i] as Node );
				}
			}
		}


		protected void RecurseElementsForTuples( Element e )
		{
			if ( e.IsTuple )
			{
				Console.WriteLine( "{0} is a tuple", e.Name );
			}

			if ( e.HasChildren )
			{
				foreach ( Element c in e.TupleChildren.GetValueList() )
				{
					RecurseElementsForTuples( c );
				}
			}
		}

		protected void SendErrorsToConsole( ArrayList errorList )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level != TraceLevel.Error )
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		protected void SendWarningsToConsole( ArrayList errorList )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Warning )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		protected void SendInfoToConsole( ArrayList errorList )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Info )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		protected void SendWarningsToConsole( ArrayList errorList, string filter )
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Message.IndexOf( filter ) < 0 )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}


		private void OutputTree(Node node, int Spaces)
		{
			int i = 0;
			string txtLabel = string.Empty;
			Console.WriteLine((new string(' ',Spaces).ToString()) + node.Label);
			if (node.HasChildren)
			{
				i+= 3;
				foreach(Node n in node.Children)
					OutputTree (n, Spaces + i);
			}
		}
		private ContextProperty CreateInstantContext( string id, string schema, string val, DateTime start )
		{
			ContextProperty cp = new ContextProperty();

			cp.ContextID = id;
			cp.EntitySchema = schema;
			cp.EntityValue = val;
			cp.PeriodType = Element.PeriodType.instant;
			cp.PeriodStartDate = start;

			return cp;
		}
		private UnitProperty CreateStandardUnit( string id, int scale, string ns, string schema, string val )
		{
			UnitProperty up = new UnitProperty( id, UnitProperty.UnitTypeCode.Standard );

			up.Scale = scale;
			up.StandardMeasure.MeasureNamespace = ns;
			up.StandardMeasure.MeasureSchema = schema;
			up.StandardMeasure.MeasureValue = val;

			return up;
		}
		private Node CreateNode( string id )
		{
			Node n = new Node();

			n.Name = id;

			return n;
		}
		private Node CreateNode( string id, string label )
		{
			Node n = new Node();

			n.Name = id;
			n.Label = label;

			return n;
		}
		private Precision CreatePrecision( Precision.PrecisionTypeCode typeCode )
		{
			return new Precision( typeCode );
		}

		private Precision CreatePrecision( Precision.PrecisionTypeCode typeCode, int decimals )
		{
			return new Precision( typeCode, decimals );
		}

		
		#endregion

		[Test]
		public void TestWebPreview_VisualTestRequired()
		{
			try
			{
				// Per Emily, QA should test the WebPreview functionality visually.
				// This is therefore just a placeholder for future tests (if any develop).

				Assert.IsTrue(true);


//				Taxonomy tx = new Taxonomy();
//				tx.Load( US_GAAP_FILE );
//				int errors = 0;
//				Assert.IsTrue(tx.Parse( out errors ));
//				tx.CurrentLabelRole = "preferredLabel";
//				tx.CurrentLanguage  = "en";
//				
//				ArrayList txnodes = tx.GetNodesByPresentation();
//				System.Collections.IEnumerator te = txnodes.GetEnumerator();
//				while (te.MoveNext())
//				{
//					Node no = (Node) te.Current;
//					OutputTree(no, 0);
//				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error:" + ex.Message);
			}
		}

		[Test] public void TestApplyScaleToMarkup()
		{
			MarkupProperty mp = new MarkupProperty();
			mp.markupData = "1234";
			// test 1 - no scale
			mp.unitRef = null;
			Assert.AreEqual("1234", ApplyScaleToMarkup(mp), "Incorrect value with no scale");

			mp.unitRef = new UnitProperty();
			mp.unitRef.Scale = 0;
			mp.markupData = "1";
			Assert.AreEqual("1", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 1;
			mp.markupData = "1";
			Assert.AreEqual("10", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 2;
			mp.markupData = "1";
			Assert.AreEqual("100", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 2;
			mp.markupData = "1";
			Assert.AreEqual("100", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 10;
			mp.markupData = "1";
			//                1234567890
			Assert.AreEqual("10000000000", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 6;
			mp.markupData = "12345.26";
			Assert.AreEqual("12345260000", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 2;
			mp.markupData = "1.11";
			Assert.AreEqual("111", ApplyScaleToMarkup(mp) );

			mp.unitRef.Scale = 1;
			mp.markupData = "123.11";
			Assert.AreEqual("1231", ApplyScaleToMarkup(mp) );


		}
	}
}
#endif
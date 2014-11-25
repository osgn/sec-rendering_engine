//=============================================================================
// Schema (class)
// Aucent Corporation
//=============================================================================

#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test_Taxonomy
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
	using Aucent.MAX.AXE.Common.Exceptions;

	[TestFixture] 
	public class TestTaxonomy_ADP_20060930 : Taxonomy
	{
		#region Overrides
//		public void TestParse( out int errors )
//		{
//			errors = 0;
//			ParseInternal( out errors );
//		}

		#endregion

		#region init
        
		/// <summary> Sets up test values for this unit test class - called once on startup</summary>
		[TestFixtureSetUp] public void RunFirst()
		{
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <summary>Tears down test values for this unit test class - called once after all tests have run</summary>
		[TestFixtureTearDown] public void RunLast() 
		{
			Trace.Listeners.Clear();
		}

		/// <summary> Sets up test values before each test is called </summary>
		[SetUp] public void RunBeforeEachTest()
		{}

		/// <summary>Tears down test values after each test is run </summary>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string ADP_FILE = TestCommon.FolderRoot + @"ADP-20060930" +System.IO.Path.DirectorySeparatorChar +"ADP-20060930.xsd";

		#region helpers
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

		protected int SendWarningsToConsole( ArrayList errorList )
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Warning )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
					++numwarnings;
				}
			}

			return numwarnings;
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

		#endregion

		/// <summary>
		/// Summary description for TestTaxonomy_MorganStanley.
		/// </summary>
		public TestTaxonomy_ADP_20060930()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[Test] public void ADP_LoadAndParse()
		{
			TestTaxonomy_ADP_20060930 s = new TestTaxonomy_ADP_20060930();

			int errors = 0;

			if ( s.Load( ADP_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			SendWarningsToConsole( s.errorList );
			SendInfoToConsole( s.ErrorList );

			Assert.AreEqual( 0, errors, "errors returned from parse" );

			s.currentLanguage = "en";
			s.currentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();

			Node statements = nodes[5] as Node;
			Assert.IsNotNull( statements, "statements is null" );
			Assert.AreEqual( "Statement of Cash Flows", statements.Label, "node 4 wrong name" );
			Assert.IsTrue( statements.ElementIsNull, "statements.MyElement is not null");

			Node direct = statements.Children[0] as Node;
			Assert.IsNotNull( direct, "direct is null" );
			Assert.AreEqual( "Statement of Cash Flows - Direct Method", direct.Label, "direct wrong name" );
			Assert.IsNotNull( direct.Children, "direct.Children is null" );

			Node netCashFlows = direct.Children[2] as Node;
			Assert.IsNotNull( netCashFlows, "statement is null" );
			Assert.AreEqual( "Net Cash Flows Provided By/(Used In) Financing Activities", netCashFlows.Label, "netCashFlows wrong label is null" );
			Assert.IsNotNull( netCashFlows.Children, "netCashFlows.Children is null" );

			Node incdec = netCashFlows.Children[0] as Node;
			Assert.IsNotNull( incdec, "incdec is null" );
			Assert.AreEqual( @"Increase/(Decrease) in Debt", incdec.Label, "incdec wrong label is null" );
			Assert.IsNotNull( incdec.Children, "incdec.Children is null" );
			Assert.AreEqual( 5, incdec.Children.Count, "wrong number of children incdec" );

			Node prohibDebt1 = incdec.Children[0] as Node;
			Assert.IsNotNull( prohibDebt1, "prohibDebt1 is null" );
			Assert.AreEqual( "Payments of debt", prohibDebt1.Label, "prohibDebt1 label is wrong" );
			Assert.IsTrue( prohibDebt1.IsProhibited, "prohibDept1 is not prohibited" );
			Assert.AreEqual( 1.0F, prohibDebt1.Order, "prohibDept1 order wrong" );

			Node shortTerm = incdec.Children[1] as Node;
			Assert.AreEqual( "Increase/(Decrease) in Short-Term Borrowings", shortTerm.Label, "shortTerm label is wrong" );
			Assert.IsNotNull( shortTerm, "shortTerm is null" );
			Assert.AreEqual( "1.005", shortTerm.Order.ToString( "#.###" ), "shortTerm order wrong" );

			Node longtermdebt = incdec.Children[2] as Node;
			Assert.IsNotNull( longtermdebt, "longtermdebt is null" );
			
			Node debt = incdec.Children[3] as Node;
			Assert.IsNotNull( debt, "dept is null" );
			Assert.IsFalse( debt.IsProhibited, "debt is prohibited" );
			Assert.AreEqual( "Payments of debt", debt.Label, "debt label is wrong" );
			Assert.AreEqual( "2.51", debt.Order.ToString( "#.##"), "debt order wrong" );

			Node prohibDebt2 = incdec.Children[4] as Node;
			Assert.IsNotNull( prohibDebt2, "prohibDebt2 is null" );
			Assert.AreEqual( "Payments of debt", prohibDebt1.Label, "prohibDebt1 label is wrong" );
			Assert.IsTrue( prohibDebt2.IsProhibited, "prohibDept2 is not prohibited" );
			Assert.AreEqual( 3.0, prohibDebt2.Order, "prohibDept2 order wrong" );

			Node proceeds = longtermdebt.Children[1] as Node;
			Assert.IsNotNull( proceeds, "proceeds is null" );
			Assert.AreEqual( @"Proceeds from Issuance of Long-Term Debt and Capital Securities", proceeds.Label, "proceeds wrong label is null" );
			Assert.IsNotNull( proceeds.Children, "proceeds.Children is null" );
			Assert.AreEqual( 3, proceeds.Children.Count, "proceeds.Children has wrong count" );

			Node note = proceeds.Children[1] as Node;
			Assert.IsFalse( note.IsProhibited, "note is prohibited" );
		}

	}
}
#endif
using System;
using System.Windows.Forms;
using Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.CLI;


namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester

{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
			bool isNoGui = false;
			foreach( string arg in args )
			{
				if( string.Equals( "--no-gui", arg, StringComparison.OrdinalIgnoreCase ) )
				{
					isNoGui = true;
					break;
				}
			}

			Primary primary = new Primary();

			if( !isNoGui )
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault( false );
				Application.Run( primary );
				return;
			}
			else
			{
				CliAbstract.Run( primary, args );
			}
        }
    }
}

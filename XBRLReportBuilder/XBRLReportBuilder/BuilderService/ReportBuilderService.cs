/*****************************************************************************
 * ReportBuilderService (class)
 * Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
 * This class is the Windows Service that manages all of the classes that wait 
 * for filings, process the filings and outputs the reports.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using Aucent.FilingServices.FilingProcessorBase;
using Aucent.XBRLReportBuilder.Builder;

namespace Aucent.XBRLReportBuilder.BuilderService
{
    public partial class ReportBuilderService : ServiceBase
    {
		
        public ReportBuilderService()
        {
            InitializeComponent();
			this.AutoLog = true;
		}

		#region events

		protected override void OnStart(string[] args)
		{
			int maxAttempts = 0;
			string strMaxAttempts = ConfigurationManager.AppSettings["MaxExtractionAttempts"];
			if (!Int32.TryParse(strMaxAttempts, out maxAttempts))
			{
				maxAttempts = 1;
			}

			if (FilingProcessorManager.TheMgr.TryLoadConfigurationSettings(new ReportBuilderProcessor(maxAttempts)))
			{
				
				FilingProcessorManager.TheMgr.Startup(true);
			}
			else
			{
				this.Stop();
			}
		}

        protected override void OnStop()
        {
			FilingProcessorManager.TheMgr.ShutDown();
		}

		#endregion

		#region Main

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ReportBuilderService ds = new ReportBuilderService();

#if DEBUG
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			string[] args = new string[0];
			ds.OnStart(args);


			System.Console.WriteLine("Please press enter to exit the console application.");
			System.Console.Read();

			ds.OnStop();

#else

            ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            ServicesToRun = new ServiceBase[] { ds };

            ServiceBase.Run(ServicesToRun);
#endif
		}

		#endregion
	}
}

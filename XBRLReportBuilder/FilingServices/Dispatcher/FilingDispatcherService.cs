/**************************************************************************
 * FilingDispatcherService (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * A generic service that can be used to manage a set of FilingProcessor services.
 * The service is built with the intention that it does not mater what type of
 * FilingProcessor service is registered as long as the service implements the
 * IFilingProcessor interface.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace Aucent.FilingServices.Dispatcher
{
	public partial class FilingDispatcherService : ServiceBase
	{
		public FilingDispatcherService()
		{
			InitializeComponent();
		}

		#region events

		protected override void OnStart(string[] args)
		{
			if (FilingDispatcherManager.TheMgr.TryLoadConfigurationSettings())
			{
				FilingDispatcherManager.TheMgr.Startup();
			}
			else
			{
				this.Stop();
			}
		}

		protected override void OnStop()
		{
			FilingDispatcherManager.TheMgr.ShutDown();
		}

		#endregion

		#region Main

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			FilingDispatcherService ds = new FilingDispatcherService();

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

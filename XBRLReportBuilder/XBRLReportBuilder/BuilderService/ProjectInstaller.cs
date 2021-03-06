/*****************************************************************************
 * ProjectInstaller (class)
 * Copyright � 2006-2011 Rivet Software, Inc. All rights reserved.
 * This class is the Installer class that is required in order to install the service
 * using "installutil.exe".  Installutil.exe is the utility application provided by
 * the .NET framework for installing .NET executables that will run as Windows Services
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Text;
using System.Xml;

namespace Aucent.XBRLReportBuilder.Builder
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}

		private static string ReadServiceName()
		{
			StringBuilder assemblyName = new StringBuilder(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
			assemblyName.Append(".exe.config");

			XmlDocument config = new XmlDocument();
			config.Load(assemblyName.ToString());

			const string xPath = "configuration/appSettings/add[@key='ServerKey']";
			XmlNode node = config.SelectSingleNode(xPath);
			if (node == null)
			{
				throw new ApplicationException(
					string.Format(
					"Failure to read node {0} from configuration file {1}",
					xPath,
					assemblyName.ToString()));
			}

			XmlAttribute serverKeyAttribute = node.Attributes["value"];
			if (serverKeyAttribute == null)
			{
				throw new ApplicationException(
					string.Format(
					"Failure to read \"ServerKey\" attribute from configuration file {0}",
					assemblyName.ToString()));
			}

			if ((serverKeyAttribute.Value == null) || (serverKeyAttribute.Value.Length == 0))
			{
				throw new ApplicationException(
					string.Format(
					"Invalid \"ServerKey\" attribute value in configuration file {0}",
					assemblyName.ToString()));
			}

			return serverKeyAttribute.Value;
		}
	}
}
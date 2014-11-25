/**************************************************************************
 * ServiceUtilities (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines static helper methods that are used by Remotable services during startup
 * and shutdown.
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting.Channels.Tcp;

namespace Aucent.FilingServices.Data
{
	public class ServiceUtilities
	{

		/// <summary>
		/// Registers well-known remoting service
		/// </summary>
		/// <param name="type">Type of service</param>
		/// <param name="endpoint">Remoting endpoint</param>
		public static void RegisterService(Type type, string endpoint)
		{
			if( (type == null) || (endpoint == null) || (endpoint == string.Empty) )
			{
				throw new ArgumentException( "Invalid remoting config" );
			}

			RemotingConfiguration.RegisterWellKnownServiceType(
				type,
				endpoint,
				WellKnownObjectMode.SingleCall);

			RemotingConfiguration.CustomErrorsEnabled( false );	// return complete error information
		}

		/// <summary>
		/// Configures and registers remoting channel
		/// </summary>
		/// <param name="protocol">"tcp" or "http"</param>
		/// <param name="port">Port number</param>
		public static void RegisterChannel(int port)
		{
			IChannel channel = null;

			BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
			provider.TypeFilterLevel = TypeFilterLevel.Full;
			ListDictionary props = new ListDictionary();
			props["port"] = port;
			channel = new TcpChannel(props, null, provider);
			ChannelServices.RegisterChannel(channel, false);
		}

		public static void UnRegisterChannel(int port)
		{
			IChannel channel = null;

			BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
			provider.TypeFilterLevel = TypeFilterLevel.Full;
			ListDictionary props = new ListDictionary();
			props["port"] = port;
			channel = new TcpChannel(props, null, provider);

			ChannelServices.UnregisterChannel(channel);
		}
	}
}

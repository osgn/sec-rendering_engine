using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.CLI
{
	public abstract class CliAbstract
	{
		public void Run( string[] args )
		{
			Run( this, args );
		}

		public static void Run<T>( T instance, string[] args )
		{
			Dictionary<string, string> arguments = LoadArguments( args );

			CommandLineInvoker defaultCommand;
			Dictionary<string, CommandLineInvoker> commands = LoadCommands<T>( instance, arguments, out defaultCommand );

			foreach( string key in arguments.Keys )
			{
				CommandLineInvoker command;
				if( commands.TryGetValue( key, out command ) && command.CanInvoke() )
				{
					command.Invoke();
					return;
				}
			}

			if( defaultCommand.CanInvoke() )
				defaultCommand.Invoke();
		}

		private static Dictionary<string, string> LoadArguments( string[] args )
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

			for( int i = 0; i < args.Length; i++ )
			{
				string arg = args[ i ];
				if( !arg.StartsWith( "-" ) )
				{
					Trace.TraceWarning( "Ignoring argument.  Reason: incorrect format: {1}{0}Arguments must start with a dash.", Environment.NewLine, arg );
					continue;
				}

				string command = arg.TrimStart( '-' );
				string[] parts = command.Split( new char[] { '=' }, 2 );
				command = parts[ 0 ];

				if( parts.Length == 2 )
					arguments[ command ] = parts[ 1 ];
				else
					arguments[ command ] = null;
			}

			return arguments;
		}

		private static Dictionary<string, CommandLineInvoker> LoadCommands<T>( T instance, Dictionary<string, string> arguments, out CommandLineInvoker defaultCommand )
		{
			defaultCommand = null;

			Type cliMethod = typeof( CommandLineMethodAttribute );
			Type parameter = typeof( ParameterAttribute );

			Dictionary<string, CommandLineInvoker> commands = new Dictionary<string, CommandLineInvoker>( StringComparer.OrdinalIgnoreCase );
			MethodInfo[] mis = instance.GetType().GetMethods( BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static );
			foreach( MethodInfo mi in mis )
			{
				object[] miAtts = mi.GetCustomAttributes( cliMethod, true );
				if( miAtts == null || miAtts.Length != 1 )
					continue;

				CommandLineMethodAttribute cliAtt = miAtts[ 0 ] as CommandLineMethodAttribute;
				if( cliAtt == null )
					continue;

				CommandLineInvoker cliInvoker;
				if( mi.IsStatic )
					cliInvoker = new CommandLineInvoker( mi );
				else
					cliInvoker = new CommandLineInvoker( mi, instance );

				ParameterInfo[] pis = mi.GetParameters();
				foreach( ParameterInfo pi in pis )
				{
					if( pi.IsRetval || pi.IsOut )
						throw new ArgumentException( "The CommandLineMethod attribute may not be applied to methods with 'ref' our 'out' parameters." );

					object[] piAtts = pi.GetCustomAttributes( parameter, true );
					if( piAtts == null || piAtts.Length != 1 )
					{
						cliInvoker.AddParameter( pi );
						continue;
					}

					ParameterAttribute param = piAtts[ 0 ] as ParameterAttribute;
					if( param == null )
					{
						cliInvoker.AddParameter( pi );
						continue;
					}

					string value;
					if( arguments.TryGetValue( param.Name, out value ) || arguments.TryGetValue( param.Shortcut.ToString(), out value ) )
						param.Value = value;

					cliInvoker.AddParameter( pi, param );
				}

				if( string.IsNullOrEmpty( cliInvoker.Name ) )
				{
					if( defaultCommand != null )
						throw new Exception( "There can only be one default method.  A default method uses the CommandLineMethod attribute with no Name." );
					else
						defaultCommand = cliInvoker;
				}
				else
				{
					commands.Add( cliAtt.Name, cliInvoker );

					if( !char.Equals( char.MinValue, cliAtt.Shortcut ) )
						commands.Add( cliAtt.Shortcut.ToString(), cliInvoker );
				}
			}

			return commands;
		}

	}
}

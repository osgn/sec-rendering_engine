using System;
using System.Diagnostics;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.CLI
{
	[AttributeUsage( AttributeTargets.Parameter )]
	public class ParameterAttribute : Attribute, ICommandLineArgument
	{
		/// <summary>
		/// Indicates the verbose command for the parameter.
		/// It can be called using the format {--command} where command is this Name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Indicates the shortcut command for the parameter.
		/// It can be called using the format {-c} where c is the this Shortcut.
		/// </summary>
		public char Shortcut { get; set; }

		/// <summary>
		/// When set by the developer, provides the default value.
		/// This value may be overwritten by the command-line arguments.
		/// </summary>
		public string Value { get; set; }

		public ParameterAttribute()
		{
		}

		public ParameterAttribute( string name )
		{
			string trimName = ( name ?? string.Empty ).Trim();
			if( string.IsNullOrEmpty( trimName ) )
				throw new ArgumentNullException( "name" );

			if( trimName.Length != name.Length )
				throw new ArgumentException( "The argument name may not have leading or trailing space.", "name" );

			if( trimName.Length == 1 )
				throw new ArgumentException( "The argument name must be more than one (1) character." );

			this.Name = name;
		}

		public ParameterAttribute( string name, char shortcut )
			: this( name )
		{
			if( char.Equals( char.MinValue, shortcut ) )
				throw new ArgumentException( "The argument 'shortcut' cannot null or empty.", "shortcut" );

			this.Shortcut = shortcut;
		}
	}
}

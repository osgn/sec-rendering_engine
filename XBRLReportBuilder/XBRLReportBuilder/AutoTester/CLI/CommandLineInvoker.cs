using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.CLI
{
	public class CommandLineInvoker
	{
		public int Length
		{
			get { return this._params.Count; }
		}

		public string Name { get; set; }

		private MethodInfo _method;
		private object _obj;
		private SortedList<int, ParameterAttribute> _params = new SortedList<int, ParameterAttribute>();

		public CommandLineInvoker( MethodInfo method )
			: this( method, null )
		{
		}

		public CommandLineInvoker( MethodInfo method, object obj )
		{
			this._method = method;
			this._obj = obj;
		}

		public void AddParameter( ParameterInfo pi )
		{
			this._params.Add( pi.Position, new ParameterAttribute() );
		}

		public void AddParameter( ParameterInfo pi, ParameterAttribute param )
		{
			this._params.Add( pi.Position, param );
		}

		public bool CanInvoke()
		{
			if( this.Length == this._method.GetParameters().Length )
				return true;

			return false;
		}

		public void Invoke()
		{
			string[] arguments = new string[ this.Length ];
			for( int i = 0; i < this._params.Count; i++ )
			{
				arguments[i] = this._params.Values[ i ].Value;
			}

			this._method.Invoke( this._obj, arguments );
		}
	}
}

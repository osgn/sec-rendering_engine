using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test.Data
{
	public abstract partial class Test_Abstract
	{
		private static readonly Regex splitter = new Regex( @"(?<text>\D+)?(?<number>\d+(\.\d+)?)?", RegexOptions.Compiled | RegexOptions.ExplicitCapture );
		private static object[] tokenize( string text )
		{
			List<object> objects = new List<object>();

			MatchCollection matches = splitter.Matches( text );
			foreach( Match m in matches )
			{
				if( m.Groups[ "text" ].Success )
					objects.Add( m.Groups[ "text" ].Value );

				if( m.Groups[ "number" ].Success )
				{
					decimal tmp;
					if( decimal.TryParse( m.Groups[ "number" ].Value, out tmp ) )
						objects.Add( tmp );
				}
			}

			return objects.ToArray();
		}

		private static int natcasecmp( string left, string right )
		{
			bool leftIsEmpty = left == null || left.Trim().Length == 0;
			bool rightIsEmpty = right == null || right.Trim().Length == 0;
			if( leftIsEmpty )
			{
				//push empty strings to the end
				return rightIsEmpty ? 0 : 1;
			}

			if( rightIsEmpty )
				return -1;

			object[] leftSubItems = tokenize( left );
			object[] rightSubItems = tokenize( right );
			int min = Math.Min( leftSubItems.Length, rightSubItems.Length );
			for( int i = 0; i < min; i++ )
			{
				object leftObj = leftSubItems[ i ];
				object rightObj = rightSubItems[ i ];
				int result = Comparer<object>.Default.Compare( leftObj, rightObj );
				if( result != 0 )
					return result;
			}

			if( leftSubItems.Length > min )
				return 1;

			if( rightSubItems.Length > min )
				return -1;

			return 0;
		}
	}
}

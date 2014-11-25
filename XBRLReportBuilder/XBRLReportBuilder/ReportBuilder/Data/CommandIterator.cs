using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.Common.Data;
using System.Xml.Serialization;
using Aucent.MAX.AXE.XBRLParser;

namespace XBRLReportBuilder
{
	[Serializable]
	public class CommandIterator : IComparable<CommandIterator>, IComparable
	{
		public enum IteratorType
		{
			Column,
			Row,
			Unknown
		};

		[Flags]
		public enum SelectionType
		{
			None = 0,
			Axis = 1,
			Element = 2,
			Period = 4,
			Primary = 2,
			Unit = 8,
			Separator = 16,
			All = 31
		}

		public enum StyleType
		{
			Compact,
			Grouped,
			UnitCell,
			NoDisplay,
			Segment
			//Complete //RR only
		} ;

		public static readonly CommandIterator DefaultElement = null;
		public static readonly CommandIterator DefaultPeriod = null;
		public static readonly CommandIterator DefaultPrimary = null;
		public static readonly CommandIterator DefaultUnit = null;
		public static readonly Dictionary<SelectionType,CommandIterator> Defaults = null;

		#region properties

		protected IteratorType type = IteratorType.Unknown;
		public IteratorType Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		protected string selectionString = string.Empty;
		public string SelectionString
		{
			get { return this.selectionString; }
			set { this.selectionString = value; }
		}

		protected SelectionType selection = SelectionType.Axis;
		public SelectionType Selection
		{
			get { return this.selection; }
			set { this.selection = value; }
		}

		protected StyleType style = StyleType.Compact;
		public StyleType Style
		{
			get { return this.style; }
			set { this.style = value; }
		}

		protected string filter = "*";
		public string Filter
		{
			get { return this.filter; }
			set { this.filter = value; }
		}

		public bool HasFilter
		{
			get
			{
				if( string.IsNullOrEmpty( this.Filter ) )
					return false;

				if( this.Filter == "*" )
					return false;

				return true;
			}
		}


		public bool IsCompact
		{
			get { return this.Style == StyleType.Compact; }
		}

		public bool IsAxis
		{
			get { return !( this.IsPeriod || this.IsPrimary || this.IsSeparator || this.IsUnit ); }
		}

		public bool IsElement
		{
			get { return this.IsPrimary; }
		}

		public bool IsPeriod
		{
			get { return int.Equals( this.Selection, SelectionType.Period ); }
		}

		public bool IsPrimary
		{
			get { return int.Equals( this.Selection, SelectionType.Primary ); }
		}

		public bool IsSeparator
		{
			get { return int.Equals( this.Selection, SelectionType.Separator ); }
		}

		public bool IsUnit
		{
			get { return int.Equals( this.Selection, SelectionType.Unit ); }
		}

		public string AxisName
		{
			get
			{
				if( this.IsAxis )
					return this.SelectionString;

				return string.Empty;
			}
		}

		public string MemberName
		{
			get
			{
				if( this.IsAxis )
					return this.Filter;

				return string.Empty;
			}
		}

		public string TempCurrentMemberKey { get; set; }
		public object TempCurrentMemberValue { get; set; }

		public readonly int Index = -1;
		private static int iteratorIndex = 1;
		#endregion

		#region constructors

		public CommandIterator()
		{
			this.Index = iteratorIndex++;
		}

		public CommandIterator( IteratorType type, string selectionString, StyleType style, string filter ) : this()
		{
			this.Type = type;
			this.SelectionString = selectionString;
			this.Style = style;
			this.Filter = filter;

			this.Selection = ParseSelectionType( selectionString );
		}

		static CommandIterator()
		{
			DefaultElement = DefaultPrimary = CreateDefaultElement();
			DefaultPeriod = CreateDefaultPeriod();
			DefaultUnit = CreateDefaultUnit();

			Defaults = new Dictionary<SelectionType, CommandIterator>();
			Defaults[ SelectionType.Element ] = DefaultElement;
			Defaults[ SelectionType.Period ] = DefaultPeriod;
			Defaults[ SelectionType.Unit ] = DefaultUnit;
		}

		#endregion

        /// <summary>
        /// Implementation of <see cref="IComparable"/> method CompareTo.
        /// </summary>
        /// <param name="obj">Instance to compare against.</param>
        /// <returns>An <see cref="int"/> containing results of the comparison.
        /// </returns>
		public int CompareTo( object obj )
		{
			return DefaultComparer( this, obj as CommandIterator );
		}

        /// <summary>
        /// Implementation of <see cref="IComparable"/> method CompareTo.
        /// </summary>
        /// <param name="other"><see cref="CommandIterator"/> to compare
        /// against.</param>
        /// <returns>An <see cref="int"/> containing results of the comparison.
        /// </returns>
		public int CompareTo( CommandIterator other )
		{
			return DefaultComparer( this, other );
		}

        /// <summary>
        /// Custom implementation of ToString.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( this.Type.ToString().ToLower() + " " );
			sb.Append( this.SelectionString + " " );
			sb.Append( this.Style.ToString().ToLower() + " " );
			sb.Append( this.Filter.ToString() );

			return sb.ToString();
		}

        /// <summary>
        /// Create a new instance of <see cref="CommandIterator"/> based on the
        /// <paramref name="selectionType"/>.
        /// </summary>
        /// <param name="selectionType">The <see cref="SelectionType"/> to
        /// instantiate the object with.</param>
        /// <returns>The generated instance of <see cref="CommandIterator"/>
        /// </returns>
		public static CommandIterator CreateDefault( SelectionType selectionType )
		{
			switch( selectionType )
			{
				case SelectionType.Element:
					return CreateDefaultElement();
				case SelectionType.Period:
					return CreateDefaultPeriod();
				case SelectionType.Unit:
					return CreateDefaultUnit();
			}

			return null;
		}

        //TODO make private
		public static CommandIterator CreateDefaultElement()
		{
			CommandIterator itr = new CommandIterator( CommandIterator.IteratorType.Row, "primary", CommandIterator.StyleType.Compact, "*" );
			return itr;
		}

        /// <summary>
        /// Create a new <see cref="CommandIterator"/> of period type.
        /// </summary>
        /// <returns>A <see cref="CommandIterator"/> instance.</returns>
		public static CommandIterator CreateDefaultPeriod()
		{
			CommandIterator itr = new CommandIterator( CommandIterator.IteratorType.Column, "period", CommandIterator.StyleType.Compact, "*" );
			return itr;
		}

		/// <summary>
        /// Create a new <see cref="CommandIterator"/> of primary type.
        /// </summary>
        /// <returns>A <see cref="CommandIterator"/> instance.</returns>
        public static CommandIterator CreateDefaultPrimary()
		{
			return CreateDefaultElement();
		}

        /// <summary>
        /// Create a new <see cref="CommandIterator"/> of unit type.
        /// </summary>
        /// <returns>A <see cref="CommandIterator"/> instance.</returns>
		public static CommandIterator CreateDefaultUnit()
		{
			CommandIterator itr = new CommandIterator( CommandIterator.IteratorType.Column, "unit", CommandIterator.StyleType.Compact, "*" );
			return itr;
		}

        /// <summary>
        /// Returns <see cref="CommandIterator.IteratorType.Column"/> or
        /// <see cref="CommandIterator.IteratorType.Row"/> based on the given
        /// type string.
        /// </summary>
        /// <param name="type">The string type to parse.</param>
        /// <param name="foundRows">Whether or not rows were found.</param>
        /// <returns></returns>
		public static IteratorType ParseIteratorType( string type, ref bool foundRows )
		{
			if( string.Equals( type, "column" ) )
				return CommandIterator.IteratorType.Column;

			if( string.Equals( type, "row" ) )
			{
				foundRows = true;
				return CommandIterator.IteratorType.Row;
			}

			return foundRows ?
				CommandIterator.IteratorType.Row :
				CommandIterator.IteratorType.Column;
		}

        //TODO make private
		public static SelectionType ParseSelectionType( string selectionString )
		{
			if( string.Equals( selectionString, "primary", StringComparison.CurrentCultureIgnoreCase ) )
				return SelectionType.Element;

			if( string.Equals( selectionString, "period", StringComparison.CurrentCultureIgnoreCase ) )
				return SelectionType.Period;

			if( string.Equals( selectionString, "separator", StringComparison.CurrentCultureIgnoreCase ) )
				return SelectionType.Separator;

			if( string.Equals( selectionString, "unit", StringComparison.CurrentCultureIgnoreCase ) )
				return SelectionType.Unit;

			return SelectionType.Axis;
		}

        /// <summary>
        /// Parse the style type string into a <see
        /// cref="CommandIterator.StyleType"/>.
        /// </summary>
        /// <param name="style">The string containing the style type.</param>
        /// <returns>A <see cref="CommandIterator.StyleType"/>.</returns>
		public static StyleType ParseStyleType( string style )
		{
			if( string.Equals( style, "grouped", StringComparison.CurrentCultureIgnoreCase ) )
				return CommandIterator.StyleType.Grouped;

			if( string.Equals( style, "nodisplay", StringComparison.CurrentCultureIgnoreCase ) )
				return CommandIterator.StyleType.NoDisplay;

			if( string.Equals( style, "segment", StringComparison.CurrentCultureIgnoreCase ) )
				return CommandIterator.StyleType.Segment;

			if( string.Equals( style, "unitcell", StringComparison.CurrentCultureIgnoreCase ) )
				return CommandIterator.StyleType.UnitCell;

			return CommandIterator.StyleType.Compact;
		}

        //TODO make private
		public static int DefaultComparer( CommandIterator left, CommandIterator right )
		{
			if( left == null )
			{
				if( right == null )
					return 0;
				else
					return 1;
			}
			else if( right == null )
			{
				return -1;
			}

			if( left.Index < right.Index )
				return -1;

			if( left.Index > right.Index )
				return 1;

			return 0;
		}
	}
}

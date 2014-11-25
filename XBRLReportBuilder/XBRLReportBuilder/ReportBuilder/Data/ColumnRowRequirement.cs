using System;
using System.Collections.Generic;
using System.Text;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser;
using System.Diagnostics;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;
using XBRLReportBuilder.Utilities;
using System.Xml;

namespace XBRLReportBuilder
{
	/// <summary>
	/// Holds all of the objects relevant to a set of <see cref="CommandIterator"/> which are stored in this.<see cref="EmbedCommands"/>.
	/// </summary>
	public class ColumnRowRequirement
	{
		public CommandIterator[] EmbedCommands { get; set; }

		public string ElementId
		{
			get
			{
				if( this.ElementRow == null )
					return string.Empty;
				else
					return this.ElementRow.ElementName;
			}
		}

		public string ElementKey
		{
			get
			{
				if( this.ElementRow == null )
					return string.Empty;
				else
					return this.ElementRow.ElementKey;
			}
		}

		public string ElementLabel
		{
			get
			{
				if( this.ElementRow == null )
					return string.Empty;
				else
					return this.ElementRow.Label;
			}
		}

		public InstanceReportRow ElementRow { get; set; }

		public CalendarPeriod Period { get; set; }
		public string PeriodLabel
		{
			get
			{
				if( this.Period == null )
					return string.Empty;
				else
					return Period.ToString();
			}
		}

		public List<Segment> Segments { get; set; }
		public List<string> SegmentLabels
		{
			get { return this.Segments.ConvertAll( seg => seg.ValueName ); }
		}

		public EmbeddedUnitWrapper Unit { get; set; }

		public string UnitCode
		{
			get
			{
				if( this.Unit == null )
					return string.Empty;
				else
					return this.Unit.GetCurrencyCode();
			}
		}

		public string UnitSymbol
		{
			get
			{
				if( this.Unit == null )
					return string.Empty;
				else
					return InstanceUtils.GetCurrencySymbolFromCode( this.UnitCode );
			}
		}

		public string UnitLabel
		{
			get
			{
				if( this.Unit == null )
					return string.Empty;

				string cc = this.UnitCode;
				string cs = this.UnitSymbol;
				if( !string.IsNullOrEmpty( cs ) )
					return string.Format( "{0} ({1})", cc, cs );

				if( !string.IsNullOrEmpty( cc ) )
					return cc;

				//if( !InstanceReportColumn.IsCustomUnit( this.Unit ) )
				//    return string.Empty;

				//return this.Unit.StandardMeasure.MeasureValue;
				return string.Empty;
			}
		}

        /// <summary>
        /// Creates a new <see cref="ColumnRowRequirement"/> instance.
        /// </summary>
		public ColumnRowRequirement()
		{
			this.EmbedCommands = new CommandIterator[ 0 ];
			this.Segments = new List<Segment>();
		}

        /// <summary>
        /// Creates a new <see cref="ColumnRowRequirement"/> object with
        /// <paramref name="iterators"/> as the embed commands.
        /// </summary>
        /// <param name="iterators">Embed commands.</param>
		public ColumnRowRequirement( params CommandIterator[] iterators ):this()
		{
			this.EmbedCommands = iterators;
		}

        /// <summary>
        /// Return the combined label for the given commands.
        /// </summary>
        /// <param name="iterator">The label pieces.</param>
        /// <param name="showDefault">Whether or not to show a default label
        /// when one cannot be generated.</param>
        /// <returns></returns>
		public string GetLabel( CommandIterator iterator, bool showDefault )
		{
			if( iterator.Style == CommandIterator.StyleType.NoDisplay )
				return string.Empty;

			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					if( this.hiddenObjects.Contains( this.ElementRow ) )
						return string.Empty;
					else
						return this.ElementLabel;


				case CommandIterator.SelectionType.Period:
					if( this.hiddenObjects.Contains( this.Period ) )
						return string.Empty;
					else
						return this.PeriodLabel;


				case CommandIterator.SelectionType.Unit:
					if( this.hiddenObjects.Contains( this.Unit ) )
						return string.Empty;
					else
						return this.UnitLabel;
			}

			if( this.Segments == null || this.Segments.Count == 0 )
				return string.Empty;

			int segIdx = this.Segments.FindIndex(
			seg =>
			{
				if( !string.Equals( seg.DimensionInfo.dimensionId, iterator.SelectionString ) )
					return false;

				if( iterator.Filter == "*" )
				{
					if( seg.IsDefaultForEntity )
						return showDefault;
					else
						return true;
				}

				if( !string.Equals( seg.DimensionInfo.Id, iterator.Filter ) )
					return false;

				if( seg.IsDefaultForEntity )
					return showDefault;
				else
					return true;
			} );

			if( segIdx == -1 )
				return string.Empty;
			else if( this.hiddenObjects.Contains( this.Segments[ segIdx ] ) )
				return string.Empty;
			else
				return this.SegmentLabels[ segIdx ];
		}

        //TODO remove - unused
		public string[] GetLabels()
		{
			return this.GetLabels( false );
		}

        //TODO make private
		public string[] GetLabels( bool showDefault )
		{
			return this.GetLabels( true, showDefault );
		}

        /// <summary>
        /// Retrieve all labels for a report.
        /// </summary>
        /// <param name="showCurrency">Whether or not to show currency labels.
        /// </param>
        /// <param name="showDefault">Whether or not to show default labels.
        /// </param>
        /// <returns></returns>
		public string[] GetLabels( bool showCurrency, bool showDefault )
		{
			List<string> labels = new List<string>();
			for( int c= 0; c < this.EmbedCommands.Length; c++ )
			{
				CommandIterator iterator = this.EmbedCommands[ c ];
				if( !showCurrency && iterator.Selection == CommandIterator.SelectionType.Unit )
					continue;

				string label = this.GetLabel( iterator, showDefault );
				if( string.IsNullOrEmpty( label ) )
					continue;

				if( !labels.Contains( label ) )
					labels.Add( label );
			}

			return labels.ToArray();
		}

		public string GetSeparator()
		{
			string separator = string.Empty;
			CommandIterator ci = Array.Find( this.EmbedCommands, ec => ec.Selection == CommandIterator.SelectionType.Separator );
			if( ci != null && !string.IsNullOrEmpty( ci.Filter ) )
			{
				separator = ci.Filter;
				if( separator.Length > 1 && separator.StartsWith( "\"" ) && separator.EndsWith( "\"" ) )
					separator = separator.Substring( 1, separator.Length - 2 );
			}
			return separator;
		}

		public void Add( CommandIterator iterator, object memberGroupValue )
		{
			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					this.ElementRow = memberGroupValue as InstanceReportRow;
					break;
				case CommandIterator.SelectionType.Period:
					this.Period = memberGroupValue as CalendarPeriod;
					break;
				case CommandIterator.SelectionType.Unit:
					this.Unit = (EmbeddedUnitWrapper)memberGroupValue;
					break;
			}

			//this must be a segment...
			Segment segment = memberGroupValue as Segment;
			if( segment != null )
				this.Segments.Add( segment );
		}

		public KeyValuePair<string,object> GetMemberKeyValue( CommandIterator iterator )
		{
			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					return new KeyValuePair<string,object>( this.ElementKey, this.ElementRow );
				case CommandIterator.SelectionType.Period:
					return new KeyValuePair<string,object>( this.Period.ToString(), this.Period );
				case CommandIterator.SelectionType.Unit:
					return new KeyValuePair<string,object>( this.UnitLabel, this.Unit );
			}

			//this must be a segment...
			Segment segment = this.Segments.Find( seg => seg.DimensionInfo.dimensionId == iterator.SelectionString );
			KeyValuePair<string, object> kvpSegment = kvpSegment = new KeyValuePair<string, object>( iterator.SelectionString, segment );
			return kvpSegment;
		}

		private List<object> hiddenObjects = new List<object>();
		public void HideLabel( CommandIterator iterator, string memberGroupKey )
		{
			object objToHide = null;
			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					objToHide = this.ElementRow;
					break;
				case CommandIterator.SelectionType.Period:
					objToHide = this.Period;
					break;
				case CommandIterator.SelectionType.Unit:
					objToHide = this.Unit;
					break;
			}

			//this must be a segment...
			if( objToHide == null )
				objToHide = this.Segments.Find( seg => seg.DimensionInfo.dimensionId == memberGroupKey );

			if( objToHide != null && !this.hiddenObjects.Contains( objToHide ) )
				this.hiddenObjects.Add( objToHide );
		}

		public void ShowLabel( CommandIterator iterator, string memberGroupKey )
		{
			object objToHide = null;
			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					objToHide = this.ElementRow;
					break;
				case CommandIterator.SelectionType.Period:
					objToHide = this.Period;
					break;
				case CommandIterator.SelectionType.Unit:
					objToHide = this.Unit;
					break;
			}

			//this must be a segment...
			if( objToHide == null )
				objToHide = this.Segments.Find( seg => seg.DimensionInfo.dimensionId == memberGroupKey );

			if( objToHide != null && this.hiddenObjects.Contains( objToHide ) )
				this.hiddenObjects.Remove( objToHide );
		}

		public void Remove( CommandIterator iterator, string memberGroupKey )
		{
			switch( iterator.Selection )
			{
				case CommandIterator.SelectionType.Element:
					this.ElementRow = null;
					break;
				case CommandIterator.SelectionType.Period:
					this.Period = null;
					break;
				case CommandIterator.SelectionType.Unit:
					this.Unit = null;
					break;
			}

			//this must be a segment...
			Segment segment = this.Segments.Find( seg => seg.DimensionInfo.dimensionId == memberGroupKey );
			if( segment != null )
				this.Segments.Remove( segment );
		}

		public CommandIterator.SelectionType GetIntersection( ColumnRowRequirement rightCol )
		{
			return GetIntersection( this, rightCol );
		}

        //TODO make private
		public static CommandIterator.SelectionType GetIntersection( ColumnRowRequirement leftCol, ColumnRowRequirement rightCol )
		{
			if( leftCol == null || rightCol == null)
				return CommandIterator.SelectionType.None;

			CommandIterator.SelectionType leftSelection = leftCol.GetSelections();
			CommandIterator.SelectionType rightSelection = rightCol.GetSelections();
			CommandIterator.SelectionType intersection = leftSelection & rightSelection;
			return intersection;
		}

        //TODO make private
		public CommandIterator.SelectionType GetSelections()
		{
			CommandIterator.SelectionType selections = CommandIterator.SelectionType.None;

			if( this.ElementRow != null )
				selections |= CommandIterator.SelectionType.Element;

			if( this.Period != null )
				selections |= CommandIterator.SelectionType.Period;

			if( this.Unit != null )
				selections |= CommandIterator.SelectionType.Unit;

			if( this.Segments != null && this.Segments.Count > 0 )
				selections |= CommandIterator.SelectionType.Axis;

			return selections;
		}

		public static bool AreSameExceptCurrency( ColumnRowRequirement thisRow, ColumnRowRequirement nextRow )
		{
			if( thisRow.ElementKey != nextRow.ElementKey )
				return false;

			if( thisRow.PeriodLabel != nextRow.PeriodLabel )
				return false;

			bool nextIsEmpty = nextRow.Segments == null || nextRow.Segments.Count == 0;
			if( thisRow.Segments == null || thisRow.Segments.Count == 0 )
			{
                if (nextIsEmpty)
                {
//                    Console.WriteLine("This/Next Segments are Null");
                    return true;
                }
                else
                    return false;
			}
			else if( nextIsEmpty )
			{
				return false;
			}

			if( thisRow.Segments.Count != nextRow.Segments.Count )
				return false;


            foreach (Segment iSeg in thisRow.Segments)
            {
                bool found = ReportUtils.Exists(nextRow.Segments,
                oSeg =>
                {
                    if (iSeg.DimensionInfo.dimensionId != oSeg.DimensionInfo.dimensionId)
                        return false;

                    if (iSeg.DimensionInfo.Id != oSeg.DimensionInfo.Id)
                        return false;

//                    Console.WriteLine("Found an ID Match " + iSeg.DimensionInfo.Id + "  " + oSeg.DimensionInfo.Id); 
                    return true;
                });

                if (!found)
                {
//                    Console.WriteLine("ID Match Not Found " + iSeg.DimensionInfo.Id + "  " + iSeg.DimensionInfo.dimensionId);
                    return false;
                }
            }
//            Console.WriteLine("Matched Segment"); 
            return true;

//			foreach( Segment iSeg in thisRow.Segments )
//			{
//               bool found = ReportUtils.Exists( nextRow.Segments,
//				oSeg =>
//				{
//					if( iSeg.DimensionInfo.dimensionId == oSeg.DimensionInfo.dimensionId || 
//                        iSeg.DimensionInfo.Id == oSeg.DimensionInfo.Id )
//						return true;
//                    else
//                    Console.WriteLine("Match Other Segment 1"); 
//                        return false;
//				} );
//
//                if (found)
//                    return true;
//                else
//                {
//                    Console.WriteLine("Match Other Segment 2");
//                    break;
//                }
//			}

//            Console.WriteLine("Misc Match Reason"); 
//            return false;
		}

		public bool IsDimensionMatch( ColumnRowRequirement columnRowRequirement )
		{
			foreach( Segment mySeg in this.Segments )
			{
				bool dimensionExists = ReportUtils.Exists( columnRowRequirement.Segments,
					iSeg => string.Equals( mySeg.DimensionInfo.dimensionId, iSeg.DimensionInfo.dimensionId ) );

				if( !dimensionExists )
					return false;
			}

			return true;
		}

		public bool HasSelectionType( CommandIterator iterator )
		{
			if( iterator.Selection == CommandIterator.SelectionType.Axis )
				return HasSelectionType( iterator.Selection, iterator.SelectionString, iterator.Filter );
			else
				return this.HasSelectionType( iterator.Selection );
		}

		public bool HasSelectionType( CommandIterator.SelectionType selectionType )
		{
			switch( selectionType )
			{
				case CommandIterator.SelectionType.Axis:
					throw new NotSupportedException( "SelectionType: Axis is not supported by this form of the method." );
				case CommandIterator.SelectionType.Element:
					return this.ElementRow != null;
				case CommandIterator.SelectionType.Period:
					return this.Period != null;
				case CommandIterator.SelectionType.Unit:
					return this.Unit != null;
			}

			return false;
		}

        //TODO make private
		public bool HasSelectionType( CommandIterator.SelectionType selectionType, string axis, string member )
		{
			if( selectionType == CommandIterator.SelectionType.Axis )
			{
				if( this.Segments == null || this.Segments.Count == 0 )
					return false;

				Segment s = this.Segments.Find( seg => seg.DimensionInfo.dimensionId == axis );
				if( s == null )
					return false;

				if( member == "*" )
					return true;

				return s.DimensionInfo.Id == member;
			}
			else
			{
				return this.HasSelectionType( selectionType );
			}
		}


		public bool IsAdjusted( XmlDocument membersXDoc )
		{
			int index = -1;
			return this.IsAdjusted( membersXDoc, out index );
		}

        //TODO make private
		public bool IsAdjusted( XmlDocument membersXDoc, out int index )
		{
			index = -1;

			if( membersXDoc == null )
				return false;

			if( this.Segments == null || this.Segments.Count == 0 )
				return false;

			int i = 0;
			foreach( Segment seg in this.Segments )
			{
				if( seg.IsDefaultForEntity )
					continue;

				if( IsSegmentAdjusted( seg, membersXDoc ) )
				{
					index = i;
					return true;
				}

				i++;
			}

			return false;
		}

		public static bool IsSegmentAdjusted( Segment seg, XmlDocument membersXDoc )
		{
			XmlNode adjustedParent = membersXDoc.SelectSingleNode( "/EquityMembers/AdjustedMembers" );
			foreach( XmlNode child in adjustedParent.ChildNodes )
			{
				if( !string.Equals( child.Name, seg.DimensionInfo.dimensionId ) )
					continue;

				if( string.Equals( child.InnerText, "*" ) || string.Equals( child.InnerText, seg.DimensionInfo.Id ) )
					return true;

			}
			return false;
		}

		public bool IsAsPreviouslyReported( XmlDocument membersXDoc )
		{
			int index = -1;
			return this.IsAsPreviouslyReported( membersXDoc, out index );
		}

        //TODO make private
		public bool IsAsPreviouslyReported( XmlDocument membersXDoc, out int index )
		{
			index = -1;

			if( membersXDoc == null )
				return false;

			if( this.Segments == null || this.Segments.Count == 0 )
				return false;

			int i = 0;
			foreach( Segment seg in this.Segments )
			{
				if( IsSegmentPreviouslyReported( seg, membersXDoc ) )
				{
					index = i;
					return true;
				}

				i++;
			}

			return false;
		}


		public static bool IsSegmentPreviouslyReported( Segment seg, XmlDocument membersXDoc )
		{
			XmlNode previousParent = membersXDoc.SelectSingleNode( "/EquityMembers/PreviouslyReportedMembers" );
			foreach( XmlNode child in previousParent.ChildNodes )
			{
				if( !string.Equals( child.Name, seg.DimensionInfo.dimensionId ) )
					continue;

				if( string.Equals( child.InnerText, "*" ) || string.Equals( child.InnerText, seg.DimensionInfo.Id ) )
					return true;

			}
			return false;
		}
	}
}

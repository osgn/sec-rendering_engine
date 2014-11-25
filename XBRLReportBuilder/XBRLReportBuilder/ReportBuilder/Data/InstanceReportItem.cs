using System;
using System.Collections.Generic;
using System.Text;
using XBRLReportBuilder;
using System.Xml.Serialization;
using Aucent.MAX.AXE.Common.Data;

namespace XBRLReportBuilder
{
	[Serializable]
	public abstract class InstanceReportItem
	{
		[XmlAttribute]
		public int FlagID = 0;

		//[XmlElement( Order = 1 )]
		public int Id = 0;

		//[XmlElement( Order = 2 )]
		public bool IsAbstractGroupTitle = false;

		//[XmlElement( Order = 3 )]
		public ColumnRowRequirement EmbedRequirements;

		//[XmlElement( Order = 4)]
		public string LabelSeparator
		{
			get { return this.GetSeparator(); }
			set { /* required for serialization */ }
		}

		[XmlIgnore]
		public bool HasMultiCurrency = false;

		[XmlIgnore]
		public bool IsGroupTotal
		{
			get { return this.GroupTotalSegment != null; }
		}
		
		[XmlIgnore]
		public Segment GroupTotalSegment = null;

		/// <summary>
		/// Returns the label for this item.  This value is never null.
		/// </summary>
		[XmlIgnore]
		public virtual string Label
		{
			get
			{
				if( this.Labels == null )
					return string.Empty;

				string separator = this.LabelSeparator;
				StringBuilder sb = new StringBuilder();
				for( int l = 0; l < this.Labels.Count; l++ )
				{
					if( l > 0 )
						sb.Append( separator );

					LabelLine label = this.Labels[l];
					sb.Append( label.Label );
				}
				return sb.ToString().Trim();
			}
			set
			{
				if( this.Labels == null )
				{
					this.Labels = new List<LabelLine>();
				}
				else
				{
					this.Labels.Clear();
					this.Labels.Add( new LabelLine( 1, value ) );
				}
			}
		}

		[XmlIgnore]
		public virtual List<LabelLine> Labels { get; set; }

		public InstanceReportItem()
		{
			if( this.Label == null )
				this.Label = string.Empty;

			if( this.Labels == null )
				this.Labels = new List<LabelLine>();
		}

		public void GenerateEmbedLabel()
		{
			if( this.EmbedRequirements != null )
			{
				this.GenerateEmbedLabel( this.EmbedRequirements );
			}
		}

		public void GenerateEmbedLabel( ColumnRowRequirement embedRequirements )
		{
			this.GenerateEmbedLabel( embedRequirements, false );
		}

		public void GenerateEmbedLabel( ColumnRowRequirement embedRequirements, bool showDefault )
		{
			this.Labels.Clear();

			string calendarLabel = null;
			if( embedRequirements.HasSelectionType( CommandIterator.SelectionType.Period ) )
			{
				CommandIterator cmdI = Array.Find( embedRequirements.EmbedCommands, cmd => cmd.Selection == CommandIterator.SelectionType.Period );
				if( cmdI != null )
					calendarLabel = embedRequirements.GetLabel( cmdI, false );
			}

			string[] labels = embedRequirements.GetLabels( this.HasMultiCurrency, showDefault );
			for( int i = 0; i < labels.Length; i++ )
			{
				if( calendarLabel != null )
				{
					if( string.Equals( labels[ i ], calendarLabel ) )
					{
						this.Labels.Add( new LabelLine( i, labels[ i ], "Calendar" ) );
						continue;
					}
				}

				this.Labels.Add( new LabelLine( i, labels[ i ] ) );
			}
		}

		public abstract Cell[] GetCellArray( InstanceReport report );

		public bool HasNonPeriodRequirements()
		{
			if( this.EmbedRequirements == null )
				return false;

			if( !string.IsNullOrEmpty( this.EmbedRequirements.ElementKey ) )
				return true;

			if( this.EmbedRequirements.Segments != null &&
				this.EmbedRequirements.Segments.Count > 0 )
				return true;

			if( this.EmbedRequirements.Unit != null )
				return true;

			return false;
		}

		public abstract void RemoveSelf( InstanceReport report );

		public abstract void ReplaceCell( InstanceReport report, int cellIndex, Cell cell );

		public bool HasUnitCell()
		{
			if( this.EmbedRequirements == null ||
				this.EmbedRequirements.EmbedCommands == null ||
				this.EmbedRequirements.EmbedCommands.Length == 0 )
				return false;

			bool hasUnitCell = Array.Exists(
				this.EmbedRequirements.EmbedCommands,
					ci => ci.Style == CommandIterator.StyleType.UnitCell );
			return hasUnitCell;
		}

		private string GetSeparator()
		{
			if( this.EmbedRequirements == null )
				return Environment.NewLine;

			string separator = this.EmbedRequirements.GetSeparator();
			if( string.IsNullOrEmpty( separator ) )
				return Environment.NewLine;

			return separator;
		}
	}
}

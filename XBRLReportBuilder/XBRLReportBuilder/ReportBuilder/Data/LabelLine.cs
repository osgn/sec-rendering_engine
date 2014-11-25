using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace XBRLReportBuilder
{
	[Serializable]
	public class LabelLine : IComparable, ICloneable
	{
		[XmlAttributeAttribute]
		public string Key = string.Empty;

		[XmlAttributeAttribute]
		public int Id = 0;

		[XmlAttributeAttribute]
		public string Label = string.Empty;

		[XmlIgnore]
		[NonSerialized]
		public bool IgnoreForMatch;

		public LabelLine()
		{
		}

		public LabelLine( LabelLine arg ) : this( arg.Id, arg.Label )
		{}

		public LabelLine( int id, string label )
		{
			this.Id = id;
			this.Label = label;
		}

		public LabelLine( int id, string label, string key ) : this( id, label )
		{
			this.Key = key;
		}

		public override bool Equals( object obj )
		{
			LabelLine ll = (LabelLine)obj;

			if( ll.IgnoreForMatch && IgnoreForMatch )
			{
				return true;
			}

			return Label.Equals( ll.Label );
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IComparable Members

		public int CompareTo( object obj )
		{
			LabelLine other = obj as LabelLine;
			if( other == null ) return 1;

			int result = this.Label.CompareTo( other.Label );
			if( result != 0 ) return result;

			return this.IgnoreForMatch.CompareTo( other.IgnoreForMatch );
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			LabelLine ll = this.MemberwiseClone() as LabelLine;
			return ll;
		}

		#endregion
	}
}

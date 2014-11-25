// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;

using Aucent.MAX.AXE.Common.Resources;
using Aucent.MAX.AXE.Common.Data;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Measure
	/// </summary>
	public class Measure
	{

		#region Constants
		internal const string MEASURE = "measure";
		#endregion

		#region properties
        private string measureType = string.Empty;
        private string measureDescription = string.Empty;

		/// <summary>
		/// Schema for this <see cref="Measure"/>.
		/// </summary>
		public string MeasureSchema = string.Empty;

		/// <summary>
		/// Value of this <see cref="Measure"/>.
		/// </summary>
        /// <remarks>
        /// This could be the "code" - for example "l".
        /// </remarks>
		public string MeasureValue = string.Empty;

        /// <summary>
        /// Value of this <see cref="Measure"/>.
        /// </summary>
        /// <remarks>
        /// This could be the "description" - for example "Liters".
        /// </remarks>
        public string MeasureDescription
        {
            get
            {
                return this.measureDescription;
            }
            set
            {
                this.measureDescription = value;
            }
        }

        /// <summary>
        /// Type of this <see cref="Measure"/>.
        /// </summary>
        /// <remarks>
        /// This could be the type in the type - for example "volumeItemType".
        /// </remarks> 
        public string MeasureType
        {
            get
            {
                return this.measureType;
            }
            set
            {
                this.measureType = value;
            }
        }

		/// <summary>
		/// XML namespace of this <see cref="Measure"/>.
		/// </summary>
		public string MeasureNamespace = string.Empty;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Measure"/>.
		/// </summary>
		public Measure()
		{
		}

		internal static bool TryCreateFromXml( XmlNode elem, ref Measure m, ref ArrayList errors )
		{
			if ( errors == null )
			{
				errors = new ArrayList();
			}

			if ( m == null )
			{
				m = new Measure();
			}

			string[] vals = elem.InnerXml.Split( ':' );

			XmlElement root = elem.OwnerDocument.DocumentElement;

			// find the root namespace and add it
			if (vals.Length > 1)
			{
				foreach ( XmlAttribute attr in root.Attributes )
				{
					if ( attr.LocalName.CompareTo( vals[0] ) == 0 )
					{
						m.MeasureSchema = attr.Value;
						break;
					}
				}

				if (m.MeasureSchema .Length == 0) //try get the measure schema from the element attribute
				{
					foreach (XmlAttribute attr in elem.Attributes)
					{
						if ( attr.LocalName.CompareTo( vals[0] ) == 0 )
						{
							m.MeasureSchema = attr.Value;
							break;
						}
					}
				}

				m.MeasureNamespace = vals[0];
				m.MeasureValue = vals[1];
			}

			if (vals.Length == 1)
			{
				m.MeasureSchema = elem.NamespaceURI;
				m.MeasureNamespace = string.Empty;
				m.MeasureValue = vals[0];
			}

			return m.Validate( errors );
		}
		#endregion

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="Measure"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="Measure"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Measure"/> is equivalent to this instance
		/// of <see cref="Measure"/>.
		/// </summary>
		/// <param name="m">A <see cref="Measure"/> 
		/// that is to be compared to this instance of <see cref="Measure"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the objects are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="Measure"/> objects are equal if there measure schemas, measure 
		/// values, and measure namespaces are equal when compared as <see cref="String"/> objects.</remarks>
		public bool Equals(Measure m)
		{
			if (MeasureSchema				!= m.MeasureSchema			) return false;	
			if (MeasureValue				!= m.MeasureValue				) return false;
			if (MeasureNamespace		!= m.MeasureNamespace		) return false;
			 
			return true;
		}

		/// <summary>
		/// value compare for sorting
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareValue(Measure other)
		{
			int ret = string.Compare(this.MeasureSchema, other.MeasureSchema);
			if (ret != 0) return ret;


			ret = string.Compare(this.MeasureValue, other.MeasureValue);
			if (ret != 0) return ret;

			return string.Compare(this.MeasureNamespace, other.MeasureNamespace);
			


		}

		public void AppendNamespace( XmlElement root )
		{
			if (MeasureNamespace != string.Empty)
			{
				root.SetAttribute( string.Format( "xmlns:{0}", MeasureNamespace ), MeasureSchema );
			}
			else
			{
				root.SetAttribute(DocumentBase.XMLNS, MeasureSchema);
			}
		}

		public XmlElement CreateElement( XmlDocument doc, XmlElement root, bool applyNamespaces )
		{
			AppendNamespace( root );

			XmlElement meas = applyNamespaces ? doc.CreateElement(MEASURE, DocumentBase.XBRL_INSTANCE_URL) : doc.CreateElement(MEASURE, root.NamespaceURI);

			if ( MeasureNamespace != string.Empty )
			{
				XmlText text = doc.CreateTextNode(string.Format(DocumentBase.NAME_FORMAT, MeasureNamespace, MeasureValue));
				meas.AppendChild( text );
			}
			else
			{
				XmlText text = doc.CreateTextNode( MeasureValue );
				meas.AppendChild( text );
			}

			return meas;
		}

		/// <summary>
		/// Determines if the properties of this <see cref="Measure"/> are valid.
		/// </summary>
		/// <param name="errors">A collection of <see cref="String"/> objects to which method 
		/// will append any errors that render the properties invalid.</param>
		/// <returns>True if the properties are valid.  False otherwise</returns>
		public bool Validate( ArrayList errors )
		{
			bool valid = true;

			// Namespace is not required.  A measure can be defined in the default namespace.
//			if ( MeasureNamespace == string.Empty )
//			{
//				valid = false;
//				Common.WriteError( "XBRLParser.Error.MeasurePropertyNull", errors, "MeasureNamespace" );
//			}

			if ( MeasureSchema == string.Empty )
			{
				valid = false;
				Common.WriteError( "XBRLParser.Error.MeasurePropertyNull", errors, "MeasureSchema" );
			}

			if ( MeasureValue == string.Empty )
			{
				valid = false;
				Common.WriteError( "XBRLParser.Error.MeasurePropertyNull", errors, "MeasureValue" );
			}

			//if ( string.Compare( MeasureSchema, "http://www.xbrl.org/2003/iso4217" ) == 0 )
			//{
			//    if ( string.Compare( MeasureNamespace, "iso4217", true ) != 0 )
			//    {
			//        valid = false;
			//        Common.WriteError( "XBRLParser.Error.MeasureNamespaceMustBe4217", errors );
			//    }

			//    //TODO: MeasureValue must be defined in the iso4217 doc
			//}
			if (string.Compare(MeasureSchema, DocumentBase.XBRL_INSTANCE_URL) == 0)
			{
				if ( string.Compare( MeasureValue, "pure" ) != 0 && string.Compare( MeasureValue, MeasureUnit._shares ) != 0 )
				{
					valid = false;
					Common.WriteError( "XBRLParser.Error.MeasureValuePureOrShares", errors );
				}
			}

			return valid;
		}
	}
}
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

using System.Text;
using System.Collections;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// UnitProperty
	/// </summary>
	public class UnitProperty : IComparable
	{
		#region Constants
		internal const string UNIT = "unit";
		private const string SCALE = "Scale";
		private const string MEASURE = "measure";

		private const string DIVIDE = "divide";
		private const string NUMER = "unitNumerator";
		private const string DENOM = "unitDenominator";

		private const string ID_ATTR = "id";

		#endregion

		#region enums

		/// <summary>
		/// The XBRL standard operators for units
		/// </summary>
		public enum UnitTypeCode
		{
			/// <summary>
			/// Normal.  A unit to which no operation is applied.
			/// </summary>
			Standard,

			/// <summary>
			/// Measures are combined by division to determine the unit.
			/// </summary>
			Divide,

			/// <summary>
			/// Measures are combined by multiplication to determine the unit.
			/// </summary>
			Multiply
		}

		#endregion

		#region properties

		private XmlNode xmlUnit = null;
		private int UseCount = 0;

		/// <summary>
		/// There are 2 major parts in the ContextProperties:
		/// Measure (required)
		/// ComplexUnit (optional)
		/// 
		/// Naming convention -- 
		/// xxxxxxID: Unique name that Identifies xxxxxx and must start with a letter
		/// xxxxxxSchema: location of the namespace
		/// xxxxxxValueType: defines a TYPE the value belongs to (for example, SegmentValueType might be "State or Province")
		/// xxxxxxValue: defines a value (for example, SegmentValue might be "CO")
		/// xxxxxxNamespace:specifies name of the schema
		/// 
		/// </summary>

		public string UnitID = string.Empty;

		// <summary>
		// Example1:
		// <unit id="u2">
		//		<measure xmlns:ISO4217="http://www.xbrl.org/2003/iso4217">ISO4217:gbp</measure>
		//	</unit>
		//	Schema = "http://www.xbrl.org/2003/iso4217"
		//	Namespace = ISO4217
		//	Value = gbp
		//	<unit id="u1"><measure>xbrli:pure</measure></unit>
		//	<unit id="u4"><measure>xbrli:shares</measure></unit>
		//	Namespace = sbrli
		//	Value = pure/share
		// Example2:
		// <unit id="u4">
		//		<measure>myuom:feet</measure>
		//		<measure>myuom:feet</measure>
		//	</unit>
		// Example3:
		//<unit id="u6">
		//	 <divide>
		//		<unitNumerator>
		//			<measure>ISO4217:EUR</measure>
		//		</unitNumerator>
		//		<unitDenominator>
		//			<measure>xbrli:shares</measure>
		//		</unitDenominator>
		//</divide>
		//</unit>
		//</summary>
		//


		/// <summary>
		/// The unit type for this <see cref="UnitProperty"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="UnitType"/> is <see cref="UnitTypeCode.Standard"/>, <see cref="StandardMeasure"/> must 
		/// be populated.
		/// If <see cref="UnitType"/> is <see cref="UnitTypeCode.Multiply"/>, <see cref="MultiplyMeasures"/> must 
		/// be populated.
		/// If <see cref="UnitType"/> is <see cref="UnitTypeCode.Divide"/>, <see cref="NumeratorMeasure"/> 
		/// and <see cref="DenominatorMeasure"/> must be populated.
		/// </remarks>
		public UnitTypeCode UnitType = UnitTypeCode.Standard;

		/// <summary>
		/// The <see cref="Measure"/> for this <see cref="UnitProperty"/> if <see cref="UnitType"/> is 
		/// <see cref="UnitTypeCode.Standard"/>.
		/// </summary>
		public Measure StandardMeasure = new Measure();

		/// <summary>
		/// The <see cref="Measure"/> collection for this <see cref="UnitProperty"/> if <see cref="UnitType"/> is 
		/// <see cref="UnitTypeCode.Multiply"/>.
		/// </summary>
		public Measure[] MultiplyMeasures = new Measure[2]
			{
				new Measure(),
				new Measure()
			};

		/// <summary>
		/// The numerator for this <see cref="UnitProperty"/> if <see cref="UnitType"/> is 
		/// <see cref="UnitTypeCode.Divide"/>.
		/// </summary>
		public Measure NumeratorMeasure = new Measure();

		/// <summary>
		/// The denominator for this <see cref="UnitProperty"/> if <see cref="UnitType"/> is 
		/// <see cref="UnitTypeCode.Divide"/>.
		/// </summary>
		public Measure DenominatorMeasure = new Measure();

		/// <summary>
		/// The scale of this <see cref="UnitProperty"/>
		/// </summary>
		/// <remarks>
		/// Multiply by 10 to the power of Scale (e.g., scale = 6, multiplies by 1M).
		/// </remarks>
		public int Scale = 0;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="UnitProperty"/>.
		/// </summary>
		public UnitProperty()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="UnitProperty"/>.
		/// </summary>
		/// <param name="unitID"></param>
		/// <param name="unitType"></param>
		/// <remarks>UnitID and unitType are required to build a valid Unit Ref.</remarks>
		public UnitProperty(string unitID, UnitTypeCode unitType)
		{
			this.UnitID = unitID;
			this.UnitType = unitType;
		}

		internal static bool TryCreateFromXml(XmlNode elem, XmlNamespaceManager theManager, out UnitProperty up, ref ArrayList errors)
		{
			up = new UnitProperty();

			up.UnitID = elem.Attributes[ID_ATTR].Value;

			XmlNodeList measures = theManager == null ? elem.SelectNodes(Measure.MEASURE) : elem.SelectNodes("./link2:" + Measure.MEASURE, theManager);
			if (measures.Count > 0)
			{
				if (measures.Count == 1)
				{
					up.UnitType = UnitTypeCode.Standard;
					return Measure.TryCreateFromXml(measures[0], ref up.StandardMeasure, ref errors);
				}
				else
				{
					up.UnitType = UnitTypeCode.Multiply;
					up.MultiplyMeasures = new Measure[measures.Count];
					for (int i = 0; i < measures.Count; ++i)
					{
						if (!Measure.TryCreateFromXml(measures[i], ref up.MultiplyMeasures[i], ref errors))
						{
							return false;
						}
					}
				}
			}
			else
			{
				XmlNode divide = theManager == null ? elem.SelectSingleNode(DIVIDE) : elem.SelectSingleNode("./link2:" + DIVIDE, theManager);

				if (divide != null)
				{
					up.UnitType = UnitTypeCode.Divide;

					XmlNode numerator = theManager == null ? divide.SelectSingleNode(NUMER) : divide.SelectSingleNode("./link2:" + NUMER, theManager);
					XmlNode denominator = theManager == null ? divide.SelectSingleNode(DENOM) : divide.SelectSingleNode("./link2:" + DENOM, theManager);

					if (numerator == null)
					{
						Common.WriteError("XBRLParser.Error.MissingRequiredField", errors, NUMER);
						return false;
					}
					else if (denominator == null)
					{
						Common.WriteError("XBRLParser.Error.MissingRequiredField", errors, DIVIDE);
						return false;
					}

					if (!Measure.TryCreateFromXml(numerator.FirstChild, ref up.NumeratorMeasure, ref errors))
					{
						return false;
					}
					else if (!Measure.TryCreateFromXml(denominator.FirstChild, ref up.DenominatorMeasure, ref errors))
					{
						return false;
					}

					return up.ValidateDivision(errors);
				}
				else
				{
					Common.WriteError("XBRLParser.Error.MissingRequiredField", errors, "measure");
					return false;
				}
			}

			return true;
		}
		#endregion

		#region instance doc xml stuff

		internal void RemoveXmlNode()
		{
			if (--UseCount == 0 && xmlUnit != null)
			{
				xmlUnit.ParentNode.RemoveChild(xmlUnit);
				xmlUnit = null;
			}
		}

		private void IncrementInUseCount()
		{
			++UseCount;
		}
		#endregion

		#region methods

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="UnitProperty"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="UnitProperty"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="object"/> is equivalent to this instance
		/// of <see cref="UnitProperty"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="UnitProperty"/>
		/// that is to be compared to this instance of <see cref="UnitProperty"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the objects are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="UnitProperty"/> objects are considered equal if their unit types, 
		/// scale, and measure are equal.  The specific measure compared depends on the unit type.</remarks>
		public bool ValueEquals(object obj)
		{
			if (!(obj is UnitProperty)) return false;

			UnitProperty up = obj as UnitProperty;

			if (UnitType != up.UnitType) return false;
			if (Scale != up.Scale) return false;

			switch (UnitType)
			{
				case UnitTypeCode.Standard:
					if (!StandardMeasure.Equals(up.StandardMeasure)) return false;
					break;
				case UnitTypeCode.Multiply:
					if (!MultiplyMeasures[0].Equals(up.MultiplyMeasures[0])) return false;
					if (!MultiplyMeasures[1].Equals(up.MultiplyMeasures[1])) return false;
					break;
				case UnitTypeCode.Divide:
					if (!NumeratorMeasure.Equals(up.NumeratorMeasure)) return false;
					if (!DenominatorMeasure.Equals(up.DenominatorMeasure)) return false;
					break;
			}

			return true;
		}

		/// <summary>
		/// Determines whether a supplied <see cref="object"/> is equivalent to this instance
		/// of <see cref="UnitProperty"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="UnitProperty"/>
		/// that is to be compared to this instance of <see cref="UnitProperty"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the objects are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="UnitProperty"/> objects are considered equal if <see cref="ValueEquals(Object)"/> 
		/// for the two objects is true and their unit id properties are equal.</remarks>
		public override bool Equals(object obj)
		{
			if (!(obj is UnitProperty)) return false;

			UnitProperty up = obj as UnitProperty;

			if (UnitID != up.UnitID) return false;

			return ValueEquals(obj);
		}

		/// <summary>
		/// Converts the value of this <see cref="UnitProperty"/> to its equivalent string representation.
		/// </summary>
		/// <returns>A <see cref="String"/> containing the representation of this <see cref="UnitProperty"/>.</returns>
		/// <remarks>A <see cref="UnitProperty"/> is represented by its unit type and measure related to that unit type.</remarks>
		public override string ToString()
		{
			StringBuilder sbUnitRef = new StringBuilder();

			sbUnitRef.Append("Unit Type:").Append(this.UnitType.ToString()).Append(Environment.NewLine);
			if (UnitType == UnitTypeCode.Multiply)
			{
				if (MultiplyMeasures[0].MeasureValue != string.Empty && MultiplyMeasures[1].MeasureValue != string.Empty)
				{
					sbUnitRef.Append(MultiplyMeasures[0].MeasureValue).Append(" * ").Append(MultiplyMeasures[1].MeasureValue).Append(Environment.NewLine);
				}
			}

			if (UnitType == UnitTypeCode.Divide)
			{
				if (NumeratorMeasure.MeasureValue != string.Empty && DenominatorMeasure.MeasureValue != string.Empty)
				{
					sbUnitRef.Append(NumeratorMeasure.MeasureValue).Append(" / ").Append(DenominatorMeasure.MeasureValue).Append(Environment.NewLine);
				}
			}

			return sbUnitRef.ToString();
		}


		public string GetMeasurementString()
		{
			string ret = string.Empty;
			switch (UnitType)
			{
				case UnitTypeCode.Standard:
					ret = StandardMeasure.MeasureValue;
					break;
				case UnitTypeCode.Multiply:
					ret = "'" + MultiplyMeasures[0].MeasureValue + "' * '" + MultiplyMeasures[1].MeasureValue + "'";
					break;
				case UnitTypeCode.Divide:
					ret = "'" + NumeratorMeasure.MeasureValue + "' / '" + DenominatorMeasure.MeasureValue + "'";
					break;
			}
			return ret;
		}

		#endregion

		private bool ValidateDivision(ArrayList errors)
		{
			if (!DenominatorMeasure.Validate(errors) ||
				!NumeratorMeasure.Validate(errors))
			{
				return false;
			}

			if (NumeratorMeasure.Equals(DenominatorMeasure))
			{
				Common.WriteError("XBRLParser.Error.NumerAndDenomEqual", errors, UnitID);
				return false;
			}

			return true;
		}

		#region to xml
		/// <summary>
		/// Creates an <see cref="XmlElement"/> to represent this instance of <see cref="UnitProperty"/>.  Save the element 
		/// as a property of this <see cref="UnitProperty"/>.  Elements are created and named with namespace qualification.
		/// </summary>
		/// <param name="doc">The <see cref="XmlDocument"/> from which any elements are to be created.</param>
		/// <param name="root">The root <see cref="XmlElement"/> to which elements will be appended.</param>
		/// <param name="errorList">A collection of <see cref="String"/> objects to which method 
		/// will append any errors that render the properties invalid.</param>
		/// <param name="unit">An output parameter.</param>
		/// <param name="writeComment">Not used.</param>
		/// <returns>True if <see cref="XmlElement"/> could be successfully created and written.</returns>
		public bool CreateElementWithNamespaces(XmlDocument doc, XmlElement root, ArrayList errorList, out XmlElement unit, bool writeComment)
		{
			return WriteXml(doc, root, errorList, out unit, writeComment, true);
		}

		/// <summary>
		/// Creates an <see cref="XmlElement"/> to represent this instance of <see cref="UnitProperty"/>.  Save the element 
		/// as a property of this <see cref="UnitProperty"/>.  Elements are created and named without namespace qualification.
		/// </summary>
		/// <param name="doc">The <see cref="XmlDocument"/> from which any elements are to be created.</param>
		/// <param name="root">The root <see cref="XmlElement"/> to which elements will be appended.</param>
		/// <param name="errorList">A collection of <see cref="String"/> objects to which method 
		/// will append any errors that render the properties invalid.</param>
		/// <param name="unit">An output parameter.</param>
		/// <param name="writeComment">Not used.</param>
		/// <returns>True if <see cref="XmlElement"/> could be successfully created and written.</returns>
		public bool CreateElement(XmlDocument doc, XmlElement root, ArrayList errorList, out XmlElement unit, bool writeComment)
		{
			return WriteXml(doc, root, errorList, out unit, writeComment, false);
		}

		/// <summary>
		/// Creates an <see cref="XmlElement"/> to represent this instance of <see cref="UnitProperty"/>.  Save the element 
		/// as a property of this <see cref="UnitProperty"/>.
		/// </summary>
		/// <param name="doc">The <see cref="XmlDocument"/> from which any elements are to be created.</param>
		/// <param name="root">The root <see cref="XmlElement"/> to which elements will be appended.</param>
		/// <param name="errorList">A collection of <see cref="String"/> objects to which method 
		/// will append any errors that render the properties invalid.</param>
		/// <param name="unit">An output parameter.</param>
		/// <param name="writeComment">Not used.</param>
		/// <param name="applyNamespaces">Namespaces are to be used in creating and naming newly created elements.</param>
		/// <returns>True if <see cref="XmlElement"/> could be successfully created and written.</returns>
		protected bool WriteXml(XmlDocument doc, XmlElement root, ArrayList errorList, out XmlElement unit, bool writeComment, bool applyNamespaces)
		{
			unit = applyNamespaces ? doc.CreateElement(UNIT, DocumentBase.XBRL_INSTANCE_URL) : doc.CreateElement(UNIT);

			this.xmlUnit = unit;
			++UseCount;

			unit.SetAttribute(ID_ATTR, UnitID);

			/*if (writeComment)
				unit.AppendChild( doc.CreateComment( string.Format( "{0}: {1}", SCALE, Scale ) ) );*/

			switch (UnitType)
			{
				case UnitTypeCode.Standard:
					if (!StandardMeasure.Validate(errorList))
					{
						unit = null;
						return false;
					}

					unit.AppendChild(StandardMeasure.CreateElement(doc, root, applyNamespaces));
					break;

				case UnitTypeCode.Divide:
					if (!ValidateDivision(errorList))
					{
						unit = null;
						return false;
					}
					XmlElement divide = applyNamespaces ? doc.CreateElement(DIVIDE, DocumentBase.XBRL_INSTANCE_URL) : doc.CreateElement(DIVIDE);
					unit.AppendChild(divide);

					XmlElement numer = applyNamespaces ? doc.CreateElement(NUMER, DocumentBase.XBRL_INSTANCE_URL) : doc.CreateElement(NUMER);
					divide.AppendChild(numer);
					numer.AppendChild(NumeratorMeasure.CreateElement(doc, root, applyNamespaces));

					XmlElement denom = applyNamespaces ? doc.CreateElement(DENOM, DocumentBase.XBRL_INSTANCE_URL) : doc.CreateElement(DENOM);
					divide.AppendChild(denom);
					denom.AppendChild(DenominatorMeasure.CreateElement(doc, root, applyNamespaces));

					break;

				case UnitTypeCode.Multiply:
					foreach (Measure m in MultiplyMeasures)
					{
						m.Validate(errorList);
						unit.AppendChild(m.CreateElement(doc, root, applyNamespaces));
					}

					if (errorList.Count != 0)
					{
						unit = null;
						return false;
					}
					break;
			}

			//doc.AppendChild( unit );
			return true;
		}
		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="UnitProperty"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="UnitProperty"/>
		/// is to be compared.  Assumed to be a <see cref="UnitProperty"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="UnitProperty"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of CompareTo for the unit id of the 
		/// two <see cref="UnitProperty"/> objects.</remarks>
		public int CompareTo(object obj)
		{
			if (obj is string) return UnitID.CompareTo((string)obj);

			return UnitID.CompareTo(((UnitProperty)obj).UnitID);
		}

		#endregion

		#region value equals

		
		/// <summary>
		/// Compare the values of two unit properties to help with sorting...
		/// </summary>
		/// <param name="other">A <see cref="UnitProperty"/>
		/// that is to be compared to this instance of <see cref="UnitProperty"/>.</param>
		/// <returns></returns>
		public int CompareValue(UnitProperty other)
		{
			// don't check id
			int ret = this.UnitType.CompareTo(other.UnitType);
			if (ret != 0) return ret;



			switch (UnitType)
			{
				case UnitTypeCode.Standard:
					ret = StandardMeasure.CompareValue(other.StandardMeasure);
					if (ret != 0) return ret;

					break;
				case UnitTypeCode.Multiply:

					ret = MultiplyMeasures[0].CompareValue(other.MultiplyMeasures[0]);
					if (ret != 0) return ret;
					ret = MultiplyMeasures[1].CompareValue(other.MultiplyMeasures[1]);
					if (ret != 0) return ret;

					break;
				case UnitTypeCode.Divide:
					ret = NumeratorMeasure.CompareValue(other.NumeratorMeasure);
					if (ret != 0) return ret;
					ret = DenominatorMeasure.CompareValue(other.DenominatorMeasure);
					if (ret != 0) return ret;
					break;
			}


			return 0;
		}

		/// <summary>
		/// Determines whether the value of a supplied <see cref="UnitProperty"/> is equivalent to the value 
		/// of this instance of <see cref="UnitProperty"/>.
		/// </summary>
		/// <param name="up">A <see cref="UnitProperty"/>
		/// that is to be compared to this instance of <see cref="UnitProperty"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the object values are equal (true) or not (false).</returns>
		/// <remarks>Two <see cref="UnitProperty"/> items values are considered equal if their unit types and measures 
		/// are equal.  The measures compared depend upon the unit type.</remarks>
		public bool ValueEquals(UnitProperty up)
		{
			// don't check id
			if (UnitType == up.UnitType)
			{
				switch (UnitType)
				{
					case UnitTypeCode.Standard:
						return StandardMeasure.Equals(up.StandardMeasure);

					case UnitTypeCode.Multiply:
						if (!MultiplyMeasures[0].Equals(up.MultiplyMeasures[0])) return false;
						return MultiplyMeasures[1].Equals(up.MultiplyMeasures[1]);

					case UnitTypeCode.Divide:
						if (!NumeratorMeasure.Equals(up.NumeratorMeasure)) return false;
						return DenominatorMeasure.Equals(up.DenominatorMeasure);
				}
			}

			return false;
		}
		#endregion
	}
}
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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.XBRLParser.Interfaces;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// MarkupProperty.
	/// </summary>
    public class MarkupProperty : LinkableProperty, IComparable, IHasContextProperty, ITupleSetChild
	{
		#region properties

		/// <summary>
		/// A <see cref="String"/> representation of the context of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <remarks>
		/// Exposed as public for Dragon View.
		/// </remarks>
		public string contextText;

		protected ContextProperty _contextRef = null;

		/// <summary>
		/// The context of this <see cref="MarkupProperty"/>.
		/// </summary>
		public ContextProperty contextRef
		{
			get { return this._contextRef; }
			set { this._contextRef = value; }
		}

		/// <summary>
		/// A <see cref="String"/> representation of the units associated of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <remarks>
		/// Exposed as public for Dragon View.
		/// </remarks>
		public string unitText;

		/// <summary>
		/// The units associated with this <see cref="MarkupProperty"/>.
		/// </summary>
		public UnitProperty unitRef = null;

		/// <summary>
		/// A <see cref="String"/> representation of the precision of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <remarks>
		/// Exposed as public for Dragon View.
		/// </remarks>
		#pragma warning disable 0414
		private string precisionText;
		#pragma warning restore 0414

		/// <summary>
		/// The precision associated with this <see cref="MarkupProperty"/>.
		/// </summary>
		public Precision precisionDef = null;

		/// <summary>
		/// Element namespace.
		/// </summary>
		public string elementNamespace;

		/// <summary>
		/// Element prefix.
		/// </summary>
		public string elementPrefix;

		/// <summary>
		/// Element name.
		/// </summary>
		public string elementName;

		/// <summary>
		/// Element id.
		/// </summary>
		public string elementId;

		/// <summary>
		/// Element as <see cref="Node"/>.
		/// </summary>
		public Node element = null;

		/// <summary>
		/// Element as <see cref="XmlNode"/>.
		/// </summary>
		public XmlNode xmlElement = null;

		/// <summary>
		/// Collection of <see cref="XmlComment"/> objects.
		/// </summary>
		public ArrayList xmlComments = new ArrayList();
		internal ArrayList oldXmlComments = new ArrayList();

		/// <summary>
		/// The index of this <see cref="MarkupProperty"/> within the taxonomy items 
		/// collection.
		/// </summary>
		public int taxonomyIndex;

		/// <summary>
		/// Tuple set name.
		/// </summary>
		public string TupleSetName = null;

		internal ArrayList validationErrors = new ArrayList();

		/// <summary>
		/// List of errors (strings) resulting from validation
		/// </summary>
		public ArrayList ValidationErrors
		{
			get{ return validationErrors; }
			set{ validationErrors = value; }
		}

		/// <summary>
		/// Defines the possible <see cref="MarkupProperty"/> validation status values.
		/// </summary>
		public enum ValidationStatus
		{
			/// <summary>
			/// Unknown.  
			/// </summary>
			UNKNOWN,

			/// <summary>
			/// Validated with no errors or warnings.
			/// </summary>
			OK,

			/// <summary>
			/// Validated with warnings.
			/// </summary>
			WARNING,

			/// <summary>
			/// Validation incomplete.
			/// </summary>
			INCOMPLETE,
			/// <summary>
			/// Validated with errors.
			/// </summary>
			ERROR
		}

		private ValidationStatus markupStatus = ValidationStatus.OK;

		/// <summary>
		/// The markup validation status of this <see cref="MarkupProperty"/>.
		/// </summary>
		public ValidationStatus MarkupStatus
		{
			get {return markupStatus;}
			set {markupStatus = value;}
		}

		/// <summary>
		/// True if this <see cref="MarkupProperty"/> has associated footnotes.
		/// </summary>
		public bool HasFootnotes
		{
			get { return _linker.Count > 0; }
		}

        /// <summary>
        /// Id of the element
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            return elementId;
        }

        /// <summary>
        /// List of tuple parents for this markup item
        /// item zero is the immediate parent .....
        /// </summary>
        public List<string> TupleParentList;

        /// <summary>
        /// It is either the parent tupleset for simple tuples and the outermost tupleset for all nested tuples
        /// this is populated when the markupProperty is loaded from the instance document
        /// this makes it easy to group markups...
        /// </summary>
        public TupleSet TopLevelTupleset = null;


	
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="MarkupProperty"/>.
		/// </summary>
		public MarkupProperty()
		{
		}

		/// <summary>
		/// Creates and populates a new <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="index">An index into <paramref name="nodes"/> indicating the <see cref="XmlNode"/> that is to be the 
		/// source of information for populating <paramref name="mp"/>.</param>
		/// <param name="nodes">The <see cref="XmlNodeList"/> containing the <XmlNode> from which <paramref name="mp"/> is 
		/// to be created.</XmlNode></param>
		/// <param name="contexts">Collection of <see cref="ContextProperty"/> to be assigned to <paramref name="mp"/>.</param>
		/// <param name="units">Collection of <see cref="UnitProperty"/> to be assigned to <paramref name="mp"/>.</param>
		/// <param name="mp">An output parameter.  The newly created <see cref="MarkupProperty"/>.</param>
		/// <param name="errors"></param>
		/// <returns>True if <paramref name="mp"/> could be successfully created and populated.</returns>
		public static bool TryCreateFromXml( int index, XmlNodeList nodes, ArrayList contexts, ArrayList units, out MarkupProperty mp, ref ArrayList errors )
		{
			XmlNode data = nodes[index];

			mp = new MarkupProperty();
			if ( errors == null )
			{
				errors = new ArrayList();
			}

			// first get the name
			string[] nameParts = data.Name.Split( ':' );
			mp.elementNamespace = data.NamespaceURI;
			mp.elementPrefix = nameParts[0];
			mp.elementName = nameParts[1];
			mp.elementId = string.Format(DocumentBase.ID_FORMAT, nameParts[0], nameParts[1]);

			mp.element = new Node( new Element( mp.elementName, mp.elementId, false ) );
			mp.element.MyElement.HasCompleteTupleFamily = false;

			// try and get the context ref
			XmlAttribute contextAttr = data.Attributes[ Instance.CONTEXT_REF ];
			if ( contextAttr == null )
			{
				Common.WriteError( "XBRLParser.Error.ElementMissingRequiredField", errors, Instance.CONTEXT_REF );
				return false;
			}

			string contextId = contextAttr.Value;

			// make sure we can find the real context
			int contextIndex = contexts.BinarySearch( contextId );
			if ( contextIndex < 0 )
			{
				Common.WriteError( "XBRLParser.Error.ContextIdNotFound", errors, contextId );
				return false;
			}

			mp.SetContext( (ContextProperty)contexts[contextIndex] );

			// and get the unit
			XmlAttribute unitAttr = data.Attributes[ Instance.UNIT_REF ];
			if ( unitAttr != null )
			{
				string unitId = unitAttr.Value;

				// make sure we can find the real context
				int unitIndex = units.BinarySearch( unitId );
				if ( unitIndex < 0 )
				{
					Common.WriteError( "XBRLParser.Error.UnitIdNotFound", errors, unitId );
					return false;
				}

				mp.SetUnit( (UnitProperty)units[unitIndex] );
			
				// and precision
				XmlAttribute attr = data.Attributes[ Precision.DECIMALS ];
				if ( attr == null )
				{
					attr = data.Attributes[ Precision.PRECISION ];
				}

				if ( attr != null )
				{
					Precision p = null;
					if ( !Precision.TryCreateFromXml( attr, out p, ref errors ) )
					{
						return false;
					}

					mp.SetPrecision( p );
				}
			}

			mp.markupData = data.InnerText;

			mp.AddXmlComments( index, nodes );
			return true;
		}

		
		
		internal static bool TryCreateFromInlineXbrl(XmlNode data,
			string inLineXBRLPrefix,
			string xbrliPrefix,
			Dictionary<string, string> namespaceused,
			ArrayList contexts, ArrayList units,
			bool isNumeric, Dictionary<string, Dictionary<decimal, ITupleSetChild>> tupleMarkups, 
			out MarkupProperty mp, ref ArrayList errors)
		{


			foreach (XmlAttribute attr in data.Attributes)
			{
				if (attr.Value.ToLower().Equals(DocumentBase.INLINE_XBRL_URI.ToLower()))
				{
					inLineXBRLPrefix = attr.LocalName;
				}
				else if (attr.Value.ToLower().Equals(DocumentBase.XBRL_INSTANCE_URL.ToLower()))
				{
					xbrliPrefix = attr.LocalName;
				}
			}
			mp = new MarkupProperty();
			if (data.Attributes[inLineXBRLPrefix + ":order"] != null && data.Attributes[inLineXBRLPrefix + ":tupleRef"] == null)
			{
				//these are tuple children that are nested inside tuples...
				//better evaluate it with the tuple.....
				return false;
			}




			/*
		<ix:nonFraction ix:name="us-gaap:SellingAndMarketingExpense" 
		 ix:format="commadot" ix:scale="6" decimals="-6" unitRef="usd" 
		 contextRef="c20080331_9_cik_00009869________">9,161
		</ix:nonFraction> */

			XmlAttribute nameAttr = data.Attributes[ inLineXBRLPrefix + ":name"];
			if (nameAttr == null)
			{
				nameAttr = data.Attributes["name"];
			}

			if (nameAttr == null)
			{
				errors.Add("Failed to parse " + data.OuterXml);
				return false;

			}

			// first get the name
			string[] nameParts = nameAttr.Value.Split(':');
			//TODO: need to parse this information
			//mp.elementNamespace = data.NamespaceURI;

			mp.elementPrefix = nameParts[0];
			mp.elementName = nameParts[1];
			mp.elementId = string.Format(DocumentBase.ID_FORMAT, nameParts[0], nameParts[1]);
			namespaceused.TryGetValue(mp.elementPrefix, out mp.elementNamespace);

			mp.element = new Node(new Element(mp.elementName, mp.elementId, false));
			mp.element.MyElement.HasCompleteTupleFamily = false;

			// try and get the context ref
			XmlAttribute contextAttr = data.Attributes[Instance.CONTEXT_REF];
			if (contextAttr == null)
			{
				contextAttr = data.Attributes[inLineXBRLPrefix + ":" + Instance.CONTEXT_REF];
			}
			if (contextAttr == null)
			{
				Common.WriteError("XBRLParser.Error.ElementMissingRequiredField", errors, Instance.CONTEXT_REF);
				return false;
			}

			string contextId = contextAttr.Value;

			// make sure we can find the real context
			int contextIndex = contexts.BinarySearch(contextId);
			if (contextIndex < 0)
			{
				Common.WriteError("XBRLParser.Error.ContextIdNotFound", errors, contextId);
				return false;
			}

			mp.SetContext((ContextProperty)contexts[contextIndex]);
			if (isNumeric)
			{
				// and get the unit
				XmlAttribute unitAttr = data.Attributes[Instance.UNIT_REF];
				if (unitAttr == null)
				{
					unitAttr = data.Attributes[inLineXBRLPrefix + ":" + Instance.UNIT_REF];
				}
				if (unitAttr != null)
				{
					string unitId = unitAttr.Value;

					// make sure we can find the real context
					int unitIndex = units.BinarySearch(unitId);
					if (unitIndex < 0)
					{
						Common.WriteError("XBRLParser.Error.UnitIdNotFound", errors, unitId);
						return false;
					}

					mp.SetUnit((UnitProperty)units[unitIndex]);

					// and precision
					XmlAttribute attr = data.Attributes[Precision.DECIMALS];
					if (attr == null)
					{
						attr = data.Attributes[xbrliPrefix + ":" + Precision.DECIMALS];
					}
					if (attr == null)
					{
						attr = data.Attributes[Precision.PRECISION];
					}

					if (attr == null)
					{
						attr = data.Attributes[xbrliPrefix + ":" + Precision.PRECISION];
					}
					if (attr != null)
					{
						Precision p = null;
						if (!Precision.TryCreateFromXml(attr, out p, ref errors))
						{
							return false;
						}

						mp.SetPrecision(p);
					}
				}


			}
			mp.markupData = data.InnerText;


			string formatvalue = string.Empty;
			XmlAttribute formatAttr = data.Attributes[inLineXBRLPrefix + ":format"];
			if (formatAttr != null)
			{
				formatvalue = formatAttr.Value;
				string[] split = formatvalue.Split(':');
				if (split.Length > 1)
				{
					formatvalue = split[1];
				}


			}

			if ( isNumeric )
			{

				int scaleVal = 0;
				bool isNegated = false;

				XmlAttribute scaleAttr = data.Attributes[inLineXBRLPrefix + ":scale"];
				if (scaleAttr != null)
				{
					int.TryParse(scaleAttr.Value, out scaleVal);


				}
				

				if (data.Attributes[inLineXBRLPrefix + ":sign"] != null)
				{
					isNegated = true;



				}

			

				if (!TryConvertInlineXbrlNonFraction(scaleVal, isNegated, formatvalue, ref  mp.markupData))
				{
					errors.Add("Failed to parse  element" + data.OuterXml);
					return false;

				}

				


			}

			if (data.Attributes[ inLineXBRLPrefix + ":order"] != null && data.Attributes[ inLineXBRLPrefix + ":tupleRef"] != null)
			{
				decimal order = 0;
				if (decimal.TryParse(data.Attributes[inLineXBRLPrefix + ":order"].Value, out order))
				{
					string tupleId = data.Attributes[inLineXBRLPrefix + ":tupleRef"].Value;

					Dictionary<decimal, ITupleSetChild> inner;
					if (!tupleMarkups.TryGetValue(tupleId, out inner))
					{
						inner = new Dictionary<decimal, ITupleSetChild>();
						tupleMarkups[tupleId] = inner;
					}

					inner[order] = mp;
				}
				else
				{

					errors.Add("Failed to parse ix:order information for  element" + data.OuterXml);
					return false;
				}
			}
			
			return true;
		}


		#region inline xbrl transformation rules....

		internal static bool TryConvertInlineXbrlNonFraction(int scale, bool isnegated, string formatToApply, ref string data)
		{
			bool ret = false;

			if (!string.IsNullOrEmpty(formatToApply))
			{
				string regExpMD = string.Empty;
				switch (formatToApply)
				{

					case "numcomma":
						regExpMD = @"\d+,(\d+)*";
						break;

					case "numcommadot":
						regExpMD = @"\d{1,3}(,\d{3,3})*(\.\d+)?";
						break;

					case "numdotcomma":
						regExpMD = @"\d{1,3}(\.\d{3,3})*(,\d+)?";
						break;

					case "numspacedot":
						regExpMD = @"\d{1,3}( \d{3,3})*(\.\d+)?";
						break;

					case "numspacecomma":
						regExpMD = @"\d{1,3}( \d{3,3})*(,\d+)?";
						break;



				}
				if (regExpMD.Length > 0)
				{
					Regex rg = new Regex(regExpMD);
					Match m = rg.Match(data);

					if (m.Value.Length > 0)
					{
						data = m.Value;
					}

				}

			}
			decimal val = 0;
			if (decimal.TryParse(data, System.Globalization.NumberStyles.Any,
				null, out val))
			{
				ret = true;
				if (isnegated)
				{
					val *= -1;
				}
				if (scale != 0)
				{
					val *= (decimal)Math.Pow(10, scale);
				}

				data = val.ToString();

			}


			return ret;
		}


		internal static bool TryConvertInlineXBRLDateFormats(string format, ref string data)
		{
			if (!string.IsNullOrEmpty(format))
			{
				string regExpMD = string.Empty;
				switch (format)
				{



					case "dateslashus":
						regExpMD = @"\d{1,2}/\d{1,2}/(\d|\d{2,2}|\d{4,4})";
						break;

					case "dateslasheu":
						regExpMD = @"\d{1,2}/\d{1,2}/(\d|\d{2,2}|\d{4,4})";
						break;

					case "datedotus":
						regExpMD = @"\d{1,2}\.\d{1,2}\.(\d|\d{2,2}|\d{4,4})";
						break;

					case "datedoteu":
						regExpMD = @"\d{1,2}\.\d{1,2}\.(\d|\d{2,2}|\d{4,4})";
						break;

					case "datelongus":
						regExpMD = @"(January|February|March|April|May|June|July|August|September|October|November|December) (\d|\d{2,2}), (\d{2,2}|\d{4,4})";
						break;

					case "datelonguk":
						regExpMD = @"(\d|\d{2,2}) (January|February|March|April|May|June|July|August|September|October|November|December) (\d{2,2}|\d{4,4})";
						break;
					case "dateshortus":
						regExpMD = @"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) (\d|\d{2,2}), (\d{2,2}|\d{4,4})";
						break;
					case "dateshortuk":
						regExpMD = @"(\d|\d{2,2}) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) (\d{2,2}|\d{4,4})";
						break;



				}
				if (regExpMD.Length > 0)
				{
					Regex rg = new Regex(regExpMD);
					Match m = rg.Match(data);

					if (m.Value.Length > 0)
					{
						data = m.Value;
					}

				}

			}

			return true;

		}
		#endregion
		#endregion

		#region xml helpers
		/// <summary>
		/// Extracts comments related to the element, if any exist.
		/// </summary>
		/// <param name="i">The index of the element node</param>
		/// <param name="nodes">All nodes for the parent</param>
		protected void AddXmlComments( int i, XmlNodeList nodes )
		{
			XmlNode childComment = nodes[i-1];
			if ( childComment is XmlComment )
			{
				if ( ((XmlComment)childComment).InnerText.IndexOf( "Document address" ) != -1 )
				{
					oldXmlComments.Add( childComment );
				}
			}

			if ( (i-2) > -1 )
			{
				childComment = nodes[i-2];
				if ( childComment is XmlComment )
				{
					if ( ((XmlComment)childComment).InnerText.IndexOf( "Tuple Set Name" ) != -1 )
					{
						oldXmlComments.Add( childComment );
					}
				}
			}
		}
		#endregion

		#region setters
		/// <summary>
		/// Sets the context reference and text properties of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="cp">The id of this <see cref="ContextProperty"/> will be 
		/// included in context text property.</param>
		public void SetContext( ContextProperty cp )
		{
			contextRef = cp;
			contextText = ( cp == null ) ? string.Empty : "context: " + cp.ContextID;
		}

		/// <summary>
		/// Sets the precision reference and text properties of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="pp">The precision type of this <see cref="Precision"/> will be 
		/// included in precision text property.</param>
		public void SetPrecision(Precision pp)
		{
			precisionDef = pp;
			precisionText = (pp == null) ? string.Empty : "precision: " + pp.PrecisionType.ToString();
		}

		/// <summary>
		/// Sets the element, element id, and element name properties of this <see cref="MarkupProperty"/> 
		/// using the properties of a given <see cref="Node"/>.
		/// </summary>
		/// <param name="n">The source of the property values.</param>
		public void SetNode( Node n )
		{
			if ( n != null )
			{
				this.element = n;
				this.elementId = n.MyElement.Id;
				this.elementName = n.MyElement.Name;
			}
		}

		/// <summary>
		/// Sets the unit reference and text properties of this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="up">The unit id of this <see cref="UnitProperty"/> will be 
		/// included in unit text property.</param>
		public void SetUnit(UnitProperty up)
		{
			this.unitRef = up;
			this.unitText = (up == null) ? string.Empty : "unit: " + up.UnitID;
		}
		#endregion

		#region validation

		/// <summary>
		/// Sets validation status (but only under certain conditions), appends error to list
		/// </summary>
		public void SetError(ValidationStatus validationStatus, ArrayList errorList, string error)
		{
			// don't change error to incomplete
			if(validationStatus == ValidationStatus.INCOMPLETE)
			{
				if(MarkupStatus != ValidationStatus.ERROR)
					MarkupStatus = validationStatus;
			}
			else
			{
				MarkupStatus = validationStatus;
			}

			if( (validationStatus != ValidationStatus.OK) && (errorList != null) && (error != null) && (error.Length > 0) )
				errorList.Add(error);
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="MarkupProperty"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="MarkupProperty"/>
		/// is to be compared.  Assumed to be a <see cref="MarkupProperty"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="MarkupProperty"/>.</returns>
		/// <remarks>The following <see cref="ContextProperty"/> properties are compared in order:
		/// <bl>
		/// <li>Element Id.</li>
		/// <li>Context reference.</li>
		/// <li>Unit ID.</li>
		/// <li>Tuple Set Name.</li>
		/// </bl>
		/// </remarks>
		public int CompareTo(object obj)
		{
			if ( obj is string ) return elementId.CompareTo( (string)obj );

			MarkupProperty mpObj = (MarkupProperty)obj;

			int nameCompare = 0;
			if ( elementId != null )
			{
				nameCompare = elementId.CompareTo( mpObj.elementId );
			}
			else
			{
				nameCompare = string.Compare( string.Empty, mpObj.elementId );
			}

			if ( nameCompare == 0 )
			{
				int contextCompare = contextRef.CompareValue( mpObj.contextRef );
				if (contextCompare == 0)
				{
					//also check units
					//BUG 1418 - make sure units are not null
					if (unitRef == null || unitRef.UnitID == null || mpObj.unitRef == null || 
						mpObj.unitRef.UnitID == null)
					{
						//and check tupleset names
						if (TupleSetName == null && (mpObj.TupleSetName == null || 
							mpObj.TupleSetName == string.Empty))
						{
							return contextCompare;
						}
						else if (TupleSetName == null && mpObj.TupleSetName != null)
						{
							//tupleSetNames not equal, so don't return 0
							return -1;
						}
						else
						{
							return TupleSetName.CompareTo(mpObj.TupleSetName);
						}
					}

                    // DE1433 we were just comparing the UnitID's here and not the complete value of the unitRef.
					int unitCompare = unitRef.CompareValue(mpObj.unitRef);
					if (unitCompare == 0)
					{
						//check tupleset names
						if (TupleSetName == null && (mpObj.TupleSetName == null || mpObj.TupleSetName == string.Empty))
						{
							return contextCompare;
						}
						else if (TupleSetName == null && mpObj.TupleSetName != null)
						{
							//tupleSetNames not equal, so don't return 0
							return -1;
						}
						else
						{
							if(TupleSetName.CompareTo(mpObj.TupleSetName) == 0)
							{
								/* BUG 1064 - we have 2 markups for the same tuple child in the same 
								 * tuple set, so compare their markup data.  If the data is the same
								 * in both markups then consider them equal, otherwise they are 
								 * different markups (multiple occurrences of the same child).  */
								return markupData.CompareTo(mpObj.markupData);
							}
							else
							{
								//tupleSetNames don't match
								return TupleSetName.CompareTo(mpObj.TupleSetName);
							}
						}
					}
					else
					{
						return unitCompare;
					}
				}
				else
				{
					return contextCompare;
				}
			}

			return nameCompare;
		}
		#endregion

		#region equals
		/// <summary>
		/// Serves as a hash function for this instance of <see cref="MarkupProperty"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="MarkupProperty"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="MarkupProperty"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="MarkupProperty"/>.  
		/// Assumed to be a <see cref="MarkupProperty"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="MarkupProperty"/>.</returns>
		/// <remarks>To be equal, the tuple set name, element id, and context reference 
		/// properties in the two <see cref="MarkupProperty"/> objects must be equal.</remarks>
		public override bool Equals(object obj)
		{
			if ( !( obj is MarkupProperty ) ) return false;

			MarkupProperty mp = (MarkupProperty)obj;

			if ( TupleSetName != null )
			{
				if ( mp.TupleSetName == null ) return false;
				if ( string.Compare( TupleSetName, mp.TupleSetName ) != 0 ) return false;
			}
			else if ( mp.TupleSetName != null && mp.TupleSetName != string.Empty ) 
			{
				return false;
			}

			if ( elementId.CompareTo( mp.elementId ) != 0 ) return false;
			if ( !contextRef.ValueEquals( mp.contextRef ) ) return false;

			return true;
		}

		#endregion

		#region virtuals

		/// <summary>
		/// Does nothing.  Method is designed to be overridden and implemented in 
		/// derived classes.
		/// </summary>
		/// <param name="elem">Not used in this class.</param>
		public virtual void AppendDisplayTypeAttributeInfo(  XmlElement elem )
		{
			//this method is overridden in the derived class implemented in
			//other Rivet applications as we need a new attribute defined inside the element
			//for xbrl based xml of folio data
		}
		#endregion


		#region Amount 
		/// <summary>
		/// Round the value defined in markupdata based on the precision defined for the
		/// markup...
		/// Pleae refer to the XBRL 2.1 Spec to understand the logic implemented.
		/// Sample cases are also provided in the code below.
		/// </summary>
		/// <returns></returns>
		public decimal GetRoundedMarkedupAmount()
		{
			decimal ret = 0;

			if (decimal.TryParse(this.markupData, System.Globalization.NumberStyles.Any,
				null, out ret))
			{
				//we might have to apply precision info... to get the 
				//exact amount that calculation would need.

                if (this.precisionDef != null && !this.precisionDef.FaceValue )
				{

					int digits = precisionDef.NumberOfDigits;
					if (precisionDef.PrecisionType == Precision.PrecisionTypeCode.Precision)
					{
						digits = digits * -1;
					}

					/*
					 decimals="2"
					The value of the numeric fact is known to be correct to 2 decimal places.
 
					decimals="-2"
					The value of the numeric fact is known to be correct to –2 
					decimal places, i.e. all digits to the left of the hundreds digit are accurate.
 
					Rounding needs to be applied on top of it....
					123456.789012 correct to n decimal places
 
					n=-3 =>123000
					n=-2 =>123500
					n=0 =>123457
					n=2 =>123456.79
					n=6 =>123456.789012
					*/
 
 
					ret = Math.Round((ret * (decimal)(Math.Pow(10, digits))), 0, MidpointRounding.AwayFromZero) / (decimal)Math.Pow(10, digits);
				}
			}


			return ret;
		}


        /// <summary>
        /// based on the precision and scale used the markup could be more precise than the rounded value
        /// and this means the instance document is reporting a more precise value than what calculation would use..
        /// </summary>
        /// <returns></returns>
        public bool IsMarkupMorePreciseThanRoundedValue()
        {
            if (unitRef != null && precisionDef != null)
            {
                if (precisionDef.FaceValue  ) return false;

                //if (unitRef.Scale > 0 &&  precisionDef.NumberOfDigits < 0 
                //    &&   unitRef.Scale >= (precisionDef.NumberOfDigits*-1)  )
                //{
                //    //since scale is atleast equal to prec the rounded number can never be more precise than
                //    // the reported number...
                //    return false;
                //}

                int digits = precisionDef.NumberOfDigits;
                if (precisionDef.PrecisionType == Precision.PrecisionTypeCode.Precision)
                {
                    digits = digits * -1;
                }

               

                 decimal val = 0;

                 if (decimal.TryParse(this.markupData, System.Globalization.NumberStyles.Any,
                     null, out val))
                 {

                     /*
                      decimals="2"
                     The value of the numeric fact is known to be correct to 2 decimal places.
 
                     decimals="-2"
                     The value of the numeric fact is known to be correct to –2 
                     decimal places, i.e. all digits to the left of the hundreds digit are accurate.
 
                     Rounding needs to be applied on top of it....
                     123456.789012 correct to n decimal places
 
                     n=-3 =>123000
                     n=-2 =>123500
                     n=0 =>123457
                     n=2 =>123456.79
                     n=6 =>123456.789012
                     */


                     decimal roundedNumber = Math.Round((val * (decimal)(Math.Pow(10, digits))), 0, MidpointRounding.AwayFromZero) / (decimal)Math.Pow(10, digits);
                     decimal diff =  Math.Abs( Math.Round(val - roundedNumber, 4));

                     if (diff > 0.0001m)
                     {
                         return true;
                     }
                     
                 }

            }

            return false;

          
        }
        /// <summary>
        /// based on the precision used in the markup, this determines if there is a level of precision that requires 
        /// digits be shown to the right of the decimal point.
        /// </summary>
        /// <returns></returns>
        public bool HasDecimalPrecision()
        {
            if(this.precisionDef == null ) 
                return false; //No precision def
                
            if(this.precisionDef.FaceValue == true)
                return false;   //Precision def says to use the face value, ignore oher properties

            if(this.precisionDef.PrecisionType != Precision.PrecisionTypeCode.Decimals)
                return false;   //Precision Type is no Decimals

            if (this.precisionDef.NumberOfDigits == 0)
                return false;   //Precision value is 0 (Whole Numbers) so no digits to the right of the decimal

            return true;
        }

        public bool HasScale()
        {
            if (this.unitRef == null)
                return false;  //No Unit Reference

            if (this.unitRef.Scale == 0)
                return false;  //No Scale applied

            return true;
        }

		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IMarkupItemDescription
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="parentKey"></param>
		/// <returns></returns>
		string GetElementDescription( Node node, object parentKey);
	}
}
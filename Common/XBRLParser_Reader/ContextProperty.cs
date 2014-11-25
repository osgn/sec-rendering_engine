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
using System.Text;
using System.Collections;
using System.Xml.Serialization;

using Aucent.MAX.AXE.Common.Data;
using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// ContextProperty
	/// </summary>
    [Serializable]
	public class ContextProperty : IComparable
	{
		#region constants
		/// <summary>
		/// The name of the XBRL context element.
		/// </summary>
		public const string		CONTEXT		= "context";		
		
		private const string	ENTITY		= "entity";
		private const string	IDENTIFIER	= "identifier";
		private const string	PERIOD		= "period";
		private const string	INSTANT		= "instant";
		private const string	FOREVER		= "forever";
		private const string	START_DATE	= "startDate";
		private const string	END_DATE	= "endDate";
		
		private const string	ID_ATTR		= "id";
		private const string	SCHEME_ATTR	= "scheme";

		private const string	SCENARIO	= "scenario";
		private const string SEGMENT = "segment";
		private const string DIMENSION = "dimension";
		private const string EXPLICITMEMBER = "explicitMember";


		#endregion

		#region properties

		private XmlNode xmlContext = null;
		private int UseCount = 0;

		/// <summary>
		/// A collection of <see cref="ParserMessage"/> that is the errors that have occurred
		/// while loading and parsing this document.
		/// </summary>
		private ArrayList ErrorList = new ArrayList();

		private string TimePeriodKey
		{
			get { return string.Format( "{0}", PeriodStartDate ); }
		}

		/// <summary>
		/// There are 4 major parts in the ContextProperties:
		/// Entity (required)
		/// Period (required)
		/// Segment (optional)
		/// Scenario (optional)
		/// 
		/// Naming convention -- 
		/// xxxxxxID: Unique name that Identifies xxxxxx and must start with a letter
		/// xxxxxxSchema: location of the namespace
		/// xxxxxxValueType: defines a TYPE the value belongs to (for example, SegmentValueType might be "State or Province")
		/// xxxxxxValue: defines a value (for example, SegmentValue might be "CO")
		/// xxxxxxNamespace:specifies name of the schema
		/// 
		/// </summary>
		public string ContextID = string.Empty;

		/// <summary> 
		/// The schema (URL/URI) identifying the entity associated with this context.
		/// </summary>
		/// <remarks>
		/// Example:
		/// <entity>
		///		<identifier scheme="http://www.softwareag.com/spain/">Software AG (The XML Company)</identifier>
		/// </entity>
		/// </remarks>
		public string EntitySchema = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string EntityValue = string.Empty;

		/// <summary>
		/// A display name used when building HTML version of instance document.
		/// This is not to be serialized, so we are ignoring it with XmlIgnore attribute.
		/// </summary>
		public string PeriodDisplayName = string.Empty;

		/// <summary>
		/// The period type of this context.
		/// </summary>
		/// <remarks>
		/// If the PeriodType == Instant, only <see cref="PeriodStartDate"/> is needed
		/// If the PeriodType == Forever, ignore <see cref="PeriodStartDate"/> and <see cref="PeriodEndDate"/>.
		/// Example:
		/// <period>
		///     <instant>2002-12-31</instant>
		/// </period>
		/// Date format is YYYY-MM-DD
		/// </remarks>
		public Element.PeriodType PeriodType = Element.PeriodType.na;

		/// <summary>
		/// The period start date for this context.
		/// </summary>
		public DateTime PeriodStartDate = DateTime.MinValue;

		/// <summary>
		/// The period end date for this context.
		/// </summary>
		public DateTime PeriodEndDate = DateTime.MinValue;

		/// <summary>
		/// A collection of <see cref="Segment"/> associated with this context.
		/// </summary>
		public ArrayList Segments = new ArrayList();

		/// <summary>
		/// A collection of <see cref="Scenario"/> associated with this context.
		/// </summary>
		public ArrayList Scenarios = new ArrayList();

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="ContextProperty"/>.
		/// </summary>
		public ContextProperty()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="ContextProperty"/>.
		/// </summary>
		/// <param name="contextID">The context id to be assigned to the newly created <see cref="ContextProperty"/>.</param>
		public ContextProperty(string contextID)
		{
			this.ContextID = contextID;
		}

		internal static bool TryCreateFromXml( XmlNode node, XmlNamespaceManager theManager, out ContextProperty cp, ref ArrayList errors )
		{
			if ( errors == null )
			{
				errors = new ArrayList();
			}

			cp = new ContextProperty();
			cp.ErrorList = errors;

			cp.ContextID = node.Attributes[ ID_ATTR ].Value;

			XmlNode entity = node.SelectSingleNode( "./link2:" + ENTITY, theManager );
			if ( entity == null )
			{
				if ( ( entity = node.SelectSingleNode( ENTITY ) ) == null )
				{
					Common.WriteError( "XBRLParser.Error.MissingRequiredField", errors, "Entity" );
					return false;
				}
			}

			XmlNode identifier = entity.SelectSingleNode( "./link2:" + IDENTIFIER, theManager );
			if ( identifier == null )
			{
				if ( ( identifier = entity.SelectSingleNode( "./" + IDENTIFIER ) ) == null )
				{
					Common.WriteError( "XBRLParser.Error.MissingRequiredField", errors, "identifier" );
					return false;
				}
			}

			cp.EntitySchema = identifier.Attributes[ SCHEME_ATTR ].Value;
			cp.EntityValue = identifier.InnerXml;

			XmlNode segments = entity.SelectSingleNode( "./link2:" + SEGMENT, theManager );

			if ( segments == null )
			{
				segments = entity.SelectSingleNode( "./" + SEGMENT );
			}

			if ( segments != null )
			{
				foreach ( XmlNode segment in segments.ChildNodes )
				{
					if (segment is XmlComment) continue;

					bool createSegment = true;
					if (segment.LocalName.ToLower().Equals(EXPLICITMEMBER.ToLower()) &&
						segment.NamespaceURI.Equals( DocumentBase.XBRLDI_URI) )
					{
						//we have a dimension info
						string dimensionId=null;
						string valueId = segment.InnerXml.Replace( ":", "_");
						foreach (XmlAttribute attr in segment.Attributes)
						{
							if (attr.Name.Equals(DIMENSION))
							{
								dimensionId = attr.Value.Replace( ":", "_");
								break;
							}
						}
						if (dimensionId != null)
						{
							Segment seg = new Segment(string.Empty, valueId, dimensionId, DocumentBase.XBRLDI_PREFIX, segment.NamespaceURI);
							ContextDimensionInfo di = new ContextDimensionInfo();
							di.dimensionId = dimensionId;
							di.Id = valueId;
							seg.DimensionInfo = di;

							cp.AddSegment(seg);
							createSegment = false;
						}

					}

					if (createSegment)
					{
						cp.AddSegment(segment.Prefix, segment.InnerXml, segment.LocalName, segment.NamespaceURI);
					}
				}
			}

			XmlNode scenarios = node.SelectSingleNode( "./link2:" + SCENARIO, theManager );
			if ( scenarios == null )
			{
				scenarios = node.SelectSingleNode( "./" + SCENARIO );
			}

			if ( scenarios != null )
			{
				foreach ( XmlNode scenario in scenarios )
				{

					bool createScenario = true;
					if (scenario.LocalName.ToLower().Equals(EXPLICITMEMBER.ToLower()) &&
						scenario.NamespaceURI.Equals(DocumentBase.XBRLDI_URI))
					{
						//we have a dimension info
						string dimensionId = null;
						string valueId = scenario.InnerXml.Replace(":", "_");
						foreach (XmlAttribute attr in scenario.Attributes)
						{
							if (attr.Name.Equals(DIMENSION))
							{
								dimensionId = attr.Value.Replace(":", "_");
								break;
							}
						}
						if (dimensionId != null)
						{
							Scenario seg = new Scenario(string.Empty, valueId, dimensionId, DocumentBase.XBRLDI_PREFIX, scenario.NamespaceURI);
							ContextDimensionInfo di = new ContextDimensionInfo();
							di.dimensionId = dimensionId;
							di.Id = valueId;
							seg.DimensionInfo = di;

							cp.AddScenario(seg);
							createScenario = false;
						}

					}

					if (createScenario)
					{

						cp.AddScenario(scenario.Prefix, scenario.InnerXml, scenario.LocalName, scenario.NamespaceURI);
					}
				}
			}

			// lastly, repopulate the time period
			XmlNode timePeriod = node.SelectSingleNode( "./link2:" + PERIOD, theManager );
			if ( timePeriod == null )
			{
				timePeriod = node.SelectSingleNode( "./" + PERIOD );
				if ( timePeriod == null )
				{
					Common.WriteError( "XBRLParser.Error.MissingRequiredField", errors, "TimePeriod" );
					return false;
				}
				else
				{
					if ( !cp.ParsePeriods( timePeriod, theManager ) )
					{
						return false;
					}
				}
			}
			else
			{
				if ( !cp.ParsePeriods( timePeriod, theManager ) )
				{
					return false;
				}
			}

			return cp.IsValid();
		}

		private bool ParsePeriods( XmlNode timePeriod, XmlNamespaceManager theManager )
		{
			XmlNode start = (XmlElement)timePeriod.SelectSingleNode( "./link2:" + START_DATE, theManager );
			if ( start == null )
			{
				start = (XmlElement)timePeriod.SelectSingleNode( "./" + START_DATE );
			}

			if ( start != null )
			{
				PeriodType = Element.PeriodType.duration;
				PeriodStartDate = DateTime.Parse( start.InnerXml );

				XmlNode end = (XmlElement)timePeriod.SelectSingleNode( "./link2:" + END_DATE, theManager );
				if ( end == null )
				{
					end = (XmlElement)timePeriod.SelectSingleNode( "./" + END_DATE );
				}

				PeriodEndDate = DateTime.Parse( end.InnerXml );
			}
			else
			{
				XmlNode instant = timePeriod.SelectSingleNode( "./link2:" + INSTANT, theManager );
				if ( instant == null )
				{
					instant = timePeriod.SelectSingleNode( "./" + INSTANT );
				}

				if ( instant != null )
				{
					PeriodType = Element.PeriodType.instant;
					PeriodStartDate = DateTime.Parse( instant.InnerXml );
				}
				else
				{
					//it's got to be forever
					if ( timePeriod.InnerXml.IndexOf( FOREVER ) == -1 )
					{
						Common.WriteError( "XBRLParser.Error.MissingRequiredField", ErrorList, "TimePeriod detail" );
						return false;
					}
					else
					{
						PeriodType = Element.PeriodType.forever;
					}
				}
			}

			return true;
		}
		#endregion

		#region instance doc xml stuff
		private void RemoveXmlNode()
		{
			if ( --UseCount == 0 && xmlContext != null )
			{
				xmlContext.ParentNode.RemoveChild( xmlContext );
				xmlContext = null;
			}
		}

		private void IncrementInUseCount()
		{
			++UseCount;
		}
		#endregion

		#region Add Segment

		/// <summary>
		/// Creates and adds a <see cref="Segment"/> to this <see cref="ContextProperty"/>'s segments collection.
		/// </summary>
		/// <param name="Namespace">The namespace to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="Value">The name and value name to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="ValueType">The value type to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="Schema">The schema to be assigned to the new <see cref="Segment"/>.</param>
		public void AddSegment( string Namespace, string Value, string ValueType, string Schema )
		{
			Segments.Add( new Segment( Value, Value, ValueType, Namespace, Schema ) );
		}

		/// <summary>
		/// Add a parameter-supplied <see cref="Segment"/> to this <see cref="ContextProperty"/>'s segments collection.
		/// </summary>
		/// <param name="s">The <see cref="Segment"/> to be added.</param>
		public void AddSegment(Segment s)
		{
			Segments.Add( s );
		}
		#endregion

		#region Add Scenario
		/// <summary>
		/// Creates and adds a <see cref="Scenario"/> to this <see cref="ContextProperty"/>'s scenarios collection.
		/// </summary>
		/// <param name="Namespace">The namespace to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="Value">The name and value name to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="ValueType">The value type to be assigned to the new <see cref="Segment"/>.</param>
		/// <param name="Schema">The schema to be assigned to the new <see cref="Segment"/>.</param>
		public void AddScenario(string Namespace, string Value, string ValueType, string Schema)
		{
			Scenarios.Add( new Scenario( Value, Value, ValueType, Namespace, Schema ) );
		}

		/// <summary>
		/// Add a parameter-supplied <see cref="Scenario"/> to this <see cref="ContextProperty"/>'s segments collection.
		/// </summary>
		/// <param name="s">The <see cref="Scenario"/> to be added.</param>
		public void AddScenario(Scenario s)
		{
			Scenarios.Add( s );
		}
		#endregion

		#region to xml
		/// <summary>
		/// creates an XML element for the context.....
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="root"></param>
		/// <param name="taxonomies"></param>
		/// <returns></returns>
		public XmlElement AppendWithNamespaces(XmlDocument doc, XmlElement root, Taxonomy[] taxonomies)
		{
			return WriteContextInfo(doc, root, true, taxonomies);
		}

		public XmlElement Append(XmlDocument doc, XmlElement root, Taxonomy[] taxonomies)
		{
			return WriteContextInfo(doc, root, false, taxonomies);
		}

		/// <summary>
		/// Creates and XML/XBRL context <see cref="XmlElement"/> based on this <see cref="ContextProperty"/> and supplied parameters.
		/// </summary>
		/// <param name="doc">The <see cref="XmlDocument"/> that is the context for the new context <see cref="XmlElement"/>.</param>
		/// <param name="root">The document-level root element.</param>
		/// <param name="applyNamespaces">If true, the new context <see cref="XmlElement"/> will be created with namespace qualification.</param>
		/// <param name="taxonomies">A collection of <see cref="Taxonomy"/> used in writing segment and scenario information.</param>
		/// <returns>The newly created <see cref="XmlElement"/>.</returns>
		virtual protected XmlElement WriteContextInfo(XmlDocument doc, XmlElement root, bool applyNamespaces, Taxonomy[] taxonomies)
		{
			XmlElement context = applyNamespaces ? doc.CreateElement( CONTEXT, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( CONTEXT );
			context.SetAttribute( ID_ATTR, this.ContextID );

			xmlContext = context;
			++this.UseCount;

			XmlElement entity = applyNamespaces ? doc.CreateElement( ENTITY, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( ENTITY );
			context.AppendChild( entity );

			XmlElement identifier = applyNamespaces ? doc.CreateElement( IDENTIFIER, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( IDENTIFIER );
			entity.AppendChild( identifier );
			identifier.SetAttribute( SCHEME_ATTR, EntitySchema );
			XmlText idText= doc.CreateTextNode( EntityValue );
			identifier.AppendChild( idText );

			if ( Segments.Count > 0 )
			{
				WriteSegments(entity, root, doc, applyNamespaces, taxonomies);
			}

			XmlElement period = applyNamespaces ? doc.CreateElement( PERIOD, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( PERIOD );
			context.AppendChild( period );

			switch ( PeriodType )
			{
				case Element.PeriodType.instant:
					XmlElement inner = applyNamespaces ? doc.CreateElement( INSTANT, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( INSTANT );
					period.AppendChild( inner );

					XmlText instText = doc.CreateTextNode( PeriodStartDate.ToString( "yyyy-MM-dd" ) );
					inner.AppendChild( instText );
					break;

				case Element.PeriodType.duration:
					XmlElement start = applyNamespaces ? doc.CreateElement( START_DATE, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( START_DATE );
					period.AppendChild( start );

					XmlText startText = doc.CreateTextNode( PeriodStartDate.ToString( "yyyy-MM-dd" ) );
					start.AppendChild( startText );

					XmlElement end = applyNamespaces ? doc.CreateElement( END_DATE, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( END_DATE );
					period.AppendChild( end );

					XmlText endText = doc.CreateTextNode( PeriodEndDate.ToString( "yyyy-MM-dd" ) );
					end.AppendChild( endText );
					break;

				case Element.PeriodType.forever:
					XmlElement forever = applyNamespaces ? doc.CreateElement( FOREVER, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( FOREVER );
					period.AppendChild( forever );
					break;

				default:
					throw new ApplicationException( "Period type None and Period Type Duration are not supported" );
			}

			if ( Scenarios.Count > 0 )
			{
				WriteScenarios(context, root, doc, applyNamespaces, taxonomies);
			}

			return context;

		}


		internal void WriteSegments( XmlElement entityElem, XmlElement root, XmlDocument doc,
			bool applyNamespaces, Taxonomy[] taxonomies)
		{
			XmlElement seg = applyNamespaces ? doc.CreateElement( SEGMENT, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( SEGMENT );
			entityElem.AppendChild( seg );

			foreach ( Segment s in Segments )
			{
				if (s.DimensionInfo == null)
				{
					XmlElement innerSeg = doc.CreateElement(s.Namespace, s.ValueType, s.Schema);
					seg.AppendChild(innerSeg);
					innerSeg.InnerText = s.ValueName;

					root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, s.Namespace), s.Schema);
				}
				else
				{
					WriteDimensionInfo(seg, s.DimensionInfo, root, doc, taxonomies);
				}
			}
		}


		#region write dimension info
		private void WriteDimensionInfo(XmlElement parent, ContextDimensionInfo cdi,
			XmlElement root, XmlDocument doc, Taxonomy[] taxonomies)
		{
			if (taxonomies != null)
			{
				root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, DocumentBase.XBRLDI_PREFIX), DocumentBase.XBRLDI_URI);

				//Add the dimension TI to the root attributes...
				Element dimensionEle;
				TaxonomyItem dimensionTI = GetMemberTaxonomyItem(cdi.dimensionId,
					taxonomies, out dimensionEle);
				if (dimensionTI != null)
				{
					root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, dimensionTI.Namespace), dimensionTI.WebLocation);
				}
				else
				{
					//error
					return;
				}
				Element memberEle;
				TaxonomyItem memberTI = GetMemberTaxonomyItem(cdi.Id,
					taxonomies, out memberEle);
				if (memberTI != null)
				{
					root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, memberTI.Namespace), memberTI.WebLocation);
				}
				else
				{
					//error 
					return;
				}
				//need to find the member taxonomy item
				XmlElement innerEle = doc.CreateElement(DocumentBase.XBRLDI_PREFIX,  EXPLICITMEMBER, DocumentBase.XBRLDI_URI);
				innerEle.SetAttribute(DIMENSION, string.Format(DocumentBase.NAME_FORMAT, dimensionTI.Namespace, dimensionEle.Name));
				parent.AppendChild(innerEle);
				innerEle.InnerText = string.Format(DocumentBase.NAME_FORMAT, memberTI.Namespace, memberEle.Name);

			}
			else
			{
				//error
			}
		}

		//get the member and the taxonomy item..
		private TaxonomyItem GetMemberTaxonomyItem(string id, Taxonomy[] taxonomies, out Element ele)
		{
			ele = null;
			foreach (Taxonomy tax in taxonomies)
			{
				ele = tax.AllElements[id] as Element;
				if (ele != null)
				{
					return tax.TaxonomyItems[ele.TaxonomyInfoId];
				}
			}

			return null;
		}
		#endregion

		internal void WriteScenarios( XmlElement contextElem, XmlElement root, XmlDocument doc,
			bool applyNamespaces, Taxonomy[] taxonomies)
		{
			XmlElement scen = applyNamespaces ? doc.CreateElement( SCENARIO, DocumentBase.XBRL_INSTANCE_URL ) : doc.CreateElement( SCENARIO );
			contextElem.AppendChild( scen );

			foreach ( Scenario s in Scenarios )
			{
				if (s.DimensionInfo == null)
				{
					XmlElement innerScen = doc.CreateElement(s.Namespace, s.ValueType, s.Schema);
					scen.AppendChild(innerScen);
					innerScen.InnerText = s.ValueName;

					root.SetAttribute(string.Format(DocumentBase.NAME_FORMAT, DocumentBase.XMLNS, s.Namespace), s.Schema);
				}
				else
				{
					WriteDimensionInfo(scen, s.DimensionInfo, root, doc, taxonomies);

				}
			}
		}

		#endregion

		#region validation
		/// <summary>
		/// This method validate the context of the context ref
		/// If any of the required properties didn't get set, 
		/// return false and set the InvalidMessage with proper message
		/// </summary>
		/// <returns></returns>
		private bool IsValid()
		{
			ErrorList.Clear();

			bool valid = true;

			if (ContextID.Length == 0 )
			{
				Common.WriteError( "XBRLParser.Error.MissingRequiredField", ErrorList, "ContextID" );
				valid =  false;
			}

			if ( PeriodType == Element.PeriodType.na )
			{
				Common.WriteError( "XBRLParser.Error.MissingRequiredField", ErrorList, "PeriodTypeCode" );
				valid =  false;
			}

			if (this.EntitySchema.Length == 0 )
			{
				Common.WriteError( "XBRLParser.Error.MissingRequiredField", ErrorList, "EntitySchema" );
				valid =  false;
			}
				
			if ( this.EntityValue.Length == 0)
			{
				Common.WriteError( "XBRLParser.Error.MissingRequiredField", ErrorList, "EntityValue" );
				valid =  false;
			}

			if (PeriodType == Element.PeriodType.instant && this.PeriodStartDate == DateTime.MinValue)
			{
				Common.WriteError( "XBRLParser.Error.InvalidInstantPeriodStart", ErrorList );
				valid =  false;
			}

			if (PeriodType == Element.PeriodType.duration && (PeriodStartDate == DateTime.MinValue || PeriodEndDate == DateTime.MinValue))
			{
				Common.WriteError( "XBRLParser.Error.InvalidDurationStartOrEnd", ErrorList );
				valid =  false;
			}
			
			return valid;
		}

		#endregion

		internal bool HasDimensionInfo()
		{
			foreach (Segment seg in Segments)
			{
				if (seg.DimensionInfo != null) return true;
			}

			foreach (Scenario sce in Scenarios)
			{
				if (sce.DimensionInfo != null) return true;
			}

			return false;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of the period of this <see cref="ContextProperty"/>.
		/// </summary>
		/// <returns>The <see cref="String"/> representation.</returns>
		/// <remarks>
		/// Public for use by Dragon View.
		/// </remarks>
		public string GetPeriodString()
		{
			string ret = string.Empty;
			switch(this.PeriodType)
			{
				case Element.PeriodType.duration:
					ret = this.PeriodStartDate.ToShortDateString() + " - " + this.PeriodEndDate.ToShortDateString();
					break;
				case Element.PeriodType.instant:
					ret = this.PeriodStartDate.ToShortDateString();
					break;
				case Element.PeriodType.forever:
					ret = "Forever";
					break;
				case Element.PeriodType.na:
					ret = "None";
					break;
			}
			return ret;
		}

		#region value equals
		/// <summary>
		/// Determines if the "value" of a parameter supplied <see cref="ContextProperty"/> is equal to 
		/// this <see cref="ContextProperty"/>.
		/// </summary>
		/// <param name="cp">The <see cref="ContextProperty"/> to which this <see cref="ContextProperty"/> 
		/// is to be compared.</param>
		/// <returns>True if <paramref name="cp"/> is equal to this <see cref="ContextProperty"/>.</returns>
		/// <remarks>Comparison is done by calling <see cref="CompareValue"/>.  See <see cref="CompareValue"/> for 
		/// details.</remarks>
		virtual public bool ValueEquals(ContextProperty cp)
		{
			return CompareValue(cp) == 0;
		}

		/// <summary>
		/// Compares the "value" of a parameter supplied <see cref="ContextProperty"/> to 
		/// this <see cref="ContextProperty"/>.
		/// </summary>
		/// <param name="cp">The <see cref="ContextProperty"/> to which this <see cref="ContextProperty"/> 
		/// is to be compared.</param>
		/// <returns>
		/// <bl>
		/// <li>Less than zero if this <see cref="ContextProperty"/> is less than <paramref name="cp"/>.</li>
		/// <li>Zero if this <see cref="ContextProperty"/> is equal to <paramref name="cp"/>.</li>
		/// <li>Greater than zero if <paramref name="cp"/> is greater than this <see cref="ContextProperty"/>.</li>
		/// </bl>
		/// </returns>
		/// <remarks>The following <see cref="ContextProperty"/> properties are compared in order:
		/// <bl>
		/// <li>Entity schema.</li>
		/// <li>Entity value.</li>
		/// <li>Period type.</li>
		/// <li>Period start date.</li>
		/// <li>Period end date, if applicable.</li>
		/// <li>Number of segments.</li>
		/// <li>Number of scenarios.</li>
		/// </bl>
		/// </remarks>
		virtual public int CompareValue(ContextProperty cp)
		{
			int ret = this.EntitySchema.CompareTo( cp.EntitySchema );
			if( ret != 0 )
				return ret;

			ret = this.EntityValue.CompareTo( cp.EntityValue ) ;
			if( ret != 0 )
				return ret;

			ret = this.PeriodType.CompareTo( cp.PeriodType );
			if( ret != 0 )
				return ret;

			switch( this.PeriodType )
			{
				case Element.PeriodType.duration:
					ret = PeriodStartDate.Date.CompareTo( cp.PeriodStartDate.Date ) ;
					if( ret != 0 )
						return ret;

					ret = PeriodEndDate.Date.CompareTo(cp.PeriodEndDate.Date ) ;
					if( ret != 0 )
						return ret;
					break;

				case Element.PeriodType.instant:
					ret = PeriodStartDate.Date.CompareTo(cp.PeriodStartDate.Date );
					if( ret != 0 )
						return ret;
					break;
			};

			ret = Segments.Count.CompareTo(cp.Segments.Count );
			if( ret != 0 )
				return ret;
			if ( Segments.Count > 0 )
			{
				//seq of segments might not be the same..
				//need to sort it to be the same
				if (Segments.Count > 1)
				{
					Segments.Sort();
					cp.Segments.Sort();
				}

				for ( int i=0; i < Segments.Count; ++i )
				{
					ret = ((Segment)Segments[i]).CompareValue( cp.Segments[i] as Segment );
					if( ret != 0 )
						return ret;
				}
			}

			ret = Scenarios.Count.CompareTo(cp.Scenarios.Count );
			if( ret != 0 )
				return ret;
			if ( Scenarios.Count > 0 )
			{
				//seq of Scenarios might not be the same..
				//need to sort it to be the same
				if (Scenarios.Count > 1)
				{
					Scenarios.Sort();
					cp.Scenarios.Sort();

				}
				for ( int i=0; i < Scenarios.Count; ++i )
				{
					ret = ((Scenario)Scenarios[i]).CompareValue( cp.Scenarios[i] as Scenario) ;
					if( ret != 0 )
						return ret;
				}
			}

			return ret;

		}
		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="ContextProperty"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="ContextProperty"/>
		/// is to be compared.  Assumed to be a <see cref="ContextProperty"/> or <see cref="String"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="ContextProperty"/>.</returns>
		/// <remarks>If <paramref name="obj"/> is a string, the <see cref="ContextID"/> of this <see cref="ContextProperty"/>
		///  is compared to <paramref name="obj"/>.  Otherwise, <paramref name="obj"/>'s <see cref="ContextID"/> is compared
		///  to the <see cref="ContextID"/> of this <see cref="ContextProperty"/>.</remarks>
		public int CompareTo(object obj)
		{
			if ( obj is string ) return ContextID.CompareTo( (string)obj);

			return ContextID.CompareTo( ((ContextProperty)obj).ContextID );
		}

        public bool IsEmpty(ContextProperty cp)
        {
            bool objectIsEmpty = true;

            if (cp.ContextID != string.Empty)
                objectIsEmpty = false;
            else if (cp.EntitySchema != string.Empty)
                objectIsEmpty = false;
            else if (cp.EntityValue != string.Empty)
                objectIsEmpty = false;
            else if (cp.PeriodDisplayName != string.Empty)
                objectIsEmpty = false;
            else if (cp.PeriodEndDate != DateTime.MinValue)
                objectIsEmpty = false;
            else if (cp.PeriodStartDate != DateTime.MinValue)
                objectIsEmpty = false;
            else if (cp.PeriodType != Element.PeriodType.na)
                objectIsEmpty = false;
            else if (cp.Scenarios != null && cp.Scenarios.Count > 0)
                objectIsEmpty = false;
            else if (cp.Segments != null && cp.Segments.Count > 0)
                objectIsEmpty = false;

            return objectIsEmpty;
        }

		#endregion
	}

	/// <summary>
	/// Provides method and properties to compare <see cref="ContextProperty"/> objects.
	/// </summary>
	public class ContextComparer : IComparer
	{
		#region IComparer Members

		/// <summary>
		/// Compare two <see cref="Object"/>s, assumed to be <see cref="ContextProperty"/>s.
		/// </summary>
		/// <param name="x">The first <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="ContextProperty"/>.</param>
		/// <param name="y">The second <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="ContextProperty"/>.</param>
		/// <returns><bl>
		/// <li>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>.</li>
		/// <li>Zero if <paramref name="x"/> is equal to <paramref name="y"/>.</li>
		/// <li>Greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.</li>
		/// </bl></returns>
		/// <remarks>
		/// <paramref name="x"/> and <paramref name="y"/> are compared by first comparing their 
		/// period start dates.  If their period start dates are equal, their period end dates 
		/// are compared.  They will be equal if their period start and period end dates are equal.
		/// </remarks>
		public int Compare(object x, object y)
		{
			ContextProperty cpX = (ContextProperty)x;
			ContextProperty cpY = (ContextProperty)y;

			if (cpX.PeriodStartDate.Equals (cpY.PeriodStartDate))//compare the end date
			{
				DateTime XEndDate = cpX.PeriodEndDate;
				DateTime YEndDate = cpY.PeriodEndDate;

				if (XEndDate == new DateTime (1,1,1))
					XEndDate = new DateTime(2999, 12,31);

				if (YEndDate == new DateTime (1,1,1))
					YEndDate = new DateTime(2999, 12,31);

				return XEndDate.CompareTo (YEndDate);
			}
			else
			{
				return cpX.PeriodStartDate.CompareTo (cpY.PeriodStartDate);
			}
				
			//return ((ContextProperty)x).TimePeriodKey.CompareTo( ((ContextProperty)y).TimePeriodKey );
		}

		#endregion

	}

}
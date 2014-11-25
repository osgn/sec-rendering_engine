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

namespace Aucent.MAX.AXE.XBRLParser
{
	#region ReferenceElementsEnum 
	/// <summary>
	/// XBRL International defined valid reference element parts.
	/// </summary>
	/// <remarks>Per FRTA-CR5-2005-01-29-redlined-to-2005-01-22.doc, Section 2.1.21; Table 3. Defined reference parts.
	/// </remarks>
	[Serializable]
	public enum ReferenceElementsEnum
	{
		// Keep in the same sequence as ReferenceExtender.TranslatedReferenceElementsEnum so index can be used for cross-reference

		/// <summary>
		/// Publisher of the reference material, such as SEC, FASB, or AICPA.
		/// </summary>
		Publisher,

		/// <summary>
		/// Name refers to the specific publication. For example, “Statement of Financial Standards”, “Statement of Position” or “IFRS”. It does not include the number.
		/// </summary>
		Name,

		/// <summary>
		/// Number is used to record the actual number of the specific publication. For example, the number for FAS 133 would be 133.
		/// </summary>
		Number,

		/// <summary>
		/// The issue date of the specific reference. The format is CCYY-MM-DD.
		/// </summary>
		IssueDate,

		/// <summary>
		/// Article refers to a statutory article in legal material.
		/// </summary>
		Article,

		/// <summary>
		/// Section is used to capture information typically captured in sections of legislation or reference documents.
		/// </summary>
		Section,

		/// <summary>
		/// Subsection is a subsection of the section part.
		/// </summary>
		Subsection,

		/// <summary>
		/// For a publication that uses chapters, this part should be used to capture this information. Chapters are not necessarily numbered.
		/// </summary>
		Chapter,

		/// <summary>
		/// Paragraph is used to refer to specific paragraphs in a document.
		/// </summary>
		Paragraph,

		/// <summary>
		/// Subparagraph of a paragraph.
		/// </summary>
		Subparagraph,

		/// <summary>
		/// Sub component of a sub paragraph.
		/// </summary>
		Clause,

		/// <summary>
		/// Subcomponent of a clause in a paragraph.
		/// </summary>
		Subclause,

		/// <summary>
		/// In some reference material individual sentences can be referred to, and this element allows them to be referenced.
		/// </summary>
		Sentence,

		/// <summary>
		/// Page number of the reference material.
		/// </summary>
		Page,

		/// <summary>
		/// Footnote is used to reference footnotes in reference information.
		/// </summary>
		Footnote,

		/// <summary>
		/// Example captures examples used in reference documentation.
		/// </summary>
		Example,

		/// <summary>
		/// Exhibit refers to exhibits in reference documentation.
		/// </summary>
		Exhibit,

		/// <summary>
		/// Notes can contain reference material; use this element when the note is published as a standalone document. 
		/// </summary>
		Note,

		/// <summary>
		/// Refers to the name of an Appendix, which could be a number or text.
		/// </summary>
		Appendix,

		/// <summary>
		/// Full URI of the reference such as “http://www.fasb.org/fas133”.
		/// </summary>
		URI,

		/// <summary>
		/// Date that the URI was valid, in CCYY-MM-DD format.
		/// </summary>
		URIDate,
	};

	#endregion


	#region ReferenceTypesEnum 
	/// <summary>
	/// XBRL International valid reference role attribute values.
	/// </summary>
	/// <remarks>Per FRTA-CR5-2005-01-29-redlined-to-2005-01-22.doc.</remarks>
	[Serializable]
	public enum ReferenceTypesEnum
	{
		//keep in the same sequence as ReferenceExtender.TranslatedReferenceTypesEnum so index can be used for cross-reference

		/// <summary>
		/// Any other general commentary on the concept that assists in determining appropriate usage.
		/// </summary>
		Commentary,

		/// <summary>
		/// Reference to documentation that details a precise definition of the concept.
		/// </summary>
		Definition,

		/// <summary>
		/// Reference to documentation that illustrates by example the application of the concept that assists in determining appropriate usage.
		/// </summary>
		Example,

		/// <summary>
		/// Reference concerning the method(s) required to be used when measuring values associated with this concept in business reports.
		/// </summary>
		Measurement,

		/// <summary>
		/// Reference to documentation which details an explanation of the presentation, placement or labeling of this concept in the 
		/// context of other concepts in one or more specific types of business reports.
		/// </summary>
		Presentation,

		/// <summary>
		/// Standard reference for a concept.
		/// </summary>
		Reference,
	};


	#region ReferenceExtenderDefinition

	/// <summary>
	/// ReferenceExtenderDefinition
	/// </summary>
	public class ReferenceExtenderDefinition : IComparable
	{
		#region public members
		//		
		// Data structure to support the following child node for the 'reference' node;
		//
		// <ref:ReferencePartName>ReferencePartValue</ref:ReferencePartName> 
		//
		// Example: 
		// <ref:Publisher>FASB</ref:Publisher> 
		//
		
		//public ReferenceElementsEnum	ReferencePartName;
		//public string					ReferencePartValue	= string.Empty;

		/// <summary>
		/// The reference role of this <see cref="ReferenceExtenderDefinition"/>.
		/// </summary>
		public ReferenceTypesEnum		ReferenceType;

		/// <summary>
		/// The reference parts associated with this <see cref="ReferenceExtenderDefinition"/>.
		/// </summary>
		/// <remarks>
		/// Key is a <see cref="ReferenceElementsEnum"/> value.  Value is a <see cref="String"/>.
		/// </remarks>
		public System.Collections.SortedList ReferenceParts = new SortedList();

		#endregion

		#region constructor
		/// <summary>
		/// Constructs a new instance of <see cref="ReferenceExtenderDefinition"/>.
		/// </summary>
		public ReferenceExtenderDefinition()
		{
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Constructs a new instance of <see cref="ReferenceExtenderDefinition"/>, adding 
		/// given reference part information to this <see cref="ReferenceExtenderDefinition"/>'s parts 
		/// collection.
		/// </summary>
		/// <param name="referencePartName">The reference part type.</param>
		/// <param name="referencePartValue">The reference part value.</param>
		/// <remarks>Method initializes <see cref="ReferenceType"/> to <see cref="ReferenceTypesEnum.Reference"/>.</remarks>
		public ReferenceExtenderDefinition(ReferenceElementsEnum referencePartName, string referencePartValue)
		{
			this.ReferenceType = ReferenceTypesEnum.Reference;
			AddReferenceElement(referencePartName, referencePartValue);
		}

		/// <summary>
		/// Constructs a new instance of <see cref="ReferenceExtenderDefinition"/>, initializing 
		/// the <see cref="ReferenceType"/> of the newly created <see cref="ReferenceExtenderDefinition"/>.
		/// </summary>
		public ReferenceExtenderDefinition(ReferenceTypesEnum referenceType)
		{
			this.ReferenceType = referenceType;
		}

		/// <summary>
		/// Adds parameter-supplied reference part information to this <see cref="ReferenceExtenderDefinition"/>'s parts 
		/// collection.
		/// </summary>
		/// <param name="referenceElements">The reference part type.</param>
		/// <param name="partValue">The reference part value.</param>
		public void AddReferenceElement(ReferenceElementsEnum referenceElements, string partValue)
		{
			ReferenceParts.Add(referenceElements, partValue);
		}

		/// <summary>
		/// Returns a <see cref="String "/>that represents the current <see cref="ReferenceExtenderDefinition"/> object.
		/// </summary>
		/// <returns>A <see cref="String "/>that represents the current <see cref="ReferenceExtenderDefinition"/> object.</returns>
		public override string ToString()
		{
			System.Text.StringBuilder sbInfo = new System.Text.StringBuilder();
			sbInfo.Append( Environment.NewLine );
			sbInfo.Append (" ").Append(this.ReferenceType.ToString()).Append( Environment.NewLine );
			foreach (ReferenceElementsEnum referenceElement in this.ReferenceParts.Keys)
			{
				sbInfo.Append("  -").Append(referenceElement.ToString()).Append(": ").Append(
					this.ReferenceParts[referenceElement].ToString()).Append(Environment.NewLine);
			}
			return sbInfo.ToString();
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="ReferenceExtenderDefinition"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="objIn">An <see cref="object"/> to which this instance of <see cref="ReferenceExtenderDefinition"/>
		/// is to be compared.  Assumed to be a <see cref="ReferenceExtenderDefinition"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="ReferenceExtenderDefinition"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="CompareTo(Object)"/> 
		/// for the <see cref="ReferenceType"/>s of the two <see cref="ReferenceExtenderDefinition"/> objects.</remarks>
		public int CompareTo(object objIn)
		{
			return this.ReferenceType.CompareTo( ((ReferenceExtenderDefinition)objIn).ReferenceType);
		}
		#endregion
	}

	#endregion ReferenceExtenderDefinition

	#endregion

	/// <summary>
	/// Represents a "Locator" type link within the reference linkbase.
	/// </summary>
	[Serializable]
	public class ReferenceLocator: LocatorBase 
	{
		private const string REFERENCE_ROLE = "http://www.xbrl.org/2003/role/{0}Ref";
		#region properties

		/// <summary>
		/// The reference data collection associated with this <see cref="ReferenceLocator"/>.
		/// </summary>
		public Hashtable referenceDatas = new Hashtable();

		/// <summary>
		/// The reference data collection associated with this <see cref="ReferenceLocator"/>.
		/// </summary>
		/// <remarks>
		/// Public for use by Dragon View.
		/// </remarks>
		public Hashtable References
		{
			get {return referenceDatas;}
		}

		#endregion

		#region constructors
		/// <summary>
		/// Initializes a new instance of <see cref="ReferenceLocator"/>.
		/// </summary>
		public ReferenceLocator()
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ReferenceLocator"/>.
		/// </summary>
		/// <param name="dummy">Not used.</param>
		/// <param name="href">The value of the "href" attribute for the locator link.</param>
		public ReferenceLocator(string href, bool dummy)
		{
			unpartitionedHref = href;
		}

		#endregion

		/// <summary>
		/// For example, type could be NAME, NUMBER, CHAPTER etc.
		/// </summary>
		internal void AddReference( string nextRefCount, string type, string reference )
		{
			if (referenceDatas[nextRefCount] == null) //a new set
				referenceDatas[nextRefCount] = new ArrayList ();

			(referenceDatas[nextRefCount] as ArrayList).Add ( type + " " + reference + Environment.NewLine);
		}

		internal void AddInformation( XmlNode child )
		{
			//Parse out eact reference sub type and add to the hash table
			int nextRefCount = References.Count + 1;
			XmlAttribute roleAttr = child.Attributes["xlink:role"] as XmlAttribute;
			string refType = string.Empty;
			if (roleAttr != null)
			{
				refType = child.Attributes["xlink:role"].Value.ToString().Trim();

			}

			foreach (XmlNode referenceTypeNode in child.ChildNodes)
			{
				//ignore the comments
				if (referenceTypeNode.NodeType == XmlNodeType.Comment) continue;
				if (refType.Length > 0)
				{
					AddReference(refType + " " + nextRefCount, referenceTypeNode.LocalName, referenceTypeNode.InnerText);
				}
				else
				{
					AddReference(nextRefCount.ToString(), referenceTypeNode.LocalName, referenceTypeNode.InnerText);

				}
			}
		}

		internal void AddInformation( ReferenceLocator tempLocator )
		{
			//Parse out eact reference sub type and add to the hash table
			int nextRefCount = References.Count + 1;
			foreach (string refKey in tempLocator.referenceDatas.Keys )
			{
				int endPos = refKey.IndexOf(' ');
				string refPart = endPos > 0 ? refKey.Substring(0, endPos) : refKey;
				referenceDatas[refPart + " " + nextRefCount.ToString()] = tempLocator.referenceDatas[refKey];
			}
		}

		private string BuildHashtableReferenceKey ( Aucent.MAX.AXE.XBRLParser.ReferenceTypesEnum referenceType)
		{
			string hashkey = string.Empty;
			hashkey = string.Format(REFERENCE_ROLE, referenceType);
			return hashkey;
		}

		/// <summary>
		/// Refreshes the collection of reference datas within this <see cref="ReferenceLocator"/> from 
		/// a supplied collection of <see cref="ReferenceExtenderDefinition"/>.
		/// </summary>
		/// <param name="references">The source of new reference information.</param>
		/// <remarks>The reference datas collection for this <see cref="ReferenceLocator"/> will be 
		/// reset to a new <see cref="Hashtable"/> regardless of the contents of <paramref name="references"/>.</remarks>
		public void UpdateNodeReferences(ReferenceExtenderDefinition[] references)
		{
			this.referenceDatas = new Hashtable();
			ArrayList refParts;
			string hashKey = string.Empty;

			foreach (ReferenceExtenderDefinition reference in references)
			{
				refParts = new ArrayList(reference.ReferenceParts);

				int i = 1;
				hashKey = BuildHashtableReferenceKey(reference.ReferenceType) + " " + i.ToString();
				while (this.referenceDatas.ContainsKey(hashKey))
				{
					i++;
					hashKey = BuildHashtableReferenceKey(reference.ReferenceType) + " " + i.ToString();
				}

				this.referenceDatas[hashKey] = refParts;
			}
		}
		
		/// <summary>
		/// Merges the contents of a parameter-supplied <see cref="ReferenceLocator"/> into 
		/// the reference datas collection of this <see cref="ReferenceLocator"/>.
		/// </summary>
		/// <param name="dupRL">The <see cref="ReferenceLocator"/> whose reference datas 
		/// collection is to be merged into the reference datas collection of this 
		/// <see cref="ReferenceLocator"/>.</param>
		public void Merge( ReferenceLocator dupRL )
		{
			IDictionaryEnumerator enumer = dupRL.referenceDatas.GetEnumerator();
			while ( enumer.MoveNext() )
			{
				if ( referenceDatas.ContainsKey( enumer.Key ) )
				{
					ArrayList values = (ArrayList)this.referenceDatas[enumer.Key];
					ArrayList newValues = (ArrayList)enumer.Value;

					// only add if they don't exist
					// this is a simple (and slow) search, but I don't think there will be many references
					foreach ( string val in newValues )
					{
						if ( !values.Contains( val ) )
						{
							values.Add( val );
						}
					}
				}
				else
				{
					referenceDatas[enumer.Key] = enumer.Value;
				}
			}
		}
	}
}
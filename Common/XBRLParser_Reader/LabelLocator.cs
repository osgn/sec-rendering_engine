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
using System.Text;

using System.Collections;
using System.Collections.Generic;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents a locator link within the label linkbase.
	/// </summary>
	[Serializable]
	public class LabelLocator : LocatorBase
	{
		#region properties

		internal List<LabelDefinition> labelDatas = new List<LabelDefinition>();

		public List<LabelDefinition> LabelDatas
		{
			get { return labelDatas; }
		}
		#endregion

		#region constructors

		/// <summary>
		/// Constructs a new instance of <see cref="LabelLocator"/>.
		/// </summary>
		public LabelLocator()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="LabelLocator"/>.
		/// </summary>
		/// <param name="hrefArg">The "href" property value to be assigned to the 
		/// new <see cref="LabelLocator"/>.</param>
		public LabelLocator(string hrefArg)
			: base(hrefArg)
		{
		}
		#endregion

		#region overrides

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="LabelLocator"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="LabelLocator"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="LabelLocator"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="LabelLocator"/>.  
		/// Assumed to be a <see cref="LabelLocator"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="LabelLocator"/>.</returns>
		/// <remarks>To be equal, the collection of <see cref="LabelDefinition"/> associated with both 
		/// <see cref="LabelLocator"/> objects must be the same size.</remarks>
		public override bool Equals(object obj)
		{
			if (!(obj is LabelLocator)) return false;

			LabelLocator ll = obj as LabelLocator;

			// just check size of labelDatas
			if (labelDatas.Count != ll.labelDatas.Count) return false;

			return true;
		}

		#endregion

		#region add label

		/// <summary>
		/// Creates and adds a new <see cref="LabelDefinition"/> to the label 
		/// definition collection for this <see cref="DefinitionLocator"/>.
		/// </summary>
		/// <param name="info">The text of the label.</param>
		/// <param name="role">The role to be assigned to the newly created 
		/// <see cref="LabelDefinition"/>.</param>
		/// <param name="lang">The language code to be assigned to the newly 
		/// created <see cref="LabelDefinition"/>.</param>
		public void AddLabel(string lang, string role, string info)
		{
			info = info.Trim(); //make sure there are no leading spaces

			AddLabel(new LabelDefinition(role, lang, info));
		}

		private bool AddLabel(LabelDefinition ld)
		{
			int index = labelDatas.BinarySearch(ld);
			if (index < 0)
			{
				labelDatas.Insert(~index, ld);

				return true;
			}

			return false;
		}

		internal int AddLabels( LabelLocator dupLocator, ArrayList errors )
		{
			int warnings = 0;

			for (int i = 0; i < dupLocator.labelDatas.Count; i++)
			{
				if (!this.AddLabel(dupLocator.labelDatas[i]))
				{
					//why bother?
					//Common.WriteWarning("XBRLParser.Warning.DuplicateLabelForLanguage",
					//    errors, Label,
					//    dupLocator.labelDatas[i].Language,
					//    dupLocator.labelDatas[i].LabelRole,
					//    dupLocator.labelDatas[i].LabelRole, dupLocator.labelDatas[i].Label);

				}
			}
			
			return warnings;
		}

		#endregion

		#region update label

		/// <summary>
		/// Adds the <see cref="LabelDefinition"/> objects within a supplied collection to this 
		///  <see cref="LabelLocator"/>'s collection of labels.
		/// </summary>
		/// <param name="labels">An <see cref="Array"/> of <see cref="LabelDefinition"/>.</param>
		public void Update( LabelDefinition[] labels )
		{
			if (labels == null) return;
			foreach ( LabelDefinition label in labels )
			{
				int index = labelDatas.BinarySearch( label );
				if ( index < 0 )
				{
					labelDatas.Insert( ~index, label );
				}
				else
				{
					labelDatas[index] = label;
				}
			}
		}

		#endregion

		#region public methods

		/// <summary>
		/// Retrieves and returns a label from this <see cref="LabelLocator"/>'s collection 
		/// of label definitions.
		/// </summary>
		/// <param name="lang">The language code of the label to be retrieved.</param>
		/// <param name="role">The role of the label to be retrieved.</param>
		/// <param name="info">An output parameter.  The retrieved label.</param>
		/// <returns>True if <paramref name="info"/> could be retrieved and is not null.</returns>
		public bool TryGetInfo( string lang, string role, out string info )
		{
			LabelDefinition ll = new LabelDefinition( role, lang, null );
			int index = labelDatas.BinarySearch( ll );
			if ( index >= 0 )
			{
				ll = labelDatas[index];
			}

			info = ll.Label;

			return info != null;
		}

		/// <summary>
		/// Writes the XML underlying this <see cref="LabelLocator"/> to a parameter-supplied
		/// XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="lang">The language code for which this <see cref="LabelLocator"/> is 
		/// to be written.  If language for this <see cref="LabelLocator"/> is 
		/// not <paramref name="lang"/>, no XML is appended to <paramref name="xml"/>.</param>
		/// <param name="xml">The output XML to which <paramref name="e"/> is to be appended.</param>
		public void WriteXmlFragment( string lang, StringBuilder xml )
		{
			bool addLang = true;
			foreach ( LabelDefinition ll in this.labelDatas )
			{
				if ( ll.Language.Equals( lang ) )
				{
					if ( addLang )
					{
						xml.Append( " lang=\"" ).Append( lang ).Append( "\"" );
						addLang = false;
					}

					xml.Append( " " ).Append( ll.LabelRole ).Append( "=\"" ).Append( ll.Label ).Append( "\"" );
				}
			}
		}

		#endregion

		#region internal, private, protected methods

		internal LabelLocator CreateCopyForMerging()
		{
			LabelLocator clone = new LabelLocator();
			clone.CopyLocatorBaseInformation(this);

			//clone.labelDatas = new List<LabelDefinition>(this.labelDatas);

			foreach (LabelDefinition ld in this.labelDatas)
			{
				clone.labelDatas.Add(new LabelDefinition(ld.LabelRole, ld.Language, ld.Label));
			}


			return clone;
		}

		internal string GetDefaultLabel( string lang )
		{
			if ( this.labelDatas.Count == 1 )
			{
				if ( labelDatas[0].Language.Equals( lang ) )
				{
					return labelDatas[0].Label != null ? labelDatas[0].Label : string.Empty;
				}

			}

			LabelDefinition ll = new LabelDefinition( "label", lang, null );
			int index = labelDatas.BinarySearch( ll );
			if ( index >= 0 )
			{
				return labelDatas[index].Label != null ? labelDatas[index].Label : string.Empty;
			}
			ll.LabelRole = "terseLabel";
			index = labelDatas.BinarySearch( ll );
			if ( index >= 0 )
			{
				return labelDatas[index].Label != null ? labelDatas[index].Label : string.Empty;
			}

			foreach ( LabelDefinition ld in this.labelDatas )
			{
				if ( ld.Language.Equals( lang ) )
				{
					return ld.Label != null ? ld.Label : string.Empty;
				}
			}

			return string.Empty;
		}

		#endregion
	}
}
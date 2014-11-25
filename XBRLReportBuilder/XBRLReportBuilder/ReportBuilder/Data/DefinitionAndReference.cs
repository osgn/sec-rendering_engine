//=============================================================================
// AuthoritativeReference (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class stores definition and references.
//=============================================================================

using System;
using System.IO;
//using System.Xml.Serialization;

using System.Xml;

using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace XBRLReportBuilder
{
	/// <summary>
	/// AuthoritativeReference
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
//	[Serializable]
	public class DefinitionAndReference
	{
        
		#region properties
		
		public Dictionary<string,DefAndRef> references = new Dictionary<string,DefAndRef>();

		public bool HasReferences
		{
			get { return this.references.Count > 0; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new AuthoritativeReference.
		/// </summary>
		public DefinitionAndReference()
		{
		}

		#endregion

        /// <summary>
        /// Add a Definition and Reference for the given element.
        /// </summary>
        /// <param name="elementName">The element.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="reference">The reference.</param>
		public void Add( string elementName, string definition, string reference )
		{
			if ( !references.ContainsKey( elementName ) )
			{
				references[elementName] = new DefAndRef( definition, reference );
			}
			else
			{
				references[elementName].Def = definition;
				references[elementName].Ref = reference;
			}
		}

        //TODO remove - unused
		public void AddDefinition( string elementName, string definition )
		{
			Add( elementName, definition, null );
		}

        //TODO remove - unused
		public void AddReference( string elementName, string reference )
		{
			Add( elementName, null, reference );
		}
	}

	public class DefAndRef
	{
		public string Def;
		public string Ref;

        /// <summary>
        /// Create a new <see cref="DefAndRef"/> instance with the given
        /// <paramref name="def"/> and <paramref name="Ref"/>.
        /// </summary>
        /// <param name="def">The definitions.</param>
        /// <param name="Ref">The references.</param>
		public DefAndRef( string def, string Ref )
		{
			this.Def = def;
			this.Ref = Ref;
		}
	}
}
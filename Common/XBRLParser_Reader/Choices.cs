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
using System.Collections;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// A collection of <see cref="Choice"/> objects.
	/// </summary>
	[Serializable]
	public class Choices
	{
		#region properties
		/// <summary>
		/// The collection of <see cref="Choice"/> objects contained within this <see cref="Choices"/>.
		/// </summary>
		public ArrayList TheChoices = new ArrayList();

		/// <summary>
		/// The "minOccurs" value for this <see cref="Choices"/>.
		/// </summary>
		public int MinOccurances = 0;

		/// <summary>
		/// The "maxOccurs" value for this <see cref="Choices"/>.
		/// </summary>
		public int MaxOccurances = int.MaxValue;
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Choices"/>.
		/// </summary>
		public Choices()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Choices"/>.
		/// </summary>
		/// <param name="minOccurs">The "minOccurs" value for this <see cref="Choices"/> object.</param>
		/// <param name="maxOccurs">The "maxOccurs" value for this <see cref="Choices"/> object.</param>
		public Choices(int minOccurs, int maxOccurs)
		{
			MinOccurances = minOccurs;
			MaxOccurances = maxOccurs;
		}

		#endregion

	}
}
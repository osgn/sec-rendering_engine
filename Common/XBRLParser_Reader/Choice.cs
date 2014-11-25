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

using Aucent.MAX.AXE.Common.Exceptions;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents the properties of a XML schema definition "choice" element.
	/// </summary>
	public class Choice
	{
		#region properties
		/// <summary>
		/// The <see cref="Element"/> to which this <see cref="Choice"/> belongs.
		/// </summary>
        public Element MyElement = null;
		private int MinOccurances = 0;
		private int MaxOccurances = int.MaxValue;
		private Choices ParentContainer = null;
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Choice"/>.
		/// </summary>
		public Choice()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Choice"/>.
		/// </summary>
		/// <param name="container">The <see cref="Choices"/> object to which this <see cref="Choice"/>
		///  object belongs.</param>
		/// <param name="elem">The <see cref="Element"/> to which this <see cref="Choice"/> belongs.</param>
		///  <param name="minOccurs">The "minOccurs" value for this <see cref="Choice"/>.</param>
		///  <param name="maxOccurs">The "maxOccurs" value for this <see cref="Choice"/>.</param>
		public Choice(Choices container, Element elem, int minOccurs, int maxOccurs)
		{
			MyElement = elem;
			MinOccurances = minOccurs;
			MaxOccurances = maxOccurs;

			ParentContainer = container;

			//container.TheChoices.Add( this );

			MyElement.IsChoice = true;
			MyElement.UseChoiceIcon = true;
		}
		#endregion

	}
}
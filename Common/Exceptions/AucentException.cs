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

namespace Aucent.MAX.AXE.Common.Exceptions
{
	/// <summary>
	/// AucentException
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class AucentException : ApplicationException
	{       
		#region properties

		protected string resKey;
		protected ArrayList msgParams;

		public String ResourceKey
		{
			get { return resKey; }
			set { resKey = value; }
		}

		public ArrayList Parameters
		{
			get { return msgParams; }
			set { msgParams = value; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new AucentException.
		/// </summary>
		public AucentException()
		{
		}

		public AucentException( string key ) : base()
		{
			resKey = key;
		}

		public AucentException( string key, string msg ) : base( msg )
		{
			Initialize( key, null );
		}

		public AucentException( string key, string msg, Exception ex ) : base( msg, ex )
		{
			Initialize( key, null );
		}

		public AucentException( string key, ArrayList parameters ) : base()
		{
			Initialize( key, parameters );
		}

		public AucentException( string key, ArrayList parameters, string msg ) : base( msg )
		{
			Initialize( key, parameters );
		}

		public AucentException( string key, ArrayList parameters, string msg, Exception ex ) : base( msg, ex )
		{
			Initialize( key, parameters );
		}

		protected void Initialize( string key, ArrayList parameters )
		{
			resKey = key;
			msgParams = parameters;
		}

		#endregion

	}
}
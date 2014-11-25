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
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Aucent.MAX.AXE.XBRLParser
{
    /// <summary>
    /// Information regarding the linkbase files used in the taxonomy
    /// </summary>
	[Serializable]
    public class LinkbaseFileInfo : IComparable<LinkbaseFileInfo>,ICloneable
    {
        #region ctors
        /// <summary>
        /// default constructor
        /// </summary>
        public LinkbaseFileInfo()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public LinkbaseFileInfo(LinkbaseFileInfo copy)
        {
            this.LinkType = copy.LinkType;
            this.Filename = copy.Filename;
            this.XSDFileName = copy.XSDFileName;
            this.RoleRefURIs = new List<string>(copy.RoleRefURIs);

        }
        #endregion
        /// <summary>
        /// enum to determine the type of the linkbase
        /// </summary>
        public enum LinkbaseType
        {
            /// <summary>
            /// Presentation linkbase
            /// </summary>
            Presentation,
            /// <summary>
            /// Calculation linkbase
            /// </summary>
            Calculation,
            /// <summary>
            /// definition linkbase
            /// </summary>
            Definition,
            /// <summary>
            /// reference linkbase
            /// </summary>
            Reference,
            /// <summary>
            /// label linkbase
            /// </summary>
            Label, 
            /// <summary>
            /// linkbase not specified .. the file could have multiple linkbase info
            /// </summary>
            None
        }

        /// <summary>
        /// linkbase type
        /// </summary>
        public LinkbaseType LinkType = LinkbaseType.None;

        /// <summary>
        /// filename of the linkbase
        /// </summary>
        public string Filename;


        /// <summary>
        /// filename of the xsd that is responsible for loading this file
        /// </summary>
        public string XSDFileName;



        /// <summary>
        /// List of URIs used by the linkbase...
        /// I.e. the rolerefs that would get included by including this file in the taxonomy...
        /// </summary>
        public List<string> RoleRefURIs = new List<string>();


        

        /// <summary>
        /// string descr of the linkbasefileinfo
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach( string str in RoleRefURIs) sb.Append( str ).Append(":");
            return string.Format("{0}:{1}:{2}:{3}", LinkType, Filename, XSDFileName, sb.ToString());
        }

        #region IComparable<LinkbaseFileInfo> Members
        /// <summary>
        /// sort by type
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(LinkbaseFileInfo other)
        {
            return this.LinkType.CompareTo(other.LinkType);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// clone object...
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new LinkbaseFileInfo(this);
        }

        #endregion
    }
}

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
using System.Xml.Serialization;
using System.Collections;

using Aucent.MAX.AXE.Common.Resources;

namespace Aucent.MAX.AXE.Common.Data
{
	/// <summary>
	/// MarkupItem
	/// </summary>
	[Serializable]
	public abstract class MarkupItem:IComparable
	{
		#region properties

        /// <summary>
        /// Markup type code
        /// </summary>
		public enum MarkupTypeCode {
            /// <summary>
            /// Unknown markup type 
            /// </summary>
            Unknown,
            /// <summary>
            /// XBRL markup type 
            /// </summary>
            XBRL,
            /// <summary>
            /// Application specific markup type 
            /// </summary>
            MAX};

        /// <summary>
        /// Name of the markup item.
        /// </summary>
		protected string myName			= string.Empty;

        /// <summary>
        /// Name of the markup item.
        /// </summary>
		public virtual string Name
		{
			get { return myName; }
			set { myName = value; }
		}
	
        /// <summary>
        /// Indicates if the markup item is draggable (from the selector panel in DT).
        /// </summary>
		[XmlIgnore]
		public bool		IsDraggable		= true;

        /// <summary>
        /// Indicated if the markup item can be edited (in the selector panel in DT).
        /// </summary>
		[XmlIgnore]
		public bool		IsEditable		= true;

		private bool isFromEntityGroup = false;
        /// <summary>
        /// Indicates if this markup item is inheritated from the entity group.
        /// </summary>
		public bool IsFromEntityGroup
		{
			get{ return isFromEntityGroup; }
			set{ isFromEntityGroup = value; }
		}

        /// <summary>
        /// Indiactes if this markup item is the default for the entity that owns this markup item.
        /// </summary>
		public bool IsDefaultForEntity = false;

		private int inUseCount = 0;
        /// <summary>
        /// Number of times this markup item is used in the current markup document.
        /// This property is used to show/hide the "in-use" star. 
        /// When the in use count is greater than zero, the in-use start would be displayed.
        /// </summary>
		[XmlIgnore]
		public int InUseCount
		{
			get {return inUseCount;}
			set {inUseCount = value;}
		}
		[NonSerialized]
		private ArrayList treeNodes;
        /// <summary>
        /// ArrayList of TreeNodeContainer objects
        /// </summary>
		[XmlIgnore]
		public ArrayList TreeNodes
		{
			get {return treeNodes;}
			set {treeNodes = value;}
		}

		#endregion

		#region constructors

        /// <summary>
        /// Creates a new MarkupItem.
        /// </summary>
		public MarkupItem()
		{
		}
        /// <summary>
        /// Creates a new MarkupItem by accepting a name for the markup item.
        /// </summary>
        /// <param name="name">Name for the markup item.</param>
		public MarkupItem(string name)
		{
			this.Name = name;
		}

		#endregion

		/// <summary>
		/// Implemented only for Segment and Scenario derived classes.
		/// </summary>
		/// <returns>Returns null.</returns>
		public virtual ContextDimensionInfo GetContextDimensionInfo()
		{
			return null;
		}

        /// <summary>
        /// Implemented only for Segment and Scenario derived classes.
        /// </summary>
        /// <param name="cdi">The ContextDimensionInfo.</param>
		public virtual void SetContextDimensionInfo(ContextDimensionInfo cdi)
		{

		}

        /// <summary>
        /// This method copies the name and if the markup item is default for the entity to the receiver.
        /// </summary>
        /// <param name="receiver">The markup item the name and is default for the entity falg would be copied to.</param>
		public virtual void CopyTo( MarkupItem receiver )
		{
			receiver.Name				= Name;
			receiver.IsDefaultForEntity = this.IsDefaultForEntity;
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="MarkupItem"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="MarkupItem"/>.  
		/// Assumed to be a <see cref="MarkupItem"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="MarkupItem"/>.</returns>
		/// <remarks>To be equal, the name and IsDefaultForEntity properties for the two <see cref="MarkupItem"/> must be equal.</remarks>
		public override bool Equals(object obj)
		{
			if( !(obj is MarkupItem) ) return false;

			MarkupItem mi = obj as MarkupItem;

			if( this.Name.CompareTo( mi.Name ) != 0 ) return false;
			if( this.IsDefaultForEntity != mi.IsDefaultForEntity ) return false;

			return ValueEquals(obj);
		}

		/// <summary>
		/// Serves as a hash function for this instance of <see cref="MarkupItem"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="MarkupItem"/>.
		/// </returns>
		/// <remarks>
		/// Override GetHashCode() is required for overriding Equals().
		/// </remarks>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="MarkupItem"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="MarkupItem"/>
		/// is to be compared.  Assumed to be a <see cref="MarkupItem"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="MarkupItem"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> 
		/// for the names of the two <see cref="MarkupItem"/>.</remarks>
		public virtual int CompareTo(object obj)
		{
			return string.Compare( this.Name, (obj as MarkupItem).Name );
		}

		#endregion

		#region IsValid
        /// <summary>
        /// This method validates the name of this <see cref="MarkupItem"/> based on the following rules:
        /// (1) Name must not be empty of null;
        /// (2) Optionally check to ensure the name is FRTA compliant;
        ///     Determines if the given name is FRTA compliant;
        ///     A name must start with an alphabetic character
        ///     A name must not contain spaces
        ///     A name must not contain special characters
        ///     A name must not exceed 256 chars
        /// </summary>
        /// <param name="markupType">Markup type code (XBRL or internal).</param>
        /// <param name="errorMessages">Validation messages.</param>
        /// <param name="CheckFRTA">Flag to indicate if the system should check to ensure the name is FRTA compliant</param>
        /// <returns>Returns true, if the name is valid.</returns>
		public virtual bool IsValid( MarkupTypeCode markupType, out ArrayList errorMessages, bool CheckFRTA)
		{
			errorMessages = new ArrayList();
			
			if ( Name == null || Name == string.Empty )
			{
				errorMessages.Add( StringResourceUtility.GetString("Common.Error.InvalidName") );
				return false;
			}

			if (CheckFRTA)
			{
				if (!Aucent.MAX.AXE.Common.Utilities.StringUtility.IsNameFRTAValid(this.Name))
				{
					errorMessages.Add( 
						String.Format(StringResourceUtility.GetString("Client.UserControl.TreeNodeRename.FRTACompliance"), 
						this.Name));

					errorMessages.Add( String.Format(StringResourceUtility.GetString("Common.Error.InvalidFRTAName"), 
						Aucent.MAX.AXE.Common.Utilities.StringUtility.InvalidString));

					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// Returns a flag to indicate if the markup item is empty.        
        /// </summary>
        /// <returns>Returns a flag to indicate if the markup item is empty.</returns>
		public virtual bool IsEmpty()
		{
			return false;
		}
		#endregion

		/// <summary>
		/// Determines whether the value of a supplied <see cref="object"/> is equivalent to the value 
		/// of this instance of <see cref="MarkupItem"/>.
		/// </summary>
		/// <param name="obj">An <see cref="Object"/>, assumed to be a <see cref="MarkupItem"/>
		/// that is to be compared to this instance of <see cref="MarkupItem"/>.</param>
		/// <returns>A <see cref="Boolean"/> indicating if the object values are equal (true) or not (false).</returns>
		/// <remarks>If <paramref name="obj"/> is a <see cref="MarkupItem"/>, this method always returns true.</remarks>
		public virtual bool ValueEquals(object obj)
		{
			if (!(obj is MarkupItem)) return false;

			return true;
		}

        /// <summary>
        /// This method increment the in-use count by the specified number. 
        /// It's used when the markup item is dragged and dropped to the markup document.
        /// </summary>
        /// <param name="incrementAmount">The number to increment.</param>
		public void IncrementInUseCount( int incrementAmount )
		{
			inUseCount += incrementAmount;
		}

        /// <summary>
        /// This method decrement the in-use count by the specified number. 
        /// It's used when the markup item is removed from the markup document.
        /// </summary>
        /// <param name="decrementAmount">The number to decrement.</param>
		public void DecrementInUseCount( int decrementAmount )
		{
			inUseCount -= decrementAmount;
			if ( inUseCount < 0 )
				inUseCount = 0;
		}

       


        /// <summary>
        /// Add a tree node to the collection (for UI).
        /// The collection of tree nodes contain this specific markup items is maintained.
        /// </summary>
        /// <param name="treeNode"></param>
		public void AddTreeNode( object treeNode )
		{
			if (treeNodes == null)
				treeNodes = new ArrayList();

			// only add the tree node if it doesn'r already exist
			bool found = false;
			foreach( object nc in treeNodes )
			{
                if (nc == treeNode )
				{
					found = true;
					break;
				}
			}

			if (!found)
                treeNodes.Add(treeNode);
		}

        /// <summary>
        /// This method clears the internal collection of the tree nodes that reference the markup item.
        /// </summary>
		public void ClearTreeNodes()
		{
			if (treeNodes != null)
				treeNodes.Clear();
		}

        /// <summary>
        /// This method returns a specific tree node based on the index number passed in.
        /// </summary>
        /// <param name="index">Index number.</param>
        /// <returns>Returns the tree node at the specified index. If cannot find the specific index, returns null.</returns>
		public object GetTreeNodeAt(int index)
		{
			if (treeNodes != null)
			{
				if (index < treeNodes.Count)
				{
					return treeNodes[index];
				}
			}

            return null ;
		}

        /// <summary>
        /// This method returns a specific tree node container based on the index number passed in.
        /// </summary>
        /// <param name="index">Index number.</param>
        /// <returns>Returns the tree node container at the specified index. If cannot find the specific index, returns null.</returns>

		public object GetTreeNodeContainerAt(int index)
		{
            object node = null;
			if (treeNodes != null)
			{
				if (index < treeNodes.Count)
				{
					node = treeNodes[index];
				}
			}

			return node;
		}
	}
}
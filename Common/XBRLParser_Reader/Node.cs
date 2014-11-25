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

using Aucent.MAX.AXE.Common.Utilities;
using Aucent.MAX.AXE.Common.Data;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Represents an XBRL node.
	/// </summary>
    public class Node : MarkupItem, IComparable, IComparable<Node>
	{       
		#region properties

		private bool prohibited = false;
		private bool xslt = false;

		private object tag;
		internal Node parent;
		private Element e;

		/// <summary>
		/// Label of this <see cref="Node"/>.
		/// </summary>
		private string label;
		private string prefLabel;
		private string calculationWeight = "1";
		private string newCalculationWeight = "1";
		
		/// <summary>
		/// The list of children Nodes for this Node.
		/// If there are Nodes in this list, then this Node is their parent.
		/// This is a linked-list.
		/// </summary>
		public ArrayList children;

        private string currentTupleSetName;

		private PresentationLink presLink = null;

		internal double order;

		internal const string LABELROLE = "label";
		private const string PREFERRED_LABEL_ROLE = "preferredLabel";

		private const string LEADING_CHARS_TO_STRIP = @"[]{}!@#$%^&*()-_+=|\?/<>:;,.";

		#endregion

		#region Element Accessors
		/// <summary>
		/// The collection of values for the enumeration data of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		public ArrayList EnumData
		{
			get { return e.EnumData == null ? null : e.EnumData.Values; }
		}

		/// <summary>
		/// The data type of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="Element.DataTypeCode.na"/>.</remarks>
		public Element.DataTypeCode MyDataType
		{
			get { return e.IsNull ? Element.DataTypeCode.na : e.MyDataType; }
		}

		/// <summary>
		/// The attribute type of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="Element.DataTypeCode.na"/>.</remarks>
		public Element.AttributeType MyAttributeType
		{
			get { return e.IsNull ? Element.AttributeType.na: e.MyAttributeType; }
		}

		/// <summary>
		/// The calculation weight of this <see cref="Node"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public string Weight 
		{
			get { return e.IsNull ? string.Empty : calculationWeight; }
			set { calculationWeight = value;}
		}

		/// <summary>
		/// The new weight of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public string NewWeight 
		{
			get { return e.IsNull ? string.Empty : newCalculationWeight; }
			set { newCalculationWeight = value;}
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is nillable.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is false.</remarks>
		public bool Nillable 
		{
			get { return e.IsNull ? false : e.Nillable; }	
		}

		/// <summary>
		/// Public accessor for the internal label property
		/// </summary>
		public string Label
		{
			get { return label; }
			set { label = value; }
		}

		/// <summary>
		/// The id of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		public string Id
		{
			get { return e.Id; }
			set { e.Id = value; }
		}

		/// <summary>
		/// The name of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public new string Name
		{
			get { return e.IsNull ? "" : e.Name; }
			set { e.Name = value; }
		}

		/// <summary>
		/// The taxonomy information index of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is zero.</remarks>
		public int TaxonomyInfoId
		{
			get { return e.IsNull ? 0 : e.TaxonomyInfoId; }
		}

		/// <summary>
		/// The period type of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public string PeriodType
		{
			get { return e.IsNull ? "" : e.PerType.ToString(); }
			set	{ e.PerType = (Element.PeriodType)Enum.Parse(typeof(Element.PeriodType), value, true);	}
		}

		/// <summary>
		/// The balance type of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public string BalanceType
		{
			get { return e.IsNull ? "" : e.BalType.ToString(); }
			set	{ e.BalType = (Element.BalanceType)Enum.Parse(typeof(Element.BalanceType), value, true); }
		}

		/// <summary>
		/// True if this <see cref="Node"/> is an Aucent (Rivet) extended element.
		/// </summary>
		public bool IsAucentExtendedElement
		{
			get { return e.IsNull ? false : e.IsAucentExtendedElement; }
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is a tuple.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is false.</remarks>
		public bool IsTuple
		{
			get { return e.IsNull ? false : e.IsTuple; }
			set { e.IsTuple = value; }
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is abstract.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is true.</remarks>
		public bool IsAbstract
		{
			get { return e.IsNull ? true : e.IsAbstract; }
			set { e.IsAbstract = value; }
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is a choice element.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is false.</remarks>
		public bool IsChoice
		{
			get { return e == null ? false : e.IsChoice; }
		}

		/// <summary>
		/// True if this <see cref="Node"/> can be marked-up.
		/// </summary>
		/// <remarks>Defined as <see cref="Element.CanMarkup"/> is true, <see cref="IsProhibited"/> is 
		/// false, and parent, if defined, is not prohibited.</remarks>
		public bool CanMarkup
		{
			get { return e.CanMarkup && !IsProhibited && ( parent == null || ( parent != null && !parent.IsProhibited ) ); }
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is null.
		/// </summary>
		public bool ElementIsNull
		{
			get { return e.IsNull; }
		}

		/// <summary>
		/// The definition of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.  
		/// Value returned is equal to <see cref="Element.GetDefinition(String)"/>.</remarks>
		public string GetDefinition(string lang)
		{
			return e.IsNull ? string.Empty : e.GetDefinition(lang); 
		}

		/// <summary>
		/// The element type of this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/>.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is <see cref="String.Empty"/>.</remarks>
		public string ElementType
		{
			get { return e.IsNull ? string.Empty : e.ElementType; }
			set { e.ElementType = value; }
		}

		/// <summary>
		/// The calculation weight of this <see cref="Node"/>.
		/// </summary>
		public string CalculationWeight 
		{
			set {calculationWeight = value;}
			get {return calculationWeight;}
		}

		/// <summary>
		/// True if this <see cref="Node"/>'s 
		/// underlying <see cref="Element"/> is a nested tuple.
		/// </summary>
		/// <remarks>If the <see cref="Element"/> is null, value is false.</remarks>
		public bool IsParentTuple
		{
            //return true if the node is a tuple child node..
            get { return  this.e == null ? false : RecursivelyDetermineIfNodeHasTupleParent(this, this.Id ) ; }
		}

        /// <summary>
        /// returns true only if a parent( recursively) node is a tuple parent and also only if 
        /// the tuple parent has the idtoCheck as a tuple child.
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="idToCheck"></param>
        /// <returns></returns>
        private bool RecursivelyDetermineIfNodeHasTupleParent(Node cur, string idToCheck )
        {
            if (cur.parent == null) return false;

            if (cur.parent.MyElement != null && cur.parent.MyElement.IsTuple )
            {
                //need to make sure that the parent tuple has the idtocheck as a child element
                //if not then it is a wierd node hierarchy where one set of tuple parent has 
                //a unrelated element as the tuple child...
                return cur.parent.MyElement.FindChildElement(idToCheck) >= 0 ? true : false;
            }

            return RecursivelyDetermineIfNodeHasTupleParent(cur.parent, idToCheck);
        }
        

		#endregion

		#region Node accessors

		/// <summary>
		/// The presentation link associated with this <see cref="Node"/>.
		/// </summary>
		public PresentationLink MyPresentationLink
		{
			get { return presLink; }
			set { presLink = value; }
		}

		private bool XSLT
		{
			get { return xslt;  }
			set { xslt = value; }
		}

		/// <summary>
		/// The element underlying this <see cref="Node"/>.
		/// </summary>
		public Element MyElement
		{
			get { return e; }
            set { e = value; }
		}

		/// <summary>
		/// The parent of this <see cref="Node"/>.
		/// </summary>
		public Node Parent
		{
			get { return parent; }
            set { parent = value; }
		}

		/// <summary>
		/// contains the preferred label role
		/// </summary>
		public string PreferredLabel
		{
			get { return prefLabel; }
            set { prefLabel = value; }
		}

		/// <summary>
		/// The first tree node within this <see cref="Node"/>'s tree nodes collection.
		/// </summary>
		public object Tag
		{
			get { return GetTreeNodeAt(0); }
			set { AddTreeNode(  value  ); }
		}

		/// <summary>
		/// The collection of child <see cref="Node"/> objects for this <see cref="Node"/>.
		/// </summary>
		public ArrayList Children
		{
			get { return children; }
		}

		/// <summary>
		/// True if this node has child <see cref="Node"/>s.
		/// </summary>
		public bool HasChildren 
		{
			get {return children != null;}
		}

		/// <summary>
		///  Represents the current tuple set name
		/// </summary>
        public string CurrentTupleSetName
		{
            get { return currentTupleSetName; }
            set { currentTupleSetName = value; }
		}

		/// <summary>
		/// The order of this <see cref="Node"/>.
		/// </summary>
		public double Order
		{
			get { return order; }
            set { order = value; }
		}

		/// <summary>
		/// True if this <see cref="Node"/> is prohibited.
		/// </summary>
		public bool IsProhibited 
		{
			get { return prohibited; }
			set { prohibited = value; }
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new instance of <see cref="Node"/>.
		/// </summary>
		public Node()
		{
			label = string.Empty;
			this.e = new Element();
		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Node"/>.
		/// </summary>
		/// <param name="titleArg">The label (title) to be assigned to the newly-created <see cref="Node"/>.</param>
		public Node(string titleArg)
		{
			label = titleArg;
			this.e = new Element();
		}

		
        /// <summary>
        /// set the element in the node...
        /// </summary>
        /// <param name="elem"></param>
		public Node(Element elem )
		{
            this.e = elem;
				

		}

		/// <summary>
		/// Overloaded.  Creates a new instance of <see cref="Node"/> and initializes 
		/// its properties from a parameter-supplied <see cref="Node"/>.
		/// </summary>
		/// <param name="n">The <see cref="Node"/> from which the newly created <see cref="Node"/> 
		/// will be initialized.</param>
		public Node(Node n)
		{
			// copy MarkupItem members
			this.myName = n.myName;
			this.IsDraggable = n.IsDraggable;
			this.IsEditable = n.IsEditable;
			this.IsFromEntityGroup = n.IsFromEntityGroup;
			this.IsDefaultForEntity = n.IsDefaultForEntity;
			this.InUseCount = n.InUseCount;

			// and copy Node members
			this.prohibited = n.prohibited;
			this.tag = n.tag;
			this.parent = n.parent;
			this.xslt = n.xslt;
			this.e = n.e;
			this.label = n.label;		
			this.presLink = n.presLink;
			this.order = n.order;
			this.prefLabel = n.prefLabel;
			this.calculationWeight = n.calculationWeight;
			this.newCalculationWeight = n.newCalculationWeight;
			if (n.children != null)
			{
				this.children = new ArrayList(n.children);
			}

            this.currentTupleSetName = n.currentTupleSetName;
			
		}
		#endregion

		#region Tuple stuff
		/// <summary>
		/// Used by normalizedNode only - DO NOT USE
		/// </summary>
		public void SetHasTupleParent(bool hasTupleParent)
		{
            if (e == null)
                return;
            else
            {
                hasTupleParent = RecursivelyDetermineIfNodeHasTupleParent(this, this.Id);
            }
		}
		
		/// <summary>
		/// Determines if any ancestor <see cref="Node"/> of this <see cref="Node"/> or of this 
		/// <see cref="Node"/>'s element is a tuple.
		/// </summary>
		/// <returns>True if an ancestor is a tuple.  False otherwise.</returns>
		public bool HasTupleParent()
		{
			/*BUG 1820 - recurse up through the parent nodes to check for a tuple parent 
			 * instead of using the element because the element and all clones have tupleParent = 1.
			 * In the IM taxonomy Common Stock is a tuple in some places and a normal element in others. */
			Node parent = this.Parent;
			while ( parent != null )
			{
				if ( parent.IsTuple )
				{
					return true;
				}
				parent = parent.Parent;
			}
            return false;
		}

		private bool ElementParentIsNull
		{
			get { return e.IsNull || !RecursivelyDetermineIfNodeHasTupleParent( this, this.Id ); }
		}

		/// <summary>
		/// Retrieves and returns lowest-level ancestor <see cref="Node"/> of this <see cref="Node"/> 
		/// that is a tuple.
		/// </summary>
		/// <returns>The retrieved <see cref="Node"/>.  Null if this <see cref="Node"/> has no parent.  
		/// The grandparent <see cref="Node"/> of this <see cref="Node"/> will be returned if no tuple 
		/// ancestor could be found.</returns>
		/// <remarks>If this <see cref="Node"/> is itself a tuple, it will be returned.</remarks>
		public Node GetNodeTupleParent()
		{
			
			if ( parent == null ) return null;
			
			if ( parent.IsTuple ) return parent;
			
			// somewhere up the chain, hopefully we have a tuple parent
			Node temp = parent.parent;
			
			while ( temp != null && !temp.IsTuple )
			{
				temp = temp.parent;
			}
						
			return temp;
		}



		/// <summary>
		/// Recursively looks through the children (and it's children's children for the matching id
		/// </summary>
		public Node GetValidChild(string id)
		{
			if (this.children != null)
			{
				foreach (Node n in children)
				{
					if (n.IsProhibited) continue;

					if (string.Compare(n.Id, id) == 0)
					{
						return n;
					}
					else if (n.HasChildren)
					{
						Node cn = n.GetValidChild(id);

						if (cn != null)
						{
							return cn;
						}

					}
				}

			}

			return null;
		}


		/// <summary>
		/// Recursively looks through the children (and it's children's children for the matching id
		/// </summary>
		public Node GetChild( string id )
		{
			if (this.children != null)
			{
				foreach (Node n in children)
				{
                    if (n.IsProhibited) continue;
					if (string.Compare(n.Id, id) == 0)
					{
						return n;
					}
					else if (n.HasChildren)
					{
						Node cn = n.GetChild(id);

						if (cn != null)
						{
							return cn;
						}

					}
				}

			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="childId"></param>
		/// <returns></returns>
		public Node GetChildNode(string parentId, string childId)
		{
			bool isParent = this.Id.Equals(parentId);
			if (this.children != null)
			{
				foreach (Node n in children)
				{
					if (n.IsProhibited) continue;

					if (isParent)
					{
						if (string.Compare(n.Id, childId) == 0)
						{
							return n;
						}
					}


					if (n.HasChildren)
					{
						Node cn = n.GetChildNode(parentId, childId);

						if (cn != null)
						{
							return cn;
						}

					}
				}
			}

			return null;
		}

		



        /// <summary>
        /// For any given node get the title node that is the top level node in that view
        /// </summary>
        /// <returns></returns>
        public Node GetRootNode()
        {
            if (this.parent == null) return this;

            return this.parent.GetRootNode();
        }

        /// <summary>
        /// for a given node get the parent tuple and then determine the 
        /// min occurance of that element under that parent tuple
        /// </summary>
        /// <returns></returns>
        public int GetMinOccurance()
        {
            Node tupleParent = this.GetNodeTupleParent();
            if (tupleParent != null)
            {
                return tupleParent.MyElement.GetChildMinOccurance(this.Id);
            }
            return 0;
        }

        /// <summary>
        /// for a given node get the parent tuple and then determine the 
        /// max occurance of that element under that parent tuple
        /// </summary>
        /// <returns></returns>
        public int GetMaxOccurance()
        {
            Node tupleParent = this.GetNodeTupleParent();
            if (tupleParent != null)
            {
                return tupleParent.MyElement.GetChildMaxOccurance(this.Id);
            }
            return int.MaxValue;
        }
		#endregion

		/// <summary>
		/// Sets the internal order property.
		/// </summary>
		/// <param name="order"></param>
		public void SetOrder( float order )
		{
			this.order = order;
		}
		/// <summary>
		/// Determines if the display for the node is reversed.
		/// this is useful if we need to reverse the data before applying it to the instance document.
		/// </summary>
		/// <returns></returns>
		public bool IsDisplayReversed()
		{
			if (prefLabel != null && prefLabel.Length > 0 )
			{
                return Taxonomy.IsNegatedLabelRole(prefLabel);
			
			}

			return false;
		}

		/// <summary>
		/// Adds a the child Node to the list of children in this Node.
		/// Also updates the child Node's parent to point to this Node.
		/// </summary>
		/// <param name="child"></param>
		public void AddChild( Node child )
		{
			if ( children == null )
			{
				children = new ArrayList();
			}

			children.Add( child );

			child.parent = this;
		}

		#region label stuff
		
		/// <summary>
		/// Retrieves a label from the <see cref="Element"/> underlying this <see cref="Node"/> based on 
		/// a parameter-supplied language and role. Updates <see cref="label"/>.
		/// </summary>
		/// <param name="lang">The language code of the label to be retrieved.</param>
		/// <param name="role">The label role of the label to be retrieved.</param>
		internal void SetLabel(string lang, string role)
		{
			if ( !TryGetLabel( lang, role, out label ) )
			{
				bool checkForLabelRole = true;
				StringBuilder sb = new StringBuilder( "[" );

				if(role != null)
				{
					// try "label" role (if not already attempted)
					if(role.CompareTo(LABELROLE) == 0)
					{
						checkForLabelRole = false;
					}
				}

				if(checkForLabelRole)
				{
					if(TryGetLabel( lang, LABELROLE, out label ) )
					{
						//put the label in brackets if the current role is not preferredLabel
						if (PREFERRED_LABEL_ROLE.CompareTo(role) != 0)
						{
							sb.Append( label ).Append( "]" );
							label = sb.ToString();
						}
						return;
					}
				}

				// default to element name
				sb.Append( e.Name ).Append( "]" );
				
				this.label = sb.ToString();
			}
		}

		/// <summary>
		/// Retrieves label from the <see cref="Element"/> underlying this <see cref="Node"/> based on 
		/// a parameter-supplied language and role.
		/// </summary>
		/// <param name="lang">The language code of the label to be retrieved.</param>
		/// <param name="role">The label role of the label to be retrieved.</param>
		/// <param name="text">An output parameter.  The label text.</param>
		/// <returns>True if the label could be retrieved.</returns>
		public bool TryGetLabel(string lang, string role, out string text)
		{
			text = null;

			if(role.CompareTo( TraceUtility.FormatStringResource("XBRLAddin.PreferredLabelRole") ) == 0)
			{
				//prefLabel may contain the preferred label role i.e. terse, total
				//if prefLabel is not null, use it to get label text
				if( prefLabel != null && prefLabel.Length > 0  )
				{
					return e.TryGetLabel( lang, prefLabel, out text );
				}

				//no preferred label, just return false (outer method will provide another role)
				return false;
			}

			return e.TryGetLabel( lang, role, out text );
		}

		public string GetLabelWithoutLeadingCharacters()
		{
			if ( !string.IsNullOrEmpty( this.label ) )
			{
				if ( this.label.Length > 1 && LEADING_CHARS_TO_STRIP.Contains( this.label.Substring( 0, 1 ) ) )
				{
					return this.label.Substring( 1, Label.Length - 1 );
				}
				else
				{
					return this.label;
				}
			}
			else
			{
				return label;
			}
		}

		#endregion

		#region references

		/// <summary>
		/// Returns a <see cref="String"/> representation of the references for this <see cref="Node"/>'s 
		/// <see cref="Element"/>.
		/// </summary>
		/// <param name="references">An output parameter.  The <see cref="String"/> representation.</param>
		/// <returns>Always true.</returns>
		public bool TryGetReferences(out string references)
		{
			references = "";
			StringBuilder sbReference = new StringBuilder();

			if (e == null)
			{
				references = "";
				return false;
			}
			else
			{
				return e.TryGetReferences(out references);
			}
		}
		#endregion

		#region search

		/// <summary>
		/// This method will search the content of the node based on the
		/// options specified
		/// </summary>
		public bool Search(string searchString, bool searchID, bool searchLabel, 
			               bool searchDefinition, bool searchReference, string lang)
			         
		{
			searchString = searchString.ToLower();

			string references = string.Empty;
			bool hasReferences = this.TryGetReferences(out references);
			bool hasDefinition = (this.e != null && this.GetDefinition(lang) != null && this.GetDefinition(lang).Length > 0);
			bool foundInReference = false;
			bool foundDefinition = false;

			if (searchReference && hasReferences)
			{
				foundInReference  = references.ToLower().IndexOf(searchString) >= 0;
			}

			if (searchDefinition && hasDefinition)
			{
				foundDefinition  = this.GetDefinition(lang).ToLower().IndexOf(searchString) >= 0;
			}

			if (foundInReference || foundDefinition)
			{
				return true;
			}
			else
			{
				return 
					(
					(searchID && (this.Name != null) && (this.Name.ToLower().IndexOf(searchString) >= 0)) ||
					(searchLabel && (this.Label != null) && (this.Label.ToLower().IndexOf(searchString) >= 0))
					);
			}
		}
		#endregion

		#region overrides
		/// <summary>
		/// Serves as a hash function for this instance of <see cref="Node"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that is the hash code for this instance of <see cref="Node"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Determines whether a supplied <see cref="Object"/> is equal to this <see cref="Node"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to be compared to this <see cref="Node"/>.  
		/// Assumed to be a <see cref="Node"/>.</param>
		/// <returns>True if <paramref name="obj"/> is equal to this <see cref="Node"/>.</returns>
		/// <remarks>To be equal, the following properties of the two <see cref="Node"/> objects must be equal.
		/// <bl>
		/// <li>Label.</li>
		/// <li>Tag.</li>
		/// <li>Parent.</li>
		/// <li>Element.</li>
		/// <li>Children count and definition.</li>
		/// </bl>
		/// </remarks>
		public override bool Equals(object obj)
		{
			if ( !(obj is Node ) )									return false;

			Node arg = (Node)obj;

			if ( label == null && arg.label != null  )				return false;
			if ( label != null && !label.Equals( arg.label ) )		return false;

			if (IsProhibited != arg.IsProhibited)					return false;
			if (order != arg.order)									return false;
			if (prefLabel != arg.prefLabel)							return false;

			if ( tag == null && arg.tag != null ) 					return false;
			if ( tag != null && !tag.Equals( arg.tag ) )			return false;

			if ( parent == null && arg.parent != null )				return false;
//			Ignore this test - can cause infinite recursion
//			if ( parent != null && !parent.Equals( arg.parent ) )	return false;

			if ( e == null && arg.e != null )						return false;
			if ( e != null && !e.Equals( arg.e ) )					return false;

			if ( children == null && arg.children != null )			return false;
			if ( children != null )
			{
				if ( arg.children == null )							return false;

				if ( children.Count != arg.children.Count )			return false;

				for( int i=0; i < children.Count; ++i )
				{
					if ( !children[i].Equals( arg.children[i] ) )	return false;
				}
			}

			return true;
		}
		#endregion

		#region xml string

		private void WriteHeader( int numTabs, StringBuilder xml )
		{
			for ( int i=0; i < numTabs; ++i )
			{
				xml.Append( "\t" );
			}

			xml.Append( "<element_" ).Append( numTabs-1 );
		}

		/// <summary>
		/// Writes this <see cref="Node"/> to a parameter-supplied XML <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="numTabs">The number of tab characters to be appended <paramref name="xml"/>
		/// before each XML node is written.</param>
		/// <param name="verbose">If true, key attributes are written with the XML.  See <see cref="Element.WriteXmlFragment"/>.</param>
		/// <param name="lang">The language code for which <see cref="LabelLocator"/> objects within
		/// this <see cref="Node"/> are to be written.  See <see cref="Element.WriteXmlFragment"/>.</param>
		/// <param name="xml">The output XML to which this <paramref name="Node"/> is to be appended.</param>
		public void ToXmlString(int numTabs, bool verbose, string lang, StringBuilder xml)
		{
			if ( e == null )
			{
				// must be a presentation link
				PresentationLink.WriteHeader( numTabs, label, xml );
			}
			else
			{
				// start this node
				WriteHeader( numTabs, xml );

				Element.WriteXmlFragment( e, verbose, lang, xml );
			}

			if ( HasChildren )
			{
				xml.Append( ">" ).Append( Environment.NewLine );	// terminate the current line

				foreach ( Node n in children )
				{
					n.ToXmlString( numTabs+1, verbose, lang, xml );
				}
			}
			
			if ( e == null )
			{
				// must be a presentation link
				PresentationLink.WriteFooter( numTabs, xml );
			}
			else
			{
				// terminate this node
				if ( HasChildren )
				{
					for ( int i=0; i < numTabs; ++i )
					{
						xml.Append( "\t" );
					}

					xml.Append( "</element_" );
				}
				else
				{
					xml.Append( "/>" );
				}

				xml.Append( Environment.NewLine );
			}
		}
		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="Node"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="Node"/>
		/// is to be compared.  Assumed to be a <see cref="Node"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="LabelDefinition"/>.</returns>
		/// <remarks>Result is equivalent to a <see cref="String.Compare(String, String)"/> on the labels 
		/// of the two <see cref="Node"/> objects.</remarks>
		public override int CompareTo(object obj)
		{
			//get the true labels (without the [)
			Node node = (obj as Node);
			string modifiedLabel = this.GetLabelWithoutLeadingCharacters();
			string modifiedObjLabel = node.GetLabelWithoutLeadingCharacters();

			return string.Compare (modifiedLabel, modifiedObjLabel);
		}

		#endregion

		#region updates
		/// <summary>
		/// Replaces key properties of the <see cref="Element"/> underlying this <see cref="Node"/> 
		/// with equivalent properties in a parameter-supplied <see cref="Element"/>.
		/// </summary>
		/// <param name="newElemInfo">The <see cref="Element"/> from which property values are 
		/// to be copied.</param>
		public void UpdateElement( Element newElemInfo )
		{
			e.Update( newElemInfo );
		}

		/// <summary>
		/// Adds the <see cref="LabelDefinition"/> objects within a supplied collection to the labels 
		/// collection of the <see cref="Element"/> underlying this <see cref="Node"/>.
		/// </summary>
		/// <param name="labels">An <see cref="Array"/> of <see cref="LabelDefinition"/>.</param>
		public void UpdateLabels(LabelDefinition[] labels)
		{
			e.UpdateLabels( labels );
		}

		/// <summary>
		/// Updates <see cref="label"/> with the label from the <see cref="Element"/> underlying 
		/// this <see cref="Node"/>.  Updates this <see cref="Node"/>'s preferred label property 
		/// with a supplied <see cref="String"/>.
		/// </summary>
		/// <param name="language">The language code to be used in retrieving the label from the 
		/// <see cref="Element"/>.</param>
		/// <param name="newPreferred">The new preferred label.</param>
		public void UpdatePreferredLabel( string language, string newPreferred )
		{
			this.prefLabel = newPreferred;

            if (!TryGetLabel(language, prefLabel, out label))
			{
				this.label = e.GetDefaultLabel( language );
			}
		}

		/// <summary>
		/// Retrieves a label from the <see cref="Element"/> underlying this <see cref="Node"/> based on 
		/// a parameter-supplied language and role. Updates <see cref="label"/>.
		/// </summary>
		/// <param name="language">The language code of the label to be retrieved.</param>
		/// <param name="role">The label role of the label to be retrieved.</param>
		public void UpdateLabel(string language, string role)
		{
			SetLabel( language, role);
		}
		#endregion

		/// <summary>
		/// used in ElementUserControl to match the BaseSchema of the presentation link
		/// </summary>
		public string GetNodeSchema(Taxonomy tax)
		{
			string nodeSchema = string.Empty;
			if (MyPresentationLink != null)
			{
				//presentation or calculation view
				nodeSchema = MyPresentationLink.BaseSchema;
			}
			else
			{
				//element view
				TaxonomyItem ti = tax.GetTaxonomyInfo(this);
				nodeSchema = ti.Location;
			}

			return nodeSchema;
		}
		/// <summary>
		/// recursively look at parent till we find the link
		/// </summary>
		/// <returns></returns>
		public PresentationLink GetPresentationLink()
		{
			if (this.presLink != null) return presLink;

			if (this.parent != null) return this.parent.GetPresentationLink();

			return null;
		}


		internal Node GetParent(string hRef)
		{
			if (this.Id.Equals(hRef)) return this;

			if (this.parent == null) return null;

			return this.parent.GetParent(hRef);
		}

		#region IComparable<Node> Members

		/// <summary>
		/// Compares this instance of <see cref="Node"/> to a supplied <see cref="Node"/>.
		/// </summary>
		/// <param name="other">A <see cref="Node"/> to which this instance of <see cref="Node"/>
		/// is to be compared.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="other"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="Node"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of <see cref="String.Compare(String, String)"/> 
		/// for the labels of the two <see cref="Node"/> objects.</remarks>
		public int CompareTo(Node other)
		{
			string modifiedLabel = this.GetLabelWithoutLeadingCharacters();
			string modifiedObjLabel = other.GetLabelWithoutLeadingCharacters();

			return string.Compare(modifiedLabel, modifiedObjLabel);
		}

		#endregion


        #region static helpers
        public static Node RecursivelyGetNode(ArrayList nodes, string Id )
        {
            foreach( Node n in nodes )
            {
                if (n.MyElement != null)
                {
                    if (n.Id == Id) return n;

                    if (n.children != null)
                    {
                        Node ret = RecursivelyGetNode(n.children, Id);
                        if (ret != null) return ret;
                    }
                }
            }

            return null;
        }
        #endregion


       
       /// <summary>
       /// walk up the node chain and get a list of all tuple elements in the chain
       /// zeroth being the innermost tuple.
       /// </summary>
       /// <returns></returns>
        public List<string> GetTupleParentList( )
        {
            List<string> ret = new List<string>();

            if (MyElement != null && this.MyElement.IsTuple)
            {
                ret.Add(this.Id);
            }

            if (this.parent != null)
            {

                parent.RecursivelySetTupleHierarchyInfo(this.Id, ref ret);
            }


            return ret.Count > 0 ? ret : null;
        }

        private void RecursivelySetTupleHierarchyInfo(string childId, ref List<string> tupleParentList)
        {
            if (this.MyElement == null) return;

            if (this.MyElement.IsTuple)
            {
                //check to see if the tuple is paretn to id
                if (MyElement.FindChildElement(childId) >= 0)
                {
                    tupleParentList.Add(MyElement.Id);
                    childId = MyElement.Id;
                }
                else
                {
                    return; //differnt tuple set is parent to this element
                }
            }

            if (this.parent != null)
            {
                parent.RecursivelySetTupleHierarchyInfo(childId, ref tupleParentList);
            }
        }

        /// <summary>
        /// Get the name of the currently selected tuple set
        /// </summary>
        /// <returns></returns>
        public string RecursivelyGetCurrentTupleSet()
        {
            if (this.currentTupleSetName != null) return this.currentTupleSetName;

            if (this.parent != null) return parent.RecursivelyGetCurrentTupleSet();

            return null;
        }

        /// <summary>
        /// get the top level node.. i.e the title node from any given node...
        /// </summary>
        /// <returns></returns>
        public Node RecursivelyGetParentNode()
        {
            if (this.parent == null) return this;

            return this.parent.RecursivelyGetParentNode();
        }

		/// <summary>
		/// Climbs up the node's Parents to build a string that represents the path to the 
		/// node in the Presentation hierarchy
		/// </summary>
		/// <returns>The path to the node in the Presentation hierarchy</returns>
		public string GetPresentationPath()
		{
			StringBuilder sbPath = new StringBuilder(this.Id);
			Node parentNode = this.parent;
			while (parentNode != null)
			{
				if (parentNode.ElementIsNull)
				{
					//We've reached the report level, add it to the path and break out of the loop
					sbPath.Insert(0, @"\").Insert(0, parentNode.presLink.Title);
					break;
				}
				else
				{
					sbPath.Insert(0, @"\").Insert(0, parentNode.Id);
					
				}
				parentNode = parentNode.parent;
			}
			return sbPath.ToString();
		}

        /// <summary>
        /// get the dimension node type of the current node
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public Boolean TryGetDimensionNodeType(out DimensionNode.NodeType nodeType)
        {
            Boolean nodeTypeFound = false;
            nodeType = DimensionNode.NodeType.Dimension;

            if (this is DimensionNode)
            {
                DimensionNode dimension = this as DimensionNode;
                if (dimension.NodeDimensionInfo == null) return false;
                nodeType = dimension.NodeDimensionInfo.NodeType;
                nodeTypeFound = true;
            }
            else
            {
                if (MyElement.IsDimensionItem() == true)
                {
                    nodeType = DimensionNode.NodeType.Dimension;
                    nodeTypeFound = true;
                }
                else if (MyElement.IsHyperCubeItem() == true)
                {
                    nodeType = DimensionNode.NodeType.Hypercube;
                    nodeTypeFound = true;
                }
            }

            return nodeTypeFound;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsHypercubeNodeInHierarchy()
		{
			if( this.MyElement.IsHyperCubeItem()) return true;
			if (this.children == null) return false;

			foreach (Node n in this.children)
			{
				if (n.IsProhibited) continue;

				if (n.IsHypercubeNodeInHierarchy()) return true;
			}

			return false;
		}


		internal string GethypercubeId()
		{
			if (this.MyElement.IsHyperCubeItem()) return this.Id;

			if (this.parent != null) return this.parent.GethypercubeId();

			return null;
		}


		public virtual void RecursivelyGetAllTargetRoleInfos(string currentRole,
			ref Dictionary<string, string> allTargetRoles)
		{

			if (this.IsProhibited) return ;

			if (this.children == null) return ;


			foreach (Node n in this.children)
			{
				n.RecursivelyGetAllTargetRoleInfos(currentRole, ref allTargetRoles);
			}

			return ;
		}


        public void RecursivelyGetAllDimensionAxisNodes(ref List<Node> dimensionAxisNodes)
        {
            if (this.IsProhibited) return;

            if (this.MyElement.IsDimensionItem())
            {
                dimensionAxisNodes.Add(this);
                return;
            }
            if (this.children != null)
            {
                foreach (Node cn in this.children)
                {
                    if (cn.IsProhibited) continue;

                    cn.RecursivelyGetAllDimensionAxisNodes(ref dimensionAxisNodes);
                }
            }

        }

    }


	/// <summary>
	/// Provides method and properties to sort <see cref="Node"/> objects.
	/// </summary>
	public class NodeOrderSorter : IComparer
	{
		#region IComparer Members

		/// <summary>
		/// Compare two <see cref="Object"/>s, assumed to be <see cref="Node"/>s.
		/// </summary>
		/// <param name="x">The first <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="Node"/>.</param>
		/// <param name="y">The second <see cref="Object"/> to be compared.  Assumed to be 
		/// a <see cref="Node"/>.</param>
		/// <returns><bl>
		/// <li>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>.</li>
		/// <li>Zero if <paramref name="x"/> is equal to <paramref name="y"/>.</li>
		/// <li>Greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.</li>
		/// </bl></returns>
		/// <remarks>Results returned are equivalent to <see cref="Double.CompareTo(Double)"/> on 
		/// the order properties of the two <see cref="Node"/> objects.</remarks>
		public int Compare(object x, object y)
		{
			Node a = x as Node;
			Node b = y as Node;

			return a.Order.CompareTo( b.Order );
		}

		#endregion
	}
}
using System;

namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// Encapsulates the properties of an HTML display node.
	/// </summary>
	[Serializable]
	public class HtmlNode : Node
	{
		#region properties
		internal int Depth = 0;
		internal int UseCount = 0;
		internal int WriteCount = 0;
		#endregion

		#region constructors

		/// <summary>
		/// Creates a new <see cref="HtmlNode"/>/
		/// </summary>
		public HtmlNode()
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new <see cref="HtmlNode"/>/
		/// </summary>
		/// <param name="title">The label (title) to be assigned to the newly created <see cref="HtmlNode"/>.</param>
		public HtmlNode(string title)
			: base(title)
		{
		}

		/// <summary>
		/// Overloaded.  Creates a new <see cref="HtmlNode"/>/
		/// </summary>
		/// <param name="level">The label to be assigned to the newly created <see cref="HtmlNode"/>.</param>
		/// <param name="href">The depth (level) to be assigned to the newly created <see cref="HtmlNode"/>.</param>
		public HtmlNode(string href, int level)
			: base(href)
		{
			Depth = level;
		}

		/// <summary>
		/// Overloaded.  Creates a new <see cref="HtmlNode"/>/
		/// </summary>
		/// <param name="e">The <see cref="Element"/> whose cloned element collection is to be search for 
		/// and <see cref="Element"/> whose parent id is equal to <paramref name="parentId"/>.</param>
		/// <param name="level">The label to be assigned to the newly created <see cref="HtmlNode"/>.</param>
		/// <param name="parentId">The parent id for which the element's in the cloned element collection of 
		/// <paramref name="e"/> is to be searched.</param>
		public HtmlNode(Element e, int level, string parentId)
			: base(e, parentId)
		{
			Depth = level;
		}
		#endregion

	}
}
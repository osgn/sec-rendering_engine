//=============================================================================
// ContextElementCount (class)
// Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
// This data class stores statistical information for context element count.
//=============================================================================

using System;

namespace XBRLReportBuilder
{
	/// <summary>
	/// ContextElementCount
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// 
	[Serializable]
	public class ContextElementCount
	{
        
		#region properties

		public string ContextName = string.Empty;
		public int ElementCount = 0;

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new ContextElementCount.
		/// </summary>
		public ContextElementCount()
		{
		}

		public ContextElementCount(string name, int count)
		{
			this.ContextName = name;
			this.ElementCount = count;
		}

		#endregion

	}
}
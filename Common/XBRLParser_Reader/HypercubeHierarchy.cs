using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Aucent.MAX.AXE.Common.Data;
namespace Aucent.MAX.AXE.XBRLParser
{
	/// <summary>
	/// HypercubeHierarchy
	/// </summary>
	[Serializable]
	public class HypercubeHierarchy : IComparable
	{
		#region Properties
		/// <summary>
		/// Valid end result = 
		/// ALL + Found = 5
		/// Not All + Not Found = 10
		/// ALL + Not Found + Dimension has default = 1+8+16 = 25
		/// All + not found + dimension not in cube + is not closed = 1+8+64+256 = 329
		/// Not All + not found + dimension not in cube + is not closed = 2+8+64+256 = 330
		/// </summary>
		private enum ValidationDataEnum
		{
			ALLRelationShip = 1,
			NotAllRelationShip = 2,
			Found = 4,
			NotFound = 8,
			DimensionHasDefault = 16,
			DimensionNotInInstance = 32,
			DimensionNotInCube = 64,
			IsClosed = 128,
			IsNotClosed = 256,
			DefaultValueUsed = 512
		}

		private static ListDictionary validResults = new ListDictionary();
		static HypercubeHierarchy()
		{
			//TODO: expand this set
			// all + closed + found = 1+4+128
			validResults[133] = 0;
			// all + not closed + found = 1 +4 + 256
			validResults[261] = 0;
			// not all + not found + closed = 2+8+ 128
			validResults[138] = 0;
			// not all + not found + not closed = 2+8+ 256
			validResults[266] = 0;
		}

		private string roleRef;
		private string RoleRef
		{
			get { return roleRef; }
			set { roleRef = value; }
		}

		private string hRef;
		private string HRef
		{
			get { return hRef; }
			set { hRef = value; }
		}

		private bool isEmpty = false;
		private bool IsEmpty
		{
			get { return isEmpty; }
			set { isEmpty = value; }
		}

		private DimensionNode hypercubeNode = null;
		/// <summary>
		/// The top level hypercube node
		/// </summary>
		public DimensionNode HypercubeNode
		{
			get { return hypercubeNode; }
			set { hypercubeNode = value; }
		}

		#endregion

		#region ctors
		/// <summary>
		/// Constructs a new instance of <see cref="HypercubeHierarchy"/>.
		/// </summary>
		/// <param name="role">Role reference to be assigned to new <see cref="HypercubeHierarchy"/>.</param>
		/// <param name="href">Href to be assigned to new <see cref="HypercubeHierarchy"/>.</param>
		public HypercubeHierarchy(string role, string href)
		{
			this.roleRef = role;
			this.hRef = href;
		}
		#endregion


		#region Public methods

		private bool IsValidMarkup(ContextProperty cp,
			Dimension.MeasureHyperCubeInfo mhci,
			ref ArrayList errors)
		{
			BuildValidMembersByDimension();
			ListDictionary instanceDimensionInfos = new ListDictionary();
			if (mhci.IsScenario)
			{
				foreach (Scenario sc in cp.Scenarios)
				{
					if (sc.DimensionInfo != null)
					{
						instanceDimensionInfos[sc.DimensionInfo.dimensionId] = sc.DimensionInfo;
					}
				}
			}
			else
			{
				//TODO: same thing for segments..
			}

			ListDictionary validationByDimension = new ListDictionary();

			int baseValidationData = 0;
			if (mhci.IsAllRelationShip)
			{
				baseValidationData += ValidationDataEnum.ALLRelationShip.GetHashCode();
			}
			else
			{
				baseValidationData += ValidationDataEnum.NotAllRelationShip.GetHashCode();

			}

			if (mhci.IsClosed)
			{
				baseValidationData += ValidationDataEnum.IsClosed.GetHashCode();

			}
			else
			{
				baseValidationData += ValidationDataEnum.IsNotClosed.GetHashCode();

			}

			foreach (DictionaryEntry de in validMembersByDimension)
			{
				int validationData = baseValidationData;
				bool found = false;
				bool hasDefault = false;

				ContextDimensionInfo cdi = instanceDimensionInfos[de.Key] as ContextDimensionInfo;
				instanceDimensionInfos.Remove(de.Key);
				if (cdi == null)
				{
					validationData += ValidationDataEnum.DimensionNotInInstance.GetHashCode();
				}
				else
				{
					HybridDictionary validMembers = de.Value as HybridDictionary;
					if (validMembers != null)
					{
						found = validMembers[cdi.Id] != null;
					}
				}

				if (found)
				{
					validationData += ValidationDataEnum.Found.GetHashCode();

				}
				else
				{
					validationData += ValidationDataEnum.NotFound.GetHashCode();

				}

				if (hasDefault)
				{
					validationData += ValidationDataEnum.DimensionHasDefault.GetHashCode();

					//TODO: determine if the default value is being used 
					//which is not a good thing and is an error condition.
				}

				ValidateResult(validationData, errors);

			}

			if (instanceDimensionInfos.Keys.Count != 0)
			{
				if (mhci.IsClosed)
				{
					//this is an error condition as we have dimensions that are not part of
					//the cube in the instance but the cube is a closed cube and does not
					//allow other dimension infos
					//TODO: enhance the error message...
					string error = "Found invalid dimension for an element that is using a closed hypercube";

					errors.Add(error);
				}
			}

			return errors.Count == 0;
		}


		#endregion


		#region Build Valid Memebers by Dimension
		private void ValidateResult(int validationNumber, ArrayList errors)
		{
			if (validResults[validationNumber] != null)
			{
				//data is valid
				return;
			}

			//TODO: need to determine the exact nature of the problem.
			string error = "Invalid Dimension info Found";
			errors.Add(error);
		}

		/// <summary>
		/// key = dim href, value = hashtable
		/// </summary>
		ListDictionary validMembersByDimension;
		private void BuildValidMembersByDimension()
		{
			//if validMembersByDimension is not null then we have already built the 
			//validation information for this hypercube.
			if (validMembersByDimension != null) return;
			validMembersByDimension = new ListDictionary();
			if (hypercubeNode.Children == null) return;
			foreach (DimensionNode dn in this.hypercubeNode.Children)
			{
				HybridDictionary validMembers = validMembersByDimension[dn.Id] as HybridDictionary;

				if (validMembers == null)
				{
					validMembers = new HybridDictionary();
					validMembersByDimension[dn.Id] = validMembers;
				}
				BuildValidMembersByDimension(validMembers, dn.Children);
			}

		}

		private void BuildValidMembersByDimension(HybridDictionary validMembers, ArrayList childrenNodes)
		{

			if (childrenNodes == null) return;

			foreach (DimensionNode dn in childrenNodes)
			{
				if (dn.NodeDimensionInfo != null && dn.NodeDimensionInfo.Usable)
				{
					validMembers[dn.Id] = dn;
				}
				BuildValidMembersByDimension(validMembers, dn.Children);
			}
		}


		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this instance of <see cref="HypercubeHierarchy"/> to a supplied <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">An <see cref="object"/> to which this instance of <see cref="HypercubeHierarchy"/>
		/// is to be compared.  Assumed to be a <see cref="HypercubeHierarchy"/>.</param>
		/// <returns>An <see cref="int"/> indicating if <paramref name="obj"/> is less than (&lt;0),
		/// greater than (>0), or equal to (0) this instance of <see cref="HypercubeHierarchy"/>.</returns>
		/// <remarks>This comparison is equivalent to the results of CompareTo for the role references and 
		/// URI (hRef) of the two <see cref="HypercubeHierarchy"/> objects.</remarks>
		public int CompareTo(object obj)
		{
			HypercubeHierarchy other = obj as HypercubeHierarchy;

			int ret = this.roleRef.CompareTo(other.roleRef);
			if (ret != 0) return ret;

			return this.hRef.CompareTo(other.hRef);
		}

		#endregion
	}
}

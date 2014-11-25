//=============================================================================
// InstanceStatistics (class)
// Copyright © 2006-2011 Rivet Software, Inc. All rights reserved.
// This data class contains statistical information for a filing.
//=============================================================================

using System;
using System.Collections;
using Aucent.MAX.AXE.XBRLParser;
using Aucent.MAX.AXE.Common.Data;

namespace XBRLReportBuilder
{
	/// <summary>
	/// InstanceStatistics
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class InstanceStatistics
	{
        
		#region properties

		private Instance currentInstance = null;

		public int NumberOfEntities = 0;
		public int NumberOfElements = 0;
		public int NumberOfUnitRefs = 0;
		public int NumberOfContexts = 0;
		public int NumberOfSegments = 0;
		public int NumberOfScenarios = 0;
		public bool HasFootnotes = false;
		public bool HasTuples = false;

		public ArrayList EntityIDList = new ArrayList();

		
		/// <summary>
		/// Key -- context name
		/// Value -- element array
		/// </summary>
		public Hashtable ElementsPerContext = new Hashtable();

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new InstanceStatistics.
		/// </summary>
		public InstanceStatistics(Instance currentInstance)
		{
			this.currentInstance = currentInstance;

			if (this.currentInstance != null)
			{
				BuildStatistics();
			}
		}

		private void BuildStatistics()
		{
			ElementsPerContext = new Hashtable();

			NumberOfUnitRefs = currentInstance.units.Count;
			NumberOfContexts = currentInstance.contexts.Count;

			Hashtable allElements = new Hashtable();
			Hashtable allEntities = new Hashtable();
			Hashtable allSegments = new Hashtable();
			Hashtable allScenarios = new Hashtable();
			EntityIDList = new ArrayList();

			foreach (MarkupProperty mp in currentInstance.markups)
			{
				if (mp!= null && mp.elementId.Length > 0)
				{
					allElements[mp.elementId] = mp.element;
					
					if (mp.HasFootnotes)
					{
						this.HasFootnotes = true;
					}
					
					if (mp.contextRef != null)
					{
						//Context
						string contextName = mp.contextRef.ContextID;
						if (ElementsPerContext[mp.contextRef] == null)
						{
							ElementsPerContext[mp.contextRef] = new ArrayList();
						}
						(ElementsPerContext[mp.contextRef] as ArrayList).Add (mp.element);

						//Entity
						allEntities[mp.contextRef.EntitySchema + mp.contextRef.EntityValue] = mp.contextRef.EntityValue;

						//Segments
						if (mp.contextRef.Segments != null && mp.contextRef.Segments.Count > 0)
						{
							foreach (Segment s in mp.contextRef.Segments)
							{
								allSegments[s.Schema + s.ValueType + s.ValueName] = s.ValueType + s.ValueName;
							}
						}

						//Scenarios
						if (mp.contextRef.Scenarios != null && mp.contextRef.Scenarios.Count > 0)
						{
							foreach (Scenario s in mp.contextRef.Scenarios)
							{
								allScenarios[s.Schema + s.ValueType + s.ValueName] = s.ValueType + s.ValueName;
							}
						}

					}
				}
			}

			NumberOfElements = allElements.Count;
			NumberOfEntities = allEntities.Count;
			foreach (string eID in allEntities.Values)
			{
				EntityIDList.Add (eID);
			}
			NumberOfSegments = allSegments.Count;
			NumberOfScenarios = allScenarios.Count;

			HasTuples = (this.currentInstance.DocumentTupleList != null && this.currentInstance.DocumentTupleList.Count > 0);

		}

		#endregion

	}
}
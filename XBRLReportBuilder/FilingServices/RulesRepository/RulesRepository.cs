/*****************************************************************************
 * RulesRepository (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class is used both as a wrapper for the NxBRE rules engine, and as a meens of
 * defining a set of rules for a given process.  Rules within the set can be added/removed
 * enabled/disabled and executed.  The list itself can be saved to XML allowing the set to
 * be persisted and retrieved at run time.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;


using NxBRE.FlowEngine;
using NxBRE.FlowEngine.Factories;
using NxBRE.FlowEngine.IO;
using System.Xml;
using System.Xml.XPath;

namespace Aucent.FilingServices.RulesRepository
{
	public class RulesRepository
	{
		private BREFactory theEngineFactory = new BREFactory();
		private DirectoryInfo sourceDir = null;
		

		private string name;
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		private Dictionary<string, Rule> myRules = new Dictionary<string, Rule>();
		public Dictionary<string, Rule> MyRules
		{
			get { return this.myRules; }
		}

		public RulesRepository(string repositoryName, DirectoryInfo rulesFolder)
		{
			this.name = repositoryName;
			this.sourceDir = rulesFolder;
		}

		public bool TryLoadExistingRulesList()
		{
			try
			{
				if( !this.sourceDir.Exists )
					return false;

				string fileName = Path.Combine( this.sourceDir.FullName, this.name +".rul" );
				if( !File.Exists( fileName ) )
					return false;


				object objRules;
				string errorMsg;
				if( !XmlUtilities.TryXmlDeserializeObjectFromFile( fileName, typeof( Rule[] ), out objRules, out errorMsg ) )
					return false;


				myRules.Clear();
				Rule[] rulesArray = objRules as Rule[];
				foreach( Rule rule in rulesArray )
				{
					this.myRules.Add( rule.FriendlyName, rule );
				}
				
				return true;
			}
			catch{}

			return false;
		}

		public bool TryLoadNewRulesList()
		{
			if (this.sourceDir.Exists)
			{
				FileInfo[] xmlFiles = this.sourceDir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
				foreach (FileInfo xmlFile in xmlFiles)
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(xmlFile.FullName);
					XmlNode node = xmlDoc.SelectSingleNode("/xBusinessRules");

					//There should always be 2 nodes in the document, the XML node, and the root object node.
					if (node != null)
					{
						Rule currentRule = new Rule();
						currentRule.FriendlyName = Path.GetFileNameWithoutExtension(xmlFile.Name);
						currentRule.RuleFile = xmlFile.Name;
						currentRule.Enabled = true;
						currentRule.IsRequired = true;

						myRules.Add(currentRule.FriendlyName, currentRule);
					}
				}
				return true;
			}
			return false;
		}

		public bool TrySaveRulesList()
		{
			string errorMsg;
			string fileName = Path.Combine( this.sourceDir.FullName, this.name +".rul");

			List<Rule> rules = new List<Rule>(this.myRules.Values);
			return XmlUtilities.TryXmlSerializeObjectToFile(fileName, rules.ToArray(), out errorMsg);
		}

        public bool IsRuleEnabled(string ruleName)
		{
			bool retValue = false;
			if (myRules.ContainsKey(ruleName))
			{
				retValue = myRules[ruleName].Enabled;
			}
			return retValue;
		}

		public bool ProcessRule( string ruleName, Dictionary<string, object> contextObjects )
		{
			IBRERuleResult ruleResult = null;
			return ProcessRule( ruleName, contextObjects, out ruleResult );
		}

		public bool ProcessRule(string ruleName, Dictionary<string, object> contextObjects, out IBRERuleResult ruleResult )
		{
			ruleResult = null;
			bool processed = false;

			if (myRules.ContainsKey(ruleName))
			{
				string rulePath = Path.Combine( this.sourceDir.FullName, myRules[ruleName].RuleFile);

				XBusinessRulesFileDriver driver = new XBusinessRulesFileDriver( rulePath );
				IFlowEngine flowEngine = theEngineFactory.NewBRE( driver );
				flowEngine.ResultHandlers += new DispatchRuleResult( flowEngine_ResultHandlers );

				foreach (string ruleKey in contextObjects.Keys)
				{
					flowEngine.RuleContext.SetObject(ruleKey, contextObjects[ruleKey]);
				}

				this.ruleResult = null;
				processed = flowEngine.Process();
				ruleResult = this.ruleResult;
			}

			return processed;
		}

		private IBRERuleResult ruleResult = null;
		private void flowEngine_ResultHandlers( object sender, IBRERuleResult ruleResult )
		{
			this.ruleResult = ruleResult;
		}
	}
}

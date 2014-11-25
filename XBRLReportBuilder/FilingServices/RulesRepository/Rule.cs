/*****************************************************************************
 * Rule (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * This class defines an object that defines a rule which can be run using the
 * NxBre rules engine.  The object contains the name of the rule, the name of the
 * xml file that contains the definition of the rule, whether or not the rule is 
 * required for processing and if the rule is enabled.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using NxBRE.FlowEngine;

namespace Aucent.FilingServices.RulesRepository
{
	public class Rule
	{
		
		private string friendlyName;
		public string FriendlyName
		{
			get { return friendlyName; }
			set { friendlyName = value; }
		}

		private string ruleFile;
		public string RuleFile
		{
			get { return ruleFile; }
			set { ruleFile = value; }
		}

		private bool isRequired;
		public bool IsRequired
		{
			get { return isRequired; }
			set { isRequired = value; }
		}
		
		private bool enabled;
		public bool Enabled
		{
			get { return enabled;}
			set { enabled = value;}
		}

		private Dictionary<string, object> contextObjects = new Dictionary<string, object>();
		[XmlIgnore]
		public Dictionary<string, object> ContextObjects
		{
			get { return contextObjects; }
			set { contextObjects = value; }
		}

		public Rule()
		{ }
	}
}

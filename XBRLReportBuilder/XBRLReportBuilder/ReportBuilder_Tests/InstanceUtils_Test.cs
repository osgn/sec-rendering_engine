using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Aucent.MAX.AXE.XBRLParser;
using XRB = XBRLReportBuilder;

namespace Aucent.MAX.AXE.XBRLReportBuilder.Test
{
	public partial class RB_Tests
	{
		public class InstanceUtils_Test
		{
			[Test]
			public void IsDisplayReversed()
			{
				foreach( string role in XRB.InstanceUtils.ExtendedNegatedRoles )
				{
					Node n = new Node();
					n.PreferredLabel = role;
					Assert.IsTrue( XRB.InstanceUtils.IsDisplayReversed( n ) );
				}
			}
		}
	}
}
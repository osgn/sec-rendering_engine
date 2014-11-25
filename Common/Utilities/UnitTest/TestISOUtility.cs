using System.Collections.Generic;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.Common.Utilities.UnitTest
{
    [TestFixture]
    public class TestISOUtility
    {
        [Test]
        public void SICCodes()
        {
            List<ISOUtility.SICReference> sics = ISOUtility.GetSICList();

            //Check the sort order
            Assert.AreEqual("010",sics[0].SICCode);
            Assert.AreEqual("011",sics[1].SICCode);
            Assert.AreEqual("0111",sics[2].SICCode);

            Assert.IsNotNull(sics);
        }
    }
}
#endif

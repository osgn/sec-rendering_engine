using Aucent.MAX.AXE.Common.Data.ElementUsage;
using NUnit.Framework;

#if UNITTEST
namespace Aucent.MAX.AXE.Common.Data.UnitTest
{
    [TestFixture]
    public class TestElementUsageManager
    {
        [Test]
        public void GetUsageDataForElement()
        {
            ElementUsageManager manager = new ElementUsageManager();
            ElementUsageDataLoader loader = new ElementUsageDataLoader(manager);

            loader.LoadSICUsageData("S:\\bin\\TEST_SICUsage.xml");

            Element e = manager.GetUsageDataForElement("usfr-pte_AdditionalPaidCapital", "3572");

            Assert.IsNotNull(e);

            Assert.AreEqual(2,e.UsedByCompanyCount);
            Assert.AreEqual(100.00000000000M,e.UsagePercent);
        }

        [Test]
        public void GetUsageDataForElement_ThreeDigitSIC()
        {
            ElementUsageManager manager = new ElementUsageManager();
            ElementUsageDataLoader loader = new ElementUsageDataLoader(manager);

            loader.LoadSICUsageData("S:\\bin\\TEST_SICUsage.xml");

            Element e = manager.GetUsageDataForElement("usfr-pte_AccountsPayable", "357");

            Assert.IsNotNull(e);

            Assert.AreEqual(4, e.UsedByCompanyCount);
            Assert.AreEqual(57.142857142857142857142857140M, e.UsagePercent);
        }
    }
}
#endif

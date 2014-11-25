using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
    public class FilingStatistics
    {
        public string SICCode { get; set; }

        public int ElementCount { get; set; }
        public int ExtendedElementCount { get; set; }
        public int DimensionCount { get; set; }
        public int TotalMarkupCount { get; set; }
    }
}

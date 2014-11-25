using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
    public class ValidationCalculationItem
    {
        public ValidationCalculationItem(string elementId, string value, string category, int sequenceNumber)
        {
            this.ElementId = elementId;
            this.Value = value;
            this.Category = category;
            this.SequenceNumber = sequenceNumber;
        }

        public string ElementId { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public int SequenceNumber { get; set; }
    }
}

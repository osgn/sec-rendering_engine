using System;
using System.Collections.Generic;
using System.Text;

namespace Aucent.MAX.AXE.Common.Data
{
    public interface IDisplayStatus
    {
        void SetStatus(string message);
        void ShowErrorMessage(string message);
    }
}

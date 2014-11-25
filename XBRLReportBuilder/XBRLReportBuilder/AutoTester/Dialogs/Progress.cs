using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Dialogs
{
    public partial class Progress : Form
    {
        public string Status = "";
        public double Percentage = 0;

        public Progress()
        {
            InitializeComponent();
        }
    }
}

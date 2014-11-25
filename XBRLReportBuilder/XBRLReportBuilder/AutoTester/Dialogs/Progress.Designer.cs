namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Dialogs
{
    partial class Progress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.pgbOverall = new System.Windows.Forms.ProgressBar();
			this.lblStatus = new System.Windows.Forms.Label();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// pgbOverall
			// 
			this.pgbOverall.Location = new System.Drawing.Point(12, 25);
			this.pgbOverall.Name = "pgbOverall";
			this.pgbOverall.Size = new System.Drawing.Size(337, 23);
			this.pgbOverall.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.pgbOverall.TabIndex = 0;
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(9, 9);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(43, 13);
			this.lblStatus.TabIndex = 1;
			this.lblStatus.Text = "Status: ";
			// 
			// cmdCancel
			// 
			this.cmdCancel.Location = new System.Drawing.Point(274, 54);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 2;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// Progress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(361, 90);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.pgbOverall);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Progress";
			this.Text = "Progress";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label lblStatus;
		public System.Windows.Forms.ProgressBar pgbOverall;
		public System.Windows.Forms.Button cmdCancel;
    }
}
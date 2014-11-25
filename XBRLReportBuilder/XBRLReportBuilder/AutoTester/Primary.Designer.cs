namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester
{
    partial class Primary
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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem();
			this.lblBaseFilings = new System.Windows.Forms.Label();
			this.cmdBaseFilings = new System.Windows.Forms.Button();
			this.cmdNewFilings = new System.Windows.Forms.Button();
			this.txtNewFilings = new System.Windows.Forms.TextBox();
			this.lblNewFilings = new System.Windows.Forms.Label();
			this.cmdLogs = new System.Windows.Forms.Button();
			this.txtLogs = new System.Windows.Forms.TextBox();
			this.lblLogs = new System.Windows.Forms.Label();
			this.cmdRun = new System.Windows.Forms.Button();
			this.folderPicker = new System.Windows.Forms.FolderBrowserDialog();
			this.txtBaseFilings = new System.Windows.Forms.TextBox();
			this.lblMaxWorkers = new System.Windows.Forms.Label();
			this.txtMaxWorkers = new System.Windows.Forms.TextBox();
			this.txtOutput = new System.Windows.Forms.ListView();
			this.columnHeader1 = ( (System.Windows.Forms.ColumnHeader)( new System.Windows.Forms.ColumnHeader() ) );
			this.columnHeader2 = ( (System.Windows.Forms.ColumnHeader)( new System.Windows.Forms.ColumnHeader() ) );
			this.columnHeader3 = ( (System.Windows.Forms.ColumnHeader)( new System.Windows.Forms.ColumnHeader() ) );
			this.columnHeader4 = ( (System.Windows.Forms.ColumnHeader)( new System.Windows.Forms.ColumnHeader() ) );
			this.lblDifferences = new System.Windows.Forms.Label();
			this.lblDifferencesCount = new System.Windows.Forms.Label();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.webBrowser1 = new System.Windows.Forms.WebBrowser();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblBaseFilings
			// 
			this.lblBaseFilings.AutoSize = true;
			this.lblBaseFilings.Location = new System.Drawing.Point( 3, 4 );
			this.lblBaseFilings.Name = "lblBaseFilings";
			this.lblBaseFilings.Size = new System.Drawing.Size( 238, 13 );
			this.lblBaseFilings.TabIndex = 0;
			this.lblBaseFilings.Text = "Base Filings (containing zips or folders of R Files):";
			// 
			// cmdBaseFilings
			// 
			this.cmdBaseFilings.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.cmdBaseFilings.Location = new System.Drawing.Point( 413, 19 );
			this.cmdBaseFilings.Name = "cmdBaseFilings";
			this.cmdBaseFilings.Size = new System.Drawing.Size( 75, 20 );
			this.cmdBaseFilings.TabIndex = 2;
			this.cmdBaseFilings.Text = "Browse...";
			this.cmdBaseFilings.UseVisualStyleBackColor = true;
			this.cmdBaseFilings.Click += new System.EventHandler( this.cmdBaseZip_Click );
			// 
			// cmdNewFilings
			// 
			this.cmdNewFilings.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.cmdNewFilings.Location = new System.Drawing.Point( 413, 58 );
			this.cmdNewFilings.Name = "cmdNewFilings";
			this.cmdNewFilings.Size = new System.Drawing.Size( 75, 20 );
			this.cmdNewFilings.TabIndex = 5;
			this.cmdNewFilings.Text = "Browse...";
			this.cmdNewFilings.UseVisualStyleBackColor = true;
			this.cmdNewFilings.Click += new System.EventHandler( this.cmdNewZip_Click );
			// 
			// txtNewFilings
			// 
			this.txtNewFilings.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.txtNewFilings.Location = new System.Drawing.Point( 3, 59 );
			this.txtNewFilings.Name = "txtNewFilings";
			this.txtNewFilings.Size = new System.Drawing.Size( 404, 20 );
			this.txtNewFilings.TabIndex = 4;
			// 
			// lblNewFilings
			// 
			this.lblNewFilings.AutoSize = true;
			this.lblNewFilings.Location = new System.Drawing.Point( 3, 43 );
			this.lblNewFilings.Name = "lblNewFilings";
			this.lblNewFilings.Size = new System.Drawing.Size( 236, 13 );
			this.lblNewFilings.TabIndex = 3;
			this.lblNewFilings.Text = "New Filings (containing zips or folders of R Files):";
			// 
			// cmdLogs
			// 
			this.cmdLogs.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.cmdLogs.Location = new System.Drawing.Point( 413, 97 );
			this.cmdLogs.Name = "cmdLogs";
			this.cmdLogs.Size = new System.Drawing.Size( 75, 20 );
			this.cmdLogs.TabIndex = 14;
			this.cmdLogs.Text = "Browse...";
			this.cmdLogs.UseVisualStyleBackColor = true;
			this.cmdLogs.Click += new System.EventHandler( this.cmdLogs_Click );
			// 
			// txtLogs
			// 
			this.txtLogs.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.txtLogs.Location = new System.Drawing.Point( 3, 98 );
			this.txtLogs.Name = "txtLogs";
			this.txtLogs.Size = new System.Drawing.Size( 404, 20 );
			this.txtLogs.TabIndex = 13;
			// 
			// lblLogs
			// 
			this.lblLogs.AutoSize = true;
			this.lblLogs.Location = new System.Drawing.Point( 3, 82 );
			this.lblLogs.Name = "lblLogs";
			this.lblLogs.Size = new System.Drawing.Size( 185, 13 );
			this.lblLogs.TabIndex = 12;
			this.lblLogs.Text = "Logs Folder (contents will be deleted):";
			// 
			// cmdRun
			// 
			this.cmdRun.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.cmdRun.Font = new System.Drawing.Font( "Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
			this.cmdRun.ForeColor = System.Drawing.Color.Green;
			this.cmdRun.Location = new System.Drawing.Point( 405, 466 );
			this.cmdRun.Name = "cmdRun";
			this.cmdRun.Size = new System.Drawing.Size( 83, 28 );
			this.cmdRun.TabIndex = 15;
			this.cmdRun.Text = "Run";
			this.cmdRun.UseVisualStyleBackColor = true;
			this.cmdRun.Click += new System.EventHandler( this.cmdRun_Click );
			// 
			// txtBaseFilings
			// 
			this.txtBaseFilings.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.txtBaseFilings.Location = new System.Drawing.Point( 3, 20 );
			this.txtBaseFilings.Name = "txtBaseFilings";
			this.txtBaseFilings.Size = new System.Drawing.Size( 404, 20 );
			this.txtBaseFilings.TabIndex = 1;
			// 
			// lblMaxWorkers
			// 
			this.lblMaxWorkers.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
			this.lblMaxWorkers.AutoSize = true;
			this.lblMaxWorkers.Location = new System.Drawing.Point( 4, 474 );
			this.lblMaxWorkers.Name = "lblMaxWorkers";
			this.lblMaxWorkers.Size = new System.Drawing.Size( 73, 13 );
			this.lblMaxWorkers.TabIndex = 17;
			this.lblMaxWorkers.Text = "Max Workers:";
			// 
			// txtMaxWorkers
			// 
			this.txtMaxWorkers.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
			this.txtMaxWorkers.Location = new System.Drawing.Point( 87, 471 );
			this.txtMaxWorkers.MaxLength = 2;
			this.txtMaxWorkers.Name = "txtMaxWorkers";
			this.txtMaxWorkers.Size = new System.Drawing.Size( 36, 20 );
			this.txtMaxWorkers.TabIndex = 18;
			this.txtMaxWorkers.Text = "2";
			this.txtMaxWorkers.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.txtMaxWorkers_KeyPress );
			// 
			// txtOutput
			// 
			this.txtOutput.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
						| System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.txtOutput.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4} );
			this.txtOutput.HideSelection = false;
			this.txtOutput.Items.AddRange( new System.Windows.Forms.ListViewItem[] {
            listViewItem1} );
			this.txtOutput.Location = new System.Drawing.Point( 3, 124 );
			this.txtOutput.MultiSelect = false;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.Size = new System.Drawing.Size( 485, 336 );
			this.txtOutput.TabIndex = 19;
			this.txtOutput.UseCompatibleStateImageBehavior = false;
			this.txtOutput.View = System.Windows.Forms.View.Details;
			//this.txtOutput.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.txtOutput_MouseDoubleClick );
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Filing";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Success";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Errors";
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Reason";
			// 
			// lblDifferences
			// 
			this.lblDifferences.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
			this.lblDifferences.AutoSize = true;
			this.lblDifferences.Location = new System.Drawing.Point( 153, 474 );
			this.lblDifferences.Name = "lblDifferences";
			this.lblDifferences.Size = new System.Drawing.Size( 64, 13 );
			this.lblDifferences.TabIndex = 20;
			this.lblDifferences.Text = "Differences:";
			// 
			// lblDifferencesCount
			// 
			this.lblDifferencesCount.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
			this.lblDifferencesCount.AutoSize = true;
			this.lblDifferencesCount.Location = new System.Drawing.Point( 223, 474 );
			this.lblDifferencesCount.Name = "lblDifferencesCount";
			this.lblDifferencesCount.Size = new System.Drawing.Size( 15, 13 );
			this.lblDifferencesCount.TabIndex = 21;
			this.lblDifferencesCount.Text = "0";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point( 0, 0 );
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add( this.lblDifferencesCount );
			this.splitContainer1.Panel1.Controls.Add( this.lblBaseFilings );
			this.splitContainer1.Panel1.Controls.Add( this.lblDifferences );
			this.splitContainer1.Panel1.Controls.Add( this.cmdNewFilings );
			this.splitContainer1.Panel1.Controls.Add( this.txtOutput );
			this.splitContainer1.Panel1.Controls.Add( this.txtLogs );
			this.splitContainer1.Panel1.Controls.Add( this.cmdLogs );
			this.splitContainer1.Panel1.Controls.Add( this.txtMaxWorkers );
			this.splitContainer1.Panel1.Controls.Add( this.txtNewFilings );
			this.splitContainer1.Panel1.Controls.Add( this.txtBaseFilings );
			this.splitContainer1.Panel1.Controls.Add( this.lblLogs );
			this.splitContainer1.Panel1.Controls.Add( this.lblMaxWorkers );
			this.splitContainer1.Panel1.Controls.Add( this.cmdRun );
			this.splitContainer1.Panel1.Controls.Add( this.cmdBaseFilings );
			this.splitContainer1.Panel1.Controls.Add( this.lblNewFilings );
			this.splitContainer1.Panel1MinSize = 496;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add( this.webBrowser1 );
			this.splitContainer1.Size = new System.Drawing.Size( 737, 504 );
			this.splitContainer1.SplitterDistance = 496;
			this.splitContainer1.SplitterWidth = 5;
			this.splitContainer1.TabIndex = 22;
			// 
			// webBrowser1
			// 
			this.webBrowser1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
						| System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.webBrowser1.Location = new System.Drawing.Point( 6, 6 );
			this.webBrowser1.Margin = new System.Windows.Forms.Padding( 6 );
			this.webBrowser1.MinimumSize = new System.Drawing.Size( 20, 20 );
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size( 224, 492 );
			this.webBrowser1.TabIndex = 0;
			// 
			// Primary
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 737, 504 );
			this.Controls.Add( this.splitContainer1 );
			this.MinimumSize = new System.Drawing.Size( 400, 400 );
			this.Name = "Primary";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "AutoTester";
			this.Load += new System.EventHandler( this.Primary_Load );
			this.splitContainer1.Panel1.ResumeLayout( false );
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout( false );
			this.splitContainer1.ResumeLayout( false );
			this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Label lblBaseFilings;
        private System.Windows.Forms.Button cmdBaseFilings;
        private System.Windows.Forms.Button cmdNewFilings;
        private System.Windows.Forms.TextBox txtNewFilings;
		private System.Windows.Forms.Label lblNewFilings;
        private System.Windows.Forms.Button cmdLogs;
        private System.Windows.Forms.TextBox txtLogs;
        private System.Windows.Forms.Label lblLogs;
        private System.Windows.Forms.Button cmdRun;
		private System.Windows.Forms.FolderBrowserDialog folderPicker;
        private System.Windows.Forms.TextBox txtBaseFilings;
		private System.Windows.Forms.Label lblMaxWorkers;
		private System.Windows.Forms.TextBox txtMaxWorkers;
		private System.Windows.Forms.ListView txtOutput;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Label lblDifferences;
		private System.Windows.Forms.Label lblDifferencesCount;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.WebBrowser webBrowser1;
    }
}
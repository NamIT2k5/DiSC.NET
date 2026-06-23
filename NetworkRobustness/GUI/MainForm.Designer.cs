namespace NetSimulation
{
    partial class MainForm
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
            this.bntStartCal = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._baResult = new System.Windows.Forms.Label();
            this._baProgress = new System.Windows.Forms.ProgressBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nudNodeTo = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.nudNodeFrom = new System.Windows.Forms.NumericUpDown();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.nudMaxLink = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudMinLink = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.labMaximumLink = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labMinLink = new System.Windows.Forms.Label();
            this.txtReportFile = new System.Windows.Forms.TextBox();
            this.nudNetNum = new System.Windows.Forms.NumericUpDown();
            this.labnNet = new System.Windows.Forms.Label();
            this.bntSave = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this._baCentrality = new System.Windows.Forms.Label();
            this._baAverageNode = new System.Windows.Forms.Label();
            this._baAverageInteraction = new System.Windows.Forms.Label();
            this.bntTest = new System.Windows.Forms.Button();
            this.bntbrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.lbgraphs = new System.Windows.Forms.ListBox();
            this.txtmsgboard = new System.Windows.Forms.TextBox();
            this.pbMain = new System.Windows.Forms.ProgressBar();
            this.cbCentrality = new System.Windows.Forms.CheckBox();
            this.cbmodularty = new System.Windows.Forms.CheckBox();
            this.cbrobustness = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bntCalculate = new System.Windows.Forms.Button();
            this.labprogress = new System.Windows.Forms.Label();
            this.bntrefresh = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodeFrom)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLink)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinLink)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNetNum)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bntStartCal
            // 
            this.bntStartCal.Location = new System.Drawing.Point(339, 196);
            this.bntStartCal.Name = "bntStartCal";
            this.bntStartCal.Size = new System.Drawing.Size(101, 23);
            this.bntStartCal.TabIndex = 11;
            this.bntStartCal.Text = "Start";
            this.bntStartCal.UseVisualStyleBackColor = true;
            this.bntStartCal.Click += new System.EventHandler(this._baNetworkBtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(68, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "From:";
            // 
            // _baResult
            // 
            this._baResult.AutoSize = true;
            this._baResult.Location = new System.Drawing.Point(145, 29);
            this._baResult.Name = "_baResult";
            this._baResult.Size = new System.Drawing.Size(0, 13);
            this._baResult.TabIndex = 3;
            // 
            // _baProgress
            // 
            this._baProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._baProgress.Location = new System.Drawing.Point(3, 249);
            this._baProgress.Name = "_baProgress";
            this._baProgress.Size = new System.Drawing.Size(463, 23);
            this._baProgress.TabIndex = 10;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox2);
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Controls.Add(this.txtReportFile);
            this.groupBox3.Controls.Add(this.nudNetNum);
            this.groupBox3.Controls.Add(this.labnNet);
            this.groupBox3.Controls.Add(this.bntSave);
            this.groupBox3.Controls.Add(this.bntStartCal);
            this.groupBox3.Controls.Add(this._baResult);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this._baProgress);
            this.groupBox3.Controls.Add(this._baCentrality);
            this.groupBox3.Controls.Add(this._baAverageNode);
            this.groupBox3.Controls.Add(this._baAverageInteraction);
            this.groupBox3.Location = new System.Drawing.Point(12, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(469, 275);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Generating random networks data";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nudNodeTo);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.nudNodeFrom);
            this.groupBox2.Location = new System.Drawing.Point(21, 50);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(189, 82);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "2. Node range";
            // 
            // nudNodeTo
            // 
            this.nudNodeTo.Location = new System.Drawing.Point(109, 52);
            this.nudNodeTo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNodeTo.Name = "nudNodeTo";
            this.nudNodeTo.Size = new System.Drawing.Size(71, 20);
            this.nudNodeTo.TabIndex = 12;
            this.nudNodeTo.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudNodeTo.ValueChanged += new System.EventHandler(this.nudNode_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(76, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(23, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "To:";
            // 
            // nudNodeFrom
            // 
            this.nudNodeFrom.Location = new System.Drawing.Point(109, 18);
            this.nudNodeFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNodeFrom.Name = "nudNodeFrom";
            this.nudNodeFrom.Size = new System.Drawing.Size(71, 20);
            this.nudNodeFrom.TabIndex = 12;
            this.nudNodeFrom.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudNodeFrom.ValueChanged += new System.EventHandler(this.nudNode_ValueChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.nudMaxLink);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.nudMinLink);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.labMaximumLink);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.labMinLink);
            this.groupBox5.Location = new System.Drawing.Point(234, 50);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(225, 83);
            this.groupBox5.TabIndex = 17;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "3. Link range";
            // 
            // nudMaxLink
            // 
            this.nudMaxLink.Location = new System.Drawing.Point(46, 57);
            this.nudMaxLink.Name = "nudMaxLink";
            this.nudMaxLink.Size = new System.Drawing.Size(52, 20);
            this.nudMaxLink.TabIndex = 12;
            this.nudMaxLink.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMaxLink.ValueChanged += new System.EventHandler(this.nudNode_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "From";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "To";
            // 
            // nudMinLink
            // 
            this.nudMinLink.Location = new System.Drawing.Point(46, 22);
            this.nudMinLink.Name = "nudMinLink";
            this.nudMinLink.Size = new System.Drawing.Size(52, 20);
            this.nudMinLink.TabIndex = 12;
            this.nudMinLink.ValueChanged += new System.EventHandler(this.nudNode_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(160, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "**";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labMaximumLink
            // 
            this.labMaximumLink.Location = new System.Drawing.Point(175, 50);
            this.labMaximumLink.Name = "labMaximumLink";
            this.labMaximumLink.Size = new System.Drawing.Size(70, 13);
            this.labMaximumLink.TabIndex = 0;
            this.labMaximumLink.Text = "nMax";
            this.labMaximumLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(123, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "In the range of:";
            // 
            // labMinLink
            // 
            this.labMinLink.AutoSize = true;
            this.labMinLink.Location = new System.Drawing.Point(129, 50);
            this.labMinLink.Name = "labMinLink";
            this.labMinLink.Size = new System.Drawing.Size(30, 13);
            this.labMinLink.TabIndex = 0;
            this.labMinLink.Text = "nMin";
            this.labMinLink.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtReportFile
            // 
            this.txtReportFile.Location = new System.Drawing.Point(341, 225);
            this.txtReportFile.Name = "txtReportFile";
            this.txtReportFile.Size = new System.Drawing.Size(122, 20);
            this.txtReportFile.TabIndex = 13;
            this.txtReportFile.Text = "ScaleFree.xlsx";
            // 
            // nudNetNum
            // 
            this.nudNetNum.Location = new System.Drawing.Point(132, 22);
            this.nudNetNum.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudNetNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNetNum.Name = "nudNetNum";
            this.nudNetNum.Size = new System.Drawing.Size(70, 20);
            this.nudNetNum.TabIndex = 12;
            this.nudNetNum.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudNetNum.ValueChanged += new System.EventHandler(this.nudNode_ValueChanged);
            // 
            // labnNet
            // 
            this.labnNet.AutoSize = true;
            this.labnNet.Location = new System.Drawing.Point(190, 233);
            this.labnNet.Name = "labnNet";
            this.labnNet.Size = new System.Drawing.Size(64, 13);
            this.labnNet.TabIndex = 0;
            this.labnNet.Text = "Num of nets";
            // 
            // bntSave
            // 
            this.bntSave.Location = new System.Drawing.Point(6, 195);
            this.bntSave.Name = "bntSave";
            this.bntSave.Size = new System.Drawing.Size(104, 23);
            this.bntSave.TabIndex = 11;
            this.bntSave.Text = "Create a network..";
            this.bntSave.UseVisualStyleBackColor = true;
            this.bntSave.Click += new System.EventHandler(this.bntSave_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "1. The number of nets";
            // 
            // _baCentrality
            // 
            this._baCentrality.AutoSize = true;
            this._baCentrality.Location = new System.Drawing.Point(336, 29);
            this._baCentrality.Name = "_baCentrality";
            this._baCentrality.Size = new System.Drawing.Size(0, 13);
            this._baCentrality.TabIndex = 5;
            // 
            // _baAverageNode
            // 
            this._baAverageNode.AutoSize = true;
            this._baAverageNode.Location = new System.Drawing.Point(172, 61);
            this._baAverageNode.Name = "_baAverageNode";
            this._baAverageNode.Size = new System.Drawing.Size(0, 13);
            this._baAverageNode.TabIndex = 7;
            // 
            // _baAverageInteraction
            // 
            this._baAverageInteraction.AutoSize = true;
            this._baAverageInteraction.Location = new System.Drawing.Point(403, 61);
            this._baAverageInteraction.Name = "_baAverageInteraction";
            this._baAverageInteraction.Size = new System.Drawing.Size(0, 13);
            this._baAverageInteraction.TabIndex = 9;
            // 
            // bntTest
            // 
            this.bntTest.Location = new System.Drawing.Point(860, 183);
            this.bntTest.Name = "bntTest";
            this.bntTest.Size = new System.Drawing.Size(90, 23);
            this.bntTest.TabIndex = 11;
            this.bntTest.Text = "Test";
            this.bntTest.UseVisualStyleBackColor = true;
            // 
            // bntbrowse
            // 
            this.bntbrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bntbrowse.Location = new System.Drawing.Point(863, 154);
            this.bntbrowse.Name = "bntbrowse";
            this.bntbrowse.Size = new System.Drawing.Size(90, 23);
            this.bntbrowse.TabIndex = 11;
            this.bntbrowse.Text = "Load from file";
            this.bntbrowse.UseVisualStyleBackColor = true;
            this.bntbrowse.Click += new System.EventHandler(this.bntbrowse_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileName.Location = new System.Drawing.Point(487, 156);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(370, 20);
            this.txtFileName.TabIndex = 12;
            // 
            // lbgraphs
            // 
            this.lbgraphs.FormattingEnabled = true;
            this.lbgraphs.Location = new System.Drawing.Point(668, 3);
            this.lbgraphs.Name = "lbgraphs";
            this.lbgraphs.Size = new System.Drawing.Size(285, 134);
            this.lbgraphs.TabIndex = 13;
            // 
            // txtmsgboard
            // 
            this.txtmsgboard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtmsgboard.Location = new System.Drawing.Point(0, 284);
            this.txtmsgboard.MaxLength = 999999999;
            this.txtmsgboard.Multiline = true;
            this.txtmsgboard.Name = "txtmsgboard";
            this.txtmsgboard.ReadOnly = true;
            this.txtmsgboard.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtmsgboard.Size = new System.Drawing.Size(965, 116);
            this.txtmsgboard.TabIndex = 14;
            // 
            // pbMain
            // 
            this.pbMain.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbMain.Location = new System.Drawing.Point(0, 413);
            this.pbMain.Name = "pbMain";
            this.pbMain.Size = new System.Drawing.Size(965, 23);
            this.pbMain.Step = 0;
            this.pbMain.TabIndex = 10;
            // 
            // cbCentrality
            // 
            this.cbCentrality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCentrality.AutoSize = true;
            this.cbCentrality.Checked = true;
            this.cbCentrality.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCentrality.Location = new System.Drawing.Point(18, 25);
            this.cbCentrality.Name = "cbCentrality";
            this.cbCentrality.Size = new System.Drawing.Size(69, 17);
            this.cbCentrality.TabIndex = 15;
            this.cbCentrality.Text = "Centrality";
            this.cbCentrality.UseVisualStyleBackColor = true;
            // 
            // cbmodularty
            // 
            this.cbmodularty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbmodularty.AutoSize = true;
            this.cbmodularty.Checked = true;
            this.cbmodularty.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbmodularty.Location = new System.Drawing.Point(18, 50);
            this.cbmodularty.Name = "cbmodularty";
            this.cbmodularty.Size = new System.Drawing.Size(74, 17);
            this.cbmodularty.TabIndex = 15;
            this.cbmodularty.Text = "Modularity";
            this.cbmodularty.UseVisualStyleBackColor = true;
            // 
            // cbrobustness
            // 
            this.cbrobustness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbrobustness.AutoSize = true;
            this.cbrobustness.Checked = true;
            this.cbrobustness.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbrobustness.Location = new System.Drawing.Point(17, 78);
            this.cbrobustness.Name = "cbrobustness";
            this.cbrobustness.Size = new System.Drawing.Size(82, 17);
            this.cbrobustness.TabIndex = 15;
            this.cbrobustness.Text = "Robustness";
            this.cbrobustness.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbrobustness);
            this.groupBox1.Controls.Add(this.cbCentrality);
            this.groupBox1.Controls.Add(this.cbmodularty);
            this.groupBox1.Location = new System.Drawing.Point(487, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 102);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Calculate network properties";
            // 
            // bntCalculate
            // 
            this.bntCalculate.Location = new System.Drawing.Point(484, 3);
            this.bntCalculate.Name = "bntCalculate";
            this.bntCalculate.Size = new System.Drawing.Size(90, 23);
            this.bntCalculate.TabIndex = 11;
            this.bntCalculate.Text = "&Calculate";
            this.bntCalculate.UseVisualStyleBackColor = true;
            this.bntCalculate.Click += new System.EventHandler(this.bntCalculate_Click);
            // 
            // labprogress
            // 
            this.labprogress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labprogress.Location = new System.Drawing.Point(0, 400);
            this.labprogress.Name = "labprogress";
            this.labprogress.Size = new System.Drawing.Size(965, 13);
            this.labprogress.TabIndex = 7;
            // 
            // bntrefresh
            // 
            this.bntrefresh.Location = new System.Drawing.Point(645, 3);
            this.bntrefresh.Name = "bntrefresh";
            this.bntrefresh.Size = new System.Drawing.Size(20, 23);
            this.bntrefresh.TabIndex = 11;
            this.bntrefresh.Text = "&R";
            this.bntrefresh.UseVisualStyleBackColor = true;
            this.bntrefresh.Click += new System.EventHandler(this.bntrefresh_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 436);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtmsgboard);
            this.Controls.Add(this.lbgraphs);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.labprogress);
            this.Controls.Add(this.bntbrowse);
            this.Controls.Add(this.bntrefresh);
            this.Controls.Add(this.bntCalculate);
            this.Controls.Add(this.bntTest);
            this.Controls.Add(this.pbMain);
            this.Controls.Add(this.groupBox3);
            this.Name = "MainForm";
            this.Text = "Network Robustness";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodeFrom)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLink)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinLink)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNetNum)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bntStartCal;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label _baResult;
        private System.Windows.Forms.ProgressBar _baProgress;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label _baCentrality;
        private System.Windows.Forms.Label _baAverageNode;
        private System.Windows.Forms.Label _baAverageInteraction;
        private System.Windows.Forms.Button bntTest;
        private System.Windows.Forms.Button bntbrowse;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.ListBox lbgraphs;
        public System.Windows.Forms.ProgressBar pbMain;
        public System.Windows.Forms.TextBox txtmsgboard;
        private System.Windows.Forms.CheckBox cbCentrality;
        private System.Windows.Forms.CheckBox cbmodularty;
        private System.Windows.Forms.CheckBox cbrobustness;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bntCalculate;
        public System.Windows.Forms.Label labprogress;
        private System.Windows.Forms.Button bntrefresh;
        private System.Windows.Forms.NumericUpDown nudMaxLink;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudMinLink;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudNodeFrom;
        private System.Windows.Forms.TextBox txtReportFile;
        private System.Windows.Forms.NumericUpDown nudNetNum;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labMaximumLink;
        private System.Windows.Forms.Button bntSave;
        private System.Windows.Forms.Label labnNet;
        private System.Windows.Forms.Label labMinLink;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nudNodeTo;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
    }
}


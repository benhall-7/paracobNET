namespace Param2Form
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openParamFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxLeft = new System.Windows.Forms.GroupBox();
            this.param_TreeView = new System.Windows.Forms.TreeView();
            this.groupBoxRight = new System.Windows.Forms.GroupBox();
            this.label_DataGridView = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.param_DataGridView = new System.Windows.Forms.DataGridView();
            this.openParamDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.groupBoxLeft.SuspendLayout();
            this.groupBoxRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.label_DataGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.param_DataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(834, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openParamFileToolStripMenuItem,
            this.openParamDatabaseToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openParamFileToolStripMenuItem
            // 
            this.openParamFileToolStripMenuItem.Name = "openParamFileToolStripMenuItem";
            this.openParamFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openParamFileToolStripMenuItem.Text = "Open Param File";
            this.openParamFileToolStripMenuItem.Click += new System.EventHandler(this.openParamFileToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 389);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(834, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(149, 17);
            this.toolStripStatusLabel.Text = "Default text pls do not read";
            // 
            // groupBoxLeft
            // 
            this.groupBoxLeft.Controls.Add(this.param_TreeView);
            this.groupBoxLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxLeft.Location = new System.Drawing.Point(0, 24);
            this.groupBoxLeft.Name = "groupBoxLeft";
            this.groupBoxLeft.Size = new System.Drawing.Size(200, 365);
            this.groupBoxLeft.TabIndex = 3;
            this.groupBoxLeft.TabStop = false;
            this.groupBoxLeft.Text = "Param Tree";
            // 
            // param_TreeView
            // 
            this.param_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param_TreeView.Location = new System.Drawing.Point(3, 16);
            this.param_TreeView.Name = "param_TreeView";
            this.param_TreeView.Size = new System.Drawing.Size(194, 346);
            this.param_TreeView.TabIndex = 0;
            this.param_TreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.param_TreeView_AfterSelect);
            // 
            // groupBoxRight
            // 
            this.groupBoxRight.Controls.Add(this.label_DataGridView);
            this.groupBoxRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBoxRight.Location = new System.Drawing.Point(634, 24);
            this.groupBoxRight.Name = "groupBoxRight";
            this.groupBoxRight.Size = new System.Drawing.Size(200, 365);
            this.groupBoxRight.TabIndex = 5;
            this.groupBoxRight.TabStop = false;
            this.groupBoxRight.Text = "Labels";
            // 
            // label_DataGridView
            // 
            this.label_DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.label_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_DataGridView.Location = new System.Drawing.Point(3, 16);
            this.label_DataGridView.Name = "label_DataGridView";
            this.label_DataGridView.Size = new System.Drawing.Size(194, 346);
            this.label_DataGridView.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.param_DataGridView);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(200, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(434, 365);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Param Data";
            // 
            // param_DataGridView
            // 
            this.param_DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.param_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param_DataGridView.Location = new System.Drawing.Point(3, 16);
            this.param_DataGridView.Name = "param_DataGridView";
            this.param_DataGridView.Size = new System.Drawing.Size(428, 346);
            this.param_DataGridView.TabIndex = 0;
            // 
            // openParamDatabaseToolStripMenuItem
            // 
            this.openParamDatabaseToolStripMenuItem.Name = "openParamDatabaseToolStripMenuItem";
            this.openParamDatabaseToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.openParamDatabaseToolStripMenuItem.Text = "Open Param Database";
            this.openParamDatabaseToolStripMenuItem.Click += new System.EventHandler(this.openParamDatabaseToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 411);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxRight);
            this.Controls.Add(this.groupBoxLeft);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(500, 100);
            this.Name = "MainForm";
            this.Text = "Param2Form";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBoxLeft.ResumeLayout(false);
            this.groupBoxRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.label_DataGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.param_DataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openParamFileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.GroupBox groupBoxLeft;
        private System.Windows.Forms.GroupBox groupBoxRight;
        private System.Windows.Forms.TreeView param_TreeView;
        private System.Windows.Forms.DataGridView label_DataGridView;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView param_DataGridView;
        private System.Windows.Forms.ToolStripMenuItem openParamDatabaseToolStripMenuItem;
    }
}


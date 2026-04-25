namespace NoScript
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnRun = new System.Windows.Forms.Button();
            this.rtbCode = new System.Windows.Forms.RichTextBox();
            this.txtOutput = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlTop.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnRun);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 28);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 50);
            this.pnlTop.TabIndex = 0;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(12, 12);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(120, 30);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run NeoScript";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // rtbCode
            // 
            this.rtbCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCode.Location = new System.Drawing.Point(0, 78);
            this.rtbCode.Name = "rtbCode";
            this.rtbCode.Size = new System.Drawing.Size(800, 405);
            this.rtbCode.TabIndex = 1;
            this.rtbCode.Text = "";
            // 
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtOutput.ForeColor = System.Drawing.Color.Red;
            this.txtOutput.Location = new System.Drawing.Point(0, 325);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(800, 158);
            this.txtOutput.TabIndex = 2;
            this.txtOutput.Text = "";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.saveScriptToolStripMenuItem,
            this.openScriptToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 28);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveScriptToolStripMenuItem
            // 
            this.saveScriptToolStripMenuItem.Name = "saveScriptToolStripMenuItem";
            this.saveScriptToolStripMenuItem.Size = new System.Drawing.Size(96, 24);
            this.saveScriptToolStripMenuItem.Text = "Save Script";
            this.saveScriptToolStripMenuItem.Click += new System.EventHandler(this.saveScriptToolStripMenuItem_Click);
            // 
            // openScriptToolStripMenuItem
            // 
            this.openScriptToolStripMenuItem.Name = "openScriptToolStripMenuItem";
            this.openScriptToolStripMenuItem.Size = new System.Drawing.Size(101, 24);
            this.openScriptToolStripMenuItem.Text = "Open Script";
            this.openScriptToolStripMenuItem.Click += new System.EventHandler(this.openScriptToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 483);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.rtbCode);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "NeoScript IDE";
            this.pnlTop.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.RichTextBox rtbCode;
        private System.Windows.Forms.RichTextBox txtOutput;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openScriptToolStripMenuItem;
    }
}
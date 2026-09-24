namespace App
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button9 = new System.Windows.Forms.Button();
            this.snippetPanel = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToTrayOnCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToTrayAfterCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(283, 28);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(73, 23);
            this.button9.TabIndex = 17;
            this.button9.Text = "Clear All";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);

            // 
            // snippetPanel
            // 
            this.snippetPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.snippetPanel.AutoScroll = true;
            this.snippetPanel.Location = new System.Drawing.Point(12, 54);
            this.snippetPanel.Name = "snippetPanel";
            this.snippetPanel.Size = new System.Drawing.Size(346, 68);
            this.snippetPanel.TabIndex = 20;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 28);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(57, 23);
            this.btnAdd.TabIndex = 21;
            this.btnAdd.Text = "+ Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(75, 28);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(65, 23);
            this.btnRemove.TabIndex = 22;
            this.btnRemove.Text = "- Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(370, 24);
            this.menuStrip1.TabIndex = 24;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem,
            this.minimizeToTrayOnCloseToolStripMenuItem,
            this.minimizeToTrayAfterCopyToolStripMenuItem,
            this.opacityToolStripMenuItem,
            this.darkModeToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
            this.alwaysOnTopToolStripMenuItem.Checked = true;
            this.alwaysOnTopToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always On Top";
            this.alwaysOnTopToolStripMenuItem.CheckedChanged += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_CheckedChanged);
            // 
            // minimizeToTrayOnCloseToolStripMenuItem
            // 
            this.minimizeToTrayOnCloseToolStripMenuItem.CheckOnClick = true;
            this.minimizeToTrayOnCloseToolStripMenuItem.Checked = true;
            this.minimizeToTrayOnCloseToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minimizeToTrayOnCloseToolStripMenuItem.Name = "minimizeToTrayOnCloseToolStripMenuItem";
            this.minimizeToTrayOnCloseToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.minimizeToTrayOnCloseToolStripMenuItem.Text = "Minimize to Tray on Close";
            this.minimizeToTrayOnCloseToolStripMenuItem.CheckedChanged += new System.EventHandler(this.settings_CheckedChanged);
            // 
            // minimizeToTrayAfterCopyToolStripMenuItem
            // 
            this.minimizeToTrayAfterCopyToolStripMenuItem.CheckOnClick = true;
            this.minimizeToTrayAfterCopyToolStripMenuItem.Name = "minimizeToTrayAfterCopyToolStripMenuItem";
            this.minimizeToTrayAfterCopyToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.minimizeToTrayAfterCopyToolStripMenuItem.Text = "Minimize to Tray after CTRL + # Copy";
            this.minimizeToTrayAfterCopyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.settings_CheckedChanged);
            // 
            // opacityToolStripMenuItem
            // 
            this.opacityToolStripMenuItem.Name = "opacityToolStripMenuItem";
            this.opacityToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.opacityToolStripMenuItem.Text = "Opacity...";
            this.opacityToolStripMenuItem.Click += new System.EventHandler(this.opacityToolStripMenuItem_Click);
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.CheckOnClick = true;
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.darkModeToolStripMenuItem.Text = "Dark Mode";
            this.darkModeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.darkModeToolStripMenuItem_CheckedChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 132);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.snippetPanel);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "OctoCopy";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Panel snippetPanel;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToTrayOnCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToTrayAfterCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem opacityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    }
}

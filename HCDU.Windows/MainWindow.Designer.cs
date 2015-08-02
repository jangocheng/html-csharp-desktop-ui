namespace HCDU.Windows
{
    partial class MainWindow
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
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.homeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.googlePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.devToolsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resourcesListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowserPanel = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1132, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.homeToolStripMenuItem,
            this.googlePageToolStripMenuItem,
            this.devToolsToolStripMenuItem1,
            this.refreshToolStripMenuItem1,
            this.resourcesListToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.actionsToolStripMenuItem.Text = "Tools";
            // 
            // homeToolStripMenuItem
            // 
            this.homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            this.homeToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.homeToolStripMenuItem.Text = "Home Page";
            this.homeToolStripMenuItem.Click += new System.EventHandler(this.MenuActionShowHomePage);
            // 
            // googlePageToolStripMenuItem
            // 
            this.googlePageToolStripMenuItem.Name = "googlePageToolStripMenuItem";
            this.googlePageToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.googlePageToolStripMenuItem.Text = "Angular Material Website";
            this.googlePageToolStripMenuItem.Click += new System.EventHandler(this.MenuActionShowAngularMaterial);
            // 
            // devToolsToolStripMenuItem1
            // 
            this.devToolsToolStripMenuItem1.Name = "devToolsToolStripMenuItem1";
            this.devToolsToolStripMenuItem1.Size = new System.Drawing.Size(207, 22);
            this.devToolsToolStripMenuItem1.Text = "Developer tools";
            this.devToolsToolStripMenuItem1.Click += new System.EventHandler(this.MenuActionOpenDevTools);
            // 
            // refreshToolStripMenuItem1
            // 
            this.refreshToolStripMenuItem1.Name = "refreshToolStripMenuItem1";
            this.refreshToolStripMenuItem1.Size = new System.Drawing.Size(207, 22);
            this.refreshToolStripMenuItem1.Text = "Reload";
            this.refreshToolStripMenuItem1.Click += new System.EventHandler(this.MenuActionReload);
            // 
            // resourcesListToolStripMenuItem
            // 
            this.resourcesListToolStripMenuItem.Name = "resourcesListToolStripMenuItem";
            this.resourcesListToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.resourcesListToolStripMenuItem.Text = "Resources List";
            this.resourcesListToolStripMenuItem.Click += new System.EventHandler(this.MenuActionShowResources);
            // 
            // webBrowserPanel
            // 
            this.webBrowserPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowserPanel.Location = new System.Drawing.Point(0, 27);
            this.webBrowserPanel.Name = "webBrowserPanel";
            this.webBrowserPanel.Size = new System.Drawing.Size(1132, 602);
            this.webBrowserPanel.TabIndex = 1;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1132, 629);
            this.Controls.Add(this.webBrowserPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "HTML/JS/C# Desktop App";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel webBrowserPanel;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem homeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resourcesListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem googlePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem devToolsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem1;
    }
}


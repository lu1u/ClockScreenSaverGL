namespace Extraire_Donnees_Meteo
{
    partial class Form2
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.extraireMétéoMaintenantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem,
            this.toolStripSeparator1,
            this.extraireMétéoMaintenantToolStripMenuItem,
            this.toolStripSeparator2,
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem,
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem,
            this.toolStripSeparator3,
            this.quitterToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(417, 154);
            // 
            // extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem
            // 
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem.Enabled = false;
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem.Name = "extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem";
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem.Text = "Extraction de données météo et actualités pour ClockScreensaver";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(413, 6);
            // 
            // extraireMétéoMaintenantToolStripMenuItem
            // 
            this.extraireMétéoMaintenantToolStripMenuItem.Name = "extraireMétéoMaintenantToolStripMenuItem";
            this.extraireMétéoMaintenantToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.extraireMétéoMaintenantToolStripMenuItem.Text = "Extraire données maintenant";
            this.extraireMétéoMaintenantToolStripMenuItem.Click += new System.EventHandler(this.extraireMétéoMaintenantToolStripMenuItem_Click);
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            this.quitterToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.quitterToolStripMenuItem.Text = "Quitter";
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quitterToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Clockscreensaver extraction données météo";
            this.notifyIcon.Visible = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ouvrirRépertoireDeConfigurationToolStripMenuItem
            // 
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem.Name = "ouvrirRépertoireDeConfigurationToolStripMenuItem";
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem.Text = "Ouvrir répertoire de configuration";
            this.ouvrirRépertoireDeConfigurationToolStripMenuItem.Click += new System.EventHandler(this.ouvrirRépertoireDeConfigurationToolStripMenuItem_Click);
            // 
            // ouvrirRépertoireDesActualitésChargéesToolStripMenuItem
            // 
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem.Name = "ouvrirRépertoireDesActualitésChargéesToolStripMenuItem";
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem.Text = "Ouvrir répertoire des actualités chargées";
            this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem.Click += new System.EventHandler(this.ouvrirRépertoireDesActualitésChargéesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(413, 6);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(413, 6);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 168);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form2";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form2_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem extractionDeDonnéesMétéoPourClockScreensaverToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem extraireMétéoMaintenantToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitterToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ouvrirRépertoireDeConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ouvrirRépertoireDesActualitésChargéesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}